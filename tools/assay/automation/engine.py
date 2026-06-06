"""Run automation triggers through Assay rails and direct programs."""

from collections.abc import Callable, Coroutine
import hashlib
from itertools import count
from operator import itemgetter
import sys
from typing import TYPE_CHECKING
from zoneinfo import ZoneInfo, ZoneInfoNotFoundError

import aiocron  # type: ignore[import-untyped]  # aiocron ships no py.typed marker
import anyio
from expression import Error, Ok, Result
import msgspec
import psutil  # typed via the types-psutil stub (psutil ships no py.typed marker)
import structlog
from watchfiles import awatch, DefaultFilter, PythonFilter

from tools.assay.automation.model import Debounce, Manual, Program, Rail, Schedule, Sequence, Watch
from tools.assay.composition.registry import rail, REGISTRY
from tools.assay.composition.settings import ArtifactScope
from tools.assay.core.engine import _spawn  # noqa: PLC2701  # intra-package woven spawn; run_check's anyio.run would nest under drive's
from tools.assay.core.model import Check, Claim, Counts, envelope, Fault, fold, Input, Language, Mode, Report, Runner, Tool
from tools.assay.core.routing import Routed, Scope
from tools.assay.core.status import join, RailStatus


if TYPE_CHECKING:
    from collections.abc import AsyncIterator

    from anyio.abc import TaskGroup  # annotation-only; anyio.abc is not exposed from the root anyio import
    from anyio.streams.memory import MemoryObjectReceiveStream
    from watchfiles import BaseFilter

    from tools.assay.automation.model import Action, Trigger
    from tools.assay.composition.settings import AssaySettings
    from tools.assay.core.model import Bind, Envelope


# --- [TYPES] ----------------------------------------------------------------------------

# Each fire reports only through the NDJSON Envelope it emits.
type Fire = Callable[[], Coroutine[None, None, None]]


# --- [CONSTANTS] ------------------------------------------------------------------------

# The wire carries a string tag; this boundary resolves watchfiles filters and degrades unknown tags to DefaultFilter.
_FILTERS: dict[str, BaseFilter] = {"default": DefaultFilter(), "python": PythonFilter()}

_PROGRAM_ROUTED: Routed = Routed(language=Language.PYTHON, scope=Scope.FULL)

_ENCODER = msgspec.json.Encoder(order="deterministic")
_LOG: structlog.stdlib.BoundLogger = structlog.get_logger("assay.automation")
_JITTER_MS: int = 100


# --- [MODELS] ---------------------------------------------------------------------------


class _RunState(msgspec.Struct, frozen=True, gc=False):
    # Cron re-entry cell: ticks coalesce into `missed`, and single-task updates keep it lock-free.
    running: bool = False
    missed: int = 0


# --- [OPERATIONS] -----------------------------------------------------------------------


def _emit(line: Envelope) -> None:
    # Per-line flushing keeps watch/schedule consumers current and preserves Envelope framing across concurrent fires.
    sys.stdout.buffer.write(_ENCODER.encode(line) + b"\n")
    sys.stdout.buffer.flush()


def _governed(threshold: float | None) -> bool:
    match threshold:
        case None:
            return False
        # The short sample pauses only the fire task; fractional thresholds map directly to percent utilization.
        case _:
            return psutil.cpu_percent(0.1) >= threshold * 100.0


def _resolve(action: Rail) -> Bind | None:
    return next((b for b in REGISTRY if b.claim is action.claim and b.verb == action.verb), None)


def _label(action: Action) -> tuple[Claim, str]:
    match action:
        case Rail(claim=c, verb=v):
            return c, v
        # Program and Sequence share the canonical direct-program label.
        case Program() | Sequence():
            return Claim.STATIC, "program"
        case Debounce(action=inner):
            return _label(inner)  # pure wrapper: project the wrapped action's label


def _program_check(argv: tuple[str, ...]) -> Check:
    # DIRECT plus Input.NONE runs argv verbatim while satisfying the shared routing shape.
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
    # Avoid nested anyio.run by awaiting the same checked/traced/retried spawn weave that fan_out uses.
    # Process exit codes stay on the Completed channel; only spawn and timeout failures become Faults.
    # The trailing None deadline selects _guarded's run-to-completion budget (check.tool.timeout), so a
    # catalog tool with no timeout (dotnet build, stryker) runs unbounded by design here, not capped per fire.
    if not action.argv:
        return Error(Fault(("program",), RailStatus.FAULTED, "program argv must be non-empty"))
    check = _program_check(action.argv)
    scope = ArtifactScope.open(settings, Claim.STATIC)
    outcome = await _spawn(check, settings)(check, settings, scope, _PROGRAM_ROUTED, None)
    match outcome:
        case Result(tag="ok", ok=done):
            return Ok(fold(Claim.STATIC, "program", (done,)))
        case Result(error=fault):
            return Error(fault)


