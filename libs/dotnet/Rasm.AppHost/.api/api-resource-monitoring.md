# [RASM_APPHOST_API_RESOURCE_MONITORING]

`Microsoft.Extensions.Diagnostics.ResourceMonitoring` sources process, container, disk, and network utilization as observable instruments on one meter: one registration mints the platform snapshot source behind them, an options policy shapes range, calculation, roster, and cadence, and `ResourceQuotaProvider` reads the container ceilings those ratios grade against. Windows and Linux carry the provider set, so the roster a host publishes — and whether it publishes at all — is a platform fact.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: registration owner, shaping policy, and container-ceiling read

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :---------------------------------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `ResourceMonitoringServiceCollectionExtensions` | class         | root registration verb                         |
|  [02]   | `ResourceMonitoringOptions`                     | class         | range, calculation, roster, and refresh policy |
|  [03]   | `ResourceQuotaProvider`                         | class         | abstract container CPU and memory ceiling read |
|  [04]   | `ResourceQuota`                                 | class         | baseline and maximum CPU and memory ceilings   |

[UTILIZATION_ROSTER]: dimensionless instruments the snapshot providers mint — `gauge` is `ObservableGauge`, `updown` is `ObservableUpDownCounter`, `counter` is `ObservableCounter`, and the token after `·` is the UCUM unit the mint passes

| [INDEX] | [INSTRUMENT]                                | [LINUX]              | [WINDOWS_HOST] | [WINDOWS_CONTAINER] |
| :-----: | :------------------------------------------ | :------------------- | :------------- | :------------------ |
|  [01]   | `process.cpu.utilization`                   | `gauge · 1`, non-V2  | `gauge · —`    | `gauge · —`         |
|  [02]   | `dotnet.process.memory.virtual.utilization` | `gauge · 1`          | `gauge · —`    | `gauge · —`         |
|  [03]   | `container.cpu.time`                        | `gauge · 1`, V2 only | absent         | `counter · s`       |
|  [04]   | `container.cpu.limit.utilization`           | `gauge · 1`          | absent         | `gauge · —`         |
|  [05]   | `container.cpu.request.utilization`         | `gauge · 1`          | absent         | `gauge · —`         |
|  [06]   | `container.memory.limit.utilization`        | `gauge · 1`          | absent         | `gauge · —`         |
|  [07]   | `container.memory.request.utilization`      | `gauge · 1`          | absent         | `gauge · —`         |
|  [08]   | `container.memory.usage`                    | `updown · By`        | absent         | `updown · By`       |

[SYSTEM_ROSTER]: platform-uniform instruments the disk and network owners mint, dimensioned per measurement

| [INDEX] | [INSTRUMENT]                 | [KIND]  | [UNIT]         | [DIMENSIONS]                                              |
| :-----: | :--------------------------- | :------ | :------------- | :-------------------------------------------------------- |
|  [01]   | `system.disk.io`             | counter | `By`           | `disk.io.direction` `system.device`                       |
|  [02]   | `system.disk.operations`     | counter | `{operation}`  | `disk.io.direction` `system.device`                       |
|  [03]   | `system.disk.io_time`        | counter | `s`            | `system.device`                                           |
|  [04]   | `system.network.connections` | updown  | `{connection}` | `network.transport` `network.type` `system.network.state` |

- `EnableSystemDiskIoMetrics` admits rows [01]-[03]; the disk owner registers on both platforms and publishes nothing while the flag is off.
- `system.network.connections` stamps `network.transport` = `tcp` at the instrument and the remaining two keys per measurement, and the Windows roster publishes twelve TCP states to Linux's eleven, adding `delete`.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration, shaping, and quota read

| [INDEX] | [SURFACE]                                                                 | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :------------------------------------------------------------------------ | :------- | :-------------------------------------------- |
|  [01]   | `AddResourceMonitoring(IServiceCollection) -> IServiceCollection`         | static   | platform provider, meter, and instrument mint |
|  [02]   | `Configure<ResourceMonitoringOptions>(Action<ResourceMonitoringOptions>)` | static   | the one live shaping path                     |
|  [03]   | `ResourceQuotaProvider.GetResourceQuota() -> ResourceQuota`               | instance | container CPU and memory ceilings             |
|  [04]   | `ResourceQuota.MaxCpuInCores` `.MaxMemoryInBytes`                         | property | limit ceilings, `double` and `ulong`          |
|  [05]   | `ResourceQuota.BaselineCpuInCores` `.BaselineMemoryInBytes`               | property | request ceilings, `double` and `ulong`        |

- `AddResourceMonitoring`: returns the same collection after `AddMetrics()` on a host outside Windows and Linux, so a null-free return proves nothing about whether an instrument exists.
- `ResourceQuota`: every ceiling is a settable property on a resolved instance, so a reader copies the values it grades against rather than holding the provider's object.

[ENTRYPOINT_SCOPE]: option policy — each row a `ResourceMonitoringOptions` property bound through the standard options pattern

