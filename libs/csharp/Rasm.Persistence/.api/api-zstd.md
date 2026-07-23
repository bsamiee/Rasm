# [RASM_PERSISTENCE_API_ZSTD]

`ZstdSharp.Port` transpiles libzstd into managed C#, so Zstandard compression ships with no native runtime and no RID-specific asset. It owns the high-ratio half of the snapshot-compression axis: a reusable context tuned across the full advanced parameter surface, an `OperationStatus` pump for payloads past one contiguous buffer, and trained dictionaries for the small-similar-blob regime. A payload frames exactly once here, so a body its serializer or IPC stream already compressed pairs with an uncompressed policy row.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ZstdSharp.Port`
- package: `ZstdSharp.Port` (MIT)
- assembly: `ZstdSharp`
- namespace: `ZstdSharp`, `ZstdSharp.Unsafe`
- target: the `net10.0` consumer binds `lib/net9.0`
- asset: pure-managed AnyCPU runtime library, no native runtime
- abi: spans in, `System.Buffers.OperationStatus` out of the pump; `Compressor` and `Decompressor` are `IDisposable`
- rail: snapshot-codec

## [02]-[PUBLIC_TYPES]

[CODEC_TYPES]: codec contexts, stream adapters, trainer, fault (`ZstdSharp`)

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :-------------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `Compressor`          | class         | `IDisposable` cctx; level, parameters, dictionary, one-shot, pump |
|  [02]   | `Decompressor`        | class         | `IDisposable` dctx; parameter, dictionary, one-shot, pump         |
|  [03]   | `CompressionStream`   | class         | `: Stream` write-only; sync and async write over a cctx           |
|  [04]   | `DecompressionStream` | class         | `: Stream` read-only; sync and async read over a dctx             |
|  [05]   | `DictBuilder`         | static class  | trains a dictionary from sample blobs                             |
|  [06]   | `ZstdException`       | class         | `: Exception`; carries `ZSTD_ErrorCode Code`                      |

[PARAMETER_TYPES]: libzstd advanced-API mirrors (`ZstdSharp.Unsafe`)

`SetParameter` is the one knob entry into the surface below, and `Methods` exposes the raw libzstd entry points the managed types wrap.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                                              |
| :-----: | :------------------------- | :------------ | :-------------------------------------------------------- |
|  [01]   | `ZSTD_cParameter`          | enum          | compress knobs, one per libzstd `ZSTD_c_*`                |
|  [02]   | `ZSTD_dParameter`          | enum          | `ZSTD_d_windowLogMax` is the stable decode knob           |
|  [03]   | `ZSTD_strategy`            | enum          | match-finder ladder, fast through btultra2                |
|  [04]   | `ZSTD_EndDirective`        | enum          | `continue`/`flush`/`end`, mapped from `isFinalBlock`      |
|  [05]   | `ZSTD_ErrorCode`           | enum          | libzstd fault code on `ZstdException.Code`                |
|  [06]   | `ZSTD_bounds`              | struct        | `error`, `lowerBound`, `upperBound` for one parameter     |
|  [07]   | `ZDICT_fastCover_params_t` | struct        | FastCover trainer knobs                                   |
|  [08]   | `ZDICT_params_t`           | struct        | `compressionLevel`, `notificationLevel`, `dictID`         |
|  [09]   | `Methods`                  | static class  | raw `ZSTD_*`/`ZDICT_*` bindings under the managed surface |

[ZSTD_cParameter]: `ZSTD_c_compressionLevel` `ZSTD_c_windowLog` `ZSTD_c_hashLog` `ZSTD_c_chainLog` `ZSTD_c_searchLog` `ZSTD_c_minMatch` `ZSTD_c_targetLength` `ZSTD_c_strategy` `ZSTD_c_targetCBlockSize` `ZSTD_c_enableLongDistanceMatching` `ZSTD_c_ldmHashLog` `ZSTD_c_ldmMinMatch` `ZSTD_c_ldmBucketSizeLog` `ZSTD_c_ldmHashRateLog` `ZSTD_c_contentSizeFlag` `ZSTD_c_checksumFlag` `ZSTD_c_dictIDFlag` `ZSTD_c_nbWorkers` `ZSTD_c_jobSize` `ZSTD_c_overlapLog`

