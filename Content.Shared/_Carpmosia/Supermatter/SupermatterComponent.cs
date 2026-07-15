using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Carpmosia.Supermatter;

[Serializable, NetSerializable]
[Access(typeof(SupermatterSystem))]
[RegisterComponent]
public sealed partial class SupermatterComponent : Component
{
    [ViewVariables]
    public bool Stable = true;

    [ViewVariables]
    public bool Delamitaion = false;

    [ViewVariables]
    public float Integrity = 100f;

    [ViewVariables]
    public float MolesAbsorbed = 0f;

    [ViewVariables]
    public float WasteMultiplier = 1f;

    [ViewVariables]
    public float StoredPower = 0f;
}
