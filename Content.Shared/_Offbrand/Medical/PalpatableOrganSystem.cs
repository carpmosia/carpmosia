using Content.Shared._Offbrand.Skeletons;
using Content.Shared._Offbrand.Wounds;
using Content.Shared.Body;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Localizations;
using Content.Shared.StatusEffectNew;
using Content.Shared.StatusEffectNew.Components;
using Robust.Shared.Utility;

namespace Content.Shared._Offbrand.Medical;

public sealed partial class PalpatableOrganSystem : EntitySystem
{
    [Dependency] private ExamineSystemShared _examine = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private StatusEffectsSystem _statusEffects = default!;

    [Dependency] private EntityQuery<ParentOrganComponent> _parentOrganQuery;
    [Dependency] private EntityQuery<OrganComponent> _organQuery;
    [Dependency] private EntityQuery<PerfusionComponent> _perfusionQuery;
    [Dependency] private EntityQuery<StatusEffectComponent> _statusEffectQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StatusEffectContainerComponent, PalpationEvent>(_statusEffects.RelayEvent);
    }

    [SubscribeLocalEvent]
    private void OnActivateInWorld(Entity<PalpatableOrganComponent> ent, ref ActivateInWorldEvent args)
    {
        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, ent.Comp.Delay, new PalpationDoAfterEvent(), ent, target: ent, used: ent)
        {
            DuplicateCondition = DuplicateConditions.SameEvent | DuplicateConditions.SameTarget,
            BreakOnMove = true,
            BreakOnHandChange = false,
        });
    }

    [SubscribeLocalEvent]
    private void OnPalpation(Entity<PalpationDescriptionComponent> ent, ref PalpationEvent args)
    {
        AddDescription(ent, ent, ref args);
    }

    [SubscribeLocalEvent]
    private void OnRelayedPalpation(Entity<PalpationDescriptionComponent> ent,
        ref StatusEffectRelayedEvent<PalpationEvent> args)
    {
        if (_statusEffectQuery.Comp(ent).AppliedTo is not { } appliedTo)
            return;

        var ev = args.Args;
        AddDescription(ent, appliedTo, ref ev);
        args.Args = ev;
    }

    private void AddDescription(Entity<PalpationDescriptionComponent> ent, EntityUid organ, ref PalpationEvent args) // TODO: see if this can be replaced by having OnRelayedPalpation call OnPalpation
    {
        args.Messages.Add(Loc.GetString(ent.Comp.Description, ("organ", organ)));
    }

    [SubscribeLocalEvent]
    private void OnDoAfter(Entity<PalpatableOrganComponent> ent, ref PalpationDoAfterEvent args)
    {
        if (args.Handled || args.Target is null || args.Cancelled || _organQuery.Comp(ent).Body is not { } body)
            return;

        var ev = new PalpationEvent(new());

        RaiseLocalEvent(ent, ref ev);
        CheckPulse(ent, ref ev); // not a subscription to avoid child organs double-reporting pulse

        if (_parentOrganQuery.TryComp(ent, out var parent))
        {
            foreach (var child in parent.Children)
            {
                if (Exists(child) && HasComp<InternalChildOrganComponent>(child))
                    RaiseLocalEvent(child, ref ev);
            }
        }

        if (ev.Messages.Count == 0)
            _examine.ElaborateExamineTooltip(args.User, ExaminationKeys.Palpation, FormattedMessage.FromMarkupOrThrow(Loc.GetString("palpation-nothing", ("target", Identity.Entity(body, EntityManager)), ("organ", ent))));
        else
            _examine.ElaborateExamineTooltip(args.User, ExaminationKeys.Palpation, FormattedMessage.FromMarkupOrThrow(Loc.GetString("palpation-feels", ("feels", ContentLocalizationManager.FormatList(ev.Messages)), ("target", Identity.Entity(body, EntityManager)), ("organ", ent))));
    }

    private void CheckPulse(Entity<PalpatableOrganComponent> ent, ref PalpationEvent args)
    {
        if (_organQuery.Comp(ent).Body is not { } body)
            return;

        if (!_perfusionQuery.TryComp(body, out var perfusion))
            return;

        if (ent.Comp.PulseQualities.HighestMatch(perfusion.Perfusion) is not { } quality)
            return;

        if (ent.Comp.PulseSpeeds.HighestMatch(perfusion.Strain) is not { } speeds)
            return;

        args.Messages.Add(Loc.GetString(speeds, ("quality", quality)));
    }
}
