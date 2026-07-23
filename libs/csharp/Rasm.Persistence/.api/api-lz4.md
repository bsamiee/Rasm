# [RASM_PERSISTENCE_API_LZ4]

`K4os.Compression.LZ4` owns the managed LZ4 codec on the snapshot-compression axis: raw blocks into a caller-sized buffer, a self-describing frame carrying its own decoded length, and a chained pipeline whose cross-block dictionary beats independent blocks over a multi-block payload. It frames a standalone payload exactly once — a payload whose serializer already compresses in-codec pairs with an uncompressed policy row.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `K4os.Compression.LZ4`
- package: `K4os.Compression.LZ4` (MIT)
- assembly: `K4os.Compression.LZ4`
- namespace: `K4os.Compression.LZ4`, `K4os.Compression.LZ4.Encoders`, `K4os.Compression.LZ4.Internal`
- target: the `net10.0` consumer binds `lib/net6.0`
- asset: pure-managed AnyCPU runtime library, no native runtime
- rail: snapshot-codec

## [02]-[PUBLIC_TYPES]

[CODEC_TYPES]: block codec, frame, and level (`K4os.Compression.LZ4`)

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :----------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `LZ4Codec`   | static class  | span/pointer/array `Encode`/`Decode`/`PartialDecode` + size bound |
|  [02]   | `LZ4Pickler` | static class  | length-prefixed `Pickle`/`Unpickle` with sink twins on both legs  |
|  [03]   | `LZ4Level`   | enum          | `L00_FAST`, `L03_HC`..`L09_HC`, `L10_OPT`, `L11_OPT`, `L12_MAX`   |

[ENCODER_TYPES]: streaming pipeline (`K4os.Compression.LZ4.Encoders`)

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]  | [CAPABILITY]                                                    |
| :-----: | :--------------------- | :------------- | :-------------------------------------------------------------- |
|  [01]   | `ILZ4Encoder`          | interface      | `IDisposable`; `BlockSize`/`BytesReady`, `Topup`, `Encode`      |
|  [02]   | `ILZ4Decoder`          | interface      | `IDisposable`; adds `Inject`, `Drain`, `Peek` to `Decode`       |
|  [03]   | `LZ4Encoder`           | static class   | `Create` selects block, fast-chain, or HC-chain                 |
|  [04]   | `LZ4Decoder`           | static class   | `Create` selects block or chain                                 |
|  [05]   | `LZ4EncoderBase`       | abstract class | `: UnmanagedResources, ILZ4Encoder`; the shared encode pump     |
|  [06]   | `LZ4BlockEncoder`      | class          | `: LZ4EncoderBase`; single-block encode, no dictionary          |
|  [07]   | `LZ4FastChainEncoder`  | class          | `: LZ4EncoderBase`; cross-block dictionary at fast level        |
|  [08]   | `LZ4HighChainEncoder`  | class          | `: LZ4EncoderBase`; cross-block dictionary at HC/OPT level      |
|  [09]   | `LZ4BlockDecoder`      | class          | `: UnmanagedResources, ILZ4Decoder`; single-block decode        |
|  [10]   | `LZ4ChainDecoder`      | class          | `: UnmanagedResources, ILZ4Decoder`; chained decode with `Peek` |
|  [11]   | `EncoderAction`        | enum           | `None`/`Loaded`/`Copied`/`Encoded` step result                  |
|  [12]   | `LZ4EncoderExtensions` | static class   | span and cursor combinators over both interfaces                |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: raw block codec

Every row is a `LZ4Codec` member; `byte*` and `byte[]`-offset overloads mirror each span form.

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------------------------ | :------- | :----------------------------------------------- |
|  [01]   | `Encode(ReadOnlySpan<byte>, Span<byte>, LZ4Level) -> int`           | static   | compress into a caller buffer; written length    |
|  [02]   | `Decode(ReadOnlySpan<byte>, Span<byte>) -> int`                     | static   | decompress into a buffer of the known size       |
|  [03]   | `Decode(ReadOnlySpan<byte>, Span<byte>, ReadOnlySpan<byte>) -> int` | static   | decode against a shared dictionary               |
|  [04]   | `PartialDecode(ReadOnlySpan<byte>, Span<byte>) -> int`              | static   | decode only enough to fill a smaller destination |
|  [05]   | `MaximumOutputSize(int) -> int`                                     | static   | worst-case compressed bound for the destination  |
|  [06]   | `Version -> int`                                                    | static   | linked block-format version constant             |
|  [07]   | `Enforce32`                                                         | property | pins the 32-bit codepath process-wide            |

