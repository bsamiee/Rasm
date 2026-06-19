# [PERSISTENCE_SCHEMA_MIGRATION]

Migration law for every store the suite opens. `SchemaFault` is the typed migration-and-downgrade fault family whose `Gate` guards every open against schema drift and whose `Classify` fold runs at generation time over `MigrationOperation` rows splitting every change into expand and contract waves. `SchemaFingerprint` and `MigrationReceipt` stamp compiled-model and applied-prefix evidence from `ClockPolicy` and `CorrelationId`, so every open proves which schema prefix it ran against.

## [1]-[INDEX]

- [1]-[MIGRATION_LAW]: migration faults, fingerprint gate, wave classifier, lock-light steps, and receipted apply.

## [2]-[MIGRATION_LAW]

- Owner: `SchemaFault` `[Union]` fault family on the doctrine `Expected` shape with the dual-tier `Create` contract; `WaveClass` `[SmartEnum]` the physical-class vocabulary; `WaveDisposition` `[Union]` the per-operation verdict; `SchemaFingerprint` the compiled-model fingerprint; `MigrationReceipt` the applied-prefix receipt; `MigrationLaw` the static fold surface.
- Cases: `SchemaFault` — Text, NewerSchema, PendingModel, PartialApplication, DestructiveUnapproved, ForbiddenRename, AclScopeMismatch, AclDenied — codes 5300-5307; `WaveClass` — additive, rename, rebuild, destructive; `WaveDisposition` — Expand, Contract, ForbiddenMiddle, RejectedExpand, GatedRewrite.
- Entry: `public static Fin<Unit> Gate(Seq<string> applied, Seq<string> known, SchemaFingerprint stored, SchemaFingerprint compiled, bool pendingChanges)` aborts a drifted open into `SchemaFault`; `public static Validation<SchemaFault, Seq<WaveDisposition>> Classify(string migrationId, Seq<MigrationOperation> ups, Option<RetentionApproval> approval)` is the generation-time wave fold over operations; `public static MigrationBuilder LockLight(MigrationBuilder migration, Seq<LockLightStep> steps)` emits the PG18 NOT VALID / NOT ENFORCED two-step.
- Auto: migrations and compiled models are generated facts — the design-time `Optimize`, `ScriptMigration`, `MigrationsBundle`, and `GetMigrations` operations own emission; hand-authored migration code and custom migration-operation types are the deleted patterns; a `MigrationsCodeGeneratorSelector` override is the one seam that swaps emission language without a hand-written generator class; `ReverseEngineerScaffolder`, `ScaffoldContext`, and `ModelReverseEngineerOptions` are the rejected DB-first inversion — the model is the source of truth, never a scaffolded store, so a reverse-engineered context is the named defect.
- Receipt: `MigrationReceipt` — profile, applied ids, failed step, lock holder, compiled fingerprint, elapsed `Duration`, `Instant`, correlation.
- Packages: Microsoft.EntityFrameworkCore.Design, Microsoft.EntityFrameworkCore.Sqlite, Npgsql.EntityFrameworkCore.PostgreSQL, System.IO.Hashing, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.AppHost (project)
- Growth: one case on `SchemaFault`, one row on `WaveClass`, one slot on `MigrationReceipt`; zero new surface.
- Boundary: one migrations source emits two provider SQL generators through `MigrationsAssembly`; `MigrateAsync` acquires the provider migration lock itself — hand-acquired `SqliteMigrationDatabaseLock` ceremony and pg advisory-lock acquisition are the deleted patterns, and the public lock surface `IMigrationsDatabaseLock` is a bare disposable handle carrying no holder identity, so `LockHolder` fills from the `StoreLeaseRow` first-opener row, never from `Migrations/Internal` types. A store history ahead of the compiled assembly folds to `NewerSchema`, never best-effort open; `HasPendingModelChanges` feeds `Gate` on the development profile only; `SchemaFingerprint.From` hashes the compiled model snapshot, the store metadata row persists it, and the open ceremony folds the persisted value through `Gate` before any provider open completes. Service deploys ride idempotent `ScriptMigration` output and `MigrationsBundle` artifacts; compiled-model adoption is settled — `ConverterRail.Compose(options, compiled)` mounts the `dotnet ef dbcontext optimize` frozen model through `UseModel` and the snake-case rewrites survive the freeze so a compiled model and a fresh model emit identical column names and migration SQL. Every schema change decomposes into an expand wave and a contract wave with a deployment boundary between — `Classify` runs at generation time over `UpOperations` because apply-time gating leaves only skip-and-drift or apply-and-lose: a nullable or defaulted `AddColumnOperation` is `Expand`, a required-no-default `AddColumnOperation` is `RejectedExpand`, a `RenameTableOperation`/`RenameColumnOperation` is the `ForbiddenMiddle`, an `AlterColumnOperation` on a rebuilding engine is `GatedRewrite`, a nullability-tightening `AlterColumnOperation` or any `DropColumnOperation`/`DropTableOperation` is `Contract` admitted only under a per-migration `RetentionApproval` or folds `DestructiveUnapproved`, and an unclassified `SqlOperation` defaults destructive worst-case. A NOT NULL constraint added to a large table rides the PG18 lock-light two-step — `ADD CONSTRAINT c NOT NULL (col) NOT VALID` then `VALIDATE CONSTRAINT c` so the validate scan takes no full-table AccessExclusiveLock — and a deferred CHECK rides `CHECK (...) NOT ENFORCED` with `ALTER CONSTRAINT ... INHERIT` for the partition tree, both emitted as `MigrationBuilder.Sql` steps inside one migration by `LockLight`; concurrent index builds are single-purpose non-transactional migrations carrying `suppressTransaction: true`. Design tooling stays a private asset.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class WaveClass {
    public static readonly WaveClass Additive = new("additive");
    public static readonly WaveClass Rename = new("rename");
    public static readonly WaveClass Rebuild = new("rebuild");
    public static readonly WaveClass Destructive = new("destructive");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WaveDisposition {
    private WaveDisposition() { }
    public sealed record Expand(string Operation) : WaveDisposition;
    public sealed record Contract(string Operation) : WaveDisposition;
    public sealed record GatedRewrite(string Operation) : WaveDisposition;
    public sealed record ForbiddenMiddle(string Operation) : WaveDisposition;
    public sealed record RejectedExpand(string Operation) : WaveDisposition;
}

public readonly record struct RetentionApproval(string MigrationId, string Approver, Instant At);

public readonly record struct LockLightStep(string AddSql, string ValidateSql);

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
    public sealed record PendingModel : SchemaFault {
        public PendingModel(SchemaFingerprint compiled) : base($"model drift beyond {compiled.Value:x16}", 5302) => Compiled = compiled;
        public SchemaFingerprint Compiled { get; }
    }
    public sealed record PartialApplication : SchemaFault {
        public PartialApplication(Seq<string> applied, string failed) : base($"{failed} after {applied.Count} applied", 5303) => (Applied, Failed) = (applied, failed);
        public Seq<string> Applied { get; }
        public string Failed { get; }
    }
    public sealed record DestructiveUnapproved : SchemaFault {
        public DestructiveUnapproved(string migrationId, string operation) : base($"{migrationId}: {operation} destructive without retention approval", 5304) => (MigrationId, Operation) = (migrationId, operation);
        public string MigrationId { get; }
        public string Operation { get; }
    }
    public sealed record ForbiddenRename : SchemaFault {
        public ForbiddenRename(string migrationId, string operation) : base($"{migrationId}: {operation} rename forbidden — expand+backfill+contract instead", 5305) => (MigrationId, Operation) = (migrationId, operation);
        public string MigrationId { get; }
        public string Operation { get; }
    }
    public sealed record AclScopeMismatch : SchemaFault {
        public AclScopeMismatch(UInt128 scope) : base($"acl-scope-mismatch:{scope:x32}", 5306) => Scope = scope;
        public UInt128 Scope { get; }
    }
    public sealed record AclDenied : SchemaFault {
        public AclDenied(string principal, AclGrant demand, UInt128 scope) : base($"acl-denied:{principal}:{demand}:{scope:x32}", 5307) => (Principal, Demand, Scope) = (principal, demand, scope);
        public string Principal { get; }
        public AclGrant Demand { get; }
        public UInt128 Scope { get; }
    }

    public static Fin<Unit> Gate(Seq<string> applied, Seq<string> known, SchemaFingerprint stored, SchemaFingerprint compiled, bool pendingChanges) =>
        applied.Exists(id => !known.Exists(id.Equals)) ? Fin.Fail<Unit>(new NewerSchema(stored, compiled))
        : pendingChanges ? Fin.Fail<Unit>(new PendingModel(compiled))
        : Fin.Succ(unit);
}

