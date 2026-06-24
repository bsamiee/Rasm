# [RASM_PERSISTENCE_API_DUCKDB]

`DuckDB.NET.Data.Full` supplies the ADO.NET DuckDB provider, bulk appenders,
data-chunk vector readers and writers, scalar and table function registration,
and schema metadata collections. It is the analytical store provider behind the
`Store/profiles` store-profile algebra; its appender and data-chunk throughput
are profile receipts, and the snapshot codecs (`api-thinktecture-serialization`,
`api-messagepack`) project `[ValueObject]`/`[SmartEnum]` owners into the columns
this provider writes through the typed `AppendValue`/`WriteValue` rails.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `DuckDB.NET.Data.Full`
- package: `DuckDB.NET.Data.Full`
- version: `1.5.3`
- assembly: `DuckDB.NET.Data` (managed ADO surface; namespaces `DuckDB.NET.Data`, `DuckDB.NET.Data.Mapping`, `DuckDB.NET.Data.DataChunk.Reader`, `DuckDB.NET.Data.DataChunk.Writer`)
- companion: `DuckDB.NET.Bindings.Full` (`1.5.3`; transitive — assembly `DuckDB.NET.Bindings`, namespace `DuckDB.NET.Native`, carries `DuckDBQueryProgress`/`DuckDBErrorType`/`IDuckDBValueReader`/`DuckDBNativeConnection`/`DuckDBType`/`DuckDBDateOnly`/`DuckDBTimeOnly` and the native `duckdb` library)
- native runtime: `DuckDB.NET.Bindings.Full/runtimes/<rid>/native` ships `osx`, `linux-x64`, `linux-arm64`, `win-x64`, `win-arm64` (no per-RID managed split; the native `duckdb` shared library is RID-resolved at load)
- target framework: `net10.0` asset binds on the `net10.0` workspace floor (package also ships `net8.0`)
- xml docs: absent (no `.xml` ships; member intent is decompile-sourced, not doc-comment-sourced)
- asset: runtime library
- rail: store-provider

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: ADO.NET provider surfaces
- rail: store-provider

`DuckDBQueryProgress` is a `DuckDB.NET.Native` struct carrying `double Percentage`, `ulong RowsProcessed`, and `ulong TotalRowsToProcess` (the `GetQueryProgress()` receipt); `DuckDBErrorType` is a `DuckDB.NET.Native` enum classifying `DuckDBException.ErrorType` over the full native fault vocabulary (`Invalid`, `OutOfRange`, `Conversion`, `MismatchType`, `DivideByZero`, `InvalidType`, `Serialization`, `Transaction`, `NotImplemented`, `Expression`, `Catalog`, `Parser`, plus constraint/connection/IO/interrupt/fatal members) — the boundary lifts `DuckDBException` to the store-profile fault rail discriminated on this enum. `DuckDBConnectionStringBuilder` exposes `DataSource` plus the `const string` anchors `InMemoryDataSource` (`:memory:`), `InMemoryConnectionString` (`DataSource=:memory:`), `InMemorySharedDataSource` (`:memory:?cache=shared`), and `InMemorySharedConnectionString`. `DuckDBClientFactory : DbProviderFactory` carries the `static readonly Instance` singleton and `const string ProviderInvariantName = "DuckDB.NET.Data"` for `DbProviderFactories` registration.

| [INDEX] | [SYMBOL]                                | [PACKAGE_ROLE]    | [CAPABILITY]              |
| :-----: | :-------------------------------------- | :---------------- | :------------------------ |
|  [01]   | `DuckDBConnection`                      | connection root   | owns native connection    |
|  [02]   | `DuckDBCommand`                         | command surface   | executes SQL              |
|  [03]   | `DuckDBDataReader`                      | reader surface    | reads result rows         |
|  [04]   | `DuckDBParameter`                       | parameter surface | binds typed values        |
|  [05]   | `DuckDBParameterCollection`             | parameter set     | collects bound values     |
|  [06]   | `DuckDBTransaction`                     | transaction scope | commits or rolls back     |
|  [07]   | `DuckDBClientFactory`                   | provider factory  | creates ADO objects       |
|  [08]   | `DuckDBConnectionStringBuilder`         | connection string | names data sources        |
|  [09]   | `DuckDBException`                       | provider error    | carries native failure    |
|  [10]   | `DuckDBSchema`                          | schema surface    | reads metadata tables     |
|  [11]   | `DuckDB.NET.Native.DuckDBQueryProgress` | progress value    | reports query progress    |
|  [12]   | `DuckDB.NET.Native.DuckDBErrorType`     | error classifier  | classifies native failure |

