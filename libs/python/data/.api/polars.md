# [PY_DATA_API_POLARS]

`polars` owns the columnar dataframe rail: a Rust-backed engine whose eager `DataFrame`/`Series` and lazy `LazyFrame` share one `Expr`-composed transformation API over a typed dtype vocabulary. `LazyFrame` defers a query graph until `collect` — engine-selectable across in-memory, streaming out-of-core, and GPU backends under per-call optimizer flags — or a streaming `sink_*`, admitting predicate/projection pushdown and out-of-core execution. `polars.plugins.register_plugin_function` admits native Rust kernels as first-class `Expr` nodes, the seam `polars-st` grafts geometry onto.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `polars`
- package: `polars` (MIT)
- owner: `data`
- module: `polars`
- namespaces: `polars.selectors` (column-set algebra), `polars.plugins` (native Rust expression plugins), `polars.io.plugins` (lazy IO-source plugins), `polars.exceptions` (typed error rail), `polars.testing`, `polars.api` (custom-namespace registration), `polars.sql`
- rail: columnar dataframe

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: frame, series, and expression types

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]   | [CAPABILITY]                                                                    |
| :-----: | :-------------------------------- | :-------------- | :------------------------------------------------------------------------------ |
|  [01]   | `DataFrame`                       | eager frame     | in-memory column-major table                                                    |
|  [02]   | `LazyFrame`                       | lazy frame      | deferred optimized query graph                                                  |
|  [03]   | `Series`                          | typed column    | named typed 1-D column                                                          |
|  [04]   | `Expr`                            | expression node | composable column expression                                                    |
|  [05]   | `Schema`                          | schema value    | ordered `{name: DataType}` mapping                                              |
|  [06]   | `Field`                           | schema field    | named typed field for `Struct`                                                  |
|  [07]   | `Config`                          | runtime config  | display, string-cache, and engine settings                                      |
|  [08]   | `SQLContext`                      | SQL engine      | register frames and run SQL                                                     |
|  [09]   | `StringCache`                     | context manager | shared categorical string-cache scope                                           |
|  [10]   | `GPUEngine`                       | engine selector | cuDF/RAPIDS CUDA backend for `collect(engine=GPUEngine(...))`                   |
|  [11]   | `QueryOptFlags`                   | opt toggle      | per-call optimizer flag bundle for `collect`/`sink_*`/`explain`                 |
|  [12]   | `Catalog`                         | unity catalog   | Unity/Iceberg REST catalog client (`scan`/`write` namespaced tables)            |
|  [13]   | `CredentialProviderAWS`           | cloud auth      | AWS object-store credential provider for `scan_*`/`sink_*`                      |
|  [14]   | `CredentialProviderAzure`         | cloud auth      | Azure object-store credential provider for `scan_*`/`sink_*`                    |
|  [15]   | `CredentialProviderGCP`           | cloud auth      | GCP object-store credential provider for `scan_*`/`sink_*`                      |
|  [16]   | `DataTypeExpr`                    | dtype expr      | deferred dtype expression (`dtype_of`, `self_dtype`) for schema-dependent logic |
|  [17]   | `Categories`                      | category store  | shared category registry for `Categorical` columns                              |
|  [18]   | `ScanCastOptions` / `CompatLevel` | scan/io policy  | cast-on-scan policy and Arrow compat-level for `to_arrow`/IPC                   |

[PUBLIC_TYPE_SCOPE]: dtype vocabulary

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]  | [CAPABILITY]                        |
| :-----: | :----------------------------- | :------------- | :---------------------------------- |
|  [01]   | `Int8/16/32/64/128`            | integer dtype  | signed integer dtypes               |
|  [02]   | `UInt8/16/32/64/128`           | integer dtype  | unsigned integer dtypes             |
|  [03]   | `Float32` / `Float64`          | float dtype    | IEEE float dtypes                   |
|  [04]   | `Boolean`                      | boolean dtype  | boolean dtype                       |
|  [05]   | `String` / `Utf8`              | string dtype   | UTF-8 string dtype                  |
|  [06]   | `Binary`                       | binary dtype   | opaque bytes dtype                  |
|  [07]   | `Date`                         | temporal dtype | calendar date dtype                 |
|  [08]   | `Datetime(time_unit, tz)`      | temporal dtype | timestamp dtype with time zone      |
|  [09]   | `Duration(time_unit)`          | temporal dtype | duration dtype                      |
|  [10]   | `Time`                         | temporal dtype | time-of-day dtype                   |
|  [11]   | `Categorical` / `Enum`         | category dtype | unordered and fixed-category dtypes |
|  [12]   | `Struct` / `Field`             | nested dtype   | struct dtype and its fields         |
|  [13]   | `List(inner)` / `Array(inner)` | nested dtype   | variable and fixed-length lists     |
|  [14]   | `Decimal(precision, scale)`    | decimal dtype  | fixed-precision decimal dtype       |
|  [15]   | `Object` / `Null` / `Unknown`  | special dtype  | opaque, all-null, and inferred      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and IO

