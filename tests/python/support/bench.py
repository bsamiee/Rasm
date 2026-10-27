"""Benchmark cases with time budgets over the pytest-benchmark fixture."""

from collections.abc import Callable
from datetime import timedelta
from typing import Protocol
lazy from collections.abc import Iterable

import msgspec
from pytest_benchmark.stats import Stats
lazy from expression import Option
lazy import psutil
lazy import pytest
lazy from pytest_benchmark.fixture import BenchmarkFixture

# --- [TYPES] ----------------------------------------------------------------------------


class _Subject[*Ts](Protocol):
    __qualname__: str

    def __call__(self, *args: *Ts) -> object: ...


class _Sized(Protocol):
    @property
    def sizes(self) -> tuple[int, ...]: ...


# --- [MODELS] ---------------------------------------------------------------------------


class BenchmarkCase[*Ts](msgspec.Struct, frozen=True, kw_only=True):
    """Subject under test, its argument workload per size, sizes, and time budget over one timing statistic."""

    subject: _Subject[*Ts]
    workload: Callable[[int], tuple[*Ts]]
    sizes: tuple[int, ...] = (100, 1_000, 10_000)
    budget: timedelta = timedelta(milliseconds=100)
    statistic: Callable[[Stats], float] = lambda stats: stats.median

    def measure(self, benchmark: BenchmarkFixture, size: int) -> object:
        """Time ``subject`` over the workload at ``size``, record resident memory growth, and fail a timed run past the budget."""
        process, arguments, millisecond = psutil.Process(), self.workload(size), timedelta(milliseconds=1)
        benchmark.group = self.subject.__qualname__
        rss_before = process.memory_info().rss
        result = benchmark(self.subject, *arguments)
        observed = Option.of_optional(benchmark.stats).map(lambda metadata: timedelta(seconds=self.statistic(metadata.stats)))
        benchmark.extra_info.update(
            rss_delta_bytes=process.memory_info().rss - rss_before, budget_ms=self.budget / millisecond, observed_ms=observed.map(lambda time: time / millisecond).to_optional(), size=size
        )
        match observed:
            case Option(tag="some", some=time) if time > self.budget:
                pytest.fail(f"{time} exceeds budget {self.budget}")
        return result


# --- [OPERATIONS] -----------------------------------------------------------------------


def benchmarks(cases: Iterable[_Sized]) -> pytest.MarkDecorator:
    """Parametrize ``case`` and ``size`` over every case at each of its sizes, identified by subject and size."""
    return pytest.mark.parametrize("case,size", [(case, size) for case in cases for size in case.sizes], ids=lambda value: value.subject.__qualname__ if isinstance(value, BenchmarkCase) else None)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BenchmarkCase", "benchmarks"]
