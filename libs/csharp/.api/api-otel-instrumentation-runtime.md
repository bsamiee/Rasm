# [RASM_API_OTEL_INSTRUMENTATION_RUNTIME]

`OpenTelemetry.Instrumentation.Runtime` admits the CLR's own health series — GC, JIT, thread pool, assembly, and exception counters — onto a meter provider through one registration verb. Process-level `process.*` series stay with `Microsoft.Extensions.Diagnostics.ResourceMonitoring`, which feeds the health fold; this package owns the runtime-interior series only.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry.Instrumentation.Runtime`
- package: `OpenTelemetry.Instrumentation.Runtime`
- assembly: `OpenTelemetry.Instrumentation.Runtime`
- namespace: `OpenTelemetry.Instrumentation.Runtime`, `OpenTelemetry.Metrics`
- asset: runtime library
- rail: runtime instrumentation

## [02]-[PUBLIC_TYPES]

[OPTION_TYPES]: registration surface
- rail: runtime instrumentation

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE]   | [CAPABILITY]                              |
| :-----: | :------------------------------ | :--------------- | :---------------------------------------- |
|  [01]   | `RuntimeInstrumentationOptions` | options carrier  | knob-free on the modern runtime           |
|  [02]   | `RuntimeMetrics`                | instrument owner | disposable holder behind the registration |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: admission
- rail: runtime instrumentation

| [INDEX] | [SURFACE]                   | [KIND]           | [CAPABILITY]                                                             |
| :-----: | :-------------------------- | :--------------- | :----------------------------------------------------------------------- |
|  [01]   | `AddRuntimeInstrumentation` | metric admission | `MeterProviderBuilder`, optional `Action<RuntimeInstrumentationOptions>` |

## [04]-[IMPLEMENTATION_LAW]

[RUNTIME_TOPOLOGY]:
- root: one `AddRuntimeInstrumentation()` per meter provider; the runtime itself also emits the in-box `System.Runtime` meter, admitted by name where the estate wants the semconv-native series without this package's shim names

[STACKING]:
- `OpenTelemetry`(`api-opentelemetry.md`): a builder row inside `WithMetrics`; view rows shape or drop individual runtime series.
- `Microsoft.Extensions.Diagnostics.ResourceMonitoring`(`api-resourcemonitoring.md`): owns `process.*` utilization into the health rail — the two rosters partition at the process boundary, runtime-interior here, process-exterior there.

[LOCAL_ADMISSION]:
- Composition-root-only; each plugin load context that wants runtime series registers on its own provider.

[RAIL_LAW]:
- Package: `OpenTelemetry.Instrumentation.Runtime`
- Owns: CLR-interior metric admission at the composition root
- Accept: one registration per meter provider
- Reject: a process-metrics package beside ResourceMonitoring; per-library runtime-counter polling
