# [PERSISTENCE_SCHEMA_IDENTITY]

Schema identity for every store the suite opens. `IdentityPolicy` is the key axis every persisted identifier traces to — one row per generation strategy carrying generator, big-endian transcription, ordering semantics, collision law, and per-provider default SQL — so an identity-row change is an expand-wave second key plus a derivation flip plus a contract-wave drop, never an `AlterColumn`. The object-level ACL fold and the signed-authorship record ride the same identity axis: every persisted object traces to its key, its access grant, and its attested author through one policy surface.

## [01]-[INDEX]

- [01]-[IDENTITY_POLICY]: key axis, big-endian transcription, identity migration, object ACL, and signed authorship.

## [02]-[IDENTITY_POLICY]

- Owner: `IdentityPolicy` `[SmartEnum<string>]` four rows under the `StoreKeyPolicy` ordinal accessor carrying generator, big-endian transcription, ordering, collision class, clr type, and per-provider default SQL columns; `Collision` `[SmartEnum]` is the collision-posture vocabulary; `ObjectAcl` is the per-object/per-branch capability-and-RBAC grant row; `SignedAuthorship` is the actor-identity attestation tying an op to a blame agent; `Authz` is the static surface folding object-level admission and authorship verification.
- Cases: uuid-v7 (default, B-tree insert-local), uuid-v7-backfill (historical-timestamp mint for deterministic backfill), content-hash (immutable-payload addressing), natural-key (caller-owned identifiers); `Collision` rows are unmintable, content-idempotent, foreign-authority; `ObjectAcl` grants `Read | Write | Delete | Grant | Admin` per principal per object scope; `SignedAuthorship` carries actor, signing key id, op-digest, signature, and `Instant`.
- Entry: `public static Guid NextKey()` mints the uuid-v7 default and `public static Guid BackfilledKey(Instant observed)` mints the deterministic historical surrogate; `public static byte[] Spelled(Guid key)` is the big-endian binary transcription law; `public static UInt128 ContentKey(ReadOnlySpan<byte> content)` derives content identity — pure values; `public static Fin<AclGrant> Admit(ObjectAcl acl, string principal, Seq<string> roles, AclGrant demand, UInt128 scope)` is the object-level admission fold and `public static bool Verify(SignedAuthorship authorship, UInt128 opDigest, Func<string, ReadOnlyMemory<byte>, UInt128, bool> verify)` checks an op's authorship.
- Packages: Thinktecture.Runtime.Extensions, System.IO.Hashing, LanguageExt.Core, NodaTime, Rasm.AppHost (project), BCL inbox
- Growth: one `IdentityPolicy` row (key text, clr type, per-provider default SQL, ordering, collision, client-generated precedence); a v3/v5 namespace key is one future row on the same axis derived through `Guid.CreateVersion7`'s sibling `CreateVersion5`; one `AclGrant` flag per new capability; one `Collision` row per new posture; one `ObjectAcl` scope per new gated object kind; zero new surface.
- Boundary: every persisted key strategy in the package traces to one row here — uuid-ossp is the deleted extension route. The per-provider default-SQL columns feed column defaults as data, and the `ClientGenerated` precedence column resolves the double-generation gate — when it is set the model configures the key column `ValueGeneratedNever` so the client `Guid.CreateVersion7` value is authoritative and the provider default never fires on the same key, while a `false` value defers to the column default for server-minted rows. Ordering survives transcription only when the spelling preserves it — `ToByteArray(bigEndian: true)` is the binary transcription law because the canonical text form is lexically time-ordered but the default little-endian byte export is not, so a binary-keyed primary index without it degrades to random-insert fragmentation; the sqlite `"uuid7()"` leg executes through the native function-registration rows. `uuid_extract_timestamp` makes a v7 key a free coarse creation-time axis so a composite `(low-cardinality discriminant, v7 key)` index stays append-local while PG18 skip scan serves its key-only lookups, deleting the bare-key second index. Identity-row change is never `AlterColumn` — it is an expand-wave second key backfilled by `BackfilledKey`, a derivation flip, and a contract-wave drop, the only identity migration preserving foreign references, changefeed continuity, and cursor validity at once. Content identity is non-cryptographic XxHash128 with no security claim. Object-level authorization is the per-object/per-branch RBAC-plus-capability grant — `ObjectAcl` scopes a grant to a document, a branch (the `Version/commits#COMMIT_DAG` `BranchAcl` is the branch-scoped projection of this fold), an element-set, or a tenant, and `Admit` folds the principal's direct grants with its role grants so a capability is the union, denying by default — a coarse table-level grant or a tenancy-only gate is the deleted form because the gate scopes to the object. Signed authorship is the actor-identity-to-blame seam — every op carries a `SignedAuthorship` whose signature is over the op digest so a blame attribution (`Version/timetravel#TIME_TRAVEL`, `Version/provenance#CAUSAL_DAG`) names a verified actor, not an unauthenticated `Actor` string, and the signing key id resolves through the AppHost identity seam (the signed actor identity is host-resolved, never minted here) so a forged authorship is detectable. The tenancy RLS gate (`Store/server#TENANCY_RLS`) stays the row-level coarse scope and the object-ACL is the fine within-tenant scope, two altitudes never duplicated.

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


## [03]-[RESEARCH]

- [AUTHORSHIP_SIGNING_KEY] [SPIKE]: the AppHost-resolved signing-key seam the `Authz.Attest`/`Verify` fold consumes — the host credential-store key handle (macOS keychain / DPAPI / credential store) the signing key id resolves through, proven by tier-1 decompile member-shape only because an unattended credential-store read prompts the operator, with the op-digest signature algorithm and the public-key verification confirmed against the host identity seam at the integrated host. This is the sole residual tier-3 owner on the page (DENSITY_BAR axis [39]); the ACL fold and the authorship record shape are fence-complete, only the OS credential read stays SPIKE.
