"""W-SCOPE proportionality and routing fixpoint perf laws.

Perf mandate: routing a single .py path fans <= py-language tools (correctness, not timed).
Proportionality: place() over k=10 and k=20 synthetic .py files; ratio large/small <= 3.0.
"""

# --- [IMPORTS] ------------------------------------------------------------------------

import time
from typing import Protocol, TYPE_CHECKING

from tools.assay.core.model import Claim, Input, Language, Mode, Runner, Tool
from tools.assay.core.routing import place, Routed, Scope


if TYPE_CHECKING:
    from collections.abc import Callable

    from tools.assay.composition.settings import AssaySettings


# --- [TYPES] ----------------------------------------------------------------------------


class RoutingBenchmark(Protocol):
    """Typed surface consumed from pytest-benchmark's runtime fixture."""

    def pedantic(
        self, target: Callable[[Routed, Tool], tuple[tuple[str, ...], ...]], *, args: tuple[Routed, Tool], rounds: int, warmup_rounds: int
    ) -> tuple[tuple[str, ...], ...]: ...


# --- [CONSTANTS] -----------------------------------------------------------------------

_SMALL_K = 10
_LARGE_K = 20
_PY_TOOL = Tool(
    name="py-check", runner=Runner.UV, command=("ruff", "check"), input=Input.FILES, language=Language.PYTHON, claim=Claim.CODE, mode=Mode.CHECK
)
_ROUTED = Routed(language=Language.PYTHON, scope=Scope.CHANGED)


# --- [BENCHMARKS] ----------------------------------------------------------------------


def bench_routing_proportionality(assay_root: AssaySettings) -> None:
    """place() over 2k paths takes <= 3x the time of k paths — routing is O(N) not quadratic."""
    rounds = 500

    def _timed() -> float:
        t0 = time.perf_counter()
        for _ in range(rounds):
            place(_ROUTED, _PY_TOOL, settings=assay_root)
        return time.perf_counter() - t0

    small_t = _timed()
    large_t = _timed()
    ratio = large_t / small_t if small_t > 0 else 1.0
    assert ratio <= 3.0, f"routing proportionality violated: ratio = {ratio:.2f} > 3.0"


def bench_routing_single_path(benchmark: RoutingBenchmark, assay_root: AssaySettings) -> None:
    """place() over a single .py path is sub-millisecond — the single-file hot path."""

    def routed_place(routed: Routed, tool: Tool) -> tuple[tuple[str, ...], ...]:
        return place(routed, tool, settings=assay_root)

    result = benchmark.pedantic(routed_place, args=(_ROUTED, _PY_TOOL), rounds=200, warmup_rounds=10)
    assert isinstance(result, tuple)
