# [PY_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_ASYNCIO]

`opentelemetry-instrumentation-asyncio` spans selected coroutines, futures, and `to_thread` callables on the event loop and carries the active context across their scheduling seams, emitting the `asyncio.process.duration` histogram and `asyncio.process.created` counter. Three environment variables own selection, so coverage tunes per deployment with zero code edit.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-instrumentation-asyncio`
- package: `opentelemetry-instrumentation-asyncio` (Apache-2.0)
- module: `opentelemetry.instrumentation.asyncio`
- namespaces: `opentelemetry.instrumentation.asyncio`, `opentelemetry.instrumentation.asyncio.environment_variables`
- rail: observability

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: instrumentor

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                         |
| :-----: | :-------------------- | :------------ | :--------------------------------------------------- |
|  [01]   | `AsyncioInstrumentor` | instrumentor  | selective asyncio tracing + the two loop instruments |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: instrumentor lifecycle and per-item wrap, all instance methods on `AsyncioInstrumentor`
- selection env vars (module `environment_variables`): `OTEL_PYTHON_ASYNCIO_COROUTINE_NAMES_TO_TRACE` (comma-set of coroutine names), `OTEL_PYTHON_ASYNCIO_FUTURE_TRACE_ENABLED` (bool), `OTEL_PYTHON_ASYNCIO_TO_THREAD_FUNCTION_NAMES_TO_TRACE` (comma-set of function names)

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                                    |
| :-----: | :----------------------------------------------- | :------- | :---------------------------------------------- |
|  [01]   | `AsyncioInstrumentor().instrument(**kwargs)`     | instance | patch gather/to_thread/TaskGroup creation       |
|  [02]   | `AsyncioInstrumentor().uninstrument(**kwargs)`   | instance | unwrap the patched surfaces                     |
|  [03]   | `trace_to_thread(func)`                          | instance | span one selected `to_thread` function          |
|  [04]   | `trace_item(coro_or_future)`                     | instance | span one selected coroutine or future           |
|  [05]   | `trace_coroutine(coro)` / `trace_future(future)` | instance | per-kind halves `trace_item` discriminates onto |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- selection law: an empty env selection traces nothing at zero cost; the instrument pair records only for selected items, so blanket every-coroutine tracing has no spelling.
- inheritance law: the serve loop crossings and any `asyncio.to_thread` inside admitted packages inherit the selected coverage, while anyio task-group interiors stay the lane owner's spans.
- provider law: `tracer_provider`/`meter_provider` default to the globals the telemetry root installed.

[STACKING]:
- `opentelemetry-api`(`.api/opentelemetry-api.md`): the `asyncio.process.duration` histogram and `asyncio.process.created` counter register through a `metrics.get_meter(...)` `Meter.create_histogram`/`create_counter`, and coroutine/future spans open through a `trace.get_tracer(...)` `Tracer.start_as_current_span`; both providers default to the globals `opentelemetry-sdk` installs at the telemetry root.
- within-lib: the anyio-driven `gather`/`create_task`/TaskGroup crossings inherit the selected-item coverage, while anyio task-group interiors stay the lane owner's spans.

[LOCAL_ADMISSION]:
- `instrument()` fires once at the composition root; coverage rides the three env vars as deployment config, never a code edit or a per-coroutine hand span.

[RAIL_LAW]:
- Package: `opentelemetry-instrumentation-asyncio`
- Owns: selected coroutine/future/`to_thread` spans, loop-crossing context carriage, and the `asyncio.process` duration/created instrument pair
- Accept: one train-row `instrument()` at the composition root, coverage tuned through the three env vars
- Reject: activation inside a library module, per-coroutine hand spans a selection row already covers
