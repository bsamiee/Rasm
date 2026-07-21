# [TS_RUNTIME_TASKLOG]

`runtime` open and closed work distilled from `IDEAS.md` and design-page RESEARCH residuals. One idea spawns one or more tasks; each task names the exact sub-domain or file it lands in.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <present only on a BLOCKED or gated card; the exact observable that flips it actionable — a catalog row landing, a member query returning evidence, a package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart card — cross-folder as `pkg` `[SLUG]` or a same-folder prerequisite `[SLUG]`, prefixed follows/precedes/mirrors when build order is load-bearing>.
- Atomic: <present only on a minor-scope task; names the small unit so a later session sizes its turn>.
Capability, Shape, Unlocks, and Anchors are required on every open card, Atomic included; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Tasks state landing-grain work decomposing an idea.
-->

[EVENTLOG_SERVER_MOUNT]-[QUEUED]: EventLog server handler mounts and the overlay declares its remote.
- Capability: `EventLogServer.makeHandlerHttp` serves the browser overlay's sync protocol with storage satisfied by the data plane; drives from IDEAS `[EVENTLOG_SYNC]`.
- Shape: one mount row in `libs/typescript/runtime/.planning/serve/live.md` `[07]`; the overlay's remote registration declared at the seam in `libs/typescript/runtime/.planning/browser/persist.md`.
- Unlocks: IDEAS.md [EVENTLOG_SYNC] — field capture syncs when connectivity returns, the browser plane gaining durable multi-device state without a second store.
- Anchors: branch `.api/effect-experimental.md` (`EventLogServer.makeHandlerHttp`); `browser/persist.md` overlay bindings.

[RATE_LIMIT_ROWS]-[QUEUED]: Store-backed rate-limit rows land on the serving edge and queue throttles.
- Capability: per-principal and per-route token-cost admission with `Problem`-rendered refusal, sharing one limiter vocabulary with the durable-queue throttles; drives from IDEAS `[SERVE_LIMITER]`.
- Shape: one ceremony row in `libs/typescript/runtime/.planning/serve/route.md`; one `RateLimiter.makeWithRateLimiter` policy row beside the keyed throttles in `libs/typescript/runtime/.planning/work/queue.md`.
- Unlocks: IDEAS.md [SERVE_LIMITER] — multi-tenant fairness becomes data, the app-neutrality law holding under contention.
- Anchors: branch `.api/effect-experimental.md` (`RateLimiter.makeWithRateLimiter`); `work/queue.md` throttle cluster.

[CREDENTIAL_PROJECTION_ROWS]-[QUEUED]: Credential-projection rows mount the machine principal.
- Capability: gRPC per-call metadata rows and the NATS connection-authentication row read the security-resolved principal with grant-lifecycle refresh — NATS credentials live on `ConnectionOptions` at dial (handshake and reconnect authentication, rotation replaces the connection), never message headers, which stay app metadata; drives from IDEAS `[WORKLOAD_CREDENTIAL]`.
- Shape: one row on `libs/typescript/runtime/.planning/net/client.md`; one row on `libs/typescript/runtime/.planning/net/pubsub.md`.
- Unlocks: IDEAS.md [WORKLOAD_CREDENTIAL] — service-to-service auth on every transport axis from one principal source, credential rotation never restarting a lane.
- Anchors: security `authn/workload.md` principal projection (carded); `net/client.md` lane table.
- Atomic: two credential rows.

[MITATA_SAMPLING_MODALITY]-[BLOCKED]: `proc/exec.md` measured-run sampling lands over verified mitata members.
- Capability: the deep-sampling modality joins the settled base receipts without an `Unknown` evidence bag; drives from IDEAS `[BENCH_CLAIM_PRODUCER]`.
- Shape: one modality row on `libs/typescript/runtime/.planning/proc/exec.md` `[05]-[MEASURED_RUN]`, its `[06]-[RESEARCH]` `[TRIAL_ENGINE]` row deleted on landing.
- Unlocks: IDEAS.md [BENCH_CLAIM_PRODUCER] — typed sampling evidence completes the receipt producer.
- Anchors: `.api/mitata.md` at the folder and branch tiers; `proc/exec.md` `[05]-[MEASURED_RUN]`.
- Arms: an applicable catalog confirms every engine member and typed result field.

