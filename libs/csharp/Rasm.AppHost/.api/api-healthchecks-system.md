# [RASM_APPHOST_API_HEALTHCHECKS_SYSTEM]

`AspNetCore.HealthChecks.System` mints the concrete `IHealthCheck` probes that grade a live host or process against a discrete threshold — free disk space, memory ceilings, file and folder presence, process liveness, a Windows-service predicate — over BCL primitives (`DriveInfo`, `GC`, `Process`, `File`/`Directory`, `ServiceController`). Every probe evaluates synchronously to a completed `Task<HealthCheckResult>` and enters the AppHost health fold as a `pressure`-tagged contributor row through the one `HealthContributorRow.Driver` adapter.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `AspNetCore.HealthChecks.System`
- package: `AspNetCore.HealthChecks.System` (Apache-2.0)
- assembly: `HealthChecks.System`
- namespace: `HealthChecks.System`, `Microsoft.Extensions.DependencyInjection`
- target: `net8.0`, `netstandard2.0` — the `net10.0` consumer binds the `net8.0` asset
- rail: health

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: probe family — each implements `IHealthCheck`

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]                            |
| :-----: | :---------------------------------- | :------------ | :-------------------------------------- |
|  [01]   | `DiskStorageHealthCheck`            | class         | per-drive free-megabytes floor          |
|  [02]   | `FileHealthCheck`                   | class         | `File.Exists` over a file set           |
|  [03]   | `FolderHealthCheck`                 | class         | `Directory.Exists` over a folder set    |
|  [04]   | `MaximumValueHealthCheck<T>`        | generic class | `T:IComparable<T>` current ≤ maximum    |
|  [05]   | `ProcessAllocatedMemoryHealthCheck` | class         | `GC.GetTotalMemory(false)` MB ceiling   |
|  [06]   | `ProcessHealthCheck`                | class         | `Process.GetProcessesByName` predicate  |
|  [07]   | `WindowsServiceHealthCheck`         | class         | Windows-fenced `ServiceController` gate |

[PUBLIC_TYPE_SCOPE]: fluent options builders — `Add*(target)` and `WithCheckAll*()` self-return, a `CheckAll*` bool selects exhaustive evaluation over first-failure

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                         |
| :-----: | :------------------------- | :------------ | :----------------------------------- |
|  [01]   | `DiskStorageOptions`       | class         | drive floors + failure-text delegate |
|  [02]   | `FileHealthCheckOptions`   | class         | file-existence target set            |
|  [03]   | `FolderHealthCheckOptions` | class         | folder-existence target set          |

- `DiskStorageOptions.FailedDescription` is the `ErrorDescription(driveName, minimumFreeMegabytes, actualFreeMegabytes)` delegate customizing per-drive failure text; `AddDrive(string, long minimumFreeMegabytes = 1)`, `AddFile(string)`, and `AddFolder(string)` append targets.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration extensions on `IHealthChecksBuilder` (`SystemHealthCheckBuilderExtensions`) — shared tail `(string? name, HealthStatus? failureStatus, IEnumerable<string>? tags, TimeSpan? timeout)`, `name` defaulting to the check slug

| [INDEX] | [SURFACE]                                                                      | [SHAPE] | [CAPABILITY]                               |
| :-----: | :----------------------------------------------------------------------------- | :------ | :----------------------------------------- |
|  [01]   | `AddDiskStorageHealthCheck(Action<DiskStorageOptions>?)`                       | static  | `"diskstorage"` free-space floor           |
|  [02]   | `AddPrivateMemoryHealthCheck(long)`                                            | static  | `"privatememory"` private-memory ceiling   |
|  [03]   | `AddWorkingSetHealthCheck(long)`                                               | static  | `"workingset"` working-set ceiling         |
|  [04]   | `AddVirtualMemorySizeHealthCheck(long)`                                        | static  | `"virtualmemory"` virtual-memory ceiling   |
|  [05]   | `AddProcessAllocatedMemoryHealthCheck(int)`                                    | static  | `"process_allocated_memory"` GC-MB ceiling |
|  [06]   | `AddProcessHealthCheck(string, Func<Process[], bool>)`                         | static  | `"process"` liveness predicate             |
|  [07]   | `AddWindowsServiceHealthCheck(string, Func<ServiceController, bool>, string?)` | static  | `"windowsservice"` service predicate       |
|  [08]   | `AddFolder(Action<FolderHealthCheckOptions>?)`                                 | static  | `"folder"` folder-existence set            |
|  [09]   | `AddFile(Action<FileHealthCheckOptions>?)`                                     | static  | `"file"` file-existence set                |
|  [10]   | `AddFile(Action<IServiceProvider, FileHealthCheckOptions>?)`                   | static  | `"file"` DI-resolved file set              |

- `AddPrivateMemoryHealthCheck`/`AddWorkingSetHealthCheck`/`AddVirtualMemorySizeHealthCheck` construct one `MaximumValueHealthCheck<long>` reading `Process.PrivateMemorySize64`/`WorkingSet64`/`VirtualMemorySize64` against the byte ceiling.
- `AddProcessAllocatedMemoryHealthCheck` grades `GC.GetTotalMemory(false)` in MB and throws `ArgumentException` at registration when the ceiling is nonpositive.
- `AddWindowsServiceHealthCheck` throws `PlatformNotSupportedException` at registration off Windows.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every probe implements `IHealthCheck.CheckHealthAsync(HealthCheckContext, CancellationToken)`, returns synchronously through `Task.FromResult(...)` (the green path reusing a cached `HealthCheckResultTask.Healthy`), and the disk/file/folder checks join failure strings via `string.Join("; ", …)` into one `HealthCheckResult(FailureStatus, joined)`.
- Every overload adds a `HealthCheckRegistration(name ?? <slug>, factory, failureStatus, tags, timeout)`, `failureStatus` defaulting to `HealthStatus.Unhealthy`.

[STACKING]:
- `Microsoft.Extensions.Diagnostics.HealthChecks`(`.api/api-health.md`): every probe is an `IHealthCheck` returning `HealthCheckResult`, and each `Add*` extension folds onto `IHealthChecksBuilder` as one `HealthCheckRegistration`.
- `HealthContributorRow.Driver` (`.planning/Observability/health.md`): the health fold admits `DiskStorageHealthCheck` under `DriverProbe.Disk` and `ProcessAllocatedMemoryHealthCheck` under `DriverProbe.Allocations`, seating each check's failure status on a synthetic `HealthCheckContext` — one `pressure`-tagged contributor row per probe, never an `Add*` registration face.

[LOCAL_ADMISSION]:
- `Disk` and `Allocations` are the admitted probes; a threshold breach is a typed `HealthCheckResult` with `FailureStatus`, folded by `HealthReport.Snapshot` into a `HealthSnapshot.Entry`, never a thrown exception crossing the fold.
- `WindowsServiceHealthCheck` is never admitted on osx-arm64; daemon liveness rides the `Wire/companion` sidecar host and `Microsoft.Extensions.Hosting.Systemd` lifecycle.

[RAIL_LAW]:
- Package: `AspNetCore.HealthChecks.System`
- Owns: concrete process/host threshold and filesystem-presence `IHealthCheck` probes over BCL primitives
- Accept: disk free-space floors, memory ceilings, process-presence predicates, and required path sets, each admitted through the `Driver` adapter as a `pressure`-tagged row
- Reject: an `Add*` registration face beside the `Driver` adapter, the Windows-service probe on a non-Windows host, a thrown probe failure crossing the health fold
