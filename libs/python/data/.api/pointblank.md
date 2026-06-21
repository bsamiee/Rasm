# [PY_DATA_API_POINTBLANK]

`pointblank` supplies the graded data-quality validation surface for the data grading rail: a `Validate` workflow that chains `col_vals_*`, `col_schema_match`, `rows_distinct`, and `row_count_match` validation steps against a Narwhals-backed frame (Polars/Pandas/Ibis/DuckDB/CSV/Parquet), interrogates the plan with `interrogate()`, and emits a `great_tables.GT` renderable report through `get_tabular_report()`. `Thresholds` and `Actions` grade each step at warning/error/critical severity; the boolean rails `all_passed`, `n_passed`/`f_passed`, and `above_threshold`/`assert_below_threshold` reduce the interrogated plan to a pass/fail grade. The package owner composes `Validate`, `Thresholds`, and `get_tabular_report` into the `PROFILE_POINTBLANK_GRADE` path; it consumes the in-package step algebra and severity grading rather than re-implementing a row-level assertion engine, and routes the emitted `GT` frame to the renderer.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pointblank`
- package: `pointblank`
- import: `pointblank`
- owner: `data`
- rail: grade
- installed: `0.24.0` reflected via `python -c "import pointblank"` on cp315
- entry points: console script `pb` (`pointblank.cli:cli`); library use is import-only
- capability: chained validation plan over Narwhals-backed frames (Polars/Pandas/Ibis/DuckDB/CSV/Parquet/database connection), per-step warning/error/critical thresholds with severity actions, column and row-level assertions, schema match, AI-driven `prompt`/`specially`/`conjointly` steps, `great_tables.GT` tabular/step/dataframe/JSON reports, table profiling via `DataScan`/`col_summary_tbl`/`missing_vals_tbl`, sundered pass/fail splitting, YAML-driven validation, and synthetic dataset generation from a `Schema`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: validation workflow, grading, schema, and profiling roots
- rail: grade

`Validate` is the workflow root; validation-step methods return `Validate` for chaining and `interrogate()` populates the reporting data. `Thresholds` grades failing test units at three severity levels and `Actions`/`FinalActions` bind callables to a severity. `Schema` declares column names and dtypes for `col_schema_match` and synthetic generation, and the `Field` family declares per-column generation constraints. `DataScan` profiles a table independent of a validation plan.

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]    | [RAIL]                                                       |
| :-----: | :---------------- | :--------------- | :----------------------------------------------------------- |
|  [01]   | `Validate`        | workflow         | validation-plan root that interrogates and reports a grade   |
|  [02]   | `Thresholds`      | grading          | warning/error/critical failing-unit limits                   |
|  [03]   | `Actions`         | grading          | per-severity callables fired when a step threshold is met    |
|  [04]   | `FinalActions`    | grading          | callables fired once after interrogation completes           |
|  [05]   | `Schema`          | schema           | column-name and dtype declaration for match and generation   |
|  [06]   | `DataScan`        | profiling        | standalone table profile with tabular and JSON reports       |
|  [07]   | `GeneratorConfig` | generation       | synthetic-dataset row count, seed, locale, and output policy |
|  [08]   | `DraftValidation` | authoring        | AI-drafted validation plan from a table and model            |
|  [09]   | `Field`           | generation value | base per-column synthetic-generation constraint              |
|  [10]   | `IntField`        | generation value | integer column constraint (`min_val`/`max_val`/`allowed`)    |
|  [11]   | `FloatField`      | generation value | float column constraint                                      |
|  [12]   | `StringField`     | generation value | string column constraint (`pattern`/`preset`/`min_length`)   |
|  [13]   | `BoolField`       | generation value | boolean column constraint                                    |
|  [14]   | `DateField`       | generation value | date column constraint                                       |
|  [15]   | `DatetimeField`   | generation value | datetime column constraint                                   |
|  [16]   | `TimeField`       | generation value | time column constraint                                       |
|  [17]   | `DurationField`   | generation value | duration column constraint                                   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Validate` plan, interrogate, and grade
- rail: grade

