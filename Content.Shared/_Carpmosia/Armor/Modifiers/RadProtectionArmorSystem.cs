using Content.Shared.Inventory.Events;
using Content.Shared.Medical;
using Content.Shared.StatusEffectNew;
using Content.Shared.StatusEffectNew.Components;
using Robust.Shared.Timing;

namespace Content.Shared._Carpmosia.Armor.Modifiers;

/// <summary>
/// System defining how radiation protecting armors should work.
/// </summary>
public sealed partial class RadProtectionArmorSystem : EntitySystem
{
    [Dependency] private EntityQuery<RadiationThresholdsComponent> _radThresholdQuery = default!;

    [SubscribeLocalEvent]
    private void OnEquipped(Entity<RadProtectionArmorComponent> ent, ref GotEquippedEvent args)
    {
        if(!_radThresholdQuery.TryComp(args.EquipTarget, out var threshold))
            return;

        threshold.Modifier *= ent.Comp.Modifier;
    }

    [SubscribeLocalEvent]
    private void OnUnequipped(Entity<RadProtectionArmorComponent> ent, ref GotUnequippedEvent args)
    {
        if(!_radThresholdQuery.TryComp(args.EquipTarget, out var threshold))
            return;

        // This Math.Min should never matter but better safe than sorry
        threshold.Modifier = Math.Min(threshold.Modifier/ent.Comp.Modifier, 1f);
    }
}
