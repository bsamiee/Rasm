"""Registry rail invocation perf laws."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------------

from typing import Protocol, TYPE_CHECKING

import msgspec
import pytest

from tools.assay.composition.registry import rail, REGISTRY
from tools.assay.core.model import Claim, Envelope
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from collections.abc import Callable

    from tests.tools.assay.conftest import AssayHarness


# --- [TYPES] ----------------------------------------------------------------------------

pytestmark = pytest.mark.benchmark


class RegistryBenchmark(Protocol):
    """Typed surface consumed from pytest-benchmark's runtime fixture."""

    def pedantic[T](self, target: Callable[[], T], *, rounds: int, warmup_rounds: int) -> T: ...


# --- [BENCHMARKS] ----------------------------------------------------------------------


def bench_registry_static_plan(benchmark: RegistryBenchmark, assay_root: AssayHarness, capsysbinary: pytest.CaptureFixture[bytes]) -> None:
    """In-process registry rail invocation stays fast independent of CLI parsing."""
    bind = next(row for row in REGISTRY if row.claim is Claim.STATIC and row.verb == "plan")
    runner = rail(bind, assay_root.settings)

    def invoke() -> Envelope:
        env = runner(bind.params())
        rows = capsysbinary.readouterr().out.splitlines()
        assert len(rows) == 1
        assert msgspec.json.decode(rows[0], type=Envelope) == env
        return env

    env = benchmark.pedantic(invoke, rounds=10, warmup_rounds=2)

    assert env.status in RailStatus
