using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     Swaps the emotes menu with an alternative menu
    /// </summary>
    public static readonly CVarDef<bool> AltEmotesMenu =
        CVarDef.Create("hud.alt_emotes_menu", false, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    ///     The required ratio of the server that must agree for a EORG vote to go through.
    /// </summary>
    public static readonly CVarDef<float> VoteEorgRequiredRatio =
        CVarDef.Create("vote.eorg_required_ratio", 0.75f, CVar.SERVERONLY);
}
