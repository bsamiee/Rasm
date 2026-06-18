# [PY_BRANCH_API_OPENTELEMETRY_SDK]

`opentelemetry-sdk` supplies the concrete `TracerProvider`, `MeterProvider`, `LoggerProvider`, and their supporting processor, exporter, sampler, and resource types that replace the no-op API implementations at application startup. It owns the in-process pipeline from span/metric/log creation through batching to the exporter boundary, and the `Resource` model that labels all telemetry signals with service identity attributes.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-sdk`
- package: `opentelemetry-sdk`
- module: `opentelemetry.sdk`
- asset: runtime library
- rail: observability
- namespaces: `opentelemetry.sdk.trace`, `opentelemetry.sdk.trace.export`, `opentelemetry.sdk.trace.sampling`, `opentelemetry.sdk.metrics`, `opentelemetry.sdk.metrics.export`, `opentelemetry.sdk._logs`, `opentelemetry.sdk._logs.export`, `opentelemetry.sdk.resources`

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: trace SDK family
- rail: observability

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY] | [RAIL]                             |
| :-----: | :---------------------------------------- | :------------ | :--------------------------------- |
|   [1]   | `sdk.trace.TracerProvider`                | provider      | SDK tracer provider implementation |
|   [2]   | `sdk.trace.Tracer`                        | tracer        | SDK tracer implementation          |
|   [3]   | `sdk.trace.Span`                          | span          | SDK mutable span implementation    |
|   [4]   | `sdk.trace.ReadableSpan`                  | span view     | immutable span for exporters       |
|   [5]   | `sdk.trace.SpanProcessor`                 | abstract      | span lifecycle hook contract       |
|   [6]   | `sdk.trace.SpanLimits`                    | config        | attribute/event/link count caps    |
|   [7]   | `sdk.trace.SynchronousMultiSpanProcessor` | processor     | sequential multi-processor         |
|   [8]   | `sdk.trace.ConcurrentMultiSpanProcessor`  | processor     | concurrent multi-processor         |
|   [9]   | `sdk.trace.RandomIdGenerator`             | id generator  | cryptographic random id source     |
|  [10]   | `sdk.trace.Event`                         | value         | timestamped span event record      |

[PUBLIC_TYPE_SCOPE]: trace export and sampling family
- rail: observability

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY] | [RAIL]                                     |
| :-----: | :------------------------------------- | :------------ | :----------------------------------------- |
|   [1]   | `sdk.trace.export.BatchSpanProcessor`  | processor     | async batching span processor              |
|   [2]   | `sdk.trace.export.SimpleSpanProcessor` | processor     | synchronous one-by-one processor           |
|   [3]   | `sdk.trace.export.SpanExporter`        | abstract      | exporter contract for spans                |
|   [4]   | `sdk.trace.export.SpanExportResult`    | enum          | `SUCCESS`, `FAILURE`                       |
|   [5]   | `sdk.trace.export.ConsoleSpanExporter` | exporter      | stdout span exporter for dev               |
|   [6]   | `sdk.trace.sampling.Sampler`           | abstract      | sampling decision contract                 |
|   [7]   | `sdk.trace.sampling.SamplingResult`    | value         | decision with attributes/trace state       |
|   [8]   | `sdk.trace.sampling.TraceIdRatioBased` | sampler       | probabilistic ratio sampler                |
|   [9]   | `sdk.trace.sampling.ParentBased`       | sampler       | parent-decision-aware sampler              |
|  [10]   | `sdk.trace.sampling.StaticSampler`     | sampler       | always-on/always-off sampler               |
|  [11]   | `sdk.trace.sampling.Decision`          | enum          | `DROP`, `RECORD_ONLY`, `RECORD_AND_SAMPLE` |

[PUBLIC_TYPE_SCOPE]: metrics SDK family
- rail: observability

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY] | [RAIL]                                |
| :-----: | :------------------------------------------------- | :------------ | :------------------------------------ |
|   [1]   | `sdk.metrics.MeterProvider`                        | provider      | SDK meter provider implementation     |
|   [2]   | `sdk.metrics.Meter`                                | meter         | SDK meter implementation              |
|   [3]   | `sdk.metrics.export.MetricReader`                  | abstract      | metric collection contract            |
|   [4]   | `sdk.metrics.export.MetricExporter`                | abstract      | metric exporter contract              |
|   [5]   | `sdk.metrics.export.MetricExportResult`            | enum          | `SUCCESS`, `FAILURE`                  |
|   [6]   | `sdk.metrics.export.PeriodicExportingMetricReader` | reader        | periodic push reader                  |
|   [7]   | `sdk.metrics.export.InMemoryMetricReader`          | reader        | in-memory reader for testing          |
|   [8]   | `sdk.metrics.export.ConsoleMetricExporter`         | exporter      | stdout metric exporter for dev        |
|   [9]   | `sdk.metrics.export.AggregationTemporality`        | enum          | `CUMULATIVE`, `DELTA`                 |
|  [10]   | `sdk.metrics.view.View`                            | config        | instrument-to-aggregation mapping     |
|  [11]   | `sdk.metrics.Exemplar`                             | value         | representative sampled measurement    |
|  [12]   | `sdk.metrics.ExemplarFilter`                       | abstract      | exemplar inclusion policy             |
|  [13]   | `sdk.metrics.TraceBasedExemplarFilter`             | filter        | include exemplar when span is sampled |
|  [14]   | `sdk.metrics.AlwaysOnExemplarFilter`               | filter        | include all exemplars                 |
|  [15]   | `sdk.metrics.AlwaysOffExemplarFilter`              | filter        | exclude all exemplars                 |

[PUBLIC_TYPE_SCOPE]: logs SDK and resource family
- rail: observability

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY] | [RAIL]                             |
| :-----: | :------------------------------------------ | :------------ | :--------------------------------- |
|   [1]   | `sdk._logs.LoggerProvider`                  | provider      | SDK logger provider implementation |
|   [2]   | `sdk._logs.Logger`                          | logger        | SDK logger implementation          |
|   [3]   | `sdk._logs.ReadableLogRecord`               | log view      | immutable log record for exporters |
|   [4]   | `sdk._logs.LogRecordProcessor`              | abstract      | log record pipeline hook           |
|   [5]   | `sdk._logs.LoggingHandler`                  | bridge        | stdlib `logging` → OTel log bridge |
|   [6]   | `sdk._logs.export.BatchLogRecordProcessor`  | processor     | async batching log processor       |
|   [7]   | `sdk._logs.export.LogRecordExporter`        | abstract      | exporter contract for log records  |
|   [8]   | `sdk._logs.export.LogRecordExportResult`    | enum          | `SUCCESS`, `FAILURE`               |
|   [9]   | `sdk._logs.export.ConsoleLogRecordExporter` | exporter      | stdout log exporter for dev        |
|  [10]   | `sdk.resources.Resource`                    | value         | service identity key-value labels  |
|  [11]   | `sdk.resources.OTELResourceDetector`        | detector      | env-var resource auto-detection    |
|  [12]   | `sdk.resources.ProcessResourceDetector`     | detector      | process resource auto-detection    |
|  [13]   | `sdk.resources.OsResourceDetector`          | detector      | OS resource auto-detection         |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: TracerProvider construction and lifecycle
- rail: observability

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY] | [RAIL]                                  |
| :-----: | :------------------------------------------------------- | :------------- | :-------------------------------------- |
|   [1]   | `TracerProvider(sampler, resource, span_limits, ...)`    | construction   | provider with full config               |
|   [2]   | `TracerProvider.add_span_processor(span_processor)`      | config         | attach additional span processor        |
|   [3]   | `TracerProvider.shutdown()`                              | lifecycle      | flush and shut down all processors      |
|   [4]   | `BatchSpanProcessor(span_exporter, max_queue_size, ...)` | construction   | batching processor with capacity config |
|   [5]   | `SimpleSpanProcessor(span_exporter)`                     | construction   | synchronous single-span processor       |
|   [6]   | `TraceIdRatioBased(rate)`                                | construction   | sampler for given fraction of traces    |
|   [7]   | `ParentBased(root, remote_sampled_parent, ...)`          | construction   | parent-decision-routed sampler          |
|   [8]   | `SpanLimits(max_attributes, max_events, max_links, ...)` | construction   | per-span attribute and event caps       |

[ENTRYPOINT_SCOPE]: MeterProvider construction and lifecycle
- rail: observability

| [INDEX] | [SURFACE]                                                                  | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :------------------------------------------------------------------------- | :------------- | :------------------------------ |
|   [1]   | `MeterProvider(metric_readers, resource, views, ...)`                      | construction   | provider with readers and views |
|   [2]   | `MeterProvider.shutdown()`                                                 | lifecycle      | flush and stop all readers      |
|   [3]   | `PeriodicExportingMetricReader(exporter, export_interval_millis)`          | construction   | push reader on timer            |
|   [4]   | `View(instrument_type, instrument_name, aggregation, attribute_keys, ...)` | construction   | instrument filter and routing   |

[ENTRYPOINT_SCOPE]: Resource construction
- rail: observability

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY] | [RAIL]                             |
| :-----: | :--------------------------------- | :------------- | :--------------------------------- |
|   [1]   | `Resource(attributes, schema_url)` | construction   | explicit service identity resource |
|   [2]   | `Resource.create(attributes)`      | construction   | resource merged with defaults      |
|   [3]   | `Resource.merge(other)`            | merge          | combine two resources              |
|   [4]   | `OTELResourceDetector().detect()`  | detection      | read `OTEL_RESOURCE_ATTRIBUTES`    |

## [4]-[IMPLEMENTATION_LAW]

[SDK_TOPOLOGY]:
- provider composition: `TracerProvider` and `MeterProvider` accept `resource` and processor/reader lists at construction; processors and readers cannot be added after provider is registered globally
- `BatchSpanProcessor` is the production path: it holds an async background thread and bounded queue; `SimpleSpanProcessor` is for testing
- sampler is evaluated once per span start; `ParentBased` delegates to `root` for root spans and to `remote_sampled_parent`/`remote_not_sampled_parent` for remote parents
- `Resource.create()` merges detected resources with `OTEL_SERVICE_NAME`; always pass explicit `service.name` via `Resource` at startup
- `LoggingHandler` bridges Python stdlib `logging` into OTel log records; install once as a `logging.Handler` on the root logger
- `View` routes instruments matching `instrument_type`/`instrument_name` to a specific aggregation and attribute key filter; unmatched instruments use default aggregation

[LOCAL_ADMISSION]:
- SDK providers are constructed at the composition root only; instrumentation libraries use API calls.
- `BatchSpanProcessor` requires explicit `shutdown()` or `TracerProvider.shutdown()` on process exit.
- `PeriodicExportingMetricReader` interval is tuned per deployment; default is 60 000 ms.

[RAIL_LAW]:
- Package: `opentelemetry-sdk`
- Owns: concrete provider implementations, batch processors, samplers, resource detection, and in-memory/console exporters
- Accept: SDK provider at composition root, `Resource.create()` with `service.name`, `BatchSpanProcessor` + `PeriodicExportingMetricReader` for production, `LoggingHandler` stdlib bridge
- Reject: SDK imports in library code, provider construction after global registration, `SimpleSpanProcessor` in production, missing `shutdown()` on exit
