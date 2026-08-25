# [TS_CORE_API_HASH_WASM]

`hash-wasm` mints WebAssembly-backed digests across the xxHash, BLAKE, SHA-2/3, Keccak, legacy-MD, and checksum families through one async pattern — `name(data, seed?)` returns a hex `string`, `create<Name>(…seed?)` returns a reusable `IHasher` whose one WASM compile amortizes across an `init`/`update`/`digest()` loop. Every entry is a `Promise`; each `.wasm` is embedded in the JS, so node, bun, browser, and worker run identically with no fetch.

`value/contentKey` is the branch's only import site: it composes `createXXHash128(0, 0)` into the `ContentKey` mint and the sibling factory rows into `Digest`, and every delegate imports that value, never this package.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the two-type substrate every algorithm composes

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [CAPABILITY]                                                                 |
| :-----: | :---------- | :------------ | :--------------------------------------------------------------------------- |
|  [01]   | `IDataType` | type alias    | `string \| Buffer \| Uint8Array \| Uint16Array \| Uint32Array` — input union |
|  [02]   | `IHasher`   | type          | reusable streaming state: `init`/`update`/`digest`, `save`/`load`, sizes     |

[IHASHER]: `init() -> IHasher` `update(IDataType) -> IHasher` `digest("binary") -> Uint8Array` `digest(?"hex") -> string` `save() -> Uint8Array` `load(Uint8Array) -> IHasher` `blockSize: number` `digestSize: number`

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: one parameterized digest pattern — `name(IDataType, …seed?) -> Promise<string>` paired with `create<Name>(…seed?) -> Promise<IHasher>` (PascalCase factory) per algorithm; the roster is seed data, a new digest is one row

| [INDEX] | [SURFACE]                                               | [CAPABILITY]                                                |
| :-----: | :------------------------------------------------------ | :---------------------------------------------------------- |
|  [01]   | `xxhash128(data, seedLow?, seedHigh?)`                  | 64-bit seed as two 32-bit halves; 128-bit → 32 hex          |
|  [02]   | `xxhash3(data, seedLow?, seedHigh?)`                    | two-half seed; 64-bit → 16 hex                              |
|  [03]   | `xxhash64(data, seedLow?, seedHigh?)`                   | two-half seed; 64-bit → 16 hex                              |
|  [04]   | `xxhash32(data, seed?)`                                 | single 32-bit seed; 32-bit → 8 hex                          |
|  [05]   | `blake3(data, bits?, key?)`                             | variable output `bits` (÷8, default 256), optional 32-B key |
|  [06]   | `blake2b(data, bits?, key?)` / `blake2s`                | variable-length keyed digest                                |
|  [07]   | `sha256` / `sha224` / `sha384` / `sha512`               | SHA-2 family, no config                                     |
|  [08]   | `sha1` / `sha3(data, bits?)` / `keccak(data, bits?)`    | SHA-1, SHA-3, Keccak; `bits` selects width                  |
|  [09]   | `md4` / `md5` / `ripemd160` / `whirlpool` / `sm3`       | MD, RIPEMD, Whirlpool, SM3 digests                          |
|  [10]   | `crc32(data, poly?)` / `crc64(data, poly?)` / `adler32` | checksums                                                   |

[KDF_AND_KEYED]: password derivation breaks the pattern — an options object in, an `outputType`-keyed return discriminant out

| [INDEX] | [SURFACE]                                             | [CAPABILITY]                                          |
| :-----: | :---------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `argon2i` / `argon2id` / `argon2d` (`IArgon2Options`) | `outputType:"binary"` → `Uint8Array`, else `string`   |
|  [02]   | `argon2Verify(Argon2VerifyOptions)`                   | `Promise<boolean>`                                    |
|  [03]   | `bcrypt(BcryptOptions)`                               | `outputType`-discriminated `Uint8Array \| string`     |
|  [04]   | `bcryptVerify(BcryptVerifyOptions)`                   | `Promise<boolean>`                                    |
|  [05]   | `scrypt(ScryptOptions)` / `pbkdf2(IPBKDF2Options)`    | derived-key hex / binary                              |
|  [06]   | `createHMAC(Promise<IHasher>, IDataType)`             | wraps another algorithm's factory into a keyed digest |

[IARGON2_OPTIONS]: `password` `salt` `secret?` `iterations` `parallelism` `memorySize` `hashLength` `outputType?: "hex"|"binary"|"encoded"`

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every entry is a `Promise` — the WASM compiles on first await, so a memoized `create<Name>` factory amortizes the compile across a chunk loop while a per-call one-shot recompiles each call; `digest("binary")` returns raw bytes in the same display order the hex renders.

[STACKING]:
- `effect` (`.api/effect.md`): `Effect.promise` compiles; `GlobalValue` memoizes factories; `Schema` brands hex; `Redacted` seals sessions.
- within-lib — `value/contentKey` maps unkeyed XXH128, XXH64, CRC32, and BLAKE3 factories onto `Digest` rows.
- within-lib — `IHasher.save()`/`load()` power nested `Digest.Session` checkpoints.
- HMAC and KDF entrypoints remain external package capabilities for security-owned consumers; core admits no keyed digest row.

[LOCAL_ADMISSION]:
- `value/contentKey` is the one import site of `hash-wasm`; every delegate composes the `Digest`/`ContentKey` value.
- `Uint8Array` is the direct input; a `string` is UTF-8 encoded first, so text hashes only after an explicit encode to bytes.
