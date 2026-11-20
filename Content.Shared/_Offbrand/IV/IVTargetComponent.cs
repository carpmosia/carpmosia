using Content.Shared._Offbrand.Wounds;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Offbrand.IV;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(IVSystem))]
public sealed partial class IVTargetComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? IVSource;

    [DataField, AutoNetworkedField]
    public string? IVJointID;

    [DataField]
    public EntityWhitelist? PermissibleContainers;

    [DataField]
    public EntProtoId<WoundComponent> RipOutWound = "WoundArterialBleeding";
}
