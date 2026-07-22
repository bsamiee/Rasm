# [RASM_PERSISTENCE_API_EF_SQLITE]

`Microsoft.EntityFrameworkCore.Sqlite` admits the EF Core SQLite provider into the store-profile algebra: one `UseSqlite` call binds the provider, and every SQLite-scoped knob, model annotation, and LINQ→SQL translation attaches to the one options builder it hands back. This catalog also carries the base EF Core and relational runtime — pooled context, execution strategy, interception, transaction savepoints, raw-SQL reads, and the migration API — the folder's store rails compose through that binding.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.EntityFrameworkCore.Sqlite`
- package: `Microsoft.EntityFrameworkCore.Sqlite` (`MIT`, Microsoft)
- assembly: `Microsoft.EntityFrameworkCore.Sqlite` — types ship in the `Microsoft.EntityFrameworkCore.Sqlite.Core` package; this meta-package's own `lib/net10.0/_._` asset holds no managed type
- namespace: `Microsoft.EntityFrameworkCore` and its `.Infrastructure`, `.Metadata`, `.Metadata.Conventions`, `.Migrations`, `.Diagnostics` children; `Microsoft.Extensions.DependencyInjection`
- depends: `Microsoft.EntityFrameworkCore.Relational`, `Microsoft.Data.Sqlite.Core` (`api-sqlite` owns the ADO surface), `SQLitePCLRaw.bundle_e_sqlite3` (`api-sqlitepcl` owns the native provider)
- rail: store-provider

## [02]-[PUBLIC_TYPES]

[ADMISSION_TYPES]: provider admission, options, and diagnostics

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :---------------------------------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `SqliteDbContextOptionsBuilderExtensions`       | class         | the `UseSqlite` admission family                    |
|  [02]   | `SqliteDbContextOptionsBuilder`                 | class         | option handle; every knob rides the relational base |
|  [03]   | `SqliteServiceCollectionExtensions`             | class         | `AddSqlite`/`AddEntityFrameworkSqlite` registration |
|  [04]   | `SqliteDatabaseFacadeExtensions`                | class         | `IsSqlite` provider probe                           |
|  [05]   | `SpatialiteLoader`                              | class         | loads `mod_spatialite` onto a `DbConnection`        |
|  [06]   | `SqliteEventId`                                 | class         | provider `EventId` catalog                          |
|  [07]   | `ConflictingValueGenerationStrategiesEventData` | class         | payload of the conflicting-strategy warning         |

[SqliteEventId]: `SchemaConfiguredWarning` `SequenceConfiguredWarning` `CompositeKeyWithValueGeneration` `ConflictingValueGenerationStrategiesWarning` `UnexpectedConnectionTypeWarning` `TableRebuildPendingWarning`, and the scaffolding `*Found`/`*Warning` band.

[MODEL_CONFIG_TYPES]: model-builder, metadata, and query-function extensions

| [INDEX] | [SYMBOL]                                     | [TYPE_FAMILY] | [CAPABILITY]                                       |
| :-----: | :------------------------------------------- | :------------ | :------------------------------------------------- |
|  [01]   | `SqlitePropertyBuilderExtensions`            | class         | `UseAutoincrement`, `HasSrid`, generation strategy |
|  [02]   | `SqliteComplexTypePropertyBuilderExtensions` | class         | the same two knobs on complex-type properties      |
|  [03]   | `SqliteTableBuilderExtensions`               | class         | fluent `UseSqlReturningClause` per table shape     |
|  [04]   | `SqliteEntityTypeBuilderExtensions`          | class         | convention-tier RETURNING toggle                   |
|  [05]   | `SqliteMigrationBuilderExtensions`           | class         | `IsSqlite` probe inside a migration body           |
|  [06]   | `SqliteDbFunctionsExtensions`                | class         | `EF.Functions` SQLite scalar family                |
|  [07]   | `SqlitePropertyExtensions`                   | class         | reads and sets the strategy on property metadata   |
|  [08]   | `SqliteEntityTypeExtensions`                 | class         | reads and sets RETURNING on entity metadata        |
|  [09]   | `SqliteEntityTypeMappingFragmentExtensions`  | class         | per-fragment RETURNING metadata                    |
|  [10]   | `SqliteTableExtensions`                      | class         | `IsSqlReturningClauseUsed` on the built table      |
|  [11]   | `SqliteValueGenerationStrategy`              | enum          | `None` / `Autoincrement` classifier                |

