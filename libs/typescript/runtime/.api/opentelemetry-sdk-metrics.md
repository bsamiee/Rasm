# [TS_RUNTIME_API_OPENTELEMETRY_SDK_METRICS]

`@opentelemetry/sdk-metrics` owns the metric collection→export pipeline: the pull-based `MetricReader`, the `View`/`AggregationOption` algebra reshaping instruments before export, the per-instrument-type selector policy, and the `MetricData` discriminated-union wire shape. `otel/emit` never constructs a `MeterProvider` directly — the facade wires `Configuration.metricReader` through `Metrics.layer`, feeding Effect's built-in `Metric` signals into the reader. It collapses with the pin block at `[OTEL_PIN_BLOCK]`; the native `Otlp` metric lane serializes Effect metrics to OTLP with no reader.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/sdk-metrics`
- package: `@opentelemetry/sdk-metrics` (Apache-2.0)
- module: dual — CJS default (`build/src/index.js`, no `"type"` field) + ESM mirror (`build/esm/index.js`); flat barrel, no `exports` subpath map.
- asset: TSDECL `build/src/index.d.ts` (restored).
- peer: `@opentelemetry/api >=catalog <catalog` — the tightest API floor of the block (the metric API stabilized at 1.9); deps `@opentelemetry/core` (`ExportResult`), `@opentelemetry/resources` (`Resource`). No `sdk-metrics-web` split — the reader is runtime-neutral.
- runtime: runtime-neutral — `PeriodicExportingMetricReader` uses a plain interval; no platform conditional export.
- plane: `plane:runtime`, edge-ledger-fenced to `scope:runtime`.
- rail: observability/sdk-bridge; `[OTEL_PIN_BLOCK]` collapse target.
- role: the `MetricReader`/`View`/`AggregationOption` roster behind `@effect/opentelemetry` `NodeSdk`/`WebSdk` `Configuration.metricReader`.

## [02]-[PROVIDER]

`MeterProvider` is the meter factory; every axis — resource, views, readers — is a `MeterProviderOptions` field, never a subclass. Under the effect facade the provider is constructed by `Metrics.layer`, so a telemetry consumer supplies the reader and views, not the provider.

| [INDEX] | [SYMBOL]               | [KIND]    | [CONSUMER_BOUNDARY]                                             |
| :-----: | :--------------------- | :-------- | :-------------------------------------------------------------- |
|  [01]   | `MeterProvider`        | class     | `implements IMeterProvider`; `getMeter`/`shutdown`/`forceFlush` |
|  [02]   | `MeterProviderOptions` | interface | `resource?`/`views?: ViewOptions[]`/`readers?: IMetricReader[]` |

[METER_PROVIDER_OPTIONS]: `MeterProviderOptions.resource: Resource` `MeterProviderOptions.views: ViewOptions[]` `MeterProviderOptions.readers: IMetricReader[]` `MeterProviderOptions.sdkMetricsEnabled: boolean`
[METER_PROVIDER]: `MeterProvider(MeterProviderOptions?)` `MeterProvider.getMeter(string,string?,MeterOptions?) -> Meter` `MeterProvider.shutdown({timeoutMillis?:number}?) -> Promise<void>` `MeterProvider.forceFlush({timeoutMillis?:number}?) -> Promise<void>`

## [03]-[READER_AND_EXPORTER]

`MetricReader` (abstract) owns pull-collection and the selector policy `selectAggregation`/`selectAggregationTemporality`/`selectCardinalityLimit`; `PeriodicExportingMetricReader` is the production row pulling on an interval into a `PushMetricExporter`, which owns format/transport (`Console`/`InMemory` built-in, OTLP-HTTP the production sibling). Selectors are `(InstrumentType) => X` pure policy functions with `DEFAULT_*` constants — a bespoke policy is a selector value, never a reader subclass. `MetricProducer` is the pull-source a reader collects from.

