# [TS_SECURITY_API_OSLOJS_ENCODING]

`@oslojs/encoding` is the pure-TypeScript, dependency-free byte↔string codec `sign` pairs with `@oslojs/crypto`: the digest/MAC bytes one produces, this renders to a wire string and parses back. It is one parameterized matrix — alphabet (`hex` · `base32` · `base64` · `base64url`) × case (`lower` · `upper`) × padding (`padded` · `noPadding`) × direction (`encode` · `decode` · `decodeIgnorePadding`) — not a bag of unrelated functions. Every `decode*` is *total*: it throws on malformed input rather than returning a partial value, so a boundary lift is one `Effect.try` with a typed fault. It is admitted over `effect`'s built-in `Encoding` module precisely because it owns base32 (the TOTP/authenticator + Crockford-family alphabet `Encoding` lacks) and the full padding/case/url variant grid with a uniform throwing contract — not because it re-implements base64.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@oslojs/encoding`
- package: `@oslojs/encoding`
- license: `MIT`
- module: ESM-only (`"type": "module"`); single root export (`dist/index.d.ts`) — every codec is a named import from `@oslojs/encoding`
- effect-boundary: pure synchronous functions — `encode*` is total (`Effect.sync`/inline), `decode*` throws on malformed input (`Effect.try`). `.api/effect.md`
- catalog-verdict: KEEP — owns base32 (absent from `effect` `Encoding`) plus the case/padding/url variant grid with a uniform total-decode contract; pairs 1:1 with `@oslojs/crypto` digest bytes
- runtime: `runtime:neutral` — pure JS, no `node:*`; safe in a `runtime:browser` composition
- exports: `hex` · `base32` · `base64` · `base64url` codecs (single entry surface, no subpaths)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the codec matrix — one axis set, not a function bag
- rail: sign
- Every member is `(bytes: Uint8Array) => string` (encode) or `(encoded: string) => Uint8Array` (decode). The variation is axis *values*, so a call is chosen by (alphabet, case, padding, direction), never by a bespoke name. The `decodeIgnorePadding`/`…urlIgnorePadding` arm is the lenient parse for inputs whose padding was stripped in transit.

| [INDEX] | [ALPHABET]  | [ENCODE_ROWS]                                                                          | [DECODE_ROWS]                                      | [CONSUMER_BOUNDARY]                                                                                            |
| :-----: | :---------- | :------------------------------------------------------------------------------------- | :------------------------------------------------- | :------------------------------------------------------------------------------------------------------------- |
|  [01]   | `hex`       | `encodeHexLowerCase` · `encodeHexUpperCase`                                            | `decodeHex`                                        | digest/MAC at-rest string; `@node-rs/argon2` peppers, fingerprints                                             |
|  [02]   | `base32`    | `encodeBase32UpperCase` · `…UpperCaseNoPadding` · `…LowerCase` · `…LowerCaseNoPadding` | `decodeBase32` · `decodeBase32IgnorePadding`       | the RFC-4648/authenticator alphabet class `effect` cannot serve; backs the optional otplib `Base32Plugin` swap |
|  [03]   | `base64`    | `encodeBase64` · `encodeBase64NoPadding`                                               | `decodeBase64` · `decodeBase64IgnorePadding`       | binary blob at-rest (PEM DER, attestation objects)                                                             |
|  [04]   | `base64url` | `encodeBase64url` · `encodeBase64urlNoPadding`                                         | `decodeBase64url` · `decodeBase64urlIgnorePadding` | WebAuthn `Base64URLString`, apikey prefix, session/recovery material; JOSE/JWS segment stays in `jose`         |

[PUBLIC_TYPE_SCOPE]: deprecated aliases — never author against these
- rail: sign
- `encodeBase32`/`encodeBase32NoPadding` are `@deprecated` re-spellings of the `UpperCase` rows, kept for source compatibility. Author against the explicit case-named rows so the produced alphabet is unambiguous at the call site.

| [INDEX] | [SYMBOL]                                 | [REPLACED_BY]                          | [CONSUMER_BOUNDARY]                  |
| :-----: | :--------------------------------------- | :------------------------------------- | :----------------------------------- |
|  [01]   | `encodeBase32` / `encodeBase32NoPadding` | `encodeBase32UpperCase` / `…NoPadding` | avoid — case-ambiguous retired alias |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the two directions `sign` composes
- rail: sign
- Encode is total; decode throws. The digest→string→digest round-trip that stores and re-compares a MAC/API-key hash is the canonical pairing with `@oslojs/crypto`.

