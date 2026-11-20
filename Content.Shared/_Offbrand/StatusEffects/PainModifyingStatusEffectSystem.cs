using Content.Shared.StatusEffectNew;
using Content.Shared._Offbrand.Wounds;

namespace Content.Shared._Offbrand.StatusEffects;

public sealed partial class PainModifyingStatusEffectSystem : EntitySystem
{
    [Dependency] private EntityQuery<IntrinsicPainComponent> _painQuery;

    [SubscribeLocalEvent]
    private void OnStatusEffectApplied(Entity<PainModifyingStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        if(!_painQuery.TryComp(args.Target, out var pain))
        {
            throw new InvalidOperationException("No pain component {ent}");
        }


        pain.FlatPainModifier += ent.Comp.Amount;
    }

    [SubscribeLocalEvent]
    private void OnStatusEffectRemoved(Entity<PainModifyingStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if(!_painQuery.TryComp(args.Target, out var pain))
            return;

        pain.FlatPainModifier -= ent.Comp.Amount;
    }
}
