// using Content.Shared.Speech.EntitySystems; Carpmosia-edit - configurable bleating
using Robust.Shared.GameStates;

namespace Content.Shared.Speech.Components;

/// <summary>
/// Makes this entity speak like a sheep or a goat in all chat messages it sends.
/// Repeats the vowel in certain consonant-vowel pairs so you taaaalk liiiike thiiiis.
/// </summary>
[RegisterComponent, NetworkedComponent]
// Carpmosia-start - configurable bleating
public sealed partial class BleatingAccentComponent : BaseAccentComponent
{
    /// <summary>
    /// Amount of vowel repeats, baaaah.
    /// </summary>
    [DataField]
    public int Repeats = 3;

    /// <summary>
    /// Any characters that will be inserted near the end of the repeated vowels, baaa-ah.
    /// </summary>
    [DataField]
    public string Stop = "'";
}
// Carpmosia-end - configurable bleating
