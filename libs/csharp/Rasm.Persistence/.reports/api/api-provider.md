# [RASM_PERSISTENCE_API_PROVIDER]

Provider APIs supply server-store profiles, provider-specific EF integration, connection identity, query translation, and provider receipt fields.

## [1]-[SURFACES]

This table is a lookup by provider package.

| [INDEX] | [PACKAGE]                         | [ASSEMBLY]                         | [LOCAL_RAIL] |
| :-----: | :-------------------------------- | :--------------------------------- | :----------- |
|   [1]   | `Npgsql`                          | `Npgsql`                           | provider     |
|   [2]   | `Npgsql.EntityFrameworkCore.PostgreSQL` | `Npgsql.EntityFrameworkCore.PostgreSQL` | provider |

## [2]-[API_LOCATORS]

This table is a lookup by assembly.

| [INDEX] | [ASSEMBLY]                         | [NAMESPACE]                       | [USING]                            | [API_LOCATOR] |
| :-----: | :--------------------------------- | :-------------------------------- | :--------------------------------- | :------------ |
|   [1]   | `Npgsql`                           | `Npgsql`                          | `Npgsql`                           | `.cache/nuget/packages/npgsql/` |
|   [2]   | `Npgsql.EntityFrameworkCore.PostgreSQL` | `Microsoft.EntityFrameworkCore` | `Microsoft.EntityFrameworkCore`    | `.cache/nuget/packages/npgsql.entityframeworkcore.postgresql/` |

## [3]-[CAPABILITIES]

This table is a lookup by type family.

| [INDEX] | [TYPE_FAMILY]      | [ENTRY_SURFACE]              | [LOCAL_RAIL] |
| :-----: | :----------------- | :--------------------------- | :----------- |
|   [1]   | `NpgsqlDataSource` | connection identity          | provider     |
|   [2]   | `NpgsqlConnection` | provider connection          | provider     |
|   [3]   | EF provider API    | provider-backed DbContext    | provider     |
|   [4]   | provider options   | store-profile policy         | provider     |
|   [5]   | query translation  | provider query receipt       | provider     |

## [4]-[REJECTED]

This table is a lookup by rejected package.

| [INDEX] | [REJECT]              | [LOCAL_RAIL] | [REASON]                     |
| :-----: | :-------------------- | :----------- | :--------------------------- |
|   [1]   | Npgsql public service | provider     | store profile owns selection |
|   [2]   | provider-branded repo | provider     | query algebra owns calls     |
