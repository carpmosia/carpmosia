using System.Linq;
using Content.Shared._FarHorizons.Research;
using Content.Shared._FarHorizons.Research.Components;
using Content.Shared.Radio;
using Content.Shared.Research.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._FarHorizons.Research;

public sealed partial class FHResearchSystem
{
    public int HandleResearch(Entity<FHResearchTreeComponent?> ent, int points)
    {
        if (!Resolve(ent, ref ent.Comp))
            return points;

        var pointsAfter = points;
        while (pointsAfter > 0)
            Research((ent, ent.Comp), ref pointsAfter);

        RefreshUIOnClients(ent);

        return pointsAfter;
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<FHResearchTreeComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if(_timing.CurTime < comp.NextUpdate)
                continue;
            
            var bankCap = GetCurrentBankCapacity((uid, comp));

            if (comp.BankedPoints <= bankCap)
                continue;

            comp.NextUpdate = _timing.CurTime + comp.RefreshRate;

            SendBankFullWarning((uid, comp));
            comp.BankedPoints -= Math.Min(comp.BankedPoints - bankCap, comp.PointBleed);
            RefreshUIOnClients((uid, comp));
        }
    }

    public void Research(Entity<FHResearchTreeComponent> ent, ref int points)
    {
        if (points <= 0)
            return;

        if (ent.Comp.Queue.Count == 0)
        {
            AddBankedPoints(ent, ref points);
            return;
        }

        var nextNode = ent.Comp.Queue.First();
        var nextNodeProto = _protoMan.Index(nextNode);

        if (!ent.Comp.Progress.ContainsKey(nextNode))
            ent.Comp.Progress[nextNode] = 0;
        
        var pointsRemaining = nextNodeProto.Cost - ent.Comp.Progress[nextNode];
        
        if (points >= pointsRemaining)
        {
            UnlockNode(ent, nextNode);
            points -= pointsRemaining;
        } else {
            ent.Comp.Progress[nextNode] += points;
            points = 0;
        }
    }

    public void AddBankedPoints(Entity<FHResearchTreeComponent> ent, int points) => 
        AddBankedPoints(ent, ref points);

    public void AddBankedPoints(Entity<FHResearchTreeComponent> ent, ref int points)
    {
        ent.Comp.BankedPoints += points;
        points = 0;

        if (ent.Comp.BankedPoints < 0)
            ent.Comp.BankedPoints = 0;
    }

    public void SendBankFullWarning(Entity<FHResearchTreeComponent> ent)
    {
        if (_timing.CurTime >= ent.Comp.NextWarning)
        {
            ent.Comp.NextWarning = _timing.CurTime + ent.Comp.WarningFrequency;
            SendAnnouncement(ent, Loc.GetString("research-tree-bank-full-warning", ("amount", GetCurrentBankCapacity(ent))));
        }
    }

    public void SendAnnouncement(Entity<FHResearchTreeComponent> ent, string message) => SendAnnouncement(ent, message, []);
    public void SendAnnouncement(Entity<FHResearchTreeComponent> ent, string message, List<ProtoId<RadioChannelPrototype>> channels)
    {
        foreach (var channel in ent.Comp.AnnounceTo.Union(channels))
            _radio.SendRadioMessage(ent, message, channel, ent, escapeMarkup: false);
    }

    public void UnlockNode(Entity<FHResearchTreeComponent> ent, ProtoId<ResearchTreeNodePrototype> node, bool sendAnnouncement = true)
    {
        ent.Comp.Queue.Remove(node);
        ent.Comp.Progress.Remove(node);
        ent.Comp.Researched.Add(node);

        if (!TryComp(ent, out TechnologyDatabaseComponent? techDb))
            return;
        
        var nodeProto = _protoMan.Index(node);

        foreach(var recipe in nodeProto.Unlocks)
            _research.AddLatheRecipe(ent, recipe, techDb);
        
        List<ProtoId<ResearchTreeUnlockFlagPrototype>> add = [];
        foreach (var unlockFlag in nodeProto.UnlockFlags)
        {
            if (!ent.Comp.UnlockFlags.Contains(unlockFlag))
            {
                add.Add(unlockFlag);
            }
        }
        ent.Comp.UnlockFlags.AddRange(add);
        AddResearchModifiers(ent, add);

        if (sendAnnouncement)
            SendAnnouncement(ent, Loc.GetString("research-tree-unlock-broadcast", ("technology", Loc.GetString(nodeProto.Name)), ("amount", nodeProto.Cost)), nodeProto.AnnounceTo);
    }

    public bool TryRemoveResearchedNode(Entity<FHResearchTreeComponent> ent, ProtoId<ResearchTreeNodePrototype> node)
    {
        if (!GetRemovableReseach(ent).Contains(node) || !TryComp(ent, out TechnologyDatabaseComponent? techDb))
            return false;
        
        var nodeProto = _protoMan.Index(node);

        ent.Comp.Researched.Remove(node);
        foreach (var unlockedRecipe in nodeProto.Unlocks)
            _research.RemoveLatheRecipe(ent, unlockedRecipe, techDb);

        List<ProtoId<ResearchTreeUnlockFlagPrototype>> remove = [];
        foreach (var unlockFlag in nodeProto.UnlockFlags)
        {
            if (ent.Comp.UnlockFlags.Contains(unlockFlag))
            {
                remove.Add(unlockFlag);
            }
        }
        ent.Comp.UnlockFlags.RemoveAll(remove.Contains);
        RemoveResearchModifiers(ent, remove);

        ent.Comp.Queue = [];

        RefreshUIOnClients((ent, ent.Comp));
        
        return true;
    }

    public bool AddResearchToQueue(Entity<FHResearchTreeComponent?> ent, ProtoId<ResearchTreeNodePrototype> node)
    {
        if (!Resolve(ent, ref ent.Comp) ||
            ent.Comp.Queue.Contains(node))
            return false;

        if (ent.Comp.Queue.Count < GetCurrentQueueSize((ent, ent.Comp)))
        {
            var nodeProto = _protoMan.Index(node);
            var nodes = GetTreeNodes((ent, ent.Comp));
            if (nodes.Contains(nodeProto) && IsNodeUnlocked((ent, ent.Comp), nodeProto))
                ent.Comp.Queue.Add(node);

            if (ent.Comp.BankedPoints > 0)
            {
                var points = ent.Comp.BankedPoints;
                ent.Comp.BankedPoints = 0;
                HandleResearch(ent, points);
            } else
                RefreshUIOnClients(ent);
        } else {
            SendErrorToClients(ent, Loc.GetString("research-tree-console-error-queue-full"));
            return false;
        }

        return true;
    }

    public bool RemoveResearchFromQueue(Entity<FHResearchTreeComponent?> ent, ProtoId<ResearchTreeNodePrototype> node)
    {
        if (!Resolve(ent, ref ent.Comp))
            return false;

        if (!ent.Comp.Queue.Remove(node))
            return false;
        
        List<ProtoId<ResearchTreeNodePrototype>> toRemove = [];
        foreach (var queuedNode in ent.Comp.Queue)
        {
            var nodeProto = _protoMan.Index(queuedNode);
            if (nodeProto.DependencyChain(_protoMan).Any(p => p.ID == node))
                toRemove.Add(queuedNode);
        }
        ent.Comp.Queue.RemoveAll(toRemove.Contains);

        RefreshUIOnClients(ent);
        return true;
    }

    public void SendErrorToClients(Entity<FHResearchTreeComponent?> ent, string message = "")
    {
        if (!Resolve(ent, ref ent.Comp) || !TryComp(ent, out ResearchServerComponent? serverComp))
            return;
        
        foreach (var client in serverComp.Clients)
            if(TryComp(client, out FHResearchConsoleComponent? console))
                ShowError((client, console), message);
    }

    public void RefreshUIOnClients(Entity<FHResearchTreeComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp) || !TryComp(ent, out ResearchServerComponent? serverComp))
            return;
        
        foreach (var client in serverComp.Clients)
            if(TryComp(client, out FHResearchConsoleComponent? console))
                UpdateUI((client, console));

    }

    public bool IsNodeUnlocked(Entity<FHResearchTreeComponent> ent, ResearchTreeNodePrototype node) =>
        GetUnlockedNodes(ent).Contains(node.ID);
    public HashSet<ProtoId<ResearchTreeNodePrototype>> GetUnlockedNodes(Entity<FHResearchTreeComponent> ent, bool withQueue = true)
    {
        HashSet<ProtoId<ResearchTreeNodePrototype>> result = [];
        
        var unlockedTiers = GetUnlockedTiers(ent);

        foreach (var node in GetTreeNodes(ent))
        {
            var nodeTierUnlocked = unlockedTiers.Contains(node.Tier);
            var nodeRequirementsUnlocked = node.Requires.All(ent.Comp.Researched.Contains);
            if (nodeTierUnlocked && nodeRequirementsUnlocked)
            {
                result.Add(node.ID);
                continue;
            }

            if (withQueue)
            {
                var pointsIncludingQueue = GetTotalPointsSpent(ent) + ent.Comp.Queue.Sum(p => _protoMan.Index(p).Cost);
                var nodeTierUnlockedWithQueue = GetTreeTiers(ent).Where(p => _protoMan.Index(p).UnlocksAt <= pointsIncludingQueue).Contains(node.Tier);
                var nodeRequirementsWithQueue = node.Requires.All(p => ent.Comp.Researched.Contains(p) || ent.Comp.Queue.Contains(p));
                if (nodeTierUnlockedWithQueue && nodeRequirementsWithQueue)
                    result.Add(node.ID);
            }
        }

        return result;
    }

    public void AddResearchModifiers(Entity<FHResearchTreeComponent> ent, List<ProtoId<ResearchTreeUnlockFlagPrototype>> addedFlags)
    {
        foreach (var flag in addedFlags)
        {
            var flagProto = _protoMan.Index(flag);
            if (flagProto.Data is ResearchTreeUnlockFlagBankSizeBonus bankSizeBonus)
                ent.Comp.BankCapacity += bankSizeBonus.Bonus;
            if (flagProto.Data is ResearchTreeUnlockFlagQueueSizeBonus queueSizeBonus)
                ent.Comp.MaxQueueSize += queueSizeBonus.Bonus;
        }
    }

    public void RemoveResearchModifiers(Entity<FHResearchTreeComponent> ent, List<ProtoId<ResearchTreeUnlockFlagPrototype>> removedFlags)
    {
        foreach (var flag in removedFlags)
        {
            var flagProto = _protoMan.Index(flag);
            if (flagProto.Data is ResearchTreeUnlockFlagBankSizeBonus bankSizeBonus)
                ent.Comp.BankCapacity -= bankSizeBonus.Bonus;
            if (flagProto.Data is ResearchTreeUnlockFlagQueueSizeBonus queueSizeBonus)
                ent.Comp.MaxQueueSize -= queueSizeBonus.Bonus;
        }
    }

    public static int GetCurrentBankCapacity(Entity<FHResearchTreeComponent> ent) => ent.Comp.BankCapacity;

    public static int GetCurrentQueueSize(Entity<FHResearchTreeComponent> ent) => ent.Comp.MaxQueueSize;

    public bool IsFlagUnlocked(Entity<FHResearchTreeComponent?> ent, ProtoId<ResearchTreeUnlockFlagPrototype> flag) =>
        Resolve(ent, ref ent.Comp) && ent.Comp.UnlockFlags.Contains(flag);

    public HashSet<ProtoId<ResearchTreeTierPrototype>> GetUnlockedTiers(Entity<FHResearchTreeComponent> ent)
    {
        var spent = GetTotalPointsSpent(ent);
        return GetTreeTiers(ent).Where(p => _protoMan.Index(p).UnlocksAt <= spent).ToHashSet();
    }
    public int GetTotalPointsSpent(Entity<FHResearchTreeComponent> ent) =>
        ent.Comp.Researched.Sum(p => _protoMan.Index(p).Cost) + ent.Comp.Progress.Sum(p => p.Value);

    public HashSet<ProtoId<ResearchTreeTierPrototype>> GetTreeTiers(Entity<FHResearchTreeComponent> ent) =>
        GetTreeNodes(ent).Select(p => p.Tier).Distinct().ToHashSet();

    public HashSet<ResearchTreeNodePrototype> GetTreeNodes(Entity<FHResearchTreeComponent> ent) =>
        _protoMan.Index(ent.Comp.Tree).Nodes.Select(p => _protoMan.Index(p)).ToHashSet();
    
    public List<ProtoId<ResearchTreeNodePrototype>> GetRemovableReseach(Entity<FHResearchTreeComponent> ent) =>
        [.. ent.Comp.Researched.Where(p => !_protoMan.Index(p).Children(_protoMan).Any(e => ent.Comp.Researched.Contains(e.ID)))];
}