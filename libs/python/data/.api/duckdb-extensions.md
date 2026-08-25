# [PY_DATA_API_DUCKDB_EXTENSIONS]

DuckDB loadable extensions install in-engine into a live `DuckDBPyConnection` through `install_extension`/`load_extension` or SQL `INSTALL`/`LOAD`, never as a pip row, Python module, or dependency entry. Loaded extensions expose their capability on the DuckDB SQL and bound-connection surface, and downstream owners compose that session rather than a per-extension Python package.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: DuckDB in-engine extensions
- package: none
- module: `duckdb`
- owner: `data`
- rail: query extensions

## [02]-[LOAD_ROWS]

[LOAD_ENTRY_SCOPE]: extension install and load

Each extension loads through `install_extension(name, *, repository)` / `load_extension(name)` or SQL `INSTALL <ext> [FROM community]; LOAD <ext>;`; the `[REPOSITORY]` row property selects the source, never a call-site branch.

| [INDEX] | [EXTENSION]        | [REPOSITORY] | [CONSUMER]                                      |
| :-----: | :----------------- | :----------- | :---------------------------------------------- |
|  [01]   | `httpfs`           | core         | remote `read_parquet` object scans              |
|  [02]   | `spatial`          | core         | `spatial/query` geometry SQL                    |
|  [03]   | `h3`               | community    | H3 bin and neighborhood SQL                     |
|  [04]   | `iceberg`          | core         | Iceberg metadata and table reads                |
|  [05]   | `substrait`        | community    | SQL to Substrait plan bridge                    |
|  [06]   | `ducklake`         | core         | DuckLake table-format catalog                   |
|  [07]   | `aws`              | core         | s3/gcs/r2 credential-chain PROVIDER over httpfs |
|  [08]   | `azure`            | core         | azure protocol beside its own secret type       |
|  [09]   | `postgres_scanner` | core         | operational-store `ATTACH … (TYPE postgres)`    |
|  [10]   | `delta`            | core         | `delta_scan` transaction-log table reads        |

[SECRET_ENTRY_SCOPE]: object-plane credential resolution

Provider rows carry DIFFERENT halves. `httpfs` registers the `s3`, `gcs`, and `r2` secret TYPES beside their protocol while `aws` supplies the `credential_chain` PROVIDER over all three, so an s3 scan loads both rows. `azure` registers its OWN protocol and its own `azure` secret type — `duckdb_secret_types()` names no azure type until that extension loads — so an azure scan needs that row alone. Scans against a real bucket therefore load the scheme's own rows and create a secret, rather than resolving whatever ambient environment the process carries. `credential_chain` walks the provider's own default chain — environment, config file, instance metadata — so no key material is spelled into SQL, and `SCOPE` binds one secret to one prefix when a session reads two buckets under different identities.

`CREATE SECRET` validates EAGERLY: a `credential_chain` provider resolving nothing raises `Secret Validation Failure` at the create statement, naming the chain link it stopped on, never later at scan time. Failure therefore lands at session setup as a provisioning fact, so a session that creates its secret has already proved the identity its scans use.

| [INDEX] | [SURFACE]        | [SHAPE]                                                                    | [CAPABILITY]                    |
| :-----: | :--------------- | :------------------------------------------------------------------------- | :------------------------------ |
|  [01]   | s3 chain secret  | `CREATE SECRET <n> (TYPE s3, PROVIDER credential_chain)`                   | AWS default-chain s3 access     |
|  [02]   | gcs chain secret | `CREATE SECRET <n> (TYPE gcs, PROVIDER credential_chain)`                  | GCS interoperability-key access |
|  [03]   | azure chain      | `CREATE SECRET <n> (TYPE azure, PROVIDER credential_chain)`                | Azure default-chain blob access |
|  [04]   | scoped secret    | `CREATE SECRET <n> (TYPE s3, PROVIDER credential_chain, SCOPE '<prefix>')` | one identity per prefix         |

[CONSUMER]: `tabular/columnar#SCAN` `_SCHEME_EXTENSION` maps the ref's own URI scheme onto its provider row, so a `RemoteGlob` loads `aws` for `s3`/`s3a`/`gs` and `azure` for `az`/`abfs`/`abfss` beside `httpfs`, and a `file://` glob loads neither.

