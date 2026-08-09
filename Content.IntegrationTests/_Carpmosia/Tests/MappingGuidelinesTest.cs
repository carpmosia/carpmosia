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
using Robust.Shared.GameObjects;
using Robust.Shared.Maths;
using Content.Server.Power.Components;
using System.Collections.Generic;
using NUnit.Framework.Internal;
using Content.Shared.Construction.Components;
using Content.Server.Atmos.Monitor.Components;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed partial class MappingGuidelinesTest : GameTest
{
    private static readonly ResPath[] AllMapFiles = GameDataScrounger.FilesInDirectoryInVfs("/Maps/_Carpmosia/Terminals", "*.yml", false);
    //private static readonly ResPath[] AllMapFiles = [.. GameDataScrounger.FilesInDirectoryInVfs("/Maps/_Carpmosia", "*.yml", true).Where(x => !x.ToString().StartsWith("/Maps/_Carpmosia/Legacy/"))];
    private static readonly ResPath[] StationMaps = [.. GameDataScrounger.FilesInDirectoryInVfs("/Maps/_Carpmosia", "*.yml", false).Where(x => !x.ToString().StartsWith("/Maps/_Carpmosia/centcomm.yml"))];

    private static readonly EntProtoId LVCable = "CableApcExtension";
    private static readonly EntProtoId MVCable = "CableMV";
    private static readonly EntProtoId HVCable = "CableHV";

    private static readonly EntProtoId[] WallmountWhitelist = [
        "RandomPosterAny",
        "RandomPosterContraband",
        "RandomPosterLegit",
        "RandomPainting",
        "PlaqueAtmos"
    ];

    // Substations don't have a unique component sadly
    private static readonly EntProtoId[] Substations = [
        "SubstationBasic",
        "SubstationBasicEmpty",
        "SubstationWallBasic",
        "SubstationWallBasicEmpty",
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

        var wallPos = GetComponents(entities, walls.Contains, GetTilePos).ToHashSet();
        var apcPos = GetComponents(entities, apcs.Contains, GetTilePos).ToHashSet();
        var subPos = GetComponents(entities, Substations.Contains, GetTilePos).ToHashSet();

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

            var isApcCable = protoId == LVCable || protoId == MVCable;
            var isSubCable = protoId == MVCable || protoId == HVCable;

            foreach (var ent in (YamlSequenceNode)proto["entities"])
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

        var lvPos = GetComponents(entities, x => x == LVCable, GetTilePos).ToHashSet();
        var mvPos = GetComponents(entities, x => x == MVCable, GetTilePos).ToHashSet();

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

                errors.Add($"Grid {trans.Item1} contains {protoId} ({ent["uid"]}) that is missing a label at {trans.Item2}");
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
            foreach (var ((grid, (x, y), rot), count) in GetComponents(entities, x => x == proto, GetApproxTransform)
                .GroupBy(x => x).Where(x => x.Count() > 1).Select(x => (x.Key, x.Count())))
            {
                errors.Add($"Grid {grid} contains {count} duplicate {proto} at <{x / 10}, {y / 10}>");
            }
        }

        Assert.That(!errors.Any(), $"Found {errors.Count} anchorable duplicates:\n{string.Join("\n", errors)}");
    }

    [Test]
    [TestCaseSource(nameof(AllMapFiles))]
    public async Task TestUnlinkedAtmosDevices(ResPath map)
    {
        var resMan = Pair.Server.ResolveDependency<IResourceManager>();

        if (LoadMapYaml(map, resMan) is not { } root)
            return;

        if (!root.TryGetNode<YamlSequenceNode>("entities", out var entities))
            return;

        var airAlarms = GetPrototypeIds<AirAlarmComponent>();
        var atmosMonitors = GetPrototypeIds<AtmosMonitorComponent>();

        var errors = new List<string>();

        foreach (var proto in entities)
        {
            var protoId = proto["proto"].AsString();

            var isAirAlarm = airAlarms.Contains(protoId);
            var isAtmosMonitor = atmosMonitors.Contains(protoId);

            // Skip unrelated entities
            if (!(isAirAlarm || isAtmosMonitor))
                continue;

            foreach (var ent in (YamlSequenceNode)proto["entities"])
            {
                // Skip invalid transforms
                if (GetTilePos(ent) is not { } trans)
                    continue;

                if (isAirAlarm && GetComp(ent, "DeviceList") is { })
                    continue;

                if (isAtmosMonitor && GetComp(ent, "DeviceNetwork") is { })
                    continue;

                errors.Add($"Grid {trans.Item1} contains {protoId} ({ent["uid"]}) that doesn't have any connections at {trans.Item2}");
            }
        }

        Assert.That(!errors.Any(), $"Found {errors.Count} unlinked atmos devices:\n{string.Join("\n", errors)}");
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

        if (comps.FirstOrDefault(x => x["type"].AsString() == comp) is not YamlMappingNode trans)
            return null;

        return trans;
    }

    private static (EntityUid, (int, int), int)? GetApproxTransform(YamlNode entNode)
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
        var pos = ((int)Math.Floor(rawPos[0] * 10), (int)Math.Floor(rawPos[1] * 10));

        var rot = 0;
        if (trans.TryGetNode("rot", out var rotRaw))
        {
            rot = (int)Math.Round(MathHelper.RadiansToDegrees(double.Parse(rotRaw.AsString().Split(" rad").First())));
        }

        return (parent, pos, rot);
    }

    private static (EntityUid, (int, int))? GetTilePos(YamlNode entNode)
    {
        if (GetApproxTransform(entNode) is not { } trans)
            return null;
        var parent = trans.Item1;
        var (px, py) = trans.Item2;
        return (parent, ((int)Math.Floor(px / 10f), (int)Math.Floor(py / 10f)));
    }

    private HashSet<EntProtoId> GetPrototypeIds<T>() where T : IComponent, new()
    {
        return [.. Pair.GetPrototypesWithComponent<T>().Select(x => x.Item1.ID)];
    }

    private static List<T> GetComponents<T>(YamlSequenceNode entities, Func<EntProtoId, bool> filter, Func<YamlNode, T?> select) where T : struct
    {
        return [.. entities.Where(x => filter(x["proto"].AsString())).SelectMany(x => ((YamlSequenceNode)x["entities"]).Select(select).OfType<T>())];
    }
}
