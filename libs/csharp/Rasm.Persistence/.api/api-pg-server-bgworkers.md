# [RASM_PERSISTENCE_API_PG_SERVER_BGWORKERS]

The PG18 server-tier maintenance companions consumed as server-side SQL across the Persistence
PostgreSQL profile — `pg_cron` (database-local cron), `pg_partman` (declarative range/list partition
maintenance), `pg_squeeze` (lock-light bloat reclamation), `pg_jsonschema` (server-side JSON Schema
CHECK validation), and `pgaudit` (session/object audit logging). They carry no managed assembly:
every surface is server-side SQL the `Store/provisioning#SERVER_EXTENSIONS` `ClusterConfig` verifies,
the `Store/provisioning#SERVER_EXTENSIONS` `ServerExtension` `CreateSql` install SQL provides, and the `Query/lanes#DOCUMENT_LANE`,
`Version/retention#AUDIT_BINDING`, and maintenance-schedule rows consume.

PRELOAD SPLIT (verified against `ClusterConfig.Rows`): the `shared_preload_libraries` value is
`timescaledb,pg_search,pg_partman_bgw,pg_squeeze,pgaudit,pg_cron,pg_net` — so `pg_cron`, `pg_partman`
(as its `pg_partman_bgw` background-worker library), `pg_squeeze`, and `pgaudit` ARE preload-gated
and verify through the `Store/profiles#PROVISIONING_ROWS` `PreloadProbe`; only `pg_jsonschema` is
NOT preloaded — it registers its `json_matches_schema`/`jsonb_matches_schema` functions via
`CREATE EXTENSION` and carries a `Json.Schema.JsonSchema.Evaluate` (JsonSchema.Net) in-process
fallback when the deploy image lacks the pgrx-compiled extension. The
rostered `pg_net` is preload-gated too — its `libcurl` worker is statically `RegisterBackgroundWorker`'d
in `_PG_init`, so it rides the same `shared_preload_libraries` value (hard-erroring on `CREATE EXTENSION`
without it); its full SQL surface is catalogued in `api-pg-net.md`, not here.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pg_cron` / `pg_partman` / `pg_squeeze` / `pg_jsonschema` / `pgaudit`
- package: server-side PostgreSQL extensions (not NuGet packages); installed in the deploy-image PG18, never linked into managed code
- namespace: SQL (`cron.*`, `partman.*`, `squeeze.*`, `json_matches_schema`/`jsonb_matches_schema`, `pgaudit.*` GUCs)
- preload (in `shared_preload_libraries`): `pg_cron`, `pg_partman_bgw`, `pg_squeeze`, `pgaudit` — verified read-only via `ClusterConfig.Verify` after boot
- type/function-registered (NOT preloaded): `pg_jsonschema` — `CREATE EXTENSION`, `Fallback: "Json.Schema.JsonSchema.Evaluate"` on its `ServerExtension` row
- license: `pg_cron`/`pg_partman`/`pgaudit` PostgreSQL/MIT-class; `pg_squeeze` BSD; `pg_jsonschema` Apache-2.0 — the in-DB deployment is the license boundary, no managed linkage
- consumed by: `ExtensionRequirement.OperatorProvisioned` rows (`Store/profiles`), `ServerExtension` `CreateSql`/`Extension` (`Store/provisioning#SERVER_EXTENSIONS`), `Version/retention#AUDIT_BINDING`, the AppHost persistence-maintenance schedule
- rail: cluster-config, document-lane, audit-binding, schedule

## [02]-[PG_CRON]

Database-local cron for SQL maintenance jobs requiring server-side cadence, never a duplicate of
TimescaleDB policy scheduling (continuous-aggregate/retention/columnstore cadence rides the native
`add_*_policy` bgworkers) and never a duplicate of the AppHost schedule port (which owns the
process-side `persistence-maintenance` cadence). Preloaded as `pg_cron`; `cron.schedule` supports
both 5-field cron and PG18 interval syntax (`'30 seconds'`).

