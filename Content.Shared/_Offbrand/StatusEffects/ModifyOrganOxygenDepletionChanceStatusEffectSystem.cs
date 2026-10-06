using Content.Shared._Offbrand.Organs;
using Content.Shared._Offbrand.Wounds;
using Content.Shared.StatusEffectNew.Components;
using Content.Shared.StatusEffectNew;

namespace Content.Shared._Offbrand.StatusEffects;

public sealed partial class ModifyOrganOxygenDepletionChanceStatusEffectSystem : EntitySystem
{
    [Dependency] private PerfusionSystem _perfusion = default!;

    [SubscribeLocalEvent]
    private void OnBeforeDepleteOrganOxygen(Entity<ModifyOrganOxygenDepletionChanceStatusEffectComponent> ent, ref StatusEffectRelayedEvent<BeforeDepleteOrganOxygen> args)
    {
        if (Comp<StatusEffectComponent>(ent).AppliedTo is not { } target)
            return;

        var oxygenation = _perfusion.Spo2(target);
        if (ent.Comp.OxygenationModifierThresholds.LowestMatch(oxygenation) is not { } modifier)
            return;

        args.Args = args.Args with { Chance = args.Args.Chance * modifier };
    }
}
