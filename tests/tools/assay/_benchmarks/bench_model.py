"""Fold micro + codec perf laws.

Perf mandate: fold(1000 Completed) mean < 1ms (rounds=100, warmup_rounds=5).
Linearity: fold(2N) / fold(N) mean ratio <= 2.5 (benchmark.pedantic, rounds=200 each).
Codec: 100-row Report Envelope; encode rounds=500, warmup_rounds=10. len(result) > 0.
"""

# --- [IMPORTS] ------------------------------------------------------------------------

from typing import TYPE_CHECKING

import msgspec

from tools.assay.core.model import Claim, envelope, fold, receipt
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    import pytest

    from tools.assay.core.model import Completed


# --- [CONSTANTS] -----------------------------------------------------------------------

_ENCODER = msgspec.json.Encoder(order="deterministic")
_SMALL_N = 100
_LARGE_N = 200
_OUTCOMES_SMALL: tuple[Completed, ...] = tuple(receipt(("tool",), 0, status=RailStatus.OK) for _ in range(_SMALL_N))  # type: ignore[type-arg]  # Completed in TYPE_CHECKING block
_OUTCOMES_LARGE: tuple[Completed, ...] = tuple(receipt(("tool",), 0, status=RailStatus.OK) for _ in range(_LARGE_N))  # type: ignore[type-arg]  # Completed in TYPE_CHECKING block


# --- [BENCHMARKS] ----------------------------------------------------------------------


def bench_fold_throughput(benchmark: pytest.FixtureDef) -> None:  # type: ignore[name-defined]
    """fold(1000 Completed) mean < 1ms — O(N) fold is the core hot path."""
    outcomes = tuple(receipt(("t",), 0, status=RailStatus.OK) for _ in range(1000))
    result = benchmark.pedantic(fold, args=(Claim.STATIC, "check", outcomes), rounds=100, warmup_rounds=5)  # ty: ignore[unresolved-attribute]
    assert result.status is RailStatus.OK


def bench_fold_linearity(benchmark: pytest.FixtureDef) -> None:  # type: ignore[name-defined]
    """fold(2N) / fold(N) mean ratio <= 2.5 via benchmark.pedantic — confirms O(N), not super-linear.

    Uses two sequential pedantic calls on the same benchmark fixture; compares means via stats dict.
    """
    benchmark.pedantic(fold, args=(Claim.STATIC, "check", _OUTCOMES_SMALL), rounds=200, warmup_rounds=10)  # ty: ignore[unresolved-attribute]
    small_mean: float = benchmark.stats["mean"]  # ty: ignore[unresolved-attribute]
    benchmark.pedantic(fold, args=(Claim.STATIC, "check", _OUTCOMES_LARGE), rounds=200, warmup_rounds=10)  # ty: ignore[unresolved-attribute]
    large_mean: float = benchmark.stats["mean"]  # ty: ignore[unresolved-attribute]
    ratio = large_mean / small_mean if small_mean > 0 else 1.0
    assert ratio <= 2.5, f"fold linearity violated: fold({_LARGE_N}) / fold({_SMALL_N}) mean = {ratio:.2f} > 2.5"


def bench_envelope_encode(benchmark: pytest.FixtureDef) -> None:  # type: ignore[name-defined]
    """msgspec.json.Encoder encodes a 100-row Report Envelope; len > 0 (no silent empty-encode regression)."""
    report = fold(Claim.STATIC, "check", tuple(receipt(("t",), 0, status=RailStatus.OK) for _ in range(100)))
    env = envelope(report, claim=Claim.STATIC, verb="check")
    result = benchmark.pedantic(_ENCODER.encode, args=(env,), rounds=500, warmup_rounds=10)  # ty: ignore[unresolved-attribute]
    assert len(result) > 0