public readonly record struct SchemaFingerprint(ulong Value) {
    public static SchemaFingerprint From(ReadOnlySpan<byte> modelSnapshot) => new(XxHash3.HashToUInt64(modelSnapshot));
}

public sealed record MigrationReceipt(
    StoreProfile Profile,
    Seq<string> Applied,
    Option<string> Failed,
    Option<string> LockHolder,
    SchemaFingerprint Compiled,
    Duration Elapsed,
    Instant At,
    CorrelationId Correlation) {
    public static MigrationReceipt Stamp(StoreProfile profile, Seq<string> applied, Option<string> failed, Option<string> lockHolder, SchemaFingerprint compiled, ClockPolicy clocks, long mark, CorrelationId correlation) =>
        new(profile, applied, failed, lockHolder, compiled, clocks.Elapsed(mark), clocks.Now, correlation);
}

public static class MigrationLaw {
    private static readonly FrozenDictionary<Type, Func<MigrationOperation, WaveClass>> Physical = new Dictionary<Type, Func<MigrationOperation, WaveClass>> {
        [typeof(AddColumnOperation)] = static op => ((AddColumnOperation)op) is { IsNullable: false, DefaultValue: null, DefaultValueSql: null } ? WaveClass.Destructive : WaveClass.Additive,
        [typeof(CreateTableOperation)] = static _ => WaveClass.Additive,
        [typeof(CreateIndexOperation)] = static _ => WaveClass.Additive,
        [typeof(RenameColumnOperation)] = static _ => WaveClass.Rename,
        [typeof(RenameTableOperation)] = static _ => WaveClass.Rename,
        [typeof(AlterColumnOperation)] = static op => ((AlterColumnOperation)op) is { IsNullable: false, OldColumn.IsNullable: true } ? WaveClass.Destructive : WaveClass.Rebuild,
        [typeof(DropColumnOperation)] = static _ => WaveClass.Destructive,
        [typeof(DropTableOperation)] = static _ => WaveClass.Destructive,
    }.ToFrozenDictionary();

