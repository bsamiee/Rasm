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

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| :-----: | :------- | :------------- | :----------- |
| [1] | `ZstdCompressor` | compressor root | level/dict/params-configured compressor with one-shot and streaming methods |
| [2] | `ZstdDecompressor` | decompressor root | dict-configured decompressor with one-shot and streaming methods |
| [3] | `ZstdCompressionDict` | trained dictionary | a shared dictionary for small-payload compression |
| [4] | `ZstdCompressionParameters` | parameter object | advanced window/hash/chain/strategy tuning |
| [5] | `ZstdCompressionReader` / `ZstdCompressionWriter` | stream codec | file-like streaming compression endpoints |
| [6] | `ZstdDecompressionReader` / `ZstdDecompressionWriter` | stream codec | file-like streaming decompression endpoints |
| [7] | `FrameParameters` | frame header | parsed frame header view |
| [8] | `BufferWithSegments` | batch buffer | a single buffer partitioned into segments for batch ops |

[PUBLIC_TYPE_SCOPE]: faults and key constants
- rail: compression

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| :-----: | :------- | :------------- | :----------- |
| [1] | `ZstdError` | codec fault | a zstd call failed |
| [2] | `MAX_COMPRESSION_LEVEL` | level cap | maximum compression level |
| [3] | `FLUSH_BLOCK` / `FLUSH_FRAME` | flush mode | streaming flush discriminants |
| [4] | `DICT_TYPE_AUTO` / `DICT_TYPE_FULLDICT` / `DICT_TYPE_RAWCONTENT` | dict mode | dictionary interpretation discriminants |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: one-shot and streaming codec
- rail: compression

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| :-----: | :-------- | :----------- | :----------- |
| [1] | `compress` | `compress(data: collections.abc.Buffer, level: int = 3) -> bytes` | module-level one-shot compress |
| [2] | `decompress` | `decompress(data: collections.abc.Buffer, max_output_size: int = 0) -> bytes` | module-level one-shot decompress |
| [3] | `ZstdCompressor` | `ZstdCompressor(level=3, dict_data=None, compression_params=None, write_checksum=None, write_content_size=None, write_dict_id=None, threads=0)` | configured compressor |
| [4] | `ZstdCompressor.compress` | `compress(data) -> bytes` | one-shot compress with the configured codec |
| [5] | `ZstdCompressor.stream_writer` | `stream_writer(writer, size=-1, write_size=..., write_return_read=True, closefd=True) -> ZstdCompressionWriter` | streaming compress sink |
| [6] | `ZstdCompressor.stream_reader` | `stream_reader(source, size=-1, read_size=..., closefd=False) -> ZstdCompressionReader` | streaming compress source |
| [7] | `ZstdCompressor.compressobj` | `compressobj(size=-1) -> ZstdCompressionObj` | incremental compress object |
| [8] | `ZstdCompressor.multi_compress_to_buffer` | `multi_compress_to_buffer(data, threads=0) -> BufferWithSegmentsCollection` | batch compress |
| [9] | `ZstdDecompressor` | `ZstdDecompressor(dict_data=None, max_window_size=0, format=FORMAT_ZSTD1)` | configured decompressor |
| [10] | `ZstdDecompressor.decompress` | `decompress(data, max_output_size=0, read_across_frames=True, allow_extra_data=True) -> bytes` | one-shot decompress |
| [11] | `ZstdDecompressor.stream_reader` | `stream_reader(source, read_size=..., read_across_frames=False, closefd=False) -> ZstdDecompressionReader` | streaming decompress source |
| [12] | `train_dictionary` | `train_dictionary(dict_size, samples, **kwargs) -> ZstdCompressionDict` | train a dictionary from samples |

## [4]-[IMPLEMENTATION_LAW]

[COMPRESSION_ZSTD]:
- import: `import zstandard` at boundary scope only; module-level import is banned by the manifest import policy.
- codec axis: `ZstdCompressor`/`ZstdDecompressor` are the two roots; level, dictionary, params, and thread count are constructor rows, never a parallel compressor per profile.
- modality axis: one-shot (`compress`/`decompress`), streaming (`stream_reader`/`stream_writer`), incremental (`compressobj`), and batch (`multi_compress_to_buffer`) are rows on the same root, never parallel codec types.
- dictionary axis: `ZstdCompressionDict` via `train_dictionary` is the small-payload optimization, configured on the root, never a separate dict-codec.
- evidence: each codec call captures level, dictionary id, frame size, input/output byte lengths, and thread count as a compression receipt.
- boundary: zstandard owns the zstd codec; archive containers route to `py7zr`; web transport codecs route to `brotli`; live UI stays outside this package.

## [5]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `zstandard`
- Owns: Zstandard one-shot/streaming/incremental/batch compression and decompression, trained dictionaries, frame inspection, tunable parameters
- Accept: level-tuned and dictionary-trained codec service feeding the compression owner
- Reject: wrapper-renames of `compress`/`decompress`; a parallel compressor per profile where a constructor row suffices; identity minting the runtime owns
