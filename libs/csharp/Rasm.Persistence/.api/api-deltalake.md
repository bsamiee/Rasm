# [RASM_PERSISTENCE_API_DELTALAKE]

`DeltaLake.Net` owns a managed Delta Lake client over the Rust `delta-rs`/`delta-kernel` FFI: `DeltaEngine` mints and loads `DeltaTable` handles carrying the full Delta protocol — DataFusion-SQL Arrow reads, writes, MERGE/UPDATE/DELETE, time-travel, and maintenance. Its distinguishing seam is the metadata-only `AddAction` commit rail publishing externally-written Parquet transactionally without a data rewrite; it binds `Apache.Arrow` on both edges and enters as an external Delta-warehouse backend beside the DuckLake catalog and `Apache.Arrow.Adbc` drivers.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `DeltaLake.Net`
- package: `DeltaLake.Net` (MIT)
- assembly: `DeltaLake` — the bound assembly and root namespace, distinct from the package id `DeltaLake.Net`
- namespace: `DeltaLake.Table`, `DeltaLake.Interfaces`, `DeltaLake.Errors`, `DeltaLake.Extensions` — the consumer surface; `DeltaLake.Bridge`/`DeltaLake.Kernel.*` are internal FFI plumbing
- target: `net9.0` bound asset (`net8.0`/`net472` also shipped)
- native: `DeltaLake.Bridge.Runtime` P/Invoke-loads the RID-resolved `delta_kernel_ffi` + `delta_rs_bridge` Rust bridge — `osx-arm64`/`osx-x64` `.dylib`, `linux-*` `.so`, `win-x64` `.dll`
- depends: `Apache.Arrow` wire model, `Microsoft.Data.Analysis` (`DataFrame` egress)
- rail: store-backend

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: engine and table owners — `DeltaEngine`/`DeltaTable` both own native handles and are `IDisposable`; the engine mints and outlives the tables

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]   | [CAPABILITY]                                              |
| :-----: | :------------ | :-------------- | :-------------------------------------------------------- |
|  [01]   | `DeltaEngine` | engine root     | native runtime owner; `CreateTableAsync`/`LoadTableAsync` |
|  [02]   | `IEngine`     | engine contract | engine DI seam                                            |
|  [03]   | `DeltaTable`  | table owner     | full Delta protocol read/write/maintenance/time-travel    |
|  [04]   | `ITable`      | table contract  | the documented operation set                              |

[PUBLIC_TYPE_SCOPE]: create/load and storage policy — a `TableStorageOptions`-rooted record hierarchy; `EngineOptions` tunes the embedded DataFusion executor

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]   | [CAPABILITY]                                                                         |
| :-----: | :-------------------- | :-------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `TableStorageOptions` | storage record  | `TableLocation` URI + `StorageOptions` map (S3/Azure/GCS creds)                      |
|  [02]   | `TableOptions`        | load record     | `: TableStorageOptions` + `Version`/`WithoutFiles`/`LogBufferSize`                   |
|  [03]   | `TableCreateOptions`  | create record   | `: TableStorageOptions` + `Schema`/`PartitionBy`/`SaveMode`/`Name`/`Configuration`   |
|  [04]   | `EngineOptions`       | engine record   | DataFusion batch size, spill, temp dir; `Default` static                             |
|  [05]   | `InsertOptions`       | insert record   | `Predicate`/`SaveMode`/`MaxRowsPerGroup`/`OverwriteSchema`; `IsValid`                |
|  [06]   | `CommitOptions`       | commit record   | `CustomMetadata` + idempotent `AppId`/`TransactionVersion`                           |
|  [07]   | `OptimizeOptions`     | optimize policy | `OptimizeType`/`TargetSize`/`ZOrderColumns`/`MaxConcurrentTasks`/`MinCommitInterval` |
|  [08]   | `VacuumOptions`       | vacuum policy   | `DryRun`/`RetentionHours`/`VacuumMode`/`CustomMetadata`                              |
|  [09]   | `RestoreOptions`      | restore policy  | `Version` xor `Timestamp`, `IgnoreMissingFiles`, `ProtocolDowngradeAllowed`          |
|  [10]   | `SelectQuery`         | query value     | DataFusion `Query` SQL + `TableAlias` (default `deltatable`)                         |

