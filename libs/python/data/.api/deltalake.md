# [PY_DATA_API_DELTALAKE]

`deltalake` binds a native Rust delta-rs Delta Lake reader and writer with ACID transactions, time travel, schema evolution, and change-data-feed reads over object-store and local backends. `DeltaTable` owns table lifecycle, reads, mutation, and maintenance; `write_deltalake` owns appends and overwrites; `QueryBuilder` runs embedded DataFusion SQL over registered tables.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: table, query, and schema owners

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `DeltaTable`   | class         | managed table: reads, writes, history, maintenance                 |
|  [02]   | `QueryBuilder` | class         | DataFusion SQL runner over registered tables                       |
|  [03]   | `Schema`       | class         | Delta schema of `Field` columns; Arrow/JSON interconvert           |
|  [04]   | `Field`        | class         | typed column descriptor                                            |
|  [05]   | `DataType`     | union         | `PrimitiveType\|ArrayType\|MapType\|StructType\|VariantType` alias |
|  [06]   | `VariantType`  | class         | Delta variant logical type                                         |
|  [07]   | `Metadata`     | class         | id, name, partition columns, config, created time                  |
|  [08]   | `Transaction`  | class         | app-level transaction descriptor (app id, version)                 |

[PUBLIC_TYPE_SCOPE]: writer and commit property carriers

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :------------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `WriterProperties`         | class         | Parquet compression, dictionary, statistics tuning  |
|  [02]   | `ColumnProperties`         | class         | per-column encoding and statistics config           |
|  [03]   | `BloomFilterProperties`    | class         | per-column bloom-filter config                      |
|  [04]   | `CommitProperties`         | class         | commit metadata, retries, app transactions          |
|  [05]   | `PostCommitHookProperties` | class         | post-commit checkpoint and log-cleanup control      |
|  [06]   | `TableFeatures`            | enum          | reader/writer protocol vocabulary for `add_feature` |

[PUBLIC_TYPE_SCOPE]: accessor builders (returned by `merge`/`optimize`/`alter`, outside `__all__`)

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [CAPABILITY]                             |
| :-----: | :--------------- | :------------ | :--------------------------------------- |
|  [01]   | `TableMerger`    | class         | merge-clause chain, terminal `execute()` |
|  [02]   | `TableOptimizer` | class         | `compact`/`z_order` file maintenance     |
|  [03]   | `TableAlterer`   | class         | additive schema and metadata evolution   |

[PUBLIC_TYPE_SCOPE]: typed failure result (`deltalake.exceptions`)

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                  |
| :-----: | :-------------------- | :------------ | :---------------------------- |
|  [01]   | `DeltaError`          | class         | base native error             |
|  [02]   | `DeltaProtocolError`  | class         | protocol invariant violation  |
|  [03]   | `TableNotFoundError`  | class         | no Delta log at the URI       |
|  [04]   | `CommitFailedError`   | class         | commit conflict after retries |
|  [05]   | `SchemaMismatchError` | class         | write schema drift            |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: table construction, lifecycle, and time travel
- `create`/`restore` carry: `commit_properties`, `post_commithook_properties`

| [INDEX] | [SURFACE]                                                       | [SHAPE]  | [CAPABILITY]                                       |
| :-----: | :-------------------------------------------------------------- | :------- | :------------------------------------------------- |
|  [01]   | `DeltaTable(table_uri, version, storage_options, ...)`          | ctor     | open a table at a URI, optionally pinned           |
|  [02]   | `DeltaTable.is_deltatable(table_uri, storage_options)`          | static   | probe a URI for a Delta log                        |
|  [03]   | `DeltaTable.create(table_uri, schema, mode, partition_by, ...)` | factory  | create a table from a schema                       |
|  [04]   | `DeltaTable.table_uri`                                          | property | the table root URI                                 |
|  [05]   | `DeltaTable.version()`                                          | instance | current loaded version                             |
|  [06]   | `DeltaTable.update_incremental()`                               | instance | advance to the latest log state                    |
|  [07]   | `DeltaTable.load_as_version(version)`                           | instance | time-travel load in place (int, ISO str, datetime) |
|  [08]   | `DeltaTable.restore(target, *, ignore_missing_files, ...)`      | instance | commit a revert to a version or timestamp          |

