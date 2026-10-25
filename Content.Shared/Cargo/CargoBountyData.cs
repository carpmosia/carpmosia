// Carpmosia-rework - Cargo bookkeeping
using Robust.Shared.Serialization;
using Content.Shared.Cargo.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared.Cargo;

/// <summary>
/// A data structure for storing currently available bounties.
/// </summary>
[DataDefinition, NetSerializable, Serializable]
public sealed partial class CargoBountyData
{
    /// <summary>
    /// A unique id used to identify the bounty
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public string Id { get; private set; } = string.Empty;

    /// <summary>
    /// The prototype containing information about the bounty.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField(required: true)]
    public ProtoId<CargoBountyPrototype> Bounty = string.Empty;

    /// <summary>
    /// NetEntity that has claimed this bounty
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public NetEntity? ClaimedBy;

    /// <summary>
    /// Used for catching the display name for UI
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public string? ClaimedByName;

    /// <summary>
    /// Current bounty status set by claimer
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public CargoBountyStatus Status = CargoBountyStatus.Undelivered;

    public CargoBountyData(CargoBountyPrototype bounty, int uniqueIdentifier)
    {
        Bounty = bounty.ID;
        Id = $"{bounty.IdPrefix}{uniqueIdentifier:D3}";
    }

    public enum CargoBountyStatus
    {
        Undelivered,
        Delivered,
        Ready,
    }
}
