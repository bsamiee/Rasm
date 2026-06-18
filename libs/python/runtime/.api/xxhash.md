# [PY_RUNTIME_API_XXHASH]

`xxhash` supplies fast non-cryptographic hashing via four algorithm families (`xxh32`, `xxh64`, `xxh3_64`, `xxh128`/`xxh3_128`), each available as a stateful hasher class and three one-shot digest functions, for use as content-identity keys, cache keys, and integrity tokens across runtime owners.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `xxhash`
- package: `xxhash`
- import: `xxhash`
- owner: `runtime`
- rail: hashing
- namespaces: `xxhash`
- capability: fast non-cryptographic hashing via `xxh32`/`xxh64`/`xxh3_64`/`xxh3_128` stateful hashers and one-shot digest functions

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: hasher classes
- rail: hashing

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY] | [RAIL]                          |
| :-----: | :--------- | :------------ | :------------------------------ |
|   [1]   | `xxh32`    | hasher class  | 32-bit xxHash                   |
|   [2]   | `xxh64`    | hasher class  | 64-bit xxHash                   |
|   [3]   | `xxh3_64`  | hasher class  | 64-bit XXH3 (streaming)         |
|   [4]   | `xxh3_128` | hasher class  | 128-bit XXH3 (streaming)        |
|   [5]   | `xxh128`   | hasher class  | 128-bit xxHash (alias xxh3_128) |

[PUBLIC_TYPE_SCOPE]: constants and metadata
- rail: hashing

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [RAIL]                              |
| :-----: | :--------------------- | :------------ | :---------------------------------- |
|   [1]   | `VERSION`              | `str`         | python-xxhash release string        |
|   [2]   | `VERSION_TUPLE`        | `tuple`       | python-xxhash release tuple         |
|   [3]   | `XXHASH_VERSION`       | `str`         | underlying xxHash C library version |
|   [4]   | `algorithms_available` | `set`         | set of algorithm name strings       |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: hasher instance operations
- rail: hashing

| [INDEX] | [SURFACE]      | [ENTRY_FAMILY]  | [RAIL]                        |
| :-----: | :------------- | :-------------- | :---------------------------- |
|   [1]   | `update(data)` | stateful update | feed bytes into running state |
|   [2]   | `digest()`     | state read      | current digest as `bytes`     |
|   [3]   | `hexdigest()`  | state read      | current digest as hex `str`   |
|   [4]   | `intdigest()`  | state read      | current digest as `int`       |
|   [5]   | `copy()`       | state snapshot  | clone hasher at current state |
|   [6]   | `reset()`      | state reset     | reset to initial seed state   |
|   [7]   | `seed`         | property        | integer seed supplied at init |
|   [8]   | `name`         | property        | algorithm name string         |
|   [9]   | `digest_size`  | property        | digest length in bytes        |
|  [10]   | `block_size`   | property        | internal block size in bytes  |

[ENTRYPOINT_SCOPE]: one-shot digest functions
- rail: hashing
- Applies to all four algorithm families; pattern shown for `xxh32`, identical for `xxh64`, `xxh3_64`, `xxh3_128`, `xxh128`.

| [INDEX] | [SURFACE]                  | [ENTRY_FAMILY] | [RAIL]                   |
| :-----: | :------------------------- | :------------- | :----------------------- |
|   [1]   | `xxh32_digest(data)`       | one-shot       | `bytes` digest, 32-bit   |
|   [2]   | `xxh32_hexdigest(data)`    | one-shot       | hex `str` digest, 32-bit |
|   [3]   | `xxh32_intdigest(data)`    | one-shot       | `int` digest, 32-bit     |
|   [4]   | `xxh64_digest(data)`       | one-shot       | `bytes` digest, 64-bit   |
|   [5]   | `xxh64_hexdigest(data)`    | one-shot       | hex `str` digest, 64-bit |
|   [6]   | `xxh64_intdigest(data)`    | one-shot       | `int` digest, 64-bit     |
|   [7]   | `xxh3_64_digest(data)`     | one-shot       | `bytes` digest, XXH3-64  |
|   [8]   | `xxh3_64_hexdigest(data)`  | one-shot       | hex `str`, XXH3-64       |
|   [9]   | `xxh3_64_intdigest(data)`  | one-shot       | `int`, XXH3-64           |
|  [10]   | `xxh3_128_digest(data)`    | one-shot       | `bytes` digest, XXH3-128 |
|  [11]   | `xxh3_128_hexdigest(data)` | one-shot       | hex `str`, XXH3-128      |
|  [12]   | `xxh3_128_intdigest(data)` | one-shot       | `int`, XXH3-128          |
|  [13]   | `xxh128_digest(data)`      | one-shot       | `bytes` digest, xxh128   |
|  [14]   | `xxh128_hexdigest(data)`   | one-shot       | hex `str`, xxh128        |
|  [15]   | `xxh128_intdigest(data)`   | one-shot       | `int`, xxh128            |

## [4]-[IMPLEMENTATION_LAW]

[HASH_TOPOLOGY]:
- four algorithm families: `xxh32` (32-bit), `xxh64` (64-bit), `xxh3_64` (64-bit XXH3), `xxh3_128`/`xxh128` (128-bit XXH3)
- each family: one stateful hasher class plus three one-shot functions (`_digest`, `_hexdigest`, `_intdigest`)
- hasher constructor accepts optional `input` bytes and `seed` integer; seed is 0 by default
- `algorithms_available`: a `set` of algorithm name strings matching the `name` property values
- `xxh128` and `xxh3_128` are the same algorithm; `xxh128` is the canonical alias

[LOCAL_ADMISSION]:
- one-shot functions are the fast path for single-buffer content keys; stateful hashers are used for incremental streaming
- content-identity keys and cache keys use `intdigest()` or `hexdigest()` output; raw `bytes` from `digest()` is for binary framing
- seed is fixed per owner identity; seeded hashes are not mixed with unseeded values in the same key namespace

[RAIL_LAW]:
- Package: `xxhash`
- Owns: fast non-cryptographic hashing for content identity, cache keys, and integrity tokens
- Accept: one-shot digest functions for single-buffer hashing; stateful `update`/`digest` cycle for streaming
- Reject: use of xxhash for cryptographic security, password storage, or HMAC; hand-rolled byte-folding hash replacements
