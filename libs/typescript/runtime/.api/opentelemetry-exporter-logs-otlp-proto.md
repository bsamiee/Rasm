# [TS_RUNTIME_API_OPENTELEMETRY_EXPORTER_LOGS_OTLP_PROTO]

`@opentelemetry/exporter-logs-otlp-proto` is the `LogRecordExporter` that POSTs `ReadableLogRecord` batches to an OTLP/HTTP collector as protobuf — the binary sibling of `.api/opentelemetry-exporter-logs-otlp-http.md`, sharing one `OTLPLogExporter` over `OTLPExporterBase` and diverging only by binding `ProtobufLogsSerializer`. A `BatchLogRecordProcessor` wraps it onto the facade's `Configuration.logRecordProcessor`, closing the `[OTLP_SDK]` wire law's log leg when the collector demands protobuf.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/exporter-logs-otlp-proto`
- package: `@opentelemetry/exporter-logs-otlp-proto` (Apache-2.0)
- otel-peer: `@opentelemetry/api` (peer), `@opentelemetry/core` (the `ExportResult` rail), `@opentelemetry/sdk-logs` (the `LogRecordExporter`/`ReadableLogRecord` contract + the `BatchLogRecordProcessor` wrapper)
- transitive-config: `@opentelemetry/otlp-exporter-base` supplies `OTLPExporterBase`, the `OTLPExporterConfigBase`/`OTLPExporterNodeConfigBase` config types, `CompressionAlgorithm`, and the `createOtlpHttpExportDelegate`/`createLegacyOtlpBrowserExportDelegate` delegates; `@opentelemetry/otlp-transformer` supplies the `ProtobufLogsSerializer` that binds this row protobuf
- consumed-by: `otel/emit` SDK-bridge log leg via the facade's `NodeSdk`/`WebSdk` `Configuration.logRecordProcessor`, on the protobuf-wire selection
- catalog-verdict: KEEP as the protobuf half of the JSON/protobuf log pair, beside the trace-proto and metrics-proto rows
- runtime: dual — the package `browser` field remaps `platform/index` to `platform/node` (`http`/`https`, config `OTLPExporterNodeConfigBase`, delegate `createOtlpHttpExportDelegate`) or `platform/browser` (`fetch`/`sendBeacon`, config `OTLPExporterConfigBase`, delegate `createLegacyOtlpBrowserExportDelegate`); ONE `OTLPLogExporter` name, a build-time platform selection
- modules: `OTLPLogExporter`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the one protobuf `LogRecordExporter`
- rail: observability/export/logs
- This package exports a SINGLE symbol, `OTLPLogExporter extends OTLPExporterBase<ReadableLogRecord[]> implements LogRecordExporter`. Its JSON sibling `.api/opentelemetry-exporter-logs-otlp-http.md` catalogs the shared class, its `export`/`forceFlush`/`shutdown` lifecycle, and the `OTLPExporterConfigBase`/`OTLPExporterNodeConfigBase` config surface; this row adds only the constructor that binds `ProtobufLogsSerializer`.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]      | [CONSUMER_BOUNDARY]                                    |
| :-----: | :----------------------------- | :----------------- | :----------------------------------------------------- |
|  [01]   | `OTLPLogExporter`              | log exporter class | protobuf exporter a `BatchLogRecordProcessor` wraps    |
|  [02]   | `new OTLPLogExporter(config?)` | constructor        | binds `ProtobufLogsSerializer` into `OTLPExporterBase` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: SDK-bridge protobuf log export composition
- rail: observability/export/logs
- Exporter is never a leaf: construct it, wrap it in a `BatchLogRecordProcessor` (production, queued) or `SimpleLogRecordProcessor` (dev, synchronous) from `sdk-logs`, hand the processor to `NodeSdk`/`WebSdk` `Configuration.logRecordProcessor`; `new OTLPLogExporter(config?)` selects its platform config type by the package `browser` remap.
- `url`/`headers`/`compression`/`timeoutMillis` source from config or the `OTEL_EXPORTER_OTLP_LOGS_*` env family (each falling back to `OTEL_EXPORTER_OTLP_*` via core's readers); the default endpoint resolves to `http://localhost:4318/v1/logs`; the protobuf wire is fixed by the package, never a config toggle.

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                           |
| :-----: | :----------------------------------------------------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `new OTLPLogExporter(config?: OTLPExporterNodeConfigBase)`         | node ctor      | node OTLP/HTTP protobuf log exporter          |
|  [02]   | `new OTLPLogExporter(config?: OTLPExporterConfigBase)`             | browser ctor   | browser OTLP/HTTP protobuf log exporter (RUM) |
|  [03]   | `new BatchLogRecordProcessor({ exporter })` → `logRecordProcessor` | composition    | exporter → processor → `NodeSdk`/`WebSdk`     |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Protobuf log leg of the SDK-bridge lane: this exporter enters only when the collector demands protobuf AND the SDK log pipeline (`LoggerConfigurator` gating, the `enabled?` pre-build drop, `BatchLogRecordProcessor` tuning) is required. `.api/effect-opentelemetry.md` [04] owns the native-lane default and the `[OTEL_PIN_BLOCK]` collapse.
- wire-encoding is a serializer binding, never a fork: JSON vs protobuf is the choice between `.api/opentelemetry-exporter-logs-otlp-http.md` and this row — one `OTLPLogExporter` over one `OTLPExporterBase`, binding JSON vs `ProtobufLogsSerializer`. A backend or encoding change is a composition-root selection, never a second exporter type in design code.

