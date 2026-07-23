# [PY_DATA_API_POINTBLANK]

`pointblank` owns the graded data-quality validation rail: a `Validate` plan chains column, row, schema, and aggregate steps over a Narwhals-backed frame, `interrogate()` executes the fold, and `Thresholds`/`Actions` grade each step at warning/error/critical severity. `Validate` reduces the interrogated plan to a boolean/fractional pass-fail grade and emits a `great_tables.GT` report; the profile plane folds this surface into the `PROFILE_POINTBLANK_GRADE` path, while frame normalization, contract grading, and render stay downstream owners.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pointblank`
- package: `pointblank`
- module: `pointblank`
- owner: `data`
- license: MIT
- rail: grade
- asset: pure Python

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: validation, grading, declarative-plan, schema, profiling, and generation roots

Per-column generation constraints carry two equivalent idioms — the `IntField`/`FloatField`/`StringField`/`BoolField`/`DateField`/`DatetimeField`/`TimeField`/`DurationField` classes and the mirror `int_field`/`float_field`/`string_field`/`bool_field`/`date_field`/`datetime_field`/`time_field`/`duration_field` helpers — either idiom composes inside a `Schema` column spec.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]    | [CAPABILITY]                                                           |
| :-----: | :----------------------------- | :--------------- | :--------------------------------------------------------------------- |
|  [01]   | `Validate`                     | workflow         | validation-plan root that interrogates and reports a grade             |
|  [02]   | `Thresholds`                   | grading          | warning/error/critical failing-unit limits                             |
|  [03]   | `Actions`                      | grading          | per-severity callables fired when a step threshold is met              |
|  [04]   | `FinalActions`                 | grading          | callables fired once after interrogation completes                     |
|  [05]   | `Contract`                     | declarative plan | serializable contract bundling a `Validate` plan, metadata, and policy |
|  [06]   | `Step`                         | declarative plan | one declarative validation step inside a `Contract`/`Pipeline`         |
|  [07]   | `Pipeline`                     | declarative plan | ordered multi-table contract execution returning `PipelineResult`      |
|  [08]   | `PipelineResult`               | declarative plan | aggregated per-contract grade from a `Pipeline.run`                    |
|  [09]   | `Schema`                       | schema           | column-name and dtype declaration for match and generation             |
|  [10]   | `DataScan`                     | profiling        | standalone table profile with tabular and JSON reports                 |
|  [11]   | `GeneratorConfig`              | generation       | synthetic-dataset row count, seed, locale, and output policy           |
|  [12]   | `DraftValidation`              | authoring        | AI-drafted validation plan from a table and model                      |
|  [13]   | `Field` / `*Field`             | generation value | per-column generation-constraint classes                               |
|  [14]   | `int_field` … `duration_field` | generation value | snake-case `*_field()` factories mirroring the `*Field` classes        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Validate` plan, interrogate, and grade

`Validate(data, ...)` opens the plan; each validation method appends a step and returns `Validate`, so steps chain under shared `thresholds`/`actions`/`brief`/`active` policy. `interrogate()` executes the fold and the boolean/fraction rails reduce it to a grade; `get_tabular_report()` emits the renderable `GT` frame.
- open: `Validate(data, *, reference, tbl_name, label, thresholds, actions, final_actions, brief, lang, locale, owner, consumers, version) -> Validate`; `interrogate(*, collect_extracts, collect_tbl_checked, get_first_n, sample_n, sample_frac, extract_limit) -> Validate`
- report: `get_tabular_report(*, title, incl_header, incl_footer, incl_footer_timings, incl_footer_notes) -> GT`; `get_step_report(i, *, columns_subset, header, limit) -> GT`; `get_json_report(*, use_fields, exclude_fields) -> str`; `get_dataframe_report(tbl_type) -> Frame`; `get_sundered_data(type) -> Frame`
- grade: `all_passed() -> bool`; `n_passed(i, *, scalar)` / `n_failed` / `f_passed` / `f_failed` -> per-step dict or scalar; `above_threshold(level, i) -> bool`; `assert_below_threshold(level, i, message)`

