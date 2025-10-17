using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array; // Carpmosia-edit - massive loadout rework

namespace Content.Shared.Preferences.Loadouts;

/// <summary>
/// Corresponds to a set of loadouts for a particular slot.
/// </summary>
[Prototype]
public sealed partial class LoadoutGroupPrototype : IPrototype, IInheritingPrototype // Carpmosia-edit - massive loadout rework
{
    [IdDataField]
    public string ID { get; private set; } = string.Empty;

    // Carpmosia-start - massive loadout rework

    /// <inheritdoc />
    [ParentDataFieldAttribute(typeof(AbstractPrototypeIdArraySerializer<LoadoutGroupPrototype>))]
    public string[]? Parents { get; }

    /// <inheritdoc />
    [NeverPushInheritance]
    [AbstractDataField]
    public bool Abstract { get; }
    // Carpmosia-end - massive loadout rework

    /// <summary>
    /// User-friendly name for the group.
    /// </summary>
    [DataField(required: true)]
    public LocId Name;

    /// <summary>
    /// Minimum number of loadouts that need to be specified for this category.
    /// </summary>
    [DataField]
    public int MinLimit = 1;

    // Carpmosia-start - massive loadout rework

    /// <summary>
    /// Number of loadouts that are selected by default.
    /// </summary>
    [DataField]
    public int DefaultSelected = 0;
    // Carpmosia-end - massive loadout rework

    /// <summary>
    /// Maximum limit for the category.
    /// </summary>
    [DataField]
    public int MaxLimit = 1;

    /// <summary>
    /// Hides the loadout group from the player.
    /// </summary>
    [DataField]
    public bool Hidden;

    [AlwaysPushInheritance] // Carpmosia-edit - massive loadout rework
    [DataField(required: true)]
    public List<ProtoId<LoadoutPrototype>> Loadouts = new();
}
