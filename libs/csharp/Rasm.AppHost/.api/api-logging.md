# [RASM_APPHOST_API_LOGGING]

`Microsoft.Extensions.Logging.Abstractions` owns the structured-logging contract every runtime port binds and every provider implements, keying each event to a stable `EventId` through source-generated delegates. This package holds contracts and the source generator alone; sink and formatting policy bind in a provider package downstream.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Logging.Abstractions`
- package: `Microsoft.Extensions.Logging.Abstractions`
- assembly: `Microsoft.Extensions.Logging.Abstractions`
- namespace: `Microsoft.Extensions.Logging`
- asset: runtime library and analyzer assets
- rail: telemetry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: logging contracts

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]       | [CAPABILITY]          |
| :-----: | :----------------------- | :------------------ | :-------------------- |
|  [01]   | `ILogger`                | logger contract     | typed event emission  |
|  [02]   | `ILogger<TCategoryName>` | category logger     | category-bound events |
|  [03]   | `ILoggerFactory`         | logger factory      | category construction |
|  [04]   | `ILoggerProvider`        | provider contract   | sink admission        |
|  [05]   | `ILoggingBuilder`        | builder contract    | provider registration |
|  [06]   | `IExternalScopeProvider` | scope provider      | ambient scope flow    |
|  [07]   | `ISupportExternalScope`  | provider capability | provider scope hookup |
|  [08]   | `EventId`                | event identity      | stable event key      |
|  [09]   | `LogLevel`               | severity enum       | event severity        |

[PUBLIC_TYPE_SCOPE]: generated and buffered family

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]        | [CAPABILITY]            |
| :-----: | :-------------------------- | :------------------- | :---------------------- |
|  [01]   | `LoggerMessage`             | delegate factory     | precompiled log methods |
|  [02]   | `LoggerMessageAttribute`    | generator attribute  | generated log methods   |
|  [03]   | `LogDefineOptions`          | delegate option      | enabled-check policy    |
|  [04]   | `ProviderAliasAttribute`    | provider attribute   | provider naming         |
|  [05]   | `Logger<T>`                 | category adapter     | generic logger wrapper  |
|  [06]   | `LogEntry<TState>`          | log entry value      | provider payload        |
|  [07]   | `BufferedLogRecord`         | buffered value       | batch payload           |
|  [08]   | `IBufferedLogger`           | batch contract       | batch emission          |
|  [09]   | `NullLogger`                | null logger          | no-op logger            |
|  [10]   | `NullLogger<T>`             | null category logger | no-op category logger   |
|  [11]   | `NullLoggerFactory`         | null factory         | no-op factory           |
|  [12]   | `NullLoggerProvider`        | null provider        | no-op provider          |
|  [13]   | `NullExternalScopeProvider` | null scope provider  | no-op scope flow        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: event and scope operations

| [INDEX] | [SURFACE]                     | [SHAPE]  | [CAPABILITY]            |
| :-----: | :---------------------------- | :------- | :---------------------- |
|  [01]   | `ILogger.Log<TState>`         | instance | structured event emit   |
|  [02]   | `IsEnabled`                   | instance | guarded event creation  |
|  [03]   | `BeginScope<TState>`          | instance | scoped event context    |
|  [04]   | `IExternalScopeProvider.Push` | instance | provider scope stack    |
|  [05]   | `ForEachScope`                | instance | scope projection        |
|  [06]   | `LogTrace`                    | static   | trace event             |
|  [07]   | `LogDebug`                    | static   | debug event             |
|  [08]   | `LogInformation`              | static   | information event       |
|  [09]   | `LogWarning`                  | static   | warning event           |
|  [10]   | `LogError`                    | static   | error event             |
|  [11]   | `LogCritical`                 | static   | critical event          |
|  [12]   | `CreateLogger<T>`             | static   | category logger         |
|  [13]   | `CreateLogger(Type)`          | static   | runtime category logger |

[ENTRYPOINT_SCOPE]: generated and batch operations

| [INDEX] | [SURFACE]                      | [SHAPE]  | [CAPABILITY]             |
| :-----: | :----------------------------- | :------- | :----------------------- |
|  [01]   | `LoggerMessage.Define`         | static   | cached event delegate    |
|  [02]   | `LoggerMessage.Define<T1..T6>` | static   | typed template delegate  |
|  [03]   | `LoggerMessage.DefineScope`    | static   | cached scope delegate    |
|  [04]   | `LoggerMessageAttribute`       | ctor     | generated partial method |
|  [05]   | `SkipEnabledCheck`             | property | explicit guard policy    |
|  [06]   | `EventName`                    | property | event name payload       |
|  [07]   | `ProviderAliasAttribute`       | ctor     | provider alias payload   |
|  [08]   | `IBufferedLogger.LogRecords`   | instance | buffered event flush     |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every emission folds through `ILogger.Log<TState>`: a `TState` payload and a `Func<TState,Exception?,string>` formatter render one message keyed by `EventId`, so severity extensions, `LoggerMessage` delegates, and generated methods share one call shape and a string message never defines identity.
- `IsEnabled(LogLevel)` guards payload construction ahead of `Log`; a generated `[LoggerMessage]` method and a `LoggerMessage.Define` delegate both skip allocation on a disabled level, `unless` `SkipEnabledCheck` or `LogDefineOptions.SkipEnabledCheck` forces the call.
- Source generation is the hot path: `[LoggerMessage]` on a partial method emits a cached `LoggerMessage.Define<T0..T5>` delegate over zero through six template values, retiring per-call boxing and format parsing.
- `IExternalScopeProvider.Push` and `ForEachScope` thread ambient scope through every provider; a provider opts into the shared stack through `ISupportExternalScope`.
- `IBufferedLogger.LogRecords` replays a `BufferedLogRecord` batch off the hot path, each record carrying timestamp, level, event id, exception, activity and thread ids, formatted message, template, and attributes.
- `NullLogger`, `NullLoggerFactory`, `NullLoggerProvider`, and `NullExternalScopeProvider` bind the no-op sink a disabled rail or a test uses without a live provider.

[STACKING]:
- `Microsoft.Extensions.Telemetry.Abstractions`(`api-telemetry-abstractions.md`): its `[LogProperties]` and `[TagProvider]` source-generated methods marshal tags through `LoggerMessageHelper` into this package's `ILogger.Log<TState>`, and `LoggingSampler.ShouldSample(in LogEntry<TState>)` reads this package's `LogEntry<TState>` off one shared delegate cache.
- `Microsoft.Extensions.Hosting`(`api-hosting.md`): `ConfigureLogging` admits `ILoggingBuilder`, and the built `IHost` resolves `ILoggerFactory` and `ILogger<T>` into hosted services and runtime ports.
- `Serilog.Extensions.Hosting`(`api-serilog-hosting.md`): the Serilog provider implements `ILoggerProvider` and `ISupportExternalScope`, and `writeToProviders` wraps registered `ILoggerProvider` sinks, so this package's contracts are the sink boundary every provider binds behind.
- AppHost runtime ports: every port takes an injected `ILogger<T>` and emits through `[LoggerMessage]` partial methods guarded by `IsEnabled`, allocating a payload only when a level is enabled.

[LOCAL_ADMISSION]:
- Runtime ports emit through generated or precompiled `LoggerMessage` delegates on hot paths.
- `ProviderAliasAttribute` names a provider for telemetry projection; `EventId` carries stable, explicit event identity.
- Scopes carry typed context and buffered records are projection material; neither replaces a runtime receipt.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Logging.Abstractions`
- Owns: structured log contracts and event identity
- Accept: logger handles enter runtime ports
- Reject: ambient static logging
