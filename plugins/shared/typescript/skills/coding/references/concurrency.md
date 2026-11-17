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
const restarting = Effect.fn('restarting')(function* <K, E, R>(keys: Stream.Stream<K, E, R>, run: (key: K) => Effect.Effect<void, never, R>) {
    const running = yield* FiberMap.make<K>();
    yield* Stream.runForEach(keys, (key) => FiberMap.run(running, key, run(key)));
});
```

`changed` streams keys and `sync` runs one. Each key restarts its run and interrupts the stale one, and `Layer.effectDiscard(Effect.forkScoped(restarting(changed, sync)))` ends the key stream and every run with the layer scope:
- Unjoined runs hold `never` as their error and log their cause through `Effect.tapCause(Effect.logError)`, the only record of a defect they hit
- Followers a service forks return their fiber, its end effect racing host exit against `Fiber.joinAll` over them and failing with a follower's failure
- `FiberMap.join` fails with a member's first failure where a caller awaits the set
- `onlyIfMissing: true` on `FiberMap.run` or `FiberHandle.run` keeps the running fiber and discards the new run
- `FiberSet.makeRuntime<R>()` serves a synchronous host callback and `makeRuntimePromise<R>()` one a host awaits, each run ending with the set's scope
- Hook add and remove pairs acquire in the scope owning runs, and an awaited host call races `Effect.andThen(FiberSet.join(set), Effect.interrupt)`
- `FiberSet.join` fails with the first failure of a callback run, a `Deferred` the callbacks fail restating it
- `Effect` forks take `{ uninterruptible: true }` for a run that leaves state inconsistent when stopped midway
- `{ startImmediately: true }` on a fork runs the fiber to its first suspension before the fork returns
- Writes outliving a canceled caller fork through `Effect.forkIn(scope, { uninterruptible: true })` and complete a `Deferred` the caller awaits
- `Exit.hasInterrupts(yield* Fiber.await(fiber))` holds when an interrupt reached the fiber, one done before `Fiber.interrupt` exiting with success

## [02]-[STREAMS]

`Stream<A, E, R>` pulls values on demand, and a slow consumer slows its producer. Host listeners with synchronous per-event work (render and write) are a plain function the host calls. Producers a consumer pulls under backpressure, runs an effect per item over, or composes stream operators on enter through `Stream.callback`, its queue holding `bufferSize` items under a full-buffer `strategy` (`'suspend'`, `'dropping'`, `'sliding'`). `Queue.make`, `Stream.buffer`, and `Stream.share` take the same pair as `capacity` and `strategy`. Host callback registrations are acquired and released in the stream's scope:

```ts
const sizes = Stream.callback<DOMRectReadOnly>((queue) =>
    Effect.acquireRelease(
        Effect.sync(() => {
            const observer = new ResizeObserver((entries) => entries.forEach((entry) => Queue.offerUnsafe(queue, entry.contentRect)));
            observer.observe(viewport);
            return observer;
        }),
        (observer) => Effect.sync(() => observer.disconnect()),
    ),
);
```

`Stream.callback` forks the registration into the stream scope, and the release disconnects `viewport`'s observer when the stream ends, in place of an `Effect.ensuring(Effect.never, release)` holding a fiber open. `ResizeObserver` reports once on `observe` for an element with a nonzero size. `Queue.offerUnsafe` drops the new item when the buffer is full, or the oldest under `strategy: 'sliding'`. Effectful producers offer through `Queue.offer`, suspending while the buffer is full:

```ts
const entries = (root: string, bufferSize: number) => Stream.callback<Entry, WalkError>((queue) => walk(root, (entry) => Queue.offer(queue, entry)).pipe(Queue.into(queue)), { bufferSize });
```

`walk` is a host traversal calling its callback per entry under `root` and failing with `WalkError`. `Queue.into` ends the stream when `walk` succeeds and fails it with `walk`'s error:
- `Stream.callback` forks registration at once, a drain under `forkScoped({ startImmediately: true })` registering before the fork returns
- `Stream.share({ capacity, replay, idleTimeToLive })` multicasts a stream in hand, starting with its first consumer and ending after its last
- Sources enter through `Stream.fromIterable`, `fromAsyncIterable`, `fromEventListener`, `fromEffectSchedule` for polling, and `paginate` for cursors
- `Stream.peel(stream, Sink.head())` returns an `Option` of the first element beside a scoped stream of the rest, seeding state first
- `flow(Effect.fn(body), Stream.unwrap)` returns a stream releasing its acquired resources at its end
- `Stream.debounce(limit)` emits once a stream stays quiet for `limit`, a `switchMap` to `Effect.sleep` restating it

## [03]-[SHARED_STATE]

Shared state sits behind one handle the runtime keeps consistent, and what fibers share and how they coordinate selects the handle:

| [INDEX] | [HANDLE]                            | [HOLDS]                                                                           |
| :-----: | :---------------------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `TxRef`                             | Values one `Effect.tx` transaction reads and writes together                      |
|  [02]   | `SubscriptionRef`                   | Value with a `changes` stream emitting current value, then each update            |
|  [03]   | `Semaphore`, `PartitionedSemaphore` | Permits bounding one resource across call sites, or shared fairly across keys     |
|  [04]   | `Cache`                             | One lookup result per key, concurrent misses sharing one lookup                   |
|  [05]   | `RequestResolver`                   | Lookups concurrent fibers make, merged into one batch call                        |
|  [06]   | `RcRef`, `RcMap`                    | One resource, or one per key, acquired on first use, released after its last user |
|  [07]   | `Pool`                              | Interchangeable resources, each lent to one user at a time                        |
|  [08]   | `PubSub`                            | Messages one producer publishes to every subscriber, `replay` kept for late ones  |

```ts
const lend = (from: TxRef.TxRef<number>, to: TxRef.TxRef<number>, permits: number) =>
    TxRef.get(from).pipe(
        Effect.filterOrElse(Number.isGreaterThanOrEqualTo(permits), () => Effect.txRetry),
        Effect.andThen(Effect.all([TxRef.update(from, Number.subtract(permits)), TxRef.update(to, Number.sum(permits))])),
        Effect.tx,
    );
