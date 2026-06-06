"""Fold micro + codec perf laws.

Perf mandate: fold(1000 Completed) mean < 1ms (rounds=100, warmup_rounds=5).
Linearity: fold(2N) / fold(N) mean ratio <= 2.5 (benchmark.pedantic, rounds=200 each).
Codec: 100-row Report Envelope; encode rounds=500, warmup_rounds=10. len(result) > 0.
"""

# --- [IMPORTS] ------------------------------------------------------------------------

from statistics import median
import time
from typing import overload, Protocol, TYPE_CHECKING

import msgspec
import pytest

from tools.assay.core.model import Claim, envelope, fold, receipt
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from collections.abc import Callable

    from tools.assay.core.model import Completed, Envelope, Report


# --- [TYPES] ----------------------------------------------------------------------------

pytestmark = pytest.mark.benchmark


class ModelBenchmark(Protocol):
    """Typed surface consumed from pytest-benchmark's runtime fixture."""

    @overload
    def pedantic(
        self,
        target: Callable[[Claim, str, tuple[Completed, ...]], Report],
        *,
        args: tuple[Claim, str, tuple[Completed, ...]],
        rounds: int,
        warmup_rounds: int,
    ) -> Report: ...

    @overload
    def pedantic(self, target: Callable[[Envelope], bytes], *, args: tuple[Envelope], rounds: int, warmup_rounds: int) -> bytes: ...

    @overload
    def pedantic(self, target: Callable[[], float], *, rounds: int, warmup_rounds: int) -> float: ...


# --- [CONSTANTS] -----------------------------------------------------------------------

_ENCODER = msgspec.json.Encoder(order="deterministic")
_SMALL_N = 100
_LARGE_N = 200
_LINEARITY_LOOPS = 64
_LINEARITY_SAMPLES = 5
_OUTCOMES_SMALL = tuple(receipt(("tool",), 0, status=RailStatus.OK) for _ in range(_SMALL_N))
_OUTCOMES_LARGE = tuple(receipt(("tool",), 0, status=RailStatus.OK) for _ in range(_LARGE_N))


# --- [BENCHMARKS] ----------------------------------------------------------------------


def bench_fold_throughput(benchmark: ModelBenchmark) -> None:
    """fold(1000 Completed) mean < 1ms — O(N) fold is the core hot path."""
    outcomes = tuple(receipt(("t",), 0, status=RailStatus.OK) for _ in range(1000))
    result = benchmark.pedantic(fold, args=(Claim.STATIC, "check", outcomes), rounds=100, warmup_rounds=5)
    assert result.status is RailStatus.OK


def bench_fold_linearity(benchmark: ModelBenchmark) -> None:
    """fold(2N) / fold(N) ratio <= 2.5 via one benchmark fixture call."""

    def sample_ratio() -> float:
        small = fold(Claim.STATIC, "check", _OUTCOMES_SMALL)
        large = fold(Claim.STATIC, "check", _OUTCOMES_LARGE)
        ratios = []
        for _ in range(_LINEARITY_SAMPLES):
            small_start = time.perf_counter()
            small = fold(Claim.STATIC, "check", _OUTCOMES_SMALL)
            for _ in range(_LINEARITY_LOOPS):
                small = fold(Claim.STATIC, "check", _OUTCOMES_SMALL)
            small_t = time.perf_counter() - small_start
            large_start = time.perf_counter()
            large = fold(Claim.STATIC, "check", _OUTCOMES_LARGE)
            for _ in range(_LINEARITY_LOOPS):
                large = fold(Claim.STATIC, "check", _OUTCOMES_LARGE)
            large_t = time.perf_counter() - large_start
            ratios.append(large_t / small_t if small_t > 0 else 1.0)
        assert small.status is RailStatus.OK
        assert large.status is RailStatus.OK
        ratio = median(ratios)
        assert ratio <= 2.5, f"fold linearity violated: fold({_LARGE_N}) / fold({_SMALL_N}) = {ratio:.2f} > 2.5"
        return ratio

    ratio = benchmark.pedantic(sample_ratio, rounds=200, warmup_rounds=10)
    assert ratio <= 2.5, f"fold linearity violated: fold({_LARGE_N}) / fold({_SMALL_N}) = {ratio:.2f} > 2.5"


def bench_envelope_encode(benchmark: ModelBenchmark) -> None:
    """msgspec.json.Encoder encodes a 100-row Report Envelope; len > 0 (no silent empty-encode regression)."""
    report = fold(Claim.STATIC, "check", tuple(receipt(("t",), 0, status=RailStatus.OK) for _ in range(100)))
    env = envelope(report, claim=Claim.STATIC, verb="check")
    result = benchmark.pedantic(_ENCODER.encode, args=(env,), rounds=500, warmup_rounds=10)
    assert len(result) > 0
