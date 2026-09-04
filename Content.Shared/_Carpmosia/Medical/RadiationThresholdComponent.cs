using Content.Shared.FixedPoint;
using Content.Shared.Mobs;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Medical;

/// <summary>
/// Component for the simple surgical tool used for brain extraction.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(RadiationSicknessSystem))]
public sealed partial class RadiationThresholdsComponent : Component
{
    /// <summary>
    /// The status effects to apply depending on the amount of rads. Highest threshold is selected.
    /// </summary>
    [DataField(required: true)]
    public SortedDictionary<FixedPoint2, EntProtoId> Thresholds;

    [DataField, AutoNetworkedField]
    public EntProtoId? CurrentThresholdState;

    [DataField]
    public FixedPoint2 Rads = 0f;

}
