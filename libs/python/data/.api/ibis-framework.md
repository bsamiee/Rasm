# [PY_DATA_API_IBIS_FRAMEWORK]

`ibis` mints a portable relational expression IR: a fluent `Table`/`Column`/`Scalar` algebra builds a lazy expression tree, `sqlglot` compiles it to dialect SQL, and pluggable `BaseBackend` adapters execute it — DuckDB in-process by default. Deferred `ibis._` and `ibis.selectors` drive column-set programming, top-level constructors mint literals and multi-branch conditionals, and a result materializes to pandas/PyArrow/Polars or streams a `RecordBatchReader`. `ibis.connect` is the sole connection entry; `Schema` bridges pandas, Polars, PyArrow, and NumPy at the boundary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ibis-framework`
- package: `ibis-framework`
- module: `ibis`
- owner: `data`
- rail: query
- asset: pure Python; SQL compiled through `sqlglot`; a per-backend extra pulls the native driver

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: expression types

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY]     | [CAPABILITY]                                 |
| :-----: | :--------- | :---------------- | :------------------------------------------- |
|  [01]   | `Table`    | relational expr   | multi-column tabular expression              |
|  [02]   | `Column`   | column expr       | column expression (named typed vector)       |
|  [03]   | `Scalar`   | scalar expr       | single-value expression                      |
|  [04]   | `Value`    | value expr        | base for `Column` and `Scalar`               |
|  [05]   | `Expr`     | base expr         | root expression type with execution protocol |
|  [06]   | `Deferred` | deferred accessor | lazy column accessor for `_[name]` patterns  |

[PUBLIC_TYPE_SCOPE]: schema and type system

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [CAPABILITY]                            |
| :-----: | :------------ | :------------ | :-------------------------------------- |
|  [01]   | `Schema`      | schema value  | ordered `{name: DataType}` mapping      |
|  [02]   | `DataType`    | type value    | ibis logical type descriptor            |
|  [03]   | `BaseBackend` | backend class | pluggable SQL execution and IO boundary |
|  [04]   | `IbisError`   | error type    | root of the typed exception hierarchy   |

[DTYPE_DISCRIMINATED]: `IntegerColumn` `IntegerScalar` `StringColumn` `TimestampColumn` `ArrayValue` `StructValue` `MapValue` `JSONColumn` `DecimalColumn` `GeoSpatialColumn` — each typed expression in `ibis.expr.types` carries its own dtype-specific method namespace; the typed error hierarchy roots at `IbisError` in `ibis.common.exceptions`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: connection and table intake

Module functions on `ibis`: `connect` opens a backend, the rest admit data as a `Table`.

| [INDEX] | [SURFACE]                                    | [CAPABILITY]                        |
| :-----: | :------------------------------------------- | :---------------------------------- |
|  [01]   | `connect(resource, **kwargs)`                | open backend by URI or path         |
|  [02]   | `get_backend(expr)` / `set_backend(backend)` | query or set the default backend    |
|  [03]   | `table(schema, name, catalog, database)`     | declare an unbound table reference  |
|  [04]   | `memtable(data, columns, schema)`            | register Python/Arrow data as table |
|  [05]   | `read_csv(paths, table_name, **kwargs)`      | read CSV into backend table         |
|  [06]   | `read_parquet(paths, table_name, **kwargs)`  | read Parquet into backend table     |
|  [07]   | `read_json(paths, table_name, **kwargs)`     | read JSON into backend table        |
|  [08]   | `read_delta(path, table_name, **kwargs)`     | read Delta Lake table               |

[ENTRYPOINT_SCOPE]: expression construction

Module functions on `ibis` minting literal and conditional value expressions.

| [INDEX] | [SURFACE]                                    | [CAPABILITY]                    |
| :-----: | :------------------------------------------- | :------------------------------ |
|  [01]   | `literal(value, type)`                       | typed literal scalar expression |
|  [02]   | `null(type)`                                 | typed null scalar expression    |
|  [03]   | `array(values)`                              | array value expression          |
|  [04]   | `struct(value, type)`                        | struct value expression         |
|  [05]   | `map(keys, values)`                          | map value expression            |
|  [06]   | `param(type)`                                | deferred parameter expression   |
|  [07]   | `ifelse(condition, true_expr, false_expr)`   | two-branch conditional          |
|  [08]   | `cases(branch, *branches, else_)`            | multi-branch CASE expression    |
|  [09]   | `coalesce(arg, *args)`                       | first non-null value            |
|  [10]   | `greatest(arg, *args)` / `least(arg, *args)` | row-wise max or min             |
|  [11]   | `and_(*predicates)` / `or_(*predicates)`     | logical AND and OR combinators  |
|  [12]   | `interval(value, unit, **kwargs)`            | interval literal                |
|  [13]   | `date(value_or_year, month, day)`            | date literal                    |
|  [14]   | `time(value_or_hour, minute, second)`        | time literal                    |
|  [15]   | `timestamp(value_or_year, ...)`              | timestamp literal               |

[ENTRYPOINT_SCOPE]: fluent Table relational algebra

Every method returns a new lazy `Table`; reference columns as `t.col`, `t["col"]`, or the deferred `_.col`.

| [INDEX] | [SURFACE]                                                  | [CAPABILITY]                                           |
| :-----: | :--------------------------------------------------------- | :----------------------------------------------------- |
|  [01]   | `Table.select(*exprs, **named)`                            | choose/derive columns (accepts selectors and deferred) |
|  [02]   | `Table.mutate(*exprs, **named)`                            | add/replace columns, keeping existing ones             |
|  [03]   | `Table.filter(*predicates)`                                | row predicate (multiple args are AND-combined)         |
|  [04]   | `Table.group_by(*by).aggregate(*metrics, **named)`         | grouped reduction; `.agg` is the alias                 |
|  [05]   | `Table.order_by(*keys)`                                    | sort by columns / `asc`/`desc` keys                    |
|  [06]   | `Table.join(right, predicates, how='inner', ...)`          | inner/left/right/outer/semi/anti join                  |
|  [07]   | `Table.asof_join(right, on, predicates, ...)`              | time-series as-of join                                 |
|  [08]   | `Table.union/intersect/difference(*rest, distinct=True)`   | fluent set operations                                  |
|  [09]   | `Table.limit(n, offset=0)` / `.head(n)` / `.sample(frac)`  | row subset and probabilistic sampling                  |
|  [10]   | `Table.distinct(on=None, keep='first')`                    | distinct rows / per-key dedup                          |
|  [11]   | `Table.pivot_longer(col, ...)` / `.pivot_wider(...)`       | long/wide reshape                                      |
|  [12]   | `Table.unnest(col, ...)`                                   | explode an array column to rows                        |
|  [13]   | `Table.rename(...)` / `.drop(*cols)` / `.relocate(*cols)`  | rename/drop/reorder columns                            |
|  [14]   | `Table.fill_null(...)` / `.drop_null(...)`                 | fill or drop null rows/values                          |
|  [15]   | `Table.window_by(time_col)` / `.sql(query)`                | windowed table / raw-SQL escape on the bound backend   |
|  [16]   | `Table.cache()` / `.alias(name)` / `.view()`               | cache result / name for raw SQL / unique-named view    |
|  [17]   | `Table.schema()` / `.columns` / `.count()` / `.describe()` | schema, column names, row count, summary stats         |

[ENTRYPOINT_SCOPE]: Column / Value expression operations

`Column`/`Value` methods; numeric, string, temporal, array, struct, map, and geospatial columns each add their own namespace beyond this shared core.

| [INDEX] | [SURFACE]                                                     | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------ | :----------------------------------------- |
|  [01]   | `Column.cast(type)` / `.try_cast(type)`                       | strict / null-on-failure cast              |
|  [02]   | `Column.over(window)`                                         | evaluate analytic expr over a window frame |
|  [03]   | `Column.cases((cond, val), ..., else_=)` / `.substitute(...)` | per-column CASE / value remap              |
|  [04]   | `Column.fill_null(v)` / `.nullif(v)` / `.coalesce(*args)`     | null fill / null-if / first non-null       |
|  [05]   | `Column.isin(values)` / `.notin(values)` / `.between(lo, hi)` | set membership / range predicate           |
|  [06]   | `Column.sum/mean/min/max/std/var()`                           | standard reductions                        |
|  [07]   | `Column.count()` / `.nunique()` / `.approx_nunique()`         | exact and HLL-approximate cardinality      |
|  [08]   | `Column.quantile(q)` / `.approx_median()` / `.arbitrary()`    | quantile, approx median, any-value         |
|  [09]   | `Column.collect()` / `.group_concat(sep)`                     | gather to array / string-join group        |
|  [10]   | `Column.lag(n)` / `.lead(n)` / `.first()` / `.last()`         | offset and frame-edge analytic functions   |
|  [11]   | `Column.cumsum()` / `.cummax()` / `.cummin()` / `.cummean()`  | cumulative aggregates                      |
|  [12]   | `Column.bucket(buckets, ...)` / `.histogram(nbins)`           | discretize into buckets / histogram bins   |

[ENTRYPOINT_SCOPE]: selectors and user-defined functions

`ibis.selectors` (`import ibis.selectors as s`) resolves column sets inside `select`/`mutate`/`agg`; `@ibis.udf.*` decorators register callables as expressions.

| [INDEX] | [SURFACE]                                                  | [CAPABILITY]                                          |
| :-----: | :--------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `s.numeric()` / `s.of_type(t)` / `s.matches(regex)`        | select by dtype / regex name match                    |
|  [02]   | `s.startswith(p)` / `s.endswith(s)` / `s.contains(sub)`    | select by name affix                                  |
|  [03]   | `s.cols(*names)` / `s.all()` / `s.none()` / `s.index[...]` | explicit / all / none / positional column set         |
|  [04]   | `s.across(selector, func, names=)`                         | apply one expr template across a column set           |
|  [05]   | `s.if_any(...)` / `s.if_all(...)` / `s.where(predicate)`   | predicate-combined column selection                   |
|  [06]   | `@ibis.udf.scalar.python` / `.pyarrow` / `.pandas`         | register a row-wise scalar function (typed signature) |
|  [07]   | `@ibis.udf.scalar.builtin` / `@ibis.udf.agg.builtin`       | bind a backend-native scalar/aggregate function       |

[ENTRYPOINT_SCOPE]: analytic and window functions

Module functions on `ibis`.

| [INDEX] | [SURFACE]                                              | [CAPABILITY]                         |
| :-----: | :----------------------------------------------------- | :----------------------------------- |
|  [01]   | `rank()` / `dense_rank()`                              | rank and dense-rank window functions |
|  [02]   | `row_number()`                                         | 0-based row number                   |
|  [03]   | `percent_rank()` / `cume_dist()`                       | relative rank and cumulative dist    |
|  [04]   | `ntile(buckets)`                                       | ntile bucket assignment              |
|  [05]   | `window(preceding, following, order_by, group_by)`     | general window frame                 |
|  [06]   | `rows_window(preceding, following, group_by, ...)`     | ROWS frame window                    |
|  [07]   | `range_window(preceding, following, group_by, ...)`    | RANGE frame window                   |
|  [08]   | `cumulative_window(group_by, order_by)`                | unbounded preceding window           |
|  [09]   | `trailing_window(preceding, group_by, order_by)`       | trailing rows window                 |
|  [10]   | `trailing_range_window(preceding, order_by, group_by)` | trailing range window                |
|  [11]   | `preceding(value)` / `following(value)`                | explicit window frame boundaries     |
|  [12]   | `now()` / `today()`                                    | current timestamp and date scalars   |
|  [13]   | `random()`                                             | random floating scalar               |
|  [14]   | `uuid(value)`                                          | UUID scalar expression               |
|  [15]   | `watermark(time_col, allowed_delay)`                   | streaming watermark declaration      |
|  [16]   | `e` / `pi`                                             | mathematical constant scalars        |
|  [17]   | `asc(expr, nulls_first)` / `desc(expr, nulls_first)`   | ascending and descending sort keys   |

[ENTRYPOINT_SCOPE]: set operations and SQL compilation

Module functions on `ibis` for set algebra and SQL round-trips.

| [INDEX] | [SURFACE]                                        | [CAPABILITY]                         |
| :-----: | :----------------------------------------------- | :----------------------------------- |
|  [01]   | `union(table, *rest, distinct)`                  | UNION [ALL] across tables            |
|  [02]   | `intersect(table, *rest, distinct)`              | INTERSECT across tables              |
|  [03]   | `difference(table, *rest, distinct)`             | EXCEPT / MINUS across tables         |
|  [04]   | `cross_join(self, right, *rest, lname, rname)`   | cartesian product join               |
|  [05]   | `join(self, right, predicates, how, ...)`        | typed join with strategy             |
|  [06]   | `to_sql(expr, dialect, pretty, **kwargs)`        | emit SQL string for any dialect      |
|  [07]   | `decompile(expr, render_import, assign, format)` | emit ibis Python expression from SQL |
|  [08]   | `parse_sql(sqlstring, catalog, dialect)`         | parse SQL into ibis expression tree  |
|  [09]   | `aggregate(self, metrics, by, ...)`              | grouped aggregation                  |

[ENTRYPOINT_SCOPE]: schema and type utilities

`ibis.*` schema/type factories and `Schema` conversion methods.

| [INDEX] | [SURFACE]                                               | [CAPABILITY]                           |
| :-----: | :------------------------------------------------------ | :------------------------------------- |
|  [01]   | `schema(pairs, names, types)`                           | build `Schema` from pairs, names+types |
|  [02]   | `dtype(value, nullable)`                                | parse string or value to `DataType`    |
|  [03]   | `infer_dtype(value)`                                    | infer `DataType` from Python value     |
|  [04]   | `infer_schema(value)`                                   | infer `Schema` from DataFrame or dict  |
|  [05]   | `Schema.from_pandas(pandas_schema)`                     | build from pandas dtype map            |
|  [06]   | `Schema.from_pyarrow(pyarrow_schema)`                   | build from PyArrow schema              |
|  [07]   | `Schema.from_polars(polars_schema)`                     | build from Polars schema               |
|  [08]   | `Schema.from_numpy(numpy_schema)`                       | build from NumPy dtype list            |
|  [09]   | `Schema.to_pandas()` / `.to_pyarrow()` / `.to_polars()` | export to backend schema type          |

[ENTRYPOINT_SCOPE]: execution and export

Execution and export methods on `Expr`/`Table`, delegating to the bound backend.

| [INDEX] | [SURFACE]                                         | [CAPABILITY]                             |
| :-----: | :------------------------------------------------ | :--------------------------------------- |
|  [01]   | `Expr.execute(*, limit='default', params)`        | run expression, return pandas frame      |
|  [02]   | `Expr.to_pandas(...)` / `.to_polars(...)`         | execute and return pandas / Polars frame |
|  [03]   | `Expr.to_pyarrow(...)`                            | execute and return a PyArrow `Table`     |
|  [04]   | `Expr.to_pyarrow_batches(*, chunk_size, ...)`     | stream a PyArrow `RecordBatchReader`     |
|  [05]   | `Expr.to_torch(...)` / `.to_parquet(path, ...)`   | tensor dict / write result to Parquet    |
|  [06]   | `Expr.to_csv(path, ...)` / `.to_delta(path, ...)` | write result to CSV / Delta Lake         |

[ENTRYPOINT_SCOPE]: BaseBackend interface

`BaseBackend` methods; each `ibis.<backend>` submodule exposes `connect()`, and `ibis.connect(uri)` selects one — DuckDB the in-process default.

| [INDEX] | [SURFACE]                                                 | [CAPABILITY]                                         |
| :-----: | :-------------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `compile(expr, limit, params, ...)`                       | compile to backend SQL string                        |
|  [02]   | `sql(query, schema=None)`                                 | wrap a raw SQL string as a `Table`                   |
|  [03]   | `table(name, database)`                                   | reference backend table by name                      |
|  [04]   | `list_tables(like, database)`                             | list available tables                                |
|  [05]   | `create_table(name, obj, schema, temp, overwrite, ...)`   | create table from expression / data                  |
|  [06]   | `create_view(name, obj, ...)` / `drop_table(name, force)` | create view / drop table                             |
|  [07]   | `insert(name, obj, ...)` / `raw_sql(query)`               | append rows / execute raw SQL on the connection      |
|  [08]   | `con`                                                     | the backend's native driver connection handle        |
|  [09]   | `ibis.duckdb.connect(database=':memory:', *, ...)`        | in-memory DuckDB backend accessor (no required args) |
|  [10]   | `disconnect()`                                            | close the connection (`-> None`), releasing `con`    |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- namespace `ibis`; expression types in `ibis.expr.types` (alias `ibis.ir`), operations in `ibis.expr.operations` (alias `ibis.ops`)
- `Table`/`Column` algebra is the primary surface — each method returns a new lazy node and nothing executes until `execute`/`to_*` runs on the bound backend
- `connect()` binds the backend per-expression, `set_backend()` process-wide; `Expr.execute()` delegates to the bound one
- `to_sql()` compiles through `sqlglot` to any dialect without connecting; `decompile()` emits ibis Python from an expression and `parse_sql()` parses SQL into an expression tree
- `_` (= `ibis.deferred`) references the current table inside chained methods: `t.filter(_.x > 0).group_by(_.k).agg(total=_.v.sum())`

[STACKING]:
- `sqlglot`(`.api/sqlglot.md`): ibis compiles the expression to a `sqlglot` AST then generates dialect SQL; emit cross-dialect with `to_sql(dialect=...)` and validate/optimize the string through the sqlglot owner, never re-parsing by hand
- `duckdb`(`.api/duckdb.md`): DuckDB is the default in-process backend — `ibis.connect("duckdb://")` executes on the embedded engine and `memtable(arrow_or_polars)` registers a zero-copy frame; `BaseBackend.con` is the `DuckDBPyConnection` a `duckdb-substrait` round-trip drives off, and `disconnect()` is the `try`/`finally` release of that handle
- `polars`(`.api/polars.md`) / `pyarrow`(`.api/pyarrow.md`): `to_pyarrow()`/`to_pyarrow_batches()`/`to_polars()` hand a result to the Arrow columnar rail directly, and `Schema.from_polars`/`from_pyarrow`/`from_pandas` cross a foreign schema in with `to_*` back — the single typed boundary, never rebuilt from raw dtype strings
- within-lib: register a vectorized `@ibis.udf.scalar.pyarrow`/`.pandas` callable so the function pushes into the backend's vectorized execution, reserving `@ibis.udf.scalar.python` for backends without a vectorized path; `ibis._` and `ibis.selectors` fold column-set programming into `select`/`mutate`/`agg`

[LOCAL_ADMISSION]:
- import `ibis` at boundary scope; the branch admits it as the portable relational expression IR and multi-backend query frontend over the shared columnar rail

[RAIL_LAW]:
- Package: `ibis-framework`
- Owns: the portable relational expression IR, the fluent `Table`/`Column` algebra, deferred/selector column programming, SQL compile/decompile/parse round-trips, and the multi-backend execution and IO surface
- Accept: any `BaseBackend` connection via `ibis.connect(uri)`, Python/Arrow/Polars data via `memtable`, `ibis._`/`ibis.selectors` for column sets, `to_pyarrow`/`to_pyarrow_batches`/`to_polars` as the columnar hand-off, the typed `ibis.common.exceptions` rail
- Reject: a backend-specific SQL string inside an expression pipeline (use `to_sql(dialect=...)` or `BaseBackend.sql`), a direct backend-driver import bypassing `ibis.connect`, an `ibis.col` accessor (reference columns via `t.col`/`t["col"]`/`_.col`), a `Schema` built from raw format strings, and a re-implemented `sqlglot` compiler ibis already drives
