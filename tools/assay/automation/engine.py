"""Run automation triggers through the assay rail and program execution spine."""

from collections.abc import Callable, Coroutine
import hashlib
import sys
from typing import TYPE_CHECKING

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
    from anyio.abc import TaskGroup  # annotation-only (TC002): the _co_resident spawn surface; anyio.abc is not an attribute of the bare anyio import
    from anyio.streams.memory import MemoryObjectReceiveStream  # annotation-only (TC002): the _quiesce recv type from create_memory_object_stream
    from watchfiles import BaseFilter  # annotation-only (TC002): the _FILTERS value type, never constructed by name

    from tools.assay.automation.model import Action, Trigger  # annotation-only (TC001)
    from tools.assay.composition.settings import AssaySettings  # annotation-only (TC001)
    from tools.assay.core.model import Bind, Envelope  # annotation-only (TC001)


# --- [TYPES] ----------------------------------------------------------------------------

# The per-fire closure returns nothing: each fire's sole observable is the NDJSON line it emits.
type Fire = Callable[[], Coroutine[None, None, None]]


# --- [MODELS] ---------------------------------------------------------------------------


class _RunState(msgspec.Struct, frozen=True, gc=False):
    # Scheduled-fire re-entrancy cell. running gates re-entry (a mid-fire cron tick is COALESCED, not queued);
    # missed counts coalesced ticks the in-flight drain must catch up. Single-task-loop confined — no await
    # between a read and the structs.replace that writes the successor — so the transition is race-free without a lock.
    running: bool = False
    missed: int = 0


# --- [CONSTANTS] ------------------------------------------------------------------------

# The wire carries a string filter tag and `engine` resolves the `BaseFilter` here, so the model
# never imports `watchfiles`; a drifting tag degrades to `DefaultFilter` rather than raising.
_FILTERS: dict[str, BaseFilter] = {"default": DefaultFilter(), "python": PythonFilter()}

_PROGRAM_ROUTED: Routed = Routed(language=Language.PYTHON, scope=Scope.FULL)  # NONE-route seed: `place` emits one empty tail

_ENCODER = msgspec.json.Encoder(order="deterministic")  # the sole automation stdout codec, cached once
_LOG: structlog.stdlib.BoundLogger = structlog.get_logger("assay.automation")  # scheduling operational-event rail (coalesced-tick recovery)
_JITTER_MS: int = 100  # deterministic per-run cron jitter window: a hashlib digest over run_id de-syncs a fleet sharing one schedule


# --- [OPERATIONS] -----------------------------------------------------------------------


def _emit(line: Envelope) -> None:
    # Sole stdout writer for the automation arm. Per-line flush is load-bearing: a long-lived Watch piped to a
    # consumer must surface each Envelope at fire time, not batched at exit, and keeps line-framing safe under
    # the concurrent cron.start task and the per-leaf emits of a Sequence fold.
    sys.stdout.buffer.write(_ENCODER.encode(line) + b"\n")
    sys.stdout.buffer.flush()


def _governed(threshold: float | None) -> bool:
    match threshold:
        case None:
            return False
        # 0.1s sample blocks the fire task briefly but never the awatch/cron drivers; threshold*100 so a
        # fractional ceiling (0.85) gates at 85% measured utilization.
        case _:
            return psutil.cpu_percent(0.1) >= threshold * 100.0


def _resolve(action: Rail) -> Bind | None:
    # None on an unbound verb so the caller folds to a FAULTED Fault at the fire boundary, never a raised KeyError.
    return next((b for b in REGISTRY if b.claim is action.claim and b.verb == action.verb), None)


def _label(action: Action) -> tuple[Claim, str]:
    # Project an Action to its (claim, verb) envelope label.
    match action:
        case Rail(claim=c, verb=v):
            return c, v
        # Program/Sequence have no single rail identity: fold under the canonical DIRECT label every non-rail fire shares.
        case Program() | Sequence():
            return Claim.STATIC, "program"
        case Debounce(action=inner):
            return _label(inner)  # pure wrapper: project the wrapped action's label


