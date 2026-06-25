# [PYTHON_CONCURRENCY]

Concurrent work runs inside one `anyio` failure boundary, never a loose task set: `create_task_group()` joins every child and cancels siblings on the first raise, so an orphaned task is structurally impossible. The deadline is a scope and never a `timeout` parameter; cancellation is the backend's own exception re-raised after cleanup, never a `Result.Error`; blocking work offloads through a `CapacityLimiter`-bounded thread, process, or subinterpreter arm; intra-boundary coordination rides `Event`/`Lock`/`Semaphore`/`Condition` whose `statistics()` is the only contention probe; resource brackets dispose on every exit including cancellation under a shielded scope. The group's `BaseExceptionGroup` converts to the domain fault exactly once at the group edge, and the retry weave sees only a raised transient, never a railed `Error`. This rail runs the carriers the rail-algebra page owns and runs on the interpreter substrate the runtime page owns; it re-teaches neither, composing the fault vocabulary, the `threaded` reducer, and `Result`/`Option` as supporting material, and the inbound `BlockingPortal` crossing and the serialized state cell stay the boundary page's surfaces this rail only supplies the runtime for.

## [01]-[SCOPE_CHOOSER]

This table selects the concurrency primitive for an effect; when an effect matches several rows, the most specific wins, and the failure-boundary row is read before the offload rows.

| [INDEX] | [EFFECT_SIGNATURE]                  | [PRIMITIVE]                          | [REJECTED_FORM]                          |
| :-----: | :---------------------------------- | :----------------------------------- | :--------------------------------------- |
|  [01]   | concurrent children, joined fate    | `create_task_group()` + `start_soon` | `asyncio.gather` / bare `create_task`    |
|  [02]   | child result carried back           | `TaskHandle.return_value`            | shared mutable list, stream gather rig   |
|  [03]   | child must signal readiness         | `group.start` + `task_status`        | a polled `ready: bool` the caller reads  |
|  [04]   | bound the whole effect, abort       | `fail_after(seconds)`                | a `timeout=` parameter threaded inward   |
|  [05]   | bound the whole effect, settle      | `move_on_after(seconds)`             | `asyncio.wait_for` wrapping the call     |
|  [06]   | nearest live bound, no token        | `current_effective_deadline()`       | a deadline argument re-threaded per call |
|  [07]   | scope tripped, not a fault          | `get_cancelled_exc_class()` re-raise | cancellation caught into `Result.Error`  |
|  [08]   | bound in-flight slots               | `CapacityLimiter` / `Semaphore`      | a counter the body increments by hand    |
|  [09]   | blocking I/O off the loop           | `to_thread.run_sync(limiter=)`       | `ThreadPoolExecutor` on the loop         |
|  [10]   | CPU-bound, picklable-free isolation | `to_interpreter.run_sync(limiter=)`  | `to_process` for a pickle-free callable  |
|  [11]   | GIL-hostile native call             | `to_process.run_sync(limiter=)`      | unbounded subprocess fan-out             |
|  [12]   | fault-ordered handle disposal       | `AsyncExitStack` + shielded teardown | bare `try`/`finally` awaiting unshielded |
|  [13]   | group failure into the vocabulary   | `except*` at the group edge          | `except Exception` over the whole block  |
|  [14]   | retry a raised transient            | `stamina.retry_context` over the op  | retrying a railed `Result.Error`         |
|  [15]   | deterministic timing in a spec      | `MockClock(autojump_threshold=0)`    | wall-clock `sleep` in a test             |

## [02]-[TASK_GROUP]

The task group is the failure boundary: one `async with anyio.create_task_group()` whose `__aexit__` awaits every child and cancels siblings on the first raise. The child carrier rides the handle, not a side channel, and every primitive that bounds or coordinates the children lives inside the same `async with` so its lifetime is the boundary's lifetime.

