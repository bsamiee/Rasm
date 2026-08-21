# [APPHOST_SECRETS_AND_CREDENTIAL_MATERIAL]

`SecretLease` owns the credential-material lifecycle: one row family acquires, rotates, and zeroizes credential material against the RID-dispatched credential-store provider the host resolves through `Runtime/config#SOURCE_AXIS`'s `ConfigSource.SecretsStore` row, and it carries the per-store-open KMS-unwrap handle `Rasm.Persistence/Element/identity#KMS_CUSTODY` reads as one `SecretLease`-class content carrier so the cloud-KMS key-handle lifecycle stays the runtime lease's concern rather than a long-lived Persistence-side key. `CredentialPem` is the suite's only credential-material wire vocabulary: the host encodes every PEM-bearing credential into one canonical RFC-7468 multi-element bundle, mints the redacted `CredentialPemWire` carrier the TS verifier and the Python admission decode, and never crosses a raw `byte[]` or a parallel base64 envelope. Owned axes: the secret-lease lifecycle, the credential-PEM encoding vocabulary, and the KMS-unwrap custody over System.Security.Cryptography (the BCL `PemEncoding`/`X509CertificateLoader` owners), Microsoft.Extensions.Compliance.Redaction, the kernel `CanonicalWriter`/`ContentHash` identity capsule, kernel `Cell`/`Transition`, Riok.Mapperly, Generator.Equals, NodaTime, Thinktecture.Runtime.Extensions, and LanguageExt.Core.

## [01]-[INDEX]

- [02]-[SECRET_LEASE]: Acquire-rotate-zeroize credential lifecycle extending the `SecretsStore` source row.
- [03]-[CREDENTIAL_PEM]: Canonical RFC-7468 bundle encoding and the one generated redacted-carrier seam.
- [04]-[TS_PROJECTION]: The redacted credential-bundle wire shape and its `jose` decode.

## [02]-[SECRET_LEASE]

