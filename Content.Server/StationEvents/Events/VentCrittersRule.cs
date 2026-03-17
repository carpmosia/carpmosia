using System.Collections.Generic; // Carpmosia-edit - Vent critter fix
using System.Linq; // Carpmosia-edit - Vent critter fix
using Content.Server.Pinpointer; // Carpmosia-edit - Vent critter fix
using Content.Server.StationEvents.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Station.Components;
using Content.Shared.Storage;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Robust.Shared.Utility; // Carpmosia-edit - Vent critter fix

namespace Content.Server.StationEvents.Events;

public sealed class VentCrittersRule : StationEventSystem<VentCrittersRuleComponent>
{
    /*
     * DO NOT COPY PASTE THIS TO MAKE YOUR MOB EVENT.
     * USE THE PROTOTYPE.
     */

    // Carpmosia-start - Vent critter fix
    [Dependency] private readonly NavMapSystem _navMap = default!;

    protected override void Added(EntityUid uid, VentCrittersRuleComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        PickLocation(component);

        if (!TryComp<StationEventComponent>(uid, out var stationEvent))
            return;

        var str = Loc.GetString("station-event-vent-creatures-start-horde-announcement",
            ("location", FormattedMessage.RemoveMarkupOrThrow(_navMap.GetNearestBeaconString((uid, component.Center)))));
        stationEvent.StartAnnouncement = str;

        base.Added(uid, component, gameRule, args);
    }

    private void PickLocation(VentCrittersRuleComponent component)
    {
        if (!TryGetRandomStation(out var station))
            return;

        var locations = EntityQueryEnumerator<VentCritterSpawnLocationComponent, TransformComponent>();
        var validLocations = new List<TransformComponent>();
        while (locations.MoveNext(out _, out _, out var transform))
        {
            if (!transform.Anchored)
                continue;

            if (CompOrNull<StationMemberComponent>(transform.GridUid)?.Station == station)
            {
                validLocations.Add(transform);
            }
        }

        if (validLocations.Count == 0)
            return;

        var spawnCenter = RobustRandom.Pick(validLocations);
        var spawns = new List<string>();

        // Guaranteed spawn
        if (component.SpecialEntries.Count > 0)
        {
            var special = RobustRandom.Pick(component.SpecialEntries).PrototypeId;
            if (special is not null)
                spawns.Add(special);
        }

        // Emulate original behaviour by trying to spawn per every valid location
        for (var i = 0; i < validLocations.Count; i++)
        {
            spawns.AddRange(EntitySpawnCollection.GetSpawns(component.Entries, RobustRandom));
            if (component.SpecialEntries.Count > 0)
                spawns.AddRange(EntitySpawnCollection.GetSpawns(component.SpecialEntries, RobustRandom));
        }

        component.Center = spawnCenter;
        component.Spawns = spawns;
        component.Locations = validLocations.Select(c => c.Coordinates).OrderBy(c => (spawnCenter.Coordinates.Position - c.Position).Length()).Take(spawns.Count).ToList();
    }
    // Carpmosia-end - Vent critter fix

    protected override void Started(EntityUid uid, VentCrittersRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        // Carpmosia-start - Vent critter fix
        if (component.Center is null)
            return;

        for (var i = 0; i < component.Spawns.Count; i++)
            Spawn(component.Spawns[i], component.Locations[i % component.Locations.Count]);
        // Carpmosia-end - Vent critter fix
    }
}
