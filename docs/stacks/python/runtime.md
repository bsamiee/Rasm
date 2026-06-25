# [PYTHON_RUNTIME]

This page is the primitive-selection law for execution and observation: interpreter isolation and free-threading, the runtime-observation surfaces, and the structured-emission spine that joins them. The split between this page and the rail is fixed: `anyio` owns every async scheduling, deadline, cancellation, and structured-concurrency surface on `rails-and-effects.md`, and the manifest bans the `asyncio` import outright; this page owns only the isolation, observation, and emission primitives `anyio` does not re-export. Each section names the form and the spelling it replaces, and the family card states the placement law.

## [01]-[CONCURRENCY_AND_INTERPRETERS]

Mutation ownership and context propagation are explicit before code relies on scheduling or cache behavior. Structured concurrency — task groups, deadlines, cancellation, blocking-call offload — is owned by `rails-and-effects.md`; this page owns the isolation and free-threading surfaces beneath the rail and the few execution primitives `anyio` does not re-export.

| [INDEX] | [CONCERN]              | [USE]                                                    | [REPLACE]                            |
| :-----: | :--------------------- | :------------------------------------------------------- | :----------------------------------- |
|  [01]   | interpreter isolation  | `concurrent.interpreters.create`                         | process-only isolation wrappers      |
|  [02]   | own-GIL call dispatch  | `Interpreter.call` / `Interpreter.call_in_thread`        | pickled `exec`-string round trips    |
|  [03]   | cross-interpreter pipe | `concurrent.interpreters.create_queue`                   | shared module-global handoff         |
|  [04]   | subinterpreter pool    | `concurrent.futures.InterpreterPoolExecutor`             | process-only CPU pools               |
|  [05]   | process-pool stop      | `ProcessPoolExecutor.terminate_workers` / `kill_workers` | private worker traversal             |
|  [06]   | process interrupt      | `multiprocessing.Process.interrupt`                      | cleanup-hostile `terminate`          |
|  [07]   | worker sizing          | `os.process_cpu_count`                                   | `os.cpu_count` worker counts         |
|  [08]   | GIL-state probe        | `sys._is_gil_enabled`                                    | platform-string free-threading guess |
|  [09]   | iterator sharing       | `threading.serialize_iterator`                           | generator lock wrappers              |
|  [10]   | cross-thread context   | `contextvars.copy_context().run`                         | thread-local async state carry       |

[FREE_THREADING]:
- Use when: shared mutation, context propagation, or a supported-target claim depends on free-threaded execution.
- Accept: free-threaded Python as a supported target, explicit synchronization for shared mutation, `ContextVar` for async or thread context, and `sys._is_gil_enabled` as the runtime free-threading probe.
- Reject: experimental no-GIL caveats, implicit GIL serialization, thread-local async state, mutable ambient globals, and import-time singleton mutation as coordination.
- Law: free-threaded code makes mutation ownership and context propagation explicit before relying on scheduling or cache behavior; an `RLock` guards only the shared-mutable cell a free-threaded build no longer serializes by GIL, and `copy_context().run` carries the caller's `ContextVar` state across the guard.
- Boundary: the async-scheduling axis — task groups, deadlines, cancellation — is owned by `anyio` on the rails page, never by a free-threading or `asyncio` surface here.

[INTERPRETER_ISOLATION]:
- Use when: interpreter isolation, independent execution, or own-GIL CPU separation owns the runtime boundary, below the process boundary.
- Accept: `concurrent.interpreters.create` (each subinterpreter owns its GIL), `Interpreter.call(fn, *args, **kwargs)` to dispatch a real callable into the isolate, `Interpreter.call_in_thread` for the non-blocking form, and `create_queue` for shareable-object transport across the boundary.
- Law: the isolation strategy is a `Lane` policy value keying the `frozendict` table to its execution arm — own-GIL subinterpreter, subinterpreter pool, or free-threaded lock — so a new isolation lane is one row, and a failed cross-interpreter call surfaces as `ExecutionFailed` mapped to the rail's `Error` through `excinfo.type.__name__`, never raised through the wrapped function; the table-dispatch and weave-fold mechanics are settled at their surface owner and composed here, not re-derived.
- Reject: a process-pool wrapper where interpreter isolation is the owner; a pickled `exec` string where `Interpreter.call` dispatches the callable directly; shared mutable module state read across an interpreter boundary; a `Lane` smuggled in as a `lane: str` knob the body re-pairs to a strategy the value already selects.
- Boundary: process isolation survives only where the process boundary is the actual requirement; the `create()` interpreter is the lighter isolate when shared address space is acceptable.

