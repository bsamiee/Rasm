# [RASM_APPHOST_API_HEALTHCHECKS_SYSTEM]

`AspNetCore.HealthChecks.System` (Xabaril) is a family of concrete process/host `IHealthCheck` probes — disk free space, file/folder existence, GC-allocated/private/working/virtual memory thresholds, a process-presence predicate, and a Windows-service predicate — over BCL primitives (`DriveInfo`, `File`/`Directory`, `GC`, `Process`, `ServiceController`). These are `pressure`-tagged contributor rows complementing the `ResourceMonitoring`-backed `UtilizationCell` gauge in the AppHost capability-health fold; the GC/disk/process checks are cross-platform, and the Windows-service check is fenced off on osx-arm64.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `AspNetCore.HealthChecks.System`
- package: `AspNetCore.HealthChecks.System`
- version: `9.0.0`
- license: `Apache-2.0`
- assembly: `HealthChecks.System`
- namespace: `HealthChecks.System`
- namespace: `Microsoft.Extensions.DependencyInjection`
- target: `net8.0` (also `netstandard2.0`); the `net10.0` consumer binds the `net8.0` asset, `RefSafetyRules(11)` nullable-annotated
- dependency floor: `Microsoft.Extensions.Diagnostics.HealthChecks` (`IHealthCheck`/`HealthCheckResult`/`HealthCheckRegistration`), `System.ServiceProcess.ServiceController` (Windows-only `ServiceController` — used only by the `windowsservice` check)
- platform: `WindowsServiceHealthCheck`/`AddWindowsServiceHealthCheck` are `[SupportedOSPlatform("windows")]` and THROW `PlatformNotSupportedException` at registration off Windows; all other checks are cross-platform (GC/`Process`/`DriveInfo`/filesystem)
- asset: runtime library
- rail: health

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: probe family
- rail: health

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]            | [RAIL]                                                  |
| :-----: | :---------------------------------- | :---------------------- | :----------------------------------------------------- |
|  [01]   | `DiskStorageHealthCheck`            | `IHealthCheck` probe    | per-drive free-megabytes floor                         |
|  [02]   | `FileHealthCheck`                   | `IHealthCheck` probe    | `File.Exists` over a file set                          |
|  [03]   | `FolderHealthCheck`                 | `IHealthCheck` probe    | `Directory.Exists` over a folder set                   |
|  [04]   | `MaximumValueHealthCheck<T>`        | generic `IHealthCheck`  | `where T : IComparable<T>` — current ≤ maximum gate    |
|  [05]   | `ProcessAllocatedMemoryHealthCheck` | `IHealthCheck` probe    | `GC.GetTotalMemory(false)` megabytes ceiling           |
|  [06]   | `ProcessHealthCheck`                | `IHealthCheck` probe    | `Process.GetProcessesByName` predicate                 |
|  [07]   | `WindowsServiceHealthCheck`         | `IHealthCheck` probe    | `[SupportedOSPlatform("windows")]` — `ServiceController` predicate |

[PUBLIC_TYPE_SCOPE]: options builders (fluent, `Add*`/`WithCheckAll*` self-returning)
- rail: health

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [MEMBERS]                                                                                 |
| :-----: | :------------------------ | :------------ | :--------------------------------------------------------------------------------------- |
|  [01]   | `DiskStorageOptions`      | probe options | `AddDrive(string, long minimumFreeMegabytes = 1)`, `WithCheckAllDrives()`, `bool CheckAllDrives`, `ErrorDescription FailedDescription` field (nested `delegate string ErrorDescription(string driveName, long minimumFreeMegabytes, long? actualFreeMegabytes)`) |
|  [02]   | `FileHealthCheckOptions`  | probe options | `AddFile(string)`, `WithCheckAllFiles()`, `List<string> Files`, `bool CheckAllFiles`       |
|  [03]   | `FolderHealthCheckOptions`| probe options | `AddFolder(string)`, `WithCheckAllFolders()`, `IList<string> Folders`, `bool CheckAllFolders` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration operations (`SystemHealthCheckBuilderExtensions`) — every overload takes `string? name, HealthStatus? failureStatus, IEnumerable<string>? tags, TimeSpan? timeout`
- rail: health

