using Content.Server.Explosion.EntitySystems;
using Content.Shared._Carpmosia.Supermatter;

namespace Content.Server._Carpmosia.Supermatter.Delamination;

public sealed partial class SuperCriticalDelamination : BaseDelaminationEffect
{
    [Dependency] private ExplosionSystem _explosion = default!;

    public override bool Requirements(SupermatterComponent comp)
    {
        return true; // fallback option, always true
    }

    public override void Delamination(EntityUid uid, SupermatterComponent comp)
    {
        _explosion.QueueExplosion(uid, "Default", 25000, 100, 50);
    }
}
