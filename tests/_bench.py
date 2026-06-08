# --- [RUNTIME_PRELUDE] -----------------------------------------------------------------

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

    from _pytest.config import Config
    from pytest_benchmark.fixture import BenchmarkFixture

# --- [CONSTANTS] -----------------------------------------------------------------------

# Auto-calibration floor: pedantic's default iterations=1 is unsafe below 100us (timer
# resolution/overhead dominates), so sub-floor subjects get their iteration count raised
# until the measured window clears the floor, capped to keep a single round bounded.
_CALIBRATION_FLOOR_NS = 100_000
_ITERATIONS_CAP = 10_000

# Session-end step detector: penalized piecewise-constant (Potts) fit over the per-(label,size)
# median time series. A breakpoint is admitted when the scale-free log-likelihood gain
# `n * ln(sse_full / sse_split)` clears the BIC-style penalty `_POTTS_BETA * ln(n)` (~4 DOF/
# breakpoint, ASV convention); a final segment exceeding the prior segment's level by more than
# `_REGRESSION_TOLERANCE` (relative) then fails the gate. Storage dir mirrors the pytest addopts.
_POTTS_BETA = 4.0
_REGRESSION_TOLERANCE = 0.70
_BENCHMARK_STORAGE_DIR = ".artifacts/python/benchmarks"

# --- [MODELS] --------------------------------------------------------------------------


