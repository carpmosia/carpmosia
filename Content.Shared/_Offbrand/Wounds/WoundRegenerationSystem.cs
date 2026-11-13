using Content.Shared.Body.Events;
using Robust.Shared.Timing;

namespace Content.Shared._Offbrand.Wounds;

public sealed partial class WoundRegenerationSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private WoundableBodySystem _woundableBody = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var enumerator = EntityQueryEnumerator<WoundRegenerationComponent, WoundableBodyComponent>();
        while (enumerator.MoveNext(out var uid, out var regeneration, out var woundable))
        {
            if (regeneration.LastUpdate is not { } last || last + regeneration.AdjustedUpdateInterval >= _timing.CurTime)
                continue;

            regeneration.LastUpdate = _timing.CurTime;
            _woundableBody.HealWounds((uid, woundable), regeneration.Damage, true, true);
            Dirty(uid, regeneration);
        }
    }

    [SubscribeLocalEvent]
    private void OnMapInit(Entity<WoundRegenerationComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.LastUpdate ??= _timing.CurTime;
        Dirty(ent);
    }

    [SubscribeLocalEvent]
    private void OnApplyMetabolicMultiplier(Entity<WoundRegenerationComponent> ent, ref ApplyMetabolicMultiplierEvent args)
    {
        ent.Comp.UpdateIntervalMultiplier = args.Multiplier;
        Dirty(ent);
    }
}
