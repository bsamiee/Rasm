# [APPHOST_SECRETS_AND_CREDENTIAL_MATERIAL]

`SecretLease` owns the credential-material lifecycle: one row family acquires, rotates, and zeroizes credential material against the RID-dispatched credential-store provider the host resolves through `Runtime/config#SOURCE_AXIS`'s `ConfigSource.SecretsStore` row, and it carries the per-store-open KMS-unwrap handle `Rasm.Persistence/Element/identity#KMS_CUSTODY` reads as one `SecretLease`-class content carrier so the cloud-KMS key-handle lifecycle stays the runtime lease's concern rather than a long-lived Persistence-side key. `CredentialPublic` is the suite's only credential-material wire vocabulary: the host admits every public credential as raw DER under one closed two-arm material family, projects the generated `Credential.CredentialPublicWire` the TypeScript verifier decodes, and crosses no armored text, bare `byte[]`, or parallel base64 envelope. Owned axes are the secret-lease lifecycle, public credential-material admission, and KMS-unwrap custody over System.Security.Cryptography, Microsoft.Extensions.Compliance.Redaction, generated protobuf contracts, the kernel identity and transition capsules, Generator.Equals, NodaTime, Thinktecture.Runtime.Extensions, and LanguageExt.Core.

## [01]-[INDEX]

- [02]-[SECRET_LEASE]: Acquire-rotate-zeroize credential lifecycle extending the `SecretsStore` source row.
- [03]-[CREDENTIAL_PEM]: Public DER material admission and the one generated carrier seam.
- [04]-[TS_PROJECTION]: Public credential wire shape and its `jose` decode.

## [02]-[SECRET_LEASE]

- Owner: `SecretLease` the live credential row extending `ConfigSource.SecretsStore`; `RotationBand` the admitted cadence pair; `SecretRuntime` the boundary capsule; `SecretFault` `[Union]` fault family riding the kernel `[FaultCase]`/`Fault` floor (`[FaultCase]` realizes the registry over `FaultBand.Secret`); `SecretLeaseOps` the acquire-rotate-zeroize fold.
- Cases: `SecretFault` = AcquireRejected | RenewMissed | RotationUnbanded.
- Entry: `Acquire(SecretRuntime runtime, string keyId)` returns `IO<Fin<Atom<SecretLease>>>` — the store read seats the LIVE lease cell and registers that cell's renewal occurrence on `SecretRuntime.Schedule` in one bind, so a returned cell is always a rotating cell; `Rotate(SecretRuntime runtime, Atom<SecretLease> cell)` returns `IO<Unit>` — the renewal occurrence's `Work` binding, committing the re-pull through kernel `Cell.Commit`; `Renew(SecretRuntime runtime, SecretLease lease)` returns `IO<Fin<SecretLease>>`, re-pulling inside the live window with no disposal of its own — the retiring buffer wipes at `Rotate`'s commit site once the replacement is seated; `Zeroize(SecretRuntime runtime, SecretLease lease)` returns `IO<Unit>`, the drain-forced terminal.
- Auto: `RotationBand.Of` reads the credential's lifetime row AND that row's own `Escalation` skew off the `Runtime/time#DEADLINE_TAXONOMY` roster, so the renewal period is `Life - Skew` and derives — a cadence equal to the lifetime fires the re-pull exactly at expiry, which is the shape under which every occurrence read `RenewMissed` and the lease silently died under prose promising rotation ahead of expiry; a lifetime row declaring no skew refuses at composition as `RotationUnbanded` rather than seating a rotation that cannot succeed. `Acquire` registers one `ScheduleEntry` on the bound `Runtime/time#SCHEDULE_PORT` delegate carrying the entry's own `RedrivePolicy` and a `LeasePolicy` whose `CrashStaleness` outlives the renewal window, so one occurrence row drives every credential with no per-secret timer. Rotation commits ride kernel `Cell.Commit`, so a lost CAS reports `Contended` instead of publishing as success, and the renewal verdict rides the SWAPPED value: a refused re-pull commits the prior lease carrying its `Refusal`, which `Observability/health#HEALTH_FOLD` reads on its `ContributorTag.Store`-tagged row and `DegradationPolicy.Derive` maps to `DegradationLevel.ReadOnly` — a level nothing produced while the refusal lived only in a discarded fan. Zeroization registers as one `Runtime/lifecycle#DRAIN_CONDUCTOR` `DrainBand.Stores` participant row under the drain-forced token, so a hung renewal never strands a live secret; the credential bytes carry `DataClassification.Secret` so `Observability/telemetry#REDACTION_TAXONOMY` erases them at every egress.
- Growth: one refusal is one `SecretFault` case; a new credential class is one `DeadlineClass` lifetime row with its skew escalation, admitted through the same `RotationBand.Of`; a new credential source is one `SecretsSource` provider value on the existing `ConfigLayer`, never a second lease owner; zero new surface.
- Boundary: the lease is the only credential-lifecycle owner: failed re-pulls keep the current lease live and degrade through health; rented material zeroizes through `CryptographicOperations.ZeroMemory` only after `Cell.Commit` seats its replacement or the drain terminal retires it; content identity uses kernel `ContentHash`. Every fault detail uses the redacted id while `CREDENTIAL_PEM` crosses the key id intact because a verifier SELECTS on it. Mutable material and rotation stay the lease's, while `CredentialPublic` owns public-material admission. That frozen secrets-store mount remains the sole provider read. Release without wipe is unlawful. KMS unwrap and PDF signing consume lease-scoped handles without a second long-lived key cache or credential lifecycle.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Security.Cryptography;
using LanguageExt;
using Microsoft.Extensions.Compliance.Redaction;
using NodaTime;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.AppHost.Runtime;

