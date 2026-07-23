# [RASM_API_OPENTELEMETRY]

`OpenTelemetry` folds the runtime's `System.Diagnostics` emission into exportable trace, metric, and log streams — admission by name, resource identity, head sampling, view surgery, exemplar policy, reader cadence, and the processor chain that drains to one exporter. Contract assembly `OpenTelemetry.Api` carries the propagation and ambient-slot surface an emitting library reaches without an SDK reference.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry`
- package: `OpenTelemetry` (Apache-2.0)
- assembly: `OpenTelemetry`
- contract assembly: `OpenTelemetry.Api` — propagation, ambient runtime-context slots, and the three provider base types
- builder assembly: `OpenTelemetry.Api.ProviderBuilderExtensions` — `IOpenTelemetryBuilder`, the seat hostless and hosted roots share
- namespace: `OpenTelemetry`, `OpenTelemetry.Trace`, `OpenTelemetry.Metrics`, `OpenTelemetry.Logs`, `OpenTelemetry.Resources`, `OpenTelemetry.Context`, `OpenTelemetry.Context.Propagation`, `Microsoft.Extensions.Logging`
- asset: runtime library
- rail: telemetry composition

## [02]-[PUBLIC_TYPES]

[ROOT_TYPES]: SDK roots, provider handles, and resource identity

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]  | [CAPABILITY]                                      |
| :-----: | :---------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `OpenTelemetrySdk`      | sealed class   | one disposable root over all three providers      |
|  [02]   | `IOpenTelemetryBuilder` | interface      | `Services` seat every cross-cutting verb extends  |
|  [03]   | `Sdk`                   | static class   | hostless builder mint and default-propagator seat |
|  [04]   | `BaseProvider`          | abstract class | disposable base of the three provider handles     |
|  [05]   | `TracerProvider`        | class          | tracer mint and span drain root                   |
|  [06]   | `MeterProvider`         | class          | metric drain root                                 |
|  [07]   | `LoggerProvider`        | class          | log drain root                                    |
|  [08]   | `TracerProviderBuilder` | abstract class | source and instrumentation admission base         |
|  [09]   | `MeterProviderBuilder`  | abstract class | meter and instrumentation admission base          |
|  [10]   | `LoggerProviderBuilder` | abstract class | instrumentation admission base                    |
|  [11]   | `Resource`              | class          | immutable attribute set folding through `Merge`   |
|  [12]   | `ResourceBuilder`       | class          | detector chain resolving one `Resource`           |
|  [13]   | `IResourceDetector`     | interface      | one `Detect()` attribute contribution             |

