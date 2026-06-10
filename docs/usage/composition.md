# [HOST_LIBRARIES]

Scope: composition roots, external I/O boundaries, app runtime, persistence, compute, and product UI surfaces. Domain modules stay on LanguageExt rails, Thinktecture shapes, MathNet/CSparse numerics, and host SDK boundaries.

## [1]-[COMPOSITION_MODES]

| [INDEX] | [MODE]                                       |  [DEFAULT]   | [COMPOSITION]                                                 |
| :-----: | -------------------------------------------- | :----------: | ------------------------------------------------------------- |
|   [1]   | Rhino/GH2 in-process plugin                  |     Yes      | `Eff.runtime<RT>()`, explicit factories, host-owned UI rails  |
|   [2]   | Companion process, test host, bridge service |      No      | Generic Host, `IServiceCollection`, Scrutor, exporters        |
|   [3]   | Domain, analysis, geometry kernel            | No container | LanguageExt effects and direct constructors                   |

Composition packages enter through the owner rail that performs the runtime work: bootstrap, scheduling, persistence, observation, compute, or support evidence. AppUi, AppHost, Compute, and Persistence are full platform packages, and each package family is selected as part of the rail that consumes it. Source exposes provider-neutral product vocabulary while package APIs remain behind the rail that owns the behavior.

Folder architecture starts from the complete suite shape. Host plugins, companion processes, sidecars, bridge services, downstream app shells, storage profiles, asset providers, compute substrates, telemetry exporters, support bundles, and proof surfaces are parameterized cases inside owner rails. New capability deepens an existing state machine, algebra, catalog, receipt family, or adapter record before it creates a new public type family.

Package posture is exact:
- Graph packages have versionless project references or centrally pinned transitive-closure reasons.
- Build-contract packages are selected package IDs for complete rail implementation.
- Rejected packages are not fallback options for the same capability.
- In-box surfaces use the BCL or SDK and do not receive package pins.

## [2]-[PACKAGE_POSTURE]

Graph packages are executable restore inputs. Build-contract packages are implementation inputs for the full rail and remain provider-neutral at public boundaries.

[RASM_APPHOST]:
- Graph: `Microsoft.Extensions.Logging.Abstractions`, `NodaTime`.
- Build contract: `Microsoft.Extensions.Hosting`, `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Configuration`, `Microsoft.Extensions.Options`, `Scrutor`, `FluentValidation`, `Microsoft.Extensions.Diagnostics.HealthChecks`, `Serilog`, `OpenTelemetry`, `OpenTelemetry.Extensions.Hosting`, `Microsoft.Extensions.Http.Resilience`.
- Public rail: runtime lifecycle, drain, health, support, telemetry correlation, companion bootstrap, validation, and outbound HTTP policy.

[RASM_APPUI]:
- Graph: `Avalonia`, `Avalonia.Desktop`, `Avalonia.Themes.Fluent`, `Avalonia.Fonts.Inter`, `Avalonia.Controls.DataGrid`, `Avalonia.Controls.ColorPicker`, `ReactiveUI`, `ReactiveUI.Avalonia`, `ReactiveUI.Validation`, `System.Reactive`, `DynamicData`, `SkiaSharp`, `SkiaSharp.NativeAssets.macOS`, `SkiaSharp.HarfBuzz`, `HarfBuzzSharp.NativeAssets.macOS`, `LiveChartsCore.SkiaSharpView.Avalonia`, `Svg.Controls.Skia.Avalonia`, `bodong.Avalonia.PropertyGrid`, `Xaml.Behaviors.Avalonia`, `DialogHost.Avalonia`.
- Build contract: `ImGui.NET` belongs to the diagnostic/debug overlay rail; product UI remains the retained AppUi shell/screen/command/live/theme/asset rail.
- Public rail: one UI/UX system for embedded Rhino panels, GH2 canvas/popup/component surfaces, companion windows, sidecar shells, downstream product apps, live dashboards, inspectors, dialogs, assets, typography, accessibility, and diagnostics.