| [INDEX] | [SURFACE]                                                           | [CAPABILITY]                                  |
| :-----: | :------------------------------------------------------------------ | :-------------------------------------------- |
|  [01]   | `DataFrame(data, schema, ...)`                                      | build eager frame from dict/rows              |
|  [02]   | `from_dict` / `from_dicts` / `from_records`                         | build from dicts or row records               |
|  [03]   | `from_arrow` / `from_pandas` / `from_numpy` / `from_torch`          | build from Arrow, pandas, NumPy, torch        |
|  [04]   | `from_dataframe`                                                    | zero-copy Arrow C-stream / interchange import |
|  [05]   | `read_csv` / `read_parquet` / `read_ipc`                            | read files into a `DataFrame`                 |
|  [06]   | `read_json` / `read_ndjson` / `read_avro`                           | read structured text/binary                   |
|  [07]   | `read_database` / `read_database_uri`                               | read from a SQL connection                    |
|  [08]   | `read_delta` / `read_excel` / `read_ods`                            | read table-store and spreadsheet              |
|  [09]   | `scan_csv` / `scan_parquet` / `scan_ipc`                            | scan files into a `LazyFrame`                 |
|  [10]   | `scan_ndjson` / `scan_delta` / `scan_lines`                         | scan structured / Delta / raw lines           |
|  [11]   | `scan_iceberg`                                                      | scan an Iceberg table                         |
|  [12]   | `scan_pyarrow_dataset`                                              | scan a `pyarrow` dataset with pushdown        |
|  [13]   | `read_parquet_metadata` / `read_parquet_schema` / `read_ipc_schema` | inspect Parquet/IPC without full read         |
|  [14]   | `defer` / `explain_all` / `collect_all` / `collect_all_async`       | defer a Python frame; batch-explain/collect   |
|  [15]   | `io.plugins.register_io_source`                                     | lift a custom Python source lazily            |

- `scan_*` carry: `glob`, `storage_options`, `credential_provider`.
- `scan_iceberg`: accepts a live `pyiceberg.table.Table` as `src`; `scan_pyarrow_dataset` pushes predicates into the dataset scan.
- `io.plugins.register_io_source`: `(io_source, *, schema, validate_schema, is_pure) -> LazyFrame`; the generator yields `DataFrame` windows under projection/predicate/`n_rows`/`batch_size` pushdown.

[ENTRYPOINT_SCOPE]: DataFrame and LazyFrame operations

