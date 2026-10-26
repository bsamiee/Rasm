"""Benchmark cases with time budgets over the pytest-benchmark fixture."""

from collections.abc import Callable
from typing import Protocol
lazy from collections.abc import Iterable

import msgspec
from pytest_benchmark.stats import Stats
lazy import psutil
lazy import pytest
lazy from pytest_benchmark.fixture import BenchmarkFixture

# --- [TYPES] ----------------------------------------------------------------------------


class _Subject[*Ts](Protocol):
    __qualname__: str

    def __call__(self, *args: *Ts) -> object: ...


class _Parametrized(Protocol):
    @property
    def parameters(self) -> tuple[object, ...]: ...


# --- [MODELS] ---------------------------------------------------------------------------


class BenchmarkCase[*Ts](msgspec.Struct, frozen=True, kw_only=True):
    """Subject under test, its argument workload per size, sizes, and millisecond budget over one timing statistic."""

    subject: _Subject[*Ts]
    workload: Callable[[int], tuple[*Ts]]
    sizes: tuple[int, ...] = (100, 1_000, 10_000)
    budget_ms: float = 100.0
    statistic: Callable[[Stats], float] = lambda stats: stats.median

    @property
    def parameters(self) -> tuple[object, ...]:
        """One ``(case, size)`` parameter per size, identified by subject and size."""
        return tuple(pytest.param(self, size, id=f"{self.subject.__qualname__}-{size}") for size in self.sizes)

    def measure(self, benchmark: BenchmarkFixture, size: int) -> object:
        """Time ``subject`` over the workload at ``size``, record resident memory growth, and fail past the budget."""
        process, arguments = psutil.Process(), self.workload(size)
        benchmark.group = self.subject.__qualname__
        rss_before = process.memory_info().rss
        result = benchmark(self.subject, *arguments)
        assert benchmark.stats is not None
        observed_ms = self.statistic(benchmark.stats.stats) * 1000.0
        benchmark.extra_info.update(rss_delta_bytes=process.memory_info().rss - rss_before, budget_ms=self.budget_ms, observed_ms=observed_ms, size=size)
        assert observed_ms <= self.budget_ms, f"{observed_ms:.4f}ms exceeds budget {self.budget_ms:.4f}ms"
        return result


# --- [OPERATIONS] -----------------------------------------------------------------------


def benchmarks(cases: Iterable[_Parametrized]) -> pytest.MarkDecorator:
    """Parametrize ``case`` and ``size`` over every case at each of its sizes."""
    return pytest.mark.parametrize("case,size", [*case.parameters for case in cases])


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BenchmarkCase", "benchmarks"]