[ZSTD_strategy]: `ZSTD_fast` `ZSTD_dfast` `ZSTD_greedy` `ZSTD_lazy` `ZSTD_lazy2` `ZSTD_btlazy2` `ZSTD_btopt` `ZSTD_btultra` `ZSTD_btultra2`

[ZDICT_fastCover_params_t]: `k` `d` `f` `steps` `nbThreads` `splitPoint` `accel` `shrinkDict` `shrinkDictMaxRegression` `zParams`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: one-shot codec

`byte[]`, `byte[]`-offset, and `ArraySegment<byte>` overloads mirror each `Compressor` span form; `Decompressor` mirrors every form but `ArraySegment` on `Unwrap`.

| [INDEX] | [SURFACE]                                                                 | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------------------------ | :------- | :------------------------------------------ |
|  [01]   | `new Compressor(int)`                                                     | ctor     | reusable cctx at a starting level           |
|  [02]   | `Compressor.MinCompressionLevel -> int`                                   | property | static lower level bound                    |
|  [03]   | `Compressor.MaxCompressionLevel -> int`                                   | property | static upper level bound                    |
|  [04]   | `Compressor.Level`                                                        | property | re-applies `ZSTD_c_compressionLevel` on set |
|  [05]   | `Compressor.GetCompressBound(int) -> int`                                 | static   | worst-case compressed size for a buffer     |
|  [06]   | `Compressor.GetCompressBoundLong(ulong) -> ulong`                         | static   | the same bound past `int` range             |
|  [07]   | `Compressor.Wrap(ReadOnlySpan<byte>) -> Span<byte>`                       | instance | compress into a freshly minted buffer       |
|  [08]   | `Compressor.Wrap(ReadOnlySpan<byte>, Span<byte>) -> int`                  | instance | compress into a caller buffer               |
|  [09]   | `Compressor.TryWrap(ReadOnlySpan<byte>, Span<byte>, out int) -> bool`     | instance | no-throw compress; `false` on short dest    |
|  [10]   | `new Decompressor()`                                                      | ctor     | reusable dctx                               |
|  [11]   | `Decompressor.GetDecompressedSize(ReadOnlySpan<byte>) -> ulong`           | static   | decoded size stored in the frame header     |
|  [12]   | `Decompressor.Unwrap(ReadOnlySpan<byte>, int) -> Span<byte>`              | instance | decompress into a minted buffer, capped     |
|  [13]   | `Decompressor.Unwrap(ReadOnlySpan<byte>, Span<byte>) -> int`              | instance | decompress into a caller buffer             |
|  [14]   | `Decompressor.TryUnwrap(ReadOnlySpan<byte>, Span<byte>, out int) -> bool` | instance | no-throw decompress                         |

[ENTRYPOINT_SCOPE]: context configuration, dictionaries, bounds

Both `LoadDictionary` legs take a `byte[]` or a span; every `DictBuilder` trainer takes `(IEnumerable<byte[]> samples, …, int dictCapacity)` defaulted to `DefaultDictCapacity`.

