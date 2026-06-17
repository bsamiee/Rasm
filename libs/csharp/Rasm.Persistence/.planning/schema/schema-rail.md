# [PERSISTENCE_SCHEMA_RAIL]

Schema truth for every store the suite opens. `IdentityPolicy` is the key axis every persisted identifier traces to — one row per generation strategy carrying generator, big-endian transcription, ordering semantics, collision law, and per-provider default SQL — so an identity-row change is an expand-wave second key plus a derivation flip plus a contract-wave drop, never an `AlterColumn`. `SchemaFault` is the typed migration-and-downgrade fault family whose `Gate` guards every open against schema drift and whose `Classify` fold runs at generation time over `MigrationOperation` rows splitting every change into expand and contract waves. `SchemaFingerprint` and `MigrationReceipt` stamp compiled-model and applied-prefix evidence from `ClockPolicy` and `CorrelationId`. `DerivedColumn` rows decide stored-versus-virtual generated columns and emit `HasComputedColumnSql(sql, stored)` plus their co-located CHECK constraints. `SchemaDdl` rows declare the PostgreSQL extension, index, exclusion, temporal-key, check, json-schema, composite, and native-enum surface as one `[Union]` over one `Declare`/`Migrate`/`Validate` fold. `ConverterRail` is the single registration row admitting every generated domain owner, the snake-case naming policy, the frozen NodaTime sqlite pattern table, and the `xmin`/integer concurrency token. The page spine is the two EF Core providers, EFCore.NamingConventions, Thinktecture.Runtime.Extensions.EntityFrameworkCore10, the NodaTime provider plugin, pgvector/pgvectorscale/pg_search, NetTopologySuite, and System.IO.Hashing.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                                    |
| :-----: | ----------------- | :--------------------------------------------------------------------------------------- |
|   [1]   | IDENTITY_POLICY   | key axis, big-endian transcription, identity migration, object ACL, signed authorship    |
|   [2]   | MIGRATION_LAW     | migration faults, fingerprint gate, wave classifier, lock-light steps, receipted apply   |
|   [3]   | GENERATED_COLUMNS | stored-versus-virtual decision, computed-column emission, co-located check constraints    |
|   [4]   | EXTENSION_DDL     | extension, index, exclusion, temporal-key, check, json-schema, composite, native-enum     |
|   [5]   | CONVERTER_RAIL    | converter and naming registration, sqlite pattern table, concurrency token, compiled mount |

## [2]-[IDENTITY_POLICY]

