using Content.Server.Objectives.Systems;

namespace Content.Server.Objectives.Components;

[RegisterComponent, Access(typeof(SelfAndTargetSurviveConditionSystem))]
public sealed partial class SelfAndTargetSurviveConditionComponent : Component;
