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

    private const string Proto = "proto";
    private const string Entities = "entities";

    [SidedDependency(Side.Server)] private readonly IResourceManager _resMan = null!;

    [Test]
    [TestCaseSource(nameof(TestScope))]
    public void TestMapRules(ResPath map)
    {
        if (LoadMapYaml(map, _resMan) is not { } root)
            return;

        if (!root.TryGetNode<YamlSequenceNode>(Entities, out var ents))
            return;

        List<string> errors = [
          ..TestNonWallmountsUnderWalls(ents),
          ..TestMissingConnections(ents),
          ..TestMissingLabels(ents),
          ..TestAnchorableDuplicates(ents),
          ..TestUnlinkedAtmosDevices(ents),
        ];

        // Station specific tests
        if (!NonStations.Any(x => map.ToString().StartsWith(x)))
        {
            errors.AddRange([
                //..TestMandatoryStationEntities(ents),
            ]);
        }

        // Assert one large list of errors instead of Assert.Multiple to avoid 5 morbillion stacktraces
        Assert.That(errors, Has.Count.EqualTo(0), $"Found {errors.Count} issues:\n{string.Join("\n", errors)}");
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

    private static (EntityUid, Vector2i, int)? GetApproxTransform(YamlNode entNode)
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

    private static (EntityUid, Vector2i, int)? GetTilePosWithRot(YamlNode entNode)
    {
        if (GetApproxTransform(entNode) is not { } trans)
            return null;
        var (px, py) = trans.Item2;
        return (trans.Item1, ((int)Math.Floor(px / 10m), (int)Math.Floor(py / 10m)), trans.Item3);
    }

    private static (EntityUid, Vector2i)? GetTilePos(YamlNode entNode)
    {
        if (GetTilePosWithRot(entNode) is not { } trans)
            return null;
        return (trans.Item1, trans.Item2);
    }

    private List<EntProtoId> GetPrototypeIds<T>() where T : IComponent, new()
    {
        return [.. Pair.GetPrototypesWithComponent<T>().Select(x => x.Item1.ID)];
    }

    private static List<T> DeserializeCompNodes<T>(YamlSequenceNode entities, IEnumerable<EntProtoId> filter, Func<YamlNode, T?> deserializer) where T : struct
    {
        return [..entities
            .Where(x => filter.Contains(x[Proto].AsString()))
            .SelectMany(x => ((YamlSequenceNode)x[Entities])
                .Select(deserializer)
                .OfType<T>()
            )
        ];
    }

    private float GetDistance((EntityUid, Vector2i) pos1, (EntityUid, Vector2i) pos2)
    {
        if (pos1.Item1 != pos2.Item1)
            return float.NaN;
        return (pos1.Item2 - pos2.Item2).Length;
    }
}
