using System.Collections.Generic;
using System.Linq;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using YamlDotNet.RepresentationModel;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed partial class MapRulesTest
{
    private static readonly ResPath MandatoryEntities = new("_Carpmosia/mandatory_entities.yml");
    private const float Threshold = 10f;

    private List<string> TestMandatoryStationEntities(ParsedRoot root)
    {
        if (LoadYaml(MandatoryEntities, _resMan) is not YamlSequenceNode rules)
            return [$"Could not load '{MandatoryEntities}'"];

        var errors = new List<string>();

        foreach (var test in rules.Cast<YamlMappingNode>())
        {
            var poiGroups = test.GetNode<YamlSequenceNode>("pois")
                .Select(x => x is YamlSequenceNode seq ? seq.Select(x => (EntProtoId)x.AsString()) : [(EntProtoId)x.AsString()]);
            var entGroups = test.GetNode<YamlSequenceNode>("ents")
                .Select(x => x is YamlSequenceNode seq ? seq.Select(x => (EntProtoId)x.AsString()) : [(EntProtoId)x.AsString()]);

            var poiIds = poiGroups.SelectMany(x => x);

            var poi = DeserializeCompNodes(root.Entities, poiIds, GetTilePos).Values;

            foreach (var poiGroup in poiGroups)
            {
                if (root.Entities.Any(x => poiGroup.Contains(x.Key)))
                    continue;

                errors.Add($"Could not find any of [{string.Join(", ", poiGroup)}] on the map");
            }

            foreach (var entGroup in entGroups)
            {
                var eoi = DeserializeCompNodes(root.Entities, entGroup, GetTilePos).Values;
                if (poi.Any(pos1 => eoi.Any(pos2 => GetDistance(pos1, pos2) <= Threshold)))
                    continue;

                errors.Add($"Could not find any of [{string.Join(", ", entGroup)}] near any of [{string.Join(", ", poiIds)}]");
            }
        }

        return errors;
    }
}
