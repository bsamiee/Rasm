# readme — workload-driven backend package verdict

## [WORKLOAD_PROFILE]

TS is **browser web-UI consumer-only**. Zero TS-owned backend: no BFF, job runners, schedulers, email, file processing, webhooks, or realtime fan-out in any plan.

- `libs/typescript/.planning/README.md` L3: "Rasm TypeScript owns the web UI: the co-hosted SPA, the evidence and benchmark dashboards, and the companion control panels. The four .NET packages ship the complete wire surface; TS consumes every contract as settled vocabulary."
- `architecture-posture.md` host topology: one co-hosted origin serves SPA + grpc-web; browser does unary `await` + server-stream `for await`; client-stream/bidi/ArtifactSync structurally absent. **No Node server process exists in the TS tree.** All five TS service owners (`WireClients`, `SnapshotFeed`, `RuntimeFeed`, `CommandGateway`, `EvidenceFeed`) are browser consumers; owner budget closed.
- Stage [2.B] gate: "no import, path mapping, or build edge resolves into `libs/csharp`; integration crosses only the contracts inventoried at wire-consumption."
- Backend is entirely .NET-owned and finalized: AppHost owns the **schedule port (CronExpression, lease-handoff, crash-reclaim)**, **outbound-resilience (one retry owner per remote boundary)**, **drainable queues + pools**, **HybridCache**, support capture, health/degradation (`Rasm.AppHost/.planning/README.md` GAP_LEDGER rows 6,7,11,16). Compute owns `scheduling-and-lanes` (WorkLane). Persistence owns `sync-collaboration` + `redaction-retention`.
- **Reconciliation of catalog intent vs plans:** root `package.json` `devDependencies` consumes only web-UI + tooling `catalog:` rows. The catalog's `@effect/platform-node`, `@effect/sql-pg`/`sql`, `@effect/cluster`, `@effect/workflow`, `@effect/rpc`, `nodemailer`/`@types/nodemailer`, `ioredis`, `@aws-sdk/*`, `@effect-aws/client-s3`, `testcontainers` are **speculative rows with no TS consumer** — not admitted dependencies.

## [BULLMQ_VERDICT]

**REJECT now — premature; no TS jobs workload exists. No contingency admit.**

