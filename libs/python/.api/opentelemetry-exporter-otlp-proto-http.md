# [PY_BRANCH_API_OPENTELEMETRY_EXPORTER_OTLP_PROTO_HTTP]

`opentelemetry-exporter-otlp-proto-http` supplies `OTLPSpanExporter`, `OTLPMetricExporter`, and `OTLPLogExporter`, which encode SDK span, metric, and log-record batches as protobuf and push them to an OTLP/HTTP endpoint using `requests`. These are the production-path exporters that wire into `BatchSpanProcessor`, `PeriodicExportingMetricReader`, and `BatchLogRecordProcessor` at the composition root.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-exporter-otlp-proto-http`
- package: `opentelemetry-exporter-otlp-proto-http`
- module: `opentelemetry.exporter.otlp.proto.http`
- asset: runtime library
- rail: observability
- namespaces: `opentelemetry.exporter.otlp.proto.http.trace_exporter`, `opentelemetry.exporter.otlp.proto.http.metric_exporter`, `opentelemetry.exporter.otlp.proto.http._log_exporter`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: exporter family
- rail: observability

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]   | [RAIL]                               |
| :-----: | :------------------- | :-------------- | :----------------------------------- |
|  [01]   | `OTLPSpanExporter`   | span exporter   | OTLP/HTTP span batch push            |
|  [02]   | `OTLPMetricExporter` | metric exporter | OTLP/HTTP metric batch push          |
|  [03]   | `OTLPLogExporter`    | log exporter    | OTLP/HTTP log record batch push      |
|  [04]   | `Compression`        | enum            | `NONE`, `GZIP` transport compression |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: OTLPSpanExporter
- rail: observability

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY] | [RAIL]                               |
| :-----: | :--------------------------------------------------------------- | :------------- | :----------------------------------- |
|  [01]   | `OTLPSpanExporter(endpoint, headers, timeout, compression, ...)` | construction   | OTLP span exporter with config       |
|  [02]   | `OTLPSpanExporter.export(spans)`                                 | export         | push `ReadableSpan` batch to backend |
|  [03]   | `OTLPSpanExporter.shutdown()`                                    | lifecycle      | flush and release HTTP session       |

[ENTRYPOINT_SCOPE]: OTLPMetricExporter
- rail: observability

| [INDEX] | [SURFACE]                                                                                                        | [ENTRY_FAMILY] | [RAIL]                                      |
| :-----: | :--------------------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `OTLPMetricExporter(endpoint, headers, timeout, compression, preferred_temporality, preferred_aggregation, ...)` | construction   | metric exporter with temporality preference |
|  [02]   | `OTLPMetricExporter.export(metrics_data)`                                                                        | export         | push `MetricsData` batch to backend         |
|  [03]   | `OTLPMetricExporter.shutdown()`                                                                                  | lifecycle      | flush and release HTTP session              |

[ENTRYPOINT_SCOPE]: OTLPLogExporter
- rail: observability

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY] | [RAIL]                                    |
| :-----: | :-------------------------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `OTLPLogExporter(endpoint, headers, timeout, compression, ...)` | construction   | OTLP log exporter with config             |
|  [02]   | `OTLPLogExporter.export(log_records)`                           | export         | push `ReadableLogRecord` batch to backend |
|  [03]   | `OTLPLogExporter.shutdown()`                                    | lifecycle      | flush and release HTTP session            |

## [04]-[IMPLEMENTATION_LAW]

[EXPORTER_TOPOLOGY]:
- all three exporters share the same constructor parameter set: `endpoint`, `certificate_file`, `client_key_file`, `client_certificate_file`, `headers`, `timeout`, `compression`, `session`, `meter_provider`
- `endpoint` defaults to `OTEL_EXPORTER_OTLP_ENDPOINT` env var; span exporter appends `/v1/traces`, metric exporter `/v1/metrics`, log exporter `/v1/logs`
- `headers` accepts `dict[str, str]`; authentication tokens go here
- `compression` is `Compression.NONE` (default) or `Compression.GZIP`; GZIP must be matched by the backend
- `session` accepts a pre-configured `requests.Session` for mTLS, proxy, or retry configuration
- `preferred_temporality` on `OTLPMetricExporter` is `dict[type[Instrument], AggregationTemporality]`; controls `CUMULATIVE` vs `DELTA` per instrument type
- exporters are designed to be passed directly to SDK processors/readers; never call `export()` manually in production

[LOCAL_ADMISSION]:
- Exporters are instantiated once at the composition root and passed to `BatchSpanProcessor`, `PeriodicExportingMetricReader`, or `BatchLogRecordProcessor`.
- `session` override is the correct place for per-deployment mTLS certificate configuration.
- `OTEL_EXPORTER_OTLP_HEADERS` env var may carry authentication headers as `key=value` pairs separated by commas.

[RAIL_LAW]:
- Package: `opentelemetry-exporter-otlp-proto-http`
- Owns: OTLP/HTTP protobuf encoding and HTTP transport for spans, metrics, and log records
- Accept: one exporter instance per signal per process, `Compression.GZIP` where the backend supports it, `session` override for mTLS
- Reject: calling `export()` directly outside processors, per-request exporter instantiation, plain HTTP over the wire in production
