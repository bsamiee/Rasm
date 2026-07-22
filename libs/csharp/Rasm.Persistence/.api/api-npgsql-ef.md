# [RASM_PERSISTENCE_API_NPGSQL_EF]

`Npgsql.EntityFrameworkCore.PostgreSQL` admits the EF Core PostgreSQL provider into the store-profile algebra: `UseNpgsql` binds the provider and every PostgreSQL-specific configuration surface attaches to the one options builder. This catalog also carries the provider-agnostic base EF Core `DbContext`, interception, and migration runtime the folder's store rails compose.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Npgsql.EntityFrameworkCore.PostgreSQL`
- package: `Npgsql.EntityFrameworkCore.PostgreSQL` (`PostgreSQL` license)
- assembly: `Npgsql.EntityFrameworkCore.PostgreSQL`
- namespace: `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore`
- rail: store-provider

## [02]-[PUBLIC_TYPES]

[PROVIDER_TYPES]: provider admission and options

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY] | [CAPABILITY]                                           |
| :-----: | :---------------------------------------- | :------------ | :----------------------------------------------------- |
|  [01]   | `NpgsqlDbContextOptionsBuilder`           | class         | provider-scoped fluent option knobs                    |
|  [02]   | `NpgsqlDbContextOptionsBuilderExtensions` | class         | the `UseNpgsql` admission family                       |
|  [03]   | `NpgsqlServiceCollectionExtensions`       | class         | `AddNpgsql`/`AddEntityFrameworkNpgsql` DI registration |
|  [04]   | `NpgsqlOptionsExtension`                  | class         | carries provider policy                                |
|  [05]   | `NpgsqlDatabaseFacadeExtensions`          | class         | `IsNpgsql` provider probe                              |
|  [06]   | `NpgsqlDesignTimeServices`                | class         | design/scaffolding services                            |
|  [07]   | `NpgsqlValueGenerationStrategy`           | enum          | value-generation classifier                            |

`[NpgsqlValueGenerationStrategy]`: `None` `SequenceHiLo` `SerialColumn` `IdentityAlwaysColumn` `IdentityByDefaultColumn` `Sequence`

[RELATIONAL_TYPES]: PostgreSQL EF provider services

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                   |
| :-----: | :----------------------------- | :------------ | :----------------------------- |
|  [01]   | `NpgsqlTypeMappingSource`      | class         | maps CLR values to store types |
|  [02]   | `NpgsqlSqlGenerationHelper`    | class         | emits SQL identifiers          |
|  [03]   | `NpgsqlQuerySqlGenerator`      | class         | emits query SQL                |
|  [04]   | `NpgsqlMigrationsSqlGenerator` | class         | emits migration SQL            |
|  [05]   | `NpgsqlUpdateSqlGenerator`     | class         | emits update SQL               |
|  [06]   | `NpgsqlDatabaseCreator`        | class         | creates store database         |
|  [07]   | `NpgsqlModelValidator`         | class         | validates store model          |
|  [08]   | `NpgsqlValueGeneratorCache`    | class         | caches value generation        |
|  [09]   | `NpgsqlAnnotationNames`        | class         | provider metadata annotations  |

[MIGRATION_TYPES]: PostgreSQL migration operations

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]           |
| :-----: | :------------------------------ | :------------ | :--------------------- |
|  [01]   | `PostgresExtension`             | class         | declares extension     |
|  [02]   | `NpgsqlCreateDatabaseOperation` | class         | creates store database |
|  [03]   | `PostgresEnum`                  | class         | declares enum type     |
|  [04]   | `PostgresRange`                 | class         | declares range type    |
|  [05]   | `NpgsqlHistoryRepository`       | class         | owns migration history |

