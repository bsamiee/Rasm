# [RASM_PERSISTENCE_API_EF_SQLITE]

`Microsoft.EntityFrameworkCore.Sqlite` admits the EF Core SQLite provider into the
unified store-profile algebra. The package is a **meta-package**: its `lib/net10.0`
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
- version: `10.0.9` (`Directory.Packages.props`, whole EF family on the 10.0.9 line); license: MIT (.NET Foundation)
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

| [INDEX] | [SYMBOL]                                  | [PACKAGE_ROLE]     | [CAPABILITY]                                      |
| :-----: | :---------------------------------------- | :----------------- | :------------------------------------------------ |
|  [01]   | `SqliteDbContextOptionsBuilderExtensions` | builder extension  | the `UseSqlite` admission family (8 overloads)     |
|  [02]   | `SqliteDbContextOptionsBuilder`           | provider options   | SQLite-scoped fluent knobs (`RelationalDbContextOptionsBuilder` base) |
|  [03]   | `SqliteServiceCollectionExtensions`       | service extension  | `AddSqlite<TContext>` / `AddEntityFrameworkSqlite` |
|  [04]   | `SqliteDatabaseFacadeExtensions`          | database extension | `IsSqlite()` provider probe on `DatabaseFacade`    |

[MODEL_CONFIG_TYPES]: model-builder configuration extensions — `Microsoft.EntityFrameworkCore`
- rail: store-provider

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]      | [CAPABILITY]                                      |
| :-----: | :-------------------------------- | :------------------ | :------------------------------------------------ |
|  [01]   | `SqlitePropertyBuilderExtensions` | property extension  | `UseAutoincrement`, `HasSrid` (spatial), value-gen strategy |
|  [02]   | `SqliteEntityTypeBuilderExtensions` | entity extension  | `UseSqlReturningClause` (RETURNING-clause toggle per entity/store-object) |
|  [03]   | `SqliteValueGenerationStrategy`   | metadata enum       | `None` \| `Autoincrement` generation classifier   |
|  [04]   | `SqliteEventId`                   | diagnostics catalog | `EventId` constants for provider warnings/scaffolding |

[INTERNAL_SERVICES]: provider service implementations — `...Sqlite.*.Internal`, `[EntityFrameworkInternal]`
- rail: store-provider
- These resolve through the EF service provider via `UseSqlite`/`AddEntityFrameworkSqlite`; a consumer NEVER references them by type. Listed for decompile-traceability of the provider's capability, NOT as a composition surface — overriding one means `services.AddSingleton<I…, …>()` against the EF interface, not subclassing the `Internal` type.

| [INDEX] | [SYMBOL]                                | [INTERNAL_NAMESPACE]      | [SERVICE_ROLE]                  |
| :-----: | :-------------------------------------- | :------------------------ | :------------------------------ |
|  [01]   | `SqliteTypeMappingSource`               | `Storage.Internal`        | CLR ⇄ SQLite type mapping        |
|  [02]   | `SqliteSqlGenerationHelper`             | `Storage.Internal`        | identifier/literal SQL emission  |
|  [03]   | `SqliteQuerySqlGenerator`               | `Query.Internal`          | SELECT SQL generation            |
|  [04]   | `SqliteUpdateSqlGenerator`              | `Update.Internal`         | INSERT/UPDATE/DELETE SQL         |
|  [05]   | `SqliteMethodCallTranslatorProvider`    | `Query.Internal`          | method-call → SQL translation    |
|  [06]   | `SqliteMemberTranslatorProvider`        | `Query.Internal`          | member-access → SQL translation  |
|  [07]   | `SqliteRegexMethodTranslator`           | `Query.Internal`          | `Regex.IsMatch` → `REGEXP`       |
|  [08]   | `SqliteQueryableAggregateMethodTranslator` | `Query.Internal`       | `Sum`/`Avg`/`Min`/`Max` → SQL    |
|  [09]   | `SqliteDatabaseCreator`                 | `Storage.Internal`        | `EnsureCreated`/`EnsureDeleted`  |
|  [10]   | `SqliteModelValidator`                  | `Infrastructure.Internal` | store-model validation           |
|  [11]   | `SqliteOptionsExtension`                | `Infrastructure.Internal` | provider option carrier          |
|  [12]   | `SqliteMigrationsSqlGenerator` (public, `Migrations` ns) | `Migrations`     | migration → table-rebuild SQL    |
|  [13]   | `SqliteMigrationDatabaseLock`           | `Migrations.Internal`     | migration mutual exclusion       |
|  [14]   | `SqliteDesignTimeServices`              | `Scaffolding.Internal`    | design/scaffolding service entry |

