# [PY_RUNTIME_API_OPENTELEMETRY_EXPORTER_OTLP_PROTO_GRPC]

`opentelemetry-exporter-otlp-proto-grpc` owns the OTLP/gRPC egress tail of the observability rail: `OTLPSpanExporter`, `OTLPMetricExporter`, and `OTLPLogExporter` each hold one persistent `grpc` channel to a `host:port` collector and reuse it across every export, sitting as the terminal sink behind an SDK processor. It is the daemon-selectable transport row, not the estate default — proto-http owns the default egress, and this gRPC row is selected only on a long-lived non-forking server where streaming throughput dominates.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-exporter-otlp-proto-grpc`
- package: `opentelemetry-exporter-otlp-proto-grpc` (`Apache-2.0`, OpenTelemetry Authors)
- module: `opentelemetry.exporter.otlp.proto.grpc`
- namespaces: `...grpc.trace_exporter`, `...grpc.metric_exporter`, `...grpc._log_exporter`, `...grpc.exporter`
- abi: pure-Python runtime library
- rail: observability

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: signal exporters over one persistent gRPC channel

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :------------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `OTLPSpanExporter`   | exporter      | OTLP/gRPC span batch export (`SpanExporter`, `trace_exporter`)     |
|  [02]   | `OTLPMetricExporter` | exporter      | OTLP/gRPC metric export (`MetricExporter`, `metric_exporter`)      |
|  [03]   | `OTLPLogExporter`    | exporter      | OTLP/gRPC log-record export (`LogRecordExporter`, `_log_exporter`) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: exporter construction
- shared ctor carry: `endpoint`, `insecure`, `credentials`, `headers`, `timeout`, `compression`, `channel_options`, `retryable_error_codes`, `*`, `meter_provider`
- metric adds: `preferred_temporality`, `preferred_aggregation`, `max_export_batch_size`

| [INDEX] | [SURFACE]                        | [SHAPE] | [CAPABILITY]                                         |
| :-----: | :------------------------------- | :------ | :--------------------------------------------------- |
|  [01]   | `OTLPSpanExporter(...shared)`    | ctor    | span exporter behind `BatchSpanProcessor`            |
|  [02]   | `OTLPMetricExporter(...+metric)` | ctor    | metric exporter behind a periodic reader             |
|  [03]   | `OTLPLogExporter(...shared)`     | ctor    | log-record exporter behind `BatchLogRecordProcessor` |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Each exporter holds one persistent `grpc` channel built once by `_initialize_channel_and_stub` and reused across every export; the channel reinitializes only on an `UNAVAILABLE` reconnect, and never survives `fork()`.
- `endpoint` is a grpc `host:port` netloc (default `localhost:4317`), never a `/v1/<signal>` path; `insecure=True` builds an `insecure_channel`, else `credentials` builds a `secure_channel`.
- `compression=` takes a `grpc.Compression`, `NoCompression` default, `Gzip`/`Deflate` selectable.

[STACKING]:
- `opentelemetry-sdk`(`.api/opentelemetry-sdk.md`): each exporter is the terminal sink behind one SDK processor — `OTLPSpanExporter` -> `BatchSpanProcessor` -> `TracerProvider`, `OTLPMetricExporter` -> `PeriodicExportingMetricReader` -> `MeterProvider`, `OTLPLogExporter` -> `BatchLogRecordProcessor` -> `LoggerProvider`; the processor owns batching and queueing, the exporter owns transport and reconnect.
- `grpcio`(`.api/grpcio.md`): the persistent channel is a `grpc.Channel` from `insecure_channel`/`secure_channel` over a `ChannelCredentials`, `compression=` a `grpc.Compression`, reconnect triggered by `grpc.StatusCode.UNAVAILABLE`.
- `protobuf`(`.api/protobuf.md`): SDK `ReadableSpan`/`MetricsData`/`LogData` encode to OTLP protobuf inside `_export`; the composing owner hands over SDK views and never hand-builds the protobuf tree.
- `opentelemetry-exporter-otlp-proto-http`(`.api/opentelemetry-exporter-otlp-proto-http.md`): the peer default egress this row substitutes for — proto-http carries the estate default, this gRPC row selects only on a long-lived non-forking server.

[LOCAL_ADMISSION]:
- One exporter per signal, built at the composition root and handed to the matching SDK processor through the telemetry install's exporter-factory seam.
- A forking-worker fabric (loky/pebble) stays on proto-http; the gRPC channel dies at `fork()`.

[RAIL_LAW]:
- Package: `opentelemetry-exporter-otlp-proto-grpc`
- Owns: OTLP span/metric/log egress over a persistent gRPC channel to a `host:port` collector target
- Accept: the daemon transport row on a long-lived non-forking server, selected through the telemetry install's exporter-factory seam
- Reject: the estate default (proto-http owns it), selection inside a forking-worker fabric, a library-level import of an exporter class
