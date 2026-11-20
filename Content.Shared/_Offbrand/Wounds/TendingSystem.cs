using Content.Shared.Body;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction.Events;
using Content.Shared.Interaction;
using Content.Shared.Medical.Healing;
using Content.Shared.Popups;
using Content.Shared.Stacks;
using Content.Shared.StatusEffectNew;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._Offbrand.Wounds;

public sealed partial class TendingSystem : EntitySystem
{
    [Dependency] private EntityWhitelistSystem _entityWhitelist = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedStackSystem _stack = default!;
    [Dependency] private StatusEffectsSystem _statusEffects = default!;
    [Dependency] private WoundableSystem _woundable = default!;

    [Dependency] private EntityQuery<StackComponent> _stackQuery;
    [Dependency] private EntityQuery<BodyComponent> _bodyQuery;
    [Dependency] private EntityQuery<TendingComponent> _tendingQuery;
    [Dependency] private EntityQuery<WoundComponent> _woundQuery;
    [Dependency] private EntityQuery<WoundableBodyComponent> _woundableBodyQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TendingComponent, UseInHandEvent>(OnUseInHand, before: new[] { typeof(HealingSystem) });
        SubscribeLocalEvent<TendingComponent, AfterInteractEvent>(OnAfterInteract, before: new[] { typeof(HealingSystem) });
    }

    private void OnUseInHand(Entity<TendingComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        if (TryTend(ent, args.User, args.User))
            args.Handled = true;
    }

    private void OnAfterInteract(Entity<TendingComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target == null)
            return;

        if (TryTend(ent, args.Target.Value, args.User))
            args.Handled = true;
    }

    [SubscribeLocalEvent]
    private void OnTendingDoAfter(Entity<TendableWoundComponent> ent, ref TendingDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Args.Target is not { } target)
            return;

        if (!_tendingQuery.TryComp(args.Used, out var tending))
            return;

        _woundable.TendWound(ent, tending.Damage);

        var hasMoreItems = true;
        if (_stackQuery.TryComp(args.Used.Value, out var stackComp))
        {
            _stack.ReduceCount((args.Used.Value, stackComp), 1);

            if (_stack.GetCount((args.Used.Value, stackComp)) <= 0)
                hasMoreItems = false;
        }
        else
        {
            hasMoreItems = false;
            PredictedQueueDel(args.Used.Value);
        }

        _audio.PlayPredicted(tending.TendingEndSound, target, args.User);

        if (hasMoreItems)
        {
            TryTend((args.Used.Value, tending), target, args.Args.User, true);
        }
        else
        {
            _popup.PopupEntity(Loc.GetString(tending.UsedUp, ("tending", args.Used.Value)), args.Args.User, args.Args.User);
        }
    }

    private Entity<TendableWoundComponent, WoundComponent>? GetWoundToTend(Entity<TendingComponent> ent, Entity<WoundableBodyComponent?> target)
    {
        if (!_bodyQuery.TryComp(target, out var body))
            return null;

        foreach (var organ in body.Organs?.ContainedEntities ?? [])
        {
            if (!_statusEffects.TryEffectsWithComp<TendableWoundComponent>(organ, out var effects))
                continue;

            foreach (var wound in effects)
            {
                if (wound.Comp1.Tended)
                    continue;

                if (!_entityWhitelist.CheckBoth(wound, ent.Comp.WoundBlacklist, ent.Comp.WoundWhitelist))
                    continue;

                return (wound.Owner, wound.Comp1, _woundQuery.Comp(wound));
            }
        }

        return null;
    }

    private bool TryTend(Entity<TendingComponent> ent, EntityUid target, EntityUid user, bool isRepeat = false)
    {
        if (!_woundableBodyQuery.HasComp(target))
            return false;

        var woundToTend = GetWoundToTend(ent, target);
        if (woundToTend is not { } foundWound)
        {
            if (isRepeat)
                _popup.PopupEntity(Loc.GetString(ent.Comp.NothingToTendRepeat, ("target", Identity.Entity(target, EntityManager)), ("tending", ent)), user, user);
            else
                _popup.PopupEntity(Loc.GetString(ent.Comp.NothingToTend, ("target", Identity.Entity(target, EntityManager)), ("tending", ent)), user, user);

            return true;
        }

        if (user != target && !_interaction.InRangeUnobstructed(user, target, popup: true))
            return false;

        if (_stackQuery.TryComp(ent, out var stack) && stack.Count < 1)
            return false;

        _audio.PlayPredicted(ent.Comp.TendingBeginSound, ent, user);

        var differentTarget = user != target;

        var delay = ent.Comp.Delay;
        if (!differentTarget)
            delay *= ent.Comp.SelfTendPenaltyModifier;

        if (differentTarget)
        {
            _popup.PopupEntity(
                Loc.GetString(ent.Comp.UserPopup, ("target", Identity.Entity(target, EntityManager)), ("tending", ent), ("wound", foundWound)),
                Loc.GetString(ent.Comp.OtherPopup, ("user", Identity.Entity(user, EntityManager)), ("target", Identity.Entity(target, EntityManager)), ("tending", ent), ("wound", foundWound)),
                target,
                user
            );
        }
        else
        {
            _popup.PopupEntity(Loc.GetString(ent.Comp.SelfPopup, ("tending", ent), ("wound", foundWound)), user, user);
        }

        var args =
            new DoAfterArgs(EntityManager, user, delay, new TendingDoAfterEvent(), foundWound, target: target, used: ent)
            {
                NeedHand = true,
                BreakOnMove = true,
                BreakOnWeightlessMove = false,
            };

        _doAfter.TryStartDoAfter(args);
        return true;
    }
}
