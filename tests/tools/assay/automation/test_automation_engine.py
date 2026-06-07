"""Automation engine laws."""

import threading
from types import SimpleNamespace
from typing import TYPE_CHECKING

import anyio
import anyio.lowlevel
import msgspec
import pytest

from tests.tools.assay.conftest import read_one_envelope
from tools.assay.automation import engine as automation_engine
from tools.assay.automation.engine import (  # noqa: TC001  # runtime annotation — type alias used in variable and parameter annotations
    _debounce,  # noqa: PLC2701  # testing internals
    _emit_leaf,  # noqa: PLC2701  # testing internals
    _governed,  # noqa: PLC2701  # testing internals
    _hardened_fire,  # noqa: PLC2701  # testing internals
    _schedule,  # noqa: PLC2701  # testing internals
    ChangeBatch,
    drive,
)
from tools.assay.automation.model import Manual, Program, Rail, Schedule, Trigger, Watch, WatchFilter
from tools.assay.core.model import Claim, Envelope, envelope, fold
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from tests.tools.assay.conftest import AssayHarness


# --- [CONSTANTS] ------------------------------------------------------------------------

_FIRST: ChangeBatch = (("added", "first.txt"),)
_SECOND: ChangeBatch = (("modified", "second.txt"),)


@pytest.mark.parametrize(
    "threshold,cpu,skip",
    [
        (0.9, 91.0, True),    # 91 >= 90
        (0.95, 91.0, False),  # 91 < 95
        (None, 91.0, False),  # None disables the governor
        (0.0, 0.0, True),     # 0 >= 0
        (1.0, 100.0, True),   # 100 >= 100
        (1.0, 99.9, False),   # 99.9 < 100
        (0.91, 91.0, True),   # 91 >= 91 (exact boundary)
    ],
)
def test_cpu_governor_boundary(threshold: float | None, cpu: float, *, skip: bool, monkeypatch: pytest.MonkeyPatch) -> None:
    """`_governed` skips iff the psutil sample is at or above `threshold * 100` (sync, warmed, non-blocking)."""
    # tolerate the warmup(0.1) + read(interval=None) calls
    monkeypatch.setattr("tools.assay.automation.engine.psutil.cpu_percent", lambda *_a, **_k: cpu)
    monkeypatch.setattr("tools.assay.automation.engine._CPU_PRIMED", [True])  # skip the one-shot warmup so the patched sample is read directly

    assert _governed(threshold) is skip


@pytest.mark.parametrize(
    "trigger,fragment",
    [
        # re.error now caught by drive except*
        (Watch(paths=("{root}",), filter=WatchFilter.DEFAULT, ignore_patterns=(r"*.pyc",)), "nothing to repeat"),
        (Schedule(cron="* * * * *", timezone="Invalid/Zone"), "No time zone"),
    ],
)
def test_drive_setup_fault_emits_one_faulted_envelope(
    trigger: Watch | Schedule, fragment: str, assay_root: AssayHarness, capsysbinary: pytest.CaptureFixture[bytes]
) -> None:
    """Trigger setup defects fold into one FAULTED envelope labelled by the action, not a task-group escape."""
    bound = msgspec.structs.replace(trigger, paths=(str(assay_root.root),)) if isinstance(trigger, Watch) else trigger
    anyio.run(drive, bound, Program(argv=("true",)), assay_root.settings)
    env = read_one_envelope(capsysbinary)

    assert env.status is RailStatus.FAULTED
    assert env.error is not None
    assert fragment in env.error.message
    assert env.claim is Claim.STATIC
    assert env.verb == "program"  # pins _label(Program) dispatch


def test_empty_program_emits_fault_envelope(assay_root: AssayHarness, capsysbinary: pytest.CaptureFixture[bytes]) -> None:
    """Directly constructed invalid Program actions still stay on the automation fault rail."""
    anyio.run(drive, Manual(), Program(argv=()), assay_root.settings)
    rows = capsysbinary.readouterr().out.splitlines()
    env = msgspec.json.decode(rows[0], type=Envelope)

    assert len(rows) == 1
    assert env.status is RailStatus.FAULTED
    assert env.error is not None
    assert "non-empty" in env.error.message


