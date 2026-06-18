# [RASM_PERSISTENCE_API_FASTCDC]

`FastCDC.Net` supplies content-defined chunking over a `byte[]` source via the FastCDC
algorithm, producing an `IEnumerable<Chunk>` of offset-and-length descriptors for
deduplication-aware chunking in Persistence snapshot and object-store profiles.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `FastCDC.Net`
- package: `FastCDC.Net`
- assembly: `FastCdc.Net`
- namespace: `FastCdc.Net`
- asset: runtime library
- rail: chunking

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: CDC family
- rail: chunking

| [INDEX] | [SYMBOL]  | [TYPE_FAMILY] | [CAPABILITY]                        |
| :-----: | :-------- | :------------ | :---------------------------------- |
|   [1]   | `FastCdc` | chunker root  | content-defined chunking over bytes |
|   [2]   | `Chunk`   | chunk value   | hash, offset, and length descriptor |

`FastCdc` exposes these public constants: `MinimumMin = 64`, `MinimumMax = 67108864`,
`AverageMin = 256`, `AverageMax = 268435456`, `MaximumMin = 1024`, `MaximumMax = 1073741824`.

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and chunking
- rail: chunking

| [INDEX] | [SURFACE]                                             | [ENTRY_FAMILY] | [CAPABILITY]                                         |
| :-----: | :---------------------------------------------------- | :------------- | :--------------------------------------------------- |
|   [1]   | `new FastCdc(source, minSize, avgSize, maxSize)`      | ctor           | creates chunker; `eof = true` by default             |
|   [2]   | `new FastCdc(source, minSize, avgSize, maxSize, eof)` | ctor           | creates chunker with explicit EOF boundary           |
|   [3]   | `GetChunks()`                                         | enumeration    | returns lazy `IEnumerable<Chunk>` of cut descriptors |

## [4]-[IMPLEMENTATION_LAW]

[CDC_TOPOLOGY]:
- one public sealed class `FastCdc` and one result type `Chunk`
- `Chunk` carries `Hash` (uint), `Offset` (uint), and `Length` (uint)
- size bounds: `minSize` in `[64, 67108864]`, `avgSize` in `[256, 268435456]`, `maxSize` in `[1024, 1073741824]`
- constraint: `minSize <= avgSize <= maxSize`; violations throw `ArgumentOutOfRangeException` or `ArgumentException`
- `eof = true` (default): remaining bytes smaller than `minSize` are emitted as a final chunk; `eof = false`: they are withheld
- internal gear masks `_maskS` and `_maskL` are derived from `Logarithm2(avgSize)` ± 1
- `GetChunks()` is a lazy iterator; the source `byte[]` is held by the chunker for its lifetime

[LOCAL_ADMISSION]:
- Callers pass the full segment as a `byte[]`; chunker does not own streaming or partial-fill of the buffer.
- Chunk identity for deduplication is the `Hash` field from `Chunk`; canonical storage key derives from it.
- Size policy (min/avg/max) is a profile constant, not a per-call concern.

[RAIL_LAW]:
- Package: `FastCDC.Net`
- Owns: content-defined chunk boundary detection
- Accept: `byte[]` source with configured size policy
- Reject: hand-rolled gear-shift CDC, streaming chunkers outside the `byte[]` model