```python conceptual
import os
from collections.abc import Callable
from concurrent import interpreters
from concurrent.futures import InterpreterPoolExecutor
from contextvars import copy_context
from enum import StrEnum
from functools import wraps
from threading import RLock

from builtins import frozendict
from expression import Error, Ok, Result


class Lane(StrEnum):
    ISOLATE = "<lane-a>"
    POOL = "<lane-b>"
    LOCKED = "<lane-c>"


def _isolate[**P, T](operation: Callable[P, T], gate: RLock, /, *args: P.args, **kwargs: P.kwargs) -> Result[T, str]:
    try:
        runtime = interpreters.create()
        try:
            return Ok(runtime.call(operation, *args, **kwargs))
        finally:
            runtime.close()
    except interpreters.ExecutionFailed as fault:
        return Error(f"<isolate:{fault.excinfo.type.__name__}>")


def _pooled[**P, T](operation: Callable[P, T], gate: RLock, /, *args: P.args, **kwargs: P.kwargs) -> Result[T, str]:
    with InterpreterPoolExecutor(max_workers=os.process_cpu_count()) as pool:
        return Ok(pool.submit(operation, *args, **kwargs).result())


def _locked[**P, T](operation: Callable[P, T], gate: RLock, /, *args: P.args, **kwargs: P.kwargs) -> Result[T, str]:
    with gate:
        return Ok(copy_context().run(operation, *args, **kwargs))


STRATEGY: frozendict[Lane, Callable[..., Result[object, str]]] = frozendict(
    {Lane.ISOLATE: _isolate, Lane.POOL: _pooled, Lane.LOCKED: _locked}
)


def isolated[**P, T](lane: Lane = Lane.ISOLATE, /) -> Callable[[Callable[P, T]], Callable[P, Result[T, str]]]:
    run, gate = STRATEGY[lane], RLock()

    def weave(operation: Callable[P, T], /) -> Callable[P, Result[T, str]]:
        @wraps(operation)
        def call(*args: P.args, **kwargs: P.kwargs) -> Result[T, str]:
            return run(operation, gate, *args, **kwargs)

        return call

    return weave
```

## [02]-[DIAGNOSTICS_AND_OBSERVABILITY]

Observation reads runtime-owned surfaces and emits through one structured spine; the active exception reaches the span unreconstructed. The stdlib observation primitives expose where and how execution ran, `structlog` carries the structured record, and `opentelemetry-api` carries the span and cross-process context — one ambient `ContextVar` correlation feeds all three, never three independent correlation paths.

| [INDEX] | [CONCERN]              | [USE]                                                 | [REPLACE]                            |
| :-----: | :--------------------- | :---------------------------------------------------- | :----------------------------------- |
|  [01]   | execution monitoring   | `Probe`-keyed `set_local_events` on one `__code__`    | `settrace` global scrapers           |
|  [02]   | callback retire/re-arm | `sys.monitoring.DISABLE` + `restart_events`           | per-call global event toggles        |
|  [03]   | audit interception     | `sys.addaudithook`                                    | monkeypatch security probes          |
|  [04]   | sampling profiler      | `profiling.sampling.Collector`                        | handwritten timers or `profile`      |
|  [05]   | C-stack dump           | `faulthandler.dump_c_stack`                           | external native stack probes         |
|  [06]   | live debug attach      | `sys.remote_exec`                                     | debugger injection hooks             |
|  [07]   | fine-grained location  | `code.co_lines` / `code.co_positions`                 | `co_lnotab` decoding                 |
|  [08]   | structured record      | `structlog.get_logger` + processor chain              | `logging` calls, `str(exc)` messages |
|  [09]   | sub-threshold elision  | `make_filtering_bound_logger`                         | `filter_by_level` over a built event |
|  [10]   | ambient correlation    | `contextvars.merge_contextvars` + `bound_contextvars` | per-call `.bind()`, trace-id args    |
|  [11]   | span and status        | `Tracer.start_as_current_span` + `set_status`         | error facts in untyped span tags     |
|  [12]   | exception-to-span      | `Span.record_exception`                               | manual traceback string on a tag     |
|  [13]   | cross-process context  | `propagate.inject` / `propagate.extract`              | hand-rolled `traceparent` headers    |
|  [14]   | active exception       | `sys.exception()` -> `record_exception`               | `sys.exc_info()[1]`, traceback string |
|  [15]   | deprecation marker     | `@warnings.deprecated()`                              | docstring-only deprecation notices   |

