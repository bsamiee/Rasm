# [API_CATALOGUE] @opentelemetry/sdk-metrics

`@opentelemetry/sdk-metrics` supplies `MeterProvider`, `MetricReader`, `PeriodicExportingMetricReader`, instrument-type enums, aggregation temporality, metric data shapes, and exporter contracts for collecting and exporting OTel metrics in Node.js and browser services.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/sdk-metrics`
- package: `@opentelemetry/sdk-metrics`
- module: `@opentelemetry/sdk-metrics`
- asset: runtime library
- rail: metrics

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: provider
- rail: metrics

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [RAIL]                          |
| :-----: | :--------------------- | :------------ | :------------------------------ |
|   [1]   | `MeterProvider`        | class         | root metric collection provider |
|   [2]   | `MeterProviderOptions` | interface     | provider construction config    |

[PUBLIC_TYPE_SCOPE]: readers and exporters
- rail: metrics

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]  | [RAIL]                                   |
| :-----: | :------------------------------------- | :------------- | :--------------------------------------- |
|   [1]   | `MetricReader`                         | abstract class | base metric reader                       |
|   [2]   | `PeriodicExportingMetricReader`        | class          | interval-driven push reader              |
|   [3]   | `InMemoryMetricExporter`               | class          | test/in-memory exporter                  |
|   [4]   | `ConsoleMetricExporter`                | class          | diagnostic console exporter              |
|   [5]   | `IMetricReader`                        | interface      | metric reader contract                   |
|   [6]   | `PushMetricExporter`                   | interface      | push exporter contract                   |
|   [7]   | `MetricReaderOptions`                  | interface      | reader config (aggregation, cardinality) |
|   [8]   | `PeriodicExportingMetricReaderOptions` | type           | interval exporter config                 |
|   [9]   | `MetricCollectOptions`                 | interface      | collect call options                     |
|  [10]   | `MetricProducer`                       | interface      | external metric producer                 |

[PUBLIC_TYPE_SCOPE]: enumerations and temporality
- rail: metrics

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [RAIL]                                                             |
| :-----: | :----------------------- | :------------ | :----------------------------------------------------------------- |
|   [1]   | `AggregationTemporality` | enum          | `DELTA = 0`, `CUMULATIVE = 1`                                      |
|   [2]   | `InstrumentType`         | enum          | `COUNTER`, `GAUGE`, `HISTOGRAM`, `UP_DOWN_COUNTER`, `OBSERVABLE_*` |
|   [3]   | `DataPointType`          | enum          | data point variant discriminant                                    |
|   [4]   | `AggregationType`        | enum          | aggregation algorithm selector                                     |

[PUBLIC_TYPE_SCOPE]: metric data shapes
- rail: metrics

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [RAIL]                         |
| :-----: | :------------------------------- | :------------ | :----------------------------- |
|   [1]   | `ResourceMetrics`                | interface     | top-level export envelope      |
|   [2]   | `ScopeMetrics`                   | interface     | per-instrumentation-scope data |
|   [3]   | `MetricData`                     | type          | union of metric data variants  |
|   [4]   | `MetricDescriptor`               | interface     | metric name, unit, type        |
|   [5]   | `DataPoint`                      | interface     | single metric measurement      |
|   [6]   | `SumMetricData`                  | interface     | sum instrument data            |
|   [7]   | `GaugeMetricData`                | interface     | gauge instrument data          |
|   [8]   | `HistogramMetricData`            | interface     | histogram instrument data      |
|   [9]   | `ExponentialHistogramMetricData` | interface     | exponential histogram data     |
|  [10]   | `CollectionResult`               | interface     | collect return with errors     |

[PUBLIC_TYPE_SCOPE]: aggregation and view
- rail: metrics

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [RAIL]                             |
| :-----: | :------------------------------- | :------------ | :--------------------------------- |
|   [1]   | `AggregationSelector`            | type          | instrument-type -> aggregation map |
|   [2]   | `AggregationTemporalitySelector` | type          | instrument-type -> temporality map |
|   [3]   | `AggregationOption`              | type          | view aggregation option            |
|   [4]   | `ViewOptions`                    | interface     | view config for a MeterProvider    |
|   [5]   | `IAttributesProcessor`           | interface     | attribute allowlist/denylist       |
|   [6]   | `TimeoutError`                   | class         | collect timeout failure            |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: MeterProvider lifecycle
- rail: metrics

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :-------------------------------------------- | :------------- | :------------------------------ |
|   [1]   | `new MeterProvider(options?)`                 | constructor    | create provider with readers    |
|   [2]   | `provider.getMeter(name, version?, options?)` | factory        | obtain a named Meter            |
|   [3]   | `provider.shutdown(options?)`                 | lifecycle      | flush and shut down all readers |
|   [4]   | `provider.forceFlush(options?)`               | lifecycle      | flush without shutdown          |

[ENTRYPOINT_SCOPE]: PeriodicExportingMetricReader construction
- rail: metrics

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY] | [RAIL]                       |
| :-----: | :--------------------------------------------------- | :------------- | :--------------------------- |
|   [1]   | `new PeriodicExportingMetricReader(options)`         | constructor    | interval-based push reader   |
|   [2]   | `new InMemoryMetricExporter(aggregationTemporality)` | constructor    | in-memory exporter for tests |
|   [3]   | `new ConsoleMetricExporter(options?)`                | constructor    | console diagnostic exporter  |

[ENTRYPOINT_SCOPE]: attribute processor factories
- rail: metrics

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :---------------------------------------------- | :------------- | :-------------------------------- |
|   [1]   | `createAllowListAttributesProcessor(allowList)` | factory        | retain only listed attribute keys |
|   [2]   | `createDenyListAttributesProcessor(denyList)`   | factory        | drop listed attribute keys        |

## [4]-[IMPLEMENTATION_LAW]

[METRICS_TOPOLOGY]:
- namespace: `@opentelemetry/sdk-metrics`; `MeterProvider` is the sole composition root
- readers are registered at construction via `MeterProviderOptions.readers`; no add-reader-after-init API
- `PeriodicExportingMetricReader` drives `PushMetricExporter` on a fixed `exportIntervalMillis` interval with a `exportTimeoutMillis` deadline
- `MetricProducer` is the seam for external metric sources (e.g., `@effect/opentelemetry` Metrics bridge); pass instances via `metricProducers` in reader or provider options

[LOCAL_ADMISSION]:
- Wire one `MeterProvider` per service with `readers` at construction; rebuild is the only reconfiguration path.
- `AggregationTemporality.DELTA` for counters when the downstream collector is cumulative-aware; `CUMULATIVE` otherwise.
- Use `InMemoryMetricExporter` exclusively in tests; never in production code paths.

[RAIL_LAW]:
- Package: `@opentelemetry/sdk-metrics`
- Owns: OTel metrics SDK — provider, readers, exporters, views, and metric data shapes
- Accept: `PushMetricExporter` implementations, `MetricProducer` external sources, `ViewOptions` per instrument
- Reject: hand-rolled metric accumulation outside `MeterProvider`
