# [PYTHON_CONCURRENCY]

Concurrent work runs inside one `anyio` failure boundary, never a loose task set: `create_task_group()` joins every child and cancels siblings on the first raise, so an orphaned task is structurally impossible. The deadline is a scope and never a `timeout` parameter; cancellation is the backend's own exception re-raised after cleanup, never a `Result.Error`; blocking work offloads through a `CapacityLimiter`-bounded thread, process, or subinterpreter arm; resource brackets dispose on every exit including cancellation under a shielded scope. The group's `BaseExceptionGroup` converts to the domain fault exactly once at the group edge, and the retry weave sees only a raised transient, never a railed `Error`. This rail runs the carriers the rail-algebra page owns and runs on the interpreter substrate the runtime page owns; it re-teaches neither, composing the fault vocabulary, the `threaded` reducer, and `Result`/`Option` as supporting material.

## [01]-[SCOPE_CHOOSER]

This table selects the concurrency primitive for an effect; when an effect matches several rows, the most specific wins, and the failure-boundary row is read before the offload rows.

| [INDEX] | [EFFECT_SIGNATURE]                  | [PRIMITIVE]                          | [REJECTED_FORM]                          |
| :-----: | :---------------------------------- | :----------------------------------- | :--------------------------------------- |
|  [01]   | concurrent children, joined fate    | `create_task_group()` + `start_soon` | `asyncio.gather` / bare `create_task`    |
|  [02]   | child result carried back           | `TaskHandle.return_value`            | shared mutable list, stream gather rig    |
|  [03]   | bound the whole effect, abort       | `fail_after(seconds)`                | a `timeout=` parameter threaded inward   |
|  [04]   | bound the whole effect, settle      | `move_on_after(seconds)`             | `asyncio.wait_for` wrapping the call     |
|  [05]   | scope tripped, not a fault          | `get_cancelled_exc_class()` re-raise | cancellation caught into `Result.Error`  |
|  [06]   | blocking I/O off the loop           | `to_thread.run_sync(limiter=)`       | `ThreadPoolExecutor` on the loop         |
|  [07]   | CPU-bound, picklable-free isolation | `to_interpreter.run_sync(limiter=)`  | `to_process` for a pickle-free callable  |
|  [08]   | GIL-hostile native call             | `to_process.run_sync(limiter=)`      | unbounded subprocess fan-out             |
|  [09]   | fault-ordered handle disposal       | `AsyncExitStack` + shielded teardown | bare `try`/`finally` awaiting unshielded |
|  [10]   | group failure into the vocabulary   | `except*` at the group edge          | `except Exception` over the whole block  |
|  [11]   | deterministic timing in a spec      | `trio.testing.MockClock(0)`          | wall-clock `sleep` in a test             |

## [02]-[TASK_GROUP]

The task group is the failure boundary: one `async with anyio.create_task_group()` whose `__aexit__` awaits every child and cancels siblings on the first raise. The child carrier rides the handle, not a side channel.

[CHILD_CARRIER]:
- Law: `group.start_soon(operation, *args)` takes the coroutine function and its arguments — never a pre-built awaitable — and returns a `TaskHandle[T]`; once the group exits cleanly, `handle.return_value` reads that child's carrier, so the gather is the typed property the handle already owns, never a shared mutable list mutated from inside the child and never a `create_memory_object_stream` rig re-implementing result transport. The stream pair stays the producer-consumer log the boundary owner holds; this rail reaches for it only when a child must publish a sequence of intermediates, not one terminal.
- Law: `handle.return_value` reads the child carrier on the clean path only — a cancelled child's raises `TaskCancelled` and a failed child's raises `TaskFailed` — so the gather yields one `Block[Result[T, E]]` of carriers and the rail page's fail-fast reducer folds it into `Result[Block[T], E]`; the gather never re-derives that reducer, and the deadline verdict and group fault that gate it are the `[03]` and `[05]` owners.
- Use: `group.start` over `start_soon` when the child must signal readiness — it blocks until the child calls `task_status.started(value)` and returns that value, the listener-bound / port-ready handshake; `start_soon` is fire-and-track with no handshake.
- Reject: `asyncio.gather`, `asyncio.create_task`, or a bare `create_task_group` without `async with`; a child appending to a closure-captured list; a `MemoryObjectReceiveStream` drained as the result carrier where `return_value` already carries it.

