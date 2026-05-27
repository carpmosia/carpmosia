using System.Reflection.Emit;
using Content.Shared.Body;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Buckle.Components;
using Content.Shared.Chat;
using Content.Shared.Destructible;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
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
using Robust.Shared.Utility;

namespace Content.Shared.Medical;

/// <summary>
/// Defines behavior for the simple brain extraction tool. Could be easily generecised but this is all temporary anyway pending discomed.
/// </summary>
public sealed partial class SharedOrganRemovalToolSystem : EntitySystem
{

    [Dependency] ISharedChatManager _chat = default!;
    [Dependency] SharedAudioSystem _audioSystem = default!;
    [Dependency] SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] SharedForensicsSystem _forensics = default!;
    [Dependency] SharedPopupSystem _popupSystem = default!;
    [Dependency] SharedTransformSystem _transformSystem = default!;
    [Dependency] MobStateSystem _mobStateSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<OrganRemovalToolComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<OrganRemovalToolComponent, OrganRemovalDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<BrainExtractedComponent, ExaminedEvent>(OnExamined);
    }
    private void OnAfterInteract(Entity<OrganRemovalToolComponent> uid, ref AfterInteractEvent args)
    {
        if (args.Handled || args.Target is null || !args.CanReach || args.User == args.Target || !HasComp<BodyComponent>(args.Target))
            return;

        args.Handled = TryStartDoAfter(uid, args.User, args.Target.Value);
    }

    private bool TryStartDoAfter(Entity<OrganRemovalToolComponent> tool, EntityUid user, EntityUid target)
    {

        // If not buckled to a surgical table, display message and then end
        if (!TryComp<BuckleComponent>(target, out var buckle) ||
            !HasComp<SurgicalTableComponent>(buckle.BuckledTo))
        {
            _popupSystem.PopupClient(Loc.GetString("organ-removal-operation-fail-table",
                ("target", Identity.Entity(target, EntityManager))), user, PopupType.MediumCaution);
            return false;
        }

        // If they aren't dead, display message and then end
        if (!_mobStateSystem.IsDead(target))
        {
            _popupSystem.PopupClient(Loc.GetString("organ-removal-operation-fail-alive",
                ("target", Identity.Entity(target, EntityManager))), user, PopupType.MediumCaution);
            return false;
        }

        var ev = new OrganRemovalDoAfterEvent();

        var doAfter = new DoAfterArgs(EntityManager, user, tool.Comp.SurgeryDelay, ev, tool, target: target, used: tool)
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

        _audioSystem.PlayPvs(tool.Comp.StartSound, tool, AudioParams.Default.WithVariation(0.125f).WithVolume(2f).WithMaxDistance(20f));

        return true;
    }

    private void OnDoAfter(Entity<OrganRemovalToolComponent> tool, ref OrganRemovalDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target == null || args.Used == null || !TryComp<BodyComponent>(args.Target, out var body))
            return;

        // Find the brain, then remove it
        foreach (var organ in body.Organs?.ContainedEntities ?? [])
        {
            // Brain plops onto the ground in highly sanitary fashion
            if (HasComp<BrainComponent>(organ))
            {
                _transformSystem.DropNextTo(organ, args.Target.Value);

                // Big bloody mess left behind
                if (TryComp<BloodstreamComponent>(args.Target.Value, out var bloodstream))
                    _bloodstream.TryBleedOut(new Entity<BloodstreamComponent?>(args.Target.Value, bloodstream), 120);

                // Forensics is fun
                _forensics.TransferDna(new Entity<OrganRemovalToolComponent>(args.Used.Value, tool), args.Target.Value);

                // Display success message, add extracted component for examine text
                _popupSystem.PopupPredicted(Loc.GetString("organ-removal-tool-operation-end",
                    ("target", Identity.Entity(args.Target.Value, EntityManager))), args.Target.Value, args.User, PopupType.MediumCaution);

                EnsureComp<BrainExtractedComponent>(args.Target.Value);

                _audioSystem.PlayPvs(tool.Comp.EndSound, tool, AudioParams.Default.WithVariation(0.125f).WithVolume(-1f).WithMaxDistance(20f));

                _chat.SendAdminAlert(args.User, Loc.GetString("interaction-remove-brain-admin-announcement",
                    ("user", Identity.Entity(args.User, EntityManager)), ("target", Identity.Entity(args.Target.Value, EntityManager))));

                return;
            }
        }
        // We didn't find a brain so display operation fail message. This is dumb but c'est la vie
        _popupSystem.PopupPredicted(Loc.GetString("organ-removal-operation-fail-brain",
            ("target", Identity.Entity(args.Target.Value, EntityManager))), args.Target.Value, args.User, PopupType.MediumCaution);
    }

    // Add examine text to individuals that got their brain removed
    private void OnExamined(Entity<BrainExtractedComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        var msg = Loc.GetString("orgam-removal-examine-text");

        args.PushMarkup(msg);
    }
}
