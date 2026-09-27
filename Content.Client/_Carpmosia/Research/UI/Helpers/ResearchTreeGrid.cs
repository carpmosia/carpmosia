using System.Linq;
using System.Numerics;
using Content.Shared._FarHorizons.Research;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;

namespace Content.Client._FarHorizons.Research.UI.Helpers;

public sealed class ResearchTreeGrid
{
    private readonly IPrototypeManager _prototypeManager;
    private readonly HashSet<ResearchTreeNodePrototype> _allNodes;

    public List<List<NodeSpace?>> Grid;
    public Dictionary<ProtoId<ResearchTreeTierPrototype>, int> TierWidths;

    public ResearchTreeGrid(IPrototypeManager protoMan, HashSet<ResearchTreeNodePrototype> nodes)
    {
        _prototypeManager = protoMan;
        _allNodes = nodes;

        List<ProtoId<ResearchTreeTierPrototype>> tiers = [.. _allNodes.Select(p => p.Tier).Distinct().OrderBy(p => _prototypeManager.Index(p).Position)];
        TierWidths = tiers.ToDictionary(
                            p => p, 
                            p => _allNodes
                                    .Where(e => e.Tier == p)
                                    .Select(e => e.GetTieredDepth(_prototypeManager))
                                    .Max()
                        );

        Grid = GetGrid(tiers);
    }

    public (List<DrawResearchTier>, List<DrawResearchNode>, List<DrawResearchEdge>) GetDrawable((int w, int h) nodeSize, (int x, int y) spacing, (int x, int y) margin, Font font)
    {
        List<DrawResearchNode> nodes = [];
        List<DrawResearchEdge> edges = [];

        Vector2 vSpacing = new(spacing.x, spacing.y);
        Vector2 vMargin = new(margin.x, margin.y);
        Vector2 vSize = new(nodeSize.w, nodeSize.h);

        for (var i = 0; i < Grid.Count; i++)
        {
            for (var j = 0; j < Grid[i].Count; j++)
            {
                if (Grid[i][j] == null)
                    continue;
                
                var posX = (nodeSize.w + spacing.x) * i;
                var posY = (nodeSize.h + spacing.y) * j;

                var nodeSpace = Grid[i][j]!;

                if (nodeSpace.IsNode)
                {
                    nodes.Add(new(nodeSpace.Node!, nodeSpace.Node!.Tier, Loc.GetString(nodeSpace.Node!.Name), (i, j), vSpacing, vMargin, vSize, font));
                } else {
                    DrawResearchEdge edge = new((i, j), (i, j), vSpacing, vMargin, vSize);
                    edge.Linked.AddRange(nodeSpace.LineFor.Select(p => (ProtoId<ResearchTreeNodePrototype>)p.Node!.ID));
                    edges.Add(edge);
                }

                Dictionary<(int, int), DrawResearchEdge> placed = [];
                foreach (var (link, linkedSource, linkedDestination) in nodeSpace.LinksTo)
                {
                    if (!placed.TryGetValue(link, out var edge))
                    {
                        var linkedPosX = (nodeSize.w + spacing.x) * link.x;
                        var linkedPosY = (nodeSize.h + spacing.y) * link.y;
                        edge = new((i, j), link, vSpacing, vMargin, vSize);
                        placed[link] = edge;
                    }

                    edge.Linked.Add(linkedDestination.Node!.ID);
                    edge.Linked.Add(linkedSource.Node!.ID);

                    edges.Add(edge);
                }
            }
        }

        var tiers = new List<DrawResearchTier>();
        int? left = null;
        int? right = 0;

        foreach (var tier in TierWidths)
        {
            right += tier.Value + 1;
            var tierProto = _prototypeManager.Index(tier.Key);
            if (!Color.TryParse(tierProto.Color, out var bgColor))
                bgColor = Color.Black;
            DrawResearchTier drawTier = new(tierProto.Name, font, left, right, vSpacing, vMargin, vSize, bgColor);
            tiers.Add(drawTier);
            left = right;
        }
        if (tiers.Count != 0)
            tiers[^1] = tiers[^1].RemoveRight();

        return (tiers, nodes, edges);
    }

    private bool ParentsPresent(ResearchTreeNodePrototype node)
    {
        foreach (var req in node.Requires)
        {
            if (!_allNodes.Any(p => p.ID == req))
                return false;

            var reqProto = _prototypeManager.Index(req);

            if (!ParentsPresent(reqProto))
                return false;
        }
        
        return true;
    }