[PUBLIC_TYPE_SCOPE]: metadata and action records — `AddAction` is the metadata-only commit unit (a pre-written Parquet file registered into the Delta log); the rest are read-side metadata

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]   | [CAPABILITY]                                                                              |
| :-----: | :-------------- | :-------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `AddAction`     | commit unit     | `record`; `Path`/`Size`/`PartitionValues`/`ModificationTime`/`DataChange`/`NumRecords`    |
|  [02]   | `TableMetadata` | metadata record | `Id`/`Name`/`SchemaString`/`PartitionColumns`/`Configuration`/`CreatedTime`               |
|  [03]   | `CommitInfo`    | history record  | `Timestamp`/`Operation`/`OperationParameters`/`ReadVersion`/`IsolationLevel`/`EngineInfo` |
|  [04]   | `ProtocolInfo`  | protocol struct | `readonly struct (MinimumReaderVersion, MinimumWriterVersion)`                            |

[PUBLIC_TYPE_SCOPE]: protocol enums — the Delta write/optimize/vacuum vocabulary

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :------------- | :------------ | :-------------------------------------------- |
|  [01]   | `SaveMode`     | write enum    | `Append`/`Overwrite`/`ErrorIfExists`/`Ignore` |
|  [02]   | `OptimizeType` | optimize enum | `BinPack` / `ZOrder`                          |
|  [03]   | `VacuumMode`   | vacuum enum   | `Lite` / `Full`                               |

[PUBLIC_TYPE_SCOPE]: failures and the DataFrame extension

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                                                           |
| :-----: | :---------------------------- | :------------ | :--------------------------------------------------------------------- |
|  [01]   | `DeltaLakeException`          | base failure  | `Exception`; carries native `ErrorCode`                                |
|  [02]   | `DeltaConfigurationException` | config fault  | `DeltaLakeException` code `1000` (bad options)                         |
|  [03]   | `DeltaRuntimeException`       | runtime fault | `DeltaLakeException` lifting a native `delta-rs` error                 |
|  [04]   | `DataFrameExtensions`         | DataFrame ext | `ToMarkdown()`/`ToPrettyText()` on `Microsoft.Data.Analysis.DataFrame` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: engine, create, and load

| [INDEX] | [SURFACE]                                                     | [SHAPE]     | [CAPABILITY]                                      |
| :-----: | :------------------------------------------------------------ | :---------- | :------------------------------------------------ |
|  [01]   | `new DeltaEngine(EngineOptions)` / `EngineOptions.Default`    | ctor        | builds the native runtime + DataFusion config     |
|  [02]   | `DeltaEngine.CreateTableAsync(TableCreateOptions, ct)`        | factory     | creates a table at the URI; returns `ITable`      |
|  [03]   | `DeltaEngine.LoadTableAsync(TableOptions, ct)`                | factory     | loads an existing table at latest/pinned version  |
|  [04]   | `new TableCreateOptions(location, Apache.Arrow.Schema)`       | object init | mandatory Arrow schema + partition/config policy  |
|  [05]   | `new TableOptions { TableLocation, StorageOptions, Version }` | object init | URI + cloud-store creds + optional pinned version |

[ENTRYPOINT_SCOPE]: Arrow-native read over embedded DataFusion — `QueryAsync` registers the table under `SelectQuery.TableAlias` and streams `RecordBatch`, the bulk reads materialize whole into memory

