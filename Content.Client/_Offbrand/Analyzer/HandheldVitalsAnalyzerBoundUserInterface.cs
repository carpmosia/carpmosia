using Content.Shared._Offbrand.Analyzers;
using Content.Shared.IdentityManagement;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Offbrand.Analyzer;

[UsedImplicitly]
public sealed class HandheldVitalsAnalyzerBoundUserInterface : BoundUserInterface
{
    private VitalsAnalyzerWindow? _window;

    public HandheldVitalsAnalyzerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<VitalsAnalyzerWindow>();
        Update();
    }

    public override void Update()
    {
        base.Update();

        if (_window is null)
            return;

        _window.Title = Identity.Name(Owner, EntMan);
        if (EntMan.TryGetComponent<VitalsAnalyzerComponent>(Owner, out var vitalsAnalyzer) &&
            vitalsAnalyzer.Data is { } data &&
            EntMan.TryGetComponent<AnalyzerComponent>(Owner, out var analyzer) && analyzer.ActualTarget is { } actualTarget)
        {
            _window.VitalsAnalyzer.Update((data, actualTarget, analyzer.IsUpdating));
        }
        else
        {
            _window.VitalsAnalyzer.Update(null);
        }
    }
}
