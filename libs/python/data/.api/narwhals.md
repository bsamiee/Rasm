# [PY_DATA_API_NARWHALS]

`narwhals` supplies a dataframe-agnostic translation layer that wraps Polars, pandas, Modin, cuDF, and any Arrow-compatible backend behind a single `DataFrame`, `LazyFrame`, `Series`, and `Expr` surface. Consumers call `from_native` or apply the `@narwhalify` decorator to accept any supported native frame and operate through the narwhals API; `to_native` extracts the underlying backend object at the boundary. The library owns dtype unification (`Int8`..`Float64`, `Datetime`, `Duration`, `Categorical`, `Enum`, `Struct`, `List`, etc.) and horizontal/aggregation expression combinators without materializing data itself.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `narwhals`
- package: `narwhals`
- module: `narwhals`
- asset: pure Python
- rail: dataframe-agnostic

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: frame and series types
- rail: dataframe-agnostic

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY]   | [ROLE]                                      |
| :-----: | :---------- | :-------------- | :------------------------------------------ |
|   [1]   | `DataFrame` | eager frame     | schema-validated multi-column eager frame   |
|   [2]   | `LazyFrame` | lazy frame      | deferred computation graph over any backend |
|   [3]   | `Series`    | typed column    | named typed 1-D column                      |
|   [4]   | `Expr`      | expression node | composable column expression                |
|   [5]   | `Schema`    | schema value    | ordered `{name: DType}` mapping             |
|   [6]   | `Field`     | schema field    | named typed field for `Struct` schemas      |

[PUBLIC_TYPE_SCOPE]: dtype vocabulary
- rail: dataframe-agnostic

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]  | [ROLE]                          |
| :-----: | :----------------------------- | :------------- | :------------------------------ |
|   [1]   | `Int8/16/32/64/128`            | integer dtype  | signed integer dtypes           |
|   [2]   | `UInt8/16/32/64/128`           | integer dtype  | unsigned integer dtypes         |
|   [3]   | `Float32` / `Float64`          | float dtype    | IEEE float dtypes               |
|   [4]   | `Boolean`                      | boolean dtype  | boolean dtype                   |
|   [5]   | `String`                       | string dtype   | UTF-8 string dtype              |
|   [6]   | `Binary`                       | binary dtype   | opaque bytes dtype              |
|   [7]   | `Date`                         | temporal dtype | calendar date dtype             |
|   [8]   | `Datetime(unit, tz)`           | temporal dtype | timestamp dtype with tz         |
|   [9]   | `Duration(unit)`               | temporal dtype | duration dtype                  |
|  [10]   | `Time`                         | temporal dtype | time-of-day dtype               |
|  [11]   | `Categorical`                  | category dtype | categorical (unordered) dtype   |
|  [12]   | `Enum(categories)`             | category dtype | ordered categorical dtype       |
|  [13]   | `Struct(fields)`               | nested dtype   | struct dtype from field list    |
|  [14]   | `List(inner)` / `Array(inner)` | nested dtype   | variable/fixed-size list dtypes |
|  [15]   | `Decimal(precision, scale)`    | decimal dtype  | decimal dtype                   |
|  [16]   | `Implementation`               | enum           | backend identifier vocabulary   |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: frame and series construction
- rail: dataframe-agnostic

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY] | [RAIL]                                |
| :-----: | :------------------------------------------ | :------------- | :------------------------------------ |
|   [1]   | `from_native(native, ...)`                  | intake adapter | wrap any backend frame or series      |
|   [2]   | `to_native(narwhals_object, ...)`           | export adapter | unwrap to underlying native frame     |
|   [3]   | `narwhalify(func, ...)`                     | decorator      | auto-wrap / unwrap function arguments |
|   [4]   | `from_dict(data, schema, backend, ...)`     | dict intake    | build from `{name: values}` mapping   |
|   [5]   | `from_dicts(data, schema, backend)`         | dict intake    | build from list of row dicts          |
|   [6]   | `from_arrow(native_frame, backend)`         | Arrow intake   | build from Arrow-compatible table     |
|   [7]   | `from_numpy(data, schema, backend)`         | numpy intake   | build from 2-D NumPy array            |
|   [8]   | `new_series(name, values, dtype, backend)`  | series factory | create named typed series             |
|   [9]   | `read_csv(source, backend, separator, ...)` | IO intake      | read CSV to `DataFrame`               |
|  [10]   | `read_parquet(source, backend, ...)`        | IO intake      | read Parquet to `DataFrame`           |
|  [11]   | `scan_csv(source, backend, separator, ...)` | IO intake      | scan CSV to `LazyFrame`               |
|  [12]   | `scan_parquet(source, backend, ...)`        | IO intake      | scan Parquet to `LazyFrame`           |

