# [PY_DATA_API_NARWHALS]

`narwhals` owns the dataframe-agnostic translation layer: one frame, series, and expression surface drives any supported backend without materializing data. `from_native` and `@narwhalify` admit a native frame at the boundary, `to_native` extracts the backend object at egress, and the library unifies the dtype vocabulary and the backend-agnostic `Expr` combinator set so a consumer never branches on backend type.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `narwhals`
- package: `narwhals`
- owner: `data`
- module: `narwhals`
- namespaces: `narwhals.stable.v1` (version-frozen mirror of the full surface), `narwhals.selectors` (column-set algebra)
- license: MIT
- asset: pure Python, no compiled extension, zero hard backend dependency
- rail: dataframe-agnostic

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: frame and series types

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY]   | [CAPABILITY]                                |
| :-----: | :---------- | :-------------- | :------------------------------------------ |
|  [01]   | `DataFrame` | eager frame     | schema-validated multi-column eager frame   |
|  [02]   | `LazyFrame` | lazy frame      | deferred computation graph over any backend |
|  [03]   | `Series`    | typed column    | named typed 1-D column                      |
|  [04]   | `Expr`      | expression node | composable column expression                |
|  [05]   | `Schema`    | schema value    | ordered `{name: DType}` mapping             |
|  [06]   | `Field`     | schema field    | named typed field for `Struct` schemas      |

[PUBLIC_TYPE_SCOPE]: dtype vocabulary

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]  | [CAPABILITY]                    |
| :-----: | :----------------------------- | :------------- | :------------------------------ |
|  [01]   | `Int8/16/32/64/128`            | integer dtype  | signed integer dtypes           |
|  [02]   | `UInt8/16/32/64/128`           | integer dtype  | unsigned integer dtypes         |
|  [03]   | `Float16` / `Float32` / `Float64` | float dtype | IEEE float dtypes               |
|  [04]   | `Boolean`                      | boolean dtype  | boolean dtype                   |
|  [05]   | `String`                       | string dtype   | UTF-8 string dtype              |
|  [06]   | `Binary`                       | binary dtype   | opaque bytes dtype              |
|  [07]   | `Date`                         | temporal dtype | calendar date dtype             |
|  [08]   | `Datetime(unit, tz)`           | temporal dtype | timestamp dtype with tz         |
|  [09]   | `Duration(unit)`               | temporal dtype | duration dtype                  |
|  [10]   | `Time`                         | temporal dtype | time-of-day dtype               |
|  [11]   | `Categorical`                  | category dtype | categorical (unordered) dtype   |
|  [12]   | `Enum(categories)`             | category dtype | ordered categorical dtype       |
|  [13]   | `Struct(fields)`               | nested dtype   | struct dtype from field list    |
|  [14]   | `List(inner)` / `Array(inner)` | nested dtype   | variable/fixed-size list dtypes |
|  [15]   | `Decimal(precision, scale)`    | decimal dtype  | decimal dtype                   |
|  [16]   | `Object`                       | opaque dtype   | Python-object column dtype      |
|  [17]   | `Unknown`                      | fallback dtype | unmapped backend dtype          |
|  [18]   | `Implementation`               | enum           | backend identifier vocabulary   |

[PUBLIC_TYPE_SCOPE]: backend implementation enum

`Implementation` carries `PANDAS` `MODIN` `CUDF` `PYARROW` `PYSPARK` `POLARS` `DASK` `DUCKDB` `IBIS` `SQLFRAME` `PYSPARK_CONNECT` `UNKNOWN`; the pyarrow member is `PYARROW` (`PYARROW.value == 'pyarrow'`, never an `ARROW` member) and `PYSPARK_CONNECT` is distinct from `PYSPARK`. Fork on it only at a feature-gate point.

| [INDEX] | [SURFACE]                                  | [SHAPE]     | [CAPABILITY]                                       |
| :-----: | :----------------------------------------- | :---------- | :------------------------------------------------- |
|  [01]   | `Implementation.from_backend(backend)`     | classmethod | build from native namespace, string, or member     |
|  [02]   | `Implementation.from_native_namespace(ns)` | classmethod | build from an imported native namespace module     |
|  [03]   | `Implementation.to_native_namespace()`     | method      | return the native namespace module for the backend |
|  [04]   | `Implementation.name`                      | property    | member name; `PYARROW.name.lower() == 'pyarrow'`   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: frame and series construction

