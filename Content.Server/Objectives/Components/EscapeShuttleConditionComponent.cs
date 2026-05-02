using Content.Server.Objectives.Systems;

namespace Content.Server.Objectives.Components;

/// <summary>
/// Requires that the player is on the emergency shuttle's grid when docking to CentCom.
/// </summary>
[RegisterComponent, Access(typeof(EscapeShuttleConditionSystem))]
public sealed partial class EscapeShuttleConditionComponent : Component
{
    // Carpmosia-start - escape restrained
    /// <summary>
    /// Count as full completion if you're handcuffed on evac
    /// </summary>
    [DataField]
    public bool AllowRestrained = false;
    // Carpmosia-end - escape restrained
}