[STACKING]:
- `@opentelemetry/exporter-logs-otlp-http`(`.api/opentelemetry-exporter-logs-otlp-http.md`): the JSON sibling owns the shared `OTLPLogExporter`/config/lifecycle surface; this row is its protobuf half. Choose ONE per log-export lane — the collector's accepted encoding decides; both feed the same processor and facade seams.
- `@opentelemetry/exporter-{trace,metrics}-otlp-proto`(`.api/opentelemetry-exporter-trace-otlp-proto.md`, `.api/opentelemetry-exporter-metrics-otlp-proto.md`): the three proto exporters share the `Protobuf{Trace,Metrics,Logs}Serializer` binding pattern from `@opentelemetry/otlp-transformer`, each wrapped by its processor/reader and fed to the one facade.
- `@opentelemetry/sdk-logs`(`.api/opentelemetry-sdk-logs.md`): `new OTLPLogExporter(cfg)` wraps in `new BatchLogRecordProcessor({ exporter })` (the exporter is an options field, not positional); the exporter is a `LogRecordExporter`, the processor owns batching and `forceFlush` drain.
- `@opentelemetry/api-logs`(`.api/opentelemetry-api-logs.md`): the records it serializes carry `SeverityNumber`, `LogBody`/`LogAttributes`/`AnyValue`; API → SDK processor → this exporter is the one pipeline order.
- `@effect/opentelemetry`(`.api/effect-opentelemetry.md`): the wrapped processor rides `NodeSdk`/`WebSdk` `Configuration.logRecordProcessor` beside the one `AppIdentity`-derived `Resource`; the facade owns provider lifecycle, this package owns protobuf wire serialization.
- `@opentelemetry/core`(`.api/opentelemetry-core.md`): `export()` reports through `ExportResult`/`ExportResultCode`; the outbound HTTP `Context` is `suppressTracing`-fenced so OTLP egress is never self-traced.
- `@effect/platform`(`.api/effect-platform.md`): node HTTP binds `createOtlpHttpExportDelegate`, browser fetch `createLegacyOtlpBrowserExportDelegate`; neither rides the `net/client` `HttpClient` retry/proxy policy the native `OtlpLogger` inherits — that transport-policy gap marks this row `[OTEL_PIN_BLOCK]`.
- `otel/emit` (within-lib): the export-boundary owner constructs this exporter at the composition root, scrubs PII through egress-redaction rows before serialization, and defaults to the native `OtlpLogger` lane.

[LOCAL_ADMISSION]:
- `@opentelemetry/*` admits ONLY inside `scope:runtime` (edge-ledger ban); the exporter constructs at the composition root only, and application code logs through `Effect.log`.
- native `OtlpLogger` is the default; this SDK exporter enters only for protobuf-wire SDK-only log-pipeline capability, recorded as an `[OTEL_PIN_BLOCK]` non-collapsed dependency.
- Browser exporter is the RUM log/crash egress leg (`otel/vital`/`otel/crash`) on collectors demanding protobuf; apply the export-boundary redaction rows before a record leaves the browser.

[RAIL_LAW]:
- Package: `@opentelemetry/exporter-logs-otlp-proto`
- Owns: OTLP/HTTP protobuf log serialization — one `OTLPLogExporter` (`LogRecordExporter`) over a node or browser transport binding `ProtobufLogsSerializer`, configured by endpoint/headers/compression/timeout
- Accept: `new OTLPLogExporter(cfg)` wrapped in a `BatchLogRecordProcessor` and fed to `NodeSdk`/`WebSdk` `Configuration.logRecordProcessor`; endpoint/runtime as config + platform-remap selection; the one `AppIdentity`-derived `Resource`; core's `ExportResult` rail
- Reject: `@opentelemetry/*` outside `scope:runtime`, this SDK exporter where the native `OtlpLogger` suffices, both the JSON and protobuf rows on one log-export lane, `SimpleLogRecordProcessor` in production, a subclass per backend/compression, an unwrapped exporter handed straight to the facade
