using Content.Server.StationEvents.Events;
using Content.Shared.Storage;
using Robust.Shared.Map; // Carpmosia-edit - Vent critter fix

namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(VentCrittersRule))]
public sealed partial class VentCrittersRuleComponent : Component
{
    [DataField("entries")]
    public List<EntitySpawnEntry> Entries = new();

    /// <summary>
    /// At least one special entry is guaranteed to spawn
    /// </summary>
    [DataField("specialEntries")]
    public List<EntitySpawnEntry> SpecialEntries = new();

    // Carpmosia-start - Vent critter fix
    [ViewVariables]
    public TransformComponent? Center;

    [ViewVariables]
    public List<string> Spawns = new();

    [ViewVariables]
    public List<EntityCoordinates> Locations = new();
    // Carpmosia-end - Vent critter fix
}