`from_native`/`narwhalify` share the `pass_through`/`eager_only`/`series_only`/`allow_series` flags; `pass_through=True` returns a non-narwhalifiable object unchanged. Every dict/Arrow/NumPy/IO factory takes `backend=` for the output backend.

| [INDEX] | [SURFACE]                                                              | [CAPABILITY]                                   |
| :-----: | :--------------------------------------------------------------------- | :--------------------------------------------- |
|  [01]   | `from_native(native, *, ...)`                                          | wrap any backend frame or series               |
|  [02]   | `to_native(narwhals_object, *, pass_through=False)`                    | unwrap to underlying native frame              |
|  [03]   | `narwhalify(func=None, *, ...)`                                        | auto-wrap / unwrap function arguments          |
|  [04]   | `from_dict(data, schema=None, *, backend=None, native_namespace=None)` | build from `{name: values}` mapping            |
|  [05]   | `from_dicts(data, schema=None, *, backend=None)`                       | build from list of row dicts                   |
|  [06]   | `from_arrow(native_frame, *, backend)`                                 | build from any `__arrow_c_stream__` table      |
|  [07]   | `from_numpy(data, schema=None, *, backend)`                            | build from 2-D NumPy array                     |
|  [08]   | `new_series(name, values, dtype=None, *, backend)`                     | create named typed series                      |
|  [09]   | `read_csv(source, *, backend, **kwargs)`                               | read CSV to eager `DataFrame`                  |
|  [10]   | `read_parquet(source, *, backend, **kwargs)`                           | read Parquet to eager `DataFrame`              |
|  [11]   | `scan_csv(source, *, backend, **kwargs)`                               | scan CSV to `LazyFrame`                        |
|  [12]   | `scan_parquet(source, *, backend, **kwargs)`                           | scan Parquet to `LazyFrame`                    |
|  [13]   | `get_native_namespace(*obj)`                                           | the native backend module for a wrapped object |
|  [14]   | `to_py_scalar(scalar_like)`                                            | coerce a backend scalar to a Python scalar     |
|  [15]   | `generate_temporary_column_name(n_bytes, columns)`                     | collision-free temp column name                |
|  [16]   | `is_ordered_categorical(series)`                                       | whether a categorical series is ordered        |
|  [17]   | `maybe_*`                                                              | pandas-index operations, no-op off pandas      |
|  [18]   | `show_versions()`                                                      | print narwhals + installed backend versions    |

- [17]-[PANDAS_ESCAPE]: `maybe_align_index`, `maybe_convert_dtypes`, `maybe_get_index`, `maybe_reset_index`, and `maybe_set_index` are pandas-index operations that no-op on non-pandas backends.

[ENTRYPOINT_SCOPE]: DataFrame operations

Every surface below is a `DataFrame` method.