| [INDEX] | [SURFACE]                                                                         | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :-------------------------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `Compressor.SetParameter(ZSTD_cParameter, int)`                                   | instance | set one compress knob                   |
|  [02]   | `Compressor.GetParameter(ZSTD_cParameter) -> int`                                 | instance | read one compress knob back             |
|  [03]   | `Decompressor.SetParameter(ZSTD_dParameter, int)`                                 | instance | bound the decode window                 |
|  [04]   | `Decompressor.GetParameter(ZSTD_dParameter) -> int`                               | instance | read the decode knob back               |
|  [05]   | `Compressor.LoadDictionary(ReadOnlySpan<byte>)`                                   | instance | install a trained dictionary on a cctx  |
|  [06]   | `Decompressor.LoadDictionary(ReadOnlySpan<byte>)`                                 | instance | install the same dictionary on a dctx   |
|  [07]   | `DictBuilder.TrainFromBuffer(…) -> byte[]`                                        | static   | train from sample blobs                 |
|  [08]   | `DictBuilder.TrainFromBufferFastCover(…, int level) -> Span<byte>`                | static   | FastCover training at a level           |
|  [09]   | `DictBuilder.TrainFromBufferFastCover(…, ZDICT_fastCover_params_t) -> Span<byte>` | static   | FastCover training under explicit knobs |
|  [10]   | `Methods.ZSTD_cParam_getBounds(ZSTD_cParameter) -> ZSTD_bounds`                   | static   | admissible range for a compress knob    |
|  [11]   | `Methods.ZSTD_dParam_getBounds(ZSTD_dParameter) -> ZSTD_bounds`                   | static   | admissible range for the decode knob    |
|  [12]   | `Methods.ZSTD_getDictID_fromFrame(void*, nuint) -> uint`                          | static   | dictionary id a frame was built against |
|  [13]   | `Methods.ZSTD_findDecompressedSize(void*, nuint) -> ulong`                        | static   | decoded size across every frame         |

[ENTRYPOINT_SCOPE]: incremental pump

Every pump member takes `(ReadOnlySpan<byte> source, Span<byte> destination, out int bytesConsumed, out int bytesWritten)` and returns `OperationStatus`.

| [INDEX] | [SURFACE]                                           | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :-------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `Compressor.WrapStream(…, bool isFinalBlock)`       | instance | compress one chunk; the flag closes a frame |
|  [02]   | `Compressor.FlushStream(Span<byte>, out int)`       | instance | drain pending output                        |
|  [03]   | `Compressor.FlushStream(Span<byte>, out int, bool)` | instance | drain, closing the frame on the flag        |
|  [04]   | `Decompressor.UnwrapStream(…)`                      | instance | decompress one chunk                        |
|  [05]   | `Compressor.SetPledgedSrcSize(ulong)`               | instance | declare frame length before the first chunk |
|  [06]   | `Compressor.ResetStream()`                          | instance | clear cctx session state between frames     |
|  [07]   | `Decompressor.ResetStream()`                        | instance | clear dctx session state between frames     |

[ENTRYPOINT_SCOPE]: `Stream` adapters

Every blocking member carries an async twin over `Memory<byte>` with a `CancellationToken`. Ctor defaults are `level = 3`, `bufferSize = 0`, `leaveOpen = true`, `checkEndOfStream = true`, and `preserveCompressor`/`preserveDecompressor = true`, so an adapter neither closes the caller's stream nor disposes a shared context.

| [INDEX] | [SURFACE]                                                                          | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :--------------------------------------------------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `CompressionStream(Stream, int, int, bool)`                                        | ctor     | wrap a sink at a level              |
|  [02]   | `CompressionStream(Stream, Compressor, int, bool, bool)`                           | ctor     | wrap a sink over a tuned context    |
|  [03]   | `CompressionStream.Write(ReadOnlySpan<byte>)`                                      | instance | blocking block write                |
|  [04]   | `CompressionStream.WriteAsync(ReadOnlyMemory<byte>, CancellationToken)`            | instance | async block write                   |
|  [05]   | `CompressionStream.DisposeAsync() -> ValueTask`                                    | instance | async close, emitting a final frame |
|  [06]   | `CompressionStream.SetPledgedSrcSize(ulong)`                                       | instance | pledge frame length on the adapter  |
|  [07]   | `DecompressionStream(Stream, int, bool, bool)`                                     | ctor     | wrap a source                       |
|  [08]   | `DecompressionStream(Stream, Decompressor, int, bool, bool, bool)`                 | ctor     | wrap a source over a tuned context  |
|  [09]   | `DecompressionStream.Read(Span<byte>) -> int`                                      | instance | blocking block read                 |
|  [10]   | `DecompressionStream.ReadAsync(Memory<byte>, CancellationToken) -> ValueTask<int>` | instance | async block read                    |

