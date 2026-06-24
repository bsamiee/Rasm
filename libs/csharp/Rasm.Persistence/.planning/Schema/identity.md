# [PERSISTENCE_SCHEMA_IDENTITY]

Every persisted object traces to three facts through one identity axis: its key (how the row is named), its access grant (who may touch it and how), and its attested author (which signed actor caused the op). `IdentityPolicy` is the `[SmartEnum<string>]` key axis — one row per generation strategy carrying generator, big-endian transcription, ordering semantics, collision law, and per-provider default SQL, dispatching mint and decode per row so a key change is an expand-wave second key plus a derivation flip plus a contract-wave drop, never an `AlterColumn`. `Capability` is the one `[Flags]` authorization vocabulary every grant draws from — object-generic, audit, and branch-specific permissions in one bit field whose `Owner` superset is a value-derived union of every lane, scoped by `AclScope`, so the `Version/commits#COMMIT_DAG` branch-ref grant is the same vocabulary narrowed to `AclScope.Branch`, never a parallel taxonomy. `ObjectAcl` is the per-object allow/deny grant with owner, scope-altitude inheritance, and grant provenance; `OpDigest` is the `[ValueObject<byte[]>]` cryptographic op-hash whose width matches the signing algorithm; `SignedAuthorship` is the KMS-signed actor attestation tying an op to a verified blame agent; `SigningKeyring` is the provider-neutral `Sign`/`Verify` delegate pair the same `KmsProvider` custody axis projects; and `Authority` folds object admission and authorship verification into one `AuthDecision`.

## [01]-[INDEX]

- [01]-[IDENTITY_POLICY]: key axis, big-endian transcription, per-row mint/decode, content addressing, and identity migration.
- [02]-[AUTHORITY]: unified capability vocabulary, object allow/deny ACL with inheritance, KMS-signed authorship, and the one admission/verification fold.

## [02]-[IDENTITY_POLICY]

