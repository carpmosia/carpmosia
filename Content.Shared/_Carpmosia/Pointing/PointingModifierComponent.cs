using Robust.Shared.Prototypes;

namespace Content.Server._Carpmosia.Pointing.Components;

/// <summary>
/// Modifies the icon & phrase used in pointing when held in hand
/// </summary>
[RegisterComponent]
public sealed partial class PointingModifierComponent : Component
{
    /// <summary>
    /// Pointing arrow prototype
    /// </summary>
    [DataField]
    public EntProtoId Pointer = "PointerArrow";

    /// <summary>
    /// Verb shown in the popup to the pointer.
    /// e.g. "You POINT AT it".
    /// </summary>
    [DataField]
    public LocId PhraseSelf = "pointing-phrase-point-self";

    /// <summary>
    /// Verb shown in the popup to viewers.
    /// e.g. "The person POINTS AT it".
    /// </summary>
    [DataField]
    public LocId PhraseOther = "pointing-phrase-point-other";
}
