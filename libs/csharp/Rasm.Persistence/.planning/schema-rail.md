# [PERSISTENCE_SCHEMA_RAIL]

Schema truth for every store the suite opens: `IdentityPolicy` is the three-row key axis every persisted identifier traces to, `SchemaFault` carries the typed migration and downgrade fault family whose `Gate` guards every open against schema drift, `SchemaFingerprint` and `MigrationReceipt` stamp compiled-model and migration evidence from `ClockPolicy` and `CorrelationId`, `DerivedColumn` rows decide stored-versus-virtual generated columns, `SchemaDdl` rows declare the PostgreSQL extension surface, and `ConverterRail` is the single registration row admitting every generated domain owner and the snake-case naming policy. The page spine is the two EF Core providers, EFCore.NamingConventions, Thinktecture.Runtime.Extensions.EntityFrameworkCore10, the NodaTime provider plugin, and System.IO.Hashing.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]         | [OWNS]                                                       |
| :-----: | ----------------- | ------------------------------------------------------------ |
|   [1]   | IDENTITY_POLICY   | Three-row key axis with per-provider default SQL             |
|   [2]   | MIGRATION_LAW     | Migration faults, fingerprint gate, receipted apply ceremony |
|   [3]   | GENERATED_COLUMNS | Stored-versus-virtual decision for derived projections       |
|   [4]   | EXTENSION_DDL     | Extension, index, exclusion, composite, native-enum declaration rows |
|   [5]   | CONVERTER_RAIL    | One converter and naming registration row                    |

## [2]-[IDENTITY_POLICY]

- Owner: `IdentityPolicy` `[SmartEnum<string>]` three rows under the `StoreKeyPolicy` ordinal accessor; `ObjectAcl` is the per-object/per-branch capability-and-RBAC grant row; `SignedAuthorship` is the actor-identity attestation tying an op to a blame agent; `Authz` is the static surface folding object-level admission and authorship verification.
- Cases: uuid-v7 (default), content-hash, natural-key — uuid-v7 orders B-tree inserts, content-hash addresses immutable payloads, natural-key admits caller-owned identifiers; `ObjectAcl` grants `Read | Write | Delete | Grant | Admin` per principal per object scope; `SignedAuthorship` carries the actor, the signing key id, and the op-digest signature.
- Entry: `public static Guid NextKey()` mints the uuid-v7 default; `public static UInt128 ContentKey(ReadOnlySpan<byte> content)` derives content identity — pure values; `public static Fin<AclGrant> Admit(ObjectAcl acl, string principal, Seq<string> roles, AclGrant demand, UInt128 scope)` is the object-level admission fold and `public static bool Verify(SignedAuthorship authorship, UInt128 opDigest, Func<string, ReadOnlyMemory<byte>> publicKey)` checks an op's authorship.
- Packages: Thinktecture.Runtime.Extensions, System.IO.Hashing, LanguageExt.Core, NodaTime, Rasm.AppHost (project), BCL inbox
- Growth: one `IdentityPolicy` row (key text, clr type, per-provider default SQL, client-generated precedence); a v3/v5 namespace key is one future row on the same axis; one `AclGrant` flag per new capability; one `ObjectAcl` scope per new gated object kind; zero new surface.
- Boundary: every persisted key strategy in the package traces to one row here — uuid-ossp is the deleted extension route; the per-provider default-SQL columns feed column defaults as data, and the `ClientGenerated` precedence column resolves the double-generation gate — when it is set the model configures the key column `ValueGeneratedNever` so the client `Guid.CreateVersion7` value is authoritative and the provider default never fires on the same key, while a `false` value defers to the column default for server-minted rows; the sqlite `"uuid7()"` leg executes through the native function-registration rows; content identity is non-cryptographic XxHash128 with no security claim; object-level authorization is the per-object/per-branch RBAC-plus-capability grant — `ObjectAcl` scopes a grant to a document, a branch (the `version-control#COMMIT_DAG` `BranchAcl` is the branch-scoped projection of this fold), an element-set, or a tenant, and `Admit` folds the principal's direct grants with its role grants so a capability is the union, denying by default — a coarse table-level grant or a tenancy-only gate is the deleted form because the gate scopes to the object; signed authorship is the actor-identity-to-blame seam — every op carries a `SignedAuthorship` whose signature is over the op digest so a blame attribution (`version-control#TIME_TRAVEL`, `provenance#CAUSAL_DAG`) names a verified actor, not an unauthenticated `Actor` string, and the signing key id resolves through the AppHost identity seam (the signed actor identity is host-resolved, never minted here) so a forged authorship is detectable; the tenancy RLS gate (`server-tier#TENANCY_RLS`) stays the row-level coarse scope and the object-ACL is the fine within-tenant scope, two altitudes never duplicated.

