# [PY_RUNTIME_API_XXHASH]

`xxhash` supplies fast non-cryptographic hashing via four algorithm families (`xxh32`, `xxh64`, `xxh3_64`, `xxh128`/`xxh3_128`), each available as a stateful hasher class and three one-shot digest functions, for use as content-identity keys, cache keys, and integrity tokens across runtime owners.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `xxhash`
- package: `xxhash`
- import: `xxhash`
- owner: `runtime`
- rail: hashing
- namespaces: `xxhash`
- capability: fast non-cryptographic hashing via `xxh32`/`xxh64`/`xxh3_64`/`xxh3_128` stateful hashers and one-shot digest functions

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: hasher classes
- rail: hashing

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY] | [RAIL]                          |
| :-----: | :--------- | :------------ | :------------------------------ |
|  [01]   | `xxh32`    | hasher class  | 32-bit xxHash                   |
|  [02]   | `xxh64`    | hasher class  | 64-bit xxHash                   |
|  [03]   | `xxh3_64`  | hasher class  | 64-bit XXH3 (streaming)         |
|  [04]   | `xxh3_128` | hasher class  | 128-bit XXH3 (streaming)        |
|  [05]   | `xxh128`   | hasher class  | 128-bit xxHash (alias xxh3_128) |

[PUBLIC_TYPE_SCOPE]: constants and metadata
- rail: hashing

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [RAIL]                              |
| :-----: | :--------------------- | :------------ | :---------------------------------- |
|  [01]   | `VERSION`              | `str`         | python-xxhash release string        |
|  [02]   | `VERSION_TUPLE`        | `tuple`       | python-xxhash release tuple         |
|  [03]   | `XXHASH_VERSION`       | `str`         | underlying xxHash C library version |
|  [04]   | `algorithms_available` | `set`         | set of algorithm name strings       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: hasher instance operations
- rail: hashing

| [INDEX] | [SURFACE]      | [ENTRY_FAMILY]  | [RAIL]                        |
| :-----: | :------------- | :-------------- | :---------------------------- |
|  [01]   | `update(data)` | stateful update | feed bytes into running state |
|  [02]   | `digest()`     | state read      | current digest as `bytes`     |
|  [03]   | `hexdigest()`  | state read      | current digest as hex `str`   |
|  [04]   | `intdigest()`  | state read      | current digest as `int`       |
|  [05]   | `copy()`       | state snapshot  | clone hasher at current state |
|  [06]   | `reset()`      | state reset     | reset to initial seed state   |
|  [07]   | `seed`         | property        | integer seed supplied at init |
|  [08]   | `name`         | property        | algorithm name string         |
|  [09]   | `digest_size`  | property        | digest length in bytes        |
|  [10]   | `block_size`   | property        | internal block size in bytes  |

[ENTRYPOINT_SCOPE]: one-shot digest functions
- rail: hashing
- Applies to all four algorithm families; pattern shown for `xxh32`, identical for `xxh64`, `xxh3_64`, `xxh3_128`, `xxh128`.

| [INDEX] | [SURFACE]                  | [ENTRY_FAMILY] | [RAIL]                   |
| :-----: | :------------------------- | :------------- | :----------------------- |
|  [01]   | `xxh32_digest(data)`       | one-shot       | `bytes` digest, 32-bit   |
|  [02]   | `xxh32_hexdigest(data)`    | one-shot       | hex `str` digest, 32-bit |
|  [03]   | `xxh32_intdigest(data)`    | one-shot       | `int` digest, 32-bit     |
|  [04]   | `xxh64_digest(data)`       | one-shot       | `bytes` digest, 64-bit   |
|  [05]   | `xxh64_hexdigest(data)`    | one-shot       | hex `str` digest, 64-bit |
|  [06]   | `xxh64_intdigest(data)`    | one-shot       | `int` digest, 64-bit     |
|  [07]   | `xxh3_64_digest(data)`     | one-shot       | `bytes` digest, XXH3-64  |
|  [08]   | `xxh3_64_hexdigest(data)`  | one-shot       | hex `str`, XXH3-64       |
|  [09]   | `xxh3_64_intdigest(data)`  | one-shot       | `int`, XXH3-64           |
|  [10]   | `xxh3_128_digest(data)`    | one-shot       | `bytes` digest, XXH3-128 |
|  [11]   | `xxh3_128_hexdigest(data)` | one-shot       | hex `str`, XXH3-128      |
|  [12]   | `xxh3_128_intdigest(data)` | one-shot       | `int`, XXH3-128          |
|  [13]   | `xxh128_digest(data)`      | one-shot       | `bytes` digest, xxh128   |
|  [14]   | `xxh128_hexdigest(data)`   | one-shot       | hex `str`, xxh128        |
|  [15]   | `xxh128_intdigest(data)`   | one-shot       | `int`, xxh128            |

## [04]-[IMPLEMENTATION_LAW]

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
