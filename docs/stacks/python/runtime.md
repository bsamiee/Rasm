# [PYTHON_RUNTIME]

This page is the interpreter execution and isolation law: how a unit of work is placed onto an isolation arm — own-GIL subinterpreter, subinterpreter or process pool, or a free-threaded lock — and how the running interpreter is observed without a global scraper. Free-threaded execution (PEP 703) and subinterpreters (PEP 734) are the two execution substrates a bleeding-edge build admits, and the placement choice is one `frozendict` policy row, never a per-call knob. Structured concurrency — task groups, deadlines, cancellation, blocking-call offload — is owned by `concurrency.md`; this page owns the substrate that concurrency runs on and the introspection surfaces that read it. Every section names the form and the spelling it replaces, and the family card states the placement law.

## [01]-[EXECUTION_AND_ISOLATION]

Mutation ownership and isolation strategy are explicit before any work is placed. The isolation arm is selected by a `Lane` policy value keying a `frozendict` table to its execution function, so a new arm is one row and every caller is untouched; a `lane: str` knob the body re-pairs to a strategy is the rejected smuggle. A subinterpreter owns its own GIL, so own-GIL CPU separation is the lighter isolate below the process boundary; a free-threaded build no longer GIL-serializes shared mutation, so an `RLock` guards exactly the shared cell that loses serialization and `copy_context().run` carries the caller's `ContextVar` state across the guard.

| [INDEX] | [CONCERN]              | [USE]                                                    | [REPLACE]                            |
| :-----: | :--------------------- | :------------------------------------------------------- | :----------------------------------- |
|  [01]   | interpreter isolation  | `concurrent.interpreters.create`                         | process-only isolation wrappers      |
|  [02]   | own-GIL call dispatch  | `Interpreter.call` / `Interpreter.call_in_thread`        | pickled `exec`-string round trips    |
|  [03]   | interpreter main seed  | `Interpreter.prepare_main`                               | module-global seeding of the isolate |
|  [04]   | cross-interpreter pipe | `interpreters.create_queue` / `is_shareable`             | shared module-global handoff         |
|  [05]   | subinterpreter pool    | `concurrent.futures.InterpreterPoolExecutor`             | process-only CPU pools               |
|  [06]   | process-pool stop      | `ProcessPoolExecutor.terminate_workers` / `kill_workers` | private worker traversal             |
|  [07]   | process interrupt      | `multiprocessing.Process.interrupt`                      | cleanup-hostile `terminate`          |
|  [08]   | worker sizing          | `os.process_cpu_count`                                   | `os.cpu_count` worker counts         |
|  [09]   | GIL-state probe        | `sys._is_gil_enabled`                                    | platform-string free-threading guess |
|  [10]   | iterator sharing       | `threading.serialize_iterator`                           | generator lock wrappers              |
|  [11]   | cross-thread context   | `contextvars.copy_context().run`                         | thread-local async state carry       |

[FREE_THREADING]:
- Use when: shared mutation, context propagation, or a supported-target claim depends on free-threaded execution.
- Accept: free-threaded Python as a supported target, an `RLock` over the one shared-mutable cell the build no longer serializes, `ContextVar` for async or thread context, `copy_context().run` to carry that context across the guard, and `sys._is_gil_enabled()` as the runtime free-threading probe.
- Reject: experimental no-GIL caveats, implicit GIL serialization, thread-local async state, mutable ambient globals, a coarse lock over computation the cell does not share, and import-time singleton mutation as coordination.
- Law: a free-threaded build makes mutation ownership and context propagation explicit before relying on scheduling or cache behavior; `threading.serialize_iterator(it)` wraps a generator shared across threads so one consumer sees one element without an external lock, the per-iterator serialization the GIL once supplied for free.
- Boundary: the async-scheduling axis — task groups, deadlines, cancellation — is owned by `anyio` on `concurrency.md`, never by a free-threading surface here; the same-task scoped `ContextVar.set` token restore is `system-apis.md`'s, distinct from this page's cross-guard `copy_context().run` carry.

