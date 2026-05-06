using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._Carpmosia.Medical.Components;

/// <summary>
/// Modifies bleeding stacks on a target with a do-after, at the cost of power.
/// <see cref="HealingComponent">
/// </summary>
[RegisterComponent]
public sealed partial class PowerCauteryComponent : Component
{
    /// <summary>
    /// The bleeding stacks to remove.
    /// </summary>
    [DataField]
    public float BloodlossModifier = -5.0f;

    /// <remarks>
    /// The supported damage types are specified using a <see cref="DamageContainerPrototype"/>s. For a
    /// HealingComponent this filters what damage container type this component should work on. If null,
    /// all damage container types are supported.
    /// </remarks>
    [DataField]
    public List<ProtoId<DamageContainerPrototype>>? DamageContainers;

    /// <summary>
    /// How long it takes to cauterize the bleeding.
    /// </summary>
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(2f);

    /// <summary>
    /// Delay multiplier when cauterizing yourself.
    /// </summary>
    [DataField]
    public float SelfHealPenaltyMultiplier = 2f;

    /// <summary>
    /// Amount of power, in watts, to draw per complete doafter.
    /// </summary>
    [DataField]
    public float PowerDraw = 20f;

    /// <summary>
    /// Sound played on doafter start.
    /// </summary>
    [DataField]
    public SoundSpecifier? BeginSound = null;

    /// <summary>
    /// Sound played on doafter end.
    /// </summary>
    [DataField]
    public SoundSpecifier? EndSound = null;
}
