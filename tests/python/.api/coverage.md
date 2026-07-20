# [PY_TESTS_API_COVERAGE]

`coverage.py` records executed lines and branches through a pluggable core, stores them in a SQLite `CoverageData` file, combines parallel data across processes, and writes seven report formats. CPython 3.15 runs the `sysmon` core (`sys.monitoring`), the C tracer being absent on the beta interpreter. `pytest-cov` drives it as a session (`.api/pytest-cov.md`); the mutmut lane drives it against `.config/coverage-mutmut.ini` for an absolute-keyed covered-line map. `patch = ["subprocess"]` auto-measures child interpreters through a shipped `.pth` and the `COVERAGE_PROCESS_CONFIG` handshake.

## [01]-[PACKAGE_SURFACE]

- package: `coverage` · version `7.15.0` · license `Apache-2.0`
- namespace: `import coverage`; console `coverage`; `coverage.Coverage` is the API root
- asset: `coverage/{control,sqldata,sysmon,patch,jsonreport,lcovreport,xmlreport}.py`; shipped `a1_coverage.pth`
- rail: the measurement substrate — `Coverage` collects and reports; `CoverageData` is the combinable SQLite store both the coverage session and mutmut read

## [02]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]                      | [KIND]     | [CAPABILITY]                                                                             |
| :-----: | :---------------------------- | :--------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `Coverage`                    | class      | the API root; start/stop collection, save, combine, and drive every report writer        |
|  [02]   | `CoverageData`                | class      | the SQLite line/arc store; add, query, combine, and serialize measured data and contexts |
|  [03]   | `CoveragePlugin`              | base class | file-tracer/reporter/configurer plugin contract resolved by name in config               |
|  [04]   | `FileTracer` / `FileReporter` | base class | per-file tracing and reporting hooks a plugin implements for a non-Python source         |
|  [05]   | `CodeRegion`                  | dataclass  | a named code region (function or class) for region-scoped reporting                      |
|  [06]   | `CoverageException`           | exception  | the base for coverage errors, including `ConfigError` and `NoDataError`                  |

```python signature
class Coverage:
    def __init__(self, data_file: FilePath | DefaultValue | None = ..., data_suffix: str | bool | None = None,
                 cover_pylib: bool | None = None, auto_data: bool = False, timid: bool | None = None, branch: bool | None = None,
                 config_file: FilePath | bool = True, source: Iterable[str] | None = None, source_pkgs: Iterable[str] | None = None,
                 source_dirs: Iterable[str] | None = None, omit: str | Iterable[str] | None = None, include: str | Iterable[str] | None = None,
                 debug: Iterable[str] | None = None, concurrency: str | Iterable[str] | None = None, check_preimported: bool = False,
                 context: str | None = None, messages: bool = False, plugins: Iterable[Callable[..., None]] | None = None) -> None: ...
class CoverageData:
    def add_lines(self, line_data: Mapping[str, Collection[int]]) -> None: ...
    def lines(self, filename: str) -> list[int] | None: ...          # exact-key lookup; absolute vs relative keys never alias outside combine
    def measured_files(self) -> set[str]: ...
    def update(self, other_data: CoverageData, map_path: Callable[[str], str] | None = None) -> None: ...
```

## [03]-[ENTRYPOINTS]

| [INDEX] | [SURFACE]                                                    | [KIND]   | [CAPABILITY]                                                  |
| :-----: | :----------------------------------------------------------- | :------- | :------------------------------------------------------------ |
|  [01]   | `start` / `stop`                                             | method   | begin and end collection on the active core                   |
|  [02]   | `save` / `load` / `erase`                                    | method   | flush data to the store, reload it, or clear it               |
|  [03]   | `combine`                                                    | method   | merge parallel files; `[tool.coverage.paths]` aliases apply   |
|  [04]   | `report`                                                     | method   | write the terminal summary and return the total percentage    |
|  [05]   | `json_report` / `xml_report` / `lcov_report` / `html_report` | method   | emit the machine-readable and browsable report formats        |
|  [06]   | `switch_context`                                             | method   | tag subsequent lines with a dynamic context label             |
|  [07]   | `get_data` / `analysis2` / `get_option` / `set_option`       | method   | reach live `CoverageData`, per-file analysis, resolved config |
|  [08]   | `process_startup`                                            | function | child-process entry the `.pth` invokes on interpreter boot    |

