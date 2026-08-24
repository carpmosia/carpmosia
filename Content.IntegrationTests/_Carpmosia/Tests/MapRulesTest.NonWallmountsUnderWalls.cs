using System.Collections.Generic;
using Content.Server.Power.Components;
using Content.Shared.Light.Components;
using Content.Shared.Wall;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using YamlDotNet.RepresentationModel;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed partial class MapRulesTest
{
    private static readonly EntProtoId[] WallmountWhitelist = [
        "RandomPosterAny",
        "RandomPosterContraband",
        "RandomPosterLegit",
        "RandomPainting",
        "PlaqueAtmos",
    ];

    // Substations don't have a unique component sadly
    private static readonly EntProtoId[] Substations = [
        "SubstationBasic",
        "SubstationBasicEmpty",
        "SubstationWallBasic",
    ];

    private List<string> TestNonWallmountsUnderWalls(YamlSequenceNode entities)
    {
        var walls = GetPrototypeIds<IsRoofComponent>();
        var wallmounts = GetPrototypeIds<WallMountComponent>();
        var apcs = GetPrototypeIds<ApcComponent>();

        var wallPos = DeserializeCompNodes(entities, walls, GetTilePos);
        var apcPos = DeserializeCompNodes(entities, apcs, GetTilePos);
        var subPos = DeserializeCompNodes(entities, Substations, GetTilePos);

        var errors = new List<string>();

        foreach (var proto in entities)
        {
            EntProtoId protoId = proto[Proto].AsString();

            // Skip the walls themselves
            if (walls.Contains(protoId))
                continue;

            // Skip wallmount entities
            if (wallmounts.Contains(protoId))
                continue;

            // Skip whitelisted entities
            if (WallmountWhitelist.Contains(protoId))
                continue;

            var isApcCable = LVCables.Contains(protoId) || MVCables.Contains(protoId);
            var isSubCable = MVCables.Contains(protoId) || HVCables.Contains(protoId);

            foreach (var ent in (YamlSequenceNode)proto[Entities])
            {
                // Skip invalid transforms
                if (GetTilePos(ent) is not { } trans)
                    continue;

                // These are allowed to be mapped under a wall when an APC is present
                if (isApcCable && apcPos.Contains(trans) || isSubCable && subPos.Contains(trans))
                    continue;

                if (!wallPos.Contains(trans))
                    continue;

                errors.Add($"Grid {trans.Item1} contains {protoId} ({ent["uid"]}) mapped under a wall at tile {trans.Item2}");
            }
        }

        return errors;
    }
}
