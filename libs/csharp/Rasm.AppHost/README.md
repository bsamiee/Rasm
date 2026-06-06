# [RASM_APPHOST]

`Rasm.AppHost` is the runtime/composition platform for Rasm plugins, apps, companion processes, and bridge-like services. It owns runtime profiles and lifecycle, built fully from the foundation.

## [1][PURPOSE]

`Rasm.AppHost` owns runtime profiles, startup/drain/unload, scheduling, bounded in-process flow, telemetry correlation, external-hop policy, typed config surface, in-process health/readiness, and lifecycle receipts. It coordinates AppUi, Persistence, Compute, Rhino, and GH2 without importing their implementation concerns.

It is not a domain service layer, job framework, DI wrapper, telemetry wrapper, or catch-all runtime package.

> [!CAUTION]
> Generic Host is companion/test/bridge only — never in-process. The composition root is `PlugIn.OnLoad`; it constructs `RasmUiScheduler` (AppUi-owned sealed record) on the UI thread, then calls `AppHost.Boot(token, timeProvider, uiScheduler, …capabilities)`. `Boot` receives the `RasmUiScheduler` and capability handles (including `StoreDispatch` for Persistence), constructs `RasmRuntime`, and returns a `BootReceipt` containing the runtime record and `DrainHandle`. The runtime record is handed back to AppUi to activate observables.

## [2][STATUS]

| [INDEX] | [SURFACE]                      | [STATE]                    |
| :-----: | ------------------------------ | -------------------------- |
|   [1]   | Project file                   | Create in Phase 0          |
|   [2]   | Production API                 | In progress                |
|   [3]   | Package references             | Add centrally in Phase 0   |
|   [4]   | In-process runtime record mode | Default mode               |
|   [5]   | Generic Host mode              | Companion/test/bridge lane |

Add packages centrally at the newest viable versions during Phase 0. Remove version numbers from any text that refers to package versions; version truth lives in `Directory.Packages.props` only.

## [3][MANUAL]

| [INDEX] | [FILE]             | [READ_FOR]                                                                     |
| :-----: | ------------------ | ------------------------------------------------------------------------------ |
|   [1]   | `_ARCHITECTURE.md` | Composition modes, type shapes, packages, flow policy, shutdown/drain sequence |
|   [2]   | `AGENTS.md`        | Build rules, package boundaries, rejections                                    |
|   [3]   | `ROADMAP.md`       | Build sequence and runtime evidence                                            |

## [4][CONSTRAINTS]

- One runtime rail owns profiles, lifecycle, flow, telemetry, typed config, in-process health, and external-hop policy.
- Generic Host bootstrap is the companion/test/bridge lane — `[NEVER]` in-process.
- Telemetry exporters, Serilog sinks, and OpenTelemetry SDK live only in companion bootstrap roots.
- `RasmRuntime` is the sole `RT` type argument; resolved via `Eff.runtime<RT>()` everywhere; no `Has<RT,_>` or `Readable.asks`.
- `UiScheduler` field type is `RasmUiScheduler` (AppUi-owned sealed record); `StoreOps` field type is `StoreDispatch` (Persistence-exported capability record).
- Channel capacity is a named `RasmRuntime` field, not a constant; `BoundedChannelFullMode.Wait` (lossless).
- No AppUi, Persistence, or Compute implementation absorbed into AppHost.
- Any Channel consumer touching `RhinoCommon` marshals via `RhinoApp.InvokeOnUiThread`.
- Subscribe `RhinoApp.Closing` to trigger drain; `OnShutdown` is final teardown only; `RhinoApp.IsClosing` is not the primary drain trigger.
