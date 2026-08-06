using System.Linq;
using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Content.Shared.Speech.EntitySystems;

namespace Content.Server.Speech.EntitySystems;

public sealed partial class BleatingAccentSystem : RelayAccentSystem<BleatingAccentComponent>
{
    [GeneratedRegex("([mbdlpwhrkcnytfo])([aiu])", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex BleatRegex();

    public override string Accentuate(string message, Entity<BleatingAccentComponent>? ent = null)
    {
        // Repeats the vowel in certain consonant-vowel pairs and adds a stop near the end.
        // So you taaa-alk liii-ike thiii-is
        var repeatAmount = ent.HasValue ? ent.Value.Comp.Repeats : 4;
        var stop = ent.HasValue ? ent.Value.Comp.Break : string.Empty;
        var replacement = "$1" + string.Concat(Enumerable.Repeat("$2", repeatAmount - 1)) + stop + "$2";
        return BleatRegex().Replace(message, replacement);
    }
}
