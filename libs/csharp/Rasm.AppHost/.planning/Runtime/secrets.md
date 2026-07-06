# [APPHOST_SECRETS_AND_CREDENTIAL_MATERIAL]

The credential-material lifecycle owner: a `SecretLease` row family acquires, renews, and zeroizes credential material against the one RID-dispatched credential-store provider the host resolves through `Runtime/config#SOURCE_AXIS`'s `ConfigSource.SecretsStore` row, surfaces the per-store-open KMS-unwrap handle `Rasm.Persistence/Element/identity#KEY_ENVELOPE` reads as one `SecretLease`-class content carrier so the cloud-KMS key-handle lifecycle stays the runtime lease's concern — never a long-lived Persistence-side key — and one `CredentialPem` axis is the suite's only credential-material wire vocabulary: the host encodes every PEM-bearing credential into one canonical RFC-7468 multi-element bundle the `\n` PEM-block delimiter joins, mints the redacted `CredentialPemWire` carrier the TS verifier and the Python admission decode, and never crosses a raw `byte[]` or a parallel base64 envelope. The page owns the secret-lease lifecycle, the credential-PEM encoding vocabulary, and the KMS-unwrap custody over System.Security.Cryptography (the BCL `PemEncoding`/`X509Certificate2` PEM owners), Microsoft.Extensions.Compliance.Redaction, the kernel `ContentHash.Of` identity entry, NodaTime, Thinktecture.Runtime.Extensions, and LanguageExt.Core.

## [01]-[INDEX]

- [01]-[SECRET_LEASE]: Acquire-renew-zeroize credential lifecycle extending the `SecretsStore` source row.
- [02]-[CREDENTIAL_PEM]: Canonical RFC-7468 PEM bundle encoding and the redacted cross-language carrier.
- [03]-[TS_PROJECTION]: The redacted credential-bundle wire shape.

## [02]-[SECRET_LEASE]

