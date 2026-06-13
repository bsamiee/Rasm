# readme — corpus architecture and lane design

All structural facts verified (effect 3.21.2 installed; `Effect.fn`/`fnUntraced`, `Effect.all` mode `"validate"`, `Effect.makeLatch`, `Stream.fromEventListener`/`fromAsyncIterable`, `Schema.TaggedClass`/`TaggedError`/`parseJson`/`decodeUnknown`, `Data.TaggedEnum`, `ManagedRuntime`, `ExecutionPlan`, `PartitionedSemaphore`, `RateLimiter` all present; Biome 2.4.14 installed; `sgconfig.yml` routes `.rules/typescript` with `tests/ast-grep/{pass,fail}` expression-flow fixtures for ts/tsx/mts/cts; `docs/stacks/typescript/` is empty; `docs/stacks/postgres/` exists as its own stack). The complete corpus architecture follows.

---

# TYPESCRIPT CORPUS ARCHITECTURE — `docs/stacks/typescript`

## 1. CORE FILE SET

Seven core pages in integration order. Each page is authored under every finalized earlier page — adhered to, never restated.

| # | File | State plan | LOC |
|---|------|-----------|-----|
| 1 | `language.md` | author this session | 300 |
| 2 | `shapes.md` | author this session | 350 |
| 3 | `surfaces-and-dispatch.md` | bedrock this session | 320 |
| 4 | `rails-and-effects.md` | bedrock this session | 350 |
| 5 | `streams-and-concurrency.md` | roadmap | 350 |
| 6 | `boundaries.md` | roadmap | 320 |
| 7 | `system-apis.md` | roadmap | 300 |

**Inclusion/exclusion decisions:**

