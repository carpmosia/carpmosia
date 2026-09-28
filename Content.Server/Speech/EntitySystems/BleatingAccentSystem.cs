using System.Linq; // Carpmosia-edit - configurable bleating
using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Content.Shared.Speech.EntitySystems;

namespace Content.Server.Speech.EntitySystems;

public sealed partial class BleatingAccentSystem : RelayAccentSystem<BleatingAccentComponent>
{
    // Carpmosia-start - configurable bleating
    [GeneratedRegex("([mbdlpwhrkcnytfo])([aiu])", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex BleatRegex();
    // Carpmosia-end - configurable bleating

    public override string Accentuate(string message, Entity<BleatingAccentComponent>? ent = null)
    {
        // Carpmosia-start - configurable bleating
        // Repeats the vowel in certain consonant-vowel pairs and adds a stop near the end.
        // So you taaa-alk liii-ike thiii-is
        var repeats = ent.HasValue ? ent.Value.Comp.Repeats : 3;
        var stop = ent.HasValue ? ent.Value.Comp.Stop : "'";
        var replacement = "$1" + string.Concat(Enumerable.Repeat("$2", repeats - 1)) + stop + "$2";
        return BleatRegex().Replace(message, replacement);
        // Carpmosia-end - configurable bleating
    }
}