- Owner: `SecretLease` boundary capsule extending `ConfigSource.SecretsStore` — the only credential lifecycle owner; `LeaseTransition` `[Union]` lifecycle vocabulary; `SecretFault` `[Union]` fault family deriving its codes through `FaultBand.Secret`; `SecretReceipt` the redacted rotation evidence record.
- Cases: lifecycle transitions Acquired | Renewed | Released | Zeroized; `SecretFault` = Text | AcquireRejected | RenewMissed | StoreUnavailable.
- Entry: `Acquire(SecretRuntime runtime, string keyId)` returns `Fin<Atom<SecretLease>>` — the credential-store read folds the `ConfigLayer.SecretsSource` provider into the LIVE lease cell whose renewal occurrence rotates it in place, consumers reading the current material off the cell at use; `Rotate(SecretRuntime runtime, Atom<SecretLease> cell)` returns `IO<Unit>` — the renewal `ScheduleEntry.Work` binding folding `Renew` over the live cell inside `Swap(current => ...)`; `Renew(SecretRuntime runtime, SecretLease lease)` returns `Fin<SecretLease>` re-pulling before expiry and zeroizing the prior copy; `Zeroize(SecretLease lease)` returns `Unit`, the drain-forced terminal that overwrites the in-memory copy.
- Auto: renewal registers one `ScheduleEntry` on Runtime/time#SCHEDULE_PORT at the credential-rotation `DeadlineClass` row carrying a `LeasePolicy` whose `CrashStaleness` outlives the renewal window, so a single occurrence row drives rotation ahead of expiry with no per-secret timer — the occurrence's `Work` binds `Rotate` over the live `Atom<SecretLease>` cell so the fire re-pulls the store, zeroizes the prior copy, and swaps the fresh lease in place (a rotation template the consumer must hand-bind is the deleted form, and a failed re-pull keeps the current lease so degradation rides the health rail); the zeroization registers as one Runtime/lifecycle#DRAIN_CONDUCTOR `DrainBand.Stores` participant row that runs under the drain-forced token so a hung renewal never strands a live secret; the credential bytes carry `DataClassification.Secret` so Observability/telemetry#REDACTION_TAXONOMY erases them at every egress and the receipt diff folds through the bound `Redactor`.
- Receipt: `SecretReceipt` carries the lease window, the kernel content digest of the canonical credential bytes, and the redacted credential-id diff only — never a secret byte; the transition emits on ReceiptSinkPort.Send partitioned by `TenantId` so each tenant's rotation stream stays isolated.
- Packages: Rasm (kernel `ContentHash.Of`), Microsoft.Extensions.Configuration.UserSecrets, Microsoft.Extensions.Compliance.Redaction, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: one lifecycle transition is one `LeaseTransition` case; one fault is one `SecretFault` case; a new credential source is one `SecretsSource` provider value on the existing `ConfigLayer`, never a second lease owner; zero new surface.
- Boundary: the lease is the suite's only credential lifecycle owner — a per-secret rotation helper, a raw `string` credential field, and a second zeroization path are the deleted forms; a lease renews strictly before expiry or the fold degrades through Observability/health#DEGRADATION_RAIL, never a hard fault, so `RenewMissed` lands `DegradationLevel.ReadOnly` rather than terminating the rail; the in-memory copy is a rented `byte[]` overwritten through `CryptographicOperations.ZeroMemory` so no managed copy survives collection; the rotation-diff identity is the kernel `Rasm.Domain.ContentHash.Of` digest of the canonical credential bytes — the federation's one seed-zero content-identity entry, non-cryptographic, carrying identity only and never a security claim, so the diff is a digest equality with no constant-time pretense layered over a non-crypto hash; the lease holds the live raw `byte[]` and owns only the in-memory lifecycle and zeroization, while the canonical at-rest and on-wire credential encoding is `CREDENTIAL_PEM`'s `CredentialBundle`/`CredentialPem` — the lease never encodes material and the PEM axis never holds a live mutable copy, so a PEM-bearing credential's `SecretReceipt.ContentDigest` is the `CredentialBundle` per-block digest fold and the redacted rotation crosses as the `CredentialPemWire` carrier, never two parallel credential encodings; the lease extends the `Runtime/config#SOURCE_AXIS` `ConfigSource.SecretsStore` rank-40 frozen-class row — the credential never re-mounts at runtime, the lease owns the live rotation above that frozen mount, and the credential-store read reuses `ConfigLayer.SecretsSource` rather than a parallel provider; the per-store-open KMS-unwrap handle `Rasm.Persistence/Element/identity#KEY_ENVELOPE` reads crosses as one `SecretLease`-class content carrier through the `Runtime ⇄ Rasm.Persistence/Element/identity # [PORT]: KMS-unwrap port` seam — the lease owns the acquire-renew-zeroize custody of the cloud-KMS CMK access (the `KmsProvider`-resolved credential the Persistence `Element/identity#KEY_ENVELOPE` `EnvelopeKeyring` `Mint`/`Unwrap`/`Rewrap`/`Probe` delegate quartet binds against, where each arm's mechanism is a policy value on the `KmsProvider` row — AWS encrypt-as-wrap, Azure native `WrapKey`/`UnwrapKey`, GCP encrypt-as-wrap with CRC32C and primary-version repoint — not one arm's spelling as a universal law) so the in-process key-handle lifecycle stays the runtime lease's concern and Persistence consumes the resolved per-open handle without minting a long-lived in-process key, the unwrapped DEK never persists and zeroizes through the same `CryptographicOperations.ZeroMemory` path the lease owns, and the KMS-unwrap handle is a content carrier riding this lifecycle, never an eighth port — a Persistence-side long-lived key cache or a second credential lifecycle is the deleted form; the `Rasm.AppUi` PDF digital-signature arm composes this owner's lease-scoped credential export for its `IDigitalSigner` material — acquire-renew-zeroize applies to the signing credential exactly as to any lease, and AppUi never holds raw key bytes.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LeaseTransition {
    private LeaseTransition() { }
    public sealed record Acquired(string KeyId, Interval Window) : LeaseTransition;
    public sealed record Renewed(string KeyId, Interval Window) : LeaseTransition;
    public sealed record Released(string KeyId, Instant At) : LeaseTransition;
    public sealed record Zeroized(string KeyId, Instant At) : LeaseTransition;
}

