# [@opentelemetry/sdk-logs] — the log SDK leg of the bridging pin block behind the facade

`@opentelemetry/sdk-logs` owns the log-export pipeline: the `LogRecordProcessor` → `LogRecordExporter` contract, the `SdkLogRecord` mutable builder and its `ReadableLogRecord` read shape, and the `LoggerConfigurator` pattern that enables/filters loggers by scope and severity. It is the youngest signal in the block — pinned `0.219.0` (pre-1.0) against the experimental `@opentelemetry/api-logs 0.219.0`, while the trace/metric legs are `2.8.0`. `otel/emit` never constructs a `LoggerProvider` directly — `@effect/opentelemetry` `NodeSdk`/`WebSdk` take a `Configuration.logRecordProcessor: LogRecordProcessor | ReadonlyArray<LogRecordProcessor>` (verified: `NodeSdk.d.ts` imports `LogRecordProcessor`, `LoggerProviderConfig` from here) and wire it through `Logger.layerLoggerProvider` → `Logger.layerLoggerAdd`, turning Effect's `Effect.log` into SDK log records on the shared resource — one signal spine, never a parallel sink. It collapses with the pin block at `[R3]`; the native `OtlpLogger` lane replaces the process `Logger` with no SDK.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/sdk-logs`
- package: `@opentelemetry/sdk-logs` · version `0.219.0` · license `Apache-2.0`
- module: dual — CJS default (`build/src/index.js`, no `"type"` field) + ESM mirror (`build/esm/index.js`); flat barrel, no `exports` subpath map.
- asset: TSDECL `build/src/index.d.ts` (`assay api resolve @opentelemetry/sdk-logs` → `0.219.0`, restored).
- peer: `@opentelemetry/api >=1.4.0 <1.10.0`; deps `@opentelemetry/api-logs` `0.219.0` (the experimental log API — `SeverityNumber`/`LogBody`/`LogAttributes`/`AnyValue`), `@opentelemetry/core` (`ExportResult`, `InstrumentationScope`), `@opentelemetry/resources` (`Resource`), `@opentelemetry/semantic-conventions`.
- runtime: runtime-neutral — the `./platform` conditional export swaps the node `BatchLogRecordProcessor<BufferConfig>` for the browser `BatchLogRecordProcessor<BatchLogRecordProcessorBrowserConfig>` (document-hide auto-flush); no fork in consumer code.
- plane: `plane:runtime`, edge-ledger-fenced to `scope:runtime`.
- rail: observability/sdk-bridge; `[R3]` collapse target — pre-1.0 ABI, the `@effect/opentelemetry` peer admits `>=0.203.0 <0.300.0`.
- role: the `LogRecordProcessor`/`LogRecordExporter`/`LoggerConfigurator` roster behind `@effect/opentelemetry` `NodeSdk`/`WebSdk` `Configuration.logRecordProcessor`.

## [02]-[PROVIDER]

`LoggerProvider` is the logger factory; every axis — resource, processors, limits, per-logger configurator — is a `LoggerProviderOptions` field. Under the effect facade the provider is built by `Logger.layerLoggerProvider`, so a telemetry consumer supplies the processor and configurator, not the provider. `LoggerProviderConfig` is a pure alias of `LoggerProviderOptions`; effect consumes `Omit<LoggerProviderConfig, "resource">` (the `Resource` layer owns identity).

| [INDEX] | [SYMBOL]                          | [KIND]        | [CONSUMER / BOUNDARY]                                          |
| :-----: | :-------------------------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `LoggerProvider`                  | class         | `implements ILoggerProvider`; `getLogger`/`forceFlush`/`shutdown` |
|  [02]   | `LoggerProviderOptions` / `LoggerProviderConfig` | interface / alias | `resource?`/`processors?`/`logRecordLimits?`/`loggerConfigurator?` |
|  [03]   | `LogRecordLimits`                 | interface     | `attributeCountLimit?` / `attributeValueLengthLimit?`           |

```ts contract
interface LoggerProviderOptions {
  resource?: Resource                        // AppIdentity-derived; supplied by the effect Resource layer under the facade
  forceFlushTimeoutMillis?: number
  logRecordLimits?: LogRecordLimits          // attributeCountLimit / attributeValueLengthLimit
  processors?: LogRecordProcessor[]          // the export pipeline (see [03])
  loggerConfigurator?: LoggerConfigurator    // per-logger enable/severity policy (see [04])
  meterProvider?: MeterProvider              // self-observability
}
type LoggerProviderConfig = LoggerProviderOptions
declare class LoggerProvider implements ILoggerProvider {
  constructor(config?: LoggerProviderOptions)
  getLogger(name: string, version?: string, options?: LoggerOptions): Logger   // name defaults to DEFAULT_LOGGER_NAME = "unknown"
  forceFlush(): Promise<void>; shutdown(): Promise<void>
}
```

## [03]-[PROCESSOR_AND_EXPORTER]

