"""Automation engine: runs Watch, Schedule, and Manual triggers through Assay rails and direct programs.

Each trigger drives one action (Rail, Program, Sequence, or Debounce) under a single-flight
CapacityLimiter.  Envelopes are written to stdout as newline-delimited JSON; structlog telemetry
goes to stderr.  The public surface is `drive` plus `is_governed` and the type aliases exported
via `__all__`.
"""

from collections.abc import Callable, Coroutine
from contextvars import ContextVar
import hashlib
from itertools import count
from operator import itemgetter
import re
import sys
from typing import assert_never, TYPE_CHECKING
from zoneinfo import ZoneInfo, ZoneInfoNotFoundError

import aiocron  # type: ignore[import-untyped]  # aiocron ships no py.typed marker
import anyio
from anyio import to_thread
from expression import Error, Ok, Result
import msgspec
import psutil  # typed via the types-psutil stub (psutil ships no py.typed marker)
import structlog
from watchfiles import awatch, DefaultFilter, PythonFilter

from tools.assay.automation.model import Debounce, Manual, Program, Rail, Schedule, Sequence, Watch, WatchFilter
from tools.assay.composition.registry import rail, REGISTRY
from tools.assay.composition.settings import ArtifactScope
from tools.assay.core.engine import run_check_async
from tools.assay.core.model import Check, Claim, Counts, Envelope, envelope, Fault, fold, Input, Language, Mode, Report, Runner, Tool
from tools.assay.core.routing import Routed, Scope
from tools.assay.core.status import join, RailStatus


if TYPE_CHECKING:
    from collections.abc import AsyncIterator

    from anyio.abc import TaskGroup  # annotation-only; anyio.abc is not exposed from the root anyio import
    from anyio.streams.memory import MemoryObjectReceiveStream
    from watchfiles import BaseFilter

    from tools.assay.automation.model import Action, Trigger
    from tools.assay.composition.settings import AssaySettings
    from tools.assay.core.model import Bind


# --- [TYPES] ----------------------------------------------------------------------------

type ChangeBatch = tuple[tuple[str, str], ...]
type Fire = Callable[[ChangeBatch], Coroutine[None, None, None]]
type Worker = Callable[[], Coroutine[None, None, None]]
type RailOutcome = tuple[Envelope, bool]

# --- [CONSTANTS] ------------------------------------------------------------------------

# ContextVar isolates the priming latch per async context so tests don't bleed state.
_CPU_PRIMED: ContextVar[bool] = ContextVar("assay_cpu_primed", default=False)
_ENCODER = msgspec.json.Encoder(order="deterministic")
_JITTER_MS: int = 100
_NO_CHANGES: ChangeBatch = ()
_PROGRAM_ROUTED: Routed = Routed(language=Language.PYTHON, scope=Scope.FULL)

# --- [SERVICES] -------------------------------------------------------------------------

_LOG: structlog.stdlib.BoundLogger = structlog.get_logger("assay.automation")

# --- [OPERATIONS] -----------------------------------------------------------------------


def _emit(line: Envelope) -> None:
    # Per-line flush preserves Envelope framing across concurrent fires.
    sys.stdout.buffer.write(_ENCODER.encode(line) + b"\n")
    sys.stdout.buffer.flush()


def is_governed(threshold: float | None) -> bool:
    """Decide whether the CPU governor should suppress a fire.

    The first call that actually samples the CPU blocks for 0.1 s to prime the
    psutil kernel counter; all subsequent calls within the same async context use
    a non-blocking interval=None sample via `_CPU_PRIMED`.

    Args:
        threshold: Fractional CPU ceiling in [0, 1]; None disables the governor.

    Returns:
        True when the current CPU sample meets or exceeds `threshold * 100`.
    """
    match threshold:
        case None:
            return False
        case _:
            match _CPU_PRIMED.get():
                case False:
                    psutil.cpu_percent(0.1)
                    _CPU_PRIMED.set(True)
                case True:
                    pass
            return psutil.cpu_percent(interval=None) >= threshold * 100.0


def _resolve(action: Rail) -> Bind | None:
    return next((b for b in REGISTRY if b.claim is action.claim and b.verb == action.verb), None)


def _label(action: Action) -> tuple[Claim, str]:
    match action:
        case Rail(claim=c, verb=v):
            return c, v
        case Program() | Sequence():
            return Claim.STATIC, "program"
        case Debounce(action=inner):
            return _label(inner)


