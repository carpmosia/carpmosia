using System.Linq;
using System.Text.RegularExpressions;
using Content.Shared._FarHorizons.Research;
using Robust.Shared.Prototypes;

namespace Content.Client._FarHorizons.Research.UI.Helpers.Search;

public sealed partial class SearchDatabase
{
    private static readonly Regex Pattern = new("[^a-z]");

    public Dictionary<ProtoId<ResearchTreeNodePrototype>, List<string>> Data = [];

    public string SearchTerm = "";

    public int BatchSize = 20;
    private int _step = 0;

    public Dictionary<ProtoId<ResearchTreeNodePrototype>, float> _movingIndex = [];

    public int NumSearchResults = 5;
    public List<ProtoId<ResearchTreeNodePrototype>> SearchResult = [];

    private bool _finishedIndexing => _movingIndex.Count == Data.Count;
    private bool _unread = false;
    public bool Unread
    {
        get {
            if (_unread)
            {
                _unread = false;
                return true;
            }
            return false;
        }
    }
    public bool HasNewResult = false;

    public void Build(IPrototypeManager protoMan, HashSet<ProtoId<ResearchTreeNodePrototype>> nodes)
    {
        Data = [];
        foreach (var nodeId in nodes)
        {
            var node = protoMan.Index(nodeId);

            List<string> searchTerms = [];

            searchTerms.AddRange(
                node
                .Unlocks
                .Select(p => protoMan.Index(p).Result)
                .Where(p => p != null)
                .Select(p => Pattern.Replace(protoMan.Index<EntityPrototype>(p!.Value).Name.ToLower(), ""))
            );
            searchTerms.AddRange(
                node
                .UnlockFlags
                .Select(p => Pattern.Replace(Loc.GetString(protoMan.Index(p).Text).ToLower(), ""))
            );

            Data[nodeId] = searchTerms;
        }
    }

    public void Update()
    {
        if (_finishedIndexing || SearchTerm == "" || Data.Count == 0)
            return;
        
        for (var i = 0; i < BatchSize; i++)
        {
            var (id, tags) = Data.ElementAt(_step);
            _movingIndex[id] = 0;
            foreach (var tag in tags)
            {
                var score = SearchScore(SearchTerm, tag);
                if (score > _movingIndex[id])
                    _movingIndex[id] = score;
                
                if (score > 0.9)
                    break;
            }

            if (_finishedIndexing)
            {
                SearchResult = [.. _movingIndex.Where(p => p.Value > 0).OrderByDescending(p => p.Value).Select(p => p.Key).Take(NumSearchResults)];
                _unread = true;
                break;
            } else
                _step++;
        }
    }

    public void Search(string search)
    {
        var normSearch = Pattern.Replace(search.ToLower(), "");
        
        SearchTerm = normSearch;
        _step = 0;
        _unread = false;
        SearchResult = [];
        _movingIndex = [];

    }

    private static float SearchScore(string search, string target)
    {
        if (search == target)
            return 1;
        else if (target.StartsWith(search))
            return 0.9f;
        else if (target.Contains(search))
            return 0.7f;
        return 0;
    }
}