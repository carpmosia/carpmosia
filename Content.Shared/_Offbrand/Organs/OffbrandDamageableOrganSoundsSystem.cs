using Content.Shared.Random.Helpers;
using Content.Shared._Offbrand.Medical;
using Content.Shared._Offbrand.Wounds;
using Robust.Shared.Timing;

namespace Content.Shared._Offbrand.Organs;

public sealed partial class OffbrandDamageableOrganSoundsSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;

    [SubscribeLocalEvent]
    private void OnStethoscopeExamine(Entity<OffbrandDamageableOrganSoundsComponent> ent, ref StethoscopeExamineEvent args)
    {
        var damage = Comp<DamageableOrganComponent>(ent);
        if (ent.Comp.Descriptions.HighestMatch(damage.Damage) is not { } match)
            return;

        var rand = SharedRandomExtensions.PredictedRandom(_timing, GetNetEntity(ent));

        var line = rand.Pick(ProtoMan.Index(match));
        args.Messages.Add(line);
    }
}