[OBSERVATION_PRIMITIVES]:
- Use when: a runtime-owned surface explains execution behavior, performance, security observation, or failure location below the emission layer.
- Accept: `sys.monitoring` tool-id registration, `sys.addaudithook` for security events, `profiling.sampling` collectors, frame-pointer-preserving builds for `faulthandler.dump_c_stack`, `sys.remote_exec` as the safe debug-attach point, and `co_lines`/`co_positions` for fine-grained traceback locations.
- Law: the observation kind is a closed vocabulary, not a tool-id parameter — one `Probe` member keys the `SCOPE` `frozendict` to its `(tool_id, event_mask)` row, so a new probe (`PY_RETURN` returns, `LINE` stepping, `C_RAISE` native faults) is one row and the callback stays polymorphic over the event family through its trailing `*rest`; a per-event `tool=`/`events=` knob pair the body re-pairs is the rejected form.
- Law: `sys.monitoring` scopes events to the observed code object through `set_local_events(tool_id, code, events)`, never `set_events` global — a process-wide RAISE scraper fires on every internal `StopIteration` of the generator and context-manager protocol, so observation targets one `__code__` and a registered callback returns `sys.monitoring.DISABLE` to retire itself at a hot location while `restart_events` re-arms the disabled set; `settrace` is the rejected whole-program scraper.
- Law: observation is a root-scoped primitive, not a per-call decorator arm — installing a monitoring callback inside the invocation aspect rescrapes on every call, so the observation surface registers once at the observation root while the emission aspect owns span, record, and ambient correlation per call.
- Reject: frame-pointer-stripped native builds, legacy `profile`, debugger injection hooks, `settrace` event scrapers, `set_events` global registration for a scoped probe, monkeypatch security probes, `co_lnotab` decoding, and line-only diagnostics that discard column positions.

[EMISSION_SPINE]:
- Use when: a structured record, a trace span, or cross-process correlation leaves the process.
- Law: `structlog.configure` runs exactly once at the composition root with `merge_contextvars` first in the processor chain so ambient context is present for every downstream processor, and a `make_filtering_bound_logger` `wrapper_class` compiles sub-threshold level methods into no-ops — filtering at method resolution, never a `filter_by_level` processor that still builds the event dict.
- Law: correlation is one ambient cell, not three — `bound_contextvars(trace_id=, span_id=)` scopes the ids derived once from the active span's `get_span_context()` over the call block and auto-resets at exit, the same active span carries `set_status` and `record_exception`, and `merge_contextvars` first in the chain lifts those ids onto every log line; a hand-written trace-id parameter on a signature or a per-record `.bind()` is the rejected fan-out.
- Law: a span is `Tracer.start_as_current_span` (the context-manager form that activates and ends), `SpanKind` declares the boundary semantics, and `set_status(Status(StatusCode.ERROR))` is the typed verdict because error facts in untyped tags are invisible to backend error filters; a fact that must outlive an unsampled trace is a `structlog` record, since span events die with the sampling verdict.
- Law: `witnessed` is the emission content — span open, ambient-id derivation, record, status — and enters the aspect stack as one `*composed` entry of the `surfaces-and-dispatch.md` weave fold; the weave is `[**P, T, E]`-generic and preserves the operation's own fault vocabulary unchanged (`map_error` returns `cause` untouched, `str(cause)` feeds only the span description and the log line), so it composes onto a closed-fault-vocabulary operation as readily as a `str`-fault one and never collapses `E` to bare `str` at the seam. The rail contract is that fold's fixed innermost arm, so this weave threads `operation` directly with no second `beartype` guard and never folds a contract of its own. A bare `beartype(operation)` rebuilt here is the contract re-teach the weave fold already owns, and its raising form is the one that surface explicitly rejects.
- Law: cross-process context rides `propagate.inject`/`extract` over a carrier-typed `Setter`/`Getter` — direction is the presence of a parent `Context` (none extracts, given injects), never carrier truthiness, since an empty carrier is the legitimate extract-to-root case; `DefaultGetter`/`DefaultSetter` already cover every dict-like header carrier, so a custom `Getter` is authored only for a multi-dict carrier (`grpc.aio.Metadata`), the W3C `traceparent` encoding stays `opentelemetry-api`'s, and a hand-rolled header pair or a re-implemented default getter is the rejected form.
- Reject: `logging` calls in a `structlog`-configured codebase, `str(exc)` where `dict_tracebacks` carries the structured traceback, a renderer placed before a mutating processor, SDK provider construction outside the composition root, and per-request instrument or span-name drift.

