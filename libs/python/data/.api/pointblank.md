# [PY_DATA_API_POINTBLANK]

`pointblank` supplies the graded data-quality validation surface for the data grading rail: a `Validate` workflow that chains `col_vals_*`, `col_schema_match`, `rows_distinct`, and `row_count_match` validation steps against a Narwhals-backed frame (Polars/Pandas/Ibis/DuckDB/CSV/Parquet/database connection), interrogates the plan with `interrogate()`, and emits a `great_tables.GT` renderable report through `get_tabular_report()`. `Thresholds` and `Actions`/`FinalActions` grade each step at warning/error/critical severity, `send_slack_notification`/`emit_otel` ship action side-effects, and the boolean rails `all_passed`, `n_passed`/`f_passed`, and `above_threshold`/`assert_below_threshold` reduce the interrogated plan to a pass/fail grade. The declarative `Contract`/`Pipeline` surface, YAML interrogation, and `import_contract`/`export_contract` adapters persist a plan; `Schema`-driven `generate_dataset` (with `int_field`/`string_field`/... helpers or `IntField`/`StringField`/... classes) synthesizes tables, and the metadata/SDTM/ADaM families validate clinical-trial dataset structure. The package owner composes `Validate`, `Thresholds`, and `get_tabular_report` into the `PROFILE_POINTBLANK_GRADE` path; it consumes the in-package step algebra and severity grading rather than re-implementing a row-level assertion engine, and routes the emitted `GT` frame to the renderer.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pointblank`
- package: `pointblank`
- import: `pointblank`
- owner: `data`
- rail: grade
- license: MIT
- asset: pure Python; core deps `narwhals>=2`, `great-tables>=0.22`, `commonmark`, `requests`, `click`, `rich`, `pyyaml`
- entry points: console script `pb` (`pointblank.cli:cli`); library use is import-only
- capability: chained validation plan over Narwhals-backed frames (Polars/Pandas/Ibis/DuckDB/CSV/Parquet/database connection), per-step warning/error/critical thresholds with severity actions and `send_slack_notification`/`emit_otel` side-effects, column and row-level assertions, schema match, AI-driven `prompt`/`specially`/`conjointly`/`assistant`/`DraftValidation` steps, `great_tables.GT` tabular/step/dataframe/JSON reports, table profiling via `DataScan`/`col_summary_tbl`/`missing_vals_tbl`/`preview`, sundered pass/fail splitting, declarative `Contract`/`Pipeline` and YAML-driven validation with `import_contract`/`export_contract` adapters, `Schema`-driven synthetic dataset generation (`generate_dataset` + `*_field`/`*Field` helpers + `GeneratorConfig`), and CDISC SDTM/ADaM clinical-dataset structure validation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: validation workflow, grading, schema, and profiling roots
- rail: grade
- fields: per-column generation constraints are `Field`/`IntField`/`FloatField`/`StringField`/`BoolField`/`DateField`/`DatetimeField`/`TimeField`/`DurationField` classes or the mirror `int_field`/`float_field`/`string_field`/`bool_field`/`date_field`/`datetime_field`/`time_field`/`duration_field` helpers, preferred inside a `Schema` column spec

`Validate` is the workflow root; validation-step methods return `Validate` for chaining and `interrogate()` populates the reporting data. `Thresholds` grades failing test units at three severity levels and `Actions`/`FinalActions` bind callables to a severity. `Contract`/`Step`/`Pipeline`/`PipelineResult` are the declarative, serializable plan model the YAML and `import_contract`/`export_contract` paths build. `Schema` declares column names and dtypes for `col_schema_match` and synthetic generation; the `Field`/`*Field` classes and the `*_field` helper functions are two equivalent idioms for per-column generation constraints. `DataScan` profiles a table independent of a validation plan, and `MissingSpec` declares custom missing-value sentinels.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]    | [RAIL]                                                                          |
| :-----: | :----------------------------- | :--------------- | :------------------------------------------------------------------------------ |
|  [01]   | `Validate`                     | workflow         | validation-plan root that interrogates and reports a grade                      |
|  [02]   | `Thresholds`                   | grading          | warning/error/critical failing-unit limits                                      |
|  [03]   | `Actions`                      | grading          | per-severity callables fired when a step threshold is met                       |
|  [04]   | `FinalActions`                 | grading          | callables fired once after interrogation completes                              |
|  [05]   | `Contract`                     | declarative plan | serializable contract bundling a `Validate` plan, metadata, and adapter binding |
|  [06]   | `Step`                         | declarative plan | one declarative validation step inside a `Contract`/`Pipeline`                  |
|  [07]   | `Pipeline`                     | declarative plan | ordered multi-table contract execution returning `PipelineResult`               |
|  [08]   | `PipelineResult`               | declarative plan | aggregated per-contract grade from a `Pipeline.run`                             |
|  [09]   | `Schema`                       | schema           | column-name and dtype declaration for match and generation                      |
|  [10]   | `MissingSpec`                  | schema           | custom missing-value sentinel declaration for missing-value steps and profiling |
|  [11]   | `DataScan`                     | profiling        | standalone table profile with tabular and JSON reports                          |
|  [12]   | `GeneratorConfig`              | generation       | synthetic-dataset row count, seed, locale, and output policy                    |
|  [13]   | `DraftValidation`              | authoring        | AI-drafted validation plan from a table and model                               |
|  [14]   | `Field` / `*Field`             | generation value | per-column generation constraint classes (see `- fields:`)                      |
|  [15]   | `int_field` … `duration_field` | generation value | snake-case `*_field()` factories mirroring the `*Field` classes                 |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Validate` plan, interrogate, and grade
- rail: grade