[VOLUME_TYPES]: head sampling, the processor chain, and exporter authoring

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]   | [CAPABILITY]                                  |
| :-----: | :------------------------------------- | :-------------- | :-------------------------------------------- |
|  [01]   | `Sampler`                              | abstract class  | one head verdict per trace id                 |
|  [02]   | `SamplingParameters`                   | readonly struct | parent context and span shape a verdict reads |
|  [03]   | `SamplingResult`                       | readonly struct | verdict carrying attributes and trace state   |
|  [04]   | `SamplingDecision`                     | enum            | the verdict vocabulary                        |
|  [05]   | `ParentBasedSampler`                   | sealed class    | delegate composite keyed by parent state      |
|  [06]   | `TraceIdRatioBasedSampler`             | sealed class    | deterministic probability over trace id       |
|  [07]   | `AlwaysOnSampler`                      | sealed class    | terminal record-and-sample seat               |
|  [08]   | `AlwaysOffSampler`                     | sealed class    | terminal drop seat                            |
|  [09]   | `BaseProcessor<T>`                     | abstract class  | start and end hooks with drain verbs          |
|  [10]   | `BaseExportProcessor<T>`               | abstract class  | export-on-end base owning one exporter        |
|  [11]   | `SimpleExportProcessor<T>`             | abstract class  | synchronous per-item export                   |
|  [12]   | `BatchExportProcessor<T>`              | abstract class  | queue-backed batched export                   |
|  [13]   | `BatchExportProcessorOptions<T>`       | class           | queue and batch bounds for one processor      |
|  [14]   | `CompositeProcessor<T>`                | class           | ordered fan over a processor chain            |
|  [15]   | `BatchActivityExportProcessor`         | class           | span batch processor                          |
|  [16]   | `SimpleActivityExportProcessor`        | class           | span pass-through processor                   |
|  [17]   | `BatchLogRecordExportProcessor`        | class           | log batch processor                           |
|  [18]   | `SimpleLogRecordExportProcessor`       | class           | log pass-through processor                    |
|  [19]   | `ExportProcessorType`                  | enum            | processor shape an exporter row selects       |
|  [20]   | `SuppressInstrumentationScope`         | sealed class    | ambient recursion guard around exporter I/O   |
|  [21]   | `BaseExporter<T>`                      | abstract class  | the one batch egress contract                 |
|  [22]   | `Batch<T>`                             | readonly struct | disposable allocation-free item run           |
|  [23]   | `ExportResult`                         | enum            | egress verdict                                |
|  [24]   | `MetricReader`                         | abstract class  | collect and shutdown under one temporality    |
|  [25]   | `BaseExportingMetricReader`            | class           | reader driving one metric exporter            |
|  [26]   | `PeriodicExportingMetricReader`        | class           | interval-driven collect and export            |
|  [27]   | `PeriodicExportingMetricReaderOptions` | class           | export interval and timeout                   |
|  [28]   | `MetricReaderOptions`                  | class           | temporality preference over periodic rows     |
|  [29]   | `MetricReaderTemporalityPreference`    | enum            | per-instrument temporality policy             |
|  [30]   | `IPullMetricExporter`                  | interface       | `Collect` seat a scrape exporter drives       |
|  [31]   | `ExportModes`                          | enum            | push or pull declaration                      |
|  [32]   | `ExportModesAttribute`                 | sealed class    | stamps an exporter's export mode              |

[SamplingDecision]: `Drop` `RecordOnly` `RecordAndSample`
[ExportProcessorType]: `Simple` `Batch`
[MetricReaderTemporalityPreference]: `Cumulative` `Delta` `LowMemory`

[STREAM_TYPES]: view rows, exemplars, and the metric read model

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY]   | [CAPABILITY]                               |
| :-----: | :--------------------------------------------- | :-------------- | :----------------------------------------- |
|  [01]   | `MetricStreamConfiguration`                    | class           | declaration-time stream surgery            |
|  [02]   | `HistogramConfiguration`                       | class           | min and max capture on any histogram row   |
|  [03]   | `Base2ExponentialBucketHistogramConfiguration` | sealed class    | exponential shape by size and scale        |
|  [04]   | `ExplicitBucketHistogramConfiguration`         | class           | caller-declared bucket boundaries          |
|  [05]   | `ExemplarFilterType`                           | enum            | measurement eligibility for exemplars      |
|  [06]   | `Exemplar`                                     | struct          | one measurement linked to its span         |
|  [07]   | `ReadOnlyExemplarCollection`                   | readonly struct | allocation-free exemplar enumeration       |
|  [08]   | `Metric`                                       | sealed class    | one stream identity over its points        |
|  [09]   | `MetricPoint`                                  | struct          | per-series value and bucket accessors      |
|  [10]   | `MetricPointsAccessor`                         | readonly struct | allocation-free point enumeration          |
|  [11]   | `ReadOnlyTagCollection`                        | readonly struct | one point's dimension set                  |
|  [12]   | `ReadOnlyFilteredTagCollection`                | readonly struct | an exemplar's residual dimensions          |
|  [13]   | `MetricType`                                   | enum            | instrument shape a reader discriminates on |
|  [14]   | `AggregationTemporality`                       | enum            | cumulative or delta window on a stream     |
|  [15]   | `HistogramBuckets`                             | class           | explicit-bucket count enumeration          |
|  [16]   | `ExponentialHistogramData`                     | sealed class    | scale and positive-bucket snapshot         |

