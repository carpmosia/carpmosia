using System.Numerics;
using Content.Shared._FarHorizons.Research;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;

namespace Content.Client._FarHorizons.Research.UI.Helpers.Search;

public sealed class SearchResultCard
{
    const int borderMargin = 2;
    const float fontScale = 0.8f;

    public Font _font;

    private IPrototypeManager _protoMan;
    public ProtoId<ResearchTreeNodePrototype> Node;

    private string _name = "";

    private List<string> _text = [];

    public float TextSize = 0;

    private Vector2 _mousePos = Vector2.Zero;

    private Color FGColor;
    
    private Color BGColor;

    public Vector2 Pos;
    private Vector2 _size;
    public Vector2 Size => _size + new Vector2(0, TextSize);

    private UIBox2 _box => 
        new(Pos, Pos + Size);
    
    public bool MouseOver =>
        _mousePos.X >= _box.Left && _mousePos.X <= _box.Right && _mousePos.Y >= _box.Top && _mousePos.Y <= _box.Bottom;

    public SearchResultCard(IPrototypeManager protoMan, ProtoId<ResearchTreeNodePrototype> node, Font font, Vector2 size, Color fgColor, Color bgColor)
    {
        _font = font;
        _protoMan = protoMan;
        Node = node;
        FGColor = fgColor;
        BGColor = bgColor;
        _size = size;

        _name = Loc.GetString(_protoMan.Index(Node).Name);
    }

    public void WrapName(DrawingHandleScreen handle)
    {
        if (_name == "")
            return;
        
        List<string> text = [];
        var dimensions = handle.GetDimensions(_font, _name, fontScale);
        if (dimensions.X < Size.X - (borderMargin * 2))
            text.Add(_name);
        else {
            var line = "";
            foreach (var word in _name.Split(' '))
            {
                var newLine = line == "" ? word : line + " " + word;
                var newDimensions = handle.GetDimensions(_font, newLine, fontScale);
                if (newDimensions.X >= Size.X - (borderMargin * 2))
                {
                    text.Add(line);
                    line = word;
                } else {
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
        handle.DrawRect(_box, FGColor, true);

        if (!MouseOver)
            handle.DrawRect(new(_box.TopLeft + (Vector2.One * borderMargin), _box.BottomRight - (Vector2.One * borderMargin)), BGColor, true);
        
        for (var i = 0; i < _text.Count; i++)
        {
            var dimensions = handle.GetDimensions(_font, _text[i], fontScale);
            Vector2 pos = new(_box.Left + borderMargin, _box.Top + borderMargin + (dimensions.Y * i));
            handle.DrawString(_font, pos, _text[i], fontScale, MouseOver ? BGColor : FGColor);
        }
    }
}