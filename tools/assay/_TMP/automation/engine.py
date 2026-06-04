"""The automation spine: one ``anyio`` loop hosts watch + schedule over a shared stop.

``drive`` is the single public surface: it discriminates the ``Trigger`` union and hosts the
``watchfiles.awatch`` loop and the ``aiocron.crontab`` tasklet concurrently under **one** ``anyio``
task group sharing **one** ``anyio.Event`` stop, running the bound ``Action`` on every fire through
the same ``composition/registry`` + ``core/engine`` rails the CLI uses and emitting **one**
``Envelope`` per fire as NDJSON. It consumes ``REGISTRY``/``rail`` and ``run_check``; it is not a
``Claim`` and never extends them.

Why the moving parts are shaped this way: ``awatch`` honors the shared ``stop_event`` natively, but
``aiocron`` has none, so ``_schedule`` waits on the same ``stop`` then ``cron.stop()`` + scope cancel
to mirror graceful shutdown across both arms. The per-drive ``CapacityLimiter(1)`` serializes fires
so a fire slower than its trigger cadence queues on the limiter rather than re-entering a leased
``Action`` into spurious ``BUSY`` (which ``@retried`` will not absorb). The CPU governor emits
``Completed{SKIP}`` and elides the fire, never waits — automation must not block on contention.
``@retried`` lives on ``run_check``, never inside a fire. No inline ``break``/``while``/``for``:
``awatch`` is the generator, ``cron.start`` is loop-resident, and the ``Sequence`` fold is a
recursive head/tail ``match``.
"""

from collections.abc import Callable, Coroutine
import sys
from typing import TYPE_CHECKING

import aiocron  # type: ignore[import-untyped]  # aiocron ships no py.typed marker
import anyio
from expression import Error, Ok, Result
import msgspec
import psutil  # type: ignore[import-untyped]  # psutil ships no py.typed marker
from watchfiles import awatch, DefaultFilter, PythonFilter

from tools.assay._TMP.automation.model import (  # noqa: PLC2701  # intra-staging import; _TMP is the package root
    Debounce,
    Manual,
    Program,
    Rail,
    Schedule,
    Sequence,
    Watch,
)
from tools.assay._TMP.composition.registry import rail, REGISTRY  # noqa: PLC2701  # intra-staging import; _TMP is the package root
from tools.assay._TMP.composition.settings import ArtifactScope  # noqa: PLC2701  # intra-staging import; _TMP is the package root
from tools.assay._TMP.core.engine import run_check  # noqa: PLC2701  # intra-staging import; _TMP is the package root
from tools.assay._TMP.core.model import (  # noqa: PLC2701  # intra-staging import; _TMP is the package root
    Check,
    Claim,
    Completed,
    envelope,
    Fault,
    fold,
    Input,
    Language,
    Mode,
    Runner,
    Tool,
)
from tools.assay._TMP.core.routing import Routed, Scope  # noqa: PLC2701  # intra-staging import; _TMP is the package root
from tools.assay._TMP.core.status import join, RailStatus  # noqa: PLC2701  # intra-staging import; _TMP is the package root


if TYPE_CHECKING:
    from anyio.abc import TaskGroup  # annotation-only (TC002): the _co_resident spawn surface; anyio.abc is not an attribute of the bare anyio import
    from anyio.streams.memory import MemoryObjectReceiveStream  # annotation-only (TC002): the _quiesce recv type from create_memory_object_stream
    from watchfiles import BaseFilter  # annotation-only (TC002): the _FILTERS value type, never constructed by name

    from tools.assay._TMP.automation.model import Action, Trigger  # intra-staging import; _TMP is the package root — annotation-only (TC001)
    from tools.assay._TMP.composition.settings import AssaySettings  # intra-staging import; _TMP is the package root — annotation-only (TC001)
    from tools.assay._TMP.core.model import Bind, Envelope, Report  # intra-staging import; _TMP is the package root — annotation-only (TC001)


# --- [TYPES] ----------------------------------------------------------------------------

