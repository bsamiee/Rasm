# [RASM_PERSISTENCE_API_LINQ2DB_EF]

`linq2db.EntityFrameworkCore` bridges an EF Core `DbContext` into LINQ To DB: it lifts an
EF `DbSet`/`IQueryable` into a LINQ To DB `ITable<T>`/`IQueryable<T>`, opens a LINQ To DB
`DataConnection`/`IDataContext` over the EF connection (optionally enlisting the EF
transaction), runs bulk COPY and value-insert/merge over EF entities reusing the EF model's
mapping schema, and supplies dual async operator families — `*AsyncLinqToDB` forcing the
LINQ To DB engine, `*AsyncEF` forcing EF. The advanced store moves (PostgreSQL binary COPY,
`MergeWithOutput` upsert with `RETURNING`) live in the core `linq2db` assembly; this bridge
is the EF-context hand-off seam onto them.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `linq2db.EntityFrameworkCore`
- package: `linq2db.EntityFrameworkCore`
- license: `MIT`
- assembly: `linq2db.EntityFrameworkCore`
- namespace: `LinqToDB.EntityFrameworkCore`
- core dependency: `linq2db` `6.3.0` (`LinqToDB`, `LinqToDB.Data` — the COPY/merge engine)
- asset: runtime library
- target: net10.0 (single-target; binds cleanly)
- rail: store-provider

## [02]-[PUBLIC_TYPES]

[BRIDGE_TYPES]: EF-to-LINQ-To-DB bridge surfaces
- rail: store-provider

`LinqToDBForEFTools` is the static hand-off root (bulk, table lift, mapping/metadata
extraction); `LinqToDBForEFExtensions` carries the query lift, connection/context
construction, options read, and `UseLinqToDB` admission; `EFForEFExtensions` carries the
`*AsyncEF` mirror. `LinqToDBForEFQueryProvider<T>` is the async query provider that backs a
lifted queryable; `LinqToDBOptionsExtension` is the EF `IDbContextOptionsExtension` the
bridge installs (distinct from the `LinqToDBContextOptionsBuilder` fluent surface).

| [INDEX] | [SYMBOL]                           | [PACKAGE_ROLE]       | [CAPABILITY]                                                      |
| :-----: | :--------------------------------- | :------------------- | :---------------------------------------------------------------- |
|  [01]   | `LinqToDBForEFTools`               | static bridge root   | bulk copy, table lift, mapping/metadata extract                   |
|  [02]   | `LinqToDBForEFExtensions`          | static extension     | query lift, connection/context build, `UseLinqToDB`               |
|  [03]   | `EFForEFExtensions`                | static extension     | `*AsyncEF` operators (force EF engine)                            |
|  [04]   | `LinqToDBContextOptionsBuilder`    | fluent options       | `AddInterceptor`/`AddMappingSchema`/`AddCustomOptions`            |
|  [05]   | `LinqToDBOptionsExtension`         | EF options extension | `IDbContextOptionsExtension` the bridge installs                  |
|  [06]   | `ILinqToDBForEFTools`              | bridge contract      | abstracts bridge policy                                           |
|  [07]   | `LinqToDBForEFToolsImplDefault`    | bridge default       | implements bridge policy (`CreateMetadataReader`)                 |
|  [08]   | `LinqToDBExtensionsAdapter`        | operator adapter     | `IExtensionsAdapter`; adapts async operators                      |
|  [09]   | `EFCoreMetadataReader`             | metadata reader      | projects EF `IModel` metadata to LINQ To DB                       |
|  [10]   | `LinqToDBForEFToolsDataConnection` | data connection      | `DataConnection` enlisting EF interceptors                        |
|  [11]   | `LinqToDBForEFToolsDataContext`    | data context         | `DataContext` enlisting EF query interceptors                     |
|  [12]   | `LinqToDBForEFQueryProvider<T>`    | async query provider | `IAsyncQueryProvider`/`IQueryProviderAsync`/`IAsyncEnumerable<T>` |
|  [13]   | `LinqToDBForEFToolsException`      | bridge error         | carries bridge failure                                            |

[INFO_TYPES]: provider resolution surfaces
- rail: store-provider

