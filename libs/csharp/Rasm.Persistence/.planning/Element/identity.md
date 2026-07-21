# [PERSISTENCE_ELEMENT_IDENTITY]

Rasm.Persistence anchors every persisted `ElementGraph` to one relational identity tier that commits ATOMICALLY with the Marten event in the same `IDocumentSession`: `ElementIdentity` is the per-model document/row carrying the `Element/graph#STREAM_GRAIN` `ModelId` PK, the `Tenant` RLS column, the set of rooted `NodeId`s, the Bim-projected IFC GlobalId strings (each rooted node's seam `Node.Object.ExternalId`), the H3 spatial cell, the PostGIS `Bounds` polygon, the pgvector embedding reference, the `ObjectAcl` (the `Element/authority` frozen vocabulary), and the classification — so identity plus event are one transaction with no two-ORM gap and the relational columns serve the spatial/vector/ACL/tenant lanes off the one tier. The EF surface is GENERATED, never hand-mapped: `ConverterRail.Compose` mounts `UseThinktectureValueConverters(Configuration.Default)` + `UseSnakeCaseNamingConvention()` + the provider row (`UseNpgsql(…, UseNetTopologySuite() + UseNodaTime() + UseVector())` or `UseSqlite`) on the ONE `IdentityContext`, so every `[ValueObject]`/`[SmartEnum]`/keyed-`[Union]` column converts with zero hand-written converter classes and only the LanguageExt carrier forms (`Option<Vector>`/`Seq<NodeId>`/`HashMap`) keep their Persistence-owned conversions. `IdentityPolicy` is the `[SmartEnum<string>]` key axis dispatching mint and decode per row through one generated `Switch`, big-endian transcription preserving order, so an identity change is an expand-wave second key plus a derivation flip plus a contract-wave drop, never an `AlterColumn`. `#KMS_CUSTODY` is the crypto tier the authz split leaves here (`Element/authority` owns WHO MAY; this page owns PROOF and KEYS): `SignedAuthorship` is the KMS-signed actor attestation tying a delta to a verified blame `StoreActor`, `Custody` folds attestation, verification, and DEK envelope minting/unwrapping into one `CustodyVerdict`, and `EnvelopeKeyring` is the DEK envelope surface the SAME `KmsProvider` axis selects beside `SigningKeyring` — provider-neutral `Mint`/`MintSealed`/`Unwrap`/`Rewrap`/`Probe` delegates wrapping a data-encryption key against the cloud CMK (AWS `GenerateDataKey`/`GenerateDataKeyWithoutPlaintext`/`Decrypt`/`ReEncrypt` encrypt-as-wrap, Azure native `WrapKey`/`UnwrapKey`, GCP `Encrypt`/`Decrypt` + CRC32C + `UpdateCryptoKeyPrimaryVersion` primary repoint), so the `#KEY_ENVELOPE` owner is THIS tier and the `Store/blobstore#BLOB_GC` `ObjectEncryption` consumes only the server-side-SSE key-id STRING this envelope mints out-of-band. The boot verdict folds the Marten startup posture plus the EF migration state into one typed `SchemaVerdict`, and the migration owner (`IdentityDdl` + the EF.Design emission lanes) generates BOTH providers' DDL through the one model. Every identity-tier failure rails the typed `IdentityFault` band (`FaultBand.Identity`, 834x — `Element/authority` composes it, no new band). `ModelId`/`StoreActor`/`ProjectionContext` arrive from the Persistence sibling `Element/graph#STORE_RAIL`; `NodeId`/`ContentAddress` from `Rasm.Element`; `ContentHash.Of` from the `Rasm` kernel; only the `SecretLease`-class KMS handle crosses from `Rasm.AppHost` through the `Runtime/secrets#KMS_UNWRAP_PORT` seam (the host resolves and leases the cloud-KMS credential; the concrete provider axis stays Persistence-side).

## [01]-[INDEX]

- [02]-[ELEMENT_IDENTITY]: the relational identity tier, the generated converter rail over both provider rows, the co-transactional Marten-document stamp, and the H3/PostGIS/vector/tenant join columns.
- [03]-[IDENTITY_POLICY]: the key axis, big-endian transcription, per-row mint/decode, and content addressing.
- [04]-[KMS_CUSTODY]: KMS-signed authorship, the DEK-envelope `#KEY_ENVELOPE` keyring (`Mint`/`MintSealed`/`Unwrap`/`Rewrap`/`Probe`), and the one `Custody` attestation/envelope fold over `CustodyVerdict`.
- [05]-[SCHEMA_VERDICT]: the boot fold over the Marten startup-assertion posture plus the EF migration state, and the `IdentityDdl` migration owner.

## [02]-[ELEMENT_IDENTITY]

- Owner: `ElementIdentity` the per-model identity row carrying the `ModelId` PK plus the `Tenant`/`Roots`/`GlobalIds`/`Cell`/`Bounds`/`Embedding`/`Acl`/`Classification`/`At` join columns; `NodeCell` the per-ELEMENT fine-cell routing-vertex row (`Model`/`Node`/`Tenant`/`Cell`) the `Query/cypher#GRAPH_QUERY` `pgrouting` `network_edge` source/target carries and `NodeAt` resolves; `StoreBinding` the `[Union]` provider row (`Postgres(NpgsqlDataSource)` / `Embedded(DbConnection)`) the one converter rail discriminates; `ConverterRail` the ONE options composition mounting the generated Thinktecture converters, the snake-case naming convention, and the provider plugin stack (the postgres row mounts `UseNetTopologySuite()` + `UseNodaTime()` + `UseVector()` so the geometry, `Instant`, and `vector(N)` columns all map through the one options entry); `IdentityContext` the one `DbContext` whose `OnModelCreating` keys the provider-divergent rows on `Database.IsSqlite()`; `IdentityShape`/`NodeCellShape` the `IEntityTypeConfiguration` mappings carrying ONLY what the conventions cannot derive — the LanguageExt carrier conversions, the jsonb ACL, the geometry column, and the indexes; `IdentityStore` the static surface owning the co-transactional model-derived upsert stamp (`Bind` derives the statement from the compiled EF model at boot; `Stamp` queues it on the Marten session), the spatial cell mints, the H3 neighborhood, the PostGIS predicate lane with its vertical-band clause, and the cell→NodeId resolver.
- Cases: `Roots` is the set of rooted `NodeId`s the model owns (the `IfcRoot` mirror nodes), `GlobalIds` the 1:1 map from rooted `NodeId` to the compressed IFC GlobalId string projected from each seam `Node.Object.ExternalId` (the rooted `NodeId` is the neutral kernel-minted durable key, the IFC GlobalId is the `ExternalId` projection the `Version/merge#STRUCTURAL_DIFF` re-ingest `Reconcile` correlates on, never the key), `Cell` the Uber-H3 cell over the model envelope centroid (bucket-equality joins), `Bounds` the `Envelope`-derived `geometry(Polygon, 4326)` PostGIS column plus the `ZMin`/`ZMax` vertical span (the three rows on the ONE spatial-key axis: cells for bucket joins, geometry for exact XY predicates, z-span for storey banding), `Embedding` the optional pgvector reference keying the ANN lane — the per-model envelope locator, distinct in grain from the corpus-grain retrieval index (`Query/retrieval`), `Acl` the `Element/authority` `ObjectAcl` grant, `Classification` the `DataClassification` ceiling.
- Entry: `IdentityStore.Bind(IModel)` derives an immutable `IdentityWriter` from one compiled provider model; `Stamp(IDocumentSession, ElementIdentity, IdentityWriter)` queues it on the event session. `Cell(Envelope, int)` mints either model or element cells without a forwarding sibling. `Nearby` uses `GridDiskDistancesSafe`; `Within` owns the exact PostGIS lane; `Bulk(IdentityContext, Seq<NodeCell>, NodeCellBulkPolicy, CancellationToken)` selects provider COPY versus embedded batching at composition.
- Auto: the identity row rides the one `IDocumentSession` the `Element/graph#STORE_RAIL` write op uses. `IdentityWriter` captures the provider-specific table, schema, primary key, relational casts, and value converters at composition; no process-global writer can accidentally reuse a PostgreSQL model for SQLite. `UseThinktectureValueConverters(Configuration.Default)` converts generated owners, while Persistence-owned conversions cover LanguageExt carriers, recursive ACL JSON, geometry, and the `UInt128` tenant's canonical `x32` text. `H3Index.FromPoint` mints cells and rejects `H3Index.Invalid`. RLS compares canonical tenant text with `current_setting('rasm.tenant')` without a fictional `UInt128`→`uuid` provider mapping.
- Receipt: an identity stamp rides `store.element.identity` carrying the `Roots` count; a spatial-neighborhood read rides `store.identity.nearby` carrying the ring radius; a predicate read rides `store.identity.within` carrying the range.
- Packages: Marten (`IDocumentSession.QueueSqlCommand`), Npgsql.EntityFrameworkCore.PostgreSQL (`UseNpgsql`), Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite (`UseNetTopologySuite` + `IsWithinDistance`/`DistanceKnn`), Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime (`Instant`), Thinktecture.Runtime.Extensions.EntityFrameworkCore10 (`UseThinktectureValueConverters`), EFCore.NamingConventions (`UseSnakeCaseNamingConvention`), Microsoft.EntityFrameworkCore.Sqlite (`UseSqlite`/`IsSqlite`), Pgvector.EntityFrameworkCore (`UseVector`), pocketken.H3 (`H3Index.FromPoint`/`GridDiskDistancesSafe`), NetTopologySuite, linq2db.EntityFrameworkCore (`BulkCopyAsync` + `BulkCopyOptions`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime.
- Growth: a new identity join column is one field on `ElementIdentity` — the conventions derive its mapping unless it is a LanguageExt carrier or a geometry, in which case ONE `IdentityShape` clause joins the residual set; a new spatial resolution is one H3 cell policy; a new provider is one `StoreBinding` case plus its `IsSqlite()`-analog model rows; zero new surface — a separate identity transaction, a second identity ORM committing apart from the event, a parallel `NodeId`-keyed identity table, a hand ADO mapping beside the generated rail, or an EF-versus-Marten atomicity dance is the deleted form.
- Boundary: the ONE transaction owner for identity plus event is the `IDocumentSession` — `IdentityStore.Stamp` is the lone stamp primitive the `GraphStore` rail composes (a queued model-derived upsert, never a Marten document: a document is an id+jsonb row structurally incapable of being the EF-shaped relation, and a `session.Store(identity)` claiming otherwise was the split-brain the queued statement deletes); EF/Npgsql is the READ projection plus the DDL owner over the ONE declared `element_identity` relation, never a second write authority (a `DbContext.SaveChanges` over the identity table beside the Marten append is the deleted two-ORM gap); the rooted `NodeId` is the neutral kernel-minted DURABLE key and the IFC GlobalId is each node's seam `Node.Object.ExternalId` projection the `GlobalIds` map mirrors — a re-import mints fresh neutral `NodeId`s and the `Version/merge#STRUCTURAL_DIFF` `Reconcile` aligns them back on the stable GlobalId, so the durable key and every foreign reference survive re-import unchanged; THREE spatial planes live on the one tier and never duplicate — the H3 CELL plane (`Cell` per-model + `NodeCell.Cell` per-element, both `bigint` reinterpretations matching the `h3-pg` convention, bucket-equality joins the GiST/BRIN index answers), the GEOMETRY plane (`Bounds` `geometry(Polygon, 4326)` + GiST, exact XY predicates riding the `EF.Functions` translators SERVER-side — `IsWithinDistance` → `ST_DWithin`, `DistanceKnn` → the `<->` KNN order, `.Intersects`/`.Distance` instance translators, `ST_Union`/`ST_Extent` aggregates — never a client scan), and the VERTICAL plane (`ZMin`/`ZMax` per-model span + `NodeCell.Z` per-element elevation, plain indexed range predicates, so stacked elements sharing a footprint discriminate server-side and a storey band is one clause on `Within`/`NodeAt`, never a client elevation scan); the `Bounds` producer contract is the seam-stable representation-bounds projection `Rasm.Element` provides (Persistence is the recorded demanding consumer); `Bounds` is a nullable CLR `Polygon?` because the PostGIS translators bind the CLR geometry type directly in LINQ predicates — an `Option` wrapper here would forfeit server-side translation, so null IS the absent-bounds state at this EF boundary; the EMBEDDED floor is provider-divergent model DATA on the one context, never a second mapping: `Database.IsSqlite()` keys the rows — `Bounds` degrades to a WKB `byte[]` column beside the H3 `bigint` cell (no PostGIS on the floor), `GlobalIds` stores as `text`, and NO vector column exists (the embedded charter is the relational identity floor + `EngineOps` checkpoint tier, never SoR and never ANN); the per-ELEMENT `NodeCell` grain stays element-distinct so the `pgrouting` cell-mesh route lands back on real element ids; the `Tenant` RLS column is the coarse partition and the `Element/authority` `ObjectAcl` the fine within-tenant grant, two altitudes never duplicated; the bulk lane is the linq2db `BulkCopyAsync` COPY lift over the ONE `IdentityContext` (the re-ingest `NodeCell` mass-stamp), never a second context and never a hand `NpgsqlBinaryImporter` beside the generated model.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Data.Common;
using H3;
using H3.Algorithms;
using LanguageExt;
using LinqToDB.EntityFrameworkCore;
using Marten;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NodaTime;
using Npgsql;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Thinktecture;
using Thinktecture.EntityFrameworkCore;
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Element;

// --- [TYPES] ---------------------------------------------------------------------------
// The H3 cell as the immutable durable spatial key: the pocketken.H3 `H3Index` is a MUTABLE class wrapping a `ulong`,
// so the tier stores the cell as the `long` reinterpretation (the `bigint`/`h3-pg` convention, bit-exact round-trip)
// and never shares a live `H3Index` across a fold. `Of` reinterprets the cell `ulong`; `Live` rehydrates the managed
// instance for an algebra call. A zero cell is `H3Index.Invalid` and never persists (it rails CellUnresolvable at mint).
[ValueObject<long>]
public readonly partial struct H3Cell {
    public static H3Cell Of(H3.H3Index cell) => Create(unchecked((long)(ulong)cell));
    public H3.H3Index Live => new((ulong)Value);
}

// The provider row the one converter rail discriminates: postgres is the SoR spine, sqlite the embedded
// relational identity floor (`Store/provisioning#StoreProfile.Embedded` binds it; raw ADO keeps EngineOps).
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StoreBinding {
    private StoreBinding() { }
    public sealed record Postgres(NpgsqlDataSource Source) : StoreBinding;
    public sealed record Embedded(DbConnection Connection) : StoreBinding;
}

// --- [MODELS] --------------------------------------------------------------------------
// `Bounds` is a nullable CLR `Polygon` (SRID 4326): the PostGIS translators bind the CLR geometry type
// directly in LINQ predicates, so an Option wrapper would forfeit server-side translation — null IS the
// absent-bounds state at this EF boundary (embedded floor / pre-bounds rows).
// `ZMin`/`ZMax` are the vertical-span columns beside the XY footprint — the third spatial plane (cells for
// bucket joins, footprint for exact XY predicates, z-span for storey banding), so two elements stacked on
// different storeys stop being server-indistinguishable; null means no vertical extent recorded (the same
// absent-state law as `Bounds`).
public sealed record ElementIdentity(
    ModelId Model,
    UInt128 Tenant,
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

// The per-ELEMENT fine-cell routing-vertex row: one rooted `NodeId` -> its own fine-resolution H3 cell (the element
// envelope-centroid cell), the element-DISTINCT vertex the `Query/cypher#GRAPH_QUERY` `pgrouting` `network_edge`
// source/target carries and `NodeAt` resolves back to a `NodeId`. Distinct in grain from the per-MODEL
// `ElementIdentity.Cell` locator — this is the per-element routing vertex, that the model-envelope spatial locator.
// `Z` is the element centroid elevation (meters), so a storey-banded spatial join is one indexed range predicate
// beside the cell equality — never a client-side elevation scan.
public sealed record NodeCell(ModelId Model, NodeId Node, UInt128 Tenant, H3Cell Cell, double Z);

public sealed record IdentityWriter(string Sql, Func<ElementIdentity, object?[]> Binds);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NodeCellBulkPolicy {
    public static readonly NodeCellBulkPolicy Server = new("server", LinqToDB.Data.BulkCopyType.ProviderSpecific, 16_384);
    public static readonly NodeCellBulkPolicy Embedded = new("embedded", LinqToDB.Data.BulkCopyType.MultipleRows, 1_024);
    public LinqToDB.Data.BulkCopyOptions Options { get; }
    private NodeCellBulkPolicy(string key, LinqToDB.Data.BulkCopyType type, int maxBatchSize) : this(key) =>
        Options = new LinqToDB.Data.BulkCopyOptions { BulkCopyType = type, KeepIdentity = true, MaxBatchSize = maxBatchSize };
}

// --- [SERVICES] ------------------------------------------------------------------------
// The ONE options composition: generated Thinktecture converters (bounded Configuration.Default key width) +
// snake-case naming + the provider plugin stack — NTS geometry, NodaTime Instant, and pgvector all mount on
// the one UseNpgsql options row. Hand `HasConversion` on a Thinktecture type and hand `HasColumnName` are the
// deleted forms; `ThinktectureValueConverterFactory.Create<T,TKey>` covers the residual EF-cannot-resolve
// case. The compiled model (`Optimize`) mounts back through this same rail byte-identically.
public static class ConverterRail {
    public static DbContextOptionsBuilder Compose(DbContextOptionsBuilder options, StoreBinding binding) =>
        binding.Switch(
            postgres: p => options.UseNpgsql(p.Source, static npgsql => npgsql.UseNetTopologySuite().UseNodaTime().UseVector()),
            embedded: e => options.UseSqlite(e.Connection))
        .UseSnakeCaseNamingConvention()
        .UseThinktectureValueConverters(Configuration.Default);
}

public sealed class IdentityContext(DbContextOptions<IdentityContext> options) : DbContext(options) {
    public DbSet<ElementIdentity> Identities => Set<ElementIdentity>();
    public DbSet<NodeCell> Cells => Set<NodeCell>();

    // Provider divergence is model DATA keyed on the one probe — never a second context or a second mapping.
    protected override void OnModelCreating(ModelBuilder model) {
        model.ApplyConfiguration(new IdentityShape(Database.IsSqlite()));
        model.ApplyConfiguration(new NodeCellShape());
    }
}

// ONLY the residual set the conventions cannot derive: LanguageExt carrier conversions, the jsonb ACL, the
// geometry column, and the indexes. Every scalar/[ValueObject]/[SmartEnum] column (Model, Tenant, Cell,
// Classification, At, NodeCell.Node) converts and names through the rail — zero HasColumnName, zero hand
// converter on a Thinktecture type.
public sealed class IdentityShape(bool embedded) : IEntityTypeConfiguration<ElementIdentity> {
    public void Configure(EntityTypeBuilder<ElementIdentity> identity) {
        ArgumentNullException.ThrowIfNull(identity);
        // The ONE declared physical relation both sides name: EF owns its DDL and every read lane targets it,
        // and `IdentityStore.Stamp` queues its model-derived upsert against THIS table — explicit, so the
        // write rail and the query surface can never resolve to two convention-divergent relations.
        identity.ToTable("element_identity");
        identity.HasKey(static e => e.Model);
        identity.Property(static e => e.Roots)
            .HasConversion(
                new ValueConverter<Seq<NodeId>, string[]>(
                    static r => r.Map(static n => n.Value).ToArray(),
                    static a => toSeq(a).Map(NodeId.Create)),
                new ValueComparer<Seq<NodeId>>(static (x, y) => x == y, static v => v.GetHashCode(), static v => v));
        identity.Property(static e => e.GlobalIds).HasColumnType(embedded ? "text" : "jsonb")
            .HasConversion(
                new ValueConverter<HashMap<NodeId, string>, string>(
                    static m => System.Text.Json.JsonSerializer.Serialize(m.ToDictionary(static p => p.Key.Value, static p => p.Value), ElementJson.Options),
                    static s => toHashMap((System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(s, ElementJson.Options) ?? []).Select(static kv => (NodeId.Create(kv.Key), kv.Value)))),
                new ValueComparer<HashMap<NodeId, string>>(static (x, y) => x == y, static v => v.GetHashCode(), static v => v));
        identity.Property(static e => e.Tenant).HasColumnType("text")
            .HasConversion(static tenant => tenant.ToString("x32"), static tenant => UInt128.Parse(tenant, System.Globalization.NumberStyles.HexNumber));
        identity.Property(static e => e.Acl).HasColumnType(embedded ? "text" : "jsonb")
            .HasConversion(
                static acl => System.Text.Json.JsonSerializer.Serialize(acl, ElementJson.Options),
                static json => System.Text.Json.JsonSerializer.Deserialize<ObjectAcl>(json, ElementJson.Options) ?? throw new System.Text.Json.JsonException("<object-acl:null>"));
        if (embedded) {
            // Embedded floor: bounds degrade to WKB bytes beside the H3 bigint; no vector column on the floor.
            identity.Property(static e => e.Bounds)
                .HasConversion(new ValueConverter<Polygon?, byte[]?>(
                    static g => g == null ? null : new WKBWriter().Write(g),
                    static b => b == null ? null : (Polygon)new WKBReader().Read(b)));
            identity.Ignore(static e => e.Embedding);
        } else {
            identity.Property(static e => e.Bounds).HasColumnType("geometry(Polygon,4326)");
            identity.HasIndex(static e => e.Bounds).HasMethod("gist");
            // Some -> the vector, None -> a NULL column; the column sizes vector(1536) by metadata.
            identity.Property(static e => e.Embedding).HasColumnType("vector(1536)")
                .HasConversion(
                    new ValueConverter<Option<Pgvector.Vector>, Pgvector.Vector?>(
                        static o => o.MatchUnsafe(Some: static v => v, None: static () => null),
                        static v => Optional(v)),
                    new ValueComparer<Option<Pgvector.Vector>>(
                        static (x, y) => x == y, static v => v.GetHashCode(), static v => v));
        }
        identity.HasIndex(static e => e.Cell);
        identity.HasIndex(static e => e.Tenant);
    }
}

public sealed class NodeCellShape : IEntityTypeConfiguration<NodeCell> {
    public void Configure(EntityTypeBuilder<NodeCell> node) {
        ArgumentNullException.ThrowIfNull(node);
        node.ToTable("node_cell");
        node.HasKey(static n => new { n.Model, n.Node });
        node.Property(static n => n.Tenant).HasColumnType("text")
            .HasConversion(static tenant => tenant.ToString("x32"), static tenant => UInt128.Parse(tenant, System.Globalization.NumberStyles.HexNumber));
        // Composite (Cell, Z): the storey-banded join is cell-equality + z-range, served index-only.
        node.HasIndex(static n => new { n.Cell, n.Z });
        node.HasIndex(static n => n.Tenant);
    }
}

public static class IdentityStore {
    static readonly GeometryFactory Wgs84 = new(new PrecisionModel(), 4326);

    // A Marten document lives in an `mt_doc_*` id+jsonb table — an EF-shaped relational row can never BE a
    // Marten document, so the co-transactional stamp is ONE model-derived upsert QUEUED on the session
    // (`QueueSqlCommand` rides the same transaction `SaveChangesAsync` commits with the appended events).
    // `Bind` derives the statement ONCE at boot from the compiled EF model — table, column names, column-type
    // casts, and the `model` conflict key all read off `IEntityType`, and each parameter runs the property's
    // own EF value converter — so EF names the ONE physical relation for DDL and every read lane while holding
    // ZERO write authority: the Marten session is the only writer, and hand-spelled column SQL never exists.
    public static IdentityWriter Bind(IModel model) {
        ArgumentNullException.ThrowIfNull(model);
        IEntityType entity = model.FindEntityType(typeof(ElementIdentity)) ?? throw new InvalidOperationException("<identity-model:absent>");
        StoreObjectIdentifier table = StoreObjectIdentifier.Table(entity.GetTableName() ?? throw new InvalidOperationException("<identity-table:absent>"), entity.GetSchema());
        Seq<(string Name, string Cast, Func<ElementIdentity, object?> Read, ValueConverter? Convert)> columns = toSeq(entity.GetProperties())
            .Map(property => (
                Name: property.GetColumnName(table) ?? throw new InvalidOperationException($"<identity-column:{property.Name}>") ,
                Cast: property.GetRelationalTypeMapping().StoreType,
                Read: MemberReader(property),
                Convert: property.GetValueConverter()));
        Seq<string> keys = toSeq(entity.FindPrimaryKey()?.Properties ?? throw new InvalidOperationException("<identity-key:absent>"))
            .Map(property => Quote(property.GetColumnName(table) ?? throw new InvalidOperationException($"<identity-key-column:{property.Name}>")));
        string relation = entity.GetSchema() is { } schema ? $"{Quote(schema)}.{Quote(table.Name)}" : Quote(table.Name);
        string placeholders = string.Join(", ", columns.Map(static column => $"CAST(? AS {column.Cast})"));
        string assignments = string.Join(", ", columns.Filter(column => !keys.Contains(Quote(column.Name))).Map(static column => $"{Quote(column.Name)} = excluded.{Quote(column.Name)}"));
        string sql = $"INSERT INTO {relation} ({string.Join(", ", columns.Map(static column => Quote(column.Name)))}) VALUES ({placeholders}) ON CONFLICT ({string.Join(", ", keys)}) DO UPDATE SET {assignments}";
        return new IdentityWriter(sql, identity => columns.Map(column => column.Convert is { } converter ? converter.ConvertToProvider(column.Read(identity)) : column.Read(identity)).ToArray());
    }

    static string Quote(string identifier) => $"\"{identifier.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";

    // Member access off the model's own `IProperty.PropertyInfo` — the same metadata the statement derived
    // from, so column order and value order can never skew; no second naming convention exists.
    static Func<ElementIdentity, object?> MemberReader(IProperty property) =>
        property.PropertyInfo is { } member ? member.GetValue : static _ => null;

    public static IDocumentSession Stamp(IDocumentSession session, ElementIdentity identity, IdentityWriter writer) {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(writer);
        session.QueueSqlCommand(writer.Sql, writer.Binds(identity));
        return session;
    }

    // Mint the cell through the managed pocketken.H3 entry that mirrors `h3_latlng_to_cell` — the NTS `Point` overload
    // (SRID 4326, the package owning the degree->radian conversion), NOT a hand-built radian LatLng. The cell is durable
    // as the `long` reinterpretation; an out-of-range centroid decodes to the `H3Index.Invalid` zero sentinel, which
    // never persists (it rails CellUnresolvable instead of a stored 0 cell).
    public static Fin<H3Cell> Cell(Envelope bounds, int resolution) {
        ArgumentNullException.ThrowIfNull(bounds);
        H3.H3Index index = H3.H3Index.FromPoint(new Point(bounds.Centre.X, bounds.Centre.Y) { SRID = 4326 }, resolution);
        return index.IsValidCell
            ? Fin<H3Cell>.Succ(H3Cell.Of(index))
            : Fin<H3Cell>.Fail(new IdentityFault.CellUnresolvable($"<centroid:{bounds.Centre.X},{bounds.Centre.Y}@{resolution}>"));
    }

    // The PER-ELEMENT FINE cell mint (same entry, finer resolution) and the exact-bounds polygon mint — the
    // `Bounds` producer is the seam-stable representation-bounds Envelope `Rasm.Element` projects.
    public static Polygon BoundsOf(Envelope bounds) => (Polygon)Wgs84.ToGeometry(bounds);

    // The H3 neighborhood is the filled grid disk: `GridDiskDistances` yields each `RingCell`, and the durable
    // `long` reinterpretation is the `IN`/`= ANY` membership the `h3_cell` index answers — never a per-row
    // distance scan (the real disk-with-distances member is `GridDiskDistances`, not a phantom `Api.GridDisk`).
    public static IQueryable<ElementIdentity> Nearby(IdentityContext db, H3Cell cell, int ring) {
        ArgumentNullException.ThrowIfNull(db);
        H3Cell[] disk = toSeq(cell.Live.GridDiskDistancesSafe(ring)).Map(static ringCell => H3Cell.Of(ringCell.Index)).ToArray();
        return db.Identities.Where(e => disk.Contains(e.Cell));
    }

    // The exact-predicate lane: `IsWithinDistance` translates to ST_DWithin and `DistanceKnn` to the `<->`
    // KNN order, both riding the GiST bounds index SERVER-side — the geometry plane complementing the H3
    // cell plane (cells for bucket-equality joins, geometry for exact XY predicates), never a client scan.
    // `z` is the vertical-overlap clause of the THIRD plane: `Some((min, max))` narrows to elements whose
    // z-span intersects the band, so a storey-scoped query stops matching every stacked element sharing a
    // footprint — one polymorphic entry, never a `WithinZ` sibling.
    public static IQueryable<ElementIdentity> Within(IdentityContext db, Point probe, double range, Option<(double Min, double Max)> z) {
        ArgumentNullException.ThrowIfNull(db);
        IQueryable<ElementIdentity> plane = db.Identities
            .Where(e => e.Bounds != null && EF.Functions.IsWithinDistance(e.Bounds!, probe, range, false));
        return z.Match(
                Some: band => plane.Where(e => e.ZMin != null && e.ZMax != null && e.ZMin <= band.Max && e.ZMax >= band.Min),
                None: () => plane)
            .OrderBy(e => EF.Functions.DistanceKnn(e.Bounds!, probe));
    }

    // The cell probe is polymorphic over the band: `None` is the flat cell-equality join, `Some` the
    // storey-banded join the composite `(Cell, Z)` index serves end to end.
    public static IQueryable<NodeCell> NodeAt(IdentityContext db, H3Cell cell, Option<(double Min, double Max)> z) {
        ArgumentNullException.ThrowIfNull(db);
        IQueryable<NodeCell> cells = db.Cells.Where(n => n.Cell == cell);
        return z.Match(Some: band => cells.Where(n => n.Z >= band.Min && n.Z <= band.Max), None: () => cells);
    }

    // The re-ingest mass-stamp lane: the linq2db bridge lifts the ONE EF context into a COPY bulk insert,
    // so thousands of per-element NodeCell rows land in one round trip — never a per-row SaveChanges loop.
    public static IO<long> Bulk(IdentityContext db, Seq<NodeCell> cells, NodeCellBulkPolicy policy, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(policy);
        return IO.liftAsync(async () => (await db.BulkCopyAsync(policy.Options, cells, cancellationToken).ConfigureAwait(false)).RowsCopied);
    }
}
```

| [INDEX] | [POLICY]         | [VALUE]                                                 | [BINDING]                                                  |
| :-----: | :--------------- | :------------------------------------------------------ | :--------------------------------------------------------- |
|  [01]   | one txn owner    | model-derived upsert queued on the session              | `IdentityStore.Stamp` + `SaveChangesAsync`; no two-ORM gap |
|  [02]   | converter rail   | `UseThinktectureValueConverters(Configuration.Default)` | zero hand converters; snake-case names derived             |
|  [03]   | spatial planes   | H3 `bigint` cells + PostGIS `Bounds` + z-span           | bucket joins; exact XY predicates; storey banding          |
|  [04]   | embedded floor   | `IsSqlite()`-keyed model rows                           | WKB bounds, text GlobalIds, no vector column; one context  |
|  [05]   | rooted key       | neutral kernel-minted durable `NodeId`                  | `ExternalId` projection; re-ingest correlates on it        |
|  [06]   | tenant partition | `Tenant` RLS column                                     | coarse scope; `ObjectAcl` is the fine grant                |

## [03]-[IDENTITY_POLICY]

- Owner: `IdentityPolicy` the `[SmartEnum<string>]` five-row key axis carrying generator, big-endian transcription, ordering, collision class, and CLR type, dispatching `Mint` per row through one generated `Switch`; `StoreKey` the `[Union]` closed key carrier (`Surrogate`/`Content`/`Natural`); `Collision` the collision-posture vocabulary.
- Cases: `uuid-v7` (default, B-tree insert-local, `Guid.CreateVersion7`), `uuid-v7-backfill` (historical-timestamp mint for deterministic backfill), `content-hash` (immutable-payload content addressing through the kernel `ContentHash.Of` — the `ContentAddress` row), `natural-key` (caller-owned identifier passthrough), `namespace-key` (RFC-4122 v5 over a namespace and a name for stable derived ids); `Collision` rows are `unmintable`, `content-idempotent`, `foreign-authority`, `derived-deterministic`.
- Entry: `Mint(ReadOnlyMemory<byte>, Instant)` dispatches through the generated `Switch`; `Decode(ReadOnlySpan<byte>)` validates width and strict UTF-8 before returning `Fin<StoreKey>`; `StoreKey.Spelled` is the ordering-preserving big-endian transcription; `StoreKey.ObservedAt` projects a v7 key's embedded creation time.
- Packages: Thinktecture.Runtime.Extensions, Rasm (`ContentHash.Of`), System.Security.Cryptography (`IncrementalHash`/`SHA1`), System.Buffers.Binary, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: one `IdentityPolicy` row carries text, CLR type, ordering, collision, and client-generated precedence; a new posture is one `Collision` row; zero new surface.
- Boundary: every persisted key strategy traces to one row here — `uuid-ossp` is the deleted extension route; `StoreKey` is the one closed key carrier so a column type is a case projection, never a parallel key type per provider; ordering survives transcription only when the spelling preserves it — every case transcribes big-endian (`Guid.ToByteArray(bigEndian: true)`, `WriteUInt128BigEndian`, UTF-8) because the platform-default little-endian export fractures a binary-keyed index; `StoreKey.ObservedAt` makes a v7 key a free coarse creation-time axis so a composite `(low-cardinality discriminant, v7 key)` index stays append-local; identity-row change is never `AlterColumn` — it is an expand-wave second key backfilled by the `uuid-v7-backfill` row, a derivation flip, and a contract-wave drop, the only identity migration preserving foreign references and AS-OF cursor validity at once; content identity is the non-cryptographic kernel `ContentHash.Of` (no security claim, no direct `XxHash128` call site) and `namespace-key` mints the canonical RFC-4122 v5 namespace UUID (`SHA1` the spec construction, not a security claim).

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Rasm.Domain;                                 // ContentHash — the ONE kernel digest entry

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
        content:   static c => SpelledContent(c.Value),
        natural:   static n => System.Text.Encoding.UTF8.GetBytes(n.Value));

    static byte[] SpelledContent(UInt128 value) { byte[] bytes = new byte[16]; System.Buffers.Binary.BinaryPrimitives.WriteUInt128BigEndian(bytes, value); return bytes; }

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

    public Fin<StoreKey> Decode(ReadOnlySpan<byte> spelled) {
        byte[] bytes = spelled.ToArray();
        if (this == NaturalKey) return DecodeNatural(bytes);
        return bytes.Length == 16
            ? Fin<StoreKey>.Succ(Switch<byte[], StoreKey>(
                state: bytes,
                uuidV7Key: static value => new StoreKey.Surrogate(new Guid(value, bigEndian: true)),
                uuidV7Backfill: static value => new StoreKey.Surrogate(new Guid(value, bigEndian: true)),
                contentHashKey: static value => new StoreKey.Content(System.Buffers.Binary.BinaryPrimitives.ReadUInt128BigEndian(value)),
                naturalKey: static value => new StoreKey.Natural(StrictUtf8.GetString(value)),
                namespaceKey: static value => new StoreKey.Surrogate(new Guid(value, bigEndian: true))))
            : Fin<StoreKey>.Fail(new IdentityFault.KeyMalformed($"<key-width:{Key}:{bytes.Length}>"));
    }

    static readonly System.Text.UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    static Fin<StoreKey> DecodeNatural(byte[] bytes) {
        try { return Fin<StoreKey>.Succ(new StoreKey.Natural(StrictUtf8.GetString(bytes))); }
        catch (System.Text.DecoderFallbackException failure) { return Fin<StoreKey>.Fail(new IdentityFault.KeyMalformed(failure.Message)); }
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

## [04]-[KMS_CUSTODY]

- Owner: `KmsProvider` is the Persistence KMS axis; `KeyState` is its lifecycle vocabulary; `SigningAlgorithm` carries hash and provider spelling; `OpDigest` is an immutable canonical-hex value; `SigningKeyring` carries `Sign`/`Verify`; `EnvelopeAad`, `WrappedKey`, `WrapForm`, and `EnvelopeKeyring` own DEK custody; `CustodyVerdict` is the closed crypto verdict; `Custody` is the one authorship and envelope fold.
- Cases: `KmsProvider` is `None | Aws | Azure | Gcp`. `SigningAlgorithm` covers ES/PS/RS at each admitted digest width plus AWS `Ed25519`/`Ed25519Ph`, with provider support stored on each row. `KeyState` is `Enabled | Disabled | Destroyed | Scheduled | Pending`; `WrapForm` is `Bound | Remote`. `CustodyVerdict` covers digest width, algorithm/provider compatibility, authenticity, envelope, and key lifecycle.
- Entry: `public static IO<CustodyVerdict> Attest(StoreActor actor, OpDigest digest, KmsProvider provider, string signingKeyId, SigningKeyring keyring, ProjectionContext frame)` signs an `OpDigest` after gating its width (the `!provider.Signs` local tier shorts to `Unsigned` so a store with no KMS still records the delta→actor binding); `Verify(SignedAuthorship, OpDigest, SigningKeyring)` checks the digest binding and signature; `Wrap(EnvelopeKeyring, EnvelopeAad, WrapForm)` probes the key lifecycle then mints per the form (`Bound` → plaintext + `WrappedKey`; `Remote` → wrapped-only, `Wrapped.Dek` empty); `Unwrap(EnvelopeKeyring, WrappedKey, EnvelopeAad)` recovers the plaintext DEK and the caller zeroizes it after the local bind; `Rewrap(EnvelopeKeyring, WrappedKey, EnvelopeAad)` advances the wrapping-key version without the plaintext crossing the wire — one envelope fold beside the one signing fold.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm (`ContentHash.Of` — the AAD tenant digest), AWSSDK.KeyManagementService (signing `SigningAlgorithmSpec`/`SignAsync`/`VerifyAsync`/`MessageType.DIGEST`; envelope `GenerateDataKeyAsync`/`GenerateDataKeyWithoutPlaintextAsync`/`DecryptAsync`/`ReEncryptAsync`; probe `DescribeKeyAsync`), Azure.Security.KeyVault.Keys (signing `SignatureAlgorithm`/`Sign`/`Verify`; native envelope `CryptographyClient.WrapKey`/`UnwrapKey` over `KeyWrapAlgorithm.RsaOaep256`; `KeyClient` key-state), Google.Cloud.Kms.V1 (envelope `EncryptAsync`/`DecryptAsync` + bidirectional CRC32C; `GenerateRandomBytesAsync` HSM-backed off-board DEK material — the Gcp `Mint` arm's DEK source; rotation `UpdateCryptoKeyPrimaryVersionAsync`; probe `GetCryptoKeyVersionAsync` `CryptoKeyVersionState`), System.Security.Cryptography (`CryptographicOperations.HashData`/`ZeroMemory`), System.Collections.Frozen.
- Growth: one `KmsProvider` row per new cloud KMS (a non-signing provider sets `Signs: false` and routes through the SAME `Unsigned` path; a native-wrap provider sets `NativeWrap: true` and binds `Mint`/`Rewrap` against its wrap verb rather than encrypt-as-wrap); one `SigningAlgorithm` row per JWS family; one `KeyState` row per lifecycle posture; one `WrapForm` row per mint modality; one `CustodyVerdict` case per verdict; zero new surface — a separate `Store/encryption` page, a second provider axis, or a Persistence-side long-lived DEK cache is the deleted form.
- Boundary: signed authorship is the actor-to-blame seam — a cloud-KMS op carries a `SignedAuthorship` over a `SigningAlgorithm`-width cryptographic `OpDigest` so a blame attribution (`Version/timetravel`, `Version/provenance#ATTESTED_LEDGER` — the consumer that chains these attestations) names a verified actor, a 16-byte non-cryptographic content hash standing in for the signed digest being the deleted form; the `SigningKeyring` is the KMS SIGNING surface (`Sign`/`Verify` over an asymmetric key, the disjoint operation from the DEK envelope), resolving the key through the AppHost `SecretLease`-class handle, never a bare passphrase, and the provider-specific algorithm type (`SigningAlgorithmSpec`/`SignatureAlgorithm`) lives only at the keyring delegate edge; the `EnvelopeKeyring` is the DEK-ENVELOPE surface this `#KEY_ENVELOPE` owner holds beside the signing keyring on the ONE `KmsProvider` axis — the `Mint`/`MintSealed`/`Unwrap`/`Rewrap`/`Probe` family wrapping a data-encryption key against the symmetric CMK where each arm's mechanism is a policy value on the `KmsProvider` row (the `NativeWrap` provider routes through Azure's native `WrapKey`/`UnwrapKey`; the encrypt-as-wrap providers through AWS `GenerateDataKey`/`Decrypt`/`ReEncrypt` and GCP `Encrypt`/`Decrypt` + `UpdateCryptoKeyPrimaryVersion`, the GCP `Mint` sourcing its DEK bytes from `GenerateRandomBytesAsync` so key material is HSM-born, never process-entropy-born), the `Probe` arm resolving `KeyState` so a wrap against a `Disabled`/`Scheduled` key rejects `KeyUnusable` at admission; the `EnvelopeAad` (store partition + the tenant id digested through the kernel `ContentHash.Of` so the AAD is a fixed-width opaque value, never a raw tenant uuid on the wire) rides the provider `EncryptionContext`/`AdditionalAuthenticatedData` on the AWS/GCP arms and is compared application-side on the Azure native-wrap arm (which carries no per-call AAD), so a DEK wrapped for one `(partition, tenant)` cannot be unwrapped under another; the recovered plaintext DEK zeroizes through `CryptographicOperations.ZeroMemory` immediately after the local bind so a Persistence-side long-lived key is the deleted form; the `Store/blobstore#BLOB_GC` `ObjectEncryption` is the downstream SSE-stance consumer carrying only the server-side-SSE key-id STRING this envelope mints out-of-band, never a second envelope owner; the authz decision this fold NEVER makes is `Element/authority#AUTHORITY` `Admit` — custody proves WHO DID and KEEPS KEYS, authority decides WHO MAY, and the two verdicts stay two unions.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Rasm.Domain;                                 // ContentHash — the AAD tenant digest entry

// --- [TYPES] ---------------------------------------------------------------------------
// The Persistence-side KMS provider axis BOTH the signing surface (`SigningKeyring`/`SignedAuthorship`) AND the
// DEK envelope surface (`EnvelopeKeyring`, the `#KEY_ENVELOPE` owner) resolve against — the concrete SDK binding
// stays Persistence-side per the AppHost `Runtime/secrets#KMS_UNWRAP_PORT` seam, AppHost surfacing only the
// `SecretLease`-class handle. `None` is the local/Personal tier: `Custody.Attest`/`Verify` short to
// `CustodyVerdict.Unsigned` so a store with no KMS still records the delta->actor binding, never a fabricated
// signature. `Signs` gates the signing arm; `NativeWrap` gates the envelope arm — Azure wraps through the native
// `CryptographyClient.WrapKey`/`UnwrapKey` verb while Aws/Gcp encrypt-as-wrap, so the keyring `Mint`/`Rewrap`
// arm reads `NativeWrap` rather than hardcoding one provider's spelling.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KmsProvider {
    public static readonly KmsProvider None = new("none", signs: false, nativeWrap: false);
    public static readonly KmsProvider Aws = new("aws", signs: true, nativeWrap: false);
    public static readonly KmsProvider Azure = new("azure", signs: true, nativeWrap: true);
    public static readonly KmsProvider Gcp = new("gcp", signs: true, nativeWrap: false);
    public bool Signs { get; }
    public bool NativeWrap { get; }
    private KmsProvider(string key, bool signs, bool nativeWrap) : this(key) => (Signs, NativeWrap) = (signs, nativeWrap);
}

// The cloud-key lifecycle the `EnvelopeKeyring.Probe` arm resolves (AWS `DescribeKey` `KeyState`, Azure
// `KeyProperties`, GCP `CryptoKeyVersionState`): only `Enabled` admits a wrap, so a `Mint`/`Rewrap` against a
// non-`Enabled` key rejects `CustodyVerdict.KeyUnusable` at admission rather than deep in the provider call.
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

// The mint modality as a policy row, never a boolean: `Bound` returns the plaintext DEK for the local cipher
// bind (zeroized after); `Remote` is the wrapped-only mint (AWS `GenerateDataKeyWithoutPlaintext`) for a
// minting node that never encrypts locally — the plaintext first materializes at the read-path `Unwrap`.
[SmartEnum]
public sealed partial class WrapForm {
    public static readonly WrapForm Bound = new();
    public static readonly WrapForm Remote = new();
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SigningKeyring(SigningAlgorithm Algorithm, Func<ReadOnlyMemory<byte>, IO<ReadOnlyMemory<byte>>> Sign, Func<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>, IO<bool>> Verify);
public sealed record SignedAuthorship(StoreActor Actor, KmsProvider Provider, string SigningKeyId, SigningAlgorithm Algorithm, OpDigest Digest, ReadOnlyMemory<byte> Signature, Instant At, Guid Correlation);

// The additional-authenticated-data binding every wrap/unwrap carries: the store partition plus (under RLS) the
// tenant id digested through the kernel `ContentHash.Of` so the AAD is a fixed-width opaque value, never a raw
// tenant uuid on the wire. It rides the provider `EncryptionContext` (AWS) / `AdditionalAuthenticatedData` (GCP)
// exact-match and is compared application-side on the Azure native-wrap arm (which carries no per-call AAD), so
// a DEK wrapped for one (partition, tenant) cannot be unwrapped under another.
[ComplexValueObject]
public sealed partial class EnvelopeAad {
    public string Partition { get; }
    public UInt128 TenantDigest { get; }
    public FrozenDictionary<string, string> Context => new Dictionary<string, string> { ["partition"] = Partition, ["tenant"] = TenantDigest.ToString("x32") }.ToFrozenDictionary();
    public static EnvelopeAad Of(string partition, UInt128 tenant) => new(partition, ContentHash.Of(System.Text.Encoding.UTF8.GetBytes($"{partition}:{tenant:x32}")));
}

// The persisted envelope carrier: the wrapped DEK bytes plus the wrapping key id and the exact key version the
// wrap used (the AWS `KeyMaterialId`, the Azure key version, the GCP `CryptoKeyVersionName`), so a `Rewrap`
// advances the version and an `Unwrap` resolves the embedded version without a second lookup. The plaintext DEK
// is NEVER a field.
public readonly record struct WrappedKey(ReadOnlyMemory<byte> Ciphertext, string WrappingKeyId, string Version);

// The provider-neutral DEK-envelope family (the `#KEY_ENVELOPE` surface, beside `SigningKeyring` on the one
// `KmsProvider` axis): `Mint` wraps a fresh DEK returning the plaintext for local AES use plus the `WrappedKey`
// to persist; `MintSealed` is the wrapped-only arm (`GenerateDataKeyWithoutPlaintext`; GCP sources DEK bytes
// from `GenerateRandomBytesAsync`); `Unwrap` recovers the plaintext; `Rewrap` advances the wrapping-key version
// with the plaintext never crossing the wire; `Probe` resolves the `KeyState`. Each delegate closes over the
// provider arm's concrete client so `Custody` composes one shape across all providers. The AAD binds every arm.
public sealed record EnvelopeKeyring(
    KmsProvider Provider,
    Func<EnvelopeAad, IO<(ReadOnlyMemory<byte> Dek, WrappedKey Wrapped)>> Mint,
    Func<EnvelopeAad, IO<WrappedKey>> MintSealed,
    Func<WrappedKey, EnvelopeAad, IO<ReadOnlyMemory<byte>>> Unwrap,
    Func<WrappedKey, EnvelopeAad, IO<WrappedKey>> Rewrap,
    Func<IO<KeyState>> Probe);

// The custody half of the fissioned decision union (the authz half is `Element/authority` `AuthDecision`):
// `Wrapped` carries the freshly-minted plaintext DEK (empty on the Remote/rewrap paths) plus the `WrappedKey`
// to persist; `Unwrapped` the recovered DEK; `KeyUnusable` the `Probe`-rejected non-`Enabled` `KeyState`.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CustodyVerdict {
    private CustodyVerdict() { }
    public sealed record Attested(SignedAuthorship Authorship) : CustodyVerdict;
    public sealed record Authentic(StoreActor Actor, string SigningKeyId) : CustodyVerdict;
    public sealed record Unsigned(StoreActor Actor, OpDigest Digest, Instant At, Guid Correlation) : CustodyVerdict;
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
        !provider.Signs ? IO.pure<CustodyVerdict>(new CustodyVerdict.Unsigned(actor, digest, frame.Now(), frame.Correlation))
        : !keyring.Algorithm.Admits(provider) ? IO.pure<CustodyVerdict>(new CustodyVerdict.UnsupportedAlgorithm(provider, keyring.Algorithm))
        : digest.Fits(keyring.Algorithm) ? keyring.Sign(digest.Bytes).Map(signature => (CustodyVerdict)new CustodyVerdict.Attested(new SignedAuthorship(actor, provider, signingKeyId, keyring.Algorithm, digest, signature, frame.Now(), frame.Correlation)))
        : IO.pure<CustodyVerdict>(new CustodyVerdict.DigestWidth(keyring.Algorithm.DigestWidth, digest.ByteLength));

    public static IO<CustodyVerdict> Verify(SignedAuthorship authorship, OpDigest digest, SigningKeyring keyring) =>
        !authorship.Provider.Signs ? IO.pure<CustodyVerdict>(new CustodyVerdict.Unsigned(authorship.Actor, authorship.Digest, authorship.At, authorship.Correlation))
        : authorship.Algorithm != keyring.Algorithm ? IO.pure<CustodyVerdict>(new CustodyVerdict.AlgorithmMismatch(keyring.Algorithm, authorship.Algorithm))
        : !keyring.Algorithm.Admits(authorship.Provider) ? IO.pure<CustodyVerdict>(new CustodyVerdict.UnsupportedAlgorithm(authorship.Provider, keyring.Algorithm))
        : authorship.Digest != digest ? IO.pure<CustodyVerdict>(new CustodyVerdict.Unauthored(digest, authorship.Digest))
        : keyring.Verify(digest.Bytes, authorship.Signature).Map(valid => valid ? (CustodyVerdict)new CustodyVerdict.Authentic(authorship.Actor, authorship.SigningKeyId) : new CustodyVerdict.Forged(authorship.Actor, authorship.SigningKeyId));

    // The DEK envelope fold beside the signing fold: `Wrap` PROBES the key lifecycle first so a wrap against a
    // non-`Enabled` key rejects `KeyUnusable` at admission (never deep in the provider call), then mints per the
    // `WrapForm` row — `Bound` returns the plaintext for the local cipher bind, `Remote` the wrapped-only
    // envelope (`Wrapped.Dek` empty; the plaintext first materializes at the read-path `Unwrap`).
    public static IO<CustodyVerdict> Wrap(EnvelopeKeyring keyring, EnvelopeAad aad, WrapForm form) =>
        from state in keyring.Probe()
        from verdict in state.Usable
            ? form.Switch(
                bound: () => keyring.Mint(aad).Map(static r => (CustodyVerdict)new CustodyVerdict.Wrapped(r.Dek, r.Wrapped)),
                remote: () => keyring.MintSealed(aad).Map(static k => (CustodyVerdict)new CustodyVerdict.Wrapped(ReadOnlyMemory<byte>.Empty, k)))
            : IO.pure<CustodyVerdict>(new CustodyVerdict.KeyUnusable("<mint>", state))
        select verdict;

    // `Unwrap` recovers the plaintext DEK bound to the persisted `WrappedKey` under the SAME `EnvelopeAad` (the
    // provider exact-match on AWS/GCP, the application-side compare on Azure) — the caller zeroizes the returned
    // DEK through `CryptographicOperations.ZeroMemory` after the local cipher bind.
    public static IO<CustodyVerdict> Unwrap(EnvelopeKeyring keyring, WrappedKey wrapped, EnvelopeAad aad) =>
        keyring.Unwrap(wrapped, aad).Map(static dek => (CustodyVerdict)new CustodyVerdict.Unwrapped(dek));

    // `Rewrap` advances the wrapping-key version (AWS `ReEncrypt`, GCP `UpdateCryptoKeyPrimaryVersion` primary
    // repoint, Azure re-`WrapKey` against the new version) with the plaintext DEK never crossing the wire —
    // gated on the same `Probe`, returning `Wrapped` carrying the new `WrappedKey` (Dek empty on this path).
    public static IO<CustodyVerdict> Rewrap(EnvelopeKeyring keyring, WrappedKey wrapped, EnvelopeAad aad) =>
        from state in keyring.Probe()
        from verdict in state.Usable
            ? keyring.Rewrap(wrapped, aad).Map(static next => (CustodyVerdict)new CustodyVerdict.Wrapped(ReadOnlyMemory<byte>.Empty, next))
            : IO.pure<CustodyVerdict>(new CustodyVerdict.KeyUnusable(wrapped.WrappingKeyId, state))
        select verdict;
}
```

## [05]-[SCHEMA_VERDICT]

- Owner: `SchemaVerdict` the `[Union]` boot verdict; `Placement` the `[SmartEnum<string>]` write-authority axis (the route-prescribed shape, declared here as the Persistence-Element owner); `IdentityFault` the `[Union]` identity-tier fault band deriving `Code => FaultBand.Identity + n` (the `Element/graph#FAULT_TABLES` registry row — `Element/authority` composes this band, no band of its own); `SchemaGate` the static surface folding the Marten startup posture plus the EF migration identifier sets into one typed verdict so boot is a total fold, never a best-effort open; `IdentityDdl` the migration owner — the EF.Design emission lanes plus the raw rows (RLS, extension installs) the generated model cannot express.
- Cases: `SchemaVerdict` is `Serving` (model matches schema), `ServingBehind(Unknown)` (applied identifiers the compiled model does not know — schema newer than binary, admitted only under a declared expand-only invariant), `Provisioned(Applied)` (fresh store migrated), `Advanced(Applied)` (pending migrations applied), `AwaitBundle(Pending, Fresh)` (pending migrations a fleet member must not self-apply), `Drifted` (a model edit with neither migration nor regeneration); `IdentityFault` is `SchemaAhead(Unknown)` (EF identifiers the binary lacks), `ApplyFailed(Detail)` (an EF `Migrate` or a Marten apply throw), `MartenMismatch(Detail)` (the host-startup Marten assertion throw lifted onto the band), `CellUnresolvable(Detail)` (an H3 centroid that yields the invalid sentinel).
- Entry: `public static Fin<SchemaVerdict> Admit(DbContext store, Placement placement)` folds the assembly and applied EF migration sets into the EF half of the verdict; `public static IO<SchemaVerdict> AdmitMarten(IMartenStorage storage, Placement placement)` is the single-writer Marten apply leg over `ApplyAllConfiguredChangesToDatabaseAsync`, the fleet member's Marten posture being the host-registered `AssertDatabaseMatchesConfigurationOnStartup` gate whose throw lifts to `IdentityFault.MartenMismatch` — two legs, one band.
- Packages: Marten (the host-builder `ApplyAllDatabaseChangesOnStartup`/`AssertDatabaseMatchesConfigurationOnStartup` registrations + the runtime `IMartenStorage.ApplyAllConfiguredChangesToDatabaseAsync(AutoCreate?)`), Microsoft.EntityFrameworkCore (`GetMigrations`/`GetAppliedMigrations`/`Migrate`/`HasPendingModelChanges`), Microsoft.EntityFrameworkCore.Design (`PrivateAssets=all` — `MigrationsOperations.AddMigration`/`ScriptMigration` idempotent SQL, `DbContextOperations.Optimize` compiled model, `MigrationsBundle.Execute` the fleet migrator; the package earns its admission HERE), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new boot outcome is one `SchemaVerdict` case; a new identity-tier failure is one `IdentityFault` case; a new non-modelable DDL fact is one `IdentityDdl` row the reviewed migration appends; zero new surface — a best-effort open, a per-process bootstrap branch, a bare `Error.New`, hand-authored `MigrationOperation` subclasses, or apply-time gating is the deleted form because boot is one total fold, the failures are one typed band, emission is generated, and the destructive change is gated at generation time.
- Boundary: TWO DDL owners compose at boot — Marten owns its event/document DDL and the EF identity model owns its relational DDL — and each owner's posture is selected by the SAME `Placement` write authority: the single-writer placement APPLIES (EF `store.Database.Migrate()`, Marten `ApplyAllConfiguredChangesToDatabaseAsync` and the host `ApplyAllDatabaseChangesOnStartup` registration) while every fleet member ASSERTS and never applies; the MIGRATION OWNER law: EF.Design EMITS — `dotnet ef migrations add` scaffolds the `element_identity`/`node_cell` migration off the one `IdentityContext` model (the snake-case names, the Thinktecture-converted column types, the `geometry(Polygon, 4326)` column and its `HasMethod("gist")` index annotation, and the SQLite divergence all derive from the model, so BOTH providers' migrations generate through the one owner), the scaffold is REVIEWED generated shape, `ScriptMigration` with the idempotent option produces the deploy-time SQL, `MigrationsBundle.Execute` the self-contained fleet migrator, and `Optimize` the compiled model `ConverterRail` mounts back byte-identically; the raw rows the model CANNOT express — the RLS enable + tenant policies and the frozen `Store/provisioning#ServerExtension.CreateSql` install rows (`postgis`, `h3-pg`, `vector` — the extension DDL commits through THIS rail, `ServerExtension` stays the frozen row vocabulary) — are `IdentityDdl` data the reviewed migration appends via `migrationBuilder.Sql`, never hand-authored operation subclasses; the expand/contract classification runs at GENERATION time over the emitted `MigrationOperation` rows (`AddColumnOperation` expands, `DropColumnOperation` contracts, `AlterColumnOperation` splits into the two waves per the `#IDENTITY_POLICY` expand/flip/contract law), so a destructive wave needs its explicit approval before a bundle ever ships; in the EF fold an applied-minus-assembly non-empty set is a typed `IdentityFault.SchemaAhead` (or `ServingBehind` under a read-ahead placement) carrying the unknown identifiers, never a silent open that corrupts on first write; the route-owned `docs/stacks/csharp/domain/persistence#MIGRATION_ALGEBRA` `SchemaGate`/`Placement`/`SchemaVerdict` is the general EF FORM this page realizes and extends with the Marten apply/assert leg; `EnsureCreated` bypasses the history mechanism and is admitted only for the ephemeral test row; read-ahead serving (`ServingBehind`) is legal only under a declared expand-only suite invariant, so the sound default is hard rejection.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------
// The write-authority axis the route-owned `docs/stacks/csharp/domain/persistence#MIGRATION_ALGEBRA` prescribes,
// declared here as the Persistence-Element owner: single-writer applies both DDL owners, the fleet member
// asserts-only, the reader serves-behind. A `bool writesPending` flag is the rejected form.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Placement {
    public static readonly Placement SingleWriter = new("single-writer", writes: true, appliesPending: true, readsAhead: false);
    public static readonly Placement FleetMember = new("fleet-member", writes: true, appliesPending: false, readsAhead: false);
    public static readonly Placement Reader = new("reader", writes: false, appliesPending: false, readsAhead: true);
    public bool Writes { get; }
    public bool AppliesPending { get; }
    public bool ReadsAhead { get; }
    private Placement(string key, bool writes, bool appliesPending, bool readsAhead) : this(key) => (Writes, AppliesPending, ReadsAhead) = (writes, appliesPending, readsAhead);
}

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SchemaVerdict {
    private SchemaVerdict() { }
    public sealed record Serving : SchemaVerdict;
    public sealed record ServingBehind(Seq<string> Unknown) : SchemaVerdict;
    public sealed record Provisioned(Seq<string> Applied) : SchemaVerdict;
    public sealed record Advanced(Seq<string> Applied) : SchemaVerdict;
    public sealed record AwaitBundle(Seq<string> Pending, bool Fresh) : SchemaVerdict;
    public sealed record Drifted : SchemaVerdict;
}