[ENTRYPOINT_SCOPE]: self-describing frame (`LZ4Pickler`)

`byte[]`, `byte[]`-offset, and `byte*` overloads mirror every span form; the generic sink twins constrain `TBufferWriter : IBufferWriter<byte>`.

| [INDEX] | [SURFACE]                                                            | [SHAPE] | [CAPABILITY]                                  |
| :-----: | :------------------------------------------------------------------- | :------ | :-------------------------------------------- |
|  [01]   | `LZ4Pickler.Pickle(ReadOnlySpan<byte>, LZ4Level) -> byte[]`          | static  | length-prefixed self-describing frame         |
|  [02]   | `LZ4Pickler.Pickle<TBufferWriter>(ReadOnlySpan<byte>, TW, LZ4Level)` | static  | frame straight into a sink, no array minted   |
|  [03]   | `LZ4Pickler.Unpickle(ReadOnlySpan<byte>) -> byte[]`                  | static  | allocate and return the decoded payload       |
|  [04]   | `LZ4Pickler.Unpickle(ReadOnlySpan<byte>, Span<byte>)`                | static  | decode into a pre-sized caller buffer         |
|  [05]   | `LZ4Pickler.Unpickle<TBufferWriter>(ReadOnlySpan<byte>, TW)`         | static  | decode into a sink, no array minted           |
|  [06]   | `LZ4Pickler.UnpickledSize(ReadOnlySpan<byte>) -> int`                | static  | read the decoded length from the frame header |

[ENTRYPOINT_SCOPE]: chained streaming primitives

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :----------------------------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `LZ4Encoder.Create(bool, LZ4Level, int, int) -> ILZ4Encoder` | factory  | block, fast-chain, or HC-chain over a block size |
|  [02]   | `LZ4Decoder.Create(bool, int, int) -> ILZ4Decoder`           | factory  | block or chain decoder matching the encoder      |
|  [03]   | `ILZ4Encoder.Topup(byte*, int) -> int`                       | instance | copy up to `BlockSize` input bytes               |
|  [04]   | `ILZ4Encoder.Encode(byte*, int, bool) -> int`                | instance | emit one compressed block                        |
|  [05]   | `ILZ4Decoder.Decode(byte*, int, int) -> int`                 | instance | decode one block into the window                 |
|  [06]   | `ILZ4Decoder.Inject(byte*, int) -> int`                      | instance | seed the window from a prior block               |
|  [07]   | `ILZ4Decoder.Drain(byte*, int, int)`                         | instance | copy decoded bytes out of the window             |
|  [08]   | `ILZ4Decoder.Peek(int) -> byte*`                             | instance | pointer into the live decode window              |

[ENTRYPOINT_SCOPE]: pump combinators (`LZ4EncoderExtensions`)

Each row folds the pointer primitives into one step over a span, an array, or an advancing cursor, so a pump loop drives the chain in safe code.

