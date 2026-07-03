"""Drive automation triggers through rail, program, sequence, and debounce actions.

Each fire emits newline-delimited Envelope JSON to stdout while telemetry remains on stderr.
The capacity limiter keeps one leaf action active per driver; every spawn rides the Executor port.
"""

from collections.abc import Callable, Coroutine
from contextvars import ContextVar
from dataclasses import dataclass, replace
from datetime import datetime, UTC
from functools import partial
import hashlib
from operator import itemgetter
import re
import sys
from typing import assert_never, TYPE_CHECKING
from zoneinfo import ZoneInfo, ZoneInfoNotFoundError

import anyio
from anyio import to_thread
from expression import Error, Ok, Result
import msgspec
import psutil  # typed via the types-psutil stub (psutil ships no py.typed marker)
import structlog
from watchfiles import awatch, DefaultFilter, PythonFilter

from tools.assay.automation.model import Debounce, describe, Edge, Manual, Program, Rail, Schedule, Sequence, Watch, WatchFilter
from tools.assay.composition.catalog import select
from tools.assay.composition.registry import rail, REGISTRY
from tools.assay.composition.store import ArtifactScope
from tools.assay.core.exec import EngineExecutor
from tools.assay.core.model import Check, Claim, Counts, Envelope, envelope, Fault, Language, RailStatus, Report, ToolArgs
from tools.assay.core.routing import Routed, Scope
from tools.assay.diagnostics import fold


if TYPE_CHECKING:
    from typing import Protocol

    from anyio.abc import TaskGroup  # annotation-only; anyio.abc is not exposed from the root anyio import
    from anyio.streams.memory import MemoryObjectReceiveStream
    from watchfiles import BaseFilter

    from tools.assay.automation.model import Action, Trigger
    from tools.assay.composition.settings import AssaySettings
    from tools.assay.core.exec import Executor
    from tools.assay.core.model import Bind, Tool

    class _CronTrigger(Protocol):
        def get_next_fire_time(self, previous_fire_time: datetime | None, now: datetime) -> datetime | None: ...

    def _cron_from_crontab(expr: str, timezone: ZoneInfo) -> _CronTrigger: ...

else:
    from apscheduler.triggers.cron import CronTrigger as _CronTrigger  # type: ignore[import-untyped]  # APScheduler ships no py.typed marker.

    def _cron_from_crontab(expr: str, timezone: ZoneInfo) -> _CronTrigger:
        return _CronTrigger.from_crontab(expr, timezone=timezone)


# --- [TYPES] ----------------------------------------------------------------------------

type ChangeBatch = tuple[tuple[str, str], ...]
type Fire = Callable[[ChangeBatch], Coroutine[None, None, None]]
type Worker = Callable[[], Coroutine[None, None, None]]
type RailOutcome = tuple[Envelope, bool]


@dataclass(frozen=True, slots=True)
class _Drive:
    """Per-run drive context: settings, the shared one-leaf limiter, the execution port, and the CPU ceiling."""

    settings: AssaySettings
    limiter: anyio.CapacityLimiter
    executor: Executor
    ceiling: float | None = None


# --- [CONSTANTS] ------------------------------------------------------------------------

# Context-local priming prevents one driver from inheriting another driver's CPU sample state.
_CPU_PRIMED: ContextVar[bool] = ContextVar("assay_cpu_primed", default=False)
_ENCODER = msgspec.json.Encoder(order="deterministic")
_JITTER_MS: int = 100
_NO_CHANGES: ChangeBatch = ()
_PROGRAM_ROUTED: Routed = Routed(language=Language.PYTHON, scope=Scope.FULL)
# The one total row for automation Program actions; a missing row is an immediate import-time KeyError.
_PROGRAM_TOOL: Tool = next(t for t in select(Claim.STATIC, Language.PYTHON) if t.name == "program")

# --- [SERVICES] -------------------------------------------------------------------------

_LOG: structlog.stdlib.BoundLogger = structlog.get_logger("assay.automation")

# --- [OPERATIONS] -----------------------------------------------------------------------


def _emit(line: Envelope) -> None:
    # Per-line flush preserves Envelope framing across concurrent fires.
    sys.stdout.buffer.write(_ENCODER.encode(line) + b"\n")
    sys.stdout.buffer.flush()


def is_governed(threshold: float | None) -> bool:
    """Decide whether the CPU governor should suppress a fire.

    The first sampled call primes the psutil kernel counter for the current async context;
    later calls reuse non-blocking samples.

    Args:
        threshold: Fractional CPU ceiling in (0, 1]; ``None`` and ``0.0`` both disable suppression
            (never gate on CPU), so the governor only engages above a positive ceiling.

    Returns:
        True when the current CPU sample meets or exceeds the positive ceiling.
    """
    match threshold:
        case None | 0.0:
            return False
        case _:
            if not _CPU_PRIMED.get():
                psutil.cpu_percent(0.1)
                _CPU_PRIMED.set(True)
            return psutil.cpu_percent(interval=None) >= threshold * 100.0