[INTERPRETER_ISOLATION]:
- Use when: interpreter isolation, independent execution, or own-GIL CPU separation owns the runtime boundary below the process boundary.
- Accept: `interpreters.create()` (each subinterpreter owns its GIL), `Interpreter.call(fn, *args, **kwargs)` to dispatch a real callable into the isolate, `Interpreter.call_in_thread` for the non-blocking form, `Interpreter.prepare_main(**ns)` guarded behind a non-empty shareable set to seed names before the call, and `create_queue()` with `is_shareable` for cross-interpreter object transport.
- Law: the isolation strategy is a `Lane` policy value keying the `frozendict` table to its execution arm — own-GIL subinterpreter, subinterpreter pool, process pool, or free-threaded lock — so a new isolation lane is one row, and a placement fault is a closed `Lapse` vocabulary whose tag is the lane that lost and whose payload names the failing type, so the cause stays structurally addressable by lane and never a bare `str` the body concatenates. The fault read is per-arm because the lanes raise different shapes: the subinterpreter arm catches `interpreters.ExecutionFailed` and reads `fault.excinfo.type.__name__` (the `excinfo` is a `SimpleNamespace` whose `type` carries `__name__`/`__qualname__`/`__module__`, not a live class because the original is unpicklable across the isolate), while the pool, process, and free-threaded arms catch a live `Exception` and read `type(fault).__name__` — every `Lane` member has its own `Lapse` case and every arm seals the rail, so the free-threaded arm captures the operation's own raise rather than letting it escape `Placed[T]`. A worker death never escapes the offload seam — `InterpreterPoolExecutor.result()` re-raises the child fault and a `BrokenInterpreterPool`/`BrokenProcessPool` marks the broken executor, both converted at the arm where the `with`-exit `shutdown` retires the interpreter pool and `ProcessPoolExecutor.kill_workers()` retires the broken process pool, never raised through the wrapped function. The table-dispatch and weave-fold mechanics are settled at their surface owner and composed here.
- Reject: a process-pool wrapper where interpreter isolation is the owner; a pickled `exec` string where `Interpreter.call` dispatches the callable directly; shared mutable module state read across an interpreter boundary; a `NotShareableError`-prone object pushed through `create_queue` without an `is_shareable` proof; a `Lane` smuggled in as a `lane: str` knob the body re-pairs to a strategy the value already selects.
- Boundary: process isolation survives only where the process boundary is the actual requirement — `ProcessPoolExecutor.terminate_workers()`/`kill_workers()` retire a stuck pool and `multiprocessing.Process.interrupt()` sends `SIGINT`, surfacing as `KeyboardInterrupt` in the child so its `finally` cleanup runs, where `terminate` sends an uncatchable `SIGTERM` that strands handles; the `create()` interpreter is the lighter isolate when shared address space is acceptable.

