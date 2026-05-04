using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Shared._Carpmosia.AreaOfEffect;

/// <summary>
/// Component that defines an area of effect (AoE) damage dealer.
/// The AoE damage dealer applies damage to entities within a certain radius,
/// optionally whitelisting or blacklisting certain entities.
/// It can also have a cooldown and a limited duration.
/// </summary>
[RegisterComponent]
public sealed partial class AreaOfEffectComponent : Component
{
     /// <summary>
     /// The radius, in tiles, around the entity within which damage is applied.
     /// </summary>
     [DataField]
     public float Radius = 5f;

     /// <summary>
     /// The damage to apply to entities within the AoE. The key is the damage type, and the value is the amount of damage to apply.
     /// </summary>
    [DataField]
    public Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2> Damage = new();

    /// <summary>
    /// Cooldown for how much time has to pass before AoE is applied again.
    /// </summary>
    [DataField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Optional duration for how long the area of effect lasts. If set, the component will be automatically removed after this time.
    /// </summary>
    [DataField]
    public TimeSpan? Duration;

    /// <summary>
    /// Whitelist of entities to damage. If set, only entities matching this whitelist will be damaged.
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// Blacklist of entities to exclude from damage. If set, entities matching this blacklist will not be damaged.
    /// </summary>
    [DataField]
    public EntityWhitelist? Blacklist;
}
