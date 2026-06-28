# [PY_BRANCH_API_OPENTELEMETRY_EXPORTER_OTLP_PROTO_HTTP]

`opentelemetry-exporter-otlp-proto-http` supplies `OTLPSpanExporter`, `OTLPMetricExporter`, and `OTLPLogExporter`, which encode SDK span, metric, and log-record batches as OTLP protobuf and POST them to an OTLP/HTTP collector over a pooled `requests.Session`. These are the production-path exporters that wire into `BatchSpanProcessor`, `PeriodicExportingMetricReader`, and `BatchLogRecordProcessor` at the composition root. Each exporter owns its own jittered exponential-backoff retry loop (`_MAX_RETRYS = 6`), per-signal endpoint/cert/header env resolution, and optional gzip/deflate body compression — the design composes the exporter as a single configured sink, never re-implementing transport, retry, or compression.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-exporter-otlp-proto-http`
- package: `opentelemetry-exporter-otlp-proto-http`
- version: `1.43.0`
- license: `Apache-2.0`
- module: `opentelemetry.exporter.otlp.proto.http`
- asset: runtime library
- rail: observability
- namespaces: `opentelemetry.exporter.otlp.proto.http` (`Compression`), `...http.trace_exporter`, `...http.metric_exporter`, `...http._log_exporter`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: exporter family
- rail: observability

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]   | [RAIL]                                          |
| :-----: | :------------------- | :-------------- | :---------------------------------------------- |
|  [01]   | `OTLPSpanExporter`   | span exporter   | OTLP/HTTP span batch POST (`SpanExporter`)      |
|  [02]   | `OTLPMetricExporter` | metric exporter | OTLP/HTTP metric batch POST (`MetricExporter`)  |
|  [03]   | `OTLPLogExporter`    | log exporter    | OTLP/HTTP log-record batch POST (`LogExporter`) |
|  [04]   | `Compression`        | enum            | `NoCompression` / `Deflate` / `Gzip`            |

[COMPRESSION_VALUES]:
- `Compression.NoCompression` = `"none"` (default unless `OTEL_EXPORTER_OTLP_COMPRESSION` set)
- `Compression.Deflate` = `"deflate"` (body via `zlib.compress`, `Content-Encoding: deflate`)
- `Compression.Gzip` = `"gzip"` (body via `gzip.GzipFile`, `Content-Encoding: gzip`)

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: OTLPSpanExporter
- rail: observability
- shared constructor shape with all three exporters except metric temporality/aggregation; positional order: `endpoint`, `certificate_file`, `client_key_file`, `client_certificate_file`, `headers`, `timeout`, `compression`, `session`, then keyword-only `meter_provider`.

| [INDEX] | [SURFACE]                                                                                                                                                                                        | [ENTRY_FAMILY] | [RAIL]                                        |
| :-----: | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `OTLPSpanExporter(endpoint=None, certificate_file=None, client_key_file=None, client_certificate_file=None, headers=None, timeout=None, compression=None, session=None, *, meter_provider=None)` | construction   | span exporter with full TLS/header/env config |
|  [02]   | `OTLPSpanExporter.export(spans: Sequence[ReadableSpan]) -> SpanExportResult`                                                                                                                     | export         | encode + POST batch with retry loop           |
|  [03]   | `OTLPSpanExporter.force_flush(timeout_millis=30000) -> bool`                                                                                                                                     | flush          | no-op true (HTTP path holds no queue)         |
|  [04]   | `OTLPSpanExporter.shutdown()`                                                                                                                                                                    | lifecycle      | set shutdown flag, abort in-flight backoff    |

[ENTRYPOINT_SCOPE]: OTLPMetricExporter
- rail: observability
- adds temporality/aggregation maps via `OTLPMetricExporterMixin`; `export` signature mirrors the SDK `MetricExporter` contract.

| [INDEX] | [SURFACE]                                                                                                                                                                                                                          | [ENTRY_FAMILY] | [RAIL]                                                  |
| :-----: | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------------------ |
|  [01]   | `OTLPMetricExporter(endpoint=None, certificate_file=None, client_key_file=None, client_certificate_file=None, headers=None, timeout=None, compression=None, session=None, preferred_temporality=None, preferred_aggregation=None)` | construction   | metric exporter with temporality/aggregation preference |
|  [02]   | `OTLPMetricExporter.export(metrics_data: MetricsData, timeout_millis=10_000, **kwargs) -> MetricExportResult`                                                                                                                      | export         | encode + POST `MetricsData` with retry                  |
|  [03]   | `OTLPMetricExporter.force_flush(timeout_millis=10_000) -> bool`                                                                                                                                                                    | flush          | no-op true                                              |
|  [04]   | `OTLPMetricExporter.shutdown(timeout_millis=30_000, **kwargs) -> None`                                                                                                                                                             | lifecycle      | release session, abort backoff                          |

[ENTRYPOINT_SCOPE]: OTLPLogExporter
- rail: observability

| [INDEX] | [SURFACE]                                                                                                                                                                                       | [ENTRY_FAMILY] | [RAIL]                                   |
| :-----: | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------- | :--------------------------------------- |
|  [01]   | `OTLPLogExporter(endpoint=None, certificate_file=None, client_key_file=None, client_certificate_file=None, headers=None, timeout=None, compression=None, session=None, *, meter_provider=None)` | construction   | log-record exporter with full config     |
|  [02]   | `OTLPLogExporter.export(batch: Sequence[LogData]) -> LogExportResult`                                                                                                                           | export         | encode + POST `LogData` batch with retry |
|  [03]   | `OTLPLogExporter.force_flush(timeout_millis=10_000) -> bool`                                                                                                                                    | flush          | no-op true                               |
|  [04]   | `OTLPLogExporter.shutdown()`                                                                                                                                                                    | lifecycle      | set shutdown flag, abort backoff         |

## [04]-[IMPLEMENTATION_LAW]

[EXPORTER_TOPOLOGY]:
- all three share constructor params `endpoint`, `certificate_file`, `client_key_file`, `client_certificate_file`, `headers`, `timeout`, `compression`, `session`; span/log add keyword-only `meter_provider` (drives internal `create_exporter_metrics` self-observability, gated on `OTEL_PYTHON_SDK_INTERNAL_METRICS_ENABLED`); metric adds `preferred_temporality`/`preferred_aggregation`.
- `endpoint` resolution order: explicit arg, else per-signal env (`OTEL_EXPORTER_OTLP_TRACES_ENDPOINT` etc.), else `OTEL_EXPORTER_OTLP_ENDPOINT` + signal path append (`v1/traces` / `v1/metrics` / `v1/logs`), else `DEFAULT_ENDPOINT = "http://localhost:4318/"`. Base-endpoint resolution appends the signal path; a per-signal env endpoint is used verbatim.
- `timeout` defaults to `DEFAULT_TIMEOUT = 10` seconds, overridable per-signal via `OTEL_EXPORTER_OTLP_*_TIMEOUT`.
- `headers` resolution: explicit `dict[str,str]`, else `parse_env_headers(OTEL_EXPORTER_OTLP_*_HEADERS, liberal=True)` (`key=value` comma-separated); authentication tokens go here. Headers merge into `session.headers` alongside `Content-Type: application/x-protobuf` and the OTel `User-Agent`.
- `compression` resolution: explicit `Compression`, else `_compression_from_env()` reading `OTEL_EXPORTER_OTLP_*_COMPRESSION`; default `NoCompression`. Non-`NoCompression` adds the matching `Content-Encoding` header and compresses the protobuf body in `_export`.
- `session` accepts a pre-built `requests.Session` (mTLS, proxy, connection-pool, custom adapters); else `_load_session_from_envvar(...CREDENTIAL_PROVIDER)` entry-point hook, else a fresh `requests.Session`. TLS verify uses `certificate_file` (server CA) and `(client_certificate_file, client_key_file)` for client mTLS, both env-resolvable.
- retry: `export` runs `for retry_num in range(_MAX_RETRYS)` with `backoff_seconds = 2**retry_num * random.uniform(0.8, 1.2)` (+/-20% jitter); `_is_retryable(resp)` gates retry on the response; `_export` already retries once on `requests.ConnectionError` (keep-alive close). The loop honors an overall deadline and aborts when `shutdown()` flips `_shutdown_in_progress`.
- `preferred_temporality` on `OTLPMetricExporter` is `dict[type, AggregationTemporality]`; controls `CUMULATIVE` vs `DELTA` per instrument type (env override via `OTEL_EXPORTER_OTLP_METRICS_TEMPORALITY_PREFERENCE`). `preferred_aggregation` is `dict[type, Aggregation]`.

[INTEGRATION_LAW]:
- Stack with `opentelemetry-sdk`: an exporter instance is the terminal sink of one signal pipeline — `OTLPSpanExporter` -> `BatchSpanProcessor` -> `TracerProvider`; `OTLPMetricExporter` -> `PeriodicExportingMetricReader` -> `MeterProvider`; `OTLPLogExporter` -> `BatchLogRecordProcessor` -> `LoggerProvider`. The exporter owns transport+retry; the SDK processor owns batching/queueing. Never call `export()` directly.
- Match `preferred_temporality` on the metric exporter to the backend (Prometheus-style scrape wants `CUMULATIVE`; OTLP-delta backends want `DELTA`) — this is the temporality decision point, set once at construction, not per-instrument in app code.
- The `protobuf`/`opentelemetry-proto` encode is internal; the design never hand-builds OTLP protobuf — it hands SDK `ReadableSpan`/`MetricsData`/`LogData` to `export` and the exporter encodes.
- `meter_provider` on span/log exporters routes the exporter's own success/failure/duration metrics into the same SDK `MeterProvider`, closing the self-observability loop without a second pipeline.

[LOCAL_ADMISSION]:
- Exporters are instantiated once per signal at the composition root and passed to the matching SDK processor/reader; one exporter instance per signal per process.
- `session` override is the correct seam for per-deployment mTLS client certs, proxy adapters, and connection-pool sizing; prefer it over env certificate files when the deployment already builds a configured `Session`.
- The built-in retry loop is sufficient; do not wrap `export` in an external retry — that would multiply the backoff.

[RAIL_LAW]:
- Package: `opentelemetry-exporter-otlp-proto-http`
- Owns: OTLP/HTTP protobuf encoding, gzip/deflate body compression, jittered-backoff retry, and pooled-session transport for spans, metrics, and log records
- Accept: one exporter per signal at the composition root, `Compression.Gzip` where the backend supports it, `session` override for mTLS/proxy, `preferred_temporality` matched to the backend, `meter_provider` for self-observability
- Reject: calling `export()`/`force_flush()` directly outside SDK processors, per-request exporter instantiation, external retry wrappers around `export`, plain HTTP in production where mTLS is required