def _program_check(argv: tuple[str, ...]) -> Check:
    # Runner.DIRECT lays the prefix as () so argv runs verbatim; Input.NONE makes routing.place emit one empty
    # argv tail, so no path projection bleeds into a free-form program. Language/claim/mode are inert for a
    # NONE route (suffix sets never read), so every Program folds under Claim.STATIC with RUN mode for live streaming.
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
    # run_check calls anyio.run unconditionally, which would nest under drive's own anyio.run; so this mirrors
    # fan_out._into and awaits the same checked ▷ traced ▷ retried _spawn weave directly. A non-zero process exit
    # rides the Ok channel as Completed{FAILED}; only a spawn failure/timeout takes Error. The Completed folds
    # through core/model.fold (the sole count-derivation site) so the engine never authors a Fault for an exit code.
    check = _program_check(action.argv)
    scope = ArtifactScope.open(settings, Claim.STATIC)
    outcome = await _spawn(check, settings)(check, settings, scope, _PROGRAM_ROUTED, None)
    match outcome:
        case Result(tag="ok", ok=done):
            return Ok(fold(Claim.STATIC, "program", (done,)))
        case Result(error=fault):
            return Error(fault)


def _emitted(line: Envelope) -> Envelope:
    # Side-effecting seam: write via the sole automation writer, return the line for the fold.
    _emit(line)
    return line


def _rail_outcome(action: Rail) -> Envelope:
    # The one path that reuses the CLI's emitter: rail(bind) writes its OWN Envelope (the registry is the sole
    # stdout writer for a rail), so the engine must NOT re-emit — it returns the runner's Envelope for the fold.
    # The opaque Raw params decode is deferred to here; an unresolved verb or malformed payload folds to a
    # FAULTED Fault written through the engine's own _emit.
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
            return rail(bind)(params)


async def _program_envelope(action: Program, settings: AssaySettings) -> Envelope:
    # Project either _program_outcome Result channel onto the canonical envelope, written through the engine's
    # own _emit (a Program has no registry runner to write for it).
    match await _program_outcome(action, settings):
        case Result(tag="ok", ok=report):
            payload: Report | Fault = report
        case Result(error=fault):
            payload = fault
    return _emitted(envelope(payload, claim=Claim.STATIC, verb="program"))


async def _emit_leaf(leaf: Action, settings: AssaySettings, limiter: anyio.CapacityLimiter, cpu_threshold: float | None) -> RailStatus:
    # Governor short-circuits first, then work runs under the per-drive limiter so a fire slower than its
    # trigger cadence queues on the single token rather than re-entering a leased Action into spurious BUSY.
    match _governed(cpu_threshold):
        case True:
            claim, verb = _label(leaf)
            # status pinned to SKIP directly, NOT folded through core/model.fold: that fold seeds at EMPTY
            # (severity 1) and join is max-by-severity, so a SKIP (severity 0) leaf would be masked to EMPTY.
            # Counts(1, 0, 1) mirrors _count's (ok, failed) projection for a SKIP so the rollup stays honest.
            skip = Report(claim, verb, RailStatus.SKIP, Counts(1, 0, 1), notes=(f"governed: cpu>={cpu_threshold or 0.0:.0%}",))
            return _emitted(envelope(skip, claim=claim, verb=verb)).status
        case False:
            async with limiter:
                match leaf:
                    case Rail() as r:
                        return _rail_outcome(r).status
                    case Program() as p:
                        return (await _program_envelope(p, settings)).status
                    case Sequence() as s:
                        return await _sequence(s.actions, settings, limiter, cpu_threshold)
                    case Debounce(action=inner):
                        return await _emit_leaf(inner, settings, limiter, cpu_threshold)


async def _sequence(
    leaves: tuple[Action, ...],
    settings: AssaySettings,
    limiter: anyio.CapacityLimiter,
    cpu_threshold: float | None,
    folded: RailStatus = RailStatus.EMPTY,
) -> RailStatus:
    # Fold the leaves by max-severity join, halting on a definitive defect or any Fault leaf. join is the
    # module-scope semilattice operator, not a method: RailStatus subclasses str whose own join is str.join.
    match leaves:
        case (head, *tail):
            advanced = join(folded, await _emit_leaf(head, settings, limiter, cpu_threshold))
            match advanced:
                case RailStatus.FAILED | RailStatus.BUSY | RailStatus.TIMEOUT | RailStatus.FAULTED:
                    return advanced  # halt: a definitive defect or any Fault leaf dominates the fold
                case _:
                    return await _sequence(tuple(tail), settings, limiter, cpu_threshold, advanced)
        case _:
            return folded  # exhausted tail returns the accumulated join (seed EMPTY)


