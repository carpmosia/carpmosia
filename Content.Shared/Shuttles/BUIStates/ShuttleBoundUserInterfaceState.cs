using Content.Shared.Shuttles.UI.MapObjects;
using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.BUIStates;

[Serializable, NetSerializable]
public sealed class ShuttleBoundUserInterfaceState : BoundUserInterfaceState
{
    public NavInterfaceState NavState;
    public ShuttleMapInterfaceState MapState;
    public DockingInterfaceState DockState;
    public bool FTLAllowed; // Carpmosia-edit - Ban civvie FTL

    public ShuttleBoundUserInterfaceState(NavInterfaceState navState, ShuttleMapInterfaceState mapState, DockingInterfaceState dockState, bool ftlAllowed) // Carpmosia-edit - Ban civvie FTL
    {
        NavState = navState;
        MapState = mapState;
        DockState = dockState;
        FTLAllowed = ftlAllowed; // Carpmosia-edit - Ban civvie FTL
    }
}