// --- [ERRORS] --------------------------------------------------------------------------
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
    [FaultCase(2)]
    public sealed partial record RotationUnbanded : SecretFault {
        public RotationUnbanded(string row) : base($"{row}: no escalation skew") => Row = row;
        public string Row { get; }
    }
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct RotationBand(DeadlineClass Life, DeadlineClass Skew) {
    public static Fin<RotationBand> Of(DeadlineClass life) =>
        life.Escalation
            .Filter(skew => skew.Allotted < life.Allotted)
            .Map(skew => new RotationBand(Life: life, Skew: skew))
            .ToFin(new SecretFault.RotationUnbanded(life.Key));

    public Duration Period => Life.Allotted - Skew.Allotted;
    public Interval Window(Instant from) => new(start: from, end: from + Life.Allotted);
}

public sealed record SecretRuntime(
    Func<string, Fin<byte[]>> Read,
    Func<ScheduleEntry, IO<Unit>> Schedule,
    Redactor Redactor,
    LeasePolicy Lease,
    RotationBand Rotation,
    RedrivePolicy Redrive,
    ClockPolicy Clocks) {
    public string Redacted(string keyId) {
        Span<char> sink = stackalloc char[Redactor.GetRedactedLength(keyId)];
        int written = Redactor.Redact(keyId, sink);
        return new string(sink[..written]);
    }
}