| [INDEX] | [SYMBOL]               | [PACKAGE_ROLE]   | [CAPABILITY]              |
| :-----: | :--------------------- | :--------------- | :------------------------ |
|  [01]   | `EFProviderInfo`       | provider info    | resolves EF provider      |
|  [02]   | `EFConnectionInfo`     | connection info  | carries connection facts  |
|  [03]   | `LinqToDBProviderInfo` | provider mapping | names LINQ To DB provider |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: bulk copy (`LinqToDBForEFTools`, `where T : class`)
- rail: store-provider

Three arities — explicit `BulkCopyOptions`, `int maxBatchSize` shorthand, and plain. The sync
form returns `BulkCopyRowsCopied`; the async form mirrors both `IEnumerable<T>` and
`IAsyncEnumerable<T>` sources, returning `Task<BulkCopyRowsCopied>`. The option shapes are
CORE-linq2db members riding the bridge transitively — `BulkCopyOptions` (with `BulkCopyType`/
`KeepIdentity`/`MaxBatchSize` slots) and the `BulkCopyType` enum (`Default`/`RowByRow`/
`MultipleRows`/`ProviderSpecific` — `ProviderSpecific` lowers to PG binary COPY; verified
against the restored linq2db assembly). `BulkCopyRowsCopied` is the typed receipt carrying rows
copied and the options that produced it.

| [INDEX] | [SURFACE]                                                                       | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------------------------------ | :------------------------------------ |
|  [01]   | `BulkCopy<T>(context, BulkCopyOptions, IEnumerable<T>)`                         | bulk insert with explicit options     |
|  [02]   | `BulkCopy<T>(context, int maxBatchSize, IEnumerable<T>)`                        | bulk insert with batch-size shorthand |
|  [03]   | `BulkCopy<T>(context, IEnumerable<T>)`                                          | bulk insert with default options      |
|  [04]   | `BulkCopyAsync<T>(context, BulkCopyOptions / int / —, IEnumerable<T>, ct)`      | async over `IEnumerable<T>`           |
|  [05]   | `BulkCopyAsync<T>(context, BulkCopyOptions / int / —, IAsyncEnumerable<T>, ct)` | async over `IAsyncEnumerable<T>`      |

[ENTRYPOINT_SCOPE]: query lift, value-insert, mapping/metadata extraction
- rail: store-provider

`ToLinqToDB` lifts an EF `IQueryable<T>` onto the LINQ To DB translator (optionally onto an
explicit `IDataContext`); `ToLinqToDBTable` lifts a `DbSet<T>` to an `ITable<T>`; `Into`
starts a LINQ To DB value-insert against a lifted table. `GetMappingSchema`/
`GetMetadataReader` extract the EF model's mapping for reuse by the `Element/identity` converter and
identity rails.

| [INDEX] | [SURFACE]                                                             | [RETURNS]             | [CAPABILITY]                             |
| :-----: | :-------------------------------------------------------------------- | :-------------------- | :--------------------------------------- |
|  [01]   | `ToLinqToDB<T>(IQueryable<T>)` / `ToLinqToDB<T>(query, IDataContext)` | `IQueryable<T>`       | hand EF query to LINQ To DB translator   |
|  [02]   | `ToLinqToDBTable<T>(DbSet<T>)` / `(DbSet<T>, IDataContext)`           | `ITable<T>`           | project `DbSet` to LINQ To DB table      |
|  [03]   | `GetTable<T>(context)`                                                | `ITable<T>`           | open a LINQ To DB table from the context |
|  [04]   | `Into<T>(context, ITable<T>)` `where T : notnull`                     | `IValueInsertable<T>` | start a LINQ To DB value-insert          |
|  [05]   | `GetMetadataReader(IModel?, IInfrastructure<IServiceProvider>?)`      | `IMetadataReader?`    | EF model -> LINQ To DB metadata reader   |
|  [06]   | `GetMappingSchema(IModel, accessor, DataOptions?)`                    | `MappingSchema`       | EF model -> LINQ To DB mapping schema    |

[ENTRYPOINT_SCOPE]: connection / context construction
- rail: store-provider

`CreateLinqToDBConnection` opens a `DataConnection` over the EF connection (optionally
enlisting an `IDbContextTransaction`); the detached form does not bind the EF change
tracker; `CreateLinqToDBContext` opens an `IDataContext`. `GetLinqToDBOptions` reads the
bridge `DataOptions` off the context.

