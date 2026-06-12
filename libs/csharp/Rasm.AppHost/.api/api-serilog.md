# [RASM_APPHOST_API_SERILOG]

`Serilog` supplies structured event construction, enrichment, sinks, audit sinks,
filters, batching, levels, message templates, property values, failure listeners,
formatters, and log context for projection.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Serilog`
- package: `Serilog`
- assembly: `Serilog`
- namespace: `Serilog`
- asset: runtime library
- rail: telemetry

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: logger configuration family
- rail: telemetry

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY]          | [RAIL]                  |
| :-----: | :--------------------------------- | :--------------------- | :---------------------- |
|   [1]   | `ILogger`                          | logger contract        | structured event emit   |
|   [2]   | `Log`                              | static logger          | process logger facade   |
|   [3]   | `LoggerConfiguration`              | configuration root     | logger construction     |
|   [4]   | `LoggerSinkConfiguration`          | sink configuration     | sink admission          |
|   [5]   | `LoggerAuditSinkConfiguration`     | audit configuration    | audit sink admission    |
|   [6]   | `LoggerEnrichmentConfiguration`    | enricher configuration | property enrichment     |
|   [7]   | `LoggerFilterConfiguration`        | filter configuration   | event filtering         |
|   [8]   | `LoggerDestructuringConfiguration` | destructuring setup    | payload shaping         |
|   [9]   | `LoggerMinimumLevelConfiguration`  | level setup            | event floor             |
|  [10]   | `LoggerSettingsConfiguration`      | settings setup         | external settings input |
|  [11]   | `LoggingLevelSwitch`               | level switch           | dynamic level floor     |
|  [12]   | `BatchingOptions`                  | batch policy           | batched sink behavior   |

[PUBLIC_TYPE_SCOPE]: event value and extension family
- rail: telemetry

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]        | [RAIL]                  |
| :-----: | :------------------------ | :------------------- | :---------------------- |
|   [1]   | `LogEvent`                | event value          | structured event        |
|   [2]   | `LogEventLevel`           | level enum           | event severity          |
|   [3]   | `MessageTemplate`         | template value       | message shape           |
|   [4]   | `LogEventProperty`        | property value       | named event property    |
|   [5]   | `EventProperty`           | property pair        | property deconstruction |
|   [6]   | `ScalarValue`             | scalar property      | scalar payload          |
|   [7]   | `SequenceValue`           | sequence property    | array payload           |
|   [8]   | `StructureValue`          | structure property   | object payload          |
|   [9]   | `DictionaryValue`         | dictionary property  | map payload             |
|  [10]   | `IDestructuringPolicy`    | destructuring policy | object shaping          |
|  [11]   | `ILogEventEnricher`       | enricher contract    | event enrichment        |
|  [12]   | `ILogEventFilter`         | filter contract      | event filtering         |
|  [13]   | `ILogEventSink`           | sink contract        | event sink              |
|  [14]   | `IBatchedLogEventSink`    | batch sink contract  | batch sink              |
|  [15]   | `ILoggingFailureListener` | failure listener     | sink failure handling   |
|  [16]   | `LogContext`              | ambient context      | scoped properties       |
|  [17]   | `Matching`                | filter helpers       | property/source filters |
|  [18]   | `SelfLog`                 | internal diagnostic  | logger diagnostics      |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: logging operations
- rail: telemetry

| [INDEX] | [SURFACE]                    | [ENTRY_FAMILY]       | [RAIL]                   |
| :-----: | :--------------------------- | :------------------- | :----------------------- |
|   [1]   | `WriteTo`                    | sink chain           | sink configuration       |
|   [2]   | `AuditTo`                    | audit sink chain     | audit sink configuration |
|   [3]   | `Sink`                       | sink registration    | sink admission           |
|   [4]   | `Logger`                     | sub-logger setup     | sub-pipeline admission   |
|   [5]   | `Conditional`                | sink predicate       | conditional sink         |
|   [6]   | `FallbackChain`              | sink fallback        | fallback sinks           |
|   [7]   | `Enrich.With`                | enricher admission   | enricher chain           |
|   [8]   | `Enrich.WithProperty`        | property enrichment  | fixed property           |
|   [9]   | `Enrich.FromLogContext`      | context enrichment   | ambient properties       |
|  [10]   | `Filter.ByExcluding`         | filter predicate     | event exclusion          |
|  [11]   | `Filter.ByIncludingOnly`     | filter predicate     | event inclusion          |
|  [12]   | `MinimumLevel.ControlledBy`  | level switch         | dynamic level floor      |
|  [13]   | `MinimumLevel.Override`      | source override      | category level override  |
|  [14]   | `Destructure.With`           | destructuring policy | payload shaping          |
|  [15]   | `Destructure.ToMaximumDepth` | destructuring limit  | depth bound              |
|  [16]   | `CreateLogger`               | logger factory       | logger construction      |

[ENTRYPOINT_SCOPE]: event context and formatting operations
- rail: telemetry

| [INDEX] | [SURFACE]                 | [ENTRY_FAMILY]     | [RAIL]                    |
| :-----: | :------------------------ | :----------------- | :------------------------ |
|   [1]   | `Write`                   | event emission     | level-bound event         |
|   [2]   | `Verbose`                 | event emission     | verbose event             |
|   [3]   | `Debug`                   | event emission     | debug event               |
|   [4]   | `Information`             | event emission     | information event         |
|   [5]   | `Warning`                 | event emission     | warning event             |
|   [6]   | `Error`                   | event emission     | error event               |
|   [7]   | `Fatal`                   | event emission     | fatal event               |
|   [8]   | `ForContext`              | context logger     | property/source context   |
|   [9]   | `LogContext.PushProperty` | ambient context    | scoped property           |
|  [10]   | `LogContext.Push`         | ambient context    | scoped enricher           |
|  [11]   | `LogContext.Clone`        | ambient context    | captured context          |
|  [12]   | `LogContext.Suspend`      | ambient context    | temporary context removal |
|  [13]   | `BindMessageTemplate`     | template binding   | template/property bind    |
|  [14]   | `BindProperty`            | property binding   | property value bind       |
|  [15]   | `RenderMessage`           | event rendering    | rendered message          |
|  [16]   | `MessageTemplate.Render`  | template rendering | rendered template         |
|  [17]   | `SelfLog.Enable`          | self diagnostic    | internal error output     |
|  [18]   | `CloseAndFlush`           | logger lifecycle   | final sink flush          |

## [4]-[IMPLEMENTATION_LAW]

[SERILOG_TOPOLOGY]:
- namespaces: `Serilog`, `Serilog.Core`, `Serilog.Events`, `Serilog.Context`, `Serilog.Configuration`
- event model: message template, properties, level, timestamp, exception
- trace model: event trace id and span id fields
- configuration rails: minimum level, enrichment, destructuring, sinks, audit sinks, filters
- sink rails: sink, batched sink, sub-logger, conditional sink, fallback chain
- level control: fixed level, source override, `LoggingLevelSwitch`
- context rail: `LogContext` pushes scoped properties through ambient context
- failure rail: logging failure listener, failure kind, self log
- formatter rail: text formatter, display formatter, JSON formatter, JSON value formatter

[LOCAL_ADMISSION]:
- Serilog projects signal facts to structured logs.
- Destructuring policy must preserve redaction and bounded payload shape.
- Sink failures emit diagnostics and do not mutate runtime state.
- Context properties are scoped projection metadata, not domain state.
- Sink configuration belongs to bootstrap composition and never to lower runtime logic.

[RAIL_LAW]:
- Package: `Serilog`
- Owns: structured log projection
- Accept: Serilog projects telemetry outward
- Reject: runtime receipts as log strings
