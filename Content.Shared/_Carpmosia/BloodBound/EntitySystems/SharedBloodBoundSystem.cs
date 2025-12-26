using Content.Shared.Actions;
using Content.Shared.Antag;
using Content.Shared.BloodBound.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Mindshield.Components;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.GameStates;
using Robust.Shared.Player;

namespace Content.Shared.BloodBound.EntitySystems;

public abstract class SharedBloodBoundSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InitialBloodBoundComponent, MapInitEvent>(OnInitialBloodBoundMapInit);
        SubscribeLocalEvent<InitialBloodBoundComponent, ComponentShutdown>(OnInitialBloodBoundhutdown);
        SubscribeLocalEvent<BloodBoundComponent, ComponentGetStateAttemptEvent>(OnBloodBoundAttemptGetState);
    }

    private void OnInitialBloodBoundMapInit(Entity<InitialBloodBoundComponent> entity, ref MapInitEvent args)
    {
        _actionsSystem.AddAction(entity, ref entity.Comp.ConvertActionEntity, entity.Comp.ConvertAction);
        _actionsSystem.AddAction(entity, ref entity.Comp.CheckConvertActionEntity, entity.Comp.CheckConvertAction);
        Dirty(entity);
    }

    private void OnInitialBloodBoundhutdown(Entity<InitialBloodBoundComponent> entity, ref ComponentShutdown args)
    {
        _actionsSystem.RemoveAction(entity.Comp.ConvertActionEntity);
        _actionsSystem.RemoveAction(entity.Comp.CheckConvertActionEntity);
    }

    private void OnBloodBoundAttemptGetState(
        Entity<BloodBoundComponent> entity,
        ref ComponentGetStateAttemptEvent args)
    {
        args.Cancelled = !CanGetState(args.Player);
    }

    public void OnBloodBoundMindshielded(Entity<MindShieldComponent> entity, ref MapInitEvent args)
    {
        if (HasComp<InitialBloodBoundComponent>(entity))
            return;

        if (!TryComp<BloodBoundComponent>(entity, out var bloodBound))
            return;

        var name = Identity.Entity(entity, EntityManager);
        RemCompDeferred<BloodBoundComponent>(entity);
        if (bloodBound.DeconversionStunTime != null)
            _stunSystem.TryUpdateParalyzeDuration(entity, bloodBound.DeconversionStunTime);
        _popupSystem.PopupEntity(
            Loc.GetString("blood-bound-break-control", ("name", name)),
            entity,
            PopupType.MediumCaution);
    }

    private bool CanGetState(ICommonSession? player)
    {
        //Apparently this can be null in replays so I am just returning true.
        if (player?.AttachedEntity is not {} uid)
            return true;

        return HasComp<BloodBoundComponent>(uid) || HasComp<ShowAntagIconsComponent>(uid);
    }
}
