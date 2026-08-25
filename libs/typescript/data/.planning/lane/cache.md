# [DATA_CACHE]

Latency caching is correctness-neutral: losing a node costs one cold recompute. Tier-0 owns keyed single-flight, request deduplication, persisted lookups, and reference-counted pools. Stampede protection, TTL, and memoization are native. Valkey enters behind the same `KeyValueStore` port only when guarantees cross a process boundary, so escalation is a Layer swap. Write-behind belongs to the journal, never this lane.

## [01]-[INDEX]

- [02]-[TIER_ROWS]: escalation table — Tier-0 rows, the gated external row, the banned policies.
- [03]-[KEYED_FLIGHT]: memo tier, keyed single-flight row, dedup plane, hit census.
- [04]-[PERSISTED]: restart-surviving cache — mint, store assembly, tenant partition.
- [05]-[POOLS]: reference-counted handles, keyed resource maps, the bounded origin-connection pool.
- [06]-[ASSEMBLY]: assembled export; lease occupancy and cache economics project at their own owners.

## [02]-[TIER_ROWS]

- Owner: the `_tiers` anchor — every caching posture the folder admits as a row answering the same five descriptor coordinates (`fits`, `admit`, `tenancy`, `lifetime`, `degrade`) beside the mechanism it names; the two banned rows carry their refusal as data so the argument is never re-had.
- Packages: none — decision facts.
- Growth: a new posture is one row with all five coordinates answered — a row leaving `tenancy` or `degrade` blank is an unfinished admission whose callers then hardcode it — and an external engine admission is the ONE gated row flipping from `gated` to a composed Layer at the root, with zero consumer edits because every consumer reads the port.
- Law: the lane guarantee is latency reduction, never correctness — every row's failure mode is a recompute; a consumer that breaks when a cache empties has smuggled state into the lane and is the named defect.
- Law: the tier row answers what a caller selects on, and its honest non-answer carries as much weight as its columns — `fits` names what a row SUITS, `admit` the call that puts a value in it, and DEGRADE reads off the mechanism each row already owns. TENANCY only two rows decide, because the persisted and external rows rewrite their backing behind a prefix before the result tier exists; the in-process rows decide nothing, since a memo, a keyed cache, a request graph, and a pooled handle all key by whatever the caller hands them, so separation is an obligation the CALLER discharges by carrying the tenant IN that key and a tier-level answer states a guess. That non-decision is never a `degrade` entry: a caching posture never owned tenant separation, so it gives none up — but a tenant-scoped value cached under a bare domain id still serves one app's entry to the next, silently and forever.
- Law: this table mints no isolation vocabulary — `Identity.tenancy` publishes the closed tenancy roster at S0, a leaf engine row is SELECTED under it and states the MECHANISM it separates by, and re-spelling a roster value here forks it while reaching upward for one violates the folder's import direction outright.
- Law: `lifetime` names the clock AND who stops it — a row whose expiry belongs to an injected backing says so, because a caller reading a lane-wide TTL assumption sizes its warm-up against the wrong number.
- Law: `degrade` is the column keeping the table honest — a posture whose given-up property stays unstated reads as strictly better than its neighbours, and the escalation trigger below is exactly the boundary at which one row's `degrade` stops being affordable.
- Law: the escalation trigger is a process boundary — shared coherence across replicas or pub/sub invalidation fan-out; in-process needs are already covered, so reaching for an external engine below the trigger is refused by the table.
- Law: `writeBehind` is banned — it smuggles a durability obligation into a may-lose-data lane; a buffered durable write is the journal's outbox, full stop. `redisClient` is banned — the runtime natively supplies single-flight, TTL, and dedup, so the client footprint buys nothing below the escalation trigger; when the trigger fires, the engine row is Valkey behind the existing port.

