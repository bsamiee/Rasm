# [PERSISTENCE]

## [01]-[STORES]

State outliving one call selects its service by shape, and its backing by where processes share it:

| [INDEX] | [BACKING]   | [KEYVALUESTORE]                                  | [PERSISTENCE]                                |
| :-----: | :---------- | :----------------------------------------------- | :------------------------------------------- |
|  [01]   | Process     | `KeyValueStore.layerMemory`                      | `Persistence.layerMemory`                    |
|  [02]   | Folder      | `KeyValueStore.layerFileSystem(directory)`       | `Persistence.layerKvs` over it               |
|  [03]   | Web Storage | `KeyValueStore.layerStorage(() => localStorage)` | `Persistence.layerKvs` over it               |
|  [04]   | SQL table   | `KeyValueStore.layerSql({ table })`              | `Persistence.layerSql`, `layerSqlMultiTable` |
|  [05]   | Redis       | None                                             | `Persistence.layerRedis`                     |

`KeyValueStore` holds string or byte values by key, and `Persistence` holds encoded `Exit` values by request key with a time to live per entry:
- `KeyValueStore.toSchemaStore(store, S)` reads and writes typed values through the JSON codec of `S`, `get` returning an `Option`
- Schema stores fail a read of a value an older build wrote with `SchemaError`, never a miss, unless its schema catches decoding as absent
- `modify` reads, then writes with no lock in every store, so concurrent writers of one key lose updates
- `KeyValueStore.prefix(store, prefix)` scopes keys, its `keys`, `size`, and `isEmpty` listing every key of the store beneath and filtering
- `layerFileSystem` writes one percent-encoded file per key and deletes its folder on `clear`, so a folder holds one store
- `Persistence.layerSqlMultiTable` names a table `effect_persistence_<storeId>` unescaped, and arbitrary store ids take `layerSql`
- `UserDirectories.layer` resolves `config`, `data`, `cache`, and `state` folders per app name, XDG variables first
- Kinds except `cache` resolve to one `~/Library/Application Support/<app>` folder on macOS, and no kind names a log folder

Documents a process edits faster than a store takes writes collect in memory and write behind:

```ts
const writer = Effect.fn('writer')(function* (write: (rows: Readonly<Record<string, Entry>>) => Effect.Effect<void, SqlError>, interval: Duration.Input) {
    const pending = yield* Ref.make(Record.empty<string, Entry>());
    const flush = Effect.gen(function* () {
        const written = yield* Ref.get(pending);
        yield* Record.isEmptyRecord(written) ? Effect.void : write(written);
        yield* Ref.update(pending, Record.filter((entry, locator) => written[locator] !== entry));
    }).pipe(Effect.catch((error) => Effect.logWarning('Flush failed', error)));
    yield* Effect.addFinalizer(() => flush);
    yield* Effect.forkScoped(Effect.repeat(flush, Schedule.spaced(interval)));
    return (locator: string, entry: Entry) => Ref.update(pending, Record.set(locator, entry));
});
```

`Entry` is one container's document keyed by its locator, and `write` upserts every row in one transaction:
- `Record.filter` drops the entries this flush wrote by identity, keeping each entry an edit replaced during the write
- Failed flushes log and keep `pending` for the next pass, so one failure never ends the writer
- Scopes run finalizers in reverse order, so the finalizer registered before the fork flushes the tail after the loop ends
- `RcMap` opens one writer per key in its entry's scope, so the entry's release flushes and closes the document
- `idleTimeToLive: Duration.infinity` keeps an entry until `RcMap.invalidate` runs on its source's exit
- Kept entries also keep a failed lookup, so `Effect.tapError(RcMap.get(map, key), () => RcMap.invalidate(map, key))` drops it

## [02]-[CACHES]

