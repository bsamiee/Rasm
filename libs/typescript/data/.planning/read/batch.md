# [DATA_BATCH]

The general request-batching engine: N identical lookups anywhere in a flow are one declared request family and one resolver, and the collapse is structural — call sites stay singular, structural `Equal` over the request's fields deduplicates, and the window settles as one provider round trip. `read/query.md`'s `SqlResolver` rows are this engine fused with the SQL decode; this page owns the engine everywhere else: the object plane's HEAD coalescing, the capability probe scan, journal head probes, and every keyed provider call a sibling branch batches by passing these values. Three window geometries ride one resolver value — same-traversal collapse (`makeBatched` under `{ batching: true }`), wall-clock collapse across unrelated fibers (`dataLoader`), and the durable result band (`persisted`) whose hits survive restart — and the per-flow dedup tier is `Effect.withRequestCaching` over `lane/cache.md`'s request-cache Layer. A resolver is built once and travels as a value: identity is the window, so a resolver minted per call site is the structural defeat this page makes unspellable.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                            |
| :-----: | :--------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `REQUEST_FAMILY` | the request-class law — field identity, the tagged family, the persistable upgrade     |
|  [02]   | `RESOLVER_ENGINE`| `Batch.of` — settle-every-request, width caps, the timing bracket, baked context       |
|  [03]   | `WINDOW_ROWS`    | the three window geometries and the per-flow caching tier                              |
|  [04]   | `SERVED_LANES`   | the folder's own batched lanes as rows — HEAD coalescing, probes, head reads           |

## [2]-[REQUEST_FAMILY]

