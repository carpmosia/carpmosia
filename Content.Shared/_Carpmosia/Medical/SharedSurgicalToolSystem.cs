using Content.Shared.Body;
using Content.Shared.Body.Components;
using Content.Shared.Buckle.Components;
using Content.Shared.Destructible;
using Content.Shared.DoAfter;
using Content.Shared.Gibbing;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;

namespace Content.Shared.Medical;

/// <summary>
/// Defines behavior for the simple brain extraction tool
/// </summary>
public sealed class SharedSurgicalToolSystem : EntitySystem
{

[Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
[Dependency] private readonly SharedPopupSystem _popupSystem = default!;
[Dependency] private readonly SharedTransformSystem _transformSystem = default!;
[Dependency] private readonly MobStateSystem _mobStateSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SurgicalToolComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<BrainComponent, OrganRemovalDoAfterEvent>(OnDoAfter);
    }


    private void OnAfterInteract(EntityUid uid, SurgicalToolComponent comp, AfterInteractEvent args)
    {
        if (args.Handled || args.Target is null || !args.CanReach)
            return;

        // If they don't have body component somehow, ollie out
        if (!TryComp<BodyComponent>(args.Target, out var body))
            return;

        // If not buckled to a surgical table, ollie out
        if (!TryComp<BuckleComponent>(args.Target, out var buckle))
            return;

        if (!TryComp<SurgicalTableComponent>(buckle.BuckledTo, out var surgicalTable))
            return;

        // If they aren't dead, ollie out
        if (!_mobStateSystem.IsDead(args.Target.Value))
            return;

        // Find the brain, if there is one, then start the doAfter
        foreach (var organ in body.Organs?.ContainedEntities ?? [])
        {
            if (TryComp<BrainComponent>(organ, out var brain))
            {
                TryStartDoAfter(args.User, args.Target, (organ, brain), comp.SurgeryDelay);
            }
        }

        args.Handled = true;
    }

    private bool TryStartDoAfter(EntityUid user, EntityUid? target, Entity<BrainComponent> ent, TimeSpan delay)
    {

        var ev = new OrganRemovalDoAfterEvent();

        var doAfter = new DoAfterArgs(EntityManager, user, delay, ev, ent, target: target)
        {
            BreakOnMove = true,
            NeedHand = true,
            BreakOnHandChange = true,
        };

        if (!_doAfterSystem.TryStartDoAfter(doAfter))
            return false;

        _popupSystem.PopupPredicted("You begin harvesting their brain.", "They begin harvesting that person's brain", ent, user, PopupType.MediumCaution);

        return true;
    }

    private void OnDoAfter(Entity<BrainComponent> ent, ref OrganRemovalDoAfterEvent args)
    {

        if (args.Cancelled || args.Handled || args.Target == null)
            return;

        var baseXform = Transform(args.Target.Value);

        _transformSystem.PlaceNextTo(ent.Owner, (args.Target.Value, baseXform));

    }
}
