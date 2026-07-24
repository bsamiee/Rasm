# [TS_SECURITY_API_JOSE]

`jose` owns every JOSE operation the `security/sign/jwt` design composes, over WebCrypto with zero runtime dependencies. Its surface collapses on three axes — crypto op, token profile, serialization — so Compact, Flattened, and General render one sign/verify to three wire forms; `createRemoteJWKSet` is the key-rotation seam, and every failure is a `JOSEError` subclass discriminated by a stable `code`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `jose`
- package: `jose` (MIT)
- module: ESM (`type: module`, `sideEffects: false`); per-concern subpaths resolve the members the root barrel re-exports, so a root import tree-shakes to the composed surface
- runtime: isomorphic — WebCrypto with global `fetch` across node, bun, deno, workerd, and browser; no native addon, so `sign/jwt.ts` stays host-neutral
- asset: pure-TypeScript runtime library (`.js` + `.d.ts`); `JWTPayload` stays open (`[propName: string]: unknown`), so a `Schema` decode gates the verified claim set
- rail: `security/sign` — token-crypto owner, admitted in `sign/` only

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: claims, keys, and the verification-policy axis — consumer `sign/jwt`

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]    | [CAPABILITY]                                             |
| :-----: | :---------------------------- | :--------------- | :------------------------------------------------------- |
|  [01]   | `JWTPayload`                  | claim set        | open claim set `Schema.decodeUnknown` brands to a domain |
|  [02]   | `JWK` / `JSONWebKeySet`       | key material     | import, export, and thumbprint input; JWKS verify set    |
|  [03]   | `CryptoKey` / `KeyObject`     | key handle       | non-extractable runtime handle held behind a `Tag`       |
|  [04]   | `JWTVerifyOptions`            | verify policy    | one row: `algorithms` allow-list and every claim gate    |
|  [05]   | `JWTClaimVerificationOptions` | claim gate       | declarative claim checks the library itself enforces     |
|  [06]   | `ProduceJWT`                  | builder contract | fluent claim-setter `SignJWT`/`EncryptJWT` implement     |

- [01]-[CLAIM_SET]: `iss` `sub` `aud` `jti` `nbf` `exp` `iat`, open under `[propName: string]: unknown`.
- [02]-[JWK]: `JWK` `JWK_RSA_Private` `JWK_RSA_Public` `JWK_EC_Private` `JWK_EC_Public` `JWK_oct` `JWK_OKP_Private` `JWK_OKP_Public` `JSONWebKeySet`.
- [05]-[CLAIM_GATE]: `issuer` `audience` `subject` `maxTokenAge` `clockTolerance` `requiredClaims` `typ` `currentDate`.

[PUBLIC_TYPE_SCOPE]: `errors.*` fault classes, discriminated by a stable `code` — consumer `sign/jwt`

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]   | [CAPABILITY]                                          |
| :-----: | :------------------------------- | :-------------- | :---------------------------------------------------- |
|  [01]   | `errors.JOSEError`               | fault base      | `code` roots the family; one `Match`/`catchTags` fold |
|  [02]   | `JWTExpired`                     | claim fault     | expiry refusal; `payload` drives the re-auth arm      |
|  [03]   | `JWSSignatureVerificationFailed` | signature fault | untrusted token; one reject-and-401 arm               |
|  [04]   | `JWKSNoMatchingKey`              | resolver fault  | rotation-resolution failure; retry and reload arms    |
|  [05]   | `JWEDecryptionFailed`            | crypto fault    | JWE and key-shape refusal                             |

- [02]-[CLAIM_FAULT]: `JWTExpired` `JWTClaimValidationFailed` — the latter carries `payload`, `claim`, `reason`.
- [03]-[SIGNATURE_FAULT]: `JWSSignatureVerificationFailed` `JWSInvalid` `JWTInvalid` `JOSEAlgNotAllowed`.
- [04]-[RESOLVER_FAULT]: `JWKSNoMatchingKey` `JWKSMultipleMatchingKeys` `JWKSTimeout` `JWKSInvalid` — `JWKSTimeout` and `JWKSNoMatchingKey` are the retry/reload arms.
- [05]-[CRYPTO_FAULT]: `JWEDecryptionFailed` `JWEInvalid` `JWKInvalid` `JOSENotSupported` — `JOSENotSupported` flags an unadmitted `alg`/`enc` at the boundary.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the JWT claim profile, primary token owner — consumer `sign/jwt`

