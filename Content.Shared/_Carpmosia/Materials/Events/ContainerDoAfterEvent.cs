using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Materials;
[Serializable, NetSerializable]
public sealed partial class ContainerDoAfterEvent : SimpleDoAfterEvent { }
