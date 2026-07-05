# [@opentelemetry/exporter-metrics-otlp-http] — the SDK-bridge `PushMetricExporter` with the aggregation-temporality selection algebra, wrapped in a reader and fed to `NodeSdk`/`WebSdk`

`@opentelemetry/exporter-metrics-otlp-http` is the concrete `PushMetricExporter` that POSTs `ResourceMetrics` to an OTLP/HTTP collector, and it carries the one capability the trace exporter does not: the aggregation-temporality selection — the `(InstrumentType) => AggregationTemporality` policy that decides DELTA vs CUMULATIVE per instrument, the axis that makes a metric backend either cheap-to-store or monotonic-friendly. It is not composed directly: a `PeriodicExportingMetricReader` (from `@opentelemetry/sdk-metrics`) wraps it, and that reader is handed to the facade's `NodeSdk`/`WebSdk` `Configuration.metricReader`. Inside Rasm it is the SDK-bridge metric leg of the otlp export plane and the wire under the `data` fact journal's (app, tenant)-keyed meter counters — the fallback lane used when the SDK reader semantics or a temporality preference are required; the native `Otlp`/`OtlpMetrics` lane is the `[R3]`-preferred default over a `@effect/platform` `HttpClient`. The edge ledger fences `@opentelemetry/*` to `scope:runtime`; this exporter is an `[R3]`-collapse member of the `[OTLP_SDK]` block (retires when native `OtlpMetrics` reaches parity; `semantic-conventions` survives, this does not).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/exporter-metrics-otlp-http`
- package: `@opentelemetry/exporter-metrics-otlp-http`
- version: `0.219.0`
- license: `Apache-2.0`
- otel-peer: `@opentelemetry/api ^1.3.0`, `@opentelemetry/core ^2.8.0` (the `ExportResult` rail), `@opentelemetry/sdk-metrics ^2.8.0` (the `PushMetricExporter`/`ResourceMetrics`/`AggregationTemporality`/`InstrumentType` contract + the `PeriodicExportingMetricReader` that wraps it)
- transitive-config: `@opentelemetry/otlp-exporter-base` supplies the base constructor config (`OTLPExporterNodeConfigBase` / `OTLPExporterConfigBase`) intersected with `OTLPMetricExporterOptions`
- consumed-by: the otlp export plane's SDK-bridge metric leg via the facade's `NodeSdk`/`WebSdk` `Configuration.metricReader`; the wire under the `data` fact journal's usage/cost counters
- catalog-verdict: KEEP as SDK-bridge peer; edge-ledger fences `@opentelemetry/*` to `scope:runtime`; `[R3]`-collapse member (native `OtlpMetrics` supersedes)
- runtime: dual — the package export map selects `platform/node` (config `OTLPExporterNodeConfigBase & OTLPMetricExporterOptions`) or `platform/browser` (config `OTLPExporterConfigBase & OTLPMetricExporterOptions`); ONE `OTLPMetricExporter` name, a build-time platform selection, never a fork
- modules: `OTLPMetricExporter`, `OTLPMetricExporterBase`, `OTLPMetricExporterOptions`, `AggregationTemporalityPreference`, the three temporality selectors

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `PushMetricExporter` and its temporality algebra
- rail: observability/export/metric
- One exporter class over one base; the richness is the temporality/aggregation selection. Temporality is a `(InstrumentType) => AggregationTemporality` FUNCTION (`AggregationTemporalitySelector`), not a subclass per backend — the three named constants are seed rows of that one function space, and `temporalityPreference` accepts either a prebuilt `AggregationTemporalityPreference` or a raw `AggregationTemporality`. `aggregationPreference` is the parallel `(InstrumentType) => AggregationOption` selector for histogram-bucket/aggregation choice.

| [INDEX] | [SYMBOL]                                                        | [TYPE_FAMILY]        | [CONSUMER / BOUNDARY]                                              |
| :-----: | :-------------------------------------------------------------- | :------------------- | :---------------------------------------------------------------- |
|  [01]   | `OTLPMetricExporter` (`extends OTLPMetricExporterBase`)         | metric exporter      | the concrete exporter a `PeriodicExportingMetricReader` wraps      |
|  [02]   | `OTLPMetricExporterBase` (`extends OTLPExporterBase<ResourceMetrics> implements PushMetricExporter`) | base exporter | `export`/`forceFlush`/`shutdown` + the two selector methods |
|  [03]   | `selectAggregationTemporality(t: InstrumentType): AggregationTemporality` / `selectAggregation(t: InstrumentType): AggregationOption` | selector method | per-instrument temporality + aggregation resolution the reader calls |
|  [04]   | `OTLPMetricExporterOptions { temporalityPreference?: AggregationTemporalityPreference \| AggregationTemporality; aggregationPreference?: AggregationSelector }` | options | the temporality + aggregation policy on top of the base transport config |
|  [05]   | `AggregationTemporalityPreference { DELTA = 0, CUMULATIVE = 1, LOWMEMORY = 2 }` | temporality enum | the prebuilt preference `temporalityPreference` accepts        |
|  [06]   | `CumulativeTemporalitySelector` / `DeltaTemporalitySelector` / `LowMemoryTemporalitySelector` | selector const (`AggregationTemporalitySelector`) | the three seed rows of the `(InstrumentType) => AggregationTemporality` space |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: SDK-bridge metric export composition
- rail: observability/export/metric
- Construct the exporter with a `temporalityPreference`, wrap it in a `PeriodicExportingMetricReader({ exporter, exportIntervalMillis })`, hand the reader to the facade's `NodeSdk`/`WebSdk` `Configuration.metricReader`. `url`/`headers`/`compression`/`timeoutMillis` come from the base config or `OTEL_EXPORTER_OTLP_METRICS_*` env (via core's readers); the export interval is a reader policy value, never a fork.

| [INDEX] | [SURFACE]                                                                          | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                          |
| :-----: | :-------------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------ |
|  [01]   | `new OTLPMetricExporter(config?: OTLPExporterNodeConfigBase & OTLPMetricExporterOptions)` | node ctor | the node OTLP/HTTP metric exporter                            |
|  [02]   | `new OTLPMetricExporter(config?: OTLPExporterConfigBase & OTLPMetricExporterOptions)`     | browser ctor | the browser OTLP/HTTP metric exporter (RUM metric egress)   |
|  [03]   | `new PeriodicExportingMetricReader({ exporter: new OTLPMetricExporter(cfg), exportIntervalMillis })` → `Configuration.metricReader` | composition | the standing stack: exporter → reader → `NodeSdk`/`WebSdk` |
|  [04]   | `{ temporalityPreference: AggregationTemporalityPreference.DELTA }`                 | policy value   | select DELTA/CUMULATIVE/LOWMEMORY without a subclass          |

## [04]-[IMPLEMENTATION_LAW]

[SDK_BRIDGE_TOPOLOGY]:
- SDK-bridge lane, not native: this exporter is the `NodeSdk`/`WebSdk` (`[R3]`) metric leg; the native `Otlp`/`OtlpMetrics` lane is the `[R3]`-preferred default that serializes metrics over a `@effect/platform` `HttpClient` with no `@opentelemetry/sdk-*`. Reach for this exporter only when the SDK reader semantics, a temporality preference, or a co-resident SDK-only exporter are required.
- temporality is a policy function, never a fork: DELTA vs CUMULATIVE vs LOWMEMORY is a `temporalityPreference` value or an `AggregationTemporalitySelector` `(InstrumentType) => AggregationTemporality` — a backend that wants delta counters is a policy row at the composition root, never a second exporter type.

[INTEGRATION_LAW]:
- Stack with `.api/opentelemetry-sdk-metrics.md` (the wrapping seam): `new OTLPMetricExporter(cfg)` is a `PushMetricExporter` wrapped in `new PeriodicExportingMetricReader({ exporter, exportIntervalMillis })`; the reader owns the collect-and-push interval and calls the exporter's `selectAggregationTemporality`/`selectAggregation` per `InstrumentType`.
- Stack with `.api/effect-opentelemetry.md` `NodeSdk`/`WebSdk` (the facade seam): the wrapped reader is handed to `NodeSdk.Configuration.metricReader` (node/bun) or `WebSdk.Configuration.metricReader` (browser), alongside the one `AppIdentity`-derived `Resource`. Effect's built-in `Metric` values feed the SDK meter that this reader drains.
- Stack with the `data` fact journal (the durable-fact seam): the (app, tenant)-keyed request/compute/storage/token counters emit as Effect metrics; a DELTA `temporalityPreference` keeps the OTLP metric stream cheap for the billing roll-up while CUMULATIVE suits monotonic totals — the temporality selector is where the meter's cost model meets the wire.
- Stack with `.api/opentelemetry-core.md`: `export()` reports through core's `ExportResult`/`ExportResultCode`; the outbound HTTP `Context` is `suppressTracing`-fenced; `timeoutMillis`/`url` default from `OTEL_EXPORTER_OTLP_METRICS_*` via core's typed env readers.
- Stack with `.api/effect-platform.md` posture (the divergence to record): like the trace exporter, this carries its own HTTP transport and does NOT inherit the `net/client` `HttpClient` retry/proxy policy — a concrete reason `otel/emit` prefers the native `OtlpMetrics` lane and marks this row `[R3]`.

[LOCAL_ADMISSION]:
- `@opentelemetry/*` is admitted ONLY inside `scope:runtime` (edge-ledger ban); the exporter is constructed at the composition root only. Instrumentation code uses Effect's native `Metric` and never imports this package.
- prefer the native `Otlp`/`OtlpMetrics` lane; reach for this SDK exporter only for SDK-only reader/exporter capability or an explicit temporality preference, and record it as an `[R3]` non-collapsed dependency.
- the browser exporter is the RUM metric egress leg (`otel/vital`); apply the export-boundary redaction policy rows before the metric leaves the browser.

[RAIL_LAW]:
- Package: `@opentelemetry/exporter-metrics-otlp-http`
- Owns: OTLP/HTTP metric serialization — one `OTLPMetricExporter` (`PushMetricExporter`) over a node or browser transport, plus the aggregation-temporality/aggregation selection algebra (`AggregationTemporalityPreference` + the three `AggregationTemporalitySelector` seed rows)
- Accept: `new OTLPMetricExporter(cfg)` wrapped in a `PeriodicExportingMetricReader` and fed to `NodeSdk`/`WebSdk` `Configuration.metricReader`; temporality as a `temporalityPreference` policy value or an `AggregationTemporalitySelector` function; endpoint/runtime as config + platform-export selection; the one `AppIdentity`-derived `Resource`; core's `ExportResult` rail
- Reject: `@opentelemetry/*` imports outside `scope:runtime`, this SDK exporter where the native `OtlpMetrics` suffices, a subclass per temporality/backend (that is a selector function or policy value), treating the node/browser platform export as a fork, an unwrapped exporter handed straight to the facade (it needs a `MetricReader`)
