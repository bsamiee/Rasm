# Message-Passing Concurrency with Agents

## When shared state is unavoidable

Pure parallel computations can usually split work into independent inputs, compute partial results, and combine them without shared mutation. Multithreaded services have a different problem: many requests may need one application-wide sequence, cache, or representation of a unique real-world entity. Giving every thread its own copy would either break correctness or defeat the purpose of the shared resource.

The default remains: avoid shared mutable state. When one logical value must be shared, serialize the operations that can change it.

Common synchronization strategies have different scopes:

| Strategy                      | Useful scope                                        | Limitation                                                        |
| ----------------------------- | --------------------------------------------------- | ----------------------------------------------------------------- |
| Lock                          | Arbitrary critical section                          | Blocks threads and admits deadlocks or overly broad serialization |
| Compare-and-swap              | One atomically replaced value                       | Too narrow for many multi-value invariants                        |
| Software transactional memory | Coordinated in-memory updates, atomic commit        | Transactions retry on conflict, so their bodies hold no effects   |
| Message passing               | State owned by a process, changed only in a handler | Requires careful ownership, granularity, and lifecycle design     |

STM gives each transaction an isolated view, commits all of its changes or none, and retries against a fresh view when a concurrent transaction invalidates its work. Some implementations can also enforce consistency constraints. `Ref<A>` under `atomic` supplies these properties in process, and a transaction body holds no effects because a conflict re-runs it.

## The agent model

An agent has three parts:
- an inbox that queues messages;
- state that only the agent owns;
- a processing loop that handles one message at a time.

For each message, the processing function may perform effects, send messages, create agents, and compute the state used for the next message. The state is conceptually the fold of all messages received so far:

```text
nextState = process(currentState, message)
```

The essential invariants are:
1. No caller can directly read or mutate the owned state.
2. Messages for one agent are processed sequentially.
3. State values passed through the loop are immutable snapshots.

The implementation keeps the state in the accumulator of a fold over the inbox, so no private mutable field exists. Immutability still matters: exposing a mutable state object would let code outside the loop modify it concurrently and invalidate the model.

## A minimal C# implementation

The primitive public operations are to start an agent and to post it a message; richer interactions are built from those operations. A `Conduit<M, M>` made with `Buffer<M>.Unbounded` supplies both the inbox and sequential dispatch. The state type appears only in the started fork's result; callers hold the inbox and the message contract.

```csharp
internal static class Agent {
    public static Conduit<M, M> Inbox<M>() => Conduit.make(Buffer<M>.Unbounded);
    public static IO<ForkIO<S>> Start<S, M>(Conduit<M, M> inbox, S initialState, Func<S, M, S> process) =>
        inbox.Reduce(initialState, (state, message) => Reduced.ContinueIO(process(state, message))).Fork();
    public static IO<ForkIO<S>> Start<S, M>(Conduit<M, M> inbox, S initialState, Func<S, M, IO<S>> process) =>
        inbox.Reduce(initialState, (state, message) => process(state, message).Map(Reduced.Continue)).Fork();
}
```

This avoids a recursively implemented loop, which is not stack-safe in C#. `Reduce` runs the fold inside the conduit and admits one handler at a time. `Reduced.ContinueIO(next)` keeps the loop running and `Reduced.DoneIO(next)` ends it from inside the reducer. `Fork()` returns an `IO<ForkIO<S>>`. Running it starts the loop, and `Await` yields the final state after `Complete()` closes the inbox. The second overload accepts an effectful processing function that returns `IO<S>`. A stateless agent is the same shape with `Unit` as its state; it serializes effects without retaining a value.

## Choose the ownership boundary for concurrency

Putting every request through one agent makes the whole service sequential. Instead, align each agent with the smallest independently mutable resource whose invariant must be protected.

For a rate cache:

```text
concurrent requests
        |
        v
coordinator: currency pair -> agent
        |
        +--> EUR/USD agent  -- serial for this pair
        +--> GBP/USD agent  -- concurrent with EUR/USD
        +--> CHF/USD agent  -- concurrent with both
```

The coordinator only owns the registry of per-key agents and delegates immediately. Requests for different keys can progress concurrently. Requests for the same key queue behind one owner, preventing duplicate updates or remote lookups for that key.

Each per-pair agent holds an `Option<decimal>` of the cached rate inside its accumulator state: initially `None`, then `Some(rate)` after its first lookup. The processing function decides whether a remote lookup is needed. Expiry and error handling remain explicit design concerns. A single response-sending agent is acceptable only under the example's assumption that sending is fire-and-forget and has minimal latency.

The rule is precise: serialize by shared state dependency, not by request type or application layer.

## Agents and actors

