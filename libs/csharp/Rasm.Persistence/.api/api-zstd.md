# [RASM_PERSISTENCE_API_ZSTD]

`ZstdSharp.Port` is a pure-managed C#-to-C# port of libzstd: the `Compressor`/`Decompressor`
one-shot codec over `ReadOnlySpan<byte>`/`byte[]`/`ArraySegment<byte>` (with `TryWrap`/
`TryUnwrap` no-throw twins and `GetCompressBound` sizing), the `WrapStream`/`UnwrapStream`/
`FlushStream` incremental engine returning `System.Buffers.OperationStatus`, the
`CompressionStream`/`DecompressionStream` `Stream` adapters with a full async mirror
(`WriteAsync`/`ReadAsync`/`FlushAsync`/`DisposeAsync` over `Memory<byte>`), the advanced
`ZSTD_cParameter`/`ZSTD_dParameter` tuning surface (long-distance matching, multithreading,
strategy, window/hash/chain logs, content-size/checksum/dictID frame flags), trained-
dictionary support (`LoadDictionary` + `DictBuilder.TrainFromBuffer`/`TrainFromBufferFastCover`),
and `SetPledgedSrcSize` for known-length single-frame embedding. It is the first-class
standalone Zstandard snapshot/blob codec sitting beside `K4os.Compression.LZ4`'s
self-describing-frame codec on the `CompressionPolicy` axis; distinct from the in-codec compression that `MessagePackCompression.Lz4BlockArray` and
the Arrow `ICompressionCodecFactory` own, never double-framing them.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ZstdSharp.Port`
- package: `ZstdSharp.Port`
- license: MIT (Oleg Stepanischev) — `github.com/oleg-st/ZstdSharp`
- assembly: `ZstdSharp` (note: assembly name differs from the package id `ZstdSharp.Port`)
- namespace: `ZstdSharp` (public codec), `ZstdSharp.Unsafe` (libzstd-mirroring parameter/strategy/error enums + raw `Methods.ZSTD_*` bindings)
- target: multi-target (`net462`, `netcoreapp3.1`, `net5.0`..`net9.0`, `netstandard2.0`, `netstandard2.1`); the `net10.0` consumer binds `lib/net9.0` (highest available)
- asset: pure-managed runtime library, AnyCPU, NO native runtime — libzstd transpiled to C#; the `net9.0` asset has zero package dependencies (`System.Memory`/`...Unsafe` polyfills only on the legacy floors)
- abi: `Span<byte>`/`ReadOnlySpan<byte>`/`Memory<byte>` + `System.Buffers.OperationStatus`; `Compressor`/`Decompressor` are `IDisposable` and hold unmanaged-style cctx/dctx wrappers
- rail: snapshot-codec

## [02]-[PUBLIC_TYPES]

[CODEC_TYPES]: codec roots + stream adapters + dictionary builder (namespace `ZstdSharp`)
- rail: snapshot-codec

`Compressor`/`Decompressor` own a reusable cctx/dctx (set parameters once, `Wrap`/`Unwrap`
many) and MUST be disposed. The `Stream` adapters wrap an inner `Stream` with a `leaveOpen`
and a `preserveCompressor` knob and supply the async mirror.

| [INDEX] | [SYMBOL]              | [PACKAGE_ROLE]        | [CAPABILITY]                                                                  |
| :-----: | :-------------------- | :-------------------- | :---------------------------------------------------------------------------- |
|  [01]   | `Compressor`          | one-shot + streaming compressor | `IDisposable`; `Level`, `SetParameter`/`GetParameter`, `LoadDictionary`, `Wrap`/`TryWrap`, `WrapStream`/`FlushStream`, `SetPledgedSrcSize` |
|  [02]   | `Decompressor`        | one-shot + streaming decompressor | `IDisposable`; `SetParameter`/`GetParameter`, `LoadDictionary`, `GetDecompressedSize`, `Unwrap`/`TryUnwrap`, `UnwrapStream` |
|  [03]   | `CompressionStream`   | write-side `Stream`   | `: Stream` (write-only); `Write`/`WriteAsync`, `Flush`/`FlushAsync`, `DisposeAsync`, `SetParameter`, `LoadDictionary`, `SetPledgedSrcSize` |
|  [04]   | `DecompressionStream` | read-side `Stream`    | `: Stream` (read-only); `Read`/`ReadAsync`, `checkEndOfStream` guard, `SetParameter`, `LoadDictionary` |
|  [05]   | `DictBuilder`         | dictionary trainer    | static; `TrainFromBuffer`, `TrainFromBufferFastCover` (level / `ZDICT_fastCover_params_t`) |
|  [06]   | `ZstdException`       | typed codec fault     | `: Exception`; carries `ZSTD_ErrorCode Code` — the boundary the rail folds into `Fin` |

[PARAMETER_ENUMS]: libzstd advanced-API mirrors (namespace `ZstdSharp.Unsafe`)
- rail: snapshot-codec

The full advanced tuning surface — not just a level int. `SetParameter(ZSTD_cParameter, int)`
is the canonical knob; the unstable params (`>=500`) are excluded.

| [INDEX] | [SYMBOL]          | [PACKAGE_ROLE]      | [CAPABILITY]                                                                       |
| :-----: | :---------------- | :------------------ | :--------------------------------------------------------------------------------- |
|  [01]   | `ZSTD_cParameter` | compress parameters | `ZSTD_c_compressionLevel`, `windowLog`/`hashLog`/`chainLog`/`searchLog`/`minMatch`/`targetLength`, `strategy`, `enableLongDistanceMatching`+`ldm*`, `contentSizeFlag`/`checksumFlag`/`dictIDFlag`, `nbWorkers`/`jobSize`/`overlapLog`, `targetCBlockSize` |
|  [02]   | `ZSTD_dParameter` | decompress parameter| `ZSTD_d_windowLogMax` (the lone stable decode knob)                                |
|  [03]   | `ZSTD_strategy`   | match strategy      | `ZSTD_fast`/`dfast`/`greedy`/`lazy`/`lazy2`/`btlazy2`/`btopt`/`btultra`/`btultra2`  |
|  [04]   | `ZSTD_EndDirective` | flush directive   | `ZSTD_e_continue`/`ZSTD_e_flush`/`ZSTD_e_end` — the libzstd directive the engine maps INTERNALLY from `WrapStream`'s `bool isFinalBlock`; not a public `WrapStream` parameter |
|  [05]   | `ZSTD_ErrorCode`  | error code          | the libzstd error enum on `ZstdException.Code`                                      |
|  [06]   | `ZDICT_fastCover_params_t` | training params | struct controlling the FastCover dictionary trainer (`k`/`d`/`steps`/`nbThreads`/...) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: one-shot codec (`Compressor`/`Decompressor`)
- rail: snapshot-codec

`Wrap` returns a freshly-allocated `Span<byte>`; the span/array overloads write into a caller
buffer sized by `GetCompressBound`. `TryWrap`/`TryUnwrap` are the no-throw twins returning a
`bool` + `out int written` — the rail-composable form. `Decompressor.GetDecompressedSize`
reads the frame's stored content size when `contentSizeFlag` was set at compress time.

| [INDEX] | [SURFACE]                                                              | [CALL_SHAPE]   | [CAPABILITY]                                              |
| :-----: | :--------------------------------------------------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `new Compressor(int level = 3)`                                        | constructor    | reusable compress context (`DefaultCompressionLevel = 3`)|
|  [02]   | `Compressor.MinCompressionLevel` / `MaxCompressionLevel`              | static prop    | libzstd level bounds (negative fast levels .. 22)        |
|  [03]   | `Compressor.GetCompressBound(int)` / `GetCompressBoundLong(ulong)`    | static         | worst-case compressed size for destination allocation    |
|  [04]   | `Compressor.Wrap(ReadOnlySpan<byte> src)`                            | one-shot       | compress -> new `Span<byte>`                             |
|  [05]   | `Compressor.Wrap(ReadOnlySpan<byte> src, Span<byte> dest)`           | one-shot       | compress into caller buffer; returns written length      |
|  [06]   | `Compressor.TryWrap(ReadOnlySpan<byte> src, Span<byte> dest, out int written)` | one-shot | no-throw compress; `false` if `dest` too small      |
|  [07]   | `new Decompressor()` / `Decompressor.GetDecompressedSize(ReadOnlySpan<byte>)` | constructor/static | reusable decompress context / frame content size  |
|  [08]   | `Decompressor.Unwrap(ReadOnlySpan<byte> src, int maxDecompressedSize = int.MaxValue)` | one-shot | decompress -> new `Span<byte>`, capped              |
|  [09]   | `Decompressor.Unwrap(ReadOnlySpan<byte> src, Span<byte> dest)` / `TryUnwrap(…, out int written)` | one-shot | decompress into caller buffer / no-throw twin    |

[ENTRYPOINT_SCOPE]: advanced tuning + dictionaries + pledged size
- rail: snapshot-codec

The catalog-level codec is configured through `SetParameter`, not a bare level. Long-distance
matching and the multithread params are the large-snapshot knobs; the frame flags govern
whether the frame self-describes its size and carries a checksum.

| [INDEX] | [SURFACE]                                                              | [CALL_SHAPE]   | [CAPABILITY]                                              |
| :-----: | :--------------------------------------------------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `Compressor.SetParameter(ZSTD_cParameter.ZSTD_c_compressionLevel, n)` | tuning         | per-context level (mirrors the ctor level)               |
|  [02]   | `Compressor.SetParameter(ZSTD_c_enableLongDistanceMatching, 1)` + `ldm*` | tuning      | LDM for large redundant snapshots                        |
|  [03]   | `Compressor.SetParameter(ZSTD_c_nbWorkers, n)` / `jobSize` / `overlapLog` | tuning      | multithreaded compression of one frame                   |
|  [04]   | `Compressor.SetParameter(ZSTD_c_checksumFlag, 1)` / `contentSizeFlag` / `dictIDFlag` | tuning | frame integrity / self-describing size / dict id   |
|  [05]   | `Compressor.SetParameter(ZSTD_c_strategy, (int)ZSTD_strategy.ZSTD_btultra2)` | tuning  | match-finder strategy                                    |
|  [06]   | `Compressor.SetPledgedSrcSize(ulong)`                                | streaming setup| pledges total size before a streaming frame (enables single-frame size header) |
|  [07]   | `Compressor.LoadDictionary(ReadOnlySpan<byte>)` / `Decompressor.LoadDictionary(…)` | dictionary | install a trained dictionary on the context        |
|  [08]   | `DictBuilder.TrainFromBuffer(IEnumerable<byte[]> samples, int dictCapacity = 112640)` | training | train a dictionary from sample blobs                |
|  [09]   | `DictBuilder.TrainFromBufferFastCover(IEnumerable<byte[]>, int level \| ZDICT_fastCover_params_t, int dictCapacity)` | training | FastCover trainer (faster, tunable) |
|  [10]   | `Compressor.ResetStream()` / `Decompressor.ResetStream()`            | streaming setup| resets streaming session state between frames            |

[ENTRYPOINT_SCOPE]: incremental engine (`OperationStatus`-driven)
- rail: snapshot-codec

The buffer-pump core under the `Stream` adapters: `WrapStream`/`UnwrapStream`/`FlushStream`
consume from `source`, write to `destination`, report `out int bytesConsumed`/`bytesWritten`,
and return `System.Buffers.OperationStatus` — `Done`/`DestinationTooSmall`/`NeedMoreData` —
the exact shape a `PipeWriter`/`IBufferWriter<byte>` pump loops on. The public frame-boundary
control is the `bool isFinalBlock` flag on `WrapStream`/`FlushStream`, NOT a `ZSTD_EndDirective`
overload — the `continue`/`flush`/`end` directive is mapped internally from `isFinalBlock`.

| [INDEX] | [SURFACE]                                                              | [RETURNS]         | [CAPABILITY]                                          |
| :-----: | :--------------------------------------------------------------------- | :---------------- | :---------------------------------------------------- |
|  [01]   | `Compressor.WrapStream(ReadOnlySpan<byte> source, Span<byte> destination, out int bytesConsumed, out int bytesWritten, bool isFinalBlock)` | `OperationStatus` | compress one chunk; `isFinalBlock` closes the frame (the only directive selector) |
|  [02]   | `Compressor.FlushStream(Span<byte> destination, out int bytesWritten)` / `FlushStream(…, bool isFinalBlock)` | `OperationStatus` | drain buffered output / close the frame on `isFinalBlock` |
|  [03]   | `Decompressor.UnwrapStream(ReadOnlySpan<byte> source, Span<byte> destination, out int bytesConsumed, out int bytesWritten)` | `OperationStatus` | decompress one chunk incrementally          |
|  [04]   | `Compressor.ResetStream()` / `Decompressor.ResetStream()`            | `void`            | clears streaming session state between frames         |

[ENTRYPOINT_SCOPE]: `Stream` adapters + async mirror
- rail: snapshot-codec

Every blocking `Stream` member has an async twin over `Memory<byte>`/`ReadOnlyMemory<byte>`
with a `CancellationToken`. `CompressionStream` is write-only (`CanRead=false`),
`DecompressionStream` read-only; the second ctor of each accepts a pre-configured
`Compressor`/`Decompressor` so the parameter/dictionary setup is shared.

| [INDEX] | [SURFACE]                                                              | [CALL_SHAPE]      | [CAPABILITY]                                       |
| :-----: | :--------------------------------------------------------------------- | :---------------- | :------------------------------------------------- |
|  [01]   | `new CompressionStream(Stream stream, int level = 3, int bufferSize = 0, bool leaveOpen = true)` | constructor | wrap a sink for streaming compression           |
|  [02]   | `new CompressionStream(Stream, Compressor, int bufferSize = 0, bool preserveCompressor = true, bool leaveOpen = true)` | constructor | reuse a tuned `Compressor` |
|  [03]   | `CompressionStream.Write(ReadOnlySpan<byte>)` / `WriteAsync(ReadOnlyMemory<byte>, CancellationToken)` | stream call | sync / async block write              |
|  [04]   | `CompressionStream.FlushAsync` / `DisposeAsync`                       | stream call       | async flush / async close (emits final frame)      |
|  [05]   | `new DecompressionStream(Stream stream, int bufferSize = 0, bool checkEndOfStream = true, bool leaveOpen = true)` | constructor | wrap a source for streaming decompression  |
|  [06]   | `DecompressionStream.Read(Span<byte>)` / `ReadAsync(Memory<byte>, CancellationToken)` | stream call | sync / async block read                  |

## [04]-[IMPLEMENTATION_LAW]

[COMPRESSION_PROFILE]:
- one-shot: `Compressor.Wrap`/`Decompressor.Unwrap` over span/array/`ArraySegment`; the destination must be sized by `GetCompressBound`, and `TryWrap`/`TryUnwrap` are the no-throw twins. `Decompressor.GetDecompressedSize` reads the frame's content size only when `ZSTD_c_contentSizeFlag` was set at compress time.
- context reuse: `Compressor`/`Decompressor` hold a reusable cctx/dctx and are `IDisposable` — set parameters and the dictionary once, then `Wrap`/`Unwrap` many; `ResetStream` clears streaming state between frames. They are the owned-value form, never a per-call allocation.
- streaming: the `WrapStream`/`UnwrapStream`/`FlushStream` core returns `System.Buffers.OperationStatus` and pumps `source -> destination` with `out` consumed/written counts; the `bool isFinalBlock` flag on `WrapStream`/`FlushStream` is the public frame-boundary control (the engine maps it to the libzstd `ZSTD_EndDirective` `continue`/`flush`/`end` internally), and `SetPledgedSrcSize` lets a known-length streaming frame still carry a size header.
- tuning: the level is one knob among the `ZSTD_cParameter` surface — long-distance matching (`ZSTD_c_enableLongDistanceMatching` + `ldm*`), multithreading (`nbWorkers`/`jobSize`/`overlapLog`), `strategy`, the window/hash/chain/search logs, and the `contentSizeFlag`/`checksumFlag`/`dictIDFlag` frame flags are all set through `SetParameter`. Unstable params (`>=500`) are excluded.
- dictionaries: `DictBuilder.TrainFromBuffer`/`TrainFromBufferFastCover` train a dictionary from sample blobs; `LoadDictionary` installs it on both sides — the small-similar-blob regime where a shared dictionary dominates raw ratio.

[LOCAL_ADMISSION]:
- `CompressionPolicy.Zstd*` (`Element/codec#COMPRESSION_HASHING`) configures a `Compressor` through `SetParameter` (level + `contentSizeFlag` so the frame self-describes its decoded size + `checksumFlag` for a frame-embedded integrity tag), so the snapshot header stores only the `CompressionId`, never a sidecar length.
- payloads above the streaming threshold pump through `WrapStream`/`FlushStream` (or the `CompressionStream` adapter) sized to a fixed segment, so a large snapshot never materializes one contiguous compressed buffer; the one-shot `Wrap(src, dest)` destination is bounded by `GetCompressBound`.
- `Compressor`/`Decompressor` are owned values disposed at the codec-scope boundary; under parallel snapshot writes each worker holds its own context (the cctx is not thread-shared), threaded through the same Task/`anyio` fan the surrounding pipeline uses.
- `ZstdException` (`Code` = `ZSTD_ErrorCode`) is mapped to a typed `Fin`/`Validation` failure at the codec boundary; the no-throw `TryWrap`/`TryUnwrap` are preferred so a too-small destination is a `bool`, never a thrown exception through the receipt path.
- the trained-dictionary blob, level, frame flags, and pledged size are receipt data on the `CompressionPolicy` row; compression never obscures redaction, retention, or schema receipts, and the `checksumFlag` frame checksum complements (never replaces) the `Crc32`/`XxHash128` integrity tag the snapshot rail computes over the framed bytes.