| [INDEX] | [SURFACE]                                                                      | [SHAPE] | [CAPABILITY]                         |
| :-----: | :----------------------------------------------------------------------------- | :------ | :----------------------------------- |
|  [01]   | `TopupAndEncode(ReadOnlySpan<byte>, Span<byte>, bool, bool, out int, out int)` | fold    | feed and emit one block, one step    |
|  [02]   | `FlushAndEncode(Span<byte>, bool, out int) -> EncoderAction`                   | fold    | emit the pending block at frame end  |
|  [03]   | `DecodeAndDrain(ReadOnlySpan<byte>, Span<byte>, out int) -> bool`              | fold    | decode and drain one block, one step |
|  [04]   | `Drain(Span<byte>, int, int)`                                                  | fold    | copy the decode window into a span   |
|  [05]   | `Topup(ref byte*, int) -> bool`                                                | fold    | cursor-advancing pointer feed        |
|  [06]   | `Topup(byte[], ref int, int) -> bool`                                          | fold    | cursor-advancing array feed          |
|  [07]   | `Encode(ref byte*, int, bool) -> EncoderAction`                                | fold    | cursor-advancing pointer emit        |
|  [08]   | `Encode(byte[], ref int, int, bool) -> EncoderAction`                          | fold    | cursor-advancing array emit          |
|  [09]   | `Inject(byte[], int, int) -> int`                                              | fold    | array-offset window seed             |
|  [10]   | `Decode(byte[], int, int, int) -> int`                                         | fold    | array-offset block decode            |

- `TopupAndEncode`: `forceEncode` emits a partial block, `allowCopy` admits the stored form, and the `out` counts report loaded and encoded bytes.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `LZ4Codec` decodes only against an out-of-band decoded length; `LZ4Pickler` prepends that length itself, so a framed payload round-trips with no sidecar and `UnpickledSize` reads it back before allocation.
- `LZ4Level` rises monotonically from `L00_FAST` through the HC band into the optimal parser, and `LZ4Encoder.Create` routes below `L03_HC` to the fast encoder, the rest to the HC chain.
- `ILZ4Encoder`/`ILZ4Decoder` hold block state over `UnmanagedResources`, so disposal is the deterministic buffer-ownership boundary; only the chain variants carry a cross-block dictionary.
- `EncoderAction.Copied` reports a block stored uncompressed because it did not shrink — the step result every pump loop discriminates on.

[STACKING]:
- `api-messagepack`(`.api/api-messagepack.md`): `MessagePackCompression.Lz4BlockArray` frames LZ4 inside the serializer at ext type 98, so `SnapshotFormat.Admits` binds that codec to `CompressionPolicy.None` and this package frames only what MessagePack leaves raw.
- `api-highperformance`(`../../.api/api-highperformance.md`): `ArrayPoolBufferWriter<byte>` satisfies the `TBufferWriter` constraint on `Pickle` and `Unpickle` alike, so one pooled rental carries the frame in and the payload out.
- `api-hashing`(`../../.api/api-hashing.md`): `XxHash128.Append(ReadOnlySpan<byte>)` folds each block as the pump emits it, so the content key over stored bytes costs no second pass.
- `Element/codec#COMPRESSION_HASHING`: `ContentChunker.Reassemble` drives an `ArrayBufferWriter<byte>` the `Unpickle` sink overload decodes a framed chunk straight into, and a payload past one contiguous compressed buffer rides `TopupAndEncode`/`DecodeAndDrain` block-at-a-time against that same writer.

[LOCAL_ADMISSION]:
- `CompressionPolicy.Lz4Fast`/`Lz4High` pack through `LZ4Pickler.Pickle` at `LZ4Level.L00_FAST`/`L09_HC` and unpack through `Unpickle`; the frame carries its decoded size, so `SnapshotHeader` stores the row's `HeaderId` alone.
- Level, frame shape, and both bounds project as receipt data, and the `Crc32`/`XxHash128` integrity tag folds the framed bytes independently, so a codec swap leaves the receipt intact.

[RAIL_LAW]:
- Package: `K4os.Compression.LZ4`
- Owns: LZ4 block compression, self-describing framing, and chained streaming for snapshot and blob payloads
- Accept: `LZ4Pickler` frames for standalone payloads; the `LZ4EncoderExtensions` span fold for streamed payloads; `LZ4Codec` raw blocks into `MaximumOutputSize`-bounded buffers; `IBufferWriter<byte>` sinks on both frame legs
- Reject: a second frame over a payload its serializer already compressed; a raw-block decode with no out-of-band size; a compression tag standing in for an integrity hash; a pointer pump where a span combinator spans the same step