- Owner: `IdentityPolicy` `[SmartEnum<string>]` four rows under the `StoreKeyPolicy` ordinal accessor carrying generator, big-endian transcription, ordering, collision class, clr type, and per-provider default SQL columns; `Collision` `[SmartEnum]` is the collision-posture vocabulary; `ObjectAcl` is the per-object/per-branch capability-and-RBAC grant row; `SignedAuthorship` is the actor-identity attestation tying an op to a blame agent; `Authz` is the static surface folding object-level admission and authorship verification.
- Cases: uuid-v7 (default, B-tree insert-local), uuid-v7-backfill (historical-timestamp mint for deterministic backfill), content-hash (immutable-payload addressing), natural-key (caller-owned identifiers); `Collision` rows are unmintable, content-idempotent, foreign-authority; `ObjectAcl` grants `Read | Write | Delete | Grant | Admin` per principal per object scope; `SignedAuthorship` carries actor, signing key id, op-digest, signature, and `Instant`.
- Entry: `public static Guid NextKey()` mints the uuid-v7 default and `public static Guid BackfilledKey(Instant observed)` mints the deterministic historical surrogate; `public static byte[] Spelled(Guid key)` is the big-endian binary transcription law; `public static UInt128 ContentKey(ReadOnlySpan<byte> content)` derives content identity — pure values; `public static Fin<AclGrant> Admit(ObjectAcl acl, string principal, Seq<string> roles, AclGrant demand, UInt128 scope)` is the object-level admission fold and `public static bool Verify(SignedAuthorship authorship, UInt128 opDigest, Func<string, ReadOnlyMemory<byte>, UInt128, bool> verify)` checks an op's authorship.
- Packages: Thinktecture.Runtime.Extensions, System.IO.Hashing, LanguageExt.Core, NodaTime, Rasm.AppHost (project), BCL inbox
- Growth: one `IdentityPolicy` row (key text, clr type, per-provider default SQL, ordering, collision, client-generated precedence); a v3/v5 namespace key is one future row on the same axis derived through `Guid.CreateVersion7`'s sibling `CreateVersion5`; one `AclGrant` flag per new capability; one `Collision` row per new posture; one `ObjectAcl` scope per new gated object kind; zero new surface.
- Boundary: every persisted key strategy in the package traces to one row here — uuid-ossp is the deleted extension route. The per-provider default-SQL columns feed column defaults as data, and the `ClientGenerated` precedence column resolves the double-generation gate — when it is set the model configures the key column `ValueGeneratedNever` so the client `Guid.CreateVersion7` value is authoritative and the provider default never fires on the same key, while a `false` value defers to the column default for server-minted rows. Ordering survives transcription only when the spelling preserves it — `ToByteArray(bigEndian: true)` is the binary transcription law because the canonical text form is lexically time-ordered but the default little-endian byte export is not, so a binary-keyed primary index without it degrades to random-insert fragmentation; the sqlite `"uuid7()"` leg executes through the native function-registration rows. `uuid_extract_timestamp` makes a v7 key a free coarse creation-time axis so a composite `(low-cardinality discriminant, v7 key)` index stays append-local while PG18 skip scan serves its key-only lookups, deleting the bare-key second index. Identity-row change is never `AlterColumn` — it is an expand-wave second key backfilled by `BackfilledKey`, a derivation flip, and a contract-wave drop, the only identity migration preserving foreign references, changefeed continuity, and cursor validity at once. Content identity is non-cryptographic XxHash128 with no security claim. Object-level authorization is the per-object/per-branch RBAC-plus-capability grant — `ObjectAcl` scopes a grant to a document, a branch (the `version-control#COMMIT_DAG` `BranchAcl` is the branch-scoped projection of this fold), an element-set, or a tenant, and `Admit` folds the principal's direct grants with its role grants so a capability is the union, denying by default — a coarse table-level grant or a tenancy-only gate is the deleted form because the gate scopes to the object. Signed authorship is the actor-identity-to-blame seam — every op carries a `SignedAuthorship` whose signature is over the op digest so a blame attribution (`version-control#TIME_TRAVEL`, `provenance#CAUSAL_DAG`) names a verified actor, not an unauthenticated `Actor` string, and the signing key id resolves through the AppHost identity seam (the signed actor identity is host-resolved, never minted here) so a forged authorship is detectable. The tenancy RLS gate (`provisioning#TENANCY_RLS`) stays the row-level coarse scope and the object-ACL is the fine within-tenant scope, two altitudes never duplicated.

```csharp signature
[Flags]
public enum AclGrant {
    None = 0,
    Read = 1,
    Write = 2,
    Delete = 4,
    Grant = 8,
    Admin = 16,
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class AclScope {
    public static readonly AclScope Document = new("document");
    public static readonly AclScope Branch = new("branch");
    public static readonly AclScope ElementSet = new("element-set");
    public static readonly AclScope Tenant = new("tenant");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class Collision {
    public static readonly Collision Unmintable = new("unmintable");
    public static readonly Collision ContentIdempotent = new("content-idempotent");
    public static readonly Collision ForeignAuthority = new("foreign-authority");
}

[SmartEnum<string>]
[ValidationError<SchemaFault>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
[KeyMemberComparer<StoreKeyPolicy, string>]
public sealed partial class IdentityPolicy {
    public static readonly IdentityPolicy UuidV7Key = new("uuid-v7", clrType: typeof(Guid), Collision.Unmintable, ordered: true, pgDefaultSql: "uuidv7()", sqliteDefaultSql: "uuid7()", clientGenerated: true);
    public static readonly IdentityPolicy UuidV7Backfill = new("uuid-v7-backfill", clrType: typeof(Guid), Collision.Unmintable, ordered: true, pgDefaultSql: null, sqliteDefaultSql: null, clientGenerated: true);
    public static readonly IdentityPolicy ContentHash = new("content-hash", clrType: typeof(UInt128), Collision.ContentIdempotent, ordered: false, pgDefaultSql: null, sqliteDefaultSql: null, clientGenerated: true);
    public static readonly IdentityPolicy NaturalKey = new("natural-key", clrType: typeof(string), Collision.ForeignAuthority, ordered: false, pgDefaultSql: null, sqliteDefaultSql: null, clientGenerated: true);

    private readonly string? pgDefaultSql;
    private readonly string? sqliteDefaultSql;

    public Type ClrType { get; }

    public Collision Collision { get; }

    public bool Ordered { get; }

    public bool ClientGenerated { get; }

    public Option<string> PgDefaultSql => Optional(pgDefaultSql);

    public Option<string> SqliteDefaultSql => Optional(sqliteDefaultSql);

    public static Guid NextKey() => Guid.CreateVersion7();

    public static Guid BackfilledKey(Instant observed) => Guid.CreateVersion7(observed.ToDateTimeOffset());

    public static byte[] Spelled(Guid key) => key.ToByteArray(bigEndian: true);

    public static UInt128 ContentKey(ReadOnlySpan<byte> content) => XxHash128.HashToUInt128(content);
}

public sealed record ObjectAcl(
    UInt128 Scope,
    AclScope Kind,
    HashMap<string, AclGrant> Principals,
    HashMap<string, AclGrant> Roles);

public sealed record SignedAuthorship(
    string Actor,
    string SigningKeyId,
    UInt128 OpDigest,
    ReadOnlyMemory<byte> Signature,
    Instant At);

public static class Authz {
    public static AclGrant Effective(ObjectAcl acl, string principal, Seq<string> roles) =>
        roles.Fold(
            acl.Principals.Find(principal).IfNone(AclGrant.None),
            (acc, role) => acc | acl.Roles.Find(role).IfNone(AclGrant.None));

    public static Fin<AclGrant> Admit(ObjectAcl acl, string principal, Seq<string> roles, AclGrant demand, UInt128 scope) =>
        acl.Scope != scope
            ? Fin.Fail<AclGrant>(new SchemaFault.AclScopeMismatch(scope))
            : Effective(acl, principal, roles) is var grant && (grant & demand) == demand
                ? Fin.Succ(grant)
                : Fin.Fail<AclGrant>(new SchemaFault.AclDenied(principal, demand, scope));

    public static SignedAuthorship Attest(string actor, string signingKeyId, UInt128 opDigest, Func<UInt128, string, ReadOnlyMemory<byte>> sign, ClockPolicy clocks) =>
        new(actor, signingKeyId, opDigest, sign(opDigest, signingKeyId), clocks.Now);

    public static bool Verify(SignedAuthorship authorship, UInt128 opDigest, Func<string, ReadOnlyMemory<byte>, UInt128, bool> verify) =>
        authorship.OpDigest == opDigest && verify(authorship.SigningKeyId, authorship.Signature, opDigest);
}
```