| [INDEX] | [SURFACE]                                                                                            | [DEFAULT_NAME]            | [RAIL]                                                  |
| :-----: | :-------------------------------------------------------------------------------------------------- | :------------------------ | :----------------------------------------------------- |
|  [01]   | `AddDiskStorageHealthCheck(this IHealthChecksBuilder, Action<DiskStorageOptions>? setup, …)`        | `"diskstorage"`           | per-drive free-space floor                             |
|  [02]   | `AddPrivateMemoryHealthCheck(this IHealthChecksBuilder, long maximumMemoryBytes, …)`                | `"privatememory"`         | `Process.PrivateMemorySize64` ≤ max                    |
|  [03]   | `AddWorkingSetHealthCheck(this IHealthChecksBuilder, long maximumMemoryBytes, …)`                   | `"workingset"`            | `Process.WorkingSet64` ≤ max                           |
|  [04]   | `AddVirtualMemorySizeHealthCheck(this IHealthChecksBuilder, long maximumMemoryBytes, …)`            | `"virtualmemory"`         | `Process.VirtualMemorySize64` ≤ max                    |
|  [05]   | `AddProcessAllocatedMemoryHealthCheck(this IHealthChecksBuilder, int maximumMegabytesAllocated, …)` | `"process_allocated_memory"` | `GC.GetTotalMemory(false)` ≤ max (throws if ≤ 0)    |
|  [06]   | `AddProcessHealthCheck(this IHealthChecksBuilder, string processName, Func<Process[], bool> predicate, …)` | `"process"`         | process-presence predicate                            |
|  [07]   | `AddWindowsServiceHealthCheck(this IHealthChecksBuilder, string serviceName, Func<ServiceController, bool> predicate, string? machineName, …)` | `"windowsservice"` | `[SupportedOSPlatform("windows")]`; throws `PlatformNotSupportedException` off Windows |
|  [08]   | `AddFolder(this IHealthChecksBuilder, Action<FolderHealthCheckOptions>? setup, …)`                  | `"folder"`                | folder-existence set                                  |
|  [09]   | `AddFile(this IHealthChecksBuilder, Action<FileHealthCheckOptions>? setup, …)`                      | `"file"`                  | file-existence set                                    |
|  [10]   | `AddFile(this IHealthChecksBuilder, Action<IServiceProvider, FileHealthCheckOptions>? setup, …)`    | `"file"`                  | file set resolved with DI                             |

[ENTRYPOINT_SCOPE]: probe operations
- rail: health

| [INDEX] | [SURFACE]                                                                | [ENTRY_FAMILY] | [RAIL]                                                                  |
| :-----: | :---------------------------------------------------------------------- | :------------- | :--------------------------------------------------------------------- |
|  [01]   | `<probe>.CheckHealthAsync(HealthCheckContext, CancellationToken)`        | probe          | each check evaluates synchronously and returns a completed `Task<HealthCheckResult>`; failures join into one `FailureStatus` description |

## [04]-[IMPLEMENTATION_LAW]

[SYSTEM_TOPOLOGY]:
- probe family: seven `IHealthCheck` types, three fluent options builders, ten extension overloads. The memory checks (`AddPrivateMemoryHealthCheck`/`AddWorkingSetHealthCheck`/`AddVirtualMemorySizeHealthCheck`) all construct a `MaximumValueHealthCheck<long>` with a `Func<long>` reading the matching `Process.GetCurrentProcess()` counter — one generic probe, three registration faces.
- evaluation mechanics: every probe is synchronous-bodied (returns `Task.FromResult(...)`, the green path reusing a cached `HealthCheckResultTask.Healthy`). Multi-target checks (disk/file/folder) accumulate failure strings and `string.Join("; ", …)` them into one `HealthCheckResult(FailureStatus, joined)`; `CheckAll*` controls whether the loop breaks on first failure or reports every failing target. `ProcessHealthCheck` and `WindowsServiceHealthCheck` return `Healthy` when the predicate holds, else `FailureStatus`.
- disk semantics: `DiskStorageOptions.AddDrive(driveName, minimumFreeMegabytes)` registers a drive; the probe reads `DriveInfo.GetDrives()`, matches by name (case-insensitive), and fails a drive whose `AvailableFreeSpace / 1MB < minimum` or that is absent; `FailedDescription` is the customizable failure-text delegate.
- memory semantics: `ProcessAllocatedMemoryHealthCheck` grades `GC.GetTotalMemory(forceFullCollection: false)` in MB and throws `ArgumentException` at registration when `maximumMegabytesAllocated <= 0`; the private/working/virtual checks grade the corresponding `Process` byte counter against a byte ceiling.
- platform fence: `WindowsServiceHealthCheck` is `[SupportedOSPlatform("windows")]`; `AddWindowsServiceHealthCheck` calls `RuntimeInformation.IsOSPlatform(OSPlatform.Windows)` and THROWS `PlatformNotSupportedException` at registration off Windows — on this osx-arm64 / Linux-container workspace this overload is never admitted. All other checks are platform-neutral.
- registration policy: every overload adds a `HealthCheckRegistration(name ?? <default>, factory, failureStatus, tags, timeout)`; `failureStatus` null defaults to `HealthStatus.Unhealthy`.