```csharp signature
[SmartEnum<string>]
[ValidationError<SchemaFault>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
[KeyMemberComparer<StoreKeyPolicy, string>]
public sealed partial class IdentityPolicy {
    public static readonly IdentityPolicy UuidV7Key = new("uuid-v7", clrType: typeof(Guid), pgDefaultSql: "uuidv7()", sqliteDefaultSql: "uuid7()", clientGenerated: true);
    public static readonly IdentityPolicy ContentHash = new("content-hash", clrType: typeof(UInt128), pgDefaultSql: null, sqliteDefaultSql: null, clientGenerated: true);
    public static readonly IdentityPolicy NaturalKey = new("natural-key", clrType: typeof(string), pgDefaultSql: null, sqliteDefaultSql: null, clientGenerated: true);

    private readonly string? pgDefaultSql;
    private readonly string? sqliteDefaultSql;

    public Type ClrType { get; }

    public bool ClientGenerated { get; }

    public Option<string> PgDefaultSql => Optional(pgDefaultSql);

    public Option<string> SqliteDefaultSql => Optional(sqliteDefaultSql);

    public static Guid NextKey() => Guid.CreateVersion7();

    public static UInt128 ContentKey(ReadOnlySpan<byte> content) => XxHash128.HashToUInt128(content);
}

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
            ? Fin.Fail<AclGrant>(Error.New($"<acl-scope-mismatch:{scope:x32}>"))
            : Effective(acl, principal, roles) is var grant && (grant & demand) == demand
                ? Fin.Succ(grant)
                : Fin.Fail<AclGrant>(Error.New($"<acl-denied:{principal}:{demand}:{scope:x32}>"));

    public static SignedAuthorship Attest(string actor, string signingKeyId, UInt128 opDigest, Func<UInt128, string, ReadOnlyMemory<byte>> sign, ClockPolicy clocks) =>
        new(actor, signingKeyId, opDigest, sign(opDigest, signingKeyId), clocks.Now);

    public static bool Verify(SignedAuthorship authorship, UInt128 opDigest, Func<string, ReadOnlyMemory<byte>, UInt128, bool> verify) =>
        authorship.OpDigest == opDigest && verify(authorship.SigningKeyId, authorship.Signature, opDigest);
}
```

## [3]-[MIGRATION_LAW]

