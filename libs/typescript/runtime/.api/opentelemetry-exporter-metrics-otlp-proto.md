# [TS_RUNTIME_API_OPENTELEMETRY_EXPORTER_METRICS_OTLP_PROTO]

`@opentelemetry/exporter-metrics-otlp-proto` binds `ProtobufMetricsSerializer` into the http package's `OTLPMetricExporterBase`, POSTing `ResourceMetrics` to an OTLP/HTTP collector as protobuf — the binary sibling of `.api/opentelemetry-exporter-metrics-otlp-http.md`, which serializes the same wire as JSON. One `OTLPMetricExporter` shape over one shared base; a `PeriodicExportingMetricReader` wraps it and feeds the `NodeSdk`/`WebSdk` `Configuration.metricReader`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/exporter-metrics-otlp-proto`
- package: `@opentelemetry/exporter-metrics-otlp-proto` (Apache-2.0)
- rail: observability/export/metric
- otel-peer: `@opentelemetry/api`, `@opentelemetry/core` (`ExportResult` rail), `@opentelemetry/sdk-metrics` (`PushMetricExporter`/`ResourceMetrics` + the `PeriodicExportingMetricReader` that wraps it), `@opentelemetry/resources`
- transitive-config: `@opentelemetry/exporter-metrics-otlp-http` supplies `OTLPMetricExporterBase`/`OTLPMetricExporterOptions`; `@opentelemetry/otlp-transformer` supplies `ProtobufMetricsSerializer`; `@opentelemetry/otlp-exporter-base` supplies the node/browser export delegates
- consumed-by: `otel/emit` metric leg via `NodeSdk`/`WebSdk` `Configuration.metricReader` on the protobuf-wire selection; the wire under the `data` fact journal's (app, tenant)-keyed usage/cost counters when the collector demands protobuf
- catalog-verdict: KEEP — SDK-bridge protobuf peer, `[OTEL_PIN_BLOCK]`-collapse member
- runtime: dual — the package `browser` field selects node HTTP through `createOtlpHttpExportDelegate` or browser fetch through `createLegacyOtlpBrowserExportDelegate`, both binding `ProtobufMetricsSerializer`
- modules: `OTLPMetricExporter`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the one protobuf `PushMetricExporter`
- `.api/opentelemetry-exporter-metrics-otlp-http.md` owns the base lifecycle (`export`/`forceFlush`/`shutdown`) and the `selectAggregationTemporality`/`selectAggregation` temporality algebra; `OTLPMetricExporter extends OTLPMetricExporterBase` inherits it and adds only the constructor binding the protobuf serializer.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]   | [CAPABILITY]                                                    |
| :-----: | :-------------------------------- | :-------------- | :-------------------------------------------------------------- |
|  [01]   | `OTLPMetricExporter`              | metric exporter | protobuf exporter a `PeriodicExportingMetricReader` wraps       |
|  [02]   | `new OTLPMetricExporter(config?)` | constructor     | binds `ProtobufMetricsSerializer` into `OTLPMetricExporterBase` |
|  [03]   | `OTLPMetricExporterOptions`       | config type     | temporality fence intersected onto `OTLPExporterNodeConfigBase` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: SDK-bridge protobuf metric export composition
- Construct with a `temporalityPreference`, wrap in `PeriodicExportingMetricReader({ exporter, exportIntervalMillis })`, hand the reader to `NodeSdk`/`WebSdk` `Configuration.metricReader`. `url`/`headers`/`compression`/`timeoutMillis` source from base config or `OTEL_EXPORTER_OTLP_METRICS_*` env; the export interval is a reader policy value; the protobuf wire is fixed by the package.

