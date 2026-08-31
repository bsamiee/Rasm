# Message-Passing Concurrency with Agents

## When shared state is unavoidable

Pure parallel computations can usually split work into independent inputs, compute partial results, and combine them without shared mutation. Multithreaded services have a different problem: many requests may need one application-wide sequence, cache, or representation of a unique real-world entity. Giving every thread its own copy would either break correctness or defeat the purpose of the shared resource.

The default remains: avoid shared mutable state. When one logical value must be shared, serialize the operations that can change it.

Common synchronization strategies have different scopes:

| Strategy                      | Useful scope                                                                  | Limitation                                                        |
| ----------------------------- | ----------------------------------------------------------------------------- | ----------------------------------------------------------------- |
| Lock                          | Arbitrary critical section                                                    | Blocks threads and admits deadlocks or overly broad serialization |
| Compare-and-swap              | One atomically replaced value                                                 | Too narrow for many multi-value invariants                        |
| Software transactional memory | Coordinated in-memory updates with isolation and atomic commit                | Requires a suitable implementation                                |
| Message passing               | State owned by a lightweight process and changed only while handling messages | Requires careful ownership, granularity, and lifecycle design     |

STM gives each transaction an isolated view, commits all of its changes or none, and retries against a fresh view when a concurrent transaction invalidates its work. Some implementations can also enforce consistency constraints. These properties make STM powerful for in-process coordination, but C# lacks an implementation comparable to those available in languages where STM is established.

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

The implementation may contain one private mutable variable, but that mutation is safe only because the inbox admits one handler at a time. Immutability still matters: exposing a mutable state object would let code outside the loop modify it concurrently and invalidate the model.

## A minimal C# implementation

The primitive public operations are to start an agent and to tell it a message; richer interactions are built from those operations. An `ActionBlock<T>` can supply both the inbox and sequential dispatch. The state type is hidden because it is an implementation detail; callers only need the message contract.

```csharp
using System.Threading.Tasks.Dataflow;

public interface IAgent<in TMessage>
{
    void Tell(TMessage message);
}

public sealed class StatefulAgent<TState, TMessage> : IAgent<TMessage>
{
    private TState state;
    private readonly ActionBlock<TMessage> inbox;

    public StatefulAgent(
        TState initialState,
        Func<TState, TMessage, TState> process)
    {
        state = initialState;
        inbox = new ActionBlock<TMessage>(message =>
        {
            var next = process(state, message);
            state = next;
        });
    }

    public void Tell(TMessage message) => inbox.Post(message);
}
```

This avoids a recursively implemented loop, which is not stack-safe in C#. By default, `ActionBlock` supplies an unbounded inbox and admits one handler at a time. Overloads can accept asynchronous processing functions or actions. A stateless agent is the same shape without the state variable; it serializes actions without retaining a value. Factory overloads can hide both implementations behind the same message-only interface, keeping the state type private.

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

Each per-pair agent can hold an `Option` of the cached rate: initially `None`, then `Some(rate)` after its first lookup. The processing function decides whether a remote lookup is needed. Expiry and error handling remain explicit design concerns. A single response-sending agent is acceptable only under the example's assumption that sending is fire-and-forget and has minimal latency.

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

Agent messaging is command-oriented and often effectful. A fire-and-forget `Tell` produces no value to feed into another function, and an agent combines state with at least part of the behavior that changes it. Message-passing concurrency therefore complements functional composition rather than behaving like another ordinary functional pipeline.

There are two coherent integration styles:
- embrace a unidirectional, event-driven flow in which agents communicate through messages;
- keep agents as private concurrency primitives and expose conventional value-returning APIs.

In either style, retain pure functions for domain decisions and use the agent only to order transitions and consistency-critical effects.

## Return replies without leaking the concurrency primitive

Fire-and-forget `Tell` supports unidirectional flows but does not compose like a value-returning function. A message can carry a `TaskCompletionSource<TReply>` that the agent completes after processing. The wiring can be hidden behind this interface:

