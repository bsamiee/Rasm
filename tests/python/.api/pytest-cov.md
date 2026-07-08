# [pytest-cov] — the session driver that runs coverage.py under pytest and gates the total

`pytest-cov` boots a `coverage.Coverage` instance from `[tool.coverage.*]`, starts it before collection, and folds child-process and xdist-worker data into one report at session end. It contributes no coverage vocabulary of its own beyond the `--cov*` CLI: source selection, branch mode, contexts, and the fail-under floor all delegate to coverage.py, which owns the measurement (`.api/coverage.md`). The Rasm suite runs the default lane through this driver; the mutmut lane bypasses it, driving coverage directly against the absolute-path side-file `.config/coverage-mutmut.ini`.

## [01]-[PACKAGE_SURFACE]

- package: `pytest-cov` · version `7.1.0` · license `MIT`
- namespace: `pytest_cov`; `pytest11` entry point `pytest_cov = pytest_cov.plugin`
- asset: `pytest_cov/{plugin,engine}.py`; the `cov` fixture and the `--cov*` option group
- rail: the coverage-session lane — starts `coverage.Coverage` under pytest, drives xdist/subprocess combination, and applies `--cov-fail-under`

## [02]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]                                | [KIND]            | [CAPABILITY]                                                                                  |
| :-----: | :-------------------------------------- | :---------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `CovPlugin`                             | plugin            | owns the session lifecycle; instantiates the engine controller and reports after the yielded `pytest_runtestloop` |
|  [02]   | `TestContextPlugin`                     | plugin            | dynamic-context switcher; labels each line by node and phase when `--cov-context=test`        |
|  [03]   | `StoreReport`                           | argparse action   | accumulates `--cov-report` type/destination pairs into one report map                         |
|  [04]   | `Central` / `DistMaster` / `DistWorker` | engine controller | single-process, xdist-controller, and xdist-worker coverage lifecycles under `engine.py`      |
|  [05]   | `cov`                                   | fixture           | returns the live `coverage.Coverage` object, or `None` when coverage is disabled              |

```python contract
@pytest.fixture
def cov(request: pytest.FixtureRequest) -> coverage.Coverage | None: ...   # the controller's live Coverage, or None
@pytest.fixture
def no_cover() -> None: ...                                                # marker-paired fixture; disables coverage for one test
class TestContextPlugin:
    def switch_context(self, item: pytest.Item, when: str) -> None: ...    # cov.switch_context(f"{item.nodeid}|{when}")
```

## [03]-[ENTRYPOINTS]

| [INDEX] | [SURFACE]                       | [KIND]     | [CAPABILITY]                                                                                                |
| :-----: | :------------------------------ | :--------- | :---------------------------------------------------------------------------------------------------------- |
|  [01]   | `--cov`                         | CLI option | append a source package or path (`nargs='?'`, multi-allowed); bare `--cov` measures everything              |
|  [02]   | `--cov-report`                  | CLI option | emit `term`/`term-missing`/`html`/`xml`/`json`/`lcov`/`annotate`/`markdown`/`markdown-append` (`:DEST` suffix, multi-allowed) |
|  [03]   | `--cov-branch`                  | CLI flag   | enable branch coverage regardless of the config `branch` key                                                |
|  [04]   | `--cov-context`                 | CLI option | dynamic context source; `test` labels each measured line by node and phase                                  |
|  [05]   | `--cov-append`                  | CLI flag   | add to existing coverage data instead of erasing at start                                                   |
|  [06]   | `--cov-config`                  | CLI option | coverage config path; default `.coveragerc`, else coverage discovers `[tool.coverage.*]`                    |
|  [07]   | `--cov-fail-under`              | CLI option | fail the session when total coverage is below `MIN`; overrides the config `fail_under`                      |
|  [08]   | `--no-cov` / `--no-cov-on-fail` | CLI flag   | disable coverage entirely / suppress the report when the run has test failures                              |
|  [09]   | `pytest.mark.no_cover`          | marker     | disable coverage for the marked test                                                                        |

```python contract
# --cov-report destinations resolve to coverage.py report writers:
#   term|term-missing -> Coverage.report ; html -> html_report ; xml -> xml_report
#   json -> json_report ; lcov -> lcov_report ; annotate -> annotate ; markdown|markdown-append -> report(output_format="markdown")
```

## [04]-[IMPLEMENTATION_LAW]

[PYTEST_COV_TOPOLOGY]:
- The engine controller starts a `coverage.Coverage` before collection and stops it inside `CovPlugin.pytest_runtestloop` after the yielded run loop, then calls the report writers named by `--cov-report` against the combined data.
- Under `pytest-xdist`, `DistMaster.configure_node` seeds each worker with `cov_master_host`/`cov_master_topdir`/`cov_master_rsync_roots`; workers construct their coverage with `data_suffix=True`, and the controller combines the per-worker parallel files before reporting.
- Every option not named `--cov*` is coverage.py's: `--cov-config` selects the file, but source, branch, contexts, exclusions, and the report shape are read from `[tool.coverage.*]`.

[STACKING]:
- `coverage`(`.api/coverage.md`): pytest-cov constructs the `Coverage` object and delegates all measurement, remapping, and report generation to it; `--cov-fail-under` mirrors the config `fail_under = 90`.
- `pyproject.toml`(`../../../pyproject.toml`): `[tool.coverage.run]` sets `source = ["tools/assay"]`, `branch`, `core = "sysmon"`, and `patch = ["subprocess"]`, so child interpreters auto-measure without pytest-cov's legacy subprocess hook.
- `coverage-mutmut.ini`(`../../../.config/coverage-mutmut.ini`): the mutmut lane runs coverage directly through this absolute-path side-file (`relative_files = false`), never through the pytest-cov session; mutmut consumes its absolute-keyed covered-line map.

[LOCAL_ADMISSION]:
- Admitted at the shared test tier through the `pytest11` entry point; `required_plugins` lists `pytest-cov`, so a coverage run cannot start without the driver present.
- `--cov` rides the assay quality rail, not default `addopts`; a bare pytest run measures nothing until the rail supplies the source scope.

[RAIL_LAW]:
- Package: `pytest-cov`
- Owns: the coverage session lifecycle under pytest, xdist worker seeding and combination, dynamic test contexts, and the fail-under gate.
- Accept: `[tool.coverage.*]` as the source of truth; `--cov`/`--cov-report`/`--cov-context` as the session driver; the `cov` fixture for a test that asserts on live coverage state.
- Reject: coverage policy duplicated in CLI flags where the config already carries it; pytest-cov driving the mutmut lane (mutmut owns coverage directly through the absolute side-file); a report writer reinvented where `--cov-report` names one.
