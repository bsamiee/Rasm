# [TS_SECURITY_API_SIMPLEWEBAUTHN_SERVER]

`@simplewebauthn/server` owns the relying-party half of the passkey ceremony: it mints ceremony options carrying the challenge and policy, then verifies the authenticator's signed response into a discriminated verdict.

One parameterized pattern spans both ceremonies at two phases; registration verification folds the attestation-format verifier internally on the decoded `fmt`, so trust enters as policy rather than a consumer-written switch.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the RP boundary contract — wire shapes crossing to and from the browser, the verdict carriers, and the trust vocabulary

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :--------------------------------------- | :------------ | :-------------------------------------------------------------- |
|  [01]   | `WebAuthnCredential`                     | record        | persisted passkey: `id`, `publicKey`, `counter`, `transports`   |
|  [02]   | `VerifiedRegistrationResponse`           | union         | `verified` discriminates; the true arm holds `registrationInfo` |
|  [03]   | `VerifiedAuthenticationResponse`         | record        | `authenticationInfo.newCounter` carries the clone signal        |
|  [04]   | `RegistrationResponseJSON`               | wire ingress  | attestation response the browser posts back                     |
|  [05]   | `AuthenticationResponseJSON`             | wire ingress  | assertion response the browser posts back                       |
|  [06]   | `PublicKeyCredentialCreationOptionsJSON` | wire egress   | registration options the browser consumes                       |
|  [07]   | `PublicKeyCredentialRequestOptionsJSON`  | wire egress   | authentication options the browser consumes                     |
|  [08]   | `AttestationFormat`                      | union         | verifier key the registration fold dispatches on                |
|  [09]   | `RootCertIdentifier`                     | union         | `AttestationFormat \| 'mds'` trust-anchor key                   |
|  [10]   | `MetadataStatement`                      | record        | FIDO MDS statement resolved by AAGUID                           |

[SCALAR]: `Base64URLString` `COSEAlgorithmIdentifier` `CredentialDeviceType` `AuthenticatorTransportFuture` `UserVerificationRequirement`
[POLICY]: `AuthenticatorSelectionCriteria` `AuthenticationExtensionsClientInputs` `AuthenticationExtensionsAuthenticatorOutputs` `VerificationMode`
[OPTS]: `GenerateRegistrationOptionsOpts` `GenerateAuthenticationOptionsOpts` `VerifyRegistrationResponseOpts` `VerifyAuthenticationResponseOpts` `AttestationFormatVerifierOpts`

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the 2x2 ceremony grid — `{registration, authentication}` by `{mint, verify}` — each taking one options object and returning a `Promise`

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                                            |
| :-----: | :----------------------------------------------------------------- | :------- | :------------------------------------------------------ |
|  [01]   | `generateRegistrationOptions(GenerateRegistrationOptionsOpts)`     | static   | mints the creation options and the challenge            |
|  [02]   | `verifyRegistrationResponse(VerifyRegistrationResponseOpts)`       | static   | verdict union; `fmt` selects the attestation verifier   |
|  [03]   | `generateAuthenticationOptions(GenerateAuthenticationOptionsOpts)` | static   | mints the request options over `allowCredentials`       |
|  [04]   | `verifyAuthenticationResponse(VerifyAuthenticationResponseOpts)`   | static   | verdict carrying `newCounter`, the clone signal         |
|  [05]   | `supportedCOSEAlgorithmIdentifiers`                                | property | default COSE allow-list `supportedAlgorithmIDs` narrows |

- `verifyRegistrationResponse`/`verifyAuthenticationResponse`: `expectedChallenge` accepts a `(challenge) => boolean | Promise<boolean>` resolver, so the session store consumes the challenge inside verification.
- `generateRegistrationOptions`: `challenge` and `userID` default to WebCrypto random bytes; supplying either routes generation to the caller's RNG owner.