`Validate(data, ...)` opens the plan; each validation method appends a step and returns `Validate` so steps chain, and every step shares `thresholds`, `actions`, `brief`, and `active` policy plus `pre`/`segments` where applicable. `interrogate()` acts on the plan and the boolean/fraction rails reduce it to a grade; `get_tabular_report()` emits the renderable `GT` frame.
- call: `Validate(data, reference=None, tbl_name=None, label=None, thresholds=None, actions=None, final_actions=None, brief=None, lang=None, locale=None, owner=None, consumers=None, version=None)`; `interrogate(collect_extracts=True, collect_tbl_checked=True, get_first_n=None, sample_n=None, sample_frac=None, extract_limit=500) -> Validate`
- report: `get_tabular_report(title=":default:", incl_header=None, incl_footer=None, incl_footer_timings=None, incl_footer_notes=None) -> GT`; `get_step_report(i, columns_subset=None, header=":default:", limit=10) -> GT`; `get_json_report(use_fields=None, exclude_fields=None) -> str`; `get_dataframe_report(tbl_type="polars")`; `get_sundered_data(type="pass")`
- grade: `all_passed() -> bool`; `n_passed(i=None, scalar=False)`/`n_failed`/`f_passed`/`f_failed` return per-step `dict` or scalar; `above_threshold(level="warning", i=None) -> bool`; `assert_below_threshold(level="warning", i=None, message=None)`

| [INDEX] | [SURFACE]                         | [CAPABILITY]                                  |
| :-----: | :-------------------------------- | :-------------------------------------------- |
|  [01]   | `Validate`                        | open a validation plan over a target table    |
|  [02]   | `Validate.interrogate`            | execute the plan and populate reporting data  |
|  [03]   | `Validate.get_tabular_report`     | emit the renderable `GT` grade report         |
|  [04]   | `Validate.get_step_report`        | emit a `GT` drill-down report for one step    |
|  [05]   | `Validate.get_json_report`        | serialize the interrogated plan to JSON       |
|  [06]   | `Validate.get_dataframe_report`   | report as a Polars/Pandas/DuckDB frame        |
|  [07]   | `Validate.get_sundered_data`      | split the table into passing/failing rows     |
|  [08]   | `Validate.all_passed`             | grade: every step passed                      |
|  [09]   | `Validate.n_passed`               | passing test-unit count per step              |
|  [10]   | `Validate.n_failed`               | failing test-unit count per step              |
|  [11]   | `Validate.f_passed`               | passing test-unit fraction per step           |
|  [12]   | `Validate.f_failed`               | failing test-unit fraction per step           |
|  [13]   | `Validate.above_threshold`        | grade: a step breached the named severity     |
|  [14]   | `Validate.assert_below_threshold` | raise when a step breached the named severity |

[ENTRYPOINT_SCOPE]: `Validate` step algebra
- rail: grade

