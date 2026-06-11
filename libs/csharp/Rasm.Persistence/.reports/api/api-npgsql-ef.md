# [RASM_PERSISTENCE_API_NPGSQL_EF]

`Npgsql.EntityFrameworkCore.PostgreSQL` supplies EF Core PostgreSQL provider options, migrations, type mappings, and query translation.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Npgsql.EntityFrameworkCore.PostgreSQL`
- package: `Npgsql.EntityFrameworkCore.PostgreSQL`
- assembly: `Npgsql.EntityFrameworkCore.PostgreSQL`
- namespace: `Microsoft.EntityFrameworkCore`
- asset: runtime library
- rail: provider-store

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: EF PostgreSQL family
- rail: provider-store

| [INDEX] | [SYMBOL]                            | [PACKAGE_ROLE]      | [CAPABILITY]               |
| :-----: | :---------------------------------- | :------------------ | :------------------------- |
|   [1]   | `NpgsqlDbContextOptionsBuilder`     | provider options    | carries provider policy    |
|   [2]   | `NpgsqlModelBuilderExtensions`      | model extension     | maps PostgreSQL model      |
|   [3]   | `NpgsqlPropertyBuilderExtensions`   | property extension  | maps PostgreSQL property   |
|   [4]   | `NpgsqlMigrationBuilderExtensions`  | migration extension | emits PostgreSQL migration |
|   [5]   | `NpgsqlServiceCollectionExtensions` | service extension   | admits provider services   |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider operations
- rail: provider-store

| [INDEX] | [SURFACE]              | [CALL_SHAPE]      | [CAPABILITY]            |
| :-----: | :--------------------- | :---------------- | :---------------------- |
|   [1]   | `UseNpgsql`            | builder extension | applies provider policy |
|   [2]   | `ConfigureDataSource`  | provider option   | configures data source  |
|   [3]   | `UseNodaTime`          | provider option   | applies time mapping    |
|   [4]   | `HasPostgresExtension` | model extension   | declares extension      |
|   [5]   | `HasPostgresEnum`      | model extension   | maps enum type          |
|   [6]   | `HasPostgresRange`     | model extension   | maps range type         |

[EF_RELATIONAL_SUBSTRATE]:
- rail: provider-store

| [INDEX] | [SURFACE]            | [PACKAGE_ROLE]       | [CAPABILITY]               |
| :-----: | :------------------- | :------------------- | :------------------------- |
|   [1]   | `HasColumnType`      | relational substrate | maps provider type         |
|   [2]   | `MigrationsAssembly` | EF Core substrate    | selects migration owner    |
|   [3]   | `MigrationBuilder`   | relational substrate | emits relational migration |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `Npgsql.EntityFrameworkCore.PostgreSQL`
- Owns: PostgreSQL EF provider
- Accept: provider choice is store profile data
- Reject: separate PostgreSQL repositories
