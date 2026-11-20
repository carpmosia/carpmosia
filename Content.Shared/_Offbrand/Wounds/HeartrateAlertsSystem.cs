using Content.Shared._Offbrand.Organs;
using Content.Shared.Alert;

namespace Content.Shared._Offbrand.Wounds;

public sealed partial class HeartrateAlertsSystem : EntitySystem
{
    [Dependency] private AlertsSystem _alerts = default!;

    [SubscribeLocalEvent]
    private void OnMapInit(Entity<HeartrateAlertsComponent> ent, ref MapInitEvent args)
    {
        UpdateAlert(ent);
    }

    [SubscribeLocalEvent]
    private void OnAfterStrainChanged(Entity<HeartrateAlertsComponent> ent, ref AfterStrainChangedEvent args)
    {
        UpdateAlert(ent);
    }

    [SubscribeLocalEvent]
    private void OnComponentShutdown(Entity<HeartrateAlertsComponent> ent, ref ComponentShutdown args)
    {
        _alerts.ClearAlertCategory(ent.Owner, ent.Comp.AlertCategory);
    }

    [SubscribeLocalEvent]
    private void OnHeartStopped(Entity<HeartrateAlertsComponent> ent, ref HeartStoppedEvent args)
    {
        ent.Comp.Beating = false;
        Dirty(ent);
        UpdateAlert(ent);
    }

    [SubscribeLocalEvent]
    private void OnHeartStarted(Entity<HeartrateAlertsComponent> ent, ref HeartStartedEvent args)
    {
        ent.Comp.Beating = true;
        Dirty(ent);
        UpdateAlert(ent);
    }

    private void UpdateAlert(Entity<HeartrateAlertsComponent> ent)
    {
        var perfusion = Comp<PerfusionComponent>(ent);
        if (ent.Comp.Beating)
        {
            var range = _alerts.GetSeverityRange(ent.Comp.StrainAlert);
            var min = _alerts.GetMinSeverity(ent.Comp.StrainAlert);
            var max = _alerts.GetMaxSeverity(ent.Comp.StrainAlert);

            var severity = Math.Min(min + (short)Math.Round(range * perfusion.Strain), max);
            _alerts.ShowAlert(ent.Owner, ent.Comp.StrainAlert, (short)severity);
        }
        else
        {
            _alerts.ShowAlert(ent.Owner, ent.Comp.StoppedAlert);
        }
    }
}
