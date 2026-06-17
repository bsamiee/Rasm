# [PY_RUNTIME_API_OPENTELEMETRY_SDK]

`opentelemetry-sdk` supplies the concrete telemetry providers: tracer/meter/logger providers, span processors and exporters, resource detection and attributes, samplers, span limits, and id generators. The runtime catalogues the SDK so the host composition root assembles it; the runtime emits against the API and never instantiates a provider itself.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-sdk`
- package: `opentelemetry-sdk`
- import: `opentelemetry.sdk`
- version: `1.42.1`
- owner: `runtime`
- rail: observability
- namespaces: `opentelemetry.sdk.trace`, `opentelemetry.sdk.trace.export`, `opentelemetry.sdk.metrics`, `opentelemetry.sdk.metrics.export`, `opentelemetry.sdk._logs`, `opentelemetry.sdk._logs.export`, `opentelemetry.sdk.resources`
- capability: concrete providers, span processors/exporters, resource detection, samplers, span limits, id generators

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: provider and processor family
- rail: observability

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [RAIL]                       |
| :-----: | :------------------------------------ | :------------ | :--------------------------- |
|   [1]   | `trace.TracerProvider`                | provider      | concrete tracer provider     |
|   [2]   | `trace.Tracer`                        | tracer        | concrete tracer              |
|   [3]   | `trace.Span`                          | span          | concrete recording span      |
|   [4]   | `trace.ReadableSpan`                  | span          | exported span view           |
|   [5]   | `trace.SpanProcessor`                 | processor     | span lifecycle hook          |
|   [6]   | `trace.SynchronousMultiSpanProcessor` | processor     | fan-out span processor       |
|   [7]   | `trace.ConcurrentMultiSpanProcessor`  | processor     | concurrent fan-out processor |
|   [8]   | `trace.SpanLimits`                    | limits        | per-span cardinality caps    |
|   [9]   | `trace.sampling.Sampler`              | sampler       | sampling-decision contract   |
|  [10]   | `trace.id_generator.IdGenerator`      | id source     | trace/span id generator      |

[PUBLIC_TYPE_SCOPE]: exporter family
- rail: observability

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY] | [RAIL]                         |
| :-----: | :--------------------------------------------- | :------------ | :----------------------------- |
|   [1]   | `trace.export.SpanExporter`                    | exporter      | span export contract           |
|   [2]   | `trace.export.BatchSpanProcessor`              | processor     | batching span exporter driver  |
|   [3]   | `trace.export.SimpleSpanProcessor`             | processor     | immediate span exporter driver |
|   [4]   | `trace.export.ConsoleSpanExporter`             | exporter      | stdout span exporter           |
|   [5]   | `metrics.export.PeriodicExportingMetricReader` | reader        | periodic metric reader         |
|   [6]   | `metrics.export.MetricExporter`                | exporter      | metric export contract         |
|   [7]   | `_logs.export.BatchLogRecordProcessor`         | processor     | batching log exporter driver   |
|   [8]   | `_logs.export.LogExporter`                     | exporter      | log export contract            |

[PUBLIC_TYPE_SCOPE]: resource family
- rail: observability

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [RAIL]                         |
| :-----: | :---------------------------------- | :------------ | :----------------------------- |
|   [1]   | `resources.Resource`                | resource      | service identity attributes    |
|   [2]   | `resources.ResourceDetector`        | detector      | environment-attribute detector |
|   [3]   | `resources.OTELResourceDetector`    | detector      | env-var resource detector      |
|   [4]   | `resources.ProcessResourceDetector` | detector      | process-attribute detector     |
|   [5]   | `_logs.LoggerProvider`              | provider      | concrete logger provider       |
|   [6]   | `metrics.MeterProvider`             | provider      | concrete meter provider        |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: composition operations
- rail: observability

| [INDEX] | [SURFACE]                            | [ENTRY_FAMILY] | [RAIL]                      |
| :-----: | :----------------------------------- | :------------- | :-------------------------- |
|   [1]   | `TracerProvider(resource=...)`       | build          | construct a tracer provider |
|   [2]   | `TracerProvider.add_span_processor`  | wire           | attach a span processor     |
|   [3]   | `BatchSpanProcessor(exporter)`       | wire           | batch spans to an exporter  |
|   [4]   | `MeterProvider(metric_readers=...)`  | build          | construct a meter provider  |
|   [5]   | `LoggerProvider(resource=...)`       | build          | construct a logger provider |
|   [6]   | `Resource.create`                    | build          | resource from attribute map |
|   [7]   | `resources.get_aggregated_resources` | detect         | merge detected resources    |
|   [8]   | `TracerProvider.shutdown`            | drain          | flush and stop processors   |

## [4]-[IMPLEMENTATION_LAW]

[OBSERVABILITY_TOPOLOGY]:
- composition law: provider construction, processor wiring, resource detection, and sampler choice are a host concern — the SDK is assembled once at `Rasm.AppHost`, and the runtime emits against the API.
- batching law: production export uses `BatchSpanProcessor`/`BatchLogRecordProcessor` with the exporter from `.api/api-opentelemetry-exporter-otlp-proto-http.md`; `SimpleSpanProcessor` is reserved for tests.
- resource law: service identity is one `Resource.create` carrying `service.name`/`service.version`/`service.instance.id` aggregated with `OTELResourceDetector` and `ProcessResourceDetector`.
- drain law: shutdown flushes every processor; the drain receipt records flush completion.

[LOCAL_ADMISSION]:
- This catalogue documents the SDK surface the host composes; the runtime planning boundary forbids the runtime from owning provider instantiation, exporter configuration, or a product telemetry pipeline.
- The runtime references SDK types only in transcription notes that name the host as the composer.

[RAIL_LAW]:
- Package: `opentelemetry-sdk`
- Owns: concrete provider/processor/exporter/resource construction for the host composition root
- Accept: host-composed providers, batch processors, aggregated resources, explicit shutdown drains
- Reject: provider instantiation inside the runtime package, `SimpleSpanProcessor` in production, ad hoc resource attributes
