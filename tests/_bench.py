# --- [RUNTIME_PRELUDE] -----------------------------------------------------------------

from collections.abc import Callable
import os
from typing import TYPE_CHECKING

import msgspec
import psutil
import pytest


if TYPE_CHECKING:
    from collections.abc import Sequence

    from pytest_benchmark.fixture import BenchmarkFixture

# --- [MODELS] --------------------------------------------------------------------------


class BenchSubject(msgspec.Struct, frozen=True):
    """One registry row binding a subject callable to its benchmark geometry.

    One row = one benchmark; callers never define per-benchmark functions.
    `workload` accepts the size integer and returns the argument tuple that
    `subject` receives (as a single positional argument, built once outside the
    timed loop).  `budget_ms` is advisory context (carried in extra_info for
    regression dashboards); the hard gate lives at the pytest invocation level
    via ``--benchmark-compare-fail=mean:N%``.
    """

    label: str
    subject: Callable[[tuple[object, ...]], object]
    workload: Callable[[int], tuple[object, ...]]
    sizes: tuple[int, ...] = (100, 1_000, 10_000)
    budget_ms: float = 100.0
    rounds: int = 5
    iterations: int = 1


# --- [OPERATIONS] ----------------------------------------------------------------------


def bench_params(rows: Sequence[BenchSubject]) -> pytest.MarkDecorator:
    """Build a ``pytest.mark.parametrize`` decorator over the Cartesian (row, size) product.

    Args:
        rows: Ordered registry of benchmark subjects.

    Returns:
        A ``MarkDecorator`` whose parametrize ids are ``"{label}-{size}"`` so
        benchmark storage keys are human-readable and stable across runs.
    """
    cases = [(row, size) for row in rows for size in row.sizes]
    ids = [f"{row.label}-{size}" for row, size in cases]
    return pytest.mark.parametrize("row,size", cases, ids=ids)


def run_bench(benchmark: BenchmarkFixture, row: BenchSubject, size: int) -> object:
    """Drive ``benchmark.pedantic`` with real psutil rss/cpu deltas attached to extra_info.

    Captures process RSS and non-blocking CPU percent immediately before and after
    the pedantic call so ``benchmark.extra_info`` carries true deltas rather than
    constants.  ``benchmark.group`` is set to ``row.label`` so
    ``--benchmark-compare-fail=mean:N%`` compares runs within the same subject
    across size variants.

    Args:
        benchmark: The ``pytest-benchmark`` ``BenchmarkFixture`` injected by pytest.
        row: Registry row describing the subject, workload factory, and timing policy.
        size: The concrete workload size drawn from ``row.sizes``.

    Returns:
        The value returned by the final pedantic round (passthrough for callers that
        want spot-check assertions; benchmarks must not depend on this).
    """
    proc = psutil.Process(os.getpid())

    # Non-blocking CPU sample primes the interval counter; the post-call sample
    # measures the fraction spent in user+sys during the pedantic window.
    proc.cpu_percent(interval=None)
    rss_before = proc.memory_info().rss

    # workload runs once here, OUTSIDE the timed loop; pedantic then measures only subject(payload).
    payload = row.workload(size)
    # warmup_rounds=1: one un-timed warm pass before the measured rounds so JIT caches/branch predictors are primed.
    # pytest-benchmark ships no stub for BenchmarkFixture.pedantic; ty resolves it, mypy needs the inline ignore.
    result = benchmark.pedantic(row.subject, args=(payload,), rounds=row.rounds, iterations=row.iterations, warmup_rounds=1)  # type: ignore[no-untyped-call]

    rss_after = proc.memory_info().rss
    cpu_delta = proc.cpu_percent(interval=None)

    benchmark.extra_info["rss_delta_bytes"] = rss_after - rss_before
    benchmark.extra_info["rss_after_bytes"] = rss_after
    benchmark.extra_info["cpu_percent_delta"] = cpu_delta
    benchmark.extra_info["budget_ms"] = row.budget_ms
    benchmark.extra_info["size"] = size
    benchmark.group = row.label

    return result


# --- [EXPORTS] -------------------------------------------------------------------------

__all__ = ["BenchSubject", "run_bench", "bench_params"]
