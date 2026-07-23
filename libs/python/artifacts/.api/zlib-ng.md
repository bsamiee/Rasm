# [PY_ARTIFACTS_API_ZLIB_NG]

`zlib-ng` binds the SIMD-accelerated zlib-ng C core as a stdlib `zlib`/`gzip` drop-in, owning the DEFLATE/gzip-container arm of the artifacts compression rail across three import surfaces: the `zlib_ng.zlib_ng` `zlib`-API mirror, the `gzip_ng` container module, and the `gzip_ng_threaded` GIL-escaping multi-threaded reader/writer. `package/codec`'s `GZIP` band binds it when bytes must round-trip with stdlib `zlib`/`gzip` (RFC 1950/1951/1952) at accelerated throughput; it re-implements neither the DEFLATE codec nor gzip framing nor the CRC/Adler checksums the native core owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `zlib-ng`
- package: `zlib-ng` (`PSF-2.0`, mirrors the CPython `zlib`/`gzip` modules)
- import: `from zlib_ng import zlib_ng` / `gzip_ng` / `gzip_ng_threaded`
- owner: `artifacts`
- rail: compression
- asset: native wheel binding the bundled zlib-ng C core, GIL-releasing; `zlib_ng.ZLIBNG_RUNTIME_VERSION` and the emulated-zlib-API `ZLIB_RUNTIME_VERSION` are STRING attributes (the `.pyi` `int` annotation is wrong)
- entry points: `python -m zlib_ng.gzip_ng` (`gzip_ng.main`) mirrors the stdlib `gzip` command; no console entry point
- capability: SIMD-accelerated DEFLATE/zlib/gzip one-shot, streaming, and file-like compression/decompression, the `_ZlibDecompressor` bytes-feeding decode, Adler-32/CRC-32 checksums with `crc32_combine` block-trailer recombination, and GIL-escaping multi-threaded gzip streaming

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `zlib_ng.zlib_ng` codec carriers and checksums — minted by `compressobj`/`decompressobj`/`_ZlibDecompressor(...)` and bound by the minting factory, never imported by name.

| [INDEX] | [SYMBOL]            | [PACKAGE_ROLE]             | [CAPABILITY]                                                   |
| :-----: | :------------------ | :------------------------- | :------------------------------------------------------------- |
|  [01]   | `_Compress`         | incremental compressor     | minted by `compressobj`; the `compress`/`flush` pair           |
|  [02]   | `_Decompress`       | incremental decompressor   | minted by `decompressobj`; `decompress`/`flush` + state props  |
|  [03]   | `_ZlibDecompressor` | bytes-feeding decompressor | bytes-feeding decoder behind `_GzipReader`                     |
|  [04]   | `_ParallelCompress` | block codec                | per-thread block engine `gzip_ng_threaded` fans across workers |
|  [05]   | `_GzipReader`       | gzip stream reader         | `RawIOBase` gzip decode source behind `gzip_ng.decompress`     |
|  [06]   | `zlib_ng.error`     | codec fault                | the `zlib.error` mirror, lifted to `expression.Result`         |

- `_Decompress`: `unused_data`/`unconsumed_tail`/`eof` carry trailing bytes, parked input, and end state; `_ZlibDecompressor` carries `unused_data`/`needs_input`/`eof`.

[PUBLIC_TYPE_SCOPE]: `gzip_ng`/`gzip_ng_threaded` container types — `GzipNGFile` mirrors stdlib `GzipFile` (binary `BufferedIOBase` over a gzip container, `mtime=0` for reproducible framing); the threaded writer fans block compression across worker threads and recombines per-block CRCs with `crc32_combine` into the single trailer.