```python conceptual
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import os
import sys
from collections.abc import Callable
from concurrent import interpreters
from concurrent.futures import InterpreterPoolExecutor, ProcessPoolExecutor
from contextlib import AbstractContextManager, nullcontext
from contextvars import copy_context
from enum import StrEnum
from functools import wraps
from threading import RLock
from typing import Literal

from builtins import frozendict
from expression import Error, Ok, Result, case, tag, tagged_union


# --- [TYPES] ----------------------------------------------------------------------------
class Lane(StrEnum):
    ISOLATE = "<lane-a>"
    POOL = "<lane-b>"
    PROCESS = "<lane-c>"
    LOCKED = "<lane-d>"


@tagged_union(frozen=True)
class Lapse:
    tag: Literal["isolate", "pool", "process", "locked"] = tag()
    isolate: str = case()
    pool: str = case()
    process: str = case()
    locked: str = case()


type Placed[T] = Result[T, Lapse]


# --- [OPERATIONS] -----------------------------------------------------------------------
def _isolate[**P, T](operation: Callable[P, T], /, *args: P.args, **kwargs: P.kwargs) -> Placed[T]:
    runtime = interpreters.create()
    if shareable := {k: v for k, v in kwargs.items() if interpreters.is_shareable(v)}:
        runtime.prepare_main(**shareable)
    try:
        return Ok(runtime.call(operation, *args, **kwargs))
    except interpreters.ExecutionFailed as fault:
        return Error(Lapse(isolate=fault.excinfo.type.__name__))
    finally:
        runtime.close()


def _pooled[**P, T](operation: Callable[P, T], /, *args: P.args, **kwargs: P.kwargs) -> Placed[T]:
    with InterpreterPoolExecutor(max_workers=os.process_cpu_count()) as pool:
        try:
            return Ok(pool.submit(operation, *args, **kwargs).result())
        except Exception as fault:
            return Error(Lapse(pool=type(fault).__name__))


def _processed[**P, T](operation: Callable[P, T], /, *args: P.args, **kwargs: P.kwargs) -> Placed[T]:
    with ProcessPoolExecutor(max_workers=os.process_cpu_count()) as pool:
        try:
            return Ok(pool.submit(operation, *args, **kwargs).result())
        except Exception as fault:
            pool.kill_workers()
            return Error(Lapse(process=type(fault).__name__))


_GATE = RLock()


def _locked[**P, T](operation: Callable[P, T], /, *args: P.args, **kwargs: P.kwargs) -> Placed[T]:
    guard: AbstractContextManager[object] = nullcontext() if sys._is_gil_enabled() else _GATE
    try:
        with guard:
            return Ok(copy_context().run(operation, *args, **kwargs))
    except Exception as fault:
        return Error(Lapse(locked=type(fault).__name__))


# --- [TABLES] ---------------------------------------------------------------------------
STRATEGY: frozendict[Lane, Callable[..., Placed[object]]] = frozendict(
    {Lane.ISOLATE: _isolate, Lane.POOL: _pooled, Lane.PROCESS: _processed, Lane.LOCKED: _locked}
)


# --- [COMPOSITION] ----------------------------------------------------------------------
def placed[**P, T](lane: Lane = Lane.ISOLATE, /) -> Callable[[Callable[P, T]], Callable[P, Placed[T]]]:
    run = STRATEGY[lane]

    def weave(operation: Callable[P, T], /) -> Callable[P, Placed[T]]:
        @wraps(operation)
        def call(*args: P.args, **kwargs: P.kwargs) -> Placed[T]:
            return run(operation, *args, **kwargs)

        return call

    return weave
```

## [02]-[RUNTIME_INTROSPECTION]

Introspection reads where and how the interpreter ran — execution location, native and managed memory, the collector, and the active exception — through scoped, retiring probes, never a whole-program scraper. The observed kind is a closed `Probe` vocabulary keyed to its `(tool_id, event_mask)` row, and a registered callback returns `sys.monitoring.DISABLE` to retire itself at a hot location while `restart_events` re-arms the disabled set. Telemetry egress — structured records, spans, cross-process propagation — is not a runtime concern and rides the observability domain page; this page stops at the interpreter-execution evidence the egress later carries.

| [INDEX] | [CONCERN]              | [USE]                                         | [REPLACE]                              |
| :-----: | :--------------------- | :-------------------------------------------- | :------------------------------------- |
|  [01]   | scoped execution probe | `set_local_events(tool_id, code, events)`     | `settrace` / `set_events` scrapers     |
|  [02]   | callback retire/re-arm | `sys.monitoring.DISABLE` + `restart_events`   | per-call global event toggles          |
|  [03]   | audit interception     | `sys.addaudithook`                            | monkeypatch security probes            |
|  [04]   | C-stack dump           | `faulthandler.dump_c_stack`                   | external native stack probes           |
|  [05]   | managed-heap snapshot  | `tracemalloc.take_snapshot` + `reset_peak`    | manual allocation counters             |
|  [06]   | collector evidence     | `gc.freeze` / `gc.get_stats` / `gc.callbacks` | opaque collection guesses              |
|  [07]   | resource accounting    | `resource.getrusage` / `setrlimit`            | shell `time` / `ulimit` scraping       |
|  [08]   | remote script attach   | `sys.remote_exec(pid, path)`                  | debugger injection hooks               |
|  [09]   | fine-grained location  | `code.co_lines` / `code.co_positions`         | `co_lnotab` decoding                   |
|  [10]   | active exception       | `sys.exception()`                             | `sys.exc_info()[1]`, traceback strings |
|  [11]   | deprecation marker     | `@warnings.deprecated()`                      | docstring-only deprecation notices     |