    public static Validation<SchemaFault, Seq<WaveDisposition>> Classify(string migrationId, Seq<MigrationOperation> ups, Option<RetentionApproval> approval) =>
        ups.Traverse(op => Disposition(migrationId, op, approval).ToValidation()).Map(static rows => rows.As().Strict()).As();

    private static Fin<WaveDisposition> Disposition(string migrationId, MigrationOperation op, Option<RetentionApproval> approval) =>
        (Physical.TryGetValue(op.GetType(), out var classify) ? classify(op) : WaveClass.Destructive).Switch(
            state: (Id: migrationId, Op: op.GetType().Name, Approval: approval),
            additive: static s => Fin.Succ<WaveDisposition>(new WaveDisposition.Expand(s.Op)),
            rename: static s => Fin.Fail<WaveDisposition>(new SchemaFault.ForbiddenRename(s.Id, s.Op)),
            rebuild: static s => Fin.Succ<WaveDisposition>(new WaveDisposition.GatedRewrite(s.Op)),
            destructive: static s => s.Approval.IsSome
                ? Fin.Succ<WaveDisposition>(new WaveDisposition.Contract(s.Op))
                : Fin.Fail<WaveDisposition>(new SchemaFault.DestructiveUnapproved(s.Id, s.Op)));

    public static MigrationBuilder LockLight(MigrationBuilder migration, Seq<LockLightStep> steps) =>
        steps.Fold(migration, static (builder, step) => {
            builder.Sql(step.AddSql, suppressTransaction: true);
            builder.Sql(step.ValidateSql, suppressTransaction: true);
            return builder;
        });
}
```

