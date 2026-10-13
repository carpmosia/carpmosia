using Content.Client.Decals.UI;
using Content.Client.Gameplay;
using Content.Client.Sandbox;
using Content.Shared.Decals;
using Robust.Client.Placement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client.MappingHelper;

public sealed class MappingHelperUIController : UIController, IOnStateExited<GameplayState>
{
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly IPlacementManager _placement = default!;

    private MappingHelperWindow? _window;

    public void ToggleWindow()
    {
        EnsureWindow();

        if (_window!.IsOpen)
        {
            _window.Close();
        }
        else
        {
            _window.Open();
        }
    }

    public void OnStateExited(GameplayState state)
    {
        if (_window == null)
            return;
        _window.Dispose();
        _window = null;
    }

    private void EnsureWindow()
    {
        if (_window is { Disposed: false })
            return;
        _window = UIManager.CreateWindow<MappingHelperWindow>();
        _window.OnClose += WindowClosed;
    }

    public void CloseWindow()
    {
        if (_window == null || _window.Disposed)
            return;

        _window?.Close();
    }

    private void WindowClosed()
    {
        if (_window == null || _window.Disposed)
            return;

        _placement.Clear();
    }
}
