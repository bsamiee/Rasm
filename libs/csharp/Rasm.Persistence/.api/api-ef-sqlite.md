# [RASM_PERSISTENCE_API_EF_SQLITE]

`Microsoft.EntityFrameworkCore.Sqlite` admits the EF Core SQLite provider into the
unified store-profile algebra. The package is a meta-package: its `lib/net10.0`
asset is an empty `_._` placeholder, so it carries no managed types of its own —
it pulls the type-owning `Microsoft.EntityFrameworkCore.Sqlite.Core` assembly plus
`SQLitePCLRaw.bundle_e_sqlite3` and the EF relational/cache/logging closure. The
public consumer surface a design page composes is the `UseSqlite` admission family,
the `SqliteDbContextOptionsBuilder` knobs, and the `Sqlite*BuilderExtensions` model
configuration; the `Sqlite*` SQL-generation, type-mapping, translation, and migration
services are `[EntityFrameworkInternal]` provider internals, never a consumer rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.EntityFrameworkCore.Sqlite`
- package: `Microsoft.EntityFrameworkCore.Sqlite` (meta) → type-owner `Microsoft.EntityFrameworkCore.Sqlite.Core`
- assembly: `Microsoft.EntityFrameworkCore.Sqlite.dll` (ships in `.Core`; the meta-package's `lib/net10.0/_._` is empty)
- public namespaces: `Microsoft.EntityFrameworkCore` (extensions + builders), `Microsoft.Extensions.DependencyInjection` (service registration), `Microsoft.EntityFrameworkCore.Metadata` (`SqliteValueGenerationStrategy`), `Microsoft.EntityFrameworkCore.Migrations` (`SqliteMigrationsSqlGenerator`), `Microsoft.EntityFrameworkCore.Diagnostics` (`SqliteEventId`)
- internal namespaces (provider services, `[EntityFrameworkInternal]`): `...Sqlite.Storage.Internal`, `...Sqlite.Query.Internal`, `...Sqlite.Migrations.Internal`, `...Sqlite.Update.Internal`, `...Sqlite.Infrastructure.Internal`, `...Sqlite.Scaffolding.Internal`, `...Sqlite.Diagnostics.Internal`
- native floor: bundled `SQLitePCLRaw.bundle_e_sqlite3` (transitive `2.1.11`), but Persistence pins `3.0.3` centrally; the SQLCipher profile swaps it for `SQLite3Provider_sqlcipher`, keyed by the DEK the `Element/identity#KEY_ENVELOPE` `EnvelopeKeyring.Unwrap` recovers (the `PRAGMA key` rehydration)
- depends: `Microsoft.EntityFrameworkCore.Relational` (relational base), `Microsoft.Data.Sqlite.Core` (ADO surface — `SqliteConnection`/`SqliteConnectionStringBuilder`/`SqliteOpenMode`/`SqliteCacheMode` live there, not here)
- asset: provider admission and runtime library
- rail: store-provider

## [02]-[PUBLIC_TYPES]

[ADMISSION_TYPES]: provider admission and options — `Microsoft.EntityFrameworkCore`, `Microsoft.Extensions.DependencyInjection`
- rail: store-provider

| [INDEX] | [SYMBOL]                                  | [PACKAGE_ROLE]     | [CAPABILITY]                                                          |
| :-----: | :---------------------------------------- | :----------------- | :-------------------------------------------------------------------- |
|  [01]   | `SqliteDbContextOptionsBuilderExtensions` | builder extension  | the `UseSqlite` admission family (8 overloads)                        |
|  [02]   | `SqliteDbContextOptionsBuilder`           | provider options   | SQLite-scoped fluent knobs (`RelationalDbContextOptionsBuilder` base) |
|  [03]   | `SqliteServiceCollectionExtensions`       | service extension  | `AddSqlite<TContext>` / `AddEntityFrameworkSqlite`                    |
|  [04]   | `SqliteDatabaseFacadeExtensions`          | database extension | `IsSqlite()` provider probe on `DatabaseFacade`                       |

[MODEL_CONFIG_TYPES]: model-builder configuration extensions — `Microsoft.EntityFrameworkCore`
- rail: store-provider