    private List<List<NodeSpace?>> GetGrid(List<ProtoId<ResearchTreeTierPrototype>> tiers)
    {
        var tierNodes = tiers.ToDictionary(p => p, p => _allNodes.Where(e => e.Tier == p).ToList());
        Dictionary<ProtoId<ResearchTreeTierPrototype>, int> tierStartingColumns = [];
        var prevTierWidth = 0;
        foreach (var tier in tiers)
        {
            tierStartingColumns[tier] = tiers.IndexOf(tier) + prevTierWidth;
            prevTierWidth += TierWidths[tier];
        }

        var result = MakeEmptyRectangle(TierWidths.Values.Sum() + TierWidths.Count, _allNodes.Count);
        List<ResearchTreeNodePrototype> handledNodes = [];

        foreach (var tier in tiers)
        {
            var startingNodes = tierNodes[tier].Where(p => p.Requires.Count == 0);
            
            foreach (var node in startingNodes)
            {
                if (handledNodes.Contains(node))
                    continue;

                var height = ApproxBranchHeight(node, tierNodes);
                var x = NodePosX(node, tierNodes, TierWidths, tierStartingColumns);
                var y = 0;
                while (!CheckFreeRow(result, (x, y), height, TierWidths[tier]))
                    y++;

                var partialTree = WalkForward(ref result, node, y, tierNodes, TierWidths, tierStartingColumns, ref handledNodes);
                var fullTree = Backfill(ref result, partialTree, tierNodes, TierWidths, tierStartingColumns, ref handledNodes);
            }
        }

        return result;
    }

    private int ApproxBranchHeight(ResearchTreeNodePrototype node, 
                         Dictionary<ProtoId<ResearchTreeTierPrototype>, List<ResearchTreeNodePrototype>> tierNodes)
    {
        var height = 1;
        var children = node.Children(_prototypeManager);
        if (children.Count == 0)
            return height;

        var childrenSum = 0;
        foreach (var child in children)
            if (tierNodes[node.Tier].Contains(child))
                childrenSum += ApproxBranchHeight(child, tierNodes);
        
        return Math.Max(children.Count, childrenSum);
    }

    private static bool CheckFreeRow(List<List<NodeSpace?>> grid, (int x, int y) pos, int searchRange = 0, int searchDepth = 0)
    {
        for (var i = 0; i < 1 + (searchRange * 2); i++)
        {
            var offset = (i + 1) / 2 * (i % 2 == 0 ? 1 : -1);
            var posY = Math.Clamp(pos.y + offset, 0, grid[0].Count - 1);
            if (grid[pos.x][posY] != null)
                return false;

            if (searchDepth > 0 && !CheckFreeRow(grid, (pos.x + 1, pos.y + offset), 0, searchDepth - 1))
                return false;
        }

        return true;
    }

    private int NodePosX(ResearchTreeNodePrototype node, 
                         Dictionary<ProtoId<ResearchTreeTierPrototype>, List<ResearchTreeNodePrototype>> tierNodes, 
                         Dictionary<ProtoId<ResearchTreeTierPrototype>, int> tierWidths, 
                         Dictionary<ProtoId<ResearchTreeTierPrototype>, int> tierStartingColumns)
    {
        var children = node.Children(_prototypeManager);
        var childrenInTier = children.Where(tierNodes[node.Tier].Contains).ToList();
        var childrenPos = childrenInTier.Select(p => NodePosX(p, tierNodes, tierWidths, tierStartingColumns)).ToList();

        var parents = node.Requires.Select(_prototypeManager.Index).ToList();
        var parentsInTier = parents.Where(tierNodes[node.Tier].Contains).ToList();

        var x = children.Count == 0 ? 
                    parentsInTier.Count == 0 ? 
                        tierStartingColumns[node.Tier] : 
                        tierStartingColumns[node.Tier] + tierWidths[node.Tier] : 
                    childrenInTier.Count == 0 ? 
                        parentsInTier.Count == 0 ? 
                            tierStartingColumns[node.Tier] :
                            tierStartingColumns[node.Tier] + tierWidths[node.Tier] : 
                        childrenPos.Order().First() - 1;
        
        return Math.Clamp(x, tierStartingColumns[node.Tier], tierStartingColumns[node.Tier] + tierWidths[node.Tier]);
    }

    private static List<List<NodeSpace?>> MakeEmptyRectangle(int columns, int rows)
    {
        List<List<NodeSpace?>> result = [];
        for (var i = 0; i <= columns; i++)
        {
            List<NodeSpace?> cells = [];
            for (var j = 0; j <= rows; j++)
            {
                cells.Add(null);
            }
            result.Add(cells);
        }
        return result;
    }

