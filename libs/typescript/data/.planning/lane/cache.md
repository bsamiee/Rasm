# [DATA_CACHE]

The latency lane, correctness-neutral by law: a cache node vanishing costs a cold recompute and nothing else, so this page owns the Tier-0 in-process rows the runtime already supplies — keyed single-flight caches, fiber-tree request deduplication, restart-surviving persisted lookups, and reference-counted resource pools — plus the one escalation law that gates any external engine. Stampede protection, TTL, and memoization are native; an external cache engine is warranted ONLY when the guarantee must cross a process boundary, and that gated row is Valkey behind the same `KeyValueStore` port, so escalation is a Layer swap, never a code path. Write-behind is banned from the lane outright — a cache that owes durability is a correctness surface misfiled, and the journal owns those.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                             |
| :-----: | :------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `TIER_ROWS`    | the escalation table — Tier-0 rows, the gated external row, the banned policies    |
|  [02]   | `KEYED_FLIGHT` | the memo tier, the keyed single-flight row, the request-dedup plane                |
|  [03]   | `PERSISTED`    | the restart-surviving cache — the mint, the store assembly, the tenant partition   |
|  [04]   | `POOLS`        | reference-counted handles, keyed resource maps, the bounded origin-connection pool |

## [02]-[TIER_ROWS]

- Owner: the `_tiers` anchor — every caching posture the folder admits as a row with its boundary and its trigger; the two banned rows carry their refusal as data so the argument is never re-had.
- Packages: none — decision facts.
- Growth: a new posture is one row; an external engine admission is the ONE gated row flipping from `gated` to a composed Layer at the root, with zero consumer edits because every consumer reads the port.
- Law: the lane guarantee is latency reduction, never correctness — every row's failure mode is a recompute; a consumer that breaks when a cache empties has smuggled state into the lane and is the named defect.
- Law: the escalation trigger is a process boundary — shared coherence across replicas or pub/sub invalidation fan-out; in-process needs are already covered, so reaching for an external engine below the trigger is refused by the table.
- Law: `writeBehind` is banned — it smuggles a durability obligation into a may-lose-data lane; a buffered durable write is the journal's outbox, full stop. `redisClient` is banned — the runtime natively supplies single-flight, TTL, and dedup, so the client footprint buys nothing below the escalation trigger; when the trigger fires, the engine row is Valkey behind the existing port.

```typescript
const _tiers = {
  memo: { row: "CacheLane.memo — Effect.cached/cachedWithTTL/cachedFunction", boundary: "in-process", trigger: "pure recompute avoidance" },
  keyed: { row: "Cache.make", boundary: "in-process, capacity-bounded", trigger: "keyed lookups with stampede risk" },
  request: { row: "CacheLane.dedup — Request.makeCache + Layer.setRequestCache", boundary: "fiber tree", trigger: "N+1 dedup across one request graph" },
  persisted: { row: "CacheLane.persisted over CacheLane.store", boundary: "restart-surviving, single node", trigger: "expensive lookups worth keeping warm" },
  pooled: { row: "RcRef / RcMap", boundary: "in-process resource lifetime", trigger: "shared scoped resources, one live instance per key" },
  bounded: { row: "CacheLane.origins — KeyedPool.makeWithTTL", boundary: "in-process, min/max sized", trigger: "bounded reuse of expensive connections — the remote-origin transfer lanes" },
  external: { row: "Valkey behind KeyValueStore", boundary: "cross-process", trigger: "GATED — replica coherence or invalidation fan-out only" },
  writeBehind: { row: "banned", boundary: "-", trigger: "durability smuggled into the latency lane" },
  redisClient: { row: "banned", boundary: "-", trigger: "native tier already owns single-flight/TTL/dedup" },
} as const

declare namespace CacheLane {
  type Tier = keyof typeof _tiers
  type _Rows<T extends Record<Tier, { readonly row: string; readonly boundary: string; readonly trigger: string }> = typeof _tiers> = T
}
```

## [03]-[KEYED_FLIGHT]