| [INDEX] | [SYMBOL]                            | [PACKAGE_ROLE]      | [CAPABILITY]                                                              |
| :-----: | :---------------------------------- | :------------------ | :------------------------------------------------------------------------ |
|  [01]   | `SqlitePropertyBuilderExtensions`   | property extension  | `UseAutoincrement`, `HasSrid` (spatial), value-gen strategy               |
|  [02]   | `SqliteEntityTypeBuilderExtensions` | entity extension    | `UseSqlReturningClause` (RETURNING-clause toggle per entity/store-object) |
|  [03]   | `SqliteValueGenerationStrategy`     | metadata enum       | `None` \| `Autoincrement` generation classifier                           |
|  [04]   | `SqliteEventId`                     | diagnostics catalog | `EventId` constants for provider warnings/scaffolding                     |

[INTERNAL_SERVICES]: provider service implementations — `...Sqlite.*.Internal`, `[EntityFrameworkInternal]`
- rail: store-provider
- These resolve through the EF service provider via `UseSqlite`/`AddEntityFrameworkSqlite`; a consumer NEVER references them by type. Listed for decompile-traceability of the provider's capability, NOT as a composition surface — overriding one means `services.AddSingleton<I…, …>()` against the EF interface, not subclassing the `Internal` type.

| [INDEX] | [SYMBOL]                                                 | [INTERNAL_NAMESPACE]      | [SERVICE_ROLE]                   |
| :-----: | :------------------------------------------------------- | :------------------------ | :------------------------------- |
|  [01]   | `SqliteTypeMappingSource`                                | `Storage.Internal`        | CLR ⇄ SQLite type mapping        |
|  [02]   | `SqliteSqlGenerationHelper`                              | `Storage.Internal`        | identifier/literal SQL emission  |
|  [03]   | `SqliteQuerySqlGenerator`                                | `Query.Internal`          | SELECT SQL generation            |
|  [04]   | `SqliteUpdateSqlGenerator`                               | `Update.Internal`         | INSERT/UPDATE/DELETE SQL         |
|  [05]   | `SqliteMethodCallTranslatorProvider`                     | `Query.Internal`          | method-call → SQL translation    |
|  [06]   | `SqliteMemberTranslatorProvider`                         | `Query.Internal`          | member-access → SQL translation  |
|  [07]   | `SqliteRegexMethodTranslator`                            | `Query.Internal`          | `Regex.IsMatch` → `REGEXP`       |
|  [08]   | `SqliteQueryableAggregateMethodTranslator`               | `Query.Internal`          | `Sum`/`Avg`/`Min`/`Max` → SQL    |
|  [09]   | `SqliteDatabaseCreator`                                  | `Storage.Internal`        | `EnsureCreated`/`EnsureDeleted`  |
|  [10]   | `SqliteModelValidator`                                   | `Infrastructure.Internal` | store-model validation           |
|  [11]   | `SqliteOptionsExtension`                                 | `Infrastructure.Internal` | provider option carrier          |
|  [12]   | `SqliteMigrationsSqlGenerator` (public, `Migrations` ns) | `Migrations`              | migration → table-rebuild SQL    |
|  [13]   | `SqliteMigrationDatabaseLock`                            | `Migrations.Internal`     | migration mutual exclusion       |
|  [14]   | `SqliteDesignTimeServices`                               | `Scaffolding.Internal`    | design/scaffolding service entry |

[BASE_EF_TYPES]: base + relational EF surface the store/migration/query rails compose — `Microsoft.EntityFrameworkCore` / `.Relational`, NOT this provider package (the SQLite profile binds them through `UseSqlite`, like the existing `EF.CompileAsyncQuery` row). `[ASSEMBLY_NS]` abbreviates the `Microsoft.EntityFrameworkCore` root as `…`; each interceptor row pairs the `IXxx` contract with its `Xxx` base class, one capsule implementing all four. The `MigrationOperation` subtypes are `AddColumnOperation`/`AlterColumnOperation`/`DropColumnOperation`/`DropTableOperation`/`RenameColumnOperation`/`RenameTableOperation`/`SqlOperation`.
- rail: store-provider

