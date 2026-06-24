# [PY_ARTIFACTS_API_ZSTANDARD]

`zstandard` supplies the Zstandard compression surface for the artifacts compression rail: a compressor root, a decompressor root, module one-shots, a trained dictionary, an advanced parameter object, and four buffer carriers that drive level-tuned compression, file-like streaming readers/writers, incremental compress/decompress objects, multi-buffer threaded batch ops, frame inspection, and dictionary-trained codecs against the native zstd library. The package owner composes `ZstdCompressor`, `ZstdDecompressor`, and `ZstdCompressionDict` into the compression owner; it never re-implements the zstd codec the native core already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `zstandard`
- package: `zstandard`
- import: `zstandard`
- owner: `artifacts`
- rail: compression
- installed: `0.25.0` reflected via assay api on cp315 (`cp315-cp315-macosx_14_0_arm64` native wheel)
- license: `BSD-3-Clause`
- abi: native `_cffi`/`backend_c` extension; `backend` module attr reports `"cext"` vs `"cffi"` and `backend_features` enumerates the active build's optional capability set — the cp315 native wheel reports `{'buffer_types', 'multi_compress_to_buffer', 'multi_decompress_to_buffer'}`; these are absent under the pure-cffi fallback
- marker floor: `cffi~=1.17` for `python_version<'3.14'`, `cffi>=2.0.0b` for `>=3.14`, only under the `cffi` extra; the C-extension build needs no cffi
- entry points: none (library only)
- capability: Zstandard one-shot, streaming, incremental, and threaded-batch compression/decompression; trained dictionaries; frame inspection; advanced compression-parameter tuning; magicless and content-size/checksum frame-header policy

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: codec roots, dictionary, and parameters
- rail: compression

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE]      | [CAPABILITY]                                                                       |
| :-----: | :-------------------------- | :------------------ | :--------------------------------------------------------------------------------- |
|  [01]   | `ZstdCompressor`            | compressor root     | level/dict/params/threads-configured compression root; all modalities are methods  |
|  [02]   | `ZstdDecompressor`          | decompressor root   | dict/window/format-configured decompression root; all modalities are methods       |
|  [03]   | `ZstdCompressionDict`       | trained dictionary  | shared small-payload dictionary; `dict_id`/`as_bytes`/`precompute_compress(level=, compression_params=)`; `k`/`d` carry the trained COVER params |
|  [04]   | `ZstdCompressionParameters` | parameter object    | full advanced tuning; `from_level(level, source_size=, dict_size=, **overrides)` derives a param set; readable `window_log`/`hash_log`/`chain_log`/`search_log`/`min_match`/`target_length`/`strategy`/`enable_ldm`/`ldm_*`/`overlap_log`/`threads`/`job_size`/`write_checksum`/`write_content_size`/`write_dict_id`; `estimated_compression_context_size()` |
|  [05]   | `FrameParameters`           | frame header view   | `content_size`/`window_size`/`dict_id`/`has_checksum` parsed from a frame header    |

[PUBLIC_TYPE_SCOPE]: streaming and incremental carriers
- rail: compression — minted by root methods, never constructed directly. These are the typed return shapes the `__init__.pyi` stub declares; under the `cext` build `ZstdCompression*Reader`/`Writer` are real module attributes, while `compressobj()`/`decompressobj()`/`chunker()` return `backend_c` runtime classes (`chunker()` -> `ZstdCompressionChunkerType`) that are NOT re-exported as `zstandard.*` names — bind them by the method that mints them, never by import

| [INDEX] | [SYMBOL]                      | [PACKAGE_ROLE]       | [CAPABILITY]                                                                   |
| :-----: | :---------------------------- | :------------------- | :----------------------------------------------------------------------------- |
|  [01]   | `ZstdCompressionReader`       | stream reader        | `BinaryIO` compress source from `stream_reader`; `read`/`read1`/`readinto`      |
|  [02]   | `ZstdCompressionWriter`       | stream writer        | `BinaryIO` compress sink from `stream_writer`; `write`/`flush(flush_mode)`      |
|  [03]   | `ZstdDecompressionReader`     | stream reader        | `BinaryIO` decompress source from `stream_reader`; seekable, `readinto`         |
|  [04]   | `ZstdDecompressionWriter`     | stream writer        | `BinaryIO` decompress sink from `stream_writer`                                 |
|  [05]   | `ZstdCompressionObj`          | incremental object   | zlib-style `compress(data)` + `flush(flush_mode)` from `compressobj`            |
|  [06]   | `ZstdDecompressionObj`        | incremental object   | zlib-style `decompress(data)` + `flush`; `unused_data`/`unconsumed_tail`/`eof`  |
|  [07]   | `ZstdCompressionChunker`      | fixed-size chunker   | `compress`/`flush`/`finish` emitting uniform output chunks from `chunker`       |

