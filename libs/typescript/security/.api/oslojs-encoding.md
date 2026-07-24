# [TS_SECURITY_API_OSLOJS_ENCODING]

`@oslojs/encoding` renders the bytes `@oslojs/crypto` mints into wire strings and parses them back. One axis product — alphabet × case × padding × direction — spans the surface, so a caller selects axis values off the domain need rather than a bespoke name; every `decode*` throws on malformed input, making the parse boundary one `Effect.try`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@oslojs/encoding`
- package: `@oslojs/encoding` (MIT)
- module: ESM-only, one root export; every codec is a named import from the package root, no subpaths
- runtime: `runtime:neutral` — pure JS, zero `node:*`, composes inside a `runtime:browser` build
- rail: sign — the byte↔string half of every digest, MAC, and secret the folder mints

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the two call shapes every export inhabits; the package exports functions alone and names no type.

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                              |
| :-----: | :----------------------- | :------------ | :-------------------------------------------------------- |
|  [01]   | `(Uint8Array) -> string` | delegate      | render bytes to the alphabet's wire string, total         |
|  [02]   | `(string) -> Uint8Array` | delegate      | parse a wire string to bytes, throwing on malformed input |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: every export grouped by alphabet; a name spells its own axis values and the two shapes above fix its signature.

[HEX]: `encodeHexLowerCase` `encodeHexUpperCase` `decodeHex`
[BASE32]: `encodeBase32UpperCase` `encodeBase32UpperCaseNoPadding` `encodeBase32LowerCase` `encodeBase32LowerCaseNoPadding` `decodeBase32` `decodeBase32IgnorePadding`
[BASE64]: `encodeBase64` `encodeBase64NoPadding` `decodeBase64` `decodeBase64IgnorePadding`
[BASE64URL]: `encodeBase64url` `encodeBase64urlNoPadding` `decodeBase64url` `decodeBase64urlIgnorePadding`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Domain need selects the axis values — WebAuthn and apikey wire take base64url-noPadding, TOTP provisioning takes base32, digest-at-rest takes hex-lower — and the selected export is the call.
- `decode*` throws on any invalid character or length, so one `Effect.try` at that call converts the throw to a typed fault; the surface carries no partial or `Option` arm to branch on.
- `NoPadding` encode pairs with `IgnorePadding` decode across a transport that rewrites padding, and the padded pair holds at rest.

[STACKING]:
- `@oslojs/crypto`(`.api/oslojs-crypto.md`): its `Uint8Array` output renders here — a stored API-key hash is `encodeHexLowerCase(hmac(SHA256, pepper, key))`, and verification parses the stored string back through `decodeHex` into `constantTimeEqual`.
- `effect`(`.api/effect.md`): a crossing inside a boundary `Schema` rides `Schema.Uint8ArrayFromBase64`, `Schema.Uint8ArrayFromBase64Url`, or `Schema.StringFromHex` so the transform stays declarative; these imperative codecs carry the base32 rows and the MAC material no `Schema` sees.
- `jose`(`.api/jose.md`): `jose` renders every JWS compact segment through its own internal `base64url`, so this package never encodes or decodes a JOSE segment; base64url here serves the opaque non-JOSE wire alone.
- `otplib`(`.api/otplib.md`): base32 renders the `otpauth://` provisioning secret only through the optional `Base32Plugin` swap, `ScureBase32Plugin` holding the default slot.
- `@simplewebauthn/browser`(`.api/simplewebauthn-browser.md`): `bufferToBase64URLString`/`base64URLStringToBuffer` render credential fields inside the `runtime:browser` half; this package renders the server-verified bytes at rest — same alphabet, opposite runtime, no cross-import.
- `@simplewebauthn/server`(`.api/simplewebauthn-server.md`): the verified `WebAuthnCredential.publicKey` bytes and the `./helpers` `iso.isoBase64URL` output render through `encodeBase64urlNoPadding` and parse back through `decodeBase64urlIgnorePadding` before the journalled credential re-enters `verifyAuthenticationResponse`.
- `sign/`: every mint the folder exposes ends on one of these codecs — `authn/apikey` prefix index, opaque `session/token` and recovery material, WebAuthn credential bytes — and each rendered string stays inside a `Redacted` carrier.

[LOCAL_ADMISSION]:
- `sign/` subpaths hold the sole import; every other rail receives already-encoded material across the folder seam.

[RAIL_LAW]:
- Package: `@oslojs/encoding`
- Owns: the hex, base32, base64, and base64url matrix with its case, padding, and lenient-decode axes under one throwing decode contract
- Accept: codec selection by axis values, base32 as the row `effect` `Encoding` never carries, `Effect.try` around `decode*`, `Schema.*FromBase64`/`FromHex` for in-`Schema` crossings, `@oslojs/crypto` bytes as the 1:1 input, base64url-noPadding for the WebAuthn, apikey, and session wire
- Reject: a bespoke per-format wrapper over the matrix, a padded-encode against a strict decode across a padding-rewriting transport, re-implementing the base64 rows `effect` `Encoding` already owns for in-`Schema` non-base32 crossings, rendering a JOSE compact segment, any import outside `sign/`
