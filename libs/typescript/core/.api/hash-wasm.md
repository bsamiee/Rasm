# [hash-wasm] — the WebAssembly XxHash128 kernel behind the one seed-zero ContentKey mint

[PACKAGE_SURFACE]:
- package: `hash-wasm` · version `4.12.0` · license `MIT`
- module: dual — `dist/index.esm.js` (ESM `module`) + `dist/index.umd.js` (UMD `main`); `type: commonjs`; single barrel, no subpaths.
- asset: `dist/lib/index.d.ts` (barrel); every algorithm is a peer module re-exported flat.
- runtime: WebAssembly; each algorithm's `.wasm` is embedded base64 IN the JS (`IEmbeddedWasm { name, data, hash }`) — no separate `.wasm` fetch, no network, runs in node / bun / browser / worker. `MAX_HEAP` bounds a single hashed chunk.
- ABI: every entry is ASYNC (`Promise<…>`) — the WASM module compiles on first await; the streaming factory hands back a reusable `IHasher` so the compile amortizes across an update loop.
- plane: `plane:runtime` (core W0); folder-local to `core`, catalogued only here.
- rail: content-identity / cryptographic-digest.

`hash-wasm` exists in the contract floor for exactly ONE consumed capability: the `XxHash128` seed-zero mint that `value/contentKey` wraps as the single `ContentKey`. `interchange/frame`, the `runtime/browser/fetch` decode worker, and `data/object/store` delegate to that mint — a second mint or a non-zero seed is the named cross-language drift defect. The full digest/KDF surface is documented below because the floor OWNS the boundary: a future core identity (a keyed digest, a wire checksum) is a row on the pattern already here, not a new dependency.

## [01]-[CONTRACT]

Two exported types are the entire substrate; every algorithm composes them. One-shot functions take `IDataType` and return a hex `string`; the `create*` factories return a reusable `IHasher` state machine.

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [CAPABILITY / BOUNDARY]                                                        |
| :-----: | :------------ | :------------ | :----------------------------------------------------------------------------- |
|  [01]   | `IDataType`   | type alias    | `string \| Buffer \| Uint8Array \| Uint16Array \| Uint32Array` — the input union |
|  [02]   | `IHasher`     | type          | reusable streaming state: `init`/`update`/`digest` + `save`/`load` + sizes      |

```ts contract
type ITypedArray = Uint8Array | Uint16Array | Uint32Array
type IDataType = string | Buffer | ITypedArray            // string is UTF-8 encoded before hashing
type IHasher = {
  init: () => IHasher                                      // reset to initial state — call before reuse
  update: (data: IDataType) => IHasher                     // chainable; feeds one chunk
  digest: { (outputType: "binary"): Uint8Array; (outputType?: "hex"): string }   // finalize (default hex)
  save: () => Uint8Array                                   // serialize mid-stream state (as sensitive as the input)
  load: (state: Uint8Array) => IHasher                     // resume a saved state (same hash-wasm build only)
  blockSize: number                                        // algorithm block size in bytes
  digestSize: number                                       // output size in bytes
}
```

## [02]-[ALGORITHM_FAMILY]

The digest surface is ONE parameterized pattern — `{ name(data, …seed?): Promise<string>; createName(…seed?): Promise<IHasher> }` per algorithm — not a fixed API per hash. The roster below is SEED DATA on that pattern; the columns that vary are the seed/config parameters. A new digest is a new row, never a new shape.

| [INDEX] | [ONE-SHOT]                            | [FACTORY]           | [CONFIG AXIS / OUTPUT]                                    |
| :-----: | :------------------------------------ | :------------------ | :-------------------------------------------------------- |
|  [01]   | `xxhash128(data, seedLow?, seedHigh?)`| `createXXHash128`   | 64-bit seed split into two 32-bit halves; 128-bit → 32 hex |
|  [02]   | `xxhash3(data, seedLow?, seedHigh?)`  | `createXXHash3`     | two-half seed; 64-bit → 16 hex                            |
|  [03]   | `xxhash64(data, seedLow?, seedHigh?)` | `createXXHash64`    | two-half seed; 64-bit → 16 hex                            |
|  [04]   | `xxhash32(data, seed?)`               | `createXXHash32`    | single 32-bit seed; 32-bit → 8 hex                        |
|  [05]   | `blake3(data, bits?, key?)`           | `createBLAKE3`      | variable output `bits` (÷8, default 256) + optional 32-B key |
|  [06]   | `blake2b(data, bits?, key?)` / `blake2s` | `createBLAKE2b`/`…2s` | variable-length keyed digest                         |
|  [07]   | `sha256` / `sha224` / `sha384` / `sha512` | `createSHA256`…  | SHA-2 family; no config                                  |
|  [08]   | `sha1` / `sha3(data, bits?)` / `keccak(data, bits?)` | `createSHA1`/`createSHA3`/`createKeccak` | SHA-1, SHA-3, Keccak; `bits` selects width |
|  [09]   | `md4` / `md5` / `ripemd160` / `whirlpool` / `sm3` | `createMD5`… | legacy + regional digests                        |
|  [10]   | `crc32` / `crc64` / `adler32`         | `createCRC32`…      | checksums                                                |

```ts contract
// The consumed row — both seed halves omitted ⇒ the seed-zero mint. Return is lowercase hex in canonical big-endian digest order.
declare function xxhash128(data: IDataType, seedLow?: number, seedHigh?: number): Promise<string>
declare function createXXHash128(seedLow?: number, seedHigh?: number): Promise<IHasher>
declare function blake3(data: IDataType, bits?: number, key?: IDataType): Promise<string>   // keyed-digest exemplar
declare function sha256(data: IDataType): Promise<string>
```

## [03]-[KDF_AND_KEYED]

