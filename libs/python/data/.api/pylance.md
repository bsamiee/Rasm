# [PY_DATA_API_PYLANCE]

`pylance` (dist `pylance`, module `lance`) supplies the Lance columnar dataset format with versioned, indexed, and vector-search-capable table storage for the data lance-format rail. The package owner reads and writes Lance datasets from local paths and cloud object stores, maintains a version history, and executes ANN vector search over indexed columns; it never re-implements the Lance columnar engine.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pylance`
- package: `pylance`
- import: `import lance` (module name is `lance`, not `pylance`)
- owner: `data`
- rail: lance-format
- capability: versioned columnar Lance dataset — Arrow-native read/write, merge-insert upserts, ANN vector index, version history, and blob column storage

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: dataset owners
- rail: lance-format

| [INDEX] | [SYMBOL]                   | [PACKAGE_ROLE]    | [CAPABILITY]                               |
| :-----: | :------------------------- | :---------------- | :----------------------------------------- |
|   [1]   | `lance.LanceDataset`       | dataset handle    | versioned Lance dataset with scanner rail  |
|   [2]   | `lance.LanceScanner`       | scan builder      | projection, filter, and batch scan config  |
|   [3]   | `lance.LanceFragment`      | fragment handle   | sub-file unit of a Lance dataset           |
|   [4]   | `lance.MergeInsertBuilder` | upsert builder    | conditional merge-insert operation         |
|   [5]   | `lance.LanceOperation`     | operation type    | schema for transactional dataset mutations |
|   [6]   | `lance.FragmentMetadata`   | fragment metadata | physical metadata for a fragment           |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: dataset I/O operations
- rail: lance-format

| [INDEX] | [SURFACE]                                                                    | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :--------------------------------------------------------------------------- | :------------- | :------------------------------- |
|   [1]   | `lance.dataset(uri, version, asof, block_size, *, storage_options, session)` | open           | open Lance dataset by URI        |
|   [2]   | `lance.write_dataset(data_obj, uri, schema, mode, *, max_rows_per_file)`     | write          | write Arrow data to Lance format |
|   [3]   | `LanceDataset.scanner(columns, filter, batch_size, batch_readahead)`         | scan           | build a column-filtered scanner  |
|   [4]   | `LanceDataset.to_table(columns, filter)`                                     | materialize    | scan to Arrow Table              |
|   [5]   | `LanceDataset.to_batches(columns, filter)`                                   | stream         | scan to Arrow RecordBatch stream |
|   [6]   | `LanceDataset.count_rows(filter)`                                            | aggregate      | count rows matching predicate    |

[ENTRYPOINT_SCOPE]: version and mutation operations
- rail: lance-format

| [INDEX] | [SURFACE]                                                           | [ENTRY_FAMILY] | [RAIL]                    |
| :-----: | :------------------------------------------------------------------ | :------------- | :------------------------ |
|   [1]   | `LanceDataset.versions()`                                           | history        | list all dataset versions |
|   [2]   | `LanceDataset.delete(predicate)`                                    | mutation       | delete rows by predicate  |
|   [3]   | `LanceDataset.merge_insert(on) -> MergeInsertBuilder`               | upsert         | conditional merge-insert  |
|   [4]   | `LanceDataset.create_index(column, index_type, *, metric, replace)` | index          | build ANN or scalar index |
|   [5]   | `lance.batch_udf(func, ...)`                                        | transform      | apply UDF to batches      |

## [4]-[IMPLEMENTATION_LAW]

[LANCE_TOPOLOGY]:
- versioning: each write appends a new version; `lance.dataset(uri, version=N)` opens a specific snapshot; `asof` resolves the latest version before a timestamp; `LanceDataset.versions()` lists the full version history, and the scalar current-version integer the `lakehouse/table.md` `_lance_receipt` reads (`lance.dataset(uri).version`) is RESEARCH-pending — the catalogue lists only the plural `versions()`, so the scalar `LanceDataset.version` property stays unconfirmed until `assay api` reflection against the live `pylance` abi3 distribution captures it. The lakehouse `[LANCE_VERSION]` arm flips from RESEARCH to settled with zero design change once reflection confirms the scalar accessor.
- Arrow native: all I/O passes Arrow `RecordBatch` or `Table`; `to_table` materializes; `to_batches` streams
- mode: `write_dataset` `mode` values are `"create"`, `"overwrite"`, `"append"`, and `"merge_insert"`
- index: `create_index` supports `index_type` values `"IVF_PQ"`, `"IVF_HNSW_PQ"`, and `"BTREE"`; `metric` values include `"L2"`, `"cosine"`, `"dot"`
- blob columns: `lance.Blob`, `BlobArray`, and `blob_field` own large-object storage with separate physical files
- merge-insert: `MergeInsertBuilder` chains `.when_matched_update_all()`, `.when_not_matched_insert_all()`, and `.execute(data)` calls

[RAIL_LAW]:
- Package: `pylance`
- Owns: versioned Lance columnar datasets, Arrow-native scan, merge-insert upsert, ANN vector index, and blob column storage
- Accept: Arrow `Table`/`RecordBatch` written to a Lance URI with explicit mode and schema
- Reject: hand-rolled versioning over Parquet; wrapper-renames of scanner or index operations lance owns
