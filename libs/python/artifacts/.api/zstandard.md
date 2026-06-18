# [PY_ARTIFACTS_API_ZSTANDARD]

`zstandard` supplies the Zstandard compression surface for the artifacts compression rail: a compressor root, a decompressor root, one-shot functions, a trained dictionary, and a parameter object that drive level-tuned compression, streaming readers/writers, multi-buffer batch ops, and dictionary-trained codecs against the native zstd library. The package owner composes `ZstdCompressor`, `ZstdDecompressor`, and `ZstdCompressionDict` into the compression owner; it never re-implements the zstd codec the native core already owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `zstandard`
- package: `zstandard`
- import: `zstandard`
- owner: `artifacts`
- rail: compression
- installed: `0.25.0` reflected via `python -c "import zstandard"` on cp315
- entry points: none (library only)
- capability: Zstandard one-shot and streaming compression/decompression, trained dictionaries, multi-buffer batch ops, frame inspection, tunable compression parameters

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: codec roots and dictionary
- rail: compression

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE]     | [CAPABILITY]                    |
| :-----: | :-------------------------- | :----------------- | :------------------------------ |
|   [1]   | `ZstdCompressor`            | compressor root    | configured compression root     |
|   [2]   | `ZstdDecompressor`          | decompressor root  | configured decompression root   |
|   [3]   | `ZstdCompressionDict`       | trained dictionary | shared small-payload dictionary |
|   [4]   | `ZstdCompressionParameters` | parameter object   | advanced compression tuning     |
|   [5]   | `ZstdCompressionReader`     | stream reader      | file-like compression source    |
|   [6]   | `ZstdCompressionWriter`     | stream writer      | file-like compression sink      |
|   [7]   | `ZstdDecompressionReader`   | stream reader      | file-like decompression source  |
|   [8]   | `ZstdDecompressionWriter`   | stream writer      | file-like decompression sink    |
|   [9]   | `FrameParameters`           | frame header       | parsed frame header view        |
|  [10]   | `BufferWithSegments`        | batch buffer       | segmented buffer for batch ops  |

[PUBLIC_TYPE_SCOPE]: faults and key constants
- rail: compression

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE] | [CAPABILITY]           |
| :-----: | :---------------------- | :------------- | :--------------------- |
|   [1]   | `ZstdError`             | codec fault    | zstd call failure      |
|   [2]   | `MAX_COMPRESSION_LEVEL` | level cap      | maximum compression    |
|   [3]   | `FLUSH_BLOCK`           | flush mode     | block flush            |
|   [4]   | `FLUSH_FRAME`           | flush mode     | frame flush            |
|   [5]   | `DICT_TYPE_AUTO`        | dict mode      | automatic dictionary   |
|   [6]   | `DICT_TYPE_FULLDICT`    | dict mode      | full dictionary        |
|   [7]   | `DICT_TYPE_RAWCONTENT`  | dict mode      | raw-content dictionary |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: one-shot and streaming codec
- rail: compression

Constructor rows carry level, dictionary, parameter, format, and thread policy; stream rows carry source/sink sizing and close behavior.

| [INDEX] | [SURFACE]                                 | [CALL_SHAPE]                   | [CAPABILITY]                                |
| :-----: | :---------------------------------------- | :----------------------------- | :------------------------------------------ |
|   [1]   | `compress`                                | buffer plus level              | module-level one-shot compress              |
|   [2]   | `decompress`                              | buffer plus output cap         | module-level one-shot decompress            |
|   [3]   | `ZstdCompressor`                          | compression policy             | configured compressor                       |
|   [4]   | `ZstdCompressor.compress`                 | buffer input                   | one-shot compress with the configured codec |
|   [5]   | `ZstdCompressor.stream_writer`            | writer sink plus sizing        | streaming compress sink                     |
|   [6]   | `ZstdCompressor.stream_reader`            | source stream plus sizing      | streaming compress source                   |
|   [7]   | `ZstdCompressor.compressobj`              | incremental size policy        | incremental compress object                 |
|   [8]   | `ZstdCompressor.multi_compress_to_buffer` | buffer batch plus threads      | batch compress                              |
|   [9]   | `ZstdDecompressor`                        | decompression policy           | configured decompressor                     |
|  [10]   | `ZstdDecompressor.decompress`             | buffer plus frame policy       | one-shot decompress                         |
|  [11]   | `ZstdDecompressor.stream_reader`          | source stream plus read policy | streaming decompress source                 |
|  [12]   | `train_dictionary`                        | dictionary size plus samples   | train a dictionary from samples             |

## [4]-[IMPLEMENTATION_LAW]

[COMPRESSION_ZSTD]:
- import: `import zstandard` at boundary scope only; module-level import is banned by the manifest import policy.
- codec axis: `ZstdCompressor`/`ZstdDecompressor` are the two roots; level, dictionary, params, and thread count are constructor rows, never a parallel compressor per profile.
- modality axis: one-shot (`compress`/`decompress`), streaming (`stream_reader`/`stream_writer`), incremental (`compressobj`), and batch (`multi_compress_to_buffer`) are rows on the same root, never parallel codec types.
- dictionary axis: `ZstdCompressionDict` via `train_dictionary` is the small-payload optimization, configured on the root, never a separate dict-codec.
- evidence: each codec call captures level, dictionary id, frame size, input/output byte lengths, and thread count as a compression receipt.
- boundary: zstandard owns the zstd codec; archive containers route to `py7zr`; web transport codecs route to `brotli`; live UI stays outside this package.

[RAIL_LAW]:
- Package: `zstandard`
- Owns: Zstandard one-shot/streaming/incremental/batch compression and decompression, trained dictionaries, frame inspection, tunable parameters
- Accept: level-tuned and dictionary-trained codec service feeding the compression owner
- Reject: wrapper-renames of `compress`/`decompress`; a parallel compressor per profile where a constructor row suffices; identity minting the runtime owns