## [3]-[MIGRATION_LAW]

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

## [4]-[GENERATED_COLUMNS]

- Owner: `DerivedColumn` row record carrying the stored-versus-virtual law plus its co-located CHECK constraint; `ColumnInvariant` row record for table-level check constraints that are not column-derived.
- Entry: `public bool Stored` — derived; `Replicated || Indexed` is the whole decision; `public PropertyBuilder<T> Apply(PropertyBuilder<T> property)` emits `HasComputedColumnSql(Sql, stored: Stored)` and `public EntityTypeBuilder<T> Constrain(EntityTypeBuilder<T> entity)` folds each `ColumnInvariant` into `ToTable(t => t.HasCheckConstraint(name, sql))`.
- Packages: Npgsql.EntityFrameworkCore.PostgreSQL, Microsoft.EntityFrameworkCore.Sqlite
- Growth: one `DerivedColumn` row per derived projection; one `ColumnInvariant` row per table-level check; zero new surface.
- Boundary: the pg provider emits VIRTUAL generated columns by default at the Npgsql-v10 provider floor over PG18 — `HasComputedColumnSql(sql, stored: false)` emits VIRTUAL when `Stored` is false and PG18 is the target — and the sqlite provider emits its own VIRTUAL/STORED form. A STORED column exists only where the `Stored` flag derives true — a hot predicate on a virtual column is the promotion signal to stored-plus-index. A replicated row is STORED so the logical-replication publication's `publish_generated_columns` field carries it; an indexed row is STORED so the index reads materialized bytes; tsvector search columns are `DerivedColumn` rows whose instances land on their owning lane through `HasGeneratedTsVectorColumn` rather than a hand-spelled `to_tsvector` computed column where the language config is a model fact. A derived projection computed in application code beside a store-computed twin is the deleted pattern — one `Sql` expression owns the derivation. Document-shape invariants and range/sign bounds are `ColumnInvariant` rows so a `CHECK` lands as a model fact emitted through `HasCheckConstraint`, never deploy-time hand DDL beside the migration set; a geodesic-planar twin rides the generated-column law as a stored planar projection feeding the hot planar predicate while the geography column holds geodesic truth.

```csharp signature
public sealed record ColumnInvariant(string Name, string Sql);

public sealed record DerivedColumn(string Table, string Column, string Sql, bool Replicated, bool Indexed, Seq<ColumnInvariant> Checks = default) {
    public bool Stored => Replicated || Indexed;

    public PropertyBuilder<T> Apply<T>(PropertyBuilder<T> property) =>
        property.HasComputedColumnSql(Sql, stored: Stored);

    public EntityTypeBuilder<TEntity> Constrain<TEntity>(EntityTypeBuilder<TEntity> entity) where TEntity : class =>
        Checks.Fold(entity, static (builder, check) =>
            builder.ToTable(table => table.HasCheckConstraint(check.Name, check.Sql)));
}
```

