# [TS_SECURITY_API_OSLOJS_CRYPTO]

`@oslojs/crypto` is the pure-TypeScript, dependency-free crypto primitive `sign/crypto` composes for the rows WebCrypto and `@node-rs/argon2` do not own: message authentication (`HMAC` over any `HashAlgorithm`), timing-safe comparison (`constantTimeEqual`), the SHA-1/2/3 + SHAKE digest roster, signature *verification* (ECDSA/RSA public-key only — no private-key signing, no key generation), and a CSPRNG port (`RandomReader`) the caller fills from the platform entropy source. It is subpath-export ESM: every capability is a `@oslojs/crypto/<sub>` import, so the `tests/typescript/_architecture` suite fences it to `sign/` and a tree-shaker drops the unused curves. It owns no symmetric cipher — the `sign/crypto` AES-GCM envelope `Shredder` is WebCrypto `SubtleCrypto`, never this package. Its one polymorphic axis is the `HashAlgorithm` constructor interface: `HMAC`, the digest one-shots, and RSA-PSS all take an algorithm *value*, so a new hash is a row, never a new API.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@oslojs/crypto`
- package: `@oslojs/crypto`
- license: `MIT`
- module: ESM-only (`"type": "module"`); no root export — capability is subpath-only, so an import is always `@oslojs/crypto/<sub>`
- effect-boundary: pure synchronous functions — lift via `Effect.sync` (total) / `Effect.try` (throwing decode); never `Effect.tryPromise` (nothing here is async). `.api/effect.md`
- catalog-verdict: KEEP — the one HMAC + constant-time-compare + streaming-digest owner; WebCrypto lacks a synchronous constant-time equal and a streaming `Hash` contract, and this is dependency-free vs `@noble/hashes`
- runtime: `runtime:neutral` — pure JS, no `node:*`; the `sign/` "node-only subpath" caveat is `@node-rs/argon catalog`'s native constraint, not this package
- subpaths: `/hash` `/hmac` `/subtle` `/random` `/sha1` `/sha2` `/sha3` `/ecdsa` `/rsa`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `/hash` — the polymorphic algorithm contract every digest and HMAC composes
- rail: sign/crypto
- `HashAlgorithm` is the single collapse point: `new (): Hash` — a zero-arg constructor yielding a streaming `Hash`. `HMAC`, RSA-PSS, and every `SHA*` class are instances of this one interface, so algorithm choice is a *value*, not a parallel call family.

| [INDEX] | [SYMBOL]                                                       | [TYPE_FAMILY]    | [CONSUMER_BOUNDARY]                             |
| :-----: | :------------------------------------------------------------- | :--------------- | :---------------------------------------------- |
|  [01]   | `Hash { blockSize; size; update(data); digest(): Uint8Array }` | streaming digest | incremental digest contract; `HMAC` wraps two   |
|  [02]   | `HashAlgorithm { new (): Hash }`                               | algorithm value  | polymorphic key passed to `HMAC`/`hmac`/RSA-PSS |

[PUBLIC_TYPE_SCOPE]: `/hmac` `/subtle` `/random` — the three primitives `sign/crypto` actually exports as rows
- rail: sign/crypto
- `HMAC` is the webhook-signing owner; `constantTimeEqual` is the NON-argon2 fixed-length token/digest/signature equality owner (`session/token` opaque-token + refresh compare, decoded ECDSA/RSA signature compare) — a stored `@node-rs/argon2` credential digest is checked by argon2's own constant-time `verify`, never re-compared here; `RandomReader` is the entropy port the caller satisfies with WebCrypto.

| [INDEX] | [SYMBOL]                                                   | [TYPE_FAMILY]  | [CONSUMER_BOUNDARY]                                       |
| :-----: | :--------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `HMAC` class (ctor `(Algorithm, key)`, `update`, `digest`) | MAC stream     | streaming webhook body MAC; `sign/crypto` HMAC row        |
|  [02]   | `constantTimeEqual(a, b): boolean`                         | timing-safe eq | `session/token` token/refresh + decoded-signature compare |
|  [03]   | `RandomReader { read(bytes: Uint catalogArray): void }`    | entropy port   | WebCrypto `crypto.getRandomValues`; never owns entropy    |

[PUBLIC_TYPE_SCOPE]: `/ecdsa` `/rsa` — verify-only public-key material (decode + verify, never sign)
- rail: sign/crypto
- Public-key *verification* only: decode a SEC1/PKIX/PKCS1 key or IEEE-P1363/PKIX signature, then `verify*`. There is no private key, no signing, no key generation — token *signing* is `jose` (`.api/jose.md`, `sign/jwt`); this is the row for verifying an externally-issued ECDSA/RSA signature (e.g. an attestation or a partner webhook).

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]   | [CONSUMER_BOUNDARY]                                            |
| :-----: | :-------------------------- | :-------------- | :------------------------------------------------------------- |
|  [01]   | ECDSA verify + codecs       | ECDSA verify    | external ECDSA signature verify; SEC1/PKIX/IEEE-P1363; at [01] |
|  [02]   | `ECDSANamedCurve` values    | curve roster    | a value row passed to the decoders; at [02] below              |
|  [03]   | RSA verify + `RSAPublicKey` | RSA verify      | external RSA verify; PKCS1 vs PSS shape at [03] below          |
|  [04]   | RSA key + OID codecs        | key + OID codec | PKCS1/PKIX key decode + hash-OID constants; at [04] below      |

- [01]-[ECDSA]: `verifyECDSASignature(pub, hash, sig)`, `ECDSAPublicKey(curve, x, y)`, `ECDSASignature(r, s)`, `decodeSEC1ECDSAPublicKey(curve, sec1)`, `decodePKIXECDSAPublicKey(der, curves)`, `decodePKIXECDSASignature(der)`, `decodeIEEEP1363ECDSASignature(curve, raw)`.
- [02]-[CURVES]: `p256`, `p384`, `p521` (NIST), `secp256k1`…`secp521r1` (SEC) — a value passed to the decoders, never a call family.
- [03]-[RSA]: `verifyRSASSAPKCS1v15Signature(pub, hashOID, hash, sig)`, `verifyRSASSASignature(pub, hashAlg, mgf1Hash, saltLength, hash, sig)`, `RSAPublicKey(n, e)` — PKCS1 takes the hash OID; PSS takes the `HashAlgorithm` class, MGF1 hash class, and salt length.
- [04]-[RSA_KEY]: `decodePKCS1RSAPublicKey`, `decodePKIXRSAPublicKey`, `SHA256ObjectIdentifier` (+ `SHA1`/`SHA224`/`SHA384`/`SHA512` OIDs) — the capitalized `SHA<n>ObjectIdentifier` constants for PKCS1.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: HMAC — the one webhook-signing row (stream + one-shot mirror)
- rail: sign/crypto
- `hmac(Algorithm, key, message)` is the one-shot; the `HMAC` class is its streaming mirror for chunked bodies. Both take the `HashAlgorithm` value — `SHA256` is the row, a `SHA512` webhook is the same call with a different value.

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                  |
| :-----: | :---------------------------------------------------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `hmac(Algorithm, key, message): Uint8Array`                 | MAC one-shot   | `sign/crypto` webhook signature over a buffered body |
|  [02]   | `new HMAC(Algorithm, key)` → `.update(chunk)` → `.digest()` | MAC stream     | chunked/streamed body MAC without buffering          |

[ENTRYPOINT_SCOPE]: digest roster — one `digest(algorithm, data)` fold over the `HashAlgorithm` value set
- rail: sign/crypto
- Every hash is a `sha*(data): Uint8Array` one-shot plus a `SHA*` streaming class implementing `Hash`. Do not enumerate a call per algorithm — the algorithm is the parameter; the table is the *value set* one polymorphic digest fold ranges over. SHAKE128/256 are extendable-output (XOF): the digest length is an argument.

| [INDEX] | [SUBPATH] | [VALUE_SET]                                                              | [CONSUMER_BOUNDARY]                     |
| :-----: | :-------- | :----------------------------------------------------------------------- | :-------------------------------------- |
|  [01]   | `/sha1`   | `sha1` / `SHA1` (retired)                                                | RFC-4226 HOTP OID; retired              |
|  [02]   | `/sha2`   | `sha256` `sha384` `sha512` `sha224` `sha512_224` `sha512_256` (+ `SHA*`) | default family; `SHA256` the HMAC value |
|  [03]   | `/sha3`   | `sha3_224/256/384/512` (+ `SHA3_*`) · `shake128/256(size, data)` (XOF)   | SHA-3 + XOF (length as arg)             |

[ENTRYPOINT_SCOPE]: `/random` — CSPRNG-port derivations, entropy supplied by the caller
- rail: sign/crypto
- The `RandomReader` port inverts entropy ownership: these functions never touch a system RNG — they consume the `read(bytes)` the caller wires to WebCrypto. `generateRandomString` is the recovery/backup-code + API-key-body generator over a bounded alphabet; `generateRandomInteger`/`generateRandomIntegerNumber` take `(random, max)` — `bigint` vs `number` bound.

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                    |
| :-----: | :------------------------------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `generateRandomString(random, alphabet, length): string` | token mint     | `authn/apikey` body, `authn/otp` recovery/backup codes |
|  [02]   | `generateRandomInteger` / `generateRandomIntegerNumber`  | bounded int    | unbiased bounded index (alphabet selection, jitter)    |

## [04]-[IMPLEMENTATION_LAW]

[PRIMITIVE_TOPOLOGY]:
- one algorithm contract: `HashAlgorithm`/`Hash` is the single interface `HMAC`, every `SHA*`, and RSA-PSS implement. Algorithm choice is a value passed as a constructor row — never a `hmacSha256`/`hmacSha512` call family. A new digest is a `HashAlgorithm` value, not a new entrypoint.
- entropy is a port, not an owner: `RandomReader.read(bytes)` is filled by the caller. `sign/crypto` binds it once to WebCrypto `crypto.getRandomValues` (or a `@effect/platform` random service) so entropy sourcing stays a single seam and tests inject a deterministic reader.
- verify-only asymmetry: `/ecdsa` and `/rsa` decode and *verify*; they never sign or generate keys. Token issuance/signing is `jose` (`sign/jwt`). This package is the row for verifying a signature Rasm did not mint.
- no symmetric cipher: there is no AES here. The `sign/crypto` AES-GCM envelope `Shredder` (`store/journal` per-subject crypto-shredding) is WebCrypto `SubtleCrypto.encrypt`/`.deriveKey`; this package backs only the HMAC, constant-time, digest, and RNG rows of that page.

[INTEGRATION_LAW]:
- Stack with `.api/effect.md` rails: HMAC/digest/verify are synchronous → `Effect.sync` for total ops, `Effect.try({ try, catch })` for the throwing decoders (`decodePKIX*`, `decodeSEC1PublicKey`). Keys and MAC secrets stay `Redacted<Uint8Array>`; `Redacted.value` unwraps only inside the sync boundary, and the digest result crosses back as opaque bytes. `constantTimeEqual` is the *only* admitted equality for a NON-argon2 fixed-length secret/digest/signature compare — never `===` on decoded token bytes, and never on an `@node-rs/argon2` credential digest (that is argon2 `verify`, itself constant-time).
- Stack with `@oslojs/encoding` (`.api/oslojs-encoding.md`): this package emits raw `Uint8Array` digests/MACs; the encoding sibling renders them to hex/base64url for at-rest storage and wire transport. The two are the paired byte↔string boundary — a digest is `hmac(SHA256, key, body)` → `encodeHexLowerCase(...)`, and a stored digest decodes back through `decodeHex` before `constantTimeEqual`.
- Stack with `otplib` `CryptoPlugin` (`.api/otplib.md`): otplib's HMAC is a swappable port (a structural type, no factory required). `sign/crypto` satisfies it with a plain object `{ name, hmac, randomBytes, constantTimeEqual }` over this package and passes it to `authn/otp`, so TOTP/HOTP HMAC rides the *same* primitive the rest of `sign` owns — the admission ban stays honored (this package never leaves `sign/`; the plugin object crosses the in-folder `authn/otp → sign/crypto` delegation), and otplib's default `@noble/hashes` dependency is bypassed.
- Stack with `@node-rs/argon2` (`.api/node-rs-argon2.md`): the credential-at-rest boundary — the reciprocal of that catalog's oslo seam. argon2 owns password/api-key digest-at-rest (`hash` → PHC, `verify`), and `verify` is itself constant-time, so a stored argon2 PHC digest is checked by argon2 and NEVER re-compared through `constantTimeEqual`. This package owns only the NON-argon2 fixed-length material behind the sign folds: the HMAC webhook MAC, the `session/token` opaque-token/refresh `constantTimeEqual`, and the `generateRandomString` that mints the `authn/apikey` body argon2 then hashes at rest (`authn/apikey` consumes `/random` for the mint, never `/subtle` for the check). The two never double-wrap — oslo mints and compares non-credential tokens; argon2 hashes and verifies credentials.
- Stack with `sign/crypto` design rows: `hmac(SHA256, …)` = webhook signing; `constantTimeEqual` = the equality every NON-argon2 fixed-length digest/token/signature compare routes through (an argon2 credential digest is argon2 `verify`); `generateRandomString` (over the WebCrypto-filled `RandomReader`) = `authn/apikey` body mint and `authn/otp` recovery-code mint before the `@node-rs/argon2` digest-at-rest.

[LOCAL_ADMISSION]:
- imported only inside `sign/` subpaths; an `authn`/`secret`/`session` rail importing it is the defect the `tests/typescript/_architecture` import audit catches. `authn/otp` reaches the HMAC only through the `CryptoPlugin` `sign/crypto` constructs.
- pure-JS and runtime-neutral: no `node:*`, safe in a `runtime:browser` composition; the `sign/` node caveat is `@node-rs/argon catalog`, not this package.
- one-shot vs stream: prefer the `hmac`/`sha*` one-shots; use the `HMAC`/`SHA*` classes only for genuinely chunked input that must not be buffered.

[RAIL_LAW]:
- Package: `@oslojs/crypto`
- Owns: `HMAC` (webhook signing), `constantTimeEqual` (timing-safe compare), the SHA-1/2/3 + SHAKE digest roster over the `HashAlgorithm` contract, verify-only ECDSA/RSA public-key material, and the `RandomReader`-port CSPRNG derivations
- Accept: the `HashAlgorithm` value as the one dispatch key, `constantTimeEqual` as the sole equality for NON-argon2 fixed-length secret/digest/token material, `RandomReader` filled from WebCrypto, `hmac`/`sha*` one-shots by default, this package as the crypto backend of an otplib `CryptoPlugin`
- Reject: a `hmacSha*`/`sha*`-per-name call family (algorithm is a value), `===`/`Buffer.equals` on secret bytes, re-comparing an `@node-rs/argon2` credential digest through `constantTimeEqual` (argon2 `verify` owns that, itself constant-time), claiming AES-GCM here (it is WebCrypto), token *signing* here (it is `jose`), a system-RNG read that bypasses the `RandomReader` seam, any import outside `sign/`
