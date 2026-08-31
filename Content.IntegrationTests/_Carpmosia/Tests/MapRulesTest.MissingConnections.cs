using System.Collections.Generic;
using Content.Server.Power.Components;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;

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

    /// <summary>
    /// Ensures that all APC's and wallmount substations are properly connected
    /// </summary>
    private List<string> TestMissingConnections(ParsedRoot root)
    {
        var apcs = GetPrototypeIds<ApcComponent>();

        var lvPos = DeserializeCompNodes(root.Entities, LVCables, GetTilePos);
        var mvPos = DeserializeCompNodes(root.Entities, MVCables, GetTilePos);
        var hvPos = DeserializeCompNodes(root.Entities, HVCables, GetTilePos);

        var errors = new List<string>();

        foreach (var (protoId, entities) in root.Entities)
        {
            var isApc = apcs.Contains(protoId);
            var isSub = WallmountSubstations.Contains(protoId);

            // Skip unrelated entities
            if (!isApc && !isSub)
                continue;

            foreach (var (uid, ent) in entities)
            {
                // Skip invalid transforms
                if (GetTilePosWithRot(ent) is not { } rawTrans)
                    continue;
                var trans = (rawTrans.Item1, rawTrans.Item2 + Angle.FromDegrees(rawTrans.Item3).GetDir().ToIntVec());

                if (isApc && !lvPos.ContainsValue(trans))
                    errors.Add($"Grid {trans.Item1} contains {protoId} ({uid}) that is missing an LV cable at {trans.Item2}");

                if (!mvPos.ContainsValue(trans))
                    errors.Add($"Grid {trans.Item1} contains {protoId} ({uid}) that is missing an MV cable at {trans.Item2}");

                if (isSub && !hvPos.ContainsValue(trans))
                    errors.Add($"Grid {trans.Item1} contains {protoId} ({uid}) that is missing an HV cable at {trans.Item2}");
            }
        }

        return errors;
    }
}
