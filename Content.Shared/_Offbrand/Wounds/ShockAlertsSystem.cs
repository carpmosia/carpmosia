using Content.Shared.Alert;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._Offbrand.Wounds;

public sealed partial class ShockAlertsSystem : EntitySystem
{
    [Dependency] private AlertsSystem _alerts = default!;
    [Dependency] private PainSystem _pain = default!;

    [SubscribeLocalEvent]
    private void OnMapInit(Entity<ShockAlertsComponent> ent, ref MapInitEvent args)
    {
        UpdateAlert(ent);
    }

    [SubscribeLocalEvent]
    private void OnComponentShutdown(Entity<ShockAlertsComponent> ent, ref ComponentShutdown args)
    {
        _alerts.ClearAlertCategory(ent.Owner, ent.Comp.AlertCategory);
    }

    [SubscribeLocalEvent]
    private void OnAfterShockChange(Entity<ShockAlertsComponent> ent, ref AfterShockChangeEvent args)
    {
        UpdateAlert(ent);
    }

    private ProtoId<AlertPrototype>? DetermineThreshold(Entity<ShockAlertsComponent> ent)
    {
        var shock = FixedPoint2.Max(_pain.GetShock(ent.Owner), FixedPoint2.Zero);

        if (Comp<PainComponent>(ent).Suppressed)
            return ent.Comp.SuppressedAlert;

        return ent.Comp.Thresholds.HighestMatch(shock);
    }

    private void UpdateAlert(Entity<ShockAlertsComponent> ent)
    {
        var targetEffect = DetermineThreshold(ent);
        if (targetEffect == ent.Comp.CurrentThresholdState)
            return;

        ent.Comp.CurrentThresholdState = targetEffect;

        if (targetEffect is { } effect)
        {
            _alerts.ShowAlert(ent.Owner, effect);
        }
        else
        {
            _alerts.ClearAlertCategory(ent.Owner, ent.Comp.AlertCategory);
        }
    }
}
