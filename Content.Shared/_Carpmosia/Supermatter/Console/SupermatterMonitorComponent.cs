using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Cargo.Components;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class SupermatterMonitorComponent : Component
{

}

[NetSerializable, Serializable]
public sealed class SupermatterMonitorConsoleState : BoundUserInterfaceState
{
    public List<CargoBountyData> Bounties;
    public List<CargoBountyHistoryData> History;
    public TimeSpan UntilNextSkip;

    public SupermatterMonitorConsoleState(List<CargoBountyData> bounties,
        List<CargoBountyHistoryData> history,
        TimeSpan untilNextSkip)
    {
        Bounties = bounties;
        History = history;
        UntilNextSkip = untilNextSkip;
    }
}
