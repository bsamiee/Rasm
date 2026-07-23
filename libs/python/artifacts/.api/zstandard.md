# [PY_ARTIFACTS_API_ZSTANDARD]

`zstandard` owns the Zstandard codec on the artifacts compression rail: the `ZstdCompressor`/`ZstdDecompressor` roots, `ZstdCompressionDict` COVER dictionaries, `ZstdCompressionParameters` advanced tuning, a `FrameParameters` header view, and the zero-copy batch carriers over native libzstd. It is the archival default: the `package/codec#CODEC` `ZSTD` arm routes high-ratio single-blob and COVER-dictionary small-payload classes here, every other class routing to its own codec on the shared `CompressionAlgo` discriminant.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `zstandard`
- package: `zstandard` (`BSD-3-Clause`, Gregory Szorc; bundled native libzstd, BSD/GPLv2-dual, Meta)
- module: `zstandard`
- rail: compression
- abi: native `cext` wheel (`backend_c`, GIL-releasing, one wheel per interpreter minor) is the live build; the `cffi` fallback (`backend_cffi`, selected via `PYTHON_ZSTANDARD_IMPORT_POLICY`) drops the zero-copy batch carriers and `multi_*_to_buffer`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: codec roots, dictionary, and parameters

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]      | [CAPABILITY]                                                    |
| :-----: | :-------------------------- | :----------------- | :-------------------------------------------------------------- |
|  [01]   | `ZstdCompressor`            | compressor root    | compression root; all modalities are methods; `memory_size()`   |
|  [02]   | `ZstdDecompressor`          | decompressor root  | decompression root; all modalities are methods; `memory_size()` |
|  [03]   | `ZstdCompressionDict`       | trained dictionary | COVER dict + `precompute_compress` compression-side caching     |
|  [04]   | `ZstdCompressionParameters` | parameter object   | advanced tuning; `from_level` + per-field overrides             |
|  [05]   | `FrameParameters`           | frame header view  | `content_size`/`window_size`/`dict_id`/`has_checksum` fields    |

- [PARAM_FIELD]: `window_log` `hash_log` `chain_log` `search_log` `min_match` `target_length` `strategy` `compression_level` `enable_ldm` `ldm_hash_log` `ldm_min_match` `ldm_bucket_size_log` `ldm_hash_rate_log` `overlap_log` `force_max_window` `threads` `job_size` `write_checksum` `write_content_size` `write_dict_id` `format` `estimated_compression_context_size()`

[PUBLIC_TYPE_SCOPE]: streaming and incremental carriers

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]      | [CAPABILITY]                                                                  |
| :-----: | :------------------------ | :----------------- | :---------------------------------------------------------------------------- |
|  [01]   | `ZstdCompressionReader`   | stream reader      | `BinaryIO` compress source; `read`/`read1`/`readinto`/`readinto1`/`readall`   |
|  [02]   | `ZstdCompressionWriter`   | stream writer      | `BinaryIO` compress sink; `write`/`flush(flush_mode=)`/`tell`/`memory_size()` |
|  [03]   | `ZstdDecompressionReader` | stream reader      | seekable `BinaryIO` decompress source; `seek(pos, whence)` + `read` family    |
|  [04]   | `ZstdDecompressionWriter` | stream writer      | `BinaryIO` decompress sink; `write`/`memory_size()`                           |
|  [05]   | `ZstdCompressionObj`      | incremental object | zlib-style `compress`/`flush(flush_mode=)` from `compressobj`                 |
|  [06]   | `ZstdDecompressionObj`    | incremental object | zlib-style `decompress`/`flush`; `unused_data`/`unconsumed_tail`/`eof` props  |
|  [07]   | `ZstdCompressionChunker`  | fixed-size chunker | uniform-size chunks: `compress`/`flush()`/`finish()` from `chunker`           |

[PUBLIC_TYPE_SCOPE]: zero-copy batch buffers and fault

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                                                                        |
| :-----: | :----------------------------- | :------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `BufferWithSegments`           | batch buffer  | one allocation framing many payloads; `size`/`segments()`/`tobytes()`/`__getitem__` |
|  [02]   | `BufferWithSegmentsCollection` | batch result  | collection from `multi_*_to_buffer`; `size()`/`__len__`/`__getitem__` per frame     |
|  [03]   | `BufferSegments`               | segment view  | indexable view of one buffer's segment offsets; `__getitem__`                       |
|  [04]   | `BufferSegment`                | segment       | `offset` + `tobytes()` + `__len__`; one payload slice in a batch buffer             |
|  [05]   | `ZstdError`                    | fault         | every native zstd failure; `Exception`, lifted to `expression.Result`               |

