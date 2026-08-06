using Content.Shared.Speech.Components;

namespace Content.Server.Speech.Components;

/// <summary>
/// Makes this entity speak like a sheep or a goat in all chat messages it sends.
/// </summary>
[RegisterComponent]
public sealed partial class BleatingAccentComponent : BaseAccentComponent
{
    /// <summary>
    /// Amount of vowel repeats, baaaah.
    /// </summary>
    [DataField]
    public int Repeats = 4;

    /// <summary>
    /// A character (if any) that will be inserted near the end of the repeated vowels, baaa'ah.
    /// </summary>
    [DataField]
    public string? Stop;
}
