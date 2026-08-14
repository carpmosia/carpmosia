#nullable enable
using System.Collections.Generic;
using Content.IntegrationTests.Fixtures;
using Content.Server.Power.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using YamlDotNet.RepresentationModel;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed partial class MapRulesTest : GameTest
{

    private List<string> TestPowerNetworkLabels(YamlSequenceNode entities)
    {
        var batteries = GetPrototypeIds<PowerNetworkBatteryComponent>();

        var errors = new List<string>();

        foreach (var proto in entities)
        {
            EntProtoId protoId = proto[PROTO].AsString();

            // Skip unrelated entities
            if (!batteries.Contains(protoId))
                continue;

            foreach (var ent in (YamlSequenceNode)proto[ENTITIES])
            {
                // Skip invalid transforms
                if (GetTilePos(ent) is not { } trans)
                    continue;

                if (GetCompNode(ent, "Label") is { } label && (label.HasNode("currentLabel") || label.HasNode("localizedLabel")))
                    continue;

                errors.Add($"Grid {trans.Item1} contains {protoId} ({ent["uid"]}) that is missing a label at {trans.Item2}");
            }
        }

        return errors;
    }

}
