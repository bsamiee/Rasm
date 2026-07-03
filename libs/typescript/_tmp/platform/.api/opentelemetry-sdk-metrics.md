# [API_CATALOGUE] @opentelemetry/sdk-metrics

`@opentelemetry/sdk-metrics` supplies the SDK-side metric collection engine: `MeterProvider` (the `IMeterProvider` root binding readers + views + resource), the `MetricReader`/`IMetricReader` reader contract, `PeriodicExportingMetricReader` (the interval-driven push reader that wraps a `PushMetricExporter`), the `View`/`ViewOptions` metric-stream rewriter, the `AggregationType`/`AggregationOption` and `AggregationTemporality`/`DataPointType`/`InstrumentType` vocabularies, the `MetricData` discriminated family with its `Sum`/`LastValue`/`Histogram`/`ExponentialHistogram` point-value types, and the `MetricProducer` seam. In the pinned WebSdk lane the design page builds a `PeriodicExportingMetricReader({ exporter: new OTLPMetricExporter(...) })` and hands it to `WebSdk.Configuration.metricReader`; `MetricProducer` is the seam through which `@effect/opentelemetry` `Metrics.makeProducer` feeds Effect's own metric registry into the same reader. Every exporter reports through the `@opentelemetry/core` `ExportResult` callback shape.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/sdk-metrics`
- package: `@opentelemetry/sdk-metrics`
- version: `2.8.0` (central pin `pnpm-workspace.yaml`; matches `@effect/opentelemetry@0.63.0` peer resolution)
- license: `Apache-2.0`
- api-peer: `@opentelemetry/api ^1.9.x` — `MeterProvider`/`Meter`/`MeterOptions`/`ValueType`/`HrTime`/`Attributes`; consumes `@opentelemetry/core` `ExportResult`/`InstrumentationScope`/`TimeoutError` and `@opentelemetry/resources` `Resource`
- module: `@opentelemetry/sdk-metrics` (barrel `build/src/index.d.ts`)
- runtime: dual — browser + node; the WebSdk lane binds it in the browser under `PeriodicExportingMetricReader`
- asset: runtime library — side-effects-free (`sideEffects: false`), tree-shakeable
- rail: metrics
- collapse-fence: SDK-bridge peer block, fenced to `scope:telemetry`, COLLAPSES at `[R3]` when the native `@effect/opentelemetry` `OtlpMetrics.layer` reaches parity (`libs/typescript/.api/effect-opentelemetry.md`) — the push metric pipeline (`PeriodicExportingMetricReader` + `MeterProvider`) retires while the propagation (`@opentelemetry/core`) / resource-identity (`@opentelemetry/resources`) / convention (`@opentelemetry/semantic-conventions`) vocabulary survives

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: provider and reader family
- rail: metrics

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]  | [CONSUMER / BOUNDARY]                                     |
| :-----: | :------------------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `MeterProvider`                        | class          | SDK `IMeterProvider`; binds `readers`/`views`/`resource` |
|  [02]   | `MeterProviderOptions`                 | interface      | `resource?`/`views?: ViewOptions[]`/`readers?: IMetricReader[]`/`sdkMetricsEnabled?` |
|  [03]   | `MetricReader`                         | abstract class | reader base; implementors add `onShutdown`/`onForceFlush` |
|  [04]   | `IMetricReader`                        | interface      | reader contract (`setMetricProducer`/`select*`/`collect`) |
|  [05]   | `MetricReaderOptions`                  | interface      | `aggregationSelector?`/`aggregationTemporalitySelector?`/`cardinalitySelector?`/`metricProducers?`/`otelComponentType?` |
|  [06]   | `PeriodicExportingMetricReader`        | class          | interval-driven push reader over a `PushMetricExporter`   |
|  [07]   | `PeriodicExportingMetricReaderOptions` | type           | `exporter`/`exportIntervalMillis?`/`exportTimeoutMillis?`/`metricProducers?`/`cardinalityLimits?` |
|  [08]   | `InMemoryMetricExporter`               | class          | in-process metric sink (test/inspection)                 |
|  [09]   | `ConsoleMetricExporter`                | class          | console metric sink (diagnostic)                         |

