# [TS_CORE_API_HASH_WASM]

[PACKAGE_SURFACE]:
- package: `hash-wasm` (MIT)
- module: dual — `dist/index.esm.js` (ESM `module`) + `dist/index.umd.js` (UMD `main`); `type: commonjs`; single barrel, no subpaths.
- asset: `dist/lib/index.d.ts` (barrel); every algorithm is a peer module re-exported flat.
- runtime: WebAssembly; each algorithm's `.wasm` is embedded base6 catalog IN the JS (`IEmbeddedWasm { name, data, hash }`) — no separate `.wasm` fetch, no network, runs in node / bun / browser / worker. `MAX_HEAP` bounds a single hashed chunk.
- ABI: every entry is ASYNC (`Promise<…>`) — the WASM module compiles on first await; the streaming factory hands back a reusable `IHasher` so the compile amortizes across an update loop.
- plane: `plane:runtime` (core W0); folder-local to `core`, catalogued only here.
- rail: content-identity / cryptographic-digest.

`hash-wasm` is consumed through exactly ONE owner — `value/contentKey`'s `Digest` row table — whose census is: `content` (`createXXHash128(0, 0)`, the cross-language `ContentKey`), `trace` (`createXXHash64(0, 0)`, the short correlation address), `check` (`createCRC32()`, the wire checksum), `proof` (`createBLAKE3(256)`, the Merkle proof-tree digest), the keyed seal mint (`createBLAKE3(256, key)` over a `Redacted` 32-byte key), and the resumable session algebra over `IHasher.save()`/`load()`. Every delegate — `interchange/frame`, the `runtime/browser/fetch` decode worker, `data/object/{store,stream,file}` — imports `Digest`, never this package; a second mint site or a non-zero seed on any content-address row is the named cross-language drift defect. The full package surface is documented below; the digest boundary is the floor's — a future core identity is a row on the pattern already here, not a new dependency — while the KDF family is the security folder's consumer surface: secret derivation never lands on `value/contentKey`, and the digest engine never carries a KDF surface.

## [01]-[CONTRACT]

Two exported types are the entire substrate; every algorithm composes them. One-shot functions take `IDataType` and return a hex `string`; the `create*` factories return a reusable `IHasher` state machine.

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [CAPABILITY_BOUNDARY]                                                            |
| :-----: | :---------- | :------------ | :------------------------------------------------------------------------------- |
|  [01]   | `IDataType` | type alias    | `string \| Buffer \| Uint8Array \| Uint16Array \| Uint32Array` — the input union |
|  [02]   | `IHasher`   | type          | reusable streaming state: `init`/`update`/`digest` + `save`/`load` + sizes       |

[ITYPED_ARRAY]: `ITypedArray = Uint8Array|Uint16Array|Uint32Array`
[IDATA_TYPE]: `IDataType = string|Buffer|ITypedArray`
[IHASHER]: `IHasher.init: ()=>IHasher` `IHasher.update: (data:IDataType)=>IHasher` `IHasher.digest: {(outputType:"binary"):Uint8Array;(outputType?:"hex"):string}` `IHasher.save: ()=>Uint8Array` `IHasher.load: (state:Uint8Array)=>IHasher` `IHasher.blockSize: number` `IHasher.digestSize: number`

## [02]-[ALGORITHM_FAMILY]

The digest surface is ONE parameterized pattern — `{ name(data, …seed?): Promise<string>; createName(…seed?): Promise<IHasher> }` per algorithm — not a fixed API per hash. Every one-shot `name(...)` has a `create<Name>(...)` streaming factory (PascalCase of the name, e.g. `xxhash128`→`createXXHash128`, `blake2b`→`createBLAKE2b`, `md5`→`createMD5`). The roster below is SEED DATA on that pattern; the columns that vary are the seed/config parameters. A new digest is a new row, never a new shape.

