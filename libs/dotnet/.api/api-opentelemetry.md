# [RASM_API_OPENTELEMETRY]

`OpenTelemetry` folds the runtime's `System.Diagnostics` emission into exportable trace, metric, and log streams — admission by name, resource identity, head sampling, view surgery, exemplar policy, reader cadence, and the processor chain that drains to one exporter. Contract assembly `OpenTelemetry.Api` carries the propagation and ambient-slot surface an emitting library reaches without an SDK reference.

## [01]-[PUBLIC_TYPES]

[ROOT_TYPES]: SDK roots, provider handles, and resource identity

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]  | [CAPABILITY]                                           |
| :-----: | :------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `OpenTelemetrySdk`               | sealed class   | one disposable root over all three providers           |
|  [02]   | `IOpenTelemetryBuilder`          | interface      | `Services` seat every cross-cutting verb extends       |
|  [03]   | `Sdk`                            | static class   | hostless builder mint and default-propagator seat      |
|  [04]   | `BaseProvider`                   | abstract class | disposable base of the three provider handles          |
|  [05]   | `TracerProvider`                 | class          | tracer mint and span drain root                        |
|  [06]   | `MeterProvider`                  | class          | metric drain root                                      |
|  [07]   | `LoggerProvider`                 | class          | log drain root                                         |
|  [08]   | `TracerProviderBuilder`          | abstract class | source and instrumentation admission base              |
|  [09]   | `MeterProviderBuilder`           | abstract class | meter and instrumentation admission base               |
|  [10]   | `LoggerProviderBuilder`          | abstract class | instrumentation admission base                         |
|  [11]   | `Resource`                       | class          | immutable attribute set folding through `Merge`        |
|  [12]   | `ResourceBuilder`                | class          | detector chain resolving one `Resource`                |
|  [13]   | `IResourceDetector`              | interface      | one `Detect()` attribute contribution                  |
|  [14]   | `IDeferredTracerProviderBuilder` | interface      | defers a trace-builder callback to service resolution  |
|  [15]   | `IDeferredMeterProviderBuilder`  | interface      | defers a metric-builder callback to service resolution |
|  [16]   | `IDeferredLoggerProviderBuilder` | interface      | defers a log-builder callback to service resolution    |

- Deferred interfaces let an instrumentation package register a builder callback before the `IServiceProvider` exists, and a builder implementing one hands the callback its resolved services at `Build`.
- `Sdk`'s static constructor runs on first touch of ANY member and has three process-wide effects — it seats a `CompositeTextMapPropagator` of trace-context beside baggage as `Propagators.DefaultTextMapPropagator`, pins `Activity.DefaultIdFormat` to W3C, and sets `Activity.ForceDefaultIdFormat`. Registration therefore REPLACES an equivalent-but-distinct value rather than filling an empty seat.
- `Sdk.SetDefaultTextMapPropagator` returns `void`, so an expression-bodied composition lifts it rather than discarding it through a tuple slot.

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
- `MetricStreamConfiguration`: `Name` `Description` `TagKeys` `ExcludedTagKeys` `CardinalityLimit`, beside the static `Drop` row. `TagKeys` is an allowlist and `ExcludedTagKeys` its denylist twin, each copied on set; an empty `TagKeys` array yields a tagless stream while an empty `ExcludedTagKeys` array excludes nothing. Leaving `CardinalityLimit` unset inherits the provider default of 2000, and setting it below one throws at assignment.

[SPAN_API_TYPES]: the contract-assembly span shim over `Activity`

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]   | [CAPABILITY]                                       |
| :-----: | :-------------------------- | :-------------- | :------------------------------------------------- |
|  [01]   | `Tracer`                    | class           | scope-keyed span mint off a `TracerProvider`       |
|  [02]   | `TelemetrySpan`             | class           | disposable span handle wrapping one `Activity`     |
|  [03]   | `SpanContext`               | readonly struct | trace and span id pair with flags and trace state  |
|  [04]   | `SpanAttributes`            | class           | initial-attribute bag a start call takes           |
|  [05]   | `SpanKind`                  | enum            | `Internal` `Server` `Client` `Producer` `Consumer` |
|  [06]   | `Link`                      | readonly struct | one causal link to a foreign span context          |
|  [07]   | `Status`                    | readonly struct | `StatusCode` with an optional description          |
|  [08]   | `StatusCode`                | enum            | `Unset` `Ok` `Error`                               |
|  [09]   | `ActivityExtensions`        | static class    | `SetStatus`/`GetStatus` over a raw `Activity`      |
|  [10]   | `ActivityContextExtensions` | static class    | `IsValid` over a raw `ActivityContext`             |

