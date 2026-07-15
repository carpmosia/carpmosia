using Content.Shared.Examine;

namespace Content.Shared._Carpmosia.Supermatter;

public sealed partial class SupermatterSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<SupermatterComponent, ExaminedEvent>(OnExamined);
    }

    public void OnExamined(EntityUid uid, SupermatterComponent comp, ExaminedEvent args)
    {
        if (comp.Delamitaion)
        {
            if (comp.Integrity < -50f)
            {
                args.PushMarkup(Loc.GetString("supermatter-examined-delamination-late"));
                return;
            }

            args.PushMarkup(Loc.GetString("supermatter-examined-delamination"));
            return;
        }

        if (comp.Stable)
            args.PushMarkup(Loc.GetString("supermatter-examined-stable"));
        else
            args.PushMarkup(Loc.GetString("supermatter-examined-unstable"));

        if (comp.Integrity < 85f)
            args.PushMarkup(Loc.GetString("supermatter-examined-low-integrity"));
    }
}