[POSTGRES_ENTRY_SCOPE]: operational-store attach

`postgres_scanner` attaches a live PostgreSQL database as a DuckDB catalog whose tables read as ordinary relations, so one statement joins columnar files against operational rows with no second transport. `ATTACH` names the mount TYPE and never the extension: the row installs as `postgres_scanner` and attaches as `TYPE postgres`.

| [INDEX] | [SURFACE]      | [SHAPE]                                            | [CAPABILITY]                     |
| :-----: | :------------- | :------------------------------------------------- | :------------------------------- |
|  [01]   | attach catalog | `ATTACH '<dsn>' AS <n> (TYPE postgres)`            | mount a live database            |
|  [02]   | read-only      | `ATTACH '<dsn>' AS <n> (TYPE postgres, READ_ONLY)` | refuse write-back at the mount   |
|  [03]   | scan function  | `postgres_scan('<dsn>', '<schema>', '<table>')`    | one relation with no attach      |
|  [04]   | pushdown scan  | `postgres_scan_pushdown('<dsn>', '<s>', '<t>')`    | filter and projection pushdown   |
|  [05]   | passthrough    | `postgres_query('<n>', '<sql>')`                   | server-side statement execution  |
|  [06]   | statement      | `postgres_execute('<n>', '<sql>')`                 | non-returning server statement   |
|  [07]   | pool tuning    | `postgres_configure_pool('<n>', <size>)`           | connection-pool sizing per mount |
|  [08]   | secret mount   | `CREATE SECRET <n> (TYPE postgres, ...)`           | credential off the attach string |

[CONSUMER]: `tabular/columnar#SCAN` `Attach` carries the row through `DuckDbExtension.attach_type`, whose `_ATTACH_TYPE` entry spells the keyword divergence once, and `DuckDbSession.connect` loads each attach row's own extension before the `ATTACH` executes. `tabular/query#QUERY` `QueryEngine.session` is where a caller spells that `Attach`, so the `[TENANT_COST_JOIN]` fold against live tenant, grant, and workload tables runs as ONE statement beside the evidence residence rather than through a second transport.

[DELTA_ENTRY_SCOPE]: Delta transaction-log reads

`delta` reads a Delta table's transaction log natively and exposes `delta_scan('<uri>')` as a SQL TABLE FUNCTION — like every reader here and unlike a connection method, so a binding names the function and the arm spells `SELECT * FROM delta_scan(<uri>)`. Live-verified: `duckdb_extensions()` rows it `core`, and once loaded `duckdb_functions()` types `delta_scan` as `table` beside `delta_list_files`, `delta_domain_metadata`, and the transaction-version pair.

| [INDEX] | [SURFACE]                       | [SHAPE]                              | [CAPABILITY]                     |
| :-----: | :------------------------------ | :----------------------------------- | :------------------------------- |
|  [01]   | `delta_scan`                    | `SELECT * FROM delta_scan('<uri>')`  | read a Delta table as a relation |
|  [02]   | `delta_list_files`              | `delta_list_files('<uri>')`          | active data-file roster          |
|  [03]   | `delta_domain_metadata`         | `delta_domain_metadata('<uri>')`     | domain metadata rows             |
|  [04]   | `delta_get_transaction_version` | `delta_get_transaction_version(...)` | last committed app version       |
|  [05]   | `delta_set_transaction_version` | `delta_set_transaction_version(...)` | record an app version            |

[CONSUMER]: `tabular/columnar#SCAN` seats `delta_scan` beside this extension for `DatasetKind.DELTA`, so `ScanPlan.DuckDb` reads Delta tables through the extension-owned scan.

## [03]-[SUBSTRAIT]

[SUBSTRAIT_ENTRY_SCOPE]: Substrait plan serialization and execution

`substrait` adds SQL table functions only — no connection-bound Python method, no extension-owned class — each reached through `con.execute`. Both serializers gate optimize-before-serialize on the `enable_optimizer` named argument; both executors take the foreign plan payload. Binary plans are Substrait protobuf; JSON plans are the inspectable twin of the same logical plan.

