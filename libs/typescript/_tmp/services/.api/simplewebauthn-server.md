# [API_CATALOGUE] @simplewebauthn/server

`@simplewebauthn/server` owns the server side of the WebAuthn/passkey ceremony: `generateRegistrationOptions`/`generateAuthenticationOptions` mint the JSON challenge options the browser passes to `navigator.credentials.create()`/`.get()`, and `verifyRegistrationResponse`/`verifyAuthenticationResponse` verify the signed attestation/assertion the browser returns — all four `async`, all four resolving to typed result objects. `MetadataService` fetches and caches FIDO MDS BLOBs for AAGUID trust policy; `SettingsService` registers root certificates per attestation format. A second entry, `@simplewebauthn/server/helpers`, exposes the ceremony primitives directly — CSPRNG challenge/user-id generation, COSE key decode, authenticator-data parse, signature verify, X.509 chain validation, and the isomorphic base64url/CBOR/crypto/Uint8Array toolkit — so advanced flows compose the library's own primitives instead of reinventing CBOR or signature math. In `services` it is the WebAuthn arm of `security/auth.md`'s one `Authn` `Effect.Service`: each async function is lifted through one `Effect.tryPromise` (rejection → `AuthFault`) inside the `AuthCommand.$match` dispatch, sitting beside the `otplib` TOTP arm; `WebAuthnCredential` is the stored row persisted over the `@effect/sql-pg` `SqlClient`, and `authenticationInfo.newCounter` is written back after every assertion to defeat replay. The browser ceremony forwards its result over the `interchange` `CommandGateway`; a browser-side verifier is the named defect.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@simplewebauthn/server`
- package: `@simplewebauthn/server` (13.3.2, MIT; © Matthew Miller / MasterKale)
- module format: ESM `exports` map — `.` → `esm/index.d.ts` (the barrel), `./helpers` → `esm/helpers/index.d.ts` (the primitives subpath); `.d.ts` inferred beside each `.js`, self-typed, no `@types`
- reflected: TSDECL — `node_modules/@simplewebauthn/server/esm/{index,helpers/index,types/index}.d.ts`
- runtime target: runtime-neutral / isomorphic — the `helpers/iso` layer abstracts crypto/CBOR/base64url so the package runs on Node, Deno, Bun, and Workers; verification is WebCrypto-backed
- ABI: `Uint8Array_` (= `ReturnType<Uint8Array['slice']>`) bridges the TS 5.6/5.7 `Uint8Array<ArrayBuffer>` generic split; `Base64URLString` is a nominal `string` brand for wire values; every DOM WebAuthn type is re-exported from a bundled `dom.d.ts` (the language's own lib is stale)
- consumer: `security/auth.md#VERIFIER` — the WebAuthn `AuthCommand` arms (`BeginWebauthnRegistration`/`VerifyWebauthnRegistration`/`VerifyWebauthnAuthentication`) under the `Authn` `Effect.Service`, `WebAuthnCredential` as the `@effect/sql-pg` stored row, the `newCounter` replay write-back
- rail: auth / webauthn

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: wire model family — registration
- rail: auth

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]       | [CAPABILITY]                                                             |
| :-----: | :--------------------------------------- | :------------------ | :---------------------------------------------------------------------- |
|  [01]   | `PublicKeyCredentialCreationOptionsJSON` | interface           | challenge options → browser `create()`; `rp`/`user`/`challenge`/`pubKeyCredParams`/`excludeCredentials`/`hints`/`attestation` |
|  [02]   | `RegistrationResponseJSON`               | interface           | browser attestation → server: `id`/`rawId`/`response`/`clientExtensionResults`/`type` |
|  [03]   | `AuthenticatorAttestationResponseJSON`   | interface           | inner: `clientDataJSON`/`attestationObject`/`authenticatorData?`/`publicKey?`/`transports?` |
|  [04]   | `VerifiedRegistrationResponse`           | discriminated union | `{verified:false}` \| `{verified:true; registrationInfo}` where `registrationInfo` = `{credential: WebAuthnCredential, fmt, aaguid, credentialDeviceType, credentialBackedUp, userVerified, attestationObject, origin, rpID?}` |
|  [05]   | `PublicKeyCredentialUserEntityJSON` / `PublicKeyCredentialDescriptorJSON` | interface | JSON user handle and credential descriptor (`id: Base64URLString`, `transports?`) |
|  [06]   | `GenerateRegistrationOptionsOpts` / `VerifyRegistrationResponseOpts` | type alias | `Parameters<typeof …>[0]` — the option-object shapes as named types |

