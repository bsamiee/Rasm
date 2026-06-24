# [PERSISTENCE_ENCRYPTION]

Rasm.Persistence makes envelope encryption, KMS key custody, rotation, and at-rest verification one closed key-management axis across sqlite, PostgreSQL, and object-store. `KeyEnvelope` is the wrapped-DEK record carrying its provider, scope, and rotation state; `KmsProvider` is the `[SmartEnum<string>]` cloud-KMS custody axis (AWS, Azure, GCP) projecting one `EnvelopeKeyring` wrap/unwrap delegate pair; `EnvelopeScope` is the `[Union]` per-engine keying family; and `RotationPolicy` is the rewrap cadence and key-version ladder. The sqlite arm drives the promoted `EncryptionGate.Sqlcipher` from a KMS-unwrapped data-encryption key bound through its `PRAGMA key`/`rekey` ceremony, the PG arm verifies the TDE GUC read-only through `ClusterConfig.Verify`, the object arm carries SSE-KMS, and the Personal-only `DemandsEncryption` reads as mandatory. The admitted `AWSSDK.KeyManagementService`, `Azure.Security.KeyVault.Keys`, and `Google.Cloud.Kms.V1` carry the wrap/unwrap members under the README `[ENCRYPTION_KMS]` group; `DataClassification` gates the encryption demand and `ClockPolicy`/`ReceiptSinkPort`/`CorrelationId` arrive settled. The AppHost per-open key handle — surfaced as one `SecretLease`-class carrier whose acquire-renew-zeroize custody the AppHost runtime owns — is the one KMS-unwrap port this owner reads, binding its `EnvelopeKeyring` against the lease-managed CMK access without minting a long-lived in-process key.

Wire posture: this page is host-local — keys are unwrapped server-side or on the embedded connection and a DEK never crosses a browser or peer wire, so it carries no `TS_PROJECTION` cluster. The KMS-unwrap port crosses to the AppHost runtime through the `Store/encryption ⇄ Rasm.AppHost/Runtime # [PORT]: KMS-unwrap port` seam as a settled key-handle, never a client-facing projection.

## [01]-[INDEX]

- [01]-[KEY_ENVELOPE]: KMS provider axis, envelope scope family, rotation policy, and the keyring wrap/unwrap.
- [02]-[SQLITE_KEYING]: the promoted SQLCipher gate bound to a KMS-unwrapped DEK through its PRAGMA ceremony.

## [02]-[KEY_ENVELOPE]

