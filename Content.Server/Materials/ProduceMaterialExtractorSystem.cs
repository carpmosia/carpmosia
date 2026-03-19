using System.Linq;
using Content.Server.Botany.Components;
using Content.Server.Materials.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.DoAfter; // Carpmosia-edit - Insert storage contents into biogenerator
using Content.Shared.Interaction;
using Content.Shared.Materials; // Carpmosia-edit - Insert storage contents into biogenerator
using Content.Shared.Popups;
using Content.Shared.Storage; // Carpmosia-edit - Insert storage contents into biogenerator
using Robust.Server.Audio;
using Robust.Shared.Containers; // Carpmosia-edit - Insert storage contents into biogenerator

namespace Content.Server.Materials;

public sealed class ProduceMaterialExtractorSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly MaterialStorageSystem _materialStorage = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!; // Carpmosia-edit - Insert storage contents into biogenerator

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ProduceMaterialExtractorComponent, AfterInteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<ProduceMaterialExtractorComponent, BiogenDoAfterEvent>(OnBiogenDoAfter); // Carpmosia-edit - Insert storage contents into biogenerator
    }

    private void OnInteractUsing(Entity<ProduceMaterialExtractorComponent> ent, ref AfterInteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!this.IsPowered(ent, EntityManager))
            return;

        // Carpmosia-start - Insert storage contents into biogenerator
        if (HasComp<StorageComponent>(args.Used))
        {
            var doAfter = new DoAfterArgs(EntityManager, args.User, 0.5f, new BiogenDoAfterEvent(), ent, ent, used: args.Used)
            {
                BreakOnDamage = true,
                NeedHand = true,
                BreakOnMove = true,
                BreakOnWeightlessMove = true,
            };
            _doAfterSystem.TryStartDoAfter(doAfter);
        }
        else
        {
            if (!TryComp<ProduceComponent>(args.Used, out var produce))
                return;

            if (!_solutionContainer.TryGetSolution(args.Used, produce.SolutionName, out var solution))
                return;

            // Can produce even have fractional amounts? Does it matter if they do?
            // Questions man was never meant to answer.
            var matAmount = solution.Value.Comp.Solution.Contents
                .Where(r => ent.Comp.ExtractionReagents.Contains(r.Reagent.Prototype))
                .Sum(r => r.Quantity.Float());

            var changed = (int)matAmount;

            if (changed == 0)
            {
                _popup.PopupEntity(Loc.GetString("material-extractor-comp-wrongreagent", ("used", args.Used)), args.User, args.User);
                return;
            }

            _materialStorage.TryChangeMaterialAmount(ent, ent.Comp.ExtractedMaterial, changed);

            _audio.PlayPvs(ent.Comp.ExtractSound, ent);
            QueueDel(args.Used);
            args.Handled = true;
        }
        // Carpmosia-end - Insert storage contents into biogenerator
    }

    // Carpmosia-start - Insert storage contents into biogenerator
    /// <summary>
    /// DoAfter function for interacting with the biogenerator with an item with a storage component.
    /// Converts any valid items in the storage into biomass for the biogenerator.
    /// </summary>
    /// <param name="uid">The biogen uid</param>
    /// <param name="comp">The material extractor component</param>
    /// <param name="args">DoAfter args</param>
    private void OnBiogenDoAfter(EntityUid uid, ProduceMaterialExtractorComponent comp, BiogenDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target == null)
            return;

        // If there's no storage component, we leave
        if (!TryComp<StorageComponent>(args.Used, out var storage))
            return;

        // If the storage is empty, we leave
        if (storage.StoredItems.Count == 0)
            return;

        // Find every valid item and convert it to biomass
        foreach (var (item, _location) in storage.StoredItems)
        {
            if (!TryComp<ProduceComponent>(item, out var produce))
                continue;

            if (!_solutionContainer.TryGetSolution(item, produce.SolutionName, out var solution))
                continue;

            var matAmount = solution.Value.Comp.Solution.Contents
                .Where(r => comp.ExtractionReagents.Contains(r.Reagent.Prototype))
                .Sum(r => r.Quantity.Float());

            var changed = (int)matAmount;

            _materialStorage.TryChangeMaterialAmount(comp.Owner, comp.ExtractedMaterial, changed);
            QueueDel(item);
        }

        _audio.PlayPvs(comp.ExtractSound, comp.Owner);
        args.Handled = true;
    }
    // Carpmosia-end - Insert storage contents into biogenerator
}
