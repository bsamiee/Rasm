# [PY_DATA_API_DUCKDB_EXTENSIONS]

`duckdb-extensions` is the evidence home for DuckDB loadable extensions. These surfaces load into a `DuckDBPyConnection` or SQL session through `install_extension`/`load_extension` or `INSTALL`/`LOAD`; they are not pip packages, Python modules, or separate dependency rows.

## [01]-[EXTENSION_SURFACE]

[PACKAGE_SURFACE]: DuckDB in-engine extensions
- package: none
- import: `import duckdb` only
- owner: `data`
- rail: query extensions
- host package: `duckdb`
- load boundary: `DuckDBPyConnection.install_extension(...)`, `DuckDBPyConnection.load_extension(...)`, `INSTALL <extension>`, and `LOAD <extension>`
- capability: connection-scoped extension loading for remote filesystem scans, spatial SQL, H3 SQL, Iceberg reads, DuckLake table-format persistence, and Substrait plan interchange

## [02]-[LOAD_ROWS]

[ENTRYPOINT_SCOPE]: extension install/load
- rail: query extensions

| [INDEX] | [EXTENSION] | [REPOSITORY] | [LOAD_SHAPE]                                                 | [CONSUMER]                                   |
| :-----: | :---------- | :----------- | :----------------------------------------------------------- | :------------------------------------------- |
|  [01]   | `httpfs`    | core         | `INSTALL httpfs; LOAD httpfs;`                               | remote `read_parquet` and object URL scans   |
|  [02]   | `spatial`   | core         | `INSTALL spatial; LOAD spatial;`                             | `spatial/query` geometry SQL                 |
|  [03]   | `h3`        | community    | `INSTALL h3 FROM community; LOAD h3;`                        | H3 SQL bins and neighborhood functions       |
|  [04]   | `iceberg`   | core         | `INSTALL iceberg; LOAD iceberg;`                             | Iceberg metadata and table reads             |
|  [05]   | `substrait` | community    | `install_extension("substrait", repository="community")`; `load_extension("substrait")` | DuckDB/Substrait plan bridge |
|  [06]   | `ducklake`  | core         | `INSTALL ducklake; LOAD ducklake;`                           | DuckLake table-format catalog                |

## [03]-[SUBSTRAIT]

[ENTRYPOINT_SCOPE]: Substrait plan serialization and execution
- rail: substrait portability

The `substrait` extension attaches methods to the host connection and table functions to the engine. It exposes no extension-owned Python class and no top-level module.

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]                                                                           | [CAPABILITY]                         |
| :-----: | :------------------------ | :------------------------------------------------------------------------------------- | :----------------------------------- |
|  [01]   | `con.get_substrait`       | `get_substrait(query: str, *, enable_optimizer: bool = True) -> DuckDBPyRelation`      | serialize SQL to binary Substrait plan |
|  [02]   | `con.get_substrait_json`  | `get_substrait_json(query: str, *, enable_optimizer: bool = True) -> DuckDBPyRelation` | serialize SQL to JSON Substrait plan |
|  [03]   | `con.from_substrait`      | `from_substrait(proto: bytes) -> DuckDBPyRelation`                                     | execute binary Substrait plan        |
|  [04]   | `con.from_substrait_json` | `from_substrait_json(json: str) -> DuckDBPyRelation`                                   | execute JSON Substrait plan          |
|  [05]   | `CALL get_substrait(...)` / `CALL get_substrait_json(...)` | SQL table functions                                            | SQL-side plan serialization          |
|  [06]   | `CALL from_substrait(...)` / `CALL from_substrait_json(...)` | SQL table functions                                           | SQL-side foreign-plan execution      |

[SUBSTRAIT_PORTABILITY]:
- load axis: the community extension is installed and loaded once per connection before any Substrait call.
- serialize axis: `get_substrait` and `get_substrait_json` produce binary or JSON plans from one SQL `SELECT` string; `enable_optimizer` controls whether DuckDB optimizes before serialization.
- consume axis: `from_substrait` and `from_substrait_json` execute foreign binary or JSON plans and return executable `DuckDBPyRelation` values.
- wire axis: binary plans are Substrait protobuf payloads; JSON plans are the inspectable twin of the same logical plan.
- stacking axis: DuckDB is the DuckDB-side peer of `datafusion.substrait`; neither side reimplements a protobuf codec.

## [04]-[DUCKLAKE]

[ENTRYPOINT_SCOPE]: DuckLake attach, snapshots, change feed, and maintenance
- rail: lakehouse

DuckLake is a DuckDB core extension. It attaches a table-format catalog backed by DuckDB, SQLite, or PostgreSQL metadata plus Parquet data files. Python reaches it only through DuckDB SQL and cursors.