def _fire(action: Action, settings: AssaySettings, *, limiter: anyio.CapacityLimiter, cpu_threshold: float | None) -> Fire:
    # Build the per-fire closure. The limiter and ceiling are captured once in drive and shared across every
    # leaf so a whole Sequence fold stays single-flight — a fire cannot collide with its own next batch.
    async def fire() -> None:
        match action:
            case Sequence(actions=acts):
                _ = await _sequence(acts, settings, limiter, cpu_threshold)
            case Rail() | Program():
                _ = await _emit_leaf(action, settings, limiter, cpu_threshold)
            case Debounce(action=inner):
                _ = await _emit_leaf(inner, settings, limiter, cpu_threshold)  # degenerate path (no storm to coalesce): fire the wrapped inner once

    return fire


async def _quiesce(recv: MemoryObjectReceiveStream[None], window_ms: float) -> None:
    # Drain coalesced signals until window_ms elapses with no fresh signal; recursion is the loop-free re-arm vehicle.
    with anyio.move_on_after(window_ms / 1000.0) as scope:
        await recv.receive()
    match scope.cancelled_caught:
        case True:
            return  # the quiescence window elapsed with no fresh signal: the storm has settled
        case False:
            await _quiesce(recv, window_ms)  # a fresh signal re-armed the window: keep draining


def _debounce(inner: Fire, window_ms: int, *, collapse: bool) -> tuple[Fire, Fire]:
    # Coalesce a trigger storm into one inner fire per window_ms. Returns (signal, worker): the trigger loop
    # awaits signal per event (non-blocking notify onto a size-1 channel, the coalescing point); drive spawns
    # worker co-resident under the same tg/stop, recursing to await the next storm (stop teardown rides the cancel scope).
    send, recv = anyio.create_memory_object_stream[None](1)

    async def signal() -> None:  # noqa: RUF029  # async is load-bearing: the trigger loop awaits this as a Fire; send_nowait is the non-blocking coalescing notify
        match send.statistics().current_buffer_used:
            case 0:
                send.send_nowait(None)
            case _:
                return  # a signal is already pending: drop this one — this is the coalescing point

    async def worker() -> None:
        await recv.receive()
        match collapse:
            case False:
                await inner()  # leading edge: fire on the first signal of the storm (collapse=True suppresses the leading edge)
            case _:
                pass
        await _quiesce(recv, window_ms)
        match collapse:
            case True:
                await inner()  # trailing edge: fire once the storm settles (collapse=False already fired the leading edge)
            case _:
                pass
        await worker()  # await the next storm — recursion is the loop-free re-arm vehicle

    return signal, worker


async def _watch(spec: Watch, fire: Fire, stop: anyio.Event) -> None:
    # awatch honors the shared stop_event natively, so stop.set() collapses the loop without engine machinery.
    match spec.ignore_patterns:
        # Unknown filter tag degrades to DefaultFilter rather than raising.
        case ():
            watch_filter: BaseFilter = _FILTERS.get(spec.filter, DefaultFilter())
        # Only DefaultFilter exposes the ctor kwarg (BaseFilter is the value type), so the wire stays a string
        # tag + glob tuple and no watchfiles subclass leaks onto it.
        case patterns:
            watch_filter = DefaultFilter(ignore_entity_patterns=patterns)
    async for _changes in awatch(*spec.paths, watch_filter=watch_filter, debounce=spec.debounce, stop_event=stop):
        await fire()


