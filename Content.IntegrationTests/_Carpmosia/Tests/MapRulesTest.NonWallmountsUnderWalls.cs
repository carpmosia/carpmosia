using System.Collections.Generic;
using Content.Server.Power.Components;
using Content.Shared.Light.Components;
using Content.Shared.Wall;
using Robust.Shared.Prototypes;

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

    /// <summary>
    /// Checks for any non-wallmount or not whitelisted entities under walls
    /// </summary>
    private List<string> TestNonWallmountsUnderWalls(ParsedRoot root)
    {
        var walls = GetPrototypeIds<IsRoofComponent>();
        var wallmounts = GetPrototypeIds<WallMountComponent>();
        var apcs = GetPrototypeIds<ApcComponent>();

        var wallPos = DeserializeCompNodes(root.Entities, walls, GetTilePos);
        var apcPos = DeserializeCompNodes(root.Entities, apcs, GetTilePos);
        var subPos = DeserializeCompNodes(root.Entities, Substations, GetTilePos);

        var errors = new List<string>();

        foreach (var (protoId, entities) in root.Entities)
        {
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

            foreach (var (uid, ent) in entities)
            {
                // Skip invalid transforms
                if (GetTilePos(ent) is not { } trans)
                    continue;

                // These are allowed to be mapped under a wall when an APC is present
                if (isApcCable && apcPos.ContainsValue(trans) || isSubCable && subPos.ContainsValue(trans))
                    continue;

                if (!wallPos.ContainsValue(trans))
                    continue;

                errors.Add($"Grid {trans.Item1} contains {protoId} ({uid}) mapped under a wall at tile {trans.Item2}");
            }
        }

        return errors;
    }
}