| [INDEX] | [SURFACE]                         | [CALL_SHAPE]                                                                 | [CAPABILITY]                         |
| :-----: | :-------------------------------- | :--------------------------------------------------------------------------- | :----------------------------------- |
|  [01]   | install/load                      | `INSTALL ducklake; LOAD ducklake;`                                           | deterministic extension load         |
|  [02]   | attach DuckDB metadata            | `ATTACH 'ducklake:metadata.ducklake' AS <name> (DATA_PATH 'data_files/')`    | mount DuckLake catalog               |
|  [03]   | attach SQLite metadata            | `ATTACH 'ducklake:sqlite:metadata.sqlite' AS <name> (DATA_PATH 'data/')`     | mount SQLite-backed catalog          |
|  [04]   | attach PostgreSQL metadata        | `ATTACH 'ducklake:postgres:dbname=postgres' AS <name> (DATA_PATH 's3://bucket/data/')` | mount Postgres-backed catalog |
|  [05]   | attach options                    | `DATA_PATH`, `METADATA_SCHEMA`, `ENCRYPTED`, `DATA_INLINING_ROW_LIMIT`, `READ_ONLY`, `AUTOMATIC_MIGRATION`, `CREATE_IF_NOT_EXISTS`, `OVERRIDE_DATA_PATH` | catalog policy |
|  [06]   | secret attach                     | `CREATE SECRET (TYPE ducklake, METADATA_PATH ..., DATA_PATH ...)`; `ATTACH 'ducklake:<secret>' AS <name>` | secret-backed mount |
|  [07]   | snapshots                         | `<catalog>.snapshots()` / `ducklake_snapshots('<catalog>')`                  | list snapshot history                |
|  [08]   | current and last committed        | `<catalog>.current_snapshot()` / `ducklake_last_committed_snapshot('<catalog>')` | snapshot identity                 |
|  [09]   | time travel                       | `FROM <table> AT (VERSION => n)` / `AT (TIMESTAMP => ts)`                    | historical table read                |
|  [10]   | change feed                       | `table_changes`, `table_insertions`, `table_deletions`                       | row-level lineage                    |
|  [11]   | commit metadata                   | `set_commit_message`                                                         | commit author/message/extra metadata |
|  [12]   | data-file registration            | `ducklake_add_data_files`                                                    | register existing Parquet files      |
|  [13]   | scans and file listing            | `ducklake_scan` / `ducklake_list_files`                                      | scan or list physical data files     |
|  [14]   | maintenance                       | `ducklake_expire_snapshots`, `ducklake_merge_adjacent_files`, `ducklake_rewrite_data_files`, `ducklake_cleanup_old_files`, `ducklake_delete_orphaned_files`, `ducklake_flush_inlined_data` | storage lifecycle |
|  [15]   | options and settings              | `set_option`, `ducklake_options`, `ducklake_settings`, `ducklake_table_info` | policy inspection and mutation       |

[LAKEHOUSE_DUCKLAKE]:
- attach axis: one `ATTACH 'ducklake:...'` owns mounting; backend and policy vary by clause rows, never per-backend connector types.
- catalog axis: the attached alias is a normal DuckDB catalog; tables, schemas, transactions, DDL, and metadata tables are the canonical state.
- time-travel axis: table-level `AT (VERSION => n)` and `AT (TIMESTAMP => ts)` plus catalog-level `SNAPSHOT_VERSION`/`SNAPSHOT_TIME` read the same snapshot history.
- change-feed axis: `table_changes` is the lineage surface; `table_insertions` and `table_deletions` are filtered projections.
- ingestion axis: CTAS/INSERT writes new Parquet through the catalog, and `ducklake_add_data_files` registers existing Parquet without copying.
- maintenance axis: expiry, adjacent-file merge, delete-ratio rewrite, old-file cleanup, orphan cleanup, and option-setting are distinct SQL calls.
- evidence axis: operations record snapshot id, snapshot time, schema version, commit author/message/extra, data-file path, row count, byte size, and change type from the metadata catalog.

## [05]-[IMPLEMENTATION_LAW]

[EXTENSION_LAW]:
- extensions load in-engine on the owning DuckDB connection; no `pip` dependency row, import statement, or module-level Python package exists for DuckDB extensions.
- repository (`core` vs `community`) is a row property on the session extension rail, not a call-site branch.
- extension methods and SQL table functions belong to the DuckDB session once loaded; downstream owners compose the attached SQL or bound connection surface.
- object-store credentials for DuckLake route through DuckDB secrets; Python never opens the Parquet data files or metadata catalog directly.

[RAIL_LAW]:
- Package: DuckDB loadable extensions
- Owns: connection-scoped load evidence and SQL/bound-method surfaces for `httpfs`, `spatial`, `h3`, `iceberg`, `substrait`, and `ducklake`
- Accept: in-engine `INSTALL`/`LOAD`, connection `install_extension`/`load_extension`, Substrait plan methods/table functions, DuckLake `ATTACH`, snapshot/change-feed/maintenance functions, and extension rows on `DuckDbSession`
- Reject: treating DuckDB extensions as pip packages, importing extension modules, re-cataloguing extension package surfaces, hand-rolled Substrait protobuf codecs, manual DuckLake snapshot ledgers, and bespoke Parquet metadata mutation outside DuckLake