`PersistedCache` reads its in-memory `Cache`, then a `Persistence` store, then runs its lookup and stores the `Exit`, so a result reaches every process and run sharing the store. Caches compare keys alone and never check an entry against what produced it:
- Inputs known before the lookup go in the key (request fields, a digest of the code and settings computing the entry, a tool's identity)
- Older builds' entries miss under that key, as their shape or meaning belongs to the code that wrote them
- Inputs the lookup discovers (modules a config loads, files an import crawl reads) go in the entry with a stamp hashed over them
- Reads hash those inputs again and recompute the entry when the stamp differs

```ts
class Inferred extends Persistable.Class<{ payload: { readonly digest: string; readonly file: string } }>()('Inferred', { primaryKey: ({ digest, file }) => `${digest}:${file}`, success: Targets }) {}

const inferred = Effect.fnUntraced(function* (infer: (file: string) => Effect.Effect<Omit<Targets, 'stamp'>>, stamp: (sources: readonly string[]) => Effect.Effect<string>) {
    const lookup = Effect.fnUntraced(function* (request: Inferred) {
        const read = yield* infer(request.file);
        return new Targets({ ...read, stamp: yield* stamp(read.sources) });
    });
    const cache = yield* PersistedCache.make(lookup, { storeId: 'inferred', timeToLive: (exit) => (Exit.isSuccess(exit) ? Duration.infinity : Duration.zero) });
    return Effect.fnUntraced(function* (request: Inferred) {
        const held = yield* cache.get(request);
        return (yield* stamp(held.sources)) === held.stamp ? held : yield* Effect.andThen(cache.invalidate(request), cache.get(request));
    });
});
```

`Targets` is a schema class holding a project's inferred targets, the `sources` its inference read, and their `stamp`. `digest` hashes the plugin code and settings, and `infer` reads one project file:
- `timeToLive(exit, request)` of zero or less skips the store write, so a failed lookup stores nothing
- `inMemoryTTL` (10 seconds for every exit, failures included) and `inMemoryCapacity` (1024) bound the in-memory `Cache`
- In-memory hits compare every request field and store hits compare `primaryKey` alone, so `primaryKey` reads every payload field
- Stored exits failing to decode under their request's schemas read as a miss, and the lookup's `Exit` replaces them
- `cache.invalidate(request)` deletes stored and in-memory entries
- `PersistedCache.make` requires `Persistence` and `Scope`, and `requireServicesAt: 'lookup'` moves the lookup's services to `get`

## [03]-[QUEUES]

`PersistedQueue` hands work across fibers and processes sharing a store and delivers each element at least once. A consumer forks one take loop and hands callers the offer:

```ts
const queued = Effect.fnUntraced(function* <E, R>(name: string, work: (delivery: Delivery, attempts: number) => Effect.Effect<unknown, E, R>) {
    const queue = yield* PersistedQueue.make({ name, schema: TailscaleEvent });
    yield* queue.take((event, { id, attempts }) => work({ id, event }, attempts)).pipe(Effect.ignoreCause({ log: 'Warn', message: `Webhook queue ${name} work failed` }), Effect.forever, Effect.forkScoped);
    return ({ id, event }: Delivery) => queue.offer(event, { id });
});
```

`TailscaleEvent` is a webhook's event schema, and `Delivery` pairs the sender's webhook id with its decoded event. `take` fails with a handler's failure after scheduling its retry, and `Effect.ignoreCause` keeps the loop taking:
- `take` marks an element done when the handler succeeds, and an interrupted handler releases it without counting an attempt
- Attempts count when an element is claimed, a crash mid-handler consumes one, and handlers are idempotent
- `maxAttempts` defaults to 10, `retrySchedule` to exponential delays from 1 second capped at 5 minutes
- Retry delays replay `retrySchedule` from the stored attempt count, so `Schedule.upTo({ duration })` caps summed delays, not wall time
- Elements failing to decode mark failed at once and never reach `take`
- `offer(value, { id })` enqueues nothing for a repeated `id` until `layerCleanup` removes its element, so a sender's id deduplicates its redelivery
- `deduplicate: 'untilExecuted'` frees an `id` once its element completes or fails its last attempt, and its handler reads current state
- Offers arriving while an `'untilExecuted'` element runs enqueue nothing
- `delay` postpones first delivery by the store's clock, SQL rounding it up to whole seconds, and an infinite delay stores nothing
- `PersistedQueue.layer` builds the factory over `layerStoreMemory`, `layerStoreSql()`, or `layerStoreRedis()`
- `layerCleanup()` runs hourly in one instance, removing completed elements after 30 days and failed ones only past `failedTimeToLive`

## [04]-[LIMITS]

`RateLimiter` and `CircuitBreaker` guard calls by string key over a store, memory stores holding state in the process (`RateLimiter.layerStoreMemory`, `CircuitBreaker.layerStoreMemory()`) and `layerStoreRedis()` sharing it:
- `limiter.consume({ key, limit, window, algorithm, onExceeded, tokens })` counts by `'fixed-window'` (default) or `'token-bucket'`
- `onExceeded: 'fail'` (default) fails with a `RateLimitExceeded` reason holding `retryAfter`, and `'delay'` returns its wait in `ConsumeResult.delay`
- `RateLimiter.makeWithRateLimiter` returns a function delaying an effect by that wait, and `RateLimiter.sleep(limiter, options)` sleeps it out
- `CircuitBreaker.layer({ tripPolicy, openDuration })` opens after `consecutive(n)` failures or a `rate({ threshold, size, minimumCalls })`
- `openDuration` takes a duration or a `Schedule`, its n-th delay timing the n-th consecutive opening, and an exponential one takes a cap
- `failure({ retryAfter })` lengthens one opening up to `maxOpenDuration` (1 hour), which never caps `openDuration`
- `breaker.execute({ key, classify })` fails calls with `CircuitOpen` while open, and admits one half-open probe as others fail with `CircuitHalfOpen`
- `classify(exit, { probe })` maps an exit to `CircuitBreaker.success`, `ignored`, or `failure({ retryAfter })`, interruption counting as ignored
- `Effect.retry` and `Effect.timeout` go inside `execute`, so the breaker sees one outcome per call, and retries of a rejection go outside
- Breakers sharing a key and a store share one circuit whatever their policies, so each policy takes its own key or Redis `prefix`

## [05]-[DURABLE_EXECUTION]

Work surviving a restart selects its family by what replays and where it runs:

| [INDEX] | [WORK]                                   | [FORM]                                        |
| :-----: | :--------------------------------------- | :-------------------------------------------- |
|  [01]   | One job per element, no step replay      | `PersistedQueue`                              |
|  [02]   | Steps replayed from their stored results | `Workflow` with `Activity` steps              |
|  [03]   | Workflow step a worker pool runs         | `DurableQueue.process`, `DurableQueue.worker` |
|  [04]   | State addressed by id across runners     | `Entity` of `effect/cluster`                  |
|  [05]   | One effect on one runner of a cluster    | `Singleton.make`, `ClusterCron.make`          |
|  [06]   | Local-first state synced across devices  | `EventLog` of `effect/eventlog`               |

Workflows record each activity's `Exit`, and a replay after a restart reads it in place of running the step again:

```ts
const Release = Workflow.make('Release', { payload: { tag: Schema.String }, idempotencyKey: ({ tag }) => tag, error: PublishError });

const ReleaseLive = Release.toLayer(({ tag }) =>
    Activity.make({ name: 'attach', success: Schema.Array(Schema.String), execute: attach(tag) }).pipe(
        Workflow.withCompensation((assets) => detach(tag, assets)),
        Effect.andThen(Activity.make({ name: 'publish', error: PublishError, execute: publish(tag) })),
    ),
);
```

`attach` uploads a tag's release files and returns their asset ids, `detach` deletes them, and `publish` sends the files to each registry. `Release.execute({ tag })` runs one execution per tag, and a failed `publish` runs `detach` over the stored asset ids:
- Activities suspended on a child workflow or a durable clock rerun their body on replay, so effects before the suspension are idempotent
- `Workflow.withCompensation` registers on top-level steps alone, never inside an activity
- `execute(payload, { discard: true })` returns the execution id, which `poll`, `resume`, and `interrupt` take
- `DurableClock.sleep({ name, duration })` waits across restarts, and a duration at or below `inMemoryThreshold` (60 seconds) sleeps in memory
- `DurableDeferred.await(deferred)` suspends a run until another process completes its `DurableDeferred.token` through `DurableDeferred.succeed`
- `DurableQueue.process(queue, payload)` suspends a run until a `DurableQueue.worker(queue, handler)` records the handler's `Exit`
- Dead-lettered `DurableQueue` items park their run until requeued
- `Entity.make(type, rpcs).toLayer(handlers, { maxIdleTime })` runs messages one at a time per entity id, stopping an idle entity
- `Rpc.fork` runs one handler beside the entity's queue, and `Entity.client` yields a client per entity id
- Entity messages are volatile unless their `Rpc` holds the `ClusterSchema.Persisted` annotation
- `NodeClusterSocket.layer()` runs a cluster over SQL storage, and `SingleRunner.layer()` runs every shard in one process over SQL messages
- `ClusterWorkflowEngine.layer` stores workflow runs as entity messages
- `EventLog` runs an event's handler before its journal commits the entry, and a failed handler commits nothing
- `EventLogRemote.layerEncrypted` syncs entries a server stores encrypted, and clients build the compacted snapshots it keeps