Every step row appends to the plan and returns `Validate`; `columns` accepts a name, list, `col(...)`, or a column selector, and `thresholds`/`actions`/`brief`/`active`/`pre`/`segments` are per-step policy rows. The `col_vals_*` family is one comparison surface discriminated by the operator and value shape, never a parallel step type per operator.
- policy: every step ends `..., pre=None, segments=None, thresholds=None, actions=None, brief=None, active=True) -> Validate` (`col_exists`/`col_*_match`/`tbl_match`/`conjointly`/`specially` omit `segments`); the heads below carry only the distinguishing args
- monotonic: `col_vals_increasing(columns, allow_stationary=False, decreasing_tol=None, na_pass=False, ...)` mirrors `col_vals_decreasing(columns, allow_stationary=False, increasing_tol=None, na_pass=False, ...)` — each direction owns the opposite-slack tolerance
- aggregate: `col_avg_gt`/`col_sum_*`/`col_sd_*` cross `avg`/`sum`/`sd` with `gt`/`ge`/`lt`/`le`/`eq` (no `ne` arm, unlike the per-cell family)
- schema: `col_schema_match(schema, complete=True, in_order=True, case_sensitive_colnames=True, case_sensitive_dtypes=True, full_match_dtypes=True, ...)`
- prompt: `prompt(prompt, model, columns_subset=None, batch_size=1000, max_concurrent=3, ...)`

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

[ENTRYPOINT_SCOPE]: grading, profiling, schema, and I/O functions
- rail: grade

`Thresholds`/`Actions` configure grading and `send_slack_notification`/`emit_otel` build action callables fired on breach; `DataScan`, `col_summary_tbl`, `missing_vals_tbl`, and `preview` emit `GT` frames without a validation plan. `load_dataset`/`generate_dataset` supply input tables, `connect_to_table` opens a database table, `schema_from_tbl` infers a `Schema`, and `yaml_interrogate`/`read_file`/`import_contract` rebuild a `Validate`/`Contract` from a declarative source. `get_action_metadata`/`get_validation_summary` read interrogation context inside an action callable.

- grade-call: `Thresholds(warning=None, error=None, critical=None)`, `Actions(warning=None, error=None, critical=None, default=None, highest_only=True)`, `FinalActions(*args)`; helpers `send_slack_notification(...)`, `emit_otel(...)`, `get_action_metadata() -> dict`, `get_validation_summary() -> dict`
- profile-call: `DataScan(data, tbl_name=None)` with `.get_tabular_report(show_sample_data=False) -> GT`/`.to_json() -> str`/`.summary_data`; `col_summary_tbl(data, tbl_name=None) -> GT`; `missing_vals_tbl(data) -> GT`; `preview(data, columns_subset=None, n_head=5, n_tail=5, limit=50, show_row_numbers=True, max_col_width=250, min_tbl_width=500, incl_header=None) -> GT`; `get_row_count(data)`/`get_column_count(data)`
- data-call: `load_dataset(dataset="small_table", tbl_type="polars")`/`get_data_path(dataset="small_table", file_type="csv")`; `generate_dataset(schema, n=100, seed=None, output="polars", country="US", shuffle=True, weighted=True)`; `connect_to_table(connection_string)`/`print_database_tables(connection_string)`; `schema_from_tbl(tbl) -> Schema`/`profile_fields(tbl) -> list[Field]`
- field-call: `int_field(min_val=, max_val=, allowed=, ...)`, `float_field(...)`, `string_field(pattern=, preset=, min_length=, ...)`, `bool_field(...)`, `date_field(...)`, `datetime_field(...)`, `time_field(...)`, `duration_field(...)` -> `Field`; `GeneratorConfig(...)`
- declare-call: `yaml_interrogate(yaml, set_tbl=None, namespaces=None) -> Validate`/`validate_yaml(yaml)`/`yaml_to_python(yaml) -> str`; `read_file(filepath) -> Validate`/`write_file(validation, filename, path=None, keep_tbl=False, keep_extracts=False, quiet=False)`; `import_contract(...)`/`export_contract(...)`/`register_adapter(adapter)`/`list_adapters() -> list[str]` with `ContractAdapter`/`ContractImport`
- target-call: `col(exprs) -> Column`/`ref(column_name) -> ReferenceColumn`/`expr_col(column_name) -> ColumnExpression`; selectors `starts_with(text, case_sensitive=False)`/`ends_with(...)`/`contains(...)`/`matches(pattern, ...)`/`everything()`/`first_n(n, offset=0)`/`last_n(n, offset=0)`; `seg_group(columns)`
- misc-call: `config(report_incl_header=True, report_incl_footer=True, report_incl_footer_timings=True, report_incl_footer_notes=True, preview_incl_header=True) -> PointblankConfig`; `assistant(...)`, `DraftValidation(tbl, model, api_key=None, ...)`; CDISC `import_metadata`/`export_metadata`, `validate_sdtm`/`validate_adam`, `get_sdtm_domain`/`get_adam_dataset`, `sdtm_to_metadata`/`adam_to_metadata`, `Codelist`/`VariableMetadata`/`SDTMVariableSpec`/`ADaMVariableSpec`

