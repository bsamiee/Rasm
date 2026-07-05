# [TS_BRANCH_API_EFFECT]

`effect` is the branch standard library and the one runtime substrate every `libs/typescript` folder types against: the `Effect<A, E, R>` carrier (typed success, typed error channel, typed requirement context), `Schema` (parse-not-validate decode/encode with derivation to `Arbitrary`/`JSONSchema`/`Pretty`/`Equivalence`), `Context`/`Layer`/`Runtime` (dependency injection and composition roots), `Match`/`Data` (tagged-union dispatch and closed families), `Stream`/`Sink`/`Channel` (pull-based backpressured streaming), the concurrency substrate (`Fiber`, `Ref`/`SubscriptionRef`, `Deferred`, `Queue`/`PubSub`/`Mailbox`, `Semaphore`, `STM`), `Schedule` (recurrence algebra for retry/repeat), the observability trio (`Metric`, `Logger`, `Tracer`), `Config`/`ConfigProvider` (validated environment ingress), the immutable collection family (`Chunk`/`HashMap`/`HashSet`/`Array`/`Record`/`SortedMap`), the caching and batching surface (`Cache`, `RcMap`, `Pool`, `RateLimiter`, `Request`/`RequestResolver`), and the interop primitives (`Option`, `Either`, `Cause`, `Exit`, `Redacted`, `Encoding`, `Duration`, `DateTime`). It is isomorphic and dependency-free — one carrier that composes monadically for dependent steps and applicatively for independent accumulation, with cross-cutting capability (decode, span, retry, lifetime) attaching at the seam through effect transformers rather than being threaded by hand.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `effect`
- package: `effect` (3.21.4, MIT, © Effectful Technologies)
- module format: ESM + CJS dual (`dist/esm` + `dist/cjs`, types `dist/dts`), `sideEffects: []`; per-module deep-import subpaths (`effect/Effect`, `effect/Schema`, …) plus the flat `effect` barrel — the module boundary graph imports the barrel, deep-imports only where tree-shaking a single module matters
- runtime target: isomorphic (node, bun, browser, worker); zero runtime dependencies; no native addon
- asset: pure-TypeScript runtime library shipping `.js` + `.d.ts`; the type-level surface (`typeof schema.Type`, `keyof typeof`, branded refinements) is load-bearing, so `tsc`/`tsgo` are the real gate
- rail: substrate (every folder types against it; catalogued once at the branch tier)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the carrier, its data siblings, and failure algebra
- rail: rails-and-effects

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY]        | [CONSUMER]                                                             |
| :-----: | :------------------------------------------------ | :------------------- | :-------------------------------------------------------------------- |
|  [01]   | `Effect<A, E, R>`                                 | carrier              | every folder — the one rail; `R` is the Tag requirement set the app root satisfies |
|  [02]   | `Option<A>` / `Either<R, L>`                      | value union          | `kernel` absence, decode results; `Option` replaces `null`/`undefined` in domain |
|  [03]   | `Cause<E>` / `Exit<A, E>`                         | failure tree / outcome | `wire/fault` reconstruction, `otel/crash` — retains defect + interrupt + parallel failures |
|  [04]   | `Data.TaggedEnum<...>` / `Data.Class`             | closed family        | `state` CRDT ops, `wire` decoded unions, `edge` control intents — value equality by structure |
|  [05]   | `Redacted<A>`                                     | secret carrier       | `security/secret`, `proc/config` — never logged; `Redacted.value` unwraps at the crypto seam only |
|  [06]   | `Duration` / `DateTime.Utc` / `DateTime.Zoned`    | time value           | `kernel/clock`, `work/queue`, `store/journal` — monotonic + wall-clock evidence |
|  [07]   | `Brand.Branded<A, K>`                             | nominal refinement   | `kernel/schema` brand floor (`ContentKey`, `Hlc`, `OrdinalKey`) — decode-once type identity |
|  [08]   | `Scope` / `Fiber<A, E>`                           | resource / handle    | `proc/life`, `work/engine`, `browser/boot` — structured lifetime and interruptible child fibers |

