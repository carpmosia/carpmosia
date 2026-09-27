using Content.Shared._FarHorizons.Research;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Prototypes;

namespace Content.Client._FarHorizons.Research.UI.Helpers;

public sealed class IconCache
{
    private readonly Dictionary<ProtoId<ResearchTreeNodePrototype>, ((string, string) texture, Color? iconColor)> _iconCache = [];
    private readonly Dictionary<string, RSI> _rsiCache = [];
    private readonly Dictionary<(string, string), Texture> _textureCache = [];

    public IconCache(IPrototypeManager protoMan, IResourceCache resourceCache)
    {
        foreach (var node in protoMan.EnumeratePrototypes<ResearchTreeNodePrototype>())
        {
            var icon = node.Icon;
            var texture = GetTexture(resourceCache, icon.Path, icon.State);
            if (texture == null)
                continue;
            
            _iconCache[node.ID] = ((icon.Path, icon.State), Color.TryFromHex(icon.Color, out var color) ? color : null);
        }
    }

    public (Texture?, Color?) GetCachedIcon(ProtoId<ResearchTreeNodePrototype> node)
    {
        if (!_iconCache.TryGetValue(node, out var cache) ||
            !_textureCache.TryGetValue(cache.texture, out var texture))
            return (null, null);
        
        return (texture, cache.iconColor);
    }

    private RSI GetRSI(IResourceCache resourceCache, string path)
    {
        if (!_rsiCache.ContainsKey(path))
            _rsiCache[path] = resourceCache.GetResource<RSIResource>(path).RSI;
        
        return _rsiCache[path];
    }

    private Texture? GetTexture(IResourceCache resourceCache, string path, string state)
    {
        var cacheKey = (path, state);
        if (!_textureCache.ContainsKey(cacheKey))
        {
            var rsi = GetRSI(resourceCache, path);
            if (rsi.TryGetState(state, out var rsiState))
                _textureCache[cacheKey] = rsiState.Frame0;
        }
        return _textureCache.TryGetValue(cacheKey, out var cache) ? cache : null;
    }
}