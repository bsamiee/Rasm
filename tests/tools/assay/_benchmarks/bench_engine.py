"""Engine spawn, fan-out, and lease perf laws."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------------

import sys
from typing import Protocol, TYPE_CHECKING

from expression import Ok, Result
import pytest

from tools.assay.core.engine import exclusive_lease, fan_out, leased, run_check
from tools.assay.core.model import Check, Claim, Input, Language, Mode, Runner, Tool
from tools.assay.core.routing import Routed, Scope
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from collections.abc import Callable

    from tests.tools.assay.conftest import AssayHarness
    from tools.assay.core.model import Fault


# --- [TYPES] ----------------------------------------------------------------------------

pytestmark = pytest.mark.benchmark


class EngineBenchmark(Protocol):
    """Typed surface consumed from pytest-benchmark's runtime fixture."""

    def pedantic[T](self, target: Callable[[], T], *, rounds: int, warmup_rounds: int) -> T: ...


# --- [CONSTANTS] -----------------------------------------------------------------------

_NOOP_TOOL = Tool(
    name="bench-noop",
    runner=Runner.DIRECT,
    command=(sys.executable, "-c", ""),
    input=Input.NONE,
    language=Language.PYTHON,
    claim=Claim.STATIC,
    mode=Mode.CHECK,
)
_ROUTED = Routed(language=Language.PYTHON, scope=Scope.FULL)


# --- [BENCHMARKS] ----------------------------------------------------------------------


def bench_local_engine_spawn(benchmark: EngineBenchmark, assay_root: AssayHarness) -> None:
    """Local engine spawn completes on the Completed channel for process exits."""
    check = Check(tool=_NOOP_TOOL)

    def invoke() -> Result[object, Fault]:
        return run_check(check, settings=assay_root.settings, scope=None, routed=_ROUTED)

    result = benchmark.pedantic(invoke, rounds=10, warmup_rounds=2)

    assert result.is_ok()


def bench_fan_out_eight_noop_processes(benchmark: EngineBenchmark, assay_root: AssayHarness) -> None:
    """Fan-out preserves ordering while scaling an eight-process batch."""
    checks = tuple(Check(tool=_NOOP_TOOL) for _ in range(8))

    def invoke() -> tuple[Result[object, Fault], ...]:
        return fan_out(checks, settings=assay_root.settings, scope=None, routed=_ROUTED)

    results = benchmark.pedantic(invoke, rounds=5, warmup_rounds=1)

    assert len(results) == len(checks)
    assert all(row.is_ok() for row in results)


def bench_lock_contention_busy(benchmark: EngineBenchmark, assay_root: AssayHarness) -> None:
    """Busy leases fail fast when another holder owns the resource.

    Raises:
        AssertionError: If the contender unexpectedly acquires the held lease.
    """

    def action(_held: object) -> Result[str, Fault]:
        return Ok("acquired")

    def invoke() -> Result[str, Fault]:
        return leased("bench-lock", action, settings=assay_root.settings, run_id="blocked")

    with exclusive_lease("bench-lock", "holder", settings=assay_root.settings):
        result = benchmark.pedantic(invoke, rounds=100, warmup_rounds=5)

    match result:
        case Result(tag="error", error=fault):
            assert fault.status is RailStatus.BUSY
        case _:
            raise AssertionError("expected busy lease result")
