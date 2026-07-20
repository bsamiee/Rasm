# [PY_RUNTIME_API_OPENTELEMETRY_EXPORTER_OTLP_PROTO_GRPC]

`opentelemetry-exporter-otlp-proto-grpc` supplies the OTLP/gRPC egress: three signal exporters — `OTLPSpanExporter`, `OTLPMetricExporter`, `OTLPLogExporter` — each holding one persistent `grpc` channel to a `host:port` collector target and reusing it across every export. It is the daemon-selectable transport row, not the estate default: proto-http owns the default egress, and a persistent channel earns selection only where streaming throughput dominates and the process never forks.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-exporter-otlp-proto-grpc`
- package: `opentelemetry-exporter-otlp-proto-grpc`
- module: `opentelemetry.exporter.otlp.proto.grpc`
- owner: `runtime`
- rail: observability
- asset: pure-Python runtime library
- namespaces: `opentelemetry.exporter.otlp.proto.grpc`
- capability: OTLP span/metric/log export over a persistent gRPC channel to a `host:port` target, with `grpc.Compression` selection and an `UNAVAILABLE`-triggered channel reconnect

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: signal exporters
- rail: observability
- import paths: `opentelemetry.exporter.otlp.proto.grpc.trace_exporter.OTLPSpanExporter`, `opentelemetry.exporter.otlp.proto.grpc.metric_exporter.OTLPMetricExporter`, `opentelemetry.exporter.otlp.proto.grpc._log_exporter.OTLPLogExporter`.
- compression: `compression=` takes `grpc.Compression` — `Gzip` or the `NoCompression` default.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [RAIL]                                         |
| :-----: | :------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `OTLPSpanExporter`   | exporter      | `trace_exporter`, `SpanExporter` over gRPC     |
|  [02]   | `OTLPMetricExporter` | exporter      | `metric_exporter`, `MetricExporter` over gRPC  |
|  [03]   | `OTLPLogExporter`    | exporter      | `_log_exporter`, `LogRecordExporter` over gRPC |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: exporter construction
- rail: observability
- shared ctor params: `endpoint`, `insecure`, `credentials`, `headers`, `timeout`, `compression`, with keyword-only `channel_options`, `retryable_error_codes`, `meter_provider`.
- `endpoint` is a grpc target `host:port` — default `http://localhost:4317`, scheme parsed for `insecure` inference, `netloc` taken as the target; no `/v1/<signal>` path.
- `credentials` is a `grpc.ChannelCredentials`; `insecure=True` builds an `insecure_channel`, otherwise a `secure_channel` carries server credentials.
- `OTLPMetricExporter` adds `preferred_temporality`, `preferred_aggregation`, `max_export_batch_size`.

| [INDEX] | [SURFACE]                                                                          | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :--------------------------------------------------------------------------------- | :------------- | :------------------------------- |
|  [01]   | `OTLPSpanExporter(endpoint, insecure, credentials, headers, timeout, compression)` | traces         | `BatchSpanProcessor` export      |
|  [02]   | `OTLPMetricExporter(..., preferred_temporality, max_export_batch_size)`            | metrics        | periodic-reader export           |
|  [03]   | `OTLPLogExporter(endpoint, insecure, credentials, headers, timeout, compression)`  | logs           | `BatchLogRecordProcessor` export |

## [04]-[IMPLEMENTATION_LAW]

[OBSERVABILITY_TOPOLOGY]:
- default law: proto-http stays the sole default estate egress; this gRPC exporter is the daemon-selectable transport row only — a long-lived non-forking server process where a persistent channel and streaming throughput dominate.
- fork law: a gRPC channel does not survive `fork()`; any process forking workers (loky/pebble fabric) never selects this row and stays on proto-http.
- channel law: each exporter holds one persistent `grpc` channel built once by `_initialize_channel_and_stub` and reused across every export; the channel reinitializes only on an `UNAVAILABLE` reconnect.
- endpoint law: the target is a grpc `host:port` netloc, never a `/v1/<signal>` path; `insecure=True` builds an `insecure_channel`, otherwise `credentials` builds a `secure_channel`.
- compression law: `compression=` takes `grpc.Compression`, defaulting to `Compression.NoCompression`.
- consumer law: composition rides the runtime telemetry install's exporter-factory seam as a selectable transport row, never a library import.

[RAIL_LAW]:
- Package: `opentelemetry-exporter-otlp-proto-grpc`
- Owns: OTLP span/metric/log egress over a persistent gRPC channel to a `host:port` collector target
- Accept: the daemon transport row on a long-lived non-forking server, selected through the telemetry install's exporter-factory seam
- Reject: the estate default (proto-http owns that), selection inside any forking-worker fabric, a library-level import of an exporter class
