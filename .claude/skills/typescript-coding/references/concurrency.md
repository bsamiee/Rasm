# [CONCURRENCY]

Covers concurrent work and the state it shares.

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
- Runs no caller joins hold `never` as their error and recover their own failure
- `FiberMap.join` fails with a member's first failure where a caller awaits the set
- `onlyIfMissing: true` on `FiberMap.run` or `FiberHandle.run` keeps the running fiber and discards the new run
- `FiberSet.makeRuntime<R>()` returns a run function a synchronous host callback calls, every fiber it starts ending with the set's scope
- `Effect.forkChild`, `forkScoped`, `forkIn`, and `forkDetach` take `{ uninterruptible: true }` for a run that leaves state inconsistent when stopped midway, starting the fiber uninterruptible

## [02]-[STREAMS]

`Stream<A, E, R>` pulls values on demand, and a slow consumer slows its producer. Producers enter through `Stream.callback`, its queue holding `bufferSize` items under a full-buffer `strategy` (`'suspend'`, `'dropping'`, `'sliding'`). `Queue.make`, `Stream.buffer`, and `Stream.share` take the same pair as `capacity` and `strategy`. Host callback registrations are acquired and released in the stream's scope:

```ts
const subscribed = <A>(subscribe: (emit: (value: A) => void) => () => void) =>
    Stream.callback<A>((queue) =>
        Effect.acquireRelease(
            Effect.sync(() => subscribe((value) => Queue.offerUnsafe(queue, value))),
            Effect.sync,
        ),
    );
```

`subscribe` registers a host listener and returns an unsubscribe function the stream runs at its end. `Queue.offerUnsafe` drops the new item when the buffer is full, or the oldest under `strategy: 'sliding'`. Effectful producers offer through `Queue.offer`, suspending while the buffer is full:

```ts
const produced = <A, E, R>(produce: (emit: (value: A) => Effect.Effect<boolean>) => Effect.Effect<unknown, E, R>, bufferSize: number) => Stream.callback<A, E, R>((queue) => produce((value) => Queue.offer(queue, value)).pipe(Queue.into(queue)), { bufferSize });
```

`produce` is a host walk calling `emit` per item. `Queue.into` ends the stream when `produce` succeeds and fails it with `produce`'s error:
- `Stream.share({ capacity, replay, idleTimeToLive })` multicasts a stream in hand, starting with its first consumer and ending after its last
- `Stream.filterMap` with `Result.fromOption` keeps the elements a lookup finds

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

```ts
const transfer = (from: TxRef.TxRef<number>, to: TxRef.TxRef<number>, amount: number) =>
    TxRef.get(from).pipe(
        Effect.filterOrElse(Number.isGreaterThanOrEqualTo(amount), () => Effect.txRetry),
        Effect.andThen(Effect.all([TxRef.update(from, Number.subtract(amount)), TxRef.update(to, Number.sum(amount))])),
        Effect.tx,
    );
```

`Effect.txRetry` suspends `transfer` until `from` changes, and both updates commit together once `from` holds `amount`. Keyed resources share one acquisition per key:

```ts
const clients = (idleTimeToLive: Duration.Input = Duration.minutes(1)) => RcMap.make({ lookup: (peer: Peer) => Effect.acquireRelease(connect(peer), close), idleTimeToLive });
```

`Peer` is the key type, `connect` and `close` are host calls. `RcMap.get(map, peer)` inside `Effect.scoped` lends every concurrent caller one connection per peer, closed `idleTimeToLive` after its last caller. Keyed reads share one query per batch:

```ts
const lineOf = Effect.map(SqlClient.SqlClient, (sql) => SqlResolver.request(SqlResolver.findById({ Id: Sku, Result: Line, ResultId: (line) => line.sku, execute: (skus) => sql`SELECT * FROM lines WHERE sku = any(${skus})` })));
```

`lineOf` yields `lookup`, and `Effect.forEach(skus, lookup, { concurrency: 'unbounded' })` sends one query for the skus its fibers request in the same tick, equal skus sharing one row and a sku with no row failing with `NoSuchElementError`:
- Host batch APIs enter through `RequestResolver.make`, completing each entry through `Request.complete`, and a failed batch call fails every entry
- `RequestResolver.batchN` caps a batch at the host's limit, `setDelay` widens the window a batch collects over
- Batches merge callers whose services differ, and `RequestResolver.makeGrouped` keys a batch by each value the host call reads (a tenant, a credential)
- Host state a view observes is a `SubscriptionRef` the host's events set, read through `Atom.subscriptionRef` or its `changes` stream
- `Semaphore.withPermits(semaphore, n)` bounds a resource shared across operations, `concurrency` bounds one traversal
- `Cache.makeWith` takes `timeToLive` as a function of each lookup's `Exit` and key

## [04]-[SCHEDULES]

Recurrence policies are `Schedule` values composed once and read by `Effect.retry`, `Effect.repeat`, and `Stream.retry`:

```ts
const backoff = ({ base = Duration.seconds(1), cap = Duration.seconds(10), times = 5 } = {}) => Schedule.min([Schedule.exponential(base), Schedule.spaced(cap)]).pipe(Schedule.jittered, Schedule.upTo({ times }));
```

`Schedule.min` waits the shorter delay while either schedule recurs, capping the doubling delay at `cap`. `read` fails with `StoreError`, and `read.pipe(Effect.retry({ schedule: backoff({ times: 3 }), while: (error) => error.reason === 'unavailable' }))` retries the reasons a later attempt can clear and fails at once with every other reason:
- Initial runs precede the schedule, `times` counts retries after the first run
- `Schedule.while` stops on a step's `attempt`, `elapsed`, `input`, or `output`, and the retried effect reads the same fields from `Schedule.CurrentMetadata`
- `ExecutionPlan.make` lists layers with their own `attempts` and `schedule`, and `Effect.withExecutionPlan` reruns an effect across them