[ExemplarFilterType]: `AlwaysOff` `AlwaysOn` `TraceBased`

- `MetricPoint`: each value accessor binds one instrument shape — `MetricType` selects the legal call and a mismatch faults.

[LOG_TYPES]: the `ILogger` bridge

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]   | [CAPABILITY]                               |
| :-----: | :------------------------------------- | :-------------- | :----------------------------------------- |
|  [01]   | `OpenTelemetryLoggerOptions`           | class           | log-record capture knobs                   |
|  [02]   | `OpenTelemetryLoggerProvider`          | class           | `ILoggerProvider` feeding the log pipeline |
|  [03]   | `LogRecord`                            | sealed class    | one captured record with its attributes    |
|  [04]   | `LogRecordScope`                       | readonly struct | one scope frame a callback folds           |
|  [05]   | `LogRecordExportProcessorOptions`      | class           | processor shape beside its batch options   |
|  [06]   | `BatchExportLogRecordProcessorOptions` | class           | log-specific queue and batch bounds        |

[OpenTelemetryLoggerOptions]: `IncludeScopes` `IncludeFormattedMessage` `ParseStateValues` `AddProcessor` `SetResourceBuilder`

[CONTEXT_TYPES]: propagation and ambient slots, carried by the contract assembly

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY]   | [CAPABILITY]                             |
| :-----: | :--------------------------------- | :-------------- | :--------------------------------------- |
|  [01]   | `Baggage`                          | readonly struct | immutable ambient key-value set          |
|  [02]   | `TextMapPropagator`                | abstract class  | inject and extract over carrier adapters |
|  [03]   | `TraceContextPropagator`           | class           | W3C `traceparent` and `tracestate` leg   |
|  [04]   | `BaggagePropagator`                | class           | W3C `baggage` leg                        |
|  [05]   | `CompositeTextMapPropagator`       | class           | one composite over ordered legs          |
|  [06]   | `PropagationContext`               | readonly struct | activity context paired with baggage     |
|  [07]   | `Propagators`                      | static class    | resolved process default propagator      |
|  [08]   | `RuntimeContext`                   | static class    | named ambient slot registry              |
|  [09]   | `RuntimeContextSlot<T>`            | abstract class  | one named typed ambient slot             |
|  [10]   | `AsyncLocalRuntimeContextSlot<T>`  | class           | slot flowing across async continuations  |
|  [11]   | `ThreadLocalRuntimeContextSlot<T>` | class           | slot pinned to the emitting thread       |
|  [12]   | `IRuntimeContextSlotValueAccessor` | interface       | untyped read of a slot's value           |

- `Baggage`: every mutation returns a new value, so a discarded `SetBaggage` return changes nothing and `Baggage.Current` is the one write surface.

## [03]-[ENTRYPOINTS]

Extension verbs list the arguments past their receiver.

[ROOT_ENTRY]: root mint and the `IOpenTelemetryBuilder` verbs `OpenTelemetryBuilderSdkExtensions` carries

| [INDEX] | [SURFACE]                                                                        | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :------------------------------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `OpenTelemetrySdk.Create(Action<IOpenTelemetryBuilder>)`                         | factory  | mints one disposable root          |
|  [02]   | `OpenTelemetrySdkExtensions.GetLoggerFactory(OpenTelemetrySdk)`                  | static   | `ILoggerFactory` off a root        |
|  [03]   | `WithTracing(Action<TracerProviderBuilder>)`                                     | static   | seats the trace builder            |
|  [04]   | `WithMetrics(Action<MeterProviderBuilder>)`                                      | static   | seats the metric builder           |
|  [05]   | `WithLogging(Action<LoggerProviderBuilder>, Action<OpenTelemetryLoggerOptions>)` | static   | seats the log builder and options  |
|  [06]   | `ConfigureResource(Action<ResourceBuilder>)`                                     | static   | augments identity across all three |
|  [07]   | `Sdk.CreateTracerProviderBuilder()`                                              | factory  | standalone trace builder           |
|  [08]   | `Sdk.CreateMeterProviderBuilder()`                                               | factory  | standalone metric builder          |
|  [09]   | `Sdk.SetDefaultTextMapPropagator(TextMapPropagator)`                             | static   | seats the process propagator       |
|  [10]   | `Sdk.SuppressInstrumentation`                                                    | property | ambient suppression flag           |