# The per-fire closure returns nothing: each fire's sole observable is the NDJSON line it emits.
type Fire = Callable[[], Coroutine[None, None, None]]


# --- [CONSTANTS] ------------------------------------------------------------------------

# The wire carries a string filter tag and ``engine`` resolves the ``BaseFilter`` here, so the model
# never imports ``watchfiles``; a drifting tag degrades to ``DefaultFilter`` rather than raising.
_FILTERS: dict[str, BaseFilter] = {"default": DefaultFilter(), "python": PythonFilter()}

_PROGRAM_ROUTED: Routed = Routed(language=Language.PYTHON, scope=Scope.FULL)  # NONE-route seed: ``place`` emits one empty tail

_ENCODER = msgspec.json.Encoder(order="deterministic")  # the sole automation stdout codec, cached once


# --- [OPERATIONS] -----------------------------------------------------------------------


def _emit(line: Envelope) -> None:
    """The sole stdout writer for the automation arm: one NDJSON line per fire.

    The immediate flush is load-bearing: a long-lived ``Watch`` whose stdout is piped to a consumer
    must surface each ``Envelope`` at fire time, not batched at process exit, and per-line flushing is
    what keeps line-framing safe under the concurrent ``cron.start`` task and the per-leaf emits of a
    ``Sequence`` fold.
    """
    sys.stdout.buffer.write(_ENCODER.encode(line) + b"\n")
    sys.stdout.buffer.flush()


def _governed(threshold: float | None) -> bool:
    """The CPU governor gate: trip when measured CPU meets the per-trigger ceiling, else fire.

    ``None`` disables the gate. A set ceiling reads ``psutil.cpu_percent(0.1)`` — a 0.1s sample that
    blocks the fire task briefly but never the ``awatch``/``cron`` drivers — against ``threshold*100``
    so a fractional ceiling (``0.85``) gates at 85% measured utilization.
    """
    match threshold:
        case None:
            return False
        case _:
            return float(psutil.cpu_percent(0.1)) >= threshold * 100.0  # coerce the untyped sample to a typed float


def _resolve(action: Rail) -> Bind | None:
    """Resolve a ``Rail`` action to its ``REGISTRY`` row by ``(claim, verb)`` — ``None`` on an unbound verb.

    The registry is the catalog-row authority: a ``Rail`` carries only the ``(claim, verb)``
    discriminant and an opaque ``msgspec.Raw`` payload. An unresolved verb yields ``None`` so the caller
    folds it to a ``FAULTED`` ``Fault`` at the fire boundary, never a raised ``KeyError``.
    """
    return next((b for b in REGISTRY if b.claim is action.claim and b.verb == action.verb), None)


def _label(action: Action) -> tuple[Claim, str]:
    """Project an ``Action`` to its ``(claim, verb)`` envelope label.

    Only a ``Rail`` carries an explicit claim/verb pair; a ``Program`` and a ``Sequence`` have no single
    rail identity, so both fold under ``(Claim.STATIC, "program")`` — the canonical DIRECT label every
    non-rail fire shares. A ``Debounce`` is a pure coalescing wrapper that adds no rail identity of its
    own, so it projects the label of the ``Action`` it wraps (recursing through nested wrappers).
    """
    match action:
        case Rail(claim=c, verb=v):
            return c, v
        case Program() | Sequence():
            return Claim.STATIC, "program"
        case Debounce(action=inner):
            return _label(inner)


def _program_check(argv: tuple[str, ...]) -> Check:
    """Bind a raw ``argv`` to a DIRECT ``Check``: ``Runner.DIRECT`` empty prefix + ``Input.NONE`` route.

    ``Runner.DIRECT`` lays the prefix as ``()`` so ``run_check`` runs ``argv`` verbatim; ``Input.NONE``
    makes ``routing.place`` emit one empty argv tail, so no path projection bleeds into a free-form
    program. ``Language``/``claim``/``mode`` are inert for a ``NONE`` route — the suffix sets are never
    read — so every ``Program`` folds under ``Claim.STATIC`` with a ``RUN`` mode for live streaming.
    """
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