[PUBLIC_TYPE_SCOPE]: key constants — flush/dict/format/strategy selectors and the parameter floors/ceilings a `ZstdCompressionParameters` consumer validates against, all module `int`s
- [LEVEL_CAP]: `MAX_COMPRESSION_LEVEL`
- [FLUSH_MODE]: `FLUSH_BLOCK` `FLUSH_FRAME` for `ZstdCompressionWriter.flush(flush_mode=)` · `COMPRESSOBJ_FLUSH_BLOCK` `COMPRESSOBJ_FLUSH_FINISH` for `ZstdCompressionObj.flush(flush_mode=)`
- [DICT_MODE]: `DICT_TYPE_AUTO` `DICT_TYPE_FULLDICT` `DICT_TYPE_RAWCONTENT` for `ZstdCompressionDict(dict_type=)`
- [FRAME_FORMAT]: `FORMAT_ZSTD1` `FORMAT_ZSTD1_MAGICLESS` for `format=`; magicless omits the 4-byte magic
- [STRATEGY]: `STRATEGY_FAST` `STRATEGY_DFAST` `STRATEGY_GREEDY` `STRATEGY_LAZY` `STRATEGY_LAZY2` `STRATEGY_BTLAZY2` `STRATEGY_BTOPT` `STRATEGY_BTULTRA` `STRATEGY_BTULTRA2` (fast to densest) for `strategy=`
- [PARAM_BOUND]: `WINDOWLOG_MIN`/`MAX` `CHAINLOG_MIN`/`MAX` `HASHLOG_MIN`/`MAX` `SEARCHLOG_MIN`/`MAX` `MINMATCH_MIN`/`MAX` `SEARCHLENGTH_MIN`/`MAX` `TARGETLENGTH_MIN`/`MAX` `LDM_MINMATCH_MIN`/`MAX` `LDM_BUCKETSIZELOG_MAX` `BLOCKSIZELOG_MAX` `BLOCKSIZE_MAX`
- [SIZE_HINT]: `COMPRESSION_RECOMMENDED_INPUT_SIZE` `COMPRESSION_RECOMMENDED_OUTPUT_SIZE` `DECOMPRESSION_RECOMMENDED_INPUT_SIZE` `DECOMPRESSION_RECOMMENDED_OUTPUT_SIZE` for `read_size`/`write_size`
- [SENTINEL]: `CONTENTSIZE_UNKNOWN` `CONTENTSIZE_ERROR` — raw u64 C spellings the binding never returns; the guard reads `-1`/raises
- [IDENTITY]: `ZSTD_VERSION` `__version__` `MAGIC_NUMBER` `FRAME_HEADER` `backend` `backend_features`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: module one-shots and file open — top-level convenience over default-configured transient roots

| [INDEX] | [SURFACE]                                                 | [SHAPE] | [CAPABILITY]                                 |
| :-----: | :-------------------------------------------------------- | :------ | :------------------------------------------- |
|  [01]   | `compress(data, level=3) -> bytes`                        | static  | one-shot compress, transient compressor      |
|  [02]   | `decompress(data, max_output_size=0)`                     | static  | one-shot decompress; cap unknown-size output |
|  [03]   | `open(filename, mode, cctx, dctx, …)`                     | static  | file handle wrapping a path/fileobj          |
|  [04]   | `train_dictionary(dict_size, samples, *, k, d, level, …)` | static  | COVER-train from a sample corpus             |
|  [05]   | `get_frame_parameters(data, format=None)`                 | static  | parse a frame header without decompress      |
|  [06]   | `frame_content_size(data) -> int`                         | static  | size from header (sentinel-guarded)          |
|  [07]   | `frame_header_size(data) -> int`                          | static  | byte length of the frame header              |
|  [08]   | `estimate_decompression_context_size() -> int`            | static  | decompression context memory estimate        |

[ENTRYPOINT_SCOPE]: `ZstdCompressor` modalities — `ZstdCompressor(level, dict_data, compression_params, write_checksum, write_content_size, write_dict_id, threads)`; `threads=-1` binds one job per logical core

