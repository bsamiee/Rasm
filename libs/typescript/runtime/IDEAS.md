# [TS_RUNTIME_IDEAS]

Forward pool of higher-order runtime concepts grounded in the execution-substrate domain and the monorepo purpose. `[1]-[OPEN]` carries live ideas; `[2]-[CLOSED]` records a finished or dropped idea with a one-line disposition so it is never re-litigated. An idea drives one or more `TASKLOG.md` tasks.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
-->

[BENCH_CLAIM_PRODUCER]-[QUEUED]: Node-side benchmark runs mint wire-grade claims.
- Capability: measured workload runs fold into the same suite/metrics/host claim shape the C# host mints — quantile folds under `Clock`, hardware-counter and GC statistics from a measured sampling engine, a node host-fingerprint mirror of the browser probe's, app-egress delivery through wire encode — so cross-runtime performance comparison rides one admitted claim plane.
- Shape: a measured-run owner landing in `libs/typescript/runtime/.planning/proc/exec.md` — bracketed runs at `Proc.Receipt`-grade timing, warmup and iteration policy as row data, `mitata` `bench`/`run` as the sampling engine with its counter and GC columns folded into the claim's metric rows, host fingerprint minted from process facts; heavy bodies execute off-thread on the worker pool.
- Unlocks: TS engines and work-plane paths gain admissible benchmark evidence on the claim board beside the C# claims; a regression reads as a claim delta.
- Anchors: core `interchange/codec` claim landing with its host-fingerprint admission gate; `proc/exec.md` measured receipts; `proc/worker.md` off-thread pool; ui `viewer/probe` claim-board join; `mitata` (candidate — hardware counters, GC statistics).
- Tension: the tests tier owns corpus benchmarking — this owner mints in-product claims, and the two never share a harness.

[PROFILE_SIGNAL]-[QUEUED]: Continuous profiling joins the signal plane as its fourth lane.
- Capability: always-on wall and heap profiles pushed from the node lane, labeled by the estate resource identity and correlated to spans, symbolicated through `SourceMapper` so transpiled frames resolve to source, path-stripped through `StripFilenamesMode` where a deployment's posture demands it — a burn alert walks metric to trace to profile in one pane.
- Shape: an `otel/profile.md` owner at `libs/typescript/runtime/.planning/otel/profile.md` — `init`/`start`/`stop` lifecycle over `@pyroscope/nodejs`, `SourceMapper.create` and the strip-mode policy as init rows, label bands via `wrapWithLabels` around work-plane workloads, a `Life` drain row joining the telemetry flush, `Setting`-fed backend origin.
- Unlocks: cpu and allocation attribution for the work plane and AI lanes; the iac Pyroscope ingest row gains its runtime producer.
- Anchors: `otel/emit.md` lane law and the rank-90 drain idiom; `proc/config.md` `Setting.otel` admission rows; iac `operate/observe.md` profile ingest row; `.api/pyroscope-nodejs.md` (`SourceMapper`, `StripFilenamesMode`, `wrapWithLabels`).
- Tension: profile push rides the Pyroscope wire, not OTLP — the lane stays its own owner so the OTLP wire owner never forks.

[NODE_VITALS]-[QUEUED]: Engine health joins the emit plane as first-class series.
- Capability: event-loop delay and utilization histograms plus GC-duration series stream from the node lane through the same Hooks registry the SDK drains, and a deny-list view row suppresses the high-cardinality attributes those instrumentations stamp — a saturated event loop or GC storm reads on the board before it reads in latency.
- Shape: an instrumentation row in `libs/typescript/runtime/.planning/otel/emit.md` — the runtime-node instrumentation registered against the `Hooks.Meter` provider beside `HostMetrics` — and a contributed `createDenyListAttributesProcessor` view row in `libs/typescript/runtime/.planning/otel/meter.md` guarding the series fan.
- Unlocks: work-plane saturation attribution — queue lag distinguishes from event-loop stall; the iac alert set gains engine-health burn rows.
- Anchors: `.api/opentelemetry-host-metrics.md`; `.api/opentelemetry-sdk-metrics.md` (`createDenyListAttributesProcessor`); `otel/emit.md` Hooks plane; `@opentelemetry/instrumentation-runtime-node` (candidate).

