# [PY_RUNTIME_API_OPENTELEMETRY_PROCESSOR_BAGGAGE]

`opentelemetry-processor-baggage` promotes OTel baggage onto emitted telemetry under a per-key predicate: a span processor stamps admitted baggage entries as span attributes at start, a log processor stamps them as log-record attributes at emit. Promoted keys ride every outbound `baggage` header, so the predicate admits a closed key vocabulary, never sensitive material.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-processor-baggage`
- package: `opentelemetry-processor-baggage` (Apache-2.0)
- module: `opentelemetry.processor.baggage`
- rail: observability

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: baggage processors

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]      | [CAPABILITY]                                       |
| :-----: | :----------------------- | :----------------- | :------------------------------------------------- |
|  [01]   | `BaggageSpanProcessor`   | span processor     | baggage entries onto a span as attributes at start |
|  [02]   | `BaggageLogProcessor`    | log processor      | baggage entries onto a log record at emit, capped  |
|  [03]   | `ALLOW_ALL_BAGGAGE_KEYS` | predicate constant | always-true key gate admitting every baggage key   |
|  [04]   | `BaggageKeyPredicates`   | predicate type     | `(str) -> bool` or a sequence of such callables    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: processor registration

| [INDEX] | [SURFACE]                                                                | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :----------------------------------------------------------------------- | :------- | :----------------------------- |
|  [01]   | `BaggageSpanProcessor(baggage_key_predicate)`                            | ctor     | span-side baggage gate         |
|  [02]   | `TracerProvider.add_span_processor(BaggageSpanProcessor(...))`           | instance | stamp spans at start           |
|  [03]   | `BaggageLogProcessor(baggage_key_predicate, max_baggage_attributes=128)` | ctor     | log-side baggage gate, cap 128 |
|  [04]   | `LoggerProvider.add_log_record_processor(BaggageLogProcessor(...))`      | instance | stamp log records at emit      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- a predicate is one `(str) -> bool` callable or a sequence; the constructor wraps a callable to a single-element list, and a baggage key promotes when any predicate admits it.
- `on_start` sets every admitted key on the span unconditionally with no cap; `on_emit` skips a key already present on the log record and stops at `max_baggage_attributes` (128), so log stamping never overwrites stdlib-logging or downstream-processor attributes and never floods the record.

[STACKING]:
- `opentelemetry-sdk`(`.api/opentelemetry-sdk.md`): `BaggageSpanProcessor` implements `sdk.trace.SpanProcessor.on_start` and `BaggageLogProcessor` implements `sdk._logs.LogRecordProcessor.on_emit`, registered through `TracerProvider.add_span_processor` / `LoggerProvider.add_log_record_processor` ahead of the batch processor.
- `opentelemetry-api`(`.api/opentelemetry-api.md`): both processors read `baggage.get_all(context)` to source the promotion set; the span side stamps each admitted key via `Span.set_attribute`, the log side writes the `ReadWriteLogRecord` attribute map directly.
- runtime telemetry install: the observability install (`_tracer_provider`/`_log_attach`, `PROMOTED_BAGGAGE`) composes both processors as the sole registration site, gating with the closed `PROMOTED_BAGGAGE.__contains__` predicate.

[LOCAL_ADMISSION]:
- `PROMOTED_BAGGAGE.__contains__` is the install predicate, a closed key set; `ALLOW_ALL_BAGGAGE_KEYS` stamps arbitrary caller baggage onto every span and outbound `baggage` header, so it is rejected in the install.
- importing the package reifies the `_logs` tier — `log_processor` imports `opentelemetry.sdk._logs` — so the runtime install defers `BaggageLogProcessor` behind a lazy import until the first emitting install.

[RAIL_LAW]:
- Package: `opentelemetry-processor-baggage`
- Owns: predicate-gated promotion of baggage entries onto spans at start and log records at emit
- Accept: a closed-key predicate, the promotion processor registered before the batch processor, the log side capped by `max_baggage_attributes`
- Reject: `ALLOW_ALL_BAGGAGE_KEYS` in the install, an eager package import reifying `_logs` ahead of the first emitting install
