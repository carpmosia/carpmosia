using Content.Server.Construction;
using Content.Shared.Body;
using Content.Shared._Offbrand.Surgery;

namespace Content.Server._Offbrand.Surgery;

public sealed partial class SurgeryGuideTargetSystem : SharedSurgeryGuideTargetSystem
{
    [Dependency] private ConstructionSystem _construction = default!;
    [Dependency] private EntityQuery<BodyComponent> _bodyQuery;

    protected override void OnStartSurgery(Entity<SurgeryGuideTargetComponent> ent, ref SurgeryGuideStartSurgeryMessage args)
    {
        base.OnStartSurgery(ent, ref args);
        if (!ProtoMan.Resolve(args.Prototype, out var construction))
            return;

        _construction.SetPathfindingTarget(ent, construction.TargetNode);
    }

    protected override void OnStartCleanup(Entity<SurgeryGuideTargetComponent> ent, ref SurgeryGuideStartCleanupMessage args)
    {
        base.OnStartCleanup(ent, ref args);
        if (!_bodyQuery.TryComp(ent, out var body))
            return;

        foreach(var organ in body.Organs?.ContainedEntities ?? [])
        {
            if (organ == null)
                continue;
            var construction = _construction.GetCurrentNode(organ);
            if (construction != null && construction.Name != "Base")
                _construction.SetPathfindingTarget(organ, "Base");
        }
    }
}
