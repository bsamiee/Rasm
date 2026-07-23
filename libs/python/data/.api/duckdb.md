# [PY_DATA_API_DUCKDB]

`duckdb` runs an in-process analytical SQL engine over a connection-and-relation model spanning Arrow, Pandas, Polars, NumPy, CSV, JSON, and Parquet sources. `DuckDBPyConnection` owns query execution, registration, and extension load; `DuckDBPyRelation` owns lazy relational algebra, aggregation, and window functions; module-level functions proxy a default connection for one-shot use.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `duckdb`
- package: `duckdb`
- module: `duckdb`
- owner: `data`
- rail: query

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: connection, relation, expression, and value types

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :------------------------ | :------------ | :-------------------------------------------------------------- |
|  [01]   | `DuckDBPyConnection`      | class         | session handle owning execution, registration, extensions, txns |
|  [02]   | `DuckDBPyRelation`        | class         | lazy relational-algebra and window builder                      |
|  [03]   | `Expression`              | class         | column-expression node with chainable `alias`                   |
|  [04]   | `Statement`               | class         | parsed statement from `extract_statements`                      |
|  [05]   | `Value`                   | class         | typed scalar literal base                                       |
|  [06]   | `StatementType`           | enum          | parsed-statement discriminant over SQL kinds incl `MERGE_INTO`  |
|  [07]   | `ExplainType`             | enum          | `STANDARD` / `ANALYZE`                                          |
|  [08]   | `RenderMode`              | enum          | `ROWS` / `COLUMNS`                                              |
|  [09]   | `ExpectedResultType`      | enum          | `QUERY_RESULT` / `CHANGED_ROWS` / `NOTHING`                     |
|  [10]   | `PythonExceptionHandling` | enum          | UDF error policy `DEFAULT` / `RETURN_NULL`                      |
|  [11]   | `CSVLineTerminator`       | enum          | `LINE_FEED` / `CARRIAGE_RETURN_LINE_FEED`                       |
|  [12]   | `Error`                   | exception     | DB-API 2.0 base of the typed error hierarchy                    |

[DuckDBPyConnection exec]: `execute` `executemany` `sql` `query` `table` `view` `cursor` `duplicate` `close` `interrupt`
[DuckDBPyConnection fetch]: `fetchone` `fetchmany` `fetchall` `fetchnumpy` `fetchdf` `df` `fetch_df_chunk` `pl` `to_arrow_table` `to_arrow_reader` `torch` `tf`
[DuckDBPyConnection admin]: `register` `unregister` `create_function` `remove_function` `table_function` `install_extension` `load_extension` `begin` `commit` `rollback` `checkpoint`
[DuckDBPyRelation transform]: `select` `project` `filter` `order` `limit` `distinct` `aggregate` `join` `union` `except_` `intersect` `cross`
[DuckDBPyRelation reduce]: `count` `sum` `mean` `min` `max` `std` `var` `median` `quantile` `mode` `product` `histogram` `value_counts`
[DuckDBPyRelation window]: `row_number` `rank` `dense_rank` `lag` `lead` `first_value` `last_value` `n_tile` `cume_dist` `percent_rank`
[DuckDBPyRelation egress]: `to_df` `pl` `to_arrow_table` `to_arrow_reader` `fetchnumpy` `to_table` `to_view` `to_csv` `to_parquet` `insert_into` `create` `create_view`
[Expression builders]: `ColumnExpression` `ConstantExpression` `FunctionExpression` `CaseExpression` `LambdaExpression` `SQLExpression` `StarExpression` `DefaultExpression` `CoalesceOperator`
[Value subclasses]: `IntegerValue` `LongValue` `DoubleValue` `FloatValue` `BooleanValue` `StringValue` `BlobValue` `DateValue` `TimeValue`
[Value subclasses]: `TimestampValue` `IntervalValue` `DecimalValue` `UUIDValue` `ListValue` `StructValue` `MapValue` `NullValue`
[Exceptions dbapi]: `DatabaseError` `DataError` `OperationalError` `ProgrammingError` `IntegrityError` `InternalError` `NotSupportedError`
[Exceptions engine]: `CatalogException` `BinderException` `ParserException` `ConversionException` `InvalidInputException` `InvalidTypeException` `OutOfRangeException`
[Exceptions engine]: `ConstraintException` `TransactionException` `SerializationException` `InterruptException` `HTTPException` `PermissionException` `DependencyException`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: module-level functions proxying the default connection

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------------------------ | :------- | :--------------------------------------- |
|  [01]   | `connect(database, read_only, config) -> DuckDBPyConnection`                    | factory  | open an isolated database session        |
|  [02]   | `default_connection` / `set_default_connection(conn)`                           | static   | read or rebind the proxied default       |
|  [03]   | `sql(query, *, alias, connection)` / `query(query)`                             | static   | build a lazy `DuckDBPyRelation` from SQL |
|  [04]   | `execute(query, parameters)` / `executemany(query, parameters)`                 | static   | run a statement with bound parameters    |
|  [05]   | `extract_statements(query)` / `tokenize(query)`                                 | static   | parse SQL to `Statement` list or tokens  |
|  [06]   | `from_df` / `from_arrow` / `from_query` / `table` / `view` / `values`           | factory  | build a relation from a frame or table   |
|  [07]   | `read_csv` / `read_json` / `read_parquet` / `from_parquet` / `from_csv_auto`    | static   | read a file into a relation              |
|  [08]   | `register(name, obj)` / `unregister(name)`                                      | static   | bind or drop a Python object as a view   |
|  [09]   | `register_filesystem` / `list_filesystems` / `filesystem_is_registered`         | static   | manage fsspec filesystem registration    |
|  [10]   | `create_function(name, fn, *, type, exception_handling)`                        | static   | register a scalar or vectorized UDF      |
|  [11]   | `remove_function(name)` / `table_function(name, parameters)`                    | static   | drop a UDF or call a table function      |
|  [12]   | `install_extension(ext, *, repository)` / `load_extension(ext)`                 | instance | connection-side extension load boundary  |
|  [13]   | `begin` / `commit` / `rollback` / `checkpoint` / `interrupt` / `query_progress` | static   | transaction and execution control        |
|  [14]   | `enable_profiling` / `disable_profiling` / `get_profiling_information`          | instance | profiling toggle and JSON dump           |

