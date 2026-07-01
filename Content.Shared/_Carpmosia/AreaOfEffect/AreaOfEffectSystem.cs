using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Whitelist;
using Robust.Shared.Timing;

namespace Content.Shared._Carpmosia.AreaOfEffect;

/// <summary>
/// This handles area of effect damage application to nearby damageable entities.
/// </summary>
public sealed class AreaOfEffectSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    private readonly Dictionary<EntityUid, AreaOfEffectTiming> _timings = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AreaOfEffectComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<AreaOfEffectComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<AreaOfEffectComponent> entity, ref ComponentStartup args)
    {
        _timings[entity.Owner] = new AreaOfEffectTiming(_timing);
    }

    private void OnShutdown(Entity<AreaOfEffectComponent> entity, ref ComponentShutdown args)
    {
        _timings.Remove(entity.Owner);
    }

    private struct AreaOfEffectTiming(IGameTiming timing)
    {
        public TimeSpan NextApplicationTime = timing.CurTime;
        public readonly TimeSpan StartTime = timing.CurTime;
    }

    /// <inheritdoc/>
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<AreaOfEffectComponent>();

        while (query.MoveNext(out var uid, out var aoe))
        {
            // Check if duration has elapsed and remove component if so
            if (aoe.Duration.HasValue)
            {
                var elapsed = curTime - _timings[uid].StartTime;
                if (elapsed >= aoe.Duration.Value)
                {
                    RemComp<AreaOfEffectComponent>(uid);
                    continue;
                }
            }

            var nextTime = _timings[uid].NextApplicationTime;

            // Check if it's time to apply damage
            if (nextTime > curTime)
                continue;

            // Apply damage to nearby entities
            ApplyAreaOfEffectDamage(new Entity<AreaOfEffectComponent>(uid, aoe));

            // Schedule next application
            var timing = _timings[uid];
            timing.NextApplicationTime = curTime + aoe.Cooldown;
            _timings[uid] = timing;
        }
    }

    /// <summary>
    /// Applies damage to all entities within the given radius.
    /// </summary>
    /// <param name="entity">The area of effect entity</param>
    private void ApplyAreaOfEffectDamage(Entity<AreaOfEffectComponent> entity)
    {
        var inRange = _lookup.GetEntitiesInRange<DamageableComponent>(Transform(entity.Owner).Coordinates, entity.Comp.Radius);

        foreach (var target in inRange)
        {
            if (!_whitelist.CheckBoth(target.Owner, entity.Comp.Blacklist, entity.Comp.Whitelist))
            {
                inRange.Remove(target);
            }
        }

        var damage = entity.Comp.Damage;
        if (entity.Comp.DamageSpread)
        {
            damage = new DamageSpecifier();
            foreach (var (damageType, amount) in entity.Comp.Damage.DamageDict)
            {
                damage.DamageDict[damageType] = amount / inRange.Count;
            }
        }

        foreach (var target in inRange)
        {
            _damageable.TryChangeDamage(
                target.Owner,
                damage,
                ignoreResistances: false,
                interruptsDoAfters: true,
                origin: entity.Owner);

        }
    }
}