[CHILD_CARRIER]:
- Law: `group.start_soon(operation, *args)` takes the coroutine function and its arguments — never a pre-built awaitable — and returns a `TaskHandle[T]` carrying the child's whole disposition through the closed `TaskHandle.Status` family (`PENDING`/`FINISHED`/`FAILED`/`CANCELLED`/`CANCELLING`), so the result transport is the property the handle already owns, never a shared mutable list mutated from inside the child and never a `create_memory_object_stream` rig re-implementing it; the stream pair stays the producer-consumer log the boundary page owns, reached only when a child must publish a sequence of intermediates rather than one terminal.
- Law: the child body rails its own offload faults so the group exits clean and every handle settles `FINISHED` — `handle.return_value` then reads the carrier with no re-raise, and the gather yields one `Block[Result[T, E]]` the rail page's fail-fast reducer threads into `Result[Block[T], E]`. A child that raises instead leaves its handle `FAILED`/`CANCELLED` and propagates a `BaseExceptionGroup` from the `async with`, so `handle.return_value` is read only on the all-clean path; the `status`/`exception` accessors — `status` total and never raising, `return_value`/`exception` raising on a non-matching state — are the post-`except*` survivor classification the `[05]` group edge owns, and the deadline verdict that gates the read is the `[03]` owner.
- Use: `group.start(operation, *args, return_handle=True)` over `start_soon` when the child must signal readiness — it blocks until the child calls `task_status.started(value)`, returns that value as the listener-bound or port-ready handshake, and still yields the tracking handle; `start_soon` is fire-and-track with no handshake and `TASK_STATUS_IGNORED` is the default `task_status` for a child that reports nothing.
- Reject: `asyncio.gather`, `asyncio.create_task`, or a bare `create_task_group` without `async with`; a child appending to a closure-captured list; a `MemoryObjectReceiveStream` drained as the result carrier where the handle already carries it; reading `handle.return_value` on a path the group could have raised through.

[OFFLOAD_LANE]:
- Law: blocking work crosses the loop on exactly one arm keyed by isolation need — `to_thread.run_sync` for blocking I/O sharing the address space, `to_interpreter.run_sync` for CPU-bound work with no pickle hop, `to_process.run_sync` for a GIL-hostile native call needing a real process — and every arm takes an explicit `CapacityLimiter` so subsystem concurrency is bounded at the boundary, not left to the per-loop `current_default_*_limiter()` defaults (40-token thread, CPU-count process). The arm is a `frozendict` row keying the `Lane` member straight to the unbound `run_sync` callable, so each `to_X` surface is the row's value with no forwarder wrapping it, a new lane is one entry, and the call site never re-pairs a `lane: str` knob to a dispatcher the value already selects; the subinterpreter isolation arm the offload targets is `runtime.md`'s.
- Law: a worker death surfaces as a typed raise the seam converts into a two-case closed family — `BrokenWorkerProcess` from `to_process`, `BrokenWorkerInterpreter` from `to_interpreter` — each kept structurally addressable rather than flattened to one undifferentiated string, so a downstream `match` routes a process death apart from an interpreter death; `abandon_on_cancel=True` on `to_thread.run_sync` lets a cancelled call leave a truly side-effect-free thread running rather than blocking the scope on a thread that ignores cancellation.
- Use: one `CapacityLimiter(slots)` constructed inside the boundary and threaded into every arm of one subsystem; `to_interpreter` over `to_process` whenever the callable and its arguments need no pickle round trip, since the subinterpreter hop is the lighter isolate.
- Reject: a per-lane forwarder function renaming `to_X.run_sync`; a bare `ThreadPoolExecutor`/`ProcessPoolExecutor` on the loop; an unbounded offload trusting the default limiter; `to_process` for a callable `to_interpreter` already isolates without the pickle cost; a single-case `Literal` collapsing the two worker deaths.