[Union]
public abstract partial record SecretFault : Expected, IValidationError<SecretFault> {
    private SecretFault(string detail, int code) : base(detail, code, None) { }
    public static SecretFault Create(string message) => new Text(message);
    public sealed record Text : SecretFault { public Text(string detail) : base(detail, FaultBand.Secret.Code(0)) { } }
    public sealed record AcquireRejected : SecretFault { public AcquireRejected(string keyId, string detail) : base($"{keyId}: {detail}", FaultBand.Secret.Code(1)) => KeyId = keyId; public string KeyId { get; } }
    public sealed record RenewMissed : SecretFault { public RenewMissed(string keyId, string detail) : base($"{keyId}: {detail}", FaultBand.Secret.Code(2)) => KeyId = keyId; public string KeyId { get; } }
    public sealed record StoreUnavailable : SecretFault { public StoreUnavailable(string detail) : base(detail, FaultBand.Secret.Code(3)) { } }
}

public sealed record SecretReceipt(
    string KeyId,
    LeaseTransition Transition,
    Interval Window,
    string ContentDigest,
    string RedactedId,
    Instant At);

public sealed record SecretRuntime(
    Func<string, Fin<byte[]>> Read,
    Redactor Redactor,
    LeasePolicy Lease,
    DeadlineClass Rotation,
    ClockPolicy Clocks,
    ReceiptSinkPort Sink,
    TenantContext Tenant,
    CorrelationId Correlation,
    JsonSerializerOptions Wire);

public sealed record SecretLease(string KeyId, byte[] Material, Interval Window, ScheduleEntry Renewal) {
    // Kernel content identity: UInt128 is the currency; hex is the boundary projection at the receipt seam.
    public static string Digest(ReadOnlySpan<byte> material) => ContentHash.Of(material).ToString("x32");

    public string Redacted(Redactor redactor) {
        Span<char> sink = stackalloc char[redactor.GetRedactedLength(KeyId)];
        var written = redactor.Redact(KeyId, sink);
        return new string(sink[..written]);
    }
}

public static class SecretLeaseOps {
    public static Fin<Atom<SecretLease>> Acquire(SecretRuntime runtime, string keyId) =>
        runtime.Read(keyId)
            .MapFail(error => (Error)new SecretFault.AcquireRejected(keyId, error.Message))
            .Map(material => {
                var now = runtime.Clocks.Now;
                var window = ClockPolicy.Window(now + runtime.Rotation.Allotted, runtime.Rotation.Allotted);
                Atom<SecretLease>? cell = null;
                var renewal = new ScheduleEntry(
                    Key: $"secret-renew:{keyId}",
                    Spec: new OccurrenceSpec.Every(runtime.Rotation.Allotted),
                    Deadline: runtime.Rotation,
                    Lease: Some(runtime.Lease),
                    Work: () => Rotate(runtime, cell!));
                cell = Atom(Emit(runtime, new SecretLease(keyId, material, window, renewal), new LeaseTransition.Acquired(keyId, window)));
                return cell;
            });

    // The renewal occurrence IS the rotation: Renew re-pulls the store and zeroizes the prior copy,
    // the swap publishes the fresh lease into the live cell, and a missed renewal keeps the current
    // lease so RenewMissed degrades through Observability/health — never a torn cell, never a no-op.
    public static IO<Unit> Rotate(SecretRuntime runtime, Atom<SecretLease> cell) =>
        IO.lift(() => Renew(runtime, cell.Value)).Bind(result => result.Match(
            Succ: renewed => IO.lift(() => { ignore(cell.Swap(_ => renewed)); return unit; }),
            Fail: fault => runtime.Sink.Send(
                runtime.Correlation, runtime.Tenant, TelemetrySource.AppHost.Key, nameof(SecretLease),
                JsonSerializer.SerializeToElement(fault.Message, runtime.Wire)).Map(static _ => unit)));

    public static Fin<SecretLease> Renew(SecretRuntime runtime, SecretLease lease) =>
        runtime.Clocks.Now is var now && now >= lease.Window.End
            ? Fin.Fail<SecretLease>(new SecretFault.RenewMissed(lease.KeyId, "lease expired before renewal"))
            : runtime.Read(lease.KeyId)
                .MapFail(error => (Error)new SecretFault.RenewMissed(lease.KeyId, error.Message))
                .Map(material => {
                    CryptographicOperations.ZeroMemory(lease.Material);
                    var window = ClockPolicy.Window(now + runtime.Rotation.Allotted, runtime.Rotation.Allotted);
                    return Emit(runtime, lease with { Material = material, Window = window }, new LeaseTransition.Renewed(lease.KeyId, window));
                });

