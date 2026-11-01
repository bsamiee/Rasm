# [EVENT_SOURCING]

Event sourcing stores the transitions of an entity as an append-only history and derives every snapshot and view from it, so persisted data stays as immutable as in-memory values.

## [01]-[EVENTS]

Events are immutable records of something that already happened: past-tense names, fields holding the minimum the occurrence states rather than an entity snapshot, and no rejection or change once stored. Transient notifications stay out of the history. The `[Union]` base holds every fact more than one consumer reads (the entity identity, the balance movement, the freeze), so transitions and views read base members and a new case states its effect where it is declared:

```csharp
[Union]
internal abstract partial record Event {
    private Event(Guid entryId, decimal delta = 0m, bool freezes = false) => (EntryId, Delta, Freezes) = (entryId, delta, freezes);

    public Guid EntryId { get; }
    public decimal Delta { get; }
    public bool Freezes { get; }

    internal sealed record Opened(Guid EntryId, decimal Floor) : Event(EntryId);
    internal sealed record Credited(Guid EntryId, decimal Amount) : Event(EntryId, Amount);
    internal sealed record Debited(Guid EntryId, decimal Amount, Guid Target) : Event(EntryId, -Amount);
    internal sealed record Frozen(Guid EntryId) : Event(EntryId, freezes: true);
}
```

Stores hold one row per event, with the entity id, its position in the entity's history, the append time, and the serializer's case discriminator as headers beside the serialized event, and return one entity's history in occurrence order up to a time bound.

## [02]-[STATE]

Snapshots are derived data for one purpose. The command side holds only what its decisions read, never the transaction list or display fields, and one transition function serves the live event and the replay. Reconstruction folds the history after its opening event, and an empty history or one that starts with another case is no entity:

```csharp
internal sealed record Snapshot(Guid EntryId, decimal Balance, decimal Floor, bool Frozen);

internal static partial class Ledger {
    public static Snapshot Apply(Snapshot snapshot, Event evt) =>
        snapshot with { Balance = snapshot.Balance + evt.Delta, Frozen = snapshot.Frozen || evt.Freezes };

    public static Option<Snapshot> Rebuild(Seq<Event> history) =>
        history is [Event.Opened opened, ..] ? history.Tail.Fold(new Snapshot(opened.EntryId, 0m, opened.Floor, false), Apply) : None;
}
```

Snapshots at a past time are the same `Rebuild` over the history the store returns for that bound.

## [03]-[COMMANDS]

Commands are decisions `Snapshot -> Fin<Event>` that a factory builds from the request's values, so one handler serves every command and a new command is one factory. Checks over one snapshot are independent and join under tuple `Apply`. The handler appends with the history count the decision read as the expected position, the store rejects the append when another command appended first, and creation appends its opening event outside the handler at position 0, which the store rejects for an existing entity:

```csharp
internal static partial class Ledger {
    public static Func<Snapshot, Fin<Event>> Debit(decimal amount, Guid target) =>
        snapshot => (
                Fault.Unless(amount > 0m, Reason.Range, amount),
                Fault.Unless(!snapshot.Frozen, Reason.Refused, snapshot.Frozen),
                Fault.Unless(snapshot.Balance - amount >= snapshot.Floor, Reason.Range, (snapshot.Balance, amount)))
            .Apply(Event (_, _, _) => new Event.Debited(snapshot.EntryId, amount, target))
            .As();

    public static IO<Snapshot> Handle(Func<Guid, IO<Seq<Event>>> load, Func<Event, int, IO<Unit>> append, Guid entryId, Func<Snapshot, Fin<Event>> decide) =>
        from history in load(entryId)
        from snapshot in IO.lift(Rebuild(history).ToFin(Fault.Of(Reason.Missing, entryId)))
        from evt in IO.lift(decide(snapshot))
        from _ in append(evt, history.Count)
        select Apply(snapshot, evt);
}
```

Subscribers (projections, notifications, transfers to other entities) read appended events from the store through a durable subscription with at-least-once delivery, so the append is the one write a command performs and each subscriber is idempotent per history position. A command appends one event, and a subscriber turns it into further events for this or other entities.

## [04]-[QUERIES]

Views are projections shaped for their reader, derived from history and never from the command snapshot. Totals sum a base fact, rows filter the cases that carry it, a period view reads the history before and inside its bound, and rendering derives each row's label from its case through `Switch`:

```csharp
internal sealed record Statement(decimal Opening, Seq<Event> Movements) {
    public decimal Closing => Opening + Movements.Sum(static evt => evt.Delta);

    public static Statement Of(Seq<Event> before, Seq<Event> period) =>
        new(before.Sum(static evt => evt.Delta), period.Filter(static evt => evt.Delta != 0m));
}
```

When replaying for each query grows costly, subscribers maintain each projection incrementally in a store shaped for its queries, and every projection rebuilds from the history.
