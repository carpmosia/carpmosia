using Robust.Shared.Player;
using Robust.Shared.Enums;
using Robust.Server.Player;
using Content.Shared.GameTicking;
using Content.Server.GameTicking;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Content.Server.Shuttles.Systems;

namespace Content.Server.ReturnToLobby;

public sealed partial class ReturnToLobbySystem : EntitySystem
{
    [Dependency] private IPlayerManager _playerManager = default!;
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private GameTicker _ticker = default!;
    [Dependency] private EmergencyShuttleSystem _shuttle = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundEndMessageEvent>((_) => UpdateStatus());
        SubscribeLocalEvent<RoundRestartCleanupEvent>((_) => UpdateStatus());
        _playerManager.PlayerStatusChanged += PlayerStatusChanged;
    }

    public void UpdateStatus()
    {
        var enabled = _shuttle.EmergencyShuttleArrived && _ticker.RunLevel == GameRunLevel.InRound || _ticker.RunLevel == GameRunLevel.PostRound;
        // cvar change automatically announces to everyone
        _cfg.SetCVar(CCVars.GameDisallowLateJoins, enabled);
    }

    private void PlayerStatusChanged(object? sender, SessionStatusEventArgs args)
    {
        if (args.NewStatus != SessionStatus.Connected)
            return;

        // clients don't have this cvar, so we have to announce it on join, this is probably an upstream bug
        var enabled = _cfg.GetCVar(CCVars.GameDisallowLateJoins);
        RaiseNetworkEvent(new TickerLateJoinStatusEvent(enabled), args.Session.Channel);
    }
}
