# [TS_RUNTIME_API_OPENTELEMETRY_EXPORTER_LOGS_OTLP_HTTP]

`@opentelemetry/exporter-logs-otlp-http` is the concrete `LogRecordExporter` that POSTs batches of `ReadableLogRecord`s to an OTLP/HTTP collector — the log egress completing the OTLP-HTTP exporter family beside the trace and metric legs. It is never a leaf: a `BatchLogRecordProcessor` (from `@opentelemetry/sdk-logs`) wraps it, and that processor is handed to the facade's `NodeSdk`/`WebSdk` `Configuration.logRecordProcessor`. Inside Rasm it is the SDK-bridge log leg of `otel/emit` — the lane used when SDK processor semantics (`LoggerConfigurator` severity gating, the `enabled?` pre-build drop, batch tuning) or a co-resident SDK-only exporter are required; the native `OtlpLogger` lane (`@effect/opentelemetry`) remains the `[OTEL_PIN_BLOCK]`-preferred default that replaces the process `Logger` with no SDK. One `OTLPLogExporter` class serves both runtimes — the node/browser split is the package export map, never a fork. It rides the overlay `0.220` line lock-stepped with `api-logs`/`sdk-logs` while its `core` dependency sits on the stable `2.9` line — the logs signal's named ABI skew. The edge ledger fences `@opentelemetry/*` to `scope:runtime`; this exporter is an `[OTEL_PIN_BLOCK]`-collapse member (retires when native `OtlpLogger` reaches parity).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/exporter-logs-otlp-http`
- package: `@opentelemetry/exporter-logs-otlp-http` (Apache-2.0, OpenTelemetry JS)
- line: overlay `0.220` family — lock-stepped with `@opentelemetry/api-logs`, `sdk-logs`, `otlp-exporter-base`, `otlp-transformer`; `@opentelemetry/core` (the `ExportResult` rail) rides the stable `2.9` line
- transitive-config: `@opentelemetry/otlp-exporter-base` supplies `OTLPExporterNodeConfigBase` / `OTLPExporterConfigBase` and `CompressionAlgorithm`
- consumed-by: `otel/emit` SDK-bridge log leg via `NodeSdk`/`WebSdk` `Configuration.logRecordProcessor`
- runtime: dual — the package `browser` field remaps the platform module: node transport (`http`/`https`, config `OTLPExporterNodeConfigBase`) or browser transport (`XMLHttpRequest`/`sendBeacon`, config `OTLPExporterConfigBase`); ONE `OTLPLogExporter` name, a build-time platform selection, never a fork
- modules: `OTLPLogExporter`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `LogRecordExporter` and its config
- rail: observability/export/logs
- One exporter class, `OTLPExporterBase<ReadableLogRecord[]> implements LogRecordExporter` — endpoint, headers, compression, and timeout are CONFIG values on one class, never a subclass per backend. The two config types differ only by node-only transport fields.

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CONSUMER_BOUNDARY]                                                |
| :-----: | :---------------------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `OTLPLogExporter`             | log exporter  | the concrete exporter a `BatchLogRecordProcessor` wraps            |
|  [02]   | `export(items, cb)`           | export method | called by the processor; reports through core's `ExportResult`     |
|  [03]   | `forceFlush()` / `shutdown()` | lifecycle     | drain-on-exit and terminal release the provider invokes            |
|  [04]   | `OTLPExporterConfigBase`      | base config   | endpoint/headers/concurrency/deadline (fence; browser + base)      |
|  [05]   | `OTLPExporterNodeConfigBase`  | node config   | node transport tuning (fence); `CompressionAlgorithm.NONE`/`.GZIP` |

```ts signature
class OTLPLogExporter extends OTLPExporterBase<ReadableLogRecord[]> implements LogRecordExporter {
  export(items: ReadableLogRecord[], cb: (r: ExportResult) => void): void
  forceFlush(): Promise<void>; shutdown(): Promise<void>
}
interface OTLPExporterConfigBase { url?: string; headers?: Record<string, string>; concurrencyLimit?: number; timeoutMillis?: number }
interface OTLPExporterNodeConfigBase extends OTLPExporterConfigBase {   // node transport tuning
  keepAlive?: boolean; compression?: CompressionAlgorithm; httpAgentOptions?: object; userAgent?: string
}
enum CompressionAlgorithm { NONE = "none", GZIP = "gzip" }
```

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: SDK-bridge log export composition
- rail: observability/export/logs
- Construct the exporter, wrap it in a processor, hand the processor to the facade. `url`/`headers`/`compression`/`timeoutMillis` are policy values sourced from config or the `OTEL_EXPORTER_OTLP_LOGS_*` env family (endpoint, headers, timeout, compression, client cert/key — each falling back to the signal-neutral `OTEL_EXPORTER_OTLP_*` variant); the default endpoint resolves to `http://localhost:4318/v1/logs`.

