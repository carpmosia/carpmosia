
using Content.Shared.Medical;
using Content.Shared.StatusEffectNew.Components;
using Robust.Shared.Timing;

namespace Content.Shared._Offbrand.StatusEffects;

public sealed partial class RadiationFlushEffectSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private EntityQuery<RadiationThresholdsComponent> _radThresholdQuery = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var enumerator = EntityQueryEnumerator<StatusEffectComponent, RadiationFlushEffectComponent>();
        var targets = new List<(EntityUid, float)>();

        while (enumerator.MoveNext(out var uid, out var effect, out var flush))
        {
            if (_timing.CurTime < flush.NextUpdate || effect.AppliedTo is not { } target)
                continue;

            if(!_radThresholdQuery.TryComp(target, out var threshold))
                continue;

            var ev = new OnRemoveRadsEvent(flush.Amount, uid);

            RaiseLocalEvent(target, ev);

        }
    }
}