- Shim types wrap `Activity`, so a span minted here and one minted from an `ActivitySource` land in the same pipeline, while `api-diagnostics-activity.md` owns the in-box surface every emitting library binds instead.
- `TelemetrySpan.Context` `IsRecording` `ParentSpanId` read the wrapped activity, and `End()` closes it — a span left unended never exports.

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

- Capture flags each default `false`, so an unconfigured bridge exports records with no scope state, no formatted body where a message template resolves, and `State` in place of parsed `Attributes`.
- Severity is derived and internal: `LogRecordSeverity`, `LogRecord.Severity`, and `LogRecord.SeverityText` are all `internal`, and the SDK stamps `(int)LogLevel * 4 + 1` with the matching text — so a composition pins the mapping by pinning `LogLevel`, and `LogLevel.None` maps to no severity at all.

[CONTEXT_TYPES]: propagation and ambient slots, carried by the contract assembly

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY]   | [CAPABILITY]                             |
| :-----: | :--------------------------------- | :-------------- | :--------------------------------------- |
|  [01]   | `Baggage`                          | readonly struct | immutable ambient key-value set          |
|  [02]   | `TextMapPropagator`                | abstract class  | inject and extract over carrier adapters |
|  [03]   | `TraceContextPropagator`           | class           | W3C `traceparent` and `tracestate` leg   |
|  [04]   | `BaggagePropagator`                | class           | W3C `baggage` leg                        |
|  [05]   | `B3Propagator`                     | class           | single- or multi-header B3 legacy leg    |
|  [06]   | `CompositeTextMapPropagator`       | class           | one composite over ordered legs          |
|  [07]   | `PropagationContext`               | readonly struct | activity context paired with baggage     |
|  [08]   | `Propagators`                      | static class    | resolved process default propagator      |
|  [09]   | `RuntimeContext`                   | static class    | named ambient slot registry              |
|  [10]   | `RuntimeContextSlot<T>`            | abstract class  | one named typed ambient slot             |
|  [11]   | `AsyncLocalRuntimeContextSlot<T>`  | class           | slot flowing across async continuations  |
|  [12]   | `ThreadLocalRuntimeContextSlot<T>` | class           | slot pinned to the emitting thread       |
|  [13]   | `IRuntimeContextSlotValueAccessor` | interface       | untyped read of a slot's value           |

- `Baggage`: every INSTANCE mutation returns a new value, so a discarded instance-`SetBaggage` return changes nothing while the STATIC overloads write `Baggage.Current` in place whenever their trailing `Baggage` argument stays default — the two families read identically at a call site and only one of them mutates.
- `SetBaggage(string, string?)` forwards a null value to `RemoveBaggage`, so set-or-clear is one call and an ambient-store mirror needs no clearing branch.

## [02]-[ENTRYPOINTS]

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