[BASE_EF_TYPES]: base + relational EF surface the store/migration/query rails compose — `Microsoft.EntityFrameworkCore` / `.Relational`, NOT this provider package (the SQLite profile binds them through `UseSqlite`, like the existing `EF.CompileAsyncQuery` row)
- rail: store-provider

| [INDEX] | [SYMBOL]                          | [ASSEMBLY/NS]                                  | [CAPABILITY]                                                                 |
| :-----: | :-------------------------------- | :--------------------------------------------- | :-------------------------------------------------------------------------- |
|  [01]   | `PooledDbContextFactory<TContext>` | `Microsoft.EntityFrameworkCore.Infrastructure` | `IDbContextFactory<TContext>`; pooled-context lease (`Run`/`Scoped` bracket) |
|  [02]   | `IExecutionStrategy`              | `…Storage`                                      | retry/resiliency strategy; `RetriesOnFailure`, `Execute`/`ExecuteAsync`      |
|  [03]   | `IDbContextTransaction`           | `…Storage`                                      | ambient transaction; `SupportsSavepoints`, savepoint verbs, `CommitAsync`/`RollbackAsync` |
|  [04]   | `ISaveChangesInterceptor` / `SaveChangesInterceptor` | `…Diagnostics` (core) | save-pipeline interceptor contract + base class (`SavingChanges(Async)`/`SavedChanges(Async)`) |
|  [05]   | `IDbCommandInterceptor` / `DbCommandInterceptor` | `…Diagnostics` (relational) | command interceptor contract + base class (`ReaderExecuting(Async)`/`NonQueryExecuted` etc.) |
|  [06]   | `IDbConnectionInterceptor` / `DbConnectionInterceptor` | `…Diagnostics` (relational) | connection interceptor contract + base class |
|  [07]   | `IDbTransactionInterceptor` / `DbTransactionInterceptor` | `…Diagnostics` (relational) | transaction interceptor contract + base class (one capsule can implement all four) |
|  [08]   | `ModelConfigurationBuilder`       | `Microsoft.EntityFrameworkCore`                | the `ConfigureConventions` surface; `Conventions`, `Properties<T>`, `DefaultTypeMapping<T>`, `IgnoreAny<T>` |
|  [09]   | `IMigrator`                       | `…Migrations`                                   | migration driver; `GenerateScript`, `MigrateAsync` |
|  [10]   | `MigrationBuilder`                | `…Migrations`                                   | imperative op builder; `Sql(string, suppressTransaction)`, `ActiveProvider` |
|  [11]   | `Migration`                       | `…Migrations`                                   | materialized migration; `ActiveProvider`, `UpOperations`/`DownOperations`, `InitialDatabase` (`"0"`) |
|  [12]   | `MigrationOperation` (+ `Operations.*`) | `…Migrations.Operations`                  | typed op; `IsDestructiveChange`; subtypes `AddColumnOperation`/`AlterColumnOperation`/`DropColumnOperation`/`DropTableOperation`/`RenameColumnOperation`/`RenameTableOperation`/`SqlOperation` |
|  [13]   | `MigrationsSqlGenerationOptions`  | `…Migrations`                                   | `[Flags]` enum: `Default = 0`, `Idempotent = 2`, `NoTransactions = 4`        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider admission — `SqliteDbContextOptionsBuilderExtensions`, `SqliteServiceCollectionExtensions`
- rail: store-provider

