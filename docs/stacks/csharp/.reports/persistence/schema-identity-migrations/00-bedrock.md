# schema-identity-migrations — bedrock

## migration anatomy and receipts

- A migration is data before it is action: `Migration.UpOperations` and `DownOperations` materialize the operation lists without touching a store (lazily built from the `Up`/`Down` bodies).
- `Migration.TargetModel` carries the post-migration model; `ActiveProvider` stamps the generating engine — both readable without a store, so audits run on the artifact alone.
- Every audit — destructive scan, rename detection, operation counting — folds over `UpOperations`; parsing generated SQL for audit is the rejected form.
- Migration identifiers are timestamp-prefixed, so identifier order is application order: set differences sort lexically, and `max(applied)` is the store's schema stamp.
- The diff engine is a public seam: `IMigrationsModelDiffer.HasDifferences(IRelationalModel?, IRelationalModel?)` and `GetDifferences(...)` compute operation lists between any two relational models — the primitive behind snapshot drift checks, reusable for "what would change" receipts without scaffolding.
- The snapshot is a model, not a string: `ModelSnapshot.Model` rebuilds the last-scaffolded model for diffing against the runtime model — the comparison object every drift gate consumes.
- Script generation is a flagged policy: `IMigrator.GenerateScript(fromMigration, toMigration, MigrationsSqlGenerationOptions)` with flags `Default`, `Script`, `Idempotent`, `NoTransactions`.
- `GenerateCreateScript()` on the facade is the from-zero variant — schema-as-artifact for fresh provisioning outside the migration path.
- Idempotent scripts check the history table per step and apply only the missing suffix — the only script form safe against unknown store state.
- A `from` newer than `to` yields a rollback script — rollback is a generation parameter, not a special tool.
- Rollback applies the `Down` bodies of newer migrations in reverse identifier order; `Migrate(targetMigration)` is the runtime spelling, and target `0` reverts everything — the full-teardown form, gated like any destructive operation.
- Each migration applies in its own transaction; a one-transaction-spanning apply across the whole set does not exist — a mid-set failure leaves an applied prefix, never a torn migration.
- The partially-applied receipt is a set difference, not a log parse: assembly set `GetMigrations()`, applied set `GetAppliedMigrations()`; prefix = intersection, remainder = pending.
- Recovery from a partial apply is resume — idempotent re-apply of the remainder — never rollback of the prefix.
- Because each migration commits independently, the applied-set receipt is monotone: observers cache "applied through X" and only ever extend it, which makes the boot verdict fold cheap enough to run on every boot, not just deploys.
- Wrapping `Migrate`/`MigrateAsync` in an explicit transaction is unsupported and throws — the migrator owns its transaction topology.
- The history table and the migration owner are profile rows declared once: `MigrationsHistoryTable(name, schema)` and `MigrationsAssembly`; `HistoryRepository` is the abstraction beneath both.
- Design-time tooling is a private asset: the design package's operations (`AddMigration`, `RemoveMigration`, `ScriptMigration`, `HasPendingModelChanges`, `GetMigrations`, `Optimize`) drive scaffolding and never become runtime dependencies.
- Scaffold output is generated shape: reviewed before it enters source, regenerated rather than hand-edited — a hand-edited migration body and its snapshot diverge silently.

## raw-SQL escape hatch

- `MigrationBuilder.Sql(sql, suppressTransaction = false)` produces an `OperationBuilder<SqlOperation>` — the escape hatch is itself an operation, so it stays visible to the operation fold.
- A `SqlOperation` is unclassifiable by the physical-class fold: the gate law is that every `Sql` operation carries an explicit class token, and an untokened `Sql` is classified destructive — worst-case by default, never benign by default.
- `suppressTransaction: true` exempts one operation from its migration's transaction — required where the engine forbids the DDL inside a transaction; the cost is that the receipt unit degrades from migration to operation: a failure after a suppressed op leaves a partially-applied migration, so a suppressed op must be idempotent in its own right.
- Hand-written DDL outside the migration set entirely — applied by hand, by scripts beside the set — is the invisible-drift form: the diff engine cannot see it, the snapshot does not record it, and the next scaffold silently fights it.

## migration locking and seeding

- `Migrate`/`MigrateAsync` acquire a store-wide lock for the full application span, covering CLI applies, bundles, and runtime applies uniformly; SQL scripts run outside the lock by definition.
- Concurrent fleet instances racing the lock serialize instead of corrupting — the lock is what makes boot-time apply merely unsound rather than catastrophic.
- The embedded-engine lock (`SqliteMigrationDatabaseLock`) is a lock table, and a hard process kill abandons it — every subsequent apply then blocks indefinitely.
- Boot choreography includes abandoned-lock reclamation as a gated step — detect the stale lock row, verify no live holder, clear — before the apply step.
- Treating a blocked apply as "store busy" retries forever against a ghost; reclamation, not patience, is the recovery.
- Seeding rides the migration lock: `UseSeeding(Action<DbContext, bool>)` / `UseAsyncSeeding(Func<DbContext, bool, CancellationToken, Task>)` on the options builder are the only sanctioned seed hooks, executing inside the lock span — single-writer by construction.
- Seed bodies are idempotent by contract: they re-run on every apply pass; the `bool` parameter reports whether the store was just created and is the only freshness signal a seed body may branch on.

