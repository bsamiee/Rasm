# [RASM_PERSISTENCE_API_LINQ2DB_EF]

`linq2db.EntityFrameworkCore` hands an EF Core `DbContext` to the LINQ To DB engine: it lifts an EF `DbSet`/`IQueryable` into an `ITable<T>`/`IQueryable<T>`, opens a `DataConnection`/`IDataContext` over the EF connection with optional transaction enlistment, and runs bulk COPY and value-insert over EF entities on the EF model's mapping schema. One async operator vocabulary carries a per-engine suffix naming the LINQ To DB or EF translator. Core `linq2db` executes the store moves a bridged context reaches — binary COPY, `RETURNING` upsert — feeding the PostgreSQL store profile.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `linq2db.EntityFrameworkCore`
- package: `linq2db.EntityFrameworkCore` (`MIT`, linq2db)
- assembly: `linq2db.EntityFrameworkCore`
- namespace: `LinqToDB.EntityFrameworkCore` (bridge), `LinqToDB.EntityFrameworkCore.Internal` (query provider, options extension)
- depends: core `linq2db` (`LinqToDB`, `LinqToDB.Data`) — the COPY/merge engine this bridge hands off to
- asset: runtime library, net10.0
- rail: store-provider

## [02]-[PUBLIC_TYPES]

[BRIDGE_TYPES]: EF-to-LINQ-To-DB hand-off surfaces

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :--------------------------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `LinqToDBForEFTools`               | class         | static hand-off root: bulk copy, table lift, metadata, caches     |
|  [02]   | `LinqToDBForEFExtensions`          | class         | static: query lift, connection/context build, `UseLinqToDB`       |
|  [03]   | `EFForEFExtensions`                | class         | static: `*AsyncEF` operators forcing the EF engine                |
|  [04]   | `LinqToDBContextOptionsBuilder`    | class         | fluent interceptor, mapping-schema, `DataOptions` admission       |
|  [05]   | `LinqToDBOptionsExtension`         | class         | `IDbContextOptionsExtension` the bridge installs                  |
|  [06]   | `ILinqToDBForEFTools`              | interface     | bridge policy contract                                            |
|  [07]   | `LinqToDBForEFToolsImplDefault`    | class         | default bridge policy; `CreateMetadataReader`                     |
|  [08]   | `LinqToDBExtensionsAdapter`        | class         | `IExtensionsAdapter`; adapts the async operator family            |
|  [09]   | `LinqToDBForEFToolsDataConnection` | class         | `DataConnection` enlisting `IEntityServiceInterceptor`            |
|  [10]   | `LinqToDBForEFToolsDataContext`    | class         | `DataContext` enlisting `IQueryExpressionInterceptor`             |
|  [11]   | `LinqToDBForEFQueryProvider<T>`    | class         | `IAsyncQueryProvider`/`IQueryProviderAsync`/`IAsyncEnumerable<T>` |
|  [12]   | `LinqToDBForEFToolsException`      | class         | sealed; carries bridge failure                                    |

[INFO_TYPES]: provider resolution carriers

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                |
| :-----: | :--------------------- | :------------ | :------------------------------------------ |
|  [01]   | `EFProviderInfo`       | class         | resolves the EF provider from context facts |
|  [02]   | `EFConnectionInfo`     | class         | carries the `DbConnection` and transaction  |
|  [03]   | `LinqToDBProviderInfo` | class         | names the resolved LINQ To DB provider      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: bulk copy on the owning `DbContext` (`where T : class`)

| [INDEX] | [SURFACE]                                                                                    | [CAPABILITY]                     |
| :-----: | :------------------------------------------------------------------------------------------- | :------------------------------- |
|  [01]   | `BulkCopy<T>(DbContext, BulkCopyOptions, IEnumerable<T>) -> BulkCopyRowsCopied`              | explicit-option bulk insert      |
|  [02]   | `BulkCopy<T>(DbContext, int, IEnumerable<T>) -> BulkCopyRowsCopied`                          | batch-size shorthand             |
|  [03]   | `BulkCopy<T>(DbContext, IEnumerable<T>) -> BulkCopyRowsCopied`                               | default-option bulk insert       |
|  [04]   | `BulkCopyAsync<T>(DbContext, BulkCopyOptions?/int?, IEnumerable<T>, CancellationToken)`      | async over `IEnumerable<T>`      |
|  [05]   | `BulkCopyAsync<T>(DbContext, BulkCopyOptions?/int?, IAsyncEnumerable<T>, CancellationToken)` | async over `IAsyncEnumerable<T>` |

