using System.Collections.Generic;
using System.Linq;
using Robust.Shared.GameObjects;
using Robust.Shared.Utility;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed partial class MapRulesTest
{
    /// <summary>
    /// Ensures that there is at least one grid at world center
    /// </summary>
    private List<string> TestNoCenteredGrid(ParsedRoot root)
    {
        // I don't think there is a good way to discern a "primary" grid, so actually we just ensure that at least one grid is at 0,0 (doesn't have pos)
        if (root.Entities[""].Any(x => root.GridIds.Contains(x.Key) && GetCompNode<TransformComponent>(x.Value) is { } trans && !trans.HasNode("pos")))
            return [];

        return ["No grid found at <0, 0>"];
    }
}
