# [RASM_PERSISTENCE_API_HASHING]

`System.IO.Hashing` supplies non-cryptographic hash algorithms for snapshot identity, cache keys, and receipt checksums.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.IO.Hashing`
- package: `System.IO.Hashing`
- assembly: `System.IO.Hashing`
- namespace: `System.IO.Hashing`
- asset: runtime library
- rail: hashing

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: hash family
- rail: hashing

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE] | [CAPABILITY]             |
| :-----: | :------------------------------ | :------------- | :----------------------- |
|   [1]   | `XxHash3`                       | hash algorithm | computes receipt hash    |
|   [2]   | `XxHash64`                      | hash algorithm | computes receipt hash    |
|   [3]   | `XxHash32`                      | hash algorithm | computes receipt hash    |
|   [4]   | `Crc32`                         | hash algorithm | computes receipt hash    |
|   [5]   | `Crc64`                         | hash algorithm | computes receipt hash    |
|   [6]   | `NonCryptographicHashAlgorithm` | hash base      | anchors hashing contract |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: hash operations
- rail: hashing

| [INDEX] | [SURFACE]         | [CALL_SHAPE]      | [CAPABILITY]           |
| :-----: | :---------------- | :---------------- | :--------------------- |
|   [1]   | `Append`          | incremental input | updates hash state     |
|   [2]   | `AppendAsync`     | async input       | updates hash state     |
|   [3]   | `GetCurrentHash`  | hash read         | reads hash bytes       |
|   [4]   | `GetHashAndReset` | hash read         | reads and resets state |
|   [5]   | `Reset`           | state reset       | resets hash state      |
|   [6]   | `Hash`            | static hash       | computes hash bytes    |

[WIDTH_SPECIFIC_ENTRYPOINTS]:
- rail: hashing

| [INDEX] | [SURFACE]                | [CALL_SHAPE] | [CAPABILITY]         |
| :-----: | :----------------------- | :----------- | :------------------- |
|   [1]   | `GetCurrentHashAsUInt32` | hash read    | reads 32-bit hash    |
|   [2]   | `HashToUInt32`           | static hash  | computes 32-bit hash |
|   [3]   | `GetCurrentHashAsUInt64` | hash read    | reads 64-bit hash    |
|   [4]   | `HashToUInt64`           | static hash  | computes 64-bit hash |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `System.IO.Hashing`
- Owns: non-cryptographic identity hashes
- Accept: hash receipts record algorithm and value
- Reject: custom checksum code
