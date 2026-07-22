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
    /// Whenever new player join alerts should be sent to admin chat in Discord
    /// </summary>
    public static readonly CVarDef<bool> AdminChatAlertNewjoin =
        CVarDef.Create("admin.chat_alert_newjoin", true, CVar.SERVERONLY);

    /// <summary>
    ///     Prototype to use for map pool for terminal stations.
    /// </summary>
    public static readonly CVarDef<string> GameMapPoolTerminal =
        CVarDef.Create("game.map_pool_terminal", "DefaultTerminalPool", CVar.SERVERONLY);

    /// <summary>
    /// Whenever the lobby auto vote is enabled
    /// </summary>
    public static readonly CVarDef<bool> GameLobbyAutoVote =
        CVarDef.Create("game.lobby_auto_vote", false, CVar.SERVERONLY);
}