[ENTRYPOINT_SCOPE]: write and convert
- carry: `commit_properties`, `post_commithook_properties`
- `write_deltalake(table_or_uri, data, *, partition_by=None, mode='error', name=None, description=None, configuration=None, schema_mode=None, storage_options=None, predicate=None, target_file_size=None, writer_properties=None, commit_properties=None, post_commithook_properties=None) -> None` — `mode` is `Literal['error','append','overwrite','ignore']` and `schema_mode` is `Literal['merge','overwrite'] | None`; NO `engine` parameter exists, so an engine selector copied off an older call site raises `TypeError`, and the `None` return sends every write metric through `history(1)[0]["operationMetrics"]`

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :---------------------------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `write_deltalake(table_or_uri, data, *, mode, schema_mode, ...)`  | function | append or overwrite from any Arrow-exportable |
|  [02]   | `convert_to_deltalake(uri, mode, partition_by, ...)`              | function | wrap existing Parquet as a Delta table        |
|  [03]   | `DeltaTable.create_write_transaction(actions, mode, schema, ...)` | instance | commit raw `AddAction` entries                |

[ENTRYPOINT_SCOPE]: read egress

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :-------------------------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `to_pyarrow_table(partitions, columns, filesystem, filters)`          | instance | eager `pyarrow.Table`                       |
|  [02]   | `to_pandas(partitions, columns, filesystem, filters, types_mapper)`   | instance | eager `pandas.DataFrame`                    |
|  [03]   | `to_pyarrow_dataset(partitions, filesystem, schema, as_large_types)`  | instance | lazy pushdown `pyarrow.dataset.Dataset`     |
|  [04]   | `get_add_actions(flatten)`                                            | instance | add-file actions as `arro3.core.Table`      |
|  [05]   | `load_cdf(starting_version, ending_version, columns, predicate, ...)` | instance | change-data-feed `RecordBatchReader`        |
|  [06]   | `deletion_vectors()`                                                  | instance | deletion-vector rows as `RecordBatchReader` |

[ENTRYPOINT_SCOPE]: ACID mutation
- carry: `commit_properties`, `post_commithook_properties`

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]              |
| :-----: | :---------------------------------------------------- | :------- | :------------------------ |
|  [01]   | `merge(source, predicate, ...) -> TableMerger`        | instance | open a merge-clause chain |
|  [02]   | `delete(predicate, writer_properties, ...) -> dict`   | instance | delete rows, metrics dict |
|  [03]   | `update(updates, new_values, predicate, ...) -> dict` | instance | update rows, metrics dict |

[ENTRYPOINT_SCOPE]: merge clauses (`TableMerger`, chain then `execute`)
- every clause takes `predicate=None` (the `*_all` pair adding `except_cols=None`) and returns the same `TableMerger`, so the chain stays fluent and `execute` alone commits; no `delete_unmatched` parameter exists anywhere in delta-rs, so an unmatched-target delete rides `when_not_matched_by_source_delete`

| [INDEX] | [SURFACE]                                               | [CAPABILITY]                      |
| :-----: | :------------------------------------------------------ | :-------------------------------- |
|  [01]   | `when_matched_update(updates, predicate)`               | update matched target rows        |
|  [02]   | `when_matched_update_all(predicate, except_cols)`       | update all columns on match       |
|  [03]   | `when_matched_delete(predicate)`                        | delete matched target rows        |
|  [04]   | `when_not_matched_insert(updates, predicate)`           | insert unmatched source rows      |
|  [05]   | `when_not_matched_insert_all(predicate, except_cols)`   | insert all columns when unmatched |
|  [06]   | `when_not_matched_by_source_update(updates, predicate)` | update rows absent from source    |
|  [07]   | `when_not_matched_by_source_delete(predicate)`          | delete rows absent from source    |
|  [08]   | `execute() -> dict`                                     | apply the chain, metrics dict     |

