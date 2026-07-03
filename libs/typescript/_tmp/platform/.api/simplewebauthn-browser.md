# [API_CATALOGUE] @simplewebauthn/browser

`@simplewebauthn/browser` provides browser-side WebAuthn ceremony execution: `startRegistration` drives `navigator.credentials.create`, `startAuthentication` drives `navigator.credentials.get`, and a set of helper functions covers feature detection, abort control, and Base64URL encoding. Key JSON types (`PublicKeyCredentialCreationOptionsJSON`, `RegistrationResponseJSON`, `PublicKeyCredentialRequestOptionsJSON`, `AuthenticationResponseJSON`) are shared with the server package and serve as the wire boundary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@simplewebauthn/browser`
- package: `@simplewebauthn/browser`
- module: `@simplewebauthn/browser` (barrel from `esm/index.js`)
- asset: browser runtime library
- rail: auth

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: ceremony method options and responses (shared with server)
- rail: auth

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY] | [RAIL]                             |
| :-----: | :--------------------------------------- | :------------ | :--------------------------------- |
|  [01]   | `PublicKeyCredentialCreationOptionsJSON` | interface     | registration options from server   |
|  [02]   | `RegistrationResponseJSON`               | interface     | registration response to server    |
|  [03]   | `PublicKeyCredentialRequestOptionsJSON`  | interface     | authentication options from server |
|  [04]   | `AuthenticationResponseJSON`             | interface     | authentication response to server  |
|  [05]   | `AuthenticatorAttestationResponseJSON`   | interface     | attestation response payload       |
|  [06]   | `AuthenticatorAssertionResponseJSON`     | interface     | assertion response payload         |
|  [07]   | `PublicKeyCredentialDescriptorJSON`      | interface     | credential descriptor JSON form    |
|  [08]   | `AuthenticatorTransportFuture`           | string union  | extended transport vocabulary      |
|  [09]   | `Base64URLString`                        | type alias    | `string` branded for Base64URL     |
|  [10]   | `PublicKeyCredentialHint`                | string union  | registration flow hint             |

[PUBLIC_TYPE_SCOPE]: helper and error types
- rail: auth

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [RAIL]                             |
| :-----: | :-------------- | :------------ | :--------------------------------- |
|  [01]   | `WebAuthnError` | error class   | ceremony failure with code + cause |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: ceremony methods
- rail: auth

```ts
// esm/methods/startRegistration.js (function signature from source)
export async function startRegistration(options: {
  optionsJSON: PublicKeyCredentialCreationOptionsJSON;
  useAutoRegister?: boolean;
}): Promise<RegistrationResponseJSON>

// esm/methods/startAuthentication.js
export async function startAuthentication(options: {
  optionsJSON: PublicKeyCredentialRequestOptionsJSON;
  useBrowserAutofill?: boolean;
  verifyBrowserAutofillInput?: boolean;
}): Promise<AuthenticationResponseJSON>
```

[ENTRYPOINT_SCOPE]: feature detection helpers
- rail: auth

```ts
// esm/helpers/browserSupportsWebAuthn.js
export function browserSupportsWebAuthn(): boolean

// esm/helpers/platformAuthenticatorIsAvailable.js
export function platformAuthenticatorIsAvailable(): Promise<boolean>

// esm/helpers/browserSupportsWebAuthnAutofill.js
export function browserSupportsWebAuthnAutofill(): Promise<boolean>
```

[ENTRYPOINT_SCOPE]: encoding utilities
- rail: auth

```ts
// esm/helpers/base64URLStringToBuffer.js
export function base64URLStringToBuffer(base64URLString: string): ArrayBuffer

// esm/helpers/bufferToBase64URLString.js
export function bufferToBase64URLString(buffer: ArrayBuffer): string
```

[ENTRYPOINT_SCOPE]: abort service singleton
- rail: auth

```ts
// esm/helpers/webAuthnAbortService.js
// Exported as singleton instance of BaseWebAuthnAbortService
export const WebAuthnAbortService: {
  createNewAbortSignal(): AbortSignal;
  cancelCeremony(): void;
}
```

[ENTRYPOINT_SCOPE]: WebAuthnError
- rail: auth

```ts
// esm/helpers/webAuthnError.js
export class WebAuthnError extends Error {
  code: unknown;
  constructor(options: {
    message: string;
    code: unknown;
    cause: Error;
    name?: string;
  });
}
```

## [04]-[IMPLEMENTATION_LAW]

[AUTH_TOPOLOGY]:
- `startRegistration` calls `navigator.credentials.create`; `startAuthentication` calls `navigator.credentials.get`
- both methods accept JSON options from the server and return JSON responses suitable for sending back to the server
- `useAutoRegister` enables silent passkey creation via the active password manager
- `useBrowserAutofill` initializes conditional UI; `verifyBrowserAutofillInput` guards for a matching `<input autocomplete="webauthn">` element
- `WebAuthnAbortService.cancelCeremony()` aborts any in-progress ceremony; call before navigation or re-invocation

[LOCAL_ADMISSION]:
- `browserSupportsWebAuthn()` is the synchronous gate; `platformAuthenticatorIsAvailable()` is the async gate for Touch ID / Windows Hello UI decisions.
- Pass server-generated `PublicKeyCredentialCreationOptionsJSON` directly into `startRegistration`; never reconstruct the options object manually.
- Pass `RegistrationResponseJSON` and `AuthenticationResponseJSON` directly to the server verification endpoints without re-encoding.

[RAIL_LAW]:
- Package: `@simplewebauthn/browser`
- Owns: browser-side WebAuthn ceremony execution, Base64URL encoding, feature detection, and abort control
- Accept: JSON option objects from `@simplewebauthn/server`; return JSON response objects to the server
- Reject: direct `navigator.credentials` calls bypassing this library, hand-rolled Base64URL encoding for credential payloads