- Owner: `IdentityPolicy` `[SmartEnum<string>]` five rows under the `StoreKeyPolicy` ordinal accessor carrying generator, big-endian transcription, ordering, collision class, CLR type, and per-provider default SQL columns, dispatching `Mint` per row through one generated `Switch`; `StoreKey` `[Union]` is the closed key carrier (`Surrogate`/`Content`/`Natural`); `Collision` `[SmartEnum]` is the collision-posture vocabulary.
- Cases: `uuid-v7` (default, B-tree insert-local, client-minted `Guid.CreateVersion7`), `uuid-v7-backfill` (historical-timestamp mint for deterministic backfill), `content-hash` (immutable-payload `XxHash128` addressing), `natural-key` (caller-owned identifier passthrough), `namespace-key` (deterministic RFC-4122 v5 over a namespace and a name for stable derived ids); `Collision` rows are `unmintable`, `content-idempotent`, `foreign-authority`, `derived-deterministic`.
- Entry: `public StoreKey Mint(ReadOnlyMemory<byte> material, Instant observed)` is the per-row identity factory dispatching on the policy through the generated `Switch` — the `uuid-v7` row mints `Guid.CreateVersion7`, `uuid-v7-backfill` mints `Guid.CreateVersion7(observed)`, `content-hash` mints `XxHash128.HashToUInt128(material.Span)`, `natural-key` admits the caller bytes, `namespace-key` mints a deterministic RFC-4122 v5 `Guid` (`SHA1` over `Namespace ++ name`, version/variant nibbles set); `StoreKey.Spelled` is the ordering-preserving big-endian binary transcription, `IdentityPolicy.Decode` its per-row inverse reading the spelled bytes back through the row's CLR type, and `StoreKey.ObservedAt` projects a v7 key's embedded creation time (the persisted analogue of PG `uuid_extract_timestamp`); all pure values, never an exception rail.
- Packages: Thinktecture.Runtime.Extensions, System.IO.Hashing, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one `IdentityPolicy` row carries text, CLR type, per-provider default SQL, ordering, collision, and client-generated precedence; a future v3 namespace row swaps `SHA1` for `MD5` on the same RFC-4122 construction; a new posture is one `Collision` row; zero new surface.
- Boundary: every persisted key strategy in the package traces to one row here — uuid-ossp is the deleted extension route. `StoreKey` is the one closed key carrier — its `Guid`/`UInt128`/`string` cases hold the three CLR shapes the rows mint so a column type is a case projection, never a parallel key type per provider. The per-provider default-SQL columns feed column defaults as data, and the `ClientGenerated` precedence column resolves the double-generation gate — when it is set the model configures the key column `ValueGeneratedNever` so the client `Guid.CreateVersion7` value is authoritative and the provider default never fires on the same key, while a `false` value defers to the column default for server-minted rows. Ordering survives transcription only when the spelling preserves it — every case transcribes big-endian (`Guid.ToByteArray(bigEndian: true)` for the surrogate, `BinaryPrimitives.WriteUInt128BigEndian` for the content key, UTF-8 for the natural key) because the canonical text form is lexically ordered but the platform-default little-endian export is not, so a binary-keyed primary index over a native-endian spelling degrades to random-insert fragmentation; the sqlite `"uuid7()"` leg executes through the native function-registration rows. `StoreKey.ObservedAt` makes a v7 key a free coarse creation-time axis (the persisted analogue of `uuid_extract_timestamp`) so a composite `(low-cardinality discriminant, v7 key)` index stays append-local while PG18 skip scan serves its key-only lookups, deleting the bare-key second index. `Mint` is total over the closed row set and `IdentityPolicy.Decode` reads `StoreKey.Spelled` back through the same row's CLR type, so the spell/decode round-trip is one owner and a hand-written per-column key parser is the deleted form. Identity-row change is never `AlterColumn` — it is an expand-wave second key backfilled by the `uuid-v7-backfill` row, a derivation flip, and a contract-wave drop, the only identity migration preserving foreign references, changefeed continuity, and cursor validity at once. Content identity is non-cryptographic `XxHash128` with no security claim — the `content-hash` row carries `Collision.ContentIdempotent` so a re-serialized identical payload collides to the same key and dedupes; the `namespace-key` row carries `Collision.DerivedDeterministic` and mints the canonical RFC-4122 v5 namespace UUID (`SHA1` over the namespace bytes concatenated with the name, version nibble `5`, variant bits `10`) so two peers deriving the same `(namespace, name)` converge on one id without coordination — `SHA1` here is the spec construction, not a security claim, and `XxHash128` stays the content-address hash.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
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

    public byte[] Spelled() =>
        this switch {
            Surrogate s => s.Value.ToByteArray(bigEndian: true),
            Content c => SpelledContent(c.Value),
            Natural n => Encoding.UTF8.GetBytes(n.Value),
        };

    // big-endian content transcription — native-endian export fractures a binary-keyed index
    private static byte[] SpelledContent(UInt128 value) {
        var bytes = new byte[16];
        BinaryPrimitives.WriteUInt128BigEndian(bytes, value);
        return bytes;
    }

    public Option<Instant> ObservedAt() =>
        this is Surrogate { Value.Version: 7 } s
            ? Some(Instant.FromUnixTimeMilliseconds(UnixMillis(s.Value)))
            : None;

    // first 48 bits of a v7 layout are the big-endian unix-millis timestamp
    private static long UnixMillis(Guid key) {
        Span<byte> bytes = stackalloc byte[16];
        _ = key.TryWriteBytes(bytes, bigEndian: true, out _);
        return ((long)BinaryPrimitives.ReadUInt16BigEndian(bytes) << 32) | BinaryPrimitives.ReadUInt32BigEndian(bytes[2..]);
    }
}