| [INDEX] | [SYMBOL]                           | [ASSEMBLY_NS]            | [CAPABILITY]                                                         |
| :-----: | :--------------------------------- | :----------------------- | :------------------------------------------------------------------- |
|  [01]   | `PooledDbContextFactory<TContext>` | `…Infrastructure`        | `IDbContextFactory<TContext>`; pooled-context lease (`Run`/`Scoped`) |
|  [02]   | `IExecutionStrategy`               | `…Storage`               | retry strategy; `RetriesOnFailure`, `Execute`/`ExecuteAsync`         |
|  [03]   | `IDbContextTransaction`            | `…Storage`               | ambient txn; `SupportsSavepoints`, `Commit`/`RollbackAsync`          |
|  [04]   | `SaveChangesInterceptor`           | `…Diagnostics (core)`    | `SavingChanges(Async)`/`SavedChanges(Async)` hooks                   |
|  [05]   | `DbCommandInterceptor`             | `…Diagnostics (rel)`     | `ReaderExecuting(Async)`/`NonQueryExecuted` hooks                    |
|  [06]   | `DbConnectionInterceptor`          | `…Diagnostics (rel)`     | connection open/close interception hooks                             |
|  [07]   | `DbTransactionInterceptor`         | `…Diagnostics (rel)`     | transaction lifecycle interception hooks                             |
|  [08]   | `ModelConfigurationBuilder`        | `… (core)`               | `ConfigureConventions`; `Properties<T>`, `DefaultTypeMapping<T>`     |
|  [09]   | `IMigrator`                        | `…Migrations`            | migration driver; `GenerateScript`, `MigrateAsync`                   |
|  [10]   | `MigrationBuilder`                 | `…Migrations`            | op builder; `Sql(string, suppressTransaction)`, `ActiveProvider`     |
|  [11]   | `Migration`                        | `…Migrations`            | `ActiveProvider`, `Up`/`DownOperations`, `InitialDatabase` (`"0"`)   |
|  [12]   | `MigrationOperation`               | `…Migrations.Operations` | typed op; `IsDestructiveChange`; subtypes in the lead                |
|  [13]   | `MigrationsSqlGenerationOptions`   | `…Migrations`            | `[Flags]`: `Default=0`, `Idempotent=2`, `NoTransactions=4`           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider admission — `SqliteDbContextOptionsBuilderExtensions`, `SqliteServiceCollectionExtensions`. `UseSqlite`/`UseSqlite<TContext>` are builder extensions, `AddSqlite<TContext>`/`AddEntityFrameworkSqlite` service extensions, `IsSqlite` the database-extension probe.
- rail: store-provider

| [INDEX] | [SURFACE]                                                                              | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------------------------------------------- | :--------------------------------------------- |
|  [01]   | `UseSqlite(connectionString?, Action<SqliteDbContextOptionsBuilder>?)`                 | admits SQLite from a connection string         |
|  [02]   | `UseSqlite(DbConnection, [bool contextOwnsConnection,] Action<…>?)`                    | admits SQLite over a `SqliteConnection`        |
|  [03]   | `UseSqlite<TContext>(…)`                                                               | the four overloads above, typed-builder form   |
|  [04]   | `AddSqlite<TContext>(connectionString?, Action<…>?, Action<DbContextOptionsBuilder>?)` | registers a pooled context + provider          |
|  [05]   | `AddEntityFrameworkSqlite()`                                                           | registers the bare EF SQLite services          |
|  [06]   | `IsSqlite(this DatabaseFacade)`                                                        | provider discriminant, profile-agnostic branch |

[ENTRYPOINT_SCOPE]: SQLite-scoped options — `SqliteDbContextOptionsBuilder` (+ `RelationalDbContextOptionsBuilder` base)
- rail: store-provider

| [INDEX] | [SURFACE]                                        | [CALL_SHAPE]      | [CAPABILITY]                                     |
| :-----: | :----------------------------------------------- | :---------------- | :----------------------------------------------- |
|  [01]   | `MigrationsAssembly(string?\|Assembly)`          | relational option | selects the migration-owning assembly            |
|  [02]   | `MigrationsHistoryTable(table, schema?)`         | relational option | renames `__EFMigrationsHistory`                  |
|  [03]   | `CommandTimeout(int?)`                           | relational option | per-command timeout                              |
|  [04]   | `MinBatchSize(int)` / `MaxBatchSize(int)`        | relational option | SaveChanges batch bounds                         |
|  [05]   | `UseRelationalNulls(bool)`                       | relational option | C-style vs SQL three-valued null semantics       |
|  [06]   | `ExecutionStrategy(Func<…, IExecutionStrategy>)` | relational option | retry/resiliency strategy (busy-retry on SQLite) |
|  [07]   | `TranslateParameterizedCollectionsToConstants()` | relational option | inlines collection params as constants           |

