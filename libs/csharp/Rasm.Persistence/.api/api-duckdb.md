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

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE]    | [CAPABILITY]           |
| :-----: | :------------------------------ | :---------------- | :--------------------- |
|   [1]   | `DuckDBConnection`              | connection root   | owns native connection |
|   [2]   | `DuckDBCommand`                 | command surface   | executes SQL           |
|   [3]   | `DuckDBDataReader`              | reader surface    | reads result rows      |
|   [4]   | `DuckDBParameter`               | parameter surface | binds typed values     |
|   [5]   | `DuckDBParameterCollection`     | parameter set     | collects bound values  |
|   [6]   | `DuckDBTransaction`             | transaction scope | commits or rolls back  |
|   [7]   | `DuckDBClientFactory`           | provider factory  | creates ADO objects    |
|   [8]   | `DuckDBConnectionStringBuilder` | connection string | names data sources     |
|   [9]   | `DuckDBException`               | provider error    | carries native failure |
|  [10]   | `DuckDBSchema`                  | schema surface    | reads metadata tables  |
|  [11]   | `DuckDbMetaDataCollectionNames` | collection names  | names metadata sets    |
|  [12]   | `DuckDB.NET.Native.DuckDBQueryProgress` | progress value | carries `double Percentage`, `ulong RowsProcessed`, `ulong TotalRowsToProcess` |
|  [13]   | `DuckDB.NET.Native.DuckDBErrorType`     | error classifier | enum discriminating `DuckDBException.ErrorType`; values include `Transaction`, `Connection`, `Io`, `Interrupt`, `Constraint`, `Catalog`, `Fatal` |

[APPENDER_TYPES]: bulk append surfaces
- rail: store-provider

| [INDEX] | [SYMBOL]                                                              | [PACKAGE_ROLE]  | [CAPABILITY]           |
| :-----: | :------------------------------------------------------------------- | :-------------- | :--------------------- |
|   [1]   | `DuckDB.NET.Data.DuckDBAppender`                                     | appender root   | streams bulk rows      |
|   [2]   | `DuckDB.NET.Data.IDuckDBAppenderRow`                                 | row contract    | appends typed values   |
|   [3]   | `DuckDB.NET.Data.DuckDBAppenderRow`                                  | row surface     | implements row appends |
|   [4]   | `DuckDB.NET.Data.DuckDBMappedAppender<T, TMap>`                      | mapped appender | appends mapped objects |
|   [5]   | `DuckDB.NET.Data.Mapping.DuckDBAppenderMap<T>`                       | mapping owner   | maps object properties |

[FUNCTION_TYPES]: user-defined function surfaces
- rail: store-provider

| [INDEX] | [SYMBOL]                                   | [PACKAGE_ROLE]     | [CAPABILITY]              |
| :-----: | :----------------------------------------- | :----------------- | :------------------------ |
|   [1]   | `TableFunction(IReadOnlyList<ColumnInfo> columns, IEnumerable data, CardinalityHint cardinality)` | function result | declares columns, data, and cardinality |
|   [2]   | `ColumnInfo(string Name, Type Type)`       | column declaration | names column type         |
|   [3]   | `CardinalityHint(ulong Value, bool IsExact)` | planner hint     | states row cardinality    |
|   [4]   | `ScalarFunctionOptions` (`bool? IsPureFunction`, `bool HandlesNulls`) | function options | states purity and null handling |
|   [5]   | `NamedAttribute`                           | parameter name     | names function parameters |
|   [6]   | `DuckDBConnectionScalarFunctionExtensions` | scalar extension   | extends connection UDFs   |
|   [7]   | `DuckDBConnectionTableFunctionExtensions`  | table extension    | extends connection UDFs   |

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

