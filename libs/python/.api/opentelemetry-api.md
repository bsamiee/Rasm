# [PY_BRANCH_API_OPENTELEMETRY_API]

`opentelemetry-api` owns the branch observability contracts: tracer, meter, and logger provider surfaces as abstract classes with no-op implementations, context propagation, baggage, and the `TextMapPropagator` carrier codec every service seam binds. Real telemetry stays with `opentelemetry-sdk` — library code resolves the global providers and emits no-ops until the SDK installs real ones at the composition root.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-api`
- package: `opentelemetry-api` (Apache-2.0)
- module: `opentelemetry`
- namespaces: `opentelemetry.trace`, `opentelemetry.metrics`, `opentelemetry._logs`, `opentelemetry.context`, `opentelemetry.propagate`, `opentelemetry.propagators.textmap`, `opentelemetry.baggage`, `opentelemetry.attributes`
- rail: observability

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: trace family

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                                 |
| :-----: | :--------------------- | :------------ | :----------------------------------------------------------- |
|  [01]   | `TracerProvider`       | abstract      | tracer factory contract                                      |
|  [02]   | `Tracer`               | abstract      | span creation contract                                       |
|  [03]   | `Span`                 | abstract      | active span operation contract                               |
|  [04]   | `SpanContext`          | value         | immutable trace/span/flag triple with `is_valid`/`is_remote` |
|  [05]   | `StatusCode`           | enum          | `UNSET`, `OK`, `ERROR`                                       |
|  [06]   | `Status`               | value         | status code plus optional message                            |
|  [07]   | `SpanKind`             | enum          | `INTERNAL`, `SERVER`, `CLIENT`, `PRODUCER`, `CONSUMER`       |
|  [08]   | `TraceFlags`           | value         | sampled and parent flags bitfield                            |
|  [09]   | `TraceState`           | value         | W3C trace state key-value list                               |
|  [10]   | `Link`                 | value         | cross-trace span link                                        |
|  [11]   | `NonRecordingSpan`     | no-op         | API-only no-op span                                          |
|  [12]   | `NoOpTracer`           | no-op         | no-op tracer for API-only use                                |
|  [13]   | `NoOpTracerProvider`   | no-op         | no-op tracer provider                                        |
|  [14]   | `INVALID_SPAN_CONTEXT` | constant      | zeroed context returned wherever no valid span resolves      |
|  [15]   | `INVALID_SPAN`         | constant      | `NonRecordingSpan` over `INVALID_SPAN_CONTEXT`               |
|  [16]   | `INVALID_TRACE_ID`     | constant      | zero trace id, exclusive lower bound of the valid range      |
|  [17]   | `INVALID_SPAN_ID`      | constant      | zero span id, exclusive lower bound of the valid range       |

[PUBLIC_TYPE_SCOPE]: metrics family

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                                            |
| :-----: | :------------------------ | :------------ | :-------------------------------------------------------------------------------------- |
|  [01]   | `MeterProvider`           | abstract      | meter factory contract                                                                  |
|  [02]   | `Meter`                   | abstract      | instrument creation contract                                                            |
|  [03]   | `Counter`                 | abstract      | monotonically increasing count                                                          |
|  [04]   | `UpDownCounter`           | abstract      | bidirectional count                                                                     |
|  [05]   | `Histogram`               | abstract      | value distribution recording                                                            |
|  [06]   | `_Gauge`                  | abstract      | synchronous last-value gauge; `__all__` spells it `_Gauge` and `Gauge` resolves nowhere |
|  [07]   | `ObservableCounter`       | abstract      | async monotonic count                                                                   |
|  [08]   | `ObservableGauge`         | abstract      | async current value                                                                     |
|  [09]   | `ObservableUpDownCounter` | abstract      | async bidirectional count                                                               |
|  [10]   | `Observation`             | value         | (value, attributes) for async                                                           |
|  [11]   | `CallbackOptions`         | value         | timeout hint for async callbacks                                                        |

[PUBLIC_TYPE_SCOPE]: logs and context family

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                                              |
| :-----: | :------------------------------ | :------------ | :-------------------------------------------------------- |
|  [01]   | `LoggerProvider`                | abstract      | logger factory contract                                   |
|  [02]   | `Logger`                        | abstract      | log record emission contract                              |
|  [03]   | `LogRecord`                     | value         | structured log record carrier                             |
|  [04]   | `SeverityNumber`                | enum          | `UNSPECIFIED` then five four-wide bands `TRACE`..`FATAL4` |
|  [05]   | `Context`                       | value         | immutable propagation context                             |
|  [06]   | `Token`                         | value         | context attachment token                                  |
|  [07]   | `TextMapPropagator`             | abstract      | carrier-based context propagator                          |
|  [08]   | `Getter[CarrierT]`              | protocol      | generic carrier read protocol (`get`/`keys`)              |
|  [09]   | `Setter[CarrierT]`              | protocol      | generic carrier write protocol (`set`)                    |
|  [10]   | `DefaultGetter`                 | value         | default getter; reads list-valued headers, returns first  |
|  [11]   | `DefaultSetter`                 | value         | default setter for dict-like header carriers              |
|  [12]   | `CarrierT`                      | type var      | carrier type parameter for `Getter`/`Setter`              |
|  [13]   | `TraceContextTextMapPropagator` | concrete      | W3C `traceparent`/`tracestate` codec                      |
|  [14]   | `W3CBaggagePropagator`          | concrete      | W3C `baggage` header codec                                |
|  [15]   | `CompositePropagator`           | concrete      | fan-out chaining trace-context + baggage over one carrier |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: trace API

| [INDEX] | [SURFACE]                                                             | [SHAPE]        | [CAPABILITY]                             |
| :-----: | :-------------------------------------------------------------------- | :------------- | :--------------------------------------- |
|  [01]   | `trace.get_tracer_provider()`                                         | provider       | global tracer provider                   |
|  [02]   | `trace.set_tracer_provider(tracer_provider)`                          | provider       | install global tracer provider           |
|  [03]   | `trace.get_tracer(name, version, ...)`                                | tracer         | obtain instrumentation tracer            |
|  [04]   | `trace.get_current_span(context)`                                     | span           | current active span                      |
|  [05]   | `trace.set_span_in_context(span, context)`                            | context        | embed span into context                  |
|  [06]   | `trace.use_span(span, end_on_exit, ...)`                              | context mgr    | span context manager                     |
|  [07]   | `trace.format_trace_id(trace_id)`                                     | utility        | hex-format trace id                      |
|  [08]   | `trace.format_span_id(span_id)`                                       | utility        | hex-format span id                       |
|  [09]   | `Tracer.start_span(name, context, kind, ...)`                         | span creation  | create detached span                     |
|  [10]   | `Tracer.start_as_current_span(name, ...)`                             | span creation  | create and activate span                 |
|  [11]   | `Span.set_attribute(key, value)`                                      | span mutation  | attach one typed attribute               |
|  [12]   | `Span.set_attributes(attributes)`                                     | span mutation  | attach a mapping of attributes at once   |
|  [13]   | `Span.add_event(name, attributes, timestamp)`                         | span mutation  | record timestamped event                 |
|  [14]   | `Span.add_link(context, attributes)`                                  | span mutation  | link to another span context post-start  |
|  [15]   | `Span.set_status(status, description)`                                | span mutation  | set span outcome status                  |
|  [16]   | `Span.record_exception(exception, attributes, timestamp, escaped)`    | span mutation  | record exception on span                 |
|  [17]   | `Span.update_name(name)`                                              | span mutation  | rename span after start                  |
|  [18]   | `Span.is_recording()` / `Span.get_span_context()`                     | span query     | recording flag / immutable `SpanContext` |
|  [19]   | `Span.end(end_time)`                                                  | span lifecycle | end and finalize span                    |
|  [20]   | `SpanContext.is_valid`                                                | span query     | precomputed id-range validity flag       |
|  [21]   | `SpanContext.is_remote`                                               | span query     | remote-origin flag fixed at construction |
|  [22]   | `SpanContext(trace_id, span_id, is_remote, trace_flags, trace_state)` | construction   | construct immutable span context         |

- `SpanContext(trace_id: int, span_id: int, is_remote: bool, trace_flags=DEFAULT_TRACE_OPTIONS, trace_state=DEFAULT_TRACE_STATE)` takes the remote flag as a POSITIONAL third argument, so a synthesized parent declares its own provenance at construction and nothing downstream re-decides it

[ENTRYPOINT_SCOPE]: metrics API

| [INDEX] | [SURFACE]                                                  | [SHAPE]    | [CAPABILITY]                            |
| :-----: | :--------------------------------------------------------- | :--------- | :-------------------------------------- |
|  [01]   | `metrics.get_meter_provider()`                             | provider   | global meter provider                   |
|  [02]   | `metrics.set_meter_provider(meter_provider)`               | provider   | install global meter provider           |
|  [03]   | `metrics.get_meter(name, version, ...)`                    | meter      | obtain instrumentation meter            |
|  [04]   | `Meter.create_counter(name, unit, description)`            | instrument | create monotonic counter                |
|  [05]   | `Meter.create_up_down_counter(name, ...)`                  | instrument | create bidirectional counter            |
|  [06]   | `Meter.create_histogram(name, ...)`                        | instrument | create histogram recorder               |
|  [07]   | `Meter.create_gauge(name, unit, description)`              | instrument | create synchronous last-value gauge     |
|  [08]   | `Meter.create_observable_counter(name, callbacks)`         | instrument | create async observable counter         |
|  [09]   | `Meter.create_observable_gauge(name, callbacks)`           | instrument | create async observable gauge           |
|  [10]   | `Meter.create_observable_up_down_counter(name, callbacks)` | instrument | create async observable up-down counter |
|  [11]   | `Counter.add(amount, attributes, context)`                 | record     | increment counter                       |
|  [12]   | `UpDownCounter.add(amount, attributes, context)`           | record     | adjust bidirectional counter            |
|  [13]   | `Histogram.record(amount, attributes, context)`            | record     | record histogram measurement            |
|  [14]   | `_Gauge.set(amount, attributes, context)`                  | record     | set synchronous gauge value             |

- `metrics.get_meter(name, version="", meter_provider=None, schema_url=None, attributes=None)` defaults its version slot to an EMPTY STRING, while the `MeterProvider.get_meter` it forwards to defaults the same slot to `None` — one meter name reached through the module helper and through a held provider therefore carries two different unset versions.
- `trace.get_tracer` defaults that slot to `None` on the module helper and the provider method alike, so the metric leg is the one carrying the divergence and a version-stamped call site is what closes it.

[ENTRYPOINT_SCOPE]: context and propagation
- W3C propagators import from `opentelemetry.trace.propagation.tracecontext`, `opentelemetry.baggage.propagation`, and `opentelemetry.propagators.composite`

| [INDEX] | [SURFACE]                                                          | [SHAPE]     | [CAPABILITY]                                  |
| :-----: | :----------------------------------------------------------------- | :---------- | :-------------------------------------------- |
|  [01]   | `context.get_current()`                                            | context     | current active context                        |
|  [02]   | `context.attach(context)`                                          | context     | activate context, return token                |
|  [03]   | `context.detach(token)`                                            | context     | restore previous context                      |
|  [04]   | `context.get_value(key, context)`                                  | context     | read value from context                       |
|  [05]   | `context.set_value(key, value, context)`                           | context     | derive context with new value                 |
|  [06]   | `context.create_key(keyname)`                                      | context     | mint a unique context key string              |
|  [07]   | `baggage.set_baggage(name, value, context)`                        | baggage     | derive context with baggage entry             |
|  [08]   | `baggage.get_baggage(name, context)`                               | baggage     | read one baggage value                        |
|  [09]   | `baggage.get_all(context)`                                         | baggage     | read full baggage mapping                     |
|  [10]   | `baggage.remove_baggage(name, context)` / `baggage.clear(context)` | baggage     | drop one / all baggage entries                |
|  [11]   | `propagate.extract(carrier, context, getter)`                      | propagation | decode context from carrier (custom `Getter`) |
|  [12]   | `propagate.inject(carrier, context, setter)`                       | propagation | encode context into carrier (custom `Setter`) |
|  [13]   | `propagate.get_global_textmap()`                                   | propagation | global composite propagator                   |
|  [14]   | `propagate.set_global_textmap(propagator)`                         | propagation | install composite propagator                  |
|  [15]   | `TraceContextTextMapPropagator()`                                  | propagator  | construct the W3C trace-context codec         |
|  [16]   | `W3CBaggagePropagator()`                                           | propagator  | construct the W3C baggage codec               |
|  [17]   | `CompositePropagator(propagators)`                                 | propagator  | chain propagators via `set_global_textmap`    |

[ENTRYPOINT_SCOPE]: logs API
- `_logs.get_logger(instrumenting_module_name, instrumenting_library_version="", logger_provider=None, schema_url=None, attributes=None)` carries the whole scope coordinate, so a log scope version-stamps and schema-pins exactly as `get_tracer`/`get_meter` do
- `Logger.emit(record=None, *, timestamp=None, observed_timestamp=None, context=None, severity_number=None, severity_text=None, body=None, attributes=None, event_name=None, exception=None)` — its keyword form builds the record itself, so a producer hands structured attributes and a typed severity with no `LogRecord` construction

| [INDEX] | [SURFACE]                                    | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `_logs.get_logger_provider()`                | provider | global logger provider                 |
|  [02]   | `_logs.set_logger_provider(logger_provider)` | provider | install global logger provider         |
|  [03]   | `_logs.get_logger(...)`                      | logger   | obtain a scoped instrumentation logger |
|  [04]   | `Logger.emit(record=None, **fields)`         | emit     | emit a record or build one from fields |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `get_tracer_provider()`/`get_meter_provider()`/`get_logger_provider()` return no-ops until `set_*_provider` installs the SDK; library code never calls `set_*`
- `LogRecord` resolves its own correlation: an absent `context` falls to the ambient one and `trace_id`/`span_id`/`trace_flags` read off the active span there, so `context` is the one admitted spelling and the three explicit slots carry a `@deprecated` marker — a producer copying them into `attributes` renders one fact twice on the wire
- `Logger` handles mint per `get_logger` call under no SDK cache, so a per-emit mint allocates on the hot path; memoize the handle per provider identity, never per call, and a pre-install no-op provider is a distinct object so the memo upgrades at install with no invalidation
- `Context` is immutable and contextvars-backed; `attach`/`detach` are scoped and token-paired
- `start_span` returns a detached span; `start_as_current_span` activates and ends the span as a context manager
- `is_valid` computes ONCE inside `__new__` as `INVALID_TRACE_ID < trace_id <= _TRACE_ID_MAX_VALUE and INVALID_SPAN_ID < span_id <= _SPAN_ID_MAX_VALUE` and stores as tuple field 5, so a consumer reads the flag and never re-derives the range check; `SpanContext` subclasses `tuple` and NOT `NamedTuple`, so it carries no `_fields` and a structural walk indexes positionally
- `trace.get_current_span(context)` over a context holding no span returns `INVALID_SPAN` — `NonRecordingSpan` over `INVALID_SPAN_CONTEXT`, `is_valid` False and `is_recording()` False — so an absent parent surfaces as an invalid context rather than `None` and every caller branches on the flag
- span attribute values are `str | bool | int | float | Sequence[str | bool | int | float]` under non-empty string keys; `set_attributes` binds a mapping in one call
- log-record attributes take the WIDER `_ExtendedAttributes`/`AnyValue` shape on `Logger.emit(attributes=)` and `get_logger(attributes=)` — `None`, `bytes`, and nested mapping and sequence values survive the record cleaner where a span attribute of that shape drops with a warning — so a producer projects a whole structured event onto one record rather than flattening it; nesting depth, collection width, and byte length stay its own bounds, since that cleaner truncates `str` alone
- extended sequences stay HOMOGENEOUS or vanish: `_clean_extended_attribute_value` types a sequence off its first non-null element and nulls the WHOLE value at the first element of another type — `bool` beside `int` reaches that bar — while a mapping carries no such rule, so a mixed collection projects onto an index-keyed mapping or vanishes unread; `None` elements never type the sequence, `bytes` survive undecoded here, and a value outside `AnyValue` stringifies rather than dropping
- synchronous `Counter`/`UpDownCounter`/`Histogram`/`_Gauge` record imperatively (`add`/`record`/`set`), so a consumer holding a synchronous instrument resolves its write member from the family rather than assuming one spelling; observable instruments pull via callbacks yielding `Observation` under a `CallbackOptions` timeout — one instrument is push or pull, never both
- `get_meter` before the install resolves `_ProxyMeterProvider`, whose `_ProxyMeter` mints a `_Proxy*` instrument per family; each SUBCLASSES the abstract family here, so `isinstance` narrowing and a structural `match` over `Counter | UpDownCounter | Histogram | _Gauge` hold identically on a proxy, and `set_meter_provider` re-mints every registered proxy against the real meter with no consumer invalidation — so an instrument set built at import time stays live and correctly typed across the install
- baggage is cross-process key-value context (`set_baggage` derives a new immutable `Context`), distinct from span-local attributes and never auto-copied onto a span
- `TextMapPropagator` reads via `Getter[CarrierT]` and writes via `Setter[CarrierT]`; one `CompositePropagator([TraceContextTextMapPropagator(), W3CBaggagePropagator()])` chains both W3C codecs over one carrier, installed via `set_global_textmap`
- `propagate.extract` defaults its `context` slot to the AMBIENT context, so a carrier carrying no `traceparent` returns that ambient context unchanged and `get_current_span` off it yields the LIVE LOCAL span (`is_valid` True, `is_remote` False) rather than an invalid one; extraction success therefore proves nothing about origin, and `parent.is_remote` is the one member deciding remote hop against local parent — a call site stamping `remote=True` off a returned context mislabels every local parent

[STACKING]:
- `connectrpc`(`.api/connectrpc.md`): a client `MetadataInterceptor.on_start` stamps the active context via `propagate.inject(ctx.request_headers)`, a server interceptor continues it via `propagate.extract(ctx.request_headers)` then `start_as_current_span(kind=SpanKind.SERVER)`; this surface owns the W3C `traceparent`/`tracestate` encoding, `connectrpc` owns the `Headers` carrier
- `msgspec`(`.api/msgspec.md`): `to_builtins(struct, str_keys=True)` yields exactly the primitive `str | bool | int | float | Sequence[...]` mapping `Span.set_attributes` accepts, annotating a span from a decoded wire struct with no manual flattening
- `structlog`(`.api/structlog.md`): `get_current_span().get_span_context()` yields the `trace_id`/`span_id` that `format_trace_id`/`format_span_id` render as the hex fields a `structlog` processor binds for trace-log correlation
- `opentelemetry-sdk`(`.api/opentelemetry-sdk.md`): this surface emits no-op providers until the SDK installs real ones via `set_*_provider` at the composition root; library code calls `get_tracer`/`get_meter`/`get_logger` and never imports the SDK. That SDK re-exports a same-named class per instrument family — including its own `_Gauge` — that DERIVES from the abstract here, and its temporality and aggregation preference maps admit those SDK classes alone, raising on every class from this surface
- retry rail (`stamina`): a `retry_context` loop wraps each attempt in a child span; a final `RpcError.code()` maps to `Status(StatusCode.ERROR)` via `Span.set_status` + `Span.record_exception`

[LOCAL_ADMISSION]:
- library code imports only from `opentelemetry-api`; SDK imports belong to the composition root
- `connectrpc` `Headers` stacks repeat keys under `add` and returns them from `getall`, so its carrier binds a `Getter` reading `getall` — the default getter takes one `__getitem__` value and drops every later add
- metrics instruments are created once per meter and reused, synchronous or observable per measurement model, never per-request
- `start_as_current_span` is the default; `start_span` is reserved for a span whose activation crosses an async boundary the context manager cannot hold

[RAIL_LAW]:
- Package: `opentelemetry-api`
- Owns: OTel API contracts, context propagation, baggage, global provider resolution, no-op implementations
- Accept: API-only imports in library code, `get_tracer`/`get_meter`/`get_logger` with attribute-bound usage, `propagate.extract`/`inject` over carrier-typed `Getter`/`Setter`, a `CompositePropagator` of `TraceContextTextMapPropagator`/`W3CBaggagePropagator` via `set_global_textmap`, synchronous and observable instruments, cross-process `baggage`
- Reject: SDK imports in library code, per-request instrument creation, in-place `Context` mutation, `set_*_provider` outside the composition root, citing `Gauge` as a top-level import
