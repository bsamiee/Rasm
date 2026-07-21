# [TS_RUNTIME_API_OPENTELEMETRY_EXPORTER_METRICS_OTLP_PROTO]

`@opentelemetry/exporter-metrics-otlp-proto` is the concrete `PushMetricExporter` that POSTs `ResourceMetrics` to an OTLP/HTTP collector as protobuf — the binary-encoded sibling of `.api/opentelemetry-exporter-metrics-otlp-http.md`, which serializes the same wire as JSON. Its `OTLPMetricExporter` extends the http package's `OTLPMetricExporterBase` and binds `ProtobufMetricsSerializer` through `createOtlpHttpExportDelegate` on node or `createLegacyOtlpBrowserExportDelegate` on browser. A `PeriodicExportingMetricReader` wraps it and feeds the facade's `NodeSdk`/`WebSdk` `Configuration.metricReader`. Inside Rasm it closes the `[OTLP_SDK]` wire law's metric leg beside `.api/opentelemetry-exporter-trace-otlp-proto.md`; the native `Otlp`/`OtlpMetrics` lane (`@effect/opentelemetry`) stays the `[OTEL_PIN_BLOCK]`-preferred default over a `@effect/platform` `HttpClient` with no SDK. Edge ledger fences `@opentelemetry/*` to `scope:runtime`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/exporter-metrics-otlp-proto`
- package: `@opentelemetry/exporter-metrics-otlp-proto`
- license: `Apache-2.0`
- otel-peer: `@opentelemetry/api ^catalog` (peer), `@opentelemetry/core ^catalog` (the `ExportResult` rail), `@opentelemetry/sdk-metrics ^catalog` (the `PushMetricExporter`/`ResourceMetrics` contract + the `PeriodicExportingMetricReader` that wraps it), `@opentelemetry/resources ^catalog`
- transitive-config: `@opentelemetry/exporter-metrics-otlp-http` supplies the inherited `OTLPMetricExporterBase` and `OTLPMetricExporterOptions`; `@opentelemetry/otlp-exporter-base` supplies the node-http and browser-fetch delegates; `@opentelemetry/otlp-transformer` supplies `ProtobufMetricsSerializer`
- consumed-by: `otel/emit` SDK-bridge metric leg via the facade's `NodeSdk`/`WebSdk` `Configuration.metricReader`, on the protobuf-wire selection; the wire under the `data` fact journal's (app, tenant)-keyed usage/cost counters when the collector demands protobuf
- catalog-verdict: KEEP as SDK-bridge protobuf peer completing the JSON/protobuf metric pair and the proto exporter family beside the trace-proto row; edge-ledger fences `@opentelemetry/*` to `scope:runtime`; `[OTEL_PIN_BLOCK]`-collapse member (native `OtlpMetrics` supersedes)
- runtime: dual — the package `browser` field selects node HTTP through `createOtlpHttpExportDelegate` or browser fetch through `createLegacyOtlpBrowserExportDelegate`; both bind `ProtobufMetricsSerializer`
- modules: `OTLPMetricExporter`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the one protobuf `PushMetricExporter`
- rail: observability/export/metric
- This package exports a SINGLE symbol: `OTLPMetricExporter extends OTLPMetricExporterBase`. Base class, its `export`/`forceFlush`/`shutdown` lifecycle, and the `selectAggregationTemporality`/`selectAggregation` per-`InstrumentType` selectors live in `.api/opentelemetry-exporter-metrics-otlp-http.md` — this row REUSES them, so the temporality algebra table is NOT restated here (an overlay never copies its sibling's surface). Only member this package adds is the constructor that binds the protobuf serializer.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]   | [CONSUMER_BOUNDARY]                                             |
| :-----: | :-------------------------------- | :-------------- | :-------------------------------------------------------------- |
|  [01]   | `OTLPMetricExporter`              | metric exporter | protobuf exporter a `PeriodicExportingMetricReader` wraps       |
|  [02]   | `new OTLPMetricExporter(config?)` | constructor     | binds `ProtobufMetricsSerializer` into `OTLPMetricExporterBase` |
|  [03]   | `OTLPMetricExporterOptions`       | config type     | temporality fence intersected onto `OTLPExporterNodeConfigBase` |

```ts signature
class OTLPMetricExporter extends OTLPMetricExporterBase {   // OTLPMetricExporterBase from @opentelemetry/exporter-metrics-otlp-http
  constructor(config?: OTLPExporterNodeConfigBase & OTLPMetricExporterOptions)
  // super(createOtlpHttpExportDelegate(convertLegacyHttpOptions(config, 'METRICS', 'v1/metrics',
  //   { 'Content-Type': 'application/x-protobuf' }), ProtobufMetricsSerializer), config)
}
```

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: SDK-bridge protobuf metric export composition
- rail: observability/export/metric
- Exporter is never a leaf: construct it with a `temporalityPreference`, wrap it in a `PeriodicExportingMetricReader({ exporter, exportIntervalMillis })`, hand the reader to the facade's `NodeSdk`/`WebSdk` `Configuration.metricReader`. `url`/`headers`/`compression`/`timeoutMillis` come from the base config or `OTEL_EXPORTER_OTLP_METRICS_*` env (via core's readers); the export interval is a reader policy value; the protobuf wire is fixed by the package, never a config toggle.

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                |
| :-----: | :-------------------------------------------------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `new OTLPMetricExporter(config?)`                               | ctor           | node/browser OTLP/HTTP protobuf metric exporter    |
|  [02]   | `new PeriodicExportingMetricReader({ exporter })`               | composition    | exporter → reader → facade `metricReader`          |
|  [03]   | `temporalityPreference: AggregationTemporalityPreference.DELTA` | policy value   | inherited temporality selection without a subclass |

