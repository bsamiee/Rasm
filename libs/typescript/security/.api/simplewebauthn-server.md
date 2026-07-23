# [TS_SECURITY_API_SIMPLEWEBAUTHN_SERVER]

[PACKAGE_SURFACE]:
- package: `@simplewebauthn/server` (MIT)
- module: ESM + CJS (`esm/` `import` / `script/` `require`); no `types` field — `.d.ts` ship adjacent to each entry, so resolution is entry-relative, not a top-level `types` asset. Two subpaths: `.` (ceremonies + services) and `./helpers` (the low-level codec/parse surface).
- asset: `esm/index.d.ts` (barrel) + `esm/helpers/index.d.ts`; bundles `@peculiar/asn catalog-*`/`@peculiar/x50 catalog` (cert-path validation), `@levischuck/tiny-cbor` (CBOR), `@hexagon/base6 catalog` — no peer dependencies.
- runtime: node `>=catalog`; server/RP-only — the WebAuthn relying-party verifier, NOT browser-safe. The browser counterpart `simplewebauthn-browser.md` invokes the ceremony; this package mints the challenge and verifies the signed response.
- ABI: every ceremony and helper is async (`Promise<…>`) — signature verification, CBOR decode, and cert-path validation run through WebCrypto/ASN.1.
- plane: `plane:runtime` (W1); admitted in `authn/` ONLY — `tests/typescript/_architecture` bans it outside its admission sub-folder. Catalogued here.
- rail: credential-ceremony / passkey-verification.

`@simplewebauthn/server` is the relying-party half of the passkey ceremony `authn/webauthn` owns: it mints ceremony options (a challenge + policy) and verifies the authenticator's signed response, returning a typed verdict — never a boolean-plus-throw. The whole surface is ONE parameterized pattern at two phases across two ceremonies: `generate{Registration,Authentication}Options` mints the options JSON, `verify{Registration,Authentication}Response` returns a discriminated `Verified…Response`. Registration verification internally dispatches the attestation-format verifier (packed/tpm/apple/android-key/android-safetynet/fido-u2f/none) keyed by the decoded `fmt` — the consumer never writes that switch; it is parameterized by `attestationType`, `supportedAlgorithmIDs`, and the `SettingsService` root-cert trust anchors. The verified registration yields a `WebAuthnCredential` (`id`/`publicKey`/`counter`/`transports`) that `session`'s `IdentityJournal` port stores; authentication verification returns a `newCounter` that is the signature-counter replay defense. `Redacted` is not the carrier here — passkey material is public-key crypto, so the credential and challenge are typed boundary values, not secrets.

## [01]-[CEREMONY]

The 2×2 pattern: `{registration, authentication} × {options, verify}`. Each entry takes ONE options object and returns a `Promise`; the verify results are typed rails, not thrown exceptions. `Parameters<typeof …>[0]` aliases (`GenerateRegistrationOptionsOpts`, …) are exported for each options bag.

| [INDEX] | [ENTRYPOINT]                          | [INPUT]                                          | [OUTPUT_BOUNDARY]                        |
| :-----: | :------------------------------------ | :----------------------------------------------- | :--------------------------------------- |
|  [01]   | `generateRegistrationOptions(opts)`   | `rpName`/`rpID`/`userName` + policy              | `PublicKeyCredentialCreationOptionsJSON` |
|  [02]   | `verifyRegistrationResponse(opts)`    | `response` + `expectedChallenge`/`Origin`/`RPID` | `VerifiedRegistrationResponse`           |
|  [03]   | `generateAuthenticationOptions(opts)` | `rpID` + `allowCredentials`/`userVerification`   | `PublicKeyCredentialRequestOptionsJSON`  |
|  [04]   | `verifyAuthenticationResponse(opts)`  | `response` + `credential` + `expectedChallenge`  | `VerifiedAuthenticationResponse`         |
|  [05]   | `supportedCOSEAlgorithmIdentifiers`   | const                                            | `COSEAlgorithmIdentifier[]`              |

[OUTPUT_BOUNDARY] role per entry:
- [01]-[REG_MINT]: the mint.
- [02]-[REG_VERIFY]: discriminated on `verified`.
- [03]-[AUTH_MINT]: the challenge.
- [04]-[AUTH_VERIFY]: carries `newCounter` — the replay/clone defense.
- [05]-[COSE_ALLOW]: the default COSE alg allow-list `supportedAlgorithmIDs` narrows.

[SURFACES]: `generateRegistrationOptions({…}) -> Promise<PublicKeyCredentialCreationOptionsJSON>` `verifyRegistrationResponse({…}) -> Promise<VerifiedRegistrationResponse>`