[OFFLOAD_LANE]:
- Law: blocking work crosses the loop on exactly one arm keyed by isolation need — `to_thread.run_sync` for blocking I/O sharing the address space, `to_interpreter.run_sync` for CPU-bound work with no pickle hop (each subinterpreter owns its GIL), `to_process.run_sync` for a GIL-hostile native call needing a real process — and every arm takes an explicit `CapacityLimiter` so subsystem concurrency is bounded at the boundary, not left to the per-loop default. The arm is a `frozendict` policy value keyed on the lane, so a new lane is one row and the call site never re-pairs a `lane: str` knob to a dispatcher the value already selects.
- Law: a worker death surfaces as a typed raise the boundary converts — `BrokenWorkerProcess` from `to_process`, `BrokenWorkerInterpreter` from `to_interpreter` — never a raw exception escaping the offload seam; the limiter the lane carries bounds the fan-out so a burst of children never exceeds the slot count the policy declares.
- Use: one `CapacityLimiter(slots)` constructed at the boundary and threaded into every arm of one subsystem; `to_interpreter` over `to_process` whenever the callable and its arguments need no pickle round trip, since the subinterpreter hop is the lighter isolate.
- Reject: a bare `ThreadPoolExecutor`/`ProcessPoolExecutor` on the loop; an unbounded offload trusting the default limiter; `to_process` for a callable `to_interpreter` already isolates without the pickle cost.

```python conceptual
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Awaitable, Callable
from enum import StrEnum
from typing import Literal

import anyio
from anyio import BrokenWorkerInterpreter, BrokenWorkerProcess, CapacityLimiter, TaskHandle
from builtins import frozendict
from expression import Error, Ok, Result
from expression.collections import Block

# --- [TYPES] ----------------------------------------------------------------------------
type RunFault = Literal["<worker>"]
type Offload[T] = Callable[[Callable[[], T], CapacityLimiter], Awaitable[T]]


class Lane(StrEnum):
    THREAD = "<lane-a>"
    INTERP = "<lane-b>"
    PROCESS = "<lane-c>"


# --- [OPERATIONS] -----------------------------------------------------------------------
async def _thread[T](work: Callable[[], T], limiter: CapacityLimiter, /) -> T:
    return await anyio.to_thread.run_sync(work, limiter=limiter)


async def _interp[T](work: Callable[[], T], limiter: CapacityLimiter, /) -> T:
    return await anyio.to_interpreter.run_sync(work, limiter=limiter)


async def _process[T](work: Callable[[], T], limiter: CapacityLimiter, /) -> T:
    return await anyio.to_process.run_sync(work, limiter=limiter)


ARM: frozendict[Lane, Offload[object]] = frozendict({Lane.THREAD: _thread, Lane.INTERP: _interp, Lane.PROCESS: _process})


async def gathered[T](work: Block[Callable[[], T]], /, *, lane: Lane, slots: int) -> Block[Result[T, RunFault]]:
    arm, limiter = ARM[lane], CapacityLimiter(slots)
    handles: list[TaskHandle[Result[T, RunFault]]] = []

    async def offloaded(job: Callable[[], T], /) -> Result[T, RunFault]:
        try:
            return Ok(await arm(job, limiter))
        except (BrokenWorkerProcess, BrokenWorkerInterpreter):
            return Error("<worker>")

    async with anyio.create_task_group() as group:
        handles = [group.start_soon(offloaded, job) for job in work]
    return Block.of_seq(handle.return_value for handle in handles)
```

## [03]-[DEADLINE_AND_CANCELLATION]

The bound on an effect is a scope, never a signature parameter: the deadline rides the enclosing `CancelScope`, so it composes through nested calls with no per-call token, and the value's removal would lose nothing the scope does not already carry. Cancellation is the backend's own exception, distinct from every domain fault.

