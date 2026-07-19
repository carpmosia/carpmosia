using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Utility;

namespace Content.Client.Guidebook.RichText;

[UsedImplicitly]
public sealed class FakeLinkTag : IMarkupTagHandler
{
    public static Color LinkColor => Color.CornflowerBlue;

    public string Name => "fakelink";

    /// <inheritdoc/>
    public bool TryCreateControl(MarkupNode node, [NotNullWhen(true)] out Control? control)
    {
        if (!node.Value.TryGetString(out var text))
        {
            control = null;
            return false;
        }

        var label = new Label();
        label.Text = text;

        label.MouseFilter = Control.MouseFilterMode.Pass;
        label.FontColorOverride = LinkColor;
        label.DefaultCursorShape = Control.CursorShape.Hand;

        label.OnMouseEntered += _ => label.FontColorOverride = Color.LightSkyBlue;
        label.OnMouseExited += _ => label.FontColorOverride = Color.CornflowerBlue;

        control = label;
        return true;
    }
}
