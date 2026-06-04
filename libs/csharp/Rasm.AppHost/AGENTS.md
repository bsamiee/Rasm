# [H1][RASM_APPHOST_AGENTS]

[CRITICAL] Build `Rasm.AppHost` now as one unified runtime platform. Add package references centrally, create the `.csproj`, and scaffold the folder structure in Phase 0 before heavy work. Build the runtime core fully; integrate external lanes with the concern that owns them.

## [1][OWNER_CONTRACT]

- Use runtime-record `Eff.runtime<RT>()` as the default Rhino/GH2 plugin composition mode. `RasmRuntime` is the sole `RT` type. No `Has<RT,_>` traits or `Readable.asks` — v4 vocabulary, forbidden by `coding-csharp/references/composition.md`.
- The composition root is `PlugIn.OnLoad`. It constructs the AppUi-owned UI scheduler boundary **on the UI thread**, then calls `AppHost.Boot(token, timeProvider, uiScheduler, …capabilities)`. `Boot` returns `BootReceipt(Runtime, Drain, Lifecycle)`; the `RasmRuntime` is handed to AppUi to activate observables.
- Use Generic Host, `IServiceCollection`, and Scrutor for companion/test/bridge bootstraps **only**. Never start a Generic Host inside the in-process plugin path.
- Emit typed lifecycle/status/fault receipts (`LifecycleReceipt` DU). Do not expose service-provider resolution as public API.
- Add in-process packages centrally in Phase 0; scope companion packages to companion bootstrap roots.
- Use `Microsoft.Extensions.Http.Resilience` for outbound hops, never raw Polly; one retry owner per hop.
- Own the one runtime record and bounded `Channel<ComputeRequest>` the siblings share. Channel capacity is a named `RasmRuntime` field; `BoundedChannelFullMode.Wait` (lossless). AppUi delegates scheduling via its `RasmUiScheduler` sealed record (constructed on the UI thread before `Boot`); Compute drains AppHost's channel. AppHost submits durable work to Persistence via `StoreDispatch` (the Persistence-exported capability record that submits `StoreLifecycleOp`/`StoreQuery<T>` as `Eff<RT, StoreReceipt>`). Correlate typed per-capability receipts (`LifecycleReceipt`, `ExecutionReceipt`, `StoreReceipt`, `DiagnosticReceipt`); never a generic `IReceipt` ledger. AppHost references `DiagnosticReceipt` (AppUi-owned); it does not redefine it.
- In-process instrumentation: `OpenTelemetry.Api` only (`ActivitySource`/`Meter`). OpenTelemetry SDK, exporters, and `Instrumentation.Runtime`/`.Process` activate at companion bootstrap roots only.
- In-process logging: `ILogger<T>` via `[LoggerMessage]` source generation, backed by `NullLogger` default. Serilog and its enrichers activate at companion bootstrap roots only.
- Typed config: `RasmConfig` sealed record bound at `Boot`. In-process = pre-bound value object; companion = `IOptions<RasmConfig>` pipeline (`Microsoft.Extensions.Configuration` + `.Options` + `.DataAnnotations`). Neither `IConfiguration` nor `IOptions<T>` crosses into `RasmRuntime`.
- Expose `HealthSnapshot` as a synchronous, projection-only readiness surface. No `IHealthCheck` middleware, no HTTP.
- Emit `LifecycleReceipt.Faulted` on sibling fault; apply degradation policy from `RasmConfig` data (`KeepRunning`/`DrainAndReload`/`UnloadPlugin`). No inline branching in domain logic.
- Own the support-bundle trigger: `SupportBundleRequested` event → Persistence export signal; window and size-cap from named `RasmConfig` fields.

## [2][BOUNDARY_RULES]