- Owner: `SecretLease` the live credential row extending `ConfigSource.SecretsStore`; `RotationBand` the admitted cadence pair; `SecretRuntime` the boundary capsule; `LeaseTransition` `[Union]` lifecycle vocabulary; `SecretFault` `[Union]` fault family riding the kernel `[FaultCase]`/`Fault` floor (`[FaultCase]` realizes the registry over `FaultBand.Secret`); `SecretReceipt` the redacted rotation evidence; `SecretLeaseOps` the acquire-rotate-zeroize fold.
- Cases: `LeaseTransition` = Acquired | Renewed | Refused | Zeroized; `SecretFault` = AcquireRejected | RenewMissed | RotationUnbanded.
- Entry: `Acquire(SecretRuntime runtime, string keyId)` returns `IO<Fin<Atom<SecretLease>>>` — the store read seats the LIVE lease cell and registers that cell's renewal occurrence on `SecretRuntime.Schedule` in one bind, so a returned cell is always a rotating cell; `Rotate(SecretRuntime runtime, Atom<SecretLease> cell)` returns `IO<Unit>` — the renewal occurrence's `Work` binding, committing the re-pull through kernel `Cell.Commit`; `Renew(SecretRuntime runtime, SecretLease lease)` returns `IO<Fin<SecretLease>>`, re-pulling inside the live window and zeroizing the prior copy; `Zeroize(SecretRuntime runtime, SecretLease lease)` returns `IO<Unit>`, the drain-forced terminal.
- Auto: `RotationBand.Of` reads the credential's lifetime row AND that row's own `Escalation` skew off the `Runtime/time#DEADLINE_TAXONOMY` roster, so the renewal period is `Life - Skew` and derives — a cadence equal to the lifetime fires the re-pull exactly at expiry, which is the shape under which every occurrence read `RenewMissed` and the lease silently died under prose promising rotation ahead of expiry; a lifetime row declaring no skew refuses at composition as `RotationUnbanded` rather than seating a rotation that cannot succeed. `Acquire` registers one `ScheduleEntry` on the bound `Runtime/time#SCHEDULE_PORT` delegate carrying the entry's own `RedrivePolicy` and a `LeasePolicy` whose `CrashStaleness` outlives the renewal window, so one occurrence row drives every credential with no per-secret timer. The rotation commit rides kernel `Cell.Commit`, so a lost CAS reports `Contended` instead of publishing as success, and the renewal verdict rides the SWAPPED value: a refused re-pull commits the prior lease carrying its `Refusal`, which `Observability/health#HEALTH_REGISTRY` reads on its `ContributorTag.Store`-tagged row and `DegradationPolicy.Derive` maps to `DegradationLevel.ReadOnly` — a level nothing produced while the refusal lived only in a discarded fan. Zeroization registers as one `Runtime/lifecycle#DRAIN_CONDUCTOR` `DrainBand.Stores` participant row under the drain-forced token, so a hung renewal never strands a live secret; the credential bytes carry `DataClassification.Secret` so `Observability/telemetry#REDACTION_TAXONOMY` erases them at every egress.
- Receipt: `SecretReceipt` carries the transition key, the lease window, the kernel content digest of the material, the redacted credential id, and the refusal detail — never a secret byte and never a raw key id; every transition fans through `ReceiptSinkPort.Send` under `ReceiptKind.Secret`, partitioned by `TenantId` so each tenant's rotation stream stays isolated.
- Packages: Rasm (kernel `CanonicalWriter`/`ContentHash`, `Cell`/`Transition`), Microsoft.Extensions.Configuration.UserSecrets, Microsoft.Extensions.Compliance.Redaction, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: one lifecycle transition is one `LeaseTransition` case; one refusal is one `SecretFault` case; a new credential class is one `DeadlineClass` lifetime row with its skew escalation, admitted through the same `RotationBand.Of`; a new credential source is one `SecretsSource` provider value on the existing `ConfigLayer`, never a second lease owner; zero new surface.
- Boundary: the lease is the suite's only credential lifecycle owner — a per-secret rotation helper, a raw `string` credential field, and a second zeroization path are the deleted forms; a failed re-pull keeps the current lease live and degrades through the health rail, never a hard fault; the in-memory copy is a rented `byte[]` overwritten through `CryptographicOperations.ZeroMemory` so no managed copy survives collection; the content identity is the kernel `Rasm.Domain.ContentHash` digest rendered through `ContentHash.Hex` — the one seed-zero content-identity entry, non-cryptographic, carrying identity only and never a security claim, so the rotation diff is a digest equality with no constant-time pretense over a non-crypto hash and no local `x32` render beside the kernel's; `SecretReceipt.ContentDigest` is that digest of the lease's OWN material bytes while the bundle-level identity is `CREDENTIAL_PEM`'s `CredentialPemWire.BundleDigest` — two preimages over two concerns, never one claiming to be the other; every fault detail spells the REDACTED id through the one `SecretRuntime.Redacted` seam, while the receipt carries the bounded structured fault observation, so a raw key id reaches no log, no receipt, and no fault text; the lease holds the live raw `byte[]` and owns only the in-memory lifecycle, while the canonical at-rest and on-wire encoding is `CREDENTIAL_PEM`'s — the lease never encodes material and the PEM axis never holds a live mutable copy; the lease extends the `Runtime/config#SOURCE_AXIS` `ConfigSource.SecretsStore` rank-40 frozen-class row, so the credential never re-mounts at runtime, the lease owns the live rotation above that frozen mount, and `SecretRuntime.Read` acquires through `ConfigLayer.SecretsSource` rather than a parallel provider — the one credential reader the suite carries; NAMED LOSS — `LeaseTransition.Released` deletes unread because a release without a wipe is not a lawful terminal for credential material; the per-store-open KMS-unwrap handle `Rasm.Persistence/Element/identity#KMS_CUSTODY` reads crosses as one `SecretLease`-class content carrier through the `Runtime ⇄ Rasm.Persistence/Element/identity # [PORT]: KMS-unwrap port` seam — the lease owns acquire-rotate-zeroize custody of the cloud-KMS CMK access the Persistence `EnvelopeKeyring` `Mint`/`Unwrap`/`Rewrap`/`Probe` delegate quartet binds against, where each arm's mechanism is a policy value on the `KmsProvider` row (AWS encrypt-as-wrap, Azure native `WrapKey`/`UnwrapKey`, GCP encrypt-as-wrap with CRC32C and primary-version repoint) rather than one arm's spelling as a universal law, so the in-process key-handle lifecycle stays the runtime lease's concern, Persistence consumes the resolved per-open handle without minting a long-lived in-process key, and the unwrapped DEK zeroizes through the same path — a Persistence-side long-lived key cache or a second credential lifecycle is the deleted form, and the handle is a content carrier riding this lifecycle, never an eighth port; the `Rasm.AppUi` PDF digital-signature arm composes this owner's lease-scoped credential export for its `IDigitalSigner` material, so acquire-rotate-zeroize applies to the signing credential exactly as to any lease and AppUi never holds raw key bytes.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Security.Cryptography;
using System.Text.Json;
using LanguageExt;
using Microsoft.Extensions.Compliance.Redaction;
using NodaTime;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.AppHost.Runtime;

// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LeaseTransition {
    private LeaseTransition() { }

    public sealed record Acquired(Interval Window) : LeaseTransition;
    public sealed record Renewed(Interval Window) : LeaseTransition;
    public sealed record Refused(Instant At) : LeaseTransition;
    public sealed record Zeroized(Instant At) : LeaseTransition;

    // ONE key spelling per case: the receipt column and every operator render read this rather than a runtime
    // type name, so a case rename moves the readers at compile time instead of silently re-labelling a stream.
    public string Key => Map(
        acquired: nameof(Acquired),
        renewed: nameof(Renewed),
        refused: nameof(Refused),
        zeroized: nameof(Zeroized));
}

// --- [ERRORS] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SecretFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Secret;
    private SecretFault(string detail) => Detail = detail;
    public string Detail { get; }
    public sealed override string Message => Detail;

    [FaultCase(0)]
    public sealed partial record AcquireRejected(string Redacted, Error Cause)
        : SecretFault($"{Redacted}: {Cause.Message}"), ICausedFault;
    [FaultCase(1)]
    public sealed partial record RenewMissed : SecretFault {
        public RenewMissed(string redacted, string detail) : base($"{redacted}: {detail}") => Redacted = redacted;
        public string Redacted { get; }
    }
    // The cadence refusal is a COMPOSITION fault, not a runtime one: a lifetime row carrying no escalation skew
    // can only seat a renewal that fires at expiry, so the band refuses before any credential is read.
    [FaultCase(2)]
    public sealed partial record RotationUnbanded : SecretFault {
        public RotationUnbanded(string row) : base($"{row}: no escalation skew") => Row = row;
        public string Row { get; }
    }
}

// --- [MODELS] -------------------------------------------------------------------------------
// The cadence is a PAIR of roster rows, never two literals: `Life` is the credential's declared lifetime bound
// and `Skew` its own declared escalation, so `Period` DERIVES and no call site can seat a renewal landing at
// expiry. `Window` is the one lease-interval mint — `ClockPolicy` carries no radius-window member to borrow.
public readonly record struct RotationBand(DeadlineClass Life, DeadlineClass Skew) {
    public static Fin<RotationBand> Of(DeadlineClass life) =>
        life.Escalation
            .Filter(skew => skew.Allotted < life.Allotted)
            .Map(skew => new RotationBand(Life: life, Skew: skew))
            .ToFin(new SecretFault.RotationUnbanded(life.Key));

    public Duration Period => Life.Allotted - Skew.Allotted;
    public Interval Window(Instant from) => new(start: from, end: from + Life.Allotted);
}

public sealed record SecretReceipt(
    string RedactedId,
    string Transition,
    Interval Window,
    string ContentDigest,
    Option<FaultObservationWire> Refusal,
    Instant At);

// `Schedule` is the `Runtime/time#SCHEDULE_PORT` registration delegate the composition root binds — the same
// shape `OrchestrationRuntime` carries. Without it the renewal entry was inert data on the lease record: no
// occurrence fired, so `Rotate` and `LeaseTransition.Renewed` had no live producer.
public sealed record SecretRuntime(
    Func<string, Fin<byte[]>> Read,
    Func<ScheduleEntry, IO<Unit>> Schedule,
    Redactor Redactor,
    LeasePolicy Lease,
    RotationBand Rotation,
    RedrivePolicy Redrive,
    ClockPolicy Clocks,
    ReceiptSinkPort Sink,
    TenantContext Tenant,
    CorrelationId Correlation) {
    // THE redaction seam. Every fault text, every receipt column, and every schedule key spells the id through
    // here, so a raw credential id is unspellable downstream rather than merely discouraged at each site.
    public string Redacted(string keyId) {
        Span<char> sink = stackalloc char[Redactor.GetRedactedLength(keyId)];
        int written = Redactor.Redact(keyId, sink);
        return new string(sink[..written]);
    }
}