Every argument binds as a prepared parameter — the SQL text, the `enable_optimizer` flag, and the plan payload alike — so only the function name is ever spelled into the statement. Each serializer returns one row read through `fetchone()[0]` as `bytes` (binary) or `str` (JSON); each executor returns a result set read through `to_arrow_table()` or the incremental `to_arrow_reader()`.

| [INDEX] | [SURFACE]             | [SHAPE]                                             | [CAPABILITY]                         |
| :-----: | :-------------------- | :-------------------------------------------------- | :----------------------------------- |
|  [01]   | `get_substrait`       | `CALL get_substrait(?, enable_optimizer => ?)`      | serialize SQL to binary plan (BLOB)  |
|  [02]   | `get_substrait_json`  | `CALL get_substrait_json(?, enable_optimizer => ?)` | serialize SQL to JSON plan (VARCHAR) |
|  [03]   | `from_substrait`      | `CALL from_substrait(?)`                            | execute binary Substrait plan        |
|  [04]   | `from_substrait_json` | `CALL from_substrait_json(?)`                       | execute JSON Substrait plan          |

[CONSUMER]: `tabular/query#QUERY` `_ir_plan` reads the `(serialize, execute)` pair off its `_SUBSTRAIT` row per `PlanWire` half and threads both `CALL`s through `con.execute`; `IrEmit.optimize` is the `enable_optimizer` value and `IrEmit.streaming` selects the reader over the table.

## [04]-[DUCKLAKE]

[DUCKLAKE_ENTRY_SCOPE]: DuckLake attach, snapshots, change feed, and maintenance

DuckLake is a DuckDB core extension attaching a table-format catalog backed by Parquet data files under DuckDB, SQLite, or PostgreSQL metadata, reached from Python through DuckDB SQL and cursors. Every mount is `ATTACH 'ducklake:<backend>:<dsn>' AS <name> (<clause>...)` where `<backend>` is empty (DuckDB), `sqlite:`, or `postgres:`. Metadata records snapshot id, snapshot time, schema version, commit author/message/extra, data-file path, row count, byte size, and change type.

[ATTACH_CLAUSES]: `DATA_PATH` `METADATA_SCHEMA` `ENCRYPTED` `DATA_INLINING_ROW_LIMIT` `READ_ONLY` `AUTOMATIC_MIGRATION` `CREATE_IF_NOT_EXISTS` `OVERRIDE_DATA_PATH`

| [INDEX] | [SURFACE]                  | [SHAPE]                                                                     | [CAPABILITY]                |
| :-----: | :------------------------- | :-------------------------------------------------------------------------- | :-------------------------- |
|  [01]   | attach DuckDB metadata     | `ATTACH 'ducklake:<file>.ducklake' AS <n> (DATA_PATH '<dir>/')`             | mount DuckLake catalog      |
|  [02]   | attach SQLite metadata     | `ATTACH 'ducklake:sqlite:<file>' AS <n> (DATA_PATH '<dir>/')`               | mount SQLite catalog        |
|  [03]   | attach Postgres metadata   | `ATTACH 'ducklake:postgres:<dsn>' AS <n> (DATA_PATH '<uri>')`               | mount Postgres catalog      |
|  [04]   | secret attach              | `CREATE SECRET (TYPE ducklake, ...)` + `ATTACH 'ducklake:<secret>'`         | secret-backed mount         |
|  [05]   | snapshots                  | `<cat>.snapshots()` / `ducklake_snapshots('<cat>')`                         | list snapshot history       |
|  [06]   | current and last committed | `<cat>.current_snapshot()` / `ducklake_last_committed_snapshot(...)`        | snapshot identity           |
|  [07]   | time travel                | `FROM <table> AT (VERSION => n)` / `AT (TIMESTAMP => ts)`                   | historical table read       |
|  [08]   | change feed                | `table_changes('<t>', <from>, <to>)`, `table_insertions`, `table_deletions` | row-level lineage           |
|  [09]   | commit metadata            | `set_commit_message`                                                        | commit author/message/extra |
|  [10]   | data-file registration     | `ducklake_add_data_files`                                                   | register existing Parquet   |
|  [11]   | scans and file listing     | `ducklake_scan` / `ducklake_list_files`                                     | scan or list physical files |
|  [12]   | expire snapshots           | `ducklake_expire_snapshots`                                                 | drop old snapshot history   |
|  [13]   | merge adjacent files       | `ducklake_merge_adjacent_files`                                             | compact adjacent data files |
|  [14]   | rewrite data files         | `ducklake_rewrite_data_files`                                               | rewrite by delete ratio     |
|  [15]   | cleanup old files          | `ducklake_cleanup_old_files`                                                | clean superseded files      |
|  [16]   | delete orphaned files      | `ducklake_delete_orphaned_files`                                            | delete orphaned Parquet     |
|  [17]   | flush inlined data         | `ducklake_flush_inlined_data`                                               | flush inlined data rows     |

