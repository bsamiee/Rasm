# [PERSISTENCE_SCHEMA_RAIL]

Schema truth for every store the suite opens: `IdentityPolicy` is the three-row key axis every persisted identifier traces to, `SchemaFault` carries the typed migration and downgrade fault family whose `Gate` guards every open against schema drift, `SchemaFingerprint` and `MigrationReceipt` stamp compiled-model and migration evidence from `ClockPolicy` and `CorrelationId`, `DerivedColumn` rows decide stored-versus-virtual generated columns, `SchemaDdl` rows declare the PostgreSQL extension surface, and `ConverterRail` is the single registration row admitting every generated domain owner and the snake-case naming policy. The page spine is the two EF Core providers, EFCore.NamingConventions, Thinktecture.Runtime.Extensions.EntityFrameworkCore10, the NodaTime provider plugin, and System.IO.Hashing.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]         | [OWNS]                                                       |
| :-----: | ----------------- | ------------------------------------------------------------ |
|   [1]   | IDENTITY_POLICY   | Three-row key axis with per-provider default SQL              |
|   [2]   | MIGRATION_LAW     | Migration faults, fingerprint gate, receipted apply ceremony  |
|   [3]   | GENERATED_COLUMNS | Stored-versus-virtual decision for derived projections        |
|   [4]   | EXTENSION_DDL     | Declared extension rows, index method and operator classes    |
|   [5]   | CONVERTER_RAIL    | One converter and naming registration row                     |

## [2]-[IDENTITY_POLICY]

- Owner: `IdentityPolicy` `[SmartEnum<string>]` three rows under the `StoreKeyPolicy` ordinal accessor.
- Cases: uuid-v7 (default), content-hash, natural-key — uuid-v7 orders B-tree inserts, content-hash addresses immutable payloads, natural-key admits caller-owned identifiers.
- Entry: `public static Guid NextKey()` mints the uuid-v7 default; `public static UInt128 ContentKey(ReadOnlySpan<byte> content)` derives content identity — pure values.
- Packages: Thinktecture.Runtime.Extensions, System.IO.Hashing, LanguageExt.Core, BCL inbox
- Growth: one `IdentityPolicy` row (key text, clr type, per-provider default SQL); a v3/v5 namespace key is one future row on the same axis; zero new surface.
- Boundary: every persisted key strategy in the package traces to one row here — uuid-ossp is the deleted extension route; the per-provider default-SQL columns feed column defaults as data, with the pg column dormant behind the double-generation research row; the sqlite `"uuid7()"` leg executes through the native function-registration rows; content identity is non-cryptographic XxHash128 with no security claim.

```csharp signature
[SmartEnum<string>]
[ValidationError<SchemaFault>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
[KeyMemberComparer<StoreKeyPolicy, string>]
public sealed partial class IdentityPolicy {
    public static readonly IdentityPolicy UuidV7Key = new("uuid-v7", clrType: typeof(Guid), pgDefaultSql: "uuidv7()", sqliteDefaultSql: "uuid7()");
    public static readonly IdentityPolicy ContentHash = new("content-hash", clrType: typeof(UInt128), pgDefaultSql: null, sqliteDefaultSql: null);
    public static readonly IdentityPolicy NaturalKey = new("natural-key", clrType: typeof(string), pgDefaultSql: null, sqliteDefaultSql: null);

    private readonly string? pgDefaultSql;
    private readonly string? sqliteDefaultSql;

    public Type ClrType { get; }

    public Option<string> PgDefaultSql => Optional(pgDefaultSql);

    public Option<string> SqliteDefaultSql => Optional(sqliteDefaultSql);

    public static Guid NextKey() => Guid.CreateVersion7();

    public static UInt128 ContentKey(ReadOnlySpan<byte> content) => XxHash128.HashToUInt128(content);
}
```

## [3]-[MIGRATION_LAW]

- Owner: `SchemaFault` `[Union]` fault family on the doctrine `Expected` shape with the dual-tier `Create` contract; `SchemaFingerprint` compiled-model fingerprint; `MigrationReceipt` record.
- Cases: Text, NewerSchema, PendingModel, PartialApplication, DestructiveUnapproved — codes 5300-5304.
- Entry: `public static Fin<Unit> Gate(Seq<string> applied, Seq<string> known, SchemaFingerprint stored, SchemaFingerprint compiled, bool pendingChanges)` — `Fin<Unit>` aborts into `SchemaFault`.
- Auto: migrations and compiled models are generated facts — the design-time `Optimize`, `ScriptMigration`, `MigrationsBundle`, and `GetMigrations` operations own emission; hand-authored migration code and custom migration-operation types are the deleted patterns.
- Receipt: `MigrationReceipt` — profile, applied ids, failed step, lock holder, compiled fingerprint, elapsed `Duration`, `Instant`, correlation.
- Packages: Microsoft.EntityFrameworkCore.Design, Microsoft.EntityFrameworkCore.Sqlite, Npgsql.EntityFrameworkCore.PostgreSQL, System.IO.Hashing, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.AppHost (project)
- Growth: one case on `SchemaFault` or one slot on `MigrationReceipt`; zero new surface.
- Boundary: one migrations source emits two provider SQL generators through `MigrationsAssembly`; `MigrateAsync` acquires the provider migration lock itself — hand-acquired `SqliteMigrationDatabaseLock` ceremony and pg advisory-lock acquisition are the deleted patterns, and the lock outcome reads from `MigrationReceipt`, never from `Migrations/Internal` types; a store history ahead of the compiled assembly folds to `NewerSchema`, never best-effort open; `HasPendingModelChanges` feeds `Gate` on the development profile only; `SchemaFingerprint.From` hashes the compiled model snapshot, the store metadata row persists it, and the open ceremony folds the persisted value through `Gate` before any provider open completes; service deploys ride idempotent `ScriptMigration` output and `MigrationsBundle` artifacts; compiled-model adoption waits on the naming research row; expand precedes contract, and a destructive step lands only behind a retention-approval receipt or folds to `DestructiveUnapproved`; design tooling stays a private asset.

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
- Boundary: both providers emit VIRTUAL generated columns by default and a STORED column exists only where the `Stored` flag derives true; a replicated row is STORED so the logical-replication publication's publish_generated_columns field carries it; an indexed row is STORED so the index reads materialized bytes; tsvector search columns are `DerivedColumn` rows whose instances land on their owning lane; a derived projection computed in application code beside a store-computed twin is the deleted pattern — one `Sql` expression owns the derivation.

