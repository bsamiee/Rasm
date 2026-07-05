# [AUDIT] libs/typescript/runtime — line-by-line doctrine grade

Scope: 33 design pages under `.planning/{proc,net,otel,serve,work,ai,browser}` graded against `docs/stacks/typescript` (README + language/derivation/values/computation/shapes/surfaces-and-dispatch/rails-and-effects/services-and-layers/concurrency/streams/boundaries, all read at source). The corpus is mature and at or near the 13/10 bar on the majority of pages; findings below are the residue an ultra-rebuild must still close.

## [00]-[THE SIXTEEN LAWS — STATED, WITH BEARING ON THIS FOLDER]

FLOW
- `EXPRESSION_SPINE` — everything rides `Effect<A,E,R>`; folds over `let`/`push`. Bears on report.md's `chunks.push` stream-collect and `_joined`'s comma-operator reduce, embed.md's spread-in-reduce `_packed`.
- `BOUNDARY_ADMISSION` — decode exactly once at the edge, `Option` for absence, no `null` interior. Broadly honored; boundary probes on `unknown` faults (problem.md, deliver.md `_classified`) are legitimate edge material.
- `CAPABILITY_CHANNEL` — every dependency is a Tag on `R`, satisfied by Layer. Exemplary corpus-wide; boot/exec/entity keep bindings to the root only.

SHAPE
- `SCHEMA_AUTHORITY` — Schema owner is the one shape authority; no interface/DTO beside it. Mostly held; the miss is plain-type receipts that cross a wire (life.md `Life.Report`).
- `SHAPE_BUDGET` — one deep owner ~1/5 the naive shape count; 3+ parallel shapes is the collapse trigger. Miss: flag.md's memo-probe shape restated 3×.
- `DEEP_SURFACES` — one/two exports, `_`-interior, terminal `[EXPORTS]` block. Honored across every page.
- `MODAL_ARITY` — one entrypoint discriminates on input value; no suffix families. Exemplary (persist `Kv`, client `dial`, surfaces `resolved`, streams `digest`).
- `ANTICIPATORY_COLLAPSE` — shape for the family, not the instance. Held; the fault families and vocabulary tables are all growth-sited.