[PUBLIC_TYPE_SCOPE]: data-model family (`MetricData` discriminated union)
- rail: metrics
- The four `*MetricData` shapes are ONE `BaseMetricData` discriminated by `dataPointType`; `MetricData` is the union alias. `DataPoint<T>`'s `T` is the aggregator point-value type (`Sum`/`LastValue`/`Histogram`/`ExponentialHistogram`). Enum numeric values are load-bearing — `DataPointType` is NOT in `SUM,GAUGE,...` textual order.

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [CONSUMER / BOUNDARY]                                       |
| :-----: | :------------------------------- | :------------ | :--------------------------------------------------------- |
|  [01]   | `InstrumentType`                 | enum (string) | `COUNTER`/`GAUGE`/`HISTOGRAM`/`UP_DOWN_COUNTER`/`OBSERVABLE_COUNTER`/`OBSERVABLE_GAUGE`/`OBSERVABLE_UP_DOWN_COUNTER` |
|  [02]   | `AggregationTemporality`         | enum (number) | `DELTA = 0`, `CUMULATIVE = 1`                              |
|  [03]   | `DataPointType`                  | enum (number) | `HISTOGRAM = 0`, `EXPONENTIAL_HISTOGRAM = 1`, `GAUGE = 2`, `SUM = 3` |
|  [04]   | `MetricDescriptor`               | interface     | `{ name; description; unit; valueType }` (readonly)        |
|  [05]   | `SumMetricData`                  | interface     | `dataPointType: SUM`; `dataPoints: DataPoint<number>[]`; `isMonotonic` |
|  [06]   | `GaugeMetricData`                | interface     | `dataPointType: GAUGE`; `dataPoints: DataPoint<number>[]`  |
|  [07]   | `HistogramMetricData`            | interface     | `dataPointType: HISTOGRAM`; `dataPoints: DataPoint<Histogram>[]` |
|  [08]   | `ExponentialHistogramMetricData` | interface     | `dataPointType: EXPONENTIAL_HISTOGRAM`; `DataPoint<ExponentialHistogram>[]` |
|  [09]   | `MetricData`                     | type (union)  | `Sum \| Gauge \| Histogram \| ExponentialHistogram` metric data |
|  [10]   | `DataPoint<T>`                   | interface     | `{ startTime; endTime; attributes; value: T }` (HrTime bounds) |
|  [11]   | `Sum` / `LastValue`              | type          | `number` — sum / last-value point value                    |
|  [12]   | `Histogram` / `ExponentialHistogram` | interface | bucket/count/sum/min/max point-value structures            |
|  [13]   | `ResourceMetrics` / `ScopeMetrics` | interface   | `{ resource; scopeMetrics }` / `{ scope; metrics }` collection tree |
|  [14]   | `CollectionResult`               | interface     | `{ resourceMetrics; errors: unknown[] }` (partial-failure aware) |

[PUBLIC_TYPE_SCOPE]: producer, exporter, and view family
- rail: metrics
- `AggregationOption` is a discriminated union over `AggregationType`; `ViewOptions` is the metric-stream rewriter (selection criteria + stream alteration). The attribute processors are ONE factory family (`allowList`/`denyList`) producing an `IAttributesProcessor`.

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [CONSUMER / BOUNDARY]                                       |
| :-----: | :------------------------------- | :------------ | :--------------------------------------------------------- |
|  [01]   | `MetricProducer`                 | interface     | `collect(options?: MetricCollectOptions) => Promise<CollectionResult>` |
|  [02]   | `MetricCollectOptions`           | interface     | `{ timeoutMillis? }` for async observable callbacks        |
|  [03]   | `PushMetricExporter`             | interface     | `export(metrics, cb: (r: ExportResult) => void)` + `forceFlush`/`shutdown` + optional `select*` |
|  [04]   | `AggregationSelector`            | type          | `(InstrumentType) => AggregationOption`                    |
|  [05]   | `AggregationTemporalitySelector` | type          | `(InstrumentType) => AggregationTemporality`               |
|  [06]   | `AggregationType`                | enum (number) | `DEFAULT=0`/`DROP=1`/`SUM=2`/`LAST_VALUE=3`/`EXPLICIT_BUCKET_HISTOGRAM=4`/`EXPONENTIAL_HISTOGRAM=5` |
|  [07]   | `AggregationOption`              | type (union)  | `{ type: AggregationType; options?: {...} }` per aggregation |
|  [08]   | `ViewOptions`                    | type          | stream alteration (`name`/`description`/`aggregation`/`attributesProcessors`) + instrument/meter selection |
|  [09]   | `IAttributesProcessor`           | interface     | `{ process(incoming, context?) => Attributes }`            |
|  [10]   | `TimeoutError`                   | class         | re-export of `@opentelemetry/core` `TimeoutError`          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: MeterProvider construction and lifecycle
- rail: metrics

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY]   | [CONSUMER / BOUNDARY]                                     |
| :-----: | :----------------------------------------- | :--------------- | :------------------------------------------------------- |
|  [01]   | `new MeterProvider(options?: MeterProviderOptions)` | provider factory | SDK meter provider with readers + views + resource |
|  [02]   | `provider.getMeter(name, version?, options?: MeterOptions)` | meter access | returns api `Meter` for instrument creation      |
|  [03]   | `provider.shutdown(options?: ShutdownOptions)` | lifecycle    | flush + shut down all readers                            |
|  [04]   | `provider.forceFlush(options?: ForceFlushOptions)` | lifecycle | flush all buffered metric data                          |

