<!-- Fully integrated into .claude/skills/dotnet-coding/references/event-sourcing.md -->
# [EVENT_SOURCING]

<!-- Integrated into .claude/skills/dotnet-coding/references/event-sourcing.md
## [01]-[STORAGE_MODEL]

Functional design applies to persisted data as well as in-memory values. Overwriting rows is still shared mutable state, even when the server process is stateless.

Append-only storage replaces updates and deletes with appends:
- Existing data is never overwritten or deleted
- New information is recorded as additional data
- Historical information remains available for audit, analysis, and reconstruction
- Appends avoid write contention caused by concurrent overwrites of the same record

Append-only storage models follow this rule:
- Event sourcing: store an ordered history of things that happened
- Valid-time storage: store facts with the time intervals during which they are valid

Event sourcing focuses on transitions rather than snapshots. The current state is derived data:

```text
initial state = Create(first event)
current state = remaining events.Fold(initial state, Apply)
```

Two snapshots show that state changed but do not explain why. Event and prior state determine the next state.

Keeping every historical snapshot is wasteful: each snapshot repeats all values that did not change, while explaining a change still requires comparing snapshots. Event history records the transition itself and derives whichever snapshot is needed.
-->

<!-- Integrated into .claude/skills/dotnet-coding/references/event-sourcing.md
## [02]-[CORE_MODEL]
-->

<!-- Integrated into .claude/skills/dotnet-coding/references/event-sourcing.md
### [02.1]-[EVENTS]

Events are immutable, serializable data objects carrying the minimum information about something that already happened.

- Its name is past tense: `CreatedAccount`, `DepositedCash`, `DebitedTransfer`
- It cannot be rejected or changed
- Its payload describes the occurrence, not a mutable entity snapshot
- Events that cause state transitions belong in persistent event history, and transient notifications must be distinguished from them

Each event is a sealed record case nested in one abstract partial base record, and `[Union]` closes the set:

```csharp
[Union]
internal abstract partial record Event {
    private Event(Guid accountId) => AccountId = accountId;

    public Guid AccountId { get; }

    internal sealed record CreatedAccount(Guid AccountId, string Currency) : Event(AccountId);
    internal sealed record DepositedCash(Guid AccountId, decimal Amount) : Event(AccountId);
    internal sealed record DebitedTransfer(Guid AccountId, decimal DebitedAmount, string Beneficiary) : Event(AccountId);
    internal sealed record FrozeAccount(Guid AccountId) : Event(AccountId);
}
```

Event types have different payload shapes. From most to least suitable, the storage options are an event store, a document database that accepts heterogeneous documents, and a relational database. Relational event tables need headers (entity ID, timestamp, event type) and a payload column for serialized event data. These headers support retrieving one entity's history in order and filtering it by time. Existing relational stores avoid extra operational infrastructure when only part of a system uses event sourcing.
-->

<!-- Integrated into .claude/skills/dotnet-coding/references/event-sourcing.md
### [02.2]-[STATE]

State is an immutable snapshot derived for a specific purpose. It is not necessarily the persisted source of truth.

The command side needs only enough state to decide whether a command is allowed. For a bank account, this state can include status, currency, balance, and allowed overdraft, but not a complete transaction list.

The query side needs read models (a monthly statement and its transactions). Analytics can use other projections. These models can differ because they answer independent questions.

State objects expose read-only values and transformation methods that return new instances. Methods (`Credit`, `Debit`, `WithStatus`) transform data. Business rules belong in command validation.

```csharp
internal enum AccountStatus {
    Requested = 0,
    Active = 1,
    Frozen = 2,
}

internal sealed record AccountState(AccountStatus Status, string Currency, decimal Balance, decimal AllowedOverdraft) {
    public AccountState Credit(decimal amount) => this with { Balance = Balance + amount };
    public AccountState Debit(decimal amount) => this with { Balance = Balance - amount };
    public AccountState WithStatus(AccountStatus status) => this with { Status = status };
}
```
-->

<!-- Integrated into .claude/skills/dotnet-coding/references/event-sourcing.md
### [02.3]-[STATE_TRANSITIONS]

Transitions after creation are pure functions:

```text
State -> Event -> State
```

Creation is the special case with no prior state:

```text
CreationEvent -> State
```

Creation builds the first state from the creation event and can establish initial domain values (a new account starts active). Later transition logic selects the event case through `Switch` and returns a new state. The same transition function must be used both when an event first occurs and when history is replayed. Duplicating these implementations risks producing a live state that cannot be reconstructed later.

