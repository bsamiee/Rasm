# [RASM_PERSISTENCE_API_NPGSQL]

`Npgsql` supplies PostgreSQL data sources, connections, commands, transactions,
batches, parameters, type mapping, name translation, schema inspection, and
replication surfaces for provider store profiles.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Npgsql`
- package: `Npgsql`
- assembly: `Npgsql`
- namespace: `Npgsql`
- asset: runtime library
- rail: store-provider

## [2]-[PUBLIC_TYPES]

[CONNECTION_TYPES]: data source and command surfaces
- rail: store-provider

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE]      | [CAPABILITY]                                                                                                                                                                                                                                                                                                                                                                                                               |
| :-----: | :------------------------------ | :------------------ | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `NpgsqlDataSource`              | data source         | owns configured pool; `CreateConnection`, `OpenConnection`, `OpenConnectionAsync`, `CreateCommand`, `CreateBatch`, `ReloadTypes`, `ReloadTypesAsync`, `Clear`                                                                                                                                                                                                                                                              |
|   [2]   | `NpgsqlDataSourceBuilder`       | data source builder | builds data source; `Build()`, `BuildMultiHost()`                                                                                                                                                                                                                                                                                                                                                                          |
|   [3]   | `NpgsqlMultiHostDataSource`     | multi-host source   | multi-host pool; `TargetSessionAttributes` per acquisition                                                                                                                                                                                                                                                                                                                                                                 |
|   [4]   | `NpgsqlConnection`              | connection          | opens PostgreSQL store; `BeginBinaryImport`/`Async`, `BeginBinaryExport`/`Async`, `BeginRawBinaryCopy`/`Async`, `ReloadTypes`/`Async`, `CreateBatch`                                                                                                                                                                                                                                                                       |
|   [5]   | `NpgsqlConnectionStringBuilder` | connection builder  | builds connection strings; `MaxAutoPrepare`, `AutoPrepareMinUsages`, `NoResetOnClose`, `Multiplexing`, `TargetSessionAttributes`, `LoadBalanceHosts`, `Options`, `IncludeErrorDetail`, `IncludeFailedBatchedCommand`, `LogParameters`                                                                                                                                                                                      |
|   [6]   | `NpgsqlCommand`                 | command             | executes statements                                                                                                                                                                                                                                                                                                                                                                                                        |
|   [7]   | `NpgsqlTransaction`             | transaction         | bounds atomic work                                                                                                                                                                                                                                                                                                                                                                                                         |
|   [8]   | `NpgsqlBatch`                   | batch command       | executes batched work; `EnableErrorBarriers` selects per-command fault granularity                                                                                                                                                                                                                                                                                                                                         |
|   [9]   | `NpgsqlBatchCommand`            | batch member        | carries batched command                                                                                                                                                                                                                                                                                                                                                                                                    |
|  [10]   | `NpgsqlParameter`               | parameter           | binds statement values                                                                                                                                                                                                                                                                                                                                                                                                     |
|  [11]   | `NpgsqlDataReader`              | data reader         | reads result rows; `GetFieldValue<T>`, `GetFieldValueAsync<T>`, `GetStream`, `GetStreamAsync`, `GetTextReader`, `GetTextReaderAsync`, `GetPostgresType`, `GetDataTypeOID`, `GetColumnSchema`, `GetColumnSchemaAsync`                                                                                                                                                                                                       |
|  [12]   | `NpgsqlException`               | provider exception  | reports provider failure; `IsTransient`                                                                                                                                                                                                                                                                                                                                                                                    |
|  [13]   | `PostgresException`             | server exception    | carries `SqlState`, `ConstraintName`, `ColumnName`, `TableName`, `Detail`, `Hint`; `IsTransient`; `PostgresErrorCodes.UndefinedObject` = 42704                                                                                                                                                                                                                                                                             |
|  [14]   | `PostgresErrorCodes`            | SQLSTATE constants  | names SQLSTATE values                                                                                                                                                                                                                                                                                                                                                                                                      |
|  [15]   | `NpgsqlMetricsOptions`          | meter options       | shapes `AddNpgsqlInstrumentation` meter stream                                                                                                                                                                                                                                                                                                                                                                             |
|  [16]   | `NpgsqlTracingOptionsBuilder`   | tracing options     | configures tracing via `ConfigureTracing`; `ConfigureCommandFilter`, `ConfigureBatchFilter`, `ConfigureCommandSpanNameProvider`, `ConfigureBatchSpanNameProvider`, `ConfigureCommandEnrichmentCallback`, `ConfigureBatchEnrichmentCallback`, `ConfigureCopyOperationFilter`, `ConfigureCopyOperationEnrichmentCallback`, `ConfigureCopyOperationSpanNameProvider`, `EnableFirstResponseEvent`, `EnablePhysicalOpenTracing` |

[TYPE_SYSTEM_TYPES]: PostgreSQL type surfaces
- rail: store-provider

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE]  | [CAPABILITY]              |
| :-----: | :------------------------------ | :-------------- | :------------------------ |
|   [1]   | `NpgsqlDbType`                  | type classifier | classifies parameters     |
|   [2]   | `NpgsqlParameter<T>`            | typed parameter | binds typed values        |
|   [3]   | `INpgsqlTypeMapper`             | type mapper     | maps provider types       |
|   [4]   | `INpgsqlNameTranslator`         | name translator | maps CLR names            |
|   [5]   | `NpgsqlSnakeCaseNameTranslator` | name translator | maps snake case names     |
|   [6]   | `PostgresType`                  | schema metadata | describes store types     |
|   [7]   | `PostgresEnumType`              | schema metadata | describes enum types      |
|   [8]   | `PostgresCompositeType`         | schema metadata | describes composite types |
|   [9]   | `NpgsqlRange<T>`                | range value     | carries range values      |
|  [10]   | `NpgsqlInterval`                | interval value  | carries interval values   |

[COPY_TYPES]: binary COPY surfaces
- rail: store-provider

| [INDEX] | [SYMBOL]               | [PACKAGE_ROLE]     | [CAPABILITY]                                                                                                                                                                                                                  |
| :-----: | :--------------------- | :----------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `NpgsqlBinaryImporter` | binary-COPY writer | streams typed rows to PostgreSQL; `StartRow`/`Async`, `Write<T>`/`Async` (3 overloads: bare, `NpgsqlDbType`, `dataTypeName`), `WriteNull`/`Async`, `WriteRow`/`Async`, `Complete`/`Async → ulong`, `Close`/`Async`, `Timeout` |
|   [2]   | `NpgsqlBinaryExporter` | binary-COPY reader | reads typed rows from PostgreSQL; `StartRow`/`Async → int`, `Read<T>`/`Async` (2 overloads: bare, `NpgsqlDbType`), `Skip`/`Async`, `IsNull`, `Timeout`, `Cancel`/`Async`                                                      |
|   [3]   | `NpgsqlRawCopyStream`  | raw-COPY stream    | zero-materialization table-to-table pipe; `Stream` subtype with `Read`/`Write`/`ReadAsync`/`WriteAsync`/`FlushAsync`, `Cancel`                                                                                                |

[REPLICATION_TYPES]: logical replication surfaces
- rail: store-provider

Replication rows share the `Npgsql.Replication` namespace; pgoutput rows live under `PgOutput` and `PgOutput.Messages`.

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]       | [CAPABILITY]                                                                                                                                                          |
| :-----: | :-------------------------------- | :------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `LogicalReplicationConnection`    | replication root     | opens logical stream; `StartReplication`, `SetReplicationStatus`, `SendStatusUpdate`, `IdentifySystem`, `CreatePgOutputReplicationSlot`                               |
|   [2]   | `NpgsqlLogSequenceNumber`         | LSN value            | WAL position; `Parse`, `TryParse`, comparison operators, `Larger`, `Invalid`                                                                                          |
|   [3]   | `ReplicationSlot`                 | slot metadata        | identifies slot                                                                                                                                                       |
|   [4]   | `PgOutputReplicationSlot`         | slot handle          | attaches pgoutput slot                                                                                                                                                |
|   [5]   | `PgOutputReplicationOptions`      | replication policy   | configures pgoutput: publication names, `PgOutputProtocolVersion`, `binary`, `streamingMode`, `messages`, two-phase                                                   |
|   [6]   | `TestDecodingOptions`             | replication policy   | rejected alternative output                                                                                                                                           |
|   [7]   | `ReplicationMessage`              | replication message  | carries stream event                                                                                                                                                  |
|   [8]   | `PgOutputProtocolVersion`         | protocol classifier  | classifies protocol version                                                                                                                                           |
|   [9]   | `PgOutputStreamingMode`           | streaming classifier | classifies streaming mode (`Off`, `On`, `Parallel`)                                                                                                                   |
|  [10]   | `PgOutputReplicationMessage`      | message base         | roots pgoutput message family                                                                                                                                         |
|  [11]   | insert/update messages            | message leaves       | `InsertMessage`, `UpdateMessage`, `FullUpdateMessage`, `IndexUpdateMessage`                                                                                           |
|  [12]   | delete/truncate/relation messages | message leaves       | `KeyDeleteMessage`, `FullDeleteMessage`, `TruncateMessage`, `RelationMessage`, `CommitMessage`, `StreamCommitMessage`, `StreamAbortMessage`, `LogicalDecodingMessage` |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: data source builder configuration
- rail: store-provider

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]       | [CAPABILITY]                                                                          |
| :-----: | :--------------------------------- | :----------------- | :------------------------------------------------------------------------------------ |
|   [1]   | `NpgsqlDataSourceBuilder(string?)` | ctor               | creates builder from connection string                                                |
|   [2]   | `Build`                            | builder call       | yields `NpgsqlDataSource`                                                             |
|   [3]   | `BuildMultiHost`                   | builder call       | yields `NpgsqlMultiHostDataSource`                                                    |
|   [4]   | `UseLoggerFactory`                 | builder config     | attaches `ILoggerFactory`                                                             |
|   [5]   | `EnableParameterLogging`           | builder config     | logs parameter values; redaction-policy decision                                      |
|   [6]   | `ConfigureTypeLoading`             | builder config     | tunes type-loading behavior via `NpgsqlTypeLoadingOptionsBuilder`                     |
|   [7]   | `ConfigureTracing`                 | builder config     | tunes OTel tracing via `NpgsqlTracingOptionsBuilder`                                  |
|   [8]   | `ConfigureJsonOptions`             | builder config     | sets `JsonSerializerOptions` for all JSON paths                                       |
|   [9]   | `MapEnum<TEnum>`                   | builder mapping    | maps CLR enum to PostgreSQL enum type                                                 |
|  [10]   | `MapEnum(Type, …)`                 | builder mapping    | maps enum by `Type`                                                                   |
|  [11]   | `MapComposite<T>`                  | builder mapping    | maps CLR type to PostgreSQL composite type                                            |
|  [12]   | `MapComposite(Type, …)`            | builder mapping    | maps composite by `Type`                                                              |
|  [13]   | `EnableDynamicJson`                | builder mapping    | enables JSON mapping; optional `jsonbClrTypes`/`jsonClrTypes` arrays                  |
|  [14]   | `EnableRecordsAsTuples`            | builder mapping    | maps C# records as PostgreSQL row types                                               |
|  [15]   | `EnableUnmappedTypes`              | builder mapping    | enables unmapped types                                                                |
|  [16]   | `UsePeriodicPasswordProvider`      | builder credential | rotates passwords on cadence; success/failure intervals                               |
|  [17]   | `UsePasswordProvider`              | builder credential | provides password via sync or async callback                                          |
|  [18]   | `UsePhysicalConnectionInitializer` | builder session    | runs per-session setup on each new physical connection; disqualifies `NoResetOnClose` |

[ENTRYPOINT_SCOPE]: data source and connection execution
- rail: store-provider

| [INDEX] | [SURFACE]                           | [CALL_SHAPE]       | [CAPABILITY]                                   |
| :-----: | :---------------------------------- | :----------------- | :--------------------------------------------- |
|   [1]   | `NpgsqlDataSource.Create`           | static factory     | creates data source from string or builder     |
|   [2]   | `OpenConnection`                    | data source call   | opens pooled connection                        |
|   [3]   | `OpenConnectionAsync`               | async call         | opens pooled connection                        |
|   [4]   | `CreateCommand`                     | factory call       | creates command (connection-less on fast path) |
|   [5]   | `CreateBatch`                       | factory call       | creates batch (connection-less on fast path)   |
|   [6]   | `NpgsqlDataSource.ReloadTypes`      | data source call   | reloads type registry after DDL changes        |
|   [7]   | `NpgsqlDataSource.ReloadTypesAsync` | async call         | async reload type registry                     |
|   [8]   | `NpgsqlDataSource.Clear`            | data source call   | clears all pooled connections                  |
|   [9]   | `BeginTransaction`                  | connection call    | starts transaction                             |
|  [10]   | `BeginTransactionAsync`             | async call         | starts transaction                             |
|  [11]   | `ExecuteReader`                     | command/batch call | reads rows                                     |
|  [12]   | `ExecuteNonQuery`                   | command/batch call | writes changes                                 |
|  [13]   | `ExecuteScalar`                     | command/batch call | reads scalar value                             |

[ENTRYPOINT_SCOPE]: binary COPY and replication
- rail: store-provider

| [INDEX] | [SURFACE]                                | [CALL_SHAPE]      | [CAPABILITY]                                                           |
| :-----: | :--------------------------------------- | :---------------- | :--------------------------------------------------------------------- |
|   [1]   | `BeginBinaryImport`                      | connection call   | opens binary-COPY import path (sync)                                   |
|   [2]   | `BeginBinaryImportAsync`                 | async call        | opens binary-COPY import path (async)                                  |
|   [3]   | `BeginBinaryExport`                      | connection call   | opens binary-COPY export path (sync)                                   |
|   [4]   | `BeginBinaryExportAsync`                 | async call        | opens binary-COPY export path (async)                                  |
|   [5]   | `BeginRawBinaryCopy`                     | connection call   | opens raw-COPY stream (zero-materialization pipe)                      |
|   [6]   | `BeginRawBinaryCopyAsync`                | async call        | opens raw-COPY stream (async)                                          |
|   [7]   | `NpgsqlBinaryImporter.StartRow`/`Async`  | importer call     | starts a row; must precede each column                                 |
|   [8]   | `NpgsqlBinaryImporter.Write<T>`/`Async`  | importer call     | writes column value (bare, `NpgsqlDbType`, or `dataTypeName` overload) |
|   [9]   | `NpgsqlBinaryImporter.WriteNull`/`Async` | importer call     | writes null column value                                               |
|  [10]   | `NpgsqlBinaryImporter.WriteRow`/`Async`  | importer call     | writes complete row from `params object?[]`                            |
|  [11]   | `NpgsqlBinaryImporter.Complete`/`Async`  | importer call     | commits COPY, returns `ulong` rows imported                            |
|  [12]   | `NpgsqlBinaryExporter.StartRow`/`Async`  | exporter call     | advances to next row, returns column count                             |
|  [13]   | `NpgsqlBinaryExporter.Read<T>`/`Async`   | exporter call     | reads typed column value (bare or `NpgsqlDbType`)                      |
|  [14]   | `NpgsqlBinaryExporter.Skip`/`Async`      | exporter call     | skips current column                                                   |
|  [15]   | `NpgsqlBinaryExporter.IsNull`            | exporter property | true when current column is null                                       |
|  [16]   | `StartReplication`                       | replication call  | starts replication; returns `IAsyncEnumerable`                         |
|  [17]   | `CreatePgOutputReplicationSlot`          | replication call  | creates pgoutput slot                                                  |
|  [18]   | `SetReplicationStatus`                   | replication call  | stamps applied-and-flushed LSN                                         |
|  [19]   | `SendStatusUpdate`                       | replication call  | sends feedback flush                                                   |
|  [20]   | `NpgsqlConnection.ReloadTypes`/`Async`   | connection call   | reloads type registry on the connection                                |

`PgOutputReplicationOptions` accepts publication names, protocol version, binary mode, streaming mode, messages, and two-phase policy. The binary importer commit edge is inverted: `Complete`/`CompleteAsync` commits; disposal without it cancels COPY and discards all buffered rows.

## [4]-[IMPLEMENTATION_LAW]

[STORE_PROFILE]:
- profile: PostgreSQL is one admitted store profile
- data source root: `NpgsqlDataSource` (factory: `NpgsqlDataSource.Create` or `NpgsqlDataSourceBuilder.Build`)
- connection root: `NpgsqlConnection`
- query root: commands, batches, parameters, and readers
- type root: provider type mapper, name translator, and PostgreSQL metadata
- tracing root: `NpgsqlTracingOptionsBuilder` via `ConfigureTracing`

[LOCAL_ADMISSION]:
- PostgreSQL enters through the unified store-profile algebra.
- Provider type mapping stays profile configuration, not public service vocabulary.
- Logical replication is a store capability and requires explicit receipt projection.
- Data source lifetime, pooling, batching, and transaction policy are profile data.
- `EnableErrorBarriers` on `NpgsqlBatch` is per-batch fault granularity policy: off for transactional batches, on for independent-fact batches.
- `MaxAutoPrepare` / `AutoPrepareMinUsages` on `NpgsqlConnectionStringBuilder` configure the LRU prepare budget; explicit `Prepare()` is exempt from eviction.
- `NoResetOnClose` disqualifies when a `UsePhysicalConnectionInitializer` establishes session state.
- `NpgsqlTracingOptionsBuilder.ConfigureCopyOperationFilter` / `EnableFirstResponseEvent` / `EnablePhysicalOpenTracing` are profile-level tracing policy rows; per-call-site tracing is the rejected form.

[RAIL_LAW]:
- Package: `Npgsql`
- Owns: PostgreSQL transport API
- Accept: PostgreSQL store profile
- Reject: provider-specific public service families
