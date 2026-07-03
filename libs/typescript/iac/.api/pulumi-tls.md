# [TS_IAC_API_PULUMI_TLS]

`@pulumi/tls` is the certificate/key material owner for the `kube/traffic` rows: `PrivateKey` mints keys, `CertRequest` builds a CSR, `SelfSignedCert`/`LocallySignedCert` sign leaf and CA-chained certs, and `getCertificate`/`getPublicKey` read external material. `iac` composes it as ONE cert-chain pipeline — `PrivateKey → CertRequest → {self-signed | CA-signed}` driven by a single cert-profile value, with `allowedUses` as a bounded key-usage vocabulary and `readyForRenewal` as the rotation trigger the drift fold watches. Private keys cross boundaries only as `Redacted`/secret `Output`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/tls`
- package: `@pulumi/tls`
- module: `@pulumi/tls`
- installed: `5.5.0`
- license: `Apache-2.0`
- asset: provider-tracked key + certificate material (private keys, CSRs, self-signed/CA-signed certs, cert/pubkey reads)
- runtime: `node` — Terraform-bridge provider plugin auto-downloads on first resource registration; key material persists in stack state
- rail: fabric

## [02]-[RESOURCE_SURFACE]

Every resource extends `pulumi.CustomResource` with `static get`/`isInstance` + `constructor(name, args, opts?)`. Private-key PEM outputs are state-encrypted sensitive. `subject` is a structured DN (`commonName`/`organization`/…); `dnsNames`/`ipAddresses`/`uris` are the SAN lists.

[PUBLIC_TYPE_SCOPE]: resource roster
- rail: fabric

| [INDEX] | [SYMBOL]            | [REQUIRED ARGS]                                              | [KEY OUTPUTS]                                          |
| :-----: | :------------------ | :---------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `PrivateKey`        | `algorithm` (`RSA`\|`ECDSA`\|`ED25519`)                     | `privateKeyPem`, `privateKeyPemPkcs8`, `privateKeyOpenssh`, `publicKeyPem`, `publicKeyOpenssh`, `publicKeyFingerprintMd5`/`Sha256` |
|  [02]   | `CertRequest`       | `privateKeyPem`                                             | `certRequestPem`                                      |
|  [03]   | `SelfSignedCert`    | `allowedUses`, `validityPeriodHours`, `privateKeyPem`      | `certPem`, `validityStartTime`/`EndTime`, `readyForRenewal` |
|  [04]   | `LocallySignedCert` | `allowedUses`, `validityPeriodHours`, `certRequestPem`, `caPrivateKeyPem`, `caCertPem` | `certPem`, `validityStartTime`/`EndTime`, `readyForRenewal` |
|  [05]   | `Provider`          | —                                                          | explicit provider instance                            |

[PUBLIC_TYPE_SCOPE]: data sources (external material)
- rail: fabric

| [INDEX] | [SURFACE]                                              | [MODE]        | [NOTE]                                              |
| :-----: | :----------------------------------------------------- | :------------ | :------------------------------------------------- |
|  [01]   | `getCertificate({url \| content, verifyChain?})`      | `Promise`     | read a served/PEM cert chain → `certificates[]`    |
|  [02]   | `getCertificateOutput(...)`                            | `Output`      | Input-accepting mirror of `getCertificate`         |
|  [03]   | `getPublicKey({privateKeyPem \| privateKeyOpenssh})`  | `Promise`     | derive public key + fingerprints from a private key |
|  [04]   | `getPublicKeyOutput(...)`                             | `Output`      | Input-accepting mirror of `getPublicKey`           |

## [03]-[CERT_CHAIN]

ONE pipeline owns certificate issuance; the four resources are its stages, not four recipes.

[PATTERN]: issuance pipeline — cert-profile value → chain
- `PrivateKey(algorithm, rsaBits|ecdsaCurve)` mints the key.
- Leaf via CA: `CertRequest(privateKeyPem, subject, SANs)` → `LocallySignedCert(certRequestPem, caPrivateKeyPem, caCertPem, allowedUses, validityPeriodHours)`.
- Standalone: `SelfSignedCert(privateKeyPem, subject, SANs, allowedUses, validityPeriodHours)` collapses request + sign into one resource.
- The self-signed-vs-CA-signed choice is a `Match` dispatch arm over the profile's `issuer` tag; a CA root is one `SelfSignedCert({ isCaCertificate: true, maxPathLength })` whose `privateKeyPem`/`certPem` sign every leaf.

[PATTERN]: `allowedUses` key-usage vocabulary
- A bounded string set (`server_auth`, `client_auth`, `digital_signature`, `key_encipherment`, `key_agreement`, `cert_signing`, `crl_signing`, `code_signing`, …) → model as a `Schema.Literal` union, not free strings. `isCaCertificate`/`maxPathLength`/`setAuthorityKeyId`/`setSubjectKeyId` shape CA-chain semantics.

[PATTERN]: renewal trigger — ONE rotation signal
- `validityPeriodHours` sets lifetime; `earlyRenewalHours` moves the renewal window forward; `readyForRenewal: Output<boolean>` flips when inside the window. The `previewRefresh` drift fold watches `readyForRenewal`/`validityEndTime` to schedule reissue — the cert analog of `random`'s `keepers`.

## [04]-[INTEGRATION]

Key + cert PEMs stack into the kube TLS + traffic rows; `effect` owns the profile shape, the usage vocabulary, and the drift signal.

[RAIL]: `tls → effect + sibling providers`

| [INDEX] | [TLS SEAM]                                | [STACKS WITH]                                   | [COMPOSED RAIL]                                             |
| :-----: | :---------------------------------------- | :---------------------------------------------- | :--------------------------------------------------------- |
|  [01]   | cert-profile args                          | `Schema.Struct` + `Schema.Literal` (allowedUses) | ONE decoded `CertProfile` value → the chain args           |
|  [02]   | `PrivateKey.privateKeyPem` + `Cert.certPem` | `@pulumi/kubernetes` `Secret` (`kubernetes.io/tls`) | `stringData: { "tls.crt": certPem, "tls.key": privateKeyPem }` → `kube/traffic` ingress |
|  [03]   | CA root `SelfSignedCert` (`isCaCertificate`) | N `LocallySignedCert` leaves                 | one CA signs the mesh; mTLS between `kube` workloads        |
|  [04]   | `PrivateKey.privateKeyPem` (secret)        | `@pulumiverse/doppler` `Secret` / `Redacted`    | CA key stored canonically; wrapped `Redacted` in outputs   |
|  [05]   | `readyForRenewal` / `validityEndTime`      | `previewRefresh` drift fold (`OpType`)          | rotation window → reissue op in the drift receipt           |
|  [06]   | `getCertificateOutput({url})`              | `Output` graph                                  | pin/trust an external endpoint's chain at deploy time       |

```ts contract
// iac/kube/traffic — cert-profile → chain → TLS secret, one pipeline
const key = new tls.PrivateKey("svc", { algorithm: profile.algorithm, ecdsaCurve: profile.curve }, { parent })
const cert = new tls.LocallySignedCert("svc", {
  certRequestPem: new tls.CertRequest("svc", { privateKeyPem: key.privateKeyPem, subject, dnsNames }, { parent }).certRequestPem,
  caPrivateKeyPem: ca.privateKeyPem, caCertPem: ca.certPem,
  allowedUses: profile.allowedUses,               // Schema.Literal union
  validityPeriodHours: profile.validityHours, earlyRenewalHours: profile.renewBeforeHours,
}, { parent })
new k8s.core.v1.Secret("svc-tls", {
  type: "kubernetes.io/tls",
  stringData: { "tls.crt": cert.certPem, "tls.key": key.privateKeyPem },
}, { parent })
```

## [05]-[IMPLEMENTATION_LAW]

[MATERIAL_TOPOLOGY]:
- `privateKeyPem`/`privateKeyPemPkcs8`/`privateKeyOpenssh` are sensitive; cross a boundary only as secret `Output`/`Redacted`. Publish `publicKeyPem`/fingerprints freely.
- Prefer `ECDSA` (P-256) or `ED25519` over `RSA` unless a consumer mandates RSA; set `rsaBits ≥ 2048` when RSA is required.
- Reissue by widening the renewal window (`earlyRenewalHours`) or bumping `validityPeriodHours`, watched via `readyForRenewal` — never by deleting the cert.

[RAIL_LAW]:
- Package: `@pulumi/tls`
- Owns: private keys, CSRs, self-signed + CA-signed certificates, external cert/pubkey reads
- Accept: one `Schema`-decoded cert profile driving the chain; `allowedUses` as a `Schema.Literal` union; `readyForRenewal` as the drift-fold rotation signal; the `*Output` mirror when an arg is an `Input`
- Reject: private-key PEM as a plain output; free-string `allowedUses`; separate self-signed and CA-signed code paths where one `issuer`-tag dispatch owns both; the eager `Promise` data source where the arg is an `Output`
