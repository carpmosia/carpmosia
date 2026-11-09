using System.Linq;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mobs;
using Content.Shared.StatusEffectNew;

namespace Content.Shared._Offbrand.Wounds;

public sealed partial class ShockThresholdsSystem : EntitySystem
{
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private PainSystem _pain = default!;
    [Dependency] private StatusEffectsSystem _statusEffects = default!;

    [SubscribeLocalEvent]
    private void OnShutdown(Entity<ShockThresholdsComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.CurrentThresholdState is { } effect)
            _statusEffects.TryRemoveStatusEffect(ent, effect);
    }

    [SubscribeLocalEvent]
    private void OnAfterShockChange(Entity<ShockThresholdsComponent> ent, ref AfterShockChangeEvent args)
    {
        var shock = _pain.GetShock(ent.Owner);
        UpdateEffects(ent, shock);
        UpdateState(ent, shock);
    }

    private void UpdateEffects(Entity<ShockThresholdsComponent> ent, FixedPoint2 shock)
    {
        var targetEffect = ent.Comp.Thresholds.HighestMatch(shock);
        if (targetEffect == ent.Comp.CurrentThresholdState)
            return;

        var seenTarget = targetEffect is null;
        if (ent.Comp.CurrentThresholdState is { } oldEffect)
            _statusEffects.TryRemoveStatusEffect(ent, oldEffect);

        if (targetEffect is { } effect)
            _statusEffects.TryUpdateStatusEffectDuration(ent, effect, out _);

        var overlays = new PotentiallyUpdateDamageOverlayEvent(ent);
        RaiseLocalEvent(ent, ref overlays, true);

        ent.Comp.CurrentThresholdState = targetEffect;
        Dirty(ent);
    }

    private void UpdateState(Entity<ShockThresholdsComponent> ent, FixedPoint2 shock)
    {
        var state = ent.Comp.MobThresholds.HighestMatch(shock) ?? MobState.Alive;

        if (state == ent.Comp.CurrentMobState || _mobState.IsDead(ent.Owner))
            return;

        var ev = new ShockStateChangedEvent
        {
            OldMobState = ent.Comp.CurrentMobState,
            NewMobState = state,
            Component = ent.Comp,
            Target = ent.Owner,
        };
        RaiseLocalEvent(ev.Target, ev);

        ent.Comp.CurrentMobState = state;
        Dirty(ent);
    }

    public bool IsCritical(Entity<ShockThresholdsComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        return ent.Comp.CurrentThresholdState == ent.Comp.Thresholds.Last().Value;
    }

    /// <summary>
    /// Event that is raised whenever a ShockThresholds MobState changes on an entity
    /// </summary>
    /// <param name="Target">The Entity whose MobState is changing</param>
    /// <param name="Component">The MobState Component owned by the Target entity</param>
    /// <param name="OldMobState">The previous MobState</param>
    /// <param name="NewMobState">The new MobState</param>
    /// <param name="Origin">The Entity that caused this state change</param>
    public record struct ShockStateChangedEvent(EntityUid Target, ShockThresholdsComponent Component, MobState OldMobState,
        MobState NewMobState, EntityUid? Origin = null);
}
