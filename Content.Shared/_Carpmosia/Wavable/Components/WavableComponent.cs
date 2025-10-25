namespace Content.Shared.Wavable.Components;

/// <summary>
///     A component added to entities that can be waved.
/// </summary>
[RegisterComponent]
[Access(typeof(SharedWavableSystem))]
public sealed partial class WavableComponent : Component
{
    [DataField]
    public LocId? WavableExamineMessage = "wavable-component-examine";
}
