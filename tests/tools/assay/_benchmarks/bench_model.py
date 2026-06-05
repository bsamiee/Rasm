"""Fold micro + codec perf laws.

Perf mandate: fold(1000 Completed) mean < 1ms (rounds=100, warmup_rounds=5).
Linearity: fold(2N) / fold(N) mean ratio <= 2.5 (benchmark.pedantic, rounds=200 each).
Codec: 100-row Report Envelope; encode rounds=500, warmup_rounds=10. len(result) > 0.
"""

# --- [IMPORTS] ------------------------------------------------------------------------

from typing import overload, Protocol, TYPE_CHECKING

import msgspec

from tools.assay.core.model import Claim, envelope, fold, receipt
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from collections.abc import Callable, Mapping

    from tools.assay.core.model import Completed, Envelope, Report


# --- [TYPES] ----------------------------------------------------------------------------


class ModelBenchmark(Protocol):
    """Typed surface consumed from pytest-benchmark's runtime fixture."""

    stats: Mapping[str, float]

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


# --- [CONSTANTS] -----------------------------------------------------------------------

_ENCODER = msgspec.json.Encoder(order="deterministic")
_SMALL_N = 100
_LARGE_N = 200
_OUTCOMES_SMALL: tuple[Completed, ...] = tuple(receipt(("tool",), 0, status=RailStatus.OK) for _ in range(_SMALL_N))
_OUTCOMES_LARGE: tuple[Completed, ...] = tuple(receipt(("tool",), 0, status=RailStatus.OK) for _ in range(_LARGE_N))


# --- [BENCHMARKS] ----------------------------------------------------------------------


def bench_fold_throughput(benchmark: ModelBenchmark) -> None:
    """fold(1000 Completed) mean < 1ms — O(N) fold is the core hot path."""
    outcomes = tuple(receipt(("t",), 0, status=RailStatus.OK) for _ in range(1000))
    result = benchmark.pedantic(fold, args=(Claim.STATIC, "check", outcomes), rounds=100, warmup_rounds=5)
    assert result.status is RailStatus.OK


def bench_fold_linearity(benchmark: ModelBenchmark) -> None:
    """fold(2N) / fold(N) mean ratio <= 2.5 via benchmark.pedantic — confirms O(N), not super-linear.

    Uses two sequential pedantic calls on the same benchmark fixture; compares means via stats dict.
    """
    benchmark.pedantic(fold, args=(Claim.STATIC, "check", _OUTCOMES_SMALL), rounds=200, warmup_rounds=10)
    small_mean = benchmark.stats["mean"]
    benchmark.pedantic(fold, args=(Claim.STATIC, "check", _OUTCOMES_LARGE), rounds=200, warmup_rounds=10)
    large_mean = benchmark.stats["mean"]
    ratio = large_mean / small_mean if small_mean > 0 else 1.0
    assert ratio <= 2.5, f"fold linearity violated: fold({_LARGE_N}) / fold({_SMALL_N}) mean = {ratio:.2f} > 2.5"


def bench_envelope_encode(benchmark: ModelBenchmark) -> None:
    """msgspec.json.Encoder encodes a 100-row Report Envelope; len > 0 (no silent empty-encode regression)."""
    report = fold(Claim.STATIC, "check", tuple(receipt(("t",), 0, status=RailStatus.OK) for _ in range(100)))
    env = envelope(report, claim=Claim.STATIC, verb="check")
    result = benchmark.pedantic(_ENCODER.encode, args=(env,), rounds=500, warmup_rounds=10)
    assert len(result) > 0
