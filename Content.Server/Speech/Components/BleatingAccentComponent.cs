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
    /// If a dash will be inserted in the middle of repeated vowels.
    /// Rounds up (baa-ah rather than ba-aah).
    /// </summary>
    [DataField]
    public bool Wobble;
}