[BUILDER_ENTRY]: admission and shaping on the three provider builders

| [INDEX] | [SURFACE]                                                                   | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :-------------------------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `TracerProviderBuilder.AddSource(string[])`                                 | instance | admits `ActivitySource` names      |
|  [02]   | `TracerProviderBuilder.AddInstrumentation<T>(Func<T>)`                      | instance | binds an instrumentation lifetime  |
|  [03]   | `TracerProviderBuilder.SetSampler(Sampler)`                                 | instance | seats the one head sampler         |
|  [04]   | `TracerProviderBuilder.SetSampler<T>()`                                     | instance | sampler type the SDK constructs    |
|  [05]   | `TracerProviderBuilder.SetSampler(Func<IServiceProvider, Sampler>)`         | instance | sampler resolved from services     |
|  [06]   | `TracerProviderBuilder.SetErrorStatusOnException(bool)`                     | instance | stamps error status on span escape |
|  [07]   | `TracerProviderBuilder.AddProcessor(BaseProcessor<Activity>)`               | instance | appends one span processor         |
|  [08]   | `MeterProviderBuilder.AddMeter(string[])`                                   | instance | admits `Meter` names               |
|  [09]   | `MeterProviderBuilder.AddView(string, string)`                              | instance | renames one instrument's stream    |
|  [10]   | `MeterProviderBuilder.AddView(string, MetricStreamConfiguration)`           | instance | shapes one named instrument        |
|  [11]   | `MeterProviderBuilder.AddView(Func<Instrument, MetricStreamConfiguration>)` | instance | shapes by instrument predicate     |
|  [12]   | `MeterProviderBuilder.AddReader(MetricReader)`                              | instance | appends one collect-export reader  |
|  [13]   | `MeterProviderBuilder.SetExemplarFilter(ExemplarFilterType)`                | instance | one exemplar policy per provider   |
|  [14]   | `MeterProviderBuilder.SetMaxMetricStreams(int)`                             | instance | caps distinct streams per provider |
|  [15]   | `LoggerProviderBuilder.AddProcessor(BaseProcessor<LogRecord>)`              | instance | appends one log processor          |
|  [16]   | `ILoggingBuilder.AddOpenTelemetry(Action<OpenTelemetryLoggerOptions>)`      | instance | in-box `ILogger` bridge            |

- `AddProcessor` and `AddReader`: each carries generic and `Func<IServiceProvider, …>` overloads beside the instance form, and registration order is execution order.
- Every provider builder carries `ConfigureResource(Action<ResourceBuilder>)`, `SetResourceBuilder(ResourceBuilder)`, and `Build()`; `SetResourceBuilder` discards earlier identity where `ConfigureResource` augments it.

