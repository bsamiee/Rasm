# [TS_SECURITY_API_JOSE]

`jose` is the one JOSE owner the `security/sign/jwt` design composes — JWS signing/verification, the JWT claim-set profile over both JWS and JWE, JWE encryption/decryption, JWK/JWKS management with remote key rotation, and key import/export/generation — all over the WebCrypto API with zero runtime dependencies (`cryptoRuntime === "WebCryptoAPI"`). Its surface collapses along three axes rather than proliferating operations: the crypto op is {sign, verify, encrypt, decrypt}; the token profile is {raw JWS/JWE, JWT claim-set (`SignJWT`/`jwtVerify`, `EncryptJWT`/`jwtDecrypt`), unsecured}; and the serialization is {Compact, Flattened, General} — three wire forms of ONE sign/verify (and one encrypt/decrypt), not three concepts. The JWT builders share the fluent `ProduceJWT` claim-setter contract; `jwtVerify` overloads on a static key versus a `createRemoteJWKSet` resolver, and that resolver — with its `reload`/`fresh`/`coolingDown` state, the `jwksCache` symbol for cloud KV persistence, and the `customFetch` symbol for retry/proxy transport — IS the key-rotation seam. Every failure is a member of the closed `JOSEError` family discriminated by a stable `code`. The design admits `jose` in `sign/` only; `decodeJwt`/`decodeProtectedHeader` are unsafe peeks the boundary uses to route by header before verification, never as verification.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `jose`
- package: `jose` (MIT, © Filip Skokan)
- module format: ESM (`type: module`, `sideEffects: false`); a rich `exports` map serves per-concern subpaths — `jose/jwt/sign`, `jose/jwt/verify`, `jose/jwks/remote`, `jose/key/import`, `jose/errors`, `jose/base64url` — and the flat root import is equivalent under `sideEffects: false` tree-shaking, so the design imports from the root and a browser-safe `sign` bundle still never carries the unused surface
- runtime target: isomorphic (node ≥ WebCrypto, bun, deno, workerd, browser); zero runtime dependencies; no native addon — catalog-bound dropped the Node `crypto` build, so a single WebCrypto path serves every runtime and `sign/jwt.ts` is host-neutral despite the folder's node-only default
- asset: pure-TypeScript runtime library (`.js` + `.d.ts`); `JWTPayload` is open (`[propName: string]: unknown`), so a `Schema` decode is the real gate on the verified claim set
- rail: `security/sign` — the token-crypto owner (admitted in `sign/` only; catalogued at the folder tier)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: claims, keys, and the verification-policy axis
- rail: boundaries

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER] |
|:-----: |:-------------------------------------------------------------- |:---------------- |:--------------------------------------------------------------------- |
| [01] | `JWTPayload` (`iss`/`sub`/`aud`/`jti`/`nbf`/`exp`/`iat` + open) | claim set | `sign/jwt` → `session/token` — open by design; `Schema.decodeUnknown` brands it before use |
| [02] | `JWK` / `JWK_RSA_Private` / `JWK_EC_Public` / `JWK_oct` / `JWK_OKP_*` / `JSONWebKeySet` | key material | `sign/jwt` — key shapes for import/export/thumbprint; the JWKS is the verification key set |
| [03] | `CryptoKey` / `KeyObject` | runtime key handle | `sign/jwt` — the imported non-extractable handle held behind a `Context.Tag`, never re-imported per call |
| [04] | `JWTVerifyOptions` (= `VerifyOptions` + `JWTClaimVerificationOptions`) | verify policy | `sign/jwt` — one options row carries `algorithms` allow-list + every claim gate; the parameterized verify axis |
| [05] | `JWTClaimVerificationOptions` (`issuer`/`audience`/`subject`/`maxTokenAge`/`clockTolerance`/`requiredClaims`/`typ`/`currentDate`) | claim gate | `sign/jwt` — declarative claim checks the library enforces so the design never re-derives `exp`/`aud` by hand |
| [06] | `ProduceJWT` | builder contract | `sign/jwt` — the shared fluent claim-setter `SignJWT` and `EncryptJWT` both implement |

