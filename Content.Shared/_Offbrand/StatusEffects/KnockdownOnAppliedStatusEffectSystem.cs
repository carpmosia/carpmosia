using Content.Shared.Stunnable;
using Content.Shared.StatusEffectNew;

namespace Content.Shared._Offbrand.StatusEffects;

public sealed partial class KnockdownOnAppliedStatusEffectSystem : EntitySystem
{
    [Dependency] private SharedStunSystem _stun = default!;

    [SubscribeLocalEvent]
    private void OnStatusEffectApplied(Entity<KnockdownOnAppliedStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        _stun.TryKnockdown(args.Target, ent.Comp.Duration, force: true);
    }
}
