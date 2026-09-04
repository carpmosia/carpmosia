using Content.Shared.Body.Events;
using Content.Shared.Metabolism;
using Content.Shared.Temperature.Components;
using Content.Shared.Temperature;

namespace Content.Shared._Offbrand.Wounds;

public sealed partial class CryostasisFactorSystem : EntitySystem
{
    [Dependency] private MetabolizerSystem _metabolizer = default!;

    [SubscribeLocalEvent]
    private void OnTemperatureChange(Entity<CryostasisFactorComponent> ent, ref OnTemperatureChangeEvent args)
    {
        _metabolizer.UpdateMetabolicMultiplier(ent);
    }

    [SubscribeLocalEvent]
    private void OnGetMetabolicMultiplier(Entity<CryostasisFactorComponent> ent, ref GetMetabolicMultiplierEvent args)
    {
        if (!TryComp<TemperatureComponent>(ent, out var temp))
            return;

        args.Multiplier *= Math.Max(ent.Comp.TemperatureCoefficient * temp.CurrentTemperature + ent.Comp.TemperatureConstant, 1);
    }
}
