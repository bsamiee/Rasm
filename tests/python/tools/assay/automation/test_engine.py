"""Laws for ``tools.assay.automation.engine``.

Covers ContextVar CPU governance and trigger/action projection through canned process, rail, psutil,
watchfiles, and emit boundaries.
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

from tests.python._testkit.laws import register_law
from tools.assay.automation import engine as _eng
from tools.assay.automation.engine import is_governed
from tools.assay.automation.model import Debounce, Edge, Manual, Program, Rail, Schedule, Sequence, Watch, WatchFilter
from tools.assay.core.model import Claim, Counts, envelope, fold, receipt
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from collections.abc import AsyncIterator

    from tests.python.tools.assay.kit import AssayHarness, CpuDoubleInstaller, CpuSampler, RailProbe
    from tools.assay.automation.model import Action, Trigger
    from tools.assay.core.model import Envelope


# --- [CONSTANTS] ------------------------------------------------------------------------

_FIRST: tuple[tuple[str, str], ...] = (("added", "first.txt"),)
_SECOND: tuple[tuple[str, str], ...] = (("modified", "second.txt"),)

_GOVERNOR_CASES: list[tuple[float | None, float, bool]] = [
    (0.9, 91.0, True),
    (0.95, 91.0, False),
    (None, 91.0, False),
    (0.0, 0.0, True),
    (1.0, 100.0, True),
    (1.0, 99.9, False),
    (0.91, 91.0, True),
]

_FAULT_CASES: list[tuple[str, Trigger, Action, Claim, str, tuple[str, ...]]] = [
    # Setup faults stay envelope-local instead of escaping the task group.
    (
        "test_drive_setup_error_emits_faulted_envelope",
        Schedule(cron="* * * * *", timezone="Invalid/Zone"),
        Rail(claim=Claim.STATIC, verb="static"),
        Claim.STATIC,
        "static",
        ("automation setup",),
    ),
    # Empty argv faults before process spawn.
    ("test_drive_empty_argv_emits_faulted_envelope", Manual(), Program(argv=()), Claim.STATIC, "program", ("non-empty",)),
    # Unbound rails fault at the leaf with no prior emission.
    (
        "test_drive_rail_unbound_emits_faulted_via_leaf",
        Manual(),
        Rail(claim=Claim.STATIC, verb="does-not-exist"),
        Claim.STATIC,
        "does-not-exist",
        ("unbound rail", "static:does-not-exist"),
    ),
    # Registered rail params decode failures fold to Fault.
    (
        "test_drive_rail_param_decode_failure_folds_to_fault",
        Manual(),
        Rail(claim=Claim.STATIC, verb="static", params=msgspec.Raw(b"{not json")),
        Claim.STATIC,
        "static",
        (),
    ),
    # Setup faults label Debounce wrappers by the inner action.
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
    # Isolate the decision predicate from warmup.
    _eng._CPU_PRIMED.set(True)
    cpu_double(lambda *_a, **_k: cpu)
    assert is_governed(threshold) is expected


register_law(is_governed, "test_is_governed_boundary")


def test_is_governed_latch_primes_once(cpu_double: CpuDoubleInstaller) -> None:
    """The warmup ``cpu_percent(0.1)`` fires exactly once per ``anyio.run`` context (fresh ``contextvars.Context``).

    Falsified by: removing ``_CPU_PRIMED.set(True)`` or replacing the ContextVar with module state.
    """
    sampler, intervals = _recording_sampler(50.0)
    cpu_double(sampler)

    async def _run() -> None:
        # Fresh contextvars.Context ignores outer latch pollution.
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

    Falsified by: a module-level mutable latch staying True across separate ``anyio.run`` calls.
    """
    seen_defaults: list[bool] = []
    _eng._CPU_PRIMED.set(False)
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

    Falsified by: calling ``cpu_percent`` in the ``None`` branch.
    """
    sampler, calls = _recording_sampler(0.0)
    cpu_double(sampler)
    _eng._CPU_PRIMED.set(False)

    assert is_governed(None) is False
    assert not calls, f"psutil.cpu_percent must not be called when threshold is None; got {calls}"


register_law(is_governed, "test_is_governed_disabled_never_primes")


def test_is_governed_primed_latch_reads_without_warmup(cpu_double: CpuDoubleInstaller) -> None:
    """A primed latch reads the non-blocking sample without warmup.

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

    Falsified by: mis-folding status, wrapping the success arm as Fault, or dropping the OK receipt.
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
    """A spawn/timeout ``Fault`` flows through the error arm to a FAULTED Envelope.

    Falsified by: swallowing the Fault into an OK Report or re-wrapping it as Completed.
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
    """A bound Rail resolves, decodes params, and does not double-emit an already-written Envelope.

    Falsified by: re-emitting the rail Envelope or failing to resolve the registered claim/verb pair.
    """
    rail_env = envelope(fold(Claim.STATIC, "static", (receipt(("static",), 0, status=RailStatus.OK),)), claim=Claim.STATIC, verb="static")
    rail_probe.install(monkeypatch, _eng, "rail", rail_env)

    anyio.run(_eng.drive, Manual(), Rail(claim=Claim.STATIC, verb="static"), assay_root.settings)

    assert [c for c in rail_probe.calls if c[0] == "rail.run"], "the registry runner must be invoked with decoded params"
    assert captured_emits == [], f"already-emitted rail Envelope must not be re-emitted; got {len(captured_emits)}"


register_law(_eng.drive, "test_drive_rail_resolves_bind_and_emits_canned_envelope")

# --- [LAWS_DRIVE_GOVERNOR]


def test_drive_governed_skip_emits_one_skip_envelope(
    assay_root: AssayHarness, captured_emits: list[Envelope], rail_probe: RailProbe, cpu_double: CpuDoubleInstaller, monkeypatch: pytest.MonkeyPatch
) -> None:
    """A tripped CPU governor emits one SKIP Envelope and never runs the leaf.

    Falsified by: ``_emit_leaf`` running the leaf despite the governor, mis-counting the governed leaf, or dropping the ``governed: cpu>=`` note.
    """
    _eng._CPU_PRIMED.set(True)
    cpu_double(lambda *_a, **_k: 100.0)
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
    """A Sequence of OK leaves folds by severity so OK dominates the EMPTY seed.

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
    """Nested Sequence and Debounce leaves recurse through ``_emit_leaf`` exactly once each.

    Falsified by: ``_emit_leaf`` not recursing on a nested Sequence, or the Debounce arm not unwrapping to its inner action (a missing or extra fire).
    """
    rail_probe.install(monkeypatch, _eng, "run_check_async", rail_probe.ok(("p",)))

    seq = Sequence(
        actions=(Program(argv=("p", "outer")), Sequence(actions=(Program(argv=("p", "nested")),)), Debounce(action=Program(argv=("p", "wrapped"))))
    )
    anyio.run(_eng.drive, Manual(), seq, assay_root.settings)

    assert len(captured_emits) == 3, f"every nested leaf must fire exactly once; got {len(captured_emits)}"
    assert all(env.status is RailStatus.OK for env in captured_emits)


