using Robust.Shared.GameStates;

namespace Content.Shared._Offbrand.Analyzers;

[RegisterComponent, NetworkedComponent]
[Access(typeof(StationaryAnalyzerSystem))]
public sealed partial class StationaryAnalyzerStrapComponent : Component;
