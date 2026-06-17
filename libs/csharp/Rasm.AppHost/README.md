# [APPHOST]

`Rasm.AppHost` is one runtime spine with zero consumers; the implementation is full-capability with no holding back. The `.planning/` pages are decision-complete blueprints an implementation agent transcribes — never re-designed downstream. The spine owns host variance, lifecycle, time, configuration, composition, resource lanes, telemetry, health, support capture, outbound resilience, post-fetch provisioning and update, the inbound control-service host with its process-modality and degradation-cascade legs, the capability/MCP/sandbox/solver/live-wire/determinism platform legs, and the cross-package port surface; siblings adapt to its ports and never reverse the dependency. Owner-state and the rails/axes registry live in `ARCHITECTURE.md`; the realized capability list in `FEATURES.md`; open work in `TASKLOG.md`.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                                                              | [OWNS]                                                                                          |
| :-----: | :------------------------------------------------------------------ | :---------------------------------------------------------------------------------------------- |
|   [1]   | [host-profiles](Hosting/.planning/host-profiles.md)                         | host-variance axis; modality rows                                                               |
|   [2]   | [lifecycle-and-drain](Hosting/.planning/lifecycle-and-drain.md)             | phase, transition, drain, cancellation, markers                                                 |
|   [3]   | [time-and-deadlines](Time/.planning/time-and-deadlines.md)                  | clocks, deadlines, schedule port, test clocks                                                   |
|   [4]   | [configuration-and-options](Configuration/.planning/configuration-and-options.md) | source layering, binding, frozen policy, reloads                                                |
|   [5]   | [composition-and-modules](Configuration/.planning/composition-and-modules.md)     | composition root and module contribution rows                                                   |
|   [6]   | [resource-lanes](Resources/.planning/resource-lanes.md)                     | cache lane, object pools, drainable queues                                                      |
|   [7]   | [diagnostics-and-telemetry](Observability/.planning/diagnostics-and-telemetry.md) | correlation, signals, classification taxonomy                                                   |
|   [8]   | [health-and-degradation](Observability/.planning/health-and-degradation.md) | health fold and degradation rail                                                                |
|   [9]   | [support-bundles](Observability/.planning/support-bundles.md)               | bounded redacted diagnostic capture                                                             |
|  [10]   | [outbound-resilience](Outbound/.planning/outbound-resilience.md)            | hop axis, single retry owner, discovery manifest, UDS attach, companion spawn                   |
|  [11]   | [runtime-ports](Ports/.planning/runtime-ports.md)                           | port records, suite wire law, TS map                                                            |
|  [12]   | [provisioning-and-update](Provisioning/.planning/provisioning-and-update.md) | post-fetch update rail, channel axis, rollover drain                                            |
|  [13]   | [companion-sidecar](Companion/.planning/companion-sidecar.md)               | process modality, control-service host, degradation cascade, peer admission                     |
|  [14]   | [capability-registry](Capability/.planning/capability-registry.md)          | self-describing op catalog, discovery, command algebra, grant/cost broker, SDK codegen          |
|  [15]   | [mcp-projection](Agent/.planning/mcp-projection.md)                         | MCP tool projection, dry-run preview, streaming progress, resumable handles                     |
|  [16]   | [sandbox-host](Sandbox/.planning/sandbox-host.md)                           | WASM/process isolation, grant broker, quota/kill/quarantine, supply-chain gate                  |
|  [17]   | [solver-plugin](Sandbox/.planning/solver-plugin.md)                         | solver-kind axis, plugin contract, sandboxed hosting, representation negotiation                |
|  [18]   | [live-wire](LiveWire/.planning/live-wire.md)                                | industrial transport axis, reactive binding, edge unit coercion, write-back transaction         |
|  [19]   | [determinism-and-replay](Determinism/.planning/determinism-and-replay.md)   | determinism kernel, content-addressed event log, replay-verify, macro engine, partial recompute |

## [2]-[ADMISSIONS_RECORD]

The executed admissions ledger maps each package to its consuming page, `.api` catalogue, and admission status. Versions live in `Directory.Packages.props`; this table never carries a pin. `[STATUS]` is one of `admitted`, `catalogue-pending`, `app-root-pending`, `tests-only`. App-root-only pins carry `app-root-pending` and are catalogued at app-root creation; the consuming-page row names where the surface lands.

