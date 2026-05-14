using System.Globalization;
using System.Linq;
using Content.Shared.Body;
using Content.Shared.Body.Components;
using Content.Shared.Buckle.Components;
using Content.Shared.Destructible;
using Content.Shared.DoAfter;
using Content.Shared.Maps;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.GameObjects;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;

namespace Content.Server.Medical;

public sealed class OperatingTableSystem : EntitySystem
{

[Dependency] private readonly BodySystem _bodySystem = default!;
[Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
[Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SurgicalToolComponent, GetVerbsEvent<UtilityVerb>>(OnUtilityVerb);
    }

    private void OnUtilityVerb(Entity<SurgicalToolComponent> ent, ref GetVerbsEvent<UtilityVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess )
            return;

        // Something something ref params in lambdas...
        var user = args.User;
        var target = args.Target;

        var verb = new UtilityVerb()
        {
            Act = () => TryRemoveOrgan(ent, user, target),
            Text = "Remove brain",
            Message = "Removing brain... ",
            DoContactInteraction = true
        };
    }

    private bool TryRemoveOrgan(Entity<SurgicalToolComponent> ent, EntityUid user, Entity<BrainComponent> target)
    {

        if (!TryComp<BuckleComponent>(target, out var buckle))
            return false;

        if (user == target.Owner)
        {
            _popupSystem.PopupEntity(Loc.GetString("Don't fucking do that"), user, user);
            return false;
        }

        var ev = new OrganRemovalDoAfterEvent();

        var doAfter = new DoAfterArgs(EntityManager, user, TimeSpan.FromSeconds(10), ev, user)
        {
            BreakOnMove = true,
            NeedHand = true,
            BreakOnHandChange = true,
        };

        if (!_doAfterSystem.TryStartDoAfter(doAfter))
            return false;

        _bodySystem.RelayEvent(target.Owner, new OrganRemovedFromEvent(target));

        return true;
    }
}
