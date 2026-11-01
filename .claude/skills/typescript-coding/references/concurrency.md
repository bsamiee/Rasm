# [CONCURRENCY]

Covers concurrent work, from fibers and streams to queues, shared state, shared resources, and schedules.

## [01]-[FIBERS]

Each fiber runs one effect concurrently with the fiber that forked it, and a scope bounds its lifetime. `Effect.all`, `Effect.forEach` with `concurrency`, and `Stream` hold work that ends inside the step starting it, a fork stands where work outlives that step. The owner of the fiber's lifetime selects the fork:

| [INDEX] | [FORK]                            | [LIFETIME]                                                                |
| :-----: | :-------------------------------- | :------------------------------------------------------------------------ |
|  [01]   | `Effect.forkChild`                | Interrupted when the forking fiber ends                                   |
|  [02]   | `Effect.forkScoped`               | Interrupted when the enclosing `Scope` closes, a layer's background job   |
|  [03]   | `Effect.forkIn(scope)`            | Interrupted when a scope other than the current one closes                |
|  [04]   | `FiberSet.run(set, effect)`       | Jobs started over time, every one interrupted when the set's scope closes |
|  [05]   | `FiberMap.run(map, key, effect)`  | One fiber per key, a new run interrupts the key's running fiber           |
|  [06]   | `FiberHandle.run(handle, effect)` | One fiber at a time, a new run interrupts the running one                 |
|  [07]   | `Effect.forkDetach`               | Runs until it completes or its handle interrupts it                       |

```ts
const resync = Layer.effectDiscard(
    Effect.gen(function* () {
        const [accounts, running] = yield* Effect.all([Accounts, FiberMap.make<AccountId>()]);
        yield* Stream.runForEach(accounts.changed, (account) => FiberMap.run(running, account, accounts.sync(account))).pipe(Effect.forkScoped);
    }),
);
```

`Accounts` exposes a `changed` stream of account ids and a `sync` effect per account. Each change restarts its account's sync and interrupts the stale run, and closing the layer's scope interrupts every sync with no handle kept:
- `FiberSet.join` and `FiberMap.join` fail with a member's first failure, `awaitEmpty` waits until every member ends
- `FiberMap.run` and `FiberHandle.run` with `onlyIfMissing: true` keep the running fiber and drop the new run
- `FiberSet.makeRuntime<R>()` answers a run function a synchronous host callback calls, and every fiber it starts ends with the set's scope
- `Effect.uninterruptible` covers a step that leaves state inconsistent when stopped midway, the wait before it stays interruptible

Racing runs effects concurrently, takes the winner, and interrupts the rest. The winning outcome and the number of contenders select the race:

| [INDEX] | [WINNER]                         | [PAIR]             | [ITERABLE]            |
| :-----: | :------------------------------- | :----------------- | :-------------------- |
|  [01]   | First success, failures wait     | `Effect.race`      | `Effect.raceAll`      |
|  [02]   | First completion, success or not | `Effect.raceFirst` | `Effect.raceAllFirst` |

## [02]-[STREAMS]

`Stream<A, E, R>` describes values over time and pulls them on demand, so a slow consumer slows its producer. A host's callback registration enters through `Stream.callback`, whose function registers the callback in the stream's scope:

```ts
const ingest = Layer.effectDiscard(
    Effect.gen(function* () {
        const [source, readings] = yield* Effect.all([ReadingSource, Readings]);
        yield* Stream.callback<Reading>((queue) =>
            Effect.acquireRelease(Effect.sync(() => source.subscribe((reading) => Queue.offerUnsafe(queue, reading))), Effect.sync),
        ).pipe(
            Stream.groupedWithin(readings.batch, readings.window),
            Stream.mapEffect(readings.store, { concurrency: readings.writers }),
            Stream.runDrain,
            Effect.forkScoped,
        );
    }),
);
```