[COORDINATION_PRIMITIVE]:
- Law: coordination inside the boundary is an `anyio` primitive, never shared mutable state plus a poll — `Event` is the set-once readiness latch a child awaits before it proceeds, `Lock`/`Semaphore` bound a critical section or an in-flight slot count, `Condition` wakes a waiter on a predicate change, and a `CapacityLimiter` resizes its `total_tokens` at runtime where a fixed `Semaphore` cannot; each is constructed inside the `async with` so it dies with the group and a stale handle is structurally impossible. The single-consumer serialized-state cell and the high-frequency `send_nowait` callback drain are the boundary page's `MemoryObjectStream` surfaces, not this primitive set — this rail supplies the task group they run inside and stops there.
- Law: contention is read through the primitive's own `statistics()` snapshot — `CapacityLimiterStatistics.borrowed_tokens`/`tasks_waiting`, `SemaphoreStatistics.tasks_waiting`, `LockStatistics.owner`, `EventStatistics.tasks_waiting` — never a hand-counted waiter tally the body maintains beside the primitive, so back-pressure evidence is the snapshot the primitive already owns and the carrier reports it as data rather than a side effect.
- Use: `Event` for a one-shot readiness gate paired with `group.start`'s handshake when the child must publish a value, a bare `Event` when readiness carries nothing; `Semaphore(initial_value, max_value=)` when the bound is fixed, `CapacityLimiter(total_tokens)` when the bound is offload slots resized under load.
- Reject: a `threading.Lock`/`threading.Event` on the async loop; a `ready: bool` flag polled in a sleep loop where an `Event` wakes exactly once; a hand-maintained in-flight counter where a `Semaphore`/`CapacityLimiter` owns the slot count and its `statistics()` owns the evidence.

```python conceptual
from collections.abc import Awaitable, Callable
from enum import StrEnum
from typing import Literal

import anyio
import anyio.to_interpreter
import anyio.to_process
import anyio.to_thread
from anyio import BrokenWorkerInterpreter, BrokenWorkerProcess, CapacityLimiter, Event, Semaphore, TaskHandle
from builtins import frozendict
from expression import Error, Ok, Result
from expression.collections import Block

type RunFault = Literal["<broken-process>", "<broken-interpreter>"]
type Offload[T] = Callable[..., Awaitable[T]]


class Lane(StrEnum):
    THREAD = "<lane-a>"
    INTERP = "<lane-b>"
    PROCESS = "<lane-c>"


ARM: frozendict[Lane, Offload[object]] = frozendict(
    {Lane.THREAD: anyio.to_thread.run_sync, Lane.INTERP: anyio.to_interpreter.run_sync, Lane.PROCESS: anyio.to_process.run_sync}
)
BROKEN: frozendict[type[Exception], RunFault] = frozendict({BrokenWorkerProcess: "<broken-process>", BrokenWorkerInterpreter: "<broken-interpreter>"})


async def gathered[T](work: Block[Callable[[], T]], /, *, lane: Lane, slots: int, gate: int) -> Block[Result[T, RunFault]]:
    arm, limiter, admit, opened = ARM[lane], CapacityLimiter(slots), Semaphore(gate), Event()

    async def railed(job: Callable[[], T], /) -> Result[T, RunFault]:
        await opened.wait()
        async with admit:
            try:
                return Ok(await arm(job, limiter=limiter))
            except (BrokenWorkerProcess, BrokenWorkerInterpreter) as broken:
                return Error(BROKEN[type(broken)])

    async with anyio.create_task_group() as group:
        handles: Block[TaskHandle[Result[T, RunFault]]] = work.map(lambda job: group.start_soon(railed, job))
        opened.set()
    return handles.map(lambda handle: handle.return_value)
```

## [03]-[DEADLINE_AND_CANCELLATION]

The bound on an effect is a scope, never a signature parameter: the deadline rides the enclosing `CancelScope`, so it composes through nested calls with no per-call token, and the value's removal would lose nothing the scope does not already carry. Cancellation is the backend's own exception, distinct from every domain fault.

