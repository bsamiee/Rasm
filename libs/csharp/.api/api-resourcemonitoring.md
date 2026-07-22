# [RASM_API_RESOURCEMONITORING]

`Microsoft.Extensions.Diagnostics.ResourceMonitoring` sources process, container, disk, and network utilization as observable instruments on one meter: a single service-collection registration mints the platform snapshot source behind them, and metric shape rides an options policy the standard options rail binds. Windows and Linux carry the provider set, so the instrument roster a host publishes is a platform fact.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Diagnostics.ResourceMonitoring`
- package: `Microsoft.Extensions.Diagnostics.ResourceMonitoring`
- assembly: `Microsoft.Extensions.Diagnostics.ResourceMonitoring`
- namespace: `Microsoft.Extensions.Diagnostics.ResourceMonitoring`, `Microsoft.Extensions.DependencyInjection`
- meter: `Microsoft.Extensions.Diagnostics.ResourceMonitoring`
- asset: runtime library
- rail: resource signals

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: registration owner and metric-shaping policy

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY] | [CAPABILITY]                              |
| :-----: | :---------------------------------------------- | :------------ | :---------------------------------------- |
|  [01]   | `ResourceMonitoringServiceCollectionExtensions` | class         | root registration verb                    |
|  [02]   | `ResourceMonitoringOptions`                     | class         | range, calculation, and source-set policy |

[METER_ROSTER]: instruments the platform snapshot source mints on the package meter through `IMeterFactory`

| [INDEX] | [INSTRUMENT]                                | [KIND]                    | [CAPABILITY]                        |
| :-----: | :------------------------------------------ | :------------------------ | :---------------------------------- |
|  [01]   | `process.cpu.utilization`                   | `ObservableGauge`         | process CPU share                   |
|  [02]   | `dotnet.process.memory.virtual.utilization` | `ObservableGauge`         | process virtual-memory share        |
|  [03]   | `container.cpu.time`                        | `ObservableCounter`       | container CPU seconds, unit `s`     |
|  [04]   | `container.cpu.limit.utilization`           | `ObservableGauge`         | consumption against the CPU limit   |
|  [05]   | `container.cpu.request.utilization`         | `ObservableGauge`         | consumption against the CPU request |
|  [06]   | `container.memory.limit.utilization`        | `ObservableGauge`         | consumption against the mem limit   |
|  [07]   | `container.memory.request.utilization`      | `ObservableGauge`         | consumption against the mem request |
|  [08]   | `container.memory.usage`                    | `ObservableUpDownCounter` | container memory bytes, unit `By`   |
|  [09]   | `system.disk.io`                            | `ObservableCounter`       | disk bytes transferred              |
|  [10]   | `system.disk.io_time`                       | `ObservableCounter`       | disk activated time                 |
|  [11]   | `system.disk.operations`                    | `ObservableCounter`       | disk operations                     |
|  [12]   | `system.network.connections`                | `ObservableUpDownCounter` | TCP connections by state            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: root registration

| [INDEX] | [SURFACE]                                   | [SHAPE] | [CAPABILITY]                                  |
| :-----: | :------------------------------------------ | :------ | :-------------------------------------------- |
|  [01]   | `AddResourceMonitoring(IServiceCollection)` | static  | platform provider, meter, and instrument mint |

[OPTION_POLICY]: `ResourceMonitoringOptions` properties, bound at the composition root through the standard options rail

| [INDEX] | [PROPERTY]                         | [TYPE]         | [DEFAULT] | [EFFECT]                                       |
| :-----: | :--------------------------------- | :------------- | :-------- | :--------------------------------------------- |
|  [01]   | `UseZeroToOneRangeForMetrics`      | `bool`         | `false`   | utilization emits in `[0, 1]`                  |
|  [02]   | `UseZeroToOneRangeForLinuxMetrics` | `bool`         | `true`    | range switch over the Linux series             |
|  [03]   | `UseLinuxCalculationV2`            | `bool`         | `false`   | cgroup v2 CPU-limit delta replaces host delta  |
|  [04]   | `EnableSystemDiskIoMetrics`        | `bool`         | `false`   | admits the `system.disk.*` instruments         |
|  [05]   | `SourceIpAddresses`                | `ISet<string>` | empty     | Windows source-IPv4 filter on connection count |

- `EXTEXP0008` gates every option row but `SourceIpAddresses`; the consuming project admits that diagnostic id to compile them.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- registration root: `AddResourceMonitoring()` registers the monitor as an `IHostedService` ahead of its platform gate, so a host outside Windows and Linux faults at activation with no snapshot source behind it.
- container root: a Windows process inside a job object and a Linux process under a cgroup pick the container snapshot source, minting the `container.*` series beside the process gauges; a bare process publishes the process gauges alone.

[STACKING]:
- `OpenTelemetry`(`api-opentelemetry.md`): `AddMeter("Microsoft.Extensions.Diagnostics.ResourceMonitoring")` admits the roster onto a meter provider, and view rows shape or drop an individual instrument like any foreign meter.
- within-lib: the AppHost composition root registers the monitor once, and a `MeterListener` over the package meter projects the process and container gauges into the typed utilization samples the compute governor folds.

[LOCAL_ADMISSION]:
- Composition-root-only registration; every reader binds a `MeterListener` on the package meter.
- Limit utilization and request utilization stay distinct alarms — throttling-imminent against under-provisioned — so the range toggles pin explicitly at the root and both container series ride.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Diagnostics.ResourceMonitoring`
- Owns: process and container utilization sourcing on one meter, platform snapshot source included
- Accept: one root registration carrying an explicit range and calculation policy
- Reject: hand-rolled `/proc`, cgroup, or job-object sampling beside the governed meter
