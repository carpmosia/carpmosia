using Content.Server.Objectives.Systems;

namespace Content.Server.Objectives.Components;

[RegisterComponent, Access(typeof(SelfAndTargetEscapeShuttleConditionSystem))]
public sealed partial class SelfAndTargetEscapeShuttleConditionComponent : Component;
