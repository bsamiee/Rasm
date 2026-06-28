# [PY_DATA_API_POLARS]

`polars` is a Rust-backed columnar dataframe engine exposing eager `DataFrame`/`Series` and lazy `LazyFrame` surfaces that share one transformation API built from `Expr` expression nodes. The lazy path defers a query graph until `collect` (with a selectable in-memory/streaming/GPU engine and per-call optimizer flags) or a streaming `sink_*`, enabling predicate/projection pushdown, common-subexpression elimination, and out-of-core execution. A typed dtype vocabulary (`Int8`..`Int128`, `Float16/32/64`, `Datetime`, `Duration`, `Categorical`, `Enum`, `Struct`, `List`, `Array`, `Decimal`) backs every column. Top-level functions (`col`, `lit`, `when`, `concat`, `fold`/`reduce`, horizontal aggregates, `read_*`/`scan_*` IO) compose expressions and frames without materializing intermediate results; `polars.selectors` provides a column-set algebra and `polars.plugins.register_plugin_function` admits native Rust kernels as first-class `Expr` nodes — the mechanism by which `polars-st` grafts geometry ops onto the engine.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `polars`
- package: `polars`
- module: `polars`
- version: `1.42.0`
- license: MIT
- rail: columnar dataframe
- subpackages: `polars.selectors` (declarative column-selection algebra), `polars.plugins` (native Rust expression-plugin registration), `polars.io.plugins` (lazy IO-source plugin registration via `register_io_source`), `polars.exceptions` (typed error rail), `polars.testing`, `polars.api` (custom-namespace registration), `polars.sql`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: frame, series, and expression types
- rail: columnar dataframe

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]   | [ROLE]                                     |
| :-----: | :------------ | :-------------- | :----------------------------------------- |
|  [01]   | `DataFrame`   | eager frame     | in-memory column-major table               |
|  [02]   | `LazyFrame`   | lazy frame      | deferred optimized query graph             |
|  [03]   | `Series`      | typed column    | named typed 1-D column                     |
|  [04]   | `Expr`        | expression node | composable column expression               |
|  [05]   | `Schema`      | schema value    | ordered `{name: DataType}` mapping         |
|  [06]   | `Field`       | schema field    | named typed field for `Struct`             |
|  [07]   | `Config`      | runtime config  | display, string-cache, and engine settings |
|  [08]   | `SQLContext`  | SQL engine      | register frames and run SQL                |
|  [09]   | `StringCache` | context manager | shared categorical string-cache scope      |
|  [10]   | `GPUEngine`   | engine selector | cuDF/RAPIDS CUDA backend for `collect(engine=GPUEngine(...))` |
|  [11]   | `QueryOptFlags` | opt toggle    | per-call optimizer flag bundle for `collect`/`sink_*`/`explain` |
|  [12]   | `Catalog`     | unity catalog   | Unity/Iceberg REST catalog client (`scan`/`write` namespaced tables) |
|  [13]   | `CredentialProviderAWS` / `CredentialProviderAzure` / `CredentialProviderGCP` | cloud auth | object-store credential providers for `scan_*`/`sink_*` `credential_provider=` |
|  [14]   | `DataTypeExpr` | dtype expr     | deferred dtype expression (`dtype_of`, `self_dtype`) for schema-dependent logic |
|  [15]   | `Categories`  | category store  | shared category registry for `Categorical` columns |
|  [16]   | `ScanCastOptions` / `CompatLevel` | scan/io policy | cast-on-scan policy and Arrow compat-level for `to_arrow`/IPC |