def _program_outcome(action: Program, settings: AssaySettings) -> Result[Report, Fault]:
    """Run a ``Program`` fire through ``run_check`` and fold its ``Completed`` into a ``Report``.

    The ``checked ▷ traced ▷ retried`` seam is owned by ``run_check``, not here. A non-zero process exit
    rides the ``Ok`` channel as a ``Completed{FAILED}``; only a spawn failure / timeout takes the
    ``Error`` channel. The ``Completed`` folds through ``core/model.fold`` (the sole count-derivation
    site) so the engine never authors a ``Fault`` for an exit code.
    """
    scope = ArtifactScope.open(settings, Claim.STATIC)
    outcome = run_check(_program_check(action.argv), settings=settings, scope=scope, routed=_PROGRAM_ROUTED, deadline=None)
    match outcome:
        case Result(tag="ok", ok=done):
            return Ok(fold(Claim.STATIC, "program", (done,)))
        case Result(error=fault):
            return Error(fault)


def _emitted(line: Envelope) -> Envelope:
    """Write one ``Envelope`` via the sole automation writer and return it for the fold (side-effecting seam)."""
    _emit(line)
    return line


def _rail_outcome(action: Rail) -> Envelope:
    """Run a ``Rail`` fire through the registry runner — the one path that reuses the CLI's emitter.

    The opaque ``msgspec.Raw`` ``params`` decode is deferred to here (zero-copy, against the bound frozen
    ``Params`` type). ``rail(bind)`` weaves ``checked ▷ logged ▷ traced`` and writes its OWN ``Envelope``
    (the registry is the sole stdout writer for a rail), so the engine must NOT re-emit a rail outcome; it
    returns the runner's ``Envelope`` for the fold. An unresolved verb or a malformed ``params`` payload
    folds to a ``FAULTED`` ``Fault`` envelope here, written through the engine's own ``_emit``.
    """
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


def _program_envelope(action: Program, settings: AssaySettings) -> Envelope:
    """Emit one ``Envelope`` for a ``Program`` fire: project the ``run_check`` outcome onto a wire line.

    ``_program_outcome`` already folded a ``Completed`` (non-zero exit on the ``Ok`` channel) or a spawn
    ``Fault``; this projects either ``Result`` channel onto the canonical ``envelope`` and writes it
    through the engine's ``_emit`` (a ``Program`` has no registry runner to write for it).
    """
    match _program_outcome(action, settings):
        case Result(tag="ok", ok=report):
            payload: Report | Fault = report
        case Result(error=fault):
            payload = fault
    return _emitted(envelope(payload, claim=Claim.STATIC, verb="program"))


