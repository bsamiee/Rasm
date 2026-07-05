# [RASM_PERSISTENCE_API_TIMESCALEDB]

`TimescaleDB` supplies the hypertable, continuous-aggregate, retention, and columnstore (hypercore)
provisioning surface over the `OpLogEntry`-rollup table, plus its native bgworker policy scheduler so
the AppHost schedule port never schedules a refresh/retention/compression job. It carries no managed
assembly and no first-party EF translator: every surface is server-side SQL the
`Store/provisioning#SERVER_EXTENSIONS` `ServerExtension` rows fold — as the `Hypertable`/`ContinuousAggregate`/
`RetentionPolicy`/`ColumnstorePolicy` cases whose `ProvisionSql` `Fin<string>` projection rides
`MigrationBuilder.Sql` through the one `ServerExtension` `CreateSql` install (`Store/provisioning#SERVER_EXTENSIONS`) —
and the rollups feed `Query/columnar#COLUMNAR_LANE` and the dashboard tiles. The extension is
preload-gated (it rides the `Store/provisioning#SERVER_EXTENSIONS` `Preload` `shared_preload_libraries`
row), never a self-provisioned `CREATE EXTENSION` annotation.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `timescaledb`
- package: `timescaledb` (server-side PostgreSQL extension, not a NuGet package)
- namespace: SQL (`public` function/procedure set; `timescaledb_information.*` views; `timescaledb.*` storage parameters)
- license: Apache-2.0 edition (Community-licensed continuous-aggregate/columnstore/retention policies run under the TSL boundary at the DB deployment, never linked into managed code)
- asset: server extension, preload-gated (`shared_preload_libraries`), bgworker policy scheduler
- emission split: SELECT-functions vs CALL-procedures (the `ServerExtension` `CreateSql` projection must emit the correct verb in its `Fin<string>` — see `[06]`)
- rail: timescale-provisioning, analytical-lane

The time column is the HLC `Physical` instant on the rollup table; the rollup mirrors the `OpLogEntry`
columns the `DuckDBOpLogMap` projects on `Query/columnar#COLUMNAR_LANE`. One time dimension is the
partition key; secondary `by_hash` dimensions are held under a provisioning probe, not catalogued.

## [02]-[HYPERTABLE]

`create_hypertable`/`add_dimension` are SELECT functions taking a dimension-builder argument
(`by_range`/`by_hash`); the modern `CREATE TABLE ... WITH (tsdb.hypertable, tsdb.partition_column=...)`
declarative form is the equivalent the fold may emit instead, but the function form is what
`MigrationBuilder.Sql` carries against an already-`CREATE TABLE`-d rollup.

| [INDEX] | [FUNCTION]          | [SIGNATURE]                                                                                           | [SEMANTICS]                                       |
| :-----: | :------------------ | :--------------------------------------------------------------------------------------------------- | :------------------------------------------------ |
|  [01]   | `create_hypertable` | `SELECT create_hypertable('tbl', by_range('time_col', INTERVAL 'i'), create_default_indexes => TRUE, if_not_exists => TRUE, migrate_data => FALSE)` | partition an existing table into time-dimension chunks |
|  [02]   | `by_range`          | `by_range('time_col', INTERVAL 'i')`                                                                  | range dimension builder (the time partition)      |
|  [03]   | `add_dimension`     | `SELECT add_dimension('tbl', by_range('col', INTERVAL 'i'), if_not_exists => TRUE)` (or `by_hash('col', n)`) | add a secondary range or hash partition dimension |

## [03]-[CONTINUOUS_AGGREGATE]

A continuous aggregate is a `MATERIALIZED VIEW WITH (timescaledb.continuous)` over a `time_bucket`
group, refreshed by a bgworker policy (a SELECT function returning the job_id) and refreshable
manually by a CALL procedure.

