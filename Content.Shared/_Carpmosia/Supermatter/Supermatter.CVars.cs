using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    /// Time interval between each supermatter update.
    /// </summary>
    public static readonly CVarDef<float> SupermatterUpdateRate =
        CVarDef.Create("game.supermatter_update_rate", 0.5f, CVar.SERVERONLY);

    /// <summary>
    /// The amount of time supermatter can exist below 0 integrity without delaminating. In seconds.
    /// </summary>
    public static readonly CVarDef<float> SupermatterDelaminationTimer =
        CVarDef.Create("game.supermatter_delamination_timer", 45.0f, CVar.SERVERONLY);
}