[LOCAL_ADMISSION]:
- Each admitted probe is one `HealthContributorRow.Peer` row tagged `Pressure`, never a parallel system-health surface — its `Probe` adapts the check's `CheckHealthAsync` and registers through `HealthSurface.Register`, sharing `DeadlineClass.HealthProbe` and the cadence policy with every other contributor.
- These checks are the discrete-threshold complement to the continuous `UtilizationCell` gauge (the `Gauge` row over `process.cpu.utilization`/`dotnet.process.memory.virtual.ratio`); the `ProcessAllocatedMemoryHealthCheck` and disk-storage probes catch a hard ceiling breach the windowed ratio grade does not express, while the gauge owns the trend. Memory probes are NOT a second utilization source — they grade an absolute ceiling, not a quota ratio.
- The Windows-service probe is excluded on this workspace; daemon liveness is the `Wire/companion` sidecar control host and `Microsoft.Extensions.Hosting.Systemd` lifecycle, not a `ServiceController` probe.
- A threshold breach or missing path is a typed `HealthCheckResult` with `FailureStatus`, folded by `HealthReport.Snapshot` into a `HealthSnapshot.Entry` — never a thrown exception crossing the fold.

[STACK]:
- health fold: `HealthContributorRow.Peer(name: "diskstorage" | "process_allocated_memory" | …, tag: HealthContributorRow.Pressure, cadence, probe: ct => new ValueTask<HealthCheckResult>(check.CheckHealthAsync(ctx, ct)))` is the canonical row; `Observability/health#HEALTH_FOLD` `HealthSurface.Register(...)` admits it alongside the `Gauge` row.
- degradation rail: `Pressure`-tagged degraded/unhealthy entries feed `Observability/health#DEGRADATION_RAIL` through the existing `Pressure`-Degraded / `Pressure`-Unhealthy rules with the same retained-set hysteresis the cgroup `ResourceQuota` grading uses — a disk-full or allocated-memory ceiling breach degrades the host with zero added `Rule` row, exactly as `PressurePolicy.Container` grading does.
- gauge complement: the `UtilizationCell`/`PressurePolicy.Grade` continuous signal (`Observability/health#HEALTH_FOLD`, fed by `Microsoft.Extensions.Diagnostics.ResourceMonitoring`) owns CPU/memory ratio trend under the container quota; these BCL probes own the discrete hard-ceiling and filesystem-presence facts — both project into the same `Pressure`-tagged contributor set, never two pressure pipelines.
- wire-health projection: the contributor results flow into `Wire/companion#HEALTH_SERVICE` `HealthServiceImpl.SetStatus` through the tag-predicate mapping, so a pressure breach reaches the gRPC health service.
- resilience boundary: every probe runs under `DeadlineClass.HealthProbe`; these are synchronous reads of host counters and the filesystem, so they carry no outbound resilience budget.

[RAIL_LAW]:
- Package: `AspNetCore.HealthChecks.System`
- Owns: discrete host/process threshold and filesystem-presence facts as `pressure`-tagged contributor probes
- Accept: disk free-space floors, hard memory ceilings, process-presence predicates, and required-path sets
- Reject: a second utilization pipeline beside `UtilizationCell`, the Windows-service probe on a non-Windows host, or a thrown probe failure crossing the health fold
