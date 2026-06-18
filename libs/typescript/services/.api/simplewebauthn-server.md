# [API_CATALOGUE] @simplewebauthn/server

`@simplewebauthn/server` owns the server-side WebAuthn flow: `generateRegistrationOptions` and `generateAuthenticationOptions` produce JSON-serialisable challenge options for the browser, while `verifyRegistrationResponse` and `verifyAuthenticationResponse` verify the authenticator's signed response. `MetadataService` downloads and caches FIDO MDS blobs; `SettingsService` manages root certificates per attestation format. All four primary functions are `async` and resolve to typed result objects.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@simplewebauthn/server`
- package: `@simplewebauthn/server`
- entry: `@simplewebauthn/server` (single barrel — ESM `esm/index.d.ts`)
- asset: registration/authentication option generators, response verifiers, `MetadataService`, `SettingsService`, and shared WebAuthn types
- rail: auth / webauthn

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: wire model family — registration
- rail: auth

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]       | [RAIL]                                                          |
| :-----: | :--------------------------------------- | :------------------ | :-------------------------------------------------------------- |
|   [1]   | `PublicKeyCredentialCreationOptionsJSON` | interface           | challenge options sent to browser for registration              |
|   [2]   | `RegistrationResponseJSON`               | interface           | browser attestation response sent to server                     |
|   [3]   | `AuthenticatorAttestationResponseJSON`   | interface           | inner attestation response fields                               |
|   [4]   | `VerifiedRegistrationResponse`           | discriminated union | `{ verified: false }` \| `{ verified: true; registrationInfo }` |
|   [5]   | `GenerateRegistrationOptionsOpts`        | type alias          | `Parameters<typeof generateRegistrationOptions>[0]`             |
|   [6]   | `VerifyRegistrationResponseOpts`         | type alias          | `Parameters<typeof verifyRegistrationResponse>[0]`              |

[PUBLIC_TYPE_SCOPE]: wire model family — authentication
- rail: auth

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [RAIL]                                                |
| :-----: | :-------------------------------------- | :------------ | :---------------------------------------------------- |
|   [1]   | `PublicKeyCredentialRequestOptionsJSON` | interface     | challenge options sent to browser for authentication  |
|   [2]   | `AuthenticationResponseJSON`            | interface     | browser assertion response sent to server             |
|   [3]   | `AuthenticatorAssertionResponseJSON`    | interface     | inner assertion response fields                       |
|   [4]   | `VerifiedAuthenticationResponse`        | interface     | `{ verified; authenticationInfo }` result             |
|   [5]   | `GenerateAuthenticationOptionsOpts`     | type alias    | `Parameters<typeof generateAuthenticationOptions>[0]` |
|   [6]   | `VerifyAuthenticationResponseOpts`      | type alias    | `Parameters<typeof verifyAuthenticationResponse>[0]`  |

[PUBLIC_TYPE_SCOPE]: credential and attestation vocabulary
- rail: auth

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]    | [RAIL]                                                                                                  |
| :-----: | :----------------------------- | :--------------- | :------------------------------------------------------------------------------------------------------ |
|   [1]   | `WebAuthnCredential`           | type alias       | `{ id: Base64URLString; publicKey: Uint8Array_; counter: number; transports? }` — stored credential row |
|   [2]   | `Base64URLString`              | type alias       | branded `string` for base64url-encoded values                                                           |
|   [3]   | `Uint8Array_`                  | type alias       | `ReturnType<Uint8Array['slice']>`; bridges TS 5.6/5.7 generics                                          |
|   [4]   | `CredentialDeviceType`         | type alias       | `'singleDevice' \| 'multiDevice'`                                                                       |
|   [5]   | `AuthenticatorTransportFuture` | type alias       | `'ble' \| 'cable' \| 'hybrid' \| 'internal' \| 'nfc' \| 'smart-card' \| 'usb'`                          |
|   [6]   | `AttestationFormat`            | type alias       | `'fido-u2f' \| 'packed' \| 'android-safetynet' \| 'android-key' \| 'tpm' \| 'apple' \| 'none'`          |
|   [7]   | `PublicKeyCredentialHint`      | type alias       | `'hybrid' \| 'security-key' \| 'client-device'`                                                         |
|   [8]   | `COSEAlgorithmIdentifier`      | type alias (DOM) | numeric COSE algorithm ID (e.g. `-8`, `-7`, `-257`)                                                     |
|   [9]   | `VerificationMode`             | type alias       | `'permissive' \| 'strict'`; controls `MetadataService` AAGUID policy                                    |

[PUBLIC_TYPE_SCOPE]: services
- rail: auth

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [RAIL]                                                               |
| :-----: | :-------------------- | :------------ | :------------------------------------------------------------------- |
|   [1]   | `MetadataService`     | singleton     | downloads FIDO MDS BLOBs; `initialize()` + `getStatement(aaguid)`    |
|   [2]   | `SettingsService`     | singleton     | `setRootCertificates()` + `getRootCertificates()` by format id       |
|   [3]   | `BaseMetadataService` | class         | concrete `MetadataService` implementation; extend for custom caching |
|   [4]   | `RootCertIdentifier`  | type alias    | `AttestationFormat \| 'mds'`                                         |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration flow
- rail: auth