[EF_CORE_BASE_TYPES]: provider-agnostic base EF Core / Relational runtime the provider composes — `Microsoft.EntityFrameworkCore`, `.Relational`; owned at the `DbContext`/interception/migration seam, not `Npgsql*` types.

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [CAPABILITY]                                                            |
| :-----: | :-------------------------------------- | :------------ | :---------------------------------------------------------------------- |
|  [01]   | `DbContext`                             | class         | `Database` facade, `SaveChangesAsync`, `ConfigureConventions` hook      |
|  [02]   | `DbContextOptionsBuilder`               | class         | `UseModel`, `AddInterceptors`; the `UseNpgsql` callback target          |
|  [03]   | `IDbContextFactory<TContext>`           | interface     | `CreateDbContext()` / `CreateDbContextAsync(CancellationToken)`         |
|  [04]   | `PooledDbContextFactory<TContext>`      | class         | `Infrastructure`; the pooled `IDbContextFactory<TContext>` impl         |
|  [05]   | `DatabaseFacade`                        | class         | `Infrastructure`; `context.Database` — strategy, transaction, raw query |
|  [06]   | `IExecutionStrategy`                    | interface     | `Storage`; `RetriesOnFailure`, `ExecuteAsync` retry rail                |
|  [07]   | `ExecutionStrategyExtensions`           | class         | `ExecuteAsync` no-context convenience overloads                         |
|  [08]   | `IDbContextTransaction`                 | interface     | `Storage`; commit/rollback plus the savepoint family                    |
|  [09]   | `ModelConfigurationBuilder`             | class         | `Properties`/`DefaultTypeMapping`/`IgnoreAny`/`Conventions` config      |
|  [10]   | `IInterceptor`                          | interface     | `Diagnostics`; base contract for every interception slot                |
|  [11]   | `ISaveChangesInterceptor`               | interface     | `Diagnostics`; `SavingChanges`/`SavedChanges`/`SaveChangesFailed`       |
|  [12]   | `IDbCommandInterceptor`                 | interface     | `Relational` `Diagnostics`; command-execution interception              |
|  [13]   | `IDbConnectionInterceptor`              | interface     | `Relational` `Diagnostics`; connection open/close interception          |
|  [14]   | `IDbTransactionInterceptor`             | interface     | `Relational` `Diagnostics`; transaction-lifecycle interception          |
|  [15]   | `IMigrator`                             | interface     | `Relational` `Migrations`; `Migrate`/`GenerateScript` deploy emission   |
|  [16]   | `MigrationsSqlGenerationOptions`        | enum          | `Relational`; `Default`/`Script`/`Idempotent`/`NoTransactions` flags    |
|  [17]   | `TableBuilder`                          | class         | `Relational`; `HasCheckConstraint`/`HasTrigger`/`ExcludeFromMigrations` |
|  [18]   | `EntityFrameworkQueryableExtensions`    | class         | base `IQueryable` async materialization plus query tagging              |
|  [19]   | `RelationalDatabaseFacadeExtensions`    | class         | `Relational`; raw `SqlQuery`/`SqlQueryRaw`, `UseTransactionAsync`       |
|  [20]   | `RelationalPropertyBuilderExtensions`   | class         | `Relational`; `HasComputedColumnSql` relational column config           |
|  [21]   | `RelationalEntityTypeBuilderExtensions` | class         | `Relational`; `ToTable(Action<TableBuilder>)` table mapping             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider admission

| [INDEX] | [SURFACE]                      | [SHAPE]            | [CAPABILITY]                                                                       |
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
|  [12]   | `UseAdminDatabase`             | provider option    | routes create/drop admin operations to a named database                            |
|  [13]   | `UseRedshift`                  | provider option    | Redshift compatibility mode; avoids unsupported modern PostgreSQL features         |
|  [14]   | `UseNodaTime`                  | plugin option      | maps NodaTime values (via `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime`)        |

[ENTRYPOINT_SCOPE]: connection security callbacks — `NpgsqlDbContextOptionsBuilder`

| [INDEX] | [SURFACE]                             | [CAPABILITY]                                    |
| :-----: | :------------------------------------ | :---------------------------------------------- |
|  [01]   | `ProvidePasswordCallback`             | resolves a dynamic password on connection open  |
|  [02]   | `ProvideClientCertificatesCallback`   | supplies client certificates for the connection |
|  [03]   | `RemoteCertificateValidationCallback` | validates the server certificate                |

