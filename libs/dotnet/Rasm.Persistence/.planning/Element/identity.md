# [PERSISTENCE_ELEMENT_IDENTITY]

Rasm.Persistence anchors every persisted `ElementGraph` to one relational identity tier that commits ATOMICALLY with the Marten event in the same `IDocumentSession`: `ElementIdentity` is the per-model document/row carrying the `Element/graph#STREAM_GRAIN` `ModelId` PK, the kernel-`TenantId` `Tenant` RLS column, the set of rooted `NodeId`s, the Bim-projected IFC GlobalId strings (each rooted node's contract `Node.Object.ExternalId`), the H3 spatial cell, the PostGIS `Bounds` polygon, the pgvector embedding reference, the `ObjectAcl` (the `Element/authority` frozen vocabulary), and the classification — so identity and event are one transaction with no two-ORM gap and the relational columns serve the spatial/vector/ACL/tenant lanes off the one tier. `ConverterOptions.Compose` GENERATES the whole EF surface rather than hand-mapping it, mounting `UseThinktectureValueConverters(Configuration.Default)` + `UseSnakeCaseNamingConvention()` + the provider row (`UseNpgsql(…, UseNetTopologySuite() + UseNodaTime() + UseVector())` or `UseSqlite`) on the ONE `IdentityContext`, so every `[ValueObject]`/`[SmartEnum]`/keyed-`[Union]` column converts with zero hand-written converter classes and only the LanguageExt carrier forms (`Option<Vector>`/`Seq<NodeId>`/`HashMap`) keep their Persistence-owned conversions. MODEL IDENTITY IS PROFILE-SCOPED: each `Store/provisioning#SERVER_EXTENSIONS` `StoreProfile` row carries its own compiled `Model` that `UseModel` mounts, `IdentityShapeRow` carries the provider-divergent column decisions as row data keyed alongside that profile, and `IdentityDesignFactory` builds the model per profile at design time — so `OnModelCreating` never executes at runtime, the framework model cache never arbitrates between two engines, and no interior provider probe survives. Every relational interaction is a value in the closed `IdentityOp` family that ONE `IdentityDispatch.Run` bracket folds — pooled acquisition, the profile's execution strategy, transaction posture, the tracking codec, `TagWith` provenance, and provider-fault conversion — beneath the two-altitude `IdentitySpine` whose save gate turns the placement's `Writes` authority into a refusal the store enforces. `IdentityPolicy` is the `[SmartEnum<string>]` key axis dispatching mint and decode per row through one generated `Switch`, big-endian transcription preserving order, so an identity change is an expand-wave second key, a derivation flip, and a contract-wave drop, never an `AlterColumn`. `#KMS_CUSTODY` is the crypto tier the authz split leaves here (`Element/authority` owns WHO MAY; this page owns PROOF and KEYS): `SignedAuthorship` is the KMS-signed actor attestation tying a delta to a verified blame `StoreActor`, `Custody` folds attestation, verification, and DEK envelope minting/unwrapping into one `CustodyVerdict`, and `EnvelopeKeyring` is the DEK envelope surface the SAME `KmsProvider` axis selects beside `SigningKeyring` — provider-neutral `Mint`/`MintSealed`/`Unwrap`/`Rewrap`/`Probe` delegates wrapping a data-encryption key against the cloud CMK (AWS `GenerateDataKey`/`GenerateDataKeyWithoutPlaintext`/`Decrypt`/`ReEncrypt` encrypt-as-wrap, Azure native `WrapKey`/`UnwrapKey`, GCP `Encrypt`/`Decrypt` + CRC32C + `UpdateCryptoKeyPrimaryVersion` primary repoint), so the DEK-envelope owner is THIS tier and the `Store/blobstore#BLOB_GC` `ObjectEncryption` consumes only the server-side-SSE key-id STRING this DEK envelope mints out-of-band. `SchemaGate` folds the boot posture — Marten startup, the store's published generation digest, and the MEASURED `ModelFingerprint` over the mounted compiled model — into one typed `SchemaVerdict`, and the generation owner (`IdentityDdl` with the EF.Design emission lanes) emits each profile's DDL through that profile's own model. Every identity-tier failure rides the typed `IdentityFault` band (`FaultBand.StoreIdentity`, 834x — `Element/authority` composes it, no new band). `ModelId`/`StoreActor`/`ProjectionContext` arrive from the Persistence sibling `Element/graph#STORE_HOOKS`; `StoreProfile` with its `Model`/`Capabilities`/`Ef`/`Admits` columns and `ServerExtension` from `Store/provisioning#SERVER_EXTENSIONS`; `NodeId`/`ContentAddress` from `Rasm.Element`; `ContentHash.Of` from the `Rasm` kernel; only the `SecretLease`-class KMS handle crosses from `Rasm.AppHost` through the `Runtime/secrets#SECRET_LEASE` boundary (the host resolves and leases the cloud-KMS credential; the concrete provider axis stays Persistence-side).

## [01]-[INDEX]

- [02]-[ELEMENT_IDENTITY]: relational identity tier, generated converter options over both provider rows, co-transactional Marten-document stamp, H3/PostGIS/vector/tenant join columns, and profile-scoped model identity.
- [03]-[IDENTITY_POLICY]: key axis, big-endian transcription, per-row mint/decode, and content addressing.
- [04]-[STORE_OPERATION_BRACKET]: closed `IdentityOp` request family, one bracket owning acquisition/strategy/transaction/tracking/provenance/fault, and the keyset page.
- [05]-[SAVE_INTERCEPTOR_SPINE]: two interceptor altitudes as declared rows, the write-authority gate and its tracker disposition.
- [06]-[KMS_CUSTODY]: KMS-signed authorship, DEK-envelope `EnvelopeKeyring` (`Mint`/`MintSealed`/`Unwrap`/`Rewrap`/`Probe`), and one `Custody` attestation-and-DEK-envelope fold over `CustodyVerdict`.
- [07]-[SCHEMA_VERDICT]: boot fold over the Marten startup-assertion posture and the published generation digest, the measured compiled-model fingerprint gate, and the `IdentityDdl` generation owner.

## [02]-[ELEMENT_IDENTITY]

- Owner: `ElementIdentity` the per-model identity row carrying the `ModelId` PK beside the `Tenant`/`Roots`/`GlobalIds`/`Cell`/`Bounds`/`Embedding`/`Acl`/`Classification`/`At` join columns; `NodeCell` the per-ELEMENT fine-cell routing-vertex row (`Model`/`Node`/`Tenant`/`Cell`) the `Query/cypher#GRAPH_QUERY` `pgrouting` `network_edge` source/target carries and the `#STORE_OPERATION_BRACKET` `Route` op resolves; `StoreBinding` the `[Union]` provider row (`Postgres(NpgsqlDataSource)` / `Embedded(DbConnection)`) the one converter composition discriminates; `ConverterOptions` the ONE options composition mounting the generated Thinktecture converters, the snake-case naming convention, and the provider plugin stack (the postgres row mounts `UseNetTopologySuite()` + `UseNodaTime()` + `UseVector()` so the geometry, `Instant`, and `vector(N)` columns all map through the one options entry); `IdentityShapeRow` the `[SmartEnum<string>]` provider-divergence axis carrying the JSON column type, the geometry column with its index method, and the vector column as OPTION-TYPED slots, keyed alongside `Store/provisioning#SERVER_EXTENSIONS` `StoreProfile` so the two axes join through one generated lookup; `IdentityContext` the one `DbContext` whose `OnModelCreating` reads the shape row its constructor carried and never probes the provider; `IdentityShape`/`NodeCellShape` the `IEntityTypeConfiguration` mappings carrying ONLY what the conventions cannot derive — the LanguageExt carrier conversions, the JSON columns, the geometry column, and the indexes including the keyset page's covering `(Tenant, At, Model)` prefix; `IdentityDesignFactory` the per-profile `IDesignTimeDbContextFactory<IdentityContext>` entrypoint every scaffold, `Optimize`, and idempotent script runs through; `IdentityStore` the static surface owning the co-transactional model-derived upsert stamp (`Bind` derives the statement from the profile row's compiled model; `Stamp` queues it on the Marten session) and the spatial cell and bounds mints.
- Cases: `Roots` is the set of rooted `NodeId`s the model owns (the `IfcRoot` mirror nodes), `GlobalIds` the 1:1 map from rooted `NodeId` to the compressed IFC GlobalId string projected from each contract `Node.Object.ExternalId` (the rooted `NodeId` is the neutral kernel-minted durable key, the IFC GlobalId is the `ExternalId` projection the `Version/merge#STRUCTURAL_DIFF` re-ingest `Reconcile` correlates on, never the key), `Cell` the Uber-H3 cell over the model bounding-envelope centroid (bucket-equality joins), `Bounds` the `Envelope`-derived `geometry(Polygon, 4326)` PostGIS column beside the `ZMin`/`ZMax` vertical span (the three rows on the ONE spatial-key axis: cells for bucket joins, geometry for exact XY predicates, z-span for storey banding), `Embedding` the optional pgvector reference keying the ANN lane — the per-model bounding-envelope locator, distinct in grain from the corpus-grain retrieval index (`Query/retrieval`), `Acl` the `Element/authority` `ObjectAcl` grant, `Classification` the `DataClassification` ceiling.
- Entry: `IdentityStore.Bind(StoreProfile)` ADMITS that profile's compiled model onto the typed result and derives an immutable `IdentityWriter` from it, accumulating every absent metadata slot into one `IdentityFault.ModelIncomplete`; `Stamp(IDocumentSession, ElementIdentity, IdentityWriter)` queues it on the event session. `Cell(Envelope, int)` mints either model or element cells without a forwarding sibling, and `BoundsOf(Envelope)` mints the exact footprint. `IdentityShapeRow.Get(profile.Key)` resolves the divergence row; `IdentityDesignFactory.CreateDbContext(string[])` reads the profile key off the design-time arguments.
- Auto: the identity row rides the one `IDocumentSession` the `Element/graph#STORE_HOOKS` write op uses. `IdentityWriter` captures the profile's table, schema, primary key, relational casts, and value converters at composition off `StoreProfile.Model()`, so the writer's model SOURCE is the profile row and no process-global writer can reuse a PostgreSQL model for SQLite. `UseThinktectureValueConverters(Configuration.Default)` converts generated owners, while Persistence-owned conversions cover LanguageExt carriers, recursive ACL JSON, geometry, and — as ONE `ConverterOptions.Tenant` pair both tenant-bearing relations bind — the `TenantId` column over the kernel's `Text`/`Of` inverse. `H3Index.FromPoint` mints cells and rejects `H3Index.Invalid`. RLS compares that canonical tenant text with `current_setting('rasm.tenant', true)` on the two-arm `[SESSION_GUC]` policy without a fictional `UInt128`→`uuid` provider mapping.
- Packages: Marten (`IDocumentSession.QueueSqlCommand`), Npgsql.EntityFrameworkCore.PostgreSQL (`UseNpgsql`), Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite (`UseNetTopologySuite` + `IsWithinDistance`/`DistanceKnn`), Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime (`Instant`), Thinktecture.Runtime.Extensions.EntityFrameworkCore10 (`UseThinktectureValueConverters`), EFCore.NamingConventions (`UseSnakeCaseNamingConvention`), Microsoft.EntityFrameworkCore.Sqlite (`UseSqlite`), Microsoft.EntityFrameworkCore (`DbContextOptionsBuilder.UseModel`, `PooledDbContextFactory<TContext>`, `IDbContextFactory<TContext>`), Microsoft.EntityFrameworkCore.Design (`IDesignTimeDbContextFactory<TContext>.CreateDbContext(string[])`), Pgvector.EntityFrameworkCore (`UseVector`), pocketken.H3 (`H3Index.FromPoint`), NetTopologySuite, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime.
- Growth: a new identity join column is one field on `ElementIdentity` — the conventions derive its mapping unless it is a LanguageExt carrier or a geometry, in which case ONE `IdentityShape` clause joins the residual set; a new spatial resolution is one H3 cell policy; a new engine is one `StoreBinding` case, one `IdentityShapeRow` row under the matching profile key, and one compiled model; a new provider divergence is one COLUMN on `IdentityShapeRow`; zero new surface — a separate identity transaction, a second identity ORM committing apart from the event, a parallel `NodeId`-keyed identity table, a hand ADO mapping beside the generated surface, an `IModelCacheKeyFactory` replacement, a per-profile context TYPE, a boolean shape argument deciding three unrelated columns, or an EF-versus-Marten atomicity dance is the deleted form.
- Boundary: the ONE transaction owner for identity beside event is the `IDocumentSession` — `IdentityStore.Stamp` is the lone stamp primitive the `GraphStore` write path composes (a queued model-derived upsert, never a Marten document: a document is an id+jsonb row structurally incapable of being the EF-shaped relation, and a `session.Store(identity)` claiming otherwise was the split-brain the queued statement deletes); EF/Npgsql is the READ projection and the DDL owner over the ONE declared `element_identity` relation, never a second write authority (a `DbContext.SaveChanges` over the identity table beside the Marten append is the deleted two-ORM gap); the rooted `NodeId` is the neutral kernel-minted DURABLE key and the IFC GlobalId is each node's contract `Node.Object.ExternalId` projection the `GlobalIds` map mirrors — a re-import mints fresh neutral `NodeId`s and the `Version/merge#STRUCTURAL_DIFF` `Reconcile` aligns them back on the stable GlobalId, so the durable key and every foreign reference survive re-import unchanged; THREE spatial planes live on the one tier and never duplicate — the H3 CELL plane (`Cell` per-model + `NodeCell.Cell` per-element, both `bigint` reinterpretations matching the `h3-pg` convention, bucket-equality joins the GiST/BRIN index answers), the GEOMETRY plane (`Bounds` `geometry(Polygon, 4326)` + GiST, exact XY predicates riding the `EF.Functions` translators SERVER-side — `IsWithinDistance` → `ST_DWithin`, `DistanceKnn` → the `<->` KNN order, `.Intersects`/`.Distance` instance translators, `ST_Union`/`ST_Extent` aggregates — never a client scan), and the VERTICAL plane (`ZMin`/`ZMax` per-model span + `NodeCell.Z` per-element elevation, plain indexed range predicates, so stacked elements sharing a footprint discriminate server-side and a storey band is one clause on the `Within` and `Route` op shapes, never a client elevation scan); `Rasm.Element` projects the contract-stable representation-bounds the `Bounds` producer contract names (Persistence is the recorded demanding consumer); `Bounds` is a nullable CLR `Polygon?` because the PostGIS translators bind the CLR geometry type directly in LINQ predicates — an `Option` wrapper here forfeits server-side translation, so null IS the absent-bounds state at this EF boundary; MODEL IDENTITY resolves per PROFILE and never per process: the framework's model cache keys on context type and design-time flag alone, so one context type against two engines serves the first-built model to the second, and the escape landed here is ONE COMPILED MODEL PER PROFILE — `StoreProfile.Model()` mounts through `UseModel`, which bypasses the model cache whole, so the cache-key hazard ceases to exist rather than getting keyed correctly, the profile row IS the model identity, and the two rejected escapes stay rejected because per-profile context types duplicate the declaration against this folder's ONE-`IdentityContext` ruling while `IModelCacheKeyFactory` forecloses compiled models whole, deleting the `#SCHEMA_VERDICT` `Optimize` deploy row outright; the EMBEDDED floor is provider-divergent model DATA on the one context, never a second mapping and never an interior probe — `IdentityShapeRow` carries the divergence as COLUMNS (`Bounds` degrades to a WKB `byte[]` column beside the H3 `bigint` cell where the geometry slot is absent, the JSON columns store as `text` rather than `jsonb`, and an absent vector slot ignores `Embedding` entirely, the embedded charter being the relational identity floor and `EngineOps` checkpoint tier, never SoR and never ANN), so a `Database.IsSqlite()` call inside model construction is the deleted form and a fourth divergence lands as a fourth column rather than a fourth predicate; `OnModelCreating` executes at DESIGN TIME alone under `IdentityDesignFactory`, which reads the profile key off the `dotnet ef` arguments so the scaffold, the `Optimize` compiled model, and the idempotent script each emit ONCE PER PROFILE off the one context type, and each profile's emission lands in its own generation namespace the runtime options bind; the per-ELEMENT `NodeCell` grain stays element-distinct so the `pgrouting` cell-mesh route lands back on real element ids; the `Tenant` RLS column is the coarse partition and the `Element/authority` `ObjectAcl` the fine within-tenant grant, two altitudes never duplicated.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Data.Common;
using H3;
using H3.Algorithms;
using LanguageExt;
using Marten;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NodaTime;
using Npgsql;
using Rasm.Domain;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Persistence.Store;
using Thinktecture;
using Thinktecture.EntityFrameworkCore;
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Element;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<long>]
public readonly partial struct H3Cell {
    public static H3Cell Of(H3.H3Index cell) => Create(unchecked((long)(ulong)cell));
    public H3.H3Index Live => new((ulong)Value);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StoreBinding {
    private StoreBinding() { }
    public sealed record Postgres(NpgsqlDataSource Source) : StoreBinding;
    public sealed record Embedded(DbConnection Connection) : StoreBinding;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class IdentityShapeRow {
    public static readonly IdentityShapeRow Server = new("server", json: "jsonb",
        geometry: Some(("geometry(Polygon,4326)", "gist")), vector: Some("vector(1536)"),
        emission: "Rasm.Persistence.Generations.Server",
        design: static builder => builder.UseNpgsql(static npgsql => npgsql
            .UseNetTopologySuite().UseNodaTime().UseVector()));
    public static readonly IdentityShapeRow Embedded = new("embedded", json: "text",
        geometry: None, vector: None,
        emission: "Rasm.Persistence.Generations.Embedded",
        design: static builder => builder.UseSqlite());

    public string Json { get; }
    public Option<(string Column, string Index)> Geometry { get; }
    public Option<string> Vector { get; }
    public string Emission { get; }
    public Func<DbContextOptionsBuilder<IdentityContext>, DbContextOptionsBuilder> Design { get; }

    private IdentityShapeRow(string key, string json, Option<(string Column, string Index)> geometry, Option<string> vector,
        string emission, Func<DbContextOptionsBuilder<IdentityContext>, DbContextOptionsBuilder> design) : this(key) =>
        (Json, Geometry, Vector, Emission, Design) = (json, geometry, vector, emission, design);
}

public static class CompiledModels {
    public static IModel Server => Rasm.Persistence.Generations.Server.Compiled.IdentityContextModel.Instance;
    public static IModel Embedded => Rasm.Persistence.Generations.Embedded.Compiled.IdentityContextModel.Instance;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ElementIdentity(
    ModelId Model,
    TenantId Tenant,
    Seq<NodeId> Roots,
    HashMap<NodeId, string> GlobalIds,
    H3Cell Cell,
    Polygon? Bounds,
    double? ZMin,
    double? ZMax,
    Option<Pgvector.Vector> Embedding,
    ObjectAcl Acl,
    DataClassification Classification,
    Instant At) {
    public int NodeCount => Roots.Count;
}

public sealed record NodeCell(ModelId Model, NodeId Node, TenantId Tenant, H3Cell Cell, double Z);

public sealed record IdentityWriter(string Sql, Func<ElementIdentity, object?[]> Binds);

// --- [SERVICES] ------------------------------------------------------------------------
public static class ConverterOptions {
    public static readonly ValueConverter<TenantId, string> Tenant = new(
        static tenant => tenant.Text, static text => TenantId.Admit(text).ThrowIfFail());

    public static DbContextOptionsBuilder Compose(DbContextOptionsBuilder options, StoreBinding binding) =>
        binding.Switch(
            postgres: p => options.UseNpgsql(p.Source, static npgsql => npgsql.UseNetTopologySuite().UseNodaTime().UseVector()),
            embedded: e => options.UseSqlite(e.Connection))
        .UseSnakeCaseNamingConvention()
        .UseThinktectureValueConverters(Configuration.Default);
}

public sealed class IdentityContext : DbContext {
    readonly Option<IdentityShapeRow> shape;

    public IdentityContext(DbContextOptions<IdentityContext> options) : base(options) => shape = None;
    public IdentityContext(DbContextOptions<IdentityContext> options, IdentityShapeRow row) : base(options) => shape = Some(row);

    public DbSet<ElementIdentity> Identities => Set<ElementIdentity>();
    public DbSet<NodeCell> Cells => Set<NodeCell>();

    protected override void OnModelCreating(ModelBuilder model) {
        ArgumentNullException.ThrowIfNull(model);
        IdentityShapeRow row = shape.IfNone(static () => throw new InvalidOperationException("<identity-shape:runtime-model-build>"));
        model.ApplyConfiguration(new IdentityShape(row));
        model.ApplyConfiguration(new NodeCellShape());
    }
}

public sealed class IdentityDesignFactory : IDesignTimeDbContextFactory<IdentityContext> {
    public IdentityContext CreateDbContext(string[] args) {
        ArgumentNullException.ThrowIfNull(args);
        IdentityShapeRow row = args is [string key, ..]
            ? IdentityShapeRow.Get()
            : throw new InvalidOperationException("<identity-design-profile:absent>");
        return new IdentityContext(
            (DbContextOptions<IdentityContext>)row.Design(new DbContextOptionsBuilder<IdentityContext>())
                .UseSnakeCaseNamingConvention().UseThinktectureValueConverters(Configuration.Default).Options,
            row);
    }
}

public sealed class IdentityShape(IdentityShapeRow shape) : IEntityTypeConfiguration<ElementIdentity> {
    public void Configure(EntityTypeBuilder<ElementIdentity> identity) {
        ArgumentNullException.ThrowIfNull(identity);
        identity.ToTable("element_identity");
        identity.HasKey(static e => e.Model);
        identity.Property(static e => e.Roots)
            .HasConversion(
                new ValueConverter<Seq<NodeId>, string[]>(
                    static r => r.Map(static n => n.Value).ToArray(),
                    static a => toSeq(a).Map(NodeId.Create)),
                new ValueComparer<Seq<NodeId>>(static (x, y) => x == y, static v => v.GetHashCode(), static v => v));
        identity.Property(static e => e.GlobalIds).HasColumnType(shape.Json)
            .HasConversion(
                new ValueConverter<HashMap<NodeId, string>, string>(
                    static m => System.Text.Json.JsonSerializer.Serialize(m.ToDictionary(static p => p.Key.Value, static p => p.Value), ElementJson.Options),
                    static s => toHashMap((System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(s, ElementJson.Options) ?? []).Select(static kv => (NodeId.Create(kv.Key), kv.Value)))),
                new ValueComparer<HashMap<NodeId, string>>(static (x, y) => x == y, static v => v.GetHashCode(), static v => v));
        identity.Property(static e => e.Tenant).HasColumnType("text").HasConversion(ConverterOptions.Tenant);
        identity.Property(static e => e.Acl).HasColumnType(shape.Json)
            .HasConversion(
                static acl => System.Text.Json.JsonSerializer.Serialize(acl, ElementJson.Options),
                static json => System.Text.Json.JsonSerializer.Deserialize<ObjectAcl>(json, ElementJson.Options) ?? throw new System.Text.Json.JsonException("<object-acl:null>"));
        shape.Geometry.Match(
            Some: geometry => {
                identity.Property(static e => e.Bounds).HasColumnType(geometry.Column);
                identity.HasIndex(static e => e.Bounds).HasMethod(geometry.Index);
            },
            None: () => identity.Property(static e => e.Bounds)
                .HasConversion(new ValueConverter<Polygon?, byte[]?>(
                    static g => g == null ? null : new WKBWriter().Write(g),
                    static b => b == null ? null : (Polygon)new WKBReader().Read(b))));
        shape.Vector.Match(
            Some: column => identity.Property(static e => e.Embedding).HasColumnType(column)
                .HasConversion(
                    new ValueConverter<Option<Pgvector.Vector>, Pgvector.Vector?>(
                        static o => o.Match<Pgvector.Vector?>(Some: static v => v, None: static () => null),
                        static v => Optional(v)),
                    new ValueComparer<Option<Pgvector.Vector>>(
                        static (x, y) => x == y, static v => v.GetHashCode(), static v => v)),
            None: () => identity.Ignore(static e => e.Embedding));
        identity.HasIndex(static e => e.Cell);
        identity.HasIndex(static e => new { e.Tenant, e.At, e.Model });
    }
}

public sealed class NodeCellShape : IEntityTypeConfiguration<NodeCell> {
    public void Configure(EntityTypeBuilder<NodeCell> node) {
        ArgumentNullException.ThrowIfNull(node);
        node.ToTable("node_cell");
        node.HasKey(static n => new { n.Model, n.Node });
        node.Property(static n => n.Tenant).HasColumnType("text").HasConversion(ConverterOptions.Tenant);
        node.HasIndex(static n => new { n.Cell, n.Z });
        node.HasIndex(static n => n.Tenant);
    }
}

public static class IdentityStore {
    static readonly GeometryFactory Wgs84 = new(new PrecisionModel(), 4326);

    public static Fin<IdentityWriter> Bind(StoreProfile profile) {
        ArgumentNullException.ThrowIfNull(profile);
        return profile.Model().FindEntityType(typeof(ElementIdentity)) is { } entity
            ? entity.GetTableName() is { } named
                ? Framed(entity, StoreObjectIdentifier.Table(named, entity.GetSchema()))
                : Fin<IdentityWriter>.Fail(new IdentityFault.ModelIncomplete(Seq("<table:absent>")))
            : Fin<IdentityWriter>.Fail(new IdentityFault.ModelIncomplete(Seq($"<entity:{nameof(ElementIdentity)}>")));
    }

    static Fin<IdentityWriter> Framed(IEntityType entity, StoreObjectIdentifier table) {
        Seq<(IProperty Property, Option<string> Column)> properties =
            toSeq(entity.GetProperties()).Map(property => (property, Optional(property.GetColumnName(table))));
        Option<Seq<(IProperty Property, Option<string> Column)>> keyed =
            Optional(entity.FindPrimaryKey()).Map(key =>
                toSeq(key.Properties).Map(property => (property, Optional(property.GetColumnName(table)))));

        Seq<string> absent =
            properties.Filter(static row => row.Column.IsNone).Map(static row => $"<column:{row.Property.Name}>")
            + keyed.Match(
                Some: rows => rows.Filter(static row => row.Column.IsNone).Map(static row => $"<key-column:{row.Property.Name}>"),
                None: static () => Seq("<key:absent>"));
        if (!absent.IsEmpty) { return Fin<IdentityWriter>.Fail(new IdentityFault.ModelIncomplete(absent)); }

        Seq<(string Name, string Cast, Func<ElementIdentity, object?> Read, ValueConverter? Convert)> columns =
            properties.Choose(static row => row.Column.Map(name => (
                Name: name,
                Cast: row.Property.GetRelationalTypeMapping().StoreType,
                Read: MemberReader(row.Property),
                Convert: row.Property.GetValueConverter())));
        Seq<string> keys = keyed.Match(
            Some: rows => rows.Choose(static row => row.Column.Map(Quote)),
            None: static () => Seq<string>());
        string relation = entity.GetSchema() is { } schema ? $"{Quote(schema)}.{Quote(table.Name)}" : Quote(table.Name);
        string placeholders = string.Join(", ", columns.Map(static column => $"CAST(? AS {column.Cast})"));
        string assignments = string.Join(", ", columns.Filter(column => !keys.Contains(Quote(column.Name))).Map(static column => $"{Quote(column.Name)} = excluded.{Quote(column.Name)}"));
        string sql = $"INSERT INTO {relation} ({string.Join(", ", columns.Map(static column => Quote(column.Name)))}) VALUES ({placeholders}) ON CONFLICT ({string.Join(", ", keys)}) DO UPDATE SET {assignments}";
        return Fin<IdentityWriter>.Succ(new IdentityWriter(sql, identity =>
            columns.Map(column => column.Convert is { } converter ? converter.ConvertToProvider(column.Read(identity)) : column.Read(identity)).ToArray()));
    }

    static string Quote(string identifier) => $"\"{identifier.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";

    static Func<ElementIdentity, object?> MemberReader(IProperty property) =>
        property.PropertyInfo is { } member ? member.GetValue : static _ => null;

    public static IDocumentSession Stamp(IDocumentSession session, ElementIdentity identity, IdentityWriter writer) {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(writer);
        session.QueueSqlCommand(writer.Sql, writer.Binds(identity));
        return session;
    }

    public static Fin<H3Cell> Cell(Envelope bounds, int resolution) {
        ArgumentNullException.ThrowIfNull(bounds);
        H3.H3Index index = H3.H3Index.FromPoint(new Point(bounds.Centre.X, bounds.Centre.Y) { SRID = 4326 }, resolution);
        return index.IsValidCell
            ? Fin<H3Cell>.Succ(H3Cell.Of(index))
            : Fin<H3Cell>.Fail(new IdentityFault.CellUnresolvable($"<centroid:{bounds.Centre.X},{bounds.Centre.Y}@{resolution}>"));
    }

    public static Polygon BoundsOf(Envelope bounds) => (Polygon)Wgs84.ToGeometry(bounds);
}
```

| [INDEX] | [POLICY]          | [VALUE]                                                 | [BINDING]                                                |
| :-----: | :---------------- | :------------------------------------------------------ | :------------------------------------------------------- |
|  [01]   | one txn owner     | model-derived upsert queued on the session              | `IdentityStore.Stamp` then `SaveChangesAsync`; no gap    |
|  [02]   | converter options | `UseThinktectureValueConverters(Configuration.Default)` | zero hand converters; snake-case names derived           |
|  [03]   | spatial planes    | H3 `bigint` cells + PostGIS `Bounds` + z-span           | bucket joins; exact XY predicates; storey banding        |
|  [04]   | embedded floor    | `IdentityShapeRow` option-typed column slots            | WKB bounds, text JSON, no vector column; one context     |
|  [05]   | model identity    | one compiled model per `StoreProfile`                   | `UseModel` bypasses the cache; build is design-time only |
|  [06]   | design emission   | `IdentityDesignFactory` keyed by profile argument       | per-profile scaffold, `Optimize`, generation script      |
|  [07]   | page index        | composite `(Tenant, At, Model)`                         | keyset tuple and its covering prefix, one declaration    |
|  [08]   | rooted key        | neutral kernel-minted durable `NodeId`                  | `ExternalId` projection; re-ingest correlates on it      |
|  [09]   | tenant partition  | `Tenant` RLS column                                     | coarse scope; `ObjectAcl` is the fine grant              |

## [03]-[IDENTITY_POLICY]

- Owner: `IdentityPolicy` the `[SmartEnum<string>]` five-row key axis carrying generator, big-endian transcription, ordering, collision class, and CLR type, dispatching `Mint` per row through one generated `Switch`; `StoreKey` the `[Union]` closed key carrier (`Surrogate`/`Content`/`Natural`); `Collision` the collision-posture vocabulary.
- Cases: `uuid-v7` (default, B-tree insert-local, `Guid.CreateVersion7`), `uuid-v7-backfill` (historical-timestamp mint for deterministic backfill), `content-hash` (immutable-payload content addressing through the kernel `ContentHash.Of` — the `ContentAddress` row), `natural-key` (caller-owned identifier passthrough), `namespace-key` (RFC-4122 v5 over a namespace and a name for stable derived ids); `Collision` rows are `unmintable`, `content-idempotent`, `foreign-authority`, `derived-deterministic`.
- Entry: `Mint(ReadOnlyMemory<byte>, Instant)` dispatches through the generated `Switch`; `Decode(ReadOnlySpan<byte>)` validates width and strict UTF-8 before returning `Fin<StoreKey>`; `StoreKey.Spelled` is the ordering-preserving big-endian transcription; `StoreKey.ObservedAt` projects a v7 key's embedded creation time.
- Packages: Thinktecture.Runtime.Extensions, Rasm (`ContentHash.Of`), System.Security.Cryptography (`IncrementalHash`/`SHA1`), System.Buffers.Binary, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: one `IdentityPolicy` row carries text, CLR type, ordering, collision, and client-generated precedence; a new posture is one `Collision` row; zero new surface.
- Boundary: every persisted key strategy traces to one row here — `uuid-ossp` is the deleted extension route; `StoreKey` is the one closed key carrier so a column type is a case projection, never a parallel key type per provider; ordering survives transcription only when the spelling preserves it — every case transcribes big-endian (`Guid.ToByteArray(bigEndian: true)`, the kernel `ContentHash.Wire`/`Admit` pair for the 16-byte content row, UTF-8) because the platform-default little-endian export fractures a binary-keyed index; `StoreKey.ObservedAt` makes a v7 key a free coarse creation-time axis so a composite `(low-cardinality discriminant, v7 key)` index stays append-local; an identity-row change mints a new generation whose `Carried` relations re-mint every key through the `uuid-v7-backfill` derivation inside the cutover projection, so foreign references and AS-OF cursor validity land already consistent in the published namespace; content identity is the non-cryptographic kernel `ContentHash.Of` (no security claim, no direct `XxHash128` call site) and `namespace-key` mints the canonical RFC-4122 v5 namespace UUID (`SHA1` the spec construction, not a security claim).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Collision {
    public static readonly Collision Unmintable = new("unmintable");
    public static readonly Collision ContentIdempotent = new("content-idempotent");
    public static readonly Collision ForeignAuthority = new("foreign-authority");
    public static readonly Collision DerivedDeterministic = new("derived-deterministic");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StoreKey {
    private StoreKey() { }
    public sealed record Surrogate(Guid Value) : StoreKey;
    public sealed record Content(UInt128 Value) : StoreKey;
    public sealed record Natural(string Value) : StoreKey;

    public byte[] Spelled() => this.Switch(
        surrogate: static s => s.Value.ToByteArray(bigEndian: true),
        content:   static c => ContentHash.Wire(c.Value).ToByteArray(),
        natural:   static n => System.Text.Encoding.UTF8.GetBytes(n.Value));

    public Option<Instant> ObservedAt() =>
        this is Surrogate { Value.Version: 7 } s ? Some(Instant.FromUnixTimeMilliseconds(UnixMillis(s.Value))) : None;

    static long UnixMillis(Guid key) {
        Span<byte> b = stackalloc byte[16];
        _ = key.TryWriteBytes(b, bigEndian: true, out _);
        return ((long)System.Buffers.Binary.BinaryPrimitives.ReadUInt16BigEndian(b) << 32) | System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(b[2..]);
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class IdentityPolicy {
    public static readonly IdentityPolicy UuidV7Key = new("uuid-v7", typeof(Guid), Collision.Unmintable, ordered: true);
    public static readonly IdentityPolicy UuidV7Backfill = new("uuid-v7-backfill", typeof(Guid), Collision.Unmintable, ordered: true);
    public static readonly IdentityPolicy ContentHashKey = new("content-hash", typeof(UInt128), Collision.ContentIdempotent, ordered: false);
    public static readonly IdentityPolicy NaturalKey = new("natural-key", typeof(string), Collision.ForeignAuthority, ordered: false);
    public static readonly IdentityPolicy NamespaceKey = new("namespace-key", typeof(Guid), Collision.DerivedDeterministic, ordered: false);
    public static readonly Guid Namespace = new("6e89a1f0-1d2b-7c4e-9f3a-0b1c2d3e4f50");

    public Type ClrType { get; }
    public Collision Collision { get; }
    public bool Ordered { get; }
    private IdentityPolicy(string key, Type clr, Collision collision, bool ordered) : this(key) => (ClrType, Collision, Ordered) = (clr, collision, ordered);

    public StoreKey Mint(ReadOnlyMemory<byte> material, Instant observed) => Switch<(ReadOnlyMemory<byte> Material, Instant Observed), StoreKey>(
        state: (material, observed),
        uuidV7Key: static _ => new StoreKey.Surrogate(Guid.CreateVersion7()),
        uuidV7Backfill: static s => new StoreKey.Surrogate(Guid.CreateVersion7(s.Observed.ToDateTimeOffset())),
        contentHashKey: static s => new StoreKey.Content(ContentHash.Of(s.Material.Span)),
        naturalKey: static s => new StoreKey.Natural(System.Text.Encoding.UTF8.GetString(s.Material.Span)),
        namespaceKey: static s => new StoreKey.Surrogate(NamespaceUuid(Namespace, s.Material.Span)));

    public Fin<StoreKey> Decode(ReadOnlySpan<byte> spelled) =>
        Switch<(byte[] Bytes, string Row), Fin<StoreKey>>(
            state: (spelled.ToArray(), Key),
            uuidV7Key:      static s => Sized(s, static value => new StoreKey.Surrogate(new Guid(value, bigEndian: true))),
            uuidV7Backfill: static s => Sized(s, static value => new StoreKey.Surrogate(new Guid(value, bigEndian: true))),
            contentHashKey: static s => ContentHash.Admit(s.Bytes).Map(static value => (StoreKey)new StoreKey.Content(value))
                .MapFail(_ => (Error)new IdentityFault.KeyMalformed($"<key-width:{s.Row}:{s.Bytes.Length}>")),
            naturalKey:     static s => Text(s),
            namespaceKey:   static s => Sized(s, static value => new StoreKey.Surrogate(new Guid(value, bigEndian: true))));

    static readonly System.Text.UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    static Fin<StoreKey> Sized((byte[] Bytes, string Row) state, Func<byte[], StoreKey> read) =>
        state.Bytes.Length == 16
            ? Fin<StoreKey>.Succ(read(state.Bytes))
            : Fin<StoreKey>.Fail(new IdentityFault.KeyMalformed($"<key-width:{state.Row}:{state.Bytes.Length}>"));

    static Fin<StoreKey> Text((byte[] Bytes, string Row) state) =>
        Try.lift(() => Fin.Succ((StoreKey)new StoreKey.Natural(StrictUtf8.GetString(state.Bytes)))).Run().Bind(static inner => inner);
    }

    static Guid NamespaceUuid(Guid ns, ReadOnlySpan<byte> name) {
        Span<byte> nsBytes = stackalloc byte[16];
        _ = ns.TryWriteBytes(nsBytes, bigEndian: true, out _);
        using System.Security.Cryptography.IncrementalHash sha = System.Security.Cryptography.IncrementalHash.CreateHash(System.Security.Cryptography.HashAlgorithmName.SHA1);
        sha.AppendData(nsBytes);
        sha.AppendData(name);
        Span<byte> hash = stackalloc byte[20];
        _ = sha.GetHashAndReset(hash);
        hash[6] = (byte)((hash[6] & 0x0F) | 0x50);
        hash[8] = (byte)((hash[8] & 0x3F) | 0x80);
        return new Guid(hash[..16], bigEndian: true);
    }
}
```

## [04]-[STORE_OPERATION_BRACKET]

- Owner: `IdentityOp` the `[Union]` closed request family every relational interaction is a value in, modelled on the folder's landed `Element/graph#STORE_HOOKS` `GraphStoreOp` precedent; `IdentityFilter` the closed predicate-SHAPE family the read arities compose, disjoint from arity so a new predicate never mints an entrypoint; `IdentityCursor` the opaque keyset cursor; `IdentityView`/`NodeCellView` the value projections ops return; `IdentityOutcome` the closed payload union every op returns; `IdentityOpFacts` the ONE derived fact row per op (verb, plan tag, replay posture, commit probe); `CommitProbe` the non-idempotent tail's measured commit read; `TrackingCodec` the tracking-posture row; `NodeCellBulkPolicy` the copy-lane row the profile's `BulkCopy` capability selects; `IdentityLease` the composed acquisition value carrying the pooled factory, the `StoreProfile` row, the `Placement`, and the codec; `IdentityDispatch` the static surface owning the ONE bracket and its slot roster.
- Cases: ARITY discriminates on the input value — `Point` resolves one key to an optional projection, `Batch` a key set to a batch, `Page` a predicate with `Option<IdentityCursor>` to a window, `Drain` a predicate alone to a bracket-internal stream, `Route` a cell with an optional storey band to `NodeCell` vertices, `Ingest` a collection to the copy lane, `Maintain` a parameterized statement to the maintenance lane. `IdentityFilter` is `All | Near(H3Cell, int) | Within(Point, double, Option<(double, double)>)`. `IdentityOutcome` is `Resolved | Batched | Paged | Routed | Drained | Affected`.
- Entry: `IdentityDispatch.Run(IdentityLease, IdentityOp, ProjectionContext, CancellationToken)` folds the closed family through one generated total `Switch`, so a new op breaks the build at the dispatch rather than falling into a runtime-silent arm; `IdentityOp.Facts` projects the op's verb, plan tag, replay posture, and commit probe in one pass.
- Auto: `Admit` refuses BEFORE acquisition, so a non-writing `Placement` never opens a mutating lane and a retrying strategy never re-drives a tail whose commit it cannot verify. Inputs travel as `TState` through STATIC lambdas, so a retry re-runs a closed value rather than a captured closure. Non-replayable tails open and commit their transaction INSIDE the strategy callback. Each leg stamps its verb as the FIRST `TagWith` line and its filter discriminant as the second, parameterizes every value, and projects through the ONE `Projection` expression.
- Packages: Microsoft.EntityFrameworkCore (`IDbContextFactory<TContext>.CreateDbContextAsync`, `DatabaseFacade.CreateExecutionStrategy`/`BeginTransactionAsync`/`AutoTransactionBehavior`/`AutoSavepointsEnabled`, `IExecutionStrategy.ExecuteAsync` with `verifySucceeded`, `ExecutionResult<TResult>`, `TagWith`, `AsAsyncEnumerable`, `ChangeTracker.QueryTrackingBehavior`), Microsoft.EntityFrameworkCore.Relational (`DatabaseFacade.ExecuteSqlAsync(FormattableString, CancellationToken)`), Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite (`EF.Functions.IsWithinDistance`/`DistanceKnn`), pocketken.H3 (`GridDiskDistancesSafe`), linq2db.EntityFrameworkCore (`BulkCopyAsync`, `BulkCopyOptions`, `BulkCopyRowsCopied.RowsCopied`), LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions.
- Growth: a new interaction is one `IdentityOp` case and one arm; a new predicate is one `IdentityFilter` case and one shape; a new cross-cutting concern is one bracket row touching zero ops; a new payload is one `IdentityOutcome` case; zero new surface — a repository per relation, a public static returning `IQueryable`, an offset page, a `GetById`/`GetMany` family, a per-call-site `try`/`catch` around a provider exception, or a second transaction scope beside the Marten session is the deleted form.
- Boundary: this bracket owns READS, the maintenance lane, and the bulk lane, while identity WRITES ride the Marten `IDocumentSession` through `IdentityStore.Stamp` — the bracket COMPOSES that session rather than opening a second transaction scope beside it, because a second scope re-mints exactly the two-ORM gap `#ELEMENT_IDENTITY` deletes; ops return VALUE projections and never entities, so no consumer couples to the mapped shape or drags the tracker across the boundary, and the `Drain` arity folds INSIDE the bracket because a live enumerable handed out after the context pools enumerates reclaimed state; the page op is KEYSET-ONLY — offset cost grows with depth and concurrent writes shift boundaries into duplicates and gaps — its ordering tuple ends in the unique `Model` tiebreaker, its predicate is the LEXICOGRAPHIC EXPANSION because tuple row-value comparison does not translate, its cursor values bind as PARAMETERS so page depth never changes the SQL shape, and its ordering tuple is the contiguous prefix of the `#ELEMENT_IDENTITY` composite index declared in the same breath; the cursor is the projected ordering tuple of the last row, opaque to callers, and its anchor's disappearance is the typed `IdentityFault.CursorStale` rejection rather than a silent empty page, because an empty page reads as exhaustion to every caller; DISTANCE ranking is not a keyset ordering, so the `Within` filter ranks nearest-first under the unpaged `Drain` arity and orders by the declared tuple under `Page`; the bracket converts provider exceptions through the ONE `IdentityFault.Rejected` Lift at ITS boundary — which CLASSIFIES on the driver's own `DbException.IsTransient` so the band publishes the kernel `Retriability` the strategy above it reads, a bare untyped rejection having left that whole class unreadable to every predicate — and interior op bodies never see them, while caller cancellation passes through UNTYPED and is never converted to a store rejection; RELATIONAL RETRY lands here at the owner `ARCHITECTURE.md` `[05]-[BOUNDARIES]` reserves for it, reading the profile's `StoreCapability.StrategyRedrive` row — `verifySucceeded` is MANDATORY for a non-idempotent tail under a re-driving profile because an ambiguous commit double-applies delta-shaped work, and the probe returns the tail's MEASURED outcome rather than a fabricated zero-row success — `CommitProbe` is the typed SHAPE that obligation takes and `Store/coordination#COORDINATION_OP` `Coordinate.Verified` is the instance the fenced-store caller supplies to the strategy this bracket seats, so the probe is a value each caller states rather than a discipline a comment describes; every op stamps provenance through `TagWith` from its own verb and parameterizes every value, so one cached plan serves one op and the `Store/observability#PLAN_PROFILE` statement key mints over the same tagged text; the copy lane reads the profile's `StoreCapability.BulkCopy` membership rather than taking a caller policy, so an engine without a native COPY lane routes the multi-row batch by profile data and never by a call-site knob.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Linq.Expressions;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Rasm.Persistence.Store;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TrackingCodec {
    public static readonly TrackingCodec Read = new("read", QueryTrackingBehavior.NoTrackingWithIdentityResolution);
    public static readonly TrackingCodec Write = new("write", QueryTrackingBehavior.TrackAll);
    public QueryTrackingBehavior Tracking { get; }
    private TrackingCodec(string key, QueryTrackingBehavior tracking) : this(key) => Tracking = tracking;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NodeCellBulkPolicy {
    public static readonly NodeCellBulkPolicy Server = new("server", LinqToDB.Data.BulkCopyType.ProviderSpecific, 16_384);
    public static readonly NodeCellBulkPolicy Embedded = new("embedded", LinqToDB.Data.BulkCopyType.MultipleRows, 1_024);
    public LinqToDB.Data.BulkCopyOptions Options { get; }
    private NodeCellBulkPolicy(string key, LinqToDB.Data.BulkCopyType type, int maxBatchSize) : this(key) =>
        Options = new LinqToDB.Data.BulkCopyOptions { BulkCopyType = type, KeepIdentity = true, MaxBatchSize = maxBatchSize };

    public static NodeCellBulkPolicy Of(StoreProfile profile) =>
        profile.Capabilities.Admits(StoreCapability.BulkCopy) ? Server : Embedded;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record IdentityFilter {
    private IdentityFilter() { }
    public sealed record All : IdentityFilter;
    public sealed record Near(H3Cell Cell, int Ring) : IdentityFilter;
    public sealed record Within(Point Probe, double Range, Option<(double Min, double Max)> Band) : IdentityFilter;

    public string Key => this.Switch(all: static _ => "all", near: static _ => "near", within: static _ => "within");
}

public readonly record struct IdentityCursor(Instant At, ModelId Model);

public delegate Task<Option<IdentityOutcome>> CommitProbe(IdentityContext store, CancellationToken cancellationToken);

// --- [MODELS] --------------------------------------------------------------------------
public sealed record IdentityView(ModelId Model, TenantId Tenant, H3Cell Cell, double? ZMin, double? ZMax, DataClassification Classification, Instant At);
public sealed record NodeCellView(ModelId Model, NodeId Node, H3Cell Cell, double Z);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record IdentityOutcome {
    private IdentityOutcome() { }
    public sealed record Resolved(Option<IdentityView> Row) : IdentityOutcome;
    public sealed record Batched(Seq<IdentityView> Rows) : IdentityOutcome;
    public sealed record Paged(Seq<IdentityView> Rows, Option<IdentityCursor> Next) : IdentityOutcome;
    public sealed record Routed(Seq<NodeCellView> Vertices) : IdentityOutcome;
    public sealed record Drained(long Rows) : IdentityOutcome;
    public sealed record Affected(long Rows) : IdentityOutcome;

    public long Rows => this.Switch(
        resolved: static r => r.Row.IsSome ? 1L : 0L,
        batched:  static b => (long)b.Rows.Count,
        paged:    static p => (long)p.Rows.Count,
        routed:   static r => (long)r.Vertices.Count,
        drained:  static d => d.Rows,
        affected: static a => a.Rows);
}

public readonly record struct IdentityOpFacts(string Verb, string Tag, bool Replayable, Option<CommitProbe> Verify);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record IdentityOp {
    private IdentityOp() { }

    public sealed record Point(ModelId Model) : IdentityOp;
    public sealed record Batch(Seq<ModelId> Models) : IdentityOp;
    public sealed record Page(IdentityFilter Filter, int Width, Option<IdentityCursor> After) : IdentityOp;
    public sealed record Drain(IdentityFilter Filter, Func<IdentityView, CancellationToken, ValueTask> Sink) : IdentityOp;
    public sealed record Route(H3Cell Cell, Option<(double Min, double Max)> Band) : IdentityOp;
    public sealed record Ingest(Seq<NodeCell> Cells, Option<CommitProbe> Verify) : IdentityOp;
    public sealed record Maintain(FormattableString Statement, Option<CommitProbe> Verify) : IdentityOp;

    public IdentityOpFacts Facts => this.Switch(
        point:    static _ => new IdentityOpFacts("point", "key", Replayable: true, None),
        batch:    static _ => new IdentityOpFacts("batch", "keys", Replayable: true, None),
        page:     static p => new IdentityOpFacts("page", p.Filter.Key, Replayable: true, None),
        drain:    static d => new IdentityOpFacts("drain", d.Filter.Key, Replayable: true, None),
        route:    static _ => new IdentityOpFacts("route", "cell", Replayable: true, None),
        ingest:   static i => new IdentityOpFacts("ingest", "copy", Replayable: false, i.Verify),
        maintain: static m => new IdentityOpFacts("maintain", "raw", Replayable: false, m.Verify));
}

public sealed record IdentityLease(IDbContextFactory<IdentityContext> Pool, StoreProfile Profile, Placement Placement, TrackingCodec Codec);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class IdentityDispatch {
    static readonly Expression<Func<ElementIdentity, IdentityView>> Projection =
        static e => new IdentityView(e.Model, e.Tenant, e.Cell, e.ZMin, e.ZMax, e.Classification, e.At);

    public static IO<Fin<IdentityOutcome>> Run(IdentityLease lease, IdentityOp op, ProjectionContext frame, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(lease);
        ArgumentNullException.ThrowIfNull();
        return Admit(lease).Match(
            Succ: facts => Bracket(lease, facts, frame, cancellationToken),
            Fail: error => IO.pure(Fin<IdentityOutcome>.Fail(error)));
    }

    static Fin<IdentityOpFacts> Admit(IdentityLease lease, IdentityOp op) =>
        op.Facts is { Replayable: false } tail
            ? lease.Placement.Held
                .Require(Placement.Mutating, missing =>
                    new IdentityFault.WriteRefused($"<placement:{lease.Placement.Key}:{tail.Verb}:{missing.Wire}>"))
                .Match(
                    Some: Fin<IdentityOpFacts>.Fail,
                    None: () => lease.Profile.Capabilities.Admits(StoreCapability.StrategyRedrive) && tail.Verify.IsNone
                        ? Fin<IdentityOpFacts>.Fail(new IdentityFault.WriteRefused($"<unverifiable-retry:{tail.Verb}>"))
                        : Fin<IdentityOpFacts>.Succ(tail))
            : Fin<IdentityOpFacts>.Succ(op.Facts);

    static IO<Fin<IdentityOutcome>> Bracket(IdentityLease lease, IdentityOp op, IdentityOpFacts facts, ProjectionContext frame, CancellationToken cancellationToken) =>
        IO.liftAsync(async () => await Op.Of().Catch(async token => {
            await using IdentityContext store = await lease.Pool.CreateDbContextAsync(token).ConfigureAwait(false);
            store.ChangeTracker.QueryTrackingBehavior = lease.Codec.Tracking;
            store.Database.AutoTransactionBehavior = AutoTransactionBehavior.WhenNeeded;
            store.Database.AutoSavepointsEnabled = true;
            return await store.Database.CreateExecutionStrategy().ExecuteAsync(
                (Store: store, Facts: facts, Profile: lease.Profile),
                static (state, inner) => Execute(state.Store, state.Facts, state.Profile, inner),
                Probe(facts),
                token).ConfigureAwait(false);
        }, error => IdentityFault.Rejected(facts.Verb, error), cancellationToken).ConfigureAwait(false));

    static Func<(IdentityContext Store, IdentityOp Op, IdentityOpFacts Facts, StoreProfile Profile), CancellationToken, Task<ExecutionResult<Fin<IdentityOutcome>>>>? Probe(IdentityOpFacts facts) =>
        facts.Verify.Match(
            Some: probe => async ((IdentityContext Store, IdentityOp Op, IdentityOpFacts Facts, StoreProfile Profile) state, CancellationToken token) =>
                (await probe(state.Store, token).ConfigureAwait(false)).Match(
                    Some: landed => new ExecutionResult<Fin<IdentityOutcome>>(successful: true, Fin<IdentityOutcome>.Succ(landed)),
                    None: () => new ExecutionResult<Fin<IdentityOutcome>>(successful: false, Fin<IdentityOutcome>.Succ(new IdentityOutcome.Drained(0L)))),
            None: static () => null);

    static Task<Fin<IdentityOutcome>> Execute(IdentityContext store, IdentityOp op, IdentityOpFacts facts, StoreProfile profile, CancellationToken token) =>
        facts.Replayable
            ? Leg(store, facts, profile, token)
            : Enlisted(store, facts, profile, token);

    static async Task<Fin<IdentityOutcome>> Enlisted(IdentityContext store, IdentityOp op, IdentityOpFacts facts, StoreProfile profile, CancellationToken token) {
        await using IDbContextTransaction transaction = await store.Database.BeginTransactionAsync(token).ConfigureAwait(false);
        Fin<IdentityOutcome> outcome = await Leg(store, facts, profile, token).ConfigureAwait(false);
        if (outcome.IsSucc) { await transaction.CommitAsync(token).ConfigureAwait(false); }
        return outcome;
    }

    static Task<Fin<IdentityOutcome>> Leg(IdentityContext store, IdentityOp op, IdentityOpFacts facts, StoreProfile profile, CancellationToken token) => op.Switch(
        state: (Store: store, Facts: facts, Profile: profile, Token: token),
        point:    static (s, p) => Resolve(s.Store, p, s.Facts, s.Token),
        batch:    static (s, b) => Collect(s.Store, b, s.Facts, s.Token),
        page:     static (s, g) => Window(s.Store, g, s.Facts, s.Token),
        drain:    static (s, d) => Stream(s.Store, d, s.Facts, s.Token),
        route:    static (s, r) => Vertices(s.Store, r, s.Facts, s.Token),
        ingest:   static (s, i) => Copy(s.Store, i, s.Facts, s.Profile, s.Token),
        maintain: static (s, m) => Raw(s.Store, m, s.Facts, s.Token));

    static async Task<Fin<IdentityOutcome>> Resolve(IdentityContext store, IdentityOp.Point op, IdentityOpFacts facts, CancellationToken token) =>
        Fin<IdentityOutcome>.Succ(new IdentityOutcome.Resolved(Optional(await Tagged(store.Identities
            .Where(e => e.Model == op.Model).Select(Projection), facts)
            .FirstOrDefaultAsync(token).ConfigureAwait(false))));

    static async Task<Fin<IdentityOutcome>> Collect(IdentityContext store, IdentityOp.Batch op, IdentityOpFacts facts, CancellationToken token) {
        ModelId[] keys = op.Models.ToArray();
        return Fin<IdentityOutcome>.Succ(new IdentityOutcome.Batched(toSeq(await Tagged(store.Identities
            .Where(e => keys.Contains(e.Model)).Select(Projection), facts)
            .ToArrayAsync(token).ConfigureAwait(false))));
    }

    static async Task<Fin<IdentityOutcome>> Window(IdentityContext store, IdentityOp.Page op, IdentityOpFacts facts, CancellationToken token) {
        IQueryable<ElementIdentity> rows = Shaped(store, op.Filter);
        IQueryable<ElementIdentity> seeded = op.After.Match(
            Some: cursor => rows.Where(e => e.At > cursor.At || (e.At == cursor.At && e.Model.Value.CompareTo(cursor.Model.Value) > 0)),
            None: () => rows);
        IdentityView[] window = await Tagged(seeded
            .OrderBy(static e => e.At).ThenBy(static e => e.Model).Take(op.Width).Select(Projection), facts)
            .ToArrayAsync(token).ConfigureAwait(false);
        return window.Length == 0 && op.After.IsSome && !await Anchored(store, op.After, token).ConfigureAwait(false)
            ? Fin<IdentityOutcome>.Fail(new IdentityFault.CursorStale($"<cursor:{facts.Verb}>"))
            : Fin<IdentityOutcome>.Succ(new IdentityOutcome.Paged(toSeq(window),
                window.Length == op.Width ? Some(new IdentityCursor(window[^1].At, window[^1].Model)) : None));
    }

    static Task<bool> Anchored(IdentityContext store, Option<IdentityCursor> cursor, CancellationToken token) =>
        cursor.Match(
            Some: anchor => store.Identities.AnyAsync(e => e.Model == anchor.Model, token),
            None: static () => Task.FromResult(true));

    static async Task<Fin<IdentityOutcome>> Stream(IdentityContext store, IdentityOp.Drain op, IdentityOpFacts facts, CancellationToken token) {
        long rows = 0L;
        await foreach (IdentityView view in Tagged(Ordered(Shaped(store, op.Filter), op.Filter).Select(Projection), facts)
            .AsAsyncEnumerable().WithCancellation(token).ConfigureAwait(false)) {
            await op.Sink(view, token).ConfigureAwait(false);
            rows++;
        }
        return Fin<IdentityOutcome>.Succ(new IdentityOutcome.Drained(rows));
    }

    static async Task<Fin<IdentityOutcome>> Vertices(IdentityContext store, IdentityOp.Route op, IdentityOpFacts facts, CancellationToken token) {
        IQueryable<NodeCell> cells = store.Cells.Where(n => n.Cell == op.Cell);
        return Fin<IdentityOutcome>.Succ(new IdentityOutcome.Routed(toSeq(await Tagged(op.Band
            .Match(Some: band => cells.Where(n => n.Z >= band.Min && n.Z <= band.Max), None: () => cells)
            .Select(static n => new NodeCellView(n.Model, n.Node, n.Cell, n.Z)), facts)
            .ToArrayAsync(token).ConfigureAwait(false))));
    }

    static async Task<Fin<IdentityOutcome>> Copy(IdentityContext store, IdentityOp.Ingest op, IdentityOpFacts facts, StoreProfile profile, CancellationToken token) {
        LinqToDB.Data.BulkCopyRowsCopied copied = await store
            .BulkCopyAsync(NodeCellBulkPolicy.Of(profile).Options, op.Cells, token).ConfigureAwait(false);
        return copied.RowsCopied == op.Cells.Count
            ? Fin<IdentityOutcome>.Succ(new IdentityOutcome.Affected(copied.RowsCopied))
            : Fin<IdentityOutcome>.Fail(new IdentityFault.WriteRefused($"<{facts.Verb}:lost:{op.Cells.Count - copied.RowsCopied}>"));
    }

    static async Task<Fin<IdentityOutcome>> Raw(IdentityContext store, IdentityOp.Maintain op, IdentityOpFacts facts, CancellationToken token) =>
        Fin<IdentityOutcome>.Succ(new IdentityOutcome.Affected(await store.Database.ExecuteSqlAsync(
            System.Runtime.CompilerServices.FormattableStringFactory.Create($"-- identity.{facts.Verb}\n{op.Statement.Format}", op.Statement.GetArguments()),
            token).ConfigureAwait(false)));

    static IQueryable<T> Tagged<T>(IQueryable<T> rows, IdentityOpFacts facts) =>
        rows.TagWith(facts.Verb).TagWith(facts.Tag);

    static IQueryable<ElementIdentity> Shaped(IdentityContext store, IdentityFilter filter) => filter.Switch(
        state: store.Identities.AsQueryable(),
        all:    static (rows, _) => rows,
        near:   static (rows, n) => Disk(rows, n),
        within: static (rows, w) => Band(Exact(rows, w), w.Band));

    static IQueryable<ElementIdentity> Disk(IQueryable<ElementIdentity> rows, IdentityFilter.Near near) {
        H3Cell[] disk = toSeq(near.Cell.Live.GridDiskDistancesSafe(near.Ring)).Map(static ring => H3Cell.Of(ring.Index)).ToArray();
        return rows.Where(e => disk.Contains(e.Cell));
    }

    static IQueryable<ElementIdentity> Exact(IQueryable<ElementIdentity> rows, IdentityFilter.Within within) =>
        rows.Where(e => e.Bounds != null && EF.Functions.IsWithinDistance(e.Bounds!, within.Probe, within.Range, false));

    static IQueryable<ElementIdentity> Band(IQueryable<ElementIdentity> rows, Option<(double Min, double Max)> band) =>
        band.Match(
            Some: span => rows.Where(e => e.ZMin != null && e.ZMax != null && e.ZMin <= span.Max && e.ZMax >= span.Min),
            None: () => rows);

    static IQueryable<ElementIdentity> Ordered(IQueryable<ElementIdentity> rows, IdentityFilter filter) => filter.Switch(
        state: rows,
        all:    static (ordered, _) => ordered.OrderBy(static e => e.At).ThenBy(static e => e.Model),
        near:   static (ordered, _) => ordered.OrderBy(static e => e.At).ThenBy(static e => e.Model),
        within: static (ordered, w) => ordered.OrderBy(e => EF.Functions.DistanceKnn(e.Bounds!, w.Probe)));
}
```

| [INDEX] | [POLICY]         | [VALUE]                                       | [BINDING]                                                     |
| :-----: | :--------------- | :-------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | arity            | `IdentityOp` case per input shape             | key, key set, predicate + cursor, predicate alone             |
|  [02]   | pagination       | keyset only, `Option<IdentityCursor>` input   | zero extra entrypoints; absent cursor is the first page       |
|  [03]   | cursor validity  | anchor probe on an empty seeded window        | `IdentityFault.CursorStale`, never a silent empty page        |
|  [04]   | retry            | `StoreCapability.StrategyRedrive`             | `verifySucceeded` mandatory for a non-replayable tail         |
|  [05]   | transaction      | opened inside the strategy callback           | mutating tails only; savepoints nest under a caller scope     |
|  [06]   | egress           | `IdentityView` / `NodeCellView`               | no entity leaves; one `Projection` expression                 |
|  [07]   | provenance       | `TagWith` verb line then predicate line       | one cached plan per op; the statement harvest reads the tag   |
|  [08]   | fault conversion | one `IdentityFault.Rejected` Lift at the edge | classifies on `DbException.IsTransient`; cancellation untyped |
|  [09]   | write authority  | Marten `IDocumentSession` via `IdentityStore` | bracket composes that session; no second transaction scope    |
|  [10]   | copy lane        | `NodeCellBulkPolicy.Of(StoreProfile)`         | `BulkCopy` selects COPY over multi-row batch; never a knob    |

## [05]-[SAVE_INTERCEPTOR_SPINE]

- Owner: `TrackerDisposition` the `[SmartEnum<string>]` delegate-bearing tracker policy a suppressing gate declares; `ResolutionPolicy` the row selecting the BUILT-IN identity-resolution pair; `SpineAltitude` the closed two-row altitude vocabulary carrying each altitude's mount delegate; `SpineMount` the composition input the profile row fills; `IdentityWriteGate` the unit-of-work write-authority gate; `IdentitySpine` the static surface composing the roster in declared order and escalating warnings at the same options row.
- Cases: `SpineAltitude` is `Compilation | UnitOfWork`. `TrackerDisposition` is `Clear | Detach | Hold`. `ResolutionPolicy` is `Ignoring | Updating`, each mounting its framework interceptor.
- Entry: `IdentitySpine.Compose(DbContextOptionsBuilder, SpineMount)` folds the altitude roster into `AddInterceptors` in DECLARED order and escalates the chosen runtime warnings through `ConfigureWarnings`.
- Auto: registration order IS execution order, so the declared roster is the runtime order and no composition root re-sequences it. Save-altitude gating reads `HasResult` first, so a later interceptor never re-suppresses an already-suppressed save. Every declared member lands BOTH modality twins.
- Packages: Microsoft.EntityFrameworkCore (`ISaveChangesInterceptor`, `IMaterializationInterceptor`, `IQueryExpressionInterceptor`, `IIdentityResolutionInterceptor`, `IgnoringIdentityResolutionInterceptor`, `UpdatingIdentityResolutionInterceptor`, `InterceptionResult<T>.SuppressWithResult`/`HasResult`, `DbContextOptionsBuilder.AddInterceptors`/`ConfigureWarnings`, `WarningsConfigurationBuilder.Throw`, `CoreEventId`, `ChangeTracker.Clear`/`Entries`, `EntityState.Detached`), Microsoft.EntityFrameworkCore.Relational (`RelationalEventId`), LanguageExt.Core, Thinktecture.Runtime.Extensions.
- Growth: a new altitude row is one `SpineAltitude` case with its mount delegate; a new tracker policy is one `TrackerDisposition` row; a new escalated warning is one `EventId` in the `Throw` roster; zero new surface — a hand-rolled identity-resolution walk, a service-layer `try`/`catch` around a save, a per-call-site write-authority check, or a sync-only interceptor member is the deleted form.
- Boundary: write authority stops being a column callers honor and becomes the gate the store REFUSES at — the save altitude suppresses through `InterceptionResult<int>.SuppressWithResult` under a non-writing `Placement`, and a suppressing gate DECLARES its tracker disposition as a closed row because the next bracket otherwise inherits phantom dirty state the pool carries forward; BOTH modality twins are mandatory on every declared member, since each member carries a pass-through default and a sync-only interceptor compiles while silently leaving the async path unintercepted, which is the path every op leg takes; tracked-conflict policy selects the BUILT-IN `IIdentityResolutionInterceptor` pair as ONE row and a hand-rolled resolution walk is the deleted form; `IQueryExpressionInterceptor` output CACHES WITH THE QUERY, so only a rewrite that is a pure function of expression shape may ride it and a per-execution rewrite replays its first execution forever — the COMPILATION altitude therefore mounts an EMPTY roster on this tier, a recorded negative rather than an absence, because every op returns a value projection so no entity materializes for `IMaterializationInterceptor` and every per-execution value binds as a query parameter rather than a rewrite; the spine is provider-invariant and engine variance is carried as `StoreProfile` columns, while `ConfigureWarnings` is the escalation boundary turning chosen runtime warnings into typed failures at the options row; the SET-BASED and BULK lanes BYPASS the save altitude by construction and prove their own row counts on the op's result, so this spine claims NO coverage there — a spine advertising a gate over writes it cannot see is the false claim the carve deletes.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Data.Common;
using System.Text;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Rasm.Domain;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TrackerDisposition {
    public static readonly TrackerDisposition Clear = new("clear", static tracker => fun(tracker.Clear)());
    public static readonly TrackerDisposition Detach = new("detach",
        static tracker => toSeq(tracker.Entries()).Iter(static entry => entry.State = EntityState.Detached));
    public static readonly TrackerDisposition Hold = new("hold", static _ => unit);

    [UseDelegateFromConstructor]
    public partial Unit Settle(ChangeTracker tracker);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ResolutionPolicy {
    public static readonly ResolutionPolicy Ignoring = new("ignoring", static () => new IgnoringIdentityResolutionInterceptor());
    public static readonly ResolutionPolicy Updating = new("updating", static () => new UpdatingIdentityResolutionInterceptor());

    [UseDelegateFromConstructor]
    public partial IIdentityResolutionInterceptor Mount();
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SpineMount(Placement Placement, TrackerDisposition Disposition, ResolutionPolicy Resolution);

// --- [SERVICES] ------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SpineAltitude {
    public static readonly SpineAltitude Compilation = new("compilation", static _ => Seq<IInterceptor>());
    public static readonly SpineAltitude UnitOfWork = new("unit-of-work",
        static mount => Seq<IInterceptor>(mount.Resolution.Mount(), new IdentityWriteGate(mount.Placement, mount.Disposition)));

    [UseDelegateFromConstructor]
    public partial Seq<IInterceptor> Mount(SpineMount mount);
}

public sealed class IdentityWriteGate(Placement placement, TrackerDisposition disposition) : ISaveChangesInterceptor {
    public InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result) => Gate(eventData, result);

    public ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(Gate(eventData, result));

    InterceptionResult<int> Gate(DbContextEventData eventData, InterceptionResult<int> result) {
        if (placement.Held.Admits(PlacementAxis.Writes) || result.HasResult || eventData.Context is not { } store) { return result; }
        _ = disposition.Settle(store.ChangeTracker);
        return InterceptionResult<int>.SuppressWithResult(0);
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public static class IdentitySpine {
    static readonly Seq<SpineAltitude> Order = Seq(SpineAltitude.Compilation, SpineAltitude.UnitOfWork);

    public static DbContextOptionsBuilder Compose(DbContextOptionsBuilder options, SpineMount mount) {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(mount);
        return options.AddInterceptors(Order.Bind(altitude => altitude.Mount(mount)))
            .ConfigureWarnings(static warnings => warnings.Throw(
                CoreEventId.RowLimitingOperationWithoutOrderByWarning,
                CoreEventId.FirstWithoutOrderByAndFilterWarning,
                RelationalEventId.AmbientTransactionWarning,
                RelationalEventId.PendingModelChangesWarning));
    }
}
```

| [INDEX] | [POLICY]           | [VALUE]                                         | [BINDING]                                                       |
| :-----: | :----------------- | :---------------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | altitudes          | `SpineAltitude` two declared rows               | registration order is execution order; roster is spelled        |
|  [02]   | compilation row    | `Seq<IInterceptor>()`, recorded negative    | projections materialize no entity; no shape rewrite exists      |
|  [03]   | write gate         | `InterceptionResult<int>.SuppressWithResult`    | the `Writes` authority refuses at the store, not the call site  |
|  [04]   | tracker settlement | `TrackerDisposition` delegate row               | clear, detach, or hold; never phantom dirty state               |
|  [05]   | modality twins     | `SavingChanges` with `SavingChangesAsync`       | pass-through defaults leave the async path unintercepted        |
|  [06]   | tracked conflict   | built-in `IIdentityResolutionInterceptor` pair  | one row selects ignoring or updating                            |
|  [07]   | query rewrite      | `IQueryExpressionInterceptor` caches its output | pure expression-shape rewrites only; values bind as parameters  |
|  [08]   | warning escalation | `ConfigureWarnings(...Throw)`                   | chosen runtime warnings fail typed at the options row           |
|  [09]   | bulk carve         | `IdentityOp.Ingest` bypasses the save altitude  | the op proves its own row count; spine claims no coverage there |

## [06]-[KMS_CUSTODY]

- Owner: `KmsProvider` is the Persistence KMS axis carrying its two authorities as one `CapabilitySet<KmsCapability>` column whose law BARS native wrapping without signing; `KeyState` is its lifecycle vocabulary; `SigningAlgorithm` carries hash and provider spelling; `OpDigest` is an immutable canonical-hex value; `SigningKeyring` carries `Sign`/`Verify`; `EnvelopeAad`, `WrappedKey`, `WrapForm`, and `EnvelopeKeyring` own DEK custody; `CustodyVerdict` is the closed crypto verdict; `Custody` is the one authorship and DEK-envelope fold.
- Cases: `KmsProvider` is `None | Aws | Azure | Gcp`. `SigningAlgorithm` covers ES/PS/RS at each admitted digest width beside AWS `Ed25519`/`Ed25519Ph`, with provider support stored on each row. `KeyState` is `Enabled | Disabled | Destroyed | Scheduled | Pending`; `WrapForm` is `Bound | Remote`. `CustodyVerdict` covers digest width, algorithm/provider compatibility, authenticity, DEK envelope, and key lifecycle.
- Entry: `public static IO<CustodyVerdict> Attest(StoreActor actor, OpDigest digest, KmsProvider provider, string signingKeyId, SigningKeyring keyring, ProjectionContext frame)` signs an `OpDigest` after gating its width (the capability-absent local tier shorts to `Unsigned` so a store with no KMS still records the delta→actor binding); `Verify(SignedAuthorship, OpDigest, SigningKeyring)` checks the digest binding and signature; `Wrap(EnvelopeKeyring, EnvelopeAad, WrapForm)` probes the key lifecycle then mints per the form (`Bound` → plaintext + `WrappedKey`; `Remote` → wrapped-only, `Wrapped.Dek` empty); `Unwrap(EnvelopeKeyring, WrappedKey, EnvelopeAad)` recovers the plaintext DEK and the caller zeroizes it after the local bind; `Rewrap(EnvelopeKeyring, WrappedKey, EnvelopeAad)` advances the wrapping-key version without the plaintext crossing the wire — one DEK-envelope fold beside the one signing fold.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm (`CanonicalWriter.Retaining`/`ToBytes` — the framed AAD preimage; `ContentHash.Admit` — the digest half's one 16-byte admission), AWSSDK.KeyManagementService (signing `SigningAlgorithmSpec`/`SignAsync`/`VerifyAsync`/`MessageType.DIGEST`; DEK envelope `GenerateDataKeyAsync`/`GenerateDataKeyWithoutPlaintextAsync`/`DecryptAsync`/`ReEncryptAsync`; probe `DescribeKeyAsync`), Azure.Security.KeyVault.Keys (signing `SignatureAlgorithm`/`Sign`/`Verify`; native DEK envelope `CryptographyClient.WrapKey`/`UnwrapKey` over `KeyWrapAlgorithm.RsaOaep256`; `KeyClient` key-state), Google.Cloud.Kms.V1 (DEK envelope `EncryptAsync`/`DecryptAsync` + bidirectional CRC32C; `GenerateRandomBytesAsync` HSM-backed off-board DEK material — the Gcp `Mint` arm's DEK source; rotation `UpdateCryptoKeyPrimaryVersionAsync`; probe `GetCryptoKeyVersionAsync` `CryptoKeyVersionState`), System.Security.Cryptography (`CryptographicOperations.HashData`/`ZeroMemory`), System.Collections.Frozen.
- Growth: one `KmsProvider` row per new cloud KMS (a non-signing provider sets `Signs: false` and routes through the SAME `Unsigned` path; a native-wrap provider sets `NativeWrap: true` and binds `Mint`/`Rewrap` against its wrap verb rather than encrypt-as-wrap); one `SigningAlgorithm` row per JWS family; one `KeyState` row per lifecycle posture; one `WrapForm` row per mint modality; one `CustodyVerdict` case per verdict; zero new surface — a separate `Store/encryption` page, a second provider axis, or a Persistence-side long-lived DEK cache is the deleted form.
- Boundary: signed authorship is the actor-to-blame binding — a cloud-KMS op carries a `SignedAuthorship` over a `SigningAlgorithm`-width cryptographic `OpDigest` so a blame attribution (`Version/timetravel`, `Version/provenance#ATTESTED_LEDGER` — the consumer that chains these attestations) names a verified actor, a 16-byte non-cryptographic content hash standing in for the signed digest being the deleted form; the `SigningKeyring` is the KMS SIGNING surface (`Sign`/`Verify` over an asymmetric key, the disjoint operation from the DEK envelope), resolving the key through the AppHost `SecretLease`-class handle, never a bare passphrase, and the provider-specific algorithm type (`SigningAlgorithmSpec`/`SignatureAlgorithm`) lives only at the keyring delegate edge; the `EnvelopeKeyring` is the DEK-ENVELOPE surface this cluster holds beside the signing keyring on the ONE `KmsProvider` axis — the `Mint`/`MintSealed`/`Unwrap`/`Rewrap`/`Probe` family wrapping a data-encryption key against the symmetric CMK where each arm's mechanism is a policy value on the `KmsProvider` row (the `NativeWrap` provider routes through Azure's native `WrapKey`/`UnwrapKey`; the encrypt-as-wrap providers through AWS `GenerateDataKey`/`Decrypt`/`ReEncrypt` and GCP `Encrypt`/`Decrypt` + `UpdateCryptoKeyPrimaryVersion`, the GCP `Mint` sourcing its DEK bytes from `GenerateRandomBytesAsync` so key material is HSM-born, never process-entropy-born), the `Probe` arm resolving `KeyState` so a wrap against a `Disabled`/`Scheduled` key rejects `KeyUnusable` at admission; the `EnvelopeAad` (store partition + the tenant text streamed through the kernel `CanonicalWriter` and digested under SHA-256 so the AAD is a fixed-width opaque value, never a raw tenant uuid on the wire) rides the provider `EncryptionContext`/`AdditionalAuthenticatedData` on the AWS/GCP arms and is compared application-side on the Azure native-wrap arm (which carries no per-call AAD), so a DEK wrapped for one `(partition, tenant)` cannot be unwrapped under another; the recovered plaintext DEK zeroizes through `CryptographicOperations.ZeroMemory` immediately after the local bind so a Persistence-side long-lived key is the deleted form; the `Store/blobstore#BLOB_GC` `ObjectEncryption` is the downstream SSE-stance consumer carrying only the server-side-SSE key-id STRING this DEK envelope mints out-of-band, never a second DEK-envelope owner; the authz decision this fold NEVER makes is `Element/authority#AUTHORITY` `Admit` — custody proves WHO DID and KEEPS KEYS, authority decides WHO MAY, and the two verdicts stay two unions.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Globalization;
using Rasm.Domain;
using Rasm.Numerics;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KmsCapability : ICapability<KmsCapability> {
    public static readonly KmsCapability Signs = new("signs");
    public static readonly KmsCapability NativeWrap = new("native-wrap");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KmsProvider {
    public static readonly KmsProvider None = new("none");
    public static readonly KmsProvider Aws = new("aws", KmsCapability.Signs);
    public static readonly KmsProvider Azure = new("azure", KmsCapability.Signs, KmsCapability.NativeWrap);
    public static readonly KmsProvider Gcp = new("gcp", KmsCapability.Signs);
    public CapabilitySet<KmsCapability> Held { get; }

    public static readonly CapabilityLaw<KmsCapability> Law =
        CapabilityLaw<KmsCapability>.Forbidden(Seq(CapabilitySet<KmsCapability>.Of(KmsCapability.NativeWrap)));

    public static readonly CapabilitySet<KmsCapability> Signing = CapabilitySet<KmsCapability>.Of(KmsCapability.Signs);

    private KmsProvider(string key, params ReadOnlySpan<KmsCapability> held) : this(key) =>
        Held = CapabilitySet<KmsCapability>.Of(held);

    public static Fin<Unit> Lawful =>
        toSeq(Items).TraverseM(static row => Law.Admit(row.Held)).As().Map(static _ => unit);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KeyState {
    public static readonly KeyState Enabled = new("enabled", usable: true);
    public static readonly KeyState Disabled = new("disabled", usable: false);
    public static readonly KeyState Destroyed = new("destroyed", usable: false);
    public static readonly KeyState Scheduled = new("scheduled", usable: false);
    public static readonly KeyState Pending = new("pending", usable: false);
    public bool Usable { get; }
    private KeyState(string key, bool usable) : this(key) => Usable = usable;
}

[SmartEnum<string>]
public sealed partial class SigningAlgorithm {
    static readonly FrozenSet<string> Universal = FrozenSet.ToFrozenSet([KmsProvider.Aws.Key, KmsProvider.Azure.Key, KmsProvider.Gcp.Key], StringComparer.Ordinal);
    static readonly FrozenSet<string> AwsOnly = FrozenSet.ToFrozenSet([KmsProvider.Aws.Key], StringComparer.Ordinal);
    public static readonly SigningAlgorithm Es256 = new("es256", System.Security.Cryptography.HashAlgorithmName.SHA256, "ECDSA_SHA_256", Universal);
    public static readonly SigningAlgorithm Es384 = new("es384", System.Security.Cryptography.HashAlgorithmName.SHA384, "ECDSA_SHA_384", Universal);
    public static readonly SigningAlgorithm Es512 = new("es512", System.Security.Cryptography.HashAlgorithmName.SHA512, "ECDSA_SHA_512", Universal);
    public static readonly SigningAlgorithm Ps256 = new("ps256", System.Security.Cryptography.HashAlgorithmName.SHA256, "RSASSA_PSS_SHA_256", Universal);
    public static readonly SigningAlgorithm Ps384 = new("ps384", System.Security.Cryptography.HashAlgorithmName.SHA384, "RSASSA_PSS_SHA_384", Universal);
    public static readonly SigningAlgorithm Ps512 = new("ps512", System.Security.Cryptography.HashAlgorithmName.SHA512, "RSASSA_PSS_SHA_512", Universal);
    public static readonly SigningAlgorithm Rs256 = new("rs256", System.Security.Cryptography.HashAlgorithmName.SHA256, "RSASSA_PKCS1_V1_5_SHA_256", Universal);
    public static readonly SigningAlgorithm Rs384 = new("rs384", System.Security.Cryptography.HashAlgorithmName.SHA384, "RSASSA_PKCS1_V1_5_SHA_384", Universal);
    public static readonly SigningAlgorithm Rs512 = new("rs512", System.Security.Cryptography.HashAlgorithmName.SHA512, "RSASSA_PKCS1_V1_5_SHA_512", Universal);
    public static readonly SigningAlgorithm Ed25519 = new("ed25519", System.Security.Cryptography.HashAlgorithmName.SHA512, "ED25519_SHA_512", AwsOnly);
    public static readonly SigningAlgorithm Ed25519Ph = new("ed25519ph", System.Security.Cryptography.HashAlgorithmName.SHA512, "ED25519_PH_SHA_512", AwsOnly);
    public System.Security.Cryptography.HashAlgorithmName Hasher { get; }
    public string WireName { get; }
    public FrozenSet<string> Providers { get; }
    public int DigestWidth => Hasher == System.Security.Cryptography.HashAlgorithmName.SHA512 ? 64 : Hasher == System.Security.Cryptography.HashAlgorithmName.SHA384 ? 48 : 32;
    private SigningAlgorithm(string key, System.Security.Cryptography.HashAlgorithmName hasher, string wireName, FrozenSet<string> providers) : this(key) => (Hasher, WireName, Providers) = (hasher, wireName, providers);
    public bool Admits(KmsProvider provider) => Providers.Contains(provider.Key);
    public OpDigest Hash(ReadOnlySpan<byte> opBytes) { Span<byte> digest = stackalloc byte[64]; int written = System.Security.Cryptography.CryptographicOperations.HashData(Hasher, opBytes, digest); return OpDigest.Create(Convert.ToHexString(digest[..written])); }
}

[ValueObject<string>]
public readonly partial struct OpDigest {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = value.Length is 64 or 96 or 128 && value.All(Uri.IsHexDigit)
            ? null
            : new ValidationError($"<op-digest:{value.Length}>");
    public ReadOnlyMemory<byte> Bytes => Convert.FromHexString(Value);
    public int ByteLength => Value.Length / 2;
    public bool Fits(SigningAlgorithm algorithm) => ByteLength == algorithm.DigestWidth;
}

[SmartEnum]
public sealed partial class WrapForm {
    public static readonly WrapForm Bound = new();
    public static readonly WrapForm Remote = new();
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SigningKeyring(SigningAlgorithm Algorithm, Func<ReadOnlyMemory<byte>, IO<ReadOnlyMemory<byte>>> Sign, Func<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>, IO<bool>> Verify);
public sealed record SignedAuthorship(StoreActor Actor, KmsProvider Provider, string SigningKeyId, SigningAlgorithm Algorithm, OpDigest Digest, ReadOnlyMemory<byte> Signature, Instant At, CorrelationId Correlation);

[ComplexValueObject]
public sealed partial class EnvelopeAad {
    public string Partition { get; }
    public UInt128 TenantDigest { get; }
    public FrozenDictionary<string, string> Context => new Dictionary<string, string> { ["partition"] = Partition, ["tenant"] = TenantDigest.ToString(TenantId.Wire, CultureInfo.InvariantCulture) }.ToFrozenDictionary();
    public static Fin<EnvelopeAad> Of(string partition, TenantId tenant) =>
        CanonicalWriter.Retaining(EpsilonPolicy.ZeroTolerance).String(partition).String(tenant.Text).ToBytes()
            .Bind(framed => {
                Span<byte> digest = stackalloc byte[32];
                _ = System.Security.Cryptography.CryptographicOperations.HashData(System.Security.Cryptography.HashAlgorithmName.SHA256, framed.Span, digest);
                return ContentHash.Admit(digest[..16]).Map(half => new EnvelopeAad(partition, half));
            });
}

public readonly record struct WrappedKey(ReadOnlyMemory<byte> Ciphertext, string WrappingKeyId, string Version);

public sealed record EnvelopeKeyring(
    KmsProvider Provider,
    Func<EnvelopeAad, IO<(ReadOnlyMemory<byte> Dek, WrappedKey Wrapped)>> Mint,
    Func<EnvelopeAad, IO<WrappedKey>> MintSealed,
    Func<WrappedKey, EnvelopeAad, IO<ReadOnlyMemory<byte>>> Unwrap,
    Func<WrappedKey, EnvelopeAad, IO<WrappedKey>> Rewrap,
    Func<IO<KeyState>> Probe);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CustodyVerdict {
    private CustodyVerdict() { }
    public sealed record Attested(SignedAuthorship Authorship) : CustodyVerdict;
    public sealed record Authentic(StoreActor Actor, string SigningKeyId) : CustodyVerdict;
    public sealed record Unsigned(StoreActor Actor, OpDigest Digest, Instant At, CorrelationId Correlation) : CustodyVerdict;
    public sealed record Unauthored(OpDigest Expected, OpDigest Found) : CustodyVerdict;
    public sealed record Forged(StoreActor Actor, string SigningKeyId) : CustodyVerdict;
    public sealed record DigestWidth(int Expected, int Actual) : CustodyVerdict;
    public sealed record UnsupportedAlgorithm(KmsProvider Provider, SigningAlgorithm Algorithm) : CustodyVerdict;
    public sealed record AlgorithmMismatch(SigningAlgorithm Expected, SigningAlgorithm Found) : CustodyVerdict;
    public sealed record Wrapped(ReadOnlyMemory<byte> Dek, WrappedKey Key) : CustodyVerdict;
    public sealed record Unwrapped(ReadOnlyMemory<byte> Dek) : CustodyVerdict;
    public sealed record KeyUnusable(string WrappingKeyId, KeyState State) : CustodyVerdict;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Custody {
    public static IO<CustodyVerdict> Attest(StoreActor actor, OpDigest digest, KmsProvider provider, string signingKeyId, SigningKeyring keyring, ProjectionContext frame) =>
        !provider.Held.Admits(KmsCapability.Signs) ? IO.pure<CustodyVerdict>(new CustodyVerdict.Unsigned(actor, digest, frame.Now(), frame.Correlation))
        : !keyring.Algorithm.Admits(provider) ? IO.pure<CustodyVerdict>(new CustodyVerdict.UnsupportedAlgorithm(provider, keyring.Algorithm))
        : digest.Fits(keyring.Algorithm) ? keyring.Sign(digest.Bytes).Map(signature => (CustodyVerdict)new CustodyVerdict.Attested(new SignedAuthorship(actor, provider, signingKeyId, keyring.Algorithm, digest, signature, frame.Now(), frame.Correlation)))
        : IO.pure<CustodyVerdict>(new CustodyVerdict.DigestWidth(keyring.Algorithm.DigestWidth, digest.ByteLength));

    public static IO<CustodyVerdict> Verify(SignedAuthorship authorship, OpDigest digest, SigningKeyring keyring) =>
        !authorship.Provider.Held.Admits(KmsCapability.Signs) ? IO.pure<CustodyVerdict>(new CustodyVerdict.Unsigned(authorship.Actor, authorship.Digest, authorship.At, authorship.Correlation))
        : authorship.Algorithm != keyring.Algorithm ? IO.pure<CustodyVerdict>(new CustodyVerdict.AlgorithmMismatch(keyring.Algorithm, authorship.Algorithm))
        : !keyring.Algorithm.Admits(authorship.Provider) ? IO.pure<CustodyVerdict>(new CustodyVerdict.UnsupportedAlgorithm(authorship.Provider, keyring.Algorithm))
        : authorship.Digest != digest ? IO.pure<CustodyVerdict>(new CustodyVerdict.Unauthored(digest, authorship.Digest))
        : keyring.Verify(digest.Bytes, authorship.Signature).Map(valid => valid ? (CustodyVerdict)new CustodyVerdict.Authentic(authorship.Actor, authorship.SigningKeyId) : new CustodyVerdict.Forged(authorship.Actor, authorship.SigningKeyId));

    public static IO<CustodyVerdict> Wrap(EnvelopeKeyring keyring, EnvelopeAad aad, WrapForm form) =>
        from state in keyring.Probe()
        from verdict in state.Usable
            ? form.Switch(
                bound: () => keyring.Mint(aad).Map(static r => (CustodyVerdict)new CustodyVerdict.Wrapped(r.Dek, r.Wrapped)),
                remote: () => keyring.MintSealed(aad).Map(static k => (CustodyVerdict)new CustodyVerdict.Wrapped(ReadOnlyMemory<byte>.Empty, k)))
            : IO.pure<CustodyVerdict>(new CustodyVerdict.KeyUnusable("<mint>", state))
        select verdict;

    public static IO<CustodyVerdict> Unwrap(EnvelopeKeyring keyring, WrappedKey wrapped, EnvelopeAad aad) =>
        keyring.Unwrap(wrapped, aad).Map(static dek => (CustodyVerdict)new CustodyVerdict.Unwrapped(dek));

    public static IO<CustodyVerdict> Rewrap(EnvelopeKeyring keyring, WrappedKey wrapped, EnvelopeAad aad) =>
        from state in keyring.Probe()
        from verdict in state.Usable
            ? keyring.Rewrap(wrapped, aad).Map(static next => (CustodyVerdict)new CustodyVerdict.Wrapped(ReadOnlyMemory<byte>.Empty, next))
            : IO.pure<CustodyVerdict>(new CustodyVerdict.KeyUnusable(wrapped.WrappingKeyId, state))
        select verdict;
}
```

## [07]-[SCHEMA_VERDICT]

- Owner: `SchemaVerdict` the `[Union]` boot verdict; `Placement` the `[SmartEnum<string>]` write-authority axis carrying its authorities as one `CapabilitySet<PlacementAxis>` column under a declared `CapabilityLaw` of legal corners (the route-prescribed shape, declared here as the Persistence-Element owner); `IdentityFault` the `[Union]` identity-tier fault band deriving `Code` from its `[FaultCase]` roster on the kernel `Fault` floor (the `FaultBand.StoreIdentity` row — `Element/authority` composes this band, no band of its own); `ModelFingerprint` the MEASURED digest over a model's own metadata under a declared total order, and the generation's whole name because the compiled model is the artifact set's one source; `SchemaGate` the static surface folding the Marten startup posture, the store's published generation digest, and the object census into one typed verdict so boot is a total fold, never a best-effort open; `IdentityDdl` the generation owner — the EF.Design emission lanes beside the raw rows (RLS, extension installs, the generation stamp) the generated model cannot express.
- Cases: `SchemaVerdict` is `Serving` (the published generation equals the mounted model's digest), `Behind(Objects)` (declared relations the published namespace lacks, the successor generation's additions named for the operator), `Ahead(Objects)` (relations the compiled model cannot describe — the store's generation is newer than the binary, admitted only under a declared carry-forward invariant), `Absent` (no generation published, so nothing serves until one materializes); `IdentityFault` is `SchemaAhead(Unknown)` (undescribable relations the binary lacks), `ApplyFailed(Detail)` (a materialization or Marten apply throw), `MartenMismatch(Detail)` (the host-startup Marten assertion throw lifted onto the band), `CellUnresolvable(Detail)` (an H3 centroid that yields the invalid sentinel), `KeyMalformed(Detail)` (a persisted key failing width or strict UTF-8), `ModelStale(Mounted, Published)` (a mounted compiled model whose fingerprint differs from the generation the store publishes), `StoreRejected(Detail)` (a provider exception converted at the `#STORE_OPERATION_BRACKET` boundary), `CursorStale(Detail)` (a keyset cursor whose anchor row no longer exists), `WriteRefused(Detail)` (a mutating op refused at admission by placement or by an unverifiable retry).
- Entry: `public static Fin<SchemaVerdict> Admit(DbContext store, Placement placement, FrozenSet<string> census, Option<UInt128> published)` grades the published generation against the mounted model's own digest and carries the census purely as verdict evidence; `ModelFingerprint.Of(IModel)` mints that digest; `public static IO<SchemaVerdict> AdmitMarten(IDocumentStore store, Placement placement)` is the single-writer Marten apply leg over `store.Storage.ApplyAllConfiguredChangesToDatabaseAsync` followed by the `store.Advanced.ApplyRollingPartitionsAsync` roster roll, the fleet member's Marten posture being the host-registered `AssertDatabaseMatchesConfigurationOnStartup` gate whose throw lifts to `IdentityFault.MartenMismatch` — two legs, one band.
- Packages: Marten (the host-builder `ApplyAllDatabaseChangesOnStartup`/`AssertDatabaseMatchesConfigurationOnStartup` registrations + the runtime `IDocumentStore.Storage.ApplyAllConfiguredChangesToDatabaseAsync(AutoCreate?)`, `IDocumentStore.Advanced.ApplyRollingPartitionsAsync`, and the daemon `RebuildProjectionAsync`), Microsoft.EntityFrameworkCore (`Database.EnsureCreated`/`EnsureDeleted`, `GenerateCreateScript`, `DbContext.Model`, `AccessorExtensions.GetService<TService>`, `IModel.GetEntityTypes`, `IEntityType.GetProperties`/`GetKeys`/`GetIndexes`), Microsoft.EntityFrameworkCore.Relational (`IRelationalDatabaseCreator.CreateTables`/`HasTables`, `StoreObjectIdentifier.Create(IReadOnlyTypeBase, StoreObjectType)`, `IProperty.GetColumnName(in StoreObjectIdentifier)`/`GetColumnType(in StoreObjectIdentifier)`, `IKey.GetName(in StoreObjectIdentifier)`, `IIndex.GetDatabaseName(in StoreObjectIdentifier)`), Microsoft.EntityFrameworkCore.Design (`PrivateAssets=all` — `DbContextOperations.Optimize` compiled model; the package earns its admission HERE), Rasm (`ContentHash.Of<TState>` + `CanonicalWriter.String`/`Bool`/`Sorted`/`Rows` — the fingerprint preimage on the one alphabet), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new boot outcome is one `SchemaVerdict` case; a new identity-tier failure is one `IdentityFault` case; a new non-modelable DDL fact is one `IdentityDdl` row the generation script appends; zero new surface — a best-effort open, a per-process bootstrap branch, a bare `Error.New`, an assembly of hand-authored schema deltas, or apply-time gating is the deleted form because boot is one total fold, the failures are one typed band, emission is generated, and a shape change replaces the whole generation.
- Boundary: TWO DDL owners compose at boot — Marten owns its event/document DDL and the EF identity model owns its relational DDL — and each owner's posture is selected by the SAME `Placement` write authority: the single-writer placement MATERIALIZES (`IRelationalDatabaseCreator.CreateTables` into the unpublished namespace, Marten `ApplyAllConfiguredChangesToDatabaseAsync` and the host `ApplyAllDatabaseChangesOnStartup` registration) while every fleet member ASSERTS and never materializes, `AutoCreate.None` holding on both so the runtime asserts its configuration and creates nothing implicitly — the `Store/provisioning#SERVER_EXTENSIONS` `RollingWindow` roster rolls on that SAME single-writer leg because a partition's leading edge provisions purely additively yet the trailing-edge drop is the destructive half no assertion may carry, and a fleet member rolls neither edge; the GENERATION OWNER law: EF.Design EMITS — the compiled model renders the `element_identity`/`node_cell` artifacts under `IdentityDesignFactory` (the snake-case names, the Thinktecture-converted column types, the geometry column with its index method, and every embedded divergence all derive from the shape row, so each profile's generation renders through the one owner into that profile's own emission namespace), `GenerateCreateScript` yields the reviewed generation script the deploy plane runs, and `Optimize` runs ONCE PER PROFILE so each `StoreProfile.Model` slot mounts its own compiled model; the raw rows the model CANNOT express — the RLS enable+force rows with the two-arm tenant/maintenance-plane policies off the kernel `SessionCoordinate` anchors, the frozen `Store/provisioning#SERVER_EXTENSIONS` `ServerExtension.CreateSql` install rows (`postgis`, `h3-pg`, `vector` — the extension DDL commits through THIS path, `ServerExtension` stays the frozen row vocabulary), and the generation stamp whose value digests the model that otherwise declares it — are `IdentityDdl` data the generation script appends as raw statements; materialization runs as ONE transaction into a namespace no session's `search_path` names and publishes by renaming it over the live name, so a torn build publishes nothing and the successor re-runs whole from an empty namespace — resume from a partial build is the declared loss, priced against a half-materialized store no verdict can classify; each relation declares its rebuild posture before it ships (`Derived` rebuilt from truth inside the materialization, `Carried` lifted by its own declared `INSERT … SELECT` inside the cutover, `Resident` outside every generation), and read models are `Derived` by construction — the projection daemon's `RebuildProjectionAsync` replays the event log, so no projection carries a state-preserving upgrade path; a census carrying relations this model cannot describe is a typed `IdentityFault.SchemaAhead` (or `Ahead` under a read-ahead placement), never a silent open that corrupts on first write, and read-ahead serving is legal only under a declared carry-forward invariant so the sound default is hard rejection; the route-owned `docs/stacks/csharp/domain/persistence#GENERATION_ALGEBRA` `SchemaGate`/`Placement`/`SchemaVerdict` is the general EF FORM this page realizes and extends with the Marten apply/assert leg; `Database.EnsureCreated` paired with `EnsureDeleted` is the ephemeral test row's arm, owning its whole store; the FINGERPRINT gate is the one drift proof — a generation materialized against a compiled model nobody regenerated leaves the mounted model describing columns it has never seen, so the mounted model's MEASURED digest compares against the digest the store published at cutover and a mismatch answers `IdentityFault.ModelStale` carrying both; the fingerprint is DERIVED, never asserted, and a zero standing in for an unmeasured digest spells no absence — so its preimage folds a canonical projection of entity-type names, table identity, property names, column names, column types, nullability, and key and index declarations under a declared total order through the kernel `ContentHash.Of`, length-framing every variable-width field and count-framing every collection so no separator-joined concatenation exists; key and index property lists keep DECLARED order because order is semantics for a composite prefix; the BUILD-TIME half — regenerate-and-diff, which catches a model edit reaching neither artifacts nor compiled model — belongs to the test suite under `tests/README.md` and `tests/RULINGS.md` law and is this gate's counterpart, never authored here.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Rasm.Domain;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PlacementAxis : ICapability<PlacementAxis> {
    public static readonly PlacementAxis Writes = new("writes");
    public static readonly PlacementAxis Materializes = new("materializes");
    public static readonly PlacementAxis ReadsAhead = new("reads-ahead");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Placement {
    public static readonly Placement SingleWriter = new("single-writer", PlacementAxis.Writes, PlacementAxis.Materializes);
    public static readonly Placement FleetMember = new("fleet-member", PlacementAxis.Writes);
    public static readonly Placement Reader = new("reader", PlacementAxis.ReadsAhead);
    public CapabilitySet<PlacementAxis> Held { get; }

    public static readonly CapabilityLaw<PlacementAxis> Law = new(Seq(
        CapabilitySet<PlacementAxis>.Of(PlacementAxis.Writes, PlacementAxis.Materializes),
        CapabilitySet<PlacementAxis>.Of(PlacementAxis.Writes),
        CapabilitySet<PlacementAxis>.Of(PlacementAxis.ReadsAhead)));

    public static readonly CapabilitySet<PlacementAxis> Mutating = CapabilitySet<PlacementAxis>.Of(PlacementAxis.Writes);
    public static readonly CapabilitySet<PlacementAxis> Materializing = CapabilitySet<PlacementAxis>.Of(PlacementAxis.Materializes);

    private Placement(string key, params ReadOnlySpan<PlacementAxis> held) : this(key) =>
        Held = CapabilitySet<PlacementAxis>.Of(held);

    public static Fin<Unit> Lawful =>
        toSeq(Items).TraverseM(static row => Law.Admit(row.Held)).As().Map(static _ => unit);
}

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SchemaVerdict {
    private SchemaVerdict() { }
    public sealed record Serving : SchemaVerdict;
    public sealed record Behind(Seq<string> Objects) : SchemaVerdict;
    public sealed record Ahead(Seq<string> Objects) : SchemaVerdict;
    public sealed record Absent : SchemaVerdict;
}

// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record IdentityFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.StoreIdentity;
    private IdentityFault() { }

    [FaultCase(0)] public sealed partial record ModelIncomplete(Seq<string> Absent) : IdentityFault;
    [FaultCase(1)] public sealed partial record SchemaAhead(Seq<string> Unknown) : IdentityFault;
    [FaultCase(2)] public sealed partial record ApplyFailed(Error Cause) : IdentityFault, ICausedFault;
    [FaultCase(3)] public sealed partial record MartenMismatch(string Detail) : IdentityFault;
    [FaultCase(4)] public sealed partial record CellUnresolvable(string Detail) : IdentityFault;
    [FaultCase(5)] public sealed partial record KeyMalformed(string Detail) : IdentityFault;
    [FaultCase(6)] public sealed partial record ModelStale(UInt128 Mounted, UInt128 Published) : IdentityFault;
    [FaultCase(7)] public sealed partial record StoreRejected(string Verb, Error Cause, Retriability Class) : IdentityFault, ICausedFault {
        public override Retriability Retriability => Class;
    }
    [FaultCase(8)] public sealed partial record CursorStale(string Detail) : IdentityFault;
    [FaultCase(9)] public sealed partial record WriteRefused(string Detail) : IdentityFault;

    public override string Message => Switch(
        schemaAhead:      static c => $"<schema-ahead:{c.Unknown.Count}>",
        applyFailed:      static c => $"<apply-failed:{c.Cause.Message}>",
        martenMismatch:   static c => $"<marten-mismatch:{c.Detail}>",
        cellUnresolvable: static c => $"<cell-unresolvable:{c.Detail}>",
        keyMalformed:     static c => $"<key-malformed:{c.Detail}>",
        modelStale:       static c => $"<model-stale:{c.Mounted:x32}!={c.Published:x32}>",
        storeRejected:    static c => $"<store-rejected:{c.Verb}:{c.Cause.Message}>",
        cursorStale:      static c => $"<cursor-stale:{c.Detail}>",
        writeRefused:     static c => $"<write-refused:{c.Detail}>",
        modelIncomplete:  static c => $"<model-incomplete:{string.Join(',', c.Absent)}>");

    public static Option<StoreRejected> Rejected(string verb, Error error) =>
        error.Exception.Case is DbException failure
            ? Some(new StoreRejected(verb, error,
                failure.IsTransient ? Retriability.Transient : Retriability.Terminal))
            : None;

}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ModelFingerprint {
    public static UInt128 Of(IModel model) {
        ArgumentNullException.ThrowIfNull(model);
        return ContentHash.Of(model, static (mounted, w) =>
            w.Sorted(toSeq(mounted.GetEntityTypes()), static type => type.Name, StringComparer.Ordinal, Entity));
    }

    static void Entity(IEntityType type, CanonicalWriter w) {
        w.String(type.Name);
        if (StoreObjectIdentifier.Create(type, StoreObjectType.Table) is not { } table) {
            w.String(string.Empty).String(string.Empty).Ordinal(0).Ordinal(0).Ordinal(0);
            return;
        }
        w.String(table.Name).String(table.Schema ?? string.Empty)
         .Sorted(toSeq(type.GetProperties()), static property => property.Name, StringComparer.Ordinal,
            (property, x) => x.String(property.Name).String(property.GetColumnName(table) ?? string.Empty).String(property.GetColumnType(table)).Bool(property.IsNullable))
         .Sorted(toSeq(type.GetKeys()), key => key.GetName(table) ?? string.Empty, StringComparer.Ordinal,
            (key, x) => Columns(x.String(key.GetName(table) ?? string.Empty), table, toSeq(key.Properties)))
         .Sorted(toSeq(type.GetIndexes()), index => index.GetDatabaseName(table) ?? string.Empty, StringComparer.Ordinal,
            (index, x) => Columns(x.String(index.GetDatabaseName(table) ?? string.Empty).Bool(index.IsUnique), table, toSeq(index.Properties)));
    }

    static CanonicalWriter Columns(CanonicalWriter w, StoreObjectIdentifier table, Seq<IProperty> properties) =>
        w.Rows(properties, (property, x) => { x.String(property.GetColumnName(table) ?? string.Empty); });
}

public static class IdentityDdl {
    public static readonly Seq<string> Rls = toSeq(new[] {
        "ALTER TABLE element_identity ENABLE ROW LEVEL SECURITY",
        "ALTER TABLE element_identity FORCE ROW LEVEL SECURITY",
        $"CREATE POLICY element_identity_tenant ON element_identity USING (tenant = current_setting('{SessionCoordinate.Tenant.Guc}', true) OR current_setting('{SessionCoordinate.Plane.Guc}', true) = '{SessionCoordinate.Maintenance}') WITH CHECK (tenant = current_setting('{SessionCoordinate.Tenant.Guc}', true) OR current_setting('{SessionCoordinate.Plane.Guc}', true) = '{SessionCoordinate.Maintenance}')",
        "ALTER TABLE node_cell ENABLE ROW LEVEL SECURITY",
        "ALTER TABLE node_cell FORCE ROW LEVEL SECURITY",
        $"CREATE POLICY node_cell_tenant ON node_cell USING (tenant = current_setting('{SessionCoordinate.Tenant.Guc}', true) OR current_setting('{SessionCoordinate.Plane.Guc}', true) = '{SessionCoordinate.Maintenance}') WITH CHECK (tenant = current_setting('{SessionCoordinate.Tenant.Guc}', true) OR current_setting('{SessionCoordinate.Plane.Guc}', true) = '{SessionCoordinate.Maintenance}')",
    });

    public static Seq<string> Stamp(UInt128 digest) => toSeq(new[] {
        "CREATE TABLE IF NOT EXISTS schema_generation (digest TEXT PRIMARY KEY)",
        $"INSERT INTO schema_generation (digest) VALUES ('{digest:x32}')",
    });
}

public static class SchemaGate {
    public static Fin<SchemaVerdict> Admit(DbContext store, Placement placement, FrozenSet<string> census, Option<UInt128> published) {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(placement);
        UInt128 compiled = ModelFingerprint.Of(store.Model);
        Seq<string> declared = toSeq(store.Model.GetEntityTypes())
            .Map(static type => StoreObjectIdentifier.Create(type, StoreObjectType.Table) is { } table ? table.Name : string.Empty)
            .Filter(static name => name.Length > 0);
        Seq<string> unknown = toSeq(census).Filter(name => !declared.Exists(held => held == name));
        return published.Case switch {
            null when placement.Held.Admits(PlacementAxis.Materializes) => Materialized(store),
            null => Fin<SchemaVerdict>.Succ(new SchemaVerdict.Absent()),
            UInt128 held when held == compiled => Fin<SchemaVerdict>.Succ(new SchemaVerdict.Serving()),
            _ when unknown.IsEmpty => Fin<SchemaVerdict>.Succ(
                new SchemaVerdict.Behind(declared.Filter(name => !census.Contains(name)))),
            _ when placement.Held.Admits(PlacementAxis.ReadsAhead) => Fin<SchemaVerdict>.Succ(new SchemaVerdict.Ahead(unknown)),
            _ => Fin<SchemaVerdict>.Fail(new IdentityFault.SchemaAhead(unknown)),
        };
    }

    static Fin<SchemaVerdict> Materialized(DbContext store) =>
        Try.lift(() => Fin.Succ(fun(() => store.GetService<IRelationalDatabaseCreator>().CreateTables()))).Run().Bind(static inner => inner)
            .Map(static _ => (SchemaVerdict)new SchemaVerdict.Serving())
            .MapFail(static error => new IdentityFault.ApplyFailed(error));

    public static IO<SchemaVerdict> AdmitMarten(IDocumentStore store, Placement placement) =>
        placement.Held.Admits(PlacementAxis.Materializes)
            ? IO.liftAsync(async () => await Try.lift(async _ => {
                await store.Storage.ApplyAllConfiguredChangesToDatabaseAsync().ConfigureAwait(false);
                await store.Advanced.ApplyRollingPartitionsAsync().ConfigureAwait(false);
                return Fin<SchemaVerdict>.Succ(new SchemaVerdict.Serving());
            }).Run().Bind(static inner => inner).ConfigureAwait(false))
                .Bind(result => IO.lift(result.MapFail(static error => new IdentityFault.ApplyFailed(error))))
            : IO.pure<SchemaVerdict>(new SchemaVerdict.Serving());
}
```

| [INDEX] | [POLICY]          | [VALUE]                                     | [BINDING]                                                         |
| :-----: | :---------------- | :------------------------------------------ | :---------------------------------------------------------------- |
|  [01]   | Marten DDL        | `AutoCreate.None` (every placement)         | writer applies explicitly; the fleet asserts configuration match  |
|  [02]   | EF identity DDL   | one generation per profile                  | shape row renders that profile's script; the script is reviewed   |
|  [03]   | non-modelable DDL | `Rls` + `Extensions` + `Stamp` rows         | raw statements the generation script appends inside the one txn   |
|  [04]   | boot verdict      | `SchemaGate.Admit` (Marten, digest, census) | ahead and stale-model are typed faults, never a silent open       |
|  [05]   | model fingerprint | measured `ModelFingerprint` both sides      | mounted model against the published stamp; framed, never asserted |
|  [06]   | materialization   | single-writer placement only                | every other placement waits on the deploy plane                   |
|  [07]   | deploy lane       | `GenerateCreateScript`                      | the generation script the deploy plane runs                       |
|  [08]   | deploy lane       | fresh-namespace cutover                     | build unpublished, publish by rename, one transaction             |
|  [09]   | deploy lane       | `Optimize`, once per profile                | each `StoreProfile.Model` slot mounts its own compiled model      |
|  [10]   | read models       | `RebuildProjectionAsync` from the log       | projections are `Derived`; replay is their whole upgrade path     |
|  [11]   | partition roll    | `ApplyRollingPartitionsAsync` (writer)      | both edges in one pass, ahead of any assertion; fleet rolls none  |

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
