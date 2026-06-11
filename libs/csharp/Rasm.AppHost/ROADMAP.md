# [RASM_APPHOST_ROADMAP]

`Rasm.AppHost` implementation starts from a manifest-backed runtime package and proceeds through one runtime rail.

## [1]-[CURRENT_POSITION]

| [INDEX] | [SURFACE]         | [STATE]                         |
| :-----: | :---------------- | :------------------------------ |
|   [1]   | Project graph     | solution node present           |
|   [2]   | Package graph     | runtime and companion admitted  |
|   [3]   | Production source | absent                          |
|   [4]   | API catalogues    | package lookup pages maintained |
|   [5]   | Boundary tests    | required with source            |

## [2]-[IMPLEMENTATION_TASKS]

[APPHOST_FOLDER_ARCHITECTURE]:
- Status: QUEUED
- Exit: owner folders, rail entrypoints, generated shapes, runtime ports, typed receipts, lifecycle transitions, and boundary adapters are planned before production source.
- Proof: architecture plan consumes every AppHost package API catalogue and names the runtime spine owners.

[APPHOST_RUNTIME_STATE]:
- Status: QUEUED
- Exit: one lifecycle owner carries boot, ready, running, degraded, draining, unloaded, faulted, and support-capture states.
- Proof: managed state-transition specs and AppHost boundary test admission.

[APPHOST_PORTS]:
- Status: QUEUED
- Exit: UI, store, compute, support, health, observability, and outbound-hop ports enter as typed runtime records.
- Proof: dependency tests show AppHost remains below implementation packages.

[APPHOST_COMPANION_BOOTSTRAP]:
- Status: QUEUED
- Exit: Generic Host, DI, configuration, options, Scrutor, health checks, telemetry, validation, resilience, and object pooling feed runtime ports.
- Proof: companion bootstrap specs and package graph restore.

[APPHOST_SUPPORT_EXPORT]:
- Status: QUEUED
- Exit: support trigger, correlation, collection window, size cap, redaction handoff, and package artifact contribution fold into one export receipt.
- Proof: support receipt specs and redaction boundary tests.

## [3]-[CATALOGUE_USE]

[RUNTIME_CATALOGUES]:
- Status: REQUIRED
- Action: runtime design consumes logging, NodaTime, dataflow, and object-pool catalogues.
- Exit: runtime owners name lifecycle state, telemetry identity, drain order, and receipt projection.

[COMPANION_CATALOGUES]:
- Status: REQUIRED
- Action: companion design consumes hosting, DI, configuration, binder, options, health, and Scrutor catalogues.
- Exit: companion owners name bootstrap entrypoints, options binding, health projection, and composition boundaries.

[RESILIENCE_CATALOGUES]:
- Status: REQUIRED
- Action: outbound design consumes HTTP resilience, OpenTelemetry, and OpenTelemetry hosting catalogues.
- Exit: outbound owners name retry policy, telemetry projection, and one owner per remote boundary.

[VALIDATION_CATALOGUES]:
- Status: REQUIRED
- Action: validation design consumes FluentValidation and validation DI catalogues.
- Exit: validation owners name external input contracts, typed failure rails, and composition entrypoints.
