using Content.Shared.ResearchTree;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client.ResearchTree.UI.Helpers;

public sealed class IconCache
{
    private readonly Dictionary<ProtoId<ResearchTreeNodePrototype>, ((ResPath, string) texture, Color? iconColor)> _iconCache = [];
    private readonly Dictionary<ResPath, RSI> _rsiCache = [];
    private readonly Dictionary<(ResPath, string), Texture> _textureCache = [];

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

    private RSI GetRSI(IResourceCache resourceCache, ResPath path)
    {
        if (!_rsiCache.ContainsKey(path))
            _rsiCache[path] = resourceCache.GetResource<RSIResource>(path).RSI;

        return _rsiCache[path];
    }

    private Texture? GetTexture(IResourceCache resourceCache, ResPath path, string state)
    {
        var cacheKey = (path, state);
        if (_textureCache.TryGetValue(cacheKey, out var cache))
            return cache;

        var rsi = GetRSI(resourceCache, path);
        if (!rsi.TryGetState(state, out var rsiState))
            return null;
        _textureCache[cacheKey] = rsiState.Frame0;
        return rsiState.Frame0;

    }
}
