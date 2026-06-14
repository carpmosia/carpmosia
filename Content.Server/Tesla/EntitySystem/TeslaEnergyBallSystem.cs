using Content.Server.Administration.Logs;
using Content.Server.Singularity.Components;
using Content.Server.Tesla.Components;
using Content.Shared.Emp; // Carpmosia-edit - Engine Loose Rework
using Content.Shared.Database;
using Content.Shared.Singularity.Components;
using Content.Shared.Mind.Components;
using Content.Shared.Tag;
using Robust.Shared.GameObjects; // Carpmosia-edit - Engine Loose Rework
using Robust.Shared.Physics.Events;
using Content.Server.Lightning.Components;
using Robust.Server.Audio;
using Content.Server.Singularity.Events;

namespace Content.Server.Tesla.EntitySystems;

/// <summary>
/// A component that tracks an entity's saturation level from absorbing other creatures by touch, and spawns new entities when the saturation limit is reached.
/// </summary>
public sealed partial class TeslaEnergyBallSystem : EntitySystem
{
    [Dependency] private AudioSystem _audio = default!;
    [Dependency] private SharedEmpSystem _emp = default!; // Carpmosia-edit - Engine Loose Rework
    [Dependency] private SharedTransformSystem _transform = default!; // Carpmosia-edit - Engine Loose Rework

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
            component.SpawnedCount++; // Carpmosia-edit - Engine Loose Rework
        }
        if (component.Energy < component.EnergyToDespawn)
        {
            _audio.PlayPvs(component.SoundCollapse, uid);
            QueueDel(uid);
        }
        // Carpmosia-start - Engine Loose Rework
        // When enough miniballs have spawned, trigger a rupture (explode into miniballs)
        if (component.SpawnedCount == component.RuptureAmount)
        {
            for (var i = 0; i < component.SpawnAmount; i++)
                Spawn(component.EmpSpawnProto, Transform(uid).Coordinates);

            _emp.EmpPulse(_transform.GetMapCoordinates(uid), component.EmpRange, component.EmpConsumption, component.EmpDuration);
            QueueDel(uid);
        }
        // Carpmosia-end - Engine Loose Rework
    }
}
