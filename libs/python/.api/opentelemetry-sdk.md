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

[PUBLIC_TYPE_SCOPE]: SDK instrument families — the preference-map key axis, each deriving from its API namesake

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [CAPABILITY]                           |
| :-----: | :------------------------------------ | :------------ | :------------------------------------- |
|  [01]   | `sdk.metrics.Counter`                 | sync family   | monotonic sum preference key           |
|  [02]   | `sdk.metrics.UpDownCounter`           | sync family   | non-monotonic sum preference key       |
|  [03]   | `sdk.metrics.Histogram`               | sync family   | distribution preference key            |
|  [04]   | `sdk.metrics._Gauge`                  | sync family   | synchronous last-value preference key  |
|  [05]   | `sdk.metrics.ObservableCounter`       | async family  | async monotonic sum preference key     |
|  [06]   | `sdk.metrics.ObservableUpDownCounter` | async family  | async non-monotonic sum preference key |
|  [07]   | `sdk.metrics.ObservableGauge`         | async family  | async last-value preference key        |

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

| [INDEX] | [SYMBOL]                                     | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :------------------------------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `sdk._logs.LoggerProvider`                   | provider      | SDK logger provider implementation                  |
|  [02]   | `sdk._logs.Logger`                           | logger        | SDK logger implementation                           |
|  [03]   | `sdk._logs.LoggingHandler`                   | bridge        | DEPRECATED stdlib `logging.Handler` -> OTel         |
|  [04]   | `sdk._logs.LogRecordLimits`                  | config        | log-record caps; `LogLimits` is its dead alias      |
|  [05]   | `sdk._logs.ReadableLogRecord`                | log view      | immutable log record for exporters                  |
|  [06]   | `sdk._logs.ReadWriteLogRecord`               | log record    | mutable in-pipeline log record                      |
|  [07]   | `sdk._logs.LogRecordProcessor`               | abstract      | log-record pipeline hook                            |
|  [08]   | `sdk._logs.export.BatchLogRecordProcessor`   | processor     | async batching log processor                        |
|  [09]   | `sdk._logs.export.SimpleLogRecordProcessor`  | processor     | synchronous one-by-one log processor                |
|  [10]   | `sdk._logs.export.LogRecordExporter`         | abstract      | exporter contract; `LogExporter` is its alias       |
|  [11]   | `sdk._logs.export.LogRecordExportResult`     | enum          | `SUCCESS`, `FAILURE`; `LogExportResult` alias       |
|  [12]   | `sdk._logs.export.ConsoleLogRecordExporter`  | exporter      | stdout dev exporter; `ConsoleLogExporter` alias     |
|  [13]   | `sdk._logs.export.InMemoryLogRecordExporter` | exporter      | capture for assertions; `InMemoryLogExporter` alias |
|  [14]   | `sdk.resources.Resource`                     | value         | service identity key-value labels                   |
|  [15]   | `sdk.resources.ResourceDetector`             | abstract      | resource-detection contract                         |
|  [16]   | `sdk.resources.OTELResourceDetector`         | detector      | `OTEL_RESOURCE_ATTRIBUTES`/`OTEL_SERVICE_NAME`      |
|  [17]   | `sdk.resources.ProcessResourceDetector`      | detector      | process pid/runtime/command resource                |
|  [18]   | `sdk.resources.OsResourceDetector`           | detector      | OS type/version resource                            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: TracerProvider construction and lifecycle
- `TracerProvider(sampler=None, resource=None, shutdown_on_exit=True, active_span_processor=None, id_generator=None, span_limits=None, *, meter_provider=None)` — provider constructor
- `ParentBased(root, remote_parent_sampled=ALWAYS_ON, remote_parent_not_sampled=ALWAYS_OFF, local_parent_sampled=ALWAYS_ON, local_parent_not_sampled=ALWAYS_OFF)` — parent-state routing sampler
- `SpanLimits(max_attributes, max_events, max_links, max_span_attributes, max_event_attributes, max_link_attributes, max_attribute_length, max_span_attribute_length)` — per-span/event/link count and value-length caps
- `BatchSpanProcessor(span_exporter, max_queue_size=None, schedule_delay_millis=None, max_export_batch_size=None, export_timeout_millis=None, *, meter_provider=None)` — four burst knobs ride it beside the self-observability meter

