# [PY_ARTIFACTS_API_LZ4]

`lz4` supplies the LZ4 compression surface for the artifacts compression rail across two submodules: `lz4.frame` for self-describing framed streams and `lz4.block` for raw block codecs. The package owner composes the frame one-shot/streaming surface and the block one-shot surface into the compression owner; it never re-implements the LZ4 codec the native core already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lz4`
- package: `lz4`
- import: `lz4.frame` / `lz4.block`
- owner: `artifacts`
- rail: compression
- installed: `4.4.5` reflected via `python -c "import lz4"` on cp315
- entry points: none (library only)
- capability: LZ4 frame (self-describing, streaming, file-like) and block (raw, fastest) compression and decompression

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: frame codec types
- rail: compression

| [INDEX] | [SYMBOL]                         | [PACKAGE_ROLE]     | [CAPABILITY]                                    |
| :-----: | :------------------------------- | :----------------- | :---------------------------------------------- |
|  [01]   | `lz4.frame.LZ4FrameCompressor`   | frame compressor   | incremental begin/chunk/flush frame compression |
|  [02]   | `lz4.frame.LZ4FrameDecompressor` | frame decompressor | incremental frame decompression                 |
|  [03]   | `lz4.frame.LZ4FrameFile`         | file codec         | file-like framed read/write endpoint            |
|  [04]   | `lz4.block.LZ4BlockError`        | block fault        | raw block codec failure                         |

[PUBLIC_TYPE_SCOPE]: frame constants
- rail: compression

| [INDEX] | [SYMBOL]             | [PACKAGE_ROLE]  | [CAPABILITY]                  |
| :-----: | :------------------- | :-------------- | :---------------------------- |
|  [01]   | `BLOCKSIZE_*`        | block-size axis | frame max-block-size selector |
|  [02]   | `COMPRESSIONLEVEL_*` | level axis      | compression-level bounds      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: frame one-shot, streaming, and file
- rail: compression

Frame rows share compression level, block size, checksum, linked-block, stored-size, bytearray, and file-mode policy.

| [INDEX] | [SURFACE]                              | [CALL_SHAPE]              | [CAPABILITY]                 |
| :-----: | :------------------------------------- | :------------------------ | :--------------------------- |
|  [01]   | `lz4.frame.compress`                   | data plus frame policy    | one-shot framed compress     |
|  [02]   | `lz4.frame.decompress`                 | data plus output policy   | one-shot framed decompress   |
|  [03]   | `lz4.frame.compress_begin`             | context plus frame policy | start a streaming frame      |
|  [04]   | `lz4.frame.compress_chunk`             | context plus data chunk   | feed a streaming chunk       |
|  [05]   | `lz4.frame.compress_flush`             | context plus end flag     | finalize the frame           |
|  [06]   | `lz4.frame.create_compression_context` | no-arg context factory    | allocate a streaming context |
|  [07]   | `lz4.frame.open`                       | filename plus mode policy | file-like framed endpoint    |
|  [08]   | `lz4.frame.get_frame_info`             | frame bytes               | parse a frame header         |

[ENTRYPOINT_SCOPE]: raw block codec
- rail: compression

| [INDEX] | [SURFACE]              | [CALL_SHAPE]             | [CAPABILITY]         |
| :-----: | :--------------------- | :----------------------- | :------------------- |
|  [01]   | `lz4.block.compress`   | source plus block policy | raw block compress   |
|  [02]   | `lz4.block.decompress` | source plus size policy  | raw block decompress |

## [04]-[IMPLEMENTATION_LAW]

[COMPRESSION_LZ4]:
- import: `import lz4.frame` / `import lz4.block` at boundary scope only; module-level import is banned by the manifest import policy.
- frame vs block axis: `lz4.frame` is the default (self-describing, interoperable, streaming-capable); `lz4.block` is the raw speed path when the size is known out-of-band — a row choice, never parallel codec owners.
- modality axis: one-shot (`compress`/`decompress`), streaming (`compress_begin`/`compress_chunk`/`compress_flush`), and file-like (`open`) are rows on the frame submodule, never separate types.
- evidence: each codec call captures frame vs block, compression level, block size, and input/output byte lengths as a compression receipt.
- boundary: lz4 owns the fast-codec path; archive containers route to `py7zr`; high-ratio archival routes to `zstandard`; live UI stays outside this package.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `lz4`
- Owns: LZ4 frame (one-shot/streaming/file) and block (raw) compression and decompression
- Accept: low-latency codec service feeding the compression owner
- Reject: wrapper-renames of `compress`/`decompress`; a parallel codec owner where the frame/block submodule choice is a row; identity minting the runtime owns