[PROVIDER_SERVICES]: `[EntityFrameworkInternal]` implementations the EF service provider resolves under `UseSqlite`; an override registers a replacement against the EF interface rather than subclassing the `Internal` type.

| [INDEX] | [SYMBOL]                             | [NAMESPACE]               | [CAPABILITY]                                     |
| :-----: | :----------------------------------- | :------------------------ | :----------------------------------------------- |
|  [01]   | `SqliteTypeMappingSource`            | `Storage.Internal`        | CLR ⇄ SQLite type mapping, JSON codecs included  |
|  [02]   | `SqliteRelationalConnection`         | `Storage.Internal`        | provider connection and `Batteries` init         |
|  [03]   | `SqliteSqlGenerationHelper`          | `Storage.Internal`        | identifier and literal SQL emission              |
|  [04]   | `SqliteQuerySqlGenerator`            | `Query.Internal`          | SELECT SQL emission                              |
|  [05]   | `SqliteMethodCallTranslatorProvider` | `Query.Internal`          | method, member, and aggregate translator rosters |
|  [06]   | `SqliteRegexMethodTranslator`        | `Query.Internal`          | `Regex.IsMatch` → `REGEXP`                       |
|  [07]   | `SqliteUpdateSqlGenerator`           | `Update.Internal`         | INSERT/UPDATE/DELETE SQL                         |
|  [08]   | `SqliteDatabaseCreator`              | `Storage.Internal`        | `EnsureCreated`/`EnsureDeleted`                  |
|  [09]   | `SqliteModelValidator`               | `Infrastructure.Internal` | store-model validation                           |
|  [10]   | `SqliteOptionsExtension`             | `Infrastructure.Internal` | provider option carrier                          |
|  [11]   | `SqliteHistoryRepository`            | `Migrations.Internal`     | `__EFMigrationsHistory` access                   |
|  [12]   | `SqliteMigrationDatabaseLock`        | `Migrations.Internal`     | migration mutual exclusion                       |
|  [13]   | `SqliteDesignTimeServices`           | `Design.Internal`         | design and scaffolding service entry             |

`SqliteMigrationsSqlGenerator` (`Microsoft.EntityFrameworkCore.Migrations`) and the `SqliteConventionSetBuilder` family (`Microsoft.EntityFrameworkCore.Metadata.Conventions`) are the two provider services declared outside an `Internal` namespace and open to subclassing.

[BASE_EF_TYPES]: base and relational EF surface the store, query, and migration rails compose — `Microsoft.EntityFrameworkCore` / `.Relational`, bound through `UseSqlite` rather than declared by this provider.

