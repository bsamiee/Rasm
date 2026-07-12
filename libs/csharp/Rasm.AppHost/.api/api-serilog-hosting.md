# [RASM_APPHOST_API_SERILOG_HOSTING]

`Serilog.Extensions.Hosting` supplies `IHostBuilder` and `IServiceCollection` integration for Serilog, a bootstrap `ReloadableLogger` pattern for two-stage initialization, a `DiagnosticContext` for per-request wide-event property accumulation, and `LoggerConfigurationExtensions.CreateBootstrapLogger` for host-lifecycle-aware logger construction.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Serilog.Extensions.Hosting`

- package: `Serilog.Extensions.Hosting`
- assembly: `Serilog.Extensions.Hosting`
- namespace: `Serilog`
- asset: runtime library
- rail: telemetry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: hosting integration family

- rail: telemetry

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY]            | [RAIL]                                  |
| :-----: | :----------------------------------- | :----------------------- | :-------------------------------------- |
|  [01]   | `SerilogHostBuilderExtensions`       | `IHostBuilder` ext       | `UseSerilog` overloads                  |
|  [02]   | `SerilogServiceCollectionExtensions` | `IServiceCollection` ext | `AddSerilog` overloads                  |
|  [03]   | `LoggerConfigurationExtensions`      | config ext               | `CreateBootstrapLogger` bootstrap entry |
|  [04]   | `IDiagnosticContext`                 | context contract         | per-scope property accumulation         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: host builder registration

- rail: telemetry

Host-builder registration has three overload families: static logger, host-context logger, and service-aware logger.

| [INDEX] | [SURFACE]    | [ENTRY_FAMILY] | [CONFIG_INPUT]                                     | [RAIL]                                  |
| :-----: | :----------- | :------------- | :------------------------------------------------- | :-------------------------------------- |
|  [01]   | `UseSerilog` | static logger  | pre-built `ILogger`                                | binds a pre-built `ILogger` to the host |
|  [02]   | `UseSerilog` | host-context   | `HostBuilderContext` + `LoggerConfiguration`       | builds logger from host context         |
|  [03]   | `UseSerilog` | service-aware  | `HostBuilderContext` + `IServiceProvider` + config | resolves services during configuration  |

[ENTRYPOINT_SCOPE]: service collection registration

- rail: telemetry

All overloads register an `ILoggerFactory`; the static form binds the supplied `ILogger`, while the delegate forms build the logger during registration.

| [INDEX] | [SURFACE]                                                                        | [ENTRY_FAMILY] |
| :-----: | :------------------------------------------------------------------------------- | :------------- |
|  [01]   | `AddSerilog(collection, logger?, dispose?, providers?)`                          | static logger  |
|  [02]   | `AddSerilog(collection, Action<LoggerConfiguration>, preserveStatic?, writeTo?)` | config action  |
|  [03]   | `AddSerilog(collection, Action<IServiceProvider, LoggerConfiguration>, ...)`     | service-aware  |

[ENTRYPOINT_SCOPE]: bootstrap and diagnostic context

- rail: telemetry

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY] | [RAIL]                                        |
| :-----: | :-------------------------------------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `LoggerConfiguration.CreateBootstrapLogger()`       | bootstrap      | returns `ReloadableLogger` for two-stage init |
|  [02]   | `IDiagnosticContext.Set(name, value, destructure?)` | property set   | accumulates property into the active context  |
|  [03]   | `IDiagnosticContext.SetException(exception)`        | exception set  | attaches exception to the active context      |

## [04]-[IMPLEMENTATION_LAW]

[SERILOG_HOSTING_TOPOLOGY]:

- namespaces: `Serilog` (6 types) plus internal `Serilog.Extensions.Hosting` (`ReloadableLogger`, `DiagnosticContext`)
- bootstrap pattern: call `LoggerConfiguration.CreateBootstrapLogger()` before `Host.CreateDefaultBuilder`; `UseSerilog` with the service-aware overload detects a `ReloadableLogger` at `Log.Logger` and reconfigures/freezes it in-place rather than replacing it
- `preserveStaticLogger = true`: leaves `Log.Logger` unchanged; `false` (default): replaces `Log.Logger` with the constructed logger
- `writeToProviders = true`: wraps `ILoggerProvider` registrations as Serilog sinks via `LoggerProviderCollection`
- `DiagnosticContext` is registered as both `DiagnosticContext` and `IDiagnosticContext` singletons by `AddSerilog`
- `dispose = true`: disposes the logger (or calls `Log.CloseAndFlush` for the static logger) when the host disposes the provider

[LOCAL_ADMISSION]:

- Use `CreateBootstrapLogger` for two-stage host initialization: a lightweight bootstrap logger active during host build, replaced by the fully configured logger after services resolve.
- Inject `IDiagnosticContext` to accumulate request-scoped properties for wide events such as request completion logs.
- Choose `UseSerilog(builder, configureLogger, ...)` with the `IServiceProvider` overload when the logger configuration needs resolved services (e.g., metrics sinks, sampler config).

[RAIL_LAW]:

- Package: `Serilog.Extensions.Hosting`
- Owns: Serilog integration into `IHostBuilder` and `IServiceCollection`
- Accept: `UseSerilog` or `AddSerilog` at composition; `CreateBootstrapLogger` for two-stage init
- Reject: manual `ILoggerFactory` replacement or static `Log.Logger` assignment outside composition bootstrap
