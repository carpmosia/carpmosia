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
            if (!comp.Active)
                continue;

            if (radUpdate)
                OnRadiationUpdate(uid, comp);

            if (smUpdate)
                OnUpdate(uid, comp);
        }

        _accumulator = smUpdate ? 0f : _accumulator;
        _radAccumulator = radUpdate ? 0f : _radAccumulator;
    }

    public void OnRadiationUpdate(EntityUid uid, SupermatterComponent comp)
    {
        if (TryComp<RadiationSourceComponent>(uid, out var radComp))
        {
            float energySpent = comp.StoredPower * 0.05f;
            comp.StoredPower *= 0.95f;

            float radPower = 10f * MathF.Log(energySpent + 1);

            _radSystem.SetIntensity(new Entity<RadiationSourceComponent?>(uid, radComp), radPower);
            Log.Info("Rad energy spent:\t" + energySpent);
        }
    }

    public void OnUpdate(EntityUid uid, SupermatterComponent comp)
    {
        float spentPower = comp.StoredPower * 0.66f;
        int lightnings = (int)(spentPower / 2500);

        if (lightnings != 0)
        {
            comp.StoredPower -= spentPower;
            _lightningSystem.ShootRandomLightnings(uid, 7, lightnings);
        }

        if (comp.Integrity < 0)
            comp.DelaminationTime += _accumulator;
        else
            comp.DelaminationTime = 0f;

        if (comp.Integrity < -100 || comp.DelaminationTime > _cfg.GetCVar(CCVars.SupermatterDelaminationTimer))
            Delaminate(uid, comp);

        Log.Info("Integrity:\t\t" + comp.Integrity.ToString());
        Log.Info("Stored Power:\t" + comp.StoredPower.ToString());
    }

    public void OnAtmosUpdate(EntityUid uid, SupermatterComponent comp, AtmosDeviceUpdateEvent args)
    {
        if (!comp.Active)
            return;

        var environment = _atmosphereSystem.GetContainingMixture(uid, args.Grid, args.Map, true, true);

        if (environment == null)
            return;

        var envAir = environment.RemoveRatio(0.5f);

        #region Multipliers

        comp.WasteMultiplier = 1f;
        comp.HeatProductionMultiplier = 1f;
        comp.HeatPowerGainMultiplier = 1f;
        comp.HeatProtectionMultiplier = 1f;
        comp.IntegrityEffectMultiplier = 1f;
        comp.PowerTransmissionMultiplier = 1f;
        comp.PowerDecayMultiplier = 1f;

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
                comp.WasteMultiplier += gasPrototype.WasteMultiplier * proportion;
                comp.HeatProductionMultiplier += gasPrototype.HeatProductionMultiplier * proportion;
                comp.HeatPowerGainMultiplier += gasPrototype.HeatPowerGainMultiplier * proportion;
                comp.HeatProtectionMultiplier += gasPrototype.HeatProtectionMultiplier * proportion;
                comp.IntegrityEffectMultiplier += gasPrototype.IntegrityEffectMultiplier * proportion;
                comp.PowerTransmissionMultiplier += gasPrototype.PowerTransmissionMultiplier * proportion;
                comp.PowerDecayMultiplier += gasPrototype.PowerDecayMultiplier * proportion;
            }
        }

        #endregion

        // IT DOESNT USE ANY MODIFIERS SINCE I NEED TO FINE-TUNE IT

        // Sum of all the "fuel" gases. Supermatter will absorb these
        var genMoles = envAir[0] + envAir[9];
        comp.StoredPower += genMoles * envAir.Temperature / 50;

        // This damage is calculated in Damage per AtmosUpdate
        // For example, 50 ticks in space will start the delamination
        // Or, one tick in 100.000 degrees
        // One tick in 200.000 degrees should blow it up instantly
        comp.Integrity -= Math.Max(0, envAir.Temperature - 1750) / 500; // 4000 degrees -> -4.5 integrity / AtmosUpdate
        comp.Integrity -= Math.Max(0, envAir.TotalMoles - 2000) / 1000; // 4000 moles -> -2 integrity / AtmosUpdate
        comp.Integrity -=
            (100 - Math.Min(100, envAir.Temperature)) / 50; // 0 kelvin (Space) -> -2 inegrity / AtmosUpdate

        environment.AdjustMoles(Gas.Plasma, envAir.TotalMoles * 0.8f);
        environment.AdjustMoles(Gas.Oxygen, envAir.TotalMoles * 0.2f);

        float wasteProportion = 1f;
        if (environment.TotalMoles != 0)
        {
            wasteProportion = genMoles * 0.5f / environment.TotalMoles;
            environment.Temperature = environment.Temperature * (1 - wasteProportion) + 1500f * wasteProportion;
        }

        Log.Info("Temp:\t\t" + environment.Temperature);
    }
}
