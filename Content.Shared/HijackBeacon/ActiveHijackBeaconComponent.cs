using Robust.Shared.GameStates;

namespace Content.Shared.HijackBeacon;

/// <summary>
/// This is used for tracking a <see cref="HijackBeaconComponent"/> that is currently activated or on cooldown.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ActiveHijackBeaconComponent : Component
{
    /// <summary>
    ///     Remaining time until the hijack is completed.
    /// </summary>
    [DataField, AutoNetworkedField, Access(typeof(HijackBeaconSystem))]
    public TimeSpan CompletionTime = TimeSpan.Zero;


    // Carpmosia-start - ATS hijack rework
    /// <summary>
    ///     Efficiency of the hijack beacon, more adjacent objects reduces its efficiency.
    /// </summary>
    [DataField, AutoNetworkedField, Access(typeof(HijackBeaconSystem))]
    public double Efficiency = 1;
    // Carpmosia-end - ATS hijack rework
}
