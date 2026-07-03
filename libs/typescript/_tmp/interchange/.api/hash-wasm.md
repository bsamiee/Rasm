# [API_CATALOGUE] hash-wasm

`hash-wasm` supplies WebAssembly hash implementations across one parameterized surface: 22 digest algorithms (xxHash 32/64/XXH3/128, CRC32/CRC64, the SHA-1/2/3 + Keccak family, BLAKE2b/2s/BLAKE3, MD4/MD5, RIPEMD-160, Whirlpool, SM3, Adler-32), four password-KDF families (PBKDF2, scrypt, bcrypt, Argon2i/id/d), and HMAC. Every non-KDF algorithm exposes the SAME two-shape pattern — a one-shot `<algo>(data, …params): Promise<string>` hex function and a `create<Algo>(…params): Promise<IHasher>` stateful factory resolving a chunked-incremental `IHasher`. The interchange branch admits `createXXHash128(0, 0)` as the 16-byte content-key digest and reads `createXXHash3`/`createCRC32`/`createCRC64` as the per-frame integrity floor; one wasm owns the surface, retiring the prior `xxhash-wasm` 32/64-only rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `hash-wasm`
- package: `hash-wasm` (4.12.0, MIT)
- module format: ESM (`./dist/index.esm.js`) + UMD (`./dist/index.umd.js`), `sideEffects: false`; type barrel at `./dist/lib/index.d.ts` re-exporting the 27 per-algorithm modules
- runtime target: isomorphic (browser, node, worker); requires a `WebAssembly` runtime; every entry is ASYNC because the factory `Promise` gates the one-time wasm compile — no synchronous digest exists
- asset: WASM binaries base64-embedded inside the JS modules (`IEmbeddedWasm`), decoded and `WebAssembly.instantiate`d on first factory call; NO external `.wasm` fetch, so the digest works offline and inside a sandboxed worker
- rail: content-hash

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: hasher and input types
- rail: content-hash

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]     | [RAIL]                                                     |
| :-----: | :------------ | :---------------- | :--------------------------------------------------------- |
|  [01]   | `IHasher`     | stateful hasher   | incremental session: `init`/`update`/`digest`/`save`/`load` + `blockSize`/`digestSize` |
|  [02]   | `IDataType`   | input union       | `string \| Buffer \| ITypedArray`                          |
|  [03]   | `ITypedArray` | typed-array union | `Uint8Array \| Uint16Array \| Uint32Array`                 |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the parameterized two-shape pattern (owns all 22 digest algorithms)
- rail: content-hash

Every non-KDF algorithm is one row of ONE pattern — a one-shot hex function and a stateful factory over the same param axis — never a bespoke API per algorithm. The six param shapes below are the whole axis; the algorithm name is data.

| [INDEX] | [FAMILY_SHAPE]        | [FACTORY]                                   | [ONE_SHOT]                                | [PARAM_AXIS]                                                  |
| :-----: | :-------------------- | :------------------------------------------ | :---------------------------------------- | :----------------------------------------------------------- |
|  [01]   | seed two-half         | `createXXHash64/3/128(seedLow?, seedHigh?)` | `xxhash64/3/128(data, seedLow?, seedHigh?)` | two 32-bit seed halves, both default `0`                     |
|  [02]   | seed single           | `createXXHash32(seed?)`                     | `xxhash32(data, seed?)`                   | one 32-bit seed, default `0`                                 |
|  [03]   | polynomial            | `createCRC32(polynomial?)` / `createCRC64(polynomial?: string)` | `crc32(data, polynomial?)` / `crc64(data, polynomial?)` | CRC32 default `0xedb88320` (CRC32C `0x82f63b78`); CRC64 poly as hex string |
|  [04]   | bits (+ optional key) | `createBLAKE2b/BLAKE2s/BLAKE3(bits?, key?)`, `createSHA3/Keccak(bits?)` | `blake2b/blake2s/blake3(data, bits?, key?)`, `sha3/keccak(data, bits?)` | output `bits` (`IValidBits` for SHA3/Keccak); optional keyed BLAKE |
|  [05]   | no-param              | `createSHA1/224/256/384/512`, `createMD4/MD5`, `createRIPEMD160`, `createWhirlpool`, `createSM3`, `createAdler32` | `sha1/…/adler32(data)` | none                                                         |
|  [06]   | keyed MAC             | `createHMAC(hash: Promise<IHasher>, key: IDataType)` | —                                | composes over ANY factory `Promise` (e.g. `createHMAC(createSHA256(), key)`) |

