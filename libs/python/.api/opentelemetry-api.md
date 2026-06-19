# [PY_BRANCH_API_OPENTELEMETRY_API]

`opentelemetry-api` supplies the provider, tracer, span, metric instrument, and log API contracts as abstract classes and no-op implementations, plus the context propagation, baggage, and `TextMapPropagator` surfaces that consuming owners depend on at the API boundary. It installs `get_tracer_provider`, `get_meter_provider`, `get_logger_provider`, and `propagate.extract`/`inject` as the global resolution points; SDK providers are registered at startup and never imported in library code.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-api`
- package: `opentelemetry-api`
- module: `opentelemetry`
- asset: runtime library
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

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [RAIL]                           |
| :-----: | :------------------------ | :------------ | :------------------------------- |
|  [01]   | `MeterProvider`           | abstract      | meter factory contract           |
|  [02]   | `Meter`                   | abstract      | instrument creation contract     |
|  [03]   | `Counter`                 | abstract      | monotonically increasing count   |
|  [04]   | `UpDownCounter`           | abstract      | bidirectional count              |
|  [05]   | `Histogram`               | abstract      | value distribution recording     |
|  [06]   | `ObservableCounter`       | abstract      | async monotonic count            |
|  [07]   | `ObservableGauge`         | abstract      | async current value              |
|  [08]   | `ObservableUpDownCounter` | abstract      | async bidirectional count        |
|  [09]   | `Observation`             | value         | (value, attributes) for async    |
|  [10]   | `CallbackOptions`         | value         | timeout hint for async callbacks |

[PUBLIC_TYPE_SCOPE]: logs and context family
- rail: observability

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [RAIL]                             |
| :-----: | :------------------ | :------------ | :--------------------------------- |
|  [01]   | `LoggerProvider`    | abstract      | logger factory contract            |
|  [02]   | `Logger`            | abstract      | log record emission contract       |
|  [03]   | `LogRecord`         | value         | structured log record carrier      |
|  [04]   | `SeverityNumber`    | enum          | severity levels `TRACE1`..`FATAL4` |
|  [05]   | `Context`           | value         | immutable propagation context      |
|  [06]   | `Token`             | value         | context attachment token           |
|  [07]   | `TextMapPropagator` | abstract      | carrier-based context propagator   |
|  [08]   | `DefaultGetter`     | value         | default HTTP-header carrier getter |
|  [09]   | `DefaultSetter`     | value         | default HTTP-header carrier setter |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: trace API
- rail: observability

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [RAIL]                         |
| :-----: | :-------------------------------------------- | :------------- | :----------------------------- |
|  [01]   | `trace.get_tracer_provider()`                 | provider       | global tracer provider         |
|  [02]   | `trace.set_tracer_provider(tracer_provider)`  | provider       | install global tracer provider |
|  [03]   | `trace.get_tracer(name, version, ...)`        | tracer         | obtain instrumentation tracer  |
|  [04]   | `trace.get_current_span(context)`             | span           | current active span            |
|  [05]   | `trace.set_span_in_context(span, context)`    | context        | embed span into context        |
|  [06]   | `trace.use_span(span, end_on_exit, ...)`      | context mgr    | legacy span context manager    |
|  [07]   | `trace.format_trace_id(trace_id)`             | utility        | hex-format trace id            |
|  [08]   | `trace.format_span_id(span_id)`               | utility        | hex-format span id             |
|  [09]   | `Tracer.start_span(name, context, kind, ...)` | span creation  | create detached span           |
|  [10]   | `Tracer.start_as_current_span(name, ...)`     | span creation  | create and activate span       |
|  [11]   | `Span.set_attribute(key, value)`              | span mutation  | attach typed attribute         |
|  [12]   | `Span.add_event(name, attributes, timestamp)` | span mutation  | record timestamped event       |
|  [13]   | `Span.set_status(status)`                     | span mutation  | set span outcome status        |
|  [14]   | `Span.record_exception(exception, ...)`       | span mutation  | record exception on span       |
|  [15]   | `Span.end(end_time)`                          | span lifecycle | end and finalize span          |

[ENTRYPOINT_SCOPE]: metrics API
- rail: observability

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :------------------------------------------------- | :------------- | :------------------------------ |
|  [01]   | `metrics.get_meter_provider()`                     | provider       | global meter provider           |
|  [02]   | `metrics.set_meter_provider(meter_provider)`       | provider       | install global meter provider   |
|  [03]   | `metrics.get_meter(name, version, ...)`            | meter          | obtain instrumentation meter    |
|  [04]   | `Meter.create_counter(name, unit, description)`    | instrument     | create monotonic counter        |
|  [05]   | `Meter.create_up_down_counter(name, ...)`          | instrument     | create bidirectional counter    |
|  [06]   | `Meter.create_histogram(name, ...)`                | instrument     | create histogram recorder       |
|  [07]   | `Meter.create_observable_counter(name, callbacks)` | instrument     | create async observable counter |
|  [08]   | `Meter.create_observable_gauge(name, callbacks)`   | instrument     | create async observable gauge   |
|  [09]   | `Counter.add(amount, attributes)`                  | record         | increment counter               |
|  [10]   | `Histogram.record(amount, attributes)`             | record         | record histogram measurement    |

[ENTRYPOINT_SCOPE]: context and propagation
- rail: observability

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY] | [RAIL]                         |
| :-----: | :----------------------------------------- | :------------- | :----------------------------- |
|  [01]   | `context.get_current()`                    | context        | current active context         |
|  [02]   | `context.attach(context)`                  | context        | activate context, return token |
|  [03]   | `context.detach(token)`                    | context        | restore previous context       |
|  [04]   | `context.get_value(key, context)`          | context        | read value from context        |
|  [05]   | `context.set_value(key, value, context)`   | context        | derive context with new value  |
|  [06]   | `propagate.extract(carrier, context)`      | propagation    | decode context from carrier    |
|  [07]   | `propagate.inject(carrier, context)`       | propagation    | encode context into carrier    |
|  [08]   | `propagate.get_global_textmap()`           | propagation    | global composite propagator    |
|  [09]   | `propagate.set_global_textmap(propagator)` | propagation    | install composite propagator   |

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
- attributes: values are `str | bool | int | float | Sequence[str | bool | int | float]`; keys must be non-empty strings
- propagation: `TextMapPropagator` reads from and writes to dict-like carriers; `DefaultGetter`/`DefaultSetter` cover HTTP header dicts

[LOCAL_ADMISSION]:
- Library code imports only from `opentelemetry-api`; SDK imports belong to the application composition root.
- Propagation carriers are plain `dict[str, str]`; the default getter reads lists and returns the first value.
- Metrics instruments are created once per meter and reused; never create per-request instruments.

[RAIL_LAW]:
- Package: `opentelemetry-api`
- Owns: OTel API contracts, context propagation, global provider resolution, no-op implementations
- Accept: API-only imports in library code, `get_tracer`/`get_meter`/`get_logger` + attribute-bound usage, `propagate.extract`/`inject`
- Reject: SDK imports in library code, per-request instrument creation, mutable Context, `set_*_provider` outside composition root
