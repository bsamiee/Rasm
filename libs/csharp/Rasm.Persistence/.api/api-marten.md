# [RASM_PERSISTENCE_API_MARTEN]

`Marten` turns one `NpgsqlDataSource` into a PostgreSQL document database and an ACID event store: per-model streams append `GraphDelta` bodies that projection views rebuild across every lifecycle off the async daemon. It is the append substrate beneath the `Version/` engine — owning the durable append and the rebuildable read views, never the op-log/CRDT/`StructuralMerge`/causal-DAG merge semantics projecting FROM these events.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Marten`
- package: `Marten` (MIT)
- assembly: `Marten` binds Postgres, the document store, and raw-JSON passthrough onto the database-agnostic event surface transitive `JasperFx.Events` carries
- namespace: `Marten`, `Marten.Events`, `Marten.Events.Aggregation`, `Marten.Events.Projections`, `Marten.Events.Projections.Flattened`, `Marten.Events.Daemon`, `Marten.Subscriptions`, `Microsoft.Extensions.DependencyInjection`; transitive `JasperFx.Events`, `JasperFx.Events.Projections`, `JasperFx.Events.Daemon`, `JasperFx.Events.Aggregation`, `JasperFx.MultiTenancy`
- asset: managed runtime library over `NpgsqlDataSource`; ships the compile-time projection/aggregation source generator (`JasperFx.Events.SourceGenerator.dll`, enabled via `UseSourceGeneratedDiscovery`/`TryUseSourceGeneratedDiscovery`)
- rail: event-store

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: configuration root and document store

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :------------------------------ | :------------ | :--------------------------------------------------------------- |
|  [01]   | `DocumentStore`                 | sealed class  | the store: `For(...)` factory + session openers                  |
|  [02]   | `IDocumentStore`                | interface     | the session factory + store operations                           |
|  [03]   | `StoreOptions`                  | class         | the full configuration surface                                   |
|  [04]   | `IReadOnlyStoreOptions`         | interface     | read-only projection of a configured `StoreOptions`              |
|  [05]   | `SessionOptions`                | class         | per-session tenancy/isolation/tracking/connection overrides      |
|  [06]   | `AdvancedOperations`            | class         | `store.Advanced` — store maintenance + event-store introspection |
|  [07]   | `ProjectionOptions`             | class         | `opts.Projections` — the projection registry                     |
|  [08]   | `AsyncOptions`                  | class         | async-projection tuning: `BatchSize`, `SubscribeFrom*`, teardown |
|  [09]   | `MartenConfigurationExpression` | class         | the `AddMarten(...)` fluent DI surface                           |

[PUBLIC_TYPE_SCOPE]: sessions and document operations

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                                                       |
| :-----: | :-------------------- | :------------ | :--------------------------------------------------------------------------------- |
|  [01]   | `IQuerySession`       | interface     | the read-side session surface                                                      |
|  [02]   | `IDocumentSession`    | interface     | the write-side session surface                                                     |
|  [03]   | `IDocumentOperations` | interface     | shared document mutation: `Store`/`Insert`/`Delete`/`HardDelete`/`QueueSqlCommand` |
|  [04]   | `IMartenQueryable<T>` | interface     | the Marten LINQ `IQueryable<T>` provider                                           |
|  [05]   | `DocumentMetadata`    | class         | per-document metadata (version, last-modified, tenant, deleted)                    |
|  [06]   | `IUnitOfWork`         | interface     | `session.PendingChanges` — staged inserts/updates/deletes/events                   |
|  [07]   | `ConcurrencyChecks`   | enum          | `Enabled`/`Disabled` optimistic-concurrency mode on a session                      |

[PUBLIC_TYPE_SCOPE]: event store — read and write (`JasperFx.Events`)

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :------------------------------------ | :------------ | :--------------------------------------------------------------- |
|  [01]   | `IEventOperations`                    | interface     | the write contract: `StartStream`/`Append`                       |
|  [02]   | `IQueryEventStore`                    | interface     | the read contract                                                |
|  [03]   | `IEventStoreOperations`               | interface     | the full session event store: read + write                       |
|  [04]   | `Marten.Events.IEventStoreOperations` | interface     | Marten extension: `StreamLatestJson<T>` raw-JSON passthrough     |
|  [05]   | `IEventStream<T>`                     | interface     | the optimistic-write handle from `FetchForWriting<T>`            |
|  [06]   | `StreamAction`                        | class         | the staged stream operation from `StartStream`/`Append`          |
|  [07]   | `StreamActionType`                    | enum          | `Start`/`Append`/`Archive` — the stream-action discriminant      |
|  [08]   | `StreamState`                         | class         | the `FetchStreamStateAsync` result: version, timestamps, archive |
|  [09]   | `IEvent` / `IEvent<T>`                | interface     | the stored-event envelope                                        |
|  [10]   | `EventTag` / `EventTagQuery`          | class         | event tag value + tag-query shape                                |

[PUBLIC_TYPE_SCOPE]: projections and the async daemon

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [CAPABILITY]                                                        |
| :-----: | :-------------------------------------- | :------------ | :------------------------------------------------------------------ |
|  [01]   | `SingleStreamProjection<TDoc,TId>`      | abstract base | one-stream aggregate → one document                                 |
|  [02]   | `MultiStreamProjection<TDoc,TId>`       | abstract base | cross-stream aggregate → grouped documents                          |
|  [03]   | `FlatTableProjection`                   | class         | events → flat relational table (columnar egress)                    |
|  [04]   | `ProjectionBase`                        | abstract base | shared projection config                                            |
|  [05]   | `IProjection`                           | interface     | the raw Marten projection contract (`ApplyAsync(ops, streams, ct)`) |
|  [06]   | `ProjectionLifecycle`                   | enum          | `Inline` (write-txn), `Async` (daemon), `Live` (on-demand fold)     |
|  [07]   | `SnapshotLifecycle`                     | enum          | `Inline`/`Async` — the snapshot variant of lifecycle                |
|  [08]   | `IProjectionDaemon`                     | interface     | the async-projection runner                                         |
|  [09]   | `DaemonMode`                            | enum          | `Disabled`/`Solo`/`HotCold` — daemon hosting topology (`JasperFx`)  |
|  [10]   | `ShardState` / `ShardName`              | class         | per-shard progress (`Sequence`, `Timestamp`) + `ShardName.Identity` |
|  [11]   | `EventStoreStatistics`                  | class         | the `FetchEventStoreStatistics` result                              |
|  [12]   | `IEventUpcaster` / `JsonTransformation` | interface     | schema-evolution upcast of old event JSON to a new event type       |

[PUBLIC_TYPE_SCOPE]: event subscriptions — the daemon changefeed lift; `SubscriptionBase` rides `Marten.Subscriptions`, the batch and controller types `JasperFx.Events.Projections`/`.Daemon`

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                                            |
| :-----: | :------------------------ | :------------ | :-------------------------------------------------------------------------------------- |
|  [01]   | `SubscriptionBase`        | abstract base | override `ProcessEventsAsync` to lift each daemon batch                                 |
|  [02]   | `EventRange`              | class         | the delivered batch (`Events`, `ShardName`, `SequenceFloor`/`Ceiling`, `Size`)          |
|  [03]   | `ISubscriptionController` | interface     | per-shard controller (`Mode`, `Name`, `MarkSuccessAsync`, `ReportCriticalFailureAsync`) |
|  [04]   | `IChangeListener`         | interface     | post-batch commit hook (`BeforeCommitAsync`/`AfterCommitAsync`)                         |
|  [05]   | `NullChangeListener`      | class         | `Instance` — the no-side-effect listener singleton                                      |

[PUBLIC_TYPE_SCOPE]: event configuration (`opts.Events`) and tenancy/identity vocabulary

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                                                                     |
| :-----: | :------------------- | :------------ | :----------------------------------------------------------------------------------------------- |
|  [01]   | `IEventStoreOptions` | interface     | `StreamIdentity`, `AppendMode`, `TenancyStyle`, `MetadataConfig`, event-type mapping, upcasting  |
|  [02]   | `StreamIdentity`     | enum          | `AsGuid` / `AsString` — stream-key type (per-model GUID vs string partition key)                 |
|  [03]   | `EventAppendMode`    | enum          | `Rich`/`Quick`/`QuickWithServerTimestamps` — append-throughput tier                              |
|  [04]   | `TenancyStyle`       | enum          | `Single` / `Conjoined` — column-tenancy vs separate tenant databases (`JasperFx`)                |
|  [05]   | `MetadataConfig`     | class         | which metadata columns (causation/correlation/headers/user) the event table carries              |
|  [06]   | `AutoCreate`         | enum          | `None`/`CreateOnly`/`CreateOrUpdate`/`All` — schema auto-migration policy (`JasperFx`)           |
|  [07]   | `MartenRegistry`     | class         | `opts.Schema` — per-document mapping (`For<T>()`: identity, indexes, multi-tenancy, soft-delete) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: configure the store and open sessions (`DocumentStore`/`StoreOptions`/`IDocumentStore`)

| [INDEX] | [SURFACE]                                           | [SHAPE]  | [CAPABILITY]                                             |
| :-----: | :-------------------------------------------------- | :------- | :------------------------------------------------------- |
|  [01]   | `DocumentStore.For(Action<StoreOptions>)`           | static   | build a configured store                                 |
|  [02]   | `StoreOptions.Connection(NpgsqlDataSource)`         | instance | bind the Postgres connection / data source               |
|  [03]   | `StoreOptions.DatabaseSchemaName`                   | property | schema + migration + Npgsql tuning                       |
|  [04]   | `StoreOptions.UseSystemTextJsonForSerialization`    | instance | STJ serializer (Thinktecture converters apply)           |
|  [05]   | `StoreOptions.Schema.For<T>()`                      | instance | document mapping, policies, strong-typed-id registration |
|  [06]   | `IDocumentStore.LightweightSession(SessionOptions)` | instance | open a write session by tracking mode                    |
|  [07]   | `IDocumentStore.QuerySession(SessionOptions)`       | instance | open a read / serializable-isolation session             |
|  [08]   | `IDocumentStore.Advanced.ResetAllData()`            | instance | teardown + event-store + projection introspection        |

[ENTRYPOINT_SCOPE]: document read and write (`session`)

| [INDEX] | [SURFACE]                           | [SHAPE]  | [CAPABILITY]                                                         |
| :-----: | :---------------------------------- | :------- | :------------------------------------------------------------------- |
|  [01]   | `session.Store<T>(params T[])`      | instance | stage document upserts (`Insert`/`Update` for insert/update only)    |
|  [02]   | `session.Delete<T>(T)`              | instance | stage soft deletion by entity or id                                  |
|  [03]   | `session.HardDelete<T>(T)`          | instance | stage hard deletion                                                  |
|  [04]   | `session.LoadAsync<T>(id)`          | instance | load one document by id                                              |
|  [05]   | `session.LoadManyAsync<T>(id[])`    | instance | load many documents by id                                            |
|  [06]   | `session.Query<T>()`                | instance | Marten LINQ `IQueryable<T>` (async materializers, `Include`, paging) |
|  [07]   | `session.QueryAsync<T>(string sql)` | instance | raw-SQL document query                                               |
|  [08]   | `session.ForTenant(string)`         | instance | scope operations to another tenant                                   |

[ENTRYPOINT_SCOPE]: append and start streams (`session.Events`)

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]                                                 |
| :-----: | :----------------------------------------------------------- | :------- | :----------------------------------------------------------- |
|  [01]   | `Events.StartStream<TAggregate>(Guid, params object[])`      | instance | begin a new stream (GUID per model, or string partition key) |
|  [02]   | `Events.Append(Guid, params object[])`                       | instance | append to an existing stream                                 |
|  [03]   | `Events.Append(Guid, long expectedVersion, params object[])` | instance | append with an inline optimistic version guard               |
|  [04]   | `Events.AppendOptimistic(Guid, params object[])`             | instance | append with a read-then-guard optimistic check               |
|  [05]   | `Events.AppendExclusive(Guid, params object[])`              | instance | append under an exclusive stream lock                        |
|  [06]   | `Events.ArchiveStream(Guid)`                                 | instance | soft-archive a whole stream                                  |
|  [07]   | `session.SaveChangesAsync()`                                 | instance | commit staged events + documents in one txn                  |

[ENTRYPOINT_SCOPE]: read and fold streams — AS-OF time-travel (`session.Events`)

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]                                              |
| :-----: | :------------------------------------------------------------ | :------- | :-------------------------------------------------------- |
|  [01]   | `Events.AggregateStreamAsync<T>(Guid, long, DateTimeOffset?)` | instance | fold a stream into `T`; AS-OF by `version` or `timestamp` |
|  [02]   | `Events.AggregateStreamToLastKnownAsync<T>(Guid)`             | instance | fold ignoring an archive cut; string-key fold variant     |
|  [03]   | `Events.FetchStreamAsync(Guid, long, DateTimeOffset?)`        | instance | read the raw `IEvent` window (full or bounded)            |
|  [04]   | `Events.FetchStreamStateAsync(Guid)`                          | instance | current `StreamState` without folding                     |
|  [05]   | `Events.LoadAsync<T>(Guid)`                                   | instance | load a single typed / untyped stored event by id          |
|  [06]   | `Events.StreamLatestJson<T>(Guid, Stream)`                    | instance | stream the aggregate's raw JSON, no round-trip            |

[ENTRYPOINT_SCOPE]: fold a LINQ event query to aggregates (`IMartenQueryable<IEvent>` — `Marten.Events.AggregateToExtensions`)

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]                                                               |
| :-----: | :---------------------------------------------- | :------- | :------------------------------------------------------------------------- |
|  [01]   | `AggregateToAsync<T>(T?) -> T?`                 | instance | fold every matched event into one aggregate of `T`                         |
|  [02]   | `AggregateToManyAsync<T>() -> IReadOnlyList<T>` | instance | run matched events through `T`'s multi-stream projection, one per identity |

[ENTRYPOINT_SCOPE]: fetch-for-writing and aggregate-handler workflow (`session.Events`)

| [INDEX] | [SURFACE]                                                              | [SHAPE]  | [CAPABILITY]                                         |
| :-----: | :--------------------------------------------------------------------- | :------- | :--------------------------------------------------- |
|  [01]   | `Events.FetchForWriting<T>(Guid, long)`                                | instance | load aggregate + version handle for a guarded append |
|  [02]   | `Events.FetchForExclusiveWriting<T>(Guid)`                             | instance | same under an exclusive lock                         |
|  [03]   | `Events.WriteToAggregate<T>(Guid, Func<IEventStream<T>,Task>)`         | instance | load-decide-append in one closure                    |
|  [04]   | `Events.WriteExclusivelyToAggregate<T>(Guid, Action<IEventStream<T>>)` | instance | the exclusive-lock variant                           |
|  [05]   | `Events.FetchLatest<T>(Guid)`                                          | instance | the up-to-date aggregate (inline doc or live fold)   |
|  [06]   | `Events.ProjectLatest<T>(Guid)`                                        | instance | force a live projection of the latest state          |
|  [07]   | `Events.QueryByTagsAsync(EventTagQuery)`                               | instance | tag-scoped read / fold / write / existence           |

[ENTRYPOINT_SCOPE]: register projections and run the daemon (`StoreOptions.Projections`/`IProjectionDaemon`)

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                                    |
| :-----: | :-------------------------------------------------------------------- | :------- | :---------------------------------------------- |
|  [01]   | `opts.Projections.Add<TProjection>(ProjectionLifecycle)`              | instance | register a projection by type / instance        |
|  [02]   | `opts.Projections.Snapshot<T>(SnapshotLifecycle)`                     | instance | self-aggregate snapshot / live-only aggregation |
|  [03]   | `opts.Projections.AddGlobalProjection<TDoc,TId>(ProjectionLifecycle)` | instance | global single-stream or daemon subscription     |
|  [04]   | `MultiStreamProjection<TDoc,TId>.Identity<TEvent>(Func<TEvent,TId>)`  | instance | configure cross-stream slicing                  |
|  [05]   | `FlatTableProjection.Project<T>(Action<StatementMap<T>>)`             | instance | events → relational table upsert / delete rows  |
|  [06]   | `store.BuildProjectionDaemonAsync()`                                  | instance | construct the async-projection daemon           |
|  [07]   | `daemon.StartAllAsync()`                                              | instance | start / rebuild / await-caught-up / stop shards |
|  [08]   | `store.Advanced.RebuildSingleStreamAsync<T>(Guid)`                    | instance | rebuild a single stream's inline projection     |

[ENTRYPOINT_SCOPE]: DI registration and host integration (`MartenServiceCollectionExtensions`)

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :-------------------------------------------------------------------- | :------- | :-------------------------------------------------- |
|  [01]   | `services.AddMarten(Action<StoreOptions>)`                            | static   | register the store + sessions in DI                 |
|  [02]   | `.AddAsyncDaemon(DaemonMode)`                                         | instance | host the daemon / pick the default session tracking |
|  [03]   | `.ApplyAllDatabaseChangesOnStartup()`                                 | instance | migrate / assert schema at host startup             |
|  [04]   | `IMartenStorage.ApplyAllConfiguredChangesToDatabaseAsync()`           | instance | runtime single-writer schema apply (`AdmitMarten`)  |
|  [05]   | `.AddProjectionWithServices<T>(ProjectionLifecycle, ServiceLifetime)` | instance | register a DI-injected projection                   |
|  [06]   | `services.AddMartenStore<T>(Action<StoreOptions>)`                    | static   | register a separate ancillary store / extend config |
|  [07]   | `host.WaitForNonStaleProjectionDataAsync(TimeSpan)`                   | instance | block until async projections are caught up         |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Stream grain is ONE stream PER MODEL (or per spatial partition), NEVER per `NodeId`; the event body is the `GraphDelta`, never a whole-graph snapshot. `StreamIdentity.AsString` keys a composite partition, `AsGuid` a model GUID. `SaveChangesAsync` is the only commit — events + documents in one Postgres transaction — and the whole `ElementGraph` rehydrates by folding the stream through `AggregateStreamAsync<T>`, zero-loss because the deltas replay deterministically.
- Optimistic concurrency rides `Append(stream, expectedVersion, …)` (inline guard), `AppendOptimistic` (read-then-guard), or `FetchForWriting<T>(id, expectedVersion)` returning an `IEventStream<T>` handle; `AppendExclusive`/`FetchForExclusiveWriting` take a stream-level advisory lock for multi-writer-hostile sections. `EventAppendMode.Quick`/`QuickWithServerTimestamps` trade event-metadata richness for bulk-append throughput.
- A `SingleStreamProjection<TDoc,TId>` folds one stream to one document through convention methods — `Create` seeds, `Apply(TEvent, TDoc)` or `Evolve(TDoc?, TId, IEvent)` evolves, `ShouldDelete`/`DeleteEvent<TEvent>` retires, `RaiseSideEffects(ops, slice)` emits follow-on events during async processing. `MultiStreamProjection<TDoc,TId>` slices across streams via `Identity`/`Identities` (event → owning id), `CustomGrouping`, and `FanOut<TEvent,TChild>`.
- AS-OF is `AggregateStreamAsync<T>(id, version:)` or `(id, timestamp:)` — one fold bounded by a sequence version or a wall-clock instant; a `fromVersion` with a `state` seed replays forward from a snapshot to bound replay cost. `FetchStreamAsync` bounded the same way returns the raw `IEvent` window for blame/scrub, and `FetchStreamStateAsync` answers head state without folding. `Version/` TimeTravel re-keys these to `NodeId`/`Relationship`, periodic inline snapshots bounding replay depth.
- `ProjectionLifecycle.Inline` writes the projected document inside the append transaction, so authoritative containment/topology is read-your-writes consistent and never routes to an async view. `ProjectionLifecycle.Async` runs the analytical lanes off the `IProjectionDaemon` under an explicit staleness watermark; an interactive-correctness read (clash, void-resolution, live QTO) blocks on `WaitForNonStaleProjectionDataAsync`/`daemon.WaitForNonStaleData` first. `ProjectionLifecycle.Live` folds on demand with no stored view.
- `ElementGraph→Ara3D.BimOpenSchema` egress lands as a co-transactional `FlatTableProjection`, never daemon-lagged. Daemon hosting is `DaemonMode.Solo` or `HotCold`; `RebuildProjectionAsync<TView>` replays a view from sequence zero and `store.Advanced.RebuildSingleStreamAsync<T>` one stream's inline projection.
- High-water detection holds the async mark behind any gap a live transaction owns: `opts.Projections.UseTransactionEvidenceForGapSkipping` (default on) proves a gap dead against `pg_locks`/`pg_stat_activity`/`pg_current_snapshot()` before skipping it, and `SkipStaleGapsDespiteLiveTransactionsAfter` (default null) is the wall-clock backstop against leaked sessions, so a bulk-import append storm drops no events from an async projection.
- A `SubscriptionBase` subclass lifts each daemon-delivered `EventRange` batch by overriding `ProcessEventsAsync(EventRange, ISubscriptionController, IDocumentOperations, CancellationToken)` returning an `IChangeListener` post-commit hook — `NullChangeListener.Instance` for no side effect; `ISubscriptionController.MarkSuccessAsync`/`ReportCriticalFailureAsync` drive per-shard progress and dead-letter recording.
- Relational `ElementIdentity` commits atomically with the event through `session.Store(identity)` in the SAME `IDocumentSession`, one transaction owning identity + event with no two-ORM gap. `StoreOptions.RegisterValueType<T>()` teaches Marten the `[ValueObject]` strong-typed `NodeId`/`GlobalId`, keeping stream keys and document ids typed end to end.
- `TenancyStyle.Conjoined` adds a tenant column for single-DB multi-tenancy; `TenancyStyle.Single` with `MultiTenantedDatabases`/`MultiTenantedWithSingleServer` shards per tenant database.

[STACKING]:
- `api-npgsql`(`.api/api-npgsql.md`) / `api-npgsql-ef`(`.api/api-npgsql-ef.md`): Marten rides the one shared `NpgsqlDataSource` bound through `StoreOptions.Connection(dataSource)`, so the EF-mapped identity row and the events share a connection pool and enlist one transaction.
- `api-thinktecture-serialization`(`.api/api-thinktecture-serialization.md`): event and document serialization is STJ through the registered `ThinktectureJsonConverterFactory`, projecting each `[ValueObject]`/`[SmartEnum]` owner to its key; `RegisterValueType<T>` then keys streams and document ids by that owner, and `NodaTime` instants ride the event `Timestamp`/`Created` columns.
- `api-ara3d-bimopenschema`(`.api/api-ara3d-bimopenschema.md`): the columnar egress is a co-transactional `FlatTableProjection` whose `Project<T>(StatementMap)` rows land the BIM analytics frame; structural EAV-generic maps are Persistence-owned, BIM-typed maps a Bim-implemented seam projection.
- `api-duckdb`(`.api/api-duckdb.md`) / `api-apache-age`(`.api/api-apache-age.md`): both mount as `ProjectionLifecycle.Async` daemon views, never the authoritative store; a strong-consistency read goes through the inline ledger view instead.
- within-lib: the `Version/` engine composes this surface as its append rail — `GraphDelta` bodies on per-model streams, an inline `SingleStreamProjection` holding authoritative containment read-your-writes consistent, and `UseSourceGeneratedDiscovery` compiling the projection/aggregation fold at build time rather than resolving it by runtime reflection; the `AdmitMarten` leg applies schema through `IMartenStorage` as the single-writer migration step.

[LOCAL_ADMISSION]:
- Event storage, stream folding, and async-projection scheduling are first-class here; the `Version/` engine composes them rather than re-deriving any.
- One `DocumentStore` per database, registered through `AddMarten`; sessions are short-lived and per-unit-of-work, and `SaveChangesAsync` is the single commit. Inline projection is the consistency boundary for authoritative reads; analytical lanes are async and watermarked.

[RAIL_LAW]:
- Package: `Marten`
- Owns: the PostgreSQL event store + document database over one `NpgsqlDataSource` — per-model event streams, stream folding with AS-OF time-travel, the single/multi-stream/flat-table projection family across every lifecycle, the async-projection daemon, optimistic/exclusive concurrency, document persistence, multi-tenancy, and schema migration.
- Accept: the append substrate beneath the `Version/` engine; inline projections for authoritative read-your-writes topology; async daemon projections for analytical lanes with explicit non-stale waits; identity + event atomicity through one `IDocumentSession`; `GraphDelta` event bodies on per-model streams; STJ serialization of typed value-object keys.
- Reject: per-`NodeId` stream grain; whole-graph snapshots as event bodies; an async projection serving an interactive-correctness read without a non-stale wait; a hand-rolled event store, stream fold, or CRDT merge (merge is the `Version/` engine's); a second JSON serializer or connection pool; a commit that bypasses `SaveChangesAsync`.
