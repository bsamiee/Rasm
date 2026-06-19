# [API_CATALOGUE] @opentelemetry/exporter-metrics-otlp-http

`@opentelemetry/exporter-metrics-otlp-http` supplies `OTLPMetricExporter`, the concrete browser `PushMetricExporter` that serializes `ResourceMetrics` to OTLP/JSON over HTTP, plus `OTLPMetricExporterBase`, the temporality/aggregation selectors (`CumulativeTemporalitySelector`, `DeltaTemporalitySelector`, `LowMemoryTemporalitySelector`), the `AggregationTemporalityPreference` enum, and the `OTLPMetricExporterOptions` type the `@effect/opentelemetry` `WebSdk` binds under a `PeriodicExportingMetricReader`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/exporter-metrics-otlp-http`
- package: `@opentelemetry/exporter-metrics-otlp-http`
- module: `@opentelemetry/exporter-metrics-otlp-http`
- asset: runtime library
- rail: metrics

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: exporter and option family
- rail: metrics

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY] | [RAIL]                                                  |
| :-----: | :--------------------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `OTLPMetricExporter`               | class         | browser `PushMetricExporter` over OTLP/HTTP             |
|  [02]   | `OTLPMetricExporterBase`           | class         | temporality/aggregation-aware push exporter base        |
|  [03]   | `OTLPMetricExporterOptions`        | interface     | `temporalityPreference`, `aggregationPreference` + base |
|  [04]   | `AggregationTemporalityPreference` | enum          | `DELTA = 0`, `CUMULATIVE = 1`, `LOWMEMORY = 2`          |

[PUBLIC_TYPE_SCOPE]: temporality selector constants
- rail: metrics

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]     | [RAIL]                                              |
| :-----: | :------------------------------ | :---------------- | :-------------------------------------------------- |
|  [01]   | `CumulativeTemporalitySelector` | selector constant | always `CUMULATIVE` per instrument                  |
|  [02]   | `DeltaTemporalitySelector`      | selector constant | `DELTA` for counter/histogram, cumulative otherwise |
|  [03]   | `LowMemoryTemporalitySelector`  | selector constant | delta sums, cumulative histograms                   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: OTLPMetricExporter construction and reader contract
- rail: metrics

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY]   | [RAIL]                                       |
| :-----: | :-------------------------------------------- | :--------------- | :------------------------------------------- |
|  [01]   | `new OTLPMetricExporter(config?)`             | exporter factory | browser metric exporter construction         |
|  [02]   | `exporter.export(metrics, resultCallback)`    | metric export    | serialize `ResourceMetrics` and POST to OTLP |
|  [03]   | `exporter.selectAggregation(instrumentType)`  | aggregation      | per-instrument `AggregationOption`           |
|  [04]   | `exporter.selectAggregationTemporality(type)` | aggregation      | `DELTA` or `CUMULATIVE` per instrument       |
|  [05]   | `exporter.forceFlush()`                       | lifecycle        | resolve when pending exports drain           |
|  [06]   | `exporter.shutdown()`                         | lifecycle        | stop exporter and release the delegate       |

[ENTRYPOINT_SCOPE]: `OTLPMetricExporterOptions` fields
- rail: metrics

| [INDEX] | [FIELD]                 | [TYPE]                                                       | [DEFAULT]                           |
| :-----: | :---------------------- | :----------------------------------------------------------- | :---------------------------------- |
|  [01]   | `temporalityPreference` | `AggregationTemporalityPreference \| AggregationTemporality` | `CUMULATIVE`                        |
|  [02]   | `aggregationPreference` | `AggregationSelector`                                        | default per instrument              |
|  [03]   | `url`                   | `string`                                                     | `—` (delegate appends `v1/metrics`) |
|  [04]   | `headers`               | `Record<string, string> \| HeadersFactory`                   | `—`                                 |
|  [05]   | `timeoutMillis`         | `number`                                                     | `10000`                             |

## [04]-[IMPLEMENTATION_LAW]

[METRIC_EXPORT_TOPOLOGY]:
- `OTLPMetricExporter` extends `OTLPMetricExporterBase` which extends `OTLPExporterBase<ResourceMetrics>` and implements `PushMetricExporter`; the browser build wraps the OTLP network export delegate over Fetch/Beacon.
- The browser constructor accepts `OTLPExporterConfigBase & OTLPMetricExporterOptions`; the Node build accepts the node config base — admit the browser intersection shape only.
- `temporalityPreference` selects the per-instrument `AggregationTemporality`; the three exported selector constants (`Cumulative`, `Delta`, `LowMemory`) realize the spec-defined temporality maps and feed `selectAggregationTemporality`.
- `selectAggregation`/`selectAggregationTemporality` are the `PushMetricExporter` contract methods the `PeriodicExportingMetricReader` calls per instrument; `export` is callback-style over `CollectionResult`-derived `ResourceMetrics`.

[LOCAL_ADMISSION]:
- Construct `OTLPMetricExporter` as the concrete `PushMetricExporter` wrapped by `PeriodicExportingMetricReader` from `@opentelemetry/sdk-metrics`; the `WebSdk` binds that reader on the `MeterProvider`.
- Set `temporalityPreference` to `AggregationTemporalityPreference.DELTA` for delta-native backends; default `CUMULATIVE` matches Prometheus-style collectors.
- Drive collection cadence from `PeriodicExportingMetricReaderOptions.exportIntervalMillis`, never by calling `export` directly.

[RAIL_LAW]:
- Package: `@opentelemetry/exporter-metrics-otlp-http`
- Owns: browser OTLP/HTTP metric serialization, temporality selection, collector transport
- Accept: `OTLPMetricExporter` as the `PushMetricExporter` the `WebSdk` requires
- Reject: hand-rolled metric POST clients; the Node exporter variant in browser code; direct `export` invocation outside the periodic reader
