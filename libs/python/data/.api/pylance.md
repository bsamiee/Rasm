# [PY_DATA_API_PYLANCE]

`pylance` binds the Lance columnar dataset format for the data lance-format rail: versioned, Arrow-native table storage with ANN vector, scalar, and BM25 full-text indices over local paths and cloud object stores, opening and committing version snapshots, evolving schema, upserting through merge-insert, and storing blob columns. Its Rust `lance` core owns the columnar engine, Arrow scan pipeline, and IVF/HNSW/inverted index kernels, never re-implemented here.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pylance`
- package: `pylance` (Apache-2.0, LanceDB)
- module: `import lance` (import name is `lance`, not `pylance`)
- native: Rust `lance` core via PyO3; depends `pyarrow`, `numpy`, `lance-namespace`
- owner: `data`
- rail: lance-format — versioned columnar datasets, Arrow-native pushdown scan, vector/scalar/FTS indices, transactional merge-insert, blob columns, multi-base storage, real-time MemWAL shard ingest, and object-store egress

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: dataset, scan, query, storage, and MemWAL streaming owners

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :------------------------------------ | :------------ | :----------------------------------------------------------------- |
|  [01]   | `lance.LanceDataset`                  | class         | versioned dataset handle; scan, mutate, index, version, blob owner |
|  [02]   | `lance.LanceScanner`                  | class         | scan-config builder: projection/filter/limit/nearest/FTS to Arrow  |
|  [03]   | `lance.LanceFragment`                 | class         | fragment handle; fragment-scoped scan and write-progress unit      |
|  [04]   | `lance.FragmentMetadata`              | class         | fragment physical metadata for `commit` transactions               |
|  [05]   | `lance.MergeInsertBuilder`            | class         | matched/not-matched merge-insert builder with conflict retry       |
|  [06]   | `lance.LanceOperation`                | union         | transactional mutation cases for `commit`/`commit_batch`           |
|  [07]   | `lance.Transaction`                   | class         | committed/uncommitted operation snapshot                           |
|  [08]   | `lance.Index` / `lance.IndexFile`     | class         | index metadata from `list_indices`; per-segment index-file record  |
|  [09]   | `lance.IndexProgress`                 | class         | index build-progress record                                        |
|  [10]   | `lance.query.FullTextQuery`           | union         | structured BM25 full-text query tree                               |
|  [11]   | `lance.Blob`                          | class         | in-memory large-object value                                       |
|  [12]   | `lance.BlobArray`                     | class         | Arrow extension array of blobs                                     |
|  [13]   | `lance.BlobColumn`                    | class         | blob-typed column accessor                                         |
|  [14]   | `lance.BlobFile`                      | class         | file-like handle to one blob in a separate physical file           |
|  [15]   | `lance.Session`                       | class         | shared cache/handle reused across `dataset()` opens                |
|  [16]   | `lance.LanceNamespace`                | class         | namespace-client + `table_id` open over a Lance catalog            |
|  [17]   | `lance.DatasetBasePath`               | class         | named storage base for multi-base datasets                         |
|  [18]   | `lance.ScanStatistics`                | class         | per-scan statistics receipt (`scan_stats_callback`)                |
|  [19]   | `lance.DataStatistics`                | class         | dataset-level data statistics receipt                              |
|  [20]   | `lance.FieldStatistics`               | class         | per-field statistics receipt                                       |
|  [21]   | `lance.ShardWriter`                   | class         | MemWAL shard writer streaming Arrow into WAL + MemTable            |
|  [22]   | `lance.LsmScanner`                    | class         | LSM-tier scanner over base + flushed generations + active MemTable |
|  [23]   | `lance.ExecutionPlan`                 | class         | physical plan emitted by the LSM planners                          |
|  [24]   | `lance.LsmPointLookupPlanner`         | class         | primary-key point-lookup planner across LSM tiers                  |
|  [25]   | `lance.LsmVectorSearchPlanner`        | class         | IVF vector-KNN planner across LSM tiers with staleness filtering   |
|  [26]   | `lance.ShardSnapshot`                 | class         | shard generation-snapshot builder for LSM reads                    |
|  [27]   | `lance.ShardingSpec` / `ShardingField`| class         | MemWAL shard-routing spec and derived-field definition             |
|  [28]   | `lance.MergedGeneration`              | class         | merged flushed-generation descriptor                               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: dataset open, write, and scan-to-Arrow
- scan carry: `columns`, `filter`, `limit`, `offset`, `nearest`, `full_text_query`, `prefilter`, `use_scalar_index`, `fast_search`, `order_by` (shared by `scanner`/`to_table`/`to_batches`)

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `lance.dataset(...)`                                           | factory  | open dataset / snapshot by URI            |
|  [02]   | `lance.write_dataset(...)`                                     | factory  | write Arrow/reader to Lance               |
|  [03]   | `LanceDataset.scanner(...)`                                    | factory  | build column/predicate/vector/FTS scan    |
|  [04]   | `LanceDataset.to_table(...)`                                   | instance | scan to an Arrow `Table`                  |
|  [05]   | `LanceDataset.to_batches(...)`                                 | instance | scan to an Arrow `RecordBatch` stream     |
|  [06]   | `LanceDataset.count_rows(filter=None)`                         | instance | count rows matching a predicate           |
|  [07]   | `LanceDataset.take(indices, columns=None)`                     | instance | row-index gather                          |
|  [08]   | `LanceDataset.take_blobs(blob_column, ids=None, indices=None)` | instance | blob-column gather to `BlobFile`          |
|  [09]   | `LanceDataset.sample(num_rows, ...)` / `head(num_rows, ...)`   | instance | random/head row sample                    |
|  [10]   | `LanceDataset.sql(sql)` / `filter(expr)`                       | factory  | DataFusion-SQL builder; expression filter |
|  [11]   | `LanceDataset.join(...)` / `join_asof(...)` / `sort_by(...)`   | instance | dataset-level join/asof-join/sort         |

[ENTRYPOINT_SCOPE]: mutation, schema evolution, and merge-insert

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `LanceDataset.insert(data_obj, *, mode='append')`                    | instance | append/overwrite rows into the dataset           |
|  [02]   | `LanceDataset.delete(...)`                                           | instance | delete rows by SQL/Arrow predicate               |
|  [03]   | `LanceDataset.update(...)`                                           | instance | SQL-expression column update with conflict retry |
|  [04]   | `LanceDataset.merge_insert(on)`                                      | factory  | begin a conditional merge-insert                 |
|  [05]   | `LanceDataset.merge(data_obj, left_on, right_on=None)`               | instance | add columns by joining an external reader        |
|  [06]   | `LanceDataset.add_columns(...)`                                      | instance | add columns via SQL exprs / BatchUDF / reader    |
|  [07]   | `LanceDataset.alter_columns(*alterations)` / `drop_columns(columns)` | instance | rename/retype/nullability change; drop cols      |
|  [08]   | `LanceDataset.commit(...)` / `commit_batch([...])`                   | instance | low-level transactional commit of operations     |

[ENTRYPOINT_SCOPE]: index, version, and maintenance
- note: surfaces are `LanceDataset` methods unless prefixed `lance.`

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                                         |
| :-----: | :---------------------------------------------------------- | :------- | :--------------------------------------------------- |
|  [01]   | `create_index(...)`                                         | instance | ANN index (`IVF_PQ`/`IVF_HNSW_PQ`/`IVF_HNSW_SQ`/`IVF_RQ`) |
|  [02]   | `create_scalar_index(...)`                                  | instance | build a scalar or BM25 index                         |
|  [03]   | `list_indices` / `describe_indices` / `index_statistics`    | instance | enumerate and describe indices                       |
|  [04]   | `has_index` / `drop_index` / `prewarm_index`                | instance | probe, drop, and warm indices                        |
|  [05]   | `version` / `latest_version` / `versions`                   | property | current/latest version; full history                 |
|  [06]   | `checkout_version` / `checkout_latest` / `restore`          | instance | open a snapshot; restore an old version as new       |
|  [07]   | `tags` / `branches` / `create_branch(name)`                 | property | named version tags and branch refs (`Tags` accessor) |
|  [08]   | `optimize` / `.compact_files` / `.optimize_indices`         | property | compaction and ANN/scalar index rebuild              |
|  [09]   | `cleanup_old_versions(...)`                                 | instance | prune old versions (`older_than` is a delta)         |
|  [10]   | `stats` / `io_stats_snapshot` / `io_stats_incremental`      | instance | data/IO statistics receipts                          |
|  [11]   | `lance.iops_counter` / `lance.bytes_read_counter`           | static   | process-wide IO counters                             |
|  [12]   | `lance.batch_udf(output_schema=None, checkpoint_file=None)` | static   | checkpointed resumable batch UDF                     |
|  [13]   | `lance.blob_field(name, *, nullable=True) -> pa.Field`      | static   | build a blob-typed Arrow field                       |
|  [14]   | `lance.blob_array(values) -> BlobArray`                     | static   | build a blob Arrow extension array                   |
|  [15]   | `lance.json_to_schema(...)` / `lance.schema_to_json(...)`   | static   | schema JSON round-trip                               |
|  [16]   | `tracked_files(*, min_version=None, progress=None)`         | instance | stream every file any manifest version references    |
|  [17]   | `all_files()`                                               | instance | stream every file physically present at the base URI |
|  [18]   | `add_bases(new_bases, transaction_properties=None)`         | instance | register named `DatasetBasePath` storage locations   |

[ENTRYPOINT_SCOPE]: MemWAL streaming ingest and LSM-tier read
- planner carry: `LsmPointLookupPlanner`/`LsmVectorSearchPlanner` take `(dataset, shard_snapshots, ...)` and emit an `ExecutionPlan` run via `to_table`/`to_reader`/`to_batches`

| [INDEX] | [SURFACE]                                                                | [SHAPE]  | [CAPABILITY]                                    |
| :-----: | :----------------------------------------------------------------------- | :------- | :---------------------------------------------- |
|  [01]   | `LanceDataset.initialize_mem_wal(...)`                                   | instance | enable the MemWAL write path on the dataset     |
|  [02]   | `LanceDataset.mem_wal_writer(shard_id, *, durable_write=None, ...)`      | factory  | open a per-shard `ShardWriter`                  |
|  [03]   | `ShardWriter.put(data, *, schema=None)` / `.close()`                     | instance | stream Arrow into WAL + MemTable; flush + close |
|  [04]   | `ShardWriter.lsm_scanner(shard_snapshots=None)`                          | factory  | read-your-writes scanner incl. active MemTable  |
|  [05]   | `ShardWriter.stats()` / `.memtable_stats()`                              | instance | write and MemTable statistics dicts             |
|  [06]   | `LsmScanner.from_snapshots(dataset, shard_snapshots)`                    | static   | scanner over base + flushed generations         |
|  [07]   | `LsmScanner.project(columns)` / `.filter(expr)` / `.limit(n, offset)`    | instance | LSM scan projection/predicate/slice build       |
|  [08]   | `LsmScanner.to_table()` / `.to_batches()` / `.count_rows()`              | instance | materialize the LSM scan to Arrow               |
|  [09]   | `LsmPointLookupPlanner.plan_lookup(pk_value, columns) -> ExecutionPlan` | instance | plan a primary-key point lookup             |
|  [10]   | `LsmVectorSearchPlanner.plan_search(query, k=10, ...) -> ExecutionPlan`  | instance | plan LSM-wide KNN vector search         |
|  [11]   | `ExecutionPlan.to_table()` / `.to_reader()` / `.explain()`              | instance | execute or explain a planned LSM operation      |
|  [12]   | `lance.evaluate_sharding_spec(batch, spec, schema) -> pa.RecordBatch`    | static   | derive shard-routing values for one batch       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- versioning: each write/mutation appends a version; `dataset(uri, version=, asof=)` opens a snapshot — `version` takes the `int` number or `str` tag, `asof` the latest before a timestamp — and `checkout_version`/`checkout_latest` switch the in-handle snapshot. `version` is the checked-out `int`, `latest_version` the newest, `versions()` the history; `tags` (via `Tags`) and `branches`/`create_branch` carry semantic refs; `restore()` re-commits an old version as head.
- write mode: `write_dataset` `mode` is `create`/`overwrite`/`append`, `data_storage_version` selects the file format, `enable_stable_row_ids` fixes row identity across compaction, `auto_cleanup_options` schedules pruning.
- Arrow wire: all I/O is Arrow `RecordBatch`/`Table` or any reader — `to_table` materializes, `to_batches` streams, `take(indices)` gathers by row index, `sample`/`head` draw rows, `take_blobs`/`read_blobs` return `BlobFile` handles for large-object columns stored in separate files via `blob_field`.
- pushdown: `scanner`/`to_table`/`to_batches` push `columns`, `filter` (SQL string or `pa.compute.Expression`), `limit`/`offset`, `nearest`, and `full_text_query` into the Rust scan; `prefilter` applies the predicate before the vector/FTS stage, `use_scalar_index`/`fast_search` toggle index acceleration, `order_by`/`late_materialization` shape the physical plan.
- vector index: `create_index(column, index_type=)` builds `IVF_FLAT`/`IVF_PQ`/`IVF_SQ`/`IVF_HNSW_FLAT`/`IVF_HNSW_PQ`/`IVF_HNSW_SQ`/`IVF_RQ` (RaBitQ, `rabitq_model` pins the rotation for distributed builds) with `metric` `L2`/`cosine`/`dot`, tuning `num_partitions` (IVF) and `num_sub_vectors` (PQ) or passing precomputed `ivf_centroids`/`pq_codebook` or a GPU `accelerator` (`IVF_PQ` only); query via `nearest={"column":, "q":vec, "k":, "minimum_nprobes":, "maximum_nprobes":, "refine_factor":, "distance_range":, "use_index":}` on `scanner`/`to_table` — `q` as a list of vectors runs a batch search prepending an Int32 `query_index` column.
- scalar/FTS index: `create_scalar_index(column, index_type)` builds `BTREE`/`BITMAP`/`LABEL_LIST`/`ZONEMAP`/`BLOOMFILTER`/`RTREE` (predicate) or `INVERTED`/`FTS`/`NGRAM` (BM25; `NGRAM` also accelerates regex and infix `LIKE`); `full_text_query=` takes a structured tree from `lance.query` — `MatchQuery`, `PhraseQuery`, `BooleanQuery`(with `Occur`), `BoostQuery`, `MultiMatchQuery` — and a bare string parses to a default match, tokenized with ICU by default.
- merge-insert: `merge_insert(on)` returns a `MergeInsertBuilder`; chain `.when_matched_update_all(condition=)`, `.when_matched_delete()`, `.when_matched_fail()`, `.when_not_matched_insert_all()`, `.when_not_matched_by_source_delete()`, optional `.conflict_retries(n)`/`.retry_timeout(td)`/`.use_index(name)`, then `.execute(data_obj)` (`.execute_uncommitted()` defers the commit; `.explain_plan()`/`.analyze_plan()` introspect). `update`/`delete` carry `conflict_retries`/`retry_timeout` and return `UpdateResult`/`DeleteResult`.
- schema evolution: `add_columns(transforms)` takes SQL-expression dicts, a `BatchUDF` (`lance.batch_udf(output_schema, checkpoint_file)` for resumable compute), a reader, or Arrow fields; `alter_columns(*AlterColumn)` renames/retypes/changes nullability; `drop_columns` removes — all version-appending, never a full rewrite.
- maintenance: `optimize` is a `DatasetOptimizer` property; `optimize.compact_files(target_rows_per_fragment=)` merges small fragments returning `CompactionMetrics`, `optimize.optimize_indices()` rebuilds ANN/scalar indices over newly written rows; `cleanup_old_versions(older_than: timedelta, retain_versions)` prunes returning `CleanupStats` — `older_than` is a `timedelta`, not an absolute timestamp.
- transactions: `commit(LanceOperation.<case>)`/`commit_batch([...])` apply low-level mutations from `FragmentMetadata`, and `Transaction`/`get_transactions`/`read_transaction` expose the operation log — reserve for distributed/staged writes, ordinary writes use `write_dataset`/`insert`/`merge_insert`.
- object store: `storage_options` (S3/GCS/Azure credentials and endpoint) threads on `dataset`/`write_dataset`/`create_index`, `Session` shares cache/handles across opens, and catalog binding uses `namespace_client`+`table_id` against a `LanceNamespace`.
- multi-base storage: `write_dataset(initial_bases=[DatasetBasePath], target_bases=, base_store_params=)` seeds named storage roots and `add_bases([DatasetBasePath])` registers more; `external_blob_mode` (`reference`/`ingest`), `allow_external_blob_outside_bases`, and `blob_pack_file_size_threshold` govern where blob bytes land across bases.
- MemWAL ingest: `initialize_mem_wal(...)` arms the write path, then `mem_wal_writer(shard_id)` returns a `ShardWriter` whose `put`/`close` stream Arrow into a durable WAL + in-memory MemTable; each `shard_id` is an independent parallel write path. `ShardWriter.lsm_scanner()` reads base + flushed generations + the active MemTable (read-your-writes), `LsmScanner.from_snapshots(dataset, snapshots)` reads base + flushed only, and `LsmPointLookupPlanner`/`LsmVectorSearchPlanner` plan PK / IVF-KNN reads across the tiers with staleness filtering (`overfetch_factor`), emitting an `ExecutionPlan`; `ShardingSpec`/`ShardingField`/`evaluate_sharding_spec` derive per-batch shard routing.
- storage introspection: `tracked_files(min_version=, progress=)` streams every file any manifest version references and `all_files()` streams every file physically at the base URI, both as `pa.RecordBatchReader` — the retention/audit inputs `cleanup_old_versions` consumes.

[STACKING]:
- `pyarrow`(`.api/pyarrow.md`): the wire both ways — `write_dataset` ingests any `pa.Table`/`RecordBatchReader`, `to_table`/`take`/`sample` egress `pa.Table`, `to_batches` yields `pa.RecordBatch`; `ShardWriter.put` ingests any `pa.Table`/`RecordBatch`/reader, `LsmScanner.to_table`/`ExecutionPlan.to_table` egress `pa.Table` and `.to_reader()` a `pa.RecordBatchReader`, and `tracked_files`/`all_files` stream `pa.RecordBatchReader` file inventories.
- `datafusion`(`.api/datafusion.md`): `LanceDataset.sql(...)` returns a `SqlQueryBuilder`, and the DataFusion table-provider FFI (`FFILanceTableProvider`) registers a dataset so joins/aggregates push into the engine rather than materializing in Python.
- `polars`(`.api/polars.md`): a frame crosses via `pl.DataFrame.to_arrow()` in and `pl.from_arrow(ds.to_table(...))` out, Lance owning the versioned storage layer beneath the Polars/DataFusion query.
- within-lib retrieval: numpy embeddings → `create_index(col, "IVF_HNSW_PQ", metric="cosine")` (or `"IVF_RQ"` for RaBitQ compression) → `scanner(nearest={"column":, "q":np_vec, "k":})` → `pa.Table`, with `prefilter` gating a scalar predicate before the ANN stage; the FTS rail stacks `create_scalar_index(col, "INVERTED")` → `full_text_query=lance.query.BoostQuery([MatchQuery(...), PhraseQuery(...)])` for one-scan hybrid lexical+vector retrieval.
- within-lib streaming: the real-time-ingest owner opens `mem_wal_writer(shard_id)` per shard, `put`s Arrow micro-batches with durable WAL, and serves fresh reads via `ShardWriter.lsm_scanner()` / `LsmVectorSearchPlanner.plan_search(...)` before compaction folds MemTable generations into base fragments — no separate streaming store beside the versioned dataset.
- within-lib lakehouse: the lakehouse LANCE arms read `version` for the receipt, take an `int` snapshot / `str` tag on `version=` and a timestamp on `asof=`, and re-head via `restore()`; `cleanup_old_versions`/`optimize.compact_files` are the retention/compaction scheduled over the `tracked_files`/`all_files` inventory.
- `add_bases`/`initial_bases` spread physical storage across named `DatasetBasePath` roots; blob columns (`blob_field`/`read_blobs`) hold geometry/raster/asset bytes read as `BlobFile` instead of separate object-store fetches.

[LOCAL_ADMISSION]:
- Lance datasets open through the lakehouse LANCE arms and the columnar/vector/FTS scan owners; a raw `lance.dataset(...)` open outside those owners is unadmitted.

[RAIL_LAW]:
- Package: `pylance`
- Owns: versioned Lance columnar datasets, Arrow-native pushdown scan, ANN vector + scalar + BM25 full-text indices, transactional merge-insert/update/delete, schema evolution, blob column storage, multi-base physical storage, real-time MemWAL shard ingest with LSM-tier read, version checkout/tags/branches/cleanup, and object-store egress
- Accept: Arrow `Table`/`RecordBatch`/reader written to a Lance URI with explicit `mode`/`schema`; `scanner`/`to_table`/`to_batches` with predicate/projection/`nearest`/`full_text_query` pushdown; `merge_insert` upsert; `create_index`/`create_scalar_index` for retrieval; `mem_wal_writer(shard_id).put(...)` for streaming ingest read back through `LsmScanner`/`Lsm*Planner`; `version`/`checkout_version`/`tags` for time-travel; `optimize`/`cleanup_old_versions` for maintenance
- Reject: hand-rolled versioning over Parquet; a separate streaming/WAL buffer beside the dataset where `mem_wal_writer` owns durable real-time ingest; client-side post-filter after a full materialize where scan pushdown discriminates; wrapper-renames of scanner, index, or merge-insert operations lance owns; a `search_vector`/`search_text` split where one `scanner(nearest=/full_text_query=)` discriminates by parameter
