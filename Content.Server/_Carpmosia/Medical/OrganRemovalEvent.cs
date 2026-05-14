using Content.Shared.Body.Components;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;

namespace Content.Server.Medical;

public sealed partial class OrganRemovalDoAfterEvent : DoAfterEvent
{
    public override DoAfterEvent Clone() => this;
}