| [INDEX] | [SURFACE]                                                              | [CALL_SHAPE]       | [CAPABILITY]                                  |
| :-----: | :--------------------------------------------------------------------- | :----------------- | :-------------------------------------------- |
|  [01]   | `UseSqlite(connectionString?, Action<SqliteDbContextOptionsBuilder>?)` | builder extension  | admits SQLite from a connection string        |
|  [02]   | `UseSqlite(DbConnection, [bool contextOwnsConnection,] Action<…>?)`    | builder extension  | admits SQLite over a pre-built `SqliteConnection` |
|  [03]   | `UseSqlite<TContext>(…)`                                               | builder extension  | the four overloads above, typed-builder form  |
|  [04]   | `AddSqlite<TContext>(connectionString?, Action<…>?, Action<DbContextOptionsBuilder>?)` | service extension | registers a pooled context + provider |
|  [05]   | `AddEntityFrameworkSqlite()`                                          | service extension  | registers the bare EF SQLite services         |
|  [06]   | `IsSqlite(this DatabaseFacade)`                                        | database extension | provider discriminant for a profile-agnostic branch |

[ENTRYPOINT_SCOPE]: SQLite-scoped options — `SqliteDbContextOptionsBuilder` (+ `RelationalDbContextOptionsBuilder` base)
- rail: store-provider

| [INDEX] | [SURFACE]                                  | [CALL_SHAPE]        | [CAPABILITY]                                  |
| :-----: | :----------------------------------------- | :------------------ | :-------------------------------------------- |
|  [01]   | `MigrationsAssembly(string?\|Assembly)`    | relational option   | selects the migration-owning assembly         |
|  [02]   | `MigrationsHistoryTable(table, schema?)`   | relational option   | renames `__EFMigrationsHistory`               |
|  [03]   | `CommandTimeout(int?)`                      | relational option   | per-command timeout                           |
|  [04]   | `MinBatchSize(int)` / `MaxBatchSize(int)`  | relational option   | SaveChanges batch bounds                       |
|  [05]   | `UseRelationalNulls(bool)`                  | relational option   | C-style vs SQL three-valued null semantics    |
|  [06]   | `ExecutionStrategy(Func<…, IExecutionStrategy>)` | relational option | retry/resiliency strategy (busy-retry on SQLite) |
|  [07]   | `TranslateParameterizedCollectionsToConstants()` | relational option | inlines collection params as constants  |

[ENTRYPOINT_SCOPE]: model configuration — `SqlitePropertyBuilderExtensions`, `SqliteEntityTypeBuilderExtensions`
- rail: store-provider

| [INDEX] | [SURFACE]                                                  | [CALL_SHAPE]        | [CAPABILITY]                                  |
| :-----: | :--------------------------------------------------------- | :------------------ | :-------------------------------------------- |
|  [01]   | `PropertyBuilder.UseAutoincrement()`                       | property extension  | emits `AUTOINCREMENT` on the rowid key        |
|  [02]   | `PropertyBuilder.HasSrid(int)`                             | property extension  | tags a spatial column SRID (NTS interop)      |
|  [03]   | `IConventionPropertyBuilder.HasValueGenerationStrategy(SqliteValueGenerationStrategy?)` | convention extension | sets the generation strategy |
|  [04]   | `IConventionEntityTypeBuilder.UseSqlReturningClause(bool?, [StoreObjectIdentifier])` | entity extension | toggles the SQLite RETURNING-clause SaveChanges path |
|  [05]   | `EF.CompileQuery` / `EF.CompileAsyncQuery` (base `Microsoft.EntityFrameworkCore`, NOT this package) | compiled shape | caches a hot projection delegate; parameterized through `TParam15` |

`UseSqlite(...)`: 8 overloads total — `{connectionString | DbConnection | DbConnection+contextOwnsConnection | parameterless}` × `{DbContextOptionsBuilder | DbContextOptionsBuilder<TContext>}`. The `Action<SqliteDbContextOptionsBuilder>` callback is where the relational + model options above bind. `EF.CompileAsyncQuery<TContext,TResult>` returns `Func<TContext,IAsyncEnumerable<TResult>>` and lives in the base `Microsoft.EntityFrameworkCore` assembly — it is provider-agnostic, listed here only because the SQLite profile's hot read lanes compile against it.

