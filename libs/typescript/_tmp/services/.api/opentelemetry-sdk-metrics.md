# [API_CATALOGUE] @opentelemetry/sdk-metrics

`@opentelemetry/sdk-metrics` is the SDK-bridge metrics reader/exporter the `execution/slo#SLO_BUDGET` owner rides: `MeterProvider` is the sole composition root, `PeriodicExportingMetricReader` drives a `PushMetricExporter` on an interval, and `MetricProducer` is the seam external sources feed in. The v2 line collapsed the aggregation family from a class-per-algorithm hierarchy into the data-driven `AggregationType` enum + `AggregationOption` `{ type, options? }` union, and the per-instrument policy is three pure selector rails — `AggregationSelector`/`AggregationTemporalitySelector`/`CardinalitySelector`, each `(InstrumentType) => X`. This package is admitted ONLY inside `scope:telemetry` (the edge-ledger fence, `.api/effect-opentelemetry.md`): folders emit through Effect's native `Metric`, and `@effect/opentelemetry` `Metrics.layer`/`NodeSdk.layer` wraps THIS SDK's reader — never a folder-level import. The `execution/slo#SLO_BUDGET` owner pairs this reader with the sibling `@opentelemetry/sdk-trace-node` `NodeTracerProvider` (`.api/opentelemetry-sdk-trace-node.md`): the `MeterProvider` collects the `latency`/`availability` request metrics while the `@effect/opentelemetry` `Metrics` bridge exports the Effect registry's `@effect/cluster` `ClusterMetrics` `saturation` gauges (`.api/effect-cluster.md`) through it, the `NodeTracerProvider` decodes the C#-minted trace context, and both readers end at the one OTLP exporter edge — never a second emitter. It is the `[R3]`-collapse candidate the native `OtlpMetrics` lane retires once parity closes.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/sdk-metrics`
- package: `@opentelemetry/sdk-metrics` (2.8.0, Apache-2.0, © OpenTelemetry Authors)
- module format: dual CJS/ESM (`build/src/index.js`), types `build/src/index.d.ts`; barrel re-export — pure-JS, zero native ABI
- runtime target: runtime-neutral (node + browser); in `services` it is node-side and edge-ledger-fenced to `scope:telemetry`
- otel-peer: `@opentelemetry/api` (the `Meter`/`Context`/`HrTime`/`Attributes`/`ValueType` surface), `@opentelemetry/core` 2.8.0 (`ExportResult`, `InstrumentationScope`), `@opentelemetry/resources` 2.8.0 (`Resource`), `@opentelemetry/semantic-conventions` 1.41.1
- catalog-verdict: KEEP but FENCED — `@opentelemetry/*` admitted only in `scope:telemetry`; `[R3]`-collapse target once `@effect/opentelemetry` native `OtlpMetrics` reaches parity
- asset: `MeterProvider`, the `MetricReader`/`PeriodicExportingMetricReader` readers, the `InMemoryMetricExporter`/`ConsoleMetricExporter` exporters, the `PushMetricExporter`/`MetricProducer`/`IMetricReader` contracts, the instrument/temporality/aggregation enums, the metric-data shapes, and the attribute-processor factories
- consumer: `execution/slo#SLO_BUDGET` — the `MeterProvider`/`PeriodicExportingMetricReader`/`MetricProducer` collecting the `latency`/`availability` request metrics and exporting the `@effect/cluster` `ClusterMetrics` `saturation` gauges (`.api/effect-cluster.md`) through the `MetricProducer` bridge; paired with `.api/opentelemetry-sdk-trace-node.md` for the trace side
- rail: telemetry / metrics

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: provider and reader family
`MeterProvider` is the one root; readers register at construction via `MeterProviderOptions.readers` (there is no add-reader-after-init API). `MetricReader` is the spec base every reader extends; `PeriodicExportingMetricReader` is the interval-driven push reader wrapping a `PushMetricExporter`.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]  | [CONSUMER / BOUNDARY]                                             |
| :-----: | :-------------------------------- | :------------- | :--------------------------------------------------------------- |
|  [01]   | `MeterProvider`                   | class          | the composition root; `getMeter` returns an `@opentelemetry/api` `Meter` |
|  [02]   | `MeterProviderOptions`            | interface      | `{ resource?, views?: ViewOptions[], readers?: IMetricReader[], sdkMetricsEnabled? }` — no `metricProducers` here |
|  [03]   | `MetricReader` / `IMetricReader`  | abstract class + interface | spec-base reader; `collect`/`forceFlush`/`shutdown`/`setMetricProducer`/`select*` |
|  [04]   | `MetricReaderOptions`             | interface      | `{ aggregationSelector?, aggregationTemporalitySelector?, cardinalitySelector?, metricProducers? }` |
|  [05]   | `PeriodicExportingMetricReader`   | class          | interval push reader over a `PushMetricExporter`                  |
|  [06]   | `PeriodicExportingMetricReaderOptions` | type      | `{ exporter, exportIntervalMillis?, exportTimeoutMillis?, metricProducers?, cardinalityLimits? }` |
|  [07]   | `PushMetricExporter`              | interface      | `export(metrics, cb)`/`forceFlush()`/`shutdown()` + optional `selectAggregation?`/`selectAggregationTemporality?` |
|  [08]   | `MetricProducer` / `MetricCollectOptions` | interface | external source — `collect(options?): Promise<CollectionResult>` |
|  [09]   | `InMemoryMetricExporter` / `ConsoleMetricExporter` | class | test-capture / diagnostic exporters (never a production sink)   |
|  [10]   | `TimeoutError`                    | class          | collect/flush timeout failure                                    |

