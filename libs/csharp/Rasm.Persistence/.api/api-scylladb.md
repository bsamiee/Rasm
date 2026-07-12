# [RASM_PERSISTENCE_API_SCYLLADB]

`ScyllaDBCSharpDriver` is the ScyllaDB fork of the DataStax C# driver: a CQL wide-column
client driving both ScyllaDB and Apache Cassandra over the one CQL binary protocol. The
surface is the `Cluster`/`Builder` fluent configuration root, the `ISession` execution rail
(sync `Execute` + `Task<RowSet>` `ExecuteAsync` mirror, `Prepare`/`PrepareAsync`), the
statement family (`SimpleStatement`/`PreparedStatement` -> `BoundStatement`/`BatchStatement`
with routing-key, idempotence, LWT, and per-statement consistency), the `Cassandra.Mapping`
POCO data-access layer (`Mapper` with full async mirror, `Cql` builder, `FetchPageAsync<T>`
paging, `InsertIfNotExistsAsync<T>` -> `AppliedInfo<T>` LWT), the `Cassandra.Data.Linq`
attribute-mapped `Table<T>` IQueryable (Linq2Cql), the `ExecutionProfiles` /
load-balancing / retry / reconnection / speculative-execution policy surface, ScyllaDB's
shard- and tablet-aware routing (`Tablet`/`HostShard`/`HostShardPair` metadata + the
`cluster.GetReplicas` shard readout), `CqlVector<T>` ANN vector
columns, and `IColumnEncryptionPolicy`/`AesColumnEncryptionPolicy` client-side encryption. It
is the scale-out wide-column backend class — no overlap with the relational PG/SQLite tier,
the columnar ClickHouse OLAP lane, or the `pgvector` ANN tier.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ScyllaDBCSharpDriver`
- package: `ScyllaDBCSharpDriver`
- license: Apache-2.0 (DataStax and ScyllaDB) — `github.com/scylladb/csharp-driver`
- assembly: `ScyllaDB` (note: assembly name `ScyllaDB.dll` differs from the package id `ScyllaDBCSharpDriver`)
- namespace: `Cassandra` (the public surface is the `Cassandra.*` namespace, NOT `ScyllaDB.*` — the fork preserves the DataStax type names); `Cassandra.Mapping`, `Cassandra.Data.Linq`, `Cassandra.Mapping.Attributes`, `Cassandra.ExecutionProfiles`, `Cassandra.Metrics.Abstractions`
- target: SINGLE-target `netstandard2.0` only (no `net*` lib in); the `net10.0` consumer binds `lib/netstandard2.0` — there is no higher TFM to prefer
- asset: pure-managed runtime library, AnyCPU, no native runtime
- abi: netstandard2.0 ABI — no `Span`/`ref struct` row API; rows are `Row`/`RowSet` reference types, async is `Task`-based (no `IAsyncEnumerable` row stream — paging is `IPage<T>` + `byte[] pagingState`)
- transitive boundary: pulls `K4os.Compression.LZ4` (the SAME pin Persistence admits for the LZ4 snapshot codec — the driver uses it for `CompressionType.LZ4` wire compression), `Newtonsoft.Json` (driver-internal; the workspace JSON rail stays STJ and never routes through it), `Microsoft.Extensions.Logging[.Abstractions]`, `System.Collections.Immutable`, `System.Threading.Tasks.Dataflow`, `System.Management`
- rail: store-backend (wide-column / CQL)

## [02]-[PUBLIC_TYPES]

[CONNECTION_TYPES]: cluster, session, configuration (namespace `Cassandra`)
- rail: store-backend

`Cluster` is the immutable connected-cluster root built by `Builder`; one `Cluster` opens many
`ISession`s. `ISession` is the execution + keyspace-DDL rail and is `IDisposable`.

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE]         | [CAPABILITY]                                                                                                                                                                                                                                                                                                             |
| :-----: | :----------------------------- | :--------------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Cluster`                      | connected-cluster root | static `Builder()`/`BuildFrom`; `Connect`/`Connect(keyspace)`/`ConnectAndCreateDefaultKeyspaceIfNotExists`; `Metadata`                                                                                                                                                                                                   |
|  [02]   | `Builder`                      | fluent configuration   | `AddContactPoints`, `WithCredentials`/`WithAuthProvider`, `WithSSL`, `WithLoadBalancingPolicy`/`WithRetryPolicy`/`WithReconnectionPolicy`/`WithSpeculativeExecutionPolicy`, `WithExecutionProfiles`, `WithColumnEncryptionPolicy`, `WithPoolingOptions`/`WithSocketOptions`, `WithCompression`, `WithMetrics`, `Build()` |
|  [03]   | `ISession`                     | execution rail         | `: IDisposable`; `Execute`/`ExecuteAsync`, `Prepare`/`PrepareAsync`, `ChangeKeyspace`, `CreateKeyspace[IfNotExists]`/`DeleteKeyspace[IfExists]`, `GetMetrics`, `ShutdownAsync`                                                                                                                                           |
|  [04]   | `ICluster`/`IInitializer`      | cluster contracts      | the `ISession`-factory contract / the `BuildFrom` initializer seam                                                                                                                                                                                                                                                       |
|  [05]   | `Configuration`                | resolved config        | the materialized cluster configuration (`Builder.Build` result)                                                                                                                                                                                                                                                          |
|  [06]   | `RowSet` / `Row`               | result set / row       | forward-iterable `IEnumerable<Row>`; `Row.GetValue<T>(name\|i)`, `RowSet.PagingState`, auto-paging                                                                                                                                                                                                                       |
|  [07]   | `ExecutionInfo` / `QueryTrace` | per-query telemetry    | achieved consistency, queried hosts, server-side trace events                                                                                                                                                                                                                                                            |