[VERIFIED_REGISTRATION_RESPONSE]: `VerifiedRegistrationResponse = |{…}`
[VERIFIED_AUTHENTICATION_RESPONSE]: `VerifiedAuthenticationResponse.verified: boolean` `VerifiedAuthenticationResponse.authenticationInfo: {…}`
[SURFACES]: `verifyAuthenticationResponse({…}) -> Promise<VerifiedAuthenticationResponse>`

## [02]-[VOCABULARY]

The type surface is the boundary contract between the browser response and the RP: the `…JSON` shapes are the wire ingress (decoded, never trusted raw), `WebAuthnCredential` is the persisted state, and the tagged scalars key the ceremony policy.

| [INDEX] | [SYMBOL]                                                         | [TYPE_FAMILY] |
| :-----: | :--------------------------------------------------------------- | :------------ |
|  [01]   | `WebAuthnCredential`                                             | record        |
|  [02]   | `RegistrationResponseJSON` / `AuthenticationResponseJSON`        | wire ingress  |
|  [03]   | `PublicKeyCredentialCreationOptionsJSON` / `…RequestOptionsJSON` | wire egress   |
|  [04]   | `AttestationFormat`                                              | union         |
|  [05]   | `CredentialDeviceType` / `AuthenticatorTransportFuture`          | union         |
|  [06]   | `Base64URLString` / `COSEAlgorithmIdentifier`                    | brand / id    |
|  [07]   | `AuthenticatorSelectionCriteria` / `…ExtensionsClientInputs`     | policy        |

[ROLE_BOUNDARY] per type:
- [01]-[CREDENTIAL]: `{ id: Base64URLString; publicKey: Uint8Array; counter: number; transports? }` — the stored passkey.
- [02]-[RESPONSE_IN]: the browser's signed response — the `verify*` input, `Schema`-decoded at the edge.
- [03]-[OPTIONS_OUT]: the ceremony options serialized to the browser.
- [04]-[ATTESTATION_FMT]: `'fido-u2f'\|'packed'\|'android-safetynet'\|'android-key'\|'tpm'\|'apple'\|'none'` — the internal verifier key.
- [05]-[DEVICE_TRANSPORT]: `'singleDevice'\|'multiDevice'`; `'ble'\|'cable'\|'hybrid'\|'internal'\|'nfc'\|'smart-card'\|'usb'`.
- [06]-[ENCODING_ID]: the ID encoding; the COSE signature-algorithm identifier.
- [07]-[POLICY]: resident-key/UV demands and the client-extension inputs (`opts.extensions`).

## [03]-[TRUST_AND_HELPERS]

Two module-singleton services own the attestation trust anchors, and the `./helpers` subpath exposes the low-level codec/parse primitives the ceremonies compose — the surface a bespoke verifier or a debugging path reaches for. The attestation-format verifiers are SEED DATA on the one verify pattern, dispatched by `fmt` inside `verifyRegistrationResponse`; `SettingsService` supplies their root certs, `MetadataService` their AAGUID metadata.

| [INDEX] | [SURFACE]                                                                                 | [PRODUCES_OWNS]                   |
| :-----: | :---------------------------------------------------------------------------------------- | :-------------------------------- |
|  [01]   | `SettingsService` (singleton)                                                             | `set`/`getRootCertificates(opts)` |
|  [02]   | `MetadataService` / `BaseMetadataService`                                                 | `initialize`/`getStatement`       |
|  [03]   | `./helpers` `iso.{isoBase64URL,isoCBOR,isoCrypto,isoUint8Array}`                          | isomorphic codecs                 |
|  [04]   | `./helpers` `cose` (namespace)                                                            | COSE key/alg vocabulary           |
|  [05]   | `./helpers` `parseAuthenticatorData` / `decodeClientDataJSON` / `decodeAttestationObject` | structured decode                 |
|  [06]   | `./helpers` `generateChallenge` / `generateUserID`                                        | `Promise<Uint8Array>`             |
|  [07]   | `./helpers` `verifySignature` / `toHash` / `validateCertificatePath` / `verifyMDSBlob`    | crypto ops                        |

[CAPABILITY] per surface:
- [01]-[ROOT_CERTS]: the attestation root-cert store keyed by `RootCertIdentifier` (`AttestationFormat \| 'mds'`).
- [02]-[METADATA]: FIDO MDS blob load + AAGUID metadata; `'strict'`/`'permissive'` unregistered-AAGUID policy.
- [03]-[ISO_CODECS]: runtime-portable base64url/CBOR/WebCrypto/byte primitives.
- [04]-[COSE]: `COSEALG`/`COSEKEYS`/`COSEKTY`/`COSEPublicKey` decode for the credential public key.
- [05]-[PARSE]: the authenticator-data/client-data/attestation parsers the ceremonies fold.
- [06]-[RNG]: WebCrypto RNG for the challenge/user handle (or supply `opts.challenge`/`userID`).
- [07]-[CRYPTO_OPS]: the signature/hash/cert-path/MDS primitives the verifiers reuse.