    public static Unit Zeroize(SecretRuntime runtime, SecretLease lease) {
        ignore(Emit(runtime, lease, new LeaseTransition.Zeroized(lease.KeyId, runtime.Clocks.Now)));
        CryptographicOperations.ZeroMemory(lease.Material);
        return unit;
    }

    static SecretLease Emit(SecretRuntime runtime, SecretLease lease, LeaseTransition transition) {
        var receipt = new SecretReceipt(
            lease.KeyId, transition, lease.Window,
            SecretLease.Digest(lease.Material), lease.Redacted(runtime.Redactor), runtime.Clocks.Now);
        ignore(runtime.Sink.Send(
            runtime.Correlation, runtime.Tenant, TelemetrySource.AppHost.Key, nameof(SecretLease),
            JsonSerializer.SerializeToElement(receipt, runtime.Wire)).Run());
        return lease;
    }
}
```

## [03]-[CREDENTIAL_PEM]

- Owner: `PemLabel` `[SmartEnum<string>]` the closed RFC-7468 textual-encoding label vocabulary under the `ComparerAccessors.StringOrdinalIgnoreCase` accessor; `PemBlock` the single armored element; `CredentialBundle` the ordered multi-element bundle the canonical `\n` PEM-block delimiter joins; `CredentialPemWire` the redacted cross-language carrier; `PemFault` `[Union]` fault family deriving its codes through `FaultBand.Pem`; `CredentialPem` the static encode-decode-redact surface.
- Cases: 6 label rows — certificate, public-key, private-key, ec-private-key, rsa-private-key, pkcs7 — the RFC-7468 armor labels the BCL `PemEncoding` writes between the `-----BEGIN {label}-----`/`-----END {label}-----` lines; `PemFault` = Text | LabelUnknown | ArmorMalformed | EmptyBundle.
- Entry: `Encode(CredentialBundle bundle)` returns `string` — one fold writes each `PemBlock` through `PemEncoding.WriteString(label, der)` and joins the armored elements with the single `\n` RFC-7468 inter-block delimiter, so a certificate chain plus its private key crosses as one canonical bundle text whose element boundary is the `-----END-----`/`-----BEGIN-----` armor pair, never a hand-built `--SEP--` token; `Decode(string text)` returns `Fin<CredentialBundle>` — one fold walks `PemEncoding.TryFind` across the text, peeling each `-----BEGIN/END-----` armored element into a `PemBlock` so the decoder reads any RFC-7468 producer's bundle without a separator contract; `Carrier(CredentialBundle bundle, string keyId, Redactor redactor, ClockPolicy clocks)` returns `CredentialPemWire` — the redacted carrier the wire crosses, carrying the bundle's label set, the per-block kernel content digest, and the redacted key-id, never a private-key byte.
- Auto: the bundle is the canonical wire shape the `SecretLease` produces and the TS verifier and Python admission consume — a credential-material wire crossing as a raw `byte[]`, a bare base64 string, or a hand-built `\n--SEP--\n`-joined envelope is the deleted form, the RFC-7468 armor IS the self-delimiting separator and the `\n` between an `-----END-----` and the next `-----BEGIN-----` is the only inter-block byte; the `CredentialBundle.Cert(X509Certificate2 certificate)` factory derives a certificate bundle from the cert's own DER (`X509Certificate2.RawData`) so the host never hand-encodes bytes it already owns through the BCL cert surface, and `CredentialPem.Decode` round-trips a `CERTIFICATE` block through `X509Certificate2.CreateFromPem(text)` so the decoder proves the armored bytes parse as a real certificate before admission; a `PrivateKey`-classed `PemBlock` carries `DataClassification.Secret` so Observability/telemetry#REDACTION_TAXONOMY erases its bytes at every egress and the `CredentialPemWire` never carries a private-key block's content, only its label and content digest; the per-block digest is the kernel `ContentHash.Of` identity value — non-cryptographic, forbidden a security claim — so the wire carrier proves bundle identity without exposing material and the rotation diff is a digest equality, never a constant-time pretense.
- Receipt: the credential rotation rides the `SecretReceipt` the `SECRET_LEASE` cluster mints — `SecretReceipt.ContentDigest` is the `CredentialBundle` per-block digest fold and `SecretReceipt.RedactedId` the carrier's redacted key-id, so the PEM axis adds no parallel receipt and the `CredentialPemWire` is the redacted projection the receipt sink already fans.
- Packages: Rasm (kernel `ContentHash.Of`), System.Security.Cryptography, Microsoft.Extensions.Compliance.Redaction, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: one armor label is one `PemLabel` row; one bundle-element kind is one `PemBlock` carried in the existing ordered bundle, never a parallel envelope; one fault is one `PemFault` case; a new credential material kind rides the label axis already; zero new surface.
- Boundary: the PEM axis is the suite's only credential-material wire owner — the `SecretLease` holds the live `byte[]` in memory and zeroizes it, while `CredentialPem` owns the canonical at-rest and on-wire encoding, so the lease lifecycle and the material encoding never merge into one surface and never split the material into two encodings; the BCL `PemEncoding` owns the RFC-7468 armor write/find and `X509Certificate2.ExportCertificatePem`/`CreateFromPem` own the certificate round-trip — a hand-rolled base64 wrap, a manual `-----BEGIN-----` string build, and a Newtonsoft or third-party PEM codec are the deleted forms; the bundle crosses to TS as the `CredentialPemWire` the `security/crypt/sign` `Material.admit` key-material fold decodes through `jose`'s own `importPKCS8`/`importSPKI`/`importX509` importers, and to Python as the carrier the `runtime/execution/admission` `SettingsAdmission` secret-file source reads through `cryptography`'s `load_pem_*` parse — both consumers decode the one C#-minted bundle and re-mint no parallel PEM vocabulary, per architecture#CROSS_LANGUAGE_WIRE; the private-key block never crosses in the `CredentialPemWire` carrier — only the public certificate chain, the label set, and the content digests cross, so a TS or Python verifier reads the credential's public identity off the wire while the private material stays host-side under the `SecretLease` zeroization; the label set is the closed RFC-7468 vocabulary the BCL writes, so an unknown armor label decodes to `PemFault.LabelUnknown` rather than admitting an unrecognized block.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class PemLabel {
    public static readonly PemLabel Certificate = new("CERTIFICATE", secret: false);
    public static readonly PemLabel PublicKey = new("PUBLIC KEY", secret: false);
    public static readonly PemLabel PrivateKey = new("PRIVATE KEY", secret: true);
    public static readonly PemLabel EcPrivateKey = new("EC PRIVATE KEY", secret: true);
    public static readonly PemLabel RsaPrivateKey = new("RSA PRIVATE KEY", secret: true);
    public static readonly PemLabel Pkcs7 = new("PKCS7", secret: false);

    public bool Secret { get; }
}

public readonly record struct PemBlock(PemLabel Label, ReadOnlyMemory<byte> Der) {
    public string Digest => ContentHash.Of(Der.Span).ToString("x32");
    public string Armor => PemEncoding.WriteString(Label.Key, Der.Span);
}

public sealed record CredentialBundle(Seq<PemBlock> Blocks) {
    public static CredentialBundle Cert(X509Certificate2 certificate) =>
        new(Seq(new PemBlock(PemLabel.Certificate, certificate.RawData)));

    public FrozenSet<string> Labels => Blocks.Map(static block => block.Label.Key).ToFrozenSet(StringComparer.Ordinal);
    public bool CarriesSecret => Blocks.Exists(static block => block.Label.Secret);
}

public readonly record struct CredentialPemWire(
    string KeyId,
    FrozenSet<string> Labels,
    Seq<string> BlockDigests,
    string BundleDigest,
    Instant At);

[Union]
public abstract partial record PemFault : Expected, IValidationError<PemFault> {
    private PemFault(string detail, int code) : base(detail, code, None) { }
    public static PemFault Create(string message) => new Text(message);
    public sealed record Text : PemFault { public Text(string detail) : base(detail, FaultBand.Pem.Code(0)) { } }
    public sealed record LabelUnknown : PemFault { public LabelUnknown(string label) : base($"{label}: unknown PEM label", FaultBand.Pem.Code(1)) => Label = label; public string Label { get; } }
    public sealed record ArmorMalformed : PemFault { public ArmorMalformed(string detail) : base(detail, FaultBand.Pem.Code(2)) { } }
    public sealed record EmptyBundle : PemFault { public EmptyBundle() : base("empty PEM bundle", FaultBand.Pem.Code(3)) { } }
}

public static class CredentialPem {
    public static string Encode(CredentialBundle bundle) =>
        string.Join('\n', bundle.Blocks.Map(static block => block.Armor));

    public static Fin<CredentialBundle> Decode(string text) {
        var span = text.AsSpan();
        var blocks = Seq<PemBlock>();
        while (PemEncoding.TryFind(span, out var fields)) {
            var label = span[fields.Label].ToString();
            var der = new byte[fields.DecodedDataLength];
            if (!Convert.TryFromBase64Chars(span[fields.Base64Data], der, out _))
                return Fin.Fail<CredentialBundle>(new PemFault.ArmorMalformed(label));
            if (!PemLabel.TryGet(label, out var row))
                return Fin.Fail<CredentialBundle>(new PemFault.LabelUnknown(label));
            blocks = blocks.Add(new PemBlock(row, der));
            span = span[fields.Location.End..];
        }
        return blocks.IsEmpty ? Fin.Fail<CredentialBundle>(new PemFault.EmptyBundle()) : Fin.Succ(new CredentialBundle(blocks));
    }

    public static CredentialPemWire Carrier(CredentialBundle bundle, string keyId, Redactor redactor, ClockPolicy clocks) {
        Span<char> sink = stackalloc char[redactor.GetRedactedLength(keyId)];
        var written = redactor.Redact(keyId, sink);
        var digests = bundle.Blocks.Map(static block => block.Digest);
        return new CredentialPemWire(
            KeyId: new string(sink[..written]),
            Labels: bundle.Labels,
            BlockDigests: digests,
            BundleDigest: ContentHash.Of(Encoding.UTF8.GetBytes(string.Concat(digests))).ToString("x32"),
            At: clocks.Now);
    }
}
```

