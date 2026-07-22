# [RASM_APPHOST_API_HOSTING]

`Microsoft.Extensions.Hosting` roots the AppHost process: `Host.CreateApplicationBuilder` mints one `HostApplicationBuilder` whose configuration, service, logging, and metrics rails `Build` freezes into an `IHost` that runs every `IHostedService` through one start/stop lifecycle. Its boundary is bootstrap — a hosted service adapts an external integration lifetime into runtime state transitions, never a second bootstrap framework.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Hosting`
- package: `Microsoft.Extensions.Hosting`
- assembly: `Microsoft.Extensions.Hosting`
- contract_assembly: `Microsoft.Extensions.Hosting.Abstractions`
- namespace: `Microsoft.Extensions.Hosting`
- asset: runtime library
- rail: bootstrap

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: host implementation family

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY]    | [CAPABILITY]         |
| :-----: | :----------------------------------- | :--------------- | :------------------- |
|  [01]   | `Host`                               | host facade      | builder entry        |
|  [02]   | `HostApplicationBuilder`             | app builder      | modern host root     |
|  [03]   | `HostApplicationBuilderSettings`     | builder settings | default policy input |
|  [04]   | `HostBuilder`                        | host builder     | staged host root     |
|  [05]   | `HostOptions`                        | host policy      | lifetime policy      |
|  [06]   | `ConsoleLifetimeOptions`             | console policy   | console lifetime     |
|  [07]   | `BackgroundServiceExceptionBehavior` | exception policy | hosted loop failure  |

[PUBLIC_TYPE_SCOPE]: host contract family

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]        | [CAPABILITY]            |
| :-----: | :------------------------- | :------------------- | :---------------------- |
|  [01]   | `IHost`                    | host contract        | runtime root            |
|  [02]   | `IHostBuilder`             | builder contract     | staged composition      |
|  [03]   | `IHostApplicationBuilder`  | app builder contract | direct composition      |
|  [04]   | `IHostedService`           | service contract     | start/stop participant  |
|  [05]   | `IHostedLifecycleService`  | lifecycle contract   | ordered lifecycle hooks |
|  [06]   | `BackgroundService`        | hosted loop base     | long-running work       |
|  [07]   | `IHostApplicationLifetime` | lifetime signals     | start/stop tokens       |
|  [08]   | `IHostLifetime`            | lifetime adapter     | process signal bridge   |
|  [09]   | `IHostEnvironment`         | environment contract | app/content identity    |
|  [10]   | `HostBuilderContext`       | builder context      | config/environment view |
|  [11]   | `HostAbortedException`     | abort exception      | bootstrap abort signal  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: builder operations

| [INDEX] | [SURFACE]                            | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :----------------------------------- | :------- | :--------------------------- |
|  [01]   | `Host.CreateApplicationBuilder`      | factory  | direct host composition      |
|  [02]   | `Host.CreateEmptyApplicationBuilder` | factory  | explicit empty defaults      |
|  [03]   | `Host.CreateDefaultBuilder`          | factory  | staged conventional defaults |
|  [04]   | `ConfigureHostConfiguration`         | instance | host config input            |
|  [05]   | `ConfigureAppConfiguration`          | instance | app config input             |
|  [06]   | `ConfigureServices`                  | instance | service graph admission      |
|  [07]   | `ConfigureContainer<TBuilder>`       | instance | provider-specific setup      |
|  [08]   | `UseServiceProviderFactory`          | instance | container replacement        |
|  [09]   | `UseDefaultServiceProvider`          | static   | provider validation policy   |
|  [10]   | `ConfigureLogging`                   | static   | logging rail admission       |
|  [11]   | `ConfigureMetrics`                   | static   | metrics rail admission       |
|  [12]   | `ConfigureHostOptions`               | static   | lifetime policy input        |
|  [13]   | `UseEnvironment`                     | static   | environment identity         |
|  [14]   | `UseContentRoot`                     | static   | content-root identity        |
|  [15]   | `UseConsoleLifetime`                 | static   | console signal bridge        |
|  [16]   | `Build`                              | instance | root host construction       |

[ENTRYPOINT_SCOPE]: runtime operations

| [INDEX] | [SURFACE]                                      | [SHAPE]  | [CAPABILITY]           |
| :-----: | :--------------------------------------------- | :------- | :--------------------- |
|  [01]   | `AddHostedService<T>`                          | static   | hosted registration    |
|  [02]   | `StartAsync`                                   | instance | starts hosted services |
|  [03]   | `RunAsync`                                     | static   | run until shutdown     |
|  [04]   | `RunConsoleAsync`                              | static   | run console host       |
|  [05]   | `WaitForShutdownAsync`                         | static   | wait for stop          |
|  [06]   | `StopAsync`                                    | instance | stops hosted services  |
|  [07]   | `IHostedLifecycleService.StartingAsync`        | instance | pre-start hook         |
|  [08]   | `IHostedLifecycleService.StartedAsync`         | instance | post-start hook        |
|  [09]   | `IHostedLifecycleService.StoppingAsync`        | instance | pre-stop hook          |
|  [10]   | `IHostedLifecycleService.StoppedAsync`         | instance | post-stop hook         |
|  [11]   | `BackgroundService.ExecuteAsync`               | instance | long-running execution |
|  [12]   | `StopApplication`                              | instance | coordinated shutdown   |
|  [13]   | `IHostApplicationLifetime.ApplicationStarted`  | property | started token          |
|  [14]   | `IHostApplicationLifetime.ApplicationStopping` | property | stopping token         |
|  [15]   | `IHostApplicationLifetime.ApplicationStopped`  | property | stopped token          |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every host folds through one builder: `Host.CreateApplicationBuilder` mints a `HostApplicationBuilder` exposing `Configuration`, `Environment`, `Services`, `Logging`, and `Metrics`, and `Build` freezes that rail into an `IHost` no later call reconfigures.
- One `IHost` lifecycle runs `StartAsync`, `RunAsync`, `StopAsync`, then `Dispose`; `IHostApplicationLifetime.StopApplication` is the sole coordinated shutdown trigger, and its three `CancellationToken`s bind transition work through `Register`.
- Hosted services start and stop in registration order: `IHostedLifecycleService` wraps each transition in `Starting`/`Started`/`Stopping`/`Stopped` hooks and `BackgroundService.ExecuteAsync` owns one long-running loop.
- `HostOptions` owns lifecycle policy — startup timeout, shutdown timeout, hosted-service start/stop concurrency, and `BackgroundServiceExceptionBehavior` on a faulted loop.

[STACKING]:
- `Microsoft.Extensions.DependencyInjection`(`.api/api-di.md`): `HostApplicationBuilder.Services` and `ConfigureServices` expose the `IServiceCollection`, `UseServiceProviderFactory`/`ConfigureContainer<TBuilder>` bind an `IServiceProviderFactory<TBuilder>`, and `Build` runs `BuildServiceProvider` into the host root provider.
- `Microsoft.Extensions.Configuration`(`.api/api-config.md`): `ConfigureHostConfiguration` and `ConfigureAppConfiguration` admit `IConfigurationBuilder` sources, and `HostApplicationBuilder.Configuration` is the `IConfigurationManager` every downstream binding reads.
- `Microsoft.Extensions.Logging.Abstractions`(`.api/api-logging.md`): `ConfigureLogging` admits `ILoggingBuilder`, and the built `IHost` resolves `ILoggerFactory` and `ILogger<T>` into hosted services.
- `Microsoft.Extensions.Options`(`.api/api-options.md`): `ConfigureHostOptions` binds `HostOptions` through the options pattern on `Services`, and lifecycle policy resolves as `IOptions<HostOptions>`.
- `Microsoft.Extensions.Hosting.Systemd`(`.api/api-hosting-lifetimes.md`): `UseConsoleLifetime` and `UseSystemd` install the `IHostLifetime` bridging process signals to `StopApplication`, the systemd row gated by its environment probe.
- within-lib: AppHost's bootstrap root mints one `HostApplicationBuilder`, folds the config, service, logging, and host-option rails onto it, and `Build` freezes the `IHost` the environment-selected `IHostLifetime` runs.

[LOCAL_ADMISSION]:
- Process-backed integrations boot through Generic Host; a hosted service adapts an external lifetime into runtime state transitions through the ordered lifecycle hooks.
- Host configuration, app configuration, logging, and metrics enter through the builder rail, and AppHost bootstrap captures environment identity and `HostOptions` policy.
- Host lifetime events feed receipts, never a second runtime state machine.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Hosting`
- Owns: process bootstrap and the host lifecycle
- Accept: builder rails admit configuration, services, logging, and lifetime; hosted services adapt external lifetimes
- Reject: a hand-rolled bootstrap framework or a second lifetime state machine
