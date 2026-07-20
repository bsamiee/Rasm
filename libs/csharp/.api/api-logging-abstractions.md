# [RASM_API_LOGGING_ABSTRACTIONS]

`Microsoft.Extensions.Logging.Abstractions` is the vendor-neutral log-emission contract every instrumented library references: `ILogger` and its category-generic twin carry every record, the `[LoggerMessage]` source generator mints allocation-free strongly-typed log methods over them, and the null objects give tests and headless boots a zero-provider default. Libraries hold this contract assembly alone; provider binding, sinks, and exporters compose at app roots.

## [01]-[PACKAGE_SURFACE]

- Package: `Microsoft.Extensions.Logging.Abstractions`
- License: MIT
- Namespace: `Microsoft.Extensions.Logging`, `Microsoft.Extensions.Logging.Abstractions`
- Asset: `Microsoft.Extensions.Logging.Abstractions.dll` + the Microsoft.Gen.Logging analyzer/generator asset
- Rail: library-tier log emission behind every structured-log egress

## [02]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]                 | [KIND]      | [CAPABILITY]                                                    |
| :-----: | :----------------------- | :---------- | :-------------------------------------------------------------- |
|  [01]   | `ILogger`                | contract    | the one emission surface every record crosses                   |
|  [02]   | `ILogger<TCategoryName>` | contract    | category-typed emission the DI graph resolves per owner         |
|  [03]   | `ILoggerFactory`         | factory     | category-keyed logger mint; app-root provider composition       |
|  [04]   | `ILoggerProvider`        | contract    | provider seam the composition root implements, never a library  |
|  [05]   | `LogLevel`               | enum        | severity vocabulary carried as data                             |
|  [06]   | `EventId`                | value       | banded event identity every generated method stamps             |
|  [07]   | `LoggerMessageAttribute` | attribute   | source-generated strongly-typed log method declaration          |
|  [08]   | `LogDefineOptions`       | options     | `SkipEnabledCheck` posture on the `Define` fallback             |
|  [09]   | `NullLogger`             | null object | zero-provider default at library seams                          |
|  [10]   | `NullLogger<T>`          | null object | category-typed zero-provider default                            |
|  [11]   | `NullLoggerFactory`      | null object | factory default for headless and test boots                     |

## [03]-[ENTRYPOINTS]

| [INDEX] | [SURFACE]                                              | [KIND]    | [CAPABILITY]                                          |
| :-----: | :----------------------------------------------------- | :-------- | :---------------------------------------------------- |
|  [01]   | `[LoggerMessage(EventId, Level, Message)]` partial     | generator | allocation-free typed emission; the canonical form    |
|  [02]   | `LoggerMessage.Define<T...>(level, eventId, format)`   | fallback  | cached delegate emission where a partial cannot host  |
|  [03]   | `ILogger.IsEnabled(LogLevel)`                          | gate      | pre-payload guard the generated methods already carry |
|  [04]   | `ILogger.BeginScope<TState>(TState)`                   | scope     | ambient-state span over enclosed records              |
|  [05]   | `NullLogger.Instance` / `NullLogger<T>.Instance`       | default   | the injected default before a provider binds          |

## [04]-[IMPLEMENTATION_LAW]

[LOGGING_TOPOLOGY]:
- Libraries emit through `ILogger` alone — provider packages, sinks, and the OTel logger bridge are composition-root pins.
- Every emission is a generated `[LoggerMessage]` partial with a banded `EventId`; severity is data on the declaration, never a call-site branch.
- Category identity rides `ILogger<T>` through DI; a hand-built category string is the deleted form.

[STACKING]:
- `Microsoft.Extensions.Telemetry.Abstractions` (`.api/api-extensions-telemetry.md`): `[LogProperties]` and `[TagProvider]` ride the same generated `[LoggerMessage]` methods, so emission grammar and classification stack on one declaration.
- `Rasm.AppHost` `Observability/telemetry#LOG_PROJECTION`: the one `ILogger` pipeline fans to the Serilog and OTLP providers at composition; `TelemetryIdentity` stamps the matching scope identity.
- `Rasm.Rhino` `Objects/authoring`: `ObjectsTelemetry` is the host structured-log egress emitting through the generated methods with classified tags.

[LOCAL_ADMISSION]:
- A library references the contract assembly alone and takes `ILogger`/`ILoggerFactory` by injection; `NullLogger` is the default at every seam a host has not bound.
- Reject: a static log facade, Serilog types below a composition root, string-interpolated `Log*` calls where a generated method exists.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Logging.Abstractions`
- Owns: the library-tier log-emission contract and the generated typed-method grammar
- Accept: `[LoggerMessage]` partials with banded `EventId` values; `LoggerMessage.Define` where a partial cannot host
- Reject: provider or sink types below app roots; a second logging facade beside `ILogger`