[PUBLIC_TYPE_SCOPE]: zero-copy batch buffers and faults
- rail: compression — buffer carriers are present only when `backend_features` advertises `'buffer_types'` (the cext capability flag; the cffi fallback omits it along with `multi_*_to_buffer`)

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]   | [CAPABILITY]                                                              |
| :-----: | :-------------------------------- | :--------------- | :----------------------------------------------------------------------- |
|  [01]   | `BufferWithSegments`              | batch buffer     | `__init__(data, segments)`; one allocation framing many payloads         |
|  [02]   | `BufferWithSegmentsCollection`    | batch result     | collection of `BufferWithSegments`; returned by `multi_*_to_buffer`      |
|  [03]   | `BufferSegments`                  | segment view     | indexable view of one buffer's segment offsets                           |
|  [04]   | `BufferSegment`                   | segment          | `offset` + `tobytes()`; one payload slice inside a batch buffer          |
|  [05]   | `ZstdError`                       | codec fault      | every native zstd call failure                                           |

[PUBLIC_TYPE_SCOPE]: key constants
- rail: compression

| [INDEX] | [SYMBOL]                                          | [PACKAGE_ROLE]   | [CAPABILITY]                                                       |
| :-----: | :------------------------------------------------ | :--------------- | :---------------------------------------------------------------- |
|  [01]   | `MAX_COMPRESSION_LEVEL`                           | level cap        | maximum supported compression level                               |
|  [02]   | `FLUSH_BLOCK` / `FLUSH_FRAME`                     | flush mode       | `flush(flush_mode=)` selectors for writers/compress objects       |
|  [03]   | `COMPRESSOBJ_FLUSH_BLOCK` / `COMPRESSOBJ_FLUSH_FINISH` | flush mode  | `ZstdCompressionObj.flush` selectors                              |
|  [04]   | `DICT_TYPE_AUTO` / `DICT_TYPE_FULLDICT` / `DICT_TYPE_RAWCONTENT` | dict mode | `ZstdCompressionDict(dict_type=)` selectors                |
|  [05]   | `FORMAT_ZSTD1` / `FORMAT_ZSTD1_MAGICLESS`         | frame format     | `format=` on params/decompressor; magicless omits the 4-byte magic |
|  [06]   | `STRATEGY_FAST`..`STRATEGY_BTULTRA2`              | match strategy   | `ZstdCompressionParameters(strategy=)` selectors                  |
|  [07]   | `CONTENTSIZE_UNKNOWN` / `CONTENTSIZE_ERROR`       | sentinel         | `frame_content_size` return sentinels                             |
|  [08]   | `COMPRESSION_RECOMMENDED_INPUT_SIZE` / `..._OUTPUT_SIZE` | size hint  | recommended stream read/write chunk sizes                         |
|  [09]   | `ZSTD_VERSION` / `__version__` / `MAGIC_NUMBER`   | identity         | native zstd version tuple, binding version, frame magic           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: module one-shots and file open
- rail: compression — top-level convenience over default-configured roots

| [INDEX] | [SURFACE]              | [CALL_SHAPE]                                                                                             | [CAPABILITY]                                              |
| :-----: | :--------------------- | :------------------------------------------------------------------------------------------------------ | :------------------------------------------------------- |
|  [01]   | `compress`             | `compress(data, level=3) -> bytes`                                                                      | one-shot compress with a transient compressor            |
|  [02]   | `decompress`           | `decompress(data, max_output_size=0) -> bytes`                                                          | one-shot decompress; cap output when frame size unknown  |
|  [03]   | `open`                 | `open(filename, mode='rb', cctx=None, dctx=None, encoding=None, errors=None, newline=None, closefd=True)` | file-like handle wrapping a path/fileobj through a codec  |
|  [04]   | `train_dictionary`     | `train_dictionary(dict_size, samples, k=, d=, f=, split_point=, accel=, dict_id=, level=, steps=, threads=, notifications=) -> ZstdCompressionDict` | COVER-train a dictionary from a sample corpus |
|  [05]   | `get_frame_parameters` | `get_frame_parameters(data, format=None) -> FrameParameters`                                            | parse a frame header without decompressing               |
|  [06]   | `frame_content_size`   | `frame_content_size(data) -> int`                                                                       | decompressed size from header (or `CONTENTSIZE_*`)        |
|  [07]   | `frame_header_size`    | `frame_header_size(data) -> int`                                                                        | byte length of the frame header                           |
|  [08]   | `estimate_decompression_context_size` | `estimate_decompression_context_size() -> int`                                           | decompression context memory estimate                    |

