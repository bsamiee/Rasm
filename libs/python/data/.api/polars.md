# [PY_DATA_API_POLARS]

`polars` is a Rust-backed columnar dataframe engine exposing eager `DataFrame`/`Series` and lazy `LazyFrame` surfaces that share one transformation API built from `Expr` expression nodes. The lazy path defers a query graph until `collect` (or streaming `sink_*`), enabling predicate/projection pushdown and out-of-core execution. A typed dtype vocabulary (`Int8`..`Int128`, `Float32/64`, `Datetime`, `Duration`, `Categorical`, `Enum`, `Struct`, `List`, `Array`, `Decimal`) backs every column, and the top-level functions (`col`, `lit`, `when`, `concat`, horizontal aggregates, `read_*`/`scan_*` IO) compose expressions and frames without materializing intermediate results.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `polars`
- package: `polars`
- module: `polars`
- asset: Rust extension (`polars-runtime-32`)
- rail: columnar dataframe

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
|  [10]   | `GPUEngine`   | engine selector | CUDA execution backend for `collect`       |

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
|  [03]   | `from_arrow` / `from_pandas` / `from_numpy`     | interop        | build from Arrow, pandas, NumPy   |
|  [04]   | `from_dataframe(df)`                            | interop        | build from any Arrow C interface  |
|  [05]   | `read_csv` / `read_parquet` / `read_ipc`        | eager IO       | read files into `DataFrame`       |
|  [06]   | `read_json` / `read_ndjson` / `read_avro`       | eager IO       | read structured text/binary       |
|  [07]   | `read_database` / `read_database_uri`           | eager IO       | read from a SQL connection        |
|  [08]   | `read_delta` / `read_excel` / `read_ods`        | eager IO       | read table-store and spreadsheet  |
|  [09]   | `scan_csv` / `scan_parquet` / `scan_ipc`        | lazy IO        | scan files into `LazyFrame`       |
|  [10]   | `scan_ndjson` / `scan_delta` / `scan_iceberg`   | lazy IO        | scan structured and table-store   |
|  [11]   | `scan_pyarrow_dataset(ds)`                      | lazy IO        | scan an Arrow dataset lazily      |
|  [12]   | `read_parquet_metadata` / `read_parquet_schema` | metadata       | inspect Parquet without full read |

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
|  [14]   | `lazy()` / `collect()` / `collect_schema`     | execution      | switch to lazy, materialize, peek schema |
|  [15]   | `LazyFrame.sink_parquet/sink_csv/sink_ipc`    | streaming IO   | streaming write without full collect     |
|  [16]   | `write_parquet / write_csv / write_delta`     | eager IO       | write eager frame to storage             |
|  [17]   | `to_arrow` / `to_pandas` / `to_numpy`         | interop        | export to Arrow, pandas, NumPy           |
|  [18]   | `sql(query)` / `SQLContext`                   | SQL            | run SQL over registered frames           |

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
|  [11]   | `int_range` / `date_range` / `datetime_range`      | range          | range-generating expressions          |
|  [12]   | `concat(items, how)` / `align_frames`              | frame combine  | concatenate and align frames          |
|  [13]   | `Expr.over(*keys)`                                 | window         | windowed expression by group          |
|  [14]   | `Expr.str` / `Expr.dt` / `Expr.list` / `Expr.arr`  | namespace      | string, temporal, list, array methods |
|  [15]   | `Expr.struct` / `Expr.cat` / `Expr.bin` / `.name`  | namespace      | struct, categorical, binary, naming   |
|  [16]   | `Expr.map_batches` / `Expr.map_elements`           | escape hatch   | apply Python UDFs over a column       |

## [04]-[IMPLEMENTATION_LAW]

[COLUMNAR_TOPOLOGY]:
- `DataFrame` and `LazyFrame` share the transformation API; `LazyFrame` records a graph and `collect()` runs the optimizer (predicate/projection pushdown, common-subplan elimination)
- expressions are lazy regardless of frame: `col("x") * 2` is an `Expr` until evaluated inside `select`, `with_columns`, `filter`, `group_by(...).agg`, or `Expr.over`
- dtypes are value objects; `Datetime`/`Duration` carry a `time_unit`, `Categorical`/`Enum` carry a string-cache identity, `Struct`/`List`/`Array` nest other dtypes
- streaming `sink_parquet`/`sink_csv`/`sink_ipc` write a `LazyFrame` out of core without a full in-memory materialization
- `group_by_dynamic` and `rolling` provide temporal windowing; `join_asof` joins on sorted keys and `join_where` joins on inequality predicates
- horizontal functions reduce across columns per row; vertical aggregates reduce down a column

[LOCAL_ADMISSION]:
- Prefer `scan_*` plus a `LazyFrame` pipeline ending in `collect()` or a `sink_*` over eager `read_*` to gain pushdown and out-of-core execution.
- Express transforms as `Expr` composition inside `select`/`with_columns`/`filter`/`group_by(...).agg`; use namespace accessors (`.str`, `.dt`, `.list`, `.struct`, `.cat`) rather than Python loops.
- Use `when().then().otherwise()` for branching and horizontal functions for row-wise reductions; reserve `map_batches`/`map_elements` for logic the expression API cannot express.
- Enter and exit interop with `from_arrow`/`from_pandas`/`from_dataframe` and `to_arrow`/`to_pandas`; wrap categorical-heavy joins in `StringCache`.

[RAIL_LAW]:
- Package: `polars`
- Owns: typed columnar dataframe storage, lazy query optimization, and expression-based transformation
- Accept: dicts, rows, NumPy, Arrow tables, pandas frames, and file/database sources via `read_*`/`scan_*`
- Reject: per-element Python loops over rows, eager reads where a lazy scan admits pushdown, hand-rolled join or window logic
