# [RASM_APPHOST_ROADMAP]

`Rasm.AppHost` implementation starts from a manifest-backed runtime package and proceeds through one runtime rail.

## [1]-[CURRENT_POSITION]

This table is a lookup by implementation surface.

| [INDEX] | [SURFACE]         | [STATE]                         |
| :-----: | :---------------- | :------------------------------ |
|   [1]   | Project graph     | solution node present           |
|   [2]   | Package graph     | runtime and companion admitted  |
|   [3]   | Production source | absent                          |
|   [4]   | API catalogues    | package lookup pages maintained |
|   [5]   | Boundary tests    | required with source            |

## [2]-[IMPLEMENTATION_TASKS]

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

## [3]-[PACKAGE_PROOF]

This table is a lookup by package rail.

| [INDEX] | [RAIL]        | [REQUIRED_STATE]                         |
| :-----: | :------------ | :--------------------------------------- |
|   [1]   | Runtime       | logging, time, diagnostics, channels     |
|   [2]   | Functional    | rails and generated shapes inherited     |
|   [3]   | Companion     | host, DI, config, options, health, export |
|   [4]   | Resilience    | outbound HTTP policy has one owner       |
|   [5]   | Validation    | external input folds to typed rails      |
