# [RASM_PERSISTENCE_API_DUCKDB]

`DuckDB.NET.Data.Full` supplies the ADO.NET DuckDB provider, bulk appenders,
data-chunk vector readers and writers, scalar and table function registration,
and schema metadata collections.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `DuckDB.NET.Data.Full`
- package: `DuckDB.NET.Data.Full`
- assembly: `DuckDB.NET.Data`
- namespace: `DuckDB.NET.Data`
- asset: runtime library
- rail: store-provider

## [2]-[PUBLIC_TYPES]

[ADO_TYPES]: ADO.NET provider surfaces
- rail: store-provider

`DuckDBQueryProgress` carries `Percentage`, `RowsProcessed`, and `TotalRowsToProcess`; `DuckDBErrorType` classifies `DuckDBException.ErrorType` values such as transaction, connection, I/O, interrupt, constraint, catalog, and fatal failures. `DuckDBConnectionStringBuilder` exposes `DataSource` plus the constants `InMemoryDataSource` (`:memory:`), `InMemoryConnectionString`, `InMemorySharedDataSource` (`:memory:?cache=shared`), and `InMemorySharedConnectionString`.

| [INDEX] | [SYMBOL]                                | [PACKAGE_ROLE]    | [CAPABILITY]              |
| :-----: | :-------------------------------------- | :---------------- | :------------------------ |
|   [1]   | `DuckDBConnection`                      | connection root   | owns native connection    |
|   [2]   | `DuckDBCommand`                         | command surface   | executes SQL              |
|   [3]   | `DuckDBDataReader`                      | reader surface    | reads result rows         |
|   [4]   | `DuckDBParameter`                       | parameter surface | binds typed values        |
|   [5]   | `DuckDBParameterCollection`             | parameter set     | collects bound values     |
|   [6]   | `DuckDBTransaction`                     | transaction scope | commits or rolls back     |
|   [7]   | `DuckDBClientFactory`                   | provider factory  | creates ADO objects       |
|   [8]   | `DuckDBConnectionStringBuilder`         | connection string | names data sources        |
|   [9]   | `DuckDBException`                       | provider error    | carries native failure    |
|  [10]   | `DuckDBSchema`                          | schema surface    | reads metadata tables     |
|  [11]   | `DuckDB.NET.Native.DuckDBQueryProgress` | progress value    | reports query progress    |
|  [12]   | `DuckDB.NET.Native.DuckDBErrorType`     | error classifier  | classifies native failure |

[APPENDER_TYPES]: bulk append surfaces
- rail: store-provider

| [INDEX] | [SYMBOL]                                        | [PACKAGE_ROLE]  | [CAPABILITY]           |
| :-----: | :---------------------------------------------- | :-------------- | :--------------------- |
|   [1]   | `DuckDB.NET.Data.DuckDBAppender`                | appender root   | streams bulk rows      |
|   [2]   | `DuckDB.NET.Data.IDuckDBAppenderRow`            | row contract    | appends typed values   |
|   [3]   | `DuckDB.NET.Data.DuckDBAppenderRow`             | row surface     | implements row appends |
|   [4]   | `DuckDB.NET.Data.DuckDBMappedAppender<T, TMap>` | mapped appender | appends mapped objects |
|   [5]   | `DuckDB.NET.Data.Mapping.DuckDBAppenderMap<T>`  | mapping owner   | maps object properties |

[FUNCTION_TYPES]: user-defined function surfaces
- rail: store-provider

`TableFunction` declares columns, data, and optional `DataFactory` (projection-push-down variant); `ScalarFunctionOptions` carries `IsPureFunction` (`bool?`) and `HandlesNulls` (`bool`); `ProjectedColumn` is a record `(int Index, string Name, Type Type)` pushed to the data factory when column projection is active; `IDuckDBValueReader` (from `DuckDB.NET.Native`) delivers typed values from table-function parameter lists.