[ENTRYPOINT_SCOPE]: model-builder extensions — `NpgsqlModelBuilderExtensions` statics

| [INDEX] | [SURFACE]                  | [CAPABILITY]                                                                  |
| :-----: | :------------------------- | :---------------------------------------------------------------------------- |
|  [01]   | `HasPostgresExtension`     | declares PostgreSQL extension; optional schema, name, version                 |
|  [02]   | `HasPostgresEnum`          | declares PostgreSQL enum; generic `<TEnum>` or by name + label array          |
|  [03]   | `HasPostgresRange`         | declares custom range type                                                    |
|  [04]   | `HasCollation`             | declares server collation; locale, lcCollate/lcCtype, provider, deterministic |
|  [05]   | `UseHiLo`                  | selects Hi-Lo sequence strategy globally                                      |
|  [06]   | `UseIdentityColumns`       | selects `IDENTITY BY DEFAULT` globally                                        |
|  [07]   | `UseIdentityAlwaysColumns` | selects `IDENTITY ALWAYS` globally                                            |
|  [08]   | `UseKeySequences`          | selects sequence-per-key strategy; optional name suffix and schema            |
|  [09]   | `UseSerialColumns`         | selects `SERIAL` strategy globally                                            |
|  [10]   | `UseDatabaseTemplate`      | sets `CREATE DATABASE ... TEMPLATE`                                           |
|  [11]   | `UseTablespace`            | sets default tablespace                                                       |

[ENTRYPOINT_SCOPE]: entity and property builder extensions

| [INDEX] | [SURFACE]                    | [SHAPE]                | [CAPABILITY]                                                                    |
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

[ENTRYPOINT_SCOPE]: index-builder extensions — `NpgsqlIndexBuilderExtensions` statics

| [INDEX] | [SURFACE]                   | [CAPABILITY]                                                                    |
| :-----: | :-------------------------- | :------------------------------------------------------------------------------ |
|  [01]   | `HasMethod`                 | selects index method: `btree`, `hash`, `gin`, `gist`, `brin`, `hnsw`, `ivfflat` |
|  [02]   | `HasOperators`              | sets per-column operator classes                                                |
|  [03]   | `IncludeProperties`         | adds covering (non-key) columns to index                                        |
|  [04]   | `AreNullsDistinct`          | controls single-null uniqueness without partial-index workaround                |
|  [05]   | `HasNullSortOrder`          | sets `NULLS FIRST` / `NULLS LAST` per column                                    |
|  [06]   | `UseCollation`              | sets per-column collation on index                                              |
|  [07]   | `HasStorageParameter`       | sets `WITH (param = value)` on index                                            |
|  [08]   | `IsCreatedConcurrently`     | emits `CREATE INDEX CONCURRENTLY`                                               |
|  [09]   | `IsTsVectorExpressionIndex` | marks index as tsvector expression index with language config                   |

[ENTRYPOINT_SCOPE]: migration builder extensions

| [INDEX] | [SURFACE]                 | [SHAPE]             | [CAPABILITY]                                           |
| :-----: | :------------------------ | :------------------ | :----------------------------------------------------- |
|  [01]   | `IsNpgsql`                | migration check     | guards migration to PostgreSQL provider                |
|  [02]   | `EnsurePostgresExtension` | migration operation | idempotently ensures extension exists; schema, version |

[ENTRYPOINT_SCOPE]: query DB-function extensions — all `public static` on `DbFunctions`, translated to PostgreSQL SQL inside a LINQ query

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

| [INDEX] | [SURFACE]                                | [SHAPE]                 | [CAPABILITY]                                                  |
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
|  [10]   | `And` / `Or` / `ToNegative`              | LINQ extension          | query algebra operators mapping to `&&` / `\|\|` / `!!`       |
|  [11]   | `Rank` / `SetWeight` / `Concat`          | LINQ extension          | tsvector operations: ranking and weighting                    |
|  [12]   | `GetResultHeadline`                      | LINQ extension          | generates highlighted headline; optional options string       |
|  [13]   | `Rewrite`                                | LINQ extension          | rewrites tsquery via target/substitute pair or SQL SELECT     |
|  [14]   | `ToPhrase`                               | LINQ extension          | phrase operator with optional distance                        |