- Owner: `SchemaFault` `[Union]` fault family on the doctrine `Expected` shape with the dual-tier `Create` contract; `SchemaFingerprint` compiled-model fingerprint; `MigrationReceipt` record.
- Cases: Text, NewerSchema, PendingModel, PartialApplication, DestructiveUnapproved — codes 5300-5304.
- Entry: `public static Fin<Unit> Gate(Seq<string> applied, Seq<string> known, SchemaFingerprint stored, SchemaFingerprint compiled, bool pendingChanges)` — `Fin<Unit>` aborts into `SchemaFault`.
- Auto: migrations and compiled models are generated facts — the design-time `Optimize`, `ScriptMigration`, `MigrationsBundle`, and `GetMigrations` operations own emission; hand-authored migration code and custom migration-operation types are the deleted patterns; a `MigrationsCodeGeneratorSelector` override is the one seam that swaps emission language without a hand-written generator class; `ReverseEngineerScaffolder`, `ScaffoldContext`, and `ModelReverseEngineerOptions` are the rejected DB-first inversion — the model is the source of truth, never a scaffolded store, so a reverse-engineered context is the named defect.
- Receipt: `MigrationReceipt` — profile, applied ids, failed step, lock holder, compiled fingerprint, elapsed `Duration`, `Instant`, correlation.
- Packages: Microsoft.EntityFrameworkCore.Design, Microsoft.EntityFrameworkCore.Sqlite, Npgsql.EntityFrameworkCore.PostgreSQL, System.IO.Hashing, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.AppHost (project)
- Growth: one case on `SchemaFault` or one slot on `MigrationReceipt`; zero new surface.
- Boundary: one migrations source emits two provider SQL generators through `MigrationsAssembly`; `MigrateAsync` acquires the provider migration lock itself — hand-acquired `SqliteMigrationDatabaseLock` ceremony and pg advisory-lock acquisition are the deleted patterns, and the public lock surface `IMigrationsDatabaseLock` is a bare disposable handle carrying no holder identity, so `LockHolder` fills from the `StoreLeaseRow` first-opener row, never from `Migrations/Internal` types; a store history ahead of the compiled assembly folds to `NewerSchema`, never best-effort open; `HasPendingModelChanges` feeds `Gate` on the development profile only; `SchemaFingerprint.From` hashes the compiled model snapshot, the store metadata row persists it, and the open ceremony folds the persisted value through `Gate` before any provider open completes; service deploys ride idempotent `ScriptMigration` output and `MigrationsBundle` artifacts; compiled-model adoption is settled — `ConverterRail.Compose(options, compiled)` mounts the `dotnet ef dbcontext optimize` frozen model through `UseModel` and the snake-case rewrites survive the freeze so a compiled model and a fresh model emit identical column names and migration SQL; expand precedes contract, and a destructive step lands only behind a retention-approval receipt or folds to `DestructiveUnapproved`; a NOT NULL constraint added to a large table rides the PG18 lock-light two-step — `ADD CONSTRAINT c NOT NULL (col) NOT VALID` then `VALIDATE CONSTRAINT c` so the validate scan takes no full-table AccessExclusiveLock — and a deferred CHECK rides `CHECK (...) NOT ENFORCED` with `ALTER CONSTRAINT ... INHERIT` for the partition tree, both emitted as `MigrationBuilder.Sql` steps inside one migration; design tooling stays a private asset.

```csharp signature
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
        public DestructiveUnapproved(string migrationId) : base($"{migrationId}: destructive step without retention approval", 5304) => MigrationId = migrationId;
        public string MigrationId { get; }
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
```

## [4]-[GENERATED_COLUMNS]

- Owner: `DerivedColumn` row record carrying the stored-versus-virtual law.
- Entry: `public bool Stored` — derived; `Replicated || Indexed` is the whole decision.
- Packages: Npgsql.EntityFrameworkCore.PostgreSQL, Microsoft.EntityFrameworkCore.Sqlite
- Growth: one `DerivedColumn` row per derived projection; zero new surface.
- Boundary: the pg provider emits VIRTUAL generated columns by default at the Npgsql-v10 provider floor over PG18 — the provider emits VIRTUAL when `stored:true` is omitted and PG18 is the target — and the sqlite provider emits its own VIRTUAL/STORED form; a STORED column exists only where the `Stored` flag derives true; a replicated row is STORED so the logical-replication publication's publish_generated_columns field carries it; an indexed row is STORED so the index reads materialized bytes; tsvector search columns are `DerivedColumn` rows whose instances land on their owning lane; a derived projection computed in application code beside a store-computed twin is the deleted pattern — one `Sql` expression owns the derivation.

```csharp signature
public sealed record DerivedColumn(string Table, string Column, string Sql, bool Replicated, bool Indexed) {
    public bool Stored => Replicated || Indexed;
}
```

## [5]-[EXTENSION_DDL]

