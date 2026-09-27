
using Content.Shared.Medical;
using Content.Shared.StatusEffectNew;
using Content.Shared.StatusEffectNew.Components;
using Robust.Shared.Timing;

namespace Content.Shared._Offbrand.StatusEffects;

/// <summary>
/// System defining how temporary radiation protection effects should work.
/// </summary>
public sealed partial class RadProtectionStatusEffectSystem : EntitySystem
{
    [Dependency] private EntityQuery<RadiationThresholdsComponent> _radThresholdQuery = default!;

    [SubscribeLocalEvent]
    private void OnStatusEffectApplied(Entity<RadProtectionStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        if(!_radThresholdQuery.TryComp(args.Target, out var threshold))
            return;

        threshold.Modifier *= ent.Comp.Modifier;
    }

    [SubscribeLocalEvent]
    private void OnStatusEffectRemoved(Entity<RadProtectionStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if(!_radThresholdQuery.TryComp(args.Target, out var threshold))
            return;

        // This Math.Min should never matter but better safe than sorry
        threshold.Modifier = Math.Min(threshold.Modifier/ent.Comp.Modifier, 1f);
    }
}
