using Content.Shared._Offbrand.Organs;
using Content.Shared._Offbrand.Skeletons;
using Content.Shared._Offbrand.Wounds;
using Content.Shared.Body;
using Content.Shared.Construction;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._Offbrand.Surgery;

[DataDefinition]
public sealed partial class ChangeOrganDamage : IGraphAction
{
    [DataField(required: true)]
    public ProtoId<OrganCategoryPrototype> Category;

    [DataField(required: true)]
    public FixedPoint2 Amount;

    public void PerformAction(EntityUid uid, EntityUid? userUid, IEntityManager entityManager)
    {

        if(!entityManager.TryGetComponent<ParentOrganComponent>(uid, out var organ))
            return;

        foreach (var child in organ.Children)
        {
            if (!entityManager.TryGetComponent<OrganComponent>(child, out var org) || org.Category != Category)
                continue;

            entityManager.System<DamageableOrganSystem>()
                .ChangeDamage(child, Amount);
        }
    }
}
