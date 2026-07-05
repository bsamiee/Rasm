# [RASM_PERSISTENCE_API_NPGSQL]

`Npgsql` supplies PostgreSQL data sources, connections, commands, transactions,
batches, parameters, type mapping, name translation, schema inspection, and
replication surfaces for provider store profiles.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Npgsql`
- package: `Npgsql`
- version: `10.0.3`
- assembly: `Npgsql`
- namespace: `Npgsql`, `Npgsql.Replication`, `Npgsql.Replication.PgOutput`
- target framework: `net10.0` asset on the `net10.0` floor (package also ships `net8.0`)
- asset: runtime library
- rail: store-provider

## [02]-[PUBLIC_TYPES]

[CONNECTION_TYPES]: data source and command surfaces
- rail: store-provider

The compact rows below preserve these member groups:
- `NpgsqlDataSource`: `CreateConnection`, `OpenConnection`, `OpenConnectionAsync`, `CreateCommand`, `CreateBatch`, `ReloadTypes`, `ReloadTypesAsync`, `Clear`
- `NpgsqlDataSourceBuilder`: `Build()`, `BuildMultiHost()`
- `NpgsqlMultiHostDataSource`: `WithTargetSession(TargetSessionAttributes)`, `OpenConnection(TargetSessionAttributes)`/`Async`, `CreateConnection(TargetSessionAttributes)`, `ClearDatabaseStates`
- `NpgsqlConnection`: `BeginBinaryImport`/`Async`, `BeginBinaryExport`/`Async`, `BeginRawBinaryCopy`/`Async`, `ReloadTypes`/`Async`, `CreateBatch`
- `NpgsqlConnectionStringBuilder`: `MaxAutoPrepare`, `AutoPrepareMinUsages`, `NoResetOnClose`, `Multiplexing`, `TargetSessionAttributes`, `LoadBalanceHosts`
- `NpgsqlConnectionStringBuilder`: `Options`, `IncludeErrorDetail`, `IncludeFailedBatchedCommand`, `LogParameters`
- `NpgsqlBatch`: `EnableErrorBarriers`
- `NpgsqlDataReader`: `GetFieldValue<T>`, `GetFieldValueAsync<T>`, `GetStream`, `GetStreamAsync`, `GetTextReader`, `GetTextReaderAsync`
- `NpgsqlDataReader`: `GetPostgresType`, `GetDataTypeOID`, `GetColumnSchema`, `GetColumnSchemaAsync`
- `PostgresException`: `SqlState`, `ConstraintName`, `ColumnName`, `TableName`, `Detail`, `Hint`, `IsTransient`, `PostgresErrorCodes.UndefinedObject = 42704`
- `NpgsqlTracingOptionsBuilder`: `ConfigureTracing`, command/batch/COPY filters, span-name providers, enrichment callbacks, `EnableFirstResponseEvent`, `EnablePhysicalOpenTracing`

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE]      | [CAPABILITY]                        |
| :-----: | :------------------------------ | :------------------ | :---------------------------------- |
|  [01]   | `NpgsqlDataSource`              | data source         | owns configured pool                |
|  [02]   | `NpgsqlDataSourceBuilder`       | data source builder | builds data source                  |
|  [03]   | `NpgsqlMultiHostDataSource`     | multi-host source   | owns multi-host pool                |
|  [04]   | `NpgsqlConnection`              | connection          | opens PostgreSQL store              |
|  [05]   | `NpgsqlConnectionStringBuilder` | connection builder  | builds connection strings           |
|  [06]   | `NpgsqlCommand`                 | command             | executes statements                 |
|  [07]   | `NpgsqlTransaction`             | transaction         | bounds atomic work                  |
|  [08]   | `NpgsqlBatch`                   | batch command       | executes batched work               |
|  [09]   | `NpgsqlBatchCommand`            | batch member        | carries batched command             |
|  [10]   | `NpgsqlParameter`               | parameter           | binds statement values              |
|  [11]   | `NpgsqlDataReader`              | data reader         | reads result rows                   |
|  [12]   | `NpgsqlException`               | provider exception  | reports provider failure            |
|  [13]   | `PostgresException`             | server exception    | reports server failure              |
|  [14]   | `PostgresErrorCodes`            | SQLSTATE constants  | names SQLSTATE values               |
|  [15]   | `NpgsqlMetricsOptions`          | meter options       | shapes instrumentation meter stream |
|  [16]   | `NpgsqlTracingOptionsBuilder`   | tracing options     | configures data-source tracing      |
|  [17]   | `NpgsqlNotificationEventArgs`   | notification event  | carries `PID`/`Channel`/`Payload` of a delivered `NOTIFY` |
|  [18]   | `NotificationEventHandler`      | notification handler | the `NpgsqlConnection.Notification` event delegate |

[TYPE_SYSTEM_TYPES]: PostgreSQL type surfaces
- rail: store-provider

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE]  | [CAPABILITY]              |
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
- rail: store-provider

The binary COPY surfaces expose these members:
- `NpgsqlBinaryImporter`: `StartRow`/`Async`, `Write<T>`/`Async` overload families, `WriteNull`/`Async`, `WriteRow`/`Async`, `Complete`/`Async`, `Close`/`Async`, `Timeout`
- `NpgsqlBinaryExporter`: `StartRow`/`Async`, `Read<T>`/`Async` overload families, `Skip`/`Async`, `IsNull`, `Timeout`, `Cancel`/`Async`
- `NpgsqlRawCopyStream`: `Read`, `Write`, async read/write, `FlushAsync`, `Cancel`

| [INDEX] | [SYMBOL]               | [PACKAGE_ROLE]     | [CAPABILITY]                     |
| :-----: | :--------------------- | :----------------- | :------------------------------- |
|  [01]   | `NpgsqlBinaryImporter` | binary-COPY writer | streams typed rows to PostgreSQL |
|  [02]   | `NpgsqlBinaryExporter` | binary-COPY reader | reads typed rows from PostgreSQL |
|  [03]   | `NpgsqlRawCopyStream`  | raw-COPY stream    | streams raw COPY bytes           |

[REPLICATION_TYPES]: logical replication surfaces
- rail: store-provider

Replication rows share the `Npgsql.Replication` namespace; pgoutput rows live under `PgOutput` and `PgOutput.Messages`.

Replication detail rows preserve these members:
- `LogicalReplicationConnection`: `StartReplication`, `SetReplicationStatus`, `SendStatusUpdate`, `IdentifySystem`, `CreatePgOutputReplicationSlot`
- `NpgsqlLogSequenceNumber`: `Parse`, `TryParse`, comparison operators, `Larger`, `Invalid`
- `PgOutputReplicationOptions`: publication names, `PgOutputProtocolVersion`, binary mode, streaming mode, messages, two-phase
- insert/update messages: `InsertMessage`, `UpdateMessage`, `FullUpdateMessage`, `IndexUpdateMessage`
- delete/truncate messages: `KeyDeleteMessage`, `FullDeleteMessage`, `TruncateMessage`
- relation/commit messages: `RelationMessage`, `CommitMessage`, `StreamCommitMessage`, `StreamAbortMessage`, `LogicalDecodingMessage`
- pgoutput tuple data: `ReplicationTuple` (`IAsyncEnumerable<ReplicationValue>`, `NumColumns`) exposed by `InsertMessage.NewRow`/`UpdateMessage.NewRow`/`FullUpdateMessage.OldRow`/`KeyDeleteMessage.Key`/`FullDeleteMessage.OldRow`; `ReplicationValue` (`Kind`, `Length`, `IsDBNull`, `Get<T>(CancellationToken)`, `GetPostgresType`, `GetDataTypeName`); `TupleDataKind` (`Null`/`UnchangedToastedValue`/`TextValue`/`BinaryValue`)
- system identity: `ReplicationSystemIdentification` (`SystemId`, `Timeline`, `XLogPos`, `DbName`) returned by `IdentifySystem`; `TimelineHistoryFile` (`readonly struct`; `FileName`, `Content` `byte[]`) returned by `TimelineHistory(uint tli, ct)`

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]       | [CAPABILITY]                    |
| :-----: | :-------------------------------- | :------------------- | :------------------------------ |
|  [01]   | `LogicalReplicationConnection`    | replication root     | opens logical stream            |
|  [02]   | `NpgsqlLogSequenceNumber`         | LSN value            | carries WAL position            |
|  [03]   | `ReplicationSlot`                 | slot metadata        | identifies slot                 |
|  [04]   | `PgOutputReplicationSlot`         | slot handle          | attaches pgoutput slot          |
|  [05]   | `PgOutputReplicationOptions`      | replication policy   | configures pgoutput             |
|  [06]   | `TestDecodingOptions`             | replication policy   | rejected alternative output     |
|  [07]   | `ReplicationMessage`              | replication message  | carries stream event            |
|  [08]   | `PgOutputProtocolVersion`         | protocol classifier  | classifies protocol version     |
|  [09]   | `PgOutputStreamingMode`           | streaming classifier | classifies streaming mode       |
|  [10]   | `PgOutputReplicationMessage`      | message base         | roots pgoutput message family   |
|  [11]   | insert/update messages            | message leaves       | insert/update leaf frames       |
|  [12]   | delete/truncate/relation messages | message leaves       | delete/truncate/relation frames |
|  [13]   | `ReplicationTuple`                | pgoutput tuple       | async-enumerates a row's column values |
|  [14]   | `ReplicationValue`                | pgoutput column      | one column value + `Kind`/`Get<T>`     |
|  [15]   | `TupleDataKind`                   | tuple-cell classifier | classifies a pgoutput cell kind       |
|  [16]   | `ReplicationSystemIdentification` | system identity      | carries WAL position + timeline        |
|  [17]   | `TimelineHistoryFile`             | timeline history     | carries a timeline history file        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: data source builder configuration
- rail: store-provider

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]       | [CAPABILITY]                                                                          |
| :-----: | :--------------------------------- | :----------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `NpgsqlDataSourceBuilder(string?)` | ctor               | creates builder from connection string                                                |
|  [02]   | `Build`                            | builder call       | yields `NpgsqlDataSource`                                                             |
|  [03]   | `BuildMultiHost`                   | builder call       | yields `NpgsqlMultiHostDataSource`                                                    |
|  [04]   | `UseLoggerFactory`                 | builder config     | attaches `ILoggerFactory`                                                             |
|  [05]   | `EnableParameterLogging`           | builder config     | logs parameter values; redaction-policy decision                                      |
|  [06]   | `ConfigureTypeLoading`             | builder config     | tunes type-loading behavior via `NpgsqlTypeLoadingOptionsBuilder`                     |
|  [07]   | `ConfigureTracing`                 | builder config     | tunes OTel tracing via `NpgsqlTracingOptionsBuilder`                                  |
|  [08]   | `ConfigureJsonOptions`             | builder config     | sets `JsonSerializerOptions` for all JSON paths                                       |
|  [09]   | `MapEnum<TEnum>`                   | builder mapping    | maps CLR enum to PostgreSQL enum type                                                 |
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
|  [01]   | `NpgsqlDataSource.Create`           | static factory     | creates data source from string or builder     |
|  [02]   | `OpenConnection`                    | data source call   | opens pooled connection                        |
|  [03]   | `OpenConnectionAsync`               | async call         | opens pooled connection                        |
|  [04]   | `CreateCommand`                     | factory call       | creates command (connection-less on fast path) |
|  [05]   | `CreateBatch`                       | factory call       | creates batch (connection-less on fast path)   |
|  [06]   | `NpgsqlDataSource.ReloadTypes`      | data source call   | reloads type registry after DDL changes        |
|  [07]   | `NpgsqlDataSource.ReloadTypesAsync` | async call         | async reload type registry                     |
|  [08]   | `NpgsqlDataSource.Clear`            | data source call   | clears all pooled connections                  |
|  [09]   | `BeginTransaction`                  | connection call    | starts transaction                             |
|  [10]   | `BeginTransactionAsync`             | async call         | starts transaction                             |
|  [11]   | `ExecuteReader`                     | command/batch call | reads rows                                     |
|  [12]   | `ExecuteNonQuery`                   | command/batch call | writes changes                                 |
|  [13]   | `ExecuteScalar`                     | command/batch call | reads scalar value                             |

[ENTRYPOINT_SCOPE]: binary COPY
- rail: store-provider

| [INDEX] | [SURFACE]                                | [CALL_SHAPE]      | [CAPABILITY]                                                           |
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

[ENTRYPOINT_SCOPE]: advisory locks (session and transaction mutual exclusion)
- rail: store-provider

PostgreSQL advisory locks have NO typed `Npgsql` member — they are server functions composed as SQL through `NpgsqlCommand`/`NpgsqlBatch`; the `_xact_` family auto-releases at transaction end (no explicit unlock), the session family requires an explicit unlock. This is the fenced-lease substrate the coordination owner composes, never a distributed-lock sidecar.

| [INDEX] | [SURFACE]                                                        | [CALL_SHAPE]     | [CAPABILITY]                                                           |
| :-----: | :--------------------------------------------------------------- | :--------------- | :-------------------------------------------------------------------- |
|  [01]   | `SELECT pg_advisory_xact_lock(@key)` via `NpgsqlCommand`         | SQL over command | transaction-scoped exclusive lock; auto-released at COMMIT/ROLLBACK   |
|  [02]   | `SELECT pg_try_advisory_xact_lock(@key)` via `NpgsqlCommand`     | SQL over command | non-blocking try; returns `bool` acquired, transaction-scoped         |
|  [03]   | `SELECT pg_advisory_xact_lock_shared(@key)` via `NpgsqlCommand`  | SQL over command | transaction-scoped SHARED lock (readers coexist, writers exclude)     |
|  [04]   | `SELECT pg_advisory_lock(@key)` / `pg_advisory_unlock(@key)`     | SQL over command | session-scoped lock requiring explicit unlock (or `pg_advisory_unlock_all()`) |
|  [05]   | `NpgsqlBatch` of `pg_advisory_xact_lock` + guarded `UPDATE … RETURNING` | SQL over batch | one round-trip lock-then-fenced-CAS: the lock and the guarded write share the transaction |

[ENTRYPOINT_SCOPE]: LISTEN/NOTIFY (asynchronous change notification)
- rail: store-provider

`LISTEN`/`NOTIFY`/`UNLISTEN` are SQL composed through `NpgsqlCommand`; delivered notifications raise the `NpgsqlConnection.Notification` event and are pumped by `Wait`/`WaitAsync` on an otherwise-idle connection. A `NpgsqlNotificationEventArgs` carries `PID`/`Channel`/`Payload`.

| [INDEX] | [SURFACE]                                                        | [CALL_SHAPE]     | [CAPABILITY]                                                           |
| :-----: | :--------------------------------------------------------------- | :--------------- | :-------------------------------------------------------------------- |
|  [01]   | `NpgsqlConnection.Notification` (`event NotificationEventHandler`) | event          | raised per delivered `NOTIFY`; args carry `PID`/`Channel`/`Payload`   |
|  [02]   | `SELECT` / `LISTEN <channel>` via `NpgsqlCommand`                | SQL over command | subscribes the connection to a channel                                |
|  [03]   | `SELECT pg_notify(@channel, @payload)` via `NpgsqlCommand`       | SQL over command | parameterized publish (the `NOTIFY` form that takes a runtime payload) |
|  [04]   | `NpgsqlConnection.WaitAsync(CancellationToken)`                  | async pump       | awaits the next notification on an idle connection                     |
|  [05]   | `NpgsqlConnection.WaitAsync(TimeSpan / int timeout, CancellationToken)` | async pump | bounded wait; returns `bool` (true if a notification arrived)          |
|  [06]   | `NpgsqlConnection.Wait()` / `Wait(TimeSpan / int timeout)`       | sync pump        | synchronous block for a notification (the blocking twin)              |

[ENTRYPOINT_SCOPE]: replication
- rail: store-provider

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]     | [CAPABILITY]                                   |
| :-----: | :------------------------------ | :--------------- | :--------------------------------------------- |
|  [01]   | `StartReplication`              | replication call | starts replication; returns `IAsyncEnumerable` |
|  [02]   | `CreatePgOutputReplicationSlot` | replication call | creates pgoutput slot                          |
|  [03]   | `SetReplicationStatus`          | replication call | stamps applied-and-flushed LSN                 |
|  [04]   | `SendStatusUpdate`              | replication call | sends feedback flush                           |
|  [05]   | `IdentifySystem`                | replication call | returns `ReplicationSystemIdentification` (`XLogPos`/`Timeline`/`SystemId`) |
|  [06]   | `TimelineHistory(uint tli, ct)` | replication call | returns the `TimelineHistoryFile` for a timeline |
|  [07]   | `await foreach (ReplicationValue v in tuple)` / `v.Kind` / `v.Get<T>(ct)` | tuple read | streams a pgoutput row's column values discriminated by `TupleDataKind` |

`PgOutputReplicationOptions` accepts publication names, protocol version, binary mode, streaming mode, messages, and two-phase policy. The binary importer commit edge is inverted: `Complete`/`CompleteAsync` commits; disposal without it cancels COPY and discards all buffered rows.

## [04]-[IMPLEMENTATION_LAW]

[STORE_PROFILE]:
- profile: PostgreSQL is one admitted store profile
- data source root: `NpgsqlDataSource` (factory: `NpgsqlDataSource.Create` or `NpgsqlDataSourceBuilder.Build`)
- connection root: `NpgsqlConnection`
- query root: commands, batches, parameters, and readers
- type root: provider type mapper, name translator, and PostgreSQL metadata
- tracing root: `NpgsqlTracingOptionsBuilder` via `ConfigureTracing`

[COORDINATION_PRIMITIVES]:
- Advisory locks are server functions, not typed members: `pg_advisory_xact_lock`/`pg_try_advisory_xact_lock`/`pg_advisory_xact_lock_shared` (transaction-scoped, auto-released at COMMIT/ROLLBACK) and `pg_advisory_lock`/`pg_advisory_unlock`/`pg_advisory_unlock_all` (session-scoped, explicit unlock) compose as SQL through `NpgsqlCommand`/`NpgsqlBatch` — the `_xact_` family is the preferred form because release is transactional, never leaked by a dropped connection.
- LISTEN/NOTIFY is asynchronous change signalling: `LISTEN <channel>` subscribes and `pg_notify(channel, payload)` publishes (both SQL over `NpgsqlCommand`), delivery raises `NpgsqlConnection.Notification` (`NpgsqlNotificationEventArgs.PID`/`Channel`/`Payload`), and an idle connection is pumped by `WaitAsync(CancellationToken)`; a notification is best-effort while the listener is disconnected — it is a low-latency WAKE, never an at-least-once cursor.

[REPLICATION_UNCONSUMED]:
- `Npgsql.Replication`/`Npgsql.Replication.PgOutput` (`LogicalReplicationConnection`, pgoutput slot + message family, `ReplicationTuple`) is RECORDED-UNCONSUMED: the changefeed is Marten's async daemon over the event stream, not raw-WAL logical decoding. Logical replication is the NAMED escalation path — admitted only if a raw-WAL CDC consumer ever lands beside the daemon; until then it is documented surface, not a composed rail.

[LOCAL_ADMISSION]:
- PostgreSQL enters through the unified store-profile algebra.
- Provider type mapping stays profile configuration, not public service vocabulary.
- Logical replication is a store capability and requires explicit receipt projection.
- Data source lifetime, pooling, batching, and transaction policy are profile data.
- `EnableErrorBarriers` on `NpgsqlBatch` is per-batch fault granularity policy: off for transactional batches, on for independent-fact batches.
- `MaxAutoPrepare` / `AutoPrepareMinUsages` on `NpgsqlConnectionStringBuilder` configure the LRU prepare budget; explicit `Prepare()` is exempt from eviction.
- `NoResetOnClose` disqualifies when a `UsePhysicalConnectionInitializer` establishes session state.
- `NpgsqlTracingOptionsBuilder.ConfigureCopyOperationFilter` / `EnableFirstResponseEvent` / `EnablePhysicalOpenTracing` are profile-level tracing policy rows; per-call-site tracing is the rejected form.

[STACKING]:
- provisioning (`Store/provisioning#SERVER_EXTENSIONS`): `Npgsql` is the transport the verification-first provisioning fold rides — ONE `NpgsqlBatch`/`CreateBatch` over the catalog reads (`pg_available_extensions`/`pg_extension`/`pg_settings`/`pg_replication_slots`) folds the `ProvisionVerdict`, and `NpgsqlDataSource.ReloadTypesAsync` completes a deploy by re-resolving wire types a freshly-admitted enum/composite introduced; a catalog read denied by privilege folds `ServerFault.CatalogDenied` (`PostgresErrorCodes.InsufficientPrivilege`) and a transport fault routes through `NpgsqlException.IsTransient` so a retry re-drives only the transient class.
- coordination (`Store/coordination#OUTBOX_CURSOR`): the fenced-lease store composes Marten `FetchForWriting`/`QueueSqlCommand` + Npgsql `pg_advisory_xact_lock` + LISTEN/NOTIFY — the advisory lock guards the fenced compare-and-decrement (`Coordinate.Run` `BudgetDebit`/`StepStateCas`/`LeaseAcquire`), the lock and the guarded `UPDATE … RETURNING` sharing one transaction so a stale token is a typed `LeaseFenced`, never a lost update.
- egress (`Version/egress#EGRESS_SINK`): `NpgsqlConnection.Notification` + `WaitAsync` is the low-latency WAKE the egress pump (`EgressPump.Drain`) listens on — a committed outbox row emits a `pg_notify` beat so the pump drains the `#OUTBOX_CURSOR` promptly instead of polling; the beat is a hint, the cursor is the at-least-once record, so a missed notification degrades to poll-latency, never a dropped delivery.

[RAIL_LAW]:
- Package: `Npgsql`
- Owns: PostgreSQL transport API — data source/connection/command/batch/COPY, the advisory-lock and LISTEN/NOTIFY primitives (as composed SQL), and the recorded-unconsumed logical-replication surface
- Accept: PostgreSQL store profile, advisory locks and LISTEN/NOTIFY composed through `NpgsqlCommand`/`NpgsqlBatch` for the coordination/egress owners
- Reject: provider-specific public service families, a distributed-lock sidecar beside the advisory-lock primitive, treating LISTEN/NOTIFY as an at-least-once cursor, and composing `Npgsql.Replication` before a raw-WAL CDC consumer is admitted