def _emitted(line: Envelope) -> Envelope:
    _emit(line)
    return line


def _rail_outcome(action: Rail, settings: AssaySettings) -> Envelope:
    # Registry rails write their own Envelope, so this branch returns it instead of emitting twice.
    # Binding and Raw decode failures fold to a FAULTED Envelope at the automation boundary.
    bind = _resolve(action)
    match bind:
        case None:
            return _emitted(
                envelope(Fault((), RailStatus.FAULTED, f"unbound rail: {action.claim.value}:{action.verb}"), claim=action.claim, verb=action.verb)
            )
        case _:
            try:
                params = msgspec.json.decode(bytes(action.params), type=bind.params) if action.params else bind.params()
            except msgspec.DecodeError as exc:
                return _emitted(envelope(Fault((), RailStatus.FAULTED, str(exc)[:1024]), claim=action.claim, verb=action.verb))
            return rail(bind, settings)(params)


async def _program_envelope(action: Program, settings: AssaySettings) -> Envelope:
    # Programs have no registry runner, so this boundary wraps their Result into the automation Envelope.
    match await _program_outcome(action, settings):
        case Result(tag="ok", ok=report):
            payload: Report | Fault = report
        case Result(error=fault):
            payload = fault
    return _emitted(envelope(payload, claim=Claim.STATIC, verb="program"))


async def _emit_leaf(leaf: Action, settings: AssaySettings, limiter: anyio.CapacityLimiter, cpu_threshold: float | None) -> RailStatus:
    # Governor checks run before the per-drive limiter; slow fires queue on one token instead of re-entering.
    match _governed(cpu_threshold):
        case True:
            claim, verb = _label(leaf)
            # SKIP bypasses fold because EMPTY has higher severity; counts still mark one governed leaf.
            skip = Report(claim, verb, RailStatus.SKIP, Counts(1, 0, 1), notes=(f"governed: cpu>={cpu_threshold or 0.0:.0%}",))
            return _emitted(envelope(skip, claim=claim, verb=verb)).status
        # The limiter wraps ONLY true leaves (Rail/Program); Sequence/Debounce recurse OUTSIDE it so the
        # capacity-1 token is not re-acquired by the same borrower task (anyio "already holding a token").
        case False:
            match leaf:
                case Rail() as r:
                    async with limiter:
                        return _rail_outcome(r, settings).status
                case Program() as p:
                    async with limiter:
                        return (await _program_envelope(p, settings)).status
                case Sequence() as s:
                    return await _sequence(s.actions, settings, limiter, cpu_threshold)
                case Debounce(action=inner):
                    return await _emit_leaf(inner, settings, limiter, cpu_threshold)


async def _forever() -> AsyncIterator[int]:  # noqa: RUF029  # async is required: callers `async for` over this inside async cycles; a sync generator is not async-iterable
    # Constant-stack indefinite async driver: the `for` is iteration over the stdlib infinite counter, not domain branching.
    # Callers thread their own state through the loop body and `return` on completion (sequence exhaustion, quiet window).
    for i in count():
        yield i


async def _sequence(leaves: tuple[Action, ...], settings: AssaySettings, limiter: anyio.CapacityLimiter, cpu_threshold: float | None) -> RailStatus:
    # Fold by severity across leaves at constant stack depth; halt on definitive defects or Fault statuses.
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
    # One captured limiter and ceiling keep a whole Sequence single-flight across fires.
    async def fire() -> None:
        match action:
            case Sequence(actions=acts):
                _ = await _sequence(acts, settings, limiter, cpu_threshold)
            case Rail() | Program():
                _ = await _emit_leaf(action, settings, limiter, cpu_threshold)
            case Debounce(action=inner):
                _ = await _emit_leaf(inner, settings, limiter, cpu_threshold)

    return fire


async def _quiesce(recv: MemoryObjectReceiveStream[None], window_ms: float) -> None:
    # Drain signals at constant stack depth until one quiet window elapses with no further signal.
    async for _pass in _forever():
        with anyio.move_on_after(window_ms / 1000.0) as scope:
            await recv.receive()
        match scope.cancelled_caught:
            case True:
                return
            case False:
                pass


def _debounce(inner: Fire, window_ms: int, *, collapse: bool) -> tuple[Fire, Fire]:
    # A size-1 channel coalesces storm signals; the worker lives under the trigger task group and stop scope.
    send, recv = anyio.create_memory_object_stream[None](1)

    async def signal() -> None:  # noqa: RUF029  # async is required: the trigger loop awaits this as a Fire; send_nowait is the non-blocking coalescing notify
        match send.statistics().current_buffer_used:
            case 0:
                send.send_nowait(None)
            case _:
                return

    async def worker() -> None:
        # recv is an async iterator: each yielded signal opens one debounce cycle at constant stack depth.
        async for _signal in recv:
            match collapse:
                case False:
                    await inner()
                case _:
                    pass
            await _quiesce(recv, window_ms)
            match collapse:
                case True:
                    await inner()
                case _:
                    pass

    return signal, worker


