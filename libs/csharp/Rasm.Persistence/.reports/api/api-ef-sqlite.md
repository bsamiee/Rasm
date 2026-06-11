# [RASM_PERSISTENCE_API_EF_SQLITE]

`Microsoft.EntityFrameworkCore.Sqlite` supplies SQLite provider configuration, provider options, service admission, and SQLite migration surfaces.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.EntityFrameworkCore.Sqlite`
- package: `Microsoft.EntityFrameworkCore.Sqlite`
- assembly: `Microsoft.EntityFrameworkCore.Sqlite`
- namespace: `Microsoft.EntityFrameworkCore`
- asset: runtime library
- rail: store-provider

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: EF SQLite family
- rail: store-provider

| [INDEX] | [SYMBOL]                                  | [PACKAGE_ROLE]      | [CAPABILITY]             |
| :-----: | :---------------------------------------- | :------------------ | :----------------------- |
|   [1]   | `SqliteDbContextOptionsBuilder`           | provider options    | carries provider policy  |
|   [2]   | `SqliteDbContextOptionsBuilderExtensions` | builder extension   | admits SQLite provider   |
|   [3]   | `SqliteServiceCollectionExtensions`       | service extension   | admits provider services |
|   [4]   | `SqliteMigrationBuilderExtensions`        | migration extension | emits SQLite migration   |
|   [5]   | SQLite provider SQL generation            | provider service    | emits SQLite SQL         |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider operations
- rail: store-provider

| [INDEX] | [SURFACE]                | [CALL_SHAPE]      | [CAPABILITY]            |
| :-----: | :----------------------- | :---------------- | :---------------------- |
|   [1]   | `UseSqlite`              | builder extension | applies provider policy |
|   [2]   | `MigrationsAssembly`     | provider option   | selects migration owner |
|   [3]   | `CommandTimeout`         | provider option   | applies timeout policy  |
|   [4]   | `MinBatchSize`           | provider option   | applies batch policy    |
|   [5]   | `MaxBatchSize`           | provider option   | applies batch policy    |
|   [6]   | SQLite migration helpers | migration surface | emits SQLite migration  |

[EF_RELATIONAL_SUBSTRATE]:
- rail: store-provider

| [INDEX] | [SURFACE]                 | [PACKAGE_ROLE]       | [CAPABILITY]               |
| :-----: | :------------------------ | :------------------- | :------------------------- |
|   [1]   | `DbContextOptionsBuilder` | EF Core substrate    | carries provider options   |
|   [2]   | `ModelBuilder`            | EF Core substrate    | builds relational model    |
|   [3]   | `MigrationBuilder`        | relational substrate | emits relational migration |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `Microsoft.EntityFrameworkCore.Sqlite`
- Owns: embedded relational store lane
- Accept: SQLite is one store profile
- Reject: SQLite-first public services
