# [@simplewebauthn/server] — the RP-side passkey ceremony verifier: one options→verify pattern across registration and authentication, attestation formats dispatched internally

[PACKAGE_SURFACE]:
- package: `@simplewebauthn/server` · version `13.3.2` · license `MIT`
- module: ESM + CJS (`esm/` `import` / `script/` `require`); no `types` field — `.d.ts` ship adjacent to each entry, so resolution is entry-relative, not a top-level `types` asset. Two subpaths: `.` (ceremonies + services) and `./helpers` (the low-level codec/parse surface).
- asset: `esm/index.d.ts` (barrel) + `esm/helpers/index.d.ts`; bundles `@peculiar/asn1-*`/`@peculiar/x509` (cert-path validation), `@levischuck/tiny-cbor` (CBOR), `@hexagon/base64` — no peer dependencies.
- runtime: node `>=20.0.0`; server/RP-only — the WebAuthn relying-party verifier, NOT browser-safe. The browser counterpart `simplewebauthn-browser.md` invokes the ceremony; this package mints the challenge and verifies the signed response.
- ABI: every ceremony and helper is async (`Promise<…>`) — signature verification, CBOR decode, and cert-path validation run through WebCrypto/ASN.1.
- plane: `plane:runtime` (W1); admitted in `authn/` ONLY — `tests/typescript/_architecture` bans it outside its admission sub-folder. Catalogued here.
- rail: credential-ceremony / passkey-verification.

`@simplewebauthn/server` is the relying-party half of the passkey ceremony `authn/webauthn` owns: it mints ceremony options (a challenge + policy) and verifies the authenticator's signed response, returning a typed verdict — never a boolean-plus-throw. The whole surface is ONE parameterized pattern at two phases across two ceremonies: `generate{Registration,Authentication}Options` mints the options JSON, `verify{Registration,Authentication}Response` returns a discriminated `Verified…Response`. Registration verification internally dispatches the attestation-format verifier (packed/tpm/apple/android-key/android-safetynet/fido-u2f/none) keyed by the decoded `fmt` — the consumer never writes that switch; it is parameterized by `attestationType`, `supportedAlgorithmIDs`, and the `SettingsService` root-cert trust anchors. The verified registration yields a `WebAuthnCredential` (`id`/`publicKey`/`counter`/`transports`) that `session`'s `IdentityJournal` port stores; authentication verification returns a `newCounter` that is the signature-counter replay defense. `Redacted` is not the carrier here — passkey material is public-key crypto, so the credential and challenge are typed boundary values, not secrets.

## [01]-[CEREMONY]

The 2×2 pattern: `{registration, authentication} × {options, verify}`. Each entry takes ONE options object and returns a `Promise`; the verify results are typed rails, not thrown exceptions. `Parameters<typeof …>[0]` aliases (`GenerateRegistrationOptionsOpts`, …) are exported for each options bag.

| [INDEX] | [ENTRYPOINT]                              | [INPUT]                                              | [OUTPUT / BOUNDARY]                                            |
| :-----: | :---------------------------------------- | :-------------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | `generateRegistrationOptions(opts)`       | `rpName`/`rpID`/`userName` + policy                 | `Promise<PublicKeyCredentialCreationOptionsJSON>` — the mint    |
|  [02]   | `verifyRegistrationResponse(opts)`        | `response` + `expectedChallenge`/`Origin`/`RPID`    | `Promise<VerifiedRegistrationResponse>` — discriminated on `verified` |
|  [03]   | `generateAuthenticationOptions(opts)`     | `rpID` + `allowCredentials`/`userVerification`      | `Promise<PublicKeyCredentialRequestOptionsJSON>` — the challenge |
|  [04]   | `verifyAuthenticationResponse(opts)`      | `response` + `credential` + `expectedChallenge`     | `Promise<VerifiedAuthenticationResponse>` — carries `newCounter` |
|  [05]   | `supportedCOSEAlgorithmIdentifiers`       | const                                               | the default COSE alg allow-list `supportedAlgorithmIDs` narrows |

```ts contract
// Phase 1 — mint. attestationType selects the trust demand; the format verifier roster is dispatched internally, not by the caller.
declare function generateRegistrationOptions(opts: {
  rpName: string; rpID: string; userName: string
  userID?: Uint8Array; userDisplayName?: string; challenge?: string | Uint8Array; timeout?: number
  attestationType?: 'direct' | 'enterprise' | 'none'
  excludeCredentials?: { id: Base64URLString; transports?: AuthenticatorTransportFuture[] }[]
  authenticatorSelection?: AuthenticatorSelectionCriteria
  supportedAlgorithmIDs?: COSEAlgorithmIdentifier[]
  preferredAuthenticatorType?: 'securityKey' | 'localDevice' | 'remoteDevice'
}): Promise<PublicKeyCredentialCreationOptionsJSON>
// Phase 2 — verify. expectedChallenge is a value OR a (challenge)=>boolean|Promise<boolean> resolver — the session-store lookup closure.
declare function verifyRegistrationResponse(opts: {
  response: RegistrationResponseJSON
  expectedChallenge: string | ((challenge: string) => boolean | Promise<boolean>)
  expectedOrigin: string | string[]; expectedRPID?: string | string[]
  requireUserPresence?: boolean; requireUserVerification?: boolean
  supportedAlgorithmIDs?: COSEAlgorithmIdentifier[]
}): Promise<VerifiedRegistrationResponse>
```

