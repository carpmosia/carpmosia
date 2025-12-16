using Content.Server._Carpmosia.Objectives.Systems;

namespace Content.Server._Carpmosia.Objectives.Components;

[RegisterComponent, Access(typeof(SelfAndTargetSurviveConditionSystem))]
public sealed partial class SelfAndTargetSurviveConditionComponent : Component;