Both models use exclusive state ownership, inboxes, sequential message processing, and message-based cooperation. Their operational boundaries differ:

| Agents                                                              | Actors                                                                  |
| ------------------------------------------------------------------- | ----------------------------------------------------------------------- |
| Local to one application process                                    | May run in different processes or machines                              |
| Referenced as in-process instances                                  | Referenced by location-transparent identity                             |
| Minimal model has no supervisor hierarchy                           | Actor systems commonly supervise and recover failed actors              |
| Mutable state can leak if an immutable-state discipline is violated | Serialized messages prevent sharing object references across boundaries |
| Low setup and operational cost                                      | Richer distribution, persistence, routing, and lifecycle machinery      |

Use agents when all coordinated access passes through one process. Use an actor system when state ownership or coordination must span processes or machines. Actor implementations differ in terminology, persistence, transport, lifecycle, and delivery guarantees; those details must be learned for the chosen implementation. The additional machinery is not free, so distribution should be a requirement rather than an aesthetic choice.

## Relationship with functional design

Agent messaging is command-oriented and often effectful. A fire-and-forget `Post` returns `IO<Unit>` and feeds no value into another function, and an agent combines state with at least part of the behavior that changes it. Message-passing concurrency therefore complements functional composition rather than behaving like another ordinary functional pipeline.

There are two coherent integration styles:
- embrace a unidirectional, event-driven flow in which agents communicate through messages;
- keep agents as private concurrency primitives and expose conventional value-returning APIs.

In either style, retain pure functions for domain decisions and use the agent only to order transitions and consistency-critical effects.

## Return replies without leaking the concurrency primitive

Fire-and-forget `Post` supports unidirectional flows but does not compose like a value-returning function. A message carries a per-request reply `Conduit` that the agent posts the reply to after processing. The caller reads the reply with `replies.Source.Take(1).Last()`, and `Agent.Start(inbox, 0, Counting.Process)` starts the loop that serves `Counter`:

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

Its processor has this shape, with the reply posted inside the effect:

```text
State -> Message -> IO<State>
```

From the caller's perspective, this is a thread-safe, stateful function from message to reply. The reply arrives on the `IO` channel, so the caller composes it and takes no lock.

The agent remains private behind a conventional domain API, and `Counter` is that facade. As a rule, expose `IO<A>` whenever the result depends on agent processing. The host runs it with `RunSafe()`, and the domain never runs the effect.

## Coordinating event-sourced domain entities

Event sourcing can reconstruct a correct aggregate state from concurrent events, but it does not by itself protect business rules that depend on the state observed before creating an event. Two concurrent debits may each return a state computed from the same snapshot while replaying both persisted events later still yields the correct balance. The problem appears when accepting both events is itself forbidden.

Suppose an account has a balance of 1,000 and an overdraft limit of 500. Two concurrent debits of 800 can each validate against the same initial snapshot. Both events would then be accepted, producing an overdraft of 600. The event log is internally consistent, but the business invariant has been violated.

Associate one lightweight process with each account and separate responsibilities. Thousands or even millions of these processes are feasible, provided one server process is the sole route for account changes; cross-process access requires actors instead.

- `AccountState`: immutable snapshot of the account;
- `Account`: pure functions that validate commands and compute an event plus next state;
- `AccountProcess`: the agent that owns the current state and serializes commands.

```csharp
internal sealed record Overdrawn() : Expected("debit exceeds the overdraft limit", 1001);
internal sealed record AccountState(decimal Balance, decimal OverdraftLimit);
internal sealed record Debited(decimal Amount);
internal sealed record Debit(decimal Amount, Conduit<Fin<AccountState>, Fin<AccountState>> Replies);

internal static class Account {
    public static Fin<(Debited Event, AccountState Next)> Debit(AccountState state, decimal amount) =>
        state.Balance - amount < -state.OverdraftLimit
            ? new Overdrawn()
            : (new Debited(amount), state with { Balance = state.Balance - amount });
}

internal sealed record AccountProcess(Conduit<Debit, Debit> Inbox, ForkIO<AccountState> Running) {
    public static IO<AccountProcess> Start(Func<Debited, IO<Unit>> persist, AccountState initial) {
        Conduit<Debit, Debit> inbox = Agent.Inbox<Debit>();
        return Agent.Start(inbox, initial, (state, command) => Handle(persist, state, command))
            .Map(running => new AccountProcess(inbox, running));
    }

    public IO<AccountState> Debit(decimal amount) {
        Conduit<Fin<AccountState>, Fin<AccountState>> replies = Conduit.make(Buffer<Fin<AccountState>>.Unbounded);
        return
            from _ in Inbox.Post(new Debit(amount, replies))
            from reply in replies.Source.Take(1).Last()
            from next in IO.lift(reply)
            select next;
    }

    private static IO<AccountState> Handle(Func<Debited, IO<Unit>> persist, AccountState state, Debit command) =>
        Account.Debit(state, command.Amount).Match(
            Succ: transition =>
                from _ in persist(transition.Event)
                from __ in command.Replies.Post(transition.Next)
                select transition.Next,
            Fail: error =>
                from _ in command.Replies.Post(error)
                select state);
}
```

