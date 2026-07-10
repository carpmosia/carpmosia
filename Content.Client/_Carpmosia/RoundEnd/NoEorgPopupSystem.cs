using Content.Shared.CCVar;
using Content.Shared.GameTicking;
using Robust.Shared.Configuration;

namespace Content.Client.RoundEnd;

public sealed partial class NoEorgPopupSystem : EntitySystem
{
    [Dependency] private IConfigurationManager _cfg = default!;

    private NoEorgPopup? _window;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<RoundEndMessageEvent>(OnRoundEnd);
    }

    private void OnRoundEnd(RoundEndMessageEvent ev)
    {
        if (_cfg.GetCVar(CCVars.SkipEorgPopup) || _cfg.GetCVar(CCVars.EorgPopupEnabled) == false)
            return;

        OpenNoEorgPopup();
    }

    private void OpenNoEorgPopup()
    {
        if (_window != null)
            return;

        _window = new NoEorgPopup();
        _window.OpenCentered();
        _window.OnClose += () => _window = null;
    }
}