[STATEMENT_TYPES]: query statements (namespace `Cassandra`)
- rail: store-backend

`Statement` is the abstract root; `SimpleStatement`/`RegularStatement` carry inline CQL,
`PreparedStatement.Bind` mints a `BoundStatement`, and `BatchStatement` groups statements
atomically. Every statement carries routing key, consistency, idempotence, and page-size knobs.

| [INDEX] | [SYMBOL]            | [PACKAGE_ROLE]          | [CAPABILITY]                                                                                                                                                  |
| :-----: | :------------------ | :---------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------ |
|  [01]   | `SimpleStatement`   | inline CQL              | `: RegularStatement`; positional/named values, `Bind`, `SetRoutingKey`/`SetRoutingValues`                                                                     |
|  [02]   | `PreparedStatement` | prepared query          | `Bind(params object[]) -> BoundStatement`, `SetConsistencyLevel`, `SetRoutingKey`/`SetRoutingNames`, `SetIdempotence`, `SetLwt`, `Variables` (RowSetMetadata) |
|  [03]   | `BoundStatement`    | bound prepared          | a `PreparedStatement` bound with values; the executed unit                                                                                                    |
|  [04]   | `BatchStatement`    | atomic batch            | `: Statement`; `Add(Statement)`, `SetBatchType(BatchType)`, `SetRoutingKey`, `SetKeyspace`                                                                    |
|  [05]   | `RoutingKey`        | routing token           | the partition-key bytes the token-aware policy routes on (`PreparedStatement.RoutingKey` / `SetRoutingKey(params RoutingKey[])`)                              |
|  [06]   | `RowSetMetadata`    | bound-variable metadata | `PreparedStatement.Variables` — the prepared column/partition-key layout the routing key is derived from                                                      |

[MAPPING_TYPES]: POCO data access (namespace `Cassandra.Mapping`)
- rail: store-backend

The higher-level object mapper — POCO CRUD over CQL without hand-written statements. `Mapper`
is the `IMapper` implementation; `Cql` is the parameterized-query builder; `AppliedInfo<T>`
carries the LWT applied/not-applied verdict.