[ENTRYPOINT_SCOPE]: `ZstdCompressor` modalities
- rail: compression — `ZstdCompressor(level=3, dict_data=None, compression_params=None, write_checksum=None, write_content_size=None, write_dict_id=None, threads=0)`

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                                                                       | [CAPABILITY]                                            |
| :-----: | :------------------------- | :------------------------------------------------------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | `compress`                 | `compress(data) -> bytes`                                                                          | one-shot with the configured codec                      |
|  [02]   | `stream_reader`            | `stream_reader(source, size=-1, read_size=, *, closefd=True) -> ZstdCompressionReader`             | read compressed bytes pulled from a source              |
|  [03]   | `stream_writer`            | `stream_writer(writer, size=-1, write_size=, write_return_read=True, *, closefd=True) -> ZstdCompressionWriter` | write source bytes compressed into a sink   |
|  [04]   | `compressobj`              | `compressobj(size=-1) -> ZstdCompressionObj`                                                       | incremental feed/flush object                           |
|  [05]   | `chunker`                  | `chunker(size=-1, chunk_size=) -> ZstdCompressionChunker`                                          | emit uniform-size output chunks                         |
|  [06]   | `copy_stream`              | `copy_stream(ifh, ofh, size=-1, read_size=, write_size=) -> tuple[int, int]`                       | pump file->file; returns (read, written)                |
|  [07]   | `read_to_iter`             | `read_to_iter(reader, size=-1, read_size=, write_size=) -> Generator[bytes]`                       | lazily iterate compressed chunks                        |
|  [08]   | `multi_compress_to_buffer` | `multi_compress_to_buffer(data, threads=0) -> BufferWithSegmentsCollection`                        | threaded batch compress of a buffer/list                |
|  [09]   | `frame_progression`        | `frame_progression() -> tuple[int, int, int]`                                                      | (ingested, consumed, produced) for the live frame       |
|  [10]   | `memory_size`              | `memory_size() -> int`                                                                             | compression context memory footprint                    |

[ENTRYPOINT_SCOPE]: `ZstdDecompressor` modalities
- rail: compression — `ZstdDecompressor(dict_data=None, max_window_size=0, format=FORMAT_ZSTD1)`

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                                                                       | [CAPABILITY]                                                |
| :-----: | :------------------------------ | :------------------------------------------------------------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `decompress`                    | `decompress(data, max_output_size=0, read_across_frames=True, allow_extra_data=True) -> bytes`     | one-shot; multi-frame and trailing-data policy as kwargs    |
|  [02]   | `stream_reader`                 | `stream_reader(source, read_size=, read_across_frames=False, *, closefd=True) -> ZstdDecompressionReader` | seekable decompress source                            |
|  [03]   | `stream_writer`                 | `stream_writer(writer, write_size=, write_return_read=True, *, closefd=True) -> ZstdDecompressionWriter` | decompress source bytes into a sink                   |
|  [04]   | `decompressobj`                 | `decompressobj(write_size=, read_across_frames=False) -> ZstdDecompressionObj`                     | incremental feed/flush object with `eof`                    |
|  [05]   | `read_to_iter`                  | `read_to_iter(reader, read_size=, write_size=, skip_bytes=0) -> Generator[bytes]`                  | lazily iterate decompressed chunks                          |
|  [06]   | `copy_stream`                   | `copy_stream(ifh, ofh, read_size=, write_size=) -> tuple[int, int]`                                | pump compressed file->plain file                            |
|  [07]   | `multi_decompress_to_buffer`    | `multi_decompress_to_buffer(frames, decompressed_sizes=, threads=0) -> BufferWithSegmentsCollection` | threaded batch decompress                                 |
|  [08]   | `decompress_content_dict_chain` | `decompress_content_dict_chain(frames: list) -> bytes`                                             | decode a content-dictionary frame chain                     |

