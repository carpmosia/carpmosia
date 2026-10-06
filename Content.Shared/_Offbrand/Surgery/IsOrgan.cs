using System.Threading;
using Content.Shared.Construction;
using Content.Shared.Examine;
using Content.Shared.Body;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Prototypes;

namespace Content.Shared._Offbrand.Surgery;

[DataDefinition]
public sealed partial class IsOrgan : IGraphCondition
{
    [DataField(required: true)]
    public ProtoId<OrganCategoryPrototype>  Category;

    [DataField]
    public bool ShouldHave = true;

    public bool Condition(EntityUid uid, IEntityManager entityManager)
    {

        if (!entityManager.TryGetComponent<OrganComponent>(uid, out var organ))
            return false;

        return (organ.Category == Category) == ShouldHave;
    }

    public bool DoExamine(ExaminedEvent args)
    {
        var entity = args.Examined;

        if(!IoCManager.Resolve<IEntityManager>().TryGetComponent<OrganComponent>(entity, out var organ))
            return false;

        var isOrgan = organ.Category == Category;

        switch (ShouldHave)
        {
            case true when !isOrgan:
                args.PushMarkup(Loc.GetString("construction-examine-status-effect-should-have", ("effect", "effect")));
                return true;
            case false when isOrgan:
                args.PushMarkup(Loc.GetString("construction-examine-status-effect-should-not-have", ("effect", "effect")));
                return true;
        }

        return false;
    }

    public IEnumerable<ConstructionGuideEntry> GenerateGuideEntry()
    {
        yield return new ConstructionGuideEntry()
        {
            Localization = ShouldHave
                ? "construction-step-condition-status-effect-should-have"
                : "construction-step-condition-status-effect-should-not-have",
            Arguments =
                [("effect", "effect")],
        };
    }
}