| [INDEX] | [SURFACE]                                                                    | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :--------------------------------------------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `TracerProviderBuilder.AddSource(string[])`                                  | instance | admits `ActivitySource` names       |
|  [02]   | `TracerProviderBuilder.AddLegacySource(string)`                              | instance | admits a sourceless `Activity` name |
|  [03]   | `TracerProviderBuilder.AddInstrumentation<T>(Func<T>)`                       | instance | binds an instrumentation lifetime   |
|  [04]   | `TracerProviderBuilder.SetSampler(Sampler)`                                  | static   | seats the one head sampler          |
|  [05]   | `TracerProviderBuilder.SetSampler<T>()`                                      | static   | sampler type the SDK constructs     |
|  [06]   | `TracerProviderBuilder.SetSampler(Func<IServiceProvider, Sampler>)`          | static   | sampler resolved from services      |
|  [07]   | `TracerProviderBuilder.SetErrorStatusOnException(bool)`                      | static   | stamps error status on span escape  |
|  [08]   | `TracerProviderBuilder.AddProcessor(BaseProcessor<Activity>)`                | static   | appends one span processor          |
|  [09]   | `MeterProviderBuilder.AddMeter(string[])`                                    | instance | admits `Meter` names                |
|  [10]   | `MeterProviderBuilder.AddView(string, string)`                               | static   | renames one instrument's stream     |
|  [11]   | `MeterProviderBuilder.AddView(string, MetricStreamConfiguration)`            | static   | shapes one named instrument         |
|  [12]   | `MeterProviderBuilder.AddView(Func<Instrument, MetricStreamConfiguration?>)` | static   | shapes by instrument predicate      |
|  [13]   | `MeterProviderBuilder.AddReader(MetricReader)`                               | static   | appends one collect-export reader   |
|  [14]   | `MeterProviderBuilder.SetExemplarFilter(ExemplarFilterType)`                 | static   | one exemplar policy per provider    |
|  [15]   | `MeterProviderBuilder.SetMaxMetricStreams(int)`                              | static   | caps distinct streams per provider  |
|  [16]   | `LoggerProviderBuilder.AddProcessor(BaseProcessor<LogRecord>)`               | static   | appends one log processor           |
|  [17]   | `ILoggingBuilder.AddOpenTelemetry(Action<OpenTelemetryLoggerOptions>)`       | static   | in-box `ILogger` bridge             |

- Each provider builder declares `AddInstrumentation<T>` beside its own admission verb alone — `AddSource` and `AddLegacySource` on the tracer base, `AddMeter` on the meter base, nothing on the logger base.
- Every other row is an SDK-assembly extension whose namespace a composing fence binds: `OpenTelemetry.Trace.TracerProviderBuilderExtensions`, `OpenTelemetry.Metrics.MeterProviderBuilderExtensions`, `OpenTelemetry.Logs.LoggerProviderBuilderExtensions`, and `Microsoft.Extensions.Logging.OpenTelemetryLoggingExtensions` carrying `AddOpenTelemetry`.
- Bases ship in the contract assembly and their shaping extensions in the SDK assembly, so a library referencing `OpenTelemetry.Api` alone reaches admission and no shaping verb — the reference split seating view surgery, sampling, and the processor chain at a composition root.
- `AddView` rows are cumulative selectors, never a first-match ladder: both string forms compile to the predicate form returning `null` on a miss, a `*`/`?` wildcard name compiles to an ignore-case regex, and EVERY matching row mints its own stream off one instrument — the behavior `SetMaxMetricStreams` documents as "a single instrument can result in multiple metric streams". Pairing a named row with a wildcard row therefore exports one instrument twice under differing `TagKeys`, so a projection guarantee holds only where exactly one row can match; the predicate form is the one shape carrying per-instrument resolution, and returning `null` takes the provider default.
- Two ceilings partition metric memory and neither substitutes for the other: row [15] caps how many STREAMS one provider mints, guarding `[1, int.MaxValue]` and throwing below one, while `MetricStreamConfiguration.CardinalityLimit` caps the points inside every stream its view selector matches. Row [15] is the whole builder-level surface, so a provider-wide point ceiling lands as one predicate-form `AddView` row returning a configured limit for each instrument.
- `AddProcessor` and `AddReader`: each carries generic and `Func<IServiceProvider, …>` overloads beside the direct-argument form, and registration order is execution order.
- Every provider builder carries `ConfigureResource(Action<ResourceBuilder>)`, `SetResourceBuilder(ResourceBuilder)`, and `Build()`; `SetResourceBuilder` discards earlier identity where `ConfigureResource` augments it.

[IDENTITY_ENTRY]: resource identity, provider drain, and the metric read path

