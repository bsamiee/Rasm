# [APPHOST_SUPPLY_CHAIN_ADMISSION]

Suite-wide supply-chain admission owner: one `SupplyChainGate.Admit` proves every downloaded artifact — a Velopack release asset and a plugin/companion component alike — through one offline Sigstore signature and SLSA in-toto provenance verify, and one `NuGet.Versioning` `VersionRange.Satisfies` contract check before a byte stages or loads. Subject discrimination rides the `AdmissionSubject` union (release | plugin), never a second verify path; `TrustPolicy` rows carry the per-subject expected signer and version contract; the ONE `SupplyChainFault` union rides the kernel `[FaultCase]`/`Fault` floor — `[FaultCase]` realizes the registry over `FaultBand.SupplyChain` and `Code` derive SEALED.

`Sandbox/provisioning#UPDATE_MACHINE` `Stage` and `Sandbox/isolation#ISOLATION_AXIS` `SandboxRows.Load` are the two fences that compose the gate — the release precondition and the plugin-artifact admission. `Sandbox/solver#SOLVER_HOSTING` reaches it THROUGH that load rather than directly, so hosted solvers add a caller and not a third crossing. Settled composition: `ContentHash.Of`/`.Hex`/`.Admit`, `IValidityEvidence`/`ValidityClaim`, `Fault`/`FaultBand`/`Op` from Rasm/Domain; `UpdateChannel` from Sandbox/provisioning#CHANNEL_AXIS; `ClockPolicy` from Runtime/time. Duplicate gates, hand-rolled signature delegates, and a `System.Version` range split beside this owner are the deleted forms.

## [01]-[INDEX]

- [02]-[ADMISSION_SUBJECTS]: One subject union — release asset and plugin artifact on one admit shape, both constructible only from real material.
- [03]-[SUPPLY_CHAIN_GATE]: Offline Sigstore signature, SLSA provenance, and SemVer-contract admission over an admitted trust anchor.

## [02]-[ADMISSION_SUBJECTS]

- Owner: `AdmissionSubject` `[Union]` the closed subject vocabulary one `Admit` discriminates on; `PluginArtifact` `[ComplexValueObject]` the candidate plugin/companion record every load path presents whole.
- Cases: Release carries the Velopack `VelopackAsset` with its `UpdateChannel`; Plugin carries the `PluginArtifact` — component bytes, crypto digest, the cosign bundle beside the artifact, and the declared host-contract range.
- Entry: `PluginArtifact.Admit(pluginId, component, bundle, contractRange)` is the fallible construction path, lowering generated factory evidence once through the kernel acceptance bridge.
- Law: sole construction holds STRUCTURALLY, where a plain `record` published its positional constructor beside the factory and left the prose invariant resting on caller discipline while the gate carried a dead re-guard nothing fires; `[ComplexValueObject]` with `ValidateFactoryArguments` makes the hollow state unconstructible and the re-guard deletes. NAMED LOSS: `with`-expression construction, which no call site used.
- Law: two digests, two jobs. `Sha256` is the cryptographic digest the Sigstore verify proves — a security claim demands a cryptographic hash — while `ContentKey` is the non-cryptographic kernel `ContentHash` identity the evidence stream, the quarantine record, and the admission cache all key on. Neither substitutes, and the gate proves it by resolving the identity per subject arm off that arm's own material rather than reusing whichever digest the verify already held.
- Law: `ContentKey` renders through kernel `ContentHash.Hex`, so the thirty-two-lowercase-character format has ONE author and a reader re-admits the same text through `ContentHash.Admit`, which REFUSES uppercase. Two sites spelling `ToString("x32")` is one convention with two authors.
- Law: artifact equality is IDENTITY-shaped and states so — `ReadOnlyMemory<byte>` compares by pointer, offset, and length, so two artifacts carrying byte-identical components from different buffers read unequal. Every cache, dedup, and quarantine lookup keys on `ContentKey`, which is the value identity, and never on the artifact itself.
- Packages: Rasm (kernel `ContentHash`), Velopack, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new admissible artifact kind is one `AdmissionSubject` case with its digest, bundle, and version projection arms on the gate's total dispatch; zero new surface.
- Boundary: the artifact's `Der`-level parse never runs during admission — the gate reads bytes, bundle, and range only, so a malicious artifact cannot exploit the gate by executing during verify; the component bytes ride as `ReadOnlyMemory<byte>` and are read twice at most (once for the crypto digest, once for the content key), never staged to a second buffer.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AdmissionSubject {
    private AdmissionSubject() { }
    public sealed record Release(VelopackAsset Asset, UpdateChannel Channel) : AdmissionSubject;
    public sealed record Plugin(PluginArtifact Artifact) : AdmissionSubject;
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError]
public sealed partial class PluginArtifact {
    public string PluginId { get; }
    public ReadOnlyMemory<byte> Component { get; }
    public Option<FileInfo> Bundle { get; }
    public string ContractRange { get; }