| [INDEX] | [SURFACE]                                     | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :-------------------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `compress(data) -> bytes`                     | instance | one-shot with the configured codec           |
|  [02]   | `stream_reader(source, …)`                    | factory  | read compressed bytes from a source          |
|  [03]   | `stream_writer(writer, …)`                    | factory  | write source bytes compressed to a sink      |
|  [04]   | `compressobj(size=-1)`                        | factory  | incremental feed/flush object                |
|  [05]   | `chunker(size=-1, chunk_size=)`               | factory  | emit uniform-size output chunks              |
|  [06]   | `copy_stream(ifh, ofh, …) -> tuple[int, int]` | instance | pump file->file; returns (read, written)     |
|  [07]   | `read_to_iter(reader, …) -> Generator`        | instance | lazily iterate compressed chunks             |
|  [08]   | `multi_compress_to_buffer(data, threads=0)`   | instance | threaded batch compress; shared-dict corpus  |
|  [09]   | `frame_progression() -> tuple[int, int, int]` | instance | (ingested, consumed, produced) for the frame |
|  [10]   | `memory_size() -> int`                        | instance | compression context memory footprint         |

[ENTRYPOINT_SCOPE]: `ZstdDecompressor` modalities — `ZstdDecompressor(dict_data, max_window_size, format)`; `max_window_size` bounds an adversarial frame's window before allocation

| [INDEX] | [SURFACE]                                                                 | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :------------------------------------------------------------------------ | :------- | :-------------------------------------------- |
|  [01]   | `decompress(data, max_output_size, read_across_frames, allow_extra_data)` | instance | one-shot; multi-frame + trailing-data kwargs  |
|  [02]   | `stream_reader(source, …)`                                                | factory  | seekable decompress source                    |
|  [03]   | `stream_writer(writer, …)`                                                | factory  | decompress source bytes into a sink           |
|  [04]   | `decompressobj(write_size=, read_across_frames=False)`                    | factory  | incremental object; `unused_data` frame walk  |
|  [05]   | `read_to_iter(reader, …) -> Generator`                                    | instance | lazily iterate decompressed chunks            |
|  [06]   | `copy_stream(ifh, ofh, read_size=, write_size=)`                          | instance | pump compressed file->plain file              |
|  [07]   | `multi_decompress_to_buffer(frames, …)`                                   | instance | threaded batch decompress; skip header probes |
|  [08]   | `decompress_content_dict_chain(frames)`                                   | instance | decode a content-dictionary frame chain       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- codec: `ZstdCompressor`/`ZstdDecompressor` are the two roots, with `level`/`dict_data`/`compression_params`/`write_checksum`/`write_content_size`/`write_dict_id`/`threads` on the compressor and `dict_data`/`max_window_size`/`format` on the decompressor as constructor rows, never a per-profile subclass; the upstream `ZstdKnobs` carries every axis as a field.
- modality: one-shot, file-like streaming, incremental zlib-style, fixed-size chunking, file pump, lazy iteration, and threaded batch are methods on one root selected by input shape — a two-or-more `*payloads` spread routes to `multi_compress_to_buffer` against a shared dictionary, a singular payload to `compress`, a streaming sink to `stream_writer`.
- dictionary: `ZstdCompressionDict(data, dict_type=)` from `train_dictionary` (COVER; `k`/`d`/`steps`/`accel` tune) is the small-payload win, passed `dict_data=` on either root; `precompute_compress` caches compression-side state for a hot repeated-compress loop; a worker-subprocess lane crosses the dict as `as_bytes()` and rehydrates at codec scope, never the unpicklable native handle.
- parameter: `ZstdCompressionParameters.from_level(level, *, source_size, dict_size, **overrides)` derives a full set and overrides any field, passed `compression_params=` and never beside `level=` on one `ZstdCompressor` (mutually exclusive). A zero-valued log/length override replaces the level-derived value and resolves as context default against `compression_level=3`, compressing worse than the level alone, so a knob whose `0` means "level-derived" is withheld, never forwarded.
- frame: `get_frame_parameters`/`frame_content_size`/`frame_header_size` inspect a header without decode; `frame_content_size` returns `-1` for an undeclared size and raises `ZstdError` on a malformed header, so the guard compares `-1`/the caller ceiling, never the `CONTENTSIZE_*` constants; `FORMAT_ZSTD1_MAGICLESS` strips the 4-byte magic and must match on both roots; `decompressobj(read_across_frames=False)` with `unused_data` walks concatenated self-delimiting frames.
- abi: `backend` is `'cext'` or `'cffi'`; the batch-buffer carriers and `multi_*_to_buffer` bind only when `backend_features` advertises them under `cext`, while the streaming/one-shot/incremental/chunked rails are present under both.
- bomb: every recovery is bounded by the declared-size guard (`frame_content_size` against the owner ceiling), `decompress(max_output_size=)`, and the decompressor `max_window_size`, never a post-hoc size check on unbounded output.
- receipt: each call folds one `BundleEvidence` receipt onto the single `core/receipt#RECEIPT` `ArtifactReceipt.Bundle` case, carrying `level`, `dict_id`, the declared-size aggregate (summing `frame_content_size` over declaring frames only, a `-1` landing in a separate `unknown_frames` count), byte lengths, `entries` arity, the `write_checksum`-verified frame count, `threads`, `format`, and the checksum flag; `ratio` feeds the runtime `rasm.artifact.compression_ratio` instrument.

