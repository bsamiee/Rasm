# [PY_ARTIFACTS_API_ZLIB_NG]

`zlib-ng` (the `python-zlib-ng` project) binds the SIMD-accelerated zlib-ng C library as a drop-in for the stdlib `zlib`/`gzip` modules, supplying the DEFLATE/gzip-container arm of the artifacts compression rail beside `zstandard` (archival), `lz4` (hot-path block), and `brotli` (`Content-Encoding: br` / WOFF2). It exposes three import surfaces: the `zlib_ng.zlib_ng` C extension (a byte-for-byte mirror of the stdlib `zlib` API plus the modern `_ZlibDecompressor` and the threading-internal `_ParallelCompress` block codec), the `gzip_ng` module (a stdlib-`gzip` drop-in), and `gzip_ng_threaded` (a GIL-escaping multi-threaded gzip reader/writer that fans block compression across workers). The `package/codec#CODEC` `GZIP` band binds `gzip_ng`/`gzip_ng_threaded`, whose threaded writer recombines its per-block CRCs into the single gzip trailer through the `zlib_ng.zlib_ng` `crc32_combine` internally — the live re-entry consumer that met the `package/`-plane condition; it is chosen exactly when the bytes must round-trip with stdlib `zlib`/`gzip` (RFC 1950/1951/1952) at accelerated throughput, an `algo` discriminant on the `package/bundle#BUNDLE` `CompressionAlgo` union, never a parallel codec owner. It never re-implements the DEFLATE codec, gzip framing, or the CRC/Adler checksums the native core already owns. This is the artifacts-owned catalog for the folder's `package/{codec,bundle}` consumers.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `zlib-ng`
- package: `zlib-ng`
- import: `from zlib_ng import zlib_ng` / `from zlib_ng import gzip_ng` / `from zlib_ng import gzip_ng_threaded`
- owner: `artifacts`
- rail: compression
- installed: `1.0.0`
- license: `PSF-2.0` (mirrors and accelerates the CPython `zlib`/`gzip` modules under the Python Software Foundation License v2 — permissive, GIL-releasing native core)
- asset: native wheel binding the bundled zlib-ng `2.2.5` C core (`zlib_ng.ZLIBNG_RUNTIME_VERSION`); the emulated zlib API level is `ZLIB_RUNTIME_VERSION` (`'1.2.12'`) — both are STRING attributes (the `.pyi` `int` annotation is wrong)
- entry points: `python -m zlib_ng.gzip_ng` is a CLI (`gzip_ng.main`) mirroring the stdlib `gzip` command; no `[project.scripts]` console entry point
- capability: SIMD-accelerated DEFLATE/zlib/gzip — one-shot and streaming `zlib`-API compression/decompression, the modern `_ZlibDecompressor` bytes-feeding decode, Adler-32/CRC-32 checksums with `crc32_combine` block-trailer recombination, a stdlib-`gzip` drop-in (`GzipNGFile`/`open`/`compress`/`decompress`), and GIL-escaping multi-threaded gzip streaming (`gzip_ng_threaded`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `zlib_ng.zlib_ng` codec carriers and checksums
- rail: compression — namespace `zlib_ng.zlib_ng`. The codec objects are underscore-prefixed runtime classes minted by the factory functions (`compressobj`/`decompressobj`/`_ZlibDecompressor(...)`), never imported or constructed by name; bind them by the factory that mints them. `_Compress` mirrors zlib with `compress(data) -> bytes` + `flush(mode=Z_FINISH) -> bytes`; `_Decompress` adds `decompress(data, max_length=0) -> bytes` + `flush(length=DEF_BUF_SIZE)` with `unused_data`/`unconsumed_tail`/`eof` (trailing bytes, parked input, end state); `_ZlibDecompressor(wbits=MAX_WBITS, zdict=None)` is the modern `decompress(data, max_length=-1) -> bytes` decoder with `unused_data`/`needs_input`/`eof`; `_ParallelCompress(buffersize, level).compress_and_crc(data, zdict) -> (bytes, int)` returns one raw-DEFLATE block plus its CRC-32 in one GIL-releasing call; `_GzipReader(fp, buffersize=32768)` is a `RawIOBase` source (`readinto`/`read`/`readall`/`seek`/`tell`) wrapped in a `BufferedReader` by `GzipNGFile`; `zlib_ng.error` mirrors `zlib.error`.

| [INDEX] | [SYMBOL]            | [PACKAGE_ROLE]             | [CAPABILITY]                                                   |
| :-----: | :------------------ | :------------------------- | :------------------------------------------------------------- |
|  [01]   | `_Compress`         | incremental compressor     | minted by `compressobj`; the `compress`/`flush` pair           |
|  [02]   | `_Decompress`       | incremental decompressor   | minted by `decompressobj`; `decompress`/`flush` + state props  |
|  [03]   | `_ZlibDecompressor` | bytes-feeding decompressor | modern bytes-feeding decoder behind `_GzipReader`              |
|  [04]   | `_ParallelCompress` | block codec                | per-thread block engine `gzip_ng_threaded` fans across workers |
|  [05]   | `_GzipReader`       | gzip stream reader         | gzip decode source behind `gzip_ng.decompress`                 |
|  [06]   | `zlib_ng.error`     | codec fault                | the `zlib.error` mirror, lifted to `expression.Result`         |

[PUBLIC_TYPE_SCOPE]: `gzip_ng` container types
- rail: compression — namespace `zlib_ng.gzip_ng` (stdlib `gzip` drop-in) and `zlib_ng.gzip_ng_threaded`. `GzipNGFile(filename=None, mode=None, compresslevel=9, fileobj=None, mtime=None)` is the stdlib `GzipFile` mirror (binary `BufferedIOBase` over a gzip container, `mtime=0` for reproducible framing); `gzip_ng_threaded.open(filename, mode='wb'|'rb', compresslevel=..., threads=1, block_size=...)` returns the writer that fans block compression (`_ParallelCompress.compress_and_crc`) across worker threads and recombines per-block CRCs with `crc32_combine` into the single gzip trailer, or the GIL-escaping multi-threaded decode reader.

| [INDEX] | [SYMBOL]                  | [PACKAGE_ROLE]       | [CAPABILITY]                                                |
| :-----: | :------------------------ | :------------------- | :---------------------------------------------------------- |
|  [01]   | `gzip_ng.GzipNGFile`      | gzip file codec      | binary `BufferedIOBase` gzip file; `mtime=0` reproducible   |
|  [02]   | `gzip_ng_threaded` writer | threaded gzip writer | fans block compress across threads; `crc32_combine` trailer |
|  [03]   | `gzip_ng_threaded` reader | threaded gzip reader | GIL-escaping multi-threaded gzip decode reader              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `zlib_ng.zlib_ng` one-shot, streaming, and checksum
- rail: compression — the `zlib` API mirror; `wbits` selects the container (`MAX_WBITS`=zlib, `MAX_WBITS|16`=gzip, `-MAX_WBITS`=raw DEFLATE). The one-shot pair is `compress(data, level=-1, wbits=MAX_WBITS) -> bytes` / `decompress(data, wbits=MAX_WBITS, bufsize=DEF_BUF_SIZE) -> bytes`, and the streaming factories are `compressobj(level=-1, method=DEFLATED, wbits=MAX_WBITS, memLevel=DEF_MEM_LEVEL, strategy=Z_DEFAULT_STRATEGY, zdict=None) -> _Compress` and `decompressobj(wbits=MAX_WBITS, zdict=None) -> _Decompress`.

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                                                | [CAPABILITY]                               |
| :-----: | :---------------------- | :---------------------------------------------------------- | :----------------------------------------- |
|  [01]   | `zlib_ng.compress`      | `compress(data, level=-1, wbits=MAX_WBITS) -> bytes`        | one-shot zlib/gzip/raw-DEFLATE compress    |
|  [02]   | `zlib_ng.decompress`    | `decompress(data, wbits=MAX_WBITS) -> bytes`                | one-shot decompress                        |
|  [03]   | `zlib_ng.compressobj`   | `compressobj(level=-1, …) -> _Compress`                     | streaming compressor with a shared `zdict` |
|  [04]   | `zlib_ng.decompressobj` | `decompressobj(wbits=MAX_WBITS, zdict=None) -> _Decompress` | streaming decompressor                     |
|  [05]   | `zlib_ng.crc32`         | `crc32(data, value=0) -> int`                               | CRC-32 checksum, resumable via `value`     |
|  [06]   | `zlib_ng.crc32_combine` | `crc32_combine(crc1, crc2, len2) -> int`                    | combine two CRC-32s -> concat checksum     |
|  [07]   | `zlib_ng.adler32`       | `adler32(data, value=1) -> int`                             | Adler-32 checksum (zlib trailer)           |

[ENTRYPOINT_SCOPE]: `gzip_ng` container one-shot and file
- rail: compression — `zlib_ng.gzip_ng` / `zlib_ng.gzip_ng_threaded`. `gzip_ng.open(filename, mode='rb', compresslevel=9, encoding=None, errors=None, newline=None)` is the file-like endpoint (text modes wrap `GzipNGFile` in a `TextIOWrapper`); `gzip_ng_threaded.open(filename, mode='rb', compresslevel=9, threads=1, block_size=...)` is the multi-threaded block-fan reader/writer.

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                                              | [CAPABILITY]                                       |
| :-----: | :---------------------- | :-------------------------------------------------------- | :------------------------------------------------- |
|  [01]   | `gzip_ng.compress`      | `compress(data, compresslevel=9, *, mtime=None) -> bytes` | one-shot gzip compress (`mtime=0` reproducible)    |
|  [02]   | `gzip_ng.decompress`    | `decompress(data) -> bytes`                               | one-shot gzip decompress (multi-member aware)      |
|  [03]   | `gzip_ng.open`          | `open(filename, mode='rb', …)`                            | file-like gzip endpoint; text-mode `TextIOWrapper` |
|  [04]   | `gzip_ng_threaded.open` | `gzip_ng_threaded.open(filename, …)`                      | multi-threaded block-fan gzip reader/writer        |

`gzip_ng_threaded.open` resolves `threads<0` to the machine's CPU count and defers `threads=0` to plain `gzip_ng.open` (live-verified), so a profile's all-cores sentinel is `-1`, never a hand-counted worker number.

## [04]-[IMPLEMENTATION_LAW]

[COMPRESSION_ZLIB_NG]:
- import: `from zlib_ng import zlib_ng` / `gzip_ng` / `gzip_ng_threaded` at boundary scope only (`package/codec` declares `lazy import zlib_ng.gzip_ng`); module-level import is banned by the manifest import policy.
- container axis: the `wbits` argument selects the wire format on the `zlib` API surface — `MAX_WBITS` (zlib), `MAX_WBITS | 16` (gzip container), `-MAX_WBITS` (raw DEFLATE); `gzip_ng` is the gzip-container drop-in that presets the gzip framing, and the `package/codec#CODEC` `GZIP` band binds it. A container is a `wbits`/module choice, never a parallel codec owner.
- modality axis: one-shot (`compress`/`decompress`), streaming (`compressobj`/`decompressobj` with a shared `zdict`), file-like (`gzip_ng.open`/`GzipNGFile`), and the multi-threaded block-fan (`gzip_ng_threaded.open`) are rows on one package, never separate owners. The threaded writer fans `_ParallelCompress.compress_and_crc(data, zdict)` — one raw-DEFLATE block plus its CRC-32 per GIL-releasing call — across worker threads, then recombines the per-block CRCs with `crc32_combine(crc1, crc2, len2)` into the single gzip trailer; `package/codec`'s `GZIP` band drives this writer through `_gzip_member`'s threaded lane for parallel gzip throughput — the recombination stays the writer's internal seam, never a codec-side re-derivation or a per-block re-checksum of the whole stream.
- checksum axis: `crc32(data, value=)` and `adler32(data, value=)` are resumable (feed the prior value); `crc32_combine` is the associativity lever that makes block-parallel gzip framing correct — combine the ordered per-block `(crc, len)` pairs rather than re-scanning the concatenated payload. The gzip trailer is CRC-32 + ISIZE; the zlib trailer is Adler-32.
- reproducibility axis: gzip framing carries an mtime — set `mtime=0` (`gzip_ng.compress(..., mtime=0)` / `GzipNGFile(..., mtime=0)`) for a content-addressed reproducible bundle so the same payload hashes identically across runs; never let the wall-clock mtime leak into the artifact content key.
- integration: at the boundary, `package/codec` selects the `GZIP` band by the `CompressionAlgo` discriminant (`package/bundle#BUNDLE` union), encodes the canonical payload, and drives `gzip_ng`/`gzip_ng_threaded` with the `CodecProfile.gzip` `(compresslevel, threads, block_size)` knobs read as named fields, never positional ints. Because the zlib-ng native core RELEASES the GIL (unlike lz4), the codec body rides the runtime thread-offload seam `lanes.offload(kernel, retry=RetryClass.OCCT)` — a bounded worker crossing, never an event-loop call and never a folder-minted `CapacityLimiter`; the result lifts onto the `expression.Result[bytes, ArtifactError]`/`RuntimeRail` rail so a `zlib_ng.error` becomes an `Error` case at the seam. Stamp a `structlog` event / `opentelemetry` span carrying container (zlib/gzip/raw), compression level, thread count, block size, `ZLIBNG_RUNTIME_VERSION`, and input/output byte lengths.
- evidence: each codec call captures container kind, compression level, thread/block-fan parameters, the `crc32`/`adler32` trailer value, native `ZLIBNG_RUNTIME_VERSION`, and input/output byte lengths as a compression fact contributed through the runtime `ReceiptContributor` port onto the single `ArtifactReceipt` family — never a parallel zlib-ng-only receipt shape.
- boundary: zlib-ng owns the DEFLATE/zlib/gzip path (`package/codec#CODEC` `GZIP` band; `package/bundle#BUNDLE` `CompressionAlgo` gzip arm). High-ratio archival routes to `zstandard`; hot-path block codecs to `lz4`; web/WOFF2 transport to `brotli`; archive containers to `py7zr`/`stream-zip`. `scene/export`'s former hand-reach repro-ZIP clone is DELETED (inversion e) — the package plane is the sole compression consumer; a scene bundle is `package/archive`'s emit over scene-file content keys, never an import. Recovery is bomb-bounded; identity minting the runtime owns; live UI stays outside this package.

[STACKING]:
- `package/codec#CODEC`'s `GZIP` band binds `gzip_ng`/`gzip_ng_threaded`: the single-thread path is `gzip_ng.compress(payload, compresslevel=k.compresslevel, mtime=0)`, the parallel path drives `gzip_ng_threaded.open(sink, 'wb', compresslevel=k.compresslevel, threads=k.threads, block_size=k.block_size)`, whose writer recombines the block trailer via `zlib_ng.crc32_combine` internally — one `CodecProfile.gzip` row on the `package/bundle#BUNDLE` `CompressionAlgo` union.
- the codec body rides `lanes.offload(retry=RetryClass.OCCT)` (the GIL-releasing native core makes the thread-offload variant correct — no subprocess lane, no folder-minted limiter), and a `zlib_ng.error` folds to the `expression.Result`/`RuntimeRail` fault at the boundary; the compression fact contributes through `ReceiptContributor` onto the one `ArtifactReceipt.Bundle` case beside the `zstandard`/`lz4`/`brotli` arms.
- `mtime=0` reproducible gzip framing makes a `package/archive` reproducible-ZIP member content-addressable, so the same scene-file or document payload hashes identically for the runtime elision seed (`ArtifactWork.key` over input bytes), never a wall-clock-perturbed re-render.

[RAIL_LAW]:
- Package: `zlib-ng`
- Owns: SIMD-accelerated DEFLATE/zlib/gzip compression and decompression (one-shot, streaming, file-like, and multi-threaded block-fan), Adler-32/CRC-32 checksums with `crc32_combine` block-trailer recombination, and the stdlib-`zlib`/`gzip` drop-in surface
- Accept: the gzip-container/DEFLATE codec service feeding the `package/codec#CODEC` `GZIP` band and the `package/bundle#BUNDLE` `CompressionAlgo` gzip arm via a `CodecProfile.gzip` row, running on the `lanes.offload(retry=RetryClass.OCCT)` thread seam, lifted onto the `expression.Result` rail
- Reject: wrapper-renames of `compress`/`decompress`; a parallel codec owner where the container is a `wbits`/module choice or a `CodecProfile.gzip` field is a row; a per-block whole-stream re-checksum where `crc32_combine` composes the ordered `(crc, len)` pairs; a wall-clock mtime leaking into a content-addressed bundle; a folder-minted `CapacityLimiter`/`stamina` caller where the GIL-releasing core rides `lanes.offload`; the reinstated `scene/export` repro-ZIP clone where the package plane owns bundling; a parallel zlib-ng-only receipt shape where `ArtifactReceipt.Bundle` already owns the case; identity minting the runtime owns
