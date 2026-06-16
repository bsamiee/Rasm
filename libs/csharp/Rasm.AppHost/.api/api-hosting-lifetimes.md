# [RASM_APPHOST_API_HOSTING_LIFETIMES]

`Microsoft.Extensions.Hosting.Systemd` binds the generic host lifetime to the systemd
service manager through the sd_notify protocol on the Linux-server host backend, carrying
READY/STOPPING state and the watchdog keep-alive over the notify socket.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Hosting.Systemd`
- package: `Microsoft.Extensions.Hosting.Systemd`
- assembly: `Microsoft.Extensions.Hosting.Systemd`
- namespace: `Microsoft.Extensions.Hosting`
- namespace: `Microsoft.Extensions.Hosting.Systemd`
- asset: runtime library
- rail: composition

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: systemd lifetime family
- rail: composition

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE]    | [CAPABILITY]                   |
| :-----: | :----------------------------- | :---------------- | :----------------------------- |
|   [1]   | `SystemdHostBuilderExtensions` | builder surface   | systemd lifetime registration  |
|   [2]   | `SystemdLifetime`              | host lifetime     | notify-aware start and stop    |
|   [3]   | `ISystemdNotifier`             | notifier contract | sd_notify channel              |
|   [4]   | `SystemdNotifier`              | notifier          | notify socket writer           |
|   [5]   | `ServiceState`                 | state value       | READY/STOPPING/WATCHDOG payloads |
|   [6]   | `SystemdHelpers`               | environment probe | systemd service detection      |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: lifetime registration
- rail: composition

| [INDEX] | [SURFACE]           | [CALL_SHAPE]                                        | [CAPABILITY]                 |
| :-----: | :------------------ | :-------------------------------------------------- | :--------------------------- |
|   [1]   | `UseSystemd`        | `IHostBuilder` extension                            | conditional systemd lifetime |
|   [2]   | `AddSystemd`        | `IServiceCollection` extension                      | systemd lifetime services    |

[ENTRYPOINT_SCOPE]: lifetime operations
- rail: composition

| [INDEX] | [SURFACE]           | [CALL_SHAPE]             | [CAPABILITY]                       |
| :-----: | :------------------ | :----------------------- | :--------------------------------- |
|   [1]   | `IsSystemdService`  | static environment probe | detects systemd service host       |
|   [2]   | `Notify`            | `ServiceState` payload   | sends sd_notify state              |
|   [3]   | `IsEnabled`         | notifier property        | reports notify socket presence     |
|   [4]   | `WaitForStartAsync` | lifetime start hook      | signals READY running state        |
|   [5]   | `StopAsync`         | lifetime stop hook       | signals STOPPING                   |

## [4]-[IMPLEMENTATION_LAW]

[LIFETIME_TOPOLOGY]:
- registration model: `UseSystemd` installs the lifetime only when `SystemdHelpers.IsSystemdService` detects the service manager
- systemd channel: `SystemdNotifier` writes `ServiceState.Ready` and `ServiceState.Stopping` to the notify socket
- watchdog channel: `ISystemdNotifier.Notify(new ServiceState("WATCHDOG=1"))` rides the heartbeat tick, the period derived from the `WATCHDOG_USEC` environment deadline the service manager sets
- backend scope: the Linux-server host profile is the only consuming row; no Windows service-control-manager backend is admitted

[LOCAL_ADMISSION]:
- The systemd lifetime is a composition-time admission gated by the environment probe; the Linux-server host profile selects it and every other host row omits it.
- Service-manager state transitions stay inside the lifetime; application code observes `IHostApplicationLifetime` only.
- Environment probes select composition shape and never gate domain logic.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Hosting.Systemd`
- Owns: host lifetime binding to the systemd service manager on the Linux-server backend
- Accept: environment-gated systemd lifetime registration at composition
- Reject: hand-rolled signal handling, Windows service-control-manager code, and notify-protocol re-implementation
