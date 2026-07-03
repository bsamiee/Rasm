# [API_CATALOGUE] @simplewebauthn/browser

`@simplewebauthn/browser` owns the browser half of the WebAuthn ceremony: `startRegistration` drives `navigator.credentials.create` (attestation) and `startAuthentication` drives `navigator.credentials.get` (assertion), each taking server-issued options JSON and returning response JSON destined for the verifier — one registration/authentication mirror over a single credential axis, never two parallel surfaces. Both methods are `Promise`-returning and lift through `Effect.tryPromise` into the closed `AuthFault` the `Session/session.md` credential fold dispatches, with `WebAuthnError.code` (a 12-member `WebAuthnErrorCode` union) as the discriminant — the exact shape as `arctic`'s `OAuth2RequestError.code -> ProtocolError`. The wire types are copied verbatim from `@simplewebauthn/types` and shared with `@simplewebauthn/server`, so the browser mints the response the `services/.api/simplewebauthn-server.md` verifier consumes over the `session/session -> services/security` seam; the browser never verifies. Feature detection, Base64URL conversion, and a single-ceremony abort singleton round out the surface.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@simplewebauthn/browser`
- package: `@simplewebauthn/browser`
- version: `13.3.0`
- license: `MIT`
- module: barrel `@simplewebauthn/browser` — dual `esm/index.js` (`"module"`) + `script/index.js` (`"main"`, CJS); `exports["."]` = `{ import: ./esm/index.js, require: ./script/index.js }`, no `exports.types`, `.d.ts` ships alongside `esm/`. The barrel re-exports the two ceremony methods, five helpers, the abort singleton, `WebAuthnError` + `WebAuthnErrorCode`, and the full `types/` wire family; the internal `identifyRegistrationError`/`identifyAuthenticationError`/`isValidDomain`/`toAuthenticatorAttachment`/`toPublicKeyCredentialDescriptor` helpers are NOT re-exported and are not public surface
- asset: browser-native ceremony library — zero dependencies, zero peer deps, no `engines` floor; runs on the WebAuthn `navigator.credentials` + `window`/`PublicKeyCredential` platform globals (no Node built-ins), so it is SPA-native; the JSON wire types are shared verbatim with `@simplewebauthn/server`
- rail: auth — WebAuthn passkey registration (attestation) and authentication (assertion) ceremony

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: ceremony method option carriers — the `Parameters<>`-derived request shapes
- rail: auth
- Both are derived from the method signature (never hand-authored), the mirror of the server catalog's `GenerateRegistrationOptionsOpts` pattern.

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [BOUNDARY_NOTE]                                                                                      |
| :-----: | :----------------------- | :------------ | :--------------------------------------------------------------------------------------------------- |
|  [01]   | `StartRegistrationOpts`  | type alias    | `Parameters<typeof startRegistration>[0]` — `{ optionsJSON; useAutoRegister? }`                      |
|  [02]   | `StartAuthenticationOpts`| type alias    | `Parameters<typeof startAuthentication>[0]` — `{ optionsJSON; useBrowserAutofill?; verifyBrowserAutofillInput? }` |

[PUBLIC_TYPE_SCOPE]: wire model family — options in, response out (shared verbatim with `@simplewebauthn/server`)
- rail: auth
- These are the one wire boundary: the server mints the `*OptionsJSON`, the browser mints the `*ResponseJSON`, and `services/.api/simplewebauthn-server.md` verifies the response. Decode server options through `Schema.decodeUnknown` at the `interchange` ingress; forward the browser response over the `CommandGateway`. Never reconstruct any of these by hand.

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]     | [BOUNDARY_NOTE]                                                        |
| :-----: | :--------------------------------------- | :---------------- | :--------------------------------------------------------------------- |
|  [01]   | `PublicKeyCredentialCreationOptionsJSON` | interface         | registration challenge options from the server into `startRegistration`|
|  [02]   | `RegistrationResponseJSON`               | interface         | attestation response from `startRegistration` to the verifier         |
|  [03]   | `PublicKeyCredentialRequestOptionsJSON`  | interface         | authentication challenge options from the server into `startAuthentication` |
|  [04]   | `AuthenticationResponseJSON`             | interface         | assertion response from `startAuthentication` to the verifier         |
|  [05]   | `AuthenticatorAttestationResponseJSON`   | interface         | inner attestation payload (`clientDataJSON`, `attestationObject`, `transports?`, `publicKey?`) |
|  [06]   | `AuthenticatorAssertionResponseJSON`     | interface         | inner assertion payload (`clientDataJSON`, `authenticatorData`, `signature`, `userHandle?`) |
|  [07]   | `PublicKeyCredentialDescriptorJSON`      | interface         | credential descriptor JSON (`id`, `type`, `transports?`)              |
|  [08]   | `PublicKeyCredentialUserEntityJSON`      | interface         | JSON user entity (`id`, `name`, `displayName`)                        |
|  [09]   | `PublicKeyCredentialJSON`                | discriminated union | `RegistrationResponseJSON \| AuthenticationResponseJSON`             |
|  [10]   | `WebAuthnCredential`                     | type alias        | stored credential row (`id`, `publicKey: Uint8Array_`, `counter`, `transports?`) — the verifier's persisted form |
|  [11]   | `Base64URLString`                        | type alias        | `string` branded for Base64URL; every credential byte field crosses the wire in this form |
|  [12]   | `Uint8Array_`                            | type alias        | `ReturnType<Uint8Array['slice']>` — bridges the TS 5.7 `Uint8Array<ArrayBuffer>` generic gap |