- `streams-and-concurrency.md` is CORE, not domain (the open question). STRUCTURED_CONCURRENCY is ratified doctrine, and every primitive involved (Fiber, Scope, Stream, Queue, PubSub, Mailbox, Deferred, Semaphore, in-process RateLimiter, FiberHandle/Map/Set) is an `effect`-core module, not a lane package. The C# precedent (concurrency as domain lane) reflected LanguageExt's periphery; in Effect, concurrency is the spine. Folding it into rails-and-effects would breach the 350-LOC core cap.
- `boundaries.md` is CORE: CHANNEL_TOTALITY admits exceptions/Promises/ambient singletons "at boundaries only" — that law needs exactly one owning page, and every lane consumes it.
- `system-apis.md` is CORE (C# precedent direct): the JS/Node built-in → Effect owner table is the highest-leverage anti-reflex artifact in the corpus; without it every page re-litigates `Date`/`Math.random`/`setTimeout`.
- NO `algorithms.md` analog — excluded: the TS catalog has no numeric package spine (no MathNet equivalent); collection/fold discipline lives in shapes.md COLLECTIONS, stream transforms in streams-and-concurrency. If a numeric lane is ever admitted, it lands as a domain lane, not core.
- NO `resilience` page anywhere (the decide-point): resilience IS core rails material. Schedule algebra, `Effect.retry`/`timeout`, `ExecutionPlan` fallback, core `RateLimiter`, `Cache` are all `effect`-core values — they are the POLICY section of rails-and-effects.md. The only lane-grade resilience artifact is the distributed RateLimiter (experimental, Redis store), which rides the cluster lane.
- NO `validation` lane (C# had one): Schema admission is shapes.md ADMISSION + boundaries.md WIRE_ADMISSION. A separate validation page would restate EVIDENCE_FUSION.
- NO `postgres` lane: `docs/stacks/postgres` owns the SQL surface; the TS persistence lane owns only the `@effect/sql-pg` embedding seam.

**Per-page specifications:**

### 3. `surfaces-and-dispatch.md` — position 3; consumes language (module/export law, `satisfies`, literal inference) and shapes (families, service/error declarations as dispatch subjects). Target 320 LOC.

Ownership: every public entrypoint's shape and every dispatch form — how one polymorphic surface, its discriminant, its service face, and its definition-time aspects compose.

Sections: chooser table (input kind → dispatch form: closed `_tag` family → `Match.exhaustive` / `Data.TaggedEnum` `$match`; bounded literal vocabulary → `satisfies`-closed `Record` table; structural capability → refinement + `Predicate`; modality/arity → input-shape discrimination at one entrypoint; service operation → `Effect.Service` method; wire verb family → request union + one handler) → card families: `[ENTRYPOINT_FAMILY]` (MODAL_ARITY instantiation), `[EFFECT_FN]` (`Effect.fn`/`fnUntraced` as the named, traced, curried entry), `[MATCH_DISPATCH]`, `[TABLE_DISPATCH]`, `[TAG_FAMILY_DISPATCH]`, `[SERVICE_SURFACE]` (accessors, default layer, one service = one concern), `[DUAL_SIGNATURE]` (data-first/data-last, `Pipeable`), `[ASPECT_VALUES]` (DEFINITION_TIME_ASPECTS: Layer wrapping, middleware values, schema annotations, Schedule attachment — decorators rejected), `[OPEN_SEAM]` (Context.Tag-keyed registry; where foreign extension is permitted).

### 4. `rails-and-effects.md` — position 4; consumes shapes (error owners, request shapes, Config shapes) and surfaces (service faces, dispatch inside recovery arms). Target 350 LOC.

Ownership: carrier selection, the typed failure alphabet, the composition spine, resource lifetime, and policy-as-value — the complete `Effect<A,E,R>` contract interior.

Sections: chooser (concern → carrier: absence → `Option`; pure sync fallibility → `Either`; effectful → `Effect`; accumulation → `Effect.all` mode `"validate"`; resource → Scoped effect/`Layer.scoped`; configuration → `Config`; mutable cell → Ref family) → cards: `[ERROR_ALPHABET]` (`Data.TaggedError` interior, `Schema.TaggedError` wire), `[FAILURE_VS_DEFECT]` (Cause, `fail` vs `die`, `orDie` promotion law), `[RECOVERY]` (`catchTag`/`catchTags` placement — recovery at the owner, never mid-pipeline), `[GEN_VS_PIPE]` (dependence licenses `Effect.gen`, independence licenses applicative `all`/`zip`), `[SCOPE_RESOURCE]` (`acquireRelease`, `addFinalizer`, RcRef/RcMap), `[SCHEDULE_POLICY]` (retry/repeat algebra, jitter, composition; `ExecutionPlan` provider fallback; core `RateLimiter`; `Cache`), `[CONFIG_RAIL]` (Config/ConfigProvider/`Redacted`), `[STATE_CELLS]` (Ref/SynchronizedRef/SubscriptionRef/FiberRef selection law).

### 5. `streams-and-concurrency.md` — position 5; consumes rails (Scope, error rail, policy values) and shapes (channel payloads). Target 350 LOC.

Ownership: fiber lifetime, interruption, declared concurrency, coordination primitives, and the Stream spine.

Sections: chooser (need → primitive: one value later → `Deferred`; many values, one consumer → `Queue`/`Mailbox`; broadcast → `PubSub`; changing value with subscribers → `SubscriptionRef`; pull pipeline → `Stream`; push source → `Stream.async`; fiber registry → FiberHandle/FiberMap/FiberSet; mutual exclusion → `Effect.makeSemaphore`/`PartitionedSemaphore`; gate → `Effect.makeLatch`) → cards: `[FIBER_LIFECYCLE]` (fork/forkScoped/forkDaemon admission law), `[INTERRUPTION_RAIL]` (the only cancellation rail; `onInterrupt`, uninterruptible regions), `[CONCURRENCY_POLICY]` (the `concurrency` option as policy value — bounded/unbounded/inherit), `[COORDINATION]`, `[CHANNEL_FAMILY]`, `[STREAM_SPINE]` (construct/transform/Sink/run), `[BACKPRESSURE_POLICY]` (buffer/throttle/debounce/groupedWithin as values), `[STREAM_ERRORS]` (typed E through pipelines, retry/recover on streams).

### 6. `boundaries.md` — position 6; consumes all prior (admission produces shapes, results ride rails, iterables/events become streams, roots provide R). Target 320 LOC.

Ownership: every seam where non-Effect material enters or leaves — foreign async forms, wire bytes, host singletons, runtime roots, and non-Effect libraries.

Sections: chooser/admission matrix (foreign form → combinator: Promise → `Effect.tryPromise`; throwing sync → `Effect.try`; callback → `Effect.async`; async iterable → `Stream.fromAsyncIterable`; event source → `Stream.fromEventListener`/PubSub bridge; AbortSignal → interruption, both directions) → cards: `[PROMISE_ADMISSION]`, `[EXCEPTION_ADMISSION]` (expected → typed E, unexpected → defect), `[CALLBACK_ADMISSION]`, `[SIGNAL_BRIDGE]`, `[WIRE_ADMISSION]` (`Schema.decodeUnknown`/`parseJson` at every ingress; ParseError conversion at the owning boundary), `[RUNTIME_ROOTS]` (`run*` admitted only at roots; one Layer graph; `ManagedRuntime` for embedded hosts, platform `runMain` for processes), `[FOREIGN_LIBRARY]` (the owning service is the membrane — SDK types never escape it), `[HOST_SINGLETONS]` (`process`, `globalThis`, ambient env admitted via Layer/Config only).

### 7. `system-apis.md` — position 7, authored last; every row routes to an owner established by pages 1-6. Target 300 LOC.

Ownership: the replacement law for JS/Node built-ins — which platform primitive is rejected, which Effect module owns it, and the narrow rows where the native form remains the owner.

Sections: `[REPLACEMENT_TABLE]` (~30 rows: `Date`→DateTime, `Math.random`→Random, `setTimeout`/`setInterval`→Clock/Schedule/`Effect.sleep`, `JSON.parse`→`Schema.parseJson`, try/catch→`Effect.try`+Cause, `AbortController`→interruption, `EventEmitter`→Stream/PubSub, `console.*`→Logger, `process.env`→Config, `Promise.all`/`race`→`Effect.all`/`race`, async iterators→Stream, memoize→`Cache`/`Effect.cachedFunction`, throttle/debounce→Stream operators, cron strings→`Cron`, `fs`/`path`/`child_process`/`fetch`/`WebSocket`/`worker_threads`→platform FileSystem/Path/Command/HttpClient/Socket/Worker, base64/hex→`Encoding`, `===`/comparators→Equal/Order/Equivalence/Hash, `Map`/`Set`→HashMap/HashSet where value-equality is load-bearing, money/precision→`BigDecimal`, ms numbers→`Duration`) → cards: `[TIME_AUTHORITY]`, `[ENTROPY_AND_IDENTITY]` (Random determinism; `crypto.randomUUID` as the one admitted host primitive behind the ID-vending service — nanoid/uuid removed), `[CODEC_AUTHORITY]`, `[EQUALITY_ORDER]`, `[COLLECTION_AUTHORITY]` (when native Array/Map stays the owner), `[SCHEDULER_AUTHORITY]`.

## 2. DOMAIN LANE MAP

14 lanes + `domain/README.md` router + a testing route row. All `target` state; each gets a `.reports` single-file bedrock before authoring.

| Lane | Owners | Ownership statement |
|------|--------|---------------------|
| `runtime.md` | `@effect/platform`, `-node`, `-browser` | Host runtime services — FileSystem, Path, Command, Terminal, Worker pools, KeyValueStore, PlatformError taxonomy — provided per host through one platform layer. |
| `transport.md` | platform HttpApi/HttpClient, `@effect/rpc`, `@connectrpc/connect(-web)`, `@bufbuild/protobuf`+`protoc-gen-es`+`buf`, `@msgpack/msgpack` | Wire surfaces: contracted external REST and internal RPC, their derivation chains, and wire codecs. |
| `persistence.md` | `@effect/sql-pg`, `@effect/sql` | The TS embedding of relational persistence — sql template composition, batching, validated queries, migration, transactions. |
| `cluster.md` | `@effect/cluster`, `@effect/workflow`, experimental distributed RateLimiter | Distributed execution — entities, sharding, durable queues, cron singletons, durable workflows. |
| `diagnostics.md` | `@effect/opentelemetry`, effect Logger/Metric/Tracer, experimental DevTools | Telemetry signal — spans, metrics, logs, export paths. |
| `cli.md` | `@effect/cli` | Command-line binding — Options/Args schemas, subcommand dispatch, wizard, exit law. |
| `ai.md` | `@effect/ai` + provider layers, `@anthropic-ai/tokenizer` | Model inference — LanguageModel/Tool/Toolkit, provider swap, streaming, structured output. |
| `infra.md` | pulumi family, doppler | Infrastructure programs — resource graphs, stack config, secret flow into runtime Config. |
| `interaction.md` | react 19, `@effect-atom/atom-react`, tanstack, react-aria, radix, nuqs, cmdk/vaul, floating-ui, react-error-boundary | Retained UI — the React-Effect bridge, state ownership, navigation, accessibility primitives. |
| `visuals.md` (KEPT — decide-point) | tailwind 4, cva/clsx/tailwind-merge, lucide, colorjs.io, sharp | Design-system mechanics — token law, variant tables as policy values, color algebra, raster pipeline. |
| `data-interchange.md` | papaparse, exceljs, sax, yaml, jszip, jspdf, rfc6902, isomorphic-dompurify | File-format codecs admitted through Schema — CSV/XLSX/XML/YAML/ZIP/PDF/patch/sanitize. |
| `durability.md` | experimental EventLog, Persistence/PersistedCache (ResultPersistence), Reactivity, VariantSchema; browser KeyValueStore | Local/offline state — event sourcing, sync, the unified cache/rate-limit/idempotency backend. |
| `identity.md` | arctic, otplib, `@simplewebauthn/server`, nodemailer | Identity and access — OAuth flows, TOTP, WebAuthn ceremonies, session evidence, auth mail egress. |
| `integrations.md` | `@aws-sdk` trio + `@effect-aws/client-s3`, `@octokit/rest`, ioredis | Third-party service clients — each SDK admitted behind exactly one Effect service with typed errors; the SDK never crosses the membrane. |
| testing | route row only → `testing/README.md` | (`@effect/vitest`, fast-check via effect `FastCheck`/`Arbitrary`, testcontainers, Stryker.) |

**Research topics per lane (fable-shortcut bedrock):**

- `runtime`: PlatformError taxonomy → domain rail mapping; Worker serialized pools keyed on `Schema.TaggedRequest`; FileSystem streaming + watch; Command pipelines vs child_process; KeyValueStore schemaed store layering; per-host Layer assembly (Node/browser split); Terminal capabilities; platform-browser Geolocation/Permissions/Clipboard admission.
- `transport`: HttpApi derivation chain (one declaration → server + client + OpenAPI); HttpApiMiddleware + security schemes; RpcGroup serialization (NDJSON/msgpack) + Stream-valued RPCs; the Rpc→cluster-Entity→workflow reuse path (one Schema vocabulary edge→mesh→actors→workflows); connect-es/buf codegen as boundary schemas feeding Schema; `Schema.TaggedError` round-trip across the wire; HttpClient retry/trace integration.
- `persistence`: sql fragment composition; SqlResolver batching over RequestResolver; SqlSchema validated queries (wire+domain one declaration); Migrator law; transaction/savepoint Scope semantics; listen/notify → Stream; pool Layer + Config.
- `cluster`: Entity protocols from shared Rpc definitions; Sharding/runner topology; MessageStorage delivery semantics + entity mailboxes as the queue owner; ClusterCron vs in-process `Cron`; workflow Activity exactly-once + DurableClock + `withCompensation` top-level-only law; distributed RateLimiter (Redis store) vs core RateLimiter boundary.
- `diagnostics`: Otlp pure-Effect vs NodeSdk interop decision matrix (choose one per process); `Effect.fn`/`withSpan` span semantics + annotations; Metric→OTLP taxonomy; structured Logger + correlation; DevTools dev loop; export scheduling/sampling as policy values.
- `cli`: Options/Args schema derivation; subcommand union dispatch; wizard + prompts; Config/env precedence; completions; Exit→process-code law.
- `ai`: LanguageModel as service + provider Layer swap via `Effect.provide`; Tool/Toolkit schema-derived definitions; response streaming as Stream; tokenizer budgeting; ExecutionPlan cross-provider fallback; structured output via Schema.
- `infra`: pulumi Output/Input graph vs Effect (boundary law: pulumi owns its own async graph — Effect not threaded through programs); component resource families; doppler → stack config → runtime Config/Redacted flow; environment promotion; preview/deploy gating in CI.
- `interaction`: Atom as the domain state cell (zustand/zundo/immer removed — Atom + structural sharing own it); atom-react Result rendering + suspense; ManagedRuntime fallback spelling (pre-1.0 documented); server-cache ownership decision (Atom-over-Effect-services vs tanstack query — must be settled here; provisional: Atom owns Effect-derived state, tanstack owns routing); Schema-driven form admission; nuqs URL state vs Atom boundary; react-error-boundary as last-resort defect surface; React 19 actions/transitions interop with fibers.
- `visuals`: tailwind 4 CSS-first tokens; cva variant tables as POLICY_VALUES; colorjs.io OKLCH interpolation law; sharp pipelines as scoped Effect services; icon vocabulary/treeshaking.
- `data-interchange`: papaparse streaming → Stream; exceljs builders; sax streaming XML; yaml admission through Schema; jszip composition; rfc6902 vs effect `Differ` boundary; DOMPurify sanitization seam; jspdf generation.
- `durability`: EventLog event sourcing + client sync; ResultPersistence backend matrix (memory/KVS/Redis) unifying cache+rate-limit+idempotency; PersistedCache idempotency keys; browser storage routes (localStorage KVS, IndexedDB — idb-keyval removed); offline conflict law; VariantSchema storage variants; Reactivity invalidation.
- `identity`: arctic provider flows; otplib TOTP enrollment/verify; WebAuthn ceremony schemas; session token evidence (Schema + Redacted); HttpApiSecurity integration; nodemailer as auth-mail egress service.
- `integrations`: the service-membrane pattern (`@effect-aws/client-s3` as the exemplar); S3 presign/stream; octokit pagination → Stream; ioredis admission (consumed by durability backends + cluster rate limiter); credential flow via Config/Redacted; typed error translation per SDK.

**Inter-lane boundaries that must be stated in `domain/README.md` routing:**

1. HttpApi = external contracted REST + OpenAPI; RpcGroup = internal transport-agnostic + Stream-capable (objective, verified).
2. The same Rpc definitions ARE cluster Entity protocols and workflow registrations — transport declares, cluster consumes; never re-declare.
3. Otlp vs NodeSdk: one per process, never both.
4. core `RateLimiter` (rails) vs distributed RateLimiter (cluster, Redis store).
5. `Cron` in-process (system-apis) vs ClusterCron distributed singleton (cluster).
6. Queues: entity mailboxes + MessageStorage own them; bullmq REJECTED from the catalog now — recorded as a contingency in the cluster roadmap entry, admitted only if a production Redis queue is needed before the cluster lane stabilizes.
7. KeyValueStore: host provision in runtime; durable/schemaed/browser use in durability.
8. msgpack is transport wire codec; data-interchange owns file formats, not wire.
9. sharp → visuals (raster/color); jspdf/jszip → data-interchange (documents/archives).
10. ioredis admission lives in integrations; durability and cluster reference the admitted service.

## 3. README CONTENT SPEC

`README.md` (~135 LOC) — sections in C# benchmark order: intro paragraph pair, `[1]-[ATLAS]` (24 rows: 7 core + testing + domain router + 14 lanes, STATE column finalized/partial/target), `[2]-[DOCTRINE]` (preamble adapted from C# with the TS enforcement clause + the density preamble citing sublinear growth), `[3]-[COLLAPSE_SCAN]`, `[4]-[RULE_ENFORCEMENT]`, `[5]-[PAGE_CRAFT]`, `[6]-[CORPUS_LAW]`, `[7]-[ROADMAP]`.

### (a) Doctrine — nineteen laws in five groups

[FLOW]:
- `EXPRESSION_SPINE` — all domain logic is expression-shaped; dependent steps compose monadically through `Effect.gen` and `pipe`, independent computations compose applicatively through `Effect.all` and schema product shapes, and `Match` closes conditional logic as the expression of record — dependence licenses sequence, independence licenses accumulation, and the carrier, never a flag, selects the algebra. Statements survive only inside measured kernels and platform-forced boundaries, and any page that shows one names the exemption.
- `BOUNDARY_ADMISSION` — raw material is admitted exactly once through `Schema.decodeUnknown` into an evidence-carrying owner; interior code never re-validates and never sees `unknown`, `any`, null-as-failure, or provider shapes. `Option` carries absence, the typed failure channel carries fallibility, and `ParseError` converts at the owning boundary.
- `CHANNEL_TOTALITY` — `Effect<A, E, R>` is the complete contract: a closed `E` alphabet of tagged failures and an `R` requirement set provided exactly once at the composition root through one `Layer` graph. Exceptions, raw Promises, and ambient singletons are admitted at boundaries only; a signature that undersells its failure or requirement set is a defect, not a style choice.
- `STRUCTURED_CONCURRENCY` — every fiber is scoped: lifetimes attach to `Scope`, interruption is the only cancellation rail, and concurrency degree is declared as a policy value on the combinator, never improvised with unsupervised forks. A fiber that outlives its scope outside a named daemon root is a leak.

[SHAPE]:
- `SHAPE_BUDGET` — a concept owns exactly one declaration; variants are members of one closed family — a `Schema.TaggedClass` union, a `Data.TaggedEnum`, a `Schema.Literal` vocabulary — never sibling types. Three or more parallel shapes, sibling factories, repeated dispatch arms, or single-call helpers is the collapse trigger, not a style preference.
- `DEEP_SURFACES` — prefer one rich polymorphic surface over many shallow ones; each module exposes one entrypoint family — one service, one schema family, one dispatch surface — and keeps internals unexported. One deep owner that holds a full concern beats four fragments that scatter it.
- `MODAL_ARITY` — one entrypoint owns all call modalities; singular, plural, batch, and stream discriminate on the shape of the input value — tag, arity, `Iterable`, `Stream` — never on name suffixes or boolean knobs. A parallel parameter that re-describes the input is the knob smuggled back in; the discriminant must be recoverable from the value itself.
- `ANTICIPATORY_COLLAPSE` — an owner is shaped for the family it will absorb, not the instance in hand: the moment a second case, dimension, or modality is conceivable, the shape generalizes so the next requirement lands as a union member the compiler chases, a handler row, a policy value, or a layer swap with zero new surface. The proof of a correct shape is the diff of the next feature — one declaration inside the owner, every consumer untouched or broken loudly by `tsc`.
- `EVIDENCE_FUSION` — one declaration owns static type and runtime evidence: the schema is primary, and every secondary artifact — type, guard, codec, equivalence, arbitrary, JSON Schema — derives from it. Types are extracted through `Schema.Type` and `typeof`, never restated; nominal identity is declared through brands and tags, never simulated with parallel interfaces.

[DERIVATION]:
- `POLICY_VALUES` — configuration enters as one domain value that carries its own behavior — a `Schedule`, a `Layer`, a `Config`, a literal-keyed handler row — never as flag sets whose combinations the implementation must re-derive. Behavior rows live with the vocabulary that selects them.
- `DERIVED_LOGIC` — when cases share generative structure, the logic is derived — `Match.exhaustive` over the family, a `satisfies`-closed table, a `Schema.transform` chain — never enumerated arms. One primary correspondence is declared and every secondary map derives from it; the derivation is the executable specification.
- `DERIVED_TYPES` — types are computed where the language allows so one declaration yields the family: conditional, mapped, and template-literal types, `Schema.Type`/`Schema.Encoded`/`Context.Tag.Service` extraction, and constrained generics replace rank-specific or per-provider copies.
- `SYMBOLIC_REFERENCE` — names, discriminants, and correspondences travel as literal-typed symbols the compiler checks — `_tag` members, `keyof typeof` chains, `satisfies`-anchored keys — never as string literals that restate something the program already knows.
- `SEMANTIC_NAMING` — (transfer verbatim from C#: canonical term, grammatical role, tense; one-word default, three-word ceiling; operations verb, values noun, policies stable rows; boundary adapters preserve foreign names only at the seam.)

[MATERIAL]:
- `LIBRARY_DEPTH` — admitted packages are the standard library: `effect` owns rails, time, entropy, schema, state, streams, and collections; `@effect/platform` owns host services; lane owners own their domains. Use the deepest combinator the package itself reaches for; wrappers, rename adapters, and platform-reflex spellings — `Date`, `Math.random`, `JSON.parse`, `setTimeout`, raw `fetch` — are rejected. Native primitives remain owners only when they carry the invariant directly.
- `DEFINITION_TIME_ASPECTS` — cross-cutting capability attaches at definition time as composition values: `Schedule` policies, `Layer` wrapping, `HttpApiMiddleware`, schema annotations, `Effect.fn` instrumentation. Decorators are rejected; an aspect materializes policy and never hides control flow, and policy pushed across the admission seam stops being recoverable from its declaration.

[INTEGRATION]:
- `ROOT_REBUILD` — new capability is woven into the owning shape as if it had always existed; reshape the owner rather than appending beside it. No shims, compat aliases, deprecation layers, or migration surfaces — break the API when the collapse improves the system.
- `ONE_HOP_RESOLUTION` — a name resolves to its semantics in one hop: no barrel re-exports, no alias chains, no forwarding helpers, no convenience wrappers. A value that takes two jumps to trace marks a layer to delete.
- `COMPOSED_IMPLEMENTATION` — a feature of any complexity is implemented by composing the page owners — admitted schemas, typed rails, dispatch surfaces, scoped fibers, boundary projections — never by scaffolding beside them. A need with no composed spelling marks a missing case in an owning page's law: the law extends first, the feature lands second.

### (b) COLLAPSE_SCAN — 12 rows

| # | Signal | Move |
|---|--------|------|
| 1 | sibling `get`/`getMany`/`getById` names | one modality-polymorphic entrypoint on input shape |
| 2 | interface and schema declared in parallel for one concept | one schema; the type is extracted, never restated |
| 3 | try/catch inside domain flow | admission at the boundary; typed failure inward |
| 4 | boolean parameter selects between two bodies | one policy value — Schedule, Layer, or handler row |
| 5 | function calls exactly one other function | delete the hop |
| 6 | repeated `_tag` switch/if-else chains | `Match.exhaustive` or a derived table |
| 7 | raw Promise or async/await in a domain signature | the Effect carrier end-to-end |
| 8 | `useState`/`useReducer` holding domain state | Atom over the owning Effect service |
| 9 | wrapper renames a package API | use the package surface directly |
| 10 | hand-rolled retry, timeout, or polling loop | a Schedule value |
| 11 | `new Date()`, `Math.random()`, `setTimeout` in domain code | Clock, DateTime, Random through `R` |
| 12 | `Effect.provide` scattered mid-pipeline | one Layer graph at the composition root |

### (c) RULE_ENFORCEMENT

The TS enforcement stack is three-tiered and doctrine-first: `tsc` under the posture flags is the compiler contract; Biome 2.4 (`noFloatingPromises` stable) is the mechanical baseline; the repository ast-grep rules (`.rules/typescript` via `sgconfig.yml`, fixtures in `tests/ast-grep/pass|fail` — expression-flow fixtures already exist for ts/tsx/mts/cts) are the doctrine's compiled form. The loop is one-directional — a doctrine page legislates, a rule enforces; a rule never introduces law. Promotion path: an anti-pattern that is doctrine-breaking yet rejected by nothing in `tsc` or Biome is captured as an ast-grep rule with paired pass and fail fixtures before it gates; style preferences and anything an existing gate already rejects are not rules. Finding law transfers verbatim: true positive = architecture pressure (fix the shape), false positive = rule pressure (refine the rule), suppression is neither.

### (d) PAGE_CRAFT + CORPUS_LAW deltas

Inherit the C# sections verbatim with four deltas: (1) snippet fences are `typescript conceptual`; (2) compile-under-active-surface means tsc-clean under strict + `exactOptionalPropertyTypes` + `noUncheckedIndexedAccess` + `erasableSyntaxOnly` + `verbatimModuleSyntax`, pure ESM, against the Effect 3.x installed surface — Effect 4 idioms are excluded until the catalog flips; (3) manifest truth lives in `package.json` and the pnpm workspace catalog — no package-named pages, a package is named only where it changes the implementation choice; (4) the region ledger and three-layer inheritance transfer unchanged. STATE vocabulary is finalized/partial/target (C# benchmark, not the python active/planned drift).

### (e) ROADMAP tail entries

One entry per unauthored page; scope moves into the page when authored.

- `[SURFACES_AND_DISPATCH]` — owns entrypoint and dispatch architecture. Must decide: the dispatch-form chooser's exact boundary between `Match` and `$match`; the `Effect.fn` naming/trace law; where `satisfies`-closed tables beat union dispatch; the open-seam registry shape; the aspect ordering law for stacked Layer/middleware values.
- `[RAILS_AND_EFFECTS]` — owns carrier selection and the `E`/`R` interior. Must decide: `Data.TaggedError` vs `Schema.TaggedError` admission line; defect-promotion (`orDie`) placement; gen-vs-pipe selection law; the Schedule composition vocabulary; ExecutionPlan's position as the fallback owner; Ref-family selection.
- `[STREAMS_AND_CONCURRENCY]` — owns fiber lifetime and the Stream spine. Must decide: fork-variant admission (when `forkDaemon` is legal); the Queue/Mailbox/PubSub chooser; backpressure policy values; Stream error-rail law; FiberHandle/Map/Set registry law.
- `[BOUNDARIES]` — owns every non-Effect seam. Must decide: the admission matrix rows; AbortSignal bidirectional bridge; the runtime-root taxonomy (runMain vs ManagedRuntime vs Atom runtime); the foreign-library membrane law; ParseError conversion ownership.
- `[SYSTEM_APIS]` — owns the built-in replacement table. Must decide: the full row set; the native-stays-owner exceptions; the `crypto.randomUUID` admission row; Map/Set vs HashMap/HashSet line.
- One 2-4 line entry per lane (14), each stating owner packages + the key decisions listed in section 2 (e.g. `[CLUSTER]` carries the bullmq contingency clause; `[INTERACTION]` carries the server-cache ownership decision; `[DIAGNOSTICS]` carries the Otlp-vs-NodeSdk single-choice law; `[INFRA]` carries the pulumi-owns-its-own-graph boundary).
- `[TESTING]` — route row; owns proof rails (`@effect/vitest`, FastCheck/Arbitrary derivation from Schema, testcontainers, mutation gate) under the shared testing grammar.

## 4. SESSION SCOPE CUT

**Full authoring this session: README.md + language.md + shapes.md — the three-file foundation pass confirmed; the third core file (surfaces-and-dispatch) is NOT pulled in.** Argument: (i) finalization is a one-way gate requiring a cold-grade zero-edit pass — three pages plus cold grades saturates the session; (ii) surfaces-and-dispatch must be authored UNDER finalized shapes.md, and shapes' card vocabulary settles only at its cold grade — same-session authoring invites restatement drift; (iii) the banked research is shape-system research; the Match/`Effect.fn`/aspect surface is not yet verified at snippet depth.

**`.reports` research only this session:** `surfaces-and-dispatch.md` and `rails-and-effects.md` bedrock (the next two integration positions), so the next session opens with author-ready reservoirs.

**Atlas STATE at session end:** `language.md` finalized, `shapes.md` finalized; `surfaces-and-dispatch.md`, `rails-and-effects.md` target (bedrock banked in `.reports`, not reflected in STATE — research is workspace material, not page state); `streams-and-concurrency.md`, `boundaries.md`, `system-apis.md`, `testing/README.md`, `domain/README.md`, and all 14 lanes target. No page enters partial — partial is reserved for authored-but-rebuilt pages, and a greenfield corpus should never mint one.
