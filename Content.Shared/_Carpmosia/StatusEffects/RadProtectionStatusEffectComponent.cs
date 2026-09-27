namespace Content.Shared._Offbrand.StatusEffects;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

/// <summary>
/// Component given to status effect entities that temporarily reduce the absorption of rads.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class RadProtectionStatusEffectComponent : Component
{
    /// <summary>
    /// How much damage these organs should receive.
    /// </summary>
    [DataField]
    public float Modifier = 0.5f;
}