async def _emit_leaf(leaf: Action, settings: AssaySettings, limiter: anyio.CapacityLimiter, cpu_threshold: float | None) -> RailStatus:
    """Run one leaf ``Action`` to exactly one ``Envelope`` NDJSON line, returning its status for the fold.

    The governor short-circuits first: a tripped CPU ceiling emits ``Completed{SKIP}`` and never waits.
    Otherwise the work runs under the per-drive ``CapacityLimiter(1)`` so a fire slower than its trigger
    cadence queues on the single token rather than re-entering a leased ``Action`` into spurious ``BUSY``.
    A ``Rail`` reuses the registry runner (which emits its own line); a ``Program`` and a nested
    ``Sequence`` emit through the engine.
    """
    match _governed(cpu_threshold):
        case True:
            claim, verb = _label(leaf)
            skipped = Completed(argv=(), returncode=0, status=RailStatus.SKIP, notes=(f"governed: cpu>={cpu_threshold or 0.0:.0%}",))
            return _emitted(envelope(fold(claim, verb, (skipped,)), claim=claim, verb=verb)).status
        case False:
            async with limiter:
                match leaf:
                    case Rail() as r:
                        return _rail_outcome(r).status
                    case Program() as p:
                        return _program_envelope(p, settings).status
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
    """Fold a ``Sequence``'s leaves by ``RailStatus.join`` max-severity, halting on policy.

    A recursive head/tail ``match`` is the fold vehicle (no loop control). A definitive defect
    (``FAILED``) or any ``Fault`` leaf (``BUSY``/``TIMEOUT``/``FAULTED``) dominates and halts the fold;
    ``SKIP``/``EMPTY``/``OK`` continue to the tail. ``join`` is the module-scope semilattice operator
    rather than a method because ``RailStatus`` subclasses ``str``, whose own ``join`` member is
    ``str.join`` — the algebra is lifted off the enum to avoid that collision.
    """
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
    """Build the per-fire closure: one ``Action`` → one ``Envelope`` per leaf, single-flight under the limiter.

    The closure is generator-free, so it carries no ``@effect.result`` decoration. The limiter and
    governor ceiling are captured once in ``drive`` and shared across every leaf so a whole ``Sequence``
    fold stays single-flight — a fire slower than its trigger cadence cannot collide with its own next
    batch. The closure swallows its return value because the fire's sole observable is the NDJSON it emits.
    """

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
    """Drain coalesced trigger signals until ``window_ms`` elapses with no fresh signal (loop-free re-arm).

    Each ``move_on_after(window)`` wraps one ``recv.receive()``: a fresh signal lands before the window and
    re-arms by recursing; an elapsed window (``cancelled_caught``) returns, signalling quiescence to the
    worker. The recursion is the re-arm vehicle — no inline ``while`` — mirroring the ``_sequence`` fold.
    """
    with anyio.move_on_after(window_ms / 1000.0) as scope:
        await recv.receive()
    match scope.cancelled_caught:
        case True:
            return  # the quiescence window elapsed with no fresh signal: the storm has settled
        case False:
            await _quiesce(recv, window_ms)  # a fresh signal re-armed the window: keep draining


def _debounce(inner: Fire, window_ms: int, *, collapse: bool) -> tuple[Fire, Fire]:
    """Coalesce a trigger storm into one ``inner`` fire per quiescence ``window_ms`` (``move_on_after`` timer).

    Returns ``(signal, worker)``: the trigger loop awaits ``signal`` per event (a non-blocking notify onto a
    size-1 channel — a second pending signal collapses to a no-op, the coalescing point), and ``drive`` spawns
    ``worker`` co-resident under the same ``tg``/``stop``. ``worker`` awaits the first signal, fires the leading
    edge when ``collapse`` is ``False`` (leading mode suppresses only the trailing tail), then ``_quiesce``
    settles the storm and the trailing edge fires when ``collapse`` is ``True`` (storm → one trailing run). The
    worker recurses to await the next storm — loop-free; ``stop`` teardown rides the shared ``tg`` cancel scope.
    """
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
                await inner()  # leading edge: fire on the first signal of the storm
            case True:
                pass
        await _quiesce(recv, window_ms)
        match collapse:
            case True:
                await inner()  # trailing edge: fire once the storm settles
            case False:
                pass
        await worker()  # await the next storm — recursion is the loop-free re-arm vehicle

    return signal, worker


async def _watch(spec: Watch, fire: Fire, stop: anyio.Event) -> None:
    """Host the ``watchfiles.awatch`` loop over the shared stop: each debounced batch fires once.

    ``awatch`` is an ``async for`` generator (no inline ``while``/``break``): it batches filesystem
    mutations at the Rust ``notify`` layer within ``spec.debounce`` ms *before* yield and honors the
    shared ``stop_event`` natively, so ``stop.set()`` collapses the loop without engine machinery. The
    ``filter`` tag resolves to a concrete ``BaseFilter`` here; an unknown tag degrades to
    ``DefaultFilter`` rather than raising. ``spec.ignore_patterns`` (entity-name regexes — vendor dirs,
    build artifacts) refine that base: a populated tuple constructs a ``DefaultFilter(ignore_entity_patterns=…)``
    overriding the vocabulary tag (only ``DefaultFilter`` exposes the ctor kwarg; ``BaseFilter`` is the value
    type), so the wire stays a string tag + glob tuple and no ``watchfiles`` subclass leaks onto it.
    """
    match spec.ignore_patterns:
        case ():
            watch_filter: BaseFilter = _FILTERS.get(spec.filter, DefaultFilter())
        case patterns:
            watch_filter = DefaultFilter(ignore_entity_patterns=patterns)
    async for _changes in awatch(*spec.paths, watch_filter=watch_filter, debounce=spec.debounce, stop_event=stop):
        await fire()


