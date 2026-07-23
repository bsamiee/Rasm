# [RASM_PERSISTENCE_API_PG_DUCKDB]

`pg_duckdb` embeds DuckDB's columnar-vectorized engine inside the PostgreSQL server, accelerating analytical SQL over live Postgres tables and reading data-lake Parquet, CSV, JSON, Iceberg, and Delta through `SETOF duckdb.row` table functions. `Store/provisioning#SERVER_EXTENSIONS` admits it preload-gated on the `columnar` lane, `FailureRank.Degradable` on absence; the in-process `DuckDB.NET` lane at `api-duckdb` runs the identical SQL dialect inside the .NET process, and every pg_duckdb surface is server-side SQL over the Npgsql/EF boundary — no managed assembly, no per-query engine spawn.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pg_duckdb`
- package: `pg_duckdb` (MIT)
- namespace: SQL `duckdb` schema (admin/secret/MotherDuck functions); `public` data-lake table functions; `duckdb.row` result type subscripted `r['col']`; `USING duckdb` table-storage clause
- abi / runtime: PG18 in-process extension, `shared_preload_libraries`-gated; one embedded DuckDB instance per Postgres connection
- asset: server extension, preload-gated
- rail: columnar-provisioning, analytical-lane

Each Postgres connection carries its own embedded DuckDB instance, so every resource setting is a connection-scoped ceiling; `duckdb.recycle_ddb()` resets it without reconnect.

## [02]-[EXECUTION_MODEL]

A query reaches DuckDB by touching a DuckDB-only feature — a `read_*` function, a `USING duckdb` table, or a remote `COPY` — or by `SET duckdb.force_execution = true` for a Postgres-only query; results cross back as `SETOF duckdb.row` projected through the `r['col']` subscript.

| [INDEX] | [SURFACE]            | [FORM]                               | [SEMANTICS]                        |
| :-----: | :------------------- | :----------------------------------- | :--------------------------------- |
|  [01]   | force execution      | `SET duckdb.force_execution = true`  | route a Postgres-only query        |
|  [02]   | row projection       | `r['col']` over `duckdb.row`         | select a column from a read row    |
|  [03]   | DuckDB storage table | `CREATE TABLE t USING duckdb AS ...` | MotherDuck-backed columnar table   |
|  [04]   | temp DuckDB table    | `CREATE TEMP TABLE t USING duckdb`   | session columnar scratch table     |
|  [05]   | DuckDB-syntax escape | `duckdb.query($$ ... $$)`            | run DuckDB-only SQL (PIVOT/STRUCT) |

Postgres and DuckDB writes never share one transaction, and PG `CREATE TABLE` never mixes with `CREATE TABLE ... USING duckdb` in one transaction; `duckdb.unsafe_allow_mixed_transactions` bypasses the split at a consistency cost.

## [03]-[DATA_LAKE_FUNCTIONS]

Each read function takes a `path` (`text` or `text[]`, glob or array) with optional DuckDB parameters passed `param := value`, returning `SETOF duckdb.row` expanded by `*` or the `r['col']` subscript; `iceberg_*`, `delta_scan`, and `read_vortex` need their named DuckDB extension installed first.

| [INDEX] | [FUNCTION]          | [SIGNATURE]                                    | [SEMANTICS]                    |
| :-----: | :------------------ | :--------------------------------------------- | :----------------------------- |
|  [01]   | `read_parquet`      | `(path, ...) -> SETOF duckdb.row`              | Parquet file or glob read      |
|  [02]   | `read_csv`          | `(path, ...) -> SETOF duckdb.row`              | CSV file or glob read          |
|  [03]   | `read_json`         | `(path, ...) -> SETOF duckdb.row`              | JSON file or glob read         |
|  [04]   | `read_vortex`       | `(path) -> SETOF duckdb.row`                   | Vortex read (`vortex` ext)     |
|  [05]   | `read_text`         | `(path) -> SETOF duckdb.row`                   | files as UTF-8 text rows       |
|  [06]   | `read_blob`         | `(path) -> SETOF duckdb.row`                   | files as binary blob rows      |
|  [07]   | `iceberg_scan`      | `(path, ...) -> SETOF duckdb.row`              | Iceberg table read (`iceberg`) |
|  [08]   | `iceberg_metadata`  | `(path, ...) -> SETOF iceberg_metadata_record` | Iceberg manifest metadata      |
|  [09]   | `iceberg_snapshots` | `(path, ...) -> SETOF iceberg_snapshot_record` | Iceberg snapshot history       |
|  [10]   | `delta_scan`        | `(path) -> SETOF duckdb.row`                   | Delta Lake read (`delta` ext)  |