| [INDEX] | [SURFACE]                                                       | [CAPABILITY]                                        |
| :-----: | :-------------------------------------------------------------- | :-------------------------------------------------- |
|  [01]   | `select` / `select_seq`                                         | select and transform columns                        |
|  [02]   | `with_columns` / `with_columns_seq`                             | add or replace columns                              |
|  [03]   | `filter`                                                        | row filter by expression                            |
|  [04]   | `group_by` / `group_by_dynamic`                                 | grouped and time-window aggregation                 |
|  [05]   | `rolling`                                                       | rolling time-window aggregation                     |
|  [06]   | `join` / `join_asof`                                            | hash and as-of joins                                |
|  [07]   | `join_where`                                                    | inequality (non-equi) join                          |
|  [08]   | `sort` / `top_k` / `bottom_k`                                   | sort and ranked selection                           |
|  [09]   | `unique` / `drop_nulls` / `drop_nans`                           | deduplicate and drop missing rows                   |
|  [10]   | `pivot` / `unpivot`                                             | wide/long reshape (`DataFrame.pivot`)               |
|  [11]   | `explode` / `unnest` / `transpose`                              | expand lists, flatten structs, transpose            |
|  [12]   | `rename` / `drop` / `cast`                                      | rename, drop, and cast columns                      |
|  [13]   | `with_row_index`                                                | add a row-index column                              |
|  [14]   | `lazy` / `collect` / `collect_schema`                           | switch to lazy, materialize, peek schema            |
|  [15]   | `sink_parquet` / `sink_csv` / `sink_ipc` / `sink_ndjson`        | streaming write without full collect                |
|  [16]   | `sink_iceberg` / `sink_delta`                                   | streaming write into an Iceberg/Delta table         |
|  [17]   | `collect_batches` / `sink_batches`                              | iterate/emit result as a `RecordBatch` stream       |
|  [18]   | `explain` / `profile` / `show_graph`                            | optimized-plan text, per-node timing, plan DAG      |
|  [19]   | `serialize` / `deserialize` / `remote`                          | round-trip a query plan; offload to Polars Cloud    |
|  [20]   | `with_context` / `match_to_schema` / `cache` / `set_sorted`     | side-frame context, coercion, cache, sortedness     |
|  [21]   | `write_parquet` / `write_csv` / `write_delta` / `write_iceberg` | write eager frame to storage                        |
|  [22]   | `to_arrow` / `to_pandas` / `to_numpy` / `__arrow_c_stream__`    | export to Arrow/pandas/NumPy; PyCapsule C-stream    |
|  [23]   | `sql` / `SQLContext` / `sql_expr`                               | run SQL over frames; parse a SQL fragment to `Expr` |
|  [24]   | `slice` / `head` / `height`                                     | row-offset slice, first-n rows, row count           |
|  [25]   | `Series.to_frame` / `Series.rename`                             | promote a `Series` to a one-column frame; rename it |

- `collect`: `(engine=, optimizations=QueryOptFlags(...), background=False)`; `sink_*` add `partition_by`, `storage_options`, `credential_provider`, `mkdir`, `sync_on_close`.
- `profile`: returns `(result, timing)` frames; timing schema `{node: String, start: UInt64, end: UInt64}` in microseconds.

[ENTRYPOINT_SCOPE]: expression functions and namespaces

| [INDEX] | [SURFACE]                                                        | [CAPABILITY]                                       |
| :-----: | :--------------------------------------------------------------- | :------------------------------------------------- |
|  [01]   | `col` / `all` / `exclude` / `nth`                                | reference columns in expressions                   |
|  [02]   | `lit` / `first` / `last` / `len` / `count`                       | scalar literals and frame aggregates               |
|  [03]   | `when().then().otherwise()`                                      | branchwise expression                              |
|  [04]   | `sum/min/max/mean/median/std/var/quantile`                       | column-wise aggregate expressions                  |
|  [05]   | `sum_horizontal/min_horizontal/max_horizontal/...`               | row-wise aggregate across columns                  |
|  [06]   | `all_horizontal` / `any_horizontal`                              | row-wise logical AND/OR                            |
|  [07]   | `concat_str` / `concat_list` / `concat_arr`                      | row-wise string/list concatenation                 |
|  [08]   | `coalesce(*exprs)`                                               | first non-null per row                             |
|  [09]   | `struct(*exprs)` / `field(name)` / `format`                      | build struct, access field, format                 |
|  [10]   | `corr` / `cov` / `rolling_corr` / `rolling_cov`                  | correlation and covariance                         |
|  [11]   | `int_range` / `date_range` / `datetime_range` / `linear_space`   | integer/temporal/linear ranges                     |
|  [12]   | `concat` / `align_frames` / `merge_sorted`                       | concatenate, align, merge sorted frames            |
|  [13]   | `fold` / `reduce` / `cum_fold` / `cum_reduce`                    | typed horizontal reduction (no Python loop)        |
|  [14]   | `Expr.over`                                                      | windowed expression                                |
|  [15]   | `Expr.rolling` / `Expr.rolling_*`                                | time-anchored rolling windows + 20 reducers        |
|  [16]   | `Expr.cut` / `qcut` / `rle` / `rle_id` / `value_counts` / `hist` | discretize, run-length encode, histogram           |
|  [17]   | `Expr.ewm_mean` / `ewm_std` / `ewm_var` / `ewm_mean_by`          | exponentially-weighted moving statistics           |
|  [18]   | `Expr.replace` / `replace_strict` / `is_in` / `is_close`         | value remap, membership, tolerance compare         |
|  [19]   | `Expr.str` / `Expr.dt` / `Expr.list` / `Expr.arr`                | string/temporal/list/array method families         |
|  [20]   | `Expr.struct` / `Expr.cat` / `Expr.bin` / `.name` / `.meta`      | struct, categorical, binary, naming, plan-metadata |
|  [21]   | `selectors` (`cs.numeric` / `by_dtype` / `matches` / ...)        | declarative dtype/name column sets, `&`/`\|`/`-`   |
|  [22]   | `plugins.register_plugin_function`                               | register a Rust expression-plugin kernel as `Expr` |
|  [23]   | `Expr.map_batches` / `map_elements` / `register_plugin`          | Series/element Python UDFs (last resort)           |