[ENTRYPOINT_SCOPE]: trust anchors, MDS metadata, and the `./helpers` primitives the ceremonies fold — the crypto family returns a `Promise`, the parse and convert families resolve synchronously

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                                              |
| :-----: | :---------------------------------------------------------------- | :------- | :-------------------------------------------------------- |
|  [01]   | `SettingsService.setRootCertificates({identifier, certificates})` | instance | registers PEM or raw anchors per format or `'mds'`        |
|  [02]   | `SettingsService.getRootCertificates({identifier})`               | instance | reads the anchors a format's certificate path walks       |
|  [03]   | `MetadataService.initialize(opts)`                                | instance | loads MDS servers or local statements; `'strict'` default |
|  [04]   | `MetadataService.getStatement(string \| Uint8Array)`              | instance | resolves an AAGUID to its `MetadataStatement`             |
|  [05]   | `BaseMetadataService`                                             | class    | own-instance MDS service beside the module singleton      |
|  [06]   | `generateChallenge()`                                             | static   | WebCrypto challenge bytes                                 |
|  [07]   | `generateUserID()`                                                | static   | WebCrypto user-handle bytes                               |
|  [08]   | `verifySignature({signature, data, credentialPublicKey})`         | static   | raw check; `x509Certificate`/`hashAlgorithm` alternate    |
|  [09]   | `validateCertificatePath(string[], string[])`                     | static   | walks an x5c PEM chain against the trust anchors          |
|  [10]   | `toHash(Uint8Array \| string, COSEALG)`                           | static   | COSE-keyed digest over client data and authenticator data |
|  [11]   | `verifyMDSBlob(string)`                                           | static   | verifies and decodes a FIDO MDS JWT blob                  |

[PARSE]: `parseAuthenticatorData` `decodeAttestationObject` `decodeClientDataJSON` `decodeCredentialPublicKey`
[ISO]: `isoBase64URL` `isoCBOR` `isoCrypto` `isoUint8Array`
[COSE]: `COSEALG` `COSECRV` `COSEKEYS` `COSEKTY` `COSEPublicKey` `isCOSEAlg` `isCOSECrv` `isCOSEKty` `isCOSEPublicKeyEC2` `isCOSEPublicKeyOKP` `isCOSEPublicKeyRSA`
[CERT]: `getCertificateInfo` `isCertRevoked` `convertCertBufferToPEM` `convertCOSEtoPKCS` `convertAAGUIDToString`

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every ceremony folds mint then verify around one challenge the RP stores between the phases and consumes once through the `expectedChallenge` resolver.
- `verifyRegistrationResponse` selects the attestation verifier on the decoded `fmt`; `attestationType`, `supportedAlgorithmIDs`, and the `SettingsService` anchors are the whole trust knob set.
- `newCounter` is the replay defense — a counter failing to exceed the journalled value marks a cloned authenticator.
- Passkey material is public-key crypto, so credential and challenge stay typed boundary values rather than `Redacted` carriers.

[STACKING]:
- `@simplewebauthn/browser`(`.api/simplewebauthn-browser.md`): `generateRegistrationOptions`/`generateAuthenticationOptions` output feeds `startRegistration`/`startAuthentication` unchanged, and the returned `RegistrationResponseJSON`/`AuthenticationResponseJSON` returns to `verifyRegistrationResponse`/`verifyAuthenticationResponse`. `expectedChallenge` resolves the challenge the browser never sees, while `browserSupportsWebAuthn` and `WebAuthnAbortService` gate and bound a ceremony this half never observes — one owner schema decodes both crossings.
- `effect`(`.api/effect.md`): `Effect.tryPromise` lifts each ceremony into the typed rail with a `Schema.TaggedError` catch; `Schema.Struct` decodes the response JSON before `verify*` and encodes the options after `generate*`; `effect/Match` on the `verified` discriminant extracts the credential only on the true arm.
- `@noble/hashes`(`.api/noble-hashes.md`): supplying `opts.challenge`/`opts.userID` routes challenge and user-handle bytes to `sign/crypto`'s `Entropy`-port RNG, keeping one RNG owner across the folder.
- `authn/webauthn`: each verified `WebAuthnCredential` and `newCounter` writes through the `IdentityJournal` port `session/token` declares, so the counter check reads the journal the same fold updates.

[LOCAL_ADMISSION]:
- `authn/` admits it alone; a `runtime:browser` composition takes `@simplewebauthn/browser` instead.
- `SettingsService` and `MetadataService` are module singletons — enterprise and direct attestation configure them once at composition, never per request.
