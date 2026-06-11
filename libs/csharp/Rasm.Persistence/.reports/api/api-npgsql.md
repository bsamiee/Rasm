# [RASM_PERSISTENCE_API_NPGSQL]

`Npgsql` supplies PostgreSQL data sources, connections, commands, transactions, batches, parameters, and type mapping.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Npgsql`
- package: `Npgsql`
- assembly: `Npgsql`
- namespace: `Npgsql`
- asset: runtime library
- rail: provider-store

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: PostgreSQL family
- rail: provider-store

| [INDEX] | [SYMBOL]                  | [PACKAGE_ROLE]     | [CAPABILITY]               |
| :-----: | :------------------------ | :----------------- | :------------------------- |
|   [1]   | `NpgsqlDataSource`        | transport handle   | owns transport state       |
|   [2]   | `NpgsqlDataSourceBuilder` | builder surface    | constructs configured root |
|   [3]   | `NpgsqlConnection`        | transport handle   | owns transport state       |
|   [4]   | `NpgsqlCommand`           | command surface    | executes user intent       |
|   [5]   | `NpgsqlTransaction`       | transaction handle | bounds atomic work         |
|   [6]   | `NpgsqlBatch`             | batch command      | executes store operation   |
|   [7]   | `NpgsqlParameter`         | SQL parameter      | executes store operation   |
|   [8]   | `NpgsqlException`         | provider exception | executes store operation   |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: PostgreSQL operations
- rail: provider-store

| [INDEX] | [SURFACE]          | [CALL_SHAPE]        | [CAPABILITY]              |
| :-----: | :----------------- | :------------------ | :------------------------ |
|   [1]   | `Create`           | factory call        | creates configured handle |
|   [2]   | `Build`            | factory call        | creates configured handle |
|   [3]   | `MapEnum`          | builder mapping     | maps enum type            |
|   [4]   | `Open`             | operation call      | executes operation        |
|   [5]   | `OpenAsync`        | async operation     | executes async work       |
|   [6]   | `CreateCommand`    | factory call        | creates configured handle |
|   [7]   | `BeginTransaction` | transaction factory | starts transaction        |
|   [8]   | `ExecuteReader`    | operation call      | executes operation        |
|   [9]   | `ExecuteNonQuery`  | operation call      | executes operation        |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `Npgsql`
- Owns: PostgreSQL provider lane
- Accept: PostgreSQL is one store profile
- Reject: provider-specific public service family
