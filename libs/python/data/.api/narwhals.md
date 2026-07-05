# [PY_DATA_API_NARWHALS]

`narwhals` supplies a dataframe-agnostic translation layer that wraps Polars, pandas, Modin, cuDF, and any Arrow-compatible backend behind a single `DataFrame`, `LazyFrame`, `Series`, and `Expr` surface. Consumers call `from_native` or apply the `@narwhalify` decorator to accept any supported native frame and operate through the narwhals API; `to_native` extracts the underlying backend object at the boundary. The library owns dtype unification (`Int8`..`Float64`, `Datetime`, `Duration`, `Categorical`, `Enum`, `Struct`, `List`, etc.) and horizontal/aggregation expression combinators without materializing data itself.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `narwhals`
- package: `narwhals`
- owner: `data`
- module: `narwhals`
- version: `2.22.1`
- license: MIT
- asset: pure Python (no compiled extension); zero hard backend dependency
- rail: dataframe-agnostic
- api gate: the main `narwhals` namespace tracks the live release; `narwhals.stable.v1` is the version-pinned mirror (`from_native`/`narwhalify`/`col`/`Expr`/dtypes all re-exported) that freezes behavior across narwhals upgrades. A library that must not break on a narwhals bump imports from `narwhals.stable.v1`; a consumer riding the newest combinators imports from `narwhals`.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: frame and series types
- rail: dataframe-agnostic

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY]   | [ROLE]                                      |
| :-----: | :---------- | :-------------- | :------------------------------------------ |
|  [01]   | `DataFrame` | eager frame     | schema-validated multi-column eager frame   |
|  [02]   | `LazyFrame` | lazy frame      | deferred computation graph over any backend |
|  [03]   | `Series`    | typed column    | named typed 1-D column                      |
|  [04]   | `Expr`      | expression node | composable column expression                |
|  [05]   | `Schema`    | schema value    | ordered `{name: DType}` mapping             |
|  [06]   | `Field`     | schema field    | named typed field for `Struct` schemas      |

[PUBLIC_TYPE_SCOPE]: dtype vocabulary
- rail: dataframe-agnostic

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]  | [ROLE]                          |
| :-----: | :----------------------------- | :------------- | :------------------------------ |
|  [01]   | `Int8/16/32/64/128`            | integer dtype  | signed integer dtypes           |
|  [02]   | `UInt8/16/32/64/128`           | integer dtype  | unsigned integer dtypes         |
|  [03]   | `Float32` / `Float64`          | float dtype    | IEEE float dtypes               |
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
- rail: dataframe-agnostic

| [INDEX] | [SYMBOL]                                                                                      | [TYPE_FAMILY] | [ROLE]                                                                  |
| :-----: | :-------------------------------------------------------------------------------------------- | :------------ | :---------------------------------------------------------------------- |
|  [01]   | `Implementation.{PANDAS,MODIN,CUDF,PYARROW,PYSPARK,PYSPARK_CONNECT,POLARS,DASK,DUCKDB,IBIS,SQLFRAME,UNKNOWN}` | enum member   | backend identity; `PYARROW.value == 'pyarrow'`, never an `ARROW` member; `PYSPARK_CONNECT` is distinct from `PYSPARK` |
|  [02]   | `Implementation.from_backend(backend)`                                                        | classmethod   | build from native namespace module, string, or `Implementation`         |
|  [03]   | `Implementation.from_native_namespace(ns)`                                                    | classmethod   | build from an imported native namespace module                          |
|  [04]   | `Implementation.to_native_namespace()`                                                        | method        | return the native namespace module for the backend                      |
|  [05]   | `Implementation.name`                                                                         | enum attr     | member name; `PYARROW.name.lower() == 'pyarrow'`                        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: frame and series construction
- rail: dataframe-agnostic

