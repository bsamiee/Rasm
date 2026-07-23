# [RASM_PERSISTENCE_API_TIMESCALEDB]

`timescaledb` owns the temporal partitioning tier over the Persistence series tables — hypertable chunking, continuous-aggregate materialisation, retention, and columnstore conversion — each policy running on the extension's own bgworker scheduler. Every surface is server-side SQL carrying no managed assembly and no EF translator, so a fence emits it as migration text and reads its outcome from the information views. Rollup relations it mints feed the analytical lane's read.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `timescaledb`
- package: `timescaledb` (Apache-2.0)
- namespace: SQL — `public` functions and procedures, `timescaledb_information.*` views, `timescaledb.*` storage parameters
- asset: server extension whose bgworker scheduler launcher requires its `shared_preload_libraries` entry
- rail: timescale-provisioning, analytical-lane

## [02]-[HYPERTABLE]

[HYPERTABLE_ENTRY_SCOPE]: one-time-dimension chunking and the exclusion metadata a read prunes on

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :----------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `CREATE TABLE t WITH (tsdb.hypertable, tsdb.partition_column='c')` | ddl      | declare a hypertable at creation        |
|  [02]   | `create_hypertable('t', by_range('c', INTERVAL 'i'))`              | function | partition an existing table into chunks |
|  [03]   | `by_range('c', INTERVAL 'i')`                                      | function | range dimension builder for time        |
|  [04]   | `by_hash('c', n)`                                                  | function | hash dimension builder for space        |
|  [05]   | `add_dimension('t', by_hash('c', n))`                              | function | add a secondary dimension               |
|  [06]   | `enable_chunk_skipping('t', 'c')`                                  | function | min/max chunk exclusion on a column     |

- `create_hypertable` also carries `create_default_indexes` and `migrate_data`.

## [03]-[CONTINUOUS_AGGREGATE]

[CONTINUOUS_AGGREGATE_ENTRY_SCOPE]: pre-bucketed rollup a dashboard tile reads without re-scanning raw chunks

`add_continuous_aggregate_policy` also carries `initial_start`, `timezone`, `buckets_per_batch`, `max_batches_per_execution`, and `refresh_newest_first`.

| [INDEX] | [SURFACE]                                                                         | [SHAPE]   | [CAPABILITY]                    |
| :-----: | :-------------------------------------------------------------------------------- | :-------- | :------------------------------ |
|  [01]   | `CREATE MATERIALIZED VIEW v WITH (timescaledb.continuous) AS q WITH NO DATA`      | ddl       | declare the materialised rollup |
|  [02]   | `time_bucket(INTERVAL, c)`                                                        | function  | fixed-width bucket on time      |
|  [03]   | `time_bucket_gapfill(INTERVAL, c, start, finish)`                                 | function  | bucket emitting empty buckets   |
|  [04]   | `locf(value)`                                                                     | function  | carry last observation forward  |
|  [05]   | `interpolate(value)`                                                              | function  | linear fill across a gap        |
|  [06]   | `add_continuous_aggregate_policy(v, start_offset, end_offset, schedule_interval)` | function  | schedule the refresh job        |
|  [07]   | `refresh_continuous_aggregate(v, window_start, window_end, force => FALSE)`       | procedure | manual or backfill refresh      |
|  [08]   | `remove_continuous_aggregate_policy(v)`                                           | function  | drop the refresh job            |

## [04]-[COLUMNSTORE_RETENTION]

[COLUMNSTORE_RETENTION_ENTRY_SCOPE]: columnar conversion and chunk expiry, scheduled as policies or fired per chunk

Every policy adder carries `schedule_interval`, `initial_start`, and `timezone`; `add_retention_policy` adds `drop_created_before` and `add_columnstore_policy` adds `created_before`.

| [INDEX] | [SURFACE]                                                               | [SHAPE]   | [CAPABILITY]                            |
| :-----: | :---------------------------------------------------------------------- | :-------- | :-------------------------------------- |
|  [01]   | `ALTER TABLE t SET (timescaledb.enable_columnstore = true)`             | ddl       | arm columnar conversion on a hypertable |
|  [02]   | `ALTER MATERIALIZED VIEW v SET (timescaledb.enable_columnstore = true)` | ddl       | arm columnar conversion on a rollup     |
|  [03]   | `add_columnstore_policy('t', after => 'a')`                             | procedure | schedule compression past an age bound  |
|  [04]   | `remove_columnstore_policy('t')`                                        | procedure | drop the columnstore job                |
|  [05]   | `convert_to_columnstore('chunk', recompress => FALSE)`                  | procedure | compress one chunk by hand              |
|  [06]   | `convert_to_rowstore('chunk')`                                          | procedure | decompress one chunk by hand            |
|  [07]   | `show_chunks('t', older_than => 'd')`                                   | function  | select chunks by age                    |
|  [08]   | `add_retention_policy('t', drop_after => 'd')`                          | function  | schedule the chunk drop                 |
|  [09]   | `remove_retention_policy('t')`                                          | function  | drop the retention job                  |
|  [10]   | `drop_chunks('t', older_than => 'd')`                                   | function  | drop chunks by hand                     |