```

`from` and `to` hold free permits of rate-limit partitions. `Effect.txRetry` suspends `lend` until `from` changes, and both updates commit together once `from` holds `permits`. Keyed resources share one acquisition per key:

```ts
const clients = (idleTimeToLive: Duration.Input) => RcMap.make({ lookup: (peer: Peer) => Effect.acquireRelease(connect(peer), close), idleTimeToLive });
```

`Peer` is the key type, `connect` and `close` are host calls. `RcMap.get(map, peer)` inside `Effect.scoped` lends every concurrent caller one connection per peer, closed `idleTimeToLive` after its last caller. Keyed reads share one query per batch:

```ts
const nodeOf = Effect.map(SqlClient.SqlClient, (sql) => SqlResolver.request(SqlResolver.findById({ Id: NodeId, Result: Node, ResultId: (node) => node.id, execute: (ids) => sql`SELECT * FROM nodes WHERE id = any(${ids})` })));
```

`Node` is a row schema with an `id` field of brand `NodeId`. `nodeOf` yields `lookup`, and `Effect.forEach(ids, lookup, { concurrency: 'unbounded' })` sends one query for the ids its fibers request in the same tick, equal ids sharing one row and an id with no row failing with `NoSuchElementError`:
- Host batch APIs enter through `RequestResolver.make`, completing each entry through `Request.complete`, and a failed batch call fails every entry
- `RequestResolver.batchN` caps a batch at the host's limit, `setDelay` widens the window a batch collects over
- Batches merge callers across services, and `RequestResolver.makeGrouped` keys a batch by each value the host call reads (a tenant, a credential)
- Host state a view observes is a `SubscriptionRef` the host's events set, read through `Atom.subscriptionRef` or its `changes` stream
- `changes` through `Stream.mapAccum(HashMap.empty, (last, current) => [current, changed])` emits the `changed` array each step returns, `changesWith(eq)` dropping equal states
- `SubscriptionRef.modifyEffect` runs a host call and the transition it decides as one update
- `Semaphore.withPermits(semaphore, n)` bounds across operations, `concurrency` bounds one traversal
- `Cache.makeWith` takes `timeToLive` from each lookup's `Exit` and key, zero for a failure a later read can clear and infinity for a fixed fact
- `capacity: Number.POSITIVE_INFINITY` keeps every key with no eviction pass, a large finite number restating it
- `Effect.cachedWithTTL(read, (exit) => ttl)` caches one value under the same policy, `Effect.bindTo` giving it a service's shape

## [04]-[SCHEDULES]

Recurrence policies are `Schedule` values composed once at the composition root and read by `Effect.retry`, `Effect.repeat`, and `Stream.retry`:

```ts
const backoff = (base: Duration.Input, cap: Duration.Input, retries: number) => Schedule.min([Schedule.exponential(base), Schedule.spaced(cap)]).pipe(Schedule.jittered, Schedule.upTo({ times: retries }));
```

`Schedule.min` waits the shorter delay while either schedule recurs, capping the doubling delay at `cap`. The root reads `base`, `cap`, and `retries` through `Config`. `read` fails with `StoreError`, and `read.pipe(Effect.retry({ schedule: backoff(base, cap, retries), while: (error) => error.reason._tag === 'Unavailable' }))` retries the reasons a later attempt can clear and fails at once with every other reason:
- Initial runs precede the schedule, `times` counts retries after the first run
- `Schedule.while` stops on a step's `attempt`, `elapsed`, `input`, or `output`, the retried effect reading them from `Schedule.CurrentMetadata`
- `ExecutionPlan.make` lists layers with their own `attempts` and `schedule`, and `Effect.withExecutionPlan` reruns an effect across them
