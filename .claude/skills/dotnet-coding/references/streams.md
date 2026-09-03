# [STREAMS]

Covers values that arrive over time and the process that owns a shared value: the observable model, the 3-layer program structure, stream creation, operators and partitioning, per-item failure, logic across events, backpressure, and the agent model with its replies, entity coordination, and registry.
- See `dotnet-coding-languageext` for the `Source`, `Sink`, `Conduit`, `Buffer`, and pipe API

## [01]-[MODEL]

`IObservable<T>` combines the multiplicity of `IEnumerable<T>` with the delivery over time of `Task<T>`, an enumerable is an observable that produces all its values synchronously and a task an observable that produces one value:

| [INDEX] | [CARDINALITY] | [SYNCHRONOUS]    | [ASYNCHRONOUS]   |
| :-----: | :------------ | :--------------- | :--------------- |
|  [01]   | One value     | `T`              | `Task<T>`        |
|  [02]   | Many values   | `IEnumerable<T>` | `IObservable<T>` |

Observables push notifications to an observer under the protocol `OnNext* (OnCompleted | OnError)?`: `OnNext(T)` delivers zero or more values, `OnCompleted()` ends the stream, `OnError(Exception)` ends it abnormally, a stream can run forever, and nothing emits after either terminal notification. `Subscribe` connects producer and consumer and returns the `IDisposable` that owns the subscription lifetime, which the consumer scopes or disposes, and a source that never completes depends on that disposal. `Source.lift(IObservable<A>)` supplies the observer once, and `Reduce(seed, f)` folds every value into one `IO<S>` that the host runs.

## [02]-[STRUCTURE]

Stream programs have 3 layers, and the separation keeps the dataflow composable and shows where effects run and resources are managed:
1. Acquire sources by adapting callbacks, tasks, collections, or external producers into observables
2. Describe the dataflow by transforming and combining streams with operators, with no side effect in this layer
3. Run effects at the boundary by subscribing only to the final streams, performing output, persistence, and diagnostics in observers, and reducing a `Source<A>` once so the host runs the resulting `IO<S>`

## [03]-[CREATION]

Subscription behavior depends on the source, not every observable is lazy. Callback-based producers (a message subscription) enter through `Event.from`, event-based APIs through `FromEvent` or `FromEventPattern` of `System.Reactive`, and a `Subject<T>` belongs only at a callback boundary that no dedicated source expresses directly, which keeps observer calls out of the stream definition.

## [04]-[OPERATORS]

Operators produce new observables in place of handling individual events imperatively, and the Rx names map onto `Source<A>`:

| [INDEX] | [OPERATOR]      | [DESCRIPTION]                                                                        | [SOURCE]              |
| :-----: | :-------------- | :----------------------------------------------------------------------------------- | :-------------------- |
|  [01]   | `Select`        | Maps each emitted value                                                              | `Map`                 |
|  [02]   | `SelectMany`    | Maps each value to an observable or task, then flattens the inner values             | Query `from`          |
|  [03]   | `Where`         | Retains values satisfying a predicate                                                | `Filter`              |
|  [04]   | `Take`, `Skip`  | Retains the first count or timespan of values, or discards an initial portion        | `Take`, `Skip`        |
|  [05]   | `First`         | Reduces the stream to its first value                                                | `Take(1).Last()`      |
|  [06]   | `Concat`        | Emits the first stream, then subscribes to the second after the first completes      | `Combine`             |
|  [07]   | `StartWith`     | Prefixes a stream with initial values                                                | `pure` then `Combine` |
|  [08]   | `Merge`         | Interleaves values from streams as they arrive                                       | `Source.merge`        |
|  [09]   | `CombineLatest` | Recomputes from the latest values whenever either source changes, after both emitted | none                  |
|  [10]   | `Zip`           | Pairs each value with one matching partner                                           | `Zip`                 |
|  [11]   | `Scan`          | Emits every successive accumulated state                                             | `Scan` on `Seq`       |
|  [12]   | `GroupBy`       | Splits one stream into keyed streams                                                 | none                  |

Queries over `Source<A>` with a `from` per stage flatten the inner source of each value instead of blocking on it. `Combine` depends on completion: if its first source never completes, the second is never observed. Partitioning is the stream form of branching, each branch transforms independently, normalizes to a common type, and rejoins with `Source.merge`:

```csharp
internal static class Branches {
    public static (Source<A> Passed, Source<A> Failed) Partition<A>(this Source<A> source, Func<A, bool> predicate) =>
        (source.Filter(predicate), source.Filter(value => !predicate(value)));
    public static IO<int> Rejoined(Source<int> source) {
        (Source<int> passed, Source<int> failed) = source.Partition(static item => item > 1);
        return Source.merge(passed.Map(static item => item * 10), failed).Reduce(0, static (sum, item) => sum + item);
    }
}
```