[ENTRYPOINT_SCOPE]: MetricReader contract (abstract + interface)
- rail: metrics

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                               |
| :-----: | :---------------------------------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `reader.setMetricProducer(producer)`            | SDK wiring     | called ONCE by the SDK; MUST throw on second call  |
|  [02]   | `reader.selectAggregation(instrumentType)`      | aggregation    | per-instrument `AggregationOption`                 |
|  [03]   | `reader.selectAggregationTemporality(type)`     | aggregation    | `DELTA`/`CUMULATIVE` per instrument                |
|  [04]   | `reader.selectCardinalityLimit(instrumentType)` | cardinality    | max time series per instrument (default 2000)      |
|  [05]   | `reader.collect(options?: CollectionOptions)`   | collection     | pull all metrics from the bound producer           |
|  [06]   | `reader.shutdown(options?)` / `reader.forceFlush(options?)` | lifecycle | stop/flush; subclasses implement `onShutdown`/`onForceFlush` |

[ENTRYPOINT_SCOPE]: PeriodicExportingMetricReader + attribute processors
- rail: metrics

```ts
// build/src/export/PeriodicExportingMetricReader.d.ts + MeterProvider.d.ts — the WebSdk-lane construction
export interface MeterProviderOptions {
  resource?: Resource;                 // @opentelemetry/resources
  views?: ViewOptions[];
  readers?: IMetricReader[];
  sdkMetricsEnabled?: boolean;         // @experimental
}
export type PeriodicExportingMetricReaderOptions = {
  exporter: PushMetricExporter;        // e.g. new OTLPMetricExporter(...) from exporter-metrics-otlp-http
  exportIntervalMillis?: number;       // default 60000
  exportTimeoutMillis?: number;        // default 30000
  metricProducers?: MetricProducer[];  // @experimental — the @effect/opentelemetry Metrics.makeProducer seam
  cardinalityLimits?: {                // per-instrument-type, NOT a flat number; default 2000
    counter?: number; gauge?: number; histogram?: number; upDownCounter?: number;
    observableCounter?: number; observableGauge?: number; observableUpDownCounter?: number; default?: number;
  };
};
export declare class PeriodicExportingMetricReader extends MetricReader {
  constructor(options: PeriodicExportingMetricReaderOptions);
}

// build/src/view/AttributesProcessor.d.ts — dimension allow/deny filtering as a View attributesProcessor
export declare function createAllowListAttributesProcessor(allowList: string[]): IAttributesProcessor;
export declare function createDenyListAttributesProcessor(denyList: string[]): IAttributesProcessor;
```

## [04]-[IMPLEMENTATION_LAW]

