using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Random.Helpers;
using Content.Shared.StatusEffectNew;
using Content.Shared.Verbs;
using Robust.Shared.Timing;

namespace Content.Shared._Offbrand.Wounds;

public sealed partial class CprSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private StatusEffectsSystem _statusEffects = default!;
    [Dependency] private WoundableSystem _woundable = default!;
    [Dependency] private MobStateSystem _mobState = default!;

    [Dependency] private EntityQuery<WoundableComponent> _woundableQuery;
    [Dependency] private EntityQuery<HeartrateAlertsComponent> _heartrateQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CprTargetComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
        SubscribeLocalEvent<CprTargetComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<CprTargetComponent, CprDoAfterEvent>(OnCprDoAfter);
    }

    private void TryStartCpr(Entity<CprTargetComponent> ent, EntityUid user)
    {
        _popup.PopupEntity(
            Loc.GetString(ent.Comp.UserPopup, ("target", Identity.Entity(ent, EntityManager))),
            Loc.GetString(ent.Comp.OtherPopup, ("user", Identity.Entity(user, EntityManager)), ("target", Identity.Entity(ent, EntityManager))),
            ent,
            user
        );

        var args =
            new DoAfterArgs(EntityManager, user, ent.Comp.DoAfterDuration, new CprDoAfterEvent(), ent, target: ent, used: ent)
            {
                NeedHand = true,
                BreakOnDamage = true,
                BreakOnMove = true,
                BreakOnWeightlessMove = true,
            };

        _doAfter.TryStartDoAfter(args);
    }

    private void OnCprDoAfter(Entity<CprTargetComponent> ent, ref CprDoAfterEvent args)
    {
        _statusEffects.TryAddStatusEffectDuration(ent, ent.Comp.Effect, ent.Comp.EffectDuration);

        if (SharedRandomExtensions.PredictedProb(_timing, ent.Comp.WoundProbability, GetNetEntity(ent))
            && _woundableQuery.TryComp(ent, out var woundable)
            && _woundable.TryWound((ent, woundable), ent.Comp.Wound, unique: true, refresh: true))
        {
            _popup.PopupEntity(
                Loc.GetString(ent.Comp.WoundPopup, ("target", Identity.Entity(ent, EntityManager))),
                ent.Owner,
                args.User,
                PopupType.MediumCaution
            );
        }

        args.Repeat = _heartrateQuery.TryComp(ent, out var heartrate) && !heartrate.Beating;
    }

    private void OnGetVerbs(Entity<CprTargetComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || ent.Owner == args.User)
            return;

        if (!_heartrateQuery.TryComp(ent, out var heartrate) || heartrate.Beating)
            return;

        var @event = args;
        args.Verbs.Add(new AlternativeVerb()
        {
            Act = () =>
            {
                TryStartCpr(ent, @event.User);
            },
            Text = Loc.GetString("verb-perform-cpr"),
        });
    }

    private void OnExamined(Entity<CprTargetComponent> ent, ref ExaminedEvent args)
    {
        if (!_heartrateQuery.TryComp(ent, out var heartrate) || heartrate.Beating)
            return;

        if (_mobState.IsDead(ent))
            return;

        args.PushMarkup(Loc.GetString("cpr-target-needs-cpr", ("target", Identity.Entity(ent, EntityManager))), priority: -5);
    }
}