| [INDEX] | [SURFACE]                         | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :-------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `Validate`                        | ctor     | open a validation plan over a target table    |
|  [02]   | `Validate.interrogate`            | instance | execute the plan and populate reporting data  |
|  [03]   | `Validate.get_tabular_report`     | instance | emit the renderable `GT` grade report         |
|  [04]   | `Validate.get_step_report`        | instance | emit a `GT` drill-down report for one step    |
|  [05]   | `Validate.get_json_report`        | instance | serialize the interrogated plan to JSON       |
|  [06]   | `Validate.get_dataframe_report`   | instance | report as a Polars/Pandas/DuckDB frame        |
|  [07]   | `Validate.get_sundered_data`      | instance | split the table into passing/failing rows     |
|  [08]   | `Validate.all_passed`             | instance | grade: every step passed                      |
|  [09]   | `Validate.n_passed`               | instance | passing test-unit count per step              |
|  [10]   | `Validate.n_failed`               | instance | failing test-unit count per step              |
|  [11]   | `Validate.f_passed`               | instance | passing test-unit fraction per step           |
|  [12]   | `Validate.f_failed`               | instance | failing test-unit fraction per step           |
|  [13]   | `Validate.above_threshold`        | instance | grade: a step breached the named severity     |
|  [14]   | `Validate.assert_below_threshold` | instance | raise when a step breached the named severity |

[ENTRYPOINT_SCOPE]: `Validate` step algebra

Every step row appends to the plan and returns `Validate`; `columns` accepts a name, list, `col(...)`, or a column selector, and `thresholds`/`actions`/`brief`/`active`/`pre`/`segments` are per-step policy rows. `col_vals_*` is one comparison surface discriminated by operator and value shape, never a parallel step type per operator.
- shared tail: every step ends `(..., pre, segments, thresholds, actions, brief, active) -> Validate`; `col_exists`/`col_*_match`/`tbl_match`/`conjointly`/`specially` omit `segments`
- monotonic: `col_vals_increasing(columns, *, allow_stationary, decreasing_tol, na_pass)` mirrors `col_vals_decreasing(columns, *, allow_stationary, increasing_tol, na_pass)` — each direction owns the opposite-slack tolerance
- aggregate: `col_avg_gt`/`col_sum_*`/`col_sd_*` cross `avg`/`sum`/`sd` with `gt`/`ge`/`lt`/`le`/`eq` (no `ne` arm, unlike the per-cell family)
- schema: `col_schema_match(schema, *, complete, in_order, case_sensitive_colnames, case_sensitive_dtypes, full_match_dtypes)`
- prompt: `prompt(prompt, model, *, columns_subset, attachments, batch_size, max_concurrent)`

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                                       | [CAPABILITY]                                        |
| :-----: | :---------------------- | :------------------------------------------------- | :-------------------------------------------------- |
|  [01]   | `col_vals_gt`           | `(columns, value, na_pass=False)`                  | per-cell comparison (`gt`/`ge`/`lt`/`le`/`eq`/`ne`) |
|  [02]   | `col_vals_between`      | `(columns, left, right, inclusive, na_pass=False)` | in-range; `col_vals_outside` out-of-range           |
|  [03]   | `col_vals_in_set`       | `(columns, set)`                                   | membership; `col_vals_not_in_set` exclusion         |
|  [04]   | `col_vals_not_null`     | `(columns)`                                        | null / non-null (`col_vals_null`)                   |
|  [05]   | `col_vals_regex`        | `(columns, pattern, na_pass=False, inverse=False)` | pattern match per cell                              |
|  [06]   | `col_vals_within_spec`  | `(columns, spec, na_pass=False)`                   | named-spec conformance (e.g. email/url)             |
|  [07]   | `col_vals_increasing`   | `(columns, allow_stationary=False, ...)`           | monotonicity; `col_vals_decreasing` mirror          |
|  [08]   | `col_vals_expr`         | `(expr)`                                           | row-wise boolean expression                         |
|  [09]   | `col_exists`            | `(columns)`                                        | column presence                                     |
|  [10]   | `col_schema_match`      | `(schema, complete=True, in_order=True, ...)`      | match a declared `Schema` (flags in `- schema:`)    |
|  [11]   | `col_count_match`       | `(count, inverse=False)`                           | column-count assertion                              |
|  [12]   | `row_count_match`       | `(count, tol=0, inverse=False)`                    | row-count assertion                                 |
|  [13]   | `rows_distinct`         | `(columns_subset=None)`                            | uniqueness (`rows_complete` for no-null rows)       |
|  [14]   | `col_avg_gt` aggregates | `(columns, value=None, tol=0)`                     | column aggregate (see `- aggregate:`)               |
|  [15]   | `col_pct_null`          | `(columns, p, tol=0)`                              | null-fraction assertion                             |
|  [16]   | `tbl_match`             | `(tbl_compare)`                                    | whole-table equality vs a comparison table          |
|  [17]   | `conjointly`            | `(*exprs)`                                         | row passes only when all expressions pass           |
|  [18]   | `specially`             | `(expr)`                                           | arbitrary table-level callable step                 |
|  [19]   | `prompt`                | `(prompt, model, columns_subset=None, ...)`        | LLM-graded per-row assertion                        |

