# [PY_DATA_API_DUCKDB]

`duckdb` supplies an in-process analytical SQL engine with a connection-and-relation model over Arrow, Pandas, Polars, NumPy, CSV, JSON, and Parquet sources. `DuckDBPyConnection` owns query execution and registration; `DuckDBPyRelation` owns lazy relational algebra and aggregation; module-level functions proxy a default connection for one-shot use.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `duckdb`
- package: `duckdb`
- import: `import duckdb`
- owner: `data`
- rail: query
- capability: in-process SQL execution, lazy relational algebra, multi-frame ingest and egress (Arrow, Pandas, Polars, NumPy, Torch), CSV/JSON/Parquet readers and writers, user-defined functions, extensions, and prepared statements

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- `duckdb.DuckDBPyConnection` — connection handle owning a database session; `execute`, `executemany`, `sql`, `query`, `table`, `view`, `register`, `unregister`, `from_df`, `from_arrow`, `from_parquet`, `from_csv_auto`, `create_function`, `install_extension`, `load_extension`, `begin`, `commit`, `rollback`, `interrupt`, `cursor`, `close`; fetch family `fetchone`/`fetchmany`/`fetchall`/`fetchnumpy`/`fetchdf`/`fetch_arrow_table`/`fetch_record_batch`.
- `duckdb.DuckDBPyRelation` — lazy relation; transforms `select`/`project`/`filter`/`order`/`limit`/`distinct`/`aggregate`/`join`/`union`/`except_`/`intersect`/`cross`; aggregates `count`/`sum`/`mean`/`min`/`max`/`std`/`var`/`median`/`quantile`/`mode`/`product`/`histogram`/`value_counts`; window functions `row_number`/`rank`/`dense_rank`/`lag`/`lead`/`first_value`/`last_value`/`n_tile`/`cume_dist`/`percent_rank`; egress `to_df`/`to_arrow_table`/`pl`/`fetchnumpy`/`to_table`/`to_view`/`to_csv`/`to_parquet`/`insert_into`.
- `duckdb.Expression` — column expression node; built by `ColumnExpression`, `ConstantExpression`, `FunctionExpression`, `CaseExpression`, `LambdaExpression`, `SQLExpression`, `StarExpression`, `DefaultExpression`, `CoalesceOperator`.
- `duckdb.Statement` — parsed statement returned by `extract_statements(sql)`; carries `StatementType` and `ExpectedResultType`.
- `duckdb.Value` — typed scalar literal; subclasses `IntegerValue`, `LongValue`, `DoubleValue`, `FloatValue`, `BooleanValue`, `StringValue`, `BlobValue`, `DateValue`, `TimeValue`, `TimestampValue`, `IntervalValue`, `DecimalValue`, `UUIDValue`, `ListValue`, `StructValue`, `MapValue`, `NullValue`, and unsigned/huge-integer variants.

[ENUMS]:
- `duckdb.ExplainType` — `STANDARD`, `ANALYZE`.
- `duckdb.RenderMode` — `ROWS`, `COLUMNS`.
- `duckdb.StatementType` — `SELECT`, `INSERT`, `UPDATE`, `DELETE`, `CREATE`, `DROP`, `ALTER`, `COPY`, `EXPORT`, `PREPARE`, `EXECUTE`, `EXPLAIN`, `PRAGMA`, `SET`, `TRANSACTION`, `ATTACH`, `DETACH`, `CALL`, `LOAD`, `MERGE_INTO`, `RELATION`, `INVALID`, and other DDL/utility kinds.
- `duckdb.ExpectedResultType` — `QUERY_RESULT`, `CHANGED_ROWS`, `NOTHING`.
- `duckdb.PythonExceptionHandling` — `DEFAULT`, `RETURN_NULL`.
- `duckdb.CSVLineTerminator` — `LINE_FEED`, `CARRIAGE_RETURN_LINE_FEED`.