- (a) Zero production job queues needed on TS. No scheduler/worker/cron/delayed-job/fan-out lands on TS; scheduling is .NET (AppHost schedule port + Compute WorkLane).
- (b)/(c) Moot — bullmq is workload-absent, not complementary or replaceable. A Redis-backed queue presupposes the Node server the plans explicitly exclude.
- Cluster cross-check (for any hypothetical future Node BFF): `@effect/cluster` has **no ClusterQueue primitive yet** (open issue Effect-TS/effect#5329; community consensus: bullmq is still the only production-viable NodeJS queue), and cluster is officially **unstable**. So even if TS later owned jobs, neither option is clean today. Revisit only when a planning doc admits a TS-owned Node server *and* a short-job workload; then weigh bullmq (Redis, mature, `ioredis` already cataloged) vs cluster (SQL spine present, no queue API).

## [CATALOG_ADDS]

**None to admit now.** The consumer-only SPA is fully served by existing wire/UI rows. No backend Effect package surfaced has a current TS consumer; adding one violates the consumer-only profile and the stage [2.A] dependency-usage gate. Research-only conditionals (versions = June-2026 reference, not admissions; pin at stage [2.C] from catalogue truth if a workload lands):

| package | latest | maintained | admit condition |
| :--- | :--- | :--- | :--- |
| `@effect-atom/atom` | 0.5.3 | yes | only if SPA adopts Effect-native atom state (atom-react lags 0.5.0/Jan-2026 — pin pair exactly) |
| `@effect-atom/atom-react` | 0.5.0 | yes (borderline) | React sibling of above |
| `@effect/printer`(+`-ansi`) | 0.49.0 | yes | only if a TS CLI/diagnostic-render surface emerges (none planned) |
| `@effect/typeclass` | 0.40.0 | yes | skip — HKT kit, no SPA need |

## [CATALOG_NOTES]

- **effect-aws** (all `1.11.x`, monorepo-synced, ~52 `client-*` mirroring `@aws-sdk/client-*`): `client-sesv2` 1.11.0 (prefer over `client-ses` for new email), `client-lambda` 1.11.1, `client-dynamodb`/`-sqs`/`-sns`/`-secrets-manager`/`-ssm` all exist+maintained, plus `@effect-aws/lambda` 1.7.0 (Jun 2026) + `powertools-logger`/`-tracer`. All are .NET-territory or deploy-time — **TS owns no AWS-service workload**; even the existing `@effect-aws/client-s3` row has no TS consumer. Backend lane candidate only; align future adds to one minor train (shared `@effect-aws/commons`).
- **@effect/sql-*** (v3-line lockstep, ~May 2026, `sql` 0.51.1/`sql-pg` 0.52.1): full driver set exists — `sqlite-node/bun/wasm/react-native/do`, `mysql2`, `mssql`, `clickhouse`, `d1`, `libsql` (=Turso; no `sql-turso`). **kysely + drizzle bridges COEXIST, do not contradict** an "`sql-pg` is owner" doctrine — both require an underlying `SqlClient` and only swap query construction; `sql-pg` keeps connection/transaction ownership (`sql-kysely` is the fragile one — binds Kysely internals). For consumer-only TS the whole family is workload-absent (Persistence owns SQLite/PG on .NET). The **sole browser-relevant member is `@effect/sql-sqlite-wasm`** — and only if the SPA ever needs local durable storage (today `idb-keyval` covers browser persistence).
- **Stability**: cluster 0.59.0 = unstable; workflow 0.18.2 = alpha; rpc 0.75.1 = stable (npm "WIP." is stale boilerplate). **Effect v4 beta (since Feb 2026)** folds sql/rpc/cluster/workflow into `effect/unstable/*`; standalone `@effect/*` remain the production surface, migration mechanical. Lockstep train: bump the whole family atomically, never `sql-pg` alone.
- **No first-class Effect bridge** exists for Pulumi (the `@pulumi/*` rows are deploy-time IaC, not TS runtime; Pulumi's Jan-2026 news is HCL/Terraform interop, not Effect), nor email/webhook/payment/search/realtime, nor tusd/upload/image/pdf. These stay domain-owner concerns (on .NET, AppHost outbound-resilience owns the retry boundary; browser file work via existing `papaparse`/`exceljs`/`jspdf` rows; resumable upload would use `tus-js-client`, not an Effect package).
- **@effect-atom** (React/Vue/Solid bindings; formerly `@effect-rx`) is the one genuinely SPA-relevant family surfaced — the only Effect-ecosystem state primitive that could displace the `zustand`/`zundo`/`immer` UI rows under the posture's "Effect state primitives own every store" law. But architecture-posture already commits to Effect ref/`Stream`-fold stores, so atom is a *choice within* the Effect-first doctrine, not a gap. **Flag for lane-research:** reconcile core-Effect-refs (current posture) vs `@effect-atom` (richer React ergonomics).

## Further Considerations

- The catalog carries a full backend Effect/AWS/Pulumi tier with **zero TS consumers** — predating the consumer-only scoping. Stage [2.A] dependency-usage proof will flag each. The honest structural move is a catalog partition: consumer-SPA tier (admitted) vs backend-aspirational tier (drift until a Node-TS planning page lands). Doctrine-facing rows must describe only the SPA tier.
- `@effect-atom/atom-react` 0.5.0 is ~5 months stale against `atom` core 0.5.3 — real 0.x skew hazard; if admitted, pin both exactly, no caret across the core/binding seam.
- The single browser-side SQL candidate is `@effect/sql-sqlite-wasm`, not `sql-pg` — if offline/local-durable SPA state ever becomes a workload, that WASM driver is the consumer-tier admission, preserving "Persistence owns server SQL on .NET."

Plan/deliverable written to `/Users/bardiasamiee/.claude/plans/ask-many-clarifying-questions-linear-dragon-agent-a8a7d22c4330d3745.md`.
