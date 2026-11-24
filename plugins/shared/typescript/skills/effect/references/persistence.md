# [PERSISTENCE]

## [01]-[STORES]

State outliving one call selects its service by shape, and its backing by where processes share it:

| [INDEX] | [BACKING]   | [KEYVALUESTORE]                                  | [PERSISTENCE]                                |
| :-----: | :---------- | :----------------------------------------------- | :------------------------------------------- |
|  [01]   | Process     | `KeyValueStore.layerMemory`                      | `Persistence.layerMemory`                    |
|  [02]   | Folder      | `KeyValueStore.layerFileSystem(directory)`       | `Persistence.layerKvs` over it               |
|  [03]   | Web Storage | `KeyValueStore.layerStorage(() => localStorage)` | `Persistence.layerKvs` over it               |
|  [04]   | SQL table   | `KeyValueStore.layerSql`                         | `Persistence.layerSql`, `layerSqlMultiTable` |
|  [05]   | Redis       | None                                             | `Persistence.layerRedis`                     |

`KeyValueStore` holds string or byte values by key, and `Persistence` holds encoded `Exit` values by request key with a time to live per entry:
- `KeyValueStore.toSchemaStore(store, S)` reads and writes typed values through the JSON codec of `S`, a read returning `Option`
- `KeyValueStore.prefix(store, prefix)` scopes keys, and `layerFileSystem` writes one percent-encoded file per key, its `clear` deleting the folder
- `Persistence.layerSqlMultiTable` names a table `effect_persistence_<storeId>` unescaped, and arbitrary store ids take `layerSql`
- `UserDirectories` resolves `config`, `data`, `cache`, and `state` folders per platform for a store folder

## [02]-[CACHES]

Lookup results cached by key select their cache by where a result sits and what it owns:

| [INDEX] | [CACHE]          | [HOLDS]                                                                |
| :-----: | :--------------- | :--------------------------------------------------------------------- |
|  [01]   | `Cache`          | Results in process memory                                              |
|  [02]   | `ScopedCache`    | Results owning a resource                                              |
|  [03]   | `PersistedCache` | `Exit` values in a `Persistence` store, read across processes and runs |

Caches compare keys alone and never check an entry against the inputs that produced it. Keys hold every input their result reads (a content digest, a source version), and a changed input reads a new key. `Persistable.Class` declares such a key with the schemas encoding its `Exit`:

```ts
class Scan extends Persistable.Class<{ payload: { readonly file: string; readonly digest: string } }>()('Scan', { primaryKey: ({ file, digest }) => `${file}@${digest}`, success: Report, error: ScanError }) {}

const scans = (scan: (request: Scan) => Effect.Effect<Report, ScanError>, kept: Duration.Input) => PersistedCache.make(scan, { storeId: 'scans', timeToLive: (exit) => (Exit.isSuccess(exit) ? kept : Duration.zero) });
```

`Report` is a schema class and `ScanError` the package error. `cache.get(new Scan({ file, digest }))` reads memory, then the store, then runs `scan` and writes its `Exit`, and writes nothing for a failed scan:
- `timeToLive` of zero or less skips the store write
- `inMemoryTTL` (10 seconds) and `inMemoryCapacity` (1024) bound an in-memory `Cache` in front of the store
- Stored exits failing to decode under their request's schemas read as a miss and rerun the lookup
- `cache.invalidate(request)` deletes stored and in-memory entries
- `PersistedCache.make` requires `Persistence` and `Scope`, its in-memory cache ending with the scope
- `ScopedCache.makeWith({ lookup, capacity, timeToLive })` closes an entry's scope on eviction, invalidation, refresh, or the owning scope's close

## [03]-[QUEUES]

`PersistedQueue` hands work across fibers and processes sharing a store and delivers each element at least once. A consumer forks one take loop and hands callers the offer:

```ts
const outbox = Effect.fn('outbox')(function* (name: string) {
    const queue = yield* PersistedQueue.make({ name, schema: Event });
    yield* queue.take((event, { attempts }) => send(event, attempts)).pipe(Effect.ignoreCause({ log: 'Warn', message: `${name} delivery failed` }), Effect.forever, Effect.forkScoped);
    return (id: string, event: typeof Event.Type) => queue.offer(event, { id });
});
```

