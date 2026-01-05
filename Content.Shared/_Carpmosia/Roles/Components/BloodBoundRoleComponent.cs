using Robust.Shared.GameStates;

namespace Content.Shared.Roles.Components;

/// <summary>
/// Added to mind role entities to tag that they are a blood bound.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BloodBoundRoleComponent : BaseMindRoleComponent
{
    [DataField, AutoNetworkedField]
    public EntityUid? Bound;
}