class BenchCase(msgspec.Struct, frozen=True):
    """One registry row binding a subject callable to its benchmark geometry and gate policy.

    One row = one benchmark; callers never define per-benchmark functions. ``workload`` accepts
    the size integer and returns the argument tuple that ``subject`` receives (as a single
    positional argument). ``budget_ms`` is a HARD per-subject absolute ceiling: ``run_bench``
    asserts ``stats.<gate_stat> <= budget_ms`` after each measured benchmark (``--benchmark-
    compare-fail`` is relative-to-prior-run only and cannot express an absolute budget). The
    gate is statistically guarded — a sample whose dispersion exceeds ``max_rel_iqr`` is skipped
    as untrustworthy rather than producing a flaky verdict.

    Fields:
        gate: When False, the row is measured and recorded but the absolute budget is not asserted
            (regression-compare-only rows).
        gate_stat: Which robust statistic the budget gates on; ``median`` (default) or ``min`` for
            the "best achievable" posture. ``mean`` is accepted but discouraged (tail-sensitive).
        max_rel_iqr: Dispersion ceiling (IQR / median). Above it the measurement is too noisy to
            gate and the case is skipped.
        fresh_per_round: When True the payload is rebuilt before every round via pedantic's setup
            callback (forces ``iterations=1``), so subjects that MUTATE or CONSUME their argument
            (in-place sort, generator drain, queue pop) measure a fresh payload each round instead
            of an exhausted one.
        warmup_rounds: Un-timed warm passes before the measured rounds (JIT/cache priming).
        disable_gc: When True the pedantic call runs inside ``gc.disable()``/``gc.enable()`` so GC
            collection spikes stay out of the timed window (the pedantic path does not honor the
            CLI ``--benchmark-disable-gc`` the auto-fixture path does).
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
    """Schema projection of a persisted ``stats`` block (median only; other keys ignored)."""

    median: float | None = None


class _StoredEntry(msgspec.Struct, frozen=True):
    """Schema projection of one persisted benchmark entry (median + (label,size) key fields only)."""

    group: str | None = None
    extra_info: dict[str, object] = msgspec.field(default_factory=dict)
    stats: _StoredStats = msgspec.field(default_factory=_StoredStats)


class _StoredDoc(msgspec.Struct, frozen=True):
    """Schema projection of one autosaved benchmark JSON document."""

    benchmarks: tuple[_StoredEntry, ...] = ()


# --- [OPERATIONS] ----------------------------------------------------------------------


def bench_params(rows: Sequence[BenchCase]) -> pytest.MarkDecorator:
    """Build a ``pytest.mark.parametrize`` decorator over the Cartesian (row, size) product.

    Args:
        rows: Ordered registry of benchmark subjects.

    Returns:
        A ``MarkDecorator`` whose parametrize ids are ``"{label}-{size}"`` so
        benchmark storage keys are human-readable and stable across runs.
    """
    cases = [(row, size) for row in rows for size in row.sizes]
    ids = [f"{row.label}-{size}" for row, size in cases]
    return pytest.mark.parametrize("row,size", cases, ids=ids)


def run_bench(benchmark: BenchmarkFixture, row: BenchCase, size: int) -> object:
    """Drive ``benchmark.pedantic`` with auto-calibration, psutil deltas, and the absolute budget gate.

    Pipeline: an untimed probe auto-raises ``iterations`` for sub-100us subjects (unless the row
    pinned iterations or requested ``fresh_per_round``); the payload is built once (or rebuilt per
    round via the setup callback when ``fresh_per_round`` is set); the pedantic call runs under an
    optional GC gate; the resulting ``benchmark.stats`` drive the dispersion floor (skip when too
    noisy) and the hard absolute-budget assertion. ``benchmark.group`` is set to ``row.label`` so
    relative compares stay within a subject across size variants, and the time series the session-
    end step detector consumes is keyed by ``(group, extra_info['size'])``.

    Args:
        benchmark: The ``pytest-benchmark`` ``BenchmarkFixture`` injected by pytest.
        row: Registry row describing the subject, workload factory, timing policy, and gate policy.
        size: The concrete workload size drawn from ``row.sizes``.

    Returns:
        The value returned by the final pedantic round (passthrough for spot-check assertions;
        benchmarks must not depend on this).
    """
    proc = psutil.Process(os.getpid())
    payload = row.workload(size)

    # Non-blocking CPU sample primes the interval counter; the post-call read measures the
    # fraction spent in user+sys during the pedantic window. RSS is sampled around the call.
    proc.cpu_percent(interval=None)
    rss_before = proc.memory_info().rss

    # Untimed probe -> auto-calibrate iterations so sub-100us subjects clear the timer-resolution
    # floor. Skipped when the row pinned iterations (>1) or rebuilds per round (setup forces 1).
    probe_start = time.perf_counter_ns()
    row.subject(payload)
    probe_dt = time.perf_counter_ns() - probe_start
    iterations = (
        min(_ITERATIONS_CAP, max(1, ceil(_CALIBRATION_FLOOR_NS / max(probe_dt, 1))))
        if (row.iterations == 1 and not row.fresh_per_round and probe_dt < _CALIBRATION_FLOOR_NS)
        else row.iterations
    )

    # pytest-benchmark ships no stub for BenchmarkFixture.pedantic; ty resolves it, mypy needs the inline ignore.
    # fresh_per_round rebuilds the payload via setup before each round (consuming/mutating subjects),
    # which forces iterations=1; otherwise the built-once payload is reused with calibrated iterations.
    def _measure() -> object:
        return (
            benchmark.pedantic(row.subject, setup=lambda: ((row.workload(size),), {}), rounds=row.rounds, warmup_rounds=row.warmup_rounds)  # type: ignore[no-untyped-call]
            if row.fresh_per_round
            else benchmark.pedantic(row.subject, args=(payload,), rounds=row.rounds, iterations=iterations, warmup_rounds=row.warmup_rounds)  # type: ignore[no-untyped-call]
        )

    # gc.disable/enable is a resource boundary (try/finally), not control flow.
    def _gated() -> object:
        gc.disable()
        try:
            return _measure()
        finally:
            gc.enable()

    result = _gated() if row.disable_gc else _measure()

    # benchmark.stats is a Metadata (always populated post-pedantic); the robust Stats sample lives
    # on its .stats attribute (verified against pytest-benchmark 5.2.3 — NOT directly on Metadata).
    assert benchmark.stats is not None
    s = benchmark.stats.stats
    rel_iqr = s.iqr / s.median if s.median > 0 else inf
    observed_ms = getattr(s, row.gate_stat) * 1000.0
    benchmark.group = row.label
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

    # Dispersion floor before the budget assertion: a sample whose IQR dominates its median is
    # noise, so an untrustworthy verdict is a skip, not a flaky fail.
    (pytest.skip(f"{row.label}-{size}: sample too noisy to gate (rel_iqr={rel_iqr:.3f} > {row.max_rel_iqr})") if rel_iqr > row.max_rel_iqr else None)
    (
        pytest.fail(f"{row.label}-{size}: {row.gate_stat}={observed_ms:.4f}ms exceeds budget {row.budget_ms:.4f}ms")
        if (row.gate and observed_ms > row.budget_ms)
        else None
    )

    return result


def run_registry(rows: Sequence[BenchCase]) -> Callable[..., None]:
    """Return a fully-decorated pytest benchmark function over the (row, size) Cartesian product.

    Composes ``bench_params`` and ``run_bench`` so callers reduce to a single assignment:
    ``bench_foo = run_registry(_ROWS)``.  pytest collects the returned function by name; the
    function's ``__module__`` is patched to the calling module so ``collect_imported_tests=false``
    does not suppress collection.

    Args:
        rows: Ordered registry of BenchCase rows.

    Returns:
        A parametrized test function whose ``__module__`` matches the caller's module.
    """
    caller_module: str = inspect.stack()[1].frame.f_globals["__name__"]

    @bench_params(rows)
    def bench_(benchmark: BenchmarkFixture, row: BenchCase, size: int) -> None:
        run_bench(benchmark, row, size)

    bench_.__module__ = caller_module
    return bench_


# --- [COMPOSITION] ---------------------------------------------------------------------


def _potts_segments(series: tuple[float, ...]) -> tuple[tuple[float, ...], ...]:
    """Greedy penalized piecewise-constant (Potts) partition of a median time series.

    Approximates the ASV step-detector via a scale-free Gaussian BIC criterion: a candidate
    breakpoint is admitted only when the log-likelihood gain ``n * ln(sse_full / sse_split)``
    exceeds the per-breakpoint penalty ``gamma = beta * ln(n)`` (~4 DOF/breakpoint, ASV/BIC
    convention). Using the log-ratio rather than the raw SSE reduction makes the criterion
    invariant to the absolute timing scale, so a 3x sustained step is detected whether the
    medians are nanoseconds or seconds, while a single-run fluke stays below the penalty.

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

    # Greedy split: at each segment pick the cut maximizing the scale-free log-likelihood gain,
    # admitting it only when the gain clears the penalty; fold the recursion over the series.
    def _gain(seg: tuple[float, ...], i: int) -> float:
        full, split = _sse(seg), _sse(seg[:i]) + _sse(seg[i:])
        return len(seg) * log(full / split) if (full > 0.0 and split > 0.0) else (inf if full > 0.0 else 0.0)

    def _split(seg: tuple[float, ...]) -> tuple[tuple[float, ...], ...]:
        candidates = [(_gain(seg, i), i) for i in range(1, len(seg))]
        best = max(candidates, default=(0.0, 0), key=itemgetter(0))
        return (*_split(seg[: best[1]]), *_split(seg[best[1] :])) if (len(seg) >= 2 and best[0] > penalty) else (seg,)

    return _split(series) if n >= 2 else ((series,) if n else ())


