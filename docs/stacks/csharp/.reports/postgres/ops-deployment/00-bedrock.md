# ops-deployment — bedrock

## migration deployment law

- The deploy artifact is the compiled migration bundle: a self-contained executable produced at build time, carrying the migrations and the provider, executed at deploy time against a connection it receives as input.
- The application process never migrates itself: bundle-at-deploy versus migrate-at-startup is the boundary that keeps DDL privilege out of the runtime credential and makes schema change a deployment step with its own exit code, receipt, and rollback story.
- The credential split is two roles: a migration role owning DDL (schema ownership, executed only at deploy) and a runtime role owning DML on the migrated objects — default privileges on the migration role pre-grant the runtime role so every new object arrives readable without per-migration grant ceremony.
- The split is also the enforcement mechanism: runtime DDL attempts fail on privilege, not on review — the deployment law self-enforces at the store.
- Lock-cost migration vocabulary (engine facts that decide migration authoring): adding a column with a non-volatile default is metadata-only — no rewrite, no long lock; adding a column with a volatile default rewrites; widening a constrained text length is metadata-only, while a type change with a conversion expression rewrites under exclusive lock.
- The authoring law that follows: every migration is classifiable as metadata-only, concurrent-capable, or rewrite before it ships; rewrite-class migrations on hot relations decompose into expand-phase shadow columns plus batched backfill plus contract-phase swap — never one rewriting statement.
- Backfill batching is slot-aware: a single long transaction backfilling millions of rows stalls vacuum and pins replication-slot WAL for its duration — backfills run as key-range batches in separate short transactions, bounding both liabilities by batch size.
- Constraint tightening follows the two-phase grammar across constraint kinds: add as not-valid (instant — new writes enforced immediately), then validate in a later migration — validation takes only a weak lock and scans concurrently with traffic, so the expensive proof never rides the lock-taking step.
- The two-phase split also decouples deploy risk: the declare step is metadata-class and retry-trivial; the validate step is long but interrupt-safe and re-runnable — two migrations with different failure economics instead of one with the worst of both.
- The idempotent SQL script is the same law's reviewable form — generated from the identical migration set, applied by operators where execution rights are theirs.
- Concurrent-deploy serialization is engine-native: the migration lock is `LOCK TABLE` on the history table `IN ACCESS EXCLUSIVE MODE`, released with the transaction.
- Consequence one: parallel bundle executions serialize cleanly; the loser sees an already-migrated history — re-running a bundle is safe by construction.
- Consequence two: a crashed deployer cannot leave a stale lock — transaction abort releases it. No lock-cleanup runbook exists because no stale-lock state exists.
- The lock's transaction scope also means a migration that suppresses its transaction runs outside the serialization window — the concurrent-index bullet below inherits this.
- Concurrent-index trap, precise boundary: concurrent index creation cannot run inside a transaction, and migrations run transactional by default. The concurrent-creation annotation on a model index emits the right DDL, but the migration carrying it must be authored non-transactional (raw-SQL operation with transaction suppression as the carrier).
- Law: concurrent index builds are single-purpose migrations — ordered after the data-bearing ones, individually retryable.
- Being outside the transaction, a failed concurrent build leaves an invalid index; the verification pass detects it via the catalog's invalid-index flag, and the retry drops it first.
- History-existence probing is create-and-catch by design — the provider declares the exists-probe unreachable. First deployment against an empty database is the same code path as the hundredth; a bootstrap-versus-steady-state deploy split is foreclosed.
- The lock-queue trap governs every lock-taking migration: an access-exclusive request waits behind any long-running reader and queues every later query behind itself — a "fast" metadata-only DDL can stall a hot relation indefinitely through queueing alone.
- The defense is session policy, not scheduling hope: the migration session sets a bounded lock timeout so a blocked DDL aborts instead of queueing the world, and the deploy retries on a cadence — converting a potential outage into bounded retry latency.
- Deploy preflight reads activity before lock-taking migrations: long-running transactions against target relations are visible server-side, and deferring the deploy by one preflight read is cheaper than aborting it by lock timeout.
- A standby check belongs in preflight where topology routes connections: migration execution requires the writable primary, and the recovery-state probe is one read — deploying into a read-only standby fails late and confusingly without it.
- Post-apply verification rows (the bundle's exit code is necessary, not sufficient): applied-migration set equals compiled set (history read); no invalid indexes (catalog flag scan); no pending-model drift.
- The drift check runs at build time where it gates artifact production — a deploy-time drift discovery is a build-gate failure escaped downstream, and the repair is upstream.
- Type-catalog refresh is part of deploy choreography: after a migration adds wire types (enums, composites, extensions), running processes must re-resolve types — a rolling restart or explicit reload step, not an afterthought. Deployment completes when processes re-resolve, not when DDL lands.
- Schema-epoch token: the deployed schema's identity is the ordered migration-id set's hash — computed from the artifact, stamped by deployment, published on the process's discovery manifest as a store-epoch token.
- Cross-process compatibility becomes one comparison: a consumer whose compiled epoch trails the store's published epoch refuses the lane with a typed rejection (composing the settled newer-schema-rejection law); a consumer ahead of the store signals the deploy gap.
- The tolerated epoch span is derived from the consumer's compiled migration set, never hand-maintained — a hand-edited span drifts from the model it claims to describe; a derived span cannot.
- The token makes "is every process on this schema" a manifest read instead of an investigation.
- The deploy receipt is structured, not an exit code: applied migration ids, per-migration duration, and the resulting epoch — emitted by the deploy step into the same fact stream maintenance receipts ride, so deployment history is queryable beside operational history.
- Rollback law: migrations roll forward only — down migrations are authoring-time tools, never deploy artifacts; the epoch is monotonic, and "rolling back" a bad expand migration is a new forward migration that reverses it, keeping history append-only and the epoch comparison total.
- Restore-from-backup is the rollback of last resort and lives outside this law entirely — it rewinds the epoch, which is exactly why it is an operator decision with suite-wide manifest consequences, never a deploy-step option.

## maintenance via the host schedule

- Store maintenance is a closed table of scheduler rows — each idempotent, each receipted, cadence owned by the suite scheduler (settled law).
- Row: partition lifecycle advance — the partition manager's one maintenance call.
- Row: concurrent reindex for churn-heavy index species — graph indexes after bulk deletes, ordered before vacuum-heavy windows.
- Concurrent-reindex failure artifact: an interrupted concurrent rebuild leaves a suffixed invalid sibling index behind; the row's retry preamble drops invalid siblings before rebuilding — the same invalid-index catalog scan the deploy law runs, reused as a maintenance precondition.
- Long structural operations are progress-observable: the engine's progress views expose phase and block counts for index builds and vacuums — the receipt stream can carry phase progress for operations spanning minutes, turning "is it stuck" into a read.
- Full-rewrite vacuum is a foreclosed row: it takes an access-exclusive lock for the duration — bloat is handled by concurrent reindex plus engine-daemon tuning at the operator rung, never by scheduling a rewrite.
- Non-concurrent materialized-view refresh is likewise foreclosed for serving views — it exclusively locks the view for the refresh; the concurrent form's unique-index precondition is the price of staying online, which is why the precondition is verified before the row is schedulable.
- Row: explicit analyze after bulk admission events — autovacuum's trigger thresholds under-serve single large loads; the schedule row informs the engine of what it cannot infer promptly.
- The analyze row is event-keyed, not clock-keyed: it consumes bulk-admission receipts from the fact stream as its trigger — statistics refresh follows the load that staled them, and an idle store schedules nothing.
- Concurrent index builds wait out every transaction that predates them: a long-running transaction anywhere in the database delays the build's completion phases — the deploy-law preflight read applies to the maintenance reindex row identically.
- Row: materialized-view refresh in its concurrent form — which requires the view's unique index, a model fact the verification pass checks before the row is schedulable.
- Row: retention pruning where retention is row-wise rather than partition-structural.
- The in-database cron primitive is explicitly deleted, with three named reasons: it is operator-provisioned (preloaded library — closed allowlists make it unportable); it is a per-cluster singleton scheduler living in one privileged database (a second cadence owner beside the suite scheduler — duplicate ownership); and its job state is invisible to suite telemetry (jobs succeed or fail inside the store, outside the receipt stream).
- Every job it could run is expressible as a scheduler row executing one SQL call through the standard rail — the deletion costs zero capability.
- Boundary against the engine's own daemons: autovacuum, background writer, and checkpointer are engine-owned — never scheduled, duplicated, or disabled by suite rows.
- The schedule owns only what the engine cannot know: domain-meaningful bulk events, partition policy, view-refresh semantics. A schedule row re-implementing an autovacuum behavior is lateral creep into engine territory — the rejected form.
- Maintenance receipts close the loop: each row emits a typed receipt (rows pruned, partitions created/detached, index rebuilt with size delta, analyze target) into the standard fact stream.
- A maintenance row completing without evidence is indistinguishable from one that never ran — the operational defect receipts exist to kill.
- Maintenance windows are budget rows, not time rows: concurrent reindex and analyze are online but I/O-priced; each row carries a concurrency budget (one structural operation at a time per store) enforced by the scheduler's own serialization — never by in-database advisory locking from application code.
- Missed-run tolerance is designed, not hoped: the partition row's pre-creation horizon is the budget for scheduler downtime — maintenance may miss runs up to the horizon with zero ingestion impact, which sizes the horizon: tolerated outage divided by partition interval, plus one.
- The same tolerance arithmetic generalizes: every maintenance row declares what accumulates while it is not running (unpruned rows, index bloat, stale statistics) and the accumulation rate bounds its maximum cadence — cadence is derived from tolerance, never picked by convention.
- Expensive rows are condition-gated, not unconditionally periodic: the reindex row runs its catalog-statistics bloat estimate first and no-ops below threshold — the receipt then carries the measured condition either way, so "checked, not needed" is distinguishable from "never checked".
- The condition-gate pattern keeps the schedule table honest: every row is cheap to run on its cadence because the expensive body fires only on evidence — cadence governs checking, evidence governs acting.

## classification-to-audit binding

- The binding law: the classification taxonomy (owned elsewhere, arriving settled) maps to store-side audit as data — classified relations and the operation classes touching them bind to the audit extension's two modes.
- The two modes compose rather than compete: session classes set the broad posture, the audit role's grants set per-relation precision — the binding declares both, and each verifies independently.
- Session-mode binding: the audit class set (READ, WRITE, FUNCTION, ROLE, DDL, MISC, MISC_SET, with subtraction syntax) expresses the suite's audit posture as one server-setting value; the declared posture for a classified store is an expectation row — write-and-DDL minimum, role changes always.
- MISC_SET earns its own mention: it audits privilege-switch statements specifically — the evidence trail for role assumption, which the broader MISC class would drown in checkpoint and discard noise; the subtraction syntax exists precisely to keep MISC out while keeping MISC_SET in.
- Audit output rides the server log pipeline — volume is a server-log concern and shipping is an operator-rung artifact; the suite's telemetry governance consumes shipped audit records as an external source, never as in-band process logs.
- Object-mode binding: a dedicated audit role is granted exactly the per-relation privileges (SELECT/INSERT/UPDATE/DELETE per classified relation) whose exercise must be audited.
- Posture can differ per connecting role by design: the role-scoped setting mechanism that creates the shadowing hazard also enables intentional asymmetry — a stricter class set for the runtime role than for maintenance roles is a declared posture pair, and the verification reads each declared role's effective value separately.
- The grant set IS the classification projection: "which classified data is audited" becomes a catalog query over the audit role's grants — mechanically derivable from the classification rows and mechanically diffable against them.
- Parameter-logging coupling: the audit extension's statement-parameter logging toggles whether bound values enter audit logs. The binding rule is severity-driven: classifications that must never appear in logs require parameter logging off — or the audited statements re-shaped so classified values ride parameters, never literals, since literals always log.
- This is the one seam where audit posture and the redaction taxonomy can contradict each other; the verification row below exists to catch the contradiction.
- Verification against server settings — the lane's operative law, since binding is declared and never installed by the process:
- Row: preload-list membership for the audit library — absence means session/object audit is impossible; the capability folds to a degradation receipt naming the classification rows left uncovered.
- Row: effective audit-class setting versus declared posture — read as the effective value for the connecting role, source-aware: role- and database-scoped settings shadow the cluster value, and the effective value is the one that binds.
- Row: audit-role existence and grant set versus the classification projection — set difference in both directions: unaudited classified relations, and stale grants on declassified ones.
- Row: parameter-logging state versus the redaction requirement.
- The parameter field's marker forms make the posture assertable from output: an audit record distinguishes "statement had no parameters" from "parameters withheld by policy" — one sampled record proves the not-logged posture is actually in force, closing the gap between setting and behavior.
- Each mismatch is one typed evidence row; the process alters nothing.
- Mismatch disposition is graded, not binary — grade one: missing preload → audit lane folded, degradation receipted, store usable.
- Grade two: posture weaker than declared → operate-with-evidence; the gap itself logs through the standard stream, so the suite knows it is under-audited.
- Grade three: grant drift → evidence plus a generated reconciliation grant script as an operator artifact.
- The process emits the repair script; the operator runs it — provisioning stays verification-only even for repairs.

## provisioning-is-verification-only

- The verification fold runs at store admission, before the store profile goes live, as one read-only pass producing a capability verdict per declared lane.
- Every row below is a catalog or settings read — no row touches user relations, so admission cost is independent of data volume and the fold is boot-path cheap by construction.
- Row: engine version floor — one numeric read (`server_version_num`), compared as an integer, never parsed from display strings.
- Row: extension presence and version floors — extension catalog rows: name, installed version, schema; the floor is per-capability row data, since a present extension below the generation that carries a needed setting is absence for that capability.
- Extension upgrades are rung-1 artifacts like creation: the upgrade statement ships in a migration, and the verification floor is what makes a skipped upgrade visible as a capability gap instead of a runtime surprise.
- Row: replication readiness where the change-contract lane is declared — WAL level logical; replication-slot and WAL-sender headroom against configured maxima; durable-slot existence, plugin, and lag.
- Slot-liability sub-rows: the slot catalog exposes per-slot WAL status and remaining safe headroom, and the WAL-retention ceiling setting bounds how much disk a lagging consumer can pin — together they convert "a slow consumer can fill the disk" into two readable gauges and one declared bound.
- A slot whose status reaches the unreserved/lost band is a consumer-integrity event, not just an ops event: the consumer's replay window is gone, and the verification verdict downgrades the change-contract lane to re-bootstrap-required — a typed state the consumer dispatches on.
- Row: invalid-index scan — the index catalog's validity flag finds half-built concurrent indexes from failed deploys or maintenance; an invalid index silently serves nothing while costing writes, so its presence is always evidence demanding disposition.
- Row: notification-queue headroom — the queue-usage fraction is one read, and a climbing value names a stalled listener before notifying transactions feel backpressure.
- Row: slot inventory versus the declared consumer set — a durable slot with no declared consumer is an unowned disk liability; the verdict flags it for operator disposition rather than guessing at ownership.
- Verification queries themselves ride the read-only rail under a statement timeout: a hung verification must not block admission indefinitely — timeout folds to the degraded verdict, and the timeout itself is evidence.
- Epoch publication is atomic with the verdict: the manifest never shows a store epoch without its capability verdict, so a consumer reading the manifest sees schema identity and capability truth from the same instant.
- Row: server-settings expectations — preload list, checksum state, and the I/O-concurrency settings that explain throughput envelopes; read as facts, never targets.
- Settings rows carry their restart class: the settings catalog exposes each parameter's change context (restart-requiring versus reload-applied versus session-settable), so a settings mismatch in the evidence names not just the gap but the disruption class of its repair — a restart-class gap is a maintenance-window artifact, a reload-class gap a routine one.
- Per-role and per-database setting scopes are repair levers as well as shadowing hazards: the generated repair artifact can target the narrowest scope that closes the gap (the connecting role) instead of demanding cluster-wide change — smallest-scope repair is the default posture.
- Row: privilege floors — the engine's privilege-probe functions (`has_table_privilege`, `has_schema_privilege` species) per relation and schema class the profile needs; probing beats attempting, because a privilege failure mid-transaction aborts work the probe would have rerouted.
- Row: audit binding (previous section).
- Row: schedule-row preconditions — partition-config rows present for registered partition sets; unique indexes present for concurrent view refresh.
- Absence folds to degradation, and the fold's algebra is explicit: each capability row declares its failure rank — required (store profile refuses to open: engine floor, core schema, epoch match), degradable (lane folds out with a receipt: vector lane absent means the lane's queries are declared unreachable at admission, not discovered at first query), observational (evidence only: settings weaker than ideal).
- The fold's output is a typed capability set the process carries; downstream code dispatches on capability rows and never re-probes — one verification, one verdict, total dispatch.
- Required-rank minimalism is deliberate: the required set stays as small as truth allows (engine floor, core schema, epoch) so a degraded environment still admits the process for diagnostics and partial service — a fat required set converts every environment gap into a total outage.
- The process never performs the alteration — the rejected-form catalogue, each row with its reason:
- Rejected: runtime `CREATE EXTENSION` / `ALTER SYSTEM` / settings writes — privilege escalation and a config-ownership fork between artifacts and runtime.
- Rejected: runtime durable-slot creation — server-disk liability bound to process lifetime.
- Rejected: runtime audit-role grants — audit integrity requires the auditor not be the audited.
- Rejected: runtime preload edits — they require restarts the process cannot own.
- Each rejected row has its artifact owner: migrations own rung-1 DDL, seeds own rung-2 state, operator runbooks own rung-3 settings, the environment owns rung-4 — verification reads all four rungs through one fold.
- Verification is itself receipted and periodic, not boot-only: the admission pass stamps a verification epoch — settings hash plus extension set plus schema epoch.
- A lightweight re-verify on a scheduler row detects environment drift (an operator upgraded an extension; a managed service changed a default) and re-emits the capability verdict — environment change becomes an observable event in the fact stream rather than a latent behavior shift.

