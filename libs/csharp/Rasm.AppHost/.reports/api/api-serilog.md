# [RASM_APPHOST_API_SERILOG]

`Serilog` supplies structured event construction, enrichment, sinks, levels, and log context for projection.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Serilog`
- package: `Serilog`
- assembly: `Serilog`
- namespace: `Serilog`
- asset: runtime library
- rail: telemetry

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: logging family
- rail: telemetry

| [INDEX] | [SYMBOL]               | [PACKAGE_ROLE]       | [CAPABILITY]               |
| :-----: | :--------------------- | :------------------- | :------------------------- |
|   [1]   | `ILogger`              | contract surface     | defines boundary contract  |
|   [2]   | `LoggerConfiguration`  | telemetry surface    | emits structured signal    |
|   [3]   | `LogEvent`             | telemetry surface    | emits structured signal    |
|   [4]   | `LogEventLevel`        | telemetry surface    | emits structured signal    |
|   [5]   | `LogContext`           | operation context    | carries operation state    |
|   [6]   | `IDestructuringPolicy` | contract surface     | defines boundary contract  |
|   [7]   | `ScalarValue`          | scalar log value     | anchors telemetry contract |
|   [8]   | `StructureValue`       | structured log value | anchors telemetry contract |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: logging operations
- rail: telemetry

| [INDEX] | [SURFACE]      | [CALL_SHAPE]      | [CAPABILITY]            |
| :-----: | :------------- | :---------------- | :---------------------- |
|   [1]   | `WriteTo`      | operation call    | executes operation      |
|   [2]   | `Enrich`       | enricher chain    | adds log property       |
|   [3]   | `ForContext`   | context enricher  | scopes logger context   |
|   [4]   | `MinimumLevel` | level selector    | sets event floor        |
|   [5]   | `Information`  | logging extension | emits telemetry event   |
|   [6]   | `Warning`      | logging extension | emits telemetry event   |
|   [7]   | `Error`        | logging extension | emits telemetry event   |
|   [8]   | `Fatal`        | logging extension | emits telemetry event   |
|   [9]   | `PushProperty` | context property  | pushes ambient property |

## [4]-[IMPLEMENTATION_LAW]

[SERILOG_TOPOLOGY]:
- namespaces: `Serilog`, `Serilog.Core`, `Serilog.Events`, `Serilog.Context`, `Serilog.Configuration`
- event model: message template, properties, level, timestamp, exception
- configuration rails: minimum level, enrichment, destructuring, sinks, audit sinks, filters
- level control: fixed level and `LoggingLevelSwitch`
- context rail: `LogContext` pushes scoped properties through ambient context

[LOCAL_ADMISSION]:
- Serilog projects signal facts to structured logs.
- Destructuring policy must preserve redaction and bounded payload shape.
- Sink configuration belongs to bootstrap composition and never to lower runtime logic.

[RAIL_LAW]:
- Package: `Serilog`
- Owns: structured log projection
- Accept: Serilog projects telemetry outward
- Reject: runtime receipts as log strings