[TENANT_SIGNAL]-[QUEUED]: Tenant identity rides every span without a hand-rolled bridge.
- Capability: the `rasm.tenant` baggage entry promotes onto span attributes through a shipped filter-keyed processor row, closing the loop `Propagation.ingress` opens — tenant cost attribution walks baggage to span to metric view under the standing three-tier cardinality governor, and a key predicate refuses foreign baggage promotion by construction.
- Shape: a contributed `BaggageSpanProcessor` row admitting exactly the `rasm.*` promotion key set in `libs/typescript/runtime/.planning/otel/emit.md`'s Hooks plane, replacing the described-but-unshipped `onStart` bridge; the promotion key set admits as a `Setting.otel` row in `libs/typescript/runtime/.planning/proc/config.md`.
- Unlocks: per-tenant traces and cost slices with zero emit-site changes; the C# and python branches mirror one promotion law.
- Anchors: `otel/emit.md` `[04]` tenant-isolation law; `otel/meter.md` `Pulse.tenants` view row; `@opentelemetry/baggage-span-processor` (candidate).
- Ripple: `libs` `[COST_ATTRIBUTION_BAGGAGE]`.

[WIRE_PROTOBUF]-[QUEUED]: Protobuf egress holds on every lane, not only the native one.
- Capability: the wire law names OTLP/HTTP+protobuf the sole egress, yet the SDK lanes serialize JSON — the `-proto` exporter trio closes the gap so `policy.serialization` selects protobuf identically on the native and SDK lanes, and a compliance deployment selecting the scrub-capable lane loses nothing on the wire.
- Shape: the `_sdk`/`_meter` exporter constructors in `libs/typescript/runtime/.planning/otel/emit.md` dispatch on `policy.serialization` between the held `-http` json family and the `-proto` families — trace, metrics, logs — under the same headers, temporality, and cardinality rows.
- Unlocks: one collector contract across every lane and branch; the C# host's protobuf-only collector posture needs no JSON side door.
- Anchors: `otel/emit.md` `[05]` lane rows; the estate wire law; `@opentelemetry/exporter-trace-otlp-proto`, `@opentelemetry/exporter-metrics-otlp-proto`, `@opentelemetry/exporter-logs-otlp-proto` (candidates).

[BROKER_ENGINE_SET]-[QUEUED]: Fanout's engine roster completes — NATS runs on Node, Kafka joins as a row.
- Capability: the JetStream engine row gains its missing Node TCP/TLS transport binding — the modular core is transport-agnostic and holds no node binding today — and Kafka lands as a second durable engine row on the same engine-blind `Broker` port: delivery, dedup, replay, and retention guarantees stated per row, payloads staying opaque octets.
- Shape: a NATS transport binding member on the node runtime row in `libs/typescript/runtime/.planning/proc/exec.md`; a Kafka engine row in `libs/typescript/runtime/.planning/net/pubsub.md` mirroring the JetStream row's guarantee columns over the librdkafka client, W3C headers riding message headers per the queued carrier task.
- Unlocks: broker choice becomes root data; the C# Confluent lane and the TS lane converse over one topic law.
- Anchors: `net/pubsub.md` `Broker` port and JetStream row; `.api/nats-io-nats-core.md`; TASKLOG `[0007]` carrier task; `@nats-io/transport-node`, `@confluentinc/kafka-javascript` (candidates).
- Tension: KIP-714 broker telemetry is a broker-ops lane, never this row's concern.

[CHANNEL_MQTT]-[QUEUED]: MQTT v5 joins the framed-channel vocabulary.
- Capability: device-plane telemetry ingest and command egress ride the same frame vocabulary sockets and SSE speak — QoS, retained messages, and shared subscriptions as row data, W3C trace context in v5 UserProperties — so device causality joins the estate traces.
- Shape: an MQTT binding row in `libs/typescript/runtime/.planning/net/channel.md` over the v5 client — connect and reconnect folded into the existing reconnection fold, the UserProperties carrier injected at publish and extracted through `Propagation.ingress` at receive.
- Unlocks: device fleets — sensors, shop-floor machines, capture rigs — join the estate's traces and command rails without a bespoke bridge.
- Anchors: `net/channel.md` frame rows; `otel/emit.md` `Propagation` string-keyed carrier contract; `mqtt` (candidate).