`Event` is the element schema and `send` delivers one element. `take` fails with a handler's failure after scheduling its retry, and `Effect.ignoreCause` keeps the loop taking:
- `take` marks an element done when the handler succeeds, and a failed one waits out `retrySchedule` until `maxAttempts` marks it failed
- Attempts count when an element is claimed, a crash mid-handler consumes one, and handlers are idempotent
- `maxAttempts` defaults to 10, `retrySchedule` to exponential delays from 1 second capped at 5 minutes
- Elements failing to decode mark failed at once and never reach `take`
- `offer(value, { id })` enqueues nothing for a repeated `id` until `layerCleanup` removes its element
- `deduplicate: 'untilExecuted'` frees an `id` once its element runs
- `delay` postpones first delivery by the store's clock, and an infinite delay stores nothing
- `PersistedQueue.layer` builds the factory over `layerStoreMemory`, `layerStoreSql()`, or `layerStoreRedis`
- `layerCleanup()` removes done elements after 30 days

## [04]-[LIMITS]

`RateLimiter` and `CircuitBreaker` guard calls by string key over a store, `layerStoreMemory` holding state in the process and `layerStoreRedis` sharing it:
- `limiter.consume({ key, limit, window, algorithm, onExceeded })` counts by `'fixed-window'` or `'token-bucket'` and returns a `ConsumeResult`
- `onExceeded: 'delay'` returns its wait in `ConsumeResult.delay`, and `'fail'` fails with a `RateLimitExceeded` reason
- `Effect.isSuccess` over `consume` with `'fail'` tests the limit
- `RateLimiter.makeWithRateLimiter` returns a function wrapping an effect to wait out its delay
- `CircuitBreaker.layer({ tripPolicy, openDuration })` opens after `CircuitBreaker.consecutive(n)` failures or a `rate({ threshold, size })`
- `openDuration` takes a `Schedule`
- `breaker.execute(effect, { key, classify })` fails with a `CircuitOpen` reason while open and admits one probe half-open
- `classify` maps each exit to `CircuitBreaker.success`, `ignored`, or `failure({ retryAfter })`, an ignored exit leaving the circuit unchanged

## [05]-[DURABLE_EXECUTION]

Work surviving a restart selects its family by what replays and where it runs:

| [INDEX] | [WORK]                                   | [FORM]                               |
| :-----: | :--------------------------------------- | :----------------------------------- |
|  [01]   | One job per element, no step replay      | `PersistedQueue`                     |
|  [02]   | Steps replayed from their stored results | `Workflow` with `Activity` steps     |
|  [03]   | State addressed by id across runners     | `Entity` of `effect/cluster`         |
|  [04]   | One effect on one runner of a cluster    | `Singleton.make`, `ClusterCron.make` |
|  [05]   | Local-first state synced across devices  | `EventLog` of `effect/eventlog`      |

Workflows record each activity's `Exit`, and a replay after a restart reads it in place of running the step again. Entities run their messages one at a time per id:

```ts
const Checkout = Workflow.make('Checkout', { payload: { order: Schema.String }, idempotencyKey: ({ order }) => order, success: Schema.String });

const CheckoutLive = Checkout.toLayer(({ order }) => Activity.make({ name: 'charge', success: Schema.String, execute: charge(order) }).pipe(Workflow.withCompensation(refund)));

const Counter = Entity.make('Counter', [Rpc.make('Increment', { payload: { amount: Schema.Int }, success: Schema.Int }), Rpc.make('Count', { success: Schema.Int })]);

const CounterLive = Counter.toLayer(
    Effect.map(Ref.make(0), (count) => Counter.of({ Increment: ({ payload }) => Ref.updateAndGet(count, (current) => current + payload.amount), Count: () => Rpc.fork(Ref.get(count)) })),
    { maxIdleTime: '5 minutes' },
);
```

`charge` returns a receipt and `refund` reverses one. `Checkout.execute({ order })` runs one execution per idempotency key, and `Counter.client` returns a client per entity id:
- Activities suspended on a child workflow or a durable clock rerun their body on replay
- `Workflow.withCompensation` runs its inverse when the whole workflow fails after its step succeeded, top-level steps alone
- `DurableClock.sleep({ name, duration })` waits across restarts, a duration at or below `inMemoryThreshold` sleeping in memory
- `DurableDeferred.make(name, { success })` suspends a run until another process completes its token through `DurableDeferred.succeed`
- `Rpc.fork` lets one handler run beside the entity's queue, and `maxIdleTime` stops an idle entity until its next message
- Entity messages are volatile unless their `Rpc` holds the `ClusterSchema.Persisted` annotation
- `NodeClusterSocket.layer()` runs a cluster over SQL storage and `SingleRunner.layer()` every shard in one process
- `ClusterWorkflowEngine.layer` stores workflows as entity messages
- `EventLog` runs an event's handler before its journal commits the entry, and `EventLogRemote.layerEncrypted` syncs entries a replica stores
