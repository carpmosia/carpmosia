using Content.Client._Carpmosia.UserInterface.Systems.Emotes.Controls;
using Content.Client._Carpmosia.UserInterface.Systems.Emotes.Windows;
using Content.Client.Gameplay;
using Content.Client.UserInterface.Controls;
using Content.Shared.CCVar;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Input;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Configuration;
using Robust.Shared.Input.Binding;
using Robust.Shared.Utility;

namespace Content.Client.UserInterface.Systems.Emotes;

public sealed partial class EmotesUIController : UIController, IOnStateChanged<GameplayState>
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private EmotesWindow? _altMenu;

    public void OnStateEntered(GameplayState state)
    {
        DebugTools.Assert(_menu == null && _altMenu == null);

        // Setup original menu
        _menu = new SimpleRadialMenu();

        _menu.OnClose += UpdateButton;
        _menu.OnOpen += UpdateButton;

        // Setup alternate menu
        _altMenu = UIManager.CreateWindow<EmotesWindow>();
        LayoutContainer.SetAnchorPreset(_altMenu, LayoutContainer.LayoutPreset.Center);

        _altMenu.OnClose += UpdateButton;
        _altMenu.OnOpen += UpdateButton;

        // Fill in the menus
        UpdateEmotes();

        // Bind key
        CommandBinds.Builder
            .Bind(ContentKeyFunctions.OpenEmotesMenu,
                InputCmdHandler.FromDelegate(_ => ToggleEmotesMenu(false)))
            .Register<EmotesUIController>();
    }

    public void UpdateEmotes()
    {
        if (_menu == null || _altMenu == null)
            return;

        var prototypes = _prototypeManager.EnumeratePrototypes<EmotePrototype>();
        var emotes = GetEmotesByCategory(prototypes);

        _menu.SetButtons(MakeRadialMenuOptions(emotes));
        _altMenu.SetButtons(MakeAltMenuOptions(emotes));
    }

    public void OnStateExited(GameplayState state)
    {
        if (_menu != null)
        {
            _menu.Close();
            _menu = null;
        }

        if (_altMenu != null)
        {
            _altMenu.Close();
            _altMenu = null;
        }

        CommandBinds.Unregister<EmotesUIController>();
    }

    private void ToggleEmotesMenu(bool centered)
    {
        if (_menu == null || _altMenu == null)
            return;

        var isOpen = _menu.IsOpen || _altMenu.IsOpen;

        EmotesButton?.SetClickPressed(!isOpen);

        if (isOpen)
        {
            _menu?.Close();
            _altMenu?.Close();
        }
        else
        {
            // Make sure the emotes are up-to-date
            UpdateEmotes();

            if (_cfg.GetCVar(CCVars.AltEmotesMenu))
            {
                _altMenu.Open();
            }
            else if (centered)
            {
                _menu.OpenCentered();
            }
            else
            {
                _menu.OpenOverMouseScreenPosition();
            }
        }
    }

    private void UpdateButton()
    {
        EmotesButton?.SetClickPressed((_menu?.IsOpen ?? false) || (_altMenu?.IsOpen ?? false));
    }

    private IEnumerable<EmoteButton> MakeAltMenuOptions(Dictionary<EmoteCategory, List<EmotePrototype>> emotesByCategory)
    {
        var list = new List<EmoteButton>();

        foreach (var (key, rawList) in emotesByCategory)
        {
            foreach (var emote in rawList)
            {
                var button = new EmoteButton(emote);
                button.OnPressed += _ => HandleRadialButtonClick(emote);
                list.Add(button);
            }
        }

        return list;
    }
}
