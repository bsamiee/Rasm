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
- Runs no caller joins hold `never` as their error and log their cause through `Effect.tapCause(Effect.logError)`, an unjoined fiber's defect leaving no other record
- `FiberMap.join` fails with a member's first failure where a caller awaits the set
- `onlyIfMissing: true` on `FiberMap.run` or `FiberHandle.run` keeps the running fiber and discards the new run
- `FiberSet.makeRuntime<R>()` returns a run function a synchronous host callback calls, every fiber it starts ending with the set's scope
- `Effect` forks take `{ uninterruptible: true }` for a run that leaves state inconsistent when stopped midway, and `{ startImmediately: true }` runs the fiber to its first suspension before the fork returns
- Writes outliving a canceled caller fork `Deferred.complete(done, write)` through `Effect.forkIn(scope, { uninterruptible: true })`, and the caller awaits `Effect.raceFirst(Deferred.await(done), Deferred.await(earlyResult))` under `Effect.interruptible` with `Effect.onInterrupt` recording the unfinished write
- `Exit.hasInterrupts(yield* Fiber.await(fiber))` holds when an interrupt reached the fiber, and a fiber that finished before `Fiber.interrupt` exits with success

## [02]-[STREAMS]

`Stream<A, E, R>` pulls values on demand, and a slow consumer slows its producer. Producers enter through `Stream.callback`, its queue holding `bufferSize` items under a full-buffer `strategy` (`'suspend'`, `'dropping'`, `'sliding'`). `Queue.make`, `Stream.buffer`, and `Stream.share` take the same pair as `capacity` and `strategy`. Host callback registrations are acquired and released in the stream's scope:

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
- `Stream.share({ capacity, replay, idleTimeToLive })` multicasts a stream in hand, starting with its first consumer and ending after its last
- `Stream.filterMap` with `Result.fromOption` keeps the elements a lookup finds
- Sources a constructor states enter through it: `Stream.fromIterable`, `fromAsyncIterable`, `fromEventListener`, `fromEffectSchedule` for polling, `paginate` for cursors
- `Stream.peel(stream, Sink.head())` returns an `Option` of the first element beside a scoped stream of the rest, seeding state first
- `flow(Effect.fn(body), Stream.unwrap)` returns a stream releasing its acquired resources at its end

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
- Batches merge callers with different services, and `RequestResolver.makeGrouped` keys a batch by each value the host call reads (a tenant, a credential)
- Host state a view observes is a `SubscriptionRef` the host's events set, read through `Atom.subscriptionRef` or its `changes` stream
- `changes` piped through `Stream.zipWithPrevious` diffs successive states, and `Stream.changesWith(eq)` drops states equal on a projection
- `SubscriptionRef.modifyEffect` runs a host call and the transition it decides as one update
- `Semaphore.withPermits(semaphore, n)` bounds across operations, `concurrency` bounds one traversal
- `Cache.makeWith` takes `timeToLive` as a function of each lookup's `Exit` and key

## [04]-[SCHEDULES]

Recurrence policies are `Schedule` values composed once at the composition root and read by `Effect.retry`, `Effect.repeat`, and `Stream.retry`:

```ts
const backoff = (base: Duration.Input, cap: Duration.Input, retries: number) => Schedule.min([Schedule.exponential(base), Schedule.spaced(cap)]).pipe(Schedule.jittered, Schedule.upTo({ times: retries }));
```

`Schedule.min` waits the shorter delay while either schedule recurs, capping the doubling delay at `cap`. The root reads `base`, `cap`, and `retries` through `Config`. `read` fails with `StoreError`, and `read.pipe(Effect.retry({ schedule: backoff(base, cap, retries), while: (error) => error.reason._tag === 'Unavailable' }))` retries the reasons a later attempt can clear and fails at once with every other reason:
- Initial runs precede the schedule, `times` counts retries after the first run
- `Schedule.while` stops on a step's `attempt`, `elapsed`, `input`, or `output`, and the retried effect reads the same fields from `Schedule.CurrentMetadata`
- `ExecutionPlan.make` lists layers with their own `attempts` and `schedule`, and `Effect.withExecutionPlan` reruns an effect across them
