using System.Numerics;
using Content.Shared.Chat.Prototypes;
using Robust.Client.UserInterface.Controls;
using Robust.Client.Utility;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Client.UserInterface.Systems.Emotes.Controls;

public sealed class EmoteButton : Button
{
    private readonly BoxContainer _box;

    public readonly TextureRect Icon;
    public new readonly RichTextLabel Label;

    public EmoteButton(EmotePrototype emote)
    {
        MinSize = new Vector2(0, 24);
        Margin = new Thickness(1);
        HorizontalAlignment = HAlignment.Left;

        _box = new BoxContainer
        {
            Orientation = LayoutOrientation.Horizontal,
            MinSize = new Vector2(0, 24),
            Margin = new Thickness(1)
        };
        AddChild(_box);

        Icon = new TextureRect
        {
            HorizontalExpand = true,
            VerticalExpand = true,
            HorizontalAlignment = HAlignment.Left,
            VerticalAlignment = VAlignment.Center,
            Stretch = TextureRect.StretchMode.Scale,
            Margin = new Thickness(0, 0, 5, 0),
            TextureScale = new Vector2(1, 1),
            MinSize = new Vector2(24, 24),
            MaxSize = new Vector2(24, 24),
            Visible = true,
            Texture = emote.Icon.Frame0()
        };
        _box.AddChild(Icon);

        Label = new RichTextLabel
        {
            HorizontalExpand = true,
            VerticalExpand = true,
            HorizontalAlignment = HAlignment.Left,
            VerticalAlignment = VAlignment.Center,
            Margin = new Thickness(1),
            Text = Loc.GetString(emote.Name),
            Visible = true
        };
        _box.AddChild(Label);
    }
}