[PUBLIC_TYPE_SCOPE]: the closed fault family (discriminated by `code`)
- rail: rails-and-effects

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER] |
|:-----: |:-------------------------------------------------------------- |:---------------- |:--------------------------------------------------------------------- |
| [01] | `errors.JOSEError` (`static code` / `code`) | fault base | `sign/jwt` — the root the design maps to one tagged union by `code`; `Match`/`catchTags` on the projection |
| [02] | `errors.JWTExpired` / `JWTClaimValidationFailed` (`payload`, `claim`, `reason`) | claim fault | `sign/jwt` → `session/token` — expired/invalid claim; `payload` retained for the re-auth decision |
| [03] | `errors.JWSSignatureVerificationFailed` / `JWSInvalid` / `JWTInvalid` / `JOSEAlgNotAllowed` | signature fault | `sign/jwt` — untrusted token; folds to a single reject-and-401 arm |
| [04] | `errors.JWKSNoMatchingKey` / `JWKSMultipleMatchingKeys` / `JWKSTimeout` / `JWKSInvalid` | resolver fault | `sign/jwt` — rotation-resolution failure; `JWKSTimeout`/`NoMatchingKey` are the retry/reload arms |
| [05] | `errors.JWEDecryptionFailed` / `JWEInvalid` / `JWKInvalid` / `JOSENotSupported` | crypto/config fault | `sign/jwt` — JWE + key-shape faults; `JOSENotSupported` flags an unadmitted `alg`/`enc` at boundary |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the JWT claim profile — the primary token owner
- rail: surfaces-and-dispatch

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER] |
|:-----: |:------------------------------------------------------------------------------------------------- |:-------------- |:-------------------------------------------------------------- |
| [01] | `new SignJWT(payload).setProtectedHeader({ alg }).setIssuedAt().setIssuer(iss).setAudience(aud).setSubject(sub).setExpirationTime(exp).setJti(id).sign(key, SignOptions?)` → `Promise<string>` | sign JWT | `sign/jwt` → `session/token` — the access-token mint; one fluent builder, no per-claim helper |
| [02] | `jwtVerify<P>(jwt, key, JWTVerifyOptions?)` / `jwtVerify<P>(jwt, getKey, opts?)` → `Promise<JWTVerifyResult<P>>` (+ `ResolvedKey` on the resolver overload) | verify JWT | `sign/jwt` — static-key or JWKS-resolver verification; claim gates enforced by options, not by hand |
| [03] | `new EncryptJWT(payload)…setProtectedHeader({ alg, enc }).setKeyManagementParameters(...).replicateIssuerAsHeader().encrypt(key, EncryptOptions?)` / `jwtDecrypt(jwt, key, opts?)` | encrypt JWT | `sign/jwt` — encrypted-claims profile when the payload must be confidential end-to-end, not just signed |
| [04] | `UnsecuredJWT` (`alg: "none"`) / `decodeJwt(jwt)` / `decodeProtectedHeader(jwt)` | unsafe peek | `sign/jwt` — header/claim peek to route by `kid`/`alg` before verify; `UnsecuredJWT` is test-only |

[ENTRYPOINT_SCOPE]: JWKS resolution and key rotation — the verification-key seam
- rail: system-apis

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER] |
|:-----: |:------------------------------------------------------------------------------------------------- |:-------------- |:-------------------------------------------------------------- |
| [01] | `createRemoteJWKSet(url, { timeoutDuration, cooldownDuration, cacheMaxAge, [jwksCache], [customFetch] })` → callable resolver | remote JWKS | `sign/jwt`, `authn/oauth` — the rotation resolver; cooldown-throttled refetch on `kid` miss |
| [02] | resolver `.reload()` / `.fresh` / `.coolingDown` / `.reloading` / `.jwks()` | rotation state | `sign/jwt` — observe/force rotation; a `Schedule` drives `reload`, `jwks()` snapshots the current set |
| [03] | `jwksCache` (symbol) / `ExportedJWKSCache` (`{ jwks, uat }`) / `customFetch` (symbol) | cache / transport | `sign/jwt` — cloud KV persistence between invocations; `customFetch` routes fetch through a retry/proxy agent |
| [04] | `createLocalJWKSet(jwks)` / `EmbeddedJWK` | static JWKS | `sign/jwt` — in-memory key set for own-issued tokens; `EmbeddedJWK` reads the key from the token header |

