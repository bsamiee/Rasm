# [CONCURRENCY]

## [01]-[FIBERS]

Work ending inside its starting step stays in `Effect.all`, `Effect.forEach` with `concurrency`, a race, or a `Stream`. Work outliving its starting step forks, and the owner of its lifetime selects the fork:

| [INDEX] | [FORK]                            | [LIFETIME]                                          |
| :-----: | :-------------------------------- | :-------------------------------------------------- |
|  [01]   | `Effect.forkChild`                | Ends with forking fiber                             |
|  [02]   | `Effect.forkScoped`               | Ends when enclosing `Scope` closes                  |
|  [03]   | `Effect.forkIn(scope)`            | Ends when another scope closes                      |
|  [04]   | `FiberSet.run(set, effect)`       | Jobs started over time, each ending with set scope  |
|  [05]   | `FiberMap.run(map, key, effect)`  | One fiber per key, new run interrupts key's fiber   |
|  [06]   | `FiberHandle.run(handle, effect)` | One fiber, new run interrupts running one           |
|  [07]   | `Effect.forkDetach`               | Runs until it completes or its handle interrupts it |

```ts
const answer = Effect.fnUntraced(function* <A, E>(calls: FiberSet.FiberSet<A, E>, call: Effect.Effect<A, E>, early: Effect.Effect<A>) {
    const fiber = yield* FiberSet.run(calls, call);
    return yield* Fiber.join(fiber).pipe(Effect.raceFirst(early), Effect.interruptible, Effect.onInterrupt(() => Fiber.interrupt(fiber)));
}, Effect.uninterruptible);
```

`calls` holds one MCP handle's running tool calls, and `early` yields a result once a target application asks a question or exits. Each call outlives its request in the handle's set, so an early result leaves it running for a later result to report. `Effect.uninterruptible` leaves the join as one interruptible region, and a client cancel interrupts the call through `onInterrupt`:
- Fibers are their own outcome (`Fiber.join`, `Fiber.await` for its `Exit`, `pollUnsafe()` in a transaction), a `Deferred` beside a fork restating it
- `Exit.hasInterrupts(yield* Fiber.await(fiber))` holds when an interrupt reached the fiber, one done before `Fiber.interrupt` exiting with success
- Unjoined runs hold `never` as their error and log their cause through `Effect.tapCause(Effect.logError)`, the only record of a defect they hit
- `tapCause` runs through `catchCause`, so an interrupt of the run itself logs nothing and one read from joined work logs
- `onlyIfMissing: true` on `FiberMap.run` or `FiberHandle.run` keeps the running fiber and drops the new run, a relisting keeping one follower per key
- `FiberSet.makeRuntime<R>()` serves a synchronous host callback and `makeRuntimePromise<R>()` one a host awaits, each run ending with the set's scope
- Awaited host calls race `Effect.andThen(FiberSet.join(set), Effect.interrupt)`, failing with a callback run's first failure in place of a `Deferred`
- Forks take `{ uninterruptible: true }` for a run leaving state inconsistent when stopped midway, `forkIn(scope, ...)` a write outliving its caller
- `{ startImmediately: true }` runs a fork to its first suspension before the fork returns, so a pump feeding queues starts before any reader pulls
- Hosts stopping a worker after a long synchronous stretch take a `Scheduler.MixedScheduler` subclass yielding by clock time in `shouldYield`

## [02]-[STREAMS]

`Stream<A, E, R>` pulls values on demand, and a slow consumer slows its producer. Host listeners with synchronous per-event work (render and write) stay a plain function the host calls, registered through `Effect.acquireRelease` in the reading scope, as a process-global source (`diagnostics_channel`) keeps a handler a rebuilt layer dropped. Each source enters through the constructor its shape selects:

| [INDEX] | [SOURCE]                     | [CONSTRUCTOR]                                                        |
| :-----: | :--------------------------- | :------------------------------------------------------------------- |
|  [01]   | `EventTarget` events         | `Stream.fromEventListener(target, type)`                             |
|  [02]   | Other host listener pairs    | `Stream.callback` acquiring and releasing the pair                   |
|  [03]   | Iterables, async iterables   | `Stream.fromIterable`, `Stream.fromAsyncIterable(iterable, onError)` |
|  [04]   | Read repeated on a schedule  | `Stream.fromEffectSchedule(read, schedule)`                          |
|  [05]   | State one step pages through | `Stream.paginate(seed, step)`                                        |
|  [06]   | Cell other fibers set        | `SubscriptionRef.changes`, `TxSubscriptionRef.changesStream`         |

```ts
const theme = Stream.callback<unknown>((queue) =>
    Effect.acquireRelease(
        Effect.sync(() => {
            const listener = (current: unknown): void => {
                Queue.offerUnsafe(queue, current);
            };
            host.theme.onUpdated.addListener(listener);
            listener(host.theme.getCurrent());
            return listener;
        }),
        (listener) => Effect.sync(() => host.theme.onUpdated.removeListener(listener)),
    ),
);
```