| [INDEX] | [SURFACE]                                                | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------------- | :---------------------------------------------- |
|  [01]   | `select(*exprs, **named_exprs)`                          | select and transform columns                    |
|  [02]   | `with_columns(*exprs, **named_exprs)`                    | add or replace columns                          |
|  [03]   | `filter(*predicates, **constraints)`                     | row filter by expression                        |
|  [04]   | `group_by(*keys, drop_null_keys)`                        | grouped aggregation builder                     |
|  [05]   | `join(other, on, how, ...)`                              | hash join with strategy                         |
|  [06]   | `join_asof(other, left_on, right_on, ...)`               | asof (sorted) join                              |
|  [07]   | `sort(by, *more_by, descending, ...)`                    | sort by one or more columns                     |
|  [08]   | `rename(mapping)`                                        | rename columns                                  |
|  [09]   | `drop(*columns, strict)`                                 | drop named columns                              |
|  [10]   | `drop_nulls(subset)`                                     | drop rows with nulls in subset                  |
|  [11]   | `unique(subset, keep, ...)`                              | deduplicate rows                                |
|  [12]   | `collect_schema()`                                       | retrieve `Schema`                               |
|  [13]   | `get_column(name)`                                       | extract named `Series`                          |
|  [14]   | `lazy(backend, session)`                                 | convert to `LazyFrame`                          |
|  [15]   | `head(n)` / `tail(n)`                                    | first or last n rows                            |
|  [16]   | `pivot(on, index, values, ...)`                          | pivot to wide format                            |
|  [17]   | `unpivot(on, index)`                                     | melt to long format                             |
|  [18]   | `iter_rows(*, named, buffer_size)`                       | row-by-row iterator                             |
|  [19]   | `iter_columns()`                                         | iterate columns as `Series`                     |
|  [20]   | `row(index)` / `rows(*, named)`                          | single row tuple / all rows                     |
|  [21]   | `item(row, column)`                                      | single cell as Python scalar                    |
|  [22]   | `explode(columns, *more)`                                | explode list columns to rows                    |
|  [23]   | `gather_every(n, offset)`                                | every n-th row                                  |
|  [24]   | `sample(n, *, fraction, with_replacement, seed)`         | random row sample                               |
|  [25]   | `is_unique()` / `is_duplicated()` / `is_empty()`         | per-row uniqueness mask, empty check            |
|  [26]   | `estimated_size(unit)` / `shape` / `columns`             | in-memory size, `(rows, cols)`, column names    |
|  [27]   | `implementation`                                         | the backend `Implementation` member             |
|  [28]   | `clone()` / `pipe(fn, *a, **k)`                          | deep clone; apply a frame-level callable        |
|  [29]   | `null_count()`                                           | per-column null counts as one-row frame         |
|  [30]   | `from_dict` / `from_dicts` / `from_numpy` / `from_arrow` | backend-bound constructors on the class         |
|  [31]   | `write_csv(file)` / `write_parquet(file)`                | serialize to CSV / Parquet                      |
|  [32]   | `to_dict(*, as_series)` / `to_numpy()`                   | column dict (of `Series` or arrays) / 2-D array |
|  [33]   | `to_native()`                                            | unwrap to backend-native frame                  |
|  [34]   | `to_polars()` / `to_pandas()` / `to_arrow()`             | lower to a `polars`/`pandas`/`pyarrow` frame    |

[ENTRYPOINT_SCOPE]: LazyFrame operations

Every surface below is a `LazyFrame` method.

| [INDEX] | [SURFACE]                                                           | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------------ | :--------------------------------------- |
|  [01]   | `collect(backend, ...)`                                             | execute graph, return `DataFrame`        |
|  [02]   | `select(*exprs, **named_exprs)`                                     | select columns                           |
|  [03]   | `with_columns(*exprs, **named)`                                     | add or replace columns                   |
|  [04]   | `filter(*predicates, **constraints)`                                | row filter                               |
|  [05]   | `group_by(*keys, drop_null_keys)`                                   | grouped aggregation builder              |
|  [06]   | `join(other, on, how, ...)`                                         | hash join                                |
|  [07]   | `sort(by, *more_by, descending, ...)`                               | sort                                     |
|  [08]   | `sink_parquet(file)`                                                | streaming write to Parquet               |
|  [09]   | `with_row_index(name, *, order_by)`                                 | add row index column                     |
|  [10]   | `top_k(k, *, by, reverse)`                                          | top-k rows by column                     |
|  [11]   | `explode(columns, *more)`                                           | explode list columns to rows             |
|  [12]   | `gather_every(n, offset)`                                           | every n-th row                           |
|  [13]   | `drop(*columns, strict)` / `drop_nulls(subset)` / `rename(mapping)` | drop/rename/null-filter columns          |
|  [14]   | `unique(subset, *, keep)` / `unpivot(on, index)`                    | deduplicate / melt to long               |
|  [15]   | `head(n)` / `tail(n)`                                               | first or last n rows                     |
|  [16]   | `columns` / `implementation`                                        | column names; backend identity           |
|  [17]   | `pipe(fn, *a, **k)`                                                 | apply a lazy-frame-level callable        |
|  [18]   | `collect_schema()`                                                  | retrieve `Schema` without execute        |
|  [19]   | `lazy()` / `to_native()`                                            | identity / unwrap to backend lazy object |

[ENTRYPOINT_SCOPE]: grouped aggregation

`group_by(*keys, drop_null_keys=False)` returns a `GroupBy` builder on both `DataFrame` and `LazyFrame`, closing with `.agg(*exprs)` over expression nodes. Grouped aggregation is the single aggregation surface — no per-stat method family on the group object.

| [INDEX] | [SURFACE]                                                      | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------- | :---------------------------------------- |
|  [01]   | `(DataFrame\|LazyFrame).group_by(*keys, drop_null_keys=False)` | open a grouped builder                    |
|  [02]   | `GroupBy.agg(*aggs, **named_aggs)`                             | aggregate each group by expressions       |
|  [03]   | `GroupBy.__iter__()`                                           | iterate `(key, sub-frame)` groups (eager) |

