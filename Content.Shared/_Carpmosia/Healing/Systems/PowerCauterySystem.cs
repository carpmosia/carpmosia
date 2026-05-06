using Content.Shared.Administration.Logs;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Medical;
using Content.Shared.Popups;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Content.Shared._Carpmosia.Medical.Components;
using Content.Shared._Carpmosia.Medical.Events;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._Carpmosia.Medical.Systems;

/// <summary>
/// Modifies bleeding stacks per doafter, at the cost of power.
/// </summary>
/// <remarks>
/// Separate from <see cref="HealingSystem"> because this wants ONLY bleeding and not damage,
/// and i'm not about to bolt on power consumption logic to topicals, which are work with stacks.
/// </remarks>
public sealed class PowerCauterySystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedBatterySystem _battery = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstreamSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedInteractionSystem _interactionSystem = default!;
    [Dependency] private readonly MobThresholdSystem _mobThresholdSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PowerCauteryComponent, UseInHandEvent>(OnHealingUse);
        SubscribeLocalEvent<PowerCauteryComponent, AfterInteractEvent>(OnHealingAfterInteract);
        SubscribeLocalEvent<DamageableComponent, PowerCauteryDoAfterEvent>(OnDoAfter);
    }

    private void OnDoAfter(Entity<DamageableComponent> target, ref PowerCauteryDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        if (!TryComp(args.Used, out PowerCauteryComponent? cautery))
            return;

        // check if our container list matches our target
        if (cautery.DamageContainers is not null &&
            target.Comp.DamageContainerID is not null &&
            !cautery.DamageContainers.Contains(target.Comp.DamageContainerID.Value))
        {
            return;
        }

        TryComp<BloodstreamComponent>(target, out var bloodstream);

        // Stem bleeding.
        if (cautery.BloodlossModifier != 0 && bloodstream != null)
        {
            var isBleeding = bloodstream.BleedAmount > 0;
            _bloodstreamSystem.TryModifyBleedAmount((target.Owner, bloodstream), cautery.BloodlossModifier);
            if (isBleeding != bloodstream.BleedAmount > 0)
            {
                var popup = (args.User == target.Owner)
                    ? Loc.GetString("medical-item-stop-bleeding-self")
                    : Loc.GetString("medical-item-stop-bleeding", ("target", Identity.Entity(target.Owner, EntityManager)));
                _popupSystem.PopupClient(popup, target, args.User);
            }
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

        _audio.PlayPredicted(cautery.EndSound, target.Owner, args.User);

        bool dontRepeat = false;
        if (TryComp<BatteryComponent>(args.Used.Value, out var battery))
        {
            _battery.UseCharge((args.Used.Value, battery), cautery.PowerDraw);

            if (_battery.GetCharge((args.Used.Value, battery)) < cautery.PowerDraw);
            {
                dontRepeat = true;
            }

            Log.Info($"{_battery.GetCharge((args.Used.Value, battery))} {cautery.PowerDraw} {(_battery.GetCharge((args.Used.Value, battery)) < cautery.PowerDraw)} {dontRepeat}");
        }

        args.Repeat = IsBleeding((args.Used.Value, cautery), target) && !dontRepeat;

        args.Handled = true;

        if (!args.Repeat)
        {
            _popupSystem.PopupClient(Loc.GetString("medical-item-finished-using", ("item", args.Used)), target.Owner, args.User);
            return;
        }

        // Update our self heal delay so it shortens as we heal more damage.
        if (args.User == target.Owner)
            args.Args.Delay = cautery.Delay * GetScaledHealingPenalty(target.Owner, cautery.SelfHealPenaltyMultiplier);
    }

    private bool IsBleeding(Entity<PowerCauteryComponent> cautery, Entity<DamageableComponent> target)
    {
        if (!TryComp<BloodstreamComponent>(target, out var bloodstream))
            return false;

        if (cautery.Comp.BloodlossModifier < 0 && bloodstream.BleedAmount > 0)
            return true;

        return false;
    }

    private void OnHealingUse(Entity<PowerCauteryComponent> cautery, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        if (TryCauterize(cautery, args.User, args.User))
            args.Handled = true;
    }

    private void OnHealingAfterInteract(Entity<PowerCauteryComponent> cautery, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target == null)
            return;

        if (TryCauterize(cautery, args.Target.Value, args.User))
            args.Handled = true;
    }

    private bool TryCauterize(Entity<PowerCauteryComponent> cautery, Entity<DamageableComponent?> target, EntityUid user)
    {
        if (!Resolve(target, ref target.Comp, false))
            return false;

        // check if our container list matches the target
        if (cautery.Comp.DamageContainers is not null &&
            target.Comp.DamageContainerID is not null &&
            !cautery.Comp.DamageContainers.Contains(target.Comp.DamageContainerID.Value))
        {
            return false;
        }

        if (user != target.Owner && !_interactionSystem.InRangeUnobstructed(user, target.Owner, popup: true))
            return false;

        if (!IsBleeding(cautery, target!))
        {
            _popupSystem.PopupClient(Loc.GetString("medical-item-cant-use", ("item", cautery.Owner)), cautery, user);
            return false;
        }

        _audio.PlayPredicted(cautery.Comp.BeginSound, cautery, user);

        var isNotSelf = user != target.Owner;

        if (isNotSelf)
        {
            var msg = Loc.GetString("medical-item-popup-target", ("user", Identity.Entity(user, EntityManager)), ("item", cautery.Owner));
            _popupSystem.PopupEntity(msg, target, target, PopupType.Medium);
        }

        var delay = isNotSelf
            ? cautery.Comp.Delay
            : cautery.Comp.Delay * GetScaledHealingPenalty(target, cautery.Comp.SelfHealPenaltyMultiplier);

        var doAfterEventArgs =
            new DoAfterArgs(EntityManager, user, delay, new PowerCauteryDoAfterEvent(), target, target: target, used: cautery)
            {
                NeedHand = true,
                BreakOnMove = true,
                BreakOnWeightlessMove = false,
            };

        _doAfter.TryStartDoAfter(doAfterEventArgs);
        return true;
    }

    /// <summary>
    /// Scales the self-heal penalty based on the amount of damage taken
    /// </summary>
    /// <param name="ent">Entity we're healing</param>
    /// <param name="mod">Maximum modifier we can have.</param>
    /// <returns>Modifier we multiply our healing time by</returns>
    public float GetScaledHealingPenalty(Entity<DamageableComponent?, MobThresholdsComponent?> ent, float mod)
    {
        if (!Resolve(ent, ref ent.Comp1, ref ent.Comp2, false))
            return mod;

        if (!_mobThresholdSystem.TryGetThresholdForState(ent, MobState.Critical, out var amount, ent.Comp2))
            return 1;

        var percentDamage = (float)(_damageable.GetTotalDamage(ent) / amount);
        //basically make it scale from 1 to the multiplier.

        var output = percentDamage * (mod - 1) + 1;
        return Math.Max(output, 1);
    }
}