| [INDEX] | [CONCERN]           | [RULE]                                                                    |
| :-----: | ------------------- | ------------------------------------------------------------------------- |
|   [1]   | UI                  | Emit status; AppUi renders                                                |
|   [2]   | UI scheduler        | AppUi constructs on the UI thread before `Boot`; AppHost receives it      |
|   [3]   | Persistence         | Schedule/correlate; Persistence stores                                    |
|   [4]   | Compute             | Schedule/drain; Compute executes                                          |
|   [5]   | Retry               | LanguageExt `Schedule` or HTTP resilience, never both on one hop          |
|   [6]   | Flow                | Channels first; Dataflow on proven topology                               |
|   [7]   | Time                | `TimeProvider` for timers/deadlines; `NodaTime.IClock` for semantic time  |
|   [8]   | Config              | `RasmConfig` value at `Boot`; companion wires `IOptions<RasmConfig>` externally |
|   [9]   | Telemetry           | `OpenTelemetry.Api` in-process; SDK/exporters at companion root only      |
|  [10]   | Logging             | `ILogger`/`NullLogger` in-process; Serilog + enrichers at companion root  |
|  [11]   | RhinoCommon threads | Any Channel consumer touching RhinoCommon marshals via `RhinoApp.InvokeOnUiThread` |
|  [12]   | Shutdown trigger    | Subscribe `RhinoApp.Closing`; `OnShutdown` = final teardown only; never `RhinoApp.IsClosing` as primary trigger |
|  [13]   | Drain deadline      | `CancellationTokenSource.CancelAfter(TimeSpan, TimeProvider)`, 3–5 s; forceful cancel |
|  [14]   | GH2 SDK             | AppHost is GH2-agnostic; no GH2 API crosses into AppHost source; `async:true` unsupported in GH components |

## [3][SHUTDOWN_DRAIN_ORDER]

[CRITICAL] Shutdown sequence — execute in strict order:

1. `RhinoApp.Closing` subscription cancels root `CancellationTokenSource`.
2. Complete `ChannelWriter<ComputeRequest>`; await channel drain with `TimeProvider`-based deadline (3–5 s, forceful cancel after).
3. Dispose Persistence — flushes final store/benchmark writes.
4. Fence: `RhinoApp.InvokeOnUiThread` before any UI observable `OnCompleted` call.
5. Call `OnCompleted` on UI observables (never `OnError`).
6. Dispose `RasmRuntime` record.

Compute-before-Persistence: prevents dropping the last batch. Persistence-before-UI: prevents racing `OnCompleted` against a pending change-set. The `InvokeOnUiThread` fence ensures `OnCompleted` fires on the UI thread before Rhino destroys the window.

## [4][EVIDENCE]

Executable proof comes from source and host scenarios. Evidence categories:

- Startup/drain/unload receipt.
- Cancellation triggered by `RhinoApp.Closing` subscription.
- Fault propagation and degradation policy applied.
- Scope disposal.
- Channel backpressure (full channel → `Wait`) and Dataflow topology proof.
- Telemetry correlation and support-bundle receipt.
- Outbound HTTP resilience ownership on external hops.
- `InvokeOnUiThread` fence before UI observable `OnCompleted`.
- `TimeProvider`-based drain deadline; forceful cancel proof.
- `HealthSnapshot` query at boot and after sibling fault.
- GH2-agnosticism: no GH2 import in AppHost source.

## [5][REJECTIONS]

- No domain logic.
- No AppUi rendering.
- No Persistence `DbContext` ownership.
- No Compute substrate/model execution.
- No Scrutor inside in-process plugin paths.
- No `IServiceProvider` exposed as a public application API.
- No Dataflow on a hop a bounded Channel already owns.
- No `Has<RT,_>` traits or `Readable.asks` (v4 vocabulary).
- No Generic Host in-process.
- No OpenTelemetry SDK in-process (process-ownership conflict with Rhino).
- No `MediatR`, `Mediator`, `MassTransit`, `NServiceBus`.
- No `Microsoft.FeatureManagement`; feature flags belong in `RasmConfig` data.
- No raw `Polly` direct reference; use `Microsoft.Extensions.Http.Resilience`.
- No `Serilog.Sinks.Async`; the OTLP pipeline is the sink.
- No `Microsoft.Extensions.Caching.Memory`; caching is a Persistence concern.
- No `Microsoft.AspNetCore.*`.
- No version numbers in documentation text; version truth lives in `Directory.Packages.props`.
