# [WORK]

`work` is the durable-execution folder: cluster entities, durable workflows, job queues, schedules, and signed egress. It is the in-process durable-actor altitude — deployment topology is `iac`, queue-as-data is `store`, and each meets `work` only at a typed seam. `work` composes the `@effect/sql` core `SqlClient` Tag and the `@effect/cluster` `MessageStorage` Tag as ports; the app root provides the `store`-owned driver Layer, so `work` never imports `store`. Cluster runner entrypoint Layers ride `host/exec` runtime rows the app root selects — `work` imports no platform-node/bun binding. The folder domain map and seam record live in `ARCHITECTURE.md`, the forward concept pool in `IDEAS.md`, and the work log in `TASKLOG.md`.

## [1]-[ROUTER]

- [01]-[ENTITY](.planning/engine/entity.md) — cluster Entities, sharding, runner discovery, per-tenant fenced-quota rows.
- [02]-[STORAGE](.planning/engine/storage.md) — MessageStorage composition law over the app-root SqlClient Tag.
- [03]-[DURABLE](.planning/flow/durable.md) — durable workflow definitions and compensation/saga folds.
- [04]-[ACTIVITY](.planning/flow/activity.md) — activity rows with retry/timeout budgets from kernel/fault.
- [05]-[JOB](.planning/queue/job.md) — DurableQueue job families: priority, dedupe/batch keys, DLQ/replay rows.
- [06]-[SCHEDULE](.planning/queue/schedule.md) — ClusterCron schedule vocabulary with misfire/window policy rows.
- [07]-[WEBHOOK](.planning/deliver/webhook.md) — durable signed egress and delivery receipts.
- [08]-[MAIL](.planning/deliver/mail.md) — nodemailer mail egress with locale-keyed template and suppression rows.
- [09]-[REPORT](.planning/deliver/report.md) — document egress rows as durable report jobs.
- [10]-[RELAY](.planning/deliver/relay.md) — outbox relay draining the channel rows under one fan-out policy.

## [2]-[DOMAIN_PACKAGES]

Every durable-execution library `work` uses, planned or implemented. Versions are centralized in the one `pnpm-workspace.yaml` catalog and never pinned here; API evidence lives in the adjacent `.api/` folder. New admissions land here from the folder's ideas and tasks.

[DURABLE_EXECUTION]:
- `@effect/cluster` — cluster Entities, sharding, runner discovery, and `MessageStorage`; the in-process durable-actor engine.
- `@effect/workflow` — durable workflow and activity definitions with compensation/saga folds.

[MAIL_EGRESS]:
- `nodemailer` (+ `@types/nodemailer`) — the one mail-egress owner behind the `deliver/mail` durable job.

[DOCUMENT_EGRESS]:
- `exceljs` — spreadsheet report egress.
- `jspdf` — PDF report egress.
- `jszip` — archive bundling for multi-artifact report jobs.
- `papaparse` (+ `@types/papaparse`) — CSV parse/serialize for tabular egress.

## [3]-[SUBSTRATE_PACKAGES]

Cross-cutting TypeScript substrate `work` composes; package charters live in `libs/typescript/.planning/README.md` `[02]` and shared API evidence lives in `libs/typescript/.api/`.

[RAILS]:
- `effect`

[PLATFORM]:
- `@effect/platform`
- `@effect/experimental`

[PORT_VOCABULARY]:
- `@effect/sql` — core only; the `SqlClient` Tag `work` composes as a port, the `store`-owned driver Layer satisfies at the app root. The `-pg`/`-sqlite-*` drivers are banned outside `store`.
