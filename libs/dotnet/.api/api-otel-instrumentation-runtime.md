# [RASM_API_OTEL_INSTRUMENTATION_RUNTIME]

`OpenTelemetry.Instrumentation.Runtime` admits the CLR's own health series onto a meter provider through one extension verb over `MeterProviderBuilder`. Runtime-owned instruments carry every identity, unit, and tag dimension, so this package mints nothing and exposes no knob; provider view rows are the whole shaping surface.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: admission seat, options carrier

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [CAPABILITY]                              |
| :-----: | :------------------------------- | :------------ | :---------------------------------------- |
|  [01]   | `MeterProviderBuilderExtensions` | class         | seats admission on `MeterProviderBuilder` |
|  [02]   | `RuntimeInstrumentationOptions`  | class         | member-free carrier, no policy slot       |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: metric admission — both overloads extend `MeterProviderBuilder` and return it for chaining.

| [INDEX] | [SURFACE]                                                          | [SHAPE] | [CAPABILITY]                               |
| :-----: | :----------------------------------------------------------------- | :------ | :----------------------------------------- |
|  [01]   | `AddRuntimeInstrumentation()`                                      | static  | subscribes the `System.Runtime` meter      |
|  [02]   | `AddRuntimeInstrumentation(Action<RuntimeInstrumentationOptions>)` | static  | identical subscription, delegate discarded |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One registration per meter provider seats the whole CLR series, and `AddMeter("System.Runtime")` is that same admission spelled directly.
- Both verbs branch on `Environment.Version.Major >= 9` at call time.
- At and above that floor the call subscribes the inbox `System.Runtime` meter and returns, mounting no package-owned series.
- Below that floor a package-owned meter carries the `process.runtime.dotnet.*` family under a `generation` tag.
- `RuntimeInstrumentationOptions` never constructs at the branch runtime floor, so its delegate is unreachable rather than ignored.
- Every series but `dotnet.exceptions` is an observable read at collection cadence; `dotnet.exceptions` counts on the first-chance hook, so a caught-and-handled throw increments it.
- Runtime-owned tags key the grain: `dotnet.gc.collections` carries `gc.heap.generation` over the closed generation set, `dotnet.exceptions` carries `error.type` unbounded — the one dimension a view row bounds.

[STACKING]:
- `OpenTelemetry`(`api-opentelemetry.md`): one builder row inside `WithMetrics`, where `AddView` shapes or drops a series by meter name or instrument name.
- `System.Diagnostics.Metrics`(`api-diagnostics-metrics.md`): the runtime mints `System.Runtime` through that surface, so every series obeys its instrument-identity and observable-collection law.
- `Microsoft.Extensions.Diagnostics.ResourceMonitoring`(`Rasm.AppHost/.api/api-resource-monitoring.md`): `dotnet.process.*` carries raw process CPU time, processor count, and working set; limit-relative container utilization rides that meter alone.
- `Rasm.AppHost/Observability/telemetry#TELEMETRY_IDENTITY`: `ForeignSource.SystemRuntime` holds `System.Runtime` as the metric-publishing foreign row — the kernel `TelemetrySource` roster carries minted Rasm scopes alone — and `ForeignSource.Admitting` folds every admitted key into one `AddMeter` span at `#SIGNAL_GOVERNANCE`, so the verb never enters the composition fence.

[LOCAL_ADMISSION]:
- Each provider wanting CLR series admits `System.Runtime` on its own vocabulary fold; a plugin load context minting its own provider carries its own row.