[SmartEnum<string>]
[ValidationError<SchemaFault>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
[KeyMemberComparer<StoreKeyPolicy, string>]
public sealed partial class IdentityPolicy {
    public static readonly IdentityPolicy UuidV7Key = new("uuid-v7", typeof(Guid), Collision.Unmintable, ordered: true, pgDefaultSql: "uuidv7()", sqliteDefaultSql: "uuid7()", clientGenerated: true);
    public static readonly IdentityPolicy UuidV7Backfill = new("uuid-v7-backfill", typeof(Guid), Collision.Unmintable, ordered: true, pgDefaultSql: null, sqliteDefaultSql: null, clientGenerated: true);
    public static readonly IdentityPolicy ContentHash = new("content-hash", typeof(UInt128), Collision.ContentIdempotent, ordered: false, pgDefaultSql: null, sqliteDefaultSql: null, clientGenerated: true);
    public static readonly IdentityPolicy NaturalKey = new("natural-key", typeof(string), Collision.ForeignAuthority, ordered: false, pgDefaultSql: null, sqliteDefaultSql: null, clientGenerated: true);
    public static readonly IdentityPolicy NamespaceKey = new("namespace-key", typeof(Guid), Collision.DerivedDeterministic, ordered: false, pgDefaultSql: null, sqliteDefaultSql: null, clientGenerated: true);

    public static readonly Guid Namespace = new("6e89a1f0-1d2b-7c4e-9f3a-0b1c2d3e4f50");

    private readonly string? pgDefaultSql;
    private readonly string? sqliteDefaultSql;

    public Type ClrType { get; }
    public Collision Collision { get; }
    public bool Ordered { get; }
    public bool ClientGenerated { get; }
    public Option<string> PgDefaultSql => Optional(pgDefaultSql);
    public Option<string> SqliteDefaultSql => Optional(sqliteDefaultSql);

    public StoreKey Mint(ReadOnlyMemory<byte> material, Instant observed) =>
        Switch<(ReadOnlyMemory<byte> Material, Instant Observed), StoreKey>(
            state: (material, observed),
            uuidV7Key: static _ => new StoreKey.Surrogate(Guid.CreateVersion7()),
            uuidV7Backfill: static s => new StoreKey.Surrogate(Guid.CreateVersion7(s.Observed.ToDateTimeOffset())),
            contentHash: static s => new StoreKey.Content(XxHash128.HashToUInt128(s.Material.Span)),
            naturalKey: static s => new StoreKey.Natural(Encoding.UTF8.GetString(s.Material.Span)),
            namespaceKey: static s => new StoreKey.Surrogate(NamespaceUuid(Namespace, s.Material.Span)));

    // Spelled inverse — the row's CLR type selects the byte reading, closing the key round-trip
    public StoreKey Decode(ReadOnlySpan<byte> spelled) =>
        Switch<byte[], StoreKey>(
            state: spelled.ToArray(),
            uuidV7Key: static b => new StoreKey.Surrogate(new Guid(b.AsSpan()[..16], bigEndian: true)),
            uuidV7Backfill: static b => new StoreKey.Surrogate(new Guid(b.AsSpan()[..16], bigEndian: true)),
            contentHash: static b => new StoreKey.Content(BinaryPrimitives.ReadUInt128BigEndian(b)),
            naturalKey: static b => new StoreKey.Natural(Encoding.UTF8.GetString(b)),
            namespaceKey: static b => new StoreKey.Surrogate(new Guid(b.AsSpan()[..16], bigEndian: true)));

    // RFC 4122 §4.3 name-based v5: SHA1(namespace ++ name), version nibble 5, variant bits 10 — streamed so a caller-sized name never sizes the stack
    private static Guid NamespaceUuid(Guid ns, ReadOnlySpan<byte> name) {
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

## [03]-[AUTHORITY]

- Owner: `Capability` the one `[Flags]` authorization vocabulary (object-generic, audit, and branch-specific permissions in one bit field with a value-derived `Owner` superset and an `Admits(demand)` superuser-aware containment operator); `AclScope` `[SmartEnum<string>]` the gated-object-kind vocabulary carrying its altitude `Parent` so inheritance walks a declared ladder; `AclEntry` the per-principal allow/deny grant with provenance and a `[From, Until)` activation window; `ObjectAcl` the owner-plus-inherited per-object grant set; `SigningAlgorithm` `[SmartEnum<string>]` the algorithm axis carrying its `HashAlgorithmName` (from which `DigestWidth` derives) and provider `WireName`; `OpDigest` the `[ValueObject<byte[]>]` cryptographic op-hash; `SigningKeyring` the provider-neutral `Sign`/`Verify` delegate pair the `KmsProvider` axis projects; `SignedAuthorship` the KMS-signed actor attestation; `AuthDecision` `[Union]` the closed admission/verification verdict; `Authority` the one static fold over object admission and authorship verification.
- Cases: `Capability` grants `Read | Write | Delete | Share | Revoke | Audit | Merge | Rebase | ForcePush | Admin` (object lane: `Read`..`Revoke`; `Audit` is the read-the-grant-and-blame-trail lane the `Query/federation#FEDERATION` audit log demands; branch lane adds `Merge`..`ForcePush`; `Admin` is the explicit superuser bit and `Owner` is the value-derived union of every lane); `AclScope` is `document → branch → tenant` with `element-set` nesting under `document`, each row naming its `Parent`; `SigningAlgorithm` carries one row per JWS algorithm × digest width the providers intersect — `es256`/`es384`/`es512` (ECDSA over SHA-256/384/512), `rs256`/`rs384`/`rs512` (RSASSA-PKCS1-v1_5), `ps256`/`ps384`/`ps512` (RSASSA-PSS) — each row holding its `HashAlgorithmName` (so `DigestWidth` derives 32/48/64 and never an imperative width branch) and the AWS-form `WireName` (`ECDSA_SHA_512`, `RSASSA_PSS_SHA_256`, …) the Azure JWS short name and GCP `CryptoKeyVersionAlgorithm` map from at the keyring edge; `AuthDecision` is `Granted | Attested | Authentic | Unsigned | Denied | ScopeMismatch | Expired | Unauthored | Forged | DigestWidth` — `Unsigned` is the local/Personal-tier authorship verdict on the `KmsProvider.None` profile (op attributed, no KMS signature), the success twin of `Authentic` carrying no cryptographic proof.
- Entry: `public static AuthDecision Admit(ObjectAcl acl, Principal principal, Seq<Principal> roles, Capability demand, UInt128 scope, Instant now)` folds the owner, the principal's direct allow/deny entries, its role entries, and the scope-altitude-inherited parent grants into one effective `Capability`, denying by default, honoring deny-over-allow, and treating the `Owner`/`Admin` superuser as admitting every demand; `public static IO<AuthDecision> Attest(Principal actor, OpDigest digest, KmsProvider provider, string signingKeyId, SigningKeyring keyring, ClockPolicy clocks, CorrelationId correlation)` short-circuits the `KmsProvider.None` local tier to `Unsigned` without a keyring call, else signs the digest through the KMS keyring after gating its width against the keyring algorithm, projecting an `Attested`-carried `SignedAuthorship` or `DigestWidth`; `public static IO<AuthDecision> Verify(SignedAuthorship authorship, OpDigest digest, SigningKeyring keyring)` resolves a `None`-provider authorship to `Unsigned` and otherwise checks the digest binding and the KMS signature, projecting `Authentic`/`Unauthored`/`Forged`.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, AWSSDK.KeyManagementService, Azure.Security.KeyVault.Keys, Google.Cloud.Kms.V1, BCL inbox (`System.Security.Cryptography` for the digest and the GCP in-process verify)
- Growth: one `Capability` flag per new permission (object, audit, or branch); one `AclScope` row per new gated object kind naming its `Parent`; one `SigningAlgorithm` row per new JWS algorithm family carrying its `HashAlgorithmName` and `WireName`; one `AclEntry` per principal grant; one `AuthDecision` case per new verdict; one `KmsProvider` arm projects its `SigningKeyring`; zero new surface.
- Boundary: `Capability` is the one authorization vocabulary — the `Version/commits#COMMIT_DAG` branch-ref grant is this same bit field narrowed to the `Merge | Rebase | ForcePush` branch lane under `AclScope.Branch`, so a second branch-only flag enum is the deleted form and `BranchRef.Permits` reads `Capability` directly; the `Query/federation#FEDERATION` blame/audit-log read is the `Audit` lane, never a parallel read flag. Superuser is value-derived, not a bare bit — `Capability.Owner` is the union of every lane and `grant.Admits(demand)` returns true when the grant carries `Admin` or `Owner` regardless of which lane bits `demand` names, so an owner demanding `Read` is admitted; the prior `(grant & demand) == demand` containment that silently denied an `Admin`-only owner its `Read` is the deleted bug. Object-level authorization is the per-object/per-branch allow/deny grant — `ObjectAcl` scopes to a document, a branch, an element-set, or a tenant, and `Admit` folds the principal's direct grants with its role grants and the inherited parent grants into one effective `Capability` with deny-over-allow so an explicit `AclEntry` deny overrides every inherited allow, and a coarse table-level grant or a tenancy-only gate is the deleted form because the gate scopes to the object; the `Owner` principal carries `Capability.Owner` so the creator never locks itself out, and the `AclEntry` `[From, Until)` window time-boxes a grant on both ends — `Live` admits only inside the window so a future-dated delegation never grants early, and a once-live grant past its `Until` that would otherwise carry the demand folds `Expired` at its own altitude through `Lapsed` rather than silently persisting, while a not-yet-`From` grant stays `Denied` (it was never granted, so it cannot have expired). Inheritance is the `AclScope.Parent` altitude ladder — an `element-set` ACL inherits its `document`, a `document` inherits its `branch`, a `branch` inherits its `tenant`, so the grant resolves at the nearest scope and `Admit` walks the `Inherited` chain once with the parent-scope invariant `child.Kind.Parent == Some(parent.Kind)` so a mis-stacked chain is rejected, not silently honored; the `Store/tenancy#TENANCY_RLS` RLS gate stays the row-level coarse scope and this object-ACL is the fine within-tenant scope, two altitudes never duplicated. Signed authorship is the actor-identity-to-blame seam — a cloud-KMS-provisioned op carries a `SignedAuthorship` whose signature is over a `SigningAlgorithm`-width cryptographic `OpDigest` through an asymmetric KMS key so a blame attribution (`Version/timetravel#TIME_TRAVEL`, `Version/provenance#CAUSAL_DAG`) names a verified actor, not an unauthenticated `Actor` string; the `OpDigest` is a real `SHA-256`/`SHA-384`/`SHA-512` over the op bytes whose byte width the keyring algorithm's `HashAlgorithmName` pins, so a 16-byte non-cryptographic `XxHash128` standing in for the signed digest — which an ES256 KMS key rejects — is the deleted form, and a width mismatch folds `DigestWidth` at `Attest` before any KMS call. The `KmsProvider.None` local/Personal tier is the one unsigned path — `Attest` projects `Unsigned` and `Verify` resolves a `None`-provider authorship to `Unsigned` so a store with no KMS still records the op→actor binding for blame, never a sentinel signature or an exception; the blame consumers read `Authentic` or `Unsigned` as attributed and only `Authentic` as cryptographically proven, so a mandated-attestation classification on a `None` profile is the encryption owner's `DemandsEncryption` gate, never a forged signature here. `SigningKeyring` is the provider-neutral `Sign`/`Verify` pair the `Store/encryption#KEY_ENVELOPE` `KmsProvider` `[SmartEnum<string>]` axis projects — the AWS arm binds `Sign`→`SignAsync(SignRequest{KeyId, Message=digest, MessageType.DIGEST, SigningAlgorithm})` and `Verify`→`VerifyAsync(VerifyRequest{..., Signature})` reading `VerifyResponse.SignatureValid`; the Azure arm binds `Sign`→`CryptographyClient.Sign(SignatureAlgorithm.ES256, digest)` reading `SignResult.Signature` and `Verify`→`Verify(SignatureAlgorithm, digest, sig)` reading `VerifyResult.IsValid`; the GCP arm binds `Sign`→`AsymmetricSignAsync(CryptoKeyVersionName, new Digest{…})` keyed by the algorithm row's digest field (`Sha256`/`Sha384`/`Sha512`) with the request `DigestCrc32C` set and the response `SignatureCrc32C` verified, reading `AsymmetricSignResponse.Signature`, and `Verify`→a local `ECDsa.VerifyHash`/`RSA` check against `GetPublicKey().Pem` whose `PemCrc32C` is verified on download (Cloud KMS exposes no remote verify verb, so the public key downloads once, its integrity is checked, and verify is in-process), so the signing key is an asymmetric KMS key resolved through the same per-open `SecretLease`-class handle the encryption keyring consumes from `Runtime/config#SECRET_LEASE`, never a bare passphrase or an in-process private key minted here. The provider-specific algorithm type (`SignatureAlgorithm`/`SigningAlgorithmSpec`/`CryptoKeyVersionAlgorithm`) lives only at the keyring delegate edge — `SignedAuthorship` carries the `KmsProvider`, the `SigningAlgorithm` canonical wire string, and the signing key version so a forged authorship is detectable and a key rotation is auditable, never a provider shape in the canonical record; a generic untyped `Error` here is the deleted form — every failure projects an `AuthDecision` case. Verification yields a dedicated `Authentic` verdict, not an overloaded `Granted(Capability.None)` — authentication and authorization are two facets the fold keeps distinct, and `Admit` resolves the capability. The whole surface is one fold — `Admit`, `Attest`, and `Verify` are facets of one authorization decision and a separate per-method static is the deleted form.

```csharp signature
[Flags]
public enum Capability {
    None = 0,
    Read = 1,
    Write = 2,
    Delete = 4,
    Share = 8,
    Revoke = 16,
    Audit = 32,
    Merge = 64,
    Rebase = 128,
    ForcePush = 256,
    Admin = 512,
    // value-derived superset — every lane plus the explicit superuser bit
    Owner = Read | Write | Delete | Share | Revoke | Audit | Merge | Rebase | ForcePush | Admin,
}

public static class CapabilityLaw {
    // superuser-aware containment — Admin/Owner admit any demand regardless of lane bits
    extension(Capability grant) {
        public bool Admits(Capability demand) =>
            (grant & Capability.Admin) == Capability.Admin || (grant & demand) == demand;
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class AclScope {
    public static readonly AclScope Tenant = new("tenant", None);
    public static readonly AclScope Branch = new("branch", Some(Tenant));
    public static readonly AclScope Document = new("document", Some(Branch));
    public static readonly AclScope ElementSet = new("element-set", Some(Document));

    public Option<AclScope> Parent { get; }

    private AclScope(string key, Option<AclScope> parent) : this(key) => Parent = parent;
}

public readonly record struct Principal(string Id) {
    public static Principal Of(string id) => new(id);
}

public sealed record AclEntry(Capability Allow, Capability Deny, Principal GrantedBy, Instant At, Option<Instant> From, Option<Instant> Until) {
    // a grant is live only inside its [From, Until) window — a future-dated delegation never grants early, a lapsed one never lingers
    public bool Live(Instant now) =>
        From.Match(Some: from => now >= from, None: static () => true) &&
        Until.Match(Some: until => now < until, None: static () => true);

    // lapsed is strictly past-Until, never not-yet-From — only a once-live grant folds Expired, a future grant folds Denied
    public bool Lapsed(Instant now) => Until.Match(Some: until => now >= until, None: static () => false);
}

public sealed record ObjectAcl(
    UInt128 Scope,
    AclScope Kind,
    Principal Owner,
    HashMap<string, AclEntry> Principals,
    HashMap<string, AclEntry> Roles,
    Option<ObjectAcl> Inherited) {
    // the altitude invariant — a child ACL only inherits its declared parent scope
    public bool LadderValid =>
        Inherited.Match(Some: parent => Kind.Parent == Some(parent.Kind) && parent.LadderValid, None: static () => true);
}

[SmartEnum<string>]
public sealed partial class SigningAlgorithm {
    public static readonly SigningAlgorithm Es256 = new("es256", HashAlgorithmName.SHA256, "ECDSA_SHA_256");
    public static readonly SigningAlgorithm Es384 = new("es384", HashAlgorithmName.SHA384, "ECDSA_SHA_384");
    public static readonly SigningAlgorithm Es512 = new("es512", HashAlgorithmName.SHA512, "ECDSA_SHA_512");
    public static readonly SigningAlgorithm Rs256 = new("rs256", HashAlgorithmName.SHA256, "RSASSA_PKCS1_V1_5_SHA_256");
    public static readonly SigningAlgorithm Rs384 = new("rs384", HashAlgorithmName.SHA384, "RSASSA_PKCS1_V1_5_SHA_384");
    public static readonly SigningAlgorithm Rs512 = new("rs512", HashAlgorithmName.SHA512, "RSASSA_PKCS1_V1_5_SHA_512");
    public static readonly SigningAlgorithm Ps256 = new("ps256", HashAlgorithmName.SHA256, "RSASSA_PSS_SHA_256");
    public static readonly SigningAlgorithm Ps384 = new("ps384", HashAlgorithmName.SHA384, "RSASSA_PSS_SHA_384");
    public static readonly SigningAlgorithm Ps512 = new("ps512", HashAlgorithmName.SHA512, "RSASSA_PSS_SHA_512");

    public HashAlgorithmName Hasher { get; }
    public string WireName { get; }
    public int DigestWidth => Hasher == HashAlgorithmName.SHA512 ? 64 : Hasher == HashAlgorithmName.SHA384 ? 48 : 32;

    private SigningAlgorithm(string key, HashAlgorithmName hasher, string wireName) : this(key) =>
        (Hasher, WireName) = (hasher, wireName);

    // the row's HashAlgorithmName selects the digest — never an imperative width branch, so a SHA-512 row hashes correctly
    public OpDigest Hash(ReadOnlySpan<byte> opBytes) {
        Span<byte> digest = stackalloc byte[64];
        var written = CryptographicOperations.HashData(Hasher, opBytes, digest);
        return OpDigest.Create(digest[..written].ToArray());
    }
}

public sealed class DigestKeyPolicy : IEqualityComparerAccessor<byte[]>, IEqualityComparer<byte[]> {
    public static IEqualityComparer<byte[]> EqualityComparer { get; } = new DigestKeyPolicy();
    bool IEqualityComparer<byte[]>.Equals(byte[]? x, byte[]? y) => x.AsSpan().SequenceEqual(y);
    int IEqualityComparer<byte[]>.GetHashCode(byte[] value) {
        var hash = new HashCode();
        hash.AddBytes(value);
        return hash.ToHashCode();
    }
}

[ValueObject<byte[]>(EqualityComparisonOperators = OperatorsGeneration.Default)]
[KeyMemberEqualityComparer<DigestKeyPolicy, byte[]>]
[ValidationError<SchemaFault>]
public readonly partial struct OpDigest {
    private static Validation<SchemaFault, byte[]> ValidateFactoryArguments(byte[] value) =>
        value is { Length: 32 or 48 or 64 }
            ? Validation<SchemaFault, byte[]>.Success(value)
            : Validation<SchemaFault, byte[]>.Fail(SchemaFault.Create($"<op-digest-width:{value.Length}>"));

    public bool Fits(SigningAlgorithm algorithm) => Value.Length == algorithm.DigestWidth;
}

public sealed record SigningKeyring(
    SigningAlgorithm Algorithm,
    Func<ReadOnlyMemory<byte>, IO<ReadOnlyMemory<byte>>> Sign,
    Func<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>, IO<bool>> Verify);

public sealed record SignedAuthorship(
    Principal Actor,
    KmsProvider Provider,
    string SigningKeyId,
    SigningAlgorithm Algorithm,
    OpDigest Digest,
    ReadOnlyMemory<byte> Signature,
    Instant At,
    CorrelationId Correlation);

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

    public Fin<Capability> Authorized() => this switch {
        Granted g => Fin.Succ(g.Effective),
        var fault => Fin.Fail<Capability>(fault.AsFault()),
    };

    public SchemaFault AsFault() => this switch {
        ScopeMismatch m => new SchemaFault.AclScopeMismatch(m.Demanded),
        Denied d => new SchemaFault.AclDenied(d.Principal.Id, d.Demand, d.Scope),
        Expired e => new SchemaFault.AclExpired(e.Principal.Id, e.LapsedAt),
        Unauthored u => new SchemaFault.AuthorshipUnauthored(u.Expected, u.Found),
        Forged f => new SchemaFault.AuthorshipForged(f.Actor.Id, f.SigningKeyId),
        DigestWidth w => new SchemaFault.OpDigestWidth(w.Expected, w.Actual),
        _ => SchemaFault.Create("<auth-granted-not-a-fault>"),
    };
}

public static class Authority {
    public static Capability Effective(ObjectAcl acl, Principal principal, Seq<Principal> roles, Instant now) {
        var owned = acl.Owner == principal ? Capability.Owner : Capability.None;
        var direct = acl.Principals.Find(principal.Id).Filter(e => e.Live(now));
        var inherited = acl.Inherited.Map(parent => Effective(parent, principal, roles, now)).IfNone(Capability.None);
        var roleAllow = roles.Fold(Capability.None, (acc, role) =>
            acc | acl.Roles.Find(role.Id).Filter(e => e.Live(now)).Map(static e => e.Allow).IfNone(Capability.None));
        var allow = owned | inherited | roleAllow | direct.Map(static e => e.Allow).IfNone(Capability.None);
        var deny = direct.Map(static e => e.Deny).IfNone(Capability.None) |
            roles.Fold(Capability.None, (acc, role) => acc | acl.Roles.Find(role.Id).Filter(e => e.Live(now)).Map(static e => e.Deny).IfNone(Capability.None));
        return allow & ~deny;
    }

    // a demand-bearing grant gone stale past its Until at any altitude is Expired, not silently dropped — a not-yet-From grant stays Denied
    private static bool LapsedFor(ObjectAcl acl, Principal principal, Seq<Principal> roles, Capability demand, Instant now) {
        var lapsedDirect = acl.Principals.Find(principal.Id).Exists(e => e.Lapsed(now) && e.Allow.Admits(demand));
        var lapsedRole = roles.Exists(r => acl.Roles.Find(r.Id).Exists(e => e.Lapsed(now) && e.Allow.Admits(demand)));
        return lapsedDirect || lapsedRole || acl.Inherited.Exists(p => LapsedFor(p, principal, roles, demand, now));
    }

    public static AuthDecision Admit(ObjectAcl acl, Principal principal, Seq<Principal> roles, Capability demand, UInt128 scope, Instant now) =>
        acl.Scope != scope || !acl.LadderValid
            ? new AuthDecision.ScopeMismatch(scope, acl.Scope)
        : Effective(acl, principal, roles, now) is var grant && grant.Admits(demand)
            ? new AuthDecision.Granted(grant)
        : LapsedFor(acl, principal, roles, demand, now)
            ? new AuthDecision.Expired(principal, now)
            : new AuthDecision.Denied(principal, demand, scope);

    // the None provider is the local/Personal tier with no KMS — the op is attributed, never cryptographically signed
    public static IO<AuthDecision> Attest(
        Principal actor, OpDigest digest, KmsProvider provider, string signingKeyId,
        SigningKeyring keyring, ClockPolicy clocks, CorrelationId correlation) =>
        provider == KmsProvider.None
            ? IO.pure<AuthDecision>(new AuthDecision.Unsigned(actor, digest, clocks.Now, correlation))
        : digest.Fits(keyring.Algorithm)
            ? keyring.Sign(digest.Value).Map(signature => (AuthDecision)new AuthDecision.Attested(
                new SignedAuthorship(actor, provider, signingKeyId, keyring.Algorithm, digest, signature, clocks.Now, correlation)))
            : IO.pure<AuthDecision>(new AuthDecision.DigestWidth(keyring.Algorithm.DigestWidth, digest.Value.Length));

    public static IO<AuthDecision> Verify(SignedAuthorship authorship, OpDigest digest, SigningKeyring keyring) =>
        authorship.Provider == KmsProvider.None
            ? IO.pure<AuthDecision>(new AuthDecision.Unsigned(authorship.Actor, authorship.Digest, authorship.At, authorship.Correlation))
        : authorship.Digest != digest
            ? IO.pure<AuthDecision>(new AuthDecision.Unauthored(digest, authorship.Digest))
            : keyring.Verify(digest.Value, authorship.Signature).Map(valid =>
                valid ? (AuthDecision)new AuthDecision.Authentic(authorship.Actor, authorship.SigningKeyId)
                      : new AuthDecision.Forged(authorship.Actor, authorship.SigningKeyId));
}
```

## [04]-[RESEARCH]

- [AUTHORSHIP_SIGNING_KEY] [SPIKE]: the live KMS asymmetric-sign round-trip the `Authority.Attest`/`Verify` fold consumes — the `SigningKeyring` delegate binding for each `KmsProvider` arm proven against a live key over a `SigningAlgorithm.DigestWidth`-sized `OpDigest` (the 32/48/64-byte `SHA-256`/`SHA-384`/`SHA-512` the row's `HashAlgorithmName` selects, never the non-cryptographic 16-byte content hash): AWS `SignAsync(SignRequest{Message=digest, MessageType.DIGEST, SigningAlgorithm})`/`VerifyAsync` echoing `SignatureValid`, Azure `CryptographyClient.Sign(SignatureAlgorithm.ES256, digest)`/`Verify` reading `SignResult.Signature`/`VerifyResult.IsValid`, and the GCP `AsymmetricSign` keyed by the per-row digest field (`Sha256`/`Sha384`/`Sha512`) with `DigestCrc32C` set and `AsymmetricSignResponse.SignatureCrc32C` verified, then `GetPublicKey().Pem` (`PemCrc32C`-checked) in-process `ECDsa.VerifyHash` (Cloud KMS has no remote verify verb), confirming the digest-width gate rejects an under-width digest before any KMS call and the signing-key-version echo round-trips before the keyring fence pins. The signing key resolves through the `Runtime/config#SECRET_LEASE` per-open handle the encryption keyring shares; only the live sign/verify round-trip stays SPIKE.