| [INDEX] | [SURFACE]                          | [SHAPE]        | [CAPABILITY]                                        |
| :-----: | :--------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `QueryAsync(SelectQuery, ct)`      | streaming read | `IAsyncEnumerable<RecordBatch>` over DataFusion SQL |
|  [02]   | `ReadAsArrowTableAsync(ct)`        | bulk read      | `Apache.Arrow.Table` of the whole table             |
|  [03]   | `ReadAsDataFrameAsync(ct)`         | dataframe read | `Microsoft.Data.Analysis.DataFrame` (in-memory)     |
|  [04]   | `FilesAsync()` / `FileUrisAsync()` | file listing   | physical Parquet file paths/URIs                    |
|  [05]   | `Schema()`                         | metadata       | Arrow `Schema`                                      |
|  [06]   | `Metadata()`                       | metadata       | `TableMetadata`                                     |
|  [07]   | `ProtocolVersions()`               | metadata       | `ProtocolInfo` reader/writer versions               |
|  [08]   | `Version()`                        | metadata       | current table version                               |
|  [09]   | `Location()`                       | metadata       | table URI                                           |

[ENTRYPOINT_SCOPE]: write, merge, and metadata-only commit — `CreateWriteTransactionAsync` registers externally-written Parquet as `AddAction`s without rewriting data, paired with `GetLatestTransactionVersionAsync` for exactly-once pre-checks

| [INDEX] | [SURFACE]                                                                   | [SHAPE]         | [CAPABILITY]                             |
| :-----: | :-------------------------------------------------------------------------- | :-------------- | :--------------------------------------- |
|  [01]   | `InsertAsync(IReadOnlyCollection<RecordBatch>, Schema, InsertOptions, ct)`  | write           | append/overwrite Arrow batches           |
|  [02]   | `InsertAsync(IArrowArrayStream, InsertOptions, ct)`                         | streaming write | append/overwrite from an Arrow stream    |
|  [03]   | `MergeAsync(query, IReadOnlyCollection<RecordBatch>, Schema, ct)`           | merge           | Delta MERGE over SQL + source batches    |
|  [04]   | `UpdateAsync(query, ct) -> Task<string>`                                    | update          | SQL UPDATE; returns JSON stats           |
|  [05]   | `DeleteAsync(predicate, ct)` / `DeleteAsync(ct)`                            | delete          | predicate delete / delete-all            |
|  [06]   | `CreateWriteTransactionAsync(IReadOnlyList<AddAction>, CommitOptions?, ct)` | metadata commit | registers pre-written Parquet in the log |
|  [07]   | `GetLatestTransactionVersionAsync(appId, ct)`                               | idempotency     | last committed version for an `AppId`    |
|  [08]   | `AddAction.ToJson()` / `AddAction.FromJson(json)`                           | action codec    | Delta add-action JSON round-trip         |

[ENTRYPOINT_SCOPE]: time-travel and maintenance

| [INDEX] | [SURFACE]                                               | [SHAPE]     | [CAPABILITY]                                             |
| :-----: | :------------------------------------------------------ | :---------- | :------------------------------------------------------- |
|  [01]   | `HistoryAsync(limit?, ct)`                              | time-travel | `CommitInfo[]` commit log                                |
|  [02]   | `LoadVersionAsync(version, ct)`                         | time-travel | reload table state at a version                          |
|  [03]   | `LoadDateTimeAsync(DateTimeOffset/long ms, ct)`         | time-travel | reload table state as of a timestamp                     |
|  [04]   | `UpdateIncrementalAsync(maxVersion?, ct)`               | refresh     | advance to latest (or pinned) version                    |
|  [05]   | `RestoreAsync(RestoreOptions, ct)`                      | restore     | restore to a version xor timestamp                       |
|  [06]   | `OptimizeAsync(OptimizeOptions, ct)`                    | compaction  | BinPack or Z-Order (`ZOrderColumns` required for ZOrder) |
|  [07]   | `VacuumAsync(VacuumOptions, ct)`                        | vacuum      | tombstone cleanup (`Lite`/`Full`, `RetentionHours`)      |
|  [08]   | `CheckpointAsync(ct)`                                   | checkpoint  | writes a Delta log checkpoint                            |
|  [09]   | `AddConstraintsAsync(constraints, customMetadata?, ct)` | constraint  | adds CHECK constraints + metadata                        |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- executor: the query engine is Rust-embedded DataFusion — `QueryAsync`/`MergeAsync`/`UpdateAsync` SQL is DataFusion SQL, never a managed parser.
- Arrow boundary: `Apache.Arrow` is the wire shape on both edges, marshalled over the Arrow C Data Interface; `ReadAsDataFrameAsync` alone crosses to `Microsoft.Data.Analysis.DataFrame`.
- storage: `TableStorageOptions.TableLocation` URI-routes the backend (`memory://`, `s3://`, `azure://`, `gs://`) with credentials in `StorageOptions`, reached natively by delta-rs rather than the managed cloud SDKs.
- disposal: `DeltaEngine` and `DeltaTable` own native handles under `using`; the engine outlives the tables it mints.
- idempotency: `CommitOptions.AppId` + `TransactionVersion` write a Delta `txn` action, but the kernel enforces no uniqueness — exactly-once requires a `GetLatestTransactionVersionAsync` pre-check that skips once the returned version reaches the batch version.

