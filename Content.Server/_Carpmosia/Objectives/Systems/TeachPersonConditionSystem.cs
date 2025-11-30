using Content.Server.Objectives.Components;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Robust.Shared.Configuration;

namespace Content.Server.Objectives.Systems;

/// <summary>
/// Handles 'Teach x a lesson' objective logic and picking random objective targets
/// </summary>

public sealed class TeachPersonConditionSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly TargetObjectiveSystem _target = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TeachPersonConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnGetProgress(EntityUid uid, TeachPersonConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        if (!_target.GetTarget(uid, out var target))
            return;

        args.Progress = GetProgress(target.Value, comp);
    }

    private float GetProgress(EntityUid target, TeachPersonConditionComponent comp)
    {
        // deleted or gibbed or something, counts as a success
        if (!TryComp<MindComponent>(target, out var mind) || mind.OwnedEntity == null)
            return 1f;

        // they're already educated
        if (comp.HasDied)
            return 1f;

        var targetDead = _mind.IsCharacterDeadIc(mind);

        // they haven't died yet!
        if (!targetDead)
            return 0f;

        comp.HasDied = true;

        return 1f; // lesson = taught
    }
}
