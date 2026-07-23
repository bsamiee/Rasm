# [PY_BRANCH_API_XXHASH]

`xxhash` binds the xxHash C library for fast non-cryptographic hashing across its classic and XXH3 families, each a stateful seedable hasher backed by a one-shot digest path. It owns content-identity keys, cache keys, and integrity tokens; its `XXH3_128` digest is the wire that agrees byte-for-byte with the C# `System.IO.Hashing` boundary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `xxhash`
- package: `xxhash` (`BSD-2-Clause`)
- module: `xxhash`
- namespaces: `xxhash`, `xxhash.version`
- rail: hashing

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: hasher classes
All four hasher classes share one `_Hasher` interface. `xxh128` is the same class object as `xxh3_128`, while `xxh64` and `xxh3_64` stay distinct despite an 8-byte digest each, so identical content hashes to different values across the two.

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :--------- | :------------ | :---------------------------------------------- |
|  [01]   | `xxh32`    | hasher class  | classic 32-bit; `name` `XXH32`, 4-byte digest   |
|  [02]   | `xxh64`    | hasher class  | classic 64-bit; `name` `XXH64`, 8-byte digest   |
|  [03]   | `xxh3_64`  | hasher class  | XXH3 64-bit; `name` `XXH3_64`, 8-byte digest    |
|  [04]   | `xxh3_128` | hasher class  | XXH3 128-bit; `name` `XXH3_128`, 16-byte digest |
|  [05]   | `xxh128`   | hasher class  | same class object as `xxh3_128`                 |

[PUBLIC_TYPE_SCOPE]: constants and metadata
- [BINDING_ID]: `VERSION` `XXHASH_VERSION` — release strings, mirrored in `xxhash.version`
- [PROBE]: `algorithms_available` — `set[str]` of available algorithm names

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: hasher instance operations
Construct with optional `input`/`seed`, feed with `update`, read the running state, `copy` to fork a partial digest, `reset` to the seed state. Constructor and `update` accept `str | Buffer` — any buffer-protocol object or a UTF-8-encoded `str`.

| [INDEX] | [SURFACE]                    | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :--------------------------- | :------- | :------------------------------------ |
|  [01]   | `xxh32(input=b"", seed=0)`   | ctor     | construct a seeded hasher, any family |
|  [02]   | `update(input)`              | instance | feed a buffer into running state      |
|  [03]   | `digest() -> bytes`          | instance | current digest as big-endian `bytes`  |
|  [04]   | `hexdigest() -> str`         | instance | current digest as hex `str`           |
|  [05]   | `intdigest() -> int`         | instance | current digest as unsigned `int`      |
|  [06]   | `copy() -> _Hasher`          | instance | clone the hasher at current state     |
|  [07]   | `reset()`                    | instance | reset to the initial seed state       |
|  [08]   | `seed`                       | property | integer seed supplied at init         |
|  [09]   | `name`                       | property | algorithm name                        |
|  [10]   | `digest_size` / `digestsize` | property | digest length in bytes                |
|  [11]   | `block_size`                 | property | internal block size in bytes          |

[ENTRYPOINT_SCOPE]: one-shot digest functions
Each family exposes `_digest -> bytes`, `_hexdigest -> hex str`, and `_intdigest -> int` one-shot functions over a buffer-protocol `input`, positional or keyword, with optional `seed=0` — argument handling matches the hasher constructors. `xxh128_*` aliases `xxh3_128_*`; `xxh64_*` stays distinct from `xxh3_64_*` (classic vs XXH3), hashing identical content to different values.

- [FAMILIES]: `xxh32` `xxh64` `xxh3_64` `xxh3_128` `xxh128`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `xxh64` and `xxh3_64` hash identical content to different values, so the family is fixed per owner identity and never mixed within one key namespace.
- XXH3 (`xxh3_64`/`xxh3_128`) is the default for new keys; classic `xxh32`/`xxh64` bind only where a fixed peer format demands cross-language compatibility.
- `digest()` returns big-endian `bytes`, equal to `intdigest()` read as `int.from_bytes(..., "big")`.

[STACKING]:
- `msgspec`(`.api/msgspec.md`): the content key hashes the canonical encoded buffer — one `msgspec.msgpack.encode` feeds one `xxh3_128_intdigest`, so identity is a deterministic function of the wire form, never in-memory object identity.
- `universal-pathlib`(`.api/universal-pathlib.md`): a `UPath` artifact keys on its read `bytes`, never its string path.
- runtime .NET boundary: `XXH3_128` matches C# `System.IO.Hashing.XxHash128`; the wire-crossing owner pins `xxh3_128`/`xxh128`, a fixed seed, and the big-endian `digest()` so the Python and C# digests agree byte-for-byte.

[LOCAL_ADMISSION]:
- one-shot `<family>_<form>(input)` is the single-buffer fast path; the stateful `update`/`digest` cycle serves incremental streaming over a chunked source.
- content-identity and cache keys read `intdigest()` or `hexdigest()`; raw `digest()` bytes serves binary framing and declares its endianness with the consumer.
- one seed per owner identity; seeded and unseeded hashes never mix in one key namespace.

[RAIL_LAW]:
- Package: `xxhash`
- Owns: fast non-cryptographic hashing for content identity, cache keys, and integrity tokens across the xxHash families
- Accept: one-shot `<family>_<form>(input, seed=)` for single-buffer hashing; the stateful `update`/`digest` cycle for streaming; a fixed family and seed per owner identity
- Reject: xxhash for cryptographic security, password storage, or HMAC; a hand-rolled byte-folding hash; mixing families or seeds in one key namespace