| [INDEX] | [ONE_SHOT]                                           | [CONFIG_AXIS_OUTPUT]                                         |
| :-----: | :--------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `xxhash128(data, seedLow?, seedHigh?)`               | 64-bit seed split into two 32-bit halves; 128-bit → 32 hex   |
|  [02]   | `xxhash3(data, seedLow?, seedHigh?)`                 | two-half seed; 64-bit → 16 hex                               |
|  [03]   | `xxhash64(data, seedLow?, seedHigh?)`                | two-half seed; 64-bit → 16 hex                               |
|  [04]   | `xxhash32(data, seed?)`                              | single 32-bit seed; 32-bit → 8 hex                           |
|  [05]   | `blake3(data, bits?, key?)`                          | variable output `bits` (÷8, default 256) + optional 32-B key |
|  [06]   | `blake2b(data, bits?, key?)` / `blake2s`             | variable-length keyed digest                                 |
|  [07]   | `sha256` / `sha224` / `sha384` / `sha512`            | SHA-2 family; no config                                      |
|  [08]   | `sha1` / `sha3(data, bits?)` / `keccak(data, bits?)` | SHA-1, SHA-3, Keccak; `bits` selects width                   |
|  [09]   | `md4` / `md5` / `ripemd160` / `whirlpool` / `sm3`    | retired + regional digests                                   |
|  [10]   | `crc32` / `crc64` / `adler32`                        | checksums                                                    |

[SURFACES]: `xxhash128(IDataType,number?,number?) -> Promise<string>` `createXXHash128(number?,number?) -> Promise<IHasher>` `blake3(IDataType,number?,IDataType?) -> Promise<string>` `sha256(IDataType) -> Promise<string>`

## [03]-[KDF_AND_KEYED]

Password/derivation functions break the digest pattern: they take an options object and carry a return-type discriminant keyed on `outputType`. This is the advanced surface — verify against the object shape, not a positional guess. The KDF rows are the security folder's consumer surface; `createHMAC` is the keyed-digest combinator the `Digest` seal row grows onto when a peer contract demands it.

| [INDEX] | [SURFACE]                          | [OPTIONS]             | [RETURN_DISCRIMINANT]                               |
| :-----: | :--------------------------------- | :-------------------- | :-------------------------------------------------- |
|  [01]   | `argon2i` / `argon2id` / `argon2d` | `IArgon2Options`      | `outputType:"binary"` → `Uint8Array`, else `string` |
|  [02]   | `argon2Verify`                     | `Argon2VerifyOptions` | `Promise<boolean>`                                  |
|  [03]   | `bcrypt`                           | `BcryptOptions`       | `outputType`-discriminated `Uint8Array \| string`   |
|  [04]   | `bcryptVerify`                     | `BcryptVerifyOptions` | `Promise<boolean>`                                  |
|  [05]   | `scrypt` / `pbkdf2`                | options object        | derived-key hex / binary                            |
|  [06]   | `createHMAC(hash, key)`            | combinator            | takes another algorithm's `Promise<IHasher>`        |

[IARGON2_OPTIONS]: `IArgon2Options.password: IDataType` `IArgon2Options.salt: IDataType` `IArgon2Options.secret: IDataType` `IArgon2Options.iterations: number` `IArgon2Options.parallelism: number` `IArgon2Options.memorySize: number` `IArgon2Options.hashLength: number` `IArgon2Options.outputType: "hex"|"binary"|"encoded"`
[SURFACES]: `argon2id(T) -> Promise<T extends{outputType:"binary"}?Uint8Array:string>` `createHMAC(Promise<IHasher>,IDataType) -> Promise<IHasher>`

## [04]-[CONTENTKEY_MINT]