The pipeline mirrors the trace leg: `LogRecordProcessor` owns the emit/flush lifecycle, `LogRecordExporter` owns format/transport, and `Simple` (per-record) / `Batch` (queued, `./platform`) / `Console` / `InMemory` are ROWS on those interfaces. `LogRecordProcessor.enabled?` is the advanced per-emit filter — a processor can reject a record by context/scope/severity/event-name BEFORE it is built, the cheapest drop point. `BatchLogRecordProcessor` is the `./platform` specialization of the internal `BatchLogRecordProcessorBase<T extends BufferConfig>` — node binds `T = BufferConfig`, browser `T = BatchLogRecordProcessorBrowserConfig`.

| [INDEX] | [SYMBOL]                              | [KIND]              | [CAPABILITY / BOUNDARY]                                          |
| :-----: | :------------------------------------ | :------------------ | :--------------------------------------------------------------- |
|  [01]   | `LogRecordProcessor`                  | interface           | `onEmit`/`forceFlush`/`shutdown` + `enabled?` per-emit filter     |
|  [02]   | `SimpleLogRecordProcessor`            | class               | one export per emitted record; sync; diagnostics/test            |
|  [03]   | `BatchLogRecordProcessor`             | class (`./platform`)| queued batch export; the production row; `BufferConfig`-tuned    |
|  [04]   | `BufferConfig` / `BatchLogRecordProcessorBrowserConfig` | interface | batch tuning; browser adds `disableAutoFlushOnDocumentHide`  |
|  [05]   | `LogRecordExporter`                   | interface           | `export(logs, cb)`/`shutdown`/`forceFlush` — format + transport   |
|  [06]   | `ConsoleLogRecordExporter`            | class               | stdout diagnostics                                              |
|  [07]   | `InMemoryLogRecordExporter`           | class               | `getFinishedLogRecords()`/`reset()` — the kit-driven spec lane       |

```ts contract
interface LogRecordProcessor {
  onEmit(logRecord: SdkLogRecord, context?: Context): void
  forceFlush(): Promise<void>; shutdown(): Promise<void>
  enabled?(options: { context: Context; instrumentationScope: InstrumentationScope; severityNumber?: SeverityNumber; eventName?: string }): boolean   // pre-build drop
}
interface LogRecordExporter {
  export(logs: ReadableLogRecord[], resultCallback: (result: ExportResult) => void): void   // ExportResult from @opentelemetry/core
  shutdown(): Promise<void>; forceFlush(): Promise<void>
}
declare class SimpleLogRecordProcessor implements LogRecordProcessor { constructor(exporter: LogRecordExporter) }
declare abstract class BatchLogRecordProcessorBase<T extends BufferConfig> implements LogRecordProcessor { constructor(exporter: LogRecordExporter, config?: T) }   // internal base — not barrel-exported
// ./platform node → `class BatchLogRecordProcessor extends BatchLogRecordProcessorBase<BufferConfig>`  (the barrel export; maxExportBatchSize 512 / scheduledDelayMillis 5000 / maxQueueSize 2048)
```

## [04]-[RECORD_AND_CONFIGURATOR]

`SdkLogRecord` is the mutable chainable builder a processor mutates on emit; `ReadableLogRecord` is the immutable projection the exporter receives — one record type, two views. The log body/attribute/severity vocabulary is the `@opentelemetry/api-logs` peer (`AnyValue`/`LogBody`/`LogAttributes`/`SeverityNumber`), never a local type. `LoggerConfigurator` is the per-logger policy: `createLoggerConfigurator(patterns)` folds a `LoggerPattern[]` (scope-name glob → `LoggerConfig`) into one `(scope) => Required<LoggerConfig>` — a parameterized enable/severity filter, never a hand-rolled per-logger switch.

| [INDEX] | [SYMBOL]                              | [KIND]              | [SHAPE / CAPABILITY]                                            |
| :-----: | :------------------------------------ | :------------------ | :-------------------------------------------------------------- |
|  [01]   | `SdkLogRecord`                        | interface           | mutable builder — `setAttribute`/`setBody`/`setSeverityNumber`/`setEventName` |
|  [02]   | `ReadableLogRecord`                   | interface           | immutable read shape the exporter serializes                    |
|  [03]   | `LoggerConfigurator` / `LoggerConfig` | type / interface    | `(scope) => Required<LoggerConfig>`; `{ disabled?, minimumSeverity?, traceBased? }` |
|  [04]   | `createLoggerConfigurator`            | fn                  | `(LoggerPattern[]) => LoggerConfigurator` — glob→config fold     |
|  [05]   | `LoggerPattern`                       | interface           | `{ pattern: string; config: LoggerConfig }` — the seed row      |

