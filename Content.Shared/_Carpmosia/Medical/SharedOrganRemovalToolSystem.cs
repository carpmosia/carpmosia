using Content.Shared.Body;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Buckle.Components;
using Content.Shared.Chat;
using Content.Shared.Destructible;
using Content.Shared.DoAfter;
using Content.Shared.Forensics.Systems;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;

namespace Content.Shared.Medical;

/// <summary>
/// Defines behavior for the simple brain extraction tool. Could be easily generecised but this is all temporary anyway pending discomed.
/// </summary>
public sealed class SharedOrganRemovalTool : EntitySystem
{

    [Dependency] private readonly ISharedChatManager _chat = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedForensicsSystem _forensics = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<OrganRemovalToolComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<BrainComponent, OrganRemovalDoAfterEvent>(OnDoAfter);
    }


    private void OnAfterInteract(EntityUid uid, OrganRemovalToolComponent tool, AfterInteractEvent args)
    {
        if (args.Handled || args.Target is null || !args.CanReach)
            return;

        // If they don't have body component somehow, ollie out
        if (!TryComp<BodyComponent>(args.Target, out var body))
            return;

        // If not buckled to a surgical table, ollie out
        if (!TryComp<BuckleComponent>(args.Target, out var buckle) ||
            !TryComp<SurgicalTableComponent>(buckle.BuckledTo, out var table))
        {
            _popupSystem.PopupClient(Loc.GetString("organ-removal-operation-fail-table",
                ("target", Identity.Entity(args.Target.Value, EntityManager))), args.User, PopupType.MediumCaution);
            return;
        }

        // If they aren't dead, ollie out
        if (!_mobStateSystem.IsDead(args.Target.Value))
        {
            _popupSystem.PopupClient(Loc.GetString("organ-removal-operation-fail-alive",
                ("target", Identity.Entity(args.Target.Value, EntityManager))), args.User, PopupType.MediumCaution);
            return;
        }

        // Find the brain, if there is one, then start the doAfter
        foreach (var organ in body.Organs?.ContainedEntities ?? [])
        {
            if (TryComp<BrainComponent>(organ, out var brain))
                TryStartDoAfter(args.User, args.Target, (organ, brain), tool.SurgeryDelay, tool);
        }

        args.Handled = true;
    }

    private bool TryStartDoAfter(EntityUid user, EntityUid? target, Entity<BrainComponent> ent, TimeSpan delay, OrganRemovalToolComponent tool)
    {
        var ev = new OrganRemovalDoAfterEvent();

        var doAfter = new DoAfterArgs(EntityManager, user, delay, ev, ent, target: target, used: tool.Owner)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            BreakOnWeightlessMove = true,
            NeedHand = true,
            BreakOnHandChange = true,
        };

        if (!_doAfterSystem.TryStartDoAfter(doAfter))
            return false;

        _popupSystem.PopupPredicted(Loc.GetString("organ-removal-operation-start"),
            Loc.GetString("organ-removal-operation-start-other", ("user", Identity.Entity(user, EntityManager))), user, user, PopupType.MediumCaution);

        _audioSystem.PlayPvs(tool.StartSound, ent, AudioParams.Default.WithVariation(0.125f).WithVolume(2f).WithMaxDistance(20f));

        return true;
    }

    private void OnDoAfter(Entity<BrainComponent> ent, ref OrganRemovalDoAfterEvent args)
    {
        // The try comp is extremely dumb but in my attempts to refactor it once it fought back so viciously I decided to no longer care
        if (args.Cancelled || args.Handled || args.Target == null || args.Used == null || !TryComp<OrganRemovalToolComponent>(args.Used, out var tool))
            return;

        var baseXform = Transform(args.Target.Value);

        // Brain plops onto the ground in highly sanitary fashion
        _transformSystem.PlaceNextTo(ent.Owner, (args.Target.Value, baseXform));

        // Big bloody mess left behind
        if (TryComp<BloodstreamComponent>(args.Target.Value, out var bloodstream))
            _bloodstream.TryBleedOut(new Entity<BloodstreamComponent?>(args.Target.Value, bloodstream), 120);

        // Forensics is fun
        _forensics.TransferDna(new Entity<OrganRemovalToolComponent>(args.Used.Value, tool), args.Target.Value);

        _popupSystem.PopupPredicted(Loc.GetString("organ-removal-tool-operation-end",
            ("target", Identity.Entity(args.Target.Value, EntityManager))), args.Target.Value, args.User, PopupType.MediumCaution);

        _audioSystem.PlayPvs(tool.EndSound, ent, AudioParams.Default.WithVariation(0.125f).WithVolume(-1f).WithMaxDistance(20f));

        _chat.SendAdminAlert(args.User, Loc.GetString("interaction-remove-brain-admin-announcement",
            ("user", Identity.Entity(args.User, EntityManager)), ("target", Identity.Entity(args.Target.Value, EntityManager))));

        args.Handled = true;
    }
}
