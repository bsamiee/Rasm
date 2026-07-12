# [RASM_APPHOST_API_RESOURCE_MONITORING]

`Microsoft.Extensions.Diagnostics.ResourceMonitoring` admits windowed CPU and memory utilization snapshots, container-aware resource limits, publisher fan-out, and monitoring option policy into the observability rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Diagnostics.ResourceMonitoring`

- package: `Microsoft.Extensions.Diagnostics.ResourceMonitoring`
- assembly: `Microsoft.Extensions.Diagnostics.ResourceMonitoring`
- namespace: `Microsoft.Extensions.Diagnostics.ResourceMonitoring`
- namespace: `Microsoft.Extensions.DependencyInjection`
- asset: runtime library
- rail: observability

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: monitor contracts

- rail: observability

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE]     | [CAPABILITY]              |
| :-----: | :------------------------------ | :----------------- | :------------------------ |
|  [01]   | `IResourceMonitor`              | monitor contract   | windowed utilization read |
|  [02]   | `IResourceMonitorBuilder`       | builder contract   | publisher admission       |
|  [03]   | `IResourceUtilizationPublisher` | publisher contract | utilization fan-out       |
|  [04]   | `ISnapshotProvider`             | snapshot contract  | platform snapshot source  |

[PUBLIC_TYPE_SCOPE]: utilization values and options

- rail: observability

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE]    | [CAPABILITY]                      |
| :-----: | :-------------------------- | :---------------- | :-------------------------------- |
|  [01]   | `ResourceUtilization`       | utilization value | CPU and memory usage percentages  |
|  [02]   | `SystemResources`           | limits value      | guaranteed and maximum units      |
|  [03]   | `Snapshot`                  | sample value      | kernel/user time and memory bytes |
|  [04]   | `ResourceQuota`             | quota value       | baseline and maximum quota        |
|  [05]   | `ResourceQuotaProvider`     | quota provider    | current container quota read      |
|  [06]   | `ResourceMonitoringOptions` | option value      | windows, intervals, metric flags  |

[PUBLIC_TYPE_SCOPE]: registration surfaces

- rail: observability

| [INDEX] | [SYMBOL]                                        | [PACKAGE_ROLE]    | [CAPABILITY]         |
| :-----: | :---------------------------------------------- | :---------------- | :------------------- |
|  [01]   | `ResourceMonitoringServiceCollectionExtensions` | service extension | monitor registration |
|  [02]   | `ResourceMonitoringBuilderExtensions`           | builder extension | option configuration |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration and configuration

- rail: observability

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                                  | [CAPABILITY]                 |
| :-----: | :---------------------- | :-------------------------------------------- | :--------------------------- |
|  [01]   | `AddResourceMonitoring` | no-arg or `Action<IResourceMonitorBuilder>`   | registers monitor service    |
|  [02]   | `ConfigureMonitor`      | options configurator or configuration section | binds monitoring options     |
|  [03]   | `AddPublisher<T>`       | builder generic registration                  | admits utilization publisher |

[ENTRYPOINT_SCOPE]: monitor operations

- rail: observability

| [INDEX] | [SURFACE]          | [CALL_SHAPE]                  | [CAPABILITY]                               |
| :-----: | :----------------- | :---------------------------- | :----------------------------------------- |
|  [01]   | `GetUtilization`   | `TimeSpan` window             | computes utilization over the window       |
|  [02]   | `PublishAsync`     | utilization plus cancellation | delivers utilization to a publisher        |
|  [03]   | `GetResourceQuota` | quota read                    | reads the current container resource quota |

[ENTRYPOINT_SCOPE]: option policy

- rail: observability
- call shape: option property on `ResourceMonitoringOptions`

| [INDEX] | [SURFACE]                          | [CAPABILITY]                  |
| :-----: | :--------------------------------- | :---------------------------- |
|  [01]   | `CollectionWindow`                 | sample retention window       |
|  [02]   | `SamplingInterval`                 | snapshot cadence              |
|  [03]   | `PublishingWindow`                 | publisher aggregation window  |
|  [04]   | `CpuConsumptionRefreshInterval`    | CPU metric refresh cadence    |
|  [05]   | `MemoryConsumptionRefreshInterval` | memory metric refresh cadence |
|  [06]   | `UseLinuxCalculationV2`            | cgroup v2 CPU calculation     |
|  [07]   | `UseZeroToOneRangeForLinuxMetrics` | normalized Linux metric range |
|  [08]   | `EnableSystemDiskIoMetrics`        | system disk I/O instruments   |

## [04]-[IMPLEMENTATION_LAW]

[MONITOR_TOPOLOGY]:

- registration root: `AddResourceMonitoring` wires monitor service, snapshot provider, publishers, and the quota provider
- observable-instrument read model: the current path reads CPU/memory pressure off the meter `Microsoft.Extensions.Diagnostics.ResourceMonitoring`, instruments `process.cpu.utilization` and `dotnet.process.memory.virtual.ratio`, via a `MeterListener` over observable instruments — never the obsolete `IResourceMonitor.GetUtilization` pull
- quota model: `ResourceQuotaProvider.GetResourceQuota()` returns the current `ResourceQuota` carrying `MaxMemoryInBytes`/`MaxCpuInCores` and `BaselineMemoryInBytes`/`BaselineCpuInCores` ceilings so a container-row grade compares against the limit the process runs under, not the host total
- obsolete pull model: `IResourceMonitor.GetUtilization(window)` folds buffered snapshots into `ResourceUtilization` (usage percentages, used bytes, `SystemResources`, latest `Snapshot`) — obsolete, kept only as the migration source
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
