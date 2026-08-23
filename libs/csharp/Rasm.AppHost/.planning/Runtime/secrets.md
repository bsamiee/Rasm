# [APPHOST_SECRETS_AND_CREDENTIAL_MATERIAL]

`SecretLease` owns the credential-material lifecycle: one row family acquires, rotates, and zeroizes credential material against the RID-dispatched credential-store provider the host resolves through `Runtime/config#SOURCE_AXIS`'s `ConfigSource.SecretsStore` row, and it carries the per-store-open KMS-unwrap handle `Rasm.Persistence/Element/identity#KMS_CUSTODY` reads as one `SecretLease`-class content carrier so the cloud-KMS key-handle lifecycle stays the runtime lease's concern rather than a long-lived Persistence-side key. `CredentialPublic` is the suite's only credential-material wire vocabulary: the host admits every public credential as raw DER under one closed two-arm material family, projects the generated `Credential.V1.CredentialPublicWire` the TypeScript verifier decodes, and crosses no armored text, bare `byte[]`, or parallel base64 envelope. Owned axes are the secret-lease lifecycle, public credential-material admission, and KMS-unwrap custody over System.Security.Cryptography, Microsoft.Extensions.Compliance.Redaction, generated protobuf contracts, the kernel identity and transition capsules, Generator.Equals, NodaTime, Thinktecture.Runtime.Extensions, and LanguageExt.Core.

## [01]-[INDEX]

- [02]-[SECRET_LEASE]: Acquire-rotate-zeroize credential lifecycle extending the `SecretsStore` source row.
- [03]-[CREDENTIAL_PEM]: Public DER material admission and the one generated carrier seam.
- [04]-[TS_PROJECTION]: Public credential wire shape and its `jose` decode.

## [02]-[SECRET_LEASE]