| [INDEX] | [SURFACE]                                                                            | [ENTRY_FAMILY]  | [RAIL]                                                                |
| :-----: | :----------------------------------------------------------------------------------- | :-------------- | :-------------------------------------------------------------------- |
|   [1]   | `generateRegistrationOptions(opts): Promise<PublicKeyCredentialCreationOptionsJSON>` | async generator | builds challenge options for browser `navigator.credentials.create()` |
|   [2]   | `verifyRegistrationResponse(opts): Promise<VerifiedRegistrationResponse>`            | async verifier  | verifies attestation; returns `verified` flag and `registrationInfo`  |
|   [3]   | `supportedCOSEAlgorithmIdentifiers: COSEAlgorithmIdentifier[]`                       | constant        | default supported algorithm IDs `[-8, -7, -257]`                      |

`generateRegistrationOptions` required options: `rpName`, `rpID`, `userName`. Optional: `userID`, `challenge`, `userDisplayName`, `timeout`, `attestationType`, `excludeCredentials`, `authenticatorSelection`, `extensions`, `supportedAlgorithmIDs`, `preferredAuthenticatorType`.

`verifyRegistrationResponse` required options: `response`, `expectedChallenge`, `expectedOrigin`. Optional: `expectedRPID`, `expectedType`, `requireUserPresence`, `requireUserVerification`, `supportedAlgorithmIDs`, `attestationSafetyNetEnforceCTSCheck`.

[ENTRYPOINT_SCOPE]: authentication flow
- rail: auth

| [INDEX] | [SURFACE]                                                                             | [ENTRY_FAMILY]  | [RAIL]                                                             |
| :-----: | :------------------------------------------------------------------------------------ | :-------------- | :----------------------------------------------------------------- |
|   [1]   | `generateAuthenticationOptions(opts): Promise<PublicKeyCredentialRequestOptionsJSON>` | async generator | builds challenge options for browser `navigator.credentials.get()` |
|   [2]   | `verifyAuthenticationResponse(opts): Promise<VerifiedAuthenticationResponse>`         | async verifier  | verifies assertion; returns `verified` + `authenticationInfo`      |

`generateAuthenticationOptions` required: `rpID`. Optional: `allowCredentials`, `challenge`, `timeout`, `userVerification`, `extensions`.

`verifyAuthenticationResponse` required: `response`, `expectedChallenge`, `expectedOrigin`, `expectedRPID`, `credential`. Optional: `expectedType`, `requireUserVerification`, `advancedFIDOConfig`.

[ENTRYPOINT_SCOPE]: services
- rail: auth

| [INDEX] | [SURFACE]                                                                       | [ENTRY_FAMILY] | [RAIL]                                        |
| :-----: | :------------------------------------------------------------------------------ | :------------- | :-------------------------------------------- |
|   [1]   | `MetadataService.initialize(opts?): Promise<void>`                              | async init     | fetches MDS BLOBs; call once at startup       |
|   [2]   | `MetadataService.getStatement(aaguid): Promise<MetadataStatement \| undefined>` | async query    | resolves cached or freshly fetched MDS entry  |
|   [3]   | `SettingsService.setRootCertificates(opts): void`                               | mutator        | register root certs by `RootCertIdentifier`   |
|   [4]   | `SettingsService.getRootCertificates(opts): string[]`                           | accessor       | retrieve PEM-encoded root certs by identifier |

## [4]-[IMPLEMENTATION_LAW]

[CREDENTIAL_TOPOLOGY]:
- `WebAuthnCredential` is the canonical stored credential row: persist `id`, `publicKey`, `counter`, and `transports` per user; pass it as `credential` to `verifyAuthenticationResponse`.
- `counter` must be updated to `authenticationInfo.newCounter` after each successful verification to prevent replay attacks.
- `credentialDeviceType` and `credentialBackedUp` must be stored for multi-device passkey eligibility tracking.
- `expectedChallenge` in both verify functions accepts a `string` (exact match) or a `(challenge: string) => boolean | Promise<boolean>` predicate for custom session handling.
- `expectedOrigin` and `expectedRPID` both accept `string | string[]` to support multi-origin / multi-RP setups.

[VERIFICATION_TOPOLOGY]:
- `VerifiedRegistrationResponse` is a discriminated union on `verified`: only the `verified: true` branch contains `registrationInfo`; guard with a truthiness check before accessing it.
- `VerifiedAuthenticationResponse` is a flat interface; `verified` is always present, `authenticationInfo` is always present regardless of `verified` value — check `verified` before trusting the info fields.
- `userID` in `generateRegistrationOptions` accepts `Uint8Array_`; generate cryptographically random bytes via `crypto.getRandomValues()` or a platform equivalent.
- Default `attestationType` is `'none'`; FIDO MDS metadata validation is only relevant for `'direct'` and `'enterprise'` attestation.

[METADATA_TOPOLOGY]:
- `MetadataService.initialize()` must be called before `verifyRegistrationResponse` if MDS validation is required; it is a no-op if already initialized.
- `verificationMode: 'strict'` (default) raises during registration when the authenticator AAGUID has no MDS entry; `'permissive'` allows unknown AAGUIDs through.
- `SettingsService` ships default root certificates for `'android-key'`, `'android-safetynet'`, `'apple'`, and `'android-mds'`; call `setRootCertificates` only to override.

[RAIL_LAW]:
- Package: `@simplewebauthn/server`
- Owns: server-side WebAuthn registration and authentication flows
- Accept: `RegistrationResponseJSON` / `AuthenticationResponseJSON` from the browser
- Reject: hand-rolled attestation parsing, custom CBOR decoding, or manual signature verification