// --- [ERRORS] --------------------------------------------------------------------------
// Identity-tier faults close schema, spatial, and persisted-key decoding failures: a closed [Union] over the
// KERNEL `Rasm.Domain.Expected` (protected ctor; `Category` virtual; `Code`/`Message` inherited from `Error`) — NOT `LanguageExt.Common.Expected`.
// Band membership derives through the `Element/graph#FAULT_TABLES` registry row — `Code => FaultBand.Identity + n`
// — so the typed case lifts BARE onto `Fin<T>`/`Validation<Error,T>` with no `.ToError()` hop and a recovery reads
// `error.IsType<IdentityFault.CellUnresolvable>()` / `error.HasCode(8344)` / `error.Category()`, never a message
// substring; a bare `Error.New(8341, …)` at a call site is the deleted form. `Element/authority` composes this
// band (no band of its own). No `[GenerateUnionOps]` — the kernel union-ops generator is strictly opt-in.
[Union]
public abstract partial record IdentityFault : Expected, IValidationError<IdentityFault> {
    private IdentityFault() : base() { }

    public sealed record SchemaAhead(Seq<string> Unknown) : IdentityFault;
    public sealed record ApplyFailed(string Detail) : IdentityFault;
    public sealed record MartenMismatch(string Detail) : IdentityFault;
    public sealed record CellUnresolvable(string Detail) : IdentityFault;
    public sealed record KeyMalformed(string Detail) : IdentityFault;

