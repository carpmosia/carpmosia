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
using System.Diagnostics.CodeAnalysis;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed partial class MappingGuidelineTest : GameTest
{
    private static readonly ResPath[] MapFiles = GameDataScrounger.FilesInDirectoryInVfs("/Maps/_Carpmosia", "*.yml", false);

    private static YamlNode? LoadMapYaml(ResPath map, IResourceManager resMan)
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

        return yamlStream.Documents[0].RootNode;
    }

    private static (EntityUid, Vector2)? GetTilePos(YamlNode entNode)
    {
        var ent = (YamlMappingNode)entNode;

        if (!ent.TryGetNode<YamlSequenceNode>("components", out var comps))
            return null;

        if (comps.First(x => x["type"].AsString() == "Transform") is not YamlMappingNode trans)
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

    [Test]
    [TestCaseSource(nameof(MapFiles))]
    [SuppressMessage("Assertion", "NUnit2014:Use SomeItemsConstraint for better assertion messages in case of failure")]
    public async Task MappedEntitiesUnderWallsTest(ResPath map)
    {
        var pair = Pair;
        var server = pair.Server;

        var resMan = server.ResolveDependency<IResourceManager>();
        var protoMan = server.ResolveDependency<IPrototypeManager>();

        if (LoadMapYaml(map, resMan) is not { } root)
            return;

        var entities = (YamlSequenceNode)root["entities"];

        // Collect all walls
        var wallProtos = Pair.GetPrototypesWithComponent<IsRoofComponent>().Select(x => x.Item1.ID).ToHashSet();

        // Collect all wallmount entities
        var wallmountProtos = Pair.GetPrototypesWithComponent<WallMountComponent>().Select(x => x.Item1.ID).ToHashSet();

        var wallPos = entities.Where(ent => wallProtos.Contains(ent["proto"].AsString()))
            .SelectMany(x => ((YamlSequenceNode)x["entities"]).Select(GetTilePos).OfType<(EntityUid, Vector2)>())
            .ToHashSet();

        //Console.WriteLine($"{wallPos.Count} {wallPos}");

        Assert.Multiple(() =>
        {
            foreach (var proto in entities)
            {
                var protoId = proto["proto"].AsString();

                // Skip allowed entities
                if (wallmountProtos.Contains(protoId))
                    continue;

                foreach (var ent in (YamlSequenceNode)proto["entities"])
                {
                    // Skip invalid transforms
                    if (GetTilePos(ent) is not { } trans)
                        continue;

                    Assert.That(!wallPos.Contains(trans),
                        $"\nMap {map} contains non-wallmount entity {protoId} ({ent["uid"]}) mapped under a wall ({trans})");
                }
            }
        });
    }
}
