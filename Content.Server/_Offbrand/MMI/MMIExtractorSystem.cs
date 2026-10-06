using Content.Server.EUI;
using Content.Shared._Offbrand.MMI;
using Content.Shared.Body.Components;
using Content.Shared.Body;
using Content.Shared.Chat;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Mind;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Whitelist;
using Robust.Shared.Player;

namespace Content.Server._Offbrand.MMI;

public sealed partial class MMIExtractorSystem : EntitySystem
{
    [Dependency] private EntityWhitelistSystem _whitelist = default!;
    [Dependency] private EuiManager _eui = default!;
    [Dependency] private ISharedPlayerManager _player = default!;
    [Dependency] private ItemSlotsSystem _slots = default!;
    [Dependency] private BodySystem _body = default!;
    [Dependency] private SharedChatSystem _chat = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedMindSystem _mind = default!;

    [Dependency] private EntityQuery<MMIExtractorComponent> _extractorQuery;
    [Dependency] private EntityQuery<MMIComponent> _mmiQuery;
    [Dependency] private EntityQuery<DoAfterComponent> _doAfterQuery;

    [SubscribeLocalEvent]
    private void OnAfterInteract(Entity<MMIExtractorComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target == null)
            return;

        if (TryExtract(ent, args.Target.Value, args.User))
            args.Handled = true;
    }

    [SubscribeLocalEvent]
    private void OnDoAfter(Entity<BrainComponent> ent, ref MMIExtractorDoAfterEvent evt)
    {
        if (evt.Handled || evt.Cancelled)
            return;

        if (evt.Args.Used is not { } mmi || !_extractorQuery.TryComp(mmi, out var mmiComp))
            return;

        if (!_mmiQuery.TryComp(mmi, out var insertionComp))
            return;

        if (!evt.Accepted)
        {
            _chat.TrySendInGameICMessage(mmi,
                Loc.GetString(mmiComp.NoResponse),
                InGameICChatType.Speak,
                true);

            return;
        }

        _slots.TryInsert(mmi, insertionComp.BrainSlotId, ent, null);
    }

    private bool TryExtract(Entity<MMIExtractorComponent> ent, EntityUid target, EntityUid user)
    {
        if (!_whitelist.CheckBoth(target, ent.Comp.Blacklist, ent.Comp.Whitelist))
            return false;

        if (!_mind.TryGetMind(target, out _, out var mind) || !_player.TryGetSessionById(mind.UserId, out var playerSession))
        {
            _chat.TrySendInGameICMessage(ent,
                Loc.GetString(ent.Comp.NoMind),
                InGameICChatType.Speak,
                true);

            return true;
        }

        if (!_body.TryGetOrgansWithComponent<BrainComponent>(target, out var organs))
        {
            _chat.TrySendInGameICMessage(ent,
                Loc.GetString(ent.Comp.Brainless),
                InGameICChatType.Speak,
                true);

            return true;
        }

        if (organs.Count != 1)
        {
            _chat.TrySendInGameICMessage(ent,
                Loc.GetString(ent.Comp.TooManyBrains),
                InGameICChatType.Speak,
                true);

            return true;
        }

        var brain = organs[0];

        _chat.TrySendInGameICMessage(ent,
            Loc.GetString(ent.Comp.Asking),
            InGameICChatType.Speak,
            true);

        var args =
            new DoAfterArgs(EntityManager, user, ent.Comp.Delay, new MMIExtractorDoAfterEvent(), brain, target: target, used: ent)
            {
                NeedHand = true,
                BreakOnMove = true,
                BreakOnWeightlessMove = false,
            };

        if (_doAfter.TryStartDoAfter(args, out var id))
            _eui.OpenEui(new MMIExtractorEui(this, id.Value), playerSession);

        return true;
    }

    public void Decline(DoAfterId id)
    {
        _doAfter.Cancel(id);

        if (!_doAfterQuery.TryComp(id.Uid, out var doAfters))
            return;

        var dict = doAfters.DoAfters; // i love access workarounds
        if (!dict.TryGetValue(id.Index, out var doAfter))
            return;

        if (doAfter.Args.Used is not { } mmi || !_extractorQuery.TryComp(mmi, out var mmiComp))
            return;

        _chat.TrySendInGameICMessage(mmi,
            Loc.GetString(mmiComp.Denied),
            InGameICChatType.Speak,
            true);
    }

    public void Accept(DoAfterId id)
    {
        if (!_doAfterQuery.TryComp(id.Uid, out var doAfters))
            return;

        var dict = doAfters.DoAfters; // i love access workarounds
        if (!dict.TryGetValue(id.Index, out var doAfter))
            return;

        if (doAfter.Args.Used is not { } mmi || !_extractorQuery.TryComp(mmi, out var mmiComp))
            return;

        if (doAfter.Args.Event is not MMIExtractorDoAfterEvent evt)
            return;

        evt.Accepted = true;
        _chat.TrySendInGameICMessage(mmi,
            Loc.GetString(mmiComp.Accepted),
            InGameICChatType.Speak,
            true);
    }
}
