using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using Robust.Shared.Map.Components;
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
    private List<string> TestTinyGrids(ParsedRoot root)
    {
        var space = root.Tilemap.Where(x => x.Value == "Space").Select(x => x.Key).ToArray();
        var errors = new List<string>();

        foreach (var (uid, ent) in root.Entities[""].Where(x => root.GridIds.Contains(x.Key)))
        {
            if (GetCompNode<MapGridComponent>(ent) is not { } mapGrid || !mapGrid.TryGetNode<YamlMappingNode>("chunks", out var chunks))
                continue;

            var count = chunks.Sum(x =>
                Convert.FromBase64String(x.Value["tiles"].AsString())
                    .Chunk(7)
                    .Select(x => BinaryPrimitives.ReadUInt32LittleEndian(x.Take(4).ToArray()))
                    .Count(x => space.Contains(x))
            );

            if (count <= TinyGridThreshold)
                errors.Add($"Grid {uid} only has {count} tiles, which is very likely an accident.");
        }

        return errors;
    }

}