[SCOPED_PROBE]:
- Use when: a runtime-owned surface explains execution behavior, performance, security observation, or failure location.
- Accept: `sys.monitoring` tool-id registration scoped to one `__code__`, `sys.addaudithook` for security events, frame-pointer-preserving builds for `faulthandler.dump_c_stack`, `sys.remote_exec(pid, path)` as the safe attach point (the target reads the named script file at its next checkpoint, so the file must outlive the read), and `co_lines`/`co_positions` for column-precise traceback locations.
- Law: the probe kind is a closed vocabulary, not a tool-id parameter — one `Probe` member keys the `SCOPE` `frozendict` to its `(tool_id, event_mask)` row, so a new probe (`RETURN` returns, `LINE` stepping, `FAULT` native raises) is one row and the callback stays polymorphic over the event family through its trailing `*rest`; a per-event `tool=`/`events=` knob pair the body re-pairs is the rejected form.
- Law: `sys.monitoring` scopes events to the observed code object through `set_local_events(tool_id, code, events)`, never `set_events` global — a process-wide `RAISE` scraper fires on every internal `StopIteration` of the generator and context-manager protocol, so observation targets one `__code__`, a registered callback returns `sys.monitoring.DISABLE` to retire itself at a hot location, and `restart_events()` re-arms the disabled set; `settrace` is the rejected whole-program scraper.
- Law: observation is a root-scoped primitive, not a per-call decorator arm — installing a monitoring callback inside an invocation aspect rescrapes on every call, so the probe registers once at the observation root over the target `__code__`.
- Reject: frame-pointer-stripped native builds, `settrace` event scrapers, `set_events` global registration for a scoped probe, monkeypatch security probes, `co_lnotab` decoding, and line-only diagnostics that discard column positions.

[MEMORY_AND_COLLECTOR]:
- Use when: managed-heap growth, native-allocation peaks, collector pressure, or a process resource ceiling is the evidence sought.
- Accept: `tracemalloc.take_snapshot()` for the per-frame allocation table, `Snapshot.compare_to(other, "lineno")` for the allocation delta, `tracemalloc.reset_peak()` between measured phases, `gc.get_stats()` for per-generation collection evidence, `gc.freeze()` to move startup-survivor objects out of every future scan, `gc.callbacks` for a phase observer, and `resource.getrusage(RUSAGE_SELF)` for `ru_maxrss`/`ru_utime` accounting.
- Law: a memory measurement is a snapshot pair, not a running counter — `take_snapshot` before and after the measured region and `compare_to` reports the per-line growth, while `reset_peak()` zeroes the high-water mark so the next region's peak is not the previous region's; the raw `get_traced_memory()` pair is the current/peak scalar, the snapshot the structured evidence.
- Law: `gc.freeze()` is the long-lived-process collection optimization — objects alive at the freeze are excluded from every later scan, so a post-warmup freeze keeps steady-state collection scanning only the working set; `gc.get_stats()` returns one `collections`/`collected`/`uncollectable`/`candidates`/`duration` row per generation, so collector pressure is the `duration` pause seconds per generation, not an object count alone — the evidence a bare `gc.collect()` return count discards.
- Reject: a hand-maintained allocation counter where a snapshot diff carries the per-frame table; a process-global `setrlimit` raise that outlives its scope; treating `gc.collect()` as a leak fix rather than the collector as evidence.