## deployment vehicles

- Bundles are the deployment artifact: `dotnet ef migrations bundle` emits a single-file executable; `--self-contained -r <rid>` for runtime-free hosts, `--force` to overwrite, `--output` to place.
- The bundle executable applies pending migrations exactly like a runtime apply — same lock, same history check, same per-migration transactions — and re-running an up-to-date bundle is a no-op.
- The bundle accepts `--connection` to retarget; absent that, it resolves the configured connection from its settings file in the execution directory — a bundle shipped without its configuration silently targets nothing.
- Vehicle selection is a profile row, not a habit: bundle-per-release for fleet deployments (CI builds it, deploy executes it before instance rollout); idempotent script for DBA-gated stores; runtime apply at boot only for the single-writer embedded profile.
- Runtime boot apply for fleets is the rejected default even though the lock makes it safe: it grants every instance DDL rights and couples rollout order to schema state — safe but not sound.
- Where runtime apply is admitted, it runs in the gated lifecycle state before serving.
- Resolve `IMigrator` from context infrastructure for receipt-bearing applies — `Migrate(targetMigration)` for pinned-version applies and rollbacks — rather than the facade shorthand when the verdict must carry evidence.

## schema-newer-than-model rejection

- Boot computes one total verdict from two sets and one drift gate, every outcome typed; the inputs are assembly set `A = GetMigrations()` and applied set `D = GetAppliedMigrations()`.
- `D − A ≠ ∅` — the store schema is newer than the compiled binary: typed rejection carrying the unknown migration identifiers as evidence, never a best-effort open.
- The rejection rationale is structural: an older binary's model cannot describe columns it has never seen; a silent open corrupts on first write.
- `A − D ≠ ∅` — pending migrations: the verdict routes by profile row — apply (single-writer), reject (fleet member awaiting its bundle), or read-only degrade (readers that tolerate an older schema by construction).
- Sets equal — the model-drift gate runs (composed from the model-fingerprint law); a drift verdict rejects in development gates and emits evidence in production.
- A fresh store — no history table at all — is its own verdict case, distinct from pending: it routes to the provision path (apply-from-zero on single-writer rows, reject-await-bundle on fleet rows), never to the pending-migration arm.
- The rejection is a receipt, not a boolean: verdict case + offending identifier set + history-table row stamps; degradation folds consume the case, support folds consume the identifiers.
- Read-only degrade has a precise legality bound: a reader may serve over a newer schema only when every consumed projection is closed over columns its own model knows — guaranteed by additive-only expansion, voided by any contract wave in the unknown suffix.
- Because the unknown suffix is by definition unclassifiable locally, the sound default for `D − A ≠ ∅` is hard rejection; the degradable variant exists only where suite policy guarantees the in-flight wave is expand-only — a deployment invariant, not a runtime discovery.

## expand-before-contract

- Every schema change decomposes into an expand wave and a contract wave with a deployment boundary between them; a migration mixing both waves in one operation list is the gate's primary rejection target.
- Expand is strictly additive: new tables; new columns nullable or defaulted; new indexes; dual-write paths for moves. Old binaries keep working against the expanded schema by construction.
- Contract removes — drop column/table, tighten nullability, narrow types — and ships only after every reader of the old shape is retired.
- Rename is the forbidden middle: a rename is expand (add new) + backfill + contract (drop old); an in-place rename breaks every concurrent old-binary reader — exactly the window a fleet rollout creates.
- The backfill between waves is a data migration and rides the bulk rail, not the schema rail: schema migrations carry DDL plus marker DML only.
- A row-mass backfill inside a migration holds the migration lock for its whole duration and serializes the fleet; above the row count whose backfill exceeds the fleet's health-probe window, the work must leave the schema rail — the lock is fleet-wide back-pressure.
- The wave split is what makes the read-only-degrade row sound and what orders bundles: bundle N (expand) deploys before binaries N; binaries N retire old-shape readers; bundle N+1 (contract) closes the loop.
- Engine-declared objects ride the model and diff like any object: declared extensions, enums, and ranges scaffold into migration operations; removing an enum value or extension is contract-wave material with the same gating as a column drop.

## destructive gating

- The gate classifies operations physically, not nominally: fold `UpOperations` into classes — additive, rename-equivalent, destructive (`DropTable`, `DropColumn`, narrowing `AlterColumn`), and rebuild.
- The rebuild class exists because the embedded engine implements most column alterations as create-copy-drop table rebuilds: an operation that reads as a benign alter is physically a full-table rewrite under the lock.
- The fold therefore gates on the physical class for the `ActiveProvider` the migration itself stamps — one classification table per engine, selected by the stamp.
- Destructive admission is a per-migration token verified at generation/CI time: a migration whose fold contains a destructive class without the explicit admission marker fails the pipeline before any store sees it.
- Apply-time gating is the rejected form — by then the bundle is in the deploy path and the only outcomes are skip-and-drift or apply-and-lose.
- `Down` bodies of destructive migrations are honesty checks: a destructive `Up` whose `Down` cannot restore data (drop column → re-add empty) must declare irreversibility rather than fabricate a lossy `Down`.
- The gate treats a data-lossy `Down` as a second destructive operation — irreversibility declared is admissible, irreversibility disguised is not.

