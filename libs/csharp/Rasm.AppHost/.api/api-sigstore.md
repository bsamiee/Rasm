# [RASM_APPHOST_API_SIGSTORE]

`Sigstore` owns the offline supply-chain admit-gate verifier: it verifies a cosign/Sigstore bundle (`*.sigstore.json`, media-type `v0.3`) against a trust root, proving the signature leg (Fulcio certificate identity, transparency-log inclusion, RFC 3161 timestamp, signed-certificate-timestamp) and the DSSE/in-toto SLSA provenance-attestation leg in one pass. Every member is async over `Task`, returning a typed `VerificationResult` with non-throwing `TryVerify*` mirrors, and pins the expected signer with the transparency, timestamp, and SCT thresholds in `VerificationPolicy`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Sigstore`
- package: `Sigstore` (`MIT`)
- assembly: `Sigstore`
- namespace: `Sigstore`
- asset: net10.0 single-TFM runtime library binding `lib/net10.0`
- rail: supply-chain

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: verifier entrypoint and digest input

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                       |
| :-----: | :------------------ | :------------ | :--------------------------------- |
|  [01]   | `SigstoreVerifier`  | class         | offline bundle-verify orchestrator |
|  [02]   | `HashAlgorithmType` | enum          | digest algorithm selector          |

[HashAlgorithmType]: `Unspecified` `Sha256` `Sha384` `Sha512` `Sha3256` `Sha3384`

- `SigstoreVerifier`: sealed; the parameterless ctor binds the public-good TUF anchor (network fetch on first use), the `(ITrustRootProvider, ISigningCertificateValidator?)` ctor injects an offline anchor.

[PUBLIC_TYPE_SCOPE]: policy, identity, and result

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                        |
| :-----: | :---------------------------- | :------------ | :---------------------------------- |
|  [01]   | `VerificationPolicy`          | class         | expected-signer and threshold knobs |
|  [02]   | `CertificateIdentity`         | class         | expected SAN, issuer, extensions    |
|  [03]   | `CertificateExtensionPolicy`  | class         | expected Fulcio OID claim set       |
|  [04]   | `VerificationResult`          | record        | verify-verdict carrier              |
|  [05]   | `VerifiedIdentity`            | record        | certificate-derived signer          |
|  [06]   | `FulcioCertificateExtensions` | record        | observed Fulcio OID values          |
|  [07]   | `VerifiedTimestamp`           | record        | per-source verified instant         |
|  [08]   | `VerificationException`       | class         | throwing-path failure               |

- `VerificationPolicy`: carries `CertificateIdentity?`, `RequireTransparencyLog` with `TransparencyLogThreshold` (default `1`), `RequireSignedTimestamps` with `SignedTimestampThreshold`, `RequireSignedCertificateTimestamps` (default `true`), and DER-SPKI `PublicKey` for managed-key mode.
- `VerificationResult`: `SignerIdentity` (`VerifiedIdentity?`), `VerifiedTimestamps` (`IReadOnlyList<VerifiedTimestamp>`), `Statement` (`InTotoStatement?`), `FailureReason` (`string?`).
- `CertificateIdentity`: `SubjectAlternativeName` (`Pattern`), `Issuer`, and `Extensions` (`CertificateExtensionPolicy`).
- `VerificationException`: throwing members raise it on policy or material failure; the `TryVerify*` mirrors fold it into `(false, null)`.

[PUBLIC_TYPE_SCOPE]: bundle and attestation carriers

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                         |
| :-----: | :--------------------- | :------------ | :----------------------------------- |
|  [01]   | `SigstoreBundle`       | class         | cosign bundle root                   |
|  [02]   | `VerificationMaterial` | class         | cert/key, log entry, and timestamp   |
|  [03]   | `MessageSignature`     | class         | non-DSSE artifact signature          |
|  [04]   | `DsseEnvelope`         | class         | DSSE-wrapped in-toto payload         |
|  [05]   | `DsseSignature`        | class         | per-key attestation signature        |
|  [06]   | `InTotoStatement`      | class         | SLSA provenance statement            |
|  [07]   | `InTotoSubject`        | class         | attested subject and digest set      |
|  [08]   | `TransparencyLogEntry` | class         | Rekor id, index, inclusion proof     |
|  [09]   | `TransparencyLogInfo`  | class         | trust-root log descriptor            |
|  [10]   | `TimestampInfo`        | class         | RFC 3161 timestamp token             |
|  [11]   | `TimestampResponse`    | class         | decoded timestamp source and instant |

- `SigstoreBundle`: `MediaType` is `v0.3+json`.
- `InTotoStatement`: `Type`, `PredicateType`, `Subject` (`IReadOnlyList<InTotoSubject>`), and `Predicate` (`JsonElement?`), surfaced through `VerificationResult.Statement`.

[PUBLIC_TYPE_SCOPE]: trust-root providers and validation seams

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                        |
| :-----: | :----------------------------- | :------------ | :---------------------------------- |
|  [01]   | `ITrustRootProvider`           | interface     | trust-anchor seam                   |
|  [02]   | `TufTrustRootProvider`         | class         | TUF-backed live or offline anchor   |
|  [03]   | `TufTrustRootProviderOptions`  | class         | custom-root and cache config        |
|  [04]   | `FileTrustRootProvider`        | class         | pinned `trusted_root.json` anchor   |
|  [05]   | `InMemoryTrustRootProvider`    | class         | embedded and test anchor            |
|  [06]   | `TrustedRoot`                  | class         | decoded trust bundle                |
|  [07]   | `ISigningCertificateValidator` | interface     | optional cert-validation seam       |
|  [08]   | `IFulcioClient`                | interface     | keyless-sign cert-request seam      |
|  [09]   | `IRekorClient`                 | interface     | keyless-sign log upload/lookup seam |
|  [10]   | `ITimestampAuthority`          | interface     | keyless-sign RFC 3161 stamp seam    |

- `ITrustRootProvider`: owns `GetTrustRootAsync(ct) -> Task<TrustedRoot>`, bound by every verifier ctor; `TrustedRoot` carries the Fulcio CA chains, Rekor and CT log keys, and timestamp-authority chains.
- `TufTrustRootProvider`: sealed, `IDisposable`; static `ProductionUrl`/`StagingUrl` `Uri` values, ctor `(Uri, TufTrustRootProviderOptions?)`; `TufTrustRootProviderOptions.CustomTrustedRoot` (`byte[]?`) with `Cache` (`ITufCache?`) forms the air-gapped root.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: verify — throwing and try mirrors

Every verify member is `async Task`, trailing `(…, SigstoreBundle bundle, VerificationPolicy policy, ct)`; `Verify*` throws `VerificationException`, and `TryVerify*` returns `(bool Success, VerificationResult?)` and never throws on policy or material failure, so the admit gate folds that tuple onto `Fin<T>`.

| [INDEX] | [SURFACE]                                                       | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :-------------------------------------------------------------- | :------- | :----------------------------- |
|  [01]   | `VerifyAsync(ReadOnlyMemory<byte>)`                             | instance | in-memory artifact bytes       |
|  [02]   | `VerifyStreamAsync(Stream)`                                     | instance | streams and hashes as it reads |
|  [03]   | `VerifyDigestAsync(ReadOnlyMemory<byte>, HashAlgorithmType)`    | instance | precomputed digest, no reread  |
|  [04]   | `VerifyFileAsync(FileInfo, FileInfo)`                           | instance | file against on-disk bundle    |
|  [05]   | `TryVerifyAsync(ReadOnlyMemory<byte>)`                          | instance | non-throwing bytes             |
|  [06]   | `TryVerifyStreamAsync(Stream)`                                  | instance | non-throwing stream            |
|  [07]   | `TryVerifyDigestAsync(ReadOnlyMemory<byte>, HashAlgorithmType)` | instance | non-throwing digest            |
|  [08]   | `TryVerifyFileAsync(FileInfo, FileInfo)`                        | instance | non-throwing file              |

[ENTRYPOINT_SCOPE]: bundle, policy, and anchor construction

| [INDEX] | [SURFACE]                                                                | [SHAPE] | [CAPABILITY]                        |
| :-----: | :----------------------------------------------------------------------- | :------ | :---------------------------------- |
|  [01]   | `SigstoreBundle.Deserialize(string)`                                     | static  | parse a bundle JSON string          |
|  [02]   | `SigstoreBundle.Deserialize(Stream)`                                     | static  | parse a bundle JSON stream          |
|  [03]   | `SigstoreBundle.LoadAsync(FileInfo, ct)`                                 | static  | load a bundle file                  |
|  [04]   | `CertificateIdentity.ForGitHubActions(string, string, string?, string?)` | static  | GitHub Actions OIDC signer identity |
|  [05]   | `InTotoStatement.Parse(ReadOnlyMemory<byte>) -> InTotoStatement?`        | static  | decode the SLSA provenance payload  |
|  [06]   | `new TufTrustRootProvider(Uri, TufTrustRootProviderOptions?)`            | ctor    | offline-capable TUF anchor          |

- `CertificateIdentity.ForGitHubActions`: `issuer` defaults to `https://token.actions.githubusercontent.com`, `workflowRef` to `null`.
- `InTotoStatement.Parse`: also overloads on `ReadOnlySpan<byte>` and `string`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One `await verifier.TryVerify*Async(input, bundle, policy, ct)` is the admit gate, returning `(Success, VerificationResult?)`; `Success=false` or a non-null `FailureReason` maps to the supply-chain reject arm lowered onto `Fin<T>`, and `VerificationException` never escapes the domain.
- `policy.CertificateIdentity` pins the expected signer; an empty policy proves cryptographic integrity but asserts no identity, so the gate sets it via `CertificateIdentity.ForGitHubActions` or a literal `CertificateIdentity`.
- Keyless cosign bundles keep `RequireTransparencyLog` and `RequireSignedCertificateTimestamps` at `true`; `TransparencyLogThreshold`, `SignedTimestampThreshold`, and `RequireSignedCertificateTimestamps` tune the thresholds. Managed-key mode sets `policy.PublicKey` (DER SPKI) instead of a `CertificateIdentity`.
- Air-gapped admission binds `FileTrustRootProvider(FileInfo)` or `TufTrustRootProvider(url, options { CustomTrustedRoot, Cache })` and makes no network call; `InMemoryTrustRootProvider(TrustedRoot)` is the embedded anchor, injected through `new SigstoreVerifier(provider, validator?)`.
- One verify proves both legs: a DSSE/in-toto bundle yields `VerificationResult.Statement` (`InTotoStatement`), whose `Subject` binds the attested artifact, while the observed Fulcio claims (`VerifiedIdentity.Extensions` -> `FulcioCertificateExtensions`) carry SLSA build provenance (`BuildSignerUri`, `RunnerEnvironment`, `SourceRepositoryUri`/`Ref`/`Digest`), asserted declaratively via `policy.CertificateIdentity.Extensions` or imperatively against the returned record.

