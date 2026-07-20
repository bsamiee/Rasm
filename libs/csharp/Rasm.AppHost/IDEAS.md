# [APPHOST_IDEAS]

Forward pool of higher-order concepts for the runtime spine, each grounded in the folder's domain and current platform capability — some are new sub-domain folders that deepen a thin owner, others bind a concrete admitted package to a settled abstract surface. Open ideas drive the tasks in `TASKLOG.md`; a finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition so it is never re-litigated.

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

[WIRE_CARRIER_ADAPTERS]-[QUEUED]: Trace context crosses every estate transport — NATS headers, MQTT v5 user properties, and CloudEvents tracing attributes become adapter rows on the one propagation spine.
- Capability: `TraceContext` grows three carrier adapters — getter/setter pairs delegating to the composite propagator — so `traceparent`/`tracestate` and baggage ride `NatsHeaders`, the MQTT v5 user-property collection, and the CloudEvents `traceparent`/`tracestate` extension attributes; per-transport hand-rolled header writes stay the deleted form.
- Shape: three rows under the `libs/csharp/Rasm.AppHost/.planning/Observability/telemetry.md#CORRELATION_SPINE` growth law — a new propagation carrier is one `IPropagator`-backed adapter on the same `Spine` composite; the MQTT adapter mounts at the `libs/csharp/Rasm.AppHost/.planning/Wire/livewire.md` `MqttLane` publish and receive edges, the NATS and CloudEvents adapters compose at the Persistence egress legs.
- Unlocks: broker-hop trace continuity — a spine delivery, CDC CloudEvent, or live-wire MQTT command joins the same trace the gRPC and Kafka legs already carry, and TraceBased exemplars survive the hop.
- Anchors: the wire-law carrier table (NATS carries no OTel instrumentation by design — manual inject and extract), `NATS.Net` `NatsHeaders`, MQTTnet `MqttApplicationMessageBuilder`, `CloudNative.CloudEvents` extension-attribute surface, `TextMapPropagator.Inject`/`Extract`.
- Ripple: `Rasm.Persistence` `[0003]`.

[TELEMETRY_OFFLINE_SPINE]-[QUEUED]: Offline-durable telemetry egress — every OTLP transmission queue gains an on-disk persistent backing, so a desktop, plugin, or companion process losing its collector keeps signal tails across network loss, crash, and ALC unload.
- Capability: `OpenTelemetry.PersistentStorage.FileSystem` backs the OTLP exporter transmission pipeline with a bounded file-system queue — failed exports persist, replay on reconnection, and survive process death; the drain band flushes what the wire takes and the store holds the rest.
- Shape: persistent-storage rows at both provider owners — `SignalGovernance.Govern` for hosted roots and `PluginTelemetryHost.Open` for per-ALC capsules — landing on `libs/csharp/Rasm.AppHost/.planning/Observability/telemetry.md#SIGNAL_GOVERNANCE` and `libs/csharp/Rasm.AppHost/.planning/Observability/instruments.md#PROVIDER_LIFETIME`; queue-directory, size-cap, and retention values enter the governance table as policy rows.
- Unlocks: honest telemetry from the disconnected desktop and plugin fleet — the dominant Rhino/GH deployment shape — and a truthful delivery-honesty statement for the telemetry sink itself.
- Anchors: `OpenTelemetry.PersistentStorage.FileSystem` (admission pending), the `PluginTelemetryHost` ForceFlush/Dispose law, `DrainConductor` ranked drain, the `OTEL_EXPORTER_OTLP_*` binding rows.
- Tension: the persistent queue holds classified bytes at rest — redaction runs before export, so stored payloads are already-redacted wire bytes, and the queue directory inherits the support-bundle retention law.

[TENANT_COST_SPAN_PROMOTION]-[QUEUED]: Tenant baggage promotes onto every span — the `rasm.tenant` baggage entry becomes a span attribute at the composition root, closing the cost-attribution chain from `GrantBroker` debits to backend per-tenant queries.
- Capability: `OpenTelemetry.Extensions` `BaggageActivityProcessor` with a baggage-key allowlist admits `rasm.tenant` and the `CorrelationId` key onto span attributes — a backend groups spend, latency, and traces by tenant with zero per-call-site tagging; the python branch already rides its baggage processor, so this row completes the family per the set-completion law.
- Shape: one `AddProcessor` row beside the `PyroscopeSpanProcessor` seat and one allowlist governance-value row on `libs/csharp/Rasm.AppHost/.planning/Observability/telemetry.md#SIGNAL_GOVERNANCE`; promotion is allowlist-only, never free-form baggage flood.
- Unlocks: per-tenant flame graphs, exemplar-linked spend dashboards, and the cross-libs `[COST_ATTRIBUTION_BAGGAGE]` chain realized at the C# altitude.
- Anchors: `Correlation.Spine` baggage carrier, `TenantContext` mint, `HostInstruments` spend rows, the wire-law tenant-baggage promotion mandate.