| [INDEX] | [SURFACE]                                                                                      | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :--------------------------------------------------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `from_native(native, *, pass_through=False, eager_only=False, series_only=False, allow_series=None)` | intake adapter | wrap any backend frame or series; `pass_through` returns the object unchanged when not narwhalifiable |
|  [02]   | `to_native(narwhals_object, *, pass_through=False)`                                             | export adapter | unwrap to underlying native frame               |
|  [03]   | `narwhalify(func=None, *, pass_through=True, eager_only=False, series_only=False, allow_series=True)` | decorator | auto-wrap / unwrap function arguments      |
|  [04]   | `from_dict(data, schema=None, *, backend=None, native_namespace=None)`                          | dict intake    | build from `{name: values}` mapping             |
|  [05]   | `from_dicts(data, schema=None, *, backend=None)`                                                | dict intake    | build from list of row dicts                    |
|  [06]   | `from_arrow(native_frame, *, backend)`                                                          | Arrow intake   | build from any `__arrow_c_stream__` table       |
|  [07]   | `from_numpy(data, schema=None, *, backend)`                                                     | numpy intake   | build from 2-D NumPy array                      |
|  [08]   | `new_series(name, values, dtype=None, *, backend)`                                              | series factory | create named typed series                       |
|  [09]   | `read_csv(source, *, backend, **kwargs)`                                                        | IO intake      | read CSV to eager `DataFrame`                   |
|  [10]   | `read_parquet(source, *, backend, **kwargs)`                                                    | IO intake      | read Parquet to eager `DataFrame`               |
|  [11]   | `scan_csv(source, *, backend, **kwargs)`                                                        | IO intake      | scan CSV to `LazyFrame`                         |
|  [12]   | `scan_parquet(source, *, backend, **kwargs)`                                                    | IO intake      | scan Parquet to `LazyFrame`                     |
|  [13]   | `get_native_namespace(*obj)`                                                                    | introspection  | the native backend module for a wrapped object  |
|  [14]   | `to_py_scalar(scalar_like)`                                                                      | scalar export  | coerce a backend scalar to a Python scalar      |
|  [15]   | `generate_temporary_column_name(n_bytes, columns)`                                              | naming         | collision-free temp column name                 |
|  [16]   | `is_ordered_categorical(series)`                                                                | introspection  | whether a categorical series is ordered          |
|  [17]   | `maybe_align_index` / `maybe_convert_dtypes` / `maybe_get_index` / `maybe_reset_index` / `maybe_set_index` | pandas escape | pandas-index operations that no-op on non-pandas backends |
|  [18]   | `show_versions()`                                                                               | diagnostics    | print narwhals + installed backend versions     |

