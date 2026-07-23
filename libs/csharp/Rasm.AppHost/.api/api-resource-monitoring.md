# [RASM_APPHOST_API_RESOURCE_MONITORING]

`Microsoft.Extensions.Diagnostics.ResourceMonitoring` admits observable process, container, disk-I/O, and network instruments, container-aware resource quota reads, and metric-shaping option policy into the observability rail. Its read surface is the package meter's observable instruments, sampled through a `MeterListener` or the OpenTelemetry `MeterProvider`; domain code never reads utilization directly.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Diagnostics.ResourceMonitoring`
- package: `Microsoft.Extensions.Diagnostics.ResourceMonitoring` (MIT)
- assembly: `Microsoft.Extensions.Diagnostics.ResourceMonitoring`
- namespace: `Microsoft.Extensions.Diagnostics.ResourceMonitoring`, `Microsoft.Extensions.DependencyInjection`
- asset: runtime library
- rail: observability

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: quota and option contracts

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                               |
| :-----: | :-------------------------- | :------------ | :----------------------------------------- |
|  [01]   | `ResourceQuotaProvider`     | class         | current container CPU/memory quota read    |
|  [02]   | `ResourceQuota`             | class         | baseline and maximum memory/CPU quota      |
|  [03]   | `ResourceMonitoringOptions` | class         | metric-shaping flags and telemetry sources |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration and quota read

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :---------------------------------------------------------- | :------- | :-------------------------------------------------- |
|  [01]   | `AddResourceMonitoring()`                                   | static   | registers OS quota provider, meter, and instruments |
|  [02]   | `ResourceQuotaProvider.GetResourceQuota() -> ResourceQuota` | instance | current container CPU/memory quota ceilings         |

[ENTRYPOINT_SCOPE]: observable instruments

A `MeterListener` samples the meter `Microsoft.Extensions.Diagnostics.ResourceMonitoring` over these observable instruments.

| [INDEX] | [INSTRUMENT]                                | [KIND]                    | [CAPABILITY]                                     |
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

[ENTRYPOINT_SCOPE]: option policy

`EXTEXP0008` admits the four gated rows a consumer opts into to compile them; `SourceIpAddresses` binds unconditionally. Every row is a property on `ResourceMonitoringOptions` bound through the options pattern.

| [INDEX] | [SURFACE]                          | [GATE]        | [CAPABILITY]                                     |
| :-----: | :--------------------------------- | :------------ | :----------------------------------------------- |
|  [01]   | `UseLinuxCalculationV2`            | `EXTEXP0008`  | cgroup v2 CPU-limit calculation on Linux         |
|  [02]   | `UseZeroToOneRangeForLinuxMetrics` | `EXTEXP0008`  | normalized `[0, 1]` Linux metric range           |
|  [03]   | `UseZeroToOneRangeForMetrics`      | `EXTEXP0008`  | normalized `[0, 1]` metric range all platforms   |
|  [04]   | `EnableSystemDiskIoMetrics`        | `EXTEXP0008`  | system disk I/O instruments                      |
|  [05]   | `SourceIpAddresses`                | unconditional | Windows source-IPv4 set for connection telemetry |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- registration root: `AddResourceMonitoring()` wires the OS-appropriate quota provider, meter, and observable instruments; a cgroup- or job-hosted process picks the container providers, a bare process the host providers.
- read model: the read path samples the meter `Microsoft.Extensions.Diagnostics.ResourceMonitoring` through a `MeterListener` over its observable instruments — process share off `process.cpu.utilization`/`dotnet.process.memory.virtual.utilization`, container pressure off the `container.*` gauges, I/O off `system.disk.*`/`system.network.connections`.
- cgroup vs host semantics: `container.*` instruments report against the cgroup limit/request the process runs under, process instruments report the raw process share, and `UseLinuxCalculationV2` selects the cgroup v2 CPU-limit delta over the host-CPU delta on Linux.
- quota model: `ResourceQuotaProvider.GetResourceQuota()` returns the current `ResourceQuota` carrying `MaxMemoryInBytes`/`MaxCpuInCores` and `BaselineMemoryInBytes`/`BaselineCpuInCores` ceilings, so a container-row grade compares against the limit the process runs under, not the host total.
- platform model: Linux and Windows quota sources stay internal behind `ResourceQuotaProvider`.

[STACKING]:
- `api-otel`(`.api/api-otel.md`): the `Microsoft.Extensions.Diagnostics.ResourceMonitoring` meter binds onto `MeterProviderBuilder` through `AddMeter`, projecting every observable instrument onto the OTel metric pipeline.
- Observability composition root (`.planning/Observability/telemetry.md`): folds the meter name into the `AddMeter` source roster once, and the in-process `MeterListener` read model samples the same observable instruments for host-row grading.

[LOCAL_ADMISSION]:
- Resource monitoring is host-level observability; domain code never reads utilization directly.
- Instrument reads flow through a `MeterListener` on the package meter.
- Option policy binds at composition through the options pattern on `ResourceMonitoringOptions`.
- Utilization evidence is read-only; throttling decisions live in owning policy surfaces.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Diagnostics.ResourceMonitoring`
- Owns: process and container resource utilization evidence
- Accept: observable-instrument reads and container quota reads
- Reject: hand-rolled `/proc`, cgroup, or job-object sampling
