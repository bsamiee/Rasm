# [RASM_PERSISTENCE_API_SCYLLADB]

`ScyllaDBCSharpDriver` owns CQL wide-column store access — the shard-per-core, tunable-consistency backend driving ScyllaDB and Apache Cassandra over the one CQL binary protocol, folding every `DriverException` onto the `Fin` rail. It is the scale-out wide-column class alone; the relational, columnar-OLAP, and dedicated-vector concerns route to their own backends.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ScyllaDBCSharpDriver`
- package: `ScyllaDBCSharpDriver` (Apache-2.0, DataStax and ScyllaDB)
- assembly: `ScyllaDB` (assembly `ScyllaDB.dll`, distinct from the package id)
- namespace: `Cassandra` public surface — the fork preserves the DataStax type names, never `ScyllaDB.*`; `Cassandra.Mapping`, `Cassandra.Data.Linq`, `Cassandra.Mapping.Attributes`, `Cassandra.ExecutionProfiles`, `Cassandra.Metrics.Abstractions`
- target: single-target `netstandard2.0`; the `net10.0` consumer binds `lib/netstandard2.0`
- asset: pure-managed, AnyCPU, no native runtime payload
- abi: `Row`/`RowSet` reference-type rows, `Task`-based async, paging via `IPage<T>` + `byte[] pagingState` — no `Span`/`ref struct` row API, no `IAsyncEnumerable` row stream
- depends: `K4os.Compression.LZ4` (shared with the LZ4 snapshot codec — the driver's `CompressionType.LZ4` wire compression) and driver-internal `Newtonsoft.Json` (the workspace JSON rail stays `System.Text.Json`, never routing through it)
- rail: store-backend (wide-column / CQL)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: cluster, session, configuration (namespace `Cassandra`)
- ddl: `ISession` runs `ChangeKeyspace`/`CreateKeyspace[IfNotExists]`/`DeleteKeyspace[IfExists]`/`GetMetrics`/`ShutdownAsync`

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]          | [CAPABILITY]                                                               |
| :-----: | :----------------------------- | :--------------------- | :------------------------------------------------------------------------- |
|  [01]   | `Cluster`                      | connected-cluster root | `Builder()`/`BuildFrom`, `Connect*`, `Metadata`                            |
|  [02]   | `Builder`                      | fluent configuration   | the one fluent config root -> `Build()`                                    |
|  [03]   | `ISession`                     | execution rail         | `: IDisposable`; execution + keyspace-DDL rail                             |
|  [04]   | `ICluster`/`IInitializer`      | cluster contracts      | the `ISession`-factory contract / the `BuildFrom` initializer seam         |
|  [05]   | `Configuration`                | resolved config        | the materialized cluster configuration (`Builder.Build` result)            |
|  [06]   | `RowSet` / `Row`               | result set / row       | `IEnumerable<Row>`; `Row.GetValue<T>(name\|i)`, `PagingState`, auto-paging |
|  [07]   | `ExecutionInfo` / `QueryTrace` | per-query telemetry    | achieved consistency, queried hosts, server-side trace events              |

[PUBLIC_TYPE_SCOPE]: query statements (namespace `Cassandra`)
- knobs: every statement sets `SetConsistencyLevel`/`SetSerialConsistencyLevel`/`SetIdempotence`/`SetRoutingKey`/`SetRoutingNames`/`SetRoutingValues`/`SetPageSize`; `PreparedStatement` adds `SetLwt` and exposes `Variables` (`RowSetMetadata`)

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]           | [CAPABILITY]                                                                         |
| :-----: | :------------------ | :---------------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `SimpleStatement`   | inline CQL              | `: RegularStatement`; positional/named values, `Bind`, `SetRoutingKey`               |
|  [02]   | `PreparedStatement` | prepared query          | `Bind(params object[]) -> BoundStatement`                                            |
|  [03]   | `BoundStatement`    | bound prepared          | a `PreparedStatement` bound with values; the executed unit                           |
|  [04]   | `BatchStatement`    | atomic batch            | `: Statement`; `Add(Statement)`, `SetBatchType(BatchType)`, `SetKeyspace`            |
|  [05]   | `RoutingKey`        | routing token           | partition-key bytes the token-aware policy routes on; `PreparedStatement.RoutingKey` |
|  [06]   | `RowSetMetadata`    | bound-variable metadata | `PreparedStatement.Variables`; prepared column/partition-key layout                  |

[PUBLIC_TYPE_SCOPE]: POCO data access (namespace `Cassandra.Mapping`)

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]     | [CAPABILITY]                                                                              |
| :-----: | :--------------------- | :---------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `Mapper`               | POCO mapper       | `: IMapper`; async `Fetch*`/`Insert*`/`Update*`/`Delete*` CRUD, `ExecuteAsync(ICqlBatch)` |
|  [02]   | `Cql`                  | query builder     | static `New(cql, params object[] args)`; `WithOptions(...)`, `WithExecutionProfile`       |
|  [03]   | `CqlQueryOptions`      | per-query options | `Set{ConsistencyLevel,SerialConsistencyLevel,PageSize,PagingState,Timestamp,RetryPolicy}` |
|  [04]   | `MappingConfiguration` | mapping registry  | `Global`; registers `Map<T>` POCO->table definitions                                      |
|  [05]   | `Map<T>` / `Mappings`  | fluent POCO map   | `TableName`, `PartitionKey`, `ClusteringKey`, `Column`, `CaseSensitive`                   |
|  [06]   | `AppliedInfo<T>`       | LWT verdict       | `Applied` (bool) + the existing row when an `IF`-conditional was not applied              |
|  [07]   | `ICqlBatch`            | mapped batch      | `mapper.CreateBatch([BatchType])` -> `ExecuteConditionalAsync<T>` -> `AppliedInfo<T>`     |
|  [08]   | `IPage<T>`             | paged result      | `: ICollection<T>`; `PagingState`/`CurrentPagingState` (`byte[]`) cursor continuation     |

[PUBLIC_TYPE_SCOPE]: attribute-mapped IQueryable (namespace `Cassandra.Data.Linq`)
- attributes: the POCO->table schema `[PartitionKey]`/`[ClusteringKey]`/`[Column]`/`[SecondaryIndex]`/`[Counter]`/`[StaticColumn]`/`[Ignore]`/`[Table]` (the `Cassandra.Data.Linq` + `Cassandra.Mapping.Attributes` twins)

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                                                            |
| :-----: | :----------------------------- | :------------ | :---------------------------------------------------------------------- |
|  [01]   | `Table<TEntity>`               | LINQ table    | `: CqlQuery<T>, ITable, IQueryProvider`; `Create*` DDL, LINQ -> CQL     |
|  [02]   | `CqlQuery<T>` / `CqlScalar<T>` | LINQ query    | `Execute()`/`ExecuteAsync()`, `AllowFiltering()`, `SetConsistencyLevel` |
|  [03]   | `CqlInsert<T>`                 | LINQ mutation | `: CqlCommand`; generic fluent insert + `IfNotExists`/`SetTTL`          |
|  [04]   | `CqlUpdate`                    | LINQ mutation | `: CqlCommand`; fluent update + `If`/`SetTTL`/`SetTimestamp`            |
|  [05]   | `CqlDelete`                    | LINQ mutation | `: CqlCommand`; fluent delete + `If`/`SetTimestamp`                     |
|  [06]   | `CqlConditionalCommand<T>`     | LWT command   | the `IF`-conditional mutation returning applied/existing                |
|  [07]   | `Batch`                        | LINQ batch    | `Append(CqlCommand)`, `Execute()`/`ExecuteAsync()` atomic batch         |

[PUBLIC_TYPE_SCOPE]: routing, retry, consistency, encryption (namespace `Cassandra` + `Cassandra.ExecutionProfiles`)

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY]         | [CAPABILITY]                                                              |
| :-----: | :----------------------------------- | :-------------------- | :------------------------------------------------------------------------ |
|  [01]   | `TokenAwarePolicy`                   | routing policy        | wraps a child policy; routes to the replica owning the `RoutingKey`       |
|  [02]   | `DefaultLoadBalancingPolicy`         | load balancing        | shard-aware default (ScyllaDB)                                            |
|  [03]   | `DCAwareRoundRobinPolicy`            | load balancing        | DC-local round-robin                                                      |
|  [04]   | `RoundRobinPolicy`                   | load balancing        | plain round-robin                                                         |
|  [05]   | `Tablet`                             | tablet metadata       | ScyllaDB tablet ownership record                                          |
|  [06]   | `HostShard`                          | shard target          | per-shard routing target; `cluster.GetReplicas(...)` readout              |
|  [07]   | `HostShardPair`                      | shard pair            | host+shard routing pair (`TabletMap` stays internal)                      |
|  [08]   | `DefaultRetryPolicy`                 | retry policy          | default `RetryDecision` per failure class                                 |
|  [09]   | `IdempotenceAwareRetryPolicy`        | retry policy          | retries only idempotent statements                                        |
|  [10]   | `LoggingRetryPolicy`                 | retry policy          | logs then delegates the decision                                          |
|  [11]   | `DowngradingConsistencyRetryPolicy`  | retry policy          | downgrades the consistency level on failure                               |
|  [12]   | `FallthroughRetryPolicy`             | retry policy          | never retries; rethrows                                                   |
|  [13]   | `ConstantReconnectionPolicy`         | reconnection          | fixed-interval reconnect schedule                                         |
|  [14]   | `ExponentialReconnectionPolicy`      | reconnection          | exponential-backoff reconnect                                             |
|  [15]   | `FixedReconnectionPolicy`            | reconnection          | explicit per-attempt delay list                                           |
|  [16]   | `ConstantSpeculativeExecutionPolicy` | speculative execution | duplicate request after a fixed delay                                     |
|  [17]   | `NoSpeculativeExecutionPolicy`       | speculative execution | no pre-emptive duplication                                                |
|  [18]   | `IExecutionProfile`                  | execution profile     | resolved consistency+timeout+LB+retry bundle                              |
|  [19]   | `IExecutionProfileBuilder`           | execution profile     | builds a named execution profile                                          |
|  [20]   | `IColumnEncryptionPolicy`            | encryption contract   | per-column encrypt-before-write hook                                      |
|  [21]   | `AesColumnEncryptionPolicy`          | AES encryption        | per-column AES; key is the nested `AesColumnEncryptionPolicy.AesKeyAndIV` |
|  [22]   | `PlainTextAuthProvider`              | authentication        | username/password auth                                                    |
|  [23]   | `IAuthProvider`                      | authentication        | pluggable auth contract                                                   |
|  [24]   | `AtomicMonotonicTimestampGenerator`  | timestamps            | client-side monotonic write timestamps                                    |
|  [25]   | `ITimestampGenerator`                | timestamps            | pluggable timestamp generator contract                                    |
|  [26]   | `PoolingOptions`                     | connection tuning     | per-host connection pool sizing                                           |
|  [27]   | `SocketOptions`                      | connection tuning     | socket timeouts + keepalive                                               |
|  [28]   | `SSLOptions`                         | connection tuning     | TLS configuration                                                         |
|  [29]   | `QueryOptions`                       | connection tuning     | cluster-default query policy                                              |

[PUBLIC_TYPE_SCOPE]: CQL value types + classification enums (namespace `Cassandra`)
- schema: live-cluster reflection via `TableMetadata`/`KeyspaceMetadata`/`MaterializedViewMetadata`/`IndexMetadata`/`Metadata`
- levels: `ConsistencyLevel` = `Any`/`One`/`Two`/`Three`/`Quorum`/`All`/`LocalOne`/`LocalQuorum`/`EachQuorum` (read/write quorum) + `Serial`/`LocalSerial` (LWT)
- faults: `DriverException` base + `NoHostAvailableException`/`Read`/`WriteTimeoutException`/`UnavailableException`/`OperationTimedOutException`/`InvalidQueryException`/`AlreadyExistsException`

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]     | [CAPABILITY]                                                                |
| :-----: | :--------------------------- | :---------------- | :-------------------------------------------------------------------------- |
|  [01]   | `CqlVector<T>`               | ANN vector column | `: IReadOnlyCollection<T>`; `New(dims\|T[])`/`AsArray()`; `vector<float,n>` |
|  [02]   | `LocalDate` / `LocalTime`    | CQL date/time     | the CQL `date`/`time` types (distinct from `DateTimeOffset` `timestamp`)    |
|  [03]   | `UdtMap`                     | UDT mapping       | maps a CQL user-defined type to a CLR class                                 |
|  [04]   | `UdtMappingDefinitions`      | UDT mapping       | the UDT mapping registry                                                    |
|  [05]   | `ConsistencyLevel`           | consistency enum  | read/write quorum levels + `Serial`/`LocalSerial` LWT consistency           |
|  [06]   | `BatchType`                  | batch atomicity   | `Logged` (atomic via batch log) / `Unlogged` (no atomicity) / `Counter`     |
|  [07]   | `CompressionType`            | wire compression  | `NoCompression` / `Snappy` / `LZ4` (via transitive `K4os.Compression.LZ4`)  |
|  [08]   | `HostDistance`               | topology distance | `Local` / `Remote` / `Ignored` (the LB-policy host classification)          |
|  [09]   | `DriverException` (+ family) | typed faults      | the driver fault family, discriminated at the session edge                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: cluster construction + session open

| [INDEX] | [SURFACE]                                                     | [SHAPE]        | [CAPABILITY]                                 |
| :-----: | :------------------------------------------------------------ | :------------- | :------------------------------------------- |
|  [01]   | `Cluster.Builder()`                                           | static factory | starts the fluent configuration              |
|  [02]   | `Builder.AddContactPoints(...)` + `WithDefaultKeyspace(...)`  | instance       | seed nodes + default keyspace                |
|  [03]   | `Builder.WithCredentials(user, pass)`                         | instance       | username/password auth                       |
|  [04]   | `Builder.WithSSL(SSLOptions)`                                 | instance       | TLS                                          |
|  [05]   | `Builder.WithColumnEncryptionPolicy(IColumnEncryptionPolicy)` | instance       | client-side column encryption                |
|  [06]   | `Builder.WithLoadBalancingPolicy(new TokenAwarePolicy(...))`  | instance       | shard/token-aware routing                    |
|  [07]   | `Builder.WithExecutionProfiles(Action<...>)`                  | instance       | named consistency/timeout/retry bundles      |
|  [08]   | `Builder.Build() -> Cluster`                                  | factory        | the immutable connected-cluster root         |
|  [09]   | `cluster.Connect()` / `Connect(string keyspace) -> ISession`  | instance       | opens a session (optionally keyspace-scoped) |

[ENTRYPOINT_SCOPE]: statement execution — sync + async mirror

| [INDEX] | [SURFACE]                                                                | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :----------------------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `session.Prepare(cql) -> PreparedStatement` / `PrepareAsync -> Task<…>`  | instance | prepare (cached server-side by query id) |
|  [02]   | `prepared.Bind(values) -> BoundStatement`                                | instance | binds values + routing key from metadata |
|  [03]   | `bound.SetConsistencyLevel`/`SetIdempotence`                             | instance | per-statement consistency + retry-safety |
|  [04]   | `session.Execute(IStatement) -> RowSet` / `ExecuteAsync -> Task<RowSet>` | instance | execute; the async form is canonical     |
|  [05]   | `session.Execute(IStatement, profileName) -> RowSet`                     | instance | execute under a named execution profile  |
|  [06]   | `new BatchStatement().Add(s).SetBatchType(Logged)`                       | ctor     | atomic multi-statement batch             |
|  [07]   | `rowSet.PagingState` / `statement.SetPagingState(byte[])`                | property | stateless cursor continuation            |

[ENTRYPOINT_SCOPE]: POCO mapping (`Mapper`) — async-first

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------------------------ | :------- | :--------------------------------------- |
|  [01]   | `new Mapper(session[, config])`                                                 | ctor     | the POCO data-access entry               |
|  [02]   | `mapper.FetchAsync<T>(Cql\|string, args) -> Task<IEnumerable<T>>`               | instance | mapped query -> POCOs                    |
|  [03]   | `mapper.FetchPageAsync<T>(Cql.New(...).WithOptions(...)) -> Task<IPage<T>>`     | instance | paged fetch with `PagingState`           |
|  [04]   | `mapper.SingleAsync<T>` / `FirstOrDefaultAsync<T>(Cql) -> Task<T>`              | instance | single-row mapped reads                  |
|  [05]   | `mapper.InsertAsync<T>(poco[, insertNulls, ttl]) -> Task`                       | instance | mapped insert (optional null-skip + TTL) |
|  [06]   | `mapper.InsertIfNotExistsAsync<T>` / `UpdateIfAsync<T> -> Task<AppliedInfo<T>>` | instance | LWT conditional insert/update            |
|  [07]   | `mapper.DeleteAsync<T>(poco)` / `ExecuteAsync(ICqlBatch) -> Task`               | instance | mapped delete / mapped atomic batch      |

[ENTRYPOINT_SCOPE]: LINQ-to-CQL (`Table<T>`)
- `cqlQuery.AllowFiltering()`: explicit opt-in to a non-indexed scan, never implicit

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------------------ | :------- | :------------------------------------ |
|  [01]   | `new Table<T>(session)` / `Create[Async]() -> Task`                 | ctor     | bind a LINQ table / DDL from the POCO |
|  [02]   | `table.Where(predicate).Select(projection) -> CqlQuery<T>`          | instance | LINQ expression -> CQL                |
|  [03]   | `cqlQuery.ExecuteAsync() -> Task<IEnumerable<T>>` / `SetPageSize()` | instance | execute / page size                   |
|  [04]   | `table.Insert(poco).IfNotExists().SetTTL(n).ExecuteAsync() -> Task` | instance | fluent conditional insert with TTL    |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- bootstrap: `Cluster.Builder().AddContactPoints(...).With*(...).Build()` -> `cluster.Connect(keyspace)`; `Cluster` and `ISession` are heavyweight, thread-safe, long-lived singletons — one `ISession` per backend, opened once and reused, never per request.
- execution: the async `ExecuteAsync(IStatement) -> Task<RowSet>` is the canonical rail; `Prepare` once and `Bind` per call so the query parses server-side once and the routing key derives from the prepared metadata.
- routing: `Builder.WithLoadBalancingPolicy(new TokenAwarePolicy(new DefaultLoadBalancingPolicy(localDc)))` is the canonical policy — token + shard aware, so a `BoundStatement` with a known routing key reaches the owning replica's owning shard; the `Tablet`/`HostShard`/`HostShardPair` metadata makes the shard target precise, and `cluster.GetReplicas(keyspace, partitionKey)` is the public shard-resolution readout.
- consistency: `ConsistencyLevel` is per-statement (`SetConsistencyLevel`) or per-execution-profile (`WithExecutionProfiles`); `Serial`/`LocalSerial` are the LWT consistency for `IF`-conditional statements, distinct from the quorum levels for normal reads/writes.
- idempotence: `SetIdempotence(true)` marks a statement retry-safe so the retry/speculative-execution policy may re-issue it; a non-idempotent statement is never speculatively duplicated.

[STACKING]:
- policy rows: the `ExecutionProfiles` + retry/LB/speculative-execution policies declare once on the `Builder` and select per query by name — consistency, timeout, retry, and routing variation lives in a named profile, never in parallel session objects or per-call branching.
- compression: `CompressionType.LZ4` rides the transitive `K4os.Compression.LZ4` (`api-lz4`) for CQL frame compression — wire transport compression on the driver, distinct from the `CompressionPolicy` snapshot/blob codec axis.
- encryption + KMS: `AesColumnEncryptionPolicy`'s `AesKeyAndIV` key provisions through the KMS rail (`api-aws-kms`/`api-azure-keyvault`/`api-google-kms`) — the driver encrypts the column client-side before write, the KMS owns the key lifecycle; the policy consumes a KMS-issued data key, never a key store itself.
- vector lane: `CqlVector<T>` gives ScyllaDB native `vector<float, n>` columns for ANN co-located with the wide-column row; the billion-scale ANN store stays `Qdrant.Client` and the in-PG ANN tier stays `pgvector`/`pgvectorscale`.
- observability: `Builder.WithMetrics(IDriverMetricsProvider)` and `WithRequestTracker(IRequestTracker)` feed the AppHost telemetry port; per-`RowSet` `ExecutionInfo`/`QueryTrace` carry achieved consistency and queried-host detail into the query receipt.

[LOCAL_ADMISSION]:
- ScyllaDBCSharpDriver is the wide-column CQL store-backend class — high-write-throughput, shard-per-core, tunable-consistency; a distinct backend class from the relational PG/SQLite tier (`Npgsql`/`Microsoft.Data.Sqlite`), the ClickHouse columnar-OLAP lane (`ClickHouse.Driver`), and the dedicated vector store (`Qdrant.Client`).
- statements are prepared — `session.Prepare(cql)` + `Bind(values)` is the admitted form for parameterized queries, caching the prepared id server-side; `SimpleStatement` with positional/named values serves one-shot DDL/admin.
- domain entities go through the `Cassandra.Mapping` `Mapper` or the `Data.Linq` `Table<T>` — one mapping layer per entity family, never both for one POCO; the raw `BoundStatement` layer serves hot paths the mapper cannot express.
- `DriverException` folds to a typed `Fin`/`Validation` failure at the session boundary; `NoHostAvailableException`/`OperationTimedOutException`/`WriteTimeoutException`/`UnavailableException` carry the retry-relevant detail, and a thrown driver exception stays inside the boundary.
- `BatchType.Logged` is the atomic multi-partition batch (writing the batch log first); `Unlogged` is a performance batch with no atomicity guarantee, reserved for same-partition co-located writes.
- column encryption rides `AesColumnEncryptionPolicy` on the `Builder` for at-rest-sensitive columns; the AES key is KMS-sourced.

[RAIL_LAW]:
- Package: `ScyllaDBCSharpDriver`
- Owns: CQL wide-column store access — cluster/session lifecycle, prepared/bound/batch statement execution (sync + async), the `Cassandra.Mapping` POCO mapper, the `Cassandra.Data.Linq` IQueryable, shard/tablet/token-aware routing, the consistency/retry/reconnection/speculative-execution + execution-profile policy surface, `CqlVector<T>` ANN columns, and `IColumnEncryptionPolicy` client-side encryption
- Accept: a long-lived `Cluster`/`ISession` singleton; prepared `Bind`+`ExecuteAsync` for parameterized queries; the `Mapper`/`Table<T>` mapping layer for domain entities; `ConsistencyLevel`/retry as named execution-profile rows; `BatchType.Logged` for atomic multi-partition writes; `DriverException` folded into `Fin`
- Reject: inline string-interpolated CQL; a per-request `Cluster`/`ISession`; `Unlogged` batches treated as atomic; routing the relational/columnar-OLAP/dedicated-vector concerns through this driver; a `DriverException` crossing the receipt boundary; domain JSON through the transitive `Newtonsoft.Json`; the netstandard2.0 row API treated as a `Span`/`IAsyncEnumerable` row stream
