<!-- catalog:DeltaLake.Net@0.32.0 -->
# [RASM_PERSISTENCE_API_DELTALAKE]

`DeltaLake.Net` supplies a managed Delta Lake read/write client over the Rust `delta-rs` + `delta-kernel` FFI bridge: the disposable `DeltaEngine`/`IEngine` factory (create/load a table) and the `DeltaTable`/`ITable` owner that exposes the full Delta protocol — Arrow-native reads (`QueryAsync` streaming `RecordBatch` via embedded DataFusion SQL, `ReadAsArrowTableAsync`, `ReadAsDataFrameAsync`), writes (`InsertAsync` of `RecordBatch`/`IArrowArrayStream` with `SaveMode`, MERGE/UPDATE/DELETE over SQL predicates), time-travel (`LoadVersionAsync`/`LoadDateTimeAsync`/`HistoryAsync`/`RestoreAsync`), maintenance (`OptimizeAsync` BinPack/Z-Order, `VacuumAsync`, `CheckpointAsync`, `AddConstraintsAsync`), and a metadata-only commit rail (`CreateWriteTransactionAsync` registering externally-written Parquet `AddAction`s with idempotent `AppId`/`TransactionVersion` txn markers). It is external Delta-warehouse interop beside the self-hosted DuckLake catalog and the `Apache.Arrow.Adbc` warehouse lane (`api-adbc-bigquery`); it composes the admitted `Apache.Arrow` `RecordBatch`/`Schema`/`Table`/`IArrowArrayStream` model (`api-arrow`) as its wire shape, registers Parquet files produced by `ParquetSharp` (`api-parquetsharp`) into the Delta log, and ships its own osx-arm64 native kernel.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `DeltaLake.Net`
- package: `DeltaLake.Net`
- version: `0.32.0`
- license: MIT
- assembly: `DeltaLake` (the package id is `DeltaLake.Net`; the bound assembly and root namespace are `DeltaLake`)
- namespace: `DeltaLake.Table`, `DeltaLake.Interfaces`, `DeltaLake.Errors`, `DeltaLake.Extensions` (the consumer surface); `DeltaLake.Bridge`, `DeltaLake.Bridge.Interop`, `DeltaLake.Kernel.*` are the internal `delta-rs`/`delta-kernel` FFI plumbing — not a consumer API
- target: multi-target (`net9.0`, `net8.0`, `net472`); the `net10.0` consumer binds `lib/net9.0` (the highest-precedence asset — there is no `net10.0` lib folder, so `net9.0` is the bound TFM, not a degraded fallback)
- native: `runtimes/<rid>/native` ships `libdelta_kernel_ffi`/`libdelta_rs_bridge` for `osx-arm64` (`.dylib`), `osx-x64` (`.dylib`), `linux-arm64`/`linux-x64` (`.so`), `win-x64` (`.dll`) — the Rust delta-kernel + delta-rs static bridge, RID-resolved and P/Invoke-loaded by `DeltaLake.Bridge.Runtime`
- transitive: `Apache.Arrow@21.0.0` (the workspace pins `23.0.0` — higher; Apache.Arrow rides the existing higher row, no diamond), `Microsoft.Data.Analysis@0.21.1` (the `DataFrame` returned by `ReadAsDataFrameAsync`), `Microsoft.Extensions.Logging.Abstractions@8.0.0`, `System.Text.Json@9.0.0`
- xml docs: present (`DeltaLake.xml` ships per TFM — member intent is doc-comment-sourced)
- rail: store-backend

The Arrow model is the wire shape on both edges: reads yield `Apache.Arrow.RecordBatch`/`Table`, writes accept `RecordBatch`/`IArrowArrayStream`. The package binds Arrow `21.0.0`; the workspace's `23.0.0` pin satisfies it. Storage is URI-routed (`memory://`, `s3://`, `azure://`, `gs://`) through `TableStorageOptions.StorageOptions`.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: engine and table owners
- rail: store-backend

