# [TS_RUNTIME_TASKLOG]

`runtime` open and closed work distilled from `IDEAS.md` and design-page RESEARCH residuals. One idea spawns one or more tasks; each task names the exact sub-domain or file it lands in.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[0005]-[QUEUED]: Host and platform identity rows complete the emit plane.
- Capability: `HostMetrics` started on the node lane's exposed meter provider; deploy-target detector rows — container, aws, gcp — joining `_DETECTORS`; the browser detector on the `web` lane; the XHR row beside fetch in the Instrument cluster under the same collector-origin self-exclusion.
- Shape: rows on `otel/emit.md` — `_DETECTORS` widens per arm, the Instrument cluster gains `XMLHttpRequestInstrumentation`, `HostMetrics` binds where `Hooks.Meter` exposes the provider.
- Anchors: `.api/opentelemetry-host-metrics.md`; `.api/opentelemetry-resource-detector-container.md` and its aws/gcp/browser siblings; `.api/opentelemetry-instrumentation-xml-http-request.md`; `otel/emit.md` `_lanes`/`Hooks`.
- Atomic: row-level additions to one page.

[0006]-[QUEUED]: `otel/profile.md` — the profiling lane owner.
- Capability: Pyroscope push lifecycle, identity-aligned `appName`/`tags`, `wrapWithLabels` bands, `SourceMapper.create` symbolication and the `StripFilenamesMode` policy as init rows, drain registration; drives from IDEAS `[PROFILE_SIGNAL]`.
- Shape: new page `libs/typescript/runtime/.planning/otel/profile.md` beside the emit, crash, vital, and meter owners; config admission lands as `Setting` rows in `libs/typescript/runtime/.planning/proc/config.md`.
- Anchors: `.api/pyroscope-nodejs.md` (`SourceMapper`, `StripFilenamesMode`); `proc/life.md` drain registry; iac `operate/observe.md` Pyroscope ingest row.
- Tension: span-profile correlation labels must match the span identity the OTLP lane stamps — one identity source, `AppIdentity`.

[0007]-[QUEUED]: Fanout carries the W3C trace context.
- Capability: `traceparent`/`tracestate`/`baggage` ride NATS message headers — `Propagation` injects at `Fanout.publish`, extracts at `consume` and `replay`, and the handler continues the parent span.
- Shape: a headers band on the publish path and a carrier read on the consume path in `net/pubsub.md`; payload octets stay opaque — the carrier is transport metadata, never envelope payload.
- Anchors: `otel/emit.md` `Propagation` string-keyed carrier contract; `net/pubsub.md` `Fanout.publish`/`consume` signatures.
- Tension: the envelope law rules payload transport-only — headers widen the transport frame, never the envelope shape.

[0008]-[QUEUED]: Measured-run owner minting claim-shaped bench receipts.
- Capability: bracketed benchmark runs fold to the wire claim's metric rows with a node host-fingerprint mint, `mitata` supplying the sampling engine with hardware-counter and GC columns; drives from IDEAS `[BENCH_CLAIM_PRODUCER]`.
- Shape: a measured-run cluster on `libs/typescript/runtime/.planning/proc/exec.md` — warmup and iteration policy rows, quantile fold, off-thread execution for heavy bodies.
- Anchors: core `interchange/codec` claim admission; `proc/worker.md` pool protocol; ui `viewer/probe` board join; `mitata` (candidate).
- Tension: the worker pool already spells `Bench` for its offload protocol — one of the two names yields before the page lands.

[0009]-[QUEUED]: Runtime-node engine series and the deny-list view guard land.
- Capability: the runtime-node instrumentation row registers against the `Hooks.Meter` provider on the node lane, and a `createDenyListAttributesProcessor` view row suppresses its high-cardinality attributes; drives from IDEAS `[NODE_VITALS]`.
- Shape: one instrumentation row in `libs/typescript/runtime/.planning/otel/emit.md` beside the `HostMetrics` bind; one contributed view row in `libs/typescript/runtime/.planning/otel/meter.md` beside `Pulse.tenants`.
- Anchors: `.api/opentelemetry-sdk-metrics.md` (`createDenyListAttributesProcessor`); `otel/emit.md` `Hooks`; `@opentelemetry/instrumentation-runtime-node` (candidate).
- Atomic: two row-level additions across two pages.

[0010]-[QUEUED]: Baggage-to-span tenant promotion ships as a contributed processor row.
- Capability: a `BaggageSpanProcessor` row keyed to the `rasm.*` promotion set replaces the described-but-unshipped `onStart` bridge; drives from IDEAS `[TENANT_SIGNAL]`.
- Shape: one `Hooks.contribute` span row in `libs/typescript/runtime/.planning/otel/emit.md`; the promotion key set admits as a `Setting.otel` row in `libs/typescript/runtime/.planning/proc/config.md`.
- Anchors: `otel/emit.md` `[04]` tenant-isolation law; `@opentelemetry/baggage-span-processor` (candidate).
- Atomic: one processor row plus one config row.

[0011]-[QUEUED]: SDK lanes gain protobuf serialization through the `-proto` exporter trio.
- Capability: `_sdk` and `_meter` exporter construction dispatches on `policy.serialization` between the json `-http` and protobuf `-proto` families for traces, metrics, and logs; drives from IDEAS `[WIRE_PROTOBUF]`.
- Shape: exporter-selection rows inside `libs/typescript/runtime/.planning/otel/emit.md` `[05]` — same headers, temporality, cardinality, and batch rows on both arms.
- Anchors: `otel/emit.md` `_sdk`/`_meter`; `@opentelemetry/exporter-trace-otlp-proto`, `@opentelemetry/exporter-metrics-otlp-proto`, `@opentelemetry/exporter-logs-otlp-proto` (candidates).
- Atomic: exporter-constructor dispatch on one page.