`Validate(data, ...)` opens the plan; each validation method appends a step and returns `Validate` so steps chain, and every step shares `thresholds`, `actions`, `brief`, and `active` policy plus `pre`/`segments` where applicable. `interrogate()` acts on the plan and the boolean/fraction rails reduce it to a grade; `get_tabular_report()` emits the renderable `GT` frame.

| [INDEX] | [SURFACE]                         | [CALL_SHAPE]                                                                                                                                                                                 | [CAPABILITY]                                  |
| :-----: | :-------------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `Validate`                        | `Validate(data, reference=None, tbl_name=None, label=None, thresholds=None, actions=None, final_actions=None, brief=None, lang=None, locale=None, owner=None, consumers=None, version=None)` | open a validation plan over a target table    |
|  [02]   | `Validate.interrogate`            | `interrogate(collect_extracts=True, collect_tbl_checked=True, get_first_n=None, sample_n=None, sample_frac=None, extract_limit=500) -> Validate`                                             | execute the plan and populate reporting data  |
|  [03]   | `Validate.get_tabular_report`     | `get_tabular_report(title=':default:', incl_header=None, incl_footer=None, incl_footer_timings=None, incl_footer_notes=None) -> GT`                                                          | emit the renderable `GT` grade report         |
|  [04]   | `Validate.get_step_report`        | `get_step_report(i, columns_subset=None, header=':default:', limit=10) -> GT`                                                                                                                | emit a `GT` drill-down report for one step    |
|  [05]   | `Validate.get_json_report`        | `get_json_report(use_fields=None, exclude_fields=None) -> str`                                                                                                                               | serialize the interrogated plan to JSON       |
|  [06]   | `Validate.get_dataframe_report`   | `get_dataframe_report(tbl_type='polars') -> Any`                                                                                                                                             | report as a Polars/Pandas/DuckDB frame        |
|  [07]   | `Validate.get_sundered_data`      | `get_sundered_data(type='pass') -> Any`                                                                                                                                                      | split the table into passing/failing rows     |
|  [08]   | `Validate.all_passed`             | `all_passed() -> bool`                                                                                                                                                                       | grade: every step passed                      |
|  [09]   | `Validate.n_passed`               | `n_passed(i=None, scalar=False) -> dict[int, int] \| int`                                                                                                                                    | passing test-unit count per step              |
|  [10]   | `Validate.n_failed`               | `n_failed(i=None, scalar=False) -> dict[int, int] \| int`                                                                                                                                    | failing test-unit count per step              |
|  [11]   | `Validate.f_passed`               | `f_passed(i=None, scalar=False) -> dict[int, float] \| float`                                                                                                                                | passing test-unit fraction per step           |
|  [12]   | `Validate.f_failed`               | `f_failed(i=None, scalar=False) -> dict[int, float] \| float`                                                                                                                                | failing test-unit fraction per step           |
|  [13]   | `Validate.above_threshold`        | `above_threshold(level='warning', i=None) -> bool`                                                                                                                                           | grade: a step breached the named severity     |
|  [14]   | `Validate.assert_below_threshold` | `assert_below_threshold(level='warning', i=None, message=None) -> None`                                                                                                                      | raise when a step breached the named severity |

[ENTRYPOINT_SCOPE]: `Validate` step algebra
- rail: grade