[Logical-type constructors]: `array_type` `list_type` `map_type` `struct_type` `union_type` `row_type` `decimal_type` `enum_type` `string_type` `sqltype`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `sql`/`query` return a lazy `DuckDBPyRelation`; execution defers until an egress call (`to_df`, `fetchall`, `to_parquet`) materializes the result.
- Module-level functions proxy the default connection; `connect` is the explicit owner for isolated databases, configs, read-only mode, and concurrent cursors.
- Frames and Arrow objects register zero-copy as scannable relations through `register`/`from_df`/`from_arrow`; SQL references them by view name without materialization.
- `DuckDBPyRelation` and a raw SQL string address one engine; the relation builder composes `select`/`filter`/`aggregate`/`join` and the window methods as the canonical form.
- Prepared parameters bind positionally as `?` or by `$name` through `execute(query, parameters)`, never string interpolation.
- `create_function` registers a scalar or `type='arrow'` vectorized UDF under an explicit `parameters`/`return_type` and a `PythonExceptionHandling` policy.
- Window methods take a `window_spec` string and `projected_columns`, returning a chained `DuckDBPyRelation`.
- `get_profiling_information('json')` returns metric-dependent JSON text; a consumer decodes the string and probes each key with `.get`, never assuming presence — durations are seconds, sizes bytes, cardinalities row counts, `children` operator nodes, `extra_info` the open detail map.

[STACKING]:
- `duckdb-extensions.md`(`.api/duckdb-extensions.md`): loadable-extension roster, per-connection load shapes, Substrait plan methods, and DuckLake catalog functions; `install_extension`/`load_extension` are the connection-side load boundary this surface owns.
- `datafusion`(`.api/datafusion.md`), `polars`, `deltalake`, `pyarrow`: `from_arrow`/`register` ingest and `to_arrow_table`/`to_arrow_reader`/`pl` egress thread one shared Arrow C-data capsule, so a frame crosses the engine boundary with no copy.
- `deltalake`(`.api/deltalake.md`): `register`/`from_arrow` adopts `DeltaTable.to_pyarrow_dataset()` for pushdown SQL, and `to_parquet`/`to_arrow_reader` feeds a `write_deltalake` commit over the same Delta/Parquet files.
- `tabular` `DuckDbSession`: the request-scoped scan rail composes `register`/`from_arrow` ingest and relation egress as the columnar and query engine behind data-branch egress.
- udf/retry: an Arrow-vectorized `create_function` batch is the shared capsule; a remote-source query composes under a `stamina` `retry_context` and an OpenTelemetry span keyed by `query_progress()`.

[LOCAL_ADMISSION]:
- `connect()` owns isolated databases; `DuckDBPyRelation` is the lazy query builder; `register`/`from_arrow` scan frames zero-copy across `datafusion`/`polars`/`deltalake`.
- Bind parameters through `execute(query, parameters)`; register UDFs through `create_function`; load the Substrait bridge and other extensions through `load_extension`.

[RAIL_LAW]:
- Package: `duckdb`
- Owns: in-process analytical SQL with native `MERGE INTO`, lazy relational algebra, programmatic window functions, multi-frame and file ingest/egress, UDFs, and prepared execution
- Accept: `connect()` sessions, the `DuckDBPyRelation` builder, zero-copy Arrow/frame registration, `execute` parameter binding, Arrow-vectorized `create_function`, and `load_extension` bridges
- Reject: string-interpolated SQL parameters, eager per-row Python iteration outside relation egress, duplicate per-frame query entrypoints, a window-SQL loop where the relation methods own the axis, and hand-rolled CSV/Parquet parsing the reader functions own
