# [EVENT_SOURCING]

Functional design applies to persisted data as well as in-memory values, because overwriting rows is shared mutable state even when the server process is stateless. Event sourcing stores the transitions of an entity as an ordered history and derives every snapshot from it.

## [01]-[STORAGE]

Append-only storage replaces updates and deletes with appends: existing data is never overwritten or deleted, new information is recorded as additional data, history stays available for audit, analysis, and reconstruction, and appends avoid the write contention of concurrent overwrites of one record. Event sourcing stores an ordered history of things that happened, and valid-time storage stores facts with the intervals during which they are valid. Event sourcing focuses on transitions rather than snapshots, and the current state is derived data:

```text
initial state = Create(first event)
current state = remaining events.Fold(initial state, Apply)
```

Two snapshots show that state changed and not why, while the event with the prior state determines the next state. Keeping every historical snapshot repeats every unchanged value and still needs a comparison to explain a change, and the event history records the transition itself and derives whichever snapshot is needed.

## [02]-[EVENTS]

Events are immutable, serializable data objects carrying the minimum information about something that already happened:
- The name is past tense (`Created`, `Credited`, `Debited`, `Frozen`)
- The event cannot be rejected or changed
- The payload describes the occurrence, not a mutable entity snapshot
- Events that cause state transitions belong in the persistent history, and transient notifications stay distinct from them

Each event is a sealed record case nested in one abstract partial base that `[Union]` closes, and the shared identity sits on the base through a private constructor that the cases pass it to:

```csharp
[Union]
internal abstract partial record Event {
    private Event(Guid entryId) => EntryId = entryId;

    public Guid EntryId { get; }

    internal sealed record Created(Guid EntryId, string Code) : Event(EntryId);
    internal sealed record Credited(Guid EntryId, decimal Amount) : Event(EntryId);
    internal sealed record Debited(Guid EntryId, decimal Amount, string Target) : Event(EntryId);
    internal sealed record Frozen(Guid EntryId) : Event(EntryId);
}
```

Event types have different payload shapes, so the storage options from most to least suitable are an event store, a document database that accepts heterogeneous documents, and a relational database. A relational event table needs headers (entity id, timestamp, event type) and a payload column for the serialized event, the headers support retrieving one entity's history in order and filtering it by time, and an existing relational store avoids extra operational infrastructure when only part of a system uses event sourcing.

## [03]-[STATE]

State is an immutable snapshot derived for one purpose, and it is not the persisted source of truth. The command side needs only enough state to decide whether a command is allowed (status, code, balance, limit, not the full transaction list), the query side needs read models shaped for each view, and analytics use other projections, because each answers an independent question. The snapshot exposes read-only values and transformation methods that return new instances, and business rules belong in command validation rather than in these methods:

```csharp
internal enum Status { Requested = 0, Active = 1, Frozen = 2 }

internal sealed record Snapshot(Status Status, string Code, decimal Balance, decimal Limit) {
    public Snapshot Credit(decimal amount) => this with { Balance = Balance + amount };
    public Snapshot Debit(decimal amount) => this with { Balance = Balance - amount };
    public Snapshot WithStatus(Status status) => this with { Status = status };
}
```

## [04]-[TRANSITIONS]

A transition after creation is a pure function `Snapshot -> Event -> Snapshot`, and creation is the special case `Created -> Snapshot` with no prior state that can establish initial domain values (a new entry starts active). The transition selects the event case through `Switch` and returns a new snapshot, and one transition function serves both the live occurrence and the replay, because a duplicate implementation risks a live state that history cannot reconstruct. A replayed creation event leaves an existing snapshot unchanged, and a new case fails every `Switch` to compile until it gains an arm:

```csharp
internal static partial class Entry {
    public static Snapshot Create(Event.Created created) => new(Status.Active, created.Code, 0m, 0m);
    public static Snapshot Apply(Snapshot snapshot, Event evt) =>
        evt.Switch(
            snapshot,
            created: static (s, _) => s,
            credited: static (s, e) => s.Credit(e.Amount),
            debited: static (s, e) => s.Debit(e.Amount),
            frozen: static (s, _) => s.WithStatus(Status.Frozen));
}
```

## [05]-[RECONSTRUCTION]

History is retrieved in occurrence order, an empty history means no entity is recorded, and reconstruction returns an optional snapshot rather than a default entity. `Head` on a `Seq<Event>` is the `Option<Event>` that separates a nonexistent entity from a replayable history, `Bind` keeps only a `Created` head, and `Fold` over `Tail` applies the transition function:

```csharp
internal static partial class Entry {
    public static Option<Snapshot> Rebuild(Seq<Event> history) =>
        history.Head
            .Bind(static head => head is Event.Created created ? Some(created) : Option<Event.Created>.None)
            .Map(created => history.Tail.Fold(Create(created), Apply));
}
```

The snapshot at a past time comes from the same computation after excluding later events, with each event's time value as the boundary.

## [06]-[COMMANDS]

Event sourcing separates the command flow from the query flow:

