using Content.Shared.Body;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Content.Shared.Tools.Components;
using Content.Shared.Whitelist;

namespace Content.Shared._Offbrand.Surgery;

public sealed partial class SurgeryToolSystem : EntitySystem
{
    [Dependency] private EntityWhitelistSystem _entityWhitelist = default!;
    [Dependency] private InventorySystem _inventory = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private StandingStateSystem _standingState = default!;

    [SubscribeLocalEvent]
    private void OnToolAttemptUse(Entity<SurgeryToolComponent> ent, ref ToolUseAttemptEvent args)
    {
        if (args.Target is not { } target)
            return;

        if (_inventory.TryGetContainerSlotEnumerator(target, out var enumerator, ent.Comp.SlotsToCheck))
        {
            while (enumerator.MoveNext(out var slot))
            {
                if (slot.ContainedEntity is not { } contained)
                    continue;

                if (_entityWhitelist.CheckBoth(contained, ent.Comp.Blacklist, ent.Comp.Whitelist))
                    continue;

                _popup.PopupCursor(Loc.GetString(ent.Comp.SlotsDenialPopup, ("target", args.Target), ("clothing", contained)), args.User);
                args.Cancel();
                return;
            }
        }

        if (TryComp<OrganComponent>(args.Target, out var organ) && organ.Body != null)
        {
            if (!_standingState.IsDown(organ.Body.Value))
            {
                _popup.PopupCursor(Loc.GetString(ent.Comp.DownDenialPopup, ("target", organ.Body)), args.User);
                args.Cancel();
            }
        }

        else if (!_standingState.IsDown(target))
        {
            _popup.PopupCursor(Loc.GetString(ent.Comp.DownDenialPopup, ("target", args.Target)), args.User);
            args.Cancel();
        }
    }
}
