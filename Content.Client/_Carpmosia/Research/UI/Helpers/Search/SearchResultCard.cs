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

    private Color _fGColor;

    private Color _bGColor;

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
        _protoMan = protoMan;
        Node = node;
        _fGColor = fgColor;
        _bGColor = bgColor;
        _size = size;

        _name = Loc.GetString(_protoMan.Index(Node).Name);
    }

    public void WrapName(DrawingHandleScreen handle)
    {
        if (_name == "")
            return;

        List<string> text = [];
        var dimensions = handle.GetDimensions(Font, _name, FontScale);
        if (dimensions.X < Size.X - (BorderMargin * 2))
            text.Add(_name);
        else
        {
            var line = "";
            foreach (var word in _name.Split(' '))
            {
                var newLine = line == "" ? word : line + " " + word;
                var newDimensions = handle.GetDimensions(Font, newLine, FontScale);
                if (newDimensions.X >= Size.X - (BorderMargin * 2))
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
        handle.DrawRect(Box, _fGColor);

        var marginVector = Vector2.One * BorderMargin;

        if (!MouseOver)
            handle.DrawRect(new(Box.TopLeft + marginVector, Box.BottomRight - marginVector), _bGColor);

        for (var i = 0; i < _text.Count; i++)
        {
            var dimensions = handle.GetDimensions(Font, _text[i], FontScale);
            Vector2 pos = new(Box.Left + BorderMargin, Box.Top + BorderMargin + (dimensions.Y * i));
            handle.DrawString(Font, pos, _text[i], FontScale, MouseOver ? _bGColor : _fGColor);
        }
    }
}
