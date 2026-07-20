# [PY_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_ASYNCIO]

`opentelemetry-instrumentation-asyncio` traces selected coroutines, futures, and `to_thread` functions on the asyncio loop and carries context across their scheduling seams, with two duration instruments of its own. Selection is deployment-owned through three environment variables, so tracing depth tunes per process with zero code edits — the anyio-driven runtime composes it for the loop-level crossings the thread patch cannot see.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-instrumentation-asyncio`
- package: `opentelemetry-instrumentation-asyncio`
- module: `opentelemetry.instrumentation.asyncio`
- owner: `runtime`
- rail: observability
- asset: pure-Python runtime library
- namespaces: `opentelemetry.instrumentation.asyncio`, `opentelemetry.instrumentation.asyncio.environment_variables`
- capability: selective coroutine/future/`to_thread` tracing over `gather`, `create_task`, and TaskGroup creation, env-var-selected coverage, and the `asyncio.process.duration` (s) histogram + `asyncio.process.created` (`{process}`) counter pair

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: instrumentor
- rail: observability

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [RAIL]                                               |
| :-----: | :-------------------- | :------------ | :--------------------------------------------------- |
|  [01]   | `AsyncioInstrumentor` | instrumentor  | selective asyncio tracing + the two loop instruments |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: instrumentor lifecycle
- rail: observability
- selection env vars (module `environment_variables`): `OTEL_PYTHON_ASYNCIO_COROUTINE_NAMES_TO_TRACE` (comma-set of coroutine names), `OTEL_PYTHON_ASYNCIO_FUTURE_TRACE_ENABLED` (bool), `OTEL_PYTHON_ASYNCIO_TO_THREAD_FUNCTION_NAMES_TO_TRACE` (comma-set of function names).

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY] | [RAIL]                                              |
| :-----: | :----------------------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `AsyncioInstrumentor().instrument(**kwargs)`     | enable         | patch gather/to_thread/TaskGroup creation           |
|  [02]   | `AsyncioInstrumentor().uninstrument(**kwargs)`   | disable        | unwrap the patched surfaces                         |
|  [03]   | `trace_to_thread(func)`                          | wrap           | span one selected `to_thread` function              |
|  [04]   | `trace_item(coro_or_future)`                     | wrap           | span one selected coroutine or future               |
|  [05]   | `trace_coroutine(coro)` / `trace_future(future)` | wrap           | the per-kind halves `trace_item` discriminates onto |

## [04]-[IMPLEMENTATION_LAW]

[OBSERVABILITY_TOPOLOGY]:
- selection law: an empty env selection traces nothing and costs nothing — coverage is a deployment decision, and the metrics pair records only for selected items; blanket every-coroutine tracing has no spelling.
- placement law: one train-row `instrument()` at the composition root; the serve daemon's loop crossings and any `asyncio.to_thread` inside admitted packages inherit the coverage, while the anyio task-group interiors stay the lane owner's spans.
- provider law: `tracer_provider`/`meter_provider` default to the globals the telemetry root installed.

[RAIL_LAW]:
- Package: `opentelemetry-instrumentation-asyncio`
- Owns: selected-coroutine spans, loop-crossing context carriage, and the asyncio duration/created instrument pair
- Accept: one train-row `instrument()` at the composition root, coverage tuned through the three env vars
- Reject: activation inside a library module, per-coroutine hand spans a selection row already covers