| [INDEX] | [SURFACE]                                                       | [SHAPE]     | [CAPABILITY]                                       |
| :-----: | :-------------------------------------------------------------- | :---------- | :------------------------------------------------- |
|  [01]   | `new OTLPMetricExporter(config?)`                               | ctor        | node/browser OTLP/HTTP protobuf metric exporter    |
|  [02]   | `new PeriodicExportingMetricReader({ exporter })`               | composition | exporter → reader → facade `metricReader`          |
|  [03]   | `temporalityPreference: AggregationTemporalityPreference.DELTA` | policy      | inherited temporality selection without a subclass |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Protobuf metric leg of the SDK-bridge lane, binding `ProtobufMetricsSerializer` into `OTLPMetricExporterBase` — node through `createOtlpHttpExportDelegate`, browser through `createLegacyOtlpBrowserExportDelegate`; `.api/effect-opentelemetry.md` `[04]` owns the native-first `[OTEL_PIN_BLOCK]`-collapse doctrine deciding when this SDK lane is reached.
- Wire-encoding is a serializer binding, never a fork: JSON versus protobuf is the `.api/opentelemetry-exporter-metrics-otlp-http.md` row versus this row, one `OTLPMetricExporter` shape sharing one `OTLPMetricExporterBase`. Node versus browser resolves to the package `browser` remap; endpoint/headers/temporality resolve to config values. A backend or encoding change is a composition-root selection, never a second exporter type.

[STACKING]:
- `.api/opentelemetry-exporter-metrics-otlp-http.md` (JSON sibling, surface owner): shared `OTLPMetricExporter`/reader/lifecycle surface, inherited `OTLPMetricExporterBase` temporality algebra; choose ONE per metric-export lane by the collector's accepted encoding, the estate's OTLP/HTTP+protobuf sole-egress mandate selecting this row for production.
- `.api/opentelemetry-exporter-trace-otlp-proto.md` (proto family peer): same `ProtobufTraceSerializer`/`ProtobufMetricsSerializer` binding pattern, each wrapped by its own processor/reader and fed to the one facade.
- `.api/opentelemetry-sdk-metrics.md` (wrapping seam): `new OTLPMetricExporter(cfg)` is a `PushMetricExporter` wrapped in `PeriodicExportingMetricReader({ exporter, exportIntervalMillis })`; the reader owns the collect-and-push interval and calls the inherited `selectAggregationTemporality`/`selectAggregation` per `InstrumentType`.
- `.api/effect-opentelemetry.md` `NodeSdk`/`WebSdk` (facade seam): the wrapped reader hands to `Configuration.metricReader` (node/bun or browser) alongside the one `AppIdentity`-derived `Resource`; Effect's `Metric` values feed the SDK meter this reader drains, this package owning protobuf wire serialization.
- `.api/opentelemetry-core.md`: `export()` reports terminal disposition through core's `ExportResult`/`ExportResultCode`; the outbound HTTP `Context` is `suppressTracing`-fenced; `timeoutMillis`/`url`/`compression` default from `OTEL_EXPORTER_OTLP_METRICS_*` via core's typed readers.
- `.api/effect-platform.md` posture: the export delegates carry their own transport and never inherit the `net/client` `HttpClient` policy the native `OtlpMetrics` lane rides.

[LOCAL_ADMISSION]:
- `@opentelemetry/*` admits ONLY inside `scope:runtime` (edge-ledger ban); the exporter constructs at the composition root, and instrumentation code emits through Effect's native `Metric`.
- Reach for this SDK exporter only for protobuf-wire SDK-only reader/exporter capability or an explicit temporality preference; record it as an `[OTEL_PIN_BLOCK]` non-collapsed dependency.
- Browser exporter is the RUM metric egress leg (`otel/vital`) on protobuf collectors; apply the export-boundary redaction rows before the metric leaves the browser.

[RAIL_LAW]:
- Package: `@opentelemetry/exporter-metrics-otlp-proto`
- Owns: OTLP/HTTP protobuf metric serialization — one `OTLPMetricExporter` (`PushMetricExporter`) binding `ProtobufMetricsSerializer` into the http package's `OTLPMetricExporterBase` over a node or browser transport
- Accept: `new OTLPMetricExporter(cfg)` wrapped in a `PeriodicExportingMetricReader` and fed to `NodeSdk`/`WebSdk` `Configuration.metricReader`; temporality inherited from the http row as a `temporalityPreference` value or `AggregationTemporalitySelector`; endpoint/runtime as config + platform-remap selection; the one `AppIdentity`-derived `Resource`; core's `ExportResult` rail
- Reject: `@opentelemetry/*` imports outside `scope:runtime`, this SDK exporter where the native `OtlpMetrics` suffices, both the JSON and protobuf rows on one metric-export lane, re-declaring the http temporality algebra here, a subclass per backend/temporality, treating the node/browser remap as a fork, an unwrapped exporter handed straight to the facade