## [5]-[EXTENSION_DDL]

- Owner: `SchemaDdl` `[Union]` declaration-row family with the frozen `Extensions` row set; the `Extension` case carries `AccessMethod`, `PreloadGated`, `Cascade`, and `Fallback` columns so one row owns the extension's install DDL, its index access method, its `shared_preload_libraries` gate, and its app-side degradation path; the `Index` case carries method, operator-class, `Include`, `NullsDistinct`, and a `With` build-option map; `TemporalShape` is the temporal-key shape vocabulary.
- Cases: Extension, Index, Exclusion, TemporalKey, Check, JsonSchemaCheck, Composite, Enum — extension declarations with access-method/preload/cascade/fallback metadata, method-and-operator-class index rows carrying a `With` option map for `diskann`/`bm25`/`hnsw` build parameters plus `Include` covering columns and `NullsDistinct` single-null uniqueness, btree_gist exclusion-constraint rows, PG18 WITHOUT OVERLAPS temporal primary-key and foreign-key rows, free-form CHECK rows, pg_jsonschema document-validation CHECK rows, PostgreSQL composite-type declarations, native PostgreSQL enum-type declarations.
- Entry: `public static ModelBuilder Declare(ModelBuilder model)` is the total fold of the non-preload extension and enum rows into model annotations; `public static MigrationBuilder Migrate(MigrationBuilder migration)` emits preload-gated `CREATE EXTENSION` DDL plus temporal-key, check, and JSON-schema SQL; `public static Fin<MigrationBuilder> Validate(MigrationBuilder migration, Func<string, bool> extensionAvailable)` folds each fallback-bearing extension, degrading to its app-side path when the deploy image lacks the pgrx-compiled extension and failing typed otherwise.
- Auto: `HasPostgresExtension` annotations flow through `AlterDatabaseOperation` into generated migration DDL — `CreatePostgresExtensionOperation` is the deleted phantom spelling. `HasPostgresEnum` annotations flow through `PostgresEnum` into the same `AlterDatabaseOperation` so a native enum column emits a `CREATE TYPE ... AS ENUM` step rather than a check-constrained text column. A `TemporalKey` row, a `Check` row, and a `JsonSchemaCheck` row emit through `MigrationBuilder.Sql` because the WITHOUT OVERLAPS clause and the `jsonb_matches_schema` predicate have no first-party EF translator; preload-gated rows (`pg_search`) emit their `CreateSql` through `Migrate` because `HasPostgresExtension` cannot encode the `shared_preload_libraries` prerequisite, and `CASCADE` rows (`vectorscale`) emit `CREATE EXTENSION IF NOT EXISTS vectorscale CASCADE` so the `vector` dependency installs in one step.
- Packages: Npgsql.EntityFrameworkCore.PostgreSQL, Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite, Pgvector.EntityFrameworkCore, Pgvector, Npgsql, JsonSchema.Net, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: one `SchemaDdl` row — a new extension is one `Extension` entry (its access method, preload gate, cascade flag, and fallback path are columns on the same row, never a sibling provisioning type), a new index family is one `Index` row carrying its operator class, `Include` covering columns, `NullsDistinct`, and `With` build-option map, a temporal-versioned table is one `TemporalKey` row, a server-validated document lane is one `JsonSchemaCheck` row, a table invariant is one `Check` row, a native pg enum is one `Enum` entry; zero new surface.
- Boundary: extension capability is one `Extension` row with its full DDL story in columns — `vectorscale` rides `AccessMethod: "diskann"` and `Cascade: true` so `CREATE EXTENSION IF NOT EXISTS vectorscale CASCADE` pulls the `vector` dependency, its diskann index lands as an `Index` row whose `With` map carries `storage_layout`/`num_neighbors`/`search_list_size`/`max_alpha`/`num_dimensions`/`num_bits_per_dimension` against `vector_cosine_ops`/`vector_l2_ops`/`vector_ip_ops`, and the query-time `diskann.query_search_list_size`/`diskann.query_rescore` GUCs belong to the search lane not this declaration row; `pg_search` rides `AccessMethod: "bm25"` and `PreloadGated: true` so it never enters `HasPostgresExtension` (`shared_preload_libraries = 'pg_search'` must precede `CREATE EXTENSION pg_search`, so `Migrate` emits it), its one-per-table bm25 index lands as an `Index` row keyed by the `key_field` UNIQUE/primary column listed first in `With`, and the `pdb.*` query builders, `|||`/`&&&`/`===`/`###` column operators, `pdb.score`/`pdb.snippet` projections, and `::pdb.fuzzy`/`::pdb.boost` casts belong to the search lane — the removed `paradedb.*` namespace is the deleted phantom spelling, every search predicate uses the `pdb.*` namespace; an hnsw vector index rides `Method: "hnsw"` with `m`/`ef_construction` in the `With` map; the self-provisioned non-preload `pg_jsonschema` carries `Fallback: "Json.Schema.JsonSchema.Evaluate"` so `Validate` degrades the document lane to JsonSchema.Net in-process evaluation when the deploy image lacks the pgrx-compiled extension, never silently dropping validation. The `Surface` column states the driver-native cost of each row and built-in ranges and multiranges map to `NpgsqlRange<T>` with zero extension entry. `Index` rows carry the method, operator-class, `Include`, `NullsDistinct`, and `With` columns that the per-column lane policies instantiate, and a compound `Index` row leading with the tenant/partition discriminant serves both keyset cursors and single-column filters through the PG18 automatic B-tree skip scan so a redundant single-column index is the deleted form; `Include` columns leave the key as covering payload and `NullsDistinct: false` is the single-null uniqueness law deleting the partial-index workaround. `Exclusion` rows ride btree_gist for range non-overlap, and `TemporalKey` rows ride the PG18 WITHOUT OVERLAPS shape over a `tstzrange` valid-time column GiST-backed by btree_gist for scalar equality — `PRIMARY KEY (id, valid_period WITHOUT OVERLAPS)`, `UNIQUE (... WITHOUT OVERLAPS)`, and `FOREIGN KEY (cust_id, PERIOD valid_period) REFERENCES parent (id, PERIOD parent_period)` are the bitemporal-versioning structural fence for geospatial-sync and multi-tenant history, the migration emits the constraint and a destructive temporal-key change rides the `DestructiveUnapproved` retention gate. `Check` rows declare free-form table invariants and `JsonSchemaCheck` rows declare the `CHECK (jsonb_matches_schema(<schema>, <column>))` document-lane invariant so the document lane validates server-side, with `Validate` carrying the in-process fallback when the server-side extension is absent. The postgis row makes NetTopologySuite the pg boundary projection of the canonical proto wire geometry; earthdistance is rejected — the postgis row owns geodesy. `Composite` rows declare PostgreSQL composite types — `MapComposites` folds each onto the data-source builder through `MapComposite(Type, name)` so the round-trip type registration is one row, never a per-type hand-written reader. `Enum` rows declare native PostgreSQL enum types symmetric with `Composite` — `MapEnums` folds each onto the data-source builder through the generic `MapEnum<TEnum>(pgName, translator)` resolver while `Declare` folds `HasPostgresEnum<TEnum>(name)` so the model annotation and the type registration trace to one row, never a per-enum hand-written `MapEnum` call beside a hand-written check constraint, and the `EnableUnmappedTypes` builder column on the pg profile row admits enum-as-text round-trips without an `Enum` entry where a native type is unwarranted. The composite and enum sets are empty until a real landmark exists, the cases are shaped for the family they absorb.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
[KeyMemberComparer<StoreKeyPolicy, string>]
public sealed partial class TemporalShape {
    public static readonly TemporalShape PrimaryKey = new("primary-key");
    public static readonly TemporalShape Unique = new("unique");
    public static readonly TemporalShape ForeignKey = new("foreign-key");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SchemaDdl {
    private SchemaDdl() { }

    public sealed record Extension(
        string Name,
        string Surface,
        Option<string> AccessMethod = default,
        bool PreloadGated = false,
        bool Cascade = false,
        Option<string> Fallback = default) : SchemaDdl {
        public string CreateSql => Cascade
            ? $"CREATE EXTENSION IF NOT EXISTS {Name} CASCADE"
            : $"CREATE EXTENSION IF NOT EXISTS {Name}";
    }
    public sealed record Index(string Table, Seq<string> Columns, string Method, Option<string> Operators, Seq<string> Include = default, Option<bool> NullsDistinct = default, Map<string, string> With = default) : SchemaDdl {
        private string ColumnList => string.Join(", ", Columns.Map(c => Operators.Match(Some: ops => $"{c} {ops}", None: () => c)));
        private string IncludeClause => Include.IsEmpty ? "" : $" INCLUDE ({string.Join(", ", Include)})";
        private string NullsClause => NullsDistinct.Match(Some: static distinct => distinct ? "" : " NULLS NOT DISTINCT", None: static () => "");
        private string WithClause => With.IsEmpty ? "" : $" WITH ({string.Join(", ", With.AsEnumerable().Map(static kv => $"{kv.Key} = {kv.Value}"))})";
        public string Sql => $"CREATE INDEX ON {Table} USING {Method} ({ColumnList}){IncludeClause}{NullsClause}{WithClause}";
    }
    public sealed record Exclusion(string Table, string Predicate, string Method) : SchemaDdl {
        public string Sql => $"ALTER TABLE {Table} ADD EXCLUDE USING {Method} ({Predicate})";
    }
    public sealed record TemporalKey(string Table, Seq<string> Columns, string Period, TemporalShape Shape, Option<(string Table, Seq<string> Columns, string Period)> References) : SchemaDdl {
        public string Sql => Shape.Switch(
            state: this,
            primaryKey: static self => $"ALTER TABLE {self.Table} ADD PRIMARY KEY ({string.Join(", ", self.Columns)}, {self.Period} WITHOUT OVERLAPS)",
            unique: static self => $"ALTER TABLE {self.Table} ADD UNIQUE ({string.Join(", ", self.Columns)}, {self.Period} WITHOUT OVERLAPS)",
            foreignKey: static self => self.References.Match(
                Some: parent => $"ALTER TABLE {self.Table} ADD FOREIGN KEY ({string.Join(", ", self.Columns)}, PERIOD {self.Period}) REFERENCES {parent.Table} ({string.Join(", ", parent.Columns)}, PERIOD {parent.Period})",
                None: () => string.Empty));
    }
    public sealed record Check(string Table, string Constraint, string Predicate) : SchemaDdl {
        public string Sql => $"ALTER TABLE {Table} ADD CONSTRAINT {Constraint} CHECK ({Predicate})";
    }
    public sealed record JsonSchemaCheck(string Table, string Column, string Constraint, string Schema) : SchemaDdl {
        public string Sql => $"ALTER TABLE {Table} ADD CONSTRAINT {Constraint} CHECK (jsonb_matches_schema('{Schema}', {Column}))";
    }
    public sealed record Composite(string Name, Type ClrType) : SchemaDdl;
    public sealed record Enum(string Name, Type ClrType, Func<NpgsqlDataSourceBuilder, string, INpgsqlNameTranslator?, NpgsqlDataSourceBuilder> Map, Func<ModelBuilder, ModelBuilder> Annotate) : SchemaDdl {
        public static Enum Of<TEnum>(string name) where TEnum : struct, System.Enum =>
            new(name, typeof(TEnum),
                static (builder, pgName, translator) => builder.MapEnum<TEnum>(pgName, translator),
                static model => model.HasPostgresEnum<TEnum>(name));
    }

    public static readonly Seq<Composite> Composites = Seq<Composite>();

    public static readonly Seq<Enum> Enums = Seq<Enum>();

    public static readonly Seq<Extension> Extensions = Seq(
        new Extension("pg_trgm", Surface: "sql-functions"),
        new Extension("btree_gin", Surface: "ddl-only"),
        new Extension("btree_gist", Surface: "ddl-only"),
        new Extension("citext", Surface: "string"),
        new Extension("hstore", Surface: "Dictionary<string,string>"),
        new Extension("ltree", Surface: "LTree"),
        new Extension("intarray", Surface: "int[]"),
        new Extension("pgcrypto", Surface: "sql-functions"),
        new Extension("fuzzystrmatch", Surface: "sql-functions"),
        new Extension("unaccent", Surface: "sql-functions"),
        new Extension("cube", Surface: "NpgsqlCube"),
        new Extension("tablefunc", Surface: "sql-functions"),
        new Extension("amcheck", Surface: "sql-functions"),
        new Extension("pg_prewarm", Surface: "sql-functions"),
        new Extension("pg_visibility", Surface: "sql-functions"),
        new Extension("pg_logicalinspect", Surface: "sql-functions"),
        new Extension("postgres_fdw", Surface: "ddl-only"),
        new Extension("pg_jsonschema", Surface: "sql-functions", Fallback: "Json.Schema.JsonSchema.Evaluate"),
        new Extension("vector", Surface: "Pgvector.Vector"),
        new Extension("postgis", Surface: "NetTopologySuite.Geometry"),
        new Extension("vectorscale", Surface: "Pgvector.Vector", AccessMethod: "diskann", Cascade: true),
        new Extension("pg_search", Surface: "bm25-search", AccessMethod: "bm25", PreloadGated: true));

    public static readonly Seq<TemporalKey> TemporalKeys = Seq<TemporalKey>();

    public static readonly Seq<Check> Checks = Seq<Check>();

    public static readonly Seq<JsonSchemaCheck> JsonSchemaChecks = Seq<JsonSchemaCheck>();

    public static NpgsqlDataSourceBuilder MapComposites(NpgsqlDataSourceBuilder builder) =>
        Composites.Fold(builder, static (mapper, row) => mapper.MapComposite(row.ClrType, row.Name));

    public static NpgsqlDataSourceBuilder MapEnums(NpgsqlDataSourceBuilder builder) =>
        Enums.Fold(builder, static (mapper, row) => row.Map(mapper, row.Name, null));

    public static ModelBuilder Declare(ModelBuilder model) =>
        Enums.Fold(
            Extensions.Filter(static row => !row.PreloadGated).Fold(model, static (builder, row) => builder.HasPostgresExtension(row.Name)),
            static (builder, row) => row.Annotate(builder));

    public static MigrationBuilder Migrate(MigrationBuilder migration) =>
        JsonSchemaChecks.Fold(
            Checks.Fold(
                TemporalKeys.Filter(static row => row.Sql.Length > 0).Fold(
                    Extensions.Filter(static row => row.PreloadGated).Fold(migration, static (builder, row) => { builder.Sql(row.CreateSql); return builder; }),
                    static (builder, row) => { builder.Sql(row.Sql); return builder; }),
                static (builder, row) => { builder.Sql(row.Sql); return builder; }),
            static (builder, row) => { builder.Sql(row.Sql); return builder; });

    public static Fin<MigrationBuilder> Validate(MigrationBuilder migration, Func<string, bool> extensionAvailable) =>
        Extensions
            .Filter(static row => row.Fallback.IsSome)
            .Fold(
                Fin.Succ(migration),
                (acc, row) => acc.Bind(builder =>
                    extensionAvailable(row.Name)
                        ? Fin.Succ(builder)
                        : row.Fallback.Match(
                            Some: _ => Fin.Succ(builder),
                            None: () => Fin.Fail<MigrationBuilder>(SchemaFault.Create($"<extension-unavailable:{row.Name}>")))));
}
```

## [6]-[CONVERTER_RAIL]

- Owner: `ConverterRail` static composition surface owning the converter and naming registration, the frozen `SqlitePatterns` NodaTime round-trip table, the concurrency-token rows, and the compiled-model mount.
- Entry: `public static DbContextOptionsBuilder Compose(DbContextOptionsBuilder options)` is the one registration row every profile's options delegate folds in; `public static DbContextOptionsBuilder Compose(DbContextOptionsBuilder options, IModel compiled)` mounts the frozen model; `public static string SqliteText<T>(T value)` and `public static Fin<T> SqliteValue<T>(string text)` round-trip a temporal CLR value through the keyed pattern table.
- Auto: `ThinktectureValueConverterFactory` converters cover every `[ValueObject]`, `[SmartEnum]`, and keyed `[Union]` column behind `UseThinktectureValueConverters` — zero hand-written converter classes; a single declared property converts through `HasThinktectureValueConverter`, a complex-type property through `ComplexTypePropertyBuilderExtensions`, and a primitive-collection element through `PrimitiveCollectionBuilderExtensions`, so a per-column conversion is one builder call, never a converter class.
- Packages: Thinktecture.Runtime.Extensions.EntityFrameworkCore10, EFCore.NamingConventions, Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime, NodaTime, LanguageExt.Core
- Growth: one policy value per naming override; a new generated domain owner costs zero converter code on the same registration row; a sqlite temporal column is one `SqlitePatterns` row; zero new surface.
- Boundary: `UseSnakeCaseNamingConvention` is the single naming policy — the `CamelCase`, `LowerCase`, `UpperCase`, and `UpperSnakeCase` rewriters are the named rejected conventions because a second casing fractures the schema fingerprint, and hand-written provider naming patches and per-entity converter classes are the deleted patterns. Key-column width rides the converter registration's `Configuration` value — `SmartEnumConfiguration` bounds smart-enum columns and `KeyedValueObjectConfiguration` bounds keyed value-object columns to a declared max-length and `Configuration.NoMaxLength` is the rejected unbounded form; `Configuration.Default` is the bounded selection. Pg temporal columns ride the profile row's `UseNodaTime` option mapping `Instant` to `timestamptz`, `LocalDate` to `date`, `LocalTime` to `time`, `OffsetTime` to `timetz`, `Duration`/`Period` to `interval`, and `Interval`/`DateInterval` to range/multirange; sqlite temporal columns persist NodaTime pattern text under the same convention so no `DateTime` sentinel reaches a store — `SqlitePatterns` is the frozen pattern table the sqlite converter rows trace to, keyed by CLR type, so a `Duration` median or p95 statistic rides `DurationPattern.Roundtrip` (the round-tripping "o" pattern preserving sign and sub-second), a date-only column rides `LocalDatePattern.Iso`, a time-only column rides `LocalTimePattern.ExtendedIso` (full sub-second precision), instants ride `InstantPattern.ExtendedIso`, local timestamps ride `LocalDateTimePattern.ExtendedIso`, offset timestamps ride `OffsetDateTimePattern.ExtendedIso`, and a period column rides `PeriodPattern.Roundtrip` (unit-preserving round-trip), so each temporal column round-trips as ISO text rather than fall back to a BCL `DateTime` column; `ZonedDateTime` is the one type whose pattern statics are format-only, so a zoned sqlite column rides `ZonedDateTimePattern.CreateWithInvariantCulture("uuuu'-'MM'-'dd'T'HH':'mm':'ss;FFFFFFFFF z", DateTimeZoneProviders.Tzdb)` to be parse-capable — the bare `GeneralFormatOnlyIso`/`ExtendedFormatOnlyIso` statics cannot read back. A hand-written `DateTime`-typed sqlite temporal column is the deleted form. Concurrency tokens are schema facts declared here — the pg row maps the system `xmin` column through `UseXminAsConcurrencyToken()` and the sqlite row carries an integer version column bumped per write — while the provider-exception fault projection belongs to the query rail. The compiled-model mount is the two-argument `Compose(options, compiled)` overload feeding `UseModel`, where `compiled` is the `dotnet ef dbcontext optimize` codegen output whose internal `IModelRuntimeInitializer.Initialize` step bakes the `UseSnakeCaseNamingConvention` rewrites into the frozen model before emission, so a fresh-built model and a compiled model emit byte-identical column names and migration SQL — the compiled-model fast path is drop-in beside the convention with no dual naming path, no post-compile fixup, and no per-column rename, and a hand-applied naming patch on the compiled model is the deleted form.

```csharp signature
public static class ConverterRail {
    public static readonly FrozenDictionary<Type, IPattern<object>> SqlitePatterns = new Dictionary<Type, IPattern<object>> {
        [typeof(Instant)] = Boxed(InstantPattern.ExtendedIso),
        [typeof(LocalDate)] = Boxed(LocalDatePattern.Iso),
        [typeof(LocalTime)] = Boxed(LocalTimePattern.ExtendedIso),
        [typeof(LocalDateTime)] = Boxed(LocalDateTimePattern.ExtendedIso),
        [typeof(OffsetDateTime)] = Boxed(OffsetDateTimePattern.ExtendedIso),
        [typeof(ZonedDateTime)] = Boxed(ZonedDateTimePattern.CreateWithInvariantCulture("uuuu'-'MM'-'dd'T'HH':'mm':'ss;FFFFFFFFF z", DateTimeZoneProviders.Tzdb)),
        [typeof(Duration)] = Boxed(DurationPattern.Roundtrip),
        [typeof(Period)] = Boxed(PeriodPattern.Roundtrip),
    }.ToFrozenDictionary();

