# [RASM_APPHOST_API_LOGGING]

`Microsoft.Extensions.Logging.Abstractions` supplies logging contracts, category identity,
scope identity, event identity, buffered records, null implementations, provider aliases,
and generated logger delegates.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Logging.Abstractions`
- package: `Microsoft.Extensions.Logging.Abstractions`
- assembly: `Microsoft.Extensions.Logging.Abstractions`
- namespace: `Microsoft.Extensions.Logging`
- asset: runtime library and analyzer assets
- rail: telemetry

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: logging contracts
- rail: telemetry

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]       | [RAIL]                |
| :-----: | :----------------------- | :------------------ | :-------------------- |
|   [1]   | `ILogger`                | logger contract     | typed event emission  |
|   [2]   | `ILogger<TCategoryName>` | category logger     | category-bound events |
|   [3]   | `ILoggerFactory`         | logger factory      | category construction |
|   [4]   | `ILoggerProvider`        | provider contract   | sink admission        |
|   [5]   | `ILoggingBuilder`        | builder contract    | provider registration |
|   [6]   | `IExternalScopeProvider` | scope provider      | ambient scope flow    |
|   [7]   | `ISupportExternalScope`  | provider capability | provider scope hookup |
|   [8]   | `EventId`                | event identity      | stable event key      |
|   [9]   | `LogLevel`               | severity enum       | event severity        |

[PUBLIC_TYPE_SCOPE]: generated and buffered family
- rail: telemetry

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]        | [RAIL]                  |
| :-----: | :-------------------------- | :------------------- | :---------------------- |
|   [1]   | `LoggerMessage`             | delegate factory     | precompiled log methods |
|   [2]   | `LoggerMessageAttribute`    | generator attribute  | generated log methods   |
|   [3]   | `LogDefineOptions`          | delegate option      | enabled-check policy    |
|   [4]   | `ProviderAliasAttribute`    | provider attribute   | provider naming         |
|   [5]   | `Logger<T>`                 | category adapter     | generic logger wrapper  |
|   [6]   | `LogEntry<TState>`          | log entry value      | provider payload        |
|   [7]   | `BufferedLogRecord`         | buffered value       | batch payload           |
|   [8]   | `IBufferedLogger`           | batch contract       | batch emission          |
|   [9]   | `NullLogger`                | null logger          | no-op logger            |
|  [10]   | `NullLogger<T>`             | null category logger | no-op category logger   |
|  [11]   | `NullLoggerFactory`         | null factory         | no-op factory           |
|  [12]   | `NullLoggerProvider`        | null provider        | no-op provider          |
|  [13]   | `NullExternalScopeProvider` | null scope provider  | no-op scope flow        |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: event and scope operations
- rail: telemetry

| [INDEX] | [SURFACE]                     | [ENTRY_FAMILY]       | [RAIL]                  |
| :-----: | :---------------------------- | :------------------- | :---------------------- |
|   [1]   | `ILogger.Log<TState>`         | core event method    | structured event emit   |
|   [2]   | `IsEnabled`                   | enablement predicate | guarded event creation  |
|   [3]   | `BeginScope<TState>`          | logger scope         | scoped event context    |
|   [4]   | `IExternalScopeProvider.Push` | provider scope       | provider scope stack    |
|   [5]   | `ForEachScope`                | provider scope read  | scope projection        |
|   [6]   | `LogTrace`                    | severity extension   | trace event             |
|   [7]   | `LogDebug`                    | severity extension   | debug event             |
|   [8]   | `LogInformation`              | severity extension   | information event       |
|   [9]   | `LogWarning`                  | severity extension   | warning event           |
|  [10]   | `LogError`                    | severity extension   | error event             |
|  [11]   | `LogCritical`                 | severity extension   | critical event          |
|  [12]   | `CreateLogger<T>`             | factory extension    | category logger         |
|  [13]   | `CreateLogger(Type)`          | factory extension    | runtime category logger |

[ENTRYPOINT_SCOPE]: generated and batch operations
- rail: telemetry

| [INDEX] | [SURFACE]                      | [ENTRY_FAMILY]     | [RAIL]                   |
| :-----: | :----------------------------- | :----------------- | :----------------------- |
|   [1]   | `LoggerMessage.Define`         | compiled delegate  | cached event delegate    |
|   [2]   | `LoggerMessage.Define<T1..T6>` | compiled delegate  | typed template delegate  |
|   [3]   | `LoggerMessage.DefineScope`    | compiled scope     | cached scope delegate    |
|   [4]   | `LoggerMessageAttribute`       | generator marker   | generated partial method |
|   [5]   | `SkipEnabledCheck`             | generator option   | explicit guard policy    |
|   [6]   | `EventName`                    | generator metadata | event name payload       |
|   [7]   | `ProviderAliasAttribute`       | provider metadata  | provider alias payload   |
|   [8]   | `IBufferedLogger.LogRecords`   | batch emission     | buffered event flush     |

## [4]-[IMPLEMENTATION_LAW]

[LOGGING_TOPOLOGY]:
- namespaces: `Microsoft.Extensions.Logging`, `Microsoft.Extensions.Logging.Abstractions`
- generator assets: logging generator analyzers under package analyzers
- generated shape: partial methods marked with `LoggerMessageAttribute`
- generator payload: event id, event name, level, message template, skip-enabled-check policy
- delegate arity: `LoggerMessage.Define` and `DefineScope` admit zero through six template values
- structured state: category, message template, event id, severity, exception, scope, state, formatter
- buffered shape: timestamp, level, event id, exception, activity ids, thread id, formatted message, template, attributes
- null implementations: null logger, null typed logger, null factory, null provider, null scope provider

[LOCAL_ADMISSION]:
- Logger methods are generated or precompiled delegates on hot paths.
- Provider aliases are explicit package-facing names for telemetry projection.
- Event identity is explicit and stable; string messages never define event identity.
- Scopes carry typed context and do not replace runtime receipts.
- Buffered logging is projection surface material and does not replace runtime receipts.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Logging.Abstractions`
- Owns: structured log contracts and event identity
- Accept: logger handles enter runtime ports
- Reject: ambient static logging
