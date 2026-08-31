#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Content.IntegrationTests.Fixtures;
using Content.IntegrationTests.Fixtures.Attributes;
using Content.IntegrationTests.Utility;
using Robust.Shared.ContentPack;
using Robust.Shared.GameObjects;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using YamlDotNet.RepresentationModel;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed partial class MapRulesTest : GameTest
{
    private static readonly string[] Exceptions = [
       "/Maps/_Carpmosia/Legacy/", // We ain't testing legacy ever
       // Maps pending fixes
       "/Maps/_Carpmosia/lampocteis.yml", // https://github.com/carpmosia/carpmosia/pull/603
       "/Maps/_Carpmosia/feint.yml",
       "/Maps/_Carpmosia/oasis.yml",
       "/Maps/_Carpmosia/packed.yml",
       "/Maps/_Carpmosia/saltern.yml",
       "/Maps/_Carpmosia/sparks.yml",
       // Shuttles gonna be fixed last
       "/Maps/_Carpmosia/Shuttles/",
    ];

    private static readonly ResPath[] TestScope = [.. GameDataScrounger.FilesInDirectoryInVfs("/Maps/_Carpmosia", "*.yml", true).Where(x => !Exceptions.Any(y => x.ToString().StartsWith(y)))];

    // Skip station specific tests on these maps
    private static readonly string[] NonStations = [
       "/Maps/_Carpmosia/Terminals/",
       "/Maps/_Carpmosia/Shuttles/",
       "/Maps/_Carpmosia/centcomm.yml",
    ];

    [SidedDependency(Side.Server)] private readonly IResourceManager _resMan = null!;
    [SidedDependency(Side.Server)] private readonly IComponentFactory _compFact = null!;

    private readonly record struct ParsedRoot(
        uint[] MapIds,
        uint[] GridIds,
        Dictionary<uint, EntProtoId> Tilemap,
        Dictionary<EntProtoId, Dictionary<uint, YamlSequenceNode>> Entities
    );

    [Test]
    [TestCaseSource(nameof(TestScope))]
    public void TestMapRules(ResPath map)
    {
        if (LoadYaml(map, _resMan) is not YamlMappingNode yamlRoot)
            return;

        // If any of these fail, you have a malformed map file
        // meta
        Assert.That(yamlRoot.TryGetNode<YamlSequenceNode>("maps", out var yamlMaps));
        Assert.That(yamlRoot.TryGetNode<YamlSequenceNode>("grids", out var yamlGrids));
        // orphans
        // nullspace
        Assert.That(yamlRoot.TryGetNode<YamlMappingNode>("tilemap", out var yamlTilemap));
        Assert.That(yamlRoot.TryGetNode<YamlSequenceNode>("entities", out var yamlEntities));

        var mapIds = yamlMaps!.Select(x => (uint)x.AsInt()).ToArray();
        var gridIds = yamlGrids!.Select(x => (uint)x.AsInt()).ToArray();
        var tilemap = yamlTilemap!.Select(x => ((uint)x.Key.AsInt(), (EntProtoId)x.Value.AsString())).ToDictionary();
        var entities = yamlEntities!
            .Select(x => ((EntProtoId)x["proto"].AsString(), (YamlSequenceNode)x["entities"]))
            .GroupBy(x => x.Item1)
            .ToDictionary(
                g => g.Key,
                g => g.SelectMany(x => x.Item2.ToDictionary(x => (uint)x["uid"].AsInt(), x => (YamlSequenceNode)x["components"])).ToDictionary()
            );

        ParsedRoot root = new(mapIds, gridIds, tilemap, entities);

        List<string> errors = [
          ..TestAnchorableDuplicates(root),
          ..TestMissingConnections(root),
          ..TestMissingLabels(root),
          ..TestNoCenteredGrid(root),
          ..TestNonWallmountsUnderWalls(root),
          ..TestMissingMapGridMetadata(root),
          ..TestTinyGrids(root),
          ..TestUnlinkedAtmosDevices(root),
        ];

        // Station specific tests
        if (!NonStations.Any(x => map.ToString().StartsWith(x)))
        {
            errors.AddRange([
                ..TestMandatoryStationEntities(root),
            ]);
        }

        // Assert one large list of errors instead of Assert.Multiple to avoid 5 morbillion stacktraces
        Assert.That(errors, Has.Count.EqualTo(0), $"Found {errors.Count} issues:\n{string.Join("\n", errors)}");
    }

    private static YamlNode? LoadYaml(ResPath map, IResourceManager resMan)
    {
        var rootedPath = map.ToRootedPath();
        if (!resMan.TryContentFileRead(rootedPath, out var fileStream))
        {
            Assert.Fail($"File not found: {rootedPath}");
            return null;
        }

        using var reader = new StreamReader(fileStream);
        var yamlStream = new YamlStream();
        yamlStream.Load(reader);

        return yamlStream.Documents[0].RootNode;
    }

    private YamlMappingNode? GetCompNode<T>(YamlSequenceNode comps) where T : IComponent, new()
    {
        if (comps.FirstOrDefault(x => x["type"].AsString() == _compFact.CompName<T>()) is not YamlMappingNode trans)
            return null;

        return trans;
    }

    private (EntityUid, Vector2i, int)? GetApproxTransform(YamlSequenceNode comps)
    {
        if (GetCompNode<TransformComponent>(comps) is not { } trans)
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

    private (EntityUid, Vector2i, int)? GetTilePosWithRot(YamlSequenceNode comps)
    {
        if (GetApproxTransform(comps) is not { } trans)
            return null;
        var (px, py) = trans.Item2;
        return (trans.Item1, ((int)Math.Floor(px / 10m), (int)Math.Floor(py / 10m)), trans.Item3);
    }

    private (EntityUid, Vector2i)? GetTilePos(YamlSequenceNode comps)
    {
        if (GetTilePosWithRot(comps) is not { } trans)
            return null;
        return (trans.Item1, trans.Item2);
    }

    private List<EntProtoId> GetPrototypeIds<T>() where T : IComponent, new()
    {
        return [.. Pair.GetPrototypesWithComponent<T>().Select(x => x.Item1.ID)];
    }

    private static Dictionary<uint, T> DeserializeCompNodes<T>(Dictionary<EntProtoId, Dictionary<uint, YamlSequenceNode>> entities, IEnumerable<EntProtoId> filter, Func<YamlSequenceNode, T?> deserializer) where T : struct
    {
        return entities
            .Where(x => filter.Contains(x.Key))
            .SelectMany(x => x.Value.Select(x => (x.Key, (T)deserializer(x.Value)!))).ToDictionary();
    }

    private float GetDistance((EntityUid, Vector2i) pos1, (EntityUid, Vector2i) pos2)
    {
        if (pos1.Item1 != pos2.Item1)
            return float.NaN;
        return (pos1.Item2 - pos2.Item2).Length;
    }
}