[PUBLIC_TYPE_SCOPE]: schema, its derivations, and boundary shapes
- rail: boundaries

| [INDEX] | [SYMBOL]                                                  | [TYPE_FAMILY]      | [CONSUMER]                                                       |
| :-----: | :-------------------------------------------------------- | :----------------- | :-------------------------------------------------------------- |
|  [01]   | `Schema.Schema<Type, Encoded, R>`                         | codec              | every boundary — one Schema owns decode, encode, and every derived surface |
|  [02]   | `Schema.Struct` / `Schema.Class` / `Schema.TaggedClass`   | record family      | `store/journal` events (`Schema.TaggedClass` + `eventVersion`), `kernel` value objects |
|  [03]   | `Schema.TaggedError` / `Schema.TaggedRequest`             | fault / request    | `wire/fault` decoded errors, `store`/`work` request schemas with success+failure channels |
|  [04]   | `Schema.PropertySignature` / `Schema.optionalWith`        | field modality     | `kernel/schema` optional-to-`Option` decode, constructor defaults, key renaming at the seam |
|  [05]   | `ParseResult.ParseError` / `ParseResult.ParseIssue`       | decode fault       | every ingress — lifts into the `Effect` error channel; `ArrayFormatter` renders issue trees |
|  [06]   | `SchemaAST.AST` + annotation IDs                          | reflection node    | the testkit arbitrary derivation (`tests/typescript/_testkit`), `edge/api` OpenAPI emission read the annotated AST |
|  [07]   | `FastCheck.Arbitrary<A>` (`effect/FastCheck`) / `Arbitrary.LazyArbitrary<A>` | generator | the testkit arbitrary source (`tests/typescript/_testkit`) — Schema-derived property generators, no hand-rolled fixtures; `LazyArbitrary<A>` = `(fc: typeof FastCheck) => FastCheck.Arbitrary<A>`, the deferred/recursive-schema form |

