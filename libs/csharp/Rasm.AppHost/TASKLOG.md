# [APPHOST_TASKLOG]

Open and closed work for the runtime spine, distilled from `IDEAS.md`. Each open task carries a status marker and the capability-to-build, packages, integration points/boundaries, and key considerations; one idea spawns one or more tasks across one or more files. Closed cards record already-settled cleanup.

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

[0003]-[QUEUED]: Resource-detector rows — five contrib detectors enrich the minted identity triple at `ConfigureResource`.
- Capability: `host.name`/`host.id`/`host.arch`, `os.type`/`os.version`/`os.build_id`, `process.pid`/`process.owner`/`process.creation.time`, `process.runtime.name`/`process.runtime.version`, and `container.id` arrive as detector rows beside the minted `service.namespace`/`service.name`/`service.instance.id` triple — placement dimensions no backend derives from the triple alone.
- Shape: one detector-row table on `Observability/telemetry.md` at the `ConfigureResource` seam — `AddHostDetector`/`AddOperatingSystemDetector`/`AddProcessDetector`/`AddProcessRuntimeDetector` always-on, `AddContainerDetector` a `HostProfile`-gated row because a container id resolves only under a containerized deploy; detectors augment and never replace the mint.
- Unlocks: fleet-grain filtering by host, runtime, and container across every OTLP backend without per-signal enrichment rows.
- Anchors: `api-otel-resources.md`, the resource-identity governance row, the per-ALC provider capsule — detector rows ride both the hosted root and the `Sdk.Create*ProviderBuilder` capsules.
- Tension: all five packages ride prerelease-only channels; the manifest comment pins each to its sole published channel.
- Atomic: detector-row table on one page.

[0004]-[QUEUED]: Outbound HttpClient latency breakdown — `Microsoft.Extensions.Http.Diagnostics` checkpoints the phases a client span cannot see.
- Capability: per-phase latency checkpoints and enriched outbound request logs over every AppHost-owned `HttpClient` path — webhook probes, OIDC flows, MCP and live-wire HTTP legs — joined to the latency-context spine the telemetry page already composes.
- Shape: `AddHttpClientLatencyTelemetry()` with `HttpClientLatencyTelemetryOptions.EnableDetailedLatencyBreakdown` and `AddExtendedHttpClientLogging(LoggingOptions)` rows beside the `AddHttpClientInstrumentation` span row; the four `LoggingOptions` `*DataClasses` maps bind the redaction taxonomy, and bespoke enrichment lands as `AddHttpClientLogEnricher<T>` rows.
- Unlocks: slow-hop attribution to name-resolution versus connection versus server time at checkpoint cost, never child-span cost.
- Anchors: `api-http-diagnostics.md`, the `api-extensions-telemetry.md` latency context, the `Http.Resilience` handler chain ordering.
- Tension: checkpoints complement the `OpenTelemetry.Instrumentation.Http` span — breakdown and span are two projections of one hop, and a second trace is the defect the composition guards against.

[0005]-[QUEUED]: Profiler agent labels — the `Pyroscope` agent surface partitions continuous profiles by the dimensions instruments already carry.
- Capability: dynamic profile labels thread tenant, command family, and degradation level into the profile stream `PyroscopeSpanProcessor` already correlates by span id — flame graphs answer per-tenant and per-command cost questions the aggregate profile cannot.
- Shape: agent rows on the `libs/csharp/Rasm.AppHost/.planning/Observability/benchmarks.md#PROFILE_CORRELATION` cluster — `Profiler.Instance` tracking toggles (`SetCPUTrackingEnabled`/`SetAllocationTrackingEnabled`/`SetExceptionTrackingEnabled`/`SetContentionTrackingEnabled`) as policy rows, label scopes through `LabelSet.BuildUpon` and `LabelsWrapper.Do`, dynamic tags through `SetDynamicTag`.
- Unlocks: per-tenant flame graphs; allocation and contention profiles cut by the same dimensions the metric governor caps.
- Anchors: the `api-pyroscope.md` two-package catalog, the `pyroscope.profile.id` span tag, the `TenantContext` dimension row.
- Tension: label cardinality shares the tenant-cap governor's budget — label vocabularies stay bounded, never free-form.

[0006]-[QUEUED]: Carrier adapter rows — NATS headers, MQTT v5 user properties, and CloudEvents tracing attributes land as three `IPropagator`-backed adapters on the propagation spine with pinned member spellings.
- Capability: getter/setter pairs over `NatsHeaders`, the MQTTnet user-property collection, and the CloudEvents `traceparent`/`tracestate` extension attributes delegate to `Correlation.Spine` `Inject`/`Extract`, so a broker hop continues the W3C trace and baggage.
- Shape: three adapter rows on `libs/csharp/Rasm.AppHost/.planning/Observability/telemetry.md#CORRELATION_SPINE` mirroring the `TraceContext` gRPC shape; mount edges at the `libs/csharp/Rasm.AppHost/.planning/Wire/livewire.md` `MqttLane` publish and receive legs; exact member spellings pin against `api-mqtt.md` and the Persistence NATS and CloudEvents catalogs before the fences land.
- Unlocks: realizes `[WIRE_CARRIER_ADAPTERS]` — trace continuity and exemplar survival across every estate broker hop.
- Anchors: the `[WIRE_CARRIER_ADAPTERS]` idea, `TextMapPropagator.Inject`/`Extract`, `MqttApplicationMessageBuilder`, `NatsHeaders`, the CloudEvents extension-attribute surface.

