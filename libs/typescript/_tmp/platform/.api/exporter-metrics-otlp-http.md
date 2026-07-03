# [API_CATALOGUE] @opentelemetry/exporter-metrics-otlp-http

`@opentelemetry/exporter-metrics-otlp-http` supplies `OTLPMetricExporter`, the concrete browser `PushMetricExporter` that serializes `ResourceMetrics` to OTLP/JSON over Fetch/Beacon, PLUS the temporality/aggregation control surface metrics adds over the shared base: the `AggregationTemporalityPreference` enum, the three `AggregationTemporalitySelector` constants (`Cumulative`/`Delta`/`LowMemory`), `OTLPMetricExporterOptions`, and `OTLPMetricExporterBase` with its `selectAggregation`/`selectAggregationTemporality` reader-contract methods. Like the trace exporter it is a thin subclass over the shared `@opentelemetry/otlp-exporter-base` owner (lifecycle/retry/transport/error). In the `platform` telemetry stack it is the `PushMetricExporter` wrapped by a `@opentelemetry/sdk-metrics` `PeriodicExportingMetricReader` inside the `@effect/opentelemetry` `WebSdk.Configuration.metricReader`; it rides the SDK-BRIDGE lane fenced to `scope:telemetry` and marked `[R3]`-collapse once the native `OtlpMetrics` lane reaches parity.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/exporter-metrics-otlp-http`
- package: `@opentelemetry/exporter-metrics-otlp-http`
- version: `0.219.0` (central pin `pnpm-workspace.yaml`; experimental line, tracks `@opentelemetry/otlp-exporter-base@0.219.0`)
- license: `Apache-2.0`
- api-peer: `@opentelemetry/api ^1.3.0` (declared peer; resolves `1.9.1`) — consumes `@opentelemetry/core` `ExportResult`/`ExportResultCode` (the `export(...)` callback shape, `opentelemetry-core.md`) + `@opentelemetry/sdk-metrics` `PushMetricExporter`/`ResourceMetrics`/`AggregationSelector`/`AggregationTemporality`/`InstrumentType`/`AggregationOption` (contract); deps `@opentelemetry/otlp-exporter-base@0.219.0` (lifecycle + transport + config), `@opentelemetry/otlp-transformer@0.219.0` (metric -> OTLP frame), `@opentelemetry/sdk-metrics@2.8.0`, `@opentelemetry/core@2.8.0`, `@opentelemetry/resources@2.8.0`
- module: `@opentelemetry/exporter-metrics-otlp-http` (single barrel; no subpaths)
- runtime: `browser` — browser build selected by the legacy package.json `browser` field remap (`platform/index.js` -> `platform/browser/index.js`), NOT a conditional `exports` map (`exports` is `null`); Vite/Rolldown honor the `browser` field
- asset: runtime library — side-effects-free (`sideEffects: false`), tree-shakeable
- rail: metrics
- collapse-fence: SDK-bridge exporter lane, fenced to `scope:telemetry`, retired at `[R3]` when native `@effect/opentelemetry` `Otlp`/`OtlpMetrics.layer` reaches parity (`libs/typescript/.api/effect-opentelemetry.md`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: exporter and temporality control family (own exports)
- rail: metrics

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY] | [CONSUMER / BOUNDARY]                                        |
| :-----: | :--------------------------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `OTLPMetricExporter`               | class         | browser `PushMetricExporter` over OTLP/HTTP                 |
|  [02]   | `OTLPMetricExporterBase`           | class         | temporality/aggregation-aware push base; owns `selectAggregation*` |
|  [03]   | `OTLPMetricExporterOptions`        | interface     | `extends OTLPExporterConfigBase` + `temporalityPreference`, `aggregationPreference` |
|  [04]   | `AggregationTemporalityPreference` | enum          | `DELTA = 0`, `CUMULATIVE = 1`, `LOWMEMORY = 2`             |

[PUBLIC_TYPE_SCOPE]: temporality selector constants
- rail: metrics
- Each is an `AggregationTemporalitySelector` — a total function `(InstrumentType) => AggregationTemporality` (seed data, not the mechanism); `temporalityPreference` picks one and the reader calls it per instrument.

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]                     | [PER-INSTRUMENT MAP]                                |
| :-----: | :------------------------------ | :-------------------------------- | :-------------------------------------------------- |
|  [01]   | `CumulativeTemporalitySelector` | `AggregationTemporalitySelector`  | always `CUMULATIVE` (Prometheus-style collectors)   |
|  [02]   | `DeltaTemporalitySelector`      | `AggregationTemporalitySelector`  | `DELTA` for counter/histogram, cumulative otherwise |
|  [03]   | `LowMemoryTemporalitySelector`  | `AggregationTemporalitySelector`  | delta sums, cumulative histograms                   |

[PUBLIC_TYPE_SCOPE]: shared `otlp-exporter-base` owner (lifecycle / config / retry / error)
- rail: metrics
- ownership split: `ExportResponse` (`success`\|`failure`\|`retryable`) is the base-owned TRANSPORT response the retry loop reads; the `export(...)` CALLBACK result — `ExportResult`/`ExportResultCode` — is `@opentelemetry/core`-owned (`opentelemetry-core.md`, imported by `sdk-metrics` `PushMetricExporter.export`), cited not redeclared here.
- `@opentelemetry/otlp-exporter-base@0.219.0`, Apache-2.0 — the SAME base `@opentelemetry/exporter-trace-otlp-http` wraps (one base, two signal subclasses). Full surface catalogued at `libs/typescript/_tmp/platform/.api/exporter-trace-otlp-http.md`; the load-bearing rows for metrics:

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CONSUMER / BOUNDARY]                                                        |
| :-----: | :------------------------------ | :------------ | :-------------------------------------------------------------------------- |
|  [01]   | `OTLPExporterBase<Internal>`    | class         | shared `export`/`forceFlush`/`shutdown` base (`Internal = ResourceMetrics`) |
|  [02]   | `OTLPExporterConfigBase`        | interface     | browser config: `url`, `headers`, `concurrencyLimit`, `timeoutMillis`       |
|  [03]   | `OTLPExporterError`             | class         | typed export failure (`code?`/`data?`)                                      |
|  [04]   | `ExportResponse`                | union         | `success` \| `failure` \| `retryable` — the retry-loop response             |
|  [05]   | `IOtlpExportDelegate<Internal>` | interface     | transport delegate the subclass injects into the base                       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: OTLPMetricExporter construction and reader contract
- rail: metrics
- `new OTLPMetricExporter(config?)` is the only user-constructed surface; `export`/`forceFlush`/`shutdown` inherit from `OTLPExporterBase`, and `selectAggregation`/`selectAggregationTemporality` are the `PushMetricExporter`-contract methods the `PeriodicExportingMetricReader` calls per instrument — never called directly.

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY]   | [CONSUMER / BOUNDARY]                         |
| :-----: | :----------------------------------------------------------- | :--------------- | :------------------------------------------- |
|  [01]   | `new OTLPMetricExporter(config?: OTLPExporterConfigBase & OTLPMetricExporterOptions)` | exporter factory | browser metric exporter; the reader wraps it |
|  [02]   | `exporter.export(metrics: ResourceMetrics, (result: ExportResult) => void): void` | metric export | callback-style; `ExportResult` is `@opentelemetry/core`-owned (`opentelemetry-core.md`); serialize `ResourceMetrics` and POST |
|  [03]   | `exporter.selectAggregation(type: InstrumentType): AggregationOption` | aggregation | per-instrument `AggregationOption` from `aggregationPreference` |
|  [04]   | `exporter.selectAggregationTemporality(type: InstrumentType): AggregationTemporality` | aggregation | `DELTA`/`CUMULATIVE` per instrument from `temporalityPreference` |
|  [05]   | `exporter.forceFlush(): Promise<void>`                       | lifecycle        | resolve when pending exports drain           |
|  [06]   | `exporter.shutdown(): Promise<void>`                         | lifecycle        | stop exporter, release the delegate          |

[ENTRYPOINT_SCOPE]: `OTLPMetricExporterOptions` fields (extends `OTLPExporterConfigBase`)
- rail: metrics

| [INDEX] | [FIELD]                 | [TYPE]                                                       | [DEFAULT]                           |
| :-----: | :---------------------- | :----------------------------------------------------------- | :---------------------------------- |
|  [01]   | `temporalityPreference` | `AggregationTemporalityPreference \| AggregationTemporality` | `CUMULATIVE`                        |
|  [02]   | `aggregationPreference` | `AggregationSelector`                                        | default aggregation per instrument  |
|  [03]   | `url`                   | `string`                                                     | `—` (delegate appends `v1/metrics`) |
|  [04]   | `headers`               | `Record<string, string> \| HeadersFactory`                   | `—`                                 |
|  [05]   | `timeoutMillis`         | `number`                                                     | `10000`                             |

## [04]-[IMPLEMENTATION_LAW]

[METRIC_EXPORT_TOPOLOGY]:
- `OTLPMetricExporter extends OTLPMetricExporterBase extends OTLPExporterBase<ResourceMetrics> implements PushMetricExporter`; the browser subclass is only a constructor that builds the browser network delegate over Fetch/Beacon and passes the config, while `OTLPMetricExporterBase` adds the `selectAggregation`/`selectAggregationTemporality` reader-contract methods over the config's `aggregationPreference`/`temporalityPreference`.
- browser vs node is a BUILD-TIME `browser`-field remap, not a runtime branch and not an `exports` condition (identical to the trace exporter); admit the browser intersection `OTLPExporterConfigBase & OTLPMetricExporterOptions` only — the Node config base and its `keepAlive`/`compression`/`httpAgentOptions` fields are unreachable in browser.
- `temporalityPreference` selects the per-instrument `AggregationTemporality`; the three exported `AggregationTemporalitySelector` constants realize the spec temporality maps and are the seed data `selectAggregationTemporality` returns — a hand-rolled per-instrument temporality switch is the deleted form.
- retry/backoff and `export`/`forceFlush`/`shutdown` are OWNED BY THE BASE (`ExportResponseRetryable` + `retrying-transport`); the exporter never re-implements them, and the `PeriodicExportingMetricReader` owns collection cadence — the `export(...)` callback reports the `@opentelemetry/core` `ExportResult`/`ExportResultCode` (`opentelemetry-core.md`), distinct from the base's `ExportResponse` transport union, never a redeclared shape.

[INTEGRATION_LAW]:
- stack under `@opentelemetry/sdk-metrics` `PeriodicExportingMetricReader`: `new PeriodicExportingMetricReader({ exporter: new OTLPMetricExporter({ url: \`${endpoint}/v1/metrics\` }) })` — the reader drives `collect` on `exportIntervalMillis` (default 60s) and calls the exporter's `select*` per instrument; the exporter owns only serialize+POST, never the collection loop.
- stack under `@effect/opentelemetry` `WebSdk`: the reader is the `WebSdk.Configuration.metricReader` row (`libs/typescript/.api/effect-opentelemetry.md` SDK-bridge lane); the exporter is constructed inline in the `Configuration` thunk (`Observability/telemetry#METRIC_REGISTRY`), never surfaced by `WebSdk.layer`.
- one resource, one endpoint, delta-native switch: the endpoint reads from `RuntimeConfig.collectorOtlpEndpoint`, the resource is the `AppIdentity`-derived `@effect/opentelemetry` `Resource` shared with the trace exporter; set `temporalityPreference: AggregationTemporalityPreference.DELTA` (+ `DeltaTemporalitySelector`) for delta-native backends, default `CUMULATIVE` for Prometheus-style collectors — one config row, never a parallel exporter.
- Effect metrics feed the reader: the `MetricRegistry` derives its `counters`/`histograms`/`gauges` from Effect's `Metric`; the `@effect/opentelemetry` `Metrics.registerProducer` bridge (or the WebSdk provider) routes those into the SDK `MetricReader` this exporter drains — a parallel `MeterProvider` outside the WebSdk is the reject.

[LOCAL_ADMISSION]:
- construct `OTLPMetricExporter` ONLY at the composition root inside `WebSdk.Configuration.metricReader`; instrumentation records into the named `MetricRegistry` gauge/counter/histogram rows, never by importing this package.
- drive collection cadence from `PeriodicExportingMetricReaderOptions.exportIntervalMillis`, never by calling `export` directly.
- record this dependency as an `[R3]` non-collapsed SDK-bridge exporter: it exists only until the native `@effect/opentelemetry` `OtlpMetrics.layer` (over `HttpClient`) reaches browser parity, collapsing the `@opentelemetry/exporter-*` + `sdk-metrics` block while the propagation (`@opentelemetry/core`) / resource-identity (`@opentelemetry/resources`) / convention (`@opentelemetry/semantic-conventions`) vocabulary survives.

[RAIL_LAW]:
- Package: `@opentelemetry/exporter-metrics-otlp-http`
- Owns: the concrete browser OTLP/HTTP `PushMetricExporter` (`OTLPMetricExporter`), the temporality/aggregation control surface, over the shared `@opentelemetry/otlp-exporter-base` lifecycle/retry/transport owner
- Accept: `OTLPMetricExporter` as the `PushMetricExporter` under `PeriodicExportingMetricReader` inside `WebSdk.Configuration.metricReader`, endpoint from `RuntimeConfig`, resource shared with the trace exporter, `temporalityPreference` as one config row, `[R3]`-tagged
- Reject: hand-rolled metric POST clients; the Node exporter variant in browser code; direct `export` invocation outside the periodic reader; a hand-rolled per-instrument temporality/aggregation switch (the selector constants own it); a parallel `MeterProvider` beside the WebSdk
