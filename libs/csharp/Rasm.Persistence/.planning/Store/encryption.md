# [PERSISTENCE_ENCRYPTION]

Rasm.Persistence makes envelope encryption, KMS key custody, rotation, integrity, and at-rest verification one closed key-management axis across sqlite, PostgreSQL, and object-store. `KmsProvider` is the `[SmartEnum<string>]` cloud-KMS custody axis (AWS, Azure, GCP, none) whose row carries its `WrapKind` mechanism, its `AadBinding` enforcement plane, and its `RotationKind` ladder as derived columns — so the three arms' genuinely different cryptography (AWS `GenerateDataKey`/`Decrypt`/`ReEncrypt` with `EncryptionContext` AAD; Azure native `WrapKey`/`UnwrapKey` with no native AAD; GCP `Encrypt`/`Decrypt` with `AdditionalAuthenticatedData` and CRC32C integrity, rotation by primary-version repoint) lives as policy values on one axis, never one provider's spelling masquerading as a universal law. `EnvelopeKeyring` is the provider-neutral `Mint`/`Unwrap`/`Rewrap`/`Probe` delegate quartet one row projects from a resolved client; `EnvelopeScope` is the `[Union]` per-engine keying family; `KeyEnvelope` is the wrapped-DEK record carrying its provider, scope, AAD digest, key-state, and version ladder; `RotationPolicy` is the rewrap cadence and version-retention ladder; `EncryptionFault` is the closed `[Union]` fault family deriving from `Expected`. The sqlite arm drives the promoted `EncryptionGate.Sqlcipher` from a KMS-unwrapped DEK bound through its `PRAGMA key`/`rekey` ceremony, the PG arm verifies the TDE GUC read-only by handing `ClusterConfig.Verify` the `data_encryption` fragment, the object arm feeds SSE-KMS as the `Store/remote#OBJECT_STORE` `ObjectEncryption.ManagedKey` the provider write request applies (never a descriptor stamp), and the Personal-only `DemandsEncryption` gates a mandated-classification write at admission. The admitted `AWSSDK.KeyManagementService`, `Azure.Security.KeyVault.Keys`, and `Google.Cloud.Kms.V1` supply the per-arm members; `System.IO.Hashing` computes the GCP CRC32C local check and the AAD digest; `Microsoft.Extensions.Compliance.Redaction` owns `DataClassification`; `ClockPolicy`/`ReceiptSinkPort`/`CorrelationId` arrive settled. The per-open key handle crosses to the AppHost runtime through the `Store/encryption ⇄ Rasm.AppHost/Runtime # [PORT]: KMS-unwrap port` seam as one `SecretLease`-class content carrier whose acquire-renew-zeroize custody `Runtime/config#SECRET_LEASE` owns — this owner binds its `EnvelopeKeyring` against the lease-managed CMK access and consumes the resolved per-open handle, never minting a long-lived in-process key.

The governing law is `docs/stacks/csharp/domain/durability#EMBEDDED_STORE`: the SQLCipher key is the first statement on the physical open or the file is unreadable, the `-wal`/`-shm` sidecar set is the unit of replacement, the `Password` connection-string row is the deleted form, and a plaintext DEK never persists. The PG TDE fragment composes `Store/provisioning#CLUSTER_CONFIG` `ClusterConfig.Verify` exactly as `Version/recovery#RECOVERY_LANES` hands it the replication triad — one read-only `pg_settings` admission contract, never a managed key-set.

Wire posture: this page is host-local — keys are unwrapped server-side or on the embedded connection and a DEK never crosses a browser or peer wire, so it carries no `TS_PROJECTION` cluster. The KMS-unwrap port crosses to the AppHost runtime through the `Store/encryption ⇄ Rasm.AppHost/Runtime # [PORT]: KMS-unwrap port` seam as a settled key-handle, never a client-facing projection; `EncryptionFault` reaches any dashboard solely as a `ReceiptSinkPort` envelope whose wire shape is owned at `AppHost/runtime-ports#TS_PROJECTION`.

## [01]-[INDEX]

- [01]-[KEY_ENVELOPE]: the provider-mechanism axis with its wrap/AAD/rotation/integrity policy columns, the envelope scope family, the rotation/version ladder, the keyring quartet, and the closed fault family.
- [02]-[SQLITE_KEYING]: the promoted SQLCipher gate bound to a KMS-unwrapped DEK through its PRAGMA ceremony.
- [03]-[AT_REST]: the PG TDE `data_encryption` fragment handed to `ClusterConfig.Verify` and the object-store SSE-KMS feed projected as the `Store/remote#OBJECT_STORE` `ObjectEncryption.ManagedKey` the provider write request applies.

## [02]-[KEY_ENVELOPE]

