using Content.Server.Objectives.Components;
using Content.Server.Shuttles.Systems;
using Content.Shared.Cuffs;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;

namespace Content.Server.Objectives.Systems;

public sealed partial class EscapeShuttleConditionSystem : EntitySystem
{
    [Dependency] private EmergencyShuttleSystem _emergencyShuttle = default!;
    [Dependency] private SharedCuffableSystem _cuffable = default!;
    [Dependency] private SharedMindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EscapeShuttleConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnGetProgress(Entity<EscapeShuttleConditionComponent> entity, ref ObjectiveGetProgressEvent args) // Carpmosia-edit - escape restrained
    {
        args.Progress = GetProgress(args.MindId, args.Mind, entity.Comp.AllowRestrained); // Carpmosia-edit - escape restrained
    }

    public float GetProgress(EntityUid mindId, MindComponent mind, bool AllowRestrained = false) // Carpmosia-edit - Harmony Blood Bound / escape restrained
    {
        // not escaping alive if you're deleted/dead
        if (mind.OwnedEntity == null || _mind.IsCharacterDeadIc(mind))
            return 0f;

        // Carpmosia-start - escape restrained
        if (!AllowRestrained)
            if (_cuffable.IsCuffed(mind.OwnedEntity.Value))
                return _emergencyShuttle.IsTargetEscaping(mind.OwnedEntity.Value) ? 0.5f : 0f;
        // Carpmosia-end - escape restrained

        // Any emergency shuttle counts for this objective, but not pods.
        return _emergencyShuttle.IsTargetEscaping(mind.OwnedEntity.Value) ? 1f : 0f;
    }
}
