# [DATA_CACHE]

Latency caching is correctness-neutral: losing a node costs one cold recompute. Tier-0 owns keyed single-flight, request deduplication, persisted lookups, and reference-counted pools. Stampede protection, TTL, and memoization are native. Valkey enters behind the same `KeyValueStore` port only when guarantees cross a process boundary, so escalation is a Layer swap. Write-behind belongs to the journal, never this lane.

## [01]-[INDEX]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                             |
| :-----: | :------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `TIER_ROWS`    | the escalation table — Tier-0 rows, the gated external row, the banned policies    |
|  [02]   | `KEYED_FLIGHT` | the memo tier, the keyed single-flight row, the dedup plane, the hit census        |
|  [03]   | `PERSISTED`    | the restart-surviving cache — the mint, the store assembly, the tenant partition   |
|  [04]   | `POOLS`        | reference-counted handles, keyed resource maps, the bounded origin-connection pool |
|  [05]   | `ASSEMBLY`     | the assembled export; occupancy and hit economics project at their own owners      |

## [02]-[TIER_ROWS]

- Owner: the `_tiers` anchor — every caching posture the folder admits as a row with its boundary and its trigger; the two banned rows carry their refusal as data so the argument is never re-had.
- Packages: none — decision facts.
- Growth: a new posture is one row; an external engine admission is the ONE gated row flipping from `gated` to a composed Layer at the root, with zero consumer edits because every consumer reads the port.
- Law: the lane guarantee is latency reduction, never correctness — every row's failure mode is a recompute; a consumer that breaks when a cache empties has smuggled state into the lane and is the named defect.
- Law: the escalation trigger is a process boundary — shared coherence across replicas or pub/sub invalidation fan-out; in-process needs are already covered, so reaching for an external engine below the trigger is refused by the table.
- Law: `writeBehind` is banned — it smuggles a durability obligation into a may-lose-data lane; a buffered durable write is the journal's outbox, full stop. `redisClient` is banned — the runtime natively supplies single-flight, TTL, and dedup, so the client footprint buys nothing below the escalation trigger; when the trigger fires, the engine row is Valkey behind the existing port.

```typescript signature
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

- Owner: `CacheLane.memo` — the ONE memo entry whose modality is the input shape: a bare effect caches whole (`Effect.cached`, or `cachedWithTTL` when a cadence rides the call), a unary function memoizes per-argument (`Effect.cachedFunction`, structural key equality); and `CacheLane.dedup(options)` — the request-cache Layer that turns fiber-tree request graphs into deduplicated batched loads. Keyed single-flight rides `Cache.make` at the package surface directly — a rename-forward alias adds no domain value and is refused here.
- Packages: `effect` (`Cache`, `Cache.ConsumerCache.cacheStats`, `Effect.cached`, `Effect.cachedWithTTL`, `Effect.cachedFunction`, `Request.makeCache`, `Layer.setRequestCache`, `Metric.set`, `Schedule.spaced`, `Duration`); `@rasm/ts/core` (`Convention` — the cache instrument and name rows).
- Entry: a read surface with stampede exposure mints `Cache.make` once at construction and yields `cache.get(key)` thereafter; a pure recompute memoizes through `memo`; the projection and retrieval read paths compose `dedup` at the root so their `RequestResolver`-batched loads share one cache per request graph — the resolver machinery is the SQL tier's, the cache Layer is this row.
- Receipt: cache entries remain correctness-neutral values, and `CacheLane.census(name, cache)` is the hit economics projection — one scoped probe folding the substrate's own `cacheStats` snapshot onto the `cacheHits`/`cacheMisses` levels tagged by the caller's cache name, so the `board#PACKS` `lake` hit-share tile reads a series a producer genuinely mints.
- Growth: a new cached surface is one mint with its own spec beside one `census` registration; a per-key TTL posture is the spec's `timeToLive` fold over the exit, never a second cache kind.
- Law: concurrent same-key lookups COLLAPSE to one execution — the single-flight guarantee is the constructor's, so no consumer wraps a semaphore around a cache; a hand-rolled in-flight map beside this row is the named reinvention.
- Law: capacity is a hard bound and eviction is the cache's own policy — an unbounded memo over unbounded keys is unspellable because `capacity` is a required field of the spec; `memo` over a function is bounded by its argument space and admits only where that space is provably small.
- Law: TTL is recompute cadence, not freshness truth — a consumer needing read-your-writes composes the reactive invalidation keys the journal stamps, never a shorter TTL.
- Law: the census READS the substrate's own counters and keeps none — `cacheStats` returns the cache's cumulative hits and misses, so the projection sets levels from that snapshot rather than incrementing counters at each lookup, and a hit therefore costs no metric write on the hot path; a mirror tally beside the cache is the hand-rolled reimplementation the substrate already forecloses.
- Law: the cache name is the caller's, because a cache is minted at its own read surface and the lane names none — the name keys the series exactly as the origin pool's scheme keys occupancy, and two caches sharing a name fold into one indistinguishable series at the board.
- Law: the census rides a scoped cadence, never the caller's fiber — one forked repeating probe per registered cache, released with the scope that registered it, so a lookup path never blocks on a metric read and a torn-down composition leaves no live probe.