```csharp
internal static partial class Account {
    public static AccountState Create(Event.CreatedAccount evt) => new(AccountStatus.Active, evt.Currency, 0m, 0m);
    public static AccountState Apply(AccountState state, Event evt) =>
        evt.Switch(
            state,
            createdAccount: static (s, _) => s,
            depositedCash: static (s, e) => s.Credit(e.Amount),
            debitedTransfer: static (s, e) => s.Debit(e.DebitedAmount),
            frozeAccount: static (s, _) => s.WithStatus(AccountStatus.Frozen));
}
```
-->

<!-- Integrated into .claude/skills/dotnet-coding/references/event-sourcing.md
### [02.4]-[PATTERN_MATCHING]

Expression-oriented pattern matching keeps transitions as expressions. For a type with a fixed set of cases (an option or functional list), a type-specific `Match` method can require handlers for every case. `Event.Switch` takes one arm per case. New cases fail to compile until each `Switch` names them. Replayed creation events leave an existing state unchanged.

Structural matching is useful for sequences. `Seq<Event>` exposes `Head` as an `Option<Event>` and `Tail` as the remaining `Seq<Event>`. This makes the distinction between a nonexistent entity and a replayable history explicit.
-->

<!-- Integrated into .claude/skills/dotnet-coding/references/event-sourcing.md
## [03]-[ENTITY_RECONSTRUCTION]

History must be retrieved in occurrence order. Empty history means no entity is recorded. Reconstruction returns an optional state, not a default entity.

Reconstruct a non-empty history as follows:
1. Treat the first event as the required entity-creation event
2. Construct the initial state from that event
3. Fold the remaining events with the transition function

```csharp
internal static partial class Account {
    public static Option<AccountState> Rebuild(Seq<Event> history) =>
        history.Head
            .Bind(static head => head is Event.CreatedAccount created ? Some(created) : Option<Event.CreatedAccount>.None)
            .Map(created => history.Tail.Fold(Create(created), Apply));
}
```

`Head` is `None` for an empty history, `Bind` keeps only a `CreatedAccount` head, and `Fold` over `Tail` applies the transition function.

State at a past time is obtained through the same computation after excluding later events. Each event's time value defines the time boundary.
-->

<!-- Integrated into .claude/skills/dotnet-coding/references/event-sourcing.md
## [04]-[CQRS]

Event sourcing separates command and query flows.

```text
command -> validate -> derive event -> persist and publish
                                      |
                                      v
events  -> fold/map/filter -> projection or view model -> query response
```
-->

<!-- Integrated into .claude/skills/dotnet-coding/references/event-sourcing.md
### [04.1]-[COMMAND_SIDE]

Imperative requests (`MakeTransfer`) are commands. Unlike events, they can be invalid, ignored, or interrupted before completion.

Commands are named imperatively because they are requests. Commands and resulting events can share fields, but conversion can add or derive values.

Command handling has these jobs:
1. Validate the command's general form
2. Load the entity's state and validate the requested transition against it
3. Convert a valid command into an event, persist it, and publish it

The transition operation can return both the event and the derived state. Each rejection is a sealed `Expected` record. The first `guard` calls `ToFin`, which establishes the LINQ query's result type. Later `guard<Error>` clauses bind without another conversion:

```csharp
internal sealed record MakeTransfer(Guid AccountId, decimal Amount, string Beneficiary) {
    public Event ToEvent() => new Event.DebitedTransfer(AccountId, Amount, Beneficiary);
}
internal sealed record AccountNotActive() : Expected("account is not active", 1401);
internal sealed record InsufficientBalance() : Expected("insufficient balance", 1402);
internal sealed record AccountNotFound() : Expected("account not found", 1403);
internal sealed record InvalidAmount() : Expected("amount is not positive", 1404);
```

```csharp
internal static partial class Account {
    public static Fin<(Event Event, AccountState State)> Debit(AccountState account, MakeTransfer command) =>
        from _ in guard<Error>(account.Status == AccountStatus.Active, new AccountNotActive()).ToFin()
        from __ in guard<Error>(account.Balance - command.Amount >= account.AllowedOverdraft, new InsufficientBalance())
        let evt = command.ToEvent()
        select (Event: evt, State: Apply(account, evt));
}
```

General input validation and state-dependent business validation are dependent computations. They compose with `Bind`. Persistence is a side effect that runs only for a valid result. Loading history returns `IO<Seq<Event>>`, and saving an event returns `IO<Unit>`. `Atom<Seq<Event>>` holds the events in memory:

