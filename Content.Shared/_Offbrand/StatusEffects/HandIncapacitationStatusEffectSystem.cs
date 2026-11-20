using Content.Shared.Body;
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.StatusEffectNew;
using Content.Shared.StatusEffectNew.Components;

namespace Content.Shared._Offbrand.StatusEffects;

public sealed partial class HandIncapacitationStatusEffectSystem : EntitySystem
{
    [Dependency] private BodySystem _body = default!;
    [Dependency] private SharedHandsSystem _hands = default!;

    [Dependency] private EntityQuery<OrganComponent> _organQuery;
    [Dependency] private EntityQuery<HandOrganComponent> _handQuery;
    [Dependency] private EntityQuery<StatusEffectComponent> _statusEffectQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BodyComponent, BeforeEquippingHandEvent>(_body.RelayEvent);
    }

    [SubscribeLocalEvent]
    private void OnStatusEffectApplied(Entity<HandIncapacitationStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        if (!_statusEffectQuery.TryComp(ent, out var status) || status.AppliedTo is not { } organ)
            return;

        if (!_handQuery.TryComp(organ, out var handOrgan))
            return;

        if (!_organQuery.TryComp(organ, out var organComp) || organComp.Body is not { } body)
            return;

        _hands.TryDrop((body, null), handOrgan.HandID);
    }

    [SubscribeLocalEvent]
    private void OnBeforeEquippingHand(Entity<HandIncapacitationStatusEffectComponent> ent, ref StatusEffectRelayedEvent<BeforeEquippingHandEvent> args)
    {
        if (!_statusEffectQuery.TryComp(ent, out var status) || status.AppliedTo is not { } organ)
            return;

        if (!_handQuery.TryComp(organ, out var handOrgan))
            return;

        if (args.Args.HandId != handOrgan.HandID)
            return;

        args.Args = args.Args with { Cancelled = true };
    }
}