```typescript signature
import { Cache, Duration, Effect, Layer, Metric, Request, Schedule, type Scope } from "effect"
import { Convention } from "@rasm/ts/core"

function _memo<B, E, R>(input: Effect.Effect<B, E, R>, ttl?: Duration.DurationInput): Effect.Effect<Effect.Effect<B, E, R>>
function _memo<A, B, E, R>(input: (a: A) => Effect.Effect<B, E, R>): Effect.Effect<(a: A) => Effect.Effect<B, E, R>>
function _memo(input: Effect.Effect<unknown> | ((a: unknown) => Effect.Effect<unknown>), ttl?: Duration.DurationInput) {
  return Effect.isEffect(input)
    ? ttl === undefined ? Effect.cached(input) : Effect.cachedWithTTL(input, ttl)
    : Effect.cachedFunction(input)
}

const _dedup = (options: { readonly capacity: number; readonly timeToLive: Duration.DurationInput }) =>
  Layer.setRequestCache(Request.makeCache(options))

const _CENSUS = { cadence: Duration.seconds(30) } as const // sampling cadence for the read-side probe; the counters are cumulative, so a miss between samples is never lost

const _hits = Convention.mount(Convention.metric.cacheHits)
const _misses = Convention.mount(Convention.metric.cacheMisses)

// `ConsumerCache.cacheStats` is the substrate's own cumulative snapshot, so the census SETS two levels from one read
// rather than instrumenting the lookup path; the probe forks on the registering scope and dies with it.
const _census = (name: string, cache: Cache.ConsumerCache<unknown, unknown, unknown>): Effect.Effect<void, never, Scope.Scope> =>
  Effect.forkScoped(
    Effect.repeat(
      Effect.flatMap(cache.cacheStats, (stats) =>
        Effect.zipRight(
          Metric.set(Metric.tagged(_hits, Convention.rasm.cacheName, name), stats.hits),
          Metric.set(Metric.tagged(_misses, Convention.rasm.cacheName, name), stats.misses),
        )),
      Schedule.spaced(_CENSUS.cadence),
    ),
  ).pipe(Effect.asVoid)
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

```typescript signature
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