[PUBLIC_TYPE_SCOPE]: wire model family — authentication
- rail: auth

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [CAPABILITY]                                                                    |
| :-----: | :-------------------------------------- | :------------ | :----------------------------------------------------------------------------- |
|  [01]   | `PublicKeyCredentialRequestOptionsJSON` | interface     | challenge options → browser `get()`; `challenge`/`rpId`/`allowCredentials`/`userVerification`/`hints` |
|  [02]   | `AuthenticationResponseJSON`            | interface     | browser assertion → server: `id`/`rawId`/`response`/`clientExtensionResults`/`type` |
|  [03]   | `AuthenticatorAssertionResponseJSON`    | interface     | inner: `clientDataJSON`/`authenticatorData`/`signature`/`userHandle?`          |
|  [04]   | `VerifiedAuthenticationResponse`        | interface     | `{verified: boolean; authenticationInfo}` where info = `{credentialID, newCounter, userVerified, credentialDeviceType, credentialBackedUp, origin, rpID, authenticatorExtensionResults?}` |
|  [05]   | `GenerateAuthenticationOptionsOpts` / `VerifyAuthenticationResponseOpts` | type alias | `Parameters<typeof …>[0]` option shapes                          |

[PUBLIC_TYPE_SCOPE]: credential and attestation vocabulary
- rail: auth

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]    | [CAPABILITY]                                                                        |
| :-----: | :----------------------------- | :--------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `WebAuthnCredential`           | type alias       | the stored credential row — `{id: Base64URLString, publicKey: Uint8Array_, counter: number, transports?}` |
|  [02]   | `Base64URLString`              | type alias       | nominal `string` brand for base64url wire values                                    |
|  [03]   | `Uint8Array_`                  | type alias       | `ReturnType<Uint8Array['slice']>` — TS 5.6/5.7 generic bridge                       |
|  [04]   | `CredentialDeviceType`         | union            | `'singleDevice' \| 'multiDevice'` — passkey backup-eligibility signal               |
|  [05]   | `AuthenticatorTransportFuture` | union            | `'ble'\|'cable'\|'hybrid'\|'internal'\|'nfc'\|'smart-card'\|'usb'`                   |
|  [06]   | `AttestationFormat`            | union            | `'fido-u2f'\|'packed'\|'android-safetynet'\|'android-key'\|'tpm'\|'apple'\|'none'`   |
|  [07]   | `PublicKeyCredentialHint`      | union            | `'hybrid'\|'security-key'\|'client-device'` — browser modal optimization             |
|  [08]   | `PublicKeyCredentialFuture` / `AuthenticatorAttestationResponseFuture` / `PublicKeyCredentialDescriptorFuture` | interface | WebAuthn-L3 augmentations over stale DOM lib types |
|  [09]   | `RegistrationCredential` / `AuthenticationCredential` / `PublicKeyCredentialJSON` | interface/union | raw `credentials.*()` return shapes and their JSON union |
|  [10]   | DOM re-exports                 | type             | `COSEAlgorithmIdentifier`/`AuthenticatorSelectionCriteria`/`AuthenticationExtensionsClientInputs`/`UserVerificationRequirement`/`AttestationConveyancePreference` from bundled `dom.d.ts` |

[PUBLIC_TYPE_SCOPE]: FIDO MDS metadata vocabulary (`metadata/mdsTypes`)
- rail: auth

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                                                    |
| :-----: | :-------------------------- | :------------ | :----------------------------------------------------------------------------- |
|  [01]   | `MetadataStatement`         | type          | the `getStatement` return — authenticator descriptor, attestation roots, verification methods |
|  [02]   | `MetadataBLOBPayloadEntry` / `StatusReport` / `BiometricStatusReport` | type | MDS BLOB entry and per-authenticator certification status timeline |
|  [03]   | `AuthenticatorStatus`       | union         | `'FIDO_CERTIFIED'\|'ATTESTATION_KEY_COMPROMISE'\|'USER_VERIFICATION_BYPASS'\|…` — the AAGUID trust signal |
|  [04]   | `AuthenticatorGetInfo` / `UserVerify` / `KeyProtection` / `MatcherProtection` / `AttachmentHint` / `Attestation` / `AlgSign` / `AlgKey` | type | the CTAP/UAF authenticator-capability vocabulary carried in a statement |

[PUBLIC_TYPE_SCOPE]: services
- rail: auth

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]      | [CAPABILITY]                                                                |
| :-----: | :-------------------- | :----------------- | :------------------------------------------------------------------------- |
|  [01]   | `MetadataService`     | interface + singleton const | `initialize()` + `getStatement(aaguid)`; the default `MetadataService` instance |
|  [02]   | `BaseMetadataService` | class              | the concrete `implements MetadataService` — extend for custom MDS caching  |
|  [03]   | `SettingsService`     | interface + singleton const | `setRootCertificates()`/`getRootCertificates()` per `RootCertIdentifier`   |
|  [04]   | `RootCertIdentifier`  | union              | `AttestationFormat \| 'mds'` — the cert-registry key space                 |
|  [05]   | `VerificationMode`    | union              | `'permissive' \| 'strict'` — `initialize` AAGUID policy for `getStatement` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration flow (barrel `.`)
- rail: auth

