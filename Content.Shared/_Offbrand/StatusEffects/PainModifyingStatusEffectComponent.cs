using Content.Shared.FixedPoint;

namespace Content.Shared._Offbrand.StatusEffects;

[RegisterComponent]
[Access(typeof(PainModifyingStatusEffectSystem))]
public sealed partial class PainModifyingStatusEffectComponent : Component
{
    /// <summary>
    /// The amount of pain caused by the effect.
    /// </summary>
    [DataField(required: true)]
    public FixedPoint2 Amount;
}