- Owner: `SecretLease` the live credential row extending `ConfigSource.SecretsStore`; `RotationBand` the admitted cadence pair; `SecretRuntime` the boundary capsule; `LeaseTransition` `[Union]` lifecycle vocabulary; `SecretFault` `[Union]` fault family riding the kernel `[FaultCase]`/`Fault` floor (`[FaultCase]` realizes the registry over `FaultBand.Secret`); `SecretReceipt` the redacted rotation evidence; `SecretLeaseOps` the acquire-rotate-zeroize fold.
- Cases: `LeaseTransition` = Acquired | Renewed | Refused | Zeroized; `SecretFault` = AcquireRejected | RenewMissed | RotationUnbanded.
- Entry: `Acquire(SecretRuntime runtime, string keyId)` returns `IO<Fin<Atom<SecretLease>>>` — the store read seats the LIVE lease cell and registers that cell's renewal occurrence on `SecretRuntime.Schedule` in one bind, so a returned cell is always a rotating cell; `Rotate(SecretRuntime runtime, Atom<SecretLease> cell)` returns `IO<Unit>` — the renewal occurrence's `Work` binding, committing the re-pull through kernel `Cell.Commit`; `Renew(SecretRuntime runtime, SecretLease lease)` returns `IO<Fin<SecretLease>>`, re-pulling inside the live window and zeroizing the prior copy; `Zeroize(SecretRuntime runtime, SecretLease lease)` returns `IO<Unit>`, the drain-forced terminal.
- Auto: `RotationBand.Of` reads the credential's lifetime row AND that row's own `Escalation` skew off the `Runtime/time#DEADLINE_TAXONOMY` roster, so the renewal period is `Life - Skew` and derives — a cadence equal to the lifetime fires the re-pull exactly at expiry, which is the shape under which every occurrence read `RenewMissed` and the lease silently died under prose promising rotation ahead of expiry; a lifetime row declaring no skew refuses at composition as `RotationUnbanded` rather than seating a rotation that cannot succeed. `Acquire` registers one `ScheduleEntry` on the bound `Runtime/time#SCHEDULE_PORT` delegate carrying the entry's own `RedrivePolicy` and a `LeasePolicy` whose `CrashStaleness` outlives the renewal window, so one occurrence row drives every credential with no per-secret timer. The rotation commit rides kernel `Cell.Commit`, so a lost CAS reports `Contended` instead of publishing as success, and the renewal verdict rides the SWAPPED value: a refused re-pull commits the prior lease carrying its `Refusal`, which `Observability/health#HEALTH_REGISTRY` reads on its `ContributorTag.Store`-tagged row and `DegradationPolicy.Derive` maps to `DegradationLevel.ReadOnly` — a level nothing produced while the refusal lived only in a discarded fan. Zeroization registers as one `Runtime/lifecycle#DRAIN_CONDUCTOR` `DrainBand.Stores` participant row under the drain-forced token, so a hung renewal never strands a live secret; the credential bytes carry `DataClassification.Secret` so `Observability/telemetry#REDACTION_TAXONOMY` erases them at every egress.
- Receipt: `SecretReceipt` carries the transition key, lease window, kernel content digest, redacted credential id, and an optional canonical ProtoJSON fault element — never secret bytes or a raw key id; every transition fans through `ReceiptSinkPort.Send` under `ReceiptKind.Secret`, partitioned by tenant.
- Packages: Rasm.Contracts, Google.Protobuf, Rasm (kernel `CanonicalWriter`/`ContentHash`, `Cell`/`Transition`), Microsoft.Extensions.Configuration.UserSecrets, Microsoft.Extensions.Compliance.Redaction, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: one lifecycle transition is one `LeaseTransition` case; one refusal is one `SecretFault` case; a new credential class is one `DeadlineClass` lifetime row with its skew escalation, admitted through the same `RotationBand.Of`; a new credential source is one `SecretsSource` provider value on the existing `ConfigLayer`, never a second lease owner; zero new surface.
- Boundary: the lease is the only credential-lifecycle owner: failed re-pulls keep the current lease live and degrade through health; rented material zeroizes through `CryptographicOperations.ZeroMemory`; content identity uses kernel `ContentHash`. Every fault detail and every receipt column uses the redacted id — the redaction seam covers the log and receipt path alone and never the wire, where `CREDENTIAL_PEM` crosses the key id intact because a verifier SELECTS on it — and an optional refusal passes through `FaultWire.Observe` with `WireJson.Element` once, so the STJ receipt holds canonical ProtoJSON without reflecting a generated message; a reader re-enters through `WireJson.Read`. Mutable material and rotation stay the lease's, while `CredentialPublic` owns public-material admission. That frozen secrets-store mount remains the sole provider read. `LeaseTransition.Released` stays deleted because release without wipe is unlawful. KMS unwrap and PDF signing consume lease-scoped handles without a second long-lived key cache or credential lifecycle.

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
    Option<JsonElement> Refusal,
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
                        Refusal: lease.Refusal.Map(error => WireJson.Element(FaultWire.Observe(error))),
                        At: runtime.Clocks.Now),
                    SuiteContracts.Host))
            .Map(_ => lease);
}
```

## [03]-[CREDENTIAL_PEM]

- Owner: `Der` is the owned-storage DER element; `DerChain` is the ordered leaf-first roster; `CredentialMaterial` `[Union]` closes the generated `material` oneof onto two public arms; generated `Credential.V1.CredentialPublicWire` is the cross-language carrier; `PemFault` `[Union]` bands through `FaultBand.Pem`; `CredentialPublic` owns admission and projection.
- Cases: `CredentialMaterial` = Chain | Spki, one arm per generated oneof member and no third; `PemFault` = CertRejected | ChainEmpty | SpkiRejected.
- Entry: `Chain(Seq<ReadOnlyMemory<byte>> certificates)` returns `Fin<CredentialMaterial>` — every element parses through `X509CertificateLoader.LoadCertificate` and copies out inside that proving scope; `Spki(ReadOnlyMemory<byte> key)` returns `Fin<CredentialMaterial>` — the body parses through `PublicKey.CreateFromSubjectPublicKeyInfo` and refuses a trailing tail; `Carrier(CredentialMaterial material, string keyId)` returns generated `Credential.V1.CredentialPublicWire` with the intact key id and exactly one oneof arm.
- Auto: raw DER is the canonical crossing and RFC-7468 armor is DELETED — armor is a text framing whose label is a self-declared string, so the private-key arm it forced a runtime filter to police has no case to inhabit once the oneof carries only public arms; the same ASN.1 parse each consumer's own import runs is the admission gate here, so a PKCS#8 body handed to the public arm refuses at the producer rather than at a verifier that has lost the cause; `Der` copies its octets because `X509Certificate2.RawDataMemory` is a view the certificate's handle bounds, and `ImmutableArray<byte>` storage discharges `UnsafeWrap`'s outlive obligation so the chain crosses with no second copy.
- Receipt: rotation rides `SECRET_LEASE`'s `SecretReceipt`, so the material axis adds no parallel receipt; generated `CredentialPublicWire` is the producer capability surface.
- Packages: Rasm.Contracts, Google.Protobuf, System.Security.Cryptography, Generator.Equals, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: one material encoding is one `CredentialMaterial` arm landing beside its generated oneof member; one refusal is one `PemFault` case; a new credential class rides the existing two arms already; zero new surface.
- Boundary: the material axis is the suite's only credential-material wire owner — the lease holds the live `byte[]` and zeroizes it while `CredentialPublic` owns public-material admission, so the two never merge; `X509CertificateLoader` owns certificate admission and `PublicKey.CreateFromSubjectPublicKeyInfo` bare-key admission, and hand-rolled ASN.1, a base64 wrap, and a third-party codec are the deleted forms. Key ids cross INTACT because a verifier SELECTS on them, matching a JWS `kid`, while `SecretRuntime.Redacted` serves the log and receipt seam alone and its output selects nothing. Private-key arms are structurally absent rather than filtered, so `CarriesSecret`, the public-half filter, and the `DataClassification.Secret` block stamp are deleted with the vocabulary that needed them; what the collapse loses is the self-describing label, and what replaces it is the SPKI and X.509 format parse, which refuses a private body by encoding rather than by trusting its own declaration.
- Exemption: `PemFault` ordinals 0 and 1 stay reserved rather than restrided — a fault ordinal is the numeric `case` a peer may already hold, so the surviving leaves keep their issued numbers and the retired armor leaves leave holes.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Generator.Equals;
using Google.Protobuf;
using LanguageExt;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;
using Host = Rasm.Contracts.Credential.V1;

namespace Rasm.AppHost.Runtime;

// --- [TYPES] --------------------------------------------------------------------------------
// Generated `material` oneof, closed onto its owner. Two public arms and no third leave the private-key block
// that armor policed with a runtime label filter no case to inhabit, so that filter deletes rather than
// re-spelling: an unrepresentable state needs no guard at each use.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CredentialMaterial {
    private CredentialMaterial() { }

    public sealed record Chain(DerChain Certificates) : CredentialMaterial;
    public sealed record Spki(Der Key) : CredentialMaterial;
}

// --- [ERRORS] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PemFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Pem;
    private PemFault(string detail) => Detail = detail;
    public string Detail { get; }
    public sealed override string Message => Detail;

    // Ordinals 0 and 1 are RESERVED, never restrided: `LabelUnknown` and `ArmorMalformed` retired with the armor
    // axis, and the ordinal is the numeric `case` a peer holds, so reusing a released number re-types history.
    [FaultCase(2)]
    public sealed partial record CertRejected(Error Cause)
        : PemFault($"certificate: {Cause.Message}"), ICausedFault;
    [FaultCase(3)]
    public sealed partial record ChainEmpty : PemFault {
        public ChainEmpty() : base("certificate chain carries no element") { }
    }
    [FaultCase(4)]
    public sealed partial record SpkiRejected(Error Cause)
        : PemFault($"spki: {Cause.Message}"), ICausedFault;
}

// --- [MODELS] -------------------------------------------------------------------------------
// DER octets OWN their storage. `X509Certificate2.RawDataMemory` is a view the certificate's own handle bounds and
// reading it after `Dispose` throws `CryptographicException: m_safeCertContext is an invalid handle`, so an element
// copies out inside the proving scope. `ImmutableArray<byte>` is the carrier that makes both halves true at once:
// a `ReadOnlyMemory<byte>` member compares its handle under the synthesized record form, while this one reaches the
// generated collection policy, and its immortality discharges `UnsafeWrap`'s obligation that the wrapped memory
// outlive the message — so the wire projection below spends no second copy.
[Equatable]
public readonly partial record struct Der([property: OrderedEquality] ImmutableArray<byte> Octets) {
    public static Der Of(ReadOnlySpan<byte> octets) => new([.. octets]);
    public ByteString Wire => UnsafeByteOperations.UnsafeWrap(Octets.AsMemory());
}

// Order IS identity: the roster is leaf-first and the same certificates in another order are another chain, so the
// ordered comparer is the one that agrees with the wire the projection writes.
[Equatable]
public sealed partial record DerChain([property: OrderedEquality] Seq<Der> Elements);

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class CredentialPublic {
    // Each element PARSES as a certificate or none enter, and the copy lands INSIDE the proving scope. An opaque
    // blob admitted unproved failed later at the consumer's own import where the cause was gone, and a view taken
    // outside the `using` reads a released handle rather than the octets it was proved over.
    public static Fin<CredentialMaterial> Chain(Seq<ReadOnlyMemory<byte>> certificates) =>
        certificates.IsEmpty
            ? Fin.Fail<CredentialMaterial>(new PemFault.ChainEmpty())
            : Op.Of()
                .Catch(() => Fin.Succ(certificates.Map(static octets => {
                    using X509Certificate2 proved = X509CertificateLoader.LoadCertificate(octets.Span);
                    return Der.Of(proved.RawDataMemory.Span);
                }).Strict()))
                .Map(static proved => (CredentialMaterial)new CredentialMaterial.Chain(new DerChain(proved)))
                .MapFail(static error => (Error)new PemFault.CertRejected(error));

    // Bare public keys admit through the SAME ASN.1 gate a consumer's own `importKey("spki", ...)` runs, so a
    // PKCS#8 private body handed to this arm refuses here rather than crossing and failing at the verifier.
    public static Fin<CredentialMaterial> Spki(ReadOnlyMemory<byte> key) =>
        Op.Of()
            .Catch(() => Fin.Succ(Admit(key.Span)))
            .Map(static admitted => (CredentialMaterial)new CredentialMaterial.Spki(admitted))
            .MapFail(static error => (Error)new PemFault.SpkiRejected(error));

    // `CreateFromSubjectPublicKeyInfo` reports the octets it consumed, so a body carrying a valid SPKI prefix and a
    // tail raises inside the one `Op.Catch` seam that lowers the BCL throw rather than needing a second rail shape.
    static Der Admit(ReadOnlySpan<byte> key) {
        _ = PublicKey.CreateFromSubjectPublicKeyInfo(key, out int read);
        return read == key.Length
            ? Der.Of(key)
            : throw new CryptographicException("trailing octets after subject public key info");
    }

    // `Carrier` writes the WHOLE message. `KeyId` is the selection key a verifier matches a JWS `kid` against, so
    // it crosses intact — `SecretRuntime.Redacted` serves the log and receipt path, and a redacted id selects no
    // key — while the union's own total `Map` writes exactly one oneof arm, leaving no declared slot unanswered.
    public static Host.CredentialPublicWire Carrier(CredentialMaterial material, string keyId) =>
        material.Map(
            chain: arm => new Host.CredentialPublicWire {
                KeyId = keyId,
                CertificateChain = new Host.CertificateChain {
                    Certificates = { arm.Certificates.Elements.Map(static der => der.Wire) },
                },
            },
            spki: arm => new Host.CredentialPublicWire { KeyId = keyId, SpkiDer = arm.Key.Wire });
}
```

