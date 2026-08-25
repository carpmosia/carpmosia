using System.Collections.Generic;
using System.Linq;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using YamlDotNet.RepresentationModel;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed partial class MapRulesTest
{
    // Substations don't have a unique component sadly
    private static readonly string[] DisallowedMetadata = [
      "grid",
      "map ",
    ];

    private List<string> TestNoGridMetadata(YamlMappingNode root)
    {
        if (!root.TryGetNode<YamlSequenceNode>(Grids, out var grids))
            return ["No grids found"];

        if (!root.TryGetNode<YamlSequenceNode>(Entities, out var entities))
            return ["No entities found"];

        var gridIds = grids.Select(node => node.AsInt()).ToArray();
        var errors = new List<string>();

        foreach (var proto in entities)
        {
            // Skip unrelated entities
            if (string.IsNullOrEmpty(proto[Proto].AsString()))
                continue;

            foreach (var ent in (YamlSequenceNode)proto[Entities])
            {
                // Skip unrelated entities
                if (gridIds.Contains(ent[Uid].AsInt()))
                    continue;

                if (GetCompNode(ent, "Metadata") is not { } meta
                    || !meta.TryGetNode("name", out var name))
                {
                    errors.Add($"Grid {ent[Uid]} is missing a name");
                    continue;
                }

                if (!DisallowedMetadata.Any(x => name.ToString().StartsWith(x)))
                    continue;

                errors.Add($"Grid {ent[Uid]} has an improper name {name}");
            }
        }

        return errors;
    }
}