| [INDEX] | [SURFACE]                                                                         | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :-------------------------------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `TracerProvider(...)`                                                             | ctor     | provider with sampler/resource/limits     |
|  [02]   | `TracerProvider.add_span_processor(span_processor)`                               | instance | attach a span processor (fans into multi) |
|  [03]   | `TracerProvider.get_tracer(name, version=None, schema_url=None, attributes=None)` | factory  | scope-coordinate tracer mint              |
|  [04]   | `TracerProvider.force_flush(timeout_millis=30000) -> bool`                        | instance | flush all processors                      |
|  [05]   | `TracerProvider.shutdown()`                                                       | instance | flush + shut down all processors          |
|  [06]   | `BatchSpanProcessor(span_exporter, ...)`                                          | ctor     | batching processor with capacity config   |
|  [07]   | `SimpleSpanProcessor(span_exporter)`                                              | ctor     | synchronous single-span processor         |
|  [08]   | `TraceIdRatioBased(rate)`                                                         | ctor     | sampler for given fraction of traces      |
|  [09]   | `ParentBased(root, ...)`                                                          | ctor     | parent-decision-routed sampler            |
|  [10]   | `SpanLimits(...)`                                                                 | ctor     | per-span/event/link caps                  |

[ENTRYPOINT_SCOPE]: MeterProvider construction and lifecycle
- `MeterProvider(metric_readers=(), resource=None, exemplar_filter=None, shutdown_on_exit=True, views=())` — provider constructor
- `PeriodicExportingMetricReader(exporter, export_interval_millis=None, export_timeout_millis=None)` — push-reader constructor, three parameters whole
- `MetricReader(preferred_temporality=None, preferred_aggregation=None, *, otel_component_type=None)` — this base constructor owns both preference maps, each keyed on the seven `sdk.metrics` instrument FAMILY classes alone
- `View(instrument_type=None, instrument_name=None, meter_name=None, meter_version=None, meter_schema_url=None, name=None, description=None, attribute_keys=None, aggregation=None, exemplar_reservoir_factory=None, instrument_unit=None)` — eleven fields match and shape one instrument set

| [INDEX] | [SURFACE]                                                                                | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :--------------------------------------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `MeterProvider(...)`                                                                     | ctor     | provider with readers/views/filter |
|  [02]   | `MeterProvider.get_meter(name, version=None, schema_url=None, attributes=None)`          | factory  | scope-coordinate meter mint        |
|  [03]   | `MeterProvider.force_flush(timeout_millis=10_000) -> bool`                               | instance | collect + export all readers       |
|  [04]   | `MeterProvider.shutdown(timeout_millis=30_000)`                                          | instance | flush + stop all readers           |
|  [05]   | `PeriodicExportingMetricReader(exporter, ...)`                                           | ctor     | push reader on timer (default 60s) |
|  [06]   | `InMemoryMetricReader(preferred_temporality=None, preferred_aggregation=None)`           | ctor     | pull reader; `.get_metrics_data()` |
|  [07]   | `MetricReader.collect(timeout_millis=10_000)`                                            | instance | drive one collection cycle         |
|  [08]   | `View(...)`                                                                              | ctor     | instrument filter + aggregation    |
|  [09]   | `ExplicitBucketHistogramAggregation(boundaries=None, record_min_max=True)`               | ctor     | fixed-bucket histogram             |
|  [10]   | `ExponentialBucketHistogramAggregation(max_size=160, max_scale=20, record_min_max=True)` | ctor     | base-2 exponential histogram       |
|  [11]   | `SimpleFixedSizeExemplarReservoir(size=1, **kwargs)`                                     | ctor     | fixed-size random reservoir        |
|  [12]   | `AlignedHistogramBucketExemplarReservoir(boundaries, **kwargs)`                          | ctor     | one exemplar per bucket            |

[ENTRYPOINT_SCOPE]: LoggerProvider construction and lifecycle
- `LoggerProvider(resource=None, shutdown_on_exit=True, multi_log_record_processor=None, *, meter_provider=None)` — provider constructor
- `LogRecordLimits(max_attributes=None, max_attribute_length=None, max_log_record_attributes=None, max_log_record_attribute_length=None)` — four log-record caps mirror `SpanLimits` on the log leg, each resolving env then default and the record-specific column falling back to its global twin; `LogLimits` subclasses it under a `@deprecated` marker and is slated for removal
- `BatchLogRecordProcessor(exporter, schedule_delay_millis=None, max_export_batch_size=None, export_timeout_millis=None, max_queue_size=None, *, meter_provider=None)` — burst knobs run in log-side parameter order, which differs from `BatchSpanProcessor`'s

