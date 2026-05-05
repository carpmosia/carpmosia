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
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    private readonly HashSet<EntityUid> _entitiesInRange = new();
    private readonly Dictionary<EntityUid, AreaOfEffectTiming> _timings = new();
    private readonly Dictionary<EntityUid, DamageSpecifier> _damageSpecifiers = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AreaOfEffectComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<AreaOfEffectComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(EntityUid uid, AreaOfEffectComponent component, ComponentStartup args)
    {
        _timings[uid] = new AreaOfEffectTiming
        {
            NextApplicationTime = null,
            StartTime = _timing.CurTime,
        };
    }

    private void OnShutdown(EntityUid uid, AreaOfEffectComponent component, ComponentShutdown args)
    {
        _timings.Remove(uid);
        _damageSpecifiers.Remove(uid);
    }

    private struct AreaOfEffectTiming
    {
        public TimeSpan? NextApplicationTime;
        public TimeSpan StartTime;
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
            if (nextTime.HasValue && nextTime > curTime)
                continue;

            // Apply damage to nearby entities
            ApplyAreaOfEffectDamage(uid, aoe);

            // Schedule next application
            var timing = _timings[uid];
            timing.NextApplicationTime = curTime + aoe.Cooldown;
            _timings[uid] = timing;
        }
    }

    /// <summary>
    /// Applies damage to all entities within the given radius.
    /// </summary>
    /// <param name="uid">The area of effect entity</param>
    /// <param name="aoe">The area of effect component</param>
    private void ApplyAreaOfEffectDamage(EntityUid uid, AreaOfEffectComponent aoe)
    {
        if (!_damageSpecifiers.TryGetValue(uid, out var damageSpecifier))
        {
            damageSpecifier = new DamageSpecifier();
            foreach (var (damageTypeId, amount) in aoe.Damage)
            {
                damageSpecifier.DamageDict[damageTypeId] = amount;
            }
            _damageSpecifiers[uid] = damageSpecifier;
        }

        foreach (var targetUid in _lookup.GetEntitiesInRange<DamageableComponent>(Transform(uid).Coordinates, aoe.Radius))
        {
            // Check whitelist/blacklist filtering
            if (!_whitelist.CheckBoth(targetUid, aoe.Blacklist, aoe.Whitelist))
                continue;

            _damageable.TryChangeDamage(
                targetUid!,
                damageSpecifier,
                ignoreResistances: false,
                interruptsDoAfters: true,
                origin: uid);
        }
    }
}