    public override int Code => FaultBand.Identity + Switch(
        schemaAhead:      static _ => 1,
        applyFailed:      static _ => 2,
        martenMismatch:   static _ => 3,
        cellUnresolvable: static _ => 4,
        keyMalformed:     static _ => 5);

    public override string Message => Switch(
        schemaAhead:      static c => $"<schema-ahead:{c.Unknown.Count}>",
        applyFailed:      static c => $"<apply-failed:{c.Detail}>",
        martenMismatch:   static c => $"<marten-mismatch:{c.Detail}>",
        cellUnresolvable: static c => $"<cell-unresolvable:{c.Detail}>",
        keyMalformed:     static c => $"<key-malformed:{c.Detail}>");

    public override string Category => Switch(
        schemaAhead:      static _ => "Schema",
        applyFailed:      static _ => "Apply",
        martenMismatch:   static _ => "Marten",
        cellUnresolvable: static _ => "Cell",
        keyMalformed:     static _ => "Key");

    public static IdentityFault Create(string message) => new MartenMismatch(message);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
// The migration owner's non-modelable rows: EF.Design EMITS the schema (scaffold -> reviewed generated shape;
// both providers' migrations generate off the one model); these are ONLY the facts the model cannot express —
// RLS enable + tenant policies, and the frozen `ServerExtension.CreateSql` install rows the reviewed migration
// appends via `migrationBuilder.Sql`. Hand-authored `MigrationOperation` subclasses are the deleted form.
public static class IdentityDdl {
    public static readonly Seq<string> Rls = toSeq(new[] {
        "ALTER TABLE element_identity ENABLE ROW LEVEL SECURITY",
        "CREATE POLICY element_identity_tenant ON element_identity USING (tenant = current_setting('rasm.tenant')) WITH CHECK (tenant = current_setting('rasm.tenant'))",
        "ALTER TABLE node_cell ENABLE ROW LEVEL SECURITY",
        "CREATE POLICY node_cell_tenant ON node_cell USING (tenant = current_setting('rasm.tenant')) WITH CHECK (tenant = current_setting('rasm.tenant'))",
    });

