
using Content.Shared._Carpmosia.Supermatter;

/// <summary>
/// Approved by Dr. Isaac Kleiner
/// </summary>
public interface IDelaminationEffect
{
    public bool Requirements(SupermatterComponent comp)
    {
        return false;
    }

    public void Delamination(EntityUid uid, SupermatterComponent comp)
    {

    }
}