register_law(_eng.drive, "test_drive_nested_sequence_and_debounce_leaves")


def test_drive_manual_debounce_unwraps_to_inner_leaf(
    assay_root: AssayHarness, captured_emits: list[Envelope], rail_probe: RailProbe, monkeypatch: pytest.MonkeyPatch
) -> None:
    """Manual Debounce unwraps to the inner leaf without starting a worker.

    Falsified by: ``_fire`` treating a Manual Debounce as a no-op, or double-firing the inner leaf.
    """
    rail_probe.install(monkeypatch, _eng, "run_check_async", rail_probe.ok(("tool",)))

    action = Debounce(action=Program(argv=("tool",)), window_ms=500, edge=Edge.TRAILING)
    anyio.run(_eng.drive, Manual(), action, assay_root.settings)

    env = _one(captured_emits)
    assert (env.status, env.verb) == (RailStatus.OK, "program")


register_law(_eng.drive, "test_drive_manual_debounce_unwraps_to_inner_leaf")


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


register_law(_eng.drive, "test_hardened_fire_coalesces_reentrant_tick")


def test_debounce_signal_after_close_is_silent() -> None:
    """Post-close debounce signals return silently through the ClosedResourceError arm.

    Falsified by: removing the closed-resource arm and raising on a late shutdown tick. Cancelling the worker closes
    both streams before the final signal.
    """

    async def _inner(_changes: tuple[tuple[str, str], ...]) -> None:  # noqa: RUF029  # Fire protocol is async; no await needed here
        return None

    async def _run() -> None:
        signal, worker = _eng._debounce(_inner, 40, edge=Edge.TRAILING)
        async with anyio.create_task_group() as tg:
            tg.start_soon(worker)
            await anyio.lowlevel.checkpoint()
            tg.cancel_scope.cancel()
        await signal(_FIRST)

    anyio.run(_run)