| [INDEX] | [SURFACE]                                                        | [SHAPE] | [CAPABILITY]                                   |
| :-----: | :--------------------------------------------------------------- | :------ | :--------------------------------------------- |
|  [01]   | `new SignJWT(JWTPayload)….sign(key, SignOptions?) -> string`     | ctor    | mint the access token; chain at [01]           |
|  [02]   | `jwtVerify(string, key\|JWTVerifyGetKey, JWTVerifyOptions?)`     | static  | verify; resolver overload adds `ResolvedKey`   |
|  [03]   | `new EncryptJWT(JWTPayload)….encrypt(...)` / `jwtDecrypt(...)`   | ctor    | confidential-claims profile; chain at [03]     |
|  [04]   | `decodeJwt(...)` / `decodeProtectedHeader(...)` / `UnsecuredJWT` | static  | unverified peek by `kid`/`alg`; test-only mint |

- [01]-[SIGN_JWT]: `setProtectedHeader({ alg })` → `setIssuedAt()` → `setIssuer` → `setAudience` → `setSubject` → `setNotBefore` → `setExpirationTime` → `setJti` → `.sign(key, SignOptions?)`.
- [03]-[ENCRYPT_JWT]: `setProtectedHeader({ alg, enc })` → `setKeyManagementParameters` → `replicateIssuerAsHeader` → `.encrypt(key, EncryptOptions?)`.

[ENTRYPOINT_SCOPE]: JWKS resolution and key rotation, the verification-key seam — consumer `sign/jwt` and `authn/workload`

| [INDEX] | [SURFACE]                                              | [SHAPE]  | [CAPABILITY]                                    |
| :-----: | :----------------------------------------------------- | :------- | :---------------------------------------------- |
|  [01]   | `createRemoteJWKSet(URL, RemoteJWKSetOptions?)`        | factory  | rotation resolver refetching on a `kid` miss    |
|  [02]   | `.reload()` / `.fresh` / `.coolingDown` / `.reloading` | property | rotation state a `Schedule` drives              |
|  [03]   | `jwksCache` / `ExportedJWKSCache` / `customFetch`      | property | `{ jwks, uat }` KV persistence; transport swap  |
|  [04]   | `createLocalJWKSet(JSONWebKeySet)` / `EmbeddedJWK`     | factory  | in-memory set; `EmbeddedJWK` reads header `jwk` |

- [01]-[REMOTE_OPTS]: `timeoutDuration` `cooldownDuration` `cacheMaxAge` `headers` `[jwksCache]` `[customFetch]`; `.jwks()` snapshots the resolved set.

[ENTRYPOINT_SCOPE]: raw JWS/JWE serialization and key management — consumer `sign/crypto` and `sign/jwt`

| [INDEX] | [SURFACE]                                                    | [SHAPE] | [CAPABILITY]                               |
| :-----: | :----------------------------------------------------------- | :------ | :----------------------------------------- |
|  [01]   | JWS serialization family                                     | ctor    | one sign/verify, three wire forms; at [01] |
|  [02]   | JWE serialization family                                     | ctor    | same axis; `*GetKey` mirrors JWS; at [02]  |
|  [03]   | `generateKeyPair(alg, opts?)` / `generateSecret(alg, opts?)` | static  | key mint; `extractable` defaults false     |
|  [04]   | key import/export codecs                                     | static  | PEM/DER/JWK ↔ `CryptoKey`; members at [04] |
|  [05]   | thumbprint and `base64url` codecs                            | static  | RFC 7638 `kid`, JOSE segments; at [05]     |

