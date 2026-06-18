namespace Content.Shared.Atmos.Piping.Components; // Carpmosia-edit - Port Vent Crawling

[RegisterComponent]
public sealed partial class GasPipeManifoldComponent : Component
{
    [DataField("inlets")]
    public HashSet<string> InletNames { get; set; } = new() { "south0", "south1", "south2", "south3", "south4" }; // Carpmosia-edit - 5 pipe layers

    [DataField("outlets")]
    public HashSet<string> OutletNames { get; set; } = new() { "north0", "north1", "north2", "north3", "north4" }; // Carpmosia-edit - 5 pipe layers
}
