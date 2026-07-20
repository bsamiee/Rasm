# [PY_RUNTIME_API_OPENTELEMETRY_PROCESSOR_BAGGAGE]

`opentelemetry-processor-baggage` copies OTel baggage onto emitted telemetry: a `SpanProcessor` stamping baggage entries onto a span as attributes at start, and a `LogRecordProcessor` stamping them onto a log record at emit, each gated by a per-key predicate. Baggage values also ride every outbound `baggage` header, so the gate stays a closed key set — arbitrary caller baggage never promotes onto spans or wire headers.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-processor-baggage`
- package: `opentelemetry-processor-baggage`
- module: `opentelemetry.processor.baggage`
- owner: `runtime`
- rail: observability
- asset: pure-Python runtime library
- namespaces: `opentelemetry.processor.baggage`
- capability: predicate-gated baggage-to-attribute promotion onto spans at start and log records at emit, log side capped

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: baggage processors
- rail: observability

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]      | [RAIL]                                             |
| :-----: | :----------------------- | :----------------- | :------------------------------------------------- |
|  [01]   | `BaggageSpanProcessor`   | span processor     | baggage entries onto a span as attributes at start |
|  [02]   | `BaggageLogProcessor`    | log processor      | baggage entries onto a log record at emit, capped  |
|  [03]   | `ALLOW_ALL_BAGGAGE_KEYS` | predicate constant | always-true key gate that admits every baggage key |
|  [04]   | `BaggageKeyPredicates`   | predicate type     | `(str) -> bool` or a sequence of such callables    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: processor registration
- rail: observability
- Constructor accepts one predicate or a sequence: a callable wraps to a single-element list, a sequence materializes to a list, and `on_start`/`on_emit` promote a key when any predicate admits it.
- `TracerProvider` exposes no `span_processors=` constructor slot; a `BaggageSpanProcessor` enters through `add_span_processor`, a `BaggageLogProcessor` through `LoggerProvider.add_log_record_processor`.

| [INDEX] | [SURFACE]                                                                | [ENTRY_FAMILY] | [RAIL]                         |
| :-----: | :----------------------------------------------------------------------- | :------------- | :----------------------------- |
|  [01]   | `BaggageSpanProcessor(baggage_key_predicate)`                            | construct      | span-side baggage gate         |
|  [02]   | `TracerProvider.add_span_processor(BaggageSpanProcessor(...))`           | register       | stamp spans at start           |
|  [03]   | `BaggageLogProcessor(baggage_key_predicate, max_baggage_attributes=128)` | construct      | log-side baggage gate, cap 128 |
|  [04]   | `LoggerProvider.add_log_record_processor(BaggageLogProcessor(...))`      | register       | stamp log records at emit      |

## [04]-[IMPLEMENTATION_LAW]

[OBSERVABILITY_TOPOLOGY]:
- reification law: package `__init__` pulls `BaggageLogProcessor`, which imports `opentelemetry.sdk._logs` for its base, so importing anything from the package reifies the `_logs` tier — the runtime telemetry install defers it behind a `lazy from`, reified at the first emitting install.
- predicate law: a closed key predicate is the Rasm posture — the telemetry `PROMOTED_BAGGAGE` frozenset's `__contains__` — because `ALLOW_ALL_BAGGAGE_KEYS` stamps arbitrary caller baggage onto every span and every outgoing `baggage` header; promoted keys therefore never carry sensitive material.
- write law: `on_start` sets each admitted key on the span unconditionally with no cap; `on_emit` skips a key already present on the log record and stops at `max_baggage_attributes` (128), so log stamping never overwrites stdlib-logging or downstream-processor attributes and never floods the record.
- order law: the promotion processor registers before the batch processor so spans and records are stamped before enqueue.
- consumer anchor: the runtime telemetry install (`_tracer_provider` / `_log_attach`, `PROMOTED_BAGGAGE`) in the observability/telemetry page composes both processors as the sole registration site.

[RAIL_LAW]:
- Package: `opentelemetry-processor-baggage`
- Owns: predicate-gated baggage-to-attribute promotion onto spans at start and log records at emit
- Accept: a closed `PROMOTED_BAGGAGE.__contains__` predicate, promotion processor registered before the batch processor
- Reject: `ALLOW_ALL_BAGGAGE_KEYS` in the install, sensitive material in a promoted key, an eager package import that reifies `_logs` ahead of the first emitting install
