# [RASM_APPHOST_API_OTEL]

`OpenTelemetry` supplies trace, metric, log, resource, processor, exporter, reader,
sampling, context, and propagation surfaces for telemetry projection.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry`
- package: `OpenTelemetry`
- assembly: `OpenTelemetry`
- api_assembly: `OpenTelemetry.Api`
- namespace: `OpenTelemetry`
- asset: runtime library
- rail: telemetry

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: provider and resource family
- rail: telemetry

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]     | [RAIL]                |
| :-----: | :---------------------- | :---------------- | :-------------------- |
|   [1]   | `TracerProvider`        | trace provider    | trace pipeline        |
|   [2]   | `TracerProviderBuilder` | trace builder     | trace pipeline setup  |
|   [3]   | `MeterProvider`         | metric provider   | metric pipeline       |
|   [4]   | `MeterProviderBuilder`  | metric builder    | metric pipeline setup |
|   [5]   | `LoggerProvider`        | log provider      | log pipeline          |
|   [6]   | `LoggerProviderBuilder` | log builder       | log pipeline setup    |
|   [7]   | `Resource`              | resource value    | telemetry identity    |
|   [8]   | `ResourceBuilder`       | resource builder  | resource construction |
|   [9]   | `IResourceDetector`     | resource detector | resource discovery    |
|  [10]   | `BaseProvider`          | provider base     | provider lifetime     |

[PUBLIC_TYPE_SCOPE]: processor exporter and reader family
- rail: telemetry

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]      | [RAIL]                 |
| :-----: | :------------------------------- | :----------------- | :--------------------- |
|   [1]   | `BaseProcessor<T>`               | processor base     | signal processing      |
|   [2]   | `BaseExporter<T>`                | exporter base      | signal export          |
|   [3]   | `BaseExportProcessor<T>`         | exporter processor | export bridge          |
|   [4]   | `SimpleExportProcessor<T>`       | export processor   | immediate export       |
|   [5]   | `BatchExportProcessor<T>`        | export processor   | queued batch export    |
|   [6]   | `BatchExportProcessorOptions<T>` | batch options      | queue/export policy    |
|   [7]   | `CompositeProcessor<T>`          | processor chain    | multi-processor fanout |
|   [8]   | `Batch<T>`                       | export batch       | batch payload          |
|   [9]   | `ExportResult`                   | export result      | export success/failure |
|  [10]   | `MetricReader`                   | metric reader      | collection/export loop |
|  [11]   | `PeriodicExportingMetricReader`  | metric reader      | periodic metric export |
|  [12]   | `MetricReaderOptions`            | reader options     | reader policy          |

[PUBLIC_TYPE_SCOPE]: signal model family
- rail: telemetry

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]   | [RAIL]                   |
| :-----: | :--------------------------- | :-------------- | :----------------------- |
|   [1]   | `Tracer`                     | trace emitter   | span creation            |
|   [2]   | `TelemetrySpan`              | span handle     | span mutation            |
|   [3]   | `SpanContext`                | trace context   | trace identity           |
|   [4]   | `SpanAttributes`             | attribute bag   | span attributes          |
|   [5]   | `Link`                       | span link       | causal link              |
|   [6]   | `Status`                     | span status     | trace result             |
|   [7]   | `Sampler`                    | sampling policy | trace admission          |
|   [8]   | `SamplingResult`             | sampling result | trace admission result   |
|   [9]   | `Metric`                     | metric payload  | metric export payload    |
|  [10]   | `MetricPoint`                | metric point    | metric timeseries point  |
|  [11]   | `MetricStreamConfiguration`  | metric stream   | metric stream policy     |
|  [12]   | `Exemplar`                   | exemplar value  | trace-linked measurement |
|  [13]   | `OpenTelemetryLoggerOptions` | logger options  | log capture policy       |
|  [14]   | `LogRecord`                  | log payload     | log export payload       |
|  [15]   | `LogRecordData`              | log data        | log emission payload     |
|  [16]   | `LogRecordAttributeList`     | log attributes  | log attribute payload    |

[PUBLIC_TYPE_SCOPE]: context and propagation family
- rail: telemetry

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]       | [RAIL]                  |
| :-----: | :--------------------------- | :------------------ | :---------------------- |
|   [1]   | `Baggage`                    | baggage value       | cross-boundary metadata |
|   [2]   | `PropagationContext`         | propagation value   | trace/baggage carrier   |
|   [3]   | `TextMapPropagator`          | propagator base     | carrier inject/extract  |
|   [4]   | `TraceContextPropagator`     | propagator          | W3C trace context       |
|   [5]   | `BaggagePropagator`          | propagator          | W3C baggage             |
|   [6]   | `CompositeTextMapPropagator` | propagator chain    | combined propagation    |
|   [7]   | `Propagators`                | propagator registry | default propagator      |
|   [8]   | `RuntimeContext`             | context registry    | ambient runtime context |
|   [9]   | `RuntimeContextSlot<T>`      | context slot        | typed ambient slot      |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider builder operations
- rail: telemetry

| [INDEX] | [SURFACE]                         | [ENTRY_FAMILY]        | [RAIL]                  |
| :-----: | :-------------------------------- | :-------------------- | :---------------------- |
|   [1]   | `Sdk.CreateTracerProviderBuilder` | SDK factory           | trace builder creation  |
|   [2]   | `Sdk.CreateMeterProviderBuilder`  | SDK factory           | metric builder creation |
|   [3]   | `Sdk.CreateLoggerProviderBuilder` | SDK factory           | log builder creation    |
|   [4]   | `AddSource`                       | trace source setup    | trace source admission  |
|   [5]   | `AddLegacySource`                 | trace source setup    | legacy source admission |
|   [6]   | `AddMeter`                        | meter setup           | meter admission         |
|   [7]   | `AddInstrumentation<T>`           | instrumentation setup | instrumentation hook    |
|   [8]   | `SetResourceBuilder`              | resource setup        | resource replacement    |
|   [9]   | `ConfigureResource`               | resource setup        | resource augmentation   |
|  [10]   | `AddProcessor`                    | processor setup       | processor admission     |
|  [11]   | `AddReader`                       | reader setup          | metric reader admission |
|  [12]   | `AddView`                         | metric view setup     | stream shaping          |
|  [13]   | `SetExemplarFilter`               | metric exemplar setup | exemplar policy         |
|  [14]   | `Build`                           | provider factory      | provider construction   |

[ENTRYPOINT_SCOPE]: signal operations
- rail: telemetry

| [INDEX] | [SURFACE]                    | [ENTRY_FAMILY]    | [RAIL]                  |
| :-----: | :--------------------------- | :---------------- | :---------------------- |
|   [1]   | `GetTracer`                  | trace access      | tracer lookup           |
|   [2]   | `StartSpan`                  | span creation     | inactive span           |
|   [3]   | `StartActiveSpan`            | span creation     | ambient active span     |
|   [4]   | `SetAttribute`               | span mutation     | span attribute          |
|   [5]   | `AddEvent`                   | span mutation     | span event              |
|   [6]   | `AddLink`                    | span mutation     | linked span context     |
|   [7]   | `SetStatus`                  | span mutation     | status projection       |
|   [8]   | `RecordException`            | span/log mutation | exception projection    |
|   [9]   | `EmitLog`                    | log emission      | log record creation     |
|  [10]   | `LogRecordAttributeList.Add` | log attributes    | log attribute admission |
|  [11]   | `MetricReader.Collect`       | metric collection | metric snapshot         |
|  [12]   | `MetricReader.ForceFlush`    | metric drain      | reader flush            |

[ENTRYPOINT_SCOPE]: processor exporter and propagation operations
- rail: telemetry

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY]     | [RAIL]              |
| :-----: | :------------------------------------- | :----------------- | :------------------ |
|   [1]   | `BaseProcessor.OnStart`                | processor callback | signal start        |
|   [2]   | `BaseProcessor.OnEnd`                  | processor callback | signal end          |
|   [3]   | `BaseExporter.Export`                  | exporter callback  | batch export        |
|   [4]   | `ForceFlush`                           | drain operation    | bounded flush       |
|   [5]   | `Shutdown`                             | drain operation    | bounded shutdown    |
|   [6]   | `TextMapPropagator.Inject`             | propagation write  | outbound carrier    |
|   [7]   | `TextMapPropagator.Extract`            | propagation read   | inbound carrier     |
|   [8]   | `Propagators.DefaultTextMapPropagator` | default propagator | process propagation |
|   [9]   | `Baggage.SetBaggage`                   | baggage mutation   | baggage write       |
|  [10]   | `RuntimeContext.RegisterSlot`          | context setup      | typed context slot  |

## [4]-[IMPLEMENTATION_LAW]

[TELEMETRY_TOPOLOGY]:
- namespaces: `OpenTelemetry`, `OpenTelemetry.Trace`, `OpenTelemetry.Metrics`, `OpenTelemetry.Logs`, `OpenTelemetry.Resources`
- context namespaces: `OpenTelemetry.Context`, `OpenTelemetry.Context.Propagation`
- signal rails: traces, metrics, logs
- provider rails: tracer provider, meter provider, logger provider
- resource rail: resource builder, resource detectors, service attributes
- processor rails: simple processor, batch processor, composite processor
- exporter rail: exporter, export result, batch payload, export processor options
- reader contract: metric readers own collection cadence and export cadence
- propagation rail: trace context, baggage, composite propagators
- sampling rail: always-on, always-off, parent-based, trace-id-ratio sampling

[LOCAL_ADMISSION]:
- Runtime code emits signals through provider builders and processor chains.
- Force-flush and shutdown are drain actions tied to unload receipts.
- Resource identity is required before provider construction.
- Context propagation crosses process and sidecar boundaries through explicit propagators.
- Projection failures never mutate runtime state directly.

[RAIL_LAW]:
- Package: `OpenTelemetry`
- Owns: trace and metric provider construction
- Accept: signals project through providers
- Reject: exporter packages inside lower runtime logic
