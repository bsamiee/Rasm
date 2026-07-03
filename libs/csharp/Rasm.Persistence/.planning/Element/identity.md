# [PERSISTENCE_ELEMENT_IDENTITY]

Rasm.Persistence anchors every persisted `ElementGraph` to one relational identity tier that commits ATOMICALLY with the Marten event in the same `IDocumentSession`: `ElementIdentity` is the per-model document/row carrying the `Element/graph#STREAM_GRAIN` `ModelId` PK, the `TenantId` RLS column, the set of rooted `NodeId`s, the Bim-projected IFC GlobalId strings (each rooted node's seam `Node.Object.ExternalId`), the H3 spatial cell, the pgvector embedding reference, the `ObjectAcl`, and the classification — so identity plus event are one transaction with no two-ORM gap and the relational columns serve the spatial/vector/ACL/tenant lanes off the one tier. `IdentityPolicy` is the `[SmartEnum<string>]` key axis dispatching mint and decode per row through one generated `Switch`, big-endian transcription preserving order, so an identity change is an expand-wave second key plus a derivation flip plus a contract-wave drop, never an `AlterColumn`. `Grant` is the one `[SmartEnum]` object-authorization vocabulary every ACL entry draws from as `GrantSet` frozen-set membership — object, audit, and branch rights in one set-algebra whose `Owner` superset is value-derived and scoped by `AclScope` — so the `Version/commits#COMMIT_DAG` branch grant is the same vocabulary narrowed, never a parallel taxonomy, and never the AppHost `Agent/capability` effect-gating `Capability` (a disjoint concept that keeps its own name). `SignedAuthorship` is the KMS-signed actor attestation tying a delta to a verified blame agent, and `Authority` folds object admission and authorship verification into one `AuthDecision`; the `Version/provenance#ATTESTED_LEDGER` `AttestedLedger` is the consumer that chains those attestations. `EnvelopeKeyring` is the DEK envelope surface the SAME `KmsProvider` axis selects beside `SigningKeyring` — a provider-neutral `Mint`/`Unwrap`/`Rewrap`/`Probe` delegate quartet wrapping a data-encryption key against the cloud-CMK (AWS `GenerateDataKey`/`Decrypt`/`ReEncrypt` encrypt-as-wrap, Azure native `WrapKey`/`UnwrapKey`, GCP `Encrypt`/`Decrypt` + CRC32C + `UpdateCryptoKeyPrimaryVersion` primary repoint), so the `#KEY_ENVELOPE` owner is THIS tier (no separate `Store/encryption` page exists), the `Store/blobstore#BLOB_GC` `ObjectEncryption` consuming only the server-side-SSE key-id STRING this envelope mints out-of-band while the DEK-wrapping lifecycle lives here. The boot verdict folds two identifier sets into one typed `SchemaVerdict` — the Marten `AssertDatabaseMatchesConfigurationOnStartup` posture plus the EF migration state — so a store whose schema is newer than the compiled model is a typed rejection, never a best-effort open. Every identity-tier failure rails the typed `IdentityFault` band (no bare `Error.New`). `ModelId` arrives settled from the Persistence sibling `Element/graph#STREAM_GRAIN` (the per-model stream key the same session appends to), `NodeId`/`ContentAddress` from `Rasm.Element` (the IFC GlobalId is the seam `Node.Object.ExternalId` string the relational tier projects, never a separate seam type); `ClockPolicy`/`TenantContext`/`CorrelationId`/`Principal`/`SecretLease`/`DataClassification` arrive from `Rasm.AppHost`; `KmsProvider` is the Persistence-side provider axis this `#AUTHORITY` owner declares for BOTH the SIGNING surface (`SigningKeyring` `Sign`/`Verify` over an asymmetric key) AND the disjoint DEK-envelope surface (`EnvelopeKeyring` `Mint`/`Unwrap`/`Rewrap`/`Probe` over the symmetric CMK, the `#KEY_ENVELOPE` owner), the two keyrings binding two different keys behind the one axis; the `Store/blobstore#BLOB_GC` `ObjectEncryption` is the downstream SSE-stance consumer carrying only the key-id STRING the envelope mints, never a second envelope owner — only the `SecretLease`-class KMS handle crosses from `Rasm.AppHost` through the `Runtime/config#KMS_UNWRAP_PORT` seam (the host resolves and leases the cloud-KMS credential; the concrete provider axis stays Persistence-side), never the provider axis itself.

## [01]-[INDEX]

- [01]-[ELEMENT_IDENTITY]: the relational identity tier, the co-transactional Marten-document stamp, and the spatial/vector/tenant join columns.
- [02]-[IDENTITY_POLICY]: the key axis, big-endian transcription, per-row mint/decode, and content addressing.
- [03]-[AUTHORITY]: the unified `Grant` authorization vocabulary, object allow/deny ACL with inheritance, KMS-signed authorship, the DEK-envelope `#KEY_ENVELOPE` keyring (`Mint`/`Unwrap`/`Rewrap`/`Probe`), and the one admission/verification fold.
- [04]-[SCHEMA_VERDICT]: the boot fold over the Marten startup-assertion posture plus the EF migration state into one typed verdict.

## [02]-[ELEMENT_IDENTITY]

- Owner: `ElementIdentity` the per-model identity row carrying the `ModelId` PK plus the `TenantId`/`Roots`/`GlobalIds`/`Cell`/`Embedding`/`Acl`/`Classification`/`At` join columns; `NodeCell` the per-ELEMENT fine-cell routing-vertex row (`Model`/`Node`/`Tenant`/`Cell`) the `Query/cypher#GRAPH_QUERY` `pgrouting` `network_edge` source/target carries and `NodeAt` resolves; `IdentityShape`/`NodeCellShape` the `IEntityTypeConfiguration` mappings of EVERY persistent member — the `Tenant` RLS column, the H3 `bigint`, the pgvector column (the `Option<Vector>` value-converted to `null`/`vector`), the jsonb ACL, the `Roots` `text[]` and `GlobalIds` jsonb (value-converted through the `Element/codec#CODEC_AXIS` `ElementJson` options, never a partial subset that drops them on write), the `Classification` key, the `At` instant, and the per-element `node_cell` GiST index; `IdentityStore` the static surface owning the co-transactional Marten-document stamp, the per-model and per-element spatial index projections, and the cell→NodeId resolver.
- Cases: `Roots` is the set of rooted `NodeId`s the model owns (the `IfcRoot` mirror nodes), `GlobalIds` the 1:1 map from rooted `NodeId` to the compressed IFC GlobalId string projected from each seam `Node.Object.ExternalId` (`H6` — the rooted `NodeId` is the neutral kernel-minted durable key, the IFC GlobalId is the `ExternalId` projection the `Version/merge#STRUCTURAL_DIFF` re-ingest `Reconcile` correlates on, never the key), `Cell` the Uber-H3 spatial index cell over the model envelope centroid, `Embedding` the optional pgvector reference keying the ANN lane — the per-model envelope locator, distinct in grain from the corpus-grain retrieval index and the `ProductCodebook` PQ vocabulary the `Query/lane#VECTOR_CODEBOOK` owner trains, never a duplicate index, `Acl` the `ObjectAcl` grant, `Classification` the `DataClassification` ceiling.
- Entry: `public static IDocumentSession Stamp(IDocumentSession session, ElementIdentity identity)` stores the identity as a Marten document in the SAME session the event appends to so `SaveChangesAsync` commits both atomically — the one primitive the `Element/graph#STORE_RAIL` `GraphStore.Commit` composes rather than inlining `session.Store`, so the identity-plus-event atomicity has ONE owner; `public static Fin<H3Index> Cell(Envelope bounds, int resolution)` projects the model envelope centroid to its H3 cell through `pocketken.H3` so the cell id is computed identically at ingest and in the `h3-pg` server extension; `public static IQueryable<ElementIdentity> Nearby(DbContext db, H3Index cell, int ring)` resolves the spatial neighborhood through the H3 grid disk.
- Auto: the identity tier is a Marten document so it rides the one `IDocumentSession` the `Element/graph#STORE_RAIL` write op uses — `session.Store(identity)` then `SaveChangesAsync` commits the identity row, the appended `GraphEvent`, and the inline `GraphProjection` in one Postgres transaction (`H10` — pick ONE transaction owner for identity plus event, the Marten document in the same session, no free two-ORM atomicity), Marten teaching the strong-typed `ModelId`/`NodeId` keys through the `Element/graph#STREAM_GRAIN` `RegisterValueType` registration so the document id stays typed; the relational columns are ALSO EF-mapped through `IdentityShape` for the Npgsql/pgvector/H3 query lanes Marten's LINQ does not reach (the GiST H3 neighborhood, the pgvector ANN), reading the same rows Marten wrote; the `Cell` mint reads the kernel-managed `H3Index.FromPoint` (the NTS `Point` overload, degree→radian conversion owned by the package) so the cell matches `h3_latlng_to_cell` bit-for-bit, projecting `None`→`IdentityFault.CellUnresolvable` only on the `H3Index.Invalid` zero sentinel; the `Tenant` column is the `current_setting('rasm.tenant')::uuid` RLS partition every read filters on by construction.
- Receipt: an identity stamp rides `store.element.identity` carrying the `Roots` count; a spatial-neighborhood read rides `store.identity.nearby` carrying the ring radius.
- Packages: Marten (`IDocumentSession.Store`), Npgsql.EntityFrameworkCore.PostgreSQL (`HasConversion`/`ComplexProperty.ToJson`), Pgvector.EntityFrameworkCore (`Vector`/`HasColumnType("vector(N)")`), pocketken.H3 (`H3Index.FromPoint`/`GridDiskDistances`), Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite (`Envelope`/`Point`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime.
- Growth: a new identity join column is one field on `ElementIdentity` plus one `IdentityShape` mapping; a new spatial resolution is one H3 cell policy; zero new surface — a separate identity transaction, a second identity ORM committing apart from the event, a parallel `NodeId`-keyed identity table, or an EF-versus-Marten atomicity dance is the deleted form because the identity is a Marten document in the one session and EF reads the same rows for the query lanes only.
- Boundary: the ONE transaction owner for identity plus event is the `IDocumentSession` — the `ElementIdentity` row commits as a Marten document in the same session as the appended events so a single `SaveChangesAsync` is the atomic boundary and `IdentityStore.Stamp` is the lone stamp primitive the `GraphStore.Commit` rail composes (a second `session.Store(identity)` inlined elsewhere is the deleted duplication); EF/Npgsql is the READ projection over the same rows for the H3/pgvector/GiST lanes Marten's LINQ cannot serve, never a second write authority (a `DbContext.SaveChanges` over the identity table beside the Marten append is the deleted two-ORM gap); the rooted `NodeId` is the neutral kernel-minted DURABLE key and the IFC GlobalId is each node's seam `Node.Object.ExternalId` projection (`H6`) the `GlobalIds` map mirrors; a re-import mints fresh neutral `NodeId`s, and the `Version/merge#STRUCTURAL_DIFF` `Reconcile` correlates the re-ingest's rooted nodes on the stable `ExternalId` GlobalId and aligns them back to the durable keys, so the durable `NodeId` and every foreign reference survive re-import unchanged while the projection map mirrors the current `NodeId`↔GlobalId pairing and the kernel owns the whole `IObjectFactory` floor; the H3 `Cell` is computed by the managed `pocketken.H3` `H3Index.FromPoint` (the SRID-4326 NTS `Point` overload, degree→radian owned by the package) identically to the `h3-pg` server extension so the same cell id indexes at ingest and in SQL, deleting a second spatial index scheme, and a `bigint` column stores the cell `ulong` reinterpreted as `long` (the `h3-pg` convention) so the GiST/BRIN index agrees; TWO cell grains live on the one tier and never duplicate — the per-MODEL `ElementIdentity.Cell` (the model envelope-centroid cell `Nearby` reads, the coarse spatial locator) and the per-ELEMENT `NodeCell.Cell` (each rooted element's own fine cell `NodeAt` resolves, the element-DISTINCT routing vertex the `Query/cypher#GRAPH_QUERY` `pgrouting` `network_edge` source/target carries so two connected elements never collapse onto one coarse cell, and the cell-mesh route lands back on real element ids), both computed through the SAME `Cell` mint at different resolutions; the `Tenant` RLS column is the coarse partition and the `ObjectAcl` (`#AUTHORITY`) is the fine within-tenant grant, two altitudes never duplicated.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Collections.Frozen;
using Generator.Equals;
using H3;
using H3.Algorithms;
using LanguageExt;
using LanguageExt.Common;
using Marten;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using NodaTime;
using Rasm.Element;
using Thinktecture;
using Expected = Rasm.Domain.Expected;            // the federation fault-band base — NOT LanguageExt.Common.Expected
using static LanguageExt.Prelude;

namespace Rasm.Persistence;

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

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ElementIdentity(
    ModelId Model,
    UInt128 Tenant,
    Seq<NodeId> Roots,
    HashMap<NodeId, string> GlobalIds,
    H3Cell Cell,
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
public sealed record NodeCell(ModelId Model, NodeId Node, UInt128 Tenant, H3Cell Cell);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class IdentityShape : IEntityTypeConfiguration<ElementIdentity> {
    public void Configure(EntityTypeBuilder<ElementIdentity> identity) {
        ArgumentNullException.ThrowIfNull(identity);
        identity.ToTable("element_identity");
        identity.HasKey(static e => e.Model);
        identity.Property(static e => e.Tenant).HasColumnName("tenant");
        identity.Property(static e => e.Cell).HasColumnName("h3_cell");
        identity.Property(static e => e.Classification).HasColumnName("classification");
        identity.Property(static e => e.At).HasColumnName("at");
        // The Option<Vector> rides one converter: Some -> the vector, None -> a NULL column, so the optional embedding
        // is an absent pgvector value rather than a wrapper EF cannot map; the column sizes vector(1536) by metadata.
        identity.Property(static e => e.Embedding).HasColumnName("embedding").HasColumnType("vector(1536)")
            .HasConversion(
                new ValueConverter<Option<Pgvector.Vector>, Pgvector.Vector?>(
                    static o => o.MatchUnsafe(Some: static v => v, None: static () => null),
                    static v => Optional(v)),
                new ValueComparer<Option<Pgvector.Vector>>(
                    static (x, y) => x == y, static v => v.GetHashCode(), static v => v));
        identity.Property(static e => e.Roots).HasColumnName("roots")
            .HasConversion(
                new ValueConverter<Seq<NodeId>, string[]>(
                    static r => r.Map(static n => n.Value).ToArray(),
                    static a => toSeq(a).Map(NodeId.Create)),
                new ValueComparer<Seq<NodeId>>(static (x, y) => x == y, static v => v.GetHashCode(), static v => v));
        identity.Property(static e => e.GlobalIds).HasColumnName("global_ids").HasColumnType("jsonb")
            .HasConversion(
                new ValueConverter<HashMap<NodeId, string>, string>(
                    static m => System.Text.Json.JsonSerializer.Serialize(m.ToDictionary(static p => p.Key.Value, static p => p.Value), ElementJson.Options),
                    static s => toHashMap((System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(s, ElementJson.Options) ?? []).Select(static kv => (NodeId.Create(kv.Key), kv.Value)))),
                new ValueComparer<HashMap<NodeId, string>>(static (x, y) => x == y, static v => v.GetHashCode(), static v => v));
        identity.ComplexProperty(static e => e.Acl, static acl => acl.ToJson("acl"));
        identity.HasIndex(static e => e.Cell);
        identity.HasIndex(static e => e.Tenant);
    }
}

// The per-element routing-vertex mapping: the composite (`Model`, `Node`) PK plus the GiST-indexed `node_cell` the
// `pgrouting` network and `NodeAt` resolve over, the `Tenant` RLS column the per-element rows share with the identity
// tier. The fine cell is element-distinct so the connection graph never collapses two elements onto one coarse cell.
public sealed class NodeCellShape : IEntityTypeConfiguration<NodeCell> {
    public void Configure(EntityTypeBuilder<NodeCell> node) {
        ArgumentNullException.ThrowIfNull(node);
        node.ToTable("node_cell");
        node.HasKey(static n => new { n.Model, n.Node });
        node.Property(static n => n.Node).HasColumnName("node").HasConversion(static n => n.Value, static s => NodeId.Create(s));
        node.Property(static n => n.Tenant).HasColumnName("tenant");
        node.Property(static n => n.Cell).HasColumnName("node_cell");
        node.HasIndex(static n => n.Cell);
        node.HasIndex(static n => n.Tenant);
    }
}

public static class IdentityStore {
    public static IDocumentSession Stamp(IDocumentSession session, ElementIdentity identity) {
        ArgumentNullException.ThrowIfNull(session);
        session.Store(identity);
        return session;
    }

    // Mint the cell through the managed pocketken.H3 entry that mirrors `h3_latlng_to_cell` — the NTS `Point` overload
    // (SRID 4326, the package owning the degree->radian conversion), NOT a hand-built radian LatLng that would treat
    // degrees as radians. The cell is durable as the `long` reinterpretation; an out-of-range centroid decodes to the
    // `H3Index.Invalid` zero sentinel, which never persists (it rails CellUnresolvable instead of a stored 0 cell).
    public static Fin<H3Cell> Cell(Envelope bounds, int resolution) {
        ArgumentNullException.ThrowIfNull(bounds);
        var index = H3.H3Index.FromPoint(new Point(bounds.Centre.X, bounds.Centre.Y) { SRID = 4326 }, resolution);
        return index.IsValidCell
            ? Fin<H3Cell>.Succ(H3Cell.Of(index))
            : Fin<H3Cell>.Fail(new IdentityFault.CellUnresolvable($"<centroid:{bounds.Centre.X},{bounds.Centre.Y}@{resolution}>"));
    }

    // The H3 neighborhood is the filled grid disk: `GridDiskDistances` yields each `RingCell` (cell + ring distance),
    // and the durable `long` reinterpretation is the `IN`/`= ANY` membership the GiST `h3_cell` index answers — never a
    // per-row distance scan, never the phantom `Api.GridDisk` (the real disk-with-distances is `GridDiskDistances`).
    public static IQueryable<ElementIdentity> Nearby(DbContext db, H3Cell cell, int ring) {
        ArgumentNullException.ThrowIfNull(db);
        var disk = toSeq(cell.Live.GridDiskDistances(ring)).Map(static rc => H3Cell.Of(rc.Index)).ToArray();
        return db.Set<ElementIdentity>().Where(e => disk.Contains(e.Cell));
    }

    // The PER-ELEMENT FINE cell index — the `NodeCell` row maps one rooted element `NodeId` to its OWN fine-resolution
    // H3 cell (the element's envelope-centroid cell, NOT the coarse per-model `ElementIdentity.Cell`), so the
    // `Query/cypher#GRAPH_QUERY` `pgrouting` `network_edge` (projected from the element Connect/Generic relationships)
    // carries an element-DISTINCT `source`/`target` cell vertex — two connected elements in one model never collapse onto
    // one coarse model-centroid cell. `NodeAt` is the cell->NodeId resolver the `Query/cypher#GRAPH_QUERY` `ToCells`->element
    // hop binds: a routed `H3Cell` resolves to the rooted `NodeId`s whose fine cell IS that cell (the GiST `node_cell`
    // index membership), so the analytical cell-mesh route lands back on real element ids rather than a fabricated NodeId.
    // The coarse model `Cell` stays the per-MODEL spatial locator (`Nearby`); this is the per-ELEMENT routing-vertex index,
    // two grains on one tier, never one duplicated.
    public static Fin<H3Cell> NodeCell(Envelope nodeBounds, int resolution) => Cell(nodeBounds, resolution);

    public static IQueryable<NodeCell> NodeAt(DbContext db, H3Cell cell) {
        ArgumentNullException.ThrowIfNull(db);
        return db.Set<NodeCell>().Where(n => n.Cell == cell);
    }
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | one txn owner       | identity as Marten doc in one session  | `IdentityStore.Stamp` + `SaveChangesAsync`; no two-ORM gap |
|  [02]   | read projection     | EF/Npgsql over the same rows           | H3/pgvector/GiST lanes Marten LINQ cannot serve           |
|  [03]   | rooted key          | neutral kernel-minted durable `NodeId` | IFC GlobalId is the `Node.Object.ExternalId` the `GlobalIds` map mirrors; re-ingest correlates on it |
|  [04]   | spatial cell        | `H3Index.FromPoint` = `h3_latlng_to_cell` | one cell id at ingest and in SQL; `bigint` reinterpretation |
|  [05]   | tenant partition    | `Tenant` RLS column                    | coarse scope; `ObjectAcl` is the fine within-tenant grant |

## [03]-[IDENTITY_POLICY]

- Owner: `IdentityPolicy` the `[SmartEnum<string>]` five-row key axis carrying generator, big-endian transcription, ordering, collision class, and CLR type, dispatching `Mint` per row through one generated `Switch`; `StoreKey` the `[Union]` closed key carrier (`Surrogate`/`Content`/`Natural`); `Collision` the collision-posture vocabulary.
- Cases: `uuid-v7` (default, B-tree insert-local, `Guid.CreateVersion7`), `uuid-v7-backfill` (historical-timestamp mint for deterministic backfill), `content-hash` (immutable-payload `XxHash128` addressing — the `ContentAddress` row), `natural-key` (caller-owned identifier passthrough), `namespace-key` (RFC-4122 v5 over a namespace and a name for stable derived ids); `Collision` rows are `unmintable`, `content-idempotent`, `foreign-authority`, `derived-deterministic`.
- Entry: `public StoreKey Mint(ReadOnlyMemory<byte> material, Instant observed)` is the per-row factory dispatching through the generated `Switch`; `public StoreKey Decode(ReadOnlySpan<byte> spelled)` is the per-row inverse; `StoreKey.Spelled` is the ordering-preserving big-endian transcription; `StoreKey.ObservedAt` projects a v7 key's embedded creation time.
- Packages: Thinktecture.Runtime.Extensions, System.IO.Hashing (`XxHash128`), System.Security.Cryptography (`IncrementalHash`/`SHA1`), System.Buffers.Binary, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: one `IdentityPolicy` row carries text, CLR type, ordering, collision, and client-generated precedence; a new posture is one `Collision` row; zero new surface.
- Boundary: every persisted key strategy traces to one row here — `uuid-ossp` is the deleted extension route; `StoreKey` is the one closed key carrier so a column type is a case projection, never a parallel key type per provider; ordering survives transcription only when the spelling preserves it — every case transcribes big-endian (`Guid.ToByteArray(bigEndian: true)`, `WriteUInt128BigEndian`, UTF-8) because the platform-default little-endian export fractures a binary-keyed index; `StoreKey.ObservedAt` makes a v7 key a free coarse creation-time axis so a composite `(low-cardinality discriminant, v7 key)` index stays append-local; identity-row change is never `AlterColumn` — it is an expand-wave second key backfilled by the `uuid-v7-backfill` row, a derivation flip, and a contract-wave drop, the only identity migration preserving foreign references and AS-OF cursor validity at once; content identity is non-cryptographic `XxHash128` (the kernel-composed `ContentAddress`, no security claim) and `namespace-key` mints the canonical RFC-4122 v5 namespace UUID (`SHA1` the spec construction, not a security claim).

```csharp signature
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

    public byte[] Spelled() => this switch {
        Surrogate s => s.Value.ToByteArray(bigEndian: true),
        Content c => SpelledContent(c.Value),
        Natural n => System.Text.Encoding.UTF8.GetBytes(n.Value),
    };

    static byte[] SpelledContent(UInt128 value) { var b = new byte[16]; System.Buffers.Binary.BinaryPrimitives.WriteUInt128BigEndian(b, value); return b; }

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
    public static readonly IdentityPolicy ContentHash = new("content-hash", typeof(UInt128), Collision.ContentIdempotent, ordered: false);
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
        contentHash: static s => new StoreKey.Content(System.IO.Hashing.XxHash128.HashToUInt128(s.Material.Span)),
        naturalKey: static s => new StoreKey.Natural(System.Text.Encoding.UTF8.GetString(s.Material.Span)),
        namespaceKey: static s => new StoreKey.Surrogate(NamespaceUuid(Namespace, s.Material.Span)));

    public StoreKey Decode(ReadOnlySpan<byte> spelled) => Switch<byte[], StoreKey>(
        state: spelled.ToArray(),
        uuidV7Key: static b => new StoreKey.Surrogate(new Guid(b.AsSpan()[..16], bigEndian: true)),
        uuidV7Backfill: static b => new StoreKey.Surrogate(new Guid(b.AsSpan()[..16], bigEndian: true)),
        contentHash: static b => new StoreKey.Content(System.Buffers.Binary.BinaryPrimitives.ReadUInt128BigEndian(b)),
        naturalKey: static b => new StoreKey.Natural(System.Text.Encoding.UTF8.GetString(b)),
        namespaceKey: static b => new StoreKey.Surrogate(new Guid(b.AsSpan()[..16], bigEndian: true)));

    static Guid NamespaceUuid(Guid ns, ReadOnlySpan<byte> name) {
        Span<byte> nsBytes = stackalloc byte[16];
        _ = ns.TryWriteBytes(nsBytes, bigEndian: true, out _);
        using var sha = System.Security.Cryptography.IncrementalHash.CreateHash(System.Security.Cryptography.HashAlgorithmName.SHA1);
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

## [04]-[AUTHORITY]

- Owner: `Grant` the one `[SmartEnum]` object-authorization vocabulary (object, audit, and branch rights) whose combinable membership is the `GrantSet` frozen-set value with set-algebra `Admits` (superuser-aware); `GrantSet` the `[ComplexValueObject]` carrying the frozen `Grant` set plus the value-derived `Owner` superset and the `Admits(demand)` containment operator; `AclScope` the gated-object-kind vocabulary carrying its altitude `Parent`; `AclEntry` the per-principal allow/deny grant with provenance and a `[From, Until)` window; `ObjectAcl` the owner-plus-inherited grant set; `KmsProvider` the Persistence-side KMS provider axis (`None`/`Aws`/`Azure`/`Gcp`) carrying its `Signs` discriminant and its `NativeWrap` envelope discriminant (Azure wraps natively; AWS/GCP encrypt-as-wrap); `SigningAlgorithm` the algorithm axis carrying its `HashAlgorithmName` and provider `WireName`; `OpDigest` the `[ValueObject<byte[]>]` cryptographic op-hash; `SigningKeyring` the provider-neutral `Sign`/`Verify` delegate pair; `SignedAuthorship` the KMS-signed actor attestation; `EnvelopeAad` the `[ComplexValueObject]` additional-authenticated-data binding (store partition plus the RLS-digested tenant) every wrap/unwrap carries; `KeyState` the cloud-key lifecycle vocabulary the `Probe` arm resolves (`Enabled`/`Disabled`/`Destroyed`/`Scheduled`/`Pending`); `WrappedKey` the persisted envelope carrier (the wrapped DEK bytes plus the wrapping key id and version); `EnvelopeKeyring` the provider-neutral `Mint`/`Unwrap`/`Rewrap`/`Probe` DEK-envelope delegate quartet the `#KEY_ENVELOPE` owner binds; `SignedAuthorship` the KMS-signed actor attestation; `AuthDecision` the closed admission/verification verdict; `Authority` the one fold over object admission, authorship verification, AND DEK envelope minting/unwrapping.
- Cases: `Grant` is `Read`, `Write`, `Delete`, `Share`, `Revoke` (object), `Audit` (the blame/audit-log lane), `Merge`, `Rebase`, `ForcePush` (branch), `Admin` (the explicit superuser bit) — `GrantSet.Owner` the value-derived full set; `AclScope` is `document → branch → tenant` with `element-set` nesting under `document`; `KmsProvider` is `None` (the local/Personal `Signs: false`, `NativeWrap: false` unsigned tier) / `Aws` (`NativeWrap: false`, encrypt-as-wrap) / `Azure` (`NativeWrap: true`, native `WrapKey`/`UnwrapKey`) / `Gcp` (`NativeWrap: false`, encrypt-as-wrap with CRC32C); `SigningAlgorithm` carries one row per JWS algorithm × digest width the providers intersect (es/rs/ps × 256/384/512); `KeyState` is `Enabled` (admits a wrap) / `Disabled` / `Destroyed` / `Scheduled` (destroy-scheduled) / `Pending` (key-material pending) — only `Enabled` admits; `AuthDecision` is `Granted | Attested | Authentic | Unsigned | Denied | ScopeMismatch | Expired | Unauthored | Forged | DigestWidth | Wrapped | Unwrapped | KeyUnusable` — `Unsigned` the `!provider.Signs` local-tier authorship verdict, `Wrapped`/`Unwrapped` the envelope verdicts carrying the `WrappedKey`/recovered-DEK, `KeyUnusable` the `Probe`-rejected non-`Enabled` `KeyState`.
- Entry: `public static AuthDecision Admit(ObjectAcl acl, Principal principal, Seq<Principal> roles, GrantSet demand, UInt128 scope, Instant now)` folds owner, direct, role, and inherited grants into one effective `GrantSet` with deny-over-allow; `public static IO<AuthDecision> Attest(...)` signs an `OpDigest` through the KMS signing keyring after gating its width; `public static IO<AuthDecision> Verify(...)` checks the digest binding and signature; `public static IO<AuthDecision> Wrap(EnvelopeKeyring keyring, EnvelopeAad aad)` mints a fresh DEK and returns it `Wrapped` (the `Probe`-gated `Mint` arm short-circuits `KeyUnusable` on a non-`Enabled` `KeyState`); `public static IO<AuthDecision> Unwrap(EnvelopeKeyring keyring, WrappedKey wrapped, EnvelopeAad aad)` recovers the plaintext DEK `Unwrapped` and the caller zeroizes it after the local bind; `public static IO<AuthDecision> Rewrap(EnvelopeKeyring keyring, WrappedKey wrapped, EnvelopeAad aad)` advances the wrapping-key version (the AWS `ReEncrypt`, the GCP primary repoint, the Azure re-`WrapKey`) without the plaintext DEK crossing the wire — one envelope fold beside the one signing fold.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, AWSSDK.KeyManagementService (signing `SigningAlgorithmSpec`/`SignAsync`/`VerifyAsync`/`MessageType.DIGEST`; envelope `GenerateDataKeyAsync`/`DecryptAsync`/`ReEncryptAsync`; probe `DescribeKeyAsync`), Azure.Security.KeyVault.Keys (signing `SignatureAlgorithm`/`Sign`/`Verify`; native envelope `CryptographyClient.WrapKey`/`UnwrapKey` over `KeyWrapAlgorithm.RsaOaep256`; `KeyClient` key-state), Google.Cloud.Kms.V1 (envelope `EncryptAsync`/`DecryptAsync` + bidirectional CRC32C; rotation `UpdateCryptoKeyPrimaryVersionAsync`; probe `GetCryptoKeyVersionAsync` `CryptoKeyVersionState`; `GenerateRandomBytesAsync` off-board DEK), System.Security.Cryptography (`CryptographicOperations.HashData`/`ZeroMemory`), System.Collections.Frozen.
- Growth: one `Grant` row per new permission; one `AclScope` row per new gated kind; one `KmsProvider` row per new cloud KMS (a non-signing provider sets `Signs: false` and routes through the SAME `Unsigned` path; a native-wrap provider sets `NativeWrap: true` and binds the `EnvelopeKeyring` `Mint`/`Rewrap` against its wrap verb rather than encrypt-as-wrap); one `SigningAlgorithm` row per JWS family; one `KeyState` row per new lifecycle posture; one `AuthDecision` case per verdict; zero new surface — a separate `Store/encryption` page for the envelope keyring (the surface lives HERE beside the signing keyring on the one `KmsProvider` axis), a second provider axis, or a Persistence-side long-lived DEK cache is the deleted form.
- Boundary: `Grant` is the one OBJECT-authorization vocabulary — distinct in name and concept from the AppHost `Agent/capability#CAPABILITY_REGISTRY` effect-gating `Capability` (`LocalCompute`/`StoreRead`/`StoreWrite`/`RemoteCompute`), the two never sharing a name across the strata; the `Version/commits#COMMIT_DAG` `BranchRef` grant is this same `GrantSet` narrowed to the branch lane under `AclScope.Branch`, so a second branch-only enum is the deleted form and the audit-log read is the `Audit` grant; combinable authority is a `GrantSet` frozen-set value with set-algebra membership (the `shapes.md` `ReplaceFlags` law — a `[Flags]` enum is the deleted form), so superuser is value-derived (`set.Admits(demand)` is true when the set carries `Admin` regardless of the demanded grants, an `Admin`-only owner admitted `Read`) and `GrantSet.Owner` is the full vocabulary frozen once; object authorization is the per-object/per-branch allow/deny grant with deny-over-allow so an explicit `AclEntry` deny set difference overrides every inherited allow, the `Owner` principal carries `GrantSet.Owner` so the creator never locks itself out, and the `[From, Until)` window time-boxes both ends (a future grant stays `Denied`, a lapsed one folds `Expired`); inheritance is the `AclScope.Parent` altitude ladder with the `child.Kind.Parent == Some(parent.Kind)` invariant so a mis-stacked chain rejects; the `#ELEMENT_IDENTITY` `Tenant` RLS column is the coarse scope and this object-ACL the fine, two altitudes never duplicated; signed authorship is the actor-to-blame seam — a cloud-KMS op carries a `SignedAuthorship` over a `SigningAlgorithm`-width cryptographic `OpDigest` so a blame attribution (`Version/timetravel`, `Version/provenance#ATTESTED_LEDGER`) names a verified actor, a 16-byte non-cryptographic content hash standing in for the signed digest being the deleted form, and the `!provider.Signs` path (`KmsProvider.None`, the local/Personal tier) is the one unsigned route projecting `Unsigned` so a store with no KMS still records the delta→actor binding; the `SigningKeyring` is the KMS SIGNING surface (`Sign`/`Verify` over an asymmetric key, the disjoint operation from the DEK envelope below), resolving the key through the same AppHost `SecretLease`-class handle, never a bare passphrase, and the provider-specific algorithm type (`SigningAlgorithmSpec`/`SignatureAlgorithm`) lives only at the keyring delegate edge; the `EnvelopeKeyring` is the DEK-ENVELOPE surface this `#KEY_ENVELOPE` owner holds beside the signing keyring on the ONE `KmsProvider` axis — there is NO separate `Store/encryption` page, the `Mint`/`Unwrap`/`Rewrap`/`Probe` quartet wrapping a data-encryption key against the symmetric CMK where each arm's mechanism is a policy value on the `KmsProvider` row (the `NativeWrap` provider routes `Mint`/`Rewrap` through Azure's native `WrapKey`/`UnwrapKey`, the encrypt-as-wrap providers through AWS `GenerateDataKey`/`Decrypt`/`ReEncrypt` and GCP `Encrypt`/`Decrypt`+`UpdateCryptoKeyPrimaryVersion`, never one arm's spelling as a universal law), the `Probe` arm resolving the `KeyState` so a wrap against a `Disabled`/`Scheduled` key rejects `KeyUnusable` at admission, the `EnvelopeAad` (store partition + the RLS-digested `TenantContext.TenantId.Uuid` through `XxHash128`) riding the provider `EncryptionContext`/`AdditionalAuthenticatedData` on the AWS/GCP context arms and compared application-side on the Azure native-wrap arm so a blob cannot be unwrapped under a foreign partition, the recovered plaintext DEK zeroized through `CryptographicOperations.ZeroMemory` immediately after the local bind so a Persistence-side long-lived key is the deleted form; the `Store/blobstore#BLOB_GC` `ObjectEncryption` is the downstream SSE-stance consumer carrying only the server-side-SSE key-id STRING this envelope mints out-of-band (a `ManagedKey` SSE-KMS key id), never a second envelope owner and never the DEK-wrapping lifecycle; only the `SecretLease`-class KMS handle crosses from `Rasm.AppHost` through the `Runtime/config#KMS_UNWRAP_PORT` `[PORT]: KMS-unwrap` seam, the host owning the credential lifecycle and Persistence the resolved-handle envelope ops; `Principal` arrives from `Rasm.AppHost/Agent/identity#PRINCIPAL` (the verified inbound-actor shape), never re-declared as a local `string` slice.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------
// The object-authorization vocabulary: keyless [SmartEnum] reference items (process-local, no wire key — the wire
// carries the GrantSet's frozen membership), distinct from the AppHost effect-gating `Capability`. Combinable
// authority is the GrantSet frozen-set value below, NOT a [Flags] enum (the `shapes.md` ReplaceFlags law).
[SmartEnum]
public sealed partial class Grant {
    public static readonly Grant Read = new();
    public static readonly Grant Write = new();
    public static readonly Grant Delete = new();
    public static readonly Grant Share = new();
    public static readonly Grant Revoke = new();
    public static readonly Grant Audit = new();
    public static readonly Grant Merge = new();
    public static readonly Grant Rebase = new();
    public static readonly Grant ForcePush = new();
    public static readonly Grant Admin = new();
}

// Combinable authority as set algebra (ReplaceFlags): the frozen Grant set, value-equal by SET membership through
// Generator.Equals `[SetEquality]` (NOT a Thinktecture key owner — the two equality models never stack; a multi-member
// structural value is `[Equatable]`), so two sets with the same grants compare equal regardless of order. `Owner` is the
// value-derived full vocabulary; `Admits` is superuser-aware containment (an Admin-bearing set admits any demand);
// union/difference are the allow/deny fold primitives. A [Flags] enum bitfield is the deleted form.
[Equatable]
public sealed partial record GrantSet([property: SetEquality] FrozenSet<Grant> Grants) {
    public static readonly GrantSet None = new(FrozenSet<Grant>.Empty);
    public static readonly GrantSet Owner = new(Grant.Items.ToFrozenSet());

    public static GrantSet Of(params ReadOnlySpan<Grant> grants) => new(grants.ToArray().ToFrozenSet());
    public GrantSet Union(GrantSet other) => new(Grants.Union(other.Grants).ToFrozenSet());
    public GrantSet Without(GrantSet other) => new(Grants.Except(other.Grants).ToFrozenSet());
    public bool Admits(GrantSet demand) => Grants.Contains(Grant.Admin) || demand.Grants.IsSubsetOf(Grants);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AclScope {
    public static readonly AclScope Tenant = new("tenant", None);
    public static readonly AclScope Branch = new("branch", Some(Tenant));
    public static readonly AclScope Document = new("document", Some(Branch));
    public static readonly AclScope ElementSet = new("element-set", Some(Document));
    public Option<AclScope> Parent { get; }
    private AclScope(string key, Option<AclScope> parent) : this(key) => Parent = parent;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record AclEntry(GrantSet Allow, GrantSet Deny, Principal GrantedBy, Instant At, Option<Instant> From, Option<Instant> Until) {
    public bool Live(Instant now) => From.Match(Some: f => now >= f, None: static () => true) && Until.Match(Some: u => now < u, None: static () => true);
    public bool Lapsed(Instant now) => Until.Match(Some: u => now >= u, None: static () => false);
}

// A plain record: LanguageExt HashMap/Option/Seq carry structural value equality by construction, so no Generator.Equals
// or Thinktecture owner is needed (stacking either over the already-value-equal LanguageExt collections is the deleted
// over-ownership) — the recursive Inherited chain compares whole by the same default equality.
public sealed record ObjectAcl(
    UInt128 Scope, AclScope Kind, Principal Owner,
    HashMap<string, AclEntry> Principals, HashMap<string, AclEntry> Roles, Option<ObjectAcl> Inherited) {
    public bool LadderValid => Inherited.Match(Some: p => Kind.Parent == Some(p.Kind) && p.LadderValid, None: static () => true);
}

// The Persistence-side KMS provider axis BOTH the signing surface (`SigningKeyring`/`SignedAuthorship`) AND the DEK
// envelope surface (`EnvelopeKeyring`, the `#KEY_ENVELOPE` owner) resolve against — the concrete SDK binding (AWS KMS /
// Azure Key Vault / GCP KMS) stays Persistence-side per the AppHost `Runtime/config#KMS_UNWRAP_PORT` seam, AppHost
// surfacing only the `SecretLease`-class handle. `None` is the local/Personal tier with no cloud KMS: `Authority.Attest`/
// `Verify` short to `AuthDecision.Unsigned` so a store with no KMS still records the delta->actor binding, never a
// fabricated signature. `Signs` gates the signing arm; `NativeWrap` gates the envelope arm — `Azure` wraps a DEK
// through the native `CryptographyClient.WrapKey`/`UnwrapKey` verb, while `Aws`/`Gcp` (no native wrap verb) encrypt-as-wrap
// (`GenerateDataKey`/`Decrypt`/`ReEncrypt`, `Encrypt`/`Decrypt`/`UpdateCryptoKeyPrimaryVersion`), so the keyring `Mint`/
// `Rewrap` arm reads `NativeWrap` rather than hardcoding one provider's spelling. The `WireName` is the provider's
// algorithm-spec dialect prefix the keyring delegate edge maps `SigningAlgorithm.WireName` through (`SigningAlgorithmSpec`/
// `SignatureAlgorithm`); a bare passphrase, a hardcoded single-provider call, or a separate `Store/encryption` provider
// axis is the deleted form (the one axis serves both keyrings).
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

// The cloud-key lifecycle the `EnvelopeKeyring.Probe` arm resolves (AWS `DescribeKey` `KeyState`, Azure `KeyProperties`,
// GCP `CryptoKeyVersion.CryptoKeyVersionState`): only `Enabled` admits a wrap, so a `Mint`/`Rewrap` against a `Disabled`/
// `Destroyed`/`Scheduled`/`Pending` key rejects `AuthDecision.KeyUnusable` at admission rather than failing the provider
// call deep in the wrap. One vocabulary across the three providers, each mapped at the keyring delegate edge.
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
    public static readonly SigningAlgorithm Es256 = new("es256", System.Security.Cryptography.HashAlgorithmName.SHA256, "ECDSA_SHA_256");
    public static readonly SigningAlgorithm Es384 = new("es384", System.Security.Cryptography.HashAlgorithmName.SHA384, "ECDSA_SHA_384");
    public static readonly SigningAlgorithm Es512 = new("es512", System.Security.Cryptography.HashAlgorithmName.SHA512, "ECDSA_SHA_512");
    public static readonly SigningAlgorithm Ps256 = new("ps256", System.Security.Cryptography.HashAlgorithmName.SHA256, "RSASSA_PSS_SHA_256");
    public static readonly SigningAlgorithm Ps384 = new("ps384", System.Security.Cryptography.HashAlgorithmName.SHA384, "RSASSA_PSS_SHA_384");
    public static readonly SigningAlgorithm Ps512 = new("ps512", System.Security.Cryptography.HashAlgorithmName.SHA512, "RSASSA_PSS_SHA_512");
    public static readonly SigningAlgorithm Rs256 = new("rs256", System.Security.Cryptography.HashAlgorithmName.SHA256, "RSASSA_PKCS1_V1_5_SHA_256");
    public static readonly SigningAlgorithm Rs384 = new("rs384", System.Security.Cryptography.HashAlgorithmName.SHA384, "RSASSA_PKCS1_V1_5_SHA_384");
    public static readonly SigningAlgorithm Rs512 = new("rs512", System.Security.Cryptography.HashAlgorithmName.SHA512, "RSASSA_PKCS1_V1_5_SHA_512");
    public System.Security.Cryptography.HashAlgorithmName Hasher { get; }
    public string WireName { get; }
    public int DigestWidth => Hasher == System.Security.Cryptography.HashAlgorithmName.SHA512 ? 64 : Hasher == System.Security.Cryptography.HashAlgorithmName.SHA384 ? 48 : 32;
    private SigningAlgorithm(string key, System.Security.Cryptography.HashAlgorithmName hasher, string wireName) : this(key) => (Hasher, WireName) = (hasher, wireName);
    public OpDigest Hash(ReadOnlySpan<byte> opBytes) { Span<byte> d = stackalloc byte[64]; var w = System.Security.Cryptography.CryptographicOperations.HashData(Hasher, opBytes, d); return OpDigest.Create(d[..w].ToArray()); }
}

[ValueObject<byte[]>(EqualityComparisonOperators = OperatorsGeneration.Default)]
public readonly partial struct OpDigest {
    private static Validation<ValidationError, byte[]> ValidateFactoryArguments(byte[] value) =>
        value is { Length: 32 or 48 or 64 } ? Validation<ValidationError, byte[]>.Success(value) : Validation<ValidationError, byte[]>.Fail(new ValidationError($"<op-digest-width:{value.Length}>"));
    public bool Fits(SigningAlgorithm algorithm) => Value.Length == algorithm.DigestWidth;
}

public sealed record SigningKeyring(SigningAlgorithm Algorithm, Func<ReadOnlyMemory<byte>, IO<ReadOnlyMemory<byte>>> Sign, Func<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>, IO<bool>> Verify);
public sealed record SignedAuthorship(Principal Actor, KmsProvider Provider, string SigningKeyId, SigningAlgorithm Algorithm, OpDigest Digest, ReadOnlyMemory<byte> Signature, Instant At, CorrelationId Correlation);

// The additional-authenticated-data binding every wrap/unwrap carries: the store partition plus (under RLS) the tenant
// id digested through the kernel seed-zero `XxHash128` so the AAD is a fixed-width opaque value, never a raw tenant
// uuid on the wire. It rides the provider `EncryptionContext` (AWS) / `AdditionalAuthenticatedData` (GCP) exact-match
// and is compared application-side on the Azure native-wrap arm (which carries no per-call AAD), so a DEK wrapped for
// one (partition, tenant) cannot be unwrapped under another. `[ComplexValueObject]` value-equates the pair.
[ComplexValueObject]
public sealed partial class EnvelopeAad {
    public string Partition { get; }
    public UInt128 TenantDigest { get; }
    public FrozenDictionary<string, string> Context => new Dictionary<string, string> { ["partition"] = Partition, ["tenant"] = TenantDigest.ToString("x32") }.ToFrozenDictionary();
    public static EnvelopeAad Of(string partition, UInt128 tenant) => new(partition, System.IO.Hashing.XxHash128.HashToUInt128(System.Text.Encoding.UTF8.GetBytes($"{partition}:{tenant:x32}")));
}

// The persisted envelope carrier: the wrapped DEK bytes plus the wrapping key id and the exact key version the wrap
// used (the AWS `KeyMaterialId`, the Azure key version, the GCP `CryptoKeyVersionName`), so a `Rewrap` advances the
// version and an `Unwrap` resolves the embedded version without a second lookup. The plaintext DEK is NEVER a field.
public readonly record struct WrappedKey(ReadOnlyMemory<byte> Ciphertext, string WrappingKeyId, string Version);

// The provider-neutral DEK-envelope quartet (the `#KEY_ENVELOPE` surface, beside `SigningKeyring` on the one
// `KmsProvider` axis): `Mint` wraps a fresh DEK (returning the plaintext for local AES use plus the `WrappedKey` to
// persist), `Unwrap` recovers the plaintext from a `WrappedKey`, `Rewrap` advances the wrapping-key version with the
// plaintext never crossing the wire, `Probe` resolves the `KeyState`. Each delegate closes over the provider arm's
// concrete client (the `NativeWrap` provider's `WrapKey`/`UnwrapKey`, the encrypt-as-wrap provider's `Encrypt`/`Decrypt`/
// rotation) so `Authority` composes one shape across all three. The AAD binds every wrap and unwrap.
public sealed record EnvelopeKeyring(
    KmsProvider Provider,
    Func<EnvelopeAad, IO<(ReadOnlyMemory<byte> Dek, WrappedKey Wrapped)>> Mint,
    Func<WrappedKey, EnvelopeAad, IO<ReadOnlyMemory<byte>>> Unwrap,
    Func<WrappedKey, EnvelopeAad, IO<WrappedKey>> Rewrap,
    Func<IO<KeyState>> Probe);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AuthDecision {
    private AuthDecision() { }
    public sealed record Granted(GrantSet Effective) : AuthDecision;
    public sealed record Attested(SignedAuthorship Authorship) : AuthDecision;
    public sealed record Authentic(Principal Actor, string SigningKeyId) : AuthDecision;
    public sealed record Unsigned(Principal Actor, OpDigest Digest, Instant At, CorrelationId Correlation) : AuthDecision;
    public sealed record Denied(Principal Principal, GrantSet Demand, UInt128 Scope) : AuthDecision;
    public sealed record ScopeMismatch(UInt128 Demanded, UInt128 Actual) : AuthDecision;
    public sealed record Expired(Principal Principal, Instant LapsedAt) : AuthDecision;
    public sealed record Unauthored(OpDigest Expected, OpDigest Found) : AuthDecision;
    public sealed record Forged(Principal Actor, string SigningKeyId) : AuthDecision;
    public sealed record DigestWidth(int Expected, int Actual) : AuthDecision;
    // The envelope verdicts: `Wrapped` carries the freshly-minted plaintext DEK (the caller binds the local cipher then
    // zeroizes it) plus the `WrappedKey` to persist; `Unwrapped` the recovered DEK; `KeyUnusable` the `Probe`-rejected
    // non-`Enabled` `KeyState` so a wrap against a disabled/destroy-scheduled key is a typed admission reject.
    public sealed record Wrapped(ReadOnlyMemory<byte> Dek, WrappedKey Key) : AuthDecision;
    public sealed record Unwrapped(ReadOnlyMemory<byte> Dek) : AuthDecision;
    public sealed record KeyUnusable(string WrappingKeyId, KeyState State) : AuthDecision;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Authority {
    public static GrantSet Effective(ObjectAcl acl, Principal principal, Seq<Principal> roles, Instant now) {
        var owned = acl.Owner == principal ? GrantSet.Owner : GrantSet.None;
        var direct = acl.Principals.Find(principal.Subject).Filter(e => e.Live(now));
        var inherited = acl.Inherited.Map(p => Effective(p, principal, roles, now)).IfNone(GrantSet.None);
        var roleAllow = roles.Fold(GrantSet.None, (acc, r) => acc.Union(acl.Roles.Find(r.Subject).Filter(e => e.Live(now)).Map(static e => e.Allow).IfNone(GrantSet.None)));
        var allow = owned.Union(inherited).Union(roleAllow).Union(direct.Map(static e => e.Allow).IfNone(GrantSet.None));
        var deny = direct.Map(static e => e.Deny).IfNone(GrantSet.None).Union(roles.Fold(GrantSet.None, (acc, r) => acc.Union(acl.Roles.Find(r.Subject).Filter(e => e.Live(now)).Map(static e => e.Deny).IfNone(GrantSet.None))));
        return allow.Without(deny);
    }

    static bool LapsedFor(ObjectAcl acl, Principal principal, Seq<Principal> roles, GrantSet demand, Instant now) =>
        acl.Principals.Find(principal.Subject).Exists(e => e.Lapsed(now) && e.Allow.Admits(demand))
        || roles.Exists(r => acl.Roles.Find(r.Subject).Exists(e => e.Lapsed(now) && e.Allow.Admits(demand)))
        || acl.Inherited.Exists(p => LapsedFor(p, principal, roles, demand, now));

    public static AuthDecision Admit(ObjectAcl acl, Principal principal, Seq<Principal> roles, GrantSet demand, UInt128 scope, Instant now) =>
        acl.Scope != scope || !acl.LadderValid ? new AuthDecision.ScopeMismatch(scope, acl.Scope)
        : Effective(acl, principal, roles, now) is var grant && grant.Admits(demand) ? new AuthDecision.Granted(grant)
        : LapsedFor(acl, principal, roles, demand, now) ? new AuthDecision.Expired(principal, now)
        : new AuthDecision.Denied(principal, demand, scope);

    public static IO<AuthDecision> Attest(Principal actor, OpDigest digest, KmsProvider provider, string signingKeyId, SigningKeyring keyring, ClockPolicy clocks, CorrelationId correlation) =>
        !provider.Signs ? IO.pure<AuthDecision>(new AuthDecision.Unsigned(actor, digest, clocks.Now, correlation))
        : digest.Fits(keyring.Algorithm) ? keyring.Sign(digest.Value).Map(sig => (AuthDecision)new AuthDecision.Attested(new SignedAuthorship(actor, provider, signingKeyId, keyring.Algorithm, digest, sig, clocks.Now, correlation)))
        : IO.pure<AuthDecision>(new AuthDecision.DigestWidth(keyring.Algorithm.DigestWidth, digest.Value.Length));

    public static IO<AuthDecision> Verify(SignedAuthorship authorship, OpDigest digest, SigningKeyring keyring) =>
        !authorship.Provider.Signs ? IO.pure<AuthDecision>(new AuthDecision.Unsigned(authorship.Actor, authorship.Digest, authorship.At, authorship.Correlation))
        : authorship.Digest != digest ? IO.pure<AuthDecision>(new AuthDecision.Unauthored(digest, authorship.Digest))
        : keyring.Verify(digest.Value, authorship.Signature).Map(valid => valid ? (AuthDecision)new AuthDecision.Authentic(authorship.Actor, authorship.SigningKeyId) : new AuthDecision.Forged(authorship.Actor, authorship.SigningKeyId));

    // The DEK envelope fold beside the signing fold (the `#KEY_ENVELOPE` surface): `Wrap` PROBES the key lifecycle first
    // so a wrap against a non-`Enabled` key rejects `KeyUnusable` at admission (never deep in the provider call), else
    // mints a fresh DEK through the provider arm (`NativeWrap` -> Azure `WrapKey`; encrypt-as-wrap -> AWS `GenerateDataKey`
    // / GCP `Encrypt`) returning `Wrapped` (the plaintext DEK for the local cipher bind + the `WrappedKey` to persist).
    public static IO<AuthDecision> Wrap(EnvelopeKeyring keyring, EnvelopeAad aad) =>
        from state in keyring.Probe()
        from decision in state.Usable
            ? keyring.Mint(aad).Map(static r => (AuthDecision)new AuthDecision.Wrapped(r.Dek, r.Wrapped))
            : IO.pure<AuthDecision>(new AuthDecision.KeyUnusable("<mint>", state))
        select decision;

    // `Unwrap` recovers the plaintext DEK bound to the persisted `WrappedKey` under the SAME `EnvelopeAad` (the provider
    // exact-match on AWS/GCP, the application-side compare on Azure) — the caller zeroizes the returned DEK through
    // `CryptographicOperations.ZeroMemory` after the local cipher bind, so no plaintext DEK survives the read path.
    public static IO<AuthDecision> Unwrap(EnvelopeKeyring keyring, WrappedKey wrapped, EnvelopeAad aad) =>
        keyring.Unwrap(wrapped, aad).Map(static dek => (AuthDecision)new AuthDecision.Unwrapped(dek));

    // `Rewrap` advances the wrapping-key version (AWS `ReEncrypt`, GCP `UpdateCryptoKeyPrimaryVersion` primary repoint,
    // Azure re-`WrapKey` against the new version) with the plaintext DEK never crossing the wire — gated on the same
    // `Probe` so a rotation onto a disabled target rejects `KeyUnusable`, returning `Wrapped` carrying the new `WrappedKey`
    // (no plaintext DEK, so the `Wrapped.Dek` is `ReadOnlyMemory<byte>.Empty` on the rewrap path).
    public static IO<AuthDecision> Rewrap(EnvelopeKeyring keyring, WrappedKey wrapped, EnvelopeAad aad) =>
        from state in keyring.Probe()
        from decision in state.Usable
            ? keyring.Rewrap(wrapped, aad).Map(static next => (AuthDecision)new AuthDecision.Wrapped(ReadOnlyMemory<byte>.Empty, next))
            : IO.pure<AuthDecision>(new AuthDecision.KeyUnusable(wrapped.WrappingKeyId, state))
        select decision;
}
```

## [05]-[SCHEMA_VERDICT]

- Owner: `SchemaVerdict` the `[Union]` boot verdict; `Placement` the `[SmartEnum<string>]` write-authority axis (the route-prescribed shape, declared here as the Persistence-Element owner); `IdentityFault` the `[Union]` identity-tier fault band (834x) deriving from the kernel `Rasm.Domain.Expected`; `SchemaGate` the static surface folding the Marten startup posture plus the EF migration identifier sets into one typed verdict so boot is a total fold, never a best-effort open.
- Cases: `SchemaVerdict` is `Serving` (model matches schema), `ServingBehind(Unknown)` (applied identifiers the compiled model does not know — schema newer than binary, admitted only under a declared expand-only invariant), `Provisioned(Applied)` (fresh store migrated), `Advanced(Applied)` (pending migrations applied), `AwaitBundle(Pending, Fresh)` (pending migrations a fleet member must not self-apply), `Drifted` (a model edit with neither migration nor regeneration); `IdentityFault` is `SchemaAhead(Unknown)` (EF identifiers the binary lacks), `ApplyFailed(Detail)` (an EF `Migrate` or a Marten apply throw), `MartenMismatch(Detail)` (the host-startup Marten assertion throw lifted onto the band), `CellUnresolvable(Detail)` (an H3 centroid that yields the invalid sentinel) — band 834x, the `Element/graph#GRAPH_PROJECTION` `GraphFault` (8300) and the `Store/blobstore#OBJECT_STORE` `RemoteStoreFault` (5401) being the federation neighbors a telemetry reader bands by code.
- Entry: `public static Fin<SchemaVerdict> Admit(DbContext store, Placement placement)` folds the assembly and applied EF migration sets into the EF half of the verdict; `public static IO<SchemaVerdict> AdmitMarten(IMartenStorage storage, Placement placement)` is the single-writer Marten apply leg over `ApplyAllConfiguredChangesToDatabaseAsync`, the fleet member's Marten posture being the host-registered `AssertDatabaseMatchesConfigurationOnStartup` gate whose throw lifts to `IdentityFault.MartenMismatch` — two legs, one band.
- Packages: Marten (the host-builder `ApplyAllDatabaseChangesOnStartup`/`AssertDatabaseMatchesConfigurationOnStartup` registrations + the runtime `IMartenStorage.ApplyAllConfiguredChangesToDatabaseAsync(AutoCreate?)`), Microsoft.EntityFrameworkCore (`GetMigrations`/`GetAppliedMigrations`/`Migrate`/`HasPendingModelChanges`), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new boot outcome is one `SchemaVerdict` case; a new identity-tier failure is one `IdentityFault` case; zero new surface — a best-effort open, a per-process bootstrap branch, a bare `Error.New`, or apply-time gating is the deleted form because boot is one total fold, the failures are one typed band, and the destructive change is gated at generation time.
- Boundary: TWO DDL owners compose at boot — Marten owns its event/document DDL and the EF identity model owns its relational DDL — and each owner's posture is selected by the SAME `Placement` write authority the route-owned migration algebra defines: the single-writer placement APPLIES (EF `store.Database.Migrate()`, Marten `ApplyAllConfiguredChangesToDatabaseAsync` and the host `ApplyAllDatabaseChangesOnStartup` registration) while every fleet member ASSERTS (the host `AssertDatabaseMatchesConfigurationOnStartup` registration that throws on a Marten skew, lifted to `IdentityFault.MartenMismatch`) and never applies; the Marten startup assertion is a host-REGISTERED boot gate (the catalogued DI method, not a runtime re-probe `SchemaGate.Admit` invents), so a Marten document-schema skew fails the host start before `Admit` runs and the EF half of the verdict carries the relational migration classification; in the EF fold an applied-minus-assembly non-empty set is a typed `IdentityFault.SchemaAhead` (or `ServingBehind` under a read-ahead placement) carrying the unknown identifiers, never a silent open that corrupts on first write; this verdict is the Element-identity-tier boot fold and the route-owned `docs/stacks/csharp/domain/persistence#MIGRATION_ALGEBRA` `SchemaGate`/`Placement`/`SchemaVerdict` is the general EF FORM this page realizes (the route carries no concrete owner, only the prescribed shape) and extends with the Marten apply/assert leg the route form does not carry, never re-teaching the EF mechanics; `EnsureCreated` bypasses the history mechanism and is admitted only for the ephemeral test row; read-ahead serving (`ServingBehind`) is legal only under a declared expand-only suite invariant, so the sound default is hard rejection.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------
// The write-authority axis the route-owned `docs/stacks/csharp/domain/persistence#MIGRATION_ALGEBRA` prescribes,
// declared here as the Persistence-Element owner (no concrete route type exists to compose): single-writer applies
// both DDL owners, the fleet member asserts-only, the reader serves-behind. A `bool writesPending` flag is the
// rejected form — the three postures are one vocabulary, not two booleans.
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
// The identity-tier fault band (834x): a closed [Union] over the KERNEL `Rasm.Domain.Expected` (parameterless
// protected ctor; `Category` virtual; `Code`/`Message` inherited from `Error`), the SAME federation base the seam
// `Rasm.Element/Projection/fault#FAULT_BAND` `ElementFault` (2500) and the `Rasm.Bim/Model/faults#FAULT_BAND`
// `BimFault` (2600) realize — NOT `LanguageExt.Common.Expected`, whose `(string,int,Option)` `base(detail, code, None)`
// ctor (no `Category` to override) is the deleted form. Band membership is a per-case `Code => 834x` override,
// `Message => Detail` projects the case detail, and `Category` projects the telemetry label through the generated
// `Switch`, so the typed case lifts BARE onto `Fin<T>`/`Validation<Error,T>` with no `.ToError()` hop and a recovery
// reads `error.IsType<IdentityFault.CellUnresolvable>()` / `error.HasCode(8344)` / `error.Category()`, never a message
// substring; a bare `Error.New(8341, …)` at a call site is the deleted form. No `[GenerateUnionOps]` — the kernel
// union-ops generator is strictly opt-in, so the band carries no generated per-case `SelfOp`; the `[Union]`-generated
// `Switch`/`Map` is untouched. `Create` is the IValidationError admission the generated converter bridge calls
// on a deserialization reject.
[Union]
public abstract partial record IdentityFault : Expected, IValidationError<IdentityFault> {
    private IdentityFault() : base() { }

    public sealed record SchemaAhead(Seq<string> Unknown) : IdentityFault;
    public sealed record ApplyFailed(string Detail) : IdentityFault;
    public sealed record MartenMismatch(string Detail) : IdentityFault;
    public sealed record CellUnresolvable(string Detail) : IdentityFault;

    public override int Code => Switch(
        schemaAhead:      static _ => 8341,
        applyFailed:      static _ => 8342,
        martenMismatch:   static _ => 8343,
        cellUnresolvable: static _ => 8344);

    public override string Message => Switch(
        schemaAhead:      static c => $"<schema-ahead:{c.Unknown.Count}>",
        applyFailed:      static c => $"<apply-failed:{c.Detail}>",
        martenMismatch:   static c => $"<marten-mismatch:{c.Detail}>",
        cellUnresolvable: static c => $"<cell-unresolvable:{c.Detail}>");

    public override string Category => Switch(
        schemaAhead:      static _ => "Schema",
        applyFailed:      static _ => "Apply",
        martenMismatch:   static _ => "Marten",
        cellUnresolvable: static _ => "Cell");

    public static IdentityFault Create(string message) => new MartenMismatch(message);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SchemaGate {
    // The EF half of the boot fold over the relational identity DDL: the assembly-vs-applied migration sets classified
    // by the route-owned `Placement` write authority (`docs/stacks/csharp/domain/persistence#MIGRATION_ALGEBRA`, reused
    // verbatim, never a divergent bool flag) — schema-ahead is a typed rejection, the single-writer applies, the fleet
    // member AwaitBundle. Every failure is the typed IdentityFault band, never a bare Error.New.
    public static Fin<SchemaVerdict> Admit(DbContext store, Placement placement) {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(placement);
        var assembly = toSeq(store.Database.GetMigrations());
        var applied = toSeq(store.Database.GetAppliedMigrations());
        var unknown = applied.Filter(id => !assembly.Exists(held => held == id));
        var pending = assembly.Filter(id => !applied.Exists(held => held == id));
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
    // `IMartenStorage.ApplyAllConfiguredChangesToDatabaseAsync` (the fleet member instead carries the host-registered
    // `AssertDatabaseMatchesConfigurationOnStartup` gate whose throw lifts to MartenMismatch BEFORE this runs, so a
    // non-applying placement asserts-only). A reader/fleet member returns Serving without touching DDL; an apply throw
    // lifts to ApplyFailed, the SAME band the EF leg uses.
    public static IO<SchemaVerdict> AdmitMarten(IMartenStorage storage, Placement placement) =>
        placement.AppliesPending
            ? IO.liftAsync(async () => { await storage.ApplyAllConfiguredChangesToDatabaseAsync().ConfigureAwait(false); return (SchemaVerdict)new SchemaVerdict.Serving(); })
                | @catch<IO, SchemaVerdict>(static _ => true, error => IO.fail<SchemaVerdict>(new IdentityFault.ApplyFailed(error.Message)))
            : IO.pure<SchemaVerdict>(new SchemaVerdict.Serving());
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | Marten DDL          | `AutoCreate.CreateOrUpdate` (writer)   | `AssertDatabaseMatchesConfigurationOnStartup` on the fleet; the assertion is a folded fact |
|  [02]   | EF identity DDL     | generated migrations                   | the relational projection over the Marten-written rows    |
|  [03]   | boot verdict        | one `SchemaGate.Admit` fold (Marten + EF) | schema-ahead is a typed `IdentityFault`, never a silent open |
|  [04]   | pending apply       | single-writer placement only           | fleet member `AwaitBundle`, never self-applies            |
