# [TS_RUNTIME_API_OPENTELEMETRY_SDK_LOGS]

`@opentelemetry/sdk-logs` owns the log-export pipeline: the `LogRecordProcessor` -> `LogRecordExporter` contract, its `Simple`/`Batch`/`Console`/`InMemory` rows, the `SdkLogRecord` mutable builder paired to the `ReadableLogRecord` read shape, and the `LoggerConfigurator` per-logger enable/severity gate.

`Logger.layerLoggerProvider` builds the `LoggerProvider` under the facade, so a consumer supplies a processor and configurator, never the provider; every record mints through `Effect.log*`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/sdk-logs`
- package: `@opentelemetry/sdk-logs` (Apache-2.0)
- module: dual CJS+ESM — `build/src/index.js` CJS `main` + `build/esm/index.js` ESM `module`; one flat barrel, no `exports` subpath map
- runtime: isomorphic — the `./platform` conditional swaps the node `BatchLogRecordProcessor` for the browser variant adding `disableAutoFlushOnDocumentHide`, no consumer fork; peers `@opentelemetry/api`
- depends: `@opentelemetry/api-logs` (`SeverityNumber`/`LogBody`/`LogAttributes`/`AnyValue`), `@opentelemetry/core` (`ExportResult`/`InstrumentationScope`), `@opentelemetry/resources` (`Resource`), `@opentelemetry/semantic-conventions`
- rail: observability/sdk-bridge — the log-pipeline contract behind the facade `Configuration.logRecordProcessor`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the provider, the processor/exporter contracts and their tuning options, the one record type's two views, and the per-logger policy

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [CAPABILITY]                                                                 |
| :-----: | :-------------------------------------- | :------------ | :--------------------------------------------------------------------------- |
|  [01]   | `LoggerProvider`                        | class         | logger factory `implements ILoggerProvider`                                  |
|  [02]   | `LoggerProviderOptions`                 | interface     | provider axes — `resource?`/`processors?`/`loggerConfigurator?`              |
|  [03]   | `LogRecordLimits`                       | interface     | `attributeCountLimit?`/`attributeValueLengthLimit?` caps                     |
|  [04]   | `ForceFlushOptions`                     | interface     | `timeoutMillis?` per-call flush deadline (30000ms)                           |
|  [05]   | `LogRecordProcessor`                    | interface     | emit/flush lifecycle contract + `enabled?` per-emit filter                   |
|  [06]   | `LogRecordProcessorOptions`             | interface     | processor base — `selfObsMeterProvider?` self-observability                  |
|  [07]   | `BatchLogRecordProcessorOptions`        | interface     | node batch tuning — `exporter` + batch/queue size, delay, timeout            |
|  [08]   | `BatchLogRecordProcessorBrowserOptions` | interface     | browser batch tuning — extends node opts + `disableAutoFlushOnDocumentHide?` |
|  [09]   | `LogRecordExporter`                     | interface     | format + transport contract — `export`/`shutdown`/`forceFlush`               |
|  [10]   | `SdkLogRecord`                          | interface     | mutable builder the processor mutates on emit                                |
|  [11]   | `ReadableLogRecord`                     | interface     | immutable read shape the exporter serializes                                 |
|  [12]   | `LoggerConfig`                          | interface     | per-logger policy — `disabled?`/`minimumSeverity?`/`traceBased?`             |
|  [13]   | `LoggerConfigurator`                    | type          | `(InstrumentationScope) => Required<LoggerConfig>` total policy fn           |
|  [14]   | `LoggerPattern`                         | interface     | `{ pattern; config }` seed row `createLoggerConfigurator` folds              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider construction and the flush/shutdown lifecycle

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :-------------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `new LoggerProvider(LoggerProviderOptions?)`                          | ctor     | build the logger factory         |
|  [02]   | `LoggerProvider.getLogger(string, string?, LoggerOptions?) -> Logger` | instance | acquire a scoped `Logger`        |
|  [03]   | `LoggerProvider.forceFlush(ForceFlushOptions?) -> Promise<void>`      | instance | drain every registered processor |
|  [04]   | `LoggerProvider.shutdown() -> Promise<void>`                          | instance | flush then tear down             |

[ENTRYPOINT_SCOPE]: processor and exporter construction, and the contract members a custom row implements

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------------------ | :------- | :---------------------------------------------- |
|  [01]   | `new BatchLogRecordProcessor(BatchLogRecordProcessorOptions)` | ctor     | queued batch export; the production row         |
|  [02]   | `new SimpleLogRecordProcessor({ exporter })`                  | ctor     | per-record synchronous export; diagnostics/test |
|  [03]   | `LogRecordProcessor.onEmit(SdkLogRecord, Context?) -> void`   | instance | processor emit hook                             |
|  [04]   | `LogRecordProcessor.enabled(opts) -> boolean`                 | instance | pre-build drop by context/scope/severity        |
|  [05]   | `new ConsoleLogRecordExporter()`                              | ctor     | stdout diagnostics exporter                     |
|  [06]   | `new InMemoryLogRecordExporter()`                             | ctor     | in-memory spec-lane exporter                    |
|  [07]   | `InMemoryLogRecordExporter.getFinishedLogRecords()`           | instance | collected records; `reset()` clears             |
|  [08]   | `LogRecordExporter.export(ReadableLogRecord[], cb) -> void`   | instance | serialize + transport one batch                 |

[ENTRYPOINT_SCOPE]: the mutable record builder and the per-logger configurator fold

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                                            |
| :-----: | :---------------------------------------------------------------- | :------- | :------------------------------------------------------ |
|  [01]   | `SdkLogRecord.setBody(LogBody) -> SdkLogRecord`                   | instance | set body; chainable                                     |
|  [02]   | `SdkLogRecord.setAttribute(string, AnyValue?) -> SdkLogRecord`    | instance | one attribute; `setAttributes(LogAttributes)` for many  |
|  [03]   | `SdkLogRecord.setSeverityNumber(SeverityNumber) -> SdkLogRecord`  | instance | severity axis; `setSeverityText(string)` the level text |
|  [04]   | `SdkLogRecord.setEventName(string) -> SdkLogRecord`               | instance | set the event name                                      |
|  [05]   | `createLoggerConfigurator(LoggerPattern[]) -> LoggerConfigurator` | factory  | fold scope-glob patterns into one total policy fn       |

- `SdkLogRecord.set*`: each setter mutates the record in place and returns the same instance; the processor builds it and the exporter reads it, never mutating.
- `LogRecordProcessor.enabled`: optional; `opts` is `{ context, instrumentationScope, severityNumber?, eventName? }`, and a present `enabled` rejects a record before it is built — the cheapest drop point.
- `BatchLogRecordProcessor`: node binds `BatchLogRecordProcessorOptions` and browser `BatchLogRecordProcessorBrowserOptions` through the `./platform` conditional export.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one record type, two views: `SdkLogRecord` is the mutable builder a processor mutates on emit and `ReadableLogRecord` the frozen projection the exporter serializes, so the export shape is read-only by construction.
- every provider axis is a `LoggerProviderOptions` field — resource, processors, limits, per-logger configurator — so a new pipeline knob is one field, never a provider subclass.
- `LoggerConfigurator` is a total `(scope) => Required<LoggerConfig>` fold over `LoggerPattern[]`, a parameterized enable/severity gate keyed on scope-name glob, never a per-logger switch.
- a processor takes one options object with `exporter` as a field, never a positional argument; batch tuning, queue caps, and delays are sibling fields on that object.

[STACKING]:
- `@opentelemetry/api-logs`(`.api/opentelemetry-api-logs.md`): `SeverityNumber` keys `SdkLogRecord.severityNumber` and `LoggerConfig.minimumSeverity`; `LogBody`/`LogAttributes`/`AnyValue` type `SdkLogRecord.body`/`setAttribute`, and `LogRecordProcessor.enabled?` mirrors `Logger.enabled` as the same pre-build drop one tier down.
- `@opentelemetry/exporter-logs-otlp-{http,proto}`(`.api/opentelemetry-exporter-logs-otlp-http.md`, `.api/opentelemetry-exporter-logs-otlp-proto.md`): each `OTLPLogExporter` is a `LogRecordExporter` riding `new BatchLogRecordProcessor({ exporter })` as its options field — the JSON and protobuf legs of SDK-bridge log egress, one chosen per lane by the collector's wire.
- `@opentelemetry/resources`(`.api/opentelemetry-resources.md`): `Resource` sets `LoggerProviderOptions.resource` and reads back readonly on `SdkLogRecord.resource`/`ReadableLogRecord.resource`, so trace, metric, and log carry the one `AppIdentity`-derived identity.
- `@effect/opentelemetry`(`.api/effect-opentelemetry.md`): `Logger.layerLoggerProvider` builds the `LoggerProvider` from a `LogRecordProcessor` + options into the `Logger.OtelLoggerProvider` Tag, `Logger.layerLoggerReplace`/`layerLoggerAdd` route it, and native `OtlpLogger.layer` is the default alternative to this SDK bridge.
- `otel/emit` (within-lib): the export-boundary owner composes `BatchLogRecordProcessor` wrapping an `OTLPLogExporter` onto the facade at the composition root, scrubs PII through egress-redaction rows before serialization, and owns `LoggerConfigurator` severity gating and the `enabled?` drop; `otel/crash` emits its fatal fold as a record keyed `SeverityNumber.FATAL` through the same gate.

[LOCAL_ADMISSION]:
- `@opentelemetry/*` admits only inside `scope:runtime` (edge-ledger ban); no other folder constructs a `LoggerProvider`, and application code logs through `Effect.log`.
- native `OtlpLogger` is the default log lane; this SDK bridge enters only where the SDK log pipeline is required — severity gating, the `enabled?` pre-build drop, or `BatchLogRecordProcessor` tuning — recorded as an `[OTEL_PIN_BLOCK]` non-collapsed dependency, with `.api/effect-opentelemetry.md` owning the native-versus-SDK lane doctrine and the collapse.
- `ConsoleLogRecordExporter` serves diagnostics and `InMemoryLogRecordExporter` the kit-driven specs through `getFinishedLogRecords()`.

[RAIL_LAW]:
- Package: `@opentelemetry/sdk-logs`
- Owns: log-export pipeline — `LogRecordProcessor`/`LogRecordExporter` contracts with the `Simple`/`Batch`/`Console`/`InMemory` rows, the `LoggerProvider` factory, the `SdkLogRecord` builder + `ReadableLogRecord` read shape, and the `LoggerConfigurator` per-logger enable/severity policy
- Accept: `BatchLogRecordProcessor` wrapping a `LogRecordExporter` for the SDK-bridge path; `createLoggerConfigurator(patterns)` for per-scope gating; the `enabled?` pre-build drop; `InMemoryLogRecordExporter` for specs; the whole surface reached through the facade `Logger.layerLoggerProvider`
- Reject: constructing `LoggerProvider` inline under the facade (`Logger.layerLoggerProvider` owns it); `SimpleLogRecordProcessor` in production; a parallel log sink beside the composition-root one; a local severity or body type where the `@opentelemetry/api-logs` vocabulary exists; a positional exporter argument; importing outside `scope:runtime`
