# [RASM_API_OPENTELEMETRY]

`OpenTelemetry` is the SDK turning the runtime's vendor-neutral `System.Diagnostics` emission into exportable signal streams: provider builders admit sources and meters by name, resource builders stamp process identity, samplers and views shape volume, and processors batch every signal toward one exporter. Contract assembly `OpenTelemetry.Api` carries the cross-process context surface — `Baggage` and the text-map propagator family. Libraries never reference either assembly; composition roots alone construct providers, and each provider's lifetime is the root that builds it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry`
- package: `OpenTelemetry`
- assembly: `OpenTelemetry`
- contract assembly: `OpenTelemetry.Api` — `Baggage`, `PropagationContext`, the `TextMapPropagator` family, `Propagators`
- namespace: `OpenTelemetry`, `OpenTelemetry.Trace`, `OpenTelemetry.Metrics`, `OpenTelemetry.Logs`, `OpenTelemetry.Resources`, `OpenTelemetry.Context.Propagation`
- asset: runtime library
- rail: telemetry composition

## [02]-[PUBLIC_TYPES]

[PROVIDER_TYPES]: builders, providers, and volume shaping
- rail: telemetry composition

| [INDEX] | [SYMBOL]                                          | [PACKAGE_ROLE]     | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------ | :----------------- | :----------------------------------------------- |
|  [01]   | `Sdk`                                             | static entry       | hostless builder mint; default-propagator seat   |
|  [02]   | `TracerProviderBuilder`                           | trace builder      | source admission, sampler seat, processor chain  |
|  [03]   | `MeterProviderBuilder`                            | metric builder     | meter admission, views, exemplar filter          |
|  [04]   | `LoggerProviderBuilder`                           | log builder        | log-record processor chain                       |
|  [05]   | `ResourceBuilder`                                 | identity builder   | `CreateDefault`/`CreateEmpty` + detector rows    |
|  [06]   | `ParentBasedSampler` / `TraceIdRatioBasedSampler` | sampler root       | one deterministic verdict per trace id           |
|  [07]   | `AlwaysOnSampler` / `AlwaysOffSampler`            | sampler terminals  | delegate seats inside the parent-based composite |
|  [08]   | `ExemplarFilterType`                              | exemplar policy    | `AlwaysOff` / `AlwaysOn` / `TraceBased`          |
|  [09]   | `MetricStreamConfiguration`                       | view row           | declaration-time stream surgery                  |
|  [10]   | `Base2ExponentialBucketHistogramConfiguration`    | view row           | exponential histogram shape                      |
|  [11]   | `ExplicitBucketHistogramConfiguration`            | view row           | explicit-bucket fallback                         |
|  [12]   | `OpenTelemetryLoggerOptions`                      | log-bridge options | `ILogger` record-capture knobs                   |
|  [13]   | `BaseProcessor<T>` / `BatchExportProcessor<T>`    | processor chain    | `ForceFlush(int)` / `Shutdown(int)` drain verbs  |
|  [14]   | `SuppressInstrumentationScope`                    | recursion guard    | `Begin(bool)` around exporter-owned I/O          |

`Sdk` mints the hostless builders — `CreateTracerProviderBuilder()`, `CreateMeterProviderBuilder()`, `CreateLoggerProviderBuilder()` — each `Build()` returning the `IDisposable` provider handle whose `ForceFlush(timeoutMilliseconds)`-then-`Dispose()` pair is the drain contract. `Sdk.SuppressInstrumentation` reads the ambient suppression flag; `Sdk.SetDefaultTextMapPropagator(TextMapPropagator)` seats the process propagator.

`ResourceBuilderExtensions` supplies `AddService(serviceName, serviceNamespace, serviceVersion, autoGenerateServiceInstanceId, serviceInstanceId)`, `AddTelemetrySdk()`, `AddAttributes(IEnumerable<KeyValuePair<string, object>>)`, and `AddEnvironmentVariableDetector()`. `MetricStreamConfiguration` carries `Drop`, `Name`, `Description`, `TagKeys`, `ExcludedTagKeys`, and `CardinalityLimit`; the histogram rows carry `MaxSize`/`MaxScale` and `Boundaries`; `OpenTelemetryLoggerOptions` carries `IncludeScopes`, `IncludeFormattedMessage`, `ParseStateValues`, `IncludeAttributes`, and `IncludeTraceState`.

[CONTEXT_TYPES]: cross-process context (contract assembly)
- rail: telemetry composition

| [INDEX] | [SYMBOL]                     | [PACKAGE_ROLE]   | [CAPABILITY]                                    |
| :-----: | :--------------------------- | :--------------- | :---------------------------------------------- |
|  [01]   | `Baggage`                    | ambient value    | `Current`, `Create`, `GetBaggage`, `SetBaggage` |
|  [02]   | `TextMapPropagator`          | carrier contract | `Inject`/`Extract` over getter/setter adapters  |
|  [03]   | `TraceContextPropagator`     | W3C traceparent  | trace-context leg of the composite              |
|  [04]   | `BaggagePropagator`          | W3C baggage      | baggage leg of the composite                    |
|  [05]   | `CompositeTextMapPropagator` | propagator fold  | one composite over the two W3C legs             |
|  [06]   | `PropagationContext`         | carrier value    | `(ActivityContext, Baggage)` pair               |
|  [07]   | `Propagators`                | ambient seat     | resolved default propagator                     |

`Baggage` is an immutable value — a discarded `SetBaggage` return changes nothing — and `Baggage.Current` is the one write surface; `Activity.Baggage` stays read-only foreign material.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: admission and shaping on the builders
- rail: telemetry composition

| [INDEX] | [SURFACE]                              | [KIND]           | [CAPABILITY]                                                 |
| :-----: | :------------------------------------- | :--------------- | :----------------------------------------------------------- |
|  [01]   | `AddSource(params string[])`           | trace admission  | admits `ActivitySource` names                                |
|  [02]   | `AddMeter(params string[])`            | metric admission | admits `Meter` names                                         |
|  [03]   | `AddView`                              | stream surgery   | name and instrument-selector overloads                       |
|  [04]   | `SetExemplarFilter`                    | exemplar policy  | one `ExemplarFilterType` per meter provider                  |
|  [05]   | `SetSampler`                           | sampler seat     | instance, generic, and factory overloads                     |
|  [06]   | `AddProcessor`                         | processor chain  | registration order is execution order                        |
|  [07]   | `ConfigureResource`                    | identity verb    | augmenting resource mutation on any builder                  |
|  [08]   | `SetResourceBuilder`                   | identity replace | discards earlier identity — augmentation is the default verb |
|  [09]   | `AddOpenTelemetry` (`ILoggingBuilder`) | log bridge       | in-box logger bridge; optional options delegate              |
|  [10]   | `Build()`                              | provider mint    | returns the disposable provider handle                       |

## [04]-[IMPLEMENTATION_LAW]

[COMPOSITION_TOPOLOGY]:
- namespaces: `OpenTelemetry`, `OpenTelemetry.Trace`, `OpenTelemetry.Metrics`, `OpenTelemetry.Logs`, `OpenTelemetry.Resources`
- hostless root: `Sdk.CreateTracerProviderBuilder()` / `Sdk.CreateMeterProviderBuilder()` / `Sdk.CreateLoggerProviderBuilder()` — the plugin-process path with no generic host
- hosted root: the `OpenTelemetry.Extensions.Hosting` builder composes these same builders through DI
- identity root: `ResourceBuilder.CreateDefault()` + `AddService(...)` with `autoGenerateServiceInstanceId: false` — the boot mint pins `serviceInstanceId`
- volume root: `ParentBasedSampler(new TraceIdRatioBasedSampler(ratio))` declared once; the recorded bit derives log sampling and `TraceBased` exemplars
- shape root: `AddView` rows — `MetricStreamConfiguration.Drop` kills, `TagKeys` projects, `CardinalityLimit` pre-commits memory
- drain root: provider `ForceFlush(timeoutMilliseconds)` then `Dispose()`, traces and metrics before the log provider

[STACKING]:
- `OpenTelemetry.Extensions.Hosting`(`api-opentelemetry-hosting.md`): `AddOpenTelemetry()` yields the DI builder whose `WithTracing`/`WithMetrics`/`WithLogging` delegates receive these provider builders.
- `OpenTelemetry.Exporter.OpenTelemetryProtocol`(`api-opentelemetry-exporter-otlp.md`): `UseOtlpExporter()` claims all three signals; exporter I/O runs inside `SuppressInstrumentationScope.Begin`.
- instrumentation packages (`api-otel-instrumentation-*.md`): each admits a foreign library's emission by name onto these builders — libraries themselves emit through in-box `System.Diagnostics` only.

[LOCAL_ADMISSION]:
- Provider construction is a composition-root act: the AppHost root under the generic host, one explicit `Sdk.Create*ProviderBuilder` set per plugin load context with lifetime bound to the context — `AssemblyLoadContext.Unloading` hooks `ForceFlush` then `Dispose` on every provider it minted.
- `IMeterFactory` scoping isolates same-named Meters across co-resident plugin contexts; a raw `static Meter` shares the global registry and cross-contaminates.
- Latency families default to the base2 exponential histogram; `ExplicitBucketHistogramConfiguration.Boundaries` is the fallback row where a backend lacks exponential support, paired with the library-side `InstrumentAdvice<T>` bucket hint.
- `SetExemplarFilter(ExemplarFilterType.TraceBased)` composes on every meter provider so metric points inside a sampled span carry trace/span exemplar links.
- Propagation registers explicitly at every root: `Sdk.SetDefaultTextMapPropagator(new CompositeTextMapPropagator([new TraceContextPropagator(), new BaggagePropagator()]))`.
- Resource identity, scope naming, and metric naming compose from the estate wire law the AppHost observability pages own; this catalog carries the package surface those rows bind.

[RAIL_LAW]:
- Package: `OpenTelemetry` (+ `OpenTelemetry.Api` contracts)
- Owns: provider construction, resource identity stamping, sampling, views, exemplar policy, processor chains, and the propagator seat
- Accept: composition-root and per-plugin-context provider ownership over in-box `System.Diagnostics` emission
- Reject: an `OpenTelemetry` reference in any library project; a second provider owner inside one root; per-signal sampling probabilities beside the one root sampler