```typescript signature
const _tiers = {
  memo: {
    row: "CacheLane.memo — Effect.cached/cachedWithTTL/cachedFunction",
    fits: "one pure recompute worth avoiding for the life of a process",
    admit: "the first call through the returned effect",
    tenancy: "decides none — the argument IS the key, so a tenant-scoped value carries its tenant there",
    lifetime: "the holding fiber's process, or the TTL when a cadence rides the call",
    degrade: "gives up eviction and capacity on the nullary form — a held effect pins its value until the process ends",
  },
  keyed: {
    row: "Cache.make",
    fits: "keyed lookups where concurrent misses would stampede one origin",
    admit: "cache.get on a key the spec's lookup resolves",
    tenancy: "decides none — a tenant-scoped entry keys by a value carrying the tenant, never a bare domain id",
    lifetime: "capacity eviction and the spec's time-to-live, whichever comes first",
    degrade: "gives up per-tenant fairness — one app's key pressure evicts another's warm entries",
  },
  request: {
    row: "CacheLane.dedup — Request.makeCache + Layer.setRequestCache",
    fits: "N+1 fan-out inside one request graph",
    admit: "a Request the resolver batches under the composed Layer",
    tenancy: "decides none — one graph happens to be one principal, which the Layer scope never enforces",
    lifetime: "the fiber tree the Layer was composed around",
    degrade: "gives up cross-request sharing — two concurrent graphs wanting one row still issue two reads",
  },
  persisted: {
    row: "CacheLane.persisted over CacheLane.store",
    fits: "an expensive schema-keyed lookup worth surviving a restart",
    admit: "a TaggedRequest key the spec's lookup resolves and the store encodes",
    tenancy: "interposes one — the store assembly rewrites the backing behind a prefix before the result tier exists",
    lifetime: "the spec's per-exit time-to-live, ended early by the backing store's own loss",
    degrade: "gives up any durability the injected store lacks — a lost backing is warm-up latency, never an error",
  },
  pooled: {
    row: "RcRef / RcMap",
    fits: "one scoped resource several holders share, at most one live instance per key",
    admit: "RcMap.get under a holder's scope",
    tenancy: "decides none — keys name a resource identity, and a tenant-scoped resource carries it in that key",
    lifetime: "the last holder's scope release, then the idle window",
    degrade: "gives up holder isolation — a poisoned instance serves every concurrent holder until invalidation",
  },
  bounded: {
    row: "CacheLane.origins — KeyedPool.makeWithTTL, leased through CacheLane.lease",
    fits: "expensive per-origin sessions reused across transfers under an exclusive lease",
    admit: "CacheLane.lease on an OriginKey the acquire dispatches",
    tenancy: "decides none — the origin key carries credentials, so separation follows the coordinate, not a tenant",
    lifetime: "the leasing scope for the item, the pool TTL for its residency",
    degrade: "gives up admission failure — a saturated origin queues callers past `max` rather than refusing them",
  },
  external: {
    row: "Valkey behind KeyValueStore",
    fits: "coherence across replicas, or invalidation fan-out no single process can serve",
    admit: "the same store assembly the persisted row composes, with its tenant prefix",
    tenancy: "interposes one — the same prefixed store assembly the persisted row composes",
    lifetime: "the engine's own key expiry, ended by eviction or engine loss",
    degrade: "gives up the memory read — every hit is a network hop, and an unreachable engine is a full recompute",
  },
  writeBehind: {
    row: "banned",
    fits: "-",
    admit: "-",
    tenancy: "-",
    lifetime: "-",
    degrade: "smuggles a durability obligation into a may-lose-data lane",
  },
  redisClient: {
    row: "banned",
    fits: "-",
    admit: "-",
    tenancy: "-",
    lifetime: "-",
    degrade: "buys nothing below the escalation trigger the native tier already covers",
  },
} as const

declare namespace CacheLane {
  type Tier = keyof typeof _tiers
  type _Rows<
    T extends Record<Tier, {
      readonly row: string
      readonly fits: string
      readonly admit: string
      readonly tenancy: string
      readonly lifetime: string
      readonly degrade: string
    }> = typeof _tiers,
  > = T
}
```

## [03]-[KEYED_FLIGHT]