[PUBLIC_TYPE_SCOPE]: aggregation and selector family — the parameterized policy axis
The v2 collapse: aggregation is chosen by the `AggregationType` enum + `AggregationOption` data union (the old `SumAggregation`/`HistogramAggregation`/… classes are no longer barrel-exported). Two selector rails are barrel-exported; the cardinality rail is option-carried, and the readers' default selectors (cumulative temporality, per-instrument default aggregation) are internal fallbacks, not exported constants.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]  | [CONSUMER / BOUNDARY]                                             |
| :-----: | :-------------------------------- | :------------- | :--------------------------------------------------------------- |
|  [01]   | `AggregationType`                 | enum           | `DEFAULT`/`DROP`/`SUM`/`LAST_VALUE`/`EXPLICIT_BUCKET_HISTOGRAM`/`EXPONENTIAL_HISTOGRAM` |
|  [02]   | `AggregationOption`               | union type     | `{ type: AggregationType; options? }` — histogram/exp-histogram carry `boundaries`/`maxSize`/`recordMinMax` |
|  [03]   | `AggregationSelector`             | type           | `(instrumentType: InstrumentType) => AggregationOption`          |
|  [04]   | `AggregationTemporalitySelector`  | type           | `(instrumentType: InstrumentType) => AggregationTemporality`     |
|  [05]   | cardinality rail (option-carried) | structural     | `MetricReaderOptions.cardinalitySelector?: (InstrumentType) => number` + `cardinalityLimits` (not a barrel-exported name) |
|  [06]   | `AggregationTemporality`          | enum           | `DELTA = 0`, `CUMULATIVE = 1`                                     |
|  [07]   | `InstrumentType`                  | enum           | `COUNTER`/`GAUGE`/`HISTOGRAM`/`UP_DOWN_COUNTER`/`OBSERVABLE_COUNTER`/`OBSERVABLE_GAUGE`/`OBSERVABLE_UP_DOWN_COUNTER` |
|  [08]   | `ViewOptions`                     | type           | metric-stream reshape — `name`/`description`/`aggregation`/`attributesProcessors`/`aggregationCardinalityLimit` + `instrument*`/`meter*` selection |
|  [09]   | `IAttributesProcessor`            | interface      | `process(incoming, context?) => Attributes` — dimension allow/deny |

