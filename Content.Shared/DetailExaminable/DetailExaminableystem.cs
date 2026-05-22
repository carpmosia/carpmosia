// # Carpmosia-rework - Flavor on initial examine
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;

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

        args.PushMarkup($"[italic][color=white]{ent.Comp.Content}[/color][/italic]", priority: -10);
    }
}