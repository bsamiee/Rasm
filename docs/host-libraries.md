# [HOST_LIBRARIES]

Scope: composition roots, external I/O boundaries, app runtime, persistence, compute, and product UI surfaces. Domain modules stay on LanguageExt rails, Thinktecture shapes, MathNet/CSparse numerics, and host SDK boundaries.

This file owns adoption policy, not package manuals. Owner-local platform stubs carry candidate rationale:

| [INDEX] | [OWNER]            | [MANUAL]                                           |
| :-----: | ------------------ | -------------------------------------------------- |
|   [1]   | `Rasm.AppUi`       | `../libs/csharp/Rasm.AppUi/ARCHITECTURE.md`       |
|   [2]   | `Rasm.AppHost`     | `../libs/csharp/Rasm.AppHost/ARCHITECTURE.md`     |
|   [3]   | `Rasm.Persistence` | `../libs/csharp/Rasm.Persistence/ARCHITECTURE.md` |
|   [4]   | `Rasm.Compute`     | `../libs/csharp/Rasm.Compute/ARCHITECTURE.md`     |

## [1][COMPOSITION_MODES]

| [INDEX] | [MODE]                                       |  [DEFAULT]   | [COMPOSITION]                                                 |
| :-----: | -------------------------------------------- | :----------: | ------------------------------------------------------------- |
|   [1]   | Rhino/GH2 in-process plugin                  |     Yes      | `Eff.runtime<RT>()`, explicit factories, host-owned UI rails  |
|   [2]   | Companion process, test host, bridge service |      No      | `IServiceCollection`, optional Generic Host, optional Scrutor |
|   [3]   | Domain, analysis, geometry kernel            | No container | LanguageExt effects and direct constructors                   |

Scrutor, Generic Host, logging providers, telemetry exporters, HTTP resilience, EF Core, and validation packages wait for a concrete bootstrap consumer. The AppUi package matrix is active direct through `Rasm.AppUi.csproj`. `Directory.Packages.props` carries graph evidence; owner-local manuals carry intent and adoption guidance.

## [2][FIRST_CONSUMER_CANDIDATES]

| [INDEX] | [CANDIDATE]                    | [TRIGGER]                                                    | [OWNER]            |
| :-----: | ------------------------------ | ------------------------------------------------------------ | ------------------ |
|   [1]   | Scrutor                        | Real `IServiceCollection` bootstrap with scan/decorate value | `Rasm.AppHost`     |
|   [2]   | Generic Host / DI abstractions | Companion process, bridge service, or test host              | `Rasm.AppHost`     |
|   [3]   | NodaTime                       | Persisted/audited semantic time or explicit zone policy      | `Rasm.AppHost`     |
|   [4]   | FluentValidation               | External DTO/config boundary before `Validation<Error,T>`    | `Rasm.AppHost`     |
|   [5]   | Serilog / OpenTelemetry        | Centralized support evidence, logs, traces, metrics          | `Rasm.AppHost`     |
|   [6]   | HTTP resilience                | Typed outbound `HttpClient` to external service              | `Rasm.AppHost`     |
|   [7]   | EF Core SQLite / SQLite        | Local durable app state outside GH solve paths               | `Rasm.Persistence` |
|   [8]   | MessagePack                    | Compact snapshots after binary round-trip proof              | `Rasm.Persistence` |
|   [9]   | ImGui.NET debug overlay        | Debug overlay proof plan after product UI rail exists        | `Rasm.AppUi`       |
|  [10]   | AppUi active matrix            | Active direct project references; runtime proof pending      | `Rasm.AppUi`       |
|  [11]   | System.Numerics.Tensors        | Measured span/TensorPrimitives kernel consumer               | `Rasm.Compute`     |
|  [12]   | ML.NET / gRPC client           | Named model or out-of-process compute companion              | `Rasm.Compute`     |

Resolve package versions from the current package graph and maintained package source when the first concrete consumer lands. Do not record exact candidate versions in owner docs.

## [3][BOUNDARIES]

- LanguageExt `Schedule` owns domain and hosted `Eff`/`IO` retry/repeat cadence.
- HTTP resilience owns typed outbound `HttpClient` policies only.
- Bounded `System.Threading.Channels` is the default in-process flow primitive; Dataflow is optional after a real multi-stage graph proves Channels or a single queue insufficient.
- `TimeProvider` owns scheduler/timer mechanics; NodaTime owns persisted/audited semantic time.
- FluentValidation validates external DTO/config inputs only, then folds to `Validation<Error,T>`.
- Observability packages register at a composition root. Domain results stay typed first and project to logs, traces, and metrics at boundaries.
- Persistence owns database open/migrate/query/dispose semantics. AppHost may schedule or orchestrate durable work; it does not own storage internals.

## [4][REJECTIONS]

- Do not add unused central `PackageVersion` entries as future intent.
- Do not call a package active until a project references it and owner-local evidence records the behavior.
- Do not import host packages into `Rasm`, `Rasm.Rhino`, or `Rasm.Grasshopper` domain/hot-path code.
- Do not stack LanguageExt retry, DB retry, and HTTP resilience on one operation.
