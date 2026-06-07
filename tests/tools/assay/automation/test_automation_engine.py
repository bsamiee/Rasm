"""Automation engine laws."""

import threading
from types import SimpleNamespace
from typing import TYPE_CHECKING

import anyio
import anyio.lowlevel
import msgspec

from tools.assay.automation import engine as automation_engine
from tools.assay.automation.engine import _emit_leaf, _governed, _hardened_fire, _schedule, drive  # noqa: PLC2701
from tools.assay.automation.model import Manual, Program, Rail, Schedule, Watch
from tools.assay.core.model import Claim, Envelope, envelope, fold
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    import pytest

    from tests.tools.assay.conftest import AssayHarness


def test_cpu_governor_threshold(monkeypatch: pytest.MonkeyPatch) -> None:
    """CPU threshold skips a fire once psutil reports utilization at or above the ceiling."""
    monkeypatch.setattr("tools.assay.automation.engine.psutil.cpu_percent", lambda _interval: 91.0)

    assert anyio.run(_governed, 0.9) is True
    assert anyio.run(_governed, 0.95) is False
    assert anyio.run(_governed, None) is False


def test_watch_setup_fault_emits_ndjson_envelope(assay_root: AssayHarness, capsysbinary: pytest.CaptureFixture[bytes]) -> None:
    """Watch setup errors emit a bounded fault envelope instead of escaping."""
    trigger = Watch(paths=(str(assay_root.root),), filter="unknown")
    action = Program(argv=("true",))

    anyio.run(drive, trigger, action, assay_root.settings)
    rows = capsysbinary.readouterr().out.splitlines()
    env = msgspec.json.decode(rows[0], type=Envelope)

    assert len(rows) == 1
    assert env.status is RailStatus.FAULTED
    assert env.error is not None
    assert "unknown watch filter" in env.error.message


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