`DeltaEngine : IEngine, IDisposable` wraps the native `delta-rs` runtime (one per `EngineOptions`, owns DataFusion config) and is the create/load factory; `DeltaTable : ITable, IDisposable` is the deep table owner returned from it. Both are disposable — the engine owns the native runtime, the table owns the native table handle.

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]    | [RAIL]                                          |
| :-----: | :-------------------- | :--------------- | :---------------------------------------------- |
|  [01]   | `DeltaEngine`         | engine root      | native runtime owner; `CreateTableAsync`/`LoadTableAsync` |
|  [02]   | `IEngine`             | engine contract  | the engine interface (DI seam)                  |
|  [03]   | `DeltaTable`          | table owner      | full Delta protocol read/write/maintenance/time-travel |
|  [04]   | `ITable`              | table contract   | the table interface (the documented operation set) |

[PUBLIC_TYPE_SCOPE]: options and create/storage policy
- rail: store-backend

The options form a record hierarchy rooted at `TableStorageOptions` (URI + `StorageOptions` map). `EngineOptions` tunes the embedded DataFusion executor (batch size, spill, temp dir).

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]     | [RAIL]                                         |
| :-----: | :--------------------- | :---------------- | :--------------------------------------------- |
|  [01]   | `TableStorageOptions`  | storage record    | `TableLocation` URI + `StorageOptions` map (S3/Azure/GCS creds) |
|  [02]   | `TableOptions`         | load record       | `TableStorageOptions` + `Version`/`WithoutFiles`/`LogBufferSize` |
|  [03]   | `TableCreateOptions`   | create record     | `TableStorageOptions` + Arrow `Schema`/`PartitionBy`/`SaveMode`/`Name`/`Configuration` |
|  [04]   | `EngineOptions`        | engine record     | DataFusion batch size, spill size, temp dir; `Default` static |
|  [05]   | `InsertOptions`        | insert record     | `Predicate`/`SaveMode`/`MaxRowsPerGroup`/`OverwriteSchema`; `IsValid` |
|  [06]   | `CommitOptions`        | commit record     | `CustomMetadata` + idempotent `AppId`/`TransactionVersion` (txn action) |
|  [07]   | `OptimizeOptions`      | optimize policy   | `OptimizeType`/`TargetSize`/`ZOrderColumns`/`MaxConcurrentTasks`/`MinCommitInterval` |
|  [08]   | `VacuumOptions`        | vacuum policy     | `DryRun`/`RetentionHours`/`VacuumMode`/`CustomMetadata` |
|  [09]   | `RestoreOptions`       | restore policy    | `Version` xor `Timestamp`, `IgnoreMissingFiles`, `ProtocolDowngradeAllowed` |
|  [10]   | `SelectQuery`          | query value       | DataFusion `Query` SQL + `TableAlias` (default `deltatable`) |

[PUBLIC_TYPE_SCOPE]: result, metadata, action records, and enums
- rail: store-backend