[DEADLINE_SCOPE]:
- Law: a deadline is `anyio.fail_after(seconds)` when expiry is a fault — it raises `TimeoutError` the group edge maps into the vocabulary — or `anyio.move_on_after(seconds)` when expiry is a settled outcome the body reads through `scope.cancelled_caught`; the scope owns the bound, so a `timeout`/`deadline` entrypoint parameter is the knob the scope already encodes, deleted under the knob test.
- Law: `move_on_after` returns and `cancelled_caught` is the post-scope verdict read before any child handle is drained, so the deadline short-circuits the terminal — a timed-out gather returns `Error("<deadline>")` without touching a `return_value` that would raise; nested scopes compose by `current_effective_deadline`, the inner bound never outliving the outer.
- Reject: `asyncio.wait_for` wrapping the call; a `timeout: float` parameter threaded through the signature; a deadline read after the handles are gathered where `cancelled_caught` already settled the outcome.

[CANCELLATION_RAIL]:
- Law: cancellation is not failure — it is the `anyio.get_cancelled_exc_class()` exception, re-raised after cleanup runs, never swallowed into a `Result.Error`; a `finally` under `anyio.CancelScope(shield=True)` owns the cleanup and lets the cancelled exception propagate, because a value returned in place of the re-raise breaks the structured-cancellation contract the group depends on.
- Law: a cancelled scope never retries — the retry predicate refuses the cancellation class first, so the schedule's `on=` names the provider's transient exceptions and a backoff sleep, itself a checkpoint, never re-arms work an outer deadline cancelled; the `[05]-[GROUP_FAULT]` conversion reads the cancellation arm before the worker arm.
- Reject: catching `get_cancelled_exc_class()` and returning a value; a broad `except Exception` over a block whose cancellation must propagate; cleanup-hostile process termination that abandons the shielded teardown.

## [04]-[RESOURCE_BRACKET]

The owner that acquires a resource disposes it on every exit — success, domain fault, raised exception, and cancellation. Acquisition order is teardown order reversed, and a later acquisition failing releases every earlier handle.

[BRACKET_OWNER]:
- Law: a dynamic, fault-ordered set of handles rides one `contextlib.AsyncExitStack` whose `enter_async_context` registers each handle as it is acquired, so the stack unwinds in reverse on exit and an acquisition raise mid-build releases the earlier handles before it propagates; a statically scoped single lifetime is one `async with` per handle, the stack reserved for the dynamic set.
- Law: cleanup runs under `with anyio.CancelScope(shield=True):` because an outer deadline or sibling failure cancels the scope mid-teardown otherwise — an unshielded `finally` that awaits is aborted before the handle closes, leaking it; the shielded scope is the named platform-forced statement seam, and the address-bound `await _release(...)` inside it is that seam's only statement.
- Law: a release that itself raises folds into the in-flight failure set through `BaseException.add_note` or an `except*` group rather than masking the fault that triggered teardown, so a cleanup fault and the original fault both reach the conversion edge.
- Exemption: the `enter_async_context` acquisition call, the shielded `await _release(...)`, and the `yield handle` inside the async context manager are the named platform-forced statement seam.
- Reject: a bare `try`/`finally` whose `finally` awaits without a shield; a handle acquired before the stack that no exit path releases; cleanup that swallows the cancellation exception instead of re-raising after release.

```python conceptual
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import AsyncIterator, Callable
from contextlib import AsyncExitStack, asynccontextmanager
from typing import Literal

import anyio
from expression import Error, Ok, Result
from expression.collections import Block

# --- [TYPES] ----------------------------------------------------------------------------
type BracketFault = Literal["<acquire-deadline>", "<release>"]


# --- [OPERATIONS] -----------------------------------------------------------------------
@asynccontextmanager
async def leased(name: str, acquire: Callable[[str], "Lease"], release: Callable[["Lease"], None], /) -> AsyncIterator["Lease"]:
    handle = await anyio.to_thread.run_sync(acquire, name)
    try:
        yield handle
    finally:
        with anyio.CancelScope(shield=True):
            await anyio.to_thread.run_sync(release, handle)


async def bracketed[T, E](
    names: Block[str], body: Callable[[Block["Lease"]], Result[T, E]], acquire: Callable[[str], "Lease"], release: Callable[["Lease"], None], /, *, seconds: float
) -> Result[T, E | BracketFault]:
    async with AsyncExitStack() as stack:
        with anyio.move_on_after(seconds) as scope:
            held = Block.of_seq([await stack.enter_async_context(leased(name, acquire, release)) for name in names])
        if scope.cancelled_caught:
            return Error("<acquire-deadline>")
    return body(held)
```