// `Refusal` is the renewal verdict riding the SWAPPED value: a re-pull that failed commits the prior lease
// carrying its cause, so the health contributor reads the refusal off the cell rather than re-deriving it from
// a level nothing published (`DECISION_UNDERIVABLE_FROM_STATE`). Its error is always a banded `SecretFault`,
// because `Renew` is the only producer of this column.
[Equatable]
public sealed partial record SecretLease(
    string KeyId,
    byte[] Material,
    Interval Window,
    Option<Error> Refusal,
    ScheduleEntry Renewal) {
    public string Digest => ContentHash.Hex(ContentHash.Of(Material));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class SecretLeaseOps {
    // Acquire REGISTERS the renewal occurrence it constructs — the seat and the registration are one bind, so a
    // returned cell is always a rotating cell and a constructed-but-unregistered entry never escapes.
    public static IO<Fin<Atom<SecretLease>>> Acquire(SecretRuntime runtime, string keyId) =>
        runtime.Read(keyId)
            .MapFail(error => (Error)new SecretFault.AcquireRejected(runtime.Redacted(keyId), error))
            .Match(
                Succ: material => Seat(runtime, keyId, material)
                    .Bind(cell => runtime.Schedule(cell.Value.Renewal).Map(_ => Fin.Succ(cell))),
                Fail: error => IO.pure(Fin.Fail<Atom<SecretLease>>(error)));

    // Exemption: the renewal occurrence's `Work` closes over the very cell this seat is minting, so the handle
    // is assigned after construction — a knot no expression form unties, and `Acquire` is its only caller.
    static IO<Atom<SecretLease>> Seat(SecretRuntime runtime, string keyId, byte[] material) {
        Atom<SecretLease>? cell = null;
        Interval window = runtime.Rotation.Window(runtime.Clocks.Now);
        ScheduleEntry renewal = new(
            Key: $"secret-renew:{runtime.Redacted(keyId)}",
            Spec: new OccurrenceSpec.Every(Period: runtime.Rotation.Period),
            Deadline: runtime.Rotation.Life,
            Lease: Some(runtime.Lease),
            Redrive: runtime.Redrive,
            Work: () => Rotate(runtime, cell!));
        return Emit(runtime, new SecretLease(keyId, material, window, None, renewal), new LeaseTransition.Acquired(window))
            .Map(seated => cell = Atom(seated));
    }

    // The occurrence IS the rotation: `Renew` re-pulls inside the live window and zeroizes the prior copy, and
    // the kernel commit publishes the outcome — a fresh lease on success, the prior lease carrying its refusal
    // on failure, and a `Contended` transition where a discarded `Swap` reported a lost CAS as success.
    public static IO<Unit> Rotate(SecretRuntime runtime, Atom<SecretLease> cell) =>
        Renew(runtime, cell.Value).Bind(outcome =>
            Cell.Commit(cell, held => outcome.Match(
                    Succ: static renewed => renewed,
                    Fail: error => held with { Refusal = Some(error) }))
                .Switch(
                    committed: row => Emit(runtime, row.State, outcome.Match(
                        Succ: static renewed => (LeaseTransition)new LeaseTransition.Renewed(renewed.Window),
                        Fail: _ => new LeaseTransition.Refused(runtime.Clocks.Now))),
                    // `Cell.Commit` answers only Committed or Contended; the two seat-and-step cases below are
                    // unreachable here and stay spelled so a widened kernel family breaks this dispatch.
                    ceded: static row => IO.pure(row.State),
                    refused: static row => IO.pure(row.State),
                    contended: row => Emit(
                        runtime,
                        row.State with { Refusal = Some((Error)new SecretFault.RenewMissed(
                            runtime.Redacted(row.State.KeyId), $"rotation lost {row.Attempts.Value} commit rounds")) },
                        new LeaseTransition.Refused(runtime.Clocks.Now)))
                .Map(static _ => unit));

    public static IO<Fin<SecretLease>> Renew(SecretRuntime runtime, SecretLease lease) =>
        runtime.Clocks.Now is var now && now >= lease.Window.End
            ? IO.pure(Fin.Fail<SecretLease>(new SecretFault.RenewMissed(runtime.Redacted(lease.KeyId), "window closed before renewal")))
            : IO.pure(runtime.Read(lease.KeyId)
                .Map(material => {
                    // Exemption: zeroization is a void BCL mutation over the retiring buffer, and it must land
                    // before the fresh lease is observable, so the fold carries it rather than a later hook.
                    CryptographicOperations.ZeroMemory(lease.Material);
                    return lease with { Material = material, Window = runtime.Rotation.Window(now), Refusal = None };
                }));

    public static IO<Unit> Zeroize(SecretRuntime runtime, SecretLease lease) =>
        Emit(runtime, lease, new LeaseTransition.Zeroized(runtime.Clocks.Now))
            .Map(retired => { CryptographicOperations.ZeroMemory(retired.Material); return unit; });

    // ONE fan site. The effect is RETURNED rather than run-and-discarded: the deleted `ignore(...Run())` shape
    // dropped every send fault on the floor and made the receipt stream a claim nothing proved.
    static IO<SecretLease> Emit(SecretRuntime runtime, SecretLease lease, LeaseTransition transition) =>
        runtime.Sink.Send(
                runtime.Correlation, runtime.Tenant, TelemetrySource.AppHost, ReceiptKind.Secret.Key,
                JsonSerializer.SerializeToElement(
                    new SecretReceipt(
                        RedactedId: runtime.Redacted(lease.KeyId),
                        Transition: transition.Key,
                        Window: lease.Window,
                        ContentDigest: lease.Digest,
                        Refusal: lease.Refusal.Map(AppHostFaultMap.Wire),
                        At: runtime.Clocks.Now),
                    SuiteContracts.Host))
            .Map(_ => lease);
}
```

## [03]-[CREDENTIAL_PEM]

- Owner: `PemLabel` `[SmartEnum<string>]` the closed RFC-7468 label vocabulary under the `ComparerAccessors.StringOrdinalIgnoreCase` accessor pair, carrying the secrecy column and its own `jose` importer name; `PemBlock` the single armored element; `CredentialBundle` the ordered multi-element bundle; `CredentialAttestation` the admitted projection input; `CredentialPemWire` the redacted cross-language carrier; `CredentialPemMap` the one generated wire seam; `PemFault` `[Union]` banding through `FaultBand.Pem`; `CredentialPem` the encode-decode-attest surface.
- Cases: 6 label rows — certificate, public-key, pkcs7, private-key, ec-private-key, rsa-private-key — the armor labels the BCL `PemEncoding` writes between the `-----BEGIN {label}-----`/`-----END {label}-----` lines; `PemFault` = LabelUnknown | ArmorMalformed | CertRejected | EmptyBundle.
- Entry: `Encode(CredentialBundle bundle)` returns `string` — one fold writes each block through `PemEncoding.WriteString(label, der)` and joins the armored elements with the single `\n` inter-block delimiter, so a certificate chain with its private key crosses as one canonical bundle text whose element boundary is the armor pair itself, never a hand-built `--SEP--` token; `Decode(string text)` returns `Fin<CredentialBundle>` — one walk over `PemEncoding.TryFind` peels each armored element into a `PemBlock` and proves every `CERTIFICATE` block through `X509CertificateLoader.LoadCertificate` before admission; `Carrier(CredentialBundle bundle, string keyId, SecretRuntime runtime)` returns `CredentialPemWire` — the redaction and the clock read ONCE at the boundary into `CredentialAttestation`, and the generated `CredentialPemMap.Wire` transcribes the carrier.
- Auto: the bundle is the canonical wire shape the `SecretLease` produces and the TS verifier consumes — a credential wire crossing as a raw `byte[]`, a bare base64 string, or a hand-built separator envelope is the deleted form, because the RFC-7468 armor IS self-delimiting and the `\n` between an `-----END-----` and the next `-----BEGIN-----` is the only inter-block byte; `CredentialBundle.Cert(X509Certificate2)` derives a certificate bundle from the cert's own `RawData` so the host never hand-encodes bytes the BCL already owns, and `Decode`'s `X509CertificateLoader.LoadCertificate` proof means an armored block that is not a real certificate refuses as `CertRejected` rather than admitting as opaque DER — the claim the deleted page made and never produced; a secret-labeled block carries `DataClassification.Secret` so `Observability/telemetry#REDACTION_TAXONOMY` erases its bytes at every egress; the per-block digest is the kernel `ContentHash.Of` value over the block's DER span, non-cryptographic and forbidden a security claim, so the carrier proves bundle identity without exposing material; the whole-bundle digest folds the ordered `(label, block digest)` rows through the kernel `CanonicalWriter.Rows` — count-framed, with each label length-framed ahead of its bytes — so a two-block bundle and a differently-split one over the same bytes cannot key alike and a hex-string concatenation, order-blind at the block boundary, is the deleted form; the carrier itself is GENERATED, so a column added to `CredentialPemWire` is an RMG012/RMG013 build break rather than a hand construction that silently transcribes a default.
- Receipt: the rotation rides `SECRET_LEASE`'s `SecretReceipt`, so the PEM axis adds no parallel receipt — `CredentialPemWire` is the redacted projection the sink already fans and `BundleDigest` is the bundle-level identity beside the lease's own material digest.
- Packages: Rasm (kernel `CanonicalWriter`/`ContentHash`), System.Security.Cryptography, Riok.Mapperly, Generator.Equals, Microsoft.Extensions.Compliance.Redaction, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: one armor label is one `PemLabel` row carrying its secrecy and its importer name; one bundle-element kind is one `PemBlock` inside the existing ordered bundle, never a parallel envelope; one refusal is one `PemFault` case; a new credential material kind rides the label axis already; zero new surface.
- Boundary: the PEM axis is the suite's only credential-material wire owner — the lease holds the live `byte[]` and zeroizes it while `CredentialPem` owns the canonical at-rest and on-wire encoding, so the two never merge and never split the material into two encodings; the BCL `PemEncoding` owns the armor write and find, `X509CertificateLoader` owns DER admission, and `X509Certificate2.RawData` owns the export — a hand-rolled base64 wrap, a manual `-----BEGIN-----` string build, and a third-party PEM codec are the deleted forms; the projection is ONE Mapperly `[Mapper]` under `RequiredMappingStrategy.Both`, so target completeness is a compile proof and the hand `new CredentialPemWire(...)` mint that could skip a column is deleted; `PemLabel` carries the importer NAME beside its secrecy column because the TS peer's importer table is keyed by that same label — one vocabulary, two readers, no second table to drift; the private-key block never crosses in the carrier: only the public chain, the label set, and the digests cross, so a TS or Python verifier reads the credential's public identity while the private material stays host-side under the lease's zeroization; `Chain` is the armored PUBLIC bundle text and is exactly what `jose`'s `importSPKI`/`importX509` consume, so a carrier that carried labels and digests alone gave the peer nothing importable — the field is the capability, not a convenience; `importPKCS8` never enters the decode list because the wire carries no private block for it to read; the bundle crosses to TS as the `CredentialPemWire` the `security/crypt/sign` `Material.admit` fold decodes per `libs/.planning/ARCHITECTURE.md` `[07]-[CROSS_LANGUAGE_WIRE]` — the one consumer decodes the one C#-minted bundle and re-mints no parallel PEM vocabulary; an unknown armor label decodes to `LabelUnknown` rather than admitting an unrecognized block.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Collections.Frozen;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Generator.Equals;
using LanguageExt;
using NodaTime;
using Rasm.Domain;
using Riok.Mapperly.Abstractions;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.AppHost.Runtime;

// --- [TYPES] --------------------------------------------------------------------------------
// `Importer` is the label's OWN column rather than a second table at the consumer: the TS peer keys its `jose`
// importer map on this same vocabulary, so the two ends read one roster and a new label lands its importer here.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class PemLabel {
    public static readonly PemLabel Certificate = new("CERTIFICATE", secret: false, importer: Some("importX509"));
    public static readonly PemLabel PublicKey = new("PUBLIC KEY", secret: false, importer: Some("importSPKI"));
    public static readonly PemLabel Pkcs7 = new("PKCS7", secret: false, importer: None);
    public static readonly PemLabel PrivateKey = new("PRIVATE KEY", secret: true, importer: None);
    public static readonly PemLabel EcPrivateKey = new("EC PRIVATE KEY", secret: true, importer: None);
    public static readonly PemLabel RsaPrivateKey = new("RSA PRIVATE KEY", secret: true, importer: None);

    public bool Secret { get; }

    public Option<string> Importer { get; }
}