| [INDEX] | [SYMBOL]               | [PACKAGE_ROLE]    | [CAPABILITY]                                                                                                                                                                                                                                                            |
| :-----: | :--------------------- | :---------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Mapper`               | POCO mapper       | `: IMapper`; `Fetch<T>`/`FetchAsync<T>`, `FetchPageAsync<T>`, `Single[OrDefault]Async<T>`/`First[OrDefault]Async<T>`, `InsertAsync<T>`/`InsertIfNotExistsAsync<T>`, `UpdateAsync<T>`/`UpdateIfAsync<T>`, `DeleteAsync<T>`/`DeleteIfAsync<T>`, `ExecuteAsync(ICqlBatch)` |
|  [02]   | `Cql`                  | query builder     | static `New(cql, params object[] args)`; `WithOptions(Action<CqlQueryOptions>)`, `WithExecutionProfile`                                                                                                                                                                 |
|  [03]   | `CqlQueryOptions`      | per-query options | `SetConsistencyLevel`/`SetSerialConsistencyLevel` (the LWT level), `SetPageSize`, `SetPagingState`, `SetTimestamp`, `SetRetryPolicy` per mapped call                                                                                                                    |
|  [04]   | `MappingConfiguration` | mapping registry  | `Global`; registers `Map<T>` POCO->table definitions                                                                                                                                                                                                                    |
|  [05]   | `Map<T>` / `Mappings`  | fluent POCO map   | `TableName`, `PartitionKey`, `ClusteringKey`, `Column`, `CaseSensitive`                                                                                                                                                                                                 |
|  [06]   | `AppliedInfo<T>`       | LWT verdict       | `Applied` (bool) + the existing row when an `IF`-conditional was not applied                                                                                                                                                                                            |
|  [07]   | `ICqlBatch`            | mapped batch      | the public mapped-batch contract minted by `mapper.CreateBatch()` / `CreateBatch(BatchType)`; `mapper.ExecuteAsync(ICqlBatch)` runs it, `mapper.ExecuteConditionalAsync<T>(ICqlBatch)` runs an LWT batch -> `AppliedInfo<T>` (the concrete `CqlBatch` is internal)      |
|  [08]   | `IPage<T>`             | paged result      | `: ICollection<T>`; `PagingState`/`CurrentPagingState` (`byte[]`) for stateless cursor continuation (the concrete `Page<T>` is internal)                                                                                                                                |

[LINQ_TYPES]: attribute-mapped IQueryable (namespace `Cassandra.Data.Linq`)
- rail: store-backend

The LINQ-to-CQL leg: `Table<T>` is an `IQueryable<T>` whose LINQ expression compiles to CQL,
with attribute-driven schema (`[PartitionKey]`/`[ClusteringKey]`/`[Column]`/`[SecondaryIndex]`).

| [INDEX] | [SYMBOL]                                                                                                           | [PACKAGE_ROLE] | [CAPABILITY]                                                                                                                                             |
| :-----: | :----------------------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Table<TEntity>`                                                                                                   | LINQ table     | `: CqlQuery<TEntity>, ITable, IQueryProvider`; `Create()`/`CreateIfNotExists()`/`CreateAsync()`, LINQ `Where`/`Select`/`OrderBy` -> CQL                  |
|  [02]   | `CqlQuery<T>` / `CqlScalar<T>`                                                                                     | LINQ query     | `Execute()`/`ExecuteAsync()`, `AllowFiltering()`, `SetConsistencyLevel`, `SetPageSize`                                                                   |
|  [03]   | `CqlInsert<T>` / `CqlUpdate` / `CqlDelete`                                                                         | LINQ mutation  | `: CqlCommand`; fluent insert/update/delete with `IfNotExists`/`If`/`SetTTL`/`SetTimestamp` (`CqlInsert<T>` is generic; `CqlUpdate`/`CqlDelete` are not) |
|  [04]   | `CqlConditionalCommand<T>`                                                                                         | LWT command    | the `IF`-conditional mutation returning applied/existing                                                                                                 |
|  [05]   | `Batch`                                                                                                            | LINQ batch     | `Append(CqlCommand)`, `Execute()`/`ExecuteAsync()` atomic batch                                                                                          |
|  [06]   | `[PartitionKey]`/`[ClusteringKey]`/`[Column]`/`[SecondaryIndex]`/`[Counter]`/`[StaticColumn]`/`[Ignore]`/`[Table]` | attributes     | POCO->table schema (`Cassandra.Data.Linq` + `Cassandra.Mapping.Attributes` twins)                                                                        |

