using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Carpmosia.Supermatter;

[NetworkedComponent]
[AutoGenerateComponentState]
[RegisterComponent]
public sealed partial class SupermatterComponent : Component
{
    [DataField]
    public float IntegrityRegeneration = 1f; // Per second

    [ViewVariables]
    [DataField, AutoNetworkedField]
    public bool Active = false;

    [ViewVariables]
    [DataField, AutoNetworkedField]
    public float Integrity = 100f;

    [ViewVariables]
    public float MolesAbsorbed = 0f;

    [ViewVariables]
    public float StoredPower = 0f;

    [ViewVariables]
    public float DelaminationTime = 0f;

    // Multipliers

    public float WasteMultiplier = 1f;
    public float HeatProductionMultiplier = 1f;
    public float HeatPowerGainMultiplier = 1f;
    public float HeatProtectionMultiplier = 1f;
    public float IntegrityEffectMultiplier = 1f;
    public float PowerTransmissionMultiplier = 1f;
    public float PowerDecayMultiplier = 1f;
}
