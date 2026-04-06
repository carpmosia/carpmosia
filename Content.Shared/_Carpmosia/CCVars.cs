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
