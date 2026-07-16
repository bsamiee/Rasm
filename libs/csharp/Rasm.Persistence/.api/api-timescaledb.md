# [RASM_PERSISTENCE_API_TIMESCALEDB]

`TimescaleDB` supplies the hypertable, continuous-aggregate, retention, and columnstore (hypercore)
provisioning surface over the series tables, plus its native bgworker policy scheduler so
the AppHost schedule port never schedules a refresh/retention/compression job. It carries no managed
assembly and no first-party EF translator: every surface is server-side SQL the
`Query/columnar#SERIES_AND_SCALEOUT` `SeriesLane.Provision` derivation emits per `SeriesKind` row,
riding `MigrationBuilder.Sql` behind the `Store/provisioning#SERVER_EXTENSIONS` extension admission —
and the rollup views feed the analytical reads and the dashboard tiles. The extension is
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

The time column is the sample instant on each `SeriesKind` table (`assessment_series`/`sensor_series`,
`Query/columnar#SERIES_AND_SCALEOUT`). One time dimension is the
partition key; secondary `by_hash` dimensions are held under a provisioning probe, not catalogued.

## [02]-[HYPERTABLE]

`create_hypertable`/`add_dimension` are SELECT functions taking a dimension-builder argument
(`by_range`/`by_hash`); the modern `CREATE TABLE ... WITH (tsdb.hypertable, tsdb.partition_column=...)`
declarative form is the equivalent the fold may emit instead, but the function form is what
`MigrationBuilder.Sql` carries against an already-`CREATE TABLE`-d rollup. Every call uses `=>`
named args and idempotent `if_not_exists` (`[06]`); `create_hypertable` also carries
`create_default_indexes => TRUE`/`migrate_data => FALSE`, and `add_dimension` a `by_hash('col', n)` hash form.

| [INDEX] | [FUNCTION]          | [CALL]                                                         | [SEMANTICS]                                  |
| :-----: | :------------------ | :------------------------------------------------------------- | :------------------------------------------- |
|  [01]   | `create_hypertable` | `create_hypertable('tbl', by_range('time_col', INTERVAL 'i'))` | time-partition an existing table into chunks |
|  [02]   | `by_range`          | `by_range('time_col', INTERVAL 'i')`                           | range dimension builder (time partition)     |
|  [03]   | `add_dimension`     | `add_dimension('tbl', by_range('col', INTERVAL 'i'))`          | add a secondary range/hash dimension         |

## [03]-[CONTINUOUS_AGGREGATE]

A continuous aggregate is a `MATERIALIZED VIEW WITH (timescaledb.continuous)` over a `time_bucket`
group, refreshed by a bgworker policy (a SELECT function returning the job_id) and refreshable
manually by a CALL procedure. `add_continuous_aggregate_policy` also accepts `buckets_per_batch`,
`max_batches_per_execution`, `refresh_newest_first`, `initial_start`, `timezone`; `=>` named args
and `if_not_exists` idempotency are the `[06]` law, and per-row clauses ride the keyed list below.

| [INDEX] | [SURFACE]                            | [FORM]                                                                                         |
| :-----: | :----------------------------------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | view                                 | `CREATE MATERIALIZED VIEW v WITH (timescaledb.continuous) AS <time_bucket query> WITH NO DATA` |
|  [02]   | `time_bucket`                        | `time_bucket('1 hour', time_col)`                                                              |
|  [03]   | `add_continuous_aggregate_policy`    | `add_continuous_aggregate_policy('v', start_offset, end_offset, schedule_interval)`            |
|  [04]   | `refresh_continuous_aggregate`       | `CALL refresh_continuous_aggregate('v', window_start, window_end, force => FALSE)`             |
|  [05]   | `remove_continuous_aggregate_policy` | `remove_continuous_aggregate_policy('v')`                                                      |

- [02]-[TIME_BUCKET]: the bucketing primitive; `time_bucket_gapfill` fills gapped dashboard tiles.
- [04]-[REFRESH]: manual/backfill refresh procedure — cannot run inside an explicit migration transaction block.

## [04]-[RETENTION_COLUMNSTORE]

The retention and hypercore (columnstore) policies and their bgworker schedulers. The columnstore
enable is an `ALTER` storage-parameter set; the policy adder is a CALL procedure (unlike the SELECT
retention/cagg adders); manual per-chunk conversion is a CALL procedure. Every policy adder also
takes `schedule_interval => INTERVAL 's'`, `initial_start`, `timezone` (plus `drop_created_before`
on retention, `created_before` on columnstore), with `if_not_exists`/`if_exists` idempotency, the
`add_retention_policy` job_id return, the `ALTER MATERIALIZED VIEW` cagg-enable variant, and the
columnstore-enable `segmentby`/`orderby` semantics all the `[06]` law.