[ENTRYPOINT_SCOPE]: DataFrame operations
- rail: dataframe-agnostic

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY] | [RAIL]                         |
| :-----: | :--------------------------------------------------- | :------------- | :----------------------------- |
|   [1]   | `DataFrame.select(*exprs, **named_exprs)`            | projection     | select and transform columns   |
|   [2]   | `DataFrame.with_columns(*exprs, **named_exprs)`      | mutation       | add or replace columns         |
|   [3]   | `DataFrame.filter(*predicates, **constraints)`       | filter         | row filter by expression       |
|   [4]   | `DataFrame.group_by(*keys, drop_null_keys)`          | aggregation    | grouped aggregation builder    |
|   [5]   | `DataFrame.join(other, on, how, ...)`                | join           | hash join with strategy        |
|   [6]   | `DataFrame.join_asof(other, left_on, right_on, ...)` | join           | asof (sorted) join             |
|   [7]   | `DataFrame.sort(by, *more_by, descending, ...)`      | sort           | sort by one or more columns    |
|   [8]   | `DataFrame.rename(mapping)`                          | mutation       | rename columns                 |
|   [9]   | `DataFrame.drop(*columns, strict)`                   | projection     | drop named columns             |
|  [10]   | `DataFrame.drop_nulls(subset)`                       | filter         | drop rows with nulls in subset |
|  [11]   | `DataFrame.unique(subset, keep, ...)`                | dedup          | deduplicate rows               |
|  [12]   | `DataFrame.collect_schema()`                         | metadata       | retrieve `Schema`              |
|  [13]   | `DataFrame.get_column(name)`                         | column access  | extract named `Series`         |
|  [14]   | `DataFrame.lazy(backend, session)`                   | deferred       | convert to `LazyFrame`         |
|  [15]   | `DataFrame.head(n)` / `.tail(n)`                     | window         | first or last n rows           |
|  [16]   | `DataFrame.pivot(on, index, values, ...)`            | reshape        | pivot to wide format           |
|  [17]   | `DataFrame.unpivot(on, index)`                       | reshape        | melt to long format            |
|  [18]   | `DataFrame.iter_rows(named, buffer_size)`            | iteration      | row-by-row iterator            |

[ENTRYPOINT_SCOPE]: LazyFrame operations
- rail: dataframe-agnostic

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :---------------------------------------------- | :------------- | :-------------------------------- |
|   [1]   | `LazyFrame.collect(backend, ...)`               | materialise    | execute graph, return `DataFrame` |
|   [2]   | `LazyFrame.select(*exprs, **named_exprs)`       | projection     | select columns                    |
|   [3]   | `LazyFrame.with_columns(*exprs, **named)`       | mutation       | add or replace columns            |
|   [4]   | `LazyFrame.filter(*predicates, **constraints)`  | filter         | row filter                        |
|   [5]   | `LazyFrame.group_by(*keys, drop_null_keys)`     | aggregation    | grouped aggregation builder       |
|   [6]   | `LazyFrame.join(other, on, how, ...)`           | join           | hash join                         |
|   [7]   | `LazyFrame.sort(by, *more_by, descending, ...)` | sort           | sort                              |
|   [8]   | `LazyFrame.sink_parquet(file)`                  | IO export      | streaming write to Parquet        |
|   [9]   | `LazyFrame.with_row_index(name, order_by)`      | mutation       | add row index column              |
|  [10]   | `LazyFrame.top_k(k, by, reverse)`               | ranking        | top-k rows by column              |
|  [11]   | `LazyFrame.collect_schema()`                    | metadata       | retrieve `Schema` without execute |
|  [12]   | `LazyFrame.to_native()`                         | export         | unwrap to backend lazy object     |