| [INDEX] | [SURFACE]                                                                            | [ENTRY_FAMILY] | [CAPABILITY]                                                          |
| :-----: | :----------------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `generateRegistrationOptions(opts): Promise<PublicKeyCredentialCreationOptionsJSON>` | async builder  | required `rpName`/`rpID`/`userName`; optional `userID: Uint8Array_`/`challenge`/`timeout`/`attestationType: 'direct'\|'enterprise'\|'none'`/`excludeCredentials`/`authenticatorSelection`/`extensions`/`supportedAlgorithmIDs`/`preferredAuthenticatorType` |
|  [02]   | `verifyRegistrationResponse(opts): Promise<VerifiedRegistrationResponse>`            | async verifier | required `response`/`expectedChallenge`/`expectedOrigin`; optional `expectedRPID`/`expectedType`/`requireUserPresence`/`requireUserVerification`/`supportedAlgorithmIDs` |
|  [03]   | `supportedCOSEAlgorithmIdentifiers`                                                  | constant       | `COSEAlgorithmIdentifier[]` — default supported set `[-8, -7, -257]` (EdDSA/ES256/RS256) |

[ENTRYPOINT_SCOPE]: authentication flow (barrel `.`)
- rail: auth

| [INDEX] | [SURFACE]                                                                             | [ENTRY_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :------------------------------------------------------------------------------------ | :------------- | :---------------------------------------------------------------- |
|  [01]   | `generateAuthenticationOptions(opts): Promise<PublicKeyCredentialRequestOptionsJSON>` | async builder  | required `rpID`; optional `allowCredentials`/`challenge`/`timeout`/`userVerification: 'required'\|'preferred'\|'discouraged'`/`extensions` |
|  [02]   | `verifyAuthenticationResponse(opts): Promise<VerifiedAuthenticationResponse>`         | async verifier | required `response`/`expectedChallenge`/`expectedOrigin`/`expectedRPID`/`credential: WebAuthnCredential`; optional `expectedType`/`requireUserVerification`/`advancedFIDOConfig` |

[ENTRYPOINT_SCOPE]: services (barrel `.`)
- rail: auth

| [INDEX] | [SURFACE]                                                                       | [ENTRY_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :------------------------------------------------------------------------------ | :------------- | :------------------------------------------------------------- |
|  [01]   | `MetadataService.initialize(opts?): Promise<void>`                              | async init     | fetch MDS BLOBs — `mdsServers?`/`statements?`/`verificationMode?`; idempotent, once at startup |
|  [02]   | `MetadataService.getStatement(aaguid: string \| Uint8Array): Promise<MetadataStatement \| undefined>` | async query | resolve cached/fetched MDS entry; `'strict'` mode raises on unknown AAGUID |
|  [03]   | `SettingsService.setRootCertificates({identifier, certificates})`               | mutator        | register PEM/`Uint8Array_` roots by `RootCertIdentifier`; defaults ship for `android-key`/`android-safetynet`/`apple` + `mds` |
|  [04]   | `SettingsService.getRootCertificates({identifier}): string[]`                   | accessor       | retrieve PEM roots for a format id                             |

[ENTRYPOINT_SCOPE]: ceremony primitives (`@simplewebauthn/server/helpers`)
- rail: auth

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :---------------------------------------------------------------- | :------------- | :------------------------------------------------------------- |
|  [01]   | `generateChallenge(): Promise<Uint8Array_>` / `generateUserID(): Promise<Uint8Array_>` | CSPRNG | the same random sources the option builders use — pre-mint for custom session storage |
|  [02]   | `parseAuthenticatorData(authData): ParsedAuthenticatorData`      | parse          | flags/counter/AAGUID/credential from the authenticator-data blob |
|  [03]   | `decodeCredentialPublicKey(publicKey): COSEPublicKey` / `convertCOSEtoPKCS(cose): Uint8Array_` | key decode | COSE key decode and COSE→PKCS conversion |
|  [04]   | `verifySignature(opts): Promise<boolean>` / `toHash(data, algorithm?): Promise<Uint8Array_>` | crypto | low-level assertion signature verify; SHA hashing |
|  [05]   | `convertAAGUIDToString(aaguid): string`                          | identity       | AAGUID bytes → UUID string for MDS lookup                      |
|  [06]   | `getCertificateInfo(cert)` / `convertCertBufferToPEM(buf)` / `validateCertificatePath(x5c, anchors?)` / `isCertRevoked(cert)` | x509 | attestation-cert inspect, PEM convert, chain validate, CRL check |
|  [07]   | `verifyMDSBlob(...)`                                              | mds            | verify a raw FIDO MDS BLOB JWT                                 |
|  [08]   | `iso.{isoBase64URL, isoCBOR, isoCrypto, isoUint8Array}`          | isomorphic     | the runtime-portable base64url/CBOR/WebCrypto/byte-array toolkit — decode the browser's base64url wire values server-side |
|  [09]   | `cose.{COSEKEYS, COSEKTY, COSECRV, COSEALG}` + `isCOSEPublicKey*`/`isCOSEAlg` guards | cose | the COSE key/curve/algorithm enums and type guards |

## [04]-[IMPLEMENTATION_LAW]

[CREDENTIAL_TOPOLOGY]:
- `WebAuthnCredential` is the canonical stored row: persist `id`/`publicKey`/`counter`/`transports` per user and pass it as `credential` to `verifyAuthenticationResponse`.
- Replay defence is mandatory: write `authenticationInfo.newCounter` back to the stored `counter` after every successful assertion; persist `credentialDeviceType`/`credentialBackedUp` from the verified result for multi-device passkey eligibility.
- `expectedChallenge` accepts a `string` (exact match) or a `(challenge: string) => boolean | Promise<boolean>` predicate for session-bound custom validation; `expectedOrigin`/`expectedRPID` accept `string | string[]` for multi-origin / multi-RP RPs.
- `userID` in `generateRegistrationOptions` is `Uint8Array_` — mint via `helpers.generateUserID()` (or `crypto.getRandomValues`), never a plaintext identifier.

[VERIFICATION_TOPOLOGY]:
- `VerifiedRegistrationResponse` discriminates on `verified`: `registrationInfo` (carrying `credential`, `credentialDeviceType`, `credentialBackedUp`, `aaguid`, `fmt`) exists only on the `verified: true` branch — guard before reading it.
- `VerifiedAuthenticationResponse` is a flat interface; `authenticationInfo` is always present, so `verified` must be checked before trusting `newCounter`/`credentialDeviceType`.
- Default `attestationType` is `'none'`; MDS metadata validation is only relevant for `'direct'`/`'enterprise'`. `MetadataService.initialize()` must precede registration when MDS trust is required (no-op if already initialized); `'strict'` mode raises on an unregistered AAGUID, `'permissive'` admits it.

[STACKING_LAW]:
- Each async function is one `Effect.tryPromise({ try: () => generate…/verify…(opts), catch: (e) => new AuthFault(...) })` — a `Data.TaggedError`/`Schema.TaggedError` rail, never a thrown exception in domain code; the four lifts are the `VerifyWebauthn*`/`BeginWebauthn*` arms of `security/auth.md`'s `AuthCommand.$match`, sitting beside the `otplib` TOTP arm under one `Authn` `Effect.Service`.
- `WebAuthnCredential` rows load/store through the `@effect/sql-pg` `SqlClient` (`persistence/store`); the `newCounter` write-back is one `sql` UPDATE in the same verify effect. `transports` round-trips as `Option` at the row boundary (`Option.getOrUndefined(row.transports) satisfies WebAuthnCredential['transports']`).
- `MetadataService.initialize()` composes as a startup `Layer.effect` (or `Layer.scoped`) run once at the composition root; `SettingsService.setRootCertificates` likewise seeds custom attestation roots at boot.
- The browser ceremony (`platform/session`) forwards its `RegistrationResponseJSON`/`AuthenticationResponseJSON` over the `interchange` `CommandGateway`; this server owner decodes and verifies. The `helpers/iso` layer (`isoBase64URL`, `isoCBOR`) is the boundary that turns the browser's base64url/CBOR wire into typed bytes — compose it, never a hand-rolled `atob`/CBOR reader.

[RAIL_LAW]:
- Package: `@simplewebauthn/server` (+ `/helpers`)
- Owns: server-side WebAuthn registration/authentication flows and, via `./helpers`, the sanctioned ceremony primitives (challenge/user-id CSPRNG, COSE decode, authenticator-data parse, signature verify, X.509 chain, isomorphic base64url/CBOR/crypto)
- Accept: `RegistrationResponseJSON`/`AuthenticationResponseJSON` from the browser; `WebAuthnCredential` as the stored row; `Effect.tryPromise` per async call; `helpers.iso*`/`helpers.parseAuthenticatorData` for any low-level decode; the `newCounter` replay write-back
- Reject: a browser-side verifier (this is the server concern the ceremony forwards to); hand-rolled attestation parsing, CBOR decode, or signature math where `./helpers` already exposes the primitive; a sibling verifier method per credential kind where `AuthCommand.$match` discriminates; reading `registrationInfo` without the `verified` guard; a plaintext `userID`
