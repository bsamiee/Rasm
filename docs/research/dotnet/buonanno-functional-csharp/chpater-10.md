# Event Sourcing as Functional Persistence

## The storage model

Functional design applies to persisted data as well as in-memory values. A database that overwrites rows is still shared mutable state, even when the server process itself is stateless.

Append-only storage replaces create-read-update-delete with create-read-append:
- Existing data is never overwritten or deleted.
- New information is recorded as additional data.
- Historical information remains available for audit, analysis, and reconstruction.
- Appends avoid the hot-cell contention caused when concurrent requests overwrite the same value.

Two immutable-storage models follow this rule:
- Event-based: store an ordered history of things that happened.
- Assertion-based: store facts together with the time intervals during which they are true.

Event sourcing focuses on transitions rather than snapshots. The current state is derived data:

```text
initial state = Create(first event)
current state = remaining events.Aggregate(initial state, Apply)
```

Given only two snapshots, determining why the state changed can be difficult. Given a prior state and an event, computing the next state is direct.

Keeping every historical snapshot is also wasteful: each snapshot repeats all values that did not change, while explaining a change still requires comparing snapshots. An event history records the transition itself and can derive whichever snapshot is needed.

## Core model

### Events

An event is a small, immutable, serializable data object containing the minimum information needed to represent something that already happened.

- Its name is past tense: `CreatedAccount`, `DepositedCash`, `DebitedTransfer`.
- It cannot be rejected or changed after the fact.
- Its payload describes the occurrence, not a mutable entity snapshot.
- Events that cause state transitions belong in persistent event history; transient notifications must be distinguished from them.

Event types naturally have different payload shapes. In order of suitability, they can be stored in a specialized event database, a document database that accepts heterogeneous documents, or a relational database. A relational event table needs common headers such as entity ID, timestamp, and event type, with the serialized payload in a wide column; those headers support retrieving one entity's history in order and filtering it by time. Reusing an existing relational store can be reasonable when only part of a system is event-sourced and avoiding extra operational infrastructure matters.

### State

State is an immutable snapshot derived for a specific purpose. It is not necessarily the persisted source of truth.

The command side needs only enough state to decide whether a command is allowed. A bank account decision model may require status, currency, balance, and allowed overdraft, but not a complete transaction list.

The query side needs view-oriented state, such as a monthly statement and its transactions. Analytics may use other projections. These models can differ because they answer independent questions.

State objects should expose read-only values and copy operations that return new instances. Copy operations such as `Credit`, `Debit`, or `WithStatus` transform data; business permission rules belong in command validation.

### State transitions

A normal transition is a pure function:

```text
State -> Event -> State
```

Creation is the special case with no prior state:

```text
CreationEvent -> State
```

Creation builds the first state from the creation event and may establish initial domain values, such as making a newly created account active. Later transition logic pattern-matches on the event type and returns a new state. The same transition function must be used both when an event first occurs and when history is replayed. Duplicating these implementations risks producing a live state that cannot be reconstructed later.

Conceptually:

```csharp
AccountState Apply(this AccountState state, Event evt)
    => new Pattern
    {
        (DepositedCash e)   => state.Credit(e.Amount),
        (DebitedTransfer e) => state.Debit(e.DebitedAmount),
        (FrozeAccount _)    => state.WithStatus(AccountStatus.Frozen)
    }.Match(evt);
```

### Pattern matching tools

Expression-oriented pattern matching keeps transitions as expressions. For a type with a fixed set of cases, such as an option or functional list, a type-specific `Match` method can require handlers for every known case. An open inheritance hierarchy such as `Event` needs an extensible matcher instead: register type-specific functions, optionally supply a default, and evaluate the first handler whose input type matches the value.

Structural matching is also useful for sequences. An `IEnumerable` matcher can accept one function for an empty sequence and another for its head and tail. This makes the distinction between a nonexistent entity and a replayable history explicit.

## Reconstructing an entity

History must be retrieved in occurrence order. An empty history means no entity is recorded, so reconstruction returns an optional state rather than inventing a default entity.

For a non-empty history:
1. Treat the first event as the required entity-creation event.
2. Construct the initial state from that event.
3. Fold the remaining events with the transition function.

