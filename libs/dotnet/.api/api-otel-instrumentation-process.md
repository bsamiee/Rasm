# [RASM_API_OTEL_INSTRUMENTATION_PROCESS]

`OpenTelemetry.Instrumentation.Process` mounts the host process's own absolute resource series — memory, CPU seconds by mode, threads, uptime, and the platform handle count — through one zero-argument verb over `MeterProviderBuilder`. `AddProcessInstrumentation` constructs the observable instruments and subscribes their meter in one call, so this package publishes no options carrier and no meter-name row substitutes for it; provider view rows are the whole shaping surface.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: one admission seat; no options carrier ships, so every knob a caller reaches is a provider view row

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [CAPABILITY]                              |
| :-----: | :------------------------------- | :------------ | :---------------------------------------- |
|  [01]   | `MeterProviderBuilderExtensions` | class         | seats admission on `MeterProviderBuilder` |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: metric admission — one zero-argument verb extending `MeterProviderBuilder` and returning it for chaining.

| [INDEX] | [SURFACE]                     | [SHAPE] | [CAPABILITY]                                           |
| :-----: | :---------------------------- | :------ | :----------------------------------------------------- |
|  [01]   | `AddProcessInstrumentation()` | static  | constructs the process series and subscribes its meter |

- `AddProcessInstrumentation`: registration alone mints the instruments, so `AddMeter` on the meter name subscribes an empty scope.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `AddProcessInstrumentation` fixes instrument identity at construction, so one registration per process yields one copy of each series.
- Scope version and scope schema url mint with the meter and reach no knob, so a composition pinning its own coordinate governs its minted scopes and never this one.
- Scope schema urls carry independently per scope where resource urls fold through one merge, so a branch pin past this package's puts two coordinates on one wire and nothing raises — `api-otel-resources.md` `[TOPOLOGY]` owns the resource-side annihilation that has no scope-side mirror.
- Detector packages stamp the same coordinate this meter does, so resource identity and this scope agree at the pin the branch already declares.
- `process.memory.usage` reports the working set as an observable up-down counter in `By`.
- `process.memory.virtual` reports committed virtual memory as an observable up-down counter in `By`.
- `process.cpu.time` reports an observable counter in `s`, one measurement per `cpu.mode` value across `user` and `system`.
- `process.thread.count` reports live OS threads as an observable up-down counter in `{thread}`.
- `process.uptime` reports seconds since process start as an observable gauge in `s`.
- `process.windows.handle.count` mounts in `{handle}` on Windows and `process.unix.file_descriptor.count` in `{file_descriptor}` on Linux.
- Darwin mounts neither, so a panel keyed on either degrades visibly rather than reading a flat zero.
- Every callback opens and disposes a `System.Diagnostics.Process` snapshot, so cost tracks reader cadence, never call volume.
- `cpu.mode` is the one dimension any series carries and closes at two values, so the family needs no view cap.

[STACKING]:
- `OpenTelemetry`(`api-opentelemetry.md`): each series enters one `WithMetrics` builder row and shapes through `AddView` by meter or instrument name.
- `System.Diagnostics.Metrics`(`api-diagnostics-metrics.md`): `Meter.CreateObservable*` mints every series as a reader-sampled snapshot.
- `Microsoft.Extensions.Diagnostics.ResourceMonitoring`(`Rasm.AppHost/.api/api-resource-monitoring.md`): `dotnet.process.*` grades saturation alone.
- `OpenTelemetry.Resources.Process`(`api-otel-resources.md`): `process.pid` rides resource identity, so no series carries a process tag.
- `Rasm.AppHost/Observability/telemetry#SIGNAL_GOVERNANCE`: `SignalGovernance.Govern` mounts the verb on the always-on metrics arm.
- `Rasm.AppHost/Observability/telemetry#SIGNAL_GOVERNANCE`: `SignalGovernance.Views` reads these as foreign streams, keeping `cpu.mode`.

[LOCAL_ADMISSION]:
- `SignalGovernance.Govern` at the hosted root is the one registration seat per process.
- `PluginTelemetryHost` capsules admit kernel scopes alone, so no co-resident capsule double-reports one process.
