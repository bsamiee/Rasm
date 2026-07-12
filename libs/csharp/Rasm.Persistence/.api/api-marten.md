# [RASM_PERSISTENCE_API_MARTEN]

`Marten` turns PostgreSQL into both a document database and an ACID event store
over the one `NpgsqlDataSource`. It is the append substrate beneath the
`Version/` engine: each model (or spatial partition) is ONE event stream whose
event body is a `GraphDelta`, started with `StartStream` and grown with
`Append`/`AppendOptimistic`/`AppendExclusive`; the whole `ElementGraph` rehydrates
by folding the stream through `AggregateStreamAsync<T>`, and AS-OF time-travel is
the same call with a `version` or `timestamp` bound. The bespoke
op-log/CRDT/time-travel/`StructuralMerge`/causal-DAG read layer PROJECTS from these
events — Marten owns the durable append + the rebuildable read views, not the
merge semantics.

Marten 9's read-your-writes topology rides a `SingleStreamProjection<TDoc,TId>`
registered `ProjectionLifecycle.Inline` (the projected document is written in the
SAME `IDocumentSession` transaction as the events, so authoritative containment is
never stale); cross-model aggregates use `MultiStreamProjection<TDoc,TId>` with
`Identity`/`Identities`/`CustomGrouping`/`FanOut` slicing; the analytical lanes
(`Apache AGE` topology, `DuckDB`/`Ara3D.BimOpenSchema` columnar) are
`ProjectionLifecycle.Async` views driven off the daemon, and the
`ElementGraph→BimOpenSchema` egress is a co-transactional `FlatTableProjection`.
The relational identity row (`ElementIdentity` via `Npgsql`/EF — `api-npgsql-ef`)
commits in the same Postgres transaction by storing it as a Marten document in the
one `IDocumentSession`, so identity + event are atomic with no two-ORM dance.
Serialization is `System.Text.Json` (Thinktecture-JSON `[ValueObject]`/`[SmartEnum]`
converters apply — `api-thinktecture-json`); `NodaTime` instants ride the event
`Timestamp`.

