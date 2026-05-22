// # Carpmosia-rework - Flavor on initial examine
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Robust.Shared.Utility;

namespace Content.Shared.DetailExaminable;

public sealed class DetailExaminableSystem : EntitySystem
{ 
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DetailExaminableComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<DetailExaminableComponent> ent, ref ExaminedEvent args)
    {
        if (Identity.Name(args.Examined, EntityManager) != MetaData(args.Examined).EntityName)
            return;

        if (!args.IsInDetailsRange)
            return;

        var message = FormattedMessage.FromMarkupPermissive($"[italic][color=#c8c8c8]{ent.Comp.Content}[/color][/italic]");
        args.PushMessage(message, priority: -10);
    }
}