```ts contract
// The verdict is a typed rail. Registration discriminates on `verified`; the credential is extracted only on the true arm.
type VerifiedRegistrationResponse =
  | { verified: false; registrationInfo?: never }
  | { verified: true; registrationInfo: {
      fmt: AttestationFormat; aaguid: string; credential: WebAuthnCredential; credentialType: 'public-key'
      credentialDeviceType: CredentialDeviceType; credentialBackedUp: boolean; userVerified: boolean
      origin: string; rpID?: string; attestationObject: Uint8Array
      authenticatorExtensionResults?: AuthenticationExtensionsAuthenticatorOutputs } }
// Authentication carries the counter — newCounter must exceed the stored counter or the assertion is a clone/replay.
declare function verifyAuthenticationResponse(opts: {
  response: AuthenticationResponseJSON; credential: WebAuthnCredential
  expectedChallenge: string | ((challenge: string) => boolean | Promise<boolean>)
  expectedOrigin: string | string[]; expectedRPID: string | string[]
  requireUserVerification?: boolean; advancedFIDOConfig?: { userVerification?: UserVerificationRequirement }
}): Promise<VerifiedAuthenticationResponse>
type VerifiedAuthenticationResponse = { verified: boolean; authenticationInfo: {
  credentialID: Base64URLString; newCounter: number; userVerified: boolean
  credentialDeviceType: CredentialDeviceType; credentialBackedUp: boolean; origin: string; rpID: string } }
```

## [02]-[VOCABULARY]

The type surface is the boundary contract between the browser response and the RP: the `…JSON` shapes are the wire ingress (decoded, never trusted raw), `WebAuthnCredential` is the persisted state, and the tagged scalars key the ceremony policy.

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]     | [ROLE / BOUNDARY]                                             |
| :-----: | :---------------------------------------------- | :---------------- | :----------------------------------------------------------- |
|  [01]   | `WebAuthnCredential`                             | record            | `{ id: Base64URLString; publicKey: Uint8Array; counter: number; transports? }` — the stored passkey |
|  [02]   | `RegistrationResponseJSON` / `AuthenticationResponseJSON` | wire ingress | the browser's signed response — the `verify*` input, `Schema`-decoded at the edge |
|  [03]   | `PublicKeyCredentialCreationOptionsJSON` / `…RequestOptionsJSON` | wire egress | the ceremony options serialized to the browser |
|  [04]   | `AttestationFormat`                             | union             | `'fido-u2f'\|'packed'\|'android-safetynet'\|'android-key'\|'tpm'\|'apple'\|'none'` — the internal verifier key |
|  [05]   | `CredentialDeviceType` / `AuthenticatorTransportFuture` | union     | `'singleDevice'\|'multiDevice'`; `'ble'\|'cable'\|'hybrid'\|'internal'\|'nfc'\|'smart-card'\|'usb'` |
|  [06]   | `Base64URLString` / `COSEAlgorithmIdentifier`   | brand / id        | the ID encoding; the COSE signature-algorithm identifier      |
|  [07]   | `AuthenticatorSelectionCriteria` / `…ExtensionsClientInputs` | policy | resident-key/UV demands and the client-extension inputs (`opts.extensions`) |

## [03]-[TRUST_AND_HELPERS]

Two module-singleton services own the attestation trust anchors, and the `./helpers` subpath exposes the low-level codec/parse primitives the ceremonies compose — the surface a bespoke verifier or a debugging path reaches for. The attestation-format verifiers are SEED DATA on the one verify pattern, dispatched by `fmt` inside `verifyRegistrationResponse`; `SettingsService` supplies their root certs, `MetadataService` their AAGUID metadata.

| [INDEX] | [SURFACE]                                            | [PRODUCES / OWNS]                  | [CAPABILITY]                                                  |
| :-----: | :--------------------------------------------------- | :--------------------------------- | :----------------------------------------------------------- |
|  [01]   | `SettingsService` (singleton)                        | `set`/`getRootCertificates(opts)`  | the attestation root-cert store keyed by `RootCertIdentifier` (`AttestationFormat \| 'mds'`) |
|  [02]   | `MetadataService` / `BaseMetadataService`            | `initialize`/`getStatement`        | FIDO MDS v3 blob load + AAGUID metadata; `'strict'`/`'permissive'` unregistered-AAGUID policy |
|  [03]   | `./helpers` `iso.{isoBase64URL,isoCBOR,isoCrypto,isoUint8Array}` | isomorphic codecs      | runtime-portable base64url/CBOR/WebCrypto/byte primitives     |
|  [04]   | `./helpers` `cose` (namespace)                       | COSE key/alg vocabulary            | `COSEALG`/`COSEKEYS`/`COSEKTY`/`COSEPublicKey` decode for the credential public key |
|  [05]   | `./helpers` `parseAuthenticatorData` / `decodeClientDataJSON` / `decodeAttestationObject` | structured decode | the authenticator-data/client-data/attestation parsers the ceremonies fold |
|  [06]   | `./helpers` `generateChallenge` / `generateUserID`   | `Promise<Uint8Array>`              | WebCrypto RNG for the challenge/user handle (or supply `opts.challenge`/`userID`) |
|  [07]   | `./helpers` `verifySignature` / `toHash` / `validateCertificatePath` / `verifyMDSBlob` | crypto ops | the signature/hash/cert-path/MDS primitives the verifiers reuse |

```ts contract
// Enterprise/direct attestation is configured once, not per-ceremony: the root certs are the trust anchors the fmt verifier validates the attestation cert chain against.
declare const SettingsService: {
  setRootCertificates(opts: { identifier: AttestationFormat | 'mds'; certificates: (Uint8Array | string)[] }): void
  getRootCertificates(opts: { identifier: AttestationFormat | 'mds' }): string[]
}
declare const MetadataService: {
  initialize(opts?: { mdsServers?: string[]; statements?: MetadataStatement[]; verificationMode?: 'permissive' | 'strict' }): Promise<void>
  getStatement(aaguid: string | Uint8Array): Promise<MetadataStatement | undefined>
}
```

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