- Owner: `KmsProvider` the `[SmartEnum<string>]` cloud-KMS custody axis under the `StoreKeyPolicy` ordinal accessor; `EnvelopeKeyring` the wrap/unwrap delegate pair one provider projects; `EnvelopeScope` the `[Union]` per-engine keying family; `RotationPolicy` the rewrap cadence and key-version ladder; `KeyEnvelope` the wrapped-DEK record; `EnvelopeFact` with `EnvelopeFactKind` the page-wide fact stream; `Envelope` the static surface owning the mint, unwrap, and rotation folds.
- Cases: `KmsProvider` aws-kms | azure-keyvault | gcp-kms | none (the plaintext-default Personal-disallowed sentinel); `EnvelopeScope` `SqliteDek` (the SQLCipher DEK), `PgTde` (the verify-only cluster GUC), `ObjectSse` (SSE-KMS object residence) on the union; `EnvelopeFactKind` mint | unwrap | rotate | verify | demand-breach.
- Entry: `public static IO<KeyEnvelope> Mint(KmsProvider provider, EnvelopeScope scope, FrozenDictionary<string, string> aad, ClockPolicy clocks)` derives a fresh DEK and its wrapped blob through `GenerateDataKey`; `public static IO<ReadOnlyMemory<byte>> Unwrap(KeyEnvelope envelope, EnvelopeKeyring keyring, FrozenDictionary<string, string> aad)` recovers the plaintext DEK through `Decrypt` and zeroes it after the local bind; `public static IO<KeyEnvelope> Rotate(KeyEnvelope envelope, EnvelopeKeyring keyring, string destinationKeyId)` rewraps the blob through `ReEncrypt` without the plaintext re-entering managed memory.
- Auto: KMS exposes no native wrap verb so the wrap of a DEK is `Encrypt` and the unwrap is `Decrypt` keyed by the customer master key ARN under `SYMMETRIC_DEFAULT` — `Mint` calls `GenerateDataKey(KeyId, KeySpec=AES_256)` and persists the returned `CiphertextBlob` beside the ciphertext while the returned `Plaintext` drives the local at-rest cipher, and `Unwrap` calls `Decrypt(CiphertextBlob, EncryptionContext)` and zeroes the recovered `Plaintext` immediately after the local decrypt; the `EncryptionContext` carries the store partition identity (and tenant id under RLS) as AAD on every wrap and unwrap so a blob cannot be unwrapped under a foreign partition; `Rotate` rewraps persisted blobs through `ReEncrypt(DestinationKeyId)` so the plaintext DEK never re-enters managed memory and a key-rotation is a rewrap, not a decrypt-then-encrypt round trip; the PG arm never holds a DEK — it verifies the TDE GUC (`data_encryption=on` where the deploy image's PG carries it, else the at-rest posture is the filesystem/volume encryption the deploy owns) read-only through `ClusterConfig.Verify`; the object arm sets SSE-KMS on the `Store/remote#OBJECT_STORE` `BlobRemote.Descriptor` so an object write is server-side encrypted under the same provider's key; the Personal-only `DemandsEncryption` reads the `DataClassification.Personal`/`Credential`/`Secret` ceiling so a mandated-classification write on a `none`-provider profile is a typed `demand-breach` rejection at admission.
- Receipt: a DEK mint rides `envelope.mint` carrying the key ARN and the scope; an unwrap rides `envelope.unwrap`; a rotation rides `envelope.rotate` carrying the source and destination ARN; a TDE verify rides `envelope.verify`; a classification-demand breach rides `envelope.demand-breach` and a typed encryption fault, never silent; every fact folds into the open receipt's proof rows.
- Packages: AWSSDK.KeyManagementService, Azure.Security.KeyVault.Keys, Google.Cloud.Kms.V1, Microsoft.Data.Sqlite, Npgsql, AWSSDK.S3, Azure.Storage.Blobs, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project)
- Growth: a new cloud-KMS custody is one `KmsProvider` row projecting its `EnvelopeKeyring`; a new keying scope is one `EnvelopeScope` case; a new rotation cadence is one column on `RotationPolicy`; a new evidence bucket is one `EnvelopeFactKind` row; zero new surface — a second key store, a hand-rolled token cache, or a per-provider keying class is the deleted form.
- Boundary: KMS has no native wrap verb so wrap is `Encrypt` and unwrap is `Decrypt` — a native-wrap assumption is the rejected form; the concrete KMS client (`IAmazonKeyManagementService`/`KeyClient`/`KeyManagementServiceClient`) is constructed once at composition per configured key and never per operation, so a per-operation client is the deleted form; the plaintext DEK never persists and is zeroed immediately after the local bind, so a persisted plaintext DEK is the deleted form; the `EncryptionContext` AAD binds the store partition (and tenant) on every wrap and unwrap so an unwrap without the binding context is the rejected form and a cross-partition blob cannot decrypt; the PG arm is verification-only — a managed TDE-key-set or `ALTER SYSTEM` is the rejected form, the deploy image's PG and its volume own the at-rest bytes and this owner verifies the GUC read-only; the object arm composes the settled `BlobRemote` SSE-KMS column and never a second object-store client; the KMS-unwrap handle crosses from the AppHost runtime per open through the `Store/encryption ⇄ Rasm.AppHost/Runtime # [PORT]: KMS-unwrap port` seam as one `SecretLease`-class content carrier whose acquire-renew-zeroize custody the AppHost `Runtime/config#SECRET_LEASE` owns, so the in-process key-handle lifecycle is the runtime lease's concern, this owner binds its `EnvelopeKeyring` wrap/unwrap delegate pair against the lease-managed CMK access and consumes the resolved per-open handle, never minting a long-lived in-process key and never a Persistence-side credential lifecycle; the concrete `KmsProvider` client and the AAD `EncryptionContext` binding stay this owner's; the Personal-only `DemandsEncryption` reads the `DataClassification` ceiling so a mandated-classification write on a plaintext-default profile is a typed rejection at admission, never a silent plaintext write.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
[KeyMemberComparer<StoreKeyPolicy, string>]
public sealed partial class EnvelopeFactKind {
    public static readonly EnvelopeFactKind Mint = new("mint");
    public static readonly EnvelopeFactKind Unwrap = new("unwrap");
    public static readonly EnvelopeFactKind Rotate = new("rotate");
    public static readonly EnvelopeFactKind Verify = new("verify");
    public static readonly EnvelopeFactKind DemandBreach = new("demand-breach");
}

