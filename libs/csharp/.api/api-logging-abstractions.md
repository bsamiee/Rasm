# [RASM_API_LOGGING_ABSTRACTIONS]

`Microsoft.Extensions.Logging.Abstractions` mints the vendor-neutral emission contract every instrumented library binds: `ILogger.Log<TState>` is the one primitive every extension, generated method, and provider folds through, and `ILoggingBuilder` is the seat every activation extension extends. Its in-package Roslyn generator turns a `[LoggerMessage]` partial into allocation-free strongly-typed emission, and the external-scope seam carries ambient state from the emitting call to the provider that formats it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Logging.Abstractions`
- package: `Microsoft.Extensions.Logging.Abstractions` (MIT, Microsoft)
- assembly: `Microsoft.Extensions.Logging.Abstractions.dll`
- asset: runtime library; `Microsoft.Extensions.Logging.Generators.dll` is the shipped `[LoggerMessage]` Roslyn generator
- namespace: `Microsoft.Extensions.Logging`, `Microsoft.Extensions.Logging.Abstractions`
- rail: library-tier log emission behind every structured-log egress

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: emission contracts, the generated-method grammar, scope propagation, and the provider-side read models

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                                           |
| :-----: | :---------------------------- | :------------ | :----------------------------------------------------- |
|  [01]   | `ILogger`                     | interface     | sole emission surface every record crosses             |
|  [02]   | `ILogger<TCategoryName>`      | interface     | covariant category-typed seat the DI graph resolves    |
|  [03]   | `Logger<T>`                   | class         | concrete `ILogger<T>` adapter over an `ILoggerFactory` |
|  [04]   | `ILoggerFactory`              | interface     | category-keyed mint and provider registration          |
|  [05]   | `ILoggerProvider`             | interface     | provider seam a composition root implements            |
|  [06]   | `ILoggingBuilder`             | interface     | `Services` seat every activation extension extends     |
|  [07]   | `LogLevel`                    | enum          | `Trace` through `Critical`, with `None` as the off row |
|  [08]   | `EventId`                     | struct        | `Id`/`Name` identity; implicit lift from `int`         |
|  [09]   | `LoggerMessageAttribute`      | class         | declares the generated typed emission method           |
|  [10]   | `LoggerMessage`               | class         | cached-delegate `Define`/`DefineScope` factories       |
|  [11]   | `LogDefineOptions`            | class         | `SkipEnabledCheck` posture on the `Define` path        |
|  [12]   | `LoggerExtensions`            | class         | message-template `Log*` and `BeginScope` overloads     |
|  [13]   | `LoggerFactoryExtensions`     | class         | category derived from a type or type argument          |
|  [14]   | `IExternalScopeProvider`      | interface     | ambient frame stack a provider walks per record        |
|  [15]   | `ISupportExternalScope`       | interface     | provider opt-in to the shared frame stack              |
|  [16]   | `LoggerExternalScopeProvider` | class         | `AsyncLocal` frame stack backing that opt-in           |
|  [17]   | `ProviderAliasAttribute`      | class         | `Alias` a configuration section binds a provider by    |
|  [18]   | `LogEntry<TState>`            | struct        | level, category, event, state, exception, formatter    |
|  [19]   | `IBufferedLogger`             | interface     | batch-replay seam a buffering provider implements      |
|  [20]   | `BufferedLogRecord`           | class         | held record with trace, span, and thread identity      |

`NullLogger`, `NullLogger<T>`, `NullLoggerFactory`, and `NullLoggerProvider` each expose a static `Instance` — the zero-provider default at every seam a host has not bound.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the emission primitive, the typed and cached-delegate declarations, and the scope and replay seams

| [INDEX] | [SURFACE]                                                                                      | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :--------------------------------------------------------------------------------------------- | :------- | :--------------------------- |
|  [01]   | `ILogger.Log<TState>(LogLevel, EventId, TState, Exception?, Func<TState, Exception?, string>)` | instance | sole emission primitive      |
|  [02]   | `ILogger.IsEnabled(LogLevel)`                                                                  | instance | pre-payload level gate       |
|  [03]   | `ILogger.BeginScope<TState>(TState) -> IDisposable?`                                           | instance | ambient state span           |
|  [04]   | `LoggerMessageAttribute(int, LogLevel, string)`                                                | ctor     | declares a generated partial |
|  [05]   | `LoggerMessage.Define<T1..T6>(LogLevel, EventId, string, LogDefineOptions?)`                   | static   | cached delegate fallback     |
|  [06]   | `LoggerMessage.DefineScope<T1..T6>(string)`                                                    | static   | cached scope delegate        |
|  [07]   | `ILoggerFactory.CreateLogger(string)`                                                          | instance | category-keyed mint          |
|  [08]   | `ILoggerFactory.AddProvider(ILoggerProvider)`                                                  | instance | provider registration        |
|  [09]   | `LoggerFactoryExtensions.CreateLogger<T>(ILoggerFactory)`                                      | static   | type-derived category        |
|  [10]   | `ILoggerProvider.CreateLogger(string)`                                                         | instance | provider category mint       |
|  [11]   | `ISupportExternalScope.SetScopeProvider(IExternalScopeProvider)`                               | instance | seats the shared stack       |
|  [12]   | `IExternalScopeProvider.Push(object?)`                                                         | instance | pushes one frame             |
|  [13]   | `IExternalScopeProvider.ForEachScope<TState>(Action<object?, TState>, TState)`                 | instance | walks frames outermost-first |
|  [14]   | `IBufferedLogger.LogRecords(IEnumerable<BufferedLogRecord>)`                                   | instance | batch replay of held records |

- `LoggerMessageAttribute`: `EventId`, `EventName`, `Level`, `Message`, and `SkipEnabledCheck` are the settable rows the generator reads.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `ILogger.Log<TState>` is the single fold every extension, generated partial, and cached delegate lowers to.
- Severity, event identity, and message template are declaration data on `[LoggerMessage]`; the generator emits the `IsEnabled` gate ahead of payload construction unless `SkipEnabledCheck` waives it.
- Category identity rides `ILogger<T>` through the dependency graph, and `LoggerFactoryExtensions.CreateLogger<T>` derives that same category name from the type.
- Scope state crosses to a provider through `ISupportExternalScope.SetScopeProvider`, so one `LoggerExternalScopeProvider` stack serves every provider on the pipeline.

[STACKING]:
- `Microsoft.Extensions.Telemetry.Abstractions`(`api-extensions-telemetry.md`): `[LogProperties]`, `[TagProvider]`, and `[TagName]` ride the same generated `[LoggerMessage]` method, so emission grammar and tag classification land on one declaration; every governance verb extends `ILoggingBuilder`.
- `Microsoft.Extensions.Compliance.Redaction`(`api-redaction.md`): properties the generated method expands reach `Redactor` selection by classification before any provider observes the tag.
- `OpenTelemetry`(`api-opentelemetry.md`): `AddOpenTelemetry(ILoggingBuilder)` seats the OTel logger provider on this contract, and `OpenTelemetryLoggerOptions.IncludeScopes` reads `BeginScope` state as record attributes.
- `Rasm.AppHost` observability: `TelemetryIdentity` stamps scope identity, and the one `ILogger` pipeline fans to the Serilog and OTLP providers at composition.
- `Rasm.Rhino` `ObjectsTelemetry`: `Configure` admits one `(PluginKey, ILogger)` row per plugin and `Publish` fans generated events across the live rows, an empty roster composing `NullLogger`.

[LOCAL_ADMISSION]:
- Libraries reference this contract assembly alone and take `ILogger`/`ILoggerFactory` by injection; `NullLogger.Instance` is the default at every seam a host has not bound.
- Every `ILoggingBuilder` extension is composition-root surface, so provider, sampler, and buffer activation lands at the app root.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Logging.Abstractions`
- Owns: the library-tier emission contract, the generated typed-method grammar, and the scope-propagation seam
- Accept: `[LoggerMessage]` partials with banded `EventId` values; `LoggerMessage.Define`/`DefineScope` cached delegates where no partial hosts
- Reject: a static log facade or a second logging facade beside `ILogger`; provider, sink, and exporter types below a composition root; `LoggerExtensions.Log*` message-template calls where a generated method exists
