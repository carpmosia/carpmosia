using Content.Shared.Body.Events;
using Content.Shared.Damage.Events;
using Content.Shared.Damage.Systems;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.Flash;
using Content.Shared.Mobs.Events;
using Content.Shared.Mobs;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.Rejuvenate;
using Content.Shared.Speech;
using Content.Shared.StatusEffectNew.Components;
using Content.Shared.Stunnable;
using Robust.Shared.Player;

namespace Content.Shared.StatusEffectNew;

public sealed partial class StatusEffectsSystem
{
    private void InitializeRelay()
    {
        SubscribeLocalEvent<StatusEffectContainerComponent, LocalPlayerAttachedEvent>(RelayStatusEffectEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, LocalPlayerDetachedEvent>(RelayStatusEffectEvent);
        // SubscribeLocalEvent<StatusEffectContainerComponent, RejuvenateEvent>(RelayStatusEffectEvent); // Offbrand

        SubscribeLocalEvent<StatusEffectContainerComponent, RefreshMovementSpeedModifiersEvent>(RelayStatusEffectEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, UpdateCanMoveEvent>(RelayStatusEffectEvent);

        // By Ref Events
        SubscribeLocalEvent<StatusEffectContainerComponent, MobStateChangedEvent>(RefRelayStatusEffectEvent);

        SubscribeLocalEvent<StatusEffectContainerComponent, RefreshFrictionModifiersEvent>(RefRelayStatusEffectEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, TileFrictionEvent>(RefRelayStatusEffectEvent);

        SubscribeLocalEvent<StatusEffectContainerComponent, StandUpAttemptEvent>(RefRelayStatusEffectEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, StunEndAttemptEvent>(RefRelayStatusEffectEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, RefreshStaminaCritThresholdEvent>(RefRelayStatusEffectEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, CanSeeAttemptEvent>(RelayStatusEffectEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, FlashAttemptEvent>(RefRelayStatusEffectEvent);

        SubscribeLocalEvent<StatusEffectContainerComponent, BeforeForceSayEvent>(RelayStatusEffectEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, BeforeAlertSeverityCheckEvent>(RelayStatusEffectEvent);

        SubscribeLocalEvent<StatusEffectContainerComponent, AccentGetEvent>(RefRelayStatusEffectEvent);

        SubscribeLocalEvent<StatusEffectContainerComponent, BleedModifierEvent>(RefRelayStatusEffectEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, DamageModifyEvent>(RelayStatusEffectEvent);

        SubscribeLocalEvent<StatusEffectContainerComponent, _Offbrand.Wounds.WoundGetDamageEvent>(RefRelayStatusEffectEvent); // Offbrand
        SubscribeLocalEvent<StatusEffectContainerComponent, _Offbrand.Wounds.GetWoundsWithSpaceEvent>(RefRelayStatusEffectEvent); // Offbrand
        SubscribeLocalEvent<StatusEffectContainerComponent, _Offbrand.Wounds.GetPainEvent>(RefRelayStatusEffectEvent); // Offbrand
        SubscribeLocalEvent<StatusEffectContainerComponent, _Offbrand.Wounds.HealWoundsEvent>(RefRelayStatusEffectEvent); // Offbrand
        SubscribeLocalEvent<StatusEffectContainerComponent, _Offbrand.Wounds.GetBleedLevelEvent>(RefRelayStatusEffectEvent); // Offbrand
        SubscribeLocalEvent<StatusEffectContainerComponent, _Offbrand.Wounds.PainSuppressionEvent>(RefRelayStatusEffectEvent); // Offbrand
        SubscribeLocalEvent<StatusEffectContainerComponent, _Offbrand.Organs.BeforeDealOrganOxygenDamage>(RefRelayStatusEffectEvent); // Offbrand
        SubscribeLocalEvent<StatusEffectContainerComponent, _Offbrand.Organs.BeforeDepleteOrganOxygen>(RefRelayStatusEffectEvent); // Offbrand
        SubscribeLocalEvent<StatusEffectContainerComponent, _Offbrand.Organs.BeforeHealOrganOxygenDamage>(RefRelayStatusEffectEvent); // Offbrand
        SubscribeLocalEvent<StatusEffectContainerComponent, _Offbrand.Wounds.ModifiedVascularToneEvent>(RefRelayStatusEffectEvent); // Offbrand
        SubscribeLocalEvent<StatusEffectContainerComponent, _Offbrand.Wounds.ModifiedLungFunctionEvent>(RefRelayStatusEffectEvent); // Offbrand
        SubscribeLocalEvent<StatusEffectContainerComponent, _Offbrand.Wounds.ModifiedMetabolicRateEvent>(RefRelayStatusEffectEvent); // Offbrand
        SubscribeLocalEvent<StatusEffectContainerComponent, _Offbrand.Wounds.ModifiedCardiacOutputEvent>(RefRelayStatusEffectEvent); // Offbrand
        SubscribeLocalEvent<StatusEffectContainerComponent, _Offbrand.Wounds.ModifiedRespiratoryRateEvent>(RefRelayStatusEffectEvent); // Offbrand
        SubscribeLocalEvent<StatusEffectContainerComponent, Content.Shared._Offbrand.Weapons.RelayedGetMeleeDamageEvent>(RefRelayStatusEffectEvent); // Offbrand
        SubscribeLocalEvent<StatusEffectContainerComponent, Content.Shared._Offbrand.Weapons.RelayedGetMeleeAttackRateEvent>(RefRelayStatusEffectEvent); // Offbrand
        SubscribeLocalEvent<StatusEffectContainerComponent, Content.Shared._Offbrand.Weapons.RelayedGunRefreshModifiersEvent>(RefRelayStatusEffectEvent); // Offbrand
        SubscribeLocalEvent<StatusEffectContainerComponent, ModifySlowOnDamageSpeedEvent>(RefRelayStatusEffectEvent); // Offbrand
        SubscribeLocalEvent<StatusEffectContainerComponent, GetBlurEvent>(RelayStatusEffectEvent); // Offbrand
        SubscribeLocalEvent<StatusEffectContainerComponent, Content.Shared.IdentityManagement.Components.SeeIdentityAttemptEvent>(RelayStatusEffectEvent); // Offbrand
        SubscribeLocalEvent<StatusEffectContainerComponent, Content.Shared.Movement.Pulling.Events.PullStartedMessage>(RelayStatusEffectEvent); // Offbrand
        SubscribeLocalEvent<StatusEffectContainerComponent, Content.Shared.Movement.Pulling.Events.PullStoppedMessage>(RelayStatusEffectEvent); // Offbrand
        SubscribeLocalEvent<StatusEffectContainerComponent, Weapons.Ranged.Events.SelfBeforeGunShotEvent>(RelayStatusEffectEvent); // Offbrand
        SubscribeLocalEvent<StatusEffectContainerComponent, Chemistry.Events.SelfBeforeInjectEvent>(RelayStatusEffectEvent); // Offbrand
        SubscribeLocalEvent<StatusEffectContainerComponent, DamageChangedEvent>(RelayStatusEffectEvent); // Offbrand
        SubscribeLocalEvent<StatusEffectContainerComponent, Examine.ExaminedEvent>(RelayStatusEffectEvent); // Offbrand
        SubscribeLocalEvent<StatusEffectContainerComponent, Verbs.GetVerbsEvent<Verbs.AlternativeVerb>>(RelayStatusEffectEvent); // Offbrand
        SubscribeLocalEvent<StatusEffectContainerComponent, Hands.BeforeEquippingHandEvent>(RefRelayStatusEffectEvent); // Offbrand
    }

    private void RefRelayStatusEffectEvent<T>(EntityUid uid, StatusEffectContainerComponent component, ref T args) where T : struct
    {
        RelayEvent((uid, component), ref args);
    }

    private void RelayStatusEffectEvent<T>(EntityUid uid, StatusEffectContainerComponent component, T args) where T : class
    {
        RelayEvent((uid, component), args);
    }

    public void RelayEvent<T>(Entity<StatusEffectContainerComponent> statusEffect, ref T args) where T : struct
    {
        // this copies the by-ref event if it is a struct
        var ev = new StatusEffectRelayedEvent<T>(args);
        foreach (var activeEffect in statusEffect.Comp.ActiveStatusEffects?.ContainedEntities ?? [])
        {
            RaiseLocalEvent(activeEffect, ref ev);
        }
        // and now we copy it back
        args = ev.Args;
    }

    public void RelayEvent<T>(Entity<StatusEffectContainerComponent> statusEffect, T args) where T : class
    {
        // this copies the by-ref event if it is a struct
        var ev = new StatusEffectRelayedEvent<T>(args);
        foreach (var activeEffect in statusEffect.Comp.ActiveStatusEffects?.ContainedEntities ?? [])
        {
            RaiseLocalEvent(activeEffect, ref ev);
        }
    }
}

/// <summary>
/// Event wrapper for relayed events.
/// </summary>
[ByRefEvent]
public record struct StatusEffectRelayedEvent<TEvent>(TEvent Args);
