# [RASM_APPHOST_ROADMAP]

This roadmap sequences the build. The runtime platform is built fully from the foundation; external-hop and companion lanes integrate with the concern that owns them.

## [1][PHASE_0]

- Add every core package to root `Directory.Packages.props` at the newest viable versions; project references stay versionless; no unusual pinning. In-process: `LanguageExt.Core`, `OpenTelemetry.Api`, `Microsoft.Extensions.Logging.Abstractions`, `NodaTime`, `FluentValidation`, `Microsoft.Extensions.Http.Resilience`. In-box no-pin: `System.Threading.Channels`, `System.Threading.Tasks.Dataflow`, `System.Diagnostics.DiagnosticSource`, `TimeProvider`. Companion: `Microsoft.Extensions.Hosting`, `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Configuration` (+ `.Binder` + `.Json`), `Microsoft.Extensions.Options` (+ `.ConfigurationExtensions` + `.DataAnnotations`), `Microsoft.Extensions.Diagnostics`, `OpenTelemetry.Instrumentation.Runtime`, `OpenTelemetry.Instrumentation.Process`, `OpenTelemetry.Exporter.OpenTelemetryProtocol`, `Serilog` (+ `.Sinks.OpenTelemetry` + `.Enrichers.Span` + `.Enrichers.Thread` + `.Enrichers.Environment`), `Scrutor`, `FluentValidation.DependencyInjectionExtensions`, `Microsoft.Extensions.ObjectPool` (conditional).
- Create `Rasm.AppHost.csproj`, wire into `Workspace.slnx` and the central build, resolve host assemblies as sibling libs do. Target `net10.0` (not `net10.0-windows`).
- Scaffold `Runtime/`, `Flow/`, root `AppHost.cs`, `Telemetry.cs` skeleton and canonical section order.
- Define `RasmRuntime` sealed record with all fields per §4.1 of `_ARCHITECTURE.md` — including `UiScheduler: RasmUiScheduler` (AppUi-owned sealed record) and `StoreOps: StoreDispatch` (Persistence-exported capability record); define `BootReceipt`, `DrainHandle`, `LifecycleReceipt` DU, `HealthSnapshot`, `RasmConfig`, `ObservabilitySlot` per §4.2–§4.6.

Phase 0 is complete when restore and build pass clean.

## [2][RUNTIME_CORE]

Build the runtime rail with its core mechanisms integrated:

| [INDEX] | [SURFACE]                       | [MECHANISM]                                                                  |
| :-----: | ------------------------------- | ---------------------------------------------------------------------------- |
|   [1]   | Runtime profile and lifecycle   | `RasmRuntime` sealed record; `Eff.runtime<RT>()`; no `Has<RT,_>`             |
|   [2]   | Boot entry                      | `AppHost.Boot(token, timeProvider, uiScheduler, …capabilities)` → `BootReceipt` |
|   [3]   | Root cancellation               | `CancellationTokenSource` linked to `RhinoApp.Closing`; `OnShutdown` = teardown only |
|   [4]   | Bounded in-process flow         | `Channel<ComputeRequest>` with `BoundedChannelFullMode.Wait`; capacity = named RT field |
|   [5]   | Retry/repeat cadence            | LanguageExt `Schedule`                                                       |
|   [6]   | Time                            | `TimeProvider` for timers/deadlines; `NodaTime.IClock` for persisted instants |
|   [7]   | Drain                           | `DrainHandle`; `TimeProvider`-based deadline 3–5 s; `InvokeOnUiThread` fence before UI `OnCompleted` |
|   [8]   | Telemetry                       | `ObservabilitySlot` in `Telemetry.cs`; `OpenTelemetry.Api` in-process only; one fused surface |
|   [9]   | In-process logging              | `ILogger` (`[LoggerMessage]`); `NullLogger` default; Serilog at companion root |
|  [10]   | Typed config                    | `RasmConfig` sealed record; bound at `Boot` (in-process: value object; companion: `IOptions<RasmConfig>`) |
|  [11]   | Health surface                  | `HealthSnapshot` — synchronous, projection-only; no HTTP, no middleware       |
|  [12]   | Degradation policy              | `LifecycleReceipt.Faulted`; named policy DU (`KeepRunning`/`DrainAndReload`/`UnloadPlugin`) driven by `RasmConfig` |
|  [13]   | Support-bundle trigger          | `SupportBundleRequested` event → Persistence export signal; window/size-cap from `RasmConfig` |

Runtime operations return typed `LifecycleReceipt` DU cases; correlate `DiagnosticReceipt` (AppUi-owned) without redefining it.

## [3][SCOPED_LANES]

| [INDEX] | [LANE]                            | [INTEGRATES_WITH]                    |
| :-----: | --------------------------------- | ------------------------------------ |
|   [1]   | Generic Host / DI / Scrutor       | Companion/test/bridge process        |
|   [2]   | Config / Options pipeline         | Companion root; in-process = value object |
|   [3]   | Serilog / OpenTelemetry SDK exporters | Support bundle or telemetry sink; companion root only |
|   [4]   | OTel runtime/process instrumentation | Companion root only              |
|   [5]   | HTTP resilience                   | Outbound typed `HttpClient` hop      |
|   [6]   | FluentValidation DI extensions    | Companion root validator scanning    |
|   [7]   | Dataflow                          | Multi-stage topology beyond Channels |
|   [8]   | ObjectPool                        | Conditional; when allocation profiling warrants |

## [4][RUNTIME_EVIDENCE]

Runtime claims are scoped to proven profiles. Owner-local receipts identify:

- Startup/drain/unload receipt.
- Cancellation (via `RhinoApp.Closing`) and fault propagation.
- Scope disposal.
- Channel backpressure or Dataflow topology proof.
- Telemetry correlation and support-bundle receipt.
- Outbound HTTP resilience ownership on external hops.
- `InvokeOnUiThread` fence before UI observable `OnCompleted`.
- `TimeProvider`-based drain deadline (3–5 s); forceful cancel proof.
- `HealthSnapshot` query at boot and after Persistence/Compute fault.
- Degradation policy applied on sibling fault.
- GH2 agnosticism verified: no GH2 API in AppHost source.