[ENTRYPOINT_SCOPE]: JSON and range DB-function extensions

| [INDEX] | [SURFACE]                                     | [SHAPE]                 | [CAPABILITY]                                        |
| :-----: | :-------------------------------------------- | :---------------------- | :-------------------------------------------------- |
|  [01]   | `JsonContains`                                | `DbFunctions` extension | jsonb containment (`@>`)                            |
|  [02]   | `JsonContained`                               | `DbFunctions` extension | jsonb contained-by (`<@`)                           |
|  [03]   | `JsonExists`                                  | `DbFunctions` extension | jsonb key-exists (`?`)                              |
|  [04]   | `JsonExistAny`                                | `DbFunctions` extension | jsonb any-key-exists (`?\|`)                        |
|  [05]   | `JsonExistAll`                                | `DbFunctions` extension | jsonb all-keys-exist (`?&`)                         |
|  [06]   | `JsonTypeof`                                  | `DbFunctions` extension | returns jsonb value type name                       |
|  [07]   | `Contains<T>(NpgsqlRange<T>, T)`              | range extension         | range contains element                              |
|  [08]   | `Contains<T>(NpgsqlRange<T>, NpgsqlRange<T>)` | range extension         | range contains range                                |
|  [09]   | `ContainedBy<T>`                              | range extension         | range is contained by range                         |
|  [10]   | `Overlaps<T>`                                 | range extension         | ranges overlap                                      |
|  [11]   | `IsStrictlyLeftOf<T>`                         | range extension         | range strictly left of                              |
|  [12]   | `IsStrictlyRightOf<T>`                        | range extension         | range strictly right of                             |
|  [13]   | `DoesNotExtendLeftOf<T>`                      | range extension         | range does not extend left of                       |
|  [14]   | `DoesNotExtendRightOf<T>`                     | range extension         | range does not extend right of                      |
|  [15]   | `IsAdjacentTo<T>`                             | range extension         | ranges are adjacent                                 |
|  [16]   | `Union<T>`                                    | range extension         | range union                                         |
|  [17]   | `Intersect<T>`                                | range extension         | range intersection                                  |
|  [18]   | `Except<T>`                                   | range extension         | range difference                                    |
|  [19]   | `Merge<T>`                                    | range extension         | merges ranges to the enclosing range                |
|  [20]   | `RangeAgg<T>`                                 | range extension         | aggregate to range array                            |
|  [21]   | `RangeIntersectAgg<T>`                        | range extension         | aggregate range intersection (scalar or multirange) |

[ENTRYPOINT_SCOPE]: base EF Core runtime — context factory, execution strategy, transaction and savepoints. Every async member takes a trailing `CancellationToken ct`, elided below; `[06]`/`[07]` delegate shapes are keyed under the table.

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
- [07]: no-context `operation` is `Func<TState,CancellationToken,Task<TResult>>`; overloads drop `TState` and/or `TResult`.

[ENTRYPOINT_SCOPE]: base EF Core model, query, migration, and interception

| [INDEX] | [SURFACE]                                                             | [CAPABILITY]                                                 |
| :-----: | :-------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `DbContext.ConfigureConventions(ModelConfigurationBuilder)`           | pre-model type and convention binding                        |
|  [02]   | `DbContextOptionsBuilder.UseModel(IModel)`                            | mounts a compiled/frozen model (the `Optimize` output)       |
|  [03]   | `RelationalPropertyBuilderExtensions.HasComputedColumnSql(…)`         | store-computed column (`stored: true` = persisted generated) |
|  [04]   | `RelationalEntityTypeBuilderExtensions.ToTable(Action<TableBuilder>)` | table-scoped mapping; the host for `HasCheckConstraint`      |
|  [05]   | `TableBuilder.HasCheckConstraint(string name, string? sql)`           | named CHECK constraint on the table builder                  |
|  [06]   | `IMigrator.GenerateScript(…)`                                         | deploy SQL between migrations; `Idempotent` = re-runnable    |
|  [07]   | `EntityFrameworkQueryableExtensions.ToListAsync<T>()`                 | async materialization on `IQueryable<T>`                     |
|  [08]   | `EntityFrameworkQueryableExtensions.TagWith<T>(…)`                    | query tagging on `IQueryable<T>`                             |
|  [09]   | `RelationalDatabaseFacadeExtensions.SqlQuery<T>(FormattableString)`   | composable raw-SQL `IQueryable<T>` off `context.Database`    |
|  [10]   | `RelationalDatabaseFacadeExtensions.SqlQueryRaw<T>(…)`                | raw-string composable `IQueryable<T>` variant                |
|  [11]   | `DbContextOptionsBuilder.AddInterceptors(…)`                          | registers the four `I*Interceptor` implementations           |

