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
using Content.Server.Power.Components;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed partial class MappingGuidelineTest : GameTest
{
    private static readonly ResPath[] MapFiles = GameDataScrounger.FilesInDirectoryInVfs("/Maps/_Carpmosia", "*.yml", false);

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
        var tilePos = new Vector2(rawPos[0] + 0.5f, rawPos[1] + 0.5f).Floored();

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

        if (!root.TryGetNode<YamlSequenceNode>("entities", out var entities))
            return;

        // Collect all walls
        var wallProtos = Pair.GetPrototypesWithComponent<IsRoofComponent>().Select(x => x.Item1.ID).ToHashSet();

        // Collect all wallmount entities
        var wallmountProtos = Pair.GetPrototypesWithComponent<WallMountComponent>().Select(x => x.Item1.ID).ToHashSet();

        // Collect all apcs
        var apcProtos = Pair.GetPrototypesWithComponent<ApcComponent>().Select(x => x.Item1.ID).ToHashSet();

        // Collect all wall positions
        var wallPos = entities.Where(ent => wallProtos.Contains(ent["proto"].AsString()))
            .SelectMany(x => ((YamlSequenceNode)x["entities"]).Select(GetTilePos).OfType<(EntityUid, Vector2)>())
            .ToHashSet();

        // Collect all apc positions
        var apcPos = entities.Where(ent => apcProtos.Contains(ent["proto"].AsString()))
            .SelectMany(x => ((YamlSequenceNode)x["entities"]).Select(GetTilePos).OfType<(EntityUid, Vector2)>())
            .ToHashSet();

        using (Assert.EnterMultipleScope())
        {
            foreach (var proto in entities)
            {
                var protoId = proto["proto"].AsString();

                // Skip the walls themselves
                if (wallProtos.Contains(protoId))
                    continue;

                // Skip wallmount entities
                if (wallmountProtos.Contains(protoId))
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

                    Assert.That(!wallPos.Contains(trans),
                        $"Grid {trans.Item1} contains {ent["uid"]} ({protoId}) mapped under a wall at {trans.Item2}");
                }
            }
        }
    }
}
