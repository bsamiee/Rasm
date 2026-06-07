"""CLI cold-start and in-process command perf laws."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------------

from typing import Protocol, TYPE_CHECKING

import pytest

from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from collections.abc import Callable

    from tests.tools.assay.conftest import VerbRunner
    from tools.assay.core.model import Envelope


# --- [TYPES] ----------------------------------------------------------------------------

pytestmark = pytest.mark.benchmark


class CliBenchmark(Protocol):
    """Typed surface consumed from pytest-benchmark's runtime fixture."""

    def pedantic[T](self, target: Callable[[], T], *, rounds: int, warmup_rounds: int) -> T: ...


# --- [BENCHMARKS] ----------------------------------------------------------------------


def bench_cli_inprocess_self_test(benchmark: CliBenchmark, cli: VerbRunner) -> None:
    """In-process CLI invocation keeps the registry path fast while preserving one Envelope stdout."""

    def invoke() -> tuple[Envelope, int]:
        return cli("self-test")

    env, code = benchmark.pedantic(invoke, rounds=10, warmup_rounds=2)

    assert code in {0, 1}
    assert isinstance(env.status, RailStatus)


def bench_cli_cold_start_self_test(benchmark: CliBenchmark, cli: VerbRunner) -> None:
    """Cold subprocess CLI invocation stays executable as a benchmarked command boundary."""

    def invoke() -> tuple[Envelope, int]:
        return cli("self-test", isolate=True)

    env, code = benchmark.pedantic(invoke, rounds=1, warmup_rounds=0)

    assert code in {0, 1}
    assert isinstance(env.status, RailStatus)
