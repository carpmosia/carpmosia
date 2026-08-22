using Content.Shared._Offbrand.Analyzers;
using Content.Shared.IdentityManagement;
using Content.Shared._Offbrand.Surgery;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Offbrand.Surgery;

[UsedImplicitly]
public sealed class SurgicalDollPanelBoundUserInterface : BoundUserInterface
{
    private SurgicalDollPanel? _window;

    public SurgicalDollPanelBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<SurgicalDollPanel>();
        Update();
    }

    public override void Update()
    {
        base.Update();

        if (_window is null)
            return;

        if (EntMan.TryGetComponent<SurgeryGuideTargetComponent>(Owner, out var comp))
        {
            _window.SurgicalDoll.Update(comp.Owner);
        }
        else
        {
            _window.SurgicalDoll.Update(null);
        }
    }
}
