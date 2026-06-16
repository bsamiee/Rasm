# [PY_ARTIFACTS_API_LZ4]

`lz4` supplies the LZ4 compression surface for the artifacts compression rail across two submodules: `lz4.frame` for self-describing framed streams and `lz4.block` for raw block codecs. The package owner composes the frame one-shot/streaming surface and the block one-shot surface into the compression owner; it never re-implements the LZ4 codec the native core already owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lz4`
- package: `lz4`
- import: `lz4.frame` / `lz4.block`
- owner: `artifacts`
- rail: compression
- installed: `4.4.5` reflected via `python -c "import lz4"` on cp315
- entry points: none (library only)
- capability: LZ4 frame (self-describing, streaming, file-like) and block (raw, fastest) compression and decompression

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: frame codec types
- rail: compression

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| :-----: | :------- | :------------- | :----------- |
| [1] | `lz4.frame.LZ4FrameCompressor` | frame compressor | incremental begin/chunk/flush frame compression |
| [2] | `lz4.frame.LZ4FrameDecompressor` | frame decompressor | incremental frame decompression |
| [3] | `lz4.frame.LZ4FrameFile` | file codec | file-like framed read/write endpoint |
| [4] | `lz4.block.LZ4BlockError` | block fault | raw block codec failure |

[PUBLIC_TYPE_SCOPE]: frame constants
- rail: compression

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| :-----: | :------- | :------------- | :----------- |
| [1] | `BLOCKSIZE_DEFAULT` / `BLOCKSIZE_MAX64KB` / `BLOCKSIZE_MAX256KB` / `BLOCKSIZE_MAX1MB` / `BLOCKSIZE_MAX4MB` | block-size axis | frame max-block-size selector |
| [2] | `COMPRESSIONLEVEL_MIN` / `COMPRESSIONLEVEL_MINHC` / `COMPRESSIONLEVEL_MAX` | level axis | compression-level bounds |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: frame one-shot, streaming, and file
- rail: compression

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| :-----: | :-------- | :----------- | :----------- |
| [1] | `lz4.frame.compress` | `compress(data, compression_level=0, block_size=0, content_checksum=0, block_linked=True, store_size=True, return_bytearray=False) -> bytes` | one-shot framed compress |
| [2] | `lz4.frame.decompress` | `decompress(data, return_bytearray=False, return_bytes_read=False) -> bytes` | one-shot framed decompress |
| [3] | `lz4.frame.compress_begin` | `compress_begin(ctx, ...) -> bytes` | start a streaming frame |
| [4] | `lz4.frame.compress_chunk` | `compress_chunk(ctx, data) -> bytes` | feed a streaming chunk |
| [5] | `lz4.frame.compress_flush` | `compress_flush(ctx, end_frame=True) -> bytes` | finalize the frame |
| [6] | `lz4.frame.create_compression_context` | `create_compression_context() -> ctx` | allocate a streaming context |
| [7] | `lz4.frame.open` | `open(filename, mode='rb', compression_level=0, block_size=0, ...) -> LZ4FrameFile` | file-like framed endpoint |
| [8] | `lz4.frame.get_frame_info` | `get_frame_info(frame) -> dict` | parse a frame header |

[ENTRYPOINT_SCOPE]: raw block codec
- rail: compression

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| :-----: | :-------- | :----------- | :----------- |
| [1] | `lz4.block.compress` | `compress(source, mode='default', acceleration=1, compression=0, store_size=True, return_bytearray=False) -> bytes` | raw block compress |
| [2] | `lz4.block.decompress` | `decompress(source, uncompressed_size=-1, return_bytearray=False) -> bytes` | raw block decompress |

## [4]-[IMPLEMENTATION_LAW]

[COMPRESSION_LZ4]:
- import: `import lz4.frame` / `import lz4.block` at boundary scope only; module-level import is banned by the manifest import policy.
- frame vs block axis: `lz4.frame` is the default (self-describing, interoperable, streaming-capable); `lz4.block` is the raw speed path when the size is known out-of-band — a row choice, never parallel codec owners.
- modality axis: one-shot (`compress`/`decompress`), streaming (`compress_begin`/`compress_chunk`/`compress_flush`), and file-like (`open`) are rows on the frame submodule, never separate types.
- evidence: each codec call captures frame vs block, compression level, block size, and input/output byte lengths as a compression receipt.
- boundary: lz4 owns the fast-codec path; archive containers route to `py7zr`; high-ratio archival routes to `zstandard`; live UI stays outside this package.

## [5]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `lz4`
- Owns: LZ4 frame (one-shot/streaming/file) and block (raw) compression and decompression
- Accept: low-latency codec service feeding the compression owner
- Reject: wrapper-renames of `compress`/`decompress`; a parallel codec owner where the frame/block submodule choice is a row; identity minting the runtime owns