| [INDEX] | [SYMBOL]                           | [NAMESPACE]              | [CAPABILITY]                                                           |
| :-----: | :--------------------------------- | :----------------------- | :--------------------------------------------------------------------- |
|  [01]   | `PooledDbContextFactory<TContext>` | `…Infrastructure`        | `IDbContextFactory<TContext>` over a context pool                      |
|  [02]   | `IExecutionStrategy`               | `…Storage`               | retry rail; `RetriesOnFailure`, `Execute`/`ExecuteAsync`               |
|  [03]   | `IDbContextTransaction`            | `…Storage`               | ambient transaction; `TransactionId`, `SupportsSavepoints`             |
|  [04]   | `SaveChangesInterceptor`           | `…Diagnostics`           | `ISaveChangesInterceptor` base; save, failure, cancel hooks            |
|  [05]   | `DbCommandInterceptor`             | `…Diagnostics` (rel)     | command reader/scalar/non-query execution hooks                        |
|  [06]   | `DbConnectionInterceptor`          | `…Diagnostics` (rel)     | connection open and close hooks                                        |
|  [07]   | `DbTransactionInterceptor`         | `…Diagnostics` (rel)     | transaction lifecycle hooks                                            |
|  [08]   | `ModelConfigurationBuilder`        | `…`                      | `ConfigureConventions` target; property and mapping defaults           |
|  [09]   | `IMigrator`                        | `…Migrations`            | migration driver; script emission and runtime apply                    |
|  [10]   | `MigrationBuilder`                 | `…Migrations`            | operation builder; `Operations`, `ActiveProvider`                      |
|  [11]   | `Migration`                        | `…Migrations`            | `UpOperations`/`DownOperations`, `InitialDatabase` (`"0"`)             |
|  [12]   | `MigrationOperation`               | `…Migrations.Operations` | typed operation base; `IsDestructiveChange`                            |
|  [13]   | `MigrationsSqlGenerationOptions`   | `…Migrations`            | `[Flags]`: `Default=0`, `Script=1`, `Idempotent=2`, `NoTransactions=4` |
|  [14]   | `TableBuilder`                     | `…Metadata.Builders`     | table-scoped mapping; check constraints, triggers, comments            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider admission — every surface is a static extension; `UseSqlite` mirrors its four argument shapes across `DbContextOptionsBuilder` and `DbContextOptionsBuilder<TContext>`.

| [INDEX] | [SURFACE]                                                       | [CAPABILITY]                                          |
| :-----: | :-------------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `UseSqlite(Action<SqliteDbContextOptionsBuilder>?)`             | admits SQLite against a connection bound elsewhere    |
|  [02]   | `UseSqlite(string?, Action<…>?)`                                | admits SQLite from a connection string                |
|  [03]   | `UseSqlite(DbConnection, Action<…>?)`                           | admits SQLite over an open `SqliteConnection`         |
|  [04]   | `UseSqlite(DbConnection, bool, Action<…>?)`                     | same, `contextOwnsConnection` deciding disposal       |
|  [05]   | `UseSqlite<TContext>(…)`                                        | the four shapes above on the typed builder            |
|  [06]   | `AddSqlite<TContext>(string?, Action<…>?, Action<…>?)`          | registers a pooled context plus the provider          |
|  [07]   | `AddEntityFrameworkSqlite()`                                    | registers the bare EF SQLite services                 |
|  [08]   | `DatabaseFacade.IsSqlite()`                                     | provider discriminant on a live context               |
|  [09]   | `MigrationBuilder.IsSqlite()`                                   | provider discriminant inside a migration body         |
|  [10]   | `SpatialiteLoader.Load(DbConnection)` / `TryLoad(DbConnection)` | arms the spatial extension; `TryLoad` reports absence |

[ENTRYPOINT_SCOPE]: option knobs — every member is an instance call on `SqliteDbContextOptionsBuilder`, inherited whole from `RelationalDbContextOptionsBuilder`, and each returns the builder for chaining.

| [INDEX] | [SURFACE]                                                  | [CAPABILITY]                                     |
| :-----: | :--------------------------------------------------------- | :----------------------------------------------- |
|  [01]   | `MigrationsAssembly(string?)` / `(Assembly)`               | selects the migration-owning assembly            |
|  [02]   | `MigrationsHistoryTable(string, string?)`                  | renames `__EFMigrationsHistory`                  |
|  [03]   | `CommandTimeout(int?)`                                     | per-command timeout                              |
|  [04]   | `MinBatchSize(int)` / `MaxBatchSize(int)`                  | `SaveChanges` batch bounds                       |
|  [05]   | `UseRelationalNulls(bool)`                                 | C-style against SQL three-valued null semantics  |
|  [06]   | `UseQuerySplittingBehavior(QuerySplittingBehavior)`        | single-query against split-query include default |
|  [07]   | `ExecutionStrategy(Func<…, IExecutionStrategy>)`           | binds the retry strategy the facade mints        |
|  [08]   | `TranslateParameterizedCollectionsToConstants()`           | inlines collection parameters as SQL constants   |
|  [09]   | `TranslateParameterizedCollectionsToParameters()`          | keeps collection parameters parameterized        |
|  [10]   | `UseParameterizedCollectionMode(ParameterTranslationMode)` | sets the collection translation mode per model   |

