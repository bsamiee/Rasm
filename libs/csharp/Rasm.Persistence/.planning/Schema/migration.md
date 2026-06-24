# [PERSISTENCE_SCHEMA_MIGRATION]

Migration law for every store the suite opens, in three folds over one fault family. `SchemaGate.Admit` folds two migration-id sets, a drift probe, and a cross-process epoch gate into a total `SchemaVerdict` routed by `StorePlacement` — a reader serves behind, a writer provisions or advances, an operator-gated store awaits a bundle, a contract wave quiesces while a live process still spans the prior epoch, and only an unknown applied suffix or unregenerated drift aborts; the open ceremony never best-effort opens. `MigrationLaw.Plan` folds a materialized `Migration` at generation time into a `MigrationPlan` that separates the expand wave from the contract wave with the backfill boundary between, classifying every operation through `MigrationOperation.IsDestructiveChange` refined by the type-keyed disposition table against the `Migration.ActiveProvider` the artifact stamps and the per-migration `RetentionApproval`. `MigrationLaw.LockLight` sequences the PG18 weak-lock DDL — `NOT VALID` constraint validation, the temporal-key `WITHOUT OVERLAPS` two-step, `NOT ENFORCED` check enablement, `CONCURRENTLY` index build with its invalid-index retry preamble, partition `INHERIT` propagation, and the post-deploy `ReloadTypes` barrier — as ordered `LockLightStep` arms with correct transaction discipline and a bounded `LockBudget` lock-timeout so the lock-queue trap converts into deploy-retry latency. `SchemaFingerprint` stamps the `ModelSnapshot` digest, `SchemaEpoch` stamps the ordered applied-id-set deploy hash, and `MigrationReceipt` stamps the applied-prefix evidence from `ClockPolicy` and `CorrelationId`, so every open proves which schema prefix it ran against, against which compiled model, and which fleet epoch it published.

## [01]-[INDEX]

- [01]-[BOOT_VERDICT]: the placement-routed `SchemaVerdict` fold, the fingerprint gate, and the `MigrationReceipt`.
- [02]-[WAVE_PLAN]: the generation-time `MigrationPlan` expand/contract split, the `IsDestructiveChange`-driven disposition, the `RetentionApproval` gate, and the `DeployVehicle` selection.
- [03]-[LOCK_LIGHT]: the PG18 weak-lock `LockLightStep` sequencing, the `LockBudget` lock-timeout/preflight discipline, and the post-deploy `ReloadTypes` barrier.

## [02]-[BOOT_VERDICT]

