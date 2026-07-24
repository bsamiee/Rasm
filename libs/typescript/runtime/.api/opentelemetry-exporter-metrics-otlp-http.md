# [TS_RUNTIME_API_OPENTELEMETRY_EXPORTER_METRICS_OTLP_HTTP]

`@opentelemetry/exporter-metrics-otlp-http` mints the concrete `PushMetricExporter` that POSTs `ResourceMetrics` to an OTLP/HTTP collector, carrying the one axis the trace exporter lacks: the `(InstrumentType) => AggregationTemporality` selection fixing DELTA vs CUMULATIVE per instrument. A `PeriodicExportingMetricReader` wraps it and feeds the facade's `NodeSdk`/`WebSdk` `Configuration.metricReader`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/exporter-metrics-otlp-http`
- package: `@opentelemetry/exporter-metrics-otlp-http` (Apache-2.0)
- module: ESM; the export map selects `platform/node` or `platform/browser` at build, exposing one `OTLPMetricExporter` name — never a fork
- runtime: dual — node ctor config `OTLPExporterNodeConfigBase & OTLPMetricExporterOptions`, browser `OTLPExporterConfigBase & OTLPMetricExporterOptions` (base config from `@opentelemetry/otlp-exporter-base`)
- rail: observability/export/metric

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `PushMetricExporter` and its temporality algebra — one exporter over one base, the richness in the `(InstrumentType) => AggregationTemporality` selection; `temporalityPreference` takes a prebuilt `AggregationTemporalityPreference` or a raw `AggregationTemporality`, `aggregationPreference` the parallel `AggregationSelector` for bucket/aggregation choice

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY] | [CAPABILITY]                                             |
| :-----: | :------------------------------------------------ | :------------ | :------------------------------------------------------- |
|  [01]   | `OTLPMetricExporter`                              | class         | the exporter a `PeriodicExportingMetricReader` wraps     |
|  [02]   | `OTLPMetricExporterBase`                          | class         | base ops + the per-`InstrumentType` selector methods     |
|  [03]   | `OTLPMetricExporterOptions`                       | interface     | `temporalityPreference` + `aggregationPreference` policy |
|  [04]   | `AggregationTemporalityPreference`                | enum          | `DELTA = 0` / `CUMULATIVE = 1` / `LOWMEMORY = 2`         |
|  [05]   | `{Cumulative,Delta,LowMemory}TemporalitySelector` | selector      | three `AggregationTemporalitySelector` seeds             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: SDK-bridge metric export composition — construct with a `temporalityPreference`, wrap in `new PeriodicExportingMetricReader({ exporter, exportIntervalMillis })`, hand the reader to `NodeSdk`/`WebSdk` `Configuration.metricReader`; `url`/`headers`/`compression`/`timeoutMillis` default from base config or `OTEL_EXPORTER_OTLP_METRICS_*`

| [INDEX] | [SURFACE]                                                       | [SHAPE]  | [CAPABILITY]                                         |
| :-----: | :-------------------------------------------------------------- | :------- | :--------------------------------------------------- |
|  [01]   | `new OTLPMetricExporter(nodeCfg)`                               | ctor     | the node OTLP/HTTP metric exporter                   |
|  [02]   | `new OTLPMetricExporter(browserCfg)`                            | ctor     | the browser exporter — RUM metric egress             |
|  [03]   | `temporalityPreference: AggregationTemporalityPreference.DELTA` | property | select DELTA/CUMULATIVE/LOWMEMORY without a subclass |
|  [04]   | `setSelfObsMeterProvider(MeterProvider)`                        | instance | meter the exporter's own export path                 |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- SDK-bridge lane: this exporter is the `NodeSdk`/`WebSdk` metric leg wrapped in a `PeriodicExportingMetricReader`; reach for it only when SDK reader semantics, a temporality preference, or a co-resident SDK-only exporter bind. `.api/effect-opentelemetry.md` `[TOPOLOGY]` owns the native-first `[OTEL_PIN_BLOCK]`-collapse doctrine — this exporter is a pin-block member retiring when native `OtlpMetrics` reaches parity.
- temporality is a policy function, never a fork: DELTA vs CUMULATIVE vs LOWMEMORY is a `temporalityPreference` value or an `AggregationTemporalitySelector`, so a delta-counter backend is one policy row at the composition root, never a second exporter type.

[STACKING]:
- `@opentelemetry/sdk-metrics`(`.api/opentelemetry-sdk-metrics.md`): `new OTLPMetricExporter(cfg)` is a `PushMetricExporter` wrapped in `new PeriodicExportingMetricReader({ exporter, exportIntervalMillis })`; the reader owns the collect-and-push interval and calls `selectAggregationTemporality`/`selectAggregation` per `InstrumentType`.
- `@effect/opentelemetry`(`.api/effect-opentelemetry.md`): the wrapped reader feeds `NodeSdk`/`WebSdk` `Configuration.metricReader` alongside the one `AppIdentity`-derived `Resource`; Effect's `Metric` values drain through the SDK meter this reader reads.
- `@opentelemetry/core`(`.api/opentelemetry-core.md`): `export()` reports through `ExportResult`/`ExportResultCode`; the outbound `Context` is `suppressTracing`-fenced; `timeoutMillis`/`url` default from `OTEL_EXPORTER_OTLP_METRICS_*` via core's env readers.
- `data` fact journal (within-lib): the (app, tenant)-keyed request/compute/storage/token counters emit as Effect metrics — a DELTA `temporalityPreference` keeps the OTLP stream cheap for the billing roll-up, CUMULATIVE suits monotonic totals; the temporality selector is where the meter's cost model meets the wire.
- `@effect/platform`(`.api/effect-platform.md`): this exporter carries its own HTTP transport and does not inherit the `net/client` `HttpClient` retry/proxy policy — the concrete reason the native `OtlpMetrics` lane is the default.

[LOCAL_ADMISSION]:
- `@opentelemetry/*` admits ONLY inside `scope:runtime` (edge-ledger ban); the exporter constructs at the composition root, and instrumentation code emits through Effect's native `Metric`.
- reach for this SDK exporter only for SDK-only reader/exporter capability or an explicit temporality preference; the browser exporter is the RUM metric egress leg (`otel/vital`), and export-boundary redaction rows apply before the metric leaves the browser.

[RAIL_LAW]:
- Package: `@opentelemetry/exporter-metrics-otlp-http`
- Owns: OTLP/HTTP metric serialization — one `OTLPMetricExporter` (`PushMetricExporter`) over a node or browser transport, with the aggregation-temporality/aggregation selection algebra (`AggregationTemporalityPreference` + the three `AggregationTemporalitySelector` seeds)
- Accept: `new OTLPMetricExporter(cfg)` wrapped in a `PeriodicExportingMetricReader` fed to `NodeSdk`/`WebSdk` `Configuration.metricReader`; temporality as a `temporalityPreference` value or an `AggregationTemporalitySelector`; endpoint/runtime as config + platform-export selection; the one `AppIdentity`-derived `Resource`; core's `ExportResult` rail
- Reject: `@opentelemetry/*` outside `scope:runtime`, this exporter where native `OtlpMetrics` suffices, a subclass per temporality/backend, an unwrapped exporter handed straight to the facade