| [INDEX] | [FUNCTION]                          | [SIGNATURE]                                             | [SEMANTICS]                     |
| :-----: | :---------------------------------- | :------------------------------------------------------ | :------------------------------ |
|  [01]   | `cron.schedule`                     | `cron.schedule('name', '* * * * *', 'SQL')`             | register a recurring SQL job; returns `bigint` jobid |
|  [02]   | `cron.schedule_in_database`         | `cron.schedule_in_database('name', sched, 'SQL', 'db'[, 'user'])` | job scoped to a target database/role |
|  [03]   | `cron.unschedule`                   | `cron.unschedule('name')` / `cron.unschedule(jobid)`    | remove a job by name or id      |
|  [04]   | `cron.alter_job`                    | `cron.alter_job(jobid, schedule:=…, active:=…)`         | mutate cadence / pause a job    |
|  [05]   | `cron.job` / `cron.job_run_details` | views                                                   | configured jobs and run history (status, return_message, end_time) |

Consumer: the page schedules in-DB maintenance through `cron.schedule_in_database` only where the
cadence must be server-local (a job the process must not own); the `cron.job_run_details` view feeds
a maintenance-receipt fact. A job that the AppHost schedule port already owns is the rejected form.

## [03]-[PG_PARTMAN]

Declarative range/list partition maintenance over a PG18-native partitioned parent, run by the
`pg_partman_bgw` background worker (the preload-library spelling; the extension installs as
`pg_partman`). Owns time/serial partition lifecycle for the `OpLogEntry`-rollup and audit-history
tables; consumed at `Version/retention` where retention is partition drop, not row `DELETE`.

| [INDEX] | [SURFACE]                 | [SIGNATURE]                                                                           | [SEMANTICS]                        |
| :-----: | :------------------------ | :------------------------------------------------------------------------------------ | :--------------------------------- |
|  [01]   | `partman.create_parent`   | `partman.create_parent(p_parent_table := 't', p_control := 'col', p_interval := 'i'[, p_type := 'range'])` | declare a partition set (named params) |
|  [02]   | `partman.run_maintenance` | `partman.run_maintenance([p_parent_table := 't'])` / `partman.run_maintenance_proc()` | create future / drop expired parts; the proc is the `pg_cron`-callable form |
|  [03]   | `partman.part_config`     | table                                                                                 | per-parent `premake`, `retention`, `retention_keep_table`, `infinite_time_partitions` policy |
|  [04]   | `partman.partition_data_proc` | `partman.partition_data_proc(p_parent_table := 't')`                              | back-fills existing rows into the new partition set |

Consumer: a partitioned history table declares one `create_parent` call in a migration; the
`pg_partman_bgw` worker (or a `cron.schedule`d `run_maintenance_proc`) rolls partitions, and
`part_config.retention` is the declarative drop policy the `Version/retention` destructive-gate
honors. A hand-rolled partition-rotation job is the rejected form.

## [04]-[PG_SQUEEZE]

Lock-light table-bloat reclamation — rewrites a bloated table with only a brief `ACCESS EXCLUSIVE`
lock at swap, the in-DB online alternative to `VACUUM FULL` and to an out-of-DB `pg_repack` client.
Preloaded as `pg_squeeze`; consumed at `Store/profiles#PROVISIONING_ROWS` where a scheduled reclaim
runs as a pure in-DB bgworker registered through `squeeze.tables`, never an external binary.

| [INDEX] | [SURFACE]                | [SIGNATURE]                                              | [SEMANTICS]                         |
| :-----: | :----------------------- | :------------------------------------------------------- | :---------------------------------- |
|  [01]   | `squeeze.squeeze_table`  | `squeeze.squeeze_table('schema', 'table'[, 'index', 'tablespace'])` | one-shot online rewrite to reclaim bloat |
|  [02]   | `squeeze.start_worker`   | `squeeze.start_worker()`                                 | start the per-database scheduling bgworker |
|  [03]   | `squeeze.tables`         | table                                                    | scheduled-table registry: `schedule` (cron), `free_space_extent_threshold`, `vacuum_max_age` policy columns |
|  [04]   | `squeeze.squeeze_table_recent` / `squeeze.log` | views                                  | last-run state and the per-run reclaim log (receipt source) |

