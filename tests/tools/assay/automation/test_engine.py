"""Automation engine laws: is_governed ContextVar latch + drive ChangeBatch behavior.

Scope: ``tools.assay.automation.engine`` public symbols — ``is_governed`` and ``drive``
(``ChangeBatch``, ``Fire``, ``Worker`` are type aliases; exempted in conftest ``_EXEMPT``).

Laws are falsifiable by exactly the defects named in each docstring. The verb-implementation laws
feed REALISTIC canned tool output through the shared canned-output DNA (``rail_probe`` for the
``run_check_async``/``rail`` seams, ``cpu_double`` for the psutil governor sample, ``captured_emits``
for the ``_emit`` boundary) so the engine's processing/projection logic executes against a wire-shaped
payload rather than letting the external process no-op.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

import contextvars
from types import SimpleNamespace
from typing import TYPE_CHECKING

import anyio
import anyio.lowlevel
from expression import Ok
import msgspec
import pytest

from tests._aspect import register_law  # noqa: PLC2701  # _-prefixed by S1 design; private to the test tree
from tools.assay.automation import engine as _eng
from tools.assay.automation.engine import is_governed
from tools.assay.automation.model import Debounce, Manual, Program, Rail, Schedule, Sequence, Watch, WatchFilter
from tools.assay.core.model import Claim, Counts, envelope, fold, receipt
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from collections.abc import AsyncIterator

    from tests.tools.assay.conftest import AssayHarness, CpuDoubleInstaller, CpuSampler, RailProbe
    from tools.assay.automation.model import Action, Trigger
    from tools.assay.core.model import Envelope


# --- [CONSTANTS] ------------------------------------------------------------------------

_FIRST: tuple[tuple[str, str], ...] = (("added", "first.txt"),)
_SECOND: tuple[tuple[str, str], ...] = (("modified", "second.txt"),)

_GOVERNOR_CASES: list[tuple[float | None, float, bool]] = [
    (0.9, 91.0, True),  # 91 >= 90  — skip
    (0.95, 91.0, False),  # 91 < 95   — run
    (None, 91.0, False),  # disabled  — always run
    (0.0, 0.0, True),  # 0 >= 0    — exact zero boundary
    (1.0, 100.0, True),  # 100 >= 100 — exact max boundary
    (1.0, 99.9, False),  # 99.9 < 100 — sub-max
    (0.91, 91.0, True),  # exact match — skip
]

_FAULT_CASES: list[tuple[str, Trigger, Action, Claim, str, tuple[str, ...]]] = [
    # except* handler in drive catches ZoneInfoNotFoundError → self-contained "automation setup" Fault, not a task-group escape.
    (
        "test_drive_setup_error_emits_faulted_envelope",
        Schedule(cron="* * * * *", timezone="Invalid/Zone"),
        Rail(claim=Claim.STATIC, verb="plan"),
        Claim.STATIC,
        "plan",
        ("automation setup",),
    ),
    # _program_outcome empty-argv guard → domain Fault instead of an empty-command spawn OSError.
    ("test_drive_empty_argv_emits_faulted_envelope", Manual(), Program(argv=()), Claim.STATIC, "program", ("non-empty",)),
    # _resolve None-arm → _rail_outcome synthesizes the "unbound rail" Fault at the leaf (emitted=False).
    (
        "test_drive_rail_unbound_emits_faulted_via_leaf",
        Manual(),
        Rail(claim=Claim.STATIC, verb="does-not-exist"),
        Claim.STATIC,
        "does-not-exist",
        ("unbound rail", "static:does-not-exist"),
    ),
    # _rail_outcome except msgspec.DecodeError arm → non-JSON Raw params on a registered bind fold to a Fault.
    (
        "test_drive_rail_param_decode_failure_folds_to_fault",
        Manual(),
        Rail(claim=Claim.STATIC, verb="plan", params=msgspec.Raw(b"{not json")),
        Claim.STATIC,
        "plan",
        (),
    ),
    # _label recurses through the Debounce wrapper into the inner Rail(CODE, search) under an except* setup fault.
    (
        "test_drive_debounce_setup_fault_labels_inner_action",
        Schedule(cron="* * * * *", timezone="Invalid/Zone"),
        Debounce(action=Rail(claim=Claim.CODE, verb="search")),
        Claim.CODE,
        "search",
        (),
    ),
]


# --- [OPERATIONS] -----------------------------------------------------------------------


def _one(seen: list[Envelope]) -> Envelope:
    """Assert exactly one emitted Envelope and return it (collapses the len==1 + index-0 wall).

    Returns:
        The single captured Envelope.
    """
    assert len(seen) == 1, f"expected exactly one Envelope, got {len(seen)}: {[e.status for e in seen]!r}"
    return seen[0]


def _recording_sampler(value: float) -> tuple[CpuSampler, list[float | None]]:
    """Build a ``cpu_percent`` double recording each call's ``interval`` arg and returning ``value``.

    The governor issues warmup as ``cpu_percent(0.1)`` and live reads as ``cpu_percent(interval=None)``;
    the recorded log distinguishes the two call shapes for the latch laws.

    Returns:
        A ``(sampler, log)`` pair: ``sampler(interval=None) -> value`` appends ``interval`` to ``log``.
    """
    log: list[float | None] = []

    def _sample(interval: float | None = None) -> float:
        log.append(interval)
        return value

    return _sample, log


def _fake_awatch(batches: tuple[tuple[tuple[str, str], ...], ...]) -> object:
    """Build a canned ``awatch`` replacement yielding ``batches`` then completing.

    Each batch is ``(kind, path)`` rows shaped like a real ``watchfiles`` change set.
    Completion lets ``_watch`` drain and the enclosing task group exit without a real filesystem watcher.

    Returns:
        An async-generator factory accepting the production ``awatch`` kwargs.
    """

    async def _awatch(*_paths: str, **_kw: object) -> AsyncIterator[set[tuple[str, str]]]:  # noqa: RUF029  # async def required; no await in body
        for batch in batches:
            yield {(kind, path) for kind, path in batch}

    return _awatch


# --- [LAWS_IS_GOVERNED]


@pytest.mark.parametrize(
    "threshold,cpu,expected",
    _GOVERNOR_CASES,
    ids=["t0.9-skip", "t0.95-run", "tNone-run", "t0.0-skip", "t1.0-100-skip", "t1.0-99.9-run", "t0.91-skip"],
)
def test_is_governed_boundary(threshold: float | None, cpu: float, *, expected: bool, cpu_double: CpuDoubleInstaller) -> None:
    """``is_governed`` returns True iff ``cpu_percent >= threshold * 100`` (None disables it).

    Falsified by: removing the ``>= threshold * 100`` check, negating the None branch,
    or treating the boundary as strict-greater-than instead of >=.
    """
    # Prime the latch so warmup is not exercised — isolates the decision predicate.
    _eng._CPU_PRIMED.set(True)
    cpu_double(lambda *_a, **_k: cpu)
    assert is_governed(threshold) is expected


register_law(is_governed, "test_is_governed_boundary")


def test_is_governed_latch_primes_once(cpu_double: CpuDoubleInstaller) -> None:
    """The warmup ``cpu_percent(0.1)`` fires exactly once per ``anyio.run`` context (fresh ``contextvars.Context``).

    Falsified by: removing ``_CPU_PRIMED.set(True)`` (warmup every call) or a module-level ``bool`` over a ``ContextVar`` (no cross-run isolation).
    """
    sampler, intervals = _recording_sampler(50.0)
    cpu_double(sampler)

    async def _run() -> None:
        # Fresh contextvars.Context → latch resets to its False default regardless of outer state.
        await anyio.lowlevel.checkpoint()
        assert _eng._CPU_PRIMED.get() is False, "ContextVar must start at its False default in a fresh context"
        is_governed(0.9)
        assert _eng._CPU_PRIMED.get() is True, "latch must be True after first governed call"
        is_governed(0.9)

    contextvars.Context().run(anyio.run, _run)

    assert [c for c in intervals if c is not None] == [0.1], f"expected one warmup call, got {intervals}"
    assert [c for c in intervals if c is None] == [None, None], f"expected two non-blocking reads, got {intervals}"


register_law(is_governed, "test_is_governed_latch_primes_once")


def test_is_governed_contextvar_isolates_across_runs(cpu_double: CpuDoubleInstaller) -> None:
    """``_CPU_PRIMED`` resets to ``False`` for each ``anyio.run`` — no cross-test contamination.

    Falsified by: a module-level mutable over ``ContextVar[bool]`` — the latch stays ``True`` across separate ``anyio.run`` calls, skipping warmup.
    """
    seen_defaults: list[bool] = []
    _eng._CPU_PRIMED.set(False)  # reset the outer latch to a known False baseline per anyio.run
    cpu_double(lambda *_a, **_k: 50.0)

    async def _capture_default() -> None:
        await anyio.lowlevel.checkpoint()
        seen_defaults.append(_eng._CPU_PRIMED.get())
        _eng._CPU_PRIMED.set(True)  # deliberately pollute the inner context

    anyio.run(_capture_default)
    anyio.run(_capture_default)

    assert seen_defaults == [False, False], f"ContextVar must be False at start of each anyio.run, got {seen_defaults}"


register_law(is_governed, "test_is_governed_contextvar_isolates_across_runs")


def test_is_governed_disabled_never_primes(cpu_double: CpuDoubleInstaller) -> None:
    """``is_governed(None)`` returns ``False`` without touching psutil.

    Falsified by: calling ``cpu_percent`` in the ``None`` branch — breaks the «threshold-less CLI runs pay no warm-up tax» invariant.
    """
    sampler, calls = _recording_sampler(0.0)
    cpu_double(sampler)
    _eng._CPU_PRIMED.set(False)

    assert is_governed(None) is False
    assert not calls, f"psutil.cpu_percent must not be called when threshold is None; got {calls}"


register_law(is_governed, "test_is_governed_disabled_never_primes")


def test_is_governed_primed_latch_reads_without_warmup(cpu_double: CpuDoubleInstaller) -> None:
    """With the latch already primed, ``is_governed`` reads the non-blocking sample with no warmup (``_CPU_PRIMED`` True arm).

    Falsified by: re-priming on every call — a primed latch would still issue the blocking 0.1s warmup.
    """
    sampler, intervals = _recording_sampler(80.0)
    cpu_double(sampler)
    _eng._CPU_PRIMED.set(True)

    assert is_governed(0.5) is True
    assert intervals == [None], f"primed latch must read once with no warmup; got {intervals}"


register_law(is_governed, "test_is_governed_primed_latch_reads_without_warmup")


# --- [LAWS_DRIVE_PROGRAM]


def test_drive_program_projects_completed_to_report(
    assay_root: AssayHarness, captured_emits: list[Envelope], rail_probe: RailProbe, monkeypatch: pytest.MonkeyPatch
) -> None:
    """A canned OK ``Completed`` (rc=0, stdout payload) projects through ``fold`` into one STATIC/program OK Envelope.

    Falsified by: ``_program_outcome`` mis-folding the status, ``_program_envelope`` wrapping the Fault arm on success, or ``fold`` dropping the OK.
    """
    canned = Ok(receipt(("dotnet", "build"), 0, stdout=b"Build succeeded.\n", status=RailStatus.OK))
    rail_probe.install(monkeypatch, _eng, "run_check_async", canned)

    anyio.run(_eng.drive, Manual(), Program(argv=("dotnet", "build")), assay_root.settings)

    assert rail_probe.commands == [("dotnet", "build")], f"engine must route the program argv verbatim; got {rail_probe.commands}"
    env = _one(captured_emits)
    assert (env.status, env.claim, env.verb) == (RailStatus.OK, Claim.STATIC, "program")
    assert env.report is not None
    assert (env.report.status, env.report.counts) == (RailStatus.OK, Counts(1, 0, 1))


register_law(_eng.drive, "test_drive_program_projects_completed_to_report")


def test_drive_program_fault_arm_emits_fault(
    assay_root: AssayHarness, captured_emits: list[Envelope], rail_probe: RailProbe, monkeypatch: pytest.MonkeyPatch
) -> None:
    """A canned spawn ``Fault`` (the channel only Faults on spawn/timeout, never a nonzero exit) flows through the error arm to a FAULTED Envelope.

    Falsified by: ``_program_envelope`` swallowing the Fault and emitting an OK Report, or ``_program_outcome`` re-wrapping the Fault as a Completed.
    """
    rail_probe.install(monkeypatch, _eng, "run_check_async", rail_probe.error(("missing-tool",), "spawn: executable not found"))

    anyio.run(_eng.drive, Manual(), Program(argv=("missing-tool",)), assay_root.settings)

    env = _one(captured_emits)
    assert (env.status, env.verb) == (RailStatus.FAULTED, "program")
    assert env.error is not None
    assert env.error.message == "spawn: executable not found"


register_law(_eng.drive, "test_drive_program_fault_arm_emits_fault")


@pytest.mark.parametrize(
    "rc,status,counts",
    [
        (1, RailStatus.FAILED, Counts(0, 1, 1)),  # nonzero exit stays on the Completed channel as FAILED
        (0, RailStatus.EMPTY, Counts(1, 0, 1)),  # zero-exit no-op folds to EMPTY (still ok-counted)
        (124, RailStatus.TIMEOUT, Counts(0, 0, 0)),  # timeout receipt folds to neither ok nor failed
    ],
    ids=["rc1-failed", "rc0-empty", "rc124-timeout"],
)
def test_drive_program_status_projection(
    rc: int,
    status: RailStatus,
    counts: Counts,
    assay_root: AssayHarness,
    captured_emits: list[Envelope],
    rail_probe: RailProbe,
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    """Process exit codes ride the Completed channel: rc projects to status+counts via ``fold``.

    Falsified by: ``_program_outcome`` promoting a nonzero exit to a Fault (only spawn/timeout Fault),
    or ``fold``/``_count`` miscounting the receipt status into the wrong (ok, failed, total) bucket.
    """
    rail_probe.install(monkeypatch, _eng, "run_check_async", rail_probe.receipt(("tool",), rc, status=status, stdout=b"out"))

    anyio.run(_eng.drive, Manual(), Program(argv=("tool",)), assay_root.settings)

    env = _one(captured_emits)
    assert env.status is status
    assert env.report is not None
    assert env.report.counts == counts


register_law(_eng.drive, "test_drive_program_status_projection")


# --- [LAWS_DRIVE_RAIL]


def test_drive_rail_resolves_bind_and_emits_canned_envelope(
    assay_root: AssayHarness, captured_emits: list[Envelope], rail_probe: RailProbe, monkeypatch: pytest.MonkeyPatch
) -> None:
    """A bound Rail resolves ``STATIC:plan``, decodes default params, and threads the rail's own ``emitted=True`` Envelope (no double-emit).

    Falsified by: ``_emit_leaf`` re-emitting an already-written rail Envelope, or ``_rail_outcome`` failing to resolve a registered claim/verb pair.
    """
    rail_env = envelope(fold(Claim.STATIC, "plan", (receipt(("plan",), 0, status=RailStatus.OK),)), claim=Claim.STATIC, verb="plan")
    rail_probe.install(monkeypatch, _eng, "rail", rail_env)

    anyio.run(_eng.drive, Manual(), Rail(claim=Claim.STATIC, verb="plan"), assay_root.settings)

    # The factory recorded a ("rail.run", (params,), {}) row → the registry runner ran with decoded params.
    assert [c for c in rail_probe.calls if c[0] == "rail.run"], "the registry runner must be invoked with decoded params"
    # emitted=True: the rail wrote its own Envelope, so the automation boundary must NOT call _emit again.
    assert captured_emits == [], f"already-emitted rail Envelope must not be re-emitted; got {len(captured_emits)}"


register_law(_eng.drive, "test_drive_rail_resolves_bind_and_emits_canned_envelope")


# --- [LAWS_DRIVE_GOVERNOR]


def test_drive_governed_skip_emits_one_skip_envelope(
    assay_root: AssayHarness, captured_emits: list[Envelope], rail_probe: RailProbe, cpu_double: CpuDoubleInstaller, monkeypatch: pytest.MonkeyPatch
) -> None:
    """A tripped CPU governor (Watch ``cpu_threshold`` + high-CPU psutil) emits one SKIP Envelope (Counts(1,0,1), governed note); the leaf never runs.

    Falsified by: ``_emit_leaf`` running the leaf despite the governor, mis-counting the governed leaf, or dropping the ``governed: cpu>=`` note.
    """
    _eng._CPU_PRIMED.set(True)  # skip warmup; isolate the governor decision
    cpu_double(lambda *_a, **_k: 100.0)  # always over threshold
    rail_probe.install(monkeypatch, _eng, "run_check_async", rail_probe.ok(("tool",)))
    monkeypatch.setattr(_eng, "awatch", _fake_awatch((_FIRST,)))

    spec = Watch(paths=(str(assay_root.root),), cpu_threshold=0.5)
    anyio.run(_eng.drive, spec, Program(argv=("tool",)), assay_root.settings)

    assert rail_probe.commands == [], "the governor must skip the fire before the leaf runs"
    env = _one(captured_emits)
    assert env.status is RailStatus.SKIP
    assert env.report is not None
    assert env.report.counts == Counts(1, 0, 1)
    assert any("governed: cpu>=" in note for note in env.report.notes)


register_law(_eng.drive, "test_drive_governed_skip_emits_one_skip_envelope")


# --- [LAWS_DRIVE_SEQUENCE]


def test_drive_sequence_folds_to_highest_severity(
    assay_root: AssayHarness, captured_emits: list[Envelope], rail_probe: RailProbe, monkeypatch: pytest.MonkeyPatch
) -> None:
    """A Sequence of canned-OK Program leaves folds by severity at constant stack depth (two OK programs → OK dominates the EMPTY seed).

    Falsified by: ``_sequence`` halting early on a non-defect status, or the severity fold losing the OK status across leaves.
    """
    rail_probe.install(monkeypatch, _eng, "run_check_async", rail_probe.ok(("p",)))

    seq = Sequence(actions=(Program(argv=("p", "1")), Program(argv=("p", "2"))))
    anyio.run(_eng.drive, Manual(), seq, assay_root.settings)

    assert len(captured_emits) == 2, f"both sequence leaves must emit; got {len(captured_emits)}"
    assert all(env.status is RailStatus.OK for env in captured_emits)


register_law(_eng.drive, "test_drive_sequence_folds_to_highest_severity")


def test_drive_nested_sequence_and_debounce_leaves(
    assay_root: AssayHarness, captured_emits: list[Envelope], rail_probe: RailProbe, monkeypatch: pytest.MonkeyPatch
) -> None:
    """A Sequence whose leaves include a nested Sequence and a Debounce recurses through ``_emit_leaf`` (nested-seq recursion + pure-wrapper unwrap).

    Falsified by: ``_emit_leaf`` not recursing on a nested Sequence, or the Debounce arm not unwrapping to its inner action (a missing or extra fire).
    """
    rail_probe.install(monkeypatch, _eng, "run_check_async", rail_probe.ok(("p",)))

    seq = Sequence(
        actions=(
            Program(argv=("p", "outer")),
            Sequence(actions=(Program(argv=("p", "nested")),)),  # → _emit_leaf Sequence arm
            Debounce(action=Program(argv=("p", "wrapped"))),  # → _emit_leaf Debounce arm (pure unwrap)
        )
    )
    anyio.run(_eng.drive, Manual(), seq, assay_root.settings)

    assert len(captured_emits) == 3, f"every nested leaf must fire exactly once; got {len(captured_emits)}"
    assert all(env.status is RailStatus.OK for env in captured_emits)


register_law(_eng.drive, "test_drive_nested_sequence_and_debounce_leaves")


def test_drive_manual_debounce_unwraps_to_inner_leaf(
    assay_root: AssayHarness, captured_emits: list[Envelope], rail_probe: RailProbe, monkeypatch: pytest.MonkeyPatch
) -> None:
    """A Manual trigger on a top-level Debounce bypasses ``_armed`` and unwraps to ``_emit_leaf(inner)``, firing the wrapped action once (no worker).

    Falsified by: ``_fire`` treating a Manual Debounce as a no-op, or double-firing the inner leaf.
    """
    rail_probe.install(monkeypatch, _eng, "run_check_async", rail_probe.ok(("tool",)))

    action = Debounce(action=Program(argv=("tool",)), window_ms=500, collapse=True)
    anyio.run(_eng.drive, Manual(), action, assay_root.settings)

    env = _one(captured_emits)
    assert (env.status, env.verb) == (RailStatus.OK, "program")


register_law(_eng.drive, "test_drive_manual_debounce_unwraps_to_inner_leaf")


def test_hardened_fire_coalesces_reentrant_tick(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """A tick re-entering the hardened cell while a fire is active hits the ``running`` guard's ``missed += 1`` arm — no concurrent fire.

    Falsified by: removing the ``running`` guard — the re-entrant tick starts a second concurrent fire (fire-body count > 1) instead of coalescing.
    """
    monkeypatch.setattr(_eng, "_JITTER_MS", 1)
    started: list[int] = []
    completed: list[int] = []

    async def _run() -> None:
        async def _fire(_changes: tuple[tuple[str, str], ...]) -> None:
            started.append(1)
            await anyio.lowlevel.checkpoint()
            if len(started) == 1:
                await hardened(())  # re-enter while running=True → coalesced via the missed cell
            completed.append(1)

        hardened = _eng._hardened_fire(_fire, assay_root.settings)
        await hardened(())

    anyio.run(_run)

    assert started == [1], f"the re-entrant tick must not start a second fire; got {started}"
    assert completed == [1]


register_law(_eng.drive, "test_hardened_fire_coalesces_reentrant_tick")


def test_debounce_signal_after_close_is_silent() -> None:
    """A ``signal`` after the worker's ``finally: aclose()`` closed the channel returns silently via the ClosedResourceError arm.

    Falsified by: removing the ``except anyio.ClosedResourceError`` arm — a late tick after shutdown would raise instead of being dropped.
    """

    async def _inner(_changes: tuple[tuple[str, str], ...]) -> None:  # noqa: RUF029  # Fire protocol is async; no await needed here
        return None

    async def _run() -> None:
        signal, worker = _eng._debounce(_inner, 40, collapse=True)
        async with anyio.create_task_group() as tg:
            tg.start_soon(worker)
            await anyio.lowlevel.checkpoint()
            tg.cancel_scope.cancel()  # worker finally closes both streams
        await signal(_FIRST)  # post-close signal: must hit the ClosedResourceError arm and return

    anyio.run(_run)  # completing without raising IS the assertion


register_law(_eng.drive, "test_debounce_signal_after_close_is_silent")


def test_drive_sequence_halts_on_fault(
    assay_root: AssayHarness, captured_emits: list[Envelope], rail_probe: RailProbe, monkeypatch: pytest.MonkeyPatch
) -> None:
    """A FAULTED leaf (first leaf empty argv) short-circuits ``_sequence`` before the trailing leaf runs.

    Falsified by: ``_sequence`` continuing past a FAULTED status, which would emit a second Envelope.
    """
    rail_probe.install(monkeypatch, _eng, "run_check_async", rail_probe.ok(("p",)))

    # First leaf has empty argv → _program_outcome FAULTs before run_check_async; second leaf would run a tool.
    seq = Sequence(actions=(Program(argv=()), Program(argv=("p", "2"))))
    anyio.run(_eng.drive, Manual(), seq, assay_root.settings)

    assert _one(captured_emits).status is RailStatus.FAULTED
    assert rail_probe.commands == [], "the trailing leaf must not run after a FAULTED halt"


register_law(_eng.drive, "test_drive_sequence_halts_on_fault")


# --- [LAWS_DRIVE_WATCH]


def test_drive_watch_fires_per_canned_batch(
    assay_root: AssayHarness, captured_emits: list[Envelope], rail_probe: RailProbe, monkeypatch: pytest.MonkeyPatch
) -> None:
    """``drive(Watch, Program)`` fires once per canned ``awatch`` batch (worker None → ``_co_resident`` early return) and emits one Envelope each.

    Falsified by: ``_watch`` dropping a batch, ``_drive_watch`` deadlocking with no debounce worker, or ``_watch_filter`` returning the wrong filter.
    """
    rail_probe.install(monkeypatch, _eng, "run_check_async", rail_probe.ok(("tool",)))
    monkeypatch.setattr(_eng, "awatch", _fake_awatch((_FIRST, _SECOND)))

    spec = Watch(paths=(str(assay_root.root),))
    anyio.run(_eng.drive, spec, Program(argv=("tool",)), assay_root.settings)

    assert len(captured_emits) == 2, f"one Envelope per watch batch; got {len(captured_emits)}"
    assert all(env.status is RailStatus.OK for env in captured_emits)


register_law(_eng.drive, "test_drive_watch_fires_per_canned_batch")


@pytest.mark.parametrize(
    "filter_tag,expected_type",
    [(WatchFilter.DEFAULT, "DefaultFilter"), (WatchFilter.PYTHON, "PythonFilter")],
    ids=["default-filter", "python-filter"],
)
def test_watch_filter_resolves_tag_to_watchfiles_filter(filter_tag: WatchFilter, expected_type: str) -> None:
    """``_watch_filter`` projects the wire tag onto the matching ``watchfiles`` filter instance (both match arms; PythonFilter adds ``.pyi``).

    Falsified by: swapping the DEFAULT/PYTHON arms or constructing the wrong filter class.
    """
    # A valid regex satisfies both arms, isolating tag→filter dispatch from pattern compilation (DefaultFilter: regex; PythonFilter: literal).
    spec = Watch(paths=("x",), filter=filter_tag, ignore_patterns=(r"\.pyc$",))
    assert type(_eng._watch_filter(spec)).__name__ == expected_type


register_law(_eng.drive, "test_watch_filter_resolves_tag_to_watchfiles_filter")


def test_drive_watch_debounce_collapses_storm(
    assay_root: AssayHarness, captured_emits: list[Envelope], rail_probe: RailProbe, monkeypatch: pytest.MonkeyPatch
) -> None:
    """A Debounce-wrapped Watch action co-resides the worker; two batches in the quiescence window collapse to one fire, then ``stop`` cancels it.

    Falsified by: the debounce firing per-batch, or ``_co_resident`` failing to cancel the task group on stop (the run hangs past the timeout).
    """
    rail_probe.install(monkeypatch, _eng, "run_check_async", rail_probe.ok(("tool",)))
    monkeypatch.setattr(_eng, "awatch", _fake_awatch((_FIRST, _SECOND)))

    spec = Watch(paths=(str(assay_root.root),))
    action = Debounce(action=Program(argv=("tool",)), window_ms=30, collapse=True)

    async def _run() -> None:
        stop = anyio.Event()

        async def _release() -> None:
            await anyio.sleep(0.12)  # > the 30ms debounce window: let the storm collapse + fire once
            stop.set()

        async with anyio.create_task_group() as tg:
            tg.start_soon(_release)
            await _eng._drive_watch(spec, action, assay_root.settings, limiter=anyio.CapacityLimiter(1), stop=stop)

    anyio.run(_run)

    assert _one(captured_emits).status is RailStatus.OK


register_law(_eng.drive, "test_drive_watch_debounce_collapses_storm")


# --- [LAWS_DRIVE_SCHEDULE]


def test_drive_schedule_fires_then_stops(
    assay_root: AssayHarness, captured_emits: list[Envelope], rail_probe: RailProbe, monkeypatch: pytest.MonkeyPatch
) -> None:
    """``drive(Schedule, Program)`` fires once per cron wakeup (``_hardened_fire`` single-flight) and stops the cron on teardown.

    Falsified by: ``_schedule`` not calling ``cron.stop()`` on teardown, or the stop event not breaking the cron loop (run hangs past the timeout).
    """
    monkeypatch.setattr(_eng, "_JITTER_MS", 1)
    rail_probe.install(monkeypatch, _eng, "run_check_async", rail_probe.ok(("tool",)))
    stopped: list[bool] = []

    async def _drive() -> None:
        spec = Schedule(cron="* * * * *")
        stop = anyio.Event()
        wakeups = 0

        def _crontab(*_a: object, **_k: object) -> SimpleNamespace:
            async def _next() -> None:
                nonlocal wakeups
                await anyio.lowlevel.checkpoint()
                wakeups += 1
                # First wakeup fires; the second arms stop so `_schedule`'s post-next guard skips the trailing fire.
                if wakeups >= 2:
                    stop.set()

            return SimpleNamespace(next=_next, stop=lambda: stopped.append(True))

        monkeypatch.setattr(_eng, "aiocron", SimpleNamespace(crontab=_crontab))
        with anyio.move_on_after(5.0) as scope:
            await _eng._drive_schedule(spec, Program(argv=("tool",)), assay_root.settings, limiter=anyio.CapacityLimiter(1), stop=stop)
        assert not scope.cancelled_caught, "schedule drive must stop, not hang"

    anyio.run(_drive)

    assert stopped == [True], "cron.stop() must run exactly once on teardown"
    assert _one(captured_emits).status is RailStatus.OK


register_law(_eng.drive, "test_drive_schedule_fires_then_stops")


def test_drive_schedule_debounce_co_resides_worker(
    assay_root: AssayHarness, captured_emits: list[Envelope], rail_probe: RailProbe, monkeypatch: pytest.MonkeyPatch
) -> None:
    """A Schedule + Debounce skips the ``_hardened_fire`` wrap (``_armed`` non-None worker); cron-wakeup bursts collapse to one fire until ``stop``.

    Falsified by: ``_co_resident`` not cancelling on stop (hang), or the Schedule wrapping a worker fire in ``_hardened_fire`` (double single-flight).
    """
    rail_probe.install(monkeypatch, _eng, "run_check_async", rail_probe.ok(("tool",)))
    wakeups = 0

    def _crontab(*_a: object, **_k: object) -> SimpleNamespace:
        async def _next() -> None:
            nonlocal wakeups
            await anyio.lowlevel.checkpoint()
            wakeups += 1
            # Three rapid wakeups coalesce; then sleep forever so the quiet window elapses before stop cancels.
            await (anyio.sleep(0.005) if wakeups < 3 else anyio.sleep_forever())

        return SimpleNamespace(next=_next, stop=lambda: None)

    monkeypatch.setattr(_eng, "aiocron", SimpleNamespace(crontab=_crontab))

    async def _run() -> None:
        spec = Schedule(cron="* * * * *")
        action = Debounce(action=Program(argv=("tool",)), window_ms=30, collapse=True)
        stop = anyio.Event()

        async def _release() -> None:
            await anyio.sleep(0.15)  # burst (~15ms) + quiet window (30ms) + fire, then stop
            stop.set()

        async with anyio.create_task_group() as tg:
            tg.start_soon(_release)
            await _eng._drive_schedule(spec, action, assay_root.settings, limiter=anyio.CapacityLimiter(1), stop=stop)

    anyio.run(_run)

    assert wakeups >= 1, "the cron loop must have fired at least one wakeup before stop"
    assert _one(captured_emits).status is RailStatus.OK


register_law(_eng.drive, "test_drive_schedule_debounce_co_resides_worker")


def test_quiesce_drains_until_quiet_window() -> None:
    """``_quiesce`` drains both batches via the ``case False: pass`` continue-arm, returning the latest after one quiet window.

    Falsified by: ``_quiesce`` returning on the first signal (no drain) or returning a stale batch instead of the latest before the quiet window.
    """
    latest: list[tuple[tuple[str, str], ...]] = []

    async def _run() -> None:
        send, recv = anyio.create_memory_object_stream[tuple[tuple[str, str], ...]](4)

        async def _produce() -> None:
            send.send_nowait(_FIRST)
            await anyio.sleep(0.01)
            send.send_nowait(_SECOND)  # second signal arrives inside the 50ms window → drain-continue arm

        async with anyio.create_task_group() as tg, send, recv:
            tg.start_soon(_produce)
            latest.append(await _eng._quiesce(recv, 50))

    anyio.run(_run)

    assert latest == [_SECOND], f"_quiesce must return the latest drained batch; got {latest}"


register_law(_eng.drive, "test_quiesce_drains_until_quiet_window")


def test_schedule_stops_cron_on_cancellation(monkeypatch: pytest.MonkeyPatch) -> None:
    """``_schedule`` calls ``cron.stop()`` when the enclosing scope cancels the loop.

    Falsified by: removing the ``finally: cron.stop()`` clause — a cancelled schedule leaks the
    aiocron timer instead of releasing it.
    """
    stopped: list[bool] = []

    async def _next() -> None:
        await anyio.sleep_forever()

    monkeypatch.setattr(_eng, "aiocron", SimpleNamespace(crontab=lambda *_a, **_k: SimpleNamespace(next=_next, stop=lambda: stopped.append(True))))

    async def _run() -> None:
        stop = anyio.Event()

        async def _fire(_changes: tuple[tuple[str, str], ...]) -> None:  # noqa: RUF029  # Fire protocol is async; no await needed here
            return None

        with anyio.move_on_after(0.05):
            await _eng._schedule(Schedule(cron="* * * * *"), _fire, stop)

    anyio.run(_run)

    assert stopped == [True]


register_law(_eng.drive, "test_schedule_stops_cron_on_cancellation")


def test_fire_with_coalesce_runs_catch_up_on_missed_tick(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """A nonzero ``missed`` makes ``_fire_with_coalesce`` jitter+fire the primary, run one ``_coalesce_missed_fire`` catch-up, then reset to 0.

    Falsified by: dropping the catch-up fire (one call not two), or returning a nonzero backlog that would wedge the hardened cell on the next tick.
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