    public string Sha256 => Convert.ToHexStringLower(SHA256.HashData(Component.Span));

    public string ContentKey => ContentHash.Hex(ContentHash.Of(Component.Span));

    public static Fin<PluginArtifact> Admit(
        string pluginId, ReadOnlyMemory<byte> component, Option<FileInfo> bundle, string contractRange) =>
        Op.Of().AcceptValidated<PluginArtifact>(
            fault: Validate(pluginId, component, bundle, contractRange, out PluginArtifact? admitted),
            admitted: admitted);

    static partial void ValidateFactoryArguments(
        ref ValidationError? error, ref string pluginId, ref ReadOnlyMemory<byte> component,
        ref Option<FileInfo> bundle, ref string contractRange) =>
        error = component.IsEmpty || string.IsNullOrWhiteSpace(pluginId)
            ? new ValidationError(string.Join(" | ", new object?[] { pluginId }))
            : null;
}
```

## [03]-[SUPPLY_CHAIN_GATE]

- Owner: `SupplyChainFault` `[Union]` the fault family riding the kernel `[FaultCase]`/`Fault` floor (`[FaultCase]` realizes the registry over `FaultBand.SupplyChain`); `SupplyChainAdmission` the admitted subject implementing the kernel `IValidityEvidence` fold over its two clocks; `TrustPolicy` the per-subject expected-signer and version-contract policy; `TrustAnchor` `[Union]` naming the two anchors a composition may seat; `SupplyChainGate` the static admit surface, with the nested `Runtime` binding the `SigstoreVerifier` that anchor selects, the policy resolver, the Velopack staging directory the update machine downloads into, the host contract version, and the one `ClockPolicy`.
- Cases: `SupplyChainFault` = BundleMissing | BundleUnreadable | SignatureRejected | ProvenanceUnbound | VersionIncompatible | TrustRootUnavailable | AttestationMissing — one case per admit-rejection cause; `TrustAnchor` = `PinnedCase(FileInfo)` | `TufCase(Uri, TufTrustRootProviderOptions)` — the offline pin and the TUF repository, one provider each.
- Entry: `Runtime.Of(TrustAnchor anchor, Func<AdmissionSubject, TrustPolicy> policyOf, DirectoryInfo staging, string hostContractVersion, ClockPolicy clocks)` returns `Fin<Runtime>` — the composition-time admit of the trust anchor itself, switching the anchor case onto `FileTrustRootProvider` or `TufTrustRootProvider` and binding the one verifier over it; `Admit(Runtime gate, AdmissionSubject subject, CancellationToken token)` returns `IO<Validation<Error, SupplyChainAdmission>>` — one total dispatch projecting the subject's digest bytes, cosign bundle, version pair, and `TrustPolicy` row, reading the admitted material once, verifying signature and SLSA provenance offline against the pinned root through `SigstoreVerifier.TryVerifyDigestAsync`, and deciding the version contract with `VersionRange.Satisfies`; `Best(TrustPolicy policy, Option<NuGetVersion> installed, Seq<NuGetVersion> candidates)` returns `Validation<Error, NuGetVersion>` — the candidate-ranking entry the sandbox module fold binds inside `SolverHostRuntime.Resolve` at `Runtime/modules#MODULE_LEDGER`, which ranks a manifest's in-range candidates through it before the resolved artifact is presented as `AdmissionSubject.Plugin`.
- Law: the signature leg and the version leg accumulate applicatively, so a subject both forged AND out-of-contract reports both faults in one pass; this is the folder's model applicative and the shape `Sandbox/solver#PLUGIN_CONTRACT` copies.
- Law: `TrustRootUnavailable` has a producing arm at the COMPOSITION boundary, where the fact lives — binding the verifier is what reads the pinned root, so an absent or unreadable anchor refuses at construction rather than at the first admit, and a case declared for a cause no arm raises is the deleted form. NAMED LOSS: the fault no longer reaches the per-admit `Validation` — a host whose anchor is gone never composes a gate to admit through. `TufCase` seats its repository at that same instant and resolves the root itself on first verify, because a fetch inside a composition fold is a network round trip at boot; its hermetic form carries `TufTrustRootProviderOptions.CustomTrustedRoot`, which resolves without leaving the node.
- Law: `Admit` runs BEFORE any stage or load commits — `UpdateMachine.Stage` branches on the admit `Validation` minting `RolledBack` on a fault, and `SandboxRows.Load` never materializes an isolation vehicle for a rejected artifact.
- Law: `SigstoreVerifier.TryVerifyDigestAsync` is the non-throwing ROP mirror returning `(bool Success, VerificationResult? Result)`, and only the RESULT crosses into the domain: the tuple's bool is subsumed by the `SignerIdentity is { }` pattern the signature leg already tests, so an admitted foreign shape stops at the boundary and never becomes an internal parameter type.
- Law: a refused verify reports what the verify SAID or that it said nothing. `verified.Result?.FailureReason ?? subject` renamed the subject as its own rejection reason, so an operator read "rejected because <artifact name>" for a verifier that reported no reason at all.
- Law: the bundle LOAD refuses typed, where a malformed or truncated `*.sigstore.json` threw out of the effect unhandled while `BundleMissing` covered absence alone; the two demand different operator acts — re-publish a corrupted bundle, fetch a missing one — so `BundleUnreadable` carries the parse refusal.
- Law: the expected signer is the `VerificationPolicy.CertificateIdentity` built once through `CertificateIdentity.ForGitHubActions(owner, repository)`, so an empty-identity verify that asserts only cryptographic integrity is the rejected form; the DSSE/in-toto provenance leg reads `VerificationResult.Statement` and binds its `Subject` digest to the admitted artifact, so one verify proves signature AND build provenance.
- Law: the version leg parses through `NuGetVersion.TryParse` and decides with `VersionRange.Satisfies` — the real SemVer-2.0 contract check `System.Version` cannot express — over the range's PINNED projection, since a floating band and a pinned candidate otherwise compare on two grammars; a parse failure on either boundary fails closed as `VersionIncompatible`, and a boundary parse never crosses as a null-forgiven local.
- Law: the version pair INVERTS per subject — a release checks its version against the channel's admitted range, a plugin parses its declared `ContractRange` through the floating-aware overload and checks the host's version against it — which is why the contract rides the probe rather than the policy. One `Satisfies` law, projected per subject case.
- Law: the content key is `ContentHash.Of` over the material BOTH arms read — the plugin's component in hand, the release's staged package off the staging directory — so no arm substitutes its verify digest for the identity.
- Output: `SupplyChainAdmission` — subject key, verified signer SAN, in-toto predicate type, admitted version string, the kernel content key, the earliest attested `VerifiedTimestamp` instant, and the admitting `Instant`.
- Packages: Sigstore, NuGet.Versioning, Rasm (kernel `ContentHash`/`IValidityEvidence`/`ValidityClaim`/`Op`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one verify threshold is one `VerificationPolicy` column; one subject's expected signer is one `TrustPolicy` row; a new trust anchor — an embedded `InMemoryTrustRootProvider` root, say — is one `TrustAnchor` case and one arm on the `Of` switch; a managed-key (non-Fulcio) feed is the `VerificationPolicy.PublicKey` column; a new attestation predicate is one policy column, never an `Attestation` record variant; zero new surface.
- Boundary: the gate is the suite's only supply-chain admit owner — a `System.Version`-based semver check, a hand-split `lower-upper` range string, a hand-rolled `Verify` delegate over pinned publisher keys, a throwing `Parse` inside the admission fold, an unsigned-release install, a trust-on-first-use path, a post-load signature check, and a network-bound verify on an air-gapped node are deleted forms, and both the self-update release and a downloaded plugin artifact verify through this one `Admit`; `vpk`-side build-time notarization is distinct — the build signs and this gate proves what the host downloaded; the anchor is a `TrustAnchor` case the composition seats and `Runtime.Of` switches, never a prose alternative beside a hardcoded provider — `TufCase` is admitted only on a connected node and its root fetch rides the `Wire/outbound` `Polly.Core` pipeline, `PinnedCase` pins the offline root for a hermetic air-gapped gate, and the provider each case mints is owned for the composition's life, so the trust-root fetch is the only outbound leg, it happens once, and the verify itself is offline; the version leg admits only the version, range, and comparer surface — package-graph resolution and framework compatibility stay out of scope, and the contract is one `VersionRange.Satisfies` membership test; candidate ranking is `Best` and lives HERE rather than at each resolver, because a feed resolver and a registry resolver each picking their own newest are two policies for one contract — its consumer is the `SolverHostRuntime.Resolve` closure the sandbox module fold binds at `Runtime/modules#MODULE_LEDGER`, which ranks its in-range candidates through `Best` before presenting `AdmissionSubject.Plugin`, and the release arm's ranking is Velopack's own `CheckForUpdatesAsync`, an SDK-internal selection this gate cannot narrow and therefore names as the one declared divergence rather than claiming a policy it does not own; the admitting instant is `ClockPolicy.Now` and never an ambient `DateTimeOffset.UtcNow`.

```csharp
// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SupplyChainFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.SupplyChain;
    private SupplyChainFault(string detail) => Detail = detail;
    public string Detail { get; }
    public sealed override string Message => Detail;

    [FaultCase(0)]
    public sealed partial record BundleMissing : SupplyChainFault { public BundleMissing(string detail) : base(detail) { } }
    [FaultCase(1)]
    public sealed partial record SignatureRejected : SupplyChainFault { public SignatureRejected(string detail) : base(detail) { } }
    [FaultCase(2)]
    public sealed partial record ProvenanceUnbound : SupplyChainFault { public ProvenanceUnbound(string detail) : base(detail) { } }
    [FaultCase(3)]
    public sealed partial record VersionIncompatible : SupplyChainFault { public VersionIncompatible(string detail) : base(detail) { } }
    [FaultCase(4)]
    public sealed partial record TrustRootUnavailable : SupplyChainFault { public TrustRootUnavailable(string detail) : base(detail) { } }
    [FaultCase(5)]
    public sealed partial record AttestationMissing : SupplyChainFault { public AttestationMissing(string detail) : base(detail) { } }
    [FaultCase(6)]
    public sealed partial record BundleUnreadable(string Subject, Error Cause)
        : SupplyChainFault($"{Subject}: {Cause.Message}"), ICausedFault;
}

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TrustAnchor {
    private TrustAnchor() { }
    public sealed record PinnedCase(FileInfo Root) : TrustAnchor;
    public sealed record TufCase(Uri Repository, TufTrustRootProviderOptions Options) : TrustAnchor;
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct SupplyChainAdmission(
    string Subject, string Signer, string Provenance, string Version, string ContentKey, Instant At, Option<Instant> Attested = default) : IValidityEvidence {
    [JsonIgnore]
    public bool IsValid => ValidityClaim.All(
        !string.IsNullOrEmpty(Signer),
        !string.IsNullOrEmpty(Provenance),
        !string.IsNullOrEmpty(ContentKey),
        Attested.Match(Some: stamp => stamp <= At, None: static () => true)).Holds;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SupplyChainGate {
    public sealed record TrustPolicy(VerificationPolicy Verification, VersionRange ContractRange);

    public sealed record Runtime(
        SigstoreVerifier Verifier,
        Func<AdmissionSubject, TrustPolicy> PolicyOf,
        DirectoryInfo Staging,
        string HostContractVersion,
        ClockPolicy Clocks) {
        public static Fin<Runtime> Of(
            TrustAnchor anchor, Func<AdmissionSubject, TrustPolicy> policyOf,
            DirectoryInfo staging, string hostContractVersion, ClockPolicy clocks) =>
            Op.Of().Catch(() => anchor.Switch(
                    pinnedCase: static row => row.Root is { Exists: true }
                        ? Fin.Succ<ITrustRootProvider>(new FileTrustRootProvider(row.Root))
                        : Fin.Fail<ITrustRootProvider>(new SupplyChainFault.TrustRootUnavailable(row.Root.FullName)),
                    tufCase: static row => Fin.Succ<ITrustRootProvider>(new TufTrustRootProvider(row.Repository, row.Options)))
                .Map(provider => new Runtime(
                    new SigstoreVerifier(provider, null), policyOf, staging, hostContractVersion, clocks)))
                .Bind(static admitted => admitted);
    }

    public static IO<Validation<Error, SupplyChainAdmission>> Admit(Runtime gate, AdmissionSubject subject, CancellationToken token) =>
        Project(gate, subject).Match(
            Succ: probe =>
                from identity in ContentKey(gate, subject, token)
                from loaded in Bundle(probe, token)
                from verified in loaded.Match(
                    Succ: bundle => IO.liftAsync(async () => Optional((await gate.Verifier.TryVerifyDigestAsync(
                        probe.Digest, HashAlgorithmType.Sha256, bundle, probe.Policy.Verification, token)).Result)),
                    Fail: _ => IO.pure(Option<VerificationResult>.None))
                select (loaded.ToValidation(), Signature(verified, probe.Subject), Version(probe.Contract, probe.Candidate, probe.Subject))
                    .Apply((_bundle, signer, version) => new SupplyChainAdmission(
                        probe.Subject, signer.Signer.SubjectAlternativeName, signer.Provenance, version.ToNormalizedString(),
                        identity, gate.Clocks.Now, Attested(verified)))
                    .As(),
            Fail: fault => IO.pure<Validation<Error, SupplyChainAdmission>>(Fail(fault)));

    static IO<Fin<SigstoreBundle>> Bundle(Probe probe, CancellationToken token) =>
        IO.liftAsync(async () => (await Op.Of().Catch(
                async execution => Fin.Succ(await SigstoreBundle.LoadAsync(probe.Bundle, execution)), token))
            .MapFail(error => (Error)new SupplyChainFault.BundleUnreadable(probe.Subject, error)));

    static IO<string> ContentKey(Runtime gate, AdmissionSubject subject, CancellationToken token) => subject.Switch(
        state: gate,
        release: static (host, found) => IO.liftAsync(async () => ContentHash.Hex(ContentHash.Of(
            await File.ReadAllBytesAsync(Path.Combine(host.Staging.FullName, found.Asset.FileName), token)))),
        plugin: static (_, held) => IO.pure(held.Artifact.ContentKey));

    static Option<Instant> Attested(Option<VerificationResult> verified) =>
        verified.Map(static result => toSeq(result.VerifiedTimestamps).Map(static stamp => Instant.FromDateTimeOffset(stamp.Timestamp)))
            .IfNone(Seq<Instant>())
            .Fold(Option<Instant>.None, static (earliest, stamp) => earliest.Filter(held => held <= stamp).IfNone(stamp));

    sealed record Probe(string Subject, byte[] Digest, FileInfo Bundle, VersionRange Contract, string Candidate, TrustPolicy Policy);

    static Fin<Probe> Project(Runtime gate, AdmissionSubject subject) => subject.Switch(
        state: gate,
        release: (host, found) => Staged(host.Staging, found.Asset.FileName)
            .ToFin(new SupplyChainFault.BundleMissing(found.Asset.FileName))
            .Map(bundle => Released(host.PolicyOf(subject), found, bundle)),
        plugin: (host, held) => held.Artifact.Bundle
            .ToFin(new SupplyChainFault.BundleMissing(held.Artifact.PluginId))
            .Bind(bundle => Declared(held.Artifact.ContractRange).Map(declared => new Probe(
                held.Artifact.PluginId, Convert.FromHexString(held.Artifact.Sha256), bundle,
                declared, host.HostContractVersion, host.PolicyOf(subject)))));

    static Fin<VersionRange> Declared(string range) =>
        VersionRange.TryParse(range, allowFloating: true, out VersionRange? declared)
            ? Optional(declared).ToFin(new SupplyChainFault.VersionIncompatible(range))
            : Fin.Fail<VersionRange>(new SupplyChainFault.VersionIncompatible(range));

    static Probe Released(TrustPolicy policy, AdmissionSubject.Release found, FileInfo bundle) =>
        new(found.Asset.FileName, Convert.FromHexString(found.Asset.SHA256), bundle,
            policy.ContractRange, found.Asset.Version.ToString(), policy);

    static Validation<Error, (VerifiedIdentity Signer, string Provenance)> Signature(Option<VerificationResult> verified, string subject) =>
        verified.Match(
            Some: result => result switch {
                { SignerIdentity: { } signer, Statement.PredicateType: { } predicate } =>
                    Success<Error, (VerifiedIdentity, string)>((signer, predicate)),
                { SignerIdentity: { } } => Fail<Error, (VerifiedIdentity, string)>(new SupplyChainFault.ProvenanceUnbound(subject)),
                _ => Fail<Error, (VerifiedIdentity, string)>(new SupplyChainFault.SignatureRejected(
                    Optional(result.FailureReason).IfNone($"{subject}: verifier reported no reason"))),
            },
            None: () => Fail<Error, (VerifiedIdentity, string)>(new SupplyChainFault.SignatureRejected(subject)));

    static Validation<Error, NuGetVersion> Version(VersionRange contract, string candidate, string subject) =>
        NuGetVersion.TryParse(candidate, out NuGetVersion? version) && Pinned(contract).Satisfies(version)
            ? Success<Error, NuGetVersion>(version)
            : Fail<Error, NuGetVersion>(new SupplyChainFault.VersionIncompatible($"{candidate} ∉ {contract.PrettyPrint()} ({subject})"));

    public static Validation<Error, NuGetVersion> Best(TrustPolicy policy, Option<NuGetVersion> installed, Seq<NuGetVersion> candidates) =>
        Pinned(policy.ContractRange).FindBestMatch(candidates) switch {
            null => Fail<Error, NuGetVersion>(new SupplyChainFault.VersionIncompatible(
                $"{Band(policy)}: no candidate in range")),
            { } best when installed.Match(Some: held => Pinned(policy.ContractRange).IsBetter(held, best), None: static () => true) =>
                Success<Error, NuGetVersion>(best),
            { } best => Fail<Error, NuGetVersion>(new SupplyChainFault.VersionIncompatible(
                $"{Band(policy)}: {best.ToNormalizedString()} does not improve on {installed.Map(static held => held.ToNormalizedString()).IfNone("<none>")}")),
        };

    static string Band(TrustPolicy policy) =>
        policy.ContractRange.IsFloating
            ? $"{policy.ContractRange.PrettyPrint()} float {policy.ContractRange.Float}"
            : policy.ContractRange.PrettyPrint();

    static VersionRange Pinned(VersionRange contract) =>
        contract.IsFloating ? contract.ToNonSnapshotRange() : contract;

    static Option<FileInfo> Staged(DirectoryInfo staging, string fileName) =>
        new FileInfo(Path.Combine(staging.FullName, $"{fileName}.sigstore.json")) is { Exists: true } file ? Some(file) : None;
}
```

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart TD
    accTitle: Supply-chain admission gate
    accDescr: Release staging and sandbox plugin loads entering one admission gate, with hosted solvers reaching it through the sandbox load, whose failure arm never stages or loads and whose success arm returns the admitted subject.
    Release[UpdateMachine.Stage] -->|AdmissionSubject.Release| Admit[SupplyChainGate.Admit]
    Solver[SolverHost.Register] --> Plugin[SandboxRows.Load]
    Plugin -->|AdmissionSubject.Plugin| Admit
    Admit -->|Validation.Fail| Closed[fail-closed: never stages, never loads]
    Admit -->|Validation.Succ| Proven[SupplyChainAdmission]
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