| [INDEX] | [SYMBOL]                                  | [KIND]               | [CAPABILITY_BOUNDARY]                                             |
| :-----: | :---------------------------------------- | :------------------- | :---------------------------------------------------------------- |
|  [01]   | `MetricReader` / `IMetricReader`          | abstract / interface | pull-collect + the three per-instrument-type selectors            |
|  [02]   | `MetricReaderOptions`                     | interface            | the selectors + `metricProducers?` + `otelComponentType?`         |
|  [03]   | `PeriodicExportingMetricReader`           | class                | interval pull → push; the production reader                       |
|  [04]   | `PeriodicExportingMetricReaderOptions`    | type                 | the reader options record — every field in the fence below        |
|  [05]   | `PushMetricExporter`                      | interface            | `export(ResourceMetrics, cb)` + temporality/aggregation selectors |
|  [06]   | `ConsoleMetricExporter`                   | class                | stdout diagnostics; `constructor({ temporalitySelector? })`       |
|  [07]   | `InMemoryMetricExporter`                  | class                | `getMetrics()`/`reset()`; `constructor(AggregationTemporality)`   |
|  [08]   | `MetricProducer` / `MetricCollectOptions` | interface            | external pull-source; `collect({ timeoutMillis? })`               |
|  [09]   | `AggregationSelector`                     | type                 | `(InstrumentType) => AggregationOption`                           |
|  [10]   | `AggregationTemporalitySelector`          | type                 | `(InstrumentType) => AggregationTemporality`                      |
|  [11]   | `AggregationTemporality`                  | enum                 | `DELTA` / `CUMULATIVE`                                            |
|  [12]   | `TimeoutError`                            | class                | thrown on collection/flush deadline; collection continues         |

[PERIODIC_EXPORTING_METRIC_READER_OPTIONS]: `PeriodicExportingMetricReaderOptions.exporter: PushMetricExporter` `PeriodicExportingMetricReaderOptions.exportIntervalMillis: number` `PeriodicExportingMetricReaderOptions.exportTimeoutMillis: number` `PeriodicExportingMetricReaderOptions.metricProducers: MetricProducer[]` `PeriodicExportingMetricReaderOptions.cardinalityLimits: {…}` `PeriodicExportingMetricReaderOptions.maxExportBatchSize: number`
[PERIODIC_EXPORTING_METRIC_READER]: `PeriodicExportingMetricReader(PeriodicExportingMetricReaderOptions)`
[PUSH_METRIC_EXPORTER]: `PushMetricExporter.export(ResourceMetrics,(result:ExportResult)=>void) -> void` `PushMetricExporter.forceFlush() -> Promise<void>` `PushMetricExporter.shutdown() -> Promise<void>` `PushMetricExporter.selectAggregationTemporality(InstrumentType) -> AggregationTemporality` `PushMetricExporter.selectAggregation(InstrumentType) -> AggregationOption`
[AGGREGATION_TEMPORALITY_SELECTOR]: `AggregationTemporalitySelector = (instrumentType:InstrumentType)=>AggregationTemporality`
[SURFACES]: `DEFAULT_AGGREGATION_TEMPORALITY_SELECTOR: AggregationTemporalitySelector`

## [04]-[VIEW_AND_AGGREGATION]

A `View` reshapes matched instruments (rename, drop, re-aggregate, cap cardinality, filter attributes) before export; it is declared as a `ViewOptions` object on `MeterProviderOptions.views` (the `View` class is internal). `AggregationOption` is a discriminated union keyed on `AggregationType` — the six variants are ROWS, and a bespoke histogram bucketing is an option-object value, never a new class. `IAttributesProcessor` is the cardinality-control interface; `createAllowList`/`createDenyList` are its parameterized constructors.