## identity-policy axis

- Identity is one closed three-row axis — time-ordered surrogate, content-hash, natural — and every row carries the same four columns: generator (who mints), transcription (how each store spells it), ordering semantics (what an index over it does), collision law (what equality of two mints means).
- Time-ordered surrogate: `Guid.CreateVersion7()` mints at the admission boundary — the owner factory stamps identity; the store never generates (`ValueGeneratedNever` is the transcription).
- Mint-at-admission means identity exists before any round-trip: keys are insertable in bulk lanes, referenceable before save, and stable across retries.
- `CreateVersion7(DateTimeOffset)` is the deterministic-backfill overload: during an identity migration, historical rows mint surrogates from their original timestamps so index locality matches history instead of clustering at migration time.
- Ordering survives transcription only when the spelling is order-preserving: the canonical text form is lexically time-ordered; the default byte export is not — the structure's little-endian field layout scrambles the timestamp prefix.
- `ToByteArray(bigEndian: true)` is the binary transcription law for order-preserving byte storage.
- The embedded engine maps the type to `TEXT` storage, so time-order survives there by default; any binary-keyed store must declare the big-endian transcription or the primary index degrades to random-insert fragmentation — the exact pathology the time-ordered row exists to delete.
- Content-hash: identity is a hash of the canonical payload encoding, so re-admission of identical content is idempotent by construction — the natural upsert key for derived and imported rows.
- The canonical encoding is declared (field order, normalization) because hash identity is encoding identity — two encodings of one payload are two identities.
- The collision posture is a declared column — reject-on-different-payload versus width-makes-it-ignorable — not an afterthought; hash mechanics arrive composed from their owning layer, and this axis owns only the policy columns.
- Natural: the domain key is the primary key, transcribed through the generated-converter seam (a keyed value object as key property), and the owner is immutable.
- Natural-key mutation is foreclosed at the shape: a mutable primary key is a delete-insert wearing an update's clothes, breaking every foreign reference and changefeed key derived from it.
- Key-selector transcription law: each aggregate declares one key selector, once; every secondary surface — foreign-key columns, index orderings, changefeed keys, cache-tag derivation, pagination cursors — derives from that selector mechanically.
- A surface restating the key shape by hand — a second tuple spelling, a stringified composite — is the drift form the law deletes; when the selector changes, derivation makes every surface follow in one diff.
- Axis selection pressure: surrogate when the aggregate has independent lifecycle and high insert volume (locality wins); content-hash when identity is the content (dedup, import, derived artifacts); natural when an external authority already owns uniqueness and immutability.
- Mixing rows per aggregate is normal; mixing rows per surface of one aggregate is the defect.

## divergent

- migration-law — the one migration law: a schema change is a typed value with a total lifecycle — diffed (`GetDifferences`), classified (the physical-class fold), gated (destructive token), waved (expand | backfill | contract), vehicled (bundle | idempotent script | runtime apply as profile rows), locked (store-wide, reclamation-aware), receipted (applied-set difference) — and every stage consumes the previous stage's value. Maximal collapse: the boot verdict fold, the destructive gate, and the partial-apply receipt are one algebra over `(A, D, UpOperations)`; implementing them as three tools re-derives the same sets three times. Foreclosed forms: hand-written DDL beside the migration set (invisible to the diff), `EnsureCreated` against a migrated store (history bypass), schema probing as a substitute for set algebra, fabricated lossy `Down` bodies, untokened `Sql` operations, and backfills that squat on the migration lock. The under-appreciated edge: `suppressTransaction` is the one knob that breaks the per-migration atomicity guarantee, so the gate tracks it as a receipt-degradation class — a migration containing a suppressed operation is receipted per-operation, not per-migration.
- identity-policy-axis — the one identity law: identity is minted exactly once at admission, spelled exactly once per store via a declared transcription, and every secondary key surface is a derivation — generator, transcription, ordering, and collision are four columns of one row, never four decisions in four files. The advanced interaction: identity policy and migration law meet at the key — changing an aggregate's identity row is never an `AlterColumn`; it is an expand-wave second key (new column, dual-keyed window, backfilled via the deterministic-timestamp mint from the old selector), a derivation flip, and a contract-wave drop of the old key — the only identity migration that preserves foreign references, changefeed continuity, and cursor validity simultaneously. Foreclosed forms: store-generated surrogates under the time-ordered row (mint-at-admission is the row's point), order-destroying byte transcriptions, hash identity without a declared canonical encoding, natural keys on mutable owners, per-surface key respelling. Edge with its law: cursors and changefeed keys derived from a retired selector remain valid through the dual-keyed window and expire with the contract wave — the window length is a declared policy value consumed by cursor-rejection logic, never an operational accident.