    private static ((int x, int y) position, NodeSpace nodeSpace, bool created) PlaceAtCoords(ref List<List<NodeSpace?>> grid, NodeSpace nodeSpace, (int x, int y) position, bool rev = false, int offset = 0)
    {
        if (FindInColumn(grid, nodeSpace, position, 1) is ((int, int), NodeSpace) existing)
            return (existing.position, existing.nodeSpace, false);
        
        if (offset > grid[0].Count - 1)
            throw new Exception("Error generating ResearchTreeGrid: Ran out of space");
        
        var order = rev ? -1 : 1;
        var verticalOffset = (offset + 1) / 2 * (offset % 2 == 0 ? order : -order);
        var posY = Math.Clamp(position.y + verticalOffset, 0, grid[0].Count - 1);
        if (grid[position.x][posY] == null)
        {
            grid[position.x][posY] = nodeSpace;
            return ((position.x, posY), nodeSpace, true);
        }
        else
            return PlaceAtCoords(ref grid, nodeSpace, position, rev, offset + 1);
    }

    private static ((int x, int y) position, NodeSpace nodeSpace)? FindInColumn(List<List<NodeSpace?>> grid, NodeSpace nodeSpace, (int x, int y) position, int lineMergeDistance = -1)
    {
        List<NodeSpace> matching = [];
        if (nodeSpace.IsNode) 
            matching = [.. grid[position.x]
                        .Where(p => p != null && p.IsNode && p.Node == nodeSpace.Node)
                        .Select(p => p!)];
        else if (lineMergeDistance >= 0)
            matching = [.. grid[position.x][Math.Max(position.y - lineMergeDistance, 0)..]
                        .Take((lineMergeDistance * 2) + 1)
                        .Where(p => p != null && !p.IsNode && p.LineFor.Intersect(nodeSpace.LineFor).Any())
                        .Select(p => p!)];
        
        return matching.Count == 0 ? null : ((position.x, grid[position.x].IndexOf(matching.First())), matching.First());
    }

    private static ((int x, int y) position, NodeSpace nodeSpace)? FindInGrid(List<List<NodeSpace?>> grid, ResearchTreeNodePrototype node)
    {
        for (var i = 0; i < grid.Count; i++)
            for (var j = 0; j < grid[i].Count; j++)
                if (grid[i][j] != null && grid[i][j]!.IsNode && grid[i][j]!.Node == node)
                    return ((i, j), grid[i][j]!);
        return null;
    }

    private List<((int x, int y) position, NodeSpace nodeSpace)> Backfill(ref List<List<NodeSpace?>> grid, 
                                                                          List<((int x, int y) position, NodeSpace nodeSpace)> tree,
                                                                          Dictionary<ProtoId<ResearchTreeTierPrototype>, List<ResearchTreeNodePrototype>> tierNodes,
                                                                          Dictionary<ProtoId<ResearchTreeTierPrototype>, int> tierWidths,
                                                                          Dictionary<ProtoId<ResearchTreeTierPrototype>, int> tierStartingColumns,
                                                                          ref List<ResearchTreeNodePrototype> excludedNodes)
    {
        List<((int x, int y) position, NodeSpace nodeSpace)> fullTree = [.. tree];

        foreach (var node in tree.Where(p => p.nodeSpace.IsNode))
        {
            foreach (var parentId in node.nodeSpace.Node!.Requires)
            {
                var parent = _prototypeManager.Index(parentId);
                if (excludedNodes.Contains(parent))
                {
                    var parentPos = FindInGrid(grid, parent);
                    if (parentPos != null)
                        ConnectNodes(ref grid, parentPos.Value, node, true);
                    continue;
                }
                
                var subTree = WalkBackward(ref grid, parent, node.position.y, tierNodes, tierWidths, tierStartingColumns, ref excludedNodes);
                ConnectNodes(ref grid, subTree[0], node, true);

                List<((int x, int y) position, NodeSpace nodeSpace)> extra = [];
                foreach (var (position, nodeSpace) in subTree)
                {
                    if (!nodeSpace.IsNode)
                        continue;
                    
                    List<ResearchTreeNodePrototype> remainingNodes = [.. nodeSpace.Node!
                                                                            .Children(_prototypeManager)
                                                                            .Except(excludedNodes)];
                    foreach (var extraNode in remainingNodes)
                    {
                        var extraTree = WalkForward(ref grid, extraNode, position.y, tierNodes, tierWidths, tierStartingColumns, ref excludedNodes);
                        extra.AddRange(extraTree);
                        ConnectNodes(ref grid, (position, nodeSpace), extraTree[0]);
                    }
                }

                fullTree.AddRange(subTree);
                fullTree.AddRange(extra);
            }
        }

        return fullTree;
    }