```csharp signature
public sealed record DerivedColumn(string Table, string Column, string Sql, bool Replicated, bool Indexed) {
    public bool Stored => Replicated || Indexed;
}
```

## [5]-[EXTENSION_DDL]

- Owner: `SchemaDdl` `[Union]` declaration-row family with the frozen `Extensions` row set.
- Cases: Extension, Index, Exclusion — extension declarations, method-and-operator-class index rows, btree_gist exclusion-constraint rows.
- Entry: `public static ModelBuilder Declare(ModelBuilder model)` — total fold of the extension rows into model annotations.
- Auto: `HasPostgresExtension` annotations flow through `AlterDatabaseOperation` into generated migration DDL — `CreatePostgresExtensionOperation` is the deleted phantom spelling.
- Packages: Npgsql.EntityFrameworkCore.PostgreSQL, Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite, Pgvector.EntityFrameworkCore, Npgsql, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: one `SchemaDdl` row — a new extension is one `Extension` entry, a new index family is one `Index` row carrying its operator class; zero new surface.
- Boundary: preload-gated extensions never enter `Extensions` — their capability verification belongs to the provisioning table; the `Surface` column states the driver-native cost of each row and built-in ranges and multiranges map to `NpgsqlRange<T>` with zero extension entry; `Index` rows carry the method and operator-class columns (gin, gist) that the per-column lane policies instantiate, and `Exclusion` rows ride btree_gist; the postgis row makes NetTopologySuite the pg boundary projection of the canonical proto wire geometry; earthdistance is rejected — the postgis row owns geodesy.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SchemaDdl {
    private SchemaDdl() { }

    public sealed record Extension(string Name, string Surface) : SchemaDdl;
    public sealed record Index(string Table, Seq<string> Columns, string Method, Option<string> Operators) : SchemaDdl;
    public sealed record Exclusion(string Table, string Predicate, string Method) : SchemaDdl;

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
        new Extension("vector", Surface: "Pgvector.Vector"),
        new Extension("postgis", Surface: "NetTopologySuite.Geometry"));

    public static ModelBuilder Declare(ModelBuilder model) =>
        Extensions.Fold(model, static (builder, row) => builder.HasPostgresExtension(row.Name));
}
```

## [6]-[CONVERTER_RAIL]

- Owner: `ConverterRail` static composition surface.
- Entry: `public static DbContextOptionsBuilder Compose(DbContextOptionsBuilder options)` — the one registration row every profile's options delegate folds in.
- Auto: `ThinktectureValueConverterFactory` converters cover every `[ValueObject]`, `[SmartEnum]`, and keyed `[Union]` column behind `UseThinktectureValueConverters` — zero hand-written converter classes.
- Packages: Thinktecture.Runtime.Extensions.EntityFrameworkCore10, EFCore.NamingConventions, Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime, NodaTime
- Growth: one policy value per naming override; a new generated domain owner costs zero converter code on the same registration row; zero new surface.
- Boundary: `UseSnakeCaseNamingConvention` is the single naming policy — hand-written provider naming patches and per-entity converter classes are the deleted patterns; pg temporal columns ride the profile row's `UseNodaTime` option and sqlite temporal columns persist `InstantPattern.ExtendedIso` text, so no `DateTime` sentinel reaches a store; concurrency tokens are schema facts declared here — the pg row maps the system `xmin` column and the sqlite row carries an integer version column bumped per write — while the provider-exception fault projection belongs to the query rail.

```csharp signature
public static class ConverterRail {
    public static DbContextOptionsBuilder Compose(DbContextOptionsBuilder options) =>
        options.UseSnakeCaseNamingConvention().UseThinktectureValueConverters();
}
```

## [7]-[RESEARCH]

| [INDEX] | [ITEM]                                                                                                                          | [PROOF]                                                                                                                                              | [GATE]          |
| :-----: | -------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------- | --------------- |
|   [1]   | uuidv7 double-generation precedence between the client `Guid.CreateVersion7` value and the `"uuidv7()"` pg column default on one key | dotnet ef migrations script on a spike entity carrying both generation routes, asserting the emitted DEFAULT clause and the insert-time value source     | IDENTITY_POLICY |
|   [2]   | snake-case naming interaction with compiled-model output (policy names baked into the optimized model versus migration SQL)        | dotnet ef dbcontext optimize on a spike context under UseSnakeCaseNamingConvention, diffing generated names against ScriptMigration SQL                  | MIGRATION_LAW   |
|   [3]   | lock-outcome source slot for `MigrationReceipt` (provider diagnostics surface exposing migration-lock acquisition outside Internal) | uv run python -m tools.assay api query --key microsoft.entityframeworkcore.sqlite --symbol SqliteMigrationDatabaseLock --full                            | MIGRATION_LAW   |