[ENTRYPOINT_SCOPE]: DataFrame operations
- rail: dataframe-agnostic

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY] | [RAIL]                                  |
| :-----: | :--------------------------------------------------- | :------------- | :-------------------------------------- |
|  [01]   | `DataFrame.select(*exprs, **named_exprs)`            | projection     | select and transform columns            |
|  [02]   | `DataFrame.with_columns(*exprs, **named_exprs)`      | mutation       | add or replace columns                  |
|  [03]   | `DataFrame.filter(*predicates, **constraints)`       | filter         | row filter by expression                |
|  [04]   | `DataFrame.group_by(*keys, drop_null_keys)`          | aggregation    | grouped aggregation builder             |
|  [05]   | `DataFrame.join(other, on, how, ...)`                | join           | hash join with strategy                 |
|  [06]   | `DataFrame.join_asof(other, left_on, right_on, ...)` | join           | asof (sorted) join                      |
|  [07]   | `DataFrame.sort(by, *more_by, descending, ...)`      | sort           | sort by one or more columns             |
|  [08]   | `DataFrame.rename(mapping)`                          | mutation       | rename columns                          |
|  [09]   | `DataFrame.drop(*columns, strict)`                   | projection     | drop named columns                      |
|  [10]   | `DataFrame.drop_nulls(subset)`                       | filter         | drop rows with nulls in subset          |
|  [11]   | `DataFrame.unique(subset, keep, ...)`                | dedup          | deduplicate rows                        |
|  [12]   | `DataFrame.collect_schema()`                         | metadata       | retrieve `Schema`                       |
|  [13]   | `DataFrame.get_column(name)`                         | column access  | extract named `Series`                  |
|  [14]   | `DataFrame.lazy(backend, session)`                   | deferred       | convert to `LazyFrame`                  |
|  [15]   | `DataFrame.head(n)` / `.tail(n)`                     | window         | first or last n rows                    |
|  [16]   | `DataFrame.pivot(on, index, values, ...)`            | reshape        | pivot to wide format                    |
|  [17]   | `DataFrame.unpivot(on, index)`                       | reshape        | melt to long format                     |
|  [18]   | `DataFrame.iter_rows(*, named, buffer_size)`         | iteration      | row-by-row iterator                     |
|  [19]   | `DataFrame.iter_columns()`                           | iteration      | iterate columns as `Series`             |
|  [20]   | `DataFrame.row(index)` / `DataFrame.rows(*, named)`  | row access     | single row tuple / all rows             |
|  [21]   | `DataFrame.item(row, column)`                        | scalar access  | single cell as Python scalar            |
|  [22]   | `DataFrame.explode(columns, *more)`                  | reshape        | explode list columns to rows            |
|  [23]   | `DataFrame.gather_every(n, offset)`                  | sampling       | every n-th row                          |
|  [24]   | `DataFrame.sample(n, *, fraction, with_replacement, seed)` | sampling | random row sample                       |
|  [25]   | `DataFrame.is_unique()` / `is_duplicated()` / `is_empty()` | predicate | per-row uniqueness mask, empty check    |
|  [26]   | `DataFrame.estimated_size(unit)` / `DataFrame.shape` / `DataFrame.columns` | metadata | in-memory size, `(rows, cols)`, column names |
|  [27]   | `DataFrame.implementation`                           | metadata       | the backend `Implementation` member     |
|  [28]   | `DataFrame.clone()` / `DataFrame.pipe(fn, *a, **k)`  | clone/compose  | deep clone; apply a frame-level callable |
|  [29]   | `DataFrame.null_count()`                             | metadata       | per-column null counts as one-row frame |
|  [30]   | `DataFrame.from_dict` / `from_dicts` / `from_numpy` / `from_arrow` | classmethod | backend-bound constructors on the class |
|  [31]   | `DataFrame.write_csv(file)` / `DataFrame.write_parquet(file)` | IO export | serialize to CSV / Parquet              |
|  [32]   | `DataFrame.to_dict(*, as_series)` / `to_numpy()`     | export         | column dict (of `Series` or arrays) / 2-D array |
|  [33]   | `DataFrame.to_native()`                              | export         | unwrap to backend-native frame          |
|  [34]   | `DataFrame.to_polars()` / `to_pandas()` / `to_arrow()` | export       | lower to `polars.DataFrame` / `pandas.DataFrame` / `pyarrow.Table` |

[ENTRYPOINT_SCOPE]: LazyFrame operations
- rail: dataframe-agnostic

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :---------------------------------------------- | :------------- | :-------------------------------- |
|  [01]   | `LazyFrame.collect(backend, ...)`               | materialise    | execute graph, return `DataFrame` |
|  [02]   | `LazyFrame.select(*exprs, **named_exprs)`       | projection     | select columns                    |
|  [03]   | `LazyFrame.with_columns(*exprs, **named)`       | mutation       | add or replace columns            |
|  [04]   | `LazyFrame.filter(*predicates, **constraints)`  | filter         | row filter                        |
|  [05]   | `LazyFrame.group_by(*keys, drop_null_keys)`     | aggregation    | grouped aggregation builder       |
|  [06]   | `LazyFrame.join(other, on, how, ...)`           | join           | hash join                         |
|  [07]   | `LazyFrame.sort(by, *more_by, descending, ...)` | sort           | sort                              |
|  [08]   | `LazyFrame.sink_parquet(file)`                  | IO export      | streaming write to Parquet        |
|  [09]   | `LazyFrame.with_row_index(name, *, order_by)`   | mutation       | add row index column              |
|  [10]   | `LazyFrame.top_k(k, *, by, reverse)`            | ranking        | top-k rows by column              |
|  [11]   | `LazyFrame.explode(columns, *more)`             | reshape        | explode list columns to rows      |
|  [12]   | `LazyFrame.gather_every(n, offset)`             | sampling       | every n-th row                    |
|  [13]   | `LazyFrame.drop(*columns, strict)` / `drop_nulls(subset)` / `rename(mapping)` | mutation | drop/rename/null-filter columns   |
|  [14]   | `LazyFrame.unique(subset, *, keep)` / `unpivot(on, index)` | reshape | deduplicate / melt to long        |
|  [15]   | `LazyFrame.head(n)` / `tail(n)`                 | window         | first or last n rows              |
|  [16]   | `LazyFrame.columns` / `LazyFrame.implementation`| metadata       | column names; backend identity    |
|  [17]   | `LazyFrame.pipe(fn, *a, **k)`                   | compose        | apply a lazy-frame-level callable |
|  [18]   | `LazyFrame.collect_schema()`                    | metadata       | retrieve `Schema` without execute |
|  [19]   | `LazyFrame.lazy()` / `LazyFrame.to_native()`    | export         | identity / unwrap to backend lazy object |