[ENTRYPOINT_SCOPE]: expression and aggregation functions
- rail: dataframe-agnostic

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :------------------------------------------------------------- | :------------- | :------------------------------------------- |
|   [1]   | `col(*names)`                                                  | selector       | select named columns                         |
|   [2]   | `all()`                                                        | selector       | select all columns                           |
|   [3]   | `exclude(*names)`                                              | selector       | select all except named columns              |
|   [4]   | `nth(*indices)`                                                | selector       | select columns by position                   |
|   [5]   | `lit(value, dtype)`                                            | literal        | scalar literal expression                    |
|   [6]   | `len()`                                                        | aggregation    | row count expression                         |
|   [7]   | `sum/min/max/mean/median(*columns)`                            | aggregation    | column-wise aggregate expressions            |
|   [8]   | `sum_horizontal/min_horizontal/max_horizontal/mean_horizontal` | horizontal     | row-wise aggregate expressions               |
|   [9]   | `all_horizontal(*exprs, ignore_nulls)`                         | horizontal     | row-wise logical AND                         |
|  [10]   | `any_horizontal(*exprs, ignore_nulls)`                         | horizontal     | row-wise logical OR                          |
|  [11]   | `concat_str(exprs, separator, ignore_nulls)`                   | string combine | row-wise string concatenation                |
|  [12]   | `coalesce(exprs, *more_exprs)`                                 | null handling  | first non-null value per row                 |
|  [13]   | `when(*predicates)`                                            | conditional    | `when(...).then(...).otherwise(...)` builder |
|  [14]   | `concat(items, how)`                                           | frame combine  | concatenate frames vertically or diag        |
|  [15]   | `corr(a, b, method)`                                           | statistics     | Pearson or Spearman correlation              |
|  [16]   | `struct(*exprs, **named_exprs)`                                | struct builder | build struct column expression               |
|  [17]   | `format(f_string, *args)`                                      | string format  | format string expression                     |

## [4]-[IMPLEMENTATION_LAW]

[DATAFRAME_AGNOSTIC_TOPOLOGY]:
- `from_native` / `narwhalify` are the sole intake points; they infer `level` as `'full'`, `'lazy'`, or `'interchange'`
- `to_native` is the sole export; it strips the narwhals wrapper and returns the backend's own type
- `DataFrame` and `LazyFrame` mirror each other's transformation API; `LazyFrame.collect()` materialises
- dtype objects are value-equal by class identity: `nw.Int32() == nw.Int32()` is `True`
- `Implementation` enum carries `PANDAS`, `POLARS`, `MODIN`, `CUDF`, `ARROW`; check at feature-fork points

[LOCAL_ADMISSION]:
- Apply `@narwhalify` at function boundaries to accept any backend frame and return the same backend type.
- Use `Expr` composition via `col()`, `when()`, `lit()`, and namespace accessors (`.dt`, `.str`, `.cat`, `.list`, `.struct`) inside `select` and `with_columns`; never branch on specific frame type inside the function body.
- The `backend` parameter on `from_dict`, `from_arrow`, `read_csv`, etc. selects the output backend; omit it only when the input already carries a native backend identity.
- `scan_csv` / `scan_parquet` return `LazyFrame`; prefer them over `read_csv` / `read_parquet` for deferred execution paths.

[RAIL_LAW]:
- Package: `narwhals`
- Owns: dataframe-agnostic abstraction over Polars, pandas, Modin, cuDF, and Arrow backends
- Accept: any `from_native`-compatible frame, series, or Arrow table
- Reject: direct backend API calls inside narwhals-wrapped functions, branching on `Implementation` for non-feature-gated paths
