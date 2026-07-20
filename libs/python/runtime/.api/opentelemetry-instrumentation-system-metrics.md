# [PY_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_SYSTEM_METRICS]

`opentelemetry-instrumentation-system-metrics` registers host, process, and interpreter-GC observable instruments on the global meter from one config-selected metric roster over psutil. In this runtime it runs the sliced roster — `system.*` and `cpython.gc.*` alone — because the `rasm.process.*` gauges own the process family off the cached `MetricState` fold and one fact keeps one owner.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-instrumentation-system-metrics`
- package: `opentelemetry-instrumentation-system-metrics`
- module: `opentelemetry.instrumentation.system_metrics`
- owner: `runtime`
- rail: observability
- asset: pure-Python runtime library over `psutil`
- namespaces: `opentelemetry.instrumentation.system_metrics`
- capability: config-selected observable gauges/counters across the `system.*`, `process.*`, and `cpython.gc.*` families, per-metric label selection, and meter registration against the global or supplied `meter_provider`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: instrumentor
- rail: observability
- construction owns the roster: `SystemMetricsInstrumentor(labels: dict[str, str] | None = None, config: dict[str, list[str] | None] | None = None)` — each config key names one metric family member and its value the label subset (`None` = defaults); an omitted config runs the full default roster.

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [RAIL]                                              |
| :-----: | :-------------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `SystemMetricsInstrumentor` | instrumentor  | config-selected system/process/GC observable roster |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: instrumentor lifecycle
- rail: observability

| [INDEX] | [SURFACE]                                             | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :---------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `SystemMetricsInstrumentor(labels=None, config=None)` | construct      | roster + label policy fixed at construction     |
|  [02]   | `instrument(**kwargs)`                                | enable         | register observables; forwards `meter_provider` |
|  [03]   | `uninstrument(**kwargs)`                              | disable        | deregister the roster                           |

## [04]-[IMPLEMENTATION_LAW]

[OBSERVABILITY_TOPOLOGY]:
- slice law: the runtime train constructs with the `_SYSTEM_SLICE` config — `system.cpu.time`, `system.cpu.utilization`, `system.memory.usage`, `system.memory.utilization`, `system.swap.usage`, `system.swap.utilization`, `system.disk.io`, `system.disk.operations`, `system.disk.time`, `system.network.dropped.packets`, `system.network.errors`, `system.network.io`, `system.network.packets`, `system.thread_count`, `cpython.gc.collections`, `cpython.gc.collected_objects`, `cpython.gc.uncollectable_objects` — so the process family stays the metrics spine's cached `rasm.process.*` gauges and no fact carries two owners.
- callback law: the registered observables sample psutil on the export thread at collection; the sliced roster keeps that sampling to host-level reads the spine's cached fold does not carry.
- provider law: `meter_provider` defaults to the global provider the telemetry root installed.

[RAIL_LAW]:
- Package: `opentelemetry-instrumentation-system-metrics`
- Owns: host and interpreter-GC observable instruments under the train's config slice
- Accept: one train-row construction with `config=_SYSTEM_SLICE`, `instrument()` at the composition root
- Reject: the unsliced default roster beside the spine's process gauges, activation inside a library module, a hand-rolled psutil gauge for a family the slice already registers
