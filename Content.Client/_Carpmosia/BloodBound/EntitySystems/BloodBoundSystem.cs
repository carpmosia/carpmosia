using Content.Shared.Antag;
using Content.Shared.BloodBound.Components;
using Content.Shared.BloodBound.EntitySystems;
using Content.Shared.StatusIcon.Components;
using Robust.Client.Player;

namespace Content.Client.BloodBound.EntitySystems;

public sealed partial class BloodBoundSystem : SharedBloodBoundSystem
{
    [Dependency] private IPlayerManager _playerManager = default!;

    [SubscribeLocalEvent]
    private void OnBloodBoundGetIcons(Entity<BloodBoundComponent> entity, ref GetStatusIconsEvent args)
    {
        if (_playerManager.LocalSession?.AttachedEntity is { } playerEntity)
        {
            if (!HasComp<ShowAntagIconsComponent>(playerEntity) &&
                entity.Owner != playerEntity &&
                entity.Comp.Bound != playerEntity)
                return;
        }

        if (ProtoMan.TryIndex(entity.Comp.BloodBoundIcon, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
    }
}
