# [TS_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_RUNTIME_NODE]

`@opentelemetry/instrumentation-runtime-node` reads the node `perf_hooks` and `v8` surface on a fixed cadence and registers one observable per engine-health series against a `Meter`: `monitorEventLoopDelay` histogram percentiles, `performance.eventLoopUtilization` state time, the `gc` `PerformanceObserver` duration histogram, and `v8.getHeapStatistics`/`getHeapSpaceStatistics` heap gauges. It is the engine-vitals producer distinct from `HostMetrics` — where host-metrics sweeps os/process cpu/memory/network, this row owns the V8 and event-loop interior the OTLP metric lane drains. `perf_hooks` monitors and a process-level `uncaughtExceptionMonitor` handler make it composition-root material — one construction seats beside the metric-reader wiring, never inside a library.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/instrumentation-runtime-node`
- package: `@opentelemetry/instrumentation-runtime-node` (Apache-2.0)
- base: `RuntimeNodeInstrumentation` extends `@opentelemetry/instrumentation` `InstrumentationBase`; per-series `BaseCollector` implementations register observables against the bound `Meter`
- backing: `@opentelemetry/api` `Meter`; `@opentelemetry/api-logs` `LoggerProvider` for the exception-log path; node `perf_hooks`/`v8`/`process` as the sample source
- naming: emitted series and attributes ride `@opentelemetry/semantic-conventions` `nodejs.eventloop.*` and `v8js.*` vocabularies
- consumed-by: the node composition root beside the metric-reader row; `otel/emit` binds its `Meter` through `Hooks.Meter`
- runtime: node only — reads `perf_hooks` monitors and `v8` heap statistics

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: instrumentation class + config
- rail: observability/vitals
- `RuntimeNodeInstrumentation` and `RuntimeNodeInstrumentationConfig` are the entire public export; the collectors and semconv constants stay internal, surfacing only as the emitted series roster below.

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY]   | [CONSUMER_BOUNDARY]                                       |
| :-----: | :--------------------------------- | :-------------- | :-------------------------------------------------------- |
|  [01]   | `RuntimeNodeInstrumentation`       | instrumentation | one row in the node root's registered instrumentation set |
|  [02]   | `RuntimeNodeInstrumentationConfig` | config          | the whole policy surface, one object at construction      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction + registration lifecycle
- rail: observability/vitals
- One `RuntimeNodeInstrumentationConfig` object is the whole knob surface; `enable()` seats the collectors and registers observables against the bound `Meter`, and `registerInstrumentations` (or a direct `enable()`) begins collection.

| [INDEX] | [SURFACE]                                 | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                           |
| :-----: | :---------------------------------------- | :------------- | :------------------------------------------------------------ |
|  [01]   | `new RuntimeNodeInstrumentation(config?)` | ctor           | one construction at the node root                             |
|  [02]   | `monitoringPrecision`                     | config field   | event-loop-delay histogram sampling resolution in ms          |
|  [03]   | `captureUncaughtException`                | config field   | opt-in `uncaughtExceptionMonitor` capture, off by default     |
|  [04]   | `applyCustomExceptionAttributes`          | config field   | `(error, 'uncaughtException') => Attributes` log-record stamp |
|  [05]   | `enabled`                                 | config field   | inherited `InstrumentationConfig` activation gate             |
|  [06]   | `.enable()` / `.disable()`                | lifecycle      | registers or tears down the collectors on the bound `Meter`   |
|  [07]   | `.setLoggerProvider(loggerProvider)`      | wiring         | binds the `LoggerProvider` the exception-log path emits on    |

## [04]-[IMPLEMENTATION_LAW]

[METER_TOPOLOGY]:
- composition-root only — a library never registers `RuntimeNodeInstrumentation`; one construction at the node root binds the `Meter` `otel/emit` exposes through `Hooks.Meter`, registered through `registerInstrumentations` beside `HostMetrics`.
- emitted series roster — event-loop delay gauges `nodejs.eventloop.delay.min`, `nodejs.eventloop.delay.max`, `nodejs.eventloop.delay.mean`, `nodejs.eventloop.delay.stddev`, `nodejs.eventloop.delay.p50`, `nodejs.eventloop.delay.p90`, `nodejs.eventloop.delay.p99`; event-loop `nodejs.eventloop.time` counter and `nodejs.eventloop.utilization` gauge; `v8js.gc.duration` histogram; heap gauges `v8js.memory.heap.limit`, `v8js.memory.heap.used`, `v8js.memory.heap.space.size`, `v8js.memory.heap.space.available_size`, `v8js.memory.heap.space.physical_size`; `v8js.resource.active` count.
- series attributes — `nodejs.eventloop.state` (`active`/`idle`), `v8js.gc.type`, `v8js.heap.space.name`, `v8js.resource.type`; the heap-space and gc-type dimensions are the high-cardinality fan the deny-list view guards.
- dotted `nodejs.*`/`v8js.*` names ride the estate Prometheus translation unchanged — a rename breaks the downstream dashboard vocabulary.

[INTEGRATION_LAW]:
- Stack with `otel/emit` meter plane: the row registers against the `Meter` whose `MeterProvider` the OTLP metric lane drains, so engine vitals inherit the same `service.name` resource as `HostMetrics` spans and logs.
- Stack with `opentelemetry-host-metrics.md`: the two collectors co-register on one meter — host-metrics owns os/process cpu/memory/network, this row owns the V8/event-loop interior; the series namespaces never collide.
- Stack with `opentelemetry-sdk-metrics.md`: a `createDenyListAttributesProcessor` view drops the `v8js.heap.space.name` and `v8js.gc.type` dimensions where a deployment reads only the aggregate series, holding the cardinality fan.
- Stack with `opentelemetry-api-logs.md`: `setLoggerProvider` binds the `LoggerProvider` the opt-in `captureUncaughtException` path emits crash records on, so a fatal exception leaves as a log record on the same export wire.

[LOCAL_ADMISSION]:
- `scope:runtime`, node lane; construction lives only in the node boot graph beside the metric-reader row.
- `captureUncaughtException` is deployment policy — a host that already owns a crash-fold handler leaves it off, never double-registers the `uncaughtExceptionMonitor`.

[RAIL_LAW]:
- Package: `@opentelemetry/instrumentation-runtime-node`
- Owns: node engine vitals — event-loop delay/utilization, GC duration, and V8 heap series on the meter plane
- Accept: one construction at the node root binding the `otel/emit` `Meter`, deny-list view guarding the heap-space and gc-type fan
- Reject: library-altitude registration, a second engine collector on the same meter, renamed series outputs, a redundant `uncaughtExceptionMonitor` where the host owns crash folding
