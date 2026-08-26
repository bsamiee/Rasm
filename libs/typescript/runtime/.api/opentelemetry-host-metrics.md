# [TS_RUNTIME_API_OPENTELEMETRY_HOST_METRICS]

`@opentelemetry/host-metrics` produces node runtime vitals: `HostMetrics` samples host and process counters on a fixed cadence and registers one observable per metric against a `Meter`. Node counter reads make it composition-root material — one construction seats beside the metric-reader wiring the OTLP lane drains, never inside a library.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the collector class, its config, and the reading projections

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]  | [CAPABILITY]                                |
| :-----: | :----------------------- | :------------- | :------------------------------------------ |
|  [01]   | `HostMetrics`            | class          | node root's concrete collector              |
|  [02]   | `BaseMetrics`            | abstract class | meter-bound base every collector extends    |
|  [03]   | `MetricsCollectorConfig` | interface      | `meterProvider`/`name`/`metricGroups` knobs |
|  [04]   | `CpuUsageData`           | interface      | per-CPU usage reading projection            |
|  [05]   | `ProcessCpuUsageData`    | interface      | per-process usage reading projection        |
|  [06]   | `MemoryData`             | interface      | absolute + percentage memory reading        |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and the collection lifecycle

`MetricsCollectorConfig` is the whole knob surface: `meterProvider` binds the target meter, `name` stamps every instrument, `metricGroups` gates which observables register. `start()` registers the roster once, then samples on the fixed cadence.

| [INDEX] | [SURFACE]                  | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :------------------------- | :------- | :-------------------------------- |
|  [01]   | `new HostMetrics(config?)` | ctor     | one construction at the node root |
|  [02]   | `HostMetrics.start()`      | instance | starts fixed-cadence collection   |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every metric is one observable registered against the single bound `Meter`; `start()` samples the whole roster on one cadence, so a second collector on the same meter double-counts.
- `metricGroups` gates registration by the closed group vocabulary `system.cpu`, `system.memory`, `system.network`, `process.cpu`, `process.memory`; an omitted `metricGroups` registers every group. Typing is `string[]` and each group tests `includes` against it, so an unrostered spelling REFUSES nothing — it silently leaves that family unregistered while every other entry still applies, which is why a consumer closes the roster on its own policy row rather than handing free strings through.
- Instrument names are the `@opentelemetry/semantic-conventions` `system.*`/`process.*` rows the repo Prometheus translation reads verbatim; a rename breaks the downstream dashboard vocabulary.

[STACKING]:
- `@opentelemetry/sdk-metrics`(`.api/opentelemetry-sdk-metrics.md`): the observables register on a `Meter` from a `MeterProvider` carrying the `AppIdentity`-derived `Resource`, so host vitals inherit the same `service.name` as spans and logs.
- `@opentelemetry/exporter-metrics-otlp-http`(`.api/opentelemetry-exporter-metrics-otlp-http.md`): a `PeriodicExportingMetricReader` on that provider drains the observables to the OTLP collector on the export interval.
- `otel/emit` (within-lib): `MetricsCollectorConfig.meterProvider` takes the `provider` member of the raw metric plane the `Hooks.Meter` Tag carries; absent it the collector binds the global meter under the facade.

[LOCAL_ADMISSION]:
- Construction lives only in the node boot graph under `scope:runtime`; `otel/emit` binds `Hooks.Meter`'s `provider` member.
- Group gating is deployment policy — a constrained host trims `metricGroups` to the vitals its dashboards read, never forks a second collector.
