using Robust.Shared.Prototypes;

namespace Content.Shared._Carpmosia.Supermatter;

[Prototype]
public sealed partial class SupermatterGasEffectPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    // How much plasma will supermatter generate.
    [DataField]
    public float WasteMultiplier = 1f;

    // Lower = more power goes into producing lightning bolts.
    // Higher = more power goes into heating the surrounding atmosphere.
    [DataField]
    public float HeatProductionMultiplier = 1f;

    // How much internal energy the SM will get from heat.
    // Does not affect the heat damage.
    [DataField]
    public float HeatPowerGainMultiplier = 1f;

    // Modifies the amount of heat damage the SM will receive
    [DataField]
    public float HeatProtectionMultiplier = 1f;

    // Lowers the overall damage.
    [DataField]
    public float IntegrityEffectMultiplier = 1f;

    // The amount of internal power SM will spent on lightning bolts/heat
    [DataField]
    public float PowerTransmissionMultiplier = 1f;

    // SM always loses some of its power to radiation.
    [DataField]
    public float PowerDecayMultiplier = 1f;
}