[ENTRYPOINT_SCOPE]: base EF execution + interception — `Microsoft.EntityFrameworkCore` (core), provider-agnostic, composed by the store/query rails
- rail: store-provider

| [INDEX] | [SURFACE]                                                                 | [CALL_SHAPE]    | [CAPABILITY]                                                            |
| :-----: | :------------------------------------------------------------------------ | :-------------- | :--------------------------------------------------------------------- |
|  [01]   | `PooledDbContextFactory<TContext>.CreateDbContextAsync(ct)` / `CreateDbContext()` | pooled lease | leases one pooled `DbContext` per `StoreOp` bracket; ctors `(IDbContextPool<TContext>)` / `(DbContextOptions<TContext>, int poolSize = 1024)` |
|  [02]   | `DatabaseFacade.CreateExecutionStrategy()`                                | strategy        | builds the provider `IExecutionStrategy` (busy-retry on SQLite, `NpgsqlRetryingExecutionStrategy` on PG) |
|  [03]   | `IExecutionStrategy.ExecuteAsync<TState,TResult>(state, Func<DbContext,TState,CancellationToken,Task<TResult>> operation, Func<…,Task<ExecutionResult<TResult>>>? verifySucceeded, ct)` | resilient run | wraps the unit of work in the retry envelope |
|  [04]   | `ISaveChangesInterceptor.SavingChangesAsync(DbContextEventData, InterceptionResult<int>, ct)` / `SavedChangesAsync(SaveChangesCompletedEventData, int, ct)` | interceptor hook | the save-pipeline hooks a registered interceptor capsule overrides |
|  [05]   | `DbContextOptionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior)` | options chain   | chains off `UseSqlite`/`UseNpgsql` to set the default tracking lane     |
|  [06]   | `DbContextOptionsBuilder.UseSeeding(Action<DbContext,bool>)` / `UseAsyncSeeding(Func<DbContext,bool,CancellationToken,Task>)` | options hook | the pooled-factory seed delegate (`StoreRows.SeedOnCreate`) |

[ENTRYPOINT_SCOPE]: base relational facade, transaction, and queryable — `Microsoft.EntityFrameworkCore.Relational` (`RelationalDatabaseFacadeExtensions`) + `Microsoft.EntityFrameworkCore` (`EntityFrameworkQueryableExtensions`), composed by the query/transaction rails
- rail: store-provider

| [INDEX] | [SURFACE]                                                                 | [CALL_SHAPE]    | [CAPABILITY]                                                            |
| :-----: | :------------------------------------------------------------------------ | :-------------- | :--------------------------------------------------------------------- |
|  [01]   | `DatabaseFacade.BeginTransactionAsync(IsolationLevel, ct)` → `IDbContextTransaction` / `DatabaseFacade.CurrentTransaction` | transaction | opens an isolation-typed transaction; the ambient one the savepoint legs read |
|  [02]   | `IDbContextTransaction.CreateSavepointAsync(name, ct)` / `RollbackToSavepointAsync(name, ct)` / `ReleaseSavepointAsync(name, ct)` / `SupportsSavepoints` | savepoint | nested savepoint mechanism for the composed-op `RollbackTo` continuation |
|  [03]   | `RelationalDatabaseFacadeExtensions.SqlQuery<TResult>(FormattableString)` / `SqlQueryRaw<TResult>(string, params object[])` → `IQueryable<TResult>` | keyless read | the keyless projection read (`(key, fused)` RRF, clash-hit scalar) — never the entity-only `FromSqlRaw` |
|  [04]   | `RelationalDatabaseFacadeExtensions.ExecuteSqlAsync(FormattableString, ct)` / `ExecuteSqlRawAsync(string, [params object[] \| IEnumerable<object>], ct)` → `Task<int>` | raw DDL/DCL | the parameterized (`ExecuteSqlAsync`) vs raw-string (`ExecuteSqlRawAsync`) statement forms; tenant id rides the parameterizing form only |
|  [05]   | `EntityFrameworkQueryableExtensions.ToListAsync<T>(ct)` / `SingleAsync<T>(ct)` / `FirstOrDefaultAsync<T>(ct)` / `AsNoTracking<T>()` | async terminator | materializes a `SqlQuery`/LINQ projection on the read lane |