| [INDEX] | [PACKAGE]                                               | [PAGE]                    | [CATALOGUE]                     | [STATUS]         |
| :-----: | :------------------------------------------------------ | :------------------------ | :------------------------------ | :--------------- |
|   [1]   | Cronos                                                  | time-and-deadlines        | api-cronos.md                   | admitted         |
|   [2]   | FluentValidation                                        | configuration-and-options | api-validation.md               | admitted         |
|   [3]   | FluentValidation.DependencyInjectionExtensions          | composition-and-modules   | api-validation-di.md            | admitted         |
|   [4]   | Microsoft.Extensions.Caching.Hybrid                     | resource-lanes            | api-hybrid-cache.md             | admitted         |
|   [5]   | Microsoft.Extensions.Compliance.Redaction               | diagnostics-and-telemetry | api-redaction.md                | admitted         |
|   [6]   | Microsoft.Extensions.Configuration                      | configuration-and-options | api-config.md                   | admitted         |
|   [7]   | Microsoft.Extensions.Configuration.Binder               | configuration-and-options | api-binder.md                   | admitted         |
|   [8]   | Microsoft.Extensions.Configuration.CommandLine          | configuration-and-options | api-config-providers.md         | admitted         |
|   [9]   | Microsoft.Extensions.Configuration.EnvironmentVariables | configuration-and-options | api-config-providers.md         | admitted         |
|  [10]   | Microsoft.Extensions.Configuration.Json                 | configuration-and-options | api-config-providers.md         | admitted         |
|  [11]   | Microsoft.Extensions.Configuration.UserSecrets          | configuration-and-options | api-config-providers.md         | admitted         |
|  [12]   | Microsoft.Extensions.DependencyInjection                | composition-and-modules   | api-di.md                       | admitted         |
|  [13]   | Microsoft.Extensions.Diagnostics.HealthChecks           | health-and-degradation    | api-health.md                   | admitted         |
|  [14]   | Microsoft.Extensions.Diagnostics.ResourceMonitoring     | health-and-degradation    | api-resource-monitoring.md      | admitted         |
|  [15]   | Microsoft.Extensions.Hosting                            | host-profiles             | api-hosting.md                  | admitted         |
|  [16]   | Microsoft.Extensions.Hosting.Systemd                    | host-profiles             | api-hosting-lifetimes.md        | admitted         |
|  [17]   | Microsoft.Extensions.Http.Resilience                    | outbound-resilience       | api-resilience.md               | admitted         |
|  [18]   | Microsoft.Extensions.Logging.Abstractions               | diagnostics-and-telemetry | api-logging.md                  | admitted         |
|  [19]   | Microsoft.Extensions.ObjectPool                         | resource-lanes            | api-objectpool.md               | admitted         |
|  [20]   | Microsoft.Extensions.Options                            | configuration-and-options | api-options.md                  | admitted         |
|  [21]   | Microsoft.Extensions.Telemetry                          | diagnostics-and-telemetry | api-telemetry.md                | admitted         |
|  [22]   | Microsoft.Extensions.Telemetry.Abstractions             | diagnostics-and-telemetry | api-telemetry-abstractions.md   | admitted         |
|  [23]   | NodaTime                                                | time-and-deadlines        | api-nodatime.md                 | admitted         |
|  [24]   | OpenTelemetry                                           | diagnostics-and-telemetry | api-otel.md                     | admitted         |
|  [25]   | OpenTelemetry.Extensions.Hosting                        | diagnostics-and-telemetry | api-otel-hosting.md             | admitted         |
|  [26]   | OpenTelemetry.Instrumentation.Http                      | diagnostics-and-telemetry | api-otel-instrumentation.md     | admitted         |
|  [27]   | OpenTelemetry.Instrumentation.Runtime                   | diagnostics-and-telemetry | api-otel-instrumentation.md     | admitted         |
|  [28]   | Polly.Core                                              | outbound-resilience       | api-polly-core.md               | admitted         |
|  [29]   | Polly.Extensions                                        | outbound-resilience       | api-polly-extensions.md         | admitted         |
|  [30]   | Polly.RateLimiting                                      | outbound-resilience       | api-polly-ratelimiting.md       | admitted         |
|  [31]   | Scrutor                                                 | composition-and-modules   | api-scrutor.md                  | admitted         |
|  [32]   | Serilog                                                 | diagnostics-and-telemetry | api-serilog.md                  | admitted         |
|  [33]   | System.Threading.Tasks.Dataflow                         | resource-lanes            | api-dataflow.md                 | admitted         |
|  [34]   | Thinktecture.Runtime.Extensions.Json                    | runtime-ports             | api-thinktecture-json.md        | admitted         |
|  [35]   | LanguageExt.Core                                        | every page                | DOCTRINE (`docs/stacks/csharp`) | admitted         |
|  [36]   | Thinktecture.Runtime.Extensions                         | every page                | DOCTRINE (`docs/stacks/csharp`) | admitted         |
|  [37]   | Velopack                                                | provisioning-and-update   | api-velopack.md                 | admitted         |
|  [38]   | Grpc.Net.Client                                         | outbound-resilience       | api-grpc-client.md              | admitted         |
|  [39]   | NodaTime.Serialization.SystemTextJson                   | runtime-ports             | api-nodatime-stj.md             | admitted         |
|  [40]   | Microsoft.Extensions.Options.ConfigurationExtensions    | configuration-and-options | api-config-providers.md         | admitted         |
|  [41]   | Microsoft.Extensions.TimeProvider.Testing               | time-and-deadlines        | api-testing-seams.md            | tests-only       |
|  [42]   | NodaTime.Testing                                        | time-and-deadlines        | api-testing-seams.md            | tests-only       |
|  [43]   | Microsoft.Extensions.Diagnostics.Testing                | diagnostics-and-telemetry | api-testing-seams.md            | tests-only       |
|  [44]   | Grpc.AspNetCore                                         | companion-sidecar         | —                               | app-root-pending |
|  [45]   | Grpc.AspNetCore.Web                                     | runtime-ports             | —                               | app-root-pending |
|  [46]   | Grpc.AspNetCore.HealthChecks                            | companion-sidecar         | —                               | app-root-pending |
|  [47]   | OpenTelemetry.Exporter.OpenTelemetryProtocol            | diagnostics-and-telemetry | —                               | app-root-pending |
|  [48]   | System.CommandLine                                      | configuration-and-options | —                               | app-root-pending |
|  [49]   | Microsoft.AspNetCore.JsonPatch.SystemTextJson           | runtime-ports             | —                               | app-root-pending |
|  [50]   | Serilog.Extensions.Hosting                              | diagnostics-and-telemetry | —                               | app-root-pending |
|  [51]   | WASM component-model host runtime                       | sandbox-host              | —                               | app-root-pending |
|  [52]   | OPC-UA / Modbus / MQTT / serial protocol clients        | live-wire                 | —                               | app-root-pending |
|  [53]   | artifact-signature + SLSA in-toto attestation surface   | sandbox-host              | —                               | app-root-pending |
|  [54]   | MCP JSON-RPC transport binding                          | mcp-projection            | —                               | app-root-pending |

## [3]-[PROOF_GATES]

Proof runs at the planned phase gate, not after each edit. `[RAIL]` names the owning rail; the executable command lives with that rail owner, never restated here.

| [INDEX] | [GATE]                     | [RAIL]                           | [EVIDENCE]                                                            |
| :-----: | :------------------------- | :------------------------------- | :-------------------------------------------------------------------- |
|  [G1]   | locked restore             | Assay restore rail               | clean closure; unchanged `packages.lock.json`                         |
|  [G2]   | API catalogue resolve      | `assay api` doctor/resolve       | fence members resolve in `.api` or doctrine pages                     |
|  [G3]   | static plan + build        | Assay static rail                | routed closure, zero `': error '` lines                               |
|  [G4]   | spec law-matrix            | Assay test rail (AppHost target) | `testing-cs` law-matrix specs pass                                    |
|  [G5]   | host-seam bridge scenarios | Assay bridge rail                | host-seam scenarios pass under live RhinoWIP                          |
|  [G6]   | page diagram render        | local mermaid-cli                | page diagrams render through the local renderer                       |
|  [G7]   | spec-compile fallback      | Assay test rail                  | `Grpc.Core.Api` members compile until assay source-map coverage lands |
