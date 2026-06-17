# [RASM_PERSISTENCE_API_NPGSQL_EF]

`Npgsql.EntityFrameworkCore.PostgreSQL` supplies the EF Core PostgreSQL
provider, provider options, relational services, SQL generation, migrations,
query translation, type mapping, value generation, and scaffolding.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Npgsql.EntityFrameworkCore.PostgreSQL`
- package: `Npgsql.EntityFrameworkCore.PostgreSQL`
- assembly: `Npgsql.EntityFrameworkCore.PostgreSQL`
- namespace: `Npgsql.EntityFrameworkCore.PostgreSQL`
- plugin package: `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime`
- asset: runtime library
- rail: store-provider

## [2]-[PUBLIC_TYPES]

[PROVIDER_TYPES]: provider admission and options
- rail: store-provider

| [INDEX] | [SYMBOL]                                  | [PACKAGE_ROLE]     | [CAPABILITY]                                                                                                                                                                                                                         |
| :-----: | :---------------------------------------- | :----------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `NpgsqlDbContextOptionsBuilder`           | provider options   | `SetPostgresVersion`, `EnableRetryOnFailure`, `ConfigureDataSource`, `MapEnum`, `MapRange`, `UseAdminDatabase`, `UseRedshift`, `ProvidePasswordCallback`, `ProvideClientCertificatesCallback`, `RemoteCertificateValidationCallback` |
|   [2]   | `NpgsqlDbContextOptionsBuilderExtensions` | builder extension  | `UseNpgsql` overloads: no-arg, connection string, `DbConnection`, `DbConnection+owned`, `DbDataSource` (generic and non-generic)                                                                                                     |
|   [3]   | `NpgsqlServiceCollectionExtensions`       | service extension  | `AddNpgsql<TContext>`, `AddEntityFrameworkNpgsql`                                                                                                                                                                                    |
|   [4]   | `NpgsqlOptionsExtension`                  | options extension  | carries provider policy                                                                                                                                                                                                              |
|   [5]   | `NpgsqlDatabaseFacadeExtensions`          | database extension | `IsNpgsql`                                                                                                                                                                                                                           |
|   [6]   | `NpgsqlDesignTimeServices`                | design services    | admits design tooling                                                                                                                                                                                                                |
|   [7]   | `NpgsqlValueGenerationStrategy`           | value-gen enum     | `None`, `SequenceTrigger`, `Serial`, `IdentityAlwaysColumn`, `IdentityByDefaultColumn`, `Sequence`                                                                                                                                   |

[RELATIONAL_TYPES]: PostgreSQL EF services
- rail: store-provider

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE]      | [CAPABILITY]            |
| :-----: | :----------------------------- | :------------------ | :---------------------- |
|   [1]   | `NpgsqlTypeMappingSource`      | type mapper         | maps CLR values         |
|   [2]   | `NpgsqlSqlGenerationHelper`    | SQL helper          | emits SQL identifiers   |
|   [3]   | `NpgsqlQuerySqlGenerator`      | query generator     | emits query SQL         |
|   [4]   | `NpgsqlMigrationsSqlGenerator` | migration generator | emits migration SQL     |
|   [5]   | `NpgsqlUpdateSqlGenerator`     | update generator    | emits update SQL        |
|   [6]   | `NpgsqlDatabaseCreator`        | database creator    | creates store database  |
|   [7]   | `NpgsqlModelValidator`         | model validator     | validates store model   |
|   [8]   | `NpgsqlValueGeneratorCache`    | value generator     | caches value generation |
|   [9]   | `NpgsqlAnnotationNames`        | annotation names    | names provider metadata |

[MIGRATION_TYPES]: PostgreSQL migration operations
- rail: store-provider

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE]      | [CAPABILITY]           |
| :-----: | :------------------------------ | :------------------ | :--------------------- |
|   [1]   | `PostgresExtension`             | metadata operation  | declares extension     |
|   [2]   | `NpgsqlCreateDatabaseOperation` | migration operation | creates store database |
|   [3]   | `PostgresEnum`                  | metadata operation  | declares enum type     |
|   [4]   | `PostgresRange`                 | metadata operation  | declares range type    |
|   [5]   | `NpgsqlHistoryRepository`       | migration service   | owns migration history |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider admission
- rail: store-provider

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]       | [CAPABILITY]                                                                        |
| :-----: | :----------------------------- | :----------------- | :---------------------------------------------------------------------------------- |
|   [1]   | `UseNpgsql`                    | builder extension  | applies provider policy; overloads: no-arg, string, `DbConnection`, `DbDataSource`  |
|   [2]   | `AddNpgsql<TContext>`          | service extension  | registers `DbContext` via DI with connection string and optional provider action    |
|   [3]   | `AddEntityFrameworkNpgsql`     | service extension  | registers EF services without `DbContext`                                           |
|   [4]   | `IsNpgsql`                     | database extension | identifies provider                                                                 |
|   [5]   | `SetPostgresVersion`           | provider option    | declares engine floor; mandatory — undeclared silently withholds newer translations |
|   [6]   | `EnableRetryOnFailure`         | provider option    | activates transient retry strategy; overloads for count, delay, and SQLSTATE codes  |
|   [7]   | `ConfigureDataSource`          | provider option    | runs `Action<NpgsqlDataSourceBuilder>` on the provider-owned source                 |
|   [8]   | `MapEnum<T>` / `MapEnum(Type)` | provider option    | maps CLR enum to PostgreSQL enum type                                               |
|   [9]   | `MapRange`                     | provider option    | maps custom range type by subtype CLR type or name                                  |
|  [10]   | `MigrationsAssembly`           | provider option    | selects migration owner assembly                                                    |
|  [11]   | `CommandTimeout`               | provider option    | sets command timeout                                                                |
|  [12]   | `UseNodaTime`                  | plugin option      | maps NodaTime values (via `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime`)         |

[ENTRYPOINT_SCOPE]: model builder extensions
- rail: store-provider

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]    | [CAPABILITY]                                                                  |
| :-----: | :------------------------- | :-------------- | :---------------------------------------------------------------------------- |
|   [1]   | `HasPostgresExtension`     | model extension | declares PostgreSQL extension; optional schema, name, version                 |
|   [2]   | `HasPostgresEnum`          | model extension | declares PostgreSQL enum; generic `<TEnum>` or by name + label array          |
|   [3]   | `HasPostgresRange`         | model extension | declares custom range type                                                    |
|   [4]   | `HasCollation`             | model extension | declares server collation; locale, lcCollate/lcCtype, provider, deterministic |
|   [5]   | `UseHiLo`                  | model extension | selects Hi-Lo sequence strategy globally                                      |
|   [6]   | `UseIdentityColumns`       | model extension | selects `IDENTITY BY DEFAULT` globally                                        |
|   [7]   | `UseIdentityAlwaysColumns` | model extension | selects `IDENTITY ALWAYS` globally                                            |
|   [8]   | `UseKeySequences`          | model extension | selects sequence-per-key strategy; optional name suffix and schema            |
|   [9]   | `UseSerialColumns`         | model extension | selects `SERIAL` strategy globally                                            |
|  [10]   | `UseDatabaseTemplate`      | model extension | sets `CREATE DATABASE ... TEMPLATE`                                           |
|  [11]   | `UseTablespace`            | model extension | sets default tablespace                                                       |

[ENTRYPOINT_SCOPE]: entity and property builder extensions
- rail: store-provider

| [INDEX] | [SURFACE]                    | [CALL_SHAPE]           | [CAPABILITY]                                                                    |
| :-----: | :--------------------------- | :--------------------- | :------------------------------------------------------------------------------ |
|   [1]   | `HasGeneratedTsVectorColumn` | entity extension       | declares stored generated `tsvector` column from named source properties        |
|   [2]   | `IsUnlogged`                 | entity extension       | marks table `UNLOGGED`                                                          |
|   [3]   | `HasStorageParameter`        | entity/index extension | sets `WITH (param = value)` on table or index                                   |
|   [4]   | `IsGeneratedTsVectorColumn`  | property extension     | configures generated `tsvector` on an existing property                         |
|   [5]   | `HasPostgresArrayConversion` | property extension     | configures element-wise converter for CLR array or `List<T>` PostgreSQL arrays  |
|   [6]   | `HasIdentityOptions`         | property extension     | sets `START`, `INCREMENT`, `MINVALUE`, `MAXVALUE`, `CYCLE`, `CACHE` on identity |
|   [7]   | `UseSerialColumn`            | property extension     | selects `SERIAL` on property                                                    |
|   [8]   | `UseIdentityAlwaysColumn`    | property extension     | selects `IDENTITY ALWAYS` on property                                           |
|   [9]   | `UseIdentityByDefaultColumn` | property extension     | selects `IDENTITY BY DEFAULT` on property                                       |
|  [10]   | `UseIdentityColumn`          | property extension     | selects identity (by-default) on property                                       |
|  [11]   | `UseCompressionMethod`       | property extension     | sets per-column storage compression                                             |

[ENTRYPOINT_SCOPE]: index builder extensions
- rail: store-provider

| [INDEX] | [SURFACE]                   | [CALL_SHAPE]    | [CAPABILITY]                                                                    |
| :-----: | :-------------------------- | :-------------- | :------------------------------------------------------------------------------ |
|   [1]   | `HasMethod`                 | index extension | selects index method: `btree`, `hash`, `gin`, `gist`, `brin`, `hnsw`, `ivfflat` |
|   [2]   | `HasOperators`              | index extension | sets per-column operator classes                                                |
|   [3]   | `IncludeProperties`         | index extension | adds covering (non-key) columns to index                                        |
|   [4]   | `AreNullsDistinct`          | index extension | controls single-null uniqueness without partial-index workaround                |
|   [5]   | `HasNullSortOrder`          | index extension | sets `NULLS FIRST` / `NULLS LAST` per column                                    |
|   [6]   | `UseCollation`              | index extension | sets per-column collation on index                                              |
|   [7]   | `HasStorageParameter`       | index extension | sets `WITH (param = value)` on index                                            |
|   [8]   | `IsCreatedConcurrently`     | index extension | emits `CREATE INDEX CONCURRENTLY`                                               |
|   [9]   | `IsTsVectorExpressionIndex` | index extension | marks index as tsvector expression index with language config                   |

[ENTRYPOINT_SCOPE]: migration builder extensions
- rail: store-provider

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]        | [CAPABILITY]                                           |
| :-----: | :------------------------ | :------------------ | :----------------------------------------------------- |
|   [1]   | `IsNpgsql`                | migration check     | guards migration to PostgreSQL provider                |
|   [2]   | `EnsurePostgresExtension` | migration operation | idempotently ensures extension exists; schema, version |

[ENTRYPOINT_SCOPE]: query DB-function extensions
- rail: store-provider

| [INDEX] | [SURFACE]                                | [CALL_SHAPE]            | [CAPABILITY]                                                         |
| :-----: | :--------------------------------------- | :---------------------- | :------------------------------------------------------------------- |
|   [1]   | `ILike`                                  | `DbFunctions` extension | case-insensitive pattern match; optional escape character            |
|   [2]   | `StringToArray`                          | `DbFunctions` extension | splits string to array with delimiter; optional null-string          |
|   [3]   | `Reverse`                                | `DbFunctions` extension | reverses a string                                                    |
|   [4]   | `Distance(DateOnly, DateOnly)`           | `DbFunctions` extension | integer day distance between dates                                   |
|   [5]   | `Distance(DateTime, DateTime)`           | `DbFunctions` extension | `TimeSpan` distance between datetimes                                |
|   [6]   | `GreaterThan` / `LessThan` / `>=` / `<=` | `DbFunctions` extension | `ITuple` row-value comparisons mapping to PostgreSQL row expressions |
|   [7]   | `ToDate` / `ToTimestamp`                 | `DbFunctions` extension | format-string conversion to date/timestamp                           |

[ENTRYPOINT_SCOPE]: full-text search extensions
- rail: store-provider

| [INDEX] | [SURFACE]                                | [CALL_SHAPE]            | [CAPABILITY]                                                  |
| :-----: | :--------------------------------------- | :---------------------- | :------------------------------------------------------------ |
|   [1]   | `ToTsVector` (DB function)               | `DbFunctions` extension | converts document string to `tsvector`; optional config       |
|   [2]   | `ArrayToTsVector`                        | `DbFunctions` extension | converts lexeme string array to `tsvector`                    |
|   [3]   | `PlainToTsQuery`                         | `DbFunctions` extension | parses plain-text query; optional config                      |
|   [4]   | `PhraseToTsQuery`                        | `DbFunctions` extension | parses phrase query; optional config                          |
|   [5]   | `ToTsQuery`                              | `DbFunctions` extension | parses full query; optional config; throws on malformed input |
|   [6]   | `WebSearchToTsQuery`                     | `DbFunctions` extension | parses web-search query; the only form admitted for user text |
|   [7]   | `Unaccent`                               | `DbFunctions` extension | removes accents; optional dictionary name                     |
|   [8]   | `Matches(NpgsqlTsVector, string)`        | LINQ extension          | `@@` operator: match vector against plain string              |
|   [9]   | `Matches(NpgsqlTsVector, NpgsqlTsQuery)` | LINQ extension          | `@@` operator: match vector against typed query               |
|  [10]   | `And` / `Or` / `ToNegative`              | LINQ extension          | query algebra: `&&`, `                                        |  | `, `!!` operators |
|  [11]   | `Rank` / `SetWeight` / `Concat`          | LINQ extension          | tsvector operations: ranking and weighting                    |
|  [12]   | `GetResultHeadline`                      | LINQ extension          | generates highlighted headline; optional options string       |
|  [13]   | `Rewrite`                                | LINQ extension          | rewrites tsquery via target/substitute pair or SQL SELECT     |
|  [14]   | `ToPhrase`                               | LINQ extension          | phrase operator with optional distance                        |

[ENTRYPOINT_SCOPE]: JSON and range DB-function extensions
- rail: store-provider

| [INDEX] | [SURFACE]                                                                                             | [CALL_SHAPE]            | [CAPABILITY]                                        |
| :-----: | :---------------------------------------------------------------------------------------------------- | :---------------------- | :-------------------------------------------------- |
|   [1]   | `JsonContains`                                                                                        | `DbFunctions` extension | jsonb `@>` containment                              |
|   [2]   | `JsonContained`                                                                                       | `DbFunctions` extension | jsonb `<@` containment                              |
|   [3]   | `JsonExists`                                                                                          | `DbFunctions` extension | jsonb `?` key-exists                                |
|   [4]   | `JsonExistAny`                                                                                        | `DbFunctions` extension | jsonb `?                                            | ` any-key-exists |
|   [5]   | `JsonExistAll`                                                                                        | `DbFunctions` extension | jsonb `?&` all-keys-exist                           |
|   [6]   | `JsonTypeof`                                                                                          | `DbFunctions` extension | returns jsonb value type name                       |
|   [7]   | `Contains<T>(NpgsqlRange<T>, T)`                                                                      | range extension         | range contains element                              |
|   [8]   | `Contains<T>(NpgsqlRange<T>, NpgsqlRange<T>)`                                                         | range extension         | range contains range                                |
|   [9]   | `ContainedBy<T>`                                                                                      | range extension         | range is contained by range                         |
|  [10]   | `Overlaps<T>`                                                                                         | range extension         | ranges overlap                                      |
|  [11]   | `IsStrictlyLeftOf<T>` / `IsStrictlyRightOf<T>` / `DoesNotExtendLeftOf<T>` / `DoesNotExtendRightOf<T>` | range extension         | directional range predicates                        |
|  [12]   | `IsAdjacentTo<T>`                                                                                     | range extension         | ranges are adjacent                                 |
|  [13]   | `Union<T>` / `Intersect<T>` / `Except<T>` / `Merge<T>`                                                | range extension         | range set algebra                                   |
|  [14]   | `RangeAgg<T>`                                                                                         | range extension         | aggregate to range array                            |
|  [15]   | `RangeIntersectAgg<T>`                                                                                | range extension         | aggregate range intersection (scalar or multirange) |

## [4]-[IMPLEMENTATION_LAW]

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

[RAIL_LAW]:
- Package: `Npgsql.EntityFrameworkCore.PostgreSQL`
- Owns: EF PostgreSQL provider admission
- Accept: PostgreSQL EF store profile
- Reject: provider-branded service families