| [INDEX] | [SYMBOL]                                   | [PACKAGE_ROLE]     | [CAPABILITY]              |
| :-----: | :----------------------------------------- | :----------------- | :------------------------ |
|   [1]   | `TableFunction`                            | function result    | declares table function   |
|   [2]   | `ColumnInfo`                               | column declaration | names column type         |
|   [3]   | `CardinalityHint`                          | planner hint       | states row cardinality    |
|   [4]   | `ScalarFunctionOptions`                    | function options   | states scalar UDF policy  |
|   [5]   | `NamedAttribute`                           | parameter name     | names function parameters |
|   [6]   | `ProjectedColumn`                          | projection cell    | carries projected column  |
|   [7]   | `DuckDB.NET.Native.IDuckDBValueReader`     | value reader       | reads typed param values  |
|   [8]   | `DuckDBConnectionScalarFunctionExtensions` | scalar extension   | extends connection UDFs   |
|   [9]   | `DuckDBConnectionTableFunctionExtensions`  | table extension    | extends connection UDFs   |

[CHUNK_TYPES]: data-chunk vector surfaces
- rail: store-provider

| [INDEX] | [SYMBOL]                  | [PACKAGE_ROLE]  | [CAPABILITY]          |
| :-----: | :------------------------ | :-------------- | :-------------------- |
|   [1]   | `IDuckDBDataReader`       | reader contract | reads vector values   |
|   [2]   | `VectorDataReaderBase`    | reader base     | projects typed reads  |
|   [3]   | `VectorDataReaderFactory` | reader factory  | creates typed readers |
|   [4]   | `IDuckDBDataWriter`       | writer contract | writes vector values  |
|   [5]   | `VectorDataWriterBase`    | writer base     | projects typed writes |
|   [6]   | `VectorDataWriterFactory` | writer factory  | creates typed writers |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: connection and command
- rail: store-provider

`NativeConnection` returns `DuckDBNativeConnection` and requires an open connection; `BeginTransaction()` returns the typed `DuckDBTransaction` directly.

| [INDEX] | [SURFACE]          | [CALL_SHAPE]     | [CAPABILITY]              |
| :-----: | :----------------- | :--------------- | :------------------------ |
|   [1]   | `Open`             | connection call  | opens native database     |
|   [2]   | `BeginTransaction` | connection call  | starts typed transaction  |
|   [3]   | `CreateCommand`    | connection call  | creates typed command     |
|   [4]   | `Duplicate`        | connection call  | clones open connection    |
|   [5]   | `GetQueryProgress` | connection call  | reads execution progress  |
|   [6]   | `GetSchema`        | connection call  | reads metadata collection |
|   [7]   | `NativeConnection` | connection prop  | exposes native handle     |
|   [8]   | `ExecuteReader`    | command call     | streams result rows       |
|   [9]   | `ExecuteNonQuery`  | command call     | runs statement            |
|  [10]   | `ExecuteScalar`    | command call     | reads single value        |
|  [11]   | `UseStreamingMode` | command property | selects streaming results |
|  [12]   | `Prepare`          | command call     | prepares statement        |

[ENTRYPOINT_SCOPE]: appender operations
- rail: store-provider

`CreateAppender` supports table, schema/table, and catalog/schema/table overloads; mapped appenders close after batch append. `IDuckDBAppenderRow.AppendValue` carries typed overloads for `bool?`, `byte[]?`, `Span<byte>`, `string?`, `decimal?`, `Guid?`, `BigInteger?`, all signed and unsigned integer widths, `float?`, `double?`, `DateTime?`, `DateTimeOffset?`, `TimeSpan?`, `DateOnly?`, `TimeOnly?`, `DuckDBDateOnly?`, `DuckDBTimeOnly?`, `IEnumerable<T>?`, and the generic `TEnum? where TEnum : Enum`.

| [INDEX] | [SURFACE]                                    | [CALL_SHAPE]    | [CAPABILITY]                |
| :-----: | :------------------------------------------- | :-------------- | :-------------------------- |
|   [1]   | `CreateAppender`                             | raw appender    | opens raw table appender    |
|   [2]   | `CreateAppender<T,TMap>`                     | mapped appender | opens mapped appender       |
|   [3]   | `DuckDBAppender.CreateRow`                   | appender call   | starts row append           |
|   [4]   | `AppendValue<T>` / `AppendNullValue`         | row call        | appends typed or null cell  |
|   [5]   | `AppendDefault`                              | row call        | appends default cell        |
|   [6]   | `IDuckDBAppenderRow.EndRow`                  | row call        | seals appended row          |
|   [7]   | `DuckDBMappedAppender<T,TMap>.AppendRecords` | mapped call     | bulk-appends mapped batch   |
|   [8]   | `DuckDBMappedAppender<T,TMap>.Close`         | mapped call     | flushes and closes appender |
|   [9]   | `DuckDBAppender.Clear`                       | appender call   | discards pending rows       |
|  [10]   | `DuckDBAppender.Close`                       | appender call   | flushes appended rows       |

