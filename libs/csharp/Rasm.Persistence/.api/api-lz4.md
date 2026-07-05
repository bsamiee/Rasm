# [RASM_PERSISTENCE_API_LZ4]

`K4os.Compression.LZ4` supplies the managed LZ4 block codec: the `LZ4Codec` raw span/pointer
compressor, the self-describing `LZ4Pickler` frame (with a zero-allocation `IBufferWriter<byte>`
sink mirror), the `LZ4Level` fast/HC/OPT/MAX gradient, and the chained streaming
`ILZ4Encoder`/`ILZ4Decoder` pipeline (`Topup`/`Encode`/`Inject`/`Drain`/`Peek`) backing the
`CompressionPolicy` snapshot-codec axis. This is the standalone-frame and >1-MiB streaming codec
that sits beside MessagePack's in-codec `Lz4BlockArray`, never double-framing it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `K4os.Compression.LZ4`
- package: `K4os.Compression.LZ4`
- version: `1.3.8`
- license: MIT (Milosz Krajewski)
- assembly: `K4os.Compression.LZ4`
- namespace: `K4os.Compression.LZ4`, `K4os.Compression.LZ4.Encoders`, `K4os.Compression.LZ4.Internal`
- target: multi-target (`net462`, `net5.0`, `net6.0`, `netstandard2.0`, `netstandard2.1`); the `net10.0` consumer binds `lib/net6.0`
- asset: pure-managed runtime library, AnyCPU, no native runtime
- rail: snapshot-codec

## [02]-[PUBLIC_TYPES]

[CODEC_TYPES]: block codec, frame, and level (namespace `K4os.Compression.LZ4`)
- rail: snapshot-codec

| [INDEX] | [SYMBOL]     | [PACKAGE_ROLE]   | [CAPABILITY]                                                              |
| :-----: | :----------- | :--------------- | :----------------------------------------------------------------------- |
|  [01]   | `LZ4Codec`   | block codec root | span/pointer/array `Encode`/`Decode`/`PartialDecode`, `MaximumOutputSize` |
|  [02]   | `LZ4Pickler` | self-describing frame | length-prefixed `Pickle`/`Unpickle` with `IBufferWriter<byte>` sink mirror |
|  [03]   | `LZ4Level`   | compression level | `L00_FAST`, `L03_HC`..`L09_HC`, `L10_OPT`..`L12_MAX` gradient             |

[ENCODER_TYPES]: streaming encoder/decoder pipeline (namespace `K4os.Compression.LZ4.Encoders`)
- rail: snapshot-codec

| [INDEX] | [SYMBOL]               | [PACKAGE_ROLE]       | [CAPABILITY]                                                       |
| :-----: | :--------------------- | :------------------- | :---------------------------------------------------------------- |
|  [01]   | `ILZ4Encoder`          | encoder contract     | `IDisposable`; `BlockSize`/`BytesReady`, `Topup`, `Encode`        |
|  [02]   | `ILZ4Decoder`          | decoder contract     | `IDisposable`; `BlockSize`/`BytesReady`, `Decode`, `Inject`, `Drain`, `Peek` |
|  [03]   | `LZ4Encoder`           | encoder factory      | `Create(chaining, level, blockSize, extraBlocks)` selects fast/HC |
|  [04]   | `LZ4Decoder`           | decoder factory      | `Create(chaining, blockSize, extraBlocks)` selects chain/block    |
|  [05]   | `LZ4BlockEncoder`      | block encoder        | `: LZ4EncoderBase`; single-block fast encode                      |
|  [06]   | `LZ4FastChainEncoder`  | chained fast encoder | `: LZ4EncoderBase`; cross-block dictionary, fast level            |
|  [07]   | `LZ4HighChainEncoder`  | chained HC encoder   | `: LZ4EncoderBase`; cross-block dictionary, HC/OPT level          |
|  [08]   | `LZ4BlockDecoder`      | block decoder        | `: UnmanagedResources, ILZ4Decoder`; single-block decode          |
|  [09]   | `LZ4ChainDecoder`      | chain decoder        | `: UnmanagedResources, ILZ4Decoder`; chained-block decode + `Peek` |
|  [10]   | `EncoderAction`        | encoder step kind    | `None`/`Loaded`/`Copied`/`Encoded` — the `Encode` step result enum |
|  [11]   | `LZ4EncoderExtensions` | encoder `ref` helpers | `Topup`/`Encode` cursor-advancing overloads returning `EncoderAction` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: raw block codec
- rail: snapshot-codec