```csharp
internal static class EventStore {
    public static IO<Seq<Event>> Load(Atom<Seq<Event>> store, Guid accountId) =>
        store.ValueIO.Map(events => events.Filter(evt => evt.AccountId == accountId));
    public static IO<Unit> Save(Atom<Seq<Event>> store, Event evt) =>
        store.SwapIO(events => events.Add(evt)).Map(static _ => unit);
}
```

The command handler composes these operations in one LINQ expression over `IO`:

```csharp
internal static class Commands {
    public static IO<(Event Event, AccountState State)> Handle(Atom<Seq<Event>> store, MakeTransfer command) =>
        from _ in guard<Error>(command.Amount > 0m, new InvalidAmount())
        from history in EventStore.Load(store, command.AccountId)
        from account in IO.lift(Account.Rebuild(history).ToFin(new AccountNotFound()))
        from result in IO.lift(Account.Debit(account, command))
        from __ in EventStore.Save(store, result.Event)
        select result;
}
```

Expected rejection and I/O failure are different effects. Expected rejection is a `Fin` from the pure transition, and the effect carries it on the `IO` error channel through `IO.lift(Fin<A>)`. Failures retrieving history or saving an event arrive on the same channel. `RunSafe` at the host returns one `Fin`, and one `Match` reads both outcomes.
-->

<!-- Integrated into .claude/skills/dotnet-coding/references/event-sourcing.md
### [04.2]-[PERSISTENCE_AND_PUBLISHING]

Accepted events can trigger multiple subscribers: external transfers, reserve calculations, notifications, and projection updates. Persisting the event and making it available to handlers must behave atomically. Saving an event and then crashing before it reaches subscribers can leave the system inconsistent.

The guarantee depends on storage and messaging infrastructure. Durable subscriptions can use the event store as the event stream and provide at-least-once delivery. The command handler then only saves the event.

Prefer one resulting event per command. Downstream handlers can translate that event into further events for the same or other entities without making the original command workflow responsible for every consequence.

Event handlers serve distinct roles. Command-side handlers perform follow-up actions and can emit further events. Query-side handlers update read models.
-->

<!-- Integrated into .claude/skills/dotnet-coding/references/event-sourcing.md
### [04.3]-[QUERY_SIDE]

Users consume view models shaped for their needs, not raw event logs or the command-side state. The query side derives each view from history using functional transformations:
- `Fold` computes totals and balances
- `Map` converts relevant events into view data
- `Choose` with an `Option` result both converts matching events and omits nonmatching events
- Time filters select the history needed for a period or point-in-time view

For an account statement, fold events before the period to obtain the starting balance, fold through the period to obtain the ending balance, and convert only transaction events into statement rows.

```csharp
internal sealed record StatementRow(string Description, decimal Amount);

internal static class Queries {
    public static Seq<StatementRow> Rows(Seq<Event> history) =>
        history.Choose(static evt => evt.Switch<Option<StatementRow>>(
            createdAccount: static _ => Option<StatementRow>.None,
            depositedCash: static e => Some(new StatementRow("deposit", e.Amount)),
            debitedTransfer: static e => Some(new StatementRow(e.Beneficiary, -e.DebitedAmount)),
            frozeAccount: static _ => Option<StatementRow>.None));
    public static decimal Total(Seq<StatementRow> rows) => rows.Fold(0m, static (total, row) => total + row.Amount);
}
```

As history grows, replaying it for every query becomes expensive. The query side can subscribe to new events, maintain cached projections incrementally, and optionally publish changed views to connected clients. Alternatively, it can use a dedicated query database shaped for filtering. The system can rebuild these projections from event history, which remains the source of truth.

CQRS does not require two deployed applications. Command and query concerns can remain separate inside one application, or they can be deployed and scaled independently. Query load benefits from multiple instances, while command processing can require tighter coordination of writes.
-->

<!-- Integrated into .claude/skills/dotnet-coding/references/event-sourcing.md
## [05]-[WHEN_TO_USE]

The domain determines whether event sourcing or valid-time storage fits.

Choose event sourcing when:
- Domain events are meaningful business occurrences rather than renamed CRUD operations
- Events drive multiple consequences
- Commands and the views consumed by users have different shapes
- Reconstructing how and why an entity evolved is central

Auctions illustrate these conditions: bids and auction closure are meaningful occurrences, while clients submit individual actions but consume item details, bid histories, and purchase lists.

Prefer valid-time storage when attributes and their validity intervals are the principal domain concepts. Product administration fits this model when it records creation, retirement, and modification and requires a temporal history of facts.
-->