[DEADLINE_SCOPE]:
- Law: a deadline is `anyio.fail_after(seconds)` when expiry is a fault — it raises `TimeoutError` the group edge maps into the vocabulary — or `anyio.move_on_after(seconds)` when expiry is a settled outcome the body reads through `scope.cancelled_caught`; the scope owns the bound, so a `timeout`/`deadline` entrypoint parameter is the knob the scope already encodes, deleted under the knob test.
- Law: nested scopes compose by deadline propagation — `current_effective_deadline()` reads the nearest active bound as monotonic time so an inner operation honours whichever of the outer abort-deadline or inner settle-deadline is sooner, and the inner scope never outlives the outer because the outer cancellation reaches every checkpoint inside it; an inner `fail_after` nested under an outer `move_on_after` lets a per-step abort live inside a whole-effect settle, the two verdicts read independently after their respective scopes.
- Law: `move_on_after` returns and `cancelled_caught` is the post-scope verdict read before any child handle is drained, so the deadline short-circuits the terminal — a timed-out gather returns `Error("<deadline>")` without touching a `return_value` that would raise.
- Reject: `asyncio.wait_for` wrapping the call; a `timeout: float` parameter threaded through the signature; a deadline read after the handles are gathered where `cancelled_caught` already settled the outcome; `time.monotonic()` arithmetic where `current_effective_deadline()` reads the live bound.

[CANCELLATION_RAIL]:
- Law: cancellation is not failure — it is the `anyio.get_cancelled_exc_class()` exception, re-raised after cleanup runs, never swallowed into a `Result.Error`; a `finally` under `anyio.CancelScope(shield=True)` owns the cleanup and lets the cancelled exception propagate, because a value returned in place of the re-raise breaks the structured-cancellation contract the group depends on. Cancellation is level-triggered: a swallowed cancellation re-raises at the next checkpoint unless the scope is shielded, so the shield is the only thing that lets an awaiting teardown complete.
- Law: a cancelled scope never retries — the retry predicate refuses the cancellation class first, so the schedule's `on=` names the provider's transient exceptions and a backoff sleep, itself a checkpoint, never re-arms work an outer deadline cancelled; the `[05]-[GROUP_FAULT]` conversion reads the cancellation arm before the worker arm.
- Reject: catching `get_cancelled_exc_class()` and returning a value; a broad `except Exception` over a block whose cancellation must propagate; cleanup-hostile process termination that abandons the shielded teardown; re-raising a railed `Error` to force a retry an outer scope already cancelled.

```python conceptual
from collections.abc import Awaitable, Callable
from typing import Literal

import anyio
from expression import Error, Ok, Result
from expression.collections import Block

type DeadlineFault = Literal["<whole-deadline>", "<step-deadline>"]


async def bounded_step[T](
    step: Callable[[], Awaitable[T]], release: Callable[[], Awaitable[None]], /, *, step_seconds: float
) -> Result[T, DeadlineFault]:
    with anyio.move_on_after(step_seconds, shield=False) as scope:
        try:
            held = await step()
        except anyio.get_cancelled_exc_class():
            with anyio.CancelScope(shield=True):
                await release()
            raise
    return Error("<step-deadline>") if scope.cancelled_caught else Ok(held)


async def staged[T](
    steps: Block[Callable[[], Awaitable[T]]],
    release: Callable[[], Awaitable[None]],
    /,
    *,
    whole_seconds: float,
    step_seconds: float,
) -> Result[Block[T], DeadlineFault]:
    async def threaded(acc: Result[Block[T], DeadlineFault], step: Callable[[], Awaitable[T]], /) -> Result[Block[T], DeadlineFault]:
        if acc.is_error():
            return acc
        tighter = min(step_seconds, anyio.current_effective_deadline() - anyio.current_time())
        outcome = await bounded_step(step, release, step_seconds=tighter)
        return acc.bind(lambda kept: outcome.map(lambda value: kept.append(Block.singleton(value))))

    with anyio.move_on_after(whole_seconds) as whole:
        gathered: Result[Block[T], DeadlineFault] = Ok(Block.empty())
        for step in steps:
            gathered = await threaded(gathered, step)
    return Error("<whole-deadline>") if whole.cancelled_caught else gathered
```