[STACKING_LAW]:
- codec axis: Zstd sits beside `K4os.Compression.LZ4` (`api-lz4`) on the `CompressionPolicy` snapshot-codec axis — LZ4 owns the lowest-latency self-describing-frame path, Zstd owns the higher-ratio path with dictionary + LDM + multithread tuning; the policy row selects one, and a payload is framed exactly once.
- Arrow IPC boundary: `Apache.Arrow.Compression` (`api-arrow`) owns Arrow-IPC `Zstd` block compression through its `ICompressionCodecFactory` and floor-pins `ZstdSharp.Port` as a transitive — the Arrow columnar path compresses inside the IPC stream, so a Zstd-compressed Arrow batch is NEVER re-framed through this standalone `Compressor`; this owner is the snapshot/blob codec for the non-Arrow payloads (`SnapshotCodec.JsonStj`, `FileRaw`, CBOR/MessagePack blobs).
- MessagePack boundary: a `SnapshotCodec.MessagePackBinary` payload carrying `MessagePackCompression.Lz4BlockArray` owns its compression in-codec and pairs with `CompressionPolicy.None` — double-framing it through Zstd is the rejected form, identical to the LZ4 double-frame law.
- integrity: the framed bytes feed `System.IO.Hashing` (`api-hashing`) `XxHash128`/`Crc32` for the snapshot integrity tag independently of the codec's optional `checksumFlag`, so the integrity receipt is codec-agnostic and survives a codec change.

[RAIL_LAW]:
- Package: `ZstdSharp.Port`
- Owns: standalone Zstandard compression — one-shot span/array codec, the `OperationStatus` streaming engine, the `CompressionStream`/`DecompressionStream` async-mirrored `Stream` adapters, the advanced `ZSTD_cParameter` tuning surface (LDM / multithread / strategy / frame flags), and trained-dictionary support
- Accept: `CompressionPolicy.Zstd*` snapshot/blob payloads configured via `SetParameter`; `WrapStream`/`FlushStream` (or the stream adapter) for large payloads; `Wrap(src, dest)` into a `GetCompressBound`-sized buffer; `LoadDictionary` for the small-similar-blob regime; `TryWrap`/`TryUnwrap` no-throw twins folded into `Fin`
- Reject: re-framing a Zstd-compressed Arrow IPC batch (`Apache.Arrow.Compression` owns it in-stream); double-framing a MessagePack `Lz4BlockArray` payload; a thrown `ZstdException` crossing the receipt boundary in place of a typed `Fin` failure; a sidecar decoded-length where `contentSizeFlag` already self-describes the frame; sharing one `Compressor` context across parallel workers; using the unstable `ZSTD_c_experimentalParam*` parameter family