- Owner: `KmsProvider` the `[SmartEnum<string>]` cloud-KMS custody axis under the `StoreKeyPolicy` ordinal accessor, each row carrying its `WrapKind` (native-wrap vs encrypt-as-wrap), `AadBinding` (provider-enforced vs application-enforced), `RotationKind` (rewrap-blob vs repoint-primary), and `Integrity` (checksum-verified vs transport-trusted) as derived behaviour columns so the keyring reads mechanism off the row rather than re-deciding per call; `EnvelopeKeyring` the provider-neutral `Mint`/`Unwrap`/`Rewrap`/`Probe` delegate quartet one row projects from a resolved client; `EnvelopeScope` the `[Union]` per-engine keying family; `RotationPolicy` the rewrap cadence and version-retention ladder; `KeyEnvelope` the wrapped-DEK record with its AAD digest, key-state, and version ladder; `EnvelopeFact` with `EnvelopeFactKind` the page-wide fact stream; `EncryptionFault` the closed `[Union]` fault family; `Envelope` the static surface owning the mint, unwrap, rewrap, demand, and key-state folds.
- Cases: `KmsProvider` `aws-kms` (encrypt-as-wrap, `EncryptionContext` AAD, `ReEncrypt` rotation, transport-trusted) | `azure-keyvault` (native `WrapKey`, application-AAD, version-rewrap rotation, transport-trusted) | `gcp-kms` (encrypt-as-wrap, `AdditionalAuthenticatedData` AAD, primary-repoint rotation, CRC32C-verified) | `none` (the plaintext-default Personal-disallowed sentinel); `WrapKind` `native` | `encrypt`; `AadBinding` `context` | `application`; `RotationKind` `rewrap` | `repoint`; `Integrity` `checksum` | `transport`; `EnvelopeScope` `SqliteDek` (the SQLCipher DEK) | `PgTde` (the verify-only cluster GUC) | `ObjectSse` (SSE-KMS object residence); `KeyState` `enabled` | `disabled` | `destroy-scheduled` | `unknown`; `EnvelopeFactKind` `mint` | `unwrap` | `rewrap` | `verify` | `demand-breach` | `aad-mismatch` | `integrity-breach` | `key-disabled`.
- Entry: `public static IO<(KeyEnvelope Envelope, DekLease Dek)> Mint(KmsProvider provider, EnvelopeScope scope, EnvelopeKeyring keyring, EnvelopeAad aad, RotationPolicy rotation, MintMode mode, Func<EnvelopeFact, IO<Unit>> sink, ClockPolicy clocks)` is the one polymorphic mint discriminating on `MintMode` (`Local` derives a fresh DEK and returns it as a self-zeroing `DekLease` for the local cipher; `ReadOnlyNode` mints the wrapped-only blob the read-path unwraps later and yields an empty lease) — it binds the row's `WrapKind` so the AWS/GCP arms call `GenerateDataKey`/`GenerateRandomBytes`+`Encrypt` while the Azure arm wraps a freshly generated DEK through native `WrapKey`, threads the `EnvelopeAad` per arm, captures the returned `KeyArn`/`KeyState`/AAD-digest onto the envelope, emits the `store.encryption.mint` (or short-circuit `store.encryption.key-disabled`) fact through the threaded `sink`, and hands the borrowed plaintext back as the `DekLease` the caller binds inside a `using`; `public static IO<DekLease> Unwrap(KeyEnvelope envelope, EnvelopeKeyring keyring, EnvelopeAad aad, Func<EnvelopeFact, IO<Unit>> sink, ClockPolicy clocks)` recovers the plaintext DEK as a `DekLease` whose `IDisposable` zeroes the borrowed span and verifies the row's `Integrity` (the GCP arm checks `DecryptResponse.PlaintextCrc32C` against the local `Crc32` and the `EncryptResponse.VerifiedPlaintextCrc32C`/`VerifiedAdditionalAuthenticatedDataCrc32C` request-integrity flags) and the AAD digest before yielding, riding `store.encryption.unwrap` or `store.encryption.aad-mismatch`; `public static IO<KeyEnvelope> Rewrap(KeyEnvelope envelope, EnvelopeKeyring keyring, string destinationKeyId, EnvelopeAad aad, Func<EnvelopeFact, IO<Unit>> sink, ClockPolicy clocks)` advances the version ladder through the row's `RotationKind` (the AWS arm `ReEncrypt`s the blob, the Azure arm re-wraps under the new version, the GCP arm `UpdateCryptoKeyPrimaryVersion` repoints and keeps the blob since `Decrypt` resolves the embedded version) and rides `store.encryption.rewrap`; `public static Fin<KeyEnvelope> Demand(KeyEnvelope envelope, DataClassification classification)` rejects a mandated-classification write on a `none`-provider envelope at admission.
- Auto: the `WrapKind` column splits the mechanism per arm rather than asserting a universal one — `encrypt` arms (AWS, GCP) wrap a DEK as `Encrypt`/`GenerateDataKey` and unwrap as `Decrypt`, while the `native` arm (Azure `CryptographyClient.WrapKey(KeyWrapAlgorithm.RsaOaep256, dek)`/`UnwrapKey`) is a first-class vault verb, so "no native wrap verb" is the AWS/GCP law and never the Azure one; the AWS arm runs `GenerateDataKey(KeyId, KeySpec=AES_256)` and persists the returned `CiphertextBlob` while the returned `Plaintext` drives the local cipher under `MintMode.Local` (or `GenerateDataKeyWithoutPlaintext` under `MintMode.ReadOnlyNode`), and `Decrypt(CiphertextBlob, EncryptionContext)` zeroes the recovered `Plaintext` immediately after the local bind; the GCP arm generates the DEK through `GenerateRandomBytes` then `Encrypt(EncryptRequest{ Name, Plaintext, AdditionalAuthenticatedData, PlaintextCrc32C, AdditionalAuthenticatedDataCrc32C })` and unwraps through `Decrypt(DecryptRequest{ Name, Ciphertext, AdditionalAuthenticatedData, CiphertextCrc32C, AdditionalAuthenticatedDataCrc32C })` verifying the Cloud KMS integrity protocol in BOTH directions — the `EncryptResponse.VerifiedPlaintextCrc32C`/`VerifiedAdditionalAuthenticatedDataCrc32C` request-integrity flags confirm KMS received the request CRC intact (a `false` flag is a request-leg corruption the response value check cannot catch) and every `DecryptResponse.PlaintextCrc32C`/`EncryptResponse.CiphertextCrc32C` is checked against the locally computed `Crc32` for the return leg, so any corruption either direction is an `EncryptionFault.IntegrityBreach`, never a trusted partial; the `AadBinding` column places the per-partition AAD enforcement — the AWS/GCP `context` arms ride the provider's `EncryptionContext`/`AdditionalAuthenticatedData` exact-match so a foreign-partition blob cannot decrypt at KMS, while the Azure `application` arm threads the same `EnvelopeAad` and compares the persisted AAD digest application-side at the boundary because `WrapKey`/`UnwrapKey` carry no native AAD parameter; the `EnvelopeAad` carries the store partition identity and (under RLS) the `TenantContext.TenantId.Uuid` rendered through the tenancy page's one canonical projection, digested through `XxHash128` onto `KeyEnvelope.AadDigest` so an unwrap whose AAD diverges from mint is an `EncryptionFault.AadMismatch` regardless of arm; the `RotationKind` column splits rotation — `rewrap` (AWS `ReEncrypt`, Azure version re-wrap) rewrites the persisted blob so the plaintext DEK never re-enters managed memory, while `repoint` (GCP `UpdateCryptoKeyPrimaryVersion`) leaves the blob untouched because `Decrypt` resolves the embedded version, so a rotation is never a decrypt-then-encrypt round trip and `RotationPolicy.RetainVersions` bounds the `KeyEnvelope.History` ladder a multi-version read walks; `Probe` resolves the key's lifecycle state (`DescribeKey.KeyState` / `KeyProperties` / `CryptoKeyVersionState`) into `KeyState` so a wrap against a `Disabled`/`DestroyScheduled` key is an `EncryptionFault.KeyDisabled` rejection at admission rather than a deferred KMS throw; the PG arm never holds a DEK — it verifies the TDE GUC read-only at `#AT_REST`; the object arm feeds the `Store/remote#OBJECT_STORE` `ObjectEncryption.ManagedKey` the provider write request applies at `#AT_REST` (never a descriptor stamp); the Personal-only `DemandsEncryption` reads the `DataClassification.Personal`/`Credential`/`Secret` ceiling so a mandated-classification write on a `none`-provider profile is a typed `demand-breach` rejection.
- Receipt: every key-management event is one `EnvelopeFact` on the page-wide stream the operations thread through a `Func<EnvelopeFact, IO<Unit>> sink`, each `EnvelopeFact.Project()`-ing onto the one `Query/rail#INTERCEPTOR_SPINE` `StoreFact(Kind, Subject, Count, Elapsed, At)` envelope the `ReceiptSinkPort` drains — a DEK mint rides `store.encryption.mint` (key ARN subject, version-1 count), an unwrap `store.encryption.unwrap`, a rewrap `store.encryption.rewrap` (the advanced version), a TDE verify `store.encryption.verify`, and the four typed-fault breaches ride `store.encryption.demand-breach`/`store.encryption.aad-mismatch`/`store.encryption.integrity-breach`/`store.encryption.key-disabled` — each carrying a typed `EncryptionFault`, never silent, and an operation that emits no fact (a probe/mint/unwrap with no `sink` call) is the deleted form (an unobserved key-management event).
- Packages: AWSSDK.KeyManagementService, Azure.Security.KeyVault.Keys, Google.Cloud.Kms.V1, System.IO.Hashing, Microsoft.Data.Sqlite, Npgsql, AWSSDK.S3, Azure.Storage.Blobs, Microsoft.Extensions.Compliance.Redaction, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project)
- Growth: a new cloud-KMS custody is one `KmsProvider` row with its `WrapKind`/`AadBinding`/`RotationKind`/`Integrity` columns; a new keying scope is one `EnvelopeScope` case; a new rotation cadence is one column on `RotationPolicy`; a new evidence bucket is one `EnvelopeFactKind` row; a new failure cause is one `EncryptionFault` case; a new key-lifecycle state is one `KeyState` row; zero new surface — a second key store, a hand-rolled token cache, a per-provider keying class, or a `bool isNativeWrap` flag beside the closed mechanism axes is the deleted form.
- Boundary: the provider mechanism is a `[SmartEnum]` policy-value axis, not a uniform prose assertion — the AWS/GCP arms wrap through `Encrypt`/`GenerateDataKey` (no native wrap verb) while the Azure arm wraps through native `WrapKey`, so a page-wide "KMS has no native wrap verb" claim is the rejected illusory uniformity and the per-arm `WrapKind`/`RotationKind` row is the truth; the concrete KMS client (`IAmazonKeyManagementService`/`CryptographyClient`/`KeyManagementServiceClient`) is constructed once at composition per configured key and never per operation, so a per-operation client is the deleted form; the plaintext DEK never persists, rides a `DekLease` whose `Dispose` zeroes the span through `CryptographicOperations.ZeroMemory`, and a persisted plaintext DEK is the deleted form; the `EnvelopeAad` binds the store partition (and tenant id under RLS, rendered through `TenantContext.TenantId.Uuid` — never a second tenant-id spelling) on every wrap and unwrap and the `AadDigest` is the application-side check the Azure `application` arm enforces because the native wrap carries no AAD parameter, so an unwrap whose AAD diverges is `AadMismatch` regardless of arm and a cross-partition blob cannot decrypt; the GCP `Integrity = checksum` arm verifies the Cloud KMS integrity protocol bidirectionally — the request-integrity `EncryptResponse.VerifiedPlaintextCrc32C`/`VerifiedAdditionalAuthenticatedDataCrc32C` flags AND the response-value `PlaintextCrc32C`/`CiphertextCrc32C` against the local `System.IO.Hashing.Crc32` on every wrap and unwrap — so a corruption either direction is `IntegrityBreach` rather than a trusted partial — a GCP arm that checks only the response-value checksum (and silently trusts the request leg) is the deleted form; the key-state `Probe` rejects a wrap against a `Disabled`/`DestroyScheduled`/`PendingImport` key as `KeyDisabled` at admission so a deferred KMS state-throw never surfaces mid-write; the PG arm is verification-only — a managed TDE-key-set or `ALTER SYSTEM` is the rejected form, the deploy image's PG and its volume own the at-rest bytes and `#AT_REST` hands `ClusterConfig.Verify` the `data_encryption` fragment as a `(setting, value, fallback)` triple exactly as the sibling recovery/two-phase consumers do; the object arm composes the settled `BlobRemote` SSE-KMS column and never a second object-store client; the KMS-unwrap handle crosses from the AppHost runtime per open through the `[PORT]: KMS-unwrap port` seam as one `SecretLease`-class content carrier whose acquire-renew-zeroize custody `Runtime/config#SECRET_LEASE` owns, so the in-process key-handle lifecycle is the runtime lease's concern, this owner binds its `EnvelopeKeyring` quartet against the lease-managed CMK access and consumes the resolved per-open handle, never minting a long-lived in-process key and never a Persistence-side credential lifecycle; the concrete `KmsProvider` client and the AAD binding stay this owner's; a multi-cause encryption domain folded onto a bare `Error.New` is the deleted form — every failure is one `EncryptionFault` case deriving from `Expected`.

