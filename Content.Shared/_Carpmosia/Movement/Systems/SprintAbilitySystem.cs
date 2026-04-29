using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Gravity;
using Content.Shared.Movement.Components;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Content.Shared.StatusEffectNew;
using Content.Shared._Carpmosia.Movement.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Events;

namespace Content.Shared._Carpmosia.Movement.Systems;

/// <summary>
/// System for adding effects to an actor entity at the cost of stamina damage, if applicable.
/// </summary>
public sealed partial class SprintAbilitySystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedGravitySystem _gravity = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SprintAbilityComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<SprintAbilityComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SprintAbilityComponent, SprintAbilityEvent>(OnSprintAction);
    }

    private void OnInit(Entity<SprintAbilityComponent> entity, ref MapInitEvent args)
    {
        if (!TryComp(entity, out ActionsComponent? comp))
            return;

        _actions.AddAction(entity, ref entity.Comp.ActionEntity, entity.Comp.Action, component: comp);
    }

    private void OnShutdown(Entity<SprintAbilityComponent> entity, ref ComponentShutdown args)
    {
        _actions.RemoveAction(entity.Owner, entity.Comp.ActionEntity);
    }

    private void OnSprintAction(Entity<SprintAbilityComponent> ent, ref SprintAbilityEvent args)
    {
        // check if we're in a state to sprint
        if (_gravity.IsWeightless(args.Performer) || _standing.IsDown(args.Performer))
        {
            if (ent.Comp.SprintFailedPopup != null)
                _popup.PopupClient(Loc.GetString(ent.Comp.SprintFailedPopup.Value), args.Performer, args.Performer);
            return;
        }

            _status.TryAddStatusEffectDuration(ent, ent.Comp.StatusEffect, ent.Comp.Duration);

        // check if we have the concept of stamina
        if (TryComp(args.Performer, out StaminaComponent? stam))
        {   // tank our stamina
            _stamina.TakeStaminaDamage(args.Performer, ent.Comp.StaminaCost);
        }

        // audio
        _audio.PlayPredicted(ent.Comp.JumpSound, args.Performer, args.Performer);

        args.Handled = true;
    }
}
