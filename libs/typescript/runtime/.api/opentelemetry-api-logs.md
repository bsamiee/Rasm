# [TS_RUNTIME_API_OPENTELEMETRY_API_LOGS]

`@opentelemetry/api-logs` owns the logs-signal API tier beside the trace and metric APIs: the `SeverityNumber`/`AnyValue`/`LogBody`/`LogAttributes` vocabulary, the `LogRecord` input shape, the `Logger`/`LoggerProvider` contracts, and the `logs` global registration seam. Application logging rides `Effect.log` under the facade; this tier serves the SDK bridge and third-party instrumentation emitting OTel logs directly.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/api-logs`
- package: `@opentelemetry/api-logs` (Apache-2.0)
- module: CJS + ESM dual build
- runtime: neutral; peers `@opentelemetry/api` for the `Context`/`TimeInput` types
- rail: observability/logs API tier; edge-ledger-fenced to `scope:runtime`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the log-record shape and shared content vocabulary

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CONSUMER_BOUNDARY]                                  |
| :-----: | :-------------------------- | :------------ | :--------------------------------------------------- |
|  [01]   | `LogRecord`                 | input record  | `Logger.emit` input; `context` links to the span     |
|  [02]   | `SeverityNumber`            | severity axis | the one severity vocabulary; crash keys `FATAL`      |
|  [03]   | `AnyValue` / `AnyValueMap`  | value algebra | recursive log content, richer than span `Attributes` |
|  [04]   | `LogBody` / `LogAttributes` | content types | body and attribute slots on the record               |
|  [05]   | `Logger`                    | contract      | emit surface; `enabled` is the pre-build drop probe  |
|  [06]   | `LoggerProvider`            | contract      | SDK-implemented; the global registration holds it    |
|  [07]   | `LoggerOptions`             | scope options | per-logger scope identity via `attributes`           |

[LOG_RECORD]: `LogRecord.timestamp: TimeInput` `LogRecord.observedTimestamp: TimeInput` `LogRecord.severityNumber: SeverityNumber` `LogRecord.severityText: string` `LogRecord.body: LogBody` `LogRecord.attributes: LogAttributes` `LogRecord.eventName: string` `LogRecord.exception: unknown` `LogRecord.context: Context`
[LOGGER]: `Logger.emit(LogRecord) -> void` `Logger.enabled({context?:Context;severityNumber?:SeverityNumber;eventName?:string}?) -> boolean`
[LOGGER_PROVIDER]: `LoggerProvider.getLogger(string,string?,LoggerOptions?) -> Logger`
[LOGGER_OPTIONS]: `LoggerOptions.schemaUrl: string` `LoggerOptions.attributes: LogAttributes`
[SEVERITY_NUMBER]: `UNSPECIFIED` `FATAL4`
[ANY_VALUE]: `AnyValue = scalar|Uint8Array|AnyValueArray|AnyValueMap|null|undefined`
[ANY_VALUE_MAP]: `AnyValueMap[string]: AnyValue`
[LOG_BODY]: `LogBody = AnyValue`
[LOG_ATTRIBUTES]: `LogAttributes = AnyValueMap`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the global logs registration seam

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                       |
| :-----: | :-------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `logs.getLogger(name, version?, options?)`    | mint           | instrumentation-side acquisition; safe before SDK install |
|  [02]   | `logs.setGlobalLoggerProvider(provider)`      | install        | composition-root-only global registration                 |
|  [03]   | `logs.getLoggerProvider()` / `logs.disable()` | introspect     | provider read-back; teardown to the no-op state           |
|  [04]   | `createNoopLogger()`                          | noop           | explicit no-op for kit-driven specs and disabled lanes    |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `logs` is one process-global provider installed once at the composition root; every `getLogger` before install returns a no-op that upgrades when the provider lands.

[STACKING]:
- `@opentelemetry/sdk-logs`(`.api/opentelemetry-sdk-logs.md`): the SDK implements these contracts — `SdkLogRecord` is the mutable build of this `LogRecord` input, `LoggerConfig.minimumSeverity` keys on `SeverityNumber`, and `LogRecordProcessor.enabled?` mirrors `Logger.enabled` as the same pre-build drop at the processor tier; vocabulary flows downward and nothing here depends on the SDK.
- `@opentelemetry/exporter-logs-otlp-http`(`.api/opentelemetry-exporter-logs-otlp-http.md`): `OTLPLogExporter` serializes `ReadableLogRecord`s carrying this vocabulary — API → SDK processor → exporter is the one pipeline order.
- `@effect/opentelemetry`(`.api/effect-opentelemetry.md`): the facade wires the provider through `Logger.layerLoggerProvider` and owns the global registration, so `Effect.log` is the application-log path; direct `logs.getLogger` serves third-party instrumentation emitting OTel logs outside the Effect rail.
- `otel/crash`: fatal capture keys `SeverityNumber.FATAL` and `eventName` on this vocabulary, threading `context` so a crash record correlates to its span.

[LOCAL_ADMISSION]:
- Install the global provider once at the composition root; instrumentation acquires loggers through `logs.getLogger` and never holds a provider.
- Probe `Logger.enabled` before constructing an expensive record body on a hot path; the cheap drop precedes allocation.
- Thread `context` on records emitted inside a span; an uncorrelated log discards its join key.

[RAIL_LAW]:
- Package: `@opentelemetry/api-logs`
- Owns: the logs-signal API — the `logs` registration, the `Logger`/`LoggerProvider` contracts, the `LogRecord` input shape, and the `SeverityNumber`/`LogBody`/`LogAttributes`/`AnyValue` vocabulary
- Accept: one composition-root provider install, `logs.getLogger` acquisition, `enabled` pre-build drops, `context`-correlated records, vocabulary imports wherever log content is typed
- Reject: a local severity/body/attribute type beside this vocabulary, a second global provider install, SDK types where the API tier suffices, emitting application logs through this tier beside the Effect rail the facade owns
