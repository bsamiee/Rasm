# [APPHOST_API_SIGSTORE]

`Sigstore` is the offline supply-chain admit-gate verifier: it verifies a cosign/Sigstore bundle (`*.sigstore.json`, bundle media-type `v0.3`) against a trust root, proving both the signature leg (Fulcio certificate identity + transparency-log Merkle inclusion + RFC 3161 timestamp + signed-certificate-timestamp) and the DSSE/in-toto SLSA provenance-attestation leg in one pass. The whole surface is async-only over `Task`, returns a typed `VerificationResult` with `TryVerify*` non-throwing mirrors, and carries the expected signer identity plus the transparency, timestamp, and SCT thresholds in policy.

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

| [INDEX] | [SYMBOL]                         | [KIND]   |
| :-----: | :------------------------------- | :------- |
|  [01]   | `SigstoreVerifier`               | verifier |
|  [02]   | `SigstoreVerifier.ArtifactInput` | input    |
|  [03]   | `HashAlgorithmType`              | enum     |

[SIGSTORE_VERIFIER]: `SigstoreVerifier` is a `sealed` verifier; its parameterless constructor wires the Sigstore public-good instance, and its injection constructor accepts `ITrustRootProvider` plus an optional `ISigningCertificateValidator`.

[ARTIFACT_INPUT]: `SigstoreVerifier.ArtifactInput` is a nested `readonly struct`; `FromStream(Stream)` and `FromDigest(ReadOnlyMemory<byte>, HashAlgorithmType)` discriminate raw bytes from precomputed-digest verification.

[HASH_ALGORITHM]: `HashAlgorithmType` carries `Unspecified`, `Sha256`, `Sha384`, `Sha512`, `Sha3256`, and `Sha3384` for `VerifyDigestAsync` and `ArtifactInput.FromDigest`.

[PUBLIC_TYPE_SCOPE]: policy, identity, and result

- package: `Sigstore`
- namespace: `Sigstore`
- rail: supply-chain

| [INDEX] | [SYMBOL]                      | [KIND]    |
| :-----: | :---------------------------- | :-------- |
|  [01]   | `VerificationPolicy`          | policy    |
|  [02]   | `CertificateIdentity`         | identity  |
|  [03]   | `CertificateExtensionPolicy`  | policy    |
|  [04]   | `VerificationResult`          | result    |
|  [05]   | `VerifiedIdentity`            | identity  |
|  [06]   | `FulcioCertificateExtensions` | claims    |
|  [07]   | `VerifiedTimestamp`           | timestamp |
|  [08]   | `VerificationException`       | exception |

[VERIFICATION_POLICY]: `VerificationPolicy` is `sealed`; it carries expected-signer `CertificateIdentity?`, `RequireTransparencyLog` with `TransparencyLogThreshold` defaulting to `1`, `RequireSignedTimestamps` with `SignedTimestampThreshold`, `RequireSignedCertificateTimestamps` defaulting to `true`, and DER-SPKI `PublicKey` for managed-key mode.

[CERTIFICATE_IDENTITY]: `CertificateIdentity` is `sealed`; it carries `SubjectAlternativeName` with `Pattern`, `Issuer`, and `Extensions` as `CertificateExtensionPolicy`, and its static `ForGitHubActions(owner, repository, issuer?, workflowRef?)` factory builds an identity.

[CERTIFICATE_EXTENSION_POLICY]: `CertificateExtensionPolicy` is `sealed`; it carries the expected Fulcio OID claims `SourceRepositoryUri`/`Ref`/`Digest`/`OwnerUri`, `BuildSignerUri`/`Digest`, `BuildConfigUri`/`Digest`, `BuildTrigger`, `RunnerEnvironment`, and `SourceRepositoryVisibilityAtSigning`.

[VERIFICATION_RESULT]: `VerificationResult` is a `sealed record` carrying `SignerIdentity` as `VerifiedIdentity?`, `VerifiedTimestamps` as `IReadOnlyList<VerifiedTimestamp>`, `Statement` as `InTotoStatement?`, and `FailureReason` as `string?`.