- Owner: `SchemaFault` `[Union]` fault family on the doctrine `Expected` shape carrying the `IValidationError<SchemaFault>` `Create` contract (the one fault family the whole `Schema` folder shares); `SchemaVerdict` `[Union]` the total boot-state classification; `SchemaFingerprint` the `ModelSnapshot` digest; `SchemaEpoch` the ordered applied-id-set deploy hash; `ContractWave` the contract-hold evidence pair; `MigrationReceipt` the applied-prefix receipt; `SchemaGate` the boot fold.
- Cases: `SchemaFault` — Text, NewerSchema, EpochDivergent, PendingModel, PartialApplication, RejectedExpand, DestructiveUnapproved, ForbiddenRename, IrreversibleUnacknowledged, TypeUnresolved, AclScopeMismatch, AclDenied, AclExpired, AuthorshipUnauthored, AuthorshipForged, OpDigestWidth, Unclassified — codes 5300-5316; the `Schema` folder shares this one fault family so `Schema/identity#AUTHORITY` projects its `AuthDecision` faults here and never declares a second taxonomy. Accumulation rides the base `Error` monoid (`Error.Combine` / `Errors.None`) per the doctrine `[VALIDATION_MONOID]` law — every `SchemaFault` IS an `Error`, so `MigrationLaw.Plan` collects independent operation faults through `Validation<Error, …>` without a self-monoidal fault or a synthetic aggregate case. `SchemaVerdict` — Serving, ServingBehind, Provisioned, Advanced, AwaitBundle, Quiescing, Drifted.
- Entry: `public static Fin<SchemaVerdict> Admit(Seq<string> applied, Seq<string> known, StorePlacement placement, SchemaFingerprint stored, SchemaFingerprint compiled, bool pendingChanges, Option<ContractWave> contractWave, Func<Fin<Seq<string>>> apply)` folds the applied-versus-known sets and the drift probe into the total verdict — an unknown applied suffix aborts to `NewerSchema` unless the placement reads ahead under an expand-only suite invariant (then `ServingBehind`), a pending suffix provisions or advances on a `MigrateOnOpen` writer through `apply` (mid-set failure carries the applied prefix as `PartialApplication`) or yields `AwaitBundle` for an operator-gated store, a pending suffix carrying a contract wave holds as `Quiescing` when the `ContractWave` evidence is present (a single `Some` that proves both a contract is pending AND a live process still spans the pre-contract `Spanning` epoch — the prior naked `pendingContracts` flag plus an unpaired `liveSpanning` option admitted the unrepresentable split where a contract holds with no spanning epoch), and equal sets fold `pendingChanges` to `Drifted` or `Serving`; `public Fin<Unit> SchemaVerdict.Require(SchemaFingerprint compiled, bool strict)` is the open-gate projection the open ceremony folds — every applied or serving verdict passes (an `AwaitBundle` operator-gated store legitimately serves the prior schema while its bundle deploys out-of-band, and a `Quiescing` store legitimately serves the current schema while the contract wave waits for the fleet to converge) and only a strict `Drifted` surfaces `PendingModel`, so a dev profile reads `Drifted` informationally while a production open gates on it. The `ContractWave.Spanning` epoch is the `Rasm.AppHost/Runtime` discovery manifest's fold over the per-process `SchemaEpoch`, and the contract-pending classification is `MigrationLaw.Plan`'s generation-time fold recorded on the migration metadata; the gate composes both as settled evidence and re-derives neither.
- Auto: migrations and compiled models are generated facts — the design-time `Optimize`, `ScriptMigration`, `MigrationsBundle`, and `GetMigrations` operations own emission; hand-authored migration code and custom migration-operation types are the deleted patterns; a `MigrationsCodeGeneratorSelector` override is the one seam that swaps emission language without a hand-written generator class; `ReverseEngineerScaffolder`, `ScaffoldContext`, and `ModelReverseEngineerOptions` are the rejected DB-first inversion — the model is the source of truth, never a scaffolded store, so a reverse-engineered context is the named defect. `RelationalDatabaseFacadeExtensions.GetMigrations` reads the assembly ids and `GetAppliedMigrations` reads the `IHistoryRepository` `HistoryRow.MigrationId` set without an open; `HasPendingModelChanges` is the runtime half of the fingerprint gate on a `MigrateOnOpen` writer.
- Receipt: `MigrationReceipt` — profile, applied ids, failed step, `LockHolderPid` and `LeaseEpoch`, compiled `SchemaFingerprint`, the applied-set `SchemaEpoch` deploy stamp, the `PendingTypeReload` wire-type-reload obligation, applied vehicle, elapsed `Duration`, `Instant`, correlation; the `SchemaEpoch` publishes atomically with the open receipt so a cross-process compatibility check is one epoch comparison over the discovery manifest, and a non-empty `PendingTypeReload` proves the deploy is not complete until every live `NpgsqlDataSource` reloads its type cache.
- Packages: Microsoft.EntityFrameworkCore.Design, Microsoft.EntityFrameworkCore.Sqlite, Npgsql.EntityFrameworkCore.PostgreSQL, Npgsql, System.IO.Hashing, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.AppHost (project), BCL inbox (`System.Runtime.InteropServices` for the epoch span hash)
- Growth: one case on `SchemaFault`, one arm on `SchemaVerdict`, one slot on `MigrationReceipt`; the cross-process tolerated-span rule lives at the discovery manifest over the id sequences, this stamp is its `SchemaEpoch.Matches` exact-match identity; zero new surface.
- Boundary: boot computes one total verdict from two id sets plus a drift gate, routed by the `StorePlacement` row — its `ReadOnly` and `MigrateOnOpen` columns decide whether an unknown suffix serves behind or aborts and whether a pending suffix applies, awaits a bundle, or provisions, so the open ceremony never best-effort opens and an older binary's model never silently describes columns it has never seen. Read-ahead serving is legal only under a declared expand-only suite invariant carried on the placement — the unknown suffix is locally unclassifiable, so the sound default is `NewerSchema` rejection and the degradable row is a deployment invariant, never a runtime discovery. `SchemaFingerprint.From` hashes the `IMigrationsAssembly.ModelSnapshot` source through `XxHash3.HashToUInt64`, the store metadata row persists it, and the open ceremony folds the persisted value against the compiled value before any provider open completes. Each migration applies in its own transaction so a mid-set failure leaves an applied prefix, never a torn migration — recovery is idempotent re-apply of the remainder through `apply`, never rollback of the prefix, and the applied-set receipt is monotone, cheap enough to fold on every boot. `MigrateAsync` acquires the provider migration lock itself — hand-acquired `SqliteMigrationDatabaseLock` ceremony and pg advisory-lock acquisition are the deleted patterns, and the public lock surface `IMigrationsDatabaseLock` carries only `HistoryRepository` and no holder identity, so `LockHolderPid` and `LeaseEpoch` fill from the `StoreLeaseRow` first-opener row, never from `Migrations/Internal` types. The model fingerprint and the deploy epoch are two distinct facts: `SchemaFingerprint` proves WHICH compiled model a binary holds (two binaries diverge on a model edit), while `SchemaEpoch` proves WHICH ordered migration-id set is applied (two binaries on identical models still diverge while one rolls its set forward), so the discovery manifest compares epochs to gate a contract-wave migration on fleet convergence and compares fingerprints to gate a model-drift open — neither substitutes for the other. A migration that adds a native pg enum, composite, or extension type leaves the new OID unresolved in every live `NpgsqlDataSource` until it reloads its type cache, so its `ReloadTypes` arm stamps the `MigrationReceipt.PendingTypeReload` set, the `Store/profiles#PROVISIONING_ROWS` `type-resolution` open-proof folds `VerifyTypes` over the live source's resolved `PostgresType` names, and an unresolved type folds `TypeUnresolved` at open rather than failing per-row at first query — the deploy is not complete until the reload lands. Compiled-model adoption is settled — `Schema/converters#CONVERTER_RAIL`'s two-arg `ConverterRail.Compose(options, compiled)` mounts the `dotnet ef dbcontext optimize` frozen model through `UseModel` (the culture-pinned snake-case naming is folded inside `Compose`, not a separate argument) and the snake-case rewrites survive the freeze so a compiled model and a fresh model emit identical column names and migration SQL, identical `ModelSnapshot` bytes, and one fingerprint. Design tooling stays a private asset.

## [03]-[WAVE_PLAN]

