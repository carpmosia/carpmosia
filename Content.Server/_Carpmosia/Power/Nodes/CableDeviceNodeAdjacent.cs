using Content.Server.NodeContainer.EntitySystems;
using Content.Server.NodeContainer.Nodes;
using Content.Shared.NodeContainer;
using Robust.Shared.Map.Components;

namespace Content.Server.Power.Nodes;

/// <summary>
///     Type of node that connects to a <see cref="CableNode"/> below it.
/// </summary>
[DataDefinition]
[Virtual]
public partial class CableDeviceNodeAdjacent : CableDeviceNode
{
    public override IEnumerable<Node> GetReachableNodes(
        Entity<TransformComponent> xform,
        EntityQuery<NodeContainerComponent> nodeQuery,
        EntityQuery<TransformComponent> xformQuery,
        Entity<MapGridComponent>? grid,
        IEntityManager entMan)
    {
        if (!xform.Comp.Anchored || grid is not { } gridEnt)
            yield break;

        var mapSystem = entMan.System<SharedMapSystem>();
        var gridIndex = mapSystem.TileIndicesFor(gridEnt, xform.Comp.Coordinates);

        var nodes = NodeHelpers.GetCardinalNeighborNodes(nodeQuery, gridEnt, gridIndex, mapSystem);
        var ownDir = xform.Comp.LocalRotation.GetCardinalDir();

        foreach (var (nodeDir, node) in nodes)
        {
            if (node is CableNode
                && (nodeDir == ownDir || nodeDir == Direction.Invalid))
                yield return node;
        }
    }
}
