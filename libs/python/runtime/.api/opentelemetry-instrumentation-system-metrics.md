# [PY_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_SYSTEM_METRICS]

`opentelemetry-instrumentation-system-metrics` registers host and interpreter-GC observable instruments on the global meter from a config-selected metric roster over `psutil`. This runtime constructs it against a slice dropping the `process.*` family the metrics spine's cached `rasm.process.*` gauges own.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-instrumentation-system-metrics`
- package: `opentelemetry-instrumentation-system-metrics` (Apache-2.0)
- module: `opentelemetry.instrumentation.system_metrics`
- rail: observability

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: instrumentor

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                |
| :-----: | :-------------------------- | :------------ | :------------------------------------------ |
|  [01]   | `SystemMetricsInstrumentor` | class         | config-selected system/GC observable roster |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: instrumentor lifecycle

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]                                    |
| :-----: | :---------------------------------------------------- | :------- | :---------------------------------------------- |
|  [01]   | `SystemMetricsInstrumentor(labels=None, config=None)` | ctor     | fix roster and label policy at construction     |
|  [02]   | `instrument(**kwargs)`                                | instance | register observables; forwards `meter_provider` |
|  [03]   | `uninstrument(**kwargs)`                              | instance | deregister the roster                           |

- `SystemMetricsInstrumentor`: each `config` key names one metric-family member and its value the label subset, `None` selecting the member defaults; an omitted `config` runs the full default roster.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Runtime construction pins a `config` slice excluding the `process.*` family and carrying the `system.*` and `cpython.gc.*` families; the metrics spine's cached `rasm.process.*` gauges own the process family off one meter.
- Registered observables sample `psutil` on the export thread at collection, so the slice holds sampling to host reads the spine's cached fold does not carry.

[STACKING]:
- `psutil`(`.api/psutil.md`): each observable callback samples `psutil` host reads — `virtual_memory`, `swap_memory`, `disk_io_counters`, `net_io_counters`, `cpu_times` — on the export thread at collection; `psutil` is the metric source, this roster the registration edge.
- `opentelemetry-api`(`.api/opentelemetry-api.md`): the sliced roster registers observable gauges and counters against the global `Meter` through `create_observable_gauge` and `create_observable_counter`; `meter_provider` defaults to the provider the telemetry root installed, and the `rasm.process.*` spine owns the process family off this same meter.

[LOCAL_ADMISSION]:
- Construct one train row with the host and GC slice and call `instrument()` at the composition root; a library module never activates the roster.

[RAIL_LAW]:
- Package: `opentelemetry-instrumentation-system-metrics`
- Owns: host and interpreter-GC observable instruments under the train's config slice
- Accept: one train-row construction with the host and GC `config` slice, `instrument()` at the composition root
- Reject: the unsliced default roster beside the spine's process gauges, a hand-rolled `psutil` gauge for a family the slice already registers