[ENTRYPOINT_SCOPE]: grading, profiling, schema, generation, and I/O functions

`Thresholds`/`Actions` configure grading and `send_slack_notification`/`emit_otel` build action callables fired on breach; `DataScan`, `col_summary_tbl`, `missing_vals_tbl`, and `preview` emit `GT` frames without a validation plan. `load_dataset`/`generate_dataset` supply input tables, `connect_to_table` opens a database table, `schema_from_tbl` infers a `Schema`, and `yaml_interrogate`/`read_file` rebuild a `Validate` from a declarative source; `get_action_metadata`/`get_validation_summary` read interrogation context inside an action callable.
- grade: `Thresholds(warning, error, critical)`, `Actions(warning, error, critical, default, *, highest_only)`, `FinalActions(*args)`; `send_slack_notification(...)`, `emit_otel(...)`, `get_action_metadata() -> dict`, `get_validation_summary() -> dict`
- profile: `DataScan(data, tbl_name)` -> `.get_tabular_report(*, show_sample_data) -> GT` / `.to_json() -> str` / `.summary_data`; `col_summary_tbl(data, tbl_name) -> GT`; `missing_vals_tbl(data) -> GT`; `preview(data, *, columns_subset, n_head, n_tail, limit, ...) -> GT`; `get_row_count(data)` / `get_column_count(data)`
- data: `load_dataset(dataset, tbl_type)` / `get_data_path(dataset, file_type)`; `generate_dataset(schema, *, n, seed, output, country, shuffle, weighted)`; `connect_to_table(connection_string)` / `print_database_tables(connection_string)`; `schema_from_tbl(tbl) -> Schema` / `profile_fields(tbl) -> list[Field]`
- field: `int_field(*, min_val, max_val, allowed, ...)`, `float_field(...)`, `string_field(*, pattern, preset, min_length, ...)`, `bool_field(...)`, `date_field(...)`, `datetime_field(...)`, `time_field(...)`, `duration_field(...) -> Field`; `GeneratorConfig(...)`
- check: `has_columns(columns)` / `has_rows(...)` build standalone existence checks for `specially`/`conjointly`
- declare: `yaml_interrogate(yaml, *, set_tbl, namespaces) -> Validate` / `validate_yaml(yaml)` / `yaml_to_python(yaml) -> str`; `read_file(filepath) -> Validate` / `write_file(validation, filename, *, path, keep_tbl, keep_extracts, quiet)`; `Contract.from_yaml`/`to_yaml`/`from_dict`/`to_dict`/`to_validate`, `Pipeline.run`/`from_yaml`/`to_yaml`
- target: `col(exprs) -> ColumnLiteral` / `ref(column_name) -> ReferenceColumn` / `expr_col(column_name) -> ColumnExpression`; selectors `starts_with`/`ends_with`/`contains`/`matches`/`everything`/`first_n`/`last_n`; `seg_group(columns)`
- misc: `config(...) -> PointblankConfig`; `assistant(...)`, `DraftValidation(tbl, model, *, api_key, ...)`