def _hardened_fire(fire: Fire, settings: AssaySettings) -> Fire:
    # In-flight gate + coalesced missed-tick recovery + deterministic run_id jitter. A mid-fire cron tick is
    # COALESCED (counted in state.missed, dropped), NOT queued into an unbounded pile-up that a leased Action turns
    # into spurious BUSY storms. Single-task loop: no await between a read of state[0] and the write of its
    # successor, so the gate and missed-count are race-free without a lock. The leading jitter_s — a hashlib digest
    # over run_id, no RNG/wall-clock/knob — de-syncs a fleet sharing one schedule. The coalesced count rides the
    # structlog operational rail, never Envelope.notes (scheduling telemetry, not check evidence) — boundary is deliberate.
    state: list[_RunState] = [_RunState()]

    async def hardened() -> None:
        match state[0].running:
            case True:
                state[0] = msgspec.structs.replace(state[0], missed=state[0].missed + 1)  # coalesce one mid-fire tick
            case False:
                state[0] = msgspec.structs.replace(state[0], running=True)
                await anyio.sleep((int.from_bytes(hashlib.sha256(settings.run_id.encode()).digest()[:4], "big") % _JITTER_MS) / 1000.0)
                await fire()
                match state[0].missed:  # ONE catch-up fire if any ticks coalesced during the leaf (cron behind-schedule: run once, never N)
                    case missed if missed:
                        _LOG.warning("schedule.coalesced", missed=missed)
                        state[0] = msgspec.structs.replace(state[0], missed=0)
                        await fire()  # single catch-up; ticks during it fold into the next natural tick's drain
                    case _:
                        pass
                state[0] = msgspec.structs.replace(state[0], running=False)

    return hardened


async def _schedule(spec: str, fire: Fire, stop: anyio.Event) -> None:
    # aiocron exposes no native stop_event (unlike awatch): cron.start() is a SYNC method arming one loop.call_at
    # callback (never a coroutine), so a one-shot waiter on the shared stop parks the task — stop.set() returns the
    # waiter and cron.stop() cancels the callback, reaching the same graceful shutdown. start=False defers arming.
    cron = aiocron.crontab(spec, func=fire, start=False)
    cron.start()
    await stop.wait()
    cron.stop()


def _armed(action: Action, settings: AssaySettings, *, limiter: anyio.CapacityLimiter, ceiling: float | None) -> tuple[Fire, Fire | None]:
    # Project a looping trigger's Action to (trigger-facing fire, co-resident worker | None).
    match action:
        # Top-level Debounce installs the coalescer: trigger loop awaits the signal, drive spawns the worker.
        case Debounce(action=inner, window_ms=window, collapse=coalesce):
            return _debounce(_fire(inner, settings, limiter=limiter, cpu_threshold=ceiling), window, collapse=coalesce)
        # Every other Action returns its plain _fire closure with NO worker — the trigger loop awaits it directly.
        case Rail() | Program() | Sequence():
            return _fire(action, settings, limiter=limiter, cpu_threshold=ceiling), None


async def drive(trigger: Trigger, action: Action, settings: AssaySettings) -> None:
    """Host a trigger and run its action for each fire.

    Args:
        trigger: Trigger that decides when the action runs.
        action: Rail, program, sequence, or debounce action to execute.
        settings: Runtime settings shared by every fire.
    """
    limiter = anyio.CapacityLimiter(1)
    stop = anyio.Event()

    async def _co_resident(tg: TaskGroup, worker: Fire | None) -> None:
        match worker:
            case None:
                return
            case _:
                tg.start_soon(worker)
                await stop.wait()  # the worker blocks on its coalescing channel; cancel the scope so it collapses with the trigger
                tg.cancel_scope.cancel()

    match trigger:
        case Manual():
            await _fire(action, settings, limiter=limiter, cpu_threshold=None)()
        case Watch(cpu_threshold=ceiling) as spec:
            fire, worker = _armed(action, settings, limiter=limiter, ceiling=ceiling)
            async with anyio.create_task_group() as tg:
                tg.start_soon(_watch, spec, fire, stop)
                await _co_resident(tg, worker)
        case Schedule(cron=cron_spec, cpu_threshold=ceiling):
            fire, worker = _armed(action, settings, limiter=limiter, ceiling=ceiling)
            # harden the direct fire (in-flight gate + coalesced-tick recovery + jitter); a top-level Debounce
            # already coalesces via its worker, so its cron-facing signal stays a bare notify.
            scheduled = _hardened_fire(fire, settings) if worker is None else fire
            async with anyio.create_task_group() as tg:
                tg.start_soon(_schedule, cron_spec, scheduled, stop)
                await _co_resident(tg, worker)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Fire", "drive"]