| [INDEX] | [SYMBOL]                             | [KIND]       | [SELECTS_PRODUCES]                                                            |
| :-----: | :----------------------------------- | :----------- | :---------------------------------------------------------------------------- |
|  [01]   | `ViewOptions`                        | type         | instrument matchers + reshaping (rename/drop/re-aggregate)                    |
|  [02]   | `AggregationType`                    | enum         | `DEFAULT/DROP/SUM/LAST_VALUE/EXPLICIT_BUCKET_HISTOGRAM/EXPONENTIAL_HISTOGRAM` |
|  [03]   | `AggregationOption`                  | tagged union | `{ type: AggregationType; options? }` — one shape per aggregation             |
|  [04]   | `IAttributesProcessor`               | interface    | `process(attrs, ctx?) => Attributes` — cardinality control                    |
|  [05]   | `createAllowListAttributesProcessor` | fn           | allow attribute-key set — the parameterized constructor                       |
|  [06]   | `createDenyListAttributesProcessor`  | fn           | deny attribute-key set — the parameterized constructor                        |

[VIEW_OPTIONS]: `ViewOptions.name: string` `ViewOptions.description: string` `ViewOptions.aggregation: AggregationOption` `ViewOptions.aggregationCardinalityLimit: number` `ViewOptions.attributesProcessors: IAttributesProcessor[]` `ViewOptions.instrumentType: InstrumentType` `ViewOptions.instrumentName: string` `ViewOptions.instrumentUnit: string` `ViewOptions.meterName: string` `ViewOptions.meterVersion: string` `ViewOptions.meterSchemaUrl: string`
[AGGREGATION_OPTION]: `AggregationOption = |{type:AggregationType.DEFAULT}|…`
[IATTRIBUTES_PROCESSOR]: `IAttributesProcessor.process: (incoming:Attributes,context?:Context)=>Attributes`

## [05]-[METRIC_DATA]

`MetricData` is the wire shape the exporter receives: ONE discriminated union keyed on `dataPointType`; `DataPoint<T>` is the parameterized point (`T = number | Histogram | ExponentialHistogram`). Collection nests `ResourceMetrics → ScopeMetrics[] → MetricData[]` — one shape, never a per-instrument struct. `InstrumentType` and `DataPointType` are the discriminants every selector and exporter key on.

| [INDEX] | [SYMBOL]                             | [KIND]              | [SHAPE]                                                        |
| :-----: | :----------------------------------- | :------------------ | :------------------------------------------------------------- |
|  [01]   | `InstrumentType`                     | enum                | `COUNTER`/`GAUGE`/`HISTOGRAM`/`UP_DOWN_COUNTER`/`OBSERVABLE_*` |
|  [02]   | `DataPointType`                      | enum                | `SUM`/`GAUGE`/`HISTOGRAM`/`EXPONENTIAL_HISTOGRAM`              |
|  [03]   | `MetricData`                         | discriminated union | `Sum`/`Gauge`/`Histogram`/`ExponentialHistogram` metric data   |
|  [04]   | `DataPoint<T>`                       | interface           | `{ startTime, endTime, attributes, value: T }`                 |
|  [05]   | `Sum` / `LastValue`                  | type                | aggregated point-value shapes                                  |
|  [06]   | `Histogram` / `ExponentialHistogram` | type                | bucketed point-value shapes                                    |
|  [07]   | `MetricDescriptor`                   | interface           | `{ name, description, unit, valueType }`                       |
|  [08]   | `ResourceMetrics`                    | interface           | `{ resource, scopeMetrics: ScopeMetrics[] }`                   |
|  [09]   | `ScopeMetrics`                       | interface           | `{ scope, metrics: MetricData[] }`                             |
|  [10]   | `CollectionResult`                   | interface           | `{ resourceMetrics, errors[] }`                                |