[PUBLIC_TYPE_SCOPE]: metric-data shapes — the `DataPointType`-discriminated export tree
`ResourceMetrics` is the top export envelope the exporter receives; `MetricData` is a union discriminated by `dataPointType`; `DataPoint<T>` is the per-attribute point whose value type follows the variant.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]  | [CONSUMER / BOUNDARY]                                             |
| :-----: | :-------------------------------- | :------------- | :--------------------------------------------------------------- |
|  [01]   | `ResourceMetrics` / `ScopeMetrics` | interface     | `{ resource, scopeMetrics[] }` / `{ scope, metrics[] }` export envelope |
|  [02]   | `MetricData`                      | union          | `SumMetricData \| GaugeMetricData \| HistogramMetricData \| ExponentialHistogramMetricData` |
|  [03]   | `DataPointType`                   | enum           | `HISTOGRAM`/`EXPONENTIAL_HISTOGRAM`/`GAUGE`/`SUM` — the `MetricData` discriminant |
|  [04]   | `DataPoint<T>`                    | interface      | `{ startTime: HrTime, endTime: HrTime, attributes: Attributes, value: T }` |
|  [05]   | `MetricDescriptor`                | interface      | `{ name, description, unit, valueType }`                          |
|  [06]   | `Sum` / `LastValue` / `Histogram` / `ExponentialHistogram` | type | the accumulated value shapes carried by each `DataPoint` variant |
|  [07]   | `CollectionResult`                | interface      | `{ resourceMetrics, errors: unknown[] }` — partial-failure aware  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider lifecycle and reader construction
The reader set is fixed at `MeterProvider` construction; the provider owns `getMeter` (delegating to the `api` `Meter` for instrument creation) plus `forceFlush`/`shutdown` over all readers.

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                 |
| :-----: | :------------------------------------------------------------ | :------------- | :--------------------------------------------------- |
|  [01]   | `new MeterProvider(options?: MeterProviderOptions)`          | constructor    | provider with `readers`/`views`/`resource`           |
|  [02]   | `provider.getMeter(name, version?, options?): api.Meter`     | factory        | a named `Meter`; instruments (`createCounter`/`createHistogram`/`createObservableGauge`) are `@opentelemetry/api` members |
|  [03]   | `provider.forceFlush(options?)` / `provider.shutdown(options?)` | lifecycle   | flush / flush-and-close all readers                  |
|  [04]   | `new PeriodicExportingMetricReader({ exporter, exportIntervalMillis?, exportTimeoutMillis?, metricProducers?, cardinalityLimits? })` | constructor | interval push reader over a `PushMetricExporter` |
|  [05]   | `reader.collect(options?)` / `reader.forceFlush()` / `reader.setMetricProducer(p)` | reader | manual collect / flush / SDK-internal producer bind |
|  [06]   | `new InMemoryMetricExporter(aggregationTemporality)` / `new ConsoleMetricExporter(options?)` | constructor | test-capture / diagnostic exporters |

[ENTRYPOINT_SCOPE]: attribute-processor factories — the egress dimension policy
The allow/deny factories build `IAttributesProcessor`s for `ViewOptions.attributesProcessors`, dropping or retaining metric dimensions — the PII-scrub seam the `telemetry` redaction policy composes.

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                 |
| :-----: | :------------------------------------------------------------ | :------------- | :--------------------------------------------------- |
|  [01]   | `createAllowListAttributesProcessor(allowList: string[])`    | factory        | retain only listed attribute keys                    |
|  [02]   | `createDenyListAttributesProcessor(denyList: string[])`      | factory        | drop listed attribute keys                           |

```ts contract
import {
  PeriodicExportingMetricReader, AggregationType, AggregationTemporality,
  InstrumentType, type AggregationSelector, type AggregationTemporalitySelector, type PushMetricExporter,
} from "@opentelemetry/sdk-metrics"
import { Metrics } from "@effect/opentelemetry"  // .api/effect-opentelemetry.md — the SDK-bridge lane

// Policy is data-driven: one selector rail per instrument type, never a class-per-aggregation.
const aggregation: AggregationSelector = (t) =>
  t === InstrumentType.HISTOGRAM
    ? { type: AggregationType.EXPLICIT_BUCKET_HISTOGRAM, options: { boundaries: [10, 100, 1000] } }
    : { type: AggregationType.DEFAULT }

const temporality: AggregationTemporalitySelector = () => AggregationTemporality.DELTA

// The reader wraps a PushMetricExporter (e.g. @opentelemetry/exporter-metrics-otlp-http OTLPMetricExporter).
declare const exporter: PushMetricExporter
const reader = new PeriodicExportingMetricReader({ exporter, exportIntervalMillis: 60_000 })

// @effect/opentelemetry owns the wiring: Metrics.layer feeds Effect's native Metric registry into THIS reader.
const MetricsLayer = Metrics.layer(() => reader)  // never a folder-level MeterProvider import
```

## [04]-[IMPLEMENTATION_LAW]

[METRICS_TOPOLOGY]:
- `MeterProvider` is the sole root; readers register once via `MeterProviderOptions.readers` — rebuild the provider to reconfigure, there is no add-reader API. `metricProducers` is a READER option (`MetricReaderOptions`/`PeriodicExportingMetricReaderOptions`), NOT a provider option.
- `PeriodicExportingMetricReader` drives its `PushMetricExporter` every `exportIntervalMillis` with an `exportTimeoutMillis` deadline; `cardinalityLimits` caps unique attribute combinations per instrument type (default 2000).
- Instrument creation (`createCounter`/`createHistogram`/`createObservableGauge`/…) lives on the `@opentelemetry/api` `Meter` that `getMeter` returns, not on this SDK; this package owns the reader/exporter/view/aggregation machinery below the instrument.
- Aggregation is data, not a class: pick via `AggregationOption` (`{ type: AggregationType.EXPLICIT_BUCKET_HISTOGRAM, options: { boundaries } }`) on a `ViewOptions.aggregation` or an `AggregationSelector`; the old `SumAggregation`/`HistogramAggregation` classes are internal to v2 and not part of the public barrel.

