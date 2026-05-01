using Content.Shared.Actions;
using Content.Shared.Movement.Systems;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._Carpmosia.Movement.Components;

/// <summary>
/// Component a status effect on action, at the cost of stamina damage, if applicable.
/// </summary>
/// <remarks>
/// The datafields are set as "sane defaults" which happens to be exactly what we want for a species. How convenient
/// </remarks>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SharedJumpAbilitySystem))]
public sealed partial class SprintAbilityComponent : Component
{
    /// <summary>
    /// The action prototype that allows you to jump.
    /// </summary>
    [DataField]
    public EntProtoId Action = "ActionLagomorphSprint"; // ha.

    /// <summary>
    /// Entity to hold the action prototype.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;

    /// <summary>
    /// Entity effect prototype to apply to the user.
    /// </summary>
    /// <remarks>
    /// This could trivially be a List<EntProtoId> but IMO it's cleaner to
    /// use one status entity w/ multiple effect components.
    /// Sure you can make multiple generic statuses but you're not gonna reuse them?
    /// And a status supports multiple effects, why not use that feature?
    /// </remarks>
    [DataField]
    public EntProtoId StatusEffect = "StatusEffectLagomorphSprint"; // ha.

    /// <summary>
    /// Stamina damage cost.
    /// </summary>
    [DataField]
    public float StaminaCost = 35f;

    /// <summary>
    /// How long the speed boost should last.
    /// </summary>
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(10);

    /// <summary>
    /// This gets played whenever the jump action is used.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier? SprintSound;

    /// <summary>
    /// The popup to show if the entity is unable to sprint.
    /// </summary>
    [DataField]
    public LocId? SprintFailedPopup = "sprint-ability-failure";
}

public sealed partial class SprintAbilityEvent : InstantActionEvent;