[ENTRYPOINT_SCOPE]: model configuration and query functions — static extensions; the `PropertyBuilder`/`ColumnBuilder`/`ComplexTypePropertyBuilder` and `TableBuilder`/`SplitTableBuilder`/`OwnedNavigationTableBuilder` families each carry the same knob across their shapes, and every `IConvention*` form adds `fromDataAnnotation` with a `CanSet…` predicate beside it.

| [INDEX] | [SURFACE]                                                  | [CAPABILITY]                                      |
| :-----: | :--------------------------------------------------------- | :------------------------------------------------ |
|  [01]   | `PropertyBuilder.UseAutoincrement()`                       | emits `AUTOINCREMENT` on the rowid key            |
|  [02]   | `ColumnBuilder.UseAutoincrement()`                         | same knob from inside `ToTable`                   |
|  [03]   | `ComplexTypePropertyBuilder.UseAutoincrement()`            | same knob on a complex-type member                |
|  [04]   | `PropertyBuilder.HasSrid(int)`                             | tags a spatial column SRID                        |
|  [05]   | `ComplexTypePropertyBuilder.HasSrid(int)`                  | same tag on a complex-type member                 |
|  [06]   | `IConventionPropertyBuilder.HasValueGenerationStrategy(…)` | sets the generation strategy at convention tier   |
|  [07]   | `TableBuilder.UseSqlReturningClause(bool)`                 | toggles the RETURNING write path per table        |
|  [08]   | `IConventionEntityTypeBuilder.UseSqlReturningClause(…)`    | same toggle per entity or `StoreObjectIdentifier` |
|  [09]   | `IReadOnlyEntityType.IsSqlReturningClauseUsed()`           | reads the resolved RETURNING decision             |
|  [10]   | `IReadOnlyProperty.GetValueGenerationStrategy()`           | reads the resolved generation strategy            |
|  [11]   | `EF.Functions.Glob(string, string)`                        | `GLOB` pattern match                              |
|  [12]   | `EF.Functions.Hex(byte[])` / `Unhex(string[, string])`     | blob ⇄ hex-text conversion                        |
|  [13]   | `EF.Functions.Substr(byte[], int[, int])`                  | byte-range slice inside SQL                       |
|  [14]   | `EF.CompileQuery` / `EF.CompileAsyncQuery`                 | compiled projection delegate, to `TParam15`       |