Every step row appends to the plan and returns `Validate`; `columns` accepts a name, list, `col(...)`, or a column selector, and `thresholds`/`actions`/`brief`/`active`/`pre`/`segments` are per-step policy rows. The `col_vals_*` family is one comparison surface discriminated by the operator and value shape, never a parallel step type per operator.

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                                                                                                                                                                                                             | [CAPABILITY]                                        |
| :-----: | :---------------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :-------------------------------------------------- |
|  [01]   | `col_vals_gt`           | `col_vals_gt(columns, value, na_pass=False, pre=None, segments=None, thresholds=None, actions=None, brief=None, active=True) -> Validate`                                                                                | per-cell comparison (`gt`/`ge`/`lt`/`le`/`eq`/`ne`) |
|  [02]   | `col_vals_between`      | `col_vals_between(columns, left, right, inclusive=(True, True), na_pass=False, pre=None, segments=None, thresholds=None, actions=None, brief=None, active=True) -> Validate`                                             | in-range / `col_vals_outside` out-of-range          |
|  [03]   | `col_vals_in_set`       | `col_vals_in_set(columns, set, pre=None, segments=None, thresholds=None, actions=None, brief=None, active=True) -> Validate`                                                                                             | membership / `col_vals_not_in_set` exclusion        |
|  [04]   | `col_vals_not_null`     | `col_vals_not_null(columns, pre=None, segments=None, thresholds=None, actions=None, brief=None, active=True) -> Validate`                                                                                                | null / non-null assertion (`col_vals_null`)         |
|  [05]   | `col_vals_regex`        | `col_vals_regex(columns, pattern, na_pass=False, inverse=False, pre=None, segments=None, thresholds=None, actions=None, brief=None, active=True) -> Validate`                                                            | pattern match per cell                              |
|  [06]   | `col_vals_within_spec`  | `col_vals_within_spec(columns, spec, na_pass=False, pre=None, segments=None, thresholds=None, actions=None, brief=None, active=True) -> Validate`                                                                        | named-spec conformance (e.g. email/url)             |
|  [07]   | `col_vals_increasing`   | `col_vals_increasing(columns, allow_stationary=False, decreasing_tol=None, na_pass=False, pre=None, segments=None, thresholds=None, actions=None, brief=None, active=True) -> Validate`                                  | monotonicity (`col_vals_decreasing`)                |
|  [08]   | `col_vals_expr`         | `col_vals_expr(expr, pre=None, segments=None, thresholds=None, actions=None, brief=None, active=True) -> Validate`                                                                                                       | row-wise boolean expression                         |
|  [09]   | `col_exists`            | `col_exists(columns, thresholds=None, actions=None, brief=None, active=True) -> Validate`                                                                                                                                | column presence                                     |
|  [10]   | `col_schema_match`      | `col_schema_match(schema, complete=True, in_order=True, case_sensitive_colnames=True, case_sensitive_dtypes=True, full_match_dtypes=True, pre=None, thresholds=None, actions=None, brief=None, active=True) -> Validate` | match a declared `Schema`                           |
|  [11]   | `col_count_match`       | `col_count_match(count, inverse=False, pre=None, thresholds=None, actions=None, brief=None, active=True) -> Validate`                                                                                                    | column-count assertion                              |
|  [12]   | `row_count_match`       | `row_count_match(count, tol=0, inverse=False, pre=None, thresholds=None, actions=None, brief=None, active=True) -> Validate`                                                                                             | row-count assertion                                 |
|  [13]   | `rows_distinct`         | `rows_distinct(columns_subset=None, pre=None, segments=None, thresholds=None, actions=None, brief=None, active=True) -> Validate`                                                                                        | uniqueness (`rows_complete` for no-null rows)       |
|  [14]   | `col_vals_*` aggregates | `col_avg_gt(columns, value=None, tol=0, thresholds=None, brief=None, actions=None, active=True) -> Validate`                                                                                                             | column aggregate (`avg`/`sum`/`sd` x comparison)    |
|  [15]   | `col_pct_null`          | `col_pct_null(columns, p, tol=0, thresholds=None, actions=None, brief=None, active=True) -> Validate`                                                                                                                    | null-fraction assertion                             |
|  [16]   | `tbl_match`             | `tbl_match(tbl_compare, pre=None, thresholds=None, actions=None, brief=None, active=True) -> Validate`                                                                                                                   | whole-table equality against a comparison table     |
|  [17]   | `conjointly`            | `conjointly(*exprs, pre=None, thresholds=None, actions=None, brief=None, active=True) -> Validate`                                                                                                                       | row passes only when all expressions pass           |
|  [18]   | `specially`             | `specially(expr, pre=None, thresholds=None, actions=None, brief=None, active=True) -> Validate`                                                                                                                          | arbitrary table-level callable step                 |
|  [19]   | `prompt`                | `prompt(prompt, model, columns_subset=None, batch_size=1000, max_concurrent=3, pre=None, segments=None, thresholds=None, actions=None, brief=None, active=True) -> Validate`                                             | LLM-graded per-row assertion                        |

