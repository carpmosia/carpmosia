using Content.Shared._Offbrand.Wounds;
using Content.Shared.Buckle;
using Content.Shared.Buckle.Components;
using Content.Shared.Chat;
using Content.Shared.DragDrop;
using Content.Shared.IdentityManagement;

namespace Content.Shared._Offbrand.Analyzers;

public sealed class StationaryAnalyzerSystem : EntitySystem
{
    [Dependency] private SharedChatSystem _chat = default!;
    [Dependency] private AnalyzerSystem _analyzer = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationaryAnalyzerComponent, AnalyzerActualTargetEvent>(OnActualTarget);
        SubscribeLocalEvent<StationaryAnalyzerComponent, CanDragEvent>(OnCanDrag);
        SubscribeLocalEvent<StationaryAnalyzerComponent, CanDropDraggedEvent>(OnCanDropDragged);
        SubscribeLocalEvent<StationaryAnalyzerComponent, DragDropDraggedEvent>(OnDragDropDragged);
        SubscribeLocalEvent<StationaryAnalyzerStrapComponent, CanDropTargetEvent>(OnCanDropTarget, after: [typeof(SharedBuckleSystem)]);
    }

    private void OnActualTarget(Entity<StationaryAnalyzerComponent> ent, ref AnalyzerActualTargetEvent args)
    {
        args.ActualTarget = null;

        if (TryComp<StrapComponent>(args.Target, out var strap))
        {
            foreach (var buckled in strap.BuckledEntities)
            {
                if (!HasComp<PerfusionComponent>(buckled))
                    continue;

                args.ActualTarget = buckled;
                break;
            }
        }
        else if (HasComp<PerfusionComponent>(args.Target))
        {
            args.ActualTarget = args.Target;
        }
    }

    private void OnCanDrag(Entity<StationaryAnalyzerComponent> ent, ref CanDragEvent args)
    {
        args.Handled = true;
    }

    private void OnCanDropDragged(Entity<StationaryAnalyzerComponent> ent, ref CanDropDraggedEvent args)
    {
        if (!(HasComp<PerfusionComponent>(args.Target) || HasComp<StrapComponent>(args.Target)))
            return;

        args.Handled = true;
        args.CanDrop = true;
    }

    private void OnDragDropDragged(Entity<StationaryAnalyzerComponent> ent, ref DragDropDraggedEvent args)
    {
        if (!(HasComp<PerfusionComponent>(args.Target) || HasComp<StrapComponent>(args.Target)) || !TryComp<AnalyzerComponent>(ent, out var analyzer))
            return;

        args.Handled = true;
        if (analyzer.Target == args.Target)
            _analyzer.Analyze((ent, analyzer), null);
        else
        {
            _analyzer.Analyze((ent, analyzer), args.Target);
            var identity = Identity.Entity(args.Target, EntityManager);

            if (HasComp<PerfusionComponent>(args.Target))
                _chat.TrySendInGameICMessage(ent, Loc.GetString(ent.Comp.ScanningPatient, ("patient", identity)), InGameICChatType.Speak, true);
            else
                _chat.TrySendInGameICMessage(ent, Loc.GetString(ent.Comp.ScanningStrap, ("strap", identity)), InGameICChatType.Speak, true);
        }

        Dirty(ent);
    }

    private void OnCanDropTarget(Entity<StationaryAnalyzerStrapComponent> ent, ref CanDropTargetEvent args)
    {
        if (HasComp<AnalyzerComponent>(args.Dragged))
        {
            args.CanDrop = true;
            args.Handled = true;
        }
    }
}