| [INDEX] | [SURFACE]                                                       | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :-------------------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `LoggerProvider(...)`                                           | ctor     | SDK logger provider with resource          |
|  [02]   | `LoggerProvider.add_log_record_processor(log_record_processor)` | instance | attach a log-record processor              |
|  [03]   | `LoggerProvider.force_flush(timeout_millis=30000) -> bool`      | instance | flush all log processors                   |
|  [04]   | `LoggerProvider.shutdown()`                                     | instance | flush + shut down all log processors       |
|  [05]   | `BatchLogRecordProcessor(exporter, ...)`                        | ctor     | batching log-record processor              |
|  [06]   | `SimpleLogRecordProcessor(exporter, *, meter_provider=None)`    | ctor     | synchronous processor for the test rail    |
|  [07]   | `InMemoryLogRecordExporter()`                                   | ctor     | zero-argument capture exporter for tests   |
|  [08]   | `LogRecordLimits(...)`                                          | ctor     | log-record attribute count/length caps     |
|  [09]   | `LoggingHandler(level=NOTSET, logger_provider=None)`            | ctor     | DEPRECATED stdlib `logging.Handler` bridge |
|  [10]   | `InMemoryLogRecordExporter.get_finished_logs()`                 | instance | captured `ReadableLogRecord` tuple         |
|  [11]   | `InMemoryLogRecordExporter.clear()`                             | instance | drop the captured records between specs    |

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
- `InstrumentationScope` keys each provider's tracer and meter cache on `(name, version, schema_url, attributes)` and coerces a `None` schema url to `""`, leaving VERSION the one slot where `None` and `""` stay distinct values.
- `TracerProvider.get_tracer` normalizes an unset version to `""` before minting that scope and `MeterProvider.get_meter` does not, so the API module helper's `""` default and the provider method's `None` default mint TWO meters for one name — an unstamped metric scope splits its instruments across two exported scopes where an unstamped trace scope collapses to one.
- `BatchSpanProcessor`/`BatchLogRecordProcessor` own the production path: a background thread and bounded queue tuned by `max_queue_size`/`schedule_delay_millis`/`max_export_batch_size`/`export_timeout_millis`; `Simple*Processor` runs synchronously for tests.
- drain publishes NO typed refusal on any provider: `TracerProvider`/`LoggerProvider` delegate `force_flush`/`shutdown` to their multi-processor, whose `BatchProcessor._export` catches every exporter raise and logs it, so the flush answers `False` and the shutdown answers nothing — a composition narrowing a fence over these two legs has no class to name.
- `MeterProvider.force_flush` and `MeterProvider.shutdown` run the opposite way and raise a BARE `Exception` whose message concatenates each failed reader's `repr`, having caught the readers' own `MetricsTimeoutError` and deadline raises first — so the only drain refusal the SDK states is the one class a boundary fence may not narrow on, and a composition that bans a bare-`Exception` catch absorbs both legs in its own drain fold and re-raises a set it names.
- `Sampler` runs once at span start; `ParentBased` routes by parent state across `root`/`remote_parent_sampled`/`remote_parent_not_sampled`/`local_parent_sampled`/`local_parent_not_sampled`, `TraceIdRatioBased(rate)` is the probabilistic head sampler, `ALWAYS_ON`/`ALWAYS_OFF`/`DEFAULT_ON`/`DEFAULT_OFF` are pre-built singletons.
- `View` + `Aggregation` set metric output shape: a `View` matches instruments by type/name/meter/unit and routes them to an `Aggregation`, attribute-key filter, and `exemplar_reservoir_factory`; `DropAggregation` mutes an instrument, `ExponentialBucketHistogramAggregation` is the dense base-2 histogram, unmatched instruments fall to `DefaultAggregation`.
- `View` matching is one-to-many, so EVERY matching view mints its own stream for the instrument: a wildcard row beside a name-exact row over the same instrument exports that instrument twice under one name. `instrument_name` accepts `fnmatch` wildcards and pairs with `name` only when it carries none — the constructor raises otherwise — and construction with no matching criterion raises as well; `attribute_keys` is an allow-list, so a key outside it drops before the stream is identified.
- `views` and `metric_readers` are construction-only on `MeterProvider`; neither admits a later addition, so every view a composition needs lands in the constructor call.
- `preferred_temporality`/`preferred_aggregation` keys match by IDENTITY against the seven `sdk.metrics` families and every other class raises `Exception("Invalid instrument class found ...")` inside `MetricReader.__init__` — so an `opentelemetry.metrics` namesake, the API BASE each family derives from, kills the reader at construction rather than degrading, and the raise surfaces at the reader even though the map was handed to the exporter. `sdk.metrics._Gauge` names the SDK synchronous-gauge family while `opentelemetry.metrics._Gauge` names the API base: one spelling, two classes, one admissible as a key.
- OTLP metric exporters seed a full family table before merging those maps over it, so a partial map leaves every unpinned family to the deployment environment.
- `OTEL_EXPORTER_OTLP_METRICS_TEMPORALITY_PREFERENCE` selects `CUMULATIVE` absent the variable, `DELTA`, or `LOWMEMORY` across that seed.
- `OTEL_EXPORTER_OTLP_METRICS_DEFAULT_HISTOGRAM_AGGREGATION` selects `explicit_bucket_histogram` absent the variable or `base2_exponential_bucket_histogram`.
- No seed table rows the synchronous gauge, so a last-value stream exports as a temporality-free Gauge point.
- `MeterProvider(exemplar_filter=...)` selects `AlwaysOn`/`AlwaysOff`/`TraceBased`; the per-view reservoir (`SimpleFixedSizeExemplarReservoir`, `AlignedHistogramBucketExemplarReservoir`) captures representative measurements with trace context for metric-to-trace linking.
- `exemplar_reservoir_factory` receives the stream's PRIVATE `_Aggregation` subclass, not the public `Aggregation` a view declares, so a caller branching on that argument names an internal type; each aggregation then re-wraps the returned builder — explicit-bucket supplies `boundaries=`, exponential supplies `size=min(20, max_bucket_count)`, sum and last-value supply nothing. Caller-pinned `size` therefore governs sum and last-value streams alone, `AlignedHistogramBucketExemplarReservoir` needs no caller-supplied boundaries, and omitting the factory selects the SDK default: aligned for explicit-bucket, single-slot fixed-size elsewhere.
- `View(aggregation=None)` resolves `DefaultAggregation`, which DEFERS to the reader's `preferred_aggregation` map keyed by instrument class — so a per-instrument view set never overrides an exporter's temporality or aggregation preference, while a view naming an aggregation outright silently wins over it.
- collected metrics serialize through the `MetricsData -> ResourceMetrics -> ScopeMetrics -> Metric -> (Sum|Gauge|Histogram|ExponentialHistogram) -> *DataPoint` tree the OTLP exporter consumes directly.
- `Resource.create()` runs the built-in detectors and merges `OTEL_SERVICE_NAME`/`OTEL_RESOURCE_ATTRIBUTES`, ordering the env detector last so env attributes win the merge.
- `LoggingHandler` bridges stdlib `logging` records into OTel `LogRecord`s, honoring any stdlib `Formatter` and `extra` attributes, and is DEPRECATED — construction warns and names `opentelemetry-instrumentation-logging` as its successor. Its body falls to `record.getMessage()` whenever a `Formatter` is attached-free, and its attributes are `vars(record)` minus a reserved key set, widened by injected `code.*`/`exception.*`, so it can carry only what a stdlib record already holds and never a projection built before the render.
- Logs tier runs mid-rename to `*LogRecord*` spellings and every old name survives only as a `@deprecated` alias slated for removal — `LogExporter`, `LogExportResult`, `ConsoleLogExporter`, `InMemoryLogExporter`, and `LogLimits` each warn at use — while `LoggingHandler` is deprecated outright with no in-repo successor, so a composition spells the `LogRecord` name and never the alias.
- `LogRecordLimits` reaches the record through `ReadWriteLogRecord.limits`, whose default factory constructs a fresh instance per record from env, and neither `LoggerProvider` nor `Logger.emit` nor `ReadWriteLogRecord._from_api_log_record` carries a limits argument — so a composition sets log-record caps through `OTEL_ATTRIBUTE_COUNT_LIMIT`/`OTEL_ATTRIBUTE_VALUE_LENGTH_LIMIT`/`OTEL_LOGRECORD_ATTRIBUTE_COUNT_LIMIT`/`OTEL_LOGRECORD_ATTRIBUTE_VALUE_LENGTH_LIMIT` alone, or bounds its own payload before emitting.
- `ReadWriteLogRecord` rebuilds its attributes as `BoundedAttributes(..., extended_attributes=True)`, so the log leg runs the `AnyValue` cleaner while spans run the flat primitive one — `None`, `bytes`, and nested mapping and sequence values reach the wire intact and `dropped_attributes` counts only what the count cap cut. That cleaner truncates `str` alone, so nesting depth, collection width, and byte length are unbounded at this tier: a producer emitting caller-shaped structure carries those three bounds itself or a self-referential mapping recurses until the interpreter's frame limit raises. Mixed-type sequences are the silent loss beside them — the cleaner types a sequence off its first non-null element and nulls the WHOLE value at the first element of another type, and `dropped_attributes` never counts it — while the count cap evicts OLDEST-first, so insertion order decides what survives.
- `Logger.emit(exception=...)` derives the whole semconv triple — `exception.type` MODULE-QUALIFIED for a non-builtin, `exception.message`, `exception.stacktrace` — through `_get_attributes_with_exception`, merging under any attribute a producer already set; a producer whose chain consumed the live exception before the emit stamps the pair it can still reach and takes the unqualified type name that costs.

