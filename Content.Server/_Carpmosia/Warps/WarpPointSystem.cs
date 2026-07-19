using Content.Server.Station.Components;
using Content.Server.Station.Events;
using Content.Shared.Station.Components;
using Content.Shared.Warps;

namespace Content.Server.Warps;

public sealed class WarpPointSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WarpPointComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<MetaDataComponent, StationPostInitEvent>(OnStationPostInitEvent);
    }

    // Sets up warp point components directly on station (Nuke disk, Nuke)
    private void OnMapInit(Entity<WarpPointComponent> ent, ref MapInitEvent _)
    {
        if (string.IsNullOrEmpty(ent.Comp.Location))
            return;

        var gridUid = Transform(ent).GridUid;
        if (!TryComp<BecomesStationComponent>(gridUid, out var bs))
            return;

        ent.Comp.Location = bs.Id + " - " + ent.Comp.Location;
    }

    // Sets up warp point components on subgrids added by the station (ATS and etc)
    private void OnStationPostInitEvent(Entity<MetaDataComponent> ent, ref StationPostInitEvent _)
    {
        List<EntityUid?> stationGrids = [];
        var stationName = Loc.GetString("generic-unknown");

        var smQuery = AllEntityQuery<StationMemberComponent>();
        while (smQuery.MoveNext(out var uid, out var comp))
        {
            if (comp.Station != ent.Owner)
                continue;

            if (TryComp<BecomesStationComponent>(uid, out var bs))
                stationName = bs.Id;
            else
                stationGrids.Add(uid);
        }

        var wpQuery = AllEntityQuery<WarpPointComponent>();
        while (wpQuery.MoveNext(out var uid, out var comp))
        {
            if (Transform(uid).GridUid is not EntityUid some
                    || !stationGrids.Contains(some))
                continue;

            if (string.IsNullOrEmpty(comp.Location))
                continue;

            comp.Location = stationName + " - " + comp.Location;
        }
    }
}
