using System.Numerics;
using Content.Shared.Chat.Prototypes;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface.Controls;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Client.UserInterface.Systems.Emotes.Controls;

public sealed class EmoteButton : Button
{
    public readonly TextureRect Icon;
    public new readonly RichTextLabel Label;

    public EmoteButton(EmotePrototype emote, SpriteSystem sprite)
    {
        MinSize = new Vector2(0, 24);
        Margin = new Thickness(1);
        HorizontalAlignment = HAlignment.Left;

        var box = new BoxContainer
        {
            Orientation = LayoutOrientation.Horizontal,
            MinSize = new Vector2(0, 24),
            Margin = new Thickness(1)
        };
        AddChild(box);

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
            Texture = sprite.Frame0(emote.Icon),
        };
        box.AddChild(Icon);

        Label = new RichTextLabel
        {
            HorizontalExpand = true,
            VerticalExpand = true,
            HorizontalAlignment = HAlignment.Left,
            VerticalAlignment = VAlignment.Center,
            Margin = new Thickness(1),
            Text = Loc.GetString(emote.Name),
            Visible = true,
        };
        box.AddChild(Label);
    }
}