- Owner: resource rows — `RcRef.make` for one shared scoped resource, `RcMap.make` for keyed singletons, and `CacheLane.origins`, the ONE bounded TTL pool keyed by structural origin.
- Packages: `effect` (`RcRef.make`, `RcMap.make`, `RcMap.get`, `RcMap.invalidate`, `KeyedPool.makeWithTTL`, `KeyedPool.get`, `Duration`, `Scope`, `Data`).
- Entry: the OLAP lane's engine instance and the per-scope warm surfaces ride `RcRef`/`RcMap` (`RcMap.get(map, key)` acquires-or-shares under the caller's `Scope`, `RcMap.invalidate(map, key)` evicts on rotation or poison); the remote-origin lane mints `origins(acquire, policy?)` keyed by the `Data`-classed `OriginKey` so structural equality pools connections and `KeyedPool.get` leases a live client per transfer under the caller's `Scope`.
- Growth: a pool sizing posture is a policy-row override; a keyed family with complex identity keys by a `Data`-classed value, structural equality carried by construction; `OriginKey<Scheme>` carries the caller's closed scheme vocabulary beside the wire coordinate so one pool arbitrates every protocol's sessions and the remote plane's acquire dispatches on the key alone.
- Law: `RcMap` and `KeyedPool` divide by cardinality — `RcMap` shares ONE live instance per key among concurrent holders, `KeyedPool` holds up to N instances per key for exclusive leases; a protocol whose control connection carries one transfer at a time (the FTP law) is exactly why the origin row is the pool, never the map.
- Law: lifetime is reference-counted or pool-owned — release follows the last scope and idle window, so hot handles survive bursts without manual cleanup.
- Law: this row pools RESOURCES, the `Stores` map pools LAYERS — the tenancy store map stays the scope-family owner, and this lane's maps hold engine sessions and warm clients beneath it; the echo is deliberate, the owners distinct.
- Law: occupancy rides the pool's own acquire bracket — each leased item increments the `Convention.metric.poolHeld` level tagged by the key's closed scheme vocabulary and its release finalizer decrements it, so held-connection pressure is dashboard-visible with zero consumer wiring and the pool's lifetime discipline stays the truth; the row's non-monotonic form is what lets a decrement land, and a monotonic row here exports a total that only climbs.

```typescript signature
import { Data, KeyedPool, Metric, type Scope } from "effect"
import { Convention } from "@rasm/ts/core"

const _ORIGIN_POOL = { min: 0, max: 4, ttl: Duration.minutes(5) } as const

class OriginKey<Scheme extends string = string> extends Data.Class<{
  readonly scheme: Scheme
  readonly host: string
  readonly port: number
  readonly username: string
}> {}

const _held = Convention.mount(Convention.metric.poolHeld) // the row's `updown` form is what makes the release finalizer's decrement legal on the wire

const _counted = (scheme: string) => Metric.tagged(_held, Convention.rasm.poolScheme, scheme)

const _origins = <Scheme extends string, A, E, R>(
  acquire: (key: OriginKey<Scheme>) => Effect.Effect<A, E, R | Scope.Scope>,
  policy?: Partial<{ readonly min: number; readonly max: number; readonly ttl: Duration.DurationInput }>,
) =>
  KeyedPool.makeWithTTL({
    acquire: (key: OriginKey<Scheme>) =>
      Effect.tap(acquire(key), () =>
        Effect.zipRight(
          Metric.incrementBy(_counted(key.scheme), 1),
          Effect.addFinalizer(() => Metric.incrementBy(_counted(key.scheme), -1)), // rides the item's own Scope: eviction, TTL, and release all decrement
        )),
    min: () => policy?.min ?? _ORIGIN_POOL.min,
    max: () => policy?.max ?? _ORIGIN_POOL.max,
    timeToLive: policy?.ttl ?? _ORIGIN_POOL.ttl,
  })
```

## [06]-[ASSEMBLY]

- Owner: `CacheLane`, the single export assembling the admitted tier, memo, persisted, store, backing, scope, and origin-pool owners.
- Law: each projection seats at the owner holding its evidence — occupancy at `_origins`, the acquisition and release bracket that knows a lease, and hit economics at `_census`, the one reader of the substrate's own counters.

```typescript signature
const CacheLane = {
  tiers: _tiers,
  memo: _memo,
  dedup: _dedup,
  census: _census,
  persisted: _persisted,
  store: _store,
  backing: _backing,
  scoped: _scoped,
  origins: _origins,
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { CacheLane, OriginKey }
```

## [07]-[RESEARCH]

(none)
