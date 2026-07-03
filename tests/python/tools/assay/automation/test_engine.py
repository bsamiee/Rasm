"""Laws for ``tools.assay.automation.engine``.

Covers ContextVar CPU governance and trigger/action projection through canned executor-port, rail,
psutil, watchfiles, and emit boundaries. ``drive(..., executor=probe.port(...))`` is the public spawn
seam; only the registry ``rail`` resolution law still pins the module-bound symbol.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

import contextvars
from datetime import datetime, UTC
from functools import partial
from types import SimpleNamespace
from typing import TYPE_CHECKING

import anyio
import anyio.lowlevel
from expression import Result  # runtime: msgspec resolves row-struct field annotations at class creation
import msgspec
import pytest

from tests.python.tools.assay.kit import RailProbe
from tools.assay.automation import engine as _eng
from tools.assay.automation.engine import drive, is_governed
from tools.assay.automation.model import Action, Debounce, Edge, Manual, Program, Rail, Schedule, Sequence, Trigger, Watch, WatchFilter
from tools.assay.core.model import Claim, Completed, Counts, envelope, Fault, RailStatus, receipt
from tools.assay.diagnostics import fold


if TYPE_CHECKING:
    from collections.abc import AsyncIterator

    from tests.python.tools.assay.kit import AssayHarness, CpuDoubleInstaller, CpuSampler
    from tools.assay.core.model import Envelope


# --- [CONSTANTS] ------------------------------------------------------------------------

COVERS: tuple[object, ...] = (drive, is_governed)

_FIRST: tuple[tuple[str, str], ...] = (("added", "first.txt"),)
_SECOND: tuple[tuple[str, str], ...] = (("modified", "second.txt"),)

# --- [TABLES] ---------------------------------------------------------------------------


class _GovernorCase(msgspec.Struct, frozen=True, gc=False):
    """Governor predicate row; ``intervals`` of None skips the sampler-interval assertion."""

    label: str
    threshold: float | None
    cpu: float
    governed: bool
    primed: bool = True
    intervals: tuple[float | None, ...] | None = None


class _ProgramCase(msgspec.Struct, frozen=True, gc=False):
    """Program outcome row; ``counts`` of None selects the error-arm assertions."""

    label: str
    argv: tuple[str, ...]
    canned: Result[Completed, Fault]
    status: RailStatus
    counts: Counts | None = None
    message: str | None = None


class _LeafCase(msgspec.Struct, frozen=True, gc=False):
    """Sequence/Debounce tree row: expected emit count, per-emit status, fired command count."""

    label: str
    action: Action
    emits: int
    status: RailStatus
    fired: int


class _FaultCase(msgspec.Struct, frozen=True, gc=False):
    """Setup/guard/resolve/decode fault row with expected Envelope label and message substrings."""

    label: str
    trigger: Trigger
    action: Action
    claim: Claim
    verb: str
    substrings: tuple[str, ...] = ()


_GOVERNOR_CASES: tuple[_GovernorCase, ...] = (
    _GovernorCase("t0.9-skip", 0.9, 91.0, governed=True),
    _GovernorCase("t0.95-run", 0.95, 91.0, governed=False),
    _GovernorCase("tNone-run", None, 91.0, governed=False),
    _GovernorCase("t0.0-run", 0.0, 0.0, governed=False),
    _GovernorCase("t1.0-100-skip", 1.0, 100.0, governed=True),
    _GovernorCase("t1.0-99.9-run", 1.0, 99.9, governed=False),
    _GovernorCase("t0.91-skip", 0.91, 91.0, governed=True),
    # Disabled governor never touches psutil even when unprimed.
    _GovernorCase("disabled-never-samples", None, 0.0, governed=False, primed=False, intervals=()),
    # A primed latch reads once, non-blocking, with no warmup.
    _GovernorCase("primed-reads-nonblocking", 0.5, 80.0, governed=True, intervals=(None,)),
)

_OK_ROW = RailProbe.receipt(("dotnet", "build"), 0, status=RailStatus.OK, stdout=b"Build succeeded.\n")
_PROGRAM_CASES: tuple[_ProgramCase, ...] = (
    _ProgramCase("ok-report", ("dotnet", "build"), _OK_ROW, RailStatus.OK, counts=Counts(1, 0, 1)),
    _ProgramCase("fault-arm", ("missing-tool",), RailProbe.error(("missing-tool",), "spawn: no tool"), RailStatus.FAULTED, message="spawn: no tool"),
    _ProgramCase("rc1-failed", ("tool",), RailProbe.receipt(("tool",), 1, status=RailStatus.FAILED), RailStatus.FAILED, counts=Counts(0, 1, 1)),
    _ProgramCase("rc0-empty", ("tool",), RailProbe.receipt(("tool",), 0, status=RailStatus.EMPTY), RailStatus.EMPTY, counts=Counts(1, 0, 1)),
    _ProgramCase("rc124-timeout", ("t",), RailProbe.receipt(("t",), 124, status=RailStatus.TIMEOUT), RailStatus.TIMEOUT, counts=Counts(0, 0, 0)),
)

_NESTED = Sequence(actions=(Program(argv=("p", "out")), Sequence(actions=(Program(argv=("p", "in")),)), Debounce(action=Program(argv=("p", "w")))))
_LEAF_CASES: tuple[_LeafCase, ...] = (
    _LeafCase("sequence-two-ok", Sequence(actions=(Program(argv=("p", "1")), Program(argv=("p", "2")))), 2, RailStatus.OK, 2),
    _LeafCase("nested-seq-debounce", _NESTED, 3, RailStatus.OK, 3),
    _LeafCase("manual-debounce-unwrap", Debounce(action=Program(argv=("tool",)), window_ms=500, edge=Edge.TRAILING), 1, RailStatus.OK, 1),
    # A FAULTED leaf (empty argv) short-circuits _sequence before the trailing leaf runs.
    _LeafCase("halt-on-fault", Sequence(actions=(Program(argv=()), Program(argv=("p", "2")))), 1, RailStatus.FAULTED, 0),
)

_BAD_ZONE = Schedule(cron="* * * * *", timezone="Invalid/Zone")
_FAULT_CASES: tuple[_FaultCase, ...] = (
    # Setup faults stay envelope-local; empty argv faults before spawn; unbound/undecodable rails fault at the leaf;
    # Debounce wrappers label setup faults by the inner action.
    _FaultCase("setup-error", _BAD_ZONE, Rail(claim=Claim.STATIC, verb="static"), Claim.STATIC, "static", ("automation setup",)),
    _FaultCase("empty-argv", Manual(), Program(argv=()), Claim.STATIC, "program", ("non-empty",)),
    _FaultCase("rail-unbound", Manual(), Rail(claim=Claim.STATIC, verb="nope"), Claim.STATIC, "nope", ("unbound rail", "static:nope")),
    _FaultCase("rail-param-decode", Manual(), Rail(claim=Claim.STATIC, verb="static", params=msgspec.Raw(b"{not json")), Claim.STATIC, "static"),
    _FaultCase("debounce-setup-inner-label", _BAD_ZONE, Debounce(action=Rail(claim=Claim.CODE, verb="search")), Claim.CODE, "search"),
)

# (label, canned awatch batches, expected fire count) — empty timeout heartbeats never fire.
_WATCH_BATCH_CASES: tuple[tuple[str, tuple[tuple[tuple[str, str], ...], ...], int], ...] = (
    ("fires-per-batch", (_FIRST, _SECOND), 2),
    ("skips-empty-heartbeat", ((), _FIRST, ()), 1),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _one(seen: list[Envelope]) -> Envelope:
    """Return the only emitted Envelope after asserting singleton emission."""
    assert len(seen) == 1, f"expected exactly one Envelope, got {len(seen)}: {[e.status for e in seen]!r}"
    return seen[0]


def _recording_sampler(value: float) -> tuple[CpuSampler, list[float | None]]:
    """Return a psutil sampler double plus interval log for warmup/latch laws."""
    log: list[float | None] = []

    def _sample(interval: float | None = None) -> float:
        log.append(interval)
        return value

    return _sample, log


def _fake_awatch(batches: tuple[tuple[tuple[str, str], ...], ...]) -> object:
    """Return an ``awatch`` double that yields wire-shaped batches then completes."""

    async def _awatch(*_paths: str, **_kw: object) -> AsyncIterator[set[tuple[str, str]]]:  # noqa: RUF029  # async def required; no await in body
        for batch in batches:
            yield {(kind, path) for kind, path in batch}

    return _awatch


def _cron_tick(stop: anyio.Event | None = None) -> SimpleNamespace:
    previous: list[datetime | None] = []

    def _next(previous_fire_time: datetime | None, _now: datetime) -> datetime:
        previous.append(previous_fire_time)
        if stop is not None and len(previous) >= 2:
            stop.set()
        return datetime.now(UTC)

    return SimpleNamespace(get_next_fire_time=_next)


# --- [LAWS_IS_GOVERNED]


@pytest.mark.parametrize("row", _GOVERNOR_CASES, ids=[c.label for c in _GOVERNOR_CASES])
def test_is_governed_matrix(row: _GovernorCase, cpu_double: CpuDoubleInstaller) -> None:
    """``is_governed`` trips iff ``cpu_percent >= threshold * 100`` on a positive ceiling; ``None``/``0.0`` disable without sampling.

    Falsified by: removing the ``>=`` boundary, gating instead of disabling on ``0.0``, sampling in the
    disabled arm, or re-priming a primed latch with the blocking warmup.
    """
    sampler, log = _recording_sampler(row.cpu)
    cpu_double(sampler)
    _eng._CPU_PRIMED.set(row.primed)
    assert is_governed(row.threshold) is row.governed
    assert row.intervals is None or tuple(log) == row.intervals, f"sampler intervals {log} != {row.intervals}"


def test_is_governed_latch_context_isolated(cpu_double: CpuDoubleInstaller) -> None:
    """The warmup ``cpu_percent(0.1)`` fires exactly once per fresh ``contextvars.Context``; the latch never leaks across runs.

    Falsified by: removing ``_CPU_PRIMED.set(True)``, replacing the ContextVar with module state, or
    a stale latch surviving into a fresh context.
    """
    sampler, intervals = _recording_sampler(50.0)
    cpu_double(sampler)

    async def _run() -> None:
        await anyio.lowlevel.checkpoint()
        assert _eng._CPU_PRIMED.get() is False, "ContextVar must start at its False default in a fresh context"
        is_governed(0.9)
        assert _eng._CPU_PRIMED.get() is True, "latch must be True after first governed call"
        is_governed(0.9)

    contextvars.Context().run(anyio.run, _run)
    contextvars.Context().run(anyio.run, _run)

    assert [c for c in intervals if c is not None] == [0.1, 0.1], f"expected one warmup per fresh context, got {intervals}"
    assert [c for c in intervals if c is None] == [None] * 4, f"expected two non-blocking reads per context, got {intervals}"


# --- [LAWS_DRIVE_PROGRAM]


@pytest.mark.parametrize("row", _PROGRAM_CASES, ids=[c.label for c in _PROGRAM_CASES])
def test_drive_program_outcome_matrix(row: _ProgramCase, assay_root: AssayHarness, captured_emits: list[Envelope], rail_probe: RailProbe) -> None:
    """Program leaves project outcomes through ``fold``: rc rides the Completed channel, spawn/timeout Faults ride the error arm.

    Falsified by: ``_program_outcome`` promoting a nonzero exit to Fault, swallowing a Fault into an OK
    Report, mis-routing the argv, or ``fold`` miscounting a receipt status.
    """
    anyio.run(partial(drive, Manual(), Program(argv=row.argv), assay_root.settings, executor=rail_probe.port(row.canned)))

    assert rail_probe.commands == [row.argv], f"engine must route the program argv verbatim; got {rail_probe.commands}"
    env = _one(captured_emits)
    assert (env.status, env.claim, env.verb) == (row.status, Claim.STATIC, "program")
    match row.counts:
        case None:
            assert env.error is not None
            assert row.message is not None
            assert row.message in env.error.message
        case counts:
            assert env.report is not None
            assert env.report.counts == counts


# --- [LAWS_DRIVE_RAIL]


def test_drive_rail_resolves_bind_and_emits_canned_envelope(
    assay_root: AssayHarness, captured_emits: list[Envelope], rail_probe: RailProbe, monkeypatch: pytest.MonkeyPatch
) -> None:
    """A bound Rail resolves, decodes params, and does not double-emit an already-written Envelope.

    Falsified by: re-emitting the rail Envelope or failing to resolve the registered claim/verb pair.
    """
    rail_env = envelope(fold(Claim.STATIC, "static", (receipt(("static",), 0, status=RailStatus.OK),)), claim=Claim.STATIC, verb="static")
    rail_probe.install(monkeypatch, _eng, "rail", rail_env)

    anyio.run(drive, Manual(), Rail(claim=Claim.STATIC, verb="static"), assay_root.settings)

    assert [c for c in rail_probe.calls if c[0] == "rail.run"], "the registry runner must be invoked with decoded params"
    assert captured_emits == [], f"already-emitted rail Envelope must not be re-emitted; got {len(captured_emits)}"


# --- [LAWS_DRIVE_GOVERNOR]


def test_drive_governed_skip_emits_one_skip_envelope(
    assay_root: AssayHarness, captured_emits: list[Envelope], rail_probe: RailProbe, cpu_double: CpuDoubleInstaller, monkeypatch: pytest.MonkeyPatch
) -> None:
    """A tripped CPU governor emits one SKIP Envelope and never runs the leaf.

    Falsified by: ``_emit_leaf`` running the leaf despite the governor, mis-counting the governed leaf, or
    dropping the ``governed:``-prefixed ``cpu>=`` note.
    """
    _eng._CPU_PRIMED.set(True)
    cpu_double(lambda *_a, **_k: 100.0)
    monkeypatch.setattr(_eng, "awatch", _fake_awatch((_FIRST,)))

    spec = Watch(paths=(str(assay_root.root),), cpu_threshold=0.5)
    anyio.run(partial(drive, spec, Program(argv=("tool",)), assay_root.settings, executor=rail_probe.port(rail_probe.ok(("tool",)))))

    assert rail_probe.commands == [], "the governor must skip the fire before the leaf runs"
    env = _one(captured_emits)
    assert env.status is RailStatus.SKIP
    assert env.report is not None
    assert env.report.counts == Counts(1, 0, 1)
    assert any(note.startswith("governed:") and "cpu>=" in note for note in env.report.notes)


# --- [LAWS_DRIVE_LEAVES]


@pytest.mark.parametrize("row", _LEAF_CASES, ids=[c.label for c in _LEAF_CASES])
def test_drive_leaf_matrix(row: _LeafCase, assay_root: AssayHarness, captured_emits: list[Envelope], rail_probe: RailProbe) -> None:
    """Sequence/Debounce trees recurse through ``_emit_leaf`` exactly once per leaf and halt on the first terminal status.

    Falsified by: ``_sequence`` continuing past FAULTED, ``_emit_leaf`` skipping a nested Sequence or
    Debounce unwrap, or a leaf firing twice.
    """
    anyio.run(partial(drive, Manual(), row.action, assay_root.settings, executor=rail_probe.port(rail_probe.ok(("p",)))))

    assert len(captured_emits) == row.emits, f"expected {row.emits} emits, got {len(captured_emits)}"
    assert all(env.status is row.status for env in captured_emits), f"statuses {[e.status for e in captured_emits]} != {row.status}"
    assert all(env.verb == "program" for env in captured_emits)
    assert len(rail_probe.commands) == row.fired, f"expected {row.fired} fired leaves, got {rail_probe.commands}"


def test_hardened_fire_coalesces_reentrant_tick(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """A re-entrant hardened-fire tick coalesces instead of starting a concurrent fire.

    Falsified by: removing the ``running`` guard and starting a second concurrent fire. The nested call exercises
    re-entry while the cell is still running.
    """
    monkeypatch.setattr(_eng, "_JITTER_MS", 1)
    started: list[int] = []
    completed: list[int] = []

    async def _run() -> None:
        async def _fire(_changes: tuple[tuple[str, str], ...]) -> None:
            started.append(1)
            await anyio.lowlevel.checkpoint()
            if len(started) == 1:
                await hardened(())
            completed.append(1)

        hardened = _eng._hardened_fire(_fire, assay_root.settings)
        await hardened(())

    anyio.run(_run)

    assert started == [1], f"the re-entrant tick must not start a second fire; got {started}"
    assert completed == [1]


def test_hardened_fire_faults_reset_after_exception(
    assay_root: AssayHarness, captured_emits: list[Envelope], monkeypatch: pytest.MonkeyPatch
) -> None:
    """A raising fire emits one FAULTED Envelope and resets the cell for the next tick.

    Falsified by: removing the ``except``/``finally`` — the exception escapes the task group or wedges
    ``running=True`` so later ticks never fire.
    """
    monkeypatch.setattr(_eng, "_JITTER_MS", 1)
    calls: list[int] = []

    async def _fire(_changes: tuple[tuple[str, str], ...]) -> None:
        await anyio.lowlevel.checkpoint()
        calls.append(1)
        if len(calls) == 1:
            msg = "boom"
            raise RuntimeError(msg)

    async def _run() -> None:
        hardened = _eng._hardened_fire(_fire, assay_root.settings)
        await hardened(())
        await hardened(())

    anyio.run(_run)

    assert calls == [1, 1], "the cell must reset and fire again after a fault"
    env = _one(captured_emits)
    assert (env.status, env.claim, env.verb) == (RailStatus.FAULTED, Claim.STATIC, "automation")
    assert env.error is not None
    assert "RuntimeError: boom" in env.error.message


# --- [LAWS_DRIVE_DEBOUNCE]


@pytest.mark.parametrize("edge", [Edge.LEADING, Edge.TRAILING], ids=["leading", "trailing"])
@pytest.mark.anyio
async def test_debounce_fires_once_per_storm(*, edge: Edge) -> None:
    """Leading and trailing debounce modes fire once per storm window.

    Falsified by: firing per-signal, or the worker leaking the channel under ``filterwarnings=error`` (an
    unclosed-stream ResourceWarning fails). The size-1 channel coalesces the second signal until quiescence.
    """
    fired: list[tuple[tuple[str, str], ...]] = []

    async def _inner(changes: tuple[tuple[str, str], ...]) -> None:  # noqa: RUF029  # Fire protocol is async; no await needed here
        fired.append(changes)

    signal, worker = _eng._debounce(_inner, 40, edge=edge)
    async with anyio.create_task_group() as tg:
        _ = tg.start_soon(worker)
        await signal(_FIRST)
        await anyio.lowlevel.checkpoint()
        await signal(_SECOND)
        await anyio.sleep(0.07)
        tg.cancel_scope.cancel()

    assert len(fired) == 1, f"each storm window must produce exactly one fire; got {fired}"


def test_debounce_signal_ignores_when_buffer_full() -> None:
    """Signals drop silently while the debounce worker's size-1 buffer is full.

    Falsified by: the signal raising on a full channel (no ``send_nowait`` guard), or growing past size 1
    (a second signal reaches ``inner``). Pinning the first fire keeps the worker from draining the channel.
    """
    fired: list[tuple[tuple[str, str], ...]] = []
    pinned = anyio.Event()

    async def _inner(changes: tuple[tuple[str, str], ...]) -> None:
        fired.append(changes)
        await pinned.wait()

    async def _run() -> None:
        signal, worker = _eng._debounce(_inner, 40, edge=Edge.LEADING)
        async with anyio.create_task_group() as tg:
            _ = tg.start_soon(worker)
            await signal(_FIRST)
            await anyio.sleep(0.02)
            await signal(_SECOND)
            await signal(_FIRST)
            await anyio.sleep(0.02)
            tg.cancel_scope.cancel()

    anyio.run(_run)

    assert fired == [_FIRST], f"buffer-full signals must drop, not fire; got {fired}"


def test_debounce_signal_after_close_is_silent() -> None:
    """Post-close debounce signals return silently through the ClosedResourceError arm.

    Falsified by: removing the closed-resource arm and raising on a late shutdown tick. Cancelling the worker
    closes both streams before the final signal.
    """

    async def _inner(_changes: tuple[tuple[str, str], ...]) -> None:  # noqa: RUF029  # Fire protocol is async; no await needed here
        return None

    async def _run() -> None:
        signal, worker = _eng._debounce(_inner, 40, edge=Edge.TRAILING)
        async with anyio.create_task_group() as tg:
            _ = tg.start_soon(worker)
            await anyio.lowlevel.checkpoint()
            tg.cancel_scope.cancel()
        await signal(_FIRST)

    anyio.run(_run)


# --- [LAWS_DRIVE_WATCH]


@pytest.mark.parametrize("label,batches,fires", _WATCH_BATCH_CASES, ids=[c[0] for c in _WATCH_BATCH_CASES])
def test_drive_watch_batch_matrix(
    label: str,
    batches: tuple[tuple[tuple[str, str], ...], ...],
    fires: int,
    assay_root: AssayHarness,
    captured_emits: list[Envelope],
    rail_probe: RailProbe,
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    """Watch drive fires once per non-empty ``awatch`` batch; empty timeout heartbeats never fire.

    Falsified by: ``_watch`` dropping a real batch, firing on a heartbeat, or ``_drive`` deadlocking with no
    debounce worker.
    """
    _ = label
    monkeypatch.setattr(_eng, "awatch", _fake_awatch(batches))

    spec = Watch(paths=(str(assay_root.root),))
    anyio.run(partial(drive, spec, Program(argv=("tool",)), assay_root.settings, executor=rail_probe.port(rail_probe.ok(("tool",)))))

    assert len(captured_emits) == fires, f"expected {fires} fires for {batches!r}; got {len(captured_emits)}"
    assert all(env.status is RailStatus.OK for env in captured_emits)


@pytest.mark.parametrize(
    "filter_tag,expected_type",
    [(WatchFilter.DEFAULT, "DefaultFilter"), (WatchFilter.PYTHON, "PythonFilter")],
    ids=["default-filter", "python-filter"],
)
def test_watch_filter_resolves_tag_to_watchfiles_filter(filter_tag: WatchFilter, expected_type: str) -> None:
    """Watch filter tags project to the matching watchfiles filter instance.

    Falsified by: swapping the DEFAULT/PYTHON arms or constructing the wrong filter class.
    """
    # Valid regex keeps the law about tag dispatch, not pattern compilation.
    spec = Watch(paths=("x",), filter=filter_tag, ignore_patterns=(r"\.pyc$",))
    assert type(_eng._watch_filter(spec)).__name__ == expected_type


def test_watch_filter_default_extends_builtin_noise_suppression() -> None:
    """DEFAULT extends watchfiles noise suppression instead of replacing it.

    Falsified by: replacing ``ignore_entity_patterns`` and surfacing built-in ``.pyc``/``.DS_Store`` churn.
    """
    flt = _eng._watch_filter(Watch(paths=("x",), filter=WatchFilter.DEFAULT, ignore_patterns=(r"\.custom$",)))
    patterns = tuple(r.pattern for r in flt._ignore_entity_regexes)  # DefaultFilter stores compiled regexes here
    assert any("DS_Store" in p for p in patterns), "built-in .DS_Store suppression must survive the extension"
    assert any("py[cod]" in p for p in patterns), "built-in .pyc suppression must survive the extension"
    assert any("custom" in p for p in patterns), "the spec's own ignore must be appended"
    assert {".git", "node_modules"} <= set(flt._ignore_dirs), "default ignore_dirs (.git/node_modules) stay suppressed"


def test_drive_watch_debounce_collapses_storm(
    assay_root: AssayHarness, captured_emits: list[Envelope], rail_probe: RailProbe, monkeypatch: pytest.MonkeyPatch
) -> None:
    """Debounced Watch co-resides the worker and collapses a two-batch storm to one fire.

    Falsified by: the debounce firing per-batch, or ``_co_resident`` failing to cancel the task group on stop
    (the run hangs past the timeout). The release delay exceeds the debounce window so the collapsed storm fires before stop.
    """
    monkeypatch.setattr(_eng, "awatch", _fake_awatch((_FIRST, _SECOND)))

    spec = Watch(paths=(str(assay_root.root),))
    action = Debounce(action=Program(argv=("tool",)), window_ms=30, edge=Edge.TRAILING)
    ctx = _eng._Drive(assay_root.settings, anyio.CapacityLimiter(1), rail_probe.port(rail_probe.ok(("tool",))))

    async def _run() -> None:
        stop = anyio.Event()

        async def _release() -> None:
            await anyio.sleep(0.12)
            stop.set()

        async with anyio.create_task_group() as tg:
            _ = tg.start_soon(_release)
            await _eng._drive(spec, action, ctx, stop=stop, harden=False)

    anyio.run(_run)

    assert _one(captured_emits).status is RailStatus.OK


# --- [LAWS_DRIVE_SCHEDULE]


def test_drive_schedule_fires_then_stops(
    assay_root: AssayHarness, captured_emits: list[Envelope], rail_probe: RailProbe, monkeypatch: pytest.MonkeyPatch
) -> None:
    """Schedule drive fires once per cron wakeup and stops on the shared event.

    Falsified by: the stop event not breaking the cron loop (run hangs past the timeout).
    """
    monkeypatch.setattr(_eng, "_JITTER_MS", 1)
    ctx = _eng._Drive(assay_root.settings, anyio.CapacityLimiter(1), rail_probe.port(rail_probe.ok(("tool",))))

    async def _drive() -> None:
        spec = Schedule(cron="* * * * *")
        stop = anyio.Event()

        monkeypatch.setattr(_eng, "_cron_trigger", lambda _spec: _cron_tick(stop))
        with anyio.move_on_after(5.0) as scope:
            await _eng._drive(spec, Program(argv=("tool",)), ctx, stop=stop, harden=True)
        assert not scope.cancelled_caught, "schedule drive must stop, not hang"

    anyio.run(_drive)

    assert _one(captured_emits).status is RailStatus.OK


def test_drive_schedule_debounce_co_resides_worker(
    assay_root: AssayHarness, captured_emits: list[Envelope], rail_probe: RailProbe, monkeypatch: pytest.MonkeyPatch
) -> None:
    """Schedule Debounce co-resides a worker and collapses cron bursts to one fire.

    Falsified by: failing to cancel on stop or wrapping worker fire in a second single-flight cell.
    The release delay covers the burst plus quiet window so the worker can emit before cancellation.
    """
    ctx = _eng._Drive(assay_root.settings, anyio.CapacityLimiter(1), rail_probe.port(rail_probe.ok(("tool",))))
    wakeups = 0

    def _next(_previous_fire_time: datetime | None, _now: datetime) -> datetime:
        nonlocal wakeups
        wakeups += 1
        return datetime.now(UTC)

    monkeypatch.setattr(_eng, "_cron_trigger", lambda _spec: SimpleNamespace(get_next_fire_time=_next))

    async def _run() -> None:
        spec = Schedule(cron="* * * * *")
        action = Debounce(action=Program(argv=("tool",)), window_ms=30, edge=Edge.TRAILING)
        stop = anyio.Event()

        async def _release() -> None:
            await anyio.sleep(0.15)
            stop.set()

        def _delay(cron: object, previous: datetime | None) -> tuple[datetime | None, float]:
            nonlocal wakeups
            _ = cron, previous
            wakeups += 1
            return (datetime.now(UTC), 0.005 if wakeups < 3 else 3600.0)

        monkeypatch.setattr(_eng, "_next_cron_delay", _delay)
        async with anyio.create_task_group() as tg:
            _ = tg.start_soon(_release)
            await _eng._drive(spec, action, ctx, stop=stop, harden=True)

    anyio.run(_run)

    assert wakeups >= 1, "the cron loop must have fired at least one wakeup before stop"
    assert _one(captured_emits).status is RailStatus.OK


def test_quiesce_drains_until_quiet_window() -> None:
    """``_quiesce`` drains until a quiet window and returns the latest batch.

    Falsified by: returning on the first signal (no drain) or returning a stale batch instead of the latest
    before the quiet window. The second signal arrives inside the quiet window to exercise the drain-continue arm.
    """
    latest: list[tuple[tuple[str, str], ...]] = []

    async def _run() -> None:
        send, recv = anyio.create_memory_object_stream[tuple[tuple[str, str], ...]](4)

        async def _produce() -> None:
            send.send_nowait(_FIRST)
            await anyio.sleep(0.01)
            send.send_nowait(_SECOND)

        async with anyio.create_task_group() as tg, send, recv:
            _ = tg.start_soon(_produce)
            latest.append(await _eng._quiesce(recv, 50))

    anyio.run(_run)

    assert latest == [_SECOND], f"_quiesce must return the latest drained batch; got {latest}"


def test_schedule_exits_on_cancellation(monkeypatch: pytest.MonkeyPatch) -> None:
    """``_schedule`` exits when the enclosing scope cancels the loop.

    Falsified by: swallowing cancellation inside the cron wait loop.
    """
    monkeypatch.setattr(_eng, "_cron_trigger", lambda _spec: SimpleNamespace(get_next_fire_time=lambda *_a: datetime.now(UTC)))
    monkeypatch.setattr(_eng, "_next_cron_delay", lambda *_a: (datetime.now(UTC), 3600.0))

    async def _run() -> None:
        stop = anyio.Event()

        async def _fire(_changes: tuple[tuple[str, str], ...]) -> None:  # noqa: RUF029  # Fire protocol is async; no await needed here
            return None

        with anyio.move_on_after(0.05):
            await _eng._schedule(Schedule(cron="* * * * *"), _fire, stop)

    anyio.run(_run)


def test_fire_with_coalesce_runs_catch_up_on_missed_tick(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """A missed tick runs one catch-up fire and resets the backlog.

    Falsified by: dropping the catch-up fire (one call not two), or returning a nonzero backlog that would
    wedge the hardened cell on the next tick.
    """
    monkeypatch.setattr(_eng, "_JITTER_MS", 1)
    fired: list[tuple[tuple[str, str], ...]] = []

    async def _fire(changes: tuple[tuple[str, str], ...]) -> None:  # noqa: RUF029  # Fire protocol is async; no await needed here
        fired.append(changes)

    async def _run() -> int:
        return await _eng._fire_with_coalesce(_fire, assay_root.settings, _FIRST, 1)

    backlog = anyio.run(_run)

    assert fired == [_FIRST, ()], f"expected primary + one coalesced catch-up; got {fired}"
    assert backlog == 0, "the coalesced cell must reset its backlog to 0"


# --- [LAWS_DRIVE_SETUP_FAULT]


@pytest.mark.parametrize("row", _FAULT_CASES, ids=[c.label for c in _FAULT_CASES])
def test_drive_fault_matrix(row: _FaultCase, assay_root: AssayHarness, captured_emits: list[Envelope]) -> None:
    """Setup, guard, resolve, and decode defects fold into one labelled FAULTED Envelope.

    Falsified by: dropping any boundary that converts those defects into a self-contained Fault.
    """
    anyio.run(drive, row.trigger, row.action, assay_root.settings)

    env = _one(captured_emits)
    assert (env.status, env.claim, env.verb) == (RailStatus.FAULTED, row.claim, row.verb)
    assert env.error is not None
    assert all(s in env.error.message for s in row.substrings), f"expected {row.substrings} in {env.error.message!r}"


def test_drive_emits_ndjson_on_stdout(assay_root: AssayHarness, capsysbinary: pytest.CaptureFixture[bytes]) -> None:
    """``drive`` writes one decodable NDJSON Envelope line to stdout (not stderr).

    Falsified by: writing to stderr, writing non-JSON bytes, or omitting the newline separator.
    """
    from tests.python.tools.assay.kit import (  # noqa: PLC0415  # deferred: single-test oracle import stays off the module import path
        read_one_envelope_from_bytes,
    )

    anyio.run(drive, Manual(), Program(argv=()), assay_root.settings)

    cap = capsysbinary.readouterr()
    env = read_one_envelope_from_bytes(cap.out)
    assert env.status is RailStatus.FAULTED
    assert b"rasm.automation" not in cap.out, "structlog must not bleed onto the Envelope stdout channel"