| [INDEX] | [SURFACE]                                                              | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :--------------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `ResourceBuilder.CreateDefault()`                                      | factory  | telemetry-SDK and env-seeded builder    |
|  [02]   | `ResourceBuilder.CreateEmpty()`                                        | factory  | unseeded builder                        |
|  [03]   | `ResourceBuilder.AddService(string, string, string, bool, string)`     | static   | the service identity triple             |
|  [04]   | `ResourceBuilder.AddAttributes(IEnumerable<KeyValuePair<…>>)`          | static   | caller attribute rows                   |
|  [05]   | `ResourceBuilder.AddAttributes(IEnumerable<KeyValuePair<…>>, string?)` | static   | caller rows stamped with a schema url   |
|  [06]   | `ResourceBuilder.AddTelemetrySdk()`                                    | static   | `telemetry.sdk.*` attributes            |
|  [07]   | `ResourceBuilder.AddEnvironmentVariableDetector()`                     | static   | environment-declared attribute rows     |
|  [08]   | `ResourceBuilder.AddDetector(IResourceDetector)`                       | instance | appends one detector                    |
|  [09]   | `IResourceDetector.Detect()`                                           | instance | one detector's attribute contribution   |
|  [10]   | `ResourceBuilder.Build()`                                              | instance | folds every detector into one resource  |
|  [11]   | `Resource(IEnumerable<KeyValuePair<string, object>>)`                  | ctor     | a detected attribute set                |
|  [12]   | `Resource.Merge(Resource)`                                             | fold     | joins two attribute sets                |
|  [13]   | `ProviderExtensions.GetResource(BaseProvider)`                         | static   | reads a provider's resolved resource    |
|  [14]   | `TracerProvider.ForceFlush(int)`                                       | instance | drains pending signal                   |
|  [15]   | `TracerProvider.Shutdown(int)`                                         | instance | terminal drain                          |
|  [16]   | `Metric.GetMetricPoints()`                                             | instance | allocation-free point enumeration       |
|  [17]   | `MetricPoint.TryGetExemplars(out ReadOnlyExemplarCollection)`          | instance | span-linked samples off one point       |
|  [18]   | `MetricReader.Collect(int)`                                            | instance | on-demand collect outside the cadence   |
|  [19]   | `MetricReader.TemporalityPreference`                                   | property | the reader's per-instrument temporality |

- `ResourceBuilder` declares `AddDetector`, `Clear()`, and `Build()` alone, `AddDetector` carrying a `Func<IServiceProvider, IResourceDetector>` twin that resolves its detector from services; `AddService`, both `AddAttributes` forms, `AddTelemetrySdk`, and `AddEnvironmentVariableDetector` are `OpenTelemetry.Resources.ResourceBuilderExtensions` verbs.
- `AddAttributes`'s trailing-`schemaUrl` overload is the one seat by which a minted identity states its own semconv coordinate: the bare form lands schema-less and adopts whatever the detector chain stamps, which `api-otel-resources.md` `[TOPOLOGY]` shows annihilating the coordinate on the first disagreement.
- `ForceFlush(int)` and `Shutdown(int)`: all three providers carry both, and `TracerProvider` and `LoggerProvider` add `AddProcessor`.
- `MetricReader.Collect` is the one pull seat — a test rail and a scrape exporter drive it, and a periodic reader drives the same call on its interval.

[SPAN_API_ENTRY]: the contract-assembly span shim

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :-------------------------------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `TracerProvider.GetTracer(string, string?, string?, IEnumerable<…>?)` | instance | name, version, schema url, and tags  |
|  [02]   | `Tracer.StartSpan(string, SpanKind, in TelemetrySpan?, …)`            | instance | inactive span off an explicit parent |
|  [03]   | `Tracer.StartRootSpan(string, SpanKind, SpanAttributes?, …)`          | instance | parentless root span                 |
|  [04]   | `Tracer.WithSpan(TelemetrySpan?)`                                     | static   | seats a span as ambient current      |
|  [05]   | `TelemetrySpan.SetAttribute(string, …)`                               | instance | scalar and array attribute arms      |
|  [06]   | `TelemetrySpan.AddEvent(string, DateTimeOffset, SpanAttributes?)`     | instance | one timestamped span event           |
|  [07]   | `TelemetrySpan.AddLink(SpanContext, SpanAttributes?)`                 | instance | one causal link post-start           |
|  [08]   | `TelemetrySpan.SetStatus(Status)`                                     | instance | terminal status projection           |
|  [09]   | `TelemetrySpan.UpdateName(string)`                                    | instance | renames a started span               |
|  [10]   | `TelemetrySpan.End(DateTimeOffset)`                                   | instance | closes the wrapped activity          |
|  [11]   | `ActivityExtensions.SetStatus(Activity?, Status)`                     | static   | shim status onto a raw activity      |