async def _schedule(spec: str, fire: Fire, stop: anyio.Event) -> None:
    """Host the ``aiocron.crontab`` tasklet over the shared stop: each cron tick fires once.

    aiocron exposes no native ``stop_event`` (unlike ``awatch``), so the loop-resident ``cron.start`` is
    launched alongside a one-shot waiter on the shared ``stop``: when ``stop.set()`` arrives the waiter
    returns, ``cron.stop()`` cancels the scheduled callback, and the inner group's ``cancel_scope``
    collapses ``cron.start`` — this is how the ``Schedule`` arm reaches the same graceful shutdown the
    ``Watch`` arm gets natively. ``crontab(start=False)`` defers launch to ``tg.start_soon(cron.start)``.
    """
    cron = aiocron.crontab(spec, func=fire, start=False)
    async with anyio.create_task_group() as tg:
        tg.start_soon(cron.start)
        await stop.wait()
        cron.stop()
        tg.cancel_scope.cancel()


def _armed(action: Action, settings: AssaySettings, *, limiter: anyio.CapacityLimiter, ceiling: float | None) -> tuple[Fire, Fire | None]:
    """Project a looping trigger's ``Action`` to its ``(trigger-facing fire, co-resident worker | None)`` pair.

    A top-level ``Debounce`` installs the coalescer: the trigger loop awaits the ``signal`` ``Fire`` (a
    non-blocking notify) and ``drive`` spawns the ``worker`` under the same ``tg``/``stop`` to collapse a
    storm into one ``inner`` fire per ``window_ms`` via ``move_on_after``. Every other ``Action`` returns its
    plain ``_fire`` closure with **no** worker — the trigger loop awaits it directly, unchanged.
    """
    match action:
        case Debounce(action=inner, window_ms=window, collapse=coalesce):
            return _debounce(_fire(inner, settings, limiter=limiter, cpu_threshold=ceiling), window, collapse=coalesce)
        case Rail() | Program() | Sequence():
            return _fire(action, settings, limiter=limiter, cpu_threshold=ceiling), None


async def drive(trigger: Trigger, action: Action, settings: AssaySettings) -> None:
    """The single public surface: host one ``Trigger`` over a shared stop, firing one ``Action`` per event.

    Discriminates the ``Trigger`` union: ``Manual`` bypasses both loops and awaits ``fire()`` once;
    ``Watch`` hosts ``awatch`` and ``Schedule`` hosts the ``aiocron.crontab`` tasklet. The ``stop`` and
    the per-drive ``CapacityLimiter(1)`` are constructed once *before* the match so a single
    ``stop.set()`` collapses either trigger and every leaf of a ``Sequence`` shares one re-entrancy token.
    A ``Watch``/``Schedule`` reads its own ``cpu_threshold`` governor ceiling off the trigger spec (never a
    settings knob); ``Manual`` is ungoverned. A top-level ``Debounce`` action is armed via ``_armed``: the
    trigger loop awaits the coalescing ``signal`` while the co-resident ``worker`` runs under the **same**
    ``tg`` against the **same** ``stop``, and ``_co_resident`` cancels the scope on ``stop`` so the worker
    collapses with the trigger (``Manual`` keeps the degenerate single-fire ``Debounce`` path via ``_fire``).
    A future combined ``Watch + Schedule`` spawns both ``_watch`` and ``_schedule`` under the **same** ``tg``
    against the **same** ``stop``. ``__main__`` enters via ``anyio.run(drive, …)`` — the single ``anyio.run``.
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
            async with anyio.create_task_group() as tg:
                tg.start_soon(_schedule, cron_spec, fire, stop)
                await _co_resident(tg, worker)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Fire", "drive"]
