"""Automation engine laws."""

from typing import TYPE_CHECKING

import anyio
import anyio.lowlevel
import msgspec
import pytest

from tools.assay.automation import engine as automation_engine
from tools.assay.automation.engine import _governed, _hardened_fire, drive  # noqa: PLC2701
from tools.assay.automation.model import Manual, Program, Watch
from tools.assay.core.model import Envelope
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from tests.tools.assay.conftest import AssayHarness


def test_cpu_governor_threshold(monkeypatch: pytest.MonkeyPatch) -> None:
    """CPU threshold skips a fire once psutil reports utilization at or above the ceiling."""
    monkeypatch.setattr("tools.assay.automation.engine.psutil.cpu_percent", lambda _interval: 91.0)

    assert _governed(0.9) is True
    assert _governed(0.95) is False
    assert _governed(None) is False


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


def test_hardened_fire_resets_after_failure(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Schedule coalescing does not wedge if the fired action raises."""
    monkeypatch.setattr(automation_engine, "_JITTER_MS", 1)
    calls = 0

    async def fire() -> None:
        nonlocal calls
        await anyio.lowlevel.checkpoint()
        calls += 1
        if calls == 1:
            raise RuntimeError("boom")

    async def run() -> None:
        hardened = _hardened_fire(fire, assay_root.settings)
        with pytest.raises(RuntimeError, match="boom"):
            await hardened()
        await hardened()

    anyio.run(run)
    assert calls == 2