[PUBLIC_TYPE_SCOPE]: bounded vocabularies — closed literal unions, never open strings
- rail: auth
- Each is the discriminant of a field, not a free `string`; a new value breaks every exhaustive match at compile time.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [BOUNDARY_NOTE]                                                                       |
| :-----: | :----------------------------- | :------------ | :------------------------------------------------------------------------------------ |
|  [01]   | `AuthenticatorTransportFuture` | string union  | `'ble' \| 'cable' \| 'hybrid' \| 'internal' \| 'nfc' \| 'smart-card' \| 'usb'` — extended transports |
|  [02]   | `PublicKeyCredentialHint`      | string union  | `'hybrid' \| 'security-key' \| 'client-device'` — RP registration-flow hint          |
|  [03]   | `CredentialDeviceType`         | string union  | `'singleDevice' \| 'multiDevice'` — backup-eligibility bit projection                 |
|  [04]   | `AttestationFormat`            | string union  | `'fido-u2f' \| 'packed' \| 'android-safetynet' \| 'android-key' \| 'tpm' \| 'apple' \| 'none'` |

[PUBLIC_TYPE_SCOPE]: DOM-augment and re-export surface (barrel-forwarded from `types/dom.js`)
- rail: auth
- The `*Future` interfaces augment TypeScript's outdated DOM `PublicKeyCredential`/`AuthenticatorAttestationResponse` with WebAuthn L3 members; the plain DOM re-exports (`AuthenticatorSelectionCriteria`, `UserVerificationRequirement`, `AuthenticatorAttachment`, `ResidentKeyRequirement`, `AttestationConveyancePreference`, `COSEAlgorithmIdentifier`, `AuthenticationExtensionsClientInputs`/`Outputs`, `PublicKeyCredentialParameters`, `PublicKeyCredentialRpEntity`) are the structural field types of the JSON options, authored server-side and rarely touched directly here.

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [BOUNDARY_NOTE]                                                    |
| :-----: | :-------------------------------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `PublicKeyCredentialFuture`             | interface     | augmented `PublicKeyCredential` — `parseCreationOptionsFromJSON?`, `toJSON?`, `isConditionalMediationAvailable?` |
|  [02]   | `AuthenticatorAttestationResponseFuture`| interface     | augmented attestation response with `getTransports()`             |
|  [03]   | `PublicKeyCredentialDescriptorFuture`   | interface     | descriptor with the extended `transports?`                        |
|  [04]   | `RegistrationCredential`                | interface     | the raw `navigator.credentials.create()` return (pre-JSON)        |
|  [05]   | `AuthenticationCredential`              | interface     | the raw `navigator.credentials.get()` return (pre-JSON)           |

[PUBLIC_TYPE_SCOPE]: ceremony fault vocabulary — the fold discriminant
- rail: auth
- `WebAuthnError.code` is a closed `WebAuthnErrorCode` union, NOT `unknown`; it is the tag the credential fold maps onto a `Data.TaggedEnum` case under exhaustive match, never a message-string parse.

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [BOUNDARY_NOTE]                                                                 |
| :-----: | :----------------- | :------------ | :------------------------------------------------------------------------------ |
|  [01]   | `WebAuthnError`    | error class   | `extends Error`; `code: WebAuthnErrorCode`; carries `cause: Error` — read by `instanceof` in the `Effect.tryPromise` catch |
|  [02]   | `WebAuthnErrorCode`| string union  | 12 members (below) — the exhaustive discriminant space                          |

