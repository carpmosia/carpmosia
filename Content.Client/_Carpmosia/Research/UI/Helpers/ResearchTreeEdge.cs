using System.Linq;
using System.Numerics;
using Content.Shared._FarHorizons.Research;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;

namespace Content.Client._FarHorizons.Research.UI.Helpers;

public struct DrawResearchEdge((int, int) indA, (int, int) indB, Vector2 spacing, Vector2 margin, Vector2 size, bool highlight = false, List<ProtoId<ResearchTreeNodePrototype>>? linked = null, bool researched = false, Vector2? offset = null)
{
    public readonly Vector2 A => Offset + new Vector2(Margin.X + Size.X + Size.Y + (IndexA.x * (Size.X + Size.Y + Spacing.X)), Margin.Y + (Size.Y / 2) + (IndexA.y * (Size.Y + Spacing.Y)));
    public readonly Vector2 B => Offset + new Vector2(Margin.X + (IndexB.x * (Size.X + Size.Y + Spacing.X)), Margin.Y + (Size.Y / 2) + (IndexB.y * (Size.Y + Spacing.Y)));

    public (int x, int y) IndexA = indA;
    public (int x, int y) IndexB = indB;

    public Vector2 Spacing = spacing;
    public Vector2 Margin = margin;
    public Vector2 Size = size;

    private Vector2? _offset = offset;
    public Vector2 Offset {
        readonly get => _offset ?? Vector2.Zero;
        set => _offset = value;
    }

    public bool Highlight = highlight;
    public bool Researched = researched;

    public readonly Color LineColor =>
        Highlight ? HighlightColor :
        Researched ? ResearchedColor :
        NotReserachedColor;

    private readonly Color NotReserachedColor = Color.Gray;
    private readonly Color ResearchedColor = Color.DarkGreen;
    private readonly Color HighlightColor = Color.White;

    public List<ProtoId<ResearchTreeNodePrototype>> Linked = linked ?? [];

    public DrawResearchEdge(DrawResearchEdge other)
        : this(
            other.IndexA,
            other.IndexB,
            other.Spacing,
            other.Margin,
            other.Size,
            other.Highlight,
            other.Linked,
            other.Researched,
            other.Offset){}

     public DrawResearchEdge Zoom(float zoom) =>
        new(this)
        {
            Size = Size * zoom,
            Spacing = Spacing * zoom,
            Margin = Margin * zoom
        };

    public DrawResearchEdge Translate(Vector2 offset) =>
        new(this)
        {
            Offset = offset,
        };

    public DrawResearchEdge Hovered(ProtoId<ResearchTreeNodePrototype>? hovered) =>
        new(this)
        {
            Highlight = hovered != null && Linked.Contains(hovered.Value),
        };
    
    public DrawResearchEdge Research(HashSet<ProtoId<ResearchTreeNodePrototype>> allResearched) =>
        new(this)
        {
            Researched = Linked.Any(allResearched.Contains),
        };

    public readonly void Draw(DrawingHandleScreen handle) => 
        handle.DrawLine(A, B, LineColor);
}