[ENTRYPOINT_SCOPE]: maintenance
- carry: `commit_properties`, `post_commithook_properties`

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :------------------------------------------------------------ | :------- | :--------------------------------- |
|  [01]   | `optimize -> TableOptimizer`                                  | property | the file-maintenance accessor      |
|  [02]   | `TableOptimizer.compact(partition_filters, target_size, ...)` | instance | bin-pack small files, metrics dict |
|  [03]   | `TableOptimizer.z_order(columns, partition_filters, ...)`     | instance | z-order rewrite, metrics dict      |
|  [04]   | `vacuum(retention_hours, dry_run, *, full, keep_versions)`    | instance | delete expired files, `list[str]`  |
|  [05]   | `create_checkpoint()`                                         | instance | write a checkpoint                 |
|  [06]   | `compact_logs(starting_version, ending_version)`              | instance | compact the commit log             |
|  [07]   | `cleanup_metadata()`                                          | instance | expire old log entries             |
|  [08]   | `repair(dry_run, ...) -> dict`                                | instance | drop dangling files, metrics dict  |
|  [09]   | `generate()`                                                  | instance | emit the symlink manifest          |

[ENTRYPOINT_SCOPE]: inspect

| [INDEX] | [SURFACE]                        | [CAPABILITY]                          |
| :-----: | :------------------------------- | :------------------------------------ |
|  [01]   | `schema() -> Schema`             | Delta schema                          |
|  [02]   | `metadata() -> Metadata`         | table metadata                        |
|  [03]   | `protocol() -> ProtocolVersions` | reader/writer protocol versions       |
|  [04]   | `history(limit) -> list[dict]`   | commit history                        |
|  [05]   | `file_uris(partition_filters)`   | active data-file URIs                 |
|  [06]   | `partitions(partition_filters)`  | active partition values               |
|  [07]   | `count() -> int`                 | total row count                       |
|  [08]   | `transaction_version(app_id)`    | last committed app version            |
|  [09]   | `table_config`                   | resolved table configuration property |

[ENTRYPOINT_SCOPE]: schema evolution (`TableAlterer`, additive only)
- carry: `commit_properties`, `post_commithook_properties`

| [INDEX] | [SURFACE]                                                | [CAPABILITY]                       |
| :-----: | :------------------------------------------------------- | :--------------------------------- |
|  [01]   | `alter -> TableAlterer`                                  | the evolution accessor             |
|  [02]   | `add_columns(fields)`                                    | append columns                     |
|  [03]   | `add_constraint(constraints)`                            | add CHECK or invariant constraints |
|  [04]   | `drop_constraint(name, raise_if_not_exists)`             | drop a constraint                  |
|  [05]   | `add_feature(feature, allow_protocol_versions_increase)` | enable a `TableFeatures` upgrade   |
|  [06]   | `set_table_properties(properties, raise_if_not_exists)`  | set table configuration            |
|  [07]   | `set_table_name(name)`                                   | set the table name                 |
|  [08]   | `set_table_description(description)`                     | set the table description          |
|  [09]   | `set_column_metadata(column, metadata)`                  | set per-column metadata            |

