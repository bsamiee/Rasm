# [RASM_APPHOST_AGENTS]

Scope: `libs/csharp/Rasm.AppHost/` only. Root policy and `libs/csharp/AGENTS.md` own universal C# and library-family rules; local README, `_ARCHITECTURE.md`, and `ROADMAP.md` own platform state and implementation sequence.

## [1][READ_ORDER]

- Before production work, read `README.md`, `_ARCHITECTURE.md`, and `ROADMAP.md` to decide platform state, package adoption, and sequence.
- Before moving package guidance into graph, read `docs/host-libraries.md`.
- Before changing cross-project runtime contracts, read AppUi, Compute, and Persistence local overlays.
- Before creating project files, solution membership, central package entries, or directory-prop changes, read root quality policy and `docs/system-api-map`.

## [2][OWNER_CONTRACT]

`Rasm.AppHost` is the unified runtime platform. It coordinates boot, lifecycle, degradation policy, scheduling, diagnostics, and typed cross-capability receipts; it does not own UI rendering, durable storage internals, compute substrate execution, domain logic, or host SDK behavior.

Keep one runtime record and one capability boundary. Expose typed receipts and value records, not service-provider access, generic ledgers, or per-package subsystems.

## [3][EXTENSION_GRAMMAR]

- Runtime lifecycle case: extend the runtime record and lifecycle rail before adding a subsystem.
- Cross-capability receipt: extend the typed receipt stream with slot/kind metadata before adding a ledger.
- Scheduling or drain behavior: extend the runtime scheduler owner and route execution to Compute.
- Telemetry or logging: keep API-level in-process signals here; SDK/exporter adoption belongs at the companion or composition root.
- Configuration: pass bound value data into boot; do not introduce configuration pipes inside the runtime core.

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
- No package versions in documentation text; version truth lives in central manifests.
- No raw host SDK assumptions without the host-boundary route that proves them.
