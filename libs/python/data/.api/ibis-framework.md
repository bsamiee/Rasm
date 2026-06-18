# [PY_DATA_API_IBIS_FRAMEWORK]

`ibis` supplies a portable SQL-dataframe API that compiles relational `Table`, `Column`, `Scalar`, and `Expr` expressions to SQL via `sqlglot` and executes them through pluggable `BaseBackend` adapters (DuckDB, SQLite, BigQuery, Snowflake, and 20+ others). The core API owns expression construction (`col`, `literal`, `memtable`, `table`), SQL compilation (`to_sql`, `decompile`, `parse_sql`), analytic functions (`rank`, `dense_rank`, `row_number`, `cume_dist`, `ntile`), window builders (`window`, `rows_window`, `range_window`), and set operations (`union`, `intersect`, `difference`). `ibis.connect` is the single connection entry point; the `Schema` type bridges to pandas, Polars, PyArrow, and NumPy schemas.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ibis-framework`
- package: `ibis-framework`
- module: `ibis`
- asset: pure Python + sqlglot
- rail: query

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: expression types
- rail: query

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY]     | [ROLE]                                       |
| :-----: | :--------- | :---------------- | :------------------------------------------- |
|   [1]   | `Table`    | relational expr   | multi-column tabular expression              |
|   [2]   | `Column`   | column expr       | column expression (named typed vector)       |
|   [3]   | `Scalar`   | scalar expr       | single-value expression                      |
|   [4]   | `Value`    | value expr        | base for `Column` and `Scalar`               |
|   [5]   | `Expr`     | base expr         | root expression type with execution protocol |
|   [6]   | `Deferred` | deferred accessor | lazy column accessor for `_[name]` patterns  |

[PUBLIC_TYPE_SCOPE]: schema and type system
- rail: query

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [ROLE]                                  |
| :-----: | :------------ | :------------ | :-------------------------------------- |
|   [1]   | `Schema`      | schema value  | ordered `{name: DataType}` mapping      |
|   [2]   | `DataType`    | type value    | ibis logical type descriptor            |
|   [3]   | `BaseBackend` | backend class | pluggable SQL execution and IO boundary |
|   [4]   | `IbisError`   | error type    | root ibis exception                     |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: connection and table intake
- rail: query

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY]   | [CAPABILITY]                        |
| :-----: | :------------------------------------------- | :--------------- | :---------------------------------- |
|   [1]   | `connect(resource, **kwargs)`                | connection       | open backend by URI or path         |
|   [2]   | `get_backend(expr)` / `set_backend(backend)` | backend registry | query or set the default backend    |
|   [3]   | `table(schema, name, catalog, database)`     | table factory    | declare an unbound table reference  |
|   [4]   | `memtable(data, columns, schema)`            | in-memory table  | register Python/Arrow data as table |
|   [5]   | `read_csv(paths, table_name, **kwargs)`      | IO intake        | read CSV into backend table         |
|   [6]   | `read_parquet(paths, table_name, **kwargs)`  | IO intake        | read Parquet into backend table     |
|   [7]   | `read_json(paths, table_name, **kwargs)`     | IO intake        | read JSON into backend table        |
|   [8]   | `read_delta(path, table_name, **kwargs)`     | IO intake        | read Delta Lake table               |

[ENTRYPOINT_SCOPE]: expression construction
- rail: query

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY]   | [CAPABILITY]                    |
| :-----: | :------------------------------------------- | :--------------- | :------------------------------ |
|   [1]   | `literal(value, type)`                       | scalar literal   | typed literal scalar expression |
|   [2]   | `null(type)`                                 | null literal     | typed null scalar expression    |
|   [3]   | `array(values)`                              | array literal    | array value expression          |
|   [4]   | `struct(value, type)`                        | struct literal   | struct value expression         |
|   [5]   | `map(keys, values)`                          | map literal      | map value expression            |
|   [6]   | `param(type)`                                | parameter        | deferred parameter expression   |
|   [7]   | `ifelse(condition, true_expr, false_expr)`   | conditional      | two-branch conditional          |
|   [8]   | `cases(branch, *branches, else_)`            | conditional      | multi-branch CASE expression    |
|   [9]   | `coalesce(arg, *args)`                       | null handling    | first non-null value            |
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
|   [1]   | `rank()` / `dense_rank()`                              | ranking        | rank and dense-rank window functions |
|   [2]   | `row_number()`                                         | ranking        | 0-based row number                   |
|   [3]   | `percent_rank()` / `cume_dist()`                       | ranking        | relative rank and cumulative dist    |
|   [4]   | `ntile(buckets)`                                       | ranking        | ntile bucket assignment              |
|   [5]   | `window(preceding, following, order_by, group_by)`     | window builder | general window frame                 |
|   [6]   | `rows_window(preceding, following, group_by, ...)`     | window builder | ROWS frame window                    |
|   [7]   | `range_window(preceding, following, group_by, ...)`    | window builder | RANGE frame window                   |
|   [8]   | `cumulative_window(group_by, order_by)`                | window builder | unbounded preceding window           |
|   [9]   | `trailing_window(preceding, group_by, order_by)`       | window builder | trailing rows window                 |
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
|   [1]   | `union(table, *rest, distinct)`                  | set operation   | UNION [ALL] across tables            |
|   [2]   | `intersect(table, *rest, distinct)`              | set operation   | INTERSECT across tables              |
|   [3]   | `difference(table, *rest, distinct)`             | set operation   | EXCEPT / MINUS across tables         |
|   [4]   | `cross_join(self, right, *rest, lname, rname)`   | join            | cartesian product join               |
|   [5]   | `join(self, right, predicates, how, ...)`        | join            | typed join with strategy             |
|   [6]   | `to_sql(expr, dialect, pretty, **kwargs)`        | SQL compilation | emit SQL string for any dialect      |
|   [7]   | `decompile(expr, render_import, assign, format)` | SQL decompile   | emit ibis Python expression from SQL |
|   [8]   | `parse_sql(sqlstring, catalog, dialect)`         | SQL parse       | parse SQL into ibis expression tree  |
|   [9]   | `aggregate(self, metrics, by, ...)`              | aggregation     | grouped aggregation                  |

[ENTRYPOINT_SCOPE]: schema and type utilities
- rail: query

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY]    | [CAPABILITY]                           |
| :-----: | :------------------------------------------------------ | :---------------- | :------------------------------------- |
|   [1]   | `schema(pairs, names, types)`                           | schema factory    | build `Schema` from pairs, names+types |
|   [2]   | `dtype(value, nullable)`                                | type factory      | parse string or value to `DataType`    |
|   [3]   | `infer_dtype(value)`                                    | type inference    | infer `DataType` from Python value     |
|   [4]   | `infer_schema(value)`                                   | schema inference  | infer `Schema` from DataFrame or dict  |
|   [5]   | `Schema.from_pandas(pandas_schema)`                     | schema conversion | build from pandas dtype map            |
|   [6]   | `Schema.from_pyarrow(pyarrow_schema)`                   | schema conversion | build from PyArrow schema              |
|   [7]   | `Schema.from_polars(polars_schema)`                     | schema conversion | build from Polars schema               |
|   [8]   | `Schema.from_numpy(numpy_schema)`                       | schema conversion | build from NumPy dtype list            |
|   [9]   | `Schema.to_pandas()` / `.to_pyarrow()` / `.to_polars()` | schema export     | export to backend schema type          |

[ENTRYPOINT_SCOPE]: BaseBackend execution interface
- rail: query

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY] | [CAPABILITY]                        |
| :-----: | :------------------------------------------------- | :------------- | :---------------------------------- |
|   [1]   | `BaseBackend.execute(expr, params, limit)`         | execution      | run expression, return pandas frame |
|   [2]   | `BaseBackend.to_pandas(expr, params, limit)`       | execution      | execute and return pandas           |
|   [3]   | `BaseBackend.to_pyarrow(expr, params, limit)`      | execution      | execute and return PyArrow table    |
|   [4]   | `BaseBackend.to_polars(expr, params, limit)`       | execution      | execute and return Polars frame     |
|   [5]   | `BaseBackend.to_parquet(expr, path, params, ...)`  | IO export      | write result to Parquet             |
|   [6]   | `BaseBackend.to_csv(expr, path, params, ...)`      | IO export      | write result to CSV                 |
|   [7]   | `BaseBackend.to_delta(expr, path, params, ...)`    | IO export      | write result to Delta Lake          |
|   [8]   | `BaseBackend.compile(expr, limit, params, ...)`    | compilation    | compile to SQL string               |
|   [9]   | `BaseBackend.table(name, database)`                | table access   | reference backend table by name     |
|  [10]   | `BaseBackend.list_tables(like, database)`          | catalog        | list available tables               |
|  [11]   | `BaseBackend.create_table(name, obj, schema, ...)` | DDL            | create table from expression        |
|  [12]   | `BaseBackend.drop_table(name, database, force)`    | DDL            | drop table                          |

## [4]-[IMPLEMENTATION_LAW]

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
