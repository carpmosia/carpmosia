using System.Numerics;
using Robust.Client.Graphics;

namespace Content.Client._FarHorizons.Research.UI.Helpers;

public struct DrawResearchTier (string name, Font? font, int? left, int? right, Vector2 spacing, Vector2 margin, Vector2 size, Color bgColor, Vector2? offset = null)
{
    public string Name = name;
    public Font? Font = font;
    public int? Left = left;
    public int? Right = right;

    public Vector2 Spacing = spacing;
    public Vector2 Margin = margin;
    public Vector2 Size = size;

    public Color BgColor = bgColor;

    public readonly Vector2 LeftPos => 
        new(Left == null ? 0 : Offset.X + Margin.X - (Spacing.X / 2) + (Left!.Value * (Size.X + Size.Y + Spacing.X)), 0);
    public readonly Vector2 RightPos => 
        new(Right == null ? 3000 : Offset.X + Margin.X - (Spacing.X / 2) + (Right!.Value * (Size.X + Size.Y + Spacing.X)), 3000);
    public readonly UIBox2 Box =>
        new(
            new Vector2(MathF.Min(LeftPos.X, RightPos.X), LeftPos.Y),
            new Vector2(MathF.Max(LeftPos.X, RightPos.X), RightPos.Y)
        );

    private Vector2? _offset = offset;
    public Vector2 Offset {
        readonly get => _offset ?? Vector2.Zero;
        set => _offset = value;
    }

    public DrawResearchTier (DrawResearchTier other) 
        : this(
            other.Name, 
            other.Font, 
            other.Left, 
            other.Right, 
            other.Spacing,
            other.Margin,
            other.Size,
            other.BgColor,
            other.Offset
        ){}

    public DrawResearchTier Zoom(float zoom) =>
        new(this)
        {
            Size = Size * zoom,
            Spacing = Spacing * zoom,
            Margin = Margin * zoom
        };
    public DrawResearchTier Translate(Vector2 offset) =>
        new(this)
        {
            Offset = offset,
        };

    public DrawResearchTier RemoveRight() =>
        new(this)
        {
            Right = null,
        };

    public void DrawBg(DrawingHandleScreen handle) => 
        handle.DrawRect(Box, BgColor, true);

    public void DrawHeader(DrawingHandleScreen handle)
    {
        if (Font != null)
        {
            var dimensions = handle.GetDimensions(Font, Name, 0.8f);
            Vector2 pos = new(Math.Max(0, Box.Left), 0);

            handle.DrawRect(new(Box.TopLeft, new(RightPos.X, Box.Top + dimensions.Y)), Color.Black, true);

            var size = Box.Right - Math.Max(0, Box.Left);
            if (size > dimensions.X)
                handle.DrawString(Font, pos, Name, 0.8f, Color.White);
        }
    }
}