## [04]-[RESOURCE_BRACKET]

The owner that acquires a resource disposes it on every exit — success, domain fault, raised exception, and cancellation. Acquisition order is teardown order reversed, and a later acquisition failing releases every earlier handle.

[BRACKET_OWNER]:
- Law: a dynamic, fault-ordered set of handles rides one `contextlib.AsyncExitStack` whose `enter_async_context` registers each handle as it is acquired, so the stack unwinds in reverse on exit and an acquisition raise mid-build releases the earlier handles before it propagates; a statically scoped single lifetime is one `async with` per handle, the stack reserved for the dynamic set.
- Law: cleanup runs under `with anyio.CancelScope(shield=True):` because an outer deadline or sibling failure cancels the scope mid-teardown otherwise — an unshielded `finally` that awaits is aborted before the handle closes, leaking it; the shielded scope is the named platform-forced statement seam, and the address-bound `await _release(...)` inside it is that seam's only statement.
- Law: a release that itself raises folds into the in-flight failure set through `BaseException.add_note` or an `except*` group rather than masking the fault that triggered teardown, so a cleanup fault and the original fault both reach the conversion edge.
- Exemption: the `enter_async_context` acquisition call, the shielded `await _release(...)`, and the `yield handle` inside the async context manager are the named platform-forced statement seam.
- Reject: a bare `try`/`finally` whose `finally` awaits without a shield; a handle acquired before the stack that no exit path releases; cleanup that swallows the cancellation exception instead of re-raising after release.

```python conceptual
from collections.abc import AsyncIterator, Callable
from contextlib import AsyncExitStack, asynccontextmanager
from typing import Literal

import anyio
from expression import Error, Result
from expression.collections import Block

type BracketFault = Literal["<acquire-deadline>"]


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
        return Error("<acquire-deadline>") if scope.cancelled_caught else body(held)
```

## [05]-[GROUP_FAULT]

The group's `BaseExceptionGroup` is the one raise this rail converts into the settled fault vocabulary, exactly once at the group edge through `except*`. The conversion reads the cancellation arm first, partitions the remaining failures into the vocabulary's cases by type, and preserves cause with `add_note`; the fault family, its two-tier constructor, and its associative `combined` monoid arrive finalized from `rails-and-effects.md` — this card weaves them, never re-spelling the combination law. The retry weave wraps each attempt and sees only a raised transient, and a session-bearing consumer (an SSH session, a forwarded stream) opens inside the group under the deadline so one transport fault cancels the siblings and surfaces through the same edge.

[GROUP_CONVERSION]:
- Law: a task group aggregates child failures into a `BaseExceptionGroup` the group edge splits with `except*` — one arm per vocabulary case binding the partitioned subgroup, because `return`/`break`/`continue` are illegal inside an `except*` block, so each arm folds its subgroup into a `Fault` and assigns the running carrier, and the post-scope read folds that carrier and the deadline verdict into one terminal; an `except Exception` over the whole `async with` flattens the group and erases which children failed.
- Law: each `except*` arm reduces its subgroup through the settled `Fault.combined` monoid the rail page owns — every child re-spelled by the vocabulary's two-tier `Fault.of_detail` constructor, the railed results threaded by `Block.choose(to_option)`, and `reduce(Fault.combined)` closing the arm — and accrues onto the prior arm through that same law, so the multi-arm, multi-child accumulation is one associative reduction and no second combination algebra is authored here; the seam composes the monoid, it does not redefine it.
- Law: cause survives the conversion — `BaseException.add_note` annotates each in-flight exception before it is re-spelled into a case, and a release that raised during teardown joins the failure set rather than masking the trigger, so the aggregate fault's typed members stay structurally addressable instead of collapsing to one concatenated string.
- Law: the conversion reads dispositions in fixed order — cancellation first (re-raised, never railed), then the worker arm, then the residual child arm — so a deadline that cancelled the group never routes through the transient set, and the cancellation class is excluded from every `except*` arm because it is the backend signal, not a vocabulary case.
- Reject: a parallel group-only fault family or a re-authored `combined` beside the settled `Fault` monoid; `except Exception` over a task group; a stringified group message standing in for the typed cases; `return`/`break`/`continue` inside an `except*` block; the cancellation class folded into a fault arm.

