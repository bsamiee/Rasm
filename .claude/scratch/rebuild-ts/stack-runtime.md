# [STACK_RUNTIME] — ultra-stacking dossier for libs/typescript/runtime

Findings-only. The corpus is already exceptionally dense (all 31 pages at or near the 13/10 bar); the yield here is
narrow-but-load-bearing: settled RESEARCH the improver closes, admitted-but-unwired packages the folder concept demands,
package×package compositions the corpus never attempts, and the telemetry hook-plane the THOUSANDS-OF-APPS mandate requires.
All member spellings verified against the folder/branch `.api` catalogs and the ruling prefetch dossiers
(`prefetch-machines-pubsub.md` [B/C/D], `prefetch-remote.md`). Every "RESEARCH" note in a page that a prefetch already
resolves is flagged as **CLOSED-GROUND** — apply, do not re-research.

---

## [0]-[HEADLINE_DEFECTS] — correctness, not polish (fix before any stacking)

These are ruled defects from `prefetch-machines-pubsub.md` [B2]/[C1]; they are wrong-as-written, not underutilization.

- **`net/pubsub.md` [5] JETSTREAM_ROW `consume` ack is a no-op** ([B2] #1, HEADLINE). `drained` mints the consumer via
  `js.consumers.get(topic, _start(anchor))` — a NAMELESS ORDERED consumer hardcoded to `ack_policy: AckPolicy.None`
  (`jetstream/src/consumer.ts:1019`). Every `msg.ack()` / `msg.nak()` / `msg.ackAck()` in `consume`/`subscribe`/`replay`
  is a NO-OP: no redelivery, no at-least-once, no double-ack — the page's central "ack-after-success" law is unenforced.
  Fix: split the kernel. Ordered lane (`subscribe`/`replay`, no ack) keeps `js.consumers.get(stream, orderedOptions)`.
  Durable lane (`consume`, real ack) becomes `jsm.consumers.add(stream, { durable_name, ack_policy: AckPolicy.Explicit,
  ack_wait, max_deliver, deliver_policy, opt_start_seq/opt_start_time })` then `js.consumers.get(stream, durable_name)`.
- **`net/pubsub.md` [5] Window anchor delivers from stream-start, not "new"** ([B2] #4). `_start` returns `{}` for
  `Window`, defaulting the ordered consumer to `DeliverPolicy.All` (from the beginning) — contradicting the PORT_SHAPE
  law's "Window → new deliveries". Fix: `Window: () => ({ deliver_policy: DeliverPolicy.New })`.
- **`net/channel.md` [3] `_hinted` / `Sse.Retry` premise is wrong** ([C1], CLOSED-GROUND). The declared `_hinted:
  (frame: Sse.Retry) => Duration.Duration` and the "millisecond payload member … unverified" RESEARCH note are both
  wrong: `Sse.Retry` is `class Retry extends Data.TaggedClass("Retry")<{ readonly duration: Duration.Duration;
  readonly lastEventId: string | undefined }>` — no ms field exists. `_hinted` collapses to reading `frame.duration`
  directly (delete the function, use `frame.duration`). Additionally thread `frame.lastEventId` into the cursor `Ref`
  (currently ignored) so a `retry:` frame carrying a last-id advances reattach. Delete the RESEARCH block.
- **`serve/live.md` [3] `Sse.encoder`/`Sse.Event` RESEARCH is CLOSED-GROUND** ([C1]). `Sse.Event` is exactly
  `{ _tag: "Event"; event: string; id: string | undefined; data: string }` (event-name field is `event`, NOT `type`),
  `Sse.encoder: Encoder` with `.write(event)` confirmed, `Sse.makeChannel<IE, Done>(options?: { bufferSize?: number })`
  confirmed. The page's `_BEAT`, `_encoded`, and `new Sse.Retry({ duration, lastEventId })` all match — delete the
  RESEARCH note, the fence is settled.

---

## [A]-[UNDERUTILIZED_MEMBERS] — admitted, catalogued, exact spelling, owning page

Members present in an admitted package's `.api` catalog but never consumed by any page; each row names the landing page.

### @effect/cluster (`entity.md`, `schedule.md`, `deliver.md`)
- `DeliverAt.DeliverAt` (`toMillis` delivery-time interface) — `schedule.md` [2] explicitly says "a one-shot delayed
  job … is a `DeliverAt` payload on the actor plane" but NO page wires it. Land in `entity.md` ACTOR_MINT as a
  scheduled-message modality (an `Actor` message annotated `DeliverAt` delivers at its `toMillis` instant), giving the
  actor plane native delayed delivery the schedule page currently defers to.
- `Entity.Replier<Rpcs>` (`succeed`/`fail`/`failCause`/`complete(Exit)`) — the out-of-band reply surface for
  streaming/mailbox handlers. `entity.md` ACTOR_MINT names `toLayerMailbox` as "the streaming-batch escape hatch" but
  never surfaces the `Replier` it hands. Land as the mailbox-drain reply seam in ACTOR_MINT.
- `Entity.CurrentAddress` / `Entity.CurrentRunnerAddress` (in-handler context Tags) — the running entity's
  `EntityAddress` and host `RunnerAddress`. Land in `entity.md` MAILBOX/GRID as the evidence a handler stamps onto its
  span (which shard, which runner) — currently the census only reports registration events, not per-message locality.
- `ClusterSchema.Uninterruptible` / `ClusterSchema.ClientTracingEnabled` — only `Persisted` + `ShardGroup` are used.
  `Uninterruptible` (`boolean | "client" | "server"`) is the interrupt-policy row a critical actor message needs;
  `ClientTracingEnabled` toggles the client span. Land in `entity.md` ACTOR_MINT as annotation rows on `Actor.Spec`.
- `ShardingRegistrationEvent.match` (fold over `EntityRegistered | SingletonRegistered`) — `Grid.census` streams the
  events raw; the `match` fold is the typed projection. Land in `entity.md` GRID as the census-row shaping.
- `ClusterMetrics.*` — the cluster's own metric surface (mailbox depth, message rate); `entity.md` MAILBOX supervises
  drain via a bespoke `Life` probe meter. Land as the standard metric source the supervision reads.
- `EntityProxyServer` — `flow.md`/`entity.md` use `EntityProxy` but not the server binding; land where a proxied entity
  needs its own served handler set (WorkflowProxyServer parallel).
- `HttpRunner.layerWebsocket` / `RunnerServer.layerWithClients` — `entity.md` GRID lists `NodeClusterHttp/Socket` only;
  the websocket runner transport is a growth row for the runner entry table.

### @effect/workflow (`flow.md`, `queue.md`, `schedule.md`)
- `Workflow.intoResult` / `wrapActivityResult` / `suspend` / `provideScope` — `flow.md` FLOW_LAW exposes
  `make`/`fromTaggedRequest`/`verdict` but not the result-lifting and explicit-suspend controls. `suspend` is the
  first-class "park this run" verb the SAGA/GATE folds imply but never name; land in `flow.md` FLOW_LAW.
- `WorkflowEngine.WorkflowInstance` (per-run context Tag: executionId, scope, suspended/interrupted flags, cause) —
  the durable-instance handle a body reads for its own run state. Land in `flow.md` STEP_MINT as the attempt/run
  evidence beside `Activity.CurrentAttempt`.
- `DurableDeferred.into(effect)` — binds an effect's result to the deferred (vs. out-of-band `succeed`/`fail`).
  `flow.md` SIGNAL_GATE only surfaces the out-of-band settle path; `into` is the in-band bind for a gate resolved by a
  sibling activity's completion. Land in `flow.md` SIGNAL_GATE.
- `WorkflowProxyServer` — `flow.md` WIRE_PROXY declares it in Packages but `_contribution` uses only `WorkflowProxy`;
  the server binding is the actual served handler set. Land in WIRE_PROXY.

### @effect/rpc (`api.md`, `entity.md`)
- `RpcClient.withHeaders(effect, headers)` / `RpcClient.currentHeaders` (FiberRef<Headers>) — `api.md` EMIT
  `Emit.caller` derives `RpcClient.make(group)` but never composes per-call headers. This is the seam for W3C trace
  propagation on RPC egress (see [C2]). Land in `api.md` EMIT.
- `Rpc.fork` / `Rpc.uninterruptible` (`Rpc.Wrapper`) — concurrent / uninterruptible handler-response markers. A
  fire-and-forget entity message or a must-not-interrupt settle needs these. Land in `entity.md` ACTOR_MINT /
  `api.md` CONTRIBUTION.
- `Rpc.make({ primaryKey })` — request-dedup key on a procedure. `api.md` CONTRIBUTION mints procedures but never sets
  `primaryKey`; the entity mailbox dedup rides `Snowflake`+primary-key — surface `primaryKey` on the RPC row.
- `RpcServer.layerProtocolSocketServer` — the raw-socket transport row (`api.md` CONTRIBUTION `_protocols` has
  http/websocket/worker/stdio but not socket-server). Add as one `_protocols` row.
- `RpcWorker.layerInitialMessage(schema, build)` — typed worker handshake for the `worker` protocol row. Land in
  `api.md` CONTRIBUTION where the worker RPC transport boots.
- `RpcServer.toHttpApp(group)` — the `HttpApp` value form (vs. `toWebHandler`) for mounting an RPC group beside raw
  routes on one `HttpLayerRouter`. Land in `route.md` LAYER_ROUTES / `api.md` EMIT.

### @effect/ai (`model.md`, `tool.md`, `agent.md`, `embed.md`)
- `Telemetry.addGenAIAnnotations(span, opts)` + `WellKnownSystem` + `Telemetry.CurrentSpanTransformer` — the standard
  GenAI-semconv attribute writer (`gen_ai.system`, `gen_ai.operation.name`, `gen_ai.usage.*`). `model.md` GATE/LADDER
  annotate spans MANUALLY with provider name; the standard writer is the correct, exporter-portable bridge into the
  otel plane. Land in `model.md` GATE (every generation) — see [C1].
- `LanguageModel.ToolChoice` `"required"` / `{ tool }` single-tool modes — `model.md` GATE `_admitted` only builds
  `{ mode: "auto", oneOf }`. A forced-single-tool turn (structured extraction, mandatory function call) needs the
  `{ tool }` / `"required"` arms. Land in `model.md` GATE as admission modes.
- `IdGenerator` (`make`/`layer`/`defaultIdGenerator`) — pluggable tool-call id source. A DETERMINISTIC id generator is
  the missing replay-stability lever for agent/workflow tool-call ids. Land in `model.md` / `agent.md` for durable
  replay parity.
- `Chat.exportJson` / `Chat.fromJson` — `agent.md` SESSION uses `export`/`fromExport` (unknown) but not the string
  JSON twins that the snapshot/persist path actually needs for KV/wire storage. Land in `agent.md` SESSION.
- `Response.DocumentSourcePart` / `Response.UrlSourcePart` — citation/source parts in a response. Agent replies and
  the guardrail sweep should carry source provenance; neither `model.md` nor `agent.md` reads them. Land in
  `agent.md` TURN (Turn receipt carries sources) / `model.md` GATE.
- `Prompt.fromResponseParts` — rebuild a prompt from prior response parts (multi-turn tool loops). `agent.md` folds
  results through `chat.history`; `fromResponseParts` is the exact reconstruction primitive. Land in `agent.md` TURN.
- `Tool.getJsonSchema(tool)` — the `JsonSchema7` projection for a tool. `tool.md` REMOTE decodes external
  `inputSchema` but never emits local-tool JSON-Schema; `Host` MCP projection and any wire-tool export need it. Land
  in `tool.md` HOST.

### @effect/cli + @effect/printer (`cli.md`)
- `Command.wizard` — interactive arg-builder over the root. `cli.md` ASSEMBLY_LAW mentions it in prose but `Verb`
  never surfaces it. Land in `cli.md` ASSEMBLY_LAW as `Verb.wizard(root)` beside `Verb.completions`.
- `Prompt.select` / `multiSelect` / `toggle` / `confirm` — `cli.md` OPS_FAMILY uses only `Prompt.text`. A doctor
  remediation menu, a replay target picker, a destructive-op confirm want these. Land in `cli.md` OPS_FAMILY.
- `Doc.encloseSep` / `Doc.list` / `Doc.tupled` — `cli.md` STRUCTURE_ROWS hand-composes with `hsep`/`vsep`/`fill`; the
  delimited-collection owner (`encloseSep`) is the correct primitive for a rendered array/tuple/set. Land in
  STRUCTURE_ROWS as a `_seq` row.
- `Doc.width` / `Doc.column` / `Doc.nesting` (position-reactive) — `_table` measures spans with `_width` arithmetic;
  `Doc.column`/`Doc.width` is the printer-native column-alignment that "never computes column positions manually"
  (printer LAW). Land in STRUCTURE_ROWS `_table`.
- `Optimize.optimize(doc, FusionDepth.Deep)` — associativity fusion before layout for large spec-tree output
  (`inspect` verb). Land in `cli.md` RENDER_SEAM `_text` for the `smart` path.

### @effect/experimental (branch substrate; `api.md`, `agent.md`, `embed.md`)
- `PersistedCache.make({ storeId, lookup, timeToLive })` — durable idempotency/result cache over a `KeyValueStore`.
  `api.md` ADMISSION_ROWS `Idempotency.memory` is in-memory and says "a store-backed Layer replaces it at the app root
  for a fleet" but NEVER names the substrate. `PersistedCache` IS that fleet tier. Land in `api.md` ADMISSION_ROWS as
  `Idempotency.persisted` — see [C3].
- `RequestResolver.persisted({ storeId })` — `embed.md` ROWS uses `dataLoader` composed with `persisted`; the durable
  band is correctly cited. No gap here, noted for completeness.

### @effect/platform (branch substrate)
- `PlatformLogger.toFile(path, { batchWindow })` — durable batched file logging behind the `Logger` service. `otel/`
  is OTLP-only; a local file sink is the offline/air-gapped log tier the emit page lacks. Land in `otel/emit.md` LANES
  as a `file` sink beside the OTLP lanes (or a policy row).
- `Template.make\`…\`` (HTML templating) — no page serves HTML templates; the PWA prerender shell (`browser/boot.md`
  hydration) and mail HTML (`deliver.md`) both hand-assemble strings. Land where a typed HTML template is warranted.
- `HttpApiSwagger.layer` — `api.md` EMIT `Emit.docs` offers Scalar only; Swagger is the alternative reference UI a
  deployment may prefer. Land in `api.md` EMIT as a docs-UI row (Scalar | Swagger policy).
- `Multipart.toPersisted` / `Multipart.schemaPersisted(schema)` / `.withLimits` — `api.md` CONTRIBUTION names
  `HttpApiSchema.Multipart` but never the persisted-file decode path that hands file parts to the data byte lift.
  Land in `api.md` CONTRIBUTION upload modality.

---

## [B]-[NEVER_USED_ADMITTED_CAPABILITY] — the folder concept demands it; the roster carries it; no page touches it

These are packages/surfaces PINNED in `pnpm-workspace.yaml` with catalogs on disk that ZERO page imports — the clearest
buildout targets, pre-staged by `admissions.md`.

- **`@nats-io/obj` (3.4.0, `nats-io-obj.md`) — the ObjectStore engine, entirely unwired.** `prefetch [B4]` gives the
  verified surface: `Objm(nc)` → `create/open/list`; `ObjectStore.put(meta, ReadableStream)` (chunked,
  `meta.options.max_chunk_size`), `putBlob(meta, Uint8Array)`, `get(name) → { data: ReadableStream, error: Promise }`,
  `getBlob`, `info`, `list`, `delete`, `link`/`linkStore`, `watch`, `seal`, `status`, `destroy` (digests via
  `js-sha256`). It is a DISTINCT engine from the iac S3 object store — chunked large-binary fanout/replay. Land as a
  `net/pubsub.md` blob-topology engine row OR a sibling `net/blob.ts` owner (the campaign "one pubsub surface owning
  every topology" argues for a row on the `Fanout` family, keyed by a blob-topic policy). This closes the
  large-binary broadcast gap the current `Envelope` (opaque octets, bounded) cannot serve.
- **`@nats-io/kv` (3.4.0, `nats-io-kv.md`) — the revision-CAS coordination engine, entirely unwired.** `prefetch [B3]`
  gives the surface: `Kvm(nc)` → `create/open/list`; `KV.create/put/update(k,data,version,timeout?)` (the OCC
  revision-CAS member), `get(k,{revision})`, `delete`, `purge`, `watch`, `keys`, `history`, `status`, `destroy`;
  `KvEntry = { bucket, key, value, created, revision, delta?, operation }`. `prefetch [B5]` RULES this is NOT a
  `Fanout` row — it is a SIBLING COORDINATION port (leader election / distributed mutex / shared-state CAS), paired
  with the browser Web-Locks row. No page provides a general coordination port (Grid uses cluster RunnerStorage
  advisory locks internally; Vault uses BroadcastChannel for session only). Land as a new small `net/coordinate.ts`
  owner (see [D2]).
- **`@opentelemetry/exporter-logs-otlp-http` (0.220.0) + `@opentelemetry/api-logs` (0.220.0) — the OTLP log egress,
  unwired, and the emit page's law is now STALE.** `otel/emit.md` LANES states as LAW: "SDK-lane log egress does not
  exist — no OTLP log exporter is admitted, so the log signal is native-lane-only." `admissions.md` APPLIED both
  packages; `prefetch [D6]` admits `OTLPLogExporter` feeding `BatchLogRecordProcessor`. The law is contradicted by the
  roster. Land in `otel/emit.md` LANES `_sdk`: add `logRecordProcessor: [new BatchLogRecordProcessor(new
  OTLPLogExporter({ url: _signal(policy,"logs"), headers }))]` (reaches through `NodeSdk.Configuration.logRecordProcessor`
  per [D3]) and delete the stale "does not exist" law. `SdkLogRecord`/`SeverityNumber` types come from `api-logs`.
- **`@nats-io/nats-core` growth members `working()` / `term(reason?)` — real, unused.** `prefetch [B2]` #5:
  `msg.working()` is the ack-wait heartbeat for long handlers (prevents redelivery of an in-flight long job);
  `msg.term(reason?)` terminates a poison message (the design only `nak`s, which redelivers forever). Both belong in
  the corrected durable `consume` lane of `net/pubsub.md`.

---

## [C]-[CROSS_STACKING_PLAYS] — package × package the corpus never composes

Compositions where two admitted surfaces fuse into a denser rail than either alone; the corpus keeps them apart.

- **[C1] `@effect/ai` Telemetry × `@effect/opentelemetry` Propagation — one GenAI span per generation.** `model.md`
  GATE hand-annotates the generation span with provider name; the correct rail is `Telemetry.addGenAIAnnotations(span,
  { system: WellKnownSystem, operation: { name: "chat" | "embeddings" }, request: { model }, usage: { inputTokens,
  outputTokens } })` — the provider `*Telemetry` modules (`OpenAiTelemetry`) already extend the semconv, so exporters
  read standard `gen_ai.*` attributes with zero bespoke mapping. Fuse with `otel/emit.md` Propagation so a tool-call
  or agent turn is one continuous, standard-attributed distributed span. Landing: `model.md` GATE + `otel/emit.md`
  (no new owner — the GenAI writer rides the existing span the gate already opens).
- **[C2] `@effect/rpc` RpcClient.withHeaders × `otel/emit` Propagation — trace-continuous RPC egress.** `api.md` EMIT
  `Emit.caller` derives `RpcClient.make(group)` with no header threading. `RpcClient.currentHeaders` is a
  `FiberRef<Headers>`; composing `RpcClient.withHeaders(call, Propagation.egressHeaders())` propagates W3C
  `traceparent`/`baggage` across the RPC wire exactly as `HttpClient.withTracerPropagation` does for HTTP. Landing:
  `api.md` EMIT — a derived RPC caller inherits the same distributed-trace posture as every HTTP call.
- **[C3] `serve/api` Idempotency × `@effect/experimental` PersistedCache — the durable idempotency tier the page
  promises but never names.** `api.md` ADMISSION_ROWS `Idempotency.memory(retention)` is single-node; the fleet tier
  is `PersistedCache.make({ storeId: "idempotency", lookup: firstExecution, timeToLive: retention })` over the
  store-owned `KeyValueStore` — the SAME substrate `agent.md`/`embed.md` already compose. Landing: `api.md`
  ADMISSION_ROWS `Idempotency.persisted`, a Layer swap at the root, zero handler change.
- **[C4] `work/report` render arms × `proc/worker` Bench pool — CPU-bound offload the page defers but never wires.**
  `report.md` PDF_ARM ("an oversized document render offloads through the worker protocol") and BUNDLE ("a large
  bundle runs off the request path") both cite an offload that does not exist. `proc/worker.md` owns the
  `Schema.TaggedRequest` protocol + `Bench` pool. Add a `Render` request class to the worker protocol (spec + rows in,
  bytes out via `Transferable.Uint8Array`) so the jsPDF/JSZip CPU arms execute off-thread. Landing: `proc/worker.md`
  protocol + `report.md` SPEC_FOLD dispatch (a `pdf`/`zip` arm routes to `Render.executed` when the row set exceeds a
  ceiling). This is the one true cross-page stacking the report page's own law demands.
- **[C5] `otel/emit` telemetry registry × every consumer — the contribute-then-collect hook plane.** `prefetch [D4]`
  is the RULING pattern: one `Context.Tag` pipeline registry carrying accumulating `Ref<Chunk<SpanProcessor>>` +
  parallel `MetricReader[]`, `LogRecordProcessor[]`, `ViewOptions[]`, `ResourceDetector[]`; each app/feature
  contributes via a `Layer.effectDiscard` that APPENDS (order-independent, zero global effects); exactly ONE
  SDK-composition root drains the registry inside `NodeSdk.layer(Effect.gen(...))` (the facade accepts
  `Effect<Configuration>`). This is the campaign's THOUSANDS-OF-APPS + consumer-HOOKS mandate made real — taps,
  processors, exporters, redaction points a project plugs in without forking the plane. Landing: a new
  `otel/emit.md` cluster (HOOKS) between POLICY and LANES; see [D3].

---

## [D]-[GAP_CAPABILITIES] — the concept demands it, no admitted surface fully carries it (improver weighs)

Each row: the gap, the package(s) that close it, the integration shape.

- **[D1] Large-binary fanout/blob-store topology.** GAP: `Fanout.Envelope` is bounded opaque octets; nothing streams
  chunked large binary (tiles, GLB bands, media) over the broadcast plane. CLOSE: `@nats-io/obj` (ADMITTED, unused;
  [B4] surface). SHAPE: a `blob`-topology engine row on the `Fanout` family (or `net/blob.ts`) —
  `ObjectStore.put(meta, ReadableStream)` bridged via `Stream.toReadableStream`, `get → ReadableStream` via
  `Stream.fromReadableStreamByob` (zero-copy byte ingest, `prefetch [C4]`). One connection capability (the `wsconnect`
  the jetstream engine already holds) fans into fanout + revision-state + blob-store per [B1].
- **[D2] Distributed coordination port (leader election / mutex / CAS shared state).** GAP: no general coordination
  primitive exists across process planes — Grid's advisory locks are cluster-internal, Vault's BroadcastChannel is
  session-only. CLOSE: `@nats-io/kv` `update(k,data,version)` revision-CAS (ADMITTED, unused; [B3]) as the node/cluster
  row, browser `navigator.locks` (Web Locks, Baseline widely-available; [B5]) as the browser row. SHAPE: a new
  `net/coordinate.ts` (or `proc/lease.ts`) polymorphic port — `acquire(name, mode)`, `cas(key, expected, next)`,
  `elect(name)` — engine-blind, KV row for node, Web-Locks row for browser, exactly the one-owner-many-engines idiom
  ([B6]) the pubsub page already models. Web Locks: `navigator.locks.request(name, { mode, ifAvailable, steal,
  signal }, cb)`, `navigator.locks.query() → { held, pending }`. This is a genuinely new owner the campaign's
  "resilience as a concept" (bulkheads, coordination) and THOUSANDS-OF-APPS (per-tenant leader) mandates justify.
- **[D3] Per-tenant/per-app telemetry isolation + consumer hooks.** GAP: `otel/emit.md` has one `cardinality.tenant`
  reader limit but no per-view cardinality, no Views, no contribute-then-collect hook plane, and metric `views` do NOT
  reach through the `@effect/opentelemetry` facade ([D3] fork-pressure #1). CLOSE: `@opentelemetry/sdk-metrics`
  `ViewOptions`/`AggregationType` (ADMITTED) + the [C5] registry pattern + a hand-built `MeterProvider({ views,
  readers })` for the unreachable Views slot, bridged through the `Metrics.makeProducer`/`Resource.Resource` Tags.
  SHAPE: the HOOKS cluster from [C5], plus `SpanProcessor.onStart` baggage→`tenant.id` bridge ([D5]) and per-view
  `aggregationCardinalityLimit` + `createAllowListAttributesProcessor([...])` (allow-list primary, limit as
  circuit-breaker, per-reader `cardinalityLimits` above the per-view max). Note: `resources` 2.9.0 changed the detector
  API — `_resource` in `emit.md` uses `detectResources({...}).attributes` where `.attributes` is now
  `DetectedResourceAttributes = Record<string, MaybePromise<AttributeValue | undefined>>` (per-attribute promises, sync
  return); mint via `resourceFromAttributes(attrs)` not `new Resource(...)` ([D2]). Also record on the Redaction lane
  card that `SpanProcessor.onEnding` (the scrub hook) is JSDoc-`@experimental` in `sdk-trace-base` 2.9.0 — the design
  is correct (`onEnding` gives a mutable `Span`, `onEnd` only `ReadableSpan`) but rides an experimental method.
- **[D4] OTLP log signal on the SDK lane.** GAP + STALE-LAW: covered in [B] — admit the log exporter, wire
  `logRecordProcessor`, delete the "no log exporter" law. The `LoggerConfig`/`minimumSeverity`/`traceBased`
  per-logger declarative config ([D2] LOGS) is the analog of metric Views for the log signal — a growth row.
- **[D5] Remote-exec / VPS lane (marginal for runtime).** `prefetch-remote` charges a `RemoteFs` surface (ssh2 +
  Command external rsync/scp/ssh); admissions route `ssh2` as branch SUBSTRATE (`libs/typescript/.api/ssh2.md`) and
  `webdav`/`basic-ftp`/`chokidar` to the DATA folder — so the primary `RemoteFs` owner is DATA-folder, NOT runtime.
  The runtime-relevant sliver: `proc/exec.md` `Proc.Spec` already models `Command.pipeTo` feed stages, so a remote
  exec is `Proc.run({ command: "ssh", feed: [...] })` or `Proc.run({ command: "rsync", ... })` with NO new surface —
  `Command.Input`/`Output` bridge Node streams natively ([prefetch-remote (c)]). No page change required unless a
  ruled ssh2 in-process lane is desired; leave to the DATA campaign. Noted so the improver does not duplicate it.

---

## [E]-[PER_PAGE_INTEGRATION_MAP] — what the improver executes, page by page

Only pages with a real edit; ordered by dependency (proc → net → otel → serve → work → ai → browser).

- **`net/pubsub.md`** — [0] fix the ordered-consumer ack no-op (split ordered vs. durable-explicit `AckPolicy.Explicit`
  lane, `jsm.consumers.add`); [0] Window→`DeliverPolicy.New`; add `AckPolicy`/`ReplayPolicy` rows + `working()`/`term()`
  ([B] growth); [D1] add the `@nats-io/obj` blob-topology engine row (or split to `net/blob.ts`). Update the
  `nats-io-jetstream.md` catalog: add `LastPerSubject`, `AckPolicy`, `ReplayPolicy`, the ordered-cannot-ack law.
- **`net/channel.md`** — [0] delete `_hinted`, read `frame.duration` directly, thread `frame.lastEventId` into the
  cursor `Ref`; delete the RESEARCH block ([C1] closed).
- **`net/coordinate.ts` (NEW)** — [D2] the distributed coordination port: `@nats-io/kv` revision-CAS node row +
  browser Web-Locks row, engine-blind `acquire`/`cas`/`elect`. New earned owner (real concern, no home).
- **`otel/emit.md`** — [D4]/[B] wire the OTLP log exporter into `_sdk.logRecordProcessor`, delete the stale
  "no log exporter" law; [C5]/[D3] add a HOOKS cluster (contribute-then-collect registry:
  `Ref<Chunk<SpanProcessor>>` + `MetricReader[]` + `LogRecordProcessor[]` + `ViewOptions[]` + `ResourceDetector[]`,
  drained in `NodeSdk.layer(Effect.gen(...))`); [D3] add per-view `ViewOptions`/`aggregationCardinalityLimit` +
  allow-list attribute processors for tenant cardinality; correct `_resource` to `resourceFromAttributes` +
  `DetectedResourceAttributes` async-attr shape; record `onEnding` experimental flag on the Redaction lane card;
  [C1] add `Telemetry.addGenAIAnnotations` as the seam `model.md` composes.
- **`serve/live.md`** — [0] delete the `Sse.encoder`/`Sse.Event` RESEARCH note ([C1] closed); confirm `_BEAT` /
  `new Sse.Retry({ duration, lastEventId })` unchanged.
- **`serve/api.md`** — [C3] `Idempotency.persisted` over `PersistedCache`; [C2] `Emit.caller` composes
  `RpcClient.withHeaders(Propagation.egressHeaders())`; [A] add `RpcServer.layerProtocolSocketServer` +
  `RpcWorker.layerInitialMessage` rows, `Rpc.make({ primaryKey })`, `Rpc.fork`/`uninterruptible` markers,
  `Multipart.toPersisted` upload path, `HttpApiSwagger.layer` docs-UI row.
- **`serve/route.md`** — [A] `RpcServer.toHttpApp` mount form for an RPC group beside raw routes.
- **`serve/cli.md`** — [A] `Verb.wizard(root)`; `Prompt.select`/`confirm` in OPS_FAMILY; `Doc.encloseSep`/`list` and
  `Doc.column`/`width` in STRUCTURE_ROWS; `Optimize.optimize` on the `smart` render path.
- **`work/entity.md`** — [A] `DeliverAt` scheduled-message modality; `Entity.Replier` mailbox reply seam;
  `Entity.CurrentAddress`/`CurrentRunnerAddress` locality evidence; `ClusterSchema.Uninterruptible`/
  `ClientTracingEnabled` annotation rows; `ShardingRegistrationEvent.match` census fold; `ClusterMetrics` as the
  supervision metric source; `HttpRunner.layerWebsocket` runner-entry growth row.
- **`work/flow.md`** — [A] `Workflow.suspend`/`intoResult`/`provideScope`; `WorkflowEngine.WorkflowInstance` run
  evidence in STEP_MINT; `DurableDeferred.into` in-band gate bind; `WorkflowProxyServer` in WIRE_PROXY.
- **`work/report.md`** — [C4] route the CPU-bound `pdf`/`zip` arms through a `proc/worker` `Render` request class
  (the offload the page's own law asserts but never wires); `report.md` SPEC_FOLD dispatch gains the ceiling-gated
  worker path.
- **`proc/worker.md`** — [C4] add the `Render` request class (spec in, `Transferable.Uint8Array` bytes out) to the
  protocol union + handler record.
- **`ai/model.md`** — [C1] `Telemetry.addGenAIAnnotations` on every generation span; [A] `ToolChoice` `"required"`/
  `{ tool }` admission modes; `IdGenerator` deterministic tool-call ids for replay parity; note `Ladder._plan`
  `ExecutionPlan.make` step-record field names remain a genuine open research seam (NOT in any prefetch — resolve via
  Context7 `/effect-ts/effect` on `effect/ExecutionPlan` before the fence's constructor literals leave RESEARCH).
- **`ai/agent.md`** — [A] `Chat.exportJson`/`fromJson` snapshot path; `Response.DocumentSourcePart`/`UrlSourcePart`
  source provenance in the `Turn` receipt; `Prompt.fromResponseParts` tool-loop reconstruction; `IdGenerator` for
  durable-replay id stability.
- **`ai/tool.md`** — [A] `Tool.getJsonSchema` for local-tool JSON-Schema emission at the `Host` MCP seam.
- **`browser/*`** — [D2] the browser Web-Locks row of the coordination port lands beside `route.md`'s existing
  BroadcastChannel session use (a browser Fanout engine row is the OTHER half of [B5]: BroadcastChannel → cross-tab
  `Fanout` engine row, distinct from the coordination port). No forced edit beyond the coordination-port browser row
  and, if pursued, a browser `Fanout` engine row in `net/pubsub.md` for cross-tab broadcast.

---

## [F]-[NON_FINDINGS] — verified dense, do NOT touch (guards against churn)

- `otel/vital.md`, `otel/crash.md`, `proc/life.md`, `proc/config.md`, `proc/flag.md` — fully realized; every admitted
  member consumed, no phantom, no stub. No stacking yield.
- `serve/problem.md` — RFC 9457 owner is complete; `HttpServerRespondable` self-render, blame-derived exposure, the
  total ladder — no underutilization.
- `work/queue.md`, `work/schedule.md`, `work/deliver.md` — the `@effect/workflow` `DurableQueue`/`DurableRateLimiter`/
  `ClusterCron` surfaces are fully mined; nodemailer/exceljs/jspdf/jszip/papaparse consumed at depth. Only `deliver`'s
  Relay could ride `DeliverAt` if [A]/entity lands it, otherwise settled.
- `browser/persist.md`, `browser/fetch.md` — idb-keyval, `@effect/experimental` EventLog/Reactivity, the worker
  protocol, and `Residency`/`ContentKey` delegation are all consumed; the folder's `Overlay` law is exact.
- `ai/embed.md` — `EmbeddingModel.make`/`makeDataLoader` batch+cache and `RequestResolver.persisted` durable band are
  correctly composed; the only add is the deferred Google raw-embedding `custom` row (already a stated growth slot).
