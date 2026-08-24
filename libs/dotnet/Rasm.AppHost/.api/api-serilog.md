# [RASM_APPHOST_API_SERILOG]

`Serilog` owns structured log projection: every runtime signal folds into one `LogEvent` that the `WriteTo` and `AuditTo` sink rails emit outward on the telemetry rail. Enrichers, filters, destructuring policies, and level switches shape each event at configuration time, while `LogContext` threads scoped properties into enrichment. This core owns the event model and its configuration rails; concrete sinks and host integration bind downstream.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Serilog`
- package: `Serilog`
- assembly: `Serilog`
- namespace: `Serilog`, `Serilog.Core`, `Serilog.Events`, `Serilog.Context`, `Serilog.Configuration`
- asset: runtime library
- rail: telemetry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: logger configuration family

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY]          | [CAPABILITY]            |
| :-----: | :--------------------------------- | :--------------------- | :---------------------- |
|  [01]   | `ILogger`                          | logger contract        | structured event emit   |
|  [02]   | `Log`                              | static logger          | process logger facade   |
|  [03]   | `LoggerConfiguration`              | configuration root     | logger construction     |
|  [04]   | `LoggerSinkConfiguration`          | sink configuration     | sink admission          |
|  [05]   | `LoggerAuditSinkConfiguration`     | audit configuration    | audit sink admission    |
|  [06]   | `LoggerEnrichmentConfiguration`    | enricher configuration | property enrichment     |
|  [07]   | `LoggerFilterConfiguration`        | filter configuration   | event filtering         |
|  [08]   | `LoggerDestructuringConfiguration` | destructuring setup    | payload shaping         |
|  [09]   | `LoggerMinimumLevelConfiguration`  | level setup            | event floor             |
|  [10]   | `LoggerSettingsConfiguration`      | settings setup         | external settings input |
|  [11]   | `LoggingLevelSwitch`               | level switch           | dynamic level floor     |
|  [12]   | `BatchingOptions`                  | batch policy           | batched sink behavior   |

[PUBLIC_TYPE_SCOPE]: event value and extension family

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]        | [CAPABILITY]            |
| :-----: | :------------------------ | :------------------- | :---------------------- |
|  [01]   | `LogEvent`                | event value          | structured event        |
|  [02]   | `LogEventLevel`           | level enum           | event severity          |
|  [03]   | `MessageTemplate`         | template value       | message shape           |
|  [04]   | `LogEventProperty`        | property value       | named event property    |
|  [05]   | `EventProperty`           | property pair        | property deconstruction |
|  [06]   | `ScalarValue`             | scalar property      | scalar payload          |
|  [07]   | `SequenceValue`           | sequence property    | array payload           |
|  [08]   | `StructureValue`          | structure property   | object payload          |
|  [09]   | `DictionaryValue`         | dictionary property  | map payload             |
|  [10]   | `IDestructuringPolicy`    | destructuring policy | object shaping          |
|  [11]   | `ILogEventEnricher`       | enricher contract    | event enrichment        |
|  [12]   | `ILogEventFilter`         | filter contract      | event filtering         |
|  [13]   | `ILogEventSink`           | sink contract        | event sink              |
|  [14]   | `IBatchedLogEventSink`    | batch sink contract  | batch sink              |
|  [15]   | `ILoggingFailureListener` | failure listener     | sink failure handling   |
|  [16]   | `LogContext`              | ambient context      | scoped properties       |
|  [17]   | `Matching`                | filter helpers       | property/source filters |
|  [18]   | `SelfLog`                 | internal diagnostic  | logger diagnostics      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: configuration and sink operations

| [INDEX] | [SURFACE]                              | [SHAPE]  | [CAPABILITY]              |
| :-----: | :------------------------------------- | :------- | :------------------------ |
|  [01]   | `WriteTo`                              | property | sink chain admission      |
|  [02]   | `AuditTo`                              | property | audit sink chain          |
|  [03]   | `Sink`                                 | instance | sink registration         |
|  [04]   | `Logger`                               | instance | sub-logger pipeline       |
|  [05]   | `Conditional`                          | instance | predicate-gated sink      |
|  [06]   | `FallbackChain`                        | instance | ordered fallback sinks    |
|  [07]   | `Enrich.With`                          | instance | enricher chain            |
|  [08]   | `Enrich.WithProperty`                  | instance | fixed property            |
|  [09]   | `Enrich.FromLogContext`                | instance | ambient properties        |
|  [10]   | `Filter.ByExcluding`                   | instance | event exclusion           |
|  [11]   | `Filter.ByIncludingOnly`               | instance | event inclusion           |
|  [12]   | `MinimumLevel.ControlledBy`            | instance | dynamic level floor       |
|  [13]   | `MinimumLevel.Override`                | instance | per-source level override |
|  [14]   | `Destructure.With`                     | instance | destructuring policy      |
|  [15]   | `Destructure.ToMaximumDepth`           | instance | depth bound               |
|  [16]   | `Destructure.ToMaximumStringLength`    | instance | string-length bound       |
|  [17]   | `Destructure.ToMaximumCollectionCount` | instance | collection-count bound    |
|  [18]   | `CreateLogger`                         | factory  | logger construction       |
|  [19]   | `Fallible`                             | instance | failure-listener wrap     |

- `Fallible(Action<LoggerSinkConfiguration>, ILoggingFailureListener)`: wraps the sink chain in a `FailureListenerSink`, so the listener observes every reported sink failure.

[ENTRYPOINT_SCOPE]: event context and formatting operations

| [INDEX] | [SURFACE]                 | [SHAPE]  | [CAPABILITY]              |
| :-----: | :------------------------ | :------- | :------------------------ |
|  [01]   | `Write`                   | instance | level-bound event         |
|  [02]   | `Verbose`                 | instance | verbose event             |
|  [03]   | `Debug`                   | instance | debug event               |
|  [04]   | `Information`             | instance | information event         |
|  [05]   | `Warning`                 | instance | warning event             |
|  [06]   | `Error`                   | instance | error event               |
|  [07]   | `Fatal`                   | instance | fatal event               |
|  [08]   | `ForContext`              | instance | property/source context   |
|  [09]   | `LogContext.PushProperty` | static   | scoped property           |
|  [10]   | `LogContext.Push`         | static   | scoped enricher           |
|  [11]   | `LogContext.Clone`        | static   | captured context          |
|  [12]   | `LogContext.Suspend`      | static   | temporary context removal |
|  [13]   | `BindMessageTemplate`     | instance | template/property bind    |
|  [14]   | `BindProperty`            | instance | property value bind       |
|  [15]   | `RenderMessage`           | instance | rendered message          |
|  [16]   | `MessageTemplate.Render`  | instance | rendered template         |
|  [17]   | `SelfLog.Enable`          | static   | internal error output     |
|  [18]   | `CloseAndFlush`           | static   | final sink flush          |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every event folds into one `LogEvent` — message template, typed properties, level, timestamp, exception, and trace/span ids — so a formatter renders identity from the template and properties, never from a pre-rendered string.
- `LoggerConfiguration` threads one fluent fold through its `MinimumLevel`, `Enrich`, `Destructure`, `Filter`, `WriteTo`, and `AuditTo` properties; each rail returns the builder so a whole config reads as one chain.
- `WriteTo` composes sinks by wrapping: `Sink` registers one, `Logger` nests a sub-pipeline, `Conditional` gates on a `LogEvent` predicate, `FallbackChain` orders alternates, and `Fallible` wraps the chain in failure observation.
- Level control resolves at three grains: a fixed floor, a per-source `MinimumLevel.Override`, and a runtime `LoggingLevelSwitch` that `ControlledBy` rebinds live.
- `LogContext` pushes scoped properties through ambient context, so an enricher reads request-scoped state without a domain handle.
- `SelfLog` and `ILoggingFailureListener` route sink failures to diagnostics; text, display, and JSON formatters render the emitted event.

[STACKING]:
- `Serilog.Sinks.Console` / `Serilog.Sinks.File`(`api-serilog-sinks.md`): both extend this package's `LoggerSinkConfiguration` with `WriteTo.Console`/`WriteTo.File` overloads, folding concrete sinks onto the rail this catalog owns.
- `Serilog.Extensions.Hosting`(`api-serilog-hosting.md`): `UseSerilog`/`AddSerilog` bind the constructed `ILogger` into `IHostBuilder`, and `CreateBootstrapLogger` returns a `ReloadableLogger` this pipeline reconfigures in place after services resolve.
- AppHost bootstrap: the composition root builds one `LoggerConfiguration`, folds every enrichment, destructuring, filter, and sink rail through it, and hands lower runtime logic an `ILogger` alone.

[LOCAL_ADMISSION]:
- Destructuring policy preserves redaction and bounded payload shape.
- Context properties are scoped projection metadata, never domain state.
- Sink failures emit diagnostics and never mutate runtime state.
- Sink configuration binds at bootstrap composition, never at lower runtime logic.

[RAIL_LAW]:
- Package: `Serilog`
- Owns: structured log projection
- Accept: telemetry projected outward as `LogEvent`s
- Reject: runtime receipts serialized as log strings