[ENTRYPOINT_SCOPE]: raw JWS/JWE (the serialization axis) and key management
- rail: boundaries

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER] |
|:-----: |:------------------------------------------------------------------------------------------------- |:-------------- |:-------------------------------------------------------------- |
| [01] | `CompactSign` / `FlattenedSign` / `GeneralSign` + `compactVerify` / `flattenedVerify` / `generalVerify` | JWS serialization | `sign/crypto` — one sign/verify concept, three wire forms; Compact is the JWT form, General carries multi-signature |
| [02] | `CompactEncrypt` / `FlattenedEncrypt` / `GeneralEncrypt` + `compactDecrypt` / `flattenedDecrypt` / `generalDecrypt` | JWE serialization | `sign/crypto` — the same three-form axis for encryption; the `*GetKey` resolvers mirror the JWS side |
| [03] | `generateKeyPair(alg, { extractable? })` → `GenerateKeyPairResult` / `generateSecret(alg, opts?)` | key mint | `sign/jwt` — mint the signing key pair (default non-extractable); the private key never leaves the layer |
| [04] | `importPKCS8` / `importSPKI` / `importX509` / `importJWK` / `exportPKCS8` / `exportSPKI` / `exportJWK` | key codec | `sign/jwt`, `secret/material` — PEM/JWK ↔ `CryptoKey` at the key boundary; `CredentialPemWire` decodes here |
| [05] | `calculateJwkThumbprint(key, 'sha256')` / `calculateJwkThumbprintUri(key)` / `base64url.encode` / `base64url.decode` | key id / codec | `sign/jwt` — RFC 7638 `kid` derivation for rotation; `base64url` for JOSE segments |

## [04]-[IMPLEMENTATION_LAW]

[JOSE_TOPOLOGY]:
- Every crypto op returns a `Promise` and rejects with a `JOSEError` subclass — there is no synchronous mirror. The design wraps each in `Effect.tryPromise` and projects the fault by its stable `code` into one tagged union, so the whole family folds through `Match`/`catchTags` at a single seam rather than instance-of ladders.
- The three serializations (Compact/Flattened/General) are one sign/verify (and one encrypt/decrypt) rendered to three wire forms. The JWT profile is the Compact JWS/JWE with the claim-set builder on top; the design owns access tokens through `SignJWT`/`jwtVerify` and reaches for the raw JWS/JWE families only when a non-JWT payload or multi-recipient form is required — never a parallel owner per serialization.
- Claim verification is declarative: `JWTClaimVerificationOptions` makes the library enforce `exp`/`nbf`/`aud`/`iss`/`sub`/`maxTokenAge`/`requiredClaims` under a `clockTolerance`, so the design never re-checks a timestamp by hand. `algorithms` on `VerifyOptions` is a mandatory allow-list — an unpinned `alg` is an accepted-algorithm-confusion hole, so the design always pins it.
- `createRemoteJWKSet` is the rotation seam: it fetches on a `kid` miss, throttled by `cooldownDuration`, refreshes up to `cacheMaxAge`, and exposes `reload`/`fresh`/`coolingDown`. The resolver is built once and held behind a `Context.Tag`; `jwksCache` persists the `{ jwks, uat }` snapshot across stateless invocations, and `customFetch` routes the fetch through a resilient transport.
- Keys are `CryptoKey` handles imported once at `Layer` construction as non-extractable; the private key lives in `Redacted` and unwraps only into `sign`. `decodeJwt`/`decodeProtectedHeader` return unverified `object`/header — boundary routing only, never a trusted read.

