// Carpmosia-start - Whistle action
using Content.Shared.Actions;
using Content.Shared.Actions.Events;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;
using Content.Shared.Timing;
// Carpmosia-end - Whistle action
using Content.Shared.Coordinates;
using Content.Shared.Humanoid;
using Content.Shared.Interaction.Events;
using Content.Shared.Stealth.Components;
using JetBrains.Annotations;
using Robust.Shared.Timing;

namespace Content.Shared.Whistle;

public sealed class WhistleSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    // Carpmosia-start - Whistle action
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;
    // Carpmosia-end - Whistle action

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WhistleComponent, UseInHandEvent>(OnUseInHand);
        // Carpmosia-start - Whistle action
        SubscribeLocalEvent<WhistleComponent, GetItemActionsEvent>(OnGetItemActions);
        SubscribeLocalEvent<WhistleComponent, SoundActionEvent>(OnWhistleAction);
        // Carpmosia-end - Whistle action
    }

    // Carpmosia-start - Whistle action
    private void OnGetItemActions(Entity<WhistleComponent> ent, ref GetItemActionsEvent args)
    {
        if (args.SlotFlags == SlotFlags.POCKET)
            return;

        args.AddAction(ref ent.Comp.Action, ent.Comp.ActionId);
    }

    public void OnWhistleAction(Entity<WhistleComponent> ent, ref SoundActionEvent args)
    {
        if (args.Handled || !_timing.IsFirstTimePredicted)
            return;

        TryMakeLoudWhistle(ent, args.Performer);
        args.Handled = true;
    }
    // Carpmosia-end - Whistle action

    private void ExclamateTarget(EntityUid target, WhistleComponent component)
    {
        SpawnAttachedTo(component.Effect, target.ToCoordinates());
    }

    public void OnUseInHand(EntityUid uid, WhistleComponent component, UseInHandEvent args)
    {
        if (args.Handled || !_timing.IsFirstTimePredicted)
            return;

        args.Handled = TryMakeLoudWhistle(uid, args.User, component);
    }

    public bool TryMakeLoudWhistle(EntityUid uid, EntityUid owner, WhistleComponent? component = null)
    {
        if (!Resolve(uid, ref component, false) || component.Distance <= 0)
            return false;

        // Carpmosia-start - Whistle action
        if (TryComp<UseDelayComponent>(uid, out var useDelay))
        {
            _actions.SetCooldown(component.Action, useDelay.Delay); // i'd like this to use ent.Comp.Action instead but i cba to convert the whole file to use Entity<T>
            _useDelay.SetLength(owner, useDelay.Delay);
            _useDelay.TryResetDelay((owner, useDelay));
        }
        // Carpmosia-end - Whistle action

        MakeLoudWhistle(uid, owner, component);
        return true;
    }

    private void MakeLoudWhistle(EntityUid uid, EntityUid owner, WhistleComponent component)
    {
        StealthComponent? stealth = null;

        foreach (var iterator in
            _entityLookup.GetEntitiesInRange<HumanoidProfileComponent>(_transform.GetMapCoordinates(uid), component.Distance))
        {
            //Avoid pinging invisible entities
            if (TryComp(iterator, out stealth) && stealth.Enabled)
                continue;

            //We don't want to ping user of whistle
            if (iterator.Owner == owner)
                continue;

            ExclamateTarget(iterator, component);
        }
    }
}
