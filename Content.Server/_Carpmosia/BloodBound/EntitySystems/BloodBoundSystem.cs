using Content.Server.Mind;
using Content.Server.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Server.Roles;
using Content.Shared.BloodBound.Components;
using Content.Shared.BloodBound.EntitySystems;
using Content.Shared.Roles.Components;

namespace Content.Server.BloodBound.EntitySystems;

public sealed partial class BloodBoundSystem : SharedBloodBoundSystem
{
    [Dependency] private MindSystem _mindSystem = default!;
    [Dependency] private RoleSystem _roleSystem = default!;
    [Dependency] private TargetObjectiveSystem _targetObjectiveSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodBoundComponent, ComponentShutdown>(OnBloodBoundShutdown);
    }

    private void OnBloodBoundShutdown(Entity<BloodBoundComponent> entity, ref ComponentShutdown args)
    {
        if (!_mindSystem.TryGetMind(entity, out var mindId, out var mind))
            return;

        if (_roleSystem.MindHasRole<BloodBoundRoleComponent>(mindId, out var role))
        {
            // Initial no longer has to worry about keeping the converted alive or on the shuttle
            if (role.Value.Comp2.Bound != null &&
                _mindSystem.TryGetMind(role.Value.Comp2.Bound.Value, out _, out var boundMind))
            {
                foreach (var objective in boundMind.Objectives)
                {
                    if (!HasComp<BloodBoundTargetComponent>(objective))
                        continue;

                    _targetObjectiveSystem.SetTarget(objective, EntityUid.Invalid);
                }
            }

            _roleSystem.MindRemoveRole<BloodBoundRoleComponent>(mindId);
        }

        int? objectiveToRemove = null;

        var i = 0;
        foreach (var objective in mind.Objectives)
        {
            if (HasComp<ConvertedBloodBoundObjectiveComponent>(objective))
            {
                objectiveToRemove = i;
                break;
            }

            i++;
        }

        if (objectiveToRemove != null)
            _mindSystem.TryRemoveObjective(mindId, mind, objectiveToRemove.Value);
    }
}
