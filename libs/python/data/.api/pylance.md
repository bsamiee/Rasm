# [PY_DATA_API_PYLANCE]

`pylance` (dist `pylance` `7.0.0`, import module `lance`) supplies the Lance columnar dataset format with versioned, indexed, vector- and full-text-search-capable table storage for the data lance-format rail. The owner reads and writes Lance datasets from local paths and cloud object stores, maintains a linear+tag+branch version history, runs ANN vector search and BM25 full-text search over indexed columns, performs schema-evolving column add/alter/drop, transactional merge-insert upserts, and blob-column large-object storage; it never re-implements the Lance columnar engine, the Arrow scan pipeline, or the IVF/HNSW/inverted index kernels.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pylance`
- package: `pylance` `7.0.0`
- import: `import lance` (module name is `lance`, not `pylance`)
- license: Apache-2.0
- python: `>=3.9`
- native: Rust `lance` core via PyO3; depends on `pyarrow` and `numpy` (and `lance-namespace` for catalog binding)
- owner: `data`
- rail: lance-format
- capability: versioned columnar Lance dataset — Arrow-native scan with predicate/projection/limit/offset pushdown, ANN vector search (IVF_PQ / IVF_HNSW_PQ / IVF_HNSW_SQ) and scalar/full-text indices, transactional merge-insert upserts, schema evolution, blob column storage, version checkout / tags / branches / cleanup, and object-store `storage_options` egress

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: dataset, scan, query, and storage owners
- rail: lance-format

| [INDEX] | [SYMBOL]                              | [PACKAGE_ROLE]     | [CAPABILITY]                                                       |
| :-----: | :------------------------------------ | :----------------- | :----------------------------------------------------------------- |
|  [01]   | `lance.LanceDataset`                  | dataset handle     | versioned Lance dataset; scan, mutate, index, version, blob owner  |
|  [02]   | `lance.LanceScanner`                  | scan builder       | projection/filter/limit/nearest/FTS scan config to Arrow           |
|  [03]   | `lance.LanceFragment`                 | fragment handle    | sub-file unit; fragment-scoped scan and write-progress unit        |
|  [04]   | `lance.FragmentMetadata`              | fragment metadata  | physical metadata for a fragment (for `commit` transactions)       |
|  [05]   | `lance.MergeInsertBuilder`            | upsert builder     | conditional matched/not-matched merge-insert with conflict retry   |
|  [06]   | `lance.LanceOperation`                | operation algebra  | transactional dataset mutation cases for `commit`/`commit_batch`   |
|  [07]   | `lance.Transaction`                   | transaction record | a committed/uncommitted operation snapshot                         |
|  [08]   | `lance.Index` / `lance.IndexProgress` | index descriptor   | index metadata from `list_indices`; build-progress callback record |
|  [09]   | `lance.FullTextQuery`                 | FTS query algebra  | structured BM25 full-text query tree                               |
|  [10]   | `lance.Blob`                          | blob storage       | in-memory large-object value                                       |
|  [11]   | `lance.BlobArray`                     | blob storage       | Arrow extension array of blobs                                     |
|  [12]   | `lance.BlobColumn`                    | blob storage       | blob-typed column accessor                                         |
|  [13]   | `lance.BlobFile`                      | blob storage       | file-like handle to one blob in a separate physical file           |
|  [14]   | `lance.Session`                       | runtime session    | shared cache/handle reused across `dataset()` opens                |
|  [15]   | `lance.LanceNamespace`                | catalog binding    | namespace-client + `table_id` open over a Lance catalog            |
|  [16]   | `lance.ScanStatistics`                | scan stats         | per-scan statistics receipt (`scan_stats_callback`)                |
|  [17]   | `lance.DataStatistics`                | data stats         | dataset-level data statistics receipt                              |
|  [18]   | `lance.FieldStatistics`               | field stats        | per-field statistics receipt                                       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: dataset open and scan-to-Arrow
- rail: lance-format
- call: `lance.dataset(uri, version=None, asof=None, block_size=None, *, storage_options=None, default_scan_options=None, session=None, namespace_client=None, table_id=None) -> LanceDataset`
- call: `lance.write_dataset(data_obj, uri, schema=None, mode='create', *, max_rows_per_file=1048576, max_rows_per_group=1024, max_bytes_per_file, data_storage_version=None, storage_options=None, enable_stable_row_ids=False, auto_cleanup_options=None) -> LanceDataset`
- call: `LanceDataset.scanner(columns=None, filter=None, limit=None, offset=None, nearest=None, batch_size=None, full_text_query=None, *, prefilter=None, with_row_id=None, fast_search=None, use_scalar_index=None, order_by=None, late_materialization=None, blob_handling=None, scan_stats_callback=None) -> LanceScanner`; `to_table`/`to_batches` take the same projection/predicate/`nearest`/`full_text_query` args

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY] | [RAIL]                                    |
| :-----: | :------------------------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `lance.dataset(...)`                                           | open           | open dataset / snapshot by URI            |
|  [02]   | `lance.write_dataset(...)`                                     | write          | write Arrow/Reader to Lance               |
|  [03]   | `LanceDataset.scanner(...)`                                    | scan           | build column/predicate/vector/FTS scan    |
|  [04]   | `LanceDataset.to_table(...)`                                   | materialize    | scan to an Arrow `Table`                  |
|  [05]   | `LanceDataset.to_batches(...)`                                 | stream         | scan to an Arrow `RecordBatch` stream     |
|  [06]   | `LanceDataset.count_rows(filter=None)`                         | aggregate      | count rows matching a predicate           |
|  [07]   | `LanceDataset.take(indices, columns=None)`                     | gather         | row-index gather                          |
|  [08]   | `LanceDataset.take_blobs(blob_column, ids=None, indices=None)` | gather         | blob-column gather to `BlobFile`          |
|  [09]   | `LanceDataset.sample(num_rows, ...)` / `head(num_rows, ...)`   | sample         | random/head row sample                    |
|  [10]   | `LanceDataset.sql(sql)` / `filter(expr)`                       | query          | DataFusion-SQL builder; expression filter |
|  [11]   | `LanceDataset.join(...)` / `join_asof(...)` / `sort_by(...)`   | relational     | dataset-level join/asof-join/sort         |

[ENTRYPOINT_SCOPE]: mutation, schema evolution, and merge-insert
- rail: lance-format
- call: `LanceDataset.delete(predicate, *, conflict_retries=10, retry_timeout=30s) -> DeleteResult`; `LanceDataset.update(updates: dict[str,str], where=None, *, conflict_retries=10) -> UpdateResult`; `LanceDataset.add_columns(transforms, read_columns=None, batch_size=None)`; `LanceDataset.commit(operation: LanceOperation, *, read_version=None)` / `commit_batch([...])`

| [INDEX] | [SURFACE]                                                            | [ENTRY_FAMILY] | [RAIL]                                           |
| :-----: | :------------------------------------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `LanceDataset.insert(data_obj, *, mode='append')`                    | append         | append/overwrite rows into the dataset           |
|  [02]   | `LanceDataset.delete(...)`                                           | mutation       | delete rows by SQL/Arrow predicate               |
|  [03]   | `LanceDataset.update(...)`                                           | mutation       | SQL-expression column update with conflict retry |
|  [04]   | `LanceDataset.merge_insert(on)`                                      | upsert         | begin a conditional merge-insert                 |
|  [05]   | `LanceDataset.merge(data_obj, left_on, right_on=None)`               | column-merge   | add columns by joining an external reader        |
|  [06]   | `LanceDataset.add_columns(...)`                                      | schema         | add columns via SQL exprs / BatchUDF / reader    |
|  [07]   | `LanceDataset.alter_columns(*alterations)` / `drop_columns(columns)` | schema         | rename/retype/nullability change; drop cols      |
|  [08]   | `LanceDataset.commit(...)` / `commit_batch([...])`                   | transaction    | low-level transactional commit of operations     |

[ENTRYPOINT_SCOPE]: index, version, and maintenance
- rail: lance-format
- note: table surfaces are `LanceDataset` methods unless prefixed `lance.`
- note: scalar/FTS index kinds — `BTREE`/`BITMAP`/`LABEL_LIST`/`ZONEMAP`/`BLOOMFILTER`/`RTREE` (predicate) and `INVERTED`/`FTS`/`NGRAM` (BM25)
- call: `LanceDataset.create_index(column, index_type, *, name=None, metric='L2', replace=False, num_partitions=None, num_sub_vectors=None, ivf_centroids=None, pq_codebook=None, accelerator=None, train=True) -> LanceDataset`
- call: `LanceDataset.cleanup_old_versions(older_than: timedelta = None, retain_versions: int = None, *, delete_unverified=False, error_if_tagged_old_versions=True) -> CleanupStats`; `optimize.compact_files(*, target_rows_per_fragment, materialize_deletions, num_threads, batch_size) -> CompactionMetrics`

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY]   | [RAIL]                                               |
| :-----: | :---------------------------------------------------------- | :--------------- | :--------------------------------------------------- |
|  [01]   | `create_index(...)`                                         | vector index     | ANN index (`IVF_PQ`/`IVF_HNSW_PQ`/`IVF_HNSW_SQ`)     |
|  [02]   | `create_scalar_index(...)`                                  | scalar/FTS index | build a scalar or BM25 index (kinds in `- note:`)    |
|  [03]   | `list_indices` / `describe_indices` / `index_statistics`    | index admin      | enumerate and describe indices                       |
|  [04]   | `has_index` / `drop_index` / `prewarm_index`                | index admin      | probe, drop, and warm indices                        |
|  [05]   | `version` / `latest_version` / `versions`                   | snapshot         | current/latest version; full history                 |
|  [06]   | `checkout_version` / `checkout_latest` / `restore`          | time-travel      | open a snapshot; restore an old version as new       |
|  [07]   | `tags` / `branches` / `create_branch(name)`                 | tag/branch       | named version tags and branch refs (`Tags` accessor) |
|  [08]   | `optimize` / `.compact_files` / `.optimize_indices`         | maintenance      | compaction and ANN/scalar index rebuild              |
|  [09]   | `cleanup_old_versions(...)`                                 | maintenance      | prune old versions (`older_than` is a delta)         |
|  [10]   | `stats` / `io_stats_snapshot` / `io_stats_incremental`      | observability    | data/IO statistics receipts                          |
|  [11]   | `lance.iops_counter` / `lance.bytes_read_counter`           | observability    | process-wide IO counters                             |
|  [12]   | `lance.batch_udf(output_schema=None, checkpoint_file=None)` | helpers          | checkpointed resumable batch UDF                     |
|  [13]   | `lance.blob_field(name, *, nullable=True) -> pa.Field`      | helpers          | build a blob-typed Arrow field                       |
|  [14]   | `lance.json_to_schema(...)` / `lance.schema_to_json(...)`   | helpers          | schema JSON round-trip                               |

## [04]-[IMPLEMENTATION_LAW]

[LANCE_TOPOLOGY]:
- versioning: each write/mutation appends a new version. `lance.dataset(uri, version=)` opens a specific snapshot — `version` takes the `int` version number or the `str` tag name — `asof=ts` resolves the latest version before a timestamp, and `checkout_version`/`checkout_latest` switch the in-handle snapshot. `LanceDataset.version` is the scalar `int` of the checked-out snapshot (the value the `lakehouse/table.md` `_lance_receipt` reads), `latest_version` is the newest, and `versions()` lists full history. `tags` (named refs via the `Tags` accessor) and `branches`/`create_branch` carry semantic refs; `restore()` re-commits an old version as the new head.
- Arrow native: all I/O passes Arrow `RecordBatch`/`Table` (or any `ReaderLike`); `to_table` materializes, `to_batches` streams. `take(indices)` is a row-index gather; `sample`/`head` draw rows; `read_blobs`/`take_blobs` return `BlobFile` handles for large-object columns stored in separate physical files via `blob_field`.
- pushdown: `scanner`/`to_table`/`to_batches` push `columns` projection, `filter` (SQL string or `pa.compute.Expression`), `limit`/`offset`, `nearest` (vector), and `full_text_query` (BM25) into the Rust scan. `prefilter` applies the predicate before the vector/FTS stage; `use_scalar_index` and `fast_search` toggle index acceleration; `order_by` and `late_materialization` shape the physical plan. Client-side post-filtering after a full materialize is the anti-pattern.
- write mode: `write_dataset` `mode` is `"create"`, `"overwrite"`, or `"append"`. `data_storage_version` selects the file format (`'stable'`/`'2.1'`/`'next'`/…); `enable_stable_row_ids` fixes row identity across compaction; `auto_cleanup_options` schedules version pruning.
- vector index: `create_index(column, index_type=...)` builds `"IVF_PQ"`, `"IVF_HNSW_PQ"`, or `"IVF_HNSW_SQ"` with `metric` in `"L2"`/`"cosine"`/`"dot"`; tune `num_partitions` (IVF), `num_sub_vectors` (PQ), and optionally pass precomputed `ivf_centroids`/`pq_codebook` or a GPU `accelerator`. Query via `scanner(nearest={"column": .., "q": vec, "k": .., "nprobes": .., "refine_factor": ..})` or `to_table(nearest=...)`.
- scalar/FTS index: `create_scalar_index(column, index_type)` builds `"BTREE"`/`"BITMAP"`/`"LABEL_LIST"`/`"ZONEMAP"`/`"BLOOMFILTER"`/`"RTREE"` (predicate acceleration) or `"INVERTED"`/`"FTS"`/`"NGRAM"` (BM25 full-text). Full-text query is a structured tree — `MatchQuery`, `PhraseQuery`, `BooleanQuery`(with `Occur`), `BoostQuery`, `MultiMatchQuery` — passed as `full_text_query=`; a bare string is parsed to a default match.
- merge-insert: `merge_insert(on)` returns a `MergeInsertBuilder`; chain `.when_matched_update_all(condition=None)`, `.when_matched_delete()`, `.when_matched_fail()`, `.when_not_matched_insert_all()`, `.when_not_matched_by_source_delete()`, optional `.conflict_retries(n)`/`.retry_timeout(td)`/`.use_index(name)`, then `.execute(data_obj)` (or `.execute_uncommitted()` for a deferred commit; `.explain_plan()`/`.analyze_plan()` introspect). `update`/`delete` carry their own `conflict_retries`/`retry_timeout` and return `UpdateResult`/`DeleteResult`.
- schema evolution: `add_columns(transforms)` accepts SQL-expression dicts, a `BatchUDF` (use `lance.batch_udf(output_schema, checkpoint_file)` for resumable compute), a reader, or Arrow fields; `alter_columns(*AlterColumn)` renames/retypes/changes nullability; `drop_columns` removes columns — all in-place version-appending operations, never a full rewrite.
- maintenance: `optimize` is a `DatasetOptimizer` property. `optimize.compact_files(target_rows_per_fragment=)` merges small fragments and returns `CompactionMetrics` (`fragments_added`/`fragments_removed`/`files_added`/`files_removed: int`); `optimize.optimize_indices()` rebuilds ANN/scalar indices to cover newly written rows. `cleanup_old_versions(older_than: timedelta, retain_versions: int)` prunes and returns `CleanupStats` (`old_versions`/`bytes_removed`/`data_files_removed: int`) — `older_than` is a `datetime.timedelta`, not an absolute timestamp.
- transactions: `commit(LanceOperation.<case>)`/`commit_batch([...])` apply low-level transactional mutations from `FragmentMetadata`; `Transaction`/`get_transactions`/`read_transaction` expose the committed operation log. Reserve for distributed/staged writes; ordinary writes use `write_dataset`/`insert`/`merge_insert`.
- object store: `storage_options` (S3/GCS/Azure credentials and endpoint) is threaded on `dataset`/`write_dataset`/`create_index`; `Session` shares cache/handles across opens. Catalog binding uses `namespace_client`+`table_id` against a `LanceNamespace`.

[INTEGRATION_STACK]:
- `pyarrow` is the wire: `write_dataset` ingests any `pa.Table`/`RecordBatchReader`; `to_table`/`take`/`sample` egress `pa.Table`; `to_batches` yields `pa.RecordBatch`. A `polars`/`polars-st` frame reaches Lance through `pl.DataFrame.to_arrow()`, and a Lance scan feeds `pl.from_arrow(ds.to_table(...))` — Lance owns the versioned storage layer beneath a Polars/DataFusion query.
- `datafusion` reads through `LanceDataset.sql(...)` (returns a `SqlQueryBuilder`) and through Lance's DataFusion table-provider FFI (`FFILanceTableProvider`); register a Lance dataset as a DataFusion table to push joins/aggregates into the engine rather than materializing in Python.
- the vector rail stacks numpy embeddings -> `create_index(..., "IVF_HNSW_PQ", metric="cosine")` -> `scanner(nearest={"column":..,"q":np_vec,"k":..})` -> `pa.Table`, with `prefilter` applying a scalar predicate before the ANN stage; the FTS rail stacks `create_scalar_index(col, "INVERTED")` -> `full_text_query=BoostQuery([MatchQuery(...), PhraseQuery(...)])` for hybrid lexical+vector retrieval in one scan.
- blob columns (`blob_field`, `read_blobs`) hold geometry/raster/asset bytes beside row metadata; the cloud-egress and tensor-cube owners read `BlobFile` handles instead of separate object-store fetches.
- the lakehouse LANCE arms read `version` (scalar) for the receipt, accept an `int` snapshot / `str` tag on `version=` and a timestamp on `asof=` for `Read` time-travel, and re-head through `restore()` on the `Restore` row; `cleanup_old_versions`/`optimize.compact_files` are the retention/compaction maintenance the table owner schedules; `tags.create`/`create_branch` authoring is the lakehouse Growth-named residue.

[RAIL_LAW]:
- Package: `pylance`
- Owns: versioned Lance columnar datasets, Arrow-native scan with pushdown, ANN vector + scalar + BM25 full-text indices, transactional merge-insert/update/delete, schema evolution, blob column storage, version checkout/tags/branches/cleanup, and object-store egress
- Accept: Arrow `Table`/`RecordBatch`/reader written to a Lance URI with explicit `mode`/`schema`; scan via `scanner`/`to_table`/`to_batches` with predicate/projection/`nearest`/`full_text_query` pushdown; `merge_insert` for upsert; `create_index`/`create_scalar_index` for retrieval; `version`/`checkout_version`/`tags` for time-travel; `optimize`/`cleanup_old_versions` for maintenance
- Reject: hand-rolled versioning over Parquet; client-side post-filter after a full materialize where scan pushdown discriminates; wrapper-renames of scanner, index, or merge-insert operations lance owns; a `search_vector`/`search_text` method split where one `scanner(nearest=/full_text_query=)` discriminates by parameter
