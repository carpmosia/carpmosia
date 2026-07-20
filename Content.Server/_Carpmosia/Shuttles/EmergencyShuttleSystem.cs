using Robust.Shared.Player;
using Robust.Shared.Enums;
using Robust.Server.Player;
using Content.Shared.GameTicking;
using Content.Server.GameTicking;
using Robust.Shared.Network;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;

namespace Content.Server.Shuttles.Systems;

// TODO full game saves
// Move state data into the emergency shuttle component
public sealed partial class EmergencyShuttleSystem
{
    [Dependency] private IPlayerManager _playerManager = default!;
    [Dependency] private IConfigurationManager _cfg = default!;

    private void InitializeEmergencyLobby()
    {
        SubscribeNetworkEvent<RoundEndMessageEvent>(OnRoundEnd);
        _playerManager.PlayerStatusChanged += PlayerStatusChanged;
    }

    private void OnRoundEnd(RoundEndMessageEvent _)
    {
        UpdateReturnToLobby();
    }

    private async void UpdateReturnToLobby()
    {
        var enabled = EmergencyShuttleArrived || _ticker.RunLevel == GameRunLevel.PostRound;
        // cvar change automatically announces to everyone
        _cfg.SetCVar(CCVars.GameDisallowLateJoins, enabled);
    }

    private async void PlayerStatusChanged(object? sender, SessionStatusEventArgs args)
    {
        if (args.NewStatus != SessionStatus.Connected)
            return;

        // clients don't have this cvar, so we have to announce it on join, this is probably an upstream bug
        var enabled = _cfg.GetCVar(CCVars.GameDisallowLateJoins);
        Log.Error("we sent this shitter a status update");
        RaiseNetworkEvent(new TickerLateJoinStatusEvent(enabled), args.Session.Channel);
    }
}