| [INDEX] | [SURFACE]               | [CAPABILITY]                                                         |
| :-----: | :---------------------- | :------------------------------------------------------------------- |
|  [01]   | `Thresholds`            | severity limits as counts or fractions                               |
|  [02]   | `Actions`               | per-severity callables fired on breach                               |
|  [03]   | `FinalActions`          | callables fired once after interrogation                             |
|  [04]   | `DataScan`              | standalone table profile with tabular and JSON reports               |
|  [05]   | `col_summary_tbl`       | per-column summary `GT` frame                                        |
|  [06]   | `missing_vals_tbl`      | missing-value `GT` frame                                             |
|  [07]   | `preview`               | head/tail preview `GT` frame                                         |
|  [08]   | `get_row_count`         | frame shape (`get_column_count` mirror)                              |
|  [09]   | `load_dataset`          | built-in datasets and on-disk paths                                  |
|  [10]   | `generate_dataset`      | synthetic table from a `Schema` via `*_field()`/`*Field` columns     |
|  [11]   | `*_field` helpers       | per-column generation-constraint factories                           |
|  [12]   | `schema_from_tbl`       | infer a `Schema` / generation fields from a table                    |
|  [13]   | `connect_to_table`      | open a database-backed table                                         |
|  [14]   | `yaml_interrogate`      | build and interrogate a plan from YAML                               |
|  [15]   | `read_file`             | persist and reload an interrogated plan                              |
|  [16]   | `Contract` / `Pipeline` | serializable plan model round-tripping through YAML and dict         |
|  [17]   | `col`                   | column reference for step targets and comparisons                    |
|  [18]   | column selectors        | resolve a column set for `columns`                                   |
|  [19]   | `seg_group`             | declare a multi-column segmentation group for `segments`             |
|  [20]   | action callables        | side-effecting/context helpers fired inside `Actions`/`FinalActions` |
|  [21]   | `assistant`             | LLM-authored validation drafting and chat assistant                  |
|  [22]   | `config`                | global report/preview defaults                                       |
|  [23]   | `has_columns`           | standalone existence check (`has_rows` mirror)                       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- plan axis: one `Validate` owns the plan; each `col_vals_*`/`rows_*`/`col_schema_match`/`row_count_match` call is a step row returning `Validate`, a chained fold, never a per-rule validator object.
- comparison axis: `col_vals_gt`/`ge`/`lt`/`le`/`eq`/`ne`/`between`/`outside`/`in_set`/`not_in_set`/`regex`/`within_spec` is one comparison surface discriminated by operator and value shape; `value` accepts a literal, `col(...)`, or `ref(...)`, never a parallel step type per operator.
- column axis: `columns` resolves a name, list, `col(...)`, or selector (`starts_with`/`ends_with`/`contains`/`matches`/`everything`/`first_n`/`last_n`); selectors are rows, never a hand-built column-name loop.
- grade axis: `Thresholds(warning, error, critical)` grades failing test units at three severity levels as counts or fractions; `Actions`/`FinalActions` bind callables to a severity and `above_threshold`/`assert_below_threshold`/`all_passed`/`n_passed`/`f_passed` reduce the interrogated plan to the boolean and fractional grade.
- action axis: severity callables are the package's own side-effect rail — `send_slack_notification`/`emit_otel` are built `Actions` payloads (the OTel span emits from pointblank), and `get_action_metadata`/`get_validation_summary` read the breaching-step context inside the callable.
- generation axis: `generate_dataset(schema, ...)` synthesizes a table from a `Schema` whose columns are `*_field()` helpers or `*Field` classes with a `GeneratorConfig` policy; `schema_from_tbl`/`profile_fields` round-trip a real frame into a generation `Schema`, so fixtures stack into the same `Validate` plan.
- interrogate axis: `interrogate()` is the single execution surface; sampling (`sample_n`/`sample_frac`/`get_first_n`) and extract limits are call rows, never a separate runner type.
- render axis: `get_tabular_report()` emits the `great_tables.GT` grade frame keyed by the interrogated plan; `get_step_report`/`get_dataframe_report`/`get_json_report` and the plan-free `col_summary_tbl`/`missing_vals_tbl`/`preview`/`DataScan.get_tabular_report` are report rows feeding the renderer directly.
- frame axis: `data` accepts a Polars/Pandas/Ibis frame, a CSV/Parquet path or glob, a GitHub URL, or a database connection string through Narwhals; `connect_to_table` and `load_dataset`/`generate_dataset` supply tables, so the engine is backend-agnostic.
- declarative axis: the typed `Contract`/`Step`/`Pipeline`/`PipelineResult` model round-trips through `from_yaml`/`to_yaml`/`from_dict`/`to_dict` and `Contract.to_validate` lowers to a `Validate` plan; `yaml_interrogate`/`validate_yaml`/`yaml_to_python` build a plan from YAML and `read_file`/`write_file` persist an interrogated `Validate`, and `Pipeline.run` executes a multi-table contract set.
- evidence: each interrogated plan captures step count, per-step passing/failing test-unit counts and fractions, severity grade per level, threshold breach flags, sundered row counts, and the emitted report kind as a grade receipt.