| [INDEX] | [SYMBOL]                  | [PACKAGE_ROLE]       | [CAPABILITY]                                                |
| :-----: | :------------------------ | :------------------- | :---------------------------------------------------------- |
|  [01]   | `gzip_ng.GzipNGFile`      | gzip file codec      | binary `BufferedIOBase` gzip file; `mtime=0` reproducible   |
|  [02]   | `gzip_ng_threaded` writer | threaded gzip writer | fans block compress across threads; `crc32_combine` trailer |
|  [03]   | `gzip_ng_threaded` reader | threaded gzip reader | GIL-escaping multi-threaded gzip decode reader              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `zlib_ng.zlib_ng` one-shot, streaming, and checksum over the `zlib`-API mirror — `wbits` selects the container: `MAX_WBITS` zlib, `MAX_WBITS | 16` gzip, `-MAX_WBITS` raw DEFLATE.

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                                               | [CAPABILITY]                               |
| :-----: | :---------------------- | :--------------------------------------------------------- | :----------------------------------------- |
|  [01]   | `zlib_ng.compress`      | `compress(data, /, level=-1, wbits=MAX_WBITS) -> bytes`    | one-shot zlib/gzip/raw-DEFLATE compress    |
|  [02]   | `zlib_ng.decompress`    | `decompress(data, /, wbits=MAX_WBITS, bufsize=) -> bytes`  | one-shot decompress                        |
|  [03]   | `zlib_ng.compressobj`   | `compressobj(level=-1, method=, wbits=, …) -> _Compress`   | streaming compressor with a shared `zdict` |
|  [04]   | `zlib_ng.decompressobj` | `decompressobj(wbits=MAX_WBITS, zdict=b'') -> _Decompress` | streaming decompressor                     |
|  [05]   | `zlib_ng.crc32`         | `crc32(data, value=0, /) -> int`                           | CRC-32 checksum, resumable via `value`     |
|  [06]   | `zlib_ng.crc32_combine` | `crc32_combine(crc1, crc2, len2) -> int`                   | combine two CRC-32s -> concat checksum     |
|  [07]   | `zlib_ng.adler32`       | `adler32(data, value=1, /) -> int`                         | Adler-32 checksum (zlib trailer)           |

[ENTRYPOINT_SCOPE]: `gzip_ng`/`gzip_ng_threaded` container one-shot and file — `gzip_ng.open` is the file-like endpoint (text modes wrap `GzipNGFile` in a `TextIOWrapper`); the threaded `open` is the multi-threaded block-fan reader/writer with keyword-only `threads`/`block_size`.

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                                              | [CAPABILITY]                                       |
| :-----: | :---------------------- | :-------------------------------------------------------- | :------------------------------------------------- |
|  [01]   | `gzip_ng.compress`      | `compress(data, compresslevel=9, *, mtime=None) -> bytes` | one-shot gzip compress (`mtime=0` reproducible)    |
|  [02]   | `gzip_ng.decompress`    | `decompress(data) -> bytes`                               | one-shot gzip decompress (multi-member aware)      |
|  [03]   | `gzip_ng.open`          | `open(filename, mode='rb', compresslevel=-1, …)`          | file-like gzip endpoint; text-mode `TextIOWrapper` |
|  [04]   | `gzip_ng_threaded.open` | `open(filename, mode='rb', *, threads=1, block_size=…)`   | multi-threaded block-fan gzip reader/writer        |

- `gzip_ng_threaded.open`: `threads<0` resolves to the CPU count and `threads=0` defers to `gzip_ng.open`, so an all-cores profile sentinel is `-1`, never a hand-counted worker number.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- import: `from zlib_ng import zlib_ng` / `gzip_ng` / `gzip_ng_threaded` at boundary scope only; module-level import is banned by the manifest import policy.
- container axis: `wbits` selects the wire format on the `zlib` surface — `MAX_WBITS` zlib, `MAX_WBITS | 16` gzip, `-MAX_WBITS` raw DEFLATE; `gzip_ng` presets the gzip framing. Container selection is a `wbits`/module choice, never a parallel codec owner.
- modality axis: one-shot (`compress`/`decompress`), streaming (`compressobj`/`decompressobj` with a shared `zdict`), file-like (`gzip_ng.open`/`GzipNGFile`), and multi-threaded block-fan (`gzip_ng_threaded.open`) are rows on one package. `gzip_ng_threaded`'s writer fans `_ParallelCompress.compress_and_crc(data, zdict)` — one raw-DEFLATE block and its CRC-32 per GIL-releasing call — across workers, then recombines the per-block CRCs into the single trailer via `crc32_combine`.
- checksum axis: `crc32(data, value=)`/`adler32(data, value=)` resume from a prior value; `crc32_combine(crc1, crc2, len2)` is the associativity lever making block-parallel gzip framing correct by composing the ordered `(crc, len)` pairs rather than re-scanning the concatenated payload. gzip's trailer is CRC-32 + ISIZE, zlib's is Adler-32.
- reproducibility axis: `mtime=0` (`gzip_ng.compress(..., mtime=0)` / `GzipNGFile(..., mtime=0)`) keeps a content-addressed bundle hashing identically across runs; a wall-clock mtime crossing into the artifact content key is the defect it forecloses.

