# [RASM_PERSISTENCE_API_PG_SERVER_BGWORKERS]

PG18 server-tier maintenance companions carry the Persistence PostgreSQL profile's in-database cadence, partition lifecycle, bloat reclaim, document validation, and audit trail as server-side SQL alone. Each installs through a `ServerExtension` `CreateSql` row, binds through raw SQL or a GUC `SET`, and verifies read-only against `pg_settings` — no managed assembly and no EF translator crosses into this tier.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pg_cron` `pg_partman` `pg_squeeze` `pg_jsonschema` `pgaudit`
- packages: `pg_cron` (PostgreSQL), `pg_partman` (PostgreSQL), `pg_squeeze` (BSD-3-Clause), `pg_jsonschema` (Apache-2.0), `pgaudit` (PostgreSQL)
- namespace: SQL — the `cron`, `partman`, `squeeze`, and `pgaudit` schemas; `pg_jsonschema` registers unqualified functions and the `jsonschema` type
- registration: `pg_cron`, `pg_partman_bgw`, `pg_squeeze`, and `pgaudit` ride the `ClusterConfig` `shared_preload_libraries` row and verify through `PreloadProbe`; `pg_jsonschema` registers on `CREATE EXTENSION` alone
- consumed by: `Store/provisioning#SERVER_EXTENSIONS` `ServerExtension` rows, `Query/lane#DOCUMENT_LANE`, `Version/retention#AUDIT_BINDING`, the AppHost persistence-maintenance schedule
- rail: cluster-config, document-lane, audit-binding, schedule

## [02]-[PG_CRON]

[CRON_ENTRY_SCOPE]: cluster-local SQL job scheduling

One bgworker drives every job from the `cron.job` registry. Schedule grammar admits 5-field cron, `$` for last day of month, and a bare `[1-59] seconds` interval no other unit may join. `pg_cron` installs in exactly one database — `cron.database_name` — so `cron.schedule_in_database` reaches every other.

| [INDEX] | [SURFACE]                                                                 | [SHAPE]  | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------------------------------ | :------- | :---------------------------------------------- |
|  [01]   | `cron.schedule(text, text) -> bigint`                                     | function | schedule an unnamed job; returns the jobid      |
|  [02]   | `cron.schedule(text, text, text) -> bigint`                               | function | named job — the idempotent re-schedule key      |
|  [03]   | `cron.schedule_in_database(text, text, text, text, text, bool) -> bigint` | function | job scoped to a target database and role        |
|  [04]   | `cron.unschedule(text) -> bool`                                           | function | remove by job name                              |
|  [05]   | `cron.unschedule(bigint) -> bool`                                         | function | remove by jobid                                 |
|  [06]   | `cron.alter_job(bigint, text, text, text, text, bool)`                    | function | mutate cadence, command, role, or active flag   |
|  [07]   | `cron.job`                                                                | table    | job registry under RLS keyed on `username`      |
|  [08]   | `cron.job_run_details`                                                    | table    | run history: `status`, `return_message`, timing |

- `cron.use_background_workers`: `off` opens a libpq connection per run, so the run needs `pg_hba` trust or a `cron.host` socket path; `on` executes in bgworkers with no auth leg.
- `cron.job_run_details` never self-prunes; a `cron.schedule`d `DELETE` over `end_time` is its retention route.
- GUC set: `cron.database_name` `cron.host` `cron.timezone` `cron.max_running_jobs` `cron.launch_active_jobs` `cron.log_run` `cron.enable_superuser_jobs`.

## [03]-[PG_PARTMAN]

[PARTMAN_ENTRY_SCOPE]: declarative partition lifecycle over a PG18-native partitioned parent

`partman.create_parent` declares the set, the `pg_partman_bgw` worker rolls it forward, and `part_config.retention` drops expired children — retention is partition drop, never row `DELETE`. Calls take named `p_*` params; `create_parent` defaults `p_type := 'range'` and `p_premake := 4`, and an id-based interval of `1` requires `p_type := 'list'`.

| [INDEX] | [SURFACE]                                                            | [SHAPE]   | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------------------------- | :-------- | :---------------------------------------------- |
|  [01]   | `partman.create_parent(p_parent_table, p_control, p_interval, …)`    | function  | declare a partition set                         |
|  [02]   | `partman.run_maintenance(p_parent_table)`                            | function  | roll one parent forward and apply its retention |
|  [03]   | `partman.run_maintenance_proc()`                                     | procedure | commit-batched maintenance over every parent    |
|  [04]   | `partman.partition_data_proc(p_parent_table, p_loop_count, …)`       | procedure | back-fill existing rows in commit batches       |
|  [05]   | `partman.drop_partition_time(p_parent_table, p_retention, …) -> int` | function  | one-shot drop overriding the configured policy  |
|  [06]   | `partman.part_config`                                                | table     | per-parent policy registry                      |
|  [07]   | `partman.part_config_sub`                                            | table     | subpartition policy child parents inherit       |

- `partman.part_config` policy columns: `premake` `automatic_maintenance` `retention` `retention_schema` `retention_keep_table` `retention_keep_index` `infinite_time_partitions` `template_table` `maintenance_order`.

