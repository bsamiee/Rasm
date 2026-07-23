# [TS_IAC_API_PULUMIVERSE_ACME]

`@pulumiverse/acme` is the CA-trusted issuance lane: two resources own the whole ACME protocol — `Registration` binds an account key to a directory endpoint, `Certificate` drives order, challenge, issuance, and renewal as one lifecycle resource — with the directory URL as the single `Provider` knob (`serverUrl`), so staging-versus-production is provider data, never a resource fork. It completes the cert axis the folder already carries: `@pulumi/tls` mints self-signed material for the mesh-internal lane (`Certs.root`/`issue`), cert-manager owns in-cluster ACME on the k8s arm, and this package owns CA-trusted certs OUTSIDE a cluster — the docker arm's edge, bare-metal endpoints, any hostname a browser must trust without a cluster to run cert-manager in. Issuance is DNS-01 by default through `dnsChallenges` rows (the lego provider vocabulary — `cloudflare` against the zone the traffic rows already manage), the issued triple sinks into the same `kubernetes.io/tls`-shaped consumers the self-signed lane feeds, and `minDaysRemaining` plus the ARI window outputs make rotation a drift-visible `update` on this one resource.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumiverse/acme`
- package: `@pulumiverse/acme` (Apache-2.0)
- import: `@pulumiverse/acme` → `{ Registration, Certificate, Provider, getServerUrl, getServerUrlOutput, types }`
- owner: `iac`
- rail: fabric / cert (the CA-trusted lane beside `@pulumi/tls`'s self-signed lane)
- runtime: Node deploy-host; DNS-01 needs the DNS provider API reachable, HTTP/TLS challenges need the target host to answer on 80/443
- depends-on: `@pulumi/pulumi`; composes `@pulumi/tls` (CSR mode), `@pulumi/cloudflare` (the challenged zone), `@pulumiverse/doppler` (challenge credentials)
- capability: ACME account registration (EAB-capable), certificate order/issue/renew as one resource lifecycle, DNS-01/HTTP-01/TLS-ALPN challenge rows, CSR-mode or provider-minted keys, ARI-driven renewal windows, revoke-on-destroy
- abi-note: every `Certificate` output mirrors its arg resolved through `Output<T>`; the issued material rides `certificatePem`/`issuerPem`/`privateKeyPem` with the private key state-encrypted and present only when the provider minted the key

## [02]-[ACCOUNT_AND_ISSUANCE]

[REGISTRATION_SCOPE]: the account row — one per directory
- rail: cert
- `Registration` self-mints its account key when given `accountKeyAlgorithm`/`accountKeyEcdsaCurve`/`accountKeyRsaBits`, or adopts one through `accountKeyPem` (a `tls.PrivateKey.privateKeyPem` output, keeping key mint in the one entropy owner). `externalAccountBinding` (`keyId` + `hmacBase64`) is the EAB row for CAs that require it. `registrationUrl` and the resolved `accountKeyPem` are the outputs every `Certificate` consumes.

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
- rail: cert
- Two key postures on one resource: provider-minted (`commonName` + `subjectAlternativeNames` + `keyType`, the key lands in `privateKeyPem`) or CSR mode (`certificateRequestPem` from a `tls.CertRequest`, the key never leaves the `tls` owner), mutually exclusive. `accountKeyPem` is the required account bind; exactly one challenge family activates per certificate. DNS-01 rows are `dnsChallenges: Input<Input<CertificateDnsChallenge>[]>`, each element `{ provider: Input<string>, config?: Input<{[k]: Input<string>}> }` — the lego provider slug plus its credential map. Non-DNS rows carry `httpChallenge` `{port?, proxyHeader?}`, `httpWebrootChallenge` `{directory}`, `httpMemcachedChallenge` `{hosts}`, `httpS3Challenge` `{s3Bucket}`, `tlsChallenge` `{port?}`.

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

[ISSUANCE_TOPOLOGY]:
- lane law: three cert lanes, one sink shape — `Certs.root`/`issue` (self-signed, mesh-internal), cert-manager CRDs (in-cluster ACME on the k8s arm), and this package (CA-trusted, cluster-external); a hostname served to browsers from the docker arm or bare metal issues here, and duplicating an in-cluster cert-manager issuance through this provider is the split-brain the lane split forbids.
- CSR law: prefer the CSR posture — `tls.PrivateKey → tls.CertRequest → Certificate.certificateRequestPem` keeps every private key in the one `tls` owner (`.api/pulumi-tls.md`) and the issued `certificatePem`/`issuerPem` join that key at the sink; the minted posture is admitted only where a P12 bundle (`certificateP12`) is the consumer's required form.
- challenge law: DNS-01 through `dnsChallenges: [{ provider: "cloudflare", config }]` against the zone the traffic rows manage — the config map binds Doppler fan-in reads (`.api/pulumiverse-doppler.md`), never literals, and the HTTP/TLS/webroot/S3/memcached rows exist for hosts a DNS API cannot reach; wildcard names are DNS-01-only.
- rotation law: renewal is this resource's own diff — inside `minDaysRemaining` (or `minDaysDynamic`, which derives the threshold as one-third the certificate lifetime and excludes `minDaysRemaining`, or the ARI-selected window under `useRenewalInfo`) the next `up` reissues as an `update` step the drift fold surfaces, the exact analog of `earlyRenewalHours`/`readyForRenewal` on the self-signed lane; deleting a certificate to rotate it is the same named defect.
- account law: one `Registration` per directory per estate, `serverUrl` as provider data — a staging directory for proof stacks, production for live ones, switched by provider construction, never by resource edits; `revokeCertificateOnDestroy: true` wherever a torn-down stack must not leave a live cert.

[RAIL_LAW]:
- Package: `@pulumiverse/acme`
- Owns: CA-trusted certificate issuance and renewal outside a cluster — account registration, challenge orchestration, ARI-windowed rotation
- Accept: CSR-mode issuance over the `tls` chain, DNS-01 `dnsChallenges` with Doppler-bound config, `minDaysRemaining`/`useRenewalInfo` rotation windows, `serverUrl` as the staging/production switch, revoke-on-destroy for ephemeral stacks
- Reject: self-signed duplication of the `Certs` lane, in-cluster issuance competing with cert-manager, challenge credentials as literals, private keys minted here when a CSR keeps them in the `tls` owner, cert rotation by resource deletion