[RETRY_BOUNDARY]:
- Law: retry triggers only on a raised transient at the effect boundary and wraps each attempt independently — a domain fault already railed as `Result.Error` is not an exception and is never retried, so the boundary maps the retried call's terminal onto the carrier after the schedule returns, and re-raising an `Error` to force a retry is the rejected inversion. `stamina.retry_context(on=...)` yields one `Attempt` per iteration whose body is the single transient-raising op, the inline form when only part of a task body retries; `@stamina.retry(on=...)` is the whole-callable form, and a fixed-policy target reused across sites is one `AsyncRetryingCaller` built once whose `.on(exc)` pre-binds the transient set so the call site passes only the callable and arguments.
- Law: the schedule is bounded — at least one of `attempts` or `timeout` is non-`None` — and its backoff sleep is an `anyio` checkpoint sniffio-dispatched onto the live backend, so an enclosing `move_on_after` deadline preempts a retry storm and the `[03]` cancellation exclusion keeps a cancelled scope out of the transient set; a `RetryHook` returning a context manager wraps each scheduled wait, so one span opened per attempt rides the retry seam the observability page's emission owner already legislates, composed here, never re-built.
- Law: a session-bearing consumer is the rail at transport scale — `asyncssh.connect(...)` is an `async with` session opened inside the task group, `conn.run(cmd, check=True)` raises `ProcessError`/`ConnectionLost` the retry seam catches as transient and the group edge converts, and the session's `aclose` is the shielded teardown the bracket owns, so a transport death cancels the siblings and surfaces through the one edge exactly as a worker death does; the SSH surface, its verification, and its fault taxonomy are the transport owner's, this rail only drives it under the deadline and cancellation it owns.
- Reject: retrying a `Result.Error` by re-raising it; an unbounded retry trusted to stop itself; a second retry implementation beside the composed weave; the cancellation class in the schedule's `on=` set; an SSH session opened outside the group where a transport fault cannot cancel the siblings.

```python conceptual
import anyio
import stamina
from anyio import BrokenWorkerProcess, TaskHandle
from asyncssh import ConnectionLost, ProcessError, SSHClientConnection
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.collections import Block

_TRANSIENT = stamina.AsyncRetryingCaller(attempts=5, timeout=30.0).on((ConnectionLost, ProcessError, BrokenWorkerProcess))


async def _driven(conn: SSHClientConnection, command: str, /) -> str:
    completed = await _TRANSIENT(conn.run, command, check=True)
    return str(completed.stdout)


def _accrued(prior: Option["Fault"], group: BaseExceptionGroup[Exception], note: str, /) -> Option["Fault"]:
    for exc in group.exceptions:
        exc.add_note(note)
    cased = Block.of_seq(Fault.of_detail(note, type(exc).__name__) for exc in group.exceptions).choose(lambda r: r.to_option())
    if cased.is_empty():
        return prior
    arm = cased.reduce(Fault.combined)
    return Some(prior.map(lambda held: Fault.combined(held, arm)).default_value(arm))


async def converged(conn: SSHClientConnection, commands: Block[str], /, *, seconds: float) -> Result[Block[str], "Fault"]:
    handles: list[TaskHandle[str]] = []
    fault: Option["Fault"] = Nothing

    with anyio.move_on_after(seconds) as scope:
        try:
            async with anyio.create_task_group() as group:
                handles = [group.start_soon(_driven, conn, command) for command in commands]
        except* (ConnectionLost, ProcessError) as transport:
            fault = _accrued(fault, transport, "<at:transport>")
        except* Exception as failed:
            fault = _accrued(fault, failed, "<at:converged>")

    if scope.cancelled_caught:
        return Error(Fault(deadline=seconds))
    return fault.map(Error).default_with(lambda: Ok(Block.of_seq(handle.return_value for handle in handles)))
```

