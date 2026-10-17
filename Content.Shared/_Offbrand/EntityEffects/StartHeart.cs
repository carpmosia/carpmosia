using Content.Shared._Offbrand.Organs;
using Content.Shared._Offbrand.Wounds;
using Content.Shared.Body;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared._Offbrand.EntityEffects;

public sealed partial class StartHeart : EntityEffectBase<StartHeart>
{
    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Loc.GetString("entity-effect-guidebook-start-heart", ("chance", Probability));
    }
}

public sealed partial class StartHeartEntityEffectSystem : EntityEffectSystem<BodyComponent, StartHeart>
{
    [Dependency] private OffbrandHeartOrganSystem _heart = default!;

    protected override void Effect(Entity<BodyComponent> ent, ref EntityEffectEvent<StartHeart> args)
    {
        foreach (var organ in ent.Comp.Organs?.ContainedEntities ?? [])
        {
            if (!TryComp<OffbrandHeartOrganComponent>(organ, out var heart))
                continue;

            _heart.TryRestartHeart(organ);
        }
    }
}
