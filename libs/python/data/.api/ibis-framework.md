# [PY_DATA_API_IBIS_FRAMEWORK]

`ibis` supplies a portable SQL-dataframe API that builds a relational `Table`/`Column`/`Scalar`/`Expr` expression tree, compiles it to SQL via `sqlglot`, and executes it through pluggable `BaseBackend` adapters (DuckDB default, plus SQLite, Postgres, Polars, DataFusion, BigQuery, Snowflake, ClickHouse, and 25 backends total). The primary surface is the fluent expression algebra on `Table`/`Column` (`select`, `filter`, `mutate`, `group_by().aggregate()`, `join`, `order_by`, `over`), the deferred accessor `ibis._` and `ibis.selectors` for column-set programming, top-level constructors (`memtable`, `table`, `literal`, `cases`), analytic/window functions, set operations, and SQL round-trips (`to_sql`, `decompile`, `parse_sql`). `ibis.connect` is the single connection entry point; the `Schema` type bridges to pandas, Polars, PyArrow, and NumPy; execution materializes to pandas/PyArrow/Polars or streams via `to_pyarrow_batches`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ibis-framework`
- package: `ibis-framework`
- module: `ibis`
- version: `12.0.0`
- asset: pure Python; SQL compilation via `sqlglot`; per-backend extras (`ibis-framework[duckdb]`, `[polars]`, `[postgres]`, ...) pull the driver
- rail: query
- capability: a backend-agnostic relational expression IR with a fluent `Table`/`Column` algebra, deferred (`_`) and selector-based column programming, scalar/array/struct/map literals, multi-branch `cases`, analytic and window functions, set operations and joins (inner/left/semi/anti/asof/cross), SQL compile/decompile/parse round-trips through sqlglot, scalar/aggregate UDFs (`ibis.udf.scalar.{builtin,pyarrow,pandas,python}`, `ibis.udf.agg.builtin`), pandas/PyArrow/Polars/NumPy schema bridges, and execution to pandas/PyArrow/Polars frames, streaming `RecordBatchReader`, or file sinks across 25 backends

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: expression types
- rail: query

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY]     | [ROLE]                                       |
| :-----: | :--------- | :---------------- | :------------------------------------------- |
|  [01]   | `Table`    | relational expr   | multi-column tabular expression              |
|  [02]   | `Column`   | column expr       | column expression (named typed vector)       |
|  [03]   | `Scalar`   | scalar expr       | single-value expression                      |
|  [04]   | `Value`    | value expr        | base for `Column` and `Scalar`               |
|  [05]   | `Expr`     | base expr         | root expression type with execution protocol |
|  [06]   | `Deferred` | deferred accessor | lazy column accessor for `_[name]` patterns  |

[PUBLIC_TYPE_SCOPE]: schema and type system
- rail: query

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [ROLE]                                  |
| :-----: | :------------ | :------------ | :-------------------------------------- |
|  [01]   | `Schema`      | schema value  | ordered `{name: DataType}` mapping      |
|  [02]   | `DataType`    | type value    | ibis logical type descriptor            |
|  [03]   | `BaseBackend` | backend class | pluggable SQL execution and IO boundary |
|  [04]   | `IbisError`   | error type    | root ibis exception                     |

`ibis._` (alias `ibis.deferred`) is the `Deferred` builder used inside `select`/`mutate`/`filter`/`agg` to reference the current table's columns and chain methods without naming the table (`t.filter(_.x > 0).mutate(y=_.x * 2)`). The typed exception rail lives in `ibis.common.exceptions`: `IbisError` (root), `IbisTypeError`, `IbisInputError`/`InputTypeError`, `ExpressionError`, `RelationError`, `IntegrityError`, `UnboundExpressionError`, `OperationNotDefinedError`, `UnsupportedOperationError`/`UnsupportedArgumentError`, `TranslationError`, `BackendConversionError`, and the UDF-decorator failures (`MissingReturnAnnotationError`, `MissingParameterAnnotationError`, `AmbiguousUDFError`, `DuplicateUDFError`, `MissingUDFError`). The typed-expression hierarchy in `ibis.expr.types` discriminates by logical type and arity (`IntegerColumn`/`IntegerScalar`, `StringColumn`, `ArrayValue`, `StructValue`, `MapValue`, `TimestampColumn`, `GeoSpatialColumn`, `JSONColumn`, `DecimalColumn`, ...), so a column expression already carries its dtype-specific method namespace.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: connection and table intake
- rail: query

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY]   | [CAPABILITY]                        |
| :-----: | :------------------------------------------- | :--------------- | :---------------------------------- |
|  [01]   | `connect(resource, **kwargs)`                | connection       | open backend by URI or path         |
|  [02]   | `get_backend(expr)` / `set_backend(backend)` | backend registry | query or set the default backend    |
|  [03]   | `table(schema, name, catalog, database)`     | table factory    | declare an unbound table reference  |
|  [04]   | `memtable(data, columns, schema)`            | in-memory table  | register Python/Arrow data as table |
|  [05]   | `read_csv(paths, table_name, **kwargs)`      | IO intake        | read CSV into backend table         |
|  [06]   | `read_parquet(paths, table_name, **kwargs)`  | IO intake        | read Parquet into backend table     |
|  [07]   | `read_json(paths, table_name, **kwargs)`     | IO intake        | read JSON into backend table        |
|  [08]   | `read_delta(path, table_name, **kwargs)`     | IO intake        | read Delta Lake table               |

[ENTRYPOINT_SCOPE]: expression construction
- rail: query

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY]   | [CAPABILITY]                    |
| :-----: | :------------------------------------------- | :--------------- | :------------------------------ |
|  [01]   | `literal(value, type)`                       | scalar literal   | typed literal scalar expression |
|  [02]   | `null(type)`                                 | null literal     | typed null scalar expression    |
|  [03]   | `array(values)`                              | array literal    | array value expression          |
|  [04]   | `struct(value, type)`                        | struct literal   | struct value expression         |
|  [05]   | `map(keys, values)`                          | map literal      | map value expression            |
|  [06]   | `param(type)`                                | parameter        | deferred parameter expression   |
|  [07]   | `ifelse(condition, true_expr, false_expr)`   | conditional      | two-branch conditional          |
|  [08]   | `cases(branch, *branches, else_)`            | conditional      | multi-branch CASE expression    |
|  [09]   | `coalesce(arg, *args)`                       | null handling    | first non-null value            |
|  [10]   | `greatest(arg, *args)` / `least(arg, *args)` | comparison       | row-wise max or min             |
|  [11]   | `and_(*predicates)` / `or_(*predicates)`     | logical          | logical AND and OR combinators  |
|  [12]   | `interval(value, unit, **kwargs)`            | temporal literal | interval literal                |
|  [13]   | `date(value_or_year, month, day)`            | temporal literal | date literal                    |
|  [14]   | `time(value_or_hour, minute, second)`        | temporal literal | time literal                    |
|  [15]   | `timestamp(value_or_year, ...)`              | temporal literal | timestamp literal               |

[ENTRYPOINT_SCOPE]: fluent Table relational algebra
- rail: query

The primary surface: every method returns a new `Table` (lazy expression node), so transforms chain. Column references use `t.col`, `t["col"]`, or the deferred `_.col`.

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [CAPABILITY]                                            |
| :-----: | :--------------------------------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `Table.select(*exprs, **named)`                            | projection     | choose/derive columns (accepts selectors and deferred) |
|  [02]   | `Table.mutate(*exprs, **named)`                            | projection     | add/replace columns, keeping existing ones             |
|  [03]   | `Table.filter(*predicates)`                                | selection      | row predicate (multiple args are AND-combined)         |
|  [04]   | `Table.group_by(*by).aggregate(*metrics, **named)`         | aggregation    | grouped reduction; `.agg` is the alias                 |
|  [05]   | `Table.order_by(*keys)`                                    | ordering       | sort by columns / `asc`/`desc` keys                    |
|  [06]   | `Table.join(right, predicates, how='inner', ...)`          | join           | inner/left/right/outer/semi/anti join                  |
|  [07]   | `Table.asof_join(right, on, predicates, ...)`              | join           | time-series as-of join                                 |
|  [08]   | `Table.union/intersect/difference(*rest, distinct=True)`   | set op         | fluent set operations                                  |
|  [09]   | `Table.limit(n, offset=0)` / `.head(n)` / `.sample(frac)`  | row limiting   | row subset and probabilistic sampling                  |
|  [10]   | `Table.distinct(on=None, keep='first')`                    | dedup          | distinct rows / per-key dedup                          |
|  [11]   | `Table.pivot_longer(col, ...)` / `.pivot_wider(...)`       | reshape        | long/wide reshape                                      |
|  [12]   | `Table.unnest(col, ...)`                                   | reshape        | explode an array column to rows                        |
|  [13]   | `Table.rename(...)` / `.drop(*cols)` / `.relocate(*cols)`  | columns        | rename/drop/reorder columns                            |
|  [14]   | `Table.fill_null(...)` / `.drop_null(...)`                 | null handling  | fill or drop null rows/values                          |
|  [15]   | `Table.window_by(time_col)` / `.sql(query)`                | window/escape  | windowed table / raw-SQL escape on the bound backend   |
|  [16]   | `Table.cache()` / `.alias(name)` / `.view()`               | materialize    | cache result / name for raw SQL / unique-named view    |
|  [17]   | `Table.schema()` / `.columns` / `.count()` / `.describe()` | introspect     | schema, column names, row count, summary stats         |

[ENTRYPOINT_SCOPE]: Column / Value expression operations
- rail: query

Column methods are dtype-discriminated (`ibis.expr.types`): numeric, string, temporal, array, struct, map, and geospatial columns each carry their own namespace; the shared core is below.

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY]  | [CAPABILITY]                                  |
| :-----: | :-------------------------------------------------------------- | :-------------- | :-------------------------------------------- |
|  [01]   | `Column.cast(type)` / `.try_cast(type)`                         | type            | strict / null-on-failure cast                 |
|  [02]   | `Column.over(window)`                                           | window          | evaluate analytic expr over a window frame    |
|  [03]   | `Column.cases((cond, val), ..., else_=)` / `.substitute(...)`   | conditional     | per-column CASE / value remap                 |
|  [04]   | `Column.fill_null(v)` / `.nullif(v)` / `.coalesce(*args)`       | null handling   | null fill / null-if / first non-null          |
|  [05]   | `Column.isin(values)` / `.notin(values)` / `.between(lo, hi)`   | membership      | set membership / range predicate              |
|  [06]   | `Column.sum/mean/min/max/std/var()`                             | aggregate       | standard reductions                           |
|  [07]   | `Column.count()` / `.nunique()` / `.approx_nunique()`           | aggregate       | exact and HLL-approximate cardinality         |
|  [08]   | `Column.quantile(q)` / `.approx_median()` / `.arbitrary()`      | aggregate       | quantile, approx median, any-value            |
|  [09]   | `Column.collect()` / `.group_concat(sep)`                       | aggregate       | gather to array / string-join group           |
|  [10]   | `Column.lag(n)` / `.lead(n)` / `.first()` / `.last()`           | window          | offset and frame-edge analytic functions      |
|  [11]   | `Column.cumsum()` / `.cummax()` / `.cummin()` / `.cummean()`    | window          | cumulative aggregates                         |
|  [12]   | `Column.bucket(buckets, ...)` / `.histogram(nbins)`             | binning         | discretize into buckets / histogram bins      |

[ENTRYPOINT_SCOPE]: selectors and user-defined functions
- rail: query

`ibis.selectors` (`import ibis.selectors as s`) resolves column sets inside `select`/`mutate`/`agg`; UDFs register Python/PyArrow/pandas callables as expressions.

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [CAPABILITY]                                       |
| :-----: | :--------------------------------------------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `s.numeric()` / `s.of_type(t)` / `s.matches(regex)`        | selector       | select by dtype / regex name match                 |
|  [02]   | `s.startswith(p)` / `s.endswith(s)` / `s.contains(sub)`    | selector       | select by name affix                               |
|  [03]   | `s.cols(*names)` / `s.all()` / `s.none()` / `s.index[...]` | selector       | explicit / all / none / positional column set      |
|  [04]   | `s.across(selector, func, names=)`                         | selector       | apply one expr template across a column set        |
|  [05]   | `s.if_any(...)` / `s.if_all(...)` / `s.where(predicate)`   | selector       | predicate-combined column selection                |
|  [06]   | `@ibis.udf.scalar.python` / `.pyarrow` / `.pandas`         | scalar UDF     | register a row-wise scalar function (typed signature) |
|  [07]   | `@ibis.udf.scalar.builtin` / `@ibis.udf.agg.builtin`       | builtin UDF    | bind a backend-native scalar/aggregate function    |

[ENTRYPOINT_SCOPE]: analytic and window functions
- rail: query

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [CAPABILITY]                         |
| :-----: | :----------------------------------------------------- | :------------- | :----------------------------------- |
|  [01]   | `rank()` / `dense_rank()`                              | ranking        | rank and dense-rank window functions |
|  [02]   | `row_number()`                                         | ranking        | 0-based row number                   |
|  [03]   | `percent_rank()` / `cume_dist()`                       | ranking        | relative rank and cumulative dist    |
|  [04]   | `ntile(buckets)`                                       | ranking        | ntile bucket assignment              |
|  [05]   | `window(preceding, following, order_by, group_by)`     | window builder | general window frame                 |
|  [06]   | `rows_window(preceding, following, group_by, ...)`     | window builder | ROWS frame window                    |
|  [07]   | `range_window(preceding, following, group_by, ...)`    | window builder | RANGE frame window                   |
|  [08]   | `cumulative_window(group_by, order_by)`                | window builder | unbounded preceding window           |
|  [09]   | `trailing_window(preceding, group_by, order_by)`       | window builder | trailing rows window                 |
|  [10]   | `trailing_range_window(preceding, order_by, group_by)` | window builder | trailing range window                |
|  [11]   | `preceding(value)` / `following(value)`                | frame bound    | explicit window frame boundaries     |
|  [12]   | `now()` / `today()`                                    | temporal       | current timestamp and date scalars   |
|  [13]   | `random()`                                             | scalar         | random floating scalar               |
|  [14]   | `uuid(value)`                                          | scalar         | UUID scalar expression               |
|  [15]   | `watermark(time_col, allowed_delay)`                   | streaming      | streaming watermark declaration      |
|  [16]   | `e` / `pi`                                             | constants      | mathematical constant scalars        |
|  [17]   | `asc(expr, nulls_first)` / `desc(expr, nulls_first)`   | ordering       | ascending and descending sort keys   |

[ENTRYPOINT_SCOPE]: set operations and SQL compilation
- rail: query

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY]  | [CAPABILITY]                         |
| :-----: | :----------------------------------------------- | :-------------- | :----------------------------------- |
|  [01]   | `union(table, *rest, distinct)`                  | set operation   | UNION [ALL] across tables            |
|  [02]   | `intersect(table, *rest, distinct)`              | set operation   | INTERSECT across tables              |
|  [03]   | `difference(table, *rest, distinct)`             | set operation   | EXCEPT / MINUS across tables         |
|  [04]   | `cross_join(self, right, *rest, lname, rname)`   | join            | cartesian product join               |
|  [05]   | `join(self, right, predicates, how, ...)`        | join            | typed join with strategy             |
|  [06]   | `to_sql(expr, dialect, pretty, **kwargs)`        | SQL compilation | emit SQL string for any dialect      |
|  [07]   | `decompile(expr, render_import, assign, format)` | SQL decompile   | emit ibis Python expression from SQL |
|  [08]   | `parse_sql(sqlstring, catalog, dialect)`         | SQL parse       | parse SQL into ibis expression tree  |
|  [09]   | `aggregate(self, metrics, by, ...)`              | aggregation     | grouped aggregation                  |

[ENTRYPOINT_SCOPE]: schema and type utilities
- rail: query

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY]    | [CAPABILITY]                           |
| :-----: | :------------------------------------------------------ | :---------------- | :------------------------------------- |
|  [01]   | `schema(pairs, names, types)`                           | schema factory    | build `Schema` from pairs, names+types |
|  [02]   | `dtype(value, nullable)`                                | type factory      | parse string or value to `DataType`    |
|  [03]   | `infer_dtype(value)`                                    | type inference    | infer `DataType` from Python value     |
|  [04]   | `infer_schema(value)`                                   | schema inference  | infer `Schema` from DataFrame or dict  |
|  [05]   | `Schema.from_pandas(pandas_schema)`                     | schema conversion | build from pandas dtype map            |
|  [06]   | `Schema.from_pyarrow(pyarrow_schema)`                   | schema conversion | build from PyArrow schema              |
|  [07]   | `Schema.from_polars(polars_schema)`                     | schema conversion | build from Polars schema               |
|  [08]   | `Schema.from_numpy(numpy_schema)`                       | schema conversion | build from NumPy dtype list            |
|  [09]   | `Schema.to_pandas()` / `.to_pyarrow()` / `.to_polars()` | schema export     | export to backend schema type          |

[ENTRYPOINT_SCOPE]: BaseBackend execution interface
- rail: query

Execution methods exist both on `Expr`/`Table` (delegating to the bound backend) and on `BaseBackend`. The 25 backends (`ibis.duckdb` default, `sqlite`, `postgres`, `mysql`, `mssql`, `oracle`, `polars`, `datafusion`, `clickhouse`, `snowflake`, `bigquery`, `databricks`, `trino`, `pyspark`, `flink`, `risingwave`, `impala`, `druid`, `exasol`, `athena`, `singlestoredb`) are submodules; `ibis.connect(uri)` selects one.

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [CAPABILITY]                              |
| :-----: | :----------------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `Expr.execute(*, limit='default', params)`             | execution      | run expression, return pandas frame       |
|  [02]   | `Expr.to_pandas(...)` / `.to_polars(...)`              | execution      | execute and return pandas / Polars frame  |
|  [03]   | `Expr.to_pyarrow(...)`                                 | execution      | execute and return a PyArrow `Table`      |
|  [04]   | `Expr.to_pyarrow_batches(*, chunk_size, ...)`          | streaming      | stream a PyArrow `RecordBatchReader`      |
|  [05]   | `Expr.to_torch(...)` / `.to_parquet(path, ...)`        | execution/IO   | tensor dict / write result to Parquet     |
|  [06]   | `Expr.to_csv(path, ...)` / `.to_delta(path, ...)`      | IO export      | write result to CSV / Delta Lake          |
|  [07]   | `BaseBackend.compile(expr, limit, params, ...)`        | compilation    | compile to backend SQL string             |
|  [08]   | `BaseBackend.sql(query, schema=None)`                  | SQL escape     | wrap a raw SQL string as a `Table`        |
|  [09]   | `BaseBackend.table(name, database)`                    | table access   | reference backend table by name           |
|  [10]   | `BaseBackend.list_tables(like, database)`              | catalog        | list available tables                     |
|  [11]   | `BaseBackend.create_table(name, obj, schema, temp, overwrite, ...)` | DDL | create table from expression / data    |
|  [12]   | `BaseBackend.create_view(name, obj, ...)` / `.drop_table(name, force)` | DDL | create view / drop table             |
|  [13]   | `BaseBackend.insert(name, obj, ...)` / `.raw_sql(query)` | DML/escape   | append rows / execute raw SQL on the connection |
|  [14]   | `BaseBackend.con`                                      | native handle  | the backend's native driver connection (the DuckDB backend's `con` IS a `DuckDBPyConnection`, the handle a `duckdb-substrait` round-trip drives `install_extension`/`get_substrait` off) |
|  [15]   | `ibis.duckdb.connect(database=':memory:', *, ...)`     | default backend | the no-required-arg DuckDB backend accessor (`ibis.duckdb.connect()` opens an in-memory DuckDB backend); each `ibis.<backend>` submodule exposes its own `connect()` |
|  [16]   | `BaseBackend.disconnect()`                             | connection     | close the connection to the backend (`-> None`); the release counterpart to `connect`/`duckdb.connect`, the handle a `try`/`finally` resource bracket releases on every exit, also closing the native `BaseBackend.con` |

## [04]-[IMPLEMENTATION_LAW]

[QUERY_TOPOLOGY]:
- namespace: `ibis`; expression types in `ibis.expr.types` (alias `ibis.ir`), operations in `ibis.expr.operations` (alias `ibis.ops`)
- the fluent `Table`/`Column` algebra is the primary surface; each method returns a new lazy expression node and nothing executes until `execute`/`to_*` is called on the bound backend
- the backend is set by `connect()` (per-expression) or `set_backend()` (process default); `Expr.execute()` delegates to it
- `to_sql()` compiles the expression through `sqlglot` to any supported SQL dialect without connecting; `decompile()` emits ibis Python from an expression and `parse_sql()` parses a SQL string into an expression tree
- the deferred `_` (= `ibis.deferred`) references the current table inside chained methods: `t.filter(_.x > 0).group_by(_.k).agg(total=_.v.sum())`
- `Schema` is an `ibis` type object; use `from_pandas`/`from_pyarrow`/`from_polars`/`from_numpy` bridges at the backend boundary

[INTEGRATION_LAW]:
- sqlglot seam: ibis compiles to a `sqlglot` AST, then generates dialect SQL; the sqlglot owner (`QUERY_IR_AND_SQLGATE`) and ibis share the same compiler, so emit cross-dialect SQL with `to_sql(dialect=...)` and validate/optimize the resulting string through the sqlglot owner rather than re-parsing by hand.
- duckdb seam: the default in-process backend is DuckDB; `ibis.connect("duckdb://")` (or the bare default) executes against the embedded engine, and `memtable(arrow_table_or_polars_df)` registers a zero-copy `pyarrow`/`polars` frame as a queryable table with no copy.
- columnar hand-off: `to_pyarrow()` returns a `pyarrow.Table` and `to_pyarrow_batches()` a streaming `RecordBatchReader`, so an ibis result feeds `polars`/`datafusion`/the Arrow columnar rail directly; `to_polars()` returns a `polars.DataFrame`. Read results in Arrow/Polars, not pandas, when staying in the columnar rail.
- schema bridge: a `polars`/`pyarrow`/`pandas` schema crosses into ibis via `Schema.from_polars`/`from_pyarrow`/`from_pandas` and back via `to_*`; this is the single typed boundary — never reconstruct an ibis schema from raw dtype strings.
- UDF seam: register a `pyarrow`-compute or pandas callable as `@ibis.udf.scalar.pyarrow`/`.pandas` so the function pushes into the backend's vectorized execution; reserve `@ibis.udf.scalar.python` (row-wise) for backends without a vectorized path.

[RAIL_LAW]:
- Package: `ibis-framework`
- Owns: the portable relational expression IR, the fluent `Table`/`Column` algebra, deferred/selector column programming, SQL compile/decompile/parse round-trips, and the multi-backend execution and IO surface
- Accept: any `BaseBackend`-compatible connection via `ibis.connect(uri)`, Python/Arrow/Polars in-memory data via `memtable`, the deferred `_` and `ibis.selectors` for column sets, `to_pyarrow`/`to_pyarrow_batches`/`to_polars` as the columnar hand-off, the typed `ibis.common.exceptions` rail
- Reject: backend-specific SQL strings inside expression pipelines (use `to_sql(dialect=...)` or `BaseBackend.sql`), direct backend-driver imports bypassing `ibis.connect`, a `ibis.col` accessor that does not exist (reference columns via `t.col`/`t["col"]`/`_.col`), `Schema` built from raw format strings, and re-implementing the sqlglot compiler ibis already drives
