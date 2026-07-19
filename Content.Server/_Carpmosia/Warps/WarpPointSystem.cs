using Content.Server.GameTicking;
using Content.Server.Station.Components;
using Content.Shared.Station.Components;
using Content.Shared.Warps;
using Robust.Shared.Map;

namespace Content.Server.Warps;

public sealed class WarpPointSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WarpPointComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<WarpPointComponent, EntityUnpausedEvent>(OnComponentStartup);
    }

    private void OnComponentStartup<T>(Entity<WarpPointComponent> ent, ref T _)
    {
        if (Transform(ent.Owner).GridUid is not EntityUid grid)
            return;

        if (TryComp<StationMemberComponent>(grid, out var member))
        {
            if (!TryComp<StationNameSetupComponent>(member.Station, out var name))
                return;

            ent.Comp.Origin = name.ShortName;
        }
        // Fallback for misc maps (CentComm, Terminal, Arrivals)
        else if (Transform(ent.Owner).MapUid is EntityUid map)
        {
            var name = MetaData(map).EntityName.Trim();
            // Fallback for new maps created for Nukeops and Wizard
            if (string.IsNullOrEmpty(name) || name == "Map Entity")
                return;
            ent.Comp.Origin = name;
        }
    }


    private void OnRoundStart(GameRunLevelChangedEvent ev)
    {
        if (ev.New != GameRunLevel.InRound)
            return;

        var query = AllEntityQuery<WarpPointComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (string.IsNullOrEmpty(comp.Location))
                continue;

            if (Transform(uid).GridUid is not EntityUid grid)
                continue;

            if (TryComp<StationMemberComponent>(grid, out var member))
            {
                if (!TryComp<StationNameSetupComponent>(member.Station, out var name))
                    continue;

                comp.Location = name.ShortName + " - " + comp.Location;
            }
            // Fallback for misc maps (CentComm, Terminal, Arrivals)
            else if (Transform(uid).MapUid is EntityUid map)
                comp.Location = MetaData(map).EntityName + " - " + comp.Location;
        }
    }
}