## [04]-[TS_PROJECTION]

- Owner: `CredentialPemWire` — the redacted credential-bundle carrier the TS `security/crypt/sign` and the Python `runtime/execution/admission` decode; the raw bundle text crosses as the standard RFC-7468 PEM string the consumers parse through their own key surfaces.
- Entry: the bundle text crosses as the canonical multi-element PEM string (`-----BEGIN/END-----` armored blocks joined by `\n`), and the redacted carrier crosses as `CredentialPemWire` so a consumer reads the bundle's label set and content digests without the private-key bytes.
- Packages: BCL inbox
- Growth: one wire-member row per new carrier field; the label set crosses as a string array of the closed RFC-7468 labels; zero new surface.
- Boundary: the PEM bundle text crosses as the standard RFC-7468 armored string so a consumer's own PEM parser (`jose` `importPKCS8`/`importSPKI`/`importX509` on TS, `cryptography.hazmat` `load_pem_*` on Python) reads the same bytes the BCL `PemEncoding` wrote, never a re-minted base64 envelope; the carrier never carries a private-key block's content — only the label set, the per-block kernel content digests, and the redacted key-id cross — so the TS and Python verifiers read the credential's public identity off the wire while the private material stays host-side; the bundle separator is the RFC-7468 armor itself, so a consumer splits blocks on the `-----BEGIN-----`/`-----END-----` boundary its PEM parser already owns, never a `--SEP--` token.

