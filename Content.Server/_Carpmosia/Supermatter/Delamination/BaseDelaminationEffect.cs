using Content.Shared._Carpmosia.Supermatter;

namespace Content.Server._Carpmosia.Supermatter.Delamination;

/// <summary>
/// Approved by Dr. Isaac Kleiner
/// </summary>
public abstract class BaseDelaminationEffect : EntitySystem
{
    public virtual bool Requirements(SupermatterComponent comp)
    {
        return false;
    }

    public virtual void Delamination(EntityUid uid, SupermatterComponent comp)
    {

    }
}