| [INDEX] | [FUNCTION]                  | [SIGNATURE]                                                 | [SEMANTICS]                             |
| :-----: | :-------------------------- | :---------------------------------------------------------- | :-------------------------------------- |
|  [01]   | `add_retention_policy`      | `add_retention_policy('tbl', drop_after => 'd')`            | drop chunks older than the bound        |
|  [02]   | `remove_retention_policy`   | `remove_retention_policy('tbl')`                            | remove the retention job                |
|  [03]   | columnstore enable          | `ALTER TABLE tbl SET (enable_columnstore = true, ...)`      | hypercore columnar conversion arm       |
|  [04]   | `add_columnstore_policy`    | `CALL add_columnstore_policy('tbl', after => 'a')`          | schedule compression past the age bound |
|  [05]   | `remove_columnstore_policy` | `CALL remove_columnstore_policy('tbl')`                     | remove the columnstore job              |
|  [06]   | `convert_to_columnstore`    | `CALL convert_to_columnstore('chunk', recompress => FALSE)` | manual per-chunk compress               |
|  [07]   | `convert_to_rowstore`       | `CALL convert_to_rowstore('chunk')`                         | manual per-chunk decompress             |

## [05]-[JOB_STATS_CONTROL]

The job-stats and informational views the provisioning receipt reads for refresh-lag, retention
drop-count, and compression-ratio proof rows, plus the job-control surface that reconfigures a
scheduled policy without dropping it. The view column rosters ride the keyed list below.

| [INDEX] | [VIEW]                                          | [PROVIDES]                                            |
| :-----: | :---------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `timescaledb_information.jobs`                  | configured policy jobs                                |
|  [02]   | `timescaledb_information.job_stats`             | per-job run-history stats                             |
|  [03]   | `timescaledb_information.job_errors`            | per-failure error log                                 |
|  [04]   | `timescaledb_information.continuous_aggregates` | cagg definition and refresh/finalized state           |
|  [05]   | `hypertable_columnstore_stats('tbl')`           | per-hypertable compression bytes, ratio, chunk counts |
|  [06]   | `chunk_columnstore_stats('tbl')`                | per-chunk compression detail for a targeted proof row |

- [01]-[JOBS]: `job_id`, `proc_name`, `schedule_interval`, `config`, `hypertable_name`.
- [02]-[JOB_STATS]: `job_id`, `last_run_started_at`, `last_successful_finish`, `last_run_status`, `job_status`, `last_run_duration`, `next_start`, `total_runs`, `total_successes`, `total_failures`.
- [03]-[JOB_ERRORS]: `job_id`, `proc_name`, `err_message`, `start_time`.
- [04]-[CONTINUOUS_AGGREGATES]: cagg definition, `materialization_hypertable_name`, refresh/finalized state.

The job-control surface reconfigures a scheduled policy in place; `alter_job`/`delete_job`/`add_job`
are SELECT functions and `run_job` is a CALL procedure (the receipt's manual-fire remediation arm).

| [INDEX] | [FUNCTION]   | [VERB] | [SEMANTICS]                                                                           |
| :-----: | :----------- | :----- | :------------------------------------------------------------------------------------ |
|  [01]   | `alter_job`  | SELECT | reconfigure a scheduled job: `schedule_interval`, `scheduled`, `config`, `next_start` |
|  [02]   | `delete_job` | SELECT | `delete_job(job_id)` removes a job                                                    |
|  [03]   | `run_job`    | CALL   | `run_job(job_id)` manual fire — the receipt remediation arm                           |
|  [04]   | `add_job`    | SELECT | `add_job(proc, schedule_interval, ...)` registers a user-defined-action job           |

## [06]-[IMPLEMENTATION_LAW]

[EMISSION_LAW]:
- The `Query/columnar#SERIES_AND_SCALEOUT` `SeriesLane.Provision` rows must carry `SELECT <fn>(...)` for `create_hypertable`, `add_dimension`, `add_retention_policy`, `add_continuous_aggregate_policy`, `remove_retention_policy`, `remove_continuous_aggregate_policy`, `add_job`, `delete_job`, and `alter_job` — they are functions returning a job_id/regclass.
- The same derivation must carry `CALL <proc>(...)`, never `SELECT`, for `add_columnstore_policy`, `remove_columnstore_policy`, `convert_to_columnstore`, `convert_to_rowstore`, `run_job`, and `refresh_continuous_aggregate` — they are procedures, so a row emitting `SELECT add_columnstore_policy(...)` is a faulted spelling. A `refresh_continuous_aggregate` CALL cannot run inside an explicit migration transaction block, so it is a `MigrationBuilder.Sql(..., suppressTransaction: true)` arm or a post-migration step, never a member of the in-transaction provisioning batch.
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