## [04]-[IMPLEMENTATION_LAW]

[SDK_BRIDGE_TOPOLOGY]:
- SDK-bridge lane, not native: this exporter is the `NodeSdk`/`WebSdk` (`[OTEL_PIN_BLOCK]`) protobuf metric leg; the native `Otlp`/`OtlpMetrics` lane is the `[OTEL_PIN_BLOCK]`-preferred default that serializes metrics over a `@effect/platform` `HttpClient` with no `@opentelemetry/sdk-*`. Reach for this exporter only when the collector demands protobuf on the wire while the SDK reader semantics, a temporality preference, or a co-resident SDK-only exporter are also required.
- wire-encoding is a serializer binding, never a fork: OTLP/HTTP JSON vs protobuf is the choice between the `.api/opentelemetry-exporter-metrics-otlp-http.md` row and this row — two package selections binding the JSON vs `ProtobufMetricsSerializer` from `@opentelemetry/otlp-transformer`, one `OTLPMetricExporter` shape sharing one `OTLPMetricExporterBase`. Node vs browser transport resolves to the package `browser` remap; endpoint/headers/temporality resolve to config values. A backend or encoding change is a composition-root selection, never a second exporter type in design code.

[INTEGRATION_LAW]:
- Stack with `.api/opentelemetry-exporter-metrics-otlp-http.md` (the JSON sibling + surface owner): identical `OTLPMetricExporter`/reader/lifecycle surface and identical `OTLPMetricExporterBase` temporality algebra — this row is the protobuf half of the pair and inherits, never re-declares, that algebra. Choose ONE per metric-export lane; the collector's accepted encoding decides. Estate's OTLP/HTTP+protobuf sole-egress mandate selects THIS row for the production metric lane.
- Stack with `.api/opentelemetry-exporter-trace-otlp-proto.md` (the trace-proto family peer): the two proto exporters complete the protobuf exporter set — same `ProtobufTraceSerializer`/`ProtobufMetricsSerializer` binding pattern from `@opentelemetry/otlp-transformer`, wrapped by their respective processor/reader and fed to the one facade.
- Stack with `.api/opentelemetry-sdk-metrics.md` (the wrapping seam): `new OTLPMetricExporter(cfg)` is a `PushMetricExporter` wrapped in `new PeriodicExportingMetricReader({ exporter, exportIntervalMillis })`; the reader owns the collect-and-push interval and calls the inherited `selectAggregationTemporality`/`selectAggregation` per `InstrumentType`.
- Stack with `.api/effect-opentelemetry.md` `NodeSdk`/`WebSdk` (the facade seam): the wrapped reader is handed to `NodeSdk.Configuration.metricReader` (node/bun) or `WebSdk.Configuration.metricReader` (browser), alongside the one `AppIdentity`-derived `Resource`. Effect's built-in `Metric` values feed the SDK meter this reader drains; this package owns protobuf wire serialization.
- Stack with `.api/opentelemetry-core.md`: `export()` reports terminal disposition through core's `ExportResult`/`ExportResultCode`; the outbound HTTP `Context` is `suppressTracing`-fenced so OTLP egress is never self-traced; `timeoutMillis`/`url`/`compression` default from `OTEL_EXPORTER_OTLP_METRICS_*` env via core's typed readers.
- Stack with `.api/effect-platform.md` posture: node HTTP uses `createOtlpHttpExportDelegate`, and browser fetch uses `createLegacyOtlpBrowserExportDelegate`; neither rides the `net/client` `HttpClient` policy inherited by native `OtlpMetrics`.

[LOCAL_ADMISSION]:
- `@opentelemetry/*` is admitted ONLY inside `scope:runtime` (edge-ledger ban); the exporter is constructed at the composition root only. Instrumentation code uses Effect's native `Metric` and never imports this package.
- prefer the native `Otlp`/`OtlpMetrics` lane; reach for this SDK exporter only for protobuf-wire SDK-only reader/exporter capability or an explicit temporality preference, and record it as an `[OTEL_PIN_BLOCK]` non-collapsed dependency.
- Browser exporter is the RUM metric egress leg (`otel/vital`) on collectors demanding protobuf; apply the export-boundary redaction policy rows before the metric leaves the browser.

[RAIL_LAW]:
- Package: `@opentelemetry/exporter-metrics-otlp-proto`
- Owns: OTLP/HTTP protobuf metric serialization — one `OTLPMetricExporter` (`PushMetricExporter`) that binds the `ProtobufMetricsSerializer` into the http package's `OTLPMetricExporterBase` over a node or browser transport, configured by endpoint/headers/compression/timeout and the inherited temporality/aggregation selectors
- Accept: `new OTLPMetricExporter(cfg)` wrapped in a `PeriodicExportingMetricReader` and fed to `NodeSdk`/`WebSdk` `Configuration.metricReader`; temporality as a `temporalityPreference` policy value or an `AggregationTemporalitySelector` inherited from the http row; endpoint/runtime as config + platform-remap selection; the one `AppIdentity`-derived `Resource`; core's `ExportResult` rail
- Reject: `@opentelemetry/*` imports outside `scope:runtime`, this SDK exporter where the native `OtlpMetrics` suffices, both the JSON and protobuf exporter rows on one metric-export lane, re-declaring the http package's temporality algebra here (inherit it), a subclass per backend/temporality (that is a selector function or policy value), treating the node/browser platform remap as a fork, an unwrapped exporter handed straight to the facade (it needs a `MetricReader`)
