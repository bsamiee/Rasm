# [RASM_PERSISTENCE_API_NPGSQL_EF]

`Npgsql.EntityFrameworkCore.PostgreSQL` supplies the EF Core PostgreSQL
provider, provider options, relational services, SQL generation, migrations,
query translation, type mapping, value generation, and scaffolding.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Npgsql.EntityFrameworkCore.PostgreSQL`
- package: `Npgsql.EntityFrameworkCore.PostgreSQL`
- assembly: `Npgsql.EntityFrameworkCore.PostgreSQL`
- namespace: `Npgsql.EntityFrameworkCore.PostgreSQL`
- asset: runtime library
- rail: store-provider

## [2]-[PUBLIC_TYPES]

[PROVIDER_TYPES]: provider admission and options
- rail: store-provider

| [INDEX] | [SYMBOL]                                  | [PACKAGE_ROLE]     | [CAPABILITY]             |
| :-----: | :---------------------------------------- | :----------------- | :----------------------- |
|   [1]   | `NpgsqlDbContextOptionsBuilder`           | provider options   | configures provider      |
|   [2]   | `NpgsqlDbContextOptionsBuilderExtensions` | builder extension  | admits provider          |
|   [3]   | `NpgsqlServiceCollectionExtensions`       | service extension  | admits provider services |
|   [4]   | `NpgsqlOptionsExtension`                  | options extension  | carries provider policy  |
|   [5]   | `NpgsqlDatabaseFacadeExtensions`          | database extension | exposes provider checks  |
|   [6]   | `NpgsqlDesignTimeServices`                | design services    | admits design tooling    |

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

| [INDEX] | [SYMBOL]                           | [PACKAGE_ROLE]      | [CAPABILITY]             |
| :-----: | :--------------------------------- | :------------------ | :----------------------- |
|   [1]   | `CreatePostgresExtensionOperation` | migration operation | creates extension        |
|   [2]   | `AlterDatabaseOperation`           | migration operation | alters database metadata |
|   [3]   | `PostgresEnum`                     | metadata operation  | declares enum type       |
|   [4]   | `PostgresRange`                    | metadata operation  | declares range type      |
|   [5]   | `NpgsqlHistoryRepository`          | migration service   | owns migration history   |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider admission
- rail: store-provider

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]       | [CAPABILITY]            |
| :-----: | :------------------------- | :----------------- | :---------------------- |
|   [1]   | `UseNpgsql`                | builder extension  | applies provider policy |
|   [2]   | `AddEntityFrameworkNpgsql` | service extension  | registers EF services   |
|   [3]   | `IsNpgsql`                 | database extension | identifies provider     |
|   [4]   | `UseNodaTime`              | provider option    | maps NodaTime values    |
|   [5]   | `MapEnum`                  | provider option    | maps enum type          |
|   [6]   | `MapRange`                 | provider option    | maps range type         |
|   [7]   | `MigrationsAssembly`       | provider option    | selects migration owner |
|   [8]   | `CommandTimeout`           | provider option    | sets command timeout    |

[ENTRYPOINT_SCOPE]: query and migration operations
- rail: store-provider

| [INDEX] | [SURFACE]              | [CALL_SHAPE]       | [CAPABILITY]             |
| :-----: | :--------------------- | :----------------- | :----------------------- |
|   [1]   | `HasPostgresExtension` | model extension    | declares extension       |
|   [2]   | `HasPostgresEnum`      | model extension    | declares enum            |
|   [3]   | `HasPostgresRange`     | model extension    | declares range           |
|   [4]   | `ForNpgsqlHasName`     | metadata extension | sets provider name       |
|   [5]   | `MigrationBuilder`     | migration surface  | emits relational changes |

## [4]-[IMPLEMENTATION_LAW]

[STORE_PROFILE]:
- profile: PostgreSQL EF is one provider implementation for the store-profile algebra
- provider root: `UseNpgsql`
- model root: EF relational model plus PostgreSQL annotations
- migration root: PostgreSQL operations and SQL generator
- query root: PostgreSQL method, member, JSON, array, range, and full-text translation

[LOCAL_ADMISSION]:
- PostgreSQL EF surfaces enter behind the same store-profile vocabulary as every provider.
- Provider-specific model annotations remain profile metadata.
- Extensions, enums, ranges, and NodaTime mapping require explicit profile declarations.
- Query translation facts live here and do not become public Persistence service families.

[RAIL_LAW]:
- Package: `Npgsql.EntityFrameworkCore.PostgreSQL`
- Owns: EF PostgreSQL provider admission
- Accept: PostgreSQL EF store profile
- Reject: provider-branded service families