| [INDEX] | [SURFACE]                                                       | [RETURNS]        | [CAPABILITY]                               |
| :-----: | :-------------------------------------------------------------- | :--------------- | :----------------------------------------- |
|  [01]   | `CreateLinqToDBConnection(DbContext[, IDbContextTransaction?])` | `DataConnection` | bridged connection, optional EF txn enlist |
|  [02]   | `CreateLinqToDBConnection(DbContextOptions)`                    | `DataConnection` | bridged connection from options            |
|  [03]   | `CreateLinqToDBConnectionDetached(DbContext)`                   | `DataConnection` | detached connection (no change tracker)    |
|  [04]   | `CreateLinqToDBContext(DbContext[, IDbContextTransaction?])`    | `IDataContext`   | bridged data context, optional txn enlist  |
|  [05]   | `GetLinqToDBOptions(DbContext)` / `(IDbContextOptions)`         | `DataOptions?`   | read bridge options off the context        |

[ENTRYPOINT_SCOPE]: async operators — dual engine mirror
- rail: store-provider

`*AsyncLinqToDB` forces the LINQ To DB engine on a lifted queryable; `*AsyncEF` forces EF.
The EF mirror includes predicate overloads, `ToDictionaryAsyncEF` (3 arities),
`ForEachAsyncEF`, `FirstOrDefaultAsyncEF`, and `SingleAsyncEF` — the full EF async operator
set, not a subset.

| [INDEX] | [SURFACE]                                                        | [ENGINE]   | [CAPABILITY]                     |
| :-----: | :--------------------------------------------------------------- | :--------- | :------------------------------- |
|  [01]   | `ToListAsyncLinqToDB` / `ToArrayAsyncLinqToDB`                   | LINQ To DB | materialize via LINQ To DB       |
|  [02]   | `FirstAsyncLinqToDB` / `CountAsyncLinqToDB` / `SumAsyncLinqToDB` | LINQ To DB | first / count / aggregate        |
|  [03]   | `ToListAsyncEF` / `ToArrayAsyncEF`                               | EF         | materialize via EF               |
|  [04]   | `ToDictionaryAsyncEF` (×3: keySel / +elemSel / +comparer)        | EF         | dictionary materialize via EF    |
|  [05]   | `FirstAsyncEF` / `FirstOrDefaultAsyncEF` (+predicate)            | EF         | first / first-or-default via EF  |
|  [06]   | `SingleAsyncEF` / `CountAsyncEF` / `ForEachAsyncEF`              | EF         | single / count / for-each via EF |

[ENTRYPOINT_SCOPE]: bridge admission and options
- rail: store-provider

`UseLinqToDB` installs `LinqToDBOptionsExtension` on the EF `DbContextOptionsBuilder` and
runs the supplied `Action<LinqToDBContextOptionsBuilder>` (interceptors, mapping schema,
custom `DataOptions`). `Initialize`/`ClearCaches` activate and reset the bridge's static
caches; `EnableChangeTracker` (static `bool`) gates whether lifted reads feed the EF change
tracker.
- note: `UseLinqToDB<TContext>` constrains `where TContext : DbContextOptionsBuilder`

| [INDEX] | [SURFACE]                                                                       | [CALL_SHAPE]      | [CAPABILITY]                      |
| :-----: | :------------------------------------------------------------------------------ | :---------------- | :-------------------------------- |
|  [01]   | `UseLinqToDB<TContext>(builder, Action<LinqToDBContextOptionsBuilder>?)`        | builder extension | admit bridge + configure options  |
|  [02]   | `LinqToDBContextOptionsBuilder.AddInterceptor(IInterceptor)`                    | fluent call       | admit a LINQ To DB interceptor    |
|  [03]   | `LinqToDBContextOptionsBuilder.AddMappingSchema(MappingSchema)`                 | fluent call       | admit a mapping schema            |
|  [04]   | `LinqToDBContextOptionsBuilder.AddCustomOptions(Func<DataOptions,DataOptions>)` | fluent call       | rewrite the `DataOptions`         |
|  [05]   | `LinqToDBForEFTools.Initialize()` / `ClearCaches()`                             | static call       | activate bridge / reset caches    |
|  [06]   | `LinqToDBForEFTools.EnableChangeTracker`                                        | static property   | gate change-tracker feed (`bool`) |

## [04]-[IMPLEMENTATION_LAW]

[STORE_PROFILE]:
- profile: the bridge is the EF-context hand-off onto the LINQ To DB bulk/query/merge engine
  over admitted EF store profiles