def _resolve(action: Rail) -> Bind | None:
    return next((b for b in REGISTRY if b.claim is action.claim and b.verb == action.verb), None)


def _label(action: Action) -> tuple[Claim, str]:
    # Canonical (claim, verb) routing pair for the Envelope; the human telemetry string is owned by model.describe.
    match action:
        case Rail(claim=c, verb=v):
            return c, v
        case Program() | Sequence():
            return Claim.STATIC, "program"
        case Debounce(action=inner):
            return _label(inner)


def _program_check(argv: tuple[str, ...]) -> Check:
    # The catalog `program` row owns arbitrary automation argv; the whole command rides the {argv*} splice.
    return Check(tool=_PROGRAM_TOOL, args=ToolArgs(argv=argv))


async def _program_outcome(action: Program, ctx: _Drive) -> Result[Report, Fault]:
    # Program deadlines stay with the invoked tool; this boundary converts spawn/process failures to Faults.
    if not action.argv:
        return Error(Fault(("program",), RailStatus.FAULTED, "program argv must be non-empty"))
    check = _program_check(action.argv)
    scope = ArtifactScope.open(ctx.settings, Claim.STATIC)
    # The port is synchronous (it owns its own loop), so the spawn hops to a thread like the Rail lane.
    outcome = await to_thread.run_sync(partial(ctx.executor.run, check, settings=ctx.settings, scope=scope, routed=_PROGRAM_ROUTED))
    match outcome:
        case Result(tag="ok", ok=done):
            return Ok(fold(Claim.STATIC, "program", (done,)))
        case Result(error=fault):
            return Error(fault)


def _emitted(line: Envelope) -> Envelope:
    _emit(line)
    return line


def _rail_outcome(action: Rail, ctx: _Drive) -> RailOutcome:
    # Decode failures fold to FAULTED Envelope rows instead of escaping the automation task.
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
            return rail(bind, ctx.settings, ctx.executor)(params), True


async def _program_envelope(action: Program, ctx: _Drive) -> Envelope:
    match await _program_outcome(action, ctx):
        case Result(tag="ok", ok=report):
            payload: Report | Fault = report
        case Result(error=fault):
            payload = fault
    return _emitted(envelope(payload, claim=Claim.STATIC, verb="program"))


async def _emit_leaf(leaf: Action, ctx: _Drive) -> RailStatus:
    match is_governed(ctx.ceiling):
        case True:
            claim, verb = _label(leaf)
            # Counts(1, 0, 1) preserves one governed leaf even though SKIP bypasses the rail fold; describe owns the label string.
            skip = Report(claim, verb, RailStatus.SKIP, Counts(1, 0, 1), notes=(f"governed: {describe(leaf)} cpu>={ctx.ceiling or 0.0:.0%}",))
            return _emitted(envelope(skip, claim=claim, verb=verb)).status
        # Recursive Sequence/Debounce leaves reuse the held limiter token; re-acquire raises in anyio.
        case False:
            match leaf:
                case Rail() as r:
                    async with ctx.limiter:
                        env, emitted = await to_thread.run_sync(_rail_outcome, r, ctx)
                        return (env if emitted else _emitted(env)).status
                case Program() as p:
                    async with ctx.limiter:
                        return (await _program_envelope(p, ctx)).status
                case Sequence() as s:
                    return await _sequence(s.actions, ctx)
                case Debounce(action=inner):
                    return await _emit_leaf(inner, ctx)


async def _sequence(leaves: tuple[Action, ...], ctx: _Drive) -> RailStatus:
    # Short-circuit fold: the first terminal status stops the leaf walk.
    folded = RailStatus.EMPTY
    for leaf in leaves:
        folded = RailStatus.dominant(folded, await _emit_leaf(leaf, ctx))
        match folded:
            case RailStatus.FAILED | RailStatus.BUSY | RailStatus.TIMEOUT | RailStatus.FAULTED:
                return folded
            case _:
                pass
    return folded


def _fire(action: Action, ctx: _Drive) -> Fire:
    # _emit_leaf is total over Action (Rail/Program/Sequence/Debounce), so the fire closure dispatches there once.
    async def fire(_changes: ChangeBatch) -> None:
        _ = await _emit_leaf(action, ctx)

    return fire


async def _quiesce(recv: MemoryObjectReceiveStream[ChangeBatch], window_ms: float) -> ChangeBatch:
    # Boundary kernel: drain until one quiet window elapses; the timeout scope is the sole exit.
    latest: ChangeBatch = _NO_CHANGES
    while True:
        with anyio.move_on_after(window_ms / 1000.0) as scope:
            latest = await recv.receive()
        if scope.cancelled_caught:
            return latest


def _debounce(inner: Fire, window_ms: int, *, edge: Edge) -> tuple[Fire, Worker]:
    # The caller owns worker lifetime through its task group and stop scope.
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
                    match edge:
                        case Edge.LEADING:
                            await inner(changes)
                            await _quiesce(recv, window_ms)  # drain the window; later events discarded
                        case Edge.TRAILING:
                            await inner((await _quiesce(recv, window_ms)) or changes)
        finally:
            await send.aclose()

    return signal, worker


