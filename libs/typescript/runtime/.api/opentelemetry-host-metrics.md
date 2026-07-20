# [TS_RUNTIME_API_OPENTELEMETRY_HOST_METRICS]

`@opentelemetry/host-metrics` collects host and process observables on a fixed cadence: `HostMetrics` extends `BaseMetrics`, reads `os`/`process` counters, and registers one observable per metric against a `Meter`. It is the node runtime-vitals producer — `system.cpu.*`, `system.memory.*`, `system.network.*`, and `process.*` observables feed the meter plane the OTLP metric lane drains. Node reads (`os.cpus()`, `process.cpuUsage()`) make it composition-root material — one construction seats beside the metric-reader wiring, never inside a library.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/host-metrics`
- package: `@opentelemetry/host-metrics`
- license: `Apache-2.0`
- base: `HostMetrics` extends the abstract `BaseMetrics`; metric names ride `@opentelemetry/semantic-conventions`
- backing: `@opentelemetry/api` `Meter`/`MeterProvider`; `os`/`process` node built-ins as the sample source
- consumed-by: the node composition root beside the metric-reader row; `otel/emit` binds its `MeterProvider`
- runtime: node only — reads `os` and `process` counters

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: collector class + config + sample shapes
- rail: observability/vitals
- `HostMetrics` is the only concrete collector; `BaseMetrics` seats the shared meter and the `start`/`_createMetrics` contract every subclass fills.

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]   | [CONSUMER_BOUNDARY]                                   |
| :-----: | :------------------------------------- | :-------------- | :---------------------------------------------------- |
|  [01]   | `HostMetrics`                          | collector class | one construction the node root starts                 |
|  [02]   | `BaseMetrics`                          | abstract base   | meter-bound base every collector extends              |
|  [03]   | `MetricsCollectorConfig`               | config          | `meterProvider`/`name`/`metricGroups` at construction |
|  [04]   | `CpuUsageData` / `ProcessCpuUsageData` | sample shape    | per-CPU and per-process usage reading projections     |
|  [05]   | `MemoryData`                           | sample shape    | absolute + percentage memory reading projection       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction + collection lifecycle
- rail: observability/vitals
- One `MetricsCollectorConfig` object is the whole knob surface; `start()` registers the observables and begins collection against the bound `Meter`.

| [INDEX] | [SURFACE]                  | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                      |
| :-----: | :------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `new HostMetrics(config?)` | ctor           | one construction at the node root                        |
|  [02]   | `meterProvider`            | config field   | the raw `MeterProvider` the collector's `Meter` binds to |
|  [03]   | `name`                     | config field   | meter name stamped on every emitted instrument           |
|  [04]   | `metricGroups`             | config field   | group allow-list gating which observables register       |
|  [05]   | `.start()`                 | lifecycle      | registers observables, begins fixed-cadence collection   |

## [04]-[IMPLEMENTATION_LAW]

[METER_TOPOLOGY]:
- composition-root only — a library never starts `HostMetrics`; one construction at the node root binds the raw `MeterProvider` `otel/emit` exposes through `Hooks.Meter`.
- observable roster — counters `system.cpu.time`, `system.network.dropped`, `system.network.errors`, `system.network.io`, `process.cpu.time`; gauges `system.cpu.utilization`, `system.memory.usage`, `system.memory.utilization`, `process.cpu.utilization`, `process.memory.usage`.
- `metricGroups` gates the roster — values `system.cpu`, `system.memory`, `system.network`, `process.cpu`, `process.memory`; an omitted `metricGroups` registers every group.
- dotted `system.*`/`process.*` names ride the estate Prometheus translation unchanged — a rename breaks the downstream dashboard vocabulary.

[INTEGRATION_LAW]:
- Stack with `otel/emit` meter plane: `MetricsCollectorConfig.meterProvider` takes the raw `MeterProvider` the OTLP metric lane drains; absent it the collector binds the global meter registered under the facade.
- Stack with `opentelemetry-sdk-metrics.md`: the observables register on a `Meter` whose provider carries the `AppIdentity`-derived resource, so host vitals inherit the same `service.name` as spans and logs.
- Stack with `opentelemetry-exporter-metrics-otlp-http.md`: a `PeriodicExportingMetricReader` on that provider pulls the observables to the collector on the export interval.

[LOCAL_ADMISSION]:
- `scope:runtime`, node lane; construction lives only in the node boot graph.
- Group gating is deployment policy — a resource-constrained host trims `metricGroups` to the vitals its dashboards read, never forks a second collector.

[RAIL_LAW]:
- Package: `@opentelemetry/host-metrics`
- Owns: node host + process observable metrics — cpu/memory/network counters and gauges on the meter plane
- Accept: one construction at the node root binding the `otel/emit` `MeterProvider`, group gating via `metricGroups`
- Reject: library-altitude construction, a second collector on the same meter, renamed metric outputs