[APPENDER_MAP_PROTOCOL]: `DuckDB.NET.Data.Mapping.DuckDBAppenderMap<T>` (abstract; base `System.Object`)
- rail: store-provider

| [INDEX] | [MEMBER]                                                   | [ACCESS]  | [CAPABILITY]                         |
| :-----: | :--------------------------------------------------------- | :-------- | :----------------------------------- |
|   [1]   | `protected void Map<TProperty>(Func<T, TProperty> getter)` | protected | maps one column in declaration order |
|   [2]   | `protected void DefaultValue()`                            | protected | writes the column's engine default   |
|   [3]   | `protected void NullValue()`                               | protected | writes an explicit null cell         |

[ENTRYPOINT_SCOPE]: functions and data chunks
- rail: store-provider

`RegisterScalarFunction` on the connection accepts low-level `Action<IReadOnlyList<IDuckDBDataReader>, IDuckDBDataWriter, ulong>` callbacks (with up to four generic type parameters) or high-level `Func<…, TResult>` delegates through `DuckDBConnectionScalarFunctionExtensions`. `RegisterTableFunction` low-level overloads accept `Func<IReadOnlyList<IDuckDBValueReader>, TableFunction>` result callbacks; high-level overloads through `DuckDBConnectionTableFunctionExtensions` accept a typed `Func<…, IEnumerable<TData>>` plus an `Expression<Func<TData, TProjection>>` projection selector.

| [INDEX] | [SURFACE]                | [CALL_SHAPE]         | [CAPABILITY]                 |
| :-----: | :----------------------- | :------------------- | :--------------------------- |
|   [1]   | `RegisterScalarFunction` | connection low-level | registers scalar UDF         |
|   [2]   | `RegisterScalarFunction` | scalar extension     | registers Func-based UDF     |
|   [3]   | `RegisterTableFunction`  | connection low-level | registers callback table UDF |
|   [4]   | `RegisterTableFunction`  | table extension      | registers typed table UDF    |
|   [5]   | `GetValue`               | reader call          | reads vector value           |
|   [6]   | `IsValid`                | reader call          | checks vector null mask      |
|   [7]   | `WriteValue`             | writer call          | writes vector value          |
|   [8]   | `WriteNull`              | writer call          | writes vector null           |

[LOW_LEVEL_TABLE_FUNCTION]:
- Surface: `DuckDBConnection.RegisterTableFunction`.
- Result callback: declares `TableFunction` columns, data, and cardinality; the `DataFactory` overload receives `IReadOnlyList<ProjectedColumn>` for push-down projection.
- Mapper callback: writes each row's cells through `IDuckDBDataWriter[]`.

## [4]-[IMPLEMENTATION_LAW]

[STORE_PROFILE]:
- profile: DuckDB is the analytical store provider behind the store-profile algebra
- provider root: `DuckDBConnection`
- bulk root: appenders and data-chunk writers
- function root: scalar and table UDF registration
- metadata root: `DuckDBSchema` collections

[LOCAL_ADMISSION]:
- DuckDB surfaces enter behind the same store-profile vocabulary as every provider.
- Appender and data-chunk throughput facts are profile receipts, not public service families.
- UDF registration requires explicit profile declarations with typed readers and writers.
- Native extension loading is profile policy and stays out of public Persistence surfaces.

[ARROW_BOUNDARY]:
- Arrow C Data Interface (`QueryArrow`, `ArrowResultStream`, `ArrowArrayStream`): absent from `DuckDB.NET.Data.Full` v1.5.3 managed surface. Neither `DuckDB.NET.Data` nor `DuckDB.NET.Bindings` exposes any Arrow CLR type, method, or entry point at this version. The underlying DuckDB C library exports `duckdb_query_arrow`/`duckdb_arrow_array_stream` natively; consuming them requires direct `[LibraryImport]` bindings against `duckdb`, outside the managed provider layer.

[RAIL_LAW]:
- Package: `DuckDB.NET.Data.Full`
- Owns: DuckDB ADO provider admission
- Accept: DuckDB store profile with appender and UDF declarations
- Reject: provider-branded service families
