# [TS_RUNTIME_API_OPENTELEMETRY_SDK_METRICS]

`@opentelemetry/sdk-metrics` owns the metric collect→export pipeline: the `MeterProvider`, the pull-based `MetricReader` with its per-instrument aggregation/temporality/cardinality selectors, the `ViewOptions`/`AggregationOption` reshaping algebra, and the `MetricData` union the `PushMetricExporter` receives.

`@effect/opentelemetry` constructs the provider and drains Effect `Metric` values into a `PeriodicExportingMetricReader`, so a consumer supplies readers, views, and exporters, never the provider.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/sdk-metrics`
- package: `@opentelemetry/sdk-metrics` (Apache-2.0)
- module: ESM (`build/esm`) + CJS (`build/src`) mirror; one flat barrel, no subpath export map
- runtime: runtime-neutral — `PeriodicExportingMetricReader` drives a plain interval, no platform-conditional export
- depends: `@opentelemetry/api` (`Meter`/`ValueType`/`Attributes`/`HrTime` peer), `@opentelemetry/core` (`ExportResult`/`InstrumentationScope`), `@opentelemetry/resources` (`Resource`)
- rail: observability/metric-sdk — the `MetricReader`/`View` roster behind the facade `Configuration.metricReader`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the collect→export pipeline — provider, reader, exporter, and their option records

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]        | [CAPABILITY]                                           |
| :-----: | :---------------------------------------- | :------------------- | :----------------------------------------------------- |
|  [01]   | `MeterProvider`                           | class                | `getMeter`/`shutdown`/`forceFlush` meter factory       |
|  [02]   | `MeterProviderOptions`                    | interface            | `resource?`/`views?`/`readers?` axes, never a subclass |
|  [03]   | `MetricReader` / `IMetricReader`          | abstract / interface | pull-collect + the three per-instrument-type selectors |
|  [04]   | `MetricReaderOptions`                     | interface            | selector policy fns + `metricProducers?`               |
|  [05]   | `PeriodicExportingMetricReader`           | class                | interval pull→push production reader                   |
|  [06]   | `PeriodicExportingMetricReaderOptions`    | type                 | reader knobs over one `exporter`                       |
|  [07]   | `PushMetricExporter`                      | interface            | `export(ResourceMetrics, cb)` + optional selectors     |
|  [08]   | `MetricProducer` / `MetricCollectOptions` | interface            | external pull-source a reader collects from            |
|  [09]   | `ConsoleMetricExporter`                   | class                | stdout diagnostics exporter                            |
|  [10]   | `InMemoryMetricExporter`                  | class                | spec-buffer exporter — `getMetrics`/`reset`            |
|  [11]   | `AggregationSelector`                     | type                 | `(InstrumentType) => AggregationOption` policy         |
|  [12]   | `AggregationTemporalitySelector`          | type                 | `(InstrumentType) => AggregationTemporality` policy    |
|  [13]   | `AggregationTemporality`                  | enum                 | `DELTA` / `CUMULATIVE`                                 |
|  [14]   | `TimeoutError`                            | class                | collection/flush deadline fault                        |

- `MetricReaderOptions`: `aggregationSelector?`/`aggregationTemporalitySelector?`/`cardinalitySelector?` are pure `(InstrumentType) => X` fns; `metricProducers?` adds external aggregate sources whose resource the SDK ignores.
- `PeriodicExportingMetricReaderOptions`: `exporter` required; `exportIntervalMillis?`/`exportTimeoutMillis?`/`metricProducers?`/`cardinalityLimits?` (per-instrument-type record)/`maxExportBatchSize?` optional.
- `PushMetricExporter`: `selectAggregationTemporality?` and `selectAggregation?` are optional per-`InstrumentType` policy the reader defers to when present.

[PUBLIC_TYPE_SCOPE]: the `View` algebra reshaping matched instruments before export

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :--------------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `ViewOptions`          | type          | instrument matchers + rename/drop/re-aggregate/cap reshaping   |
|  [02]   | `AggregationType`      | enum          | `DEFAULT`/`DROP`/`SUM`/`LAST_VALUE`/`*_HISTOGRAM` discriminant |
|  [03]   | `AggregationOption`    | union         | `{ type: AggregationType; options? }` per aggregation          |
|  [04]   | `IAttributesProcessor` | interface     | `process(Attributes, Context?) => Attributes` dimension fold   |

- `AggregationOption`: `EXPLICIT_BUCKET_HISTOGRAM` carries `options?: { boundaries; recordMinMax? }`, `EXPONENTIAL_HISTOGRAM` carries `options?: { maxSize?; recordMinMax? }`; the other four are bare `{ type }`.

[PUBLIC_TYPE_SCOPE]: the `MetricData` wire shape the exporter receives

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :----------------------------------- | :------------ | :-------------------------------------------------------------- |
|  [01]   | `InstrumentType`                     | enum          | `COUNTER`/`GAUGE`/`HISTOGRAM`/`UP_DOWN_COUNTER`/`OBSERVABLE_*`  |
|  [02]   | `DataPointType`                      | enum          | `HISTOGRAM`/`EXPONENTIAL_HISTOGRAM`/`GAUGE`/`SUM` discriminant  |
|  [03]   | `MetricData`                         | union         | `Sum`/`Gauge`/`Histogram`/`ExponentialHistogram` metric data    |
|  [04]   | `DataPoint<T>`                       | interface     | `{ startTime; endTime; attributes; value: T }` aggregated point |
|  [05]   | `Sum` / `LastValue`                  | type          | `number` point-value aliases                                    |
|  [06]   | `Histogram` / `ExponentialHistogram` | interface     | bucketed point-value shapes                                     |
|  [07]   | `MetricDescriptor`                   | interface     | `{ name; description; unit; valueType }`                        |
|  [08]   | `ResourceMetrics`                    | interface     | `{ resource; scopeMetrics: ScopeMetrics[] }`                    |
|  [09]   | `ScopeMetrics`                       | interface     | `{ scope; metrics: MetricData[] }`                              |
|  [10]   | `CollectionResult`                   | interface     | `{ resourceMetrics; errors: unknown[] }`                        |

- `DataPoint<T>`: `T` resolves to `number` for `Sum`/`Gauge`, `Histogram` for the explicit-bucket point, `ExponentialHistogram` for the exponential point — one parameterized point, never a per-instrument struct.
- `Histogram`: `buckets` (`boundaries`/`counts`, `n+1` counts for `n` boundaries) and `count` always present; `sum`/`min`/`max` optional.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider construction and meter minting

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :---------------------------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `new MeterProvider(MeterProviderOptions?)`                        | ctor     | seat resource, views, and readers |
|  [02]   | `MeterProvider.getMeter(string, string?, MeterOptions?) -> Meter` | instance | mint a named/versioned meter      |
|  [03]   | `MeterProvider.forceFlush(ForceFlushOptions?) -> Promise<void>`   | instance | flush every registered reader     |
|  [04]   | `MeterProvider.shutdown(ShutdownOptions?) -> Promise<void>`       | instance | shut down provider and readers    |

[ENTRYPOINT_SCOPE]: reader, exporters, and pull collection

| [INDEX] | [SURFACE]                                                                 | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :------------------------------------------------------------------------ | :------- | :------------------------------ |
|  [01]   | `new PeriodicExportingMetricReader(PeriodicExportingMetricReaderOptions)` | ctor     | interval pull into one exporter |
|  [02]   | `MetricReader.collect(CollectionOptions?) -> Promise<CollectionResult>`   | instance | one pull from the producer      |
|  [03]   | `new ConsoleMetricExporter({ temporalitySelector }?)`                     | ctor     | stdout diagnostics exporter     |
|  [04]   | `new InMemoryMetricExporter(AggregationTemporality)`                      | ctor     | spec-buffer exporter            |
|  [05]   | `InMemoryMetricExporter.getMetrics() -> ResourceMetrics[]`                | instance | read buffered collections       |
|  [06]   | `InMemoryMetricExporter.reset()`                                          | instance | clear the buffer                |

- `MetricReader.collect`: a throwing observable callback aggregates into `CollectionResult.errors` while successfully collected metrics still return — collection never throws on a single-callback fault.
- `MetricReader.shutdown`/`forceFlush`: the operation MAY keep running after the returned promise rejects on timeout.

[ENTRYPOINT_SCOPE]: view attribute-processor constructors for cardinality control

| [INDEX] | [SURFACE]                                                              | [SHAPE] | [CAPABILITY]                    |
| :-----: | :--------------------------------------------------------------------- | :------ | :------------------------------ |
|  [01]   | `createAllowListAttributesProcessor(string[]) -> IAttributesProcessor` | factory | keep only listed attribute keys |
|  [02]   | `createDenyListAttributesProcessor(string[]) -> IAttributesProcessor`  | factory | drop listed attribute keys      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Aggregation temporality is per-instrument policy: an `AggregationTemporalitySelector` maps each `InstrumentType` to `DELTA` or `CUMULATIVE`, defaulting to `CUMULATIVE` for every instrument when unset, and a `PushMetricExporter.selectAggregationTemporality` overrides the reader's selection.
- A `ViewOptions` matches instruments by `instrumentType`, wildcard `instrumentName`, and exact `meterName`/`meterVersion`/`meterSchemaUrl`, then reshapes the matched stream — rename, `AggregationType.DROP`, re-aggregate, attribute-filter, `aggregationCardinalityLimit` — and `attributesProcessors` apply in list order.
- Aggregation is a discriminated `AggregationOption` value keyed on `AggregationType`, so a bespoke histogram bucketing is an `options` object, never a reader or aggregator subclass.
- Collection nests `ResourceMetrics -> ScopeMetrics[] -> MetricData[]`; `MetricData` discriminates on `dataPointType` and `DataPoint<T>` parameterizes the point value, so one union carries every instrument shape.

[STACKING]:
- `@effect/opentelemetry`(`.api/effect-opentelemetry.md`): `Metrics.layer`/`Metrics.registerProducer` register a `MetricProducer` draining Effect `Metric` values into this package's `MetricReader`, and `NodeSdk`/`WebSdk` `Configuration.metricReader` takes exactly that `MetricReader` (or a readonly array) — the facade constructs the `MeterProvider`, never a direct `new MeterProvider`.
- `@opentelemetry/exporter-metrics-otlp-http`(`.api/opentelemetry-exporter-metrics-otlp-http.md`) + `-proto`(`.api/opentelemetry-exporter-metrics-otlp-proto.md`): `OTLPMetricExporter` implements `PushMetricExporter`; `new PeriodicExportingMetricReader({ exporter, exportIntervalMillis })` wraps it, its `AggregationTemporalityPreference` satisfies the optional `selectAggregationTemporality`, and `export` reports through core's `ExportResult`.
- `@opentelemetry/resources`(`.api/opentelemetry-resources.md`): `MeterProviderOptions.resource` takes the one `AppIdentity`-derived `Resource`, so `ResourceMetrics.resource` carries the same `service.name` spans and logs stamp.
- `@opentelemetry/host-metrics`(`.api/opentelemetry-host-metrics.md`) + `instrumentation-runtime-node`(`.api/opentelemetry-instrumentation-runtime-node.md`): both register observables on a `Meter` from this provider, and a `createDenyListAttributesProcessor` `ViewOptions` drops the high-cardinality `v8js.heap.space.name`/`v8js.gc.type` dimensions the engine collector fans.
- `otel/meter.ts` (within-lib): the work-plane fact-to-instrument bridge composes the `(app, tenant)`-keyed census gauges and tenant views — exactly the case `ViewOptions` + `createAllowListAttributesProcessor` + `aggregationCardinalityLimit` govern — while `InMemoryMetricExporter(temporality)` backs the census specs and `otel/emit.ts` wires the reader onto the OTLP egress Layer.

[LOCAL_ADMISSION]:
- `@opentelemetry/*` admits only inside `scope:runtime` (edge-ledger ban); the `otel/` folder composes this surface, and every other folder emits through Effect's native `Metric` rather than importing `sdk-metrics`.
- design code reaches the reader/exporter/view surface through the `@effect/opentelemetry` `Configuration.metricReader`, constructing `MeterProvider` directly only where an SDK-only spec harness demands it; `InMemoryMetricExporter` backs those specs.
- it persists as an `[OTEL_PIN_BLOCK]` member retiring when native `OtlpMetrics` reaches parity; `.api/effect-opentelemetry.md` owns the collapse roster.

[RAIL_LAW]:
- Package: `@opentelemetry/sdk-metrics`
- Owns: the metric collect→export pipeline — `MeterProvider`, `MetricReader`/`PeriodicExportingMetricReader` with the per-instrument-type aggregation/temporality/cardinality selectors, the `PushMetricExporter` contract with `Console`/`InMemory` rows, the `ViewOptions`/`AggregationOption`/`IAttributesProcessor` reshaping algebra, and the `MetricData` discriminated-union wire shape
- Accept: `PeriodicExportingMetricReader` wrapping an `OTLPMetricExporter` reached through `@effect/opentelemetry` `Configuration.metricReader`; `ViewOptions` with `AggregationOption` + `createAllowList`/`createDenyList` + `aggregationCardinalityLimit` for cardinality control; the exporter's `selectAggregationTemporality` for `DELTA`/`CUMULATIVE`; `InMemoryMetricExporter(temporality)` for specs; one `AppIdentity`-derived `Resource` at construction
- Reject: a direct `new MeterProvider` under the facade (`Metrics.layer` owns it); a reader or aggregator subclass where a selector value or `AggregationOption` suffices; a new metric struct where `MetricData` + `DataPoint<T>` discriminates; `@opentelemetry/*` outside `scope:runtime`; mutable attribute accumulation instead of an `IAttributesProcessor` fold