`AddAction` is the metadata-only commit unit (a pre-written Parquet file registered into the Delta log); `TableMetadata`/`CommitInfo`/`ProtocolInfo` are read-side metadata. The `SaveMode`/`OptimizeType`/`VacuumMode` enums are the protocol vocabulary.

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]    | [RAIL]                                          |
| :-----: | :-------------------- | :--------------- | :---------------------------------------------- |
|  [01]   | `AddAction`           | commit unit      | `record`; `Path`/`Size`/`PartitionValues`/`ModificationTime`/`DataChange`/`NumRecords`; `ToJson`/`FromJson` |
|  [02]   | `TableMetadata`       | metadata record  | `Id`/`Name`/`SchemaString`/`PartitionColumns`/`Configuration`/`CreatedTime` |
|  [03]   | `CommitInfo`          | history record   | `Timestamp`/`Operation`/`OperationParameters`/`ReadVersion`/`IsolationLevel`/`EngineInfo` |
|  [04]   | `ProtocolInfo`        | protocol struct  | `readonly struct (MinimumReaderVersion, MinimumWriterVersion)` |
|  [05]   | `SaveMode`            | write enum       | `Append`/`Overwrite`/`ErrorIfExists`/`Ignore`  |
|  [06]   | `OptimizeType`        | optimize enum    | `BinPack` / `ZOrder`                            |
|  [07]   | `VacuumMode`          | vacuum enum      | `Lite` / `Full`                                 |
|  [08]   | `DeltaLakeException`   | base failure     | `Exception`; carries native `ErrorCode`         |
|  [09]   | `DeltaConfigurationException` | config fault | `DeltaLakeException` code `1000` (bad options)  |
|  [10]   | `DeltaRuntimeException` | runtime fault   | `DeltaLakeException`; lifts a native `delta-rs` error |
|  [11]   | `DataFrameExtensions`  | DataFrame ext    | `ToMarkdown()`/`ToPrettyText()` on `Microsoft.Data.Analysis.DataFrame` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: engine, create, and load
- rail: store-backend

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :--------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `new DeltaEngine(EngineOptions)` / `EngineOptions.Default` | ctor      | builds the native runtime + DataFusion config |
|  [02]   | `DeltaEngine.CreateTableAsync(TableCreateOptions, ct)` | factory      | creates a table at the URI; returns `ITable` |
|  [03]   | `DeltaEngine.LoadTableAsync(TableOptions, ct)`       | factory        | loads an existing table at latest/pinned version |
|  [04]   | `new TableCreateOptions(location, Apache.Arrow.Schema)` + `{ PartitionBy, SaveMode, Configuration }` | object init | mandatory Arrow schema + partition/config policy |
|  [05]   | `new TableOptions { TableLocation, StorageOptions, Version }` | object init | URI + cloud-store creds + optional pinned version |

[ENTRYPOINT_SCOPE]: Arrow-native read (DataFusion)
- rail: store-backend

`QueryAsync` runs a DataFusion SQL `SELECT` over the table and streams `RecordBatch`es as an `IAsyncEnumerable` (the table is registered under `SelectQuery.TableAlias`). `ReadAsArrowTableAsync` materializes the whole table as an Arrow `Table`; `ReadAsDataFrameAsync` materializes a `Microsoft.Data.Analysis.DataFrame` (loads into memory).

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :--------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `QueryAsync(SelectQuery, ct)`                        | streaming read | `IAsyncEnumerable<RecordBatch>` over DataFusion SQL |
|  [02]   | `ReadAsArrowTableAsync(ct)`                          | bulk read      | `Apache.Arrow.Table` of the whole table      |
|  [03]   | `ReadAsDataFrameAsync(ct)`                           | dataframe read | `Microsoft.Data.Analysis.DataFrame` (in-memory) |
|  [04]   | `FilesAsync()` / `FileUrisAsync()`                   | file listing   | physical Parquet file paths/URIs             |
|  [05]   | `Schema()` / `Metadata()` / `ProtocolVersions()` / `Version()` / `Location()` | metadata | Arrow `Schema`, `TableMetadata`, `ProtocolInfo`, version, URI |

[ENTRYPOINT_SCOPE]: write, merge, and metadata-only commit
- rail: store-backend

