using System.Linq;
using Robust.Shared.Prototypes;

namespace Content.Shared._FarHorizons.Research;

[Prototype]
public sealed partial class ResearchTreePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;
    [DataField(required: true)]
    public HashSet<ProtoId<ResearchTreeNodePrototype>> Nodes = default!;

    public HashSet<ResearchTreeNodePrototype> GetNodes(IPrototypeManager protoMan) =>
        [.. Nodes.Select(p => protoMan.Index(p))];
    
    public HashSet<ResearchTreeTierPrototype> GetTiers(IPrototypeManager protoMan) =>
        [.. Nodes.Select(p => protoMan.Index(p).Tier).Distinct().Select(p => protoMan.Index(p))];
}