def _series_from_storage(config: Config, output_json: dict[str, object]) -> dict[tuple[str, int], tuple[float, ...]]:
    """Reconstruct the per-(label,size) median time series from prior storage plus the current run.

    Reads every prior autosaved benchmark JSON under the machine-specific storage subdirectory,
    keyed by ``(group, extra_info['size'])`` to match the live registry rows, then appends the
    current run's medians (oldest-first) so the last observation is always the run under gate.
    Documents are decoded into the ``_StoredDoc`` projection so malformed/foreign entries (no
    ``group``/``size``/``median``) are dropped rather than crashing the gate.

    Args:
        config: The pytest config (resolves the rootdir the storage URI is relative to).
        output_json: The serialized current-run benchmark report from ``pytest_benchmark_update_json``.

    Returns:
        Mapping from ``(label, size)`` to its ordered median series (seconds), oldest first.
    """
    storage_root = config.rootpath / _BENCHMARK_STORAGE_DIR
    prior_docs = (msgspec.json.decode(path.read_bytes(), type=_StoredDoc) for path in sorted(storage_root.glob("*/*.json")))
    current_doc = msgspec.convert(output_json, type=_StoredDoc, strict=False)
    ordered_entries = [entry for doc in (*prior_docs, current_doc) for entry in doc.benchmarks]

    def _accumulate(acc: dict[tuple[str, int], tuple[float, ...]], entry: _StoredEntry) -> dict[tuple[str, int], tuple[float, ...]]:
        size = entry.extra_info.get("size")
        median = entry.stats.median
        return (
            {**acc, (entry.group, size): (*acc.get((entry.group, size), ()), median)}
            if (entry.group is not None and isinstance(size, int) and median is not None)
            else acc
        )

    return reduce(_accumulate, ordered_entries, {})


def pytest_benchmark_update_json(config: Config, benchmarks: object, output_json: dict[str, object]) -> None:
    """Session-end sustained-regression gate over the stored median time series.

    Fits a penalized piecewise-constant (greedy Potts/BIC) model to each ``(label,size)`` median
    series (prior autosaved runs + this run) and fails the session when the final segment's level
    exceeds the prior segment's by more than ``_REGRESSION_TOLERANCE`` relative. This is a SUSTAINED
    regression signal (robust to one-run flakiness), strictly stronger than the single-prior
    relative threshold of ``--benchmark-compare-fail``. Series with a single segment (no sustained
    shift) pass silently.

    Args:
        config: The pytest config.
        benchmarks: The live benchmark fixtures (unused; medians read from ``output_json``/storage).
        output_json: The serialized current-run benchmark report (mutated in place by the plugin).
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
            "sustained benchmark regression: " + "; ".join(f"{label}-{size}: +{ratio:.1%}" for (label, size), ratio in breaches), pytrace=False
        )
        if breaches
        else None
    )


# --- [EXPORTS] -------------------------------------------------------------------------

__all__ = ["BenchCase", "bench_params", "run_bench", "run_registry"]
