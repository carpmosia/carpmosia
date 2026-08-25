using System.Collections.Generic;
using System.Linq;
using Robust.Shared.Utility;
using YamlDotNet.RepresentationModel;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed partial class MapRulesTest
{
    private List<string> TestNoCenteredGrid(YamlSequenceNode grids, YamlSequenceNode entities)
    {
        var gridIds = grids.Select(node => node.AsInt()).ToArray();

        // I don't think there is a good way to discern a "primary" grid, so actually we just ensure that at least one grid is at 0,0 (doesn't have pos)
        if (entities
            .Where(x => string.IsNullOrEmpty(x[Proto].ToString()))
            .SelectMany(x => ((YamlSequenceNode)x[Entities])
                .Where(x => gridIds.Contains(x["uid"].AsInt()))
            ).Any(x => GetCompNode(x, "Transform") is { } trans && !trans.HasNode("pos")))
            return [];

        return ["No grid found at <0, 0>"];
    }
}