- [01]-[JWS]: `CompactSign` `FlattenedSign` `GeneralSign` `compactVerify` `flattenedVerify` `generalVerify` — General carries multi-signature.
- [02]-[JWE]: `CompactEncrypt` `FlattenedEncrypt` `GeneralEncrypt` `compactDecrypt` `flattenedDecrypt` `generalDecrypt`.
- [04]-[KEY_CODEC]: `importPKCS8` `importSPKI` `importX509` `importJWK` `exportPKCS8` `exportSPKI` `exportJWK`.
- [05]-[THUMBPRINT]: `calculateJwkThumbprint(key, 'sha256')` `calculateJwkThumbprintUri(key)` `base64url.encode` `base64url.decode`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every op returns a `Promise` and rejects with a `JOSEError` subclass; the stable `code` projects the whole family onto one tagged union folded at a single seam, never an instance-of ladder.
- JWT profile rides Compact JWS/JWE under the claim-set builder; a raw JWS/JWE family answers a non-JWT payload or a multi-recipient form alone, so no serialization mints a parallel owner.
- `JWTClaimVerificationOptions` makes the library enforce every claim gate under `clockTolerance`, and `algorithms` pins the accepted set on verify, closing algorithm confusion at the option row.
- `createRemoteJWKSet` owns rotation: it refetches on a `kid` miss throttled by `cooldownDuration`, refreshes to `cacheMaxAge`, and exposes its own state; `jwksCache` persists `{ jwks, uat }` across stateless invocations and `customFetch` routes the fetch through a resilient transport.
- Keys import once at `Layer` construction as non-extractable `CryptoKey` handles; `decodeJwt` and `decodeProtectedHeader` return unverified values that route at the boundary alone.

[STACKING]:
- `effect` (`.api/effect.md`): `Effect.tryPromise` lifts every op, a `Schema` transform projects `JOSEError.code` onto a `Data.TaggedError` union `Effect.catchTags` folds, `Schema.decodeUnknown` brands the verified `JWTPayload`, `Schedule.exponential` drives `resolver.reload()`, `Layer.scoped` owns the resolver lifetime, `Duration` bridges `maxTokenAge`/`clockTolerance`, and `Redacted.make`/`Redacted.value` carry the signing key and minted token.
- `@effect/platform` (`.api/effect-platform.md`): `HttpApiSecurity.bearer` decodes the inbound `Authorization` header to the raw token `jwtVerify` takes; the `customFetch` symbol routes the JWKS fetch through an `HttpClient.retryTransient({ schedule })` client so rotation inherits the shared net policy and trace propagation, and `Headers.redact` keeps the bearer out of logs.
- `openid-client` (`.api/openid-client.md`): `discovery` resolves `jwks_uri` into `createRemoteJWKSet` for `fetchUserInfo` and `authorizationCodeGrant` id-token claim verification, `randomDPoPKeyPair`/`getDPoPHandle` take their `cnf.jkt` from `calculateJwkThumbprintUri`, and both packages share the one WebCrypto `CryptoKey` path.
- `arctic` (`.api/arctic.md`): `validateAuthorizationCode` yields the OIDC `id_token` that `jwtVerify(idToken, createRemoteJWKSet(provider.jwksUri), { issuer, audience })` verifies, so arctic's `decodeIdToken` peek defers its verification authority here.
- `@oslojs/encoding` (`.api/oslojs-encoding.md`): `jose/base64url` `encode`/`decode` render every JOSE/JWS compact segment inside `CompactSign`/`SignJWT`, so no JWS segment crosses to a general codec.
- `security/session/token` (in-folder): `SignJWT`/`jwtVerify` mint and verify the access token, and `JWTExpired` is the tagged arm that triggers refresh rotation.

[LOCAL_ADMISSION]:
- Mint and verify the JWT profile through `SignJWT`/`jwtVerify` behind a pinned `algorithms` allow-list, delegating every claim gate to `JWTClaimVerificationOptions`; `UnsecuredJWT` stays test-only.
- Hold one `createRemoteJWKSet` resolver behind a `Tag` under a `Schedule`-driven `reload` with `jwksCache` persistence, so a request reuses both the set and the imported key.
- Wrap every op in `Effect.tryPromise` and fold the `JOSEError` family by `code` at that seam, so a jose rejection reaches domain logic as a tagged arm.

[RAIL_LAW]:
- Package: `jose`
- Owns: JWS sign/verify across three serializations, the JWT claim profile, JWE encrypt/decrypt, JWKS resolution and rotation, key import/export/generation, JWK thumbprint, JOSE `base64url`, and the closed `JOSEError` fault family
- Accept: `SignJWT`/`jwtVerify` under a pinned `algorithms` allow-list, declarative `JWTClaimVerificationOptions`, `createRemoteJWKSet` behind a `Tag` with `Schedule` reload and `jwksCache`, `Effect.tryPromise` with a `code`-tagged fault fold, `Schema` decode of the verified `JWTPayload`, non-extractable `CryptoKey` handles in `Redacted`
- Reject: an unpinned `alg` on verify, `decodeJwt`/`decodeProtectedHeader` read as verification, a hand-rolled claim or timestamp check, a per-request JWKS fetch or per-call key import, a bare jose rejection in domain code