public sealed record EnvelopeKeyring(
    Func<FrozenDictionary<string, string>, IO<(ReadOnlyMemory<byte> Plaintext, ReadOnlyMemory<byte> Wrapped, string KeyArn)>> Mint,
    Func<ReadOnlyMemory<byte>, FrozenDictionary<string, string>, IO<ReadOnlyMemory<byte>>> Unwrap,
    Func<ReadOnlyMemory<byte>, string, FrozenDictionary<string, string>, IO<(ReadOnlyMemory<byte> Wrapped, string KeyArn)>> Rotate);

[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
[KeyMemberComparer<StoreKeyPolicy, string>]
public sealed partial class KmsProvider {
    public static readonly KmsProvider AwsKms = new("aws-kms");
    public static readonly KmsProvider AzureKeyVault = new("azure-keyvault");
    public static readonly KmsProvider GcpKms = new("gcp-kms");
    public static readonly KmsProvider None = new("none");

    public bool Custodies => this != None;
}

public sealed record RotationPolicy(Duration Interval, int RetainVersions) {
    public static readonly RotationPolicy Quarterly = new(Duration.FromDays(90), RetainVersions: 2);

    public bool Due(Instant minted, Instant now) => now - minted >= Interval;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EnvelopeScope {
    private EnvelopeScope() { }

    public sealed record SqliteDek(string Store) : EnvelopeScope;
    public sealed record PgTde(string Database) : EnvelopeScope;
    public sealed record ObjectSse(string Bucket, string Region) : EnvelopeScope;
}

public sealed record KeyEnvelope(
    KmsProvider Provider, EnvelopeScope Scope, ReadOnlyMemory<byte> WrappedDek, string KeyArn,
    RotationPolicy Rotation, int Version, Instant MintedAt);

public static class Envelope {
    public static FrozenSet<DataClassification> DemandsEncryption =>
        new[] { DataClassification.Personal, DataClassification.Credential, DataClassification.Secret }.ToFrozenSet();

    public static IO<KeyEnvelope> Mint(KmsProvider provider, EnvelopeScope scope, EnvelopeKeyring keyring, FrozenDictionary<string, string> aad, RotationPolicy rotation, ClockPolicy clocks) =>
        keyring.Mint(aad).Map(minted =>
            new KeyEnvelope(provider, scope, minted.Wrapped, minted.KeyArn, rotation, Version: 1, clocks.Now));

    public static IO<ReadOnlyMemory<byte>> Unwrap(KeyEnvelope envelope, EnvelopeKeyring keyring, FrozenDictionary<string, string> aad) =>
        keyring.Unwrap(envelope.WrappedDek, aad);

    public static IO<KeyEnvelope> Rotate(KeyEnvelope envelope, EnvelopeKeyring keyring, string destinationKeyId, FrozenDictionary<string, string> aad, ClockPolicy clocks) =>
        keyring.Rotate(envelope.WrappedDek, destinationKeyId, aad).Map(rewrapped =>
            envelope with { WrappedDek = rewrapped.Wrapped, KeyArn = rewrapped.KeyArn, Version = envelope.Version + 1, MintedAt = clocks.Now });

    public static Fin<KeyEnvelope> Demand(KeyEnvelope envelope, DataClassification classification) =>
        DemandsEncryption.Contains(classification) && !envelope.Provider.Custodies
            ? Fin.Fail<KeyEnvelope>(Error.New($"<encryption-demanded:{classification.Key}>"))
            : Fin.Succ(envelope);
}
```

## [03]-[SQLITE_KEYING]

- Owner: `EncryptionGate` the promoted SQLCipher keying record (relocated from `Store/engine#EXTENSION_GATES` Research) carrying its `PRAGMA key`/`cipher_migrate`/`cipher_version`/`rekey` ceremony rows; `SqliteKeying` the static surface binding the ceremony to a KMS-unwrapped DEK supplied through the `EnvelopeKeyring` unwrap delegate.
- Entry: `public static IO<Seq<SqliteFact>> Open(SqliteConnection connection, KeyEnvelope envelope, EnvelopeKeyring keyring, FrozenDictionary<string, string> aad, ClockPolicy clocks)` — unwraps the DEK through the keyring, applies `PRAGMA key` with the unwrapped key as the first statement on the physical open, runs `cipher_migrate` for a legacy-format file, and reads `cipher_version` as a fact; the keyring's `Unwrap` delegate owns zeroing the recovered plaintext after the local bind so `Open` consumes the borrowed key span and never persists it; `public static IO<SqliteFact> Rekey(SqliteConnection connection, ReadOnlyMemory<byte> nextDek, ClockPolicy clocks)` runs `PRAGMA rekey` to re-cipher the file under a rotated DEK.
- Auto: the SQLCipher gate is promoted out of `ExtensionGateState.Research` to production — its key source is no longer a static passphrase but the `EnvelopeKeyring` unwrap delegate, so `Open` unwraps the KMS-wrapped DEK, applies `PRAGMA key` as the first statement before any user statement (or the file is unreadable), and the keyring's `Unwrap` delegate zeroes the recovered plaintext once the pragma has consumed the key span; `cipher_migrate` upgrades a legacy SQLCipher file format and `cipher_version` receipts the cipher provider version; `Rekey` runs `PRAGMA rekey` so a `RotationPolicy`-due rotation re-ciphers the embedded file under the rotated DEK in place; the `SQLite3Provider_sqlcipher` provider replaces the default `e_sqlite3` bundle on the encrypted profile and the two routes never mix; the gate arms only on a Personal/Credential/Secret-classified embedded store so a plaintext-default store never pays the cipher cost.
- Receipt: the keying ceremony rides `SqliteFactKind.ExtensionLoad`/`Registration` facts on the existing sqlite fact stream carrying the cipher version (never the key); a rekey rides a rotation fact; the `EnvelopeFact` `unwrap`/`rotate` facts carry the envelope evidence.
- Packages: Microsoft.Data.Sqlite, SQLitePCLRaw (the `SQLite3Provider_sqlcipher` provider), AWSSDK.KeyManagementService, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime.
- Growth: a new ceremony pragma is one `Ceremony` row; a new keying provider is the `KmsProvider` axis above; zero new surface — a static-passphrase keying, a connection-string `Password` knob, or a second cipher provider is the deleted form because the DEK is KMS-unwrapped and the ceremony rides the one `EncryptionGate`.
- Boundary: the SQLCipher gate is promoted from Research to production by binding its ceremony to the KMS-unwrapped DEK — a static-passphrase key source is the deleted form, and the `Password` connection-string row (the admitted `e_sqlite3` bundle has no cipher and fails at open) is the rejected spelling; the DEK arrives unwrapped through the `EnvelopeKeyring` per open and is zeroed immediately after `PRAGMA key`, never persisted; `PRAGMA key` is the first statement on the physical open before any user statement or the file is unreadable; the `SQLite3Provider_sqlcipher` provider replaces the default bundle on the encrypted profile (the batteries bundle route is deprecated and the two never mix); `Rekey` re-ciphers in place under a rotated DEK so a key rotation never re-exports the store; the gate arms only on a mandated-classification embedded store per `Envelope.Demand` so a plaintext-default store stays unencrypted at zero cost; this keying composes the `Store/engine#PRAGMA_TABLE` ladder — `PRAGMA key` precedes the PRAGMA ladder so the cipher is armed before the engine knobs apply.

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
        FrozenDictionary<string, string> aad, ClockPolicy clocks) =>
        Envelope.Unwrap(envelope, keyring, aad).Bind(dek => IO.lift(() => {
            var mark = clocks.Mark();
            using var key = connection.CreateCommand();
            key.CommandText = $"PRAGMA key = \"x'{Convert.ToHexString(dek.Span)}'\"; PRAGMA cipher_migrate;";
            _ = key.ExecuteNonQuery();
            using var version = connection.CreateCommand();
            version.CommandText = "PRAGMA cipher_version;";
            var cipher = Optional(version.ExecuteScalar()).Map(static v => v.ToString() ?? "");
            return Seq(new SqliteFact(SqliteFactKind.ExtensionLoad, "sqlcipher", None, cipher, Count: 0, Bytes: 0, clocks.Elapsed(mark), clocks.Now));
        }));

    public static IO<SqliteFact> Rekey(SqliteConnection connection, ReadOnlyMemory<byte> nextDek, ClockPolicy clocks) =>
        IO.lift(() => {
            var mark = clocks.Mark();
            using var rekey = connection.CreateCommand();
            rekey.CommandText = $"PRAGMA rekey = \"x'{Convert.ToHexString(nextDek.Span)}'\";";
            _ = rekey.ExecuteNonQuery();
            return new SqliteFact(SqliteFactKind.Registration, "sqlcipher-rekey", None, None, Count: 0, Bytes: 0, clocks.Elapsed(mark), clocks.Now);
        });
}
```

| [INDEX] | [SCOPE]        | [MECHANISM]                                      | [KEY_SOURCE]                                       |
| :-----: | :------------- | :----------------------------------------------- | :------------------------------------------------- |
|  [01]   | sqlite         | SQLCipher `PRAGMA key`/`rekey` in-file cipher     | KMS-unwrapped DEK via `EnvelopeKeyring`            |
|  [02]   | postgres       | TDE GUC, verify-only via `ClusterConfig.Verify`   | deploy-image PG + volume own the at-rest bytes     |
|  [03]   | object-store   | SSE-KMS on the `BlobRemote.Descriptor`            | provider KMS key, server-side encrypted            |
|  [04]   | demand gate    | `Envelope.Demand` over `DataClassification`       | Personal/Credential/Secret mandate, else rejected  |

## [04]-[RESEARCH]

- [SQLCIPHER_KEYING]: the `SQLite3Provider_sqlcipher` keying surface for the promoted `EncryptionGate.Sqlcipher` ceremony — the inline `PRAGMA key`/`rekey` hex-literal form versus the connection-string keying keyword, the key-literal escaping, the `cipher_migrate` legacy-format upgrade, and the pooled-physical-open application point of `PRAGMA key` as the first statement, proven against the externally supplied `osx-arm64` SQLCipher native library before the keying fence pins.
- [KMS_AAD_BINDING]: the `EncryptionContext` AAD round-trip across `GenerateDataKey`/`Decrypt`/`ReEncrypt` carrying the store partition and tenant id — whether the exact case-sensitive AAD map supplied at wrap is enforced at unwrap and `ReEncrypt` rotates the wrapping key entirely inside KMS without the plaintext DEK crossing the wire, confirmed against a live KMS key before the keyring fence pins.
- [PG_TDE_VERIFY]: the PG18 at-rest encryption GUC the `PgTde` arm verifies read-only — whether the deploy image's PG carries a TDE GUC or the at-rest posture is filesystem/volume encryption, confirmed against the Forge-provisioned PG18 before the verify fence pins.
