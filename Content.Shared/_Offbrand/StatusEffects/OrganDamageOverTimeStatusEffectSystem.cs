using Content.Shared._Offbrand.Organs;
using Content.Shared.Body;
using Content.Shared.Damage.Systems;
using Content.Shared.Damage;
using Content.Shared.StatusEffectNew.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._Offbrand.StatusEffects;

public sealed partial class OrganDamageOverTimeStatusEffectSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private DamageableOrganSystem _damageable = default!;
    [Dependency] private EntityQuery<BodyComponent> _bodyQuery = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var enumerator = EntityQueryEnumerator<StatusEffectComponent, OrganDamageOverTimeStatusEffectComponent>();
        var targets = new List<(EntityUid, float)>();

        while (enumerator.MoveNext(out var uid, out var effect, out var damageOverTime))
        {
            if (_timing.CurTime < damageOverTime.NextUpdate || effect.AppliedTo is not { } target)
                continue;

            if(!_bodyQuery.TryComp(target, out var body))
                return;

            foreach (var category in damageOverTime.Categories)
            {
                foreach (var organ in body.Organs?.ContainedEntities ?? [])
                {
                    if (!TryComp<OrganComponent>(organ, out var comp) || comp.Category != category)
                        continue;
                    damageOverTime.NextUpdate = _timing.CurTime + damageOverTime.UpdateInterval;
                    Dirty(uid, damageOverTime);

                    targets.Add((organ, damageOverTime.Amount));
                }
            }
        }

        // work around a concurrent modification exception
        foreach (var (org, damage) in targets)
        {
            _damageable.ChangeDamage(org, damage);
        }
    }
}