[PUBLIC_TYPE_SCOPE]: bulk append surfaces
- rail: store-provider

| [INDEX] | [SYMBOL]                                        | [PACKAGE_ROLE]  | [CAPABILITY]           |
| :-----: | :---------------------------------------------- | :-------------- | :--------------------- |
|  [01]   | `DuckDB.NET.Data.DuckDBAppender`                | appender root   | streams bulk rows      |
|  [02]   | `DuckDB.NET.Data.IDuckDBAppenderRow`            | row contract    | appends typed values   |
|  [03]   | `DuckDB.NET.Data.DuckDBAppenderRow`             | row surface     | implements row appends |
|  [04]   | `DuckDB.NET.Data.DuckDBMappedAppender<T, TMap>` | mapped appender | appends mapped objects |
|  [05]   | `DuckDB.NET.Data.Mapping.DuckDBAppenderMap<T>`  | mapping owner   | maps object properties |

[PUBLIC_TYPE_SCOPE]: user-defined function surfaces
- rail: store-provider

`TableFunction` is a record whose PUBLIC constructor is `(IReadOnlyList<ColumnInfo> columns, IEnumerable data, CardinalityHint? cardinality = null)` exposing `Columns`/`Cardinality`; the projection-push-down `DataFactory` (`Func<IReadOnlyList<ProjectedColumn>, IEnumerable>`) and its second constructor are `internal` — column projection is reached through the high-level `DuckDBConnectionTableFunctionExtensions.RegisterTableFunction<TData, TProjection>(…, Expression<Func<TData, TProjection>> projection)` selector, never a public `DataFactory` ctor. `ScalarFunctionOptions` is a record carrying `IsPureFunction` (`bool?`, `init`) and `HandlesNulls` (`bool`, `init`); `ColumnInfo` is `(string Name, Type Type)`; `CardinalityHint` is `(ulong Value, bool IsExact = false)`; `ProjectedColumn` is `(int Index, string Name, Type Type)` materialized per-call when projection is active; `IDuckDBValueReader` (from `DuckDB.NET.Native`) exposes `T GetValue<T>()` and delivers typed values from table-function parameter readers.

| [INDEX] | [SYMBOL]                                   | [PACKAGE_ROLE]     | [CAPABILITY]              |
| :-----: | :----------------------------------------- | :----------------- | :------------------------ |
|  [01]   | `TableFunction`                            | function result    | declares table function   |
|  [02]   | `ColumnInfo`                               | column declaration | names column type         |
|  [03]   | `CardinalityHint`                          | planner hint       | states row cardinality    |
|  [04]   | `ScalarFunctionOptions`                    | function options   | states scalar UDF policy  |
|  [05]   | `NamedAttribute`                           | parameter name     | names function parameters |
|  [06]   | `ProjectedColumn`                          | projection cell    | carries projected column  |
|  [07]   | `DuckDB.NET.Native.IDuckDBValueReader`     | value reader       | reads typed param values  |
|  [08]   | `DuckDBConnectionScalarFunctionExtensions` | scalar extension   | extends connection UDFs   |
|  [09]   | `DuckDBConnectionTableFunctionExtensions`  | table extension    | extends connection UDFs   |

[PUBLIC_TYPE_SCOPE]: data-chunk vector surfaces
- rail: store-provider

| [INDEX] | [SYMBOL]                                              | [PACKAGE_ROLE]  | [CAPABILITY]                       |
| :-----: | :--------------------------------------------------- | :-------------- | :--------------------------------- |
|  [01]   | `DuckDB.NET.Data.DataChunk.Reader.IDuckDBDataReader` | reader contract | `IsValid(offset)`, `GetValue<T>`   |
|  [02]   | `DataChunk.Reader.VectorDataReaderBase`              | reader base     | projects typed columnar reads      |
|  [03]   | `DataChunk.Reader.VectorDataReaderFactory`           | reader factory  | creates typed vector readers       |
|  [04]   | `DuckDB.NET.Data.DataChunk.Writer.IDuckDBDataWriter` | writer contract | `WriteValue<T>`, `WriteNull`       |
|  [05]   | `DataChunk.Writer.VectorDataWriterBase`              | writer base     | projects typed columnar writes     |
|  [06]   | `DataChunk.Writer.VectorDataWriterFactory`           | writer factory  | creates typed vector writers       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: connection and command
- rail: store-provider

`NativeConnection` returns `DuckDBNativeConnection` and requires an open connection; `BeginTransaction()` returns the typed `DuckDBTransaction` directly.