`host.theme` is the UXP document theme, a listener pair outside `EventTarget` reporting changes alone, so the stream seeds its current value and the release removes the listener when the stream ends:
- `Stream.callback` buffers `bufferSize` items under a `strategy` (`'suspend'`, `'dropping'`, `'sliding'`)
- `Queue.make`, `Stream.buffer`, and `Stream.share` take the buffer size as `capacity` beside the same `strategy`
- `Queue.offerUnsafe` drops a new item on a full buffer, or the oldest under `'sliding'`, and effectful producers suspend in `Queue.offer` instead
- `Queue.into(queue)` around an effectful producer ends the stream when the producer succeeds and fails the stream with its error
- Process-lifetime listeners run `Stream.runForEach` over `Stream.fromEventListener` under `Effect.forkScoped` and `Layer.effectDiscard`
- `Stream.paginate` steps return `[page, Option<next>]`: an empty page continues without emitting, and `None` ends after its page with no stack depth
- Polled reads emit transitions through `Effect.result(read)` and `changesWith(Result.makeEquivalence(eq, Function.constTrue))`, outages one state
- `Stream.peel(stream, Sink.head())` returns an `Option` of the first element beside a scoped stream of the rest, seeding a cell the rest then feeds
- Subscriptions over a changing set stay one stream seeded through `peel`, as `Stream.switchMap` restarts them and loses events between subscriptions
- `Stream.switchMap` stands where a new value voids the running stream (a peer coming back reconnects)
- `Stream.share(stream, { capacity, replay, idleTimeToLive })` gives readers one upstream subscription, opened by the first and closed after the last
- `replay: 1` hands each new reader the latest value, so a one-off read runs `Stream.runHead` over the shared stream, never a subscription of its own
- `Stream.mapAccum` and `mapAccumEffect` carry state across elements and emit the array each step returns, `onHalt` emitting from the final state
- `Stream.debounce(SubscriptionRef.changes(progress), quiet)` emits after `quiet` without progress, a beat raced against the work restating it
- `Stream.zipLatestAll` emits each latest tuple of its sources, each behind `Stream.concat(Stream.succeed(empty), source)` so none holds back
- `Stream.mapEffect(f, { concurrency, unordered: true })` emits results in completion order
- `Stream.toReadableStream(stream).values()` meets a host's `AsyncIterable` contract, an `async` generator restating it
- `Stream.filter` before `mapEffect` runs effects on kept items alone, `asSome` or `succeedNone` effects before `filter(Option.isSome)` restating it
- Per-value streams merge in `Stream.make(...values).pipe(Stream.flatMap(f, { concurrency }))`, `Stream.mergeAll` over a mapped array restating it
- `flow(Effect.fn(body), Stream.unwrap)` returns a stream releasing its acquired resources at its end
- `Stream.unwrap(<Service>.useSync((service) => service.<member>(...)))` opens a service stream as one pipe, `flatMap` over the service restating it
- `Stream.runCollect` pipes end in `Effect.zip(other, { concurrent: true })` beside an independent `other`, `Effect.all` around the pipe restating it

## [03]-[SHARED_STATE]

Shared state sits behind one handle the runtime keeps consistent, and what fibers share and how they coordinate selects the handle:

| [INDEX] | [HANDLE]                            | [HOLDS]                                                                           |
| :-----: | :---------------------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `TxRef`                             | Values one `Effect.tx` transaction reads and writes together                      |
|  [02]   | `TxSubscriptionRef`                 | Transactional value with a stream of committed changes                            |
|  [03]   | `SubscriptionRef`                   | Value with a `changes` stream emitting current value, then each update            |
|  [04]   | `Semaphore`, `PartitionedSemaphore` | Permits bounding one resource across call sites, or shared fairly across keys     |
|  [05]   | `Cache`, `Effect.cachedWithTTL`     | One lookup result per key, or one value, concurrent misses joining one lookup     |
|  [06]   | `RequestResolver`                   | Lookups concurrent fibers make, merged into one batch call                        |
|  [07]   | `RcRef`, `RcMap`                    | One resource, or one per key, acquired on first use, released after its last user |
|  [08]   | `Pool`                              | Interchangeable resources, each lent to `concurrency` users at once               |
|  [09]   | `PubSub`                            | Messages one producer publishes to every subscriber, `replay` kept for late ones  |

```ts
const follow = (slot: TxSubscriptionRef.TxSubscriptionRef<Slot>) =>
    TxSubscriptionRef.changesStream(slot).pipe(
        Stream.filter(({ answering }) => answering === 0),
        Stream.mapAccum(Function.constant(0), (sent, { entries }) => [Math.max(sent, ...Array.map(entries, Struct.get('sequence'))), Array.filter(entries, ({ sequence }) => sequence > sent)] as const),
    );
```

