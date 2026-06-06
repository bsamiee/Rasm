# [RASM_APPHOST_AGENTS]

Scope: `libs/csharp/Rasm.AppHost/` only. `Rasm.AppHost` is the in-process runtime-record rail; Generic Host, DI roots, Scrutor, Serilog sinks, OpenTelemetry SDK/exporters, and companion service roots stay companion/test/bridge concerns unless architecture and manifest proof move them into a composition root.

Root policy and `libs/csharp/AGENTS.md` own universal C# and library-family rules; local README, `ARCHITECTURE.md`, and `ROADMAP.md` own platform state and implementation sequence.

## [1][READ_ORDER]

- Before production work, read `README.md`, `ARCHITECTURE.md`, and `ROADMAP.md` to decide platform state, package adoption, and sequence.
- Before moving package guidance into graph, read `docs/host-libraries.md`.
- Before changing cross-project runtime contracts, read AppUi, Compute, and Persistence local overlays.
- Before creating project files, solution membership, central package entries, or directory-prop changes, read root quality policy and `docs/system-api-map`.

## [2][OWNER_CONTRACT]

`Rasm.AppHost` is the in-process runtime-record platform. Generic Host, DI containers, Scrutor, Serilog sinks, OpenTelemetry SDK/exporters, and companion service roots are companion/test/bridge concerns only unless architecture and manifest proof explicitly move them into a composition root.

It coordinates boot, lifecycle, degradation policy, scheduling, diagnostics, and typed cross-capability receipts; it does not own UI rendering, durable storage internals, compute substrate execution, domain logic, or host SDK behavior. Keep one runtime record and one capability boundary. Expose typed receipts and value records, not service-provider access, generic ledgers, or per-package subsystems.

## [3][EXTENSION_GRAMMAR]

- Runtime lifecycle case: extend `RasmRuntime`, lifecycle receipts, and capability boundary records before adding a subsystem.
- Cross-capability receipt: extend the typed receipt stream with slot/kind metadata before adding a ledger.
- Scheduling or drain behavior: extend the runtime scheduler owner and route execution to Compute.
- Telemetry or logging: keep API-level in-process signals here; SDK/exporter adoption belongs at the companion or composition root.
- Configuration: pass bound value data into boot; do not introduce configuration pipes inside the runtime core.
- Package-backed runtime behavior: read `ARCHITECTURE.md` and central manifests, then internalize approved package capability into `RasmRuntime`, lifecycle rails, scheduler/drain policy, and typed receipts before adding config knobs, service registries, facade methods, or compatibility aliases.

## [4][BOUNDARY_RULES]

| [INDEX] | [CONCERN]     | [RULE]                                                       |
| :-----: | :------------ | :----------------------------------------------------------- |
|   [1]   | UI            | AppUi renders and owns scheduler construction                |
|   [2]   | Compute       | AppHost schedules and drains; Compute executes requests      |
|   [3]   | Persistence   | AppHost submits durable work; Persistence stores and queries |
|   [4]   | Telemetry     | In-process path owns API-level signals only                  |
|   [5]   | Configuration | Runtime receives bound value data, not configuration pipes   |
|   [6]   | Shutdown      | Drain compute before persistence, then complete UI signals   |
|   [7]   | Host SDK      | Rhino/GH2 facts route to their host-boundary owners          |

## [5][REJECTIONS]

- No domain logic, UI rendering, persistence context ownership, or compute substrate implementation.
- No public `IServiceProvider` or generic runtime service-location API.
- No duplicate scheduling, retry, telemetry, logging, or config rail beside the runtime owner.
- No companion-service packages inside the in-process plugin path unless the architecture and root package route prove the boundary.
- No in-process Generic Host boot, exporter SDK, raw host SDK call, or unproved shutdown/drain behavior without architecture and host proof.
- No package versions in documentation text; version truth lives in central manifests.
- No raw host SDK assumptions without the host-boundary route that proves them.

## [6][STOP_RULES]

If a change requires in-process `IServiceProvider`, Generic Host boot, exporter SDKs, raw host SDK calls, unproved shutdown/drain behavior, or companion package adoption without architecture and host proof, stop and route to `ARCHITECTURE.md`, `docs/host-libraries.md`, and manifest proof.
