#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Content.IntegrationTests.Fixtures.Attributes;
using Content.IntegrationTests.Fixtures;
using Content.Server.Atmos.Monitor.Components;
using Content.Server.Power.Components;
using Content.Shared.Construction.Components;
using Content.Shared.Light.Components;
using Content.Shared.Wall;
using Robust.Shared.ContentPack;
using Robust.Shared.GameObjects;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using YamlDotNet.RepresentationModel;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed partial class MappingGuidelinesTest : GameTest
{
    // Temporary override until most of the maps are fixed
    private static readonly ResPath[] AllMapFiles = [
        //new("/Maps/_Carpmosia/Terminals/donk_rest_stop.yml"),
        // new("/Maps/_Carpmosia/amber.yml"),
        // new("/Maps/_Carpmosia/centcomm.yml"),
        // new("/Maps/_Carpmosia/feint.yml"),
        // new("/Maps/_Carpmosia/lampocteis.yml"),
        // new("/Maps/_Carpmosia/oasis.yml"),
        // new("/Maps/_Carpmosia/packed.yml"),
        // new("/Maps/_Carpmosia/saltern.yml"),
        // new("/Maps/_Carpmosia/sparks.yml")
    ];
    //private static readonly ResPath[] AllMapFiles = [.. GameDataScrounger.FilesInDirectoryInVfs("/Maps/_Carpmosia", "*.yml", true).Where(x => !x.ToString().StartsWith("/Maps/_Carpmosia/Legacy/"))];
    //private static readonly ResPath[] StationMaps = [.. GameDataScrounger.FilesInDirectoryInVfs("/Maps/_Carpmosia", "*.yml", false).Where(x => !x.ToString().StartsWith("/Maps/_Carpmosia/centcomm.yml"))];

    private static readonly EntProtoId[] LVCables = ["CableApcExtension"];
    private static readonly EntProtoId[] MVCables = ["CableMV"];
    private static readonly EntProtoId[] HVCables = ["CableHV"];

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

    private const string PROTO = "proto";
    private const string ENTITIES = "entities";

    [SidedDependency(Side.Server)] private readonly IResourceManager _resMan = null!;

    [Test]
    [TestCaseSource(nameof(AllMapFiles))]
    public void TestMappingGuidelines(ResPath map)
    {
        if (LoadMapYaml(map, _resMan) is not { } root)
            return;

        if (!root.TryGetNode<YamlSequenceNode>(ENTITIES, out var ents))
            return;

        List<string> errors = [
            ..TestNonWallmountEntitiesUnderWalls(ents),
            ..TestApcMissingConnections(ents),
            ..TestPowerNetworkLabels(ents),
            ..TestAnchorableDuplicates(ents),
            ..TestUnlinkedAtmosDevices(ents),
        ];

        // Assert one large list of errors instead of Assert.Multiple to avoid 5 morbillion stacktraces
        Assert.That(errors, Has.Count.EqualTo(0), $"Found {errors.Count} issues:\n{string.Join("\n", errors)}");
    }

    private List<string> TestNonWallmountEntitiesUnderWalls(YamlSequenceNode entities)
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
            EntProtoId protoId = proto[PROTO].AsString();

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

            foreach (var ent in (YamlSequenceNode)proto[ENTITIES])
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

    private List<string> TestAnchorableDuplicates(YamlSequenceNode entities)
    {
        var anchorables = GetPrototypeIds<AnchorableComponent>();

        var errors = new List<string>();

        foreach (var proto in anchorables)
        {
            foreach (var ((grid, (x, y), _), count) in DeserializeCompNodes(entities, [proto], GetApproxTransform)
                .GroupBy(x => x).Where(x => x.Count() > 1).Select(x => (x.Key, x.Count())))
            {
                errors.Add($"Grid {grid} contains {count} duplicate {proto} at <{x / 10}, {y / 10}>");
            }
        }

        return errors;
    }

    private List<string> TestUnlinkedAtmosDevices(YamlSequenceNode entities)
    {
        var airAlarms = GetPrototypeIds<AirAlarmComponent>();
        var atmosMonitors = GetPrototypeIds<AtmosMonitorComponent>();

        var errors = new List<string>();

        foreach (var proto in entities)
        {
            EntProtoId protoId = proto[PROTO].AsString();

            var isAirAlarm = airAlarms.Contains(protoId);
            var isAtmosMonitor = atmosMonitors.Contains(protoId);

            // Skip unrelated entities
            if (!(isAirAlarm || isAtmosMonitor))
                continue;

            foreach (var ent in (YamlSequenceNode)proto[ENTITIES])
            {
                // Skip invalid transforms
                if (GetTilePos(ent) is not { } trans)
                    continue;

                if (isAirAlarm && GetCompNode(ent, "DeviceList") is { })
                    continue;

                if (isAtmosMonitor && GetCompNode(ent, "DeviceNetwork") is { })
                    continue;

                errors.Add($"Grid {trans.Item1} contains {protoId} ({ent["uid"]}) that doesn't have any connections at {trans.Item2}");
            }
        }

        return errors;
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

    private static YamlMappingNode? GetCompNode(YamlNode entNode, string comp)
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
        if (GetCompNode(entNode, "Transform") is not { } trans)
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
        return (parent, ((int)Math.Floor(px / 10m), (int)Math.Floor(py / 10m)));
    }

    private List<EntProtoId> GetPrototypeIds<T>() where T : IComponent, new()
    {
        return [.. Pair.GetPrototypesWithComponent<T>().Select(x => x.Item1.ID)];
    }

    private static List<T> DeserializeCompNodes<T>(YamlSequenceNode entities, IEnumerable<EntProtoId> filter, Func<YamlNode, T?> deserializer) where T : struct
    {
        return [..entities
            .Where(x => filter.Contains(x[PROTO].AsString()))
            .SelectMany(x => ((YamlSequenceNode)x[ENTITIES])
                .Select(deserializer)
                .OfType<T>()
            )
        ];
    }
}