    // The extension DDL commits through this rail on the postgres arm only: the frozen provisioning row
    // vocabulary supplies the SQL, the migration executes it — never a second install path.
    public static Seq<string> Extensions(Seq<ServerExtension> required) => required.Map(static ext => ext.CreateSql);
}

public static class SchemaGate {
    // The EF half of the boot fold over the relational identity DDL: the assembly-vs-applied migration sets
    // classified by the route-owned `Placement` write authority — schema-ahead is a typed rejection, the
    // single-writer applies, the fleet member AwaitBundle. Every failure is the typed IdentityFault band.
    public static Fin<SchemaVerdict> Admit(DbContext store, Placement placement) {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(placement);
        Seq<string> assembly = toSeq(store.Database.GetMigrations());
        Seq<string> applied = toSeq(store.Database.GetAppliedMigrations());
        Seq<string> unknown = applied.Filter(id => !assembly.Exists(held => held == id));
        Seq<string> pending = assembly.Filter(id => !applied.Exists(held => held == id));
        return (unknown.IsEmpty, pending.IsEmpty) switch {
            (false, _) when placement.ReadsAhead => Fin<SchemaVerdict>.Succ(new SchemaVerdict.ServingBehind(unknown)),
            (false, _) => Fin<SchemaVerdict>.Fail(new IdentityFault.SchemaAhead(unknown)),
            (_, false) when placement.AppliesPending => Try.lift(fun(() => store.Database.Migrate())).Run()
                .Match(
                    Succ: _ => Fin<SchemaVerdict>.Succ(applied.IsEmpty ? new SchemaVerdict.Provisioned(pending) : new SchemaVerdict.Advanced(pending)),
                    Fail: error => Fin<SchemaVerdict>.Fail(new IdentityFault.ApplyFailed(error.Message))),
            (_, false) => Fin<SchemaVerdict>.Succ(new SchemaVerdict.AwaitBundle(pending, Fresh: applied.IsEmpty)),
            _ => Fin<SchemaVerdict>.Succ(store.Database.HasPendingModelChanges() ? new SchemaVerdict.Drifted() : new SchemaVerdict.Serving()),
        };
    }

