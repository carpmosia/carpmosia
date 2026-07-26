using Content.Shared.CCVar;
using Robust.Shared.Configuration;

namespace Content.Shared._FarHorizons.Power.Generation.FissionGenerator;

public abstract partial class SharedReactorPartSystem : EntitySystem
{
    [Dependency] private IConfigurationManager _cfg = default!;

    // Put CVars in shared space just so that the guidebook can update live

    public float ReactionRate { get; private set; }
    public float NeutronReactionBias { get; private set; }
    public float ReactionReactant { get; private set; }
    public float ReactionProduct { get; private set; }
    public float StimulatedHeatingFactor { get; private set; }
    public float SpontaneousHeatingFactor { get; private set; }
    public float SpontaneousReactionConsumptionMultiplier { get; private set; }
    public float ReactorPartHotTemp { get; private set; }
    public float ReactorPartBurnTemp { get; private set; }

    /// <summary>
    /// Ratio of product to reactant for reactions
    /// </summary>
    public float ReactionRatio => ReactionReactant != 0 ? (ReactionProduct / ReactionReactant) : 0;

    public override void Initialize()
    {
        base.Initialize();

        InitializeCVars();
    }

    private void InitializeCVars()
    {
        Subs.CVar(_cfg, CCVars.ReactionRate, value => ReactionRate = value, true);
        Subs.CVar(_cfg, CCVars.NeutronReactionBias, value => NeutronReactionBias = value, true);
        Subs.CVar(_cfg, CCVars.ReactionReactant, value => ReactionReactant = value, true);
        Subs.CVar(_cfg, CCVars.ReactionProduct, value => ReactionProduct = value, true);
        Subs.CVar(_cfg, CCVars.StimulatedHeatingFactor, value => StimulatedHeatingFactor = value, true);
        Subs.CVar(_cfg, CCVars.SpontaneousHeatingFactor, value => SpontaneousHeatingFactor = value, true);
        Subs.CVar(_cfg, CCVars.SpontaneousReactionConsumptionMultiplier, value => SpontaneousReactionConsumptionMultiplier = value, true);
        Subs.CVar(_cfg, CCVars.ReactorPartHotTemp, value => ReactorPartHotTemp = value, true);
        Subs.CVar(_cfg, CCVars.ReactorPartBurnTemp, value => ReactorPartBurnTemp = value, true);
    }
}