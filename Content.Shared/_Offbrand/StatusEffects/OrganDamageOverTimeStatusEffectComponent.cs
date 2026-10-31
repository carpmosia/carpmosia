namespace Content.Shared._Offbrand.StatusEffects;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
[Access(typeof(OrganDamageOverTimeStatusEffectSystem))]
public sealed partial class OrganDamageOverTimeStatusEffectComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan NextUpdate;

    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Which organs should receive damage.
    /// </summary>
    [DataField(required: true)]
    public string[] Categories;

    /// <summary>
    /// How much damage these organs should receive.
    /// </summary>
    [DataField]
    public float Amount = 0.5f;
}