| [INDEX] | [SURFACE]                         | [FORM]                                                                                                                                  |
| :-----: | :-------------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | view                              | `CREATE MATERIALIZED VIEW v WITH (timescaledb.continuous) AS SELECT time_bucket('b', t) AS bucket, ... FROM tbl GROUP BY 1 WITH NO DATA` |
|  [02]   | `time_bucket`                     | `time_bucket('1 hour', time_col)` — the bucketing primitive (`time_bucket_gapfill` for gap-filled tiles)                                |
|  [03]   | `add_continuous_aggregate_policy` | `SELECT add_continuous_aggregate_policy('v', start_offset => INTERVAL 's', end_offset => INTERVAL 'e', schedule_interval => INTERVAL 'sched', if_not_exists => TRUE)` — also `buckets_per_batch`, `max_batches_per_execution`, `refresh_newest_first`, `initial_start`, `timezone` |
|  [04]   | `refresh_continuous_aggregate`    | `CALL refresh_continuous_aggregate('v', window_start, window_end, force => FALSE)` — manual/backfill refresh (procedure, not in a txn block) |
|  [05]   | `remove_continuous_aggregate_policy` | `SELECT remove_continuous_aggregate_policy('v', if_not_exists => TRUE)`                                                              |

## [04]-[RETENTION_COLUMNSTORE]

The retention and hypercore (columnstore) policies and their bgworker schedulers. The columnstore
enable is an `ALTER` storage-parameter set; the policy adder is a **CALL procedure** (unlike the SELECT
retention/cagg adders); manual per-chunk conversion is a CALL procedure.

| [INDEX] | [FUNCTION]               | [SIGNATURE]                                                                                                           | [SEMANTICS]                        |
| :-----: | :----------------------- | :-------------------------------------------------------------------------------------------------------------------- | :--------------------------------- |
|  [01]   | `add_retention_policy`   | `SELECT add_retention_policy('tbl', drop_after => INTERVAL 'd', schedule_interval => INTERVAL 's', if_not_exists => TRUE)` — also `drop_created_before`, `initial_start`, `timezone` | drop chunks older than the bound (returns job_id) |
|  [02]   | `remove_retention_policy` | `SELECT remove_retention_policy('tbl', if_not_exists => TRUE)`                                                       | remove the retention job           |
|  [03]   | columnstore enable       | `ALTER TABLE tbl SET (timescaledb.enable_columnstore = true, timescaledb.segmentby = 's', timescaledb.orderby = 'o')` (or `ALTER MATERIALIZED VIEW` for a cagg) | hypercore columnar conversion arm |
|  [04]   | `add_columnstore_policy` | `CALL add_columnstore_policy('tbl', after => INTERVAL 'a', schedule_interval => INTERVAL 's', if_not_exists => TRUE)` — also `created_before`, `initial_start`, `timezone` | schedule compression of chunks past the age bound (procedure) |
|  [05]   | `remove_columnstore_policy` | `CALL remove_columnstore_policy('tbl', if_exists => TRUE)`                                                          | remove the columnstore job         |
|  [06]   | `convert_to_columnstore` / `convert_to_rowstore` | `CALL convert_to_columnstore('chunk', if_not_columnstore => TRUE, recompress => FALSE)` / `CALL convert_to_rowstore('chunk', if_columnstore => TRUE)` | manual per-chunk compress/decompress |

## [05]-[JOB_STATS_CONTROL]

The job-stats and informational views the provisioning receipt reads for refresh-lag, retention
drop-count, and compression-ratio proof rows, plus the job-control surface that reconfigures a
scheduled policy without dropping it.