## divergent — deploy-migration-ops

- The unified deploy law in one shape: build produces an artifact triple — bundle, idempotent script, schema epoch — from one migration set; deploy executes exactly one of the first two depending on the environment's execution-rights rung, then publishes the third.
- Everything else — serialization, re-runnability, empty-database bootstrap — is engine-and-provider-native (table lock, history idempotence, create-and-catch) and needs zero orchestration code.
- The entire deployment "framework" is therefore three steps: run artifact, read receipts, publish epoch. No state machine exists because no state needs machining.
- Failure taxonomy at the deploy seam — row one: lock wait → another deploy in flight; wait or yield, never force.
- Row two: transactional-migration failure → full rollback, history untouched, artifact re-runnable as-is.
- Row three: suppressed-transaction failure → partial DDL possible; the invalid-index scan plus a migration-specific compensation note are mandatory before retry.
- Row four: post-apply epoch mismatch across processes → rolling restart incomplete; a manifest sweep names the trailing processes.
- The taxonomy is closed because the lock model and transaction scopes are known — every deploy failure lands in exactly one row, and a deploy failure outside the table is a new fact about the engine, not about the deployment.
- Zero-downtime composition — expand-contract expressed in this page's primitives: expand migrations (additive, transactional) deploy ahead of code.
- The epoch token's compatibility is therefore a range, not a point: a process declares the epoch span it tolerates, and the manifest comparison admits any store epoch in-span — which is what makes rolling deploys with mixed process generations a non-event.
- Contract migrations (destructive) deploy only after the manifest shows no live process whose span includes the contracted shape — the manifest is the contract-gate's evidence, closing the loop between deployment and discovery.
- Bundle-execution context is part of the artifact contract: the bundle carries the provider and migrations but not the secrets — the connection arrives at execution from the deploy environment's credential surface, which is what lets one artifact serve every environment rung without rebuild.
- The bundle also executes from the deploy environment's network seat: store reachability from that seat is part of the artifact contract, verified separately from runtime-process reachability — the two seats can differ, and a deploy that assumes they match fails in exactly the environments where segmentation is strictest.
- Maintenance receipts stamp the verification epoch they ran under — a receipt emitted before an environment-drift repair is distinguishable from one after it, so post-repair behavior change has its boundary in the fact stream.
- Multi-store suites deploy per-store: each database carries its own history, lock, and epoch; there is no cross-store migration transaction, and a contract spanning two stores gates on both manifests showing both epochs in-span — the manifest comparison generalizes from one store to N without new machinery.
- Store-count growth is therefore rows again: a new database is one more (bundle, epoch, verification) triple riding the same three-step deploy — nothing about the deployment law is per-store code.

