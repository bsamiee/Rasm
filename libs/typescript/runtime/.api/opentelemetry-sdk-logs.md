# [TS_RUNTIME_API_OPENTELEMETRY_SDK_LOGS]

`@opentelemetry/sdk-logs` owns the log-export pipeline: the `LogRecordProcessor` → `LogRecordExporter` contract, the `SdkLogRecord` builder with its `ReadableLogRecord` read shape, and the `LoggerConfigurator` gate on logger scope and severity. `otel/emit` never constructs a `LoggerProvider` directly — `@effect/opentelemetry` wires `Configuration.logRecordProcessor` through `Logger.layerLoggerProvider` → `Logger.layerLoggerAdd`, turning `Effect.log` into SDK log records on one signal spine, never a parallel sink; the native `OtlpLogger` lane replaces the process `Logger` with no SDK.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/sdk-logs`
- package: `@opentelemetry/sdk-logs` (Apache-2.0)
- module: dual — CJS default (`build/src/index.js`, no `"type"` field) + ESM mirror (`build/esm/index.js`); flat barrel, no `exports` subpath map.
- asset: TSDECL `build/src/index.d.ts` (restored).
- peer: `@opentelemetry/api >=catalog <catalog`; deps `@opentelemetry/api-logs` `catalog` (the overlay log API — `SeverityNumber`/`LogBody`/`LogAttributes`/`AnyValue`), `@opentelemetry/core` (`ExportResult`, `InstrumentationScope`), `@opentelemetry/resources` (`Resource`), `@opentelemetry/semantic-conventions`.
- runtime: runtime-neutral — the `./platform` conditional export swaps the node `BatchLogRecordProcessor<BatchLogRecordProcessorOptions>` for the browser `BatchLogRecordProcessor<BatchLogRecordProcessorBrowserOptions>` (document-hide auto-flush); no fork in consumer code.
- plane: `plane:runtime`, edge-ledger-fenced to `scope:runtime`.
- rail: observability/sdk-bridge; `[OTEL_PIN_BLOCK]` collapse target — pre-1.0 ABI, the `@effect/opentelemetry` peer admits `>=catalog <catalog`.
- role: the `LogRecordProcessor`/`LogRecordExporter`/`LoggerConfigurator` roster behind `@effect/opentelemetry` `NodeSdk`/`WebSdk` `Configuration.logRecordProcessor`.

## [02]-[PROVIDER]

`LoggerProvider` is the logger factory; every axis — resource, processors, limits, per-logger configurator — is a `LoggerProviderOptions` field. Under the effect facade the provider is built by `Logger.layerLoggerProvider`, so a telemetry consumer supplies the processor and configurator, not the provider. `forceFlush` takes a per-call `ForceFlushOptions` (`timeoutMillis`, default 30000ms); effect's `loggerProviderConfig` field is `Omit<LoggerProviderConfig, "resource">` (the `Resource` layer owns identity).

| [INDEX] | [SYMBOL]                | [KIND]    | [CONSUMER_BOUNDARY]                                                        |
| :-----: | :---------------------- | :-------- | :------------------------------------------------------------------------- |
|  [01]   | `LoggerProvider`        | class     | `implements ILoggerProvider`; `getLogger`/`forceFlush`/`shutdown`          |
|  [02]   | `LoggerProviderOptions` | interface | `resource?`/`processors?`/`loggerConfigurator?`/`logRecordLimits?` (fence) |
|  [03]   | `ForceFlushOptions`     | interface | `timeoutMillis?` — per-call flush deadline for `forceFlush`                |
|  [04]   | `LogRecordLimits`       | interface | `attributeCountLimit?` / `attributeValueLengthLimit?`                      |

[LOGGER_PROVIDER_OPTIONS]: `LoggerProviderOptions.resource: Resource` `LoggerProviderOptions.logRecordLimits: LogRecordLimits` `LoggerProviderOptions.processors: LogRecordProcessor[]` `LoggerProviderOptions.loggerConfigurator: LoggerConfigurator` `LoggerProviderOptions.meterProvider: MeterProvider`
[FORCE_FLUSH_OPTIONS]: `ForceFlushOptions.timeoutMillis: number`
[LOGGER_PROVIDER]: `LoggerProvider(LoggerProviderOptions?)` `LoggerProvider.getLogger(string,string?,LoggerOptions?) -> Logger` `LoggerProvider.forceFlush(ForceFlushOptions?) -> Promise<void>` `LoggerProvider.shutdown() -> Promise<void>`

## [03]-[PROCESSOR_AND_EXPORTER]

`LogRecordProcessor` owns the emit/flush lifecycle and `LogRecordExporter` format/transport, mirroring the trace leg; `Simple` (per-record) / `Batch` (queued, `./platform`) / `Console` / `InMemory` are ROWS on those interfaces. `LogRecordProcessor.enabled?` rejects a record by context/scope/severity/event-name BEFORE it is built — the cheapest drop point. Each processor takes one `options` object with `exporter` as a field (`new BatchLogRecordProcessor({ exporter, ... })`), never a positional arg; browser's options variant adds `disableAutoFlushOnDocumentHide`.

| [INDEX] | [SYMBOL]                                | [KIND]               | [CAPABILITY_BOUNDARY]                                                |
| :-----: | :-------------------------------------- | :------------------- | :------------------------------------------------------------------- |
|  [01]   | `LogRecordProcessor`                    | interface            | `onEmit`/`forceFlush(opts?)`/`shutdown` + `enabled?` per-emit filter |
|  [02]   | `SimpleLogRecordProcessor`              | class                | one export per emitted record; sync; diagnostics/test                |
|  [03]   | `BatchLogRecordProcessor`               | class (`./platform`) | queued batch export; the production row; options-object tuned        |
|  [04]   | `LogRecordProcessorOptions`             | interface            | processor base opts — `selfObsMeterProvider?` self-observability     |
|  [05]   | `BatchLogRecordProcessorOptions`        | interface            | node batch tuning: `exporter` + batch/queue size, delay, timeout     |
|  [06]   | `BatchLogRecordProcessorBrowserOptions` | interface            | extends node opts; adds `disableAutoFlushOnDocumentHide`             |
|  [07]   | `LogRecordExporter`                     | interface            | `export(logs, cb)`/`shutdown`/`forceFlush` — format + transport      |
|  [08]   | `ConsoleLogRecordExporter`              | class                | stdout diagnostics                                                   |
|  [09]   | `InMemoryLogRecordExporter`             | class                | `getFinishedLogRecords()`/`reset()` — the kit-driven spec lane       |

[LOG_RECORD_PROCESSOR]: `LogRecordProcessor.onEmit(SdkLogRecord,Context?) -> void` `LogRecordProcessor.forceFlush(ForceFlushOptions?) -> Promise<void>` `LogRecordProcessor.shutdown() -> Promise<void>` `LogRecordProcessor.enabled({context:Context;instrumentationScope:InstrumentationScope;severityNumber?:SeverityNumber;eventName?:string}) -> boolean`
[LOG_RECORD_EXPORTER]: `LogRecordExporter.export(ReadableLogRecord[],(result:ExportResult)=>void) -> void` `LogRecordExporter.shutdown() -> Promise<void>` `LogRecordExporter.forceFlush() -> Promise<void>`
[LOG_RECORD_PROCESSOR_OPTIONS]: `LogRecordProcessorOptions.selfObsMeterProvider: MeterProvider`
[SIMPLE_LOG_RECORD_PROCESSOR_OPTIONS]: `SimpleLogRecordProcessorOptions.exporter: LogRecordExporter`
[BATCH_LOG_RECORD_PROCESSOR_OPTIONS]: `BatchLogRecordProcessorOptions.exporter: LogRecordExporter` `BatchLogRecordProcessorOptions.maxExportBatchSize: number` `BatchLogRecordProcessorOptions.scheduledDelayMillis: number` `BatchLogRecordProcessorOptions.exportTimeoutMillis: number` `BatchLogRecordProcessorOptions.maxQueueSize: number`
[SIMPLE_LOG_RECORD_PROCESSOR]: `SimpleLogRecordProcessor(SimpleLogRecordProcessorOptions)`
[BATCH_LOG_RECORD_PROCESSOR_BASE]: `BatchLogRecordProcessorBase(T)`

## [04]-[RECORD_AND_CONFIGURATOR]

`SdkLogRecord` is the mutable builder a processor mutates on emit; `ReadableLogRecord` is the immutable projection the exporter receives — one record type, two views. Log body/attribute/severity vocabulary is the `@opentelemetry/api-logs` peer (`AnyValue`/`LogBody`/`LogAttributes`/`SeverityNumber`), never a local type. `LoggerConfigurator` is the per-logger policy: `createLoggerConfigurator(patterns)` folds a `LoggerPattern[]` (scope-name glob → `LoggerConfig`) into one `(scope) => Required<LoggerConfig>` — a parameterized enable/severity filter, never a hand-rolled per-logger switch.

| [INDEX] | [SYMBOL]                              | [KIND]           | [SHAPE_CAPABILITY]                                                          |
| :-----: | :------------------------------------ | :--------------- | :-------------------------------------------------------------------------- |
|  [01]   | `SdkLogRecord`                        | interface        | mutable builder — `setAttribute(s)`/`setBody`/`setSeverity*`/`setEventName` |
|  [02]   | `ReadableLogRecord`                   | interface        | immutable read shape the exporter serializes                                |
|  [03]   | `LoggerConfigurator` / `LoggerConfig` | type / interface | `(scope) => Required<LoggerConfig>` (config fields in fence)                |
|  [04]   | `createLoggerConfigurator`            | fn               | `(LoggerPattern[]) => LoggerConfigurator` — glob→config fold                |
|  [05]   | `LoggerPattern`                       | interface        | `{ pattern: string; config: LoggerConfig }` — the seed row                  |

[SDK_LOG_RECORD]: `SdkLogRecord.hrTime: HrTime` `SdkLogRecord.hrTimeObserved: HrTime` `SdkLogRecord.spanContext: SpanContext` `SdkLogRecord.severityText: string` `SdkLogRecord.severityNumber: SeverityNumber` `SdkLogRecord.body: LogBody` `SdkLogRecord.eventName: string` `SdkLogRecord.resource: Resource` `SdkLogRecord.instrumentationScope: InstrumentationScope` `SdkLogRecord.attributes: LogAttributes` `SdkLogRecord.droppedAttributesCount: number` `SdkLogRecord.setAttribute(string,AnyValue?) -> SdkLogRecord` `SdkLogRecord.setAttributes(LogAttributes) -> SdkLogRecord` `SdkLogRecord.setBody(LogBody) -> SdkLogRecord` `SdkLogRecord.setEventName(string) -> SdkLogRecord` `SdkLogRecord.setSeverityNumber(SeverityNumber) -> SdkLogRecord` `SdkLogRecord.setSeverityText(string) -> SdkLogRecord`
[LOGGER_CONFIG]: `LoggerConfig.disabled: boolean` `LoggerConfig.minimumSeverity: SeverityNumber` `LoggerConfig.traceBased: boolean`
[SURFACES]: `createLoggerConfigurator({pattern:string;config:LoggerConfig}[]) -> (loggerScope:InstrumentationScope)=>Required<LoggerConfig>`

## [05]-[STACKING]

- Stack with `@effect/opentelemetry` `NodeSdk`/`WebSdk`, the primary consumer: `Configuration.logRecordProcessor: LogRecordProcessor | ReadonlyArray<LogRecordProcessor>` and `loggerProviderConfig: Omit<LoggerProviderConfig, "resource">` are exactly this package's types; effect pipes `Logger.layerLoggerProvider(logRecordProcessor, loggerProviderConfig)` into `Logger.layerLoggerAdd`, so `Effect.log` becomes a log record on the shared resource — never a `LoggerProvider` directly, never the `resource` field.
- Native lane owns OTLP log egress: the roster carries trace + metric OTLP-HTTP exporters but no log one, so OTLP log egress rides the native `@effect/opentelemetry` `OtlpLogger` lane. SDK-bridge log paths here wrap `ConsoleLogRecordExporter` (diagnostics), `InMemoryLogRecordExporter` (kit-driven specs), or a custom `LogRecordExporter`.
- Stack with `@opentelemetry/api-logs` vocabulary: `SeverityNumber` is the severity axis both `LoggerConfig.minimumSeverity` and `SdkLogRecord.severityNumber` key on; `LogBody`/`LogAttributes`/`AnyValue` are the record content types. `api-logs` versions on the overlay line apart from the trace/metric legs — the ABI-skew fact the pin block names.
- Stack with `effect` `Logger` — one sink: `OtlpLogger.layer` (native) or the SDK bridge here REPLACES the process `Logger`; there is never a parallel log sink beside it (the `@effect/opentelemetry` rail-law reject). Structured `Effect.log` and OTLP log records are the same signal on the same `AppIdentity` resource.
- Stack with `otel/crash`: crash capture reconstructs `FaultDetail` through the kernel fault-enricher contract and emits it as a log record with `setSeverityNumber(SeverityNumber.FATAL)` + `setEventName`; the `LogRecordProcessor.enabled?` filter and `LoggerConfigurator` minimum-severity are where replay-redaction-at-capture severity gating lands. Instrumentation emits through `Effect.log`; no `plane:runtime` folder imports `sdk-logs` (edge-ledger `scope:runtime` ban).

## [06]-[RAIL_LAW]

- Owns: the log-export pipeline — `LogRecordProcessor`/`LogRecordExporter` contracts + the `Simple`/`Batch` and `Console`/`InMemory` rows, the `SdkLogRecord` builder + `ReadableLogRecord` read shape, and the `LoggerConfigurator` per-logger enable/severity policy.
- Accept: `BatchLogRecordProcessor` wrapping a `LogRecordExporter` for the SDK-bridge path (OTLP log egress rides the native `OtlpLogger` lane — no log OTLP-HTTP exporter is admitted); `createLoggerConfigurator(patterns)` for per-scope severity gating; the `LogRecordProcessor.enabled?` filter for cheap pre-build drops; `InMemoryLogRecordExporter` for kit-driven specs; the whole surface reached through `@effect/opentelemetry` `NodeSdk`/`WebSdk` `Configuration.logRecordProcessor`.
- Reject: constructing `LoggerProvider` inline under the effect facade (`Logger.layerLoggerProvider` owns it); `SimpleLogRecordProcessor` in production (per-record sync export — diagnostics only); a parallel log sink beside the native `OtlpLogger` one; a local severity/body type where the `@opentelemetry/api-logs` vocabulary exists; importing outside `scope:runtime`; treating this leg as permanent — it collapses at `[OTEL_PIN_BLOCK]`.
- Boundary: the `catalog` pin trails the trace/metric `catalog` legs because the log API is pre-1.0 — the `@effect/opentelemetry` peer admits `sdk-logs >=catalog <catalog`. `BatchLogRecordProcessorBase<T>` is internal (not a barrel export) — consume the concrete `BatchLogRecordProcessor`. `SdkLogRecord` is mutable by design (the processor builds it); `ReadableLogRecord` is the frozen export view. Under effect the `resource` is the `AppIdentity`-derived `Resource` layer, not a field set here.
