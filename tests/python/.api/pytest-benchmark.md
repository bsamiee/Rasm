# [PY_TESTS_API_PYTEST_BENCHMARK]

`pytest-benchmark` injects a `benchmark` fixture that runs a callable under a calibrated timer, folds robust statistics (`min`, `median`, `iqr`, `ops`), and persists each run as a JSON document under a storage URI. Rasm's testkit wraps it: `BenchCase` rows drive absolute-budget gates through `run_bench`, and the `pytest_benchmark_update_json` hook reconstructs per-subject median series to fail a session on a sustained regression. Benchmarks are deselected by default (`-m "not benchmark"`) and run in a session separate from `pytest-xdist`.

## [01]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]           | [KIND]         | [CAPABILITY]                                                                                   |
| :-----: | :----------------- | :------------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | `BenchmarkFixture` | fixture object | injected `benchmark`; carries `group`/`extra_info`/`stats`, `pedantic`/`weave`/`patch` runners |
|  [02]   | `Stats`            | stat carrier   | fields `min`/`max`/`mean`/`stddev`/`median`/`iqr`/`q1`/`q3`/`ops`/`rounds`/`total`, outliers   |
|  [03]   | `Metadata`         | stats wrapper  | holds `Stats` under `.stats`; `as_dict()` projects the JSON entry, `has_error` flags failure   |
|  [04]   | `BenchmarkSession` | session        | aggregates fixtures, resolves the storage URI, and generates/compares the run JSON             |
|  [05]   | `FileStorage`      | storage        | `file://` backend writing `<machine>/NNNN_<name>.json` under the storage root; eager `mkdir`   |

```python
class BenchmarkFixture:
    group: str | None; extra_info: dict[str, object]; stats: Metadata | None
    def __call__(self, function_to_benchmark: Callable[..., R], *args: object, **kwargs: object) -> R: ...
    def pedantic(self, target: Callable[..., R], args: tuple = (), kwargs: dict | None = None, setup: Callable | None = None,
                 teardown: Callable | None = None, rounds: int = 1, warmup_rounds: int = 0, iterations: int = 1) -> R: ...
class Stats:
    min: float; median: float; mean: float; iqr: float; ops: float; rounds: int
```

## [02]-[ENTRYPOINTS]

| [INDEX] | [SURFACE]                                          | [KIND]        | [CAPABILITY]                                                        |
| :-----: | :------------------------------------------------- | :------------ | :------------------------------------------------------------------ |
|  [01]   | `benchmark(fn, *args, **kwargs)`                   | fixture call  | runs `fn` under an autocalibrated round/iteration schedule          |
|  [02]   | `benchmark.pedantic(...)`                          | fixture call  | explicit `rounds`/`iterations`/`warmup_rounds` + `setup` closure    |
|  [03]   | `benchmark.extra_info` / `benchmark.group`         | metadata slot | arbitrary keys and the series group folded into the JSON entry      |
|  [04]   | `--benchmark-storage`                              | CLI option    | run-storage URI: `file://` or `elasticsearch+http://`               |
|  [05]   | `--benchmark-autosave` / `--benchmark-save=NAME`   | CLI option    | persist the run; autosave when benchmarks ran, save needs a name    |
|  [06]   | `--benchmark-compare` / `--benchmark-compare-fail` | CLI option    | diff a stored run; fail on breach (`min:5%`, `mean:0.001`)          |
|  [07]   | `--benchmark-disable`                              | CLI flag      | run subjects without timing                                         |
|  [08]   | `--benchmark-only`                                 | CLI flag      | run only benchmarks                                                 |
|  [09]   | `--benchmark-skip`                                 | CLI flag      | skip all benchmarks                                                 |
|  [10]   | `pytest_benchmark_update_json`                     | hookspec      | mutate `output_json` post-run; the regression gate reads the series |

```python
def pytest_benchmark_update_json(config: pytest.Config, benchmarks: object, output_json: dict[str, object]) -> None: ...
```

## [03]-[IMPLEMENTATION_LAW]

[PYTEST_BENCHMARK_TOPOLOGY]:
- `benchmark` times a subject once per fixture request; `pedantic` is the only path with a per-round `setup`, which the testkit uses to rebuild mutating payloads.
- `stats` is `None` until the run finishes, then exposes robust statistics; `benchmark.group` set before `pedantic` becomes the storage-series key, so late assignment drops the series.
- `--benchmark-autosave` persists one JSON document per run under `<storage-root>/<machine>/`; the sustained-regression fold reads the ordered set of those documents and the current report.

[STACKING]:
- `bench.py`(`../testkit/bench.py`): `BenchCase` rows carry `budget_ms`/`gate_stat`/`max_rel_iqr`; `run_bench` drives `pedantic`, writes `extra_info`, and skips or fails on dispersion or budget; `pytest_benchmark_update_json` folds `_series_from_storage` medians through the Potts/BIC step detector.
- `pyproject.toml`(`../../../pyproject.toml`): `addopts` pins `--benchmark-storage=file://.artifacts/python/benchmarks` and `--benchmark-autosave`; `-m "not benchmark"` deselects the lane and `filterwarnings` ignores the autosave `PytestBenchmarkWarning` in benchmark-free sessions.

[LOCAL_ADMISSION]:
- Admitted at the shared test tier through the `pytest11` entry point; the plugin auto-disables under `pytest-xdist`, so benchmark and parallel runs stay in separate sessions and default `addopts` carries no `-n`.
- `required_plugins` lists `pytest-benchmark`; the testkit registers its `bench` module as `testkit-bench` from `runtime.py` only when the update-json hook is present.