`source.subscribe` returns the host's unsubscribe function, which becomes the release, and `Readings` states its batch size, flush window, writer count, and `store`. The layer's scope ends the subscription:
- Batch sizes, windows, and concurrency bounds come from the owner of the limit, a store's batch limit or a pool's size
- `Stream.fromEventListener` adapts an `EventTarget`, `Stream.fromAsyncIterable` and `Stream.fromReadableStream` their host types
- `Stream.filterMap(Filter.fromPredicateOption(decode))` keeps and maps each element a function answering `Option` decodes
- `Stream.groupedWithin(size, duration)` emits a batch at `size` elements or `duration` after the batch's first element
- `Stream.mapEffect` with `concurrency` overlaps elements and keeps input order, `unordered: true` emits each result as it completes
- `Stream.flatMap` with `concurrency` merges inner streams, `Stream.switchMap` interrupts the previous inner stream on each element
- `Stream.debounce` emits the latest element after a pause, `Stream.throttle` bounds `units` per `duration` through a `cost` function
- `Stream.interruptWhen` interrupts the stream when an effect completes, `Stream.haltWhen` stops it after the current pull

## [03]-[QUEUES]

`Queue` hands each item to one taker and `PubSub` hands each item to every subscriber. Each bounds its buffer by one strategy, and `Stream.callback`, `Stream.buffer`, `Stream.broadcast`, and `Stream.share` take the same names as their `strategy` option:

| [INDEX] | [STRATEGY] | [WHEN_FULL]                   | [CONSTRUCTORS]                        | [USE_WHEN]                          |
| :-----: | :--------- | :---------------------------- | :------------------------------------ | :---------------------------------- |
|  [01]   | `suspend`  | Offer waits for room          | `Queue.bounded`, `PubSub.bounded`     | Producer is an effect that can wait |
|  [02]   | `dropping` | New item is discarded         | `Queue.dropping`, `PubSub.dropping`   | Earlier items outrank later ones    |
|  [03]   | `sliding`  | Oldest item is discarded      | `Queue.sliding`, `PubSub.sliding`     | Later item supersedes earlier ones  |
|  [04]   | Unbounded  | Buffer grows with the backlog | `Queue.unbounded`, `PubSub.unbounded` | Source bounds its own rate          |

- Synchronous host callbacks cannot wait, so `Stream.callback` keeps its unbounded buffer for a host with a bounded rate, `bufferSize` with `dropping` or `sliding` otherwise
- `Queue.make({ capacity, strategy })` and `PubSub.bounded({ capacity, replay })` take the strategy and the replayed count as data
- `Queue.end` completes every taker's stream and `Queue.fail` fails it, `Queue.offerUnsafe` and `Queue.endUnsafe` act inside a host callback
- Producers receive `Queue.Enqueue<A>` and consumers `Queue.Dequeue<A>`, each side holds its own operations
- `Stream.fromQueue` drains a queue, `Stream.fromPubSub` subscribes for the stream's lifetime, `PubSub.subscribe` scopes a subscription
- `Stream.broadcast` multicasts one source within its scope, `Stream.share` connects on the first consumer and ends after the last, or `idleTimeToLive` later

## [04]-[SHARED_STATE]

Shared state is a value fibers read and update through one handle the runtime keeps consistent. What fibers share and how they coordinate selects the handle:

| [INDEX] | [HANDLE]          | [HOLDS]                                                                      |
| :-----: | :---------------- | :--------------------------------------------------------------------------- |
|  [01]   | `Ref`             | Value a pure function updates in one atomic step, `Ref.modify` answers too   |
|  [02]   | `SynchronizedRef` | Value an effect updates, updates serialized                                  |
|  [03]   | `TxRef`           | Values one `Effect.tx` transaction reads and writes together                 |
|  [04]   | `SubscriptionRef` | Value whose `changes` stream emits the current value, then every update      |
|  [05]   | `Deferred`        | One value or failure completed once and awaited by many                      |
|  [06]   | `Latch`           | Gate fibers wait on until it opens                                           |
|  [07]   | `Semaphore`       | Permits bounding access across call sites, a job takes permits by its weight |
|  [08]   | `Cache`           | One lookup result per key, concurrent misses share one lookup                |
|  [09]   | `RcRef`           | One resource acquired on first use and released after its last user          |
|  [10]   | `RcMap`           | One such resource per key                                                    |
|  [11]   | `Pool`            | Interchangeable resources, each lent to one user at a time                   |