[ENTRYPOINT_SCOPE]: model configuration — `SqlitePropertyBuilderExtensions`, `SqliteEntityTypeBuilderExtensions`. Rows [01]-[02] are property extensions, [03] a convention extension, [04] an entity extension, [05] the base-assembly compiled-query shape.
- rail: store-provider

| [INDEX] | [SURFACE]                                                                               | [CAPABILITY]                             |
| :-----: | :-------------------------------------------------------------------------------------- | :--------------------------------------- |
|  [01]   | `PropertyBuilder.UseAutoincrement()`                                                    | emits `AUTOINCREMENT` on the rowid key   |
|  [02]   | `PropertyBuilder.HasSrid(int)`                                                          | tags a spatial column SRID (NTS interop) |
|  [03]   | `IConventionPropertyBuilder.HasValueGenerationStrategy(SqliteValueGenerationStrategy?)` | sets the generation strategy             |
|  [04]   | `IConventionEntityTypeBuilder.UseSqlReturningClause(bool?, [StoreObjectIdentifier])`    | toggles the SQLite RETURNING-clause path |
|  [05]   | `EF.CompileQuery` / `EF.CompileAsyncQuery`                                              | hot projection delegate (`TParam15`)     |

`UseSqlite(...)`: 8 overloads total — `{connectionString | DbConnection | DbConnection+contextOwnsConnection | parameterless}` × `{DbContextOptionsBuilder | DbContextOptionsBuilder<TContext>}`. The `Action<SqliteDbContextOptionsBuilder>` callback is where the relational + model options above bind. `EF.CompileAsyncQuery<TContext,TResult>` returns `Func<TContext,IAsyncEnumerable<TResult>>` and lives in the base `Microsoft.EntityFrameworkCore` assembly — it is provider-agnostic, listed here only because the SQLite profile's hot read lanes compile against it.

[ENTRYPOINT_SCOPE]: base EF execution + interception — `Microsoft.EntityFrameworkCore` (core), provider-agnostic, composed by the store/query rails. Row [03] `operation` is `Func<DbContext,TState,CancellationToken,Task<TResult>>` and `verifySucceeded` is `Func<…,Task<ExecutionResult<TResult>>>?`; `PooledDbContextFactory` ctors are `(IDbContextPool<TContext>)` / `(DbContextOptions<TContext>, int poolSize = 1024)`. The execution strategy is SQLite busy-retry, PG `NpgsqlRetryingExecutionStrategy`. Rows [04]-[05] are `ISaveChangesInterceptor` hooks, shown bare.
- rail: store-provider

| [INDEX] | [SURFACE]                                                                                 | [CAPABILITY]                         |
| :-----: | :---------------------------------------------------------------------------------------- | :----------------------------------- |
|  [01]   | `PooledDbContextFactory<TContext>.CreateDbContextAsync(ct)` / `CreateDbContext()`         | one pooled `DbContext` per `StoreOp` |
|  [02]   | `DatabaseFacade.CreateExecutionStrategy()`                                                | builds the provider strategy         |
|  [03]   | `IExecutionStrategy.ExecuteAsync<TState,TResult>(state, operation, verifySucceeded?, ct)` | wraps the unit of work in retry      |
|  [04]   | `SavingChangesAsync(DbContextEventData, InterceptionResult<int>, ct)`                     | before-save pipeline hook            |
|  [05]   | `SavedChangesAsync(SaveChangesCompletedEventData, int, ct)`                               | after-save pipeline hook             |
|  [06]   | `DbContextOptionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior)`                 | sets the default query-tracking lane |
|  [07]   | `DbContextOptionsBuilder.UseSeeding(Action<…>)` / `UseAsyncSeeding(Func<…>)`              | pooled-factory seed delegate         |

[ENTRYPOINT_SCOPE]: base relational facade, transaction, and queryable — `Microsoft.EntityFrameworkCore.Relational` (`RelationalDatabaseFacadeExtensions`) + `Microsoft.EntityFrameworkCore` (`EntityFrameworkQueryableExtensions`), composed by the query/transaction rails. `RelationalDatabaseFacadeExtensions` (`SqlQuery`/`ExecuteSql*`) and `EntityFrameworkQueryableExtensions` terminators show bare; `SqlQuery` is the keyless `(key, fused)` RRF read, never the entity-only `FromSqlRaw`, and the parameterized `ExecuteSqlAsync` carries the tenant id, the raw form does not.
- rail: store-provider