def _program_check(argv: tuple[str, ...]) -> Check:
    # Runner.DIRECT + Input.NONE bypasses registry lookup and passes argv verbatim through the Check shape.
    tool = Tool(
        name=argv[0] if argv else "program",
        runner=Runner.DIRECT,
        command=argv,
        input=Input.NONE,
        language=Language.PYTHON,
        claim=Claim.STATIC,
        mode=Mode.RUN,
    )
    return Check(tool=tool, paths=argv)


async def _program_outcome(action: Program, settings: AssaySettings) -> Result[Report, Fault]:
    # No deadline: catalog tools run unbounded per fire; only spawn and process failures become Faults.
    if not action.argv:
        return Error(Fault(("program",), RailStatus.FAULTED, "program argv must be non-empty"))
    check = _program_check(action.argv)
    scope = ArtifactScope.open(settings, Claim.STATIC)
    outcome = await run_check_async(check, settings=settings, scope=scope, routed=_PROGRAM_ROUTED)
    match outcome:
        case Result(tag="ok", ok=done):
            return Ok(fold(Claim.STATIC, "program", (done,)))
        case Result(error=fault):
            return Error(fault)


def _emitted(line: Envelope) -> Envelope:
    _emit(line)
    return line


def _rail_outcome(action: Rail, settings: AssaySettings) -> RailOutcome:
    # Decode failures fold to a FAULTED Envelope at this boundary rather than escaping as exceptions.
    bind = _resolve(action)
    match bind:
        case None:
            fault = Fault((), RailStatus.FAULTED, f"unbound rail: {action.claim.value}:{action.verb}")
            return envelope(fault, claim=action.claim, verb=action.verb), False
        case _:
            try:
                params = msgspec.json.decode(bytes(action.params), type=bind.params) if action.params else bind.params()
            except msgspec.DecodeError as exc:
                return envelope(Fault((), RailStatus.FAULTED, str(exc)[:1024]), claim=action.claim, verb=action.verb), False
            return rail(bind, settings)(params), True


async def _program_envelope(action: Program, settings: AssaySettings) -> Envelope:
    match await _program_outcome(action, settings):
        case Result(tag="ok", ok=report):
            payload: Report | Fault = report
        case Result(error=fault):
            payload = fault
    return _emitted(envelope(payload, claim=Claim.STATIC, verb="program"))


async def _emit_leaf(leaf: Action, settings: AssaySettings, limiter: anyio.CapacityLimiter, cpu_threshold: float | None) -> RailStatus:
    match is_governed(cpu_threshold):
        case True:
            claim, verb = _label(leaf)
            # SKIP bypasses the fold rail; Counts(1, 0, 1) still marks one governed leaf.
            skip = Report(claim, verb, RailStatus.SKIP, Counts(1, 0, 1), notes=(f"governed: cpu>={cpu_threshold or 0.0:.0%}",))
            return _emitted(envelope(skip, claim=claim, verb=verb)).status
        # Sequence/Debounce recurse without the limiter; re-acquiring the same token raises anyio `already holding a token`.
        case False:
            match leaf:
                case Rail() as r:
                    async with limiter:
                        env, emitted = await to_thread.run_sync(_rail_outcome, r, settings)
                        return (env if emitted else _emitted(env)).status
                case Program() as p:
                    async with limiter:
                        return (await _program_envelope(p, settings)).status
                case Sequence() as s:
                    return await _sequence(s.actions, settings, limiter, cpu_threshold)
                case Debounce(action=inner):
                    return await _emit_leaf(inner, settings, limiter, cpu_threshold)


# async required: callers need an async iterable for cooperative scheduling, not a sync generator
async def _forever() -> AsyncIterator[int]:  # noqa: RUF029
    for i in count():
        yield i


async def _sequence(leaves: tuple[Action, ...], settings: AssaySettings, limiter: anyio.CapacityLimiter, cpu_threshold: float | None) -> RailStatus:
    folded = RailStatus.EMPTY
    async for i in _forever():
        match i < len(leaves):
            case False:
                return folded
            case True:
                folded = join(folded, await _emit_leaf(leaves[i], settings, limiter, cpu_threshold))
                match folded:
                    case RailStatus.FAILED | RailStatus.BUSY | RailStatus.TIMEOUT | RailStatus.FAULTED:
                        return folded
                    case _:
                        pass
    return folded  # pragma: no cover  # _forever never terminates; the in-loop returns are the only exits


def _fire(action: Action, settings: AssaySettings, *, limiter: anyio.CapacityLimiter, cpu_threshold: float | None) -> Fire:
    async def fire(_changes: ChangeBatch) -> None:
        match action:
            case Sequence(actions=acts):
                _ = await _sequence(acts, settings, limiter, cpu_threshold)
            case Rail() | Program():
                _ = await _emit_leaf(action, settings, limiter, cpu_threshold)
            case Debounce(action=inner):
                _ = await _emit_leaf(inner, settings, limiter, cpu_threshold)

    return fire


