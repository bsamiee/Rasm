# [PERSISTENCE_ELEMENT_IDENTITY]

Rasm.Persistence anchors every persisted `ElementGraph` to one relational identity tier that commits ATOMICALLY with the Marten event in the same `IDocumentSession`: `ElementIdentity` is the per-model document/row carrying the `ModelId` PK, the `TenantId` RLS column, the set of rooted `NodeId`s, the Bim-projected IFC GlobalId strings (each rooted node's seam `Node.Object.ExternalId`), the H3 spatial cell, the pgvector embedding reference, the `ObjectAcl`, and the classification — so identity plus event are one transaction with no two-ORM gap and the relational columns serve the spatial/vector/ACL/tenant lanes off the one tier. `IdentityPolicy` is the `[SmartEnum<string>]` key axis dispatching mint and decode per row, big-endian transcription preserving order, so an identity change is an expand-wave second key plus a derivation flip plus a contract-wave drop, never an `AlterColumn`. `Capability` is the one `[Flags]` authorization vocabulary every grant draws from — object, audit, and branch permissions in one bit field whose `Owner` superset is value-derived and scoped by `AclScope` — so the `Version/commits#COMMIT_DAG` branch grant is the same vocabulary narrowed, never a parallel taxonomy. `SignedAuthorship` is the KMS-signed actor attestation tying a delta to a verified blame agent, and `Authority` folds object admission and authorship verification into one `AuthDecision`. The boot verdict folds two identifier sets into one typed `SchemaVerdict` — Marten `AutoCreate` plus the EF migration state — so a store whose schema is newer than the compiled model is a typed rejection, never a best-effort open. `ModelId`/`NodeId`/`ContentAddress` arrive settled from `Rasm.Element` (the IFC GlobalId is the seam `Node.Object.ExternalId` string the relational tier projects, never a separate seam type); `ClockPolicy`/`TenantContext`/`CorrelationId`/`SecretLease` arrive from `Rasm.AppHost`; `KmsProvider` is the Persistence-side KMS provider axis the `SigningKeyring` and the `Store/blobstore#BLOB_GC` SSE envelope resolve — only the `SecretLease`-class KMS-unwrap handle crosses from AppHost, never the provider axis itself.

## [01]-[INDEX]

- [01]-[ELEMENT_IDENTITY]: the relational identity tier, the co-transactional Marten-document commit, and the spatial/vector/tenant join columns.
- [02]-[IDENTITY_POLICY]: the key axis, big-endian transcription, per-row mint/decode, and content addressing.
- [03]-[AUTHORITY]: the unified capability vocabulary, object allow/deny ACL with inheritance, KMS-signed authorship, and the one admission/verification fold.
- [04]-[SCHEMA_VERDICT]: the boot fold over the Marten `AutoCreate` plus EF migration state into one typed verdict.

## [02]-[ELEMENT_IDENTITY]

- Owner: `ElementIdentity` the per-model identity row carrying the `ModelId` PK plus the `TenantId`/`Roots`/`GlobalIds`/`Cell`/`Embedding`/`Acl`/`Classification` join columns; `IdentityShape` the `IEntityTypeConfiguration<ElementIdentity>` mapping EVERY persistent member — the `Tenant` RLS column, the H3 `bigint`, the pgvector column, the jsonb ACL, the `Roots` `text[]` and `GlobalIds` jsonb (value-converted through the canonical `ElementJson` codec, never a partial subset that drops them on write), the `Classification` key, and the `At` instant; `IdentityStore` the static surface owning the co-transactional Marten-document store and the spatial/vector index projections.
- Cases: `Roots` is the set of rooted `NodeId`s the model owns (the `IfcRoot` mirror nodes), `GlobalIds` the 1:1 map from rooted `NodeId` to the compressed IFC GlobalId string projected from each seam `Node.Object.ExternalId` (`H6` — the rooted `NodeId` is the neutral kernel-minted durable key, the IFC GlobalId is the `ExternalId` projection the `Version/merge#STRUCTURAL_DIFF` re-ingest `Reconcile` correlates on, never the key), `Cell` the Uber-H3 spatial index cell over the model envelope centroid, `Embedding` the optional pgvector reference keying the ANN lane — the per-model envelope locator, distinct in grain from the corpus-grain retrieval index and the `ProductCodebook` PQ vocabulary the `Query/lane#VECTOR_CODEBOOK` owner trains, never a duplicate index, `Acl` the `ObjectAcl` grant, `Classification` the `DataClassification` ceiling.
- Entry: `public static IDocumentSession Stamp(IDocumentSession session, ElementIdentity identity)` stores the identity as a Marten document in the SAME session the event appends to so `SaveChangesAsync` commits both atomically; `public static H3Index Cell(Envelope bounds, int resolution)` projects the model envelope centroid to its H3 cell through `pocketken.H3` so the cell id is computed identically at ingest and in the `h3-pg` server extension; `public static IQueryable<ElementIdentity> Nearby(DbContext db, H3Index cell, int ring)` resolves the spatial neighborhood through the H3 ring.
- Auto: the identity tier is a Marten document so it rides the one `IDocumentSession` the `Element/graph#STORE_RAIL` write op uses — `session.Store(identity)` then `SaveChangesAsync` commits the identity row, the appended `GraphEvent`, and the inline projection in one Postgres transaction (`H10` — pick ONE transaction owner for identity plus event, the Marten document in the same session, no free two-ORM atomicity); the relational columns are also EF-mapped through `IdentityShape` for the Npgsql/pgvector/H3 query lanes that Marten's LINQ does not reach (the GiST H3 neighborhood, the pgvector ANN), reading the same rows Marten wrote; the `Tenant` column is the `current_setting('rasm.tenant')::uuid` RLS partition every read filters on by construction.
- Receipt: an identity stamp rides `store.element.identity` carrying the `Roots` count; a spatial-neighborhood read rides `store.identity.nearby` carrying the ring radius.
- Packages: Marten (`IDocumentSession.Store`), Npgsql.EntityFrameworkCore.PostgreSQL, Pgvector.EntityFrameworkCore, pocketken.H3, Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime.
- Growth: a new identity join column is one field on `ElementIdentity` plus one `IdentityShape` mapping; a new spatial resolution is one H3 cell policy; zero new surface — a separate identity transaction, a second identity ORM committing apart from the event, a parallel `NodeId`-keyed identity table, or an EF-versus-Marten atomicity dance is the deleted form because the identity is a Marten document in the one session and EF reads the same rows for the query lanes only.
- Boundary: the ONE transaction owner for identity plus event is the `IDocumentSession` — the `ElementIdentity` row commits as a Marten document in the same session as the appended events so a single `SaveChangesAsync` is the atomic boundary; EF/Npgsql is the READ projection over the same rows for the H3/pgvector/GiST lanes Marten's LINQ cannot serve, never a second write authority (a `DbContext.SaveChanges` over the identity table beside the Marten append is the deleted two-ORM gap); the rooted `NodeId` is the neutral kernel-minted DURABLE key and the IFC GlobalId is each node's seam `Node.Object.ExternalId` projection (`H6`) the `GlobalIds` map mirrors; a re-import mints fresh neutral `NodeId`s, and the `Version/merge#STRUCTURAL_DIFF` `Reconcile` correlates the re-ingest's rooted nodes on the stable `ExternalId` GlobalId and aligns them back to the durable keys, so the durable `NodeId` and every foreign reference survive re-import unchanged while the projection map mirrors the current `NodeId`↔GlobalId pairing and the kernel owns the whole `IObjectFactory` floor; the H3 `Cell` is computed by the managed `pocketken.H3` identically to the `h3-pg` server extension so the same cell id indexes at ingest and in SQL, deleting a second spatial index scheme; the `Tenant` RLS column is the coarse partition and the `ObjectAcl` (`#AUTHORITY`) is the fine within-tenant grant, two altitudes never duplicated.

```csharp signature
// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<long>]
public readonly partial struct H3Index;

public sealed record ElementIdentity(
    ModelId Model,
    UInt128 Tenant,
    Seq<NodeId> Roots,
    HashMap<NodeId, string> GlobalIds,
    H3Index Cell,
    Option<Pgvector.Vector> Embedding,
    ObjectAcl Acl,
    DataClassification Classification,
    Instant At) {
    public int NodeCount => Roots.Count;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class IdentityShape : IEntityTypeConfiguration<ElementIdentity> {
    public void Configure(EntityTypeBuilder<ElementIdentity> identity) {
        ArgumentNullException.ThrowIfNull(identity);
        identity.ToTable("element_identity");
        identity.HasKey(static e => e.Model);
        identity.Property(static e => e.Tenant).HasColumnName("tenant");
        identity.Property(static e => e.Cell).HasColumnName("h3_cell");
        identity.Property(static e => e.Embedding).HasColumnType("vector(1536)");
        identity.Property(static e => e.Classification).HasColumnName("classification");
        identity.Property(static e => e.At).HasColumnName("at");
        identity.Property(static e => e.Roots).HasColumnName("roots")
            .HasConversion(
                new ValueConverter<Seq<NodeId>, string[]>(
                    static r => r.Map(static n => n.Value).ToArray(),
                    static a => toSeq(a).Map(NodeId.Create)),
                new ValueComparer<Seq<NodeId>>(static (x, y) => x == y, static v => v.GetHashCode(), static v => v));
        identity.Property(static e => e.GlobalIds).HasColumnName("global_ids").HasColumnType("jsonb")
            .HasConversion(
                new ValueConverter<HashMap<NodeId, string>, string>(
                    static m => JsonSerializer.Serialize(m.ToDictionary(static p => p.Key.Value, static p => p.Value), ElementJson.Options),
                    static s => toHashMap((JsonSerializer.Deserialize<Dictionary<string, string>>(s, ElementJson.Options) ?? []).Select(static kv => (NodeId.Create(kv.Key), kv.Value)))),
                new ValueComparer<HashMap<NodeId, string>>(static (x, y) => x == y, static v => v.GetHashCode(), static v => v));
        identity.ComplexProperty(static e => e.Acl, static acl => acl.ToJson("acl"));
        identity.HasIndex(static e => e.Cell);
        identity.HasIndex(static e => e.Tenant);
    }
}

public static class IdentityStore {
    public static IDocumentSession Stamp(IDocumentSession session, ElementIdentity identity) {
        ArgumentNullException.ThrowIfNull(session);
        session.Store(identity);
        return session;
    }

    public static H3Index Cell(Envelope bounds, int resolution) =>
        H3Index.Create((long)(ulong)pocketken.H3.Api.LatLngToCell(
            new pocketken.H3.LatLng(bounds.Centre.Y, bounds.Centre.X), resolution));

    public static IQueryable<ElementIdentity> Nearby(DbContext db, H3Index cell, int ring) {
        ArgumentNullException.ThrowIfNull(db);
        var disk = toSeq(pocketken.H3.Api.GridDisk((ulong)cell.Value, ring)).Map(static c => H3Index.Create((long)c)).ToArray();
        return db.Set<ElementIdentity>().Where(e => disk.Contains(e.Cell));
    }
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | one txn owner       | identity as Marten doc in one session  | `session.Store` + `SaveChangesAsync`; no two-ORM gap      |
|  [02]   | read projection     | EF/Npgsql over the same rows           | H3/pgvector/GiST lanes Marten LINQ cannot serve           |
|  [03]   | rooted key          | neutral kernel-minted durable `NodeId` | IFC GlobalId is the `Node.Object.ExternalId` the `GlobalIds` map mirrors; re-ingest correlates on it |
|  [04]   | spatial cell        | `pocketken.H3` = `h3-pg`               | one cell id at ingest and in SQL                          |
|  [05]   | tenant partition    | `Tenant` RLS column                    | coarse scope; `ObjectAcl` is the fine within-tenant grant |

## [03]-[IDENTITY_POLICY]

- Owner: `IdentityPolicy` the `[SmartEnum<string>]` five-row key axis carrying generator, big-endian transcription, ordering, collision class, and CLR type, dispatching `Mint` per row through one generated `Switch`; `StoreKey` the `[Union]` closed key carrier (`Surrogate`/`Content`/`Natural`); `Collision` the collision-posture vocabulary.
- Cases: `uuid-v7` (default, B-tree insert-local, `Guid.CreateVersion7`), `uuid-v7-backfill` (historical-timestamp mint for deterministic backfill), `content-hash` (immutable-payload `XxHash128` addressing — the `ContentAddress` row), `natural-key` (caller-owned identifier passthrough), `namespace-key` (RFC-4122 v5 over a namespace and a name for stable derived ids); `Collision` rows are `unmintable`, `content-idempotent`, `foreign-authority`, `derived-deterministic`.
- Entry: `public StoreKey Mint(ReadOnlyMemory<byte> material, Instant observed)` is the per-row factory dispatching through the generated `Switch`; `public StoreKey Decode(ReadOnlySpan<byte> spelled)` is the per-row inverse; `StoreKey.Spelled` is the ordering-preserving big-endian transcription; `StoreKey.ObservedAt` projects a v7 key's embedded creation time.
- Packages: Thinktecture.Runtime.Extensions, System.IO.Hashing, LanguageExt.Core, NodaTime, BCL inbox.
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
        Natural n => Encoding.UTF8.GetBytes(n.Value),
    };

    static byte[] SpelledContent(UInt128 value) { var b = new byte[16]; BinaryPrimitives.WriteUInt128BigEndian(b, value); return b; }

    public Option<Instant> ObservedAt() =>
        this is Surrogate { Value.Version: 7 } s ? Some(Instant.FromUnixTimeMilliseconds(UnixMillis(s.Value))) : None;

    static long UnixMillis(Guid key) {
        Span<byte> b = stackalloc byte[16];
        _ = key.TryWriteBytes(b, bigEndian: true, out _);
        return ((long)BinaryPrimitives.ReadUInt16BigEndian(b) << 32) | BinaryPrimitives.ReadUInt32BigEndian(b[2..]);
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
        contentHash: static s => new StoreKey.Content(XxHash128.HashToUInt128(s.Material.Span)),
        naturalKey: static s => new StoreKey.Natural(Encoding.UTF8.GetString(s.Material.Span)),
        namespaceKey: static s => new StoreKey.Surrogate(NamespaceUuid(Namespace, s.Material.Span)));

    public StoreKey Decode(ReadOnlySpan<byte> spelled) => Switch<byte[], StoreKey>(
        state: spelled.ToArray(),
        uuidV7Key: static b => new StoreKey.Surrogate(new Guid(b.AsSpan()[..16], bigEndian: true)),
        uuidV7Backfill: static b => new StoreKey.Surrogate(new Guid(b.AsSpan()[..16], bigEndian: true)),
        contentHash: static b => new StoreKey.Content(BinaryPrimitives.ReadUInt128BigEndian(b)),
        naturalKey: static b => new StoreKey.Natural(Encoding.UTF8.GetString(b)),
        namespaceKey: static b => new StoreKey.Surrogate(new Guid(b.AsSpan()[..16], bigEndian: true)));

    static Guid NamespaceUuid(Guid ns, ReadOnlySpan<byte> name) {
        Span<byte> nsBytes = stackalloc byte[16];
        _ = ns.TryWriteBytes(nsBytes, bigEndian: true, out _);
        using var sha = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
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

- Owner: `Capability` the one `[Flags]` authorization vocabulary with a value-derived `Owner` superset and an `Admits(demand)` superuser-aware containment operator; `AclScope` the gated-object-kind vocabulary carrying its altitude `Parent`; `AclEntry` the per-principal allow/deny grant with provenance and a `[From, Until)` window; `ObjectAcl` the owner-plus-inherited grant set; `SigningAlgorithm` the algorithm axis carrying its `HashAlgorithmName` and provider `WireName`; `OpDigest` the `[ValueObject<byte[]>]` cryptographic op-hash; `SigningKeyring` the provider-neutral `Sign`/`Verify` delegate pair; `SignedAuthorship` the KMS-signed actor attestation; `AuthDecision` the closed admission/verification verdict; `Authority` the one fold.
- Cases: `Capability` grants `Read..Revoke` (object), `Audit` (the blame/audit-log lane), `Merge..ForcePush` (branch), `Admin` (the explicit superuser bit), and `Owner` (the value-derived union); `AclScope` is `document → branch → tenant` with `element-set` nesting under `document`; `SigningAlgorithm` carries one row per JWS algorithm × digest width the providers intersect (es/rs/ps × 256/384/512); `AuthDecision` is `Granted | Attested | Authentic | Unsigned | Denied | ScopeMismatch | Expired | Unauthored | Forged | DigestWidth` — `Unsigned` the local-tier authorship verdict on `KmsProvider.None`.
- Entry: `public static AuthDecision Admit(ObjectAcl acl, Principal principal, Seq<Principal> roles, Capability demand, UInt128 scope, Instant now)` folds owner, direct, role, and inherited grants into one effective `Capability` with deny-over-allow; `public static IO<AuthDecision> Attest(...)` signs an `OpDigest` through the KMS keyring after gating its width; `public static IO<AuthDecision> Verify(...)` checks the digest binding and signature.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, AWSSDK.KeyManagementService, Azure.Security.KeyVault.Keys, Google.Cloud.Kms.V1, System.Security.Cryptography (BCL inbox).
- Growth: one `Capability` flag per new permission; one `AclScope` row per new gated kind; one `SigningAlgorithm` row per JWS family; one `AuthDecision` case per verdict; zero new surface.
- Boundary: `Capability` is the one authorization vocabulary — the `Version/commits#COMMIT_DAG` `BranchRef` grant is this same bit field narrowed to the branch lane under `AclScope.Branch`, so a second branch-only enum is the deleted form and the audit-log read is the `Audit` lane; superuser is value-derived (`grant.Admits(demand)` is true when the grant carries `Admin`/`Owner` regardless of lane bits, so an `Admin`-only owner is admitted `Read`); object authorization is the per-object/per-branch allow/deny grant with deny-over-allow so an explicit `AclEntry` deny overrides every inherited allow, the `Owner` principal carries `Capability.Owner` so the creator never locks itself out, and the `[From, Until)` window time-boxes both ends (a future grant stays `Denied`, a lapsed one folds `Expired`); inheritance is the `AclScope.Parent` altitude ladder with the `child.Kind.Parent == Some(parent.Kind)` invariant so a mis-stacked chain rejects; the `#ELEMENT_IDENTITY` `Tenant` RLS column is the coarse scope and this object-ACL the fine, two altitudes never duplicated; signed authorship is the actor-to-blame seam — a cloud-KMS op carries a `SignedAuthorship` over a `SigningAlgorithm`-width cryptographic `OpDigest` so a blame attribution (`Version/timetravel`, `Version/provenance`) names a verified actor, a 16-byte non-cryptographic content hash standing in for the signed digest being the deleted form, and `KmsProvider.None` is the one unsigned path projecting `Unsigned` so a store with no KMS still records the delta→actor binding; the `SigningKeyring` resolves the asymmetric key through the same AppHost `SecretLease`-class handle the `Store/blobstore#BLOB_GC` SSE envelope consumes, never a bare passphrase, and the provider-specific algorithm type lives only at the keyring delegate edge.

```csharp signature
[Flags]
public enum Capability {
    None = 0, Read = 1, Write = 2, Delete = 4, Share = 8, Revoke = 16, Audit = 32,
    Merge = 64, Rebase = 128, ForcePush = 256, Admin = 512,
    Owner = Read | Write | Delete | Share | Revoke | Audit | Merge | Rebase | ForcePush | Admin,
}

public static class CapabilityLaw {
    extension(Capability grant) {
        public bool Admits(Capability demand) => (grant & Capability.Admin) == Capability.Admin || (grant & demand) == demand;
    }
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

public readonly record struct Principal(string Id) { public static Principal Of(string id) => new(id); }

public sealed record AclEntry(Capability Allow, Capability Deny, Principal GrantedBy, Instant At, Option<Instant> From, Option<Instant> Until) {
    public bool Live(Instant now) => From.Match(Some: f => now >= f, None: static () => true) && Until.Match(Some: u => now < u, None: static () => true);
    public bool Lapsed(Instant now) => Until.Match(Some: u => now >= u, None: static () => false);
}

public sealed record ObjectAcl(UInt128 Scope, AclScope Kind, Principal Owner, HashMap<string, AclEntry> Principals, HashMap<string, AclEntry> Roles, Option<ObjectAcl> Inherited) {
    public bool LadderValid => Inherited.Match(Some: p => Kind.Parent == Some(p.Kind) && p.LadderValid, None: static () => true);
}

[SmartEnum<string>]
public sealed partial class SigningAlgorithm {
    public static readonly SigningAlgorithm Es256 = new("es256", HashAlgorithmName.SHA256, "ECDSA_SHA_256");
    public static readonly SigningAlgorithm Es384 = new("es384", HashAlgorithmName.SHA384, "ECDSA_SHA_384");
    public static readonly SigningAlgorithm Es512 = new("es512", HashAlgorithmName.SHA512, "ECDSA_SHA_512");
    public static readonly SigningAlgorithm Ps256 = new("ps256", HashAlgorithmName.SHA256, "RSASSA_PSS_SHA_256");
    public static readonly SigningAlgorithm Ps384 = new("ps384", HashAlgorithmName.SHA384, "RSASSA_PSS_SHA_384");
    public static readonly SigningAlgorithm Ps512 = new("ps512", HashAlgorithmName.SHA512, "RSASSA_PSS_SHA_512");
    public HashAlgorithmName Hasher { get; }
    public string WireName { get; }
    public int DigestWidth => Hasher == HashAlgorithmName.SHA512 ? 64 : Hasher == HashAlgorithmName.SHA384 ? 48 : 32;
    private SigningAlgorithm(string key, HashAlgorithmName hasher, string wireName) : this(key) => (Hasher, WireName) = (hasher, wireName);
    public OpDigest Hash(ReadOnlySpan<byte> opBytes) { Span<byte> d = stackalloc byte[64]; var w = CryptographicOperations.HashData(Hasher, opBytes, d); return OpDigest.Create(d[..w].ToArray()); }
}

[ValueObject<byte[]>(EqualityComparisonOperators = OperatorsGeneration.Default)]
public readonly partial struct OpDigest {
    private static Validation<ValidationError, byte[]> ValidateFactoryArguments(byte[] value) =>
        value is { Length: 32 or 48 or 64 } ? Validation<ValidationError, byte[]>.Success(value) : Validation<ValidationError, byte[]>.Fail(new ValidationError($"<op-digest-width:{value.Length}>"));
    public bool Fits(SigningAlgorithm algorithm) => Value.Length == algorithm.DigestWidth;
}

public sealed record SigningKeyring(SigningAlgorithm Algorithm, Func<ReadOnlyMemory<byte>, IO<ReadOnlyMemory<byte>>> Sign, Func<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>, IO<bool>> Verify);
public sealed record SignedAuthorship(Principal Actor, KmsProvider Provider, string SigningKeyId, SigningAlgorithm Algorithm, OpDigest Digest, ReadOnlyMemory<byte> Signature, Instant At, CorrelationId Correlation);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AuthDecision {
    private AuthDecision() { }
    public sealed record Granted(Capability Effective) : AuthDecision;
    public sealed record Attested(SignedAuthorship Authorship) : AuthDecision;
    public sealed record Authentic(Principal Actor, string SigningKeyId) : AuthDecision;
    public sealed record Unsigned(Principal Actor, OpDigest Digest, Instant At, CorrelationId Correlation) : AuthDecision;
    public sealed record Denied(Principal Principal, Capability Demand, UInt128 Scope) : AuthDecision;
    public sealed record ScopeMismatch(UInt128 Demanded, UInt128 Actual) : AuthDecision;
    public sealed record Expired(Principal Principal, Instant LapsedAt) : AuthDecision;
    public sealed record Unauthored(OpDigest Expected, OpDigest Found) : AuthDecision;
    public sealed record Forged(Principal Actor, string SigningKeyId) : AuthDecision;
    public sealed record DigestWidth(int Expected, int Actual) : AuthDecision;
}

public static class Authority {
    public static Capability Effective(ObjectAcl acl, Principal principal, Seq<Principal> roles, Instant now) {
        var owned = acl.Owner == principal ? Capability.Owner : Capability.None;
        var direct = acl.Principals.Find(principal.Id).Filter(e => e.Live(now));
        var inherited = acl.Inherited.Map(p => Effective(p, principal, roles, now)).IfNone(Capability.None);
        var roleAllow = roles.Fold(Capability.None, (acc, r) => acc | acl.Roles.Find(r.Id).Filter(e => e.Live(now)).Map(static e => e.Allow).IfNone(Capability.None));
        var allow = owned | inherited | roleAllow | direct.Map(static e => e.Allow).IfNone(Capability.None);
        var deny = direct.Map(static e => e.Deny).IfNone(Capability.None) | roles.Fold(Capability.None, (acc, r) => acc | acl.Roles.Find(r.Id).Filter(e => e.Live(now)).Map(static e => e.Deny).IfNone(Capability.None));
        return allow & ~deny;
    }

    static bool LapsedFor(ObjectAcl acl, Principal principal, Seq<Principal> roles, Capability demand, Instant now) =>
        acl.Principals.Find(principal.Id).Exists(e => e.Lapsed(now) && e.Allow.Admits(demand))
        || roles.Exists(r => acl.Roles.Find(r.Id).Exists(e => e.Lapsed(now) && e.Allow.Admits(demand)))
        || acl.Inherited.Exists(p => LapsedFor(p, principal, roles, demand, now));

    public static AuthDecision Admit(ObjectAcl acl, Principal principal, Seq<Principal> roles, Capability demand, UInt128 scope, Instant now) =>
        acl.Scope != scope || !acl.LadderValid ? new AuthDecision.ScopeMismatch(scope, acl.Scope)
        : Effective(acl, principal, roles, now) is var grant && grant.Admits(demand) ? new AuthDecision.Granted(grant)
        : LapsedFor(acl, principal, roles, demand, now) ? new AuthDecision.Expired(principal, now)
        : new AuthDecision.Denied(principal, demand, scope);

    public static IO<AuthDecision> Attest(Principal actor, OpDigest digest, KmsProvider provider, string signingKeyId, SigningKeyring keyring, ClockPolicy clocks, CorrelationId correlation) =>
        provider == KmsProvider.None ? IO.pure<AuthDecision>(new AuthDecision.Unsigned(actor, digest, clocks.Now, correlation))
        : digest.Fits(keyring.Algorithm) ? keyring.Sign(digest.Value).Map(sig => (AuthDecision)new AuthDecision.Attested(new SignedAuthorship(actor, provider, signingKeyId, keyring.Algorithm, digest, sig, clocks.Now, correlation)))
        : IO.pure<AuthDecision>(new AuthDecision.DigestWidth(keyring.Algorithm.DigestWidth, digest.Value.Length));

    public static IO<AuthDecision> Verify(SignedAuthorship authorship, OpDigest digest, SigningKeyring keyring) =>
        authorship.Provider == KmsProvider.None ? IO.pure<AuthDecision>(new AuthDecision.Unsigned(authorship.Actor, authorship.Digest, authorship.At, authorship.Correlation))
        : authorship.Digest != digest ? IO.pure<AuthDecision>(new AuthDecision.Unauthored(digest, authorship.Digest))
        : keyring.Verify(digest.Value, authorship.Signature).Map(valid => valid ? (AuthDecision)new AuthDecision.Authentic(authorship.Actor, authorship.SigningKeyId) : new AuthDecision.Forged(authorship.Actor, authorship.SigningKeyId));
}
```

## [05]-[SCHEMA_VERDICT]

- Owner: `SchemaVerdict` the `[Union]` boot verdict; `SchemaGate` the static surface folding the Marten `AutoCreate` posture plus the EF migration identifier sets into one typed verdict so boot is a total fold, never a best-effort open.
- Cases: `Serving` (model matches schema), `ServingBehind(Unknown)` (applied identifiers the compiled model does not know — schema newer than binary, admitted only under a declared expand-only invariant), `Provisioned(Applied)` (fresh store migrated), `Advanced(Applied)` (pending migrations applied), `AwaitBundle(Pending, Fresh)` (pending migrations a fleet member must not self-apply), `Drifted` (a model edit with neither migration nor regeneration).
- Entry: `public static Fin<SchemaVerdict> Admit(DbContext store, bool writesPending)` folds the assembly and applied EF migration sets plus the Marten `AssertDatabaseMatchesConfigurationOnStartup` posture into one verdict; the write-authority placement (single-writer applies, fleet member awaits a bundle) routes the pending arm.
- Packages: Marten (`StoreOptions.AutoCreateSchemaObjects`/`ApplyAllDatabaseChangesOnStartup`/`AssertDatabaseMatchesConfigurationOnStartup`), Microsoft.EntityFrameworkCore, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new boot outcome is one `SchemaVerdict` case; zero new surface — a best-effort open, a per-process bootstrap branch, or apply-time gating is the deleted form because boot is one total fold and the destructive change is gated at generation time.
- Boundary: Marten owns its event/document DDL through `AutoCreate.CreateOrUpdate` plus `ApplyAllDatabaseChangesOnStartup` on the single-writer placement and `AssertDatabaseMatchesConfigurationOnStartup` everywhere else (the fleet asserts, never applies); the EF identity model owns its relational DDL through generated migrations; the two compose at boot into one `SchemaGate.Admit` fold so a store whose Marten schema or EF migration set is newer than the compiled model is a typed `ServingBehind`/`Advanced` rejection carrying the unknown identifiers, never a silent open that corrupts on first write; `EnsureCreated` bypasses the history mechanism and is admitted only for the ephemeral test row; read-ahead serving (`ServingBehind`) is legal only under a declared expand-only suite invariant, so the sound default is hard rejection.

```csharp signature
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

public static class SchemaGate {
    public static Fin<SchemaVerdict> Admit(DbContext store, bool writesPending) {
        ArgumentNullException.ThrowIfNull(store);
        var assembly = toSeq(store.Database.GetMigrations());
        var applied = toSeq(store.Database.GetAppliedMigrations());
        var unknown = applied.Filter(id => !assembly.Exists(held => held == id));
        var pending = assembly.Filter(id => !applied.Exists(held => held == id));
        return (unknown.IsEmpty, pending.IsEmpty) switch {
            (false, _) => Fin.Fail<SchemaVerdict>(Error.New(8341, $"<schema-ahead:{unknown.Count}>")),
            (_, false) when writesPending => Try.lift(fun(() => store.Database.Migrate())).Run()
                .MapFail(error => Error.New(8342, $"<apply-failed:{error.Message}>"))
                .Map(_ => (SchemaVerdict)(applied.IsEmpty ? new SchemaVerdict.Provisioned(pending) : new SchemaVerdict.Advanced(pending))),
            (_, false) => Fin.Succ<SchemaVerdict>(new SchemaVerdict.AwaitBundle(pending, Fresh: applied.IsEmpty)),
            _ => Fin.Succ<SchemaVerdict>(store.Database.HasPendingModelChanges() ? new SchemaVerdict.Drifted() : new SchemaVerdict.Serving()),
        };
    }
}
```

| [INDEX] | [POLICY]            | [VALUE]                                | [BINDING]                                                  |
| :-----: | :------------------ | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | Marten DDL          | `AutoCreate.CreateOrUpdate` (writer)   | `AssertDatabaseMatchesConfigurationOnStartup` on the fleet |
|  [02]   | EF identity DDL     | generated migrations                   | the relational projection over the Marten-written rows    |
|  [03]   | boot verdict        | one `SchemaGate.Admit` fold            | schema-ahead is a typed rejection, never a silent open    |
|  [04]   | pending apply       | single-writer placement only           | fleet member `AwaitBundle`, never self-applies            |