[STACKING]:
- `package/codec#CODEC`'s `GZIP` band binds `gzip_ng`/`gzip_ng_threaded` as one `CodecProfile.gzip` `(compresslevel, threads, block_size)` row on the `package/bundle#BUNDLE` `CompressionAlgo` union: single-thread calls `gzip_ng.compress(payload, compresslevel=k, mtime=0)`, parallel drives `gzip_ng_threaded.open(sink, 'wb', ...)` whose writer recombines the trailer via `crc32_combine`.
- runtime seam — the codec body rides `lane.offload(Kernel.of(kernel, KernelTrait.RELEASING), ...)` because the native core releases the GIL: a bounded worker crossing, never an event-loop call, never a folder-minted `CapacityLimiter`; a `zlib_ng.error` folds to the `expression.Result[bytes, ArtifactError]`/`RuntimeRail` fault at the boundary.
- evidence seam — each call contributes its compression fact (container, level, thread/block params, `crc32`/`adler32` trailer, `ZLIBNG_RUNTIME_VERSION`, byte lengths) through the runtime `ReceiptContributor` port onto the one `ArtifactReceipt.Bundle` case beside the `zstandard`/`lz4`/`brotli` arms, mirrored on a `structlog`/`opentelemetry` span.
- `package/archive`'s reproducible-ZIP member turns content-addressable under `mtime=0` framing, so a scene-file or document payload hashes identically for the runtime elision seed (`ArtifactWork.key` over input bytes).

[LOCAL_ADMISSION]:
- zlib-ng owns the DEFLATE/zlib/gzip path (`package/codec` `GZIP` band; `package/bundle` `CompressionAlgo` gzip arm); high-ratio archival routes to `zstandard`, hot-path block codecs to `lz4`, web/WOFF2 transport to `brotli`, archive containers to `py7zr`/`stream-zip`.
- package-plane owners alone consume compression: a scene bundle is `package/archive`'s emit over scene-file content keys, never an import; recovery is bomb-bounded, identity minting the runtime owns, live UI stays outside this package.

[RAIL_LAW]:
- Package: `zlib-ng`
- Owns: SIMD-accelerated DEFLATE/zlib/gzip compression and decompression (one-shot, streaming, file-like, and multi-threaded block-fan), Adler-32/CRC-32 checksums with `crc32_combine` block-trailer recombination, and the stdlib-`zlib`/`gzip` drop-in surface
- Accept: the gzip-container/DEFLATE codec service feeding the `package/codec#CODEC` `GZIP` band via a `CodecProfile.gzip` row, running on the `lane.offload(Kernel.of(..., KernelTrait.RELEASING))` thread seam, lifted onto the `expression.Result` rail with an `ArtifactReceipt.Bundle` receipt
- Reject: a container modeled as a parallel codec owner where `wbits`/module choice suffices; a per-block whole-stream re-checksum where `crc32_combine` composes the ordered `(crc, len)` pairs; a wall-clock mtime crossing into a content-addressed bundle; a folder-minted `CapacityLimiter` where the GIL-releasing core rides `lane.offload`; a `scene/export` repro-ZIP clone where the package plane owns bundling; a parallel zlib-ng-only receipt where `ArtifactReceipt.Bundle` owns the case
