using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server.Access.Components;

[RegisterComponent]
public sealed partial class PresetIdCardComponent : Component
{
    [DataField("job")]
    public ProtoId<JobPrototype>? JobName;

    // Carpmosia-start - alternative-job-titles
    [DataField("customTitle")]
    public LocId? CustomTitle;
    // Carpmosia-end - alternative-job-titles
    [DataField("name")]
    public string? IdName;
}
