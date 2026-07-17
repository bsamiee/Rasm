# [TS_BRANCH_API_EFFECT]

`effect` is the branch standard library and the one runtime substrate every `libs/typescript` folder types against: the `Effect<A, E, R>` carrier (typed success, typed error channel, typed requirement context), `Schema` (parse-not-validate decode/encode with derivation to `Arbitrary`/`JSONSchema`/`Pretty`/`Equivalence`), `Context`/`Layer`/`Runtime` (dependency injection and composition roots), `Match`/`Data` (tagged-union dispatch and closed families), `Stream`/`Sink`/`Channel` (pull-based backpressured streaming), the concurrency substrate (`Fiber`, `Ref`/`SubscriptionRef`, `Deferred`, `Queue`/`PubSub`/`Mailbox`, `Semaphore`, `STM`), `Schedule` (recurrence algebra for retry/repeat), the observability trio (`Metric`, `Logger`, `Tracer`), `Config`/`ConfigProvider` (validated environment ingress), the immutable collection family (`Chunk`/`HashMap`/`HashSet`/`Array`/`Record`/`SortedMap`), the caching and batching surface (`Cache`, `RcMap`, `Pool`, `RateLimiter`, `Request`/`RequestResolver`), and the interop primitives (`Option`, `Either`, `Cause`, `Exit`, `Redacted`, `Encoding`, `Duration`, `DateTime`). It is isomorphic and dependency-free — one carrier that composes monadically for dependent steps and applicatively for independent accumulation, with cross-cutting capability (decode, span, retry, lifetime) attaching at the seam through effect transformers rather than being threaded by hand.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `effect`
- package: `effect` (MIT, © Effectful Technologies)
- module format: ESM + CJS dual (`dist/esm` + `dist/cjs`, types `dist/dts`), `sideEffects: []`; per-module deep-import subpaths (`effect/Effect`, `effect/Schema`, …) plus the flat `effect` barrel — the module boundary graph imports the barrel, deep-imports only where tree-shaking a single module matters
- runtime target: isomorphic (node, bun, browser, worker); zero runtime dependencies; no native addon
- asset: pure-TypeScript runtime library shipping `.js` + `.d.ts`; the type-level surface (`typeof schema.Type`, `keyof typeof`, branded refinements) is load-bearing, so `tsc`/`tsgo` are the real gate
- rail: substrate (every folder types against it; catalogued once at the branch tier)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the carrier, its data siblings, and failure algebra
- rail: rails-and-effects

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY]          | [CONSUMER]                                                   |
| :-----: | :--------------------------------------------- | :--------------------- | :----------------------------------------------------------- |
|  [01]   | `Effect<A, E, R>`                              | carrier                | every folder — the one rail; `R` is the app-root Tag set     |
|  [02]   | `Option<A>` / `Either<R, L>`                   | value union            | `kernel` absence, decode results — replaces `null` in domain |
|  [03]   | `Cause<E>` / `Exit<A, E>`                      | failure tree / outcome | `wire/fault`, `otel/crash` — retains defect + interrupt      |
|  [04]   | `Data.TaggedEnum<...>` / `Data.Class`          | closed family          | `state`, `wire`, `edge` — structural value equality          |
|  [05]   | `Redacted<A>`                                  | secret carrier         | `security/secret`, `proc/config` — never logged              |
|  [06]   | `Duration` / `DateTime.Utc` / `DateTime.Zoned` | time value             | `core/value/clock`, `work`, `store` — monotonic + wall-clock |
|  [07]   | `Brand.Branded<A, K>`                          | nominal refinement     | `core/value/schema` brand floor — decode-once type identity  |
|  [08]   | `Scope` / `Fiber<A, E>`                        | resource / handle      | `proc/life`, `work`, `browser` — structured lifetime         |

