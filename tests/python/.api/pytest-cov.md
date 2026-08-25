# [PY_TESTS_API_PYTEST_COV]

`pytest-cov` boots a `coverage.Coverage` instance from `[tool.coverage.*]`, starts it before collection, and folds child-process and xdist-worker data into one report at session end. It contributes no coverage vocabulary of its own beyond the `--cov*` CLI: source selection, branch mode, contexts, and the fail-under floor all delegate to coverage.py, which owns the measurement (`.api/coverage.md`). Rasm's default lane runs through this driver.

## [01]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]            | [KIND]            | [CAPABILITY]                                                               |
| :-----: | :------------------ | :---------------- | :------------------------------------------------------------------------- |
|  [01]   | `CovPlugin`         | plugin            | owns the session lifecycle; reports after the yielded `pytest_runtestloop` |
|  [02]   | `TestContextPlugin` | plugin            | keys each measured line by node and phase under `--cov-context=test`       |
|  [03]   | `StoreReport`       | argparse action   | accumulates `--cov-report` type/destination pairs into one report map      |
|  [04]   | `Central`           | engine controller | single-process coverage lifecycle in `engine.py`                           |
|  [05]   | `DistMaster`        | engine controller | xdist controller; seeds each worker, combines the per-worker data          |
|  [06]   | `DistWorker`        | engine controller | xdist worker; measures with `data_suffix=True` parallel files              |
|  [07]   | `cov`               | fixture           | the live `coverage.Coverage`, or `None` when coverage is disabled          |

```python
@pytest.fixture
def cov(request: pytest.FixtureRequest) -> coverage.Coverage | None: ...
@pytest.fixture
def no_cover() -> None: ...
class TestContextPlugin:
    def switch_context(self, item: pytest.Item, when: str) -> None: ...
```

## [02]-[ENTRYPOINTS]

| [INDEX] | [SURFACE]                       | [KIND]     | [CAPABILITY]                                                                 |
| :-----: | :------------------------------ | :--------- | :--------------------------------------------------------------------------- |
|  [01]   | `--cov`                         | CLI option | append a source package/path (`nargs='?'`, multi); bare `--cov` measures all |
|  [02]   | `--cov-report`                  | CLI option | emit one or more report types, each with optional `:DEST`; multi-allowed     |
|  [03]   | `--cov-branch`                  | CLI flag   | enable branch coverage regardless of the config `branch` key                 |
|  [04]   | `--cov-context`                 | CLI option | dynamic context source; `test` keys each line by node and phase              |
|  [05]   | `--cov-append`                  | CLI flag   | add to existing coverage data instead of erasing at start                    |
|  [06]   | `--cov-config`                  | CLI option | config path; default `.coveragerc`, else discovers `[tool.coverage.*]`       |
|  [07]   | `--cov-fail-under`              | CLI option | fail when total coverage < `MIN`; overrides the config `fail_under`          |
|  [08]   | `--no-cov` / `--no-cov-on-fail` | CLI flag   | disable coverage entirely / suppress the report on test failures             |
|  [09]   | `pytest.mark.no_cover`          | marker     | disable coverage for the marked test                                         |

```python
```

## [03]-[IMPLEMENTATION_LAW]

[PYTEST_COV_TOPOLOGY]:
- Engine controller starts a `coverage.Coverage` before collection and stops it inside `CovPlugin.pytest_runtestloop` after the yielded run loop, then calls the report writers named by `--cov-report` against the combined data.
- Under `pytest-xdist`, `DistMaster.configure_node` seeds each worker with `cov_master_host`/`cov_master_topdir`/`cov_master_rsync_roots`; workers construct their coverage with `data_suffix=True`, and the controller combines the per-worker parallel files before reporting.
- Every option not named `--cov*` is coverage.py's: `--cov-config` selects the file, but source, branch, contexts, exclusions, and the report shape are read from `[tool.coverage.*]`.

[STACKING]:
- `coverage`(`.api/coverage.md`): pytest-cov constructs the `Coverage` object and delegates all measurement, remapping, and report generation to it; `--cov-fail-under` mirrors the config `fail_under = 90`.

[LOCAL_ADMISSION]:
- Admitted at the shared test tier through the `pytest11` entry point; `required_plugins` lists `pytest-cov`, so a coverage run cannot start without the driver present.
