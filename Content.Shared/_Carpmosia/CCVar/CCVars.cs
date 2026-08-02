using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    /// Disables the new emotes menu
    /// </summary>
    public static readonly CVarDef<bool> OldEmotesMenu =
        CVarDef.Create("hud.old_emotes_menu", false, CVar.CLIENTONLY | CVar.ARCHIVE);

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

    /// <summary>
    ///     Enables HV cable docking
    /// </summary>
    public static readonly CVarDef<bool> DockCableHV =
        CVarDef.Create("dock.cable_hv", true, CVar.SERVERONLY);

    /// <summary>
    ///     Enables MV cable docking
    /// </summary>
    public static readonly CVarDef<bool> DockCableMV =
        CVarDef.Create("dock.cable_mv", false, CVar.SERVERONLY);

    /// <summary>
    ///     Enables LV cable docking
    /// </summary>
    public static readonly CVarDef<bool> DockCableLV =
        CVarDef.Create("dock.cable_lv", false, CVar.SERVERONLY);

    /// <summary>
    ///     Enables pipe docking
    /// </summary>
    public static readonly CVarDef<bool> DockPipes =
        CVarDef.Create("dock.pipes", true, CVar.SERVERONLY);
}