| [INDEX] | [SURFACE]                          | [GATE]       | [DEFAULT] | [CAPABILITY]                                           |
| :-----: | :--------------------------------- | :----------- | :-------- | :----------------------------------------------------- |
|  [01]   | `UseZeroToOneRangeForMetrics`      | `EXTEXP0008` | `false`   | `[0, 1]` range on the Windows host and container arms  |
|  [02]   | `UseZeroToOneRangeForLinuxMetrics` | `EXTEXP0008` | `true`    | `[0, 1]` range on the Linux arm                        |
|  [03]   | `UseLinuxCalculationV2`            | `EXTEXP0008` | `false`   | cgroup v2 CPU-limit delta replaces the host delta      |
|  [04]   | `EnableSystemDiskIoMetrics`        | `EXTEXP0008` | `false`   | admits the `system.disk.*` instruments                 |
|  [05]   | `SourceIpAddresses`                | —            | empty     | Windows local-address filter; empty admits every row   |
|  [06]   | `CpuConsumptionRefreshInterval`    | `EXTOBS0001` | `5 s`     | snapshot re-read behind every CPU gauge                |
|  [07]   | `MemoryConsumptionRefreshInterval` | `EXTOBS0001` | `5 s`     | snapshot re-read behind every memory gauge             |
|  [08]   | `SamplingInterval`                 | `EXTOBS0001` | `1 s`     | TCP-state snapshot refresh behind the connection count |

- Rows [06]-[08] govern the live observable read path while carrying `EXTOBS0001`, so a project tuning cadence admits that id beside the `EXTEXP0008` rows [01]-[04] need.
- Validation bounds rows [06]-[07] to `[100 ms, 900 s]` and row [08] to `[1 ms, 900 s]`, holds `SourceIpAddresses` non-null, and faults a breach at start through the validate-on-start seat the registration installs.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- registration root: `AddResourceMonitoring()` adds the metrics services, then RETURNS on any host that is neither Windows nor Linux — no snapshot source, no instruments, no `ResourceQuotaProvider`, and no fault — so a darwin host reads a permanently-empty meter and a consumer needing utilization everywhere carries its own BCL-counter arm.
- provider selection: Windows picks the container snapshot source inside a job object and the host source outside it, while Linux mounts ONE provider publishing the `container.*` series for every process, cgroup-hosted or bare — so a container series is evidence of a Linux host, never of a container.
- quota registration: Linux registers `ResourceQuotaProvider` unconditionally, Windows only inside a job object, and darwin never — so a container-limit grade resolves its ceilings on any Linux host and a Windows process outside a job object resolves none.
- conditional roster: under `UseLinuxCalculationV2` the Linux provider publishes the container CPU triple and NO `process.cpu.utilization`, while the non-V2 arm scales that process gauge against the cpu REQUEST — so a limit-relative CPU grade reads `container.cpu.limit.utilization` on either arm.
- range semantics: the emitted multiplier is `1.0` or `100.0` — the Linux provider selects it from `UseZeroToOneRangeForLinuxMetrics` (default on) and the Windows host and container providers from `UseZeroToOneRangeForMetrics` (default OFF), so a ratio-shaped ceiling compared against an unflipped Windows series breaches at one percent load.
- process-relative memory is Linux-absent: the Linux provider feeds `dotnet.process.memory.virtual.utilization` from the same container memory-limit ratio it publishes as `container.memory.limit.utilization`, so those two names carry ONE series there and only the Windows arms make the process gauge process-relative.
- kind and unit divergence: `container.cpu.time` mints as an `ObservableCounter` in `s` on the Windows container arm and an `ObservableGauge` in `1` on Linux, and the Windows utilization gauges pass no unit where the Linux ones pass `1` — so a view, aggregation, or panel keyed on instrument kind or unit forks across the platform halves of one name.
- refresh cadence: `CpuConsumptionRefreshInterval` and `MemoryConsumptionRefreshInterval` gate the snapshot re-read behind every utilization gauge and `SamplingInterval` gates the TCP-state snapshot behind the connection count, so an observation returns a value at most one interval old and a faster observer reads the same cached sample, while the publisher-window rows reach nothing on this path.

[STACKING]:
- `OpenTelemetry`(`libs/dotnet/.api/api-opentelemetry.md`): `AddMeter("Microsoft.Extensions.Diagnostics.ResourceMonitoring")` on `MeterProviderBuilder` admits the whole roster, and `AddView` with a `MetricStreamConfiguration` drops or reshapes an individual instrument as on any foreign meter — the one seat normalizing the platform-divergent kind and unit of `container.cpu.time` before export.
- `ProbeSource.Gauge.Mount` registers the monitor once under `HealthSurface.Register`, pinning both range flips and reading `UseLinuxCalculationV2`/`EnableSystemDiskIoMetrics` off `PressurePolicy.Features`, `PressureSource.Container`/`.Host` name the gauge pair each arm grades, `UtilizationCell` binds the `MeterListener` on the package meter and drives the observation, and `PressurePolicy.Quota` carries the `ResourceQuotaProvider.GetResourceQuota()` ceilings the grade detail stamps.

[LOCAL_ADMISSION]:
- Registration is composition-root-only; every reader binds a `MeterListener` on the package meter and observes through `MeterListener.RecordObservableInstruments()`, which a bare `Start()` never does.
- Each consumed instrument name lands as a declared const at the reading owner because the package's name holder is internal; a call-site literal is the deleted form.
- Limit utilization and request utilization stay distinct grades — throttling-imminent against under-provisioned — so both container series ride and the range flips pin explicitly at the root.
- Diagnostic-gated option rows compile only where the consuming project admits their ids, so admission is a project-manifest fact rather than a per-call suppression.
