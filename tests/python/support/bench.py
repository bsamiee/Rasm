"""Benchmark cases with absolute performance budgets over the pytest-benchmark fixture."""

# --- [IMPORTS] --------------------------------------------------------------------------

from collections.abc import Callable, Sequence
import math
from typing import Literal

import msgspec
import psutil
import pytest
from pytest_benchmark.fixture import BenchmarkFixture

# --- [MODELS] ---------------------------------------------------------------------------


class BenchmarkCase(msgspec.Struct, frozen=True):
    """Benchmark subject, workload generator, and performance budget.

    ``workload(size)`` builds the tuple passed to ``subject``, ``budget_ms`` is an absolute ceiling over ``budget_statistic`` and ``math.inf`` records timings without one.
    ``--benchmark-compare-fail`` holds a relative ceiling against a stored run.

    Attributes:
        budget_statistic: Statistic compared with the budget, ``mean`` is tail-sensitive.
        fresh_rounds: Measured rounds with the workload rebuilt before each, for a mutating or consuming subject, ``None`` lets the benchmark options own the rounds.
    """

    label: str
    subject: Callable[[tuple[object, ...]], object]
    workload: Callable[[int], tuple[object, ...]]
    sizes: tuple[int, ...] = (100, 1_000, 10_000)
    budget_ms: float = 100.0
    budget_statistic: Literal["min", "median", "mean"] = "median"
    fresh_rounds: int | None = None


# --- [OPERATIONS] -----------------------------------------------------------------------


def benchmark_parameters(cases: Sequence[BenchmarkCase]) -> pytest.MarkDecorator:
    """Build the ``(case, size)`` parametrization with stable ``"{label}-{size}"`` ids."""
    parameters = [(case, size) for case in cases for size in case.sizes]
    return pytest.mark.parametrize("case,size", parameters, ids=[f"{case.label}-{size}" for case, size in parameters])


def run_benchmark(benchmark: BenchmarkFixture, case: BenchmarkCase, size: int) -> object:
    """Measure a benchmark case, record its resident memory growth, and assert its performance budget."""
    process = psutil.Process()
    benchmark.group = case.label
    rss_before = process.memory_info().rss
    result = (
        benchmark(case.subject, case.workload(size)) if case.fresh_rounds is None else benchmark.pedantic(case.subject, setup=lambda: ((case.workload(size),), {}), rounds=case.fresh_rounds)  # type: ignore[no-untyped-call]
    )
    assert benchmark.stats is not None
    observed_ms = getattr(benchmark.stats.stats, case.budget_statistic) * 1000.0
    benchmark.extra_info.update(rss_delta_bytes=process.memory_info().rss - rss_before, budget_ms=case.budget_ms, observed_ms=observed_ms, size=size)
    if math.isfinite(case.budget_ms) and observed_ms > case.budget_ms:
        pytest.fail(f"{case.label}-{size}: {case.budget_statistic}={observed_ms:.4f}ms exceeds budget {case.budget_ms:.4f}ms")
    return result


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BenchmarkCase", "benchmark_parameters", "run_benchmark"]