[PUBLIC_TYPE_SCOPE]: schema, its derivations, and boundary shapes
- rail: boundaries

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY]   | [CONSUMER]                                               |
| :-----: | :------------------------------------------------------ | :-------------- | :------------------------------------------------------- |
|  [01]   | `Schema.Schema<Type, Encoded, R>`                       | codec           | every boundary — decode + encode + derivations           |
|  [02]   | `Schema.Struct` / `Schema.Class` / `Schema.TaggedClass` | record family   | `store/journal` events, `kernel` value objects           |
|  [03]   | `Schema.TaggedError` / `Schema.TaggedRequest`           | fault / request | `wire/fault` errors, `store`/`work` request schemas      |
|  [04]   | `Schema.PropertySignature` / `Schema.optionalWith`      | field modality  | `core/value/schema` optional→`Option`, defaults, rename  |
|  [05]   | `ParseResult.ParseError` / `ParseResult.ParseIssue`     | decode fault    | every ingress → `Effect` channel; `ArrayFormatter` trees |
|  [06]   | `SchemaAST.AST` + annotation IDs                        | reflection node | testkit derivation; `edge/api` OpenAPI reads the AST     |
|  [07]   | `FastCheck.Arbitrary<A>` / `Arbitrary.LazyArbitrary<A>` | generator       | testkit source — Schema-derived; `makeLazy` recursive    |

[PUBLIC_TYPE_SCOPE]: services, layers, and dispatch surfaces
- rail: surfaces-and-dispatch

