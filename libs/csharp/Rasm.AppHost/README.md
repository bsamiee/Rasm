# [RASM_APPHOST]

`Rasm.AppHost` is the runtime spine for product app packages. It owns neutral composition policy, lifecycle, drain, health, telemetry correlation, support-bundle triggering, degradation policy, and companion-process boundaries.

## [1]-[PURPOSE]

`Rasm.AppHost` provides the shared runtime doctrine consumed by AppUi, Compute, Persistence, Rhino/GH2 plugin roots, bridge roots, and companion processes. In-process Rhino/GH2 apps compose a host-neutral runtime record. Companion processes may use Generic Host, DI, configuration binding, exporters, and service-backed integrations through the same runtime doctrine.

It is not a domain service layer, job framework, DI wrapper, telemetry wrapper, UI package, persistence package, compute implementation, or shared-contracts project.

## [2]-[STATUS]

| [INDEX] | [SURFACE]          | [STATE]                                             |
| :-----: | ------------------ | --------------------------------------------------- |
|   [1]   | Project file       | Present in `Workspace.slnx`                         |
|   [2]   | Production source  | Runtime rail contract defined                       |
|   [3]   | Package references | Active setup references are versionless             |
|   [4]   | Runtime mode       | Host-neutral runtime spine                          |
|   [5]   | Companion mode     | First-class lane behind the same lifecycle doctrine |
|   [6]   | Boundary proof     | Assembly laws cover implementation assemblies       |

## [3]-[CONSTRAINTS]

- AppHost owns one host-neutral runtime spine. App packages adapt to it; AppHost never imports AppUi, Compute, Persistence, Rhino, or GH2 implementation records.
- AppHost is built as a complete runtime package for Rhino/GH2 plugin roots, companion processes, hidden sidecars, bridge services, service-backed integrations, test hosts, and adjacent platform packages through the same runtime rail.
- Neutral ports are AppHost-owned concepts: UI scheduling, store dispatch, compute dispatch, runtime config, health, degradation, lifecycle, support triggering, and observability.
- Runtime lifecycle is explicit state, not scattered booleans or independent shutdown flags.
- Folder architecture is rail-first: runtime state, drain state, health state, degradation policy, support capture, telemetry correlation, and companion bootstrap deepen the same state/receipt shapes instead of adding sibling enum, service, helper, or wrapper families.
- Configuration, policy, deadlines, health contributors, exporters, validation boundaries, and outbound hops are parameterized inputs to the runtime rail, not hardcoded startup modes.
- In-process observability is BCL-owned through `System.Diagnostics.ActivitySource`, `Meter`, counters, and histograms. OpenTelemetry SDK/exporters are companion roots only.
- `NodaTime.Instant` owns semantic timestamps in receipts and health snapshots. `TimeProvider` owns elapsed time, delays, deadlines, cancellation windows, and deterministic tests.
- Drain deadlines use supported TimeProvider-aware APIs: `CancellationTokenSource(TimeSpan, TimeProvider)`, `Task.Delay(TimeSpan, TimeProvider, ...)`, or `Task.WaitAsync(TimeSpan, TimeProvider, ...)`.
- Generic Host, DI, Scrutor, configuration binding, options validation, health-check export, Serilog, and OpenTelemetry exporters belong to companion roots. They do not start inside the Rhino/GH2 in-process plugin path.
- Companion apps, hidden sidecars, explicit external companions, bridge services, and service-backed integrations use the same runtime states, drain states, health states, support states, and telemetry correlation.
- Support bundles are exported diagnostics packages: UI receipts, screenshots, logs, store/migration/corruption receipts, compute/runtime evidence, and redacted artifact metadata.