- Owner: the request-declaration law — every batched lookup is a class extending `Request.TaggedClass("<tag>")<Success, Error, Fields>`, one name serving value, type, constructor, and dedup identity; the class absorbs its resolver and windowed consumers as statics so the family cannot scatter.
- Packages: `effect` (`Request`, `Schema`, `PrimaryKey`).
- Growth: a new lookup kind in an existing family is one more tagged class sharing the family resolver through `RequestResolver.fromEffectTagged`; a new family is one class plus one resolver value.
- Law: the fields are exactly the identity — structural `Equal` over them is what collapses two requests for one key, so a field that varies without changing the answer (a caller label, a trace id) rides span annotation, never the request; success and failure types declare once at the family and no call site restates them.
- Law: `Request.TaggedClass` is the family form and `Request.Class` is the admitted single-tag degenerate — a process-local family with exactly one member and no tagged-resolver fan (`lane/capability.md`'s `_Probe`) carries no `_tag` because nothing dispatches on it; the moment a second member or a `fromEffectTagged` handler arrives, the declaration upgrades to the tagged form.
- Law: the persisted upgrade is a declaration swap — a family whose results must survive restart is `Schema.TaggedRequest` (payload, success, and failure schemas in one declaration) satisfying `PrimaryKey`, so hits and misses both encode through the family's own schemas and a persisted failure replays typed; the in-memory family stays `Request.TaggedClass` at zero codec cost, and promotion rewrites only the declaration.
- Law: the request's fault is the family's — a provider fault fans out to every request in the window as the same tagged class, so a batched miss and a singular miss are indistinguishable to recovery, which is the point.
- Boundary: `Schema.TaggedRequest` declaration mechanics are the core shape law arriving settled; `lane/capability.md`'s `_Probe` is a realized instance of this family law and stays where it is.

```typescript
import { Request, Schema } from "effect"
import { ContentKey } from "@rasm/ts/core"
import type { ObjectFault } from "../object/store.ts"

class Presence extends Request.TaggedClass("Presence")<
  { readonly key: ContentKey; readonly bytes: number; readonly etag: string },
  ObjectFault,
  { readonly key: ContentKey }
> {}

class Descriptor extends Schema.TaggedRequest<Descriptor>()("Descriptor", {
  payload: { key: ContentKey },
  success: Schema.Struct({ key: ContentKey, bytes: Schema.Number, contentType: Schema.String }),
  failure: Schema.TaggedStruct("DescriptorMiss", { key: ContentKey }),
}) {}
```

## [3]-[RESOLVER_ENGINE]

- Owner: `Batch.of(settle)` — the one resolver mint over `RequestResolver.makeBatched` with the settlement law enforced in its shape — plus the combinator tail every resolver composes: `batchN` width caps and the `aroundRequests` timing bracket; identity baking is `RequestResolver.contextFromServices` consumed at the package surface directly, because a forwarding wrapper adds no domain value and cannot state the variadic tag contract more honestly than the package signature.
- Packages: `effect` (`RequestResolver` — `makeBatched`, `fromEffectTagged`, `batchN`, `aroundRequests`, `contextFromServices`; `Request` — `completeEffect`, `succeed`, `fail`; `Clock`).
- Entry: the owning service mints its resolver once at construction and publishes `execute`-shaped members that close over it; a capability-consuming resolver bakes its services through `contextFromServices(...tags)` at the same construction, yielding the context-free, identity-stable value the window groups on.
- Receipt: the timing bracket's evidence pair — `aroundRequests`' `before` receives the window and its result feeds `after` — carries window size and wall span onto the span, so batch efficiency is observable per window with zero body wiring.
- Growth: a resolver policy axis (width, bracket, context) is a combinator on the one value; a family growing a new tag lands a handler row on the `fromEffectTagged` record, never a sibling resolver.
- Law: every request settles — the batch body completes each request with `Request.completeEffect`/`succeed` per hit and `Request.fail` per miss, and a provider-level fault fans out to every request in the window; an unsettled request suspends its caller forever, so the settle-everything fold is the resolver's shape, not a discipline.
- Law: identity is the window — batch windows group by resolver reference, so the resolver is built once and travels as a value; `Effect.provide` wrapped around call sites re-mints identity and defeats the window, which is what `contextFromServices` exists to prevent.
- Law: the tagged-family resolver answers positionally — `fromEffectTagged` hands each handler its tag's whole window and index `i` resolves request `i`, so family growth is a handler row.

```typescript
import { Array, Clock, Effect, RequestResolver } from "effect"

declare namespace Batch {
  type Settle<Req extends Request.Request<unknown, unknown>, R> = (
    window: Array.NonEmptyArray<Req>,
  ) => Effect.Effect<void, never, R>
}

const _of = <Req extends Request.Request<unknown, unknown>, R>(
  width: number,
  settle: Batch.Settle<Req, R>,
): RequestResolver.RequestResolver<Req, R> =>
  RequestResolver.makeBatched(settle).pipe(
    RequestResolver.batchN(width),
    RequestResolver.aroundRequests(
      (window) => Effect.zipLeft(Clock.currentTimeMillis, Effect.annotateCurrentSpan("batch.window", window.length)),
      (_, opened) =>
        Effect.flatMap(Clock.currentTimeMillis, (closed) => Effect.annotateCurrentSpan("batch.millis", closed - opened)),
    ),
  )
```

## [4]-[WINDOW_ROWS]

- Owner: the window-geometry table — same-traversal, wall-clock, and durable rows over one resolver value — and the per-flow caching tier that deduplicates repeated keys across a request graph.
- Packages: `@effect/experimental` (`RequestResolver.dataLoader`, `RequestResolver.persisted`); `effect` (`Effect.request`, `Effect.withRequestBatching`, `Effect.withRequestCaching`); `lane/cache.md` (`CacheLane.dedup` — the request-cache Layer; `CacheLane.backing` — the `Persistence` rows behind the durable band).
- Entry: call sites are `Effect.request(new Req({ ... }), resolver)` and stay singular; `Effect.forEach(keys, ..., { batching: true })` funnels a traversal into one window; `Batch.windowed(resolver, policy)` upgrades to the wall-clock collapse; `Batch.durable(resolver, policy)` upgrades to the persisted band.
- Growth: a lane moves between rows by swapping the wrapping combinator — the request family, the settle fold, and every call site hold.
- Law: geometry selection is collapse scope — `makeBatched` alone collapses one traversal; `dataLoader({ window, maxBatchSize })` trades that for a wall-clock window batching across unrelated fibers, a scoped acquisition over a context-free resolver; `persisted({ storeId, timeToLive })` adds the durable result band keyed by the request's schema identity, with `timeToLive` folding request and `Exit` so hits and misses age separately.
- Law: the durable band demands the persistable family — `persisted` composes only over `Schema.TaggedRequest` rows, and its `Persistence` backing is a Layer row from the cache lane's escalation table, memory in specs and the store-backed row where restart-survival is the requirement.
- Law: the durable band is tenant-partitioned by construction — a scope-owning composition interposes `CacheLane.scoped(scopeKey)` between the `Persistence` backing and its `KeyValueStore`, so two apps sharing one physical store cannot collide persisted results; an unprefixed shared band under a multi-app deployment is the named cross-tenant leak.
- Law: per-flow dedup is a transformer, never a map — `Effect.withRequestCaching(true)` scoped at the flow boundary deduplicates repeated keys across the graph, and the cache it consults is the `CacheLane.dedup` Layer composed once at the root; a hand `Map` of in-flight lookups beside a resolver is the reinvention this row deletes.

```typescript
import { RequestResolver as Experimental } from "@effect/experimental"
import { type Duration, type Exit, type Scope } from "effect"

const _windowed = <Req extends Request.Request<unknown, unknown>>(
  resolver: RequestResolver.RequestResolver<Req>,
  policy: { readonly window: Duration.DurationInput; readonly maxBatchSize: number },
): Effect.Effect<RequestResolver.RequestResolver<Req>, never, Scope.Scope> =>
  Experimental.dataLoader(resolver, policy)

const _durable = <Req extends Schema.TaggedRequest.All>(
  resolver: RequestResolver.RequestResolver<Req>,
  policy: {
    readonly storeId: string
    readonly timeToLive: (request: Req, exit: Exit.Exit<unknown, unknown>) => Duration.DurationInput
  },
) => Experimental.persisted(resolver, policy)
```

## [5]-[SERVED_LANES]

- Owner: the folder's own batched lanes, each one request family plus one window row — the census of where the engine already earns its keep, kept as data so a new lane is a row.
- Packages: composition only — each lane's provider members are its owning page's.
- Entry: each row names the family, the settle statement, and the window geometry; the owning page constructs the lane at its service build and this table is the cross-page map.
- Growth: a sibling branch batching a keyed provider call (an embedding window, a key-material fetch) declares its own family against this engine and appears in its own folder — the engine travels as these values, never as an import of provider surfaces.
- Law: `probe` is realized — `lane/capability.md`'s `_Probe` family folds the whole extension roster into one `pg_extension` scan under `{ batching: true }`; this table records it as the engine's in-folder proof, and the probe page stays the owner.
- Law: `presence` serves the object plane — `object/store.md` completes each `Presence` request from one windowed sweep of `HeadObjectCommand` sends under bounded concurrency, so a fan of existence probes against one bucket costs one window of HEADs with per-key settlement, and repeated keys inside a flow cost one.
- Law: `head` serves stream-position reads — a fan of per-stream `Journal.head` probes folds into one `GROUP BY` statement through the SQL specialization (`read/query.md`'s `StreamHead` `findById` row, where an eventless stream is a lawful `Option.none`), because where the provider is the database the fused resolver wins over the general engine.

```typescript
const _lanes = {
  probe: { family: "CapabilityProbe", window: "traversal", owner: "lane/capability" },
  presence: { family: "Presence", window: "dataLoader", owner: "object/store" },
  descriptor: { family: "Descriptor", window: "persisted", owner: "object/store" },
  head: { family: "StreamHead", window: "sqlFindById", owner: "read/query" },
} as const

declare namespace Batch {
  type Lane = keyof typeof _lanes
  type _Rows<T extends Record<Lane, { readonly family: string; readonly window: string; readonly owner: string }> = typeof _lanes> = T
}

const Batch = {
  lanes: _lanes,
  of: _of,
  tagged: RequestResolver.fromEffectTagged,
  windowed: _windowed,
  durable: _durable,
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Batch, Descriptor, Presence }
```
