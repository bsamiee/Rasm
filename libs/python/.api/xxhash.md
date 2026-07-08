# [PY_BRANCH_API_XXHASH]

`xxhash` (release, BSD-2-Clause) binds the xxHash C library (`XXHASH_VERSION` release) for fast non-cryptographic hashing across four distinct algorithm families — classic `xxh32`/`xxh64` and the modern `xxh3_64`/`xxh3_128` — each exposed as a stateful seedable hasher class plus three one-shot digest functions (`_digest`/`_hexdigest`/`_intdigest`). It is the runtime owner for content-identity keys, cache keys, and integrity tokens, mirroring the C# `System.IO.Hashing` XXH digests at the wire.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `xxhash`
- package: `xxhash`
- version: `3.7.1`
- license: BSD-2-Clause
- import: `xxhash`
- owner: `runtime`
- rail: hashing
- namespaces: `xxhash`, `xxhash.version`
- capability: fast non-cryptographic hashing via four families — `xxh32` (32-bit), `xxh64` (classic 64-bit), `xxh3_64` (XXH3 64-bit), `xxh3_128`/`xxh128` (XXH3 128-bit) — each a stateful hasher plus one-shot functions

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: hasher classes
- rail: hashing
- All four are `@final` and share the `_Hasher` interface: constructor `(input: str | Buffer = b"", seed: int = 0)`, the `update`/`digest`/`hexdigest`/`intdigest`/`copy`/`reset` operations, and the `seed`/`name`/`digest_size`/`digestsize`/`block_size` properties. `xxh64`/`xxh3_64` are genuinely distinct at runtime — same 8-byte digest size but different hash values (`name` `XXH64` vs `XXH3_64`); only `xxh128` is a true alias of `xxh3_128` (identical class object). The `.pyi` stub aliases `xxh64 = xxh3_64`, but the compiled module keeps them separate — runtime is authoritative.

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY] | [RAIL]                                                              |
| ------- | ---------- | ------------- | ------------------------------------------------------------------- |
| [01]    | `xxh32`    | hasher class  | classic 32-bit xxHash; `name` `XXH32`, 4-byte digest                |
| [02]    | `xxh64`    | hasher class  | classic 64-bit xxHash; `name` `XXH64`, 8-byte digest                |
| [03]    | `xxh3_64`  | hasher class  | XXH3 64-bit; `name` `XXH3_64`, 8-byte digest, faster on modern CPUs |
| [04]    | `xxh3_128` | hasher class  | XXH3 128-bit; `name` `XXH3_128`, 16-byte digest                     |
| [05]    | `xxh128`   | hasher class  | true alias of `xxh3_128` (same class object)                        |

[PUBLIC_TYPE_SCOPE]: constants and metadata
- rail: hashing

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]    | [RAIL]                                                         |
| ------- | ---------------------- | ---------------- | -------------------------------------------------------------- |
| [01]    | `VERSION`              | `str`            | python-xxhash release string (`3.7.0`)                         |
| [02]    | `XXHASH_VERSION`       | `str`            | bundled xxHash C library version (`0.8.2`)                     |
| [03]    | `algorithms_available` | `set[str]`       | `{'xxh32','xxh64','xxh3_64','xxh3_128','xxh128'}`              |
| [04]    | `VERSION_TUPLE`        | `tuple[int,...]` | deprecated, removed next major; use `VERSION`/`XXHASH_VERSION` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: hasher instance operations
- rail: hashing
- The stateful cycle for incremental/streaming input: construct with optional `input`/`seed`, feed with `update`, read the running state, `copy` to fork a partial digest, `reset` to return to the seed state. Both the constructor and `update` accept `_InputType = str | Buffer` — any buffer-protocol object (`bytes`, `bytearray`, `memoryview`, `array`) or a `str` (UTF-8 encoded by the binding); the owner still pins one input form (raw `bytes`) so the digest is reproducible across producers.

| [INDEX] | [SURFACE]                    | [ENTRY_FAMILY]  | [RAIL]                                                |
| ------- | ---------------------------- | --------------- | ----------------------------------------------------- |
| [01]    | `xxh32(input=b"", seed=0)`   | build           | construct a seeded hasher (any family)                |
| [02]    | `update(input)`              | stateful update | feed a buffer into running state                      |
| [03]    | `digest() -> bytes`          | state read      | current digest as big-endian `bytes`                  |
| [04]    | `hexdigest() -> str`         | state read      | current digest as hex `str`                           |
| [05]    | `intdigest() -> int`         | state read      | current digest as unsigned `int`                      |
| [06]    | `copy() -> _Hasher`          | state snapshot  | clone hasher at current state                         |
| [07]    | `reset()`                    | state reset     | reset to initial seed state                           |
| [08]    | `seed`                       | property        | integer seed supplied at init                         |
| [09]    | `name`                       | property        | algorithm name (`XXH32`/`XXH64`/`XXH3_64`/`XXH3_128`) |
| [10]    | `digest_size` / `digestsize` | property        | digest length in bytes (4/8/8/16)                     |
| [11]    | `block_size`                 | property        | internal block size in bytes                          |