- Owner: `WaveClass` `[SmartEnum]` the physical-class vocabulary; `WaveVerdict` `[SmartEnum]` the per-operation verdict carrying its own typed-fault projection as a policy value (`Reject(migrationId, operation) -> Option<SchemaFault>`); `WaveStep` the `(WaveVerdict, operation)` classification row; `RetentionApproval` the destructive-authorization token; `DeployVehicle` `[SmartEnum]` the apply-vehicle axis carrying its `MigrationsSqlGenerationOptions`; `MigrationPlan` the two-wave split value; `MigrationLaw.Plan` the generation-time fold.
- Cases: `WaveClass` — additive, rename, rebuild (the three physical refinements of a non-destructive op; destructiveness is EF's `IsDestructiveChange`, never a class-token row, so no `destructive` member exists); `WaveVerdict` — expand, contract, gated-rewrite, forbidden-middle, rejected-expand, destructive-unapproved, irreversible, unclassified (the last five `Terminal`, each projecting its own `SchemaFault`); `DeployVehicle` — bundle-per-release (fleets), idempotent-script (operator-gated), runtime-apply (single-writer). Five parallel identical `WaveDisposition` record cases collapse to one `WaveStep` row keyed by the `WaveVerdict` tag whose `Reject` policy column owns the whole-migration rejection.
- Entry: `public static Validation<Error, MigrationPlan> Plan(Migration migration, StorePlacement placement, Option<RetentionApproval> approval)` classifies the materialized migration's `UpOperations` against its `DownOperations` and `ActiveProvider` into total `WaveStep` rows, then folds each verdict's `Reject` projection through the applicative `Traverse` so every terminal operation fault accumulates over the base `Error` monoid before reporting (the `SchemaFault` cases combine as the `Error`s they are, per `[VALIDATION_MONOID]` — the failure slot is the canonical `Error`, never a self-monoidal fault); `public static DeployVehicle Vehicle(StorePlacement placement)` selects the apply vehicle as a placement projection; `public static Fin<string> Script(IMigrator migrator, Option<string> from, Option<string> to, DeployVehicle vehicle)` emits the vehicle's `MigrationsSqlGenerationOptions` deploy SQL through `IMigrator.GenerateScript`, folding a down-direction request on any non-`RuntimeApply` vehicle to `IrreversibleUnacknowledged` so a fleet store can only roll forward.
- Auto: `MigrationOperation.IsDestructiveChange` is EF's own per-operation destructive stamp — `DropColumnOperation`/`DropTableOperation` set it true at construction — so the fold reads that flag as the destructive signal and the type-keyed `Physical` table only refines the non-destructive operations into rename/rebuild/additive, never re-deriving destructiveness from the CLR type; a `Physical` miss is an operation with no class token (a raw `SqlOperation` carrying `IsDestructiveChange = false` until its author sets it), which folds the terminal `Unclassified` verdict demanding the author annotate its wave or stamp `IsDestructiveChange` — never a silent additive pass and never a false `destructive` label.
- Packages: Microsoft.EntityFrameworkCore.Design, Microsoft.EntityFrameworkCore.Sqlite, Npgsql.EntityFrameworkCore.PostgreSQL, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime
- Growth: one row on `WaveClass`, one row on `WaveVerdict`, one row on `DeployVehicle`; zero new surface.
- Boundary: every schema change decomposes into an expand wave and a contract wave with a deployment boundary between — expand strictly additive, contract removing only after every old-shape reader retires — and the fold runs at generation time over the materialized `Migration`, never apply-time, because apply-time gating leaves only skip-and-drift or apply-and-lose. The classification is physical and provider-aware, keyed off the `Migration.ActiveProvider` the artifact stamps: a nullable or defaulted `AddColumnOperation` is `Expand`, a required-no-default `AddColumnOperation` is `RejectedExpand`, a `RenameTableOperation`/`RenameColumnOperation` is the `ForbiddenMiddle` — expand plus backfill plus contract instead — an `AlterColumnOperation` on a sqlite `ActiveProvider` (the rebuild-on-alter engine) is `GatedRewrite` because the table-rebuild copies every row, while the same alter on the server provider is in-place, a nullability-tightening `AlterColumnOperation` and any operation EF stamps `IsDestructiveChange` is `Contract` admitted only under a `RetentionApproval` whose `Covers` scopes the migration id and operation and whose `Irreversible` acknowledges loss, or it folds `DestructiveUnapproved`, and a migration mixing waves rejects whole. The backfill between waves is bulk-rail data work, never schema-rail — a row-mass backfill squats on the fleet-wide lock past the health-probe window, so the `MigrationPlan` names the boundary and the backfill rides `Query/pipeline`. A destructive `Up` whose `Down` cannot restore data declares irreversibility — the fold inspects `DownOperations` and demands the `RetentionApproval.Irreversible` acknowledgment when the down body is empty or itself destructive, because a fabricated lossy `Down` is a second destructive operation in disguise. The apply vehicle is a placement projection, never a special tool — `BundlePerRelease` for fleets, `IdempotentScript` (`MigrationsSqlGenerationOptions.Idempotent | NoTransactions`) for operator-gated stores, `RuntimeApply` only on the single-writer row inside the gated lifecycle state; fleet boot apply is rejected even though the lock makes it safe because it grants every instance DDL rights and couples rollout order to schema state. Recovery direction is itself a placement projection: a single-writer embedded store rolls back through the down body (`Script(migrator, from: newer, to: older, …)`) inside its own gated lifecycle, but a fleet `PostgresServer` store rolls FORWARD only — a bad expand is reversed by a new forward migration, never a down artifact, because a down on a fleet whose other members already read the new shape tears live readers — so `Vehicle` gating a `RuntimeApply` placement is the only row that admits a down-direction `Script`, and a fleet placement requesting `to: older` folds `IrreversibleUnacknowledged` rather than emitting a down the fleet cannot safely run. Target `"0"` (`Migration.InitialDatabase`) full teardown is gated like any destructive operation and only on the single-writer row.

## [04]-[LOCK_LIGHT]

- Owner: `LockLightStep` `[Union]` the PG18 weak-lock DDL-step vocabulary, each case projecting its ordered `(Sql, SuppressTransaction)` emission plus its `LockGuard` preflight discipline; `MigrationLaw.LockLight` the ordered emission fold.
- Cases: `LockLightStep` — `NotValidConstraint` (the `ADD CONSTRAINT … CHECK (predicate) NOT VALID` then `VALIDATE CONSTRAINT` two-step), `TwoStep` (the pre-rendered ADD/VALIDATE pair the `Schema/ddl#EXTENSION_DDL` temporal-key `WITHOUT OVERLAPS` non-`CHECK` constraint body emits, exposing `AddSql`/`ValidateSql`), `NotEnforcedCheck` (the deferred `CHECK … NOT ENFORCED` then later `ALTER CONSTRAINT … ENFORCED`), `ConcurrentIndex` (the `CREATE INDEX CONCURRENTLY` non-transactional build), `PartitionInherit` (the `ALTER CONSTRAINT … INHERIT` propagation across the partition tree), `ReloadTypes` (the post-deploy `NpgsqlDataSource.ReloadTypes` type-resolution barrier a native-enum/composite/extension migration leaves for live processes).
- Entry: `public static MigrationBuilder LockLight(MigrationBuilder migration, Seq<LockLightStep> steps, LockBudget budget)` folds each step's ordered SQL into the builder with the correct transaction discipline per arm, prefixing each lock-taking arm with its `LockGuard` builder SQL — a bounded `SET LOCAL lock_timeout` from the `LockBudget` row, and the invalid-index catalog-scan drop for a re-run `CONCURRENTLY` build; `public static Seq<string> PendingTypeReload(Seq<LockLightStep> steps)` collects the wire types the `ReloadTypes` arms leave for the receipt. The `LockBudget.ActivityCeiling`/`StandbyLagCeiling` are consumed by the deploy harness's preflight `pg_stat_activity`/`pg_stat_replication` read (a `Fin` gate over a live read, never builder DDL) so a lock-taking migration never queues behind a slow reporting transaction or starves a lagging replica.
- Packages: Microsoft.EntityFrameworkCore.Design, Npgsql.EntityFrameworkCore.PostgreSQL, Npgsql, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: one arm on `LockLightStep`, one row on `LockBudget`; zero new surface.
- Boundary: a NOT NULL constraint added to a large table rides the `NotValidConstraint` two-step — `ADD CONSTRAINT c CHECK (col IS NOT NULL) NOT VALID` is a fast metadata-only catalog change taking a brief `ShareUpdateExclusiveLock` and stays transactional, then `VALIDATE CONSTRAINT c` runs the full-table scan under a `ShareUpdateExclusiveLock` that never blocks reads or writes — splitting them is the whole point, so the ADD is never `suppressTransaction` and only the validate scan is the lock-light gain. The `Schema/ddl#EXTENSION_DDL` temporal-key path renders a `WITHOUT OVERLAPS` primary/unique/foreign-key constraint body that is not a `CHECK` predicate, so its populated-table two-step is the `TwoStep` case carrying the already-rendered `AddSql`/`ValidateSql` strings — one `LockLightStep` case the temporal-key `Option<LockLightStep>` projection mints directly, never a parallel two-step shape on the DDL side. A deferred invariant rides `NotEnforcedCheck` — `CHECK (…) NOT ENFORCED` admits the constraint without validating existing rows, then a later migration's `ALTER CONSTRAINT … ENFORCED` validates once the backfill lands. A `ConcurrentIndex` build is the one genuinely non-transactional arm — `CREATE INDEX CONCURRENTLY` cannot run inside a transaction block, so its migration carries `suppressTransaction: true` and is single-purpose by construction, the only place the suppression degrades the receipt unit from migration to operation; a re-run after an interrupted build must first drop the left-behind `INVALID` index, so its `LockGuard` carries the `pg_index.indisvalid = false` catalog-scan drop as the retry preamble rather than colliding on the existing-relation name. A `PartitionInherit` step propagates an enforced constraint to a partition tree through `ALTER CONSTRAINT … INHERIT` so the parent constraint covers every child without a per-partition rewrite. The `ReloadTypes` arm is the deploy-completion barrier — a migration that adds a native pg enum, composite, or extension type leaves every live `NpgsqlDataSource` resolving the new OID as unknown until it reloads its type cache, so the arm emits no DDL and instead carries the post-apply `dataSource.ReloadTypes()` obligation the `MigrationReceipt.PendingTypeReload` slot stamps, gated against the `Store/profiles#PROVISIONING_ROWS` `VerifyTypes` resolved-type set so an unresolved type folds `TypeUnresolved` rather than failing per-row at first query. Every lock-taking arm sets a bounded `SET LOCAL lock_timeout` from the `LockBudget` row so the lock-queue trap converts into bounded retry latency the deploy retries on cadence, and a preflight read of long-running activity plus standby replay lag gates the lock-taking arm so a migration never queues behind a reporting transaction or starves a lagging replica — the lock-light gain is forfeit if the ACCESS EXCLUSIVE wait blocks the whole table behind one slow reader. The declarative DDL emission (extensions, generated columns, temporal keys, checks) is `Schema/ddl#EXTENSION_DDL`'s `SchemaDdl.Migrate`/`Sql` owner — `LockLight` owns only the wave-sequencing and lock discipline of weak-lock alterations on existing populated tables, never the declaration of new schema objects, two orthogonal owners meeting at `MigrationBuilder`.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class WaveClass {
    public static readonly WaveClass Additive = new("additive");
    public static readonly WaveClass Rename = new("rename");
    public static readonly WaveClass Rebuild = new("rebuild");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class DeployVehicle {
    public static readonly DeployVehicle BundlePerRelease = new("bundle-per-release", MigrationsSqlGenerationOptions.Default);
    public static readonly DeployVehicle IdempotentScript = new("idempotent-script", MigrationsSqlGenerationOptions.Idempotent | MigrationsSqlGenerationOptions.NoTransactions);
    public static readonly DeployVehicle RuntimeApply = new("runtime-apply", MigrationsSqlGenerationOptions.Default);
    public MigrationsSqlGenerationOptions ScriptOptions { get; }
    private DeployVehicle(string key, MigrationsSqlGenerationOptions scriptOptions) : this(key) => ScriptOptions = scriptOptions;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class WaveVerdict {
    public static readonly WaveVerdict Expand = new("expand", Fault: None);
    public static readonly WaveVerdict Contract = new("contract", Fault: None);
    public static readonly WaveVerdict GatedRewrite = new("gated-rewrite", Fault: None);
    public static readonly WaveVerdict ForbiddenMiddle = new("forbidden-middle", Fault: Some<Func<string, string, SchemaFault>>(static (id, op) => new SchemaFault.ForbiddenRename(id, op)));
    public static readonly WaveVerdict RejectedExpand = new("rejected-expand", Fault: Some<Func<string, string, SchemaFault>>(static (id, op) => new SchemaFault.RejectedExpand(id, op)));
    public static readonly WaveVerdict DestructiveUnapproved = new("destructive-unapproved", Fault: Some<Func<string, string, SchemaFault>>(static (id, op) => new SchemaFault.DestructiveUnapproved(id, op)));
    public static readonly WaveVerdict Irreversible = new("irreversible", Fault: Some<Func<string, string, SchemaFault>>(static (id, op) => new SchemaFault.IrreversibleUnacknowledged(id, op)));
    public static readonly WaveVerdict Unclassified = new("unclassified", Fault: Some<Func<string, string, SchemaFault>>(static (id, op) => new SchemaFault.Unclassified(id, op)));
    private readonly Option<Func<string, string, SchemaFault>> fault;
    public bool Terminal => fault.IsSome;
    public Option<SchemaFault> Reject(string migrationId, string operation) => fault.Map(make => make(migrationId, operation));
    private WaveVerdict(string key, Option<Func<string, string, SchemaFault>> Fault) : this(key) => fault = Fault;
}

public readonly record struct WaveStep(WaveVerdict Verdict, string Operation);

public readonly record struct RetentionApproval(string MigrationId, Seq<string> Operations, string Approver, bool Irreversible, Instant At) {
    public bool Covers(string migrationId, string operation) =>
        MigrationId == migrationId && (Operations.IsEmpty || Operations.Exists(operation.Equals));
}

public sealed record MigrationPlan(string MigrationId, Seq<string> Expand, Seq<string> Contract, Seq<string> Rewrites, DeployVehicle Vehicle, bool BackfillBoundary) {
    public static MigrationPlan From(string migrationId, Seq<WaveStep> steps, DeployVehicle vehicle) {
        var contract = steps.Filter(static s => s.Verdict == WaveVerdict.Contract).Map(static s => s.Operation);
        var rewrites = steps.Filter(static s => s.Verdict == WaveVerdict.GatedRewrite).Map(static s => s.Operation);
        return new(migrationId,
            steps.Filter(static s => s.Verdict == WaveVerdict.Expand).Map(static s => s.Operation),
            contract, rewrites, vehicle,
            BackfillBoundary: !contract.IsEmpty || !rewrites.IsEmpty);
    }
}

public readonly record struct LockBudget(Duration LockTimeout, Duration ActivityCeiling, Duration StandbyLagCeiling) {
    // Bounded so the ACCESS EXCLUSIVE / SHARE UPDATE EXCLUSIVE wait fails fast into deploy-retry latency
    // rather than queueing every reader behind the migration; the activity/standby ceilings gate the
    // preflight so a lock-taking arm never queues behind a slow reporting transaction or starves a lagging replica.
    public static readonly LockBudget Default = new(Duration.FromSeconds(3), Duration.FromMinutes(2), Duration.FromSeconds(30));
    public string LockTimeoutSql => $"SET LOCAL lock_timeout = '{(long)LockTimeout.TotalMilliseconds}ms'";
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LockLightStep {
    private LockLightStep() { }

    // Ordered emission: each tuple is one statement plus its transaction discipline.
    public abstract Seq<(string Sql, bool SuppressTransaction)> Emit { get; }

    // Preflight rendered only for the lock-taking arms; the safe arms return empty so the fold prefixes nothing.
    public virtual Seq<string> LockGuard(LockBudget budget) => Seq<string>();

    // Native-enum/composite/extension migrations leave a type cache to reload before live processes resolve the new OID.
    public virtual Seq<string> PendingTypeReload => Seq<string>();

    public sealed record NotValidConstraint(string Table, string Constraint, string Predicate) : LockLightStep {
        public override Seq<(string, bool)> Emit => Seq(
            ($"ALTER TABLE {Table} ADD CONSTRAINT {Constraint} CHECK ({Predicate}) NOT VALID", false),
            ($"ALTER TABLE {Table} VALIDATE CONSTRAINT {Constraint}", false));
        public override Seq<string> LockGuard(LockBudget budget) => Seq(budget.LockTimeoutSql);
    }
    // The `Schema/ddl#EXTENSION_DDL` temporal-key `WITHOUT OVERLAPS` body is not a CHECK predicate, so the
    // DDL owner renders the ADD/VALIDATE pair and hands it here as one case with the `AddSql`/`ValidateSql` accessors.
    public sealed record TwoStep(string AddSql, string ValidateSql) : LockLightStep {
        public override Seq<(string, bool)> Emit => Seq((AddSql, false), (ValidateSql, false));
        public override Seq<string> LockGuard(LockBudget budget) => Seq(budget.LockTimeoutSql);
    }
    public sealed record NotEnforcedCheck(string Table, string Constraint, string Predicate) : LockLightStep {
        public override Seq<(string, bool)> Emit => Seq(
            ($"ALTER TABLE {Table} ADD CONSTRAINT {Constraint} CHECK ({Predicate}) NOT ENFORCED", false));
    }
    public sealed record ConcurrentIndex(string Index, string Table, string Method, Seq<string> Columns) : LockLightStep {
        public override Seq<(string, bool)> Emit => Seq(
            ($"CREATE INDEX CONCURRENTLY {Index} ON {Table} USING {Method} ({string.Join(", ", Columns)})", true));
        // A re-run after an interrupted concurrent build collides on the left-behind INVALID index name, so the
        // retry preamble drops it first; the bounded lock_timeout is omitted because CONCURRENTLY takes no blocking lock.
        public override Seq<string> LockGuard(LockBudget budget) => Seq(
            $"DROP INDEX CONCURRENTLY IF EXISTS {Index}");
    }
    public sealed record PartitionInherit(string Table, string Constraint) : LockLightStep {
        public override Seq<(string, bool)> Emit => Seq(
            ($"ALTER TABLE {Table} ALTER CONSTRAINT {Constraint} INHERIT", false));
        public override Seq<string> LockGuard(LockBudget budget) => Seq(budget.LockTimeoutSql);
    }
    // No DDL — the deploy-completion barrier carrying the post-apply type-cache reload obligation only;
    // the harness discharges it through `NpgsqlDataSource.ReloadTypesAsync` on every live source.
    public sealed record ReloadTypes(Seq<string> PgTypes) : LockLightStep {
        public override Seq<(string, bool)> Emit => Seq<(string, bool)>();
        public override Seq<string> PendingTypeReload => PgTypes;
    }
}

[Union]
public abstract partial record SchemaFault : Expected, IValidationError<SchemaFault> {
    private SchemaFault(string detail, int code) : base(detail, code, None) { }

    public static SchemaFault Create(string message) => new Text(message);

    public sealed record Text : SchemaFault { public Text(string detail) : base(detail, 5300) { } }
    public sealed record NewerSchema : SchemaFault {
        public NewerSchema(SchemaFingerprint stored, SchemaFingerprint compiled) : base($"store {stored.Value:x16} ahead of compiled {compiled.Value:x16}", 5301) => (Stored, Compiled) = (stored, compiled);
        public SchemaFingerprint Stored { get; }
        public SchemaFingerprint Compiled { get; }
    }
    public sealed record EpochDivergent : SchemaFault {
        public EpochDivergent(SchemaEpoch local, SchemaEpoch fleet) : base($"epoch {local.Value:x16} outside fleet tolerance {fleet.Value:x16}", 5302) => (Local, Fleet) = (local, fleet);
        public SchemaEpoch Local { get; }
        public SchemaEpoch Fleet { get; }
    }
    public sealed record PendingModel : SchemaFault {
        public PendingModel(SchemaFingerprint compiled) : base($"model drift beyond {compiled.Value:x16}", 5303) => Compiled = compiled;
        public SchemaFingerprint Compiled { get; }
    }
    public sealed record PartialApplication : SchemaFault {
        public PartialApplication(Seq<string> applied, string failed) : base($"{failed} after {applied.Count} applied", 5304) => (Applied, Failed) = (applied, failed);
        public Seq<string> Applied { get; }
        public string Failed { get; }
    }
    public sealed record RejectedExpand : SchemaFault {
        public RejectedExpand(string migrationId, string operation) : base($"{migrationId}: {operation} required-no-default add rewrites existing rows — expand nullable then backfill then tighten", 5305) => (MigrationId, Operation) = (migrationId, operation);
        public string MigrationId { get; }
        public string Operation { get; }
    }
    public sealed record DestructiveUnapproved : SchemaFault {
        public DestructiveUnapproved(string migrationId, string operation) : base($"{migrationId}: {operation} destructive without retention approval", 5306) => (MigrationId, Operation) = (migrationId, operation);
        public string MigrationId { get; }
        public string Operation { get; }
    }
    public sealed record ForbiddenRename : SchemaFault {
        public ForbiddenRename(string migrationId, string operation) : base($"{migrationId}: {operation} rename forbidden — expand+backfill+contract instead", 5307) => (MigrationId, Operation) = (migrationId, operation);
        public string MigrationId { get; }
        public string Operation { get; }
    }
    public sealed record IrreversibleUnacknowledged : SchemaFault {
        public IrreversibleUnacknowledged(string migrationId, string operation) : base($"{migrationId}: {operation} irreversible — empty or destructive Down without acknowledgment", 5308) => (MigrationId, Operation) = (migrationId, operation);
        public string MigrationId { get; }
        public string Operation { get; }
    }
    public sealed record TypeUnresolved : SchemaFault {
        public TypeUnresolved(Seq<string> pgTypes) : base($"{pgTypes.Count} wire type(s) unresolved post-deploy — ReloadTypes pending: {string.Join(',', pgTypes)}", 5309) => PgTypes = pgTypes;
        public Seq<string> PgTypes { get; }
    }
    public sealed record AclScopeMismatch : SchemaFault {
        public AclScopeMismatch(UInt128 scope) : base($"acl-scope-mismatch:{scope:x32}", 5310) => Scope = scope;
        public UInt128 Scope { get; }
    }
    public sealed record AclDenied : SchemaFault {
        public AclDenied(string principal, Capability demand, UInt128 scope) : base($"acl-denied:{principal}:{demand}:{scope:x32}", 5311) => (Principal, Demand, Scope) = (principal, demand, scope);
        public string Principal { get; }
        public Capability Demand { get; }
        public UInt128 Scope { get; }
    }
    public sealed record AclExpired : SchemaFault {
        public AclExpired(string principal, Instant lapsedAt) : base($"acl-expired:{principal}:{lapsedAt}", 5312) => (Principal, LapsedAt) = (principal, lapsedAt);
        public string Principal { get; }
        public Instant LapsedAt { get; }
    }
    public sealed record AuthorshipUnauthored : SchemaFault {
        public AuthorshipUnauthored(OpDigest expected, OpDigest found) : base("authorship-digest-mismatch", 5313) => (Expected, Found) = (expected, found);
        public OpDigest Expected { get; }
        public OpDigest Found { get; }
    }
    public sealed record AuthorshipForged : SchemaFault {
        public AuthorshipForged(string actor, string signingKeyId) : base($"authorship-forged:{actor}:{signingKeyId}", 5314) => (Actor, SigningKeyId) = (actor, signingKeyId);
        public string Actor { get; }
        public string SigningKeyId { get; }
    }
    public sealed record OpDigestWidth : SchemaFault {
        public OpDigestWidth(int expected, int actual) : base($"op-digest-width:{expected}:{actual}", 5315) => (Expected, Actual) = (expected, actual);
        public int Expected { get; }
        public int Actual { get; }
    }
    public sealed record Unclassified : SchemaFault {
        public Unclassified(string migrationId, string operation) : base($"{migrationId}: {operation} carries no class token (raw SqlOperation with IsDestructiveChange=false) — annotate its wave or stamp IsDestructiveChange", 5316) => (MigrationId, Operation) = (migrationId, operation);
        public string MigrationId { get; }
        public string Operation { get; }
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SchemaVerdict {
    private SchemaVerdict() { }
    public sealed record Serving : SchemaVerdict;
    public sealed record ServingBehind(Seq<string> Unknown) : SchemaVerdict;
    public sealed record Provisioned(Seq<string> Applied) : SchemaVerdict;
    public sealed record Advanced(Seq<string> Applied) : SchemaVerdict;
    public sealed record AwaitBundle(Seq<string> Pending, bool Fresh) : SchemaVerdict;
    // A contract-wave suffix held back because a live process still spans the pre-contract epoch — the open
    // serves the current schema and the contract applies on a later boot once the fleet quiesces to one epoch.
    public sealed record Quiescing(Seq<string> Pending, SchemaEpoch Spanning) : SchemaVerdict;
    public sealed record Drifted : SchemaVerdict;

    public Fin<Unit> Require(SchemaFingerprint compiled, bool strict) =>
        this is Drifted && strict ? Fin.Fail<Unit>(new SchemaFault.PendingModel(compiled)) : Fin.Succ(unit);
}

public readonly record struct SchemaFingerprint(ulong Value) {
    public static SchemaFingerprint From(ReadOnlySpan<byte> modelSnapshot) => new(XxHash3.HashToUInt64(modelSnapshot));
}

// The applied-migration-id set hashed in order — the deploy stamp published atomically with the open receipt,
// distinct from `SchemaFingerprint` (the compiled-model digest): two binaries on identical models still hold
// different epochs while one rolls its migration set forward, and a contract-wave migration gates on no live
// process spanning the contracted shape by comparing epochs across the discovery manifest.
public readonly record struct SchemaEpoch(ulong Value) {
    public static SchemaEpoch From(Seq<string> appliedInOrder) {
        var hash = new XxHash3(seed: 0);
        appliedInOrder.Iter(id => { hash.Append(MemoryMarshal.AsBytes(id.AsSpan())); hash.Append(" "u8); });
        return new(hash.GetCurrentHashAsUInt64());
    }

    // Exact-match identity only — a scalar hash cannot detect prefix-tolerance, so the expand-only tolerated
    // span is derived at the `Rasm.AppHost/Runtime` discovery manifest over the ordered id sequences, and this
    // epoch is the published stamp that manifest compares: equal epochs are one schema, divergent epochs route
    // to the manifest's id-sequence span check, never to a false prefix claim here.
    public bool Matches(SchemaEpoch other) => Value == other.Value;
}

// The contract-wave hold evidence: a `Some` proves BOTH that the pending suffix carries a contract wave
// AND that a live process still spans the pre-contract `Spanning` epoch, so the gate holds the contract.
// One shape forecloses the illegal split states the prior two-input form admitted (a contracts-pending
// flag with no spanning epoch, or a spanning epoch with no pending contract). The classification of the
// pending suffix into a contract wave is `MigrationLaw.Plan`'s generation-time fold recorded on the
// migration metadata, and the spanning epoch is the `Rasm.AppHost/Runtime` discovery manifest's fold over
// the per-process `SchemaEpoch` — both arrive resolved; the gate never re-derives either.
public readonly record struct ContractWave(SchemaEpoch Spanning);

public sealed record MigrationReceipt(
    StoreProfile Profile,
    Seq<string> Applied,
    Option<string> Failed,
    Option<int> LockHolderPid,
    Option<ulong> LeaseEpoch,
    SchemaFingerprint Compiled,
    SchemaEpoch Epoch,
    Seq<string> PendingTypeReload,
    DeployVehicle Vehicle,
    Duration Elapsed,
    Instant At,
    CorrelationId Correlation) {
    public static MigrationReceipt Stamp(StoreProfile profile, Seq<string> applied, Option<string> failed, Option<StoreLeaseRow> lease, SchemaFingerprint compiled, Seq<string> pendingTypeReload, DeployVehicle vehicle, ClockPolicy clocks, long mark, CorrelationId correlation) =>
        new(profile, applied, failed, lease.Map(static l => l.HolderPid), lease.Map(static l => l.Epoch), compiled, SchemaEpoch.From(applied), pendingTypeReload, vehicle, clocks.Elapsed(mark), clocks.Now, correlation);
}

public static class SchemaGate {
    public static Fin<SchemaVerdict> Admit(Seq<string> applied, Seq<string> known, StorePlacement placement, SchemaFingerprint stored, SchemaFingerprint compiled, bool pendingChanges, Option<ContractWave> contractWave, Func<Fin<Seq<string>>> apply) {
        var unknown = applied.Filter(id => !known.Exists(id.Equals));
        var pending = known.Filter(id => !applied.Exists(id.Equals));
        return (unknown.IsEmpty, pending.IsEmpty) switch {
            (false, _) when placement.ReadOnly => Fin.Succ<SchemaVerdict>(new SchemaVerdict.ServingBehind(unknown)),
            (false, _) => Fin.Fail<SchemaVerdict>(new SchemaFault.NewerSchema(stored, compiled)),
            // A pending contract wave holds while a live process still spans the pre-contract epoch — the open
            // serves the current schema and the contract applies once the fleet quiesces to one epoch. The
            // `ContractWave` evidence proves both facts at once, so no split flag state reaches the gate.
            (_, false) when placement.MigrateOnOpen && contractWave is { IsSome: true, Case: ContractWave held } =>
                Fin.Succ<SchemaVerdict>(new SchemaVerdict.Quiescing(pending, held.Spanning)),
            (_, false) when placement.MigrateOnOpen => apply()
                .MapFail(_ => (Error)new SchemaFault.PartialApplication(applied, pending.HeadOrNone().IfNone(string.Empty)))
                .Map(done => (SchemaVerdict)(applied.IsEmpty ? new SchemaVerdict.Provisioned(done) : new SchemaVerdict.Advanced(done))),
            (_, false) => Fin.Succ<SchemaVerdict>(new SchemaVerdict.AwaitBundle(pending, Fresh: applied.IsEmpty)),
            _ => Fin.Succ<SchemaVerdict>(pendingChanges ? new SchemaVerdict.Drifted() : new SchemaVerdict.Serving()),
        };
    }
}

public static class MigrationLaw {
    private const string SqliteProvider = "Microsoft.EntityFrameworkCore.Sqlite";

    private static readonly FrozenDictionary<Type, Func<MigrationOperation, string, WaveClass>> Physical = new Dictionary<Type, Func<MigrationOperation, string, WaveClass>> {
        [typeof(AddColumnOperation)] = static (_, _) => WaveClass.Additive,
        [typeof(CreateTableOperation)] = static (_, _) => WaveClass.Additive,
        [typeof(CreateIndexOperation)] = static (_, _) => WaveClass.Additive,
        [typeof(AddCheckConstraintOperation)] = static (_, _) => WaveClass.Additive,
        [typeof(RenameColumnOperation)] = static (_, _) => WaveClass.Rename,
        [typeof(RenameTableOperation)] = static (_, _) => WaveClass.Rename,
        [typeof(AlterColumnOperation)] = static (_, provider) => provider == SqliteProvider ? WaveClass.Rebuild : WaveClass.Additive,
    }.ToFrozenDictionary();

    public static Validation<Error, MigrationPlan> Plan(Migration migration, StorePlacement placement, Option<RetentionApproval> approval) {
        var id = MigrationId(migration);
        var reversible = Reversible(migration);
        var provider = migration.ActiveProvider ?? string.Empty;
        var steps = toSeq(migration.UpOperations).Map(op => Step(provider, reversible, id, op, approval));
        // Applicative accumulation over the base `Error` monoid (`Error.Combine`/`Errors.None`): every terminal
        // operation fault collects before reporting, the SchemaFault cases combining as the Errors they are.
        return steps.Traverse(step => step.Verdict.Reject(id, step.Operation)
            .Match(Some: f => Validation<Error, WaveStep>.Fail(f), None: () => Validation<Error, WaveStep>.Success(step))).As()
            .Map(ok => MigrationPlan.From(id, ok.Strict(), Vehicle(placement)));
    }

    public static DeployVehicle Vehicle(StorePlacement placement) =>
        (placement.MigrateOnOpen, placement.Durable == StoreProfile.PostgresServer) switch {
            (true, _) => DeployVehicle.RuntimeApply,
            (false, true) => DeployVehicle.IdempotentScript,
            _ => DeployVehicle.BundlePerRelease,
        };

    // A down-direction script (`to` ordering before `from`) is admitted only on the single-writer `RuntimeApply`
    // row inside its own gated lifecycle; a fleet placement rolls forward only, so a down request folds typed.
    public static Fin<string> Script(IMigrator migrator, Option<string> from, Option<string> to, DeployVehicle vehicle) =>
        IsDownDirection(from, to) && vehicle != DeployVehicle.RuntimeApply
            ? Fin.Fail<string>(new SchemaFault.IrreversibleUnacknowledged(to.IfNone(Migration.InitialDatabase), "fleet rolls forward only — author a forward migration"))
            : Fin.Succ(migrator.GenerateScript(from.IfNoneUnsafe((string?)null), to.IfNoneUnsafe((string?)null), vehicle.ScriptOptions));

    private static bool IsDownDirection(Option<string> from, Option<string> to) =>
        (from, to) switch {
            ({ IsSome: true, Case: string f }, { IsSome: true, Case: string t }) => string.CompareOrdinal(t, f) < 0,
            (_, { IsSome: true, Case: string t }) => string.CompareOrdinal(t, Migration.InitialDatabase) <= 0,
            _ => false,
        };

    public static MigrationBuilder LockLight(MigrationBuilder migration, Seq<LockLightStep> steps, LockBudget budget) =>
        steps.Bind(step => step.LockGuard(budget).Map(static guard => (Sql: guard, Suppress: false))
                                + step.Emit.Map(static e => (Sql: e.Sql, Suppress: e.SuppressTransaction)))
            .Fold(migration, static (builder, op) => {
                builder.Sql(op.Sql, suppressTransaction: op.Suppress);
                return builder;
            });

    public static Seq<string> PendingTypeReload(Seq<LockLightStep> steps) =>
        steps.Bind(static step => step.PendingTypeReload);

    private static WaveStep Step(string provider, bool reversible, string id, MigrationOperation op, Option<RetentionApproval> approval) {
        var name = op.GetType().Name;
        return new(op.IsDestructiveChange
            ? approval.Filter(a => a.Covers(id, name)) is { IsSome: true, Case: RetentionApproval covering }
                ? reversible || covering.Irreversible ? WaveVerdict.Contract : WaveVerdict.Irreversible
                : WaveVerdict.DestructiveUnapproved
            // EF's IsDestructiveChange already stamped non-destructive; the Physical table refines the
            // class token, and a token-less op (a raw SqlOperation carrying IsDestructiveChange=false)
            // is genuinely unclassifiable here — it folds Unclassified for review, never a false Destructive.
            : Physical.TryGetValue(op.GetType(), out var classify)
                ? classify(op, provider).Switch(
                    op,
                    additive: static o => o is ColumnOperation { IsNullable: false, DefaultValue: null, DefaultValueSql: null } ? WaveVerdict.RejectedExpand : WaveVerdict.Expand,
                    rename: static _ => WaveVerdict.ForbiddenMiddle,
                    rebuild: static _ => WaveVerdict.GatedRewrite)
                : WaveVerdict.Unclassified,
            name);
    }

    private static bool Reversible(Migration migration) =>
        toSeq(migration.DownOperations) is { IsEmpty: false } downs && !downs.Exists(static d => d.IsDestructiveChange);

    private static string MigrationId(Migration migration) =>
        migration.GetType().GetCustomAttribute<MigrationAttribute>()?.Id ?? migration.GetType().Name;
}
```

