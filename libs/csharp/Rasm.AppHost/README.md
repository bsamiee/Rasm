# [RASM_APPHOST]

`Rasm.AppHost` is the host-neutral runtime spine for app packages. It owns runtime
composition policy, lifecycle state, drain order, health projection, telemetry
identity, degradation, support triggering, companion bootstrap, and outbound-hop
ownership as one runtime rail.

## [1]-[PURPOSE]

`Rasm.AppHost` defines the runtime contract consumed by app packages and app roots.
AppUi, Compute, Persistence, Rhino/GH2 app roots, bridge roots, companion processes,
hidden sidecars, service integrations, and test hosts adapt to AppHost runtime state
instead of creating local lifecycle systems.

It is not a domain service layer, job framework, dependency-injection wrapper,
telemetry wrapper, UI package, persistence package, compute implementation,
host-boundary package, or shared-contracts project.

## [2]-[STACK_DOCTRINE]

| [INDEX] | [READ_FOR]        | [OPEN]                                            |
| :-----: | :---------------- | :------------------------------------------------ |
|   [1]   | C# package law    | [C# stack](../../../docs/stacks/csharp/README.md) |
|   [2]   | package API facts | [.reports/api](.reports/api/README.md)            |

Implementation planning follows the C# stack atlas and finalized concept pages. Package-local documents state AppHost law and package API facts only.

## [3]-[STATUS]

| [INDEX] | [SURFACE]         | [STATE]                                        |
| :-----: | :---------------- | :--------------------------------------------- |
|   [1]   | Project file      | present in `Workspace.slnx`                    |
|   [2]   | Package manifest  | runtime and companion packages admitted        |
|   [3]   | Lockfile          | restored package closure tracked               |
|   [4]   | Production source | absent                                         |
|   [5]   | Package law       | documented in this folder                      |
|   [6]   | Boundary proof    | required when source enters architecture tests |

## [4]-[DOCUMENTS]

| [INDEX] | [READ_FOR]              | [OPEN]                                 |
| :-----: | :---------------------- | :------------------------------------- |
|   [1]   | current structure       | [architecture](ARCHITECTURE.md)        |
|   [2]   | implementation sequence | [roadmap](ROADMAP.md)                  |
|   [3]   | package API catalogues  | [.reports/api](.reports/api/README.md) |

## [5]-[CONSTRAINTS]

- AppHost owns one host-neutral runtime spine. App packages adapt to it; AppHost never references AppUi,
  Compute, Persistence, Rhino, Grasshopper, Eto, or host implementation assemblies.
- Runtime lifecycle is explicit state with typed transition receipts. Independent shutdown flags, string status fields, and sibling lifecycle enums are rejected.
- UI scheduling, store dispatch, compute dispatch, runtime config, health, degradation, support triggering,
  outbound hops, and observability enter as runtime ports and policy values.
- Configuration, deadlines, health contributors, exporters, validators, outbound clients, object pools, and companion bootstrap shape are parameterized inputs to the runtime rail.
- In-process observability uses `ActivitySource`, `Meter`, counters, and histograms. Exporters belong to
  companion bootstrap and project AppHost signals outward without replacing runtime receipts.
- `NodaTime.Instant` owns semantic timestamps in receipts and health snapshots. `TimeProvider` owns elapsed time, delays, deadlines, cancellation windows, and deterministic tests.
- Companion processes, hidden sidecars, bridge services, and service-backed integrations use Generic Host,
  DI, configuration, options, health, telemetry, validation, resilience, and object pooling as one bootstrap
  rail over the AppHost state machine.
- Support bundles are bounded diagnostic exports with one correlation spine, redaction policy, collection window, size cap, and package-owned artifact contributions.