[IDENTITY_ENTRY]: resource identity, provider drain, and the metric read path

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :----------------------------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `ResourceBuilder.CreateDefault()`                                  | factory  | telemetry-SDK and env-seeded builder   |
|  [02]   | `ResourceBuilder.CreateEmpty()`                                    | factory  | unseeded builder                       |
|  [03]   | `ResourceBuilder.AddService(string, string, string, bool, string)` | instance | the service identity triple            |
|  [04]   | `ResourceBuilder.AddAttributes(IEnumerable<KeyValuePair<…>>)`      | instance | caller attribute rows                  |
|  [05]   | `ResourceBuilder.AddTelemetrySdk()`                                | instance | `telemetry.sdk.*` attributes           |
|  [06]   | `ResourceBuilder.AddEnvironmentVariableDetector()`                 | instance | environment-declared attribute rows    |
|  [07]   | `ResourceBuilder.AddDetector(IResourceDetector)`                   | instance | appends one detector                   |
|  [08]   | `ResourceBuilder.Build()`                                          | instance | folds every detector into one resource |
|  [09]   | `Resource.Merge(Resource)`                                         | fold     | joins two attribute sets               |
|  [10]   | `ProviderExtensions.GetResource(BaseProvider)`                     | static   | reads a provider's resolved resource   |
|  [11]   | `TracerProvider.ForceFlush(int)`                                   | instance | drains pending signal                  |
|  [12]   | `TracerProvider.Shutdown(int)`                                     | instance | terminal drain                         |
|  [13]   | `Metric.GetMetricPoints()`                                         | instance | allocation-free point enumeration      |
|  [14]   | `MetricPoint.TryGetExemplars(out ReadOnlyExemplarCollection)`      | instance | span-linked samples off one point      |

- `ForceFlush(int)` and `Shutdown(int)`: all three providers carry both, and `TracerProvider` and `LoggerProvider` add `AddProcessor`.

[CONTEXT_ENTRY]: propagation, baggage, and ambient slots

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :------------------------------------------------------------------------------ | :------- | :------------------------------ |
|  [01]   | `Baggage.Current`                                                               | property | the one ambient write surface   |
|  [02]   | `Baggage.SetBaggage(string, string)`                                            | instance | returns a new immutable value   |
|  [03]   | `Baggage.GetBaggage(string)`                                                    | instance | one entry read                  |
|  [04]   | `Baggage.Create(Dictionary<string, string>)`                                    | factory  | detached baggage value          |
|  [05]   | `TextMapPropagator.Inject<T>(PropagationContext, T, Action<T, string, string>)` | instance | writes carrier headers          |
|  [06]   | `TextMapPropagator.Extract<T>(PropagationContext, T, Func<T, string, …>)`       | instance | reads carrier headers           |
|  [07]   | `Propagators.DefaultTextMapPropagator`                                          | property | the resolved process propagator |
|  [08]   | `RuntimeContext.ContextSlotType`                                                | property | ambient carrier for every slot  |
|  [09]   | `RuntimeContext.RegisterSlot<T>(string)`                                        | static   | mints one named slot            |
|  [10]   | `RuntimeContext.SetValue<T>(string, T)`                                         | static   | writes a slot by name           |
|  [11]   | `RuntimeContext.GetValue<T>(string)`                                            | static   | reads a slot by name            |
|  [12]   | `SuppressInstrumentationScope.Begin(bool)`                                      | static   | scoped recursion guard          |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `OpenTelemetrySdk.Create` folds one `IOpenTelemetryBuilder` into a disposable root carrying all three providers, so a hostless load context owns one telemetry object.
- `Sdk.CreateTracerProviderBuilder` and `Sdk.CreateMeterProviderBuilder` mint standalone builders; a log provider reaches a hostless root through `OpenTelemetrySdk.Create` alone.
- Identity mints once at boot: `ResourceBuilder.CreateDefault()` then `AddService` with `autoGenerateServiceInstanceId: false` pins the instance id across restarts.
- One `ParentBasedSampler` over a `TraceIdRatioBasedSampler` declares volume, and the recorded bit derives log sampling and `TraceBased` exemplars.
- `AddView` rows shape streams at declaration: `MetricStreamConfiguration.Drop` kills a stream, `TagKeys` projects dimensions, and `CardinalityLimit` pre-commits per-stream memory.
- `AddReader` binds collection cadence and temporality, so `PeriodicExportingMetricReader` sets the push interval and `MetricReaderTemporalityPreference` decides which instruments report delta.
- Registration order is execution order for processors and readers alike; `CompositeProcessor<T>` folds a chain the provider drives as one.
- Drain runs `ForceFlush(timeoutMilliseconds)` then `Dispose()`, traces and metrics ahead of the log provider.
- Exporter-owned I/O runs inside `SuppressInstrumentationScope.Begin`, so an instrumented transport never re-enters the pipeline draining it.
- `RuntimeContext.ContextSlotType` selects the ambient carrier before the first slot registers — the async-local slot flows across continuations and the thread-local slot pins to the emitting thread.

