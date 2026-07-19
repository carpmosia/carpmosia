using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Carpmosia.Supermatter;

[RegisterComponent]
public sealed partial class SupermatterComponent : Component
{
    [ViewVariables]
    public bool Stable = true;

    [ViewVariables]
    public bool Delamination = false;

    [ViewVariables]
    public float Integrity = 100f;

    [ViewVariables]
    public float MolesAbsorbed = 0f;

    [ViewVariables]
    public float StoredPower = 0f;

    // Multipliers

    public float WasteMultiplier = 1f;
    public float HeatProductionMultiplier = 1f;
    public float HeatPowerGainMultiplier = 1f;
    public float HeatProtectionMultiplier = 1f;
    public float IntegrityEffectMultiplier = 1f;
    public float PowerTransmissionMultiplier = 1f;
    public float PowerDecayMultiplier = 1f;
}