[0012]-[QUEUED]: NATS node transport binding and the Kafka engine row land on the fanout plane.
- Capability: the node runtime row carries the NATS TCP/TLS transport binding, and Kafka enters as a `Broker` engine row with its delivery, dedup, replay, and retention columns; drives from IDEAS `[BROKER_ENGINE_SET]`.
- Shape: one binding member on the node row in `libs/typescript/runtime/.planning/proc/exec.md`; one engine row in `libs/typescript/runtime/.planning/net/pubsub.md` mirroring the JetStream guarantee columns.
- Anchors: `net/pubsub.md` `Broker` port; `.api/nats-io-nats-core.md`; `@nats-io/transport-node`, `@confluentinc/kafka-javascript` (candidates).

[0013]-[QUEUED]: MQTT v5 binding row joins the framed channels.
- Capability: QoS, retained-message, and shared-subscription columns as row data with the UserProperties W3C carrier injected at publish and extracted through `Propagation.ingress`; drives from IDEAS `[CHANNEL_MQTT]`.
- Shape: one binding row in `libs/typescript/runtime/.planning/net/channel.md` folded into the existing reconnection fold.
- Anchors: `net/channel.md` frame rows; `otel/emit.md` `Propagation`; `mqtt` (candidate).
- Atomic: one binding row on one page.

[0014]-[QUEUED]: CloudEvents envelope codec lands on webhook egress and intake.
- Capability: structured and binary content modes as row data, the distributed-tracing extension folded through `Propagation` both directions; drives from IDEAS `[CLOUDEVENTS_ENVELOPE]`.
- Shape: one envelope codec row in `libs/typescript/runtime/.planning/work/deliver.md`'s webhook channel; one intake decode row on the webhook ceremony in `libs/typescript/runtime/.planning/serve/route.md`.
- Anchors: `work/deliver.md` hook row; `serve/route.md` webhook ceremony; `cloudevents` (candidate).

[0015]-[QUEUED]: Connect-node mount and dispatch rows open the gRPC lane.
- Capability: a Connect router mounts through the foreign-protocol port, outbound Connect calls ride a client lane row, and one interceptor pair carries W3C context both directions; drives from IDEAS `[GRPC_LANE]`.
- Shape: one mount row in `libs/typescript/runtime/.planning/serve/live.md` `[07]`; one dispatch row in `libs/typescript/runtime/.planning/net/client.md` inheriting budget and circuit rows.
- Anchors: `serve/live.md` mount port; `net/client.md` lane table; `otel/emit.md` `Propagation`; `@connectrpc/connect-node` (candidate).

[0016]-[QUEUED]: EventLog server handler mounts and the overlay declares its remote.
- Capability: `EventLogServer.makeHandlerHttp` serves the browser overlay's sync protocol with storage satisfied by the data plane; drives from IDEAS `[EVENTLOG_SYNC]`.
- Shape: one mount row in `libs/typescript/runtime/.planning/serve/live.md` `[07]`; the overlay's remote registration declared at the seam in `libs/typescript/runtime/.planning/browser/persist.md`.
- Anchors: branch `.api/effect-experimental.md` (`EventLogServer.makeHandlerHttp`); `browser/persist.md` overlay bindings.

[0017]-[QUEUED]: Store-backed rate-limit rows land on the serving edge and queue throttles.
- Capability: per-principal and per-route token-cost admission with `Problem`-rendered refusal, sharing one limiter vocabulary with the durable-queue throttles; drives from IDEAS `[SERVE_LIMITER]`.
- Shape: one ceremony row in `libs/typescript/runtime/.planning/serve/route.md`; one `RateLimiter.makeWithRateLimiter` policy row beside the keyed throttles in `libs/typescript/runtime/.planning/work/queue.md`.
- Anchors: branch `.api/effect-experimental.md` (`RateLimiter.makeWithRateLimiter`); `work/queue.md` throttle cluster.

[0018]-[QUEUED]: `BoardPack` census projection folds the instrument and budget rows.
- Capability: `Pulse` projects the `_WORK`/`_GAUGES` instrument rows and `Vital.rows` budget thresholds into one typed dashboard-feed value the iac compile leg consumes; drives from IDEAS `[BOARD_FEED]`.
- Shape: one census-projection cluster on `libs/typescript/runtime/.planning/otel/meter.md`.
- Anchors: `otel/meter.md` `Pulse`; `otel/vital.md` `Vital.rows`; iac `operate/observe.md` Foundation-SDK compile leg.
- Atomic: one projection cluster on one page.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

- [0001]-[COMPLETE]: `otel/meter.md` landed — `Pulse.mark`/`Pulse.live` over Convention work-plane rows, `Probe` port for the data census, `verbosity`, `tenants`.
- [0002]-[COMPLETE]: browser telemetry admissions registered — `@opentelemetry/api`, `context-zone`, `instrumentation-{fetch,document-load,user-interaction}` rows with `.api/` catalogs and the composition-root law on `otel/emit.md`.
- [0003]-[COMPLETE]: browser instrumentation registration realized — `Instrument` cluster on `otel/emit.md` (zone bracket, `registerInstrumentations` on the web lane's exposed provider, policy-fed self-exclusion), `@opentelemetry/instrumentation` admission with its `.api/` catalog, `Pulse.mark` composed beside the `work/deliver` and `work/queue` fact sites.
- [0004]-[COMPLETE]: conformance critique landed — `Hooks.add` keyed-append collapse, per-module exports separation on the otel pages, the `otel/vital.md` key-tuple anchor, baggage sealed behind `Propagation.ingress`, the `Feed.cadence` hop deleted, and the `Vital.enrich` dial seam declared on `browser/fetch.md` with its span-handle research row.
