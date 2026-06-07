"""Automation fire, hardening, and debounce perf laws."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------------

import sys
from typing import Protocol, TYPE_CHECKING

import anyio
import anyio.lowlevel
import pytest

from tools.assay.automation import engine as automation_engine
from tools.assay.automation.engine import _debounce, _fire, _hardened_fire  # noqa: PLC2701
from tools.assay.automation.model import Program


if TYPE_CHECKING:
    from collections.abc import Callable

    from tests.tools.assay.conftest import AssayHarness
    from tools.assay.automation.engine import ChangeBatch


# --- [TYPES] ----------------------------------------------------------------------------

pytestmark = pytest.mark.benchmark


class AutomationBenchmark(Protocol):
    """Typed surface consumed from pytest-benchmark's runtime fixture."""

    def pedantic[T](self, target: Callable[[], T], *, rounds: int, warmup_rounds: int) -> T: ...


# --- [CONSTANTS] -----------------------------------------------------------------------

_NO_CHANGES: tuple[tuple[str, str], ...] = ()


# --- [BENCHMARKS] ----------------------------------------------------------------------


def bench_automation_program_fire(benchmark: AutomationBenchmark, assay_root: AssayHarness, capsysbinary: pytest.CaptureFixture[bytes]) -> None:
    """Automation program fire includes engine dispatch and NDJSON Envelope framing."""
    fire = _fire(Program(argv=(sys.executable, "-c", "")), assay_root.settings, limiter=anyio.CapacityLimiter(1), cpu_threshold=None)

    async def run() -> bytes:
        await fire(_NO_CHANGES)
        rows = capsysbinary.readouterr().out.splitlines()
        assert len(rows) == 1
        return rows[0]

    result = benchmark.pedantic(lambda: anyio.run(run), rounds=5, warmup_rounds=1)

    assert result


def bench_automation_hardened_fire(benchmark: AutomationBenchmark, assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Hardened fire benchmarks scheduling jitter separately from direct dispatch."""
    monkeypatch.setattr(automation_engine, "_JITTER_MS", 1)

    async def leaf(_changes: ChangeBatch) -> None:
        await anyio.lowlevel.checkpoint()

    hardened = _hardened_fire(leaf, assay_root.settings)

    result = benchmark.pedantic(lambda: anyio.run(hardened, _NO_CHANGES), rounds=10, warmup_rounds=2)

    assert result is None


def bench_automation_debounce_path(benchmark: AutomationBenchmark) -> None:
    """Debounce coalesces a burst through the same channel-backed action path used by watch triggers."""
    calls = 0

    async def leaf(_changes: ChangeBatch) -> None:
        nonlocal calls
        await anyio.lowlevel.checkpoint()
        calls += 1

    async def run() -> int:
        nonlocal calls
        calls = 0
        fire, worker = _debounce(leaf, 1, collapse=True)
        async with anyio.create_task_group() as tg:
            tg.start_soon(worker)
            await fire(_NO_CHANGES)
            await fire(_NO_CHANGES)
            await anyio.sleep(0.01)
            tg.cancel_scope.cancel()
        return calls

    result = benchmark.pedantic(lambda: anyio.run(run), rounds=20, warmup_rounds=2)

    assert result == 1
