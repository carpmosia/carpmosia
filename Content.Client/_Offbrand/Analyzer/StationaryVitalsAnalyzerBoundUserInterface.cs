using Content.Client._Offbrand.Examine;
using Content.Shared._Offbrand.Analyzers;
using Content.Shared.IdentityManagement;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Offbrand.Analyzer;

[UsedImplicitly]
public sealed class StationaryVitalsAnalyzerBoundUserInterface : BoundUserInterface, IExamineEmbeddedUserInterface
{
    private VitalsAnalyzerControl? _control;

    public StationaryVitalsAnalyzerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    public Control CreateExamineControl()
    {
        _control = new();
        Update();
        return _control;
    }

    public override void Update()
    {
        base.Update();

        if (_control is null)
            return;

        if (EntMan.TryGetComponent<VitalsAnalyzerComponent>(Owner, out var vitalsAnalyzer) &&
            vitalsAnalyzer.Data is { } data &&
            EntMan.TryGetComponent<AnalyzerComponent>(Owner, out var analyzer) && analyzer.ActualTarget is { } actualTarget)
        {
            _control.Update((data, actualTarget, analyzer.IsUpdating));
        }
        else
        {
            _control.Update(null);
        }
    }
}
