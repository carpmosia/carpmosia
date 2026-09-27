using System.Numerics;
using Content.Shared._FarHorizons.Research;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;

namespace Content.Client._FarHorizons.Research.UI.Helpers;

public struct DrawResearchNode
    (
        ProtoId<ResearchTreeNodePrototype> proto,
        ProtoId<ResearchTreeTierPrototype> tier,
        string name,
        (int, int) index,
        Vector2 spacing,
        Vector2 margin,
        Vector2 size,
        Font? font = null,
        List<string>? text = null,
        int textSize = 0,
        Texture? iconTexture = null,
        Color? iconColor = null,
        float? progress = null,
        float fontScale = 0.5f,
        bool hovered = false,
        bool selected = false,
        bool unlocked = false,
        bool completed = false,
        bool queued = false,
        int queueOrder = -1,
        Vector2? offset = null
    )
{
    public ProtoId<ResearchTreeNodePrototype> Proto = proto;
    public ProtoId<ResearchTreeTierPrototype> Tier = tier;
    public string Name = name;
    public List<string> Text = text ?? [];
    public int TextSize = textSize;
    public float? Progress = progress;
    public bool Unlocked = unlocked;
    public bool Completed = completed;
    public bool Highlight = hovered;
    public bool Selected = selected;
    public bool Queued = queued;
    public int QueueOrder = queueOrder;
    public (int x, int y) Index = index;
    public Vector2 Spacing = spacing;
    public Vector2 Margin = margin;
    public readonly Vector2 Position => 
        Offset + new Vector2(Margin.X + (Index.x * (_size.X + _size.Y + Spacing.X)), Margin.Y + (Index.y * (_size.Y + Spacing.Y)));
    private Vector2? _offset = offset;
    public Vector2 Offset {
        readonly get => _offset ?? Vector2.Zero;
        set => _offset = value;
    }
    private Vector2 _size = size;
    public readonly Vector2 Size
    {
        get
        {
            var width = _size.X + _size.Y;
            var height = _size.Y + TextSize;

            if (Progress != null && !Completed)
                height += ProgressHeight / 2;

            return new(width, height);
        }
    }
    public UIBox2 Box => 
        new(Position, Position + Size);
    public Vector2 Center => 
        Position + new Vector2(_size.Y, 0) + (new Vector2(_size.X, Size.Y) / 2);
    public Font? Font = font;
    public float FontScale = fontScale;

    public readonly Color ForegroundColor
    {
        get
        {
            if (Completed)
                return ResearchedColor;
            if (Queued)
                return QueuedColor;
            if (!Unlocked)
                return LockedColor;
            return UnlockedColor;
        }
    }

    public readonly Color IconColor => 
        _iconColor == null || !Unlocked || Completed ? 
            ForegroundColor : 
            _iconColor.Value;

    private Texture? _iconTexture = iconTexture;
    private Color? _iconColor = iconColor;

    private const int NodeMargin = 2;
    private const int SelectedMargin = 2;
    private const int ProgressHeight = 8;

    private readonly Color BackgroundColor = Color.Black;
    private readonly Color LockedColor = Color.Gray;
    private readonly Color UnlockedColor = Color.White;
    private readonly Color ResearchedColor = Color.DarkGreen;
    private readonly Color QueuedColor = Color.Orange;
    private readonly Color SelectionColor = Color.Yellow;

    public DrawResearchNode(DrawResearchNode other) 
        : this(
            other.Proto, 
            other.Tier,
            other.Name,
            other.Index,
            other.Spacing,
            other.Margin,
            other._size,
            other.Font,
            other.Text,
            other.TextSize,
            other._iconTexture,
            other._iconColor,
            other.Progress,
            other.FontScale,
            other.Highlight,
            other.Selected,
            other.Unlocked,
            other.Completed,
            other.Queued,
            other.QueueOrder,
            other.Offset){}

    public DrawResearchNode WrapName(DrawingHandleScreen handle)
    {
        if (Text.Count != 0 || Font == null)
            return this;
        
        List<string> text = [];

        var dimensions = handle.GetDimensions(Font!, Name, FontScale);
        if (dimensions.X < _size.X - (NodeMargin * 2))
            text.Add(Name);
        else
        {
            var line = "";
            foreach (var word in Name.Split(' '))
            {
                var newLine = line == "" ? word : line + " " + word;
                var newDimensions = handle.GetDimensions(Font!, newLine, FontScale);
                if (newDimensions.X >= _size.X - (NodeMargin * 2))
                {
                    text.Add(line);
                    line = word;
                } else {
                    line = newLine;
                }
            }
            text.Add(line);
        }

        return new(this)
        {
            Text = text,
            TextSize = (int)dimensions.Y * (text.Count - 1),
        };
    }

    public DrawResearchNode Zoom(float zoom) =>
        new(this)
        {
            _size = _size * zoom,
            Spacing = Spacing * zoom,
            Margin = Margin * zoom,
            FontScale = FontScale * zoom,
            TextSize = (int)(TextSize * zoom),
        };

    public DrawResearchNode Icon((Texture? texture, Color? color) icon) => 
        icon.texture == null ? 
            this : 
            new(this)
            {
                _iconTexture = icon.texture,
                _iconColor = icon.color,
            };

    public DrawResearchNode Translate(Vector2 offset) =>
        new(this)
        {
            Offset = offset,
        };

    public DrawResearchNode Hovered(Vector2 mousePos) =>
        new(this)
        {
            Highlight = IsHovering(mousePos),
        };

    public DrawResearchNode Select(ProtoId<ResearchTreeNodePrototype>? target) =>
        new(this)
        {
            Selected = target == Proto,
        };

    public DrawResearchNode Researching(Dictionary<ProtoId<ResearchTreeNodePrototype>, float> researching) =>
        new(this)
        {
            Progress = researching.TryGetValue(Proto, out var value) ? value : null,
        };
    
    public DrawResearchNode Unlock(HashSet<ProtoId<ResearchTreeTierPrototype>> unlockedTiers, HashSet<ProtoId<ResearchTreeNodePrototype>> unlockedNodes) =>
        new(this)
        {
            Unlocked = unlockedTiers.Contains(Tier) && unlockedNodes.Contains(Proto),
        };
    
    public DrawResearchNode Queue(List<ProtoId<ResearchTreeNodePrototype>> queuedNodes) => 
        new(this)
        {
            Queued = queuedNodes.Contains(Proto),
            QueueOrder = !queuedNodes.Contains(Proto) ? -1 : queuedNodes.IndexOf(Proto) + 1,
        };
        

    public DrawResearchNode Research(HashSet<ProtoId<ResearchTreeNodePrototype>> allResearched) =>
        new(this)
        {
            Completed = allResearched.Contains(Proto),
        };

    public bool IsHovering(Vector2 mousePos) =>
        mousePos.X > Position.X && mousePos.Y > Position.Y && mousePos.X < Position.X + Size.X && mousePos.Y < Position.Y + Size.Y;

    public void Draw(DrawingHandleScreen handle)
    {
        if (Selected)
            handle.DrawRect(new(Box.TopLeft - (Vector2.One * SelectedMargin), Box.BottomRight + (Vector2.One * SelectedMargin)), SelectionColor, true);

        handle.DrawRect(Box, ForegroundColor, true);

        if (!Highlight)
            handle.DrawRect(new(Box.TopLeft + (Vector2.One * NodeMargin), Box.BottomRight - (Vector2.One * NodeMargin)), BackgroundColor, true);

        if (Progress != null && !Completed)
        {
            var progressBarWidth = (Size.X - (NodeMargin * 2)) * Progress!.Value;
            handle.DrawRect(new(new(Box.BottomLeft.X + NodeMargin, Box.BottomLeft.Y - ProgressHeight - NodeMargin), new(Box.BottomLeft.X + NodeMargin + progressBarWidth, Box.BottomRight.Y - NodeMargin)), Color.Orange);
        }

        if (Font != null && Text.Count > 0)
        {
            for (var i = 0; i < Text.Count; i++)
            {
                var text = Text[i];
                var dimensions = handle.GetDimensions(Font, text, FontScale);
                Vector2 pos = new(Center.X - (dimensions.X / 2), Position.Y + (dimensions.Y * i) + (dimensions.Y / 2));
                handle.DrawString(Font, pos, text, FontScale, Highlight ? BackgroundColor : ForegroundColor);
            }
        }

        if (_iconTexture != null)
        {
            var iconPos = new Vector2(Position.X + NodeMargin, Position.Y + (Size.Y / 2) - (_size.Y / 2));
            handle.DrawTextureRect(_iconTexture!, new(iconPos, iconPos + new Vector2(_size.Y, _size.Y)), Highlight ? BackgroundColor : IconColor);
        }
    }
}