## divergent — verification-only-audit

- The deepest consequence of verification-only: the store's environment becomes a typed input to the process rather than a side effect of it — the capability verdict is a value, capability rows are dispatch surface, and environment variance across deployments is absorbed by the same fold that handles version floors.
- Provisioning code, environment-conditional branches, and runtime "if extension exists" probes are all foreclosed by one admission-time fold — the law converts an operational concern into the paradigm's standard admit-once shape.
- Audit binding as classification's proof obligation: classification without store-side audit is a claim; the binding's verification rows are the claim's evidence chain — taxonomy → grant projection → effective settings → log reality.
- The chain's weakest link is scope shadowing: role- and database-scoped settings silently override cluster posture, which is why verification reads effective values for the connecting role, never cluster defaults.
- An audit posture verified against the wrong scope is a false assurance — the single most dangerous defect this lane can emit, because it fails toward confidence.
- Repair-artifact emission is the verification law's ceiling: the process may generate operator artifacts — reconciliation grants, missing-extension runbook entries, settings diffs — as typed outputs of the verification fold; maximal helpfulness without crossing the alteration boundary.
- The emitted artifact carries the same evidence rows that justified it, making operator action auditable against the verification that requested it.
- The loop closes on the next periodic re-verify, which either clears the evidence or re-emits it — verification, evidence, artifact, re-verification: the full lifecycle is a fold over reads, and nothing in it ever holds write privilege.
- Cross-lane closure: the verification epoch (this lane), the schema epoch (deploy law), and the capability verdict (admission fold) are three stamps on one manifest surface — together they make "what store, what shape, what capabilities, audited how" answerable for every process in a suite from manifest reads alone.
