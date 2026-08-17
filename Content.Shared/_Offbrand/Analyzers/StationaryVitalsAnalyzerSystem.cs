using Content.Shared._Offbrand.UserInterface;
using Content.Shared._Offbrand.Wounds;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Network;

namespace Content.Shared._Offbrand.Analyzers;

public sealed partial class StationaryVitalsAnalyzerSystem : EntitySystem
{
    [Dependency] private INetManager _net = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedUserInterfaceSystem _userInterface = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationaryVitalsAnalyzerComponent, AfterAnalyzerUpdatedEvent>(OnAfterUpdated);
        SubscribeLocalEvent<StationaryVitalsAnalyzerComponent, AnalyzerActualTargetUpdatedEvent>(OnTargetUpdated);
    }

    private void OnAfterUpdated(Entity<StationaryVitalsAnalyzerComponent> ent, ref AfterAnalyzerUpdatedEvent args)
    {
        _userInterface.SetUiState(ent.Owner, ent.Comp.UiKey, new DummyBoundUserInterfaceState());

        if (Comp<AnalyzerComponent>(ent).IsUpdating)
            UpdateAppearance(ent);
        else
            ClearAppearance(ent);
    }

    private void OnTargetUpdated(Entity<StationaryVitalsAnalyzerComponent> ent, ref AnalyzerActualTargetUpdatedEvent args)
    {
        if (Comp<AnalyzerComponent>(ent).ActualTarget is null)
            ClearAppearance(ent);
    }

    // TODO: this should be defined in RT
    private bool AudioEquals(SoundSpecifier? a, SoundSpecifier? b)
    {
        return (a, b) switch {
            (SoundPathSpecifier pa, SoundPathSpecifier pb) => pa.Path == pb.Path,
            (SoundCollectionSpecifier ca, SoundCollectionSpecifier cb) => ca.Collection == cb.Collection,
            (null, null) => true,
            _ => false,
        };
    }

    private void UpdateAppearance(Entity<StationaryVitalsAnalyzerComponent> ent)
    {
        if (Comp<VitalsAnalyzerComponent>(ent).Data is not { } scanned)
        {
            ClearAppearance(ent);
            return;
        }

        // Brain activity
        if (ent.Comp.BrainActivityThresholds.LowestMatch(scanned.BrainHealth) is { } brainActivity)
            _appearance.SetData(ent, VitalsMonitorVisuals.BrainActivity, brainActivity);

        if (ent.Comp.BrainActivityWarningThresholds.LowestMatch(scanned.BrainHealth) is { } brainActivtyWarning)
            _appearance.SetData(ent, VitalsMonitorVisuals.BrainActivityWarning, brainActivtyWarning);
        else
            _appearance.SetData(ent, VitalsMonitorVisuals.BrainActivityWarning, false);

        // Breathing
        if (ent.Comp.BreathingThresholds.LowestMatch(scanned.RespiratoryRateModifier) is { } breathing)
            _appearance.SetData(ent, VitalsMonitorVisuals.Breathing, breathing);

        if (ent.Comp.BreathingWarningThresholds.LowestMatch(scanned.RespiratoryRateModifier) is { } breathingWarning)
            _appearance.SetData(ent, VitalsMonitorVisuals.BreathingWarning, breathingWarning);
        else
            _appearance.SetData(ent, VitalsMonitorVisuals.BreathingWarning, false);

        // Pulse
        if (scanned.HeartRate == 0)
        {
            SetAudio(ent, ent.Comp.AsystoleAudio);
            _appearance.SetData(ent, VitalsMonitorVisuals.Pulse, VitalsMonitorPulse.Asystole);
            _appearance.SetData(ent, VitalsMonitorVisuals.PulseWarning, true);
        }
        else
        {
            SetAudio(ent, ent.Comp.PulseAudioThresholds.HighestMatchClass(scanned.HeartStrain));

            if (ent.Comp.PulseThresholds.HighestMatch(scanned.HeartStrain) is { } pulse)
                _appearance.SetData(ent, VitalsMonitorVisuals.Pulse, pulse);

            if (ent.Comp.PulseWarningThresholds.HighestMatch(scanned.HeartStrain) is { } pulseWarning)
                _appearance.SetData(ent, VitalsMonitorVisuals.PulseWarning, pulseWarning);
            else
                _appearance.SetData(ent, VitalsMonitorVisuals.PulseWarning, false);
        }
    }

    private void ClearAppearance(Entity<StationaryVitalsAnalyzerComponent> ent)
    {
        _appearance.SetData(ent, VitalsMonitorVisuals.BrainActivity, VitalsMonitorBrainActivity.Blank);
        _appearance.SetData(ent, VitalsMonitorVisuals.BrainActivityWarning, false);
        _appearance.SetData(ent, VitalsMonitorVisuals.Breathing, VitalsMonitorBreathing.Blank);
        _appearance.SetData(ent, VitalsMonitorVisuals.BreathingWarning, false);
        _appearance.SetData(ent, VitalsMonitorVisuals.Pulse, VitalsMonitorPulse.Blank);
        _appearance.SetData(ent, VitalsMonitorVisuals.PulseWarning, false);
        SetAudio(ent, null);
    }

    private void SetAudio(Entity<StationaryVitalsAnalyzerComponent> ent, SoundSpecifier? audio)
    {
        if (_net.IsClient)
            return;

        if (AudioEquals(ent.Comp.CurrentAudio, audio))
            return;

        if (ent.Comp.LoopingAudio is { } looping)
        {
            ent.Comp.LoopingAudio = _audio.Stop(looping);
            Dirty(ent);
        }

        ent.Comp.CurrentAudio = audio;
        Dirty(ent);

        if (audio is null)
            return;

        ent.Comp.LoopingAudio = _audio.PlayPvs(audio, ent, audio.Params.WithLoop(true))?.Entity;
        Dirty(ent);
    }
}