## [04]-[PG_SQUEEZE]

[SQUEEZE_ENTRY_SCOPE]: online bloat reclamation

A bloated table rewrites from the WAL decode stream and takes `ACCESS EXCLUSIVE` only at the final swap — the in-DB path where `VACUUM FULL` locks the whole rewrite and `pg_repack` needs an out-of-DB client. One `squeeze.tables` row registers a table for periodic bloat checks, and the scheduler worker queues a task whenever its thresholds trip.

| [INDEX] | [SURFACE]                                               | [SHAPE]  | [CAPABILITY]                                      |
| :-----: | :------------------------------------------------------ | :------- | :------------------------------------------------ |
|  [01]   | `squeeze.squeeze_table(name, name, name, name, name[])` | function | one-shot online rewrite of one table              |
|  [02]   | `squeeze.start_worker()`                                | function | start the per-database scheduler worker           |
|  [03]   | `squeeze.stop_worker()`                                 | function | stop every worker in the current database         |
|  [04]   | `squeeze.get_active_workers()`                          | function | in-flight worker progress keyed by `pid`          |
|  [05]   | `squeeze.tables`                                        | table    | scheduled-table registry, the sole write target   |
|  [06]   | `squeeze.log`                                           | table    | one row per completed rewrite; the receipt source |
|  [07]   | `squeeze.errors`                                        | table    | per-task failure rows                             |

- `squeeze.squeeze_table` args: `(tabschema, tabname, clustering_index, rel_tablespace, ind_tablespaces)` — the index and tablespace args NULL out.
- `squeeze.tables` policy columns: `schedule` `free_space_extra` `min_size` `vacuum_max_age` `max_retry` `skip_analyze`. `schedule` is a `(minutes, hours, days_of_month, months, days_of_week)` composite, not a cron string, and admits no NULL row value.
- `squeeze_table` is non-transactional: it hands the table to a worker and exits, so rolling back the calling transaction leaves the rewrite running.
- An `ALTER TABLE`, `VACUUM FULL`, `CLUSTER`, or `TRUNCATE` committing mid-rewrite aborts the squeeze and rolls its changes back; `max_retry` bounds the re-attempts and the schedule moves to avoid the collision.

## [05]-[PG_JSONSCHEMA]

[JSONSCHEMA_ENTRY_SCOPE]: server-side JSON Schema validation inside a column `CHECK`

`CREATE EXTENSION` alone registers the functions and the `jsonschema` type — no preload row. Casting a schema to `jsonschema` compiles the validator once and caches it per call site, so a hot `CHECK` never recompiles per row; the uncompiled overloads recompile on every evaluation.

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :------------------------------------------------------------------ | :------- | :-------------------------------------------------- |
|  [01]   | `jsonb_matches_schema(json, jsonb) -> bool`                         | function | validate a `jsonb` value inside a `CHECK`           |
|  [02]   | `json_matches_schema(json, json) -> bool`                           | function | validate a `json` value inside a `CHECK`            |
|  [03]   | `jsonb_matches_compiled_schema(jsonschema, jsonb) -> bool`          | function | same verdict over a pre-compiled schema             |
|  [04]   | `json_matches_compiled_schema(jsonschema, json) -> bool`            | function | same verdict over a pre-compiled schema             |
|  [05]   | `jsonschema_validation_errors(json, json) -> text[]`                | function | per-error messages for a failed instance            |
|  [06]   | `jsonb_validation_errors_compiled(jsonschema, jsonb) -> text[]`     | function | per-error messages over a pre-compiled schema       |
|  [07]   | `jsonschema_validation_errors_compiled(jsonschema, json) -> text[]` | function | per-error messages over a pre-compiled schema       |
|  [08]   | `jsonschema_is_valid(json) -> bool`                                 | function | the schema document is itself well-formed           |
|  [09]   | `jsonschema`                                                        | type     | compiled-validator cast target, cached per callsite |

- `Query/lane#DOCUMENT_LANE` declares one `ServerExtension` `CreateSql` over `(Table, Column, Constraint, Schema)` emitting `ALTER TABLE … ADD CONSTRAINT … CHECK (jsonb_matches_schema('<schema>', <column>))`, so a declared-shape document is rejected at WRITE.
- Schema strings arrive pre-frozen from the document-lane shape, never raw runtime input, so the `jsonschema` cast stays stable for the life of the constraint.
- A `Validate` receipt reads `jsonschema_validation_errors` for the per-error rail the `CHECK` boolean cannot surface.
- Absent the pgrx-compiled extension, the `ServerExtension("pg_jsonschema", Fallback: "Json.Schema.JsonSchema.Evaluate")` row moves the same verdict in-process; `api-jsonschema-net.md` owns that evaluator's surface.

## [06]-[PGAUDIT]

[PGAUDIT_ENTRY_SCOPE]: session and object audit logging into the server log

Every surface is a GUC bound through `SET`; the runtime obligation is the bound value verified read-only through `SELECT setting FROM pg_settings WHERE name = ANY(...)`. `Version/retention#AUDIT_BINDING` maps one `DataClassification` to one audit class.

