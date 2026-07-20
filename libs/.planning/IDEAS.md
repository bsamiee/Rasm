# [CROSS_LIBS_IDEAS]

Cross-language ideas span two or more of C# / Python / TypeScript at the wire and the companion/offline seams. A concept coupling packages within one language lives in that branch's idea register. `[1]-[OPEN]` holds the live concert; `[2]-[CLOSED]` records a finished or dropped idea with a one-line disposition so it is never re-litigated.

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

[UNIFIED_SIGNAL_FABRIC]-[ACTIVE]: One observability fabric spans the three runtimes — shared wire law, receipt-projected instruments, hook rails, and an IaC-compiled backend.
- Capability: Every runtime emits through its vendor-neutral surface (C# `Meter`/`ActivitySource`/`ILogger`, Python `opentelemetry-api` + structlog, TS Effect `Metric`/`withSpan`/log) under one wire law — the `service.namespace=rasm` resource triple, `rasm.<domain>.<measure>` UCUM metric names, scope = package id, pinned semconv schema, OTLP/HTTP+protobuf egress, W3C composite propagation, trace-based exemplars — so signals from any runtime correlate without coupling.
- Shape: Receipts stay the truth and instruments/logs/spans are projections — C# `InstrumentFan` over the receipt fan, Python `Metrics.record` folds, TS `Pulse` Fact-to-Metric bridge; per-branch hook registries (`rasm.<pkg>.<domain>.<point>`) make telemetry a tap on domain facts; `core/observe` Convention, SLO, and `DashboardModel` compile through the Foundation-SDK leg into the iac-realized Grafana stack with Prometheus as the exemplar-bearing reference store row.
- Unlocks: Any future app composes exporters at its root and inherits the full metric/trace/log/profile plane; dashboards and burn-rate alerts derive from the same typed vocabulary the emitters use, breaking at type-check instead of drifting.
- Anchors: `csharp:Rasm.AppHost/Observability`; `python:runtime/observability`; `typescript:core/observe`, `typescript:runtime/otel`, `typescript:iac/operate/observe.md`; the `OtelExport`/`TraceContext`/`DashboardModel`/`StackOutputs` seams.
- Tension: Three SDK trains move on split maturity channels, so the wire-law constants are the only shared surface — conformance rides transcription of the same rows in each branch, never a shared library.
- Ripple: `typescript:data` `[OBJECT_PLANE_INSTRUMENT_PROJECTION]`.

[FLEET_TELEMETRY_SCALE_ROWS]-[QUEUED]: Named fleet-escalation rows flip the estate telemetry plane to fleet scale without new surfaces.
- Capability: Broker-buffered OTLP transport (Kafka/NATS collector legs), tail-based sampling at the gateway, the Mimir scale-out store row, and per-app agent topology form one closed escalation family — each a row on an axis the corpus already models, armed only when a deployment placement earns it.
- Shape: Collector pipeline rows and the iac store/topology coordinates carry the family; apps and libraries change nothing — the same OTLP egress and Convention vocabulary serve both scales.
- Unlocks: Multi-host fleets, tenant isolation at volume, and lossless telemetry under backpressure, all as deploy-time row flips.
- Anchors: `typescript:iac/operate/observe.md` store arm + collector rows; `csharp:Rasm.AppHost/Observability/telemetry.md` exporter seam; the collector file-storage queue that serves the single-estate scale today.
- Tension: Every row is currently ruled OFF at estate scale (file-storage queue covers durability, one gateway suffices, Prometheus reference row holds exemplars) — the card exists so fleet pressure re-arms rows instead of re-deriving the design.

[COST_ATTRIBUTION_BAGGAGE]-[QUEUED]: Tenant baggage joined to grant-cost vectors yields per-tenant cost and usage boards from the standing signal fabric.
- Capability: W3C baggage's tenant dimension, the C# grant/cost spend instruments, and trace-based exemplars compose into per-tenant cost attribution — usage, spend, and burn boards keyed by the same `TenantContext` every runtime already stamps.
- Shape: C# mints the cost facts (`GrantBroker` vectors projected through the instrument fan), the collector routes tenant baggage onto metric dimensions, and the board models derive tenant cost views compiled through the Foundation-SDK leg.
- Unlocks: Metered multi-tenant products, per-agent/per-model AI spend governance, and chargeback-grade evidence without a second metering pipeline.
- Anchors: `csharp:Rasm.AppHost/Agent/capability.md` grant/cost vectors; `csharp:Rasm.AppHost/Observability/instruments.md` spend rows; `typescript:core/observe/board.md`; the `rasm.tenant` baggage law.
- Tension: Tenant cardinality caps on metric streams bound the attribution grain; above the cap, attribution rides exemplar-sampled traces, not per-tenant series.

[PROFILE_SIGNAL_OTLP]-[QUEUED]: Continuous profiles migrate from vendor push onto the OTLP profiles signal the moment it stabilizes.
- Capability: Profiles, the fourth signal, ride the same gateway, resource identity, and scope law as traces/metrics/logs — Pyroscope push SDKs in all three runtimes retire into OTLP exporters, and profile-to-span correlation becomes wire-native.
- Shape: One exporter-row swap per runtime composition root and one collector pipeline row; the span-profile correlation processors and dashboards survive unchanged.
- Unlocks: Vendor-neutral profiling, one ingress for all four signals, and profile exemplar links alongside the metric-trace jumps.
- Anchors: `csharp` Pyroscope span-profile correlation; `python:runtime/observability/profiles.md`; `typescript:iac/operate/observe.md` Pyroscope row; the collector gateway.
- Tension: OTLP profiles stay pre-stable across all three SDKs — the card arms on signal stabilization, never before.

[LAYER_TOPOLOGY_GRAPH_FACTS]-[QUEUED]: Shared `LayerTopologyFact` wire rows carry host organization into every `ElementGraph` peer.
- Capability: `LayerTopologyFact` projects `LayerStamp` identity, `LayerPath` nesting, membership, and per-viewport overrides as detached entity and containment facts, so each runtime answers layer organization without a host handle.
- Shape: C# owns the canonical wire schema and codec; `Rasm.Rhino` `Layers.Ask` emits rows, `Rasm.Element` folds them through `IElementProjection`, `Rasm.Bim` aligns them with IFC spatial structure, Python decodes them into IFC graph projections, and TypeScript decodes them into query-store relations. `ContentHash` identifies referenced geometry and payloads, while the schema's typed layer and relation keys identify graph facts.
- Unlocks: Host-organized element queries, spatial-structure round-tripping, and one organizational producer for the graph's containment axis.
- Anchors: `Rasm.Rhino` `Document/layers.md` `LayerStamp`/`LayerPath`/`Layers.Ask`; `libs/.planning/architecture.md` shared wire and AEC-domain strata; `Rasm.Element/.planning/Projection/projection.md`; `Rasm.Bim/.planning/Projection/semantic.md`.
- Tension: Detached values cross the wire; host handles remain inside `Rasm.Rhino`, and each peer projects the same canonical fact identity.
- Ripple: `python:data` `[LAYER_TOPOLOGY_GRAPH_FACTS]`; `typescript:data` `[LAYER_TOPOLOGY_GRAPH_FACTS]`.

[DAYLIGHTING_SCENE_DESCRIPTOR]-[QUEUED]: Owned sun astronomy, scene lights, and GLB tessellation compose one daylighting scene descriptor the Python analysis companion consumes for EnergyPlus/OpenStudio-grade solar and daylight studies.
- Capability: A content-keyed, host-free scene descriptor — sun state (`SunSolver` astronomy), photometric light roster (`LightStamp` rows with `Radiance` power and the `PhotometricWeb` distribution payload), shading geometry as GLB tessellation — emitted from the C# host and folded by the Python geometry energy owner into radiation, shading, and daylight-autonomy analyses.
- Shape: A descriptor emitter on the `Rasm.Rhino` Render/Exchange surface stacking `SunState` + `Objects/lights.md` stamps + the GLB rail over the content-keyed wire; a Python consumer on `python:geometry/energy/simulate.md` driving the machine's `energyplus`/`openstudio` engines through the runtime recipe binding; results return as wire receipts keyed by the same content identity.
- Unlocks: Closed-loop environmental analysis from the live model — solar exposure, shading studies, daylight metrics — without a host dependency in the analysis runtime, and a reusable scene-descriptor vocabulary for any future physics consumer.
- Anchors: `Rasm.Rhino` `Render/settings.md` `SunSolver.Solve`/`SunState`; `Objects/lights.md` `LightStamp`/`Radiance`; `Render/kinds.md` `PhotometricWeb`; `libs/.planning/architecture.md` `[04]` geometry-flow law (kernel and Python geometry meet only at the wire: content identity and the GLB tessellation rail) and `[06]` per-language roles (Python as the host-free science/compute companion).
- Tension: Descriptor schema is a new shared wire owner — C# mints it, Python decodes it, and the schema lands in the wire vocabulary before either end builds; tessellation fidelity policy remains an explicit descriptor axis.
- Ripple: `python:geometry` `[DAYLIGHTING_SCENE_DESCRIPTOR]`.

[HOST_OPLOG_CRDT_PRODUCER]-[QUEUED]: Committed host transactions become a replayable, mergeable causal op-log — the host end of the shared op-log CRDT wire owner.
- Capability: Every sealed commit folds into an `OperationId`-keyed causal log, so equal payloads remain distinct operations and cross-runtime sync, collaborative merge, and checkpoint replay become wire operations instead of file exchanges.
- Shape: `OperationId` is the shared `[ComplexValueObject]` over tenant, actor, actor sequence, and causal frontier; its canonical codec sorts the vector-clock rows before encoding. Each op-log entry carries `OperationId Id`, `ContentHash Payload`, and the sealed-commit facts. `Rasm.Rhino` `DocumentStream` and `DocumentCommit.Sealed` emit entries, while every branch decodes the same shared payload owner and replays or merges it against a checkpoint snapshot.
- Unlocks: Multi-runtime document sync, collaborative editing groundwork, deterministic replay for testing and audit, and the first live producer for the wire law's op-log owner.
- Anchors: `Rasm.Rhino` `Document/events.md` `DocumentStream.Observe`; `Document/tables.md` `DocumentCommit.Sealed`/`UndoBracket` (the one commit envelope every mutation walks — the natural single tap point); `libs/.planning/architecture.md` `[07]` shared-owner roster (the op-log CRDT payload and tenant/causal identity are named shared wire owners; C# owns the wire vocabulary, Python and TypeScript decode it).
- Tension: Distinct from the static archive diff (`Exchange/archive.md` `ArchiveDelta`) — the op-log is causal and live, the diff is structural and at-rest; CRDT merge policy settles commutation and conflict per mutation kind without conflating operation identity with payload identity.
- Ripple: `typescript:data` `[HOST_OPLOG_CRDT_CONSUMER]`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
