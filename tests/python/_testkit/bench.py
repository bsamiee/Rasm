"""Benchmark registry, absolute-budget gates, and sustained-regression detection."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

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

# Sub-100us probes are re-calibrated because timer resolution dominates single-iteration pedantic runs.
_CALIBRATION_FLOOR_NS = 100_000
_ITERATIONS_CAP = 10_000

# Breakpoints require scale-free BIC gain beyond the ASV-style per-step penalty.
_POTTS_BETA = 4.0
_REGRESSION_TOLERANCE = 0.70

# --- [MODELS] ---------------------------------------------------------------------------


class BenchCase(msgspec.Struct, frozen=True):
    """Registry row for one benchmark subject, workload geometry, and absolute gate.

    ``workload(size)`` builds the tuple passed as the single positional argument to ``subject``.
    ``budget_ms`` is an absolute ceiling over ``gate_stat``; samples above ``max_rel_iqr`` skip
    rather than emit flaky verdicts.

    Attributes:
        gate: False records timings without asserting the absolute budget.
        gate_stat: Robust statistic the budget gates on; ``mean`` is admitted but tail-sensitive.
        max_rel_iqr: Dispersion ceiling for a trustworthy gate.
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
    gate: bool = True
    gate_stat: Literal["min", "median", "mean"] = "median"
    max_rel_iqr: float = 0.25
    fresh_per_round: bool = False
    warmup_rounds: int = 1
    disable_gc: bool = False


class _StoredStats(msgspec.Struct, frozen=True):
    """Persisted benchmark ``stats`` projection used by the regression gate."""

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


def bench_params(rows: Sequence[BenchCase]) -> pytest.MarkDecorator:
    """Build the benchmark ``(row, size)`` parametrization.

    Args:
        rows: Ordered registry of benchmark subjects.

    Returns:
        Parametrize decorator with stable ``"{label}-{size}"`` ids.
    """
    cases = [(row, size) for row in rows for size in row.sizes]
    ids = [f"{row.label}-{size}" for row, size in cases]
    return pytest.mark.parametrize("row,size", cases, ids=ids)


def run_bench(benchmark: BenchmarkFixture, row: BenchCase, size: int) -> object:
    """Measure one benchmark case and enforce its dispersion and absolute-budget gates.

    Args:
        benchmark: ``pytest-benchmark`` fixture carrying stats and storage metadata.
        row: Subject, workload, and gate policy.
        size: The concrete workload size drawn from ``row.sizes``.

    Returns:
        Final pedantic result for spot-check assertions.
    """
    proc = psutil.Process(os.getpid())
    payload = row.workload(size)
    # Metadata snapshots fixture.group inside pedantic; late assignment drops the storage series key.
    benchmark.group = row.label

    # Prime psutil so the post-pedantic read reflects the timed window.
    proc.cpu_percent(interval=None)
    rss_before = proc.memory_info().rss

    probe_start = time.perf_counter_ns()
    row.subject(payload)
    probe_dt = time.perf_counter_ns() - probe_start
    iterations = (
        min(_ITERATIONS_CAP, max(1, ceil(_CALIBRATION_FLOOR_NS / max(probe_dt, 1))))
        if (row.iterations == 1 and not row.fresh_per_round and probe_dt < _CALIBRATION_FLOOR_NS)
        else row.iterations
    )

    def _measure() -> object:
        return (
            benchmark.pedantic(  # type: ignore[no-untyped-call]  # BenchmarkFixture.pedantic has no stub; ty resolves, mypy cannot
                row.subject, setup=lambda: ((row.workload(size),), {}), rounds=row.rounds, warmup_rounds=row.warmup_rounds
            )
            if row.fresh_per_round
            else benchmark.pedantic(  # type: ignore[no-untyped-call]  # BenchmarkFixture.pedantic has no stub; ty resolves, mypy cannot
                row.subject, args=(payload,), rounds=row.rounds, iterations=iterations, warmup_rounds=row.warmup_rounds
            )
        )

    def _gated() -> object:
        gc.disable()
        try:
            return _measure()
        finally:
            gc.enable()

    result = _gated() if row.disable_gc else _measure()

    # pytest-benchmark stores robust stats under Metadata.stats, not on the Metadata wrapper.
    assert benchmark.stats is not None
    s = benchmark.stats.stats
    rel_iqr = s.iqr / s.median if s.median > 0 else inf
    observed_ms = getattr(s, row.gate_stat) * 1000.0
    benchmark.extra_info.update(
        rss_delta_bytes=proc.memory_info().rss - rss_before,
        rss_after_bytes=proc.memory_info().rss,
        cpu_percent_delta=proc.cpu_percent(interval=None),
        budget_ms=row.budget_ms,
        observed_ms=observed_ms,
        rel_iqr=rel_iqr,
        iterations=iterations,
        size=size,
    )

    match (rel_iqr > row.max_rel_iqr, row.gate and observed_ms > row.budget_ms):
        case (True, _):
            pytest.skip(f"{row.label}-{size}: sample too noisy to gate (rel_iqr={rel_iqr:.3f} > {row.max_rel_iqr})")
        case (_, True):
            pytest.fail(f"{row.label}-{size}: {row.gate_stat}={observed_ms:.4f}ms exceeds budget {row.budget_ms:.4f}ms")
        case _:
            pass

    return result


