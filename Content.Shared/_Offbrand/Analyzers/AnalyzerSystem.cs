using Robust.Shared.Timing;

namespace Content.Shared._Offbrand.Analyzers;

public sealed partial class AnalyzerSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<AnalyzerComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var analyzer, out var transform))
        {
            if (analyzer.NextUpdate >= _timing.CurTime || analyzer.Target is not { } target)
                continue;

            analyzer.NextUpdate = _timing.CurTime + analyzer.UpdateInterval;
            Dirty(uid, analyzer);

            if (Deleted(target))
            {
                Analyze((uid, analyzer), null);
                continue;
            }

            TryUpdate((uid, analyzer, transform));
        }
    }

    private bool CheckRange(Entity<AnalyzerComponent, TransformComponent?> analyzer, Entity<TransformComponent?> target)
    {
        return analyzer.Comp1.ScanRange is not { } range || _transform.InRange((analyzer, analyzer.Comp2), target, range);
    }

    private void TryUpdate(Entity<AnalyzerComponent, TransformComponent?> analyzer)
    {
        var after = new AfterAnalyzerUpdatedEvent();

        if (analyzer.Comp1.Target is not { } target || !analyzer.Comp1.ShouldUpdate)
        {
            analyzer.Comp1.IsUpdating = false;
            Dirty(analyzer, analyzer.Comp1);

            RaiseLocalEvent(analyzer, ref after);
            return;
        }

        analyzer.Comp1.IsUpdating = CheckRange(analyzer, target);
        Dirty(analyzer, analyzer.Comp1);

        if (!analyzer.Comp1.IsUpdating)
        {
            if (!analyzer.Comp1.AutoRelink)
            {
                analyzer.Comp1.Target = null;
                analyzer.Comp1.ActualTarget = null;
                Dirty(analyzer, analyzer.Comp1);
            }
            RaiseLocalEvent(analyzer, ref after);
            return;
        }

        var actual = new AnalyzerActualTargetEvent(target, target);
        RaiseLocalEvent(analyzer, ref actual);

        if (actual.ActualTarget != analyzer.Comp1.ActualTarget)
        {
            analyzer.Comp1.ActualTarget = actual.ActualTarget;
            Dirty(analyzer, analyzer.Comp1);

            var updated = new AnalyzerActualTargetUpdatedEvent();
            RaiseLocalEvent(analyzer, ref updated);
        }

        if (actual.ActualTarget is not { } actualTarget)
            return;

        var evt = new AnalyzerUpdatedEvent(actualTarget);
        RaiseLocalEvent(analyzer, ref evt);
        RaiseLocalEvent(analyzer, ref after);
    }


    public void Analyze(Entity<AnalyzerComponent?> analyzer, EntityUid? target)
    {
        if (!Resolve(analyzer, ref analyzer.Comp))
            return;

        analyzer.Comp.Target = target;
        analyzer.Comp.ActualTarget = null;
        analyzer.Comp.ShouldUpdate = target is not null;
        Dirty(analyzer);

        TryUpdate((analyzer, analyzer.Comp));
    }

    public void SetShouldUpdate(Entity<AnalyzerComponent?> analyzer, bool shouldUpdate)
    {
        if (!Resolve(analyzer, ref analyzer.Comp))
            return;

        analyzer.Comp.ShouldUpdate = shouldUpdate;
        Dirty(analyzer);

        TryUpdate((analyzer, analyzer.Comp));
    }
}
