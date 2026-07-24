# [TS_IAC_API_PULUMIVERSE_ACME]

`@pulumiverse/acme` owns CA-trusted certificate issuance outside a cluster: `Registration` binds an account key to a directory endpoint and `Certificate` drives order, challenge, issuance, and renewal as one lifecycle resource, with the directory URL the single `Provider` knob (`serverUrl`) so staging-versus-production is provider data, never a resource fork.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumiverse/acme`
- package: `@pulumiverse/acme` (Apache-2.0)
- module: `@pulumiverse/acme` → `{ Registration, Certificate, Provider, getServerUrl, getServerUrlOutput, types }`
- asset: the bridged ACME provider plugin; every `Certificate` output mirrors its arg through `Output<T>`, `privateKeyPem` state-encrypted and present only on the provider-minted posture
- runtime: `node` — DNS-01 needs the DNS provider API reachable; HTTP/TLS challenges need the target host answering on 80/443
- rail: cert — the CA-trusted lane beside `@pulumi/tls`'s self-signed lane

## [02]-[ACCOUNT_AND_ISSUANCE]

[REGISTRATION_SCOPE]: the account row — one per directory

`Registration` self-mints its account key from `accountKeyAlgorithm`/`accountKeyEcdsaCurve`/`accountKeyRsaBits`, or adopts a `tls.PrivateKey.privateKeyPem` through `accountKeyPem` to keep key mint in the one entropy owner; `externalAccountBinding` (`keyId`+`hmacBase64`) carries EAB where the CA requires it. `registrationUrl` and the resolved `accountKeyPem` are the outputs every `Certificate` consumes.

| [INDEX] | [SYMBOL]                                                             | [SHAPE_BOUNDARY]                                     |
| :-----: | :------------------------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `Registration`                                                       | `new Registration(name, args, opts?)`                |
|  [02]   | `RegistrationArgs.emailAddress`                                      | `Input<string>` — account contact                    |
|  [03]   | `accountKeyAlgorithm` / `accountKeyEcdsaCurve` / `accountKeyRsaBits` | self-mint policy; ECDSA floor per `Certs`            |
|  [04]   | `accountKeyPem`                                                      | adopt `tls.PrivateKey.privateKeyPem`, else self-mint |
|  [05]   | `externalAccountBinding`                                             | `{ keyId, hmacBase64 }` — EAB, Doppler-fed           |
|  [06]   | `ProviderArgs.serverUrl`                                             | sole provider field — the ACME directory URL         |
|  [07]   | `getServerUrl` / `getServerUrlOutput`                                | invoke mirror reading the directory URL              |

[CERTIFICATE_SCOPE]: the one lifecycle resource

Two mutually-exclusive key postures on one resource: provider-minted (`commonName`+`subjectAlternativeNames`+`keyType`, key lands in `privateKeyPem`) or CSR (`certificateRequestPem` from a `tls.CertRequest`, key never leaves the `tls` owner). `accountKeyPem` binds the account; exactly one challenge family activates.

DNS-01 rows are `dnsChallenges: Input<Input<CertificateDnsChallenge>[]>`, each `{ provider: Input<string>, config?: Input<{[k]: Input<string>}> }` — a lego provider slug and its credential map; non-DNS rows carry `httpChallenge {port?, proxyHeader?}`, `httpWebrootChallenge {directory}`, `httpMemcachedChallenge {hosts}`, `httpS3Challenge {s3Bucket}`, `tlsChallenge {port?}`.

| [INDEX] | [MEMBER]                                                                     | [SHAPE_MEANING]                                           |
| :-----: | :--------------------------------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | `accountKeyPem`                                                              | `Input<string>` (required); the `registration` bind       |
|  [02]   | `commonName` / `subjectAlternativeNames` / `keyType`                         | provider-minted posture; SANs add hostnames               |
|  [03]   | `certificateRequestPem`                                                      | CSR posture; `tls.CertRequest.certRequestPem`             |
|  [04]   | `dnsChallenges`                                                              | DNS-01 rows, one per lego provider (element type in lead) |
|  [05]   | `httpChallenge` / `httpWebrootChallenge` / `httpMemcachedChallenge`          | HTTP-01 challenge rows (shapes in lead)                   |
|  [06]   | `httpS3Challenge` / `tlsChallenge`                                           | S3-served + TLS-ALPN rows (shapes in lead)                |
|  [07]   | `recursiveNameservers` / `disableCompletePropagation`                        | DNS-01 propagation: resolver + skip-check                 |
|  [08]   | `preCheckDelay` / `propagationWait`                                          | DNS-01 propagation timing                                 |
|  [09]   | `minDaysRemaining` / `minDaysDynamic` / `useRenewalInfo`                     | fixed-days + lifetime-fraction threshold, ARI toggle      |
|  [10]   | `renewalInfoMaxSleep` / `renewalInfoIgnoreRetryAfter`                        | ARI max-sleep + retry-after override                      |
|  [11]   | `revokeCertificateOnDestroy` / `revokeCertificateReason`                     | revoke on teardown + reason                               |
|  [12]   | `deactivateAuthorizations`                                                   | deactivate pending authz on teardown                      |
|  [13]   | `preferredChain` / `profile` / `mustStaple` / `validityDays` / `certTimeout` | chain, CA profile, must-staple, validity, timeout         |
|  [14]   | `certificatePem` / `issuerPem` / `privateKeyPem`                             | issued material; `privateKeyPem` minted-only              |
|  [15]   | `certificateP12` (+`certificateP12Password`)                                 | PKCS#12 bundle + password                                 |
|  [16]   | `certificateDomain` / `certificateNotAfter` / `certificateNotBefore`         | issued identity: domain + validity bounds                 |
|  [17]   | `certificateSerial` / `certificateUrl`                                       | issued serial + order URL                                 |
|  [18]   | `renewalInfoWindowStart` / `renewalInfoWindowEnd`                            | ARI window bounds                                         |
|  [19]   | `renewalInfoWindowSelected`                                                  | ARI selected window                                       |
|  [20]   | `renewalInfoRetryAfter` / `renewalInfoExplanationUrl`                        | ARI retry-after + explanation URL                         |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Three cert lanes share one sink shape: `Certs.root`/`issue` (self-signed, mesh-internal), cert-manager CRDs (in-cluster ACME on the k8s arm), and this package (CA-trusted, cluster-external); a hostname served to browsers from the docker arm or bare metal issues here, and re-issuing an in-cluster cert-manager cert through this provider is the split-brain the lane split forbids.
- Renewal is this resource's own diff: inside `minDaysRemaining` (or `minDaysDynamic`, deriving the threshold as one-third the certificate lifetime and excluding `minDaysRemaining`, or the ARI-selected window under `useRenewalInfo`) the next `up` reissues as an `update` the drift fold surfaces — deleting a certificate to rotate it is the named defect.
- One `Registration` per directory per estate binds `serverUrl` as provider data: a staging directory for proof stacks, production for live ones, switched by provider construction, never resource edits; `revokeCertificateOnDestroy: true` where a torn-down stack must not leave a live cert.

[STACKING]:
- `@pulumi/tls`(`.api/pulumi-tls.md`): `tls.PrivateKey → tls.CertRequest.certRequestPem → Certificate.certificateRequestPem` keeps every private key in the one `tls` owner (CSR posture); the issued `certificatePem`/`issuerPem` rejoin that `tls.PrivateKey` at the sink, and the provider-minted posture is admitted only where a P12 bundle (`certificateP12`) is the consumer's required form.
- `@pulumiverse/doppler`(`.api/pulumiverse-doppler.md`): `getSecretsOutput(...).apply(r => r.map[KEY])` `Output<string>` feeds each `dnsChallenges[].config` credential entry and `externalAccountBinding.hmacBase64`, so challenge and EAB credentials stay Doppler-bound, never literals.
- `@pulumi/cloudflare`(`.api/pulumi-cloudflare.md`): `dnsChallenges: [{ provider: "cloudflare", config }]` writes the TXT authz into the `cloudflare.Zone` the traffic rows already manage; the non-DNS `httpChallenge`/`tlsChallenge`/webroot/S3/memcached rows serve hosts a DNS API cannot reach, and wildcard names are DNS-01-only.
- within-lib: the issued `certificatePem`/`issuerPem`/`privateKeyPem` triple sinks into the same `kubernetes.io/tls`-shaped `stringData` consumers the self-signed lane feeds, the ARI window and `minDaysRemaining` making rotation a drift-visible `update`.

[RAIL_LAW]:
- Package: `@pulumiverse/acme`
- Owns: CA-trusted certificate issuance and renewal outside a cluster — account registration, challenge orchestration, ARI-windowed rotation
- Accept: CSR-mode issuance over the `tls` chain, DNS-01 `dnsChallenges` with Doppler-bound config, `minDaysRemaining`/`useRenewalInfo` rotation windows, `serverUrl` as the staging/production switch, revoke-on-destroy for ephemeral stacks
- Reject: self-signed duplication of the `Certs` lane, in-cluster issuance competing with cert-manager, challenge credentials as literals, private keys minted here when a CSR keeps them in the `tls` owner, cert rotation by resource deletion
