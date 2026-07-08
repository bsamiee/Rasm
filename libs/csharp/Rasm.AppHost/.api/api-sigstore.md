# [APPHOST_API_SIGSTORE]

`Sigstore` is the offline supply-chain admit-gate verifier: it verifies a cosign/Sigstore
bundle (`*.sigstore.json`, bundle media-type `v0.3`) against a trust root, proving both the
signature leg (Fulcio certificate identity + transparency-log Merkle inclusion + RFC 3161
timestamp + signed-certificate-timestamp) AND the DSSE/in-toto SLSA provenance-attestation
leg in one pass. The whole surface is async-only over `Task`, returns a typed
`VerificationResult` (plus `TryVerify*` non-throwing mirrors), and the policy carries the
expected signer identity and the transparency/timestamp/SCT thresholds.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Sigstore`
- package: `Sigstore`
- license: MIT (nuspec `license` expression)
- assembly: `Sigstore`
- namespace: `Sigstore`
- asset: net10.0 only; the net10.0 consumer binds the `lib/net10.0` asset (single TFM, no fallback ambiguity)
- xml-doc: ships `Sigstore.xml`
- rail: supply-chain

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: verifier entrypoint and artifact input
- package: `Sigstore`
- namespace: `Sigstore`
- rail: supply-chain

| [INDEX] | [SYMBOL]                       | [RAIL]       | [CAPABILITY]                                                                                  |
| :-----: | :----------------------------- | :----------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `SigstoreVerifier`             | supply-chain | `sealed` verifier; parameterless ctor wires the Sigstore public-good instance, or inject `ITrustRootProvider` + optional `ISigningCertificateValidator` |
|  [02]   | `SigstoreVerifier.ArtifactInput` | supply-chain | nested `readonly struct`: `FromStream(Stream)` / `FromDigest(ReadOnlyMemory<byte>, HashAlgorithmType)` — discriminates raw-bytes vs precomputed-digest verification |
|  [03]   | `HashAlgorithmType`            | supply-chain | enum: `Unspecified`, `Sha256`, `Sha384`, `Sha512`, `Sha3256`, `Sha3384` — digest algorithm for `VerifyDigestAsync`/`ArtifactInput.FromDigest` |

[PUBLIC_TYPE_SCOPE]: policy, identity, and result
- package: `Sigstore`
- namespace: `Sigstore`
- rail: supply-chain

| [INDEX] | [SYMBOL]                       | [RAIL]       | [CAPABILITY]                                                                                  |
| :-----: | :----------------------------- | :----------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `VerificationPolicy`           | supply-chain | `sealed`; expected-signer `CertificateIdentity?`, `RequireTransparencyLog`/`TransparencyLogThreshold` (default 1), `RequireSignedTimestamps`/`SignedTimestampThreshold`, `RequireSignedCertificateTimestamps` (default `true`), `PublicKey` (DER SPKI for managed-key mode) |
|  [02]   | `CertificateIdentity`          | supply-chain | `sealed`; `SubjectAlternativeName`(+`Pattern`), `Issuer`, `Extensions` (`CertificateExtensionPolicy`); static `ForGitHubActions(owner, repository, issuer?, workflowRef?)` factory |
|  [03]   | `CertificateExtensionPolicy`   | supply-chain | `sealed`; the expected Fulcio OID claims to assert (`SourceRepositoryUri`/`Ref`/`Digest`/`OwnerUri`, `BuildSignerUri`/`Digest`, `BuildConfigUri`/`Digest`, `BuildTrigger`, `RunnerEnvironment`, `SourceRepositoryVisibilityAtSigning`) |
|  [04]   | `VerificationResult`           | supply-chain | `sealed record`; `SignerIdentity` (`VerifiedIdentity?`), `VerifiedTimestamps` (`IReadOnlyList<VerifiedTimestamp>`), `Statement` (`InTotoStatement?`), `FailureReason` (`string?`) |
|  [05]   | `VerifiedIdentity`             | supply-chain | `sealed record`; verified-from-cert `SubjectAlternativeName` + `Issuer` (both `required`), `Extensions` (`FulcioCertificateExtensions?`) |
|  [06]   | `FulcioCertificateExtensions`  | supply-chain | `sealed record`; the OBSERVED Fulcio OID values read off the cert (`Issuer`, `BuildSignerUri`/`Digest`, `RunnerEnvironment`, `SourceRepositoryUri`/`Ref`/`Digest`/`Identifier`/`OwnerUri`, `BuildConfigUri`/`Digest`, `BuildTrigger`, `RunInvocationUri`, `GithubWorkflowTrigger`/`Sha`) |
|  [07]   | `VerifiedTimestamp`            | supply-chain | per-source verified timestamp instant (transparency-log integrated-time or RFC 3161) |
|  [08]   | `VerificationException`        | supply-chain | thrown by the non-`TryVerify*` members on policy/material failure; the `TryVerify*` mirrors fold it into `(false, null)` |

[PUBLIC_TYPE_SCOPE]: bundle and attestation carriers
- package: `Sigstore`
- namespace: `Sigstore`
- rail: supply-chain

| [INDEX] | [SYMBOL]                       | [RAIL]       | [CAPABILITY]                                                                                  |
| :-----: | :----------------------------- | :----------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `SigstoreBundle`               | supply-chain | `sealed`; `MediaType` (`v0.3+json`), `VerificationMaterial`, `MessageSignature`, `DsseEnvelope`; `Deserialize(string)`/`(Stream)`, `Serialize()`/`(Stream)`, async `LoadAsync(FileInfo, ct)`/`SaveAsync(FileInfo, ct)` |
|  [02]   | `VerificationMaterial`         | supply-chain | the cert-or-public-key + transparency-log-entry + timestamp material carried by a bundle |
|  [03]   | `MessageSignature`             | supply-chain | hash-algorithm + digest + raw signature for the artifact (non-DSSE) signature form |
|  [04]   | `DsseEnvelope` / `DsseSignature` | supply-chain | DSSE-wrapped in-toto payload + per-key signatures (the attestation signature form) |
|  [05]   | `InTotoStatement`              | supply-chain | `sealed`; decoded in-toto statement — `Type`, `PredicateType`, `Subject` (`IReadOnlyList<InTotoSubject>`), `Predicate` (`JsonElement?`); static `Parse(ReadOnlyMemory<byte>)`/`(ReadOnlySpan<byte>)`/`(string)`; the SLSA-provenance payload `VerificationResult.Statement` surfaces |
|  [06]   | `InTotoSubject`                | supply-chain | one attested subject: artifact name + digest set the statement binds |
|  [07]   | `TransparencyLogEntry` / `TransparencyLogInfo` | supply-chain | a Rekor log entry (log id, index, integrated time, inclusion proof/promise) + the trust-root log descriptor |
|  [08]   | `TimestampInfo` / `TimestampResponse` | supply-chain | RFC 3161 timestamp token + its decoded source/instant |

[PUBLIC_TYPE_SCOPE]: trust-root providers and validation seams
- package: `Sigstore`
- namespace: `Sigstore`
- rail: supply-chain

| [INDEX] | [SYMBOL]                       | [RAIL]       | [CAPABILITY]                                                                                  |
| :-----: | :----------------------------- | :----------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `ITrustRootProvider`           | supply-chain | `Task<TrustedRoot> GetTrustRootAsync(ct)` — the single trust-anchor seam every verifier ctor binds |
|  [02]   | `TufTrustRootProvider`         | supply-chain | `sealed`, `IDisposable`; TUF-backed live trust root; `ProductionUrl`/`StagingUrl` static `Uri`; ctor `(Uri repositoryUrl, TufTrustRootProviderOptions?)` |
|  [03]   | `TufTrustRootProviderOptions`  | supply-chain | `sealed`; `CustomTrustedRoot` (`byte[]?`), `Cache` (`ITufCache?`), `Repository` (`ITufRepository?`) — air-gap by supplying `CustomTrustedRoot` + a local cache |
|  [04]   | `FileTrustRootProvider`        | supply-chain | `sealed`; ctor `(FileInfo)` — loads a pinned `trusted_root.json` from disk (the offline/air-gapped anchor) |
|  [05]   | `InMemoryTrustRootProvider`    | supply-chain | `sealed`; ctor `(TrustedRoot)` — a fully in-memory anchor for tests and embedded roots |
|  [06]   | `ITrustRootProvider` impl `TrustedRoot` | supply-chain | the decoded trust bundle: Fulcio CA chain(s), Rekor + CT log keys, timestamp-authority chain(s) |
|  [07]   | `ISigningCertificateValidator` | supply-chain | optional cert-validation seam; `DefaultSigningCertificateValidator` is the wired default (`SigningCertificateValidationResult`) |
|  [08]   | `IFulcioClient` / `IRekorClient` / `ITimestampAuthority` | supply-chain | the keyless-signing client seams (cert request, log upload/lookup, RFC 3161 stamp) — consumed by the SIGN path, not the offline verify gate |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: SigstoreVerifier — verify (throwing + try mirrors)
- package: `Sigstore`
- namespace: `Sigstore`
- rail: supply-chain

Every verify member is `async Task`. The `Verify*` members throw `VerificationException` on
failure; the `TryVerify*` members return `(bool Success, VerificationResult? Result)` and
never throw on a policy/material failure — that tuple is the ROP-shaped entry the admit gate
folds onto `Fin<T>`. The policy is the per-call expected-identity + threshold carrier.

| [INDEX] | [SURFACE]                                | [CALL_SHAPE]                                                                                  | [CAPABILITY]                                            |
| :-----: | :--------------------------------------- | :------------------------------------------------------------------------------------------- | :----------------------------------------------------- |
|  [01]   | `SigstoreVerifier.VerifyAsync`           | `(ReadOnlyMemory<byte> artifact, SigstoreBundle, VerificationPolicy, ct)`                     | verify in-memory artifact bytes against the bundle      |
|  [02]   | `SigstoreVerifier.VerifyStreamAsync`     | `(Stream artifact, SigstoreBundle, VerificationPolicy, ct)`                                   | verify a streamed artifact (hashes as it reads)         |
|  [03]   | `SigstoreVerifier.VerifyDigestAsync`     | `(ReadOnlyMemory<byte> artifactDigest, HashAlgorithmType, SigstoreBundle, VerificationPolicy, ct)` | verify against a precomputed digest (no artifact re-read) |
|  [04]   | `SigstoreVerifier.VerifyFileAsync`       | `(FileInfo artifact, FileInfo bundle, VerificationPolicy, ct)`                                | verify a file against an on-disk bundle file            |
|  [05]   | `SigstoreVerifier.TryVerifyAsync`        | `(ReadOnlyMemory<byte>, SigstoreBundle, VerificationPolicy, ct)` → `(bool, VerificationResult?)` | non-throwing bytes verify                               |
|  [06]   | `SigstoreVerifier.TryVerifyStreamAsync`  | `(Stream, SigstoreBundle, VerificationPolicy, ct)` → `(bool, VerificationResult?)`           | non-throwing stream verify                              |
|  [07]   | `SigstoreVerifier.TryVerifyDigestAsync`  | `(ReadOnlyMemory<byte>, HashAlgorithmType, SigstoreBundle, VerificationPolicy, ct)` → `(bool, VerificationResult?)` | non-throwing digest verify                              |
|  [08]   | `SigstoreVerifier.TryVerifyFileAsync`    | `(FileInfo, FileInfo, VerificationPolicy, ct)` → `(bool, VerificationResult?)`               | non-throwing file verify                                |

[ENTRYPOINT_SCOPE]: bundle and policy construction
- package: `Sigstore`
- namespace: `Sigstore`
- rail: supply-chain

| [INDEX] | [SURFACE]                                | [CALL_SHAPE]                                                  | [CAPABILITY]                                            |
| :-----: | :--------------------------------------- | :----------------------------------------------------------- | :----------------------------------------------------- |
|  [01]   | `SigstoreBundle.Deserialize`             | `(string json)` / `(Stream)`                                | parse a `*.sigstore.json` bundle                        |
|  [02]   | `SigstoreBundle.LoadAsync`               | `(FileInfo, ct)`                                            | async-load a bundle file                                |
|  [03]   | `CertificateIdentity.ForGitHubActions`   | `(string owner, string repository, string issuer = "https://token.actions.githubusercontent.com", string? workflowRef = null)` | build the expected GitHub-Actions OIDC signer identity |
|  [04]   | `SigstoreVerifier.ArtifactInput.FromStream` / `.FromDigest` | `(Stream)` / `(ReadOnlyMemory<byte>, HashAlgorithmType)` | discriminate the artifact-input shape before verify     |
|  [05]   | `new TufTrustRootProvider`               | `(TufTrustRootProvider.ProductionUrl, new TufTrustRootProviderOptions { CustomTrustedRoot = bytes, Cache = cache })` | construct an offline-capable TUF anchor                 |

## [04]-[IMPLEMENTATION_LAW]

[VERIFY_GATE]:
- the admit gate is a single `await verifier.TryVerify*Async(input, bundle, policy, ct)` returning `(Success, VerificationResult?)`; map `Success=false` (or a non-null `FailureReason`) to the supply-chain reject arm and lower onto `Fin<T>` — never let `VerificationException` escape the domain.
- `policy.CertificateIdentity` (built via `CertificateIdentity.ForGitHubActions` or set directly) is what pins the EXPECTED signer; an empty policy verifies cryptographic integrity but asserts no identity, so the gate MUST set it.
- thresholds (`TransparencyLogThreshold`, `SignedTimestampThreshold`, `RequireSignedCertificateTimestamps`) are the policy knobs; leave `RequireTransparencyLog`/`RequireSignedCertificateTimestamps` at their `true` defaults for keyless cosign bundles.
- managed-key mode: set `policy.PublicKey` (DER SPKI) for a non-Fulcio raw-key signature instead of a `CertificateIdentity`.

[OFFLINE_TRUST]:
- air-gapped admission uses `FileTrustRootProvider(new FileInfo(pinnedTrustedRootJson))` or `TufTrustRootProvider(url, new TufTrustRootProviderOptions { CustomTrustedRoot = rootBytes, Cache = localCache })`; the verify path then performs NO network call.
- `InMemoryTrustRootProvider(trustedRoot)` is the embedded/test anchor; inject any provider through `new SigstoreVerifier(provider, validator?)`.

[ATTESTATION_LEG]:
- a DSSE/in-toto provenance bundle (`SigstoreBundle.DsseEnvelope` set) yields a decoded `VerificationResult.Statement` (`InTotoStatement`); read `Statement.Subject` (`IReadOnlyList<InTotoSubject>`, each `Name` + `Digest` map) to bind the attested artifact to the package being admitted — one verify proves signature AND provenance.
- the observed Fulcio claims (`VerifiedIdentity.Extensions` → `FulcioCertificateExtensions`) carry the SLSA build provenance (`BuildSignerUri`, `RunnerEnvironment`, `SourceRepositoryUri`/`Ref`/`Digest`); assert them either declaratively via `policy.CertificateIdentity.Extensions` (`CertificateExtensionPolicy`) or imperatively against the returned record.

[INTEGRATION_STACK]:
- semver leg: this gate is the SIGNATURE half of the admit decision; the sibling `NuGet.Versioning` (`api-nuget-versioning`, same SUPPLY_CHAIN manifest cluster) is the VERSION half — `NuGetVersion` + `VersionRange.Satisfies` performs the SemVer-2.0 contract-range check `System.Version` cannot. One admit row composes both: `TryVerify*Async` proves the artifact, then `VersionRange.Satisfies(NuGetVersion.Parse(...))` proves the version is in-contract before the package is bound.
- identity leg: a passing `VerificationResult.SignerIdentity` (`VerifiedIdentity`) is the verified principal the AppHost `Agent/capability` GrantBroker can treat as a trusted publisher — distinct from the request-time `IdentityModel` JWT principal (`api-identitymodel-jwt`), but folded onto the same authorization decision when admitting third-party plugin/SDK artifacts.
- identity-digest leg: hash the artifact once with `System.IO.Hashing` `XxHash3` (substrate, `api-hashing`) for the content-key, then pass the SHA-256/384 digest as `ArtifactInput.FromDigest(digest, HashAlgorithmType.Sha256)` to `VerifyDigestAsync` so the verify reuses the already-computed digest instead of re-reading the artifact stream.
- provisioning leg: `Sandbox/provisioning` post-fetch admission runs this gate over a downloaded plugin/companion artifact bundle BEFORE `Velopack` (`api-velopack`) applies it; a failed verify aborts the install, so the verify result is the precondition the `Sandbox/isolation` plugin loader and the `Velopack` apply both gate on.
- resilience leg: the network-bound `TufTrustRootProvider.GetTrustRootAsync` (when not air-gapped) is an outbound call eligible for the `Polly.Core`/`Microsoft.Extensions.Http.Resilience` (`api-polly-core`, `api-resilience`) pipeline; the offline `FileTrustRootProvider` removes that dependency entirely for a hermetic gate.
- telemetry leg: wrap the verify in an `Microsoft.Extensions.Telemetry` (`api-telemetry`) span and emit `VerificationResult.FailureReason` / `SignerIdentity.SubjectAlternativeName` as span attributes so a rejected admission is observable on the four-signal rail.

[LOCAL_ADMISSION]:
- supply-chain verification enters through `SigstoreVerifier.TryVerify*Async`; the trust anchor is selected once at composition by the injected `ITrustRootProvider` (file/TUF/in-memory).
- bundle loading enters through `SigstoreBundle.LoadAsync`/`Deserialize`; identity expectation is built once via `CertificateIdentity.ForGitHubActions` or a literal `CertificateIdentity`.

[RAIL_LAW]:
- Package: `Sigstore`
- Owns: offline cosign/Sigstore bundle verification (keyless cert identity + transparency-log inclusion + RFC 3161 timestamp + SCT) and DSSE/in-toto SLSA attestation decode
- Accept: supply-chain admit gating, publisher-identity assertion, provenance extraction
- Reject: signing/issuance orchestration as a domain concern (the `IFulcioClient`/`IRekorClient` SIGN seams are present but the admit gate is verify-only), version-range comparison (NuGet.Versioning), JWT request authentication (IdentityModel)
