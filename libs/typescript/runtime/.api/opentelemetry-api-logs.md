# [@opentelemetry/api-logs] — the logs-signal API tier: the global `logs` registration, `Logger.emit`, and the severity/body/attribute vocabulary

`@opentelemetry/api-logs` is the API tier of the logs signal — the third signal joining the runtime otel plane beside trace and metric APIs. It carries the vocabulary every log leg shares and none may re-declare: `SeverityNumber` (the 24-step severity axis), `LogBody`/`LogAttributes`/`AnyValue` (the content types), the `LogRecord` input shape, and the `Logger`/`LoggerProvider` contracts. The `logs` singleton is the global registration seam: `logs.setGlobalLoggerProvider(provider)` installs the SDK once at the composition root, `logs.getLogger(name, version?, options?)` hands instrumentation a `Logger` that no-ops until a provider exists — the library law in one API: a library emits through this tier and produces nothing until an app installs the SDK. It still rides the experimental `0.220` line while the trace/metric APIs are stable — the named ABI-skew fact of the logs signal. Under the effect facade, application logging is `Effect.log` wired through `Logger.layerLoggerProvider`; this API tier exists for the SDK-bridge leg's vocabulary and for third-party instrumentation that emits OTel logs directly.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/api-logs`
- package: `@opentelemetry/api-logs` (Apache-2.0, OpenTelemetry JS)
- module format: CJS + ESM dual build; depends on `@opentelemetry/api` (the `Context`/`TimeInput` types) as a regular dependency
- line: experimental `0.220` family — lock-stepped with `sdk-logs` and the OTLP exporter family; the trace/metric API tier is stable, this signal's API is not yet
- rail: observability/api tier of the logs signal; `@opentelemetry/*` is fenced to `scope:runtime` by the edge ledger
- public surface: `logs`, `Logger`, `LoggerProvider`, `LogRecord`, `LoggerOptions`, `SeverityNumber`, `LogBody`, `LogAttributes`, `AnyValue`, `AnyValueMap`, `createNoopLogger` — the Noop/Proxy classes exist internally and are NOT exported

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the record shape and the shared vocabulary
- rail: observability/logs

| [INDEX] | [SYMBOL]                                                                                       | [TYPE_FAMILY] | [CONSUMER / BOUNDARY]                                             |
| :-----: | :----------------------------------------------------------------------------------------------- | :------------ | :------------------------------------------------------------------- |
|  [01]   | `LogRecord` (`timestamp?`, `observedTimestamp?`, `severityNumber?`, `severityText?`, `body?`, `attributes?`, `eventName?`, `context?`) | input record | what `Logger.emit` takes; `context?` links the record to the active span |
|  [02]   | `SeverityNumber` (enum: `UNSPECIFIED = 0`, then `TRACE`/`DEBUG`/`INFO`/`WARN`/`ERROR`/`FATAL` anchors stepping by 4 to `FATAL4 = 24`) | severity axis | the one severity vocabulary — `sdk-logs` `LoggerConfig.minimumSeverity` and the crash lane's `FATAL` key on it |
|  [03]   | `AnyValue` (`scalar \| Uint8Array \| AnyValueArray \| AnyValueMap \| null \| undefined`) / `AnyValueMap` | value algebra | the recursive log-content value type — richer than span `Attributes` |
|  [04]   | `LogBody` (= `AnyValue`) / `LogAttributes` (= `AnyValueMap`)                                       | content types | body and attribute slots; never a local re-declaration                |
|  [05]   | `Logger` (`emit(logRecord)`, `enabled(options?)`)                                                  | contract      | the emit surface; `enabled` is the cheap pre-build drop probe          |
|  [06]   | `LoggerProvider` (`getLogger(name, version?, options?)`)                                           | contract      | what the SDK implements and the global registration holds              |
|  [07]   | `LoggerOptions` (`schemaUrl?`, `attributes?: LogAttributes`)                                       | scope options | per-logger scope identity — the field is `attributes`, not `scopeAttributes` |

```ts contract
interface LogRecord {
  timestamp?: TimeInput; observedTimestamp?: TimeInput          // TimeInput from @opentelemetry/api
  severityNumber?: SeverityNumber; severityText?: string
  body?: LogBody; attributes?: LogAttributes; eventName?: string
  context?: Context                                             // span linkage — trace-correlated logs
}
interface Logger {
  emit(logRecord: LogRecord): void
  enabled(options?: { context?: Context; severityNumber?: SeverityNumber; eventName?: string }): boolean   // pre-build drop probe
}
```

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the global registration seam
- rail: observability/logs
- `logs` is a singleton, not a class: one global provider per process, installed once at the composition root; every `getLogger` before installation returns a no-op that upgrades when the provider lands.

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                           |
| :-----: | :------------------------------------------------------------------------ | :------------- | :------------------------------------------------------------------ |
|  [01]   | `logs.getLogger(name, version?, options?): Logger`                         | mint           | instrumentation-side logger acquisition; safe before SDK install      |
|  [02]   | `logs.setGlobalLoggerProvider(provider): LoggerProvider`                   | install        | composition-root-only — the one global registration                   |
|  [03]   | `logs.getLoggerProvider(): LoggerProvider` / `logs.disable()`              | introspect     | provider read-back; teardown to the no-op state                       |
|  [04]   | `createNoopLogger(): Logger`                                               | noop           | the explicit no-op row for kit-driven specs and disabled lanes         |

## [04]-[IMPLEMENTATION_LAW]

[STACKS_WITH]:
- `@opentelemetry/sdk-logs` (`.api/opentelemetry-sdk-logs.md`): the SDK leg implements these contracts — `SdkLogRecord` is the mutable build of this `LogRecord` input, `LoggerConfig.minimumSeverity` keys on `SeverityNumber`, and `LogRecordProcessor.enabled?` mirrors `Logger.enabled` as the same pre-build drop at the processor tier. The vocabulary flows downward; nothing here depends on the SDK.
- `@opentelemetry/exporter-logs-otlp-http` (`.api/opentelemetry-exporter-logs-otlp-http.md`): the OTLP egress of records built against this vocabulary — API → SDK processor → exporter is the one pipeline order.
- `@effect/opentelemetry` (`libs/typescript/.api/effect-opentelemetry.md`): under the facade, application logs are `Effect.log` — the facade wires the provider through `Logger.layerLoggerProvider` and owns the global registration; no `plane:runtime` folder calls `logs.setGlobalLoggerProvider` beside it. Direct `logs.getLogger` is the lane for third-party instrumentation emitting OTel logs outside the Effect rail.
- `otel/crash`: the fatal-capture lane keys `SeverityNumber.FATAL` + `eventName` on this vocabulary and threads `context` so a crash record correlates to its span.

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