[CHANGE_FEED_SHAPE]: `table_changes` prepends three columns to the table's own, then repeats every table column

| [INDEX] | [COLUMN]      | [TYPE]    | [CAPABILITY]                                              |
| :-----: | :------------ | :-------- | :-------------------------------------------------------- |
|  [01]   | `snapshot_id` | `BIGINT`  | the snapshot the change committed under                   |
|  [02]   | `rowid`       | `BIGINT`  | row identity within the changed file                      |
|  [03]   | `change_type` | `VARCHAR` | `insert`, `delete`, `update_preimage`, `update_postimage` |

- DuckLake spells the discriminant `change_type`, dropping the underscore Delta's `_change_type` carries
- Consumers keying one spelling across both formats filter a real feed to silent emptiness
- Updates emit BOTH halves, `update_preimage` the row as it stood and `update_postimage` the row as it stands
- Survivor-state reads keep the post half alone
- Snapshot ranges close on both ends and refuse a bound past the newest snapshot
- Callers read `current_snapshot()` before naming an upper bound

[OPTION_SURFACE]: `set_option` `ducklake_options` `ducklake_settings` `ducklake_table_info` inspect and mutate catalog policy.

## [05]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Extensions load in-engine on the owning connection; the `core`/`community` repository is a row property, never a call-site branch.
- Loaded extensions attach their SQL functions and bound methods to the DuckDB session, so downstream owners compose the attached SQL or bound connection.
- Loading attaches NO Python members: `DuckDBPyConnection` gains no `get_substrait`/`from_substrait` method and no extension class, so a connection-method spelling of any row above resolves nowhere and the `CALL` shape is the whole surface.
- DuckLake object-store credentials route through DuckDB secrets; Python never opens the Parquet data files or metadata catalog directly.
- Substrait crosses as the shared protobuf `Plan` wire artifact, emitted and consumed by the extension, never a private codec.

[STACKING]:
- `datafusion`(`.api/datafusion.md`): the `CALL get_substrait` BLOB is the same wire `Plan` `datafusion.substrait.Consumer` ingests, and `Producer` output feeds `CALL from_substrait`; neither side reimplements the protobuf codec.
- `substrait`(`.api/substrait.md`): `Plan.ParseFromString` validates the emitted BLOB before a peer engine executes it. `get_substrait` emits a comparison-function anchor whose urn is the bare `extension:io.substrait:` — live-verified, and referenced by the declaration carrying `gt:i64_i64` — where the bundled registry rows `extension:io.substrait:functions_comparison`. `ExtensionRegistry.lookup_urn` therefore answers `None` for it, so a DuckDB-minted plan routed into the inbound plan gate refuses as an unknown extension. Estate plans mint and execute inside ONE connection instead, and that gate stays for the foreign wire it exists to admit.
- `duckdb`(`.api/duckdb.md`): the `data` owner loads extensions on one bound connection and composes their SQL through `con.execute`/`con.sql`.

[LOCAL_ADMISSION]:
- Extensions enter as load rows on the DuckDB session, never a pip dependency, module import, or their own `.api` catalog.

[RAIL_LAW]:
- Package: DuckDB loadable extensions
- Owns: connection-scoped load evidence and the SQL surface each loaded extension attaches
- Accept: in-engine `INSTALL`/`LOAD`, `install_extension`/`load_extension`, Substrait plan SQL table functions, DuckLake `ATTACH`, snapshot/change-feed/maintenance functions, and extension rows on `DuckDbSession`
- Reject: a per-extension pip or module row, a hand-rolled Substrait protobuf codec, a manual DuckLake snapshot ledger, and bespoke Parquet metadata mutation outside DuckLake