Marten 9's database-agnostic event surface — `Append`/`StartStream`,
`FetchStreamAsync`, `AggregateStreamAsync`, `FetchForWriting`/`WriteToAggregate`,
`FetchLatest`/`ProjectLatest`, the tag-query family, and the projection/daemon
contracts — lives in the transitive `JasperFx.Events` assembly; the Marten
assembly adds the Postgres binding, document store, and JSON-passthrough extras.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Marten`
- package: `Marten`
- license: MIT (© Jeremy D. Miller + JasperFx contributors)
- assembly: `Marten`
- transitive surface: `JasperFx.Events` (database-agnostic event store), `JasperFx` (`TenancyStyle`, `AutoCreate`, `DaemonMode`), `Weasel.Postgresql`/`Weasel.Core` (DDL/schema diff)
- namespace: `Marten`, `Marten.Events`, `Marten.Events.Aggregation`, `Marten.Events.Projections`, `Marten.Events.Projections.Flattened`, `Marten.Events.Daemon`, `Microsoft.Extensions.DependencyInjection`; transitive `JasperFx.Events`, `JasperFx.Events.Projections`, `JasperFx.Events.Daemon`, `JasperFx.Events.Aggregation`, `JasperFx.MultiTenancy`
- target framework: `net10.0` asset binds the `net10.0` consumer (package also ships `net9.0`)
- asset: managed runtime library over `Npgsql`/`Npgsql.DataSource`; ships `analyzers/dotnet/cs/JasperFx.Events.SourceGenerator.dll` (compile-time projection/aggregation source generator, enabled via `TryUseSourceGeneratedDiscovery`/`UseSourceGeneratedDiscovery`)
- rail: event-store

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: configuration root and document store
- rail: event-store

| [INDEX] | [SYMBOL]                        | [SHAPE]                    | [CAPABILITY]                                                                                                                                               |
| :-----: | :------------------------------ | :------------------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `DocumentStore`                 | sealed class               | the store: `For(...)` factory + session openers; `IDocumentStore` impl                                                                                     |
|  [02]   | `IDocumentStore`                | interface                  | session factory (`LightweightSession`/`IdentitySession`/`DirtyTrackedSession`/`QuerySession`), `Advanced`, `BulkInsertAsync`, `BuildProjectionDaemonAsync` |
|  [03]   | `StoreOptions`                  | class                      | the configuration surface: `Connection`, `DatabaseSchemaName`, `Events`, `Projections`, `Schema`, `Policies`, serializer, tenancy                          |
|  [04]   | `IReadOnlyStoreOptions`         | interface                  | the read-only projection of a configured `StoreOptions`                                                                                                    |
|  [05]   | `SessionOptions`                | class                      | per-session tenancy/isolation/tracking/connection overrides                                                                                                |
|  [06]   | `AdvancedOperations`            | class (`store.Advanced`)   | `ResetAllData`, `FetchEventStoreStatistics`, `AllProjectionProgress`, `RebuildSingleStreamAsync`, `Clean`                                                  |
|  [07]   | `ProjectionOptions`             | class (`opts.Projections`) | projection registry: `Add`, `Snapshot<T>`, `LiveStreamAggregation<T>`, `Subscribe`, `AddGlobalProjection`                                                  |
|  [08]   | `AsyncOptions`                  | class                      | async-projection tuning: `BatchSize`, `SubscribeFromPresent`/`FromTime`/`FromSequence`, teardown                                                           |
|  [09]   | `MartenConfigurationExpression` | class (DI)                 | `AddMarten(...)` fluent: `AddAsyncDaemon`, `UseLightweightSessions`, `ApplyAllDatabaseChangesOnStartup`, projection-with-services                          |

[PUBLIC_TYPE_SCOPE]: sessions and document operations
- rail: event-store

| [INDEX] | [SYMBOL]              | [SHAPE]   | [CAPABILITY]                                                                                                                        |
| :-----: | :-------------------- | :-------- | :---------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `IQuerySession`       | interface | read side: `LoadAsync`/`LoadManyAsync`, `Query<T>` (LINQ), raw `QueryAsync<T>(sql)`, `Events` (read), full-text search, `ForTenant` |
|  [02]   | `IDocumentSession`    | interface | write side: `Store`/`Insert`/`Update`/`Delete`, `Events` (write), `SaveChangesAsync`, `SetHeader`, transaction control              |
|  [03]   | `IDocumentOperations` | interface | the document mutation contract shared by sessions + projection apply (`Store`/`Insert`/`Delete`/`HardDelete`/`QueueSqlCommand`)     |
|  [04]   | `IMartenQueryable<T>` | interface | the `IQueryable<T>` Marten LINQ provider (async materializers, `Include`, paging, `QueryForNonStaleData`)                           |
|  [05]   | `DocumentMetadata`    | class     | per-document metadata (version, last-modified, tenant, deleted)                                                                     |
|  [06]   | `IUnitOfWork`         | interface | `session.PendingChanges` — the staged inserts/updates/deletes/events                                                                |
|  [07]   | `ConcurrencyChecks`   | enum      | `Enabled`/`Disabled` optimistic-concurrency mode on a session                                                                       |

[PUBLIC_TYPE_SCOPE]: event store — read and write (`JasperFx.Events`)
- rail: event-store

| [INDEX] | [SYMBOL]                              | [SHAPE]   | [CAPABILITY]                                                                                                                                                                         |
| :-----: | :------------------------------------ | :-------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `IEventOperations`                    | interface | the write contract: `StartStream`/`Append` overloads (Guid/string key, typed-aggregate, expected-version)                                                                            |
|  [02]   | `IQueryEventStore`                    | interface | the read contract: `FetchStreamAsync`, `AggregateStreamAsync<T>` (AS-OF), `AggregateStreamToLastKnownAsync`, `FetchStreamStateAsync`, event `LoadAsync`                              |
|  [03]   | `IEventStoreOperations`               | interface | the full session event store: read + write + `FetchForWriting`/`WriteToAggregate`, `AppendOptimistic`/`AppendExclusive`, `FetchLatest`/`ProjectLatest`, tag queries, `ArchiveStream` |
|  [04]   | `Marten.Events.IEventStoreOperations` | interface | Marten extension of the above: adds `StreamLatestJson<T>` (raw-JSON passthrough to a `Stream`)                                                                                       |
|  [05]   | `IEventStream<T>`                     | interface | the optimistic-write handle from `FetchForWriting<T>` (current `Aggregate`, `AppendOne`/`AppendMany`)                                                                                |
|  [06]   | `StreamAction`                        | class     | the staged stream operation returned by `StartStream`/`Append` (`Id`/`Key`, `Events`, `Version`, `ExpectedVersionOnServer`, `ActionType`)                                            |
|  [07]   | `StreamActionType`                    | enum      | `Start`/`Append`/`Archive` — the stream-action discriminant                                                                                                                          |
|  [08]   | `StreamState`                         | class     | `FetchStreamStateAsync` result: `Id`/`Key`, `Version`, `AggregateType`, `LastTimestamp`, `Created`, `IsArchived`                                                                     |
|  [09]   | `IEvent` / `IEvent<T>`                | interface | the stored-event envelope: `Sequence`/`Version`/`StreamId`/`StreamKey`, `Timestamp`, `Data`, `Headers`, `CausationId`/`CorrelationId`, `Tags`, `IsArchived`                          |
|  [10]   | `EventTag` / `EventTagQuery`          | class     | event tag value + the tag-query shape for `QueryByTagsAsync`/`AggregateByTagsAsync`/`FetchForWritingByTags`                                                                          |

[PUBLIC_TYPE_SCOPE]: projections and the async daemon
- rail: event-store

| [INDEX] | [SYMBOL]                                | [SHAPE]           | [CAPABILITY]                                                                                                                                                                                                                                  |
| :-----: | :-------------------------------------- | :---------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `SingleStreamProjection<TDoc,TId>`      | abstract base     | one-stream aggregate: convention `Create`/`Apply`/`Evolve`/`DeleteEvent`/`ShouldDelete` over one stream → one document                                                                                                                        |
|  [02]   | `MultiStreamProjection<TDoc,TId>`       | abstract base     | cross-stream aggregate: `Identity`/`Identities`/`CustomGrouping`/`FanOut` slicing → grouped documents                                                                                                                                         |
|  [03]   | `FlatTableProjection`                   | class             | events → flat relational table (`Project<T>(StatementMap)`/`Delete<T>`) — the co-transactional columnar egress                                                                                                                                |
|  [04]   | `ProjectionBase`                        | abstract base     | shared projection config (name, version, async options, `AssembleAndAssertValidity`)                                                                                                                                                          |
|  [05]   | `IProjection`                           | interface         | the raw Marten projection contract (`ApplyAsync(ops, streams, ct)`)                                                                                                                                                                           |
|  [06]   | `ProjectionLifecycle`                   | enum              | `Inline` (write-txn), `Async` (daemon), `Live` (on-demand fold)                                                                                                                                                                               |
|  [07]   | `SnapshotLifecycle`                     | enum              | `Inline`/`Async` — the snapshot variant of lifecycle                                                                                                                                                                                          |
|  [08]   | `IProjectionDaemon`                     | interface         | the async-projection runner: `StartAllAsync`, `RebuildProjectionAsync`, `WaitForNonStaleData`, agent/shard control                                                                                                                            |
|  [09]   | `DaemonMode`                            | enum (`JasperFx`) | `Disabled`/`Solo`/`HotCold` — daemon hosting topology                                                                                                                                                                                         |
|  [10]   | `ShardState` / `ShardName`              | class             | per-shard progress: `ShardState.Sequence` (`long` high-water) / `.Timestamp` (`DateTimeOffset` recording stamp) / `.ShardName` (`string`); `ShardName.Identity` the shard identity string (decompile-verified, `JasperFx.Events.Projections`) |
|  [11]   | `EventStoreStatistics`                  | class             | `FetchEventStoreStatistics` result (event/stream counts, sequence + projection high-water marks)                                                                                                                                              |
|  [12]   | `IEventUpcaster` / `JsonTransformation` | interface         | schema-evolution upcast of old event JSON to a new event type                                                                                                                                                                                 |

[PUBLIC_TYPE_SCOPE]: event subscriptions (the daemon changefeed lift)
- rail: event-store

| [INDEX] | [SYMBOL]                                         | [SHAPE]           | [CAPABILITY]                                                                                                                                                                                                                                                                                                                                |
| :-----: | :----------------------------------------------- | :---------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
|  [01]   | `Marten.Subscriptions.SubscriptionBase`          | abstract base     | the subscription base (`: JasperFxSubscriptionBase<IDocumentOperations,IQuerySession,ISubscription>, ISubscription`): override `public abstract Task<IChangeListener> ProcessEventsAsync(EventRange page, ISubscriptionController controller, IDocumentOperations operations, CancellationToken)` to lift each daemon-delivered event batch |
|  [02]   | `JasperFx.Events.Projections.EventRange`         | class             | the delivered batch: `List<IEvent> Events`, `ShardName`, `SequenceFloor`/`SequenceCeiling`, `Size` — iterate `range.Events.OfType<IEvent<T>>()` for the typed bodies                                                                                                                                                                        |
|  [03]   | `JasperFx.Events.Daemon.ISubscriptionController` | interface         | the shard control handed to the batch: `Mode`, `Name` (`ShardName`), `Options` (`AsyncOptions`), `MarkSuccessAsync(long)`, `ReportCriticalFailureAsync(Exception[, long])`, dead-letter recording                                                                                                                                           |
|  [04]   | `IChangeListener` / `NullChangeListener`         | interface / class | the post-batch commit hook (`BeforeCommitAsync`/`AfterCommitAsync(IDocumentSession, IChangeSet, CancellationToken)`); a no-side-effect subscription returns the singleton `NullChangeListener.Instance`                                                                                                                                     |

[PUBLIC_TYPE_SCOPE]: event configuration and tenancy/identity vocabulary
- rail: event-store

| [INDEX] | [SYMBOL]             | [SHAPE]                   | [CAPABILITY]                                                                                                              |
| :-----: | :------------------- | :------------------------ | :------------------------------------------------------------------------------------------------------------------------ |
|  [01]   | `IEventStoreOptions` | interface (`opts.Events`) | the event config surface: `StreamIdentity`, `AppendMode`, `TenancyStyle`, `MetadataConfig`, event-type mapping, upcasting |
|  [02]   | `StreamIdentity`     | enum                      | `AsGuid` / `AsString` — stream-key type (per-model GUID vs string partition key)                                          |
|  [03]   | `EventAppendMode`    | enum                      | `Rich` (full metadata), `Quick`, `QuickWithServerTimestamps` — append-throughput tier                                     |
|  [04]   | `TenancyStyle`       | enum (`JasperFx`)         | `Single` / `Conjoined` — single-DB column-tenancy vs separate tenant databases                                            |
|  [05]   | `MetadataConfig`     | class                     | which metadata columns (causation/correlation/headers/user) the event table carries                                       |
|  [06]   | `AutoCreate`         | enum (`JasperFx`)         | `None`/`CreateOnly`/`CreateOrUpdate`/`All` — schema auto-migration policy                                                 |
|  [07]   | `MartenRegistry`     | class (`opts.Schema`)     | per-document mapping (`For<T>()`: identity, indexes, multi-tenancy, soft-delete)                                          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: configure the store and open sessions
- rail: event-store
- surface-root: `DocumentStore` / `StoreOptions` / `IDocumentStore`

| [INDEX] | [SURFACE]                                                                                                                                        | [CAPABILITY]                                             |
| :-----: | :----------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `DocumentStore.For(Action<StoreOptions> configure)` / `For(string connectionString)` / `For<T>() where T : StoreOptions, new()`                  | build a configured store                                 |
|  [02]   | `StoreOptions.Connection(string)` / `Connection(NpgsqlDataSource)` / `DataSourceFactory(INpgsqlDataSourceFactory, string?)`                      | bind the Postgres connection / data source               |
|  [03]   | `StoreOptions.DatabaseSchemaName` / `AutoCreateSchemaObjects (AutoCreate)` / `ConfigureNpgsqlDataSourceBuilder(Action<NpgsqlDataSourceBuilder>)` | schema + migration + Npgsql tuning                       |
|  [04]   | `StoreOptions.UseSystemTextJsonForSerialization([JsonSerializerOptions?], EnumStorage, Casing, Action<JsonSerializerOptions>?)`                  | STJ serializer (Thinktecture converters apply)           |
|  [05]   | `StoreOptions.Schema.For<T>()` / `RegisterDocumentType<T>()` / `Policies.*` / `RegisterValueType<T>()`                                           | document mapping, policies, strong-typed-id registration |
|  [06]   | `IDocumentStore.LightweightSession([SessionOptions]\|[tenantId]\|[IsolationLevel])` / `IdentitySession(...)` / `DirtyTrackedSession(...)`        | open a write session by tracking mode                    |
|  [07]   | `IDocumentStore.QuerySession([SessionOptions]\|[tenantId])` / `*SerializableSessionAsync(...)`                                                   | open a read session / serializable-isolation session     |
|  [08]   | `IDocumentStore.Advanced.ResetAllData(ct)` / `FetchEventStoreStatistics([tenantId], ct)` / `AllProjectionProgress([tenantId], ct)`               | teardown + event-store + projection introspection        |

[ENTRYPOINT_SCOPE]: append and start streams (`session.Events`)
- rail: event-store
- surface-root: `IEventStoreOperations` (`session.Events`)

| [INDEX] | [SURFACE]                                                                                                                                                          | [CAPABILITY]                                                 |
| :-----: | :----------------------------------------------------------------------------------------------------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `Events.StartStream<TAggregate>(Guid id, params object[] events)` / `StartStream(Guid id, IEnumerable<object>)` / `StartStream<TAggregate>(string streamKey, ...)` | begin a new stream (GUID per model, or string partition key) |
|  [02]   | `Events.Append(Guid stream, params object[] events)` / `Append(string stream, IEnumerable<object>)`                                                                | append to an existing stream                                 |
|  [03]   | `Events.Append(Guid stream, long expectedVersion, params object[] events)`                                                                                         | append with inline optimistic version guard                  |
|  [04]   | `Events.AppendOptimistic(Guid streamId, [CancellationToken], params object[] events)`                                                                              | append with a read-then-guard optimistic check               |
|  [05]   | `Events.AppendExclusive(Guid streamId, [CancellationToken], params object[] events)`                                                                               | append under an exclusive stream lock                        |
|  [06]   | `Events.ArchiveStream(Guid streamId)` / `ArchiveStream(string streamKey)`                                                                                          | soft-archive a whole stream (Retired)                        |
|  [07]   | `session.SaveChangesAsync([CancellationToken])`                                                                                                                    | commit staged events + documents in one txn                  |

[ENTRYPOINT_SCOPE]: read and fold streams (AS-OF time-travel)
- rail: event-store
- surface-root: `IQueryEventStore` (`session.Events`)

| [INDEX] | [SURFACE]                                                                                                                                                                     | [CAPABILITY]                                                            |
| :-----: | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :---------------------------------------------------------------------- |
|  [01]   | `Events.AggregateStreamAsync<T>(Guid streamId, long version = 0, DateTimeOffset? timestamp = null, T? state = null, long fromVersion = 0, CancellationToken token = default)` | fold a stream into `T`; AS-OF by `version` or `timestamp`               |
|  [02]   | `Events.AggregateStreamAsync<T>(string streamKey, ...)` / `AggregateStreamToLastKnownAsync<T>(...)`                                                                           | string-key fold / fold ignoring an archive cut                          |
|  [03]   | `Events.FetchStreamAsync(Guid streamId, long version = 0, DateTimeOffset? timestamp = null, long fromVersion = 0, CancellationToken token = default)`                         | read the raw `IEvent` list (full or windowed)                           |
|  [04]   | `Events.FetchStreamStateAsync(Guid streamId, [CancellationToken])`                                                                                                            | current `StreamState` (version/timestamps/archive) without folding      |
|  [05]   | `Events.LoadAsync<T>(Guid id, [CancellationToken])` / `LoadAsync(Guid id, ...)`                                                                                               | load a single typed / untyped stored event by id                        |
|  [06]   | `Events.StreamLatestJson<T>(Guid id, Stream destination, [CancellationToken])`                                                                                                | stream the projected aggregate's raw JSON (no (de)serialize round-trip) |

[ENTRYPOINT_SCOPE]: fetch-for-writing and aggregate-handler workflow
- rail: event-store
- surface-root: `IEventStoreOperations` (`session.Events`)

| [INDEX] | [SURFACE]                                                                                                                                                  | [CAPABILITY]                                          |
| :-----: | :--------------------------------------------------------------------------------------------------------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `Events.FetchForWriting<T>(Guid id, [long expectedVersion], [CancellationToken])` / `FetchForWriting<T,TId>(TId id, ...)`                                  | load aggregate + version handle for a guarded append  |
|  [02]   | `Events.FetchForExclusiveWriting<T>(Guid id, [CancellationToken])`                                                                                         | same under an exclusive lock                          |
|  [03]   | `Events.WriteToAggregate<T>(Guid id, Func<IEventStream<T>, Task> writing, [CancellationToken])` / `WriteToAggregate<T>(id, int expectedVersion, ...)`      | load-decide-append in one closure                     |
|  [04]   | `Events.WriteExclusivelyToAggregate<T>(Guid id, Action<IEventStream<T>>, [CancellationToken])`                                                             | the exclusive-lock variant                            |
|  [05]   | `Events.FetchLatest<T>(Guid id, [CancellationToken])` / `FetchLatest<T,TId>(TId id, ...)`                                                                  | the up-to-date aggregate (inline doc or live fold)    |
|  [06]   | `Events.ProjectLatest<T>(Guid id, [CancellationToken])`                                                                                                    | force a live projection of the latest aggregate state |
|  [07]   | `Events.QueryByTagsAsync(EventTagQuery, [CancellationToken])` / `AggregateByTagsAsync<T>(...)` / `FetchForWritingByTags<T>(...)` / `EventsExistAsync(...)` | tag-scoped read / fold / write / existence            |

[ENTRYPOINT_SCOPE]: register projections and run the daemon
- rail: event-store
- surface-root: `StoreOptions.Projections` / `IProjectionDaemon`

| [INDEX] | [SURFACE]                                                                                                                                                     | [CAPABILITY]                                                                                         |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------------ | :--------------------------------------------------------------------------------------------------- |
|  [01]   | `opts.Projections.Add<TProjection>(ProjectionLifecycle lifecycle, Action<AsyncOptions>? = null)` / `Add(TProjection projection, lifecycle, ...)`              | register a projection by type / instance                                                             |
|  [02]   | `opts.Projections.Snapshot<T>(SnapshotLifecycle lifecycle, [Action<AsyncOptions>?])` / `LiveStreamAggregation<T>([Action<AsyncOptions>?])`                    | self-aggregate snapshot / live-only aggregation                                                      |
|  [03]   | `opts.Projections.AddGlobalProjection<TDoc,TId>(SingleStreamProjection<TDoc,TId>, ProjectionLifecycle)` / `Subscribe(ISubscription, [Action<AsyncOptions>?])` | global single-stream registration / register a `SubscriptionBase`-derived subscription on the daemon |
|  [04]   | `MultiStreamProjection<TDoc,TId>.Identity<TEvent>(Func<TEvent,TId>)` / `Identities<TEvent>(...)` / `CustomGrouping(...)` / `FanOut<TEvent,TChild>(...)`       | configure cross-stream slicing                                                                       |
|  [05]   | `FlatTableProjection.Project<T>(Action<StatementMap<T>>, [Expression<Func<T,object>>? pk])` / `Delete<T>([pk])`                                               | events → relational table upsert / delete rows                                                       |
|  [06]   | `await store.BuildProjectionDaemonAsync([tenantIdOrDatabase], [ILogger])`                                                                                     | construct the async-projection daemon                                                                |
|  [07]   | `daemon.StartAllAsync()` / `RebuildProjectionAsync<TView>([TimeSpan shardTimeout], CancellationToken)` / `WaitForNonStaleData(TimeSpan)` / `StopAllAsync()`   | start / rebuild / await-caught-up / stop shards                                                      |
|  [08]   | `store.Advanced.RebuildSingleStreamAsync<T>(Guid id, [CancellationToken])`                                                                                    | rebuild a single stream's inline projection                                                          |

[ENTRYPOINT_SCOPE]: DI registration and host integration
- rail: event-store
- surface-root: `MartenServiceCollectionExtensions`

| [INDEX] | [SURFACE]                                                                                                                                              | [CAPABILITY]                                                                                                          |
| :-----: | :----------------------------------------------------------------------------------------------------------------------------------------------------- | :-------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `services.AddMarten(Action<StoreOptions>)` / `AddMarten(Func<IServiceProvider,StoreOptions>)` / `AddMarten(string connectionString)`                   | register the store + sessions in DI                                                                                   |
|  [02]   | `.AddAsyncDaemon(DaemonMode mode)` / `.UseLightweightSessions()` / `.UseIdentitySessions()` / `.UseDirtyTrackedSessions()`                             | host the daemon / pick the default session tracking                                                                   |
|  [03]   | `.ApplyAllDatabaseChangesOnStartup()` / `.AssertDatabaseMatchesConfigurationOnStartup()`                                                               | migrate / assert schema at host startup                                                                               |
|  [04]   | `IMartenStorage.ApplyAllConfiguredChangesToDatabaseAsync()` (via `store.Storage`)                                                                      | the runtime single-writer schema apply — the `Element/identity#SCHEMA_VERDICT` `AdmitMarten` leg (decompile-verified) |
|  [05]   | `.AddProjectionWithServices<TProjection>(ProjectionLifecycle, ServiceLifetime, [Action<ProjectionBase>?])`                                             | register a DI-injected projection                                                                                     |
|  [06]   | `services.AddMartenStore<T>(Action<StoreOptions>) where T : IDocumentStore` / `ConfigureMarten(...)`                                                   | register a separate ancillary store / extend config                                                                   |
|  [07]   | `host.WaitForNonStaleProjectionDataAsync(TimeSpan)` / `store.WaitForNonStaleProjectionDataAsync([tenantIdOrDatabaseName], TimeSpan)` (`Marten.Events`) | block until async projections are caught up                                                                           |

