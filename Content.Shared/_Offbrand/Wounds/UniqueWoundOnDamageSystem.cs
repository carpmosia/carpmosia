using Content.Shared.Damage.Systems;
using Content.Shared.Random.Helpers;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._Offbrand.Wounds;

public sealed partial class UniqueWoundOnDamageSystem : OffbrandDamageSystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private WoundableSystem _woundable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UniqueWoundOnDamageComponent, DamageDealtEvent>(OnDamageDealt, after: [typeof(WoundableSystem)]);
    }

    private void OnDamageDealt(Entity<UniqueWoundOnDamageComponent> ent, ref DamageDealtEvent args)
    {
        var rand = SharedRandomExtensions.PredictedRandom(_timing, GetNetEntity(ent));

        var woundable = Comp<WoundableComponent>(ent);

        foreach (var wound in ent.Comp.Wounds)
        {
            var incomingAmount = ThresholdHelpers.Count(wound.DamageTypes, args.Damage);
            var totalAmount = ThresholdHelpers.Count(wound.DamageTypes, woundable.Damage);

            if (incomingAmount < wound.MinimumDamage || totalAmount < wound.MinimumTotalDamage)
                continue;

            var probability = wound.DamageProbabilityCoefficient * incomingAmount.Float() + wound.TotalProbabilityCoefficient * totalAmount.Float() + wound.DamageProbabilityConstant;
            probability = Math.Clamp(probability, 0f, 1f); // Floating point errors <3
            if (!rand.Prob(probability))
                continue;

            _woundable.TryWound((ent.Owner, woundable), wound.WoundPrototype, wound.WoundDamages, unique: true);
        }
    }
}
