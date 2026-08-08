#nullable enable
using System.IO;
using Content.IntegrationTests.Fixtures;
using Content.IntegrationTests.Utility;
using YamlDotNet.RepresentationModel;
using Robust.Shared.ContentPack;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System.Linq;
using Content.Shared.Wall;
using Content.Shared.Light.Components;
using System.Numerics;
using Robust.Shared.GameObjects;
using Robust.Shared.Maths;
using Content.Server.Power.Components;
using System.Collections.Generic;
using NUnit.Framework.Internal;
using Content.Shared.Construction.Components;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed partial class MappingGuidelinesTest : GameTest
{
    private static readonly ResPath[] AllMapFiles = [.. GameDataScrounger.FilesInDirectoryInVfs("/Maps/_Carpmosia", "*.yml", true).Where(x => !x.ToString().StartsWith("/Maps/_Carpmosia/Legacy/"))];
    private static readonly ResPath[] StationMaps = [.. GameDataScrounger.FilesInDirectoryInVfs("/Maps/_Carpmosia", "*.yml", false).Where(x => !x.ToString().StartsWith("/Maps/_Carpmosia/centcomm.yml"))];

    private static readonly EntProtoId[] WallmountWhitelist = [
        "RandomPosterAny",
        "RandomPosterContraband",
        "RandomPosterLegit",
        "RandomPainting",
        "PlaqueAtmos"
    ];

    [Test]
    [TestCaseSource(nameof(AllMapFiles))]
    public async Task TestNonWallmountEntitiesUnderWalls(ResPath map)
    {
        var resMan = Pair.Server.ResolveDependency<IResourceManager>();
        var protoMan = Pair.Server.ResolveDependency<IPrototypeManager>();

        if (LoadMapYaml(map, resMan) is not { } root)
            return;

        if (!root.TryGetNode<YamlSequenceNode>("entities", out var entities))
            return;

        var walls = GetPrototypeIds<IsRoofComponent>();
        var wallmounts = GetPrototypeIds<WallMountComponent>();
        var apcs = GetPrototypeIds<ApcComponent>();

        var wallPos = GetEntityPositions(entities, walls.Contains).ToHashSet();
        var apcPos = GetEntityPositions(entities, apcs.Contains).ToHashSet();

        var errors = new List<string>();

        foreach (var proto in entities)
        {
            var protoId = proto["proto"].AsString();

            // Skip the walls themselves
            if (walls.Contains(protoId))
                continue;

            // Skip wallmount entities
            if (wallmounts.Contains(protoId))
                continue;

            // Skip whitelisted entities
            if (WallmountWhitelist.Contains(protoId))
                continue;

            var isApcCable = protoId == "CableApcExtension" || protoId == "CableMV";

            foreach (var ent in (YamlSequenceNode)proto["entities"])
            {
                // Skip invalid transforms
                if (GetTilePos(ent) is not { } trans)
                    continue;

                // These are allowed to be mapped under a wall when an APC is present
                if (isApcCable && apcPos.Contains(trans))
                    continue;

                if (!wallPos.Contains(trans))
                    continue;

                errors.Add($"Grid {trans.Item1} contains {protoId} ({ent["uid"]}) mapped under a wall at tile {trans.Item2}");
            }
        }

        Assert.That(!errors.Any(), $"Found {errors.Count} entities mapped under walls:\n{string.Join("\n", errors)}");
    }

    [Test]
    [TestCaseSource(nameof(AllMapFiles))]
    public async Task TestApcMissingConnections(ResPath map)
    {
        var resMan = Pair.Server.ResolveDependency<IResourceManager>();
        var protoMan = Pair.Server.ResolveDependency<IPrototypeManager>();

        if (LoadMapYaml(map, resMan) is not { } root)
            return;

        if (!root.TryGetNode<YamlSequenceNode>("entities", out var entities))
            return;

        var apcs = GetPrototypeIds<ApcComponent>();
        var lvPos = GetEntityPositions(entities, x => x == "CableApcExtension").ToHashSet();
        var mvPos = GetEntityPositions(entities, x => x == "CableMV").ToHashSet();

        var errors = new List<string>();

        foreach (var proto in entities)
        {
            var protoId = proto["proto"].AsString();

            // Skip unrelated entities
            if (!apcs.Contains(protoId))
                continue;

            foreach (var ent in (YamlSequenceNode)proto["entities"])
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

        Assert.That(!errors.Any(), $"Found {errors.Count} apcs missing connections:\n{string.Join("\n", errors)}");
    }

    [Test]
    [TestCaseSource(nameof(AllMapFiles))]
    public async Task TestPowerNetworkLabels(ResPath map)
    {
        var resMan = Pair.Server.ResolveDependency<IResourceManager>();
        var protoMan = Pair.Server.ResolveDependency<IPrototypeManager>();

        if (LoadMapYaml(map, resMan) is not { } root)
            return;

        if (!root.TryGetNode<YamlSequenceNode>("entities", out var entities))
            return;

        var batteries = GetPrototypeIds<PowerNetworkBatteryComponent>();

        var errors = new List<string>();

        foreach (var proto in entities)
        {
            var protoId = proto["proto"].AsString();

            // Skip unrelated entities
            if (!batteries.Contains(protoId))
                continue;

            foreach (var ent in (YamlSequenceNode)proto["entities"])
            {
                // Skip invalid transforms
                if (GetTilePos(ent) is not { } trans)
                    continue;

                if (GetComp(ent, "Label") is { } label && (label.HasNode("currentLabel") || label.HasNode("localizedLabel")))
                    continue;

                errors.Add($"Grid {trans.Item1} contains {protoId} ({ent["uid"]}) that is missing a label {trans.Item2}");
            }
        }

        Assert.That(!errors.Any(), $"Found {errors.Count} power network members missing labels:\n{string.Join("\n", errors)}");
    }

    [Test]
    [TestCaseSource(nameof(AllMapFiles))]
    public async Task TestAnchorableDuplicates(ResPath map)
    {
        var resMan = Pair.Server.ResolveDependency<IResourceManager>();

        if (LoadMapYaml(map, resMan) is not { } root)
            return;

        if (!root.TryGetNode<YamlSequenceNode>("entities", out var entities))
            return;

        var anchorables = GetPrototypeIds<AnchorableComponent>();

        var errors = new List<string>();

        foreach (var proto in anchorables)
        {
            foreach (var ((grid, pos), count) in GetEntityPositions(entities, x => x == proto)
                .GroupBy(x => x).Where(x => x.Count() > 1).Select(x => (x.Key, x.Count())))
            {
                errors.Add($"Grid {grid} contains {count} duplicate {proto} at {pos}");
            }
        }

        Assert.That(!errors.Any(), $"Found {errors.Count} anchorable duplicates:\n{string.Join("\n", errors)}");
    }

    private static YamlMappingNode? LoadMapYaml(ResPath map, IResourceManager resMan)
    {
        var rootedPath = map.ToRootedPath();
        if (!resMan.TryContentFileRead(rootedPath, out var fileStream))
        {
            Assert.Fail($"Map not found: {rootedPath}");
            return null;
        }

        using var reader = new StreamReader(fileStream);
        var yamlStream = new YamlStream();
        yamlStream.Load(reader);

        return (YamlMappingNode)yamlStream.Documents[0].RootNode;
    }

    private static YamlMappingNode? GetComp(YamlNode entNode, string comp)
    {
        var ent = (YamlMappingNode)entNode;

        if (!ent.TryGetNode<YamlSequenceNode>("components", out var comps))
            return null;

        if (comps.First(x => x["type"].AsString() == comp) is not YamlMappingNode trans)
            return null;

        return trans;
    }

    private static (EntityUid, Vector2)? GetTilePos(YamlNode entNode)
    {
        if (GetComp(entNode, "Transform") is not { } trans)
            return null;

        if (!trans.TryGetNode("parent", out var rawParent))
            return null;

        if (rawParent.ToString() == "invalid")
            return null;

        var parent = new EntityUid(rawParent.AsInt());

        if (!trans.TryGetNode("pos", out var posRaw))
            return null;

        var rawPos = posRaw.AsString().Split(",").Select(float.Parse).ToArray();
        var tilePos = new Vector2(rawPos[0], rawPos[1]).Floored();

        return (parent, tilePos);
    }

    private HashSet<string> GetPrototypeIds<T>() where T : IComponent, new()
    {
        return [.. Pair.GetPrototypesWithComponent<T>().Select(x => x.Item1.ID)];
    }

    private static List<(EntityUid, Vector2)> GetEntityPositions(YamlSequenceNode entities, Func<string, bool> filter)
    {
        return [.. entities.Where(x => filter(x["proto"].AsString())).SelectMany(x => ((YamlSequenceNode)x["entities"]).Select(GetTilePos).OfType<(EntityUid, Vector2)>())];
    }
}
