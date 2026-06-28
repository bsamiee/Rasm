# [PY_RUNTIME_API_OPENTELEMETRY_API]

`opentelemetry-api` supplies the runtime observability boundary: the global provider resolution points, the full synchronous/asynchronous metric-instrument set lanes and receipts record against, the W3C trace-context and baggage propagation surface that crosses the lane/RPC seam, and the context attach/detach primitives. Library code imports only this API tier; the SDK provider is installed once at startup and never referenced from runtime owners, so every instrument and propagator resolves through a proxy that forwards to a no-op until that registration lands.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-api`
- package: `opentelemetry-api`
- module: `opentelemetry`
- owner: `runtime`
- rail: observability
- asset: pure-Python runtime library
- namespaces: `opentelemetry.metrics`, `opentelemetry.trace`, `opentelemetry.context`, `opentelemetry.propagate`, `opentelemetry.baggage`, `opentelemetry.propagators.textmap`, `opentelemetry.propagators.composite`, `opentelemetry.trace.propagation.tracecontext`, `opentelemetry.baggage.propagation`, `opentelemetry._logs`, `opentelemetry._events`, `opentelemetry.util.types`
- installed: `1.43.0`
- capability: global meter/tracer/logger/propagator resolution, synchronous + observable metric instruments, the full span lifecycle (events/links/status/exception recording), W3C trace-context + baggage propagation with composite/tracecontext propagators, span-context value types (`SpanContext`/`TraceFlags`/`TraceState`/`Link`), context scope tokens, and the experimental logs/events signal API tiers

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: synchronous metric instruments
- rail: observability

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [RAIL]                                       |
| :-----: | :-------------- | :------------ | :------------------------------------------- |
|  [01]   | `Meter`         | abstract      | instrument-creation contract per scope       |
|  [02]   | `Counter`       | abstract      | monotonically increasing count               |
|  [03]   | `UpDownCounter` | abstract      | bidirectional (additive) count               |
|  [04]   | `Histogram`     | abstract      | value-distribution recording (with buckets)  |
|  [05]   | `Gauge` (exported from `metrics` as `_Gauge`) | abstract | synchronous last-value gauge; the API `Meter.create_gauge` is a no-op stub until the SDK installs the real instrument |

[PUBLIC_TYPE_SCOPE]: asynchronous (observable) instruments
- rail: observability
- read state through registered callbacks returning `Iterable[Observation]`; never `.add`/`.record` directly.

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [RAIL]                                  |
| :-----: | :---------------------------- | :------------ | :-------------------------------------- |
|  [01]   | `ObservableCounter`           | abstract      | async monotonic count via callback      |
|  [02]   | `ObservableUpDownCounter`     | abstract      | async bidirectional count via callback  |
|  [03]   | `ObservableGauge`             | abstract      | async current value via callback        |
|  [04]   | `Observation`                 | value         | `(value, attributes)` callback yield    |
|  [05]   | `CallbackOptions`             | value         | timeout hint passed into async callbacks |

[PUBLIC_TYPE_SCOPE]: context, trace, and propagation
- rail: observability

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [RAIL]                                          |
| :-----: | :------------------------ | :------------ | :---------------------------------------------- |
|  [01]   | `context.Context`         | value         | immutable propagation context map               |
|  [02]   | `context.Token`           | value         | context-attachment token for scoped detach      |
|  [03]   | `trace.Tracer`            | abstract      | span factory (`start_span`, `start_as_current_span`) |
|  [04]   | `trace.Span`              | abstract      | active span (`set_attribute`/`set_attributes`, `add_event`, `add_link`, `update_name`, `set_status`, `record_exception`, `get_span_context`, `is_recording`, `end`) |
|  [05]   | `trace.NonRecordingSpan`  | concrete      | span wrapper carrying only a `SpanContext`; the propagation/no-op span when no SDK records |
|  [06]   | `trace.SpanContext`       | value         | immutable `(trace_id, span_id, is_remote, trace_flags, trace_state)` identity tuple; `is_valid` (property `-> bool`) gates the trace-context log/correlation injector before reading the ids |
|  [07]   | `trace.SpanKind`          | enum          | `CLIENT`/`SERVER`/`INTERNAL`/`PRODUCER`/`CONSUMER` |
|  [08]   | `trace.Link`              | value         | causal link to another `SpanContext` with attributes |
|  [09]   | `trace.TraceFlags`        | `int` subclass | W3C sampled-bit flags (`DEFAULT`/`SAMPLED`)    |
|  [10]   | `trace.TraceState`        | `Mapping`     | W3C `tracestate` vendor key/value list (immutable copy-on-write `add`/`update`) |
|  [11]   | `trace.Status` / `StatusCode` | value     | span status (`OK`/`ERROR`/`UNSET`)              |
|  [12]   | `trace.INVALID_SPAN` / `INVALID_SPAN_CONTEXT` / `INVALID_TRACE_ID` / `INVALID_SPAN_ID` | constant | sentinels marking an absent/invalid span or id |
|  [13]   | `propagators.textmap.TextMapPropagator` | abstract | carrier inject/extract contract; `Setter`/`Getter` carrier accessor protocols |
|  [14]   | `propagators.composite.CompositePropagator` | concrete | fan-out propagator chaining trace-context + baggage over one carrier |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider resolution and meter
- rail: observability
- runtime owners call only the `get_*` getters; the SDK installs providers via `set_*` at the composition root.

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY] | [RAIL]                              |
| :-----: | :--------------------------------------------------------------------- | :------------- | :---------------------------------- |
|  [01]   | `metrics.get_meter(name, version="", meter_provider=None, schema_url=None, attributes=None)` | meter | obtain an instrumentation meter     |
|  [02]   | `metrics.get_meter_provider()` / `metrics.set_meter_provider(p)`       | provider       | global meter provider get/set       |
|  [03]   | `trace.get_tracer(name, version="", tracer_provider=None, schema_url=None, attributes=None)` | tracer | obtain a tracer for span emission   |
|  [04]   | `trace.get_tracer_provider()` / `trace.set_tracer_provider(p)`         | provider       | global tracer provider get/set      |

[ENTRYPOINT_SCOPE]: instrument creation and recording
- rail: observability
- create instruments once per meter at owner construction; record against the cached instrument.

| [INDEX] | [SURFACE]                                                                  | [ENTRY_FAMILY] | [RAIL]                              |
| :-----: | :------------------------------------------------------------------------- | :------------- | :---------------------------------- |
|  [01]   | `Meter.create_counter(name, unit="", description="")`                      | instrument     | monotonic counter                   |
|  [02]   | `Meter.create_up_down_counter(name, unit, description)`                    | instrument     | bidirectional counter               |
|  [03]   | `Meter.create_histogram(name, unit, description, explicit_bucket_boundaries_advisory=None)` | instrument | histogram recorder         |
|  [04]   | `Meter.create_gauge(name, unit, description)`                              | instrument     | synchronous last-value gauge        |
|  [05]   | `Meter.create_observable_counter(name, callbacks, unit, description)`      | instrument     | async monotonic counter             |
|  [06]   | `Meter.create_observable_up_down_counter(name, callbacks, unit, description)` | instrument  | async bidirectional counter         |
|  [07]   | `Meter.create_observable_gauge(name, callbacks, unit, description)`        | instrument     | async gauge                         |
|  [08]   | `Counter.add(amount, attributes=None, context=None)`                       | record         | increment counter                   |
|  [09]   | `UpDownCounter.add(amount, attributes=None, context=None)`                 | record         | adjust bidirectional counter        |
|  [10]   | `Histogram.record(amount, attributes=None, context=None)`                  | record         | record a histogram measurement      |
|  [11]   | `Gauge.set(amount, attributes=None, context=None)`                         | record         | set the synchronous gauge value     |

[ENTRYPOINT_SCOPE]: span lifecycle and trace helpers
- rail: observability
- `start_as_current_span` is the default context-manager span; `start_span` returns a detached span the caller activates with `use_span`. `get_current_span`/`set_span_in_context` bridge the active span and the propagation `Context`.

| [INDEX] | [SURFACE]                                                                  | [ENTRY_FAMILY] | [RAIL]                              |
| :-----: | :------------------------------------------------------------------------- | :------------- | :---------------------------------- |
|  [01]   | `Tracer.start_span(name, context=None, kind=SpanKind.INTERNAL, attributes=None, links=None, start_time=None, record_exception=True, set_status_on_exception=True)` | span | create a detached, unactivated span |
|  [02]   | `Tracer.start_as_current_span(name, context=None, kind=..., attributes=None, links=None, start_time=None, record_exception=True, set_status_on_exception=True, end_on_exit=True)` | span | context-manager span activated and ended automatically |
|  [03]   | `Span.set_attribute(key, value)` / `Span.set_attributes(mapping)`          | span mutate    | annotate the active span            |
|  [04]   | `Span.add_event(name, attributes=None, timestamp=None)`                    | span event     | timestamped event on the span       |
|  [05]   | `Span.add_link(context, attributes=None)`                                  | span link      | attach a causal `SpanContext` link  |
|  [06]   | `Span.set_status(status, description=None)` / `Span.record_exception(exc, attributes=None, timestamp=None, escaped=False)` | span status | mark status / record an exception event |
|  [07]   | `Span.update_name(name)` / `Span.get_span_context()` / `Span.is_recording()` / `Span.end(end_time=None)` | span lifecycle | rename, read context, gate recording, finalize |
|  [08]   | `trace.use_span(span, end_on_exit=False, record_exception=True, set_status_on_exception=True)` | activate | context-manage a detached span as current |
|  [09]   | `trace.get_current_span(context=None)` / `trace.set_span_in_context(span, context=None)` | bridge | read active span / embed a span into a `Context` |
|  [10]   | `trace.format_trace_id(trace_id)` / `trace.format_span_id(span_id)`        | format         | canonical lowercase-hex W3C id rendering |

[ENTRYPOINT_SCOPE]: context and propagation
- rail: observability

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :--------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `context.attach(context) -> Token`                         | context        | activate a context, return detach token      |
|  [02]   | `context.detach(token)`                                    | context        | restore the previous context                 |
|  [03]   | `context.get_current() -> Context`                         | context        | snapshot the active context                  |
|  [04]   | `context.create_key(name)` / `get_value(key, ctx)` / `set_value(key, value, ctx)` | context | custom context-key carrier ops        |
|  [05]   | `propagate.inject(carrier, context=None, setter=default_setter)` | propagation | encode trace-context + baggage into a carrier via the `Setter` |
|  [06]   | `propagate.extract(carrier, context=None, getter=default_getter) -> Context` | propagation | decode a `Context` from an inbound carrier via the `Getter` |
|  [07]   | `propagate.get_global_textmap()` / `set_global_textmap(p)` | propagation    | global composite propagator get/set          |
|  [08]   | `propagators.textmap.default_setter` / `default_getter` (`DefaultSetter`/`DefaultGetter`) | carrier accessor | mapping-backed read/write accessors for non-dict carriers |
|  [09]   | `trace.propagation.tracecontext.TraceContextTextMapPropagator()`  | propagator     | W3C `traceparent`/`tracestate` codec        |
|  [10]   | `baggage.propagation.W3CBaggagePropagator()`               | propagator     | W3C `baggage` header codec                   |

[ENTRYPOINT_SCOPE]: baggage
- rail: observability
- baggage rides the same W3C carrier as trace context and crosses the lane/RPC seam paired with `inject`/`extract`.

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [RAIL]                              |
| :-----: | :---------------------------------------------- | :------------- | :---------------------------------- |
|  [01]   | `baggage.set_baggage(name, value, context=None) -> Context` | baggage | attach a key/value to the context   |
|  [02]   | `baggage.get_baggage(name, context=None)`       | baggage        | read one baggage value              |
|  [03]   | `baggage.get_all(context=None) -> Mapping`      | baggage        | read the full baggage map           |
|  [04]   | `baggage.remove_baggage(name, context=None)` / `clear(context=None)` | baggage | drop one / all baggage entries |

[ENTRYPOINT_SCOPE]: logs and events signal tier (experimental)
- rail: observability
- `_logs`/`_events` are the underscore-prefixed experimental signal APIs; same proxy-until-install discipline as metrics/trace. The runtime emits structured records here only where a log/event signal is genuinely first-class, not as a print replacement.

| [INDEX] | [SURFACE]                                                                  | [ENTRY_FAMILY] | [RAIL]                              |
| :-----: | :------------------------------------------------------------------------- | :------------- | :---------------------------------- |
|  [01]   | `_logs.get_logger(name, version=None, schema_url=None, attributes=None)`   | logger         | obtain a `Logger` for `LogRecord` emission |
|  [02]   | `_logs.get_logger_provider()` / `set_logger_provider(p)`                   | provider       | global logger provider get/set      |
|  [03]   | `Logger.emit(record: LogRecord)` / `_logs.LogRecord(...)` / `_logs.SeverityNumber` | record  | emit a structured log record with severity |
|  [04]   | `_events.get_event_logger(name, version=None, schema_url=None, attributes=None)` | event logger | obtain an `EventLogger`        |
|  [05]   | `EventLogger.emit(event: Event)` / `_events.Event(name, ...)`              | event          | emit a named, structured event (a `LogRecord` subtype) |

## [04]-[IMPLEMENTATION_LAW]

[OBSERVABILITY_TOPOLOGY]:
- proxy-until-install: `metrics.get_meter`/`trace.get_tracer` return a proxy meter/tracer that forwards to a no-op implementation until the SDK provider is installed at startup; runtime owners never call `set_meter_provider`/`set_tracer_provider`. Once the SDK installs, the proxies retroactively bind to the real provider, so cached instruments created before install continue working.
- instrument selection: `Counter` for monotonic totals, `UpDownCounter` for additive in-flight gauges (queue depth, active lanes), `Histogram` for latency/size distributions, synchronous `Gauge` for last-write sampled values, and the three `Observable*` mirrors for state polled on the SDK collection cycle. One instrument is created per meter at owner construction and reused; never a per-request or per-lane instrument.
- async instruments: `ObservableCounter`/`ObservableUpDownCounter`/`ObservableGauge` read state through registered callbacks that return `Iterable[Observation]` and accept `CallbackOptions`; callbacks run on the SDK collection cycle and must be non-blocking.
- span lifecycle: `start_as_current_span` is the default — it activates and ends the span across a `with` block, recording exceptions and setting `StatusCode.ERROR` automatically. `start_span` returns a detached span the caller activates with `use_span` when span scope and code scope diverge (e.g. a span spanning an async boundary). `add_event` records a timestamped event, `add_link` attaches a causal `SpanContext`, `set_status`/`record_exception` mark outcome, and `get_span_context()` yields the `SpanContext` injected onto an outbound carrier. `is_recording()` gates expensive attribute computation when no SDK records.
- span-context value law: `SpanContext` is the immutable `(trace_id, span_id, is_remote, trace_flags, trace_state)` identity; `TraceFlags` is an `int` subclass exposing the W3C sampled bit (`SAMPLED`), and `TraceState` is a copy-on-write vendor key/value `Mapping`. `INVALID_SPAN`/`INVALID_SPAN_CONTEXT` are the sentinels returned when no span is active; `format_trace_id`/`format_span_id` render canonical lowercase hex. A `NonRecordingSpan` wraps a remote `SpanContext` extracted on the inbound hop so the trace continues without local recording.
- propagation seam: `propagate.inject(carrier, context, setter)` writes the W3C `traceparent`/`tracestate`/`baggage` headers into a `dict[str, str]` carrier on the outbound lane/RPC hop via the `Setter`; `propagate.extract(carrier, context, getter)` rebuilds the `Context` on the inbound hop via the `Getter`. The global propagator is a `CompositePropagator` chaining `TraceContextTextMapPropagator` + `W3CBaggagePropagator`; baggage is the cross-hop key/value channel layered on the same carrier. A non-`dict` carrier (gRPC metadata, header tuple list) is read/written through a custom `Setter`/`Getter` rather than copied into a dict.
- context scope: `context.attach(context)` returns a `Token` paired with `context.detach(token)`; attach/detach bracket the active span scope across a lane boundary and never leak across hops. `get_current`/`get_value`/`set_value` read and thread custom context keys created by `create_key`.

[LOCAL_ADMISSION]:
- runtime owners import only from `opentelemetry-api`; SDK construction and `set_*_provider` live in the application composition root.
- propagation carriers are plain `dict[str, str]`; the lane/RPC seam injects on send and extracts on receive, pairing every `inject` with a corresponding `extract`, and the carrier is the same `dict` the `grpc.aio` interceptor reads (see `.api/opentelemetry-instrumentation-grpc.md`).
- metric instruments are owner-held singletons; lanes/metrics/receipt owners record against the cached instrument, never re-create one per call. `UpDownCounter` backs in-flight-lane gauges and `Histogram` backs per-stage latency, both keyed by attribute dicts, never a per-value instrument.
- integration rail: a `stamina` `retry_context` runs inside a span obtained from `trace.get_tracer(...).start_as_current_span`, the retry attempt count increments a cached `Counter`, and a `UpDownCounter` tracks live attempts; the W3C context (trace + baggage) is injected into the gRPC metadata carrier so the server leg's `opentelemetry-instrumentation-grpc` interceptor continues the same trace.
- every `context.attach` is balanced by a `context.detach` of the returned token in the same scope.

[RAIL_LAW]:
- Package: `opentelemetry-api`
- Owns: global meter/tracer/logger/propagator resolution, the full synchronous + observable metric-instrument set, the span lifecycle (events/links/status/exception recording) and span-context value types, W3C trace-context + baggage propagation with composite/tracecontext propagators, context scope tokens, and the experimental logs/events signal tier at the API boundary
- Accept: `get_meter`/`get_tracer` + cached instrument recording, `Counter`/`UpDownCounter`/`Histogram`/`Gauge` and their `Observable*` mirrors, `start_as_current_span`/`start_span` + `use_span` with `add_event`/`add_link`/`set_status`/`record_exception`, `SpanContext`/`TraceFlags`/`TraceState`/`Link` value types, `propagate.inject`/`extract` through a `CompositePropagator` (with custom `Setter`/`Getter` for non-dict carriers) + baggage at the lane/RPC seam, token-paired `context.attach`/`detach`, custom context keys via `create_key`/`get_value`/`set_value`, `_logs`/`_events` only where a log/event signal is first-class
- Reject: SDK imports in runtime owners, per-request instrument creation, unbalanced attach/detach, `set_*_provider` outside the composition root, recording directly on an `Observable*` instrument, a hand-rolled `traceparent`/`baggage` header parser where the propagators apply, copying a non-dict carrier into a dict instead of supplying a `Setter`/`Getter`, computing span attributes without an `is_recording()` gate on hot paths
