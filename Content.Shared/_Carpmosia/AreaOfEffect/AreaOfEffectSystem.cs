using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._Carpmosia.AreaOfEffect;

/// <summary>
/// This handles area of effect damage application to nearby damageable entities.
/// </summary>
public sealed class AreaOfEffectSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    /// <inheritdoc/>
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<AreaOfEffectComponent>();
        while (query.MoveNext(out var uid, out var aoe))
        {
            // Check if it's time to apply damage
            if (aoe.NextApplicationTime > _timing.CurTime.TotalSeconds)
                continue;

            // Apply damage to nearby entities
            ApplyAreaOfEffectDamage(uid, aoe.Damage, aoe.Radius);

            // Schedule next application
            aoe.NextApplicationTime = (float)(_timing.CurTime.TotalSeconds + aoe.Cooldown);
        }
    }

    /// <summary>
    /// Applies damage to all entities within the given radius.
    /// </summary>
    /// <param name="uid">The area of effect entity</param>
    /// <param name="damage">A list of all to be applied damages</param>
    /// <param name="radius">The application radius</param>
    public void ApplyAreaOfEffectDamage(EntityUid uid, Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2> damage, float radius)
    {
        var nearbyEntities = GetNearbyDamageableEntities(uid, radius);

        foreach (var targetUid in nearbyEntities)
        {
            var damageSpecifier = new DamageSpecifier();
            foreach (var (damageTypeId, amount) in damage)
            {
                damageSpecifier.DamageDict[damageTypeId] = amount;
            }

            _damageable.TryChangeDamage(
                (targetUid, null),
                damageSpecifier,
                ignoreResistances: false,
                interruptsDoAfters: true,
                origin: uid);
        }
    }

    /// <summary>
    /// Gets all damageable entities near the specified area of effect entity within the given radius.
    /// </summary>
    /// <param name="uid">The area of effect entity</param>
    /// <param name="radius">The detection radius</param>
    /// <returns>A list of damageable entities within the radius</returns>
    public List<EntityUid> GetNearbyDamageableEntities(EntityUid uid, float radius)
    {
        var result = new List<EntityUid>();

        var aoeTransform = Transform(uid);

        if (!TryComp(uid, out AreaOfEffectComponent? aoe))
            return result;

        var aoePosition = _transform.GetWorldPosition(aoeTransform);
        var aoeMap = aoeTransform.MapID;

        // Query all damageable entities
        var query = EntityQueryEnumerator<DamageableComponent>();
        while (query.MoveNext(out var damageableUid, out _))
        {
            if (!TryComp(damageableUid, out TransformComponent? damageableTransform))
                continue;

            // Check if on same map
            if (damageableTransform.MapID != aoeMap)
                continue;

            var damageablePosition = _transform.GetWorldPosition(damageableTransform);
            var distance = (damageablePosition - aoePosition).Length();

            if (distance > radius)
                continue;

            // Check whitelist/blacklist filtering
            if (!_whitelist.CheckBoth(damageableUid, aoe.Blacklist, aoe.Whitelist))
                continue;

            result.Add(damageableUid);
        }

        return result;
    }
}
