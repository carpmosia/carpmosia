using Content.Server.Medical.Components;
using Content.Shared.CartridgeLoader;

namespace Content.Server.CartridgeLoader.Cartridges;

public sealed partial class MedTekPlusCartridgeSystem : EntitySystem
{
    [Dependency] private CartridgeLoaderSystem _cartridgeLoaderSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MedTekPlusCartridgeComponent, CartridgeAddedEvent>(OnCartridgeAdded);
        SubscribeLocalEvent<MedTekPlusCartridgeComponent, CartridgeRemovedEvent>(OnCartridgeRemoved);
    }

    private void OnCartridgeAdded(Entity<MedTekPlusCartridgeComponent> ent, ref CartridgeAddedEvent args)
    {
        var healthAnalyzer = EnsureComp<HealthAnalyzerPlusComponent>(args.Loader);
    }

    private void OnCartridgeRemoved(Entity<MedTekPlusCartridgeComponent> ent, ref CartridgeRemovedEvent args)
    {
        // only remove when the program itself is removed
        if (!_cartridgeLoaderSystem.HasProgram<MedTekPlusCartridgeComponent>(args.Loader))
        {
            RemComp<HealthAnalyzerPlusComponent>(args.Loader);
        }
    }
}