```ts
class Clients extends Context.Service<Clients>()('@app/Clients', {
    make: Effect.map(
        RcMap.make({ lookup: (account: AccountId) => Effect.acquireRelease(Client.connect(account), Client.close), idleTimeToLive: '1 minute' }),
        (clients) => ({
            query: Effect.fn('Clients.query')(
                (query: Query) => RcMap.get(clients, query.account).pipe(Effect.flatMap((client) => client.run(query.sql))),
                Effect.scoped,
            ),
        }),
    ),
}) {
    static readonly layer = Layer.effect(Clients, Clients.make);
}
```

`Client.connect` and `Client.close` are the host's calls, and `Query` holds an `account` and its `sql`. Concurrent queries for one account share one connection, the connection closes a minute after its last query, and the layer's scope closes every connection:
- `Effect.tx` commits every `TxRef` write in its body or none, `Effect.txRetry` suspends the transaction until a `TxRef` it read changes
- Host state a view observes is a `SubscriptionRef` the host's events set, read through `Atom.subscriptionRef` or its `changes` stream
- `Semaphore.withPermits(semaphore, n)` bounds one resource shared across operations, `concurrency` bounds one traversal
- `Cache` stores a failed lookup like a success, `Cache.makeWith` derives `timeToLive` from each lookup's `Exit`
- `RcMap.make` with `capacity` fails a new key with `Cause.ExceededCapacityError` when full and keeps every existing entry
- `Pool.make` holds `size` items, `Pool.makeWithTTL` grows from `min` to `max` under load and releases an idle item after `timeToLive`

## [05]-[SCHEDULES]

`Schedule<Out, In>` states a recurrence policy as a value: the delay before each recurrence and when recurrence stops. `Effect.retry` reruns a failure on it, `Effect.repeat` reruns a success, `Stream.retry` and `Stream.schedule` apply it to a stream, and `Stream.fromSchedule` emits its outputs:

| [INDEX] | [SCHEDULE]                   | [DELAY_BEFORE_NEXT_RUN]                                        |
| :-----: | :--------------------------- | :------------------------------------------------------------- |
|  [01]   | `Schedule.spaced(d)`         | `d` after each run ends                                        |
|  [02]   | `Schedule.fixed(d)`          | To the next `d` beat, a late run starts at once                |
|  [03]   | `Schedule.windowed(d)`       | To the next `d` window boundary, a late run waits for the next |
|  [04]   | `Schedule.exponential(b, f)` | `b`, multiplied by `f` each attempt, `f` defaults to 2         |
|  [05]   | `Schedule.fibonacci(b)`      | Previous delay plus the one before it, starting at `b`         |
|  [06]   | `Schedule.cron(expr, zone)`  | To the next instant `expr` matches, a malformed `expr` fails   |
|  [07]   | `Schedule.recurs(n)`         | None, recurrence stops after `n`                               |

Combinators derive every other policy from those constructors:
- `Schedule.upTo({ times, duration })` stops after a count or an elapsed time, `Schedule.during(d)` is the elapsed bound alone
- `Schedule.jittered` scales each delay by a random factor between 0.8 and 1.2, so clients failing together retry apart
- `Schedule.min([a, b])` recurs while either recurs and waits the shorter delay, over `spaced(cap)` it caps a growing delay
- `Schedule.max([a, b])` recurs while both recur and waits the longer delay, over `spaced(floor)` it sets a minimum delay
- `Schedule.concat(a, b)` runs `b` once `a` stops, `Schedule.while` stops on attempt, elapsed time, input, or output
- `Schedule.tap` observes each recurrence, so a log or metric sits beside the policy and the retried effect stays unchanged

```ts
const fetched = Feed.fetch.pipe(
    Effect.retry({
        schedule: Schedule.min([Schedule.exponential('100 millis'), Schedule.spaced('10 seconds')]).pipe(Schedule.jittered),
        times: 5,
        while: (error) => error.reason === 'unavailable',
    }),
);
```

Delays double from 100 milliseconds to a 10 second cap with jitter, five retries at most, and only the `unavailable` reason retries:
- Initial runs precede the schedule, `Schedule.recurs(n)` allows `n` retries after one
- Retries apply to typed failures, a cause holding a defect propagates at once
- Polling is `Effect.repeat(Schedule.spaced(d))`, a timed source is `Stream.tick(d)`, a calendar job is `Schedule.cron(expr, zone)`
