# [RASM_APPHOST_API_SERILOG_HOSTING]

`Serilog.Extensions.Hosting` binds Serilog into the Generic Host: `UseSerilog` and `AddSerilog` replace the host `ILoggerFactory` at composition, `CreateBootstrapLogger` mints a `ReloadableLogger` for two-stage init, and `IDiagnosticContext` accumulates request-scoped wide-event properties. Its boundary is bootstrap composition — the logger binds once through the host builder or service collection, never a runtime `Log.Logger` reassignment.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Serilog.Extensions.Hosting`
- package: `Serilog.Extensions.Hosting`
- assembly: `Serilog.Extensions.Hosting`
- namespace: `Serilog`, `Serilog.Extensions.Hosting`
- asset: runtime library
- rail: telemetry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: hosting integration family

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [CAPABILITY]                           |
| :-----: | :-------------------------------------- | :------------ | :------------------------------------- |
|  [01]   | `SerilogHostBuilderExtensions`          | class         | `UseSerilog` host-builder registration |
|  [02]   | `SerilogServiceCollectionExtensions`    | class         | `AddSerilog` service registration      |
|  [03]   | `LoggerConfigurationExtensions`         | class         | `CreateBootstrapLogger` factory        |
|  [04]   | `LoggerSettingsConfigurationExtensions` | class         | `ReadFrom.Services` DI settings input  |
|  [05]   | `IDiagnosticContext`                    | interface     | per-scope property accumulation        |
|  [06]   | `DiagnosticContext`                     | class         | diagnostic-context singleton           |
|  [07]   | `ReloadableLogger`                      | class         | reconfigurable bootstrap logger        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: host and service registration

| [INDEX] | [SURFACE]                                                                                   | [SHAPE] | [CAPABILITY]                   |
| :-----: | :------------------------------------------------------------------------------------------ | :------ | :----------------------------- |
|  [01]   | `UseSerilog(ILogger, bool, LoggerProviderCollection)`                                       | static  | bind the pre-built logger      |
|  [02]   | `UseSerilog(Action<HostBuilderContext, LoggerConfiguration>, bool, bool)`                   | static  | build from host context        |
|  [03]   | `UseSerilog(Action<HostBuilderContext, IServiceProvider, LoggerConfiguration>, bool, bool)` | static  | build with resolved services   |
|  [04]   | `AddSerilog(ILogger, bool, LoggerProviderCollection)`                                       | static  | register the pre-built factory |
|  [05]   | `AddSerilog(Action<LoggerConfiguration>, bool, bool)`                                       | static  | build during registration      |
|  [06]   | `AddSerilog(Action<IServiceProvider, LoggerConfiguration>, bool, bool)`                     | static  | build with resolved services   |

[ENTRYPOINT_SCOPE]: bootstrap, diagnostic context, and DI settings

| [INDEX] | [SURFACE]                                                                 | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :------------------------------------------------------------------------ | :------- | :-------------------------------------------- |
|  [01]   | `LoggerConfiguration.CreateBootstrapLogger() -> ReloadableLogger`         | static   | mint a reloadable logger for two-stage init   |
|  [02]   | `ReloadableLogger.Reload(Func<LoggerConfiguration, LoggerConfiguration>)` | instance | reconfigure the bootstrap logger in place     |
|  [03]   | `ReloadableLogger.Freeze()`                                               | instance | freeze reconfiguration after services resolve |
|  [04]   | `IDiagnosticContext.Set(string, object, bool)`                            | instance | accumulate a property on the active context   |
|  [05]   | `IDiagnosticContext.SetException(Exception)`                              | instance | attach an exception to the active context     |
|  [06]   | `ReadFrom.Services(IServiceProvider) -> LoggerConfiguration`              | static   | inject DI-resolved logger settings            |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `CreateBootstrapLogger` mints a `ReloadableLogger` before `Host.CreateApplicationBuilder`; both service-aware overloads — `UseSerilog` and `AddSerilog` alike — read `Log.Logger as ReloadableLogger` and, unless `preserveStaticLogger` is set, `Reload` then `Freeze` that instance in place rather than replacing it. The detection reads the STATIC slot, so a bootstrap logger the caller never assigned to `Log.Logger` takes the replace branch and its boot-window records land on an abandoned logger.
- `preserveStaticLogger` leaves `Log.Logger` untouched; the default replaces it with the constructed logger.
- `writeToProviders` wraps every `ILoggerProvider` registration as a Serilog sink through `LoggerProviderCollection`.
- `AddSerilog` owns that reload-and-freeze body outright — `UseSerilog`'s service-aware overload is an `IHostBuilder` shim whose `ConfigureServices` calls it — so the two spellings share one implementation and never diverge.
- `AddSerilog` registers the concrete instance as both the `DiagnosticContext` and `IDiagnosticContext` singleton.
- `dispose` disposes the logger, or calls `Log.CloseAndFlush` for the static logger, when the host disposes the provider.

[STACKING]:
- `api-serilog`(`.api/api-serilog.md`): `UseSerilog`/`AddSerilog` build their logger from a `LoggerConfiguration`, and `CreateBootstrapLogger` wraps a `Serilog` `Logger` as the reloadable bootstrap instance, binding the core pipeline into the host.
- `api-hosting`(`.api/api-hosting.md`): `UseSerilog` extends `IHostBuilder` and its `ConfigureServices` folds `AddSerilog` onto the host `IServiceCollection`, replacing the default `ILoggerFactory`.
- `api-di`(`.api/api-di.md`): `AddSerilog` mints the `ILoggerFactory` and the `DiagnosticContext`/`IDiagnosticContext` singletons as `ServiceDescriptor` rows, and `ReadFrom.Services` pulls `ILogEventEnricher`, `ILogEventSink`, and `LoggingLevelSwitch` from the resolved `IServiceProvider`.
- within-lib: `Observability/telemetry#LOG_PROJECTION` `SerilogHost.Boot` calls `CreateBootstrapLogger` before the host builder and seats the result at `Log.Logger` — the one sanctioned write to that slot, since the bridge's reconfigure-in-place path reads it — then `SerilogHost.Compose` runs the service-aware `AddSerilog(IServiceCollection, Action<IServiceProvider, LoggerConfiguration>)` bridge, which reloads that logger with the DI-resolved sink set and freezes it once the provider is built; `UseSerilog` is the `IHostBuilder`-era spelling no AppHost fence composes.

[LOCAL_ADMISSION]:
- `CreateBootstrapLogger` roots two-stage host init: a lightweight logger runs during host build, replaced by the fully configured logger after services resolve.
- Inject `IDiagnosticContext` to accumulate request-scoped properties for wide events such as request-completion logs.
- `AddSerilog`'s service-aware overload owns any configuration needing resolved services, such as metrics sinks or sampler config.

[RAIL_LAW]:
- Package: `Serilog.Extensions.Hosting`
- Owns: Serilog integration into `IHostBuilder` and `IServiceCollection`
- Accept: `AddSerilog` at composition; `CreateBootstrapLogger` for two-stage init
- Reject: manual `ILoggerFactory` replacement, static `Log.Logger` assignment outside bootstrap, and the `IHostBuilder`-era `UseSerilog` shim beside the `IServiceCollection` bridge