- `Expr.over`: `(partition_by, *, order_by=, mapping_strategy=)`; `Expr.rolling(index_column, period=, offset=, closed=)` anchors time windows.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `DataFrame` and `LazyFrame` share one `Expr` transformation API; `LazyFrame` records a graph and `collect` runs the optimizer, whose passes toggle per call through `QueryOptFlags` or boolean kwargs.
- `collect(engine=)` selects in-memory, streaming out-of-core, or `GPUEngine` execution; `background=True` returns an `InProcessQuery`, and `collect_batches`/`sink_batches` stream the result as `RecordBatch`es.
- Expressions stay lazy regardless of frame: `col("x") * 2` is an `Expr` until evaluated inside `select`/`with_columns`/`filter`/`group_by(...).agg`/`Expr.over`; `DataTypeExpr`/`dtype_of`/`self_dtype` defer schema-dependent dtype decisions into the graph.
- Dtypes are value objects: `Datetime`/`Duration` carry a `time_unit`, `Categorical`/`Enum` bind a string-cache identity through `Categories`, and `Struct`/`List`/`Array` nest inner dtypes.
- Temporal windowing folds through `group_by_dynamic`, `Expr.rolling`, and `Expr.over(order_by=, mapping_strategy=)`; `join_asof` joins sorted keys and `join_where` joins inequality predicates.
- Horizontal functions and `fold`/`reduce`/`cum_fold`/`cum_reduce` reduce across columns per row as typed expressions; `selectors` set-algebra addresses column sets without name strings.

[STACKING]:
- `pyiceberg`(`.api/pyiceberg.md`): `scan_iceberg(table, reader_override='native', use_pyiceberg_filter=True)` pushes a live `Table`'s row-filter through PyIceberg's planner — PyIceberg owns catalog/snapshot/manifest planning, Polars owns the scan/transform graph.
- `pyarrow`(`.api/pyarrow.md`): `scan_pyarrow_dataset` and `from_arrow`/`from_dataframe` consume the Arrow C-stream/interchange zero-copy; `to_arrow(compat_level=)`/`__arrow_c_stream__` export the same wire, never a Python row roundtrip.
- `polars-st`(`.api/polars-st.md`): registers its `.st` geometry ops as native `Expr` nodes through `plugins.register_plugin_function`, the first-class path a `#[polars_expr]` kernel declares via `is_elementwise`/`returns_scalar`/`changes_length`.
- within-lib: the `data` folder composes `scan_*` into a `LazyFrame` `Expr` pipeline closed by `collect(engine='streaming')` or a `sink_*`, native plugins grafting domain kernels onto the same graph.

[LOCAL_ADMISSION]:
- A `scan_*` `LazyFrame` pipeline closed by `collect(engine='streaming')` or a `sink_*` wins over eager `read_*`, gaining pushdown and out-of-core execution.
- Express transforms as `Expr` composition inside `select`/`with_columns`/`filter`/`group_by(...).agg`; address columns with `selectors`, reduce with namespace accessors (`.str`/`.dt`/`.list`/`.arr`/`.struct`/`.cat`/`.bin`) and `fold`/`reduce`.
- Reach for `register_plugin_function` (native) before `map_batches`/`map_elements` (Python); wrap categorical-heavy joins in `StringCache`.

[RAIL_LAW]:
- Package: `polars`
- Owns: typed columnar storage, lazy query optimization, `Expr`-based transformation, and the native expression-plugin host.
- Accept: dicts, rows, NumPy/torch, Arrow tables/C-stream, pandas frames, Iceberg/Delta/pyarrow datasets, and file/database sources via `read_*`/`scan_*`.
- Reject: per-row Python loops, eager reads where a lazy scan admits pushdown, hand-rolled join/window/fold logic, `map_elements` where a native plugin or expr combinator exists, hardcoded column-name lists where `selectors` apply.
</content>
</invoke>
