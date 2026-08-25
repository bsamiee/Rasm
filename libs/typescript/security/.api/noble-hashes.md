# [TS_SECURITY_API_NOBLE_HASHES]

`@noble/hashes` owns the audited synchronous symmetric primitives the crypto authority composes: HMAC over any hash value, the SHA-1/2/3 digest roster, and a WebCrypto-backed `randomBytes`. It carries no asymmetric surface — ECDSA and RSA verification ride WebCrypto `subtle.verify` at `crypt/verify` — and no constant-time equality worth trusting, so the timing-safe compare and the unbiased alphabet sampler home folder-local. Version 2.x publishes explicit-extension ESM subpaths (`@noble/hashes/<sub>.js`), so a tree-shaker drops every unreached digest.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `/hmac.js` `/sha2.js` `/legacy.js` — the hash value contract and the streaming MAC carrier. Every `CHash` is a callable `(msg) -> Uint8Array` that also constructs an incremental instance, so the algorithm crosses as a value the fold ranges over rather than a per-name call family.

| [INDEX] | [SYMBOL]                                      | [TYPE_FAMILY] | [CAPABILITY]                                           |
| :-----: | :-------------------------------------------- | :------------ | :----------------------------------------------------- |
|  [01]   | `CHash { (msg): Uint8Array; create(): Hash }` | interface     | hash passed as a value, also incremental               |
|  [02]   | `Hash { update(buf), digest(): Uint8Array }`  | interface     | incremental digest and MAC contract                    |
|  [03]   | `TRet<T>`                                     | type wrapper  | TS 5.6/5.9 byte-output compat; values stay synchronous |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `/hmac.js` — MAC, synchronous over raw bytes, one-shot and streaming off one owner.

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :---------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `hmac(CHash, Uint8Array, Uint8Array) -> Uint8Array`   | static   | one-shot MAC over a buffered body     |
|  [02]   | `hmac.create(CHash, Uint8Array).update(buf).digest()` | instance | chunked MAC over a prefix then a body |

[ENTRYPOINT_SCOPE]: `/sha2.js` `/legacy.js` `/sha3.js` — every digest is a `CHash` callable; one polymorphic fold ranges over the value set, and SHA-1 lives on `/legacy.js` under its legacy posture.

- [SHA2]: `sha256` `sha384` `sha512` `sha224` `sha512_256` — `/sha2.js`
- [SHA1]: `sha1` — `/legacy.js`, the RFC-6238 TOTP compatibility digest alone
- [SHA3]: `sha3_256` `sha3_512` `keccak_256` — `/sha3.js`, unreached by the folder today

[ENTRYPOINT_SCOPE]: `/utils.js` — the entropy read the `Entropy` port and the sampler draw.

| [INDEX] | [SURFACE]                            | [SHAPE] | [CAPABILITY]                                   |
| :-----: | :----------------------------------- | :------ | :--------------------------------------------- |
|  [01]   | `randomBytes(number) -> Uint8Array`  | static  | WebCrypto `getRandomValues` fill, one buffer   |
|  [02]   | `equalBytes(Uint8Array, Uint8Array)` | static  | length-and-content compare — NOT constant-time |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Hashes cross as values — `hmac(sha256, key, body)` and `sha256(bytes)` take the algorithm as the `CHash` itself, so a new digest is a row on the standing `_HASHES` fold, never a `hmacSha256` name fork.
- `hmac.create` streams a signed prefix ahead of a held body without a joined copy, the byte-identical replacement for a one-shot over a concatenated buffer.
- `randomBytes` reads WebCrypto internally, but `sign/crypto` draws entropy through its own `Entropy` port (a `{ read(bytes) }` seam) so a deterministic reader replaces the one seam under test — the port owns injection, not this package.
- This package carries NO asymmetric surface: ECDSA and RSA verification are WebCrypto `subtle.verify` at `crypt/verify`, and no member here decodes or verifies a public-key signature.

[STACKING]:
- `@oslojs/encoding`(`.api/oslojs-encoding.md`): digest and MAC bytes cross to `encodeHexLowerCase` for storage and wire and return through `decodeHex` before the folder compare — the paired byte-to-string boundary this package's `Uint8Array` output feeds.
- `otplib`(`.api/otplib.md`): `hmac(_HASHES[alg], key, data)` satisfies the `CryptoPlugin.hmac` member, so OTP HMAC rides the primitive `sign/crypto` owns and the bundled `NobleCryptoPlugin` path is bypassed for one folder HMAC owner.
- `effect`(`.api/effect.md`): every member is synchronous — `Effect.try` lifts the total ops, keys stay `Redacted<Uint8Array>` unwrapped only inside the sync boundary.
- `sign/crypto` (in-folder owner): one seam folds the HMAC webhook row, the `session/token` compare, and the `authn/apikey`/`authn/otp` mints; the constant-time compare `_sameBytes`, the unbiased alphabet sampler `_sample`, and the `Entropy` port are folder-owned beside these primitives because no member here supplies a timing-safe equality or a bounded-alphabet mint.

[LOCAL_ADMISSION]:
- `crypt/sign` imports this package alone.
- One-shot `hmac`/`sha*` carry the default; `hmac.create` serves the chunked prefix-then-body input that must not buffer.