| [INDEX] | [SURFACE]                                       | [SEMANTICS]                                                                  |
| :-----: | :---------------------------------------------- | :-------------------------------------------------------------------------- |
|  [01]   | `timescaledb_information.jobs`                   | configured policy jobs (`job_id`, `proc_name`, `schedule_interval`, `config`, `hypertable_name`) |
|  [02]   | `timescaledb_information.job_stats`             | `job_id`, `last_run_started_at`, `last_successful_finish`, `last_run_status`, `job_status`, `last_run_duration`, `next_start`, `total_runs`, `total_successes`, `total_failures` |
|  [03]   | `timescaledb_information.job_errors`            | per-failure error log (`job_id`, `proc_name`, `err_message`, `start_time`)  |
|  [04]   | `timescaledb_information.continuous_aggregates` | cagg definition, `materialization_hypertable_name`, refresh/finalized state |
|  [05]   | `hypertable_columnstore_stats('tbl')`           | per-hypertable before/after compression bytes, ratio, and chunk counts      |
|  [06]   | `chunk_columnstore_stats('tbl')`                | per-chunk compression detail for a targeted columnstore proof row           |
|  [07]   | `alter_job` / `delete_job` / `run_job` / `add_job` | `SELECT alter_job(job_id, schedule_interval => ..., scheduled => ..., config => ..., next_start => ..., if_exists => TRUE)` and `SELECT delete_job(job_id)` are functions; `CALL run_job(job_id)` is a procedure (manual fire — the receipt's remediation arm); `SELECT add_job(proc, schedule_interval, ...)` registers a user-defined-action job |

## [06]-[IMPLEMENTATION_LAW]

[EMISSION_LAW]:
- The `Hypertable`/`ContinuousAggregate`/`RetentionPolicy`/`ColumnstorePolicy` cases' `ProvisionSql` `Fin<string>.Succ` must carry `SELECT <fn>(...)` for `create_hypertable`, `add_dimension`, `add_retention_policy`, `add_continuous_aggregate_policy`, `remove_retention_policy`, `remove_continuous_aggregate_policy`, `add_job`, `delete_job`, and `alter_job` — they are functions returning a job_id/regclass (the existing `Hypertable` row already emits `SELECT create_hypertable(...)`).
- The same `ProvisionSql` must carry `CALL <proc>(...)`, never `SELECT`, for `add_columnstore_policy`, `remove_columnstore_policy`, `convert_to_columnstore`, `convert_to_rowstore`, `run_job`, and `refresh_continuous_aggregate` — they are procedures, so a `ColumnstorePolicy` row emitting `SELECT add_columnstore_policy(...)` is a faulted spelling. A `refresh_continuous_aggregate` CALL cannot run inside an explicit migration transaction block, so it is a `MigrationBuilder.Sql(..., suppressTransaction: true)` arm or a post-migration step, never a member of the in-transaction provisioning batch.
- Named-argument `=>` syntax is used for every optional parameter so a server default-shift never silently rebinds a positional argument; `if_not_exists`/`if_exists` make every provisioning step idempotent under re-run.
- The columnstore enable (`ALTER TABLE ... SET (timescaledb.enable_columnstore=true, segmentby, orderby)`) precedes `add_columnstore_policy` — the policy schedules compression of already-columnstore-enabled chunks; `segmentby` is the equality-filter column family of the analytical lane and `orderby` is the `Physical` time column.

[PROVISIONING_LAW]:
- preload: `timescaledb` leads the `Store/provisioning#SERVER_EXTENSIONS` `Preload` row's `shared_preload_libraries` value (`timescaledb,pg_search,pg_partman_bgw,pg_squeeze,pgaudit,pg_cron,pg_net`); the bgworker scheduler launcher requires it, so the extension is NOT index-AM-registered like `pgvectorscale` and is correctly absent from a self-provisioned `CREATE EXTENSION` annotation — the server tier preloads it before the migration runs, and the `ClusterConfig` probe verifies the value read-only against `pg_settings` after boot.
- bgworker ownership: refresh, retention, and columnstore jobs run on TimescaleDB's own bgworker scheduler, so the AppHost schedule port (`ScheduleEntry`) never schedules a database-internal maintenance job — the `timescaledb_information.job_stats` view is the receipt the provisioning verification fold reads for refresh-lag/drop-count/compression-ratio proof rows, and a non-firing job surfaces as a stale `last_successful_finish` the receipt flags.
- analytical residence: the rollup hypertable feeds `Query/columnar#COLUMNAR_LANE` where DuckDB reads the columnstore-compressed chunks; the continuous aggregate is the pre-bucketed tile source the dashboard reads without re-scanning raw chunks.

[RAIL_LAW]:
- Package: `timescaledb`
- Owns: hypertable partitioning, continuous-aggregate materialisation, retention, and columnstore policies plus their bgworker schedulers, over the `OpLogEntry` rollup
- Accept: `MigrationBuilder.Sql` SELECT/CALL emission per the function/procedure split, idempotent `if_not_exists` steps, `job_stats`-backed provisioning receipts
- Reject: a managed EF translator for these functions, a self-provisioned `CREATE EXTENSION` (preload-gated), an AppHost-scheduled database maintenance job, a per-policy positional-argument emission
