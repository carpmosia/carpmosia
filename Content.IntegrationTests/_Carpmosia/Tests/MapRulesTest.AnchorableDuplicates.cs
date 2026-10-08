using System.Collections.Generic;
using System.Linq;
using Content.Shared.Construction.Components;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed partial class MapRulesTest
{
    /// <summary>
    /// Checks for any two or more entities of the same prototype anchored in the same tile and rotation
    /// </summary>
    private List<string> TestAnchorableDuplicates(ParsedRoot root)
    {
        var anchorables = GetPrototypeIds<AnchorableComponent>();

        var errors = new List<string>();

        foreach (var proto in anchorables)
        {
            foreach (var ((grid, (x, y), _), count) in DeserializeCompNodes(root.Entities, [proto], GetApproxTransform).Values
                .GroupBy(x => x).Where(x => x.Count() > 1).Select(x => (x.Key, x.Count())))
            {
                errors.Add($"Grid {grid} contains {count} duplicate {proto} at <{x / 10}, {y / 10}>");
            }
        }

        return errors;
    }
}
