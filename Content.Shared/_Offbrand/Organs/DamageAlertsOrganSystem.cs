using Content.Shared._Offbrand.Wounds;
using Content.Shared.Alert;
using Content.Shared.Body;

namespace Content.Shared._Offbrand.Organs;

public sealed partial class DamageAlertsOrganSystem : EntitySystem
{
    [Dependency] private AlertsSystem _alerts = default!;

    [SubscribeLocalEvent]
    private void OnDamageChanged(Entity<DamageAlertsOrganComponent> ent, ref OrganDamageChangedEvent args)
    {
        var lungDamage = Comp<DamageableOrganComponent>(ent);
        var targetAlert = ent.Comp.AlertThresholds.HighestMatch(lungDamage.Damage);

        if (targetAlert == ent.Comp.CurrentAlertThresholdState)
            return;

        ent.Comp.CurrentAlertThresholdState = targetAlert;
        if (Comp<OrganComponent>(ent).Body is not { } body)
            return;

        if (targetAlert is { } alert)
        {
            _alerts.ShowAlert(body, alert);
        }
        else
        {
            _alerts.ClearAlertCategory(body, ent.Comp.AlertCategory);
        }
    }

    [SubscribeLocalEvent]
    private void OnGotInserted(Entity<DamageAlertsOrganComponent> ent, ref OrganGotInsertedEvent args)
    {
        if (ent.Comp.CurrentAlertThresholdState is { } alert)
            _alerts.ShowAlert(args.Target, alert);
    }

    [SubscribeLocalEvent]
    private void OnGotRemoved(Entity<DamageAlertsOrganComponent> ent, ref OrganGotRemovedEvent args)
    {
        _alerts.ClearAlertCategory(args.Target, ent.Comp.AlertCategory);
    }
}