// --- [ERRORS] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PemFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Pem;
    private PemFault(string detail) => Detail = detail;
    public string Detail { get; }
    public sealed override string Message => Detail;

    [FaultCase(0)]
    public sealed partial record LabelUnknown : PemFault {
        public LabelUnknown(string label) : base($"{label}: unknown PEM label") => Label = label;
        public string Label { get; }
    }
    [FaultCase(1)]
    public sealed partial record ArmorMalformed : PemFault {
        public ArmorMalformed(string label) : base($"{label}: base64 body outside armor") => Label = label;
        public string Label { get; }
    }
    [FaultCase(2)]
    public sealed partial record CertRejected(Error Cause)
        : PemFault($"CERTIFICATE: {Cause.Message}"), ICausedFault;
    [FaultCase(3)]
    public sealed partial record EmptyBundle : PemFault { public EmptyBundle() : base("empty PEM bundle") { } }
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct PemBlock(PemLabel Label, ReadOnlyMemory<byte> Der) {
    public UInt128 Content => ContentHash.Of(Der.Span);
    public string Digest => ContentHash.Hex(Content);
    public string Armor => PemEncoding.WriteString(Label.Key, Der.Span);
}

// Equality is GENERATED: `Blocks` is a `Seq` the synthesized record form compares by reference, and block ORDER
// is part of the bundle's identity, so the ordered comparer is the one that agrees with the digest below.
[Equatable]
public sealed partial record CredentialBundle([property: OrderedEquality] Seq<PemBlock> Blocks) {
    public static CredentialBundle Cert(X509Certificate2 certificate) =>
        new(Seq(new PemBlock(PemLabel.Certificate, certificate.RawData)));

