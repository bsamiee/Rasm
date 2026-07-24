# [TS_IAC_API_PULUMI_TLS]

`@pulumi/tls` mints provider-tracked certificate and key material: private keys, CSRs, self-signed and CA-signed certificates, and data-source reads of external chains and public keys.

`iac` folds it into ONE cert-chain pipeline — an `issuer`-tag `Match` arm signs each leaf self-signed or CA-chained — driven by one decoded cert-profile; `allowedUses` bounds key usage, `readyForRenewal` triggers the drift fold's reissue, and a private key crosses a boundary only as secret `Output`/`Redacted`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/tls`
- package: `@pulumi/tls` (Apache-2.0)
- module: `@pulumi/tls` — flat resource-class and data-source exports over the Terraform-bridge provider plugin
- runtime: `node`; the provider plugin auto-downloads on first resource registration and key material persists in stack state
- rail: fabric

## [02]-[RESOURCE_SURFACE]

Every resource extends `pulumi.CustomResource` with `static get`/`isInstance` and `constructor(name, args, opts?)`; private-key and certificate PEM surface as `Output<string>`, the private-key forms state-encrypted sensitive. `subject` is a structured DN, `dnsNames`/`ipAddresses`/`uris` the SAN lists, and `SelfSignedCert`/`LocallySignedCert` share `certPem`, `validityStartTime`/`validityEndTime`, and `readyForRenewal`.

[PUBLIC_TYPE_SCOPE]: resource roster

| [INDEX] | [SYMBOL]            | [REQUIRED_ARGS]                                                                        |
| :-----: | :------------------ | :------------------------------------------------------------------------------------- |
|  [01]   | `PrivateKey`        | `algorithm` (`RSA`/`ECDSA`/`ED25519`); `rsaBits`/`ecdsaCurve`                          |
|  [02]   | `CertRequest`       | `privateKeyPem`                                                                        |
|  [03]   | `SelfSignedCert`    | `allowedUses`, `validityPeriodHours`, `privateKeyPem`                                  |
|  [04]   | `LocallySignedCert` | `allowedUses`, `validityPeriodHours`, `certRequestPem`, `caPrivateKeyPem`, `caCertPem` |
|  [05]   | `Provider`          | —                                                                                      |

[PRIVATE_KEY]: `privateKeyPem` (secret) `privateKeyPemPkcs8` `privateKeyOpenssh` `publicKeyPem` `publicKeyOpenssh` `publicKeyFingerprintMd5` `publicKeyFingerprintSha256`
[CERT_REQUEST]: `certRequestPem`

[PUBLIC_TYPE_SCOPE]: data sources

| [INDEX] | [SURFACE]                                          | [SHAPE]   | [CAPABILITY]                               |
| :-----: | :------------------------------------------------- | :-------- | :----------------------------------------- |
|  [01]   | `getCertificate(url \| content, verifyChain?)`     | `Promise` | read a served/PEM chain → `certificates[]` |
|  [02]   | `getCertificateOutput(...)`                        | `Output`  | Input-accepting mirror                     |
|  [03]   | `getPublicKey(privateKeyPem \| privateKeyOpenssh)` | `Promise` | derive public key + fingerprints           |
|  [04]   | `getPublicKeyOutput(...)`                          | `Output`  | Input-accepting mirror                     |

## [03]-[CERT_CHAIN]

ONE pipeline owns issuance; the four resources are its stages, not four recipes.

[PATTERN]: issuance — cert-profile value → chain
- `PrivateKey(algorithm, rsaBits|ecdsaCurve)` mints the key.
- CA leaf: `CertRequest(privateKeyPem, subject, SANs)` → `LocallySignedCert(certRequestPem, caPrivateKeyPem, caCertPem, allowedUses, validityPeriodHours)`.
- Standalone: `SelfSignedCert(privateKeyPem, subject, SANs, allowedUses, validityPeriodHours)` collapses request and sign into one resource.
- A `Match` arm over the profile's `issuer` tag picks self-signed vs CA-signed; a CA root is one `SelfSignedCert({ isCaCertificate: true, maxPathLength })` whose key and cert sign every leaf.

[PATTERN]: `allowedUses` key-usage vocabulary
- A bounded string set (`server_auth`, `client_auth`, `digital_signature`, `key_encipherment`, `key_agreement`, `cert_signing`, `crl_signing`, `code_signing`) models as a `Schema.Literal` union, never free strings; `isCaCertificate`/`maxPathLength`/`setAuthorityKeyId`/`setSubjectKeyId` shape CA-chain semantics.

[PATTERN]: renewal trigger
- `validityPeriodHours` sets lifetime, `earlyRenewalHours` moves the window forward, and `readyForRenewal: Output<boolean>` flips inside it; the `previewRefresh` drift fold watches `readyForRenewal`/`validityEndTime` to schedule reissue — the cert analog of `@pulumi/random`'s `keepers`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `privateKeyPem`/`privateKeyPemPkcs8`/`privateKeyOpenssh` are sensitive and cross a boundary only as secret `Output`/`Redacted`; publish `publicKeyPem` and fingerprints freely.
- Mint `ECDSA` (P-256) or `ED25519` keys; `RSA` binds only where a consumer mandates it, at `rsaBits ≥ 2048`.
- Reissue by widening `earlyRenewalHours` or bumping `validityPeriodHours` under `readyForRenewal`, never by deleting the cert.

[STACKING]:
- `@pulumi/kubernetes`(`.api/pulumi-kubernetes.md`): `PrivateKey.privateKeyPem` + `SelfSignedCert.certPem`/`LocallySignedCert.certPem` feed a `core.v1.Secret.stringData` of type `kubernetes.io/tls`, serving the kube/traffic rows.
- `@pulumiverse/doppler`(`.api/pulumiverse-doppler.md`): a CA `PrivateKey.privateKeyPem` stores canonically as a `Secret` value wrapped `Redacted`.
- `effect`(`libs/typescript/.api/effect.md`): a `Schema.Struct` cert-profile with `allowedUses` a `Schema.Literal` union decodes into the chain args, and `readyForRenewal`/`validityEndTime` feed the `previewRefresh` drift fold's reissue op.
- within-lib: `iac` folds the four resources into ONE pipeline — one CA-root `SelfSignedCert({ isCaCertificate: true })` signs N `LocallySignedCert` leaves for kube mTLS, an `issuer`-tag `Match` arm dispatches self-signed vs CA-signed, and `getCertificateOutput({ url })` pins an external chain at deploy.

[LOCAL_ADMISSION]:
- Admitted wherever certificate or key material must persist and diff in state and rotate under audit; an ad-hoc openssl invocation or inline PEM literal is rejected for that role.

[RAIL_LAW]:
- Package: `@pulumi/tls`
- Owns: private keys, CSRs, self-signed and CA-signed certificates, external certificate and public-key reads
- Accept: one `Schema`-decoded cert profile driving the chain; `allowedUses` as a `Schema.Literal` union; `readyForRenewal` as the drift-fold rotation signal; the `*Output` mirror when an arg is an `Input`
- Reject: a private-key PEM as a plain output; free-string `allowedUses`; separate self-signed and CA-signed code paths where one `issuer`-tag dispatch owns both; the eager `Promise` data source where the arg is an `Output`
