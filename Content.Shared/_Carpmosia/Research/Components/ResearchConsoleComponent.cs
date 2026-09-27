using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._FarHorizons.Research.Components;

[RegisterComponent]
public sealed partial class FHResearchConsoleComponent : Component
{
    [DataField]
    public SoundSpecifier ErrorSound = new SoundPathSpecifier("/Audio/Machines/custom_deny.ogg");

    [DataField]
    public bool Readonly = false;
}

[NetSerializable, Serializable]
public enum FHResearchConsoleUiKey
{
    Key,
}

[NetSerializable, Serializable]
public sealed class FHResearchConsoleBUIFullState(
    HashSet<ProtoId<ResearchTreeNodePrototype>> nodes, 
    HashSet<ProtoId<ResearchTreeTierPrototype>> unlockedTiers,
    HashSet<ProtoId<ResearchTreeNodePrototype>> unlockedNodes,
    HashSet<ProtoId<ResearchTreeNodePrototype>> researchedNodes,
    List<ProtoId<ResearchTreeNodePrototype>> queuedNodes,
    Dictionary<ProtoId<ResearchTreeNodePrototype>, int> researchProgress,
    int bankedPoints,
    bool readonlyConsole
    ) : BoundUserInterfaceState
{
    public HashSet<ProtoId<ResearchTreeNodePrototype>> Nodes = nodes;
    public HashSet<ProtoId<ResearchTreeTierPrototype>> UnlockedTiers = unlockedTiers;
    public HashSet<ProtoId<ResearchTreeNodePrototype>> UnlockedNodes = unlockedNodes;
    public HashSet<ProtoId<ResearchTreeNodePrototype>> ResearchedNodes = researchedNodes;
    public List<ProtoId<ResearchTreeNodePrototype>> QueuedNodes = queuedNodes;
    public Dictionary<ProtoId<ResearchTreeNodePrototype>, int> ResearchProgress = researchProgress;
    public int BankedPoints = bankedPoints;
    public bool Readonly = readonlyConsole;
}

[NetSerializable, Serializable]
public sealed class FHResearchConsoleBUIPartialState(
    HashSet<ProtoId<ResearchTreeTierPrototype>> unlockedTiers,
    HashSet<ProtoId<ResearchTreeNodePrototype>> unlockedNodes,
    HashSet<ProtoId<ResearchTreeNodePrototype>> researchedNodes,
    List<ProtoId<ResearchTreeNodePrototype>> queuedNodes,
    Dictionary<ProtoId<ResearchTreeNodePrototype>, int> researchProgress,
    int bankedPoints
    ) : BoundUserInterfaceState
{
    public HashSet<ProtoId<ResearchTreeTierPrototype>> UnlockedTiers = unlockedTiers;
    public HashSet<ProtoId<ResearchTreeNodePrototype>> UnlockedNodes = unlockedNodes;
    public HashSet<ProtoId<ResearchTreeNodePrototype>> ResearchedNodes = researchedNodes;
    public List<ProtoId<ResearchTreeNodePrototype>> QueuedNodes = queuedNodes;
    public Dictionary<ProtoId<ResearchTreeNodePrototype>, int> ResearchProgress = researchProgress;
    public int BankedPoints = bankedPoints;
}

[Serializable, NetSerializable]
public sealed class FHResearchConsoleResearchRequest(ProtoId<ResearchTreeNodePrototype> node) : BoundUserInterfaceMessage
{
    public ProtoId<ResearchTreeNodePrototype> Node = node;
}

[Serializable, NetSerializable]
public sealed class FHResearchConsoleRemoveQueueRequest(ProtoId<ResearchTreeNodePrototype> node) : BoundUserInterfaceMessage
{
    public ProtoId<ResearchTreeNodePrototype> Node = node;
}