[PUBLIC_TYPE_SCOPE]: services, layers, and dispatch surfaces
- rail: surfaces-and-dispatch

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]     | [CONSUMER]                                                          |
| :-----: | :---------------------------------------------- | :---------------- | :----------------------------------------------------------------- |
|  [01]   | `Context.Tag<Id, Service>` / `Context.TagClass` / `Context.Reference`| service key        | every port (`SqlClient`, `Embedder`, `SessionStore`); `class X extends Context.Tag("id")<X, S>()` mints a `TagClass` — the form `ai`/`store` service Tags carry; `Reference` carries a default |
|  [02]   | `Layer<ROut, E, RIn>`                           | wiring             | app composition roots — folders ship `Layer` families the thin `main.ts` selects |
|  [03]   | `LayerMap.Service` / `LayerMap`                 | keyed layer cache | `store/scope` per-tenant `StoreHandle` Layers keyed `(appKey, tenancy)` — isolation as a scope value |
|  [04]   | `ManagedRuntime<R, E>`                          | runtime root      | `browser/boot`, `proc/exec` — a built runtime the imperative host edge calls into |
|  [05]   | `Match.Matcher` (`Match.type`/`Match.value`)    | dispatch builder  | `wire` codec dispatch, `iac/provider` closed-arm selection, `edge` fault mapping |
|  [06]   | `Metric.Metric` / `Logger.Logger` / `Tracer.Span` | signal owner     | `telemetry` — counters/histograms, structured loggers, spans composed onto the rail |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Effect` carrier — construction, boundary lift, and running
- rail: rails-and-effects

| [INDEX] | [SURFACE]                                                                                   | [ENTRY_FAMILY]     | [CONSUMER]                                                        |
| :-----: | :------------------------------------------------------------------------------------------ | :----------------- | :--------------------------------------------------------------- |
|  [01]   | `Effect.succeed` / `Effect.fail` / `Effect.sync` / `Effect.suspend`                         | construct          | pure values and lazy thunks into the rail; `fail` seeds the typed error channel |
|  [02]   | `Effect.gen(function*(){ … })` / `Effect.fn("span")(body, …pipeline)`                        | do-notation        | every domain flow; `Effect.fn` is the traced-body seam — body per attempt, pipeline carries resilience |
|  [03]   | `Effect.tryPromise({ try, catch })` / `Effect.try` / `Effect.async` / `Effect.promise`      | boundary lift      | `BOUNDARY_ADMISSION` — a `Promise`/throw converts to a typed error at the owning seam |
|  [04]   | `Effect.all(effects, { concurrency, mode })` / `Effect.forEach` / `Effect.validateAll`      | applicative        | independent operands accumulate; `mode: "validate"` collects every failure, not just the first |
|  [05]   | `Effect.flatMap` / `Effect.andThen` / `Effect.zipWith` / `Effect.tap`                        | sequence           | dependent steps; `andThen` accepts value, `Effect`, or thunk — one combinator, many operands |
|  [06]   | `Effect.catchTag` / `Effect.catchTags` / `Effect.catchAll` / `Effect.tapErrorTag`           | recover            | `wire`/`store` typed recovery by `_tag`; exhaustive over the tagged error family |
|  [07]   | `Effect.retry(policy)` / `Effect.timeout` / `Effect.withExecutionPlan` / `Effect.race`      | resilience         | `net/client`, `ai/model`, `work/activity` — `ExecutionPlan` is multi-provider fallback with per-tier schedules |
|  [08]   | `Effect.acquireRelease` / `Effect.acquireUseRelease` / `Effect.addFinalizer` / `Effect.scoped` | resource        | `proc/life`, `store` connections, `work` leases — release runs on success, failure, and interrupt |
|  [09]   | `Effect.fork` / `Effect.forkScoped` / `Effect.forkDaemon` / `Effect.interrupt`              | concurrency        | `edge/live` subscriptions, `browser/transport` workers — scoped forks die with their scope |
|  [10]   | `Effect.provide` / `Effect.provideService` / `Effect.updateService`                         | context supply     | app root injects `Layer`/service; `provideService` for a single Tag at a call site |
|  [11]   | `Effect.withSpan` / `Effect.annotateLogs` / `Effect.withMetric` / `Effect.log*`             | observability seam | `telemetry` — `DEFINITION_TIME_ASPECTS` attach span/log/metric onto the rail, recoverable from declaration |
|  [12]   | `Effect.runFork` / `Effect.runPromise` / `Effect.runSync` / `Effect.runPromiseExit`         | run                | the one imperative edge — `runFork` for long-lived roots, `runPromiseExit` for typed outcome |

[ENTRYPOINT_SCOPE]: `Schema` — decode, project, transform, derive
- rail: boundaries

| [INDEX] | [SURFACE]                                                                                        | [ENTRY_FAMILY]  | [CONSUMER]                                                     |
| :-----: | :----------------------------------------------------------------------------------------------- | :-------------- | :------------------------------------------------------------ |
|  [01]   | `Schema.decode(schema)` / `Schema.decodeUnknown(schema)` / `Schema.decodeUnknownEither` / `Schema.encode` | decode / encode | `decodeUnknown` decodes an `unknown` ingress once; `decode` takes an already-`Encoded` typed input `I` — a post-codec value (the `hash-wasm` hex `string` → `ContentKey` mint); `encode` at explicit egress; `ParseError` lifts to the error channel |
|  [02]   | `Schema.Struct({...})` / `Schema.TaggedStruct(tag, {...})` / `Schema.Class<Self>(id)({...})`     | declare shape   | `SHAPE_BUDGET` — one runtime authority per concept; `Class` adds an opaque constructor + prototype |
|  [03]   | `Schema.Union` / `Schema.Literal` / `Schema.Enums` / `Schema.TemplateLiteralParser`              | closed union    | `state`/`wire` tagged families; `TemplateLiteralParser` decodes structured string keys |
|  [04]   | `Schema.pick` / `Schema.omit` / `Schema.partial` / `Schema.extend` / `Schema.pluck`              | project         | `DERIVED_TYPES` — every projection derives from the one owner, never a parallel schema |
|  [05]   | `Schema.transformOrFail(from, to, { decode, encode })` / `Schema.transform`                      | bidirectional   | `wire` codec crossings, `kernel` brand mint — total both directions, proven by the `@rasm/ts-testkit` law combinators (`tests/typescript/_testkit`) |
|  [06]   | `Schema.brand("K")` / `Schema.filter` / `Schema.optionalWith(s, { as: "Option", default })`     | refine          | `kernel/schema` brand floor; `optionalWith` decodes absence to `Option` with a constructor default |
|  [07]   | `Schema.Option` / `Schema.Either` / `Schema.Chunk` / `Schema.HashMap` / `Schema.Redacted`        | effect-data     | schemas whose decoded value is an Effect data structure, not a plain object |
|  [08]   | `Schema.Uint8ArrayFromBase64` / `Schema.StringFromHex` / `Schema.parseJson(inner)`               | wire codec      | `wire` byte↔value crossings; `parseJson` composes an inner schema over a JSON string field |
|  [09]   | `Arbitrary.make(schema)` / `Arbitrary.makeLazy(schema)` / `JSONSchema.make(schema)` / `Pretty.make(schema)` / `Schema.equivalence(schema)` | derive | one Schema yields the generator (`tests/typescript/_testkit`), the OpenAPI node (`edge`), the printer, and structural equality; `makeLazy` returns the deferred `LazyArbitrary<A>` for recursive/suspended schemas |
|  [10]   | `Schema.standardSchemaV1(schema)`                                                                | interop         | `ui` form libraries and any Standard-Schema consumer bind the decoder without an adapter |

[ENTRYPOINT_SCOPE]: `Context` / `Layer` / `Runtime` — services and composition roots
- rail: surfaces-and-dispatch

| [INDEX] | [SURFACE]                                                                                     | [ENTRY_FAMILY]  | [CONSUMER]                                                    |
| :-----: | :-------------------------------------------------------------------------------------------- | :-------------- | :----------------------------------------------------------- |
|  [01]   | `class Svc extends Effect.Service<Svc>()("app/Svc", { effect, dependencies })`                | service owner   | the canonical service form — Tag, default `Layer`, and accessor in one declaration |
|  [02]   | `class Tag extends Context.Tag("app/Port")<Tag, Shape>() {}` / `Context.Reference`            | port Tag        | `PORT_LAW` — `security`/`store`/`ui` declare ports the app root satisfies across the ledger edge |
|  [03]   | `Layer.effect(Tag, build)` / `Layer.scoped(Tag, build)` / `Layer.succeed(Tag, value)`         | construct layer | folder Layers; `scoped` ties the service to a `Scope` so teardown is structural |
|  [04]   | `Layer.provide` / `Layer.provideMerge` / `Layer.mergeAll` / `Layer.merge`                     | compose layer   | app root builds the dependency graph; `provideMerge` keeps the provided service in the output |
|  [05]   | `Layer.launch(layer)` / `Layer.memoize` / `Layer.fresh` / `Layer.retry(schedule)`             | run / lifetime  | `proc/life` long-lived roots; `memoize` shares one instance, `fresh` forces a new build |
|  [06]   | `Layer.setConfigProvider` / `Layer.setTracer` / `Layer.setClock` / `Layer.setRequestBatching` | override policy | `proc/config`, `telemetry`, the testkit harness (TestClock) swap the runtime service under the whole graph |
|  [07]   | `ManagedRuntime.make(layer)` → `.runFork` / `.runPromise` / `.dispose`                         | runtime root    | `browser/boot` builds once, the imperative host calls in; `LayerMap.make` for keyed runtimes |
|  [08]   | `Reloadable.auto(Tag, { layer, schedule })` / `Reloadable.reload`                             | hot reload      | `proc/flag` re-evaluates a remote provider on a schedule without tearing the graph |

[ENTRYPOINT_SCOPE]: dispatch — `Match` and `Data`
- rail: surfaces-and-dispatch

| [INDEX] | [SURFACE]                                                                                   | [ENTRY_FAMILY]  | [CONSUMER]                                                      |
| :-----: | :------------------------------------------------------------------------------------------ | :-------------- | :------------------------------------------------------------- |
|  [01]   | `Match.type<T>()` / `Match.value(v)` → `Match.tag`/`Match.when` → `Match.exhaustive`         | dispatch        | `wire`/`edge`/`iac` — exhaustive over a tagged family; missing arm is a compile error |
|  [02]   | `Match.tagsExhaustive({ TagA: fa, TagB: fb })` / `Match.discriminatorsExhaustive("kind")`    | record dispatch | closed-family dispatch by `_tag` or a named discriminant, total by construction |
|  [03]   | `Match.whenOr` / `Match.whenAnd` / `Match.not` / `Match.orElse` / `Match.withReturnType`      | predicate arm   | structural/predicate dispatch on non-keyed shapes; `withReturnType` pins the arm result type |
|  [04]   | `Data.taggedEnum<Union>()` → `{ $match, $is, ...ctors }`                                     | closed family   | `state`/`wire` decoded unions with generated constructors, `$match`, and `$is` guards |
|  [05]   | `Data.TaggedError("Tag")<{...}>` / `Data.TaggedClass` / `Data.struct` / `Data.array`         | value + fault   | structural-equality records and typed errors; `Data.struct` gives `Equal`/`Hash` for free |

[ENTRYPOINT_SCOPE]: `Stream` / `Sink` — pull-based streaming
- rail: rails-and-effects

| [INDEX] | [SURFACE]                                                                                          | [ENTRY_FAMILY] | [CONSUMER]                                                    |
| :-----: | :------------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `Stream.fromQueue` / `Stream.fromPubSub` / `Stream.async` / `Stream.asyncScoped` / `Stream.asyncPush` / `Stream.fromReadableStream` | source         | `edge/live` SSE/WS feeds, `wire/frame` reassembly, `browser/transport` fetch streams; `asyncScoped` bridges a scoped `addEventListener`, `asyncPush` a backpressured push source |
|  [02]   | `Stream.mapEffect(f, { concurrency })` / `Stream.grouped` / `Stream.groupedWithin` / `Stream.throttle` | transform   | bounded-concurrency per-element effects, batch windows, rate shaping on one carrier |
|  [03]   | `Stream.broadcast` / `Stream.partition` / `Stream.merge` / `Stream.zipLatest`                      | fan            | multi-consumer fan-out, keyed split, latest-value join for live dashboards |
|  [04]   | `Stream.run(sink)` / `Stream.runForEach` / `Stream.runFold` / `Stream.toReadableStream`            | drain          | terminal consumption; `Sink.foldWeighted`/`Sink.collectAllN` are the reusable fold sinks |
|  [05]   | `Stream.retry(schedule)` / `Stream.debounce` / `Stream.buffer` / `Stream.haltWhen`                 | resilience     | resumable feeds; `haltWhen(deferred)` ends the stream on an external signal |

[ENTRYPOINT_SCOPE]: concurrency and mutable state
- rail: rails-and-effects

| [INDEX] | [SURFACE]                                                                                    | [ENTRY_FAMILY]  | [CONSUMER]                                                    |
| :-----: | :------------------------------------------------------------------------------------------- | :-------------- | :----------------------------------------------------------- |
|  [01]   | `Ref.make` / `SubscriptionRef.make` / `Subscribable` / `SynchronizedRef` (`.modify`, `.updateSomeAndGet`)     | shared cell     | `SubscriptionRef` is a `Ref` + `Stream.changes`; `Subscribable` is its read-only `{ get, changes }` projection — `ui/atom`, `nuqs`, and `state/query/live` observe through it |
|  [02]   | `Deferred.make` / `Deferred.await` / `Deferred.succeed`                                       | one-shot        | fiber handoff, `haltWhen` signals, promise-once coordination |
|  [03]   | `Queue.bounded` / `Queue.sliding` / `PubSub.bounded` / `Mailbox.make`                         | channel         | `work/queue` job intake, `edge/live` fan-out, `wire/quarantine` poison buffer with backpressure |
|  [04]   | `FiberRef.make` / `FiberRef.locallyScoped` / built-in `FiberRef.currentLogAnnotations`        | fiber-local     | `edge/middleware` request/tenant/locale context, propagated across `fork` without threading |
|  [05]   | `Effect.makeSemaphore(n)` / `Effect.makeLatch` / `FiberSet.make` / `FiberMap.make`            | bound / track   | concurrency caps (`edge` load-shed), keyed fiber registries (`work/engine` per-entity fibers) |
|  [06]   | `STM.commit` / `TRef.make` / `TMap` / `TQueue` / `TSemaphore` / `TReentrantLock`              | transaction     | `state`/`work` multi-cell atomic updates with automatic retry on conflict |

[ENTRYPOINT_SCOPE]: schedule, config, time, and observability signals
- rail: system-apis

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY]  | [CONSUMER]                                                 |
| :-----: | :---------------------------------------------------------------------------------------------- | :-------------- | :-------------------------------------------------------- |
|  [01]   | `Schedule.exponential` / `Schedule.jittered` / `Schedule.intersect` / `Schedule.recurWhile` / `Schedule.cron` | recurrence | `kernel/fault` budget rows compile to schedules; `work/queue` cron; retry policy as a value |
|  [02]   | `Config.string` / `Config.redacted` / `Config.integer` / `Config.withDefault` / `Config.nested` | config schema   | `proc/config` typed ingress; `Config.redacted` keeps secrets in `Redacted` end-to-end |
|  [03]   | `ConfigProvider.fromEnv` / `ConfigProvider.fromJson` / `.orElse` / `ConfigProvider.constantCase`| provider chain  | `runtime/src/proc/config.ts` env→file→remote chain; case adapters map `FOO_BAR`↔`fooBar` |
|  [04]   | `Duration.seconds` / `Duration.decode` / `DateTime.now` / `DateTime.addDuration` / `Cron.parse` | time            | `kernel/clock` HLC composition, `work` deadlines, `store` retention windows |
|  [05]   | `Metric.counter` / `Metric.histogram` / `Metric.gauge` / `Metric.timerWithBoundaries` / `.tagged` | metric        | `telemetry/signal` — `(app, tenant)`-tagged instruments; `Effect.withMetric` attaches to a rail |
|  [06]   | `Logger.make` / `Logger.replace` / `Logger.batched` / `LogLevel.Debug` / `Logger.structuredLogger`| logger        | `telemetry` structured logging; `Logger.batched` for buffered export, `withMinimumLogLevel` gate |
|  [07]   | `Metric.snapshot` / `Tracer.externalSpan` / `Effect.makeSpanScoped`                             | signal read     | `core/observe/board` reads the registry; `externalSpan` continues an extracted W3C trace context |

[ENTRYPOINT_SCOPE]: immutable collections, equality, and caching
- rail: shapes

| [INDEX] | [SURFACE]                                                                                    | [ENTRY_FAMILY]  | [CONSUMER]                                                    |
| :-----: | :------------------------------------------------------------------------------------------- | :-------------- | :----------------------------------------------------------- |
|  [01]   | `Chunk` / `HashMap` / `HashSet` / `SortedMap` / `MutableHashMap` (algorithm-local)           | collection      | `state/fold` keyed folds, `wire` decoded maps — Effect collections, not JS `Map`/`Set` |
|  [02]   | `Array.*` / `Record.*` / `Struct.*` / `Tuple.*` (module functions over plain values)          | stdlib fold     | `DERIVED_LOGIC` folds; `Array.reduce`/`Record.map` replace imperative loops at FFI-safe altitude |
|  [03]   | `Order.*` / `Equivalence.*` / `Equal.equals` / `Hash.hash` / `Predicate.*`                    | comparison      | `state/causal` ordering, `Data`-backed structural equality, `Predicate.and`/`or` refinement algebra |
|  [04]   | `Cache.make({ capacity, timeToLive, lookup })` / `RcMap.make` / `KeyedPool.make` / `Pool.make`| cache / pool     | `store/scope` lookups, `net/client` connection pools, `ai` provider clients — TTL + reference counting |
|  [05]   | `RateLimiter.make({ limit, interval })` / `Request.Class` / `RequestResolver.makeBatched`     | rate / batch     | `net/client` API-key limits, `store`/`ai` request de-duplication and batching under one resolver |
|  [06]   | `Encoding.encodeBase64` / `Encoding.decodeHex` / `Redacted.make` / `Redacted.value`           | codec / secret   | `security` byte encodings, `wire` frame codecs; `Redacted` unwraps only at the crypto boundary |

## [04]-[IMPLEMENTATION_LAW]

[EFFECT_TOPOLOGY]:
- `Effect<A, E, R>` is a description, not a running computation; nothing executes until `Effect.runFork`/`runPromise`/`runSync` at the one imperative edge, or `Layer.launch`/`ManagedRuntime` at a composition root. Dependent steps compose through `Effect.flatMap`/`Effect.gen`; independent operands accumulate through `Effect.all`/`Effect.forEach` where the `mode`/`concurrency` option — never a flag on the value — selects abort-on-first-failure versus validate-all.
- The three type parameters are the whole contract: `A` success, `E` the typed error channel (a tagged union, discriminated by `catchTag`/`catchTags`), `R` the requirement set of `Context.Tag`s the app root must satisfy. `R` reaching `never` at the composition root is the proof that every dependency is wired; an unsatisfied Tag is a compile error, not a runtime `undefined`.
- `Schema` is the one boundary codec: `Type` is the decoded interior value, `Encoded` the wire shape, and every secondary surface (`Arbitrary`, `JSONSchema`, `Pretty`, `Equivalence`, `standardSchemaV1`) derives from the same AST so it cannot drift. Decode once at ingress with `Schema.decodeUnknown`; the interior never re-validates and never sees `null`/`undefined`/provider shapes.
- `Context.Tag`/`Effect.Service` are the DI primitives; `Layer` builds the acyclic dependency graph with memoized construction, `scoped` construction for resource lifetime, and `provide`/`provideMerge` for wiring. A `Layer` is a value — folders export Layer families and the thin app `main.ts` selects and composes them with zero lib edits.
- Cross-cutting capability attaches at its seam as an effect transformer: decode+brand at the single Schema, `Effect.withSpan`/`annotateLogs`/`withMetric` for observability, `Effect.retry(Schedule)` for resilience, `Effect.acquireRelease`+`Layer.scoped` for lifetime. `Effect.fn("name")(body, ...pipeline)` is the seam where the body runs once per attempt and the pipeline carries the policy — resilience is recoverable from the declaration, never buried inside the body.
- Interruption is structural: `Effect.forkScoped` fibers are interrupted when their `Scope` closes, finalizers run on success, failure, and interrupt alike, and `Cause` retains the full failure tree (typed error + defect + interrupt + parallel causes) so `wire/fault` and `telemetry/crash` reconstruct rather than flatten.

[STACKS_WITH]:
- `@effect/platform` (`.api/effect-platform.md`): the platform contracts are `Effect`-returning services keyed by `Context.Tag` — `HttpClient` yields `Effect<HttpClientResponse, HttpClientError>`, `HttpApiEndpoint` bodies are `Schema`-typed handlers, and `FileSystem`/`Command`/`Worker` are Tags a `Layer` satisfies. One Schema decodes the request and encodes the response; the same tagged error family flows the `Effect` error channel to the `edge` problem-detail mapping.
- `@effect/platform-node` (`.api/effect-platform-node.md`): the `Node*` `Layer`s satisfy the platform Tags this substrate's `Layer` graph requires; `NodeRuntime.runMain` is the `Effect.runFork` edge for a node process, draining fibers and finalizers on signal.
- `@effect/opentelemetry` (`.api/effect-opentelemetry.md`): `Metric`, `Logger`, and `Tracer` are the vendor-neutral signal owners; the OTel `Layer` swaps the `Tracer`/`MetricRegistry`/`Logger` services under the whole graph via `Layer.setTracer`, so `Effect.withSpan`/`withMetric` on any rail export through OTLP with no call-site change.
- `@effect/vitest` (dev-tool tier, `tests/typescript/.api/effect-vitest.md`): `it.effect`/`it.scoped` run an `Effect` under `TestServices` (deterministic `TestClock`/`TestRandom`); `it.prop` consumes `Schema`-derived `FastCheck.Arbitrary`s; `it.layer` shares a `Layer` across a spec block — the testkit needs no hand-rolled harness.
- folder-local substrate (catalogued at `libs/typescript/<folder>/.api/`): `@effect/sql` `SqlClient` and `@effect/cluster` `MessageStorage` are `Context.Tag`s `work`/`store` compose and the app root satisfies; `@effect-atom` binds an `Effect`/`SubscriptionRef` into a React atom (`ui`); `@electric-sql/d2ts` folds a `state` dataflow; `hash-wasm` sits behind the `kernel` `ContentKey` mint — each is `Effect`-wrapped at its owner, never leaked raw.

[LOCAL_ADMISSION]:
- Use `Effect<A, E, R>` for every fallible or effectful operation; never a bare `Promise`, a thrown exception in domain logic, or a `try`/`catch` outside a `BOUNDARY ADAPTER` kernel — a `Promise`/throw converts at the owning seam through `Effect.tryPromise`/`Effect.try`.
- Use one `Schema` per concept with `pick`/`omit`/`partial`/`extend` projections; never a parallel `type`/`interface` mirroring a runtime shape, a fresh Schema per view, or a standalone branded-primitive export.
- Use `Context.Tag`/`Effect.Service` + `Layer` for dependencies; never a module-level singleton, a hand-passed config object, or a manual constructor-injection chain.
- Use `Match.exhaustive`/`Data.taggedEnum().$match` for closed-family dispatch and vocabulary lookup for keyed domains; never an `if`/`switch` ladder that re-derives what a tag or a vocabulary row already carries.
- Use Effect collections (`HashMap`/`HashSet`/`Chunk`) and `Option` in domain code; JS `Map`/`Set`/`Array`/`null` survive only at FFI and serialization boundaries.
- Use `Effect.retry(Schedule)`, `Effect.withSpan`, and `Effect.acquireRelease` as composed transformers; never a hand-rolled retry loop, a manual `startSpan`/`endSpan` pair, or a `try`/`finally` cleanup where a scope owns the lifetime.

[RAIL_LAW]:
- Package: `effect`
- Owns: the `Effect` carrier, `Schema` decode/encode + derivations, `Context`/`Layer`/`Runtime` DI, `Match`/`Data` dispatch, `Stream`/`Sink` streaming, the concurrency + `STM` substrate, `Schedule` recurrence, `Config`/`ConfigProvider`, `Metric`/`Logger`/`Tracer` signals, the immutable collection + equality family, `Cache`/`Pool`/`Request` batching, and the `Option`/`Either`/`Cause`/`Redacted`/`Encoding`/`Duration`/`DateTime` interop primitives
- Accept: `Effect.gen`/`Effect.fn`, `Schema.decodeUnknown` + one owner schema, `Effect.Service`/`Context.Tag` + `Layer`, `Match.exhaustive`/`Data.taggedEnum`, `Effect.retry(Schedule)`/`Effect.withSpan`/`Effect.acquireRelease` as seam transformers, `Effect.all`/`forEach` with explicit `concurrency`, Effect collections + `Option`
- Reject: bare `Promise`/`async`/throw in domain logic, parallel schemas or standalone branded-type exports, module singletons in place of Layers, `if`/`switch` re-deriving a vocabulary, JS `Map`/`Set`/`null` in domain code, hand-rolled retry/span/cleanup where a combinator owns it