register_law(_eng.drive, "test_fire_with_coalesce_runs_catch_up_on_missed_tick")


def test_hardened_fire_faults_reset_after_exception(
    assay_root: AssayHarness, captured_emits: list[Envelope], monkeypatch: pytest.MonkeyPatch
) -> None:
    """A raising fire emits one FAULTED Envelope (no action → STATIC/automation label) and the ``finally`` resets the cell so the next tick fires.

    Falsified by: removing the ``except``/``finally`` — the exception escapes the task group or wedges ``running=True`` so later ticks never fire.
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
        await hardened(())  # raises internally → one FAULTED Envelope
        await hardened(())  # cell reset → second fire runs cleanly

    anyio.run(_run)

    assert calls == [1, 1], "the cell must reset and fire again after a fault"
    env = _one(captured_emits)
    assert (env.status, env.claim, env.verb) == (RailStatus.FAULTED, Claim.STATIC, "automation")  # no action → default automation label
    assert env.error is not None
    assert "RuntimeError: boom" in env.error.message


register_law(_eng.drive, "test_hardened_fire_faults_reset_after_exception")


# --- [LAWS_DRIVE_DEBOUNCE]


@pytest.mark.parametrize("collapse", [False, True], ids=["leading", "trailing"])
@pytest.mark.anyio
async def test_debounce_fires_once_per_storm(*, collapse: bool) -> None:
    """Both leading (collapse=False) and trailing (collapse=True) modes fire once per storm window (size-1 channel coalesces two signals).

    Falsified by: firing per-signal, or the worker leaking the channel under ``filterwarnings=error`` (an unclosed-stream ResourceWarning fails).
    """
    fired: list[tuple[tuple[str, str], ...]] = []

    async def _inner(changes: tuple[tuple[str, str], ...]) -> None:  # noqa: RUF029  # Fire protocol is async; no await needed here
        fired.append(changes)

    signal, worker = _eng._debounce(_inner, 40, collapse=collapse)
    async with anyio.create_task_group() as tg:
        tg.start_soon(worker)
        await signal(_FIRST)
        await anyio.lowlevel.checkpoint()
        await signal(_SECOND)  # coalesced: the size-1 channel already holds _FIRST
        await anyio.sleep(0.07)  # > 40ms quiescence window
        tg.cancel_scope.cancel()

    assert len(fired) == 1, f"each storm window must produce exactly one fire; got {fired}"


register_law(_eng.drive, "test_debounce_fires_once_per_storm")


def test_debounce_signal_ignores_when_buffer_full() -> None:
    """A second ``signal`` while the worker is pinned in its first fire (full size-1 buffer) drops via the ``current_buffer_used != 0`` arm.

    Falsified by: the signal raising on a full channel (no ``send_nowait`` guard), or growing past size 1 (a second signal reaches ``inner``).
    """
    fired: list[tuple[tuple[str, str], ...]] = []
    pinned = anyio.Event()

    async def _inner(changes: tuple[tuple[str, str], ...]) -> None:
        fired.append(changes)
        await pinned.wait()  # pin the worker inside the first fire so it stops draining the channel

    async def _run() -> None:
        signal, worker = _eng._debounce(_inner, 40, collapse=False)
        async with anyio.create_task_group() as tg:
            tg.start_soon(worker)
            await signal(_FIRST)  # worker drains this, then pins inside _inner
            await anyio.sleep(0.02)  # let the worker block on `pinned` so the buffer stays full
            await signal(_SECOND)  # buffer full → dropped without error
            await signal(_FIRST)  # also dropped
            await anyio.sleep(0.02)
            tg.cancel_scope.cancel()  # triggers worker finally → aclose() both streams

    anyio.run(_run)

    assert fired == [_FIRST], f"buffer-full signals must drop, not fire; got {fired}"


register_law(_eng.drive, "test_debounce_signal_ignores_when_buffer_full")


# --- [LAWS_DRIVE_SETUP_FAULT]


def test_drive_manual_program_emits_single_envelope(
    assay_root: AssayHarness, captured_emits: list[Envelope], rail_probe: RailProbe, monkeypatch: pytest.MonkeyPatch
) -> None:
    """``drive(Manual(), Program(argv=('true',)), settings)`` emits exactly one Envelope.

    Falsified by: any code path that writes 0 or 2+ Envelopes — e.g., a double-emit bug where
    both the Program outcome and an outer fault handler both call ``_emit``.
    """
    rail_probe.install(monkeypatch, _eng, "run_check_async", rail_probe.ok(("true",)))

    anyio.run(_eng.drive, Manual(), Program(argv=("true",)), assay_root.settings)

    env = _one(captured_emits)
    assert (env.claim, env.verb) == (Claim.STATIC, "program")


register_law(_eng.drive, "test_drive_manual_program_emits_single_envelope")


@pytest.mark.parametrize(
    "trigger,action,claim,verb,substrings", [c[1:] for c in _FAULT_CASES], ids=[c[0].removeprefix("test_drive_") for c in _FAULT_CASES]
)
def test_drive_faults(
    trigger: Trigger, action: Action, claim: Claim, verb: str, substrings: tuple[str, ...], assay_root: AssayHarness, captured_emits: list[Envelope]
) -> None:
    """Every setup-time / guard / resolve / decode defect folds into exactly one FAULTED Envelope labelled by the wrapped ``_label`` (claim, verb).

    Falsified by: dropping the ``except*`` handler, the empty-argv guard, the unbound-rail synthesis, the ``msgspec.DecodeError`` arm, or the
    Debounce ``_label`` recursion — each escapes the boundary instead of folding into a self-contained Fault.
    """
    anyio.run(_eng.drive, trigger, action, assay_root.settings)

    env = _one(captured_emits)
    assert (env.status, env.claim, env.verb) == (RailStatus.FAULTED, claim, verb)
    assert env.error is not None
    assert all(s in env.error.message for s in substrings), f"expected {substrings} in {env.error.message!r}"


for _case in _FAULT_CASES:
    register_law(_eng.drive, _case[0])


def test_drive_emits_ndjson_on_stdout(assay_root: AssayHarness, capsysbinary: pytest.CaptureFixture[bytes]) -> None:
    """``drive`` writes one decodable NDJSON Envelope line to stdout (not stderr).

    Falsified by: writing the Envelope to stderr, writing raw non-JSON bytes, or omitting the
    newline separator — any of which breaks the NDJSON framing contract for downstream consumers.
    """
    from tests.tools.assay.conftest import (  # noqa: PLC0415  # deferred: conftest import is typed; top-level would be circular
        read_one_envelope_from_bytes,
    )

    anyio.run(_eng.drive, Manual(), Program(argv=()), assay_root.settings)

    cap = capsysbinary.readouterr()
    env = read_one_envelope_from_bytes(cap.out)
    assert env.status is RailStatus.FAULTED
    # Structlog diagnostics must stay on stderr — stdout is the Envelope-only channel.
    assert b"rasm.automation" not in cap.out, "structlog must not bleed onto the Envelope stdout channel"


register_law(_eng.drive, "test_drive_emits_ndjson_on_stdout")