The command path is:

```text
command
  -> evaluate pure transition against current state
  -> if invalid: retain current state and return validation errors
  -> if valid: persist and publish the event
  -> only after persistence succeeds: adopt the computed next state
  -> return the command result
```

A rejected command replies with `Overdrawn`, a typed `Expected`, and `Debit` raises it on the `IO` error channel of the caller. Persistence belongs inside this agent's processing function because the next message must not observe the new in-memory state before the corresponding event has been persisted successfully. Otherwise memory can disagree with replayable history. The pure transition logic still remains outside the concurrency mechanism and can be understood independently.

## A registry with correct granularity

Controllers need the one live process associated with an entity ID. An application-wide `AtomHashMap<Guid, AccountProcess>` owns the map from ID to process, ensuring that two processes are never registered for the same entity.

A naive registry loads missing state from storage inside the registry update. That stalls every lookup, including unrelated IDs, while one slow read completes, and the update re-runs on conflict, so it must stay free of effects. The corrected workflow is:
1. Read `Find(id)` on the registry and return the existing process when present.
2. If absent, load the state and start the process on the caller's flow, outside the registry.
3. Register with `FindOrAdd(id, started)`. The call adds the started process only when the ID is still absent and returns the registered process.

The double check is the `FindOrAdd`. Moving the load outside the registry restores concurrency, but it also allows multiple callers to observe a miss and load concurrently. Only the final registry-controlled add makes creation unique, and the process whose add loses completes its inbox.

```csharp
internal sealed record UnknownAccount() : Expected("no account has this id", 1002);

internal static class Registry {
    public static OptionT<IO, AccountProcess> Lookup(
        AtomHashMap<Guid, AccountProcess> processes,
        Func<Guid, OptionT<IO, AccountState>> load,
        Func<Debited, IO<Unit>> persist,
        Guid id) =>
        processes.Find(id).Match(
            Some: OptionT.Some<IO, AccountProcess>,
            None: () =>
                from state in load(id)
                from started in AccountProcess.Start(persist, state)
                from resolved in IO.lift(() => processes.FindOrAdd(id, started))
                from _ in ReferenceEquals(resolved, started) ? IO.pure(unit) : started.Inbox.Complete()
                select resolved);
    public static IO<AccountProcess> Require(OptionT<IO, AccountProcess> lookup) =>
        lookup.Run().As().Bind(static found => IO.lift(found.ToFin(new UnknownAccount())));
}
```

The public lookup is an `OptionT<IO, AccountProcess>`: the load and the registration share that stack. The query ends with `None` when storage has no such account. At the controller boundary, `Require` runs the transformer and maps `None` to `UnknownAccount`, a typed `Expected` on the `IO` error channel. A rejected command and a missing account then use one result shape. Persistence and publication remain inside `AccountProcess`; the returned state is feedback, not a second persistence path.

## Design rules and failure modes

- Give an agent responsibility for owning and transitioning state, not every activity associated with that state.
- Keep expensive work outside the inbox unless ordering that exact effect is required for correctness. Database loading for a registry can run outside; persistence of an accepted account transition cannot.
- Make message types express intent, such as `Debit` and `Increment`, instead of sending ambiguous data and inferring the operation.
- Never expose mutable agent state, even through a reply. Return immutable snapshots or derived results.
- Preserve independent concurrency by partitioning agents per entity or key. One coarse agent turns unrelated work into a queue.
- Keep agent identity unique. A registry is part of the correctness boundary, not merely a performance cache.
- Plan lifecycle explicitly. Keeping every created process alive means its state is loaded at most once and subsequent access is fast, but memory grows with the number and size of resident processes.
- Do not confuse an agent with an object. Its purpose is serialized ownership; slow orchestration that does not touch owned state belongs in caller-level workflows.

## Selection boundary

- Avoid shared mutable state when the work can be decomposed into independent computations and their results combined.
- Use an agent when one application process must serialize access to owned state while allowing different owners to proceed independently.
- Use an actor system when that coordination must cross application or machine boundaries and its distribution, routing, supervision, persistence, and delivery guarantees are required.

Agents complement functional composition where shared mutable state cannot be avoided; they need not dictate the public architecture.
