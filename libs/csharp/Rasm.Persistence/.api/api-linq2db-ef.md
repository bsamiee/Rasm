# [RASM_PERSISTENCE_API_LINQ2DB_EF]

`linq2db.EntityFrameworkCore` bridges EF Core contexts into LINQ To DB,
supplying bulk copy, query translation hand-off, data connection creation,
async LINQ operators, and bridge options.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `linq2db.EntityFrameworkCore`
- package: `linq2db.EntityFrameworkCore`
- assembly: `linq2db.EntityFrameworkCore`
- namespace: `LinqToDB.EntityFrameworkCore`
- asset: runtime library
- rail: store-provider

## [2]-[PUBLIC_TYPES]

[BRIDGE_TYPES]: EF-to-LINQ-To-DB bridge surfaces
- rail: store-provider

| [INDEX] | [SYMBOL]                           | [PACKAGE_ROLE]   | [CAPABILITY]               |
| :-----: | :--------------------------------- | :--------------- | :------------------------- |
|   [1]   | `LinqToDBForEFTools`               | bridge root      | bulk copies and bridges    |
|   [2]   | `LinqToDBForEFExtensions`          | async extension  | runs LINQ To DB operators  |
|   [3]   | `EFForEFExtensions`                | async extension  | runs EF operators          |
|   [4]   | `LinqToDBContextOptionsBuilder`    | bridge options   | configures bridge          |
|   [5]   | `ILinqToDBForEFTools`              | bridge contract  | abstracts bridge policy    |
|   [6]   | `LinqToDBForEFToolsImplDefault`    | bridge default   | implements bridge policy   |
|   [7]   | `LinqToDBExtensionsAdapter`        | operator adapter | adapts async operators     |
|   [8]   | `EFCoreMetadataReader`             | metadata reader  | projects EF model metadata |
|   [9]   | `LinqToDBForEFToolsDataConnection` | data connection  | owns bridged connection    |
|  [10]   | `LinqToDBForEFToolsDataContext`    | data context     | owns bridged context       |
|  [11]   | `LinqToDBForEFToolsException`      | bridge error     | carries bridge failure     |

[INFO_TYPES]: provider resolution surfaces
- rail: store-provider

| [INDEX] | [SYMBOL]               | [PACKAGE_ROLE]   | [CAPABILITY]              |
| :-----: | :--------------------- | :--------------- | :------------------------ |
|   [1]   | `EFProviderInfo`       | provider info    | resolves EF provider      |
|   [2]   | `EFConnectionInfo`     | connection info  | carries connection facts  |
|   [3]   | `LinqToDBProviderInfo` | provider mapping | names LINQ To DB provider |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: bulk copy and bridge hand-off
- rail: store-provider

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]      | [CAPABILITY]               |
| :-----: | :--------------------------------- | :---------------- | :------------------------- |
|   [1]   | `BulkCopy`                         | context extension | bulk inserts entities      |
|   [2]   | `BulkCopyAsync`                    | context extension | bulk inserts async streams |
|   [3]   | `ToLinqToDB`                       | query extension   | hands query to LINQ To DB  |
|   [4]   | `ToLinqToDBTable`                  | set extension     | projects `DbSet` to table  |
|   [5]   | `GetTable`                         | context extension | opens LINQ To DB table     |
|   [6]   | `Into`                             | context extension | starts value insert        |
|   [7]   | `CreateLinqToDBConnection`         | context extension | opens bridged connection   |
|   [8]   | `CreateLinqToDBContext`            | context extension | opens bridged context      |
|   [9]   | `CreateLinqToDBConnectionDetached` | context extension | opens detached connection  |
|  [10]   | `UseLinqToDB`                      | builder extension | admits bridge options      |
|  [11]   | `Initialize`                       | static call       | activates bridge           |
|  [12]   | `ClearCaches`                      | static call       | resets bridge caches       |

[ENTRYPOINT_SCOPE]: async operators
- rail: store-provider

| [INDEX] | [SURFACE]              | [CALL_SHAPE]    | [CAPABILITY]                |
| :-----: | :--------------------- | :-------------- | :-------------------------- |
|   [1]   | `ToListAsyncLinqToDB`  | query extension | materializes via LINQ To DB |
|   [2]   | `ToArrayAsyncLinqToDB` | query extension | materializes via LINQ To DB |
|   [3]   | `FirstAsyncLinqToDB`   | query extension | reads first via LINQ To DB  |
|   [4]   | `CountAsyncLinqToDB`   | query extension | counts via LINQ To DB       |
|   [5]   | `SumAsyncLinqToDB`     | query extension | aggregates via LINQ To DB   |
|   [6]   | `ToListAsyncEF`        | query extension | materializes via EF         |
|   [7]   | `FirstAsyncEF`         | query extension | reads first via EF          |
|   [8]   | `CountAsyncEF`         | query extension | counts via EF               |

[ENTRYPOINT_SCOPE]: bridge options
- rail: store-provider

| [INDEX] | [SURFACE]             | [CALL_SHAPE]      | [CAPABILITY]          |
| :-----: | :-------------------- | :---------------- | :-------------------- |
|   [1]   | `AddInterceptor`      | options call      | admits interceptor    |
|   [2]   | `AddMappingSchema`    | options call      | admits mapping schema |
|   [3]   | `AddCustomOptions`    | options call      | rewrites data options |
|   [4]   | `GetLinqToDBOptions`  | context extension | reads bridge options  |
|   [5]   | `EnableChangeTracker` | static property   | gates change tracking |

## [4]-[IMPLEMENTATION_LAW]

[STORE_PROFILE]:
- profile: the bridge is a bulk and query lane over admitted EF store profiles
- bulk root: `BulkCopy` and `BulkCopyAsync`
- query root: `ToLinqToDB` hand-off over EF queryables
- connection root: bridged data connections from the owning `DbContext`

[LOCAL_ADMISSION]:
- Bridge surfaces operate on contexts admitted through the store-profile algebra.
- Merge and output surfaces live in the LINQ To DB core assembly, not this bridge — `LinqToDB.LinqExtensions.MergeWithOutputAsync<TTarget, TSource, TOutput>(IMergeable<TTarget, TSource>, Expression<...>) : IAsyncEnumerable<TOutput>`; `LinqToDB.Data.DataContextExtensions.BulkCopyAsync<T>(IDataContext, BulkCopyOptions, IEnumerable<T>, CancellationToken)`.
- `LinqToDB.Data.BulkCopyOptions` carries `BulkCopyType` (enum `Default`/`RowByRow`/`MultipleRows`/`ProviderSpecific`; `ProviderSpecific` = pg binary COPY, `MultipleRows` = sqlite downgrade), `MaxBatchSize`, `WithoutSession`, `MaxDegreeOfParallelism`.
- Bulk copy options and rows-copied receipts are profile receipts.
- Bridge interceptors and mapping schemas require explicit profile declarations.

[RAIL_LAW]:
- Package: `linq2db.EntityFrameworkCore`
- Owns: EF-to-LINQ-To-DB bulk and query bridging
- Accept: bridge calls on admitted EF store profiles
- Reject: parallel query rails outside the store profile