[SETTINGS_SERVICE]: `SettingsService.setRootCertificates({identifier:AttestationFormat|'mds';certificates:(Uint8Array|string)[]}) -> void` `SettingsService.getRootCertificates({identifier:AttestationFormat|'mds'}) -> string[]`
[METADATA_SERVICE]: `MetadataService.initialize({mdsServers?:string[];statements?:MetadataStatement[];verificationMode?:'permissive'|'strict'}?) -> Promise<void>` `MetadataService.getStatement(string|Uint8Array) -> Promise<MetadataStatement|undefined>`

## [04]-[INTEGRATION]

[STACK: ceremony + `Effect.tryPromise` + `Schema.TaggedError` (`.api/effect.md`)] — the four `Promise`-returning ceremonies land on the folder's typed rail: `Effect.tryPromise({ try: () => verifyRegistrationResponse(opts), catch: (e) => new WebAuthnCeremonyError({ cause: e }) })`. A thrown attestation failure becomes a typed error in the `E` channel; the `VerifiedRegistrationResponse` discriminant is matched with `effect/Match` so the credential is extracted only on `verified: true` — no boolean-plus-throw control flow.

[STACK: response ingress + `Schema.Struct` (`.api/effect.md`)] — `RegistrationResponseJSON`/`AuthenticationResponseJSON` arrive from the browser as untrusted JSON; `authn/webauthn` decodes them through a `Schema.Struct` (parse-not-validate) BEFORE `verify*`, so a malformed client payload fails at the boundary with a `ParseError`, not deep in CBOR decode. The `…OptionsJSON` egress is `Schema`-encoded symmetrically.

[STACK: challenge + credential + `session`/`store` (`ARCHITECTURE.md` seams)] — the challenge is stateful across the two phases: `generate*Options` mints it, `session` stores it, and `expectedChallenge` is passed as the `(challenge) => boolean | Promise<boolean>` resolver that looks it up and consumes it single-use. The verified `WebAuthnCredential` is written through the `IdentityJournal` port `session/token` declares; `verifyAuthenticationResponse`'s `newCounter` updates the journalled counter — a non-increasing counter is the cloned-authenticator signal.

[STACK: RNG + `sign/crypto` (`.api/oslojs-crypto.md`) · browser counterpart `simplewebauthn-browser.md`] — the RP can delegate challenge/user-handle generation to `sign/crypto`'s `@oslojs/crypto` RNG by supplying `opts.challenge`/`opts.userID`, keeping one RNG owner; otherwise the package's `generateChallenge` uses WebCrypto. The `…OptionsJSON` this package mints is consumed by `simplewebauthn-browser`'s `startRegistration`/`startAuthentication`, whose `RegistrationResponseJSON`/`AuthenticationResponseJSON` output feeds back into `verify*` — the two packages are the two ends of one ceremony.

## [05]-[RAIL_LAW]

- Owns: the RP-side passkey ceremony — `generate{Registration,Authentication}Options` minting, `verify{Registration,Authentication}Response` verification with internal attestation-format dispatch, the `WebAuthnCredential`/`…JSON` boundary vocabulary, the `SettingsService` root-cert and `MetadataService` MDS trust surfaces, and the `./helpers` codec/parse/crypto primitives.
- Accept: one options object per ceremony; `expectedChallenge` as a session-store resolver closure; `Effect.tryPromise` wrapping into the typed error rail; `Schema` decode of the browser response before `verify*` and encode of the options after `generate*`; `effect/Match` on the `verified` discriminant; the verified `WebAuthnCredential` and `newCounter` journalled through the `IdentityJournal` port; `SettingsService`/`MetadataService` for enterprise/direct attestation.
- Reject: treating the verdict as a boolean-plus-throw (it is a discriminated rail); trusting the raw browser JSON without a `Schema` boundary decode; ignoring `newCounter` (the replay/clone defense); hand-writing an attestation-format switch (the `fmt` dispatch is internal — parameterize via `attestationType`/`SettingsService`); importing this node-only package into a browser-safe path (that is `simplewebauthn-browser`); resolving it outside the `authn/` admission sub-folder.
- Boundary: every entry is async and node-only (`>=20`, WebCrypto/ASN.1). Passkey material is public-key crypto — the credential/challenge are typed boundary values, not `Redacted` secrets. The challenge must be stored server-side between phases and consumed single-use; the RP never derives trust from the client payload alone.