[VERIFIED_IDENTITY]: `VerifiedIdentity` is a `sealed record`; its certificate-derived `SubjectAlternativeName` and `Issuer` are `required`, and `Extensions` is `FulcioCertificateExtensions?`.

[FULCIO_CERTIFICATE_EXTENSIONS]: `FulcioCertificateExtensions` is a `sealed record` carrying observed Fulcio OID values from the certificate: `Issuer`, `BuildSignerUri`/`Digest`, `RunnerEnvironment`, `SourceRepositoryUri`/`Ref`/`Digest`/`Identifier`/`OwnerUri`, `BuildConfigUri`/`Digest`, `BuildTrigger`, `RunInvocationUri`, and `GithubWorkflowTrigger`/`Sha`.

[VERIFIED_TIMESTAMP]: `VerifiedTimestamp` carries a per-source verified timestamp instant from transparency-log integrated time or RFC 3161.

[VERIFICATION_EXCEPTION]: Non-`TryVerify*` members throw `VerificationException` on policy or material failure; the `TryVerify*` mirrors fold it into `(false, null)`.

[PUBLIC_TYPE_SCOPE]: bundle and attestation carriers

- package: `Sigstore`
- namespace: `Sigstore`
- rail: supply-chain

| [INDEX] | [SYMBOL]               | [KIND]       |
| :-----: | :--------------------- | :----------- |
|  [01]   | `SigstoreBundle`       | bundle       |
|  [02]   | `VerificationMaterial` | material     |
|  [03]   | `MessageSignature`     | signature    |
|  [04]   | `DsseEnvelope`         | envelope     |
|  [05]   | `DsseSignature`        | signature    |
|  [06]   | `InTotoStatement`      | statement    |
|  [07]   | `InTotoSubject`        | subject      |
|  [08]   | `TransparencyLogEntry` | transparency |
|  [09]   | `TransparencyLogInfo`  | descriptor   |
|  [10]   | `TimestampInfo`        | timestamp    |
|  [11]   | `TimestampResponse`    | response     |

[SIGSTORE_BUNDLE]: `SigstoreBundle` is `sealed`; it carries `MediaType` as `v0.3+json`, `VerificationMaterial`, `MessageSignature`, and `DsseEnvelope`, with `Deserialize(string)`/`(Stream)`, `Serialize()`/`(Stream)`, async `LoadAsync(FileInfo, ct)`, and async `SaveAsync(FileInfo, ct)`.

[VERIFICATION_MATERIAL]: `VerificationMaterial` carries the certificate or public key, transparency-log entry, and timestamp material in a bundle.

[MESSAGE_SIGNATURE]: `MessageSignature` carries the hash algorithm, digest, and raw signature for the artifact's non-DSSE signature form.

[DSSE_ENVELOPE]: `DsseEnvelope` carries the DSSE-wrapped in-toto payload for the attestation signature form.

[DSSE_SIGNATURE]: `DsseSignature` carries the attestation form's per-key signatures.

[IN_TOTO_STATEMENT]: `InTotoStatement` is `sealed`; it carries `Type`, `PredicateType`, `Subject` as `IReadOnlyList<InTotoSubject>`, and `Predicate` as `JsonElement?`, while static `Parse(ReadOnlyMemory<byte>)`, `Parse(ReadOnlySpan<byte>)`, and `Parse(string)` decode the SLSA-provenance payload surfaced through `VerificationResult.Statement`.

[IN_TOTO_SUBJECT]: `InTotoSubject` carries one attested subject's artifact name and the digest set bound by the statement.

[TRANSPARENCY_LOG_ENTRY]: `TransparencyLogEntry` carries a Rekor log ID, index, integrated time, and inclusion proof or promise.

[TRANSPARENCY_LOG_INFO]: `TransparencyLogInfo` carries the trust-root log descriptor.

[TIMESTAMP_INFO]: `TimestampInfo` carries the RFC 3161 timestamp token.

[TIMESTAMP_RESPONSE]: `TimestampResponse` carries the decoded timestamp source and instant.

[PUBLIC_TYPE_SCOPE]: trust-root providers and validation seams

- package: `Sigstore`
- namespace: `Sigstore`
- rail: supply-chain