- Owner: `CacheLane.memo` — the ONE memo entry whose modality is the input shape: a bare effect caches whole (`Effect.cached`, or `cachedWithTTL` when a cadence rides the call), a unary function memoizes per-argument (`Effect.cachedFunction`, structural key equality); and `CacheLane.dedup(options)` — the request-cache Layer that turns fiber-tree request graphs into deduplicated batched loads. The keyed single-flight row is `Cache.make` consumed at the package surface directly — a rename-forward alias adds no domain value and is refused here.
- Packages: `effect` (`Cache`, `Effect.cached`, `Effect.cachedWithTTL`, `Effect.cachedFunction`, `Request.makeCache`, `Layer.setRequestCache`, `Duration`).
- Entry: a read surface with stampede exposure mints `Cache.make` once at construction and yields `cache.get(key)` thereafter; a pure recompute memoizes through `memo`; the projection and retrieval read paths compose `dedup` at the root so their `RequestResolver`-batched loads share one cache per request graph — the resolver machinery is the SQL tier's, the cache Layer is this row.
- Receipt: `cacheStats` — hits, misses, size — read on demand and emitted through the fact stream's meter row by the composition that owns the cache; the lane never self-reports.
- Growth: a new cached surface is one mint with its own spec; a per-key TTL posture is the spec's `timeToLive` fold over the exit, never a second cache kind.
- Law: concurrent same-key lookups COLLAPSE to one execution — the single-flight guarantee is the constructor's, so no consumer wraps a semaphore around a cache; a hand-rolled in-flight map beside this row is the named reinvention.
- Law: capacity is a hard bound and eviction is the cache's own policy — an unbounded memo over unbounded keys is unspellable because `capacity` is a required field of the spec; `memo` over a function is bounded by its argument space and admits only where that space is provably small.
- Law: TTL is recompute cadence, not freshness truth — a consumer needing read-your-writes composes the reactive invalidation keys the journal stamps, never a shorter TTL.

```typescript
import { Cache, Duration, Effect, Layer, Request } from "effect"

function _memo<B, E, R>(input: Effect.Effect<B, E, R>, ttl?: Duration.DurationInput): Effect.Effect<Effect.Effect<B, E, R>>
function _memo<A, B, E, R>(input: (a: A) => Effect.Effect<B, E, R>): Effect.Effect<(a: A) => Effect.Effect<B, E, R>>
function _memo(input: Effect.Effect<unknown> | ((a: unknown) => Effect.Effect<unknown>), ttl?: Duration.DurationInput) {
  return Effect.isEffect(input)
    ? ttl === undefined ? Effect.cached(input) : Effect.cachedWithTTL(input, ttl)
    : Effect.cachedFunction(input)
}

const _dedup = (options: { readonly capacity: number; readonly timeToLive: Duration.DurationInput }) =>
  Layer.setRequestCache(Request.makeCache(options))
```

## [04]-[PERSISTED]

- Owner: `CacheLane.persisted(spec)` — the ONE restart-surviving mint: `PersistedCache.make` with the lane's in-memory front tier folded in as policy defaults (`inMemoryCapacity`/`inMemoryTTL`), so every persisted band fronts its store with a hot tier unless the spec overrides it; `CacheLane.store(kvs, prefix)` — the ONE backing assembly composing `Persistence.layerResultKeyValueStore` over the selected `KeyValueStore` row with the tenant partition interposed structurally; and `CacheLane.scoped(prefix)` — the raw prefix transformer for compositions that own their store wiring. `CacheLane.backing.memory` remains the explicit process-local test or isolated-single-app row and never backs a shared tenant surface.
- Packages: `@effect/experimental` (`PersistedCache.make`, `Persistence.layerResultKeyValueStore`, `Persistence.layerResultMemory`); `@effect/platform` (`KeyValueStore.layerMemory`, `KeyValueStore.layerFileSystem`, `KeyValueStore.prefix`); `effect` (`Duration`, `Effect`, `Layer`).
- Entry: an expensive schema-keyed lookup (a rendered derivative index, a resolved capability report) mints `persisted` with its request schema; the root composes `store(backing.storeFile(dir), scopeKey)` per deployment posture, and the gated Valkey admission is a `KeyValueStore` implementation handed to the same `store` call, with this page unchanged.
- Growth: a new backing is one Layer row behind the same port; a front-tier posture is a spec override, never a second cache kind.
- Law: persisted bands are tenant-partitioned by construction — `store` requires the partition and rewrites the `KeyValueStore` behind the persistence tier through `KeyValueStore.prefix` BEFORE the result store exists, so two apps sharing one physical store cannot collide keys and one app's cached report can never serve another; omission is not a signature modality, and an isolated caller states its own constant partition explicitly.
- Law: durability equals the injected store's — the cache never promises more than its backing; a persisted entry lost with its node is a recompute, which is the lane's lawful failure mode.
- Law: the persisted cache is an overlay, never a record of truth — nothing reads it as authority, and dropping the backing store costs warm-up latency only; the journal boundary law of the folder applies unchanged.
- Law: keys are schema-typed persistables — the request schema owns identity and serialization (`Schema.TaggedRequest` under `PrimaryKey`), success and failure both encode through the key's own result schemas so a persisted failure replays typed, and `timeToLive(request, exit)` folds both dispositions so hits and misses age separately; a cache key is never a hand-built string, and a shape change invalidates structurally.