[ENTRYPOINT_SCOPE]: base migration + model-config surface — `Microsoft.EntityFrameworkCore.Relational` (migrations) + `Microsoft.EntityFrameworkCore` (`ModelConfigurationBuilder`), composed by the schema/migration rails
- rail: store-provider

| [INDEX] | [SURFACE]                                                                 | [CALL_SHAPE]    | [CAPABILITY]                                                            |
| :-----: | :------------------------------------------------------------------------ | :-------------- | :--------------------------------------------------------------------- |
|  [01]   | `IMigrator.GenerateScript(string? fromMigration = null, string? toMigration = null, MigrationsSqlGenerationOptions = Default)` | script emit | the deploy-SQL emission the `DeployVehicle` `Script` fold drives; `MigrateAsync(targetMigration?, ct)` for runtime apply |
|  [02]   | `MigrationBuilder.Sql(string, bool suppressTransaction = false)` → `OperationBuilder<SqlOperation>` / `MigrationBuilder.ActiveProvider` | op emit | the lock-light step SQL fold target; `ActiveProvider` keys provider-aware emission |
|  [03]   | `Migration.ActiveProvider` / `UpOperations` / `DownOperations` / `Migration.InitialDatabase` (`"0"`) | plan input | the materialized migration the wave/disposition fold classifies |
|  [04]   | `MigrationOperation.IsDestructiveChange` (+ `Operations.{AddColumnOperation,AlterColumnOperation,DropColumnOperation,DropTableOperation,RenameColumnOperation,RenameTableOperation,SqlOperation}`) | op classify | EF's per-op destructive stamp the disposition table refines |
|  [05]   | `RelationalPropertyBuilderExtensions.HasComputedColumnSql(this PropertyBuilder, string? sql, bool? stored)` (+ `<TProperty>` overloads) | generated col | the stored/virtual generated-column expression the `DerivedColumn` rows emit |
|  [06]   | `TableBuilder.HasCheckConstraint(string name, string? sql)` (via `builder.ToTable(t => …)`; mirror on `RelationalEntityTypeBuilderExtensions`) → `CheckConstraintBuilder` | check | the table-scoped `CHECK` the `ServerExtension` `CreateSql`/quality rows emit |
|  [07]   | `ModelConfigurationBuilder` (the `DbContext.ConfigureConventions(ModelConfigurationBuilder)` parameter) | conventions | the provider-blind convention surface the converter rail folds text converters through |

## [04]-[IMPLEMENTATION_LAW]

[STORE_PROFILE]:
- profile: SQLite is the `sqlite-embedded` and `sqlite-memory` arms of the `Store/provisioning#PROFILE_AXIS` `StoreProfile` `[SmartEnum<string>]`
- admission root: `StoreRows.SqliteOptions` calls `options.UseSqlite(SqliteText(placement))`; `StoreRows.Sqlite` opens a raw `SqliteConnection` for the open-ceremony bracket
- connection shape: `SqliteConnectionStringBuilder` (in `Microsoft.Data.Sqlite.Core`) sets `Pooling=true`, `Mode = SqliteOpenMode.{ReadWriteCreate | ReadOnly | Memory}`, `Cache = SqliteCacheMode.Shared` for the memory profile
- model root: EF relational model plus SQLite annotations (`UseAutoincrement`, `HasSrid`, `UseSqlReturningClause`)
- migration root: `SqliteMigrationsSqlGenerator` (provider-internal) emits the table-rebuild dance for unsupported `ALTER`; the migration lock outcome reads from migration receipts, never from `Internal`-namespace types
- query root: SQLite method/member/aggregate translation including `SqliteRegexMethodTranslator` so `Regex.IsMatch` projects to the `REGEXP` operator instead of client evaluation (`Query/lane#FUSION_AND_CACHE`)

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