| [INDEX] | [SURFACE]                        | [SHAPE] | [CAPABILITY]                                                         |
| :-----: | :------------------------------- | :------ | :------------------------------------------------------------------- |
|  [01]   | `pgaudit.log`                    | guc     | class selection: `read` `write` `function` `role` `ddl` `misc` `all` |
|  [02]   | `pgaudit.log_catalog`            | guc     | include statements whose relations all sit in `pg_catalog`           |
|  [03]   | `pgaudit.log_client`             | guc     | echo audit entries to the client connection                          |
|  [04]   | `pgaudit.log_level`              | guc     | server log level the audit entries write at                          |
|  [05]   | `pgaudit.log_parameter`          | guc     | include bound statement parameters                                   |
|  [06]   | `pgaudit.log_parameter_max_size` | guc     | byte ceiling above which a parameter logs suppressed                 |
|  [07]   | `pgaudit.log_relation`           | guc     | one entry per relation in a statement                                |
|  [08]   | `pgaudit.log_rows`               | guc     | include the row count a statement retrieved or affected              |
|  [09]   | `pgaudit.log_statement`          | guc     | include the statement text                                           |
|  [10]   | `pgaudit.log_statement_once`     | guc     | statement text on the first entry only                               |
|  [11]   | `pgaudit.role`                   | guc     | object-audit role whose `GRANT`s select per-object auditing          |
|  [12]   | `pg_settings`                    | view    | read-only GUC verification probe                                     |

- A missing `pgaudit` preload entry folds into the `<server-not-provisioned>` provisioning fault.
- Object-level auditing rides `pgaudit.role` and a `GRANT` to that role, so per-object scope needs no per-table GUC.

## [07]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every surface lands through the EF `MigrationBuilder.Sql` rail — a `ServerExtension` `CreateSql`, a GUC `SET`, or a `cron`/`squeeze` registry `INSERT`. `HasPostgresExtension` cannot encode a preload prerequisite, so a preload-gated `CREATE EXTENSION` rides raw SQL.
- Cadence ownership partitions with no overlap: TimescaleDB `add_*_policy` bgworkers own continuous-aggregate, retention, and columnstore cadence; `pg_partman_bgw` owns partition rotation; `pg_squeeze` owns bloat-reclaim cadence through its `squeeze.tables.schedule` column; `pg_cron` owns server-local jobs alone; the process-side `persistence-maintenance` schedule owns `ANALYZE` and `REINDEX`.

[STACKING]:
- `timescaledb`(`.api/api-timescaledb.md`): native `add_retention_policy`/`add_continuous_aggregate_policy` bgworkers own hypertable cadence, so a `partman.create_parent` set covers the non-hypertable history tables and `cron.schedule` never re-drives a Timescale policy.
- `JsonSchema.Net`(`.api/api-jsonschema-net.md`): `Json.Schema.JsonSchema.Evaluate` computes the `jsonb_matches_schema` verdict in-process, selected by the `ServerExtension` `Fallback` row when the extension is absent.
- `pg_net`(`.api/api-pg-net.md`): a `cron.schedule_in_database` job body calls `net.http_post`, so a server-local cadence drives outbound HTTP with no process leg.
- within-lib: one `cron.schedule` composes `partman.run_maintenance_proc()` and a `squeeze.tables` registration into a single server-local cadence, and `cron.job_run_details` joined against `squeeze.log` and `partman.part_config` projects one maintenance receipt across all three workers.

[LOCAL_ADMISSION]:
- A server-local cadence enters only where the process must not own the job; anything the AppHost schedule port can drive stays process-side.
- A partitioned history table declares one `create_parent` call in a migration; `part_config.retention` is the drop policy the `Version/retention` destructive gate reads.
- A `jsonb` column carrying a declared document shape takes one `CHECK` over the pre-frozen schema, cast to `jsonschema` where the column is write-hot.
- An audit class binds through the `Version/retention#AUDIT_BINDING` classification table and verifies read-only; preloads are deploy-time `postgresql.conf` values the runtime observes.

[RAIL_LAW]:
- Package: `pg_cron` (PostgreSQL), `pg_partman` (PostgreSQL), `pg_squeeze` (BSD-3-Clause), `pg_jsonschema` (Apache-2.0), `pgaudit` (PostgreSQL)
- Owns: server-tier scheduled maintenance, declarative partitioning, online bloat reclaim, server-side document validation, and audit logging — server-side SQL the managed tier emits and verifies
- Accept: `cron.schedule_in_database` for server-local cadence, `partman.create_parent` and `part_config` for partition lifecycle, `squeeze.tables` rows for scheduled reclaim, `jsonb_matches_schema` inside a `ServerExtension` `CreateSql` with the compiled `jsonschema` cast on hot columns, `pgaudit` GUCs bound per the audit-binding classification table
- Reject: a hand-rolled partition-rotation or bloat-reclaim job, an out-of-DB reclaim or audit-pipeline client, a runtime `ALTER SYSTEM` setting a preload, a second document validator beside `pg_jsonschema` and its in-process fallback