def run_registry(rows: Sequence[BenchCase]) -> Callable[..., None]:
    """Return a collectable benchmark function for a registry.

    Args:
        rows: Ordered registry of BenchCase rows.

    Returns:
        Parametrized benchmark function stamped with the caller module for collection.
    """
    caller_module: str = inspect.stack()[1].frame.f_globals["__name__"]

    @bench_params(rows)
    def bench_(benchmark: BenchmarkFixture, row: BenchCase, size: int) -> None:
        run_bench(benchmark, row, size)

    bench_.__module__ = caller_module
    return bench_


# --- [ENTRY] ----------------------------------------------------------------------------


def _potts_segments(series: tuple[float, ...]) -> tuple[tuple[float, ...], ...]:
    """Partition a median time series with the greedy Potts/BIC step criterion.

    Args:
        series: Ordered per-(label,size) median observations, oldest first.

    Returns:
        Tuple of segments partitioning ``series`` in order.
    """
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
    """Resolve the autosaved-benchmark root from the live ``--benchmark-storage`` option.

    Returns:
        Absolute storage root (``config.rootpath / <path>`` for relative URIs).
    """
    raw = str(config.getoption("benchmark_storage"))
    path = raw.removeprefix("file://")
    return config.rootpath / path


def _series_from_storage(config: pytest.Config, output_json: dict[str, object]) -> dict[tuple[str, str, int], tuple[float, ...]]:
    """Reconstruct per-file benchmark median series from stored runs and the current report.

    Args:
        config: Pytest config that owns the storage URI root.
        output_json: Current-run benchmark report from ``pytest_benchmark_update_json``.

    Returns:
        Mapping from ``(file, label, size)`` to its ordered median series (seconds), oldest first.
    """
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
    """Fail the session when stored median series show a sustained final-segment regression.

    Args:
        config: The pytest config.
        benchmarks: Live benchmark fixtures; unused because medians come from report/storage.
        output_json: Current-run benchmark report.
    """
    _ = benchmarks
    series_by_key = _series_from_storage(config, output_json)

    def _regression(segments: tuple[tuple[float, ...], ...]) -> float:
        prior_level = sum(segments[-2]) / len(segments[-2])
        last_level = sum(segments[-1]) / len(segments[-1])
        return (last_level - prior_level) / prior_level if prior_level > 0 else 0.0

    breaches = [
        (key, ratio)
        for key, series in series_by_key.items()
        if len(segments := _potts_segments(series)) >= 2 and (ratio := _regression(segments)) > _REGRESSION_TOLERANCE
    ]
    (
        pytest.fail(
            "sustained benchmark regression: " + "; ".join(f"{file}::{label}-{size}: +{ratio:.1%}" for (file, label, size), ratio in breaches),
            pytrace=False,
        )
        if breaches
        else None
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BenchCase", "bench_params", "run_bench", "run_registry"]