[0007]-[QUEUED]: Persistent OTLP transmission queue — file-system-backed export durability rows at both provider owners.
- Capability: failed OTLP exports persist to a bounded on-disk queue and replay on reconnect; provider shutdown flushes wire-first, store-second.
- Shape: persistent-storage binding rows at `SignalGovernance.Govern` and `PluginTelemetryHost.Open` on `libs/csharp/Rasm.AppHost/.planning/Observability/telemetry.md#SIGNAL_GOVERNANCE` and `libs/csharp/Rasm.AppHost/.planning/Observability/instruments.md#PROVIDER_LIFETIME`; queue-directory, size-cap, and retention policy values join the governance table.
- Unlocks: realizes `[TELEMETRY_OFFLINE_SPINE]` — the durability claim holds on every host profile.
- Anchors: `OpenTelemetry.PersistentStorage.FileSystem` (admission pending), the `OTEL_EXPORTER_OTLP_*` rows, the telemetry drain band.

[0008]-[QUEUED]: Baggage-to-span promotion rows — `BaggageActivityProcessor` admits the tenant and correlation keys onto span attributes at every provider owner.
- Capability: `rasm.tenant` and `CorrelationId` baggage entries promote to span attributes under a key-allowlist predicate; unlisted baggage never leaks onto spans.
- Shape: one `AddProcessor` row beside the `PyroscopeSpanProcessor` seat and one allowlist policy row on `libs/csharp/Rasm.AppHost/.planning/Observability/telemetry.md#SIGNAL_GOVERNANCE`; the per-ALC capsule takes the same processor row.
- Unlocks: realizes `[TENANT_COST_SPAN_PROMOTION]` — backend per-tenant grouping over spend, latency, and traces.
- Anchors: `OpenTelemetry.Extensions` `BaggageActivityProcessor` (admission pending), `Correlation.Spine`, `TenantContext.Tag`.
- Atomic: two registration rows and one policy row on one page.

[0009]-[QUEUED]: `RollAnnotationWire` record — wave, channel, verdict, host-count, and instant fields fold off the roll receipts into one deploy-annotation wire shape.
- Capability: every wave advance, hold, and rollback lands one annotation record on the receipt fan, HLC-stamped like every spine fact.
- Shape: one wire record and projection fold on `libs/csharp/Rasm.AppHost/.planning/Sandbox/provisioning.md#ROLLOVER_DRAIN`; the record joins the `AppHostWireContext` roster at `libs/csharp/Rasm.AppHost/.planning/Runtime/ports.md#WIRE_LAW`.
- Unlocks: realizes `[FLEET_DEPLOY_ANNOTATIONS]` — the dashboard timeline marks every fleet wave.
- Anchors: `FleetRoll` wave conduct, `ReceiptSinkPort.Send`, the TypeScript iac deploy-annotation rail.

[0010]-[QUEUED]: `DumpTriage` fold — ClrMD walks the captured dump into bounded triage rows inside the bundle manifest.
- Capability: top heap types by retained size, per-thread managed stacks, and blocked-thread evidence land as typed rows under the bundle caps; the raw dump artifact keeps its key beside the triage.
- Shape: one triage fold and one `SupportArtifact` factory row on `libs/csharp/Rasm.AppHost/.planning/Observability/bundles.md#CAPTURE_PIPELINE`; row caps and the capture-window deadline bound the walk.
- Unlocks: realizes `[DIAGNOSTIC_HEAP_ANALYSIS]` — debugger-free first response from the bundle alone.
- Anchors: `Microsoft.Diagnostics.Runtime` `DataTarget.LoadDump` (admission pending), `DumpPolicy`, `SupportManifest.Entry`.

[0011]-[QUEUED]: Partitioned observable gauges — keyed level atoms bind `Measurement<T>` multi-value callbacks where a partition dimension exists.
- Capability: per-probe health, per-channel outbox lag, and per-family roster series read from one callback each; scalar cells stay for un-partitioned levels.
- Shape: keyed-atom widening of `LevelCells` and multi-measurement `CreateObservableGauge` bindings on `libs/csharp/Rasm.AppHost/.planning/Observability/instruments.md#INSTRUMENT_CATALOG`; partition tag vocabularies bind to their owning SmartEnum rows.
- Unlocks: realizes `[PARTITIONED_OBSERVABLE_LEVELS]` — partitioned dashboards inside the existing series budget.
- Anchors: `api-diagnostics-metrics.md` `Measurement<T>`, `DriverProbe.Items`, the `*` `AddView` cardinality row.