## [04]-[IMPLEMENTATION_LAW]

[EVENT_SUBSTRATE]:
- Stream grain is ONE stream PER MODEL (or per spatial partition), NEVER per `NodeId`; the event body is the `GraphDelta`, not a whole-graph snapshot. Pick `StreamIdentity.AsString` when the partition key is composite, `AsGuid` for a model GUID. `StartStream` opens the stream; `Append`/`AppendOptimistic`/`AppendExclusive` extend it; `SaveChangesAsync` commits events + documents in one Postgres transaction. The whole `ElementGraph` rehydrates by folding the stream through `AggregateStreamAsync<T>` — zero-loss because the deltas replay deterministically.
- Optimistic concurrency is `Append(stream, expectedVersion, …)` (inline guard), `AppendOptimistic` (read-then-guard), or `FetchForWriting<T>(id, expectedVersion)` returning an `IEventStream<T>` handle; `AppendExclusive`/`FetchForExclusiveWriting` take a stream-level advisory lock for multi-writer-hostile sections. `EventAppendMode.Quick`/`QuickWithServerTimestamps` trade event metadata richness for append throughput on bulk ingest.

[AGGREGATION]:
- A `SingleStreamProjection<TDoc,TId>` folds one stream into one document through convention methods: `Create(IEvent<TStarted>)` / `Create(TStarted)` to seed, `Apply(TEvent, TDoc)` (or `Evolve(TDoc?, TId, IEvent)` for the explicit form) to evolve, `ShouldDelete`/`DeleteEvent<TEvent>()` to retire, and `RaiseSideEffects(ops, slice)` to emit follow-on events/messages during async processing. `MultiStreamProjection<TDoc,TId>` slices across streams via `Identity<TEvent>`/`Identities<TEvent>` (event → owning aggregate id), `CustomGrouping` (arbitrary grouper), and `FanOut<TEvent,TChild>` (explode one event into many).
- `FetchLatest<T>`/`ProjectLatest<T>` return the up-to-date aggregate by reading the inline document when present or live-folding the tail; `AggregateStreamToLastKnownAsync<T>` folds up to the last non-archived event.