## [04]-[ADMIN_SECRETS_MOTHERDUCK]

`duckdb.*` owns the control surface: extension bootstrap, direct-execution escapes, object-store credentials, and MotherDuck cloud compute. Install and execution functions default to superuser or the `duckdb.postgres_role`; `enable_motherduck` is a CALL procedure.

| [INDEX] | [SURFACE]                      | [SIGNATURE]                           | [SEMANTICS]                       |
| :-----: | :----------------------------- | :------------------------------------ | :-------------------------------- |
|  [01]   | `duckdb.install_extension`     | `(name)`                              | install a DuckDB extension        |
|  [02]   | `duckdb.load_extension`        | `(name)`                              | load an extension for the session |
|  [03]   | `duckdb.autoload_extension`    | `(name, bool)`                        | set an extension's autoload flag  |
|  [04]   | `duckdb.query`                 | `(sql) -> SETOF duckdb.row`           | run a SELECT in DuckDB            |
|  [05]   | `duckdb.raw_query`             | `(sql)`                               | run any query, result to logs     |
|  [06]   | `duckdb.recycle_ddb`           | `()`                                  | reset the connection's instance   |
|  [07]   | `duckdb.create_simple_secret`  | `(type, key_id, secret, region, ...)` | S3/GCS/R2 credential secret       |
|  [08]   | `duckdb.create_azure_secret`   | `(connection_string)`                 | Azure credential secret           |
|  [09]   | `duckdb.enable_motherduck`     | `(token)`                             | enable MotherDuck compute (CALL)  |
|  [10]   | `duckdb.is_motherduck_enabled` | `() -> bool`                          | MotherDuck enablement probe       |
|  [11]   | `duckdb.force_motherduck_sync` | `()`                                  | resync the MotherDuck catalog     |

## [05]-[SETTINGS]

A sealed columnar profile pins these `postgresql.conf` knobs; `[SCOPE]` names the change authority (`general` any session, `superuser` privileged, `restart` server-restart), and resource settings are per-connection because each connection carries its own embedded instance.

| [INDEX] | [SETTING]                                | [DEFAULT]                      | [SCOPE]   | [EFFECT]                           |
| :-----: | :--------------------------------------- | :----------------------------- | :-------- | :--------------------------------- |
|  [01]   | `duckdb.force_execution`                 | `false`                        | general   | route Postgres-only queries        |
|  [02]   | `duckdb.memory_limit`                    | `4096`                         | superuser | per-connection memory ceiling MB   |
|  [03]   | `duckdb.worker_threads`                  | `-1`                           | superuser | DuckDB threads per connection      |
|  [04]   | `duckdb.max_workers_per_postgres_scan`   | `2`                            | general   | PG workers per scan                |
|  [05]   | `duckdb.threads_for_postgres_scan`       | `2`                            | general   | DuckDB threads per PG scan         |
|  [06]   | `duckdb.postgres_role`                   | `""`                           | restart   | role granted execution + secrets   |
|  [07]   | `duckdb.enable_external_access`          | `true`                         | superuser | gate all file/HTTP/S3/Azure access |
|  [08]   | `duckdb.allowed_directories`             | `""`                           | superuser | allowlist under locked access      |
|  [09]   | `duckdb.disabled_filesystems`            | `""`                           | superuser | block named filesystems for all    |
|  [10]   | `duckdb.autoinstall_known_extensions`    | `true`                         | superuser | auto-INSTALL on first reference    |
|  [11]   | `duckdb.autoload_known_extensions`       | `true`                         | superuser | auto-LOAD on first reference       |
|  [12]   | `duckdb.allow_community_extensions`      | `false`                        | superuser | admit community extensions         |
|  [13]   | `duckdb.extension_directory`             | `DataDir/pg_duckdb/extensions` | superuser | extension install cache path       |
|  [14]   | `duckdb.temporary_directory`             | `DataDir/pg_duckdb/temp`       | superuser | spill directory                    |
|  [15]   | `duckdb.max_temp_directory_size`         | `""`                           | superuser | spill size cap                     |
|  [16]   | `duckdb.unsafe_allow_mixed_transactions` | `false`                        | general   | permit mixed PG/DuckDB writes      |

