using Content.Shared.Examine;

namespace Content.Shared._Carpmosia.Supermatter;

public sealed partial class SharedSupermatterSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<SupermatterComponent, ExaminedEvent>(OnExamined);
    }

    public void OnExamined(Entity<SupermatterComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.Integrity < 0f)
        {
            if (ent.Comp.Integrity < -50f)
            {
                args.PushMarkup(Loc.GetString("supermatter-examined-delamination-late"));
                return;
            }

            args.PushMarkup(Loc.GetString("supermatter-examined-delamination"));
            return;
        }

        if (ent.Comp.Active)
            args.PushMarkup(Loc.GetString("supermatter-examined-unstable"));
        else
            args.PushMarkup(Loc.GetString("supermatter-examined-stable"));

        if (ent.Comp.Integrity < 85f)
            args.PushMarkup(Loc.GetString("supermatter-examined-low-integrity"));

        Log.Info(ent.Comp.Active.ToString());
        Log.Info(ent.Comp.Integrity.ToString());
    }
}
