using Content.Shared._Offbrand.Wounds;
using Content.Shared.StatusEffectNew;

namespace Content.Shared._Offbrand.StatusEffects;

public sealed partial class PainSuppressionStatusEffectSystem : EntitySystem
{
    [Dependency] private PainSystem _pain = default!;

    [SubscribeLocalEvent]
    private static void OnPainSuppression(Entity<PainSuppressionStatusEffectComponent> ent, ref StatusEffectRelayedEvent<PainSuppressionEvent> args)
    {
        args.Args = args.Args with { Suppressed = true };
    }

    [SubscribeLocalEvent]
    private void OnStatusEffectApplied(Entity<PainSuppressionStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        _pain.UpdateSuppression(args.Target);
    }

    [SubscribeLocalEvent]
    private void OnStatusEffectRemoved(Entity<PainSuppressionStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        _pain.UpdateSuppression(args.Target);
    }
}
