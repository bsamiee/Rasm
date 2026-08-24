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

| [INDEX] | [SURFACE]                 | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :------------------------ | :------- | :------------------------------------- |
|  [01]   | `ServiceState.Ready`      | static   | `READY=1` payload                      |
|  [02]   | `ServiceState.Stopping`   | static   | `STOPPING=1` payload                   |
|  [03]   | `ServiceState(string)`    | ctor     | any sd_notify assertion                |
|  [04]   | `ServiceState.ToString()` | instance | round-trips the payload back to UTF-16 |

- `SystemdLifetime.WaitForStartAsync`: arms the `ApplicationStarted`/`ApplicationStopping` registrations that write the notifications; `StopAsync` completes without notifying, since STOPPING fires from the stopping token.
- `ServiceState(string)` reaches the whole protocol. Two statics carry names; that ctor UTF-8-encodes any assertion, so `WATCHDOG=1`, `RELOADING=1` with its `MONOTONIC_USEC=` correlation stamp, `STATUS=<text>`, `EXTEND_TIMEOUT_USEC=`, and `ERRNO=` all mint through it. Multi-line payloads newline-separate inside one call, since `Notify` sends one datagram per call.
- `ISystemdNotifier.IsEnabled` answers only whether `NOTIFY_SOCKET` was exported at process start; it never proves the manager is still listening, so a `Notify` on a torn-down socket throws and every emission belongs on a typed rail.

[ENTRYPOINT_SCOPE]: environment reads (the complete set)

| [INDEX] | [VARIABLE]      | [READER]          | [CAPABILITY]                                 |
| :-----: | :-------------- | :---------------- | :------------------------------------------- |
|  [01]   | `NOTIFY_SOCKET` | `SystemdNotifier` | notify datagram endpoint; `@` means abstract |
|  [02]   | `LISTEN_PID`    | `SystemdHelpers`  | one half of the systemd-host detection       |

- WATCHDOG IS ABSENT. This assembly reads `NOTIFY_SOCKET` and `LISTEN_PID` and nothing else — no `WATCHDOG_USEC`, no `WATCHDOG_PID`, no timer, no keep-alive member of any kind.
- Consumers arming the watchdog read both variables themselves, exactly as they read `LISTEN_FDS` for socket activation, and that guard polarity DIFFERS from the socket-activation one: `sd_watchdog_enabled(3)` admits when `WATCHDOG_PID` is unset OR equal to the current pid, where `LISTEN_PID` must equal it. Ticks run at half `WATCHDOG_USEC` and the manager restarts its countdown from each notification; an unset `WATCHDOG_USEC` means the manager expects no keep-alive at all.
- Two UNIT-side facts the runtime cannot supply, both witnessed on systemd 260. `WatchdogSignal=` defaults to SIGABRT, which the CoreCLR PAL absorbs, so a missed deadline hangs the unit in `deactivating` for the whole `TimeoutStopSec` before SIGKILL — the unit declares `WatchdogSignal=SIGKILL` or an explicit SIGABRT disposition to kill promptly. `RELOADING=1` requires a `MONOTONIC_USEC=` stamp (`Stopwatch.GetTimestamp()` is CLOCK_MONOTONIC on Linux, no P/Invoke); a bare `RELOADING=1` is silently discarded and `systemctl reload` blocks to `TimeoutStartSec` then fails while the service survives.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `UseSystemd` installs the lifetime only when `SystemdHelpers.IsSystemdService` detects a PID-1, `NOTIFY_SOCKET`, or systemd-parent host.
- `SystemdNotifier` writes `ServiceState.Ready` on `ApplicationStarted` and `ServiceState.Stopping` on `ApplicationStopping` to the `NOTIFY_SOCKET` datagram socket.
- `SystemdLifetime` registers SIGTERM alone. It installs no SIGHUP handler, so a process under a `Type=notify-reload` unit that registers none of its own dies at the default disposition, exit 129.
- SIGTERM routes to graceful shutdown through `IHostApplicationLifetime.StopApplication`, which makes it a SECOND owner beside any consumer trap on the same signal.

[STACKING]:
- `api-hosting.md`(`Microsoft.Extensions.Hosting`): `UseSystemd` extends `IHostBuilder` and `AddSystemd` extends `IServiceCollection`; `SystemdLifetime` implements `IHostLifetime`, replacing the console lifetime and driving both notifications off `IHostApplicationLifetime` tokens.
- `Profiles`/`Modules` composition root: the Linux-server host-variance profile binds `AddSystemd` as its lifetime adapter, folded into the frozen service graph; `ProfileBoot` mints the protocol assertions the package does not name — `WATCHDOG=1`, the `RELOADING=1`/`STATUS=`/`READY=1` reload window — through the `ServiceState(string)` ctor and emits each on a typed rail, and it derives the watchdog period from `WATCHDOG_USEC` itself, since the package exposes no reader.

[LOCAL_ADMISSION]:
- Linux-server host profile alone selects the systemd lifetime at composition; every other host row omits it.
- Service-manager state transitions stay inside the lifetime; application code observes `IHostApplicationLifetime` only.
- `SystemdHelpers.IsSystemdService` selects composition shape, never domain logic — including which owner arms SIGTERM.
- Watchdog and reload assertions are consumer-owned payload mints, never a package gap to work around.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Hosting.Systemd`
- Owns: Generic Host lifetime binding to the systemd service manager on the Linux-server backend
- Accept: environment-gated systemd lifetime registration at composition; `ServiceState(string)` payload mints for every assertion beyond ready and stopping
- Reject: hand-rolled sd_notify socket writes, a second SIGTERM trap beside the lifetime's, custom systemd-service detection, and an untyped `Notify` call whose socket failure escapes the caller's rail
