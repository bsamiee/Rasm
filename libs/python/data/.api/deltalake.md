# [PY_DATA_API_DELTALAKE]

`deltalake` supplies a native Rust-backed Delta Lake table reader and writer with ACID transactions, time travel, schema evolution, and change-data-feed access over object-store and local backends. `DeltaTable` owns lifecycle, reads, and maintenance; `write_deltalake` owns appends and overwrites; `QueryBuilder` runs DataFusion SQL over registered tables.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `deltalake`
- package: `deltalake`
- import: `import deltalake`
- owner: `data`
- rail: columnar
- capability: Delta Lake table exchange, ACID append/overwrite/merge/delete/update, time travel and version loading, change-data-feed reads, file compaction and vacuum, schema evolution, and SQL query over Arrow egress

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- `deltalake.DeltaTable` — managed Delta table; constructed from a `table_uri`; owns reads, writes, history, maintenance, and protocol; egress via `to_pyarrow_table`, `to_pandas`, `to_pyarrow_dataset`, and `get_add_actions`.
- `deltalake.Schema` — Delta table schema; holds `Field` columns; converts to and from Arrow.
- `deltalake.Field` — typed column descriptor with name, `DataType`, nullability, and metadata.
- `deltalake.DataType` — Delta type factory for primitive, array, map, and struct types.
- `deltalake.Metadata` — table metadata: id, name, description, partition columns, configuration, and created time.
- `deltalake.Transaction` — application-level transaction descriptor with app id, version, and last-updated fields.
- `deltalake.QueryBuilder` — DataFusion SQL runner; `register(table_name, delta_table)` then `execute(sql)` returning Arrow batches.
- `deltalake.WriterProperties` — Parquet writer tuning: compression, dictionary, statistics, bloom-filter, and column properties.
- `deltalake.ColumnProperties`, `deltalake.BloomFilterProperties` — per-column writer property carriers.
- `deltalake.CommitProperties` — commit-time metadata: app transactions, custom metadata, and max commit retries.
- `deltalake.PostCommitHookProperties` — post-commit hook control for checkpoint creation and cleanup.
- `deltalake.TableFeatures` — reader/writer table feature flags for protocol upgrades.
- `deltalake.VariantType` — Delta variant logical type.

[ENTRYPOINTS]:
- table open: `DeltaTable(table_uri, version=None, storage_options=None, without_files=False, log_buffer_size=None, skip_stats=False)`, `DeltaTable.is_deltatable(table_uri, storage_options=None) -> bool`.
- table create: `DeltaTable.create(table_uri, schema, mode='error', partition_by=None, name=None, description=None, configuration=None, storage_options=None, *, commit_properties=None, post_commithook_properties=None, raise_if_key_not_exists=True) -> DeltaTable`.
- write: `write_deltalake(table_or_uri, data, *, partition_by=None, mode='error', name=None, description=None, configuration=None, schema_mode=None, storage_options=None, predicate=None, target_file_size=None, writer_properties=None, commit_properties=None, post_commithook_properties=None) -> None`.
- convert: `convert_to_deltalake(uri, mode='error', partition_by=None, partition_strategy=None, name=None, description=None, configuration=None, storage_options=None, *, commit_properties=None, post_commithook_properties=None) -> None`.
- read egress: `DeltaTable.to_pyarrow_table(partitions=None, columns=None, filesystem=None, filters=None) -> pyarrow.Table`, `.to_pandas(...) -> pd.DataFrame`, `.to_pyarrow_dataset(partitions=None, filesystem=None, parquet_read_options=None, schema=None, as_large_types=False) -> pyarrow.dataset.Dataset`, `.get_add_actions(flatten=False) -> pyarrow.Table`, `.load_cdf(...)`.
- mutate: `DeltaTable.merge(source, predicate, source_alias=None, target_alias=None, merge_schema=False, ...) -> TableMerger`, `.delete(predicate=None, ...) -> dict`, `.update(updates=None, new_values=None, predicate=None, ...) -> dict`.
- time travel: `DeltaTable.load_as_version(version)`, `.load_cdf(starting_version=0, ending_version=None, ...)`, `.restore(target, *, ignore_missing_files=False, protocol_downgrade_allowed=False, ...) -> dict`, `.version() -> int`, `.update_incremental()`.
- maintenance: `DeltaTable.optimize` (compact/z-order accessor), `.vacuum(retention_hours=None, dry_run=True, enforce_retention_duration=True, ...) -> list[str]`, `.create_checkpoint()`, `.cleanup_metadata()`, `.repair(dry_run=False, ...) -> dict`, `.generate(...)`.
- inspect: `DeltaTable.schema() -> Schema`, `.metadata() -> Metadata`, `.protocol()`, `.history(limit=None) -> list[dict]`, `.file_uris(partition_filters=None) -> list[str]`, `.files(...)`, `.partitions(...)`, `.table_config()`, `.count() -> int`, `.transaction_version(app_id)`, `.deletion_vectors()`.
- evolve and query: `DeltaTable.alter` (schema/property accessor), `QueryBuilder().register(name, table).execute(sql)`.

[EXCEPTIONS]:
- `deltalake.exceptions.DeltaError` — base native error; `DeltaProtocolError`, `TableNotFoundError`, `CommitFailedError`, `SchemaMismatchError` follow from invalid protocol, missing table, commit conflict, and schema drift.

[IMPLEMENTATION_LAW]:
- `write_deltalake` accepts any Arrow C-stream-exportable object (PyArrow, Polars, pandas via Arrow); `mode` discriminates `error`/`append`/`overwrite`/`ignore` and `schema_mode` controls merge versus overwrite evolution.
- `DeltaTable.merge` returns a `TableMerger` builder; chain `when_matched_update`/`when_not_matched_insert`/`when_not_matched_by_source_delete` then `execute()`, never separate per-clause commit calls.
- Reads are lazy through `to_pyarrow_dataset`; column projection and partition/predicate filters push down before materialisation, while `to_pyarrow_table`/`to_pandas` materialise eagerly.
- Time travel loads a prior version in place via `load_as_version(version)`; `restore` writes a new commit that reverts the table rather than mutating history.
- Writer tuning flows through `WriterProperties` with nested `ColumnProperties`/`BloomFilterProperties`; commit metadata and retries flow through `CommitProperties`.
- `QueryBuilder` runs DataFusion SQL over registered Delta tables and returns Arrow record batches; it is the SQL surface, distinct from the imperative `DeltaTable` builder.

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `deltalake`
- Owns: Delta Lake table read/write, ACID merge/delete/update, time travel and CDF, compaction and vacuum, schema evolution, and DataFusion SQL egress
- Accept: `DeltaTable(table_uri, ...)` as the lifecycle owner, `write_deltalake` for appends/overwrites, `merge(...).execute()` builder chains, Arrow-exportable inputs, `WriterProperties`/`CommitProperties` for tuning
- Reject: per-clause merge commits, eager full-table reads when predicate/column pushdown applies, hand-rolled Parquet log parsing, and duplicate per-frame write entry points outside `write_deltalake`
