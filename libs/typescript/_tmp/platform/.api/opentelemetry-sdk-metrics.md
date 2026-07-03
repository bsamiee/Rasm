# [API_CATALOGUE] @opentelemetry/sdk-metrics

`@opentelemetry/sdk-metrics` supplies `MeterProvider`, `MetricReader`, `PeriodicExportingMetricReader`, aggregation types, `InstrumentType`, `AggregationTemporality`, and data-model interfaces (`MetricData`, `DataPoint`, `CollectionResult`) for SDK-side metric collection, aggregation, and export.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/sdk-metrics`
- package: `@opentelemetry/sdk-metrics`
- module: `@opentelemetry/sdk-metrics`
- asset: runtime library
- rail: metrics

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: provider and reader family
- rail: metrics

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]  | [RAIL]                                        |
| :-----: | :------------------------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `MeterProvider`                        | class          | SDK `IMeterProvider` implementation           |
|  [02]   | `MeterProviderOptions`                 | interface      | resource, views, readers, sdkMetricsEnabled   |
|  [03]   | `MetricReader`                         | abstract class | base abstract metric reader                   |
|  [04]   | `IMetricReader`                        | interface      | reader contract for setMetricProducer etc     |
|  [05]   | `MetricReaderOptions`                  | interface      | aggregation/temporality/cardinality selectors |
|  [06]   | `PeriodicExportingMetricReader`        | class          | interval-based push reader                    |
|  [07]   | `PeriodicExportingMetricReaderOptions` | type           | exporter, intervals, cardinality limits       |
|  [08]   | `InMemoryMetricExporter`               | class          | in-process metric sink                        |
|  [09]   | `ConsoleMetricExporter`                | class          | console metric sink                           |

[PUBLIC_TYPE_SCOPE]: data model family
- rail: metrics

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [RAIL]                                                   |
| :-----: | :------------------------------- | :------------ | :------------------------------------------------------- |
|  [01]   | `InstrumentType`                 | enum          | COUNTER, GAUGE, HISTOGRAM, UP_DOWN_COUNTER, OBSERVABLE_* |
|  [02]   | `AggregationTemporality`         | enum          | DELTA = 0, CUMULATIVE = 1                                |
|  [03]   | `DataPointType`                  | enum          | SUM, GAUGE, HISTOGRAM, EXPONENTIAL_HISTOGRAM             |
|  [04]   | `AggregationType`                | enum          | aggregation option selector                              |
|  [05]   | `MetricDescriptor`               | interface     | name, description, unit, valueType                       |
|  [06]   | `SumMetricData`                  | interface     | monotonic sum data points                                |
|  [07]   | `GaugeMetricData`                | interface     | gauge data points                                        |
|  [08]   | `HistogramMetricData`            | interface     | histogram data points                                    |
|  [09]   | `ExponentialHistogramMetricData` | interface     | exponential histogram data points                        |
|  [10]   | `ResourceMetrics`                | interface     | resource + scope metrics collection                      |
|  [11]   | `ScopeMetrics`                   | interface     | instrumentation scope + metrics                          |
|  [12]   | `CollectionResult`               | interface     | resourceMetrics + errors array                           |

[PUBLIC_TYPE_SCOPE]: producer and exporter family
- rail: metrics

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [RAIL]                                           |
| :-----: | :------------------------------- | :------------ | :----------------------------------------------- |
|  [01]   | `MetricProducer`                 | interface     | collect(options?) -> Promise\<CollectionResult\> |
|  [02]   | `MetricCollectOptions`           | interface     | timeoutMillis for async observable callbacks     |
|  [03]   | `PushMetricExporter`             | interface     | export + forceFlush + shutdown                   |
|  [04]   | `AggregationSelector`            | type          | InstrumentType -> AggregationOption              |
|  [05]   | `AggregationTemporalitySelector` | type          | InstrumentType -> AggregationTemporality         |
|  [06]   | `TimeoutError`                   | class         | timeout during collection or flush               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: MeterProvider construction and lifecycle
- rail: metrics

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY]   | [RAIL]                                   |
| :-----: | :----------------------------------------- | :--------------- | :--------------------------------------- |
|  [01]   | `new MeterProvider(options?)`              | provider factory | SDK meter provider with readers + views  |
|  [02]   | `provider.getMeter(name, version?, opts?)` | meter access     | returns `IMeter` for instrument creation |
|  [03]   | `provider.shutdown(options?)`              | lifecycle        | flush all readers and shut down          |
|  [04]   | `provider.forceFlush(options?)`            | lifecycle        | flush all buffered metric data           |

[ENTRYPOINT_SCOPE]: MetricReader abstract interface
- rail: metrics

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [RAIL]                               |
| :-----: | :---------------------------------------------- | :------------- | :----------------------------------- |
|  [01]   | `reader.setMetricProducer(metricProducer)`      | SDK wiring     | called once by SDK to bind producer  |
|  [02]   | `reader.selectAggregation(instrumentType)`      | aggregation    | per-instrument aggregation option    |
|  [03]   | `reader.selectAggregationTemporality(type)`     | aggregation    | DELTA or CUMULATIVE per instrument   |
|  [04]   | `reader.selectCardinalityLimit(instrumentType)` | cardinality    | max time series per instrument       |
|  [05]   | `reader.collect(options?)`                      | collection     | pull all metrics from bound producer |
|  [06]   | `reader.shutdown(options?)`                     | lifecycle      | stop reader, flush pending data      |
|  [07]   | `reader.forceFlush(options?)`                   | lifecycle      | collect and export immediately       |

[ENTRYPOINT_SCOPE]: PeriodicExportingMetricReader construction
- rail: metrics

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [RAIL]                                     |
| :-----: | :------------------------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `new PeriodicExportingMetricReader(options)` | reader factory | interval-based reader wrapping an exporter |

[ENTRYPOINT_SCOPE]: attribute processor factories
- rail: metrics

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :---------------------------------------------- | :------------- | :------------------------------ |
|  [01]   | `createAllowListAttributesProcessor(allowList)` | factory        | keep only listed attribute keys |
|  [02]   | `createDenyListAttributesProcessor(denyList)`   | factory        | drop listed attribute keys      |

## [04]-[IMPLEMENTATION_LAW]

[METRICS_TOPOLOGY]:
- `MeterProvider` is the SDK root: it accepts `readers` (push or pull `IMetricReader` implementations), `views` (`ViewOptions[]` for aggregation and attribute filtering), and an optional `Resource`
- `MetricReader` is abstract; implementors provide `onShutdown` and `onForceFlush`; `setMetricProducer` is called once by the SDK and must throw on second call
- `PeriodicExportingMetricReader` wraps any `PushMetricExporter` and drives collection on `exportIntervalMillis` (default 60 s) with `exportTimeoutMillis` (default 30 s) timeout
- `InstrumentType` enum values are strings; `AggregationTemporality` values are integers (DELTA = 0, CUMULATIVE = 1)

[LOCAL_ADMISSION]:
- Register readers at `MeterProvider` construction via `options.readers`; readers added after construction are not supported
- Cardinality limits per instrument default to 2000 time series; override via `PeriodicExportingMetricReaderOptions.cardinalityLimits` or `MetricReaderOptions.cardinalitySelector`
- `MetricProducer` is the seam for injecting external metric sources (e.g. `@effect/opentelemetry` Metrics bridge); pass via `MetricReaderOptions.metricProducers`

[RAIL_LAW]:
- Package: `@opentelemetry/sdk-metrics`
- Owns: SDK-side metric aggregation, reader lifecycle, data model
- Accept: `MeterProvider` as SDK metrics root; `PeriodicExportingMetricReader` for push export
- Reject: direct `MetricProducer.collect` calls from application code; hand-rolled aggregation outside views/readers