[STACKING]:
- `narwhals`(`.api/narwhals.md`): `Validate` accepts the Narwhals-normalized frame, so Polars/Pandas/Ibis/DuckDB/CSV/Parquet/connection-string inputs all enter one backend-agnostic plan through the narwhals carrier.
- `great-tables`(`libs/python/artifacts/.api/great-tables.md`): `get_tabular_report`/`get_step_report`/`col_summary_tbl`/`missing_vals_tbl`/`preview`/`DataScan.get_tabular_report` emit a `great_tables.GT` frame the artifacts renderer consumes directly, never a re-rendered HTML table.
- `pandera`(`.api/pandera.md`) / `dataframely`(`.api/dataframely.md`): one validation concern partitioned by engine — column-health grading and severity thresholds here, pandas/multi-backend checks to pandera, Polars-native declarative contracts to dataframely.
- profile plane: `Validate`+`Thresholds`+`get_tabular_report` compose into the `PROFILE_POINTBLANK_GRADE` path emitting the `QualityProfile` frame.

[LOCAL_ADMISSION]:
- Import `pointblank as pb` at boundary scope; author a chained `Validate` plan whose steps return `Validate`, never a per-rule validator object or an external `assert` loop.
- Grade through `Thresholds`/`Actions` and bind side-effects (`send_slack_notification`/`emit_otel`) as `Actions` payloads, never a try/notify or OTel-span wrapper around `interrogate()`.
- Synthesize fixtures with `generate_dataset(schema, ...)` over `*_field()`/`*Field` columns and round-trip a real frame through `schema_from_tbl`/`profile_fields`, never a hand-built mock frame.
- Persist a plan through `read_file`/`write_file` or a `Contract`/`Pipeline` `to_yaml`/`from_yaml`, and drive declarative validation with `yaml_interrogate`, never a parallel plan API.

[RAIL_LAW]:
- Package: `pointblank`
- Owns: chained validation-plan authoring over Narwhals-backed frames, warning/error/critical threshold grading with severity actions and `send_slack_notification`/`emit_otel` side-effects, column/row/schema/aggregate assertions, table profiling, `Schema`-driven synthetic-dataset generation, declarative `Contract`/`Pipeline`/YAML plans, and `great_tables.GT` report emission
- Accept: graded data-quality validation feeding a renderable `GT` frame to the document, visuals, and grade owners
- Reject: wrapper-renames of `Validate`/`interrogate`/`get_tabular_report`; a hand-rolled row-level assertion engine; a parallel step type per comparison operator or per frame library; a re-rendered HTML table where the in-package `GT` frame needs none; an external try/notify or OTel-span wrapper around `interrogate()` where `Actions`+`emit_otel`/`send_slack_notification` own the side-effect rail; a hand-built mock frame where `generate_dataset`+`Schema` synthesizes one
