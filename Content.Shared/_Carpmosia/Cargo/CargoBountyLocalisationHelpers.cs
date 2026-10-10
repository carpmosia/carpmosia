using Content.Shared.Cargo.Prototypes;

namespace Content.Shared.Cargo;

public static class CargoBountyLocalisationHelpers
{
    public static string GetManifestText(CargoBountyEntry entry)
    {
        return entry switch
        {
            CargoBountyItemEntry itemEntry => Loc.GetString("bounty-console-manifest-entry",
                ("amount", itemEntry.Amount),
                ("item", Loc.GetString(itemEntry.Name))),
            CargoBountyReagentEntry reagentEntry => Loc.GetString("bounty-console-manifest-reagent-entry",
                ("amount", reagentEntry.Amount),
                ("reagent", Loc.GetString(reagentEntry.Name))),
            _ => throw new NotSupportedException("Unsupported bounty entry type."),
        };
    }
}
