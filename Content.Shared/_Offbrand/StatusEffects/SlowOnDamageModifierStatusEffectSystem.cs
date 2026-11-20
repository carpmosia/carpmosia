using Content.Shared.Damage.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.StatusEffectNew;

namespace Content.Shared._Offbrand.StatusEffects;

public sealed partial class SlowOnDamageModifierStatusEffectSystem : EntitySystem
{
    [Dependency] private MovementSpeedModifierSystem _movementSpeedModifier = default!;

    [SubscribeLocalEvent]
    private void OnStatusEffectApplied(Entity<SlowOnDamageModifierStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        _movementSpeedModifier.RefreshMovementSpeedModifiers(args.Target);
    }

    [SubscribeLocalEvent]
    private void OnStatusEffectRemoved(Entity<SlowOnDamageModifierStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        _movementSpeedModifier.RefreshMovementSpeedModifiers(args.Target);
    }

    [SubscribeLocalEvent]
    private void OnModifySlowOnDamageSpeed(Entity<SlowOnDamageModifierStatusEffectComponent> ent, ref StatusEffectRelayedEvent<ModifySlowOnDamageSpeedEvent> args)
    {
        var delta = 1f - args.Args.Speed;
        if (delta <= 0f)
            return;

        args.Args = args.Args with { Speed = args.Args.Speed + delta * ent.Comp.Modifier };
    }
}
