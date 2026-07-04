using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.MedicalScannerPlus;

[Serializable, NetSerializable]
public sealed partial class HealthAnalyzerPlusDoAfterEvent : SimpleDoAfterEvent
{
}