- the node ctor takes `OTLPExporterNodeConfigBase`, the browser ctor `OTLPExporterConfigBase`; the wrapped processor rides `Configuration.logRecordProcessor`.

| [INDEX] | [SURFACE]                               | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                       |
| :-----: | :-------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `new OTLPLogExporter(nodeCfg)`          | node ctor      | the node OTLP/HTTP log exporter                           |
|  [02]   | `new OTLPLogExporter(browserCfg)`       | browser ctor   | the browser OTLP/HTTP log exporter (RUM crash/log egress) |
|  [03]   | `new BatchLogRecordProcessor(exporter)` | composition    | exporter → processor → `NodeSdk`/`WebSdk`                 |

## [04]-[IMPLEMENTATION_LAW]

[SDK_BRIDGE_TOPOLOGY]:
- SDK-bridge lane, not native: this exporter is the `NodeSdk`/`WebSdk` (`[OTEL_PIN_BLOCK]`) log leg; the native `OtlpLogger` lane replaces the process `Logger` and serializes over the `@effect/platform` `HttpClient` with no `@opentelemetry/sdk-*`. Reach for this exporter only when the SDK log pipeline — `LoggerConfigurator` severity gating, the `enabled?` pre-build drop, `BatchLogRecordProcessor` tuning, a co-resident SDK-only exporter — is required.
- endpoint/runtime are config, never a fork: JSON versus protobuf, node versus browser transport, gzip versus none — all resolve to a config value or the platform export condition. A collector change is a `url`/`headers` value at the composition root.

[INTEGRATION_LAW]:
- Stack with `.api/opentelemetry-sdk-logs.md` (the wrapping seam): `new OTLPLogExporter(cfg)` is wrapped in `new BatchLogRecordProcessor(exporter)` (production, queued) — never `SimpleLogRecordProcessor` in production; the exporter is a `LogRecordExporter`, the processor owns batching and `forceFlush` drain. This admission closes the family gap that catalog recorded: the SDK-bridge log leg now has its OTLP egress beside `ConsoleLogRecordExporter`/`InMemoryLogRecordExporter`.
- Stack with `.api/opentelemetry-api-logs.md`: the records it serializes carry that vocabulary — `SeverityNumber`, `LogBody`/`LogAttributes`/`AnyValue`; API → SDK processor → this exporter is the one pipeline order.
- Stack with `.api/effect-opentelemetry.md` `NodeSdk`/`WebSdk` (the facade seam): the wrapped processor rides `Configuration.logRecordProcessor` (single or array), alongside the one `AppIdentity`-derived `Resource`. Under the `2.9`-line resources API that `Resource` is minted via `resourceFromAttributes(...)` and custom detectors return `DetectedResource` synchronously with per-attribute promises — the facade's `Resource` layer owns the mint; this exporter only serializes the resource each `ReadableLogRecord` carries.
- Stack with `.api/opentelemetry-core.md`: `export()` reports through `ExportResult`/`ExportResultCode`; the exporter's outbound HTTP context is `suppressTracing`-fenced so log egress is never self-traced; endpoint/timeout default from the `OTEL_EXPORTER_OTLP_LOGS_*` readers.
- Stack with `.api/effect-platform.md` posture (the divergence to record): this exporter carries its OWN `http`/`XMLHttpRequest` transport — it does not ride the `net/client` `HttpClient` retry/proxy policy the native `OtlpLogger` lane inherits. That transport-policy gap is a concrete reason `otel/emit` prefers the native lane and marks this row `[OTEL_PIN_BLOCK]`.

[LOCAL_ADMISSION]:
- `@opentelemetry/*` is admitted ONLY inside `scope:runtime`; the exporter is constructed at the composition root. Application code logs through `Effect.log` and never imports this package.
- prefer the native `OtlpLogger` lane; reach for this SDK exporter only for SDK-only log-pipeline capability, and record it as an `[OTEL_PIN_BLOCK]` non-collapsed dependency.
- the browser exporter is the RUM log/crash egress leg (`otel/vital`/`otel/crash`); apply the export-boundary redaction policy rows before a record leaves the browser.

[RAIL_LAW]:
- Package: `@opentelemetry/exporter-logs-otlp-http`
- Owns: OTLP/HTTP log serialization — one `OTLPLogExporter` (`LogRecordExporter`) over a node or browser transport, configured by endpoint/headers/compression/timeout
- Accept: `new OTLPLogExporter(cfg)` wrapped in a `BatchLogRecordProcessor` and fed to `NodeSdk`/`WebSdk` `Configuration.logRecordProcessor`; endpoint/runtime as config + platform-export selection; the one `AppIdentity`-derived `Resource`; core's `ExportResult` rail
- Reject: `@opentelemetry/*` imports outside `scope:runtime`, this SDK exporter where the native `OtlpLogger` suffices, `SimpleLogRecordProcessor` in production, a subclass per backend/compression, an unwrapped exporter handed straight to the facade, a parallel log sink beside the one the composition root rules
