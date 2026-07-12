# [RASM_APPHOST_API_HEALTHCHECKS_SYSTEM]

`AspNetCore.HealthChecks.System` (Xabaril) is a family of concrete process/host `IHealthCheck` probes — disk free space, file/folder existence, GC-allocated/private/working/virtual memory thresholds, a process-presence predicate, and a Windows-service predicate — over BCL primitives (`DriveInfo`, `File`/`Directory`, `GC`, `Process`, `ServiceController`). These are `pressure`-tagged contributor rows complementing the `ResourceMonitoring`-backed `UtilizationCell` gauge in the AppHost capability-health fold; the GC/disk/process checks are cross-platform, and the Windows-service check is fenced off on osx-arm64.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `AspNetCore.HealthChecks.System`
- package: `AspNetCore.HealthChecks.System`
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

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]          | [RAIL]                                                             |
| :-----: | :---------------------------------- | :--------------------- | :----------------------------------------------------------------- |
|  [01]   | `DiskStorageHealthCheck`            | `IHealthCheck` probe   | per-drive free-megabytes floor                                     |
|  [02]   | `FileHealthCheck`                   | `IHealthCheck` probe   | `File.Exists` over a file set                                      |
|  [03]   | `FolderHealthCheck`                 | `IHealthCheck` probe   | `Directory.Exists` over a folder set                               |
|  [04]   | `MaximumValueHealthCheck<T>`        | generic `IHealthCheck` | `where T : IComparable<T>` — current ≤ maximum gate                |
|  [05]   | `ProcessAllocatedMemoryHealthCheck` | `IHealthCheck` probe   | `GC.GetTotalMemory(false)` megabytes ceiling                       |
|  [06]   | `ProcessHealthCheck`                | `IHealthCheck` probe   | `Process.GetProcessesByName` predicate                             |
|  [07]   | `WindowsServiceHealthCheck`         | `IHealthCheck` probe   | `[SupportedOSPlatform("windows")]` — `ServiceController` predicate |

[PUBLIC_TYPE_SCOPE]: options builders (fluent, `Add*`/`WithCheckAll*` self-returning)
- rail: health

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] |
| :-----: | :------------------------- | :------------ |
|  [01]   | `DiskStorageOptions`       | probe options |
|  [02]   | `FileHealthCheckOptions`   | probe options |
|  [03]   | `FolderHealthCheckOptions` | probe options |

[DISK_STORAGE_OPTIONS]: `AddDrive(string, long minimumFreeMegabytes = 1)` and `WithCheckAllDrives()` return the builder; `bool CheckAllDrives` selects exhaustive evaluation.
- failure text: `ErrorDescription FailedDescription`, where `delegate string ErrorDescription(string driveName, long minimumFreeMegabytes, long? actualFreeMegabytes)` defines the field contract

[FILE_HEALTH_CHECK_OPTIONS]: `AddFile(string)` and `WithCheckAllFiles()` return the builder; `List<string> Files` carries the targets, and `bool CheckAllFiles` selects exhaustive evaluation.

[FOLDER_HEALTH_CHECK_OPTIONS]: `AddFolder(string)` and `WithCheckAllFolders()` return the builder; `IList<string> Folders` carries the targets, and `bool CheckAllFolders` selects exhaustive evaluation.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration operations (`SystemHealthCheckBuilderExtensions`) — every overload takes `string? name, HealthStatus? failureStatus, IEnumerable<string>? tags, TimeSpan? timeout`
- rail: health

| [INDEX] | [SURFACE]                              | [DEFAULT_NAME]               | [RAIL]                   |
| :-----: | :------------------------------------- | :--------------------------- | :----------------------- |
|  [01]   | `AddDiskStorageHealthCheck`            | `"diskstorage"`              | free-space floor         |
|  [02]   | `AddPrivateMemoryHealthCheck`          | `"privatememory"`            | private-memory ceiling   |
|  [03]   | `AddWorkingSetHealthCheck`             | `"workingset"`               | working-set ceiling      |
|  [04]   | `AddVirtualMemorySizeHealthCheck`      | `"virtualmemory"`            | virtual-memory ceiling   |
|  [05]   | `AddProcessAllocatedMemoryHealthCheck` | `"process_allocated_memory"` | allocated-memory ceiling |
|  [06]   | `AddProcessHealthCheck`                | `"process"`                  | process predicate        |
|  [07]   | `AddWindowsServiceHealthCheck`         | `"windowsservice"`           | service predicate        |
|  [08]   | `AddFolder`                            | `"folder"`                   | folder-existence set     |
|  [09]   | `AddFile`                              | `"file"`                     | file-existence set       |
|  [10]   | `AddFile`                              | `"file"`                     | DI-resolved file set     |

