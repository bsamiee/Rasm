# [PY_BRANCH_API_OPENTELEMETRY_EXPORTER_OTLP_PROTO_HTTP]

`opentelemetry-exporter-otlp-proto-http` owns the OTLP/HTTP transport tail of the observability rail: `OTLPSpanExporter`, `OTLPMetricExporter`, and `OTLPLogExporter` encode SDK span, metric, and log-record batches as OTLP protobuf and POST them to a collector over a pooled `requests.Session`. Each is the terminal sink of one signal pipeline, wired behind an SDK processor at the composition root, owning jittered-backoff retry, per-signal env resolution, and gzip/deflate compression.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-exporter-otlp-proto-http`
- package: `opentelemetry-exporter-otlp-proto-http` (`Apache-2.0`, OpenTelemetry Authors)
- module: `opentelemetry.exporter.otlp.proto.http`
- namespaces: `opentelemetry.exporter.otlp.proto.http` (`Compression`), `...http.trace_exporter`, `...http.metric_exporter`, `...http._log_exporter`
- rail: observability

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: exporter family

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]   | [CAPABILITY]                                    |
| :-----: | :------------------- | :-------------- | :---------------------------------------------- |
|  [01]   | `OTLPSpanExporter`   | span exporter   | OTLP/HTTP span batch POST (`SpanExporter`)      |
|  [02]   | `OTLPMetricExporter` | metric exporter | OTLP/HTTP metric batch POST (`MetricExporter`)  |
|  [03]   | `OTLPLogExporter`    | log exporter    | OTLP/HTTP log-record batch POST (`LogExporter`) |
|  [04]   | `Compression`        | enum            | `NoCompression` / `Deflate` / `Gzip`            |

[COMPRESSION_VALUES]:
- `NoCompression` = `"none"`, default absent `OTEL_EXPORTER_OTLP_COMPRESSION`
- `Deflate` = `"deflate"`, sends `Content-Encoding: deflate`
- `Gzip` = `"gzip"`, sends `Content-Encoding: gzip`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: OTLPSpanExporter

| [INDEX] | [SURFACE]                                   | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :------------------------------------------ | :------- | :-------------------------------------------- |
|  [01]   | `OTLPSpanExporter(...)`                     | ctor     | span exporter with full TLS/header/env config |
|  [02]   | `export(spans) -> SpanExportResult`         | instance | encode + POST batch with retry                |
|  [03]   | `force_flush(timeout_millis=30000) -> bool` | instance | no-op true (HTTP path holds no queue)         |
|  [04]   | `shutdown()`                                | instance | set shutdown flag, abort in-flight backoff    |

[ENTRYPOINT_SCOPE]: OTLPMetricExporter

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :---------------------------------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `OTLPMetricExporter(..., preferred_temporality, preferred_aggregation)` | ctor     | metric exporter, temporality prefs     |
|  [02]   | `export(metrics_data, timeout_millis=10_000) -> MetricExportResult`     | instance | encode + POST `MetricsData` with retry |
|  [03]   | `force_flush(timeout_millis=10_000) -> bool`                            | instance | no-op true                             |
|  [04]   | `shutdown(timeout_millis=30_000) -> None`                               | instance | release session, abort backoff         |

[ENTRYPOINT_SCOPE]: OTLPLogExporter

| [INDEX] | [SURFACE]                                    | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `OTLPLogExporter(...)`                       | ctor     | log-record exporter with full config     |
|  [02]   | `export(batch) -> LogExportResult`           | instance | encode + POST `LogData` batch with retry |
|  [03]   | `force_flush(timeout_millis=10_000) -> bool` | instance | no-op true                               |
|  [04]   | `shutdown()`                                 | instance | set shutdown flag, abort backoff         |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- All three exporters share `endpoint`, `certificate_file`, `client_key_file`, `client_certificate_file`, `headers`, `timeout`, `compression`, `session`; span/log add keyword-only `meter_provider` (drives internal self-observability metrics under `OTEL_PYTHON_SDK_INTERNAL_METRICS_ENABLED`), metric adds `preferred_temporality`/`preferred_aggregation`.
- `endpoint` resolves explicit arg, else the per-signal env (`OTEL_EXPORTER_OTLP_TRACES_ENDPOINT` family, used verbatim), else `OTEL_EXPORTER_OTLP_ENDPOINT` with the signal path appended (`v1/traces` / `v1/metrics` / `v1/logs`), else `DEFAULT_ENDPOINT`.
- `timeout` resolves explicit arg, else `OTEL_EXPORTER_OTLP_*_TIMEOUT`, else `DEFAULT_TIMEOUT`.
- `headers` resolves explicit `dict`, else `parse_env_headers(OTEL_EXPORTER_OTLP_*_HEADERS, liberal=True)`; auth tokens ride here, merged into `session.headers` beside `Content-Type: application/x-protobuf` and the OTel `User-Agent`.
- `compression` resolves explicit `Compression`, else `_compression_from_env()` over `OTEL_EXPORTER_OTLP_*_COMPRESSION`; a non-`NoCompression` value adds its `Content-Encoding` header and compresses the protobuf body.
- `session` resolves an explicit `requests.Session` (mTLS, proxy, pool, adapters), else the `_load_session_from_envvar` credential-provider hook, else a fresh `requests.Session`; TLS verify uses `certificate_file` for the server CA and `(client_certificate_file, client_key_file)` for client mTLS.
- `export` retries retryable responses and keep-alive connection close with jittered exponential backoff (±20% per step) under an overall deadline, aborting when `shutdown()` fires the `_shutdown_in_progress` event.
- `preferred_temporality` (`dict[type, AggregationTemporality]`) selects `CUMULATIVE` vs `DELTA` per instrument type (env override `OTEL_EXPORTER_OTLP_METRICS_TEMPORALITY_PREFERENCE`); `preferred_aggregation` (`dict[type, Aggregation]`) overrides the per-instrument aggregation.

[STACKING]:
- `opentelemetry-sdk`(`.api/opentelemetry-sdk.md`): each exporter is the terminal sink behind one SDK processor — `OTLPSpanExporter` -> `BatchSpanProcessor` -> `TracerProvider`, `OTLPMetricExporter` -> `PeriodicExportingMetricReader` -> `MeterProvider`, `OTLPLogExporter` -> `BatchLogRecordProcessor` -> `LoggerProvider`; the processor owns batching and queueing, the exporter owns transport and retry.
- `opentelemetry-sdk`(`.api/opentelemetry-sdk.md`): the exporter `preferred_temporality` binds the reader `preferred_temporality` and the backend — `CUMULATIVE` for Prometheus-style scrape, `DELTA` for OTLP-delta backends — set once at construction.
- `protobuf`(`.api/protobuf.md`): SDK `ReadableSpan`/`MetricsData`/`LogData` encode to OTLP protobuf inside `export`; the composing owner passes SDK views and never hand-builds the protobuf tree.
- `meter_provider` on the span/log exporter routes the exporter's own success/failure/duration metrics into the same `MeterProvider`, closing the self-observability loop without a second pipeline.

[LOCAL_ADMISSION]:
- One exporter instance per signal, built at the composition root and handed to the matching SDK processor or reader.
- A deployment holding a configured `Session` (mTLS client certs, proxy adapters, pool sizing) passes it through `session` rather than the env certificate files.
- Built-in retry is the whole retry budget; an external retry around `export` multiplies the backoff.

[RAIL_LAW]:
- Package: `opentelemetry-exporter-otlp-proto-http`
- Owns: OTLP/HTTP protobuf encoding, gzip/deflate body compression, jittered-backoff retry, and pooled-session transport for spans, metrics, and log records
- Accept: one exporter per signal at the composition root, `Compression.Gzip` where the backend supports it, a `session` override for mTLS/proxy, `preferred_temporality` matched to the backend, `meter_provider` for self-observability
- Reject: calling `export`/`force_flush` directly outside an SDK processor, per-request exporter construction, an external retry wrapper around `export`, plain HTTP where the backend requires mTLS
