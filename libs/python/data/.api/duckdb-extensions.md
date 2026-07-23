# [PY_DATA_API_DUCKDB_EXTENSIONS]

DuckDB loadable extensions install in-engine into a live `DuckDBPyConnection` through `install_extension`/`load_extension` or SQL `INSTALL`/`LOAD`, never as a pip row, Python module, or dependency entry. A loaded extension's capability rides the DuckDB SQL and bound-connection surface, and downstream owners compose that session rather than a per-extension Python package.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: DuckDB in-engine extensions
- package: none
- module: `duckdb`
- owner: `data`
- rail: query extensions

## [02]-[LOAD_ROWS]

[LOAD_ENTRY_SCOPE]: extension install and load

Each extension loads through `install_extension(name, *, repository)` / `load_extension(name)` or SQL `INSTALL <ext> [FROM community]; LOAD <ext>;`; the `[REPOSITORY]` row property selects the source, never a call-site branch.

| [INDEX] | [EXTENSION] | [REPOSITORY] | [CONSUMER]                         |
| :-----: | :---------- | :----------- | :--------------------------------- |
|  [01]   | `httpfs`    | core         | remote `read_parquet` object scans |
|  [02]   | `spatial`   | core         | `spatial/query` geometry SQL       |
|  [03]   | `h3`        | community    | H3 bin and neighborhood SQL        |
|  [04]   | `iceberg`   | core         | Iceberg metadata and table reads   |
|  [05]   | `substrait` | community    | SQL to Substrait plan bridge       |
|  [06]   | `ducklake`  | core         | DuckLake table-format catalog      |

## [03]-[SUBSTRAIT]

[SUBSTRAIT_ENTRY_SCOPE]: Substrait plan serialization and execution

`substrait` adds SQL table functions only — no connection-bound Python method, no extension-owned class — each reached through `con.execute`. Both serializers gate optimize-before-serialize on the `enable_optimizer` named argument; both executors take the foreign plan payload. Binary plans are Substrait protobuf; JSON plans are the inspectable twin of the same logical plan.

| [INDEX] | [SURFACE]             | [SHAPE]                            | [CAPABILITY]                         |
| :-----: | :-------------------- | :--------------------------------- | :----------------------------------- |
|  [01]   | `get_substrait`       | `CALL get_substrait('<sql>')`      | serialize SQL to binary plan (BLOB)  |
|  [02]   | `get_substrait_json`  | `CALL get_substrait_json('<sql>')` | serialize SQL to JSON plan (VARCHAR) |
|  [03]   | `from_substrait`      | `CALL from_substrait(<blob>)`      | execute binary Substrait plan        |
|  [04]   | `from_substrait_json` | `CALL from_substrait_json(<json>)` | execute JSON Substrait plan          |

## [04]-[DUCKLAKE]

[DUCKLAKE_ENTRY_SCOPE]: DuckLake attach, snapshots, change feed, and maintenance

DuckLake is a DuckDB core extension attaching a table-format catalog backed by Parquet data files under DuckDB, SQLite, or PostgreSQL metadata, reached from Python through DuckDB SQL and cursors. Every mount is `ATTACH 'ducklake:<backend>:<dsn>' AS <name> (<clause>...)` where `<backend>` is empty (DuckDB), `sqlite:`, or `postgres:`. Metadata records snapshot id, snapshot time, schema version, commit author/message/extra, data-file path, row count, byte size, and change type.

[ATTACH_CLAUSES]: `DATA_PATH` `METADATA_SCHEMA` `ENCRYPTED` `DATA_INLINING_ROW_LIMIT` `READ_ONLY` `AUTOMATIC_MIGRATION` `CREATE_IF_NOT_EXISTS` `OVERRIDE_DATA_PATH`

