using Robust.Shared.Random;

namespace Content.Server.Procedural;

public sealed class ConditionalSpawnerSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IEntityManager _entities = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PrefabSpawnerComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, PrefabSpawnerComponent component, MapInitEvent args)
    {
        var coords = _entities.GetComponent<TransformComponent>(uid).Coordinates;

        // Yes this is a bunch of very niche specialized hardcoded stuff
        // but probably in some near future I will come up with an idea on how to generalize this
        var deps = new List<string> { "Arrivals", "Command", "Engineering", "Evac", "Medical", "Science", "Security", "Service" };

        // Pick the center piece, which MUSTN't be Cargo
        SpawnAttachedTo($"Prefab{_random.PickAndTake(deps)}CenterMarker", coords, null);
        deps.Add("Cargo");

        // Build the ring sections around the center
        SpawnAttachedTo($"Prefab{_random.PickAndTake(deps)}LeftMarker", coords, null, Direction.North.ToAngle());
        SpawnAttachedTo($"Prefab{_random.PickAndTake(deps)}RightMarker", coords, null, Direction.North.ToAngle());

        SpawnAttachedTo($"Prefab{_random.PickAndTake(deps)}LeftMarker", coords, null, Direction.East.ToAngle());
        SpawnAttachedTo($"Prefab{_random.PickAndTake(deps)}RightMarker", coords, null, Direction.East.ToAngle());

        SpawnAttachedTo($"Prefab{_random.PickAndTake(deps)}LeftMarker", coords, null, Direction.South.ToAngle());
        SpawnAttachedTo($"Prefab{_random.PickAndTake(deps)}RightMarker", coords, null, Direction.South.ToAngle());

        SpawnAttachedTo($"Prefab{_random.PickAndTake(deps)}LeftMarker", coords, null, Direction.West.ToAngle());
        SpawnAttachedTo($"Prefab{_random.PickAndTake(deps)}RightMarker", coords, null, Direction.West.ToAngle());
    }
}

