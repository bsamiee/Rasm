# [mutmut] — the copied-tree mutation runner the assay gate governs over the Assay suite

`mutmut` 3.x mutates `tools/assay` source, stages each mutant into a copied `mutants/` workdir, and runs the pytest selection against it — a mutant the suite fails to kill is a survivor, a gap in the tests. It is CLI-first: the run is `mutmut run`, results and triage read the persisted cache, and all policy lives in `[tool.mutmut]` in `pyproject.toml`. The Rasm mutation lane is not a bare `mutmut run` — assay stages the workdir, feeds mutmut an absolute-keyed coverage side-file for its covered-line map, governs concurrency, and grades the cache through `mutation_gate.py`.

## [01]-[PACKAGE_SURFACE]

- package: `mutmut` · version `3.6.0` · license `BSD-3-Clause`
- namespace: `import mutmut` · CLI `mutmut` · cache reader `mutmut.mutation.data.SourceFileMutationData`
- asset: pure-Python wheel; config parsed from `[tool.mutmut]` via `tomllib`; cache under `mutants/`
- rail: mutation gate — the survivor census `tools/assay/rails/mutation_gate.py` grades

## [02]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]                                                          | [KIND]        | [CAPABILITY]                                                                                                         |
| :-----: | :---------------------------------------------------------------- | :------------ | :------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Config`                                                          | dataclass     | the resolved `[tool.mutmut]` policy; `Config.get()` is the singleton accessor                                        |
|  [02]   | `SourceFileMutationData`                                          | cache model   | per-file mutant results; `.load()` faults on corrupt/version-skewed/io cache                                         |
|  [03]   | `status_by_exit_code`                                             | dict          | exit code → status: `killed`/`survived`/`no tests`/`timeout`/`suspicious`/`skipped`/`segfault`/`not checked`/`caught by type check`/`check was interrupted by user` |
|  [04]   | `walk_mutatable_files` · `orig_function_and_class_names_from_key` | cache walkers | enumerate mutants and decode a mutant key back to its origin function; both live on `mutmut.__main__` |

```python contract
@dataclass
class Config:
    source_paths: list[Path]; only_mutate: list[str]; do_not_mutate: list[str]; do_not_mutate_patterns: list[str]
    also_copy: list[Path]; pytest_add_cli_args: list[str]; pytest_add_cli_args_test_selection: list[str]
    mutate_only_covered_lines: bool; max_stack_depth: int; debug: bool
    timeout_multiplier: float; timeout_constant: float; type_check_command: list[str]; use_setproctitle: bool
```

## [03]-[ENTRYPOINTS]

| [INDEX] | [SURFACE]                                           | [KIND]        | [CAPABILITY]                                                                                         |
| :-----: | :-------------------------------------------------- | :------------ | :--------------------------------------------------------------------------------------------------- |
|  [01]   | `mutmut run [MUTANT_NAMES...] --max-children N`     | CLI run       | stage `mutants/`, mutate, run the pytest selection; `--max-children` is the only concurrency control |
|  [02]   | `mutmut results --all BOOLEAN`                      | CLI report    | print the per-mutant status roster from the cache                                                    |
|  [03]   | `mutmut show MUTANT_NAME`                           | CLI diff      | the mutated source diff for one mutant                                                               |
|  [04]   | `mutmut apply MUTANT_NAME`                          | CLI patch     | apply a survivor's mutation to real source to inspect the escape                                     |
|  [05]   | `mutmut browse [--show-killed]`                     | CLI TUI       | interactive survivor browser                                                                         |
|  [06]   | `mutmut tests-for-mutant MUTANT_NAME`               | CLI trace     | the tests that exercised a mutant's covered lines                                                    |
|  [07]   | `mutmut print-time-estimates` · `export-cicd-stats` | CLI telemetry | worst-case per-mutant time; CI/CD stats JSON export                                                  |

```python contract
# [tool.mutmut] as the repo carries it — the covered-line map is fed an absolute-keyed coverage side-file.
source_paths = ["tools/assay"]; do_not_mutate = ["tools/assay/__init__.py", "tools/assay/rails/mutation_gate.py"]
pytest_add_cli_args_test_selection = ["tests/python/tools/assay"]; mutate_only_covered_lines = True; max_stack_depth = -1
pytest_add_cli_args = ["--hypothesis-profile=rasm-mutation", "-o", "required_plugins=", "-m", "not benchmark and not network and not subprocess"]
timeout_multiplier = 4.0; timeout_constant = 5.0   # per-mutant cap = (estimated suite time + timeout_constant) * timeout_multiplier
```

## [04]-[IMPLEMENTATION_LAW]

[MUTMUT_TOPOLOGY]:
- Each mutant runs in a copied `mutants/` workdir (`also_copy` seeds `tests/`, `pyproject.toml`, `setup.cfg`); the mutation lives only in the copy, so real source is never touched during a run.
- `subprocess`-marked tests deselect because a child interpreter executes the unmutated tree outside `mutants/` — its pass masks a live mutation; `network`/`benchmark` deselect for socket and timing determinism.
- `mutate_only_covered_lines=True` reads a covered-line map keyed by absolute mutant path; `.config/coverage-mutmut.ini` sets `relative_files = false` because the in-process gather never runs the `combine` step that aliases keys (`.api/coverage.md`) — relative keys collapse the covered set to empty and abort with "no covered mutants".
- Per-mutant wall cap is `(estimated_time_of_tests + timeout_constant) * timeout_multiplier`, replacing the lax `15x` default; `type_check_command` stays unset, so tests are the sole kill mechanism.
- `max_stack_depth = -1` disables the strict stack walk over importlib pseudo-filenames the copied tree produces.
- Concurrency has no config key — `--max-children` is CLI-only (assay-governed); a bare run defaults to `cpu_count`, and the timeout bounds cap it.

[STACKING]:
- `tools/assay/rails/mutation_gate.py`: reads `SourceFileMutationData` and folds `status_by_exit_code` into a score — only `killed` and `survived` enter the denominator; `no tests`/`timeout`/`suspicious` route to structured stderr triage. It is in `do_not_mutate` so the gate never mutates itself.
- `runtime.py`(`../_testkit/runtime.py`): the `rasm-mutation` Hypothesis profile — derandomized and database-free with short traces — selects via `--hypothesis-profile=rasm-mutation` to preserve kill-signal budget across the mutant fan-out.
- `pytest`(`.api/pytest.md`): mutmut disables `pytest-randomly`, so `-o required_plugins=` waives the guard the default session enforces.
- `mutation` marker: the acceptance and survivor-triage laws that prove the gate's own grading.

[LOCAL_ADMISSION]:
- Admitted at the dedicated `[dependency-groups] mutation` tier (`default-groups` includes it); never a runtime or `dev`-tier dependency.
- The assay mutation rail is the sole consumer; `mutmut run` is reached through the assay-staged workdir, never a direct repo-root invocation.

[RAIL_LAW]:
- Package: `mutmut`
- Owns: the copied-tree mutation run over `tools/assay` and its persisted cache — the survivor signal the gate grades.
- Accept: `[tool.mutmut]` policy in `pyproject.toml`; `--max-children` for assay-governed concurrency; the absolute-keyed `.config/coverage-mutmut.ini` for the covered-line map; the cache readers for gating, never stdout scraping.
- Reject: a 2.x `[mutmut] runner=` config; `relative_files = true` for the mutmut coverage side-file (collapses the covered set); a `subprocess`/`network`/`benchmark` test in the mutation selection; mutating `mutation_gate.py` or `__init__.py`.
