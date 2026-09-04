using Content.Shared.Hands;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;

namespace Content.Shared._Offbrand.Weapons;

public sealed partial class HeldGunModifierRefreshSystem : EntitySystem
{
    [Dependency] private SharedGunSystem _gun = default!;

    [SubscribeLocalEvent]
    private void OnGotEquippedHand(Entity<GunComponent> ent, ref GotEquippedHandEvent args)
    {
        _gun.RefreshModifiers(ent.AsNullable());
    }

    [SubscribeLocalEvent]
    private void OnGotUnequippedHand(Entity<GunComponent> ent, ref GotUnequippedHandEvent args)
    {
        _gun.RefreshModifiers(ent.AsNullable());
    }
}

[ByRefEvent]
public record struct RelayedGunRefreshModifiersEvent(GunRefreshModifiersEvent Args);
