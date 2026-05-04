using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Shared._Carpmosia.AreaOfEffect;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
[AutoGenerateComponentState]
public sealed partial class AreaOfEffectComponent : Component
{
    [DataField]
    public Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2> Damage = new();

    [DataField]
    public float Cooldown = 1f;

    /// <summary>
    /// The radius, in tiles, around the entity within which damage is applied.
    /// </summary>
    [DataField]
    public float Radius = 5f;

    /// <summary>
    /// The time when the next damage application should occur.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float NextApplicationTime;

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
