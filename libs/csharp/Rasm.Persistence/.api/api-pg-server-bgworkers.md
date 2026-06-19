# [RASM_PERSISTENCE_API_PG_SERVER_BGWORKERS]

The preload-gated PG18 server-tier maintenance companions that ride the `ClusterConfig`
`shared_preload_libraries` row beside `timescaledb`/`pg_search`: `pg_cron` (database-local cron),
`pg_partman` (declarative range/list partition maintenance), `pg_squeeze` (lock-light bloat
reclamation), `pg_jsonschema` (server-side JSON Schema CHECK validation), and `pgaudit` (session/object
audit logging). Each carries no managed assembly: every surface is server-side SQL consumed by
`Store/server#CLUSTER_CONFIG`, `Query/lanes#DOCUMENT_LANE`, `Version/retention#AUDIT_BINDING`,
and the maintenance schedule rows. `pg_jsonschema` and `pgaudit` register through type/preload rather
than a self-provisioned `CREATE EXTENSION` annotation on the hot path.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pg_cron` / `pg_partman` / `pg_squeeze` / `pg_jsonschema` / `pgaudit`
- package: server-side PostgreSQL extensions (not NuGet packages)
- namespace: SQL (`cron.*`, `partman.*`, `squeeze.*`, `json_matches_schema`/`jsonb_matches_schema`, `pgaudit.*` GUCs)
- asset: server extensions; `pg_cron`/`pg_partman`/`pg_squeeze`/`pgaudit` preload-gated, `pg_jsonschema` type-registered
- rail: cluster-config, document-lane, audit-binding, schedule

## [02]-[PG_CRON]

Database-local cron for SQL maintenance jobs requiring server-side cadence, never a duplicate of
TimescaleDB policy scheduling.

| [INDEX] | [FUNCTION]                          | [SIGNATURE]                                             | [SEMANTICS]                     |
| :-----: | :---------------------------------- | :------------------------------------------------------ | :------------------------------ |
|  [01]   | `cron.schedule`                     | `cron.schedule('name', '* * * * *', 'SQL')`             | register a recurring SQL job    |
|  [02]   | `cron.schedule_in_database`         | `cron.schedule_in_database('name', sched, 'SQL', 'db')` | job scoped to a target database |
|  [03]   | `cron.unschedule`                   | `cron.unschedule('name')`                               | remove a job by name            |
|  [04]   | `cron.job` / `cron.job_run_details` | views                                                   | configured jobs and run history |

## [03]-[PG_PARTMAN]

Declarative range/list partition maintenance over a parent table, run by the `pg_partman_bgw`
background worker.

| [INDEX] | [SURFACE]                 | [SIGNATURE]                                                                           | [SEMANTICS]                        |
| :-----: | :------------------------ | :------------------------------------------------------------------------------------ | :--------------------------------- |
|  [01]   | `partman.create_parent`   | `partman.create_parent(p_parent_table => 't', p_control => 'col', p_interval => 'i')` | declare a partition set            |
|  [02]   | `partman.run_maintenance` | `partman.run_maintenance(p_parent_table => 't')`                                      | create future / drop expired parts |
|  [03]   | `partman.part_config`     | table                                                                                 | per-parent retention and premake   |

## [04]-[PG_SQUEEZE]

Lock-light table-bloat reclamation — rewrites a bloated table with only a brief lock at swap, the
online alternative to `VACUUM FULL`.

| [INDEX] | [SURFACE]               | [SIGNATURE]                                | [SEMANTICS]                         |
| :-----: | :---------------------- | :----------------------------------------- | :---------------------------------- |
|  [01]   | `squeeze.squeeze_table` | `squeeze.squeeze_table('schema', 'table')` | online rewrite to reclaim bloat     |
|  [02]   | `squeeze.start_worker`  | `squeeze.start_worker()`                   | start the scheduling bgworker       |
|  [03]   | `squeeze.tables`        | table                                      | scheduled-table registry and policy |

## [05]-[PG_JSONSCHEMA_PGAUDIT]

Server-side JSON Schema validation and session/object audit logging.

| [INDEX] | [SURFACE]                   | [SIGNATURE]                                   | [SEMANTICS]                          |
| :-----: | :-------------------------- | :-------------------------------------------- | :----------------------------------- |
|  [01]   | `jsonb_matches_schema`      | `jsonb_matches_schema('<schema>'::json, doc)` | validate a `jsonb` document; `CHECK` |
|  [02]   | `json_matches_schema`       | `json_matches_schema('<schema>'::json, doc)`  | validate a `json` document; `CHECK`  |
|  [03]   | `pgaudit.log` (GUC)         | `SET pgaudit.log = 'read, write, ddl, role'`  | session-audit class selection        |
|  [04]   | `pgaudit.log_catalog` (GUC) | `SET pgaudit.log_catalog = off`               | exclude catalog statements           |
|  [05]   | `pg_settings`               | view                                          | read-only GUC verification probe     |
