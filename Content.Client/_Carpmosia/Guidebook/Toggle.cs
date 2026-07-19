using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;

namespace Content.Client.Guidebook.Richtext;

[UsedImplicitly]
public sealed class Toggle : ContainerButton, IDocumentTag
{
    public Toggle()
    {
        ToggleMode = true;
    }

    public bool TryParseTag(Dictionary<string, string> args, [NotNullWhen(true)] out Control? control)
    {
        DebugTools.Assert(args.Count == 0);
        control = this;
        return true;
    }
}