## [05]-[FAILURES]

`OnError` is terminal: when a derived stream reports an error, that stream and every downstream stream terminate while upstream streams continue, only part of the dataflow stays active. Expected per-item failures never use that channel. Apply the operation with `Map`, return a `Fin<R>` per item, keep both cases in one stream by translating them to a common output with `Match`, or reduce the stream into values and errors:

```csharp
internal sealed record UnknownCode() : Expected("unknown code", 1901);

internal static class Failures {
    public static Fin<decimal> Rate(HashMap<string, decimal> table, string code) => table.Find(code).ToFin(new UnknownCode());
    public static Source<Fin<decimal>> Outcomes(HashMap<string, decimal> table, Source<string> codes) => codes.Map(code => Rate(table, code));
    public static Source<string> Outputs(HashMap<string, decimal> table, Source<string> codes) =>
        Source.pure("Enter a code").Combine(
            Outcomes(table, codes).Map(static outcome => outcome.Match(
                Succ: static rate => rate.ToString(CultureInfo.InvariantCulture),
                Fail: static error => error.Message)));
    public static IO<(Seq<decimal> Rates, Seq<Error> Errors)> Partitioned(HashMap<string, decimal> table, Source<string> codes) =>
        Outcomes(table, codes).Reduce(
            (Rates: Seq<decimal>(), Errors: Seq<Error>()),
            static (state, outcome) => outcome.Match(
                Succ: rate => (state.Rates.Add(rate), state.Errors),
                Fail: error => (state.Rates, state.Errors.Add(error))));
}
```

One malformed message then cannot terminate a long-lived processing branch.

## [06]-[EVENTS]

Reactive streams express logic in which processing a new event depends on earlier events or another source. Pairing every value with its successor makes a transition explicit, and filtering the pairs recognizes a multi-key sequence without a mutable state machine:

```csharp
internal static class Transitions {
    public static Source<(A Previous, A Current)> PairWithPrevious<A>(this Source<A> source) =>
        source.Zip(source.Skip(1)).Map(static pair => (Previous: pair.First, Current: pair.Second));
}
```

`Skip(1)` shifts the second subscription by one value and `Zip` pairs each value with its successor. This subscribes to the source twice, each subscription observes values from its own subscription time, and the meaning depends on how the source behaves when subscribed more than once.

When one source emits more often than the output requires, reduce it before combining: `Zip` is the choice when each value has one matching partner, and `CombineLatest` when either input invalidates the derived value. When production outpaces consumption, the `Buffer<A>` given to `Conduit.make` states the policy, and the policy reflects whether intermediate values can be dropped, delayed, grouped, or preserved, where Rx names the time-based and grouping-based counterparts `Sample`, `Throttle`, `Debounce`, `Buffer`, and `Window`:

```csharp
internal static class Backpressure {
    public static Source<decimal> Converted(Source<decimal> amounts, Source<decimal> rates) =>
        amounts.Zip(rates).Map(static pair => pair.First * pair.Second);
}
```

`Scan` is a running fold that emits each accumulated state where `Aggregate` waits for completion, and it detects a state crossing: `Reduce` groups the amounts of each key, `Scan` carries each balance forward with the seed emitted first, and `Zip` against `Tail` pairs each balance with its predecessor, the filter sees only a crossing from nonnegative to negative:

```csharp
internal sealed record Entry(Guid Key, decimal Amount);

internal static class Ledger {
    public static IO<Seq<Guid>> Crossed(Source<Entry> entries) =>
        entries
            .Reduce(HashMap<Guid, Seq<decimal>>(), static (ledger, entry) =>
                ledger.AddOrUpdate(entry.Key, amounts => amounts.Add(entry.Amount), Seq(entry.Amount)))
            .Map(static ledger => toSeq(ledger.Filter(WentNegative).Keys));
    private static bool WentNegative(Seq<decimal> amounts) {
        Seq<decimal> balances = amounts.Scan(0m, static (balance, amount) => balance + amount);
        return balances.Zip(balances.Tail).Exists(static step => step.First >= 0m && step.Second < 0m);
    }
}
```

## [07]-[AGENTS]

Parallel computations with independent inputs compute partial results and combine them without shared mutation, and some services need one application-wide sequence, cache, or representation of a unique real-world entity, where a copy per thread breaks correctness or defeats the purpose. When one logical value must be shared, serialize the operations that can change it, and each strategy has its scope:

| [INDEX] | [STRATEGY]                   | [SCOPE]                                   | [LIMITATION]                                             |
| :-----: | :--------------------------- | :---------------------------------------- | :------------------------------------------------------- |
|  [01]   | Lock                         | Arbitrary critical section                | Blocks threads, can deadlock or serialize unrelated work |
|  [02]   | Compare-and-swap (`Atom`)    | One atomically replaced value             | Too narrow for many multi-value invariants               |
|  [03]   | Transactional memory (`Ref`) | Coordinated updates with an atomic commit | Retries on conflict, transaction bodies hold no effects  |
|  [04]   | Message passing (agent)      | State owned by a process, one handler     | Requires ownership, granularity, and lifecycle design    |