[Equatable]
public sealed partial record SecretLease(
    string KeyId,
    byte[] Material,
    Interval Window,
    Option<Error> Refusal,
    ScheduleEntry Renewal) {
    public string Digest => ContentHash.Hex(ContentHash.Of(Material));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SecretLeaseOps {
    public static IO<Fin<Atom<SecretLease>>> Acquire(SecretRuntime runtime, string keyId) =>
        runtime.Read(keyId)
            .MapFail(error => (Error)new SecretFault.AcquireRejected(runtime.Redacted(keyId), error))
            .Match(
                Succ: material => Seat(runtime, keyId, material)
                    .Bind(cell => runtime.Schedule(cell.Value.Renewal).Map(_ => Fin.Succ(cell))),
                Fail: error => IO.pure(Fin.Fail<Atom<SecretLease>>(error)));

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
        cell = Atom(new SecretLease(keyId, material, window, None, renewal));
        return IO.pure(cell!);
    }

    public static IO<Unit> Rotate(SecretRuntime runtime, Atom<SecretLease> cell) =>
        IO.lift(() => cell.Value).Bind(prior => Renew(runtime, prior).Bind(outcome =>
            Cell.Commit(cell, held => outcome.Match(
                    Succ: static renewed => renewed,
                    Fail: error => held with { Refusal = Some(error) }))
                .Switch(
                    committed: row => outcome.Match(
                            Succ: _ => IO.lift(() => { CryptographicOperations.ZeroMemory(prior.Material); return unit; }),
                            Fail: static _ => IO.pure(unit))
                        .Map(_ => row.State),
                    ceded: static row => IO.pure(row.State),
                    refused: static row => IO.pure(row.State),
                    contended: row => IO.pure(row.State with { Refusal = Some((Error)new SecretFault.RenewMissed(
                        runtime.Redacted(row.State.KeyId), $"rotation lost {row.Attempts.Value} commit rounds")) }))
                .Map(static _ => unit)));

    public static IO<Fin<SecretLease>> Renew(SecretRuntime runtime, SecretLease lease) =>
        runtime.Clocks.Now is var now && now >= lease.Window.End
            ? IO.pure(Fin.Fail<SecretLease>(new SecretFault.RenewMissed(runtime.Redacted(lease.KeyId), "window closed before renewal")))
            : IO.pure(runtime.Read(lease.KeyId)
                .Map(material => lease with { Material = material, Window = runtime.Rotation.Window(now), Refusal = None }));

    public static IO<Unit> Zeroize(SecretRuntime runtime, SecretLease lease) =>
        IO.lift(() => { CryptographicOperations.ZeroMemory(lease.Material); return unit; });
}
```

## [03]-[CREDENTIAL_PEM]

- Owner: `Der` is the owned-storage DER element; `DerChain` is the ordered leaf-first roster; `CredentialMaterial` `[Union]` closes the generated `material` oneof onto two public arms; generated `Credential.CredentialPublicWire` is the cross-language carrier; `PemFault` `[Union]` bands through `FaultBand.Pem`; `CredentialPublic` owns admission and projection.
- Cases: `CredentialMaterial` = Chain | Spki, one arm per generated oneof member and no third; `PemFault` = CertRejected | ChainEmpty | SpkiRejected.
- Entry: `Chain(Seq<ReadOnlyMemory<byte>> certificates)` returns `Fin<CredentialMaterial>` — every element parses through `X509CertificateLoader.LoadCertificate` and copies out inside that proving scope; `Spki(ReadOnlyMemory<byte> key)` returns `Fin<CredentialMaterial>` — the body parses through `PublicKey.CreateFromSubjectPublicKeyInfo` and refuses a trailing tail; `Carrier(CredentialMaterial material, string keyId)` returns generated `Credential.CredentialPublicWire` with the intact key id and exactly one oneof arm.
- Auto: raw DER is the canonical crossing and RFC-7468 armor is DELETED — armor is a text framing whose label is a self-declared string, so the private-key arm it forced a runtime filter to police has no case to inhabit once the oneof carries only public arms; the same ASN.1 parse each consumer's own import runs is the admission gate here, so a PKCS#8 body handed to the public arm refuses at the producer rather than at a verifier that has lost the cause; `Der` copies its octets because `X509Certificate2.RawDataMemory` is a view the certificate's handle bounds, and `ImmutableArray<byte>` storage discharges `UnsafeWrap`'s outlive obligation so the chain crosses with no second copy.
- Auto: generated `CredentialPublicWire` is the producer capability surface.
- Growth: one material encoding is one `CredentialMaterial` arm landing beside its generated oneof member; one refusal is one `PemFault` case; a new credential class rides the existing two arms already; zero new surface.
- Boundary: the material axis is the suite's only credential-material wire owner — the lease holds the live `byte[]` and zeroizes it while `CredentialPublic` owns public-material admission, so the two never merge; `X509CertificateLoader` owns certificate admission and `PublicKey.CreateFromSubjectPublicKeyInfo` bare-key admission, and hand-rolled ASN.1, a base64 wrap, and a third-party codec are the deleted forms. Key ids cross INTACT because a verifier SELECTS on them, matching a JWS `kid`, while `SecretRuntime.Redacted` serves logging alone and its output selects nothing. Private-key arms are structurally absent rather than filtered, so `CarriesSecret`, the public-half filter, and the `DataClassification.Secret` block stamp are deleted with the vocabulary that needed them; what the collapse loses is the self-describing label, and what replaces it is the SPKI and X.509 format parse, which refuses a private body by encoding rather than by trusting its own declaration.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Generator.Equals;
using Google.Protobuf;
using LanguageExt;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;
// Contracts are retired from this logic.

namespace Rasm.AppHost.Runtime;

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CredentialMaterial {
    private CredentialMaterial() { }

    public sealed record Chain(DerChain Certificates) : CredentialMaterial;
    public sealed record Spki(Der Key) : CredentialMaterial;
}

// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PemFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Pem;
    private PemFault(string detail) => Detail = detail;
    public string Detail { get; }
    public sealed override string Message => Detail;

    [FaultCase(0)]
    public sealed partial record CertRejected(Error Cause)
        : PemFault($"certificate: {Cause.Message}"), ICausedFault;
    [FaultCase(1)]
    public sealed partial record ChainEmpty : PemFault {
        public ChainEmpty() : base("certificate chain carries no element") { }
    }
    [FaultCase(2)]
    public sealed partial record SpkiRejected(Error Cause)
        : PemFault($"spki: {Cause.Message}"), ICausedFault;
}

// --- [MODELS] --------------------------------------------------------------------------
[Equatable]
public readonly partial record struct Der([property: OrderedEquality] ImmutableArray<byte> Octets) {
    public static Der Of(ReadOnlySpan<byte> octets) => new([.. octets]);
    public ByteString Wire => UnsafeByteOperations.UnsafeWrap(Octets.AsMemory());
}

[Equatable]
public sealed partial record DerChain([property: OrderedEquality] Seq<Der> Elements);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CredentialPublic {
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

    public static Fin<CredentialMaterial> Spki(ReadOnlyMemory<byte> key) =>
        Op.Of()
            .Catch(() => Fin.Succ(Admit(key.Span)))
            .Map(static admitted => (CredentialMaterial)new CredentialMaterial.Spki(admitted))
            .MapFail(static error => (Error)new PemFault.SpkiRejected(error));

    static Der Admit(ReadOnlySpan<byte> key) {
        _ = PublicKey.CreateFromSubjectPublicKeyInfo(key, out int read);
        return read == key.Length
            ? Der.Of(key)
            : throw new CryptographicException("trailing octets after subject public key info");
    }

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
- Growth: one carrier field extends every generated peer; one new material encoding is one oneof member landing beside its consumer arm; no hand-maintained carrier surface.
- Boundary: only public material and the key id cross. `CertificateChain` is a nested support message the parent descriptor reaches, so it registers no second interchange family and no consumer imports it as a root.

```ts
export {
  CertificateChainSchema,
  CredentialPublicWireSchema,
} from "@rasm\/contracts/rasm/contracts/credential/public_pb";
export type { CertificateChain, CredentialPublicWire } from "@rasm\/contracts/rasm/contracts/credential/public_pb";
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
