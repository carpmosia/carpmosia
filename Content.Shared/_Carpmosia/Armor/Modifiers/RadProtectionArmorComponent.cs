using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Carpmosia.Armor.Modifiers;
/// <summary>
/// Component given to equippable items that reduces the abosption of radiation.
/// </summary>

[RegisterComponent, NetworkedComponent]
public sealed partial class RadProtectionArmorComponent : Component
{
    /// <summary>
    /// How much this should modify the amount of rads received
    /// </summary>
    [DataField]
    public float Modifier = 0.1f;
}
