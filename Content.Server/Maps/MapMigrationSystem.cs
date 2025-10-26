using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Robust.Shared.ContentPack;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Markdown;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Serialization.Markdown.Value;
using Robust.Shared.Utility;

namespace Content.Server.Maps;

/// <summary>
///     Performs basic map migration operations by listening for engine <see cref="MapLoaderSystem"/> events.
/// </summary>
public sealed class MapMigrationSystem : EntitySystem
{
#pragma warning disable CS0414
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
#pragma warning restore CS0414
    [Dependency] private readonly IResourceManager _resMan = default!;

    // Carpmosia-start - Starlight migration system
    private static readonly string[] _migrationFiles =
    [
        "/migration.yml",
        "/_Carpmosia/migration.yml"
    ];
    // Carpmosia-end- Starlight migration system

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BeforeEntityReadEvent>(OnBeforeReadEvent);

#if DEBUG
        // Carpmosia-start - Starlight migration system
        foreach (var file in _migrationFiles)
            ValidateMigrations(file);
        // Carpmosia-end - Starlight migration system
#endif
    }

    private bool TryReadFile(string file, [NotNullWhen(true)] out MappingDataNode? mappings) // Carpmosia-edit - Starlight migration system
    {
        mappings = null;
        var path = new ResPath(file); // Carpmosia-edit - Starlight migration system
        if (!_resMan.TryContentFileRead(path, out var stream))
            return false;

        using var reader = new StreamReader(stream, EncodingHelpers.UTF8);
        var documents = DataNodeParser.ParseYamlStream(reader).FirstOrDefault();

        if (documents == null)
            return false;

        mappings = (MappingDataNode) documents.Root;
        return true;
    }

    private void OnBeforeReadEvent(BeforeEntityReadEvent ev)
    {
    // Carpmosia-start - Starlight migration system
        foreach (var file in _migrationFiles)
            ReadMigrations(ev, file);
    }

    private void ReadMigrations(BeforeEntityReadEvent ev, string file)
    {
    // Carpmosia-end - Starlight migration system
        if (!TryReadFile(file, out var mappings)) // Carpmosia-edit - Starlight migration system
            return;

        foreach (var (key, value) in mappings)
        {
            if (value is not ValueDataNode valueNode)
                continue;

            if (string.IsNullOrWhiteSpace(valueNode.Value) || valueNode.Value == "null")
                ev.DeletedPrototypes.Add(key);
            else
                ev.RenamedPrototypes.Add(key, valueNode.Value);
        }
    }

    // Carpmosia-start - Starlight migration system
    private void ValidateMigrations(string file)
    {
        if (!TryReadFile(file, out var mappings))
            return;

        // Verify that all of the entries map to valid entity prototypes.
        foreach (var node in mappings.Children.Values)
        {
            var newId = ((ValueDataNode)node).Value;
            if (!string.IsNullOrEmpty(newId) && newId != "null")
                DebugTools.Assert(_protoMan.HasIndex<EntityPrototype>(newId), $"{newId} is not an entity prototype.");
        }
    }
    // Carpmosia-end - Starlight migration system
}
