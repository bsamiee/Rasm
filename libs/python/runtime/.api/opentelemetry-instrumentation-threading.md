# [PY_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_THREADING]

`opentelemetry-instrumentation-threading` propagates the active OTel context across thread starts: it wraps `threading.Thread.start`/`run`, `threading.Timer`, and `ThreadPoolExecutor` submission so a span opened on the submitting side stays the parent inside the worker. It emits no telemetry of its own — pure context carriage for the offload crossings the worker fabric drives.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-instrumentation-threading`
- package: `opentelemetry-instrumentation-threading`
- module: `opentelemetry.instrumentation.threading`
- owner: `runtime`
- rail: observability
- asset: pure-Python runtime library over `wrapt`
- namespaces: `opentelemetry.instrumentation.threading`
- capability: context capture at `Thread.start`/`Timer` schedule and `ThreadPoolExecutor` submit, context activation inside the worker body, zero emitted signals

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: instrumentor
- rail: observability

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [RAIL]                                         |
| :-----: | :---------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `ThreadingInstrumentor` | instrumentor  | cross-thread OTel context capture + activation |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: instrumentor lifecycle
- rail: observability

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY] | [RAIL]                                      |
| :-----: | :----------------------------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `ThreadingInstrumentor().instrument(**kwargs)`   | enable         | wrap thread, timer, and executor submission |
|  [02]   | `ThreadingInstrumentor().uninstrument(**kwargs)` | disable        | unwrap the patched surfaces                 |

## [04]-[IMPLEMENTATION_LAW]

[OBSERVABILITY_TOPOLOGY]:
- carriage law: the wrap captures the submitting context and activates it in the worker body, so `anyio.to_thread` offloads and executor pools inherit the live span with no per-call `attach` plumbing; the explicit `Signals.attach` stitch stays the law for crossings this patch cannot see — the pickled process and interpreter arms.
- signal law: the instrumentor emits nothing — no span, metric, or log — so it composes under any provider state at zero export cost.

[RAIL_LAW]:
- Package: `opentelemetry-instrumentation-threading`
- Owns: OTel context carriage across in-process thread crossings
- Accept: one train-row `instrument()` at the composition root
- Reject: activation inside a library module, per-call `contextvars` plumbing for a thread crossing the patch already carries
