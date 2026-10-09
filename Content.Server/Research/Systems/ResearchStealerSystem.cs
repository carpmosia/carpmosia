using Content.Server._Carpmosia.Research;
using Content.Shared._Carpmosia.Research.Components;
using Content.Shared.Research.Components;
using Content.Shared.Research.Systems;
using Robust.Shared.Random;

namespace Content.Server.Research.Systems;

public sealed partial class ResearchStealerSystem : SharedResearchStealerSystem
{
    [Dependency] private FHResearchSystem _fhResearch = default!; // Carpmosia-edit - FH research tree
    [Dependency] private IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ResearchStealerComponent, ResearchStealDoAfterEvent>(OnDoAfter);
    }

    private void OnDoAfter(EntityUid uid, ResearchStealerComponent comp, ResearchStealDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target == null)
            return;

        var target = args.Target.Value;

        if (!TryComp<FHResearchTreeComponent>(target, out var tree)) // Carpmosia-edit - FH research tree
            return;

        var ev = new ResearchStolenEvent(uid, target, new());
        var count = _random.Next(comp.MinToSteal, comp.MaxToSteal + 1);
        for (var i = 0; i < count; i++)
        {
            if (tree.Researched.Count == 0) // Carpmosia-edit - FH research tree
                break;

            var toRemove = _random.Pick(_fhResearch.GetRemovableReseach((target, tree))); // Carpmosia-edit - FH research tree
            if (_fhResearch.TryRemoveResearchedNode((target, tree), toRemove)) // Carpmosia-edit - FH research tree
                ev.Techs.Add(toRemove);
        }
        RaiseLocalEvent(uid, ref ev);

        args.Handled = true;
    }
}

/// <summary>
/// Event raised on the user when research is stolen from a RND server.
/// Techs contains every technology id researched.
/// </summary>
[ByRefEvent]
public record struct ResearchStolenEvent(EntityUid Used, EntityUid Target, List<string> Techs);