```text
command -> validate -> derive event -> persist and publish
                                      |
                                      v
events  -> fold, map, filter -> projection or view model -> query response
```

Commands are imperative requests (`Debit`) that can be invalid, ignored, or interrupted before completion, and a command and its event can share fields while the conversion adds or derives values. Command handling validates the command's general form, loads the entity's snapshot and validates the requested transition against it, and converts a valid command into an event that it persists and publishes. General validation and state-dependent validation are dependent computations that bind, each rejection is a sealed `Expected` record, the transition returns the event beside the derived snapshot, and the first `guard` calls `ToFin` to establish the query's result type:

```csharp
internal sealed record Debit(Guid EntryId, decimal Amount, string Target) {
    public Event ToEvent() => new Event.Debited(EntryId, Amount, Target);
}
internal sealed record NotActive() : Expected("entry is not active", Codes.NotActive);
internal sealed record InsufficientBalance() : Expected("insufficient balance", Codes.InsufficientBalance);
internal sealed record NotFound() : Expected("entry not found", Codes.NotFound);
internal sealed record InvalidAmount() : Expected("amount is not positive", Codes.InvalidAmount);

internal static partial class Entry {
    public static Fin<(Event Event, Snapshot Snapshot)> Debit(Snapshot snapshot, Debit command) =>
        from _ in guard<Error>(snapshot.Status == Status.Active, new NotActive()).ToFin()
        from __ in guard<Error>(snapshot.Balance - command.Amount >= snapshot.Limit, new InsufficientBalance())
        let evt = command.ToEvent()
        select (Event: evt, Snapshot: Apply(snapshot, evt));
}
```

Loading history returns `IO<Seq<Event>>`, saving an event returns `IO<Unit>`, and the handler composes them in one query over `IO`, where the expected rejection enters the `IO` error channel through `IO.lift(Fin<A>)` beside the failures of loading and saving, so `RunSafe` at the host returns one `Fin` and one `Match` reads both outcomes:

```csharp
internal static class Commands {
    public static IO<(Event Event, Snapshot Snapshot)> Handle(Func<Guid, IO<Seq<Event>>> load, Func<Event, IO<Unit>> save, Debit command) =>
        from _ in guard<Error>(command.Amount > 0m, new InvalidAmount())
        from history in load(command.EntryId)
        from snapshot in IO.lift(Entry.Rebuild(history).ToFin(new NotFound()))
        from result in IO.lift(Entry.Debit(snapshot, command))
        from __ in save(result.Event)
        select result;
}
```

Accepted events can trigger subscribers (external transfers, derived calculations, notifications, projection updates), and persisting the event and publishing it to them must behave atomically, because saving and then crashing before subscribers see it leaves the system inconsistent. The guarantee depends on the storage and messaging infrastructure, durable subscriptions can use the event store as the event stream with at-least-once delivery, and the handler then only saves the event. Prefer one resulting event per command, and let downstream handlers translate it into further events for the same or other entities. Command-side handlers perform follow-up actions and can emit further events, and query-side handlers update read models. A rule that depends on the snapshot observed before the event is created needs one process per entity that serializes its commands, with persistence inside that process.

## [07]-[QUERIES]

Users consume view models shaped for their needs, not raw event logs or the command-side snapshot, and the query side derives each view from history: `Fold` computes totals and balances, `Map` converts relevant events into view data, `Choose` with an `Option` result converts matching events and omits the rest, and time filters select the history for a period or a point in time. For a statement, fold the events before the period for the opening balance, fold through the period for the closing balance, and convert only the transaction events into rows:

```csharp
internal sealed record Row(string Description, decimal Amount);

internal static class Queries {
    public static Seq<Row> Rows(Seq<Event> history) =>
        history.Choose(static evt => evt.Switch<Option<Row>>(
            created: static _ => Option<Row>.None,
            credited: static e => Some(new Row("credit", e.Amount)),
            debited: static e => Some(new Row(e.Target, -e.Amount)),
            frozen: static _ => Option<Row>.None));
    public static decimal Total(Seq<Row> rows) => rows.Fold(0m, static (total, row) => total + row.Amount);
}
```

As history grows, replaying it for every query becomes expensive, so the query side subscribes to new events and maintains cached projections incrementally, publishes changed views to connected clients where that is wanted, or uses a dedicated query database shaped for filtering, and the system rebuilds every projection from the event history, which remains the source of truth. Command and query concerns can stay separate inside one application or deploy and scale independently, query load benefits from more instances, and command processing can need tighter coordination of writes.

## [08]-[FIT]

The domain decides between event sourcing and valid-time storage. Choose event sourcing when domain events are meaningful business occurrences rather than renamed CRUD operations, events drive more than one consequence, commands and the views users consume have different shapes, and reconstructing how and why an entity evolved is central (an auction, where bids and closure are occurrences and clients submit single actions while consuming item details, bid histories, and purchase lists). Prefer valid-time storage when attributes and their validity intervals are the principal domain concepts (product administration that records creation, retirement, and modification and needs a temporal history of facts).
