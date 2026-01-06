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

    /// <summary>
    ///     The time in seconds that the server should wait before restarting the round during EORG.
    ///     Defaults to 2 minutes.
    /// </summary>
    public static readonly CVarDef<float> EorgRoundRestartTime =
        CVarDef.Create("game.eorg_round_restart_time", 120f, CVar.SERVERONLY);

    /// <summary>
    /// The number of shared moods to give thaven by default.
    /// </summary>
    public static readonly CVarDef<uint> ThavenSharedMoodCount =
        CVarDef.Create<uint>("thaven.shared_mood_count", 1, CVar.SERVERONLY);
}