| [INDEX] | [SYMBOL]                       | [KIND]    |
| :-----: | :----------------------------- | :-------- |
|  [01]   | `ITrustRootProvider`           | provider  |
|  [02]   | `TufTrustRootProvider`         | provider  |
|  [03]   | `TufTrustRootProviderOptions`  | options   |
|  [04]   | `FileTrustRootProvider`        | provider  |
|  [05]   | `InMemoryTrustRootProvider`    | provider  |
|  [06]   | `TrustedRoot`                  | root      |
|  [07]   | `ISigningCertificateValidator` | validator |
|  [08]   | `IFulcioClient`                | client    |
|  [09]   | `IRekorClient`                 | client    |
|  [10]   | `ITimestampAuthority`          | client    |

[TRUST_ROOT_PROVIDER]: `ITrustRootProvider` owns the `Task<TrustedRoot> GetTrustRootAsync(ct)` trust-anchor seam bound by every verifier constructor.

[TUF_TRUST_ROOT_PROVIDER]: `TufTrustRootProvider` is `sealed` and `IDisposable`; it carries a TUF-backed live trust root, static `ProductionUrl` and `StagingUrl` `Uri` values, and a `(Uri repositoryUrl, TufTrustRootProviderOptions?)` constructor.

[TUF_TRUST_ROOT_PROVIDER_OPTIONS]: `TufTrustRootProviderOptions` is `sealed`; it carries `CustomTrustedRoot` as `byte[]?`, `Cache` as `ITufCache?`, and `Repository` as `ITufRepository?`, and `CustomTrustedRoot` with a local cache forms the air-gapped configuration.

[FILE_TRUST_ROOT_PROVIDER]: `FileTrustRootProvider` is `sealed`; its `(FileInfo)` constructor loads pinned `trusted_root.json` from disk as the offline or air-gapped anchor.

[IN_MEMORY_TRUST_ROOT_PROVIDER]: `InMemoryTrustRootProvider` is `sealed`; its `(TrustedRoot)` constructor binds a fully in-memory anchor for tests and embedded roots.

[TRUSTED_ROOT]: `TrustedRoot`, returned by `ITrustRootProvider`, carries the decoded trust bundle: Fulcio CA chains, Rekor and CT log keys, and timestamp-authority chains.

[SIGNING_CERTIFICATE_VALIDATOR]: `ISigningCertificateValidator` is the optional certificate-validation seam; `DefaultSigningCertificateValidator` is the wired default and returns `SigningCertificateValidationResult`.

[FULCIO_CLIENT]: `IFulcioClient` owns the keyless-signing certificate-request seam consumed by the sign path, not the offline verify gate.

[REKOR_CLIENT]: `IRekorClient` owns the keyless-signing log upload and lookup seam consumed by the sign path, not the offline verify gate.

[TIMESTAMP_AUTHORITY]: `ITimestampAuthority` owns the keyless-signing RFC 3161 stamp seam consumed by the sign path, not the offline verify gate.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: SigstoreVerifier — verify (throwing + try mirrors)

- package: `Sigstore`
- namespace: `Sigstore`
- rail: supply-chain

Every verify member is `async Task`. The `Verify*` members throw `VerificationException` on failure; the `TryVerify*` members return `(bool Success, VerificationResult? Result)` and never throw on policy or material failure, so the admit gate folds that tuple onto `Fin<T>`. The policy carries the per-call expected identity and thresholds.

| [INDEX] | [SURFACE]                               | [INPUT] | [FAILURE] |
| :-----: | :-------------------------------------- | :------ | :-------- |
|  [01]   | `SigstoreVerifier.VerifyAsync`          | bytes   | throws    |
|  [02]   | `SigstoreVerifier.VerifyStreamAsync`    | stream  | throws    |
|  [03]   | `SigstoreVerifier.VerifyDigestAsync`    | digest  | throws    |
|  [04]   | `SigstoreVerifier.VerifyFileAsync`      | files   | throws    |
|  [05]   | `SigstoreVerifier.TryVerifyAsync`       | bytes   | result    |
|  [06]   | `SigstoreVerifier.TryVerifyStreamAsync` | stream  | result    |
|  [07]   | `SigstoreVerifier.TryVerifyDigestAsync` | digest  | result    |
|  [08]   | `SigstoreVerifier.TryVerifyFileAsync`   | files   | result    |

