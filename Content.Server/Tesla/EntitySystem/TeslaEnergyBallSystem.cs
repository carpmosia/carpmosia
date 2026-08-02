using Content.Server.Administration.Logs;
using Content.Server.Power.SMES; // Carpmosia-edit - Engine Loose Rework
using Content.Server.Singularity.Components;
using Content.Server.Tesla.Components;
using Content.Shared.Emp; // Carpmosia-edit - Engine Loose Rework
using Content.Shared.Database;
using Content.Shared.Singularity.Components;
using Content.Shared.Mind.Components;
using Content.Shared.Tag;
using Robust.Shared.Physics.Events;
using Content.Server.Lightning.Components;
using Robust.Server.Audio;
using Content.Server.Singularity.Events;
using Content.Server.Lightning; // Carpmosia-edit - Engine Loose Rework

namespace Content.Server.Tesla.EntitySystems;

/// <summary>
/// A component that tracks an entity's saturation level from absorbing other creatures by touch, and spawns new entities when the saturation limit is reached.
/// </summary>
public sealed partial class TeslaEnergyBallSystem : EntitySystem
{
    [Dependency] private AudioSystem _audio = default!;
    // Carpmosia-start - Engine Loose Rework
    [Dependency] private SharedEmpSystem _emp = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private LightningSystem _lightning = default!;
    // Carpmosia-end - Engine Loose Rework

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TeslaEnergyBallComponent, EntityConsumedByEventHorizonEvent>(OnConsumed);
    }

    private void OnConsumed(Entity<TeslaEnergyBallComponent> tesla, ref EntityConsumedByEventHorizonEvent args)
    {
        Spawn(tesla.Comp.ConsumeEffectProto, Transform(args.Entity).Coordinates);
        if (TryComp<SinguloFoodComponent>(args.Entity, out var singuloFood))
        {
            AdjustEnergy(tesla, tesla.Comp, singuloFood.Energy);
        // Carpmosia-start - Engine Loose Rework
        }
        else if (HasComp<SmesComponent>(args.Entity))
        {
            Rupture(tesla);
        // Carpmosia-end - Engine Loose Rework
        } else
        {
            AdjustEnergy(tesla, tesla.Comp, tesla.Comp.ConsumeStuffEnergy);
        }
    }

    public void AdjustEnergy(EntityUid uid, TeslaEnergyBallComponent component, float delta)
    {
        component.Energy += delta;

        if (component.Energy > component.NeedEnergyToSpawn)
        {
            component.Energy -= component.NeedEnergyToSpawn;
            Spawn(component.SpawnProto, Transform(uid).Coordinates);
        }
        if (component.Energy < component.EnergyToDespawn)
        {
            _audio.PlayPvs(component.SoundCollapse, uid);
            QueueDel(uid);
        }
    }

    // Carpmosia-start - Engine Loose Rework
    private void Rupture(Entity<TeslaEnergyBallComponent> ent)
    {
        var coords = Transform(ent).Coordinates;

        for (var i = 0; i < ent.Comp.SpawnAmount; i++)
        {
            SpawnAtPosition(ent.Comp.EmpSpawnProto, coords);
        }

        _audio.PlayPvs(ent.Comp.SoundExplosion, coords);
        _emp.EmpPulse(coords, ent.Comp.EmpRange, ent.Comp.EmpConsumption, ent.Comp.EmpDuration);
        _lightning.ShootRandomLightnings(ent, ent.Comp.EmpRange, ent.Comp.SpawnAmount * 4, arcDepth: 3);

        QueueDel(ent);
    }
    // Carpmosia-end - Engine Loose Rework
}