[ENTRYPOINT_SCOPE]: expression and aggregation functions

Module-level expression and frame constructors.

| [INDEX] | [SURFACE]                                                      | [CAPABILITY]                                 |
| :-----: | :------------------------------------------------------------- | :------------------------------------------- |
|  [01]   | `col(*names)`                                                  | select named columns                         |
|  [02]   | `all()`                                                        | select all columns                           |
|  [03]   | `exclude(*names)`                                              | select all except named columns              |
|  [04]   | `nth(*indices)`                                                | select columns by position                   |
|  [05]   | `lit(value, dtype)`                                            | scalar literal expression                    |
|  [06]   | `len()`                                                        | row count expression                         |
|  [07]   | `sum/min/max/mean/median(*columns)`                            | column-wise aggregate expressions            |
|  [08]   | `sum_horizontal/min_horizontal/max_horizontal/mean_horizontal` | row-wise aggregate expressions               |
|  [09]   | `all_horizontal(*exprs, ignore_nulls)`                         | row-wise logical AND                         |
|  [10]   | `any_horizontal(*exprs, ignore_nulls)`                         | row-wise logical OR                          |
|  [11]   | `concat_str(exprs, separator, ignore_nulls)`                   | row-wise string concatenation                |
|  [12]   | `coalesce(exprs, *more_exprs)`                                 | first non-null value per row                 |
|  [13]   | `when(*predicates)`                                            | `when(...).then(...).otherwise(...)` builder |
|  [14]   | `concat(items, how)`                                           | concatenate frames vertically or diag        |
|  [15]   | `corr(a, b, method)`                                           | Pearson or Spearman correlation              |
|  [16]   | `cov(a, b, *, ddof)`                                           | sample covariance of two columns             |
|  [17]   | `struct(*exprs, **named_exprs)`                                | build struct column expression               |
|  [18]   | `list(*exprs)`                                                 | combine columns row-wise into a list column  |
|  [19]   | `format(f_string, *args)`                                      | format string expression                     |

[ENTRYPOINT_SCOPE]: Expr combinators

`Expr` is the composable column node from `col()`/`lit()`/`nth()`/`when()`; every method returns `Self` so expressions chain, and every surface below is an `Expr` method. `over` and the window/rolling/cumulative/ranking family are the backend-agnostic analytic surface — never drop to a native window spec. `rolling_*` aggregates take `(window_size, *, min_samples=None, center=False)`; `ewm_mean` takes `(*, com=None, span=None, half_life=None, alpha=None, adjust=True, min_samples=1, ignore_nulls=False)`.

| [INDEX] | [SURFACE]                                                                | [CAPABILITY]                            |
| :-----: | :----------------------------------------------------------------------- | :-------------------------------------- |
|  [01]   | `alias(name)` / `cast(dtype)`                                            | rename node; cast to a narwhals dtype   |
|  [02]   | `over(*partition_by, order_by=None)`                                     | window/partitioned aggregation          |
|  [03]   | `cum_sum/cum_prod/cum_min/cum_max/cum_count(*, reverse=False)`           | running aggregates                      |
|  [04]   | `rolling_sum/rolling_mean/rolling_std/rolling_var(...)`                  | sliding-window aggregates               |
|  [05]   | `ewm_mean(...)`                                                          | exponentially-weighted mean             |
|  [06]   | `shift(n)` / `diff()` / `rank(method, *, descending)`                    | lag, first difference, rank             |
|  [07]   | `fill_null(value, strategy, limit)` / `fill_nan(value)`                  | impute nulls / NaNs                     |
|  [08]   | `is_null/is_nan/is_finite/is_in(other)/is_between(lower, upper, closed)` | boolean masks                           |
|  [09]   | `is_first_distinct/is_last_distinct/is_duplicated/is_unique/is_close`    | distinctness / approx-equality masks    |
|  [10]   | `clip(lower_bound, upper_bound)` / `abs/round(decimals)/floor/ceil`      | bound and round                         |
|  [11]   | `sum/min/max/mean/median/std/var/quantile(q, interpolation)`             | scalar reductions                       |
|  [12]   | `n_unique/count/null_count/len/first/last/mode/skew/kurtosis`            | distinct/extreme counts, shape stats    |
|  [13]   | `replace_strict(old, new=None, *, default, return_dtype=None)`           | exhaustive value remap with default     |
|  [14]   | `map_batches(function, return_dtype=None, *, returns_scalar=False)`      | apply a native-frame callable per chunk |
|  [15]   | `pipe(function, *args, **kwargs)`                                        | thread the expr through a callable      |
|  [16]   | `exp/log(base)/sqrt/sin/cos`                                             | elementwise transcendental ops          |
|  [17]   | `drop_nulls/unique/filter(*predicates)/any/all/any_value`                | within-expr row filters and reductions  |