[POLICY_TYPES]: routing, retry, consistency, encryption (namespace `Cassandra` + `Cassandra.ExecutionProfiles`)
- rail: store-backend

The pluggable cluster behavior. ScyllaDB's shard-aware routing is the differentiator over the
Cassandra base — the public `HostShard`/`Tablet`/`HostShardPair` metadata lets the token-aware
policy route to the exact owning shard, not just the owning host (the `TabletMap` index that
holds them is driver-internal, surfaced only through `cluster.GetReplicas`).

| [INDEX] | [SYMBOL]                                                                                                                                     | [PACKAGE_ROLE]         | [CAPABILITY]                                                                                                                                                                                                                                          |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------- | :--------------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `TokenAwarePolicy`                                                                                                                           | routing policy         | wraps a child policy; routes to the replica owning the `RoutingKey`                                                                                                                                                                                   |
|  [02]   | `DefaultLoadBalancingPolicy` / `DCAwareRoundRobinPolicy` / `RoundRobinPolicy`                                                                | load balancing         | shard-aware default / DC-local / round-robin                                                                                                                                                                                                          |
|  [03]   | `Tablet` / `HostShard` / `HostShardPair`                                                                                                     | shard/tablet metadata  | ScyllaDB tablet ownership + per-shard routing target; `cluster.GetReplicas(keyspace, partitionKey) -> ICollection<HostShard>` is the public shard-resolution readout (`TabletMap` itself is internal — not a public surface)                          |
|  [04]   | `DefaultRetryPolicy` / `IdempotenceAwareRetryPolicy` / `LoggingRetryPolicy` / `DowngradingConsistencyRetryPolicy` / `FallthroughRetryPolicy` | retry policy           | typed `RetryDecision` per failure class                                                                                                                                                                                                               |
|  [05]   | `ConstantReconnectionPolicy` / `ExponentialReconnectionPolicy` / `FixedReconnectionPolicy`                                                   | reconnection           | node-down reconnect schedule                                                                                                                                                                                                                          |
|  [06]   | `ConstantSpeculativeExecutionPolicy` / `NoSpeculativeExecutionPolicy`                                                                        | speculative execution  | pre-emptive duplicate request on slow replica                                                                                                                                                                                                         |
|  [07]   | `IExecutionProfile` / `IExecutionProfileBuilder`                                                                                             | execution profile      | named bundle of consistency + timeout + LB + retry per query                                                                                                                                                                                          |
|  [08]   | `IColumnEncryptionPolicy` / `AesColumnEncryptionPolicy`                                                                                      | client-side encryption | per-column AES encrypt-before-write; `AesColumnEncryptionPolicy : BaseColumnEncryptionPolicy<AesColumnEncryptionPolicy.AesKeyAndIV>` and the key material is the NESTED `AesColumnEncryptionPolicy.AesKeyAndIV` value (not a top-level `AesKeyAndIV`) |
|  [09]   | `PlainTextAuthProvider` / `IAuthProvider`                                                                                                    | authentication         | username/password / pluggable auth                                                                                                                                                                                                                    |
|  [10]   | `AtomicMonotonicTimestampGenerator` / `ITimestampGenerator`                                                                                  | timestamps             | client-side monotonic write timestamps                                                                                                                                                                                                                |
|  [11]   | `PoolingOptions` / `SocketOptions` / `SSLOptions` / `QueryOptions`                                                                           | connection tuning      | per-host connection pool / socket / TLS / default query policy                                                                                                                                                                                        |

