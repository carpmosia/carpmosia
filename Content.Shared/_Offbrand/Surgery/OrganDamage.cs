using Content.Shared._Offbrand.Organs;
using Content.Shared._Offbrand.Skeletons;
using Content.Shared.Body;
using Content.Shared.Construction;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._Offbrand.Surgery;

[DataDefinition]
public sealed partial class OrganDamage : IGraphCondition
{
    [DataField(required: true)]
    public ProtoId<OrganCategoryPrototype> Category;

    [DataField]
    public FixedPoint2 Max = FixedPoint2.MaxValue;

    [DataField]
    public FixedPoint2 Min = FixedPoint2.Zero;

    public bool Condition(EntityUid uid, IEntityManager entityManager)
    {
        if(!entityManager.TryGetComponent<ParentOrganComponent>(uid, out var organ))
            return false;

        foreach (var child in organ.Children)
        {
            if (!entityManager.TryGetComponent<OrganComponent>(child, out var org) || org.Category != Category)
                continue;

            if (!entityManager.TryGetComponent<DamageableOrganComponent>(child, out var damageable))
                continue;

            return damageable.Damage >= Min && damageable.Damage <= Max;
        }

        return false;
    }

    public bool DoExamine(ExaminedEvent args)
    {
        if (!IoCManager.Resolve<IEntityManager>().System<BodySystem>()
                .TryGetOrgansWithCategoryAndComponent<DamageableOrganComponent>(args.Examined, out var organs, Category))
            return false;

        if (organs[0].Comp2.Damage >= Min && organs[0].Comp2.Damage <= Max)
            return false;

        args.PushMarkup(Loc.GetString("construction-examine-lung-damage-range", ("min", Min.Float()), ("max", Max == FixedPoint2.MaxValue ? (float) int.MaxValue : Max.Float())));
        return true;
    }

    public IEnumerable<ConstructionGuideEntry> GenerateGuideEntry()
    {
        yield return new ConstructionGuideEntry()
        {
            Localization = "construction-step-lung-damage-range",
            Arguments =
                [ ("min", Min.Float()), ("max", Max == FixedPoint2.MaxValue ? int.MaxValue : Max.Float()) ],
        };
    }
}
