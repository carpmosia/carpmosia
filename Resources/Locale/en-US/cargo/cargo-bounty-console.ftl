bounty-console-menu-title = Cargo bounty console
bounty-console-label-button-text = Print label
bounty-console-skip-button-text = Skip
# Carpmosia-start - Cargo bookkeeping
bounty-console-access-denied = Insufficient access!
bounty-console-id-required-claim = ID required to claim a bounty!
bounty-console-id-required-status = ID required to change delivery status!
bounty-console-claim-button-claim = Claim
bounty-console-claim-button-unclaim = Unclaim

bounty-console-delivery-status-button-tooltip = Set the current status
bounty-console-delivery-status-undelivered = Undelivered
bounty-console-delivery-status-delivered = Delivered
bounty-console-delivery-status-ready = Ready
bounty-console-delivery-status-label = Status: { $status ->
    *[undelivered] [color=gray]{bounty-console-delivery-status-undelivered}[/color]
    [delivered] [color=orange]{bounty-console-delivery-status-delivered}[/color]
    [ready] [color=limegreen]{bounty-console-delivery-status-ready}[/color]
}

bounty-console-claimed-none = None
bounty-console-claimed-unknown = Unknown
bounty-console-claimed-label = Claimed by: { $claimState ->
    [unclaimed] [color=gray]{bounty-console-claimed-none}[/color]
    [unknown] [color=red]{bounty-console-claimed-unknown}[/color]
    *[claimed] [color=orange]{$crew}[/color]
}
# Carpmosia-end - Cargo bookkeeping
bounty-console-time-label = Time: [color=orange]{$time}[/color]
bounty-console-reward-label = Reward: [color=limegreen]${$reward}[/color]
bounty-console-manifest-label = Manifest: [color=orange]{$item}[/color]
bounty-console-manifest-entry =
    { $amount ->
        [1] {$item}
        *[other] {$item} x{$amount}
    }
bounty-console-manifest-reward = Reward: ${$reward}
bounty-console-description-label = [color=gray]{$description}[/color]
bounty-console-id-label = ID#{$id}

bounty-console-flavor-left = Bounties sourced from local unscrupulous dealers.
bounty-console-flavor-right = v1.4

bounty-manifest-header = [font size=14][bold]Official cargo bounty manifest[/bold] (ID#{$id})[/font]
bounty-manifest-list-start = Item manifest:

bounty-console-tab-available-label = Available
bounty-console-tab-history-label = History
bounty-console-history-empty-label = No bounty history found
bounty-console-history-notice-completed-label = [color=limegreen]Completed[/color]
bounty-console-history-notice-skipped-label = [color=red]Skipped[/color] by {$id}