[AS_OF_TIMETRAVEL]:
- AS-OF is `AggregateStreamAsync<T>(id, version: v)` or `AggregateStreamAsync<T>(id, timestamp: t)` — the same fold bounded by a sequence version or a wall-clock instant; `fromVersion` plus a `state` seed replays a window forward from a known snapshot to bound replay cost. `FetchStreamAsync` with `version`/`timestamp` returns the raw `IEvent` window for blame/scrub. `FetchStreamStateAsync` answers head version/timestamps/archive without folding. The `Version/` engine's TimeTravel (AS-OF/blame/bisect/scrub/checkpoint) is these calls re-keyed to `NodeId`/`Relationship`; periodic inline snapshots bound the replay depth.

[PROJECTION_LANES]:
- `ProjectionLifecycle.Inline` writes the projected document inside the append transaction → authoritative containment/topology is read-your-writes consistent and NEVER routes to an async view. `ProjectionLifecycle.Async` runs off the `IProjectionDaemon` for the analytical lanes (AGE topology, DuckDB/BimOpenSchema columnar) with an explicit staleness watermark; interactive-correctness reads (clash, void-resolution, live QTO) block on `WaitForNonStaleProjectionDataAsync`/`daemon.WaitForNonStaleData` before querying. `ProjectionLifecycle.Live` folds on demand with no stored view.
- The `ElementGraph→Ara3D.BimOpenSchema` egress is a `FlatTableProjection` (`Project<T>(StatementMap)`), co-transactional with the write — not daemon-lagged. The daemon hosts as `DaemonMode.Solo` (single node) or `HotCold` (leader-elected); `RebuildProjectionAsync<TView>` replays a view from sequence zero, `store.Advanced.RebuildSingleStreamAsync<T>` rebuilds one stream's inline projection.

