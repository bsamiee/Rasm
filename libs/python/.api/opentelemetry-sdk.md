# [PY_BRANCH_API_OPENTELEMETRY_SDK]

`opentelemetry-sdk` owns the in-process telemetry pipeline: concrete signal providers replacing the no-op API surface at startup, the processor/reader/sampler machinery carrying signals from creation through batching and aggregation to the exporter boundary, the `Resource` labeling every signal with service identity, and the `View`/`Aggregation`/`ExemplarReservoir` shaping of metric output. One provider per signal composes at the root over a shared `Resource` and configured processors, the OTLP exporter its terminal sink.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-sdk`
- package: `opentelemetry-sdk` (Apache-2.0)
- module: `opentelemetry.sdk`
- namespaces: `opentelemetry.sdk.trace`, `...trace.export`, `...trace.sampling`, `...trace.id_generator`, `opentelemetry.sdk.metrics`, `...metrics.export`, `...metrics.view`, `opentelemetry.sdk._logs`, `...logs.export`, `opentelemetry.sdk.resources`
- asset: runtime library
- rail: observability

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: trace SDK family

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY] | [CAPABILITY]                               |
| :-----: | :----------------------------------------- | :------------ | :----------------------------------------- |
|  [01]   | `sdk.trace.TracerProvider`                 | provider      | SDK tracer provider implementation         |
|  [02]   | `sdk.trace.Tracer`                         | tracer        | SDK tracer implementation                  |
|  [03]   | `sdk.trace.Span`                           | span          | SDK mutable span (`trace_api.Span` + view) |
|  [04]   | `sdk.trace.ReadableSpan`                   | span view     | immutable span snapshot for exporters      |
|  [05]   | `sdk.trace.SpanProcessor`                  | abstract      | span lifecycle hook contract               |
|  [06]   | `sdk.trace.SpanLimits`                     | config        | attribute/event/link/length count caps     |
|  [07]   | `sdk.trace.SynchronousMultiSpanProcessor`  | processor     | sequential multi-processor fan-out         |
|  [08]   | `sdk.trace.ConcurrentMultiSpanProcessor`   | processor     | thread-pool multi-processor fan-out        |
|  [09]   | `sdk.trace.id_generator.IdGenerator`       | abstract      | trace/span id source contract              |
|  [10]   | `sdk.trace.id_generator.RandomIdGenerator` | id generator  | random 128/64-bit id source                |
|  [11]   | `sdk.trace.Event`                          | value         | timestamped span event record              |

[PUBLIC_TYPE_SCOPE]: trace export and sampling family

| [INDEX] | [SYMBOL]                                                               | [TYPE_FAMILY] | [CAPABILITY]                               |
| :-----: | :--------------------------------------------------------------------- | :------------ | :----------------------------------------- |
|  [01]   | `sdk.trace.export.BatchSpanProcessor`                                  | processor     | async batching span processor (bg thread)  |
|  [02]   | `sdk.trace.export.SimpleSpanProcessor`                                 | processor     | synchronous one-by-one processor (test)    |
|  [03]   | `sdk.trace.export.SpanExporter`                                        | abstract      | exporter contract for spans                |
|  [04]   | `sdk.trace.export.SpanExportResult`                                    | enum          | `SUCCESS`, `FAILURE`                       |
|  [05]   | `sdk.trace.export.ConsoleSpanExporter`                                 | exporter      | stdout span exporter for dev               |
|  [06]   | `sdk.trace.export.InMemorySpanExporter`                                | exporter      | captures spans for assertions              |
|  [07]   | `sdk.trace.sampling.Sampler`                                           | abstract      | sampling decision contract                 |
|  [08]   | `sdk.trace.sampling.SamplingResult`                                    | value         | decision + attributes + trace state        |
|  [09]   | `sdk.trace.sampling.TraceIdRatioBased`                                 | sampler       | probabilistic ratio sampler                |
|  [10]   | `sdk.trace.sampling.ParentBased`                                       | sampler       | parent-decision-routed sampler             |
|  [11]   | `sdk.trace.sampling.StaticSampler`                                     | sampler       | always-on/always-off sampler               |
|  [12]   | `sdk.trace.sampling.Decision`                                          | enum          | `DROP`, `RECORD_ONLY`, `RECORD_AND_SAMPLE` |
|  [13]   | `sdk.trace.sampling.ALWAYS_ON`/`ALWAYS_OFF`/`DEFAULT_ON`/`DEFAULT_OFF` | const sampler | pre-built sampler singletons               |

[PUBLIC_TYPE_SCOPE]: metrics SDK family

| [INDEX] | [SYMBOL]                                              | [TYPE_FAMILY] | [CAPABILITY]                          |
| :-----: | :---------------------------------------------------- | :------------ | :------------------------------------ |
|  [01]   | `sdk.metrics.MeterProvider`                           | provider      | SDK meter provider implementation     |
|  [02]   | `sdk.metrics.Meter`                                   | meter         | SDK meter implementation              |
|  [03]   | `sdk.metrics.MetricsTimeoutError`                     | exception     | collection/export deadline exceeded   |
|  [04]   | `sdk.metrics.Exemplar`                                | value         | representative sampled measurement    |
|  [05]   | `sdk.metrics.ExemplarFilter`                          | abstract      | exemplar inclusion policy             |
|  [06]   | `sdk.metrics.TraceBasedExemplarFilter`                | filter        | include exemplar when span is sampled |
|  [07]   | `sdk.metrics.AlwaysOnExemplarFilter`                  | filter        | include all exemplars                 |
|  [08]   | `sdk.metrics.AlwaysOffExemplarFilter`                 | filter        | exclude all exemplars                 |
|  [09]   | `sdk.metrics.ExemplarReservoir`                       | abstract      | exemplar storage strategy             |
|  [10]   | `sdk.metrics.SimpleFixedSizeExemplarReservoir`        | reservoir     | fixed-size random exemplar reservoir  |
|  [11]   | `sdk.metrics.AlignedHistogramBucketExemplarReservoir` | reservoir     | one exemplar per histogram bucket     |

[PUBLIC_TYPE_SCOPE]: metrics export, reader, view, and aggregation family

| [INDEX] | [SYMBOL]                                                                         | [TYPE_FAMILY] | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------------------------------- | :------------ | :------------------------------------ |
|  [01]   | `sdk.metrics.export.MetricReader`                                                | abstract      | metric collection contract            |
|  [02]   | `sdk.metrics.export.MetricExporter`                                              | abstract      | metric exporter contract              |
|  [03]   | `sdk.metrics.export.MetricExportResult`                                          | enum          | `SUCCESS`, `FAILURE`                  |
|  [04]   | `sdk.metrics.export.PeriodicExportingMetricReader`                               | reader        | timer-driven push reader              |
|  [05]   | `sdk.metrics.export.InMemoryMetricReader`                                        | reader        | in-memory reader for testing          |
|  [06]   | `sdk.metrics.export.ConsoleMetricExporter`                                       | exporter      | stdout metric exporter for dev        |
|  [07]   | `sdk.metrics.export.AggregationTemporality`                                      | enum          | `CUMULATIVE`, `DELTA`, `UNSPECIFIED`  |
|  [08]   | `sdk.metrics.export.MetricsData` / `ResourceMetrics` / `ScopeMetrics` / `Metric` | data tree     | hierarchical export payload           |
|  [09]   | `sdk.metrics.export.Sum` / `Gauge` / `Histogram` / `ExponentialHistogram`        | point-kind    | aggregated metric point bodies        |
|  [10]   | `sdk.metrics.export.NumberDataPoint`                                             | data point    | per-attribute-set number value        |
|  [11]   | `sdk.metrics.export.HistogramDataPoint`                                          | data point    | per-attribute-set histogram value     |
|  [12]   | `sdk.metrics.export.ExponentialHistogramDataPoint`                               | data point    | per-attribute-set exp-histogram value |
|  [13]   | `sdk.metrics.export.DataPointT` / `DataT`                                        | type alias    | data-point / point-kind unions        |
|  [14]   | `sdk.metrics.view.View`                                                          | config        | instrument-to-aggregation mapping     |
|  [15]   | `sdk.metrics.view.Aggregation`                                                   | abstract      | aggregation strategy contract         |
|  [16]   | `sdk.metrics.view.DefaultAggregation`                                            | aggregation   | per-instrument default strategy       |
|  [17]   | `sdk.metrics.view.DropAggregation`                                               | aggregation   | discard instrument output             |
|  [18]   | `sdk.metrics.view.SumAggregation` / `LastValueAggregation`                       | aggregation   | sum / last-value strategies           |
|  [19]   | `sdk.metrics.view.ExplicitBucketHistogramAggregation`                            | aggregation   | fixed-bucket histogram                |
|  [20]   | `sdk.metrics.view.ExponentialBucketHistogramAggregation`                         | aggregation   | base-2 exponential histogram          |

[PUBLIC_TYPE_SCOPE]: logs SDK and resource family

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :------------------------------------------ | :------------ | :--------------------------------------------- |
|  [01]   | `sdk._logs.LoggerProvider`                  | provider      | SDK logger provider implementation             |
|  [02]   | `sdk._logs.Logger`                          | logger        | SDK logger implementation                      |
|  [03]   | `sdk._logs.LoggingHandler`                  | bridge        | stdlib `logging.Handler` -> OTel bridge        |
|  [04]   | `sdk._logs.LogLimits`                       | config        | log-record attribute count/length caps         |
|  [05]   | `sdk._logs.ReadableLogRecord`               | log view      | immutable log record for exporters             |
|  [06]   | `sdk._logs.ReadWriteLogRecord`              | log record    | mutable in-pipeline log record                 |
|  [07]   | `sdk._logs.LogRecordProcessor`              | abstract      | log-record pipeline hook                       |
|  [08]   | `sdk._logs.export.BatchLogRecordProcessor`  | processor     | async batching log processor                   |
|  [09]   | `sdk._logs.export.SimpleLogRecordProcessor` | processor     | synchronous one-by-one log processor           |
|  [10]   | `sdk._logs.export.LogExporter`              | abstract      | exporter contract for log records              |
|  [11]   | `sdk._logs.export.LogExportResult`          | enum          | `SUCCESS`, `FAILURE`                           |
|  [12]   | `sdk._logs.export.ConsoleLogExporter`       | exporter      | stdout log exporter for dev                    |
|  [13]   | `sdk._logs.export.InMemoryLogExporter`      | exporter      | captures log records for assertions            |
|  [14]   | `sdk.resources.Resource`                    | value         | service identity key-value labels              |
|  [15]   | `sdk.resources.ResourceDetector`            | abstract      | resource-detection contract                    |
|  [16]   | `sdk.resources.OTELResourceDetector`        | detector      | `OTEL_RESOURCE_ATTRIBUTES`/`OTEL_SERVICE_NAME` |
|  [17]   | `sdk.resources.ProcessResourceDetector`     | detector      | process pid/runtime/command resource           |
|  [18]   | `sdk.resources.OsResourceDetector`          | detector      | OS type/version resource                       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: TracerProvider construction and lifecycle
- `TracerProvider(sampler=None, resource=Resource.create(), shutdown_on_exit=True, active_span_processor=None, id_generator=None, span_limits=None)` — provider constructor
- `ParentBased(root, remote_parent_sampled=ALWAYS_ON, remote_parent_not_sampled=ALWAYS_OFF, local_parent_sampled=ALWAYS_ON, local_parent_not_sampled=ALWAYS_OFF)` — parent-state routing sampler
- `SpanLimits(max_attributes, max_events, max_links, max_span_attributes, max_event_attributes, max_link_attributes, max_attribute_length, max_span_attribute_length)` — per-span/event/link count and value-length caps

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :---------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `TracerProvider(...)`                                       | ctor     | provider with sampler/resource/limits     |
|  [02]   | `TracerProvider.add_span_processor(span_processor)`         | instance | attach a span processor (fans into multi) |
|  [03]   | `TracerProvider.get_tracer(instrumenting_module_name, ...)` | factory  | obtain a `Tracer`                         |
|  [04]   | `TracerProvider.force_flush(timeout_millis=30000) -> bool`  | instance | flush all processors                      |
|  [05]   | `TracerProvider.shutdown()`                                 | instance | flush + shut down all processors          |
|  [06]   | `BatchSpanProcessor(span_exporter, ...)`                    | ctor     | batching processor with capacity config   |
|  [07]   | `SimpleSpanProcessor(span_exporter)`                        | ctor     | synchronous single-span processor         |
|  [08]   | `TraceIdRatioBased(rate)`                                   | ctor     | sampler for given fraction of traces      |
|  [09]   | `ParentBased(root, ...)`                                    | ctor     | parent-decision-routed sampler            |
|  [10]   | `SpanLimits(...)`                                           | ctor     | per-span/event/link caps                  |

[ENTRYPOINT_SCOPE]: MeterProvider construction and lifecycle
- `MeterProvider(metric_readers=(), resource=None, exemplar_filter=None, shutdown_on_exit=True, views=())` — provider constructor
- `PeriodicExportingMetricReader(exporter, export_interval_millis=None, export_timeout_millis=None, preferred_temporality=None, preferred_aggregation=None)` — push-reader constructor

| [INDEX] | [SURFACE]                                                                  | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `MeterProvider(...)`                                                       | ctor     | provider with readers/views/filter         |
|  [02]   | `MeterProvider.force_flush(timeout_millis=10_000) -> bool`                 | instance | collect + export all readers               |
|  [03]   | `MeterProvider.shutdown(timeout_millis=30_000)`                            | instance | flush + stop all readers                   |
|  [04]   | `PeriodicExportingMetricReader(exporter, ...)`                             | ctor     | push reader on timer (default 60s)         |
|  [05]   | `InMemoryMetricReader(preferred_temporality, preferred_aggregation)`       | ctor     | pull reader exposing `.get_metrics_data()` |
|  [06]   | `View(...)`                                                                | ctor     | instrument filter + aggregation routing    |
|  [07]   | `ExplicitBucketHistogramAggregation(boundaries=None, record_min_max=True)` | ctor     | fixed-bucket histogram strategy            |
|  [08]   | `ExponentialBucketHistogramAggregation(max_size=160, max_scale=20)`        | ctor     | exponential histogram strategy             |

[ENTRYPOINT_SCOPE]: LoggerProvider construction and lifecycle
- `LoggerProvider(resource=None, shutdown_on_exit=True, multi_log_record_processor=None)` — provider constructor

| [INDEX] | [SURFACE]                                                       | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :-------------------------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `LoggerProvider(...)`                                           | ctor     | SDK logger provider with resource    |
|  [02]   | `LoggerProvider.add_log_record_processor(log_record_processor)` | instance | attach a log-record processor        |
|  [03]   | `LoggerProvider.force_flush(timeout_millis=30000) -> bool`      | instance | flush all log processors             |
|  [04]   | `LoggerProvider.shutdown()`                                     | instance | flush + shut down all log processors |
|  [05]   | `BatchLogRecordProcessor(exporter, ...)`                        | ctor     | batching log-record processor        |
|  [06]   | `LoggingHandler(level=NOTSET, logger_provider=None)`            | ctor     | stdlib `logging.Handler` bridge      |

[ENTRYPOINT_SCOPE]: Resource construction and detection

| [INDEX] | [SURFACE]                                                                        | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :------------------------------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `Resource.create(attributes=None, schema_url=None)`                              | factory  | resource + detected defaults       |
|  [02]   | `Resource.get_empty()`                                                           | factory  | empty resource                     |
|  [03]   | `Resource.merge(other) -> Resource`                                              | instance | combine two resources (other wins) |
|  [04]   | `Resource.attributes` / `Resource.schema_url`                                    | property | read labels and schema             |
|  [05]   | `OTELResourceDetector().detect()`                                                | instance | env-attributes resource            |
|  [06]   | `ProcessResourceDetector().detect()`                                             | instance | process pid/runtime resource       |
|  [07]   | `OsResourceDetector().detect()`                                                  | instance | OS type/version resource           |
|  [08]   | `get_aggregated_resources(detectors, initial_resource=None, timeout=5)`          | static   | merge a detector sequence          |
|  [09]   | `SERVICE_NAME` / `SERVICE_NAMESPACE` / `SERVICE_VERSION` / `SERVICE_INSTANCE_ID` | const    | canonical resource attribute keys  |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one provider per signal at the composition root over a shared `Resource`; `TracerProvider`/`LoggerProvider` take processors at construction or via `add_*`, `MeterProvider` takes `metric_readers`/`views` at construction only — readers never added later.
- `BatchSpanProcessor`/`BatchLogRecordProcessor` own the production path: a background thread and bounded queue tuned by `max_queue_size`/`schedule_delay_millis`/`max_export_batch_size`/`export_timeout_millis`; `Simple*Processor` runs synchronously for tests.
- `Sampler` runs once at span start; `ParentBased` routes by parent state across `root`/`remote_parent_sampled`/`remote_parent_not_sampled`/`local_parent_sampled`/`local_parent_not_sampled`, `TraceIdRatioBased(rate)` is the probabilistic head sampler, `ALWAYS_ON`/`ALWAYS_OFF`/`DEFAULT_ON`/`DEFAULT_OFF` are pre-built singletons.
- `View` + `Aggregation` set metric output shape: a `View` matches instruments by type/name/meter/unit and routes them to an `Aggregation`, attribute-key filter, and `exemplar_reservoir_factory`; `DropAggregation` mutes an instrument, `ExponentialBucketHistogramAggregation` is the dense base-2 histogram, unmatched instruments fall to `DefaultAggregation`.
- `MeterProvider(exemplar_filter=...)` selects `AlwaysOn`/`AlwaysOff`/`TraceBased`; the per-view reservoir (`SimpleFixedSizeExemplarReservoir`, `AlignedHistogramBucketExemplarReservoir`) captures representative measurements with trace context for metric-to-trace linking.
- collected metrics serialize through the `MetricsData -> ResourceMetrics -> ScopeMetrics -> Metric -> (Sum|Gauge|Histogram|ExponentialHistogram) -> *DataPoint` tree the OTLP exporter consumes directly.
- `Resource.create()` runs the built-in detectors and merges `OTEL_SERVICE_NAME`/`OTEL_RESOURCE_ATTRIBUTES`, ordering the env detector last so env attributes win the merge.
- `LoggingHandler` bridges stdlib `logging` records into OTel `LogRecord`s, honoring any stdlib `Formatter` and `extra` attributes; install once on the root logger with `logger_provider` bound.

[STACKING]:
- `opentelemetry-api`(`.api/opentelemetry-api.md`): SDK providers implement the API's abstract `TracerProvider`/`MeterProvider`/`LoggerProvider` and register through `trace.set_tracer_provider(...)` at startup; instrumentation binds the no-op API surface, so a live SDK is a composition-root swap invisible to library code.
- `opentelemetry-exporter-otlp-proto-http`(`.api/opentelemetry-exporter-otlp-proto-http.md`): its `OTLPSpanExporter`/`OTLPMetricExporter`/`OTLPLogExporter` are the terminal sink wired into `BatchSpanProcessor`/`PeriodicExportingMetricReader`/`BatchLogRecordProcessor`; SDK processors own batching/aggregation/sampling/resource, the exporter owns transport, and reader `preferred_temporality` must match the exporter's.
- `psutil`(`.api/psutil.md`): a process-health gauge or observable counter fed by `psutil.Process(...).memory_info()`/`cpu_percent()` registers through the API `Meter` and takes shape from an SDK `View`; SDK aggregation is the only place a raw psutil reading becomes a temporality-correct metric point.
- within-lib test rail: `InMemorySpanExporter`/`InMemoryMetricReader`/`InMemoryLogExporter` capture `ReadableSpan`/`MetricsData`/`ReadableLogRecord` for assertion without a live collector.

[LOCAL_ADMISSION]:
- SDK providers construct at the composition root only; instrumentation and library code bind the no-op API surface and never import `opentelemetry.sdk`.
- providers and `Batch*Processor` require `shutdown()` on exit (`shutdown_on_exit=True` is the default); short-lived processes `force_flush()` before exit.
- `PeriodicExportingMetricReader` defaults to a 60_000 ms interval; tune via `export_interval_millis` or `OTEL_METRIC_EXPORT_INTERVAL`.
- pass explicit `service.name` via `Resource.create({SERVICE_NAME: ...})` at startup; an unset name degrades to `unknown_service`.

[RAIL_LAW]:
- Package: `opentelemetry-sdk`
- Owns: concrete provider implementations, batch/simple processors, samplers, id generators, metric readers, view/aggregation/exemplar machinery, resource detection, and in-memory/console exporters
- Accept: one SDK provider per signal at the composition root, `Resource.create()` with `SERVICE_NAME`, `Batch*Processor` + `PeriodicExportingMetricReader` for production, `View`/`Aggregation` for metric shaping, `LoggingHandler` stdlib bridge, in-memory exporters for tests
- Reject: SDK imports in library code, metric readers added after `MeterProvider` construction, `Simple*Processor` in production, missing `shutdown()`/`force_flush()` on exit, hand-built `MetricsData` trees