```ts contract
interface SdkLogRecord {   // mutable + chainable; hrTime/spanContext/resource/instrumentationScope are read-only
  severityText?: string; severityNumber?: SeverityNumber; body?: LogBody; eventName?: string
  readonly attributes: LogAttributes; droppedAttributesCount: number
  setAttribute(key: string, value?: AnyValue): SdkLogRecord      // AnyValue/LogBody/LogAttributes/SeverityNumber ← @opentelemetry/api-logs
  setBody(body: LogBody): SdkLogRecord; setEventName(eventName: string): SdkLogRecord
  setSeverityNumber(severityNumber: SeverityNumber): SdkLogRecord; setSeverityText(severityText: string): SdkLogRecord
}
// The per-logger policy — pattern rows folded into one scope→config function:
interface LoggerConfig { disabled?: boolean; minimumSeverity?: SeverityNumber; traceBased?: boolean }
declare function createLoggerConfigurator(patterns: { pattern: string; config: LoggerConfig }[]): (loggerScope: InstrumentationScope) => Required<LoggerConfig>
```

## [05]-[STACKING]

- [STACK: `@effect/opentelemetry` `NodeSdk`/`WebSdk`] — the primary consumer. `Configuration.logRecordProcessor: LogRecordProcessor | ReadonlyArray<LogRecordProcessor>` and `loggerProviderConfig: Omit<LoggerProviderConfig, "resource">` are exactly this package's types (verified import); effect wires them via `Logger.layerLoggerProvider(logRecordProcessor, loggerProviderConfig)` piped into `Logger.layerLoggerAdd`, so `Effect.log` becomes a log record on the shared resource — never a `LoggerProvider` directly, and never the `resource` field.
- [STACK: no admitted log OTLP-HTTP exporter — native lane owns egress] — the roster carries trace + metric OTLP-HTTP exporters (`exporter-trace-otlp-http`, `exporter-metrics-otlp-http`) but NO log one, so OTLP log egress rides the native `@effect/opentelemetry` `OtlpLogger` lane. The SDK-bridge log path here wraps `ConsoleLogRecordExporter` (diagnostics), `InMemoryLogRecordExporter` (kit-driven specs), or a custom `LogRecordExporter`; the log signal is native-lane-first even harder than trace/metric, because its SDK-bridge exporter is unadmitted.
- [STACK: `@opentelemetry/api-logs` vocabulary] — `SeverityNumber` is the severity axis both `LoggerConfig.minimumSeverity` and `SdkLogRecord.severityNumber` key on; `LogBody`/`LogAttributes`/`AnyValue` are the record content types. The log API is still experimental (`0.x`), which is why this leg carries a `0.219.0` pin distinct from the `2.8.0` trace/metric legs — the named ABI-skew fact for the pin block.
- [STACK: `effect` `Logger` — one sink] — `OtlpLogger.layer` (native) or the SDK bridge here REPLACES the process `Logger`; there is never a parallel log sink beside it (the `@effect/opentelemetry` rail-law reject). Structured `Effect.log` and OTLP log records are the same signal on the same `AppIdentity` resource.
- [STACK: `otel/crash`] — crash capture reconstructs `FaultDetail` through the kernel fault-enricher contract and emits it as a log record with `setSeverityNumber(SeverityNumber.FATAL)` + `setEventName`; the `LogRecordProcessor.enabled?` filter and `LoggerConfigurator` minimum-severity are where replay-redaction-at-capture severity gating lands. Instrumentation emits through `Effect.log`; no `plane:runtime` folder imports `sdk-logs` (edge-ledger `scope:runtime` ban).

## [06]-[RAIL_LAW]

- Owns: the log-export pipeline — `LogRecordProcessor`/`LogRecordExporter` contracts + the `Simple`/`Batch` and `Console`/`InMemory` rows, the `SdkLogRecord` builder + `ReadableLogRecord` read shape, and the `LoggerConfigurator` per-logger enable/severity policy.
- Accept: `BatchLogRecordProcessor` wrapping a `LogRecordExporter` for the SDK-bridge path (OTLP log egress rides the native `OtlpLogger` lane — no log OTLP-HTTP exporter is admitted); `createLoggerConfigurator(patterns)` for per-scope severity gating; the `LogRecordProcessor.enabled?` filter for cheap pre-build drops; `InMemoryLogRecordExporter` for kit-driven specs; the whole surface reached through `@effect/opentelemetry` `NodeSdk`/`WebSdk` `Configuration.logRecordProcessor`.
- Reject: constructing `LoggerProvider` inline under the effect facade (`Logger.layerLoggerProvider` owns it); `SimpleLogRecordProcessor` in production (per-record sync export — diagnostics only); a parallel log sink beside the native `OtlpLogger` one; a local severity/body type where the `@opentelemetry/api-logs` vocabulary exists; importing outside `scope:runtime`; treating this leg as permanent — it collapses at `[R3]`.
- Boundary: the `0.219.0` pin trails the trace/metric `2.8.0` legs because the log API is pre-1.0 — the `@effect/opentelemetry` peer admits `sdk-logs >=0.203.0 <0.300.0`. `BatchLogRecordProcessorBase<T>` is internal (not a barrel export) — consume the concrete `BatchLogRecordProcessor`. `SdkLogRecord` is mutable by design (the processor builds it); `ReadableLogRecord` is the frozen export view. Under effect the `resource` is the `AppIdentity`-derived `Resource` layer, not a field set here.