[LOCAL_ADMISSION]:
- Wire one `MeterProvider` per telemetry composition root with `readers` at construction; `DELTA` temporality when the downstream collector is delta-aware, `CUMULATIVE` otherwise, chosen per instrument through `AggregationTemporalitySelector`.
- `InMemoryMetricExporter` is test-only; `ConsoleMetricExporter` is diagnostic-only — the production sink is a `PushMetricExporter` such as `@opentelemetry/exporter-metrics-otlp-http` `OTLPMetricExporter` (workspace-cataloged at 0.219.0).
- Every `@opentelemetry/*` import stays inside `scope:telemetry`; no `services` folder imports this package directly.

[STACKING]:
- `@effect/opentelemetry` SDK-bridge (`.api/effect-opentelemetry.md`) is the primary consumer: `Metrics.layer(() => reader)` feeds Effect's native `Metric` registry into this SDK's `PeriodicExportingMetricReader`, and `NodeSdk.layer({ metricReader })` wires the reader into the runtime; `Metrics.makeProducer`/`registerProducer` yields the `MetricProducer` — the seam external sources feed in — off that Effect registry, which a `MeterProvider`/reader collects. The registry carries both the `latency`/`availability` request metrics AND the `@effect/cluster` `ClusterMetrics` gauges (`entities`/`singletons`/`runners`/`runnersHealthy`/`shards` `Metric.Metric.Gauge<bigint>`, `.api/effect-cluster.md` `[SATURATION_STACKING]`), so the five `saturation` gauges reach the OTLP edge through THIS reader without a second exporter. Effect's `Metric` is the emit surface, this SDK is the read/export boundary — the two never both mint a signal.
- `execution/slo#SLO_BUDGET` reads the `latency`/`availability` objectives off the `MeterProvider`-collected request series and sources the `saturation` objective from the `@effect/cluster` `ClusterMetrics` gauges (`runnersHealthy`/`runners`) read Effect-natively via `Metric.value` — the same registry gauges this reader exports through the `Metrics` bridge — pairs the trace side via `@opentelemetry/sdk-trace-node` `NodeTracerProvider` (`.api/opentelemetry-sdk-trace-node.md`) to decode-and-reattach the C#-minted trace context, and both readers end at the one OTLP exporter edge — never a second emitter beside the `provisioning/contract#PROVISIONING` `ObservabilityStack` collector.
- Egress redaction: `createAllowListAttributesProcessor`/`createDenyListAttributesProcessor` on `ViewOptions.attributesProcessors` scrub PII dimensions at collect time, composing with the `telemetry/otlp/export` redaction policy so no unlisted attribute crosses the export boundary.
- `[R3]` collapse: this SDK is the fallback reader/exporter block the native `@effect/opentelemetry` `Otlp`/`OtlpMetrics` lane retires once it reaches parity; `semantic-conventions` survives as the signal-name vocabulary, the rest of the `@opentelemetry/*` peer set collapses.

[RAIL_LAW]:
- package: `@opentelemetry/sdk-metrics`
- owns: the OTel metrics SDK reader/exporter machinery — `MeterProvider`, `MetricReader`/`PeriodicExportingMetricReader`, the `PushMetricExporter`/`MetricProducer` contracts, the data-driven aggregation/temporality/cardinality selectors, `ViewOptions`, and the `DataPointType`-discriminated metric-data shapes
- accept: one `MeterProvider` per telemetry root with `readers` at construction; `PushMetricExporter` production sinks; `AggregationOption` data + `(InstrumentType) => X` selectors for policy; consumption through `@effect/opentelemetry` `Metrics.layer`/`NodeSdk.layer` — the `Metrics.makeProducer` `MetricProducer` carrying the Effect registry's `@effect/cluster` `ClusterMetrics` `saturation` gauges plus the request metrics — never a folder import
- reject: hand-rolled metric accumulation outside `MeterProvider`; a `metricProducers` on `MeterProviderOptions` (reader-only); the retired aggregation classes where `AggregationType`/`AggregationOption` is the public surface; `@opentelemetry/*` imports outside `scope:telemetry`; `InMemoryMetricExporter` in a production path