| [INDEX] | [SURFACE]          | [CAPABILITY]                                                         |
| :-----: | :----------------- | :------------------------------------------------------------------- |
|  [01]   | `Thresholds`       | severity limits as counts or fractions                               |
|  [02]   | `Actions`          | per-severity callables fired on breach                               |
|  [03]   | `FinalActions`     | callables fired once after interrogation                             |
|  [04]   | `DataScan`         | standalone table profile with tabular and JSON reports               |
|  [05]   | `col_summary_tbl`  | per-column summary `GT` frame                                        |
|  [06]   | `missing_vals_tbl` | missing-value `GT` frame                                             |
|  [07]   | `preview`          | head/tail preview `GT` frame                                         |
|  [08]   | `get_row_count`    | frame shape (`get_column_count` mirror)                              |
|  [09]   | `load_dataset`     | built-in datasets and on-disk paths                                  |
|  [10]   | `generate_dataset` | synthetic table from a `Schema` via `*_field()`/`*Field` columns     |
|  [11]   | `*_field` helpers  | per-column generation-constraint factories + global policy           |
|  [12]   | `schema_from_tbl`  | infer a `Schema` / generation fields from a table                    |
|  [13]   | `connect_to_table` | open a database-backed table                                         |
|  [14]   | `yaml_interrogate` | build and interrogate a plan from YAML                               |
|  [15]   | `read_file`        | persist and reload an interrogated plan                              |
|  [16]   | `import_contract`  | serialize a declarative `Contract` through pluggable adapters        |
|  [17]   | `col`              | column reference for step targets and comparisons                    |
|  [18]   | column selectors   | resolve a column set for `columns`                                   |
|  [19]   | `seg_group`        | declare a multi-column segmentation group for `segments`             |
|  [20]   | action callables   | side-effecting/context helpers fired inside `Actions`/`FinalActions` |
|  [21]   | `assistant`        | LLM-authored validation drafting and chat assistant                  |
|  [22]   | `config`           | global report/preview defaults                                       |
|  [23]   | CDISC metadata     | clinical-trial dataset metadata and SDTM/ADaM structure validation   |

## [04]-[IMPLEMENTATION_LAW]

