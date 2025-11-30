using Content.Server.Objectives.Systems;

namespace Content.Server.Objectives.Components;

/// <summary>
/// Requires that a target enters the 'dead' state at least once.
/// Depends on <see cref="TargetObjectiveComponent"/> to function.
/// </summary>
[RegisterComponent, Access(typeof(TeachPersonConditionSystem))]
public sealed partial class TeachPersonConditionComponent : Component
{

}
