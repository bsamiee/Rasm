# [PY_BRANCH_API_OPENTELEMETRY_API]

`opentelemetry-api` supplies the provider, tracer, span, metric instrument, and log API contracts as abstract classes and no-op implementations, plus the context propagation, baggage, and `TextMapPropagator` surfaces that consuming owners depend on at the API boundary. It installs `get_tracer_provider`, `get_meter_provider`, `get_logger_provider`, and `propagate.extract`/`inject` as the global resolution points; SDK providers are registered at startup and never imported in library code.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-api`
- package: `opentelemetry-api`
- module: `opentelemetry`
- version: `1.43.0`
- license: `Apache-2.0`
- rail: observability
- namespaces: `opentelemetry.trace`, `opentelemetry.metrics`, `opentelemetry._logs`, `opentelemetry.context`, `opentelemetry.propagate`, `opentelemetry.propagators.textmap`, `opentelemetry.baggage`, `opentelemetry.attributes`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: trace family
- rail: observability

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [RAIL]                                                 |
| :-----: | :------------------- | :------------ | :----------------------------------------------------- |
|  [01]   | `TracerProvider`     | abstract      | tracer factory contract                                |
|  [02]   | `Tracer`             | abstract      | span creation contract                                 |
|  [03]   | `Span`               | abstract      | active span operation contract                         |
|  [04]   | `SpanContext`        | value         | immutable trace/span/flag triple                       |
|  [05]   | `StatusCode`         | enum          | `UNSET`, `OK`, `ERROR`                                 |
|  [06]   | `Status`             | value         | status code plus optional message                      |
|  [07]   | `SpanKind`           | enum          | `INTERNAL`, `SERVER`, `CLIENT`, `PRODUCER`, `CONSUMER` |
|  [08]   | `TraceFlags`         | value         | sampled and parent flags bitfield                      |
|  [09]   | `TraceState`         | value         | W3C trace state key-value list                         |
|  [10]   | `Link`               | value         | cross-trace span link                                  |
|  [11]   | `NonRecordingSpan`   | no-op         | API-only no-op span                                    |
|  [12]   | `NoOpTracer`         | no-op         | no-op tracer for API-only use                          |
|  [13]   | `NoOpTracerProvider` | no-op         | no-op tracer provider                                  |

[PUBLIC_TYPE_SCOPE]: metrics family
- rail: observability

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [RAIL]                                                                              |
| :-----: | :------------------------ | :------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `MeterProvider`           | abstract      | meter factory contract                                                              |
|  [02]   | `Meter`                   | abstract      | instrument creation contract                                                        |
|  [03]   | `Counter`                 | abstract      | monotonically increasing count                                                      |
|  [04]   | `UpDownCounter`           | abstract      | bidirectional count                                                                 |
|  [05]   | `Histogram`               | abstract      | value distribution recording                                                        |
|  [06]   | `Meter.create_gauge(...)` | factory       | synchronous last-value gauge instrument (the `Gauge` ABC is not top-level exported) |
|  [07]   | `ObservableCounter`       | abstract      | async monotonic count                                                               |
|  [08]   | `ObservableGauge`         | abstract      | async current value                                                                 |
|  [09]   | `ObservableUpDownCounter` | abstract      | async bidirectional count                                                           |
|  [10]   | `Observation`             | value         | (value, attributes) for async                                                       |
|  [11]   | `CallbackOptions`         | value         | timeout hint for async callbacks                                                    |

[PUBLIC_TYPE_SCOPE]: logs and context family
- rail: observability

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [RAIL]                                                   |
| :-----: | :------------------ | :------------ | :------------------------------------------------------- |
|  [01]   | `LoggerProvider`    | abstract      | logger factory contract                                  |
|  [02]   | `Logger`            | abstract      | log record emission contract                             |
|  [03]   | `LogRecord`         | value         | structured log record carrier                            |
|  [04]   | `SeverityNumber`    | enum          | severity levels `TRACE1`..`FATAL4`                       |
|  [05]   | `Context`           | value         | immutable propagation context                            |
|  [06]   | `Token`             | value         | context attachment token                                 |
|  [07]   | `TextMapPropagator` | abstract      | carrier-based context propagator                         |
|  [08]   | `Getter[CarrierT]`  | protocol      | generic carrier read protocol (`get`/`keys`)             |
|  [09]   | `Setter[CarrierT]`  | protocol      | generic carrier write protocol (`set`)                   |
|  [10]   | `DefaultGetter`     | value         | default getter; reads list-valued headers, returns first |
|  [11]   | `DefaultSetter`     | value         | default setter for dict-like header carriers             |
|  [12]   | `CarrierT`          | type var      | carrier type parameter for `Getter`/`Setter`             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: trace API
- rail: observability

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [RAIL]                                   |
| :-----: | :----------------------------------------------------------------- | :------------- | :--------------------------------------- |
|  [01]   | `trace.get_tracer_provider()`                                      | provider       | global tracer provider                   |
|  [02]   | `trace.set_tracer_provider(tracer_provider)`                       | provider       | install global tracer provider           |
|  [03]   | `trace.get_tracer(name, version, ...)`                             | tracer         | obtain instrumentation tracer            |
|  [04]   | `trace.get_current_span(context)`                                  | span           | current active span                      |
|  [05]   | `trace.set_span_in_context(span, context)`                         | context        | embed span into context                  |
|  [06]   | `trace.use_span(span, end_on_exit, ...)`                           | context mgr    | legacy span context manager              |
|  [07]   | `trace.format_trace_id(trace_id)`                                  | utility        | hex-format trace id                      |
|  [08]   | `trace.format_span_id(span_id)`                                    | utility        | hex-format span id                       |
|  [09]   | `Tracer.start_span(name, context, kind, ...)`                      | span creation  | create detached span                     |
|  [10]   | `Tracer.start_as_current_span(name, ...)`                          | span creation  | create and activate span                 |
|  [11]   | `Span.set_attribute(key, value)`                                   | span mutation  | attach one typed attribute               |
|  [12]   | `Span.set_attributes(attributes)`                                  | span mutation  | attach a mapping of attributes at once   |
|  [13]   | `Span.add_event(name, attributes, timestamp)`                      | span mutation  | record timestamped event                 |
|  [14]   | `Span.add_link(context, attributes)`                               | span mutation  | link to another span context post-start  |
|  [15]   | `Span.set_status(status, description)`                             | span mutation  | set span outcome status                  |
|  [16]   | `Span.record_exception(exception, attributes, timestamp, escaped)` | span mutation  | record exception on span                 |
|  [17]   | `Span.update_name(name)`                                           | span mutation  | rename span after start                  |
|  [18]   | `Span.is_recording()` / `Span.get_span_context()`                  | span query     | recording flag / immutable `SpanContext` |
|  [19]   | `Span.end(end_time)`                                               | span lifecycle | end and finalize span                    |

[ENTRYPOINT_SCOPE]: metrics API
- rail: observability

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [RAIL]                                  |
| :-----: | :--------------------------------------------------------- | :------------- | :-------------------------------------- |
|  [01]   | `metrics.get_meter_provider()`                             | provider       | global meter provider                   |
|  [02]   | `metrics.set_meter_provider(meter_provider)`               | provider       | install global meter provider           |
|  [03]   | `metrics.get_meter(name, version, ...)`                    | meter          | obtain instrumentation meter            |
|  [04]   | `Meter.create_counter(name, unit, description)`            | instrument     | create monotonic counter                |
|  [05]   | `Meter.create_up_down_counter(name, ...)`                  | instrument     | create bidirectional counter            |
|  [06]   | `Meter.create_histogram(name, ...)`                        | instrument     | create histogram recorder               |
|  [07]   | `Meter.create_gauge(name, unit, description)`              | instrument     | create synchronous last-value gauge     |
|  [08]   | `Meter.create_observable_counter(name, callbacks)`         | instrument     | create async observable counter         |
|  [09]   | `Meter.create_observable_gauge(name, callbacks)`           | instrument     | create async observable gauge           |
|  [10]   | `Meter.create_observable_up_down_counter(name, callbacks)` | instrument     | create async observable up-down counter |
|  [11]   | `Counter.add(amount, attributes, context)`                 | record         | increment counter                       |
|  [12]   | `UpDownCounter.add(amount, attributes, context)`           | record         | adjust bidirectional counter            |
|  [13]   | `Histogram.record(amount, attributes, context)`            | record         | record histogram measurement            |
|  [14]   | `Gauge.set(amount, attributes, context)`                   | record         | set synchronous gauge value             |

[ENTRYPOINT_SCOPE]: context and propagation
- rail: observability

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [RAIL]                                        |
| :-----: | :----------------------------------------------------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `context.get_current()`                                            | context        | current active context                        |
|  [02]   | `context.attach(context)`                                          | context        | activate context, return token                |
|  [03]   | `context.detach(token)`                                            | context        | restore previous context                      |
|  [04]   | `context.get_value(key, context)`                                  | context        | read value from context                       |
|  [05]   | `context.set_value(key, value, context)`                           | context        | derive context with new value                 |
|  [06]   | `context.create_key(keyname)`                                      | context        | mint a unique context key string              |
|  [07]   | `baggage.set_baggage(name, value, context)`                        | baggage        | derive context with baggage entry             |
|  [08]   | `baggage.get_baggage(name, context)`                               | baggage        | read one baggage value                        |
|  [09]   | `baggage.get_all(context)`                                         | baggage        | read full baggage mapping                     |
|  [10]   | `baggage.remove_baggage(name, context)` / `baggage.clear(context)` | baggage        | drop one / all baggage entries                |
|  [11]   | `propagate.extract(carrier, context, getter)`                      | propagation    | decode context from carrier (custom `Getter`) |
|  [12]   | `propagate.inject(carrier, context, setter)`                       | propagation    | encode context into carrier (custom `Setter`) |
|  [13]   | `propagate.get_global_textmap()`                                   | propagation    | global composite propagator                   |
|  [14]   | `propagate.set_global_textmap(propagator)`                         | propagation    | install composite propagator                  |

[ENTRYPOINT_SCOPE]: logs API
- rail: observability

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [RAIL]                         |
| :-----: | :------------------------------------------- | :------------- | :----------------------------- |
|  [01]   | `_logs.get_logger_provider()`                | provider       | global logger provider         |
|  [02]   | `_logs.set_logger_provider(logger_provider)` | provider       | install global logger provider |
|  [03]   | `_logs.get_logger(name, version, ...)`       | logger         | obtain instrumentation logger  |
|  [04]   | `Logger.emit(record)`                        | emit           | emit a `LogRecord`             |

## [04]-[IMPLEMENTATION_LAW]

[OBSERVABILITY_TOPOLOGY]:
- provider singletons: `get_tracer_provider()`, `get_meter_provider()`, `get_logger_provider()` return no-op providers until SDK providers are installed via `set_*`; library code never calls `set_*`
- context model: `Context` is immutable; `attach`/`detach` are scoped and token-paired; contextvars back the default implementation
- span lifecycle: `start_span` returns a detached span; `start_as_current_span` is a context manager that activates and ends the span; always use the context manager form
- attributes: values are `str | bool | int | float | Sequence[str | bool | int | float]`; keys must be non-empty strings; `set_attributes` accepts a mapping in one call
- instruments: synchronous `Counter`/`UpDownCounter`/`Histogram`/`Gauge` are recorded imperatively (`add`/`record`/`set`); observable (`Observable*`) instruments pull via registered callbacks yielding `Observation` under a `CallbackOptions` timeout — never mix push and pull on one instrument
- baggage vs attributes: baggage is propagated cross-process key-value context (`set_baggage` derives a new immutable `Context`), distinct from span attributes which are local to a span; baggage does not auto-copy onto spans
- propagation: `TextMapPropagator` reads via `Getter[CarrierT]` and writes via `Setter[CarrierT]`; `DefaultGetter`/`DefaultSetter` cover dict-like HTTP header carriers, and a custom `Getter` handles multi-dict carriers such as `grpc.aio.Metadata`

[STACKS_WITH]:
- grpc trace continuity: a client interceptor injects via `propagate.inject(metadata, setter=...)` and a server interceptor extracts via `propagate.extract(invocation_metadata, getter=...)` then opens `Tracer.start_as_current_span(kind=SpanKind.SERVER)` — `opentelemetry-api` owns the W3C `traceparent`/`tracestate` encoding while `grpcio` owns the metadata carrier.
- msgspec attribute projection: `msgspec.to_builtins(struct, str_keys=True)` produces exactly the primitive `str | bool | int | float | Sequence[...]` mapping that `Span.set_attributes` accepts, so a decoded wire struct annotates a span with no manual flattening.
- structlog correlation: `trace.get_current_span().get_span_context()` yields the `trace_id`/`span_id` that `trace.format_trace_id`/`format_span_id` render as the hex fields a `structlog` processor binds onto every log line for trace-log correlation.
- stamina retry spans: a `stamina.retry_context` loop wraps each attempt in a child span; `RpcError.code()` (from `grpcio`) maps to `Status(StatusCode.ERROR)` on the final failed span via `Span.set_status` + `Span.record_exception`.
- SDK boundary: this package emits only no-op providers until `opentelemetry-sdk` installs real ones via `set_*_provider` at the composition root; library code calls `get_tracer`/`get_meter`/`get_logger` and never imports the SDK.

[LOCAL_ADMISSION]:
- Library code imports only from `opentelemetry-api`; SDK imports belong to the application composition root.
- Propagation carriers are dict-like; the default getter reads list-valued headers and returns the first value; supply a custom `Getter` for `grpc.aio.Metadata` or other multi-dict carriers.
- Metrics instruments are created once per meter and reused; never create per-request instruments; choose synchronous (`add`/`record`/`set`) vs observable (callback) per measurement model.
- Use `start_as_current_span` (context-manager form) over `start_span`; reserve `start_span` for spans whose activation crosses an async boundary the context manager cannot span.

[RAIL_LAW]:
- Package: `opentelemetry-api`
- Owns: OTel API contracts, context propagation, baggage, global provider resolution, no-op implementations
- Accept: API-only imports in library code, `get_tracer`/`get_meter`/`get_logger` + attribute-bound usage, `propagate.extract`/`inject` with carrier-typed `Getter`/`Setter`, synchronous and observable instruments, cross-process `baggage`
- Reject: SDK imports in library code, per-request instrument creation, mutating a `Context` in place, `set_*_provider` outside composition root, citing `Gauge` as a top-level import (create via `Meter.create_gauge`)