- `timescaledb.enable_columnstore`: one `SET` also carries `timescaledb.segmentby`, the analytical equality-filter column family, and `timescaledb.orderby`, the time column.

## [05]-[JOBS_AND_STATS]

[JOBS_AND_STATS_ENTRY_SCOPE]: policy-job registry, its run-history receipts, and the in-place job controls

| [INDEX] | [SURFACE]                                                             | [SHAPE]   | [CAPABILITY]                         |
| :-----: | :-------------------------------------------------------------------- | :-------- | :----------------------------------- |
|  [01]   | `timescaledb_information.jobs`                                        | view      | configured policy-job registry       |
|  [02]   | `timescaledb_information.job_stats`                                   | view      | per-job run history and next start   |
|  [03]   | `timescaledb_information.job_errors`                                  | view      | `err_message` per failure            |
|  [04]   | `timescaledb_information.continuous_aggregates`                       | view      | rollup definition and refresh state  |
|  [05]   | `timescaledb_information.hypertable_columnstore_settings`             | view      | landed `segmentby` and `orderby`     |
|  [06]   | `hypertable_columnstore_stats('t')`                                   | function  | chunk counts and compression bytes   |
|  [07]   | `chunk_columnstore_stats('t')`                                        | function  | compression bytes per chunk          |
|  [08]   | `alter_job(job_id, schedule_interval, scheduled, config, next_start)` | function  | reconfigure a scheduled job in place |
|  [09]   | `delete_job(job_id)`                                                  | function  | remove a job                         |
|  [10]   | `run_job(job_id)`                                                     | procedure | fire a job by hand                   |
|  [11]   | `add_job(proc, schedule_interval, config)`                            | function  | register a user-defined-action job   |

- `jobs` keys every control by `job_id`, the value each policy adder returns.
- `job_stats` carries `last_successful_finish` and `last_run_status` as the receipt's refresh-lag and failure evidence, and `run_job` re-fires a stalled policy.

## [06]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `MigrationBuilder.Sql` emits every surface, and the `[SHAPE]` verb is the emission verb — a `SELECT` over a procedure or a `CALL` over a function faults at execution.
- Named `=>` arguments bind every optional parameter and `if_not_exists`/`if_exists` gates every step, so a server default shift never rebinds positionally and a re-run never faults.
- `refresh_continuous_aggregate` holds its own transaction control, so it rides `MigrationBuilder.Sql(..., suppressTransaction: true)` or a post-migration step.
- Columnstore enable precedes `add_columnstore_policy`, which compresses chunks of an already-armed relation.

[STACKING]:
- `timescaledb_toolkit`(`.api/api-timescaledb-toolkit.md`): `time_weight`/`average`/`rollup` fold raw chunks into the time-weighted read a bucketed rollup cannot answer.
- `duckdb`(`.api/api-duckdb.md`): `ATTACH (TYPE postgres, READ_ONLY)` and `postgres_scan` read the continuous-aggregate relation as a columnar join leg against live PG.
- `pg_cron`(`.api/api-pg-server-bgworkers.md`): `cron.schedule` owns in-database cadence outside this extension, while refresh, retention, and columnstore jobs stay on the TimescaleDB scheduler and the AppHost `ScheduleEntry` port schedules no database-internal maintenance.
- `Query/columnar#SERIES_AND_SCALEOUT`: one `SeriesKind` row derives the whole ordered provisioning set through `SeriesLane.Provision`, and `SeriesLane.Verify` joins `jobs` against `job_stats` into one `JobHealth` row per policy.

[LOCAL_ADMISSION]:
- `Store/provisioning#SERVER_EXTENSIONS` carries the `Preload` admission row; the extension enters through the operator's preload configuration and its absence is a typed repair artifact.
- Policy scheduling, continuous aggregates, and columnstore run under TSL at the database deployment and link into no managed assembly, so the server process is the license boundary.

[RAIL_LAW]:
- Package: `timescaledb`
- Owns: chunked time partitioning, continuous-aggregate materialisation, retention, and columnstore conversion over the `SeriesKind` series tables, each policy on its own bgworker
- Accept: derived `MigrationBuilder.Sql` rows carrying the shape-correct verb, named idempotent arguments, and `job_stats`-backed provisioning receipts
- Reject: a managed EF translator over these functions, a hand-spelled per-environment policy script, an AppHost-scheduled database-internal job, a positional-argument emission