DERIVATION
- `VOCABULARY_VALUES` — one `as const` table + guard pair derives every secondary surface; no Match over a keyed correspondence. Miss: deliver.md `_dispatch`, embed.md `_pieces`/`_folded`, flag.md `fetched` restate keyed tables as Match/ternary ladders.
- `DERIVED_TYPES` — `typeof`/`keyof typeof`/indexed access; no hand union parallel to a table. Held; recursive owners (flag `Rollout.Rule`) hand-state the decoded twin where inference cannot close it (sanctioned).
- `SEMANTIC_NAMING` — one canonical name per bounded concept; parallel labels rejected. **Primary structural miss: `Gate` (×3) and `GateFault` (×2) collide across owners** (see [FOLDER_VERDICT] #1). Also cli.md `_out2`.

MATERIAL
- `LIBRARY_DEPTH` — Effect ecosystem is the stdlib; JS `Map`/`Date`/native array methods only at the FFI seam. Recurring minor leak: native `.reduce`/`.filter`/`.map`/`.every` in embed/report/schedule.
- `INLINE_COMPOSITION` — policy attaches at the declaration seam. Held; the `Effect.fn` seam and `Schedule` values are used correctly where they appear.

INTEGRATION
- `ROOT_REBUILD` — weave new capability into the owner, no shims. Held.
- `ONE_HOP_RESOLUTION` — a name resolves in one hop; no re-export aliases. **Miss: work/flow `Step`/`Gate`/`Flow` and work/entity `Grid` collect bare package re-export aliases** (see [FOLDER_VERDICT] #4).
- `COMPOSED_IMPLEMENTATION` — features compose page owners before new scaffold. Exemplary; cross-page composition (`Client`, `Feed`, `Life`, `WorkClass`, `Budget`, `Transition`, `Residency`) is the corpus's strongest trait.

---

## [01]-[PROC]

### proc/exec.md — [RUNTIME_EXEC]
- `[2] Runtime.Core` (l.59-67): the row member types are `unknown` (`context`, `client`, `worker`, `runner`, `kv`). DERIVED_TYPES weakened — the guard proves presence but not shape, so a boot-module mis-wire (bun `serve` returning the wrong Layer) is not a compile error. Deliberate per stated law (guard constrains presence, not the binding's output union), but a tighter bound (`Layer.Layer<CommandExecutor|FileSystem|…>` for `context`, `Layer.Layer<HttpClient.HttpClient>` for `client`) would make the row honest without over-constraining. LOW.
- `[4] Proc.Spec` (l.123-133): `args?`/`env?`/`feed?`/`budget?`/`demand?` are `?:` optionals on a plain type. Acceptable as an interior single-consumer options block (never decoded), but the `capture` discriminant the overloads read (l.163-166) is not declared on `Spec` — it lives only in the overload intersections. A reader of `Proc.Spec` cannot see the modality axis. Fold `capture?: Capture` into the `Spec` type so the discriminant is on the value, not smuggled into three intersection types. LOW.
- Verdict: strong. Overload ordering (text→stream→Receipt→impl) is correct; `ExecFault.get class()` derives.

### proc/config.md — [RUNTIME_CONFIG]
- `[2] Provider.Stage` `Data.taggedEnum` (l.40-49): correct process-local case family. Good.
- `[4] SETTING_OWNER` (l.105-184): the five `_flag`/`_life`/`_fanout`/`_cluster`/`_mail` blocks are near-peer `Config.nested(Config.unwrap({...}))` structures printed in full. This is the real contract (scale fidelity), acceptable — but the `[5]` `Serve`/`_shaped`/`_tiers` demonstration re-instantiates the whole family form purely to "prove the fence." PAGE_CRAFT snippet-coverage: `[5]`'s `Serve` duplicates the `[4]` `Setting` demonstration region. Trim `[5]` to the row-vocabulary novelty (shaped scalar, `Config.literal` spread, `Config.redacted`) without a second full service. LOW.
- Verdict: strong; `_folded` empty-chain fold is honest.

### proc/flag.md — [RUNTIME_FLAG]
- `[6] GATE_SERVICE` memo-probe shape (l.415, 427, 454-460): the inline object `{ readonly flag; readonly key; readonly axes; readonly kind; readonly fallback }` is restated three times — `fetched`'s parameter, `Cache.makeWith` lookup's parameter, and the `Data.struct(...)` construction. SHAPE_BUDGET 3+ parallel shapes = collapse trigger. Lift to one `Rollout.MemoProbe` (or a `Schema.Class` since it feeds `Data.struct` keying) declared once. MODERATE.
- `[6] fetched` (l.417-423): `Match.value(probe.kind).pipe(Match.when("boolean")…Match.exhaustive)` over a kind→client-method correspondence. FORM_CHOOSER row [09]/COLLAPSE_SCAN [14]: a keyed static correspondence dispatches through lookup, not Match arms. The `_guards` record already keys by kind; a `_getters` handler record `{ boolean: (flag,fb,ctx)=>client.getBooleanDetails(...), … }` collapses the ladder. MODERATE.
- `[6] evaluate` kind computation (l.458): nested ternary `isBoolean?…:isString?…:isNumber?…:"object"` — a structural probe ladder restating the `_guards` keys; a `Match.type<Verdict.Json>()` with structural atoms or a shared `_kindOf` fold removes the restatement (it appears again inside `fetched`). LOW.
- `[6] reason cast` (l.439): `Array.contains<string>(Rollout.reasons, details.reason) ? (details.reason as Rollout.Reason) : "UNKNOWN"` — COLLAPSE_SCAN [10], an `as` after a runtime check `Array.contains` cannot narrow. A `Predicate`-shaped `_isReason` guard (or `Schema.is(Schema.Literal(...reasons))`) narrows without the cast. LOW.
- `[2] Rollout.Rule` decoded twin (l.85-90): hand-declared in the namespace, parallel to `typeof _Rule.Type`. For recursive `Schema.Union` this is the sanctioned hand-stated twin (shapes RECURSIVE_OWNER), but the encoded `_RuleEncoded` (l.62-72) AND the decoded `Rollout.Rule` are both hand-written — two hand twins where the recursive law sanctions one (encoded) and derives the other via `Schema.Schema<Rollout.Rule, _RuleEncoded>`'s first param. Accept as-is only if inference genuinely cannot close `.Type`; otherwise the decoded twin should derive. VERIFY.
- Verdict: rich and correct in the hard parts (recursive rule fold, deterministic bucket delegation, total `evaluate`); the dispatch-ladder and repeated-probe-shape are the cleanup.

### proc/life.md — [RUNTIME_LIFE]
- `[3]-[5] Life.Report`/`Life.Graded` (l.55-62): plain `type` receipts. But `route.md` `_health` (l.85-90) encodes `Life.report(kind)` to JSON with 200/503 status AND telemetry consumes the same rows — a wire crossing with 2+ consumers. shapes OWNER_FORMS row [01] → `Schema.Class` so the wire twin derives and the serving edge cannot skew the health body. Today the serving edge hand-serializes via `HttpServerResponse.json(report)`, which trusts the plain shape. MODERATE.
- Verdict: exemplary otherwise. `_bounded` collapses both registries to one executor; `_drainGrade`/`_probeGrade` are two graders of one evidence shape; memo record derives from `_KINDS`; the ranked-fold + severed-fiber discipline is model-grade.

### proc/worker.md — [RUNTIME_WORKER]
- Near-identical to boundaries.md's MARSHAL snippet — legitimate instantiation, not re-teaching. `PoolFault` carries `class: FaultClass.schema` (wire-crossing schema field). Good.
- `[3] _SIZING` (l.82-85): `elastic` row declared but only `fixed` consumed in `BenchLive`. Documents the modality (acceptable), but a fence that ships an unconsumed row invites drift; either consume it in a second stated lane or note it as the alternate the app selects. LOW.
- Verdict: clean, doctrine-exact.

---

## [02]-[NET]

### net/client.md — [RUNTIME_CLIENT]
- `[4] DISPATCH_ROWS` (l.116-127): `_dispatch` `as const` row is declared but no code applies it; the `RESEARCH` note admits the undici `Agent` member spellings are unverified. A dead policy row + a phantom application point. Honest deferral, but until the application member settles the row is non-load-bearing. TRACK.
- Verdict: strong. Lane table + pulse-from-Budget-ledger + `dial` overloads (feed vs decoded) are exemplary; three unwrapped fault families (`HttpClientError`, `Lapse`, `ParseError`), none re-wrapped.

### net/channel.md — [RUNTIME_CHANNEL]
- `[3] _hinted` (l.109): `declare const _hinted: (frame: Sse.Retry) => Duration.Duration` — a phantom body the reconnect fold (l.140) depends on; the `RESEARCH` note admits the `Sse.Retry` payload member is unverified. The Retry-driven reconnect (a load-bearing resilience path) is not realizable until this settles. MODERATE (incompleteness).
- Verdict: strong. `Duplex.framed` frame-row swap, `Feed` SSE session with cursor fold + `_PULSE` backoff, one codec owner for both directions.

### net/pubsub.md — [RUNTIME_PUBSUB]
- `[5] jetstream unknown-topic split-brain` (l.212): `js.publish(topics[topic]?.subject ?? topic, …)` silently falls back to using `topic` as the subject when the topic is absent from the table, whereas the local row (l.108-112) faults `horizon` on an unknown topic. Two engines handle the same error differently — a consumer swapping engines gets different behavior for an unknown topic. Make jetstream `publish`/`consume`/`replay` resolve the topic through the same `held`-style guard so unknown-topic → `horizon` on both engines. MODERATE.
- `[5] subscribe/replay body duplication` (l.217-231): jetstream `subscribe` and `replay` have identical bodies (`Stream.mapEffect(drained(...), ([e,msg])=>Effect.as(sync(msg.ack()), e))`), differing only by the anchor (`_Anchor.Window()` vs `anchor`). `subscribe(topic)` is `replay(topic, Window())` at the impl level. The port keeps them distinct semantically (fine), but the impl should share one `_acked(anchor)` helper rather than two copies. LOW.
- Verdict: strong. Engine-blind port, `Fanout.Anchor` `Data.taggedEnum` compiled to NATS start options via `$match`, correct `tryPromise`/`acquireRelease` at the NATS seam, honest local-degradation table.

---

## [03]-[OTEL]

### otel/emit.md — [RUNTIME_EMIT]
- `[2] POLICY` fence imports (l.25-28): three separate `effect` import lines (`import type {Duration,Redacted}`, then `import type {Layer}`) in one fence. verbatimModuleSyntax hygiene — one module, one import statement. LOW.
- `[2] Export.Policy` (l.32-52): an ~18-field plain type. Config-resolved value (produced by `Config.unwrap`), so a plain type is defensible, but `collector.headers: Redacted<string>` + `redaction: Redaction.Rules` are structured; a `Schema.Class` would let the `Config`-side and the wire-resource-side derive one twin. Borderline — accept if it is purely the `Config.unwrap` output shape. LOW.
- Verdict: strong. `_lanes` engine roster `as const satisfies`, `Export.live` one entry, `Redaction` one scrub owner two consumption sites, `Propagation` `Function.dual` ingress, DevTools fenced to `./dev`. The SDK-lane constructor block is correctly bracketed boundary material under the `[R3]` pin.

### otel/crash.md — [RUNTIME_CRASH]
- `[3] JSON.stringify breadcrumbs` (l.114): `[Convention.event.breadcrumb]: JSON.stringify(Chunk.toReadonlyArray(crumbs))` — a raw `JSON.stringify` at the log-annotation seam. Log annotations take string values, so this is a legitimate egress serialization, but a `Schema.encode` over a breadcrumb-array schema would keep the wire shape derived. LOW (annotation values are string-typed by the sink).
- Verdict: exemplary. Total `Cause`→fatal fold, `FaultEnricher` as `R` (compile-forced), `Match.instanceOf` foreign triage, `FiberSet.makeRuntime` net registration, redaction-at-capture.

### otel/vital.md — [RUNTIME_VITAL]
- `[3] _fact switch` (l.70-87): a `switch` over `entry.entryType`. Explicitly the marked admission kernel (Exemption line l.55), so the statement seam is sanctioned. Each arm extracts a different field with different guards (structural admission, not a pure keyed lookup) — Match.discriminatorsExhaustive would fit but the exemption covers the switch. ACCEPT.
- `[3] _accounted` (l.94-100): `{ crest, last, sum }[_rows[fact.kind].fold]` computes all three then indexes — a micro-inefficiency (computes two unused values per fact). A `_folds` handler record keyed by fold-name evaluated lazily would avoid it, but at fact rate this is negligible. LOW.
- Verdict: exemplary. Vocabulary table + one scoped `PerformanceObserver` bridge + `mapAccum` fold + two bounded instruments; grades derive from thresholds.

---

## [04]-[SERVE]

### serve/api.md — [RUNTIME_API]
- `[5] Idempotency.Claim` (l.321-324): hand-written `{ _tag: "Fresh" } | { _tag: "Replay" }` union, dispatched by `_tag`, process-local. shapes OWNER_FORMS row [04] → `Data.taggedEnum`. The generic `Claim<A>` rides `Data.TaggedEnum.WithGenerics<1>` cleanly. MODERATE.
- `[5] _Cell / outcome cast` (l.281, 311): `Deferred.Deferred<unknown>` erases the claim type, forcing `outcome as A` on replay (l.311). A heterogeneous idempotency store cannot type its slots, so the cast is structural — but the store keyed by `Schema.TaggedRequest` identity (as `PersistedCache`/`RequestResolver.persisted` do) would type it. Accept for the in-memory tier; note the store-backed tier should type through the request schema. LOW.
- `[2] CONVENTION fence import block` (l.26-38): front-loads the whole module's imports (RateLimiter, Deferred, HashMap, Ref, Redacted) into the CONVENTION snippet that only uses Schema/Option — the fence carries unused imports and fails snippet-law "type-checks under the strict set" (noUnusedLocals-adjacent). Split imports per fence or the page is one fence. LOW.
- Verdict: strong. `Gate.shed`/`Gate.window` correctly kept separate (concurrency vs throughput is the named non-conflation), `Authn` scheme-threaded middleware, `Current` `Context.Reference` rows, `GateFault` reason table. **NB: `GateFault`/`Gate` name-collision is the folder-level finding.**

### serve/route.md — [RUNTIME_ROUTE]
- `[3] _octets` (l.81-83), `[4] _asserted`/`_enrolled`/`_cleared` (l.146-158): four `declare const` phantom bodies behind two `RESEARCH` notes (raw-octet accessor; passkey response admission schema). The webhook `Intake` verify seam (load-bearing security) and the WebAuthn ceremonies are not realizable until these settle. MODERATE (incompleteness, several load-bearing).
- `[5] _assets` traversal check (l.243): `clean.includes("..")` is a substring test, not a resolved-prefix check — an encoded/normalized `..` or an absolute-path escape could slip. Prefer resolving `path.join` then asserting the result is under `options.root`. Defense-in-depth; the SPA-fallback softens it, but the check is weaker than the prose ("structurally refused") claims. LOW-MODERATE.
- Verdict: strong. `Seam.guard` one middleware composition, `_SHIELD` header table, `Router` route-Layer vocabulary, `Problem.net` floor. CORS delegated not re-implemented.

### serve/live.md — [RUNTIME_LIVE]
- `[3] SSE encoder RESEARCH` (l.81): `Sse.encoder.write` member spelling unverified; `_encoded` (l.98-99) depends on it. The emit side of the one-codec-owner law is not realizable until settled. TRACK.
- Verdict: exemplary. `Realtime.Source` resumable-feed contract, `LiveFault` reason table, `Admission.make` Trie prefix rules + `FiberMap` per-connection fan registry, one `Stream.mapError`/`Channel.mapError` normalization seam. `_query`/`_topic`/`_roster` adapters unify three feed families into one contract.

### serve/problem.md — [RUNTIME_PROBLEM]
- `[5] _of ladder` (l.171-180): a nested-ternary `instanceof Problem ? … : isTagged("FaultDetail") ? … : isTagged("ParseError") ? … : isTagged("RouteNotFound") ? … : _classed`. A `Match.type<unknown>()` pipeline with `Match.when(Predicate.isTagged(...))` arms + orElse residue is the FORM_CHOOSER row [08]/[13] form and reads the ladder as arms rather than a hand ternary. The ordering-by-evidence-specificity is load-bearing and a Match preserves it. LOW.
- Verdict: exemplary. `Problem` `Schema.Class` implementing `HttpServerRespondable.symbol`, `_rows` governed `Record<FaultClass.Kind,_Grade>` (compile-forced on core growth), blame-derived exposure, `Cause` interrupt-first fold. The self-rendering-first net is the model for "faults leave only as self-rendering Problems."

### serve/cli.md — [RUNTIME_CLI]
- `[2]/[5] _out2` (l.88): numeric-suffix name to dodge collision with `_out` (the render seam). SEMANTIC_NAMING rejects suffix families; rename `_outFlag`/`_target`. LOW.
- Verdict: exemplary. `Verb.main` clean-exit rail (the genuine lib value over `Command.run`), `Ops.family` runbooks with `Effect.partition` accumulate-doctor, `Render` role-table + structure-rows + ambient-mode seam. All `Doc<Ansi>` composition, one render seam.

---

## [05]-[WORK]

### work/entity.md — [RUNTIME_ENTITY]
- `[5] GRID runner entries vs ARCHITECTURE boundary` (prose l.161): the runner entry names `NodeClusterHttp.layer`/`NodeClusterSocket.layer` / `BunClusterHttp`/`BunClusterSocket`. ARCHITECTURE.md [04] freezes `@effect/cluster-node` (and `-browser`, `-workflow`, `@effect/rpc-http`) as "never admitted." These `*Cluster*` runner layers are cluster-node/cluster-bun package exports. Either the boundary is contradicted, the member names are wrong, or the runners live in a non-frozen package that must be named. **Resolve before build.** HIGH (doctrine-vs-design contradiction).
- `[5] Grid bare aliases` (l.195-200): `singleton: Singleton.make`, `engine: ClusterWorkflowEngine.layer` are pure package re-exports collected under `Grid`. ONE_HOP_RESOLUTION / COLLAPSE_SCAN [22]. `Grid.layer` and `Grid.census` add value; the two aliases should drop (consumers use `Singleton.make`/`ClusterWorkflowEngine.layer`, or the aliases earn a domain-value wrap). MODERATE.
- Verdict: strong otherwise. `WorkClass` assembled vocabulary (the three-table collapse point), `Actor.make` mint, `Mailbox._bridge` governed `ClusterError→FaultClass` record.

### work/flow.md — [RUNTIME_FLOW]
- `[2]/[4]/[5] bare package aliases` (l.59-64, l.99-104, l.159-165): `Step.race = Activity.raceAll`, `Step.attempt = Activity.CurrentAttempt`, `Step.key = Activity.idempotencyKey`, `Flow.make = Workflow.make`, `Flow.fromTaggedRequest = Workflow.fromTaggedRequest`, `Flow.engineSpec = WorkflowEngine.layerMemory`, `Gate.declare = DurableDeferred.make`, `Gate.token.fromPayload = DurableDeferred.tokenFromPayload`, `Gate.settle.succeed = DurableDeferred.succeed` (etc.). Pure renames, zero domain value — the exact thin-wrapper ONE_HOP_RESOLUTION deletes. `Step.run`, `Gate.hold`, `Gate.pause` genuinely add value and stay. The aliases should drop; consumers reach the package surface directly. MODERATE (heaviest instance of the pattern).
- `[3] _verdict exit read` (l.92): `complete.exit._tag === "Success"` reads the Exit discriminant by `===` rather than `Exit.match`/`Exit.isSuccess`. Minor; a fold is more honest. LOW.
- Verdict: strong core (`Step.run` budget geometry, `FlowVerdict` generic `Data.taggedEnum`, saga fold, `Gate.hold` race-not-poll). **NB: `Gate` name-collision (see folder verdict).**

### work/queue.md — [RUNTIME_QUEUE]
- Verdict: exemplary. `LaneVerdict` `Data.taggedEnum`, `Lane.settle` `Match.tag`/`exhaustive` fold over the outbox, dead-letter-as-evidence on the fact rail, `Throttle` `as const satisfies` quota rows, `Job.of` class-priced worker. No naive SQL — rides `Journal.claimBatch`/`Journal.complete`.

### work/schedule.md — [RUNTIME_SCHEDULE]
- `[3] _missed native methods` (l.56-61): `Array.fromIterable(Iterable.takeWhile(...)).filter(...)` mixes native `.filter` with `Array.fromIterable`; LIBRARY_DEPTH prefers `Array.filter`. `Array.takeRight(ticks,1)` is correct. LOW.
- Verdict: strong. `Cadence.Anchor` discriminated pair encoding the clock/previous↔catchUp orthogonality (ADT does the documentation), `_cluster`/`_host` two engines over one row table, catch-up as a computed prefix not a loop.

### work/deliver.md — [RUNTIME_DELIVER]
- `[6] _dispatch Match restates _channels` (l.265-274): `Match.value(kind).pipe(Match.when("mail", k=>_transmitted(k,_channels.mail,claim)), Match.when("webhook", …), Match.exhaustive)` — the `_channels` table is already keyed by channel; the Match arms restate its keys. COLLAPSE_SCAN [14]. A mapped-signature indexed dispatch `_transmitted(kind, _channels[kind], claim)` removes the ladder (the arms exist only to narrow the literal for `_transmitted`'s generics — a mapped `{ [K in Channel]: … }` handler record types it cast-free). MODERATE.
- `[6] _channels satisfies unknown` (l.245-254): `as const satisfies Record<Receipt["channel"], unknown>` — the `unknown` value type checks only the key set, forfeiting the `Deliver.Channel` shape check that would catch a malformed row. The channels carry differing type params, but a `satisfies Record<Receipt["channel"], Deliver.Channel<any, any, any>>` (or a mapped contract) restores the shape gate. LOW.
- Verdict: strong. One channel table + one `Receipt` + one `DeliverFault` reason table + one suppression fold, `Relay` singleton draining under queue's verdict vocabulary, byte-identity webhook signing, native DKIM. The formerly-twice-owned transport convention is genuinely spelled once.

### work/report.md — [RUNTIME_REPORT]
- `[2] RenderFault.class hardcoded` (l.41): `readonly class: FaultClass.Kind = "defect"` — a stored constant for ALL four reasons (`engine`/`sink`/`archive`/`slip`), unlike every other fault family in the branch which derives `class` from a `_reasons` table via `get class()`. Consequence: a zip-slip traversal (`slip`, l.250) classifies as system-blamed 500-defect instead of a caller-blamed `denied`/`malformed`; a transient `sink` fault cannot route as retryable. Add a `_reasons` governed record (`engine→defect`, `sink→unavailable`, `archive→defect`, `slip→malformed`) and a `get class()`. MODERATE (correctness + branch-consistency).
- `[3] _xlsx chunk collect` (l.104-105): `const chunks: Array<Uint8Array> = []; sink.on("data", c=>chunks.push(c))` — hand stream-collection via native push at the PassThrough seam. EXPRESSION_SPINE's mutable-accumulation form; `NodeStream.fromReadable`/`Stream.fromReadable` collected through the rail owns this. Boundary-adjacent but not marked as an exemption. LOW-MODERATE.
- `[5] _joined` (l.207-212): native `.reduce` twice + a comma-operator side-effect `(joined.set(chunk,offset), offset+chunk.length)`. Works, but the comma-operator mutation is a code smell; `Array.reduce` + explicit offset fold reads cleaner. LOW.
- Verdict: strong architecture (one `Report.Spec` fold, `_render` Match dispatch on format, three streaming engine arms, byte reproducibility, jszip streaming egress with zip-slip guard). The fault-class regression is the one real defect.

---

## [06]-[AI]

### ai/model.md — [RUNTIME_MODEL]
- `[4] GateFault naming` (l.125): `class GateFault extends Data.TaggedError("ModelRefusal")` — the export name `GateFault` collides with serve/api's `GateFault` AND the export name (`GateFault`) does not match the tag (`ModelRefusal`). SEMANTIC_NAMING: one canonical name per concept; export/tag mismatch obscures the catch key. Rename to `Refusal`/`GuardrailFault` (tag-matched). MODERATE (part of the folder `Gate`/`GateFault` collision).
- `[3] _plan` (l.99): `declare const _plan` — RESEARCH phantom (`ExecutionPlan.make` step-record field names unverified). The tiered-failover ladder (the page's core resilience mechanism) is not realizable until settled. MODERATE (load-bearing).
- `[5] _meters heterogeneous rows` (l.208-211): `anthropic: Layer.succeed(...)` (a Layer value) vs `openai: (model)=>OpenAiTokenizer.layer({model})` (a factory) — the row shapes differ (value vs function). The asymmetry is real (bare service vs keyed factory) and documented, but a consumer indexing `_meters[name]` gets `Layer | ((model)=>Layer)` and must know which. A uniform `(model?)=>Layer` shape (anthropic ignoring the arg) makes the table indexable. LOW.
- `[4] Gate.object skips content sweep` (l.175-176): `object` arm applies no output sweep beyond the caller Schema, while `text`/`stream` sweep. The GATE prose claims "output sweep runs last… modality-uniform," but structured output is the sweep only for schema-shape, not content policy (a Schema-valid object can still contain swept content). Either the claim narrows ("schema IS the object sweep") or object gains a content sweep over stringified fields. LOW (stated as intentional, but the uniformity claim overreaches).
- Verdict: strong. Provider asymmetry table, `Ladder`/`Gate`/`Tokens`/`Spend` owners, `Refusal` `Data.taggedEnum`, `BigDecimal` spend fold (no float money), measured greedy `Tokens.weave`.

### ai/embed.md — [RUNTIME_EMBED]
- `[2] _packed O(n²) spread` (l.30-36): `spans.reduce(...[...packed.slice(0,-1), {...}])` rebuilds the array each step via spread — quadratic over span count. Use `Array.reduce` with a `Chunk`/mutable-in-fold-then-freeze or a proper append that does not re-spread the prefix. LOW-MODERATE (real perf on large corpora, the page's stated scale).
- `[2] _pieces ternary` (l.71) + `_folded` (l.150-154): `lane.kind==="fixed"?…:…:…` restates `_lanes` keys; `_folded` uses `fault._tag==="MalformedInput"||…` string comparisons restating a fault-bridge that Mailbox does as a governed record. Both are keyed correspondences done as ladders (COLLAPSE_SCAN [14]); a `_lanes[lane.kind]` dispatch (with narrowing) and a `_folded` bridge table align with the branch pattern. LOW.
- `[2]/[4] native array methods` (l.35, 61, 73-81, 168): `.reduce`/`.map`/`.filter`/`.every` on plain arrays in domain flow; LIBRARY_DEPTH prefers `Array.*`. Recurring. LOW.
- Verdict: strong. Determinism anchor (`_scrubbed` NFC once), `Piece` `Schema.Class`, two-tier cache, `Embedder`/`Reranker` port satisfaction, fingerprint brand.

### ai/tool.md — [RUNTIME_TOOL]
- Verdict: exemplary. `Safety` blast-radius partition (the one admission), `Arsenal` provider-defined-tool ledger, `Host` native MCP / `Remote` SDK-client duality with the hard `./server`-subpath boundary, fail-closed grading (absent hint → `destroy`/`destructive:true`), `ToolFault` reason table. Model page for "one owner over the real SDK."

### ai/agent.md — [RUNTIME_AGENT]
- `[5] two `Gate` imports collide` (Packages l.139 + l.22, 28): the page imports `Gate` from `./model.ts` (guardrail) AND `Gate` from `../work/flow.ts` (durable deferred) into one module — a compile collision requiring an alias the page does not spell. This is the concrete failure the `Gate`-collision finding predicts. **The page cannot compile as written** until one `Gate` is aliased or renamed. HIGH (concrete, in-file).
- `[5] Agent object redundant re-exports` (l.153-161): `Agent.act = Act`, `Agent.turn = Turn` re-collect classes that are also directly exported (l.165). ONE_HOP redundancy — a consumer reaches `Act` two ways. Drop the aliases or drop the direct exports. LOW.
- Verdict: exemplary machine work — `Transition.spec` data-carried statechart (hierarchical phases, guarded transitions, `watch` timer self-signal, `recover` schedule) booted as the in-process serializable actor, `Act`/`Turn`/`AgentFault` `Schema.TaggedRequest` triple serving three surfaces, held-tool approval as evidence with in-process + durable release paths. This is the "machines at research-paper depth" the campaign demands. The `Gate` collision is the one blocking defect.

---

## [07]-[BROWSER]

### browser/boot.md — [RUNTIME_BOOT]
- Verdict: exemplary. `AppSpec` `Schema.Class` (identity composed at full depth, lane axis governed both ways by `_Spans`), `Boot` Tag single-boot + shared-`MemoMap` handle, `Connect` signal cells (read-only `Subscribable`, owned folds, `Stream.zipWithPrevious` edges, closed `_GRADES` network vocabulary), `Capability` Web-API roster. Boundary refinements (`_NetSource`/`_SyncHost`) pinned at the one owner.

### browser/shell.md — [RUNTIME_SHELL]
- `[4] BroadcastCacheUpdate RESEARCH` (l.148): member spellings unverified — the cache-update-announce column is deferred off the row table. Non-load-bearing (a growth column). TRACK.
- Verdict: exemplary. `Manifest` `Schema.Class` with `fromKey` renames (the wire-twin model), `SwLifecycle` `Data.taggedEnum`, `_PHASES` keyed lookup (explicitly rejecting the Match ladder), `Sw` scoped resource with `// BOUNDARY ADAPTER`-marked event bridge, `REPLAY_DRAIN` merged-wake fold with atomic re-enqueue, `Install` prompt-capture owner. `SwFault`/`InstallFault` reason tables.

### browser/persist.md — [RUNTIME_PERSIST]
- `[3] _service overload-via-cast` (l.176-185): the outer `Kv` service `read`/`write` are const arrows with `as { <overloads> }` casts, where surfaces-and-dispatch OVERLOAD_COLLAPSE prefers the `function` declaration form to avoid the cast. The inner `_lane` correctly uses `function`; the outer delegate re-wraps with arrow+cast because it closes over `lanes`. A `function read<D>(...)` closure over `lanes` avoids the cast. LOW.
- Verdict: exemplary — the model MODAL_ARITY page. `Kv` one polymorphic lane (point/batch read, atomic-batch write, in-transaction mutate, drop, atomic drain, wipe) domain-generic over `_domains` via mapped-contract `_lanes` and indexed dispatch; codec seam at every boundary; `KvFault`/`Opfs` reason tables; `Overlay` EventLog backings; the one `idb-keyval` site.

### browser/route.md — [RUNTIME_ROUTE]
- `[2] RouteFault single reason` (l.151): `reason: "unsupported"` is the only value; a `reason` field with one inhabitant adds nothing today (the `get class()` ignores it). Consistent with the pattern and growth-ready, but note it is a degenerate reason table. LOW.
- Verdict: exemplary (largest, and among the best). `_Names<Path>` template-literal segment derivation, `Router.make` minted Tag + one-commit intercept, `Vault` session plane (`FiberHandle` supersede-keyed refresh, `BroadcastChannel` cross-tab, one sanctioned `document.cookie` CSRF read, OAuth continuity over `persist` flow domain), `Guard` admission `Effect.reduce` chain (first-refusal-wins, `Authenticating` waited-not-guessed). All casts `// BOUNDARY ADAPTER`-marked and one-directional. `SessionStatus`/`FlowFault`/`Router.Admission` `Data.taggedEnum`.

### browser/fetch.md — [RUNTIME_FETCH]
- Verdict: exemplary. `Web` binding rows, `_flows` per-class policy (frugal downshift as one multiplier, in-stream ceiling via `mapAccumEffect`), `Fetch` decorated dial composing (never forking) `net/client`, `Pool` worker protocol (`Transferable` zero-copy, `Schema.TaggedRequest` union, `PoolFault` reason table with evidence), `Depot` residency scheduler (`Effect.partition` quarantine, warm-before-fetch with parity-only eviction, degree from `Opfs` verdict), `RUNNER_ENTRY` worker boot delegating the content mint to core `Digest`. The kernel-delegation and ULTRA-stacking of `core/interchange` surfaces is the corpus's best cross-wave composition.
- RESEARCH: XHR progress-event members unverified (l.105) — the progress modality is deferred. TRACK.

---

## [FOLDER_VERDICT] — deepest structural weaknesses, ranked by leverage

1. **Cross-corpus name collisions: `Gate` (×3), `GateFault` (×2).** `Gate` names three unrelated owners — `serve/api` (HTTP admission), `ai/model` (LLM guardrail), `work/flow` (durable signal). `agent.md` imports two `Gate` symbols into one module and **cannot compile as written**. `GateFault` collides between `serve/api` (tag `GateFault`) and `ai/model` (export `GateFault`, tag `ModelRefusal` — export/tag also mismatched). Violates SEMANTIC_NAMING ("one canonical name per bounded concept"). FIX: bounded-context names — e.g. `serve` keeps `Gate`/`GateFault`; `ai/model` → `Guardrail`/`Refusal` (already has `Refusal`); `work/flow` → `Signal`/`Await`. Highest leverage because it is a live compile break plus a corpus-wide clarity defect.

2. **ARCHITECTURE-vs-GRID contradiction on cluster runner packages.** ARCHITECTURE.md [04] freezes `@effect/cluster-node`/`-browser`/`-workflow` and `@effect/rpc-http` as never-admitted; exec.md and entity.md GRID name `NodeClusterHttp.layer`/`NodeClusterSocket.layer`/`BunClusterHttp`/`BunClusterSocket` runner entries, which are cluster-node/cluster-bun exports. Either the boundary is wrong, the member names are wrong, or the runners live in a package that must be named. Blocks any implementation of the durable-actor plane. Resolve first.

3. **`RenderFault` (report.md) breaks the branch-wide reason→class derivation.** Every fault family in the folder derives `class` from a `_reasons` table via `get class()`; `RenderFault` alone hardcodes `class = "defect"` for all reasons, misclassifying a zip-slip traversal (`slip`) as a system 500 and denying retryability to transient `sink` faults. A one-table fix restores both correctness and consistency.

4. **Thin-wrapper re-export aliases in `work/flow` and `work/entity`.** `Step.race`/`Step.attempt`/`Step.key`/`Flow.make`/`Flow.fromTaggedRequest`/`Gate.declare`/`Gate.token.*`/`Gate.settle.*`/`Grid.singleton`/`Grid.engine` are pure package renames — the exact hop ONE_HOP_RESOLUTION / COLLAPSE_SCAN [22] deletes. Value-adding members (`Step.run`, `Gate.hold`, `Grid.layer`) stay; the aliases drop.

5. **Match/ternary ladders restating keyed tables (5+ sites).** deliver.md `_dispatch` over `_channels`, embed.md `_pieces`/`_folded`, flag.md `fetched`/`evaluate` kind-probe — all restate keys a table already carries (COLLAPSE_SCAN [14]). Collapse to indexed/handler-record dispatch. Recurring but low-risk; a consistency sweep.

6. **Missing circuit-breaker resilience on egress/backend owners.** Campaign law (6) demands "circuit breakers, bulkheads, hedging, load-shed, adaptive retry" ride every cache/store/backend/egress owner. Present: bulkheads (`makeSemaphore`/`PartitionedSemaphore`), hedging (`Step.race`, `Effect.raceAll`, model `Ladder`), load-shed (`Gate.shed`, buffer `sliding`/`dropping`), adaptive retry (`Schedule`/`Budget`). ABSENT: an explicit circuit breaker (failure-rate → open/half-open/closed) on `net/client`'s lane table, `net/pubsub` egress, and `work/deliver`'s webhook/mail egress. `model.md`'s `ExecutionPlan` ladder is breaker-adjacent (tier yields on retryable class) but per-provider, not a breaker over the shared client. Add a breaker owner (Ref + Schedule state machine, or a `Transition` machine reusing agent.md's owner) composed into `Client.dial` and the delivery/fanout dials.

7. **Plain-type receipts that cross a wire.** `Life.Report`/`Life.Graded` are plain types the serving edge encodes to JSON (200/503) and telemetry consumes — 2+ consumers + wire crossing → `Schema.Class` (shapes OWNER_FORMS [01]) so the health-body twin derives instead of being hand-serialized. Isolated; most receipts (`Fanout.Receipt`, `Deliver.Receipt` is already a `Schema.Class`, worker/proc receipts) are correctly process-local.

8. **`declare const` phantom stubs behind RESEARCH markers (load-bearing subset).** channel `_hinted` (SSE reconnect hint), serve/route `_octets`/`_asserted`/`_enrolled`/`_cleared` (webhook verify + OAuth/WebAuthn ceremonies), model `_plan` (tiered failover), live SSE `encoder` spelling. These are honest catalogue-pending deferrals, but several gate load-bearing resilience/security paths — they are the settle backlog a build must close before the pages are realizable.

9. **`Idempotency.Claim` hand-written union → `Data.taggedEnum` (serve/api).** Isolated shapes-doctrine miss; the generic `Claim<A>` rides `WithGenerics<1>` cleanly.

10. **Native stdlib array methods + one O(n²) spread (embed `_packed`).** LIBRARY_DEPTH leaks in embed/report/schedule; the `_packed` spread-in-reduce is the only one with a real perf cost at the page's stated scale.

Non-findings worth recording (strengths, so the rebuild does not "fix" them): no naive SQL anywhere (SqlSchema/SqlResolver/Journal rails throughout); the agent `Transition` machine is research-grade and reused for the entity escalation; pubsub is a genuine one-port-many-engines surface; the streaming/backpressure discipline (`asyncScoped`, `mapAccumEffect` ceilings, `buffer` posture rows, `Take` handoffs) is doctrine-exact; `persist Kv`, `browser route`/`fetch`, `problem`, `tool`, `worker`, `life`, `boot` are exemplary and should anchor the rebuild rather than be reshaped.