```csharp
public interface IAgent<in TMessage, TReply>
{
    Task<TReply> Tell(TMessage message);
}
```

Its processor has one of these shapes:

```text
State -> Message -> (State, Reply)
State -> Message -> Task<(State, Reply)>
```

From the caller's perspective, this is a thread-safe, stateful, asynchronous function from message to reply. Awaiting a queued reply releases the caller's thread rather than blocking it on a lock.

The agent can remain private behind a conventional domain API:

```csharp
public sealed class Counter
{
    private readonly IAgent<int, int> agent;

    public Counter(IAgent<int, int> agent) => this.agent = agent;

    public Task<int> IncrementBy(int amount) => agent.Tell(amount);
}
```

As a rule, expose `Task<T>` whenever the result depends on agent processing. A synchronous facade using `.Result` is an exception suitable only for an operation expected to be extremely fast even under contention, such as incrementing an in-memory counter.

## Coordinating event-sourced domain entities

Event sourcing can reconstruct a correct aggregate state from concurrent events, but it does not by itself protect business rules that depend on the state observed before creating an event. Two concurrent debits may each return a state computed from the same snapshot while replaying both persisted events later still yields the correct balance. The problem appears when accepting both events is itself forbidden.

Suppose an account has a balance of 1,000 and an overdraft limit of 500. Two concurrent debits of 800 can each validate against the same initial snapshot. Both events would then be accepted, producing an overdraft of 600. The event log is internally consistent, but the business invariant has been violated.

Associate one lightweight process with each account and separate responsibilities. Thousands or even millions of these processes are feasible, provided one server process is the sole route for account changes; cross-process access requires actors instead.

- `AccountState`: immutable snapshot of the account;
- `Account`: pure functions that validate commands and compute an event plus next state;
- `AccountProcess`: the agent that owns the current state and serializes commands.

The command path is:

```text
command
  -> evaluate pure transition against current state
  -> if invalid: retain current state and return validation errors
  -> if valid: persist and publish the event
  -> only after persistence succeeds: adopt the computed next state
  -> return the command result
```

Persistence belongs inside this agent's processing function because the next message must not observe the new in-memory state before the corresponding event has been persisted successfully. Otherwise memory can disagree with replayable history. The pure transition logic still remains outside the concurrency mechanism and can be understood independently.

## A registry with correct granularity

Controllers need the one live process associated with an entity ID. An application-wide registry agent can own an immutable dictionary from ID to process, ensuring that two processes are never registered for the same entity.

A naive registry loads missing state from storage inside its processing function. That stalls every lookup, including unrelated IDs, while one slow read completes. The corrected workflow is:
1. Send `Lookup(id)` to the registry.
2. If present, return the existing process.
3. If absent, load the state on the caller's asynchronous flow, outside the agent.
4. Send `Register(id, loadedState)` to the registry.
5. During registration, check the dictionary again; another request may have registered the ID while the load was running.
6. Return the existing process if found, otherwise create, store, and return one process.

The double check is essential. Moving the load outside the agent restores concurrency, but it also allows multiple callers to observe a miss and load concurrently. Only the final registry-controlled check makes creation unique.

The public lookup stays in `Task<Option<T>>`: both loading and agent replies share that stack, and the fallback runs if the task faults or the option is empty. At the controller boundary, an adapter can translate the lookup to `Task<Validation<T>>` so command validation and a missing account use one composable result shape. Persistence and publication remain inside `AccountProcess`; the returned event/state tuple is feedback, not a second persistence path.

## Design rules and failure modes

- Give an agent responsibility for owning and transitioning state, not every activity associated with that state.
- Keep expensive work outside the inbox unless ordering that exact effect is required for correctness. Database loading for a registry can run outside; persistence of an accepted account transition cannot.
- Make message types express intent, such as `Lookup` and `Register`, instead of sending ambiguous data and inferring the operation.
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