## [04]-[TS_PROJECTION]

- Owner: generated `CredentialPublicWire` — the public credential carrier on the protobuf `apphost-wire` seam, with `CertificateChain` its nested chain member.
- Law: this page MINTS the carrier and `libs/typescript/core/.planning/interchange/codec.md` `Credential` BINDS the decode, which `libs/typescript/security/.planning/crypt/sign.md` `[03]-[KEY_MATERIAL]` folds into a `KeyHandle` — `Material.admit` dispatches the generated `material` oneof case rather than scraping a label out of text. This wire declares no private arm, so `importPKCS8` is unreachable from it by shape, and its one consumer decodes the one C#-minted vocabulary and re-mints none.
- Entry: the carrier crosses as `CredentialPublicWire` under ProtoJSON; `key_id` crosses intact as the verifier's selection key, and `material` carries either the leaf-first `certificate_chain` DER roster or one bare `spki_der` body.
- Packages: generated `@rasm/ts-contracts` credential-v1 module
- Growth: one carrier field extends every generated peer; one new material encoding is one oneof member landing beside its consumer arm; no hand-maintained carrier surface.
- Boundary: only public material and the key id cross. `CertificateChain` is a nested support message the parent descriptor reaches, so it registers no second interchange family and no consumer imports it as a root.

```ts signature
export {
  CertificateChainSchema,
  CredentialPublicWireSchema,
} from "@rasm\/contracts/rasm/contracts/credential/v1/public_pb";
export type { CertificateChain, CredentialPublicWire } from "@rasm\/contracts/rasm/contracts/credential/v1/public_pb";
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

- [SPKI_ADMISSION]-[OPEN]: does `libs/csharp/.api/api-bcl-cryptography.md` register `PublicKey`, its `CreateFromSubjectPublicKeyInfo(ReadOnlySpan<byte>, out int)` factory, and the `X509Certificate2.PublicKey` property `[03]-[CREDENTIAL_PEM]` admits bare keys through; resolve each against the pinned net10.0 assembly and land the rows at that catalog, whose `[STACKING]` entry still names this page's retired armor axis.
