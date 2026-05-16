using System.Globalization;
using System.Linq;
using System.Runtime;
using Content.Shared.Body;
using Content.Shared.Body.Components;
using Content.Shared.Buckle.Components;
using Content.Shared.Destructible;
using Content.Shared.DoAfter;
using Content.Shared.Gibbing;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.GameObjects;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;

namespace Content.Shared.Medical;

public sealed class SharedSurgicalToolSystem : EntitySystem
{

// [Dependency] private readonly BodySystem _bodySystem = default!;
// [Dependency] private readonly GibbingSystem _gibbing = default!;
[Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
[Dependency] private readonly SharedPopupSystem _popupSystem = default!;
[Dependency] private readonly SharedTransformSystem _transformSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SurgicalToolComponent, AfterInteractEvent>(OnAfterInteract);

        // SubscribeLocalEvent<SurgicalToolComponent, GetVerbsEvent<AlternativeVerb>>(AddVerbs);
        SubscribeLocalEvent<BrainComponent, OrganRemovalDoAfterEvent>(OnDoAfter);
        // SubscribeLocalEvent <SurgicalTableComponent, InteractUsingEvent>(OnInteractUsing);
    }


    private void OnAfterInteract(EntityUid uid, SurgicalToolComponent component, AfterInteractEvent args)
    {
        if (args.Handled || args.Target is null || !args.CanReach)
            return;



        if (!TryComp<BodyComponent>(args.Target, out var body))
            return;

        foreach (var organ in body.Organs?.ContainedEntities ?? [])
        {
            if (TryComp<BrainComponent>(organ, out var brain))
            {
                TryStartDoAfter(args.User, args.Target, (organ, brain));

            }
        }

        args.Handled = true;
    }

    private bool TryStartDoAfter(EntityUid user, EntityUid? target, Entity<BrainComponent> ent)
    {

        var ev = new OrganRemovalDoAfterEvent();

        var doAfter = new DoAfterArgs(EntityManager, user, TimeSpan.FromSeconds(10), ev, ent, target: target)
        {
            BreakOnMove = true,
            NeedHand = true,
            BreakOnHandChange = true,
        };

        if (!_doAfterSystem.TryStartDoAfter(doAfter))
            return false;

        return true;
    }

    private void OnDoAfter(Entity<BrainComponent> ent, ref OrganRemovalDoAfterEvent args)
    {

        if (args.Cancelled || args.Handled || args.Target == null)
            return;

        _popupSystem.PopupEntity(Loc.GetString("lol lmao brain go byebye"), args.Target.Value,
                        args.Target.Value, PopupType.LargeCaution);

        var baseXform = Transform(args.Target.Value);

        _transformSystem.PlaceNextTo(ent.Owner, (args.Target.Value, baseXform));

    }
}
