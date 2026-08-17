using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Offbrand.Analyzers;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
[Access(typeof(AnalyzerSystem))]
public sealed partial class AnalyzerComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField, AutoNetworkedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(1);

    [DataField, AutoNetworkedField]
    public bool IsUpdating;

    [DataField, AutoNetworkedField]
    public bool ShouldUpdate;

    [DataField, AutoNetworkedField]
    public EntityUid? Target;

    [DataField, AutoNetworkedField]
    public EntityUid? ActualTarget;

    [DataField, AutoNetworkedField]
    public float? ScanRange = 3f;

    [DataField]
    public bool AutoRelink = true;
}

[ByRefEvent]
public readonly record struct AnalyzerUpdatedEvent(EntityUid Target);

[ByRefEvent]
public record struct AnalyzerActualTargetEvent(EntityUid Target, EntityUid? ActualTarget);

[ByRefEvent]
public record struct AnalyzerActualTargetUpdatedEvent;

[ByRefEvent]
public readonly record struct AfterAnalyzerUpdatedEvent;