[ENTRYPOINT_SCOPE]: password-KDF families (option-object one-shots; no incremental factory)
- rail: content-hash

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY] | [RAIL]                                                           |
| :-----: | :----------------------------------------------------------- | :------------- | :-------------------------------------------------------------- |
|  [01]   | `pbkdf2({ password, salt, iterations, hashLength, hashFunction, outputType? })` | KDF one-shot | `Promise<string \| Uint8Array>` by `outputType`; `hashFunction` composes over a `create<Algo>()` Promise |
|  [02]   | `scrypt(options)` / `bcrypt(options)` / `argon2i/argon2id/argon2d(options)` | KDF one-shot | option-object derivation; `outputType: "binary"` narrows the return to `Uint8Array` |
|  [03]   | `bcryptVerify(options)` / `argon2Verify(options)`            | KDF verify     | `Promise<boolean>` constant-time password verification         |

[ENTRYPOINT_SCOPE]: admitted rows — the content key and the per-frame integrity floor
- rail: content-hash

| [INDEX] | [SURFACE]                                                                          | [ENTRY_FAMILY]   | [RAIL]                                                                 |
| :-----: | :--------------------------------------------------------------------------------- | :--------------- | :-------------------------------------------------------------------- |
|  [01]   | `createXXHash128(seedLow?=0, seedHigh?=0): Promise<IHasher>`                        | 128-bit factory  | the `ContentKey` mint — `digest("binary")` → 16-byte little-endian digest |
|  [02]   | `xxhash128(data, seedLow?=0, seedHigh?=0): Promise<string>`                         | 128-bit one-shot | 32-char hex; the one-shot mirror of the factory                       |
|  [03]   | `createXXHash3(0, 0)` / `createCRC32()` / `createCRC64()`                           | integrity floor  | the per-frame/whole-artifact integrity surface the README names       |

[ENTRYPOINT_SCOPE]: IHasher instance operations
- rail: content-hash

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :----------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `init(): IHasher`                          | reset          | reinitializes state to default, returns `this`  |
|  [02]   | `update(data: IDataType): IHasher`         | chain method   | feeds a chunk, returns `this`                   |
|  [03]   | `digest(outputType?: "hex"): string`       | finalize       | 32-char hexadecimal digest (`createXXHash128`)  |
|  [04]   | `digest(outputType: "binary"): Uint8Array` | finalize       | 16-byte `Uint8Array` digest (overload)          |
|  [05]   | `save(): Uint8Array`                       | snapshot       | serializes internal state for later `load`      |
|  [06]   | `load(state: Uint8Array): IHasher`         | restore        | resumes a `save()` state, returns `this`        |
|  [07]   | `blockSize`                                | property       | block size in bytes (`512` for XXHash128)       |
|  [08]   | `digestSize`                               | property       | digest size in bytes (`16` for XXHash128)       |

## [04]-[IMPLEMENTATION_LAW]

[HASH_TOPOLOGY]:
- every entry is async; `create*` factories and one-shot functions both return a `Promise`, and the `IHasher` handle is unavailable until that `Promise` resolves the wasm instance
- `createXXHash128(seedLow, seedHigh)` seeds the 128-bit state from two 32-bit halves, both default `0`; `xxhash3`/`xxhash64` share the two-half seed, `xxhash32` a single seed
- `digest()` defaults to a 32-character hexadecimal string; `digest("binary")` returns a `digestSize`-byte `Uint8Array` (16 for XXHash128) — the overload return is discriminated by the literal argument
- `init`, `update`, and `load` return the same `IHasher`, supporting chained calls; `digest` is terminal for the current cycle and `init` reopens it
- `save` cannot be called before `init` or after `digest`; the serialized state may embed plaintext bytes of the hashed value and is as sensitive as the input; `load` throws if the state was not produced by a compatible `hash-wasm` build
- `createHMAC` and `pbkdf2` COMPOSE over a factory `Promise` (`createHMAC(createSHA256(), key)`, `pbkdf2({ hashFunction: createSHA512(), … })`), so the keyed and derived surfaces are parameterized by the same factory axis, never a per-algorithm HMAC/KDF