[STACKING]:
- `opentelemetry-api`(`.api/opentelemetry-api.md`): SDK providers implement the API's abstract `TracerProvider`/`MeterProvider`/`LoggerProvider` and register through `trace.set_tracer_provider(...)` at startup; instrumentation binds the no-op API surface, so a live SDK is a composition-root swap invisible to library code.
- `opentelemetry-exporter-otlp-proto-http`(`.api/opentelemetry-exporter-otlp-proto-http.md`): its `OTLPSpanExporter`/`OTLPMetricExporter`/`OTLPLogExporter` are the terminal sink wired into `BatchSpanProcessor`/`PeriodicExportingMetricReader`/`BatchLogRecordProcessor`; SDK processors own batching/sampling/resource, the exporter owns transport, and the wire temporality and aggregation preferences ride the METRIC EXPORTER constructor — `PeriodicExportingMetricReader` accepts neither, so a composition passing them to the reader raises `TypeError` at construction.
- `psutil`(`.api/psutil.md`): a process-health gauge or observable counter fed by `psutil.Process(...).memory_info()`/`cpu_percent()` registers through the API `Meter` and takes shape from an SDK `View`; SDK aggregation is the only place a raw psutil reading becomes a temporality-correct metric point.
- `rasm.runtime` diagnostic read: `InMemoryMetricReader` mounts BESIDE the exporting reader on one `MeterProvider` — each reader owns independent aggregation storage, so the diagnostic one drains nothing the exporting one owes — and `get_metrics_data()` answers `MetricsData | None`, whose `to_json(indent=None)` is the shipped projection a support archive decodes back to a mapping so redaction reaches every depth.
- within-lib test rail: `InMemorySpanExporter`/`InMemoryMetricReader`/`InMemoryLogRecordExporter` capture `ReadableSpan`/`MetricsData`/`ReadableLogRecord` for assertion without a live collector; the log exporter reaches an already-registered `LoggerProvider` through `add_log_record_processor(SimpleLogRecordProcessor(...))`, so a spec captures without minting a second provider against the set-once global.

