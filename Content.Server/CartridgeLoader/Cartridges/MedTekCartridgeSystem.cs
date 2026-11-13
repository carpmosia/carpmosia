using Content.Server.Medical.Components;
using Content.Shared.CartridgeLoader;
using Content.Shared._Offbrand.Analyzers; // Carpmosia-edit - Offmed Port

namespace Content.Server.CartridgeLoader.Cartridges;

public sealed partial class MedTekCartridgeSystem : EntitySystem
{
    [Dependency] private CartridgeLoaderSystem _cartridgeLoaderSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MedTekCartridgeComponent, CartridgeAddedEvent>(OnCartridgeAdded);
        SubscribeLocalEvent<MedTekCartridgeComponent, CartridgeRemovedEvent>(OnCartridgeRemoved);
    }

    private void OnCartridgeAdded(Entity<MedTekCartridgeComponent> ent, ref CartridgeAddedEvent args)
    {
        // Carpmosia-start - Offmed Port
        EnsureComp<AnalyzerComponent>(args.Loader);
        EnsureComp<VitalsAnalyzerComponent>(args.Loader);
        EnsureComp<HandheldAnalyzerComponent>(args.Loader);
        // Carpmosia-end - Offmed Port
    }

    private void OnCartridgeRemoved(Entity<MedTekCartridgeComponent> ent, ref CartridgeRemovedEvent args)
    {
        // only remove when the program itself is removed
        if (!_cartridgeLoaderSystem.HasProgram<MedTekCartridgeComponent>(args.Loader.AsNullable()))
        {
            // Carpmosia-start - Offmed Port
            RemComp<AnalyzerComponent>(args.Loader);
            RemComp<VitalsAnalyzerComponent>(args.Loader);
            RemComp<HandheldAnalyzerComponent>(args.Loader);
            // Carpmosia-end - Offmed Port
        }
    }
}