`InsertAsync` appends/overwrites Arrow data (`RecordBatch` collection or `IArrowArrayStream`) per `InsertOptions.SaveMode`. `MergeAsync`/`UpdateAsync`/`DeleteAsync` run Delta MERGE/UPDATE/DELETE over SQL predicates. `CreateWriteTransactionAsync` is the metadata-only rail: it registers externally-written Parquet files (`AddAction`s) into the Delta log without rewriting data, with optional idempotent `AppId`/`TransactionVersion` txn markers — pair with `GetLatestTransactionVersionAsync` for exactly-once pre-checks.

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :--------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `InsertAsync(IReadOnlyCollection<RecordBatch>, Schema, InsertOptions, ct)` | write       | append/overwrite Arrow batches               |
|  [02]   | `InsertAsync(IArrowArrayStream, InsertOptions, ct)`                    | streaming write | append/overwrite from an Arrow stream        |
|  [03]   | `MergeAsync(query, IReadOnlyCollection<RecordBatch>, Schema, ct)`      | merge          | Delta MERGE over a SQL statement + source batches |
|  [04]   | `UpdateAsync(query, ct)`                                              | update         | SQL UPDATE; returns JSON stats               |
|  [05]   | `DeleteAsync(predicate, ct)` / `DeleteAsync(ct)`                      | delete         | predicate delete / delete-all                |
|  [06]   | `CreateWriteTransactionAsync(IReadOnlyList<AddAction>, CommitOptions?, ct)` | metadata commit | registers pre-written Parquet files in the Delta log |
|  [07]   | `GetLatestTransactionVersionAsync(appId, ct)`                         | idempotency    | last committed version for an `AppId` (pre-check) |
|  [08]   | `AddAction.ToJson()` / `AddAction.FromJson(json)`                    | action codec   | Delta-protocol add-action JSON round-trip    |

[ENTRYPOINT_SCOPE]: time-travel and maintenance
- rail: store-backend

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :--------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `HistoryAsync(limit?, ct)`                           | time-travel    | `CommitInfo[]` commit log                    |
|  [02]   | `LoadVersionAsync(version, ct)`                      | time-travel    | reload table state at a version              |
|  [03]   | `LoadDateTimeAsync(DateTimeOffset/long ms, ct)`      | time-travel    | reload table state as of a timestamp         |
|  [04]   | `UpdateIncrementalAsync(maxVersion?, ct)`            | refresh        | advance to latest (or pinned) version        |
|  [05]   | `RestoreAsync(RestoreOptions, ct)`                   | restore        | restore to a version xor timestamp           |
|  [06]   | `OptimizeAsync(OptimizeOptions, ct)`                 | compaction     | BinPack or Z-Order (`ZOrderColumns` required for ZOrder) |
|  [07]   | `VacuumAsync(VacuumOptions, ct)`                     | vacuum         | tombstone cleanup (`Lite`/`Full`, `RetentionHours`) |
|  [08]   | `CheckpointAsync(ct)`                                | checkpoint     | writes a Delta log checkpoint                |
|  [09]   | `AddConstraintsAsync(constraints, customMetadata?, ct)` | constraint  | adds CHECK constraints + metadata            |

## [04]-[IMPLEMENTATION_LAW]

[DELTALAKE_TOPOLOGY]:
- native bridge: `DeltaLake.Bridge.Runtime` P/Invoke-loads the RID-resolved `delta_kernel_ffi` + `delta_rs_bridge` library; `osx-arm64` ships `.dylib`. The engine query executor is the Rust-embedded DataFusion — `QueryAsync`/`MergeAsync`/`UpdateAsync` SQL is DataFusion SQL, not a managed parser.
- Arrow boundary: the wire shape on both edges is `Apache.Arrow` (`api-arrow`) — reads yield `RecordBatch`/`Table`, writes accept `RecordBatch`/`IArrowArrayStream` marshalled over the Arrow C Data Interface (`FFI_ArrowArray`/`FFI_ArrowSchema` in the internal kernel). `ReadAsDataFrameAsync` returns `Microsoft.Data.Analysis.DataFrame`.
- storage: URI-routed via `TableStorageOptions.TableLocation` (`memory://`, `s3://`, `azure://`, `gs://`) with credentials in the `StorageOptions` map — the same object-store residence as the AWS/Azure/GCS/Minio rows (`api-objectstore`), reached natively by delta-rs rather than the managed SDKs.
- disposal: `DeltaEngine` and `DeltaTable` both own native handles and are `IDisposable` — they bracket a `using` scope; the engine outlives the tables it mints.
- protocol: `ProtocolInfo` exposes the table's minimum reader/writer versions; `RestoreOptions.ProtocolDowngradeAllowed` gates downgrades.