Password/derivation functions break the digest pattern: they take an options object and carry a return-type discriminant keyed on `outputType`. This is the advanced surface — verify against the object shape, not a positional guess.

| [INDEX] | [SURFACE]                          | [OPTIONS]                | [RETURN DISCRIMINANT]                          |
| :-----: | :--------------------------------- | :----------------------- | :--------------------------------------------- |
|  [01]   | `argon2i` / `argon2id` / `argon2d` | `IArgon2Options`         | `outputType:"binary"` → `Uint8Array`, else `string` |
|  [02]   | `argon2Verify`                     | `Argon2VerifyOptions`    | `Promise<boolean>`                             |
|  [03]   | `bcrypt`                           | `BcryptOptions`          | `outputType`-discriminated `Uint8Array \| string` |
|  [04]   | `bcryptVerify`                     | `BcryptVerifyOptions`    | `Promise<boolean>`                             |
|  [05]   | `scrypt` / `pbkdf2`                | options object           | derived-key hex / binary                       |
|  [06]   | `createHMAC(hash, key)`            | combinator               | takes another algorithm's `Promise<IHasher>`   |

```ts contract
// Conditional return type: the binary variant narrows to Uint8Array, everything else to string.
interface IArgon2Options {
  password: IDataType; salt: IDataType; secret?: IDataType
  iterations: number; parallelism: number; memorySize: number; hashLength: number
  outputType?: "hex" | "binary" | "encoded"
}
declare function argon2id<T extends IArgon2Options>(options: T): Promise<T extends { outputType: "binary" } ? Uint8Array : string>
// HMAC is a COMBINATOR — it wraps another algorithm's factory, not a fixed sha256-only helper:
declare function createHMAC(hash: Promise<IHasher>, key: IDataType): Promise<IHasher>
//   createHMAC(createSHA256(), key)  →  keyed SHA-256; swap the inner factory for any digest.
```

## [04]-[CONTENTKEY_MINT]

The one consumed rail. `value/contentKey` calls `xxhash128(bytes)` with both seed halves defaulted to `0` — the seed-zero mint — and the returned hex IS the canonical `:x32` (32-hex-char) `ContentKey` directly: hash-wasm emits big-endian digest order, byte-identical to the C# `System.IO.Hashing.XxHash128` seed-0 `:x32` rendering with NO normalize step (the frozen corpus vector hashes to `9462a71a5dd13dcfa3b1d6d225fcbe70` from both mints). A hand-rolled byte-order shuffle on this path is a defect, not a compatibility layer. One asymmetry survives at the BYTES level only: `digest("binary")` returns the 16 raw bytes in that same display order — the reverse of the C# destination-buffer little-endian memory dump the corpus manifest records — so raw-buffer parity compares reverse one side, while hex parity is direct. The `tests/contracts` corpus parity drivers (TS readers in `tests/typescript/_testkit`) assert bit-identity against the frozen `ContentHash` corpus.

- Small payload: `await xxhash128(bytes)` — one call, seed-zero.
- Large / chunked payload: `createXXHash128(0, 0)` once, then `hasher.init().update(chunk)…digest()` — the WASM compile amortizes; `digest("binary")` returns the raw 16 bytes when the brand wants bytes before hexing.
- The two-half seed `(seedLow, seedHigh)` is the split of a C# 64-bit `XxHash128` seed; the ContentKey pins BOTH to zero, so any non-zero seed is out of contract.

## [05]-[INTEGRATION]

[STACK: `hash-wasm` + `effect/Schema` + `Effect`] — the mint is not a bare `Promise<string>`; it lands as a `Schema`-branded `ContentKey`. `Effect.promise` carries the memoized `createXXHash128(0, 0)` compile (the digest cannot fail once bytes exist), the synchronous `init`/`update`/`digest()` walk emits the hex, and `Schema.decode(ContentKey)` brands it directly. The brand's refinement is the `:x32` shape (32 lowercase hex); `Schema` guarantees no `{value}` re-decode downstream.

[STACK: compile-once lifecycle] — every entry is async because the WASM compiles on first await. The floor memoizes the `createXXHash128(0, 0)` promise as a module singleton so the compile happens once per runtime, not per mint; `hasher.init()` between mints resets state without recompiling. A per-call `xxhash128` is correct for one-off small payloads but recompiles-and-runs each call — reserve it for the single-mint case.

[STACK: delegate law] — `interchange/frame`, `runtime/browser/fetch`, and `data/object/store` never import `hash-wasm`; they import `value/contentKey`. This catalog documents the substrate the floor internalizes; downstream folders compose the `ContentKey` VALUE, never the hasher. The C# parity seam (`Rasm/Spatial/reconciliation` mints, `Rasm.Compute/Runtime` two-half digest vectors) is asserted read-only through the `tests/contracts` corpus parity drivers.

## [06]-[RAIL_LAW]

- Owns: WASM-backed content digests and KDFs; the seed-zero `XxHash128` mint the whole branch's content identity derives from.
- Accept: `xxhash128(bytes)` / `createXXHash128(0,0)` for the ContentKey; `IDataType` inputs (prefer `Uint8Array` — a `string` input is UTF-8 encoded first); the streaming `IHasher` for chunked payloads; `createHMAC(createDIGEST(), key)` when a keyed digest is ever needed.
- Reject: a non-zero seed on the ContentKey path; a second content-address notion or a non-`xxhash128` content key; re-minting in a delegate folder instead of importing `value/contentKey`; treating any entry as sync — every one is a `Promise`.
- Boundary: hex output is already the canonical big-endian digest — a byte-order shuffle on the hex path is a defect; `digest("binary")` bytes are display-ordered (the reverse of the C# little-endian destination buffer), and every cross-language parity claim is byte-level against the frozen corpus, never a re-hash comparison.