[FLEET_DEPLOY_ANNOTATIONS]-[QUEUED]: Fleet rollout writes the dashboard timeline — every provisioning wave and roll verdict projects one deploy-annotation wire record the estate dashboard plane ingests, so panels mark plugin-fleet rollouts beside stack deploys.
- Capability: `RollAnnotationWire` — wave, channel, verdict, host-count, and instant fields folded off the existing roll receipts; a rollback annotates as loudly as an advance.
- Shape: one wire record and its projection fold on `libs/csharp/Rasm.AppHost/.planning/Sandbox/provisioning.md#ROLLOVER_DRAIN`, joining the `AppHostWireContext` roster at `libs/csharp/Rasm.AppHost/.planning/Runtime/ports.md#WIRE_LAW`; egress rides the receipt envelope like every spine fact.
- Unlocks: deploy-correlated regression triage — a latency shift on any panel resolves to the wave that shipped it.
- Anchors: `FleetRoll` wave conduct, `ReceiptSinkPort`/`ReceiptEnvelope` HLC stamping, the TypeScript iac deploy-annotation rail already ingesting `RunReceipt`.

[DIAGNOSTIC_HEAP_ANALYSIS]-[QUEUED]: Support bundles carry analyzed evidence — a captured process dump folds through ClrMD into a typed triage receipt inside the bundle, so a bundle answers what leaked, what blocked, and what ran before a debugger ever attaches.
- Capability: `Microsoft.Diagnostics.Runtime` `DataTarget.LoadDump` walks the captured dump — top heap types by retained size, per-thread managed stacks, blocked-thread sync-block evidence — into bounded `DumpTriage` rows under the bundle's caps and redaction law.
- Shape: one triage fold and one `SupportArtifact` factory row on `libs/csharp/Rasm.AppHost/.planning/Observability/bundles.md#CAPTURE_PIPELINE`; the manifest entry carries triage rows beside the raw artifact key.
- Unlocks: first-response diagnosis from the bundle alone — the disconnected-fleet shape where a live debugger never attaches.
- Anchors: `Microsoft.Diagnostics.NETCore.Client` dump capture already admitted, `Microsoft.Diagnostics.Runtime` (admission pending), `SupportManifest.Entry` truncation receipts, `FaultTransition` trigger payloads.
- Tension: triage runs post-freeze inside the capture-window deadline — row caps bound the fold, never a full heap walk.

[PARTITIONED_OBSERVABLE_LEVELS]-[QUEUED]: Level cells grow partitions — observable instruments return tagged `Measurement<T>` sets from one callback, so per-probe health, per-channel outbox lag, and per-family capability roster read as partitioned series instead of one scalar per concept.
- Capability: `Measurement<T>` multi-value observation — `CreateObservableGauge(Func<IEnumerable<Measurement<long>>>)` projects an atom-held map of partition keys to levels in one collection pass; partition vocabularies stay bounded by their owning SmartEnum rows.
- Shape: `LevelCells` scalar atoms widen to keyed atoms where a partition dimension exists, and the affected `HostInstruments.Rows` bind multi-measurement callbacks on `libs/csharp/Rasm.AppHost/.planning/Observability/instruments.md#INSTRUMENT_CATALOG`; the wildcard `AddView` cardinality cap already bounds the series budget.
- Unlocks: a dashboard reads which probe degraded and which channel lags with zero metrics re-architecture, and the branch `Measurement<T>` substrate member stops being admitted-but-unexploited.
- Anchors: `api-diagnostics-metrics.md` `Measurement<T>`, `LevelCells.Live` atoms, `DriverProbe` and delivery-channel vocabularies, the `AddView` series caps.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

- [0001]-[COMPLETE]: Domain-instrument catalog over the receipt fan — landed as `.planning/Observability/instruments.md` with the roster, projection fold, per-ALC provider capsule, and observation rail.
- [0002]-[COMPLETE]: Typed hook rail over the existing bus, lifecycle, and receipt seams — landed as `.planning/Observability/hooks.md` with id grammar, modality rows, registry, and fault isolation.
- [0003]-[COMPLETE]: Benchmark receipt family and corpus-gate ownership — landed as `.planning/Observability/benchmarks.md` with the gate fold, bundle capture seam, and span-profile correlation.
