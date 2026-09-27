using System.Linq;
using Content.Shared._FarHorizons.Research;
using Content.Shared._FarHorizons.Research.Components;
using Content.Shared.Research.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client._FarHorizons.Research.UI;

[UsedImplicitly]
public sealed partial class FHResearchConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [Dependency] private IPrototypeManager _protoMan = default!;

    private HashSet<ResearchTreeNodePrototype> _nodeProtos = [];
    private HashSet<ProtoId<ResearchTreeNodePrototype>> _unlockedNodes = [];
    private HashSet<ProtoId<ResearchTreeNodePrototype>> _researchedNodes = [];
    private HashSet<ProtoId<ResearchTreeTierPrototype>> _unlockedTiers = [];
    private List<ProtoId<ResearchTreeNodePrototype>> _queuedNodes = [];
    private bool _readonly = false;

    [ViewVariables]
    private FHResearchConsoleWindow? _window;

    protected override void Open()
    {
        base.Open();
        
        _nodeProtos = [.. _protoMan.EnumeratePrototypes<ResearchTreeNodePrototype>()];
        _window = this.CreateWindow<FHResearchConsoleWindow>();
        _window.Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName;

        _window.OnServerButtonPressed += () => SendMessage(new ConsoleServerSelectionMessage());

        if (!_readonly)
        {
            _window.OnResearchButtonPressed += SendReseachRequest;
            _window.OnRemoveQueueButtonPressed += SendRemoveFromQueueRequest;
            _window.OnQuickResearch += QuickResearchRequest;
        }
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is FHResearchConsoleBUIFullState fullState)
        {
            _nodeProtos = [.. fullState.Nodes.Select(p => _protoMan.Index(p))];
            _researchedNodes = fullState.ResearchedNodes;
            _unlockedTiers = fullState.UnlockedTiers;
            _unlockedNodes = fullState.UnlockedNodes;
            _queuedNodes = fullState.QueuedNodes;
            _readonly = fullState.Readonly;

            if (_readonly && _window != null)
            {
                _window.OnResearchButtonPressed -= SendReseachRequest;
                _window.OnRemoveQueueButtonPressed -= SendRemoveFromQueueRequest;
                _window.OnQuickResearch -= QuickResearchRequest;
            }
            
            _window?.SetupUI(_nodeProtos, _unlockedTiers, _unlockedNodes, _researchedNodes, _queuedNodes, fullState.ResearchProgress, fullState.BankedPoints, fullState.Readonly);
        } else if (state is FHResearchConsoleBUIPartialState partialState)
        {
            _researchedNodes = partialState.ResearchedNodes;
            _unlockedTiers = partialState.UnlockedTiers;
            _unlockedNodes = partialState.UnlockedNodes;
            _queuedNodes = partialState.QueuedNodes;
            
            _window?.RefreshUI(_unlockedTiers, _unlockedNodes, _researchedNodes, _queuedNodes, partialState.ResearchProgress, partialState.BankedPoints);
        }
    }

    private void QuickResearchRequest(ProtoId<ResearchTreeNodePrototype> node)
    {
        if (_queuedNodes.Contains(node))
            SendRemoveFromQueueRequest(node);
        else if (_unlockedNodes.Contains(node))
            SendReseachRequest(node);
    }

    private void SendReseachRequest(ProtoId<ResearchTreeNodePrototype> node)
    {
        if (!_readonly)
            SendMessage(new FHResearchConsoleResearchRequest(node));
    }

    private void SendRemoveFromQueueRequest(ProtoId<ResearchTreeNodePrototype> node)
    {
        if (!_readonly)
            SendMessage(new FHResearchConsoleRemoveQueueRequest(node));
    }
}