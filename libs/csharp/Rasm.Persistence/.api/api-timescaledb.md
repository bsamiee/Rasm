# [RASM_PERSISTENCE_API_TIMESCALEDB]

`TimescaleDB` supplies the hypertable, continuous-aggregate, retention, and columnstore (hypercore)
provisioning surface over the `OpLogEntry`-rollup table, plus its native bgworker policy scheduler so
the AppHost schedule port never schedules a refresh/retention/compression job. It carries no managed
assembly and no first-party EF translator: every surface is server-side SQL the
`Store/server#TIMESCALE_PROVISIONING` `TimescaleStep` fold emits through `MigrationBuilder.Sql`, and
the rollups feed `Query/lanes#ANALYTICAL_LANE` and the dashboard tiles. The extension is preload-gated
(it rides the `ClusterConfig` `shared_preload_libraries` row), never a `Schema/ddl#EXTENSION_DDL`
self-provisioned `CREATE EXTENSION` annotation.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `timescaledb`
- package: `timescaledb` (server-side PostgreSQL extension, not a NuGet package)
- namespace: SQL (public function set; `timescaledb.*` storage parameters)
- asset: server extension, preload-gated (`shared_preload_libraries`), bgworker scheduler
- rail: timescale-provisioning, analytical-lane

## [2]-[HYPERTABLE]

| [INDEX] | [FUNCTION]          | [SIGNATURE]                                                                           | [SEMANTICS]                                       |
| :-----: | :------------------ | :------------------------------------------------------------------------------------ | :------------------------------------------------ |
|   [1]   | `create_hypertable` | `create_hypertable('tbl', by_range('time_col', INTERVAL 'i'), if_not_exists => TRUE)` | partition a table into chunks by a time dimension |
|   [2]   | `add_dimension`     | `add_dimension('tbl', by_range('col', INTERVAL 'i'))`                                 | add a secondary partition dimension               |

The time column is the HLC `Physical` instant on the rollup table; the rollup mirrors the
`OpLogEntry` columns the `DuckDBOpLogMap` projects on `Query/lanes#ANALYTICAL_LANE`.

## [3]-[CONTINUOUS_AGGREGATE]

A continuous aggregate is a `MATERIALIZED VIEW WITH (timescaledb.continuous)` over a `time_bucket`
group, plus its bgworker refresh policy as one DDL pair.

| [INDEX] | [SURFACE]                         | [FORM]                                                                                                                                  |
| :-----: | :-------------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | view                              | `CREATE MATERIALIZED VIEW v WITH (timescaledb.continuous) AS SELECT time_bucket('b', t) AS bucket, ... GROUP BY 1 WITH NO DATA`         |
|   [2]   | `time_bucket`                     | `time_bucket('1 hour', time_col)`                                                                                                       |
|   [3]   | `add_continuous_aggregate_policy` | `add_continuous_aggregate_policy('v', start_offset => INTERVAL 's', end_offset => INTERVAL 'e', schedule_interval => INTERVAL 'sched')` |

## [4]-[RETENTION_COLUMNSTORE]

The retention and hypercore (columnstore) policies and their bgworker schedulers.

| [INDEX] | [FUNCTION]               | [SIGNATURE]                                                                                                           | [SEMANTICS]                        |
| :-----: | :----------------------- | :-------------------------------------------------------------------------------------------------------------------- | :--------------------------------- |
|   [1]   | `add_retention_policy`   | `add_retention_policy('tbl', drop_after => INTERVAL 'd')`                                                             | drop chunks older than the bound   |
|   [2]   | columnstore enable       | `ALTER TABLE tbl SET (timescaledb.enable_columnstore = true, timescaledb.segmentby = 's', timescaledb.orderby = 'o')` | hypercore columnar conversion arm  |
|   [3]   | `add_columnstore_policy` | `add_columnstore_policy('tbl', after => INTERVAL 'a')`                                                                | compress chunks past the age bound |

## [5]-[JOB_STATS]

The job-stats and informational views the provisioning receipt reads for refresh-lag, retention
drop-count, and compression-ratio proof rows.

| [INDEX] | [VIEW]                                          | [SEMANTICS]                                           |
| :-----: | :---------------------------------------------- | :---------------------------------------------------- |
|   [1]   | `timescaledb_information.jobs`                  | configured policy jobs (refresh, retention, compress) |
|   [2]   | `timescaledb_information.job_stats`             | last-run, next-start, success, and run-duration stats |
|   [3]   | `timescaledb_information.continuous_aggregates` | continuous-aggregate definition and refresh state     |
|   [4]   | `hypertable_columnstore_stats`                  | per-hypertable compression ratio and chunk counts     |
