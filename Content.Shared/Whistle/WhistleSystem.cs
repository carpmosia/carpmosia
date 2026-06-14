// Carpmosia-start - Whistle action
using Content.Shared.Actions;
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

public sealed partial class WhistleSystem : EntitySystem
{
    [Dependency] private EntityLookupSystem _entityLookup = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedActionsSystem _actions = default!; // Carpmosia-edit - Whistle action
    [Dependency] private UseDelaySystem _useDelay = default!; // Carpmosia-edit - Whistle action

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WhistleComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<WhistleComponent, GetItemActionsEvent>(OnGetItemActions); // Carpmosia-edit - Whistle action
        SubscribeLocalEvent<WhistleComponent, SoundActionEvent>(OnWhistleAction); // Carpmosia-edit - Whistle action
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
        if (!_timing.IsFirstTimePredicted)
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