[ENTRYPOINT_SCOPE]: base EF runtime — the provider-agnostic execution, interception, transaction, read, model, and migration surfaces the store, query, and schema rails bind through this provider; every async member takes a trailing `CancellationToken`, elided below.

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                                         |
| :-----: | :------------------------------------------------------------------- | :------- | :--------------------------------------------------- |
|  [01]   | `PooledDbContextFactory<T>.CreateDbContextAsync()`                   | instance | leases one pooled `DbContext` per store op           |
|  [02]   | `PooledDbContextFactory<T>(DbContextOptions<T>, int poolSize)`       | ctor     | pool sizing; the `IDbContextPool<T>` form mirrors it |
|  [03]   | `DatabaseFacade.CreateExecutionStrategy()`                           | instance | mints the provider retry strategy                    |
|  [04]   | `IExecutionStrategy.ExecuteAsync<TState,TResult>(…)`                 | instance | retry-wraps the unit of work                         |
|  [05]   | `DbContextOptionsBuilder.AddInterceptors(IInterceptor…)`             | instance | registers the four interceptor families once         |
|  [06]   | `SavingChangesAsync(DbContextEventData, InterceptionResult<int>)`    | instance | before-save hook                                     |
|  [07]   | `SavedChangesAsync(SaveChangesCompletedEventData, int)`              | instance | after-save hook                                      |
|  [08]   | `DbContextOptionsBuilder.UseQueryTrackingBehavior(…)`                | instance | sets the default tracking lane                       |
|  [09]   | `DbContextOptionsBuilder.UseSeeding(…)` / `UseAsyncSeeding(…)`       | instance | seed delegate run at pooled-factory build            |
|  [10]   | `DatabaseFacade.BeginTransactionAsync(IsolationLevel)`               | instance | opens an isolation-typed transaction                 |
|  [11]   | `DatabaseFacade.UseTransactionAsync(DbTransaction?, Guid)`           | instance | enrolls an open ADO transaction                      |
|  [12]   | `IDbContextTransaction.CreateSavepointAsync(string)`                 | instance | savepoint mark; rollback and release mirror          |
|  [13]   | `DatabaseFacade.SqlQuery<T>(FormattableString)`                      | instance | keyless parameterized projection read                |
|  [14]   | `DatabaseFacade.SqlQueryRaw<T>(string, object[])`                    | instance | raw keyless projection read                          |
|  [15]   | `DatabaseFacade.ExecuteSqlAsync(FormattableString)`                  | instance | parameterized statement                              |
|  [16]   | `DatabaseFacade.ExecuteSqlRawAsync(string, object[])`                | instance | raw statement                                        |
|  [17]   | `ToListAsync<T>()` / `SingleAsync<T>()` / `FirstOrDefaultAsync<T>()` | static   | materializes a `SqlQuery` or LINQ read               |
|  [18]   | `AsNoTracking<T>()` / `TagWith<T>(string)`                           | static   | per-query tracking opt-out and SQL tag               |
|  [19]   | `ExecuteUpdateAsync<T>(…)` / `ExecuteDeleteAsync<T>(…)`              | static   | set-based write with no change tracker               |
|  [20]   | `DbContext.ConfigureConventions(ModelConfigurationBuilder)`          | instance | pre-model type and convention binding                |
|  [21]   | `DbContextOptionsBuilder.UseModel(IModel)`                           | instance | mounts a compiled model                              |
|  [22]   | `IMigrator.GenerateScript(string?, string?, …Options)`               | instance | deploy-SQL emission                                  |
|  [23]   | `IMigrator.MigrateAsync(string?)`                                    | instance | runtime migration apply                              |
|  [24]   | `IMigrator.HasPendingModelChanges()`                                 | instance | model-against-migrations drift probe                 |
|  [25]   | `DatabaseFacade.GetPendingMigrationsAsync()`                         | instance | unapplied migration roster                           |
|  [26]   | `MigrationBuilder.Sql(string, bool suppressTransaction)`             | instance | lock-light step SQL fold target                      |
|  [27]   | `MigrationBuilder.ActiveProvider`                                    | property | keys provider-aware emission                         |
|  [28]   | `Migration.UpOperations` / `DownOperations`                          | property | materialized migration plan input                    |
|  [29]   | `MigrationOperation.IsDestructiveChange`                             | property | per-operation destructive stamp                      |
|  [30]   | `RelationalPropertyBuilderExtensions.HasComputedColumnSql(…)`        | static   | stored or virtual generated column                   |
|  [31]   | `TableBuilder.HasCheckConstraint(string, string?)`                   | instance | table-scoped `CHECK`                                 |
|  [32]   | `TableBuilder.ExcludeFromMigrations(bool)`                           | instance | holds a mapped table out of migrations               |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every SQLite store op folds through one `UseSqlite` binding: the provider, its relational knobs, the SQLite model annotations, and the LINQ→SQL translation all attach to the single `SqliteDbContextOptionsBuilder` the callback receives, so a second provider entry point is unrepresentable.
- `SqliteDbContextOptionsBuilder` declares no member of its own; every knob is inherited `RelationalDbContextOptionsBuilder` state, so a SQLite-only option is a model annotation or a `StoreProfile` row value, never a builder call.
- SQLite refuses most `ALTER TABLE` forms, so `SqliteMigrationsSqlGenerator` defers add, alter, drop, and rename column operations into one table rebuild and raises `SqliteEventId.TableRebuildPendingWarning`; migration exclusion reads from receipts rather than from `Internal`-namespace types.