[METADATA_COMMIT_RAIL]:
- `CreateWriteTransactionAsync` is the metadata-only write: Parquet files written externally (by `ParquetSharp`, `api-parquetsharp`) are registered into the Delta log as `AddAction`s without rewriting data — the load-bearing seam for a pipeline that produces columnar files out-of-band and publishes them transactionally.
- idempotency: `CommitOptions.AppId` + `TransactionVersion` write a Delta `txn` action. The kernel does NOT enforce uniqueness — duplicate appId/version pairs are accepted and the latest version wins on reconciliation. Exactly-once requires the caller to check `GetLatestTransactionVersionAsync` and skip when the returned version >= the batch version.

[LOCAL_ADMISSION]:
- DeltaLake enters behind the `Store/profiles` store-profile vocabulary as a distinct external-warehouse backend class, orthogonal to the self-hosted DuckLake catalog and the `Apache.Arrow.Adbc` warehouse drivers (`api-adbc-bigquery`).
- the engine/table lifecycle is profile-owned ceremony (`Store/lifecycle`): the native runtime and table handle are bracketed resources, not ambient singletons.
- writes are profile policy: `SaveMode`, partition columns, `OptimizeType`, and retention windows are declared on the profile, not chosen per-call.

[STACKING]:
- columnar-file commit: `ParquetSharp` (`api-parquetsharp`) writes a partition file out-of-band; the Persistence pipeline computes its `AddAction` (`Path`/`Size`/`PartitionValues`/`NumRecords`) and `CreateWriteTransactionAsync` registers it in the Delta log — the Parquet codec and the Delta catalog meet at the `AddAction`, never a re-serialization.
- Arrow round-trip: an analytical extract from DuckDB or ClickHouse (`api-duckdb`, `api-clickhouse`) lands as an `Apache.Arrow.RecordBatch` and `InsertAsync` appends it to a Delta table — one Arrow model spans the read backend and the Delta sink.
- snapshot codec: a `[ValueObject]`/`[SmartEnum]` owner crosses into a Delta column through the `api-thinktecture-serialization` factory projecting it to its key, written into the Arrow `RecordBatch` field that `InsertAsync` commits; the inverse decodes the read `RecordBatch` column. No hand-rolled mapping.
- time-travel seam: `HistoryAsync`/`LoadVersionAsync`/`LoadDateTimeAsync` back the `Version/timetravel` rail for an external Delta table — the Delta commit log is the version source, projected into the same time-travel vocabulary as the self-hosted catalog.
- fault rail: `DeltaRuntimeException`/`DeltaConfigurationException : DeltaLakeException` lift at the table edge discriminated on the native `ErrorCode`, joining the store-profile failure rail rather than surfacing a raw FFI fault.

[RAIL_LAW]:
- Package: `DeltaLake.Net`
- Owns: managed Delta Lake read/write over the `delta-rs`/`delta-kernel` FFI — Arrow-native reads (DataFusion SQL), writes, MERGE/UPDATE/DELETE, time-travel, maintenance, and the metadata-only `AddAction` commit rail
- Accept: bracketed `DeltaEngine`/`DeltaTable` resources, Arrow `RecordBatch`/`IArrowArrayStream` as the wire shape, URI-routed cloud storage, `CreateWriteTransactionAsync` registration of externally-written Parquet, and `DeltaLakeException` fault discrimination on `ErrorCode`
- Reject: confusing the package id `DeltaLake.Net` with the bound assembly/namespace `DeltaLake`, treating the `DeltaLake.Bridge`/`DeltaLake.Kernel` FFI plumbing as a consumer API, ambient (non-disposed) engine/table handles, and assuming `AppId`/`TransactionVersion` enforce uniqueness without the `GetLatestTransactionVersionAsync` pre-check