[TENANCY_IDENTITY]:
- The relational `ElementIdentity` row (PK/TenantId/GlobalId/H3/pgvector + ACL + classification — `api-npgsql-ef`, `api-pgvector-ef`, `api-h3`) commits atomically with the event by storing it as a Marten document via `session.Store(identity)` in the SAME `IDocumentSession`; one transaction owns identity + event with no two-ORM atomicity gap. `TenancyStyle.Conjoined` adds a tenant column for single-DB multi-tenancy; `TenancyStyle.Single` plus `MultiTenantedDatabases`/`MultiTenantedWithSingleServer` shards per tenant database. `StoreOptions.RegisterValueType<T>()` teaches Marten the `[ValueObject]` strong-typed `NodeId`/`GlobalId` so stream keys and document ids stay typed end to end.

[LOCAL_ADMISSION]:
- Marten owns the durable APPEND substrate and the rebuildable read views ONLY; the op-log/CRDT/`StructuralMerge`/causal-DAG merge semantics live in the `Version/` engine that projects FROM these events. Never re-implement event storage, stream folding, or async-projection scheduling locally — they are first-class here.
- One `DocumentStore` per database, registered through `AddMarten`; sessions are short-lived and per-unit-of-work. `SaveChangesAsync` is the only commit; never bypass it. The inline projection is the consistency boundary for authoritative reads — analytical lanes are explicitly async and watermarked.

