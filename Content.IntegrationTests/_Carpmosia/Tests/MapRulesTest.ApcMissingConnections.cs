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
    private static readonly EntProtoId[] LVCables = ["CableApcExtension"];
    private static readonly EntProtoId[] MVCables = ["CableMV"];
    private static readonly EntProtoId[] HVCables = ["CableHV"];

    private List<string> TestApcMissingConnections(YamlSequenceNode entities)
    {
        var apcs = GetPrototypeIds<ApcComponent>();

        var lvPos = DeserializeCompNodes(entities, LVCables, GetTilePos);
        var mvPos = DeserializeCompNodes(entities, MVCables, GetTilePos);

        var errors = new List<string>();

        foreach (var proto in entities)
        {
            EntProtoId protoId = proto[PROTO].AsString();

            // Skip unrelated entities
            if (!apcs.Contains(protoId))
                continue;

            foreach (var ent in (YamlSequenceNode)proto[ENTITIES])
            {
                // Skip invalid transforms
                if (GetTilePos(ent) is not { } trans)
                    continue;

                if (!lvPos.Contains(trans))
                    errors.Add($"Grid {trans.Item1} contains {protoId} ({ent["uid"]}) that is missing an LV cable at {trans.Item2}");

                if (!mvPos.Contains(trans))
                    errors.Add($"Grid {trans.Item1} contains {protoId} ({ent["uid"]}) that is missing an MV cable at {trans.Item2}");
            }
        }

        return errors;
    }
}