[TYPE_SYSTEM]: CQL value types + classification enums (namespace `Cassandra`)
- rail: store-backend

| [INDEX] | [SYMBOL]                                                                                         | [PACKAGE_ROLE]    | [CAPABILITY]                                                                                                                                                                                                                 |
| :-----: | :----------------------------------------------------------------------------------------------- | :---------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `CqlVector<T>`                                                                                   | ANN vector column | `: IReadOnlyCollection<T>`; `New(dimensions)`/`New(T[])`, `AsArray()` — the CQL `vector<float, n>` type                                                                                                                      |
|  [02]   | `LocalDate` / `LocalTime`                                                                        | CQL date/time     | the CQL `date`/`time` types (distinct from `DateTimeOffset` `timestamp`)                                                                                                                                                     |
|  [03]   | `UdtMap` / `UdtMappingDefinitions`                                                               | UDT mapping       | maps a CQL user-defined type to a CLR class                                                                                                                                                                                  |
|  [04]   | `TableMetadata` / `KeyspaceMetadata` / `MaterializedViewMetadata` / `IndexMetadata` / `Metadata` | schema reflection | live cluster schema introspection                                                                                                                                                                                            |
|  [05]   | `ConsistencyLevel`                                                                               | consistency enum  | `Any`/`One`/`Two`/`Three`/`Quorum`/`All`/`LocalQuorum`/`EachQuorum`/`Serial`/`LocalSerial`/`LocalOne`                                                                                                                        |
|  [06]   | `BatchType`                                                                                      | batch atomicity   | `Logged` (atomic via batch log) / `Unlogged` (no atomicity) / `Counter`                                                                                                                                                      |
|  [07]   | `CompressionType`                                                                                | wire compression  | `NoCompression` / `Snappy` / `LZ4` (LZ4 via the transitive `K4os.Compression.LZ4`)                                                                                                                                           |
|  [08]   | `HostDistance`                                                                                   | topology distance | `Local` / `Remote` / `Ignored` (the LB-policy host classification)                                                                                                                                                           |
|  [09]   | `DriverException` (+ family)                                                                     | typed faults      | `NoHostAvailableException`, `ReadTimeoutException`/`WriteTimeoutException`, `UnavailableException`, `OperationTimedOutException`, `InvalidQueryException`, `AlreadyExistsException` — the boundary the rail folds into `Fin` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: cluster construction + session open
- rail: store-backend

`Cluster.Builder()` -> fluent config -> `Build()` -> `Connect()` is the canonical bootstrap;
the `Builder` is the one configuration surface (policies, auth, SSL, pooling, execution
profiles, column encryption), never a parallel options bag. A `Cluster`/`ISession` is a
heavyweight long-lived singleton — one per backend, never per request.

| [INDEX] | [SURFACE]                                                                                                             | [CALL_SHAPE]   | [CAPABILITY]                                 |
| :-----: | :-------------------------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `Cluster.Builder()`                                                                                                   | static factory | starts the fluent configuration              |
|  [02]   | `Builder.AddContactPoints(params string[])` + `WithDefaultKeyspace(string)`                                           | config         | seed nodes + default keyspace                |
|  [03]   | `Builder.WithCredentials(user, pass)` / `WithSSL(SSLOptions)` / `WithColumnEncryptionPolicy(IColumnEncryptionPolicy)` | config         | auth / TLS / client-side encryption          |
|  [04]   | `Builder.WithLoadBalancingPolicy(new TokenAwarePolicy(new DefaultLoadBalancingPolicy(dc)))`                           | config         | shard/token-aware routing                    |
|  [05]   | `Builder.WithExecutionProfiles(Action<IExecutionProfileOptions>)`                                                     | config         | named consistency/timeout/retry bundles      |
|  [06]   | `Builder.Build()` -> `Cluster`                                                                                        | terminal       | the immutable connected-cluster root         |
|  [07]   | `cluster.Connect()` / `Connect(string keyspace)` -> `ISession`                                                        | open           | opens a session (optionally keyspace-scoped) |

