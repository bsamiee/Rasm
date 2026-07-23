# [TS_RUNTIME_API_OPENTELEMETRY_API_LOGS]

`@opentelemetry/api-logs` is the API tier of the logs signal, beside the trace and metric APIs. It carries the vocabulary no log leg re-declares: `SeverityNumber`, `LogBody`/`LogAttributes`/`AnyValue`, the `LogRecord` input shape, and the `Logger`/`LoggerProvider` contracts. `logs` is the global registration seam — `setGlobalLoggerProvider` installs the SDK once; `getLogger` hands instrumentation a `Logger` that no-ops until a provider exists. Application logging is `Effect.log` under the facade; this tier serves the SDK-bridge leg and third-party instrumentation emitting OTel logs directly.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/api-logs`
- package: `@opentelemetry/api-logs` (Apache-2.0)
- module format: CJS + ESM dual build; depends on `@opentelemetry/api` (the `Context`/`TimeInput` types) as a regular dependency
- line: overlay family lock-stepped with `sdk-logs` and the OTLP exporter family; the trace/metric API tier versions on the stable line while the logs-signal API rides the overlay
- rail: observability/api tier of the logs signal; `@opentelemetry/*` is fenced to `scope:runtime` by the edge ledger
- public surface: `logs`, `Logger`, `LoggerProvider`, `LogRecord`, `LoggerOptions`, `SeverityNumber`, `LogBody`, `LogAttributes`, `AnyValue`, `AnyValueMap`, `createNoopLogger` — the Noop/Proxy classes exist internally and are NOT exported

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the record shape and the shared vocabulary
- rail: observability/logs

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CONSUMER_BOUNDARY]                                                    |
| :-----: | :-------------------------- | :------------ | :--------------------------------------------------------------------- |
|  [01]   | `LogRecord`                 | input record  | `Logger.emit` input (fence); `context?` links to the span              |
|  [02]   | `SeverityNumber`            | severity axis | the one severity vocabulary (fence); `minimumSeverity` + crash `FATAL` |
|  [03]   | `AnyValue` / `AnyValueMap`  | value algebra | recursive log-content value (fence) — richer than span `Attributes`    |
|  [04]   | `LogBody` / `LogAttributes` | content types | body and attribute slots; never a local re-declaration                 |
|  [05]   | `Logger`                    | contract      | the emit surface (fence); `enabled` is the pre-build drop probe        |
|  [06]   | `LoggerProvider`            | contract      | what the SDK implements and the global registration holds              |
|  [07]   | `LoggerOptions`             | scope options | per-logger scope identity — the field is `attributes`                  |

[LOG_RECORD]: `LogRecord.timestamp: TimeInput` `LogRecord.observedTimestamp: TimeInput` `LogRecord.severityNumber: SeverityNumber` `LogRecord.severityText: string` `LogRecord.body: LogBody` `LogRecord.attributes: LogAttributes` `LogRecord.eventName: string` `LogRecord.context: Context`
[LOGGER]: `Logger.emit(LogRecord) -> void` `Logger.enabled({context?:Context;severityNumber?:SeverityNumber;eventName?:string}?) -> boolean`
[LOGGER_PROVIDER]: `LoggerProvider.getLogger(string,string?,LoggerOptions?) -> Logger`
[LOGGER_OPTIONS]: `LoggerOptions.schemaUrl: string` `LoggerOptions.attributes: LogAttributes`
[SEVERITY_NUMBER]: `UNSPECIFIED` `FATAL4`
[ANY_VALUE]: `AnyValue = scalar|Uint8Array|AnyValueArray|AnyValueMap|null|undefined`
[ANY_VALUE_MAP]: `AnyValueMap[string]: AnyValue`
[LOG_BODY]: `LogBody = AnyValue`
[LOG_ATTRIBUTES]: `LogAttributes = AnyValueMap`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the global registration seam
- rail: observability/logs
- `logs` is a singleton, not a class: one global provider per process, installed once at the composition root; every `getLogger` before installation returns a no-op that upgrades when the provider lands.

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                              |
| :-----: | :-------------------------------------------- | :------------- | :--------------------------------------------------------------- |
|  [01]   | `logs.getLogger(name, version?, options?)`    | mint           | instrumentation-side logger acquisition; safe before SDK install |
|  [02]   | `logs.setGlobalLoggerProvider(provider)`      | install        | composition-root-only — the one global registration              |
|  [03]   | `logs.getLoggerProvider()` / `logs.disable()` | introspect     | provider read-back; teardown to the no-op state                  |
|  [04]   | `createNoopLogger()`                          | noop           | the explicit no-op row for kit-driven specs and disabled lanes   |

## [04]-[IMPLEMENTATION_LAW]

[STACKS_WITH]:
- `@opentelemetry/sdk-logs` (`.api/opentelemetry-sdk-logs.md`): the SDK leg implements these contracts — `SdkLogRecord` is the mutable build of this `LogRecord` input, `LoggerConfig.minimumSeverity` keys on `SeverityNumber`, and `LogRecordProcessor.enabled?` mirrors `Logger.enabled` as the same pre-build drop at the processor tier. Vocabulary flows downward; nothing here depends on the SDK.
- `@opentelemetry/exporter-logs-otlp-http` (`.api/opentelemetry-exporter-logs-otlp-http.md`): the OTLP egress of records built against this vocabulary — API → SDK processor → exporter is the one pipeline order.
- `@effect/opentelemetry` (`.api/effect-opentelemetry.md`): under the facade, application logs are `Effect.log` — the facade wires the provider through `Logger.layerLoggerProvider` and owns the global registration; no `plane:runtime` folder calls `logs.setGlobalLoggerProvider` beside it. Direct `logs.getLogger` is the lane for third-party instrumentation emitting OTel logs outside the Effect rail.
- `otel/crash`: fatal capture keys `SeverityNumber.FATAL` + `eventName` on this vocabulary, threading `context` so a crash record correlates to its span.

[LOCAL_ADMISSION]:
- Declare severity, body, and attribute types from this package only; a local severity enum or body union beside it is the named split-brain.
- Install the global provider exactly once at the composition root; instrumentation acquires loggers through `logs.getLogger` and never holds a provider.
- Probe `Logger.enabled` before constructing expensive record bodies on hot paths; the cheap drop belongs before allocation.
- Thread `context` on records emitted inside spans; an uncorrelated log beside an active span is a discarded join key.

[RAIL_LAW]:
- Package: `@opentelemetry/api-logs`
- Owns: the logs-signal API — the `logs` global registration, the `Logger`/`LoggerProvider` contracts, the `LogRecord` input shape, and the `SeverityNumber`/`LogBody`/`LogAttributes`/`AnyValue` vocabulary
- Accept: one composition-root provider install, `logs.getLogger` acquisition, `enabled` pre-build drops, `context`-correlated records, vocabulary imports wherever log content is typed
- Reject: local re-declarations of the severity/body/attribute vocabulary, a second global provider install, SDK types leaking where the API tier suffices, emitting through this tier beside the Effect rail for application logs the facade already owns
