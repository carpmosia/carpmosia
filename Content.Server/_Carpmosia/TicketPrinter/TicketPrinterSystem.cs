using Content.Shared.TicketPrinter;
using Robust.Shared.Prototypes;
using Content.Server.Stack;

namespace Content.Server.TicketPrinter;

public sealed class TicketPrinterSystem : SharedTicketPrinterSystem
{
    [Dependency] private readonly StackSystem _stack = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    public override void Initialize()
    {
        base.Initialize();
    }

    /// <summary>
    /// Applies ticket multiplier and spawns tickets, stores any remainder for future spawns
    /// </summary>
    /// <param name="ent">Entity spawning the tickets</param>
    /// <param name="amount">Base amount of tickets to spawn</param>
    protected override void PrintTickets(Entity<TicketPrinterComponent> ent, float amount)
    {
        var proto = ent.Comp.TicketProtoId;
        var spawnAmount = ent.Comp.Remainder + amount * ent.Comp.TicketMultiplier;
        if (spawnAmount <= 0 || string.IsNullOrEmpty(proto.ToString()))
            return;

        if (!_proto.HasIndex(proto)) //does it exist?
            return;

        var tickets = _stack.SpawnMultipleAtPosition(proto, (int)Math.Floor(spawnAmount), Transform(ent).Coordinates);

        foreach (var ticket in tickets)
            _stack.TryMergeToContacts(ticket); //try to make into a single stack

        ent.Comp.Remainder = spawnAmount - (float)Math.Floor(spawnAmount); //can't spawn fractional tickets so store for the future
    }
}