[ENTRYPOINT_SCOPE]: grouped aggregation
- rail: dataframe-agnostic

`group_by(*keys, drop_null_keys=False)` returns a `GroupBy` builder on both `DataFrame` and `LazyFrame`; the builder closes with `.agg(*exprs)` over expression nodes. Grouped aggregation is the single aggregation surface — no per-stat method family on the group object.

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [RAIL]                                  |
| :-----: | :---------------------------------------------- | :------------- | :-------------------------------------- |
|  [01]   | `(DataFrame\|LazyFrame).group_by(*keys, drop_null_keys=False)` | aggregation | open a grouped builder       |
|  [02]   | `GroupBy.agg(*aggs, **named_aggs)`              | aggregation    | aggregate each group by expressions     |
|  [03]   | `GroupBy.__iter__()`                            | iteration      | iterate `(key, sub-frame)` groups (eager) |

[ENTRYPOINT_SCOPE]: expression and aggregation functions
- rail: dataframe-agnostic

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `col(*names)`                                                  | selector       | select named columns                         |
|  [02]   | `all()`                                                        | selector       | select all columns                           |
|  [03]   | `exclude(*names)`                                              | selector       | select all except named columns              |
|  [04]   | `nth(*indices)`                                                | selector       | select columns by position                   |
|  [05]   | `lit(value, dtype)`                                            | literal        | scalar literal expression                    |
|  [06]   | `len()`                                                        | aggregation    | row count expression                         |
|  [07]   | `sum/min/max/mean/median(*columns)`                            | aggregation    | column-wise aggregate expressions            |
|  [08]   | `sum_horizontal/min_horizontal/max_horizontal/mean_horizontal` | horizontal     | row-wise aggregate expressions               |
|  [09]   | `all_horizontal(*exprs, ignore_nulls)`                         | horizontal     | row-wise logical AND                         |
|  [10]   | `any_horizontal(*exprs, ignore_nulls)`                         | horizontal     | row-wise logical OR                          |
|  [11]   | `concat_str(exprs, separator, ignore_nulls)`                   | string combine | row-wise string concatenation                |
|  [12]   | `coalesce(exprs, *more_exprs)`                                 | null handling  | first non-null value per row                 |
|  [13]   | `when(*predicates)`                                            | conditional    | `when(...).then(...).otherwise(...)` builder |
|  [14]   | `concat(items, how)`                                           | frame combine  | concatenate frames vertically or diag        |
|  [15]   | `corr(a, b, method)`                                           | statistics     | Pearson or Spearman correlation              |
|  [16]   | `struct(*exprs, **named_exprs)`                                | struct builder | build struct column expression               |
|  [17]   | `format(f_string, *args)`                                      | string format  | format string expression                     |

[ENTRYPOINT_SCOPE]: Expr combinators
- rail: dataframe-agnostic

`Expr` is the composable column node returned by `col()`/`lit()`/`nth()`/`when()`; every method returns `Self` so expressions chain. The window/rolling/cumulative/ranking family and `over` are the backend-agnostic analytic surface — never drop to a native window spec.

