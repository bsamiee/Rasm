# [RASM_APPUI_API_HASHING]

`System.IO.Hashing` supplies non-cryptographic hashing algorithms for snapshot
identity, cache keys, receipt fingerprints, benchmark indexes, and support
bundle correlation.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.IO.Hashing`
- package: `System.IO.Hashing`
- assembly: `System.IO.Hashing`
- namespace: `System.IO.Hashing`
- asset: runtime library
- rail: snapshot-identity

## [2]-[PUBLIC_TYPES]

[HASH_TYPES]: hashing surfaces
- rail: snapshot-identity

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE]     | [CAPABILITY]             |
| :-----: | :------------------------------ | :----------------- | :----------------------- |
|   [1]   | `NonCryptographicHashAlgorithm` | algorithm base     | defines hash lifecycle   |
|   [2]   | `XxHash32`                      | hash algorithm     | computes 32-bit hash     |
|   [3]   | `XxHash64`                      | hash algorithm     | computes 64-bit hash     |
|   [4]   | `XxHash3`                       | hash algorithm     | computes 64-bit hash     |
|   [5]   | `XxHash128`                     | hash algorithm     | computes 128-bit hash    |
|   [6]   | `Crc32`                         | checksum algorithm | computes 32-bit checksum |
|   [7]   | `Crc64`                         | checksum algorithm | computes 64-bit checksum |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: hash operations
- rail: snapshot-identity

| [INDEX] | [SURFACE]         | [CALL_SHAPE]  | [CAPABILITY]           |
| :-----: | :---------------- | :------------ | :--------------------- |
|   [1]   | `Hash`            | static call   | computes hash bytes    |
|   [2]   | `HashToUInt32`    | static call   | computes 32-bit value  |
|   [3]   | `HashToUInt64`    | static call   | computes 64-bit value  |
|   [4]   | `HashToUInt128`   | static call   | computes 128-bit value |
|   [5]   | `Append`          | instance call | appends payload bytes  |
|   [6]   | `GetCurrentHash`  | instance call | reads current hash     |
|   [7]   | `GetHashAndReset` | instance call | finalizes and resets   |
|   [8]   | `Reset`           | instance call | resets hash state      |

## [4]-[IMPLEMENTATION_LAW]

[IDENTITY_PROFILE]:
- namespace: `System.IO.Hashing`
- base root: `NonCryptographicHashAlgorithm`
- fast root: XxHash algorithms
- checksum root: CRC algorithms
- receipt root: hash algorithm, input class, and output width

[LOCAL_ADMISSION]:
- Hashing creates non-cryptographic identity, cache, and correlation values only.
- Redaction, security, and tamper evidence use separate declared rails.
- Hash algorithm, output width, and input domain are receipt facts.
- Snapshot identity cannot hide codec, compression, schema, or retention policy.

[RAIL_LAW]:
- Package: `System.IO.Hashing`
- Owns: non-cryptographic snapshot identity
- Accept: cache and receipt fingerprints
- Reject: security claims from non-cryptographic hashes