[ENTRYPOINT_SCOPE]: Expr / Series namespace accessors

Typed namespace accessors carry the per-dtype combinators; they are the only path to temporal/string/list/struct work, identical on `Expr` and `Series`. Branching on backend type for string or date work is rejected — use the namespace.

| [INDEX] | [NAMESPACE] | [CAPABILITY]                                |
| :-----: | :---------- | :------------------------------------------ |
|  [01]   | `.dt`       | temporal component and arithmetic accessors |
|  [02]   | `.str`      | string manipulation accessors               |
|  [03]   | `.cat`      | categorical accessor                        |
|  [04]   | `.list`     | list-column accessors                       |
|  [05]   | `.struct`   | struct-field accessor                       |
|  [06]   | `.name`     | column-name transforms                      |

- [01]-[DT]: `year`/`month`/`day`/`hour`/`minute`/`second`/`millisecond`/`microsecond`/`nanosecond`/`ordinal_day`/`weekday`/`date`/`to_string`/`timestamp`/`truncate`/`offset_by`/`replace_time_zone`/`convert_time_zone`/`total_seconds`/`total_milliseconds`/`total_microseconds`/`total_nanoseconds`/`total_minutes`.
- [02]-[STR]: `len_chars`/`contains`/`starts_with`/`ends_with`/`slice`/`head`/`tail`/`split`/`replace`/`replace_all`/`strip_chars`/`pad_start`/`pad_end`/`zfill`/`to_lowercase`/`to_uppercase`/`to_titlecase`/`to_date`/`to_datetime`/`to_time`.
- [03]-[CAT]: `get_categories`.
- [04]-[LIST]: `len`/`get(index)`/`contains(item)`/`unique`/`sort`/`min`/`max`/`sum`/`mean`/`median`.
- [05]-[STRUCT]: `field(name)`.
- [06]-[NAME]: `keep`/`map(function)`/`prefix(prefix)`/`suffix(suffix)`/`to_lowercase`/`to_uppercase`.

[ENTRYPOINT_SCOPE]: Series eager-only operations

`Series` shares every `Expr` combinator and namespace and, being eager, adds materialized accessors. Reach `Series` only past a `DataFrame.get_column`/`iter_columns` boundary; lazy paths stay in `Expr`. Every surface below is a `Series` method.

| [INDEX] | [SURFACE]                                                                 | [CAPABILITY]                         |
| :-----: | :------------------------------------------------------------------------ | :----------------------------------- |
|  [01]   | `from_numpy(name, values, dtype=None, *, backend)` / `from_iterable(...)` | build a typed series                 |
|  [02]   | `to_list()` / `to_numpy()` / `to_arrow()` / `to_frame()` / `to_dummies()` | materialize to Python/Arrow/frame    |
|  [03]   | `to_native()` / `to_polars()` / `to_pandas()`                             | unwrap or lower to a backend series  |
|  [04]   | `value_counts()` / `hist(bins)`                                           | frequency table / histogram frame    |
|  [05]   | `arg_min()` / `arg_max()` / `arg_true()`                                  | position of extreme / true values    |
|  [06]   | `scatter(indices, values)` / `zip_with(mask, other)`                      | positional set / masked combine      |
|  [07]   | `item(index)` / `is_sorted(*, descending)` / `is_empty()`                 | single value, sortedness, emptiness  |
|  [08]   | `dtype` / `name` / `shape` / `implementation`                             | dtype, name, shape, backend identity |
|  [09]   | `sort(*, descending, nulls_last)` / `rename(name)` / `sample(...)`        | sort, rename, sample                 |

[ENTRYPOINT_SCOPE]: selectors module (`narwhals.selectors`)