| [INDEX] | [KEY]                             | [SCOPE]                       | [CAPABILITY]                                                      |
| :-----: | :-------------------------------- | :---------------------------- | :---------------------------------------------------------------- |
|  [01]   | `core = "sysmon"`                 | `[run]`                       | select the `sys.monitoring` core; C tracer absent on 3.15 beta    |
|  [02]   | `patch = ["subprocess"]`          | `[run]`                       | children auto-measure; forces parallel data files                 |
|  [03]   | `relative_files`                  | `[run]`                       | repo-relative keys so parallel files combine; mutmut sets `false` |
|  [04]   | `branch` / `source` / `data_file` | `[run]`                       | branch mode, the measured roots, and the store location           |
|  [05]   | `fail_under`                      | `[report]`                    | total-percentage floor; `show_missing`/`skip_covered` shape it    |
|  [06]   | `exclude_also`                    | `[report]`                    | extra exclusion regexes added to the built-in defaults            |
|  [07]   | `output`                          | `[json]` / `[xml]` / `[lcov]` | per-format report file under `.artifacts/python/coverage`         |
|  [08]   | `directory`                       | `[html]`                      | the browsable html report tree root                               |

```python signature
def process_startup(*, force: bool = False, slug: str = "default") -> Coverage | None: ...  # reads COVERAGE_PROCESS_START / COVERAGE_PROCESS_CONFIG
# apply_patches dispatches on the [run] patch list: "subprocess" sets COVERAGE_PROCESS_CONFIG; "execv"/"fork" wrap the exec/fork family.
```

## [04]-[IMPLEMENTATION_LAW]

[COVERAGE_TOPOLOGY]:
- `Coverage` composes a collector over a core, a `CoverageData` SQLite store, and a report layer; the core is `sysmon` on CPython 3.15 because the compiled C tracer needs a stable ABI.
- `CoverageData.lines(path)` is an exact-key lookup: absolute and relative filenames never alias except through the `combine` step, so a covered-line map keyed one way collapses to empty when read the other.
- `patch = ["subprocess"]` writes `COVERAGE_PROCESS_CONFIG` into the child environment; the shipped `a1_coverage.pth` calls `process_startup` on interpreter boot, producing parallel data files that a later `combine` merges.

[STACKING]:
- `pytest-cov`(`.api/pytest-cov.md`): the coverage session constructs a `Coverage` from `[tool.coverage.*]`, drives `start`/`stop`, and calls the report writers named by `--cov-report`.
- `coverage-mutmut.ini`(`../../../.config/coverage-mutmut.ini`): the mutmut lane runs an in-process `Coverage(data_file=None)` under the `mutants/` cwd with `relative_files = false`, so `CoverageData.lines(abs)` resolves the absolute mutant path mutmut's covered-line map requires.
- `pyproject.toml`(`../../../pyproject.toml`): `[tool.coverage.run]` pins `core`, `patch`, `branch`, and `relative_files`; `[tool.coverage.report]` sets `fail_under = 90` and `exclude_also`; the json/html/xml/lcov tables route outputs into the artifact tree.

[LOCAL_ADMISSION]:
- Admitted at the shared test tier as the measurement substrate; no suite imports `coverage` directly in the default lane — pytest-cov owns the session and mutmut owns the direct in-process gather.
- `sysmon` is the mandated core while the interpreter is a 3.15 beta; the floor stays at `90` because `sysmon` captures fewer arcs than the unavailable C core.

[RAIL_LAW]:
- Package: `coverage`
- Owns: line/branch collection, the combinable SQLite data store, subprocess and exec/fork auto-measurement, dynamic contexts, and the seven report writers.
- Accept: `[tool.coverage.*]` as the single config surface for the default lane; the absolute-keyed side-file for the mutmut gather; `patch = ["subprocess"]` for child auto-measurement; `combine` before any report over parallel data.
- Reject: a hand-rolled tracer where a `CoveragePlugin` file-tracer fits; relative-keyed data feeding mutmut's absolute covered-line lookup; a report reader that scrapes formatted output where `CoverageData`/`json_report` expose it structurally.