register_law(_eng.drive, "test_debounce_signal_after_close_is_silent")


def test_drive_sequence_halts_on_fault(
    assay_root: AssayHarness, captured_emits: list[Envelope], rail_probe: RailProbe, monkeypatch: pytest.MonkeyPatch
) -> None:
    """A FAULTED leaf (first leaf empty argv) short-circuits ``_sequence`` before the trailing leaf runs.

    Falsified by: ``_sequence`` continuing past a FAULTED status, which would emit a second Envelope.
    """
    rail_probe.install(monkeypatch, _eng, "run_check_async", rail_probe.ok(("p",)))

    seq = Sequence(actions=(Program(argv=()), Program(argv=("p", "2"))))
    anyio.run(_eng.drive, Manual(), seq, assay_root.settings)

    assert _one(captured_emits).status is RailStatus.FAULTED
    assert rail_probe.commands == [], "the trailing leaf must not run after a FAULTED halt"


register_law(_eng.drive, "test_drive_sequence_halts_on_fault")

# --- [LAWS_DRIVE_WATCH]


def test_drive_watch_fires_per_canned_batch(
    assay_root: AssayHarness, captured_emits: list[Envelope], rail_probe: RailProbe, monkeypatch: pytest.MonkeyPatch
) -> None:
    """Watch drive fires once per canned ``awatch`` batch when no worker co-resides.

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
    """Watch filter tags project to the matching watchfiles filter instance.

    Falsified by: swapping the DEFAULT/PYTHON arms or constructing the wrong filter class.
    """
    # Valid regex keeps the law about tag dispatch, not pattern compilation.
    spec = Watch(paths=("x",), filter=filter_tag, ignore_patterns=(r"\.pyc$",))
    assert type(_eng._watch_filter(spec)).__name__ == expected_type


register_law(_eng.drive, "test_watch_filter_resolves_tag_to_watchfiles_filter")


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


register_law(_eng.drive, "test_watch_filter_default_extends_builtin_noise_suppression")


def test_watch_skips_empty_timeout_heartbeat(
    assay_root: AssayHarness, captured_emits: list[Envelope], rail_probe: RailProbe, monkeypatch: pytest.MonkeyPatch
) -> None:
    """Empty ``awatch`` timeout heartbeats do not fire the action.

    Falsified by: firing once per heartbeat window with no file event.
    """
    rail_probe.install(monkeypatch, _eng, "run_check_async", rail_probe.ok(("tool",)))
    monkeypatch.setattr(_eng, "awatch", _fake_awatch(((), _FIRST, ())))

    spec = Watch(paths=(str(assay_root.root),))
    anyio.run(_eng.drive, spec, Program(argv=("tool",)), assay_root.settings)

    assert len(captured_emits) == 1, f"only the non-empty batch fires; empty heartbeats are dropped, got {len(captured_emits)}"


register_law(_eng.drive, "test_watch_skips_empty_timeout_heartbeat")


def test_drive_watch_debounce_collapses_storm(
    assay_root: AssayHarness, captured_emits: list[Envelope], rail_probe: RailProbe, monkeypatch: pytest.MonkeyPatch
) -> None:
    """Debounced Watch co-resides the worker and collapses a two-batch storm to one fire.

    Falsified by: the debounce firing per-batch, or ``_co_resident`` failing to cancel the task group on stop (the run hangs past the timeout).
    The release delay exceeds the debounce window so the collapsed storm can fire before stop.
    """
    rail_probe.install(monkeypatch, _eng, "run_check_async", rail_probe.ok(("tool",)))
    monkeypatch.setattr(_eng, "awatch", _fake_awatch((_FIRST, _SECOND)))

    spec = Watch(paths=(str(assay_root.root),))
    action = Debounce(action=Program(argv=("tool",)), window_ms=30, edge=Edge.TRAILING)

    async def _run() -> None:
        stop = anyio.Event()

        async def _release() -> None:
            await anyio.sleep(0.12)
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
    """Schedule drive fires once per cron wakeup and stops the cron on teardown.

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
                # The second wakeup arms stop before the post-next guard.
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
    """Schedule Debounce co-resides a worker and collapses cron bursts to one fire.

    Falsified by: failing to cancel on stop or wrapping worker fire in a second single-flight cell.
    The release delay covers the burst plus quiet window so the worker can emit before cancellation.
    """
    rail_probe.install(monkeypatch, _eng, "run_check_async", rail_probe.ok(("tool",)))
    wakeups = 0

    def _crontab(*_a: object, **_k: object) -> SimpleNamespace:
        async def _next() -> None:
            nonlocal wakeups
            await anyio.lowlevel.checkpoint()
            wakeups += 1
            # Sleep after the burst so the quiet window can elapse before stop cancels.
            await (anyio.sleep(0.005) if wakeups < 3 else anyio.sleep_forever())

        return SimpleNamespace(next=_next, stop=lambda: None)

    monkeypatch.setattr(_eng, "aiocron", SimpleNamespace(crontab=_crontab))

    async def _run() -> None:
        spec = Schedule(cron="* * * * *")
        action = Debounce(action=Program(argv=("tool",)), window_ms=30, edge=Edge.TRAILING)
        stop = anyio.Event()

        async def _release() -> None:
            await anyio.sleep(0.15)
            stop.set()

        async with anyio.create_task_group() as tg:
            tg.start_soon(_release)
            await _eng._drive_schedule(spec, action, assay_root.settings, limiter=anyio.CapacityLimiter(1), stop=stop)

    anyio.run(_run)

    assert wakeups >= 1, "the cron loop must have fired at least one wakeup before stop"
    assert _one(captured_emits).status is RailStatus.OK


