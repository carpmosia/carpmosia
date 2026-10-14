using System.Linq; // Carpmosia-edit - configurable bleating
using System.Text.RegularExpressions;
using Content.Shared.Speech.Components;

namespace Content.Shared.Speech.EntitySystems;

public sealed partial class BleatingAccentSystem : RelayAccentSystem<BleatingAccentComponent>
{
    // Carpmosia-start - configurable bleating
    private static readonly Regex bleat = new("([mbdlpwhrkcnytfo])([aiu])", RegexOptions.IgnoreCase);

    public override string Accentuate(string message, Entity<BleatingAccentComponent>? ent = null)
    {
        // Repeats the vowel in certain consonant-vowel pairs and adds a stop near the end.
        // So you taaa-alk liii-ike thiii-is
        var repeats = ent.HasValue ? ent.Value.Comp.Repeats : 3;
        var stop = ent.HasValue ? ent.Value.Comp.Stop : "'";
        var replacement = "$1" + string.Concat(Enumerable.Repeat("$2", repeats - 1)) + stop + "$2";
        return bleat.Replace(message, replacement);
    }
    // Carpmosia-end - configurable bleating
}