async def _quiesce(recv: MemoryObjectReceiveStream[ChangeBatch], window_ms: float) -> ChangeBatch:
    latest: ChangeBatch = _NO_CHANGES
    async for _pass in _forever():
        with anyio.move_on_after(window_ms / 1000.0) as scope:
            latest = await recv.receive()
        match scope.cancelled_caught:
            case True:
                return latest
            case False:
                pass
    return latest  # pragma: no cover  # _forever returns only through quiet-window cancellation


def _debounce(inner: Fire, window_ms: int, *, collapse: bool) -> tuple[Fire, Worker]:
    # Worker lifetime belongs to the caller's task group and stop scope, not to this closure.
    send, recv = anyio.create_memory_object_stream[ChangeBatch](1)

    async def signal(changes: ChangeBatch) -> None:  # noqa: RUF029  # async required: trigger loop awaits this as a Fire coroutine
        try:
            match send.statistics().current_buffer_used:
                case 0:
                    send.send_nowait(changes)
                case _:
                    return
        except anyio.ClosedResourceError:
            return

    async def worker() -> None:
        try:
            async with recv:
                async for changes in recv:
                    match collapse:
                        case False:
                            await inner(changes)
                        case True:
                            pass
                    latest = await _quiesce(recv, window_ms)
                    match collapse:
                        case True:
                            await inner(latest or changes)
                        case False:
                            pass
        finally:
            await send.aclose()

    return signal, worker


def _watch_filter(spec: Watch) -> BaseFilter:
    ignores = spec.ignore_patterns
    match spec.filter:
        case WatchFilter.DEFAULT:
            # Extend, don't replace: a bare ignore_entity_patterns= drops watchfiles' built-in file-noise suppression
            # (.pyc/.DS_Store/swap/editor-lock); keep those and append the spec's own ignores.
            return DefaultFilter(ignore_entity_patterns=(*DefaultFilter.ignore_entity_patterns, *ignores))
        case WatchFilter.PYTHON:
            return PythonFilter(ignore_paths=ignores, extra_extensions=(".pyi",))
        case _ as unreachable:
            assert_never(unreachable)


async def _watch(spec: Watch, fire: Fire, stop: anyio.Event) -> None:
    async for changes in awatch(
        *spec.paths,
        watch_filter=_watch_filter(spec),
        debounce=spec.debounce,
        step=spec.step,
        stop_event=stop,
        rust_timeout=1000,  # cap the Rust watcher's blocking wait at 1s so a stop_event is honored within ≤1s, not ≤5s
        yield_on_timeout=True,  # empty-set yields on each 1s timeout are routed safely by the empty-batch fire path
        force_polling=spec.force_polling,
        poll_delay_ms=spec.poll_delay_ms,
        recursive=spec.recursive,
        ignore_permission_denied=spec.ignore_permission_denied,
    ):
        batch = tuple((str(kind), path) for kind, path in sorted(changes, key=itemgetter(1)))
        # yield_on_timeout surfaces an empty changeset every rust_timeout window purely to re-check the stop_event;
        # those are stop-latency heartbeats, not file events, so they must not fire the action.
        match batch:
            case ():
                pass
            case _:
                _LOG.info("watch.fire", changes=len(batch), paths=tuple(path for _, path in batch[:8]))
                await fire(batch)


def _emit_automation_fault(exc: Exception, settings: AssaySettings, action: Action | None) -> None:
    claim, verb = _label(action) if action is not None else (Claim.STATIC, "automation")
    _emit(envelope(Fault((), RailStatus.FAULTED, f"automation fire: {type(exc).__name__}: {exc}"), claim=claim, verb=verb, run_id=settings.run_id))


async def _coalesce_missed_fire(fire: Fire, missed: int) -> None:
    _LOG.warning("schedule.coalesced", missed=missed)
    await fire(_NO_CHANGES)


async def _jittered_fire(fire: Fire, settings: AssaySettings, changes: ChangeBatch) -> None:
    await anyio.sleep((int.from_bytes(hashlib.sha256(settings.run_id.encode()).digest()[:4], "big") % _JITTER_MS) / 1000.0)
    await fire(changes)


async def _fire_with_coalesce(fire: Fire, settings: AssaySettings, changes: ChangeBatch, missed: int) -> int:
    await _jittered_fire(fire, settings, changes)
    match missed:
        case coalesced if coalesced:
            await _coalesce_missed_fire(fire, coalesced)
            return 0
        case _:
            return 0


