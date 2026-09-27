using Content.Shared.Radio;
using Robust.Shared.Prototypes;

namespace Content.Shared._FarHorizons.Research.Components;

[RegisterComponent]
public sealed partial class FHResearchTreeComponent : Component
{

    [DataField(required: true)]
    public ProtoId<ResearchTreePrototype> Tree;

    [DataField]
    public HashSet<ProtoId<ResearchTreeNodePrototype>> Researched = [];

    [DataField]
    public Dictionary<ProtoId<ResearchTreeNodePrototype>, int> Progress = [];

    [DataField]
    public List<ProtoId<ResearchTreeNodePrototype>> Queue = [];

    [DataField]
    public int BankedPoints = 0;
    [DataField]
    public int BankCapacity = 15000;
    [DataField]
    public int PointBleed = 1000;

    [DataField]
    public int MaxQueueSize = 3;
    [DataField]
    public List<ProtoId<RadioChannelPrototype>> AnnounceTo = [];

    [DataField]
    public TimeSpan WarningFrequency = TimeSpan.Zero;

    [DataField]
    public List<ProtoId<ResearchTreeUnlockFlagPrototype>> UnlockFlags = [];

    [ViewVariables]
    public TimeSpan NextWarning = TimeSpan.Zero;

    [ViewVariables]
    public TimeSpan NextUpdate = TimeSpan.Zero;
    [DataField]
    public TimeSpan RefreshRate = TimeSpan.FromSeconds(1);
}