[ENTRYPOINT_SCOPE]: statement execution — sync + async mirror
- rail: store-backend

`Prepare` once (cached cluster-side), `Bind` per call, `ExecuteAsync` the `BoundStatement`.
Every blocking `Execute`/`Prepare` has a `Task`-returning twin. Auto-paging walks `RowSet`
transparently; explicit paging uses `SetPageSize` + `PagingState`.

| [INDEX] | [SURFACE]                                                              | [RETURNS]                       | [CAPABILITY]                                                     |
| :-----: | :--------------------------------------------------------------------- | :------------------------------ | :--------------------------------------------------------------- |
|  [01]   | `session.Prepare(string cql)` / `PrepareAsync(string cql)`             | `PreparedStatement` / `Task<…>` | prepare (cached server-side by query id)                         |
|  [02]   | `prepared.Bind(params object[] values)` -> `BoundStatement`            | bind                            | binds values; carries the routing key from the prepared metadata |
|  [03]   | `bound.SetConsistencyLevel(ConsistencyLevel)` / `SetIdempotence(bool)` | statement tuning                | per-statement consistency + retry-safety                         |
|  [04]   | `session.Execute(IStatement)` / `ExecuteAsync(IStatement)`             | `RowSet` / `Task<RowSet>`       | execute (the async form is the canonical rail)                   |
|  [05]   | `session.Execute(IStatement, string executionProfileName)`             | `RowSet`                        | execute under a named execution profile                          |
|  [06]   | `new BatchStatement().Add(s1).Add(s2).SetBatchType(BatchType.Logged)`  | batch                           | atomic multi-statement batch                                     |
|  [07]   | `rowSet.PagingState` / `statement.SetPagingState(byte[])`              | paging                          | stateless cursor continuation across requests                    |

[ENTRYPOINT_SCOPE]: POCO mapping (`Mapper`) — async-first
- rail: store-backend

The object-mapper rail for domain entities: register the POCO once on `MappingConfiguration`,
then `Mapper` does CRUD over CQL with the same routing/consistency the statement layer applies.
LWT mutations return `AppliedInfo<T>`; paging returns `IPage<T>` + `PagingState`.

| [INDEX] | [SURFACE]                                                                   | [RETURNS]              | [CAPABILITY]                             |
| :-----: | :-------------------------------------------------------------------------- | :--------------------- | :--------------------------------------- |
|  [01]   | `new Mapper(ISession session[, MappingConfiguration config])`               | constructor            | the POCO data-access entry               |
|  [02]   | `mapper.FetchAsync<T>(Cql.New(cql, args))` / `FetchAsync<T>(string, args)`  | `Task<IEnumerable<T>>` | mapped query -> POCOs                    |
|  [03]   | `mapper.FetchPageAsync<T>(Cql.New(...).WithOptions(o => o.SetPageSize(n)))` | `Task<IPage<T>>`       | paged fetch with `PagingState`           |
|  [04]   | `mapper.SingleAsync<T>` / `FirstOrDefaultAsync<T>(Cql)`                     | `Task<T>`              | single-row mapped reads                  |
|  [05]   | `mapper.InsertAsync<T>(poco[, insertNulls, ttl])`                           | `Task`                 | mapped insert (optional null-skip + TTL) |
|  [06]   | `mapper.InsertIfNotExistsAsync<T>(poco)` / `UpdateIfAsync<T>(Cql)`          | `Task<AppliedInfo<T>>` | LWT conditional insert/update            |
|  [07]   | `mapper.DeleteAsync<T>(poco)` / `ExecuteAsync(ICqlBatch)`                   | `Task`                 | mapped delete / mapped atomic batch      |