[STACKING]:
- `msgspec`(`.api/msgspec.md`): the boundary encodes the canonical payload via `msgspec.msgpack.encode`, then a `level`/`threads`-tuned `ZstdCompressor` compresses the buffer.
- `anyio`(`.api/anyio.md`): the GIL-releasing codec body runs off the event loop through `anyio.to_thread.run_sync` under a shared `CapacityLimiter`, never inline on the loop.
- `expression`(`.api/expression.md`): the `stamina`-retried producing call lifts onto `expression.Result[bytes, ArtifactError]`, a `ZstdError` becoming an `Error` case at the seam rather than a raised exception crossing the owner.
- `structlog`(`.api/structlog.md`) / `opentelemetry`(`.api/opentelemetry-api.md`): each call stamps an event and span carrying `level`, `dict_id`, `frame_content_size`, the `threads`/`format`/`write_checksum` flags, byte lengths, and the `frame_progression()` produced count.
- artifacts compression owner: composes the two roots and contributes the receipt through the runtime `ReceiptContributor` port; a shared-structure `*payloads` spread trains one dictionary and drives `multi_compress_to_buffer`, while a streaming sink wraps the downstream writer in `stream_writer` to keep back-pressure in the codec.

[LOCAL_ADMISSION]:
- `import zstandard` at boundary scope only; module-level import is banned by the manifest import policy.
- carriers bind through the minting method, never import: `compressobj`/`decompressobj`/`chunker` return `backend_c` classes with no `zstandard.*` name.
- probe `backend_features` before the batch path; the `cffi` fallback omits the carriers and `multi_*_to_buffer`.
- live UI stays outside this package.

[RAIL_LAW]:
- Package: `zstandard`
- Owns: Zstandard one-shot/streaming/incremental/chunked/threaded-batch compression and decompression, COVER-trained dictionaries with precompute caching, advanced compression parameters, frame inspection, and magicless/checksum/content-size/dict-id frame policy
- Accept: a level-tuned, dictionary-trained, optionally multi-threaded codec service feeding the `package/codec#CODEC` `ZSTD` arm, selecting modality by input shape on one of the two roots, offloaded off the event loop under a bounded limiter, lifted onto the `expression.Result` rail with a `BundleEvidence` receipt on the single `ArtifactReceipt.Bundle` case
- Reject: wrapper-renames of `compress`/`decompress`; a per-profile compressor where a constructor row or `ZstdCompressionParameters` suffices; `level=` and `compression_params=` together on one `ZstdCompressor`; a zero-valued `from_level` override where withholding keeps the level-derived cparam; `dict_type` passed to `train_dictionary` (it is a `ZstdCompressionDict` ctor axis); in-RAM accumulation where `stream_writer`/`compressobj` streams; binding a carrier by import; an unbounded recovery past the declared-size and `max_window_size`/`max_output_size` bounds; reaching the batch path without a `backend_features` probe; a `ZstdError` crossing the owner as a raised exception; duplicating the data-rail `numcodecs` `Zstd`/`Blosc` buffer codec here; identity-minting the runtime owns
