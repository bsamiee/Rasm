"""Fold micro + codec perf laws.

Perf mandate: fold(1000 Completed) mean < 1ms (rounds=100, warmup_rounds=5).
Codec: 100-row Report Envelope; encode rounds=500, warmup_rounds=10. len(result) > 0.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------------

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


# --- [CONSTANTS] -----------------------------------------------------------------------

_ENCODER = msgspec.json.Encoder(order="deterministic")


# --- [BENCHMARKS] ----------------------------------------------------------------------


def bench_fold_throughput(benchmark: ModelBenchmark) -> None:
    """fold(1000 Completed) mean < 1ms — O(N) fold is the core hot path."""
    outcomes = tuple(receipt(("t",), 0, status=RailStatus.OK) for _ in range(1000))
    result = benchmark.pedantic(fold, args=(Claim.STATIC, "check", outcomes), rounds=100, warmup_rounds=5)
    assert result.status is RailStatus.OK


def bench_envelope_encode(benchmark: ModelBenchmark) -> None:
    """msgspec.json.Encoder encodes a 100-row Report Envelope; len > 0 (no silent empty-encode regression)."""
    report = fold(Claim.STATIC, "check", tuple(receipt(("t",), 0, status=RailStatus.OK) for _ in range(100)))
    env = envelope(report, claim=Claim.STATIC, verb="check")
    result = benchmark.pedantic(_ENCODER.encode, args=(env,), rounds=500, warmup_rounds=10)
    assert len(result) > 0