[CLOUDEVENTS_CODEC_ROWS]-[BLOCKED]: verified CloudEvents codecs replace the egress and intake fences.
- Capability: egress preserves structured content type, binary data bytes, and W3C extension attributes; intake auto-detects binary versus structured mode with decode evidence on the `Problem` rail; drives from IDEAS `[CLOUDEVENTS_ENVELOPE]`.
- Shape: codec rows on `libs/typescript/runtime/.planning/work/deliver.md` and `libs/typescript/runtime/.planning/serve/route.md`, each page's `[07]-[RESEARCH]` row (`[CLOUDEVENTS_EGRESS]`, `[CLOUDEVENTS_INTAKE]`) deleted on landing.
- Unlocks: IDEAS.md [CLOUDEVENTS_ENVELOPE] — both HTTP binding directions verified.
- Anchors: `.api/cloudevents.md` at the folder and branch tiers.
- Arms: applicable catalogs confirm both HTTP binding directions.

[CONNECT_INTERCEPTOR_MOUNT]-[BLOCKED]: the Connect interceptor pair and guarded mount land over exact surfaces.
- Capability: W3C injection and extraction interceptors and the `Seam.guard`-preserving server mount complete the settled transport dispatch; drives from IDEAS `[GRPC_LANE]`.
- Shape: rows on `libs/typescript/runtime/.planning/net/client.md` `[06]-[CONNECT_ROW]` gated by its `[07]-[RESEARCH]` `[CONNECT_INTERCEPTORS]` row, and on `libs/typescript/runtime/.planning/serve/live.md` gated by its `[08]-[RESEARCH]` `[CONNECT_MOUNT]` row.
- Unlocks: IDEAS.md [GRPC_LANE] — trace-carrying gRPC lanes under the one guard law.
- Anchors: `.api/connectrpc-connect-node.md` at the folder and branch tiers; core `.api/connectrpc-connect.md` peer contract; `.api/effect-platform-node.md` host interop rows; the rpc admission boundary at `libs/typescript/runtime/RULINGS.md` `[01]-[PACKAGES]` — dial admitted, serving only through the `Mount` port.
- Arms: both surfaces exact — the interceptor members, the composite carrier setter, and the `connectNodeAdapter` handler lift.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[FETCH_TIMING_SETTLE]-[COMPLETE]: `browser/fetch.md` `[04]-[DIAL_SURFACE]` holds the caller span through body consumption and resolves the `PerformanceResourceTiming` race with the `_SETTLE` bounded poll; the page's `[08]-[RESEARCH]` emptied to `(none)`.
[MQTT_FENCE_VERIFIED]-[COMPLETE]: `net/channel.md` `Mqtt` fence composes the v5 members verified at `libs/typescript/core/.api/mqtt.md` (`connectAsync`, `subscribeAsync`, `publishAsync`, `endAsync`, `userProperties`); the `[MQTT_V5]` research row deleted as resolved; drives from IDEAS `[CHANNEL_MQTT]`.
[PULSE_METER_PAGE]-[COMPLETE]: `otel/meter.md` landed — `Pulse.mark`/`Pulse.live` over Convention work-plane rows, `Probe` port for the data census, `verbosity`, `tenants`.
[BROWSER_TELEMETRY_ADMISSIONS]-[COMPLETE]: browser telemetry admissions registered — `@opentelemetry/api`, `context-zone`, `instrumentation-{fetch,document-load,user-interaction}` rows with `.api/` catalogs and the composition-root law on `otel/emit.md`.
[BROWSER_INSTRUMENT_CLUSTER]-[COMPLETE]: browser instrumentation registration realized — `Instrument` cluster on `otel/emit.md` (zone bracket, `registerInstrumentations` on the web lane's exposed provider, policy-fed self-exclusion), `@opentelemetry/instrumentation` admission with its `.api/` catalog, `Pulse.mark` composed beside the `work/deliver` and `work/queue` fact sites.
[CONFORMANCE_CRITIQUE]-[COMPLETE]: conformance critique landed — `Hooks.add` keyed-append collapse, per-module exports separation on the otel pages, the `otel/vital.md` key-tuple anchor, baggage sealed behind `Propagation.ingress`, the `Feed.cadence` hop deleted, and the `Vital.enrich` dial seam declared on `browser/fetch.md` with its span-handle research row.
[HOST_METRICS_BINDING]-[COMPLETE]: already realized — `emit.md` binds `HostMetrics` on the raw `Hooks.Meter` provider, `_placed` arms container/aws/gcp detectors, `_rum` folds `browserDetector`, and the Instrument cluster carries the XHR row under the shared self-exclusion policy.
[PROFILE_PAGE_MINT]-[COMPLETE]: `otel/profile.md` minted — `Profile.Policy` off `Setting.otel.profile`, init/start/stop bracket with `SourceMapper.create([...roots])` and `StripFilenamesMode`, identity-projected `appName`/`tags`, rank-91 `Life` drain, `wrapWithLabels` bands; the pyroscope catalog gained the verified declarations.
[PUBSUB_CARRIER_INJECTION]-[COMPLETE]: `pubsub.md` local, tab, NATS, and Kafka publish rows inject `Propagation.current` through matching core `Carrier` dialects, and each consume row extracts the same dialect before `Propagation.ingress`.
[NODE_RUNTIME_INSTRUMENTATION]-[COMPLETE]: already realized — `emit.md` `_vitals` registers `RuntimeNodeInstrumentation` via `registerInstrumentations` and `eter.md` `[06]-[ENGINE]` contributes the `createDenyListAttributesProcessor` view row.
 [BAGGAGE_PROMOTION]-[COMPLETE]: already realized — `emit.md` `_sdk` wires `BaggageSpanProcessor(_admitted(policy.promote))` and `config.md` admits the `Setting.otel.promote` prefix row.
[WIRE_EXPORTER_DISPATCH]-[COMPLETE]: already realized — `emit.md` `_wire` table dispatches `policy.serialization` across the `-http`/`-proto` exporter families for all three signals under shared policy rows.
[NATS_KAFKA_ROWS]-[COMPLETE]: landed — `exec.md` runtime rows carry `nats: connect` (`@nats-io/transport-node`) consumed by `Broker.live(dial)`, and `pubsub.md` `[07]-[KAFKA_ROW]` lands the Kafka engine with honest dedup/replay/blob degradation columns.
[PULSE_BOARD_FOLD]-[COMPLETE]: `meter.md` `[07]-[BOARD]` folds `_WORK`/`_GAUGES` and `Vital.rows` into `Pulse.Board`; iac `operate/observe.md` admits `runtime.pulse` in `_PACKS` through the shared producer-pack ingest arm.
[HOOKS_DISPATCH]-[COMPLETE]: already realized — `emit.md` `Hooks.Dispatch` with app-keyed rails, the pure veto fold, `FiberSet`-isolated delivery, and the policy-bounded replay ring.
 [CARRIER_ROW_COMPOSITION]-[COMPLETE]: `emit.md` owns carried runtime context and `pubsub.md` composes the exact `fanout`, `nats`, and `kafka` rows from `core/.planning/interchange/carrier.md`; no engine borrows another dialect.
[ENVELOPE_CODEC_DEDUP]-[COMPLETE]: `pubsub.md` preserves the opaque envelope body and band while `data/.planning/journal/append.md` owns strict CloudEvents projection and inverse carrier decode, eliminating the duplicate runtime codec.
[CLOUD_PLACEMENT_TABLE]-[COMPLETE]: already realized — `emit.md` `_CLOUD` placement table (five aws arms + gcp), the `containerDetector` arm on `_placed`, and `browserDetector` on `_rum`.