[REGISTRATION_INPUTS]: each surface extends `IHealthChecksBuilder`; the unique arguments select its probe input.
- `AddDiskStorageHealthCheck`: `Action<DiskStorageOptions>? setup`
- `AddPrivateMemoryHealthCheck`: `long maximumMemoryBytes`; reads `Process.PrivateMemorySize64`
- `AddWorkingSetHealthCheck`: `long maximumMemoryBytes`; reads `Process.WorkingSet64`
- `AddVirtualMemorySizeHealthCheck`: `long maximumMemoryBytes`; reads `Process.VirtualMemorySize64`
- `AddProcessAllocatedMemoryHealthCheck`: `int maximumMegabytesAllocated`; reads `GC.GetTotalMemory(false)` and throws when the value is nonpositive
- `AddProcessHealthCheck`: `string processName, Func<Process[], bool> predicate`
- `AddWindowsServiceHealthCheck`: `string serviceName, Func<ServiceController, bool> predicate, string? machineName`; the Windows-only surface throws `PlatformNotSupportedException` off Windows
- `AddFolder`: `Action<FolderHealthCheckOptions>? setup`
- `AddFile`: either `Action<FileHealthCheckOptions>? setup` or `Action<IServiceProvider, FileHealthCheckOptions>? setup`

[ENTRYPOINT_SCOPE]: probe operations
- rail: health

[CHECK_HEALTH_ASYNC]: `<probe>.CheckHealthAsync(HealthCheckContext, CancellationToken)` evaluates synchronously and returns a completed `Task<HealthCheckResult>`.
- failure: multiple failures join into one `FailureStatus` description

## [04]-[IMPLEMENTATION_LAW]

[SYSTEM_TOPOLOGY]:
- probe family: seven `IHealthCheck` types, three fluent options builders, and ten extension overloads
- memory probes: `AddPrivateMemoryHealthCheck`, `AddWorkingSetHealthCheck`, and `AddVirtualMemorySizeHealthCheck` construct `MaximumValueHealthCheck<long>` with a `Func<long>` reading the matching `Process.GetCurrentProcess()` counter; one generic probe owns the three registration faces
- evaluation mechanics: every probe returns `Task.FromResult(...)`, and the green path reuses a cached `HealthCheckResultTask.Healthy`
- multi-target evaluation: disk, file, and folder checks accumulate failure strings and join them through `string.Join("; ", …)` into one `HealthCheckResult(FailureStatus, joined)`; `CheckAll*` selects exhaustive evaluation or the first failure
- predicate evaluation: `ProcessHealthCheck` and `WindowsServiceHealthCheck` return `Healthy` when their predicate holds and `FailureStatus` otherwise
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
- health fold: `HealthContributorRow.Peer(name: "diskstorage" | "process_allocated_memory" | …, tag: HealthContributorRow.Pressure, cadence, probe: ct => new ValueTask<HealthCheckResult>(check.CheckHealthAsync(ctx, ct)))` is the canonical row; `HealthSurface.Register(...)` admits it alongside the `Gauge` row
- degradation rail: `Pressure`-tagged degraded/unhealthy entries feed the existing `Pressure`-Degraded and `Pressure`-Unhealthy rules with the retained-set hysteresis used by cgroup `ResourceQuota` grading; a disk-full or allocated-memory ceiling breach degrades the host without another `Rule` row
- gauge complement: the `UtilizationCell` and `PressurePolicy.Grade` signal, fed by `Microsoft.Extensions.Diagnostics.ResourceMonitoring`, owns CPU/memory ratio trend under the container quota; these BCL probes own discrete hard-ceiling and filesystem-presence facts, and both project into one `Pressure`-tagged contributor set
- wire-health projection: the contributor results reach `HealthServiceImpl.SetStatus` through the tag-predicate mapping, so a pressure breach reaches the gRPC health service
- resilience boundary: every probe runs under `DeadlineClass.HealthProbe`; these are synchronous reads of host counters and the filesystem, so they carry no outbound resilience budget.

[RAIL_LAW]:
- Package: `AspNetCore.HealthChecks.System`
- Owns: discrete host/process threshold and filesystem-presence facts as `pressure`-tagged contributor probes
- Accept: disk free-space floors, hard memory ceilings, process-presence predicates, and required-path sets
- Reject: a second utilization pipeline beside `UtilizationCell`, the Windows-service probe on a non-Windows host, or a thrown probe failure crossing the health fold