## [05]-[GROUP_FAULT]

The group's `BaseExceptionGroup` is converted into the closed fault vocabulary exactly once at the group edge through `except*`. The conversion reads the cancellation arm first, partitions the remaining failures by type, and preserves cause with `add_note`; the retry weave wraps each attempt and sees only a raised transient.

[GROUP_CONVERSION]:
- Law: a task group aggregates child failures into a `BaseExceptionGroup` the group edge splits with `except*` — one `except*` arm per closed fault case binding the partitioned subgroup, because `return`/`break`/`continue` are illegal inside an `except*` block, so each arm assigns a carrier and the post-scope read folds the assigned fault and the deadline verdict into one terminal; an `except Exception` over the whole `async with` flattens the group and erases which children failed.
- Law: cause survives the conversion — `BaseException.add_note` annotates the in-flight exception before it is re-spelled into the vocabulary, and a release that raised during teardown joins the failure set rather than masking the trigger, so the aggregate fault's typed members stay structurally addressable instead of collapsing to one concatenated string.
- Law: the conversion reads dispositions in fixed order — cancellation first (re-raised, never railed), then the worker arm, then the residual child arm — so a deadline that cancelled the group never routes through the transient set, and the fault vocabulary's own combination law aggregates multi-child failures.
- Reject: `except Exception` over a task group; a stringified group message standing in for the typed cases; `return`/`break`/`continue` inside an `except*` block; the cancellation class folded into the retry-eligible set.

[RETRY_BOUNDARY]:
- Law: retry triggers only on a raised transient at the effect boundary and wraps each attempt independently — a domain fault already railed as `Result.Error` is not an exception and is never retried, so the boundary maps the retried call's terminal onto the carrier after the schedule returns, and re-raising an `Error` to force a retry is the rejected inversion. The `stamina.retry(on=...)` schedule is the policy value the surface page weaves at definition time; this rail composes that settled weave and owns the raised-versus-railed boundary that bounds what it may see.
- Law: the schedule is bounded — at least one of `attempts` or `timeout` is non-`None` — and its backoff sleep is an `anyio` checkpoint, so an enclosing `move_on_after` deadline preempts a retry storm and the `[03]` cancellation exclusion keeps a cancelled scope out of the transient set; a fixed-policy target reused across sites is one `AsyncRetryingCaller` built once with `.on(exc)` pre-binding the transient set.
- Reject: retrying a `Result.Error` by re-raising it; an unbounded retry trusted to stop itself; a second retry implementation beside the composed weave; the cancellation class in the schedule's `on=` set.

