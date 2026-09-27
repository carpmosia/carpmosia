using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Offbrand.Analyzers;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(StationaryVitalsAnalyzerSystem))]
public sealed partial class StationaryVitalsAnalyzerComponent : Component
{
    // Audio stuff
    [DataField, AutoNetworkedField]
    public EntityUid? LoopingAudio;

    [DataField, AutoNetworkedField]
    public SoundSpecifier? CurrentAudio;

    [DataField]
    public SortedDictionary<float, SoundSpecifier> PulseAudioThresholds;

    [DataField]
    public SoundSpecifier AsystoleAudio;

    // Thresholds for sprites
    [DataField]
    public SortedDictionary<float, VitalsMonitorBrainActivity> BrainActivityThresholds;

    [DataField]
    public SortedDictionary<float, bool> BrainActivityWarningThresholds;

    [DataField]
    public SortedDictionary<float, VitalsMonitorBreathing> BreathingThresholds;

    [DataField]
    public SortedDictionary<float, bool> BreathingWarningThresholds;

    [DataField]
    public SortedDictionary<float, VitalsMonitorPulse> PulseThresholds;

    [DataField]
    public SortedDictionary<float, bool> PulseWarningThresholds;

    [DataField(required: true)]
    public Enum UiKey;
}

[Serializable, NetSerializable]
public enum VitalsMonitorVisuals : byte
{
    BrainActivity,
    BrainActivityWarning,
    Breathing,
    BreathingWarning,
    Pulse,
    PulseWarning,
}

[Serializable, NetSerializable]
public enum VitalsMonitorBrainActivity : byte
{
    Blank,
    Okay,
    Bad,
    VeryBad,
}

[Serializable, NetSerializable]
public enum VitalsMonitorBreathing : byte
{
    Blank,
    Normal,
    Shallow,
}

[Serializable, NetSerializable]
public enum VitalsMonitorPulse : byte
{
    Blank,
    Asystole,
    Normal,
    Fast,
    VentricularTachycardia,
}
