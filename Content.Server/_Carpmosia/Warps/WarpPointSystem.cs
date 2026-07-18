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

    private void OnMapInit(EntityUid ent, WarpPointComponent comp, MapInitEvent _)
    {
        if (string.IsNullOrEmpty(comp.Location))
            return;

        var gridUid = Transform(ent).GridUid;
        if (!TryComp<BecomesStationComponent>(gridUid, out var bs))
            return;

        comp.Location = bs.Id + " - " + comp.Location;
        Log.Error("DOING THIS SHIT FOR " + comp.Location);
    }

    private void OnStationPostInitEvent(EntityUid stationId, MetaDataComponent stationMeta, StationPostInitEvent _)
    {
        List<EntityUid?> stationGrids = [];
        var stationName = "Unknown";

        var smQuery = AllEntityQuery<StationMemberComponent>();
        while (smQuery.MoveNext(out var ent, out var comp))
        {
            if (comp.Station != stationId)
                continue;

            if (TryComp<BecomesStationComponent>(ent, out var bs))
                stationName = bs.Id;
            else
                stationGrids.Add(ent);
        }

        var wpQuery = AllEntityQuery<WarpPointComponent, TransformComponent>();
        while (wpQuery.MoveNext(out var _, out var comp, out var xForm))
        {
            if (xForm?.GridUid is not EntityUid some
                || !stationGrids.Contains(some))
                continue;

            if (string.IsNullOrEmpty(comp.Location))
                continue;

            comp.Location = stationName + " - " + comp.Location;
        }
    }
}