    public static DbContextOptionsBuilder Compose(DbContextOptionsBuilder options) =>
        options.UseSnakeCaseNamingConvention().UseThinktectureValueConverters(Configuration.Default);

    public static DbContextOptionsBuilder Compose(DbContextOptionsBuilder options, IModel compiled) =>
        Compose(options).UseModel(compiled);

    public static string SqliteText<T>(T value) where T : struct =>
        SqlitePatterns[typeof(T)].Format(value);

    public static Fin<T> SqliteValue<T>(string text) where T : struct =>
        SqlitePatterns[typeof(T)].Parse(text) is { Success: true, Value: T parsed }
            ? Fin.Succ(parsed)
            : Fin.Fail<T>(SchemaFault.Create($"<temporal-parse:{typeof(T).Name}:{text}>"));

    private static IPattern<object> Boxed<T>(IPattern<T> pattern) =>
        new BoxedPattern<T>(pattern);

    private sealed record BoxedPattern<T>(IPattern<T> Inner) : IPattern<object> {
        public ParseResult<object> Parse(string text) => Inner.Parse(text) switch {
            { Success: true, Value: T value } => ParseResult<object>.ForValue(value!),
            var failed => ParseResult<object>.ForException(() => failed.Exception),
        };
        public string Format(object value) => Inner.Format((T)value);
        public StringBuilder AppendFormat(object value, StringBuilder builder) => Inner.AppendFormat((T)value, builder);
    }
}
```

## [7]-[RESEARCH]

- [AUTHORSHIP_SIGNING_KEY] [SPIKE]: the AppHost-resolved signing-key seam the `Authz.Attest`/`Verify` fold consumes — the host credential-store key handle (macOS keychain / DPAPI / credential store) the signing key id resolves through, proven by tier-1 decompile member-shape only because an unattended credential-store read prompts the operator, with the op-digest signature algorithm and the public-key verification confirmed against the host identity seam at the integrated host. This is the sole residual tier-3 owner on the page (DENSITY_BAR axis [39]); the ACL fold and the authorship record shape are fence-complete, only the OS credential read stays SPIKE.
