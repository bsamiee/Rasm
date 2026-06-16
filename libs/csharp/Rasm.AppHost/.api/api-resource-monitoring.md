# [RASM_APPHOST_API_RESOURCE_MONITORING]

`Microsoft.Extensions.Diagnostics.ResourceMonitoring` admits windowed CPU and memory
utilization snapshots, container-aware resource limits, publisher fan-out, and
monitoring option policy into the observability rail.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Diagnostics.ResourceMonitoring`
- package: `Microsoft.Extensions.Diagnostics.ResourceMonitoring`
- assembly: `Microsoft.Extensions.Diagnostics.ResourceMonitoring`
- namespace: `Microsoft.Extensions.Diagnostics.ResourceMonitoring`
- namespace: `Microsoft.Extensions.DependencyInjection`
- asset: runtime library
- rail: observability

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: monitor contracts
- rail: observability

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE]     | [CAPABILITY]              |
| :-----: | :------------------------------ | :----------------- | :------------------------ |
|   [1]   | `IResourceMonitor`              | monitor contract   | windowed utilization read |
|   [2]   | `IResourceMonitorBuilder`       | builder contract   | publisher admission       |
|   [3]   | `IResourceUtilizationPublisher` | publisher contract | utilization fan-out       |
|   [4]   | `ISnapshotProvider`             | snapshot contract  | platform snapshot source  |

[PUBLIC_TYPE_SCOPE]: utilization values and options
- rail: observability

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE]    | [CAPABILITY]                      |
| :-----: | :-------------------------- | :---------------- | :-------------------------------- |
|   [1]   | `ResourceUtilization`       | utilization value | CPU and memory usage percentages  |
|   [2]   | `SystemResources`           | limits value      | guaranteed and maximum units      |
|   [3]   | `Snapshot`                  | sample value      | kernel/user time and memory bytes |
|   [4]   | `ResourceQuota`             | quota value       | baseline and maximum quota        |
|   [5]   | `ResourceQuotaProvider`     | quota provider    | current container quota read      |
|   [6]   | `ResourceMonitoringOptions` | option value      | windows, intervals, metric flags  |

The pull-model contracts in the prior table (`IResourceMonitor`, `IResourceUtilizationPublisher`,
`ISnapshotProvider`, `ResourceUtilization`, `SystemResources`) are obsolete as of v10.7.0; the
current observability path reads CPU/memory pressure off OTel observable instruments and reads the
container quota off `ResourceQuotaProvider`. The obsolete contracts stay catalogued only as the
migration source the consuming page deletes.

[PUBLIC_TYPE_SCOPE]: registration surfaces
- rail: observability

| [INDEX] | [SYMBOL]                                        | [PACKAGE_ROLE]    | [CAPABILITY]         |
| :-----: | :---------------------------------------------- | :---------------- | :------------------- |
|   [1]   | `ResourceMonitoringServiceCollectionExtensions` | service extension | monitor registration |
|   [2]   | `ResourceMonitoringBuilderExtensions`           | builder extension | option configuration |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration and configuration
- rail: observability

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                                  | [CAPABILITY]                 |
| :-----: | :---------------------- | :-------------------------------------------- | :--------------------------- |
|   [1]   | `AddResourceMonitoring` | no-arg or `Action<IResourceMonitorBuilder>`   | registers monitor service    |
|   [2]   | `ConfigureMonitor`      | options configurator or configuration section | binds monitoring options     |
|   [3]   | `AddPublisher<T>`       | builder generic registration                  | admits utilization publisher |

[ENTRYPOINT_SCOPE]: monitor operations
- rail: observability

| [INDEX] | [SURFACE]            | [CALL_SHAPE]                                     | [CAPABILITY]                          |
| :-----: | :------------------- | :----------------------------------------------- | :------------------------------------ |
|   [1]   | `GetUtilization`     | `TimeSpan` window                                | computes utilization over the window  |
|   [2]   | `PublishAsync`       | utilization plus cancellation token              | delivers utilization to a publisher   |
|   [3]   | `GetResourceQuota`   | `ResourceQuotaProvider.GetResourceQuota()` -> `ResourceQuota`; `GetResourceQuota(out ResourceQuota)` try-shape | reads the current container resource quota |

[ENTRYPOINT_SCOPE]: option policy
- rail: observability

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]    | [CAPABILITY]                  |
| :-----: | :--------------------------------- | :-------------- | :---------------------------- |
|   [1]   | `CollectionWindow`                 | option property | sample retention window       |
|   [2]   | `SamplingInterval`                 | option property | snapshot cadence              |
|   [3]   | `PublishingWindow`                 | option property | publisher aggregation window  |
|   [4]   | `CpuConsumptionRefreshInterval`    | option property | CPU metric refresh cadence    |
|   [5]   | `MemoryConsumptionRefreshInterval` | option property | memory metric refresh cadence |
|   [6]   | `UseLinuxCalculationV2`            | option property | cgroup v2 CPU calculation     |
|   [7]   | `UseZeroToOneRangeForLinuxMetrics` | option property | normalized Linux metric range |
|   [8]   | `EnableSystemDiskIoMetrics`        | option property | system disk I/O instruments   |

## [4]-[IMPLEMENTATION_LAW]

[MONITOR_TOPOLOGY]:
- registration root: `AddResourceMonitoring` wires monitor service, snapshot provider, publishers, and the quota provider
- observable-instrument read model: the current path reads CPU/memory pressure off the meter `Microsoft.Extensions.Diagnostics.ResourceMonitoring`, instruments `process.cpu.utilization` and `dotnet.process.memory.virtual.ratio`, via a `MeterListener` over observable instruments — never the obsolete `IResourceMonitor.GetUtilization` pull
- quota model: `ResourceQuotaProvider.GetResourceQuota()` returns the current `ResourceQuota` carrying `MaxMemoryInBytes`/`MaxCpuInCores` and `BaselineMemoryInBytes`/`BaselineCpuInCores` ceilings so a container-row grade compares against the limit the process runs under, not the host total
- obsolete pull model: `IResourceMonitor.GetUtilization(window)` folds buffered snapshots into `ResourceUtilization` (usage percentages, used bytes, `SystemResources`, latest `Snapshot`) — obsolete v10.7.0, kept only as the migration source
- platform model: Linux and Windows snapshot sources stay internal behind `ISnapshotProvider`

[LOCAL_ADMISSION]:
- Resource monitoring is host-level observability; domain code never reads utilization directly.
- Publishers are admitted through the builder, not resolved ad hoc from the container.
- Option policy binds at composition from typed options or a configuration section.
- Utilization values are read-only evidence; throttling decisions live in owning policy surfaces.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Diagnostics.ResourceMonitoring`
- Owns: process and container resource utilization evidence
- Accept: windowed utilization reads and publisher fan-out
- Reject: hand-rolled `/proc` or job-object sampling
