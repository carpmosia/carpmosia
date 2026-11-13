using System.Numerics;
using Content.Shared.ResearchTree;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;

namespace Content.Client.ResearchTree.UI.Helpers.Search;

public sealed class SearchResultCard
{
    private const int BorderMargin = 2;
    private const float FontScale = 0.8f;

    public Font Font;

    private readonly IPrototypeManager _protoMan;
    public ProtoId<ResearchTreeNodePrototype> Node;

    private readonly string _name;

    private List<string> _text = [];

    public float TextSize;

    private Vector2 _mousePos = Vector2.Zero;

    private Color _foregroundColor;

    private Color _backgroundColor;

    public Vector2 Pos;
    private Vector2 _size;
    public Vector2 Size => _size + new Vector2(0, TextSize);

    private UIBox2 Box =>
        new(Pos, Pos + Size);

    public bool MouseOver =>
        _mousePos.X >= Box.Left && _mousePos.X <= Box.Right && _mousePos.Y >= Box.Top && _mousePos.Y <= Box.Bottom;

    public SearchResultCard(IPrototypeManager protoMan, ProtoId<ResearchTreeNodePrototype> node, Font font, Vector2 size, Color fgColor, Color bgColor)
    {
        Font = font;
        Node = node;
        _protoMan = protoMan;
        _foregroundColor = fgColor;
        _backgroundColor = bgColor;
        _size = size;

        _name = Loc.GetString(_protoMan.Index(Node).Name);
    }

    public void WrapName(DrawingHandleScreen handle)
    {
        if (_name == "")
            return;

        List<string> text = [];
        var dimensions = handle.GetDimensions(Font, _name, FontScale);
        if (dimensions.X < Size.X - BorderMargin * 2)
            text.Add(_name);
        else
        {
            var line = "";
            var doubleMargin = BorderMargin * 2;
            foreach (var word in _name.Split(' '))
            {
                var newLine = line == "" ? word : line + " " + word;
                var newDimensions = handle.GetDimensions(Font, newLine, FontScale);
                if (newDimensions.X >= Size.X - doubleMargin)
                {
                    text.Add(line);
                    line = word;
                }
                else
                {
                    line = newLine;
                }
            }
            text.Add(line);
        }

        _text = text;
        TextSize = dimensions.Y * (text.Count - 1);
    }

    public void Update(Vector2 mousePos)
    {
        _mousePos = mousePos;
    }

    public void Draw(DrawingHandleScreen handle)
    {
        handle.DrawRect(Box, _foregroundColor);

        if (!MouseOver)
        {
            var marginVector = Vector2.One * BorderMargin;
            handle.DrawRect(new(Box.TopLeft + marginVector, Box.BottomRight - marginVector), _backgroundColor);
        }

        for (var i = 0; i < _text.Count; i++)
        {
            var dimensions = handle.GetDimensions(Font, _text[i], FontScale);
            Vector2 pos = new(Box.Left + BorderMargin, Box.Top + BorderMargin + (dimensions.Y * i));
            handle.DrawString(Font, pos, _text[i], FontScale, MouseOver ? _backgroundColor : _foregroundColor);
        }
    }
}