- Owner: `CacheLane.memo` — the ONE memo entry whose modality is the input shape: a bare effect caches whole (`Effect.cached`, or `cachedWithTTL` when a cadence rides the call), a unary function memoizes per-argument (`Effect.cachedFunction` under the caller's own key equivalence); and `CacheLane.dedup(options)` — the request-cache Layer that turns fiber-tree request graphs into deduplicated batched loads. Keyed single-flight rides `Cache.make` at the package surface directly — a rename-forward alias adds no domain value and is refused here.
- Packages: `effect` (`Cache`, `Cache.ConsumerCache.cacheStats`, `Cache.makeCacheStats`, `Effect.cached`, `Effect.cachedWithTTL`, `Effect.cachedFunction`, `Equivalence`, `Request.makeCache`, `Layer.setRequestCache`, `Metric.incrementBy`, `Metric.set`, `Ref.getAndSet`, `Schedule.spaced`, `Duration`); `@rasm/core` (`Convention` — the cache instrument and name rows).
- Entry: a read surface with stampede exposure mints `Cache.make` once at construction and yields `cache.get(key)` thereafter; a pure recompute memoizes through `memo`; the projection and retrieval read paths compose `dedup` at the root so their `RequestResolver`-batched loads share one cache per request graph — the resolver machinery is the SQL tier's, the cache Layer is this row.
- Receipt: cache entries remain correctness-neutral values, and `CacheLane.census(name, cache)` is the economics projection — one scoped probe folding the substrate's own `cacheStats` snapshot onto the `cacheHits`/`cacheMisses` sums and the `cacheEntries` occupancy level, each tagged by the caller's cache name, so the `board#PACKS` `lake` hit-share tile reads a series a producer genuinely mints and residency reads beside it.
- Growth: a new cached surface is one mint with its own spec beside one `census` registration; a per-key TTL posture is the spec's `timeToLive` fold over the exit, never a second cache kind.
- Law: concurrent same-key lookups COLLAPSE to one execution — the single-flight guarantee is the constructor's, so no consumer wraps a semaphore around a cache; a hand-rolled in-flight map beside this row is the named reinvention.
- Law: capacity is a hard bound and eviction is the cache's own policy — an unbounded memo over unbounded keys is unspellable because `capacity` is a required field of the spec; `memo` over a function is bounded by its argument space and admits only where that space is provably small.
- Law: argument identity is the CALLER's to state — the substrate keys per-argument through `Equal.equals`, which answers reference identity for a plain object, so a memo over a plain record misses on every call and silently degrades to no cache at all. Arguments already carrying `Equal` — a `Data` value, a `Schema` class, a primitive — memoize structurally with no second argument; every other shape passes its own equivalence, and the parameter exists so the failure is a choice rather than an accident.
- Law: TTL is recompute cadence, not freshness truth — a consumer needing read-your-writes composes the reactive invalidation keys the journal stamps, never a shorter TTL.
- Law: the census READS the substrate's own snapshot and instruments no lookup — `cacheStats` answers hits, misses, and size in ONE read, so a hit costs no metric write on the hot path and a mirror tally beside the cache is the hand-rolled reimplementation the substrate already forecloses. Two wire forms ride that one read and the projection honours both: hits and misses are cumulative, so each monotonic sum advances by the delta against the prior sample the probe holds, while size is an instantaneous count the occupancy gauge sets outright. `Metric.set` is spellable on a gauge alone, so a snapshot total pushed onto a sum refuses at the type — and a fold reaching past that refusal re-adds the whole running total every tick and exports a hit share climbing quadratically off one honest cache.
- Law: the cache name is the caller's, because a cache is minted at its own read surface and the lane names none — the name keys the series exactly as the origin pool's scheme keys occupancy, and two caches sharing a name fold into one indistinguishable series at the board.
- Law: the census rides a scoped cadence, never the caller's fiber — one forked repeating probe per registered cache, released with the scope that registered it, so a lookup path never blocks on a metric read and a torn-down composition leaves no live probe.

```typescript signature
import { Cache, Duration, Effect, type Equivalence, Layer, Metric, Ref, Request, Schedule, type Scope } from "effect"
import { Convention } from "@rasm/core"

function _memo<B, E, R>(input: Effect.Effect<B, E, R>, ttl?: Duration.DurationInput): Effect.Effect<Effect.Effect<B, E, R>>
function _memo<A, B, E, R>(
  input: (a: A) => Effect.Effect<B, E, R>,
  key?: Equivalence.Equivalence<A>,
): Effect.Effect<(a: A) => Effect.Effect<B, E, R>>
function _memo(
  input: Effect.Effect<unknown> | ((a: unknown) => Effect.Effect<unknown>),
  policy?: Duration.DurationInput | Equivalence.Equivalence<unknown>,
) {
  return Effect.isEffect(input)
    ? policy === undefined ? Effect.cached(input) : Effect.cachedWithTTL(input, policy as Duration.DurationInput)
    : Effect.cachedFunction(input, policy as Equivalence.Equivalence<unknown> | undefined)
}

const _dedup = (options: { readonly capacity: number; readonly timeToLive: Duration.DurationInput }) =>
  Layer.setRequestCache(Request.makeCache(options))

const _CENSUS = { cadence: Duration.seconds(30) } as const

const _entries = Convention.mount(Convention.metric.cacheEntries)
const _hits = Convention.mount(Convention.metric.cacheHits)
const _misses = Convention.mount(Convention.metric.cacheMisses)

const _census = (name: string, cache: Cache.ConsumerCache<unknown, unknown, unknown>): Effect.Effect<void, never, Scope.Scope> =>
  Effect.forkScoped(
    Effect.flatMap(Ref.make(Cache.makeCacheStats({ hits: 0, misses: 0, size: 0 })), (sampled) =>
      Effect.repeat(
        Effect.flatMap(cache.cacheStats, (stats) =>
          Effect.flatMap(Ref.getAndSet(sampled, stats), (prior) =>
            Effect.all([
              Metric.incrementBy(Metric.tagged(_hits, Convention.rasm.cacheName, name), stats.hits - prior.hits),
              Metric.incrementBy(Metric.tagged(_misses, Convention.rasm.cacheName, name), stats.misses - prior.misses),
              Metric.set(Metric.tagged(_entries, Convention.rasm.cacheName, name), stats.size),
            ], { concurrency: "inherit", discard: true }))),
        Schedule.spaced(_CENSUS.cadence),
      )),
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

- Owner: resource rows — `RcRef.make` for one shared scoped resource, `RcMap.make` for keyed singletons, `CacheLane.origins`, the ONE bounded TTL pool keyed by structural origin, and `CacheLane.lease`, the ONE road out of that pool.
- Packages: `effect` (`RcRef.make`, `RcMap.make`, `RcMap.get`, `RcMap.invalidate`, `KeyedPool`, `KeyedPool.makeWithTTL`, `KeyedPool.get`, `Duration`, `Scope`, `Data`).
- Entry: the OLAP lane's engine instance and the per-scope warm surfaces ride `RcRef`/`RcMap` (`RcMap.get(map, key)` acquires-or-shares under the caller's `Scope`, `RcMap.invalidate(map, key)` evicts on rotation or poison); the remote-origin lane mints `origins(acquire, policy?)` keyed by the `Data`-classed `OriginKey` so structural equality pools connections, and every transfer takes its client through `lease(pool, key)` under the caller's `Scope`.
- Growth: a pool sizing posture is a policy-row override; a keyed family with complex identity keys by a `Data`-classed value, structural equality carried by construction; `OriginKey<Scheme>` carries the caller's closed scheme vocabulary beside the wire coordinate so one pool arbitrates every protocol's sessions and the remote plane's acquire dispatches on the key alone.
- Law: `RcMap` and `KeyedPool` divide by cardinality — `RcMap` shares ONE live instance per key among concurrent holders, `KeyedPool` holds up to N instances per key for exclusive leases; a protocol whose control connection carries one transfer at a time (the FTP law) is exactly why the origin row is the pool, never the map.
- Law: lifetime is reference-counted or pool-owned — release follows the last scope and idle window, so hot handles survive bursts without manual cleanup.
- Law: this row pools RESOURCES, the `Stores` map pools LAYERS — the tenancy store map stays the scope-family owner, and this lane's maps hold engine sessions and warm clients beneath it; the echo is deliberate, the owners distinct.
- Law: occupancy brackets the LEASE, never the acquire — a pool re-hands an idle item without re-running its acquire, so an acquire-time counter reports how many items the pool has ever built and left alive while every one of them sits idle, and a board reading it for held-connection pressure reads pool residency under an occupancy label. `lease` is therefore the one acquisition road: it increments the `Convention.metric.poolHeld` level tagged by the key's closed scheme vocabulary and decrements on the caller's own scope close, so the series counts exactly the transfers holding a connection right now. Non-monotonic form is what lets that decrement land; a monotonic row here exports a total that only climbs. Every pooled consumer takes this road and mounts no level of its own — the remote plane's origin sessions included — because one pool answering two series hands a board two held-connection numbers no reader can reconcile.

```typescript signature
import { Data, KeyedPool, Metric, type Scope } from "effect"
import { Convention } from "@rasm/core"

const _ORIGIN_POOL = { min: 0, max: 4, ttl: Duration.minutes(5) } as const

class OriginKey<Scheme extends string = string> extends Data.Class<{
  readonly scheme: Scheme
  readonly host: string
  readonly port: number
  readonly username: string
}> {}

const _held = Convention.mount(Convention.metric.poolHeld)

const _counted = (scheme: string) => Metric.tagged(_held, Convention.rasm.poolScheme, scheme)

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

const _lease = <Scheme extends string, A, E>(
  pool: KeyedPool.KeyedPool<OriginKey<Scheme>, A, E>,
  key: OriginKey<Scheme>,
): Effect.Effect<A, E, Scope.Scope> =>
  Effect.tap(KeyedPool.get(pool, key), () =>
    Effect.zipRight(
      Metric.incrementBy(_counted(key.scheme), 1),
      Effect.addFinalizer(() => Metric.incrementBy(_counted(key.scheme), -1)),
    ))
```

## [06]-[ASSEMBLY]

- Owner: `CacheLane`, the single export assembling the admitted tier, memo, persisted, store, backing, scope, origin-pool, and lease owners.
- Law: each projection seats at the owner holding its evidence — lease occupancy at `_lease`, the only bracket that knows a lease from a residency, and cache hits, misses, and residency at `_census`, the one reader of the substrate's own snapshot.

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
  lease: _lease,
} as const

// --- [EXPORTS] -------------------------------------------------------------------------

export { CacheLane, OriginKey }
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
