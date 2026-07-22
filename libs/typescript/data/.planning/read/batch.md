# [DATA_BATCH]

General request-batching engine: N identical lookups anywhere in a flow are one declared request family and one resolver — call sites stay singular, structural `Equal` over the request's fields deduplicates, and the window settles as one provider round trip. `read/query.md`'s `SqlResolver` rows are this engine fused with the SQL decode; this page owns the engine everywhere else: the object plane's HEAD coalescing, the capability probe scan, journal head probes, and every keyed provider call a sibling branch batches by passing these values.

A request family is one deep owner — the class carries its dedup identity in its fields AND its resolver mint, window upgrade, and provider seam as statics, so the family resolves from one name. Three window geometries ride one resolver value — same-traversal collapse, wall-clock collapse across unrelated fibers, and the durable result band — with the per-flow dedup tier `Effect.withRequestCaching` over `lane/cache.md`'s request-cache Layer. A resolver is built once and travels as a value: identity is the window, so a resolver minted per call site is the structural defeat.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                             |
| :-----: | :---------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `REQUEST_FAMILY`  | the request-class law — field identity, absorbed statics, the persistable upgrade  |
|  [02]   | `RESOLVER_ENGINE` | `Batch.Engine` and `Batch.of` — admitted policy, settlement, timing bracket        |
|  [03]   | `WINDOW_ROWS`     | the three window geometries and the per-flow caching tier                          |
|  [04]   | `SERVED_LANES`    | the folder's own batched lanes as rows over the closed geometry vocabulary         |

## [02]-[REQUEST_FAMILY]