[STACKING]:
- `api-sqlite`(`.api/api-sqlite.md`): `UseSqlite(DbConnection, contextOwnsConnection: false)` mounts the already-dialed `SqliteConnection`, so the ADO open ritual's pragma rows, registered UDFs and collations, and `sqlite3_db_config` hardening carry into every `DbContext` lease instead of a second connection posture.
- `api-sqlitepcl`(`.api/api-sqlitepcl.md`): both providers ride the one bundled `e_sqlite3` engine, so `SpatialiteLoader.Load` arms `mod_spatialite` only where the loader-enable db_config posture that catalog governs already permits it.
- `api-thinktecture-ef`(`.api/api-thinktecture-ef.md`) / `api-ef-naming`(`.api/api-ef-naming.md`) / `api-ef-design`(`.api/api-ef-design.md`): value-object conversion, snake_case naming, and design-time migration and compiled-model emission stack onto this provider through the one `DbContextOptionsBuilder` chain.
- `Store/provisioning#EMBEDDED_FLOOR`: `StoreProfile.Embedded` carries `builder.UseSqlite((SqliteConnection)connection)` as its `Ef` bind delegate over the connection the open ritual dialed, so provider variance stays one row on the closed engine axis.
- `Element/identity#ELEMENT_IDENTITY`: `ConverterRail.Compose` mounts that provider row onto the one `IdentityContext`, and `UseAutoincrement`, `HasSrid`, and `UseSqlReturningClause` are the SQLite-side annotations the generated model carries.
- `Query/retrieval#FUSION_AND_REUSE`: `SqliteRegexMethodTranslator` projects `Regex.IsMatch` onto the `REGEXP` operator and the `EF.Functions` `Glob`/`Substr`/`Hex` family projects its SQLite scalars, so the lexical lane translates server-side instead of evaluating on the client.

[LOCAL_ADMISSION]:
- SQLite enters through the store-profile algebra: `UseSqlite` binds once on the profile row, and every provider knob stays row data rather than a public service family.
- `SqliteValueGenerationStrategy.Autoincrement` and `UseSqlReturningClause` are the two SQLite-specific model knobs the schema layer sets; the fluent `TableBuilder` form is the design-time declaration and the `IConvention*` form the convention-tier override.
- `SqliteEventId` is the provider diagnostic surface the EF logging path raises; the interceptor families register once on `DbContextOptionsBuilder` and span every provider, so the interception seam owns them.
- A seed delegate enters through `UseSeeding`/`UseAsyncSeeding` at pooled-factory build.
- SQLite is one engine row whose capability columns (`vector:false`, `fullText:true`, `migrations:true`) gate which lanes a profile admits.

[RAIL_LAW]:
- Package: `Microsoft.EntityFrameworkCore.Sqlite`
- Owns: EF SQLite provider admission, the SQLite model and query-function surface, and the base EF runtime the embedded store rails compose
- Accept: the `UseSqlite` admission family, the relational option knobs, the model and metadata extensions, `EF.Functions` SQLite scalars, `SqliteEventId` diagnostics, and the composed base EF execution, interception, transaction, read, and migration surface
- Reject: referencing `…Sqlite.*.Internal` services by type; treating the meta-package as the assembly home; a SQLite-branded public service family; the raw-`SqliteConnection` open ceremony `api-sqlite` owns; hand DDL where a builder extension owns the declaration
