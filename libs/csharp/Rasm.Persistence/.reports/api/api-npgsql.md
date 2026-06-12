# [RASM_PERSISTENCE_API_NPGSQL]

`Npgsql` supplies PostgreSQL data sources, connections, commands, transactions,
batches, parameters, type mapping, name translation, schema inspection, and
replication surfaces for provider store profiles.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Npgsql`
- package: `Npgsql`
- assembly: `Npgsql`
- namespace: `Npgsql`
- asset: runtime library
- rail: store-provider

## [2]-[PUBLIC_TYPES]

[CONNECTION_TYPES]: data source and command surfaces
- rail: store-provider

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE]      | [CAPABILITY]              |
| :-----: | :------------------------------ | :------------------ | :------------------------ |
|   [1]   | `NpgsqlDataSource`              | data source         | owns configured pool      |
|   [2]   | `NpgsqlDataSourceBuilder`       | data source builder | builds data source        |
|   [3]   | `NpgsqlConnection`              | connection          | opens PostgreSQL store    |
|   [4]   | `NpgsqlConnectionStringBuilder` | connection builder  | builds connection strings |
|   [5]   | `NpgsqlCommand`                 | command             | executes statements       |
|   [6]   | `NpgsqlTransaction`             | transaction         | bounds atomic work        |
|   [7]   | `NpgsqlBatch`                   | batch command       | executes batched work     |
|   [8]   | `NpgsqlBatchCommand`            | batch member        | carries batched command   |
|   [9]   | `NpgsqlParameter`               | parameter           | binds statement values    |
|  [10]   | `NpgsqlDataReader`              | data reader         | reads result rows         |
|  [11]   | `NpgsqlException`               | provider exception  | reports provider failure  |

[TYPE_SYSTEM_TYPES]: PostgreSQL type surfaces
- rail: store-provider

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE]  | [CAPABILITY]              |
| :-----: | :------------------------------ | :-------------- | :------------------------ |
|   [1]   | `NpgsqlDbType`                  | type classifier | classifies parameters     |
|   [2]   | `NpgsqlParameter<T>`            | typed parameter | binds typed values        |
|   [3]   | `NpgsqlTypeMapper`              | type mapper     | maps provider types       |
|   [4]   | `INpgsqlNameTranslator`         | name translator | maps CLR names            |
|   [5]   | `NpgsqlSnakeCaseNameTranslator` | name translator | maps snake case names     |
|   [6]   | `PostgresType`                  | schema metadata | describes store types     |
|   [7]   | `PostgresEnumType`              | schema metadata | describes enum types      |
|   [8]   | `PostgresCompositeType`         | schema metadata | describes composite types |
|   [9]   | `NpgsqlRange<T>`                | range value     | carries range values      |
|  [10]   | `NpgsqlInterval`                | interval value  | carries interval values   |

[REPLICATION_TYPES]: logical replication surfaces
- rail: store-provider

| [INDEX] | [SYMBOL]                         | [PACKAGE_ROLE]      | [CAPABILITY]             |
| :-----: | :------------------------------- | :------------------ | :----------------------- |
|   [1]   | `LogicalReplicationConnection`   | replication root    | opens logical stream     |
|   [2]   | `ReplicationSlot`                | slot metadata       | identifies slot          |
|   [3]   | `PgOutputReplicationOptions`     | replication policy  | configures pgoutput      |
|   [4]   | `TestDecodingReplicationOptions` | replication policy  | configures test decoding |
|   [5]   | `ReplicationMessage`             | replication message | carries stream event     |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: data source and execution
- rail: store-provider

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]        | [CAPABILITY]           |
| :-----: | :------------------------ | :------------------ | :--------------------- |
|   [1]   | `NpgsqlDataSource.Create` | factory call        | creates data source    |
|   [2]   | `Build`                   | builder call        | creates data source    |
|   [3]   | `Open`                    | connection call     | opens store connection |
|   [4]   | `OpenAsync`               | async call          | opens store connection |
|   [5]   | `CreateCommand`           | factory call        | creates command        |
|   [6]   | `BeginTransaction`        | transaction factory | starts transaction     |
|   [7]   | `ExecuteReader`           | command call        | reads rows             |
|   [8]   | `ExecuteNonQuery`         | command call        | writes changes         |
|   [9]   | `ExecuteScalar`           | command call        | reads scalar value     |
|  [10]   | `ExecuteBatch`            | batch call          | executes batch         |

[ENTRYPOINT_SCOPE]: mapping and replication
- rail: store-provider

| [INDEX] | [SURFACE]               | [CALL_SHAPE]     | [CAPABILITY]         |
| :-----: | :---------------------- | :--------------- | :------------------- |
|   [1]   | `MapEnum`               | builder mapping  | maps enum type       |
|   [2]   | `MapComposite`          | builder mapping  | maps composite type  |
|   [3]   | `EnableDynamicJson`     | builder mapping  | enables JSON mapping |
|   [4]   | `UseNodaTime`           | builder mapping  | enables time mapping |
|   [5]   | `StartReplication`      | replication call | starts replication   |
|   [6]   | `CreateReplicationSlot` | replication call | creates slot         |

## [4]-[IMPLEMENTATION_LAW]

[STORE_PROFILE]:
- profile: PostgreSQL is one admitted store profile
- data source root: `NpgsqlDataSource`
- connection root: `NpgsqlConnection`
- query root: commands, batches, parameters, and readers
- type root: provider type mapper, name translator, and PostgreSQL metadata

[LOCAL_ADMISSION]:
- PostgreSQL enters through the unified store-profile algebra.
- Provider type mapping stays profile configuration, not public service vocabulary.
- Logical replication is a store capability and requires explicit receipt projection.
- Data source lifetime, pooling, batching, and transaction policy are profile data.

[RAIL_LAW]:
- Package: `Npgsql`
- Owns: PostgreSQL transport API
- Accept: PostgreSQL store profile
- Reject: provider-specific public service families