| [INDEX] | [SURFACE]                                                                      | [ENTRY_FAMILY]  | [CAPABILITY]                                            |
| :-----: | :----------------------------------------------------------------------------- | :-------------- | :------------------------------------------------------ |
|  [01]   | `Expr.alias(name)` / `Expr.cast(dtype)`                                        | rename/type     | rename node; cast to a narwhals dtype                   |
|  [02]   | `Expr.over(*partition_by, order_by=None)`                                      | window          | window/partitioned aggregation                          |
|  [03]   | `Expr.cum_sum/cum_prod/cum_min/cum_max/cum_count(*, reverse=False)`             | cumulative      | running aggregates                                      |
|  [04]   | `Expr.rolling_sum/rolling_mean/rolling_std/rolling_var(window_size, *, min_samples=None, center=False)` | rolling | sliding-window aggregates                  |
|  [05]   | `Expr.ewm_mean(*, com=None, span=None, half_life=None, alpha=None, adjust=True, min_samples=1, ignore_nulls=False)` | rolling | exponentially-weighted mean        |
|  [06]   | `Expr.shift(n)` / `Expr.diff()` / `Expr.rank(method, *, descending)`            | sequence        | lag, first difference, rank                             |
|  [07]   | `Expr.fill_null(value, strategy, limit)` / `Expr.fill_nan(value)`              | null handling   | impute nulls / NaNs                                     |
|  [08]   | `Expr.is_null/is_nan/is_finite/is_in(other)/is_between(lower, upper, closed)`   | predicate       | boolean masks                                           |
|  [09]   | `Expr.is_first_distinct/is_last_distinct/is_duplicated/is_unique/is_close`      | predicate       | distinctness and approximate-equality masks             |
|  [10]   | `Expr.clip(lower_bound, upper_bound)` / `Expr.abs/round(decimals)/floor/ceil`   | numeric         | bound and round                                         |
|  [11]   | `Expr.sum/min/max/mean/median/std/var/quantile(q, interpolation)`               | aggregation     | scalar reductions                                       |
|  [12]   | `Expr.n_unique/count/null_count/len/first/last/mode/skew/kurtosis`              | aggregation     | distinct count, count, extremes, shape stats            |
|  [13]   | `Expr.replace_strict(old, new=None, *, default, return_dtype=None)`             | map             | exhaustive value remap with default                     |
|  [14]   | `Expr.map_batches(function, return_dtype=None, *, returns_scalar=False)`        | escape hatch    | apply a native-frame callable per backend chunk         |
|  [15]   | `Expr.pipe(function, *args, **kwargs)`                                          | compose         | thread the expr through a callable                      |
|  [16]   | `Expr.exp/log(base)/sqrt/sin/cos`                                               | math            | elementwise transcendental ops                          |
|  [17]   | `Expr.drop_nulls/unique/filter(*predicates)/any/all/any_value`                  | reshape         | within-expr row filters and reductions                  |

[ENTRYPOINT_SCOPE]: Expr / Series namespace accessors
- rail: dataframe-agnostic

The five typed namespaces (`.dt`, `.str`, `.cat`, `.list`, `.struct`) plus `.name` carry the per-dtype combinators; they are the only path to temporal/string/list/struct work and are identical on `Expr` and `Series`. Branching on backend type to do string or date work is rejected — use the namespace.