[ENTRYPOINT_SCOPE]: LINQ-to-CQL (`Table<T>`)
- rail: store-backend

The attribute-mapped IQueryable leg — LINQ comprehensions compile to CQL with compile-time
schema from the POCO attributes. `AllowFiltering()` is the explicit opt-in to a non-indexed
scan; never implicit.

| [INDEX] | [SURFACE]                                                           | [RETURNS]              | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------------------------ | :--------------------- | :---------------------------------------------- |
|  [01]   | `new Table<T>(ISession session)` / `Create()` / `CreateAsync()`     | `Table<T>` / `Task`    | bind a LINQ table / DDL the table from the POCO |
|  [02]   | `table.Where(predicate).Select(projection)`                         | `CqlQuery<T>`          | LINQ expression -> CQL                          |
|  [03]   | `cqlQuery.ExecuteAsync()` / `.AllowFiltering()` / `.SetPageSize(n)` | `Task<IEnumerable<T>>` | execute / opt-in non-indexed scan / page size   |
|  [04]   | `table.Insert(poco).IfNotExists().SetTTL(n).ExecuteAsync()`         | `Task`                 | fluent conditional insert with TTL              |

## [04]-[IMPLEMENTATION_LAW]

[CONNECTION_PROFILE]:
- bootstrap: `Cluster.Builder().AddContactPoints(...).With*(...).Build()` -> `cluster.Connect(keyspace)`; `Cluster` and `ISession` are heavyweight, thread-safe, long-lived singletons — one `ISession` per backend, opened once and reused, never per request or per call.
- execution: the async `ExecuteAsync(IStatement)` -> `Task<RowSet>` is the canonical rail; `Prepare` once and `Bind` per call so the query is parsed server-side once and the routing key is derived from the prepared metadata.
- routing: `Builder.WithLoadBalancingPolicy(new TokenAwarePolicy(new DefaultLoadBalancingPolicy(localDc)))` is the canonical policy — token + shard aware, so a `BoundStatement` whose routing key is known reaches the owning replica's owning shard directly; the ScyllaDB `Tablet`/`HostShard`/`HostShardPair` metadata is what makes the shard target precise, and `cluster.GetReplicas(keyspace, partitionKey)` is the public shard-resolution readout.
- consistency: `ConsistencyLevel` is per-statement (`SetConsistencyLevel`) or per-execution-profile (`WithExecutionProfiles`); `Serial`/`LocalSerial` are the LWT (lightweight-transaction) consistency for `IF`-conditional statements, distinct from the quorum levels for normal reads/writes.
- idempotence: `SetIdempotence(true)` marks a statement retry-safe so the retry/speculative-execution policy may re-issue it; non-idempotent statements are never speculatively duplicated.

[LOCAL_ADMISSION]:
- ScyllaDBCSharpDriver is the wide-column CQL store-backend class — the high-write-throughput, shard-per-core, tunable-consistency lane; it does NOT overlap the relational PG/SQLite tier (`Npgsql`/`Microsoft.Data.Sqlite`), the ClickHouse columnar-OLAP lane (`ClickHouse.Driver`), or the dedicated vector store (`Qdrant.Client`).
- statements are PREPARED, never inline string interpolation — `session.Prepare(cql)` + `Bind(values)` is the only admitted form for parameterized queries, so CQL injection is structurally impossible and the prepared id is cached server-side; `SimpleStatement` with positional/named values is for one-shot DDL/admin only.
- domain entities go through the `Cassandra.Mapping` `Mapper` (or the `Data.Linq` `Table<T>`) — one mapping layer chosen per entity family, never both for the same POCO; the raw `BoundStatement` layer is reserved for hot paths the mapper cannot express.
- the `DriverException` family is mapped to a typed `Fin`/`Validation` failure at the session boundary — `NoHostAvailableException`/`OperationTimedOutException`/`WriteTimeoutException`/`UnavailableException` carry the retry-relevant detail; a thrown driver exception never crosses the receipt path.
- `BatchType.Logged` is the atomic multi-partition batch (writes to the batch log first); `Unlogged` is a performance batch with NO atomicity guarantee and is used only for same-partition co-located writes — the two are never conflated.
- column encryption (`AesColumnEncryptionPolicy` with `AesKeyAndIV`) is configured on the `Builder` for at-rest-sensitive columns; the AES key material is sourced from the KMS rail, never inlined.