| [INDEX] | [SURFACE]                                                              | [CALL_SHAPE]   | [CAPABILITY]                                              |
| :-----: | :--------------------------------------------------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `LZ4Codec.Encode(ReadOnlySpan<byte> src, Span<byte> dst, LZ4Level)`    | span codec     | compresses into a caller buffer; returns written length  |
|  [02]   | `LZ4Codec.Decode(ReadOnlySpan<byte> src, Span<byte> dst)`             | span codec     | decompresses into a caller buffer of known size          |
|  [03]   | `LZ4Codec.Decode(ReadOnlySpan<byte> src, Span<byte> dst, ReadOnlySpan<byte> dict)` | dictionary decode | decompresses against a shared dictionary           |
|  [04]   | `LZ4Codec.PartialDecode(ReadOnlySpan<byte> src, Span<byte> dst)`      | bounded decode | decodes only enough to fill a smaller destination span   |
|  [05]   | `LZ4Codec.MaximumOutputSize(int length)`                             | sizing call    | worst-case compressed bound for destination allocation   |
|  [06]   | `LZ4Codec.Enforce32` / `LZ4Codec.Version`                            | static policy  | forces 32-bit codepath; reports the linked codec version |

[ENTRYPOINT_SCOPE]: self-describing frame (`LZ4Pickler`)
- rail: snapshot-codec

| [INDEX] | [SURFACE]                                                                  | [CALL_SHAPE]    | [CAPABILITY]                                                |
| :-----: | :------------------------------------------------------------------------- | :-------------- | :--------------------------------------------------------- |
|  [01]   | `LZ4Pickler.Pickle(ReadOnlySpan<byte> src, LZ4Level)`                      | frame write     | length-prefixed self-describing frame; stack-allocs ≤1 KiB |
|  [02]   | `LZ4Pickler.Unpickle(ReadOnlySpan<byte> src)`                             | frame read      | allocates and returns the decoded payload                  |
|  [03]   | `LZ4Pickler.Unpickle(ReadOnlySpan<byte> src, Span<byte> output)`         | frame read      | decodes into a pre-sized caller buffer                     |
|  [04]   | `LZ4Pickler.Unpickle<TBufferWriter>(ReadOnlySpan<byte>, TBufferWriter)`   | sink read       | zero-copy decode into an `IBufferWriter<byte>` sink         |
|  [05]   | `LZ4Pickler.UnpickledSize(ReadOnlySpan<byte> src)`                       | frame inspect   | reads the decoded length from the frame header             |

[ENTRYPOINT_SCOPE]: chained streaming pipeline (`ILZ4Encoder`/`ILZ4Decoder`)
- rail: snapshot-codec

| [INDEX] | [SURFACE]                                                          | [CALL_SHAPE]   | [CAPABILITY]                                                        |
| :-----: | :----------------------------------------------------------------- | :------------- | :----------------------------------------------------------------- |
|  [01]   | `LZ4Encoder.Create(chaining, level, blockSize, extraBlocks)`      | factory        | builds a block/fast-chain/HC-chain encoder over a fixed block size |
|  [02]   | `LZ4Decoder.Create(chaining, blockSize, extraBlocks)`            | factory        | builds a block or chain decoder matching the encoder               |
|  [03]   | `encoder.Topup(ref byte* src, int length)`                       | feed           | copies up to `BlockSize` input bytes; advances the cursor          |
|  [04]   | `encoder.Encode(ref byte* dst, int length, bool allowCopy)`      | flush          | emits one compressed block; returns an `EncoderAction`             |
|  [05]   | `decoder.Decode(byte* src, int length, int blockSize)`           | block decode   | decodes one chained block into the internal window                 |
|  [06]   | `decoder.Inject(byte* src, int length)`                          | dictionary inject | seeds the decoder window from a prior block                      |
|  [07]   | `decoder.Drain(byte* dst, int offset, int length)`              | drain          | copies decoded bytes out of the internal window                    |
|  [08]   | `decoder.Peek(int offset)`                                       | window peek    | returns a pointer into the live decode window                      |