[RASM_PERSISTENCE]:
- Graph: `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.Data.Sqlite`, `SQLitePCLRaw.bundle_e_sqlite3`, `EFCore.NamingConventions`, `NodaTime`, `NodaTime.Serialization.SystemTextJson`, `Microsoft.Extensions.Compliance.Redaction`, `System.IO.Hashing`.
- Build contract: `Microsoft.EntityFrameworkCore.Design`, `MessagePack`, `MessagePackAnalyzer`, `K4os.Compression.LZ4`.
- Public rail: store profiles, schema and migration, JSON and MessagePack snapshots, compression, redaction, support bundles, cache metadata, benchmark indexes, and companion database profiles.

[RASM_COMPUTE]:
- Graph: workspace functional core and referenced kernel packages.
- Build contract: `System.Numerics.Tensors`, `CommunityToolkit.HighPerformance`, `Microsoft.ML.OnnxRuntime`, `Grpc.Net.Client`, `Google.Protobuf`, `Grpc.Tools`, `UnitsNet`, `Microsoft.IO.RecyclableMemoryStream`.
- Public rail: execution intent, substrate selection, tensor staging, ONNX/CoreML model execution, gRPC companion execution, units boundaries, stream pooling, measurement, progress, cache keys, and receipts.

## [3]-[OWNER_GATES]

| [INDEX] | [CAPABILITY]                  | [OWNER]            | [PUBLIC_SHAPE]                 |
| :-----: | ----------------------------- | ------------------ | ------------------------------ |
|   [1]   | Generic Host and Scrutor      | `Rasm.AppHost`     | runtime bootstrap rail         |
|   [2]   | Health, telemetry, validation | `Rasm.AppHost`     | typed runtime receipts         |
|   [3]   | HTTP resilience               | `Rasm.AppHost`     | outbound-hop policy            |
|   [4]   | Retained UI and assets        | `Rasm.AppUi`       | shell/screen/command rail      |
|   [5]   | UI debug overlays             | `Rasm.AppUi`       | diagnostics rail               |
|   [6]   | SQLite and snapshots          | `Rasm.Persistence` | store and snapshot algebras    |
|   [7]   | Redaction and support export  | `Rasm.Persistence` | classified export rail         |
|   [8]   | Tensor, model, remote compute | `Rasm.Compute`     | execution substrate rail       |
|   [9]   | Units and stream pooling      | `Rasm.Compute`     | measurement and receipt rail   |

Package versions are resolved by the executable package graph. Public contracts name capabilities, rails, receipts, and provider-neutral identities instead of package-specific API types.

## [4]-[BOUNDARIES]

- LanguageExt `Schedule` owns domain and hosted `Eff`/`IO` retry/repeat cadence.
- HTTP resilience owns typed outbound `HttpClient` policies only.
- Bounded `System.Threading.Channels` is the default in-process flow primitive. Dataflow belongs to a declared multi-stage topology with observable stage boundaries.
- `TimeProvider` owns scheduler/timer mechanics; NodaTime owns persisted/audited semantic time.
- FluentValidation validates external DTO/config inputs only, then folds to `Validation<Error,T>`.
- Observability packages register at a composition root. Domain results stay typed first and project to logs, traces, and metrics at boundaries.
- Persistence owns database open/migrate/query/dispose semantics. AppHost may schedule or orchestrate durable work; it does not own storage internals.
- File, JSON, SQLite, companion database, model cache, support bundle, and benchmark index storage all enter through Persistence-owned store or snapshot algebras. Provider-specific packages never create separate public persistence surfaces.
- Embedded panels, companion windows, explicit sidecars, and downstream app UI all enter through AppUi shell/screen/command/live/theme/asset rails. Provider-specific UI packages never create separate public app surfaces.
- Vector, tensor, model, remote, stream, and units execution all enter through Compute-owned intent, substrate, receipt, and measurement rails. Provider-specific compute packages never create separate public execution services.

## [5]-[REJECTIONS]

- Do not add unused central `PackageVersion` entries.
- Do not expose package-specific API types as public product vocabulary.
- Do not import host packages into `Rasm`, `Rasm.Rhino`, or `Rasm.Grasshopper` domain/hot-path code.
- Do not stack LanguageExt retry, DB retry, and HTTP resilience on one operation.