- `BulkCopyAsync`: every arity returns `Task<BulkCopyRowsCopied>`; the three option arities mirror the sync forms across both source shapes.

[ENTRYPOINT_SCOPE]: query lift, table lift, value-insert, mapping extraction

| [INDEX] | [SURFACE]                                                                                         | [CAPABILITY]                     |
| :-----: | :------------------------------------------------------------------------------------------------ | :------------------------------- |
|  [01]   | `ToLinqToDB<T>(IQueryable<T>[, IDataContext]) -> IQueryable<T>`                                   | lift an EF query onto LINQ To DB |
|  [02]   | `ToLinqToDBTable<T>(DbSet<T>[, IDataContext]) -> ITable<T>`                                       | project a `DbSet` to a table     |
|  [03]   | `GetTable<T>(DbContext) -> ITable<T>`                                                             | open a table from the context    |
|  [04]   | `Into<T>(DbContext, ITable<T>) -> IValueInsertable<T> where T : notnull`                          | start a value-insert             |
|  [05]   | `GetMetadataReader(IModel?, IInfrastructure<IServiceProvider>?) -> IMetadataReader?`              | EF model to a metadata reader    |
|  [06]   | `GetMappingSchema(IModel, IInfrastructure<IServiceProvider>?, DataOptions?) -> MappingSchema`     | EF model to a mapping schema     |
|  [07]   | `GetMappingSchema(IModel, IRelationalTypeMappingSource?, IValueConverterSelector?, DataOptions?)` | mapping schema, explicit source  |

[ENTRYPOINT_SCOPE]: connection and context construction

| [INDEX] | [SURFACE]                                                                              | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------------------------------------- | :------------------------------------------ |
|  [01]   | `CreateLinqToDBConnection(DbContext, IDbContextTransaction? = null) -> DataConnection` | bridged connection, optional txn enlistment |
|  [02]   | `CreateLinqToDBConnection(DbContextOptions) -> DataConnection`                         | bridged connection from options alone       |
|  [03]   | `CreateLinqToDBConnectionDetached(DbContext) -> DataConnection`                        | change-tracker-free connection              |
|  [04]   | `CreateLinqToDBContext(DbContext, IDbContextTransaction? = null) -> IDataContext`      | bridged data context, optional txn enlist   |
|  [05]   | `GetLinqToDBOptions(DbContext/IDbContextOptions) -> DataOptions?`                      | read the bridge options off the context     |

[ENTRYPOINT_SCOPE]: async operators over a lifted `IQueryable<T>`

One operator vocabulary carries both engine suffixes: `*AsyncLinqToDB` forces the LINQ To DB translator, `*AsyncEF` forces the EF translator, and every operator exists under both.

[OPERATORS]: `All` `Any` `Average` `Contains` `Count` `First` `FirstOrDefault` `ForEach` `LongCount` `Max` `Min` `Single` `SingleOrDefault` `Sum` `ToArray` `ToDictionary` `ToList`
- Predicate and selector overloads ride the element operators; `Sum`/`Average`/`Max`/`Min` carry the numeric-selector overload set, and `ToDictionary` carries key-selector, element-selector, and comparer arities.

[ENTRYPOINT_SCOPE]: bridge admission and options

| [INDEX] | [SURFACE]                                                                             | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :------------------------------------------------------------------------------------ | :------- | :---------------------------------- |
|  [01]   | `UseLinqToDB<TContext>(TContext, Action<LinqToDBContextOptionsBuilder>?) -> TContext` | static   | admit the bridge, configure options |
|  [02]   | `LinqToDBContextOptionsBuilder.AddInterceptor(IInterceptor)`                          | instance | admit a LINQ To DB interceptor      |
|  [03]   | `LinqToDBContextOptionsBuilder.AddMappingSchema(MappingSchema)`                       | instance | admit a mapping schema              |
|  [04]   | `LinqToDBContextOptionsBuilder.AddCustomOptions(Func<DataOptions,DataOptions>)`       | instance | rewrite the `DataOptions`           |
|  [05]   | `LinqToDBContextOptionsBuilder.DbContextOptions`                                      | property | read the EF options under the build |
|  [06]   | `LinqToDBForEFTools.Initialize()` / `ClearCaches()`                                   | static   | activate the bridge, reset caches   |
|  [07]   | `LinqToDBForEFTools.EnableChangeTracker`                                              | property | gate the change-tracker feed        |

