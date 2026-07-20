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
- Shape: agent rows on the `Observability/benchmarks.md` profile-correlation cluster — `Profiler.Instance` tracking toggles (`SetCPUTrackingEnabled`/`SetAllocationTrackingEnabled`/`SetExceptionTrackingEnabled`) as policy rows, label scopes through `LabelSet.BuildUpon` and `LabelsWrapper.Do`, dynamic tags through `SetDynamicTag`.
- Unlocks: per-tenant flame graphs; allocation and contention profiles cut by the same dimensions the metric governor caps.
- Anchors: the `api-pyroscope.md` two-package catalog, the `pyroscope.profile.id` span tag, the `TenantContext` dimension row.
- Tension: label cardinality shares the tenant-cap governor's budget — label vocabularies stay bounded, never free-form.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

- [0001]-[COMPLETE]: Instruments, hooks, and benchmarks pages authored under `.planning/Observability/`; `InstrumentRow` widened to carry unit, description, and bind; `FaultBand` gained the `Hook` and `Benchmark` rows; `BenchmarkReceipt` joined the `AppHostWireContext` roster.
- [0002]-[COMPLETE]: `OpenTelemetry.Instrumentation.AspNetCore` and `OpenTelemetry.Instrumentation.GrpcNetClient` admitted — csproj rows, README registry rows, `.api` catalogs, and the `SignalGovernance` gRPC-client registration row landed.