[LOCAL_ADMISSION]:
- SDK providers construct at the composition root only; instrumentation and library code bind the no-op API surface and never import `opentelemetry.sdk`.
- providers and `Batch*Processor` require `shutdown()` on exit (`shutdown_on_exit=True` is the default); short-lived processes `force_flush()` before exit.
- `PeriodicExportingMetricReader` defaults to a 60_000 ms interval; tune via `export_interval_millis` or `OTEL_METRIC_EXPORT_INTERVAL`.
- Temporality and aggregation preference homes at the constructing surface that owns the wire: an exporting composition sets both on the OTLP metric exporter, and a reader-side preference exists only on `MetricReader` subclasses that carry no exporter. `InMemoryMetricReader()` constructed bare seeds CUMULATIVE for every instrument class, so a backend-free read passes no preference map: under the DELTA wire pin that construction makes a second read a total, never the sliver since the last.
- Preference maps state every family whose wire shape the composition rules, since an unstated one falls to the exporter's environment seed; both maps spell their keys from `sdk.metrics`, never from `opentelemetry.metrics`.
- pass explicit `service.name` via `Resource.create({SERVICE_NAME: ...})` at startup; an unset name degrades to `unknown_service`.

[RAIL_LAW]:
- Package: `opentelemetry-sdk`
- Owns: concrete provider implementations, batch/simple processors, samplers, id generators, metric readers, view/aggregation/exemplar machinery, resource detection, and in-memory/console exporters
- Accept: one SDK provider per signal at the composition root, `Resource.create()` with `SERVICE_NAME`, `Batch*Processor` + `PeriodicExportingMetricReader` for production, `View`/`Aggregation` for metric shaping, in-memory exporters for tests
- Reject: SDK imports in library code, metric readers added after `MeterProvider` construction, `Simple*Processor` in production, missing `shutdown()`/`force_flush()` on exit, hand-built `MetricsData` trees, temporality or aggregation preferences passed to `PeriodicExportingMetricReader` or keyed on `opentelemetry.metrics` instrument classes, the deprecated `Log*` aliases and the `LoggingHandler` stdlib bridge