[PUBLIC_TYPE_SCOPE]: dtype vocabulary
- rail: columnar dataframe

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]  | [ROLE]                              |
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
- rail: columnar dataframe

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :---------------------------------------------- | :------------- | :-------------------------------- |
|  [01]   | `DataFrame(data, schema, ...)`                  | construct      | build eager frame from dict/rows  |
|  [02]   | `from_dict` / `from_dicts` / `from_records`     | construct      | build from dicts or row records   |
|  [03]   | `from_arrow` / `from_pandas` / `from_numpy` / `from_torch` | interop | build from Arrow, pandas, NumPy, torch |
|  [04]   | `from_dataframe(df)`                            | interop        | build from any `__arrow_c_stream__`/interchange-protocol object (zero-copy) |
|  [05]   | `read_csv` / `read_parquet` / `read_ipc`        | eager IO       | read files into `DataFrame`       |
|  [06]   | `read_json` / `read_ndjson` / `read_avro`       | eager IO       | read structured text/binary       |
|  [07]   | `read_database` / `read_database_uri`           | eager IO       | read from a SQL connection        |
|  [08]   | `read_delta` / `read_excel` / `read_ods`        | eager IO       | read table-store and spreadsheet  |
|  [09]   | `scan_csv` / `scan_parquet` / `scan_ipc`        | lazy IO        | scan files into `LazyFrame` (glob/`storage_options`/`credential_provider`) |
|  [10]   | `scan_ndjson` / `scan_delta` / `scan_lines`     | lazy IO        | scan structured / Delta / raw lines |
|  [11]   | `scan_iceberg(src, snapshot_id, catalog, reader_override, use_pyiceberg_filter)` | lazy IO | scan an Iceberg table; `src` accepts a `pyiceberg.table.Table` directly |
|  [12]   | `scan_pyarrow_dataset(ds, allow_pyarrow_filter, batch_size)` | lazy IO | scan a `pyarrow.dataset.Dataset` lazily with predicate pushdown |
|  [13]   | `read_parquet_metadata` / `read_parquet_schema` / `read_ipc_schema` | metadata | inspect Parquet/IPC without full read |
|  [14]   | `defer(fn, *, schema)` / `explain_all` / `collect_all` / `collect_all_async` | lazy plan | defer a Python-built frame into a lazy plan; batch-explain/collect many `LazyFrame`s |
|  [15]   | `io.plugins.register_io_source(io_source, *, schema, validate_schema=False, is_pure=False) -> LazyFrame` | lazy IO plugin | lift a custom Python source into a `LazyFrame` with projection/predicate/`n_rows`/`batch_size` pushdown; the `io_source` generator `(with_columns, predicate, n_rows, batch_size) -> Iterator[DataFrame]` yields `DataFrame` windows (never `RecordBatch`) and `schema` is the full-source schema |

[ENTRYPOINT_SCOPE]: DataFrame and LazyFrame operations
- rail: columnar dataframe

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [RAIL]                                   |
| :-----: | :-------------------------------------------- | :------------- | :--------------------------------------- |
|  [01]   | `select(*exprs)` / `select_seq`               | projection     | select and transform columns             |
|  [02]   | `with_columns(*exprs)` / `with_columns_seq`   | mutation       | add or replace columns                   |
|  [03]   | `filter(*predicates)`                         | filter         | row filter by expression                 |
|  [04]   | `group_by(*keys)` / `group_by_dynamic`        | aggregation    | grouped and time-window aggregation      |
|  [05]   | `rolling(index_column, period)`               | aggregation    | rolling time-window aggregation          |
|  [06]   | `join(other, on, how)` / `join_asof`          | join           | hash and as-of joins                     |
|  [07]   | `join_where(other, *predicates)`              | join           | inequality (non-equi) join               |
|  [08]   | `sort(by, descending)` / `top_k` / `bottom_k` | sort           | sort and ranked selection                |
|  [09]   | `unique` / `drop_nulls` / `drop_nans`         | dedup/filter   | deduplicate and drop missing rows        |
|  [10]   | `pivot(on, index, values)` / `unpivot`        | reshape        | wide/long reshape (`DataFrame.pivot`)    |
|  [11]   | `explode` / `unnest` / `transpose`            | reshape        | expand lists, flatten structs, transpose |
|  [12]   | `rename` / `drop` / `cast`                    | schema         | rename, drop, and cast columns           |
|  [13]   | `with_row_index(name)`                        | mutation       | add a row-index column                   |
|  [14]   | `lazy()` / `collect(engine=, optimizations=, background=)` / `collect_schema` | execution | switch to lazy, materialize (CPU/`'streaming'`/`GPUEngine`), peek schema |
|  [15]   | `LazyFrame.sink_parquet/sink_csv/sink_ipc/sink_ndjson` | streaming IO | streaming write w/o full collect; `partition_by`/`storage_options`/`credential_provider`/`mkdir`/`sync_on_close` |
|  [16]   | `LazyFrame.sink_iceberg / sink_delta`         | streaming IO   | streaming write into an Iceberg/Delta table |
|  [17]   | `LazyFrame.collect_batches / sink_batches`    | streaming IO   | iterate/emit result as `RecordBatch` stream out-of-core |
|  [18]   | `LazyFrame.explain / profile / show_graph`    | introspection  | optimized-plan text, per-node timing, plan DAG render |
|  [19]   | `LazyFrame.serialize / deserialize` / `remote()` | plan IO / cloud | round-trip a query plan; offload to Polars Cloud |
|  [20]   | `LazyFrame.with_context / match_to_schema / cache / set_sorted` | lazy aux | side-frame context, schema coercion, subplan cache, sortedness hint |
|  [21]   | `write_parquet / write_csv / write_delta / write_iceberg` | eager IO | write eager frame to storage             |
|  [22]   | `to_arrow(compat_level=)` / `to_pandas` / `to_numpy` / `__arrow_c_stream__` | interop | export to Arrow/pandas/NumPy; expose PyCapsule C-stream for zero-copy |
|  [23]   | `sql(query)` / `SQLContext` / `sql_expr`      | SQL            | run SQL over registered frames; parse a SQL fragment to `Expr` |
|  [24]   | `DataFrame.slice(offset, length)` / `DataFrame.height` / `DataFrame.head(n)` / `LazyFrame.head(n)` / `Series.to_frame()` / `Series.rename(name)` | row window / size / promote | row-offset slice, row count, first-n rows; promote a `Series` to a one-column `DataFrame`, rename a `Series` |

