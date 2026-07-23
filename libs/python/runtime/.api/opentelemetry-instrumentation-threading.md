# [PY_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_THREADING]

`opentelemetry-instrumentation-threading` carries the active OTel context across in-process thread crossings: it wraps `threading.Thread.start`/`run`, `threading.Timer`, and `ThreadPoolExecutor` submission so a span opened on the submitting side parents the worker body. It emits no telemetry of its own — pure context carriage at zero export cost.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-instrumentation-threading`
- package: `opentelemetry-instrumentation-threading` (Apache-2.0)
- module: `opentelemetry.instrumentation.threading`
- namespaces: `opentelemetry.instrumentation.threading`
- rail: observability

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: instrumentor

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :---------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `ThreadingInstrumentor` | instrumentor  | cross-thread OTel context capture + activation |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: instrumentor lifecycle, instance methods on `ThreadingInstrumentor`

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :----------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `ThreadingInstrumentor().instrument(**kwargs)`   | instance | wrap thread, timer, and executor submission |
|  [02]   | `ThreadingInstrumentor().uninstrument(**kwargs)` | instance | unwrap the patched surfaces                 |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- carriage law: each patched submit captures the active context and re-activates it in the worker body, so a `to_thread` offload or executor pool inherits the live span with no per-call `attach` plumbing.
- signal law: zero signals emit — no span, metric, or log — so the patch composes under any provider state at zero export cost.

[STACKING]:
- `opentelemetry-api`(`.api/opentelemetry-api.md`): the wrap reads the active context via `context.get_current()` at `Thread.start`/`Timer`/`ThreadPoolExecutor.submit` and re-activates it in the worker body via token-paired `context.attach`/`context.detach`; this surface owns the thread-crossing carriage, `opentelemetry-api` owns the immutable `Context` and its attach/detach tokens.
- within-lib: the `anyio.to_thread` and executor offload crossings inherit the live span, while the pickled process and interpreter arms stay the explicit `Signals.attach` stitch owner's.

[LOCAL_ADMISSION]:
- `instrument()` fires once at the composition root; a thread crossing the patch already carries takes no per-call `contextvars` plumbing.

[RAIL_LAW]:
- Package: `opentelemetry-instrumentation-threading`
- Owns: OTel context carriage across in-process thread crossings
- Accept: one train-row `instrument()` at the composition root
- Reject: activation inside a library module, per-call `contextvars` plumbing for a thread crossing the patch already carries
