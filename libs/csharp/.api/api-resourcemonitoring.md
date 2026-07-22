# [RASM_API_RESOURCEMONITORING]

`Microsoft.Extensions.Diagnostics.ResourceMonitoring` owns the process/container utilization source: one registration mints the monitor, publishers consume snapshots on a declared cadence, and the package's meter carries the `process.*` and container series the health fold reads. It is a governed source, never a decision-maker — the runtime's health machines consume the signal, and no separate process-metrics package exists beside it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Diagnostics.ResourceMonitoring`
- package: `Microsoft.Extensions.Diagnostics.ResourceMonitoring`
- assembly: `Microsoft.Extensions.Diagnostics.ResourceMonitoring`
- namespace: `Microsoft.Extensions.Diagnostics.ResourceMonitoring`, `Microsoft.Extensions.DependencyInjection`
- meter: `Microsoft.Extensions.Diagnostics.ResourceMonitoring` — admitted by name at the signal root
- asset: runtime library
- rail: resource signals

## [02]-[PUBLIC_TYPES]

[MONITOR_TYPES]: monitor, snapshot, and policy
- rail: resource signals

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE]   | [CAPABILITY]                                          |
| :-----: | :-------------------------- | :--------------- | :---------------------------------------------------- |
|  [01]   | `IResourceMonitor`          | monitor contract | `GetUtilization(TimeSpan window)` on-demand snapshot  |
|  [02]   | `ResourceUtilization`       | snapshot value   | CPU/memory percentages, bytes, limits, and snapshot   |
|  [03]   | `SystemResources`           | limits value     | guaranteed and maximum CPU/memory the host grants     |
|  [04]   | `ResourceMonitoringOptions` | cadence policy   | cadence windows and posture toggles                   |
|  [05]   | `IResourceMonitorBuilder`   | builder contract | `Services` + `AddPublisher<T>()` + `ConfigureMonitor` |

`ResourceUtilization` projects `CpuUsedPercentage`, `MemoryUsedPercentage`, `MemoryUsedInBytes`, `SystemResources`, and `Snapshot`. `ResourceMonitoringOptions` carries `CollectionWindow`, `SamplingInterval`, and `PublishingWindow` with `CpuConsumptionRefreshInterval`, `MemoryConsumptionRefreshInterval`, `UseZeroToOneRangeForMetrics`, `UseZeroToOneRangeForLinuxMetrics`, `UseLinuxCalculationV2`, `EnableSystemDiskIoMetrics`, and `SourceIpAddresses`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration
- rail: resource signals

| [INDEX] | [SURFACE]               | [KIND]         | [CAPABILITY]                                                 |
| :-----: | :---------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `AddResourceMonitoring` | registration   | bare and `Action<IResourceMonitorBuilder>` overloads         |
|  [02]   | `AddPublisher<T>`       | publisher row  | cadence-coupled snapshot consumer                            |
|  [03]   | `ConfigureMonitor`      | cadence policy | `Action<ResourceMonitoringOptions>` or configuration section |
|  [04]   | `GetUtilization`        | snapshot read  | windowed utilization for gauge-style consumers               |

## [04]-[IMPLEMENTATION_LAW]

[RESOURCE_TOPOLOGY]:
- source root: `AddResourceMonitoring()` registers the monitor; the signal root admits the meter by name and the health machines consume — signals owned here, machines there
- publisher root: publishers record and return; the `SamplingInterval` bounds every derived signal's reaction time, and a publisher window wider than the consuming policy's period aliases

[STACKING]:
- `OpenTelemetry`(`api-opentelemetry.md`): `AddMeter("Microsoft.Extensions.Diagnostics.ResourceMonitoring")` is the admission row; view rows shape cardinality like any foreign meter.
- `OpenTelemetry.Instrumentation.Runtime`(`api-otel-instrumentation-runtime.md`): the rosters partition at the process boundary — CLR-interior series there, process/container utilization here.

[LOCAL_ADMISSION]:
- Limit-utilization and request-utilization stay distinct alarms — throttling-imminent versus under-provisioned; one collapsed percentage loses the distinction the orchestrator acts on, so the range toggles pin explicitly.
- Composition-root-only; the health fold consumes utilization as typed snapshot values, never raw counter polls.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Diagnostics.ResourceMonitoring`
- Owns: process and container utilization sourcing, cadence policy, and publisher fan-out
- Accept: one monitor registration with declared cadence and publisher rows
- Reject: a parallel process-metrics instrumentation package; counter polling loops beside the governed source
