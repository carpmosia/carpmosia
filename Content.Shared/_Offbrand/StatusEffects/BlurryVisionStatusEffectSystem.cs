using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.StatusEffectNew;

namespace Content.Shared._Offbrand.StatusEffects;

public sealed partial class BlurryVisionStatusEffectSystem : EntitySystem
{
    [Dependency] private BlurryVisionSystem _blurryVision = default!;

    [SubscribeLocalEvent]
    private void OnStatusEffectApplied(Entity<BlurryVisionStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        _blurryVision.UpdateBlurMagnitude(args.Target);
    }

    [SubscribeLocalEvent]
    private void OnStatusEffectRemoved(Entity<BlurryVisionStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        _blurryVision.UpdateBlurMagnitude(args.Target);
    }

    [SubscribeLocalEvent]
    private static void OnGetBlur(Entity<BlurryVisionStatusEffectComponent> ent, ref StatusEffectRelayedEvent<GetBlurEvent> args)
    {
        args.Args.Blur += ent.Comp.Blur;
        args.Args.CorrectionPower *= ent.Comp.CorrectionPower;
    }
}
