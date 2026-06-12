# [RASM_APPHOST_API_HOSTING]

`Microsoft.Extensions.Hosting` supplies Generic Host composition, hosted services, lifecycle callbacks, environment identity, lifetime signals, and bootstrap roots.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Hosting`
- package: `Microsoft.Extensions.Hosting`
- assembly: `Microsoft.Extensions.Hosting`
- contract_assembly: `Microsoft.Extensions.Hosting.Abstractions`
- namespace: `Microsoft.Extensions.Hosting`
- asset: runtime library
- rail: bootstrap

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: host implementation family
- rail: bootstrap

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY]    | [RAIL]               |
| :-----: | :----------------------------------- | :--------------- | :------------------- |
|   [1]   | `Host`                               | host facade      | builder entry        |
|   [2]   | `HostApplicationBuilder`             | app builder      | modern host root     |
|   [3]   | `HostApplicationBuilderSettings`     | builder settings | default policy input |
|   [4]   | `HostBuilder`                        | host builder     | staged host root     |
|   [5]   | `HostOptions`                        | host policy      | lifetime policy      |
|   [6]   | `ConsoleLifetimeOptions`             | console policy   | console lifetime     |
|   [7]   | `BackgroundServiceExceptionBehavior` | exception policy | hosted loop failure  |

[PUBLIC_TYPE_SCOPE]: host contract family
- rail: bootstrap

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]        | [RAIL]                  |
| :-----: | :------------------------- | :------------------- | :---------------------- |
|   [1]   | `IHost`                    | host contract        | runtime root            |
|   [2]   | `IHostBuilder`             | builder contract     | staged composition      |
|   [3]   | `IHostApplicationBuilder`  | app builder contract | direct composition      |
|   [4]   | `IHostedService`           | service contract     | start/stop participant  |
|   [5]   | `IHostedLifecycleService`  | lifecycle contract   | ordered lifecycle hooks |
|   [6]   | `BackgroundService`        | hosted loop base     | long-running work       |
|   [7]   | `IHostApplicationLifetime` | lifetime signals     | start/stop tokens       |
|   [8]   | `IHostLifetime`            | lifetime adapter     | process signal bridge   |
|   [9]   | `IHostEnvironment`         | environment contract | app/content identity    |
|  [10]   | `HostBuilderContext`       | builder context      | config/environment view |
|  [11]   | `HostAbortedException`     | abort exception      | bootstrap abort signal  |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: builder operations
- rail: bootstrap

| [INDEX] | [SURFACE]                            | [ENTRY_FAMILY]         | [RAIL]                  |
| :-----: | :----------------------------------- | :--------------------- | :---------------------- |
|   [1]   | `Host.CreateApplicationBuilder`      | app builder factory    | direct host composition |
|   [2]   | `Host.CreateEmptyApplicationBuilder` | app builder factory    | explicit empty defaults |
|   [3]   | `Host.CreateDefaultBuilder`          | staged builder factory | conventional defaults   |
|   [4]   | `ConfigureHostConfiguration`         | host config delegate   | host policy input       |
|   [5]   | `ConfigureAppConfiguration`          | app config delegate    | app policy input        |
|   [6]   | `ConfigureServices`                  | DI delegate            | service graph admission |
|   [7]   | `ConfigureContainer<TBuilder>`       | container delegate     | provider-specific setup |
|   [8]   | `UseServiceProviderFactory`          | provider factory       | container replacement   |
|   [9]   | `UseDefaultServiceProvider`          | provider policy        | provider validation     |
|  [10]   | `ConfigureLogging`                   | logging delegate       | logging rail admission  |
|  [11]   | `ConfigureMetrics`                   | metrics delegate       | metrics rail admission  |
|  [12]   | `ConfigureHostOptions`               | host options delegate  | lifetime policy         |
|  [13]   | `UseEnvironment`                     | environment setting    | environment identity    |
|  [14]   | `UseContentRoot`                     | filesystem setting     | content-root identity   |
|  [15]   | `UseConsoleLifetime`                 | lifetime adapter       | console signal bridge   |
|  [16]   | `Build`                              | host factory           | root host construction  |

[ENTRYPOINT_SCOPE]: runtime operations
- rail: bootstrap

| [INDEX] | [SURFACE]                               | [ENTRY_FAMILY]      | [RAIL]                   |
| :-----: | :-------------------------------------- | :------------------ | :----------------------- |
|   [1]   | `AddHostedService<T>`                   | hosted registration | hosted service admission |
|   [2]   | `StartAsync`                            | host lifecycle      | starts hosted services   |
|   [3]   | `RunAsync`                              | host lifecycle      | run-until-shutdown       |
|   [4]   | `RunConsoleAsync`                       | console lifecycle   | run console host         |
|   [5]   | `WaitForShutdownAsync`                  | shutdown wait       | blocks until stop        |
|   [6]   | `StopAsync`                             | host lifecycle      | stops hosted services    |
|   [7]   | `IHostedLifecycleService.StartingAsync` | lifecycle hook      | before service start     |
|   [8]   | `IHostedLifecycleService.StartedAsync`  | lifecycle hook      | after service start      |
|   [9]   | `IHostedLifecycleService.StoppingAsync` | lifecycle hook      | before service stop      |
|  [10]   | `IHostedLifecycleService.StoppedAsync`  | lifecycle hook      | after service stop       |
|  [11]   | `BackgroundService.ExecuteAsync`        | hosted loop         | long-running execution   |
|  [12]   | `StopApplication`                       | lifetime signal     | coordinated shutdown     |

## [4]-[IMPLEMENTATION_LAW]

[HOSTING_TOPOLOGY]:
- namespaces: `Microsoft.Extensions.Hosting`
- builder path: create builder, configure host, configure app, register services, configure container, build host
- builder state: environment, configuration, services, logging, metrics, properties
- lifetime path: start, run, stop, dispose
- hosted-service contract: `IHostedService`, `IHostedLifecycleService`, `BackgroundService`
- shutdown policy: `HostOptions` carries startup timeout, shutdown timeout, concurrency, and background-service exception behavior
- environment identity: application name, environment name, content root path, content root file provider
- console policy: `ConsoleLifetimeOptions.SuppressStatusMessages`

[LOCAL_ADMISSION]:
- Process-backed integrations boot through Generic Host.
- Hosted services adapt external lifetimes into runtime state transitions with ordered lifecycle hooks.
- Host configuration, app configuration, logging, and metrics enter through the builder rail.
- AppHost bootstrap records capture environment identity and host option policy.
- Host lifetime events feed receipts; they never become a second runtime state machine.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Hosting`
- Owns: process bootstrap
- Accept: Generic Host feeds runtime ports
- Reject: custom bootstrap framework
