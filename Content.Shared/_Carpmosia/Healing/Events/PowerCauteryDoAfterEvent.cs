using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Healing.Events;

[Serializable, NetSerializable]
public sealed partial class PowerCauteryDoAfterEvent : SimpleDoAfterEvent
{
}
