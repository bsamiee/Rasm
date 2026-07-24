# [TS_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_RUNTIME_NODE]

`@opentelemetry/instrumentation-runtime-node` samples node `perf_hooks` and `v8` on a fixed cadence, registering one observable per engine-health series against a `Meter` — event-loop delay/utilization, GC duration, V8 heap. It owns the V8 and event-loop interior the OTLP metric lane drains where `HostMetrics` owns os/process cpu/memory/network; `perf_hooks` monitors and the `uncaughtExceptionMonitor` handler seat it at the composition root beside the metric-reader wiring, never inside a library.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/instrumentation-runtime-node`
- package: `@opentelemetry/instrumentation-runtime-node` (Apache-2.0)
- base: `RuntimeNodeInstrumentation` extends `@opentelemetry/instrumentation` `InstrumentationBase`; per-series `BaseCollector` implementations register observables against the bound `Meter`
- backing: `@opentelemetry/api` `Meter`, `@opentelemetry/api-logs` `LoggerProvider` for the exception-log path, node `perf_hooks`/`v8`/`process` as the sample source
- rail: observability/vitals — node engine interior on the meter plane
- consumed-by: the node composition root beside the metric-reader row; `otel/emit` binds its `Meter` through `Hooks.Meter`
- runtime: node only — reads `perf_hooks` monitors and `v8` heap statistics

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: instrumentation class and config — the whole public export; collectors and semconv constants stay internal.

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY]   | [CAPABILITY]                                              |
| :-----: | :--------------------------------- | :-------------- | :-------------------------------------------------------- |
|  [01]   | `RuntimeNodeInstrumentation`       | instrumentation | one row in the node root's registered instrumentation set |
|  [02]   | `RuntimeNodeInstrumentationConfig` | config          | the whole policy surface, one object at construction      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and lifecycle — one `RuntimeNodeInstrumentationConfig` is the whole knob surface; `enable()` registers the observables against the bound `Meter` and begins collection.

| [INDEX] | [SURFACE]                                 | [SHAPE]      | [CAPABILITY]                                                  |
| :-----: | :---------------------------------------- | :----------- | :------------------------------------------------------------ |
|  [01]   | `new RuntimeNodeInstrumentation(config?)` | ctor         | one construction at the node root                             |
|  [02]   | `monitoringPrecision`                     | config field | event-loop-delay histogram sampling resolution in ms          |
|  [03]   | `captureUncaughtException`                | config field | opt-in `uncaughtExceptionMonitor` capture, off by default     |
|  [04]   | `applyCustomExceptionAttributes`          | config field | `(error, 'uncaughtException') => Attributes` log-record stamp |
|  [05]   | `enabled`                                 | config field | inherited `InstrumentationConfig` activation gate             |
|  [06]   | `.enable()` / `.disable()`                | lifecycle    | registers or tears down the collectors on the bound `Meter`   |
|  [07]   | `.setLoggerProvider(loggerProvider)`      | wiring       | binds the `LoggerProvider` the exception-log path emits on    |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Each collector registers one observable per series against the single bound `Meter`; the emitted set is the surface contract — delay gauges `nodejs.eventloop.delay.` `min` `max` `mean` `stddev` `p50` `p90` `p99`; `nodejs.eventloop.time` counter and `nodejs.eventloop.utilization` gauge; `v8js.gc.duration` histogram; heap gauges `v8js.memory.heap.` `limit` `used` `space.size` `space.available_size` `space.physical_size`; `v8js.resource.active` count.
- Series attributes `nodejs.eventloop.state` (`active`/`idle`), `v8js.gc.type`, `v8js.heap.space.name`, `v8js.resource.type` — the heap-space and gc-type dimensions are the high-cardinality fan the deny-list view guards.
- Dotted `nodejs.*`/`v8js.*` names, the `@opentelemetry/semantic-conventions` vocabulary, ride the estate Prometheus translation unchanged; a rename breaks the downstream dashboard vocabulary.

[STACKING]:
- `otel/emit`: registers against the `Meter` whose `MeterProvider` the OTLP metric lane drains, so engine vitals inherit the same `service.name` resource as `HostMetrics` spans and logs.
- `@opentelemetry/host-metrics`(`.api/opentelemetry-host-metrics.md`): co-registers on one meter — host-metrics owns os/process cpu/memory/network, this owns the V8/event-loop interior; the series namespaces never collide.
- `@opentelemetry/sdk-metrics`(`.api/opentelemetry-sdk-metrics.md`): a `createDenyListAttributesProcessor` view drops the `v8js.heap.space.name` and `v8js.gc.type` dimensions where a deployment reads only the aggregate series, holding the cardinality fan.
- `@opentelemetry/api-logs`(`.api/opentelemetry-api-logs.md`): `setLoggerProvider` binds the `LoggerProvider` the opt-in `captureUncaughtException` path emits crash records on, so a fatal exception leaves as a log record on the same export wire.

[LOCAL_ADMISSION]:
- `scope:runtime`, node lane; a library never registers `RuntimeNodeInstrumentation` — construction lives only in the node boot graph beside the metric-reader row.
- `captureUncaughtException` is deployment policy — a host owning a crash-fold handler leaves it off, never double-registers the `uncaughtExceptionMonitor`.

[RAIL_LAW]:
- Package: `@opentelemetry/instrumentation-runtime-node`
- Owns: node engine vitals — event-loop delay/utilization, GC duration, and V8 heap series on the meter plane
- Accept: one construction at the node root binding the `otel/emit` `Meter`, deny-list view guarding the heap-space and gc-type fan
- Reject: library-altitude registration, a second engine collector on the same meter, renamed series outputs, a redundant `uncaughtExceptionMonitor` where the host owns crash folding
