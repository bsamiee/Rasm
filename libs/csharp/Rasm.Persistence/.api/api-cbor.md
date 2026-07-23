# [RASM_PERSISTENCE_API_CBOR]

`System.Formats.Cbor` is the first-party BCL codec for RFC 8949 CBOR: a hand-driven `CborWriter`/`CborReader` token pair over one `ReadOnlyMemory<byte>` buffer, carrying no schema, attributes, or reflection. `Canonical` conformance makes an encoding byte-deterministic — shortest-form integers, length-sorted map keys, definite lengths — so its `Encode()` bytes feed `XxHash128` for a stable `ContentKey`.

CBOR owns the self-describing, schema-free blob/snapshot leg, `MessagePack` the schemaless wire and Avro the schema-governed leg. Blob encoding runs under `Canonical`: the rail hashes the bytes and re-reads peer blobs under a depth-bounded `PeekState()` loop that refuses untrusted indefinite-length frames.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.Formats.Cbor`
- package: `System.Formats.Cbor` (MIT)
- assembly: `System.Formats.Cbor`
- namespace: `System.Formats.Cbor`
- asset: in-box BCL managed assembly (`lib/net10.0`), AnyCPU, zero transitive dependencies, no native payload
- rail: blob-codec

## [02]-[PUBLIC_TYPES]

[CODEC_TYPES]: writer and reader surfaces

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                      |
| :-----: | :--------------------- | :------------ | :------------------------------------------------ |
|  [01]   | `CborWriter`           | class         | push-model encoder over an internal grow buffer   |
|  [02]   | `CborReader`           | class         | pull-model decoder over a fixed `ReadOnlyMemory`  |
|  [03]   | `CborReaderState`      | enum          | `PeekState()` discriminant driving the read loop  |
|  [04]   | `CborContentException` | class         | thrown on structurally malformed CBOR during read |

[POLICY_TYPES]: conformance and tag vocabulary

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :-------------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `CborConformanceMode` | enum          | validation profile `Lax`/`Strict`/`Canonical`/`Ctap2Canonical` |
|  [02]   | `CborTag`             | enum          | RFC 8949 semantic-tag vocabulary, `ulong`-backed               |
|  [03]   | `CborSimpleValue`     | enum          | `byte`-backed `False=20`/`True`/`Null`/`Undefined`             |

`Canonical` emits shortest-form integers, length-sorted map keys, and definite-length headers; `Ctap2Canonical` adds the CTAP2 depth and type restrictions for FIDO2. `Strict` enforces well-formedness and rejects duplicate keys, and `Lax` validates nothing. `WriteSimpleValue(CborSimpleValue)` also writes the raw major-type-7 values 0-19 and 32-255 beyond the four named cases. Typed `WriteDateTimeOffset`/`WriteUnixTimeSeconds`/`WriteBigInteger`/`WriteDecimal` emit their matching `CborTag`; raw `WriteTag` precedes a hand-encoded tagged item.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: writer construction and framing
- ctor: `new CborWriter(conformanceMode, convertIndefiniteLengthEncodings, allowMultipleRootLevelValues, initialCapacity)`

| [INDEX] | [SURFACE]                                            | [SHAPE]  | [CAPABILITY]                                      |
| :-----: | :--------------------------------------------------- | :------- | :------------------------------------------------ |
|  [01]   | `new CborWriter(…)`                                  | ctor     | encoder with a fixed conformance profile          |
|  [02]   | `WriteStartArray(int?)` / `WriteEndArray()`          | instance | definite or indefinite (`null`) array frame       |
|  [03]   | `WriteStartMap(int?)` / `WriteEndMap()`              | instance | definite/indefinite map, canonical reorders keys  |
|  [04]   | `WriteStartIndefiniteLengthByteString()`             | instance | opens a chunked byte-string frame                 |
|  [05]   | `WriteEndIndefiniteLengthByteString()`               | instance | closes the chunked byte-string frame              |
|  [06]   | `WriteStartIndefiniteLengthTextString()`             | instance | opens a chunked text-string frame                 |
|  [07]   | `WriteEndIndefiniteLengthTextString()`               | instance | closes the chunked text-string frame              |
|  [08]   | `WriteTag(CborTag)`                                  | instance | prefixes the next data item with a tag            |
|  [09]   | `WriteEncodedValue(ReadOnlySpan<byte>)`              | instance | appends a pre-encoded CBOR item verbatim          |
|  [10]   | `Encode() -> byte[]`                                 | instance | materializes the buffer to a new array            |
|  [11]   | `Encode(Span<byte>)`                                 | instance | materializes into a caller span                   |
|  [12]   | `TryEncode(Span<byte>, out int)`                     | instance | allocation-free drain into a span                 |
|  [13]   | `Reset()`                                            | instance | clears state for buffer reuse without realloc     |
|  [14]   | `BytesWritten` / `CurrentDepth` / `IsWriteCompleted` | property | encoder cursor, nesting depth, root-complete flag |

[ENTRYPOINT_SCOPE]: writer scalar surface — instance `CborWriter` methods

| [INDEX] | [SURFACE]                                                          | [CAPABILITY]                                |
| :-----: | :----------------------------------------------------------------- | :------------------------------------------ |
|  [01]   | `WriteInt32` / `WriteInt64` / `WriteUInt32` / `WriteUInt64`        | major-type 0/1 signed and unsigned integers |
|  [02]   | `WriteCborNegativeIntegerRepresentation(ulong)`                    | raw major-type-1 full negative range        |
|  [03]   | `WriteBigInteger(BigInteger)`                                      | arbitrary-precision integer via tag 2/3     |
|  [04]   | `WriteDecimal(decimal)`                                            | `System.Decimal` via tag 4 decimal-fraction |
|  [05]   | `WriteHalf(Half)` / `WriteSingle(float)` / `WriteDouble(double)`   | RFC 8949 half/single/double precision       |
|  [06]   | `WriteBoolean` / `WriteNull` / `WriteSimpleValue(CborSimpleValue)` | major-type-7 simple values                  |
|  [07]   | `WriteByteString(byte[])` / `WriteByteString(ReadOnlySpan<byte>)`  | major-type-2 octet string                   |
|  [08]   | `WriteTextString(string)` / `WriteTextString(ReadOnlySpan<char>)`  | major-type-3 UTF-8 string                   |
|  [09]   | `WriteDateTimeOffset(DateTimeOffset)`                              | tag 0 RFC 3339 string                       |
|  [10]   | `WriteUnixTimeSeconds(long)` / `WriteUnixTimeSeconds(double)`      | tag 1 epoch seconds                         |

[ENTRYPOINT_SCOPE]: reader construction and navigation
- ctor: `new CborReader(ReadOnlyMemory<byte>, conformanceMode, allowMultipleRootLevelValues)`

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                                            |
| :-----: | :----------------------------------------------- | :------- | :------------------------------------------------------ |
|  [01]   | `new CborReader(…)`                              | ctor     | binds a decoder to an immutable buffer                  |
|  [02]   | `PeekState() -> CborReaderState`                 | instance | next-token kind without consuming — the read-loop pivot |
|  [03]   | `PeekTag() -> CborTag`                           | instance | reads the upcoming tag without consuming                |
|  [04]   | `ReadStartArray() -> int?` / `ReadEndArray()`    | instance | definite length or `null` for indefinite                |
|  [05]   | `ReadStartMap() -> int?` / `ReadEndMap()`        | instance | definite pair-count or `null` for indefinite            |
|  [06]   | `SkipValue(bool)` / `SkipToParent(bool)`         | instance | skips a sub-tree; flag bypasses conformance checks      |
|  [07]   | `ReadEncodedValue(bool) -> ReadOnlyMemory<byte>` | instance | returns the next item's encoded bytes verbatim          |
|  [08]   | `Reset(ReadOnlyMemory<byte>)`                    | instance | rebinds the reader to a new buffer                      |
|  [09]   | `BytesRemaining` / `CurrentDepth`                | property | unconsumed byte count and nesting depth                 |

[ENTRYPOINT_SCOPE]: reader scalar surface — instance `CborReader` methods

| [INDEX] | [SURFACE]                                                     | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------------ | :------------------------------------------ |
|  [01]   | `ReadInt32` / `ReadInt64` / `ReadUInt32` / `ReadUInt64`       | typed integer reads, range-checked per type |
|  [02]   | `ReadCborNegativeIntegerRepresentation() -> ulong`            | raw negative-integer representation         |
|  [03]   | `ReadBigInteger()` / `ReadDecimal()`                          | tag-2/3 bignum and tag-4 decimal-fraction   |
|  [04]   | `ReadHalf()` / `ReadSingle()` / `ReadDouble()`                | half/single/double precision                |
|  [05]   | `ReadBoolean` / `ReadNull` / `ReadSimpleValue`                | major-type-7 simple values                  |
|  [06]   | `ReadByteString() -> byte[]`                                  | allocates a copy of the octet string        |
|  [07]   | `TryReadByteString(Span<byte>, out int)`                      | allocation-free read into a caller span     |
|  [08]   | `ReadDefiniteLengthByteString() -> ReadOnlyMemory<byte>`      | zero-copy slice into the source buffer      |
|  [09]   | `ReadTextString() -> string`                                  | decodes a UTF-8 string                      |
|  [10]   | `TryReadTextString(Span<char>, out int)`                      | allocation-free decode into a caller span   |
|  [11]   | `ReadDefiniteLengthTextStringBytes() -> ReadOnlyMemory<byte>` | zero-copy UTF-8 bytes of the text string    |
|  [12]   | `ReadTag() -> CborTag`                                        | consumes a tag prefix                       |
|  [13]   | `ReadDateTimeOffset()` / `ReadUnixTimeSeconds()`              | tag-0 string / tag-1 epoch decode           |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `System.Formats.Cbor` is the whole surface: one namespace, no DI, no attributes, no schema, a hand-driven token stream.
- `CborWriter` owns an internal growable buffer seeded by `initialCapacity`; `CborReader` is read-only over an immutable `ReadOnlyMemory<byte>`, and no `Stream` overload exists — the whole item set is buffer-resident.
- `ConformanceMode` is fixed at construction; `Canonical`/`Ctap2Canonical` defer map-key sorting to `WriteEndMap`, reordering per-key encoding ranges by RFC 8949 byte order, so deterministic output needs no caller-side sort.
- `ConvertIndefiniteLengthEncodings` rewrites indefinite frames to definite on `Encode()`; `AllowMultipleRootLevelValues` permits a top-level item sequence in one buffer.
- `PeekState()` drives the decode loop: each `CborReaderState` selects the matching typed `Read*`, a wrong-type read throws `InvalidOperationException`, and structurally malformed bytes throw `CborContentException`.
- Zero-copy reads (`ReadDefiniteLengthByteString`, `ReadDefiniteLengthTextStringBytes`, `ReadEncodedValue`) return `ReadOnlyMemory<byte>` over the source buffer; `TryRead*(Span, out int)` targets a caller span with no allocation.

[STACKING]:
- `api-hashing`: `Canonical` `Encode()` bytes feed `XxHash128.HashToUInt128` through the seed-zero `ContentHash.Of` entry, keying the `ContentKey` — canonical mode is the seam holding the key stable across map-insertion order.
- `api-messagepack` / `api-chr-avro`: codec-selection peers — CBOR owns the self-describing schema-free blob, `MessagePack` the schemaless wire, and Avro the schema-governed evolving-payload leg.
- `api-cloudevents`: a nested pre-encoded CBOR item splices verbatim through `WriteEncodedValue` and re-extracts through `ReadEncodedValue`, the outer codec never re-parsing the inner body; `WriteTag(CborTag.SelfDescribeCbor)` marks a self-describing top-level frame for content-type-free detection.
- within-lib: the blob rail composes `new CborWriter(Canonical)` -> `Encode()` -> hash -> `ContentKey`, and reads untrusted ingestion under a `Strict`/`Ctap2Canonical` `CborReader` whose `PeekState()` loop bounds `CurrentDepth` against a profile cap and refuses indefinite-length frames before allocating, faulting a depth-bomb as `CborContentException` — the BCL caps no decompressed size, so the depth/length guard is the rail's.

[LOCAL_ADMISSION]:
- CBOR is a codec inside blob/snapshot profiles, never public Persistence vocabulary; conformance mode and tag usage are profile data.
- A stored CBOR body carries receipt projection for codec, conformance mode, and compression/redaction class, matching the `MessagePack` payload contract.
- `CborReader` is buffer-resident: a body larger than memory is framed into items by the store layer (`api-objectstore`), never streamed through one reader.
- Canonical determinism binds wherever bytes feed content identity; `Lax`/`Strict` are read-time validation choices and never key a `ContentKey`.

[RAIL_LAW]:
- Package: `System.Formats.Cbor`
- Owns: RFC 8949 self-describing binary blob/snapshot codec with canonical determinism
- Accept: profile-declared CBOR serialization, canonical-mode content addressing, bounded untrusted decode
- Reject: hand-rolled CBOR framing, `Lax`-mode content keys, unbounded indefinite-length decode on the untrusted path