[METRIC_DATA]: `MetricData = SumMetricData|GaugeMetricData|HistogramMetricData|ExponentialHistogramMetricData`
[DATA_POINT]: `DataPoint.startTime: HrTime` `DataPoint.endTime: HrTime` `DataPoint.attributes: Attributes` `DataPoint.value: T`
[HISTOGRAM]: `Histogram.buckets: {boundaries:number[];counts:number[]}` `Histogram.sum: number` `Histogram.count: number` `Histogram.min: number` `Histogram.max: number`
[RESOURCE_METRICS]: `ResourceMetrics.resource: Resource` `ResourceMetrics.scopeMetrics: ScopeMetrics[]`
[COLLECTION_RESULT]: `CollectionResult.resourceMetrics: ResourceMetrics` `CollectionResult.errors: unknown[]`

## [06]-[STACKING]

- Stack with `@effect/opentelemetry` `NodeSdk`/`WebSdk`: the primary consumer. `Configuration.metricReader: MetricReader | ReadonlyArray<MetricReader>` is exactly this package's `MetricReader` (verified import); effect wires it via `Metrics.layer(constant(metricReader), config)`, registering a `MetricProducer` that feeds Effect's built-in `Metric` values into the reader. `otel/emit` passes `new PeriodicExportingMetricReader({ exporter: new OTLPMetricExporter(opts), exportIntervalMillis })` — never a `MeterProvider` directly.
- Stack with sibling `exporter-metrics-otlp-http`: `OTLPMetricExporter implements PushMetricExporter`; it carries `AggregationTemporalityPreference` (`Cumulative`/`Delta`/`LowMemory` selectors) satisfying the exporter's optional `selectAggregationTemporality?`, so OTLP delta-vs-cumulative is an exporter policy, not a reader fork. `PeriodicExportingMetricReader(new OTLPMetricExporter(opts))` is the production metric pipeline; `ExportResult` comes from `@opentelemetry/core`.
- Stack with `@opentelemetry/resources` + `semantic-conventions`: `MeterProviderOptions.resource` carries the `AppIdentity`-derived resource (supplied by the effect `Resource` layer under the facade); `MetricDescriptor.name` and `DataPoint.attributes` keys are `semantic-conventions` vocabulary rows (`telemetry/core/observe/convention`).
- Stack with fact-journal meter cardinality: the `(app, tenant)`-keyed usage/cost counters are exactly the high-cardinality case `View` + `createAllowListAttributesProcessor` and `aggregationCardinalityLimit` govern; the metering fact stream is durable through the `data` fact journal, while the OTLP metric egress here is the aggregate rollup. Instrumentation emits through `Effect.Metric`; no `plane:runtime` folder imports `sdk-metrics` (edge-ledger `scope:runtime` ban).

## [07]-[RAIL_LAW]

- Owns: the metric collection→export pipeline — `MetricReader`/`PeriodicExportingMetricReader` + the three per-instrument-type selectors, `PushMetricExporter` + `Console`/`InMemory` rows, the `View`/`AggregationOption`/`AttributesProcessor` reshaping algebra, and the `MetricData` discriminated-union wire shape.
- Accept: `PeriodicExportingMetricReader` wrapping an `OTLPMetricExporter` for production; `ViewOptions` with `AggregationOption` + `createAllowList`/`createDenyList` for cardinality control; the exporter's `selectAggregationTemporality?` for delta/cumulative preference; `InMemoryMetricExporter(temporality)` for kit-driven specs; the whole surface reached through `@effect/opentelemetry` `NodeSdk`/`WebSdk` `Configuration.metricReader`.
- Reject: constructing `MeterProvider` inline under the effect facade (`Metrics.layer` owns it); a reader subclass where a selector value suffices; a new metric struct where the `MetricData` union + `DataPoint<T>` already discriminates; importing outside `scope:runtime`; treating this leg as permanent — it collapses at `[OTEL_PIN_BLOCK]`.
- Boundary: `View` is configured as `ViewOptions` (the class is internal); `Aggregator`/`AggregatorKind` and `createNoopAttributesProcessor`/`createMultiAttributesProcessor` are internal (not barrel exports); `CollectionResult.errors` carries partial-collection faults — collection surfaces errors rather than throwing.
