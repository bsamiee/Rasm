# [DOMAIN_STACK]

The domain stack extends the finalized C# root doctrine into runtime, concurrency, diagnostics, boundary admission, resilience, persistence, compute, and interaction. Each page adds one layer to the same body: later pages consume earlier law silently and spend their lines only on their own capability.

## [1]-[BUILD_ORDER]

| [INDEX] | [PAGE]           | [OWNS]                                             | [PRIMARY_PACKAGES]                                      |
| :-----: | :--------------- | :------------------------------------------------- | :------------------------------------------------------ |
|   [1]   | `runtime.md`     | hosting, DI composition, options, caching, time    | Scrutor, Hosting/Options, HybridCache, NodaTime         |
|   [2]   | `concurrency.md` | threading, channels, reactive streams, parallelism | Channels, System.Reactive, DynamicData                  |
|   [3]   | `diagnostics.md` | logging, traces, metrics, redaction, correlation   | Serilog, OpenTelemetry, Extensions.Telemetry, Redaction |
|   [4]   | `validation.md`  | boundary-shape validation ownership                | FluentValidation                                        |
|   [5]   | `resilience.md`  | transport and boundary resilience pipelines        | Polly, Http.Resilience                                  |
|   [6]   | `persistence.md` | relational doctrine, provider-polymorphic storage  | EF Core, Npgsql, SQLite stack, BulkExtensions           |
|   [7]   | `compute.md`     | tensors, measured dispatch, remote compute lanes   | Numerics.Tensors, Grpc.Net.Client, Protobuf             |
|   [8]   | `interaction.md` | retained shell, screens, commands, live view state | Avalonia, ReactiveUI, SkiaSharp, LiveCharts             |

## [2]-[CUMULATIVE_LAW]

[PROSE]:
- A domain page consumes earlier layers as settled material: admitted owners, typed rails, policy values, boundary capsules, and runtime spines arrive already decided.
- Pages carry zero links, outside links, release narration, process state, or repository anchors.
- Each page states law as fact and uses package names only where they decide implementation shape.

[SNIPPETS]:
- A snippet first captures its own card, then composes finalized layers silently.
- The spotlight region belongs to the current page; established regions are supporting material and never re-taught.
- Every code identifier is language-valid and neutral unless the code must name an actual package surface.

## [3]-[PAGE_SCOPE]

[RUNTIME]:
- Owns hosting lifecycle, DI composition with assembly scanning and decoration, keyed services, options, HybridCache, process cancellation, `TimeProvider`, and NodaTime calendar vocabulary.
- Establishes one process root, one cancellation spine, one clock, one calendar vocabulary, and options as policy values.
- Consumes boundary state and effect rails without re-teaching them.

[CONCURRENCY]:
- Owns threading law, structured concurrency, `System.Threading.Channels`, System.Reactive, DynamicData, atomic or STM state at concurrency scope, and parallelism policy.
- Channels own producer-consumer seams.
- Reactive streams enter only where operator composition changes the design.

[DIAGNOSTICS]:
- Owns structured logging, traces, metrics, sampling, enrichment, redaction, and one correlation spine.
- Signals cross logs, traces, and metrics through one correlation identity.
- Sensitive shapes redact at definition time before any exporter can observe them.

[BOUNDARY_ADMISSION]:
- FluentValidation owns wire DTOs, options, and input validation.
- Generated validation partials admit value objects.
- `Validation<E,T>` rails own domain accumulation.
- Boundary validators own wire shapes.

[RESILIENCE]:
- Polly owns non-HTTP transport and boundary resilience pipelines.
- Microsoft.Extensions.Http.Resilience owns HTTP seams.
- Domain-internal retry and repeat stays `Schedule` policy on effect rails.
- One outbound hop has one retry owner at the composition root.

[PERSISTENCE]:
- EF Core doctrine is provider-polymorphic across embedded and server engines as one case axis inside one owner.
- Compiled models, JSON columns, complex types, interceptors, naming conventions, migrations, bulk movement, integrity, snapshots, and retention stay in one persistence law.
- A store whose schema is newer than the compiled model is a typed rejection, not a best-effort open.

[COMPUTE]:
- Owns tensor primitives at application scope, measured dispatch with typed receipts, remote compute lanes over typed contracts, and model-inference lanes scoped to admitted package behavior.
- Remote lanes carry schema-derived contracts at compile time.
- Payloads outside generator coverage project to attributed records at the boundary.

[INTERACTION]:
- Owns retained shell, screen, navigation, command, reactive view-state, live collection projection, validation display, visual asset, control, dialog, inspector, and UI thread-affinity law.
- Provider vocabulary remains package policy at the interaction boundary.
- Earlier domain pages own runtime, concurrency, diagnostics, validation, resilience, persistence, and compute mechanics; interaction composes those mechanics into user-facing state and commands without re-teaching them.

## [4]-[PACKAGE_RESIDENCY]

Every package has one doctrine home. Cross-page composition is implicit.

| [INDEX] | [PACKAGE_FAMILY]                                           | [HOME]      |
| :-----: | :--------------------------------------------------------- | :---------- |
|   [1]   | Scrutor                                                    | runtime     |
|   [2]   | Microsoft.Extensions.Caching.Hybrid                        | runtime     |
|   [3]   | NodaTime and `TimeProvider` law                            | runtime     |
|   [4]   | System.Threading.Channels                                  | concurrency |
|   [5]   | System.Reactive and DynamicData                            | concurrency |
|   [6]   | Serilog and logging abstractions                           | diagnostics |
|   [7]   | OpenTelemetry and Microsoft.Extensions.Telemetry           | diagnostics |
|   [8]   | Compliance.Redaction                                       | diagnostics |
|   [9]   | FluentValidation                                           | validation  |
|  [10]   | Polly and Microsoft.Extensions.Http.Resilience             | resilience  |
|  [11]   | EF Core, Npgsql, SQLite, NamingConventions, BulkExtensions | persistence |
|  [12]   | System.Numerics.Tensors                                    | compute     |
|  [13]   | Grpc.Net.Client and Google.Protobuf                        | compute     |
|  [14]   | ONNX Runtime .NET                                          | compute     |
|  [15]   | Avalonia, Avalonia controls, and Avalonia themes           | interaction |
|  [16]   | ReactiveUI, ReactiveUI.Avalonia, ReactiveUI.Validation     | interaction |
|  [17]   | SkiaSharp, HarfBuzzSharp, Svg.Skia, and LiveCharts         | interaction |
|  [18]   | DialogHost.Avalonia, PropertyGrid, and Xaml.Behaviors      | interaction |

## [5]-[BOUNDARIES]

- Scrutor belongs to runtime law; host and wire boundaries stay in the root boundary page.
- System.Reactive and DynamicData stream mechanics belong to concurrency; ReactiveUI retained view-state and command composition belongs to interaction.
- FluentValidation boundary-shape validation belongs to validation; ReactiveUI.Validation belongs to interaction as user-facing validation projection.
- Package release labels live in the central manifest, not this charter.
- API discovery, package admission, and implementation confirmation happen through owning tools and manifests, not through this page.