`WebAuthnErrorCode` = `'ERROR_CEREMONY_ABORTED'` (user/router abort) `| 'ERROR_INVALID_DOMAIN' | 'ERROR_INVALID_RP_ID' | 'ERROR_INVALID_USER_ID_LENGTH' | 'ERROR_MALFORMED_PUBKEYCREDPARAMS' | 'ERROR_AUTHENTICATOR_GENERAL_ERROR' | 'ERROR_AUTHENTICATOR_MISSING_DISCOVERABLE_CREDENTIAL_SUPPORT' | 'ERROR_AUTHENTICATOR_MISSING_USER_VERIFICATION_SUPPORT' | 'ERROR_AUTHENTICATOR_PREVIOUSLY_REGISTERED' | 'ERROR_AUTHENTICATOR_NO_SUPPORTED_PUBKEYCREDPARAMS_ALG' | 'ERROR_AUTO_REGISTER_USER_VERIFICATION_FAILURE' | 'ERROR_PASSTHROUGH_SEE_CAUSE_PROPERTY'` (the DOM error survives on `.cause`).

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: ceremony methods — attestation and assertion
- rail: auth
- `optionsJSON` is passed through untouched from the server; the library owns the ArrayBuffer conversion internally (no manual `base64URLStringToBuffer` on this path) and auto-calls `WebAuthnAbortService.createNewAbortSignal()` per invocation.

```ts
// esm/methods/startRegistration.d.ts
export declare function startRegistration(options: {
  optionsJSON: PublicKeyCredentialCreationOptionsJSON;
  useAutoRegister?: boolean; // silently mint a passkey via the just-used password manager; default false
}): Promise<RegistrationResponseJSON>;

// esm/methods/startAuthentication.d.ts
export declare function startAuthentication(options: {
  optionsJSON: PublicKeyCredentialRequestOptionsJSON;
  useBrowserAutofill?: boolean;          // conditional UI; requires <input autocomplete="webauthn">; default false
  verifyBrowserAutofillInput?: boolean;  // guard for that input when autofill is on; default true
}): Promise<AuthenticationResponseJSON>;
```

[ENTRYPOINT_SCOPE]: feature detection — the admission gates
- rail: auth
- The sync `browserSupportsWebAuthn` is the hard gate; the two async probes decide the UI modality and can feed the capability rank.

```ts
// esm/helpers/*
export declare function browserSupportsWebAuthn(): boolean;              // sync hard gate
export declare function platformAuthenticatorIsAvailable(): Promise<boolean>; // Touch ID / Windows Hello present
export declare function browserSupportsWebAuthnAutofill(): Promise<boolean>;  // conditional-UI (autofill) capable
```

[ENTRYPOINT_SCOPE]: Base64URL conversion and the abort singleton
- rail: auth
- The two encoders are for adjacent credential-adjacent bytes (never the ceremony path itself). `WebAuthnAbortService` is a process singleton — the methods drive it automatically; manual use is client-routing cancellation only.

```ts
// esm/helpers/base64URLStringToBuffer.d.ts, bufferToBase64URLString.d.ts
export declare function base64URLStringToBuffer(base64URLString: string): ArrayBuffer;
export declare function bufferToBase64URLString(buffer: ArrayBuffer): string;

// esm/helpers/webAuthnAbortService.d.ts — exported singleton instance
export declare const WebAuthnAbortService: {
  createNewAbortSignal(): AbortSignal; // auto-called by both ceremony methods
  cancelCeremony(): void;              // abort any in-flight ceremony (call on route navigation)
};
```

[ENTRYPOINT_SCOPE]: WebAuthnError — the typed failure
- rail: auth

```ts
// esm/helpers/webAuthnError.d.ts
export declare class WebAuthnError extends Error {
  code: WebAuthnErrorCode;
  constructor(opts: { message: string; code: WebAuthnErrorCode; cause: Error; name?: string });
}
```

## [04]-[IMPLEMENTATION_LAW]