    public FrozenSet<string> Labels => Blocks.Map(static block => block.Label.Key).ToFrozenSet(StringComparer.Ordinal);
    public bool CarriesSecret => Blocks.Exists(static block => block.Label.Secret);

    // The public half is a BUNDLE, not a filtered text: every downstream member — armor, labels, digest —
    // reads the same owner, so the chain the peer imports and the digests it checks come off one shape.
    public CredentialBundle Public => new(Blocks.Filter(static block => !block.Label.Secret));

    // Count-framed rows with each label length-framed ahead of its block digest, so a differently split bundle
    // over identical bytes keys apart (`DIGEST_OVER_UNORDERED_CONTAINER`).
    public UInt128 Digest => ContentHash.Of(Blocks, static (blocks, writer) =>
        writer.Rows(blocks, static (block, inner) => inner.String(block.Label.Key).U128(block.Content)));
}

// The projection's ADMITTED input: the redaction and the clock read once at the boundary, so the generated
// mapper below transcribes evidence and never reaches for a service.
public readonly record struct CredentialAttestation(string KeyId, Instant At, CredentialBundle Bundle);

[Equatable]
public readonly partial record struct CredentialPemWire(
    string KeyId,
    [property: UnorderedEquality] FrozenSet<string> Labels,
    string Chain,
    [property: OrderedEquality] Seq<string> BlockDigests,
    string BundleDigest,
    Instant At);

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class CredentialPem {
    public static string Encode(CredentialBundle bundle) =>
        string.Join('\n', bundle.Blocks.Map(static block => block.Armor));

    // Exemption: `PemEncoding.TryFind` walks a `ReadOnlySpan<char>` that no lambda may capture, so the peel is a
    // language-forced loop; every refusal still leaves on the rail and the accumulator is local to the walk.
    public static Fin<CredentialBundle> Decode(string text) {
        ReadOnlySpan<char> span = text.AsSpan();
        Seq<PemBlock> blocks = Seq<PemBlock>();
        while (PemEncoding.TryFind(span, out PemFields fields)) {
            string label = span[fields.Label].ToString();
            byte[] der = new byte[fields.DecodedDataLength];
            if (!Convert.TryFromBase64Chars(span[fields.Base64Data], der, out _)) {
                return Fin.Fail<CredentialBundle>(new PemFault.ArmorMalformed(label));
            }
            if (!PemLabel.TryGet(label, out PemLabel? row)) {
                return Fin.Fail<CredentialBundle>(new PemFault.LabelUnknown(label));
            }
            blocks = blocks.Add(new PemBlock(row, der));
            span = span[fields.Location.End..];
        }
        return blocks.IsEmpty
            ? Fin.Fail<CredentialBundle>(new PemFault.EmptyBundle())
            : Certified(new CredentialBundle(blocks));
    }

    // The armored bytes PARSE as a certificate or they do not enter: an opaque DER blob wearing a CERTIFICATE
    // label admitted silently and failed later at the consumer's own import, where the cause was gone.
    static Fin<CredentialBundle> Certified(CredentialBundle bundle) =>
        Op.Of().Catch(() => Fin.Succ((bundle.Blocks
                .Filter(static block => block.Label == PemLabel.Certificate)
                .Iter(static block => X509CertificateLoader.LoadCertificate(block.Der.Span).Dispose()), unit).Item2))
            .Map(_ => bundle)
            .MapFail(static error => (Error)new PemFault.CertRejected(error));

    public static CredentialPemWire Carrier(CredentialBundle bundle, string keyId, SecretRuntime runtime) =>
        CredentialPemMap.Wire(new CredentialAttestation(
            KeyId: runtime.Redacted(keyId), At: runtime.Clocks.Now, Bundle: bundle));
}

