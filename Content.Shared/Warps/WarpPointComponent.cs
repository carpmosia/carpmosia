using Robust.Shared.GameStates;

namespace Content.Shared.Warps;

/// <summary>
/// Allows ghosts etc to warp to this entity by name.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class WarpPointComponent : Component
{
    [DataField]
    public LocId? Location;

    // Carpmosia-start - Warp point prefixes
    /// <summary>
    /// Origin of this warp point.
    /// </summary>
    public string Origin = "Unknown";
    // Carpmosia-end - Warp point prefixes

    /// <summary>
    /// If true, ghosts warping to this entity will begin following it.
    /// </summary>
    [DataField]
    public bool Follow;
}
