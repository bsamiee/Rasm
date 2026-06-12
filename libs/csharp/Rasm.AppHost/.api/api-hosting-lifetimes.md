# [RASM_APPHOST_API_HOSTING_LIFETIMES]

`Microsoft.Extensions.Hosting.Systemd` and `Microsoft.Extensions.Hosting.WindowsServices`
bind the generic host lifetime to the owning service manager: systemd notify protocol on
Linux and the Windows service control manager on Windows.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Hosting.Systemd`
- package: `Microsoft.Extensions.Hosting.Systemd`
- assembly: `Microsoft.Extensions.Hosting.Systemd`
- namespace: `Microsoft.Extensions.Hosting`
- namespace: `Microsoft.Extensions.Hosting.Systemd`
- asset: runtime library
- rail: composition

[PACKAGE_SURFACE]: `Microsoft.Extensions.Hosting.WindowsServices`
- package: `Microsoft.Extensions.Hosting.WindowsServices`
- assembly: `Microsoft.Extensions.Hosting.WindowsServices`
- namespace: `Microsoft.Extensions.Hosting`
- namespace: `Microsoft.Extensions.Hosting.WindowsServices`
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
|   [5]   | `ServiceState`                 | state value       | READY/STOPPING notify payloads |
|   [6]   | `SystemdHelpers`               | environment probe | systemd service detection      |

[PUBLIC_TYPE_SCOPE]: windows service lifetime family
- rail: composition

| [INDEX] | [SYMBOL]                                      | [PACKAGE_ROLE]    | [CAPABILITY]                    |
| :-----: | :-------------------------------------------- | :---------------- | :------------------------------ |
|   [1]   | `WindowsServiceLifetimeHostBuilderExtensions` | builder surface   | windows service registration    |
|   [2]   | `WindowsServiceLifetime`                      | host lifetime     | `ServiceBase`-backed start/stop |
|   [3]   | `WindowsServiceLifetimeOptions`               | option value      | service name policy             |
|   [4]   | `WindowsServiceHelpers`                       | environment probe | windows service detection       |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: lifetime registration
- rail: composition

| [INDEX] | [SURFACE]           | [CALL_SHAPE]                                        | [CAPABILITY]                 |
| :-----: | :------------------ | :-------------------------------------------------- | :--------------------------- |
|   [1]   | `UseSystemd`        | `IHostBuilder` extension                            | conditional systemd lifetime |
|   [2]   | `AddSystemd`        | `IServiceCollection` extension                      | systemd lifetime services    |
|   [3]   | `UseWindowsService` | `IHostBuilder`, optional options configurator       | conditional windows lifetime |
|   [4]   | `AddWindowsService` | `IServiceCollection`, optional options configurator | windows lifetime services    |

[ENTRYPOINT_SCOPE]: lifetime operations
- rail: composition

| [INDEX] | [SURFACE]           | [CALL_SHAPE]             | [CAPABILITY]                       |
| :-----: | :------------------ | :----------------------- | :--------------------------------- |
|   [1]   | `IsSystemdService`  | static environment probe | detects systemd service host       |
|   [2]   | `IsWindowsService`  | static environment probe | detects windows service host       |
|   [3]   | `Notify`            | `ServiceState` payload   | sends sd_notify state              |
|   [4]   | `IsEnabled`         | notifier property        | reports notify socket presence     |
|   [5]   | `WaitForStartAsync` | lifetime start hook      | signals READY or SCM running state |
|   [6]   | `StopAsync`         | lifetime stop hook       | signals STOPPING or SCM stop       |
|   [7]   | `ServiceName`       | option property          | names the windows service          |

## [4]-[IMPLEMENTATION_LAW]

[LIFETIME_TOPOLOGY]:
- registration model: `UseSystemd`/`UseWindowsService` install the lifetime only when the matching probe detects the service manager
- systemd channel: `SystemdNotifier` writes `ServiceState.Ready` and `ServiceState.Stopping` to the notify socket
- windows channel: `WindowsServiceLifetime` derives from `ServiceBase` and bridges SCM callbacks to `IHostApplicationLifetime`
- option surface: `WindowsServiceLifetimeOptions.ServiceName` names the SCM registration

[LOCAL_ADMISSION]:
- Lifetime packages are composition-time admissions; both registrations may coexist because each is environment-gated.
- Service-manager state transitions stay inside the lifetime; application code observes `IHostApplicationLifetime` only.
- Environment probes select composition shape and never gate domain logic.

[RAIL_LAW]:
- Packages: `Microsoft.Extensions.Hosting.Systemd`, `Microsoft.Extensions.Hosting.WindowsServices`
- Owns: host lifetime binding to the owning service manager
- Accept: environment-gated lifetime registration at composition
- Reject: hand-rolled signal handling and SCM protocol code
