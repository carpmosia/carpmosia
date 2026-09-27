using System.Linq;
using Content.Shared._FarHorizons.Research;
using Content.Shared._FarHorizons.Research.Components;
using Content.Shared.Chat;
using Content.Shared.Research.Components;
using Content.Shared.UserInterface;
using Robust.Shared.Prototypes;
using Content.Server.Power.EntitySystems;
using Content.Shared.Access.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.IdentityManagement;

namespace Content.Server._FarHorizons.Research;

public sealed partial class FHResearchSystem
{
    private void InitializeConsole()
    {
        SubscribeLocalEvent<FHResearchConsoleComponent, FHResearchConsoleResearchRequest>(OnResearchRequest);
        SubscribeLocalEvent<FHResearchConsoleComponent, FHResearchConsoleRemoveQueueRequest>(OnRemoveQueueRequest);
        SubscribeLocalEvent<FHResearchConsoleComponent, BeforeActivatableUIOpenEvent>(OnConsoleBeforeUiOpened);
        SubscribeLocalEvent<FHResearchConsoleComponent, ResearchRegistrationChangedEvent>(OnConsoleRegistrationChanged);
        SubscribeLocalEvent<FHResearchConsoleComponent, GotEmaggedEvent>(OnEmagged);
    }

    private void OnRemoveQueueRequest(Entity<FHResearchConsoleComponent> ent, ref FHResearchConsoleRemoveQueueRequest args)
    {
        if (!this.IsPowered(ent, EntityManager))
            return;

        if (TryComp<AccessReaderComponent>(ent, out var access) && !_accessReader.IsAllowed(args.Actor, ent, access))
        {
            ShowError((ent, ent.Comp), Loc.GetString("research-console-no-access-popup"));
            return;
        }
        
        if (ent.Comp.Readonly || !TryGetServerWithTree(ent.Owner, out var server))
            return;

        if (!RemoveResearchFromQueue((server.Value, server.Value.Comp), args.Node))
            return;
        
        if (!_emag.CheckFlag(ent, EmagType.Interaction))
        {
            var getIdentityEvent = new TryGetIdentityShortInfoEvent(ent, args.Actor);
            RaiseLocalEvent(getIdentityEvent);

            var message = Loc.GetString(
                "research-tree-console-remove-queue-radio-broadcast",
                ("technology", Loc.GetString(_protoMan.Index(args.Node).Name)),
                ("approver", getIdentityEvent.Title ?? string.Empty)
            );

            foreach (var channel in server.Value.Comp.AnnounceTo)
                if (_protoMan.TryIndex(channel, out var channelProto))
                    _radio.SendRadioMessage(ent, message, channelProto, ent, escapeMarkup: false);
        }
    }
    private void OnResearchRequest(Entity<FHResearchConsoleComponent> ent, ref FHResearchConsoleResearchRequest args)
    {
        if (!this.IsPowered(ent, EntityManager))
            return;

        if (TryComp<AccessReaderComponent>(ent, out var access) && !_accessReader.IsAllowed(args.Actor, ent, access))
        {
            ShowError((ent, ent.Comp), Loc.GetString("research-console-no-access-popup"));
            return;
        }

        if (ent.Comp.Readonly || !TryGetServerWithTree(ent.Owner, out var server))
            return;
        
        if (!AddResearchToQueue((server.Value, server.Value.Comp), args.Node))
            return;
        
        if (!_emag.CheckFlag(ent, EmagType.Interaction))
        {
            var getIdentityEvent = new TryGetIdentityShortInfoEvent(ent, args.Actor);
            RaiseLocalEvent(getIdentityEvent);

            var message = Loc.GetString(
                "research-tree-console-add-queue-radio-broadcast",
                ("technology", Loc.GetString(_protoMan.Index(args.Node).Name)),
                ("approver", getIdentityEvent.Title ?? string.Empty)
            );

            foreach (var channel in server.Value.Comp.AnnounceTo)
                if (_protoMan.TryIndex(channel, out var channelProto))
                    _radio.SendRadioMessage(ent, message, channelProto, ent, escapeMarkup: false);
        }
    }

    private void OnEmagged(Entity<FHResearchConsoleComponent> ent, ref GotEmaggedEvent args)
    {
        if (!_emag.CompareFlag(args.Type, EmagType.Interaction))
            return;

        if (_emag.CheckFlag(ent, EmagType.Interaction))
            return;

        args.Handled = true;
    }

    private void OnConsoleRegistrationChanged(Entity<FHResearchConsoleComponent> ent, ref ResearchRegistrationChangedEvent args) =>
        UpdateUI((ent, ent.Comp), true);

    private void OnConsoleBeforeUiOpened(Entity<FHResearchConsoleComponent> ent, ref BeforeActivatableUIOpenEvent args) =>
        UpdateUI((ent, ent.Comp), true);

    public void UpdateUI(Entity<FHResearchConsoleComponent?> ent, bool build = false)
    {
        if (!Resolve(ent, ref ent.Comp) || (!build && !_ui.IsUiOpen(ent.Owner, FHResearchConsoleUiKey.Key)))
            return;

        HashSet<ProtoId<ResearchTreeNodePrototype>> nodes = [];
        HashSet<ProtoId<ResearchTreeTierPrototype>> unlockedTiers = [];
        HashSet<ProtoId<ResearchTreeNodePrototype>> unlockedNodes = [];
        HashSet<ProtoId<ResearchTreeNodePrototype>> researchedNodes = [];
        List<ProtoId<ResearchTreeNodePrototype>> queuedNodes = [];
        Dictionary<ProtoId<ResearchTreeNodePrototype>, int> researchProgress = [];
        int bankedPoints = 0;
        bool readonlyConsole = false;

        if (TryGetServerWithTree(ent.Owner, out var server))
        {
            if (build)
                nodes = [.. GetTreeNodes(server.Value).Select(p => (ProtoId<ResearchTreeNodePrototype>)p.ID)];
            
            unlockedTiers = GetUnlockedTiers(server.Value);
            unlockedNodes = GetUnlockedNodes(server.Value);
            researchedNodes = server!.Value.Comp.Researched;
            queuedNodes = server!.Value.Comp.Queue;
            researchProgress = server!.Value.Comp.Progress;
            bankedPoints = server!.Value.Comp.BankedPoints;
            readonlyConsole = ent.Comp.Readonly;
        }

        if (build)
            _ui.SetUiState(ent.Owner, FHResearchConsoleUiKey.Key, new FHResearchConsoleBUIFullState(nodes, unlockedTiers, unlockedNodes, researchedNodes, queuedNodes, researchProgress, bankedPoints, readonlyConsole));
        else
            _ui.SetUiState(ent.Owner, FHResearchConsoleUiKey.Key, new FHResearchConsoleBUIPartialState(unlockedTiers, unlockedNodes, researchedNodes, queuedNodes, researchProgress, bankedPoints));
    }

    public void ShowError(Entity<FHResearchConsoleComponent?> ent, string message = "")
    {
        if (!Resolve(ent, ref ent.Comp) || ent.Comp.Readonly || !this.IsPowered(ent, EntityManager))
            return;

        _audio.PlayPvs(ent.Comp.ErrorSound, ent);
        if (!string.IsNullOrEmpty(message))
            _chat.TrySendInGameICMessage(ent, message, InGameICChatType.Speak, ChatTransmitRange.HideChat, true);
    }
}