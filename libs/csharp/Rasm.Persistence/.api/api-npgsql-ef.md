# [RASM_PERSISTENCE_API_NPGSQL_EF]

`Npgsql.EntityFrameworkCore.PostgreSQL` supplies the EF Core PostgreSQL
provider, provider options, relational services, SQL generation, migrations,
query translation, type mapping, value generation, and scaffolding.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Npgsql.EntityFrameworkCore.PostgreSQL`
- package: `Npgsql.EntityFrameworkCore.PostgreSQL`
- license: `PostgreSQL`
- assembly: `Npgsql.EntityFrameworkCore.PostgreSQL`
- namespace: `Npgsql.EntityFrameworkCore.PostgreSQL`
- plugin package: `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime`
- asset: runtime library
- rail: store-provider

## [02]-[PUBLIC_TYPES]

[PROVIDER_TYPES]: provider admission and options
- rail: store-provider

The compact rows below preserve these provider member sets:
- `NpgsqlDbContextOptionsBuilder`: `SetPostgresVersion`, `EnableRetryOnFailure`, `ConfigureDataSource`, `MapEnum`, `MapRange`, `UseAdminDatabase`, `UseRedshift`
- `NpgsqlDbContextOptionsBuilder`: `ProvidePasswordCallback`, `ProvideClientCertificatesCallback`, `RemoteCertificateValidationCallback`
- `NpgsqlDbContextOptionsBuilderExtensions`: `UseNpgsql` overload families for no-arg, connection string, `DbConnection`, owned `DbConnection`, `DbDataSource`
- `NpgsqlDbContextOptionsBuilderExtensions`: generic and non-generic `UseNpgsql` forms
- `NpgsqlServiceCollectionExtensions`: `AddNpgsql<TContext>`, `AddEntityFrameworkNpgsql`
- `NpgsqlValueGenerationStrategy`: `None`, `SequenceTrigger`, `Serial`, `IdentityAlwaysColumn`, `IdentityByDefaultColumn`, `Sequence`

| [INDEX] | [SYMBOL]                                  | [PACKAGE_ROLE]     | [CAPABILITY]                |
| :-----: | :---------------------------------------- | :----------------- | :-------------------------- |
|  [01]   | `NpgsqlDbContextOptionsBuilder`           | provider options   | configures provider options |
|  [02]   | `NpgsqlDbContextOptionsBuilderExtensions` | builder extension  | admits provider             |
|  [03]   | `NpgsqlServiceCollectionExtensions`       | service extension  | admits provider services    |
|  [04]   | `NpgsqlOptionsExtension`                  | options extension  | carries provider policy     |
|  [05]   | `NpgsqlDatabaseFacadeExtensions`          | database extension | exposes provider checks     |
|  [06]   | `NpgsqlDesignTimeServices`                | design services    | admits design tooling       |
|  [07]   | `NpgsqlValueGenerationStrategy`           | value-gen enum     | classifies value generation |

[RELATIONAL_TYPES]: PostgreSQL EF services
- rail: store-provider

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE]      | [CAPABILITY]            |
| :-----: | :----------------------------- | :------------------ | :---------------------- |
|  [01]   | `NpgsqlTypeMappingSource`      | type mapper         | maps CLR values         |
|  [02]   | `NpgsqlSqlGenerationHelper`    | SQL helper          | emits SQL identifiers   |
|  [03]   | `NpgsqlQuerySqlGenerator`      | query generator     | emits query SQL         |
|  [04]   | `NpgsqlMigrationsSqlGenerator` | migration generator | emits migration SQL     |
|  [05]   | `NpgsqlUpdateSqlGenerator`     | update generator    | emits update SQL        |
|  [06]   | `NpgsqlDatabaseCreator`        | database creator    | creates store database  |
|  [07]   | `NpgsqlModelValidator`         | model validator     | validates store model   |
|  [08]   | `NpgsqlValueGeneratorCache`    | value generator     | caches value generation |
|  [09]   | `NpgsqlAnnotationNames`        | annotation names    | names provider metadata |

[MIGRATION_TYPES]: PostgreSQL migration operations
- rail: store-provider

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE]      | [CAPABILITY]           |
| :-----: | :------------------------------ | :------------------ | :--------------------- |
|  [01]   | `PostgresExtension`             | metadata operation  | declares extension     |
|  [02]   | `NpgsqlCreateDatabaseOperation` | migration operation | creates store database |
|  [03]   | `PostgresEnum`                  | metadata operation  | declares enum type     |
|  [04]   | `PostgresRange`                 | metadata operation  | declares range type    |
|  [05]   | `NpgsqlHistoryRepository`       | migration service   | owns migration history |

[EF_CORE_BASE_TYPES]: base EF Core runtime surface the provider composes — `Microsoft.EntityFrameworkCore` / `Microsoft.EntityFrameworkCore.Relational`
- rail: store-provider

This folder ships no standalone base-EF catalog, so the provider catalog carries the base `DbContext` runtime surface a design page composes WITH `UseNpgsql` (the sibling `api-ef-sqlite` shares the same base surface). These are provider-agnostic EF Core base members owned at the `DbContext`/interception/migration seam, NOT `Npgsql*` types.

| [INDEX] | [SYMBOL]                                | [PACKAGE_ROLE]       | [CAPABILITY]                                                            |
| :-----: | :-------------------------------------- | :------------------- | :---------------------------------------------------------------------- |
|  [01]   | `DbContext`                             | context root         | `Database` facade, `SaveChangesAsync`, `ConfigureConventions` hook      |
|  [02]   | `DbContextOptionsBuilder`               | options builder      | `UseModel`, `AddInterceptors`; the `UseNpgsql` callback target          |
|  [03]   | `IDbContextFactory<TContext>`           | context factory      | `CreateDbContext()` / `CreateDbContextAsync(CancellationToken)`         |
|  [04]   | `PooledDbContextFactory<TContext>`      | pooled factory       | `Infrastructure`; the pooled `IDbContextFactory<TContext>` impl         |
|  [05]   | `DatabaseFacade`                        | database facade      | `Infrastructure`; `context.Database` — strategy, transaction, raw query |
|  [06]   | `IExecutionStrategy`                    | resiliency strategy  | `Storage`; `RetriesOnFailure`, `ExecuteAsync` retry rail                |
|  [07]   | `ExecutionStrategyExtensions`           | strategy extensions  | `ExecuteAsync` no-context convenience overloads                         |
|  [08]   | `IDbContextTransaction`                 | transaction handle   | `Storage`; commit/rollback plus the savepoint family                    |
|  [09]   | `ModelConfigurationBuilder`             | convention builder   | `Properties`/`DefaultTypeMapping`/`IgnoreAny`/`Conventions` config      |
|  [10]   | `IInterceptor`                          | interceptor marker   | `Diagnostics`; base contract for every interception slot                |
|  [11]   | `ISaveChangesInterceptor`               | save interceptor     | `Diagnostics`; `SavingChanges`/`SavedChanges`/`SaveChangesFailed`       |
|  [12]   | `IDbCommandInterceptor`                 | command interceptor  | `Relational` `Diagnostics`; command-execution interception              |
|  [13]   | `IDbConnectionInterceptor`              | connection intercept | `Relational` `Diagnostics`; connection open/close interception          |
|  [14]   | `IDbTransactionInterceptor`             | txn interceptor      | `Relational` `Diagnostics`; transaction-lifecycle interception          |
|  [15]   | `IMigrator`                             | migration runtime    | `Relational` `Migrations`; `Migrate`/`GenerateScript` deploy emission   |
|  [16]   | `MigrationsSqlGenerationOptions`        | script options enum  | `Relational`; `Default`/`Script`/`Idempotent`/`NoTransactions` flags    |
|  [17]   | `TableBuilder`                          | table builder        | `Relational`; `HasCheckConstraint`/`HasTrigger`/`ExcludeFromMigrations` |
|  [18]   | `EntityFrameworkQueryableExtensions`    | query extensions     | base `IQueryable` async materialization plus query tagging              |
|  [19]   | `RelationalDatabaseFacadeExtensions`    | facade extensions    | `Relational`; raw `SqlQuery`/`SqlQueryRaw`, `UseTransactionAsync`       |
|  [20]   | `RelationalPropertyBuilderExtensions`   | property extensions  | `Relational`; `HasComputedColumnSql` relational column config           |
|  [21]   | `RelationalEntityTypeBuilderExtensions` | entity extensions    | `Relational`; `ToTable(Action<TableBuilder>)` table mapping             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider admission
- rail: store-provider

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]       | [CAPABILITY]                                                                       |
| :-----: | :----------------------------- | :----------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `UseNpgsql`                    | builder extension  | applies provider policy; overloads: no-arg, string, `DbConnection`, `DbDataSource` |
|  [02]   | `AddNpgsql<TContext>`          | service extension  | registers `DbContext` via DI with connection string and optional provider action   |
|  [03]   | `AddEntityFrameworkNpgsql`     | service extension  | registers EF services without `DbContext`                                          |
|  [04]   | `IsNpgsql`                     | database extension | identifies provider                                                                |
|  [05]   | `SetPostgresVersion`           | provider option    | declares engine floor; mandatory — undeclared withholds newer translations         |
|  [06]   | `EnableRetryOnFailure`         | provider option    | activates transient retry; overloads for count, delay, SQLSTATE codes              |
|  [07]   | `ConfigureDataSource`          | provider option    | runs `Action<NpgsqlDataSourceBuilder>` on the provider-owned source                |
|  [08]   | `MapEnum<T>` / `MapEnum(Type)` | provider option    | maps CLR enum to PostgreSQL enum type                                              |
|  [09]   | `MapRange`                     | provider option    | maps custom range type by subtype CLR type or name                                 |
|  [10]   | `MigrationsAssembly`           | provider option    | selects migration owner assembly                                                   |
|  [11]   | `CommandTimeout`               | provider option    | sets command timeout                                                               |
|  [12]   | `UseNodaTime`                  | plugin option      | maps NodaTime values (via `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime`)        |

[ENTRYPOINT_SCOPE]: model builder extensions
- rail: store-provider

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]    | [CAPABILITY]                                                                  |
| :-----: | :------------------------- | :-------------- | :---------------------------------------------------------------------------- |
|  [01]   | `HasPostgresExtension`     | model extension | declares PostgreSQL extension; optional schema, name, version                 |
|  [02]   | `HasPostgresEnum`          | model extension | declares PostgreSQL enum; generic `<TEnum>` or by name + label array          |
|  [03]   | `HasPostgresRange`         | model extension | declares custom range type                                                    |
|  [04]   | `HasCollation`             | model extension | declares server collation; locale, lcCollate/lcCtype, provider, deterministic |
|  [05]   | `UseHiLo`                  | model extension | selects Hi-Lo sequence strategy globally                                      |
|  [06]   | `UseIdentityColumns`       | model extension | selects `IDENTITY BY DEFAULT` globally                                        |
|  [07]   | `UseIdentityAlwaysColumns` | model extension | selects `IDENTITY ALWAYS` globally                                            |
|  [08]   | `UseKeySequences`          | model extension | selects sequence-per-key strategy; optional name suffix and schema            |
|  [09]   | `UseSerialColumns`         | model extension | selects `SERIAL` strategy globally                                            |
|  [10]   | `UseDatabaseTemplate`      | model extension | sets `CREATE DATABASE ... TEMPLATE`                                           |
|  [11]   | `UseTablespace`            | model extension | sets default tablespace                                                       |

[ENTRYPOINT_SCOPE]: entity and property builder extensions
- rail: store-provider

| [INDEX] | [SURFACE]                    | [CALL_SHAPE]           | [CAPABILITY]                                                                    |
| :-----: | :--------------------------- | :--------------------- | :------------------------------------------------------------------------------ |
|  [01]   | `HasGeneratedTsVectorColumn` | entity extension       | declares stored generated `tsvector` column from named source properties        |
|  [02]   | `IsUnlogged`                 | entity extension       | marks table `UNLOGGED`                                                          |
|  [03]   | `HasStorageParameter`        | entity/index extension | sets `WITH (param = value)` on table or index                                   |
|  [04]   | `IsGeneratedTsVectorColumn`  | property extension     | configures generated `tsvector` on an existing property                         |
|  [05]   | `HasPostgresArrayConversion` | property extension     | configures element-wise converter for CLR array or `List<T>` PostgreSQL arrays  |
|  [06]   | `HasIdentityOptions`         | property extension     | sets `START`, `INCREMENT`, `MINVALUE`, `MAXVALUE`, `CYCLE`, `CACHE` on identity |
|  [07]   | `UseSerialColumn`            | property extension     | selects `SERIAL` on property                                                    |
|  [08]   | `UseIdentityAlwaysColumn`    | property extension     | selects `IDENTITY ALWAYS` on property                                           |
|  [09]   | `UseIdentityByDefaultColumn` | property extension     | selects `IDENTITY BY DEFAULT` on property                                       |
|  [10]   | `UseIdentityColumn`          | property extension     | selects identity (by-default) on property                                       |
|  [11]   | `UseCompressionMethod`       | property extension     | sets per-column storage compression                                             |

[ENTRYPOINT_SCOPE]: index builder extensions
- rail: store-provider

| [INDEX] | [SURFACE]                   | [CALL_SHAPE]    | [CAPABILITY]                                                                    |
| :-----: | :-------------------------- | :-------------- | :------------------------------------------------------------------------------ |
|  [01]   | `HasMethod`                 | index extension | selects index method: `btree`, `hash`, `gin`, `gist`, `brin`, `hnsw`, `ivfflat` |
|  [02]   | `HasOperators`              | index extension | sets per-column operator classes                                                |
|  [03]   | `IncludeProperties`         | index extension | adds covering (non-key) columns to index                                        |
|  [04]   | `AreNullsDistinct`          | index extension | controls single-null uniqueness without partial-index workaround                |
|  [05]   | `HasNullSortOrder`          | index extension | sets `NULLS FIRST` / `NULLS LAST` per column                                    |
|  [06]   | `UseCollation`              | index extension | sets per-column collation on index                                              |
|  [07]   | `HasStorageParameter`       | index extension | sets `WITH (param = value)` on index                                            |
|  [08]   | `IsCreatedConcurrently`     | index extension | emits `CREATE INDEX CONCURRENTLY`                                               |
|  [09]   | `IsTsVectorExpressionIndex` | index extension | marks index as tsvector expression index with language config                   |

[ENTRYPOINT_SCOPE]: migration builder extensions
- rail: store-provider

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]        | [CAPABILITY]                                           |
| :-----: | :------------------------ | :------------------ | :----------------------------------------------------- |
|  [01]   | `IsNpgsql`                | migration check     | guards migration to PostgreSQL provider                |
|  [02]   | `EnsurePostgresExtension` | migration operation | idempotently ensures extension exists; schema, version |

[ENTRYPOINT_SCOPE]: query DB-function extensions
- rail: store-provider

All are `public static` extensions on `DbFunctions`, translated to PostgreSQL SQL inside a LINQ query.

| [INDEX] | [SURFACE]                                | [CAPABILITY]                                                         |
| :-----: | :--------------------------------------- | :------------------------------------------------------------------- |
|  [01]   | `ILike`                                  | case-insensitive pattern match; optional escape character            |
|  [02]   | `StringToArray`                          | splits string to array with delimiter; optional null-string          |
|  [03]   | `Reverse`                                | reverses a string                                                    |
|  [04]   | `Distance(DateOnly, DateOnly)`           | integer day distance between dates                                   |
|  [05]   | `Distance(DateTime, DateTime)`           | `TimeSpan` distance between datetimes                                |
|  [06]   | `GreaterThan` / `LessThan` / `>=` / `<=` | `ITuple` row-value comparisons mapping to PostgreSQL row expressions |
|  [07]   | `ToDate` / `ToTimestamp`                 | format-string conversion to date/timestamp                           |

[ENTRYPOINT_SCOPE]: full-text search extensions
- rail: store-provider

`And`, `Or`, and `ToNegative` map to `&&`, `||`, and `!!`.

| [INDEX] | [SURFACE]                                | [CALL_SHAPE]            | [CAPABILITY]                                                  |
| :-----: | :--------------------------------------- | :---------------------- | :------------------------------------------------------------ |
|  [01]   | `ToTsVector` (DB function)               | `DbFunctions` extension | converts document string to `tsvector`; optional config       |
|  [02]   | `ArrayToTsVector`                        | `DbFunctions` extension | converts lexeme string array to `tsvector`                    |
|  [03]   | `PlainToTsQuery`                         | `DbFunctions` extension | parses plain-text query; optional config                      |
|  [04]   | `PhraseToTsQuery`                        | `DbFunctions` extension | parses phrase query; optional config                          |
|  [05]   | `ToTsQuery`                              | `DbFunctions` extension | parses full query; optional config; throws on malformed input |
|  [06]   | `WebSearchToTsQuery`                     | `DbFunctions` extension | parses web-search query; the only form admitted for user text |
|  [07]   | `Unaccent`                               | `DbFunctions` extension | removes accents; optional dictionary name                     |
|  [08]   | `Matches(NpgsqlTsVector, string)`        | LINQ extension          | `@@` operator: match vector against plain string              |
|  [09]   | `Matches(NpgsqlTsVector, NpgsqlTsQuery)` | LINQ extension          | `@@` operator: match vector against typed query               |
|  [10]   | `And` / `Or` / `ToNegative`              | LINQ extension          | query algebra operators                                       |
|  [11]   | `Rank` / `SetWeight` / `Concat`          | LINQ extension          | tsvector operations: ranking and weighting                    |
|  [12]   | `GetResultHeadline`                      | LINQ extension          | generates highlighted headline; optional options string       |
|  [13]   | `Rewrite`                                | LINQ extension          | rewrites tsquery via target/substitute pair or SQL SELECT     |
|  [14]   | `ToPhrase`                               | LINQ extension          | phrase operator with optional distance                        |

[ENTRYPOINT_SCOPE]: JSON and range DB-function extensions
- rail: store-provider

The compact rows below preserve these operator and range member sets:
- JSON operators: `JsonContains` maps to `@>`, `JsonContained` maps to `<@`, `JsonExists` maps to `?`, `JsonExistAny` maps to `?|`, and `JsonExistAll` maps to `?&`
- directional range predicates: `IsStrictlyLeftOf<T>`, `IsStrictlyRightOf<T>`, `DoesNotExtendLeftOf<T>`, `DoesNotExtendRightOf<T>`
- range set algebra: `Union<T>`, `Intersect<T>`, `Except<T>`, `Merge<T>`

| [INDEX] | [SURFACE]                                     | [CALL_SHAPE]            | [CAPABILITY]                                        |
| :-----: | :-------------------------------------------- | :---------------------- | :-------------------------------------------------- |
|  [01]   | `JsonContains`                                | `DbFunctions` extension | jsonb containment                                   |
|  [02]   | `JsonContained`                               | `DbFunctions` extension | jsonb contained-by                                  |
|  [03]   | `JsonExists`                                  | `DbFunctions` extension | jsonb key-exists                                    |
|  [04]   | `JsonExistAny`                                | `DbFunctions` extension | jsonb any-key-exists                                |
|  [05]   | `JsonExistAll`                                | `DbFunctions` extension | jsonb all-keys-exist                                |
|  [06]   | `JsonTypeof`                                  | `DbFunctions` extension | returns jsonb value type name                       |
|  [07]   | `Contains<T>(NpgsqlRange<T>, T)`              | range extension         | range contains element                              |
|  [08]   | `Contains<T>(NpgsqlRange<T>, NpgsqlRange<T>)` | range extension         | range contains range                                |
|  [09]   | `ContainedBy<T>`                              | range extension         | range is contained by range                         |
|  [10]   | `Overlaps<T>`                                 | range extension         | ranges overlap                                      |
|  [11]   | directional range predicates                  | range extension         | directional range predicates                        |
|  [12]   | `IsAdjacentTo<T>`                             | range extension         | ranges are adjacent                                 |
|  [13]   | range set algebra                             | range extension         | range set algebra                                   |
|  [14]   | `RangeAgg<T>`                                 | range extension         | aggregate to range array                            |
|  [15]   | `RangeIntersectAgg<T>`                        | range extension         | aggregate range intersection (scalar or multirange) |

[ENTRYPOINT_SCOPE]: base EF Core runtime — context factory, execution strategy, transaction and savepoints
- rail: store-provider

Every async member takes a trailing `CancellationToken ct`, elided below; the `[06]`/`[07]` delegate shapes are keyed under the table.

| [INDEX] | [SURFACE]                                                                  | [CAPABILITY]                                               |
| :-----: | :------------------------------------------------------------------------- | :--------------------------------------------------------- |
|  [01]   | `PooledDbContextFactory<TContext>.CreateDbContextAsync()`                  | leases a pooled `DbContext`; the `Bracket`/`using` root    |
|  [02]   | `PooledDbContextFactory<TContext>.CreateDbContext()`                       | synchronous pooled lease                                   |
|  [03]   | `DbContext.SaveChangesAsync()`                                             | persists the unit of work                                  |
|  [04]   | `DbContext.Database`                                                       | the `DatabaseFacade` root off the context                  |
|  [05]   | `DatabaseFacade.CreateExecutionStrategy()`                                 | mints the provider retry strategy (`EnableRetryOnFailure`) |
|  [06]   | `IExecutionStrategy.ExecuteAsync<TState,TResult>(…)`                       | retry-wrapped op; leased `DbContext` threads as first arg  |
|  [07]   | `ExecutionStrategyExtensions.ExecuteAsync<TState,TResult>(…)`              | no-context convenience overload family                     |
|  [08]   | `DatabaseFacade.BeginTransactionAsync()`                                   | opens the ambient `IDbContextTransaction`                  |
|  [09]   | `DatabaseFacade.CurrentTransaction`                                        | reads the ambient transaction                              |
|  [10]   | `RelationalDatabaseFacadeExtensions.BeginTransactionAsync(IsolationLevel)` | isolation-level transaction begin                          |
|  [11]   | `RelationalDatabaseFacadeExtensions.UseTransactionAsync(DbTransaction?)`   | enrolls an open ADO transaction into the context           |
|  [12]   | `IDbContextTransaction.CreateSavepointAsync(name)`                         | savepoint mark on the handle                               |
|  [13]   | `IDbContextTransaction.RollbackToSavepointAsync(name)`                     | savepoint rollback on the handle                           |
|  [14]   | `IDbContextTransaction.ReleaseSavepointAsync(name)`                        | savepoint release on the handle                            |
|  [15]   | `IDbContextTransaction.SupportsSavepoints`                                 | savepoint-capability probe                                 |
|  [16]   | `IDbContextTransaction.CommitAsync()` / `RollbackAsync()`                  | async commit / rollback                                    |

- [06]: `operation` is `Func<DbContext,TState,CancellationToken,Task<TResult>>`; `verifySucceeded?` is the optional idempotency re-check predicate.
- [07]: the no-context `operation` is `Func<TState,CancellationToken,Task<TResult>>`; overloads drop `TState` and/or `TResult`.

[ENTRYPOINT_SCOPE]: base EF Core model, query, migration, and interception
- rail: store-provider

Long parameter signatures and the interceptor roster are keyed under the table.

| [INDEX] | [SURFACE]                                                             | [CAPABILITY]                                                 |
| :-----: | :-------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `DbContext.ConfigureConventions(ModelConfigurationBuilder)`           | pre-model type and convention binding                        |
|  [02]   | `DbContextOptionsBuilder.UseModel(IModel)`                            | mounts a compiled/frozen model (the `Optimize` output)       |
|  [03]   | `RelationalPropertyBuilderExtensions.HasComputedColumnSql(…)`         | store-computed column (`stored: true` = persisted generated) |
|  [04]   | `RelationalEntityTypeBuilderExtensions.ToTable(Action<TableBuilder>)` | table-scoped mapping; the host for `HasCheckConstraint`      |
|  [05]   | `TableBuilder.HasCheckConstraint(…)`                                  | named CHECK constraint (non-obsolete form)                   |
|  [06]   | `IMigrator.GenerateScript(…)`                                         | deploy SQL between migrations; `Idempotent` = re-runnable    |
|  [07]   | `EntityFrameworkQueryableExtensions.ToListAsync<T>()`                 | async materialization on `IQueryable<T>`                     |
|  [08]   | `EntityFrameworkQueryableExtensions.TagWith<T>(…)`                    | query tagging on `IQueryable<T>`                             |
|  [09]   | `RelationalDatabaseFacadeExtensions.SqlQuery<T>(FormattableString)`   | composable raw-SQL `IQueryable<T>` off `context.Database`    |
|  [10]   | `RelationalDatabaseFacadeExtensions.SqlQueryRaw<T>(…)`                | raw-string composable `IQueryable<T>` variant                |
|  [11]   | `DbContextOptionsBuilder.AddInterceptors(…)`                          | registers the four `I*Interceptor` implementations           |

- [01]: `ConfigureConventions` yields a `ModelConfigurationBuilder` with `Properties<T>()` / `DefaultTypeMapping<T>()` / `IgnoreAny<T>()` / `Conventions`.
- [03]: signature `(string? sql, bool? stored)`; `stored: true` is a persisted generated column.
- [05]: non-obsolete `TableBuilder.HasCheckConstraint(string name, string? sql)`, vs the `[Obsolete]` `RelationalEntityTypeBuilderExtensions.HasCheckConstraint`.
- [06]: signature `(string? from = null, string? to = null, MigrationsSqlGenerationOptions = Default)`.
- [10]: signature `SqlQueryRaw<T>(string, params object[])`.
- [11]: the four are `IDbCommandInterceptor`/`ISaveChangesInterceptor`/`IDbConnectionInterceptor`/`IDbTransactionInterceptor`.

## [04]-[IMPLEMENTATION_LAW]

[STORE_PROFILE]:
- profile: PostgreSQL EF is one provider implementation for the store-profile algebra
- provider root: `UseNpgsql` (5 overloads: no-arg, string, `DbConnection`, `DbConnection+owned`, `DbDataSource`)
- model root: EF relational model plus PostgreSQL annotations via `NpgsqlModelBuilderExtensions`
- migration root: PostgreSQL operations, SQL generator, `NpgsqlMigrationBuilderExtensions`
- query root: `NpgsqlFullTextSearchDbFunctionsExtensions`, `NpgsqlFullTextSearchLinqExtensions`, `NpgsqlJsonDbFunctionsExtensions`, `NpgsqlRangeDbFunctionsExtensions`, `NpgsqlDbFunctionsExtensions`, `NpgsqlNetworkDbFunctionsExtensions`
- index root: `NpgsqlIndexBuilderExtensions`

[LOCAL_ADMISSION]:
- PostgreSQL EF surfaces enter behind the same store-profile vocabulary as every provider.
- Provider-specific model annotations remain profile metadata.
- `SetPostgresVersion` is mandatory; absent, the provider assumes a trailing default and silently withholds newer translations.
- Extensions, enums, ranges, and NodaTime mapping require explicit profile declarations.
- `HasMethod`/`HasOperators` on `NpgsqlIndexBuilderExtensions` are the index-design declaration surface; hand DDL is the rejected form.
- `AreNullsDistinct(false)` is the single-null uniqueness law, deleting the partial-index workaround.
- `WebSearchToTsQuery` is the only query parser admitted for user-provided text; `ToTsQuery` throws on malformed input.
- Query translation facts live here and do not become public Persistence service families.
- The base EF Core runtime surface (`DbContext` save/strategy/transaction, `IDbContextTransaction` savepoints, the interceptor family, `IMigrator.GenerateScript`, `ModelConfigurationBuilder`, the relational builder/facade extensions, `EntityFrameworkQueryableExtensions`) is documented here as the folder's base-EF home — there is no standalone base-EF catalog and the sibling `api-ef-sqlite` shares the identical surface. These members are provider-agnostic; the savepoint API lives on the `IDbContextTransaction` handle, never on `DatabaseFacade`, and interceptors register once on the `DbContextOptionsBuilder` and span every provider.

[STACKING]:
- identity owner: this provider is the relational spine of `Element/identity` — the `ElementIdentity` row (PK/TenantId/GlobalId/H3/pgvector + ACL + classification) is EF-mapped through `UseNpgsql`, and Marten (`api-marten`) commits it atomically with the event by storing it in the same `IDocumentSession` over the one `NpgsqlDataSource`.
- provisioning owner: `Store/provisioning` selects this as one provider row of the store-profile algebra — `SetPostgresVersion`, the `IMigrator`/`NpgsqlMigrationBuilderExtensions` migration API, and the `NpgsqlIndexBuilderExtensions` (`HasMethod`/`HasOperators`/`AreNullsDistinct`) index design are the provisioning declarations, never hand DDL.
- plugin composition: the temporal (`api-npgsql-ef-nodatime`), spatial (`api-nts-ef`), and vector (`api-pgvector-ef`) mappings stack onto this base provider through one options builder; the ADO codec below is `api-npgsql`, so identity columns, query translation, and migrations share one provider surface.

[RAIL_LAW]:
- Package: `Npgsql.EntityFrameworkCore.PostgreSQL`
- Owns: EF PostgreSQL provider admission
- Accept: PostgreSQL EF store profile
- Reject: provider-branded service families
