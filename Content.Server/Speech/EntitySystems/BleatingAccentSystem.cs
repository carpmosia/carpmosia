using System.Linq;
using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Content.Shared.Speech.EntitySystems;
using Robust.Shared.Random;

namespace Content.Server.Speech.EntitySystems;

public sealed partial class BleatingAccentSystem : RelayAccentSystem<BleatingAccentComponent>
{
    [Dependency] private IRobustRandom _random = default!;

    [GeneratedRegex("([mbdlpwhrkcnytfo])([aiu])", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex BleatRegex();

    public override string Accentuate(string message, Entity<BleatingAccentComponent>? ent = null)
    {
        // Repeats the vowel in certain consonant-vowel pairs and adds a dash in the middle (if set).
        // So you taa-aalk lii-iike thii-iis
        var repeatAmount = ent.HasValue ? ent.Value.Comp.Repeats : 3;
        var replacement = "$1"
                          + string.Concat(Enumerable.Repeat("$2", (repeatAmount / 2) + 1))
                          + (ent.HasValue && ent.Value.Comp.Wobble ? "-" : "")
                          + string.Concat(Enumerable.Repeat("$2", repeatAmount - ((repeatAmount / 2) + 1)));
        return BleatRegex().Replace(message, replacement);
    }
}