- `GetTracer`'s `schemaUrl` argument is where the semconv coordinate binds, so tracer, meter, and logger scopes bump on one constant.
- Shim types carry no exception recorder, so `Activity.AddException`(`api-diagnostics-activity.md`) holds the one exception path.

[CONTEXT_ENTRY]: propagation, baggage, and ambient slots

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------------------------------ | :------- | :-------------------------------------- |
|  [01]   | `Baggage.Current`                                                               | property | the one ambient write surface           |
|  [02]   | `Baggage.SetBaggage(string, string?)`                                           | instance | new value; a null value removes the key |
|  [03]   | `Baggage.SetBaggage(IEnumerable<KeyValuePair<string, string?>>)`                | instance | folds many entries into one value       |
|  [04]   | `Baggage.RemoveBaggage(string)`                                                 | instance | drops one entry                         |
|  [05]   | `Baggage.ClearBaggage()`                                                        | instance | drops every entry                       |
|  [06]   | `Baggage.GetBaggage(string)`                                                    | instance | one entry read                          |
|  [07]   | `Baggage.GetBaggage()`                                                          | instance | the whole entry map                     |
|  [08]   | `Baggage.SetBaggage(string, string?, Baggage)`                                  | static   | ambient write when the arg is default   |
|  [09]   | `Baggage.RemoveBaggage(string, Baggage)`                                        | static   | ambient drop when the arg is default    |
|  [10]   | `Baggage.GetBaggage(string, Baggage)`                                           | static   | ambient read when the arg is default    |
|  [11]   | `Baggage.Create(Dictionary<string, string>?)`                                   | factory  | detached baggage value                  |
|  [12]   | `TextMapPropagator.Inject<T>(PropagationContext, T, Action<T, string, string>)` | instance | writes carrier headers                  |
|  [13]   | `TextMapPropagator.Extract<T>(PropagationContext, T, Func<T, string, …>)`       | instance | reads carrier headers                   |
|  [14]   | `Propagators.DefaultTextMapPropagator`                                          | property | the resolved process propagator         |
|  [15]   | `RuntimeContext.ContextSlotType`                                                | property | ambient carrier for every slot          |
|  [16]   | `RuntimeContext.RegisterSlot<T>(string)`                                        | static   | mints one named slot                    |
|  [17]   | `RuntimeContext.SetValue<T>(string, T)`                                         | static   | writes a slot by name                   |
|  [18]   | `RuntimeContext.GetValue<T>(string)`                                            | static   | reads a slot by name                    |
|  [19]   | `SuppressInstrumentationScope.Begin(bool)`                                      | static   | scoped recursion guard                  |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `OpenTelemetrySdk.Create` folds one `IOpenTelemetryBuilder` into a disposable root carrying all three providers, so a hostless load context owns one telemetry object.
- `Sdk.CreateTracerProviderBuilder` and `Sdk.CreateMeterProviderBuilder` mint standalone builders; a log provider reaches a hostless root through `OpenTelemetrySdk.Create` alone.
- Identity mints once at boot: `ResourceBuilder.CreateDefault()` then `AddService` with `autoGenerateServiceInstanceId: false` pins the instance id across restarts.
- One `ParentBasedSampler` over a `TraceIdRatioBasedSampler` declares volume, and the recorded bit derives log sampling and `TraceBased` exemplars.
- `AddView` rows shape streams at declaration: `MetricStreamConfiguration.Drop` kills a stream, `TagKeys` projects dimensions, `ExcludedTagKeys` subtracts them, and `CardinalityLimit` pre-commits per-stream memory; one predicate row resolving every instrument is the shape that keeps stream count equal to instrument count.
- View rows own histogram aggregation alone: `Base2ExponentialBucketHistogramConfiguration` and `ExplicitBucketHistogramConfiguration` derive from `MetricStreamConfiguration`, so one predicate returns the shape per instrument. No provider-wide aggregation default ships and `OTEL_EXPORTER_OTLP_METRICS_DEFAULT_HISTOGRAM_AGGREGATION` — the key other SDK trains read — appears in neither assembly here, so a base2 pin spelled as that variable governs nothing.
- `AddReader` binds collection cadence and temporality, so `PeriodicExportingMetricReader` sets the push interval and `MetricReaderTemporalityPreference` decides which instruments report delta.
- Registration order is execution order for processors and readers alike; `CompositeProcessor<T>` folds a chain the provider drives as one.
- Drain runs `ForceFlush(timeoutMilliseconds)` then `Dispose()`, traces and metrics ahead of the log provider.
- Exporter-owned I/O runs inside `SuppressInstrumentationScope.Begin`, so an instrumented transport never re-enters the pipeline draining it.
- `RuntimeContext.ContextSlotType` selects the ambient carrier before the first slot registers — the async-local slot flows across continuations and the thread-local slot pins to the emitting thread.

