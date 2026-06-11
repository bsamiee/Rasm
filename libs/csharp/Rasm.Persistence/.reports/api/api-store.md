# [RASM_PERSISTENCE_API_STORE]

Store APIs supply EF SQLite mapping, naming, migration tooling, raw SQLite access, native initialization, schema gates, backup, integrity, and PRAGMA policy.

## [1]-[SURFACES]

This table is a lookup by store package.

| [INDEX] | [PACKAGE]                         | [ASSEMBLY]                         | [LOCAL_RAIL] |
| :-----: | :-------------------------------- | :--------------------------------- | :----------- |
|   [1]   | `Microsoft.EntityFrameworkCore.Sqlite` | `Microsoft.EntityFrameworkCore.Sqlite` | ef-sqlite |
|   [2]   | `EFCore.NamingConventions`        | `EFCore.NamingConventions`         | schema       |
|   [3]   | `Microsoft.EntityFrameworkCore.Design` | `Microsoft.EntityFrameworkCore.Design` | migration |
|   [4]   | `Microsoft.Data.Sqlite`           | `Microsoft.Data.Sqlite`            | raw-sqlite   |
|   [5]   | `SQLitePCLRaw.bundle_e_sqlite3`   | native bundle package              | native       |

## [2]-[API_LOCATORS]

This table is a lookup by assembly.

| [INDEX] | [ASSEMBLY]                         | [NAMESPACE]                       | [USING]                            | [API_LOCATOR] |
| :-----: | :--------------------------------- | :-------------------------------- | :--------------------------------- | :------------ |
|   [1]   | `Microsoft.EntityFrameworkCore`    | `Microsoft.EntityFrameworkCore`   | `Microsoft.EntityFrameworkCore`    | `.cache/nuget/packages/microsoft.entityframeworkcore.sqlite/` |
|   [2]   | `EFCore.NamingConventions`         | `Microsoft.EntityFrameworkCore`   | `Microsoft.EntityFrameworkCore`    | `.cache/nuget/packages/efcore.namingconventions/` |
|   [3]   | `Microsoft.EntityFrameworkCore.Design` | `Microsoft.EntityFrameworkCore.Design` | `Microsoft.EntityFrameworkCore.Design` | `.cache/nuget/packages/microsoft.entityframeworkcore.design/` |
|   [4]   | `Microsoft.Data.Sqlite`            | `Microsoft.Data.Sqlite`           | `Microsoft.Data.Sqlite`            | `.cache/nuget/packages/microsoft.data.sqlite/` |
|   [5]   | `SQLitePCLRaw.bundle_e_sqlite3`    | `SQLitePCL`                       | `SQLitePCL`                        | `.cache/nuget/packages/sqlitepclraw.bundle_e_sqlite3/` |

## [3]-[CAPABILITIES]

This table is a lookup by type family.

| [INDEX] | [TYPE_FAMILY]          | [ENTRY_SURFACE]             | [LOCAL_RAIL] |
| :-----: | :--------------------- | :-------------------------- | :----------- |
|   [1]   | `DbContext`            | operation-scoped context    | ef-sqlite    |
|   [2]   | model builder APIs     | schema mapping              | schema       |
|   [3]   | migration APIs         | forward schema evolution    | migration    |
|   [4]   | `SqliteConnection`     | raw connection and backup   | raw-sqlite   |
|   [5]   | `SqliteCommand`        | PRAGMA and low-level query  | raw-sqlite   |
|   [6]   | `SQLitePCL.Batteries`  | native initialization       | native       |

## [4]-[REJECTED]

This table is a lookup by rejected package.

| [INDEX] | [REJECT]             | [LOCAL_RAIL] | [REASON]             |
| :-----: | :------------------- | :----------- | :------------------- |
|   [1]   | EF proxies           | schema       | shape generation owns admission |
|   [2]   | broad bulk extensions | raw-sqlite  | measured raw import first |
|   [3]   | SQLCipher bundle     | native       | file protection is profile policy |
