using System.Collections.Generic;
using System.Linq;
using Content.Shared.Construction.Components;
using YamlDotNet.RepresentationModel;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed partial class MapRulesTest
{
    private List<string> TestAnchorableDuplicates(YamlSequenceNode entities)
    {
        var anchorables = GetPrototypeIds<AnchorableComponent>();

        var errors = new List<string>();

        foreach (var proto in anchorables)
        {
            foreach (var ((grid, (x, y), _), count) in DeserializeCompNodes(entities, [proto], GetApproxTransform)
                .GroupBy(x => x).Where(x => x.Count() > 1).Select(x => (x.Key, x.Count())))
            {
                errors.Add($"Grid {grid} contains {count} duplicate {proto} at <{x / 10}, {y / 10}>");
            }
        }

        return errors;
    }
}