[STACKING_LAW]:
- consistency/retry as policy rows: the `ExecutionProfiles` + retry/LB/speculative-execution policies are declared once on the `Builder` and selected per query by name — the consistency, timeout, retry, and routing variation lives in a policy row / named profile, never in parallel session objects or per-call branching.
- compression boundary: `CompressionType.LZ4` uses the transitive `K4os.Compression.LZ4` — the SAME pin the LZ4 snapshot codec (`api-lz4`) admits — for CQL frame compression; this is wire transport compression on the driver, entirely distinct from the `CompressionPolicy` snapshot/blob codec axis, and the two never interact.
- encryption + KMS: `AesColumnEncryptionPolicy`'s `AesKeyAndIV` key is provisioned through the KMS rail (`api-aws-kms`/`api-azure-keyvault`/`api-google-kms`) — the driver encrypts the column client-side before write, the KMS owns the key lifecycle; the encryption policy is the consumer of a KMS-issued data key, never a key store itself.
- vector lane separation: `CqlVector<T>` + `VectorColumnInfo` give ScyllaDB native `vector<float, n>` columns for co-located ANN beside the wide-column data, but the dedicated billion-scale ANN store remains `Qdrant.Client` and the in-PG ANN tier remains `pgvector`/`pgvectorscale` — a `CqlVector` column is the embedding-next-to-the-row case, not the primary vector index.
- JSON boundary: the driver's transitive `Newtonsoft.Json` is driver-internal (schema/payload handling) — the Persistence JSON rail stays `System.Text.Json` (NodaTime STJ / Thinktecture STJ converters), and no domain code routes through `Newtonsoft.Json`.
- observability: `Builder.WithMetrics(IDriverMetricsProvider)` and `WithRequestTracker(IRequestTracker)` feed the AppHost telemetry port; `ExecutionInfo`/`QueryTrace` per-`RowSet` carry the achieved consistency and queried-host detail folded into the query receipt.

[RAIL_LAW]:
- Package: `ScyllaDBCSharpDriver`
- Owns: CQL wide-column store access — cluster/session lifecycle, prepared/bound/batch statement execution (sync + async), the `Cassandra.Mapping` POCO mapper, the `Cassandra.Data.Linq` IQueryable, shard/tablet/token-aware routing, the consistency/retry/reconnection/speculative-execution + execution-profile policy surface, `CqlVector<T>` ANN columns, and `IColumnEncryptionPolicy` client-side encryption
- Accept: a long-lived `Cluster`/`ISession` singleton; prepared `Bind`+`ExecuteAsync` for parameterized queries; the `Mapper`/`Table<T>` mapping layer for domain entities; `ConsistencyLevel`/retry as named execution-profile rows; `BatchType.Logged` for atomic multi-partition writes; `DriverException` folded into `Fin`
- Reject: inline string-interpolated CQL (use prepared statements); a per-request `Cluster`/`ISession`; conflating `Unlogged` batches with atomicity; routing through this driver for the relational/columnar-OLAP/dedicated-vector concerns those backends own; a thrown `DriverException` crossing the receipt boundary; routing domain JSON through the transitive `Newtonsoft.Json`; treating the netstandard2.0 row API as if it offered a `Span`/`IAsyncEnumerable` row stream
