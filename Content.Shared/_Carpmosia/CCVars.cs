using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     The required ratio of the server that must agree for a EORG vote to go through.
    /// </summary>
    public static readonly CVarDef<float> VoteEorgRequiredRatio =
        CVarDef.Create("vote.eorg_required_ratio", 0.75f, CVar.SERVERONLY);
}