[ACTIVE_EXCEPTION]:
- Use when: the in-flight exception feeds an observation surface, or a syntax form changes what the interpreter reports about it.
- Law: the emission weave reads the active exception through `sys.exception()` — the single-object form that replaces the `sys.exc_info()` triple — and hands it straight to `Span.record_exception`, so the exception-to-span seam never reconstructs a traceback string and never threads an `exc_info` tuple through the signature; `code.co_positions` supplies the column-precise location the span carries. Grouped-failure transport into the carrier — `except*` partitioning and `BaseException.add_note` cause preservation — is the boundary-conversion owner's law, composed when emission rides a multi-failure boundary, never re-spelled here.
- Reject: `sys.exc_info()[1]` where `sys.exception()` reads the active exception; a hand-built traceback string on a span tag where `record_exception` carries the structured frame; `return`/`break`/`continue` that exits a `finally` and discards the in-flight exception; a docstring-only deprecation where `@warnings.deprecated()` is the auditable marker.

```python conceptual
import sys
from collections.abc import Callable, MutableMapping
from enum import StrEnum
from functools import wraps
from types import CodeType

import structlog
from builtins import frozendict
from expression import Result
from opentelemetry import propagate, trace
from opentelemetry.context import Context
from opentelemetry.propagators.textmap import DefaultGetter, DefaultSetter
from opentelemetry.trace import SpanKind, Status, StatusCode
from structlog.contextvars import bound_contextvars, merge_contextvars
from structlog.processors import JSONRenderer, TimeStamper, add_log_level, dict_tracebacks
from structlog.typing import FilteringBoundLogger

_EVENT = sys.monitoring.events


class Probe(StrEnum):
    FAULT = "<probe-a>"
    RETURN = "<probe-b>"
    LINE = "<probe-c>"


SCOPE: frozendict[Probe, tuple[int, int]] = frozendict(
    {Probe.FAULT: (sys.monitoring.PROFILER_ID, _EVENT.RAISE),
     Probe.RETURN: (sys.monitoring.PROFILER_ID, _EVENT.PY_RETURN),
     Probe.LINE: (sys.monitoring.DEBUGGER_ID, _EVENT.LINE)}
)


def root(level: int, sink: FilteringBoundLogger, /) -> None:
    structlog.configure(
        processors=[merge_contextvars, add_log_level, TimeStamper(fmt="iso", utc=True), dict_tracebacks, JSONRenderer()],
        wrapper_class=structlog.make_filtering_bound_logger(level),
        cache_logger_on_first_use=True,
    )
    sys.addaudithook(lambda event, args: sink.info("<audit>", hook=event, arity=len(args)))


def witnessed[**P, T, E](name: str, /, *, kind: SpanKind = SpanKind.INTERNAL) -> Callable[[Callable[P, Result[T, E]]], Callable[P, Result[T, E]]]:
    log: FilteringBoundLogger = structlog.get_logger(name)
    tracer = trace.get_tracer(name)

    def weave(operation: Callable[P, Result[T, E]], /) -> Callable[P, Result[T, E]]:
        op = operation.__qualname__

        @wraps(operation)
        def call(*args: P.args, **kwargs: P.kwargs) -> Result[T, E]:
            with tracer.start_as_current_span(name, kind=kind) as span:
                marker = span.get_span_context()

                def faulted(cause: E, /) -> E:
                    described = str(cause)
                    log.error("<faulted>", op=op, cause=described)
                    span.set_status(Status(StatusCode.ERROR, described))
                    span.record_exception(sys.exception() or RuntimeError(described))
                    return cause

                with bound_contextvars(trace_id=trace.format_trace_id(marker.trace_id), span_id=trace.format_span_id(marker.span_id)):
                    return operation(*args, **kwargs).map(lambda value: log.info("<settled>", op=op) or value).map_error(faulted)

        return call

    return weave


def observe(target: Callable[..., object], sink: FilteringBoundLogger, /, *, probe: Probe = Probe.FAULT) -> None:
    tool, events = SCOPE[probe]

    def fired(code: CodeType, offset: int, /, *rest: object) -> object:
        sink.error("<observed>", at=code.co_qualname, offset=offset, lines=tuple(code.co_lines()), probe=probe)
        return sys.monitoring.DISABLE

    sys.monitoring.use_tool_id(tool, target.__qualname__)
    sys.monitoring.register_callback(tool, events, fired)
    sys.monitoring.set_local_events(tool, target.__code__, events)


def propagated(carrier: MutableMapping[str, str], parent: Context | None = None, /) -> Context:
    if parent is None:
        return propagate.extract(carrier, getter=DefaultGetter())
    propagate.inject(carrier, context=parent, setter=DefaultSetter())
    return parent
```