[STACKING]:
- `OpenTelemetry.Extensions.Hosting`(`api-opentelemetry-hosting.md`): `AddOpenTelemetry()` yields an `IOpenTelemetryBuilder`, so the host's `WithTracing`/`WithMetrics`/`WithLogging` delegates hand out the same three builders a plugin root mints.
- `OpenTelemetry.Exporter.OpenTelemetryProtocol`(`api-opentelemetry-exporter-otlp.md`): `UseOtlpExporter()` chains off `IOpenTelemetryBuilder` and claims all three signals; the per-signal `AddOtlpExporter` overloads land a `BaseExporter<T>` inside the processor and reader rows here.
- `OpenTelemetry.Extensions`(`api-opentelemetry-extensions.md`): supplies the `BaseProcessor<Activity>` and `Sampler` implementations this package declares — `AddBaggageActivityProcessor` fills a processor row, while its `RateLimitingSampler` absolute-ceiling arm and `AddAutoFlushActivityProcessor` flush arm are the `SetSampler` and processor rows the branch declares nowhere.
- resource detectors(`api-otel-resources.md`): each `Add<X>Detector` appends one `IResourceDetector` onto the `ResourceBuilder` an augmenting `ConfigureResource` delegate carries.
- `System.Diagnostics`(`api-diagnostics-activity.md`) and `System.Diagnostics.Metrics`(`api-diagnostics-metrics.md`): `AddSource` and `AddMeter` subscribe this SDK to the in-box `ActivitySource` and `Meter` names an emitting library mints.
- `Microsoft.Extensions.Diagnostics`(`api-extensions-diagnostics.md`): its provider-owned `IMeterFactory` scopes same-named meters per load context, so `AddMeter` admits one plugin's streams without touching a co-resident twin.
- instrumentation packages(`api-otel-instrumentation-*.md`): each `Add*Instrumentation` verb registers a foreign library's emission onto these builders through `AddInstrumentation<T>`.
- `Rasm.AppHost` `Observability/telemetry#SIGNAL_GOVERNANCE`: resource identity, scope naming, and metric naming compose from the observability conformance that page owns; `ResourceIdentity.Compose` seats its detector chain through `ConfigureResource`, `SignalGovernance.Views` binds ONE `AddView` predicate resolving each instrument against the contributed roster, and `SetExemplarFilter` pins the trace-based row.
- Within-lib: one `OpenTelemetrySdk.Create` call folds identity, sampler, view rows, exemplar policy, reader cadence, and the processor chain onto a single root, so a plugin composes its whole telemetry graph in one pass and drains through one handle.

[LOCAL_ADMISSION]:
- One `OpenTelemetrySdk.Create` root per plugin load context owns provider lifetime, and `AssemblyLoadContext.Unloading` hooks `ForceFlush` then `Dispose` on that root.
- `IMeterFactory` scoping isolates same-named meters across co-resident plugin contexts; a process-static `Meter` shares the global registry.
- Latency families take the base2 exponential histogram, and `ExplicitBucketHistogramConfiguration.Boundaries` carries the row where the backend consumes explicit buckets, paired with the library-side `InstrumentAdvice<T>` hint.
- `SetExemplarFilter(ExemplarFilterType.TraceBased)` composes on every meter provider, so a metric point inside a sampled span carries its trace and span link.
- Propagation registers explicitly at every root: `Sdk.SetDefaultTextMapPropagator(new CompositeTextMapPropagator([new TraceContextPropagator(), new BaggagePropagator()]))`.
- Metric exporters declare cadence through the reader they ride — `PeriodicExportingMetricReader` for push egress, `IPullMetricExporter` where the backend scrapes.