| [INDEX] | [SURFACE]          | [CALL_SHAPE]     | [CAPABILITY]              |
| :-----: | :----------------- | :--------------- | :------------------------ |
|  [01]   | `Open`             | connection call  | opens native database     |
|  [02]   | `BeginTransaction` | connection call  | starts typed transaction  |
|  [03]   | `CreateCommand`    | connection call  | creates typed command     |
|  [04]   | `Duplicate`        | connection call  | clones open connection    |
|  [05]   | `GetQueryProgress` | connection call  | reads execution progress  |
|  [06]   | `GetSchema`        | connection call  | reads metadata collection |
|  [07]   | `NativeConnection` | connection prop  | exposes native handle     |
|  [08]   | `ExecuteReader`    | command call     | streams result rows       |
|  [09]   | `ExecuteNonQuery`  | command call     | runs statement            |
|  [10]   | `ExecuteScalar`    | command call     | reads single value        |
|  [11]   | `UseStreamingMode` | command property | selects streaming results |
|  [12]   | `Prepare`          | command call     | prepares statement        |

[ENTRYPOINT_SCOPE]: appender operations
- rail: store-provider

`CreateAppender` supports table, schema/table, and catalog/schema/table overloads; mapped appenders close after batch append. `IDuckDBAppenderRow.AppendValue` carries typed overloads for `bool?`, `byte[]?`, `Span<byte>`, `string?`, `decimal?`, `Guid?`, `BigInteger?`, all signed and unsigned integer widths, `float?`, `double?`, `DateTime?`, `DateTimeOffset?`, `TimeSpan?`, `DateOnly?`, `TimeOnly?`, `DuckDBDateOnly?`, `DuckDBTimeOnly?`, `IEnumerable<T>?`, and the generic `TEnum? where TEnum : Enum`.

| [INDEX] | [SURFACE]                                    | [CALL_SHAPE]    | [CAPABILITY]                |
| :-----: | :------------------------------------------- | :-------------- | :-------------------------- |
|  [01]   | `CreateAppender`                             | raw appender    | opens raw table appender    |
|  [02]   | `CreateAppender<T,TMap>`                     | mapped appender | opens mapped appender       |
|  [03]   | `DuckDBAppender.CreateRow`                   | appender call   | starts row append           |
|  [04]   | `AppendValue<T>` / `AppendNullValue`         | row call        | appends typed or null cell  |
|  [05]   | `AppendDefault`                              | row call        | appends default cell        |
|  [06]   | `IDuckDBAppenderRow.EndRow`                  | row call        | seals appended row          |
|  [07]   | `DuckDBMappedAppender<T,TMap>.AppendRecords` | mapped call     | bulk-appends mapped batch   |
|  [08]   | `DuckDBMappedAppender<T,TMap>.Close`         | mapped call     | flushes and closes appender |
|  [09]   | `DuckDBAppender.Clear`                       | appender call   | discards pending rows       |
|  [10]   | `DuckDBAppender.Close`                       | appender call   | flushes appended rows       |

[ENTRYPOINT_SCOPE]: mapped appender protocol — `DuckDBAppenderMap<T>`
- rail: store-provider

| [INDEX] | [SURFACE]                                   | [CALL_SHAPE]                 | [CAPABILITY]                         |
| :-----: | :------------------------------------------ | :--------------------------- | :----------------------------------- |
|  [01]   | `Map<TProperty>(Func<T, TProperty> getter)` | protected; declaration order | maps one column in declaration order |
|  [02]   | `DefaultValue()`                            | protected                    | writes the column's engine default   |
|  [03]   | `NullValue()`                               | protected                    | writes an explicit null cell         |

[ENTRYPOINT_SCOPE]: functions and data chunks
- rail: store-provider

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]         | [CAPABILITY]                          |
| :-----: | :------------------------------ | :------------------- | :------------------------------------ |
|  [01]   | `RegisterScalarFunction`        | connection low-level | registers `Action`-callback scalar UDF |
|  [02]   | `RegisterScalarFunction`        | scalar extension     | registers `Func`-based scalar UDF      |
|  [03]   | `RegisterTableFunction`         | connection low-level | registers callback table UDF          |
|  [04]   | `RegisterTableFunction`         | table extension      | registers typed projecting table UDF  |
|  [05]   | `GetValue`                      | reader call          | reads vector value                    |
|  [06]   | `IsValid`                       | reader call          | checks vector null mask               |
|  [07]   | `WriteValue`                    | writer call          | writes vector value                   |
|  [08]   | `WriteNull`                     | writer call          | writes vector null                    |
|  [09]   | `DuckDBClientFactory.Instance`  | static singleton     | `DbProviderFactories` provider object |