[ENTRYPOINT_SCOPE]: one-shot digest functions
- rail: hashing
- The fast single-buffer path: `<family>_<form>(data, seed=0)` where `data` is a buffer-protocol object and `seed` is an optional keyword. `_digest` returns `bytes`, `_hexdigest` returns hex `str`, `_intdigest` returns `int`. `xxh128_*` are true aliases of `xxh3_128_*`; `xxh64_*` are distinct from `xxh3_64_*` (classic vs XXH3).

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY] | [RAIL]                                   |
| ------- | ---------------------------------- | -------------- | ---------------------------------------- |
| [01]    | `xxh32_digest(data, seed=0)`       | one-shot       | `bytes`, 32-bit                          |
| [02]    | `xxh32_hexdigest(data, seed=0)`    | one-shot       | hex `str`, 32-bit                        |
| [03]    | `xxh32_intdigest(data, seed=0)`    | one-shot       | `int`, 32-bit                            |
| [04]    | `xxh64_digest(data, seed=0)`       | one-shot       | `bytes`, classic 64-bit                  |
| [05]    | `xxh64_hexdigest(data, seed=0)`    | one-shot       | hex `str`, classic 64-bit                |
| [06]    | `xxh64_intdigest(data, seed=0)`    | one-shot       | `int`, classic 64-bit                    |
| [07]    | `xxh3_64_digest(data, seed=0)`     | one-shot       | `bytes`, XXH3-64                         |
| [08]    | `xxh3_64_hexdigest(data, seed=0)`  | one-shot       | hex `str`, XXH3-64                       |
| [09]    | `xxh3_64_intdigest(data, seed=0)`  | one-shot       | `int`, XXH3-64                           |
| [10]    | `xxh3_128_digest(data, seed=0)`    | one-shot       | `bytes`, XXH3-128                        |
| [11]    | `xxh3_128_hexdigest(data, seed=0)` | one-shot       | hex `str`, XXH3-128                      |
| [12]    | `xxh3_128_intdigest(data, seed=0)` | one-shot       | `int`, XXH3-128                          |
| [13]    | `xxh128_digest(data, seed=0)`      | one-shot       | `bytes`, alias of `xxh3_128_digest`      |
| [14]    | `xxh128_hexdigest(data, seed=0)`   | one-shot       | hex `str`, alias of `xxh3_128_hexdigest` |
| [15]    | `xxh128_intdigest(data, seed=0)`   | one-shot       | `int`, alias of `xxh3_128_intdigest`     |

## [04]-[IMPLEMENTATION_LAW]

[HASH_TOPOLOGY]:
- four families: `xxh32` (32-bit), `xxh64` (classic 64-bit), `xxh3_64` (XXH3 64-bit), `xxh3_128`/`xxh128` (XXH3 128-bit); each has one stateful hasher plus three one-shot functions.
- the constructor and `update` accept optional `input: str | Buffer` and the constructor an integer `seed` (default 0); one-shot functions accept the same `seed=` keyword.
- `xxh64` and `xxh3_64` are not interchangeable — the same content hashes to different values; the family choice is fixed per owner identity and never mixed within one key namespace.
- modern XXH3 families (`xxh3_64`/`xxh3_128`) are the default choice for new keys; classic `xxh32`/`xxh64` exist for cross-language compatibility with fixed peer formats.

[LOCAL_ADMISSION]:
- one-shot functions are the fast path for single-buffer content keys; stateful hashers serve incremental streaming over a chunked source (`update` per chunk, one `intdigest()` at end).
- content-identity keys and cache keys use `intdigest()` or `hexdigest()`; raw `bytes` from `digest()` is for binary framing and must declare endianness with the consumer.
- the seed is fixed per owner identity; seeded and unseeded hashes are never mixed in one key namespace.
- `XXH3_128` is the content-identity digest that matches the C# `System.IO.Hashing.XxHash128` boundary; the owner that crosses the .NET wire pins `xxh3_128`/`xxh128` and a fixed seed so the Python and C# digests agree byte-for-byte. `digest()` returns big-endian `bytes`, so the .NET-bound owner declares the same byte order on the C# side rather than re-ordering.
- the one-shot `<family>_intdigest(data)` is the content key feeding the keyed-cache and content-identity owners; it composes directly with a `msgspec`-encoded payload (`.api/msgspec.md`) — hash the canonical encoded `bytes` (one `msgspec.msgpack.encode` -> one `xxh3_128_intdigest`) so the identity is a deterministic function of the wire form, never of in-memory object identity. A `UPath` artifact (`.api/universal-pathlib.md`) is keyed by hashing its read `bytes`, not its string path.

[RAIL_LAW]:
- Package: `xxhash`
- Owns: fast non-cryptographic hashing for content identity, cache keys, and integrity tokens across four xxHash families
- Accept: one-shot `<family>_<form>(data, seed=)` for single-buffer hashing; the stateful `update`/`digest` cycle for streaming; a fixed family+seed per owner identity
- Reject: xxhash for cryptographic security, password storage, or HMAC; mixing distinct families or seeds in one key namespace; a hand-rolled byte-folding hash; `VERSION_TUPLE` in new code