```csharp signature
// --- [TYPES] ----------------------------------------------------------------------------

// The four per-provider mechanism axes — each a closed [SmartEnum] policy value so the keyring reads the
// arm's behaviour off the KmsProvider row, never a `bool isNativeWrap`/`bool verifyCrc` flag soup nor a
// universal prose assertion. `WrapKind.Native` is Azure's first-class `WrapKey` verb; `WrapKind.Encrypt`
// is the AWS/GCP `Encrypt`/`GenerateDataKey`-as-wrap shim. The keys ARE the canonical wire spelling.
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class WrapKind {
    public static readonly WrapKind Native = new("native");    // Azure CryptographyClient.WrapKey/UnwrapKey
    public static readonly WrapKind Encrypt = new("encrypt");   // AWS/GCP Encrypt/Decrypt-as-wrap (no native wrap verb)
}

// Where the per-partition AAD is enforced. `Context` rides the provider's exact-match `EncryptionContext`
// (AWS) / `AdditionalAuthenticatedData` (GCP) so a foreign-partition blob cannot decrypt AT KMS; the Azure
// `WrapKey`/`UnwrapKey` verb carries no native AAD parameter, so `Application` threads the same EnvelopeAad
// and compares the persisted AadDigest application-side at the boundary.
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class AadBinding {
    public static readonly AadBinding Context = new("context");           // provider-enforced (AWS/GCP)
    public static readonly AadBinding Application = new("application");    // boundary-compared digest (Azure)
}

// How a rotation advances the key. `Rewrap` rewrites the persisted blob (AWS `ReEncrypt`, Azure re-wrap
// under the new version) so the plaintext DEK never re-enters managed memory; `Repoint` leaves every stored
// blob untouched (GCP `UpdateCryptoKeyPrimaryVersion`) because `Decrypt` resolves the embedded version, so
// a rotation is a primary repoint, never a decrypt-then-encrypt round trip.
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class RotationKind {
    public static readonly RotationKind Rewrap = new("rewrap");     // AWS ReEncrypt / Azure version re-wrap
    public static readonly RotationKind Repoint = new("repoint");   // GCP UpdateCryptoKeyPrimaryVersion
}

// Whether the arm verifies a transport checksum. The GCP `EncryptResponse`/`DecryptResponse` carry the
// load-bearing CRC32C integrity field (`PlaintextCrc32C`/`CiphertextCrc32C`), so `Checksum` verifies every
// payload against the local `System.IO.Hashing.Crc32` and a mismatch is an `IntegrityBreach`, never trusted;
// AWS/Azure return no caller-verifiable checksum, so `Transport` trusts TLS+KMS-side integrity.
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class Integrity {
    public static readonly Integrity Checksum = new("checksum");     // GCP CRC32C-verified
    public static readonly Integrity Transport = new("transport");   // AWS/Azure transport-trusted
}

// The key's lifecycle state resolved by `Probe` from the provider's own state surface
// (`DescribeKey.KeyState` / `KeyProperties.Enabled` / `CryptoKeyVersionState`). A wrap against any non-Enabled
// state is a `KeyDisabled` admission reject so a deferred KMS state-throw never surfaces mid-write.
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class KeyState {
    public static readonly KeyState Enabled = new("enabled");
    public static readonly KeyState Disabled = new("disabled");
    public static readonly KeyState DestroyScheduled = new("destroy-scheduled");
    public static readonly KeyState Unknown = new("unknown");

    public bool Usable => this == Enabled;
}

// The mint modality. `Local` derives a fresh DEK and returns its plaintext for the local at-rest cipher
// (AWS `GenerateDataKey`); `ReadOnlyNode` mints only the wrapped blob the read path unwraps later (AWS
// `GenerateDataKeyWithoutPlaintext`), the minting node never performing local encryption — one entrypoint,
// two modalities, never two mint methods.
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class MintMode {
    public static readonly MintMode Local = new("local");
    public static readonly MintMode ReadOnlyNode = new("read-only-node");

    public bool YieldsPlaintext => this == Local;
}

// The closed evidence axis on the page-wide fact stream, keyed in the `store.encryption.*` family the
// `Query/rail#INTERCEPTOR_SPINE` `StoreFact` envelope drains alongside `store.object.*`/`store.bulk` — the
// key IS the `StoreFact.Kind` wire spelling, never a bare local token a second projection re-spells.
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class EnvelopeFactKind {
    public static readonly EnvelopeFactKind Mint = new("store.encryption.mint");
    public static readonly EnvelopeFactKind Unwrap = new("store.encryption.unwrap");
    public static readonly EnvelopeFactKind Rewrap = new("store.encryption.rewrap");
    public static readonly EnvelopeFactKind Verify = new("store.encryption.verify");
    public static readonly EnvelopeFactKind DemandBreach = new("store.encryption.demand-breach");
    public static readonly EnvelopeFactKind AadMismatch = new("store.encryption.aad-mismatch");
    public static readonly EnvelopeFactKind IntegrityBreach = new("store.encryption.integrity-breach");
    public static readonly EnvelopeFactKind KeyDisabled = new("store.encryption.key-disabled");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EnvelopeScope {
    private EnvelopeScope() { }

    public sealed record SqliteDek(string Store) : EnvelopeScope;
    public sealed record PgTde(string Database) : EnvelopeScope;
    public sealed record ObjectSse(string Bucket, string Region) : EnvelopeScope;
}

// --- [MODELS] ---------------------------------------------------------------------------

// The per-partition additional-authenticated-data the wrap binds, admitted ONCE from the store partition
// identity and (under RLS) the tenancy-owned `TenantContext.TenantId.Uuid` canonical spelling — never a
// decimal UInt128 nor a second tenant-id transcription. `Context` is the provider AAD map (`EncryptionContext`
// keys / `AdditionalAuthenticatedData` bytes); `Digest` is the `XxHash128` the `KeyEnvelope` persists so the
// Azure `application` arm — whose `WrapKey` carries no AAD — compares mint-vs-unwrap at the boundary.
public sealed record EnvelopeAad(FrozenDictionary<string, string> Context) {
    public static EnvelopeAad Partition(string partition, Option<TenantId> tenant) =>
        new(tenant.Match(
            Some: id => new Dictionary<string, string> { ["partition"] = partition, ["tenant"] = id.Uuid },
            None: () => new Dictionary<string, string> { ["partition"] = partition })
            .ToFrozenDictionary(StringComparer.Ordinal));

    // The canonical wire bytes the provider AAD parameter and the digest both read: the sorted key=value
    // pairs joined, so the digest is stable across dictionary iteration order and matches across mint/unwrap.
    public ReadOnlyMemory<byte> Bytes =>
        Encoding.UTF8.GetBytes(string.Join('\n', Context.OrderBy(static kv => kv.Key, StringComparer.Ordinal).Map(static kv => $"{kv.Key}={kv.Value}")));

    public UInt128 Digest => XxHash128.HashToUInt128(Bytes.Span);
}

// One version on the key ladder `RotationPolicy.RetainVersions` bounds: the wrapped blob, the key ARN it
// wraps under, the exact AWS key material that wrapped it, and the mint instant. A multi-version read walks
// `KeyEnvelope.History` newest-first so a blob wrapped under a retired-but-retained version still unwraps,
// while a repoint-rotated (GCP) envelope keeps one entry because the embedded version decrypts. `Material`
// is the `GenerateDataKeyResponse.KeyMaterialId`/`ReEncryptResponse.KeyMaterialId` the AWS arm returns — under
// KMS automatic annual material rotation one ARN spans many materials, so the material id is the only
// forensic discriminator of which key bytes wrapped this version (`None` for Azure/GCP, which expose none).
public readonly record struct KeyVersion(ReadOnlyMemory<byte> WrappedDek, string KeyArn, Option<string> Material, int Version, Instant MintedAt);

public sealed record RotationPolicy(Duration Interval, int RetainVersions) {
    public static readonly RotationPolicy Quarterly = new(Duration.FromDays(90), RetainVersions: 2);

    public bool Due(Instant minted, Instant now) => now - minted >= Interval;

    // The retained-version window: the rewrap drops every version older than the newest `RetainVersions`,
    // so a stored blob under a still-retained version unwraps while history never grows unbounded.
    public Seq<KeyVersion> Retain(Seq<KeyVersion> history) =>
        history.OrderByDescending(static v => v.Version).Take(RetainVersions).ToSeq();
}

// The wrapped-DEK record: its provider, scope, the active key ARN, the AAD digest the unwrap re-checks, the
// probed key-state, the rotation policy, and the retained version ladder. `WrappedDek`/`KeyArn`/`Version`
// project the head of `History` so a single-version consumer reads them directly while a multi-version read
// walks the ladder; `AadDigest` is the application-side AAD check for the native-wrap (Azure) arm.
public sealed record KeyEnvelope(
    KmsProvider Provider, EnvelopeScope Scope, Seq<KeyVersion> History, UInt128 AadDigest,
    KeyState State, RotationPolicy Rotation, Instant MintedAt) {
    public ReadOnlyMemory<byte> WrappedDek => History.Head.WrappedDek;
    public string KeyArn => History.Head.KeyArn;
    public int Version => History.Head.Version;
}

// The borrowed plaintext DEK: an `IDisposable` whose `Dispose` zeroes the rented buffer through
// `CryptographicOperations.ZeroMemory`, so the unwrap consumer binds the key span inside a `using` and the
// plaintext never outlives the local bind. A `ReadOnlyMemory<byte>`-returning unwrap that leaves zeroization
// to the caller is the deleted form.
public sealed class DekLease(byte[] material) : IDisposable {
    public ReadOnlyMemory<byte> Dek => material;
    public void Dispose() => CryptographicOperations.ZeroMemory(material);
}

// The provider-neutral wrap/unwrap/rewrap/probe quartet one KmsProvider row projects from a resolved client.
// `Mint` returns the wrapped blob, the key ARN, the probed state, and (under MintMode.Local) the plaintext
// DEK; `Unwrap` returns the recovered plaintext having verified the arm's Integrity and AAD; `Rewrap`
// advances the key per RotationKind; `Probe` resolves the live KeyState. The AWS/Azure/GCP arms bind the
// same four delegates against their own members, so the keyring is the single dense surface and a
// per-provider keying class is the deleted form.
public sealed record EnvelopeKeyring(
    Func<EnvelopeAad, MintMode, IO<(byte[] Plaintext, ReadOnlyMemory<byte> Wrapped, string KeyArn, Option<string> Material, KeyState State)>> Mint,
    Func<ReadOnlyMemory<byte>, EnvelopeAad, IO<byte[]>> Unwrap,
    Func<ReadOnlyMemory<byte>, string, EnvelopeAad, IO<(ReadOnlyMemory<byte> Wrapped, string KeyArn, Option<string> Material)>> Rewrap,
    Func<IO<KeyState>> Probe);

[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
[KeyMemberComparer<StoreKeyPolicy, string>]
public sealed partial class KmsProvider {
    public static readonly KmsProvider AwsKms = new("aws-kms", WrapKind.Encrypt, AadBinding.Context, RotationKind.Rewrap, Integrity.Transport);
    public static readonly KmsProvider AzureKeyVault = new("azure-keyvault", WrapKind.Native, AadBinding.Application, RotationKind.Rewrap, Integrity.Transport);
    public static readonly KmsProvider GcpKms = new("gcp-kms", WrapKind.Encrypt, AadBinding.Context, RotationKind.Repoint, Integrity.Checksum);
    public static readonly KmsProvider None = new("none", WrapKind.Encrypt, AadBinding.Application, RotationKind.Rewrap, Integrity.Transport);

    public WrapKind Wrap { get; }
    public AadBinding Aad { get; }
    public RotationKind Rotation { get; }
    public Integrity Integrity { get; }

    public bool Custodies => this != None;
}

// One fact per key-management event on the page-wide stream — the mint/unwrap/rewrap/verify proofs and the
// four typed-fault breaches all ride one record with `EnvelopeFactKind` slot metadata, never four parallel
// status buckets. `Of` stamps the elapsed/instant from the settled `ClockPolicy`; `Project()` lands it on the
// one `Query/rail#INTERCEPTOR_SPINE` `StoreFact(Kind, Subject, Count, Elapsed, At)` envelope the receipt sink
// drains — `KeyArn` is the subject and `Version` rides `Count`, so a key ARN never reaches a signal unredacted
// past the redactor seam the `StoreFact` envelope owns.
public readonly record struct EnvelopeFact(EnvelopeFactKind Kind, EnvelopeScope Scope, string KeyArn, int Version, Duration Elapsed, Instant At) {
    public static EnvelopeFact Of(EnvelopeFactKind kind, EnvelopeScope scope, string keyArn, int version, ClockPolicy clocks, Duration elapsed) =>
        new(kind, scope, keyArn, version, elapsed, clocks.Now);

    public StoreFact Project() => new(Kind.Key, KeyArn, Version, Elapsed, At);
}

// --- [ERRORS] ---------------------------------------------------------------------------

// The closed encryption-fault family deriving from Expected — a bare `Error.New` for this multi-cause
// domain is the deleted form. `DemandBreach` is a mandated-classification write on a plaintext profile;
// `AadMismatch` an unwrap whose AAD diverges from mint (cross-partition / cross-tenant); `IntegrityBreach`
// a GCP CRC32C divergence; `KeyDisabled` a wrap against a non-Enabled key; `UnwrapRejected` a KMS-side
// decrypt refusal; `TdeUnverified` a failed `data_encryption` GUC verification; recovery is code-keyed.
[Union]
public abstract partial record EncryptionFault : Expected {
    private EncryptionFault(string detail, int code) : base(detail, code, None) { }

    public sealed record DemandBreach : EncryptionFault { public DemandBreach(string classification) : base($"<encryption-demanded:{classification}>", 7610) { Classification = classification; } public string Classification { get; } }
    public sealed record AadMismatch : EncryptionFault { public AadMismatch(UInt128 minted, UInt128 presented) : base($"<aad-mismatch:{minted:x32}:{presented:x32}>", 7611) => (Minted, Presented) = (minted, presented); public UInt128 Minted { get; } public UInt128 Presented { get; } }
    public sealed record IntegrityBreach : EncryptionFault { public IntegrityBreach(string keyArn, uint expected, uint observed, Option<string> requestLeg = default) : base(requestLeg.Match(Some: leg => $"<crc32c-request-corrupt:{keyArn}:{leg}>", None: () => $"<crc32c-mismatch:{keyArn}:{expected:x8}:{observed:x8}>"), 7612) => (KeyArn, Expected, Observed, RequestLeg) = (keyArn, expected, observed, requestLeg); public string KeyArn { get; } public uint Expected { get; } public uint Observed { get; } public Option<string> RequestLeg { get; } }
    public sealed record KeyDisabled : EncryptionFault { public KeyDisabled(string keyArn, KeyState state) : base($"<key-unusable:{keyArn}:{state.Key}>", 7613) => (KeyArn, State) = (keyArn, state); public string KeyArn { get; } public KeyState State { get; } }
    public sealed record UnwrapRejected : EncryptionFault { public UnwrapRejected(string keyArn, string detail) : base($"<unwrap-rejected:{keyArn}:{detail}>", 7614) { KeyArn = keyArn; } public string KeyArn { get; } }
    public sealed record TdeUnverified : EncryptionFault { public TdeUnverified(string detail) : base($"<tde-unverified:{detail}>", 7615) { } }
    public sealed record Provider : EncryptionFault { public Provider(string provider, string detail) : base($"<kms-provider:{provider}:{detail}>", 7619) { } }

    public bool Admission => this is DemandBreach or KeyDisabled;

    // The one boundary fold: a provider exception becomes one closed case per arm. Azure
    // `RequestFailedException`, AWS `AmazonKeyManagementServiceException`/`DisabledException`, and GCP
    // `RpcException` convert here ONCE so no provider exception escapes the keyring delegates.
    public static EncryptionFault Lift(KmsProvider provider, string keyArn, Exception boundary) =>
        boundary switch {
            DisabledException or KMSInvalidStateException => new KeyDisabled(keyArn, KeyState.Disabled),
            AmazonKeyManagementServiceException aws => new UnwrapRejected(keyArn, aws.Message),
            RequestFailedException { Status: 403 or 409 } az => new KeyDisabled(keyArn, KeyState.Disabled),
            RequestFailedException az => new UnwrapRejected(keyArn, az.Message),
            Grpc.Core.RpcException { StatusCode: Grpc.Core.StatusCode.FailedPrecondition } => new KeyDisabled(keyArn, KeyState.Disabled),
            Grpc.Core.RpcException gcp => new UnwrapRejected(keyArn, gcp.Status.Detail),
            _ => new Provider(provider.Key, boundary.Message),
        };
}

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class Envelope {
    public static FrozenSet<DataClassification> DemandsEncryption =>
        new[] { DataClassification.Personal, DataClassification.Credential, DataClassification.Secret }.ToFrozenSet();

    // One polymorphic mint discriminating on MintMode, threading the row's WrapKind through the keyring
    // delegate and capturing the AAD digest + probed key-state onto the version ladder. The local plaintext
    // (MintMode.Local) is returned to the caller's cipher and zeroed there; ReadOnlyNode yields an empty
    // plaintext. A KeyDisabled probe short-circuits before any key derives. Mint returns BOTH the persisted
    // envelope AND the borrowed plaintext as a self-zeroing DekLease (empty under ReadOnlyNode) — the caller
    // binds the local cipher inside a `using` and the plaintext never leaks past the bind. Returning the
    // envelope alone and dropping the plaintext on the floor is the deleted form (a leaked or undelivered DEK).
    // The `sink` threads the page-wide EnvelopeFact stream so every mint rides `store.encryption.mint` and a
    // disabled-key short-circuit rides `store.encryption.key-disabled` — a probe/mint that emits no fact is the
    // deleted form (an unobserved key-management event).
    public static IO<(KeyEnvelope Envelope, DekLease Dek)> Mint(KmsProvider provider, EnvelopeScope scope, EnvelopeKeyring keyring, EnvelopeAad aad, RotationPolicy rotation, MintMode mode, Func<EnvelopeFact, IO<Unit>> sink, ClockPolicy clocks) =>
        from mark in IO.lift(clocks.Mark)
        from state in keyring.Probe()
        from minted in state.Usable
            ? keyring.Mint(aad, mode)
            : sink(EnvelopeFact.Of(EnvelopeFactKind.KeyDisabled, scope, "<mint>", 0, clocks, clocks.Elapsed(mark)))
                .Bind(_ => IO.fail<(byte[] Plaintext, ReadOnlyMemory<byte> Wrapped, string KeyArn, Option<string> Material, KeyState State)>(new EncryptionFault.KeyDisabled("<mint>", state)))
        from _ in sink(EnvelopeFact.Of(EnvelopeFactKind.Mint, scope, minted.KeyArn, 1, clocks, clocks.Elapsed(mark)))
        select (
            new KeyEnvelope(
                provider, scope,
                History: Seq(new KeyVersion(minted.Wrapped, minted.KeyArn, minted.Material, Version: 1, clocks.Now)),
                AadDigest: aad.Digest, State: minted.State, Rotation: rotation, MintedAt: clocks.Now),
            new DekLease(minted.Plaintext));

    // Recover the plaintext DEK as a self-zeroing DekLease, verifying the AAD digest first (the native-wrap
    // arm's only AAD check) so a foreign-partition unwrap rejects before the borrowed key ever materializes
    // to a caller. The keyring delegate owns the arm's Integrity (GCP CRC32C) verification internally; the
    // `sink` rides `store.encryption.unwrap` on success and `store.encryption.aad-mismatch` on divergence.
    public static IO<DekLease> Unwrap(KeyEnvelope envelope, EnvelopeKeyring keyring, EnvelopeAad aad, Func<EnvelopeFact, IO<Unit>> sink, ClockPolicy clocks) =>
        from mark in IO.lift(clocks.Mark)
        from plaintext in aad.Digest == envelope.AadDigest
            ? keyring.Unwrap(envelope.WrappedDek, aad)
            : sink(EnvelopeFact.Of(EnvelopeFactKind.AadMismatch, envelope.Scope, envelope.KeyArn, envelope.Version, clocks, clocks.Elapsed(mark)))
                .Bind(_ => IO.fail<byte[]>(new EncryptionFault.AadMismatch(envelope.AadDigest, aad.Digest)))
        from _ in sink(EnvelopeFact.Of(EnvelopeFactKind.Unwrap, envelope.Scope, envelope.KeyArn, envelope.Version, clocks, clocks.Elapsed(mark)))
        select new DekLease(plaintext);

    // Rotate per the row's RotationKind: `Rewrap` rewrites the blob and pushes a new version onto the
    // retained ladder; `Repoint` (GCP) leaves the blob untouched (the embedded version still decrypts) and
    // only re-stamps the head's mint instant. `RotationPolicy.Retain` bounds the ladder either way; the
    // `sink` rides `store.encryption.rewrap` carrying the advanced version.
    public static IO<KeyEnvelope> Rewrap(KeyEnvelope envelope, EnvelopeKeyring keyring, string destinationKeyId, EnvelopeAad aad, Func<EnvelopeFact, IO<Unit>> sink, ClockPolicy clocks) =>
        from mark in IO.lift(clocks.Mark)
        from advanced in envelope.Provider.Rotation == RotationKind.Repoint
            ? IO.pure(envelope with { History = envelope.Rotation.Retain(envelope.History), MintedAt = clocks.Now })
            : keyring.Rewrap(envelope.WrappedDek, destinationKeyId, aad).Map(rewrapped =>
                envelope with {
                    History = envelope.Rotation.Retain(envelope.History.Cons(new KeyVersion(rewrapped.Wrapped, rewrapped.KeyArn, rewrapped.Material, envelope.Version + 1, clocks.Now))),
                    MintedAt = clocks.Now,
                })
        from _ in sink(EnvelopeFact.Of(EnvelopeFactKind.Rewrap, advanced.Scope, advanced.KeyArn, advanced.Version, clocks, clocks.Elapsed(mark)))
        select advanced;

    // The classification-demand gate is a pure synchronous check — `Fin` (RAIL_CHOOSER: synchronous
    // fallibility), never `IO`, because no boundary work runs. A mandated-classification write on a plaintext
    // (`none`-provider) profile is the typed `DemandBreach`; the `store.encryption.demand-breach` fact rides
    // the egress fold the caller already threads, not a second sink on this pure predicate.
    public static Fin<KeyEnvelope> Demand(KeyEnvelope envelope, DataClassification classification) =>
        DemandsEncryption.Contains(classification) && !envelope.Provider.Custodies
            ? Fin.Fail<KeyEnvelope>(new EncryptionFault.DemandBreach(classification.Key))
            : Fin.Succ(envelope);
}
```

The keyring rows bind the provider-neutral quartet against each arm's verified members. The `Mint`/`Unwrap` AWS/GCP arms thread the AAD onto the provider parameter (`EncryptionContext` / `AdditionalAuthenticatedData`) and the GCP arm verifies CRC32C through `System.IO.Hashing.Crc32` before any payload crosses the boundary; the Azure arm wraps natively and the AAD is the application-side digest check `Envelope.Unwrap` already performs.

```csharp signature
public static class KeyringRows {
    // The AWS arm: GenerateDataKey (Local) / GenerateDataKeyWithoutPlaintext (ReadOnlyNode) with the
    // EncryptionContext exact-match AAD, Decrypt to unwrap, ReEncrypt to rotate, DescribeKey to probe. The
    // recovered Plaintext copies to a managed buffer and the response MemoryStream disposes immediately.
    public static EnvelopeKeyring Aws(IAmazonKeyManagementService client, string keyId) =>
        new(
            Mint: (aad, mode) => IO.liftAsync(async () => {
                var context = aad.Context.ToDictionary(static kv => kv.Key, static kv => kv.Value);
                if (mode.YieldsPlaintext) {
                    var key = await client.GenerateDataKeyAsync(new GenerateDataKeyRequest { KeyId = keyId, KeySpec = DataKeySpec.AES_256, EncryptionContext = context });
                    return (key.Plaintext.ToArray(), (ReadOnlyMemory<byte>)key.CiphertextBlob.ToArray(), key.KeyId, Optional(key.KeyMaterialId), KeyState.Enabled);
                }
                var wrapped = await client.GenerateDataKeyWithoutPlaintextAsync(new GenerateDataKeyWithoutPlaintextRequest { KeyId = keyId, KeySpec = DataKeySpec.AES_256, EncryptionContext = context });
                return (Array.Empty<byte>(), (ReadOnlyMemory<byte>)wrapped.CiphertextBlob.ToArray(), wrapped.KeyId, Optional(wrapped.KeyMaterialId), KeyState.Enabled);
            }).MapFail(boundary => (Error)EncryptionFault.Lift(KmsProvider.AwsKms, keyId, boundary)),
            Unwrap: (blob, aad) => IO.liftAsync(async () => {
                using var stream = new MemoryStream(blob.ToArray());
                var plain = await client.DecryptAsync(new DecryptRequest { CiphertextBlob = stream, EncryptionContext = aad.Context.ToDictionary(static kv => kv.Key, static kv => kv.Value) });
                return plain.Plaintext.ToArray();
            }).MapFail(boundary => (Error)EncryptionFault.Lift(KmsProvider.AwsKms, keyId, boundary)),
            Rewrap: (blob, destinationKeyId, aad) => IO.liftAsync(async () => {
                using var stream = new MemoryStream(blob.ToArray());
                var context = aad.Context.ToDictionary(static kv => kv.Key, static kv => kv.Value);
                var rewrapped = await client.ReEncryptAsync(new ReEncryptRequest { CiphertextBlob = stream, DestinationKeyId = destinationKeyId, SourceEncryptionContext = context, DestinationEncryptionContext = context });
                return ((ReadOnlyMemory<byte>)rewrapped.CiphertextBlob.ToArray(), rewrapped.KeyId, Optional(rewrapped.DestinationKeyMaterialId));
            }).MapFail(boundary => (Error)EncryptionFault.Lift(KmsProvider.AwsKms, keyId, boundary)),
            Probe: () => IO.liftAsync(async () => MapAwsState((await client.DescribeKeyAsync(keyId)).KeyMetadata.KeyState))
                .MapFail(boundary => (Error)EncryptionFault.Lift(KmsProvider.AwsKms, keyId, boundary)));

    // The Azure arm: the NATIVE WrapKey/UnwrapKey verb (RSA-OAEP-256) — no GenerateDataKey, no Encrypt-as-wrap.
    // A fresh DEK is generated locally (32 random bytes) then wrapped; rotation re-wraps under `destinationKeyId`'s
    // CryptographyClient; the AAD has no native vault parameter, so EnvelopeAad rides the application digest
    // `Envelope.Unwrap` checks. The key-state probe reads `KeyProperties.Enabled` off the bound vault key.
    public static EnvelopeKeyring Azure(CryptographyClient client, Func<string, CryptographyClient> forKey, Func<IO<KeyState>> probeState) =>
        new(
            Mint: (aad, mode) => IO.liftAsync(async () => {
                var dek = RandomNumberGenerator.GetBytes(32);
                var wrap = await client.WrapKeyAsync(KeyWrapAlgorithm.RsaOaep256, dek);
                return (Borrow(dek, mode), (ReadOnlyMemory<byte>)wrap.EncryptedKey, wrap.KeyId, Option<string>.None, KeyState.Enabled);
            }).MapFail(boundary => (Error)EncryptionFault.Lift(KmsProvider.AzureKeyVault, client.KeyId, boundary)),
            Unwrap: (blob, _) => IO.liftAsync(async () => (await client.UnwrapKeyAsync(KeyWrapAlgorithm.RsaOaep256, blob.ToArray())).Key)
                .MapFail(boundary => (Error)EncryptionFault.Lift(KmsProvider.AzureKeyVault, client.KeyId, boundary)),
            Rewrap: (blob, destinationKeyId, _) => IO.liftAsync(async () => {
                using var lease = new DekLease((await client.UnwrapKeyAsync(KeyWrapAlgorithm.RsaOaep256, blob.ToArray())).Key);
                var wrap = await forKey(destinationKeyId).WrapKeyAsync(KeyWrapAlgorithm.RsaOaep256, lease.Dek.ToArray());
                return ((ReadOnlyMemory<byte>)wrap.EncryptedKey, wrap.KeyId, Option<string>.None);
            }).MapFail(boundary => (Error)EncryptionFault.Lift(KmsProvider.AzureKeyVault, client.KeyId, boundary)),
            Probe: probeState);

    // The GCP arm: GenerateRandomBytes for the DEK then Encrypt with AdditionalAuthenticatedData + the
    // load-bearing BIDIRECTIONAL CRC32C integrity stack — the Cloud KMS protocol verifies BOTH directions:
    // the request-integrity flags (`EncryptResponse.VerifiedPlaintextCrc32C`/`VerifiedAdditionalAuthenticatedDataCrc32C`)
    // prove KMS received the request `PlaintextCrc32C`/`AdditionalAuthenticatedDataCrc32C` intact, and the
    // response-value CRC32C (`CiphertextCrc32C`/`PlaintextCrc32C`) proves the response returned intact against
    // the local System.IO.Hashing.Crc32. A `false` verified flag means the request corrupted in transit; a
    // value divergence means the response corrupted — both are an `IntegrityBreach`, never a trusted partial.
    // Rotation is a primary REPOINT (UpdateCryptoKeyPrimaryVersion), so Rewrap is unreachable for this arm
    // (Envelope.Rewrap short-circuits on RotationKind.Repoint); the probe reads the primary CryptoKeyVersionState.
    // The provider call rides IO.liftAsync (Lift-mapping real RpcExceptions); both integrity verdicts are pure
    // `Bind`-level guards returning the typed IntegrityBreach onto the rail, never a thrown exception the
    // generic Lift would mis-map to Provider — so the integrity fault survives as its own closed case.
    public static EnvelopeKeyring Gcp(KeyManagementServiceClient client, CryptoKeyName key) =>
        new(
            Mint: (aad, mode) =>
                from dek in IO.liftAsync(async () => (await client.GenerateRandomBytesAsync($"projects/{key.ProjectId}/locations/{key.LocationId}", 32, ProtectionLevel.Hsm)).Data.ToByteArray())
                    .MapFail(boundary => (Error)EncryptionFault.Lift(KmsProvider.GcpKms, key.ToString(), boundary))
                from wrapped in IO.liftAsync(() => client.EncryptAsync(EncryptArgs(key, dek, aad)))
                    .MapFail(boundary => (Error)EncryptionFault.Lift(KmsProvider.GcpKms, key.ToString(), boundary))
                from request in VerifyRequest(wrapped.VerifiedPlaintextCrc32C, wrapped.VerifiedAdditionalAuthenticatedDataCrc32C, key)
                from response in Verify(wrapped.CiphertextCrc32C, Crc32.HashToUInt32(wrapped.Ciphertext.Span), key)
                select (Borrow(dek, mode), (ReadOnlyMemory<byte>)wrapped.Ciphertext.ToByteArray(), key.ToString(), Option<string>.None, KeyState.Enabled),
            Unwrap: (blob, aad) =>
                from plain in IO.liftAsync(() => client.DecryptAsync(DecryptArgs(key, blob, aad)))
                    .MapFail(boundary => (Error)EncryptionFault.Lift(KmsProvider.GcpKms, key.ToString(), boundary))
                from response in Verify(plain.PlaintextCrc32C, Crc32.HashToUInt32(plain.Plaintext.Span), key)
                select plain.Plaintext.ToByteArray(),
            Rewrap: (_, _, _) => IO.fail<(ReadOnlyMemory<byte>, string, Option<string>)>(new EncryptionFault.Provider("gcp-kms", "<repoint-not-rewrap>")),
            Probe: () => IO.liftAsync(async () => MapGcpState((await client.GetCryptoKeyAsync(key)).Primary?.State))
                .MapFail(boundary => (Error)EncryptionFault.Lift(KmsProvider.GcpKms, key.ToString(), boundary)));

    static EncryptRequest EncryptArgs(CryptoKeyName key, byte[] dek, EnvelopeAad aad) {
        var aadBytes = ByteString.CopyFrom(aad.Bytes.ToArray());
        return new EncryptRequest {
            ResourceName = key.ToString(), Plaintext = ByteString.CopyFrom(dek),
            PlaintextCrc32C = Crc32.HashToUInt32(dek), AdditionalAuthenticatedData = aadBytes,
            AdditionalAuthenticatedDataCrc32C = Crc32.HashToUInt32(aad.Bytes.Span),
        };
    }

    static DecryptRequest DecryptArgs(CryptoKeyName key, ReadOnlyMemory<byte> blob, EnvelopeAad aad) =>
        new() {
            Name = key.ToString(), Ciphertext = ByteString.CopyFrom(blob.ToArray()),
            CiphertextCrc32C = Crc32.HashToUInt32(blob.Span), AdditionalAuthenticatedData = ByteString.CopyFrom(aad.Bytes.ToArray()),
            AdditionalAuthenticatedDataCrc32C = Crc32.HashToUInt32(aad.Bytes.Span),
        };

    // The GCP CRC32C verdict on the rail: the response carries a `long?` wrapper; a null (KMS returned no
    // checksum) or a divergence from the locally computed Crc32 fails onto the typed IntegrityBreach case
    // directly, so a corrupt payload never crosses the boundary and the fault is never a mis-mapped Provider.
    static IO<Unit> Verify(long? reported, uint local, CryptoKeyName key) =>
        reported is { } want && (uint)want == local
            ? IO.pure(unit)
            : IO.fail<Unit>(new EncryptionFault.IntegrityBreach(key.ToString(), (uint)(reported ?? 0L), local));

    // The GCP request-integrity verdict: the EncryptResponse `Verified*Crc32C` flags confirm KMS received the
    // request `PlaintextCrc32C`/`AdditionalAuthenticatedDataCrc32C` intact. A `false` flag means that request
    // leg corrupted in transit (the response value check cannot catch this — it covers only the return leg), so
    // the DEK was wrapped over corrupted bytes and the wrap is discarded as a typed IntegrityBreach naming the
    // corrupted leg (`plaintext`/`aad`) rather than a misleading `0:0` value pair.
    static IO<Unit> VerifyRequest(bool plaintextVerified, bool aadVerified, CryptoKeyName key) =>
        plaintextVerified && aadVerified
            ? IO.pure(unit)
            : IO.fail<Unit>(new EncryptionFault.IntegrityBreach(key.ToString(), 0u, 0u, plaintextVerified ? "aad" : "plaintext"));

    // Borrow the freshly minted DEK per modality: MintMode.Local hands the plaintext to the caller's cipher
    // (zeroed there by the DekLease), MintMode.ReadOnlyNode keeps only the wrapped blob and zeroes the local
    // copy here so a minting read-replica never retains plaintext. The AWS arm's GenerateDataKey already
    // splits Local/ReadOnlyNode at the request verb, so this borrow owns the Azure/GCP locally-derived DEK.
    static byte[] Borrow(byte[] dek, MintMode mode) {
        if (mode.YieldsPlaintext) { return dek; }
        CryptographicOperations.ZeroMemory(dek);
        return [];
    }

    // Map the AWS KeyState constant-class (fully qualified to avoid the name-collision with this page's own
    // KeyState smart-enum) to the closed key-lifecycle axis: only `Enabled` is usable, `Disabled`/`Unavailable`/
    // `PendingImport` reject as unusable, a pending-deletion state is the destroy-scheduled rung, and the
    // transient `Creating`/`Updating` states fall to `Unknown` (the probe rejects them, retryable). A stub
    // returning Enabled regardless of the argument is the hollow form the probe rejects.
    static KeyState MapAwsState(Amazon.KeyManagementService.KeyState state) =>
        state == Amazon.KeyManagementService.KeyState.Enabled ? KeyState.Enabled
        : state == Amazon.KeyManagementService.KeyState.PendingDeletion || state == Amazon.KeyManagementService.KeyState.PendingReplicaDeletion ? KeyState.DestroyScheduled
        : state == Amazon.KeyManagementService.KeyState.Disabled || state == Amazon.KeyManagementService.KeyState.Unavailable || state == Amazon.KeyManagementService.KeyState.PendingImport ? KeyState.Disabled
        : KeyState.Unknown;

    static KeyState MapGcpState(CryptoKeyVersion.Types.CryptoKeyVersionState? state) =>
        state switch {
            CryptoKeyVersion.Types.CryptoKeyVersionState.Enabled => KeyState.Enabled,
            CryptoKeyVersion.Types.CryptoKeyVersionState.Disabled => KeyState.Disabled,
            CryptoKeyVersion.Types.CryptoKeyVersionState.Destroyed or CryptoKeyVersion.Types.CryptoKeyVersionState.DestroyScheduled => KeyState.DestroyScheduled,
            _ => KeyState.Unknown,
        };
}
```

## [03]-[SQLITE_KEYING]

- Owner: `EncryptionGate` the promoted SQLCipher keying record (relocated from `Store/engine#EXTENSION_GATES`) carrying its `PRAGMA key`/`cipher_migrate`/`cipher_version`/`rekey` ceremony rows; `SqliteKeying` the static surface binding the ceremony to a KMS-unwrapped DEK supplied through the `EnvelopeKeyring` quartet.
- Entry: `public static IO<Seq<SqliteFact>> Open(SqliteConnection connection, KeyEnvelope envelope, EnvelopeKeyring keyring, EnvelopeAad aad, Func<EnvelopeFact, IO<Unit>> sink, ClockPolicy clocks)` — threads the page-wide `EnvelopeFact` sink into `Envelope.Unwrap` so the KMS unwrap rides `store.encryption.unwrap`, unwraps the DEK into a `DekLease`, applies `PRAGMA key` with the unwrapped key as the first statement on the physical open inside the `using`, runs `cipher_migrate` for a legacy-format file, reads `cipher_version` as a fact, and disposes the lease so the recovered plaintext is zeroed once the pragma has consumed the key span; `public static IO<SqliteFact> Rekey(SqliteConnection connection, DekLease nextDek, ClockPolicy clocks)` runs `PRAGMA rekey` to re-cipher the file under a rotated DEK and disposes the lease.
- Auto: the SQLCipher gate moves out of `ExtensionGateState.Research` to `ExtensionGateState.Gated` (`Store/engine#EXTENSION_GATES`, pending the `SQLitePCLRaw.bundle_e_sqlcipher` admission and the externally supplied native-library proof the `[RESEARCH]` SQLCIPHER_KEYING item closes) — its key source is no longer a static passphrase but the `EnvelopeKeyring` unwrap delegate, so `Open` unwraps the KMS-wrapped DEK (verifying the arm's AAD digest and CRC32C through `Envelope.Unwrap`), applies `PRAGMA key` as the first statement before any user statement (or the file is unreadable), and the `DekLease.Dispose` zeroes the recovered plaintext once the pragma has consumed the key span; `cipher_migrate` upgrades a legacy SQLCipher file format and `cipher_version` receipts the cipher provider version; `Rekey` runs `PRAGMA rekey` so a `RotationPolicy`-due rotation re-ciphers the embedded file under the rotated DEK in place; the `SQLite3Provider_sqlcipher` provider replaces the default `e_sqlite3` bundle on the encrypted profile and the two routes never mix; the gate arms only on a Personal/Credential/Secret-classified embedded store so a plaintext-default store never pays the cipher cost.
- Receipt: the keying ceremony rides `SqliteFactKind.ExtensionLoad`/`Registration` facts on the existing sqlite fact stream carrying the cipher version (never the key); a rekey rides a rotation fact; the `EnvelopeFact` `unwrap`/`rewrap` facts carry the envelope evidence.
- Packages: Microsoft.Data.Sqlite, the `SQLite3Provider_sqlcipher` provider bundle (`SQLitePCLRaw.bundle_e_sqlcipher` — NOT yet on the admitted manifest, which carries only `SQLitePCLRaw.bundle_e_sqlite3`; the central pin plus the externally supplied `osx-arm64` SQLCipher native library land with the `[RESEARCH]` SQLCIPHER_KEYING resolution before this arm realizes), AWSSDK.KeyManagementService, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime.
- Growth: a new ceremony pragma is one `Ceremony` row; a new keying provider is the `KmsProvider` axis above; zero new surface — a static-passphrase keying, a connection-string `Password` knob, or a second cipher provider is the deleted form because the DEK is KMS-unwrapped and the ceremony rides the one `EncryptionGate`.
- Boundary: the SQLCipher gate moves from Research to `Gated` by binding its ceremony to the KMS-unwrapped DEK (the `SQLitePCLRaw.bundle_e_sqlcipher` central pin and the externally supplied native library land with the `[RESEARCH]` SQLCIPHER_KEYING resolution) — a static-passphrase key source is the deleted form, and the `Password` connection-string row (the admitted `e_sqlite3` bundle has no cipher and fails at open) is the rejected spelling; the DEK arrives unwrapped as a `DekLease` per open and is zeroed by its `Dispose` immediately after `PRAGMA key`, never persisted; `PRAGMA key` is the first statement on the physical open before any user statement or the file is unreadable; the `SQLite3Provider_sqlcipher` provider replaces the default bundle on the encrypted profile (the batteries bundle route is deprecated and the two never mix); `Rekey` re-ciphers in place under a rotated DEK so a key rotation never re-exports the store; the gate arms only on a mandated-classification embedded store per `Envelope.Demand` so a plaintext-default store stays unencrypted at zero cost; this keying composes the `Store/engine#PRAGMA_TABLE` ladder — `PRAGMA key` precedes the PRAGMA ladder so the cipher is armed before the engine knobs apply.

```csharp signature
public sealed record EncryptionGate(string GateRow, FrozenSet<DataClassification> Mandating, Seq<string> Ceremony) {
    public static readonly EncryptionGate Sqlcipher = new(
        GateRow: "sqlcipher",
        Mandating: new[] { DataClassification.Personal, DataClassification.Credential, DataClassification.Secret }.ToFrozenSet(),
        Ceremony: Seq("PRAGMA key", "PRAGMA cipher_migrate", "PRAGMA cipher_version", "PRAGMA rekey"));
}

public static class SqliteKeying {
    public static IO<Seq<SqliteFact>> Open(
        SqliteConnection connection, KeyEnvelope envelope, EnvelopeKeyring keyring,
        EnvelopeAad aad, Func<EnvelopeFact, IO<Unit>> sink, ClockPolicy clocks) =>
        Envelope.Unwrap(envelope, keyring, aad, sink, clocks).Bind(lease => IO.lift(() => {
            // BOUNDARY ADAPTER — the SQLCipher key ceremony; the DekLease zeroes the borrowed key after PRAGMA key.
            using (lease) {
                var mark = clocks.Mark();
                using var key = connection.CreateCommand();
                key.CommandText = $"PRAGMA key = \"x'{Convert.ToHexString(lease.Dek.Span)}'\"; PRAGMA cipher_migrate;";
                _ = key.ExecuteNonQuery();
                using var version = connection.CreateCommand();
                version.CommandText = "PRAGMA cipher_version;";
                var cipher = Optional(version.ExecuteScalar()).Map(static v => v.ToString() ?? "");
                return Seq(new SqliteFact(SqliteFactKind.ExtensionLoad, "sqlcipher", None, cipher, Count: 0, Bytes: 0, clocks.Elapsed(mark), clocks.Now));
            }
        }));

    public static IO<SqliteFact> Rekey(SqliteConnection connection, DekLease nextDek, ClockPolicy clocks) =>
        IO.lift(() => {
            using (nextDek) {
                var mark = clocks.Mark();
                using var rekey = connection.CreateCommand();
                rekey.CommandText = $"PRAGMA rekey = \"x'{Convert.ToHexString(nextDek.Dek.Span)}'\";";
                _ = rekey.ExecuteNonQuery();
                return new SqliteFact(SqliteFactKind.Registration, "sqlcipher-rekey", None, None, Count: 0, Bytes: 0, clocks.Elapsed(mark), clocks.Now);
            }
        });
}
```

## [04]-[AT_REST]

- Owner: `AtRest` the verify-only PG-TDE surface and the object-store SSE-KMS feed — `PgTde` hands the `data_encryption` GUC fragment to the `Store/provisioning#CLUSTER_CONFIG` `ClusterConfig.Verify` fold exactly as the sibling recovery/two-phase consumers hand their fragments, and `ObjectSse` projects the settled `Store/remote#OBJECT_STORE` `ObjectEncryption.ManagedKey` the remote multipart seal already consumes, feeding it the envelope axis's CMK ARN and the per-partition AAD.
- Cases: the PG arm verifies `data_encryption=on` where the deploy image's PG carries it (else the at-rest posture is the filesystem/volume encryption the deploy owns) read-only — a `Degradable` `Enumeration` fragment so an image without the TDE GUC degrades to volume encryption rather than aborting; the object arm projects `ObjectEncryption.ManagedKey(keyArn, aad)` so the remote tier's per-arm seal (S3 `aws:kms`+key id+`EncryptionContext`, Azure named `EncryptionScope`, GCS `KmsKeyName`) encrypts under the same provider key without minting a second SSE vocabulary.
- Entry: `public static Fin<GucVerdict> VerifyPg(FrozenDictionary<string, GucReading> observed)` folds the `data_encryption` fragment through `ClusterConfig.Verify`, `.MapFail`ing an abort into `EncryptionFault.TdeUnverified`; `public static ObjectEncryption ObjectSse(string keyArn, EnvelopeAad aad)` projects the settled `Store/remote#OBJECT_STORE` `ObjectEncryption.ManagedKey` carrying the CMK ARN and the per-partition AAD the remote seal applies, never a parallel SSE type.
- Auto: the PG arm never holds a DEK — TDE is an initdb/deploy-image decision the cluster owns, so this owner verifies the GUC read-only and routes a `pending_restart` or fallback through the `GucVerdict` degrade receipts rather than minting a key; the object arm feeds the settled `Store/remote#OBJECT_STORE` `ObjectEncryption.ManagedKey` whose comment already states "the key-id and AAD arrive from the envelope", so the multipart upload sets SSE-KMS under the same `KmsProvider` key the envelope axis custodies on the provider write request, never on the content-identity descriptor and never a second object-store client or a second SSE union; a deploy image whose PG lacks `data_encryption` degrades to the filesystem/volume encryption the deploy owns, recorded as a degrade receipt, never a silent plaintext cluster.
- Receipt: a TDE verify rides `envelope.verify` carrying the `GucVerdict` matched/degraded set; an object SSE stance rides the `ObjectTransferFact` the remote tier already mints.
- Packages: Npgsql, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new at-rest GUC is one `GucRow` triple handed to `ClusterConfig.Verify`; a new SSE stance is one `Store/remote#OBJECT_STORE` `ObjectEncryption` case (owned there, not here); zero new surface.
- Boundary: the PG arm is verification-only — a managed TDE-key-set or `ALTER SYSTEM` is the rejected form, the deploy image's PG and its volume own the at-rest bytes, and the `data_encryption` fragment composes the one shared `ClusterConfig.Verify` admission contract `Store/provisioning#CLUSTER_CONFIG` owns, never a parallel verifier; the `GucKind.Enumeration`/`GucRank.Degradable` row admits the primary `on` or degrades to volume encryption, so a generously-encrypted host never faults the way a brittle `==` would; the object arm projects the settled `Store/remote#OBJECT_STORE` `ObjectEncryption.ManagedKey` the remote seal already owns — a second SSE-request type minted here, or stamping the content-identity `BlobRemote.Descriptor.CodecId`/`CompressionId` (which carry the payload codec, not a key) with an SSE marker, is the deleted form because the remote tier owns the SSE write policy and the content round-trip `Store/remote#OBJECT_RESIDENCE` owns the descriptor; the SSE key is the same `KmsProvider`-custodied CMK the envelope axis resolves through the lease, never a Persistence-side key.

```csharp signature
public static class AtRest {
    // The data_encryption TDE fragment handed to the shared ClusterConfig.Verify fold — a (setting, value,
    // fallback) triple the implicit GucRow conversion lifts, but Enumeration/Degradable rather than the
    // default hard Text row so an image without the GUC degrades to volume encryption. A hard abort lifts
    // to EncryptionFault.TdeUnverified naming the breached setting.
    public static readonly GucRow Fragment =
        new("data_encryption", "on", "off", GucKind.Enumeration, GucRank.Degradable, GucContext.Postmaster);

    public static Fin<GucVerdict> VerifyPg(FrozenDictionary<string, GucReading> observed) =>
        ClusterConfig.Verify(Seq(Fragment), observed)
            .MapFail(static error => (Error)new EncryptionFault.TdeUnverified(error.Message));

    // The object SSE-KMS feed: the settled Store/remote#OBJECT_STORE `ObjectEncryption.ManagedKey` the remote
    // multipart seal already consumes, carrying the envelope-axis CMK ARN and the per-partition AAD — so the
    // object encrypts server-side under the same key the DEK envelope custodies and this page mints no second
    // SSE vocabulary. The remote tier's per-arm seal lowers ManagedKey to the provider write parameter.
    public static ObjectEncryption ObjectSse(string keyArn, EnvelopeAad aad) =>
        new ObjectEncryption.ManagedKey(keyArn, aad.Context);
}
```

| [INDEX] | [SCOPE]        | [MECHANISM]                                      | [KEY_SOURCE]                                       |
| :-----: | :------------- | :----------------------------------------------- | :------------------------------------------------- |
|  [01]   | sqlite         | SQLCipher `PRAGMA key`/`rekey` in-file cipher     | KMS-unwrapped DEK via `EnvelopeKeyring`, AAD+CRC-checked |
|  [02]   | postgres       | TDE GUC, `ClusterConfig.Verify(data_encryption)`  | deploy-image PG + volume own the at-rest bytes     |
|  [03]   | object-store   | SSE-KMS via `ObjectEncryption.ManagedKey` (provider write request) | same `KmsProvider` CMK, server-side encrypted      |
|  [04]   | demand gate    | `Envelope.Demand` over `DataClassification`       | Personal/Credential/Secret mandate, else rejected  |

| [INDEX] | [PROVIDER]     | [WRAP]                          | [AAD]                                  | [ROTATION]                        | [INTEGRITY]        |
| :-----: | :------------- | :------------------------------ | :------------------------------------- | :-------------------------------- | :----------------- |
|  [01]   | aws-kms        | `Encrypt`/`GenerateDataKey`     | `EncryptionContext` (provider)         | `ReEncrypt` rewrap                | transport          |
|  [02]   | azure-keyvault | native `WrapKey`/`UnwrapKey`    | application digest (no native AAD)     | re-wrap under new version         | transport          |
|  [03]   | gcp-kms        | `Encrypt`/`GenerateRandomBytes` | `AdditionalAuthenticatedData` (provider) | `UpdateCryptoKeyPrimaryVersion` repoint | CRC32C-verified |

## [05]-[RESEARCH]

- [SQLCIPHER_KEYING]: the `SQLite3Provider_sqlcipher` keying surface for the promoted `EncryptionGate.Sqlcipher` ceremony — the inline `PRAGMA key`/`rekey` hex-literal form versus the connection-string keying keyword, the key-literal escaping, the `cipher_migrate` legacy-format upgrade, and the pooled-physical-open application point of `PRAGMA key` as the first statement, proven against the externally supplied `osx-arm64` SQLCipher native library before the keying fence pins.
- [KMS_PER_ARM_MECHANISM]: the three arms' divergent verbs confirmed against the admitted assemblies — AWS `GenerateDataKey(KeyId, KeySpec=AES_256)`/`GenerateDataKeyWithoutPlaintext`/`Decrypt(CiphertextBlob, EncryptionContext)`/`ReEncrypt(DestinationKeyId)`, the Azure NATIVE `CryptographyClient.WrapKey(KeyWrapAlgorithm.RsaOaep256, dek)`/`UnwrapKey` with no native AAD parameter, and the GCP `GenerateRandomBytes`+`Encrypt(EncryptRequest{ AdditionalAuthenticatedData, PlaintextCrc32C })`/`Decrypt(DecryptRequest{ CiphertextCrc32C })` with `UpdateCryptoKeyPrimaryVersion` rotation — whether the per-arm `EncryptionContext`/`AdditionalAuthenticatedData` exact-match enforces at unwrap, whether the GCP `DecryptResponse.PlaintextCrc32C` divergence is detectable against the local `System.IO.Hashing.Crc32`, and whether `ReEncrypt` rotates the wrapping key entirely inside KMS, confirmed against a live key per provider before the keyring fence pins.
- [PG_TDE_VERIFY]: the PG18 `data_encryption` GUC the `PgTde` arm hands `ClusterConfig.Verify` — whether the deploy image's PG carries the TDE GUC (so the `Enumeration`/`Degradable` fragment matches `on` or degrades to volume encryption) or the at-rest posture is filesystem/volume encryption, confirmed against the Forge-provisioned PG18 before the verify fence pins.
- [KEY_STATE_PROBE]: the per-arm key-lifecycle probe — AWS `DescribeKey.KeyMetadata.KeyState`, Azure `KeyProperties.Enabled`, GCP `GetCryptoKey().Primary.State` (`CryptoKeyVersionState`) — whether a wrap against a `Disabled`/`DestroyScheduled` key surfaces the state at `Probe` time as an `EncryptionFault.KeyDisabled` admission reject rather than a deferred KMS state-throw mid-write, confirmed against a disabled key per provider before the probe fence pins.
