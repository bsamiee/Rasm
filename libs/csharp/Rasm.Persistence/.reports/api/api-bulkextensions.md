# [RASM_PERSISTENCE_API_BULKEXTENSIONS]

`EFCore.BulkExtensions` admits bulk insert, update, upsert, sync, delete, read,
save-change, truncate, batch update, batch delete, provider adapter, dialect,
bulk configuration, progress, statistics, and activity-source surfaces into the
store-provider rail.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `EFCore.BulkExtensions`
- package: `EFCore.BulkExtensions`
- core assembly: `EFCore.BulkExtensions.Core`
- provider assembly: `EFCore.BulkExtensions.Sqlite`
- provider assembly: `EFCore.BulkExtensions.PostgreSql`
- provider assembly: `EFCore.BulkExtensions.SqlServer`
- provider assembly: `EFCore.BulkExtensions.Oracle`
- namespace: `EFCore.BulkExtensions`
- namespace: `EFCore.BulkExtensions.SqlAdapters`
- asset: meta package plus provider adapters
- rail: store-provider

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: operation and configuration family
- rail: store-provider

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]       | [RAIL]                    |
| :-----: | :--------------------------- | :------------------ | :------------------------ |
|   [1]   | `DbContextBulkExtensions`    | DbContext extension | bulk operation entrypoint |
|   [2]   | `IQueryableBatchExtensions`  | query extension     | batch update/delete       |
|   [3]   | `IQueryableExtensions`       | query extension     | parameterized SQL view    |
|   [4]   | `BulkConfig`                 | policy value        | bulk operation policy     |
|   [5]   | `StatsInfo`                  | result value        | operation statistics      |
|   [6]   | `TimeStampInfo`              | result value        | timestamp conflict data   |
|   [7]   | `OperationType`              | operation enum      | bulk operation kind       |
|   [8]   | `InvalidBulkConfigException` | validation error    | invalid policy projection |

[PUBLIC_TYPE_SCOPE]: adapter and dialect family
- rail: store-provider

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]     | [RAIL]                     |
| :-----: | :---------------------------- | :---------------- | :------------------------- |
|   [1]   | `IDbServer`                   | provider contract | database identity          |
|   [2]   | `ISqlOperationsAdapter`       | adapter contract  | provider bulk operations   |
|   [3]   | `IQueryBuilderSpecialization` | dialect contract  | provider SQL differences   |
|   [4]   | `SqliteAdapter`               | provider adapter  | SQLite bulk operations     |
|   [5]   | `PostgreSqlAdapter`           | provider adapter  | PostgreSQL bulk operations |
|   [6]   | `SqlServerAdapter`            | provider adapter  | SQL Server bulk operations |
|   [7]   | `OracleAdapter`               | provider adapter  | Oracle bulk operations     |
|   [8]   | `TableInfo`                   | metadata value    | bulk table projection      |
|   [9]   | `ProgressHelper`              | progress helper   | progress callbacks         |
|  [10]   | `ActivitySources`             | telemetry helper  | bulk activity source       |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: DbContext bulk operations
- rail: store-provider

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY]      | [RAIL]                   |
| :-----: | :--------------------------------------------------------------- | :------------------ | :----------------------- |
|   [1]   | `BulkInsert` / `BulkInsertAsync`                                 | DbContext extension | insert movement          |
|   [2]   | `BulkInsertOrUpdate` / `BulkInsertOrUpdateAsync`                 | DbContext extension | upsert movement          |
|   [3]   | `BulkInsertOrUpdateOrDelete` / `BulkInsertOrUpdateOrDeleteAsync` | DbContext extension | sync movement            |
|   [4]   | `BulkUpdate` / `BulkUpdateAsync`                                 | DbContext extension | update movement          |
|   [5]   | `BulkDelete` / `BulkDeleteAsync`                                 | DbContext extension | delete movement          |
|   [6]   | `BulkRead` / `BulkReadAsync`                                     | DbContext extension | join-backed read         |
|   [7]   | `BulkSaveChanges` / `BulkSaveChangesAsync`                       | DbContext extension | change-tracker bulk save |
|   [8]   | `Truncate` / `TruncateAsync`                                     | DbContext extension | table truncate           |

[ENTRYPOINT_SCOPE]: policy and query operations
- rail: store-provider

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY]  | [RAIL]                        |
| :-----: | :----------------------------------------------- | :-------------- | :---------------------------- |
|   [1]   | `BatchUpdate` / `BatchUpdateAsync`               | query extension | set-based update              |
|   [2]   | `BatchDelete` / `BatchDeleteAsync`               | query extension | set-based delete              |
|   [3]   | `ToParametrizedSql`                              | query extension | SQL projection                |
|   [4]   | `SetSynchronizeFilter<T>`                        | bulk config     | sync subset filter            |
|   [5]   | `SetSynchronizeSoftDelete<T>`                    | bulk config     | sync soft-delete projection   |
|   [6]   | `UpdateByProperties`                             | bulk config     | alternate match keys          |
|   [7]   | `PropertiesToInclude` / `PropertiesToExclude`    | bulk config     | property movement policy      |
|   [8]   | `UnderlyingConnection` / `UnderlyingTransaction` | bulk config     | caller-owned connection scope |

## [4]-[IMPLEMENTATION_LAW]

[BULK_TOPOLOGY]:
- namespaces: `EFCore.BulkExtensions`, `EFCore.BulkExtensions.SqlAdapters`
- package shape: meta package admits core plus provider-specific adapters
- operation surface: insert, update, upsert, sync, delete, read, save changes, truncate
- query surface: batch update, batch delete, parameterized SQL projection
- configuration surface: batch size, streaming, temp table policy, graph inclusion, compare/update/include/exclude columns
- provider surface: SQLite, PostgreSQL, SQL Server, Oracle adapter and dialect families
- integrity surface: alternate match keys, timestamp conflict data, stats, progress, and activity source

[LOCAL_ADMISSION]:
- Bulk movement is a persistence boundary capability over admitted store profiles.
- Bulk operations do not replace domain invariants, migrations, or query shape ownership.
- Provider adapter differences stay behind store profiles and never leak as public persistence vocabulary.
- Bulk config is an explicit operation value; hidden global bulk policy is rejected.
- Caller-owned connection and transaction values are admitted only when the store boundary owns their lifetime.

[RAIL_LAW]:
- Package: `EFCore.BulkExtensions`
- Owns: provider-backed EF bulk movement
- Accept: explicit bulk import, sync, read, and truncate policy
- Reject: provider-specific bulk methods as public domain services