`Slot` is one MCP handle's state, `entries` its numbered events and `answering` its count of calls in progress. `changesStream` subscribes and reads the current value in one transaction, so no update falls between them, and `follow` streams each event once while no call answers:
- `TxRef.modify` and `update` run in their own `Effect.tx`, rerunning a pure function on a conflicting commit, and an outer `Effect.tx` joins them
- `Effect.txRetry` inside `Effect.tx` suspends the transaction until a value it read changes
- Registries are one `TxRef` of a record of cells, one `Effect.tx` numbering an event and appending it to each live cell, never a `Semaphore`
- Counts a scope holds are `Effect.acquireRelease` of an increment and its decrement on the cell, a reader gating on zero
- Host state a view observes is a `SubscriptionRef` the host's events set, read through `Atom.subscriptionRef` or its `changes` stream
- `SubscriptionRef.modifyEffect` runs a host call and the transition it decides as one update

Keyed resources share one acquisition per key:

```ts
const handles = RcMap.make({
    lookup: (id: string) => Effect.tap(Effect.all({ slot: TxSubscriptionRef.make(FRESH), calls: FiberSet.make() }), ({ slot }) => Effect.acquireRelease(TxRef.update(live, Record.set(id, slot)), () => TxRef.update(live, Record.remove(id)))),
    idleTimeToLive: retention,
});
```

`live` is the registry `TxRef`, `FRESH` an empty `Slot`, and `retention` a setting. Each handle owns its slot and its calls' `FiberSet`, registers while held, and releases both `retention` after its last call, in place of a sleep-then-filter fiber per entry:
- `RcMap.get` inside `Effect.scoped` creates an entry, and `RcMap.getOption` reads a held one without its lookup, an unknown or expired key failing
- Entries ending on a host event fork `Effect.andThen(<event>, RcMap.invalidate(map, key))` into the owning scope from their lookup
- `Pool.make({ acquire, size, concurrency })` lends each item to `concurrency` users at once (default 1), infinite with `size: 1` sharing one session
- `Pool.invalidate(pool, item)` drops an item matched by identity, and the next `Pool.get` acquires a fresh one, reconnecting a lost session lazily
- `Cache.makeWith` takes `timeToLive` from each lookup's `Exit` and key, zero for a failure a later read can clear and infinity for a fixed fact
- `capacity: Number.POSITIVE_INFINITY` skips eviction, and `capacity: 1` keyed by a file's path, mtime, and size keeps one decode per file change
- `requireServicesAt: 'lookup'` moves a lookup's services from construction to `Cache.get`, building the cache with no service read
- `Cache.getSuccess` reads a resolved success with no lookup, and a forked follower ends its entry by `Effect.ensuring(Cache.invalidate(cache, key))`
- `Effect.cachedWithTTL(effect, (exit) => ttl)` caches one value under that policy, concurrent first callers joining one run that ends when all leave
- `ScopedCache.makeWith({ lookup, capacity, timeToLive })` keeps a scoped resource per key, closing it on invalidation, eviction, or cache close
- `Semaphore.withPermits(semaphore, n)` bounds across operations, and `concurrency` bounds one traversal
- Host batch APIs enter through `RequestResolver.make`, each entry completed through `Request.complete`, a failed batch call failing every entry
- `RequestResolver.batchN` caps a batch at the host's limit, `setDelay` widens its window, and `makeGrouped` keys a batch by a host call value

## [04]-[SCHEDULES]

Recurrence policies are `Schedule` values read by `Effect.retry`, `Effect.repeat`, `Stream.retry`, and `Stream.fromEffectSchedule`. Fixed policies compose at the composition root from `Config` values, and a policy paced by its effect's output builds beside the effect:

```ts
const watch = (check: Effect.Effect<Duration.Duration>) =>
    Effect.repeat(check, {
        while: Duration.isFinite,
        schedule: Schedule.forever.pipe(Schedule.addDelay(({ input }) => Effect.succeed(input))),
    });
```

`check` applies policy rules to a waiting task and returns the shortest recheck any rule asked for, `Duration.infinity` where none did. Each repeat waits the delay the last check returned and the loop ends on an infinite one, a fixed interval and repeat count in the library restating it:
- `Schedule.min([Schedule.exponential(base), Schedule.spaced(cap)])` caps a doubling delay at `cap`, `jittered` spreading it
- `schedule` in `Effect.repeat` options takes the effect's output type as its `input`, a `Schedule.setInputType` restating it
- Initial runs precede the schedule, and `upTo({ times })` or `times` counts runs after the first
- `Effect.retry({ schedule, while })` retries the reasons a later attempt can clear and fails at once with every other reason
- `Effect.repeat(probe, { until, schedule })` polls until the probe's output holds (a lock query returning a row), the schedule pacing each miss
- `Schedule.while` stops on a step's `attempt`, `elapsed`, `input`, or `output`, the effect reading them from `Schedule.CurrentMetadata`
- `ExecutionPlan.make` lists layers with their own `attempts` and `schedule`, and `Effect.withExecutionPlan` reruns an effect across them
