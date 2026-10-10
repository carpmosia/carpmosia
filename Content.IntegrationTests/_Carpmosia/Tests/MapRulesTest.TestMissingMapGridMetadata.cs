using System.Collections.Generic;
using System.Linq;
using Robust.Shared.GameObjects;
using Robust.Shared.Utility;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed partial class MapRulesTest
{
    // Substations don't have a unique component sadly
    private static readonly string[] DisallowedMetadata = [
      "grid",
      "map ",
    ];

    /// <summary>
    /// Ensures that all grids have a name
    /// </summary>
    private List<string> TestMissingMapGridMetadata(ParsedRoot root)
    {
        var errors = new List<string>();

        foreach (var (uid, ent) in root.Entities[""].Where(x => root.MapIds.Contains(x.Key) || root.GridIds.Contains(x.Key)))
        {
            if (GetCompNode<MetaDataComponent>(ent) is not { } meta
                || !meta.TryGetNode("name", out var name))
            {
                errors.Add($"Map or Grid {uid} is missing a name");
                continue;
            }

            if (DisallowedMetadata.Any(x => name.ToString().StartsWith(x)))
                errors.Add($"Map or Grid {uid} has an improper name {name}");
        }

        return errors;
    }
}
