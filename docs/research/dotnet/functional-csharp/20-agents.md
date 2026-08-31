# [AGENTS]

## [01]-[SHARED_STATE]

Parallel computations with independent inputs can compute partial results and combine them without shared mutation. Some multithreaded services need one application-wide sequence, cache, or representation of a unique real-world entity. Giving every thread its own copy either breaks correctness or defeats the purpose of the shared resource.

Avoid shared mutable state by default. When one logical value must be shared, serialize the operations that can change it.

Synchronization strategies have different scopes:

| [INDEX] | [STRATEGY]                    | [SCOPE]                                             | [LIMITATION]                                                    |
| :-----: | :---------------------------- | :-------------------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | Lock                          | Arbitrary critical section                          | Blocks threads and can deadlock or serialize unrelated work     |
|  [02]   | Compare-and-swap              | One atomically replaced value                       | Too narrow for many multi-value invariants                      |
|  [03]   | Software transactional memory | Coordinated in-memory updates, atomic commit        | Retries on conflict. Transaction bodies must contain no effects |
|  [04]   | Message passing               | State owned by a process, changed only in a handler | Requires careful ownership, granularity, and lifecycle design   |

STM gives each transaction an isolated view, commits all of its changes or none, and retries against a fresh view when a concurrent transaction invalidates its work. Some implementations can enforce consistency constraints. `Ref<A>` under `atomic` supplies these properties in process, and a transaction body holds no effects because a conflict re-runs it.

## [02]-[AGENT_MODEL]

An agent has three parts:
- an inbox that queues messages;
- state that only the agent owns;
- a processing loop that handles one message at a time.

For each message, the processing function can perform effects, send messages, create agents, and compute the state used for the next message. The state is the fold of all messages received so far:

```text
nextState = process(currentState, message)
```

The invariants are:
1. No caller can directly read or mutate the owned state.
2. Messages for one agent are processed sequentially.
3. State values passed through the loop are immutable snapshots.

The implementation keeps the state in the accumulator of a fold over the inbox and does not use a private mutable field. Immutability matters: exposing a mutable state object lets code outside the loop modify it concurrently and invalidates the model.

## [03]-[MINIMAL_IMPLEMENTATION]

The public operations start an agent and post a message. Other interactions compose these operations. A `Conduit<M, M>` made with `Buffer<M>.Unbounded` supplies the inbox and sequential dispatch. The state type appears only in the `ForkIO<S>` returned by `Start`; callers hold the inbox and the message contract.

```csharp
internal static class Agent {
    public static Conduit<M, M> Inbox<M>() => Conduit.make(Buffer<M>.Unbounded);
    public static IO<ForkIO<S>> Start<S, M>(Conduit<M, M> inbox, S initialState, Func<S, M, S> process) =>
        inbox.Reduce(initialState, (state, message) => Reduced.ContinueIO(process(state, message))).Fork();
    public static IO<ForkIO<S>> Start<S, M>(Conduit<M, M> inbox, S initialState, Func<S, M, IO<S>> process) =>
        inbox.Reduce(initialState, (state, message) => process(state, message).Map(Reduced.Continue)).Fork();
}
```

This avoids a recursively implemented loop, which is not stack-safe in C#. `Reduce` runs the fold inside the conduit and admits one handler at a time. `Reduced.ContinueIO(next)` keeps the loop running and `Reduced.DoneIO(next)` ends it from inside the reducer. `Fork()` returns an `IO<ForkIO<S>>`. Running it starts the loop, and `Await` yields the final state after `Complete()` closes the inbox. The second overload accepts an effectful processing function that returns `IO<S>`. A stateless agent uses `Unit` as its state and serializes effects without retaining a value.

## [04]-[STATE_OWNERSHIP]

Putting every request through one agent makes the whole service sequential. Align each agent with the smallest independently mutable resource whose invariant must be protected.

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

Each per-pair agent holds an `Option<decimal>` of the cached rate inside its accumulator state: initially `None`, then `Some(rate)` after its first lookup. The processing function decides whether a remote lookup is needed. Expiry and error handling remain explicit design concerns. One agent can send all replies only if sending is fire-and-forget and does not delay the queue.

Serialize by dependency on shared state, not by request type or application layer.

## [05]-[AGENTS_AND_ACTORS]

Both models use exclusive state ownership, inboxes, sequential message processing, and message-based cooperation. Their operational boundaries differ:

| [INDEX] | [AGENTS]                                                            | [ACTORS]                                                                |
| :-----: | :------------------------------------------------------------------ | :---------------------------------------------------------------------- |
|  [01]   | Local to one application process                                    | Can run in different processes or machines                              |
|  [02]   | Referenced as in-process instances                                  | Referenced by location-transparent identity                             |
|  [03]   | Minimal model has no supervisor hierarchy                           | Actor systems can supervise and recover failed actors                   |
|  [04]   | Mutable state can leak if an immutable-state discipline is violated | Serialized messages prevent sharing object references across boundaries |
|  [05]   | Requires only in-process setup and operation                        | Provides distribution, persistence, routing, and lifecycle support      |

Use agents when all coordinated access passes through one process. Use an actor system when state ownership or coordination must span processes or machines. Actor implementations differ in terminology, persistence, transport, lifecycle, and delivery guarantees; those details must be learned for the chosen implementation. Distribution adds operational cost. Use it only when coordination must cross process boundaries.

## [06]-[FUNCTIONAL_DESIGN]

Agent messaging is command-oriented and can be effectful. An agent combines state with the behavior that changes it. Message-passing concurrency complements functional composition; it is not a value-returning pipeline.

Two integration styles apply:
- use a unidirectional, event-driven flow in which agents communicate through messages;
- keep agents as private concurrency primitives and expose value-returning APIs.

In either style, retain pure functions for domain decisions and use the agent only to order transitions and effects whose order preserves consistency.

## [07]-[REPLIES]

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

Its processor posts the reply inside the effect and has this type:

```text
State -> Message -> IO<State>
```

From the caller's perspective, this is a thread-safe, stateful function from message to reply. The caller composes the `IO` result without a lock.

The agent stays private behind a domain API, and `Counter` provides that facade. Expose `IO<A>` whenever the result depends on agent processing. The host runs it with `RunSafe()`, and the domain never runs the effect.

## [08]-[ENTITY_COORDINATION]

Event sourcing can reconstruct a correct aggregate state from concurrent events, but it does not by itself protect business rules that depend on the state observed before creating an event. Two concurrent debits can each return a state computed from the same snapshot while replaying both persisted events later yields the correct balance. The problem appears when accepting both events is itself forbidden.

For example, an account has a balance of 1,000 and an overdraft limit of 500. Two concurrent debits of 800 can each validate against the same initial snapshot. Both events are accepted, and the overdraft becomes 600. The event log is internally consistent, but the business invariant has been violated.

Associate one lightweight process with each account and separate responsibilities. One server process can host thousands or millions of these processes if it is the sole route for account changes; cross-process access requires actors instead.

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

A rejected command replies with `Overdrawn`, a typed `Expected`, and `Debit` raises it on the `IO` error channel of the caller. Persistence belongs inside this agent's processing function because the next message must not observe the new in-memory state before the corresponding event has been persisted. Otherwise memory can disagree with persisted event history. The pure transition logic stays outside the concurrency mechanism and can be understood independently.

## [09]-[ENTITY_REGISTRY]

Controllers need the one live process associated with an entity ID. An application-wide `AtomHashMap<Guid, AccountProcess>` owns the map from ID to process, ensuring that two processes are never registered for the same entity.

A registry that loads missing state inside its update stalls every lookup, including unrelated IDs, until that storage read completes. Because the update re-runs on conflict, it must stay free of effects. Use this workflow:
1. Read `Find(id)` on the registry and return the existing process when present.
2. If absent, load the state and start the process in the caller's `IO` computation, outside the registry.
3. Register with `FindOrAdd(id, started)`. The call adds the started process only when the ID is still absent and returns the registered process.

`FindOrAdd` performs the atomic check and add. Moving the load outside the registry restores concurrency, but it also allows multiple callers to observe a miss and load concurrently. Only the `FindOrAdd` call makes creation unique, and the process not returned by `FindOrAdd` completes its inbox.

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

The public lookup is an `OptionT<IO, AccountProcess>`: the load and the registration share that stack. The query ends with `None` when storage has no such account. At the controller boundary, `Require` runs the transformer and maps `None` to `UnknownAccount`, a typed `Expected` on the `IO` error channel. A rejected command and a missing account use the same result type. The returned state reports the result. It does not persist the event again.

## [10]-[DESIGN_RULES]

- Give an agent responsibility for owning and transitioning state, not every activity associated with that state.
- Move work outside the inbox when it does not use owned state or require ordering.
- Make message types express intent, such as `Debit` and `Increment`, instead of sending ambiguous data and inferring the operation.
- Never expose mutable agent state, even through a reply. Return immutable snapshots or derived results.
- Plan lifecycle. Keeping every created process alive means its state is loaded at most once, but memory grows with the number and size of resident processes.
- Do not confuse an agent with an object. Its purpose is serialized ownership; orchestration unrelated to owned state belongs in the caller's workflow.