- Owner: the request-declaration law — every batched lookup is a class extending `Request.TaggedClass("<tag>")<Success, Error, Fields>`, one name serving value, type, constructor, and dedup identity, with the family's resolver mint, window upgrade, and provider seam riding the class as statics; the folder's `Descriptor` family consumes `object/store.md`'s singular `head` member and owns bounded parallel settlement inside the resolver window — one head-lookup family whose geometry (traversal, wall-clock, persisted) is the `[4]` upgrade axis on one declaration, never a sibling family per geometry.
- Packages: `effect` (`Request`, `Schema`, `PrimaryKey`, `HashMap`, `Option`, `Array`, `Effect`).
- Growth: a new lookup kind in an existing family is one more tagged class sharing the family resolver through `RequestResolver.fromEffectTagged`; a new family is one class whose statics compose the `[3]` engine — never a loose resolver const orbiting an empty class.
- Law: the fields are exactly the identity — structural `Equal` over them is what collapses two requests for one key, so a field that varies without changing the answer (a caller label, a trace id) rides span annotation, never the request; success and failure types declare once at the family and no call site restates them.
- Law: the class absorbs its engine — `Descriptor.resolver(head)` mints the settle fold over the provider's singular member, `Descriptor.windowed(head)` upgrades it to wall-clock geometry, and `Descriptor.durable(head, policy)` composes the persisted band; provider plurality never leaks through the port, and each window executes under the admitted concurrency bound.
- Law: `Request.TaggedClass` is the family form and `Request.Class` is the admitted single-tag degenerate — a process-local family with exactly one member and no tagged-resolver fan (`lane/capability.md`'s `_Probe`) carries no `_tag` because nothing dispatches on it; the moment a second member or a `fromEffectTagged` handler arrives, the declaration upgrades to the tagged form.
- Law: persistence selects the declaration form once per family — a family any geometry persists is `Schema.TaggedRequest` (payload, success, and failure schemas in one declaration) satisfying `PrimaryKey`, so hits and misses both encode through the family's own schemas and a persisted failure replays typed; a family that never persists stays `Request.TaggedClass` at zero codec cost, and promotion rewrites only the declaration.
- Law: the request's fault is the family's — `Descriptor` maps a missing `head` to `DescriptorMiss` and every other fault to schema-owned `DescriptorFault`; every request settles independently, so one failed HEAD never poisons its siblings and persisted failures retain reason, key, and detail.
- Law: the success row is the provider page's schema owner — `Descriptor` answers `ObjectStore.Stat`, the store-minted evidence class, so the probe, the wall-clock window, and the durable band persist ONE row shape and a parallel success struct restating the stat fields is unspellable.
- Boundary: `Schema.TaggedRequest` declaration mechanics are the core shape law arriving settled; `lane/capability.md`'s `_Probe` stays its own realized family, and `object/store.md` hands the singular `ObjectStore.head` member across as a value — S3 has no batch HEAD operation to invent.

```typescript signature
import { Effect, Exit, Request, RequestResolver, Schema, type Scope } from "effect"
import { ContentKey } from "@rasm/ts/core"
import { type ObjectFault, ObjectStore } from "../object/store.ts"

class DescriptorMiss extends Schema.TaggedError<DescriptorMiss>()("DescriptorMiss", { key: ContentKey }) {}

class DescriptorFault extends Schema.TaggedError<DescriptorFault>()("DescriptorFault", {
  reason: Schema.Literal("missing", "integrity", "io"),
  key: ContentKey,
  detail: Schema.String,
}) {}

class Descriptor extends Schema.TaggedRequest<Descriptor>()("Descriptor", {
  payload: { key: ContentKey },
  success: ObjectStore.Stat, // the store-minted evidence class: probe, window, and durable band persist one row shape
  failure: Schema.Union(DescriptorMiss, DescriptorFault),
}) {
  static readonly resolver = (
    head: (key: ContentKey) => Effect.Effect<ObjectStore.Stat, ObjectFault>,
  ): RequestResolver.RequestResolver<Descriptor> =>
    _of(_ENGINE, (window) =>
      Effect.forEach(window, (request) =>
        Request.completeEffect(request, head(request.key).pipe(
          Effect.mapError((fault) =>
            fault.reason === "missing"
              ? new DescriptorMiss({ key: request.key })
              : new DescriptorFault({ reason: fault.reason, key: request.key, detail: fault.detail })),
        )), { concurrency: _ENGINE.width, discard: true }))
  static readonly windowed = (
    head: (key: ContentKey) => Effect.Effect<ObjectStore.Stat, ObjectFault>,
  ): Effect.Effect<RequestResolver.RequestResolver<Descriptor>, never, Scope.Scope> =>
    _windowed(Descriptor.resolver(head), _ENGINE)
  static readonly durable = (
    head: (key: ContentKey) => Effect.Effect<ObjectStore.Stat, ObjectFault>,
    policy: Batch.Persistence,
  ) =>
    _durable(Descriptor.resolver(head), {
      storeId: policy.storeId,
      timeToLive: (_request, exit) => (Exit.isSuccess(exit) ? policy.hit : policy.miss), // hits and misses age separately
    })
}
```

## [03]-[RESOLVER_ENGINE]

- Owner: `Batch.Engine` and `Batch.of(engine, settle)` — one admitted resolver policy and one mint over `RequestResolver.makeBatched` with the settlement law enforced in its shape — and the combinator tail every resolver composes: `batchN` width caps under the engine's refined field and the `aroundRequests` timing bracket; identity baking is `RequestResolver.contextFromServices` consumed at the package surface directly, because a forwarding wrapper adds no domain value and cannot state the variadic tag contract more honestly than the package signature.
- Packages: `effect` (`RequestResolver` — `makeBatched`, `fromEffectTagged`, `batchN`, `aroundRequests`, `contextFromServices`; `Request` — `completeEffect`, `succeed`, `fail`; `Clock`, `Schema`).
- Entry: the owning service mints its resolver once at construction and publishes `execute`-shaped members that close over it; a capability-consuming resolver bakes its services through `contextFromServices(...tags)` at the same construction, yielding the context-free, identity-stable value the window groups on.
- Receipt: the timing bracket's evidence pair — `aroundRequests`' `before` receives the window and its result feeds `after` — carries window size and wall span onto the span AND updates the `Convention.instrument.batchDuration` histogram, so batch efficiency is observable per window and queryable as a duration distribution with zero body wiring; name and description read off that instrument row, and the `Convention.duration` projection converts its millisecond span wherever an SLO or board consumes it.
- Growth: a resolver policy axis (width, bracket, context) is a combinator on the one value; a family growing a new tag lands a handler row on the `fromEffectTagged` record, never a sibling resolver.
- Law: the SQL-provider spans are catalogued vocabulary — every statement runs inside a `sql.execute` client span and every `SqlResolver` window inside its `sql.Resolver.batch <tag>` span — so board queries key on those names beside the `spanPrefix` attributes, never a re-derived spelling.
- Law: the bracket holds two `Clock` instants across separate `aroundRequests` hooks, so the histogram updates from their difference — `Metric.trackDuration` demands the window run as one wrappable effect the hook pair never holds.
- Law: every request settles — the batch body completes each request with `Request.completeEffect`/`succeed` per hit and `Request.fail` per miss, and a provider-level fault fans out to every request in the window; an unsettled request suspends its caller forever, so the settle-everything fold is the resolver's shape, not a discipline.
- Law: width and window are admitted once as `Batch.Engine` — the bounded-integer refinement and duration share the identity-rich policy owner, configuration decodes the class at the boot edge, and no loose width brand or window option bag can orbit the resolver; the engine's defaults are one sealed value of the same class.
- Law: identity is the window — batch windows group by resolver reference, so the resolver is built once and travels as a value; `Effect.provide` wrapped around call sites re-mints identity and defeats the window, which is what `contextFromServices` exists to prevent.
- Law: the tagged-family resolver answers positionally — `fromEffectTagged` hands each handler its tag's whole window and index `i` resolves request `i`, so family growth is a handler row.

```typescript signature
import { Array, Clock, Duration, Effect, Metric, MetricBoundaries, RequestResolver, Schema } from "effect"
import { Convention } from "@rasm/ts/core"

const _Width = Schema.Int.pipe(Schema.between(1, 1024), Schema.brand("BatchWidth"))

const _wall = Metric.histogram(
  Convention.instrument.batchDuration.name,
  MetricBoundaries.exponential({ start: 1, factor: 2, count: 12 }),
  Convention.instrument.batchDuration.description,
)

class _Engine extends Schema.Class<_Engine>("Batch.Engine")({
  width: _Width,
  window: Schema.DurationFromSelf,
}) {}

class _Persistence extends Schema.Class<_Persistence>("Batch.Persistence")({
  storeId: Schema.NonEmptyString,
  hit: Schema.DurationFromSelf,
  miss: Schema.DurationFromSelf,
}) {}

const _ENGINE = new _Engine({
  width: Schema.decodeSync(_Width)(64),
  window: Duration.millis(50),
})

declare namespace Batch {
  type Engine = _Engine
  type Persistence = _Persistence
  type Settle<Req extends Request.Request<unknown, unknown>, R> = (
    window: Array.NonEmptyArray<Req>,
  ) => Effect.Effect<void, never, R>
}

const _of = <Req extends Request.Request<unknown, unknown>, R>(
  engine: Batch.Engine,
  settle: Batch.Settle<Req, R>,
): RequestResolver.RequestResolver<Req, R> =>
  RequestResolver.makeBatched(settle).pipe(
    RequestResolver.batchN(engine.width),
    RequestResolver.aroundRequests(
      (window) => Effect.tap(Clock.currentTimeMillis, () => Effect.annotateCurrentSpan("batch.window", window.length)),
      (_, opened) =>
        Effect.flatMap(Clock.currentTimeMillis, (closed) =>
          Effect.zipRight(Metric.update(_wall, closed - opened), Effect.annotateCurrentSpan("batch.millis", closed - opened))),
    ),
  )
```

## [04]-[WINDOW_ROWS]

- Owner: the window-geometry vocabulary — `Batch.Geometry`, the closed union the `[5]` lane census and every upgrade member speak — with the three geometry rows over one resolver value and the per-flow caching tier that deduplicates repeated keys across a request graph.
- Packages: `@effect/experimental` (`RequestResolver.dataLoader`, `RequestResolver.persisted` — both `Function.dual`, data-first here); `effect` (`Effect.request`, `Effect.withRequestBatching`, `Effect.withRequestCaching`); `lane/cache.md` (`CacheLane.dedup` — the request-cache Layer; `CacheLane.backing` — the `Persistence` rows behind the durable band).
- Entry: call sites are `Effect.request(new Req({ ... }), resolver)` and stay singular; `Effect.forEach(keys, ..., { batching: true })` funnels a traversal into one window; `Batch.windowed(resolver, policy)` upgrades to the wall-clock collapse; `Batch.durable(resolver, policy)` upgrades to the persisted band.
- Growth: a lane moves between rows by swapping the wrapping combinator — the request family, the settle fold, and every call site hold.
- Law: geometry selection is collapse scope — `makeBatched` alone collapses one traversal; `dataLoader({ window, maxBatchSize })` trades that for a wall-clock window batching across unrelated fibers, a scoped acquisition over a context-free resolver; `persisted({ storeId, timeToLive })` adds the durable result band keyed by the request's schema identity, with `timeToLive` folding request and `Exit` so hits and misses age separately.
- Law: the durable band demands the persistable family — `persisted` composes only over `Schema.TaggedRequest` rows, and its `Persistence` backing is a Layer row from the cache lane's escalation table, memory in specs and the store-backed row where restart-survival is the requirement.
- Law: the durable band is tenant-partitioned by construction — a scope-owning composition interposes `CacheLane.scoped(scopeKey)` between the `Persistence` backing and its `KeyValueStore`, so two apps sharing one physical store cannot collide persisted results; an unprefixed shared band under a multi-app deployment is the named cross-tenant leak.
- Law: per-flow dedup is a transformer, never a map — `Effect.withRequestCaching(true)` scoped at the flow boundary deduplicates repeated keys across the graph, and the cache it consults is the `CacheLane.dedup` Layer composed once at the root; a hand `Map` of in-flight lookups beside a resolver is the reinvention this row deletes.

```typescript signature
import { RequestResolver as Experimental } from "@effect/experimental"
import { type Duration, type Exit, type Scope } from "effect"

const _GEOMETRY = ["traversal", "dataLoader", "persisted"] as const

const _PROVIDER = ["requestResolver", "sqlResolver"] as const

declare namespace Batch {
  type Geometry = (typeof _GEOMETRY)[number]
  type Provider = (typeof _PROVIDER)[number]
}

const _windowed = <Req extends Request.Request<unknown, unknown>>(
  resolver: RequestResolver.RequestResolver<Req>,
  policy: Batch.Engine,
): Effect.Effect<RequestResolver.RequestResolver<Req>, never, Scope.Scope> =>
  Experimental.dataLoader(resolver, { window: policy.window, maxBatchSize: policy.width })

const _durable = <Req extends Schema.TaggedRequest.All>(
  resolver: RequestResolver.RequestResolver<Req>,
  policy: {
    readonly storeId: string
    readonly timeToLive: (request: Req, exit: Exit.Exit<unknown, unknown>) => Duration.DurationInput
  },
) => Experimental.persisted(resolver, policy)
```

## [05]-[SERVED_LANES]

- Owner: the folder's own batched lanes, each one request family with orthogonal provider and geometry rows — the census of where the engine already earns its keep, kept as data so a new lane is a row; `provider` selects the general or SQL-specialized resolver and `window` speaks the `[4]` collapse geometry.
- Packages: composition only — each lane's provider members are its owning page's.
- Entry: each row names the family, provider, and window geometry; the owning page constructs the lane at its service build and this table is the cross-page map.
- Growth: a sibling branch batching a keyed provider call (an embedding window, a key-material fetch) declares its own family against this engine and appears in its own folder — the engine travels as these values, never as an import of provider surfaces.
- Law: `probe` is realized — `lane/capability.md`'s `_Probe` family folds the whole extension roster into one `pg_extension` scan under `{ batching: true }`; this table records it as the engine's in-folder proof, and the probe page stays the owner.
- Law: `presence` serves the object plane — `Descriptor.windowed` lifts `ObjectStore.head` into bounded parallel `HeadObjectCommand` sends under the wall-clock window, so a fan of probes settles one resolver window, repeated keys collapse structurally, and one provider fault remains local to its key.
- Law: `head` serves stream-position reads — a fan of per-stream `Journal.head` probes folds into one `GROUP BY` statement through the SQL specialization (`read/query.md`'s `StreamHead` `findById` row, where an eventless stream is a lawful `Option.none`), because where the provider is the database the fused resolver wins over the general engine.

```typescript signature
const _lanes = {
  probe: { family: "CapabilityProbe", provider: "requestResolver", window: "traversal", owner: "lane/capability" },
  presence: { family: "Descriptor", provider: "requestResolver", window: "dataLoader", owner: "object/store" },
  descriptor: { family: "Descriptor", provider: "requestResolver", window: "persisted", owner: "object/store" },
  head: { family: "StreamHead", provider: "sqlResolver", window: "traversal", owner: "read/query" },
} as const

declare namespace Batch {
  type Lane = keyof typeof _lanes
  type _Rows<T extends Record<Lane, {
    readonly family: string
    readonly provider: Provider
    readonly window: Geometry
    readonly owner: string
  }> = typeof _lanes> = T
}

const Batch = {
  Engine: _Engine,
  Persistence: _Persistence,
  defaults: _ENGINE,
  geometries: _GEOMETRY,
  providers: _PROVIDER,
  lanes: _lanes,
  of: _of,
  tagged: RequestResolver.fromEffectTagged,
  windowed: _windowed,
  durable: _durable,
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Batch, Descriptor, DescriptorFault, DescriptorMiss }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