| [INDEX] | [SURFACE]                                                                                     | [CAPABILITY]                         |
| :-----: | :-------------------------------------------------------------------------------------------- | :----------------------------------- |
|  [01]   | `DatabaseFacade.BeginTransactionAsync(IsolationLevel, ct)` → `IDbContextTransaction`          | opens an isolation-typed transaction |
|  [02]   | `DatabaseFacade.CurrentTransaction`                                                           | the ambient transaction              |
|  [03]   | `IDbContextTransaction.{CreateSavepoint,RollbackToSavepoint,ReleaseSavepoint}Async(name, ct)` | nested savepoint mechanism           |
|  [04]   | `IDbContextTransaction.SupportsSavepoints`                                                    | savepoint support probe              |
|  [05]   | `SqlQuery<TResult>(FormattableString)` → `IQueryable<TResult>`                                | keyless RRF projection read          |
|  [06]   | `SqlQueryRaw<TResult>(string, params object[])` → `IQueryable<TResult>`                       | raw keyless projection read          |
|  [07]   | `ExecuteSqlAsync(FormattableString, ct)` → `Task<int>`                                        | parameterized DDL/DCL statement      |
|  [08]   | `ExecuteSqlRawAsync(string, [params object[] \| IEnumerable<object>], ct)` → `Task<int>`      | raw-string DDL/DCL statement         |
|  [09]   | `{ToList,Single,FirstOrDefault}Async<T>(ct)` / `AsNoTracking<T>()`                            | materializes a `SqlQuery`/LINQ read  |

[ENTRYPOINT_SCOPE]: base migration + model-config surface — `Microsoft.EntityFrameworkCore.Relational` (migrations) + `Microsoft.EntityFrameworkCore` (`ModelConfigurationBuilder`), composed by the schema/migration rails. Row [06] subtypes are rostered in `[BASE_EF_TYPES]`; `HasComputedColumnSql` carries `<TProperty>` overloads and drives the `DerivedColumn` rows; `HasCheckConstraint` binds via `builder.ToTable(t => …)`, mirrors on `RelationalEntityTypeBuilderExtensions`, and emits the `ServerExtension` `CreateSql`/quality `CHECK`; `ModelConfigurationBuilder` is the `DbContext.ConfigureConventions(…)` parameter the converter rail folds text converters through.
- rail: store-provider

| [INDEX] | [SURFACE]                                                                                           | [CAPABILITY]                      |
| :-----: | :-------------------------------------------------------------------------------------------------- | :-------------------------------- |
|  [01]   | `IMigrator.GenerateScript(fromMigration?, toMigration?, MigrationsSqlGenerationOptions = Default)`  | deploy-SQL emission               |
|  [02]   | `IMigrator.MigrateAsync(targetMigration?, ct)`                                                      | runtime migration apply           |
|  [03]   | `MigrationBuilder.Sql(string, bool suppressTransaction = false)` → `OperationBuilder<SqlOperation>` | lock-light step SQL fold target   |
|  [04]   | `MigrationBuilder.ActiveProvider`                                                                   | keys provider-aware emission      |
|  [05]   | `Migration.ActiveProvider` / `UpOperations` / `DownOperations` / `InitialDatabase` (`"0"`)          | materialized-migration plan input |
|  [06]   | `MigrationOperation.IsDestructiveChange`                                                            | per-op destructive stamp          |
|  [07]   | `RelationalPropertyBuilderExtensions.HasComputedColumnSql(PropertyBuilder, sql?, stored?)`          | stored/virtual generated column   |
|  [08]   | `TableBuilder.HasCheckConstraint(string name, string? sql)` → `CheckConstraintBuilder`              | table-scoped `CHECK`              |
|  [09]   | `ModelConfigurationBuilder`                                                                         | the `ConfigureConventions` param  |

## [04]-[IMPLEMENTATION_LAW]

