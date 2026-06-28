# [PY_BRANCH_API_OPENTELEMETRY_SDK]

`opentelemetry-sdk` supplies the concrete `TracerProvider`, `MeterProvider`, and `LoggerProvider` plus their processor, exporter, reader, sampler, aggregation, exemplar, view, and resource types that replace the no-op API implementations at application startup. It owns the in-process pipeline from span/metric/log creation through batching and aggregation to the exporter boundary, the `Resource` model that labels every signal with service identity, and the `View`/`Aggregation`/`ExemplarReservoir` machinery that shapes metric output before export. The dense composition is one provider per signal, each fed configured processors/readers and a shared `Resource`, with the OTLP exporter as terminal sink.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-sdk`
- package: `opentelemetry-sdk`
- version: `1.43.0`
- license: `Apache-2.0`
- module: `opentelemetry.sdk`
- asset: runtime library
- rail: observability
- namespaces: `opentelemetry.sdk.trace`, `...trace.export`, `...trace.sampling`, `...trace.id_generator`, `opentelemetry.sdk.metrics`, `...metrics.export`, `...metrics.view`, `opentelemetry.sdk._logs`, `...logs.export`, `opentelemetry.sdk.resources`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: trace SDK family
- rail: observability

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY] | [RAIL]                                     |
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
- rail: observability

| [INDEX] | [SYMBOL]                                                                     | [TYPE_FAMILY] | [RAIL]                                                                |
| :-----: | :--------------------------------------------------------------------------- | :------------ | :-------------------------------------------------------------------- |
|  [01]   | `sdk.trace.export.BatchSpanProcessor`                                        | processor     | async batching span processor (bg thread)                             |
|  [02]   | `sdk.trace.export.SimpleSpanProcessor`                                       | processor     | synchronous one-by-one processor (test)                               |
|  [03]   | `sdk.trace.export.SpanExporter`                                              | abstract      | exporter contract for spans                                           |
|  [04]   | `sdk.trace.export.SpanExportResult`                                          | enum          | `SUCCESS`, `FAILURE`                                                  |
|  [05]   | `sdk.trace.export.ConsoleSpanExporter`                                       | exporter      | stdout span exporter for dev                                          |
|  [06]   | `sdk.trace.export.InMemorySpanExporter`                                      | exporter      | captures spans for assertions (in `..export.in_memory_span_exporter`) |
|  [07]   | `sdk.trace.sampling.Sampler`                                                 | abstract      | sampling decision contract                                            |
|  [08]   | `sdk.trace.sampling.SamplingResult`                                          | value         | decision + attributes + trace state                                   |
|  [09]   | `sdk.trace.sampling.TraceIdRatioBased`                                       | sampler       | probabilistic ratio sampler                                           |
|  [10]   | `sdk.trace.sampling.ParentBased`                                             | sampler       | parent-decision-routed sampler                                        |
|  [11]   | `sdk.trace.sampling.StaticSampler`                                           | sampler       | always-on/always-off sampler                                          |
|  [12]   | `sdk.trace.sampling.Decision`                                                | enum          | `DROP`, `RECORD_ONLY`, `RECORD_AND_SAMPLE`                            |
|  [13]   | `sdk.trace.sampling.ALWAYS_ON` / `ALWAYS_OFF` / `DEFAULT_ON` / `DEFAULT_OFF` | const sampler | pre-built sampler singletons                                          |

[PUBLIC_TYPE_SCOPE]: metrics SDK family
- rail: observability

| [INDEX] | [SYMBOL]                                              | [TYPE_FAMILY] | [RAIL]                                |
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
- rail: observability

| [INDEX] | [SYMBOL]                                                                                      | [TYPE_FAMILY] | [RAIL]                               |
| :-----: | :-------------------------------------------------------------------------------------------- | :------------ | :----------------------------------- |
|  [01]   | `sdk.metrics.export.MetricReader`                                                             | abstract      | metric collection contract           |
|  [02]   | `sdk.metrics.export.MetricExporter`                                                           | abstract      | metric exporter contract             |
|  [03]   | `sdk.metrics.export.MetricExportResult`                                                       | enum          | `SUCCESS`, `FAILURE`                 |
|  [04]   | `sdk.metrics.export.PeriodicExportingMetricReader`                                            | reader        | timer-driven push reader             |
|  [05]   | `sdk.metrics.export.InMemoryMetricReader`                                                     | reader        | in-memory reader for testing         |
|  [06]   | `sdk.metrics.export.ConsoleMetricExporter`                                                    | exporter      | stdout metric exporter for dev       |
|  [07]   | `sdk.metrics.export.AggregationTemporality`                                                   | enum          | `CUMULATIVE`, `DELTA`, `UNSPECIFIED` |
|  [08]   | `sdk.metrics.export.MetricsData` / `ResourceMetrics` / `ScopeMetrics` / `Metric`              | data tree     | hierarchical export payload          |
|  [09]   | `sdk.metrics.export.Sum` / `Gauge` / `Histogram` / `ExponentialHistogram`                     | point-kind    | aggregated metric point bodies       |
|  [10]   | `sdk.metrics.export.NumberDataPoint` / `HistogramDataPoint` / `ExponentialHistogramDataPoint` | data point    | per-attribute-set aggregated value   |
|  [11]   | `sdk.metrics.export.DataPointT` / `DataT`                                                     | type alias    | data-point / point-kind unions       |
|  [12]   | `sdk.metrics.view.View`                                                                       | config        | instrument-to-aggregation mapping    |
|  [13]   | `sdk.metrics.view.Aggregation`                                                                | abstract      | aggregation strategy contract        |
|  [14]   | `sdk.metrics.view.DefaultAggregation`                                                         | aggregation   | per-instrument default strategy      |
|  [15]   | `sdk.metrics.view.DropAggregation`                                                            | aggregation   | discard instrument output            |
|  [16]   | `sdk.metrics.view.SumAggregation` / `LastValueAggregation`                                    | aggregation   | sum / last-value strategies          |
|  [17]   | `sdk.metrics.view.ExplicitBucketHistogramAggregation`                                         | aggregation   | fixed-bucket histogram               |
|  [18]   | `sdk.metrics.view.ExponentialBucketHistogramAggregation`                                      | aggregation   | base-2 exponential histogram         |

[PUBLIC_TYPE_SCOPE]: logs SDK and resource family
- rail: observability

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY] | [RAIL]                                                                                                   |
| :-----: | :------------------------------------------ | :------------ | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `sdk._logs.LoggerProvider`                  | provider      | SDK logger provider implementation                                                                       |
|  [02]   | `sdk._logs.Logger`                          | logger        | SDK logger implementation                                                                                |
|  [03]   | `sdk._logs.LoggingHandler`                  | bridge        | stdlib `logging.Handler` -> OTel log bridge (deprecated; prefer `opentelemetry-instrumentation-logging`) |
|  [04]   | `sdk._logs.LogLimits`                       | config        | log-record attribute count/length caps                                                                   |
|  [05]   | `sdk._logs.ReadableLogRecord`               | log view      | immutable log record for exporters                                                                       |
|  [06]   | `sdk._logs.ReadWriteLogRecord`              | log record    | mutable in-pipeline log record                                                                           |
|  [07]   | `sdk._logs.LogRecordProcessor`              | abstract      | log-record pipeline hook                                                                                 |
|  [08]   | `sdk._logs.export.BatchLogRecordProcessor`  | processor     | async batching log processor                                                                             |
|  [09]   | `sdk._logs.export.SimpleLogRecordProcessor` | processor     | synchronous one-by-one log processor                                                                     |
|  [10]   | `sdk._logs.export.LogExporter`              | abstract      | exporter contract for log records                                                                        |
|  [11]   | `sdk._logs.export.LogExportResult`          | enum          | `SUCCESS`, `FAILURE`                                                                                     |
|  [12]   | `sdk._logs.export.ConsoleLogExporter`       | exporter      | stdout log exporter for dev                                                                              |
|  [13]   | `sdk._logs.export.InMemoryLogExporter`      | exporter      | captures log records for assertions                                                                      |
|  [14]   | `sdk.resources.Resource`                    | value         | service identity key-value labels                                                                        |
|  [15]   | `sdk.resources.ResourceDetector`            | abstract      | resource-detection contract                                                                              |
|  [16]   | `sdk.resources.OTELResourceDetector`        | detector      | `OTEL_RESOURCE_ATTRIBUTES`/`OTEL_SERVICE_NAME`                                                           |
|  [17]   | `sdk.resources.ProcessResourceDetector`     | detector      | process pid/runtime/command resource                                                                     |
|  [18]   | `sdk.resources.OsResourceDetector`          | detector      | OS type/version resource                                                                                 |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: TracerProvider construction and lifecycle
- rail: observability

| [INDEX] | [SURFACE]                                                                                                                                                                                                    | [ENTRY_FAMILY] | [RAIL]                                    |
| :-----: | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `TracerProvider(sampler=None, resource=Resource.create(), shutdown_on_exit=True, active_span_processor=None, id_generator=None, span_limits=None)`                                                           | construction   | provider with sampler/resource/limits     |
|  [02]   | `TracerProvider.add_span_processor(span_processor)`                                                                                                                                                          | config         | attach a span processor (fans into multi) |
|  [03]   | `TracerProvider.get_tracer(instrumenting_module_name, ...)`                                                                                                                                                  | factory        | obtain a `Tracer`                         |
|  [04]   | `TracerProvider.force_flush(timeout_millis=30000) -> bool`                                                                                                                                                   | flush          | flush all processors                      |
|  [05]   | `TracerProvider.shutdown()`                                                                                                                                                                                  | lifecycle      | flush + shut down all processors          |
|  [06]   | `BatchSpanProcessor(span_exporter, max_queue_size=None, schedule_delay_millis=None, max_export_batch_size=None, export_timeout_millis=None)`                                                                 | construction   | batching processor with capacity config   |
|  [07]   | `SimpleSpanProcessor(span_exporter)`                                                                                                                                                                         | construction   | synchronous single-span processor         |
|  [08]   | `TraceIdRatioBased(rate)`                                                                                                                                                                                    | construction   | sampler for given fraction of traces      |
|  [09]   | `ParentBased(root, remote_parent_sampled=ALWAYS_ON, remote_parent_not_sampled=ALWAYS_OFF, local_parent_sampled=ALWAYS_ON, local_parent_not_sampled=ALWAYS_OFF)`                                              | construction   | parent-decision-routed sampler            |
|  [10]   | `SpanLimits(max_attributes=None, max_events=None, max_links=None, max_span_attributes=None, max_event_attributes=None, max_link_attributes=None, max_attribute_length=None, max_span_attribute_length=None)` | construction   | per-span/event/link caps                  |

[ENTRYPOINT_SCOPE]: MeterProvider construction and lifecycle
- rail: observability

| [INDEX] | [SURFACE]                                                                                                                                                                                      | [ENTRY_FAMILY] | [RAIL]                                     |
| :-----: | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `MeterProvider(metric_readers=(), resource=None, exemplar_filter=None, shutdown_on_exit=True, views=())`                                                                                       | construction   | provider with readers/views/filter         |
|  [02]   | `MeterProvider.force_flush(timeout_millis=10_000) -> bool`                                                                                                                                     | flush          | collect + export all readers               |
|  [03]   | `MeterProvider.shutdown(timeout_millis=30_000)`                                                                                                                                                | lifecycle      | flush + stop all readers                   |
|  [04]   | `PeriodicExportingMetricReader(exporter, export_interval_millis=None, export_timeout_millis=None, preferred_temporality=None, preferred_aggregation=None)`                                     | construction   | push reader on timer (default 60s)         |
|  [05]   | `InMemoryMetricReader(preferred_temporality=None, preferred_aggregation=None)`                                                                                                                 | construction   | pull reader exposing `.get_metrics_data()` |
|  [06]   | `View(instrument_type=None, instrument_name=None, meter_name=None, instrument_unit=None, name=None, description=None, attribute_keys=None, aggregation=None, exemplar_reservoir_factory=None)` | construction   | instrument filter + aggregation routing    |
|  [07]   | `ExplicitBucketHistogramAggregation(boundaries=None, record_min_max=True)`                                                                                                                     | construction   | fixed-bucket histogram strategy            |
|  [08]   | `ExponentialBucketHistogramAggregation(max_size=160, max_scale=20)`                                                                                                                            | construction   | exponential histogram strategy             |

[ENTRYPOINT_SCOPE]: LoggerProvider construction and lifecycle
- rail: observability

| [INDEX] | [SURFACE]                                                                                                                                    | [ENTRY_FAMILY] | [RAIL]                               |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------- | :------------- | :----------------------------------- |
|  [01]   | `LoggerProvider(resource=None, shutdown_on_exit=True, multi_log_record_processor=None)`                                                      | construction   | SDK logger provider with resource    |
|  [02]   | `LoggerProvider.add_log_record_processor(log_record_processor)`                                                                              | config         | attach a log-record processor        |
|  [03]   | `LoggerProvider.force_flush(timeout_millis=30000) -> bool`                                                                                   | flush          | flush all log processors             |
|  [04]   | `LoggerProvider.shutdown()`                                                                                                                  | lifecycle      | flush + shut down all log processors |
|  [05]   | `BatchLogRecordProcessor(exporter, max_queue_size=None, schedule_delay_millis=None, max_export_batch_size=None, export_timeout_millis=None)` | construction   | batching log-record processor        |
|  [06]   | `LoggingHandler(level=NOTSET, logger_provider=None)`                                                                                         | construction   | stdlib `logging.Handler` bridge      |

[ENTRYPOINT_SCOPE]: Resource construction and detection
- rail: observability

| [INDEX] | [SURFACE]                                                                                                  | [ENTRY_FAMILY] | [RAIL]                                 |
| :-----: | :--------------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------- |
|  [01]   | `Resource.create(attributes=None, schema_url=None)` (classmethod)                                          | construction   | resource merged with detected defaults |
|  [02]   | `Resource.get_empty()` (classmethod)                                                                       | construction   | empty resource                         |
|  [03]   | `Resource.merge(other) -> Resource`                                                                        | merge          | combine two resources (other wins)     |
|  [04]   | `Resource.attributes` / `Resource.schema_url`                                                              | accessor       | read labels and schema                 |
|  [05]   | `OTELResourceDetector().detect()` / `ProcessResourceDetector().detect()` / `OsResourceDetector().detect()` | detection      | per-source resource                    |
|  [06]   | `get_aggregated_resources(detectors, initial_resource=None, timeout=5)`                                    | detection      | merge a detector sequence              |
|  [07]   | `SERVICE_NAME` / `SERVICE_NAMESPACE` / `SERVICE_VERSION` / `SERVICE_INSTANCE_ID`                           | const key      | canonical resource attribute keys      |

## [04]-[IMPLEMENTATION_LAW]

[SDK_TOPOLOGY]:
- one provider per signal at the composition root, each constructed with a shared `Resource` and its processors/readers. `TracerProvider`/`LoggerProvider` accept processors at construction or via `add_*`; `MeterProvider` takes `metric_readers`/`views` at construction only (readers cannot be added later).
- `BatchSpanProcessor`/`BatchLogRecordProcessor` are the production path: each owns a background thread + bounded queue with `max_queue_size`/`schedule_delay_millis`/`max_export_batch_size`/`export_timeout_millis`. `Simple*Processor` is synchronous, for tests.
- sampler runs once at span start. `ParentBased` routes by parent state to `root`/`remote_parent_sampled`/`remote_parent_not_sampled`/`local_parent_sampled`/`local_parent_not_sampled`; `TraceIdRatioBased(rate)` is the probabilistic head sampler. `ALWAYS_ON`/`ALWAYS_OFF`/`DEFAULT_ON`/`DEFAULT_OFF` are pre-built singletons.
- metric output shape is set by `View` + `Aggregation`: a `View` matches instruments (by type/name/meter/unit) and routes them to a chosen `Aggregation`, attribute-key filter, and `exemplar_reservoir_factory`. `DropAggregation` mutes an instrument; `ExponentialBucketHistogramAggregation` is the dense base-2 histogram. Unmatched instruments use `DefaultAggregation`.
- exemplars: `MeterProvider(exemplar_filter=...)` selects `AlwaysOn`/`AlwaysOff`/`TraceBased`; the reservoir (`SimpleFixedSizeExemplarReservoir`, `AlignedHistogramBucketExemplarReservoir`) per view captures representative measurements with trace context for metric-to-trace exemplar linking.
- collected metrics serialize through the `MetricsData -> ResourceMetrics -> ScopeMetrics -> Metric -> (Sum|Gauge|Histogram|ExponentialHistogram) -> *DataPoint` tree; the OTLP exporter consumes this tree directly.
- `Resource.create()` runs the built-in detectors and merges `OTEL_SERVICE_NAME`/`OTEL_RESOURCE_ATTRIBUTES`; the `_build_resource_detectors` order puts `OTELResourceDetector` last so env attributes win the merge.
- `LoggingHandler(level=logging.NOTSET, logger_provider=None)` bridges stdlib `logging` into OTel `LogRecord`s; install once on the root logger with `logger_provider` bound. It is deprecated in this package — prefer the handler from `opentelemetry-instrumentation-logging` when that dependency is admitted.

[INTEGRATION_LAW]:
- Stack with `opentelemetry-exporter-otlp-proto-http`: the OTLP exporter is the terminal sink wired into `BatchSpanProcessor`/`PeriodicExportingMetricReader`/`BatchLogRecordProcessor`. The SDK owns batching/aggregation/sampling/resource; the exporter owns transport. The metric exporter's `preferred_temporality` and the reader's `preferred_temporality` must agree.
- Stack with `psutil`: a process-health gauge/observable-counter fed by `psutil.Process(...).memory_info()`/`cpu_percent()` registers through the API `Meter` and is shaped by an SDK `View`; the SDK aggregates and the OTLP exporter ships it. The SDK is the only place the raw psutil reading becomes a temporality-correct metric point.
- `InMemorySpanExporter`/`InMemoryMetricReader`/`InMemoryLogExporter` are the test seams: assert against captured `ReadableSpan`/`MetricsData`/`ReadableLogRecord` without a live collector — the verification rail for telemetry-shape laws.

[LOCAL_ADMISSION]:
- SDK providers are constructed at the composition root only; instrumentation/library code uses the no-op API surface and never imports `opentelemetry.sdk`.
- `Batch*Processor` and providers require `shutdown()` (or `shutdown_on_exit=True`, the default) on process exit; flush before exit in short-lived processes via `force_flush()`.
- `PeriodicExportingMetricReader` interval defaults to 60_000 ms; tune per deployment via `export_interval_millis` or `OTEL_METRIC_EXPORT_INTERVAL`.
- Always pass explicit `service.name` through `Resource.create({SERVICE_NAME: ...})` at startup; an unset service name degrades to `unknown_service`.

[RAIL_LAW]:
- Package: `opentelemetry-sdk`
- Owns: concrete provider implementations, batch/simple processors, samplers, id generators, metric readers, view/aggregation/exemplar machinery, resource detection, and in-memory/console exporters
- Accept: one SDK provider per signal at the composition root, `Resource.create()` with `SERVICE_NAME`, `Batch*Processor` + `PeriodicExportingMetricReader` for production, `View`/`Aggregation` for metric shaping, `LoggingHandler` stdlib bridge, in-memory exporters for tests
- Reject: SDK imports in library code, metric readers added after `MeterProvider` construction, `Simple*Processor` in production, missing `shutdown()`/`force_flush()` on exit, hand-built `MetricsData` trees
