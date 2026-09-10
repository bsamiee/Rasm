"""Benchmark cases with absolute performance budgets over the pytest-benchmark fixture."""

# --- [IMPORTS] --------------------------------------------------------------------------

from collections.abc import Callable
import os
from typing import Literal, TYPE_CHECKING

import msgspec
import psutil
import pytest

if TYPE_CHECKING:
    from collections.abc import Sequence

    from pytest_benchmark.fixture import BenchmarkFixture

# --- [MODELS] ---------------------------------------------------------------------------


class BenchmarkCase(msgspec.Struct, frozen=True):
    """Benchmark subject, workload generator, and performance budget.

    ``workload(size)`` builds the tuple passed to ``subject``, ``budget_ms`` is an absolute ceiling over ``budget_statistic``.
    ``--benchmark-compare-fail`` holds a relative ceiling against a stored run.

    Attributes:
        enforce_budget: False records timings without asserting the budget.
        budget_statistic: Statistic compared with the budget, ``mean`` is tail-sensitive.
        fresh_per_round: Rebuilds a mutating or consuming workload before each of ``rounds`` rounds after ``warmup_rounds``.
        rounds: Measured rounds of a fresh-per-round case, the benchmark options own rounds and warmup otherwise.
    """

    label: str
    subject: Callable[[tuple[object, ...]], object]
    workload: Callable[[int], tuple[object, ...]]
    sizes: tuple[int, ...] = (100, 1_000, 10_000)
    budget_ms: float = 100.0
    enforce_budget: bool = True
    budget_statistic: Literal["min", "median", "mean"] = "median"
    fresh_per_round: bool = False
    rounds: int = 5
    warmup_rounds: int = 1


# --- [OPERATIONS] -----------------------------------------------------------------------


def benchmark_parameters(cases: Sequence[BenchmarkCase]) -> pytest.MarkDecorator:
    """Build the ``(case, size)`` parametrization with stable ``"{label}-{size}"`` ids."""
    parameters = [(case, size) for case in cases for size in case.sizes]
    ids = [f"{case.label}-{size}" for case, size in parameters]
    return pytest.mark.parametrize("case,size", parameters, ids=ids)


def run_benchmark(benchmark: BenchmarkFixture, case: BenchmarkCase, size: int) -> object:
    """Measure a benchmark case, record its resident memory growth, and assert its performance budget."""
    process = psutil.Process(os.getpid())
    arguments = case.workload(size)
    benchmark.group = case.label
    rss_before = process.memory_info().rss
    result = (
        benchmark.pedantic(  # type: ignore[no-untyped-call]
            case.subject, setup=lambda: ((case.workload(size),), {}), rounds=case.rounds, warmup_rounds=case.warmup_rounds
        )
        if case.fresh_per_round
        else benchmark(case.subject, arguments)
    )
    assert benchmark.stats is not None
    observed_ms = getattr(benchmark.stats.stats, case.budget_statistic) * 1000.0
    benchmark.extra_info.update(rss_delta_bytes=process.memory_info().rss - rss_before, budget_ms=case.budget_ms, observed_ms=observed_ms, size=size)
    if case.enforce_budget and observed_ms > case.budget_ms:
        pytest.fail(f"{case.label}-{size}: {case.budget_statistic}={observed_ms:.4f}ms exceeds budget {case.budget_ms:.4f}ms")
    return result


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BenchmarkCase", "benchmark_parameters", "run_benchmark"]