```ts contract
type PemLabelKey =
  | "CERTIFICATE"
  | "PUBLIC KEY"
  | "PRIVATE KEY"
  | "EC PRIVATE KEY"
  | "RSA PRIVATE KEY"
  | "PKCS7";

interface CredentialPemWire {
  readonly keyId: string;
  readonly labels: ReadonlyArray<PemLabelKey>;
  readonly blockDigests: ReadonlyArray<string>;
  readonly bundleDigest: string;
  readonly at: string;
}
```

## [05]-[RESEARCH]

- [ZEROIZE_PRIMITIVE]: `System.Security.Cryptography.CryptographicOperations.ZeroMemory(Span<byte>)` is the doctrine integrity owner's overwrite member the `SecretLease` zeroize and renew paths bind, catalogued at the substrate tier. The rotation-diff digest is the kernel `ContentHash.Of` identity value — non-cryptographic, forbidden a security claim — so no constant-time comparison member is bound and none is catalogued.
- [KMS_UNWRAP_PORT]: the per-store-open KMS-unwrap handle `Rasm.Persistence/Element/identity#KEY_ENVELOPE` reads crosses through the `[PORT]: KMS-unwrap port` seam as one `SecretLease`-class content carrier — the lease owns the acquire-renew-zeroize custody of the cloud-KMS CMK access the Persistence `Element/identity#KEY_ENVELOPE` `EnvelopeKeyring` (the provider-neutral `Mint`/`Unwrap`/`Rewrap`/`Probe` delegate quartet one `KmsProvider` row projects, each arm's mechanism a policy value on that row — AWS `GenerateDataKey`/`Decrypt`/`ReEncrypt` encrypt-as-wrap, Azure native `WrapKey`/`UnwrapKey` RSA-OAEP-256, GCP `Encrypt`/`Decrypt` encrypt-as-wrap with bidirectional CRC32C integrity and `UpdateCryptoKeyPrimaryVersion` primary-version repoint, never one arm's spelling masquerading as a universal law) binds against, so the in-process key-handle lifecycle stays the runtime lease's concern and Persistence consumes the resolved per-open handle, never minting a long-lived in-process key; the `Probe` arm resolves the key's lifecycle `KeyState` so a wrap against a disabled or destroy-scheduled key rejects at admission, the `Rewrap` arm advances the version ladder per the row's rotation kind, the unwrapped DEK zeroizes through the same `CryptographicOperations.ZeroMemory` path and the handle is a content carrier riding this lifecycle, never an eighth port, per `Runtime/ports#PORT_RECORDS` (a `SecretLease` row is never promoted to a port). The concrete `KmsProvider` axis (`AWSSDK.KeyManagementService`/`Azure.Security.KeyVault.Keys`/`Google.Cloud.Kms.V1`) and the per-arm AAD binding stay Persistence-side — the `EnvelopeAad` carrying the store partition and (under RLS) the `TenantContext.TenantId.Uuid` digest, ridden as the provider `EncryptionContext`/`AdditionalAuthenticatedData` exact-match on the AWS/GCP `context` arms and compared application-side against the persisted digest on the Azure native-wrap `application` arm; AppHost surfaces only the lease-managed handle custody.
- [PEM_ENCODING]: the `System.Security.Cryptography.PemEncoding` armor owner the `CredentialPem` axis binds — `PemEncoding.WriteString(ReadOnlySpan<char> label, ReadOnlySpan<byte> data)` for the per-block armor write, `PemEncoding.TryFind(ReadOnlySpan<char> pemData, out PemFields fields)` for the multi-element walk, and the `PemFields` struct (`Location`/`Label`/`Base64Data`/`DecodedDataLength` ranges) the decoder reads — plus `System.Security.Cryptography.X509Certificates.X509Certificate2.RawData` (the cert's DER bytes the `CredentialBundle.Cert` factory armors) and `X509Certificate2.CreateFromPem(ReadOnlySpan<char> certPem)` for the certificate round-trip the decoder proves, all catalogued at the substrate tier. The `PemBlock.Der` `ReadOnlyMemory<byte>` is the raw DER the armor encodes; the `Decode` fold reads `PemFields.DecodedDataLength` and `PemFields.Base64Data` to size and fill the DER through `Convert.TryFromBase64Chars`, never a fabricated single decode member. The bundle separator is the RFC-7468 `-----END-----`/`-----BEGIN-----` armor boundary joined by `\n`, never a `--SEP--` token: the textual-encoding self-delimitation IS the separator, so no separator constant is minted.
- [CREDENTIAL_WIRE_CONSUMERS]: the C#-minted `CredentialBundle` PEM text and the redacted `CredentialPemWire` carrier are the suite's only credential-material wire vocabulary — TS `security/crypt/sign` `Material.admit` decodes the certificate/public-key blocks through `jose`'s own importers and Python `runtime/execution/admission` `SettingsAdmission` reads the bundle through `cryptography.hazmat.primitives.serialization` `load_pem_*`, both decoding the one host-minted RFC-7468 bundle and re-minting no parallel PEM vocabulary per architecture#CROSS_LANGUAGE_WIRE. The `\n`-joined armor is the wire contract; the consumer-side `load_pem_*`/key-import spellings are the peer branches' own admissions, not this page's fences.
