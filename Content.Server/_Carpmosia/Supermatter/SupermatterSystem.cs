using System.Linq;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.CCVar;
using Content.Server.Lightning;
using Content.Server._Carpmosia.Supermatter.Delamination;
using Content.Server.Singularity.Events;
using Content.Shared._Carpmosia.Supermatter;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.Atmos.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Radiation.Components;
using Content.Shared.Radiation.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.GameObjects;

namespace Content.Server._Carpmosia.Supermatter;

// cheese
public sealed partial class SupermatterSystem : EntitySystem
{
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private LightningSystem _lightningSystem = default!;
    [Dependency] private IPrototypeManager _protoMan = default!;
    [Dependency] private SharedRadiationSystem _radSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        InitializeUpdates();

        SubscribeLocalEvent<SupermatterComponent, DamageDealtEvent>(OnDamage);
        SubscribeLocalEvent<SupermatterComponent, EntityConsumedByEventHorizonEvent>(OnEventHorizonEntity);
    }

    public void OnDamage(EntityUid uid, SupermatterComponent comp, DamageDealtEvent args)
    {
        FixedPoint2 total = args.Damage.GetTotal();

        if (total <= 0)
            return;

        comp.Active = true;
        comp.StoredPower += (float)(total * 2.5f);
    }

    public void OnEventHorizonEntity(EntityUid uid, SupermatterComponent comp, EntityConsumedByEventHorizonEvent args)
    {
        comp.Active = true;
    }

    public void Delaminate(EntityUid uid, SupermatterComponent comp)
    {
        // TODO: add delam logic
        // No idea how to do the whole interface thing, since there's no automatic
        // dependency injection in non-RT classes

        QueueDel(uid);
    }
}
