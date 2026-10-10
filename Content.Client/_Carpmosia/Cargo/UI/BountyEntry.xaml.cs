using Content.Client.Message;
using Content.Shared.Cargo;

namespace Content.Client.Cargo.UI;

public sealed partial class BountyEntry
{
    public Action? OnClaimButtonPressed;
    public Action<CargoBountyData.CargoBountyStatus>? OnDeliveryStatusChanged;

    private void InitializeCargoBookkeeping(CargoBountyData bounty)
    {
        string claimState;
        if (bounty.ClaimedBy == null)
        {
            claimState = "unclaimed";
        }
        else if (bounty.ClaimedByName == null)
        {
            claimState = "unknown";
        }
        else
        {
            claimState = "claimed";
        }

        ClaimedBy.SetMarkup(Loc.GetString("bounty-console-claimed-label", ("claimState", claimState), ("crew", bounty.ClaimedByName ?? ""))); // ?? "" might cause an issue
        DeliveryStatus.SetMarkup(Loc.GetString("bounty-console-delivery-status-label", ("status", bounty.Status)));

        ClaimButton.Text = bounty.ClaimedBy == null
            ? Loc.GetString("bounty-console-claim-button-claim")
            : Loc.GetString("bounty-console-claim-button-unclaim");

        foreach(var status in Enum.GetValues<CargoBountyData.CargoBountyStatus>())
        {
            var key = $"bounty-console-delivery-status-{status.ToString().ToLowerInvariant()}";
            DeliveryButton.AddItem(Loc.GetString(key), (int) status);
        }

        DeliveryButton.Select((int) bounty.Status);

        ClaimButton.OnPressed += _ => OnClaimButtonPressed?.Invoke();
        DeliveryButton.OnItemSelected += args =>
        {
            DeliveryButton.Select(args.Id);
            OnDeliveryStatusChanged?.Invoke((CargoBountyData.CargoBountyStatus) args.Id);
        };
    }
}