Agents have an inbox that queues messages, state that only the agent owns, and a processing loop that handles one message at a time, and for each message the processing function can perform effects, send messages, create agents, and compute the state for the next message, the state is the fold of all messages received. The invariants are that no caller reads or mutates the owned state, that messages for one agent are processed sequentially, and that the state values passed through the loop are immutable snapshots, because a mutable state object lets code outside the loop modify it concurrently. The minimal implementation keeps the state in the accumulator of a fold over the inbox, not in a private field, and avoids a recursive loop, which is not stack-safe in C#:

```csharp
internal static class Agent {
    public static Conduit<M, M> Inbox<M>() => Conduit.make(Buffer<M>.Unbounded);
    public static IO<ForkIO<S>> Start<S, M>(Conduit<M, M> inbox, S initial, Func<S, M, S> process) =>
        inbox.Reduce(initial, (state, message) => Reduced.ContinueIO(process(state, message))).Fork();
    public static IO<ForkIO<S>> Start<S, M>(Conduit<M, M> inbox, S initial, Func<S, M, IO<S>> process) =>
        inbox.Reduce(initial, (state, message) => process(state, message).Map(Reduced.Continue)).Fork();
}
```

`Reduce` runs the fold inside the conduit and calls one handler at a time, `Reduced.ContinueIO(next)` keeps the loop running and `Reduced.DoneIO(next)` ends it from inside the reducer, running the `IO<ForkIO<S>>` starts the loop, and `Await` yields the final state after `Complete()` closes the inbox. The second overload accepts an effectful processing function, and a stateless agent uses `Unit` as its state to serialize effects without retaining a value. Callers hold the inbox and the message contract, and the state type appears only in the returned `ForkIO<S>`.

Putting every request through one agent makes the whole service sequential, align each agent with the smallest independently mutable resource with an invariant to protect, and serialize by dependency on shared state, not by request type or application layer. For a keyed cache, a coordinator owns only the registry of per-key agents and delegates immediately, requests for different keys progress concurrently, and requests for the same key queue behind one owner, which prevents duplicate lookups for that key. Each per-key agent holds an `Option<A>` of its cached value in its accumulator, `None` at first and `Some` after the first lookup, the processing function decides whether a remote lookup is needed, expiry and error handling remain explicit design concerns, and one agent sends every reply only when sending is fire-and-forget and does not delay its queue.

Agents and actors share exclusive ownership, inboxes, sequential processing, and message-based cooperation, and differ in boundary: an agent is local to one process, referenced as an instance, unsupervised, and can leak mutable state when the discipline lapses, while an actor can run across machines, is referenced by location-transparent identity, is supervised, and prevents shared references through serialized messages. Use agents when all coordinated access passes through one process, and an actor system, with its own terminology, persistence, transport, and delivery guarantees, only when ownership must span processes. Agent messaging is command-oriented and effectful and is not a value-returning pipeline, either use a unidirectional event-driven flow between agents, or keep agents as private concurrency primitives behind value-returning APIs, and in both styles keep domain decisions in pure functions and use the agent only to order transitions and effects.

## [08]-[REPLIES]

Fire-and-forget `Post` supports a unidirectional flow but does not compose like a value-returning function, a message holds a per-request reply conduit the agent posts to after processing, and the caller reads the reply with `Take(1).Last()`:

```csharp
internal sealed record Increment(int Amount, Conduit<int, int> Replies);

internal static class Counting {
    public static IO<int> Process(int state, Increment message) =>
        message.Replies.Post(state + message.Amount).Map(_ => state + message.Amount);
}

internal sealed class Counter(Conduit<Increment, Increment> inbox) {
    public IO<int> IncrementBy(int amount) {
        Conduit<int, int> replies = Conduit.make(Buffer<int>.Unbounded);
        return
            from _ in inbox.Post(new Increment(amount, replies))
            from reply in replies.Source.Take(1).Last()
            select reply;
    }
}
```

`Agent.Start(inbox, 0, Counting.Process)` starts the loop that serves `Counter`, the processor has the type `State -> Message -> IO<State>` and posts the reply inside the effect, and from the caller's side this is a thread-safe stateful function from message to reply composed as an `IO` without a lock. The agent stays private behind the domain facade, the facade exposes `IO<A>` whenever the result depends on agent processing, and the host runs it.

## [09]-[ENTITIES]