[ENTRYPOINT_SCOPE]: grading, profiling, schema, and I/O functions
- rail: grade

`Thresholds`/`Actions` configure grading; `DataScan`, `col_summary_tbl`, `missing_vals_tbl`, and `preview` emit `GT` frames without a validation plan. `load_dataset`/`generate_dataset` supply input tables, `connect_to_table` opens a database table, and `yaml_interrogate`/`read_file` rebuild a `Validate` from a declarative source.

| [INDEX] | [SURFACE]          | [CALL_SHAPE]                                                                                                                                                             | [CAPABILITY]                                      |
| :-----: | :----------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------ |
|  [01]   | `Thresholds`       | `Thresholds(warning=None, error=None, critical=None)`                                                                                                                    | severity limits as counts or fractions            |
|  [02]   | `Actions`          | `Actions(warning=None, error=None, critical=None, default=None, highest_only=True)`                                                                                      | per-severity callables fired on breach            |
|  [03]   | `FinalActions`     | `FinalActions(*args)`                                                                                                                                                    | callables fired once after interrogation          |
|  [04]   | `DataScan`         | `DataScan(data, tbl_name=None)`; `.get_tabular_report(show_sample_data=False) -> GT`; `.to_json() -> str`; `.summary_data`                                               | standalone table profile and reports              |
|  [05]   | `col_summary_tbl`  | `col_summary_tbl(data, tbl_name=None) -> GT`                                                                                                                             | per-column summary `GT` frame                     |
|  [06]   | `missing_vals_tbl` | `missing_vals_tbl(data) -> GT`                                                                                                                                           | missing-value `GT` frame                          |
|  [07]   | `preview`          | `preview(data, columns_subset=None, n_head=5, n_tail=5, limit=50, show_row_numbers=True, max_col_width=250, min_tbl_width=500, incl_header=None) -> GT`                  | head/tail preview `GT` frame                      |
|  [08]   | `get_row_count`    | `get_row_count(data) -> int`; `get_column_count(data) -> int`                                                                                                            | frame shape                                       |
|  [09]   | `load_dataset`     | `load_dataset(dataset='small_table', tbl_type='polars') -> Any`; `get_data_path(dataset='small_table', file_type='csv') -> str`                                          | built-in datasets and on-disk paths               |
|  [10]   | `generate_dataset` | `generate_dataset(schema, n=100, seed=None, output='polars', country='US', shuffle=True, weighted=True) -> Any`                                                          | synthetic table from a `Schema`                   |
|  [11]   | `connect_to_table` | `connect_to_table(connection_string) -> Any`; `print_database_tables(connection_string) -> list[str]`                                                                    | open a database-backed table                      |
|  [12]   | `yaml_interrogate` | `yaml_interrogate(yaml, set_tbl=None, namespaces=None) -> Validate`; `validate_yaml(yaml) -> None`; `yaml_to_python(yaml) -> str`                                        | build and interrogate a plan from YAML            |
|  [13]   | `read_file`        | `read_file(filepath) -> Validate`; `write_file(validation, filename, path=None, keep_tbl=False, keep_extracts=False, quiet=False) -> None`                               | persist and reload an interrogated plan           |
|  [14]   | `col`              | `col(exprs) -> Column`; `ref(column_name) -> ReferenceColumn`; `expr_col(column_name) -> ColumnExpression`                                                               | column reference for step targets and comparisons |
|  [15]   | column selectors   | `starts_with(text, case_sensitive=False)`; `ends_with(...)`; `contains(...)`; `matches(pattern, ...)`; `everything()`; `first_n(n, offset=0)`; `last_n(n, offset=0)`     | resolve a column set for `columns`                |
|  [16]   | `config`           | `config(report_incl_header=True, report_incl_footer=True, report_incl_footer_timings=True, report_incl_footer_notes=True, preview_incl_header=True) -> PointblankConfig` | global report/preview defaults                    |

