using Content.Shared._Offbrand.Examine;
using Robust.Client.UserInterface;
using Robust.Shared.Reflection;

namespace Content.Client._Offbrand.Examine;

public sealed partial class ExaminableUserInterfaceSystem : EntitySystem
{
    [Dependency] private IReflectionManager _reflection = default!;
    [Dependency] private SharedUserInterfaceSystem _ui = default!;

    private readonly HashSet<(EntityUid Entity, Enum UiKey, EntityUid Actor)> _openedByExamine = new();
    private readonly HashSet<(EntityUid Entity, Enum UiKey)> _openingByExamine = new();
    private readonly HashSet<(EntityUid Entity, Enum UiKey)> _openByExamine = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExaminableUserInterfaceComponent, ExaminableUserInterfaceRequestEvent>(OnRequest);
        SubscribeLocalEvent<ExaminableUserInterfaceComponent, ExaminableUserInterfaceCloseEvent>(OnClose);
    }

    private void OnRequest(Entity<ExaminableUserInterfaceComponent> ent, ref ExaminableUserInterfaceRequestEvent args)
    {
        if (ent.Comp.RequiresDetailsRange && !args.IsInDetailsRange)
            return;

        var ui = (ent.Owner, CompOrNull<UserInterfaceComponent>(ent.Owner));
        if (!_ui.TryGetInterfaceData(ui, ent.Comp.UiKey, out var data) ||
            !_reflection.TryLooseGetType(data.ClientType, out var type) ||
            !type.IsAssignableTo(typeof(IExamineEmbeddedUserInterface)))
        {
            return;
        }

        var wasOpen = _ui.IsUiOpen(ui, ent.Comp.UiKey, args.Examiner);
        var opened = false;

        if (!wasOpen)
        {
            _openingByExamine.Add((ent.Owner, ent.Comp.UiKey));

            try
            {
                _ui.OpenUi(ui, ent.Comp.UiKey, args.Examiner, predicted: true);
            }
            finally
            {
                _openingByExamine.Remove((ent.Owner, ent.Comp.UiKey));
            }

            opened = _ui.IsUiOpen(ui, ent.Comp.UiKey, args.Examiner) &&
                     _openedByExamine.Add((ent.Owner, ent.Comp.UiKey, args.Examiner));

            if (opened)
                _openByExamine.Add((ent.Owner, ent.Comp.UiKey));
        }

        if (!_ui.TryGetOpenUi(ui, ent.Comp.UiKey, out var bui) || bui is not IExamineEmbeddedUserInterface embedded)
        {
            if (opened)
                CloseExamineUi(ent.Owner, ent.Comp.UiKey, args.Examiner);

            return;
        }

        var control = embedded.CreateExamineControl();
        if (control.Parent != null)
        {
            if (opened)
                CloseExamineUi(ent.Owner, ent.Comp.UiKey, args.Examiner);

            return;
        }

        args.Container.AddChild(control);
    }

    public bool IsOpeningForExamine(EntityUid entity, Enum uiKey)
    {
        return _openingByExamine.Contains((entity, uiKey));
    }

    public bool IsOpenForExamine(EntityUid entity, Enum uiKey)
    {
        return _openByExamine.Contains((entity, uiKey));
    }

    private void OnClose(Entity<ExaminableUserInterfaceComponent> ent, ref ExaminableUserInterfaceCloseEvent args)
    {
        CloseExamineUi(ent.Owner, ent.Comp.UiKey, args.Examiner);
    }

    private void CloseExamineUi(EntityUid entity, Enum uiKey, EntityUid actor)
    {
        if (!_openedByExamine.Remove((entity, uiKey, actor)))
            return;

        _openByExamine.Remove((entity, uiKey));
        _ui.CloseUi((entity, CompOrNull<UserInterfaceComponent>(entity)), uiKey, actor, predicted: true);
    }
}