- [01]: `ConfigureConventions` yields a `ModelConfigurationBuilder` with `Properties<T>()` / `DefaultTypeMapping<T>()` / `IgnoreAny<T>()` / `Conventions`.
- [03]: signature `(string? sql, bool? stored)`; `stored: true` is a persisted generated column.
- [06]: signature `(string? from = null, string? to = null, MigrationsSqlGenerationOptions = Default)`.
- [10]: signature `SqlQueryRaw<T>(string, params object[])`.
- [11]: `AddInterceptors` takes the four `IDbCommandInterceptor`/`ISaveChangesInterceptor`/`IDbConnectionInterceptor`/`IDbTransactionInterceptor`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every PostgreSQL store op folds through one options builder: `UseNpgsql` binds the provider, `SetPostgresVersion` gates translation, and the model, migration, index, and query-translation surfaces attach to the same builder — no second provider entry.

[STACKING]:
- `api-marten`(`.api/api-marten.md`): the EF-mapped `ElementIdentity` row commits atomically with its event inside one `IDocumentSession` over the shared `NpgsqlDataSource`.
- `api-npgsql`(`.api/api-npgsql.md`): the ADO codec under this provider — identity columns, query translation, and migrations share one `NpgsqlDataSource` surface.
- `api-npgsql-ef-nodatime`(`.api/api-npgsql-ef-nodatime.md`) / `api-nts-ef`(`.api/api-nts-ef.md`) / `api-pgvector-ef`(`.api/api-pgvector-ef.md`): temporal, spatial, and vector mappings stack onto this base provider through the one `UseNpgsql` options builder.
- within-lib: `Store/provisioning` selects this as one `StoreProfile` provider row — `SetPostgresVersion`, the `IMigrator`/`NpgsqlMigrationBuilderExtensions` migration API, and `NpgsqlIndexBuilderExtensions` (`HasMethod`/`HasOperators`/`AreNullsDistinct`) are the provisioning declarations, never hand DDL.

[LOCAL_ADMISSION]:
- PostgreSQL EF enters behind the store-profile vocabulary like every provider; provider-specific model annotations stay profile metadata and never become public Persistence service families.
- `SetPostgresVersion` is mandatory — undeclared, the provider assumes a trailing default and withholds newer translations.
- Extensions, enums, ranges, and NodaTime mappings require explicit model-builder declarations.
- `HasMethod`/`HasOperators` on `NpgsqlIndexBuilderExtensions` are the index-design surface and `AreNullsDistinct(false)` is the single-null uniqueness law; hand DDL and the partial-index workaround are the rejected forms.
- `WebSearchToTsQuery` is the only query parser admitted for user text; `ToTsQuery` throws on malformed input.
- Base EF Core runtime lands in this catalog: savepoints live on the `IDbContextTransaction` handle, never `DatabaseFacade`, and interceptors register once on `DbContextOptionsBuilder` across every provider.

[RAIL_LAW]:
- Package: `Npgsql.EntityFrameworkCore.PostgreSQL`
- Owns: EF PostgreSQL provider admission and the base EF Core runtime the folder's store rails compose
- Accept: the `UseNpgsql` store profile, the PostgreSQL model/migration/index/query-translation extensions, and the composed base EF surface
- Reject: provider-branded service families; hand DDL where the builder extensions own the declaration
