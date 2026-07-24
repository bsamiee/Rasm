# [TS_RUNTIME_API_OPENTELEMETRY_EXPORTER_LOGS_OTLP_HTTP]

`@opentelemetry/exporter-logs-otlp-http` is the concrete `LogRecordExporter` POSTing `ReadableLogRecord` batches to an OTLP/HTTP collector — the SDK-bridge log leg of `otel/emit`. A `BatchLogRecordProcessor` wraps it and rides the facade's `Configuration.logRecordProcessor`, never composing as a leaf. Reach for it only where SDK log-pipeline semantics are required, recorded as an `[OTEL_PIN_BLOCK]` dependency; `.api/effect-opentelemetry.md` owns the native-versus-SDK lane doctrine.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/exporter-logs-otlp-http`
- package: `@opentelemetry/exporter-logs-otlp-http` (Apache-2.0)
- module: ESM; single `OTLPLogExporter` export, platform-selected at build, never a fork
- runtime: isomorphic — the `browser` field swaps the platform module: node `http`/`https` transport (`OTLPExporterNodeConfigBase`) or browser `XMLHttpRequest`/`sendBeacon` (`OTLPExporterConfigBase`)
- depends: `@opentelemetry/sdk-logs` (`ReadableLogRecord`/`LogRecordExporter`), `@opentelemetry/otlp-exporter-base` (`OTLPExporterNodeConfigBase`/`OTLPExporterConfigBase`/`CompressionAlgorithm`), `@opentelemetry/core` (`ExportResult`)
- rail: observability/export/logs

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `LogRecordExporter` and its config

`OTLPLogExporter` extends `OTLPExporterBase<ReadableLogRecord[]>` implementing `LogRecordExporter`; endpoint, headers, compression, and timeout are config on the one class, the node and browser config types differing only by node-transport fields.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :--------------------------- | :------------ | :-------------------------------------------------------------- |
|  [01]   | `OTLPLogExporter`            | class         | the `LogRecordExporter` a `BatchLogRecordProcessor` wraps       |
|  [02]   | `OTLPExporterConfigBase`     | interface     | browser + base config: endpoint, headers, concurrency, deadline |
|  [03]   | `OTLPExporterNodeConfigBase` | interface     | node transport tuning atop base config                          |
|  [04]   | `CompressionAlgorithm`       | enum          | request-body compression selector                               |

[OTLPLOG_EXPORTER]: `export(items, cb) -> void` `forceFlush() -> Promise<void>` `shutdown() -> Promise<void>`
[OTLPEXPORTER_CONFIG_BASE]: `url` `headers` `concurrencyLimit` `timeoutMillis`
[OTLPEXPORTER_NODE_CONFIG_BASE]: `keepAlive` `compression` `httpAgentOptions` `userAgent`
[COMPRESSION_ALGORITHM]: `NONE` `GZIP`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: SDK-bridge log export composition

`url`/`headers`/`compression`/`timeoutMillis` source from config or the `OTEL_EXPORTER_OTLP_LOGS_*` env family, each falling back to the signal-neutral `OTEL_EXPORTER_OTLP_*` variant; the default endpoint is `http://localhost:4318/v1/logs`.

| [INDEX] | [SURFACE]                                   | [SHAPE]     | [CAPABILITY]                                                        |
| :-----: | :------------------------------------------ | :---------- | :------------------------------------------------------------------ |
|  [01]   | `new OTLPLogExporter(nodeCfg)`              | ctor        | the node OTLP/HTTP log exporter                                     |
|  [02]   | `new OTLPLogExporter(browserCfg)`           | ctor        | the browser OTLP/HTTP log exporter (RUM crash/log egress)           |
|  [03]   | `new BatchLogRecordProcessor({ exporter })` | composition | wrap the exporter for the facade `Configuration.logRecordProcessor` |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- SDK-bridge log leg of `otel/emit`, reached only where the SDK log pipeline is required — `LoggerConfigurator` severity gating, the `enabled?` pre-build drop, `BatchLogRecordProcessor` tuning, or a co-resident SDK-only exporter; `.api/effect-opentelemetry.md` owns the native-lane default and the `[OTEL_PIN_BLOCK]` collapse.
- Endpoint, transport, and compression are config, never a fork: node/browser transport resolves at the platform export condition and gzip/none at `compression`, so a collector change is a `url`/`headers` edit at the composition root.

[STACKING]:
- `.api/opentelemetry-sdk-logs.md` (wrapping seam): `new OTLPLogExporter(cfg)` rides `new BatchLogRecordProcessor({ exporter })` as an options field; the processor owns batching and `forceFlush` drain, this exporter only the `LogRecordExporter` serialization.
- `.api/opentelemetry-api-logs.md`: serialized records carry `SeverityNumber`, `LogBody`, `LogAttributes`, `AnyValue`; API -> SDK processor -> this exporter is the pipeline order.
- `.api/effect-opentelemetry.md` `NodeSdk`/`WebSdk` (facade seam): the wrapped processor rides `Configuration.logRecordProcessor` beside the one `AppIdentity`-derived `Resource`; the facade owns the resource mint, this exporter only serializes what each `ReadableLogRecord` carries.
- `.api/opentelemetry-core.md`: `export()` reports through `ExportResult`/`ExportResultCode`, and the outbound HTTP context is `suppressTracing`-fenced so log egress is never self-traced.
- `.api/effect-platform.md` (divergence to record): this exporter carries its own `http`/`XMLHttpRequest` transport, not the `net/client` `HttpClient` retry/proxy policy the native `OtlpLogger` lane inherits — the gap that routes `otel/emit` to the native lane and marks this row `[OTEL_PIN_BLOCK]`.

[LOCAL_ADMISSION]:
- `@opentelemetry/*` admits ONLY inside `scope:runtime`, constructed at the composition root; application code logs through `Effect.log` and never imports this package.
- recorded as an `[OTEL_PIN_BLOCK]` non-collapsed dependency, admitted for SDK-only log-pipeline capability.
- browser exporters carry the RUM log/crash egress leg (`otel/vital`/`otel/crash`); export-boundary redaction applies before a record leaves the browser.

[RAIL_LAW]:
- Package: `@opentelemetry/exporter-logs-otlp-http`
- Owns: OTLP/HTTP log serialization — one `OTLPLogExporter` (`LogRecordExporter`) over a node or browser transport, configured by endpoint/headers/compression/timeout
- Accept: `new OTLPLogExporter(cfg)` wrapped in a `BatchLogRecordProcessor` and fed to `NodeSdk`/`WebSdk` `Configuration.logRecordProcessor`; endpoint/runtime as config + platform-export selection; the one `AppIdentity`-derived `Resource`; core's `ExportResult` rail
- Reject: `@opentelemetry/*` outside `scope:runtime`, this SDK exporter where the native `OtlpLogger` suffices, `SimpleLogRecordProcessor` in production, a subclass per backend/compression, an unwrapped exporter handed straight to the facade, a parallel log sink beside the composition-root one