- Owner: `SchemaDdl` `[Union]` declaration-row family with the frozen `Extensions` row set.
- Cases: Extension, Index, Exclusion, TemporalKey, JsonSchemaCheck, Composite, Enum — extension declarations, method-and-operator-class index rows, btree_gist exclusion-constraint rows, PG18 WITHOUT OVERLAPS temporal primary-key and foreign-key rows, pg_jsonschema document-validation CHECK rows, PostgreSQL composite-type declarations, native PostgreSQL enum-type declarations.
- Entry: `public static ModelBuilder Declare(ModelBuilder model)` — total fold of the extension and enum rows into model annotations.
- Auto: `HasPostgresExtension` annotations flow through `AlterDatabaseOperation` into generated migration DDL — `CreatePostgresExtensionOperation` is the deleted phantom spelling; `HasPostgresEnum` annotations flow through `PostgresEnum` into the same `AlterDatabaseOperation` so a native enum column emits a `CREATE TYPE ... AS ENUM` step rather than a check-constrained text column; a `TemporalKey` row and a `JsonSchemaCheck` row emit through `MigrationBuilder.Sql` because the WITHOUT OVERLAPS clause and the `jsonb_matches_schema` CHECK have no first-party EF translator.
- Packages: Npgsql.EntityFrameworkCore.PostgreSQL, Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite, Pgvector.EntityFrameworkCore, Npgsql, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: one `SchemaDdl` row — a new extension is one `Extension` entry, a new index family is one `Index` row carrying its operator class, a temporal-versioned table is one `TemporalKey` row, a server-validated document lane is one `JsonSchemaCheck` row, a native pg enum is one `Enum` entry; zero new surface.
- Boundary: preload-gated extensions never enter `Extensions` — their capability verification belongs to the provisioning table; only the self-provisioned non-preload pg_jsonschema joins the frozen set beside pg_trgm/pgcrypto, gated on the deploy image supplying the pgrx-compiled extension and falling back to application-side validation with the row cut where the image cannot; the `Surface` column states the driver-native cost of each row and built-in ranges and multiranges map to `NpgsqlRange<T>` with zero extension entry; `Index` rows carry the method and operator-class columns (gin, gist) that the per-column lane policies instantiate, and a compound `Index` row leading with the tenant/partition discriminant serves both keyset cursors and single-column filters through the PG18 automatic B-tree skip scan so a redundant single-column index is the deleted form; `Exclusion` rows ride btree_gist for range non-overlap, and `TemporalKey` rows ride the PG18 WITHOUT OVERLAPS shape over a `tstzrange` valid-time column GiST-backed by btree_gist for scalar equality — `PRIMARY KEY (id, valid_period WITHOUT OVERLAPS)`, `UNIQUE (... WITHOUT OVERLAPS)`, and `FOREIGN KEY (cust_id, PERIOD valid_period) REFERENCES parent (id, PERIOD parent_period)` are the bitemporal-versioning structural fence for geospatial-sync and multi-tenant history, the migration emits the constraint and a destructive temporal-key change rides the `DestructiveUnapproved` retention gate; `JsonSchemaCheck` rows declare the `CHECK (jsonb_matches_schema(<schema>, <column>))` document-lane invariant so the document lane validates server-side rather than nothing; the postgis row makes NetTopologySuite the pg boundary projection of the canonical proto wire geometry; earthdistance is rejected — the postgis row owns geodesy; `Composite` rows declare PostgreSQL composite types — `MapComposites` folds each onto the data-source builder through `MapComposite` so the round-trip type registration is one row, never a per-type hand-written reader; `Enum` rows declare native PostgreSQL enum types symmetric with `Composite` — `MapEnums` folds each onto the data-source builder through the generic `MapEnum<TEnum>` resolver while `Declare` folds `HasPostgresEnum` so the model annotation and the type registration trace to one row, never a per-enum hand-written `MapEnum` call beside a hand-written check constraint, and the `EnableUnmappedTypes` builder column on the pg profile row admits enum-as-text round-trips without an `Enum` entry where a native type is unwarranted; the composite and enum sets are empty until a real landmark exists, the cases are shaped for the family they absorb.

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

    public sealed record Extension(string Name, string Surface) : SchemaDdl;
    public sealed record Index(string Table, Seq<string> Columns, string Method, Option<string> Operators) : SchemaDdl;
    public sealed record Exclusion(string Table, string Predicate, string Method) : SchemaDdl;
    public sealed record TemporalKey(string Table, Seq<string> Columns, string Period, TemporalShape Shape, Option<(string Table, Seq<string> Columns, string Period)> References) : SchemaDdl {
        public string Sql => Shape.Switch(
            state: this,
            primaryKey: static self => $"ALTER TABLE {self.Table} ADD PRIMARY KEY ({string.Join(", ", self.Columns)}, {self.Period} WITHOUT OVERLAPS)",
            unique: static self => $"ALTER TABLE {self.Table} ADD UNIQUE ({string.Join(", ", self.Columns)}, {self.Period} WITHOUT OVERLAPS)",
            foreignKey: static self => self.References.Match(
                Some: parent => $"ALTER TABLE {self.Table} ADD FOREIGN KEY ({string.Join(", ", self.Columns)}, PERIOD {self.Period}) REFERENCES {parent.Table} ({string.Join(", ", parent.Columns)}, PERIOD {parent.Period})",
                None: () => string.Empty));
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
        new Extension("pg_jsonschema", Surface: "sql-functions"),
        new Extension("vector", Surface: "Pgvector.Vector"),
        new Extension("postgis", Surface: "NetTopologySuite.Geometry"));

    public static readonly Seq<TemporalKey> TemporalKeys = Seq<TemporalKey>();

    public static readonly Seq<JsonSchemaCheck> JsonSchemaChecks = Seq<JsonSchemaCheck>();

    public static NpgsqlDataSourceBuilder MapComposites(NpgsqlDataSourceBuilder builder) =>
        Composites.Fold(builder, static (mapper, row) => mapper.MapComposite(row.ClrType, row.Name));

    public static NpgsqlDataSourceBuilder MapEnums(NpgsqlDataSourceBuilder builder) =>
        Enums.Fold(builder, static (mapper, row) => row.Map(mapper, row.Name, null));

    public static ModelBuilder Declare(ModelBuilder model) =>
        Enums.Fold(
            Extensions.Fold(model, static (builder, row) => builder.HasPostgresExtension(row.Name)),
            static (builder, row) => row.Annotate(builder));

    public static MigrationBuilder Migrate(MigrationBuilder migration) =>
        JsonSchemaChecks.Fold(
            TemporalKeys.Filter(static row => row.Sql.Length > 0).Fold(migration, static (builder, row) => { builder.Sql(row.Sql); return builder; }),
            static (builder, row) => { builder.Sql(row.Sql); return builder; });
}
```

## [6]-[CONVERTER_RAIL]

- Owner: `ConverterRail` static composition surface.
- Entry: `public static DbContextOptionsBuilder Compose(DbContextOptionsBuilder options)` — the one registration row every profile's options delegate folds in.
- Auto: `ThinktectureValueConverterFactory` converters cover every `[ValueObject]`, `[SmartEnum]`, and keyed `[Union]` column behind `UseThinktectureValueConverters` — zero hand-written converter classes; a single declared property converts through `HasThinktectureValueConverter`, a complex-type property through `ComplexTypePropertyBuilderExtensions`, and a primitive-collection element through `PrimitiveCollectionBuilderExtensions`, so a per-column conversion is one builder call, never a converter class.
- Packages: Thinktecture.Runtime.Extensions.EntityFrameworkCore10, EFCore.NamingConventions, Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime, NodaTime
- Growth: one policy value per naming override; a new generated domain owner costs zero converter code on the same registration row; a sqlite temporal column is one `NodaTime` pattern row; zero new surface.
- Boundary: `UseSnakeCaseNamingConvention` is the single naming policy — the `CamelCase`, `LowerCase`, `UpperCase`, and `UpperSnakeCase` rewriters are the named rejected conventions because a second casing fractures the schema fingerprint, and hand-written provider naming patches and per-entity converter classes are the deleted patterns; key-column width rides the converter registration's `Configuration` value — `SmartEnumConfiguration` bounds smart-enum columns and `KeyedValueObjectConfiguration` bounds keyed value-object columns to a declared max-length and `NoMaxLength` is the rejected unbounded form; pg temporal columns ride the profile row's `UseNodaTime` option and sqlite temporal columns persist NodaTime pattern text through the `instant_iso` collation under the same convention so no `DateTime` sentinel reaches a store — `SqlitePatterns` is the frozen pattern table the sqlite converter rows trace to, keyed by CLR type so a `Duration` median or p95 statistic rides `DurationPattern`, a date-only column rides `LocalDatePattern`, a time-only column rides `LocalTimePattern`, instants ride `InstantPattern`, and ranged temporal columns ride `ZonedDateTimePattern`, `OffsetDateTimePattern`, `LocalDateTimePattern`, and `PeriodPattern`, so each temporal column round-trips as ISO text rather than fall back to a BCL `DateTime` column; a hand-written `DateTime`-typed sqlite temporal column is the deleted form; concurrency tokens are schema facts declared here — the pg row maps the system `xmin` column and the sqlite row carries an integer version column bumped per write — while the provider-exception fault projection belongs to the query rail; the compiled-model mount is the two-argument `Compose(options, compiled)` overload feeding `UseModel`, where `compiled` is the `dotnet ef dbcontext optimize` codegen output whose internal `IModelRuntimeInitializer.Initialize` step bakes the `UseSnakeCaseNamingConvention` rewrites into the frozen model before emission, so a fresh-built model and a compiled model emit byte-identical column names and migration SQL — the compiled-model fast path is drop-in beside the convention with no dual naming path, no post-compile fixup, and no per-column rename, and a hand-applied naming patch on the compiled model is the deleted form.

```csharp signature
public static class ConverterRail {
    public static readonly FrozenDictionary<Type, object> SqlitePatterns = new Dictionary<Type, object> {
        [typeof(Instant)] = InstantPattern.ExtendedIso,
    }.ToFrozenDictionary();

