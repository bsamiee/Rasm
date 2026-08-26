# [PY_RUNTIME_API_OPENTELEMETRY_EXPORTER_OTLP_PROTO_GRPC]

`opentelemetry-exporter-otlp-proto-grpc` owns the OTLP/gRPC egress tail of the observability pipeline: `OTLPSpanExporter`, `OTLPMetricExporter`, and `OTLPLogExporter` each hold one persistent `grpc` channel to a `host:port` collector and reuse it across every export, sitting as the terminal sink behind an SDK processor. It is the daemon-selectable transport row, not the repo default — proto-http owns the default egress, and this gRPC row is selected only on a long-lived non-forking server where streaming throughput dominates.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: signal exporters over one persistent gRPC channel

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :------------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `OTLPSpanExporter`   | exporter      | OTLP/gRPC span batch export (`SpanExporter`, `trace_exporter`)     |
|  [02]   | `OTLPMetricExporter` | exporter      | OTLP/gRPC metric export (`MetricExporter`, `metric_exporter`)      |
|  [03]   | `OTLPLogExporter`    | exporter      | OTLP/gRPC log-record export (`LogRecordExporter`, `_log_exporter`) |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: exporter construction
- shared ctor carry: `endpoint`, `insecure`, `credentials`, `headers`, `timeout`, `compression`, `channel_options`, `retryable_error_codes`, `*`, `meter_provider`
- metric adds: `preferred_temporality`, `preferred_aggregation`, `max_export_batch_size`
- `timeout` bounds the whole retry window rather than one RPC, and `retryable_error_codes` decides which failures re-drive inside it; both slots fall to the environment when a row leaves them empty.
- `channel_options` MERGES over the package's own gRPC option defaults by name, so a row overrides one option and inherits every other rather than replacing the set.
- `shutdown(timeout_millis=30_000)` sets the event a pending backoff waits on, so it preempts the retry rather than joining it.

| [INDEX] | [SURFACE]                        | [SHAPE] | [CAPABILITY]                                         |
| :-----: | :------------------------------- | :------ | :--------------------------------------------------- |
|  [01]   | `OTLPSpanExporter(...shared)`    | ctor    | span exporter behind `BatchSpanProcessor`            |
|  [02]   | `OTLPMetricExporter(...+metric)` | ctor    | metric exporter behind a periodic reader             |
|  [03]   | `OTLPLogExporter(...shared)`     | ctor    | log-record exporter behind `BatchLogRecordProcessor` |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Each exporter holds one persistent `grpc` channel built once by `_initialize_channel_and_stub` and reused across every export; the channel reinitializes only on an `UNAVAILABLE` reconnect, and never survives `fork()`.
- `endpoint` is a grpc `host:port` netloc (default `localhost:4317`), never a `/v1/<signal>` path; `insecure=True` builds an `insecure_channel`, else `credentials` builds a `secure_channel`.
- `compression=` takes a `grpc.Compression`, `NoCompression` default, `Gzip`/`Deflate` selectable.
- `grpcio` reaches the repo as this exporter's transitive alone, so its whole surface stays interior here: the channel is a `grpc.Channel` from `insecure_channel`/`secure_channel` over a `ChannelCredentials`, and `grpc.StatusCode.UNAVAILABLE` triggers the reconnect.
- Each exporter owns a COMPLETE retry schedule of its own, and every consumer reads it as the second one: `_export` opens `deadline = time() + timeout`, loops over a fixed maximum attempt count, waits `2**attempt` seconds jittered by a uniform `0.8`–`1.2` factor between attempts, and hands each RPC the REMAINING window as its own per-call timeout, so one deadline bounds the whole re-drive rather than each attempt separately.
- Peer-stated backoff outranks that curve: an `error.trailing_metadata()` carrying `google.rpc.retryinfo-bin` parses into a `RetryInfo` whose `retry_delay` replaces the computed wait — the same peer-directive precedence `reliability/resilience#RESILIENCE` gives a `throttled` verdict.
- Four facts each stop the retry, and `_export` reads all four per attempt: a code outside the retryable set, the attempt ceiling, a wait past the remaining deadline, or a live shutdown. Stopping answers `FAILURE` carrying the code, and the loop waits on `_shutdown_in_progress` rather than sleeping, so `shutdown()` preempts a pending backoff instead of holding the drain open.
- `retryable_error_codes` is a CONSTRUCTOR slot whose absence reads `OTEL_PYTHON_EXPORTER_OTLP_GRPC_RETRYABLE_ERROR_CODES` — a case-insensitive comma list the package marks unstable — and falls last to the package set `CANCELLED`, `DEADLINE_EXCEEDED`, `RESOURCE_EXHAUSTED`, `ABORTED`, `OUT_OF_RANGE`, `UNAVAILABLE`, `DATA_LOSS`. Deployment therefore decides which export failures re-drive wherever the row leaves the slot empty.
- `timeout` absent reads `OTEL_EXPORTER_OTLP_TIMEOUT` and falls last to ten seconds, and that one value bounds the whole retry window rather than one RPC.
- `UNAVAILABLE` on the FIRST attempt alone closes the channel and rebuilds it before the re-drive; every later attempt reuses the standing channel, so a mid-window peer move costs the remaining attempts.
- Exhaustion logs at ERROR under the module's own `DuplicateFilter`, which collapses repeats — the drop reaches a series only where the SDK's internal-metrics flag arms the exporter's recorder, so an unarmed process loses the accounting and keeps one deduplicated log line.

[STACKING]:
- `opentelemetry-sdk`(`.api/opentelemetry-sdk.md`): each exporter is the terminal sink behind one SDK processor — `OTLPSpanExporter` -> `BatchSpanProcessor` -> `TracerProvider`, `OTLPMetricExporter` -> `PeriodicExportingMetricReader` -> `MeterProvider`, `OTLPLogExporter` -> `BatchLogRecordProcessor` -> `LoggerProvider`; the processor owns batching and queueing, the exporter owns transport and reconnect.
- `protobuf`(`.api/protobuf.md`): SDK `ReadableSpan`/`MetricsData`/`ReadableLogRecord` encode to OTLP protobuf inside `_export`; the composing owner hands over SDK views and never hand-builds the protobuf tree.
- `opentelemetry-exporter-otlp-proto-http`(`.api/opentelemetry-exporter-otlp-proto-http.md`): the peer default egress this row substitutes for — proto-http carries the repo default, this gRPC row selects only on a long-lived non-forking server.

[LOCAL_ADMISSION]:
- One exporter per signal, built at the composition root and handed to the matching SDK processor through the telemetry install's exporter-factory boundary.
- Forking-worker crossings (loky/pebble) stay on proto-http; the gRPC channel dies at `fork()`.
- Every row PINS `retryable_error_codes` at construction. `observability/telemetry#TELEMETRY` rules a constructor slot the place a conformance-relevant behavior binds precisely because it holds against every deployment value, and an empty slot hands that decision to an environment variable the package marks unstable and no conformance row reports.
- Pinning HOLDS the specification set rather than narrowing it — the pin denies a deployment its vote over a running pipeline's re-drive policy and settles nothing about which codes OTLP calls retryable, since a narrowed roster forks a client behavior every conforming peer already agrees on.
- Every row states `timeout`, since that one value bounds the entire retry window and an absent slot yields it to `OTEL_EXPORTER_OTLP_TIMEOUT`.
- `Telemetry.shutdown` runs the drain, so the exporter's shutdown-abort is the flush contract's second half: the queue flushes first and a pending backoff preempts rather than outliving it.