Consumer: a table that needs scheduled reclaim gets one `INSERT INTO squeeze.tables (..., schedule)`
row; the bgworker reads the `schedule` cron column and rewrites on cadence. A scheduled
`REINDEX`-style reclaim that spawns an out-of-DB process is the rejected form.

## [05]-[PG_JSONSCHEMA]

Server-side JSON Schema validation. NOT preloaded — `CREATE EXTENSION pg_jsonschema` registers the
`json_matches_schema`/`jsonb_matches_schema` boolean functions used inside a column `CHECK`.

| [INDEX] | [SURFACE]                 | [SIGNATURE]                                   | [SEMANTICS]                          |
| :-----: | :------------------------ | :-------------------------------------------- | :----------------------------------- |
|  [01]   | `jsonb_matches_schema`    | `jsonb_matches_schema('<schema>'::json, doc)` | validate a `jsonb` document; → `bool` for `CHECK` |
|  [02]   | `json_matches_schema`     | `json_matches_schema('<schema>'::json, doc)`  | validate a `json` document; → `bool` for `CHECK`  |
|  [03]   | `jsonschema_is_valid`     | `jsonschema_is_valid('<schema>'::json)`       | validate that the schema itself is well-formed |

Consumer + fallback stack: the document lane declares one `ServerExtension` `CreateSql` over `(Table, Column,
Constraint, Schema)` (`Store/provisioning#SERVER_EXTENSIONS`) whose `Sql` emits `ALTER TABLE … ADD CONSTRAINT …
CHECK (jsonb_matches_schema('<schema>', <column>))`, so a declared-shape document is rejected at
WRITE (`Query/lanes#DOCUMENT_LANE`). Because this is the one non-preloaded companion, the
`ServerExtension("pg_jsonschema", Fallback: "Json.Schema.JsonSchema.Evaluate")` row degrades the
SAME validation to in-process `JsonSchema.Net` evaluation when the deploy image lacks the
pgrx-compiled extension — the `Validate` fold moves the check application-side rather than silently
dropping it. The schema string arrives pre-frozen from the document-lane shape, never raw runtime
input. A second loose-DOM validator path is the rejected form.

In-process fallback surface (`JsonSchema.Net`, Apache-2.0): the canonical entry is
`Json.Schema.JsonSchema.FromText(string) -> JsonSchema` to parse the pre-frozen schema once, then
`JsonSchema.Evaluate(JsonNode? instance, EvaluationOptions options) -> EvaluationResults`, reading
`EvaluationResults.IsValid -> bool` for the same boolean verdict the server-side
`jsonb_matches_schema` returns; `EvaluationOptions.OutputFormat = OutputFormat.List` plus
`EvaluationResults.Details` yields the per-keyword failure rail for a richer `Validate` receipt than
the server-side `bool`. ADMISSION GAP: `JsonSchema.Net` is design-page-specified on
`Store/provisioning#SERVER_EXTENSIONS` `Packages:` yet absent from `Directory.Packages.props` and every
csproj/lockfile, so these members resolve UNVERIFIED against the manifest — the fallback path cannot
compile until the central package owner admits `JsonSchema.Net`. The member signatures are the
package's real public surface, not the resolved-artifact reflection this catalog otherwise pins.

## [06]-[PGAUDIT]

Session/object audit logging into the server log. PRELOADED as `pgaudit`; the runtime obligation is
the bound `pgaudit.log` GUC value verified read-only against `pg_settings`. Owned at
`Version/retention#AUDIT_BINDING` where a `DataClassification` binds to one audit category.

