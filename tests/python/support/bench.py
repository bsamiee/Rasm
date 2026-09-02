"""Benchmark cases, absolute performance budgets, and sustained-regression detection."""

# --- [IMPORTS] --------------------------------------------------------------------------

from collections.abc import Callable
from functools import reduce
import gc
import inspect
from math import ceil, inf, log
from operator import itemgetter
import os
import time
from typing import Literal, TYPE_CHECKING

import msgspec
import psutil
import pytest

if TYPE_CHECKING:
    from collections.abc import Sequence
    from pathlib import Path

    from pytest_benchmark.fixture import BenchmarkFixture

# --- [CONSTANTS] ------------------------------------------------------------------------

_CALIBRATION_FLOOR_NS = 100_000
_ITERATIONS_CAP = 10_000
_POTTS_BETA = 4.0
_REGRESSION_TOLERANCE = 0.70

# --- [MODELS] ---------------------------------------------------------------------------


class BenchmarkCase(msgspec.Struct, frozen=True):
    """Benchmark subject, workload generator, and performance budget.

    ``workload(size)`` builds the tuple passed as the positional argument to ``subject``.
    ``budget_ms`` is an absolute ceiling over ``budget_statistic``, samples above ``max_relative_iqr`` skip
    rather than emit flaky verdicts.

    Attributes:
        enforce_budget: False records timings without asserting the absolute budget.
        budget_statistic: Statistic compared with the budget; ``mean`` is tail-sensitive.
        max_relative_iqr: Dispersion ceiling for a trustworthy budget assertion.
        fresh_per_round: Rebuilds mutating or consuming payloads before each measured round.
        warmup_rounds: Untimed passes before measurement.
        disable_gc: Disables GC inside pedantic because this path does not honor the CLI GC flag.
    """

    label: str
    subject: Callable[[tuple[object, ...]], object]
    workload: Callable[[int], tuple[object, ...]]
    sizes: tuple[int, ...] = (100, 1_000, 10_000)
    budget_ms: float = 100.0
    rounds: int = 5
    iterations: int = 1
    enforce_budget: bool = True
    budget_statistic: Literal["min", "median", "mean"] = "median"
    max_relative_iqr: float = 0.25
    fresh_per_round: bool = False
    warmup_rounds: int = 1
    disable_gc: bool = False


class _StoredStats(msgspec.Struct, frozen=True):
    """Persisted benchmark statistics used for regression detection."""

    median: float | None = None


class _StoredEntry(msgspec.Struct, frozen=True):
    """Persisted benchmark entry projection keyed by file, group, and size."""

    fullname: str | None = None
    group: str | None = None
    extra_info: dict[str, object] = msgspec.field(default_factory=dict)
    stats: _StoredStats = msgspec.field(default_factory=_StoredStats)


class _StoredDoc(msgspec.Struct, frozen=True):
    """Autosaved benchmark JSON document projection."""

    benchmarks: tuple[_StoredEntry, ...] = ()


# --- [OPERATIONS] -----------------------------------------------------------------------


def benchmark_parameters(cases: Sequence[BenchmarkCase]) -> pytest.MarkDecorator:
    """Build the ``(row, size)`` parametrization with stable ``"{label}-{size}"`` ids."""
    parameters = [(case, size) for case in cases for size in case.sizes]
    ids = [f"{case.label}-{size}" for case, size in parameters]
    return pytest.mark.parametrize("case,size", parameters, ids=ids)


def run_benchmark(benchmark: BenchmarkFixture, case: BenchmarkCase, size: int) -> object:
    """Measure a benchmark case and enforce its dispersion and performance budget."""
    process = psutil.Process(os.getpid())
    payload = case.workload(size)
    benchmark.group = case.label

    process.cpu_percent(interval=None)
    rss_before = process.memory_info().rss

    probe_start = time.perf_counter_ns()
    case.subject(payload)
    probe_duration_ns = time.perf_counter_ns() - probe_start
    iterations = min(_ITERATIONS_CAP, max(1, ceil(_CALIBRATION_FLOOR_NS / max(probe_duration_ns, 1)))) if (case.iterations == 1 and not case.fresh_per_round and probe_duration_ns < _CALIBRATION_FLOOR_NS) else case.iterations

    def _measure() -> object:
        return (
            benchmark.pedantic(  # type: ignore[no-untyped-call]
                case.subject, setup=lambda: ((case.workload(size),), {}), rounds=case.rounds, warmup_rounds=case.warmup_rounds
            )
            if case.fresh_per_round
            else benchmark.pedantic(  # type: ignore[no-untyped-call]
                case.subject, args=(payload,), rounds=case.rounds, iterations=iterations, warmup_rounds=case.warmup_rounds
            )
        )

    def _without_gc() -> object:
        gc.disable()
        try:
            return _measure()
        finally:
            gc.enable()

    result = _without_gc() if case.disable_gc else _measure()

    assert benchmark.stats is not None
    statistics = benchmark.stats.stats
    relative_iqr = statistics.iqr / statistics.median if statistics.median > 0 else inf
    observed_ms = getattr(statistics, case.budget_statistic) * 1000.0
    benchmark.extra_info.update(rss_delta_bytes=process.memory_info().rss - rss_before, rss_after_bytes=process.memory_info().rss, cpu_percent_delta=process.cpu_percent(interval=None), budget_ms=case.budget_ms, observed_ms=observed_ms, rel_iqr=relative_iqr, iterations=iterations, size=size)

    match (relative_iqr > case.max_relative_iqr, case.enforce_budget and observed_ms > case.budget_ms):
        case (True, _):
            pytest.skip(f"{case.label}-{size}: relative IQR {relative_iqr:.3f} exceeds {case.max_relative_iqr}, performance budget not evaluated")
        case (_, True):
            pytest.fail(f"{case.label}-{size}: {case.budget_statistic}={observed_ms:.4f}ms exceeds budget {case.budget_ms:.4f}ms")
        case _:
            pass

    return result