[STACKING]:
- `ParquetSharp`(`.api/api-parquetsharp.md`): a partition file written out-of-band computes its `AddAction` (`Path`/`Size`/`PartitionValues`/`NumRecords`), and `CreateWriteTransactionAsync` registers it in the Delta log — the Parquet codec and the Delta catalog meet at the `AddAction`, never a re-serialization.
- `Apache.Arrow`(`.api/api-arrow.md`): an analytical extract from DuckDB or ClickHouse (`.api/api-duckdb.md`, `.api/api-clickhouse.md`) lands as a `RecordBatch` and `InsertAsync` appends it — one Arrow model spans read backend and Delta sink.
- `Thinktecture` serialization(`.api/api-thinktecture-serialization.md`): a `[ValueObject]`/`[SmartEnum]` owner projects to its key into the Arrow `RecordBatch` field `InsertAsync` commits, and the inverse decodes the read column — no hand-rolled mapping.
- object store(`.api/api-objectstore.md`): delta-rs reaches the AWS/Azure/GCS/Minio residence natively, the same object-store the managed SDK rows serve.
- within-lib: `HistoryAsync`/`LoadVersionAsync`/`LoadDateTimeAsync` back the `Version/timetravel` rail for an external Delta table, and `DeltaRuntimeException`/`DeltaConfigurationException` lift at the table edge discriminated on the native `ErrorCode` onto the store-profile failure rail.

[LOCAL_ADMISSION]:
- DeltaLake enters behind the `Store/provisioning` store-profile vocabulary as a distinct external-warehouse backend class, orthogonal to the self-hosted DuckLake catalog and the `Apache.Arrow.Adbc` drivers.
- engine and table handles are bracketed profile-owned resources: the native runtime and table handle open under a `using` scope, never as ambient singletons.
- `SaveMode`, partition columns, `OptimizeType`, and retention windows declare on the profile, never per call.

[RAIL_LAW]:
- Package: `DeltaLake.Net`
- Owns: managed Delta Lake read/write over the `delta-rs`/`delta-kernel` FFI — Arrow-native DataFusion-SQL reads, writes, MERGE/UPDATE/DELETE, time-travel, maintenance, and the metadata-only `AddAction` commit rail
- Accept: bracketed `DeltaEngine`/`DeltaTable` resources, Arrow `RecordBatch`/`IArrowArrayStream` as the wire shape, URI-routed cloud storage, `CreateWriteTransactionAsync` registration of externally-written Parquet, and `DeltaLakeException` fault discrimination on `ErrorCode`
- Reject: confusing the package id `DeltaLake.Net` with the bound assembly/namespace `DeltaLake`, treating the `DeltaLake.Bridge`/`DeltaLake.Kernel` FFI plumbing as a consumer API, ambient non-disposed engine/table handles, and assuming `AppId`/`TransactionVersion` enforce uniqueness without the `GetLatestTransactionVersionAsync` pre-check
