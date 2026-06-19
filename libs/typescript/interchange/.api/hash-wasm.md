# [API_CATALOGUE] hash-wasm

`hash-wasm` supplies WebAssembly hash implementations across the xxHash family (32-bit, 64-bit, XXH3, 128-bit), CRC, and cryptographic digests, exposing both one-shot `Promise<string>` functions and `create*` factories that resolve a stateful `IHasher` for chunked incremental hashing; `createXXHash128` produces the 128-bit content-key digest used as the content-addressed frame key, replacing the prior `xxhash-wasm` rail so one wasm owns the 32/64/128-bit surface.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `hash-wasm`
- package: `hash-wasm`
- module: `hash-wasm` (ESM via `./dist/index.esm.js`; UMD via `./dist/index.umd.js`; type barrel at `./dist/lib/index.d.ts`)
- asset: WASM binaries base64-embedded in JS modules; no external `.wasm` fetch
- rail: content-hash

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: hasher and input types
- rail: content-hash

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]     | [RAIL]                                               |
| :-----: | :------------ | :---------------- | :--------------------------------------------------- |
|   [1]   | `IHasher`     | stateful hasher   | incremental session with save/load and size metadata |
|   [2]   | `IDataType`   | input union       | `string \| Buffer \| ITypedArray`                    |
|   [3]   | `ITypedArray` | typed-array union | `Uint8Array \| Uint16Array \| Uint32Array`           |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: xxHash 128-bit content key
- rail: content-hash

| [INDEX] | [SURFACE]                                                                          | [ENTRY_FAMILY]   | [RAIL]                             |
| :-----: | :--------------------------------------------------------------------------------- | :--------------- | :--------------------------------- |
|   [1]   | `createXXHash128(seedLow?: number, seedHigh?: number): Promise<IHasher>`           | 128-bit factory  | resolves a stateful 128-bit hasher |
|   [2]   | `xxhash128(data: IDataType, seedLow?: number, seedHigh?: number): Promise<string>` | 128-bit one-shot | data to 32-char hexadecimal string |

[ENTRYPOINT_SCOPE]: xxHash family factories
- rail: content-hash

| [INDEX] | [SURFACE]                                                               | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :---------------------------------------------------------------------- | :------------- | :-------------------------------- |
|   [1]   | `createXXHash32(seed?: number): Promise<IHasher>`                       | 32-bit factory | resolves a stateful 32-bit hasher |
|   [2]   | `createXXHash64(seedLow?: number, seedHigh?: number): Promise<IHasher>` | 64-bit factory | resolves a stateful 64-bit hasher |
|   [3]   | `createXXHash3(seedLow?: number, seedHigh?: number): Promise<IHasher>`  | XXH3 factory   | resolves a stateful XXH3 hasher   |

[ENTRYPOINT_SCOPE]: IHasher instance operations
- rail: content-hash

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY] | [RAIL]                                         |
| :-----: | :----------------------------------------- | :------------- | :--------------------------------------------- |
|   [1]   | `init(): IHasher`                          | reset          | reinitializes state to default, returns `this` |
|   [2]   | `update(data: IDataType): IHasher`         | chain method   | feeds a chunk, returns `this`                  |
|   [3]   | `digest(outputType?: "hex"): string`       | finalize       | 32-char hexadecimal digest                     |
|   [4]   | `digest(outputType: "binary"): Uint8Array` | finalize       | 16-byte `Uint8Array` digest                    |
|   [5]   | `save(): Uint8Array`                       | snapshot       | serializes internal state for later `load`     |
|   [6]   | `load(state: Uint8Array): IHasher`         | restore        | resumes a `save()` state, returns `this`       |
|   [7]   | `blockSize`                                | property       | block size in bytes (`512` for XXHash128)      |
|   [8]   | `digestSize`                               | property       | digest size in bytes (`16` for XXHash128)      |

## [4]-[IMPLEMENTATION_LAW]

[HASH_TOPOLOGY]:
- every entry is async; `create*` factories and one-shot functions both return a `Promise`, and the `IHasher` handle is unavailable until that `Promise` resolves
- `createXXHash128(seedLow, seedHigh)` seeds the 128-bit state from two 32-bit halves; both default to `0`
- `digest()` defaults to a 32-character hexadecimal string; `digest("binary")` returns a 16-byte `Uint8Array` (`digestSize`)
- `init`, `update`, and `load` return the same `IHasher`, supporting chained calls; `digest` is terminal for the current cycle and `init` reopens it
- `save` cannot be called before `init` or after `digest`; the serialized state may embed plaintext bytes of the hashed value and is as sensitive as the input
- `load` throws if the state was not produced by a compatible `hash-wasm` build

[ENDIANNESS_LAW]:
- `createXXHash128` emits its digest in little-endian byte order, while the C# `System.IO.Hashing.XxHash128` persists big-endian; the frame-reassembly harness normalizes byte order at the boundary so both sides agree on the canonical content key
- compare normalized bytes, never the raw hex strings, when checking cross-runtime content-key parity

[LOCAL_ADMISSION]:
- Resolve `createXXHash128` once at the composition root; pass the `IHasher` through the Effect service layer rather than re-resolving per call.
- Use `update` over chunked `Uint8Array` payloads for streamed frames; `save`/`load` checkpoint a partial digest across transferable worker boundaries.
- `IDataType` accepts `string`, `Buffer`, and `Uint8Array`/`Uint16Array`/`Uint32Array`; feed raw frame bytes without a string round-trip.

[RAIL_LAW]:
- Package: `hash-wasm`
- Owns: xxHash 32/64/XXH3/128-bit digests plus CRC and cryptographic hashes over strings and byte buffers
- Accept: `string`, `Buffer`, or `ITypedArray` input; `number` seed halves
- Reject: hand-rolled xxHash; raw-hex cross-runtime parity checks that skip endianness normalization; synchronous WASM evaluation before the factory `Promise` resolves
