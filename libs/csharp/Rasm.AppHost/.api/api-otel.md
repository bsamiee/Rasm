# [RASM_APPHOST_API_OTEL]

`OpenTelemetry` owns the SDK provider pipeline projecting trace, metric, and log signals through processors, readers, and exporters. Every provider builds once at the composition root over a `Resource` identity, and no exporter binds below it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry`
- package: `OpenTelemetry`
- assembly: `OpenTelemetry`
- api_assembly: `OpenTelemetry.Api`
- namespace: `OpenTelemetry`, `OpenTelemetry.Trace`, `OpenTelemetry.Metrics`, `OpenTelemetry.Logs`, `OpenTelemetry.Resources`, `OpenTelemetry.Context`, `OpenTelemetry.Context.Propagation`
- asset: runtime library
- rail: telemetry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: provider and resource family

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]     | [CAPABILITY]          |
| :-----: | :---------------------- | :---------------- | :-------------------- |
|  [01]   | `TracerProvider`        | trace provider    | trace pipeline        |
|  [02]   | `TracerProviderBuilder` | trace builder     | trace pipeline setup  |
|  [03]   | `MeterProvider`         | metric provider   | metric pipeline       |
|  [04]   | `MeterProviderBuilder`  | metric builder    | metric pipeline setup |
|  [05]   | `LoggerProvider`        | log provider      | log pipeline          |
|  [06]   | `LoggerProviderBuilder` | log builder       | log pipeline setup    |
|  [07]   | `Resource`              | resource value    | telemetry identity    |
|  [08]   | `ResourceBuilder`       | resource builder  | resource construction |
|  [09]   | `IResourceDetector`     | resource detector | resource discovery    |
|  [10]   | `BaseProvider`          | provider base     | provider lifetime     |

[PUBLIC_TYPE_SCOPE]: processor exporter and reader family

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]      | [CAPABILITY]           |
| :-----: | :------------------------------- | :----------------- | :--------------------- |
|  [01]   | `BaseProcessor<T>`               | processor base     | signal processing      |
|  [02]   | `BaseExporter<T>`                | exporter base      | signal export          |
|  [03]   | `BaseExportProcessor<T>`         | exporter processor | export bridge          |
|  [04]   | `SimpleExportProcessor<T>`       | export processor   | immediate export       |
|  [05]   | `BatchExportProcessor<T>`        | export processor   | queued batch export    |
|  [06]   | `BatchExportProcessorOptions<T>` | batch options      | queue/export policy    |
|  [07]   | `CompositeProcessor<T>`          | processor chain    | multi-processor fanout |
|  [08]   | `Batch<T>`                       | export batch       | batch payload          |
|  [09]   | `ExportResult`                   | export result      | export success/failure |
|  [10]   | `MetricReader`                   | metric reader      | collection/export loop |
|  [11]   | `PeriodicExportingMetricReader`  | metric reader      | periodic metric export |
|  [12]   | `MetricReaderOptions`            | reader options     | reader policy          |

[PUBLIC_TYPE_SCOPE]: signal model family

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]        | [CAPABILITY]                             |
| :-----: | :--------------------------- | :------------------- | :--------------------------------------- |
|  [01]   | `Tracer`                     | trace emitter        | span creation                            |
|  [02]   | `TelemetrySpan`              | span handle          | span mutation                            |
|  [03]   | `SpanContext`                | trace context        | trace identity                           |
|  [04]   | `SpanAttributes`             | attribute bag        | span attributes                          |
|  [05]   | `SpanKind`                   | span kind enum       | internal/server/client/producer/consumer |
|  [06]   | `Link`                       | span link            | causal link                              |
|  [07]   | `Status`                     | span status          | trace result                             |
|  [08]   | `StatusCode`                 | status code enum     | unset/ok/error                           |
|  [09]   | `Sampler`                    | sampling policy      | trace admission base                     |
|  [10]   | `SamplingResult`             | sampling result      | trace admission result                   |
|  [11]   | `Metric`                     | metric payload       | metric export payload                    |
|  [12]   | `MetricPoint`                | metric point         | metric timeseries point                  |
|  [13]   | `MetricStreamConfiguration`  | metric stream        | metric stream policy                     |
|  [14]   | `Exemplar`                   | exemplar value       | trace-linked measurement                 |
|  [15]   | `ExemplarFilterType`         | exemplar filter enum | exemplar admission policy                |
|  [16]   | `ReadOnlyExemplarCollection` | exemplar collection  | metric-point exemplars                   |
|  [17]   | `Logger`                     | log emitter          | log record emission                      |
|  [18]   | `OpenTelemetryLoggerOptions` | logger options       | log capture policy                       |
|  [19]   | `LogRecord`                  | log payload          | log export payload                       |
|  [20]   | `LogRecordData`              | log data             | log emission payload                     |
|  [21]   | `LogRecordAttributeList`     | log attributes       | log attribute payload                    |
|  [22]   | `LogRecordSeverity`          | log severity enum    | log severity vocabulary                  |

[PUBLIC_TYPE_SCOPE]: context and propagation family

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]       | [CAPABILITY]            |
| :-----: | :--------------------------- | :------------------ | :---------------------- |
|  [01]   | `Baggage`                    | baggage value       | cross-boundary metadata |
|  [02]   | `PropagationContext`         | propagation value   | trace/baggage carrier   |
|  [03]   | `TextMapPropagator`          | propagator base     | carrier inject/extract  |
|  [04]   | `TraceContextPropagator`     | propagator          | W3C trace context       |
|  [05]   | `BaggagePropagator`          | propagator          | W3C baggage             |
|  [06]   | `CompositeTextMapPropagator` | propagator chain    | combined propagation    |
|  [07]   | `Propagators`                | propagator registry | default propagator      |
|  [08]   | `RuntimeContext`             | context registry    | ambient runtime context |
|  [09]   | `RuntimeContextSlot<T>`      | context slot        | typed ambient slot      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider builder operations

| [INDEX] | [SURFACE]                                     | [SHAPE] | [CAPABILITY]            |
| :-----: | :-------------------------------------------- | :------ | :---------------------- |
|  [01]   | `Sdk.CreateTracerProviderBuilder`             | factory | trace builder creation  |
|  [02]   | `Sdk.CreateMeterProviderBuilder`              | factory | metric builder creation |
|  [03]   | `AddSource`                                   | static  | trace source admission  |
|  [04]   | `AddLegacySource`                             | static  | legacy source admission |
|  [05]   | `AddMeter`                                    | static  | meter admission         |
|  [06]   | `AddInstrumentation<T>`                       | static  | instrumentation hook    |
|  [07]   | `SetResourceBuilder`                          | static  | resource replacement    |
|  [08]   | `ConfigureResource`                           | static  | resource augmentation   |
|  [09]   | `AddProcessor`                                | static  | processor admission     |
|  [10]   | `AddReader`                                   | static  | metric reader admission |
|  [11]   | `AddView`                                     | static  | stream shaping          |
|  [12]   | `SetExemplarFilter`                           | static  | exemplar policy         |
|  [13]   | `SetSampler`                                  | static  | sampler instance        |
|  [14]   | `SetSampler<T>()`                             | static  | generic sampler         |
|  [15]   | `SetSampler(Func<IServiceProvider, Sampler>)` | static  | sampler factory         |
|  [16]   | `Build`                                       | factory | provider construction   |

- `SetSampler`: accepts `AlwaysOnSampler`, `AlwaysOffSampler`, `ParentBasedSampler(Sampler)`, `TraceIdRatioBasedSampler(double)` on the tracer-provider builder.
- `SetExemplarFilter`: `ExemplarFilterType` gates trace-linked sample admission on the meter-provider builder — `AlwaysOff`, `AlwaysOn`, `TraceBased`.

[ENTRYPOINT_SCOPE]: signal operations

| [INDEX] | [SURFACE]                     | [SHAPE]  | [CAPABILITY]            |
| :-----: | :---------------------------- | :------- | :---------------------- |
|  [01]   | `GetTracer`                   | instance | tracer lookup           |
|  [02]   | `StartSpan`                   | instance | inactive span           |
|  [03]   | `StartActiveSpan`             | instance | ambient active span     |
|  [04]   | `StartRootSpan`               | instance | parentless root span    |
|  [05]   | `SetAttribute`                | instance | span attribute          |
|  [06]   | `AddEvent`                    | instance | span event              |
|  [07]   | `AddLink`                     | instance | linked span context     |
|  [08]   | `SetStatus`                   | instance | status projection       |
|  [09]   | `RecordException`             | instance | exception projection    |
|  [10]   | `EmitLog`                     | instance | log record creation     |
|  [11]   | `LogRecordAttributeList.Add`  | instance | log attribute admission |
|  [12]   | `MetricReader.Collect`        | instance | metric snapshot         |
|  [13]   | `MetricReader.ForceFlush`     | instance | reader flush            |
|  [14]   | `MetricPoint.TryGetExemplars` | instance | metric-point exemplars  |

[ENTRYPOINT_SCOPE]: processor exporter and propagation operations

| [INDEX] | [SURFACE]                              | [SHAPE]  | [CAPABILITY]        |
| :-----: | :------------------------------------- | :------- | :------------------ |
|  [01]   | `BaseProcessor.OnStart`                | instance | signal start        |
|  [02]   | `BaseProcessor.OnEnd`                  | instance | signal end          |
|  [03]   | `BaseExporter.Export`                  | instance | batch export        |
|  [04]   | `ForceFlush`                           | instance | bounded flush       |
|  [05]   | `Shutdown`                             | instance | bounded shutdown    |
|  [06]   | `TextMapPropagator.Inject`             | instance | outbound carrier    |
|  [07]   | `TextMapPropagator.Extract`            | instance | inbound carrier     |
|  [08]   | `Propagators.DefaultTextMapPropagator` | property | process propagation |
|  [09]   | `Baggage.SetBaggage`                   | static   | baggage write       |
|  [10]   | `RuntimeContext.RegisterSlot`          | static   | typed context slot  |
|  [11]   | `Baggage.Current`                      | property | ambient get/set     |
|  [12]   | `GetBaggage`                           | static   | ambient lookup      |

- `Baggage.Current`: carries inbound baggage after `Extract` populates the propagation context.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every signal folds through a provider built once at the composition root; processors and readers pipeline it to exporters, and a `Resource` binds identity at construction.
- Trace context and baggage cross process and sidecar boundaries only through explicit `TextMapPropagator` carriers.

[STACKING]:
- `api-otel-exporter`(`.api/api-otel-exporter.md`): `OtlpTraceExporter`/`OtlpMetricExporter`/`OtlpLogExporter` bind onto these builders through `AddProcessor`/`AddReader`, or `UseOtlpExporter` wires all three signals at once.
- `api-otel-aspnetcore`(`.api/api-otel-aspnetcore.md`): instrumentation registers its `ActivitySource` and hosting meters onto the builders through `AddSource`/`AddMeter` under the `AddInstrumentation` family.
- Observability composition root (`.planning/Observability/telemetry.md`): folds `Sdk.CreateTracerProviderBuilder`/`CreateMeterProviderBuilder` and `Build` once, threading `Resource` identity and the processor/reader chain into the built providers.

[LOCAL_ADMISSION]:
- `Resource` identity binds before provider construction.
- Signals emit only through a built provider; `ForceFlush` and `Shutdown` drain on unload receipts.
- Cross-boundary trace and baggage flow crosses only through explicit propagators.
- Telemetry projection failures never mutate runtime state.

[RAIL_LAW]:
- Package: `OpenTelemetry`
- Owns: trace, metric, and log provider construction and the signal-processing pipeline
- Accept: signals project through built providers over resources, processors, readers, and samplers
- Reject: the SDK provider pipeline or any exporter bound below the composition root; a hand-rolled collect/export loop the `MetricReader` and processor chain already own
