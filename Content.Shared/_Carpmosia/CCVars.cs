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
    /// Whether the no EORG popup is enabled.
    /// </summary>
    public static readonly CVarDef<bool> EorgPopupEnabled =
        CVarDef.Create("game.eorg_popup_enabled", true, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Skip the no EORG popup.
    /// </summary>
    public static readonly CVarDef<bool> SkipEorgPopup =
        CVarDef.Create("game.skip_eorg_popup", false, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// How long to display the EORG popup for.
    /// </summary>
    public static readonly CVarDef<float> EorgPopupTime =
        CVarDef.Create("game.eorg_popup_time", 5f, CVar.SERVER | CVar.REPLICATED);
}