[CEREMONY_TOPOLOGY]:
- `startRegistration` calls `navigator.credentials.create`; `startAuthentication` calls `navigator.credentials.get` — one attestation/assertion mirror over one credential axis, never a per-flow method family.
- server options JSON is passed in verbatim and the response JSON is returned verbatim; the library owns all ArrayBuffer<->Base64URL conversion, so the caller never touches the raw credential objects.
- `useAutoRegister` enables silent passkey creation via the active password manager after a password sign-in; `useBrowserAutofill` initializes conditional UI and requires an `<input autocomplete="webauthn">` element (`verifyBrowserAutofillInput` guards for it, default true).
- `WebAuthnError.code` is the closed discriminant; `ERROR_CEREMONY_ABORTED` is raised by `WebAuthnAbortService.cancelCeremony()` or user cancellation, and `ERROR_PASSTHROUGH_SEE_CAUSE_PROPERTY` carries the original DOM error on `.cause`.

[INTEGRATION_LAW]:
- Stack with `effect` boundary lift (`.api/effect.md`): both ceremony methods are `Promise`-returning, so each crosses through `Effect.tryPromise({ try, catch })` where `catch` narrows the thrown `WebAuthnError` by `.code` into the closed `AuthFault` `Data.TaggedEnum` the `Session/session.md` fold owns — `ERROR_CEREMONY_ABORTED` -> the cancellation arm, `ERROR_PASSTHROUGH_SEE_CAUSE_PROPERTY` -> read `.cause`, the remaining ten -> the second-factor-failure case — dispatched under `Match.value(err.code)`, never a bare rejected `Promise` or a message-string parse. This is the identical rail `arctic` uses for `OAuth2RequestError.code`, so registration and OAuth share one `AuthFault` owner.
- Stack with `effect` `Schema` (`.api/effect.md`): the server-issued `PublicKeyCredentialCreationOptionsJSON`/`PublicKeyCredentialRequestOptionsJSON` decode through `Schema.decodeUnknown` at the `interchange` wire ingress and pass DIRECTLY into `startRegistration({ optionsJSON })` / `startAuthentication({ optionsJSON })`; the `Base64URLString` fields stay branded strings across decode (no manual conversion on the ceremony path).
- Cross-package wire mirror (the `session/session -> services/security` seam): the browser produces `RegistrationResponseJSON`/`AuthenticationResponseJSON`; these are the exact shapes `services/.api/simplewebauthn-server.md`'s `verifyRegistrationResponse`/`verifyAuthenticationResponse` consume. Forward the response over the `interchange` `CommandGateway` to the `services` `Authn` verifier — a browser-side `@simplewebauthn/server` import or any browser attestation/signature check is the named defect.
- Stack with the abort service under client routing (`.api/effect.md`): wrap `WebAuthnAbortService.cancelCeremony()` in `Effect.sync` and fire it on a `Session/router.md` Navigation-API route change to abort an in-flight ceremony (the methods auto-mint a fresh `AbortSignal` per call, so manual use is router-driven cancellation only).
- Feature-detect gate: `browserSupportsWebAuthn()` (via `Effect.sync`) is the hard admission gate before any ceremony; `platformAuthenticatorIsAvailable()` and `browserSupportsWebAuthnAutofill()` (via `Effect.promise`) project into the `Shell/capability.md` axis to decide the Touch-ID/autofill UI modality — never invoke a ceremony without the sync gate passing.

[RAIL_LAW]:
- Package: `@simplewebauthn/browser`
- Owns: browser-side WebAuthn ceremony execution (attestation + assertion), Base64URL conversion, WebAuthn feature detection, and single-ceremony abort control
- Accept: server-issued options JSON decoded through `Schema` and passed verbatim into the ceremony method; every ceremony lifted through `Effect.tryPromise` with `WebAuthnError.code` folded to the closed `AuthFault`; the response JSON forwarded to the `services` verifier over the wire
- Reject: direct `navigator.credentials.create`/`.get` bypassing the ceremony methods; hand-rolled Base64URL for credential payloads; hand-reconstructed options objects; a browser-side attestation/assertion verifier (the `services` `Authn` concern); a bare rejected `Promise` escaping the `AuthFault` rail; reaching for the internal `identify*Error`/`isValidDomain`/`toAuthenticatorAttachment`/`toPublicKeyCredentialDescriptor` helpers (not barrel-exported)
