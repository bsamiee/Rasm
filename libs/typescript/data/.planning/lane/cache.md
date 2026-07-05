# [DATA_CACHE]

The latency lane, correctness-neutral by law: a cache node vanishing costs a cold recompute and nothing else, so this page owns the Tier-0 in-process rows the runtime already supplies — keyed single-flight caches, fiber-tree request deduplication, restart-surviving persisted lookups, and reference-counted resource pools — plus the one escalation law that gates any external engine. Stampede protection, TTL, and memoization are native; an external cache engine is warranted ONLY when the guarantee must cross a process boundary, and that gated row is Valkey behind the same `KeyValueStore` port, so escalation is a Layer swap, never a code path. Write-behind is banned from the lane outright — a cache that owes durability is a correctness surface misfiled, and the journal owns those.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                            |
| :-----: | :--------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `TIER_ROWS`      | the escalation table — Tier-0 rows, the gated external row, the banned policies       |
|  [02]   | `KEYED_FLIGHT`   | the single-flight keyed cache mint and the request-dedup plane                        |
|  [03]   | `PERSISTED`      | the restart-surviving cache over the KeyValueStore port                               |
|  [04]   | `POOLS`          | reference-counted single handles and keyed resource maps                              |

## [2]-[TIER_ROWS]

- Owner: the `_tiers` anchor — every caching posture the folder admits as a row with its boundary and its trigger; the two banned rows carry their refusal as data so the argument is never re-had.
- Packages: none — decision facts.
- Growth: a new posture is one row; an external engine admission is the ONE gated row flipping from `gated` to a composed Layer at the root, with zero consumer edits because every consumer reads the port.
- Law: the lane guarantee is latency reduction, never correctness — every row's failure mode is a recompute; a consumer that breaks when a cache empties has smuggled state into the lane and is the named defect.
- Law: the escalation trigger is a process boundary — shared coherence across replicas or pub/sub invalidation fan-out; in-process needs are already covered, so reaching for an external engine below the trigger is refused by the table.
- Law: `writeBehind` is banned — it smuggles a durability obligation into a may-lose-data lane; a buffered durable write is the journal's outbox, full stop. `redisClient` is banned — the runtime natively supplies single-flight, TTL, and dedup, so the client footprint buys nothing below the escalation trigger; when the trigger fires, the engine row is Valkey behind the existing port.

```typescript
const _tiers = {
  memo: { row: "Effect.cachedFunction / cachedWithTTL", boundary: "in-process", trigger: "pure recompute avoidance" },
  keyed: { row: "Cache.make", boundary: "in-process, capacity-bounded", trigger: "keyed lookups with stampede risk" },
  request: { row: "Request.makeCache + Layer.setRequestCache", boundary: "fiber tree", trigger: "N+1 dedup across one request graph" },
  persisted: { row: "PersistedCache.make over Persistence", boundary: "restart-surviving, single node", trigger: "expensive lookups worth keeping warm" },
  pooled: { row: "RcRef / RcMap", boundary: "in-process resource lifetime", trigger: "shared scoped resources" },
  external: { row: "Valkey behind KeyValueStore", boundary: "cross-process", trigger: "GATED — replica coherence or invalidation fan-out only" },
  writeBehind: { row: "banned", boundary: "-", trigger: "durability smuggled into the latency lane" },
  redisClient: { row: "banned", boundary: "-", trigger: "native tier already owns single-flight/TTL/dedup" },
} as const

declare namespace CacheLane {
  type Tier = keyof typeof _tiers
  type _Rows<T extends Record<Tier, { readonly row: string; readonly boundary: string; readonly trigger: string }> = typeof _tiers> = T
}
```

## [3]-[KEYED_FLIGHT]

- Owner: `CacheLane.keyed` — `Cache.make` published as the lane's keyed row, capacity- and TTL-bounded, whose lookup is the caller's effect, taken at the package surface with zero wrapper; and `CacheLane.dedup(options)` — the request-cache Layer that turns fiber-tree request graphs into deduplicated batched loads.
- Packages: `effect` (`Cache`, `Request`, `Layer`, `Duration`, `Effect`).
- Entry: a read surface with stampede exposure mints `keyed` once at construction and yields `cache.get(key)` thereafter; the projection and retrieval read paths compose `dedup` at the root so their `RequestResolver`-batched loads share one cache per request graph — the resolver machinery is the SQL tier's, the cache Layer is this row.
- Receipt: `cacheStats` — hits, misses, size — read on demand and emitted through the fact stream's meter row by the composition that owns the cache; the lane never self-reports.
- Growth: a new cached surface is one `keyed` mint with its own spec; a per-key TTL posture is the spec's `timeToLive` fold over the exit, never a second cache kind.
- Law: concurrent same-key lookups COLLAPSE to one execution — the single-flight guarantee is the constructor's, so no consumer wraps a semaphore around a cache; a hand-rolled in-flight map beside this row is the named reinvention.
- Law: capacity is a hard bound and eviction is the cache's own policy — an unbounded memo over unbounded keys is unspellable because `capacity` is a required field of the spec.
- Law: TTL is recompute cadence, not freshness truth — a consumer needing read-your-writes composes the reactive invalidation keys the journal stamps, never a shorter TTL.