## [04]-[IMPLEMENTATION_LAW]

[COMPRESSION_PROFILE]:
- block codec: `LZ4Codec.Encode`/`Decode` operate on `ReadOnlySpan<byte>`/`Span<byte>`/`byte*`/`byte[]`; the destination must be sized by `MaximumOutputSize`; `Decode` requires the exact decompressed length, `PartialDecode` fills a smaller destination and stops.
- self-describing frame: `LZ4Pickler` prepends a length header (`UnpickledSize` reads it back), so a pickled frame round-trips with no out-of-band size; the `Unpickle(..., IBufferWriter<byte>)` sink decodes without a pre-sized array.
- level gradient: `LZ4Level` runs `L00_FAST` (block, no dictionary), `L03_HC`..`L09_HC` (high-compression), `L10_OPT`..`L12_MAX` (optimal parser); `LZ4Encoder.Create` routes `< L03_HC` to the fast encoder and `>= L03_HC` to the HC chain encoder.
- streaming pipeline: `ILZ4Encoder`/`ILZ4Decoder` are `IDisposable` and stateful — `Topup` fills the input block, `Encode` emits it, `Decode`/`Drain` read it back, and the chain variants (`LZ4FastChainEncoder`/`LZ4HighChainEncoder`/`LZ4ChainDecoder`) carry a cross-block dictionary so a multi-block stream compresses better than independent blocks. `LZ4ChainDecoder`/`LZ4BlockDecoder` derive from `UnmanagedResources`, so disposal is the deterministic buffer-ownership boundary.
- `EncoderAction` (`None`/`Loaded`/`Copied`/`Encoded`) is the `Encode` step result the `ref`-cursor `LZ4EncoderExtensions` overloads surface; a `Copied` step means the block was stored uncompressed because it did not shrink.

[LOCAL_ADMISSION]:
- `CompressionPolicy.Lz4Fast`/`Lz4High` (`Element/codec#COMPRESSION_HASHING`) are `LZ4Pickler.Pickle(payload.Span, LZ4Level.L00_FAST)` / `Pickle(payload.Span, LZ4Level.L09_HC)` with `Unpickle(framed.Span)` — the self-describing frame carries its own decoded size so the snapshot header stores only the `CompressionId`, never a sidecar length.
- The standalone-frame route serves `SnapshotCodec.JsonStj` and `FileRaw` payloads; `SnapshotCodec.MessagePackBinary` pairs with `CompressionPolicy.None` because `MessagePackCompression.Lz4BlockArray` owns compression in-codec — double-framing an already-`Lz4BlockArray` payload through the `LZ4Pickler` is the rejected form.
- Payloads above 1 MiB stream through `LZ4Encoder`/`LZ4ChainDecoder` `Topup`/`Encode`/`Drain` chained blocks sized to a 1-MiB segment, so a large snapshot never materializes a single contiguous compressed buffer; the destination of every raw-block `Encode` is bounded by `LZ4Codec.MaximumOutputSize`.
- The `LZ4Pickler.Unpickle(..., IBufferWriter<byte>)` sink stacks onto the same `ArrayBufferWriter<byte>` the `ContentChunker.Reassemble` and `SealedSnapshot` folds already drive, so a framed decode lands directly in the receipt buffer with no intermediate array.
- Compression level, frame shape, input bounds, and output bounds are receipt data on the `CompressionPolicy` row; compression cannot obscure redaction, retention, or schema receipts, and a `Crc32`/`XxHash128` integrity tag is computed over the framed bytes independently of the codec.

[RAIL_LAW]:
- Package: `K4os.Compression.LZ4`
- Owns: LZ4 block compression, self-describing framing, and chained streaming for snapshot and blob payloads
- Accept: `LZ4Pickler` self-describing frames for standalone payloads, `LZ4Encoder`/`LZ4ChainDecoder` streaming for >1-MiB payloads, `LZ4Codec` raw blocks into caller buffers bounded by `MaximumOutputSize`
- Reject: double-framing a MessagePack `Lz4BlockArray` payload through `LZ4Pickler`, hidden compression wrappers, raw-block decode without an out-of-band size, a compression tag standing in for an integrity hash