def _hardened_fire(fire: Fire, settings: AssaySettings, action: Action | None = None) -> Fire:
    # Single-flight gate: mid-fire ticks accumulate in missed for one deferred catch-up; scheduling telemetry routes to structlog, not Envelope notes.
    running = False
    missed = 0

    async def hardened(changes: ChangeBatch = _NO_CHANGES) -> None:
        nonlocal running, missed
        match running:
            case True:
                missed += 1
            case False:
                running = True
                try:
                    missed = await _fire_with_coalesce(fire, settings, changes, missed)
                except Exception as exc:  # noqa: BLE001  # automation boundary: exceptions emit one FAULTED row instead of escaping the task group
                    _emit_automation_fault(exc, settings, action)
                finally:
                    running = False

    return hardened


async def _schedule(spec: Schedule, fire: Fire, stop: anyio.Event) -> None:
    # aiocron computes next wakeup instants only; the loop and stop integration are owned here.
    tz = ZoneInfo(spec.timezone)
    cron = aiocron.crontab(spec.cron, start=False, tz=tz)
    try:
        while not stop.is_set():
            await cron.next()
            if not stop.is_set():
                await fire(_NO_CHANGES)
    finally:
        cron.stop()


def _armed(action: Action, settings: AssaySettings, *, limiter: anyio.CapacityLimiter, ceiling: float | None) -> tuple[Fire, Worker | None]:
    match action:
        case Debounce(action=inner, window_ms=window, collapse=coalesce):
            return _debounce(_fire(inner, settings, limiter=limiter, cpu_threshold=ceiling), window, collapse=coalesce)
        case Rail() | Program() | Sequence():
            return _fire(action, settings, limiter=limiter, cpu_threshold=ceiling), None


async def _co_resident(tg: TaskGroup, resident: Worker | None, stop: anyio.Event) -> None:
    match resident:
        case None:
            return
        case _:
            tg.start_soon(resident)
            await stop.wait()
            tg.cancel_scope.cancel()


async def _drive_watch(spec: Watch, action: Action, settings: AssaySettings, *, limiter: anyio.CapacityLimiter, stop: anyio.Event) -> None:
    fire, worker = _armed(action, settings, limiter=limiter, ceiling=spec.cpu_threshold)
    async with anyio.create_task_group() as tg:
        tg.start_soon(_watch, spec, fire, stop)
        await _co_resident(tg, worker, stop)


async def _drive_schedule(spec: Schedule, action: Action, settings: AssaySettings, *, limiter: anyio.CapacityLimiter, stop: anyio.Event) -> None:
    fire, worker = _armed(action, settings, limiter=limiter, ceiling=spec.cpu_threshold)
    scheduled = _hardened_fire(fire, settings, action) if worker is None else fire
    async with anyio.create_task_group() as tg:
        tg.start_soon(_schedule, spec, scheduled, stop)
        await _co_resident(tg, worker, stop)


async def _run_trigger(trigger: Trigger, action: Action, settings: AssaySettings, *, limiter: anyio.CapacityLimiter, stop: anyio.Event) -> None:
    match trigger:
        case Manual():
            await _fire(action, settings, limiter=limiter, cpu_threshold=None)(_NO_CHANGES)
        case Watch() as spec:
            await _drive_watch(spec, action, settings, limiter=limiter, stop=stop)
        case Schedule() as spec:
            await _drive_schedule(spec, action, settings, limiter=limiter, stop=stop)


async def drive(trigger: Trigger, action: Action, settings: AssaySettings) -> None:
    """Run an action each time its trigger fires, writing Envelope JSON to stdout.

    Manual triggers fire once and return.  Watch and Schedule triggers loop until
    their underlying stop event fires or the process exits.  Setup failures
    (invalid cron expression, unresolvable timezone, bad watch path, bad regex)
    are caught from the ExceptionGroup raised by anyio and emitted as a single
    FAULTED Envelope rather than propagating to the caller.
    """
    limiter = anyio.CapacityLimiter(1)
    stop = anyio.Event()

    try:
        await _run_trigger(trigger, action, settings, limiter=limiter, stop=stop)
    except* (OSError, ValueError, ZoneInfoNotFoundError, re.error) as errors:
        claim, verb = _label(action)
        message = "; ".join(str(exc) for exc in errors.exceptions)
        _emit(envelope(Fault((), RailStatus.FAULTED, f"automation setup: {message}"), claim=claim, verb=verb, run_id=settings.run_id))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ChangeBatch", "Fire", "Worker", "drive", "is_governed"]