[STACKING]:
- `Npgsql`/`Npgsql.DataSource` (`api-npgsql`): Marten rides the one shared `NpgsqlDataSource`; `StoreOptions.Connection(dataSource)` binds it so identity (EF — `api-npgsql-ef`) and events share the connection pool and can enlist one transaction.
- `System.Text.Json` + Thinktecture-JSON (`api-thinktecture-json`): event/document serialization is STJ; `[ValueObject]`/`[SmartEnum]` converters serialize typed `NodeId`/`GlobalId`/discriminants, and `RegisterValueType<T>` keys streams by them. `NodaTime` instants ride the event `Timestamp`/`Created` columns.
- `Ara3D.BimOpenSchema` (`api-ara3d-bimopenschema`): the columnar egress is a co-transactional `FlatTableProjection`; structural (EAV-generic) maps are Persistence-owned, BIM-typed maps are a Bim-implemented seam projection.
- `DuckDB`/`Apache AGE` (`api-duckdb`, `api-apache-age`): both are `ProjectionLifecycle.Async` analytical read lanes off the daemon, never the authoritative store; strong-consistency reads go through the inline ledger view.

[RAIL_LAW]:
- Package: `Marten` (event surface in transitive `JasperFx.Events`)
- Owns: the PostgreSQL event store + document database — per-model event streams (`StartStream`/`Append`), stream folding + AS-OF time-travel (`AggregateStreamAsync` by version/timestamp), the `SingleStreamProjection`/`MultiStreamProjection`/`FlatTableProjection` view family across `Inline`/`Async`/`Live` lifecycles, the async-projection daemon, optimistic/exclusive concurrency, document persistence, multi-tenancy, and schema migration over the one `NpgsqlDataSource`.
- Accept: the append substrate beneath the `Version/` engine; inline projections for authoritative read-your-writes topology; async daemon projections for the analytical lanes with explicit non-stale waits; identity + event atomicity through one `IDocumentSession`; `GraphDelta` event bodies on per-model streams; STJ serialization of typed value-object keys.
- Reject: per-`NodeId` stream grain; whole-graph snapshots as event bodies; routing interactive-correctness reads to an async projection without a non-stale wait; re-implementing event storage / stream folding / CRDT-merge here (merge is the `Version/` engine's); a second JSON serializer or a second connection pool; bypassing `SaveChangesAsync`.