| [INDEX] | [SURFACE]          | [CALL_SHAPE]     | [CAPABILITY]              |
| :-----: | :----------------- | :--------------- | :------------------------ |
|   [1]   | `Open`             | connection call  | opens native database     |
|   [2]   | `BeginTransaction` | connection call  | starts transaction        |
|   [3]   | `CreateCommand`    | connection call  | creates typed command     |
|   [4]   | `Duplicate`        | connection call  | clones open connection    |
|   [5]   | `GetQueryProgress` | connection call  | reads execution progress  |
|   [6]   | `GetSchema`        | connection call  | reads metadata collection |
|   [7]   | `ExecuteReader`    | command call     | streams result rows       |
|   [8]   | `ExecuteNonQuery`  | command call     | runs statement            |
|   [9]   | `ExecuteScalar`    | command call     | reads single value        |
|  [10]   | `UseStreamingMode` | command property | selects streaming results |
|  [11]   | `Prepare`          | command call     | prepares statement        |

[ENTRYPOINT_SCOPE]: appender operations
- rail: store-provider

| [INDEX] | [SURFACE]                                                                                      | [CALL_SHAPE]    | [CAPABILITY]              |
| :-----: | :--------------------------------------------------------------------------------------------- | :-------------- | :----------------------- |
|   [1]   | `DuckDBConnection.CreateAppender(string table)` (+ `(schema, table)` / `(catalog, schema, table)`) | connection call | opens raw table appender |
|   [2]   | `DuckDBConnection.CreateAppender<T, TMap>(string table)` (+ schema / catalog overloads)        | connection call | opens mapped appender     |
|   [3]   | `DuckDBAppender.CreateRow() : IDuckDBAppenderRow`                                              | appender call   | starts row append        |
|   [4]   | `IDuckDBAppenderRow.AppendValue<T>(...)` / `AppendNullValue()` / `AppendDefault()` (chainable) | row call        | appends typed/null/default cell |
|   [5]   | `IDuckDBAppenderRow.EndRow()`                                                                  | row call        | seals appended row       |
|   [6]   | `DuckDBMappedAppender<T, TMap>.AppendRecords(IEnumerable<T> records)` then `Close()`           | mapped call     | bulk-appends mapped batch |
|   [7]   | `DuckDBAppender.Clear()`                                                                       | appender call   | discards pending rows    |
|   [8]   | `DuckDBAppender.Close()`                                                                       | appender call   | flushes appended rows    |

[APPENDER_MAP_PROTOCOL]: `DuckDB.NET.Data.Mapping.DuckDBAppenderMap<T>` (base `System.Object`)
- rail: store-provider

| [INDEX] | [MEMBER]                                | [ACCESS]  | [CAPABILITY]                            |
| :-----: | :-------------------------------------- | :-------- | :-------------------------------------- |
|   [1]   | `protected DuckDBAppenderMap()`         | protected | base ctor; subclass declares column map |
|   [2]   | `protected void Map<TProperty>(Func<T, TProperty> selector)` | protected | maps one column in declaration order    |
|   [3]   | `protected void DefaultValue()`         | protected | writes the column's engine default      |
|   [4]   | `protected void NullValue()`            | protected | writes an explicit null cell            |

[ENTRYPOINT_SCOPE]: functions and data chunks
- rail: store-provider

| [INDEX] | [SURFACE]                | [CALL_SHAPE]    | [CAPABILITY]            |
| :-----: | :----------------------- | :-------------- | :---------------------- |
|   [1]   | `RegisterScalarFunction` | connection call | registers scalar UDF    |
|   [2]   | `RegisterTableFunction`  | connection call | registers table UDF     |
|   [3]   | `GetValue`               | reader call     | reads vector value      |
|   [4]   | `IsValid`                | reader call     | checks vector null mask |
|   [5]   | `WriteValue`             | writer call     | writes vector value     |
|   [6]   | `WriteNull`              | writer call     | writes vector null      |

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

[RAIL_LAW]:
- Package: `DuckDB.NET.Data.Full`
- Owns: DuckDB ADO provider admission
- Accept: DuckDB store profile with appender and UDF declarations
- Reject: provider-branded service families
