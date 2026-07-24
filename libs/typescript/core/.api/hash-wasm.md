# [TS_CORE_API_HASH_WASM]

`hash-wasm` mints WebAssembly-backed digests across the xxHash, BLAKE, SHA-2/3, Keccak, legacy-MD, and checksum families through one async pattern — `name(data, seed?)` returns a hex `string`, `create<Name>(…seed?)` returns a reusable `IHasher` whose one WASM compile amortizes across an `init`/`update`/`digest()` loop. Every entry is a `Promise`; each `.wasm` is embedded in the JS, so node, bun, browser, and worker run identically with no fetch.

`value/contentKey` is the branch's only import site: it composes `createXXHash128(0, 0)` into the `ContentKey` mint and the sibling factory rows into `Digest`, and every delegate imports that value, never this package.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `hash-wasm`
- package: `hash-wasm` (MIT)
- module: dual — `dist/index.esm.js` (ESM) + `dist/index.umd.js` (UMD); one flat `dist/lib/index.d.ts` barrel re-exporting every algorithm module, no subpaths
- runtime: WebAssembly embedded in the JS — no `.wasm` fetch, no network; `MAX_HEAP` bounds one hashed chunk
- rail: content-identity / cryptographic-digest

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the two-type substrate every algorithm composes

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [CAPABILITY]                                                                 |
| :-----: | :---------- | :------------ | :--------------------------------------------------------------------------- |
|  [01]   | `IDataType` | type alias    | `string \| Buffer \| Uint8Array \| Uint16Array \| Uint32Array` — input union |
|  [02]   | `IHasher`   | type          | reusable streaming state: `init`/`update`/`digest`, `save`/`load`, sizes     |

[IHASHER]: `init() -> IHasher` `update(IDataType) -> IHasher` `digest("binary") -> Uint8Array` `digest(?"hex") -> string` `save() -> Uint8Array` `load(Uint8Array) -> IHasher` `blockSize: number` `digestSize: number`

## [03]-[ENTRYPOINTS]

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

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every entry is a `Promise` — the WASM compiles on first await, so a memoized `create<Name>` factory amortizes the compile across a chunk loop while a per-call one-shot recompiles each call; `digest("binary")` returns raw bytes in the same display order the hex renders.

[STACKING]:
- `effect` (`.api/effect.md`): `Effect.promise` carries the compile, `GlobalValue.globalValue` memoizes each factory promise per runtime, `Schema.decode`/`Schema.brand` brand the digest hex, `Redacted` seals the keyed-mac key to one unwrap at the mint.
- within-lib — `value/contentKey` composes the whole surface: `createXXHash128(0, 0)` → the `Digest` `content` row → branded `ContentKey`, the sibling factory rows (`createXXHash64(0, 0)`, `createCRC32()`, `createBLAKE3(256)`, keyed `createBLAKE3(256, key)`) as `Digest` width rows, and `IHasher.save()`/`load()` as the `Digest.session`/`absorb`/`finish` checkpoint algebra; `createHMAC(create<Hash>(), key)` grows the keyed-digest combinator a peer HMAC contract demands.

[LOCAL_ADMISSION]:
- `value/contentKey` is the one import site of `hash-wasm`; every delegate composes the `Digest`/`ContentKey` value.
- `Uint8Array` is the direct input; a `string` is UTF-8 encoded first, so text hashes only after an explicit encode to bytes.

[RAIL_LAW]:
- Package: `hash-wasm`
- Owns: WASM-backed digests across the xxHash, BLAKE, SHA-2/3, Keccak, MD/RIPEMD/Whirlpool/SM3, and CRC/Adler families, the streaming `IHasher` with `save`/`load` sessions, and the `createHMAC` combinator; the KDF family (`argon2*`, `bcrypt`, `scrypt`, `pbkdf2`) is the security folder's consumer surface, off the digest floor.
- Accept: the one-shot `name(data, seed?)` for a single small payload, the memoized `create<Name>(…seed?)` factory for a streaming or repeated payload, `IDataType` inputs, `digest("binary")` for raw bytes, `createHMAC(create<Hash>(), key)` when a peer contract demands an HMAC.
- Reject: a byte-order shuffle on the hex path — the hex is already canonical; treating any entry as synchronous — every one is a `Promise`; a second import site outside `value/contentKey`.
