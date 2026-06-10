# [RASM_APPHOST_ROADMAP]

`Rasm.AppHost` is built through capability-backed runtime rails. Package lanes follow owner responsibility; central pins are executable graph facts.

## [1]-[CAPABILITY_RAILS]

| [INDEX] | [RAIL]             | [EXIT_STATE]                                                                                  |
| :-----: | ------------------ | --------------------------------------------------------------------------------------------- |
|   [1]   | Runtime spine      | One runtime surface carries cancellation, time, clock, ports, config                          |
|   [2]   | Neutral ports      | UI, store, compute, support, health, and observability adapt through one boundary             |
|   [3]   | Lifecycle states   | Boot, ready, running, degraded, draining, unloaded, faulted, and support capture are explicit |
|   [4]   | Drain policy       | Deadline, cancellation, operation fencing, and disposal order are executable                  |
|   [5]   | Health projection  | Health derives from typed capability states and semantic timestamps                           |
|   [6]   | Degradation policy | Degradation is config data and receipt output, not inline branching                           |
|   [7]   | Observability      | BCL `ActivitySource` and `Meter` identities are stable                                        |

## [2]-[COMPANION_RAILS]

| [INDEX] | [RAIL]              | [CONTRACT]                                                              |
| :-----: | ------------------- | ----------------------------------------------------------------------- |
|   [1]   | Generic Host        | Companion executable/service owns process lifecycle                     |
|   [2]   | DI/Scrutor          | Companion composition root scans/decorates real services                |
|   [3]   | Config/options      | Companion owner binds and validates runtime config                      |
|   [4]   | Health checks       | Companion owner exposes `Microsoft.Extensions.Diagnostics.HealthChecks` |
|   [5]   | Serilog/OTel export | Companion exporter root owns sinks/providers                            |
|   [6]   | HTTP resilience     | One typed outbound HTTP hop owns `Microsoft.Extensions.Http.Resilience` |
|   [7]   | Object pooling      | Allocation proof identifies a pooled object family                      |

Companion packages form one bootstrap rail: `Microsoft.Extensions.Hosting`, DI/config/options packages, `Scrutor`, health checks, Serilog, OpenTelemetry, and HTTP resilience. Companion processes, hidden sidecars, bridge services, test hosts, and service-backed integrations use the same lifecycle states, drain policy, health projection, support evidence, and telemetry correlation as in-process AppHost composition.

## [3]-[IMPLEMENTATION_DOCTRINE]

- AppHost source never references AppUi, Compute, Persistence, Rhino, or Grasshopper implementation assemblies.
- AppUi, Compute, and Persistence adapt to AppHost runtime policy through narrow ports.
- Rhino/GH2 app roots compose AppHost and host-boundary packages; AppHost never calls native host APIs.
- Runtime packages return typed receipts. Generic `IReceipt` ledgers are not introduced.
- State machines are closed and transition-driven. Independent booleans, string status fields, and parallel lifecycle enums are rejected.
- Dispatch is polymorphic and table-driven where variants grow; branching clusters collapse into one rail before new entrypoints appear.
- Package-specific APIs stay behind the rail that owns the package. Public runtime vocabulary stays provider-neutral.

## [4]-[VALIDATION]

| [INDEX] | [GATE]       | [REQUIRED_STATE]                                                |
| :-----: | ------------ | --------------------------------------------------------------- |
|   [1]   | Restore      | AppHost project lockfile is current                             |
|   [2]   | Build        | AppHost package graph builds                                    |
|   [3]   | Architecture | AppHost remains below implementation app packages               |
|   [4]   | Package      | Direct/transitive package checks are clean                      |
|   [5]   | Runtime      | Rhino/GH2 scenario rail proves drain behavior                   |