def register_benchmarks(cases: Sequence[BenchmarkCase]) -> Callable[..., None]:
    """Return a parametrized benchmark function assigned to the caller module."""
    caller_module: str = inspect.stack()[1].frame.f_globals["__name__"]

    @benchmark_parameters(cases)
    def benchmark_case(benchmark: BenchmarkFixture, case: BenchmarkCase, size: int) -> None:
        run_benchmark(benchmark, case, size)

    benchmark_case.__module__ = caller_module
    return benchmark_case


# --- [REGRESSION_DETECTION] -------------------------------------------------------------


def _potts_segments(series: tuple[float, ...]) -> tuple[tuple[float, ...], ...]:
    """Partition an oldest-first median series into segments with the greedy Potts/BIC step criterion."""
    n = len(series)
    penalty = _POTTS_BETA * log(max(n, 2))

    def _sse(seg: tuple[float, ...]) -> float:
        mu = sum(seg) / len(seg)
        return reduce(lambda acc, v: acc + (v - mu) ** 2, seg, 0.0)

    def _gain(seg: tuple[float, ...], i: int) -> float:
        full, split = _sse(seg), _sse(seg[:i]) + _sse(seg[i:])
        return len(seg) * log(full / split) if (full > 0.0 and split > 0.0) else (inf if full > 0.0 else 0.0)

    def _split(seg: tuple[float, ...]) -> tuple[tuple[float, ...], ...]:
        candidates = [(_gain(seg, i), i) for i in range(1, len(seg))]
        best = max(candidates, default=(0.0, 0), key=itemgetter(0))
        return (*_split(seg[: best[1]]), *_split(seg[best[1] :])) if (len(seg) >= 2 and best[0] > penalty) else (seg,)

    return _split(series) if n >= 2 else ((series,) if n else ())


def _storage_root(config: pytest.Config) -> Path:
    """Resolve the autosaved-benchmark root from the live ``--benchmark-storage`` option against ``config.rootpath``."""
    raw = str(config.getoption("benchmark_storage"))
    path = raw.removeprefix("file://")
    return config.rootpath / path


def _series_from_storage(config: pytest.Config, output_json: dict[str, object]) -> dict[tuple[str, str, int], tuple[float, ...]]:
    """Map ``(file, label, size)`` to its oldest-first median series from stored runs and the current report."""
    storage_root = _storage_root(config)
    prior_docs = (msgspec.json.decode(path.read_bytes(), type=_StoredDoc) for path in sorted(storage_root.glob("*/*.json")))
    current_doc = msgspec.convert(output_json, type=_StoredDoc, strict=False)
    ordered_entries = [entry for doc in (*prior_docs, current_doc) for entry in doc.benchmarks]

    def _accumulate(acc: dict[tuple[str, str, int], tuple[float, ...]], entry: _StoredEntry) -> dict[tuple[str, str, int], tuple[float, ...]]:
        match (entry.group, entry.extra_info.get("size"), entry.stats.median):
            case (str() as group, int() as size, float() as median):
                key = ((entry.fullname or "").partition("::")[0], group, size)
                return {**acc, key: (*acc.get(key, ()), median)}
            case _:
                return acc

    return reduce(_accumulate, ordered_entries, {})


def pytest_benchmark_update_json(config: pytest.Config, benchmarks: object, output_json: dict[str, object]) -> None:
    """Fail the session when a stored median series shows a sustained final-segment regression."""
    _ = benchmarks
    series_by_key = _series_from_storage(config, output_json)

    def _regression(segments: tuple[tuple[float, ...], ...]) -> float:
        prior_level = sum(segments[-2]) / len(segments[-2])
        last_level = sum(segments[-1]) / len(segments[-1])
        return (last_level - prior_level) / prior_level if prior_level > 0 else 0.0

    regressions = [(key, ratio) for key, series in series_by_key.items() if len(segments := _potts_segments(series)) >= 2 and (ratio := _regression(segments)) > _REGRESSION_TOLERANCE]
    (pytest.fail("sustained benchmark regression: " + "; ".join(f"{file}::{label}-{size}: +{ratio:.1%}" for (file, label, size), ratio in regressions), pytrace=False) if regressions else None)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BenchmarkCase", "benchmark_parameters", "run_benchmark", "register_benchmarks"]
