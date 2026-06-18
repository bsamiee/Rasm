# [API_CATALOGUE] xxhash-wasm

`xxhash-wasm` supplies a WebAssembly implementation of xxHash, exposing 32-bit and 64-bit one-shot hash functions, raw-buffer variants, and streaming incremental hashers for the interchange content-key rail; `h32` and `h64` produce the whole-artifact digest used as the content-addressed frame key.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `xxhash-wasm`
- package: `xxhash-wasm`
- module: `xxhash-wasm` (ESM default export via `./esm/xxhash-wasm.js`; CJS via `./cjs/xxhash-wasm.cjs`)
- asset: WASM binary bundled in JS modules; type declarations at `./types.d.ts`
- rail: content-hash

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: API types
- rail: content-hash

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [RAIL]                              |
| :-----: | :---------- | :------------ | :---------------------------------- |
|   [1]   | `XXHashAPI` | interface     | resolved API handle from `xxhash()` |
|   [2]   | `XXHash<T>` | interface     | streaming incremental hasher        |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: module default
- rail: content-hash

| [INDEX] | [SURFACE]                      | [ENTRY_FAMILY] | [RAIL]                       |
| :-----: | :----------------------------- | :------------- | :--------------------------- |
|   [1]   | `xxhash(): Promise<XXHashAPI>` | async init     | resolves once WASM is loaded |

[ENTRYPOINT_SCOPE]: XXHashAPI — one-shot operations
- rail: content-hash

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY] | [RAIL]               |
| :-----: | :------------------------------------------------------- | :------------- | :------------------- |
|   [1]   | `h32(input: string, seed?: number): number`              | 32-bit hash    | string to `number`   |
|   [2]   | `h32ToString(input: string, seed?: number): string`      | 32-bit hash    | string to hex string |
|   [3]   | `h32Raw(inputBuffer: Uint8Array, seed?: number): number` | 32-bit hash    | buffer to `number`   |
|   [4]   | `h64(input: string, seed?: bigint): bigint`              | 64-bit hash    | string to `bigint`   |
|   [5]   | `h64ToString(input: string, seed?: bigint): string`      | 64-bit hash    | string to hex string |
|   [6]   | `h64Raw(inputBuffer: Uint8Array, seed?: bigint): bigint` | 64-bit hash    | buffer to `bigint`   |

[ENTRYPOINT_SCOPE]: XXHashAPI — streaming incremental hashers
- rail: content-hash

| [INDEX] | [SURFACE]                                 | [ENTRY_FAMILY] | [RAIL]                     |
| :-----: | :---------------------------------------- | :------------- | :------------------------- |
|   [1]   | `create32(seed?: number): XXHash<number>` | 32-bit builder | incremental 32-bit session |
|   [2]   | `create64(seed?: bigint): XXHash<bigint>` | 64-bit builder | incremental 64-bit session |

[ENTRYPOINT_SCOPE]: XXHash<T> instance operations
- rail: content-hash

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY] | [RAIL]                    |
| :-----: | :----------------------------------------------- | :------------- | :------------------------ |
|   [1]   | `update(input: string \| Uint8Array): XXHash<T>` | chain method   | feed data, returns `this` |
|   [2]   | `digest(): T`                                    | finalize       | produce hash result       |

## [4]-[IMPLEMENTATION_LAW]

[HASH_TOPOLOGY]:
- default export is an async factory; the `XXHashAPI` handle is unavailable until the returned `Promise` resolves
- `h32` variants return `number`; `h64` variants return `bigint`; `h32Raw`/`h64Raw` accept `Uint8Array` without string encoding overhead
- incremental `create32`/`create64` sessions support chained `update` calls; `digest()` finalizes and is terminal
- ESM entry: `./esm/xxhash-wasm.js`; CJS entry: `./cjs/xxhash-wasm.cjs`; Cloudflare Workers entry: `./workerd/xxhash-wasm.js`
- `seed` parameters are optional; `h32` seed defaults to `0` (`number`), `h64` seed defaults to `0n` (`bigint`)

[LOCAL_ADMISSION]:
- Initialize once at composition root; pass the resolved `XXHashAPI` handle through the Effect service layer.
- Use `h64Raw` or `h64` for the interchange content-key digest; `h32` is a secondary fast path for smaller keys.
- The `XXHash<T>` streaming interface feeds chunked `Uint8Array` payloads without full-buffer allocation.

[RAIL_LAW]:
- Package: `xxhash-wasm`
- Owns: 32-bit and 64-bit xxHash digests over strings and byte buffers
- Accept: string and `Uint8Array` inputs; `number` or `bigint` seeds
- Reject: hand-rolled hash functions; synchronous `require`-time WASM evaluation before the factory resolves
