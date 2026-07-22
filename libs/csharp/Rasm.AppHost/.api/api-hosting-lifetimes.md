# [RASM_APPHOST_API_HOSTING_LIFETIMES]

`Microsoft.Extensions.Hosting.Systemd` binds the Generic Host lifetime to the systemd service manager over the sd_notify socket, signaling READY on start and STOPPING on graceful shutdown and bridging SIGTERM into `IHostApplicationLifetime`; the Linux-server host profile is its sole consumer.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Hosting.Systemd`
- package: `Microsoft.Extensions.Hosting.Systemd`
- assembly: `Microsoft.Extensions.Hosting.Systemd`
- namespace: `Microsoft.Extensions.Hosting`, `Microsoft.Extensions.Hosting.Systemd`
- rail: composition

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: systemd lifetime family

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                  |
| :-----: | :----------------------------- | :------------ | :---------------------------- |
|  [01]   | `SystemdHostBuilderExtensions` | class         | systemd lifetime registration |
|  [02]   | `SystemdLifetime`              | class         | notify-aware `IHostLifetime`  |
|  [03]   | `ISystemdNotifier`             | interface     | sd_notify channel contract    |
|  [04]   | `SystemdNotifier`              | class         | notify socket writer          |
|  [05]   | `ServiceState`                 | struct        | sd_notify state payload       |
|  [06]   | `SystemdHelpers`               | class         | systemd host detection        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: lifetime registration

| [INDEX] | [SURFACE]                                              | [SHAPE] | [CAPABILITY]                                  |
| :-----: | :----------------------------------------------------- | :------ | :-------------------------------------------- |
|  [01]   | `UseSystemd(IHostBuilder) -> IHostBuilder`             | static  | install the lifetime when hosted as a service |
|  [02]   | `AddSystemd(IServiceCollection) -> IServiceCollection` | static  | register the lifetime services                |

[ENTRYPOINT_SCOPE]: lifetime and notify operations

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :------------------------------------------------------------- | :------- | :----------------------------- |
|  [01]   | `SystemdHelpers.IsSystemdService() -> bool`                    | static   | detect a systemd service host  |
|  [02]   | `ISystemdNotifier.Notify(ServiceState)`                        | instance | send an sd_notify state        |
|  [03]   | `ISystemdNotifier.IsEnabled -> bool`                           | property | report notify socket presence  |
|  [04]   | `SystemdLifetime.WaitForStartAsync(CancellationToken) -> Task` | instance | arm notify hooks, signal READY |
|  [05]   | `SystemdLifetime.StopAsync(CancellationToken) -> Task`         | instance | complete host stop             |

[ENTRYPOINT_SCOPE]: service state vocabulary

| [INDEX] | [SURFACE]               | [SHAPE] | [CAPABILITY]             |
| :-----: | :---------------------- | :------ | :----------------------- |
|  [01]   | `ServiceState.Ready`    | static  | `READY=1` payload        |
|  [02]   | `ServiceState.Stopping` | static  | `STOPPING=1` payload     |
|  [03]   | `ServiceState(string)`  | ctor    | custom sd_notify payload |

- `SystemdLifetime.WaitForStartAsync`: arms the `ApplicationStarted`/`ApplicationStopping` registrations that write the notifications; `StopAsync` completes without notifying, since STOPPING fires from the stopping token.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `UseSystemd` installs the lifetime only when `SystemdHelpers.IsSystemdService` detects a PID-1, `NOTIFY_SOCKET`, or systemd-parent host.
- `SystemdNotifier` writes `ServiceState.Ready` on `ApplicationStarted` and `ServiceState.Stopping` on `ApplicationStopping` to the `NOTIFY_SOCKET` datagram socket.
- SIGTERM routes to graceful shutdown through `IHostApplicationLifetime.StopApplication`.
- `ServiceState(string)` admits any custom sd_notify payload; the package names only `Ready` and `Stopping`.

[STACKING]:
- `api-hosting.md`(`Microsoft.Extensions.Hosting`): `UseSystemd` extends `IHostBuilder` and `AddSystemd` extends `IServiceCollection`; `SystemdLifetime` implements `IHostLifetime`, replacing the console lifetime and driving both notifications off `IHostApplicationLifetime` tokens.
- `Profiles`/`Modules` composition root: the Linux-server host-variance profile binds `UseSystemd` as its lifetime adapter, folded into the frozen service graph.

[LOCAL_ADMISSION]:
- Linux-server host profile alone selects the systemd lifetime at composition; every other host row omits it.
- Service-manager state transitions stay inside the lifetime; application code observes `IHostApplicationLifetime` only.
- `SystemdHelpers.IsSystemdService` selects composition shape, never domain logic.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Hosting.Systemd`
- Owns: Generic Host lifetime binding to the systemd service manager on the Linux-server backend
- Accept: environment-gated systemd lifetime registration at composition
- Reject: hand-rolled sd_notify socket writes, manual SIGTERM signal handling, and custom systemd-service detection
