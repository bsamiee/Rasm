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
- Capability: Pyroscope push lifecycle, identity-aligned `appName`/`tags`, `wrapWithLabels` bands, drain registration; drives from IDEAS `[PROFILE_SIGNAL]`.
- Shape: new page `otel/profile.md` beside the emit, crash, vital, and meter owners; config admission lands as `Setting` rows in `proc/config.md`.
- Anchors: `.api/pyroscope-nodejs.md`; `proc/life.md` drain registry; iac `operate/observe.md` Pyroscope ingest row.
- Tension: span-profile correlation labels must match the span identity the OTLP lane stamps — one identity source, `AppIdentity`.

[0007]-[QUEUED]: Fanout carries the W3C trace context.
- Capability: `traceparent`/`tracestate`/`baggage` ride NATS message headers — `Propagation` injects at `Fanout.publish`, extracts at `consume` and `replay`, and the handler continues the parent span.
- Shape: a headers band on the publish path and a carrier read on the consume path in `net/pubsub.md`; payload octets stay opaque — the carrier is transport metadata, never envelope payload.
- Anchors: `otel/emit.md` `Propagation` string-keyed carrier contract; `net/pubsub.md` `Fanout.publish`/`consume` signatures.
- Tension: the envelope law rules payload transport-only — headers widen the transport frame, never the envelope shape.

[0008]-[QUEUED]: Measured-run owner minting claim-shaped bench receipts.
- Capability: bracketed benchmark runs fold to the wire claim's metric rows with a node host-fingerprint mint; drives from IDEAS `[BENCH_CLAIM_PRODUCER]`.
- Shape: a measured-run cluster on `proc/exec.md` or a sibling proc page — warmup and iteration policy rows, quantile fold, off-thread execution for heavy bodies.
- Anchors: core `interchange/codec` claim admission; `proc/worker.md` pool protocol; ui `viewer/probe` board join.
- Tension: the worker pool already spells `Bench` for its offload protocol — one of the two names yields before the page lands.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

- [0001]-[COMPLETE]: `otel/meter.md` landed — `Pulse.mark`/`Pulse.live` over Convention work-plane rows, `Probe` port for the data census, `verbosity`, `tenants`.
- [0002]-[COMPLETE]: browser telemetry admissions registered — `@opentelemetry/api`, `context-zone`, `instrumentation-{fetch,document-load,user-interaction}` rows with `.api/` catalogs and the composition-root law on `otel/emit.md`.
- [0003]-[COMPLETE]: browser instrumentation registration realized — `Instrument` cluster on `otel/emit.md` (zone bracket, `registerInstrumentations` on the web lane's exposed provider, policy-fed self-exclusion), `@opentelemetry/instrumentation` admission with its `.api/` catalog, `Pulse.mark` composed beside the `work/deliver` and `work/queue` fact sites.
- [0004]-[COMPLETE]: conformance critique landed — `Hooks.add` keyed-append collapse, per-module exports separation on the otel pages, the `otel/vital.md` key-tuple anchor, baggage sealed behind `Propagation.ingress`, the `Feed.cadence` hop deleted, and the `Vital.enrich` dial seam declared on `browser/fetch.md` with its span-handle research row.
