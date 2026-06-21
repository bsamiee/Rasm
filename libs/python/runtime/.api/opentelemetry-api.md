# [PY_RUNTIME_API_OPENTELEMETRY_API]

`opentelemetry-api` supplies the runtime observability boundary: the global provider resolution points, the W3C context-propagation surface that crosses the lane/RPC seam, and the metric-instrument contracts that lanes, metrics, telemetry, and receipt owners record against. Library code imports only this API tier; the SDK provider is installed once at startup and never referenced from runtime owners, so every instrument and propagator resolves through a no-op until that registration lands.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-api`
- package: `opentelemetry-api`
- module: `opentelemetry`
- asset: runtime library
- rail: observability
- namespaces: `opentelemetry.metrics`, `opentelemetry.context`, `opentelemetry.propagate`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: metric instruments
- rail: observability

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [RAIL]                           |
| :-----: | :-------------------- | :------------ | :------------------------------- |
|  [01]   | `Meter`               | abstract      | instrument creation contract     |
|  [02]   | `Counter`             | abstract      | monotonically increasing count   |
|  [03]   | `Histogram`           | abstract      | value distribution recording     |
|  [04]   | `ObservableCounter`   | abstract      | async monotonic count            |
|  [05]   | `ObservableGauge`     | abstract      | async current value              |
|  [06]   | `Observation`         | value         | (value, attributes) for async    |
|  [07]   | `CallbackOptions`     | value         | timeout hint for async callbacks |

[PUBLIC_TYPE_SCOPE]: context and propagation
- rail: observability

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY] | [RAIL]                         |
| :-----: | :--------- | :------------ | :----------------------------- |
|  [01]   | `Context`  | value         | immutable propagation context  |
|  [02]   | `Token`    | value         | context attachment token       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: metrics API
- rail: observability

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :------------------------------------------------- | :------------- | :------------------------------ |
|  [01]   | `metrics.get_meter(name, version, ...)`            | meter          | obtain instrumentation meter    |
|  [02]   | `Meter.create_counter(name, unit, description)`    | instrument     | create monotonic counter        |
|  [03]   | `Meter.create_histogram(name, ...)`                | instrument     | create histogram recorder       |
|  [04]   | `Meter.create_observable_counter(name, callbacks)` | instrument     | create async observable counter |
|  [05]   | `Meter.create_observable_gauge(name, callbacks)`   | instrument     | create async observable gauge   |
|  [06]   | `Counter.add(amount, attributes)`                  | record         | increment counter               |
|  [07]   | `Histogram.record(amount, attributes)`             | record         | record histogram measurement    |

[ENTRYPOINT_SCOPE]: context and propagation
- rail: observability

| [INDEX] | [SURFACE]                             | [ENTRY_FAMILY] | [RAIL]                         |
| :-----: | :------------------------------------ | :------------- | :----------------------------- |
|  [01]   | `context.attach(context)`             | context        | activate context, return token |
|  [02]   | `context.detach(token)`               | context        | restore previous context       |
|  [03]   | `propagate.extract(carrier, context)` | propagation    | decode context from carrier    |
|  [04]   | `propagate.inject(carrier, context)`  | propagation    | encode context into carrier    |

## [04]-[IMPLEMENTATION_LAW]

[OBSERVABILITY_TOPOLOGY]:
- meter resolution: `metrics.get_meter(name, version)` returns the no-op meter until the SDK meter provider is installed at startup; runtime owners never call `set_meter_provider`.
- instrument lifetime: each `Counter`/`Histogram`/`ObservableCounter`/`ObservableGauge` is created once per meter at owner construction and reused; never create a per-request or per-lane instrument.
- async instruments: `ObservableCounter`/`ObservableGauge` read state through their registered callbacks, which return `Iterable[Observation]` and accept `CallbackOptions`; callbacks run on the SDK collection cycle and must be non-blocking.
- propagation seam: `propagate.inject(carrier, context)` writes the W3C trace headers into a `dict[str, str]` carrier on the outbound lane/RPC hop; `propagate.extract(carrier, context)` rebuilds the `Context` on the inbound hop.
- context scope: `context.attach(context)` returns a `Token` that must be paired with `context.detach(token)`; attach/detach bracket the active span scope across a lane boundary and never leak across hops.

[LOCAL_ADMISSION]:
- Runtime owners import only from `opentelemetry-api`; SDK construction and `set_*_provider` live in the application composition root.
- Propagation carriers are plain `dict[str, str]`; the lane/RPC seam injects on send and extracts on receive, pairing every `inject` with a corresponding `extract`.
- Metric instruments are owner-held singletons; lanes/metrics/receipt owners record against the cached instrument, never re-create one per call.
- Every `context.attach` is balanced by a `context.detach` of the returned token in the same scope.

[RAIL_LAW]:
- Package: `opentelemetry-api`
- Owns: global provider resolution, W3C context propagation, metric-instrument contracts at the API boundary
- Accept: `metrics.get_meter` + cached instrument recording, `propagate.inject`/`extract` at the lane/RPC seam, token-paired `context.attach`/`detach`
- Reject: SDK imports in runtime owners, per-request instrument creation, unbalanced attach/detach, `set_*_provider` outside the composition root
