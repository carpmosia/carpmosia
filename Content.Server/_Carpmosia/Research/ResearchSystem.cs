using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Chat.Systems;
using Content.Server.Radio.EntitySystems;
using Content.Server.Research.Systems;
using Content.Shared._FarHorizons.Research;
using Content.Shared._FarHorizons.Research.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Emag.Systems;
using Content.Shared.Research.Components;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._FarHorizons.Research;

public sealed partial class FHResearchSystem : EntitySystem
{
    [Dependency] private IPrototypeManager _protoMan = default!;
    [Dependency] private UserInterfaceSystem _ui = default!;
    [Dependency] private ResearchSystem _research = default!;
    [Dependency] private AudioSystem _audio = default!;
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private RadioSystem _radio = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private AccessReaderSystem _accessReader = default!;
    [Dependency] private EmagSystem _emag = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeConsole();
    }

    public bool TryGetServerWithTree(Entity<ResearchClientComponent?> ent, [NotNullWhen(true)] out Entity<FHResearchTreeComponent>? server)
    {   
        server = null;
        if (Resolve(ent, ref ent.Comp) && 
            _research.TryGetClientServer(ent, out var serverEnt, out _) && 
            TryComp(serverEnt, out FHResearchTreeComponent? treeComp))
        {
            server = (serverEnt.Value, treeComp);
            return true;
        }
        return false;
    }

    public void TrickFullResearch(EntityUid target, FHResearchTreeComponent component)
    {
        Entity<FHResearchTreeComponent> serverEnt = (target, component);
        HashSet<ProtoId<ResearchTreeNodePrototype>> nodes =
            [
                .. GetTreeNodes(serverEnt).Where(p => !component.Researched.Contains(p.ID))
                    .Select(p => (ProtoId<ResearchTreeNodePrototype>)p.ID)
            ];
        foreach (var nodeProtoId in nodes)
            UnlockNode(serverEnt, nodeProtoId, sendAnnouncement: false);
    }
}