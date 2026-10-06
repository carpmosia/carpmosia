using System.Linq;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mobs;

namespace Content.Shared._Offbrand.Wounds;

public sealed partial class HealthRankingSystem : EntitySystem
{
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private PainSystem _pain = default!;
    [Dependency] private ShockThresholdsSystem _shockThresholds = default!;

    [Dependency] private EntityQuery<ShockThresholdsComponent> _shockThresholdsQuery;
    [Dependency] private EntityQuery<WoundableBodyComponent> _woundableBodyQuery;

    private const float PainDeathRatio = 0.5f;
    private const float BrainDeathRatio = 0.3f;
    private const float HeartDeathRatio = 0.2f;

    public float? RankHealth(EntityUid entity, MobState targetMobState)
    {
        if (!_woundableBodyQuery.HasComp(entity))
            return null;

        if (targetMobState == MobState.Invalid)
            targetMobState = MobState.Critical;

        if (targetMobState == MobState.Alive)
            return 1f;

        if (_shockThresholds.IsCritical(entity))
            return 0f;

        if (!_shockThresholdsQuery.TryComp(entity, out var shockThresholds))
            return 1f;

        var dict = shockThresholds.Thresholds; // TODO: just move this to a ShockThresholds method, like TryGetHealthRanking or something
        var percentageToPainCrit = _pain.GetShock(entity).Float() / dict.Keys.Last().Float();
        return 1f - (Math.Clamp(percentageToPainCrit, 0f, 1f));
    }

    public bool IsCritical(EntityUid uid)
    {
        return _mobState.IsCritical(uid) || _shockThresholds.IsCritical(uid);
    }
}
