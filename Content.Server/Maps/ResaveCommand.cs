using System.Linq;
using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.Maps; // Carpmosia-edit - Resave variantize
using Robust.Shared.Console;
using Robust.Shared.ContentPack;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Components;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map; // Carpmosia-edit - Resave variantize
using Robust.Shared.Map.Components; // Carpmosia-edit - Resave variantize
using Robust.Shared.Utility;

namespace Content.Server.Maps;

/// <summary>
/// Loads every map and resaves it into the data folder.
/// </summary>
[AdminCommand(AdminFlags.Host)]
public sealed partial class ResaveCommand : LocalizedCommands
{
    [Dependency] private IEntityManager _entManager = default!;
    [Dependency] private IResourceManager _res = default!;
    [Dependency] private ILogManager _log = default!;

    public override string Command => "resave";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var loader = _entManager.System<MapLoaderSystem>();

        var opts = MapLoadOptions.Default with
        {

            DeserializationOptions = DeserializationOptions.Default with
            {
                StoreYamlUids = true,
                LogOrphanedGrids = false
            }
        };

        // Carpmosia-start - Resave variantize
        var variantize = args.Length > 0 && args[0] == "true";
        var mapsSystem = _entManager.System<SharedMapSystem>();
        var tileSystem = _entManager.System<TileSystem>();
        var turfSystem = _entManager.System<TurfSystem>();
        // Carpmosia-end - Resave variantize

        var log = _log.GetSawmill(Command);
        var files = _res.ContentFindFiles(new ResPath("/Maps/_Carpmosia/")).ToList(); // Carpmosia-edit - Mapping

        for (var i = 0; i < files.Count; i++)
        {
            var fn = files[i];
            log.Info($"Re-saving file {i}/{files.Count} : {fn}");

            if (!loader.TryLoadGeneric(fn, out var result, opts))
                continue;

            if (result.Maps.Count != 1)
            {
                shell.WriteError(
                    $"Multi-map or multi-grid files like {fn} are not yet supported by the {Command} command");
                loader.Delete(result);
                continue;
            }

            // Carpmosia-start - Resave variantize
            if (variantize)
            {
                foreach (var grid in result.Grids)
                {
                    if (!_entManager.TryGetComponent(grid, out MapGridComponent? gridComp))
                    {
                        shell.WriteError($"Euid '{grid}' does not exist or is not a grid.");
                        return;
                    }

                    foreach (var tile in mapsSystem.GetAllTiles(grid, gridComp))
                    {
                        var def = turfSystem.GetContentTileDefinition(tile);
                        var newTile = new Tile(tile.Tile.TypeId, tile.Tile.Flags, tileSystem.PickVariant(def), tile.Tile.RotationMirroring);
                        mapsSystem.SetTile(grid, gridComp, tile.GridIndices, newTile);
                    }
                }
            }
            // Carpmosia-end - Resave variantize

            var map = result.Maps.First();

            // Process deferred component removals.
            _entManager.CullRemovedComponents();

            if (_entManager.HasComponent<LoadedMapComponent>(map))
            {
                loader.TrySaveMap(map.Comp.MapId, fn);
            }
            else if (result.Grids.Count == 1)
            {
                loader.TrySaveGrid(result.Grids.First(), fn);
            }
            else
            {
                shell.WriteError($"Failed to resave {fn}");
            }

            loader.Delete(result);
        }

        shell.WriteLine($"Resaved all maps");
    }
}
