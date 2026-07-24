# [TS_SECURITY_API_OSLOJS_CRYPTO]

`@oslojs/crypto` owns the pure-JS primitive rows WebCrypto leaves open: HMAC over any `HashAlgorithm`, timing-safe `constantTimeEqual`, the SHA-1/2/3 and SHAKE digest roster, verify-only ECDSA/RSA material, and a caller-filled `RandomReader` CSPRNG port. Subpath-only ESM exports keep every capability a `@oslojs/crypto/<sub>` import, so a tree-shaker drops the unreached curves.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@oslojs/crypto`
- package: `@oslojs/crypto` (MIT)
- module: ESM-only, no root export; subpaths `/hash` `/hmac` `/subtle` `/random` `/sha1` `/sha2` `/sha3` `/ecdsa` `/rsa`
- runtime: neutral — pure JS, zero dependencies, no `node:*`
- rail: `security/sign` — the HMAC, timing-safe-compare, digest, verify, and CSPRNG-port primitives `sign/crypto` folds

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `/hash` `/hmac` `/random` — the algorithm contract `HMAC`, every `SHA*` class, and RSA-PSS take, with the two stateful carriers.

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY] | [CAPABILITY]                  |
| :-----: | :----------------------------------------- | :------------ | :---------------------------- |
|  [01]   | `HashAlgorithm { new (): Hash }`           | interface     | algorithm passed as a value   |
|  [02]   | `Hash { blockSize, size, update, digest }` | interface     | incremental digest contract   |
|  [03]   | `HMAC(HashAlgorithm, Uint8Array)`          | class         | streaming webhook-body MAC    |
|  [04]   | `RandomReader { read(Uint8Array) }`        | interface     | entropy port the caller fills |

[PUBLIC_TYPE_SCOPE]: `/ecdsa` `/rsa` — public-key carriers holding no private key and generating none.

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------ | :------------ | :--------------------------------------------- |
|  [01]   | `ECDSAPublicKey(ECDSANamedCurve, bigint, bigint)` | class         | curve-bound point with SEC1/PKIX encoders      |
|  [02]   | `ECDSASignature(bigint, bigint)`                  | class         | `(r, s)` pair with P1363/PKIX encoders         |
|  [03]   | `ECDSANamedCurve`                                 | class         | curve parameters and point arithmetic          |
|  [04]   | `RSAPublicKey(bigint, bigint)`                    | class         | `(n, e)` modulus pair with PKCS1/PKIX encoders |

[CURVES]: `p192` `p224` `p256` `p384` `p521` `secp192k1` `secp192r1` `secp224k1` `secp224r1` `secp256k1` `secp256r1` `secp384r1` `secp521r1`
[HASH_OIDS]: `sha1ObjectIdentifier` `sha224ObjectIdentifier` `sha256ObjectIdentifier` `sha384ObjectIdentifier` `sha512ObjectIdentifier`
[KEY_ENCODERS]: `ECDSAPublicKey.encodeSEC1Uncompressed()` `.encodeSEC1Compressed()` `.encodePKIXUncompressed()` `.encodePKIXCompressed()` `.isCurve(ECDSANamedCurve)` `ECDSASignature.encodeIEEEP1363(ECDSANamedCurve)` `.encodePKIX()` `RSAPublicKey.encodePKCS1()` `.encodePKIX()`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `/hmac` `/subtle` — MAC and equality, both synchronous over raw bytes.

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :---------------------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `hmac(HashAlgorithm, Uint8Array, Uint8Array) -> Uint8Array` | static   | one-shot MAC over a buffered body |
|  [02]   | `HMAC.update(Uint8Array)` then `.digest() -> Uint8Array`    | instance | chunked MAC without buffering     |
|  [03]   | `constantTimeEqual(Uint8Array, Uint8Array) -> boolean`      | static   | timing-safe fixed-length compare  |

[ENTRYPOINT_SCOPE]: `/sha1` `/sha2` `/sha3` — every digest ships a `sha*(Uint8Array) -> Uint8Array` one-shot and a `SHA*` class implementing `Hash`; one polymorphic fold ranges over the value set.

- [SHA1]: `sha1` `SHA1`
- [SHA2]: `sha224` `sha256` `sha384` `sha512` `sha512_224` `sha512_256`
- [SHA3]: `sha3_224` `sha3_256` `sha3_384` `sha3_512`
- [XOF]: `shake128(number, Uint8Array)` `shake256(number, Uint8Array)` `SHAKE128(number)` `SHAKE256(number)` — output length is an argument

[ENTRYPOINT_SCOPE]: `/random` — derivations consuming the `read(bytes)` the caller wires to WebCrypto, never a system RNG.

| [INDEX] | [SURFACE]                                                      | [SHAPE] | [CAPABILITY]                         |
| :-----: | :------------------------------------------------------------- | :------ | :----------------------------------- |
|  [01]   | `generateRandomString(RandomReader, string, number) -> string` | static  | bounded-alphabet token and code mint |
|  [02]   | `generateRandomInteger(RandomReader, bigint) -> bigint`        | static  | unbiased bounded index               |
|  [03]   | `generateRandomIntegerNumber(RandomReader, number) -> number`  | static  | `number`-bounded mirror              |

[ENTRYPOINT_SCOPE]: `/ecdsa` `/rsa` verification — every `verify*` returns `boolean`; PKCS1 keys on the hash OID string, PSS on the message and MGF1 `HashAlgorithm` values with a salt length.

| [INDEX] | [SURFACE]                                                                                              | [SHAPE] | [CAPABILITY]     |
| :-----: | :----------------------------------------------------------------------------------------------------- | :------ | :--------------- |
|  [01]   | `verifyECDSASignature(ECDSAPublicKey, Uint8Array, ECDSASignature)`                                     | static  | ECDSA verify     |
|  [02]   | `verifyRSASSAPKCS1v15Signature(RSAPublicKey, string, Uint8Array, Uint8Array)`                          | static  | RSA PKCS1 verify |
|  [03]   | `verifyRSASSAPSSSignature(RSAPublicKey, HashAlgorithm, HashAlgorithm, number, Uint8Array, Uint8Array)` | static  | RSA PSS verify   |

[ENTRYPOINT_SCOPE]: `/ecdsa` `/rsa` codecs — every `decode*` throws on malformed input.

| [INDEX] | [SURFACE]                                                                      | [SHAPE] | [CAPABILITY]                           |
| :-----: | :----------------------------------------------------------------------------- | :------ | :------------------------------------- |
|  [01]   | `decodeSEC1PublicKey(ECDSANamedCurve, Uint8Array) -> ECDSAPublicKey`           | static  | SEC1 point to a curve-bound key        |
|  [02]   | `decodePKIXECDSAPublicKey(Uint8Array, ECDSANamedCurve[]) -> ECDSAPublicKey`    | static  | PKIX key against an admitted curve set |
|  [03]   | `decodePKIXECDSASignature(Uint8Array) -> ECDSASignature`                       | static  | DER signature to `(r, s)`              |
|  [04]   | `decodeIEEEP1363ECDSASignature(ECDSANamedCurve, Uint8Array) -> ECDSASignature` | static  | raw P1363 signature to `(r, s)`        |
|  [05]   | `decodePKCS1RSAPublicKey(Uint8Array) -> RSAPublicKey`                          | static  | PKCS1 key to `(n, e)`                  |
|  [06]   | `decodePKIXRSAPublicKey(Uint8Array) -> RSAPublicKey`                           | static  | PKIX key to `(n, e)`                   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `HashAlgorithm` collapses algorithm choice to a constructor value, so a new digest lands as a row on the standing folds.
- `RandomReader` inverts entropy ownership onto the caller; `sign/crypto` binds it once to WebCrypto `crypto.getRandomValues`, so a deterministic reader replaces the one seam.
- `/ecdsa` and `/rsa` serve signatures minted elsewhere: they decode, re-encode, and verify, while `jose` mints every signature Rasm issues.
- WebCrypto `SubtleCrypto` owns the `sign/crypto` AES-GCM envelope; this package backs that page's HMAC, equality, digest, and CSPRNG rows.

[STACKING]:
- `@oslojs/encoding`(`.api/oslojs-encoding.md`): digest bytes cross to `encodeHexLowerCase` for storage and wire, and return through `decodeHex` before `constantTimeEqual` — the paired byte-to-string boundary.
- `otplib`(`.api/otplib.md`): `hmac(SHA256, key, data)` and `constantTimeEqual` satisfy the `CryptoPlugin.hmac`/`.constantTimeEqual` members, and the WebCrypto-filled `RandomReader` feeds `randomBytes`.
- `@node-rs/argon2`(`.api/node-rs-argon2.md`): `generateRandomString` mints the `authn/apikey` body and `authn/otp` recovery codes argon2 `hash` digests at rest.
- `effect`(`.api/effect.md`): every member is synchronous — `Effect.sync` lifts total ops, `Effect.try` the throwing decoders; keys stay `Redacted<Uint8Array>`, unwrapped only inside the sync boundary.
- `sign/crypto` (in-folder owner): one seam folds the HMAC webhook row, the `session/token` compare, and the `authn/apikey`/`authn/otp` mints; `authn/otp` reaches the HMAC only through the `CryptoPlugin` object `sign/crypto` constructs.

[LOCAL_ADMISSION]:
- `sign/` subpaths import this package; the `tests/typescript/_architecture` audit catches an `authn`, `secret`, or `session` rail reaching it directly.
- One-shot `hmac`/`sha*` carry the default; the `HMAC`/`SHA*` classes serve chunked input that must not be buffered.

[RAIL_LAW]:
- Package: `@oslojs/crypto`
- Owns: `HMAC` webhook signing, `constantTimeEqual`, the SHA-1/2/3 and SHAKE digest roster over the `HashAlgorithm` contract, verify-only ECDSA/RSA material, and the `RandomReader`-port derivations
- Accept: `HashAlgorithm` as the one dispatch key, `constantTimeEqual` as the equality for non-argon2 fixed-length secret, digest, and token material, `RandomReader` filled from WebCrypto, the `hmac`/`sha*` one-shots by default, this package as the crypto backend of an `otplib` `CryptoPlugin`
- Reject: a `hmacSha*` per-name call family, `===` or `Buffer.equals` on secret bytes, AES-GCM claimed here, token signing claimed here, a system-RNG read bypassing the `RandomReader` seam, an import outside `sign/`
