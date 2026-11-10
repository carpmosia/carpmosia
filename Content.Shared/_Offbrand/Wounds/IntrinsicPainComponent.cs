using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Content.Shared._Offbrand.StatusEffects;

namespace Content.Shared._Offbrand.Wounds;

[RegisterComponent, NetworkedComponent]
[Access(typeof(IntrinsicPainSystem), typeof(PainModifyingStatusEffectSystem))]
public sealed partial class IntrinsicPainComponent : Component
{
    /// <summary>
    /// Coefficients for damage to pain
    /// </summary>
    [DataField(required: true)]
    public Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2> PainCoefficients;

    /// <summary>
    /// Used for things like status effects or other non-damage and indirect sources of pain.
    /// </summary>
    [DataField]
    public FixedPoint2 FlatPainModifier = FixedPoint2.Zero;
}
