# [@simplewebauthn/browser] — the browser-half passkey ceremony (navigator.credentials) over the browser-safe subpath; admitted in authn only

`@simplewebauthn/browser` is the client half of the WebAuthn passkey ceremony `authn/webauthn` composes: it wraps `navigator.credentials.create()`/`.get()` into two `Promise` entries (`startRegistration`/`startAuthentication`) that take the server-issued options JSON and return the response JSON to POST back. It is browser-only (`navigator`/`window`), so it is a `runtime:browser` package fenced by `proof/gauge`, and it is exactly one endpoint of a two-package seam — its `PublicKeyCredentialCreationOptionsJSON`/`RegistrationResponseJSON` type vocabulary is the same wire shape `@simplewebauthn/server` (`.api/simplewebauthn-server.md`) generates and verifies. At v13 the ceremony entries take a single **options object** (`{ optionsJSON }`), not the pre-12 positional argument — a design page citing the positional form is a phantom. It owns the capability probes that gate the ceremony, the base64url codecs, the single-ceremony `WebAuthnAbortService`, and the `WebAuthnError` rail that intuits *why* a `DOMException` fired.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@simplewebauthn/browser`
- package: `@simplewebauthn/browser`
- version: `13.3.0`
- license: `MIT`
- module: dual ESM (`import` → `esm/index.js`) + CJS (`require` → `script/index.js`); the browser-safe entry (`@simplewebauthn/server` is the separate node/edge verifier half)
- effect-boundary: `startRegistration`/`startAuthentication` return `Promise` → `Effect.tryPromise({ try, catch })`; the reject value is already a coded `WebAuthnError` (classified internally), so the `catch` narrows on `.code` rather than re-classifying. `.api/effect.md`
- catalog-verdict: KEEP — the one `navigator.credentials` ceremony wrapper; the abort service + error-intuition rail are capability WebCrypto/raw `navigator` do not provide
- runtime: `runtime:browser` — uses `navigator`/`window`; edge-ledger banned inside `runtime:node`, mirrors the `@effect/platform-browser` subpath-purity law
- API_VERSION: v13 ceremony entries take a single `{ optionsJSON, … }` object; the pre-12 positional form is banned

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the JSON wire vocabulary — the seam shared with `@simplewebauthn/server`
- rail: authn/webauthn
- These are the exact shapes the server half generates (options) and verifies (response). The browser never mints or verifies them — it receives the options from the server, invokes the authenticator, and returns the response. A `Schema` at the fetch boundary decodes both directions.

| [INDEX] | [SYMBOL]                                                                        | [TYPE_FAMILY]   | [CONSUMER / BOUNDARY]                                        |
| :-----: | :------------------------------------------------------------------------------ | :-------------- | :---------------------------------------------------------- |
|  [01]   | `PublicKeyCredentialCreationOptionsJSON` / `PublicKeyCredentialRequestOptionsJSON` | server → client | input to `startRegistration`/`startAuthentication`; issued by the server generators |
|  [02]   | `RegistrationResponseJSON` / `AuthenticationResponseJSON`                        | client → server | output POSTed back to `verifyRegistrationResponse`/`verifyAuthenticationResponse` |
|  [03]   | `AuthenticatorAttestationResponseJSON` / `AuthenticatorAssertionResponseJSON`    | nested response | the credential response payloads inside the two response JSONs |
|  [04]   | `Base64URLString` / `AuthenticatorTransportFuture` (`'ble'\|'hybrid'\|'internal'\|'nfc'\|'usb'\|…`) | wire scalar | base64url credential fields; transport hints for the descriptor |
|  [05]   | `WebAuthnCredential` / `PublicKeyCredentialHint` / `CredentialDeviceType` / `AttestationFormat` | domain vocab | the stored-credential shape and the ceremony hint/format enums |

[PUBLIC_TYPE_SCOPE]: the ceremony option shapes and the error rail
- rail: authn/webauthn
- `StartRegistrationOpts`/`StartAuthenticationOpts` are the exact call-option types; `WebAuthnError` is the tagged fault whose `code` (12-arm union) discriminates the failure the raw `DOMException` obscures.

| [INDEX] | [SYMBOL]                                                                        | [TYPE_FAMILY]  | [CONSUMER / BOUNDARY]                                        |
| :-----: | :------------------------------------------------------------------------------ | :------------- | :---------------------------------------------------------- |
|  [01]   | `StartRegistrationOpts` `{ optionsJSON; useAutoRegister? }`                      | call option    | the `startRegistration` argument shape                       |
|  [02]   | `StartAuthenticationOpts` `{ optionsJSON; useBrowserAutofill?; verifyBrowserAutofillInput? }` | call option | the `startAuthentication` argument shape                     |
|  [03]   | `WebAuthnError` (`extends Error`, `code: WebAuthnErrorCode`, `cause`)            | tagged fault   | the pre-classified reject value the ceremony `Promise` throws; the `catch` arm of the ceremony `Effect` |
|  [04]   | `WebAuthnErrorCode` (`'ERROR_CEREMONY_ABORTED'` \| `'ERROR_INVALID_RP_ID'` \| `'ERROR_AUTHENTICATOR_PREVIOUSLY_REGISTERED'` \| … 12 arms) | error discriminant | `Match` on `.code` → tagged domain fault + user-facing message |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the two ceremonies — the only stateful calls (object-argument form)
- rail: authn/webauthn
- Each takes the server-issued options JSON and returns the response JSON. `useBrowserAutofill` opts into conditional-UI (passkey autofill on a login field); `useAutoRegister` silently upgrades a just-signed-in password to a passkey. Both auto-arm the abort service.

| [INDEX] | [SURFACE]                                                                                             | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :--------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `startRegistration({ optionsJSON, useAutoRegister? }): Promise<RegistrationResponseJSON>`             | register       | attestation ceremony; options from server `generateRegistrationOptions` |
|  [02]   | `startAuthentication({ optionsJSON, useBrowserAutofill?, verifyBrowserAutofillInput? }): Promise<AuthenticationResponseJSON>` | authenticate | assertion ceremony; options from server `generateAuthenticationOptions` |

[ENTRYPOINT_SCOPE]: capability probes, codecs, and the ceremony abort guard
- rail: authn/webauthn
- The probes gate the ceremony at the `ui` edge (feature-detect before offering passkeys); the codecs bridge `ArrayBuffer`↔`Base64URLString`; the abort service enforces one live ceremony. The DOM-exception→`WebAuthnError` classification is internal to `startRegistration`/`startAuthentication` (the `identify*Error` mappers are not root-exported), so the rejected `Promise` already carries a coded `WebAuthnError`.

| [INDEX] | [SURFACE]                                                                                             | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :--------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `browserSupportsWebAuthn(): boolean` / `platformAuthenticatorIsAvailable(): Promise<boolean>` / `browserSupportsWebAuthnAutofill(): Promise<boolean>` | capability probe | `ui` feature-gate before offering passkey / conditional-UI |
|  [02]   | `base64URLStringToBuffer(s): ArrayBuffer` / `bufferToBase64URLString(buf): string`                   | wire codec     | `ArrayBuffer`↔base64url for hand-built credential fields  |
|  [03]   | `WebAuthnAbortService.createNewAbortSignal()` / `.cancelCeremony()`                                   | ceremony guard | single-live-ceremony law; cancel on client-route change   |

## [04]-[IMPLEMENTATION_LAW]

[CEREMONY_TOPOLOGY]:
- two ceremonies, object argument: `startRegistration`/`startAuthentication` are the only stateful entries; both take a single options object at v13. Never the pre-12 positional form.
- one live ceremony: `WebAuthnAbortService` is a singleton — each ceremony auto-arms a fresh `AbortSignal` and a new call cancels the prior. This is the single-ceremony law (kin to the `@effect/platform-browser` one-boot law); a client-router navigation calls `.cancelCeremony()`.
- the browser never verifies: it invokes the authenticator and returns the response. Attestation/assertion *verification* — challenge match, origin/RP-ID check, signature-counter — is `@simplewebauthn/server` in the node/edge half. The browser half is unforgeable-input collection only.
- error intuition, not raw throw: `navigator.credentials` rejects with an opaque `DOMException` (`NotAllowedError` covers both user-cancel and timeout). The ceremony entries classify it internally into a `WebAuthnError` with a `WebAuthnErrorCode` and re-throw that, so the rejected `Promise` carries the coded fault — except two plain-`Error` guard throws (`'WebAuthn is not supported in this browser'`, `'…​ was not completed'`) the caller must still handle.

[INTEGRATION_LAW]:
- Stack with `.api/effect.md` rails: `Effect.tryPromise({ try: () => startRegistration({ optionsJSON }), catch: (e) => e instanceof WebAuthnError ? new WebAuthnCeremony({ code: e.code, cause: e }) : new WebAuthnCeremony({ code: "ERROR_PASSTHROUGH_SEE_CAUSE_PROPERTY", cause: e }) })` lifts the ceremony and normalizes the pre-classified `WebAuthnError` (and the two plain-`Error` guards) into one `Data.TaggedError("WebAuthnCeremony")`; dispatch recovery with `Match.value(err.code).pipe(Match.when("ERROR_CEREMONY_ABORTED", …), …, Match.exhaustive)`. Gate the whole `Effect` on `browserSupportsWebAuthn()` so an unsupported browser short-circuits to a typed capability fault before the call.
- Stack with the browser↔server seam (`.api/simplewebauthn-server.md`): the server issues `PublicKeyCredentialCreationOptionsJSON` (with the challenge parked in a short-lived `session` store), the browser runs the ceremony, and the returned `RegistrationResponseJSON` POSTs back to `verifyRegistrationResponse`. A single `Schema.Struct` per JSON shape decodes both the inbound options and the outbound response at the fetch boundary — one owner schema, two crossings, no hand-parsed credential fields.
- Stack with `@effect/platform-browser` (`.api/effect-platform-browser.md`): the ceremony runs inside the `BrowserRuntime.runMain` boot; the response POST rides the `BrowserHttpClient.layerXMLHttpRequest`/`FetchHttpClient` client with the shared `host/net` retry/timeout policy. Capability probes feed a `BrowserStream.fromEventListener*` connectivity/visibility row so a backgrounded tab defers the conditional-UI prompt.
- Stack with `@oslojs/encoding` base64url (`.api/oslojs-encoding.md`): the browser ships its own `base64URLStringToBuffer`/`bufferToBase64URLString` for the `runtime:browser` half (no node-subpath pull); the `sign`-side base64url for the server-verified credential public key at rest is the encoding sibling — same alphabet, opposite runtime, no cross-import.

[LOCAL_ADMISSION]:
- imported only inside the `authn/webauthn` browser-safe subpath; a node/edge rail importing it is the defect `proof/gauge` catches — the node half is `@simplewebauthn/server`.
- `runtime:browser` purity: banned inside `runtime:node`; the ceremony belongs to a browser composition, its verify counterpart to the server composition.
- probe before prompt: `browserSupportsWebAuthn()`/`browserSupportsWebAuthnAutofill()` gate the ceremony; never call a ceremony entry without the capability guard.

[RAIL_LAW]:
- Package: `@simplewebauthn/browser`
- Owns: the `navigator.credentials` registration/authentication ceremonies, the capability probes, the base64url codecs, the single-ceremony `WebAuthnAbortService`, the internally-classified `WebAuthnError`/`WebAuthnErrorCode` rail, and the browser half of the JSON wire vocabulary
- Accept: the v13 `{ optionsJSON }` object form, `Effect.tryPromise` catching the pre-thrown `WebAuthnError` (guarding the two plain-`Error` cases), `Match` on `WebAuthnErrorCode`, a `Schema` per JSON shape across the fetch seam, the probes as the ceremony gate, the abort service as the single-ceremony law, verification delegated to `@simplewebauthn/server`
- Reject: the pre-12 positional `startRegistration` form (a phantom), verifying attestation/assertion in the browser (it is the server half), the raw `DOMException` in the error channel instead of a coded `WebAuthnError`, hand-parsed credential fields instead of the shared JSON schema, calling a ceremony without a capability probe, any import inside `runtime:node`/outside `authn/`