[ENTRYPOINT_SCOPE]: expression functions and namespaces
- rail: columnar dataframe

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY] | [RAIL]                                |
| :-----: | :------------------------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `col(*names)` / `all()` / `exclude` / `nth`        | selector       | reference columns in expressions      |
|  [02]   | `lit(value)` / `first` / `last` / `len` / `count`  | literal/agg    | scalar literals and frame aggregates  |
|  [03]   | `when(pred).then(x).otherwise(y)`                  | conditional    | branchwise expression                 |
|  [04]   | `sum/min/max/mean/median/std/var/quantile`         | aggregation    | column-wise aggregate expressions     |
|  [05]   | `sum_horizontal/min_horizontal/max_horizontal/...` | horizontal     | row-wise aggregate across columns     |
|  [06]   | `all_horizontal` / `any_horizontal`                | horizontal     | row-wise logical AND/OR               |
|  [07]   | `concat_str` / `concat_list` / `concat_arr`        | combine        | row-wise string/list concatenation    |
|  [08]   | `coalesce(*exprs)`                                 | null handling  | first non-null per row                |
|  [09]   | `struct(*exprs)` / `field(name)` / `format`        | struct/string  | build struct, access field, format    |
|  [10]   | `corr` / `cov` / `rolling_corr` / `rolling_cov`    | statistics     | correlation and covariance            |
|  [11]   | `int_range` / `date_range` / `datetime_range` / `linear_space` | range | integer/temporal/linear range-generating expressions |
|  [12]   | `concat(items, how)` / `align_frames` / `merge_sorted` | frame combine | concatenate, align, merge two sorted frames |
|  [13]   | `fold` / `reduce` / `cum_fold` / `cum_reduce`      | expr fold      | horizontal accumulate over a column set (typed reduction, no Python loop) |
|  [14]   | `Expr.over(partition_by, *, order_by=, mapping_strategy=)` | window  | windowed expression; ordered windows + group/explode mapping |
|  [15]   | `Expr.rolling(index_column, period=, offset=, closed=)` / `Expr.rolling_*` | window | time-anchored rolling windows + 20 rolling reducers (`rolling_mean`/`_quantile`/`_skew`/`_rank`/`_*_by`) |
|  [16]   | `Expr.cut` / `qcut` / `rle` / `rle_id` / `value_counts` / `hist` | binning | discretize, run-length encode, histogram |
|  [17]   | `Expr.ewm_mean` / `ewm_std` / `ewm_var` / `ewm_mean_by` | smoothing | exponentially-weighted moving statistics |
|  [18]   | `Expr.replace` / `replace_strict` / `is_in` / `is_close` | mapping | value remap (strict = exhaustive), membership, tolerance compare |
|  [19]   | `Expr.str` (49) / `Expr.dt` (47) / `Expr.list` (43) / `Expr.arr` (31) | namespace | string/temporal/list/array method families |
|  [20]   | `Expr.struct` / `Expr.cat` / `Expr.bin` / `.name` / `.meta` | namespace | struct, categorical, binary, naming, plan-metadata |
|  [21]   | `selectors` (`cs.numeric`/`by_dtype`/`matches`/`starts_with`/`exclude`/set algebra `&` `\|` `-`) | column selector | declarative dtype/name column sets composable with `&`/`\|`/`-` inside `select`/`with_columns` |
|  [22]   | `plugins.register_plugin_function(plugin_path, function_name, args, is_elementwise=, returns_scalar=, ...)` | native plugin | register a Rust expression-plugin kernel as a first-class `Expr` |
|  [23]   | `Expr.map_batches` / `Expr.map_elements` / `Expr.register_plugin` | escape hatch | Series/element Python UDFs; reserve for logic the expr API + plugins cannot express |

