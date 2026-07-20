using Content.Server.Station.Components;
using Content.Shared.Station.Components;
using Content.Shared.Warps;

namespace Content.Server.Warps;

public sealed class WarpPointSystem : EntitySystem
{
    private const string DefaultMapName = "Map Entity";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WarpPointComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<WarpPointComponent, EntityUnpausedEvent>(OnComponentStartup);
    }

    private void OnComponentStartup<T>(Entity<WarpPointComponent> ent, ref T _)
    {
        if (Transform(ent.Owner).GridUid is not { } grid)
            return;

        if (TryComp<StationMemberComponent>(grid, out var member))
        {
            if (!TryComp<StationNameSetupComponent>(member.Station, out var name))
                return;

            ent.Comp.Origin = name.ShortName;
        }
        // Fallback for misc maps (CentComm, Terminal, Arrivals)
        else if (Transform(ent.Owner).MapUid is { } map)
        {
            var name = MetaData(map).EntityName.Trim();
            // Fallback for new maps created for Nukeops and Wizard
            if (string.IsNullOrEmpty(name) || name == DefaultMapName)
                return;
            ent.Comp.Origin = name;
        }
    }
}
