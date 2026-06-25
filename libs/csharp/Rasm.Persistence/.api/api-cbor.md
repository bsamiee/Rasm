# [RASM_PERSISTENCE_API_CBOR]

`System.Formats.Cbor` is the first-party BCL CBOR (RFC 8949) codec: the `CborWriter`/`CborReader` push-pull pair over a single `ReadOnlyMemory<byte>` buffer, the four `CborConformanceMode` validation profiles (`Lax`/`Strict`/`Canonical`/`Ctap2Canonical`), the full `CborTag` semantic-tag vocabulary, the `CborReaderState` peek discriminant driving a state-machine decode loop, and the typed scalar surface down to `Half`, `BigInteger`, and `decimal`. There is no schema, no attributes, no reflection — the codec is a hand-driven token stream, which is exactly why it is the self-describing IETF blob/snapshot codec orthogonal to the schemaless `MessagePack` wire format (`api-messagepack`) and the schema-governed Avro leg (`api-chr-avro`). A blob rail composes one `CborWriter` under a `Canonical` mode (deterministic map-key ordering for content addressing), feeds `Encode()` bytes to `XxHash128` (`api-hashing`) for the `ContentKey`, and re-reads peer blobs under a bounded `CborReader` whose `PeekState()` loop never trusts an indefinite-length frame.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.Formats.Cbor`
- package: `System.Formats.Cbor`
- version: `10.0.9`
- license: MIT
- assembly: `System.Formats.Cbor`
- namespace: `System.Formats.Cbor`
- bound asset: `lib/net10.0` (consumer-bound; package multi-targets `net462`/`net8.0`/`net9.0`/`net10.0` — the `net10.0` consumer binds the `net10.0` surface directly)
- runtime: in-box BCL component on `net10.0`; the package row is a deterministic floor pin, not an out-of-band dependency. Zero transitive package dependencies, AnyCPU, no native asset
- rail: blob-codec

## [02]-[PUBLIC_TYPES]

[CODEC_TYPES]: writer and reader surfaces
- rail: blob-codec

| [INDEX] | [SYMBOL]               | [PACKAGE_ROLE]       | [CAPABILITY]                                       |
| :-----: | :--------------------- | :------------------- | :------------------------------------------------- |
|  [01]   | `CborWriter`           | encoder              | push-model encoder over an internal grow buffer    |
|  [02]   | `CborReader`           | decoder              | pull-model decoder over a fixed `ReadOnlyMemory`   |
|  [03]   | `CborReaderState`      | decode discriminant  | `PeekState()` result driving the read loop         |
|  [04]   | `CborContentException` | malformed-data fault | thrown on structurally invalid CBOR during read    |

[POLICY_TYPES]: conformance and tag vocabulary
- rail: blob-codec

| [INDEX] | [SYMBOL]              | [PACKAGE_ROLE]      | [CAPABILITY]                                         |
| :-----: | :-------------------- | :------------------ | :--------------------------------------------------- |
|  [01]   | `CborConformanceMode` | validation profile  | `Lax`/`Strict`/`Canonical`/`Ctap2Canonical`          |
|  [02]   | `CborTag`             | semantic-tag enum   | RFC 8949 tag vocabulary (`ulong`-backed)             |
|  [03]   | `CborSimpleValue`     | simple-value enum   | `byte`-backed `False=20`/`True`/`Null`/`Undefined`   |

`CborConformanceMode` cases: `Lax` (no validation), `Strict` (well-formedness + no duplicate keys), `Canonical` (RFC 8949 §4.2.1 deterministic — shortest-form ints, length-sorted map keys, definite lengths), `Ctap2Canonical` (CTAP2 §6 — canonical plus depth/type restrictions for FIDO2). `Canonical`/`Ctap2Canonical` force `WriteStartMap`/`WriteStartArray` to emit definite-length headers and reorder map-key encodings on `WriteEndMap`.
`CborSimpleValue` cases (RFC 8949 major type 7): `False = 20`, `True = 21`, `Null = 22`, `Undefined = 23`. Values 0-19 and 32-255 are writable via the raw `WriteSimpleValue(CborSimpleValue)` overload.
`CborTag` cases: `DateTimeString = 0`, `UnixTimeSeconds = 1`, `UnsignedBigNum = 2`, `NegativeBigNum = 3`, `DecimalFraction = 4`, `BigFloat = 5`, `Base64UrlLaterEncoding = 21`, `Base64StringLaterEncoding = 22`, `Base16StringLaterEncoding = 23`, `EncodedCborDataItem = 24`, `Uri = 32`, `Base64Url = 33`, `Base64 = 34`, `Regex = 35`, `MimeMessage = 36`, `SelfDescribeCbor = 55799`. The typed `WriteDateTimeOffset`/`WriteUnixTimeSeconds`/`WriteBigInteger`/`WriteDecimal` helpers emit the matching tag automatically; raw `WriteTag` precedes a hand-encoded tagged item.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: writer construction and framing
- rail: blob-codec

| [INDEX] | [SURFACE]                                                       | [CALL_SHAPE]   | [CAPABILITY]                                          |
| :-----: | :-------------------------------------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `new CborWriter(conformanceMode, convertIndefiniteLengthEncodings, allowMultipleRootLevelValues, initialCapacity)` | ctor | builds an encoder with a fixed conformance profile |
|  [02]   | `WriteStartArray(int?)` / `WriteEndArray()`                     | container      | definite (length) or indefinite (`null`) array frame |
|  [03]   | `WriteStartMap(int?)` / `WriteEndMap()`                         | container      | definite or indefinite map frame; canonical reorders keys on end |
|  [04]   | `WriteStartIndefiniteLengthByteString()` / `WriteEndIndefiniteLengthByteString()` | container | chunked byte-string frame |
|  [05]   | `WriteStartIndefiniteLengthTextString()` / `WriteEndIndefiniteLengthTextString()` | container | chunked text-string frame |
|  [06]   | `WriteTag(CborTag)`                                             | semantic tag   | prefixes the next data item with a tag               |
|  [07]   | `WriteEncodedValue(ReadOnlySpan<byte>)`                         | raw splice     | appends a pre-encoded CBOR data item verbatim        |
|  [08]   | `Encode()` → `byte[]` / `Encode(Span<byte>)` / `TryEncode(Span<byte>, out int)` | drain | materializes the buffer; `TryEncode` is allocation-free |
|  [09]   | `Reset()`                                                       | reuse          | clears state for buffer reuse without realloc        |
|  [10]   | `BytesWritten` / `CurrentDepth` / `IsWriteCompleted`            | state          | encoder cursor, nesting depth, root-complete flag    |

[ENTRYPOINT_SCOPE]: writer scalar surface
- rail: blob-codec

| [INDEX] | [SURFACE]                                            | [CALL_SHAPE] | [CAPABILITY]                                  |
| :-----: | :--------------------------------------------------- | :----------- | :-------------------------------------------- |
|  [01]   | `WriteInt32` / `WriteInt64` / `WriteUInt32` / `WriteUInt64` | integer | major-type 0/1 unsigned and signed integers |
|  [02]   | `WriteCborNegativeIntegerRepresentation(ulong)`      | integer      | raw major-type-1 encoding for the full negative range |
|  [03]   | `WriteBigInteger(BigInteger)`                        | bignum       | arbitrary-precision via tag 2/3               |
|  [04]   | `WriteDecimal(decimal)`                              | decimal      | `System.Decimal` via tag 4 decimal-fraction   |
|  [05]   | `WriteHalf(Half)` / `WriteSingle(float)` / `WriteDouble(double)` | float | RFC 8949 half/single/double precision |
|  [06]   | `WriteBoolean` / `WriteNull` / `WriteSimpleValue(CborSimpleValue)` | simple | major-type-7 simple values |
|  [07]   | `WriteByteString(byte[])` / `WriteByteString(ReadOnlySpan<byte>)` | byte string | major-type-2 octet string |
|  [08]   | `WriteTextString(string)` / `WriteTextString(ReadOnlySpan<char>)` | text string | major-type-3 UTF-8 string |
|  [09]   | `WriteDateTimeOffset(DateTimeOffset)`                | tagged       | tag 0 RFC 3339 string                         |
|  [10]   | `WriteUnixTimeSeconds(long)` / `WriteUnixTimeSeconds(double)` | tagged | tag 1 epoch seconds (integer or float)        |

[ENTRYPOINT_SCOPE]: reader construction and navigation
- rail: blob-codec

| [INDEX] | [SURFACE]                                                  | [CALL_SHAPE]  | [CAPABILITY]                                       |
| :-----: | :--------------------------------------------------------- | :------------ | :------------------------------------------------- |
|  [01]   | `new CborReader(ReadOnlyMemory<byte>, conformanceMode, allowMultipleRootLevelValues)` | ctor | binds a decoder to an immutable buffer |
|  [02]   | `PeekState()` → `CborReaderState`                          | discriminant  | next-token kind without consuming — the read-loop pivot |
|  [03]   | `PeekTag()` → `CborTag`                                    | lookahead     | reads the upcoming tag without consuming           |
|  [04]   | `ReadStartArray()` → `int?` / `ReadEndArray()`             | container     | definite length or `null` for indefinite           |
|  [05]   | `ReadStartMap()` → `int?` / `ReadEndMap()`                 | container     | definite pair-count or `null` for indefinite       |
|  [06]   | `SkipValue(bool)` / `SkipToParent(bool)`                   | skip          | skips a sub-tree; `disableConformanceModeChecks` bypasses validation |
|  [07]   | `ReadEncodedValue(bool)` → `ReadOnlyMemory<byte>`          | raw slice     | returns the next item's encoded bytes verbatim     |
|  [08]   | `Reset(ReadOnlyMemory<byte>)`                              | rebind        | reuses the reader against a new buffer             |
|  [09]   | `BytesRemaining` / `CurrentDepth`                          | state         | unconsumed byte count and nesting depth            |

[ENTRYPOINT_SCOPE]: reader scalar surface
- rail: blob-codec

| [INDEX] | [SURFACE]                                              | [CALL_SHAPE] | [CAPABILITY]                                   |
| :-----: | :----------------------------------------------------- | :----------- | :--------------------------------------------- |
|  [01]   | `ReadInt32` / `ReadInt64` / `ReadUInt32` / `ReadUInt64` | integer | typed integer reads; range-checked per type    |
|  [02]   | `ReadCborNegativeIntegerRepresentation()` → `ulong`    | integer      | raw negative-integer representation            |
|  [03]   | `ReadBigInteger()` / `ReadDecimal()`                   | numeric      | tag-2/3 bignum and tag-4 decimal-fraction      |
|  [04]   | `ReadHalf()` / `ReadSingle()` / `ReadDouble()`         | float        | half/single/double precision                   |
|  [05]   | `ReadBoolean` / `ReadNull` / `ReadSimpleValue`         | simple       | major-type-7 simple values                     |
|  [06]   | `ReadByteString()` → `byte[]`                          | byte string  | allocates a copy of the octet string           |
|  [07]   | `TryReadByteString(Span<byte>, out int)`               | byte string  | allocation-free read into a caller span        |
|  [08]   | `ReadDefiniteLengthByteString()` → `ReadOnlyMemory<byte>` | byte string | zero-copy slice into the source buffer       |
|  [09]   | `ReadTextString()` → `string`                          | text string  | decodes a UTF-8 string                         |
|  [10]   | `TryReadTextString(Span<char>, out int)`               | text string  | allocation-free decode into a caller span      |
|  [11]   | `ReadDefiniteLengthTextStringBytes()` → `ReadOnlyMemory<byte>` | text string | zero-copy UTF-8 bytes of the text string |
|  [12]   | `ReadTag()` → `CborTag`                                | semantic tag | consumes a tag prefix                          |
|  [13]   | `ReadDateTimeOffset()` / `ReadUnixTimeSeconds()`       | tagged       | tag-0 string / tag-1 epoch decode              |

## [04]-[IMPLEMENTATION_LAW]

[CBOR_TOPOLOGY]:
- namespace: `System.Formats.Cbor` — the entire surface; no sub-namespaces, no DI, no attributes
- the encoder owns an internal growable buffer (`initialCapacity` seeds it); the decoder is read-only over an immutable `ReadOnlyMemory<byte>` — there is no streaming `Stream` overload, the whole item set is buffer-resident
- `CborWriter.ConformanceMode` is fixed at construction; `Canonical`/`Ctap2Canonical` defer map-key sorting to `WriteEndMap`, where the writer reorders the per-key encoding ranges by RFC 8949 §4.2.1 byte order — deterministic output requires no caller-side key sorting
- `ConvertIndefiniteLengthEncodings = true` rewrites indefinite-length frames to definite-length on `Encode()`; `AllowMultipleRootLevelValues` permits a sequence of top-level items in one buffer
- the decode loop is a `PeekState()` switch: each `CborReaderState` (`UnsignedInteger`, `NegativeInteger`, `ByteString`, `StartArray`, `StartMap`, `Tag`, `HalfPrecisionFloat`, `Boolean`, `Null`, `Finished`, the `Start/EndIndefiniteLength*` markers, …) selects the matching typed `Read*` call; a wrong-type read throws `InvalidOperationException`, structurally malformed bytes throw `CborContentException`
- the zero-copy reads (`ReadDefiniteLengthByteString`, `ReadDefiniteLengthTextStringBytes`, `ReadEncodedValue`) return `ReadOnlyMemory<byte>` slices over the source buffer; the `TryRead*(Span<…>, out int)` reads target a caller-owned span with no heap allocation

[INTEGRATION_LAW]:
- Content addressing stacks CBOR under the hasher: a blob serialized with `new CborWriter(CborConformanceMode.Canonical)` then `Encode()` produces a byte-deterministic representation (map keys reordered, shortest-form integers, definite lengths), and `XxHash128.HashToUInt128` over those bytes (`api-hashing`) is the `Schema/identity#IDENTITY_LADDER` `ContentKey` — the canonical mode is the load-bearing seam making the content key stable across map-insertion order.
- Codec selection is profile data, not a public surface: a snapshot/blob profile that picks CBOR over `MessagePack` records the choice in the codec receipt (`api-messagepack` `[LOCAL_ADMISSION]`), so a stored body carries its codec, conformance mode, and any compression class as receipt fields. CBOR carries no schema, so an evolving record payload routes to Avro (`api-chr-avro`) instead; CBOR owns the self-describing, schema-free blob.
- Untrusted-blob ingestion (a CBOR body received over `Sync/egress`) reads under a `Strict` (or `Ctap2Canonical`) `CborReader` whose `PeekState()` loop bounds `CurrentDepth` against a profile cap and refuses indefinite-length frames before allocating, so a depth-bomb or unterminated-stream payload faults as `CborContentException` instead of exhausting memory — the BCL exposes no decompressed-size cap, so the depth/length guard is the rail's responsibility.
- Envelope interop stacks at the tag layer: a CloudEvents (`api-cloudevents`) or registry payload that nests a pre-encoded CBOR item is spliced verbatim with `WriteEncodedValue` and re-extracted with `ReadEncodedValue` — the outer codec never re-parses the inner body, and `WriteTag(CborTag.SelfDescribeCbor)` (55799) marks a self-describing top-level frame for content-type-free detection.

[LOCAL_ADMISSION]:
- CBOR is a codec inside blob/snapshot profiles, not public Persistence vocabulary; the conformance mode and tag usage are profile data.
- A stored CBOR body requires receipt projection for codec, conformance mode, and compression/redaction class, identical to the `MessagePack` payload contract.
- The reader is buffer-resident: a body larger than memory is chunked by the store layer (`api-objectstore`) into framed items, never streamed through one `CborReader`.
- Canonical determinism is required wherever the bytes feed content identity; `Lax`/`Strict` modes are read-time validation choices and never feed a `ContentKey`.

[RAIL_LAW]:
- Package: `System.Formats.Cbor`
- Owns: RFC 8949 self-describing binary blob/snapshot codec with canonical determinism
- Accept: profile-declared CBOR serialization, canonical-mode content addressing, bounded untrusted decode
- Reject: hand-rolled CBOR framing, `Lax`-mode content keys, unbounded indefinite-length decode on the untrusted path