[0012]-[QUEUED]: Contributed-arm rows — six sibling kind partitions and their contributor ports pin onto the fan mount.
- Capability: arm-table and port rows for kernel, Bim, Element, Grasshopper, Materials, and Rhino beside the Persistence precedent; the duplicate-kind fault law restates per row; the kernel hook-tap subscriber row lands at the same composition point.
- Shape: rows on `libs/csharp/Rasm.AppHost/.planning/Observability/instruments.md#RECEIPT_PROJECTION`; plugin-profile custody rows at `libs/csharp/Rasm.AppHost/.planning/Observability/instruments.md#PROVIDER_LIFETIME` name the Grasshopper and Rhino ALC capsules.
- Unlocks: realizes `[CONTRIBUTED_ARM_ROSTER]` — the fan mount names every branch contributor.
- Anchors: `InstrumentFan.Mount`, `TelemetryContributorPort`, `PluginTelemetryHost.Open`.

[0013]-[QUEUED]: Claim-ingest rows — the kernel ledger, Materials workloads, and the Rhino harvest lane fold to gate cases.
- Capability: each sibling claim family maps to `BenchmarkReceipt` fields and judges under the standing `GatePolicy`; the bridge-run harvest lane declares its in-host capture provenance beside the BenchmarkDotNet corpus rows.
- Shape: ingestion rows on `libs/csharp/Rasm.AppHost/.planning/Observability/benchmarks.md#BENCHMARK_RECEIPT`.
- Unlocks: realizes `[CORPUS_GATE_INGEST]` — one verdict grammar over every sibling family.
- Anchors: `BenchmarkGate.Gate`/`Judge`, `HostEvidence` binding, the Persistence claim custody.

[0014]-[QUEUED]: Profile-sample feed port — capture stacks keyed by correlation expose as one composition-bound delegate.
- Capability: Pyroscope span profiles and EventPipe stacks keyed by correlation surface through one delegate port the AppUi devloop `FlameNode` fold consumes as samples.
- Shape: one port record on `libs/csharp/Rasm.AppHost/.planning/Runtime/ports.md#WIRE_LAW` and its capture binding at `libs/csharp/Rasm.AppHost/.planning/Observability/benchmarks.md#PROFILE_CORRELATION`.
- Unlocks: host CPU stacks and receipt durations merge into one flame surface on the dev loop.
- Anchors: `api-pyroscope.md` agent surface, EventPipe capture seam, the AppUi FlameNode fold.
- Ripple: `Rasm.AppUi` `[PROFILE-FLAME-JOIN]`.

[0015]-[QUEUED]: Schema-pinned meter mint — the contributor port carries the semconv schema coordinate and the mint stamps it.
- Capability: `TelemetryContributorPort` gains the schema coordinate; `TelemetryIdentity.Mint` applies `MeterOptions.TelemetrySchemaUrl` at every contributor mint, so each contributed roster satisfies the wire law's schema pin.
- Shape: one port field and one mint row on `libs/csharp/Rasm.AppHost/.planning/Observability/instruments.md#RECEIPT_PROJECTION` beside the `TelemetryIdentity` owner.
- Unlocks: schema-aware backends read every `rasm.*` scope with pinned semantics.
- Anchors: branch `api-diagnostics-metrics.md` `MeterOptions.TelemetrySchemaUrl` law row, the Persistence contributor-side coordinate.
- Ripple: `Rasm.Persistence` `[0019]`.
- Atomic: one port field and one mint stamp.

[0016]-[QUEUED]: Observation decode rows — the `MachineObservation` record, decode fold, freshness stamps, and the Fabrication port crossing.
- Capability: transport bytes fold to typed observation records with machine identity and freshness instants; the crossing record joins the port vocabulary so Fabrication consumes one slice.
- Shape: decode-lane rows on `libs/csharp/Rasm.AppHost/.planning/Wire/livewire.md` and one port record on `libs/csharp/Rasm.AppHost/.planning/Runtime/ports.md#WIRE_LAW`.
- Unlocks: realizes `[MACHINE_OBSERVATION_LANE]` — wear, fleet, and engagement consumers rebind onto decoded truth.
- Anchors: the Fabrication decode-lane invariant, the MTConnect observation model, the wire lane owners.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

- [0001]-[COMPLETE]: Instruments, hooks, and benchmarks pages authored under `.planning/Observability/`; `InstrumentRow` widened to carry unit, description, and bind; `FaultBand` gained the `Hook` and `Benchmark` rows; `BenchmarkReceipt` joined the `AppHostWireContext` roster.
- [0002]-[COMPLETE]: `OpenTelemetry.Instrumentation.AspNetCore` and `OpenTelemetry.Instrumentation.GrpcNetClient` admitted — csproj rows, README registry rows, `.api` catalogs, and the `SignalGovernance` gRPC-client registration row landed.
