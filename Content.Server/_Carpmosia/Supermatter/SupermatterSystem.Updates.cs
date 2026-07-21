using System.Linq;
using Content.Shared._Carpmosia.Supermatter;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.CCVar;
using Content.Shared.Radiation.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._Carpmosia.Supermatter;

public sealed partial class SupermatterSystem
{
    private float _accumulator = 0f; // all supermatters are synced now
    private float _radAccumulator = 0f;

    private string[] _gasToEffect =
    {
        "Oxygen", "Nitrogen", "CarbonDioxide",
        "Plasma", "Tritium", "WaterVapor",
        "Ammonia", "NitrousOxide", "Frezon",
        "Hydrogen", "Helium"
    };

    public void InitializeUpdates()
    {
        SubscribeLocalEvent<SupermatterComponent, AtmosDeviceUpdateEvent>(OnAtmosUpdate);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _accumulator += frameTime;
        _radAccumulator += frameTime;

        bool radUpdate = _radAccumulator > _cfg.GetCVar(CCVars.RadiationGridcastUpdateRate);
        bool smUpdate = _accumulator > _cfg.GetCVar(CCVars.SupermatterUpdateRate);

        if (!(radUpdate || smUpdate))
            return;

        var query = EntityQueryEnumerator<SupermatterComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            Entity<SupermatterComponent> ent = (uid, comp);

            if (!ent.Comp.Active)
                continue;

            if (radUpdate)
                OnRadiationUpdate(ent);

            if (smUpdate)
                OnUpdate(ent);
        }

        _accumulator = smUpdate ? 0f : _accumulator;
        _radAccumulator = radUpdate ? 0f : _radAccumulator;
    }

    public void OnRadiationUpdate(Entity<SupermatterComponent> ent)
    {
        if (TryComp<RadiationSourceComponent>(ent, out var radComp))
        {
            float energySpent = ent.Comp.StoredPower * 0.05f;
            ent.Comp.StoredPower *= 0.95f;

            float radPower = 10f * MathF.Log(energySpent + 1);

            _radSystem.SetIntensity(new Entity<RadiationSourceComponent?>(ent, radComp), radPower);
            Log.Info("Rad energy spent:\t" + energySpent);
        }
    }

    public void OnUpdate(Entity<SupermatterComponent> ent)
    {
        float spentPower = ent.Comp.StoredPower * 0.66f;
        int lightnings = (int)(spentPower / 5000);

        if (lightnings != 0)
        {
            ent.Comp.StoredPower -= 5000 * lightnings;
            _lightningSystem.ShootRandomLightnings(ent, 7, lightnings, arcDepth:3);
        }

        if (ent.Comp.StoredPower > 10000)
        {
            ent.Comp.Integrity -= (ent.Comp.StoredPower - 10000) / 1000; // 15000 units = -5 integrity
            ent.Comp.StoredPower = 8000;
        }

        if (ent.Comp.Integrity < 0)
            ent.Comp.DelaminationTime += _accumulator;
        else
            ent.Comp.DelaminationTime = 0f;

        if (ent.Comp.Integrity < -100 || ent.Comp.DelaminationTime > _cfg.GetCVar(CCVars.SupermatterDelaminationTimer))
            Delaminate(ent);

        Log.Info("Integrity:\t\t" + ent.Comp.Integrity.ToString());
        Log.Info("Stored Power:\t" + ent.Comp.StoredPower.ToString());
        Dirty(ent);
    }

    public void OnAtmosUpdate(Entity<SupermatterComponent> ent, ref AtmosDeviceUpdateEvent args)
    {
        if (!ent.Comp.Active)
            return;

        var environment = _atmosphereSystem.GetContainingMixture((EntityUid)ent, args.Grid, args.Map, true, true);

        if (environment == null)
            return;

        var envAir = environment.RemoveRatio(0.5f);

        #region Multipliers

        ent.Comp.WasteMultiplier = 1f;
        ent.Comp.HeatProductionMultiplier = 1f;
        ent.Comp.HeatPowerGainMultiplier = 1f;
        ent.Comp.HeatProtectionMultiplier = 1f;
        ent.Comp.IntegrityEffectMultiplier = 1f;
        ent.Comp.PowerTransmissionMultiplier = 1f;
        ent.Comp.PowerDecayMultiplier = 1f;

        for (int i = 0; i < Atmospherics.TotalNumberOfGases; i++)
        {
            float moles = envAir[i];

            if (moles == 0)
                continue;

            float proportion = moles / envAir.TotalMoles;

            string gas = _gasToEffect.ElementAtOrDefault(i) ?? "Default";
            var protoId = new ProtoId<SupermatterGasEffectPrototype>(gas);

            if (_protoMan.TryIndex(protoId, out var gasPrototype))
            {
                ent.Comp.WasteMultiplier += gasPrototype.WasteMultiplier * proportion;
                ent.Comp.HeatProductionMultiplier += gasPrototype.HeatProductionMultiplier * proportion;
                ent.Comp.HeatPowerGainMultiplier += gasPrototype.HeatPowerGainMultiplier * proportion;
                ent.Comp.HeatProtectionMultiplier += gasPrototype.HeatProtectionMultiplier * proportion;
                ent.Comp.IntegrityEffectMultiplier += gasPrototype.IntegrityEffectMultiplier * proportion;
                ent.Comp.PowerTransmissionMultiplier += gasPrototype.PowerTransmissionMultiplier * proportion;
                ent.Comp.PowerDecayMultiplier += gasPrototype.PowerDecayMultiplier * proportion;
            }
        }

        #endregion

        // IT DOESNT USE ANY MODIFIERS SINCE I NEED TO FINE-TUNE IT

        // Sum of all the "fuel" gases. Supermatter will absorb these
        var genMoles = envAir[0] + envAir[9];
        ent.Comp.StoredPower += genMoles * envAir.Temperature / 50;

        // This damage is calculated in Damage per AtmosUpdate
        // For example, 50 ticks in space will start the delamination
        // Or, one tick in 100.000 degrees
        // One tick in 200.000 degrees should blow it up instantly
        ent.Comp.Integrity -= Math.Max(0, envAir.Temperature - 1750) / 500; // 4000 degrees -> -4.5 integrity / AtmosUpdate
        ent.Comp.Integrity -= Math.Max(0, envAir.TotalMoles - 2000) / 1000; // 4000 moles -> -2 integrity / AtmosUpdate
        ent.Comp.Integrity -= (100 - Math.Min(100, envAir.Temperature)) / 50; // 0 kelvin (Space) -> -2 inegrity / AtmosUpdate

        environment.AdjustMoles(Gas.Plasma, envAir.TotalMoles * 0.8f);
        environment.AdjustMoles(Gas.Oxygen, envAir.TotalMoles * 0.2f);

        ent.Comp.MolesAbsorbed = envAir.TotalMoles;

        float wasteProportion = 1f;
        if (environment.TotalMoles != 0)
        {
            wasteProportion = genMoles * 0.5f / environment.TotalMoles;
            environment.Temperature = environment.Temperature * (1 - wasteProportion) + 1500f * wasteProportion;
        }

        Log.Info("Temp:\t\t" + environment.Temperature);
        Log.Info("Moles eaten:\t" + ent.Comp.MolesAbsorbed);
    }
}