def test_rail_action_runs_registry_in_worker_thread(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Automation rail leaves offload the synchronous registry runner instead of nesting AnyIO in the loop thread."""
    loop_thread = threading.get_ident()
    seen: list[int] = []

    def fake_outcome(_action: Rail, _settings: object) -> tuple[Envelope, bool]:
        seen.append(threading.get_ident())
        return envelope(fold(Claim.STATIC, "plan", ()), claim=Claim.STATIC, verb="plan"), True

    monkeypatch.setattr(automation_engine, "_rail_outcome", fake_outcome)

    status = anyio.run(_emit_leaf, Rail(claim=Claim.STATIC, verb="plan"), assay_root.settings, anyio.CapacityLimiter(1), None)

    assert status is RailStatus.EMPTY
    assert seen
    assert seen[0] != loop_thread


def test_rail_setup_fault_uses_leaf_emitter(assay_root: AssayHarness, capsysbinary: pytest.CaptureFixture[bytes]) -> None:
    """Automation rail binding faults return to the leaf emitter instead of writing inside setup helpers."""
    status = anyio.run(_emit_leaf, Rail(claim=Claim.STATIC, verb="missing"), assay_root.settings, anyio.CapacityLimiter(1), None)
    rows = capsysbinary.readouterr().out.splitlines()
    env = msgspec.json.decode(rows[0], type=Envelope)

    assert status is RailStatus.FAULTED
    assert len(rows) == 1
    assert env.error is not None
    assert "unbound rail" in env.error.message


def test_schedule_stops_cron_on_cancellation(monkeypatch: pytest.MonkeyPatch) -> None:
    """Schedule teardown calls cron.stop when the task group cancels the trigger loop."""
    stopped: list[bool] = []

    async def next_fire() -> None:
        await anyio.sleep_forever()

    def stop() -> None:
        stopped.append(True)

    monkeypatch.setattr("tools.assay.automation.engine.aiocron.crontab", lambda *_args, **_kwargs: SimpleNamespace(next=next_fire, stop=stop))

    async def run() -> None:
        stop = anyio.Event()

        async def fire(_changes: tuple[tuple[str, str], ...]) -> None:
            pass

        with anyio.move_on_after(0.01):
            await _schedule(Schedule(cron="* * * * *"), fire, stop)

    anyio.run(run)

    assert stopped == [True]


@pytest.mark.anyio
@pytest.mark.parametrize("collapse", [False, True])
async def test_debounce_collapse_modes(*, collapse: bool) -> None:
    """Both leading (collapse=False) and trailing (collapse=True) modes fire exactly once per storm window."""
    fired: list[ChangeBatch] = []

    async def inner(changes: ChangeBatch) -> None:  # noqa: RUF029  # must satisfy Fire = Callable[[ChangeBatch], Coroutine[...]] for _debounce
        fired.append(changes)

    fire, worker = _debounce(inner, 50, collapse=collapse)
    async with anyio.create_task_group() as tg:
        tg.start_soon(worker)
        await fire(_FIRST)
        await anyio.lowlevel.checkpoint()
        await fire(_SECOND)  # coalesced: the size-1 channel already holds _FIRST
        await anyio.sleep(0.07)  # > 50ms quiescence window
        tg.cancel_scope.cancel()

    assert len(fired) == 1


def test_watch_filter_unknown_rejected_at_decode() -> None:
    """An unknown filter tag now fails at the wire boundary (WatchFilter StrEnum) — validation moved to decode."""
    blob = msgspec.json.encode({"kind": "watch", "paths": ["x"], "filter": "unknown"})
    with pytest.raises(msgspec.ValidationError, match="unknown"):
        msgspec.json.decode(blob, type=Trigger)


def test_hardened_fire_resets_after_failure(
    assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch, capsysbinary: pytest.CaptureFixture[bytes]
) -> None:
    """Schedule coalescing emits one fault envelope and does not wedge if the fired action raises."""
    monkeypatch.setattr(automation_engine, "_JITTER_MS", 1)
    calls = 0

    async def fire(_changes: tuple[tuple[str, str], ...]) -> None:
        nonlocal calls
        await anyio.lowlevel.checkpoint()
        calls += 1
        if calls == 1:
            raise RuntimeError("boom")

    async def run() -> None:
        hardened = _hardened_fire(fire, assay_root.settings)
        await hardened(())
        await hardened(())

    anyio.run(run)
    rows = capsysbinary.readouterr().out.splitlines()
    env = msgspec.json.decode(rows[0], type=Envelope)

    assert calls == 2
    assert len(rows) == 1
    assert env.status is RailStatus.FAULTED
    assert env.error is not None
    assert "RuntimeError: boom" in env.error.message
