using System.Linq;
using System.Numerics;
using Content.Shared._FarHorizons.Research;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client._FarHorizons.Research.UI.Helpers.Search;

public sealed class ResearchSearch(IGameTiming timing, IPrototypeManager protoMan, Font font, Texture texture)
{
    const int borderMargin = 2;

    public bool Active = false;
    public bool MouseOver =>
        _mousePos.X >= _button.Left && _mousePos.X <= _button.Right && _mousePos.Y >= _button.Top && _mousePos.Y <= _button.Bottom;

    public bool AnyMouseOver =>
        MouseOver || _searchResults.Any(p => p.MouseOver);

    private Vector2 _mousePos = Vector2.Zero;
    private Vector2 _viewportSize = Vector2.Zero;

    private Vector2 _inactiveSize = new(30, 30);
    private Vector2 _activeSize = new(160, 30);
    private Vector2 _buttonMargin = new(10, 10);

    // Button animation
    private Vector2 _sizeAnimFrom = new(30, 30);
    private Vector2 _sizeAnimTo = new(30, 30);
    private TimeSpan _sizeAnimStart = TimeSpan.Zero;
    private TimeSpan _sizeAnimEnd = TimeSpan.Zero;
    private float _animProgress => Math.Clamp((float)(timing.CurTime - _sizeAnimStart).TotalSeconds / (float)(_sizeAnimEnd - _sizeAnimStart).TotalSeconds, 0f, 1f);
    private TimeSpan _animSpeed = TimeSpan.FromSeconds(0.1);
    public Vector2 ButtonSize => Vector2.Lerp(_sizeAnimFrom, _sizeAnimTo, _animProgress);

    private UIBox2 _button => 
        new(_viewportSize - _buttonMargin - ButtonSize, _viewportSize - _buttonMargin);

    private Texture _texture = texture;
    private Font _font = font;
    public string SearchText = "";
    private int _textMaxSize = 12;
    private string _placeholderText = "Search...";

    private Color FGColor = Color.White;
    
    private Color BGColor => Color.Black;

    private TimeSpan _backspaceFrequency = TimeSpan.FromSeconds(0.1);
    private TimeSpan _nextBackspace = TimeSpan.Zero;

    private SearchDatabase? _searchDb = null;
    private List<SearchResultCard> _searchResults = [];
    private float _searchResultMargin = 4;

    public Action<ProtoId<ResearchTreeNodePrototype>>? OnSearchSelected;

    public void OnClicked()
    {   
        if (!MouseOver)
            foreach (var result in _searchResults)
                if (result.MouseOver)
                {
                    OnSearchSelected?.Invoke(result.Node);
                    break;
                }
        
        Active = !Active;
        _sizeAnimFrom = ButtonSize;
        _sizeAnimTo = Active ? _activeSize : _inactiveSize;
        _sizeAnimStart = timing.CurTime;
        _sizeAnimEnd = _sizeAnimStart + _animSpeed;
        SearchText = _placeholderText;
        _searchResults = [];
    }

    public void UpdateText(string text)
    {
        if (SearchText == _placeholderText)
            SearchText = "";

        if (SearchText.Length < _textMaxSize)
            SearchText += text;
        
        _searchDb?.Search(SearchText);
        _searchResults = [];
    }

    public void Backspace()
    {
        if (timing.CurTime > _nextBackspace && SearchText != "" && SearchText != _placeholderText)
        {
            SearchText = SearchText[..^1];
            _searchDb?.Search(SearchText);
            _searchResults = [];
            if (SearchText == "")
                SearchText = _placeholderText;
            _nextBackspace = timing.CurTime + _backspaceFrequency;
        }
    }

    public void AddSearchResults(DrawingHandleScreen handle, List<ProtoId<ResearchTreeNodePrototype>> results)
    {
        List<SearchResultCard> cards = [];
        Vector2 size = _activeSize;
        var postion = _button.TopLeft;
        foreach (var result in results)
        {
            SearchResultCard card = new(protoMan, result, _font, size, FGColor, BGColor);
            card.WrapName(handle);
            var adjustedOffset = new Vector2(0, card.Size.Y + _searchResultMargin);
            postion -= adjustedOffset;
            card.Pos = postion;
            cards.Add(card);
        }
        cards.Reverse();

        _searchResults = cards;
    }

    public void SetDb(SearchDatabase db) =>
        _searchDb = db;

    public void Update(DrawingHandleScreen handle, Vector2 viewportSize, Vector2 mousePos)
    {
        _mousePos = mousePos;
        _viewportSize = viewportSize;
        _searchDb?.Update();
        if (_searchDb?.Unread ?? false)
            AddSearchResults(handle, _searchDb!.SearchResult);
        
        foreach(var card in _searchResults)
            card.Update(mousePos);
    }

    public void Draw(DrawingHandleScreen handle)
    {
        foreach(var card in _searchResults)
            card.Draw(handle);

        handle.DrawRect(_button, FGColor, true);

        if (!MouseOver)
            handle.DrawRect(new(_button.TopLeft + (Vector2.One * borderMargin), _button.BottomRight - (Vector2.One * borderMargin)), BGColor, true);
        
        handle.DrawTextureRect(_texture, new(_button.BottomRight - _inactiveSize, _button.BottomRight), MouseOver ? BGColor : FGColor);

        if (Active && _animProgress == 1)
        {
            handle.DrawString(_font, new(_button.Left + 3, _button.Top + 3), SearchText, 0.8f, MouseOver ? BGColor : FGColor);
        }
    }
}