[STACKING]:
- `api-nuget-versioning`(`.api/api-nuget-versioning.md`): the version leg of the same `SupplyChainGate.Admit` row — `TryVerify*Async` proves the artifact, then `VersionRange.Satisfies(NuGetVersion.Parse(...))` proves the version in-contract before binding, the two legs accumulating one verdict.
- `api-identitymodel-jwt`(`.api/api-identitymodel-jwt.md`): a passing `VerificationResult.SignerIdentity` (`VerifiedIdentity`) is the trusted-publisher principal the `Agent/capability` GrantBroker folds onto the same authorization decision as the request-time JWT principal when admitting third-party artifacts.
- `api-hashing`(`.api/api-hashing.md`): hash the artifact once with `XxHash3` for the content-key, then pass the SHA-256/384 digest straight to `VerifyDigestAsync(digest, HashAlgorithmType.Sha256, …)` so verify reuses the digest instead of rereading the stream.
- `api-velopack`(`.api/api-velopack.md`): `Sandbox/provisioning` post-fetch admission runs this gate over a downloaded artifact bundle before `Velopack` applies it, a failed verify aborting the install and gating both the `Sandbox/isolation` plugin loader and the `Velopack` apply.
- `api-polly-core`(`.api/api-polly-core.md`), `api-resilience`(`.api/api-resilience.md`): the network-bound `TufTrustRootProvider.GetTrustRootAsync` is an outbound call eligible for the resilience pipeline; `FileTrustRootProvider` removes the dependency for a hermetic gate.
- `api-telemetry`(`.api/api-telemetry.md`): wrap the verify in a telemetry span emitting `VerificationResult.FailureReason` and `SignerIdentity.SubjectAlternativeName` as span attributes, so a rejected admission is observable on the four-signal rail.
- within-lib: `Sandbox/admission`'s `SupplyChainGate.Admit` folds this signature leg with the version leg applicatively, reporting both faults on a forged out-of-contract subject and folding `VerificationException` to `SupplyChainFault` on the typed rail.

[LOCAL_ADMISSION]:
- Supply-chain verification enters through `SigstoreVerifier.TryVerify*Async`; the injected `ITrustRootProvider` (file, TUF, or in-memory) selects the trust anchor once at composition, bundle loading through `SigstoreBundle.LoadAsync`/`Deserialize`, and identity expectation through `CertificateIdentity.ForGitHubActions` or a literal `CertificateIdentity`.
- `IFulcioClient`, `IRekorClient`, and `ITimestampAuthority` stay out of scope; the admit gate is verify-only.

[RAIL_LAW]:
- Package: `Sigstore`
- Owns: offline cosign/Sigstore bundle verification (keyless cert identity, transparency-log inclusion, RFC 3161 timestamp, SCT) and DSSE/in-toto SLSA attestation decode
- Accept: supply-chain admit gating, publisher-identity assertion, provenance extraction
- Reject: signing/issuance orchestration (the `IFulcioClient`/`IRekorClient` sign seams are present, the admit gate verify-only), version-range comparison (`NuGet.Versioning`), JWT request authentication (`IdentityModel`)