[ENTRYPOINTS]:
- connection construction: `connect(database=':memory:', read_only=False, config=None) -> DuckDBPyConnection`, `default_connection() -> DuckDBPyConnection`, `set_default_connection(conn)`, `DuckDBPyConnection.cursor() -> DuckDBPyConnection`, `DuckDBPyConnection.duplicate() -> DuckDBPyConnection`.
- query execution: `sql(query, *, alias='', connection=None) -> DuckDBPyRelation`, `query(query) -> DuckDBPyRelation`, `execute(query, parameters=None) -> DuckDBPyConnection`, `executemany(query, parameters=None)`, `extract_statements(query) -> list[Statement]`, `tokenize(query)`.
- relational sources: `from_df(df) -> DuckDBPyRelation`, `from_arrow(arrow_object) -> DuckDBPyRelation`, `from_parquet(file_glob, ...) -> DuckDBPyRelation`, `from_csv_auto(path, ...) -> DuckDBPyRelation`, `from_query(query, ...) -> DuckDBPyRelation`, `table(table_name) -> DuckDBPyRelation`, `view(view_name) -> DuckDBPyRelation`, `values(values) -> DuckDBPyRelation`.
- file readers: `read_csv(path, ...) -> DuckDBPyRelation`, `read_json(path, ...) -> DuckDBPyRelation`, `read_parquet(file_glob, ...) -> DuckDBPyRelation`.
- registration: `register(view_name, python_object) -> DuckDBPyConnection`, `unregister(view_name)`, `register_filesystem(filesystem)`, `unregister_filesystem(name)`, `list_filesystems()`, `filesystem_is_registered(name)`.
- result egress: `fetchall()`, `fetchone()`, `fetchmany(size)`, `fetchnumpy()`, `df()`/`fetchdf()`, `pl()`, `arrow()`/`fetch_arrow_table()`, `fetch_record_batch(rows_per_batch)`, `torch()`, `tf()`, `to_arrow_reader()`.
- relation egress: `DuckDBPyRelation.to_df()`, `.pl()`, `.to_arrow_table()`, `.fetchnumpy()`, `.to_table(table_name)`, `.to_view(view_name)`, `.to_csv(file_name, ...)`, `.to_parquet(file_name, ...)`, `.insert_into(table_name)`, `.create(table_name)`, `.create_view(view_name)`.
- functions and extensions: `create_function(name, function, parameters=None, return_type=None, *, type=..., null_handling=..., exception_handling=..., side_effects=False)`, `remove_function(name)`, `table_function(name, parameters=None) -> DuckDBPyRelation`, `install_extension(extension, *, force_install=False, repository=None)`, `load_extension(extension)`.
- type construction: `array_type(type, size)`, `list_type(type)`, `map_type(key, value)`, `struct_type(fields)`, `row_type(fields)`, `union_type(members)`, `decimal_type(width, scale)`, `enum_type(name, type, values)`, `string_type(collation='')`, `sqltype(type_str)`, `dtype(type_str)`, `type(type_str)`.
- transactions and profiling: `begin()`, `commit()`, `rollback()`, `checkpoint()`, `interrupt()`, `query_progress()`, `enable_profiling()`, `disable_profiling()`, `get_profiling_information()`.

[EXCEPTIONS]:
- `duckdb.Error` — base exception; `DatabaseError`, `DataError`, `OperationalError`, `ProgrammingError`, `IntegrityError`, `InternalError`, `NotSupportedError` follow the DB-API 2.0 hierarchy.
- engine-specific: `CatalogException`, `BinderException`, `ParserException`, `SyntaxException`, `ConversionException`, `InvalidInputException`, `InvalidTypeException`, `TypeMismatchException`, `OutOfRangeException`, `OutOfMemoryException`, `ConstraintException`, `TransactionException`, `SerializationException`, `InterruptException`, `FatalException`, `IOException`, `HTTPException`, `PermissionException`, `DependencyException`, `SequenceException`, `NotImplementedException`.

[IMPLEMENTATION_LAW]:
- `sql`/`query` return a lazy `DuckDBPyRelation`; no execution happens until an egress call (`to_df`, `to_arrow_table`, `fetchall`, `to_parquet`, `count`) materialises the result.
- Module-level functions proxy the default connection; `connect(...)` is the explicit owner for isolated databases, configs, read-only mode, and concurrent cursors via `cursor()`.
- Frame and Arrow objects are zero-copy registered as scannable relations through `register`, `from_df`, or `from_arrow`; SQL references them by the registered view name without materialisation.
- Relational algebra composes on `DuckDBPyRelation` (`select`/`filter`/`aggregate`/`join`) as the canonical builder; raw SQL strings and the relation builder address the same engine.
- Prepared parameters pass positionally as `?` or by `$name`; `execute(query, parameters)` binds them, never string interpolation.
- User-defined functions register through `create_function` with explicit `parameters`/`return_type` and a `PythonExceptionHandling` policy; vectorised UDFs use Arrow `type='arrow'`.

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `duckdb`
- Owns: in-process analytical SQL, lazy relational algebra, multi-frame and file ingest/egress, UDFs, extensions, and prepared execution
- Accept: `connect()` for isolated databases, `DuckDBPyRelation` as the lazy query builder, `register`/`from_arrow` for zero-copy frame scanning, parameter binding via `execute(query, parameters)`, `create_function` for UDFs
- Reject: string-interpolated SQL parameters, eager per-row Python iteration outside relation egress, duplicate per-frame query entry points, and hand-rolled CSV/Parquet parsing when the reader functions own the format