Column selectors are composable expression-like objects accepted anywhere an expression is, with `&`/`|`/`~` set algebra; they replace stringly column-name globbing. Import as `import narwhals.selectors as ncs`.

| [INDEX] | [SURFACE]                                                | [CAPABILITY]                       |
| :-----: | :------------------------------------------------------- | :--------------------------------- |
|  [01]   | `by_dtype(*dtypes)`                                      | columns matching narwhals dtypes   |
|  [02]   | `numeric()` / `boolean()` / `string()` / `categorical()` | columns by broad type class        |
|  [03]   | `enum()`                                                | `Enum`-dtype columns               |
|  [04]   | `datetime(time_unit=None, time_zone=('*', None))`        | temporal columns by unit/zone      |
|  [05]   | `matches(pattern)`                                       | columns whose name matches a regex |
|  [06]   | `all()`                                                  | all columns                        |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `from_native`/`@narwhalify` are the sole intake and `to_native` the sole export; `pass_through=True` flows a non-narwhalifiable object through unchanged.
- `DataFrame` and `LazyFrame` mirror one transformation API; `LazyFrame.collect(backend=)` materializes and may retarget the output backend.
- `Expr`'s analytic surface and the typed namespaces are backend-agnostic; `map_batches` is the sole escape hatch to a native callable.
- dtype objects are value-equal by class identity (`Int32() == Int32()`); `Object` carries opaque Python objects and `Unknown` is the unmapped-backend fallback.
- `narwhals.stable.v1` freezes the surface behind a version-independent contract: a durable library boundary imports it, a consumer riding new combinators imports `narwhals`.

[STACKING]:
- `nanoarrow`(`.api/nanoarrow.md`): a `nanoarrow.Array`/`ArrayStream` capsule enters `from_arrow(table, backend=)` zero-copy — dtype and null mask ride the `__arrow_c_stream__` capsule, never a Python-list hop.
- `pyarrow`(`.api/pyarrow.md`), `polars`(`.api/polars.md`), `pandas`(`.api/pandas.md`): `to_arrow`/`to_polars`/`to_pandas` lower a wrapped frame to the concrete backend at egress, and `from_native` re-wraps any of them.
- `msgspec`(`libs/python/.api/msgspec.md`): `iter_rows(named=True)`/`to_dict(as_series=False)` feed a `Struct` decode, and `from_dicts(rows, schema=)` is the validated inverse that enforces the schema once.
- `numpy`(`libs/python/.api/numpy.md`): `from_numpy(arr, schema, backend=)` and `Series.from_numpy` lift the raw NumPy buffers a mesh or point-cloud decode produces into a typed frame/column, dtype precise rather than `Object`.
- within-lib: `data/tabular/interop` `FrameAdmission`/`FrameInterop` own the intake/export boundary that `compute` studies consume; a plane builds one `LazyFrame` graph (`scan_parquet -> with_columns(Expr) -> group_by.agg -> sort`) and calls `collect(backend=)` once, the backend a leaf parameter, never an `if Implementation` branch around parallel paths.

[LOCAL_ADMISSION]:
- `@narwhalify` at a function boundary accepts any backend frame and returns the same backend type.
- compose `Expr` via `col()`/`when()`/`lit()`/`nth()`, the `selectors` module, and namespace accessors inside `select`/`with_columns`/`GroupBy.agg`; never branch on frame type in the body.
- `backend=` on `from_dict`/`from_arrow`/`read_csv`/`new_series` selects the output backend; omit it only when the input already carries a native backend identity.
- `scan_csv`/`scan_parquet` return `LazyFrame` for deferred paths; stream the result with `LazyFrame.sink_parquet`.
- reach `maybe_*` only at a pandas-specific boundary; they no-op off pandas.

[RAIL_LAW]:
- Package: `narwhals`
- Owns: the dataframe-agnostic surface over every supported backend, the full `Expr`/`Series` combinator set, the typed namespaces, `selectors`, and the version-frozen `stable.v1` mirror.
- Accept: any `from_native`-compatible frame, series, or `__arrow_c_stream__` table, plus dicts, row-dicts, and NumPy arrays with an explicit `backend=`.
- Reject: a direct backend API call inside a wrapped function; an `Implementation` branch on a non-feature-gated path; a drop to a native window/string/date spec the `Expr` namespace owns; a Python-list hop where `from_arrow` passes an Arrow capsule straight through.
