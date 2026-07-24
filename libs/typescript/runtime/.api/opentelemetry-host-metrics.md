# [TS_RUNTIME_API_OPENTELEMETRY_HOST_METRICS]

`@opentelemetry/host-metrics` produces node runtime vitals: `HostMetrics` samples host and process counters on a fixed cadence and registers one observable per metric against a `Meter`. Node counter reads make it composition-root material — one construction seats beside the metric-reader wiring the OTLP lane drains, never inside a library.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/host-metrics`
- package: `@opentelemetry/host-metrics` (Apache-2.0)
- module: CJS single entry (`build/src/index.js`); no subpath exports
- runtime: node only — samples `os`, `process`, and `systeminformation`
- depends: `@opentelemetry/api` `Meter`/`MeterProvider` peer; instrument names from `@opentelemetry/semantic-conventions`
- rail: observability/vitals — the node host and process metric producer

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the collector class, its config, and the reading projections

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]  | [CAPABILITY]                                |
| :-----: | :----------------------- | :------------- | :------------------------------------------ |
|  [01]   | `HostMetrics`            | class          | node root's concrete collector              |
|  [02]   | `BaseMetrics`            | abstract class | meter-bound base every collector extends    |
|  [03]   | `MetricsCollectorConfig` | interface      | `meterProvider`/`name`/`metricGroups` knobs |
|  [04]   | `CpuUsageData`           | interface      | per-CPU usage reading projection            |
|  [05]   | `ProcessCpuUsageData`    | interface      | per-process usage reading projection        |
|  [06]   | `MemoryData`             | interface      | absolute + percentage memory reading        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and the collection lifecycle

`MetricsCollectorConfig` is the whole knob surface: `meterProvider` binds the target meter, `name` stamps every instrument, `metricGroups` gates which observables register. `start()` registers the roster once, then samples on the fixed cadence.

| [INDEX] | [SURFACE]                  | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :------------------------- | :------- | :-------------------------------- |
|  [01]   | `new HostMetrics(config?)` | ctor     | one construction at the node root |
|  [02]   | `HostMetrics.start()`      | instance | starts fixed-cadence collection   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every metric is one observable registered against the single bound `Meter`; `start()` samples the whole roster on one cadence, so a second collector on the same meter double-counts.
- `metricGroups` gates registration by group vocabulary `system.cpu`, `system.memory`, `system.network`, `process.cpu`, `process.memory`; an omitted `metricGroups` registers every group.
- Instrument names are the `@opentelemetry/semantic-conventions` `system.*`/`process.*` rows the estate Prometheus translation reads verbatim; a rename breaks the downstream dashboard vocabulary.

[STACKING]:
- `@opentelemetry/sdk-metrics`(`.api/opentelemetry-sdk-metrics.md`): the observables register on a `Meter` from a `MeterProvider` carrying the `AppIdentity`-derived `Resource`, so host vitals inherit the same `service.name` as spans and logs.
- `@opentelemetry/exporter-metrics-otlp-http`(`.api/opentelemetry-exporter-metrics-otlp-http.md`): a `PeriodicExportingMetricReader` on that provider drains the observables to the OTLP collector on the export interval.
- `otel/emit` (within-lib): `MetricsCollectorConfig.meterProvider` takes the raw `MeterProvider` the emit facade exposes through `Hooks.Meter`; absent it the collector binds the global meter under the facade.

[LOCAL_ADMISSION]:
- Construction lives only in the node boot graph under `scope:runtime`; `otel/emit` binds the raw `MeterProvider` through `Hooks.Meter`.
- Group gating is deployment policy — a constrained host trims `metricGroups` to the vitals its dashboards read, never forks a second collector.

[RAIL_LAW]:
- Package: `@opentelemetry/host-metrics`
- Owns: node host and process observable vitals — cpu/memory/network counters and gauges on the meter plane
- Accept: one node-root construction binding the `otel/emit` `MeterProvider`, group-gated by `metricGroups`
- Reject: library-altitude construction, a second collector on the same meter, renamed instrument outputs
