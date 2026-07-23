# [TS_RUNTIME_API_OPENTELEMETRY_EXPORTER_LOGS_OTLP_PROTO]

`@opentelemetry/exporter-logs-otlp-proto` is the `LogRecordExporter` that POSTs `ReadableLogRecord` batches to an OTLP/HTTP collector as protobuf — the binary sibling of `.api/opentelemetry-exporter-logs-otlp-http.md`, sharing one `OTLPLogExporter` class and one base transport; the sole divergence is the `ProtobufLogsSerializer` binding. A `sdk-logs` processor wraps it and rides the facade's `Configuration.logRecordProcessor`. It closes the `[OTLP_SDK]` wire law's log leg, selected when the collector demands protobuf; the native `OtlpLogger` lane is the `[OTEL_PIN_BLOCK]` default with no SDK.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/exporter-logs-otlp-proto`
- package: `@opentelemetry/exporter-logs-otlp-proto`
- license: `Apache-2.0`
- otel-peer: `@opentelemetry/api ^catalog` (peer), `@opentelemetry/core ^catalog` (the `ExportResult` rail via `otlp-exporter-base`), `@opentelemetry/sdk-logs ^catalog` (the `LogRecordExporter`/`ReadableLogRecord` contract + the `BatchLogRecordProcessor` that wraps it)
- transitive-config: `@opentelemetry/otlp-exporter-base` supplies the constructor config types (`OTLPExporterNodeConfigBase` / `OTLPExporterConfigBase`), the `OTLPExporterBase` the class extends, `CompressionAlgorithm`, and the `createOtlpHttpExportDelegate`/`createLegacyOtlpBrowserExportDelegate`/`convertLegacyHttpOptions` delegate factories; `@opentelemetry/otlp-transformer` supplies the `ProtobufLogsSerializer` that makes this row protobuf — both are peers, not roster rows
- consumed-by: `otel/emit` SDK-bridge log leg via the facade's `NodeSdk`/`WebSdk` `Configuration.logRecordProcessor`, on the protobuf-wire selection
- catalog-verdict: KEEP as SDK-bridge protobuf peer completing the JSON/protobuf log pair and the three-signal proto exporter set beside the trace-proto and metrics-proto rows; edge-ledger fences `@opentelemetry/*` to `scope:runtime`; `[OTEL_PIN_BLOCK]`-collapse member (native `OtlpLogger` supersedes)
- runtime: dual — the package `browser` field remaps `platform/index` to select `platform/node` (uses `http`/`https`, config `OTLPExporterNodeConfigBase`, delegate `createOtlpHttpExportDelegate`) or `platform/browser` (uses `fetch`/`sendBeacon`, config `OTLPExporterConfigBase`, delegate `createLegacyOtlpBrowserExportDelegate`); ONE `OTLPLogExporter` name, a build-time platform selection, never a fork
- modules: `OTLPLogExporter`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `LogRecordExporter` and its config
- rail: observability/export/logs
- One exporter class, `OTLPExporterBase<ReadableLogRecord[]> implements LogRecordExporter` — endpoint, headers, compression, and timeout are CONFIG values on one class, never a subclass per backend. Node/browser split is the package `browser` field remap; `CompressionAlgorithm` resolves `"none" | "gzip"`. Protobuf encoding is a fixed serializer binding invisible at the constructor; lifecycle `export`/`forceFlush`/`shutdown` inherits from `OTLPExporterBase`, never re-declared here.

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY]      | [CONSUMER_BOUNDARY]                                             |
| :-----: | :--------------------------------------------- | :----------------- | :-------------------------------------------------------------- |
|  [01]   | `OTLPLogExporter`                              | log exporter class | concrete exporter a `BatchLogRecordProcessor` wraps             |
|  [02]   | `export(items: ReadableLogRecord[], cb): void` | export method      | inherited; `cb` receives core's `ExportResult`                  |
|  [03]   | `forceFlush(): Promise<void>`                  | lifecycle          | drain-on-exit flush the SDK provider invokes                    |
|  [04]   | `shutdown(): Promise<void>`                    | lifecycle          | terminal release the SDK provider invokes                       |
|  [05]   | `OTLPExporterConfigBase`                       | base config        | `url?`, `headers?`, `concurrencyLimit?`, `timeoutMillis?`       |
|  [06]   | `OTLPExporterNodeConfigBase`                   | node config        | `keepAlive?`, `compression?`, `httpAgentOptions?`, `userAgent?` |

```ts signature
class OTLPLogExporter extends OTLPExporterBase<ReadableLogRecord[]> implements LogRecordExporter {
  constructor(config?: OTLPExporterNodeConfigBase)   // node platform arm
  constructor(config?: OTLPExporterConfigBase)        // browser platform arm (browser-field remap)
  // node super:    createOtlpHttpExportDelegate(convertLegacyHttpOptions(config, 'LOGS', 'v1/logs',
  //                  { 'Content-Type': 'application/x-protobuf' }), ProtobufLogsSerializer)
  // browser super: createLegacyOtlpBrowserExportDelegate(config, ProtobufLogsSerializer, 'v1/logs',
  //                  { 'Content-Type': 'application/x-protobuf' })
}
```

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: SDK-bridge protobuf log export composition
- rail: observability/export/logs
- Exporter is never a leaf: construct it, wrap it in a processor, hand the processor to the facade — `new OTLPLogExporter(config?)` selects its platform config type by the package `browser` remap. `BatchLogRecordProcessor` (production, queued) or `SimpleLogRecordProcessor` (dev, synchronous) from `sdk-logs` is the wrapper; `NodeSdk`/`WebSdk` `Configuration.logRecordProcessor` is the sink.
- `url`/`headers`/`compression`/`timeoutMillis` are policy values sourced from config or the `OTEL_EXPORTER_OTLP_LOGS_*` env family (each falling back to the signal-neutral `OTEL_EXPORTER_OTLP_*` variant, via core's readers), never forks; the default endpoint resolves to `http://localhost:4318/v1/logs`; the protobuf wire is fixed by the package, never a config toggle.

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                           |
| :-----: | :------------------------------------------------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `new OTLPLogExporter(config?: OTLPExporterNodeConfigBase)`     | node ctor      | node OTLP/HTTP protobuf log exporter          |
|  [02]   | `new OTLPLogExporter(config?: OTLPExporterConfigBase)`         | browser ctor   | browser OTLP/HTTP protobuf log exporter (RUM) |
|  [03]   | `new BatchLogRecordProcessor({ exporter })` → `logRecordProcessor` | composition | exporter → processor → `NodeSdk`/`WebSdk` |

## [04]-[IMPLEMENTATION_LAW]

[SDK_BRIDGE_TOPOLOGY]:
- SDK-bridge lane, not native: this exporter is the facade's protobuf log leg; the native `OtlpLogger` lane is the `[OTEL_PIN_BLOCK]` default, replacing the process `Logger` over a `@effect/platform` `HttpClient` with no `@opentelemetry/sdk-*`. This exporter enters only when the collector demands protobuf AND SDK log-pipeline semantics (`LoggerConfigurator` gating, the `enabled?` pre-build drop, batch tuning) are required.
- wire-encoding is a serializer binding, never a fork: JSON vs protobuf is the choice between the `.api/opentelemetry-exporter-logs-otlp-http.md` row and this row — one `OTLPLogExporter` shape over one `OTLPExporterBase`, binding JSON vs `ProtobufLogsSerializer` from `@opentelemetry/otlp-transformer`. A backend or encoding change is a composition-root selection, never a second exporter type in design code.

[INTEGRATION_LAW]:
- Stack with `.api/opentelemetry-exporter-logs-otlp-http.md` (the JSON sibling): identical `OTLPLogExporter`/config/lifecycle surface; this row is the protobuf half of the pair. Choose ONE per log-export lane — the collector's accepted encoding decides; both feed the same processor and facade seams unchanged.
- Stack with `.api/opentelemetry-exporter-trace-otlp-proto.md` and `.api/opentelemetry-exporter-metrics-otlp-proto.md` (the proto family peers): the three proto exporters complete the protobuf exporter set — the same `ProtobufTraceSerializer`/`ProtobufMetricsSerializer`/`ProtobufLogsSerializer` binding pattern from `@opentelemetry/otlp-transformer`, each wrapped by its respective processor/reader and fed to the one facade.
- Stack with `.api/opentelemetry-sdk-logs.md` (the wrapping seam): `new OTLPLogExporter(cfg)` is wrapped in `new BatchLogRecordProcessor({ exporter })` (queued, production; the exporter is an options field, not a positional arg) — never `SimpleLogRecordProcessor` in production; the exporter is a `LogRecordExporter`, the processor owns batching and `forceFlush` drain. This closes the SDK-bridge log leg's protobuf egress beside `ConsoleLogRecordExporter`/`InMemoryLogRecordExporter`.
- Stack with `.api/opentelemetry-api-logs.md`: the records it serializes carry that vocabulary — `SeverityNumber`, `LogBody`/`LogAttributes`/`AnyValue`; API → SDK processor → this exporter is the one pipeline order.
- Stack with `.api/effect-opentelemetry.md` `NodeSdk`/`WebSdk` (the facade seam): the wrapped processor is handed to `NodeSdk.Configuration.logRecordProcessor` (node/bun) or `WebSdk.Configuration.logRecordProcessor` (browser RUM), alongside the one `AppIdentity`-derived `Resource`. Facade owns provider lifecycle; this package owns protobuf wire serialization.
- Stack with `.api/opentelemetry-core.md`: `export()` reports terminal disposition through core's `ExportResult`/`ExportResultCode`; the exporter's own outbound HTTP `Context` is `suppressTracing`-fenced so OTLP egress is never self-traced; `timeoutMillis`/`url`/`compression` default from `OTEL_EXPORTER_OTLP_LOGS_*` env via core's typed readers.
- Stack with `.api/effect-platform.md` posture: node HTTP uses `createOtlpHttpExportDelegate`, and browser fetch uses `createLegacyOtlpBrowserExportDelegate`; neither rides the `net/client` `HttpClient` policy inherited by native `OtlpLogger`.

[LOCAL_ADMISSION]:
- `@opentelemetry/*` is admitted ONLY inside `scope:runtime` (edge-ledger ban); the exporter is constructed at the composition root only. Application code logs through `Effect.log` and never imports this package.
- native `OtlpLogger` lane is the default; this SDK exporter enters only for protobuf-wire SDK-only log-pipeline capability, recorded as an `[OTEL_PIN_BLOCK]` non-collapsed dependency.
- Browser exporter is the RUM log/crash egress leg (`otel/vital`/`otel/crash`) on collectors demanding protobuf; apply the export-boundary redaction policy rows before a record leaves the browser.

[RAIL_LAW]:
- Package: `@opentelemetry/exporter-logs-otlp-proto`
- Owns: OTLP/HTTP protobuf log serialization — one `OTLPLogExporter` (`LogRecordExporter`) over a node or browser transport binding the `ProtobufLogsSerializer`, configured by endpoint/headers/compression/timeout
- Accept: `new OTLPLogExporter(cfg)` wrapped in a `BatchLogRecordProcessor` and fed to `NodeSdk`/`WebSdk` `Configuration.logRecordProcessor`; endpoint/runtime as config + platform-remap selection; the one `AppIdentity`-derived `Resource`; core's `ExportResult` rail
- Reject: `@opentelemetry/*` imports outside `scope:runtime`, this SDK exporter where the native `OtlpLogger` suffices, both the JSON and protobuf exporter rows on one log-export lane, `SimpleLogRecordProcessor` in production, a subclass per backend/compression (that is config), treating the node/browser platform remap as a fork, an unwrapped exporter handed straight to the facade (it needs a processor)