[GRADE_VALIDATION]:
- import: `import pointblank as pb` at boundary scope only; module-level import is banned by the manifest import policy.
- plan axis: one `Validate` owns the validation plan; each `col_vals_*`/`rows_*`/`col_schema_match`/`row_count_match` call is a step row returning `Validate`, never a per-rule validator object — the plan is a chained fold, not a list of hand-instantiated checks.
- comparison axis: `col_vals_gt`/`ge`/`lt`/`le`/`eq`/`ne`/`between`/`outside`/`in_set`/`not_in_set`/`regex`/`within_spec` is one comparison surface discriminated by operator and value shape; `value` accepts a literal, `col(...)`, or `ref(...)`, never a parallel step type per operator.
- column axis: `columns` resolves a name, list, `col(...)`, or selector (`starts_with`/`ends_with`/`contains`/`matches`/`everything`/`first_n`/`last_n`); selectors are rows, never a hand-built column-name loop.
- grade axis: `Thresholds(warning, error, critical)` grades failing test units at three severity levels as counts or fractions; `Actions`/`FinalActions` bind callables to a severity and `above_threshold`/`assert_below_threshold` reduce the interrogated plan to a pass/fail grade — `all_passed`/`n_passed`/`f_passed` are the boolean and fractional grade rails.
- action axis: severity callables are the package's own side-effect rail — `send_slack_notification`/`emit_otel` are built `Actions` payloads (the OTel span is emitted by pointblank, never hand-stitched around `interrogate`), and `get_action_metadata`/`get_validation_summary` read the breaching-step context inside the callable; bind side-effects through `Actions`, never wrap `interrogate()` in an external try/notify block.
- generation axis: `generate_dataset(schema, ...)` synthesizes a table from a `Schema` whose columns are `int_field()`/`string_field()`/... helpers (or the equivalent `IntField`/`StringField`/... classes) plus a `GeneratorConfig` policy; `schema_from_tbl`/`profile_fields` round-trip a real frame into a generation `Schema` — synthetic fixtures stack into the same `Validate` plan, never a parallel mock-frame builder.
- interrogate axis: `interrogate()` is the single execution surface that acts on the plan; sampling (`sample_n`/`sample_frac`/`get_first_n`) and extract limits are call rows, never a separate runner type.
- render axis: `get_tabular_report()` emits the `great_tables.GT` grade frame keyed by the interrogated plan; `get_step_report`/`get_dataframe_report`/`get_json_report` are report rows for the same plan, and `col_summary_tbl`/`missing_vals_tbl`/`preview`/`DataScan.get_tabular_report` emit `GT` frames for profiling without a plan — the `GT` object feeds the renderer directly, never a re-rendered HTML table.
- frame axis: `data` accepts a Polars/Pandas/Ibis frame, a CSV/Parquet path or glob, a GitHub URL, or a database connection string through Narwhals; `connect_to_table` and `load_dataset`/`generate_dataset` supply tables — the validation engine is backend-agnostic, never re-implemented per frame library.
- declarative axis: `yaml_interrogate`/`validate_yaml`/`yaml_to_python` build and interrogate a plan from YAML and `read_file`/`write_file` persist an interrogated `Validate`; the typed `Contract`/`Step`/`Pipeline`/`PipelineResult` model plus `import_contract`/`export_contract` over registered `ContractAdapter`s is the serializable plan owner, and `Pipeline` runs a multi-table contract set — declarative validation is a source/contract row, never a parallel plan API.
- clinical axis: the SDTM/ADaM metadata family (`validate_sdtm`/`validate_adam`, `get_sdtm_domain`/`get_adam_dataset`, `import_metadata`/`export_metadata`, `Codelist`/`SDTMVariableSpec`/`ADaMVariableSpec`) validates CDISC dataset structure against a domain template inside the same grading engine; it is a domain-template row over the `Validate` plan, never a separate validator.
- evidence: each interrogated plan captures step count, per-step passing/failing test-unit counts and fractions, severity grade per level, threshold breach flags, sundered row counts, and the emitted report kind as a grade receipt.
- boundary: pointblank owns the validation plan, severity grading, and `GT` report emission; `great_tables` (`>=0.22`) owns the renderable frame downstream; Narwhals owns frame normalization; the emitted `GT` feeds the document and visuals owners; live UI stays outside this package.

[RAIL_LAW]:
- Package: `pointblank`
- Owns: chained validation-plan authoring over Narwhals-backed frames, warning/error/critical threshold grading with severity actions and `send_slack_notification`/`emit_otel` side-effects, column/row/schema/aggregate assertions, table profiling, `Schema`-driven synthetic-dataset generation, declarative `Contract`/`Pipeline`/YAML plans with pluggable adapters, CDISC SDTM/ADaM structure validation, and `great_tables.GT` report emission
- Accept: graded data-quality validation feeding a renderable `GT` frame to the document, visuals, and grade owners
- Reject: wrapper-renames of `Validate`/`interrogate`/`get_tabular_report`; a hand-rolled row-level assertion engine; a parallel step type per comparison operator or per frame library; a re-rendered HTML table where the in-package `GT` frame needs none; an external try/notify or OTel-span wrapper around `interrogate()` where `Actions`+`emit_otel`/`send_slack_notification` own the side-effect rail; a hand-built mock frame where `generate_dataset`+`Schema` synthesizes one; identity or threshold-grade minting the runtime owns