## [04]-[IMPLEMENTATION_LAW]

[UDF_PROTOCOL]:
- scalar low-level: `DuckDBConnection.RegisterScalarFunction` has a zero-input form `<TResult>(string, Action<IDuckDBDataWriter, ulong>, ScalarFunctionOptions?)` and input forms `<T1[..T4], TResult>(string, Action<IReadOnlyList<IDuckDBDataReader>, IDuckDBDataWriter, ulong>, ScalarFunctionOptions?)` (the single-input arity also takes a `bool @params` varargs flag); the callback fans the column readers and writes the result vector at `rowCount` rows
- scalar high-level: `DuckDBConnectionScalarFunctionExtensions.RegisterScalarFunction<…, TResult>` wraps a typed `Func<T1[..T4], TResult>` (plus a `Func<T[], TResult>` varargs overload), reading inputs through `ReadValue<T>` with nullability inference and synthesizing the `ScalarFunctionOptions`
- table low-level: `DuckDBConnection.RegisterTableFunction(string, Func<…, TableFunction> resultCallback, Action<object?, IDuckDBDataWriter[], ulong> mapperCallback)` declares `TableFunction` columns/data/cardinality through the public `(columns, data, cardinality)` ctor; the parameterized overloads bind `Func<IReadOnlyList<IDuckDBValueReader>, TableFunction>` over up to eight typed bind parameters plus a `params DuckDBType[] parameterTypes` form; the mapper writes cells through `IDuckDBDataWriter[]`
- table high-level: `DuckDBConnectionTableFunctionExtensions.RegisterTableFunction<TData, TProjection>(string, Func<… , IEnumerable<TData>> dataFunc, Expression<Func<TData, TProjection>> projection)` (over up to four typed bind parameters) is the only PUBLIC reach to column-projection push-down — the `Expression` selector drives the `internal` `DataFactory(IReadOnlyList<ProjectedColumn>)` so DuckDB requests only the projected columns

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

[STACKING]:
- snapshot codec: a `[ValueObject]`/`[SmartEnum]` owner crosses into a DuckDB column through one rail — the `api-thinktecture-serialization` `ThinktectureJsonConverterFactory`/`ThinktectureMessageFormatterResolver` projects the owner to its key, and the appender's typed `AppendValue` (or the data-chunk `WriteValue<T>`) writes that key; the inverse decodes the column value back through the same factory. No hand-rolled column mapping.
- store-profile receipts: the appender batch and the data-chunk vector throughput land as the `Store/profiles` typed receipt, alongside the `GetQueryProgress()` `DuckDBQueryProgress` percentage stream consumed by the AppHost `telemetry`/`drain` ports — the percentage/rows-processed values feed a progress span, not a bespoke logger.
- fault rail: `DuckDBException` lifts at the provider edge discriminated on `DuckDBErrorType`, joining the store-profile failure rail rather than surfacing as a raw ADO exception.
- columnar interchange: the analytical extract path (DuckDB result -> Arrow record batch) bridges through `api-arrow` plus the `Apache.Arrow.Adbc` driver manager, NOT a managed DuckDB Arrow member — see `ARROW_BOUNDARY`.

[ARROW_BOUNDARY]:
- Arrow C Data Interface (`QueryArrow`, `ArrowResultStream`, `ArrowArrayStream`): absent from the `DuckDB.NET.Data.Full`/`DuckDB.NET.Bindings.Full` v1.5.3 managed surface. Neither `DuckDB.NET.Data` nor `DuckDB.NET.Bindings` exposes any Arrow CLR type, method, or entry point at this version. The underlying DuckDB C library exports `duckdb_query_arrow`/`duckdb_arrow_array_stream` natively.
- The admitted Arrow stack is the columnar bridge: `Apache.Arrow` (`api-arrow`) owns the `RecordBatch`/`Schema` CLR model and `Apache.Arrow.Adbc` is the ADBC driver-manager surface (`Apache.Arrow.Adbc` ships `net8.0`/`netstandard2.0`; no bundled DuckDB driver assembly — the DuckDB ADBC driver is a native `[LibraryImport]` against `duckdb` loaded through the driver manager). A managed DuckDB->Arrow zero-copy path is therefore an explicit native-bridge rail, never a `DuckDBConnection` member.

[RAIL_LAW]:
- Package: `DuckDB.NET.Data.Full`
- Owns: DuckDB ADO provider admission
- Accept: DuckDB store profile with appender and UDF declarations
- Reject: provider-branded service families
