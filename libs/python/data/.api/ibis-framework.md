# [PY_DATA_API_IBIS_FRAMEWORK]

`ibis` supplies a portable SQL-dataframe API that compiles relational `Table`, `Column`, `Scalar`, and `Expr` expressions to SQL via `sqlglot` and executes them through pluggable `BaseBackend` adapters (DuckDB, SQLite, BigQuery, Snowflake, and 20+ others). The core API owns expression construction (`col`, `literal`, `memtable`, `table`), SQL compilation (`to_sql`, `decompile`, `parse_sql`), analytic functions (`rank`, `dense_rank`, `row_number`, `cume_dist`, `ntile`), window builders (`window`, `rows_window`, `range_window`), and set operations (`union`, `intersect`, `difference`). `ibis.connect` is the single connection entry point; the `Schema` type bridges to pandas, Polars, PyArrow, and NumPy schemas.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ibis-framework`
- package: `ibis-framework`
- module: `ibis`
- asset: pure Python + sqlglot
- rail: query

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

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY] | [CAPABILITY]                        |
| :-----: | :------------------------------------------------- | :------------- | :---------------------------------- |
|  [01]   | `BaseBackend.execute(expr, params, limit)`         | execution      | run expression, return pandas frame |
|  [02]   | `BaseBackend.to_pandas(expr, params, limit)`       | execution      | execute and return pandas           |
|  [03]   | `BaseBackend.to_pyarrow(expr, params, limit)`      | execution      | execute and return PyArrow table    |
|  [04]   | `BaseBackend.to_polars(expr, params, limit)`       | execution      | execute and return Polars frame     |
|  [05]   | `BaseBackend.to_parquet(expr, path, params, ...)`  | IO export      | write result to Parquet             |
|  [06]   | `BaseBackend.to_csv(expr, path, params, ...)`      | IO export      | write result to CSV                 |
|  [07]   | `BaseBackend.to_delta(expr, path, params, ...)`    | IO export      | write result to Delta Lake          |
|  [08]   | `BaseBackend.compile(expr, limit, params, ...)`    | compilation    | compile to SQL string               |
|  [09]   | `BaseBackend.table(name, database)`                | table access   | reference backend table by name     |
|  [10]   | `BaseBackend.list_tables(like, database)`          | catalog        | list available tables               |
|  [11]   | `BaseBackend.create_table(name, obj, schema, ...)` | DDL            | create table from expression        |
|  [12]   | `BaseBackend.drop_table(name, database, force)`    | DDL            | drop table                          |

## [04]-[IMPLEMENTATION_LAW]

[QUERY_TOPOLOGY]:
- namespace: `ibis`; expression types in `ibis.expr.types` (`ibis.ir`)
- `Table.execute()` / `Expr.execute()` delegate to the bound backend; the backend is set by `connect()` or `set_backend()`
- `to_sql()` compiles the expression through `sqlglot` to any supported SQL dialect without connecting
- `Deferred` (`_`) enables method-chaining without an explicit column reference: `_.col_name.func()`
- `Schema` is an `ibis` type object; use `from_pandas/pyarrow/polars` bridges at the backend boundary

[LOCAL_ADMISSION]:
- Build expressions with `table()`, `memtable()`, `literal()`, and column operations before calling `execute()` or `to_sql()`.
- Use `connect(URI)` to select the backend; the default backend is DuckDB when no connection is set.
- Keep `Schema` construction through `ibis.schema()` or bridge methods; never build from raw format strings.
- Use `to_sql(dialect=...)` for cross-dialect SQL emission; do not call `compile()` directly when portability is the goal.
- `parse_sql` round-trips SQL strings back to ibis expression trees for programmatic modification.

[RAIL_LAW]:
- Package: `ibis-framework`
- Owns: portable relational expression compiler and multi-backend execution surface
- Accept: any `BaseBackend`-compatible connection, Python/Arrow in-memory data via `memtable`
- Reject: backend-specific SQL strings inside expression pipelines, direct backend-module imports bypassing `ibis.connect`