The anchor row of the consumed table. `value/contentKey` calls `xxhash128(bytes)` with both seed halves defaulted to `0` — the seed-zero mint — and the returned hex IS the canonical `:x32` (32-hex-char) `ContentKey` directly: hash-wasm emits big-endian digest order, byte-identical to the C# `System.IO.Hashing.XxHash128` seed-0 `:x32` rendering with NO normalize step (the frozen corpus vector hashes to `9462a71a5dd13dcfa3b1d6d225fcbe70` from both mints). A hand-rolled byte-order shuffle on this path is a defect, not a compatibility layer. One asymmetry survives at the BYTES level only: `digest("binary")` returns the 16 raw bytes in that same display order — the reverse of the C# destination-buffer little-endian memory dump the corpus manifest records — so raw-buffer parity compares reverse one side, while hex parity is direct. The `tests/contracts` corpus parity drivers (TS readers in `tests/typescript/_testkit`) assert bit-identity against the frozen `ContentHash` corpus.

- Small payload: `await xxhash128(bytes)` — one call, seed-zero.
- Large / chunked payload: `createXXHash128(0, 0)` once, then `hasher.init().update(chunk)…digest()` — the WASM compile amortizes; `digest("binary")` returns the raw 16 bytes when the brand wants bytes before hexing.
- The two-half seed `(seedLow, seedHigh)` is the split of a C# 64-bit `XxHash128` seed; the ContentKey pins BOTH to zero, so any non-zero seed is out of contract.

## [05]-[INTEGRATION]

[STACK: `hash-wasm` + `effect/Schema` + `Effect`] — the mint is not a bare `Promise<string>`; it lands as a `Schema`-branded `ContentKey`. `Effect.promise` carries the memoized `createXXHash128(0, 0)` compile (the digest cannot fail once bytes exist), the synchronous `init`/`update`/`digest()` walk emits the hex, and `Schema.decode(ContentKey)` brands it directly. The brand's refinement is the `:x32` shape (32 lowercase hex); `Schema` guarantees no `{value}` re-decode downstream.

[STACK: compile-once lifecycle] — every entry is async because the WASM compiles on first await. The floor memoizes the `createXXHash128(0, 0)` promise as a module singleton so the compile happens once per runtime, not per mint; `hasher.init()` between mints resets state without recompiling. A per-call `xxhash128` is correct for one-off small payloads but recompiles-and-runs each call — reserve it for the single-mint case.

[STACK: delegate law] — `interchange/frame`, `runtime/browser/fetch`, and `data/object/store` never import `hash-wasm`; they import `value/contentKey`. This catalog documents the substrate the floor internalizes; downstream folders compose the `ContentKey` VALUE, never the hasher. The C# parity seam (`Rasm/Spatial/reconciliation` mints, `Rasm.Compute/Runtime` two-half digest vectors) is asserted read-only through the `tests/contracts` corpus parity drivers.

## [06]-[RAIL_LAW]

- Owns: WASM-backed content digests; the seed-zero `XxHash128` mint the whole branch's content identity derives from, plus the `Digest` table's sibling rows (`createXXHash64(0, 0)` trace, `createCRC32()` check, `createBLAKE3(256)` proof) and the keyed `createBLAKE3(256, key)` seal. KDF consumption (`argon2id`, `bcrypt`, `scrypt`, `pbkdf2`) is the security folder's.
- Accept: `xxhash128(bytes)` / `createXXHash128(0,0)` for the ContentKey; the `Digest` table's sibling factory rows behind `value/contentKey` only; `IDataType` inputs (prefer `Uint8Array` — a `string` input is UTF-8 encoded first); the streaming `IHasher` (`save`/`load` sessions) for chunked payloads; `createHMAC(createDIGEST(), key)` when a peer contract ever demands an HMAC construction.
- Reject: a non-zero seed on the ContentKey path; a second content-address notion or a non-`xxhash128` content key; re-minting in a delegate folder instead of importing `value/contentKey`; treating any entry as sync — every one is a `Promise`.
- Boundary: hex output is already the canonical big-endian digest — a byte-order shuffle on the hex path is a defect; `digest("binary")` bytes are display-ordered (the reverse of the C# little-endian destination buffer), and every cross-language parity claim is byte-level against the frozen corpus, never a re-hash comparison.