[STACKS_WITH]:
- `effect` (`.api/effect.md`): `Effect.tryPromise` lifts every op; a `Schema` transform projects `JOSEError.code` → a `Data.TaggedError` union folded by `Effect.catchTags`; `Schema.decodeUnknown` brands the open `JWTPayload` into the domain claim after verification; `Redacted` holds the signing key/secret and the minted token string; `Config.redacted` sources the HS secret / issuer key; `Schedule.exponential`/`Schedule.fixed` drives `resolver.reload()`; `Cache`/`Layer.scoped` owns the resolver lifetime; `Duration` bridges `maxTokenAge`/`clockTolerance`.
- `@effect/platform` (`.api/effect-platform.md`): `HttpApiSecurity.bearer` decodes the inbound `Authorization` header to the raw token the design hands to `jwtVerify`; `createRemoteJWKSet`'s `customFetch` symbol routes the JWKS fetch through an `HttpClient.retryTransient({ schedule })` client so rotation inherits the shared net policy and W3C trace propagation; `Headers.redact` keeps the bearer out of logs.
- `arctic` (`.api/arctic.md`): the OIDC `id_token` from `validateAuthorizationCode` is verified here via `jwtVerify(idToken, createRemoteJWKSet(provider.jwksUri), { issuer, audience })` — jose is the verification authority arctic's `decodeIdToken` peek defers to.
- `@oslojs/encoding` (`.api/oslojs-encoding.md`): jose owns the `base64url` codec for JOSE segments internally (`jose/base64url` `encode`/`decode` under `CompactSign`/`SignJWT`); the sibling oslo codec owns the general non-JOSE token/digest matrix (opaque API-key prefixes, recovery codes, session-token material, WebAuthn attestation bytes, base32 TOTP secrets, hex digests) — the boundary keeps JOSE base64url inside jose and general codecs in oslo, never cross-wired: oslo never encodes a JWS segment, jose never renders opaque token material.
- `security/session/token` (in-folder): `SignJWT`/`jwtVerify` mint and verify the access token; `JWTExpired` is the tagged arm that triggers refresh rotation; the refresh token is opaque and hashed via `sign/crypto`, not a JWT.

[LOCAL_ADMISSION]:
- Use `SignJWT`/`jwtVerify` for the JWT profile and always pin `algorithms` on verify; never omit the allow-list, never trust `decodeJwt`/`decodeProtectedHeader` as verification, and never emit `UnsecuredJWT` outside a test.
- Use `JWTClaimVerificationOptions` for `exp`/`aud`/`iss`/`maxTokenAge`; never re-check a claim timestamp by hand once the option can enforce it.
- Use `createRemoteJWKSet` held behind a `Tag` with a `Schedule`-driven `reload` and `jwksCache` persistence for rotation; never re-fetch the JWKS per request or re-import a key per call.
- Wrap every op in `Effect.tryPromise` and fold the `JOSEError` family by `code`; never let a jose reject cross into domain logic as a bare rejection.
- Hold signing keys and minted tokens in `Redacted`; import keys once as non-extractable at layer construction.

[RAIL_LAW]:
- Package: `jose`
- Owns: JWS sign/verify (three serializations), the JWT claim profile (`SignJWT`/`jwtVerify`/`EncryptJWT`/`jwtDecrypt`/`UnsecuredJWT`), JWE encrypt/decrypt, JWKS resolution + rotation (`createRemoteJWKSet`/`createLocalJWKSet`/`EmbeddedJWK`/`jwksCache`/`customFetch`), key import/export/generation, JWK thumbprint, `base64url`, and the closed `JOSEError` fault family
- Accept: `SignJWT`/`jwtVerify` with a pinned `algorithms` allow-list, declarative `JWTClaimVerificationOptions`, `createRemoteJWKSet` behind a `Tag` with `Schedule` reload + `jwksCache`, `Effect.tryPromise` + `code`-tagged fault fold, `Schema` decode of the verified `JWTPayload`, non-extractable `CryptoKey`s in `Redacted`, deep-imported concern subpaths
- Reject: an unpinned `alg` on verify, `decodeJwt`/`decodeProtectedHeader` used as verification, `UnsecuredJWT` in production, a hand-rolled claim/timestamp check, a per-request JWKS fetch or per-call key import, a jose reject escaping into domain code, a signing key or token outside `Redacted`