    // The Marten DDL leg: the single-writer placement APPLIES the document/event schema through the runtime
    // `IMartenStorage.ApplyAllConfiguredChangesToDatabaseAsync` (the fleet member instead carries the
    // host-registered `AssertDatabaseMatchesConfigurationOnStartup` gate whose throw lifts to MartenMismatch
    // BEFORE this runs). A reader/fleet member returns Serving without touching DDL; an apply throw lifts to
    // ApplyFailed, the SAME band the EF leg uses.
    public static IO<SchemaVerdict> AdmitMarten(IMartenStorage storage, Placement placement) =>
        placement.AppliesPending
            ? IO.liftAsync(async () => { await storage.ApplyAllConfiguredChangesToDatabaseAsync().ConfigureAwait(false); return (SchemaVerdict)new SchemaVerdict.Serving(); })
                | @catch<IO, SchemaVerdict>(static _ => true, error => IO.fail<SchemaVerdict>(new IdentityFault.ApplyFailed(error.Message)))
            : IO.pure<SchemaVerdict>(new SchemaVerdict.Serving());
}
```

| [INDEX] | [POLICY]          | [VALUE]                                   | [BINDING]                                                          |
| :-----: | :---------------- | :---------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | Marten DDL        | `AutoCreate.CreateOrUpdate` (writer)      | `AssertDatabaseMatchesConfigurationOnStartup` asserts on the fleet |
|  [02]   | EF identity DDL   | generated migrations (both providers)     | one model emits per-provider SQL; scaffold is reviewed shape       |
|  [03]   | non-modelable DDL | `IdentityDdl.Rls` + `Extensions` rows     | `migrationBuilder.Sql` appends the frozen `ServerExtension` SQL    |
|  [04]   | boot verdict      | one `SchemaGate.Admit` fold (Marten + EF) | schema-ahead is a typed `IdentityFault`, never a silent open       |
|  [05]   | pending apply     | single-writer placement only              | fleet member `AwaitBundle`, never self-applies                     |
|  [06]   | deploy lane       | `ScriptMigration`                         | idempotent deploy SQL                                              |
|  [07]   | deploy lane       | `MigrationsBundle`                        | self-contained fleet migrator                                      |
|  [08]   | deploy lane       | `Optimize`                                | compiled model back through `ConverterRail`                        |

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