[CLOUDEVENTS_ENVELOPE]-[QUEUED]: Webhook egress and intake speak CloudEvents with causal identity.
- Capability: outbound webhooks and inbound event intake carry one self-describing envelope — id, source, type, time per the spec, the distributed-tracing extension carrying `traceparent`/`tracestate` — so a foreign consumer joins the estate's traces and a foreign producer's causality survives intake.
- Shape: an envelope codec row in `libs/typescript/runtime/.planning/work/deliver.md`'s webhook channel with structured and binary content modes as row data, and an intake decode row on the webhook ceremony in `libs/typescript/runtime/.planning/serve/route.md`, both folding the tracing extension through `Propagation`.
- Unlocks: estate webhooks interoperate with any CloudEvents-aware bus; delivery receipts gain the envelope id as correlation evidence.
- Anchors: `work/deliver.md` hook row and settlement vocabulary; `serve/route.md` webhook ceremony; `cloudevents` (candidate).

[GRPC_LANE]-[QUEUED]: Connect opens the gRPC axis server- and client-side.
- Capability: Connect/gRPC serving mounts through the foreign-protocol Mount port and outbound Connect calls ride a client lane row — a hand-written W3C interceptor pair carries trace context both directions because no TS otelconnect exists — so the C# gRPC host and TS services converse with unbroken causality under the branch budget posture.
- Shape: a Connect-node router mount row in `libs/typescript/runtime/.planning/serve/live.md`'s Mount port; an outbound Connect dispatch row in `libs/typescript/runtime/.planning/net/client.md` inheriting the lane table's budget and circuit rows; one interceptor pair injecting and extracting via `Propagation`.
- Unlocks: the transport inventory's gRPC axis lands in TS; rpc traffic inherits lane budgets instead of bespoke clients.
- Anchors: `serve/live.md` `[07]` mount port; `net/client.md` lane table; `otel/emit.md` `Propagation`; `@connectrpc/connect-node` (candidate).

[EVENTLOG_SYNC]-[QUEUED]: Offline-first closes its loop — the EventLog overlay gains its server half.
- Capability: browser EventLog writes replicate through a mounted server handler so the persist overlay stops being a client-only diary — encrypted event sync, remote flush on reconnect, and multi-device convergence ride the shipped protocol instead of a bespoke sync endpoint.
- Shape: an `EventLogServer.makeHandlerHttp` mount row in `libs/typescript/runtime/.planning/serve/live.md`'s foreign-protocol Mount port with storage satisfied by the data plane; the overlay's remote registration declared at the seam in `libs/typescript/runtime/.planning/browser/persist.md`.
- Unlocks: field capture — site surveys, fabrication travelers — syncs when connectivity returns; the browser plane gains durable multi-device state without a second store.
- Anchors: branch `.api/effect-experimental.md` (`EventLogServer.makeHandlerHttp`); `browser/persist.md` overlay bindings; `serve/live.md` mount port.

[SERVE_LIMITER]-[QUEUED]: Admission throttling becomes a policy value on the serving edge and tenant egress.
- Capability: request admission and tenant egress inherit persistent token-cost rate limits — window, tokens, cost-per-route as policy rows over a store-backed limiter — so a burst tenant degrades to a `Problem`-rendered refusal instead of starving peers, and the durable-queue throttles and the serving gate share one limiter vocabulary.
- Shape: a limiter ceremony row in `libs/typescript/runtime/.planning/serve/route.md` with per-principal and per-route cost columns, and a `RateLimiter.makeWithRateLimiter`-backed policy row beside the keyed throttles in `libs/typescript/runtime/.planning/work/queue.md`.
- Unlocks: multi-tenant fairness as data; the app-neutrality law holds under contention.
- Anchors: branch `.api/effect-experimental.md` (`RateLimiter.makeWithRateLimiter`); `serve/route.md` ceremony rows; `work/queue.md` throttle cluster.

[BOARD_FEED]-[QUEUED]: Instruments carry their own board — the census projects as a typed dashboard feed.
- Capability: every Convention instrument row, vital budget threshold, and work-plane series this folder emits projects into one typed `BoardPack` value — panels, units, thresholds, and burn-rate inputs as data — so the iac dashboard compile leg derives panels and alerts from the same rows the emitters write, and a budget edit moves the emission grade and the board panel in one place.
- Shape: a census projection on `libs/typescript/runtime/.planning/otel/meter.md` — `Pulse` folds the `_WORK`/`_GAUGES` rows and `Vital.rows` budgets into the pack the iac counterpart consumes.
- Unlocks: zero-drift dashboards; a new instrument appears on the board by construction.
- Anchors: `otel/meter.md` instrument rows; `otel/vital.md` `Vital.rows`; iac `operate/observe.md` Foundation-SDK compile leg.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

- [0001]-[COMPLETE]: work-plane meter bridge — realized as `otel/meter.md` (`Pulse`): fact→instrument projection, census gauges, log-floor wiring, tenant views.
