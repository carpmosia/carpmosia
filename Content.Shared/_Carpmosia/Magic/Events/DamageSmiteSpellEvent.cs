using Content.Shared.Actions;
using Content.Shared.FixedPoint;

namespace Content.Shared.Magic.Events;

public sealed partial class DamageSmiteSpellEvent : EntityTargetActionEvent
{
    /// <summary>
    /// Damage dealt by default in case we fail to find the threshold
    /// </summary>
    public FixedPoint2 Damage = 100;

    /// <summary>
    /// Damage type dealt by the spell
    /// </summary>
    [DataField("damageType")]
    public string DamageType { get; set; } = "Cellular";
}