## [04]-[IMPLEMENTATION_LAW]

[COLUMNAR_TOPOLOGY]:
- `DataFrame` and `LazyFrame` share the transformation API; `LazyFrame` records a graph and `collect()` runs the optimizer. Optimizer passes are individually toggleable via `collect(optimizations=QueryOptFlags(...))` or boolean kwargs: `predicate_pushdown`, `projection_pushdown`, `slice_pushdown`, `comm_subplan_elim`, `comm_subexpr_elim`, `cluster_with_columns`, `collapse_joins`, `simplify_expression`.
- `collect(engine=)` selects the executor: `'auto'`/`'in-memory'`/`'streaming'` (out-of-core) or a `GPUEngine(...)` for cuDF/RAPIDS; `background=True` returns an `InProcessQuery`; `collect_batches`/`sink_batches` stream the result as `RecordBatch`es.
- expressions are lazy regardless of frame: `col("x") * 2` is an `Expr` until evaluated inside `select`, `with_columns`, `filter`, `group_by(...).agg`, or `Expr.over`. `DataTypeExpr`/`dtype_of`/`self_dtype` defer dtype decisions so schema-dependent logic stays in the graph.
- dtypes are value objects; `Datetime`/`Duration` carry a `time_unit`, `Categorical`/`Enum` carry a string-cache identity via `Categories`, `Struct`/`List`/`Array` nest other dtypes.
- `group_by_dynamic` and `Expr.rolling` provide temporal windowing; `Expr.over(order_by=, mapping_strategy=)` is the windowed-expression form; `join_asof` joins on sorted keys and `join_where` joins on inequality predicates.
- horizontal functions and `fold`/`reduce`/`cum_fold`/`cum_reduce` reduce across columns per row as typed expressions; vertical aggregates reduce down a column. `selectors` set-algebra (`cs.numeric() & ~cs.matches("_id$")`) addresses column sets without name strings.

[STACKING_LAW]:
- `scan_iceberg(table_obj, reader_override='native', use_pyiceberg_filter=True)` accepts a live `pyiceberg.table.Table` and pushes its row-filter through PyIceberg's planner: PyIceberg owns catalog/snapshot/manifest planning, Polars owns the scan/transform graph. `Table.to_polars()` (PyIceberg) is the lazy mirror returning a `LazyFrame`.
- `scan_pyarrow_dataset` and `from_arrow`/`from_dataframe` consume the `pyarrow` C-stream / interchange protocol zero-copy; `to_arrow(compat_level=)`/`__arrow_c_stream__` export the same way. Arrow `Table`/`RecordBatch` is the wire between Polars and pyarrow/DuckDB, never a Python row roundtrip.
- `register_plugin_function` (or a `#[polars_expr]` Rust crate behind it) is the first-class extension path: a custom geometry/numeric kernel becomes a real `Expr` node with pushdown, declared via `is_elementwise`/`returns_scalar`/`changes_length`. `polars_st` registers its `.st` ops exactly this way.

[LOCAL_ADMISSION]:
- Prefer `scan_*` plus a `LazyFrame` pipeline ending in `collect(engine='streaming')` or a `sink_*` over eager `read_*` to gain pushdown and out-of-core execution.
- Express transforms as `Expr` composition inside `select`/`with_columns`/`filter`/`group_by(...).agg`; address columns with `selectors`, use namespace accessors (`.str`/`.dt`/`.list`/`.arr`/`.struct`/`.cat`/`.bin`) and `fold`/`reduce` rather than Python loops.
- Use `when().then().otherwise()` for branching, horizontal functions/`fold` for row-wise reductions; reach for `register_plugin_function` (native) before `map_batches`/`map_elements` (Python) for logic the expression API cannot express.
- Enter and exit interop with `from_arrow`/`from_dataframe`/`scan_iceberg(Table)` and `to_arrow`/`__arrow_c_stream__`; wrap categorical-heavy joins in `StringCache`.

[RAIL_LAW]:
- Package: `polars`
- Owns: typed columnar dataframe storage, lazy query optimization, expression-based transformation, and the native expression-plugin host
- Accept: dicts, rows, NumPy/torch, Arrow tables/C-stream objects, pandas frames, Iceberg/Delta/pyarrow datasets, and file/database sources via `read_*`/`scan_*`
- Reject: per-element Python loops over rows, eager reads where a lazy scan admits pushdown, hand-rolled join/window/fold logic, Python `map_elements` where a native plugin or expr combinator exists, hardcoded column-name lists where `selectors` apply
