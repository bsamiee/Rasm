# [TS_SECURITY_API_SIMPLEWEBAUTHN_BROWSER]

`@simplewebauthn/browser` owns the client half of the passkey ceremony `authn/webauthn` composes: two `Promise` entries fold `navigator.credentials.create()`/`.get()` over the server-issued options JSON and return the response JSON to POST back. Capability probes gate the call, `WebAuthnAbortService` holds one ceremony live, and every rejection carries a coded `WebAuthnError` naming why an opaque `DOMException` fired.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@simplewebauthn/browser`
- package: `@simplewebauthn/browser` (MIT)
- module: dual ESM (`import` → `esm/index.js`) and CJS (`require` → `script/index.js`); one root entry, no subpaths
- runtime: `runtime:browser` — binds `navigator`/`window`; `@simplewebauthn/server` is the node/edge verifier half
- rail: `authn/webauthn` client ceremony

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: call options, the coded fault rail, and the response-union alias; `@simplewebauthn/server` (`.api/simplewebauthn-server.md`) owns the JSON wire vocabulary both halves share, decoded by one `Schema.Struct` per shape at the fetch boundary.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :------------------------ | :------------ | :---------------------------------------------------------------- |
|  [01]   | `StartRegistrationOpts`   | interface     | `optionsJSON` plus the `useAutoRegister` password-manager upgrade |
|  [02]   | `StartAuthenticationOpts` | interface     | `optionsJSON` plus the two conditional-UI autofill flags          |
|  [03]   | `WebAuthnError`           | class         | `Error` carrying `code` and the originating `DOMException` cause  |
|  [04]   | `WebAuthnErrorCode`       | union         | recovery discriminant an exhaustive match folds over              |
|  [05]   | `PublicKeyCredentialJSON` | union         | both response shapes under one POST handler                       |

[WEBAUTHN_ERROR_CODE]: `ERROR_CEREMONY_ABORTED` `ERROR_INVALID_DOMAIN` `ERROR_INVALID_RP_ID` `ERROR_INVALID_USER_ID_LENGTH` `ERROR_MALFORMED_PUBKEYCREDPARAMS` `ERROR_AUTHENTICATOR_GENERAL_ERROR` `ERROR_AUTHENTICATOR_MISSING_DISCOVERABLE_CREDENTIAL_SUPPORT` `ERROR_AUTHENTICATOR_MISSING_USER_VERIFICATION_SUPPORT` `ERROR_AUTHENTICATOR_PREVIOUSLY_REGISTERED` `ERROR_AUTHENTICATOR_NO_SUPPORTED_PUBKEYCREDPARAMS_ALG` `ERROR_AUTO_REGISTER_USER_VERIFICATION_FAILURE` `ERROR_PASSTHROUGH_SEE_CAUSE_PROPERTY`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the two stateful ceremonies and the probes, codecs, and abort guard bounding them

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :----------------------------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `startRegistration(StartRegistrationOpts)`                   | static   | attestation ceremony; `RegistrationResponseJSON` |
|  [02]   | `startAuthentication(StartAuthenticationOpts)`               | static   | assertion ceremony; `AuthenticationResponseJSON` |
|  [03]   | `browserSupportsWebAuthn() -> boolean`                       | static   | synchronous gate before offering a passkey       |
|  [04]   | `platformAuthenticatorIsAvailable() -> Promise<boolean>`     | static   | platform-authenticator probe                     |
|  [05]   | `browserSupportsWebAuthnAutofill() -> Promise<boolean>`      | static   | conditional-UI probe                             |
|  [06]   | `base64URLStringToBuffer(string) -> ArrayBuffer`             | static   | wire decode for a credential field               |
|  [07]   | `bufferToBase64URLString(ArrayBuffer) -> string`             | static   | wire encode for a credential field               |
|  [08]   | `WebAuthnAbortService.createNewAbortSignal() -> AbortSignal` | instance | arms the live-ceremony signal                    |
|  [09]   | `WebAuthnAbortService.cancelCeremony()`                      | instance | cancels the live ceremony                        |

- `startRegistration`/`startAuthentication`: two guard paths throw a plain `Error` ahead of classification — an unsupported browser and an incomplete autofill ceremony.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `WebAuthnAbortService` is a module singleton: a ceremony auto-arms a fresh `AbortSignal` and supersedes the prior one, so a client-router navigation calls `.cancelCeremony()`.
- Browser code collects unforgeable input only; challenge match, origin and RP-ID checks, and signature-counter defense belong to the RP half.
- Ceremony entries classify the opaque `DOMException` into a `WebAuthnError` before rejecting, so `.code` discriminates recovery where the DOM error name conflates user-cancel with timeout.

[STACKING]:
- `effect`(`.api/effect.md`): `Effect.tryPromise({ try, catch })` lifts each ceremony and the `catch` narrows the pre-coded `WebAuthnError` into one `Data.TaggedError`; `Match.value(err.code)` closed on `Match.exhaustive` folds recovery, and `browserSupportsWebAuthn()` gates the whole `Effect` so an unsupported browser short-circuits to a typed capability fault.
- `@simplewebauthn/server`(`.api/simplewebauthn-server.md`): `generateRegistrationOptions` output enters `startRegistration`, and the returned `RegistrationResponseJSON` feeds `verifyRegistrationResponse`; one `Schema.Struct` per JSON shape owns both crossings of the fetch seam.
- `@effect/platform-browser`(`.api/effect-platform-browser.md`): the ceremony runs inside the `BrowserRuntime.runMain` boot, the response POST rides `BrowserHttpClient.layerXMLHttpRequest` under the shared `host/net` retry policy, and a `BrowserStream.fromEventListenerDocument` visibility row defers the conditional-UI prompt on a backgrounded tab.
- `@oslojs/encoding`(`.api/oslojs-encoding.md`): `bufferToBase64URLString`/`base64URLStringToBuffer` cover the browser-side `Base64URLString` fields, leaving `encodeBase64url`/`decodeBase64url` the `sign/` owner for credential material at rest — one alphabet, two runtimes, no cross-import.
- `authn/webauthn`: its browser subpath composes probe → ceremony → POST as one `Effect` pipeline, binding the abort service to the client router's navigation stream.

[LOCAL_ADMISSION]:
- Resolves only inside the browser-safe `authn/webauthn` subpath; a node composition binds `@simplewebauthn/server`.

[RAIL_LAW]:
- Package: `@simplewebauthn/browser`
- Owns: the `navigator.credentials` registration and authentication ceremonies, the capability probes, the credential-field base64url codecs, the single-ceremony `WebAuthnAbortService`, and the `WebAuthnError`/`WebAuthnErrorCode` classification rail
- Accept: `Effect.tryPromise` over each ceremony, exhaustive `Match` on `WebAuthnErrorCode`, one `Schema` per JSON shape across the fetch seam, a probe as the ceremony gate, verification delegated to `@simplewebauthn/server`
- Reject: a raw `DOMException` in the error channel, hand-parsed credential fields, attestation or assertion verified in the browser, a ceremony call with no capability probe
