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

[APPENDER_TYPES]: bulk append surfaces
- rail: store-provider

| [INDEX] | [SYMBOL]               | [PACKAGE_ROLE]  | [CAPABILITY]           |
| :-----: | :--------------------- | :-------------- | :--------------------- |
|   [1]   | `DuckDBAppender`       | appender root   | streams bulk rows      |
|   [2]   | `IDuckDBAppenderRow`   | row contract    | appends typed values   |
|   [3]   | `DuckDBAppenderRow`    | row surface     | implements row appends |
|   [4]   | `DuckDBMappedAppender` | mapped appender | appends mapped objects |
|   [5]   | `DuckDBAppenderMap`    | mapping owner   | maps object properties |

[FUNCTION_TYPES]: user-defined function surfaces
- rail: store-provider

| [INDEX] | [SYMBOL]                                   | [PACKAGE_ROLE]     | [CAPABILITY]              |
| :-----: | :----------------------------------------- | :----------------- | :------------------------ |
|   [1]   | `TableFunction`                            | function result    | declares columns and data |
|   [2]   | `ColumnInfo`                               | column declaration | names column type         |
|   [3]   | `CardinalityHint`                          | planner hint       | states row cardinality    |
|   [4]   | `ScalarFunctionOptions`                    | function options   | states purity and nulls   |
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

| [INDEX] | [SURFACE]         | [CALL_SHAPE]    | [CAPABILITY]          |
| :-----: | :---------------- | :-------------- | :-------------------- |
|   [1]   | `CreateAppender`  | connection call | opens table appender  |
|   [2]   | `CreateRow`       | appender call   | starts row append     |
|   [3]   | `AppendValue`     | row call        | appends typed value   |
|   [4]   | `AppendNullValue` | row call        | appends null          |
|   [5]   | `EndRow`          | row call        | seals appended row    |
|   [6]   | `Clear`           | appender call   | discards pending rows |
|   [7]   | `Close`           | appender call   | flushes appended rows |

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
