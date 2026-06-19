# [RASM_PERSISTENCE_API_NPGSQL_EF]

`Npgsql.EntityFrameworkCore.PostgreSQL` supplies the EF Core PostgreSQL
provider, provider options, relational services, SQL generation, migrations,
query translation, type mapping, value generation, and scaffolding.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Npgsql.EntityFrameworkCore.PostgreSQL`
- package: `Npgsql.EntityFrameworkCore.PostgreSQL`
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

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider admission
- rail: store-provider

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]       | [CAPABILITY]                                                                        |
| :-----: | :----------------------------- | :----------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `UseNpgsql`                    | builder extension  | applies provider policy; overloads: no-arg, string, `DbConnection`, `DbDataSource`  |
|  [02]   | `AddNpgsql<TContext>`          | service extension  | registers `DbContext` via DI with connection string and optional provider action    |
|  [03]   | `AddEntityFrameworkNpgsql`     | service extension  | registers EF services without `DbContext`                                           |
|  [04]   | `IsNpgsql`                     | database extension | identifies provider                                                                 |
|  [05]   | `SetPostgresVersion`           | provider option    | declares engine floor; mandatory — undeclared silently withholds newer translations |
|  [06]   | `EnableRetryOnFailure`         | provider option    | activates transient retry strategy; overloads for count, delay, and SQLSTATE codes  |
|  [07]   | `ConfigureDataSource`          | provider option    | runs `Action<NpgsqlDataSourceBuilder>` on the provider-owned source                 |
|  [08]   | `MapEnum<T>` / `MapEnum(Type)` | provider option    | maps CLR enum to PostgreSQL enum type                                               |
|  [09]   | `MapRange`                     | provider option    | maps custom range type by subtype CLR type or name                                  |
|  [10]   | `MigrationsAssembly`           | provider option    | selects migration owner assembly                                                    |
|  [11]   | `CommandTimeout`               | provider option    | sets command timeout                                                                |
|  [12]   | `UseNodaTime`                  | plugin option      | maps NodaTime values (via `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime`)         |

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

| [INDEX] | [SURFACE]                                | [CALL_SHAPE]            | [CAPABILITY]                                                         |
| :-----: | :--------------------------------------- | :---------------------- | :------------------------------------------------------------------- |
|  [01]   | `ILike`                                  | `DbFunctions` extension | case-insensitive pattern match; optional escape character            |
|  [02]   | `StringToArray`                          | `DbFunctions` extension | splits string to array with delimiter; optional null-string          |
|  [03]   | `Reverse`                                | `DbFunctions` extension | reverses a string                                                    |
|  [04]   | `Distance(DateOnly, DateOnly)`           | `DbFunctions` extension | integer day distance between dates                                   |
|  [05]   | `Distance(DateTime, DateTime)`           | `DbFunctions` extension | `TimeSpan` distance between datetimes                                |
|  [06]   | `GreaterThan` / `LessThan` / `>=` / `<=` | `DbFunctions` extension | `ITuple` row-value comparisons mapping to PostgreSQL row expressions |
|  [07]   | `ToDate` / `ToTimestamp`                 | `DbFunctions` extension | format-string conversion to date/timestamp                           |

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

[RAIL_LAW]:
- Package: `Npgsql.EntityFrameworkCore.PostgreSQL`
- Owns: EF PostgreSQL provider admission
- Accept: PostgreSQL EF store profile
- Reject: provider-branded service families