    private List<((int x, int y) position, NodeSpace nodeSpace)> WalkBackward(ref List<List<NodeSpace?>> grid, 
                                                                             ResearchTreeNodePrototype node,
                                                                             int startY, 
                                                                             Dictionary<ProtoId<ResearchTreeTierPrototype>, List<ResearchTreeNodePrototype>> tierNodes,
                                                                             Dictionary<ProtoId<ResearchTreeTierPrototype>, int> tierWidths,
                                                                             Dictionary<ProtoId<ResearchTreeTierPrototype>, int> tierStartingColumns,
                                                                             ref List<ResearchTreeNodePrototype> excludedNodes)
    {
        List<((int x, int y) position, NodeSpace nodeSpace)> subTree = [];

        var startPosition = (NodePosX(node, tierNodes, tierWidths, tierStartingColumns), startY);
        var newNodeSpace = new NodeSpace(node);
        (var newPos, newNodeSpace, _) = PlaceAtCoords(ref grid, newNodeSpace, startPosition, true);
        subTree.Add((newPos, newNodeSpace));
        excludedNodes.Add(node);
        
        foreach (var parentId in node.Requires)
        {
            var parent = _prototypeManager.Index(parentId);

            var parentNode = new NodeSpace(parent);
            var prev = WalkBackward(ref grid, parent, newPos.y, tierNodes, tierWidths, tierStartingColumns, ref excludedNodes);
            subTree.AddRange(prev);
            ConnectNodes(ref grid, prev[0], (newPos, newNodeSpace), true);
        }

        return subTree;
    }

    private List<((int x, int y) position, NodeSpace nodeSpace)> WalkForward(ref List<List<NodeSpace?>> grid, 
                                                                             ResearchTreeNodePrototype node, 
                                                                             int startY, 
                                                                             Dictionary<ProtoId<ResearchTreeTierPrototype>, List<ResearchTreeNodePrototype>> tierNodes,
                                                                             Dictionary<ProtoId<ResearchTreeTierPrototype>, int> tierWidths,
                                                                             Dictionary<ProtoId<ResearchTreeTierPrototype>, int> tierStartingColumns,
                                                                             ref List<ResearchTreeNodePrototype> excludedNodes)
    {
        List<((int x, int y) position, NodeSpace nodeSpace)> tree = [];
        var startPosition = (NodePosX(node, tierNodes, tierWidths, tierStartingColumns), startY);
        var newNodeSpace = new NodeSpace(node);
        (var newPos, newNodeSpace, _) = PlaceAtCoords(ref grid, newNodeSpace, startPosition);
        tree.Add((newPos, newNodeSpace));
        excludedNodes.Add(node);

        foreach (var child in node.Children(_prototypeManager))
        {
            if (excludedNodes.Contains(child))
            {
                var childNode = FindInGrid(grid, child);
                if (childNode != null)
                    ConnectNodes(ref grid, (newPos, newNodeSpace), childNode.Value);
                continue;
            }

            var next = WalkForward(ref grid, child, tree[0].position.y, tierNodes, tierWidths, tierStartingColumns, ref excludedNodes);
            tree.AddRange(next);
            ConnectNodes(ref grid, (newPos, newNodeSpace), next[0]);
        }
        return tree;
    }

    private static void ConnectNodes(ref List<List<NodeSpace?>> grid, ((int x, int y) position, NodeSpace nodeSpace) from, ((int x, int y) position, NodeSpace nodeSpace) to, bool rev = false)
    {
        if (from.nodeSpace.LinksTo.Any(p => p.to.Node == to.nodeSpace.Node))
            return;

        if (to.position.x <= from.position.x + 2 && (to.position.x != from.position.x + 2 || to.position.y != from.position.y))
            from.nodeSpace.LinksTo.Add((to.position, from.nodeSpace, to.nodeSpace));
        else {
            var previous = from.nodeSpace;
            var nextPos = (from.position.x + 1, to.position.y);
            for (var i = from.position.x + 1; i < to.position.x; i++)
            {
                var nextLine = new NodeSpace();
                nextLine.LineFor.AddRange([from.nodeSpace, to.nodeSpace]);

                (var pos, nextLine, var created) = PlaceAtCoords(ref grid, nextLine, nextPos, rev);
                nextPos = (i + 1, pos.y);

                previous.LinksTo.Add((pos, from.nodeSpace, to.nodeSpace));
                nextLine.LineFor.AddRange([from.nodeSpace, to.nodeSpace]);
                nextLine.LineFor = [.. nextLine.LineFor.Distinct()];
                previous = nextLine;
            }
            previous.LinksTo.Add((to.position, from.nodeSpace, to.nodeSpace));
        }
    }
}

public sealed class NodeSpace
{
    public ResearchTreeNodePrototype? Node;
    public List<((int x, int y) pos, NodeSpace from, NodeSpace to)> LinksTo = [];

    public List<NodeSpace> LineFor = [];

    public bool IsNode => 
        Node != null;

    public NodeSpace() => 
        Node = null;

    public NodeSpace(ResearchTreeNodePrototype node) => 
        Node = node;
}