[ENTRYPOINT_SCOPE]: DataFusion query, schema interconvert, and process

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :----------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `QueryBuilder.register(table_name, delta_table)` | instance | register a table for SQL, returns self   |
|  [02]   | `QueryBuilder.execute(sql) -> RecordBatchReader` | instance | run DataFusion SQL, drain `.read_all()`  |
|  [03]   | `Schema.to_arrow(*, as_large_types)`             | instance | Delta schema to `pyarrow.Schema`         |
|  [04]   | `Schema.from_arrow(data_type) -> Schema`         | static   | build a Delta schema from Arrow          |
|  [05]   | `Schema.to_json()` / `Schema.json()`             | instance | schema as JSON text                      |
|  [06]   | `Schema.from_json(schema_json) -> Schema`        | static   | parse a Delta schema from JSON           |
|  [07]   | `Field(name, type, *, nullable, metadata)`       | ctor     | typed column; accepts a type string      |
|  [08]   | `init_tracing(endpoint)`                         | function | register the delta-rs OTel exporter once |
|  [09]   | `enable_nanosecond_timestamps()`                 | function | opt into nanosecond timestamp writes     |
|  [10]   | `rust_core_version()`                            | function | delta-rs core version string             |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `write_deltalake`/`merge` accept any Arrow C-stream-exportable (PyArrow, Polars, pandas via Arrow); `mode` discriminates `error`/`append`/`overwrite`/`ignore`, `schema_mode` selects `merge` versus `overwrite` evolution.
- Reads run lazy through `to_pyarrow_dataset`: column projection and partition/predicate filters push down before materialization, while `to_pyarrow_table`/`to_pandas` materialize eagerly.
- `load_as_version` loads a prior version in place; `restore` writes a new commit reverting the table, never mutating history.
- `merge` returns a `TableMerger`: chain the `when_*` clauses then call `execute()` once, never a per-clause commit.
- `TableMerger.execute() -> dict[str, Any]` answers `num_source_rows`, `num_target_rows_inserted`, `num_target_rows_updated`, `num_target_rows_deleted`, `num_target_rows_copied`, `num_output_rows`, `num_target_files_scanned`, `num_target_files_skipped_during_scan`, `num_target_files_added`, `num_target_files_removed`, `execution_time_ms`, `scan_time_ms`, and `rewrite_time_ms`. `num_output_rows` totals inserted, updated, and COPIED rows — the row count of the rewritten output files, never an insert tally, so 2 inserted with 2 updated and 1 copied answers 5 against 4 source rows — therefore an acceptance count reads `num_target_rows_inserted`, a duplicate count reads `num_target_rows_updated`, and those two sum exactly to `num_source_rows`.
- `when_matched_update({})` with an empty updates dict neither raises nor changes the standing row, yet still reports `num_target_rows_updated=1`, so the metrics cannot separate a no-op match from a real update; a caller needing that split narrows the clause `predicate` rather than reading the count.
- `delete`/`update`/`merge`/`compact`/`z_order` return a metrics dict keyed `num_added_files`/`num_removed_files`/`num_updated_rows`/`num_copied_rows`/`num_deleted_rows`; a predicate-less `delete` omits `num_deleted_rows`. `restore` alone answers CAMEL-cased native metrics — `numRemovedFile`/`numRestoredFile`, live-verified — because those spellings originate in the Rust layer and never reach the Python docstring; a snake spelling copied off its sibling metrics dicts resolves nowhere and reports every revert as zero files touched.
- `history(1)[0]["operationMetrics"]` carries the newest commit's OWN churn and its key roster is per-operation, live-verified: `WRITE`/`DELETE`/`UPDATE` spell `num_added_files`/`num_removed_files`, `MERGE` spells `num_target_files_added`/`num_target_files_removed` beside `num_output_rows`, `OPTIMIZE` spells camel `numFilesAdded`/`numFilesRemoved` beside a JSON-STRING `filesAdded`/`filesRemoved` summary carrying `totalSize`, `RESTORE` spells singular camel `numRestoredFile`/`numRemovedFile`, and `ADD COLUMN` answers `operationMetrics: None` outright — so a consumer keys the spelling per operation and defaults the map, never one roster across all six.
- No append, merge, or delete `operationMetrics` key carries BYTES — every one is a file count, row count, or millisecond duration — so `OPTIMIZE`'s JSON-string `filesAdded`/`filesRemoved` `totalSize` is the only commit-level byte evidence and a writer wanting written bytes measures them on its own sink.
- `get_add_actions` answers the WHOLE current snapshot's active file roster, never the commit's added files — two two-file appends answer four rows — so summing its `size_bytes` measures the table's TOTAL on-disk volume, a state fact; reading it as one commit's evidence prices every append at the whole table's size. Its `arro3.core.Table` (never a `pyarrow.Table`) carries `path`, `size_bytes`, `modification_time`, `num_records`, `null_count`, `min`, and `max`, so the clean added-byte route diffs per-file `size_bytes` across two versions.
- `is_deltatable(table_uri)` is a STATIC probe taking the uri, so an `ignore`-mode short-circuit tests existence without constructing a handle; `write_deltalake(mode="ignore")` on an existing table commits nothing, which leaves `history(1)` pointing at the PREVIOUS commit.
- `TableOptimizer.compact`/`z_order` take `partition_filters` as a `(column, operator, value)` conjunction and rewrite the WHOLE table when it is absent, so a clustering pass over a partitioned plane scopes itself or pays a full rewrite per run.
- `TableAlterer` evolves by addition and metadata alone — no column drop or rename — so a portable `Evolve` binds only the `adds` clause on Delta.
- `Field(name, type_string)` and `PrimitiveType(type_string)` accept a primitive type string directly, so a `(name, type-string)` column-add tuple constructs a `Field` with no separate type parser; `DataType` is the union alias, never a factory call.
- `init_tracing` registers a delta-rs OpenTelemetry exporter once at process start; subsequent write/merge/optimize commits emit spans at the Rust layer, never wrapped per call.