| [INDEX] | [SURFACE]                                                                                        | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                                    |
| :-----: | :----------------------------------------------------------------------------------------------- | :------------- | :--------------------------------------------------------------------- |
|  [01]   | `encodeHexLowerCase(bytes)` / `encodeBase64url(bytes)` / `encodeBase32UpperCaseNoPadding(bytes)` | render (total) | digest/MAC/secret → at-rest or wire string                             |
|  [02]   | `decodeHex(s)` / `decodeBase64url(s)` / `decodeBase32(s)`                                        | parse (throws) | stored string → bytes before `constantTimeEqual`; lift in `Effect.try` |
|  [03]   | `decodeBase64urlIgnorePadding(s)` / `decodeBase32IgnorePadding(s)`                               | lenient parse  | wire input whose padding was stripped in transit                       |

## [04]-[IMPLEMENTATION_LAW]

[CODEC_TOPOLOGY]:
- one matrix, not a function bag: choose a codec by (alphabet, case, padding, direction). The 24-odd exported names are the enumeration of that axis product; author code reads the axis values off the domain need (WebAuthn/apikey ⇒ base64url-noPadding, TOTP ⇒ base32, digest-at-rest ⇒ hex-lower), never invents a wrapper.
- total decode: `decode*` throws on any invalid character or length. That throw is the boundary — one `Effect.try` converts it to a typed fault; there is no partial/`Option` decode to branch on.
- padding discipline: the `NoPadding` encode + `IgnorePadding` decode pair is the wire-safe combination (WebAuthn/JWS wire strips `=`); the padded pair is the at-rest/PEM combination. Never mix a padded encode with a strict decode across a transport that rewrites padding.

[INTEGRATION_LAW]:
- Stack with `@oslojs/crypto` (`.api/oslojs-crypto.md`): the crypto sibling emits `Uint8Array` digests/MACs/random bytes; this package is their string rendering. A stored API-key hash is `encodeHexLowerCase(hmac(SHA256, pepper, key))`; verification is `decodeHex(stored)` → `constantTimeEqual`. The two form the single byte↔string seam `sign` exposes.
- Stack with `.api/effect.md` — chosen over `Encoding` and behind `Schema`: `effect`'s `Encoding.encodeBase64`/`decodeHex` return `Either` and own no base32; this package owns base32 and the case/url/padding grid with a throwing contract. When the crossing lives inside a boundary `Schema`, prefer the `Schema.Uint8ArrayFromBase64`/`Schema.StringFromHex` codecs so the transform is declarative and derivation-visible; drop to these imperative functions for the base32 rows and for MAC material that never enters a `Schema`.
- Stack with `authn/apikey` + `session/token` across the `sign/` seam, bounded against `sign/jwt` (`.api/jose.md`): base64url-noPadding is the non-JOSE wire alphabet — the `authn/apikey` prefix index, opaque `session/token`/recovery material, and WebAuthn `Base64URLString` attestation/credential bytes rendered in `sign/` and consumed across the seam; the JOSE/JWS compact segment is NOT oslo's — `jose` owns that base64url internally (`jose/base64url` `encode`/`decode` under `CompactSign`/`SignJWT`), so oslo never encodes a JWS segment and never receives one to decode. base32 is the provisioning-secret alphabet of the otpauth:// URI, reached by `authn/otp` only through the optional `Base32Plugin` swap (otplib's bundled scure is the default, `.api/otplib.md`). All secret/digest strings stay inside a `Redacted` carrier once rendered.
- Stack with `@simplewebauthn/browser` (`.api/simplewebauthn-browser.md`): the browser package ships its own `base6 catalogURLStringToBuffer`/`bufferToBase6 catalogURLString` for the `runtime:browser` half; this package is the `sign`-side base6 catalogurl for the server-verified attestation/credential bytes at rest — same alphabet, opposite runtime, no cross-import.

[LOCAL_ADMISSION]:
- imported only inside `sign/` subpaths; a non-`sign` rail that needs a codec receives already-encoded material across the seam, never this import.
- pure-JS and runtime-neutral: no `node:*`; safe in any composition. The `sign/` node caveat is `@node-rs/argon2`, not this package.
- author the case-explicit rows: never the `@deprecated` `encodeBase32`/`encodeBase32NoPadding` aliases.

[RAIL_LAW]:
- Package: `@oslojs/encoding`
- Owns: the hex/base32/base64/base64url codec matrix with case + padding + lenient-decode variants and a uniform total-decode contract
- Accept: codec selection by (alphabet, case, padding, direction) axis values, base32 as the row `effect` cannot serve, `Effect.try` around `decode*`, `Schema.*FromBase64`/`FromHex` for in-`Schema` crossings, pairing 1:1 with `@oslojs/crypto` bytes, base64url-noPadding for the WebAuthn/apikey/session wire (JOSE/JWS segments stay in `jose`)
- Reject: a bespoke per-format wrapper over the matrix, the `@deprecated` case-ambiguous base32 aliases, a padded-encode/strict-decode mismatch across a padding-rewriting transport, re-implementing base64 that `effect` `Encoding` already owns for non-base32 in-`Schema` cases, encoding a JOSE/JWS compact segment here (`jose` owns JOSE base64url internally), any import outside `sign/`
