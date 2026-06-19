# [API_CATALOGUE] @opentelemetry/sdk-metrics

`@opentelemetry/sdk-metrics` supplies `MeterProvider`, `MetricReader`, `PeriodicExportingMetricReader`, instrument-type enums, aggregation temporality, metric data shapes, and exporter contracts for collecting and exporting OTel metrics in Node.js and browser services.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/sdk-metrics`
- package: `@opentelemetry/sdk-metrics`
- module: `@opentelemetry/sdk-metrics`
- asset: runtime library
- rail: metrics

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: provider
- rail: metrics

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [RAIL]                          |
| :-----: | :--------------------- | :------------ | :------------------------------ |
|  [01]   | `MeterProvider`        | class         | root metric collection provider |
|  [02]   | `MeterProviderOptions` | interface     | provider construction config    |

[PUBLIC_TYPE_SCOPE]: readers and exporters
- rail: metrics

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]  | [RAIL]                                   |
| :-----: | :------------------------------------- | :------------- | :--------------------------------------- |
|  [01]   | `MetricReader`                         | abstract class | base metric reader                       |
|  [02]   | `PeriodicExportingMetricReader`        | class          | interval-driven push reader              |
|  [03]   | `InMemoryMetricExporter`               | class          | test/in-memory exporter                  |
|  [04]   | `ConsoleMetricExporter`                | class          | diagnostic console exporter              |
|  [05]   | `IMetricReader`                        | interface      | metric reader contract                   |
|  [06]   | `PushMetricExporter`                   | interface      | push exporter contract                   |
|  [07]   | `MetricReaderOptions`                  | interface      | reader config (aggregation, cardinality) |
|  [08]   | `PeriodicExportingMetricReaderOptions` | type           | interval exporter config                 |
|  [09]   | `MetricCollectOptions`                 | interface      | collect call options                     |
|  [10]   | `MetricProducer`                       | interface      | external metric producer                 |

[PUBLIC_TYPE_SCOPE]: enumerations and temporality
- rail: metrics

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [RAIL]                                                             |
| :-----: | :----------------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `AggregationTemporality` | enum          | `DELTA = 0`, `CUMULATIVE = 1`                                      |
|  [02]   | `InstrumentType`         | enum          | `COUNTER`, `GAUGE`, `HISTOGRAM`, `UP_DOWN_COUNTER`, `OBSERVABLE_*` |
|  [03]   | `DataPointType`          | enum          | data point variant discriminant                                    |
|  [04]   | `AggregationType`        | enum          | aggregation algorithm selector                                     |

[PUBLIC_TYPE_SCOPE]: metric data shapes
- rail: metrics

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [RAIL]                         |
| :-----: | :------------------------------- | :------------ | :----------------------------- |
|  [01]   | `ResourceMetrics`                | interface     | top-level export envelope      |
|  [02]   | `ScopeMetrics`                   | interface     | per-instrumentation-scope data |
|  [03]   | `MetricData`                     | type          | union of metric data variants  |
|  [04]   | `MetricDescriptor`               | interface     | metric name, unit, type        |
|  [05]   | `DataPoint`                      | interface     | single metric measurement      |
|  [06]   | `SumMetricData`                  | interface     | sum instrument data            |
|  [07]   | `GaugeMetricData`                | interface     | gauge instrument data          |
|  [08]   | `HistogramMetricData`            | interface     | histogram instrument data      |
|  [09]   | `ExponentialHistogramMetricData` | interface     | exponential histogram data     |
|  [10]   | `CollectionResult`               | interface     | collect return with errors     |

[PUBLIC_TYPE_SCOPE]: aggregation and view
- rail: metrics

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [RAIL]                             |
| :-----: | :------------------------------- | :------------ | :--------------------------------- |
|  [01]   | `AggregationSelector`            | type          | instrument-type -> aggregation map |
|  [02]   | `AggregationTemporalitySelector` | type          | instrument-type -> temporality map |
|  [03]   | `AggregationOption`              | type          | view aggregation option            |
|  [04]   | `ViewOptions`                    | interface     | view config for a MeterProvider    |
|  [05]   | `IAttributesProcessor`           | interface     | attribute allowlist/denylist       |
|  [06]   | `TimeoutError`                   | class         | collect timeout failure            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: MeterProvider lifecycle
- rail: metrics

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :-------------------------------------------- | :------------- | :------------------------------ |
|  [01]   | `new MeterProvider(options?)`                 | constructor    | create provider with readers    |
|  [02]   | `provider.getMeter(name, version?, options?)` | factory        | obtain a named Meter            |
|  [03]   | `provider.shutdown(options?)`                 | lifecycle      | flush and shut down all readers |
|  [04]   | `provider.forceFlush(options?)`               | lifecycle      | flush without shutdown          |

[ENTRYPOINT_SCOPE]: PeriodicExportingMetricReader construction
- rail: metrics

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY] | [RAIL]                       |
| :-----: | :--------------------------------------------------- | :------------- | :--------------------------- |
|  [01]   | `new PeriodicExportingMetricReader(options)`         | constructor    | interval-based push reader   |
|  [02]   | `new InMemoryMetricExporter(aggregationTemporality)` | constructor    | in-memory exporter for tests |
|  [03]   | `new ConsoleMetricExporter(options?)`                | constructor    | console diagnostic exporter  |

[ENTRYPOINT_SCOPE]: attribute processor factories
- rail: metrics

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :---------------------------------------------- | :------------- | :-------------------------------- |
|  [01]   | `createAllowListAttributesProcessor(allowList)` | factory        | retain only listed attribute keys |
|  [02]   | `createDenyListAttributesProcessor(denyList)`   | factory        | drop listed attribute keys        |

## [04]-[IMPLEMENTATION_LAW]

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