| [INDEX] | [NAMESPACE]    | [MEMBERS]                                                                                                   |
| :-----: | :------------- | :--------------------------------------------------------------------------------------------------------- |
|  [01]   | `.dt`          | `year`/`month`/`day`/`hour`/`minute`/`second`/`millisecond`/`microsecond`/`nanosecond`/`ordinal_day`/`weekday`/`date`/`to_string`/`timestamp`/`truncate`/`offset_by`/`replace_time_zone`/`convert_time_zone`/`total_seconds`/`total_milliseconds`/`total_microseconds`/`total_nanoseconds`/`total_minutes` |
|  [02]   | `.str`         | `len_chars`/`contains`/`starts_with`/`ends_with`/`slice`/`head`/`tail`/`split`/`replace`/`replace_all`/`strip_chars`/`pad_start`/`pad_end`/`zfill`/`to_lowercase`/`to_uppercase`/`to_titlecase`/`to_date`/`to_datetime`/`to_time` |
|  [03]   | `.cat`         | `get_categories`                                                                                           |
|  [04]   | `.list`        | `len`/`get(index)`/`contains(item)`/`unique`/`sort`/`min`/`max`/`sum`/`mean`/`median`                      |
|  [05]   | `.struct`      | `field(name)`                                                                                              |
|  [06]   | `.name`        | `keep`/`map(function)`/`prefix(prefix)`/`suffix(suffix)`/`to_lowercase`/`to_uppercase`                      |

[ENTRYPOINT_SCOPE]: Series eager-only operations
- rail: dataframe-agnostic

`Series` shares every `Expr` combinator and namespace but, being eager, adds materialized accessors. Use `Series` only past a `DataFrame.get_column`/`iter_columns` boundary; lazy paths stay in `Expr`.

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY] | [CAPABILITY]                                  |
| :-----: | :-------------------------------------------------------------- | :------------- | :--------------------------------------------- |
|  [01]   | `Series.from_numpy(name, values, dtype=None, *, backend)` / `Series.from_iterable(...)` | factory | build a typed series          |
|  [02]   | `Series.to_list()` / `to_numpy()` / `to_arrow()` / `to_frame()` / `to_dummies()` | export | materialize to Python/Arrow/frame |
|  [03]   | `Series.to_native()` / `to_polars()` / `to_pandas()`           | export         | unwrap or lower to a backend series           |
|  [04]   | `Series.value_counts()` / `Series.hist(bins)`                  | summary        | frequency table / histogram frame             |
|  [05]   | `Series.arg_min()` / `arg_max()` / `arg_true()`                | index reduce   | position of extreme / true values             |
|  [06]   | `Series.scatter(indices, values)` / `Series.zip_with(mask, other)` | combine    | positional set / masked combine               |
|  [07]   | `Series.item(index)` / `Series.is_sorted(*, descending)` / `Series.is_empty()` | scalar/predicate | single value, sortedness, emptiness |
|  [08]   | `Series.dtype` / `Series.name` / `Series.shape` / `Series.implementation` | metadata | dtype, name, shape, backend identity     |
|  [09]   | `Series.sort(*, descending, nulls_last)` / `Series.rename(name)` / `Series.sample(...)` | reshape | sort, rename, sample          |

[ENTRYPOINT_SCOPE]: selectors module (`narwhals.selectors`)
- rail: dataframe-agnostic

Column selectors are composable expression-like objects accepted anywhere an expression is, with `&`/`|`/`~` set algebra; they replace stringly column-name globbing. Import as `import narwhals.selectors as ncs`.

| [INDEX] | [SURFACE]                                                                 | [ENTRY_FAMILY] | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------------------ | :------------- | :---------------------------------------- |
|  [01]   | `by_dtype(*dtypes)`                                                        | selector       | columns matching narwhals dtypes          |
|  [02]   | `numeric()` / `boolean()` / `string()` / `categorical()`                  | selector       | columns by broad type class               |
|  [03]   | `datetime(time_unit=None, time_zone=('*', None))`                         | selector       | temporal columns by unit/zone             |
|  [04]   | `matches(pattern)`                                                        | selector       | columns whose name matches a regex        |
|  [05]   | `all()`                                                                   | selector       | all columns                               |

## [04]-[IMPLEMENTATION_LAW]