register_law(_eng.drive, "test_drive_schedule_debounce_co_resides_worker")


def test_quiesce_drains_until_quiet_window() -> None:
    """``_quiesce`` drains until a quiet window and returns the latest batch.

    Falsified by: ``_quiesce`` returning on the first signal (no drain) or returning a stale batch instead of the latest before the quiet window.
    The second signal arrives inside the quiet window to exercise the drain-continue arm.
    """
    latest: list[tuple[tuple[str, str], ...]] = []

    async def _run() -> None:
        send, recv = anyio.create_memory_object_stream[tuple[tuple[str, str], ...]](4)

        async def _produce() -> None:
            send.send_nowait(_FIRST)
            await anyio.sleep(0.01)
            send.send_nowait(_SECOND)

        async with anyio.create_task_group() as tg, send, recv:
            tg.start_soon(_produce)
            latest.append(await _eng._quiesce(recv, 50))

    anyio.run(_run)

    assert latest == [_SECOND], f"_quiesce must return the latest drained batch; got {latest}"


register_law(_eng.drive, "test_quiesce_drains_until_quiet_window")


def test_schedule_stops_cron_on_cancellation(monkeypatch: pytest.MonkeyPatch) -> None:
    """``_schedule`` calls ``cron.stop()`` when the enclosing scope cancels the loop.

    Falsified by: removing ``finally: cron.stop()`` and leaking the aiocron timer.
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
    """A missed tick runs one catch-up fire and resets the backlog.

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
    """A raising fire emits one FAULTED Envelope and resets the cell for the next tick.

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
        await hardened(())
        await hardened(())

    anyio.run(_run)

    assert calls == [1, 1], "the cell must reset and fire again after a fault"
    env = _one(captured_emits)
    assert (env.status, env.claim, env.verb) == (RailStatus.FAULTED, Claim.STATIC, "automation")
    assert env.error is not None
    assert "RuntimeError: boom" in env.error.message


register_law(_eng.drive, "test_hardened_fire_faults_reset_after_exception")

# --- [LAWS_DRIVE_DEBOUNCE]


@pytest.mark.parametrize("edge", [Edge.LEADING, Edge.TRAILING], ids=["leading", "trailing"])
@pytest.mark.anyio
async def test_debounce_fires_once_per_storm(*, edge: Edge) -> None:
    """Leading and trailing debounce modes fire once per storm window.

    Falsified by: firing per-signal, or the worker leaking the channel under ``filterwarnings=error`` (an unclosed-stream ResourceWarning fails).
    The size-1 channel coalesces the second signal until the quiescence window elapses.
    """
    fired: list[tuple[tuple[str, str], ...]] = []

    async def _inner(changes: tuple[tuple[str, str], ...]) -> None:  # noqa: RUF029  # Fire protocol is async; no await needed here
        fired.append(changes)

    signal, worker = _eng._debounce(_inner, 40, edge=edge)
    async with anyio.create_task_group() as tg:
        tg.start_soon(worker)
        await signal(_FIRST)
        await anyio.lowlevel.checkpoint()
        await signal(_SECOND)
        await anyio.sleep(0.07)
        tg.cancel_scope.cancel()

    assert len(fired) == 1, f"each storm window must produce exactly one fire; got {fired}"


register_law(_eng.drive, "test_debounce_fires_once_per_storm")


def test_debounce_signal_ignores_when_buffer_full() -> None:
    """Signals drop silently while the debounce worker's size-1 buffer is full.

    Falsified by: the signal raising on a full channel (no ``send_nowait`` guard), or growing past size 1 (a second signal reaches ``inner``).
    Pinning the first fire keeps the worker from draining the channel.
    """
    fired: list[tuple[tuple[str, str], ...]] = []
    pinned = anyio.Event()

    async def _inner(changes: tuple[tuple[str, str], ...]) -> None:
        fired.append(changes)
        await pinned.wait()

    async def _run() -> None:
        signal, worker = _eng._debounce(_inner, 40, edge=Edge.LEADING)
        async with anyio.create_task_group() as tg:
            tg.start_soon(worker)
            await signal(_FIRST)
            await anyio.sleep(0.02)
            await signal(_SECOND)
            await signal(_FIRST)
            await anyio.sleep(0.02)
            tg.cancel_scope.cancel()

    anyio.run(_run)

    assert fired == [_FIRST], f"buffer-full signals must drop, not fire; got {fired}"


register_law(_eng.drive, "test_debounce_signal_ignores_when_buffer_full")

# --- [LAWS_DRIVE_SETUP_FAULT]


def test_drive_manual_program_emits_single_envelope(
    assay_root: AssayHarness, captured_emits: list[Envelope], rail_probe: RailProbe, monkeypatch: pytest.MonkeyPatch
) -> None:
    """``drive(Manual(), Program(argv=('true',)), settings)`` emits exactly one Envelope.

    Falsified by: no emission or double emission across the Program and outer fault boundaries.
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
    """Setup, guard, resolve, and decode defects fold into one labelled FAULTED Envelope.

    Falsified by: dropping any boundary that converts those defects into a self-contained Fault.
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

    Falsified by: writing to stderr, writing non-JSON bytes, or omitting the newline separator.
    """
    from tests.python.tools.assay.kit import (  # noqa: PLC0415  # deferred: single-test oracle import stays off the module import path
        read_one_envelope_from_bytes,
    )

    anyio.run(_eng.drive, Manual(), Program(argv=()), assay_root.settings)

    cap = capsysbinary.readouterr()
    env = read_one_envelope_from_bytes(cap.out)
    assert env.status is RailStatus.FAULTED
    assert b"rasm.automation" not in cap.out, "structlog must not bleed onto the Envelope stdout channel"


register_law(_eng.drive, "test_drive_emits_ndjson_on_stdout")
