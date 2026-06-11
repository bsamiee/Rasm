# [RASM_APPHOST_API_LOGGING]

`Microsoft.Extensions.Logging.Abstractions` supplies logging contracts, scope identity, event identity, buffered records, and generated logger delegates.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Logging.Abstractions`
- package: `Microsoft.Extensions.Logging.Abstractions`
- assembly: `Microsoft.Extensions.Logging.Abstractions`
- namespace: `Microsoft.Extensions.Logging`
- asset: runtime library
- rail: telemetry

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: contract family
- rail: telemetry

| [INDEX] | [SYMBOL]                 | [PACKAGE_ROLE]        | [CAPABILITY]               |
| :-----: | :----------------------- | :-------------------- | :------------------------- |
|   [1]   | `ILogger`                | contract surface      | defines boundary contract  |
|   [2]   | `ILogger<TCategoryName>` | contract surface      | defines boundary contract  |
|   [3]   | `ILoggerFactory`         | contract surface      | defines boundary contract  |
|   [4]   | `ILoggerProvider`        | contract surface      | defines boundary contract  |
|   [5]   | `EventId`                | event identity        | anchors telemetry contract |
|   [6]   | `LogLevel`               | severity enum         | anchors telemetry contract |
|   [7]   | `LoggerMessage.Define`   | compiled log delegate | anchors telemetry contract |
|   [8]   | `LoggerMessageAttribute` | source-gen attribute  | marks generated log method |
|   [9]   | `BufferedLogRecord`      | buffered log value    | carries batch log payload  |
|  [10]   | `IBufferedLogger`        | buffered contract     | defines batch log boundary |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: logger extensions
- rail: telemetry

| [INDEX] | [SURFACE]            | [CALL_SHAPE]         | [CAPABILITY]           |
| :-----: | :------------------- | :------------------- | :--------------------- |
|   [1]   | `BeginScope<TState>` | scope method         | attaches log scope     |
|   [2]   | `LogDebug`           | logging extension    | emits telemetry event  |
|   [3]   | `LogInformation`     | logging extension    | emits telemetry event  |
|   [4]   | `LogWarning`         | logging extension    | emits telemetry event  |
|   [5]   | `LogError`           | logging extension    | emits telemetry event  |
|   [6]   | `LogCritical`        | logging extension    | emits telemetry event  |
|   [7]   | `IsEnabled`          | enablement predicate | guards log work        |
|   [8]   | `DefineScope`        | compiled scope       | creates cached scope   |
|   [9]   | `LogRecords`         | batch log method     | emits buffered records |

## [4]-[IMPLEMENTATION_LAW]

[LOGGING_TOPOLOGY]:
- namespaces: `Microsoft.Extensions.Logging`, `Microsoft.Extensions.Logging.Abstractions`
- generator assets: logging generator analyzers under package analyzers
- generated shape: partial methods marked with `LoggerMessageAttribute`
- structured state: message template, event id, severity, exception, scope, properties
- buffered shape: `BufferedLogRecord` carries message template, attributes, activity ids, and thread id

[LOCAL_ADMISSION]:
- Logger methods are generated or precompiled delegates on hot paths.
- Event identity is explicit and stable; string messages never define event identity.
- Buffered logging is projection surface material and does not replace runtime receipts.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Logging.Abstractions`
- Owns: structured log contracts and event identity
- Accept: logger handles enter runtime ports
- Reject: ambient static logging