[METRICS_TOPOLOGY]:
- `MeterProvider` is the SDK root: `readers` (push/pull `IMetricReader`), `views` (`ViewOptions[]` altering aggregation + attributes), and an optional `@opentelemetry/resources` `Resource` — readers added after construction are unsupported, register them via `options.readers` or, in the WebSdk lane, via `WebSdk.Configuration.metricReader`
- `MetricReader` is abstract; implementors provide `onShutdown`/`onForceFlush`; `setMetricProducer` is called once by the SDK and throws on a second call; the reader's `select*` methods MUST be pure
- `PeriodicExportingMetricReader` wraps any `PushMetricExporter` and drives collection on `exportIntervalMillis` (default 60 s) bounded by `exportTimeoutMillis` (default 30 s); it decides retry from the exporter's `ExportResult.code`
- enum discipline: `InstrumentType` values are STRINGS; `AggregationTemporality` is `DELTA=0`/`CUMULATIVE=1`; `DataPointType` is `HISTOGRAM=0`/`EXPONENTIAL_HISTOGRAM=1`/`GAUGE=2`/`SUM=3` (numeric order is NOT the textual order — never assume `SUM=0`)
- `View` (via `ViewOptions`) is the ONLY sanctioned metric-stream rewriter: instrument-selection criteria (`instrumentType`/`instrumentName` wildcard/`meterName`) plus stream alteration (`aggregation: AggregationOption`, `attributesProcessors`, `aggregationCardinalityLimit`); a hand-rolled aggregation outside a `View`/reader selector is the defect

[INTEGRATION_LAW]:
- Stack with `@effect/opentelemetry` `WebSdk`: build `new PeriodicExportingMetricReader({ exporter: new OTLPMetricExporter({ url }) })` and pass it as `WebSdk.Configuration.metricReader` (`MetricReader | ReadonlyArray<MetricReader>`); the WebSdk builds the `MeterProvider` internally over the shared `Resource`
- Stack with `@effect/opentelemetry` `Metrics`: `MetricProducer` is the injection seam — `Metrics.makeProducer`/`Metrics.registerProducer` yields a `MetricProducer` fed via `MetricReaderOptions.metricProducers`/`PeriodicExportingMetricReaderOptions.metricProducers`, so Effect's native `Metric` registry exports through the same reader as the SDK's own instruments (one metric spine)
- Stack with `exporter-metrics-otlp-http`: the sibling `OTLPMetricExporter` IS the `PushMetricExporter` the reader wraps; its `selectAggregationTemporality`/`selectAggregation` satisfy the optional exporter selectors, and its `export(metrics, cb)` reports the `@opentelemetry/core` `ExportResult`
- Stack with `@opentelemetry/core`: `PushMetricExporter.export`'s callback receives `ExportResult`/`ExportResultCode`; `TimeoutError` is re-exported from core; `ResourceMetrics.resource` is the `@opentelemetry/resources` `Resource`; `ScopeMetrics.scope` is the core `InstrumentationScope`
- Stack with `MetricBoundaries`: the Effect-side `Metric.histogram(name, boundaries)` in `Observability/telemetry` defines bucket layout on the Effect meter; the SDK `View` `AggregationType.EXPLICIT_BUCKET_HISTOGRAM` `options.boundaries` is the SDK-side equivalent when a raw `MeterProvider` is built

[LOCAL_ADMISSION]:
- register readers at `MeterProvider` construction (or via `WebSdk.Configuration.metricReader`); post-construction reader addition is unsupported
- cardinality limits default to 2000 time series/instrument — override via `PeriodicExportingMetricReaderOptions.cardinalityLimits` (the per-instrument-type record) or `MetricReaderOptions.cardinalitySelector`
- `@opentelemetry/sdk-metrics` is admitted ONLY inside `scope:telemetry`; instrumentation code uses Effect's native `Metric`, never this package directly

[RAIL_LAW]:
- Package: `@opentelemetry/sdk-metrics`
- Owns: SDK-side metric aggregation, the reader lifecycle, the `View`/`AggregationOption` stream rewriter, and the `MetricData`/`DataPoint` data model
- Accept: `MeterProvider` as the SDK metrics root; `PeriodicExportingMetricReader` wrapping the sibling `OTLPMetricExporter` for push export via `WebSdk.Configuration.metricReader`; `MetricProducer` as the `@effect/opentelemetry` `Metrics` injection seam; `View`/`ViewOptions` for stream alteration
- Reject: direct `MetricProducer.collect` from application code; readers added post-construction; hand-rolled aggregation outside views/reader selectors; assuming textual enum order for `DataPointType`