[STACKING]:
- `arro3-core`(`arro3-core.md`): `load_cdf`/`deletion_vectors`/`QueryBuilder.execute` return an `arro3.core.RecordBatchReader` and `get_add_actions` an `arro3.core.Table`, drained via `.read_all()` into the columnar interop owner with no Python-row hop. `pyarrow.compute` REFUSES an arro3 `ChunkedArray` (`TypeError: Got unexpected argument type`), so an aggregate over an add-action column crosses the PyCapsule first through `pyarrow.table(...)`, never by reaching into the arro3 column directly.
- `pyarrow`(`pyarrow.md`), `datafusion`(`datafusion.md`), `duckdb`(`duckdb.md`): `to_pyarrow_dataset()` is a pushdown-capable `pa.dataset.Dataset` a `SessionContext.register_table` or `con.register` adopts directly, so a Delta table joins Parquet and object-store sources under one engine's SQL with pruning pushed into the Delta scan, never an eager `to_pyarrow_table` copy.
- `polars`(`polars.md`): `write_deltalake`/`merge` take any Arrow-exportable, so a `LazyFrame.collect()`, a `datafusion` `DataFrame.to_arrow_table()`, or a `duckdb` `to_arrow_reader()` writes straight into a commit.
- `substrait`(`substrait.md`): `QueryBuilder` runs embedded DataFusion SQL in-process, while cross-engine plan exchange registers the Delta dataset into the `datafusion`/`duckdb-substrait` spine, so SQL stays local while plans cross engines.
- within-lib: the data folder's tabular lakehouse domain owns Delta as its ACID table format; `DeltaTable` is the sole lifecycle owner and `write_deltalake` the single write entry, and `CommitProperties.app_transactions` carries an idempotent `Transaction` app-id under a `stamina` `retry_context` keyed by `max_commit_retries`.

[LOCAL_ADMISSION]:
- open through `DeltaTable(table_uri, ...)`, write through `write_deltalake`, mutate through `merge(...).execute()` clause chains; the egress frame is a call argument, never a per-frame wrapper.
- tune through `WriterProperties` with nested `ColumnProperties`/`BloomFilterProperties`; carry commit metadata and retries through `CommitProperties`.
- federate through `to_pyarrow_dataset()` registered into a `datafusion`/`duckdb` engine; probe `schema()`/`metadata()` before a heavy read.