[STACKING]:
- `OpenTelemetry.Extensions.Hosting`(`api-opentelemetry-hosting.md`): `AddOpenTelemetry()` yields an `IOpenTelemetryBuilder`, so the host's `WithTracing`/`WithMetrics`/`WithLogging` delegates hand out the same three builders a plugin root mints.
- `OpenTelemetry.Exporter.OpenTelemetryProtocol`(`api-opentelemetry-exporter-otlp.md`): `UseOtlpExporter()` chains off `IOpenTelemetryBuilder` and claims all three signals; the per-signal `AddOtlpExporter` overloads land a `BaseExporter<T>` inside the processor and reader rows here.
- `OpenTelemetry.Extensions`(`api-opentelemetry-extensions.md`): supplies the `BaseProcessor<Activity>` and `Sampler` implementations this package declares — `AddBaggageActivityProcessor` fills a processor row and `RateLimitingSampler` a `SetSampler` row.
- resource detectors(`api-otel-resources.md`): each `Add<X>Detector` appends one `IResourceDetector` onto the `ResourceBuilder` an augmenting `ConfigureResource` delegate carries.
- `System.Diagnostics`(`api-diagnostics-activity.md`) and `System.Diagnostics.Metrics`(`api-diagnostics-metrics.md`): `AddSource` and `AddMeter` subscribe this SDK to the in-box `ActivitySource` and `Meter` names an emitting library mints.
- `Microsoft.Extensions.Diagnostics`(`api-extensions-diagnostics.md`): its provider-owned `IMeterFactory` scopes same-named meters per load context, so `AddMeter` admits one plugin's streams without touching a co-resident twin.
- instrumentation packages(`api-otel-instrumentation-*.md`): each `Add*Instrumentation` verb registers a foreign library's emission onto these builders through `AddInstrumentation<T>`.
- `Rasm.AppHost` `Observability/telemetry`: resource identity, scope naming, and metric naming compose from the wire law that page owns, bound here by the `AddService`, `AddView`, and `SetExemplarFilter` rows.
- Within-lib: one `OpenTelemetrySdk.Create` call folds identity, sampler, view rows, exemplar policy, reader cadence, and the processor chain onto a single root, so a plugin composes its whole telemetry graph in one pass and drains through one handle.

[LOCAL_ADMISSION]:
- One `OpenTelemetrySdk.Create` root per plugin load context owns provider lifetime, and `AssemblyLoadContext.Unloading` hooks `ForceFlush` then `Dispose` on that root.
- `IMeterFactory` scoping isolates same-named meters across co-resident plugin contexts; a process-static `Meter` shares the global registry.
- Latency families take the base2 exponential histogram, and `ExplicitBucketHistogramConfiguration.Boundaries` carries the row where the backend consumes explicit buckets, paired with the library-side `InstrumentAdvice<T>` hint.
- `SetExemplarFilter(ExemplarFilterType.TraceBased)` composes on every meter provider, so a metric point inside a sampled span carries its trace and span link.
- Propagation registers explicitly at every root: `Sdk.SetDefaultTextMapPropagator(new CompositeTextMapPropagator([new TraceContextPropagator(), new BaggagePropagator()]))`.
- A metric exporter declares cadence through the reader it rides — `PeriodicExportingMetricReader` for push egress, `IPullMetricExporter` where the backend scrapes.

[RAIL_LAW]:
- Package: `OpenTelemetry`
- Owns: provider construction, resource identity, head sampling, view surgery, exemplar policy, reader cadence, the processor chain, and the propagator seat
- Accept: one composition-root or per-plugin-context root over in-box `System.Diagnostics` emission, drained through the handle that minted it
- Reject: an `OpenTelemetry` reference in a library project; a second provider owner inside one root; per-signal sampling probabilities beside the one root sampler