[ACTIVE_EXCEPTION]:
- Use when: the in-flight exception feeds an introspection surface, or a syntax form changes what the interpreter reports about it.
- Law: the active exception is read through `sys.exception()` — the single-object form that replaces the `sys.exc_info()` triple — and the raise-site `(lineno, end_lineno, col, end_col)` is `co_positions()` indexed at the deepest traceback frame: walk `__traceback__` to its last `tb_next`, then index `tuple(code.co_positions())[tb_lasti // 2]`, because `co_positions()` enumerates every instruction and only `tb_lasti` selects the raising one — a bare `next(co_positions())` reads the first instruction's location, not the fault's. The read never reconstructs a traceback string and never threads an `exc_info` tuple through a signature. Grouped-failure transport into the carrier — `except*` partitioning and `BaseException.add_note` cause preservation — is `concurrency.md`'s group-edge conversion and `rails-and-effects.md`'s boundary-capture law, composed when introspection rides a multi-failure boundary, never re-spelled here.
- Reject: `sys.exc_info()[1]` where `sys.exception()` reads the active exception; a hand-built traceback string where `co_positions` carries the structured location; `next(co_positions())` standing in for the raise-site position it does not name; the top `__traceback__` frame where the deepest `tb_next` frame holds the raise; `return`/`break`/`continue` that exits a `finally` and discards the in-flight exception; a docstring-only deprecation where `@warnings.deprecated()` is the auditable marker the type checker and `DeprecationWarning` filter both see.

```python conceptual
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import gc
import resource
import sys
import tracemalloc
from collections.abc import Callable
from dataclasses import dataclass
from enum import StrEnum
from functools import wraps
from types import CodeType

from builtins import frozendict

_EVENT = sys.monitoring.events


# --- [TYPES] ----------------------------------------------------------------------------
type Location = tuple[int | None, int | None, int | None, int | None]
type Sink = Callable[["Witness"], None]


class Probe(StrEnum):
    FAULT = "<probe-a>"
    RETURN = "<probe-b>"
    LINE = "<probe-c>"


# --- [MODELS] ---------------------------------------------------------------------------
@dataclass(frozen=True, slots=True, kw_only=True)
class Witness:
    qualname: str
    grew: int
    peak: int
    maxrss: int
    collected: tuple[int, ...]
    pressure: tuple[float, ...]
    raised: str | None
    at: Location | None


# --- [TABLES] ---------------------------------------------------------------------------
SCOPE: frozendict[Probe, tuple[int, int]] = frozendict(
    {Probe.FAULT: (sys.monitoring.PROFILER_ID, _EVENT.RAISE),
     Probe.RETURN: (sys.monitoring.PROFILER_ID, _EVENT.PY_RETURN),
     Probe.LINE: (sys.monitoring.DEBUGGER_ID, _EVENT.LINE)}
)


# --- [OPERATIONS] -----------------------------------------------------------------------
def _at(exc: BaseException, /) -> Location | None:
    frame = exc.__traceback__
    while frame and frame.tb_next:
        frame = frame.tb_next
    return tuple(frame.tb_frame.f_code.co_positions())[frame.tb_lasti // 2] if frame else None


def measured[**P, T](sink: Sink, /) -> Callable[[Callable[P, T]], Callable[P, T]]:
    def weave(operation: Callable[P, T], /) -> Callable[P, T]:
        @wraps(operation)
        def call(*args: P.args, **kwargs: P.kwargs) -> T:
            tracemalloc.start()
            tracemalloc.reset_peak()
            before = tracemalloc.take_snapshot()
            try:
                return operation(*args, **kwargs)
            finally:
                active = sys.exception()
                _, peak = tracemalloc.get_traced_memory()
                stats = gc.get_stats()
                sink(Witness(
                    qualname=operation.__qualname__,
                    grew=sum(stat.size_diff for stat in tracemalloc.take_snapshot().compare_to(before, "lineno")),
                    peak=peak,
                    maxrss=resource.getrusage(resource.RUSAGE_SELF).ru_maxrss,
                    collected=tuple(row["collected"] for row in stats),
                    pressure=tuple(row["duration"] for row in stats),
                    raised=type(active).__name__ if active else None,
                    at=_at(active) if active else None,
                ))
                tracemalloc.stop()

        return call

    return weave


def observed(target: CodeType, sink: Callable[[Location], None], /, *, probe: Probe = Probe.FAULT) -> None:
    tool, events = SCOPE[probe]

    def fired(code: CodeType, offset: int, /, *rest: object) -> object:
        sink(tuple(code.co_positions())[offset // 2])
        return sys.monitoring.DISABLE

    sys.monitoring.use_tool_id(tool, target.co_qualname)
    sys.monitoring.register_callback(tool, events, fired)
    sys.monitoring.set_local_events(tool, target, events)
```
