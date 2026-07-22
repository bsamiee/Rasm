# [RASM_PERSISTENCE_API_NPGSQL]

`Npgsql` owns the PostgreSQL transport every store profile rides — the pooled data source through binary COPY and logical-replication streams — and the advisory-lock and LISTEN/NOTIFY coordination primitives it composes as SQL. Store-profile algebra folds this transport, never a second provider service family.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Npgsql`
- package: `Npgsql`
- assembly: `Npgsql`
- namespace: `Npgsql`, `NpgsqlTypes`, `Npgsql.PostgresTypes`, `Npgsql.NameTranslation`, `Npgsql.TypeMapping`, `Npgsql.Replication`, `Npgsql.Replication.PgOutput`, `Npgsql.Replication.PgOutput.Messages`
- target: `net10.0`
- asset: runtime library
- rail: store-provider

## [02]-[PUBLIC_TYPES]

[CONNECTION_TYPES]: data source and command surfaces

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]        | [CAPABILITY]                                              |
| :-----: | :------------------------------ | :------------------- | :-------------------------------------------------------- |
|  [01]   | `NpgsqlDataSource`              | data source          | owns configured pool                                      |
|  [02]   | `NpgsqlDataSourceBuilder`       | data source builder  | builds data source                                        |
|  [03]   | `NpgsqlMultiHostDataSource`     | multi-host source    | owns multi-host pool                                      |
|  [04]   | `NpgsqlConnection`              | connection           | opens PostgreSQL store                                    |
|  [05]   | `NpgsqlConnectionStringBuilder` | connection builder   | builds connection strings                                 |
|  [06]   | `NpgsqlCommand`                 | command              | executes statements                                       |
|  [07]   | `NpgsqlTransaction`             | transaction          | bounds atomic work                                        |
|  [08]   | `NpgsqlBatch`                   | batch command        | executes batched work                                     |
|  [09]   | `NpgsqlBatchCommand`            | batch member         | carries batched command                                   |
|  [10]   | `NpgsqlParameter`               | parameter            | binds statement values                                    |
|  [11]   | `NpgsqlDataReader`              | data reader          | reads result rows                                         |
|  [12]   | `NpgsqlException`               | provider exception   | reports provider failure                                  |
|  [13]   | `PostgresException`             | server exception     | reports server failure                                    |
|  [14]   | `PostgresErrorCodes`            | SQLSTATE constants   | names SQLSTATE values                                     |
|  [15]   | `NpgsqlMetricsOptions`          | meter options        | shapes instrumentation meter stream                       |
|  [16]   | `NpgsqlTracingOptionsBuilder`   | tracing options      | configures data-source tracing                            |
|  [17]   | `NpgsqlNotificationEventArgs`   | notification event   | carries `PID`/`Channel`/`Payload` of a delivered `NOTIFY` |
|  [18]   | `NotificationEventHandler`      | notification handler | the `NpgsqlConnection.Notification` event delegate        |

- `NpgsqlMultiHostDataSource`: `WithTargetSession(TargetSessionAttributes)` `CreateConnection(TargetSessionAttributes)` `OpenConnection(TargetSessionAttributes)`/`Async` `ClearDatabaseStates`
- `NpgsqlConnectionStringBuilder`: `MaxAutoPrepare` `AutoPrepareMinUsages` `NoResetOnClose` `Multiplexing` `TargetSessionAttributes` `LoadBalanceHosts` `Options` `IncludeErrorDetail` `IncludeFailedBatchedCommand` `LogParameters`
- `NpgsqlDataReader`: `GetFieldValue<T>`/`Async` `GetStream`/`Async` `GetTextReader`/`Async` `GetPostgresType` `GetDataTypeOID` `GetColumnSchema`/`Async`
- `PostgresException`: `SqlState` `ConstraintName` `ColumnName` `TableName` `Detail` `Hint` `IsTransient`; `PostgresErrorCodes.UndefinedObject = "42704"`, `InsufficientPrivilege = "42501"`
- `NpgsqlTracingOptionsBuilder`: `ConfigureCommandFilter` `ConfigureBatchFilter` `ConfigureCopyOperationFilter` `ConfigureCommandSpanNameProvider` `EnableFirstResponseEvent` `EnablePhysicalOpenTracing`

[TYPE_SYSTEM_TYPES]: PostgreSQL type surfaces

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]   | [CAPABILITY]              |
| :-----: | :------------------------------ | :-------------- | :------------------------ |
|  [01]   | `NpgsqlDbType`                  | type classifier | classifies parameters     |
|  [02]   | `NpgsqlParameter<T>`            | typed parameter | binds typed values        |
|  [03]   | `INpgsqlTypeMapper`             | type mapper     | maps provider types       |
|  [04]   | `INpgsqlNameTranslator`         | name translator | maps CLR names            |
|  [05]   | `NpgsqlSnakeCaseNameTranslator` | name translator | maps snake case names     |
|  [06]   | `PostgresType`                  | schema metadata | describes store types     |
|  [07]   | `PostgresEnumType`              | schema metadata | describes enum types      |
|  [08]   | `PostgresCompositeType`         | schema metadata | describes composite types |
|  [09]   | `NpgsqlRange<T>`                | range value     | carries range values      |
|  [10]   | `NpgsqlInterval`                | interval value  | carries interval values   |

[COPY_TYPES]: binary COPY surfaces

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]      | [CAPABILITY]                     |
| :-----: | :--------------------- | :----------------- | :------------------------------- |
|  [01]   | `NpgsqlBinaryImporter` | binary-COPY writer | streams typed rows to PostgreSQL |
|  [02]   | `NpgsqlBinaryExporter` | binary-COPY reader | reads typed rows from PostgreSQL |
|  [03]   | `NpgsqlRawCopyStream`  | raw-COPY stream    | streams raw COPY bytes           |

- `NpgsqlRawCopyStream` is a `Stream` carrying `Cancel` for mid-copy abort; importer and exporter members ride the `[03]` binary-COPY entrypoints.

[REPLICATION_TYPES]: logical replication surfaces

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]         | [CAPABILITY]                           |
| :-----: | :-------------------------------- | :-------------------- | :------------------------------------- |
|  [01]   | `LogicalReplicationConnection`    | replication root      | opens logical stream                   |
|  [02]   | `NpgsqlLogSequenceNumber`         | LSN value             | carries WAL position                   |
|  [03]   | `ReplicationSlot`                 | slot metadata         | identifies slot                        |
|  [04]   | `PgOutputReplicationSlot`         | slot handle           | attaches pgoutput slot                 |
|  [05]   | `PgOutputReplicationOptions`      | replication policy    | configures pgoutput                    |
|  [06]   | `TestDecodingOptions`             | replication policy    | rejected alternative output            |
|  [07]   | `ReplicationMessage`              | replication message   | carries stream event                   |
|  [08]   | `PgOutputProtocolVersion`         | protocol classifier   | classifies protocol version            |
|  [09]   | `PgOutputStreamingMode`           | streaming classifier  | classifies streaming mode              |
|  [10]   | `PgOutputReplicationMessage`      | message base          | roots pgoutput message family          |
|  [11]   | `ReplicationTuple`                | pgoutput tuple        | async-enumerates a row's column values |
|  [12]   | `ReplicationValue`                | pgoutput column       | one column value + `Kind`/`Get<T>`     |
|  [13]   | `TupleDataKind`                   | tuple-cell classifier | classifies a pgoutput cell kind        |
|  [14]   | `ReplicationSystemIdentification` | system identity       | carries WAL position + timeline        |
|  [15]   | `TimelineHistoryFile`             | timeline history      | carries a timeline history file        |

- `Npgsql.Replication.PgOutput.Messages`: `InsertMessage` `UpdateMessage` `FullUpdateMessage` `IndexUpdateMessage` `KeyDeleteMessage` `FullDeleteMessage` `TruncateMessage` `RelationMessage` `CommitMessage` `StreamCommitMessage` `StreamAbortMessage` `LogicalDecodingMessage`
- `NpgsqlLogSequenceNumber`: `Parse` `TryParse` `Larger` `Invalid` and comparison operators.
- `PgOutputReplicationOptions`: publication names, `PgOutputProtocolVersion`, binary mode, streaming mode, messages, two-phase.
- `ReplicationValue`: `Kind` `Length` `IsDBNull` `Get<T>(CancellationToken)` `GetPostgresType` `GetDataTypeName`; `TupleDataKind` cases `Null` `UnchangedToastedValue` `TextValue` `BinaryValue`.
- `ReplicationTuple` (`IAsyncEnumerable<ReplicationValue>`, `NumColumns`) rides `InsertMessage.NewRow`, `UpdateMessage.NewRow`, `FullUpdateMessage.OldRow`, `KeyDeleteMessage.Key`, `FullDeleteMessage.OldRow`.
- `ReplicationSystemIdentification`: `SystemId` `Timeline` `XLogPos` `DbName` from `IdentifySystem`; `TimelineHistoryFile`: `FileName` `Content` from `TimelineHistory(uint, ct)`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: data source builder configuration

| [INDEX] | [SURFACE]                          | [SHAPE]            | [CAPABILITY]                                                                 |
| :-----: | :--------------------------------- | :----------------- | :--------------------------------------------------------------------------- |
|  [01]   | `NpgsqlDataSourceBuilder(string?)` | ctor               | creates builder from connection string                                       |
|  [02]   | `Build`                            | builder call       | yields `NpgsqlDataSource`                                                    |
|  [03]   | `BuildMultiHost`                   | builder call       | yields `NpgsqlMultiHostDataSource`                                           |
|  [04]   | `UseLoggerFactory`                 | builder config     | attaches `ILoggerFactory`                                                    |
|  [05]   | `EnableParameterLogging`           | builder config     | logs parameter values; redaction-policy decision                             |
|  [06]   | `ConfigureTypeLoading`             | builder config     | tunes type-loading behavior via `NpgsqlTypeLoadingOptionsBuilder`            |
|  [07]   | `ConfigureTracing`                 | builder config     | tunes OTel tracing via `NpgsqlTracingOptionsBuilder`                         |
|  [08]   | `ConfigureJsonOptions`             | builder config     | sets `JsonSerializerOptions` for all JSON paths                              |
|  [09]   | `MapEnum<TEnum>`                   | builder mapping    | maps CLR enum to PostgreSQL enum type                                        |
|  [10]   | `MapEnum(Type, …)`                 | builder mapping    | maps enum by `Type`                                                          |
|  [11]   | `MapComposite<T>`                  | builder mapping    | maps CLR type to PostgreSQL composite type                                   |
|  [12]   | `MapComposite(Type, …)`            | builder mapping    | maps composite by `Type`                                                     |
|  [13]   | `EnableDynamicJson`                | builder mapping    | enables JSON mapping; optional `jsonbClrTypes`/`jsonClrTypes` arrays         |
|  [14]   | `EnableRecordsAsTuples`            | builder mapping    | maps C# records as PostgreSQL row types                                      |
|  [15]   | `EnableUnmappedTypes`              | builder mapping    | enables unmapped types                                                       |
|  [16]   | `UsePeriodicPasswordProvider`      | builder credential | rotates passwords on cadence; success/failure intervals                      |
|  [17]   | `UsePasswordProvider`              | builder credential | provides password via sync or async callback                                 |
|  [18]   | `UsePhysicalConnectionInitializer` | builder session    | per-session setup per new physical connection; disqualifies `NoResetOnClose` |

[ENTRYPOINT_SCOPE]: data source and connection execution

| [INDEX] | [SURFACE]                           | [SHAPE]            | [CAPABILITY]                                   |
| :-----: | :---------------------------------- | :----------------- | :--------------------------------------------- |
|  [01]   | `NpgsqlDataSource.Create`           | static factory     | creates data source from string or builder     |
|  [02]   | `NpgsqlDataSource.CreateConnection` | factory call       | creates unopened pooled connection             |
|  [03]   | `OpenConnection`                    | data source call   | opens pooled connection                        |
|  [04]   | `OpenConnectionAsync`               | async call         | opens pooled connection                        |
|  [05]   | `CreateCommand`                     | factory call       | creates command (connection-less on fast path) |
|  [06]   | `CreateBatch`                       | factory call       | creates batch (connection-less on fast path)   |
|  [07]   | `NpgsqlDataSource.ReloadTypes`      | data source call   | reloads type registry after DDL changes        |
|  [08]   | `NpgsqlDataSource.ReloadTypesAsync` | async call         | async reload type registry                     |
|  [09]   | `NpgsqlDataSource.Clear`            | data source call   | clears all pooled connections                  |
|  [10]   | `BeginTransaction`                  | connection call    | starts transaction                             |
|  [11]   | `BeginTransactionAsync`             | async call         | starts transaction                             |
|  [12]   | `ExecuteReader`                     | command/batch call | reads rows                                     |
|  [13]   | `ExecuteNonQuery`                   | command/batch call | writes changes                                 |
|  [14]   | `ExecuteScalar`                     | command/batch call | reads scalar value                             |

[ENTRYPOINT_SCOPE]: binary COPY

| [INDEX] | [SURFACE]                                | [SHAPE]           | [CAPABILITY]                                                           |
| :-----: | :--------------------------------------- | :---------------- | :--------------------------------------------------------------------- |
|  [01]   | `BeginBinaryImport`                      | connection call   | opens binary-COPY import path (sync)                                   |
|  [02]   | `BeginBinaryImportAsync`                 | async call        | opens binary-COPY import path (async)                                  |
|  [03]   | `BeginBinaryExport`                      | connection call   | opens binary-COPY export path (sync)                                   |
|  [04]   | `BeginBinaryExportAsync`                 | async call        | opens binary-COPY export path (async)                                  |
|  [05]   | `BeginRawBinaryCopy`                     | connection call   | opens raw-COPY stream (zero-materialization pipe)                      |
|  [06]   | `BeginRawBinaryCopyAsync`                | async call        | opens raw-COPY stream (async)                                          |
|  [07]   | `NpgsqlBinaryImporter.StartRow`/`Async`  | importer call     | starts a row; must precede each column                                 |
|  [08]   | `NpgsqlBinaryImporter.Write<T>`/`Async`  | importer call     | writes column value (bare, `NpgsqlDbType`, or `dataTypeName` overload) |
|  [09]   | `NpgsqlBinaryImporter.WriteNull`/`Async` | importer call     | writes null column value                                               |
|  [10]   | `NpgsqlBinaryImporter.WriteRow`/`Async`  | importer call     | writes complete row from `params object?[]`                            |
|  [11]   | `NpgsqlBinaryImporter.Complete`/`Async`  | importer call     | commits COPY, returns `ulong` rows imported                            |
|  [12]   | `NpgsqlBinaryExporter.StartRow`/`Async`  | exporter call     | advances to next row, returns column count                             |
|  [13]   | `NpgsqlBinaryExporter.Read<T>`/`Async`   | exporter call     | reads typed column value (bare or `NpgsqlDbType`)                      |
|  [14]   | `NpgsqlBinaryExporter.Skip`/`Async`      | exporter call     | skips current column                                                   |
|  [15]   | `NpgsqlBinaryExporter.IsNull`            | exporter property | true when current column is null                                       |
|  [16]   | `NpgsqlConnection.ReloadTypes`/`Async`   | connection call   | reloads type registry on the connection                                |
|  [17]   | `NpgsqlBinaryImporter.Close`/`Async`     | importer call     | closes the importer, leaving an uncompleted COPY cancelled             |
|  [18]   | `NpgsqlBinaryExporter.Cancel`/`Async`    | exporter call     | cancels the in-flight COPY export                                      |
|  [19]   | `NpgsqlBinaryImporter.Timeout`           | importer property | bounds each importer operation                                         |
|  [20]   | `NpgsqlBinaryExporter.Timeout`           | exporter property | bounds each exporter operation                                         |

- `NpgsqlBinaryImporter.Complete`/`CompleteAsync` commits the COPY; disposal without it cancels and discards every buffered row.

[ENTRYPOINT_SCOPE]: advisory locks (session and transaction mutual exclusion)

PostgreSQL advisory locks carry no typed member — server functions composed as SQL through `NpgsqlCommand`/`NpgsqlBatch`.

| [INDEX] | [SURFACE]                                                     | [CAPABILITY]                                                         |
| :-----: | :------------------------------------------------------------ | :------------------------------------------------------------------- |
|  [01]   | `SELECT pg_advisory_xact_lock(@key)`                          | transaction-scoped exclusive lock; auto-released at COMMIT/ROLLBACK  |
|  [02]   | `SELECT pg_try_advisory_xact_lock(@key)`                      | non-blocking try; returns `bool` acquired, transaction-scoped        |
|  [03]   | `SELECT pg_advisory_xact_lock_shared(@key)`                   | transaction-scoped SHARED lock (readers coexist, writers exclude)    |
|  [04]   | `SELECT pg_advisory_lock(@key)` / `pg_advisory_unlock(@key)`  | session-scoped lock; explicit unlock (or `pg_advisory_unlock_all()`) |
|  [05]   | `NpgsqlBatch`: `pg_advisory_xact_lock` + `UPDATE … RETURNING` | one round-trip: lock + guarded CAS in one transaction                |

[ENTRYPOINT_SCOPE]: LISTEN/NOTIFY (asynchronous change notification)

`LISTEN`/`NOTIFY`/`UNLISTEN` compose as SQL through `NpgsqlCommand`; a delivered `NOTIFY` raises `NpgsqlConnection.Notification` (`NpgsqlNotificationEventArgs.PID`/`Channel`/`Payload`) and pumps through `Wait`/`WaitAsync` on an idle connection.

| [INDEX] | [SURFACE]                                              | [SHAPE]          | [CAPABILITY]                                             |
| :-----: | :----------------------------------------------------- | :--------------- | :------------------------------------------------------- |
|  [01]   | `Notification` (`event NotificationEventHandler`)      | event            | raised per delivered `NOTIFY`                            |
|  [02]   | `SELECT` / `LISTEN <channel>`                          | SQL over command | subscribes the connection to a channel                   |
|  [03]   | `SELECT pg_notify(@channel, @payload)`                 | SQL over command | parameterized publish; takes a runtime payload           |
|  [04]   | `WaitAsync(CancellationToken)`                         | async pump       | awaits the next notification on an idle connection       |
|  [05]   | `WaitAsync(TimeSpan / int timeout, CancellationToken)` | async pump       | bounded wait; `bool` true if a notification arrived      |
|  [06]   | `Wait()` / `Wait(TimeSpan / int timeout)`              | sync pump        | synchronous block for a notification (the blocking twin) |

[ENTRYPOINT_SCOPE]: replication

| [INDEX] | [SURFACE]                                     | [SHAPE]          | [CAPABILITY]                                                    |
| :-----: | :-------------------------------------------- | :--------------- | :-------------------------------------------------------------- |
|  [01]   | `StartReplication`                            | replication call | starts replication; returns `IAsyncEnumerable`                  |
|  [02]   | `CreatePgOutputReplicationSlot`               | replication call | creates pgoutput slot                                           |
|  [03]   | `SetReplicationStatus`                        | replication call | stamps applied-and-flushed LSN                                  |
|  [04]   | `SendStatusUpdate`                            | replication call | sends feedback flush                                            |
|  [05]   | `IdentifySystem`                              | replication call | returns `ReplicationSystemIdentification`                       |
|  [06]   | `TimelineHistory(uint tli, ct)`               | replication call | returns the `TimelineHistoryFile` for a timeline                |
|  [07]   | `await foreach (ReplicationValue v in tuple)` | tuple read       | streams column values; `v.Kind`/`v.Get<T>(ct)`, `TupleDataKind` |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- PostgreSQL enters as one store profile behind the unified store-profile algebra: `NpgsqlDataSource` is the pooled root (`NpgsqlDataSource.Create` or `NpgsqlDataSourceBuilder.Build`), `NpgsqlConnection` the connection root, commands/batches/parameters/readers the query root, the type mapper and name translator the type root, `NpgsqlTracingOptionsBuilder` via `ConfigureTracing` the tracing root.
- Advisory locks and LISTEN/NOTIFY carry no typed member — both compose as SQL through `NpgsqlCommand`/`NpgsqlBatch`. `pg_advisory_xact_lock` releases at COMMIT/ROLLBACK so a dropped connection never leaks a lock; a delivered `NOTIFY` on `NpgsqlConnection.Notification` is a low-latency WAKE, the durable cursor owning at-least-once delivery.

[STACKING]:
- provisioning (`Store/provisioning`): one `CreateBatch` over `pg_available_extensions`/`pg_extension`/`pg_settings`/`pg_replication_slots` folds the `ProvisionVerdict`, and `NpgsqlDataSource.ReloadTypesAsync` re-resolves wire types a freshly-admitted enum or composite introduced; a privilege-denied read folds `ServerFault.CatalogDenied` (`PostgresErrorCodes.InsufficientPrivilege`), and `NpgsqlException.IsTransient` gates retry to the transient class.
- coordination (`Store/coordination`): fenced-lease store composes Marten `FetchForWriting`/`QueueSqlCommand` with `pg_advisory_xact_lock` and LISTEN/NOTIFY — advisory lock and guarded `UPDATE … RETURNING` share one transaction, so a stale token is a typed `LeaseFenced` rather than a lost update.
- egress (`Version/egress`): `NpgsqlConnection.Notification` with `WaitAsync` is the WAKE the egress pump listens on — a committed outbox row emits a `pg_notify` beat so the pump drains the outbox cursor instead of polling, and a missed beat degrades to poll-latency, never a dropped delivery.

[LOCAL_ADMISSION]:
- PostgreSQL enters through the store-profile algebra; provider type mapping, data-source lifetime, pooling, batching, and transaction policy are profile data, never public service vocabulary.
- `EnableErrorBarriers` on `NpgsqlBatch` sets per-batch fault granularity: off for transactional batches, on for independent-fact batches.
- `MaxAutoPrepare`/`AutoPrepareMinUsages` size the LRU prepare budget, explicit `Prepare()` eviction-exempt; `NoResetOnClose` disqualifies once a `UsePhysicalConnectionInitializer` establishes session state.
- `NpgsqlTracingOptionsBuilder` filters (`ConfigureCopyOperationFilter`, `EnableFirstResponseEvent`, `EnablePhysicalOpenTracing`) are profile-level tracing policy, never per-call-site.
- `Npgsql.Replication`/`Npgsql.Replication.PgOutput` is recorded-unconsumed: Marten's async daemon over the event stream owns the changefeed, and logical replication admits only when a raw-WAL CDC consumer lands beside the daemon.

[RAIL_LAW]:
- Package: `Npgsql`
- Owns: PostgreSQL transport — data source/connection/command/batch/COPY, the advisory-lock and LISTEN/NOTIFY primitives as composed SQL, and the recorded-unconsumed logical-replication surface
- Accept: the PostgreSQL store profile, advisory locks and LISTEN/NOTIFY composed through `NpgsqlCommand`/`NpgsqlBatch` for the coordination and egress owners
- Reject: provider-specific public service families, a distributed-lock sidecar beside the advisory-lock primitive, treating LISTEN/NOTIFY as an at-least-once cursor, and composing `Npgsql.Replication` before a raw-WAL CDC consumer is admitted
