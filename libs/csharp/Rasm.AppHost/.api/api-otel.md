# [RASM_APPHOST_API_OTEL]

`OpenTelemetry` supplies trace, metric, log, resource, processor, exporter, reader,
sampling, context, and propagation surfaces for telemetry projection.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry`
- package: `OpenTelemetry`
- assembly: `OpenTelemetry`
- api_assembly: `OpenTelemetry.Api`
- namespace: `OpenTelemetry`
- asset: runtime library
- rail: telemetry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: provider and resource family
- rail: telemetry

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]     | [RAIL]                |
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
- rail: telemetry

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]      | [RAIL]                 |
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
- rail: telemetry

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]   | [RAIL]                   |
| :-----: | :--------------------------- | :-------------- | :----------------------- |
|  [01]   | `Tracer`                     | trace emitter   | span creation            |
|  [02]   | `TelemetrySpan`              | span handle     | span mutation            |
|  [03]   | `SpanContext`                | trace context   | trace identity           |
|  [04]   | `SpanAttributes`             | attribute bag   | span attributes          |
|  [05]   | `Link`                       | span link       | causal link              |
|  [06]   | `Status`                     | span status     | trace result             |
|  [07]   | `Sampler`                    | sampling policy | trace admission base     |
|  [08]   | `SamplingResult`             | sampling result | trace admission result   |
|  [09]   | `Metric`                     | metric payload  | metric export payload    |
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
- rail: telemetry

| [INDEX] | [SURFACE]                         | [ENTRY_FAMILY]        | [RAIL]                  |
| :-----: | :-------------------------------- | :-------------------- | :---------------------- |
|  [01]   | `Sdk.CreateTracerProviderBuilder` | SDK factory           | trace builder creation  |
|  [02]   | `Sdk.CreateMeterProviderBuilder`  | SDK factory           | metric builder creation |
|  [03]   | `Sdk.CreateLoggerProviderBuilder` | SDK factory           | log builder creation    |
|  [04]   | `AddSource`                       | trace source setup    | trace source admission  |
|  [05]   | `AddLegacySource`                 | trace source setup    | legacy source admission |
|  [06]   | `AddMeter`                        | meter setup           | meter admission         |
|  [07]   | `AddInstrumentation<T>`           | instrumentation setup | instrumentation hook    |
|  [08]   | `SetResourceBuilder`              | resource setup        | resource replacement    |
|  [09]   | `ConfigureResource`               | resource setup        | resource augmentation   |
|  [10]   | `AddProcessor`                    | processor setup       | processor admission     |
|  [11]   | `AddReader`                       | reader setup          | metric reader admission |
|  [12]   | `AddView`                         | metric view setup     | stream shaping          |
|  [13]   | `SetExemplarFilter`               | metric exemplar setup | exemplar policy         |
|  [14]   | `SetSampler`                      | trace sampler setup   | sets the `Sampler` on `TracerProviderBuilder` (also `SetSampler<T>()` and the `Func<IServiceProvider, Sampler>` factory overload) |
|  [15]   | `Build`                           | provider factory      | provider construction   |

[ENTRYPOINT_SCOPE]: signal operations
- rail: telemetry

| [INDEX] | [SURFACE]                    | [ENTRY_FAMILY]    | [RAIL]                  |
| :-----: | :--------------------------- | :---------------- | :---------------------- |
|  [01]   | `GetTracer`                  | trace access      | tracer lookup           |
|  [02]   | `StartSpan`                  | span creation     | inactive span           |
|  [03]   | `StartActiveSpan`            | span creation     | ambient active span     |
|  [04]   | `SetAttribute`               | span mutation     | span attribute          |
|  [05]   | `AddEvent`                   | span mutation     | span event              |
|  [06]   | `AddLink`                    | span mutation     | linked span context     |
|  [07]   | `SetStatus`                  | span mutation     | status projection       |
|  [08]   | `RecordException`            | span/log mutation | exception projection    |
|  [09]   | `EmitLog`                    | log emission      | log record creation     |
|  [10]   | `LogRecordAttributeList.Add` | log attributes    | log attribute admission |
|  [11]   | `MetricReader.Collect`       | metric collection | metric snapshot         |
|  [12]   | `MetricReader.ForceFlush`    | metric drain      | reader flush            |

[ENTRYPOINT_SCOPE]: processor exporter and propagation operations
- rail: telemetry

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY]     | [RAIL]              |
| :-----: | :------------------------------------- | :----------------- | :------------------ |
|  [01]   | `BaseProcessor.OnStart`                | processor callback | signal start        |
|  [02]   | `BaseProcessor.OnEnd`                  | processor callback | signal end          |
|  [03]   | `BaseExporter.Export`                  | exporter callback  | batch export        |
|  [04]   | `ForceFlush`                           | drain operation    | bounded flush       |
|  [05]   | `Shutdown`                             | drain operation    | bounded shutdown    |
|  [06]   | `TextMapPropagator.Inject`             | propagation write  | outbound carrier    |
|  [07]   | `TextMapPropagator.Extract`            | propagation read   | inbound carrier     |
|  [08]   | `Propagators.DefaultTextMapPropagator` | default propagator | process propagation |
|  [09]   | `Baggage.SetBaggage`                   | baggage mutation   | baggage write       |
|  [10]   | `RuntimeContext.RegisterSlot`          | context setup      | typed context slot  |
|  [11]   | `Baggage.Current`                      | baggage ambient    | get/set the ambient `Baggage` (with `GetBaggage`); seats inbound baggage after `Extract` |

## [04]-[IMPLEMENTATION_LAW]

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
- sampling rail: `AlwaysOnSampler`, `AlwaysOffSampler`, `ParentBasedSampler(Sampler root[, remote/local sampled/notSampled])`, `TraceIdRatioBasedSampler(double probability)`, set through `SetSampler` on the tracer-provider builder

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