[STORE_PROFILE]:
- profile: SQLite is the `sqlite-embedded` and `sqlite-memory` arms of the `Store/provisioning#PROFILE_AXIS` `StoreProfile` `[SmartEnum<string>]`
- admission root: `StoreRows.SqliteOptions` calls `options.UseSqlite(SqliteText(placement))`; `StoreRows.Sqlite` opens a raw `SqliteConnection` for the open-ceremony bracket
- connection shape: `SqliteConnectionStringBuilder` (in `Microsoft.Data.Sqlite.Core`) sets `Pooling=true`, `Mode = SqliteOpenMode.{ReadWriteCreate | ReadOnly | Memory}`, `Cache = SqliteCacheMode.Shared` for the memory profile
- model root: EF relational model plus SQLite annotations (`UseAutoincrement`, `HasSrid`, `UseSqlReturningClause`)
- migration root: `SqliteMigrationsSqlGenerator` (provider-internal) emits the table-rebuild dance for unsupported `ALTER`; the migration lock outcome reads from migration receipts, never from `Internal`-namespace types
- query root: SQLite method/member/aggregate translation including `SqliteRegexMethodTranslator` so `Regex.IsMatch` projects to the `REGEXP` operator instead of client evaluation (`Query/retrieval#FUSION_AND_REUSE`)

[LOCAL_ADMISSION]:
- SQLite enters through the unified store-profile algebra — `UseSqlite` is called once inside `StoreRows.SqliteOptions`, never scattered, and the provider-specific knobs stay `StoreProfile` row data and never become public service families.
- The ADO-level open ceremony (Batteries_V2 init, `PRAGMA` ladder, writer-lease, `MigrateAsync`, fingerprint, `quick_check`) runs on a raw `SqliteConnection` from `Microsoft.Data.Sqlite` (`api-sqlite.md`), NOT through this EF package — this package owns only the EF provider admission and the LINQ→SQL translation that the `DbContext` lanes ride.
- Migration, model, query, and update behavior share the one provider rail; `SqliteValueGenerationStrategy.Autoincrement` and `UseSqlReturningClause` are the two SQLite-specific model knobs the schema layer sets.
- `SqliteEventId` is the real diagnostic surface (e.g. `TableRebuildPendingWarning`, `CompositeKeyWithValueGeneration`, `ConflictingValueGenerationStrategiesWarning`) that the EF interception/logging path raises; the cross-cutting `SavingChangesAsync`/`ReaderExecutedAsync`/`ConnectionOpenedAsync` interception hooks are members of the base-assembly interceptor base classes (`SaveChangesInterceptor`/`DbCommandInterceptor`/`DbConnectionInterceptor` in `Microsoft.EntityFrameworkCore.Diagnostics`), registered once on the `DbContextOptionsBuilder` and shared across every provider — they are not `Sqlite*` types and are owned at the interception seam, not here.
- The seed delegate enters EF through the `UseSeeding`/`UseAsyncSeeding` option hooks at pooled-factory build (`Store/provisioning` `StoreRows.SeedOnCreate`).
- SQLite cannot define Persistence vocabulary by itself; it is one engine row whose capability columns (`vector:false`, `fullText:true`, `migrations:true`) gate which lanes a profile admits.

[RAIL_LAW]:
- Package: `Microsoft.EntityFrameworkCore.Sqlite` (admission meta) / `.Core` (type owner)
- Owns: EF SQLite provider admission and the LINQ→SQLite translation for the `sqlite-embedded`/`sqlite-memory` profiles
- Accept: the `UseSqlite` admission family, the relational + SQLite-scoped option knobs, `SqlitePropertyBuilderExtensions`/`SqliteEntityTypeBuilderExtensions` model config, `IsSqlite` discrimination, `SqliteEventId` diagnostics, the base-assembly `EF.CompileAsyncQuery` for hot lanes, and the composed base/relational EF surface the store rails bind through this provider (`PooledDbContextFactory`, the interceptor family, `IExecutionStrategy`, `IDbContextTransaction` savepoints, `RelationalDatabaseFacadeExtensions` `SqlQuery`/`ExecuteSql*`, `EntityFrameworkQueryableExtensions`, `ModelConfigurationBuilder`, the `IMigrator`/`Migration`/`MigrationBuilder`/`MigrationsSqlGenerationOptions` migration API, `HasComputedColumnSql`/`HasCheckConstraint`)
- Reject: referencing `...Sqlite.*.Internal` provider services by type; treating the meta-package as the assembly home; a SQLite-first public service family; the raw-`SqliteConnection` open ceremony (owned by `Microsoft.Data.Sqlite`); reading the migration lock from `Internal`-namespace types
