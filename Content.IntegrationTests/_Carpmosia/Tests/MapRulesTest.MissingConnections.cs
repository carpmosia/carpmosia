using System.Collections.Generic;
using Content.Server.Power.Components;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using YamlDotNet.RepresentationModel;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed partial class MapRulesTest
{
    private static readonly EntProtoId[] LVCables = ["CableApcExtension"];
    private static readonly EntProtoId[] MVCables = ["CableMV"];
    private static readonly EntProtoId[] HVCables = ["CableHV"];

    private static readonly EntProtoId[] WallmountSubstations = [
        "SubstationWallBasic",
        "BaseSubstationWall"
    ];

    private List<string> TestMissingConnections(YamlSequenceNode entities)
    {
        var apcs = GetPrototypeIds<ApcComponent>();

        var lvPos = DeserializeCompNodes(entities, LVCables, GetTilePos);
        var mvPos = DeserializeCompNodes(entities, MVCables, GetTilePos);
        var hvPos = DeserializeCompNodes(entities, HVCables, GetTilePos);

        var errors = new List<string>();

        foreach (var proto in entities)
        {
            EntProtoId protoId = proto[Proto].AsString();

            var isApc = apcs.Contains(protoId);
            var isSub = WallmountSubstations.Contains(protoId);

            // Skip unrelated entities
            if (!isApc && !isSub)
                continue;

            foreach (var ent in (YamlSequenceNode)proto["entities"])
            {
                // Skip invalid transforms
                if (GetTilePosWithRot(ent) is not { } rawTrans)
                    continue;
                var trans = (rawTrans.Item1, rawTrans.Item2 + Angle.FromDegrees(rawTrans.Item3).GetDir().ToIntVec());

                if (isApc && !lvPos.Contains(trans))
                    errors.Add($"Grid {trans.Item1} contains {protoId} ({ent["uid"]}) that is missing an LV cable at {trans.Item2}");

                if (!mvPos.Contains(trans))
                    errors.Add($"Grid {trans.Item1} contains {protoId} ({ent["uid"]}) that is missing an MV cable at {trans.Item2}");

                if (isSub && !hvPos.Contains(trans))
                    errors.Add($"Grid {trans.Item1} contains {protoId} ({ent["uid"]}) that is missing an HV cable at {trans.Item2}");
            }
        }

        return errors;
    }
}