## [04]-[IMPLEMENTATION_LAW]

[GRADE_VALIDATION]:
- import: `import pointblank as pb` at boundary scope only; module-level import is banned by the manifest import policy.
- plan axis: one `Validate` owns the validation plan; each `col_vals_*`/`rows_*`/`col_schema_match`/`row_count_match` call is a step row returning `Validate`, never a per-rule validator object — the plan is a chained fold, not a list of hand-instantiated checks.
- comparison axis: `col_vals_gt`/`ge`/`lt`/`le`/`eq`/`ne`/`between`/`outside`/`in_set`/`not_in_set`/`regex`/`within_spec` is one comparison surface discriminated by operator and value shape; `value` accepts a literal, `col(...)`, or `ref(...)`, never a parallel step type per operator.
- column axis: `columns` resolves a name, list, `col(...)`, or selector (`starts_with`/`ends_with`/`contains`/`matches`/`everything`/`first_n`/`last_n`); selectors are rows, never a hand-built column-name loop.
- grade axis: `Thresholds(warning, error, critical)` grades failing test units at three severity levels as counts or fractions; `Actions` binds callables to a severity and `above_threshold`/`assert_below_threshold` reduce the interrogated plan to a pass/fail grade — `all_passed`/`n_passed`/`f_passed` are the boolean and fractional grade rails.
- interrogate axis: `interrogate()` is the single execution surface that acts on the plan; sampling (`sample_n`/`sample_frac`/`get_first_n`) and extract limits are call rows, never a separate runner type.
- render axis: `get_tabular_report()` emits the `great_tables.GT` grade frame keyed by the interrogated plan; `get_step_report`/`get_dataframe_report`/`get_json_report` are report rows for the same plan, and `col_summary_tbl`/`missing_vals_tbl`/`preview`/`DataScan.get_tabular_report` emit `GT` frames for profiling without a plan — the `GT` object feeds the renderer directly, never a re-rendered HTML table.
- frame axis: `data` accepts a Polars/Pandas/Ibis frame, a CSV/Parquet path or glob, a GitHub URL, or a database connection string through Narwhals; `connect_to_table` and `load_dataset`/`generate_dataset` supply tables — the validation engine is backend-agnostic, never re-implemented per frame library.
- declarative axis: `yaml_interrogate`/`validate_yaml`/`yaml_to_python` build and interrogate a plan from YAML and `read_file`/`write_file` persist an interrogated `Validate`; declarative validation is a source row, never a parallel plan API.
- evidence: each interrogated plan captures step count, per-step passing/failing test-unit counts and fractions, severity grade per level, threshold breach flags, sundered row counts, and the emitted report kind as a grade receipt.
- boundary: pointblank owns the validation plan, severity grading, and `GT` report emission; `great_tables` (`GT` 0.21.0) owns the renderable frame downstream; Narwhals owns frame normalization; the emitted `GT` feeds the document and visuals owners; live UI stays outside this package.

[RAIL_LAW]:
- Package: `pointblank`
- Owns: chained validation-plan authoring over Narwhals-backed frames, warning/error/critical threshold grading with severity actions, column/row/schema/aggregate assertions, table profiling, and `great_tables.GT` report emission
- Accept: graded data-quality validation feeding a renderable `GT` frame to the document, visuals, and grade owners
- Reject: wrapper-renames of `Validate`/`interrogate`/`get_tabular_report`; a hand-rolled row-level assertion engine; a parallel step type per comparison operator or per frame library; a re-rendered HTML table where the in-package `GT` frame needs none; identity or threshold-grade minting the runtime owns
