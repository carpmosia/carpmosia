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

    /// <summary>
    /// Whenever new player join alerts should be sent to admin chat in Discord
    /// </summary>
    public static readonly CVarDef<bool> AdminChatAlertNewjoin =
        CVarDef.Create("admin.chat_alert_newjoin", true, CVar.SERVERONLY);


    /// <summary>
    /// Changes the overall rate of reaction events
    /// </summary>
    public static readonly CVarDef<float> ReactionRate =
        CVarDef.Create("reactor.reaction_rate", 10f, CVar.SERVERONLY);

    /// <summary>
    /// Changes the likelyhood of neutron interactions
    /// </summary>
    public static readonly CVarDef<float> NeutronReactionBias =
        CVarDef.Create("reactor.neutron_reaction_bias", 1f, CVar.SERVERONLY);

    /// <summary>
    /// The amount of a property consumed by a reaction
    /// </summary>
    public static readonly CVarDef<float> ReactionReactant =
        CVarDef.Create("reactor.reaction_reactant", 0.01f, CVar.SERVERONLY);

    /// <summary>
    /// The amount of a property resultant from a reaction
    /// </summary>
    public static readonly CVarDef<float> ReactionProduct =
        CVarDef.Create("reactor.reaction_product", 0.005f, CVar.SERVERONLY);

    /// <summary>
    /// Multiplier for heating from neutron stimulated reactions
    /// </summary>
    public static readonly CVarDef<float> StimulatedHeatingFactor =
        CVarDef.Create("reactor.stimulated_heating_factor", 1f, CVar.SERVERONLY);

    /// <summary>
    /// Multiplier for heating from spontaneous reactions
    /// </summary>
    public static readonly CVarDef<float> SpontaneousHeatingFactor =
        CVarDef.Create("reactor.spontaneous_heating_factor", 1f, CVar.SERVERONLY);

    /// <summary>
    /// Multiplier for how much reactant/product is consumed/produced in spontaneous reactions
    /// </summary>
    public static readonly CVarDef<float> SpontaneousReactionConsumptionMultiplier =
        CVarDef.Create("reactor.spontaneous_reaction_consumption_multiplier", 1f, CVar.SERVERONLY);

    /// <summary>
    /// Temperature (in C) when people's bare hands can be burnt
    /// </summary>
    public static readonly CVarDef<float> ReactorPartHotTemp =
        CVarDef.Create("reactor.part_hot_temp", 80f, CVar.SERVERONLY);

    /// <summary>
    /// Temperature (in C) when insulated gloves can no longer protect
    /// </summary>
    public static readonly CVarDef<float> ReactorPartBurnTemp =
        CVarDef.Create("reactor.part_burn_temp", 400f, CVar.SERVERONLY);
}