[VERIFY_ASYNC]: `SigstoreVerifier.VerifyAsync(ReadOnlyMemory<byte> artifact, SigstoreBundle, VerificationPolicy, ct)` verifies in-memory artifact bytes against the bundle.

[VERIFY_STREAM_ASYNC]: `SigstoreVerifier.VerifyStreamAsync(Stream artifact, SigstoreBundle, VerificationPolicy, ct)` verifies a streamed artifact and hashes it as it reads.

[VERIFY_DIGEST_ASYNC]: `SigstoreVerifier.VerifyDigestAsync(ReadOnlyMemory<byte> artifactDigest, HashAlgorithmType, SigstoreBundle, VerificationPolicy, ct)` verifies a precomputed digest without rereading the artifact.

[VERIFY_FILE_ASYNC]: `SigstoreVerifier.VerifyFileAsync(FileInfo artifact, FileInfo bundle, VerificationPolicy, ct)` verifies a file against an on-disk bundle file.

[TRY_VERIFY_ASYNC]: `SigstoreVerifier.TryVerifyAsync(ReadOnlyMemory<byte>, SigstoreBundle, VerificationPolicy, ct)` returns `(bool, VerificationResult?)` for non-throwing byte verification.

[TRY_VERIFY_STREAM_ASYNC]: `SigstoreVerifier.TryVerifyStreamAsync(Stream, SigstoreBundle, VerificationPolicy, ct)` returns `(bool, VerificationResult?)` for non-throwing stream verification.

[TRY_VERIFY_DIGEST_ASYNC]: `SigstoreVerifier.TryVerifyDigestAsync(ReadOnlyMemory<byte>, HashAlgorithmType, SigstoreBundle, VerificationPolicy, ct)` returns `(bool, VerificationResult?)` for non-throwing digest verification.

[TRY_VERIFY_FILE_ASYNC]: `SigstoreVerifier.TryVerifyFileAsync(FileInfo, FileInfo, VerificationPolicy, ct)` returns `(bool, VerificationResult?)` for non-throwing file verification.

[ENTRYPOINT_SCOPE]: bundle and policy construction

- package: `Sigstore`
- namespace: `Sigstore`
- rail: supply-chain

| [INDEX] | [SURFACE]                                   | [ROLE]   |
| :-----: | :------------------------------------------ | :------- |
|  [01]   | `SigstoreBundle.Deserialize`                | parse    |
|  [02]   | `SigstoreBundle.LoadAsync`                  | load     |
|  [03]   | `CertificateIdentity.ForGitHubActions`      | identity |
|  [04]   | `SigstoreVerifier.ArtifactInput.FromStream` | stream   |
|  [05]   | `SigstoreVerifier.ArtifactInput.FromDigest` | digest   |
|  [06]   | `new TufTrustRootProvider`                  | anchor   |

[BUNDLE_DESERIALIZE]: `SigstoreBundle.Deserialize(string json)` and `SigstoreBundle.Deserialize(Stream)` parse a `*.sigstore.json` bundle.

[BUNDLE_LOAD]: `SigstoreBundle.LoadAsync(FileInfo, ct)` asynchronously loads a bundle file.

[GITHUB_ACTIONS_IDENTITY]: `CertificateIdentity.ForGitHubActions(string owner, string repository, string issuer = "https://token.actions.githubusercontent.com", string? workflowRef = null)` builds the expected GitHub Actions OIDC signer identity.

[ARTIFACT_INPUT_STREAM]: `SigstoreVerifier.ArtifactInput.FromStream(Stream)` selects the stream input shape before verification.

[ARTIFACT_INPUT_DIGEST]: `SigstoreVerifier.ArtifactInput.FromDigest(ReadOnlyMemory<byte>, HashAlgorithmType)` selects the precomputed-digest input shape before verification.

[TUF_ANCHOR_CONSTRUCTION]: `new TufTrustRootProvider(TufTrustRootProvider.ProductionUrl, new TufTrustRootProviderOptions { CustomTrustedRoot = bytes, Cache = cache })` constructs an offline-capable TUF anchor.

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