async def _watch(spec: Watch, fire: Fire, stop: anyio.Event) -> None:
    # awatch honors the shared stop_event directly.
    async for changes in awatch(
        *spec.paths,
        watch_filter=_watch_filter(spec),
        debounce=spec.debounce,
        step=spec.step,
        stop_event=stop,
        force_polling=spec.force_polling,
        poll_delay_ms=spec.poll_delay_ms,
        recursive=spec.recursive,
        ignore_permission_denied=spec.ignore_permission_denied,
    ):
        _LOG.info("watch.fire", changes=len(changes), paths=tuple(path for _, path in sorted(changes, key=itemgetter(1))[:8]))
        await fire()


def _watch_filter(spec: Watch) -> BaseFilter:
    if spec.ignore_patterns:
        if spec.filter != "default":
            raise ValueError(f"unknown watch filter: {spec.filter}")
        return DefaultFilter(ignore_entity_patterns=spec.ignore_patterns)
    if spec.filter in _FILTERS:
        return _FILTERS[spec.filter]
    raise ValueError(f"unknown watch filter: {spec.filter}")


def _hardened_fire(fire: Fire, settings: AssaySettings) -> Fire:
    # Cron fires are single-flight: mid-fire ticks coalesce, then one catch-up runs after the active fire.
    # A run_id hash adds deterministic start jitter; scheduling telemetry stays on structlog, not Envelope notes.
    state: list[_RunState] = [_RunState()]

    async def hardened() -> None:
        match state[0].running:
            case True:
                state[0] = msgspec.structs.replace(state[0], missed=state[0].missed + 1)
            case False:
                state[0] = msgspec.structs.replace(state[0], running=True)
                try:
                    await anyio.sleep((int.from_bytes(hashlib.sha256(settings.run_id.encode()).digest()[:4], "big") % _JITTER_MS) / 1000.0)
                    await fire()
                    match state[0].missed:
                        case missed if missed:
                            _LOG.warning("schedule.coalesced", missed=missed)
                            state[0] = msgspec.structs.replace(state[0], missed=0)
                            await fire()
                        case _:
                            pass
                finally:
                    state[0] = msgspec.structs.replace(state[0], running=False)

    return hardened


async def _schedule(spec: Schedule, fire: Fire, stop: anyio.Event) -> None:
    # aiocron has no stop_event; a shared stop waiter parks the task and cron.stop cancels the armed callback.
    tz = ZoneInfo(spec.timezone) if spec.timezone else None
    cron = aiocron.crontab(spec.cron, func=fire, start=False, tz=tz)
    try:
        cron.start()
        await stop.wait()
    finally:
        cron.stop()


def _armed(action: Action, settings: AssaySettings, *, limiter: anyio.CapacityLimiter, ceiling: float | None) -> tuple[Fire, Fire | None]:
    match action:
        case Debounce(action=inner, window_ms=window, collapse=coalesce):
            return _debounce(_fire(inner, settings, limiter=limiter, cpu_threshold=ceiling), window, collapse=coalesce)
        case Rail() | Program() | Sequence():
            return _fire(action, settings, limiter=limiter, cpu_threshold=ceiling), None


async def drive(trigger: Trigger, action: Action, settings: AssaySettings) -> None:
    """Run an action each time its trigger fires."""
    limiter = anyio.CapacityLimiter(1)
    stop = anyio.Event()

    async def _co_resident(tg: TaskGroup, worker: Fire | None) -> None:
        match worker:
            case None:
                return
            case _:
                tg.start_soon(worker)
                await stop.wait()
                tg.cancel_scope.cancel()

    try:
        match trigger:
            case Manual():
                await _fire(action, settings, limiter=limiter, cpu_threshold=None)()
            case Watch(cpu_threshold=ceiling) as spec:
                fire, worker = _armed(action, settings, limiter=limiter, ceiling=ceiling)
                async with anyio.create_task_group() as tg:
                    tg.start_soon(_watch, spec, fire, stop)
                    await _co_resident(tg, worker)
            case Schedule(cpu_threshold=ceiling) as spec:
                fire, worker = _armed(action, settings, limiter=limiter, ceiling=ceiling)
                # Top-level Debounce already coalesces via its worker, so only direct scheduled fires need hardening.
                scheduled = _hardened_fire(fire, settings) if worker is None else fire
                async with anyio.create_task_group() as tg:
                    tg.start_soon(_schedule, spec, scheduled, stop)
                    await _co_resident(tg, worker)
    except* (OSError, ValueError, ZoneInfoNotFoundError) as errors:
        claim, verb = _label(action)
        message = "; ".join(str(exc) for exc in errors.exceptions)
        _emit(envelope(Fault((), RailStatus.FAULTED, f"automation setup: {message}"), claim=claim, verb=verb, run_id=settings.run_id))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Fire", "drive"]