```typescript
import { Cache, type Duration, Layer, Request } from "effect"

const _dedup = (options: { readonly capacity: number; readonly timeToLive: Duration.DurationInput }) =>
  Layer.setRequestCache(Request.makeCache(options))
```

## [4]-[PERSISTED]

- Owner: `CacheLane.persisted` — `PersistedCache.make` published as the restart-surviving row, backed by the `Persistence` result store, which itself rides the `KeyValueStore` port — memory in specs, filesystem on a single node, and the gated external engine when the escalation trigger fires, all by Layer swap.
- Packages: `@effect/experimental` (`PersistedCache.make`, `Persistence.layerResultKeyValueStore`, `Persistence.layerResultMemory`); `@effect/platform` (`KeyValueStore.layerMemory`, `KeyValueStore.layerFileSystem`); `effect` (`Duration`, `Layer`).
- Entry: an expensive schema-keyed lookup (a rendered derivative index, a resolved capability report) mints `persisted` with its request schema; the backing Layer composes at the root per deployment posture.
- Growth: a new backing is one Layer row behind the same port; the gated Valkey admission is exactly this — a `KeyValueStore` implementation over the external engine composed at the root, with this page unchanged.
- Law: durability equals the injected store's — the cache never promises more than its backing; a persisted entry lost with its node is a recompute, which is the lane's lawful failure mode.
- Law: the persisted cache is an overlay, never a record of truth — nothing reads it as authority, and dropping the backing store costs warm-up latency only; the journal boundary law of the folder applies unchanged.
- Law: keys are schema-typed persistables — the request schema owns identity and serialization, so a cache key is never a hand-built string and a shape change invalidates structurally.

```typescript
import { Persistence, PersistedCache } from "@effect/experimental"
import { KeyValueStore } from "@effect/platform"

const _backing = {
  memory: Persistence.layerResultMemory,
  keyValue: Persistence.layerResultKeyValueStore,
  storeMemory: KeyValueStore.layerMemory,
  storeFile: (directory: string) => KeyValueStore.layerFileSystem(directory),
} as const
```

## [5]-[POOLS]

- Owner: the reference-counted resource rows — `CacheLane.handle` (`RcRef.make`) for one shared scoped resource (an engine instance, a warmed client) and `CacheLane.map` (`RcMap.make`) for keyed resource families (per-key sessions, per-tenant warm handles), both releasing on last-reference exit with idle expiry, both the package surface directly.
- Packages: `effect` (`RcRef.make`, `RcMap.make`, `RcMap.get`, `RcMap.invalidate`, `Duration`, `Scope`).
- Entry: the OLAP lane's engine instance and the per-scope warm surfaces ride these rows; `RcMap.get(map, key)` acquires-or-shares under the caller's `Scope`, and `RcMap.invalidate(map, key)` is the targeted eviction on rotation or poison.
- Growth: a pool sizing posture is the spec's `capacity`/`idleTimeToLive` fields; a keyed family with complex identity keys by a `Data`-classed value, structural equality carried by construction.
- Law: lifetime is reference-counted, never manual — the resource releases when the last scope closes plus the idle window, so a leak is unspellable and a hot handle survives bursts without churn.
- Law: this row pools RESOURCES, the `Stores` map pools LAYERS — the tenancy store map stays the scope-family owner, and this lane's maps hold engine sessions and warm clients beneath it; the echo is deliberate, the owners distinct.

```typescript
import { RcMap, RcRef } from "effect"

const CacheLane = {
  tiers: _tiers,
  keyed: Cache.make,
  dedup: _dedup,
  persisted: PersistedCache.make,
  backing: _backing,
  handle: RcRef.make,
  map: RcMap.make,
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { CacheLane }
```