## [06]-[BACKEND_PROOF]

`trio` is the backend `anyio` runs on, selected by the `run(backend=...)` argument; the boundary tier targets the `anyio` surface, and the bare `trio` surface is reached only for what `anyio` does not expose — guest-mode hosting, a custom clock, instrument hooks, and the deterministic concurrency kit. A backend swap is one `run` argument, never a rewrite, because the vocabularies are parallel: `create_task_group`/`CancelScope`/`move_on_after`/`create_memory_object_stream` map one-to-one onto `open_nursery`/`CancelScope`/`move_on_after`/`open_memory_channel`, and the retry sleep is a checkpoint on whichever backend `sniffio` detects.

[DETERMINISTIC_KIT]:
- Law: a concurrency spec gates timing on `trio.testing.MockClock(autojump_threshold=0)` injected at `trio.run(main, clock=clock)`, which collapses every idle wait to zero virtual time, so a deadline test runs instantly and deterministically; `trio.testing.wait_all_tasks_blocked()` settles the run to a fixed interleaving before an assertion, `trio.testing.Sequencer()` forces a deterministic cross-task interleaving where ordering is the property under test, and `assert_checkpoints()` proves a block yields to the scheduler — wall-clock `sleep` and timing tolerance in a test are the rejected forms.
- Law: a `trio.abc.Instrument` attached through `lowlevel.add_instrument` observes `task_spawned`/`before_task_step`/`task_exited` across the whole run without touching task code, so cross-task tracing and metrics ride the instrument bus rather than a per-task wrapper; `lowlevel.start_guest_run` interleaves a trio run inside a foreign loop by handing trio a `run_sync_soon_threadsafe` callback, the one integration point when trio must coexist with a host loop it does not own.
- Reject: a wall-clock `sleep` or timing tolerance in a deterministic spec; instrumenting each task by hand where one `Instrument` covers the run; mixing `asyncio` primitives into a trio run; catching `trio.Cancelled` directly rather than letting the owning scope re-raise it.

```python conceptual
from collections.abc import Callable
from typing import override

import trio
import trio.testing
from trio.abc import Instrument
from trio.lowlevel import Task


class Spans(Instrument):
    def __init__(self, sink: Callable[[str, str], None], /) -> None:
        self._sink = sink

    @override
    def task_spawned(self, task: Task, /) -> None:
        self._sink("<spawned>", task.name)

    @override
    def task_exited(self, task: Task, /) -> None:
        self._sink("<exited>", task.name)


async def _expired(seconds: float, /) -> bool:
    with trio.testing.assert_checkpoints():
        await trio.sleep(0)
    with trio.move_on_after(seconds) as scope:
        async with trio.open_nursery() as nursery:
            nursery.start_soon(trio.sleep_forever)
            await trio.testing.wait_all_tasks_blocked()
            await trio.sleep(seconds * 2)
    return scope.cancelled_caught


def proven(sink: Callable[[str, str], None], /, *, seconds: float) -> bool:
    clock = trio.testing.MockClock(autojump_threshold=0)
    return trio.run(_expired, seconds, clock=clock, instruments=[Spans(sink)])
```
