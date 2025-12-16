using Content.Server._Carpmosia.Objectives.Systems;

namespace Content.Server._Carpmosia.Objectives.Components;

[RegisterComponent, Access(typeof(SelfAndTargetEscapeShuttleConditionSystem))]
public sealed partial class SelfAndTargetEscapeShuttleConditionComponent : Component;