- bulk root: `BulkCopy`/`BulkCopyAsync` on the owning `DbContext`
- query root: `ToLinqToDB` lift over EF `IQueryable`, backed by `LinqToDBForEFQueryProvider<T>`
- connection root: `CreateLinqToDBConnection`/`CreateLinqToDBContext` from the owning context
- mapping root: `GetMappingSchema`/`GetMetadataReader` reuse the EF model's mapping

[CORE_ENGINE_HANDOFF]:
- The advanced store moves live in the `linq2db` `6.3.0` core assembly, reached through a
  bridged `IDataContext`/`DataConnection`, not re-declared here:
  - `LinqToDB.Data.DataContextExtensions.BulkCopyAsync<T>(IDataContext, BulkCopyOptions, IEnumerable<T>, CancellationToken) where T : class` -> `Task<BulkCopyRowsCopied>`.
  - `LinqToDB.LinqExtensions.MergeWithOutputAsync<TTarget,TSource,TOutput>(IMergeable<TTarget,TSource>, Expression<Func<string,TTarget,TTarget,TOutput>>) : IAsyncEnumerable<TOutput>` — the `(action, deleted, inserted)` projection; the `RETURNING`-streaming upsert.
- `LinqToDB.Data.BulkCopyOptions` is a `sealed record` carrying `BulkCopyType` (enum
  `Default`/`RowByRow`/`MultipleRows`/`ProviderSpecific`; `ProviderSpecific` = PostgreSQL
  binary COPY, `MultipleRows` = the multi-row-INSERT fallback), `MaxBatchSize`,
  `WithoutSession`, `MaxDegreeOfParallelism`, `KeepIdentity`, `TableLock`, `FireTriggers`,
  `ConflictAction`, `RowsCopiedCallback`, and a `with`-friendly `Default` singleton.
- `ConflictAction` (`Default`/…) drives the ON CONFLICT path for the merge/upsert lane.

[LOCAL_ADMISSION]:
- Bridge surfaces operate on contexts admitted through the store-profile algebra.
- Bulk-copy options and `BulkCopyRowsCopied` receipts are profile receipts.
- Bridge interceptors and mapping schemas require explicit profile declarations through
  `UseLinqToDB`'s `Action<LinqToDBContextOptionsBuilder>`.
- A lifted read defaults to NOT feeding the EF change tracker unless `EnableChangeTracker`
  is set; the detached connection is the change-tracker-free path for high-throughput reads.

[STACKING_LAW]:
- EF + Npgsql: the bridge sits over `Npgsql.EntityFrameworkCore.PostgreSQL` — `BulkCopy`
  with `BulkCopyType.ProviderSpecific` drives Npgsql binary COPY; geometry columns mapped by
  `api-nts-ef` and timestamp columns mapped by NodaTime survive the COPY path because the
  bridge reuses the EF model's mapping schema (`GetMappingSchema`).
- Sep ingress: `SepReaderExtensions.Enumerate(RowFunc<T>)`/`EnumerateAsync` (`api-sep`)
  produce the `IEnumerable<T>`/`IAsyncEnumerable<T>` source for `BulkCopy`/`BulkCopyAsync`,
  streaming a CSV file straight into PostgreSQL COPY.
- Query lanes: `ToLinqToDB` is the seam where `Query/lane` selects the LINQ To DB engine
  for a window-function/CTE/bulk-update query EF cannot translate, then re-materializes via
  `ToListAsyncLinqToDB`; `*AsyncEF` keeps the EF engine for change-tracked reads.
- Telemetry: bridged `DataConnection` activity flows through the core `linq2db`
  `ActivityService`, which the AppHost `telemetry` port / `api-npgsql-opentelemetry` span pipeline
  observes alongside the EF command interceptor.

[RAIL_LAW]:
- Package: `linq2db.EntityFrameworkCore`
- Owns: EF-to-LINQ-To-DB hand-off — bulk copy, query lift, value-insert/merge entry,
  connection/context construction, mapping/metadata reuse, dual-engine async operators
- Accept: bridge calls on admitted EF store profiles; `UseLinqToDB` admission with explicit
  interceptor/mapping declarations
- Reject: parallel query rails outside the store profile; re-declaring core `linq2db`
  COPY/merge surfaces here; change-tracked lifts where the detached/COPY path is required