def _watch_filter(spec: Watch) -> BaseFilter:
    ignores = spec.ignore_patterns
    match spec.filter:
        case WatchFilter.DEFAULT:
            # Supplying ignore_entity_patterns replaces watchfiles' built-in file-noise filters.
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
        rust_timeout=1000,  # bound stop latency to the local event loop instead of watchfiles' default wait
        yield_on_timeout=True,  # timeout heartbeats are filtered by the empty-batch path
        force_polling=spec.force_polling,
        poll_delay_ms=spec.poll_delay_ms,
        recursive=spec.recursive,
        ignore_permission_denied=spec.ignore_permission_denied,
    ):
        batch = tuple((str(kind), path) for kind, path in sorted(changes, key=itemgetter(1)))
        # Timeout heartbeats preserve stop responsiveness without firing actions.
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
    if missed:
        await _coalesce_missed_fire(fire, missed)
    return 0


def _hardened_fire(fire: Fire, settings: AssaySettings, action: Action | None = None) -> Fire:
    # Mid-fire ticks collapse to one catch-up fire; telemetry stays off the Envelope stream.
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


def _cron_trigger(spec: Schedule) -> _CronTrigger:
    return _cron_from_crontab(spec.cron, timezone=ZoneInfo(spec.timezone))


def _next_cron_delay(cron: _CronTrigger, previous: datetime | None) -> tuple[datetime | None, float]:
    now = datetime.now(UTC)
    fire_at = cron.get_next_fire_time(previous, now)
    return (None, 0.0) if fire_at is None else (fire_at, max(0.0, (fire_at - datetime.now(fire_at.tzinfo or UTC)).total_seconds()))


async def _schedule(spec: Schedule, fire: Fire, stop: anyio.Event) -> None:
    cron = _cron_trigger(spec)
    previous: datetime | None = None
    while not stop.is_set():
        previous, delay = _next_cron_delay(cron, previous)
        if previous is None:
            return
        with anyio.move_on_after(delay) as scope:
            await stop.wait()
        if scope.cancelled_caught and not stop.is_set():
            await fire(_NO_CHANGES)


def _armed(action: Action, ctx: _Drive) -> tuple[Fire, Worker | None]:
    match action:
        case Debounce(action=inner, window_ms=window, edge=edge):
            return _debounce(_fire(inner, ctx), window, edge=edge)
        case Rail() | Program() | Sequence():
            return _fire(action, ctx), None


async def _co_resident(tg: TaskGroup, resident: Worker | None, stop: anyio.Event) -> None:
    match resident:
        case None:
            return
        case _:
            _ = tg.start_soon(resident)
            await stop.wait()
            tg.cancel_scope.cancel()


async def _drive(spec: Watch | Schedule, action: Action, ctx: _Drive, *, stop: anyio.Event, harden: bool) -> None:
    # One driver for watch and schedule: spec selects the wakeup source; harden coalesces missed schedule ticks while
    # watch relies on watchfiles' own debounce, and a debounce worker (worker is not None) already owns its catch-up.
    fire, worker = _armed(action, replace(ctx, ceiling=spec.cpu_threshold))
    driven = _hardened_fire(fire, ctx.settings, action) if harden and worker is None else fire
    async with anyio.create_task_group() as tg:
        match spec:
            case Watch():
                _ = tg.start_soon(_watch, spec, driven, stop)
            case Schedule():
                _ = tg.start_soon(_schedule, spec, driven, stop)
        await _co_resident(tg, worker, stop)


async def _run_trigger(trigger: Trigger, action: Action, ctx: _Drive, *, stop: anyio.Event) -> None:
    match trigger:
        case Manual():
            await _fire(action, ctx)(_NO_CHANGES)
        case Watch() as spec:
            await _drive(spec, action, ctx, stop=stop, harden=False)
        case Schedule() as spec:
            await _drive(spec, action, ctx, stop=stop, harden=True)


async def drive(trigger: Trigger, action: Action, settings: AssaySettings, *, executor: Executor | None = None) -> None:
    """Run an action for each trigger fire and emit Envelope JSON to stdout.

    Manual triggers fire once. Watch and schedule triggers loop until their stop event
    fires. Setup failures are collapsed from the anyio ExceptionGroup into one FAULTED
    Envelope row. Every spawn rides ``executor``; the engine-bound port when absent.
    """
    ctx = _Drive(settings=settings, limiter=anyio.CapacityLimiter(1), executor=executor if executor is not None else EngineExecutor())
    stop = anyio.Event()

    try:
        await _run_trigger(trigger, action, ctx, stop=stop)
    except* (OSError, ValueError, ZoneInfoNotFoundError, re.error) as errors:
        claim, verb = _label(action)
        message = "; ".join(str(exc) for exc in errors.exceptions)
        _emit(envelope(Fault((), RailStatus.FAULTED, f"automation setup: {message}"), claim=claim, verb=verb, run_id=settings.run_id))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ChangeBatch", "Fire", "Worker", "drive", "is_governed"]
