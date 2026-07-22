# [RASM_API_OTEL_INSTRUMENTATION_RUNTIME]

`OpenTelemetry.Instrumentation.Runtime` admits the CLR's own health series onto a meter provider through one extension verb over `MeterProviderBuilder`. Runtime-owned instruments carry every identity, unit, and tag dimension, so this package mints nothing and exposes no knob; provider view rows are the whole shaping surface.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry.Instrumentation.Runtime`
- package: `OpenTelemetry.Instrumentation.Runtime`
- assembly: `OpenTelemetry.Instrumentation.Runtime`
- namespace: `OpenTelemetry.Instrumentation.Runtime`, `OpenTelemetry.Metrics`
- meter: `System.Runtime` — runtime-owned, admitted by name
- rail: runtime instrumentation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: admission seat, options carrier

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [CAPABILITY]                              |
| :-----: | :------------------------------- | :------------ | :---------------------------------------- |
|  [01]   | `MeterProviderBuilderExtensions` | class         | seats admission on `MeterProviderBuilder` |
|  [02]   | `RuntimeInstrumentationOptions`  | class         | member-free carrier, no policy slot       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: metric admission — both overloads extend `MeterProviderBuilder` and return it for chaining.

| [INDEX] | [SURFACE]                                                          | [SHAPE] | [CAPABILITY]                               |
| :-----: | :----------------------------------------------------------------- | :------ | :----------------------------------------- |
|  [01]   | `AddRuntimeInstrumentation()`                                      | static  | subscribes the `System.Runtime` meter      |
|  [02]   | `AddRuntimeInstrumentation(Action<RuntimeInstrumentationOptions>)` | static  | identical subscription, delegate discarded |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One registration per meter provider seats the whole CLR series, and `AddMeter("System.Runtime")` is that same admission spelled directly.
- Every series but `dotnet.exceptions` is an observable read at collection cadence; `dotnet.exceptions` counts on the first-chance hook, so a caught-and-handled throw increments it.
- Runtime-owned tags key the grain: `dotnet.gc.collections` carries `gc.heap.generation` over the closed generation set, `dotnet.exceptions` carries `error.type` unbounded — the one dimension a view row bounds.

[STACKING]:
- `OpenTelemetry`(`api-opentelemetry.md`): one builder row inside `WithMetrics`, where `AddView` shapes or drops a series by meter name or instrument name.
- `System.Diagnostics.Metrics`(`api-diagnostics-metrics.md`): the runtime mints `System.Runtime` through that surface, so every series obeys its instrument-identity and observable-collection law.
- `Microsoft.Extensions.Diagnostics.ResourceMonitoring`(`api-resourcemonitoring.md`): `dotnet.process.*` carries raw process CPU time, processor count, and working set; limit-relative container utilization rides that meter alone.
- `Rasm.AppHost` telemetry spine: `TelemetrySource.SystemRuntime` holds `System.Runtime` as an unminted vocabulary row and the meter fold admits every row key in one `AddMeter` span, so the verb never enters the composition fence.

[LOCAL_ADMISSION]:
- Each provider wanting CLR series admits `System.Runtime` on its own vocabulary fold; a plugin load context minting its own provider carries its own row.

[RAIL_LAW]:
- Package: `OpenTelemetry.Instrumentation.Runtime`
- Owns: CLR runtime-series admission onto a meter provider
- Accept: one meter-name subscription per provider, shaped downstream by view rows
- Reject: a hand-rolled GC, JIT, or thread-pool poller beside the runtime meter; an options delegate treated as a policy slot
