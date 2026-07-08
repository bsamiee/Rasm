# [pytest-benchmark] — the timed-measurement fixture and stored-run series behind the regression gate

`pytest-benchmark` injects a `benchmark` fixture that runs a callable under a calibrated timer, folds robust statistics (`min`, `median`, `iqr`, `ops`), and persists each run as a JSON document under a storage URI. The Rasm testkit wraps it: `BenchCase` rows drive absolute-budget gates through `run_bench`, and the `pytest_benchmark_update_json` hook reconstructs per-subject median series to fail a session on a sustained regression. Benchmarks are deselected by default (`-m "not benchmark"`) and run in a session separate from `pytest-xdist`.

## [01]-[PACKAGE_SURFACE]

- package: `pytest-benchmark` · version `5.2.3` · license `BSD-2-Clause`
- namespace: `pytest_benchmark`; `pytest11` entry point `benchmark = pytest_benchmark.plugin`; console `pytest-benchmark`
- asset: `pytest_benchmark/{fixture,session,stats,hookspec,storage}.py`; the `benchmark` fixture
- rail: the measurement lane — `benchmark`/`benchmark.pedantic` time a subject, autosave persists the series, and the update-json hook gates regressions

## [02]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]           | [KIND]         | [CAPABILITY]                                                                                                     |
| :-----: | :----------------- | :------------- | :--------------------------------------------------------------------------------------------------------------- |
|  [01]   | `BenchmarkFixture` | fixture object | the injected `benchmark`; carries `group`, `extra_info`, `stats`, and the `pedantic`/`weave`/`patch` runners     |
|  [02]   | `Stats`            | stat carrier   | robust fields `min`/`max`/`mean`/`stddev`/`median`/`iqr`/`q1`/`q3`/`ops`/`rounds`/`total` plus outlier counts    |
|  [03]   | `Metadata`         | stats wrapper  | holds the `Stats` under `.stats`; `as_dict()` projects the JSON entry, `has_error` flags a failed subject        |
|  [04]   | `BenchmarkSession` | session        | aggregates fixtures, resolves the storage URI, and generates/compares the run JSON                               |
|  [05]   | `FileStorage`      | storage        | `file://` backend that writes `<machine>/NNNN_<name>.json` under the storage root; eager `mkdir` on construction |

```python contract
class BenchmarkFixture:
    group: str | None; extra_info: dict[str, object]; stats: Metadata | None       # stats is None until a run completes
    def __call__(self, function_to_benchmark: Callable[..., R], *args: object, **kwargs: object) -> R: ...
    def pedantic(self, target: Callable[..., R], args: tuple = (), kwargs: dict | None = None, setup: Callable | None = None,
                 teardown: Callable | None = None, rounds: int = 1, warmup_rounds: int = 0, iterations: int = 1) -> R: ...
class Stats:  # median, iqr, ops, ... read as attributes after a run
    min: float; median: float; mean: float; iqr: float; ops: float; rounds: int
```

## [03]-[ENTRYPOINTS]

| [INDEX] | [SURFACE]                                                       | [KIND]        | [CAPABILITY]                                                                                       |
| :-----: | :-------------------------------------------------------------- | :------------ | :------------------------------------------------------------------------------------------------- |
|  [01]   | `benchmark(fn, *args, **kwargs)`                                | fixture call  | runs `fn` under an autocalibrated round/iteration schedule and returns its result                  |
|  [02]   | `benchmark.pedantic(...)`                                       | fixture call  | explicit `rounds`/`iterations`/`warmup_rounds` with a per-round `setup` closure for fresh payloads |
|  [03]   | `benchmark.extra_info` / `benchmark.group`                      | metadata slot | arbitrary keys and the storage-series group folded into the JSON entry                             |
|  [04]   | `--benchmark-storage`                                           | CLI option    | run-storage URI (`file://path` or `elasticsearch+http://...`); default `file://./.benchmarks`      |
|  [05]   | `--benchmark-autosave` / `--benchmark-save=NAME`                | CLI option    | persist the current run to storage; autosave writes only when benchmarks ran, save requires a name |
|  [06]   | `--benchmark-compare` / `--benchmark-compare-fail`              | CLI option    | diff against a stored run and fail on a threshold breach (`min:5%`, `mean:0.001`)                  |
|  [07]   | `--benchmark-disable` / `--benchmark-only` / `--benchmark-skip` | CLI flag      | run subjects without timing / run only benchmarks / skip all benchmarks                            |
|  [08]   | `pytest_benchmark_update_json`                                  | hookspec      | mutate the final `output_json` after a run; the regression gate reads the series here              |

```python contract
def pytest_benchmark_update_json(config: pytest.Config, benchmarks: object, output_json: dict[str, object]) -> None: ...
# output_json shape: {"machine_info": {...}, "commit_info": {...}, "datetime": str, "version": str,
#   "benchmarks": [{"fullname": str, "name": str, "group": str | None, "params": dict, "extra_info": dict,
#                   "stats": {"min": float, "median": float, "mean": float, "iqr": float, "ops": float, "rounds": int}}]}
```

## [04]-[IMPLEMENTATION_LAW]

[PYTEST_BENCHMARK_TOPOLOGY]:
- The `benchmark` fixture times a subject once per fixture request; `pedantic` is the only path with a per-round `setup`, which the testkit uses to rebuild mutating payloads.
- `stats` is `None` until the run finishes, then exposes robust statistics; `benchmark.group` set before `pedantic` becomes the storage-series key, so late assignment drops the series.
- `--benchmark-autosave` persists one JSON document per run under `<storage-root>/<machine>/`; the sustained-regression fold reads the ordered set of those documents plus the current report.

[STACKING]:
- `bench.py`(`../_testkit/bench.py`): `BenchCase` rows carry `budget_ms`/`gate_stat`/`max_rel_iqr`; `run_bench` drives `pedantic`, writes `extra_info`, and skips or fails on dispersion or budget; `pytest_benchmark_update_json` folds `_series_from_storage` medians through the Potts/BIC step detector.
- `conftest.py`(`../tools/assay/conftest.py`): `pytest_configure` rebinds the repo-root default `file://./.benchmarks` to the canonical `BENCHMARK_STORAGE_URI` before `BenchmarkSession` eagerly builds its `FileStorage`, so an ad-hoc `-o addopts=` escape never litters the root.
- `pyproject.toml`(`../../../pyproject.toml`): `addopts` pins `--benchmark-storage=file://.artifacts/python/benchmarks` and `--benchmark-autosave`; `-m "not benchmark"` deselects the lane and `filterwarnings` ignores the autosave `PytestBenchmarkWarning` in benchmark-free sessions.

[LOCAL_ADMISSION]:
- Admitted at the shared test tier through the `pytest11` entry point; the plugin auto-disables under `pytest-xdist`, so benchmark and parallel runs stay in separate sessions and default `addopts` carries no `-n`.
- `required_plugins` lists `pytest-benchmark`; the testkit registers its `bench` module as `testkit-bench` from `runtime.py` only when the update-json hook is present.

[RAIL_LAW]:
- Package: `pytest-benchmark`
- Owns: the timed measurement, robust-statistics fold, run-JSON persistence, and the storage/compare/update-json surface.
- Accept: `BenchCase` rows over ad-hoc `benchmark()` calls; `pedantic` with a `setup` closure for fresh-per-round payloads; `extra_info`/`group` as the series metadata; the update-json hook for regression logic.
- Reject: benchmarks in a `pytest-xdist` session; timing assertions in the default lane (deselected by `-m "not benchmark"`); the repo-root storage default; late `benchmark.group` assignment that drops the series key.