- `CompressionStream`/`DecompressionStream`: each adapter re-exposes `SetParameter`, `GetParameter`, and `LoadDictionary` against its own context.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Compressor` and `Decompressor` hold a context across many calls — level, parameters, and dictionary set once, `Wrap`/`Unwrap` many, disposal at the owning scope — and a parallel fan gives each worker its own context.
- `ZSTD_c_contentSizeFlag` writes the decoded size into the frame header and `GetDecompressedSize` reads it back, so a framed payload round-trips with no sidecar length.
- `ZSTD_c_checksumFlag` seals a frame-integrity word inside the frame while the content address folds the stored bytes separately, so the two facts survive a codec change independently.
- `Wrap` mints its own buffer where the two-span form writes into a `GetCompressBound`-sized destination, and `TryWrap`/`TryUnwrap` report a short destination as `false` instead of a throw.
- `OperationStatus` discriminates every pump step: `NeedMoreData` asks for input, `DestinationTooSmall` for room, `Done` closes the step.

[STACKING]:
- `api-lz4`(`.api/api-lz4.md`): `LZ4Pickler` owns the low-latency self-describing frame and this surface the high-ratio one, so `CompressionPolicy` selects exactly one per payload.
- `api-arrow`(`.api/api-arrow.md`): `Apache.Arrow.Compression.CompressionCodecFactory` compresses `Zstd` inside the Arrow IPC stream, so a batch arrives framed and its policy row adds no outer frame.
- `api-messagepack`(`.api/api-messagepack.md`): `MessagePackCompression.Lz4BlockArray` frames inside the serializer, so that codec pairs with the uncompressed policy row.
- `api-hashing`(`../../.api/api-hashing.md`): `Crc32.HashToUInt32` seals the snapshot header prefix and `XxHash3.HashToUInt64` tags each chunk over bytes this codec produced, keeping frame checksum and content address distinct.
- `api-highperformance`(`../../.api/api-highperformance.md`): `ArrayPoolBufferWriter<byte>.GetSpan` rents the pump's destination and `Advance` commits `bytesWritten`, so a streamed frame costs one pooled rental.
- `Element/codec#COMPRESSION_HASHING`: `ZstdFrame.Pack` sets `contentSizeFlag` and `checksumFlag` on every frame, adds `enableLongDistanceMatching` with `ZSTD_btultra2` on the archival row, and `PackStream`/`UnpackStream` drive the adapters as the one streaming residence on the compression axis.

[LOCAL_ADMISSION]:
- `CompressionPolicy.Zstd` and `ZstdHigh` are the admitted rows, each a level with its archival flag; a further profile is one more row against the same frame helper.
- A payload past one contiguous buffer rides `PackStream`/`UnpackStream`, and a one-shot destination stays bounded by `GetCompressBound`.
- `ZstdException` maps to a typed `Fin` failure at the codec boundary, so the no-throw twins keep a short destination on the value rail.
- Level, frame flags, dictionary id, and pledged size project as receipt data on the policy row.

[RAIL_LAW]:
- Package: `ZstdSharp.Port`
- Owns: Zstandard compression for snapshot and blob payloads — one-shot span codec, the `OperationStatus` pump, async-mirrored `Stream` adapters, the advanced parameter surface, and trained dictionaries
- Accept: `CompressionPolicy` rows configured through `SetParameter`; `GetCompressBound`-sized one-shot destinations; the pump or the adapters for large payloads; `LoadDictionary` for the small-similar-blob regime; `TryWrap`/`TryUnwrap` folded into `Fin`
- Reject: a second frame over a body its serializer or IPC stream already compressed; a sidecar decoded length where `contentSizeFlag` self-describes the frame; one context shared across parallel workers; a thrown `ZstdException` crossing the receipt boundary; a `ZSTD_c_experimentalParam*` value in a policy row