[ENDIANNESS_LAW]:
- `createXXHash128` emits its digest in little-endian byte order, while the C# `System.IO.Hashing.XxHash128` persists big-endian; `Codec/frame.md` `xxHash128Of` byte-reverses the 16-byte `digest("binary")` once at the `h128` boundary so the branch holds the C# big-endian canonical key
- compare normalized bytes, never the raw hex strings, when checking cross-runtime content-key parity

[STACKS_WITH]:
- `effect` (`.api/effect.md`): `createXXHash128(0, 0)` is resolved ONCE at the `platform` `DecodeWorkerPool` composition root and threaded through the `Effect.Service` layer as the `XxHash128` interface — never re-resolved per artifact; `Codec/frame.md` `xxHash128Of` binds it as `Schema.decodeSync(ContentKey)(hasher.init().update(bytes).digest("binary").reverse())`, so the raw `IHasher` never leaks past its owner and the 16-byte digest lands in the branded `ContentKey` slot; `save`/`load` checkpoint a partial digest across the transferable worker boundary that `Stream.toReadableStream`/`Stream.fromReadableStreamByob` marshal
- `Codec/frame.md` CRC seam: the design OWNS a synchronous table-driven `Crc32` built from the polynomial `0xedb88320` — the exact default `createCRC32` uses — instead of hash-wasm's `createCRC32`, because the per-frame verify is a hot synchronous number-eq compare (`crc.of(payload) !== frame.frameCrc`) that cannot afford an async `Promise` boundary or a hex-string round-trip per 64-KiB frame; `createCRC32`/`createCRC64`/`createXXHash3` remain the admitted async surface for whole-artifact or integrity-floor checks where the async boundary is amortized
- `Codec/parity.md` (`.api/msgpack-msgpack.md` bigint seam): `ContentKeyParity` routes the SAME `xxHash128Of` binding to reproduce the frozen `CANONICAL_BYTE_IDENTITY` corpus digest bit-identically, sharing the single-hash-mint path so a green parity assertion proves the worker reassembly reproduces the C# seed
- future keyed/derived rail: `createHMAC` and the KDF families back a designed-only signed-frame or key-derivation lane, composing over the same factory `Promise` the content-key rail already resolves — never a second hashing dependency

[LOCAL_ADMISSION]:
- Resolve `createXXHash128` once at the worker composition root; pass the `IHasher` through the Effect service layer rather than re-resolving per call.
- Use `update` over chunked `Uint8Array` payloads for streamed frames; `save`/`load` checkpoint a partial digest across transferable worker boundaries.
- `IDataType` accepts `string`, `Buffer`, and `Uint8Array`/`Uint16Array`/`Uint32Array`; feed raw frame bytes without a string round-trip.
- Prefer the `create<Algo>` factory over the one-shot when hashing chunked or streamed input; the one-shot re-resolves the wasm per call.

[RAIL_LAW]:
- Package: `hash-wasm`
- Owns: 22 digest algorithms (xxHash 32/64/XXH3/128, CRC32/CRC64, SHA-1/2/3 + Keccak, BLAKE2b/2s/BLAKE3, MD4/MD5, RIPEMD-160, Whirlpool, SM3, Adler-32) as a factory+one-shot pattern, four password-KDF families, and HMAC, all over `IDataType` input and the `IHasher` incremental session
- Accept: `string`, `Buffer`, or `ITypedArray` input; `number` seed halves; `number`/`string` CRC polynomials; a `create<Algo>()` `Promise` as the HMAC/PBKDF2 inner hash
- Reject: hand-rolled xxHash; raw-hex cross-runtime parity checks that skip endianness normalization; synchronous WASM evaluation before the factory `Promise` resolves; hash-wasm's async `createCRC32` on the hot per-frame verify path (the owned synchronous `Crc32` is the correct floor there)
