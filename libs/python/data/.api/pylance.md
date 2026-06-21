# [PY_DATA_API_PYLANCE]

`pylance` (dist `pylance`, module `lance`) supplies the Lance columnar dataset format with versioned, indexed, and vector-search-capable table storage for the data lance-format rail. The package owner reads and writes Lance datasets from local paths and cloud object stores, maintains a version history, and executes ANN vector search over indexed columns; it never re-implements the Lance columnar engine.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pylance`
- package: `pylance`
- import: `import lance` (module name is `lance`, not `pylance`)
- owner: `data`
- rail: lance-format
- capability: versioned columnar Lance dataset — Arrow-native read/write, merge-insert upserts, ANN vector index, version history, and blob column storage

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: dataset owners
- rail: lance-format

| [INDEX] | [SYMBOL]                   | [PACKAGE_ROLE]    | [CAPABILITY]                               |
| :-----: | :------------------------- | :---------------- | :----------------------------------------- |
|  [01]   | `lance.LanceDataset`       | dataset handle    | versioned Lance dataset with scanner rail  |
|  [02]   | `lance.LanceScanner`       | scan builder      | projection, filter, and batch scan config  |
|  [03]   | `lance.LanceFragment`      | fragment handle   | sub-file unit of a Lance dataset           |
|  [04]   | `lance.MergeInsertBuilder` | upsert builder    | conditional merge-insert operation         |
|  [05]   | `lance.LanceOperation`     | operation type    | schema for transactional dataset mutations |
|  [06]   | `lance.FragmentMetadata`   | fragment metadata | physical metadata for a fragment           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: dataset I/O operations
- rail: lance-format

| [INDEX] | [SURFACE]                                                                    | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :--------------------------------------------------------------------------- | :------------- | :------------------------------- |
|  [01]   | `lance.dataset(uri, version, asof, block_size, *, storage_options, session)` | open           | open Lance dataset by URI        |
|  [02]   | `lance.write_dataset(data_obj, uri, schema, mode, *, max_rows_per_file)`     | write          | write Arrow data to Lance format |
|  [03]   | `LanceDataset.scanner(columns, filter, batch_size, batch_readahead)`         | scan           | build a column-filtered scanner  |
|  [04]   | `LanceDataset.to_table(columns, filter)`                                     | materialize    | scan to Arrow Table              |
|  [05]   | `LanceDataset.to_batches(columns, filter)`                                   | stream         | scan to Arrow RecordBatch stream |
|  [06]   | `LanceDataset.count_rows(filter)`                                            | aggregate      | count rows matching predicate    |

[ENTRYPOINT_SCOPE]: version and mutation operations
- rail: lance-format

| [INDEX] | [SURFACE]                                                                                                                                                                       | [ENTRY_FAMILY] | [RAIL]                             |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | :------------- | :--------------------------------- |
|  [01]   | `LanceDataset.version -> int`                                                                                                                                                   | snapshot       | scalar current checked-out version |
|  [02]   | `LanceDataset.versions()`                                                                                                                                                       | history        | list all dataset versions          |
|  [03]   | `LanceDataset.delete(predicate)`                                                                                                                                                | mutation       | delete rows by predicate           |
|  [04]   | `LanceDataset.merge_insert(on) -> MergeInsertBuilder`                                                                                                                           | upsert         | conditional merge-insert           |
|  [05]   | `LanceDataset.create_index(column, index_type, *, metric, replace)`                                                                                                             | index          | build ANN or scalar index          |
|  [06]   | `lance.batch_udf(func, ...)`                                                                                                                                                    | transform      | apply UDF to batches               |
|  [07]   | `LanceDataset.optimize -> DatasetOptimizer`                                                                                                                                     | maintenance    | compaction and index-rebuild owner |
|  [08]   | `DatasetOptimizer.compact_files(*, target_rows_per_fragment, max_rows_per_group, max_bytes_per_file, materialize_deletions, num_threads, batch_size, ...) -> CompactionMetrics` | maintenance    | compact small fragments            |
|  [09]   | `DatasetOptimizer.optimize_indices(**kwargs)`                                                                                                                                   | maintenance    | rebuild ANN/scalar indices         |
|  [10]   | `LanceDataset.cleanup_old_versions(older_than=None, retain_versions=None, *, delete_unverified=False, error_if_tagged_old_versions=True) -> CleanupStats`                       | maintenance    | prune old versions                 |

## [04]-[IMPLEMENTATION_LAW]

[LANCE_TOPOLOGY]:
- versioning: each write appends a new version; `lance.dataset(uri, version=N)` opens a specific snapshot; `asof` resolves the latest version before a timestamp; `LanceDataset.version` is a reflection-verified scalar `int` `@property` returning the currently checked-out version (`lance.dataset(uri).version`), the value the `lakehouse/table.md` `_lance_receipt` reads, while `LanceDataset.versions()` lists the full version history. The lakehouse `[LANCE_VERSION]` arm is settled: the scalar accessor and the plural history rail are distinct confirmed members.
- Arrow native: all I/O passes Arrow `RecordBatch` or `Table`; `to_table` materializes; `to_batches` streams
- mode: `write_dataset` `mode` values are `"create"`, `"overwrite"`, `"append"`, and `"merge_insert"`
- index: `create_index` supports `index_type` values `"IVF_PQ"`, `"IVF_HNSW_PQ"`, and `"BTREE"`; `metric` values include `"L2"`, `"cosine"`, `"dot"`
- blob columns: `lance.Blob`, `BlobArray`, and `blob_field` own large-object storage with separate physical files
- merge-insert: `MergeInsertBuilder` chains `.when_matched_update_all()`, `.when_not_matched_insert_all()`, and `.execute(data)` calls
- maintenance: `LanceDataset.optimize` is a `DatasetOptimizer` accessor; `compact_files(target_rows_per_fragment=)` merges small fragments and returns `CompactionMetrics` (`fragments_added`/`fragments_removed`/`files_added`/`files_removed: int`), `optimize_indices()` rebuilds indices; `LanceDataset.cleanup_old_versions(older_than: timedelta, retain_versions: int)` prunes versions and returns `CleanupStats` (`old_versions`/`bytes_removed`/`data_files_removed: int`) — `older_than` is a `datetime.timedelta`, not an absolute timestamp

[RAIL_LAW]:
- Package: `pylance`
- Owns: versioned Lance columnar datasets, Arrow-native scan, merge-insert upsert, ANN vector index, and blob column storage
- Accept: Arrow `Table`/`RecordBatch` written to a Lance URI with explicit mode and schema
- Reject: hand-rolled versioning over Parquet; wrapper-renames of scanner or index operations lance owns
