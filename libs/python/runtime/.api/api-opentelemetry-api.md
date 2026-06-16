# [PY_RUNTIME_API_OPENTELEMETRY_API]

`opentelemetry-api` supplies the vendor-neutral telemetry contracts: tracer/span/span-context, the metrics instrument family, the logs bridge, context propagation and baggage, and the no-op default providers. It is the runtime owner for emitting trace/metric/log signals against the abstract API; concrete export composition lives at the host.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-api`
- package: `opentelemetry-api`
- import: `opentelemetry`
- version: `1.42.1`
- owner: `runtime`
- rail: observability
- namespaces: `opentelemetry.trace`, `opentelemetry.metrics`, `opentelemetry._logs`, `opentelemetry.context`, `opentelemetry.baggage`, `opentelemetry.propagate`
- capability: tracer/span contracts, metrics instruments, logs bridge, context propagation, baggage, no-op default providers

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: trace family
- rail: observability

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| :-----: | :------- | :------------ | :----- |
| [1] | `trace.TracerProvider` | provider | tracer factory contract |
| [2] | `trace.Tracer` | tracer | span factory |
| [3] | `trace.Span` | span | trace span contract |
| [4] | `trace.SpanContext` | context | span identity/flags |
| [5] | `trace.SpanKind` | enum | span role classification |
| [6] | `trace.Status` | status | span status value |
| [7] | `trace.StatusCode` | enum | span status code |
| [8] | `trace.Link` | link | inter-span link |
| [9] | `trace.TraceFlags` | flags | sampling flags |
| [10] | `trace.TraceState` | state | vendor trace state |

[PUBLIC_TYPE_SCOPE]: metrics and logs family
- rail: observability

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| :-----: | :------- | :------------ | :----- |
| [1] | `metrics.MeterProvider` | provider | meter factory contract |
| [2] | `metrics.Meter` | meter | instrument factory |
| [3] | `metrics.Counter` | instrument | monotonic counter |
| [4] | `metrics.UpDownCounter` | instrument | bidirectional counter |
| [5] | `metrics.Histogram` | instrument | value distribution |
| [6] | `metrics.ObservableGauge` | instrument | callback gauge |
| [7] | `metrics.Observation` | value | callback measurement |
| [8] | `_logs.LoggerProvider` | provider | logger factory contract |
| [9] | `_logs.Logger` | logger | log-record emitter |
| [10] | `_logs.LogRecord` | record | structured log record |
| [11] | `_logs.SeverityNumber` | enum | log severity |

[PUBLIC_TYPE_SCOPE]: context and propagation family
- rail: observability

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| :-----: | :------- | :------------ | :----- |
| [1] | `context.Context` | context | immutable context map |
| [2] | `baggage` | baggage | cross-cutting key/value carrier |
| [3] | `propagate` | propagator | inject/extract context across wire |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: telemetry operations
- rail: observability

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
| :-----: | :-------- | :------------- | :----- |
| [1] | `trace.get_tracer` | tracer | obtain a named tracer |
| [2] | `Tracer.start_as_current_span` | span | scoped active span |
| [3] | `trace.get_current_span` | span | active span read |
| [4] | `trace.use_span` | span | activate an existing span |
| [5] | `metrics.get_meter` | meter | obtain a named meter |
| [6] | `Meter.create_counter` | instrument | build a counter |
| [7] | `Meter.create_histogram` | instrument | build a histogram |
| [8] | `Meter.create_observable_gauge` | instrument | build a callback gauge |
| [9] | `context.attach` / `context.detach` | context | activate/restore context |
| [10] | `baggage.set_baggage` | baggage | set a baggage entry |
| [11] | `baggage.get_baggage` | baggage | read a baggage entry |
| [12] | `propagate.inject` | propagation | write context into a carrier |
| [13] | `propagate.extract` | propagation | read context from a carrier |

## [4]-[IMPLEMENTATION_LAW]

[OBSERVABILITY_TOPOLOGY]:
- emission law: the runtime emits against the API contracts only — `get_tracer`/`get_meter` with module names and a service identity; it never instantiates a concrete `TracerProvider`/`MeterProvider`, which the SDK and host own.
- span law: spans are `start_as_current_span` blocks carrying the correlation identity as an attribute; manual `start_span`/`end` pairs are used only when the span crosses an async boundary.
- correlation law: the runtime correlation id flows through `baggage`/`propagate` so the companion seam carries it across the wire; it is the same identity structlog renders and the receipt owner records.
- metrics law: counters/histograms are created once per meter and reused; instruments are not rebuilt per call.
- no-op law: with no provider set the API returns no-op providers; the runtime stays importable and silent without an exporter — the host installs the provider.

[LOCAL_ADMISSION]:
- The runtime contributes telemetry signals through the API; the SDK provider, processors, and exporter compose at `Rasm.AppHost`, never inside the runtime.
- Baggage carries only the correlation/deadline identity the context owner defines, never arbitrary payload.

[RAIL_LAW]:
- Package: `opentelemetry-api`
- Owns: trace/metric/log emission against the vendor-neutral contracts, context propagation, and baggage
- Accept: `get_tracer`/`get_meter`, scoped spans, reused instruments, `propagate`/`baggage` for correlation
- Reject: concrete provider instantiation in the runtime, per-call instrument rebuilding, arbitrary baggage payloads
