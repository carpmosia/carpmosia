using Content.Server.NodeContainer.Nodes;
using Content.Shared.NodeContainer;
using Robust.Shared.Map.Components;
using Robust.Shared.Utility; // Carpmosia-edit -  Starlight cable/pipe docking

namespace Content.Server.Power.Nodes
{
    [DataDefinition]
    public sealed partial class CableNode : Node
    {
        // Carpmosia-start -  Starlight cable/pipe docking
        private HashSet<CableNode>? _alwaysReachable;
        public HashSet<CableNode>? GetAlwaysReachable() => _alwaysReachable;

        public void AddAlwaysReachable(CableNode node)
        {
            if (node == this) return;
            _alwaysReachable ??= new();
            _alwaysReachable.Add(node);
        }

        public void RemoveAlwaysReachable(CableNode node)
        {
            if (_alwaysReachable == null) return;
            _alwaysReachable.Remove(node);
        }
        // Carpmosia-end - Starlight cable/pipe docking

        public override IEnumerable<Node> GetReachableNodes(
            Entity<TransformComponent> xform,
            EntityQuery<NodeContainerComponent> nodeQuery,
            EntityQuery<TransformComponent> xformQuery,
            Entity<MapGridComponent>? grid,
            IEntityManager entMan)
        {
            // Carpmosia-start - Starlight cable/pipe docking
            if (_alwaysReachable != null)
            {
                var remQ = new RemQueue<CableNode>();
                foreach (var node in _alwaysReachable)
                {
                    if (node.Deleting)
                        remQ.Add(node);
                    else
                        yield return node;
                }
                foreach (var node in remQ)
                {
                    _alwaysReachable.Remove(node);
                }
            }
            // Carpmosia-end - Starlight cable/pipe docking

            if (!xform.Comp.Anchored || grid is not { } gridEnt)
                yield break;

            var mapSystem = entMan.System<SharedMapSystem>();
            var gridIndex = mapSystem.TileIndicesFor(gridEnt, xform.Comp.Coordinates);

            // While we go over adjacent nodes, we build a list of blocked directions due to
            // incoming or outgoing wire terminals.
            var terminalDirs = 0;
            List<(Direction, Node)> nodeDirs = new();

            foreach (var (dir, node) in NodeHelpers.GetCardinalNeighborNodes(nodeQuery, gridEnt, gridIndex, mapSystem))
            {
                if (node is CableNode && node != this)
                {
                    nodeDirs.Add((dir, node));
                }

                if (node is CableDeviceNode && dir == Direction.Invalid)
                {
                    // device on same tile
                    nodeDirs.Add((Direction.Invalid, node));
                }

                if (node is CableTerminalNode)
                {
                    if (dir == Direction.Invalid)
                    {
                        // On own tile, block direction it faces
                        terminalDirs |= 1 << (int) xformQuery.GetComponent(node.Owner).LocalRotation.GetCardinalDir();
                    }
                    else
                    {
                        var terminalDir = xformQuery.GetComponent(node.Owner).LocalRotation.GetCardinalDir();
                        if (terminalDir.GetOpposite() == dir)
                        {
                            // Target tile has a terminal towards us, block the direction.
                            terminalDirs |= 1 << (int) dir;
                        }
                    }
                }
            }

            foreach (var (dir, node) in nodeDirs)
            {
                // If there is a wire terminal connecting across this direction, skip the node.
                if (dir != Direction.Invalid && (terminalDirs & (1 << (int) dir)) != 0)
                    continue;

                yield return node;
            }
        }

        // Carpmosia-start - Starlight cable/pipe docking
        public override void OnAnchorStateChanged(IEntityManager entityManager, bool anchored)
        {
            base.OnAnchorStateChanged(entityManager, anchored);

            var dockCableSystem = entityManager.System<Docking.CableDockingSystem>();
            if (anchored)
                dockCableSystem.TryConnectDockedCable(this);
            else
                dockCableSystem.RemoveDockConnections(this);
        }
        // Carpmosia-end - Starlight cable/pipe docking
    }
}