| [INDEX] | [SYMBOL]                                                 | [TYPE_FAMILY]     | [CONSUMER]                                              |
| :-----: | :------------------------------------------------------- | :---------------- | :------------------------------------------------------ |
|  [01]   | `Context.Tag` / `Context.TagClass` / `Context.Reference` | service key       | every port — `ai`/`store` Tags; `Reference` default     |
|  [02]   | `Layer<ROut, E, RIn>`                                    | wiring            | app roots — `Layer` families `main.ts` selects          |
|  [03]   | `LayerMap.Service` / `LayerMap`                          | keyed layer cache | `store/scope` `StoreHandle` `(appKey, tenancy)`         |
|  [04]   | `ManagedRuntime<R, E>`                                   | runtime root      | `browser/boot`, `proc/exec` — host edge calls in        |
|  [05]   | `Match.Matcher` (`Match.type`/`Match.value`)             | dispatch builder  | `wire` codec, `iac/provider` arms, `edge` fault mapping |
|  [06]   | `Metric.Metric` / `Logger.Logger` / `Tracer.Span`        | signal owner      | `telemetry` — counters, loggers, spans on the rail      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Effect` carrier — construct, boundary lift, run; every surface is `Effect.*`
- rail: rails-and-effects

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY] | [CONSUMER]                                             |
| :-----: | :------------------------------------------------------ | :------------- | :----------------------------------------------------- |
|  [01]   | `succeed` / `fail` / `sync` / `suspend`                 | construct      | values + thunks; `fail` seeds the error channel        |
|  [02]   | `gen(function*(){ … })` / `fn("span")(body, …pipeline)` | do-notation    | every flow; body per attempt, policy pipeline          |
|  [03]   | `tryPromise({...})` / `try` / `async` / `promise`       | boundary lift  | `Promise`/throw → typed error at the seam              |
|  [04]   | `all(effects, {...})` / `forEach` / `validateAll`       | applicative    | operands accumulate; `mode`: abort vs validate-all     |
|  [05]   | `flatMap` / `andThen` / `zipWith` / `tap`               | sequence       | steps; `andThen` = value / `Effect` / thunk            |
|  [06]   | `catchTag` / `catchTags` / `catchAll` / `tapErrorTag`   | recover        | `wire`/`store` typed recovery by `_tag`                |
|  [07]   | `retry` / `timeout` / `withExecutionPlan` / `race`      | resilience     | `net`/`ai`/`work` — `ExecutionPlan` multi-tier         |
|  [08]   | `acquireRelease` / `acquireUseRelease`                  | resource       | scoped acquire; release on success/fail/interrupt      |
|  [09]   | `addFinalizer` / `scoped`                               | resource       | `proc/life`, `store`, `work` leases                    |
|  [10]   | `fork` / `forkScoped` / `forkDaemon` / `interrupt`      | concurrency    | `edge/live`, `browser` — scoped forks die with scope   |
|  [11]   | `provide` / `provideService` / `updateService`          | context supply | app root injects `Layer`/service; per-Tag at a site    |
|  [12]   | `withSpan` / `annotateLogs` / `withMetric` / `log*`     | observe seam   | `telemetry` — span/log/metric attach onto the rail     |
|  [13]   | `runFork` / `runPromise` / `runSync` / `runPromiseExit` | run            | imperative edge; `runtime` re-enters foreign callbacks |

[ENTRYPOINT_SCOPE]: `Schema` — decode, project, transform, derive; surfaces are `Schema.*` unless qualified
- rail: boundaries

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY] | [CONSUMER]                                                |
| :-----: | :------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `decode` / `decodeUnknown` / `decodeUnknownEither`       | decode         | `decodeUnknown` at ingress; `decode` over `Encoded` input |
|  [02]   | `decodeSync` / `encode`                                  | encode         | interior sync mint; `encode` egress; `ParseError` lifts   |
|  [03]   | `Struct({...})` / `TaggedStruct({...})` / `Class(...)`   | declare shape  | one runtime authority per concept; `Class` adds a ctor    |
|  [04]   | `Union` / `Literal` / `Enums` / `TemplateLiteralParser`  | closed union   | `state`/`wire` families; parse structured string keys     |
|  [05]   | `pick` / `omit` / `partial` / `extend`                   | project        | every projection derives from one owner                   |
|  [06]   | `pluck` / `typeSchema`                                   | project        | `typeSchema` projects the type-side schema                |
|  [07]   | `transformOrFail(from, to, {...})` / `transform`         | bidirectional  | `wire` codec, `kernel` brand — total both directions      |
|  [08]   | `brand("K")` / `filter` / `optionalWith(s, {...})`       | refine         | brand floor; `optionalWith` absence→`Option`              |
|  [09]   | `Option` / `Either` / `Chunk` / `HashMap` / `Redacted`   | effect-data    | decoded value is an Effect data structure                 |
|  [10]   | `Uint8ArrayFromBase64` / `StringFromHex` / `parseJson`   | wire codec     | `wire` byte↔value; `parseJson` over a JSON field          |
|  [11]   | `Arbitrary.make` / `Arbitrary.makeLazy`                  | derive         | generator; `makeLazy` for recursive schemas               |
|  [12]   | `JSONSchema.make` / `Pretty.make` / `Schema.equivalence` | derive         | OpenAPI node, printer, structural equality                |
|  [13]   | `standardSchemaV1(schema)`                               | interop        | `ui` forms + Standard-Schema consumers bind the decoder   |

[ENTRYPOINT_SCOPE]: `Context` / `Layer` / `Runtime` — services and composition roots; surfaces are `Layer.*` unless qualified
- rail: surfaces-and-dispatch

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY]  | [CONSUMER]                                                 |
| :-----: | :--------------------------------------------------- | :-------------- | :--------------------------------------------------------- |
|  [01]   | `class Svc extends Effect.Service<Svc>()("app/Svc")` | service owner   | Tag + default `Layer` + accessor in one declaration        |
|  [02]   | `class Tag extends Context.Tag("id")<Tag, Shape>()`  | port Tag        | `security`/`store`/`ui` ports; `Reference` default         |
|  [03]   | `effect(Tag, build)` / `scoped` / `succeed`          | construct layer | folder Layers; `scoped` → a `Scope`                        |
|  [04]   | `provide` / `provideMerge` / `mergeAll` / `merge`    | compose layer   | build the dep graph; `provideMerge` keeps it               |
|  [05]   | `launch` / `memoize` / `fresh` / `retry(schedule)`   | run / lifetime  | `proc/life` roots; `memoize` shares, `fresh` rebuilds      |
|  [06]   | `setConfigProvider` / `setTracer`                    | override policy | `proc/config`/`telemetry` swap services in the graph       |
|  [07]   | `setClock` / `setRequestBatching`                    | override policy | testkit `TestClock`; batching under the graph              |
|  [08]   | `ManagedRuntime.make(layer)`                         | runtime root    | `.runFork`/`.runPromise`/`.dispose`; `LayerMap.make` keyed |
|  [09]   | `Reloadable.auto(Tag, {...})` / `Reloadable.reload`  | hot reload      | `proc/flag` re-evaluates a provider on a schedule          |

[ENTRYPOINT_SCOPE]: dispatch — `Match` builder and `Data` closed families
- rail: surfaces-and-dispatch

| [INDEX] | [SURFACE]                                                 | [ENTRY_FAMILY]  | [CONSUMER]                                              |
| :-----: | :-------------------------------------------------------- | :-------------- | :------------------------------------------------------ |
|  [01]   | `Match.type<T>()` / `Match.value(v)` → `Match.exhaustive` | dispatch        | `tag`/`when` arms; missing arm is a compile error       |
|  [02]   | `Match.tagsExhaustive` / `Match.discriminatorsExhaustive` | record dispatch | dispatch by `_tag` or a named discriminant, total       |
|  [03]   | `Match.whenOr` / `Match.whenAnd` / `Match.not`            | predicate arm   | structural/predicate dispatch on non-keyed shapes       |
|  [04]   | `Match.orElse` / `Match.withReturnType`                   | predicate arm   | fallback arm; `withReturnType` pins the arm result type |
|  [05]   | `Data.taggedEnum<Union>()` → `{ $match, $is, ...ctors }`  | closed family   | `state`/`wire` unions; generated ctors + `$match`/`$is` |
|  [06]   | `Data.TaggedError("Tag")<{...}>` / `Data.TaggedClass`     | value + fault   | typed errors + structural-equality records              |
|  [07]   | `Data.struct` / `Data.array`                              | value           | `Equal`/`Hash` for free on plain structures             |

[ENTRYPOINT_SCOPE]: `Stream` / `Sink` — pull-based streaming; surfaces are `Stream.*` unless qualified
- rail: rails-and-effects

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY] | [CONSUMER]                                              |
| :-----: | :------------------------------------------------------- | :------------- | :------------------------------------------------------ |
|  [01]   | `fromQueue` / `fromPubSub` / `async`                     | source         | `edge/live` feeds, `wire/frame` reassembly              |
|  [02]   | `asyncScoped` / `asyncPush`                              | source         | scoped `addEventListener`; backpressured push           |
|  [03]   | `fromReadableStream` / `fromReadableStreamByob`          | source         | `browser/transport` streams, `data/object` BYOB ingress |
|  [04]   | `mapEffect(f, { concurrency })` / `mapConcatEffect`      | transform      | bounded per-element effects, effectful expansion        |
|  [05]   | `grouped` / `groupedWithin` / `throttle`                 | transform      | batch windows, rate shaping on one carrier              |
|  [06]   | `broadcast` / `partition` / `merge` / `zipLatest`        | fan            | fan-out, keyed split, latest-value join                 |
|  [07]   | `run(sink)` / `runForEach` / `runFold` / `runFoldEffect` | drain          | terminal fold; `Sink.foldWeighted`/`collectAllN`        |
|  [08]   | `toReadableStream` / `toReadableStreamEffect`            | drain          | feed web-stream consumers (`data/object` puts)          |
|  [09]   | `retry(schedule)` / `debounce` / `buffer` / `haltWhen`   | resilience     | resumable feeds; `haltWhen(deferred)` ends stream       |

[ENTRYPOINT_SCOPE]: concurrency and mutable state
- rail: rails-and-effects

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY] | [CONSUMER]                                                  |
| :-----: | :------------------------------------------------------ | :------------- | :---------------------------------------------------------- |
|  [01]   | `Ref.make` / `SubscriptionRef.make`                     | shared cell    | `SubscriptionRef` = `Ref` + `Stream.changes`                |
|  [02]   | `Subscribable` / `SynchronizedRef`                      | shared cell    | read-only `{ get, changes }`; `ui/atom`/`nuqs` observe      |
|  [03]   | `Deferred.make` / `Deferred.await` / `Deferred.succeed` | one-shot       | fiber handoff, `haltWhen` signals, promise-once             |
|  [04]   | `Queue.bounded` / `Queue.sliding`                       | channel        | `work/queue` job intake with backpressure                   |
|  [05]   | `PubSub.bounded` / `Mailbox.make`                       | channel        | `edge/live` fan-out, `wire/quarantine` poison buffer        |
|  [06]   | `FiberRef.make` / `FiberRef.locallyScoped`              | fiber-local    | `edge/middleware` context; built-in `currentLogAnnotations` |
|  [07]   | `Effect.makeSemaphore(n)` / `Effect.makeLatch`          | bound / track  | concurrency caps for `edge` load-shed                       |
|  [08]   | `FiberSet.make` / `FiberMap.make`                       | bound / track  | keyed fiber registries; `work/engine` per-entity            |
|  [09]   | `STM.commit` / `TRef.make` / `TMap`                     | transaction    | `state`/`work` atomic multi-cell updates, auto-retry        |
|  [10]   | `TQueue` / `TSemaphore` / `TReentrantLock`              | transaction    | transactional queue, semaphore, reentrant lock              |

[ENTRYPOINT_SCOPE]: schedule, config, time, and observability signals
- rail: system-apis

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [CONSUMER]                                                |
| :-----: | :--------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `Schedule.exponential` / `jittered` / `resetAfter`         | recurrence     | policy as a value; `resetAfter` re-arms after quiet       |
|  [02]   | `Schedule.intersect` / `recurs` / `upTo`                   | recurrence     | compose schedules; `upTo` bounds total elapsed            |
|  [03]   | `Schedule.whileInput` / `recurWhile` / `cron`              | recurrence     | gate re-drive on the fault value; `work/queue` cron       |
|  [04]   | `Config.string` / `redacted` / `integer`                   | config schema  | `proc/config` typed ingress; `redacted` keeps secrets     |
|  [05]   | `Config.withDefault` / `Config.nested`                     | config schema  | defaults + nested config sections                         |
|  [06]   | `ConfigProvider.fromEnv` / `fromJson` / `constantCase`     | provider chain | env→file→remote; `.orElse` chains; case adapters          |
|  [07]   | `Duration.seconds` / `Duration.decode` / `Cron.parse`      | time           | `core/value/clock` HLC, `work` deadlines, `store` windows |
|  [08]   | `DateTime.now` / `DateTime.addDuration`                    | time           | wall-clock evidence over the HLC composition              |
|  [09]   | `Metric.counter` / `histogram` / `gauge`                   | metric         | `telemetry` `(app, tenant)`-tagged instruments            |
|  [10]   | `Metric.timerWithBoundaries` / `increment` / `incrementBy` | metric         | `incrementBy` charges batch counts (`data/journal`)       |
|  [11]   | `Logger.make` / `replace` / `batched`                      | logger         | structured logging; `batched` buffered export             |
|  [12]   | `Metric.snapshot` / `Tracer.externalSpan`                  | signal read    | `core/observe/board` reads; `externalSpan` continues W3C  |
|  [13]   | `Effect.makeSpanScoped`                                    | signal read    | a scoped span the algorithm owns                          |

[ENTRYPOINT_SCOPE]: immutable collections, equality, and caching
- rail: shapes

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [CONSUMER]                                         |
| :-----: | :---------------------------------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `Chunk` / `HashMap` / `HashSet` / `SortedMap`   | collection     | not JS `Map`/`Set`; `MutableHashMap` algo-local    |
|  [02]   | `Array.*` / `Record.*` / `Struct.*` / `Tuple.*` | stdlib fold    | `reduce`/`map` replace imperative loops            |
|  [03]   | `Option.match` / `Option.zipWith`               | stdlib fold    | join optional evidence without a null sentinel     |
|  [04]   | `Order.*` / `Equivalence.*` / `Predicate.*`     | comparison     | ordering + `Predicate.and`/`or` refinement algebra |
|  [05]   | `Equal.equals` / `Hash.hash`                    | comparison     | `Data`-backed structural equality                  |
|  [06]   | `Cache.make({...})` / `RcMap.make`              | cache / pool   | TTL + reference counting; `store/scope` lookups    |
|  [07]   | `KeyedPool.make` / `Pool.make`                  | cache / pool   | `net/client` connection pools, `ai` clients        |
|  [08]   | `RateLimiter.make({...})` / `Request.Class`     | rate / batch   | `net` API-key limits; request de-duplication       |
|  [09]   | `RequestResolver.makeBatched`                   | rate / batch   | batch requests under one resolver                  |
|  [10]   | `Encoding.encodeBase64` / `Encoding.decodeHex`  | codec          | `security` byte encodings, `wire` frame codecs     |
|  [11]   | `Redacted.make` / `Redacted.value`              | secret         | unwrap only at the crypto boundary                 |

## [04]-[IMPLEMENTATION_LAW]

[EFFECT_TOPOLOGY]:
- `Effect<A, E, R>` is a description, not a running computation; nothing executes until `Effect.runFork`/`runPromise`/`runSync` at the one imperative edge, or `Layer.launch`/`ManagedRuntime` at a composition root. Dependent steps compose through `Effect.flatMap`/`Effect.gen`; independent operands accumulate through `Effect.all`/`Effect.forEach` where the `mode`/`concurrency` option — never a flag on the value — selects abort-on-first-failure versus validate-all.
- Three type parameters are the whole contract: `A` success, `E` the typed error channel (a tagged union, discriminated by `catchTag`/`catchTags`), `R` the requirement set of `Context.Tag`s the app root must satisfy. `R` reaching `never` at the composition root is the proof that every dependency is wired; an unsatisfied Tag is a compile error, not a runtime `undefined`.
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