// --- [BOUNDARIES] ---------------------------------------------------------------------------
// The ONE credential-carrier seam mapper. Target completeness is a compile proof, so a column added to the wire
// breaks here rather than transcribing a default. Every derived column binds a whole-source reader, which
// suppresses source-side RMG020 for this mapping — the `Bundle` ignore below is authored inventory, not proof.
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Both,
        EnabledConversions = MappingConversionType.All & ~MappingConversionType.ExplicitCast)]
internal static partial class CredentialPemMap {
    [MapperIgnoreSource(nameof(CredentialAttestation.Bundle))]
    [MapPropertyFromSource(nameof(CredentialPemWire.Labels), Use = nameof(LabelsOf))]
    [MapPropertyFromSource(nameof(CredentialPemWire.Chain), Use = nameof(ChainOf))]
    [MapPropertyFromSource(nameof(CredentialPemWire.BlockDigests), Use = nameof(DigestsOf))]
    [MapPropertyFromSource(nameof(CredentialPemWire.BundleDigest), Use = nameof(BundleDigestOf))]
    public static partial CredentialPemWire Wire(CredentialAttestation attestation);

    [NamedMapping(nameof(LabelsOf))]
    static FrozenSet<string> LabelsOf(CredentialAttestation row) => row.Bundle.Labels;

