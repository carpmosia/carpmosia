using Content.Shared.Body.Components;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Medical;

[Serializable, NetSerializable]
public sealed partial class OrganRemovalDoAfterEvent : SimpleDoAfterEvent
{
    public override DoAfterEvent Clone() => this;
}
