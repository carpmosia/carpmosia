using Robust.Shared.GameStates;
using Content.Shared.Humanoid;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Prototypes;

namespace Content.Shared.Whistle;

/// <summary>
/// Spawn attached entity for entities in range with <see cref="HumanoidAppearanceComponent"/>.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class WhistleComponent : Component
{
    /// <summary>
    /// Entity prototype to spawn
    /// </summary>
    [DataField]
    public EntProtoId Effect = "WhistleExclamation";

    /// <summary>
    /// Range value.
    /// </summary>
    [DataField]
    public float Distance = 0;

    // Carpmosia-start - Whistle action
    /// <summary>
    /// Entity prototype for the whistling action
    /// </summary>
    [DataField]
    public EntProtoId ActionId = "ActionWhistle";

    [DataField]
    public EntityUid? Action;
    // Carpmosia-end - Whistle action
}