```typescript
import { Persistence, PersistedCache } from "@effect/experimental"
import { KeyValueStore } from "@effect/platform"
import type { Schema } from "effect"

const _FRONT = { inMemoryCapacity: 256, inMemoryTTL: Duration.minutes(1) } as const

const _backing = {
  memory: Persistence.layerResultMemory,
  storeMemory: KeyValueStore.layerMemory,
  storeFile: (directory: string) => KeyValueStore.layerFileSystem(directory),
} as const

const _scoped = (prefix: string): Layer.Layer<KeyValueStore.KeyValueStore, never, KeyValueStore.KeyValueStore> =>
  Layer.effect(
    KeyValueStore.KeyValueStore,
    Effect.map(KeyValueStore.KeyValueStore, (store) => KeyValueStore.prefix(store, prefix)),
  )

const _store = <E, R>(
  kvs: Layer.Layer<KeyValueStore.KeyValueStore, E, R>,
  prefix: string,
): Layer.Layer<Persistence.ResultPersistence, E, R> =>
  Persistence.layerResultKeyValueStore.pipe(
    Layer.provide(_scoped(prefix).pipe(Layer.provide(kvs))),
  )

const _persisted = <K extends Persistence.ResultPersistence.KeyAny, R>(spec: {
  readonly storeId: string
  readonly lookup: (key: K) => Effect.Effect<Schema.WithResult.Success<K>, Schema.WithResult.Failure<K>, R>
  readonly timeToLive: (...args: Persistence.ResultPersistence.TimeToLiveArgs<K>) => Duration.DurationInput
  readonly inMemoryCapacity?: number
  readonly inMemoryTTL?: Duration.DurationInput
}) => PersistedCache.make({ ..._FRONT, ...spec })
```

## [05]-[POOLS]

- Owner: the resource rows — `RcRef.make` for one shared scoped resource and `RcMap.make` for keyed single-instance families, both consumed at the package surface directly (a rename-forward alias is refused) — plus `CacheLane.origins`, the ONE bounded keyed-pool mint: min/max-sized, TTL-expiring connection reuse keyed by a structural origin coordinate, the row every remote-origin transfer lane (`object/remote.md`'s SFTP/FTP/DAV clients) rides.
- Packages: `effect` (`RcRef.make`, `RcMap.make`, `RcMap.get`, `RcMap.invalidate`, `KeyedPool.makeWithTTL`, `KeyedPool.get`, `Duration`, `Scope`, `Data`).
- Entry: the OLAP lane's engine instance and the per-scope warm surfaces ride `RcRef`/`RcMap` (`RcMap.get(map, key)` acquires-or-shares under the caller's `Scope`, `RcMap.invalidate(map, key)` evicts on rotation or poison); the remote-origin lane mints `origins(acquire, policy?)` keyed by the `Data`-classed `OriginKey` so structural equality pools connections and `KeyedPool.get` leases a live client per transfer under the caller's `Scope`.
- Growth: a pool sizing posture is a policy-row override; a keyed family with complex identity keys by a `Data`-classed value, structural equality carried by construction; `OriginKey<Scheme>` carries the caller's closed scheme vocabulary beside the wire coordinate so one pool arbitrates every protocol's sessions and the remote plane's acquire dispatches on the key alone.
- Law: `RcMap` and `KeyedPool` divide by cardinality — `RcMap` shares ONE live instance per key among concurrent holders, `KeyedPool` holds up to N instances per key for exclusive leases; a protocol whose control connection carries one transfer at a time (the FTP law) is exactly why the origin row is the pool, never the map.
- Law: lifetime is reference-counted or pool-owned, never manual — the resource releases when the last scope closes plus the idle window, so a leak is unspellable and a hot handle survives bursts without churn.
- Law: this row pools RESOURCES, the `Stores` map pools LAYERS — the tenancy store map stays the scope-family owner, and this lane's maps hold engine sessions and warm clients beneath it; the echo is deliberate, the owners distinct.

```typescript
import { Data, KeyedPool, type Scope } from "effect"

const _ORIGIN_POOL = { min: 0, max: 4, ttl: Duration.minutes(5) } as const

class OriginKey<Scheme extends string = string> extends Data.Class<{
  readonly scheme: Scheme
  readonly host: string
  readonly port: number
  readonly username: string
}> {}

const _origins = <Scheme extends string, A, E, R>(
  acquire: (key: OriginKey<Scheme>) => Effect.Effect<A, E, R | Scope.Scope>,
  policy?: Partial<{ readonly min: number; readonly max: number; readonly ttl: Duration.DurationInput }>,
) =>
  KeyedPool.makeWithTTL({
    acquire,
    min: () => policy?.min ?? _ORIGIN_POOL.min,
    max: () => policy?.max ?? _ORIGIN_POOL.max,
    timeToLive: policy?.ttl ?? _ORIGIN_POOL.ttl,
  })

const CacheLane = {
  tiers: _tiers,
  memo: _memo,
  dedup: _dedup,
  persisted: _persisted,
  store: _store,
  backing: _backing,
  scoped: _scoped,
  origins: _origins,
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { CacheLane, OriginKey }
```