```csharp
Option<AccountState> Rebuild(IEnumerable<Event> history)
    => history.Match(
        Empty: () => None,
        Otherwise: (head, tail) =>
            Some(tail.Aggregate(
                seed: Create((CreatedAccount)head),
                func: Apply)));
```

State at a past time is obtained through the same computation after excluding later events. This makes temporal queries and audit history intrinsic to the model.

## CQRS architecture

Event sourcing separates two different flows.

```text
command -> validate -> derive event -> persist and publish
                                      |
                                      v
events  -> fold/map/filter -> projection or view model -> query response
```

### Command side

A command is an imperative request, such as `MakeTransfer`. Unlike an event, it may be invalid, ignored, or interrupted before completion.

Commands are named imperatively; events are named in the past tense because they record facts that can no longer fail. A command and its primary event generally carry the same information, so conversion is mostly field-by-field, with any required variations. The primary event directly affects one entity, then its publication may cause handlers to derive events for that or other entities.

Command handling performs three jobs:
1. Validate the command's general form.
2. Load the entity's decision state and validate the requested transition against it.
3. Convert a valid command into an event, persist it, and publish it.

The transition operation can return both the event and the newly derived state:

```csharp
Validation<(Event Event, AccountState State)> Debit(
    AccountState account,
    MakeTransfer command)
{
    if (account.Status != AccountStatus.Active)
        return Errors.AccountNotActive;

    if (account.Balance - command.Amount < account.AllowedOverdraft)
        return Errors.InsufficientBalance;

    Event evt = command.ToEvent();
    return (evt, Apply(account, evt));
}
```

General input validation and state-dependent business validation are dependent computations, so they compose with `Bind`. Persistence is a side effect that runs only for a valid result. A pass-through `Do` operation can perform it while retaining the event-state pair for the final response.

Validation errors and I/O failures are different effects. `Validation` models expected command rejection, while retrieving history or saving an event can fail exceptionally; a complete workflow must compose `Validation` with that additional effect.

### Persisting and publishing

An accepted event may trigger multiple subscribers: external transfers, reserve calculations, notifications, and projection updates. Persisting the event and making it available to handlers must behave atomically. Saving an event and then crashing before it reaches subscribers can leave the system inconsistent.

The exact guarantee depends on storage and messaging infrastructure. Durable subscriptions can make the event store itself the publication stream and provide at-least-once delivery, allowing the command handler's boundary to focus on saving the event.

A command should normally produce one primary event. Downstream handlers may translate that event into further events, often for other entities, without making the original command workflow responsible for every consequence.

Event handlers therefore serve two distinct roles: command-side handlers perform consequences and may emit further events; query-side handlers update derived view models.

### Query side

Users consume view models shaped for their needs, not raw event logs or the command-side state. The query side derives each view from history using ordinary functional transformations:
- `Aggregate` computes totals and balances.
- `Map` converts relevant events into view data.
- `Bind` with an optional result both converts matching events and omits nonmatching events.
- Time filters select the history needed for a period or point-in-time view.

For an account statement, fold events before the period to obtain the starting balance, fold through the period to obtain the ending balance, and convert only transaction events into statement rows.

Replaying a long history for every query can be expensive. The query side can subscribe to new events, maintain cached projections incrementally, and optionally publish changed views to connected clients. It can instead use a dedicated query database shaped for efficient filtering. These projections remain rebuildable derivatives; the event history remains the source of truth.

CQRS does not require two deployed applications. Command and query concerns can remain separate inside one application, or they can be deployed and scaled independently. Query load often benefits from multiple instances, while command processing may require tighter coordination of writes.

## Choosing the immutable-storage approach

Event-based and assertion-based storage both preserve an audit trail, support point-in-time state, and avoid overwriting stored facts. They differ in what the domain naturally expresses.

Choose event sourcing when:
- Domain events are meaningful business occurrences rather than renamed CRUD operations.
- Events naturally drive multiple consequences.
- Commands and the views consumed by users have substantially different shapes.
- Reconstructing how and why an entity evolved is central.

An auction is a strong fit: bids and auction closure are meaningful occurrences, while clients submit individual actions but consume item details, bid histories, and purchase lists.

Prefer assertion-based storage when the domain is primarily about attributes whose values become valid at particular times. Product administration is closer to this model when its apparent events are merely create, retire, or modify operations and the main requirement is a temporal record of changing facts.