[DATAFRAME_AGNOSTIC_TOPOLOGY]:
- `from_native` / `narwhalify` are the sole intake points; they infer the level as full eager, lazy, or interchange-only; `pass_through=True` makes a non-narwhalifiable object flow through unchanged for graceful boundaries
- `to_native` is the sole export; it strips the narwhals wrapper and returns the backend's own type
- `DataFrame` and `LazyFrame` mirror each other's transformation API (`select`/`with_columns`/`filter`/`group_by`/`join`/`sort`/`unique`/`explode`/`gather_every`); `LazyFrame.collect(backend=)` materialises and may retarget the output backend
- the `Expr` analytic surface (`over`, `cum_*`, `rolling_*`, `ewm_mean`, `rank`, `shift`, `diff`) and the typed namespaces (`.dt`/`.str`/`.cat`/`.list`/`.struct`/`.name`) are backend-agnostic; `map_batches` is the only escape hatch to a native callable
- dtype objects are value-equal by class identity: `nw.Int32() == nw.Int32()` is `True`; `Object` carries opaque Python objects and `Unknown` is the fallback for an unmapped backend dtype
- `Implementation` enum carries `PANDAS`, `MODIN`, `CUDF`, `PYARROW`, `PYSPARK`, `PYSPARK_CONNECT`, `POLARS`, `DASK`, `DUCKDB`, `IBIS`, `SQLFRAME`, `UNKNOWN`; the pyarrow member is `PYARROW` (value `'pyarrow'`), never `ARROW`; `PYSPARK_CONNECT` is distinct from `PYSPARK`; check at feature-fork points
- `narwhals.stable.v1` is the version-pinned mirror; a durable library boundary imports from it so a narwhals upgrade never silently changes behavior

[LOCAL_ADMISSION]:
- Apply `@narwhalify` at function boundaries to accept any backend frame and return the same backend type.
- Use `Expr` composition via `col()`, `when()`, `lit()`, `nth()`, the `selectors` module, and namespace accessors inside `select` / `with_columns` / `GroupBy.agg`; never branch on specific frame type inside the function body.
- The `backend` parameter on `from_dict`, `from_arrow`, `read_csv`, `new_series`, etc. selects the output backend; omit it only when the input already carries a native backend identity.
- `scan_csv` / `scan_parquet` return `LazyFrame`; prefer them over `read_csv` / `read_parquet` for deferred execution paths and stream the result with `LazyFrame.sink_parquet`.
- Reach `maybe_*` only at a pandas-specific boundary; they no-op on non-pandas backends so they stay safe in agnostic code.

[STACK]:
- arrow-intake stack: `narwhals.from_arrow(table, backend=)` accepts any `__arrow_c_stream__` producer, so a `nanoarrow.Array`/`ArrayStream` (or a `pyarrow.Table`) flows into the agnostic frame layer with zero copy and zero Python-list hop; the dtype and null mask ride the capsule.
- numpy/mesh stack: `from_numpy(arr, schema, backend=)` and `Series.from_numpy` lift the raw NumPy buffers a `meshio.Mesh` or a point-cloud decode produces into a typed frame/column, keeping the dtype precise rather than defaulting to `Object`.
- typed-row decode stack: a frame's `iter_rows(named=True)` or `to_dict(as_series=False)` feeds a `msgspec.Struct`/Pydantic model build at the boundary, while `from_dicts(rows, schema=)` is the inverse — validated row models become a frame with the schema enforced once.
- lazy-pipeline stack: build the whole transform as one `LazyFrame` graph (`scan_parquet -> with_columns(Expr over/rolling) -> group_by.agg -> sort`) and call `collect(backend=)` once; the agnostic graph is the single rail and the backend is a leaf parameter, never an `if Implementation` branch around parallel code paths.

[RAIL_LAW]:
- Package: `narwhals`
- Owns: dataframe-agnostic abstraction over Polars, pandas, Modin, cuDF, PySpark, DuckDB, Ibis, SQLFrame, Dask, and Arrow backends; the full `Expr`/`Series` combinator surface; the typed namespaces; selectors; the version-pinned `stable.v1` mirror
- Accept: any `from_native`-compatible frame, series, or `__arrow_c_stream__` table, plus dicts/row-dicts/NumPy arrays with an explicit `backend`
- Reject: direct backend API calls inside narwhals-wrapped functions; branching on `Implementation` for non-feature-gated paths; dropping to a native window/string/date spec where the agnostic `Expr` namespace owns it; a Python-list hop where an Arrow capsule passes straight through `from_arrow`
