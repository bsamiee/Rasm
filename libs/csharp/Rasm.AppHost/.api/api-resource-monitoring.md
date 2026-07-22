# [RASM_APPHOST_API_RESOURCE_MONITORING]

`Microsoft.Extensions.Diagnostics.ResourceMonitoring` admits observable process, container, disk-I/O, and network instruments, container-aware resource quotas, platform snapshot sources, and metric-shaping option policy into the observability rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Diagnostics.ResourceMonitoring`
- package: `Microsoft.Extensions.Diagnostics.ResourceMonitoring`
- assembly: `Microsoft.Extensions.Diagnostics.ResourceMonitoring`
- namespace: `Microsoft.Extensions.Diagnostics.ResourceMonitoring`
- namespace: `Microsoft.Extensions.DependencyInjection`
- meter: `Microsoft.Extensions.Diagnostics.ResourceMonitoring`
- asset: runtime library
- rail: observability

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: snapshot and quota contracts
- rail: observability

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE]    | [CAPABILITY]                      |
| :-----: | :---------------------- | :---------------- | :-------------------------------- |
|  [01]   | `ISnapshotProvider`     | snapshot contract | platform snapshot and limits read |
|  [02]   | `ResourceQuotaProvider` | quota provider    | current container quota read      |

[PUBLIC_TYPE_SCOPE]: sample, limit, and option values
- rail: observability

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE] | [CAPABILITY]                               |
| :-----: | :-------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `Snapshot`                  | sample value   | total/kernel/user time and memory bytes    |
|  [02]   | `SystemResources`           | limits value   | guaranteed and maximum CPU/memory          |
|  [03]   | `ResourceQuota`             | quota value    | baseline and maximum memory/CPU quota      |
|  [04]   | `ResourceMonitoringOptions` | option value   | metric-shaping flags and telemetry sources |

[PUBLIC_TYPE_SCOPE]: registration surface
- rail: observability

| [INDEX] | [SYMBOL]                                        | [PACKAGE_ROLE]    | [CAPABILITY]         |
| :-----: | :---------------------------------------------- | :---------------- | :------------------- |
|  [01]   | `ResourceMonitoringServiceCollectionExtensions` | service extension | monitor registration |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration
- rail: observability

| [INDEX] | [SURFACE]               | [CALL_SHAPE] | [CAPABILITY]                                   |
| :-----: | :---------------------- | :----------- | :--------------------------------------------- |
|  [01]   | `AddResourceMonitoring` | no-arg       | registers OS providers, meter, and instruments |

[ENTRYPOINT_SCOPE]: observable instruments
- rail: observability
- read model: `MeterListener` over the meter's observable instruments

| [INDEX] | [INSTRUMENT]                                | [KIND]                    | [SEMANTICS]                                      |
| :-----: | :------------------------------------------ | :------------------------ | :----------------------------------------------- |
|  [01]   | `process.cpu.utilization`                   | `ObservableGauge`         | process CPU share in `[0, 1]`                    |
|  [02]   | `dotnet.process.memory.virtual.utilization` | `ObservableGauge`         | process virtual-memory share in `[0, 1]`         |
|  [03]   | `container.cpu.time`                        | `ObservableCounter`       | container CPU seconds across all cores           |
|  [04]   | `container.cpu.limit.utilization`           | `ObservableGauge`         | container CPU-limit consumption in `[0, 1]`      |
|  [05]   | `container.cpu.request.utilization`         | `ObservableGauge`         | container CPU-request consumption in `[0, 1]`    |
|  [06]   | `container.memory.limit.utilization`        | `ObservableGauge`         | container memory-limit consumption in `[0, 1]`   |
|  [07]   | `container.memory.request.utilization`      | `ObservableGauge`         | container memory-request consumption in `[0, 1]` |
|  [08]   | `container.memory.usage`                    | `ObservableUpDownCounter` | container memory usage in bytes                  |
|  [09]   | `system.disk.io`                            | `ObservableCounter`       | disk bytes transferred                           |
|  [10]   | `system.disk.io_time`                       | `ObservableCounter`       | disk activated time                              |
|  [11]   | `system.disk.operations`                    | `ObservableCounter`       | disk operations                                  |
|  [12]   | `system.network.connections`                | `ObservableUpDownCounter` | TCP connection counts by state                   |

[ENTRYPOINT_SCOPE]: snapshot and quota reads
- rail: observability

| [INDEX] | [SURFACE]          | [CALL_SHAPE]                 | [CAPABILITY]                                   |
| :-----: | :----------------- | :--------------------------- | :--------------------------------------------- |
|  [01]   | `GetSnapshot`      | `ISnapshotProvider` no-arg   | samples CPU times and memory bytes to Snapshot |
|  [02]   | `Resources`        | `ISnapshotProvider` property | static CPU/memory limits as SystemResources    |
|  [03]   | `GetResourceQuota` | `ResourceQuotaProvider` read | reads the current container resource quota     |

[ENTRYPOINT_SCOPE]: option policy
- rail: observability
- call shape: option property on `ResourceMonitoringOptions`, bound via the standard options pattern
- gate: the `EXTEXP0008` diagnostic admits the gated rows below; a consumer opts into it to compile them, while `SourceIpAddresses` binds unconditionally

| [INDEX] | [SURFACE]                          | [GATE]        | [CAPABILITY]                                     |
| :-----: | :--------------------------------- | :------------ | :----------------------------------------------- |
|   [01]  | `UseLinuxCalculationV2`            | `EXTEXP0008`  | cgroup v2 CPU-limit calculation on Linux         |
|   [02]  | `UseZeroToOneRangeForLinuxMetrics` | `EXTEXP0008`  | normalized `[0, 1]` Linux metric range           |
|   [03]  | `UseZeroToOneRangeForMetrics`      | `EXTEXP0008`  | normalized `[0, 1]` metric range all platforms   |
|   [04]  | `EnableSystemDiskIoMetrics`        | `EXTEXP0008`  | system disk I/O instruments                      |
|   [05]  | `SourceIpAddresses`                | unconditional | Windows source-IPv4 set for connection telemetry |

## [04]-[IMPLEMENTATION_LAW]

[MONITOR_TOPOLOGY]:
- registration root: `AddResourceMonitoring()` wires the OS-appropriate snapshot provider, quota provider, meter, and observable instruments; a job/cgroup-hosted process picks the container snapshot and quota providers, a bare process picks the host providers
- observable-instrument read model: the read path samples the meter `Microsoft.Extensions.Diagnostics.ResourceMonitoring` via a `MeterListener` over its observable instruments — process share off `process.cpu.utilization` and `dotnet.process.memory.virtual.utilization`, container-relative pressure off the `container.*` gauges, and I/O off the `system.disk.*`/`system.network.connections` instruments
- cgroup vs host semantics: `container.*` instruments report against the cgroup limit/request the process runs under, process instruments report the raw process share, and `UseLinuxCalculationV2` selects the cgroup v2 CPU-limit delta over the host-CPU delta on Linux
- quota model: `ResourceQuotaProvider.GetResourceQuota()` returns the current `ResourceQuota` carrying `MaxMemoryInBytes`/`MaxCpuInCores` and `BaselineMemoryInBytes`/`BaselineCpuInCores` ceilings so a container-row grade compares against the limit the process runs under, not the host total
- snapshot source: `ISnapshotProvider.GetSnapshot()` samples a `Snapshot` (total/kernel/user `TimeSpan` and `MemoryUsageInBytes`) and `Resources` exposes the static `SystemResources` (`GuaranteedCpuUnits`/`MaximumCpuUnits`, `GuaranteedMemoryInBytes`/`MaximumMemoryInBytes`)
- platform model: Linux and Windows snapshot and quota sources stay internal behind `ISnapshotProvider` and `ResourceQuotaProvider`

[LOCAL_ADMISSION]:
- Resource monitoring is host-level observability; domain code never reads utilization directly.
- Instrument reads flow through a `MeterListener` on the package meter, not ad-hoc snapshot polling.
- Option policy binds at composition through the standard options pattern on `ResourceMonitoringOptions`.
- Utilization evidence is read-only; throttling decisions live in owning policy surfaces.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Diagnostics.ResourceMonitoring`
- Owns: process and container resource utilization evidence
- Accept: observable-instrument reads, snapshot and quota reads
- Reject: hand-rolled `/proc`, cgroup, or job-object sampling
