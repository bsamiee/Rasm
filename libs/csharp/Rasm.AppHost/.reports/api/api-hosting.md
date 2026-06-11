# [RASM_APPHOST_API_HOSTING]

`Microsoft.Extensions.Hosting` supplies Generic Host composition, hosted services, lifetime signals, and bootstrap roots.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Hosting`
- package: `Microsoft.Extensions.Hosting`
- assembly: `Microsoft.Extensions.Hosting`
- namespace: `Microsoft.Extensions.Hosting`
- asset: runtime library
- rail: bootstrap

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: host family
- rail: bootstrap

| [INDEX] | [SYMBOL]                   | [PACKAGE_ROLE]   | [CAPABILITY]               |
| :-----: | :------------------------- | :--------------- | :------------------------- |
|   [1]   | `Host`                     | host facade      | anchors bootstrap contract |
|   [2]   | `HostApplicationBuilder`   | builder surface  | constructs configured root |
|   [3]   | `IHost`                    | contract surface | defines boundary contract  |
|   [4]   | `IHostBuilder`             | builder surface  | constructs configured root |
|   [5]   | `IHostedService`           | contract surface | defines boundary contract  |
|   [6]   | `BackgroundService`        | hosted loop base | anchors bootstrap contract |
|   [7]   | `IHostApplicationLifetime` | contract surface | defines boundary contract  |
|   [8]   | `HostBuilderContext`       | builder surface  | constructs configured root |
|   [9]   | `HostOptions`              | policy object    | carries policy input       |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: host operations
- rail: bootstrap

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]           | [CAPABILITY]              |
| :-----: | :------------------------------ | :--------------------- | :------------------------ |
|   [1]   | `Host.CreateApplicationBuilder` | host builder factory   | starts host composition   |
|   [2]   | `Build`                         | factory call           | creates configured handle |
|   [3]   | `RunAsync`                      | async operation        | executes async work       |
|   [4]   | `StartAsync`                    | async operation        | executes async work       |
|   [5]   | `StopAsync`                     | async operation        | executes async work       |
|   [6]   | `ConfigureServices`             | configuration delegate | applies policy value      |
|   [7]   | `ConfigureHostConfiguration`    | configuration delegate | applies policy value      |

## [4]-[IMPLEMENTATION_LAW]

[HOSTING_TOPOLOGY]:
- namespaces: `Microsoft.Extensions.Hosting`
- builder path: create builder, register services, configure host, build host
- lifetime path: start, run, stop, dispose
- hosted-service contract: `IHostedService` plus `BackgroundService` loop base
- shutdown policy: `HostOptions` carries timeout and background-service exception behavior

[LOCAL_ADMISSION]:
- Process-backed integrations boot through Generic Host.
- Hosted services adapt external lifetimes into runtime state transitions.
- Host lifetime events feed receipts; they never become a second runtime state machine.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Hosting`
- Owns: process bootstrap
- Accept: Generic Host feeds runtime ports
- Reject: custom bootstrap framework