    // The PUBLIC half alone is armored into the chain, while the digests below cover every block — secret ones
    // included — so the peer imports what it can read and still verifies the whole bundle's identity.
    [NamedMapping(nameof(ChainOf))]
    static string ChainOf(CredentialAttestation row) => CredentialPem.Encode(row.Bundle.Public);

    [NamedMapping(nameof(DigestsOf))]
    static Seq<string> DigestsOf(CredentialAttestation row) => row.Bundle.Blocks.Map(static block => block.Digest);

    [NamedMapping(nameof(BundleDigestOf))]
    static string BundleDigestOf(CredentialAttestation row) => ContentHash.Hex(row.Bundle.Digest);
}
```

## [04]-[TS_PROJECTION]

- Owner: `CredentialPemWire` — the redacted credential-bundle carrier registered on the `apphost-wire` seam (`tests/contracts/MANIFEST.md` `[02.21]-[APPHOST_WIRE]`, one `[JsonSerializable]` row on the `Runtime/ports#WIRE_LAW` roster, decoded under the TS census `json` arm).
- Law: this page MINTS the carrier and `libs/typescript/core/.planning/interchange/codec.md` `Credential` BINDS the decode, which `libs/typescript/security/.planning/crypt/sign.md` `[03]-[KEY_MATERIAL]` folds into a `KeyHandle` — `Material.admit`'s `Attested` arm reads the LEAF block out of `chain` and routes it through the label-keyed `jose` importer table, so `chain` is the field the whole consumer path runs on and a carrier without it left the peer holding labels and digests it could not import. The codec filters every secret-carrying label at decode, so the broken-mint leak refuses at the wire and `importPKCS8` reads nothing this carrier holds. The one consumer decodes the one C#-minted vocabulary and re-mints none, so a column added here is a decode-side widening at the counterpart, never a second PEM family.
- Entry: the redacted carrier crosses as `CredentialPemWire`; `chain` carries the canonical multi-element PEM string (armored blocks joined by `\n`) so a consumer's own parser reads the same bytes the BCL `PemEncoding` wrote.
- Packages: BCL inbox
- Growth: one wire-member row per new carrier field; the label set crosses as a string array of the closed RFC-7468 labels; zero new surface.
- Boundary: the carrier never carries a private-key block's content — only the public chain, the label set, the per-block kernel content digests, and the redacted key-id cross — so the TS and Python verifiers read the credential's public identity while the private material stays host-side; the bundle separator is the RFC-7468 armor itself, so a consumer splits blocks on the `-----BEGIN-----`/`-----END-----` boundary its PEM parser already owns, never a `--SEP--` token; `blockDigests` covers every block the bundle held, including the secret ones the chain omits, so a peer proving a landing against the digests is proving the whole credential's identity and not just the half it received.

```ts signature
type PemLabelKey =
  | "CERTIFICATE"
  | "PUBLIC KEY"
  | "PKCS7"
  | "PRIVATE KEY"
  | "EC PRIVATE KEY"
  | "RSA PRIVATE KEY";

interface CredentialPemWire {
  readonly keyId: string;
  readonly labels: ReadonlyArray<PemLabelKey>;
  // The armored public chain — `Material.admit` matches its leaf block and hands it to `importX509`/`importSPKI`.
  readonly chain: string;
  readonly blockDigests: ReadonlyArray<string>;
  readonly bundleDigest: string;
  readonly at: string;
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