## [06]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every pg_duckdb op enters as server-side SQL over the Npgsql/EF boundary; the per-connection embedded DuckDB instance is the analytical execution root, `duckdb.force_execution` the routing switch, the `read_*`/`duckdb.query` table functions the data-lake root, and `SETOF duckdb.row` the one result carrier subscripted by `r['col']`.

[STACKING]:
- `api-duckdb`(`.api/api-duckdb.md`): `DuckDB.NET` runs the identical DuckDB SQL dialect and extension surface inside the .NET process, owning the standalone columnar store; pg_duckdb runs it embedded in the PG server so `read_parquet`/`iceberg_scan`/`delta_scan` join live Postgres tables with no ETL hop — the two meet at the columnar SQL surface, never a shared handle.
- `api-npgsql`(`.api/api-npgsql.md`)/`api-npgsql-ef`(`.api/api-npgsql-ef.md`): every surface lands as raw SQL over the Npgsql connection — the `SET`/`CALL` bootstrap and `duckdb.*` admin calls through `NpgsqlBatch`, the storage DDL via `MigrationBuilder.Sql`, and the `read_*` projections via `FromSql`/`SqlQuery` on the boundary the native analytical baseline rides.
- `Store/provisioning#SERVER_EXTENSIONS`: `ServerExtension` admits pg_duckdb `Preload`-gated on `shared_preload_libraries`, `FailureRank.Degradable` folding the `columnar` lane out on absence, `RestartClass.Restart` naming the preload-gap repair; a `CREATE EXTENSION` without the preload library hard-errors, so the row is verification-only, never a self-provisioned annotation.
- `Query/columnar`: DuckDB serves the analytical lane, reading the data-lake frames and columnstore-compressed chunks and meeting the managed `api-deltalake`/`api-parquetsharp` writers at the table path where the managed writer is the system of record and DuckDB the read/aggregate projection.
- within-lib: a sealed columnar profile pins the resource and security settings once (`memory_limit`, `worker_threads`, `enable_external_access`, `allowed_directories`, `disabled_filesystems`), bootstraps its data-lake extensions through `duckdb.install_extension`, resolves object-store credentials through `duckdb.create_simple_secret`, and routes DuckDB-only syntax through `duckdb.query` while automatic acceleration handles every standard analytical query.

[LOCAL_ADMISSION]:
- pg_duckdb enters behind the `Store/provisioning` verification fold like every server extension; its `columnar` lane is `FailureRank.Degradable`, so absence folds the DuckDB acceleration out at admission and the query runs on the native Postgres executor rather than faulting at first analytical query.
- Object-store credentials are `duckdb.create_simple_secret`/`duckdb.create_azure_secret` objects, never inline keys in a path; `enable_external_access` with `allowed_directories` bounds the reachable filesystems, and `postgres_role` scopes DuckDB execution off superuser.
- Extension loading is `duckdb.install_extension`/`load_extension` SQL at profile bootstrap, never a per-extension package.

[RAIL_LAW]:
- Package: `pg_duckdb`
- Owns: in-PostgreSQL DuckDB columnar execution — analytical acceleration over live Postgres tables, data-lake `read_*`/`iceberg_scan`/`delta_scan` table functions, `USING duckdb` MotherDuck-backed storage, and the `duckdb.query` DuckDB-syntax escape, all server-side SQL
- Accept: the preload-gated `ServerExtension` row, `SET duckdb.force_execution` routing, sealed resource and security settings, `CREATE SECRET` credentials, `duckdb.install_extension` bootstrap, `r['col']` projection over `SETOF duckdb.row`
- Reject: a managed assembly or per-query engine spawn, inline object-store keys, a self-provisioned `CREATE EXTENSION` without the preload library, mixed PG/DuckDB writes in one transaction, and treating the embedded analytical lane as the consistency owner