## [04]-[IMPLEMENTATION_LAW]

[COMPRESSION_ZSTD]:
- import: `import zstandard` at boundary scope only; module-level import is banned by the manifest import policy.
- codec axis: `ZstdCompressor`/`ZstdDecompressor` are the two roots. `level`, `dict_data`, `compression_params`, `write_checksum`/`write_content_size`/`write_dict_id`, and `threads` are constructor rows on the compressor; `dict_data`, `max_window_size`, and `format` are constructor rows on the decompressor. Never a parallel compressor type per profile.
- modality axis: one-shot (`compress`/`decompress`), file-like streaming (`stream_reader`/`stream_writer` minting `BinaryIO` carriers), incremental zlib-style (`compressobj`/`decompressobj`), fixed-size chunking (`chunker`), file pump (`copy_stream`), lazy iteration (`read_to_iter`), and threaded batch (`multi_compress_to_buffer`/`multi_decompress_to_buffer`) are all methods on the same root, never parallel codec types. Pick the modality by the input shape, not by minting a new object.
- dictionary axis: `ZstdCompressionDict` via `train_dictionary` (COVER algorithm; `k`/`d`/`steps`/`accel` tune training) is the small-payload optimization, passed as `dict_data=` on either root. `DICT_TYPE_*` selects auto/fulldict/rawcontent interpretation; `precompute_compress` caches the compression-side dictionary state. Never a separate dict-codec type.
- parameter axis: `ZstdCompressionParameters` (or `.from_level(level, source_size=, dict_size=)`) carries window/hash/chain/search logs, `strategy` (`STRATEGY_*`), long-distance-matching (`enable_ldm`, `ldm_*`), and `threads`; `estimated_compression_context_size()` sizes the context before allocation. Pass as `compression_params=` instead of scattering raw ints.
- frame axis: `get_frame_parameters`/`frame_content_size`/`frame_header_size` inspect a frame header without decoding; `FORMAT_ZSTD1_MAGICLESS` strips the 4-byte magic for embedded frames and must match on both roots. `read_across_frames`/`allow_extra_data` on decompress govern multi-frame and trailing-byte tolerance.
- batch axis: `BufferWithSegments(data, segments)` frames many payloads in one allocation; `multi_*_to_buffer(..., threads=N)` runs zero-copy threaded batch codec returning a `BufferWithSegmentsCollection` — gate on `'multi_compress_to_buffer' in zstandard.backend_features` for the method and `'buffer_types' in zstandard.backend_features` for the `BufferWithSegments` carrier; the cffi fallback omits both flags (the cp315 native wheel advertises all three).
- abi axis: `zstandard.backend` is `"cext"` (native) or `"cffi"` (pure-Python fallback); `backend_features` is the capability set. Treat batch-buffer ops as conditional on the C extension; the streaming/one-shot/incremental rails are present under both backends.
- integration: feed a `stamina`-retried boundary call producing the input bytes, compress the canonical `msgspec.msgpack.encode(...)` payload with a level/`threads`-tuned `ZstdCompressor`, and stamp an `otel` span with `level`, `dict_id`, `frame_content_size`, and `frame_progression()` produced-byte count. For many small records sharing structure, `train_dictionary` once and thread the `ZstdCompressionDict` through every per-record `compress`; for a streaming sink, wrap the downstream writer in `stream_writer(...)` so back-pressure stays in the codec, not in an in-RAM buffer.
- evidence: each codec call captures level, `dict_id`, `frame_content_size`, input/output byte lengths, `threads`, `format`, and checksum flag as a compression receipt.
- boundary: zstandard owns the zstd codec; archive containers route to `py7zr`; web transport codecs route to `brotli`; live UI stays outside this package.

[RAIL_LAW]:
- Package: `zstandard`
- Owns: Zstandard one-shot/streaming/incremental/chunked/batch compression and decompression, COVER-trained dictionaries, advanced compression parameters, frame inspection, and magicless/checksum frame policy
- Accept: a level-tuned, dictionary-trained, optionally multi-threaded codec service feeding the compression owner, selecting modality by input shape on one of the two roots
- Reject: wrapper-renames of `compress`/`decompress`; a parallel compressor per profile where a constructor row or `ZstdCompressionParameters` suffices; an in-RAM accumulation where `stream_writer`/`compressobj` already streams; identity-minting the runtime owns; assuming batch-buffer ops exist without checking `backend_features`