| [INDEX] | [SURFACE]                   | [SIGNATURE]                                          | [SEMANTICS]                          |
| :-----: | :-------------------------- | :--------------------------------------------------- | :----------------------------------- |
|  [01]   | `pgaudit.log` (GUC)         | `SET pgaudit.log = 'read, write, ddl, role'`         | session-audit class selection (`read`/`write`/`function`/`role`/`ddl`/`misc`/`all`) |
|  [02]   | `pgaudit.log_catalog` (GUC) | `SET pgaudit.log_catalog = off`                      | exclude `pg_catalog` statements      |
|  [03]   | `pgaudit.log_relation` (GUC)| `SET pgaudit.log_relation = on`                      | one log entry per relation in a statement |
|  [04]   | `pgaudit.log_parameter` (GUC)| `SET pgaudit.log_parameter = on`                    | include bound statement parameters   |
|  [05]   | `pgaudit.role` (GUC)        | `SET pgaudit.role = 'auditor'`                       | object-audit role for `GRANT`-scoped per-object auditing |
|  [06]   | `pg_settings`               | view                                                 | read-only GUC verification probe (`SELECT setting FROM pg_settings WHERE name = ANY(...)`) |

Consumer + stack: the audit binding maps a `DataClassification` to one pgaudit category and binds it
per-tenant through `BindTenant` (`Version/retention#AUDIT_BINDING`); the provisioning verifier folds
the bound `pgaudit.log` value against the `pg_settings` observation and a missing
`shared_preload_libraries=pgaudit` entry folds into the `<server-not-provisioned>` provisioning
fault. Object-level auditing rides `pgaudit.role` + a `GRANT` to that role rather than a per-table
GUC. A client-side audit-log pipeline is the rejected form — execution is server-log-side, the
runtime only verifies the GUC.

## [07]-[IMPLEMENTATION_LAW]

[BGWORKER_TOPOLOGY]:
- Five extensions, two registration modes: four preload-gated (`pg_cron`, `pg_partman_bgw`, `pg_squeeze`, `pgaudit` — verified via `ClusterConfig`/`PreloadProbe`) and one function-registered (`pg_jsonschema` — `CREATE EXTENSION`, with the JsonSchema.Net in-process fallback).
- None carries a managed assembly or a first-party EF translator: every surface lands through the EF `MigrationBuilder.Sql` rail (the `ServerExtension` `CreateSql` install SQL, raw GUC SET for pgaudit) or a `cron`/`squeeze` registry `INSERT`. `CreatePostgresExtensionOperation` is a phantom spelling; preload-gated `CREATE EXTENSION` rides the EF `MigrationBuilder.Sql` rail because `HasPostgresExtension` cannot encode the `shared_preload_libraries` prerequisite.
- Cadence ownership is partitioned and non-overlapping: TimescaleDB native `add_*_policy` bgworkers own continuous-aggregate/retention/columnstore cadence; `pg_partman_bgw` owns partition rotation; `pg_squeeze` owns bloat-reclaim cadence via its `squeeze.tables.schedule` column; `pg_cron` owns only server-local jobs the AppHost schedule port must not own. The process-side `persistence-maintenance` schedule owns `ANALYZE`/`REINDEX`. No two own the same job.

[RAIL_LAW]:
- Packages: `pg_cron` / `pg_partman` / `pg_squeeze` / `pg_jsonschema` / `pgaudit` (server-side, in the deploy-image PG18)
- Owns: server-tier scheduled maintenance, declarative partitioning, online bloat reclaim, server-side document validation, and audit logging — all as server-side SQL the managed code verifies/emits, never links
- Accept: `cron.schedule_in_database` for server-local cadence, `partman.create_parent`/`part_config` for partition lifecycle, `squeeze.tables` registry rows for scheduled reclaim, `jsonb_matches_schema` inside a `ServerExtension` `CreateSql` (with JsonSchema.Net fallback), `pgaudit.log` GUC bound per the audit-binding classification table
- Reject: a hand-rolled partition-rotation or bloat-reclaim job, an out-of-DB `pg_repack`/audit-pipeline client, a runtime `ALTER SYSTEM` to set a preload (preloads are deploy-time `postgresql.conf`, verified not executed), treating `pgaudit` as function-registered rather than preloaded, a second document validator beside `pg_jsonschema`+JsonSchema.Net
