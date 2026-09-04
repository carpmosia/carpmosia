
using Content.Shared.FixedPoint;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mobs;
using Content.Shared.Radiation.Events;
using Content.Shared.StatusEffectNew;
using Content.Shared._Offbrand.Wounds;
using Robust.Shared.Prototypes;
using Content.Shared._Offbrand.StatusEffects;

namespace Content.Shared.Medical;

/// <summary>
/// The brains behind rad sickness.
/// </summary>
public sealed partial class RadiationSicknessSystem : EntitySystem
{

    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private StatusEffectsSystem _statusEffects = default!;

    [Dependency] private EntityQuery<RadProtectionStatusEffectComponent> _protQuery = default!;

     EntProtoId RadProtection = "RadProtection";

    private void OnShutdown(Entity<RadiationThresholdsComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.CurrentThresholdState is { } effect)
            _statusEffects.TryRemoveStatusEffect(ent, effect);
    }


    [SubscribeLocalEvent]
    private void OnIrradiated(Entity<RadiationThresholdsComponent> ent, ref OnIrradiatedEvent args)
    {
        if (!_mobState.IsAlive(ent))
            return;

        var radsToAdd = args.TotalRads;

        if (_statusEffects.TryGetStatusEffect(ent, RadProtection, out var protection) && _protQuery.TryComp(protection, out var comp))
            radsToAdd = radsToAdd * comp.Modifier;

        ent.Comp.Rads += radsToAdd;

        UpdateEffects(ent);
    }

    [SubscribeLocalEvent]
    private void OnRemoveRads(Entity<RadiationThresholdsComponent> ent, ref OnRemoveRadsEvent args)
    {
        ent.Comp.Rads = Math.Max((float) ent.Comp.Rads - args.Rads, 0f);

        UpdateEffects(ent);
    }

    private void UpdateEffects(Entity<RadiationThresholdsComponent> ent)
    {
        var targetEffect = ent.Comp.Thresholds.HighestMatch(ent.Comp.Rads);
        if (targetEffect == ent.Comp.CurrentThresholdState)
            return;

        var seenTarget = targetEffect is null;
        if (ent.Comp.CurrentThresholdState is { } oldEffect)
            _statusEffects.TryRemoveStatusEffect(ent, oldEffect);

        if (targetEffect is { } effect)
            _statusEffects.TryUpdateStatusEffectDuration(ent, effect, out _);

        ent.Comp.CurrentThresholdState = targetEffect;
        Dirty(ent);
    }


}
public readonly record struct OnRemoveRadsEvent(float Rads, EntityUid? Origin)
{
    public readonly float Rads = Rads;
    public readonly EntityUid? Origin = Origin;
}
