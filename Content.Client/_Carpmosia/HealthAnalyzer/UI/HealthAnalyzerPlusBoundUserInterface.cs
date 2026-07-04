using Content.Shared.MedicalScanner;
using Content.Shared.MedicalScannerPlus;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Carpmosia.HealthAnalyzer.UI
{
    [UsedImplicitly]
    public sealed class HealthAnalyzerPlusBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private HealthAnalyzerPlusWindow? _window;

        public HealthAnalyzerPlusBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();

            _window = this.CreateWindow<HealthAnalyzerPlusWindow>();

            _window.Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName;
        }

        protected override void ReceiveMessage(BoundUserInterfaceMessage message)
        {
            if (_window == null)
                return;

            if (message is not HealthAnalyzerPlusScannedUserMessage cast)
                return;

            _window.Populate(cast);
        }
    }
}