- `UseLinqToDB<TContext>` constrains `TContext : DbContextOptionsBuilder` and returns the same builder, so admission chains inline.
- `LinqToDBContextOptionsBuilder` returns itself from every `Add*` call, chaining interceptor, mapping-schema, and `DataOptions` rewrites into one admission expression.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every bridge op opens a LINQ To DB `IDataContext`/`DataConnection` over the EF connection and reuses the EF model's mapping schema, so bulk COPY, query lift, and value-insert bind the same column mapping EF persists through.
- Core `linq2db` owns every store engine this surface reaches; PostgreSQL binary COPY and `RETURNING` upsert execute there against the bridged context.

[STACKING]:
- core `linq2db` engine: a bridged `IDataContext` or a lifted `ITable<T>` reaches `DataContextExtensions.BulkCopyAsync(BulkCopyOptions, IEnumerable<T>/IAsyncEnumerable<T>, CancellationToken) -> Task<BulkCopyRowsCopied>` and `LinqExtensions.MergeWithOutputAsync(IMergeable<TTarget,TSource>, Expression<Func<string,TTarget,TTarget,TOutput>>) -> IAsyncEnumerable<TOutput>`, whose `(action, deleted, inserted)` projection streams the `RETURNING` rows of an upsert.
- `BulkCopyOptions` is a `sealed record` whose `BulkCopyType` selects the lowering — `ProviderSpecific` to PostgreSQL binary COPY, `MultipleRows` to multi-row INSERT, `RowByRow` to per-row INSERT — and whose `ConflictAction` (`Default`/`Ignore`) drives the merge ON CONFLICT path; a `with` expression over `BulkCopyOptions.Default` mints each profile's option set.
- `BulkCopyOptions` knobs: `MaxBatchSize` `BulkCopyTimeout` `CheckConstraints` `KeepIdentity` `KeepNulls` `TableLock` `FireTriggers` `UseInternalTransaction` `ServerName` `DatabaseName` `SchemaName` `TableName` `TableOptions` `NotifyAfter` `RowsCopiedCallback` `UseParameters` `MaxParametersForBatch` `MaxDegreeOfParallelism` `WithoutSession` `ConflictAction`.
- `Npgsql.EntityFrameworkCore.PostgreSQL`(`.api/api-npgsql-ef.md`): provider under the bridge — `BulkCopyType.ProviderSpecific` lowers onto the Npgsql binary COPY writer, and `GetLinqToDBOptions` reads the profile's `DataOptions` off the provider-configured context.
- `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite`(`.api/api-nts-ef.md`): geometry and NodaTime timestamp columns survive the COPY path because `GetMappingSchema` hands the EF model's own type mappings to the COPY writer instead of a second mapping.
- `Sep`(`.api/api-sep.md`): `SepReaderExtensions.Enumerate(RowFunc<T>)`/`EnumerateAsync` produce the `IEnumerable<T>`/`IAsyncEnumerable<T>` source `BulkCopy`/`BulkCopyAsync` consume, streaming a CSV file straight into PostgreSQL COPY with no intermediate materialization.
- `Npgsql.OpenTelemetry`(`libs/csharp/.api/api-npgsql-opentelemetry.md`): bridged `DataConnection` activity flows through the core `linq2db` `ActivityService` onto the same span pipeline that observes the EF command interceptor.
- query-lane composition: `ToLinqToDB` is the seam a window-function, CTE, or bulk-update query EF cannot translate crosses onto the LINQ To DB translator, re-materializing through `ToListAsyncLinqToDB`; `*AsyncEF` holds the EF translator for change-tracked reads on the same lifted queryable.

[LOCAL_ADMISSION]:
- Bridge surfaces operate on contexts admitted through the store-profile algebra.
- Bulk-copy options and `BulkCopyRowsCopied` are profile receipts.
- Interceptors and mapping schemas enter through an explicit `UseLinqToDB` `Action<LinqToDBContextOptionsBuilder>` declaration.
- A lifted read stays out of the EF change tracker until `EnableChangeTracker` is set; `CreateLinqToDBConnectionDetached` is the change-tracker-free path for high-throughput reads.

[RAIL_LAW]:
- Package: `linq2db.EntityFrameworkCore`
- Owns: the EF-to-LINQ-To-DB hand-off — bulk copy, query and table lift, value-insert entry, connection and context construction, mapping and metadata reuse, and the dual-engine async operator vocabulary
- Accept: bridge calls on admitted EF store profiles; `UseLinqToDB` admission carrying explicit interceptor and mapping declarations; core-engine COPY and merge reached through a bridged `IDataContext` or `ITable<T>`
- Reject: query rails standing outside the store profile; core `linq2db` COPY and merge surfaces re-declared as bridge entrypoints; change-tracked lifts where the detached or COPY path carries the load
