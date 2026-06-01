using Content.Shared.Administration.Logs;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Healing.Components;
using Content.Shared.Healing.Events;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared.Healing.Systems;

/// <summary>
/// Modifies bleeding stacks per doafter, at the cost of power.
/// </summary>
/// <remarks>
/// Separate from <see cref="HealingSystem"> because this wants ONLY bleeding and not damage,
/// and i'm not about to bolt on power consumption logic to topicals, which work with stacks.
/// </remarks>
public sealed partial class PowerCauterySystem : EntitySystem
{
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private SharedBatterySystem _battery = default!;
    [Dependency] private SharedBloodstreamSystem _bloodstreamSystem = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedInteractionSystem _interactionSystem = default!;
    [Dependency] private SharedPopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PowerCauteryComponent, UseInHandEvent>(OnCauteryUse);
        SubscribeLocalEvent<PowerCauteryComponent, AfterInteractEvent>(OnCauteryAfterInteract);
        SubscribeLocalEvent<PowerCauteryComponent, ExaminedEvent>(OnCauteryExamined);
        SubscribeLocalEvent<DamageableComponent, PowerCauteryDoAfterEvent>(OnDoAfter);
    }

    // entrypoints
    private void OnCauteryUse(Entity<PowerCauteryComponent> cautery, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        if (TryCauterize(cautery, args.User, args.User))
            args.Handled = true;
    }

    private void OnCauteryAfterInteract(Entity<PowerCauteryComponent> cautery, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target == null)
            return;

        if (TryCauterize(cautery, args.Target.Value, args.User))
            args.Handled = true;
    }

    private void OnCauteryExamined(Entity<PowerCauteryComponent> ent, ref ExaminedEvent args)
    {
        if (!TryComp<BatteryComponent>(ent, out var battery))
            return;

        var charges = (int)(_battery.GetCharge((ent, battery)) / ent.Comp.PowerDraw);
        args.PushMarkup(Loc.GetString("power-cautery-charge-examine", ("charges", charges)));
    }

    // checks
    private bool IsBleeding(Entity<PowerCauteryComponent> cautery, Entity<DamageableComponent> target)
    {
        if (!TryComp<BloodstreamComponent>(target, out var bloodstream))
            return false;

        if (bloodstream.BleedAmount > 0)
            return true;

        return false;
    }

    private bool GetAndDrawPower(Entity<BatteryComponent?> ent, float draw)
    {
        bool couldDraw = (_battery.GetCharge(ent) >= draw);

        if (couldDraw)
            _battery.UseCharge(ent, draw);

        return couldDraw;
    }

    private bool MatchDamageContainers(List<ProtoId<DamageContainerPrototype>>? containers, DamageableComponent damagable)
    {
        if (containers is null)
            return true;

        // DamageContainerID is nullable so we need to do this hogwash with a null check
        if (damagable.DamageContainerID is not null && containers.Contains(damagable.DamageContainerID.Value))
            return true;

        return false;
    }

    // cauterization
    private bool TryCauterize(Entity<PowerCauteryComponent> cautery, Entity<DamageableComponent?> target, EntityUid user)
    {
        if (!Resolve(target, ref target.Comp, false))
            return false;

        // check if our container list matches our target
        if (!MatchDamageContainers(cautery.Comp.DamageContainers, target.Comp))
            return false;

        // range check
        if (user != target.Owner && !_interactionSystem.InRangeUnobstructed(user, target.Owner, popup: true))
            return false;

        // is the target even bleeding
        if (!IsBleeding(cautery, target!))
        {
            _popupSystem.PopupClient(Loc.GetString("medical-item-cant-use", ("item", cautery.Owner)), cautery, user);
            return false;
        }

        // start sound
        _audio.PlayPredicted(cautery.Comp.BeginSound, cautery, user);

        bool isSelf = user == target.Owner;

        // let the target know we're helping
        if (!isSelf)
        {
            var msg = Loc.GetString("medical-item-popup-target", ("user", Identity.Entity(user, EntityManager)), ("item", cautery.Owner));
            _popupSystem.PopupEntity(msg, target, target, PopupType.Medium);
        }

        // set a delay and penalize self-healing
        var delay = cautery.Comp.Delay;
        if (isSelf)
            delay *= cautery.Comp.SelfHealPenaltyMultiplier;

        // set up a doafter
        var doAfterEventArgs =
            new DoAfterArgs(EntityManager, user, delay, new PowerCauteryDoAfterEvent(), target, target: target, used: cautery)
            {
                NeedHand = true,
                BreakOnMove = true,
                BreakOnWeightlessMove = false,
            };

        // start the doafter
        _doAfter.TryStartDoAfter(doAfterEventArgs);
        return true;
    }

    private void OnDoAfter(Entity<DamageableComponent> target, ref PowerCauteryDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        if (!TryComp(args.Used, out PowerCauteryComponent? cautery))
            return;

        if (!TryComp(args.Used, out BatteryComponent? battery))
            return;

        // get target bloodstream
        TryComp<BloodstreamComponent>(target, out var bloodstream);

        // try to draw from our battery, fail if we can't
        bool powered = GetAndDrawPower((args.Used.Value, battery), cautery.PowerDraw);
        if (!powered)
        {   // starting the doafter w/o power is fine if we can fail it here
            _popupSystem.PopupClient(Loc.GetString("power-cautery-no-power"), args.User, args.User);
            return;
        }

        // Stem bleeding.
        if (cautery.BloodlossModifier != 0 && bloodstream != null)
        {
            var isBleeding = bloodstream.BleedAmount > 0;
            _bloodstreamSystem.TryModifyBleedAmount((target.Owner, bloodstream), cautery.BloodlossModifier);
        }

        // Log the cauterizing.
        if (target.Owner != args.User)
        {
            _adminLogger.Add(LogType.Healed,
                $"{ToPrettyString(args.User):user} cauterized {ToPrettyString(target.Owner):target} for {cautery.BloodlossModifier} points");
        }
        else
        {
            _adminLogger.Add(LogType.Healed,
                $"{ToPrettyString(args.User):user} cauterized themselves for {cautery.BloodlossModifier} points");
        }

        // end sound
        _audio.PlayPredicted(cautery.EndSound, target.Owner, args.User);

        // repeat if our target is still bleeding
        args.Repeat = IsBleeding((args.Used.Value, cautery), target);

        args.Handled = true;

        // say we're finished if we're not repeating
        if (!args.Repeat)
        {
            _popupSystem.PopupClient(Loc.GetString("medical-item-finished-using", ("item", args.Used)), target.Owner, args.User);
            return;
        }
    }
}
