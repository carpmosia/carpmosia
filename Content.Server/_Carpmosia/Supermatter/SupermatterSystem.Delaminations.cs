namespace Content.Server._Carpmosia.Supermatter;

// did you know that cheese can explode in the microwave?
// i know
// and now you know, too
public sealed partial class SupermatterSystem
{
    // Triggers when there's no other delamination that fits the case
    public void CriticalDelamination(EntityUid uid)
    {
        _explosion.QueueExplosion(uid, "Default", 15000, 5, 100);
    }

    public void SingularityDelamination(EntityUid uid)
    {
        var coords = Transform(uid).Coordinates;
        SpawnAtPosition("Singularity", coords);
    }

    public void TeslaDelamination(EntityUid uid)
    {
        var coords = Transform(uid).Coordinates;
        SpawnAtPosition("TeslaEnergyBall", coords);
    }
}