```python conceptual
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Awaitable, Callable
from typing import Literal

import anyio
import stamina
from anyio import BrokenWorkerProcess, TaskHandle
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block

# --- [ERRORS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class Fault:
    tag: Literal["deadline", "worker", "child", "aggregate"] = tag()
    deadline: float = case()
    worker: str = case()
    child: str = case()
    aggregate: tuple["Fault", ...] = case()

    @staticmethod
    def combined(left: "Fault", right: "Fault", /) -> "Fault":
        match left, right:
            case Fault(tag="aggregate"), Fault(tag="aggregate"):
                return Fault(aggregate=(*left.aggregate, *right.aggregate))
            case Fault(tag="aggregate"), _:
                return Fault(aggregate=(*left.aggregate, right))
            case _, _:
                return Fault(aggregate=(left, right))


# --- [OPERATIONS] -----------------------------------------------------------------------
@stamina.retry(on=BrokenWorkerProcess, attempts=5, timeout=30.0)
async def _attempt[T](operation: Callable[[], Awaitable[Result[T, Fault]]], /) -> Result[T, Fault]:
    return await operation()


def _partitioned(group: BaseExceptionGroup[Exception], kind: Callable[[str], Fault], note: str, /) -> Fault:
    for exc in group.exceptions:
        exc.add_note(note)
    return Block.of_seq(kind(type(exc).__name__) for exc in group.exceptions).reduce(Fault.combined)


async def converged[T](work: Block[Callable[[], Awaitable[Result[T, Fault]]]], /, *, seconds: float) -> Result[Block[Result[T, Fault]], Fault]:
    handles: list[TaskHandle[Result[T, Fault]]] = []
    fault: Option[Fault] = Nothing

    with anyio.move_on_after(seconds) as scope:
        try:
            async with anyio.create_task_group() as group:
                handles = [group.start_soon(_attempt, job) for job in work]
        except* BrokenWorkerProcess as broken:
            fault = Some(_partitioned(broken, lambda name: Fault(worker=name), "<at:worker>"))
        except* Exception as failed:
            fault = Some(_partitioned(failed, lambda name: Fault(child=name), "<at:converged>"))

    if scope.cancelled_caught:
        return Error(Fault(deadline=seconds))
    return fault.map(Error).default_with(lambda: Ok(Block.of_seq(handle.return_value for handle in handles)))
```

## [06]-[BACKEND_PROOF]

`trio` is the backend `anyio` runs on, selected by the `run(backend=...)` argument; the boundary tier targets the `anyio` surface, and the bare `trio` surface is reached only for what `anyio` does not expose — guest-mode hosting, a custom clock, instrument hooks, and the deterministic concurrency kit. A backend swap is one `run` argument, never a rewrite, because the vocabularies are parallel: `create_task_group`/`CancelScope`/`move_on_after` map one-to-one onto `open_nursery`/`CancelScope`/`move_on_after`.

[DETERMINISTIC_KIT]:
- Law: a concurrency spec gates timing on `trio.testing.MockClock(autojump_threshold=0)` injected at `trio.run(main, clock=clock)`, which collapses every idle wait to zero virtual time, so a deadline test runs instantly and deterministically; `trio.testing.wait_all_tasks_blocked()` settles the run to a fixed interleaving before an assertion, and `assert_checkpoints()` proves a block yields to the scheduler — wall-clock `sleep` and timing tolerance in a test are the rejected forms.
- Law: a `trio.abc.Instrument` attached through `lowlevel.add_instrument` observes `task_spawned`/`before_task_step`/`task_exited` across the whole run without touching task code, so cross-task tracing and metrics ride the instrument bus rather than a per-task wrapper; `lowlevel.start_guest_run` interleaves a trio run inside a foreign loop by handing trio a `run_sync_soon_threadsafe` callback, the one integration point when trio must coexist with a host loop it does not own.
- Reject: a wall-clock `sleep` or timing tolerance in a deterministic spec; instrumenting each task by hand where one `Instrument` covers the run; mixing `asyncio` primitives into a trio run; catching `trio.Cancelled` directly rather than letting the owning scope re-raise it.

```python conceptual
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Awaitable, Callable
from typing import override

import trio
from trio.abc import Instrument
from trio.lowlevel import Task

# --- [SERVICES] -------------------------------------------------------------------------
class Spans(Instrument):
    def __init__(self, sink: Callable[[str, str], None], /) -> None:
        self._sink = sink

    @override
    def task_spawned(self, task: Task, /) -> None:
        self._sink("<spawned>", task.name)

    @override
    def task_exited(self, task: Task, /) -> None:
        self._sink("<exited>", task.name)


# --- [OPERATIONS] -----------------------------------------------------------------------
async def _settled(body: Callable[[], Awaitable[None]], /) -> None:
    async with trio.open_nursery() as nursery:
        nursery.start_soon(body)
        await trio.testing.wait_all_tasks_blocked()


def proven(body: Callable[[], Awaitable[None]], sink: Callable[[str, str], None], /) -> None:
    clock = trio.testing.MockClock(autojump_threshold=0)
    trio.run(_settled, body, clock=clock, instruments=[Spans(sink)])
```
