using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using Robust.Shared.Utility;
using YamlDotNet.RepresentationModel;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed partial class MapRulesTest
{
    // Evac pods are 12
    private const int TinyGridThreshold = 10;

    /// <summary>
    /// Checks for presence of any extremely tiny grids, as those are 99.9% accidental.
    /// </summary>
    private List<string> TestTinyGrids(YamlMappingNode root)
    {
        if (!root.TryGetNode<YamlSequenceNode>(Grids, out var grids))
            return ["No 'grids' entry found"];
        if (!root.TryGetNode<YamlMappingNode>(Tilemap, out var tilemap))
            return ["No 'tilemap' entry found"];
        if (!root.TryGetNode<YamlSequenceNode>(Entities, out var entities))
            return ["No 'entities' entry found"];

        var targets = grids.Select(node => node.AsInt());
        var space = tilemap.First(x => x.Value.AsString() == "Space").Key.AsInt();
        var errors = new List<string>();

        foreach (var proto in entities)
        {
            // Skip unrelated entities
            if (!string.IsNullOrEmpty(proto[Proto].AsString()))
                continue;

            foreach (var ent in (YamlSequenceNode)proto[Entities])
            {
                // Skip unrelated entities
                if (!targets.Contains(ent[Uid].AsInt()))
                    continue;

                if (GetCompNode(ent, "MapGrid") is not { } mapGrid || !mapGrid.TryGetNode<YamlMappingNode>("chunks", out var chunks))
                    continue;

                var count = chunks.Sum(x =>
                    Convert.FromBase64String(x.Value["tiles"].AsString())
                        .Chunk(7)
                        .Select(x => BinaryPrimitives.ReadUInt32LittleEndian(x.Take(4).ToArray()))
                        .Count(x => x != space)
                );

                if (count <= TinyGridThreshold)
                    errors.Add($"Grid {ent[Uid]} only has {count} tiles, which is very likely an accident.");
            }
        }

        return errors;
    }

}