Event sourcing reconstructs a correct aggregate from concurrent events, but it does not protect a rule that depends on the state observed before an event is created: two concurrent debits can each validate against the same snapshot, both events are accepted, and replaying them yields a balance that violates the limit while the log stays internally consistent. Associate one process with each entity, and one server process hosts millions of them when it is the sole route for changes, while cross-process access needs actors. The responsibilities separate into an immutable snapshot, pure functions that validate a command and compute the event with the next state, and the process that owns the current state and serializes commands:

```csharp
internal sealed record Overdrawn() : Expected("debit exceeds the limit", 2001);
internal sealed record Snapshot(decimal Balance, decimal Limit);
internal sealed record Debited(decimal Amount);
internal sealed record Debit(decimal Amount, Conduit<Fin<Snapshot>, Fin<Snapshot>> Replies);

internal static class Rules {
    public static Fin<(Debited Event, Snapshot Next)> Debit(Snapshot state, decimal amount) =>
        state.Balance - amount < -state.Limit
            ? new Overdrawn()
            : (new Debited(amount), state with { Balance = state.Balance - amount });
}

internal sealed record EntityProcess(Conduit<Debit, Debit> Inbox, ForkIO<Snapshot> Running) {
    public static IO<EntityProcess> Start(Func<Debited, IO<Unit>> persist, Snapshot initial) {
        Conduit<Debit, Debit> inbox = Agent.Inbox<Debit>();
        return Agent.Start(inbox, initial, (state, command) => Handle(persist, state, command))
            .Map(running => new EntityProcess(inbox, running));
    }

    public IO<Snapshot> Debit(decimal amount) {
        Conduit<Fin<Snapshot>, Fin<Snapshot>> replies = Conduit.make(Buffer<Fin<Snapshot>>.Unbounded);
        return
            from _ in Inbox.Post(new Debit(amount, replies))
            from reply in replies.Source.Take(1).Last()
            from next in IO.lift(reply)
            select next;
    }

    private static IO<Snapshot> Handle(Func<Debited, IO<Unit>> persist, Snapshot state, Debit command) =>
        Rules.Debit(state, command.Amount).Match(
            Succ: transition =>
                from _ in persist(transition.Event)
                from __ in command.Replies.Post(transition.Next)
                select transition.Next,
            Fail: error =>
                from _ in command.Replies.Post(error)
                select state);
}
```

The command path evaluates the pure transition against the current state, retains the state and returns the typed rejection when invalid, persists and publishes the event when valid, adopts the computed next state only after persistence succeeds, and returns the result, where `IO.lift(reply)` raises the rejection on the caller's `IO` error channel. Persistence belongs inside the processing function because the next message must not observe the new in-memory state before its event is persisted, and the pure transition stays outside the concurrency mechanism.

Controllers need the one live process for an entity id, and an application-wide `AtomHashMap<Guid, EntityProcess>` owns that map. Registries that load missing state inside their update stall every lookup until the read completes, and the update reruns on conflict, the load happens in the caller's `IO` outside the registry: read `Find(id)` and return the existing process, otherwise load the state, start the process, and register it with `FindOrAdd(id, started)`, and its atomic check and add alone makes creation unique, the process that `FindOrAdd` did not return completes its inbox:

```csharp
internal sealed record UnknownEntity() : Expected("no entity has this id", 2002);

internal static class Registry {
    public static OptionT<IO, EntityProcess> Lookup(
        AtomHashMap<Guid, EntityProcess> processes,
        Func<Guid, OptionT<IO, Snapshot>> load,
        Func<Debited, IO<Unit>> persist,
        Guid id) =>
        processes.Find(id).Match(
            Some: OptionT.Some<IO, EntityProcess>,
            None: () =>
                from state in load(id)
                from started in EntityProcess.Start(persist, state)
                from resolved in IO.lift(() => processes.FindOrAdd(id, started))
                from _ in ReferenceEquals(resolved, started) ? IO.pure(unit) : started.Inbox.Complete()
                select resolved);
    public static IO<EntityProcess> Require(OptionT<IO, EntityProcess> lookup) =>
        lookup.Run().As().Bind(static found => IO.lift(found.ToFin(new UnknownEntity())));
}
```

The lookup is an `OptionT<IO, EntityProcess>` because the load and the registration share that stack, the query ends with `None` when storage has no such entity, and `Require` at the controller boundary maps `None` to a typed `Expected` on the `IO` error channel, a rejected command and a missing entity use the same result type. The design rules:
- Give an agent responsibility for owning and transitioning state, not every activity associated with it, and move work that uses no owned state and needs no ordering outside the inbox
- Make message types express intent (`Debit`, `Increment`) instead of sending data and inferring the operation
- Return immutable snapshots or derived results, never mutable agent state, even through a reply
- Plan the lifecycle, because keeping every process alive loads its state at most once while memory grows with the number and size of resident processes
- Keep orchestration unrelated to owned state in the caller's workflow, because an agent's purpose is serialized ownership and not object modeling