    public static DbContextOptionsBuilder Compose(DbContextOptionsBuilder options) =>
        options.UseSnakeCaseNamingConvention().UseThinktectureValueConverters();

    public static DbContextOptionsBuilder Compose(DbContextOptionsBuilder options, IModel compiled) =>
        Compose(options).UseModel(compiled);
}
```

## [7]-[RESEARCH]

- [AUTHORSHIP_SIGNING_KEY]: the AppHost-resolved signing-key seam the `Authz.Attest`/`Verify` fold consumes — the host credential-store key handle (macOS keychain / DPAPI / credential store) the signing key id resolves through, proven by tier-1 decompile member-shape only because an unattended credential-store read prompts the operator, with the op-digest signature algorithm and the public-key verification confirmed against the host identity seam at the integrated host.
- [COMPOSITE_DDL]: the `MapComposite` runtime overload arity for a `(Type, name)` pair and the migration-side `CREATE TYPE` emission path for a `SchemaDdl.Composite` row — whether the EF model annotation surface emits the composite-type DDL or a manual `MigrationBuilder.Sql` row carries it; the `Composites` set stays empty until a real composite landmark resolves this.
- [ENUM_DDL]: the `MapEnum<TEnum>` data-source-builder overload arity carrying the `(pgName, INpgsqlNameTranslator)` pair and the migration-side `CREATE TYPE ... AS ENUM` emission path for a `SchemaDdl.Enum` row through `HasPostgresEnum<TEnum>` — whether the `PostgresEnum` model annotation emits the enum-type DDL directly; the `Enums` set stays empty until a native pg enum landmark resolves this.
- [TEMPORAL_PATTERN_MEMBERS]: the exact NodaTime round-trip static pattern instance per CLR type for the `SqlitePatterns` table — `DurationPattern`, `PeriodPattern`, `LocalDatePattern`, `LocalTimePattern`, `ZonedDateTimePattern`, and `OffsetDateTimePattern` carry one ISO round-trip instance each beside the confirmed `InstantPattern.ExtendedIso`, resolved against the installed NodaTime assembly before the per-type rows land beside the instant row.
