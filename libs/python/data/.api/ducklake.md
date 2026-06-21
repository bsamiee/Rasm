# [PY_DATA_API_DUCKLAKE]

`ducklake` is the DuckDB core extension that implements the DuckLake lakehouse table format: a SQL-database catalog (DuckDB, SQLite, or PostgreSQL) holding all schema, snapshot, and statistics metadata, paired with Parquet data files in object storage. It is loaded inside a DuckDB connection and surfaced through the `ATTACH 'ducklake:...'` boundary; once attached, a DuckLake behaves as a regular DuckDB catalog supporting transactions, time travel, schema evolution, and a row-level change feed. The data owner composes the `ducklake:` attach string, the catalog-scoped helper functions (`snapshots`, `table_changes`, `set_option`), and the global maintenance procedures (`ducklake_expire_snapshots`, `ducklake_merge_adjacent_files`, `ducklake_cleanup_old_files`) into `LAKEHOUSE_DUCKLAKE_FORMAT`; it drives every call through `duckdb` cursors and never hand-rolls a manifest, snapshot table, or Parquet writer the extension already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ducklake`
- package: `ducklake` (DuckDB core extension; not a pip distribution)
- import: loaded inside DuckDB via `INSTALL ducklake; LOAD ducklake;` — driven from Python through `duckdb` cursors, never imported as a module
- owner: `data`
- rail: lakehouse
- installed: `1.0` docs-derived (duckdb-extension (no pip)); live reflection pending env provisioning
- entry points: SQL surface only — `ATTACH 'ducklake:...'` plus the `ducklake_*` global functions/procedures and catalog-scoped (`<catalog>.<fn>`) helpers; no Python entry points, no console script
- capability: DuckLake catalog/table format over a DuckDB/SQLite/PostgreSQL metadata catalog and Parquet data files — ACID multi-table transactions, snapshot-based time travel (`AT (VERSION => n)` / `AT (TIMESTAMP => ts)`), schema evolution, partitioning, encryption, data inlining, row-level change feed (insert/update/delete lineage), and maintenance (snapshot expiry, file merge, orphan/old-file cleanup)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: catalog and snapshot roots
- rail: lakehouse

The DuckLake catalog is an attached DuckDB database alias; its state is modeled by the metadata schema (`ducklake_snapshot`, `ducklake_table`, `ducklake_data_file`, ...) and surfaced to queries through table functions. The catalog requires DuckDB v1.5.2+ and a metadata backend (`duckdb` file, `sqlite:`, or `postgres:`); the format version negotiated at attach time is validated against the extension and migrated when `AUTOMATIC_MIGRATION` is enabled.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]     | [RAIL]                                                                        |
| :-----: | :------------------- | :---------------- | :---------------------------------------------------------------------------- |
|  [01]   | DuckLake catalog     | attached database | the `ATTACH 'ducklake:...' AS <name>` alias acting as a normal DuckDB catalog |
|  [02]   | `ducklake_snapshot`  | metadata table    | the row of valid snapshots (`snapshot_id`, `snapshot_time`, `schema_version`) |
|  [03]   | `ducklake_table`     | metadata table    | catalog table identity (`table_id`, `begin_snapshot`, `schema_id`, name)      |
|  [04]   | `ducklake_data_file` | metadata table    | Parquet data-file registry (path, row count, size, partition values)          |
|  [05]   | `ducklake` secret    | DuckDB secret     | named `TYPE ducklake` secret carrying `METADATA_PATH`/`DATA_PATH` for attach  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: attach and catalog secret
- rail: lakehouse

`ATTACH` is the single boundary that mounts a DuckLake; the `ducklake:` prefix selects the metadata backend (`<file>.ducklake`/`<file>.duckdb` for DuckDB, `sqlite:<path>`, `postgres:<conn>`, or a secret name). Options ride in the trailing `(...)` clause; `META_*`-prefixed keys pass through to the metadata backend. `DATA_PATH` is persisted in the catalog and is not re-specified on later attaches.

| [INDEX] | [SURFACE]              | [CALL_SHAPE]                                                                                                                                                                                                                                      | [CAPABILITY]                                                                              |
| :-----: | :--------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | :---------------------------------------------------------------------------------------- |
|  [01]   | install/load           | `INSTALL ducklake; LOAD ducklake;`                                                                                                                                                                                                                | install the core extension (auto-loads on first ATTACH)                                   |
|  [02]   | attach (DuckDB meta)   | `ATTACH 'ducklake:metadata.ducklake' AS <name> (DATA_PATH 'data_files/')`                                                                                                                                                                         | mount a DuckLake on a DuckDB metadata file                                                |
|  [03]   | attach (SQLite meta)   | `ATTACH 'ducklake:sqlite:metadata.sqlite' AS <name> (DATA_PATH 'data/')`                                                                                                                                                                          | mount a DuckLake on a SQLite metadata catalog                                             |
|  [04]   | attach (Postgres meta) | `ATTACH 'ducklake:postgres:dbname=postgres' AS <name> (DATA_PATH 's3://bucket/data/')`                                                                                                                                                            | mount a DuckLake on a PostgreSQL metadata catalog                                         |
|  [05]   | attach (options)       | `ATTACH 'ducklake:...' AS <name> (DATA_PATH ..., METADATA_SCHEMA 'main', ENCRYPTED, DATA_INLINING_ROW_LIMIT 0, READ_ONLY, AUTOMATIC_MIGRATION false, CREATE_IF_NOT_EXISTS true, OVERRIDE_DATA_PATH true, SNAPSHOT_VERSION n, SNAPSHOT_TIME 'ts')` | full attach policy row (path, schema, encryption, inlining, mode, migration, time travel) |
|  [06]   | attach (secret)        | `ATTACH 'ducklake:<secret_name>' AS <name>`                                                                                                                                                                                                       | mount via a named `ducklake` secret                                                       |
|  [07]   | create secret          | `CREATE SECRET (TYPE ducklake, METADATA_PATH 'metadata.duckdb', DATA_PATH 'data_files/')`                                                                                                                                                         | persist metadata+data path config as a reusable secret                                    |

[ENTRYPOINT_SCOPE]: snapshots, time travel, and change feed
- rail: lakehouse

Snapshot inspection, time travel, and the change feed are read surfaces. Each function exists as a catalog-scoped helper (`<catalog>.<fn>(...)`) and a global form (`ducklake_<fn>('<catalog>', ...)`); the global form takes the catalog name as the first argument. Change-feed bounds are inclusive and accept either a `BIGINT` snapshot id or a `TIMESTAMP`.

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                                                              | [CAPABILITY]                                                                       |
| :-----: | :---------------------- | :------------------------------------------------------------------------ | :--------------------------------------------------------------------------------- |
|  [01]   | `snapshots`             | `FROM <catalog>.snapshots()` / `ducklake_snapshots('<catalog>')`          | list valid snapshots (`snapshot_id`, `snapshot_time`, `schema_version`, `changes`) |
|  [02]   | `current_snapshot`      | `FROM <catalog>.current_snapshot()`                                       | the latest committed snapshot for the catalog                                      |
|  [03]   | time travel (version)   | `FROM <table> AT (VERSION => 3)`                                          | query a table at a snapshot version                                                |
|  [04]   | time travel (timestamp) | `FROM <table> AT (TIMESTAMP => now() - INTERVAL '1 week')`                | query a table as of a point in time                                                |
|  [05]   | `table_changes`         | `FROM <catalog>.table_changes('<table>', <start>, <end>)`                 | full change feed: `snapshot_id`, `rowid`, `change_type`, plus columns              |
|  [06]   | `table_insertions`      | `FROM <catalog>.table_insertions('<table>', <start>, <end>)`              | inserted rows only: `snapshot_id`, `rowid`, plus columns                           |
|  [07]   | `table_deletions`       | `FROM <catalog>.table_deletions('<table>', <start>, <end>)`               | deleted rows only: `snapshot_id`, `rowid`, plus columns                            |
|  [08]   | `set_commit_message`    | `CALL <catalog>.set_commit_message(author, message, extra_info => '...')` | attach author/message/extra metadata to the current transaction commit             |

[ENTRYPOINT_SCOPE]: maintenance and options
- rail: lakehouse

Maintenance procedures are invoked with `CALL`; each returns a single-column `Success` table. `ducklake_expire_snapshots` retires snapshots but does not delete files — `ducklake_cleanup_old_files` and `ducklake_delete_orphaned_files` reclaim storage. Compaction options (`auto_compact`, `target_file_size`, `expire_older_than`) are set through `set_option`.

| [INDEX] | [SURFACE]                        | [CALL_SHAPE]                                                                                                                                                   | [CAPABILITY]                                                                             |
| :-----: | :------------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `ducklake_expire_snapshots`      | `CALL ducklake_expire_snapshots('<catalog>', versions => [2], older_than => <ts>, dry_run => false)`                                                           | retire snapshots by id list or age (file deletion deferred)                              |
|  [02]   | `ducklake_merge_adjacent_files`  | `CALL ducklake_merge_adjacent_files('<catalog>', '<table>', schema => '<schema>', max_compacted_files => 10, min_file_size => 10240, max_file_size => 102400)` | compact small/adjacent data files into target-sized files                                |
|  [03]   | `ducklake_cleanup_old_files`     | `CALL ducklake_cleanup_old_files('<catalog>', cleanup_all => true, dry_run => false, older_than => <ts>)`                                                      | delete data files scheduled for deletion                                                 |
|  [04]   | `ducklake_delete_orphaned_files` | `CALL ducklake_delete_orphaned_files('<catalog>', dry_run => false, older_than => <ts>)`                                                                       | delete files in the data path not referenced by metadata                                 |
|  [05]   | `ducklake_flush_inlined_data`    | `CALL ducklake_flush_inlined_data('<catalog>')`                                                                                                                | flush inlined catalog rows out to Parquet data files                                     |
|  [06]   | `set_option`                     | `CALL <catalog>.set_option('<key>', '<value>', table_name => '<table>')` / `ducklake_set_option('<catalog>', 'target_file_size', '5MB')`                       | set catalog/table options (`auto_compact`, `target_file_size`, `expire_older_than`, ...) |
|  [07]   | `ducklake_table_info`            | `FROM ducklake_table_info('<catalog>')`                                                                                                                        | per-table file counts and sizes                                                          |

## [04]-[IMPLEMENTATION_LAW]

[LAKEHOUSE_DUCKLAKE]:
- load: `INSTALL ducklake; LOAD ducklake;` executed once per DuckDB connection at boundary scope; the extension auto-loads on the first `ducklake:` ATTACH, so the explicit `LOAD` is the deterministic provisioning path, not a parallel install rail.
- attach axis: one `ATTACH 'ducklake:...'` owns mounting; backend selection (`<file>.ducklake` / `sqlite:` / `postgres:` / secret) and policy (`DATA_PATH`, `METADATA_SCHEMA`, `ENCRYPTED`, `DATA_INLINING_ROW_LIMIT`, `READ_ONLY`, `AUTOMATIC_MIGRATION`, `CREATE_IF_NOT_EXISTS`, `OVERRIDE_DATA_PATH`) are clause rows, never a per-backend connector type — `DATA_PATH` is persisted and re-attaches read it from metadata.
- catalog axis: the attached alias is a normal DuckDB catalog; tables, schemas, transactions, and DDL flow through standard SQL, and the `ducklake_*` metadata tables are the canonical state, never a hand-rolled manifest or snapshot ledger.
- time-travel axis: `AT (VERSION => n)` and `AT (TIMESTAMP => ts)` are the query rows for historical reads; `SNAPSHOT_VERSION`/`SNAPSHOT_TIME` ATTACH options pin a whole-catalog snapshot — both read the same `ducklake_snapshot` history, never a copied table.
- change-feed axis: `table_changes` is the single lineage surface (`change_type` in `insert`/`update_preimage`/`update_postimage`/`delete`); `table_insertions`/`table_deletions` are filtered projections of the same feed, never a separately maintained audit table; bounds accept snapshot id or timestamp.
- maintenance axis: `ducklake_expire_snapshots` retires history, `ducklake_merge_adjacent_files` compacts, and `ducklake_cleanup_old_files`/`ducklake_delete_orphaned_files` reclaim storage — expiry and physical deletion are distinct CALL rows; compaction policy lives in `set_option` rows (`auto_compact`, `target_file_size`, `expire_older_than`), never a bespoke vacuum loop.
- evidence: each operation records snapshot id, snapshot time, schema version, commit author/message, data-file path, row count, byte size, and change type as a lakehouse receipt drawn from the metadata catalog.
- boundary: ducklake owns the catalog/table format, snapshot lineage, Parquet data-file lifecycle, and metadata persistence (DuckDB/SQLite/PostgreSQL); all access routes through `duckdb` cursors against the attached catalog; object-store credentials route through DuckDB secrets, and the metadata-backend connection routes through `META_*` ATTACH keys — Python never opens the Parquet files or metadata catalog directly.

[RAIL_LAW]:
- Package: `ducklake`
- Owns: DuckLake catalog/table format — ACID multi-table transactions, snapshot time travel, schema evolution, partitioning, encryption, data inlining, row-level change feed, and storage maintenance over a DuckDB/SQLite/PostgreSQL metadata catalog plus Parquet data files
- Accept: DuckLake lakehouse persistence driven through `duckdb` cursors and the `ducklake:` ATTACH boundary, feeding the data owner's `LAKEHOUSE_DUCKLAKE_FORMAT`
- Reject: a hand-rolled snapshot/manifest table or Parquet writer the extension owns; a per-backend connector type when ATTACH options discriminate; a separate audit table duplicating the change feed; a bespoke vacuum loop replacing the maintenance CALL rows; treating `ducklake` as an importable Python module rather than a SQL-surface extension