| [INDEX] | [SURFACE]                  | [SHAPE]                                                              | [CAPABILITY]                |
| :-----: | :------------------------- | :------------------------------------------------------------------- | :-------------------------- |
|  [01]   | attach DuckDB metadata     | `ATTACH 'ducklake:<file>.ducklake' AS <n> (DATA_PATH '<dir>/')`      | mount DuckLake catalog      |
|  [02]   | attach SQLite metadata     | `ATTACH 'ducklake:sqlite:<file>' AS <n> (DATA_PATH '<dir>/')`        | mount SQLite catalog        |
|  [03]   | attach Postgres metadata   | `ATTACH 'ducklake:postgres:<dsn>' AS <n> (DATA_PATH '<uri>')`        | mount Postgres catalog      |
|  [04]   | secret attach              | `CREATE SECRET (TYPE ducklake, ...)` + `ATTACH 'ducklake:<secret>'`  | secret-backed mount         |
|  [05]   | snapshots                  | `<cat>.snapshots()` / `ducklake_snapshots('<cat>')`                  | list snapshot history       |
|  [06]   | current and last committed | `<cat>.current_snapshot()` / `ducklake_last_committed_snapshot(...)` | snapshot identity           |
|  [07]   | time travel                | `FROM <table> AT (VERSION => n)` / `AT (TIMESTAMP => ts)`            | historical table read       |
|  [08]   | change feed                | `table_changes`, `table_insertions`, `table_deletions`               | row-level lineage           |
|  [09]   | commit metadata            | `set_commit_message`                                                 | commit author/message/extra |
|  [10]   | data-file registration     | `ducklake_add_data_files`                                            | register existing Parquet   |
|  [11]   | scans and file listing     | `ducklake_scan` / `ducklake_list_files`                              | scan or list physical files |
|  [12]   | expire snapshots           | `ducklake_expire_snapshots`                                          | drop old snapshot history   |
|  [13]   | merge adjacent files       | `ducklake_merge_adjacent_files`                                      | compact adjacent data files |
|  [14]   | rewrite data files         | `ducklake_rewrite_data_files`                                        | rewrite by delete ratio     |
|  [15]   | cleanup old files          | `ducklake_cleanup_old_files`                                         | clean superseded files      |
|  [16]   | delete orphaned files      | `ducklake_delete_orphaned_files`                                     | delete orphaned Parquet     |
|  [17]   | flush inlined data         | `ducklake_flush_inlined_data`                                        | flush inlined data rows     |

[OPTION_SURFACE]: `set_option` `ducklake_options` `ducklake_settings` `ducklake_table_info` inspect and mutate catalog policy.

## [05]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Extensions load in-engine on the owning connection; the `core`/`community` repository is a row property, never a call-site branch.
- A loaded extension's SQL functions and bound methods belong to the DuckDB session, so downstream owners compose the attached SQL or bound connection.
- DuckLake object-store credentials route through DuckDB secrets; Python never opens the Parquet data files or metadata catalog directly.
- Substrait crosses as the shared protobuf `Plan` wire artifact, emitted and consumed by the extension, never a private codec.

[STACKING]:
- `datafusion`(`.api/datafusion.md`): the `CALL get_substrait` BLOB is the same wire `Plan` `datafusion.substrait.Consumer` ingests, and `Producer` output feeds `CALL from_substrait`; neither side reimplements the protobuf codec.
- `substrait`(`.api/substrait.md`): `Plan.ParseFromString` validates the emitted BLOB before a peer engine executes it.
- `duckdb`(`.api/duckdb.md`): the `data` owner loads extensions on one bound connection and composes their SQL through `con.execute`/`con.sql`.

[LOCAL_ADMISSION]:
- An extension is a load row on the DuckDB session, never a pip dependency, module import, or its own `.api` catalog.

[RAIL_LAW]:
- Package: DuckDB loadable extensions
- Owns: connection-scoped load evidence and the SQL surface each loaded extension attaches
- Accept: in-engine `INSTALL`/`LOAD`, `install_extension`/`load_extension`, Substrait plan SQL table functions, DuckLake `ATTACH`, snapshot/change-feed/maintenance functions, and extension rows on `DuckDbSession`
- Reject: a per-extension pip or module row, a hand-rolled Substrait protobuf codec, a manual DuckLake snapshot ledger, and bespoke Parquet metadata mutation outside DuckLake
